using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// AchievementPopup — popup lencana yang slide-in dari kanan.
///
/// Cara pakai dari script lain:
///   AchievementPopup.Show("Tolak Orang Asing");
///   AchievementPopup.Show("Lapor ke Polisi", customIcon);
///
/// Auto-dibuat saat pertama dipanggil (singleton lazy). Tidak perlu setup
/// di scene kecuali ingin custom sprite / warna / font.
///
/// Setup opsional (drag GameObject ke scene + tambah komponen ini):
///   • badgeSprite    → sprite lencana (background popup)
///   • iconDefault    → ikon trophy default
///   • fontAsset      → TMP font Indonesia (Poppins, Inter, dll)
///   • slideDuration  → durasi animasi slide-in/out
///   • holdDuration   → berapa detik popup tampil sebelum slide-out
/// </summary>
public class AchievementPopup : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // SINGLETON
    // ══════════════════════════════════════════════════════════════════════
    public static AchievementPopup Instance { get; private set; }

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — semua opsional, ada default agar langsung berfungsi
    // ══════════════════════════════════════════════════════════════════════

    [Header("Sprite (opsional — drag PNG ke sini)")]
    [Tooltip("Sprite latar lencana. Kosong = pakai rounded rect oranye solid.")]
    public Sprite badgeSprite;
    [Tooltip("Ikon default (trophy/medali). Kosong = pakai emoji 🏆 sebagai teks.")]
    public Sprite iconDefault;

    [Header("Warna")]
    public Color  bgColor       = new Color(0.96f, 0.65f, 0.10f, 0.95f);  // oranye emas
    public Color  borderColor   = new Color(1f, 0.95f, 0.6f, 1f);
    public Color  titleColor    = new Color(0.20f, 0.10f, 0f, 1f);        // coklat tua
    public Color  subtitleColor = new Color(0.10f, 0.06f, 0f, 0.85f);
    public Color  iconTint      = Color.white;

    [Header("Teks")]
    public string title          = "🏆 LENCANA BARU!";
    public int    titleFontSize  = 28;
    public int    subFontSize    = 24;

    [Header("Font (opsional)")]
    public TMP_FontAsset fontAsset;

    [Header("Ukuran & Posisi")]
    [Tooltip("Lebar popup (pixel referensi 1920x1080)")]
    public float popupWidth   = 460f;
    public float popupHeight  = 110f;
    [Tooltip("Jarak dari tepi kanan saat tampil")]
    public float marginRight  = 24f;
    [Tooltip("Jarak dari tepi atas")]
    public float marginTop    = 80f;
    [Tooltip("Jarak antar popup jika banyak antrian")]
    public float gapBetween   = 8f;

    [Header("Animasi")]
    [Tooltip("Durasi slide-in/out (detik)")]
    public float slideDuration = 0.35f;
    [Tooltip("Berapa detik popup tampil sebelum slide-out")]
    public float holdDuration  = 2.5f;

    [Header("Audio (opsional)")]
    [Tooltip("SFX saat popup muncul. Kosong = pakai AudioManager.Correct()")]
    public AudioClip sfxUnlock;

    [Header("Sorting")]
    [Tooltip("Sorting order Canvas. Default 1050 — di atas dialog (999) & prolog (1000).")]
    public int sortingOrder = 1050;

    // ── runtime ───────────────────────────────────────────────────────────
    private Canvas       _canvas;
    private RectTransform _container;
    private Sprite       _roundedRectSprite;
    private Queue<(string text, Sprite icon)> _queue = new Queue<(string, Sprite)>();
    private int          _activeCount = 0;

    // ══════════════════════════════════════════════════════════════════════
    // PUBLIC API
    // ══════════════════════════════════════════════════════════════════════

    /// Tampilkan popup achievement. Auto-buat singleton + canvas jika belum ada.
    public static void Show(string achievementName, Sprite customIcon = null)
    {
        EnsureInstance();
        Instance._queue.Enqueue((achievementName, customIcon));
        Instance.TryProcessQueue();
    }

    static void EnsureInstance()
    {
        if (Instance != null)
        {
            // GO singleton bisa saja ter-disable (mis. ikut ke-nonaktif saat transisi
            // hari, atau di-reparent ke Day2_Root yang sempat nonaktif). Lepaskan ke
            // root + DontDestroyOnLoad + aktifkan rantai supaya activeInHierarchy=true
            // dan StartCoroutine tidak gagal diam-diam.
            PastikanAktifDanRoot(Instance.gameObject);
            return;
        }

        var existing = FindFirstObjectByType<AchievementPopup>(FindObjectsInactive.Include);
        if (existing != null)
        {
            Instance = existing;
            PastikanAktifDanRoot(Instance.gameObject);
            return;
        }

        var go = new GameObject("[AchievementPopup]");
        Instance = go.AddComponent<AchievementPopup>();
        DontDestroyOnLoad(go);
    }

    // Lepaskan GO dari parent (hindari ikut nonaktif saat parent di-SetActive(false)),
    // jadikan objek root persisten, lalu pastikan aktif.
    static void PastikanAktifDanRoot(GameObject go)
    {
        if (go.transform.parent != null)
            go.transform.SetParent(null, true);
        DontDestroyOnLoad(go);
        if (!go.activeSelf) go.SetActive(true);
    }

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else if (Instance != this) { Destroy(gameObject); return; }
    }

    // ══════════════════════════════════════════════════════════════════════
    // CANVAS BUILDER (lazy)
    // ══════════════════════════════════════════════════════════════════════
    void EnsureCanvas()
    {
        if (_canvas != null) return;

        var cGO = new GameObject("AchievementCanvas");
        DontDestroyOnLoad(cGO);
        _canvas = cGO.AddComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = sortingOrder;

        var sc = cGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920f, 1080f);
        sc.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // Container kanan-atas (anchor top-right)
        var contGO = new GameObject("Container");
        contGO.transform.SetParent(cGO.transform, false);
        _container = contGO.AddComponent<RectTransform>();
        _container.anchorMin = new Vector2(1f, 1f);
        _container.anchorMax = new Vector2(1f, 1f);
        _container.pivot     = new Vector2(1f, 1f);
        _container.anchoredPosition = new Vector2(-marginRight, -marginTop);
        _container.sizeDelta = new Vector2(popupWidth, popupHeight * 4);
    }

    // ══════════════════════════════════════════════════════════════════════
    // QUEUE PROCESSING
    // ══════════════════════════════════════════════════════════════════════
    void TryProcessQueue()
    {
        EnsureCanvas();
        // Jaring pengaman: kalau GO masih inactive (rantai parent belum aktif),
        // StartCoroutine akan gagal diam-diam. Pastikan aktif dulu.
        if (!gameObject.activeInHierarchy) PastikanAktifDanRoot(gameObject);
        while (_queue.Count > 0)
        {
            var (text, icon) = _queue.Dequeue();
            StartCoroutine(ShowOne(text, icon));
        }
    }

    IEnumerator ShowOne(string text, Sprite customIcon)
    {
        int slot = _activeCount;
        _activeCount++;

        // Buat popup GO
        var popupGO = BuildPopup(text, customIcon);
        var rt = popupGO.GetComponent<RectTransform>();
        rt.SetParent(_container, false);
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot     = new Vector2(1f, 1f);

        float yOffset = -slot * (popupHeight + gapBetween);
        Vector2 onScreen  = new Vector2(0f, yOffset);
        Vector2 offScreen = new Vector2(popupWidth + marginRight + 40f, yOffset);

        rt.anchoredPosition = offScreen;

        // SFX
        if (sfxUnlock != null && AudioManager.Instance != null && AudioManager.Instance.sfxSource != null)
            AudioManager.Instance.sfxSource.PlayOneShot(sfxUnlock);
        else
            AudioManager.Instance?.Correct();

        // Slide-in
        yield return AnimatePos(rt, offScreen, onScreen, slideDuration, EaseOutBack);

        // Hold
        yield return new WaitForSeconds(holdDuration);

        // Slide-out
        yield return AnimatePos(rt, onScreen, offScreen, slideDuration * 0.8f, EaseInQuad);

        Destroy(popupGO);
        _activeCount--;
    }

    // ══════════════════════════════════════════════════════════════════════
    // POPUP BUILDER
    // ══════════════════════════════════════════════════════════════════════
    GameObject BuildPopup(string achievementName, Sprite customIcon)
    {
        var go = new GameObject("AchievementPopup");
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(popupWidth, popupHeight);

        // Background image
        var bgImg = go.AddComponent<Image>();
        if (badgeSprite != null)
        {
            bgImg.sprite = badgeSprite;
            bgImg.type   = Image.Type.Sliced;
            bgImg.color  = Color.white;
        }
        else
        {
            bgImg.sprite = GetRoundedRectSprite();
            bgImg.type   = Image.Type.Sliced;
            bgImg.color  = bgColor;
        }

        // Outline border (selalu, agar terlihat menonjol)
        var outline = go.AddComponent<Outline>();
        outline.effectColor    = borderColor;
        outline.effectDistance = new Vector2(2f, -2f);

        // ── Icon kiri ─────────────────────────────────────────────────────
        var iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(go.transform, false);
        var iconRT = iconGO.AddComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0f, 0.5f);
        iconRT.anchorMax = new Vector2(0f, 0.5f);
        iconRT.pivot     = new Vector2(0f, 0.5f);
        iconRT.sizeDelta = new Vector2(popupHeight - 20f, popupHeight - 20f);
        iconRT.anchoredPosition = new Vector2(12f, 0f);

        Sprite useIcon = customIcon != null ? customIcon : iconDefault;
        if (useIcon != null)
        {
            var iconImg = iconGO.AddComponent<Image>();
            iconImg.sprite         = useIcon;
            iconImg.color          = iconTint;
            iconImg.preserveAspect = true;
        }
        else
        {
            // Fallback: teks emoji
            var tmp = iconGO.AddComponent<TextMeshProUGUI>();
            ApplyFont(tmp);
            tmp.text      = "🏆";
            tmp.fontSize  = (int)(popupHeight * 0.55f);
            tmp.color     = iconTint;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        // ── Teks kanan (title + subtitle) ────────────────────────────────
        float textLeft = popupHeight + 4f;

        var titleGO = new GameObject("Title");
        titleGO.transform.SetParent(go.transform, false);
        var titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 0.55f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.offsetMin = new Vector2(textLeft, 0f);
        titleRT.offsetMax = new Vector2(-10f,     -6f);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        ApplyFont(titleTMP);
        titleTMP.text      = title;
        titleTMP.fontSize  = titleFontSize;
        titleTMP.color     = titleColor;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.BottomLeft;

        var subGO = new GameObject("Subtitle");
        subGO.transform.SetParent(go.transform, false);
        var subRT = subGO.AddComponent<RectTransform>();
        subRT.anchorMin = new Vector2(0f, 0f);
        subRT.anchorMax = new Vector2(1f, 0.55f);
        subRT.offsetMin = new Vector2(textLeft, 6f);
        subRT.offsetMax = new Vector2(-10f,     0f);
        var subTMP = subGO.AddComponent<TextMeshProUGUI>();
        ApplyFont(subTMP);
        subTMP.text             = achievementName;
        subTMP.fontSize         = subFontSize;
        subTMP.color            = subtitleColor;
        subTMP.alignment        = TextAlignmentOptions.TopLeft;
        subTMP.textWrappingMode = TextWrappingModes.Normal;

        return go;
    }

    // ══════════════════════════════════════════════════════════════════════
    // ANIMASI
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator AnimatePos(RectTransform rt, Vector2 from, Vector2 to, float dur, System.Func<float, float> ease)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dur);
            rt.anchoredPosition = Vector2.LerpUnclamped(from, to, ease(k));
            yield return null;
        }
        rt.anchoredPosition = to;
    }

    static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    static float EaseInQuad(float t) => t * t;

    // ══════════════════════════════════════════════════════════════════════
    // HELPERS
    // ══════════════════════════════════════════════════════════════════════
    void ApplyFont(TextMeshProUGUI tmp)
    {
        TMP_FontAsset f = fontAsset
            ?? TMP_Settings.defaultFontAsset
            ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (f != null) tmp.font = f;
    }

    Sprite GetRoundedRectSprite()
    {
        if (_roundedRectSprite != null) return _roundedRectSprite;

        const int w = 64, h = 32, radius = 14;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            // Hitung jarak ke sudut terdekat
            int dx = x < radius ? radius - x
                   : x > w - radius ? x - (w - radius)
                   : 0;
            int dy = y < radius ? radius - y
                   : y > h - radius ? y - (h - radius)
                   : 0;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            float a = Mathf.Clamp01(radius - dist);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        tex.Apply();
        _roundedRectSprite = Sprite.Create(tex, new Rect(0, 0, w, h),
            new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius));
        return _roundedRectSprite;
    }
}
