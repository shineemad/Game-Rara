using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// MobileControls — tombol virtual untuk HP (layar sentuh).
///
/// Setup di Inspector:
///   1. Tambahkan komponen ini ke GameObject mana saja (mis. GameManager).
///   2. Biarkan semua field default — tombol dibangun otomatis.
///   3. Di HP, tombol muncul. Di PC tanpa sentuh, tombol disembunyikan.
///   4. Centang "forceShowOnDesktop" untuk melihat tampilan di editor.
///
/// Cara baca input dari script lain:
///   MobileControls.Horizontal  → float (-1, 0, +1)
///   MobileControls.IsRunning   → bool
///   MobileControls.ShoutHeld   → bool
///   MobileControls.Instance    → referensi komponen
/// </summary>
public class MobileControls : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // SINGLETON
    // ══════════════════════════════════════════════════════════════════════
    public static MobileControls Instance { get; private set; }

    /// Input horizontal dari tombol mobile (-1 = kiri, +1 = kanan, 0 = diam)
    public static float Horizontal  { get; private set; }
    /// Tombol Run sedang ditekan?
    public static bool  IsRunning   { get; private set; }
    /// Tombol Teriak sedang ditekan/ditahan?
    public static bool  ShoutHeld   { get; private set; }

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR
    // ══════════════════════════════════════════════════════════════════════

    [Header("Tampilkan")]
    [Tooltip("Tampilkan tombol mobile di PC juga (untuk testing di editor)")]
    public bool forceShowOnDesktop = false;

    [Header("Ukuran & Warna")]
    [Tooltip("Ukuran tombol arah (kiri/kanan) dalam pixel referensi")]
    public float  dpadButtonSize  = 160f;
    [Tooltip("Ukuran tombol aksi (RUN, TERIAK)")]
    public float  actionButtonSize = 140f;
    [Tooltip("Jarak dari tepi bawah layar")]
    public float  bottomMargin    = 30f;
    [Tooltip("Jarak dari tepi kiri layar untuk D-pad")]
    public float  leftMargin      = 30f;
    [Tooltip("Jarak dari tepi kanan layar untuk tombol aksi")]
    public float  rightMargin     = 30f;

    public Color  dpadColor       = new Color(1f, 1f, 1f, 0.25f);
    public Color  dpadPressColor  = new Color(1f, 0.85f, 0.2f, 0.65f);
    public Color  runColor        = new Color(0.2f, 0.8f, 0.3f, 0.5f);
    public Color  shoutColor      = new Color(0.9f, 0.2f, 0.2f, 0.6f);
    public Color  labelColor      = Color.white;

    [Header("Font (opsional)")]
    public TMP_FontAsset fontAsset;

    [Header("Icon Tombol (opsional — drag sprite ke sini)")]
    [Tooltip("Icon tombol Kiri. Jika kosong, tampilkan teks ◄")]
    public Sprite iconLeft;
    [Tooltip("Icon tombol Kanan. Jika kosong, tampilkan teks ►")]
    public Sprite iconRight;
    [Tooltip("Icon tombol RUN. Jika kosong, tampilkan teks RUN")]
    public Sprite iconRun;
    [Tooltip("Icon tombol TERIAK. Jika kosong, tampilkan teks TERIAK")]
    public Sprite iconShout;
    [Tooltip("Warna icon (default putih)")]
    public Color  iconColor = Color.white;
    [Tooltip("Ukuran icon relatif terhadap tombol (0.5 = 50% ukuran tombol)")]
    [Range(0.2f, 0.95f)]
    public float  iconSizeFraction = 0.65f;

    // ── internal ──────────────────────────────────────────────────────────
    private Canvas canvas;
    private bool   leftHeld;
    private bool   rightHeld;
    private bool   runHeld;
    private bool   shoutBtnHeld;

    private Image  leftImg, rightImg, runImg, shoutImg;

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        bool isMobile = Application.isMobilePlatform;
        if (!isMobile && !forceShowOnDesktop) return;

        BuildUI();
    }

    void Update()
    {
        // Hitung input horizontal dari tombol
        Horizontal = 0f;
        if (leftHeld)  Horizontal -= 1f;
        if (rightHeld) Horizontal += 1f;

        IsRunning = runHeld;
        ShoutHeld = shoutBtnHeld;

        // Perbarui warna tombol arah sesuai status tekan
        if (leftImg  != null) leftImg.color  = leftHeld  ? dpadPressColor : dpadColor;
        if (rightImg != null) rightImg.color = rightHeld ? dpadPressColor : dpadColor;
        if (runImg   != null) runImg.color   = runHeld   ? new Color(0.3f, 1f, 0.4f, 0.8f) : runColor;
        if (shoutImg != null) shoutImg.color = shoutBtnHeld ? new Color(1f, 0.4f, 0.2f, 0.9f) : shoutColor;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ══════════════════════════════════════════════════════════════════════
    // BUILD UI
    // ══════════════════════════════════════════════════════════════════════

    void BuildUI()
    {
        // Canvas khusus — di atas semua kecuali dialog
        var cGO = new GameObject("MobileControlsCanvas");
        DontDestroyOnLoad(cGO);
        canvas = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 800;
        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // EventSystem — dibutuhkan agar Button menerima sentuhan
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
        }

        float bs = dpadButtonSize;
        float ab = actionButtonSize;
        float bm = bottomMargin;

        // ── D-PAD: kiri & kanan (pojok kiri bawah) ──────────────────────
        // Tombol KIRI
        leftImg = MakeButton(cGO, "BtnLeft", "◄", iconLeft,
            new Vector2(leftMargin, bm),
            new Vector2(bs, bs),
            new Vector2(0f, 0f),   // anchor kiri-bawah
            new Vector2(0f, 0f),
            dpadColor,
            onDown: () => leftHeld  = true,
            onUp:   () => leftHeld  = false);

        // Tombol KANAN
        rightImg = MakeButton(cGO, "BtnRight", "►", iconRight,
            new Vector2(leftMargin + bs + 20f, bm),
            new Vector2(bs, bs),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            dpadColor,
            onDown: () => rightHeld = true,
            onUp:   () => rightHeld = false);

        // ── AKSI: RUN & TERIAK (pojok kanan bawah) ───────────────────────
        // Tombol TERIAK (besar, merah — paling sering dipakai)
        shoutImg = MakeButton(cGO, "BtnShout", "TERIAK", iconShout,
            new Vector2(-(rightMargin + ab), bm),
            new Vector2(ab, ab),
            new Vector2(1f, 0f),   // anchor kanan-bawah
            new Vector2(1f, 0f),
            shoutColor,
            onDown: () => shoutBtnHeld = true,
            onUp:   () => shoutBtnHeld = false);

        // Tombol RUN (di atas TERIAK)
        runImg = MakeButton(cGO, "BtnRun", "RUN", iconRun,
            new Vector2(-(rightMargin + ab), bm + ab + 16f),
            new Vector2(ab, ab * 0.65f),
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            runColor,
            onDown: () => runHeld = true,
            onUp:   () => runHeld = false);

        Debug.Log("[MobileControls] Tombol mobile berhasil dibuat.");
    }

    // ── Helper: buat satu tombol dengan EventTrigger hold/release ────────
    Image MakeButton(GameObject parent,
        string objName, string label, Sprite icon,
        Vector2 anchoredPos, Vector2 size,
        Vector2 anchorMin, Vector2 anchorMax,
        Color bgColor,
        System.Action onDown, System.Action onUp)
    {
        var go  = new GameObject(objName);
        go.transform.SetParent(parent.transform, false);

        var rt         = go.AddComponent<RectTransform>();
        rt.anchorMin   = anchorMin;
        rt.anchorMax   = anchorMax;
        rt.pivot       = anchorMin;   // pivot = anchor agar pos mudah dihitung
        rt.sizeDelta   = size;
        rt.anchoredPosition = anchoredPos;

        // Lingkaran latar
        var img   = go.AddComponent<Image>();
        img.color = bgColor;
        img.sprite = MakeCircleSprite();

        bool hasIcon = (icon != null);

        // Teks label (disembunyikan jika ada icon)
        var txtGO = new GameObject("Label");
        txtGO.transform.SetParent(go.transform, false);
        var txtRT     = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = new Vector2(4f, 4f);
        txtRT.offsetMax = new Vector2(-4f, -4f);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        ApplyFont(tmp);
        tmp.text      = label;
        tmp.fontSize  = 36;
        tmp.color     = labelColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableWordWrapping = true;
        txtGO.SetActive(!hasIcon);   // sembunyikan teks jika icon ada

        // Icon Image (hanya dibuat jika sprite di-assign)
        if (hasIcon)
        {
            var iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(go.transform, false);
            var iconRT = iconGO.AddComponent<RectTransform>();

            // Posisikan icon di tengah tombol dengan fraksi ukuran yang bisa diatur
            float pad = (1f - iconSizeFraction) * 0.5f;
            iconRT.anchorMin = new Vector2(pad, pad);
            iconRT.anchorMax = new Vector2(1f - pad, 1f - pad);
            iconRT.offsetMin = Vector2.zero;
            iconRT.offsetMax = Vector2.zero;

            var iconImg = iconGO.AddComponent<Image>();
            iconImg.sprite         = icon;
            iconImg.color          = iconColor;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget  = false;
        }

        // EventTrigger: PointerDown = tekan, PointerUp + PointerExit = lepas
        var et = go.AddComponent<EventTrigger>();
        AddETEntry(et, EventTriggerType.PointerDown, _ => onDown());
        AddETEntry(et, EventTriggerType.PointerUp,   _ => onUp());
        AddETEntry(et, EventTriggerType.PointerExit, _ => onUp());

        return img;
    }

    static void AddETEntry(EventTrigger et, EventTriggerType type, System.Action<BaseEventData> action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(data => action(data));
        et.triggers.Add(entry);
    }

    // Buat sprite lingkaran sederhana (agar tombol terlihat bulat)
    static Sprite MakeCircleSprite()
    {
        int res = 64;
        var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        float r = res / 2f;
        for (int y = 0; y < res; y++)
            for (int x = 0; x < res; x++)
            {
                float dx = x - r + 0.5f, dy = y - r + 0.5f;
                float d  = Mathf.Sqrt(dx * dx + dy * dy);
                float a  = Mathf.Clamp01(r - d);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
    }

    void ApplyFont(TextMeshProUGUI tmp)
    {
        TMP_FontAsset f = fontAsset;
        if (f == null) f = TMP_Settings.defaultFontAsset;
        if (f == null) f = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (f != null) tmp.font = f;
    }
}
