using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Day2PrologScreen — Prolog 3-slide sebelum Day 2 dimulai.
///
/// Berbeda dari PrologScreen (Day 1) yang auto-tampil di Start, layar ini
/// dipicu eksplisit oleh DayTransitionManager.LanjutKeDay2() setelah user
/// menekan tombol "LANJUT HARI 2" di Day1SummaryScreen.
///
/// Cara setup:
///   1. Tambahkan Day2Preset ke scene (sudah membuat GO ini otomatis), ATAU
///      buat manual: Create Empty → "[Day2PrologScreen]" → Add Component.
///   2. Custom slides[] di Inspector (3 default sudah disediakan).
///   3. Tidak perlu wiring tambahan — DayTransitionManager mencari komponen ini
///      via Singleton.Instance dan memanggil Tampilkan() sebelum mulai Day 2.
/// </summary>
public class Day2PrologScreen : MonoBehaviour
{
    public static Day2PrologScreen Instance { get; private set; }

    // ══════════════════════════════════════════════════════════════════════
    // DATA
    // ══════════════════════════════════════════════════════════════════════

    [System.Serializable]
    public class PrologSlide
    {
        [Tooltip("Warna latar belakang slide.")]
        public Color backgroundColor = new Color(0.05f, 0.10f, 0.18f, 1f);

        [Tooltip("Sprite latar belakang penuh (opsional). Menutupi backgroundColor.")]
        public Sprite backgroundSprite;

        [Tooltip("Ilustrasi (opsional) — tampil di tengah atas layar.")]
        public Sprite illustration;

        [Tooltip("Judul slide.")]
        public string title = "Judul Slide";

        [TextArea(3, 6)]
        [Tooltip("Isi narasi slide.")]
        public string text = "Teks cerita di sini...";

        [Tooltip("Sprite background panel dialog (opsional). Override globalDialogSprite.")]
        public Sprite dialogSprite;
    }

    [Header("Slide (CUSTOMIZABLE) — DEFAULT: mengikuti narasi referensi Phaser")]
    [Tooltip("3 slide default Day 2 sesuai dengan game referensi game-jaga-diri.vercel.app. Edit teks/sprite sesuka hati.")]
    public PrologSlide[] slides = new PrologSlide[]
    {
        // Slide 1 — pengenalan situasi
        new PrologSlide
        {
            backgroundColor = new Color(0.91f, 0.72f, 0.43f, 1f),   // krem-jingga (siang hari)
            title = "Hari 2: Naik Angkot ke Sekolah",
            text  = "Siang hari. Rara menunggu di halte angkot.\n" +
                    "Ia akan naik angkot menuju sekolah.\n\n" +
                    "\"Pilih angkot yang ramai penumpang, ya!\" pesan Ibu sebelum berangkat.",
        },
        // Slide 2 — konflik / ancaman yang akan dihadapi
        new PrologSlide
        {
            backgroundColor = new Color(0.78f, 0.47f, 0.25f, 1f),   // jingga gelap (tegang)
            title = "Batas Tubuh & Dunia Digital",
            text  = "Di dalam angkot, ada penumpang yang berperilaku mencurigakan.\n\n" +
                    "Selain itu, HP Rara tiba-tiba menerima pesan dari nomor tak dikenal.",
        },
        // Slide 3 — panduan / kunci keselamatan (tone edukasi tegas)
        new PrologSlide
        {
            backgroundColor = new Color(0.55f, 0.37f, 0.24f, 1f),   // coklat (panduan)
            title = "Yang Perlu Rara Tahu",
            text  = "\u25CF Tubuhmu = milikmu! Nggak ada yang boleh sembarangan.\n" +
                    "\u25CF Area privat NGGAK BOLEH disentuh orang lain.\n" +
                    "\u25CF Ada pesan mencurigakan di HP?\n" +
                    "    Jangan balas \u2014 langsung lapor ke orang tua!",
        },
    };

    [Header("Sprite Bersama (CUSTOMIZABLE)")]
    [Tooltip("Sprite background panel dialog yang dipakai SEMUA slide kalau slide tidak override.")]
    public Sprite globalDialogSprite;
    [Tooltip("Path default sprite dialog (relatif Assets/) untuk Reset auto-assign Editor.")]
    public string defaultDialogSpritePath = "sprites/UI day 1/9.png";
    [Tooltip("Path background Day 2 per slide (relatif Assets/). Urutan = urutan slide.")]
    public string[] defaultBackgroundPaths = new string[]
    {
        "sprites/Backgroundnya/prolog day 2/prolog day 1.png",
        "sprites/Backgroundnya/prolog day 2/prolog day 2.png",
        "sprites/Backgroundnya/prolog day 2/prolog day 3.png"
    };

    [Header("Padding Teks")]
    [Tooltip("Padding teks horizontal (px di referensi 1080).")]
    public float panelPaddingH = 110f;
    [Tooltip("Padding teks vertikal (px di referensi 1920).")]
    public float panelPaddingV = 80f;

    [Header("Warna Teks (CUSTOMIZABLE) — DEFAULT: kontras di latar gelap (Phaser style)")]
    public Color titleColor  = new Color(1f, 0.84f, 0f, 1f);          // #FFD700 kuning emas
    public Color textColor   = new Color(1f, 1f, 1f, 1f);             // putih bersih
    public Color hintColor   = new Color(1f, 0.84f, 0f, 0.85f);       // kuning emas (semi)
    public Color panelColor  = new Color(0f, 0f, 0f, 0.88f);
    public Color borderColor = new Color(1f, 0.85f, 0.2f, 1f);        // kuning ornamen (= Day 1)

    [Header("Ukuran Font")]
    public int titleFontSize = 36;
    public int textFontSize  = 26;
    public int hintFontSize  = 18;

    [Header("Font (opsional)")]
    public TMP_FontAsset fontAsset;

    [Header("Layout — Dialog Box")]
    [Range(0f, 1f)] public float panelCenterX = 0.50f;
    [Range(0f, 1f)] public float panelCenterY = 0.26f;
    [Range(0.1f, 1f)] public float panelWidth  = 0.94f;
    [Range(0.05f, 1f)] public float panelHeight = 0.28f;
    public float titleBodyGap = 12f;

    [Header("Layout — Ilustrasi")]
    [Range(0f, 1f)] public float illCenterX = 0.50f;
    [Range(0f, 1f)] public float illCenterY = 0.67f;
    [Range(0.1f, 1f)] public float illWidth  = 0.84f;
    [Range(0.05f, 1f)] public float illHeight = 0.58f;

