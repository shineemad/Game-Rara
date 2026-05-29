using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// PrologScreen — layar prolog 3 slide sebelum Day 1 dimulai.
///
/// Cara Setup di Inspector:
///   1. Tambahkan komponen ini ke GameObject mana saja di scene.
///   2. Isi slides[] di Inspector (3 slide sudah tersedia via ContextMenu).
///   3. Atur warna & font sesuai selera.
///   4. Saat scene Play, prolog langsung muncul dan menunggu klik/SPACE.
///   5. Setelah slide terakhir, onPrologEnd dipanggil (sambungkan ke Day1Controller.StartDay1).
/// </summary>
public class PrologScreen : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // DATA
    // ══════════════════════════════════════════════════════════════════════

    [System.Serializable]
    public class PrologSlide
    {
        [Tooltip("Warna latar belakang slide")]
        public Color backgroundColor = new Color(0.12f, 0.06f, 0.02f, 1f);

        [Tooltip("Sprite latar belakang penuh (opsional). Jika diisi, menutupi seluruh layar di belakang ilustrasi.")]
        public Sprite backgroundSprite;

        [Tooltip("Gambar/ilustrasi slide (opsional) — tampil di atas background, di area tengah atas")]
        public Sprite illustration;

        [Tooltip("Judul slide — mis. 'Hari 1: Jalan Kaki ke Sekolah'")]
        public string title = "Judul Slide";

        [TextArea(3, 6)]
        [Tooltip("Isi teks slide")]
        public string text  = "Teks cerita di sini...";

        [Tooltip("Sprite untuk background panel teks (opsional). Jika diisi, menggantikan warna solid panelColor.")]
        public Sprite dialogSprite;
    }

    [Header("Konten Slide")]
    [Tooltip("Tambah/kurangi slide sesukanya. Klik kanan → Load Preset untuk load data game asli.")]
    public PrologSlide[] slides;

    [Header("Tampilan")]
    [Tooltip("Sprite dialog yang dipakai untuk SEMUA slide. Drag sprite kotak dialog kamu ke sini.")]
    public Sprite globalDialogSprite;
    [Tooltip("Padding teks dari tepi kiri/kanan panel (px di resolusi 1080). Naikkan agar teks tidak menyentuh border ornamen.")]
    public float  panelPaddingH = 70f;
    [Tooltip("Padding teks dari tepi atas/bawah panel (px di resolusi 1920).")]
    public float  panelPaddingV = 44f;
    public Color  titleColor     = new Color(0.25f, 0.08f, 0f, 1f);
    public Color  textColor      = new Color(0.18f, 0.06f, 0f, 1f);
    public Color  hintColor      = new Color(1f, 1f, 1f, 0.55f);
    public Color  panelColor     = new Color(0f, 0f, 0f, 0.88f);
    public Color  borderColor    = new Color(1f, 0.85f, 0.2f, 1f);
    public int    titleFontSize  = 36;
    public int    textFontSize   = 26;
    public int    hintFontSize   = 18;

    [Header("Font (opsional)")]
    [Tooltip("Kosongkan = pakai font default TMP")]
    public TMP_FontAsset fontAsset;

    [Header("Layout — Dialog Box")]
    [Tooltip("Posisi tengah horizontal (0=kiri, 0.5=tengah, 1=kanan layar)")]
    [Range(0f, 1f)] public float panelCenterX = 0.50f;
    [Tooltip("Posisi tengah vertikal (0=bawah, 1=atas layar)")]
    [Range(0f, 1f)] public float panelCenterY = 0.26f;
    [Tooltip("Lebar dialog (0–1 relatif lebar layar)")]
    [Range(0.1f, 1f)] public float panelWidth  = 0.94f;
    [Tooltip("Tinggi dialog (0–1 relatif tinggi layar)")]
    [Range(0.05f, 1f)] public float panelHeight = 0.28f;
    [Tooltip("Jarak (px) antara judul dan isi teks")]
    public float titleBodyGap = 12f;

    [Header("Layout — Ilustrasi")]
    [Tooltip("Posisi tengah horizontal ilustrasi")]
    [Range(0f, 1f)] public float illCenterX = 0.50f;
    [Tooltip("Posisi tengah vertikal ilustrasi")]
    [Range(0f, 1f)] public float illCenterY = 0.67f;
    [Tooltip("Lebar area ilustrasi")]
    [Range(0.1f, 1f)] public float illWidth  = 0.84f;
    [Tooltip("Tinggi area ilustrasi")]
    [Range(0.05f, 1f)] public float illHeight = 0.58f;

    [Header("Layout — Tombol LANJUT")]
    [Tooltip("Sprite untuk tombol LANJUT (opsional). Jika kosong, pakai bar gelap otomatis.")]
    public Sprite btnSprite;
    [Tooltip("Posisi tengah horizontal tombol")]
    [Range(0f, 1f)] public float btnCenterX = 0.50f;
    [Tooltip("Posisi tengah vertikal tombol")]
    [Range(0f, 1f)] public float btnCenterY = 0.038f;
    [Tooltip("Lebar tombol")]
    [Range(0.05f, 1f)] public float btnWidth  = 1.00f;
    [Tooltip("Tinggi tombol")]
    [Range(0.02f, 0.3f)] public float btnHeight = 0.062f;

    [Header("Kontrol Input")]
    [Tooltip("Aktifkan agar SPACE / ENTER bisa memajukan slide (selain tombol LANJUT).")]
    public bool advanceOnKeyboard   = true;
    [Tooltip("Aktifkan agar klik mouse di mana saja bisa memajukan slide.")]
    public bool advanceOnMouseClick = true;

    [Header("Event")]
    [Tooltip("Dipanggil setelah slide terakhir selesai. Sambungkan ke metode StartDay1() di Day1Controller.")]
    public UnityEngine.Events.UnityEvent onPrologEnd;

    // ── runtime ──────────────────────────────────────────────────────────
    private Canvas           canvas;
    private GameObject       root;
    private Image            bgImage;         // warna solid background
    private Image            bgSpriteImage;   // sprite background penuh
    private Image            illustrationImg;
    private Image            panelImg;        // background panel teks
    private Outline          panelOutline;    // border kuning (disembunyikan saat pakai sprite)
    private RectTransform    panelRT;
    private RectTransform    illRT;
    private RectTransform    btnRT;
    private TextMeshProUGUI  pageCounter;
    private TextMeshProUGUI  titleTMP;
    private TextMeshProUGUI  bodyTMP;
    private TextMeshProUGUI  hintTMP;
    private Button           nextButton;      // tombol LANJUT untuk mobile

    private int  currentSlide = 0;
    private bool ready        = false;   // sudah boleh menerima input

    /// Flag statis — Day1Controller membaca ini lewat WaitUntil.
    /// Jauh lebih reliable daripada UnityEvent karena tidak bergantung
    /// pada urutan Awake/Start antar MonoBehaviour.
    public static bool prologDone = false;

    // ══════════════════════════════════════════════════════════════════════
    void Start()
    {
        // Reset flag setiap kali scene dimuat ulang
        prologDone = false;

        if (slides == null || slides.Length == 0)
        {
            Debug.LogWarning("[PrologScreen] Tidak ada slide — prolog dilewati.");
            prologDone = true;
            onPrologEnd?.Invoke();
            return;
        }

        BuildUI();
        ShowSlide(0);
    }

    void Update()
    {
        if (!ready) return;

        if (advanceOnKeyboard &&
            (Input.GetKeyDown(KeyCode.Space) ||
             Input.GetKeyDown(KeyCode.Return) ||
             Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            NextSlide();
            return;
        }

        if (advanceOnMouseClick && Input.GetMouseButtonDown(0))
            NextSlide();
    }

    // ══════════════════════════════════════════════════════════════════════
    // PUBLIC
    // ══════════════════════════════════════════════════════════════════════

    public void NextSlide()
    {
        if (!ready) return;   // cegah double-click sebelum slide selesai dimuat
        ready = false;
        currentSlide++;

        if (currentSlide < slides.Length)
        {
            ShowSlide(currentSlide);
        }
        else
        {
            EndProlog();
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // INTERNAL
    // ══════════════════════════════════════════════════════════════════════

    void ShowSlide(int idx)
    {
        var s = slides[idx];

        // Warna background solid
        bgImage.color = s.backgroundColor;

        // Sprite background penuh (menutupi warna solid jika ada)
        if (s.backgroundSprite != null)
        {
            bgSpriteImage.sprite  = s.backgroundSprite;
            bgSpriteImage.color   = Color.white;
            bgSpriteImage.enabled = true;
        }
        else
        {
            bgSpriteImage.enabled = false;
        }

        // Sprite background panel teks — per slide override, atau global
        Sprite ds = s.dialogSprite != null ? s.dialogSprite : globalDialogSprite;
        if (ds != null)
        {
            panelImg.sprite  = ds;
            panelImg.color   = Color.white;
            panelImg.type    = Image.Type.Simple;
            if (panelOutline != null) panelOutline.enabled = false;
        }
        else
        {
            panelImg.sprite = null;
            panelImg.color  = panelColor;
            if (panelOutline != null) panelOutline.enabled = true;
        }

        // Ilustrasi di area tengah atas
        if (s.illustration != null)
        {
            illustrationImg.sprite  = s.illustration;
            illustrationImg.color   = Color.white;
            illustrationImg.enabled = true;
        }
        else
        {
            illustrationImg.enabled = false;
        }

        // Teks
        pageCounter.text = $"{idx + 1} / {slides.Length}";
        titleTMP.text    = s.title;
        bodyTMP.text     = s.text;

        // Boleh input setelah frame berikutnya (hindari double-skip)
        StartCoroutine(AllowInputNextFrame());
    }

    IEnumerator AllowInputNextFrame()
    {
        yield return null;
        ready = true;
    }

    void EndProlog()
    {
        if (root != null) root.SetActive(false);

        // Set flag SEBELUM invoke — Day1Controller memantau flag ini via WaitUntil
        prologDone = true;

        onPrologEnd?.Invoke();

        // Hancurkan canvas prolog — tidak dibutuhkan lagi setelah selesai
        if (canvas != null) Destroy(canvas.gameObject);
    }

    // ══════════════════════════════════════════════════════════════════════
    // BUILD UI
    // ══════════════════════════════════════════════════════════════════════

    void BuildUI()
    {
        // Canvas khusus prolog — selalu di atas semua
        var cGO = new GameObject("PrologCanvas");
        DontDestroyOnLoad(cGO);
        canvas = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);  // portrait mobile
        scaler.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // EventSystem — dibutuhkan agar tombol LANJUT merespons klik
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Root — full screen
        root = new GameObject("PrologRoot");
        root.transform.SetParent(canvas.transform, false);
        var rootRT = root.AddComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = Vector2.zero;
        rootRT.offsetMax = Vector2.zero;

        // Background warna solid (layer paling bawah)
        bgImage = root.AddComponent<Image>();

        // Background sprite penuh (layer di atas warna, di bawah semua elemen)
        var bgSprGO = new GameObject("BackgroundSprite");
        bgSprGO.transform.SetParent(root.transform, false);
        var bgSprRT = bgSprGO.AddComponent<RectTransform>();
        bgSprRT.anchorMin = Vector2.zero;
        bgSprRT.anchorMax = Vector2.one;
        bgSprRT.offsetMin = Vector2.zero;
        bgSprRT.offsetMax = Vector2.zero;
        bgSpriteImage = bgSprGO.AddComponent<Image>();
        bgSpriteImage.preserveAspect = false;   // rentangkan penuh layar
        bgSpriteImage.enabled        = false;   // disembunyikan sampai ada sprite

        // Ilustrasi — bagian atas layar (55% atas)
        var illGO = new GameObject("Illustration");
        illGO.transform.SetParent(root.transform, false);
        illRT = illGO.AddComponent<RectTransform>();
        SetAnchors(illRT, illCenterX, illCenterY, illWidth, illHeight);
        illustrationImg = illGO.AddComponent<Image>();
        illustrationImg.preserveAspect = true;

        // Panel teks
        var panelGO = new GameObject("TextPanel");
        panelGO.transform.SetParent(root.transform, false);
        panelRT = panelGO.AddComponent<RectTransform>();
        SetAnchors(panelRT, panelCenterX, panelCenterY, panelWidth, panelHeight);
        panelImg = panelGO.AddComponent<Image>();
        panelImg.color = panelColor;
        panelOutline = panelGO.AddComponent<Outline>();
        panelOutline.effectColor    = borderColor;
        panelOutline.effectDistance = new Vector2(2f, -2f);

        float pH = panelPaddingH;
        float pV = panelPaddingV;

        // Counter halaman (pojok kanan atas panel)
        pageCounter = MakeText(panelGO, "Counter",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(pH, -(pV + 6f)), new Vector2(-pH, -pV * 0.3f),
            hintFontSize, hintColor, TextAlignmentOptions.TopRight);

        // Judul
        titleTMP = MakeText(panelGO, "Title",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(pH, -(pV * 2f + 36f)), new Vector2(-pH, -(pV + 6f)),
            titleFontSize, titleColor, TextAlignmentOptions.TopLeft);
        titleTMP.fontStyle = FontStyles.Bold;

        // Isi teks
        bodyTMP = MakeText(panelGO, "Body",
            new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(pH, pV), new Vector2(-pH, -(pV * 2f + 36f + titleBodyGap)),
            textFontSize, textColor, TextAlignmentOptions.TopLeft);
        bodyTMP.enableWordWrapping = true;

        // ── Tombol LANJUT (besar, mobile-friendly) ──────────────────────
        var btnGO = new GameObject("NextButton");
        btnGO.transform.SetParent(root.transform, false);
        btnRT = btnGO.AddComponent<RectTransform>();
        SetAnchors(btnRT, btnCenterX, btnCenterY, btnWidth, btnHeight);

        var btnImg = btnGO.AddComponent<Image>();
        if (btnSprite != null)
        {
            btnImg.sprite         = btnSprite;
            btnImg.type           = Image.Type.Simple;
            btnImg.color          = Color.white;
            btnImg.preserveAspect = false;
        }
        else
        {
            // Bar gelap semi-transparan — gaya retro bawah layar
            btnImg.color = new Color(0.05f, 0.05f, 0.08f, 0.92f);
            // Garis tipis atas sebagai border
            var topLine = new GameObject("TopBorder");
            topLine.transform.SetParent(btnGO.transform, false);
            var tlRT = topLine.AddComponent<RectTransform>();
            tlRT.anchorMin = new Vector2(0f, 1f);
            tlRT.anchorMax = new Vector2(1f, 1f);
            tlRT.pivot     = new Vector2(0.5f, 1f);
            tlRT.sizeDelta = new Vector2(0f, 2f);
            tlRT.anchoredPosition = Vector2.zero;
            var tlImg = topLine.AddComponent<Image>();
            tlImg.color = new Color(1f, 1f, 1f, 0.22f);
        }

        nextButton = btnGO.AddComponent<Button>();
        var colors = nextButton.colors;
        colors.normalColor      = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
        colors.pressedColor     = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.colorMultiplier  = 1f;
        nextButton.colors = colors;
        nextButton.onClick.AddListener(NextSlide);

        hintTMP = MakeText(btnGO, "BtnLabel",
            new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(14f, 4f), new Vector2(-14f, -4f),
            hintFontSize + 4, new Color(1f, 1f, 1f, 0.90f),
            TextAlignmentOptions.Center);
        hintTMP.text      = "▼  SPACE / KLIK UNTUK LANJUT";
        hintTMP.fontStyle = FontStyles.Bold;

        StartCoroutine(BlinkHint());
    }

    // cx/cy = center (0-1), w/h = size (0-1), resets pixel offsets to zero
    void SetAnchors(RectTransform rt, float cx, float cy, float w, float h)
    {
        rt.anchorMin = new Vector2(cx - w * 0.5f, cy - h * 0.5f);
        rt.anchorMax = new Vector2(cx + w * 0.5f, cy + h * 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    TextMeshProUGUI MakeText(GameObject parent, string name,
        Vector2 ancMin, Vector2 ancMax,
        Vector2 offMin, Vector2 offMax,
        int fontSize, Color color, TextAlignmentOptions align)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin;
        rt.anchorMax = ancMax;
        rt.pivot     = new Vector2(0f, 1f);
        rt.offsetMin = offMin;
        rt.offsetMax = offMax;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        ApplyFont(tmp);
        tmp.fontSize  = fontSize;
        tmp.color     = color;
        tmp.alignment = align;
        tmp.enableWordWrapping = true;
        return tmp;
    }

    // ── Terapkan ulang layout dari Inspector saat runtime ─────────────────
    [ContextMenu("▶ Terapkan Ulang Layout Sekarang")]
    public void ApplyLayout()
    {
        if (panelRT != null) SetAnchors(panelRT, panelCenterX, panelCenterY, panelWidth,  panelHeight);
        if (illRT   != null) SetAnchors(illRT,   illCenterX,   illCenterY,   illWidth,    illHeight);
        if (btnRT   != null) SetAnchors(btnRT,   btnCenterX,   btnCenterY,   btnWidth,    btnHeight);

        // Update posisi & padding teks secara live
        float pH = panelPaddingH;
        float pV = panelPaddingV;
        float g  = titleBodyGap;
        if (pageCounter != null)
        {
            var rt = pageCounter.rectTransform;
            rt.offsetMin = new Vector2(pH,  -(pV + 6f));
            rt.offsetMax = new Vector2(-pH, -pV * 0.3f);
        }
        if (titleTMP != null)
        {
            var rt = titleTMP.rectTransform;
            rt.offsetMin = new Vector2(pH,  -(pV * 2f + 36f));
            rt.offsetMax = new Vector2(-pH, -(pV + 6f));
        }
        if (bodyTMP != null)
        {
            var rt = bodyTMP.rectTransform;
            rt.offsetMin = new Vector2(pH,  pV);
            rt.offsetMax = new Vector2(-pH, -(pV * 2f + 36f + g));
        }

        // Refresh konten slide yang sedang tampil
        if (slides != null && currentSlide < slides.Length)
            ShowSlide(currentSlide);
    }

    // Auto-apply saat nilai Inspector berubah (selama Play mode)
    void OnValidate()
    {
        if (Application.isPlaying && panelRT != null)
            ApplyLayout();
    }

    [ContextMenu("Reset: Bar Bawah Layar (Default)")]
    void ResetBtnToBottomBar()
    {
        btnCenterX = 0.50f; btnCenterY = 0.038f;
        btnWidth   = 1.00f; btnHeight  = 0.062f;
        if (btnRT != null) SetAnchors(btnRT, btnCenterX, btnCenterY, btnWidth, btnHeight);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    [ContextMenu("Reset: Tombol Pojok Kanan Kecil")]
    void ResetBtnToRightCorner()
    {
        btnCenterX = 0.825f; btnCenterY = 0.055f;
        btnWidth   = 0.29f;  btnHeight  = 0.07f;
        if (btnRT != null) SetAnchors(btnRT, btnCenterX, btnCenterY, btnWidth, btnHeight);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    IEnumerator BlinkHint()
    {
        // Kedipkan ikon ▼ saja (karakter pertama) dengan animasi sinus
        while (true)
        {
            if (hintTMP != null)
            {
                float t = Time.time * 1.8f;
                float a = 0.45f + 0.45f * Mathf.Abs(Mathf.Sin(t));
                var c = hintTMP.color;
                c.a = a;
                hintTMP.color = c;
            }
            yield return null;
        }
    }

    void ApplyFont(TextMeshProUGUI tmp)
    {
        TMP_FontAsset f = fontAsset;
        if (f == null) f = TMP_Settings.defaultFontAsset;
        if (f == null) f = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (f != null) tmp.font = f;
    }

    // ══════════════════════════════════════════════════════════════════════
    // PRESET (klik kanan komponen → Load Preset)
    // ══════════════════════════════════════════════════════════════════════

    [ContextMenu("Load Preset: Prolog Day 1 (Game Asli)")]
    void LoadPreset()
    {
        slides = new PrologSlide[]
        {
            new PrologSlide
            {
                backgroundColor = new Color(0.53f, 0.81f, 0.92f, 1f),
                title = "Hari 1: Jalan Kaki ke Sekolah",
                text  = "Pagi hari di sebuah jalan menuju sekolah.\nRara, gadis 13 tahun berbaju ungu,\nbersiap berangkat ke SMP Harapan.\n\"Hati-hati di jalan, Rara!\" kata Ibu."
            },
            new PrologSlide
            {
                backgroundColor = new Color(0.36f, 0.63f, 0.78f, 1f),
                title = "Kenali Batas",
                text  = "Di luar rumah, banyak orang lalu-lalang.\nNggak semua orang asing bisa dipercaya!\nRara harus tetap waspada dan berani bersuara\nkalau ada yang bikin dia nggak nyaman."
            },
            new PrologSlide
            {
                backgroundColor = new Color(0.29f, 0.48f, 0.61f, 1f),
                title = "Panduan Bermain",
                text  = "← → : Gerakkan Rara ke kiri / kanan\nShift + ← → : Lari lebih cepat\nSPASI / Klik : Interaksi\nTombol TERIAK : Usir orang asing yang mendekat\n\nNyawa Rara ada 3. Hati-hati!"
            }
        };

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log("[PrologScreen] Preset Day 1 dimuat — 3 slide.");
#endif
    }
}
