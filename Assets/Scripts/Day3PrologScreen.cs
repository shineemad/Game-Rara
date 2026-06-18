using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Day3PrologScreen — Prolog 3-slide sebelum Day 3 (BOSS FIGHT melawan intimidasi).
///
/// Mengikuti pola Day2PrologScreen: dipicu eksplisit oleh
/// DayTransitionManager.LanjutKeDay3() SEBELUM Day3Controller.TriggerStart().
/// Setelah slide terakhir, prolog memanggil callback (onSelesai) yang menjalankan
/// boss fight. Kalau tidak ada callback/UnityEvent, fallback langsung trigger
/// Day3Controller.
///
/// Cara setup:
///   1. Create Empty → "[Day3PrologScreen]" → Add Component Day3PrologScreen.
///   2. (Opsional) custom slides[] di Inspector (3 default sudah disediakan).
///   3. Drag GO ini ke DayTransitionManager.day3Prolog (atau biarkan auto-find via
///      Singleton.Instance).
/// </summary>
public class Day3PrologScreen : MonoBehaviour
{
    public static Day3PrologScreen Instance { get; private set; }

    // ══════════════════════════════════════════════════════════════════════
    // DATA
    // ══════════════════════════════════════════════════════════════════════

    [System.Serializable]
    public class PrologSlide
    {
        [Tooltip("Warna latar belakang slide.")]
        public Color backgroundColor = new Color(0.05f, 0.06f, 0.12f, 1f);

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

    [Header("Slide (CUSTOMIZABLE) — DEFAULT: tema boss fight Hari 3")]
    [Tooltip("3 slide default Hari 3 (parkiran SMP, musim hujan, hadapi Si Bayangan Gelap). Edit teks/sprite sesuka hati.")]
    public PrologSlide[] slides = new PrologSlide[]
    {
        // Slide 1 — pengenalan situasi (hujan, parkiran sepi, ojol palsu menghadang)
        new PrologSlide
        {
            backgroundColor = new Color(0.12f, 0.14f, 0.20f, 1f),   // biru-kelabu (hujan)
            title = "Hari 3: Hujan di Parkiran",
            text  = "Hujan deras. Rara menuju parkiran.\n" +
                    "\"Hei, mau kuantar ojol?\"",
        },
        // Slide 2 — konflik / ancaman: grooming via chat
        new PrologSlide
        {
            backgroundColor = new Color(0.18f, 0.10f, 0.12f, 1f),   // merah gelap (tegang)
            title = "Ancaman Grooming",
            text  = "Orang asing minta foto & rahasia.\n" +
                    "Ini GROOMING!",
        },
        // Slide 3 — bekal / kunci menghadapi Si Bayangan Gelap
        new PrologSlide
        {
            backgroundColor = new Color(0.10f, 0.16f, 0.14f, 1f),   // hijau gelap (panduan)
            title = "Hadapi Si Bayangan Gelap",
            text  = "BERSUARA KERAS & tekan PANIC BUTTON!\n" +
                    "Minta bantuan orang dewasa!",
        },
    };

    [Header("Sprite Bersama (CUSTOMIZABLE)")]
    [Tooltip("Sprite background panel dialog yang dipakai SEMUA slide kalau slide tidak override.")]
    public Sprite globalDialogSprite;

    [Header("Padding Teks")]
    [Tooltip("Padding teks horizontal (px di referensi 1920).")]
    public float panelPaddingH = 90f;
    [Tooltip("Padding teks vertikal (px di referensi 1080).")]
    public float panelPaddingV = 60f;

    [Header("Warna Teks (CUSTOMIZABLE) — DEFAULT: kontras di latar gelap")]
    public Color titleColor  = new Color(1f, 0.84f, 0f, 1f);          // #FFD700 kuning emas
    public Color textColor   = new Color(1f, 1f, 1f, 1f);             // putih bersih
    public Color hintColor   = new Color(1f, 0.84f, 0f, 0.85f);       // kuning emas (semi)
    public Color panelColor  = new Color(0f, 0f, 0f, 0.88f);
    public Color borderColor = new Color(1f, 0.85f, 0.2f, 1f);        // kuning ornamen

    [Header("Ukuran Font")]
    public int titleFontSize = 44;
    public int textFontSize  = 32;
    public int hintFontSize  = 22;

    [Header("Font (opsional)")]
    public TMP_FontAsset fontAsset;

