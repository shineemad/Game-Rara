using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// KataSaktiGlossary — Tombol "?" melayang + popup penjelasan 3 Kata Sakti
/// (TIDAK \u2192 PERGI \u2192 CERITA) yang bisa dibuka anak kapan saja.
///
/// Tidak perlu setup di scene. Panggil statis:
///   KataSaktiGlossary.EnsureButton();   // tampilkan tombol ?
///   KataSaktiGlossary.Hide();           // sembunyikan tombol + popup
///
/// Kata sakti yang SUDAH dikuasai (lihat GameState) ditandai dengan centang
/// supaya anak melihat progresnya.
/// </summary>
public class KataSaktiGlossary : MonoBehaviour
{
    public static KataSaktiGlossary Instance { get; private set; }

    [System.Serializable]
    public class KataData
    {
        public string kata;
        [TextArea(2, 4)] public string penjelasan;
        public Color warna = Color.white;
    }

    [Header("Tombol ? (pojok)")]
    public string tombolTeks = "?";
    public Color  tombolWarna = new Color(0.20f, 0.55f, 0.85f, 1f);
    public Vector2 tombolUkuran = new Vector2(64f, 64f);
    public float  tombolMarginKanan = 24f;
    public float  tombolMarginAtas  = 24f;

    [Header("Popup")]
    public string judulPopup = "3 KATA SAKTI MENJAGA DIRI";
    public Color  warnaJudul = new Color(1f, 0.85f, 0.25f, 1f);
    public Color  warnaPanel = new Color(0.06f, 0.09f, 0.14f, 0.98f);
    public Color  warnaBorder = new Color(1f, 0.85f, 0.25f, 1f);
    public Color  warnaOverlay = new Color(0f, 0f, 0f, 0.72f);
    public Vector2 ukuranPopup = new Vector2(900f, 620f);

    [Header("Isi Kata Sakti (CUSTOMIZABLE)")]
    public KataData[] kataList = new KataData[]
    {
        new KataData {
            kata = "TIDAK",
            penjelasan = "Berani berkata TIDAK saat ada yang membuatmu tak nyaman. Kamu berhak menolak \u2014 tubuhmu milikmu sendiri.",
            warna = new Color(0.91f, 0.30f, 0.24f, 1f)
        },
        new KataData {
            kata = "PERGI",
            penjelasan = "Segera PERGI / menjauh dari situasi bahaya. Cari tempat ramai atau orang dewasa yang bisa dipercaya.",
            warna = new Color(0.98f, 0.78f, 0.18f, 1f)
        },
        new KataData {
            kata = "CERITA",
            penjelasan = "CERITA ke orang dewasa yang kamu percaya: orang tua, guru, atau telepon KPAI 021-31901556. Jangan simpan sendiri!",
            warna = new Color(0.20f, 0.78f, 0.40f, 1f)
        }
    };

    [Header("Font (opsional)")]
    public TMP_FontAsset fontAsset;

    [Header("Sorting")]
    public int sortingTombol = 945;
    public int sortingPopup  = 1010;

    // ── runtime ───────────────────────────────────────────────────────────
    private Canvas     _tombolCanvas;
    private GameObject _popupCanvasGO;
    private Sprite     _roundedSprite;

    // ══════════════════════════════════════════════════════════════════════
    public static void EnsureButton()
    {
        EnsureInstance();
        Instance.gameObject.SetActive(true);
        if (Instance._tombolCanvas == null) Instance.BuildTombol();
        else Instance._tombolCanvas.gameObject.SetActive(true);
    }

    public static void Hide()
    {
        if (Instance == null) return;
        if (Instance._tombolCanvas != null) Instance._tombolCanvas.gameObject.SetActive(false);
        Instance.TutupPopup();
    }

