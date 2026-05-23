using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Mengelola panel dialog (NPC berbicara + pilihan AMAN/RAGU/BAHAYA).
/// Letakkan komponen ini pada Canvas di tiap scene.
///
/// Setup di Inspector:
///   dialogPanel     → Panel utama dialog (aktifkan/nonaktifkan)
///   speakerText     → TMP: nama pembicara (mis. "Rara", "Si Bayangan")
///   dialogText      → TMP: isi percakapan (efek ketik)
///   portraitImage   → Image: foto/ikon pembicara
///   choicePanel     → Panel tempat tombol pilihan dimunculkan
///   choiceButtonPrefab → Prefab Button dengan TMP child
///   continueButton  → Tombol "Lanjutkan ▶" (klik untuk maju)
///   portraits       → Array sprite: [0]=rara [1]=boss [2]=polisi [3]=npc [4]=narasi
/// </summary>
public class DialogManager : MonoBehaviour
{
    [Header("Panel Utama")]
    public GameObject       dialogPanel;
    public TextMeshProUGUI  speakerText;
    public TextMeshProUGUI  dialogText;
    public Image            portraitImage;
    public Button           continueButton;

    [Header("Panel Pilihan")]
    public GameObject choicePanel;
    public GameObject choiceButtonPrefab;

    [Header("Potret Karakter (urutan: rara, boss, polisi, npc, narasi)")]
    public Sprite[] portraits;

    [Header("Pengaturan Efek Ketik")]
    [Range(0.01f, 0.1f)]
    public float typeSpeed = 0.025f;

    // ── Internal State ─────────────────────────────────────────────────────
    private List<DialogLine> currentLines;
    private int              lineIndex;
    private Action           onComplete;
    private bool             isTyping;
    private Coroutine        typingCoroutine;

    // ── Warna Pilihan ──────────────────────────────────────────────────────
    private static readonly Color COLOR_AMAN   = new Color(0.15f, 0.68f, 0.38f);
    private static readonly Color COLOR_RAGU   = new Color(0.95f, 0.61f, 0.07f);
    private static readonly Color COLOR_BAHAYA = new Color(0.91f, 0.30f, 0.24f);
    private static readonly Color COLOR_NETRAL = new Color(0.20f, 0.60f, 0.86f);

    // ══════════════════════════════════════════════════════════════════════
    // DATA STRUCTURES
    // ══════════════════════════════════════════════════════════════════════

    [System.Serializable]
    public class DialogLine
    {
        public string   speaker;
        public string   portrait;   // "rara" | "boss" | "polisi" | "npc" | "narasi"
        [TextArea(2, 6)]
        public string   text;
        public Choice[] choices;    // null → tombol Lanjutkan biasa
    }

    [System.Serializable]
    public class Choice
    {
        public string label;
        public string category;     // "AMAN" | "RAGU" | "BAHAYA"
        public float  damage;       // untuk boss fight: pengurangan mental boss
        public bool   isPanic;      // memicu Panic Button
        [NonSerialized]
        public Action onSelect;     // callback saat dipilih (set via kode)
    }

    // ══════════════════════════════════════════════════════════════════════
    // PUBLIC API
    // ══════════════════════════════════════════════════════════════════════

    /// Tampilkan urutan dialog. onComplete dipanggil setelah baris terakhir.
    public void Show(List<DialogLine> lines, Action onComplete = null)
    {
        this.currentLines = lines;
        this.lineIndex    = 0;
        this.onComplete   = onComplete;

        dialogPanel.SetActive(true);
        continueButton.gameObject.SetActive(true);
        ShowLine(0);
    }

    /// Dipanggil tombol Lanjutkan atau klik panel.
    public void OnContinueClicked()
    {
        if (isTyping)
        {
            // Skip animasi ketik — tampilkan teks langsung
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            isTyping         = false;
            dialogText.text  = currentLines[lineIndex].text;
            ShowChoicesIfAny(currentLines[lineIndex]);
            return;
        }

        // Jika ada pilihan aktif, tunda sampai pilihan dipilih
        if (choicePanel.activeSelf) return;

        lineIndex++;
        ShowLine(lineIndex);
    }

    // ══════════════════════════════════════════════════════════════════════
    // INTERNAL
    // ══════════════════════════════════════════════════════════════════════

    void ShowLine(int index)
    {
        if (index >= currentLines.Count)
        {
            EndDialog();
            return;
        }

        var line = currentLines[index];
        speakerText.text = line.speaker;
        SetPortrait(line.portrait);
        choicePanel.SetActive(false);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(line.text, () =>
        {
            ShowChoicesIfAny(line);
        }));
    }

    IEnumerator TypeText(string fullText, Action onDone)
    {
        isTyping        = true;
        dialogText.text = "";
        foreach (char c in fullText)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
        isTyping = false;
        onDone?.Invoke();
    }

    void ShowChoicesIfAny(DialogLine line)
    {
        if (line.choices == null || line.choices.Length == 0)
        {
            continueButton.gameObject.SetActive(true);
            return;
        }

        continueButton.gameObject.SetActive(false);
        choicePanel.SetActive(true);

        // Bersihkan tombol lama
        foreach (Transform child in choicePanel.transform)
            Destroy(child.gameObject);

        foreach (var choice in line.choices)
        {
            var btnObj = Instantiate(choiceButtonPrefab, choicePanel.transform);
            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = choice.label;

            // Warna tombol sesuai kategori
            var img = btnObj.GetComponent<Image>();
            if (img != null)
                img.color = CategoryColor(choice.category);

            var c = choice; // closure capture
            btnObj.GetComponent<Button>().onClick.AddListener(() => OnChoiceSelected(c));
        }
    }

    void OnChoiceSelected(Choice c)
    {
        // Jalankan callback khusus (mis. kurangi boss mental)
        c.onSelect?.Invoke();

        // Skor + nyawa via GameState
        if (GameState.Instance != null)
        {
            GameState.Instance.AddChoice(GameState.Instance.day, c.label, c.category);
            if (c.category == "BAHAYA")
            {
                bool alive = GameState.Instance.LoseLife();
                if (!alive)
                {
                    // Informasikan ke controller bahwa pemain mati
                    onComplete?.Invoke();
                    dialogPanel.SetActive(false);
                    return;
                }
            }
        }

        choicePanel.SetActive(false);
        lineIndex++;
        ShowLine(lineIndex);
    }

    void EndDialog()
    {
        dialogPanel.SetActive(false);
        choicePanel.SetActive(false);
        onComplete?.Invoke();
    }

    // ── Portrait Mapping ───────────────────────────────────────────────────
    void SetPortrait(string key)
    {
        if (portraitImage == null || portraits == null) return;

        int idx = key?.ToLower() switch
        {
            "rara"   => 0,
            "boss"   => 1,
            "polisi" => 2,
            "npc"    => 3,
            "narasi" => 4,
            _        => -1
        };

        if (idx >= 0 && idx < portraits.Length && portraits[idx] != null)
        {
            portraitImage.gameObject.SetActive(true);
            portraitImage.sprite = portraits[idx];
        }
        else
        {
            portraitImage.gameObject.SetActive(false);
        }
    }

    static Color CategoryColor(string category) => category switch
    {
        "AMAN"   => COLOR_AMAN,
        "RAGU"   => COLOR_RAGU,
        "BAHAYA" => COLOR_BAHAYA,
        _        => COLOR_NETRAL
    };
}
