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
    [Header("Tata Letak Bersama (opsional)")]
    [Tooltip("Aset DialogBoxLayout. Jika di-assign, panel/portrait/banner/teks/hint\n" +
             "akan dibangun dengan anchor & sprite dari aset ini — sama dengan Day1Intro & NpcDialog.")]
    public DialogBoxLayout layout;

    [Header("Banner Nama Pembicara")]
    [Tooltip("Sembunyikan latar/banner nama pembicara (mis. saat PEMOTOR). Teks nama tetap tampil.")]
    public bool sembunyikanLatarNama = false;

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

    // Pool tombol pilihan — dipakai ulang antar baris (hindari Destroy/Instantiate
    // berulang yang memicu GC spike di mobile).
    private readonly List<GameObject> _choicePool = new List<GameObject>();

    // Pilihan baris yang sedang tampil (untuk fitur pilih ulang).
    private Choice[] _currentChoices;
    // True jika pemain sudah pernah memilih BAHAYA (salah) di baris pilihan ini.
    // Dipakai untuk: (1) salah pertama gratis, salah berikutnya kurangi nyawa;
    // (2) batasi skor pilihan benar setelah sempat salah ke maksimal RAGU.
    private bool _pernahSalahDiBaris;

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
        [TextArea(1, 3)]
        public string penjelasan = ""; // alasan edukatif (opsional); kosong = generik per kategori
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
        // SFX klik untuk setiap ketukan lanjut (skip ketik / maju baris).
        AudioManager.Instance?.Click();

        if (isTyping)
        {
            // Skip animasi ketik — tampilkan teks langsung (seluruh karakter terlihat)
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            isTyping         = false;
            dialogText.text  = currentLines[lineIndex].text;
            dialogText.maxVisibleCharacters = int.MaxValue;
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
        isTyping = true;

        // Set teks penuh SEKALI lalu ungkap bertahap via maxVisibleCharacters.
        // Keuntungan: rich-text (<b>, <color>) tidak pernah tampil mentah,
        // tanpa alokasi string per huruf (bebas GC), dan tidak terpengaruh
        // Time.timeScale karena memakai WaitForSecondsRealtime.
        dialogText.text = fullText;
        dialogText.ForceMeshUpdate();
        int total = dialogText.textInfo.characterCount;
        dialogText.maxVisibleCharacters = 0;

        int shown = 0;
        while (shown < total)
        {
            shown++;
            dialogText.maxVisibleCharacters = shown;
            yield return new WaitForSecondsRealtime(typeSpeed);
        }

        dialogText.maxVisibleCharacters = int.MaxValue;
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

        // VerticalLayoutGroup menjaga tinggi tombol minimum (target sentuh mobile).
        // Tinggi tiap tombol dijamin >= MIN_CHOICE_HEIGHT via LayoutElement.
        var vlg = choicePanel.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) vlg = choicePanel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing               = 12f;
        vlg.padding               = new RectOffset(0, 0, 0, 0);
        vlg.childAlignment        = TextAnchor.LowerCenter;
        vlg.childControlWidth     = true;
        vlg.childControlHeight    = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = true;

        const float MIN_CHOICE_HEIGHT = 96f; // px (ref 1080) — target sentuh ramah anak/mobile

        // Baris pilihan baru: reset status "pernah salah" & simpan daftar pilihan
        // agar tombol salah bisa dinonaktifkan saat pemain diberi kesempatan ulang.
        _currentChoices = line.choices;
        _pernahSalahDiBaris = false;

        for (int i = 0; i < line.choices.Length; i++)
        {
            var choice = line.choices[i];

            // Ambil tombol dari pool (atau bangun bila kurang) — bukan recreate tiap kali.
            GameObject btnObj = GetOrCreateChoiceButton(i);
            btnObj.SetActive(true);
            btnObj.transform.SetSiblingIndex(i);

            // Jamin tinggi minimum tombol (target sentuh) untuk semua jalur build
            var le = btnObj.GetComponent<LayoutElement>();
            if (le == null) le = btnObj.AddComponent<LayoutElement>();
            le.minHeight       = MIN_CHOICE_HEIGHT;
            le.preferredHeight = MIN_CHOICE_HEIGHT;
            le.flexibleHeight  = 1f;

            // Teks + ikon penanda kategori (aksesibilitas buta warna): ✓ / ! / ✕
            var tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
                tmp.text = UIPalette.KategoriIkon(choice.category) + "  " + choice.label;

            var img = btnObj.GetComponent<Image>();
            if (img != null) img.color = CategoryColor(choice.category);

            // Efek tekan kecil agar tombol terasa responsif di layar sentuh.
            if (btnObj.GetComponent<ButtonPressFeedback>() == null)
                btnObj.AddComponent<ButtonPressFeedback>();

            var c   = choice;
            var btn = btnObj.GetComponent<Button>();
            btn.interactable = true; // pastikan aktif kembali bila tombol dipakai ulang dari pool
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnChoiceSelected(c));
        }

        // Sembunyikan sisa tombol pool yang tidak terpakai baris ini.
        for (int i = line.choices.Length; i < _choicePool.Count; i++)
            if (_choicePool[i] != null) _choicePool[i].SetActive(false);
    }

    // Ambil tombol pilihan dari pool pada indeks tertentu; bangun bila belum ada.
    GameObject GetOrCreateChoiceButton(int index)
    {
        if (index < _choicePool.Count && _choicePool[index] != null)
            return _choicePool[index];

        GameObject btnObj;
        if (choiceButtonPrefab != null)
        {
            btnObj = Instantiate(choiceButtonPrefab, choicePanel.transform);
        }
        else
        {
            // Auto-build tombol tanpa prefab — posisi diatur VerticalLayoutGroup
            btnObj = new GameObject("Choice_" + index);
            btnObj.transform.SetParent(choicePanel.transform, false);
            btnObj.AddComponent<RectTransform>();
            btnObj.AddComponent<Image>();

            var lblGO = new GameObject("Label");
            lblGO.transform.SetParent(btnObj.transform, false);
            var lblRT = lblGO.AddComponent<RectTransform>();
            lblRT.anchorMin = Vector2.zero;
            lblRT.anchorMax = Vector2.one;
            lblRT.offsetMin = new Vector2(18f,  6f);
            lblRT.offsetMax = new Vector2(-18f, -6f);
            var lbl = lblGO.AddComponent<TextMeshProUGUI>();
            lbl.fontSize           = 28;
            lbl.color              = Color.white;
            lbl.fontStyle          = FontStyles.Bold;
            lbl.alignment          = TextAlignmentOptions.MidlineLeft;
            lbl.textWrappingMode   = TextWrappingModes.Normal;
            lbl.enableAutoSizing   = true;
            lbl.fontSizeMin        = 20;
            lbl.fontSizeMax        = 28;
            lbl.raycastTarget      = false;
            btnObj.AddComponent<Button>();
        }

        if (index < _choicePool.Count) _choicePool[index] = btnObj;
        else                           _choicePool.Add(btnObj);
        return btnObj;
    }

    void OnChoiceSelected(Choice c)
    {
        // SFX per kategori untuk pilihan tanpa callback kustom (mis. Hari 1).
        // Pilihan dengan onSelect kustom memainkan SFX-nya sendiri — hindari dobel.
        if (c.onSelect == null)
            AudioManager.Instance?.PlayKategori(c.category);

        // ── Pilihan SALAH (BAHAYA) → beri kesempatan memilih ulang (game edukasi) ──
        // Tujuannya pemain BELAJAR jawaban benar, bukan langsung dihukum.
        if (c.category == "BAHAYA")
        {
            // Selalu tampilkan feedback edukatif "kenapa ini berbahaya".
            TampilkanPenjelasanPilihan(c);

            if (!_pernahSalahDiBaris)
            {
                // Salah PERTAMA = peringatan gratis, nyawa belum berkurang.
                _pernahSalahDiBaris = true;
            }
            else
            {
                // Salah LAGI = baru kurangi nyawa.
                bool alive = GameState.Instance?.LoseLife() ?? true;
                HUDManager.Instance?.FlashHeartLost(GameState.Instance?.lives ?? 0);
                HUDManager.Instance?.ShowLifeLostPopup();
                if (!alive)
                {
                    onComplete?.Invoke();
                    dialogPanel.SetActive(false);
                    return;
                }
            }

            // Nonaktifkan tombol salah ini, panel tetap terbuka agar pemain memilih ulang.
            NonaktifkanTombolPilihan(c);
            return; // JANGAN lanjut ke baris berikutnya
        }

        // ── Pilihan BENAR (AMAN/RAGU) ──
        // Jalankan callback khusus (mis. kurangi boss mental)
        c.onSelect?.Invoke();

        // Skor via GameState. Jika pemain sempat salah di baris ini, skor dibatasi
        // maksimal RAGU (50) sebagai konsekuensi — tetap edukatif tapi tak "gratis".
        if (GameState.Instance != null)
        {
            if (_pernahSalahDiBaris)
            {
                int basePts = c.category == "AMAN" ? GameState.SCORE_AMAN
                            : c.category == "RAGU" ? GameState.SCORE_RAGU
                            :                        GameState.SCORE_BAHAYA;
                int poin = Mathf.Min(basePts, GameState.SCORE_RAGU);
                GameState.Instance.AddChoice(GameState.Instance.day, c.label, c.category, poin);
            }
            else
            {
                GameState.Instance.AddChoice(GameState.Instance.day, c.label, c.category);
            }
        }

        // Feedback edukatif "Kenapa?" — hanya di Hari 2 (game edukasi).
        if (GameState.Instance != null && GameState.Instance.day == 2)
            TampilkanPenjelasanPilihan(c);

        choicePanel.SetActive(false);
        lineIndex++;
        ShowLine(lineIndex);
    }

    // Nonaktifkan tombol pilihan yang salah agar tidak bisa diklik lagi saat
    // pemain diberi kesempatan memilih ulang (visual diredupkan).
    void NonaktifkanTombolPilihan(Choice c)
    {
        if (_currentChoices == null) return;
        int idx = System.Array.IndexOf(_currentChoices, c);
        if (idx < 0 || idx >= _choicePool.Count) return;
        var go = _choicePool[idx];
        if (go == null) return;
        var btn = go.GetComponent<Button>();
        if (btn != null) btn.interactable = false;
        var img = go.GetComponent<Image>();
        if (img != null) { var col = img.color; col.a = 0.4f; img.color = col; }
    }

    // ── Toast edukatif "💡 Kenapa?" setelah memilih (Hari 2) ───────────────
    private GameObject _eduToast;
    void TampilkanPenjelasanPilihan(Choice c)
    {
        if (dialogPanel == null) return;
        Transform host = dialogPanel.transform.parent != null
            ? dialogPanel.transform.parent : dialogPanel.transform;

        string isi = !string.IsNullOrWhiteSpace(c.penjelasan)
            ? c.penjelasan
            : PenjelasanGenerik(c.category);
        if (string.IsNullOrWhiteSpace(isi)) return;

        if (_eduToast != null) Destroy(_eduToast);

        Color aksen = UIPalette.Kategori(c.category);

        var toast = new GameObject("EduToast");
        toast.transform.SetParent(host, false);
        var img = toast.AddComponent<Image>();
        img.sprite = GetEduRounded();
        img.type   = Image.Type.Sliced;
        img.color  = new Color(0.10f, 0.08f, 0.06f, 0.97f);
        img.raycastTarget = false;
        var outl = toast.AddComponent<Outline>();
        outl.effectColor    = aksen;
        outl.effectDistance = new Vector2(2.5f, -2.5f);
        var rt = toast.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f); rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(820f, 120f);
        rt.anchoredPosition = new Vector2(0f, -36f);

        var tmp = new GameObject("Teks").AddComponent<TextMeshProUGUI>();
        tmp.transform.SetParent(toast.transform, false);
        if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
        string judul = c.category == "AMAN" ? "\uD83D\uDCA1 KENAPA AMAN?"
                     : c.category == "RAGU" ? "\uD83D\uDCA1 PERLU LEBIH TEGAS"
                     : c.category == "BAHAYA" ? "\u26A0 KENAPA BERBAHAYA?"
                     : "\uD83D\uDCA1 CATATAN";
        tmp.text = $"<b><color=#FFD24A>{judul}</color></b>\n<size=88%>{isi}</size>";
        tmp.fontSize = 22;
        tmp.color = new Color(1f, 1f, 0.95f, 1f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = true; tmp.fontSizeMin = 15; tmp.fontSizeMax = 23;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        var trt = tmp.rectTransform;
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(24f, 12f); trt.offsetMax = new Vector2(-24f, -12f);

        _eduToast = toast;
        StartCoroutine(EduToastAnim(toast, rt));
    }

    string PenjelasanGenerik(string kategori)
    {
        switch (kategori)
        {
            case "AMAN":
                return "Kamu menjaga jarak dan tetap waspada terhadap orang asing. Itu langkah yang tepat!";
            case "RAGU":
                return "Belum sepenuhnya aman \u2014 masih ada risiko. Lebih baik bersikap tegas: bilang TIDAK lalu menjauh.";
            case "BAHAYA":
                return "Pilihan ini bisa membahayakanmu. Ingat 3 kata sakti: TIDAK \u2192 PERGI \u2192 CERITA pada orang dewasa.";
            default:
                return "";
        }
    }

    IEnumerator EduToastAnim(GameObject toast, RectTransform rt)
    {
        if (toast == null) yield break;
        float t = 0f;
        while (t < 0.2f && toast != null)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / 0.2f);
            float s = p < 0.7f ? Mathf.Lerp(0.85f, 1.05f, p / 0.7f)
                               : Mathf.Lerp(1.05f, 1f, (p - 0.7f) / 0.3f);
            if (rt != null) rt.localScale = Vector3.one * s;
            yield return null;
        }
        if (rt != null) rt.localScale = Vector3.one;
        yield return new WaitForSeconds(3f);
        var img  = toast != null ? toast.GetComponent<Image>() : null;
        var ol   = toast != null ? toast.GetComponent<Outline>() : null;
        var tmps = toast != null ? toast.GetComponentsInChildren<TextMeshProUGUI>() : null;
        float f = 0f;
        while (f < 0.45f && toast != null)
        {
            f += Time.deltaTime;
            float a = 1f - Mathf.Clamp01(f / 0.45f);
            if (img != null) { var col = img.color; col.a = 0.97f * a; img.color = col; }
            if (ol  != null) { var col = ol.effectColor; col.a = a; ol.effectColor = col; }
            if (tmps != null) foreach (var x in tmps) { if (x == null) continue; var col = x.color; col.a = a; x.color = col; }
            yield return null;
        }
        if (toast != null) { if (_eduToast == toast) _eduToast = null; Destroy(toast); }
    }

    static Sprite GetEduRounded()
    {
        // Delegasi ke sprite rounded 9-slice bersama (UIKit) — satu sprite untuk semua UI.
        return UIKit.RoundedSprite();
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
        // Dibuat lewat factory bersama → CanvasScaler responsif (Expand) sejak awal.
        var cv = UIKit.CreateOverlayCanvas("DialogManagerCanvas", 990, dontDestroy: true);
        var canvasGO = cv.gameObject;

        // ── EventSystem — wajib agar Button/onClick merespon ───────────────
        // Kalau scene belum punya EventSystem, tombol Lanjut & pilihan tidak
        // akan menerima klik sama sekali (penyebab umum "button tidak merespon").
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            DontDestroyOnLoad(esGO);
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ── Panel utama ────────────────────────────────────────────────────
        var panelGO = new GameObject("DialogPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRT  = panelGO.AddComponent<RectTransform>();
        if (layout != null)
        {
            panelRT.anchorMin = layout.PanelAnchorMin;
            panelRT.anchorMax = layout.PanelAnchorMax;
        }
        else
        {
            // Posisi bawah layar (default lama)
            panelRT.anchorMin = new Vector2(0.03f, 0.01f);
            panelRT.anchorMax = new Vector2(0.97f, 0.33f);
        }
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;
        var panelImg = panelGO.AddComponent<Image>();
        if (layout != null && layout.boxSprite != null)
        {
            panelImg.sprite = layout.boxSprite;
            panelImg.type   = Image.Type.Simple;
            panelImg.color  = Color.white;
        }
        else
        {
            panelImg.color = new Color(0f, 0f, 0f, 0.82f);
            var outline = panelGO.AddComponent<Outline>();
            outline.effectColor    = new Color(1f, 0.85f, 0.3f, 1f);
            outline.effectDistance = new Vector2(3f, -3f);
        }
        dialogPanel = panelGO;

        // ── Foto pembicara ─────────────────────────────────────────────────
        var portGO = new GameObject("Portrait");
        portGO.transform.SetParent(panelGO.transform, false);
        var portRT  = portGO.AddComponent<RectTransform>();
        if (layout != null)
        {
            portRT.anchorMin = layout.PortraitAnchorMin;
            portRT.anchorMax = layout.PortraitAnchorMax;
            portRT.offsetMin = Vector2.zero;
            portRT.offsetMax = Vector2.zero;
        }
        else
        {
            portRT.anchorMin = new Vector2(0.01f, 0.05f);
            portRT.anchorMax = new Vector2(0.18f, 0.95f);
            portRT.offsetMin = new Vector2(8f,  8f);
            portRT.offsetMax = new Vector2(-8f, -8f);
        }
        portraitImage = portGO.AddComponent<Image>();
        portraitImage.preserveAspect = (layout != null) ? layout.portraitPreserveAspect : true;
        portraitImage.color          = new Color(1f, 1f, 1f, 1f);
        portraitImage.raycastTarget  = false;

        // ── Banner nama ────────────────────────────────────────────────────
        var bannerGO = new GameObject("Banner");
        bannerGO.transform.SetParent(panelGO.transform, false);
        var bannerRT  = bannerGO.AddComponent<RectTransform>();
        if (layout != null)
        {
            bannerRT.anchorMin = layout.bannerAnchorMin;
            bannerRT.anchorMax = layout.bannerAnchorMax;
        }
        else
        {
            bannerRT.anchorMin = new Vector2(0.20f, 0.65f);
            bannerRT.anchorMax = new Vector2(0.55f, 0.92f);
        }
        bannerRT.offsetMin = Vector2.zero;
        bannerRT.offsetMax = Vector2.zero;
        var bannerImg = bannerGO.AddComponent<Image>();
        if (sembunyikanLatarNama)
        {
            // Banner tetap ada (sebagai container teks) tapi visual disembunyikan total
            bannerImg.sprite  = null;
            bannerImg.color   = new Color(0f, 0f, 0f, 0f);
            bannerImg.enabled = false;
        }
        else if (layout != null && layout.nameBannerSprite != null)
        {
            bannerImg.sprite = layout.nameBannerSprite;
            bannerImg.type   = Image.Type.Sliced;
            bannerImg.color  = Color.white;
        }
        else
        {
            bannerImg.color = new Color(0.14f, 0.09f, 0.01f, 0.92f);
        }
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
        if (layout != null)
        {
            bodyRT.anchorMin = layout.textAnchorMin;
            bodyRT.anchorMax = layout.textAnchorMax;
            bodyRT.offsetMin = Vector2.zero;
            bodyRT.offsetMax = Vector2.zero;
        }
        else
        {
            bodyRT.anchorMin = new Vector2(0.20f, 0.06f);
            bodyRT.anchorMax = new Vector2(0.96f, 0.63f);
            bodyRT.offsetMin = new Vector2(8f,  4f);
            bodyRT.offsetMax = new Vector2(-8f, -4f);
        }
        dialogText = bodyGO.AddComponent<TextMeshProUGUI>();
        dialogText.fontSize           = 26;
        dialogText.color              = Color.white;
        dialogText.alignment          = TextAlignmentOptions.TopLeft;
        dialogText.textWrappingMode   = TextWrappingModes.Normal;
        dialogText.raycastTarget      = false;

        // ── Tombol Lanjutkan ───────────────────────────────────────────────
        var contGO = new GameObject("ContinueButton");
        contGO.transform.SetParent(panelGO.transform, false);
        var contRT  = contGO.AddComponent<RectTransform>();
        if (layout != null)
        {
            contRT.anchorMin = layout.HintAnchorMin;
            contRT.anchorMax = layout.HintAnchorMax;
        }
        else
        {
            contRT.anchorMin = new Vector2(0.72f, 0.04f);
            contRT.anchorMax = new Vector2(0.97f, 0.30f);
        }
        contRT.offsetMin = Vector2.zero;
        contRT.offsetMax = Vector2.zero;
        var contImg = contGO.AddComponent<Image>();
        contImg.color = new Color(0.2f, 0.6f, 0.86f, 0.85f);
        continueButton = contGO.AddComponent<Button>();
        continueButton.onClick.AddListener(OnContinueClicked);
        contGO.AddComponent<ButtonPressFeedback>();

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
            // Potret/sprite profil disembunyikan dari box dialog.
            portraitImage.gameObject.SetActive(false);
            portraitImage.sprite = portraits[idx];
        }
        else
        {
            portraitImage.gameObject.SetActive(false);
        }
    }

    static Color CategoryColor(string category) => UIPalette.Kategori(category);
}
