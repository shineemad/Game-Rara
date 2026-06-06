using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// LaporButtonUI — popup tombol "📞 LAPOR sekarang!" yang muncul setelah
/// pemain memilih jawaban BAHAYA. Memberikan kesempatan recovery skor.
///
/// Cara pakai:
///   LaporButtonUI.Show(onLapor: () => { ... }, onSkip: () => { ... });
///
/// Setup opsional (drag GameObject ke scene + tambah komponen):
///   • buttonSprite      → sprite tombol custom
///   • phoneIconSprite   → ikon telepon di depan teks
///   • headerText        → teks judul (default "Kamu masih bisa LAPOR!")
///   • laporButtonText   → label tombol lapor
///   • skipButtonText    → label tombol skip
///   • recoveryScore     → poin recovery (default 250 = SCORE_LAPOR/2)
///   • countdownSeconds  → countdown otomatis skip (0 = tanpa countdown)
/// </summary>
public class LaporButtonUI : MonoBehaviour
{
    public static LaporButtonUI Instance { get; private set; }

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR
    // ══════════════════════════════════════════════════════════════════════

    [Header("Sprite (opsional)")]
    [Tooltip("Sprite latar panel. Kosong = panel rounded rect putih.")]
    public Sprite panelSprite;
    [Tooltip("Sprite tombol LAPOR. Kosong = solid hijau.")]
    public Sprite buttonSprite;
    [Tooltip("Sprite tombol SKIP. Kosong = outline abu.")]
    public Sprite skipButtonSprite;
    [Tooltip("Ikon telepon di kiri tombol. Kosong = pakai emoji 📞.")]
    public Sprite phoneIconSprite;

    [Header("Warna")]
    public Color panelColor       = new Color(1f, 1f, 1f, 0.97f);
    public Color overlayColor     = new Color(0f, 0f, 0f, 0.55f);
    public Color laporButtonColor = new Color(0.15f, 0.68f, 0.38f, 1f);
    public Color skipButtonColor  = new Color(0.55f, 0.55f, 0.55f, 1f);
    public Color headerColor      = new Color(0.85f, 0.30f, 0.24f, 1f);
    public Color bodyColor        = new Color(0.20f, 0.20f, 0.20f, 1f);

    [Header("Teks")]
    public string headerText      = "Kamu masih bisa LAPOR!";
    public string bodyText        = "Ceritakan kejadian ini ke orang tua, guru, atau polisi (110). Tidak pernah terlambat untuk minta tolong.";
    public string laporButtonText = "📞 LAPOR Sekarang";
    public string skipButtonText  = "Lewati";

    [Header("Skor & Behaviour")]
    [Tooltip("Poin recovery saat klik LAPOR. Default = SCORE_LAPOR / 2 (250).")]
    public int   recoveryScore     = 250;
    [Tooltip("Countdown auto-skip (detik). 0 = tanpa countdown.")]
    public float countdownSeconds  = 8f;
    [Tooltip("Nama achievement saat klik LAPOR.")]
    public string laporAchievement = "Berani Lapor";

    [Header("Font (opsional)")]
    public TMP_FontAsset fontAsset;

    [Header("Sorting")]
    [Tooltip("Sorting order Canvas. Default 1020 — di atas dialog (999).")]
    public int sortingOrder = 1020;

    // ── runtime ──────────────────────────────────────────────────────────
    private Canvas _canvas;
    private GameObject _activePanel;
    private Sprite _roundedRect;

    // ══════════════════════════════════════════════════════════════════════
    // PUBLIC API
    // ══════════════════════════════════════════════════════════════════════

    /// Tampilkan popup. onLapor dipanggil jika klik LAPOR (dan setelah skor masuk),
    /// onSkip dipanggil jika lewat / countdown habis.
    public static void Show(System.Action onLapor, System.Action onSkip = null)
    {
        EnsureInstance();
        Instance.ShowInternal(onLapor, onSkip);
    }

    static void EnsureInstance()
    {
        if (Instance != null) return;
        var existing = FindFirstObjectByType<LaporButtonUI>();
        if (existing != null) { Instance = existing; return; }
        var go = new GameObject("[LaporButtonUI]");
        Instance = go.AddComponent<LaporButtonUI>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else if (Instance != this) { Destroy(gameObject); return; }
    }

