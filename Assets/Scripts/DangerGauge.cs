using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// DangerGauge — Meteran Bahaya melayang yang memberi umpan balik berkelanjutan
/// seberapa terkendali situasi Rara.
///
/// Membaca <see cref="GameState.dangerLevel"/> (0..1). Bar mengisi mulus dari
/// kiri ke kanan, warnanya berubah hijau → kuning → merah, dan berdenyut saat
/// bahaya tinggi. Tidak perlu setup di scene — panggil statis:
///
///   DangerGauge.Show();   // tampilkan (auto-buat singleton + canvas)
///   DangerGauge.Hide();   // sembunyikan
///
/// Sprite/ikon opsional bisa di-assign lewat Inspector kalau komponen ini
/// dipasang manual di scene.
/// </summary>
public class DangerGauge : MonoBehaviour
{
    public static DangerGauge Instance { get; private set; }

    [Header("Sprite (opsional)")]
    [Tooltip("Sprite latar panel meteran. Kosong = rounded rect.")]
    public Sprite panelSprite;
    [Tooltip("Ikon di kiri label (opsional). Kosong = emoji \u26A0.")]
    public Sprite ikonSprite;

    [Header("Teks")]
    public string judul = "TINGKAT BAHAYA";
    public int    judulUkuran = 18;
    public Color  judulWarna = new Color(1f, 1f, 1f, 0.9f);

    [Header("Warna Bar (gradasi sesuai tingkat)")]
    public Color warnaAman   = new Color(0.20f, 0.78f, 0.40f, 1f);  // hijau
    public Color warnaSedang = new Color(0.98f, 0.78f, 0.18f, 1f);  // kuning
    public Color warnaBahaya = new Color(0.90f, 0.25f, 0.20f, 1f);  // merah
    public Color warnaBgBar  = new Color(0.10f, 0.10f, 0.12f, 0.9f);
    public Color warnaPanel  = new Color(0.06f, 0.07f, 0.10f, 0.92f);
    public Color warnaBorder  = new Color(1f, 1f, 1f, 0.18f);

    [Header("Ukuran & Posisi (referensi 1920x1080)")]
    public Vector2 ukuranPanel = new Vector2(360f, 78f);
    [Tooltip("Jarak dari tepi kiri.")]
    public float marginKiri = 24f;
    [Tooltip("Jarak dari tepi atas.")]
    public float marginAtas = 150f;

    [Header("Animasi")]
    [Tooltip("Kecepatan bar mengejar nilai bahaya.")]
    public float kecepatanLerp = 4f;
    [Tooltip("Ambang bahaya tinggi yang memicu denyut.")]
    [Range(0f, 1f)] public float ambangDenyut = 0.7f;

    [Header("Font (opsional)")]
    public TMP_FontAsset fontAsset;

    [Header("Sorting")]
    public int sortingOrder = 940;

    // ── runtime ───────────────────────────────────────────────────────────
    private Canvas        _canvas;
    private RectTransform _fillRT;
    private Image         _fillImg;
    private Image         _panelImg;
    private TextMeshProUGUI _persenText;
    private Sprite        _roundedSprite;
    private float         _tampil;   // 0..1 nilai bar yang dianimasikan
    private float         _maxFillWidth;

    // ══════════════════════════════════════════════════════════════════════
    // PUBLIC API
    // ══════════════════════════════════════════════════════════════════════
    public static void Show()
    {
        EnsureInstance();
        Instance.gameObject.SetActive(true);
        if (Instance._canvas != null) Instance._canvas.gameObject.SetActive(true);
    }

    public static void Hide()
    {
        if (Instance != null && Instance._canvas != null)
            Instance._canvas.gameObject.SetActive(false);
    }