    [Header("Layout — Tombol LANJUT")]
    public Sprite btnSprite;
    [Range(0f, 1f)] public float btnCenterX = 0.50f;
    [Range(0f, 1f)] public float btnCenterY = 0.038f;
    [Range(0.05f, 1f)] public float btnWidth  = 1.00f;
    [Range(0.02f, 0.3f)] public float btnHeight = 0.062f;
    public Color btnBgColor = new Color(0.05f, 0.08f, 0.10f, 0.92f);

    [Header("Tombol LANJUT — Teks")]
    public string hintText = "\u25bc  SPACE / KLIK UNTUK LANJUT";
    public int hintFontSizeOverride = 0;
    public TextAlignmentOptions hintAlign = TextAlignmentOptions.Center;

    [Header("Kontrol Input")]
    public bool advanceOnKeyboard   = true;
    public bool advanceOnMouseClick = true;

    [Header("Auto-Tampil")]
    [Tooltip("Debug saja — tampilkan otomatis saat scene start. Normalnya FALSE (dipicu manual).")]
    public bool autoTampilSaatStart = false;

    [Header("Sorting Order")]
    [Tooltip("Sorting order canvas. Default tinggi (1000) supaya di atas Day1SummaryScreen.")]
    public int sortingOrder = 1000;

    [Header("Event")]
    [Tooltip("Dipanggil setelah slide terakhir. DayTransitionManager auto-listen via cek instance.")]
    public UnityEngine.Events.UnityEvent onPrologEnd;

    [Header("PRESET — Mirror PrologScreen Day 1")]
    [Tooltip("DIMATIKAN default. Kalau true, semua warna/font/padding/layout di-FORCE ke nilai preset Day 1 setiap Play — OVERWRITE semua nilai Inspector. Biarkan FALSE supaya perubahan Inspector kamu BENAR-BENAR berlaku saat Play. Cuma centang kalau ingin reset cepat ke gaya Day 1.")]
    public bool mirrorPrologDay1Layout = false;

    [Header("Layout Editor Runtime (saat Play)")]
    [Tooltip("Aktif default. Overlay slider runtime untuk geser/resize semua elemen secara live saat Play. Toggle dengan tombol F2 saat prolog tampil.")]
    public bool enableRuntimeLayoutEditor = true;
    [Tooltip("Tombol toggle overlay editor saat Play.")]
    public KeyCode toggleEditorKey = KeyCode.F2;

    // ── runtime ──────────────────────────────────────────────────────────
    private Canvas           canvas;
    private GameObject       root;
    private Image            bgImage;
    private Image            bgSpriteImage;
    private Image            illustrationImg;
    private Image            panelImg;
    private Outline          panelOutline;
    private RectTransform    panelRT, illRT, btnRT;
    private TextMeshProUGUI  pageCounter, titleTMP, bodyTMP, hintTMP;
    private Button           nextButton;

    // Runtime layout editor
    private GameObject       editorOverlay;
    private TextMeshProUGUI  editorReadoutTMP;
    private bool             userHasEditedRuntimeLayout = false;

    private int  currentSlide = 0;
    private bool ready        = false;
    private bool sedangAktif  = false;

