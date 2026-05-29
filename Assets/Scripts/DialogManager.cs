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
        BuildUIIfNeeded();

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

        float slotH = 1f / line.choices.Length;
        for (int i = 0; i < line.choices.Length; i++)
        {
            var choice = line.choices[i];
            float yMax = 1f - i * slotH;
            float yMin = yMax - slotH + 0.015f;

            GameObject btnObj;
            if (choiceButtonPrefab != null)
            {
                btnObj = Instantiate(choiceButtonPrefab, choicePanel.transform);
            }
            else
            {
                // Auto-build tombol tanpa prefab
                btnObj = new GameObject("Choice_" + i);
                btnObj.transform.SetParent(choicePanel.transform, false);
                var bRT = btnObj.AddComponent<RectTransform>();
                bRT.anchorMin = new Vector2(0f, yMin);
                bRT.anchorMax = new Vector2(1f, yMax);
                bRT.offsetMin = new Vector2(0f,  4f);
                bRT.offsetMax = new Vector2(0f, -4f);
                btnObj.AddComponent<Image>();

                var lblGO = new GameObject("Label");
                lblGO.transform.SetParent(btnObj.transform, false);
                var lblRT = lblGO.AddComponent<RectTransform>();
                lblRT.anchorMin = Vector2.zero;
                lblRT.anchorMax = Vector2.one;
                lblRT.offsetMin = new Vector2(14f,  4f);
                lblRT.offsetMax = new Vector2(-14f, -4f);
                var lbl = lblGO.AddComponent<TextMeshProUGUI>();
                lbl.fontSize           = 24;
                lbl.color              = Color.white;
                lbl.fontStyle          = FontStyles.Bold;
                lbl.alignment          = TextAlignmentOptions.MidlineLeft;
                lbl.enableWordWrapping = true;
                lbl.raycastTarget      = false;
                btnObj.AddComponent<Button>();
            }

            // Teks & warna
            var tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = choice.label;

            var img = btnObj.GetComponent<Image>();
            if (img != null) img.color = CategoryColor(choice.category);

            var c = choice;
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

    // ══════════════════════════════════════════════════════════════════════
    // AUTO-BUILD UI — jika field Inspector tidak di-assign, bangun sendiri
    // ══════════════════════════════════════════════════════════════════════

    void BuildUIIfNeeded()
    {
        if (dialogPanel != null) return;   // sudah ter-assign, tidak perlu build

        // ── Canvas ─────────────────────────────────────────────────────────
        var canvasGO = new GameObject("DialogManagerCanvas");
        DontDestroyOnLoad(canvasGO);
        var cv = canvasGO.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 990;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Panel utama ────────────────────────────────────────────────────
        var panelGO = new GameObject("DialogPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRT  = panelGO.AddComponent<RectTransform>();
        // Posisi bawah layar (sama dengan NpcDialog default)
        panelRT.anchorMin = new Vector2(0.03f, 0.01f);
        panelRT.anchorMax = new Vector2(0.97f, 0.33f);
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;
        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.82f);
        var outline = panelGO.AddComponent<Outline>();
        outline.effectColor    = new Color(1f, 0.85f, 0.3f, 1f);
        outline.effectDistance = new Vector2(3f, -3f);
        dialogPanel = panelGO;

        // ── Foto pembicara ─────────────────────────────────────────────────
        var portGO = new GameObject("Portrait");
        portGO.transform.SetParent(panelGO.transform, false);
        var portRT  = portGO.AddComponent<RectTransform>();
        portRT.anchorMin = new Vector2(0.01f, 0.05f);
        portRT.anchorMax = new Vector2(0.18f, 0.95f);
        portRT.offsetMin = new Vector2(8f,  8f);
        portRT.offsetMax = new Vector2(-8f, -8f);
        portraitImage = portGO.AddComponent<Image>();
        portraitImage.preserveAspect = true;
        portraitImage.color          = new Color(0.15f, 0.15f, 0.15f, 0.6f);
        portraitImage.raycastTarget  = false;

        // ── Banner nama ────────────────────────────────────────────────────
        var bannerGO = new GameObject("Banner");
        bannerGO.transform.SetParent(panelGO.transform, false);
        var bannerRT  = bannerGO.AddComponent<RectTransform>();
        bannerRT.anchorMin = new Vector2(0.20f, 0.65f);
        bannerRT.anchorMax = new Vector2(0.55f, 0.92f);
        bannerRT.offsetMin = Vector2.zero;
        bannerRT.offsetMax = Vector2.zero;
        var bannerImg = bannerGO.AddComponent<Image>();
        bannerImg.color         = new Color(0.14f, 0.09f, 0.01f, 0.92f);
        bannerImg.raycastTarget = false;

        var speakerGO = new GameObject("SpeakerText");
        speakerGO.transform.SetParent(bannerGO.transform, false);
        var speakerRT  = speakerGO.AddComponent<RectTransform>();
        speakerRT.anchorMin = Vector2.zero;
        speakerRT.anchorMax = Vector2.one;
        speakerRT.offsetMin = new Vector2(10f, 2f);
        speakerRT.offsetMax = new Vector2(-10f, -2f);
        speakerText = speakerGO.AddComponent<TextMeshProUGUI>();
        speakerText.fontSize        = 30;
        speakerText.fontStyle       = FontStyles.Bold;
        speakerText.color           = new Color(1f, 0.85f, 0.3f, 1f);
        speakerText.alignment       = TextAlignmentOptions.MidlineLeft;
        speakerText.raycastTarget   = false;

        // ── Teks dialog ────────────────────────────────────────────────────
        var bodyGO = new GameObject("DialogText");
        bodyGO.transform.SetParent(panelGO.transform, false);
        var bodyRT  = bodyGO.AddComponent<RectTransform>();
        bodyRT.anchorMin = new Vector2(0.20f, 0.06f);
        bodyRT.anchorMax = new Vector2(0.96f, 0.63f);
        bodyRT.offsetMin = new Vector2(8f,  4f);
        bodyRT.offsetMax = new Vector2(-8f, -4f);
        dialogText = bodyGO.AddComponent<TextMeshProUGUI>();
        dialogText.fontSize           = 26;
        dialogText.color              = Color.white;
        dialogText.alignment          = TextAlignmentOptions.TopLeft;
        dialogText.enableWordWrapping = true;
        dialogText.raycastTarget      = false;

        // ── Tombol Lanjutkan ───────────────────────────────────────────────
        var contGO = new GameObject("ContinueButton");
        contGO.transform.SetParent(panelGO.transform, false);
        var contRT  = contGO.AddComponent<RectTransform>();
        contRT.anchorMin = new Vector2(0.72f, 0.04f);
        contRT.anchorMax = new Vector2(0.97f, 0.30f);
        contRT.offsetMin = Vector2.zero;
        contRT.offsetMax = Vector2.zero;
        var contImg = contGO.AddComponent<Image>();
        contImg.color = new Color(0.2f, 0.6f, 0.86f, 0.85f);
        continueButton = contGO.AddComponent<Button>();
        continueButton.onClick.AddListener(OnContinueClicked);

        var contLblGO = new GameObject("Label");
        contLblGO.transform.SetParent(contGO.transform, false);
        var contLblRT  = contLblGO.AddComponent<RectTransform>();
        contLblRT.anchorMin = Vector2.zero;
        contLblRT.anchorMax = Vector2.one;
        contLblRT.offsetMin = Vector2.zero;
        contLblRT.offsetMax = Vector2.zero;
        var contTMP = contLblGO.AddComponent<TextMeshProUGUI>();
        contTMP.text          = "▼ Lanjut";
        contTMP.fontSize      = 22;
        contTMP.color         = Color.white;
        contTMP.fontStyle     = FontStyles.Bold;
        contTMP.alignment     = TextAlignmentOptions.Center;
        contTMP.raycastTarget = false;

        // ── Panel pilihan ──────────────────────────────────────────────────
        var cpGO = new GameObject("ChoicePanel");
        cpGO.transform.SetParent(canvasGO.transform, false);
        var cpRT  = cpGO.AddComponent<RectTransform>();
        cpRT.anchorMin = new Vector2(0.03f, 0.34f);
        cpRT.anchorMax = new Vector2(0.65f, 0.88f);
        cpRT.offsetMin = Vector2.zero;
        cpRT.offsetMax = Vector2.zero;
        cpGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f); // transparan
        choicePanel = cpGO;
        choicePanel.SetActive(false);

        // choiceButtonPrefab tidak diperlukan — tombol dibuat langsung di ShowChoicesIfAny()
        choiceButtonPrefab = null;

        dialogPanel.SetActive(false);

        Debug.Log("[DialogManager] UI dibangun otomatis (auto-build). " +
                  "Untuk kustomisasi tampilan, assign field di Inspector.");
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