    // ══════════════════════════════════════════════════════════════════════
    // BUILDER
    // ══════════════════════════════════════════════════════════════════════
    void EnsureCanvas()
    {
        if (_canvas != null) return;
        var cGO = new GameObject("LaporCanvas");
        DontDestroyOnLoad(cGO);
        _canvas = cGO.AddComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = sortingOrder;
        var sc = cGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920f, 1080f);
        sc.matchWidthOrHeight = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();
    }

    void ShowInternal(System.Action onLapor, System.Action onSkip)
    {
        EnsureCanvas();
        if (_activePanel != null) Destroy(_activePanel);

        // ── Overlay gelap fullscreen ─────────────────────────────────────
        _activePanel = new GameObject("LaporPanel");
        _activePanel.transform.SetParent(_canvas.transform, false);
        var overlayRT = _activePanel.AddComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.offsetMin = overlayRT.offsetMax = Vector2.zero;
        var overlayImg = _activePanel.AddComponent<Image>();
        overlayImg.color = overlayColor;
        overlayImg.raycastTarget = true;

        // ── Panel tengah ──────────────────────────────────────────────────
        var card = new GameObject("Card");
        card.transform.SetParent(_activePanel.transform, false);
        var cardRT = card.AddComponent<RectTransform>();
        cardRT.anchorMin = cardRT.anchorMax = new Vector2(0.5f, 0.5f);
        cardRT.pivot     = new Vector2(0.5f, 0.5f);
        cardRT.sizeDelta = new Vector2(640f, 360f);
        var cardImg = card.AddComponent<Image>();
        if (panelSprite != null) { cardImg.sprite = panelSprite; cardImg.type = Image.Type.Sliced; cardImg.color = Color.white; }
        else { cardImg.sprite = GetRoundedRect(); cardImg.type = Image.Type.Sliced; cardImg.color = panelColor; }
        var cardOut = card.AddComponent<Outline>();
        cardOut.effectColor = new Color(0f, 0f, 0f, 0.18f);
        cardOut.effectDistance = new Vector2(3f, -3f);

        // Header
        var header = CreateText(card.transform, "Header", headerText, 32, headerColor, FontStyles.Bold);
        var hRT = header.rectTransform;
        hRT.anchorMin = new Vector2(0f, 1f); hRT.anchorMax = new Vector2(1f, 1f);
        hRT.pivot = new Vector2(0.5f, 1f);
        hRT.offsetMin = new Vector2(24f, -90f);
        hRT.offsetMax = new Vector2(-24f, -18f);
        header.alignment = TextAlignmentOptions.Center;

        // Body
        var body = CreateText(card.transform, "Body", bodyText, 20, bodyColor, FontStyles.Normal);
        var bRT = body.rectTransform;
        bRT.anchorMin = new Vector2(0f, 0.3f); bRT.anchorMax = new Vector2(1f, 0.7f);
        bRT.offsetMin = new Vector2(28f, 0f);
        bRT.offsetMax = new Vector2(-28f, 0f);
        body.alignment = TextAlignmentOptions.Center;
        body.textWrappingMode = TextWrappingModes.Normal;

        // Countdown teks (opsional)
        TextMeshProUGUI cdTMP = null;
        if (countdownSeconds > 0f)
        {
            cdTMP = CreateText(card.transform, "Countdown", "", 16,
                new Color(0.4f, 0.4f, 0.4f, 1f), FontStyles.Italic);
            var cdRT = cdTMP.rectTransform;
            cdRT.anchorMin = new Vector2(0f, 0f); cdRT.anchorMax = new Vector2(1f, 0f);
            cdRT.pivot = new Vector2(0.5f, 0f);
            cdRT.offsetMin = new Vector2(0f, 4f);
            cdRT.offsetMax = new Vector2(0f, 22f);
            cdTMP.alignment = TextAlignmentOptions.Center;
        }

        // Tombol LAPOR (utama, kiri)
        var btnLapor = BuildButton(card.transform, laporButtonText, true);
        var lRT = btnLapor.transform as RectTransform;
        lRT.anchorMin = new Vector2(0.5f, 0f); lRT.anchorMax = new Vector2(0.5f, 0f);
        lRT.pivot     = new Vector2(0.5f, 0f);
        lRT.anchoredPosition = new Vector2(-130f, 38f);
        lRT.sizeDelta = new Vector2(240f, 64f);

        // Tombol SKIP (kanan)
        var btnSkip = BuildButton(card.transform, skipButtonText, false);
        var sRT = btnSkip.transform as RectTransform;
        sRT.anchorMin = new Vector2(0.5f, 0f); sRT.anchorMax = new Vector2(0.5f, 0f);
        sRT.pivot     = new Vector2(0.5f, 0f);
        sRT.anchoredPosition = new Vector2(130f, 38f);
        sRT.sizeDelta = new Vector2(200f, 64f);

        bool resolved = false;

        btnLapor.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (resolved) return;
            resolved = true;
            AudioManager.Instance?.PlayLapor();
            // Beri skor recovery sebagai kategori LAPOR
            GameState.Instance?.AddChoice(GameState.Instance.day,
                "Lapor ke orang tua / polisi", "LAPOR", recoveryScore);
            HUDManager.Instance?.ShowScorePopup(recoveryScore, "AMAN");
            HUDManager.Instance?.Refresh();
            if (!string.IsNullOrEmpty(laporAchievement))
                GameState.Instance?.EarnAchievement(laporAchievement);
            Close();
            onLapor?.Invoke();
        });

        btnSkip.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (resolved) return;
            resolved = true;
            AudioManager.Instance?.Click();
            Close();
            onSkip?.Invoke();
        });

        if (countdownSeconds > 0f)
            StartCoroutine(CountdownRoutine(cdTMP, btnSkip.GetComponent<Button>()));

        // Slide-in scale
        cardRT.localScale = Vector3.one * 0.85f;
        StartCoroutine(PopIn(cardRT));
    }

    IEnumerator PopIn(RectTransform rt)
    {
        float t = 0f, dur = 0.25f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dur);
            // Ease out back
            float c1 = 1.70158f, c3 = c1 + 1f;
            float e = 1f + c3 * Mathf.Pow(k - 1, 3) + c1 * Mathf.Pow(k - 1, 2);
            rt.localScale = Vector3.one * Mathf.LerpUnclamped(0.85f, 1f, e);
            yield return null;
        }
        rt.localScale = Vector3.one;
    }

    IEnumerator CountdownRoutine(TextMeshProUGUI tmp, Button skipBtn)
    {
        float t = countdownSeconds;
        while (t > 0f && _activePanel != null)
        {
            if (tmp != null) tmp.text = $"Otomatis lewati dalam {Mathf.CeilToInt(t)} detik...";
            t -= Time.unscaledDeltaTime;
            yield return null;
        }
        if (_activePanel != null && skipBtn != null) skipBtn.onClick.Invoke();
    }

    void Close()
    {
        if (_activePanel != null) Destroy(_activePanel);
        _activePanel = null;
    }

    // ══════════════════════════════════════════════════════════════════════
    // UI HELPERS
    // ══════════════════════════════════════════════════════════════════════
    GameObject BuildButton(Transform parent, string label, bool isPrimary)
    {
        var go = new GameObject(isPrimary ? "BtnLapor" : "BtnSkip");
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var img = go.AddComponent<Image>();
        Sprite spr = isPrimary ? buttonSprite : skipButtonSprite;
        if (spr != null) { img.sprite = spr; img.type = Image.Type.Sliced; img.color = Color.white; }
        else { img.sprite = GetRoundedRect(); img.type = Image.Type.Sliced; img.color = isPrimary ? laporButtonColor : skipButtonColor; }

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
        colors.pressedColor     = new Color(0.85f, 0.85f, 0.85f, 1f);
        btn.colors = colors;

        // Label TMP
        var txt = CreateText(go.transform, "Label", label,
            isPrimary ? 22 : 20, Color.white,
            isPrimary ? FontStyles.Bold : FontStyles.Normal);
        var tRT = txt.rectTransform;
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = new Vector2(12f, 0f);
        tRT.offsetMax = new Vector2(-12f, 0f);
        txt.alignment = TextAlignmentOptions.Center;

        // Ikon telepon (jika ada & primary)
        if (isPrimary && phoneIconSprite != null)
        {
            var iGO = new GameObject("Icon");
            iGO.transform.SetParent(go.transform, false);
            var iRT = iGO.AddComponent<RectTransform>();
            iRT.anchorMin = new Vector2(0f, 0.5f); iRT.anchorMax = new Vector2(0f, 0.5f);
            iRT.pivot = new Vector2(0f, 0.5f);
            iRT.sizeDelta = new Vector2(36f, 36f);
            iRT.anchoredPosition = new Vector2(8f, 0f);
            var iImg = iGO.AddComponent<Image>();
            iImg.sprite = phoneIconSprite;
            iImg.preserveAspect = true;
        }

        return go;
    }

    TextMeshProUGUI CreateText(Transform parent, string name, string content,
                                int size, Color color, FontStyles style)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        TMP_FontAsset f = fontAsset ?? TMP_Settings.defaultFontAsset;
        if (f != null) tmp.font = f;
        tmp.text      = content;
        tmp.fontSize  = size;
        tmp.color     = color;
        tmp.fontStyle = style;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        return tmp;
    }

    Sprite GetRoundedRect()
    {
        if (_roundedRect != null) return _roundedRect;
        const int w = 64, h = 32, radius = 14;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            int dx = x < radius ? radius - x : x > w - radius ? x - (w - radius) : 0;
            int dy = y < radius ? radius - y : y > h - radius ? y - (h - radius) : 0;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            float a = Mathf.Clamp01(radius - dist);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        tex.Apply();
        _roundedRect = Sprite.Create(tex, new Rect(0, 0, w, h),
            new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius));
        return _roundedRect;
    }
}