    /// Callback opsional yang di-set runtime oleh DayTransitionManager.
    /// Lebih reliable daripada UnityEvent karena tidak bergantung serialisasi.
    public System.Action onSelesai;

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // PRESET: paksa layout & warna Day 1 sebelum apapun di-build
        if (mirrorPrologDay1Layout) ApplyPresetDay1Internal();

#if UNITY_EDITOR
        // Auto-load default sprite di Awake (Editor) supaya tidak perlu right-click Reset manual
        if (globalDialogSprite == null) TryLoadDefaultDialogSprite();
        TryLoadDefaultBackgrounds(overwrite: false);
#endif
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
#if UNITY_EDITOR
        if (globalDialogSprite == null) TryLoadDefaultDialogSprite();
        TryLoadDefaultBackgrounds(overwrite: false);
#endif
        if (autoTampilSaatStart) Tampilkan();
    }

    void Update()
    {
        if (!sedangAktif || !ready) return;

        // Toggle Layout Editor runtime (F2 default). Hanya saat prolog aktif.
        if (enableRuntimeLayoutEditor && Input.GetKeyDown(toggleEditorKey))
        {
            ToggleLayoutEditor();
            return;
        }

        // Kalau editor overlay aktif, JANGAN advance slide saat klik/SPACE
        // (supaya user bisa edit dengan tenang).
        if (editorOverlay != null && editorOverlay.activeSelf) return;

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
    // PUBLIC API
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Tampilkan prolog dari slide pertama. Dipanggil eksternal (DayTransitionManager).</summary>
    public void Tampilkan(System.Action callback = null)
    {
        if (callback != null) onSelesai = callback;

        // PENTING: pastikan GameObject ini AKTIF di hierarki sebelum mulai.
        // Kalau prolog berada di bawah parent yang sengaja di-disable (mis.
        // Day2_Root yang baru di-enable SETELAH prolog selesai), maka
        // gameObject.SetActive(true) saja tidak cukup — activeInHierarchy tetap
        // false karena parent-nya inactive, sehingga StartCoroutine GAGAL dan
        // prolog macet di slide pertama (tombol SPACE/KLIK tak berfungsi).
        // Solusi: lepaskan ke root scene lalu aktifkan.
        if (!gameObject.activeInHierarchy)
        {
            if (transform.parent != null) transform.SetParent(null, true);
            gameObject.SetActive(true);
        }
        if (slides == null || slides.Length == 0)
        {
            Debug.LogWarning("[Day2PrologScreen] Tidak ada slide — prolog dilewati.");
            Selesai();
            return;
        }
        // FORCE apply preset Day 1 — HANYA kalau user secara eksplisit centang
        // mirrorPrologDay1Layout = true di Inspector. Default false supaya
        // perubahan Inspector tidak ditimpa.
        if (mirrorPrologDay1Layout && !userHasEditedRuntimeLayout)
        {
            ApplyPresetDay1Internal();
            if (canvas != null) { Destroy(canvas.gameObject); canvas = null; root = null; }
        }
        if (canvas == null) BuildUI();
        if (root != null) root.SetActive(true);
        sedangAktif = true;
        currentSlide = 0;
        ShowSlide(0);
    }

    /// <summary>Tutup prolog secara paksa (tanpa panggil callback).</summary>
    public void Tutup()
    {
        sedangAktif = false;
        if (root != null) root.SetActive(false);
    }

    public void NextSlide()
    {
        if (!ready) return;
        ready = false;
        currentSlide++;
        if (currentSlide < slides.Length) ShowSlide(currentSlide);
        else Selesai();
    }

    // ══════════════════════════════════════════════════════════════════════
    // INTERNAL
    // ══════════════════════════════════════════════════════════════════════

    void ShowSlide(int idx)
    {
        var s = slides[idx];

        bgImage.color = s.backgroundColor;
        if (s.backgroundSprite != null)
        {
            bgSpriteImage.sprite  = s.backgroundSprite;
            bgSpriteImage.color   = Color.white;
            bgSpriteImage.enabled = true;
        }
        else bgSpriteImage.enabled = false;

        Sprite ds = s.dialogSprite != null ? s.dialogSprite : globalDialogSprite;
        if (ds != null)
        {
            panelImg.sprite = ds; panelImg.color = Color.white; panelImg.type = Image.Type.Simple;
            if (panelOutline != null) panelOutline.enabled = false;
        }
        else
        {
            panelImg.sprite = null; panelImg.color = panelColor;
            if (panelOutline != null) panelOutline.enabled = true;
        }

        if (s.illustration != null)
        {
            illustrationImg.sprite = s.illustration; illustrationImg.color = Color.white;
            illustrationImg.enabled = true;
        }
        else illustrationImg.enabled = false;

        pageCounter.text = $"{idx + 1} / {slides.Length}";
        titleTMP.text    = s.title;
        bodyTMP.text     = s.text;

        StartCoroutine(AllowInputNextFrame());
    }

    IEnumerator AllowInputNextFrame()
    {
        yield return null;
        ready = true;
    }

    void Selesai()
    {
        sedangAktif = false;
        if (root != null) root.SetActive(false);

        AudioManager.Instance?.Click();

        // PRIORITAS 1: callback runtime
        var cb = onSelesai;
        onSelesai = null;
        if (cb != null) { cb.Invoke(); return; }

        // PRIORITAS 2: UnityEvent Inspector
        if (onPrologEnd != null && onPrologEnd.GetPersistentEventCount() > 0)
        {
            onPrologEnd.Invoke();
            return;
        }

        // PRIORITAS 3 (fallback): langsung trigger Day 2
        if (Day2Controller.Instance != null)
            Day2Controller.Instance.TriggerStart();
    }

    // ══════════════════════════════════════════════════════════════════════
    // BUILD UI
    // ══════════════════════════════════════════════════════════════════════

    void BuildUI()
    {
        var cGO = new GameObject("Day2PrologCanvas");
        canvas = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        root = new GameObject("Day2PrologRoot");
        root.transform.SetParent(canvas.transform, false);
        var rootRT = root.AddComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero; rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = Vector2.zero; rootRT.offsetMax = Vector2.zero;

        bgImage = root.AddComponent<Image>();
        bgImage.raycastTarget = true;

        var bgSprGO = new GameObject("BackgroundSprite");
        bgSprGO.transform.SetParent(root.transform, false);
        var bgSprRT = bgSprGO.AddComponent<RectTransform>();
        bgSprRT.anchorMin = Vector2.zero; bgSprRT.anchorMax = Vector2.one;
        bgSprRT.offsetMin = Vector2.zero; bgSprRT.offsetMax = Vector2.zero;
        bgSpriteImage = bgSprGO.AddComponent<Image>();
        bgSpriteImage.preserveAspect = false;
        bgSpriteImage.enabled        = false;
        bgSpriteImage.raycastTarget  = false;

        var illGO = new GameObject("Illustration");
        illGO.transform.SetParent(root.transform, false);
        illRT = illGO.AddComponent<RectTransform>();
        SetAnchors(illRT, illCenterX, illCenterY, illWidth, illHeight);
        illustrationImg = illGO.AddComponent<Image>();
        illustrationImg.preserveAspect = true;
        illustrationImg.raycastTarget  = false;

        var panelGO = new GameObject("TextPanel");
        panelGO.transform.SetParent(root.transform, false);
        panelRT = panelGO.AddComponent<RectTransform>();
        SetAnchors(panelRT, panelCenterX, panelCenterY, panelWidth, panelHeight);
        panelImg = panelGO.AddComponent<Image>();
        panelImg.color = panelColor;
        panelImg.raycastTarget = false;
        panelOutline = panelGO.AddComponent<Outline>();
        panelOutline.effectColor    = borderColor;
        panelOutline.effectDistance = new Vector2(2f, -2f);

        float pH = panelPaddingH, pV = panelPaddingV;

        pageCounter = MakeText(panelGO, "Counter",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(pH, -(pV + 6f)), new Vector2(-pH, -pV * 0.3f),
            hintFontSize, hintColor, TextAlignmentOptions.TopRight);

        titleTMP = MakeText(panelGO, "Title",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(pH, -(pV * 2f + 36f)), new Vector2(-pH, -(pV + 6f)),
            titleFontSize, titleColor, TextAlignmentOptions.TopLeft);
        titleTMP.fontStyle = FontStyles.Bold;

        bodyTMP = MakeText(panelGO, "Body",
            new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(pH, pV), new Vector2(-pH, -(pV * 2f + 36f + titleBodyGap)),
            textFontSize, textColor, TextAlignmentOptions.TopLeft);
        bodyTMP.textWrappingMode = TextWrappingModes.Normal;

        // Tombol LANJUT (full-width bar bawah)
        var btnGO = new GameObject("NextButton");
        btnGO.transform.SetParent(root.transform, false);
        btnRT = btnGO.AddComponent<RectTransform>();
        SetAnchors(btnRT, btnCenterX, btnCenterY, btnWidth, btnHeight);

        var btnImg = btnGO.AddComponent<Image>();
        if (btnSprite != null)
        {
            btnImg.sprite = btnSprite; btnImg.type = Image.Type.Simple;
            btnImg.color = Color.white; btnImg.preserveAspect = false;
        }
        else
        {
            btnImg.color = btnBgColor;
            var topLine = new GameObject("TopBorder");
            topLine.transform.SetParent(btnGO.transform, false);
            var tlRT = topLine.AddComponent<RectTransform>();
            tlRT.anchorMin = new Vector2(0f, 1f);
            tlRT.anchorMax = new Vector2(1f, 1f);
            tlRT.pivot     = new Vector2(0.5f, 1f);
            tlRT.sizeDelta = new Vector2(0f, 2f);
            tlRT.anchoredPosition = Vector2.zero;
            var tlImg = topLine.AddComponent<Image>();
            tlImg.color = borderColor;
            tlImg.raycastTarget = false;
        }
        btnImg.raycastTarget = true;

        nextButton = btnGO.AddComponent<Button>();
        nextButton.onClick.AddListener(NextSlide);

        int resolvedHintSize = (hintFontSizeOverride > 0) ? hintFontSizeOverride : hintFontSize + 4;
        hintTMP = MakeText(btnGO, "BtnLabel",
            new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(14f, 4f), new Vector2(-14f, -4f),
            resolvedHintSize, hintColor, hintAlign);
        hintTMP.text      = hintText;
        hintTMP.fontStyle = FontStyles.Bold;

        StartCoroutine(BlinkHint());
    }

    void SetAnchors(RectTransform rt, float cx, float cy, float w, float h)
    {
        rt.anchorMin = new Vector2(cx - w * 0.5f, cy - h * 0.5f);
        rt.anchorMax = new Vector2(cx + w * 0.5f, cy + h * 0.5f);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    TextMeshProUGUI MakeText(GameObject parent, string name,
        Vector2 ancMin, Vector2 ancMax, Vector2 offMin, Vector2 offMax,
        int fontSize, Color color, TextAlignmentOptions align)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.pivot = new Vector2(0f, 1f);
        rt.offsetMin = offMin; rt.offsetMax = offMax;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        ApplyFont(tmp);
        tmp.fontSize  = fontSize;
        tmp.color     = color;
        tmp.alignment = align;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        return tmp;
    }

    void ApplyFont(TextMeshProUGUI tmp)
    {
        TMP_FontAsset f = fontAsset;
        if (f == null) f = TMP_Settings.defaultFontAsset;
        if (f != null)
        {
            tmp.font = f;
            // PENTING: refresh material juga, kalau tidak TMP render dengan
            // material font lama dan teks bisa hilang/blank.
            if (f.material != null) tmp.fontSharedMaterial = f.material;
        }
    }

    /// <summary>Refresh font ke semua TMP yang sudah dibangun (judul, body, hint, counter).
    /// Panggil ini kalau ganti fontAsset saat runtime.</summary>
    [ContextMenu("\u25B6 Refresh Font ke Semua Teks (saat Play)")]
    public void RefreshFontSemuaTeks()
    {
        if (titleTMP    != null) ApplyFont(titleTMP);
        if (bodyTMP     != null) ApplyFont(bodyTMP);
        if (hintTMP     != null) ApplyFont(hintTMP);
        if (pageCounter != null) ApplyFont(pageCounter);
        var name = (fontAsset != null) ? fontAsset.name : "Default TMP";
        Debug.Log($"[Day2PrologScreen] Font di-refresh ke: {name}");
    }

    IEnumerator BlinkHint()
    {
        while (true)
        {
            if (hintTMP != null)
            {
                float t = Time.time * 1.8f;
                float a = 0.45f + 0.45f * Mathf.Abs(Mathf.Sin(t));
                var c = hintTMP.color; c.a = a; hintTMP.color = c;
            }
            yield return null;
        }
    }

    [ContextMenu("\u25B6 Terapkan Ulang Layout Sekarang")]
    public void ApplyLayout()
    {
        if (panelRT != null) SetAnchors(panelRT, panelCenterX, panelCenterY, panelWidth, panelHeight);
        if (illRT   != null) SetAnchors(illRT,   illCenterX,   illCenterY,   illWidth,   illHeight);
        if (btnRT   != null) SetAnchors(btnRT,   btnCenterX,   btnCenterY,   btnWidth,   btnHeight);

        // Update padding teks DI DALAM panel (sama persis dengan PrologScreen Day 1)
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

        // Update FONT SIZE, COLOR, & HINT TEXT secara live (perubahan Inspector
        // untuk titleFontSize / textFontSize / warna / hintText sekarang langsung
        // terlihat tanpa harus restart Play).
        if (titleTMP != null)
        {
            titleTMP.fontSize = titleFontSize;
            titleTMP.color    = titleColor;
            ApplyFont(titleTMP);
        }
        if (bodyTMP != null)
        {
            bodyTMP.fontSize = textFontSize;
            bodyTMP.color    = textColor;
            ApplyFont(bodyTMP);
        }
        if (pageCounter != null)
        {
            pageCounter.fontSize = hintFontSize;
            pageCounter.color    = hintColor;
            ApplyFont(pageCounter);
        }
        if (hintTMP != null)
        {
            int hSize = (hintFontSizeOverride > 0) ? hintFontSizeOverride : hintFontSize + 4;
            hintTMP.fontSize  = hSize;
            hintTMP.color     = hintColor;
            hintTMP.text      = hintText;
            hintTMP.alignment = hintAlign;
            ApplyFont(hintTMP);
        }
        if (panelImg != null && panelImg.sprite == null) panelImg.color = panelColor;
        if (panelOutline != null) panelOutline.effectColor = borderColor;

        if (sedangAktif && slides != null && currentSlide < slides.Length) ShowSlide(currentSlide);
    }

    [ContextMenu("\u25B6 Reset ke Layout Prolog Day 1 (warna + posisi)")]
    public void ResetKeLayoutDay1()
    {
        ApplyPresetDay1Internal();

        if (Application.isPlaying)
        {
            // Re-build supaya warna/sprite baru apply langsung
            if (root != null) { Destroy(root); root = null; canvas = null; }
            if (sedangAktif) Tampilkan(onSelesai);
            else ApplyLayout();
        }
#if UNITY_EDITOR
        else
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        Debug.Log("[Day2PrologScreen] Layout & warna di-reset ke PrologScreen Day 1.");
    }

    /// <summary>Preset internal — copy layout & padding dari PrologScreen Day 1,
    /// tapi warna teks pakai pola Phaser (kuning emas + putih) supaya kontras
    /// di atas sprite background Day 2 yang dominan coklat gelap.</summary>
    void ApplyPresetDay1Internal()
    {
        // Padding teks
        panelPaddingH = 110f;
        panelPaddingV = 80f;

        // Warna TEKS — ikuti referensi Phaser Prolog2.js (kontras di latar gelap)
        titleColor  = new Color(1f, 0.84f, 0f, 1f);          // #FFD700 kuning emas
        textColor   = new Color(1f, 1f, 1f, 1f);             // putih bersih
        hintColor   = new Color(1f, 0.84f, 0f, 0.85f);       // kuning emas (semi)
        // Warna PANEL (kalau sprite null) — sama Day 1
        panelColor  = new Color(0f, 0f, 0f, 0.88f);
        borderColor = new Color(1f, 0.85f, 0.2f, 1f);

        // Ukuran font (sama Day 1)
        titleFontSize = 36;
        textFontSize  = 26;
        hintFontSize  = 18;

        // Layout dialog box (sama Day 1)
        panelCenterX = 0.50f;
        panelCenterY = 0.26f;
        panelWidth   = 0.94f;
        panelHeight  = 0.28f;
        titleBodyGap = 12f;

        // Layout ilustrasi (sama Day 1)
        illCenterX = 0.50f;
        illCenterY = 0.67f;
        illWidth   = 0.84f;
        illHeight  = 0.58f;

        // Layout tombol LANJUT — PRESET POJOK KANAN KECIL (sama persis screenshot Day 1)
        // Tombol kecil sticker di kanan-bawah, supaya teks panel di kiri tetap leluasa.
        btnCenterX = 0.825f;
        btnCenterY = 0.055f;
        btnWidth   = 0.29f;
        btnHeight  = 0.07f;
        btnBgColor = new Color(0.05f, 0.05f, 0.08f, 0.92f);

        // Teks hint
        hintText             = "\u25bc  SPACE / KLIK UNTUK LANJUT";
        hintAlign            = TextAlignmentOptions.Center;
        hintFontSizeOverride = 0;

        // Pakai sprite dialog yang sama dengan Day 1
        defaultDialogSpritePath = "sprites/UI day 1/9.png";
    }

    [ContextMenu("\u25B6 Reset Teks Slide ke Default Referensi")]
    public void ResetTeksKeDefaultReferensi()
    {
        slides = new PrologSlide[]
        {
            new PrologSlide
            {
                backgroundColor = new Color(0.91f, 0.72f, 0.43f, 1f),
                title = "Hari 2: Naik Angkot ke Sekolah",
                text  = "Siang hari. Rara menunggu di halte angkot.\n" +
                        "Ia akan naik angkot menuju sekolah.\n\n" +
                        "\"Pilih angkot yang ramai penumpang, ya!\" pesan Ibu sebelum berangkat.",
            },
            new PrologSlide
            {
                backgroundColor = new Color(0.78f, 0.47f, 0.25f, 1f),
                title = "Batas Tubuh & Dunia Digital",
                text  = "Di dalam angkot, ada penumpang yang berperilaku mencurigakan.\n\n" +
                        "Selain itu, HP Rara tiba-tiba menerima pesan dari nomor tak dikenal.",
            },
            new PrologSlide
            {
                backgroundColor = new Color(0.55f, 0.37f, 0.24f, 1f),
                title = "Yang Perlu Rara Tahu",
                text  = "\u25CF Tubuhmu = milikmu! Nggak ada yang boleh sembarangan.\n" +
                        "\u25CF Area privat NGGAK BOLEH disentuh orang lain.\n" +
                        "\u25CF Ada pesan mencurigakan di HP?\n" +
                        "    Jangan balas \u2014 langsung lapor ke orang tua!",
            },
        };
#if UNITY_EDITOR
        // Re-load sprite background ke 3 slide baru (kalau ada di folder)
        TryLoadDefaultBackgrounds(overwrite: true);
        if (globalDialogSprite == null) TryLoadDefaultDialogSprite();
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        if (Application.isPlaying && sedangAktif && currentSlide < slides.Length)
            ShowSlide(currentSlide);
        Debug.Log("[Day2PrologScreen] Teks 3 slide di-reset ke narasi referensi.");
    }

    void OnValidate()
    {
        // CATATAN: SENGAJA tidak panggil ApplyPresetDay1Internal() di sini supaya
        // slider/field di Inspector bisa di-edit user tanpa langsung di-overwrite preset.
        // Preset hanya apply otomatis di Awake (saat scene start) atau via:
        //   - ContextMenu "▶ Reset ke Layout Prolog Day 1"
        //   - Tombol "RESET DAY 1" di Runtime Layout Editor (F2)
        if (Application.isPlaying && panelRT != null) ApplyLayout();

        // Auto-refresh font kalau user ganti fontAsset di Inspector saat Play
        if (Application.isPlaying && titleTMP != null) RefreshFontSemuaTeks();
    }

#if UNITY_EDITOR
    void Reset()
    {
        TryLoadDefaultDialogSprite();
        TryLoadDefaultBackgrounds(overwrite: false);
    }

    [ContextMenu("\u25B6 Muat Sprite Dialog Default")]
    void TryLoadDefaultDialogSpriteMenu()
    {
        TryLoadDefaultDialogSprite();
        if (globalDialogSprite != null) Debug.Log($"[Day2PrologScreen] Sprite dialog: {globalDialogSprite.name}");
    }

    [ContextMenu("\u25B6 Muat Background Prolog Day 2")]
    void TryLoadDefaultBackgroundsMenu() => TryLoadDefaultBackgrounds(overwrite: true);

    void TryLoadDefaultDialogSprite()
    {
        if (string.IsNullOrEmpty(defaultDialogSpritePath)) return;
        var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/" + defaultDialogSpritePath);
        if (sp != null) { globalDialogSprite = sp; UnityEditor.EditorUtility.SetDirty(this); }
    }

    void TryLoadDefaultBackgrounds(bool overwrite)
    {
        if (defaultBackgroundPaths == null || defaultBackgroundPaths.Length == 0) return;
        if (slides == null || slides.Length < defaultBackgroundPaths.Length)
        {
            var resized = new PrologSlide[defaultBackgroundPaths.Length];
            if (slides != null)
                for (int i = 0; i < slides.Length && i < resized.Length; i++) resized[i] = slides[i];
            for (int i = 0; i < resized.Length; i++) if (resized[i] == null) resized[i] = new PrologSlide();
            slides = resized;
        }
        int assigned = 0;
        for (int i = 0; i < defaultBackgroundPaths.Length; i++)
        {
            if (i >= slides.Length) break;
            if (slides[i] == null) slides[i] = new PrologSlide();
            if (!overwrite && slides[i].backgroundSprite != null) continue;
            var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/" + defaultBackgroundPaths[i]);
            if (sp != null) { slides[i].backgroundSprite = sp; assigned++; }
        }
        if (assigned > 0)
        {
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[Day2PrologScreen] {assigned} background di-assign.");
        }
    }
#endif

    // ══════════════════════════════════════════════════════════════════════
    // RUNTIME LAYOUT EDITOR (toggle F2 saat Play)
    // ══════════════════════════════════════════════════════════════════════

    void ToggleLayoutEditor()
    {
        if (editorOverlay == null) BuildLayoutEditor();
        else editorOverlay.SetActive(!editorOverlay.activeSelf);
        UpdateEditorReadout();
    }

    void BuildLayoutEditor()
    {
        if (canvas == null) return;

        // Saat editor dipakai, matikan mirror Day 1 supaya slider tidak ditimpa preset
        mirrorPrologDay1Layout = false;

        editorOverlay = new GameObject("LayoutEditorOverlay");
        editorOverlay.transform.SetParent(canvas.transform, false);
        var orRT = editorOverlay.AddComponent<RectTransform>();
        // Panel kanan, full tinggi, lebar 460 px (resolusi 1080 = ~43%)
        orRT.anchorMin = new Vector2(1f, 0f);
        orRT.anchorMax = new Vector2(1f, 1f);
        orRT.pivot     = new Vector2(1f, 0.5f);
        orRT.sizeDelta = new Vector2(460f, 0f);
        orRT.anchoredPosition = Vector2.zero;

        var bg = editorOverlay.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.78f);
        bg.raycastTarget = true;

        // Outline kiri
        var line = new GameObject("EdgeLine");
        line.transform.SetParent(editorOverlay.transform, false);
        var lineRT = line.AddComponent<RectTransform>();
        lineRT.anchorMin = new Vector2(0f, 0f);
        lineRT.anchorMax = new Vector2(0f, 1f);
        lineRT.pivot     = new Vector2(0f, 0.5f);
        lineRT.sizeDelta = new Vector2(3f, 0f);
        lineRT.anchoredPosition = Vector2.zero;
        var lineImg = line.AddComponent<Image>();
        lineImg.color = new Color(1f, 0.85f, 0.2f, 1f);
        lineImg.raycastTarget = false;

        // Judul header
        EdMakeText(editorOverlay, "Header",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(20f, -50f), new Vector2(-20f, -10f),
            22, new Color(1f, 0.84f, 0f, 1f), TextAlignmentOptions.TopLeft,
            "LAYOUT EDITOR  (F2 = toggle)", true);

        // Tombol RESET PRESET DAY 1 (kiri 33%)
        EdMakeButton(editorOverlay, "BtnReset",
            new Vector2(0f, 1f), new Vector2(0.33f, 1f),
            new Vector2(20f, -82f), new Vector2(-3f, -54f),
            "RESET DAY 1", new Color(0.5f, 0.3f, 0.2f, 1f),
            () => {
                mirrorPrologDay1Layout = true;
                ApplyPresetDay1Internal();
                if (panelRT != null) SetAnchors(panelRT, panelCenterX, panelCenterY, panelWidth, panelHeight);
                if (illRT   != null) SetAnchors(illRT,   illCenterX,   illCenterY,   illWidth,   illHeight);
                if (btnRT   != null) SetAnchors(btnRT,   btnCenterX,   btnCenterY,   btnWidth,   btnHeight);
                if (titleTMP != null) titleTMP.fontSize = titleFontSize;
                if (bodyTMP  != null) bodyTMP.fontSize  = textFontSize;
                if (hintTMP  != null) hintTMP.fontSize  = (hintFontSizeOverride > 0) ? hintFontSizeOverride : hintFontSize + 4;
                if (titleTMP != null) titleTMP.color = titleColor;
                if (bodyTMP  != null) bodyTMP.color  = textColor;
                if (hintTMP  != null) hintTMP.color  = hintColor;
                if (panelImg != null) panelImg.color = panelColor;
                if (panelOutline != null) panelOutline.effectColor = borderColor;
                mirrorPrologDay1Layout = false;
                userHasEditedRuntimeLayout = false; // reset flag — preset baru saja diapply
                UpdateEditorReadout();
            });

        // Tombol PRINT (tengah 33%)
        EdMakeButton(editorOverlay, "BtnPrint",
            new Vector2(0.33f, 1f), new Vector2(0.66f, 1f),
            new Vector2(3f, -82f), new Vector2(-3f, -54f),
            "PRINT", new Color(0.2f, 0.5f, 0.2f, 1f),
            () => {
                Debug.Log(GetLayoutValuesAsCode());
                UpdateEditorReadout();
            });

        // Tombol REFRESH FONT (kanan 33%) — apply fontAsset Inspector ke semua TMP runtime
        EdMakeButton(editorOverlay, "BtnFont",
            new Vector2(0.66f, 1f), new Vector2(1f, 1f),
            new Vector2(3f, -82f), new Vector2(-20f, -54f),
            "REFRESH FONT", new Color(0.4f, 0.2f, 0.5f, 1f),
            () => {
                RefreshFontSemuaTeks();
                UpdateEditorReadout();
            });

        // Container slider (scrollable area)
        var content = new GameObject("Content");
        content.transform.SetParent(editorOverlay.transform, false);
        var contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 0f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot     = new Vector2(0.5f, 1f);
        contentRT.offsetMin = new Vector2(20f, 280f);
        contentRT.offsetMax = new Vector2(-20f, -120f);

        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8f;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth      = true;
        vlg.childControlHeight     = true;

        // ── Slider rows ─────────────────────────────────────────────────
        EdSection(content, "PANEL DIALOG");
        EdSlider(content, "panel CenterX",  0f,    1f,   panelCenterX,  v => { panelCenterX = v; ReapplyPanel(); });
        EdSlider(content, "panel CenterY",  0f,    1f,   panelCenterY,  v => { panelCenterY = v; ReapplyPanel(); });
        EdSlider(content, "panel Width",    0.1f,  1f,   panelWidth,    v => { panelWidth = v;   ReapplyPanel(); });
        EdSlider(content, "panel Height",   0.05f, 1f,   panelHeight,   v => { panelHeight = v;  ReapplyPanel(); });
        EdSlider(content, "padding H (px)", 0f,    300f, panelPaddingH, v => { panelPaddingH = v; ReapplyText(); });
        EdSlider(content, "padding V (px)", 0f,    200f, panelPaddingV, v => { panelPaddingV = v; ReapplyText(); });
        EdSlider(content, "titleBodyGap",   0f,    80f,  titleBodyGap,  v => { titleBodyGap = v; ReapplyText(); });

        EdSection(content, "ILUSTRASI");
        EdSlider(content, "ill CenterX",    0f, 1f, illCenterX, v => { illCenterX = v; ReapplyIll(); });
        EdSlider(content, "ill CenterY",    0f, 1f, illCenterY, v => { illCenterY = v; ReapplyIll(); });
        EdSlider(content, "ill Width",      0.1f, 1f, illWidth,  v => { illWidth = v;   ReapplyIll(); });
        EdSlider(content, "ill Height",     0.05f, 1f, illHeight, v => { illHeight = v; ReapplyIll(); });

        EdSection(content, "TOMBOL LANJUT");
        EdSlider(content, "btn CenterX",    0f, 1f,   btnCenterX, v => { btnCenterX = v; ReapplyBtn(); });
        EdSlider(content, "btn CenterY",    0f, 0.5f, btnCenterY, v => { btnCenterY = v; ReapplyBtn(); });
        EdSlider(content, "btn Width",      0.05f, 1f,  btnWidth,  v => { btnWidth = v;   ReapplyBtn(); });
        EdSlider(content, "btn Height",     0.02f, 0.3f, btnHeight, v => { btnHeight = v; ReapplyBtn(); });

        EdSection(content, "FONT SIZE");
        EdSlider(content, "titleFontSize",  10f, 80f, titleFontSize, v => { titleFontSize = Mathf.RoundToInt(v); if (titleTMP) titleTMP.fontSize = titleFontSize; UpdateEditorReadout(); });
        EdSlider(content, "textFontSize",   10f, 60f, textFontSize,  v => { textFontSize  = Mathf.RoundToInt(v); if (bodyTMP)  bodyTMP.fontSize  = textFontSize;  UpdateEditorReadout(); });
        EdSlider(content, "hintFontSize",   8f,  40f, hintFontSize,  v => { hintFontSize  = Mathf.RoundToInt(v); if (hintTMP)  hintTMP.fontSize  = hintFontSize + 4; UpdateEditorReadout(); });

        // Readout bawah
        editorReadoutTMP = EdMakeText(editorOverlay, "Readout",
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(20f, 20f), new Vector2(-20f, 260f),
            14, new Color(0.85f, 0.85f, 0.85f, 1f), TextAlignmentOptions.TopLeft,
            "", false);
        editorReadoutTMP.textWrappingMode = TextWrappingModes.Normal;

        UpdateEditorReadout();
    }

    void ReapplyPanel() { if (panelRT != null) SetAnchors(panelRT, panelCenterX, panelCenterY, panelWidth, panelHeight); ReapplyText(); UpdateEditorReadout(); }
    void ReapplyIll()   { if (illRT   != null) SetAnchors(illRT,   illCenterX,   illCenterY,   illWidth,   illHeight);   UpdateEditorReadout(); }
    void ReapplyBtn()   { if (btnRT   != null) SetAnchors(btnRT,   btnCenterX,   btnCenterY,   btnWidth,   btnHeight);   UpdateEditorReadout(); }
    void ReapplyText()
    {
        // Anchor & offset teks tergantung padding/titleBodyGap → recompute
        float pH = panelPaddingH, pV = panelPaddingV;
        if (pageCounter != null)
        {
            var rt = pageCounter.rectTransform;
            rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = new Vector2(pH, -(pV + 6f));
            rt.offsetMax = new Vector2(-pH, -pV * 0.3f);
        }
        if (titleTMP != null)
        {
            var rt = titleTMP.rectTransform;
            rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = new Vector2(pH, -(pV * 2f + 36f));
            rt.offsetMax = new Vector2(-pH, -(pV + 6f));
        }
        if (bodyTMP != null)
        {
            var rt = bodyTMP.rectTransform;
            rt.anchorMin = new Vector2(0f, 0f); rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = new Vector2(pH, pV);
            rt.offsetMax = new Vector2(-pH, -(pV * 2f + 36f + titleBodyGap));
        }
    }

    void UpdateEditorReadout()
    {
        if (editorReadoutTMP == null) return;
        editorReadoutTMP.text =
            $"<b>Nilai Saat Ini:</b>\n" +
            $"panel  C=({panelCenterX:F2},{panelCenterY:F2})  W={panelWidth:F2} H={panelHeight:F2}\n" +
            $"pad    H={panelPaddingH:F0} V={panelPaddingV:F0}  gap={titleBodyGap:F0}\n" +
            $"ill    C=({illCenterX:F2},{illCenterY:F2})  W={illWidth:F2} H={illHeight:F2}\n" +
            $"btn    C=({btnCenterX:F2},{btnCenterY:F2})  W={btnWidth:F2} H={btnHeight:F2}\n" +
            $"font   T={titleFontSize} B={textFontSize} H={hintFontSize}";
    }

    string GetLayoutValuesAsCode()
    {
        return
            "[Day2PrologScreen LAYOUT — copy ke ApplyPresetDay1Internal()]\n" +
            $"panelCenterX = {panelCenterX:F2}f;\n" +
            $"panelCenterY = {panelCenterY:F2}f;\n" +
            $"panelWidth   = {panelWidth:F2}f;\n" +
            $"panelHeight  = {panelHeight:F2}f;\n" +
            $"panelPaddingH = {panelPaddingH:F0}f;\n" +
            $"panelPaddingV = {panelPaddingV:F0}f;\n" +
            $"titleBodyGap = {titleBodyGap:F0}f;\n" +
            $"illCenterX = {illCenterX:F2}f;\n" +
            $"illCenterY = {illCenterY:F2}f;\n" +
            $"illWidth   = {illWidth:F2}f;\n" +
            $"illHeight  = {illHeight:F2}f;\n" +
            $"btnCenterX = {btnCenterX:F2}f;\n" +
            $"btnCenterY = {btnCenterY:F2}f;\n" +
            $"btnWidth   = {btnWidth:F2}f;\n" +
            $"btnHeight  = {btnHeight:F2}f;\n" +
            $"titleFontSize = {titleFontSize};\n" +
            $"textFontSize  = {textFontSize};\n" +
            $"hintFontSize  = {hintFontSize};";
    }

    // ── Helper UI builders untuk Layout Editor ──────────────────────────

    void EdSection(GameObject parent, string label)
    {
        var go = new GameObject("Section_" + label);
        go.transform.SetParent(parent.transform, false);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 28f;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        ApplyFont(tmp);
        tmp.text = label;
        tmp.fontSize = 16;
        tmp.color = new Color(1f, 0.85f, 0.2f, 1f);
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
    }

    void EdSlider(GameObject parent, string label, float min, float max, float value, System.Action<float> onChange)
    {
        var row = new GameObject("Row_" + label);
        row.transform.SetParent(parent.transform, false);
        var le = row.AddComponent<LayoutElement>();
        le.preferredHeight = 36f;

        // Label
        var lbl = new GameObject("Label");
        lbl.transform.SetParent(row.transform, false);
        var lblRT = lbl.AddComponent<RectTransform>();
        lblRT.anchorMin = new Vector2(0f, 0f); lblRT.anchorMax = new Vector2(0.42f, 1f);
        lblRT.offsetMin = Vector2.zero; lblRT.offsetMax = Vector2.zero;
        var lblTMP = lbl.AddComponent<TextMeshProUGUI>();
        ApplyFont(lblTMP);
        lblTMP.text = label;
        lblTMP.fontSize = 13;
        lblTMP.color = Color.white;
        lblTMP.alignment = TextAlignmentOptions.MidlineLeft;

        // Slider background
        var sliderGO = new GameObject("Slider");
        sliderGO.transform.SetParent(row.transform, false);
        var sliderRT = sliderGO.AddComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0.42f, 0f); sliderRT.anchorMax = new Vector2(0.80f, 1f);
        sliderRT.offsetMin = new Vector2(0f, 8f); sliderRT.offsetMax = new Vector2(0f, -8f);
        var slider = sliderGO.AddComponent<Slider>();

        // Background image
        var sBg = new GameObject("Background");
        sBg.transform.SetParent(sliderGO.transform, false);
        var sBgRT = sBg.AddComponent<RectTransform>();
        sBgRT.anchorMin = Vector2.zero; sBgRT.anchorMax = Vector2.one;
        sBgRT.offsetMin = Vector2.zero; sBgRT.offsetMax = Vector2.zero;
        var sBgImg = sBg.AddComponent<Image>();
        sBgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // Fill area + Fill
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        var faRT = fillArea.AddComponent<RectTransform>();
        faRT.anchorMin = new Vector2(0f, 0.25f); faRT.anchorMax = new Vector2(1f, 0.75f);
        faRT.offsetMin = new Vector2(5f, 0f); faRT.offsetMax = new Vector2(-5f, 0f);

        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillRT = fill.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero; fillRT.offsetMax = Vector2.zero;
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(1f, 0.85f, 0.2f, 1f);

        // Handle area + Handle
        var handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderGO.transform, false);
        var haRT = handleArea.AddComponent<RectTransform>();
        haRT.anchorMin = Vector2.zero; haRT.anchorMax = Vector2.one;
        haRT.offsetMin = new Vector2(10f, 0f); haRT.offsetMax = new Vector2(-10f, 0f);

        var handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        var hRT = handle.AddComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0f, 0f); hRT.anchorMax = new Vector2(0f, 1f);
        hRT.sizeDelta = new Vector2(20f, 0f);
        var hImg = handle.AddComponent<Image>();
        hImg.color = Color.white;

        slider.fillRect      = fillRT;
        slider.handleRect    = hRT;
        slider.targetGraphic = hImg;
        slider.direction     = Slider.Direction.LeftToRight;
        slider.minValue      = min;
        slider.maxValue      = max;
        slider.value         = value;
        slider.onValueChanged.AddListener(v => { userHasEditedRuntimeLayout = true; onChange(v); UpdateSliderReadout(row, v); });

        // Value text
        var val = new GameObject("Value");
        val.transform.SetParent(row.transform, false);
        var valRT = val.AddComponent<RectTransform>();
        valRT.anchorMin = new Vector2(0.80f, 0f); valRT.anchorMax = new Vector2(1f, 1f);
        valRT.offsetMin = Vector2.zero; valRT.offsetMax = Vector2.zero;
        var valTMP = val.AddComponent<TextMeshProUGUI>();
        ApplyFont(valTMP);
        valTMP.text = FormatVal(value);
        valTMP.fontSize = 13;
        valTMP.color = new Color(1f, 0.85f, 0.2f, 1f);
        valTMP.alignment = TextAlignmentOptions.MidlineRight;
        valTMP.fontStyle = FontStyles.Bold;
        valTMP.name = "ValueText";
    }

    string FormatVal(float v)
    {
        if (Mathf.Abs(v) >= 10f) return v.ToString("F0");
        return v.ToString("F2");
    }

    void UpdateSliderReadout(GameObject row, float v)
    {
        var valTr = row.transform.Find("Value");
        if (valTr == null) return;
        var tmp = valTr.GetComponent<TextMeshProUGUI>();
        if (tmp != null) tmp.text = FormatVal(v);
        UpdateEditorReadout();
    }

    TextMeshProUGUI EdMakeText(GameObject parent, string name,
        Vector2 ancMin, Vector2 ancMax, Vector2 offMin, Vector2 offMax,
        int fontSize, Color color, TextAlignmentOptions align, string text, bool bold)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.offsetMin = offMin; rt.offsetMax = offMax;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        ApplyFont(tmp);
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        if (bold) tmp.fontStyle = FontStyles.Bold;
        return tmp;
    }

    void EdMakeButton(GameObject parent, string name,
        Vector2 ancMin, Vector2 ancMax, Vector2 offMin, Vector2 offMax,
        string label, Color bg, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.offsetMin = offMin; rt.offsetMax = offMax;
        var img = go.AddComponent<Image>();
        img.color = bg;
        var btn = go.AddComponent<Button>();
        btn.onClick.AddListener(onClick);
        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(go.transform, false);
        var lblRT = lblGO.AddComponent<RectTransform>();
        lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one;
        lblRT.offsetMin = Vector2.zero; lblRT.offsetMax = Vector2.zero;
        var tmp = lblGO.AddComponent<TextMeshProUGUI>();
        ApplyFont(tmp);
        tmp.text = label;
        tmp.fontSize = 14;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
    }
}

