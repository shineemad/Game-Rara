using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Mengelola HUD game: navbar (skor, nyawa, progress hari, shout gauge)
/// dan layar intro hari.
///
/// Centang 'buildHUDAtRuntime' untuk membangun navbar gaya baru secara otomatis
/// tanpa perlu setup Canvas manual di Inspector.
///
/// Pemanggilan dari Day1Controller:
///   hudManager.Refresh()
///   hudManager.ShowDayIntro(1)       → layar intro hari
///   hudManager.SetShoutGauge(0–1)    → isi gauge TERIAK
///   hudManager.FlashHeartLost(lives) → animasi kehilangan nyawa
/// </summary>
public class HUDManager : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — Setup Manual (diabaikan jika buildHUDAtRuntime = true)
    // ══════════════════════════════════════════════════════════════════════

    [Header("Nyawa (Hati) — setup manual")]
    public Image[]         heartImages;
    public Sprite          heartFull;
    public Sprite          heartEmpty;

    [Header("Teks — setup manual")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI locationText;
    public TextMeshProUGUI dayText;

    [Header("Lokasi Per Hari")]
    public string[] locationNames =
    {
        "Jalan Menuju Sekolah",
        "Angkot Jurusan Sekolah",
        "Parkiran SMP — Musim Hujan"
    };

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — Navbar Otomatis
    // ══════════════════════════════════════════════════════════════════════

    [Header("Navbar Otomatis (centang = bangun gaya baru)")]
    public bool          buildHUDAtRuntime = true;
    public TMP_FontAsset fontAsset;

    [Tooltip("Sprite hati penuh. Jika kosong, pakai lingkaran merah solid.")]
    public Sprite heartFullSprite;
    [Tooltip("Sprite hati kosong. Jika kosong, pakai lingkaran abu solid.")]
    public Sprite heartEmptySprite;

    [Header("Warna Navbar")]
    public Color panelBgColor     = new Color(0.12f, 0.12f, 0.15f, 0.90f);
    public Color dayActiveColor   = new Color(0.95f, 0.78f, 0.10f, 1f);
    public Color dayInactiveColor = new Color(0.28f, 0.28f, 0.33f, 1f);
    public Color gaugeFillColor   = new Color(0.92f, 0.18f, 0.18f, 1f);
    public Color gaugeEmptyColor  = new Color(0.08f, 0.08f, 0.10f, 1f);

    [Header("Intro Hari")]
    public float introDuration     = 2.8f;
    public float introFadeDuration = 0.5f;

    // ══════════════════════════════════════════════════════════════════════
    // RUNTIME REFS
    // ══════════════════════════════════════════════════════════════════════

    private TextMeshProUGUI   _rScore;
    private Image[]           _rHearts;
    private Image[]           _dayCircles;
    private TextMeshProUGUI[] _dayNums;
    private TextMeshProUGUI[] _dayLabels;
    private Image[]           _dayLines;
    private RectTransform     _gaugeFillRT;

    private CanvasGroup      _introGroup;
    private TextMeshProUGUI  _introTitle;
    private TextMeshProUGUI  _introSub;
    private Coroutine        _introCoroutine;

    // Sprite bersama — dibuat sekali saat runtime
    private static Sprite _sCircle;
    private static Sprite _sRoundRect;

    // Referensi singleton runtime — agar Day1Controller bisa mengaksesnya
    public static HUDManager Instance { get; private set; }

    // ══════════════════════════════════════════════════════════════════════
    // AUTO-SPAWN: navbar muncul otomatis di setiap scene Play tanpa perlu
    // menaruh HUDManager secara manual di scene Hierarchy.
    // BeforeSceneLoad memastikan Instance sudah ada sebelum Awake/Start apapun.
    // ══════════════════════════════════════════════════════════════════════
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoSpawn()
    {
        // Jangan spawn ganda
        if (Instance != null) return;

        var go = new GameObject("[HUDManager]");
        DontDestroyOnLoad(go);
        var hud = go.AddComponent<HUDManager>();
        // buildHUDAtRuntime default = true, jadi navbar langsung dibangun di Start()
        Instance = hud;
    }

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        // Jika ada instance manual di scene, prioritaskan itu; hancurkan duplikat
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (buildHUDAtRuntime)
            BuildHUD();
        Refresh();
    }

    // ══════════════════════════════════════════════════════════════════════
    // PUBLIC API — backward-compatible
    // ══════════════════════════════════════════════════════════════════════

    /// Perbarui seluruh HUD dari GameState.
    public void Refresh()
    {
        if (GameState.Instance == null) return;
        UpdateHearts(GameState.Instance.lives, GameState.Instance.maxLives);
        UpdateScore(GameState.Instance.score);
        UpdateLocation(GameState.Instance.day);
        UpdateDay(GameState.Instance.day);
        UpdateDayProgress(GameState.Instance.day);
    }

    public void UpdateHearts(int current, int max)
    {
        // Manual setup
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null) continue;
            heartImages[i].sprite = (i < current) ? heartFull : heartEmpty;
        }
        // Runtime navbar
        if (_rHearts == null) return;
        for (int i = 0; i < _rHearts.Length; i++)
        {
            if (_rHearts[i] == null) continue;
            bool alive = i < current;
            if (heartFullSprite != null && heartEmptySprite != null)
                _rHearts[i].sprite = alive ? heartFullSprite : heartEmptySprite;
            else
                _rHearts[i].color = alive
                    ? new Color(0.92f, 0.18f, 0.18f, 1f)
                    : new Color(0.32f, 0.32f, 0.36f, 0.75f);
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = $"Skor: {score}";
        if (_rScore   != null) _rScore.text   = $"Skor: {score}";
    }

    public void UpdateLocation(int day)
    {
        if (locationText == null) return;
        int idx = Mathf.Clamp(day - 1, 0, locationNames.Length - 1);
        locationText.text = locationNames[idx];
    }

    public void UpdateDay(int day)
    {
        if (dayText != null) dayText.text = $"Hari {day}";
    }

    /// Perbarui lingkaran progress hari (H1/H2/H3).
    public void UpdateDayProgress(int currentDay)
    {
        if (_dayCircles == null) return;
        for (int i = 0; i < _dayCircles.Length; i++)
        {
            bool active = (i + 1 == currentDay);
            bool done   = (i + 1 < currentDay);

            if (_dayCircles[i] != null)
                _dayCircles[i].color = active ? dayActiveColor : dayInactiveColor;

            if (_dayNums != null && i < _dayNums.Length && _dayNums[i] != null)
                _dayNums[i].color = active
                    ? Color.black
                    : new Color(0.62f, 0.62f, 0.65f, 0.85f);

            if (_dayLabels != null && i < _dayLabels.Length && _dayLabels[i] != null)
                _dayLabels[i].color = active
                    ? dayActiveColor
                    : new Color(0.48f, 0.48f, 0.52f, 0.85f);

            if (_dayLines != null && i < _dayLines.Length && _dayLines[i] != null)
                _dayLines[i].color = done ? dayActiveColor : dayInactiveColor;
        }
    }

    /// Isi shout gauge (0–1). Panggil dari Day1Controller tiap frame.
    public void SetShoutGauge(float value)
    {
        if (_gaugeFillRT == null) return;
        _gaugeFillRT.anchorMax = new Vector2(Mathf.Clamp01(value), 1f);
    }

    /// Tampilkan layar intro "HARI N: ..." dengan fade in/out.
    /// onComplete dipanggil setelah animasi selesai — gunakan untuk mulai game.
    public void ShowDayIntro(int day, System.Action onComplete = null)
    {
        if (_introGroup == null) BuildIntroUI();
        if (_introTitle != null) _introTitle.text = DayTitle(day);
        if (_introSub   != null)
        {
            int idx = Mathf.Clamp(day - 1, 0, locationNames.Length - 1);
            _introSub.text = "📍  " + locationNames[idx];
        }
        if (_introCoroutine != null) StopCoroutine(_introCoroutine);
        _introCoroutine = StartCoroutine(PlayIntroCoroutine(onComplete));
    }

    /// Animasikan kehilangan nyawa.
    public void FlashHeartLost(int newLives)
    {
        UpdateHearts(newLives, GameState.Instance?.maxLives ?? 3);

        if (_rHearts != null && newLives < _rHearts.Length && _rHearts[newLives] != null)
            StartCoroutine(FlashImage(_rHearts[newLives]));

        if (heartImages != null && newLives < heartImages.Length && heartImages[newLives] != null)
            StartCoroutine(FlashImage(heartImages[newLives]));
    }

    // ══════════════════════════════════════════════════════════════════════
    // BUILD NAVBAR
    // ══════════════════════════════════════════════════════════════════════

    void BuildHUD()
    {
        // Pastikan sprite bersama sudah dibuat
        if (_sCircle    == null) _sCircle    = GenCircle(64);
        if (_sRoundRect == null) _sRoundRect = GenRoundedRect(128, 64, 16);

        // Canvas navbar
        var cGO = new GameObject("HUDCanvas_Navbar");
        DontDestroyOnLoad(cGO);
        var cv = cGO.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 900;
        var sc = cGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920f, 1080f);
        sc.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // ── KIRI: Skor + Hati ─────────────────────────────────────────────
        var left = Panel(cGO, "NavLeft", panelBgColor,
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(14f, -12f), new Vector2(340f, 66f));

        // Teks skor
        _rScore = Tmp(left, "Score", "Skor: 0", 26, Color.white);
        _rScore.alignment = TextAlignmentOptions.MidlineLeft;
        _rScore.fontStyle = FontStyles.Bold;
        var scoreRT = _rScore.rectTransform;
        scoreRT.anchorMin = new Vector2(0f,    0.08f);
        scoreRT.anchorMax = new Vector2(0.50f, 0.92f);
        scoreRT.offsetMin = new Vector2(14f, 0f);
        scoreRT.offsetMax = Vector2.zero;

        // Tiga hati
        _rHearts = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            float x0 = 0.54f + i * 0.155f;
            var hRT  = Rect(left, "Heart" + i,
                new Vector2(x0, 0.15f), new Vector2(x0 + 0.14f, 0.85f));
            var hImg = hRT.gameObject.AddComponent<Image>();
            hImg.sprite         = heartFullSprite != null ? heartFullSprite : _sCircle;
            hImg.color          = heartFullSprite != null ? Color.white
                                                          : new Color(0.92f, 0.18f, 0.18f, 1f);
            hImg.preserveAspect = true;
            _rHearts[i]         = hImg;
        }

        // ── TENGAH: Progress Hari (H1 → H2 → H3) ─────────────────────────
        var center = Panel(cGO, "NavCenter", new Color(0, 0, 0, 0),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -6f), new Vector2(360f, 92f));

        float[] nx = { 0.15f, 0.50f, 0.85f };
        _dayCircles = new Image[3];
        _dayNums    = new TextMeshProUGUI[3];
        _dayLabels  = new TextMeshProUGUI[3];
        _dayLines   = new Image[2];

        // Garis konektor (di belakang lingkaran, dibuat lebih dulu)
        for (int i = 0; i < 2; i++)
        {
            var lineRT  = Rect(center, "Line" + i,
                new Vector2(nx[i] + 0.11f, 0.60f),
                new Vector2(nx[i + 1] - 0.11f, 0.72f));
            var lineImg = lineRT.gameObject.AddComponent<Image>();
            lineImg.color = dayInactiveColor;
            _dayLines[i]  = lineImg;
        }

        // Lingkaran hari + nomor + label
        for (int i = 0; i < 3; i++)
        {
            // Lingkaran (point anchor + sizeDelta agar bulat sempurna)
            var circGO = new GameObject("Circle" + i);
            circGO.transform.SetParent(center.transform, false);
            var circRT = circGO.AddComponent<RectTransform>();
            circRT.anchorMin        = circRT.anchorMax = new Vector2(nx[i], 0.72f);
            circRT.pivot            = new Vector2(0.5f, 0.5f);
            circRT.sizeDelta        = new Vector2(50f, 50f);
            circRT.anchoredPosition = Vector2.zero;
            var circImg   = circGO.AddComponent<Image>();
            circImg.sprite = _sCircle;
            circImg.color  = (i == 0) ? dayActiveColor : dayInactiveColor;
            _dayCircles[i] = circImg;

            // Nomor dalam lingkaran
            var numTMP = Tmp(circGO, "Num", (i + 1).ToString(), 22,
                (i == 0) ? Color.black : new Color(0.65f, 0.65f, 0.65f, 0.9f));
            numTMP.alignment = TextAlignmentOptions.Center;
            numTMP.fontStyle = FontStyles.Bold;
            numTMP.rectTransform.anchorMin = Vector2.zero;
            numTMP.rectTransform.anchorMax = Vector2.one;
            numTMP.rectTransform.offsetMin = numTMP.rectTransform.offsetMax = Vector2.zero;
            _dayNums[i] = numTMP;

            // Label "H1/H2/H3" di bawah lingkaran
            var lblTMP = Tmp(center, "Label" + i, "H" + (i + 1), 19,
                (i == 0) ? dayActiveColor : new Color(0.48f, 0.48f, 0.52f, 0.9f));
            lblTMP.alignment = TextAlignmentOptions.Center;
            lblTMP.fontStyle = FontStyles.Bold;
            var lblRT = lblTMP.rectTransform;
            lblRT.anchorMin = new Vector2(nx[i] - 0.10f, 0.00f);
            lblRT.anchorMax = new Vector2(nx[i] + 0.10f, 0.36f);
            lblRT.offsetMin = lblRT.offsetMax = Vector2.zero;
            _dayLabels[i] = lblTMP;
        }

        // ── KANAN: Shout Gauge ────────────────────────────────────────────
        var right = Panel(cGO, "NavRight", panelBgColor,
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-14f, -12f), new Vector2(280f, 66f));

        // Label
        var gaugeLabel = Tmp(right, "GaugeLabel", "📢  TERIAK", 17,
            new Color(1f, 1f, 1f, 0.68f));
        gaugeLabel.alignment = TextAlignmentOptions.MidlineLeft;
        var glRT = gaugeLabel.rectTransform;
        glRT.anchorMin = new Vector2(0f,    0.44f);
        glRT.anchorMax = new Vector2(0.42f, 1.00f);
        glRT.offsetMin = new Vector2(10f, 0f);
        glRT.offsetMax = Vector2.zero;

        // Background bar
        var barBgRT  = Rect(right, "GaugeBg",
            new Vector2(0.42f, 0.20f), new Vector2(0.95f, 0.80f));
        var barBgImg = barBgRT.gameObject.AddComponent<Image>();
        barBgImg.color = gaugeEmptyColor;

        // Fill (anchorMax.x = nilai 0–1 saat runtime)
        var fillGO   = new GameObject("GaugeFill");
        fillGO.transform.SetParent(barBgRT, false);
        _gaugeFillRT = fillGO.AddComponent<RectTransform>();
        _gaugeFillRT.anchorMin = Vector2.zero;
        _gaugeFillRT.anchorMax = new Vector2(0f, 1f);   // mulai kosong
        _gaugeFillRT.offsetMin = _gaugeFillRT.offsetMax = Vector2.zero;
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = gaugeFillColor;
    }

    // ══════════════════════════════════════════════════════════════════════
    // BUILD INTRO SCREEN
    // ══════════════════════════════════════════════════════════════════════

    void BuildIntroUI()
    {
        var introGO = new GameObject("DayIntroCanvas");
        DontDestroyOnLoad(introGO);
        var iCv = introGO.AddComponent<Canvas>();
        iCv.renderMode   = RenderMode.ScreenSpaceOverlay;
        iCv.sortingOrder = 970;
        var iSc = introGO.AddComponent<CanvasScaler>();
        iSc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        iSc.referenceResolution = new Vector2(1920f, 1080f);
        iSc.matchWidthOrHeight  = 0.5f;
        introGO.AddComponent<GraphicRaycaster>();

        _introGroup                = introGO.AddComponent<CanvasGroup>();
        _introGroup.alpha          = 0f;
        _introGroup.blocksRaycasts = false;
        introGO.SetActive(false);

        // ── Overlay gelap penuh layar ─────────────────────────────────────
        var overlayGO = new GameObject("Overlay");
        overlayGO.transform.SetParent(introGO.transform, false);
        var overlayRT = overlayGO.AddComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.offsetMin = overlayRT.offsetMax = Vector2.zero;
        var overlayImg  = overlayGO.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0.04f, 0.82f);

        // ── Garis dekoratif atas ─────────────────────────────────────────
        var lineTop = new GameObject("LineTop");
        lineTop.transform.SetParent(introGO.transform, false);
        var ltRT = lineTop.AddComponent<RectTransform>();
        ltRT.anchorMin = new Vector2(0.08f, 0.68f);
        ltRT.anchorMax = new Vector2(0.92f, 0.685f);
        ltRT.offsetMin = ltRT.offsetMax = Vector2.zero;
        var ltImg = lineTop.AddComponent<Image>();
        ltImg.color = new Color(0.95f, 0.78f, 0.10f, 0.75f);

        // ── Garis dekoratif bawah ─────────────────────────────────────────
        var lineBot = new GameObject("LineBot");
        lineBot.transform.SetParent(introGO.transform, false);
        var lbRT = lineBot.AddComponent<RectTransform>();
        lbRT.anchorMin = new Vector2(0.08f, 0.295f);
        lbRT.anchorMax = new Vector2(0.92f, 0.300f);
        lbRT.offsetMin = lbRT.offsetMax = Vector2.zero;
        var lbImg = lineBot.AddComponent<Image>();
        lbImg.color = new Color(0.95f, 0.78f, 0.10f, 0.75f);

        // ── Judul — teks besar kuning bold ────────────────────────────────
        _introTitle                    = Tmp(introGO, "IntroTitle",
                                             "HARI 1: Jalan Kaki ke Sekolah", 72,
                                             new Color(0.96f, 0.80f, 0.12f, 1f));
        _introTitle.alignment          = TextAlignmentOptions.Center;
        _introTitle.fontStyle          = FontStyles.Bold;
        _introTitle.enableWordWrapping = true;
        var tRT = _introTitle.rectTransform;
        tRT.anchorMin = new Vector2(0.05f, 0.44f);
        tRT.anchorMax = new Vector2(0.95f, 0.68f);
        tRT.offsetMin = tRT.offsetMax = Vector2.zero;

        // ── Sub-judul — lokasi putih ──────────────────────────────────────
        _introSub                    = Tmp(introGO, "IntroSub",
                                           "📍  Jalan Menuju Sekolah", 34, Color.white);
        _introSub.alignment          = TextAlignmentOptions.Center;
        _introSub.enableWordWrapping = true;
        var sRT = _introSub.rectTransform;
        sRT.anchorMin = new Vector2(0.10f, 0.31f);
        sRT.anchorMax = new Vector2(0.90f, 0.44f);
        sRT.offsetMin = sRT.offsetMax = Vector2.zero;

        // ── Hint kecil di bawah ───────────────────────────────────────────
        var hintTmp = Tmp(introGO, "IntroHint", "Bersiaplah...", 22,
                          new Color(1f, 1f, 1f, 0.45f));
        hintTmp.alignment = TextAlignmentOptions.Center;
        var hRT = hintTmp.rectTransform;
        hRT.anchorMin = new Vector2(0.15f, 0.22f);
        hRT.anchorMax = new Vector2(0.85f, 0.30f);
        hRT.offsetMin = hRT.offsetMax = Vector2.zero;
    }

    IEnumerator PlayIntroCoroutine(System.Action onComplete = null)
    {
        if (_introGroup == null) yield break;
        _introGroup.gameObject.SetActive(true);

        // Fade in
        for (float t = 0f; t < introFadeDuration; t += Time.deltaTime)
        {
            _introGroup.alpha = t / introFadeDuration;
            yield return null;
        }
        _introGroup.alpha = 1f;

        yield return new WaitForSeconds(introDuration);

        // Fade out
        for (float t = 0f; t < introFadeDuration; t += Time.deltaTime)
        {
            _introGroup.alpha = 1f - t / introFadeDuration;
            yield return null;
        }
        _introGroup.alpha = 0f;
        _introGroup.gameObject.SetActive(false);

        // Panggil callback setelah animasi selesai
        onComplete?.Invoke();
    }

    // ══════════════════════════════════════════════════════════════════════
    // HELPERS
    // ══════════════════════════════════════════════════════════════════════

    string DayTitle(int day)
    {
        switch (day)
        {
            case 1:  return "HARI 1: Jalan Kaki ke Sekolah";
            case 2:  return "HARI 2: Naik Angkot";
            case 3:  return "HARI 3: Parkiran Sekolah";
            default: return "HARI " + day;
        }
    }

    // Panel dengan background warna (support rounded rect via 9-slice)
    GameObject Panel(GameObject parent, string name, Color bg,
        Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = rt.anchorMax = anchor;
        rt.pivot            = pivot;
        rt.sizeDelta        = size;
        rt.anchoredPosition = pos;
        if (bg.a > 0.01f)
        {
            var img    = go.AddComponent<Image>();
            img.sprite = _sRoundRect;
            img.type   = Image.Type.Sliced;
            img.color  = bg;
        }
        return go;
    }

    // RectTransform kosong (stretch anchor)
    RectTransform Rect(GameObject parent, string name,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }

    // TextMeshProUGUI
    TextMeshProUGUI Tmp(GameObject parent, string name,
        string text, int size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        ApplyFont(tmp);
        tmp.text               = text;
        tmp.fontSize           = size;
        tmp.color              = color;
        tmp.enableWordWrapping = false;
        tmp.overflowMode       = TextOverflowModes.Overflow;
        return tmp;
    }

    void ApplyFont(TextMeshProUGUI tmp)
    {
        TMP_FontAsset f = fontAsset;
        if (f == null) f = TMP_Settings.defaultFontAsset;
        if (f == null) f = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (f != null) tmp.font = f;
    }

    IEnumerator FlashImage(Image img)
    {
        if (img == null) yield break;
        Color orig = img.color;
        for (int i = 0; i < 3; i++)
        {
            img.color = new Color(1f, 0.3f, 0.3f, orig.a);
            yield return new WaitForSeconds(0.15f);
            img.color = orig;
            yield return new WaitForSeconds(0.15f);
        }
    }

    // Buat sprite lingkaran anti-aliased di runtime
    static Sprite GenCircle(int res)
    {
        var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        float r = res * 0.5f;
        for (int y = 0; y < res; y++)
            for (int x = 0; x < res; x++)
            {
                float dx = x - r + 0.5f, dy = y - r + 0.5f;
                float a  = Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
    }

    // Buat sprite rounded rectangle untuk 9-slice panel
    static Sprite GenRoundedRect(int w, int h, int r)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                // Jarak dari pixel ke titik terdekat di persegi "inner" (inset r pixel)
                float cx   = Mathf.Clamp(x, r, w - r - 1);
                float cy   = Mathf.Clamp(y, r, h - r - 1);
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                float a    = Mathf.Clamp01((float)r - dist + 0.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        // Border untuk 9-slice = r pixel di tiap sisi
        return Sprite.Create(tex,
            new Rect(0, 0, w, h),
            new Vector2(0.5f, 0.5f),
            100f, 0,
            SpriteMeshType.FullRect,
            new Vector4(r, r, r, r));
    }
}