    static void EnsureInstance()
    {
        if (Instance != null) return;
        var go = new GameObject("[KataSaktiGlossary]");
        Instance = go.AddComponent<KataSaktiGlossary>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }
    }

    // ══════════════════════════════════════════════════════════════════════
    void BuildTombol()
    {
        var canvasGO = new GameObject("Glossary_TombolCanvas");
        canvasGO.transform.SetParent(transform, false);
        _tombolCanvas = canvasGO.AddComponent<Canvas>();
        _tombolCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _tombolCanvas.sortingOrder = sortingTombol;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        var btnGO = new GameObject("TombolTanya");
        btnGO.transform.SetParent(canvasGO.transform, false);
        var img = btnGO.AddComponent<Image>();
        img.sprite = GetRoundedSprite(); img.color = tombolWarna; img.type = Image.Type.Sliced;
        var outl = btnGO.AddComponent<Outline>();
        outl.effectColor = new Color(1f, 1f, 1f, 0.4f); outl.effectDistance = new Vector2(2f, -2f);
        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f); rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.sizeDelta = tombolUkuran;
        rt.anchoredPosition = new Vector2(-tombolMarginKanan, -tombolMarginAtas);
        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(BukaPopup);

        var lab = BuatTeks(btnGO.transform, "Label", tombolTeks, 34, Color.white, FontStyles.Bold);
        lab.alignment = TextAlignmentOptions.Center;
        var lrt = lab.rectTransform;
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
    }

    void BukaPopup()
    {
        AudioManager.Instance?.Click();
        if (_popupCanvasGO != null) return;
        BuildPopup();
    }

    void TutupPopup()
    {
        if (_popupCanvasGO != null) Destroy(_popupCanvasGO);
        _popupCanvasGO = null;
    }

    void BuildPopup()
    {
        _popupCanvasGO = new GameObject("Glossary_PopupCanvas");
        _popupCanvasGO.transform.SetParent(transform, false);
        var canvas = _popupCanvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingPopup;
        var scaler = _popupCanvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        _popupCanvasGO.AddComponent<GraphicRaycaster>();

        // Overlay (klik untuk tutup)
        var ov = new GameObject("Overlay");
        ov.transform.SetParent(_popupCanvasGO.transform, false);
        var ovImg = ov.AddComponent<Image>();
        ovImg.color = warnaOverlay;
        var ovRT = ov.GetComponent<RectTransform>();
        ovRT.anchorMin = Vector2.zero; ovRT.anchorMax = Vector2.one;
        ovRT.offsetMin = Vector2.zero; ovRT.offsetMax = Vector2.zero;
        var ovBtn = ov.AddComponent<Button>();
        ovBtn.transition = Selectable.Transition.None;
        ovBtn.onClick.AddListener(TutupPopup);

        // Panel
        var panel = new GameObject("Panel");
        panel.transform.SetParent(_popupCanvasGO.transform, false);
        var pImg = panel.AddComponent<Image>();
        pImg.sprite = GetRoundedSprite(); pImg.color = warnaPanel; pImg.type = Image.Type.Sliced;
        var outl = panel.AddComponent<Outline>();
        outl.effectColor = warnaBorder; outl.effectDistance = new Vector2(3f, -3f);
        var pRT = panel.GetComponent<RectTransform>();
        pRT.anchorMin = new Vector2(0.5f, 0.5f); pRT.anchorMax = new Vector2(0.5f, 0.5f);
        pRT.pivot = new Vector2(0.5f, 0.5f);
        pRT.sizeDelta = ukuranPopup;

        // Judul
        var jud = BuatTeks(panel.transform, "Judul", "\uD83D\uDEE1  " + judulPopup, 30, warnaJudul, FontStyles.Bold);
        jud.alignment = TextAlignmentOptions.Center;
        var jRT = jud.rectTransform;
        jRT.anchorMin = new Vector2(0f, 1f); jRT.anchorMax = new Vector2(1f, 1f);
        jRT.pivot = new Vector2(0.5f, 1f);
        jRT.offsetMin = new Vector2(20f, -70f); jRT.offsetMax = new Vector2(-20f, -16f);

        // List kata sakti (vertical layout)
        var list = new GameObject("List");
        list.transform.SetParent(panel.transform, false);
        var listRT = list.AddComponent<RectTransform>();
        listRT.anchorMin = new Vector2(0f, 0f); listRT.anchorMax = new Vector2(1f, 1f);
        listRT.offsetMin = new Vector2(30f, 96f); listRT.offsetMax = new Vector2(-30f, -80f);
        var vlg = list.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 14f; vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true; vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

        var gs = GameState.Instance;
        foreach (var k in kataList)
        {
            bool dikuasai = gs != null && (
                (k.kata.ToUpperInvariant() == "TIDAK"  && gs.usedTidak)  ||
                (k.kata.ToUpperInvariant() == "PERGI"  && gs.usedPergi)  ||
                (k.kata.ToUpperInvariant() == "CERITA" && gs.usedCerita));
            BuatBarisKata(list.transform, k, dikuasai);
        }

        // Tombol tutup
        var btnGO = new GameObject("TombolTutup");
        btnGO.transform.SetParent(panel.transform, false);
        var bImg = btnGO.AddComponent<Image>();
        bImg.sprite = GetRoundedSprite(); bImg.color = new Color(0.20f, 0.62f, 0.86f, 1f); bImg.type = Image.Type.Sliced;
        var bRT = btnGO.GetComponent<RectTransform>();
        bRT.anchorMin = new Vector2(0.5f, 0f); bRT.anchorMax = new Vector2(0.5f, 0f);
        bRT.pivot = new Vector2(0.5f, 0f);
        bRT.sizeDelta = new Vector2(260f, 56f);
        bRT.anchoredPosition = new Vector2(0f, 18f);
        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = bImg;
        btn.onClick.AddListener(TutupPopup);
        var lab = BuatTeks(btnGO.transform, "Label", "\u2713  Mengerti", 22, Color.white, FontStyles.Bold);
        lab.alignment = TextAlignmentOptions.Center;
        var lrt = lab.rectTransform;
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
    }

    void BuatBarisKata(Transform parent, KataData k, bool dikuasai)
    {
        var row = new GameObject("Kata_" + k.kata);
        row.transform.SetParent(parent, false);
        var img = row.AddComponent<Image>();
        img.sprite = GetRoundedSprite();
        img.color = new Color(k.warna.r, k.warna.g, k.warna.b, 0.16f);
        img.type = Image.Type.Sliced;
        var outl = row.AddComponent<Outline>();
        outl.effectColor = new Color(k.warna.r, k.warna.g, k.warna.b, 0.7f);
        outl.effectDistance = new Vector2(2f, -2f);
        var le = row.AddComponent<LayoutElement>();
        le.preferredHeight = 130f; le.flexibleWidth = 1f;

        // Badge kata (kiri)
        string badge = (dikuasai ? "\u2713 " : "") + k.kata;
        var kataTmp = BuatTeks(row.transform, "Kata", badge, 30, k.warna, FontStyles.Bold);
        kataTmp.alignment = TextAlignmentOptions.MidlineLeft;
        var kRT = kataTmp.rectTransform;
        kRT.anchorMin = new Vector2(0f, 0f); kRT.anchorMax = new Vector2(0f, 1f);
        kRT.pivot = new Vector2(0f, 0.5f);
        kRT.sizeDelta = new Vector2(220f, 0f);
        kRT.anchoredPosition = new Vector2(18f, 0f);

        // Penjelasan (kanan)
        var desc = BuatTeks(row.transform, "Desc", k.penjelasan, 19,
            new Color(1f, 1f, 0.95f, dikuasai ? 1f : 0.92f), FontStyles.Normal);
        desc.alignment = TextAlignmentOptions.MidlineLeft;
        var dRT = desc.rectTransform;
        dRT.anchorMin = new Vector2(0f, 0f); dRT.anchorMax = new Vector2(1f, 1f);
        dRT.offsetMin = new Vector2(250f, 12f); dRT.offsetMax = new Vector2(-18f, -12f);
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
        tmp.textWrappingMode = TextWrappingModes.Normal; tmp.raycastTarget = false;
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