    static void EnsureInstance()
    {
        if (Instance != null) return;
        var go = new GameObject("[DangerGauge]");
        Instance = go.AddComponent<DangerGauge>();
        DontDestroyOnLoad(go);
    }

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else if (Instance != this) { Destroy(gameObject); return; }
    }

    void OnEnable()
    {
        if (_canvas == null) BuildUI();
        var gs = GameState.Instance;
        if (gs != null) _tampil = gs.dangerLevel;
    }

    void Update()
    {
        var gs = GameState.Instance;
        if (gs == null || _fillRT == null) return;

        // Bar mengejar nilai target dengan mulus.
        _tampil = Mathf.MoveTowards(_tampil, gs.dangerLevel, kecepatanLerp * Time.unscaledDeltaTime);

        _fillRT.sizeDelta = new Vector2(_maxFillWidth * _tampil, _fillRT.sizeDelta.y);

        // Warna gradasi: hijau → kuning → merah.
        Color c = _tampil < 0.5f
            ? Color.Lerp(warnaAman, warnaSedang, _tampil / 0.5f)
            : Color.Lerp(warnaSedang, warnaBahaya, (_tampil - 0.5f) / 0.5f);
        _fillImg.color = c;

        if (_persenText != null)
            _persenText.text = Mathf.RoundToInt(_tampil * 100f) + "%";

        // Denyut saat bahaya tinggi.
        if (_panelImg != null)
        {
            if (_tampil >= ambangDenyut)
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6f);
                _panelImg.color = Color.Lerp(warnaPanel, new Color(0.45f, 0.08f, 0.08f, 0.95f), pulse);
            }
            else _panelImg.color = warnaPanel;
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    void BuildUI()
    {
        var canvasGO = new GameObject("DangerGauge_Canvas");
        canvasGO.transform.SetParent(transform, false);
        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = sortingOrder;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Panel
        var panel = new GameObject("Panel");
        panel.transform.SetParent(canvasGO.transform, false);
        _panelImg = panel.AddComponent<Image>();
        _panelImg.sprite = panelSprite != null ? panelSprite : GetRoundedSprite();
        _panelImg.color  = warnaPanel;
        _panelImg.type   = Image.Type.Sliced;
        _panelImg.raycastTarget = false;
        var outl = panel.AddComponent<Outline>();
        outl.effectColor = warnaBorder; outl.effectDistance = new Vector2(1.5f, -1.5f);
        var pRT = panel.GetComponent<RectTransform>();
        pRT.anchorMin = new Vector2(0f, 1f); pRT.anchorMax = new Vector2(0f, 1f);
        pRT.pivot = new Vector2(0f, 1f);
        pRT.sizeDelta = ukuranPanel;
        pRT.anchoredPosition = new Vector2(marginKiri, -marginAtas);

        // Judul (atas) + persen (kanan)
        var jud = BuatTeks(panel.transform, "Judul", "\u26A0  " + judul, judulUkuran, judulWarna, FontStyles.Bold);
        jud.alignment = TextAlignmentOptions.MidlineLeft;
        var jRT = jud.rectTransform;
        jRT.anchorMin = new Vector2(0f, 1f); jRT.anchorMax = new Vector2(1f, 1f);
        jRT.pivot = new Vector2(0.5f, 1f);
        jRT.offsetMin = new Vector2(14f, -34f); jRT.offsetMax = new Vector2(-58f, -6f);

        _persenText = BuatTeks(panel.transform, "Persen", "0%", judulUkuran, judulWarna, FontStyles.Bold);
        _persenText.alignment = TextAlignmentOptions.MidlineRight;
        var prt = _persenText.rectTransform;
        prt.anchorMin = new Vector2(1f, 1f); prt.anchorMax = new Vector2(1f, 1f);
        prt.pivot = new Vector2(1f, 1f);
        prt.sizeDelta = new Vector2(56f, 28f);
        prt.anchoredPosition = new Vector2(-12f, -6f);

        // Track bar (bg)
        var barBg = new GameObject("BarBG");
        barBg.transform.SetParent(panel.transform, false);
        var bgImg = barBg.AddComponent<Image>();
        bgImg.sprite = GetRoundedSprite(); bgImg.color = warnaBgBar; bgImg.type = Image.Type.Sliced;
        bgImg.raycastTarget = false;
        var bgRT = barBg.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0f, 0f); bgRT.anchorMax = new Vector2(1f, 0f);
        bgRT.pivot = new Vector2(0.5f, 0f);
        bgRT.offsetMin = new Vector2(14f, 12f); bgRT.offsetMax = new Vector2(-14f, 12f);
        bgRT.sizeDelta = new Vector2(bgRT.sizeDelta.x, 18f);

        // Fill
        var fill = new GameObject("Fill");
        fill.transform.SetParent(barBg.transform, false);
        _fillImg = fill.AddComponent<Image>();
        _fillImg.sprite = GetRoundedSprite(); _fillImg.color = warnaAman; _fillImg.type = Image.Type.Sliced;
        _fillImg.raycastTarget = false;
        _fillRT = fill.GetComponent<RectTransform>();
        _fillRT.anchorMin = new Vector2(0f, 0f); _fillRT.anchorMax = new Vector2(0f, 1f);
        _fillRT.pivot = new Vector2(0f, 0.5f);
        _fillRT.offsetMin = new Vector2(2f, 2f); _fillRT.offsetMax = new Vector2(2f, -2f);
        _maxFillWidth = ukuranPanel.x - 28f - 4f;
        _fillRT.sizeDelta = new Vector2(0f, 0f);
    }

    // ── Helper ──────────────────────────────────────────────────────────────
    TextMeshProUGUI BuatTeks(Transform parent, string name, string content, int size, Color color, FontStyles style)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) tmp.font = fontAsset;
        else if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
        tmp.text = content; tmp.fontSize = size; tmp.color = color; tmp.fontStyle = style;
        tmp.textWrappingMode = TextWrappingModes.NoWrap; tmp.raycastTarget = false;
        return tmp;
    }

    Sprite GetRoundedSprite()
    {
        if (_roundedSprite != null) return _roundedSprite;
        int size = 64; int radius = 14;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp; tex.filterMode = FilterMode.Bilinear;
        Color32 w = new Color32(255, 255, 255, 255), c = new Color32(255, 255, 255, 0);
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
        {
            bool inside = true;
            if      (x < radius && y < radius)               { int dx = radius - x, dy = radius - y; inside = dx * dx + dy * dy <= radius * radius; }
            else if (x >= size - radius && y < radius)        { int dx = x - (size - 1 - radius), dy = radius - y; inside = dx * dx + dy * dy <= radius * radius; }
            else if (x < radius && y >= size - radius)        { int dx = radius - x, dy = y - (size - 1 - radius); inside = dx * dx + dy * dy <= radius * radius; }
            else if (x >= size - radius && y >= size - radius){ int dx = x - (size - 1 - radius), dy = y - (size - 1 - radius); inside = dx * dx + dy * dy <= radius * radius; }
            tex.SetPixel(x, y, inside ? (Color)w : (Color)c);
        }
        tex.Apply();
        _roundedSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        return _roundedSprite;
    }
}