    [Header("Layout — Dialog Box (referensi 1920x1080 landscape)")]
    [Range(0f, 1f)] public float panelCenterX = 0.50f;
    [Range(0f, 1f)] public float panelCenterY = 0.20f;
    [Range(0.1f, 1f)] public float panelWidth  = 0.86f;
    [Range(0.05f, 1f)] public float panelHeight = 0.30f;
    public float titleBodyGap = 12f;

    [Header("Layout — Ilustrasi")]
    [Range(0f, 1f)] public float illCenterX = 0.50f;
    [Range(0f, 1f)] public float illCenterY = 0.62f;
    [Range(0.1f, 1f)] public float illWidth  = 0.60f;
    [Range(0.05f, 1f)] public float illHeight = 0.62f;

    [Header("Layout — Tombol LANJUT")]
    public Sprite btnSprite;
    [Range(0f, 1f)] public float btnCenterX = 0.50f;
    [Range(0f, 1f)] public float btnCenterY = 0.045f;
    [Range(0.05f, 1f)] public float btnWidth  = 1.00f;
    [Range(0.02f, 0.3f)] public float btnHeight = 0.075f;
    public Color btnBgColor = new Color(0.05f, 0.08f, 0.10f, 0.92f);

    [Header("Tombol LANJUT — Teks")]
    public string hintText = "LANJUT";
    public TextAlignmentOptions hintAlign = TextAlignmentOptions.Center;

    [Header("Kontrol Input")]
    public bool advanceOnKeyboard   = true;
    public bool advanceOnMouseClick = true;

    [Header("Auto-Tampil")]
    [Tooltip("Debug saja — tampilkan otomatis saat scene start. Normalnya FALSE (dipicu manual).")]
    public bool autoTampilSaatStart = false;

    [Header("Sorting Order")]
    [Tooltip("Sorting order canvas. Default tinggi (1000) supaya di atas summary screen.")]
    public int sortingOrder = 1000;

    [Header("Event")]
    [Tooltip("Dipanggil setelah slide terakhir. DayTransitionManager auto-listen via cek instance.")]
    public UnityEngine.Events.UnityEvent onPrologEnd;

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
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        if (autoTampilSaatStart) Tampilkan();
    }

    void Update()
    {
        if (!sedangAktif || !ready) return;

        if (advanceOnKeyboard &&
            (Input.GetKeyDown(KeyCode.Space) ||
             Input.GetKeyDown(KeyCode.Return) ||
             Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            NextSlide();
            return;
        }

        // Klik di mana saja TIDAK lagi memajukan slide — gunakan tombol LANJUT.
        // (advanceOnMouseClick sengaja diabaikan agar klik di luar tombol tidak melompati slide.)
    }

    // ══════════════════════════════════════════════════════════════════════
    // PUBLIC API
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Tampilkan prolog dari slide pertama. Dipanggil eksternal (DayTransitionManager).</summary>
    public void Tampilkan(System.Action callback = null)
    {
        if (callback != null) onSelesai = callback;

        // PENTING: pastikan GameObject ini AKTIF di hierarki sebelum mulai.
        // Kalau prolog berada di bawah parent yang sengaja di-disable, maka
        // gameObject.SetActive(true) saja tidak cukup (activeInHierarchy tetap
        // false) dan StartCoroutine gagal. Lepaskan ke root scene lalu aktifkan.
        if (!gameObject.activeInHierarchy)
        {
            if (transform.parent != null) transform.SetParent(null, true);
            gameObject.SetActive(true);
        }

        if (slides == null || slides.Length == 0)
        {
            Debug.LogWarning("[Day3PrologScreen] Tidak ada slide — prolog dilewati.");
            Selesai();
            return;
        }

        if (canvas == null) BuildUI();
        if (root != null) root.SetActive(true);
        sedangAktif  = true;
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

        // PRIORITAS 3 (fallback): langsung trigger Day 3
        if (Day3Controller.Instance != null)
            Day3Controller.Instance.TriggerStart();
    }

    // ══════════════════════════════════════════════════════════════════════
    // BUILD UI
    // ══════════════════════════════════════════════════════════════════════

    void BuildUI()
    {
        var cGO = new GameObject("Day3PrologCanvas");
        canvas = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        root = new GameObject("Day3PrologRoot");
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

        hintTMP = MakeText(btnGO, "BtnLabel",
            new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(14f, 4f), new Vector2(-14f, -4f),
            hintFontSize + 4, hintColor, hintAlign);
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
}
