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
    public float holdDuration  = 4f;

    [Header("Audio (opsional)")]
    [Tooltip("SFX saat popup muncul. Kosong = pakai AudioManager.Correct()")]
    public AudioClip sfxUnlock;

    [Header("Sorting")]
    [Tooltip("Sorting order Canvas. Default 1050 — di atas dialog (999) & prolog (1000).")]
    public int sortingOrder = 1050;

    [Header("Menu Persisten (daftar lencana)")]
    [Tooltip("Tampilkan tombol 🏆 yang selalu ada untuk membuka daftar semua lencana yang sudah diraih.")]
    public bool   tampilkanMenuPersisten = true;
    [Tooltip("Ukuran tombol 🏆 di pojok kanan-atas (px, referensi 1920x1080).")]
    public Vector2 tombolMenuSize   = new Vector2(84f, 84f);
    [Tooltip("Jarak tombol 🏆 dari sudut kanan-atas (px).")]
    public Vector2 tombolMenuMargin = new Vector2(24f, 22f);
    [Tooltip("Jaga tombol 🏆 tetap di dalam Safe Area (notch / rounded corner HP).")]
    public bool   hormatiSafeArea  = true;
    [Tooltip("Warna tombol 🏆.")]
    public Color  tombolMenuWarna  = new Color(0.96f, 0.65f, 0.10f, 0.95f);
    [Tooltip("Warna panel daftar lencana.")]
    public Color  menuPanelWarna   = new Color(0.10f, 0.08f, 0.04f, 0.97f);
    [Tooltip("Judul panel daftar lencana.")]
    public string menuJudul        = "🏆 LENCANA KAMU";
    [Tooltip("Teks saat belum ada lencana.")]
    [TextArea(2, 4)]
    public string menuKosongTeks   = "Belum ada lencana.\nMain terus & ambil keputusan AMAN untuk meraihnya!";

    // ── runtime ───────────────────────────────────────────────────────────
    private Canvas       _canvas;
    private RectTransform _container;
    private Sprite       _roundedRectSprite;
    private Queue<(string text, Sprite icon)> _queue = new Queue<(string, Sprite)>();
    private int          _activeCount = 0;

    // ── runtime menu persisten ────────────────────────────────────────────
    private TextMeshProUGUI _menuCountLabel;   // angka jumlah lencana pada tombol 🏆
    private GameObject      _menuPanel;        // panel daftar lencana (null = tertutup)
    private RectTransform   _menuButtonRT;
    private Rect            _lastSafeArea;     // cache untuk deteksi perubahan Safe Area

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

    void Start()
    {
        // Bangun canvas + tombol 🏆 sejak awal agar menu lencana selalu tersedia.
        if (tampilkanMenuPersisten) EnsureCanvas();
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
        // Bila tombol menu persisten ada di pojok kanan-atas, geser popup turun
        // agar tidak menumpuk dengan tombol 🏆.
        float topOffset = marginTop;
        if (tampilkanMenuPersisten)
            topOffset = Mathf.Max(marginTop, tombolMenuMargin.y + tombolMenuSize.y + 12f);
        _container.anchoredPosition = new Vector2(-marginRight, -topOffset);
        _container.sizeDelta = new Vector2(popupWidth, popupHeight * 4);

        // Tombol menu persisten 🏆 (selalu tampil, buka daftar lencana).
        if (tampilkanMenuPersisten)
            BuildMenuButton(cGO.transform);
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

        RefreshMenuCount();

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
    // MENU PERSISTEN (daftar lencana)
    // ══════════════════════════════════════════════════════════════════════

    // Tombol 🏆 yang selalu tampil di pojok kanan-atas. Klik → buka/tutup daftar.
    void BuildMenuButton(Transform canvasTf)
    {
        var btnGO = new GameObject("MenuLencanaButton");
        btnGO.transform.SetParent(canvasTf, false);
        _menuButtonRT = btnGO.AddComponent<RectTransform>();
        _menuButtonRT.anchorMin = new Vector2(1f, 1f);
        _menuButtonRT.anchorMax = new Vector2(1f, 1f);
        _menuButtonRT.pivot     = new Vector2(1f, 1f);
        _menuButtonRT.sizeDelta = tombolMenuSize;
        _menuButtonRT.anchoredPosition = new Vector2(-tombolMenuMargin.x, -tombolMenuMargin.y);

        var img = btnGO.AddComponent<Image>();
        img.sprite = GetRoundedRectSprite();
        img.type   = Image.Type.Sliced;
        img.color  = tombolMenuWarna;
        var ol = btnGO.AddComponent<Outline>();
        ol.effectColor    = borderColor;
        ol.effectDistance = new Vector2(2f, -2f);

        var btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(ToggleMenu);

        // Ikon 🏆
        var icoGO = new GameObject("Ikon");
        icoGO.transform.SetParent(btnGO.transform, false);
        var icoRT = icoGO.AddComponent<RectTransform>();
        icoRT.anchorMin = new Vector2(0f, 0.30f);
        icoRT.anchorMax = new Vector2(1f, 1f);
        icoRT.offsetMin = Vector2.zero; icoRT.offsetMax = Vector2.zero;
        var icoTMP = icoGO.AddComponent<TextMeshProUGUI>();
        ApplyFont(icoTMP);
        icoTMP.text      = "🏆";
        icoTMP.fontSize  = (int)(tombolMenuSize.y * 0.45f);
        icoTMP.color     = Color.white;
        icoTMP.alignment = TextAlignmentOptions.Center;
        icoTMP.raycastTarget = false;

        // Label jumlah lencana
        var lblGO = new GameObject("Jumlah");
        lblGO.transform.SetParent(btnGO.transform, false);
        var lblRT = lblGO.AddComponent<RectTransform>();
        lblRT.anchorMin = new Vector2(0f, 0f);
        lblRT.anchorMax = new Vector2(1f, 0.34f);
        lblRT.offsetMin = Vector2.zero; lblRT.offsetMax = Vector2.zero;
        _menuCountLabel = lblGO.AddComponent<TextMeshProUGUI>();
        ApplyFont(_menuCountLabel);
        _menuCountLabel.fontSize  = (int)(tombolMenuSize.y * 0.26f);
        _menuCountLabel.color     = new Color(0.2f, 0.1f, 0f, 1f);
        _menuCountLabel.fontStyle = FontStyles.Bold;
        _menuCountLabel.alignment = TextAlignmentOptions.Center;
        _menuCountLabel.raycastTarget = false;

        RefreshMenuCount();
        ApplyMenuButtonSafeArea();
    }

    // Geser tombol 🏆 masuk ke dalam Safe Area (notch / rounded corner) dengan
    // menambahkan inset kanan & atas ke margin dasar. Dipanggil saat build dan
    // saat Safe Area berubah (orientasi / device).
    void ApplyMenuButtonSafeArea()
    {
        if (_menuButtonRT == null) return;
        float rightInset = 0f, topInset = 0f;
        if (hormatiSafeArea)
        {
            Rect sa = Screen.safeArea;
            rightInset = Mathf.Max(0f, Screen.width  - sa.xMax);
            topInset   = Mathf.Max(0f, Screen.height - sa.yMax);
        }
        _lastSafeArea = Screen.safeArea;
        _menuButtonRT.anchoredPosition = new Vector2(
            -(tombolMenuMargin.x + rightInset),
            -(tombolMenuMargin.y + topInset));
    }

    void Update()
    {
        // Refresh posisi tombol 🏆 bila Safe Area berubah (mis. rotasi layar).
        if (_menuButtonRT != null && Screen.safeArea != _lastSafeArea)
            ApplyMenuButtonSafeArea();
    }

    // Perbarui angka jumlah lencana di tombol 🏆.
    void RefreshMenuCount()
    {
        if (_menuCountLabel == null) return;
        int n = (GameState.Instance != null && GameState.Instance.achievements != null)
            ? GameState.Instance.achievements.Count : 0;
        _menuCountLabel.text = n.ToString();
    }

    // Buka/tutup panel daftar lencana.
    public void ToggleMenu()
    {
        AudioManager.Instance?.Click();
        if (_menuPanel != null) { Destroy(_menuPanel); _menuPanel = null; return; }
        EnsureCanvas();
        BuildMenuPanel();
    }

    void BuildMenuPanel()
    {
        // Overlay gelap full-screen (klik di luar panel = tutup)
        _menuPanel = new GameObject("MenuLencanaPanel");
        _menuPanel.transform.SetParent(_canvas.transform, false);
        var ovRT = _menuPanel.AddComponent<RectTransform>();
        ovRT.anchorMin = Vector2.zero; ovRT.anchorMax = Vector2.one;
        ovRT.offsetMin = Vector2.zero; ovRT.offsetMax = Vector2.zero;
        var ovImg = _menuPanel.AddComponent<Image>();
        ovImg.color = new Color(0f, 0f, 0f, 0.6f);
        var ovBtn = _menuPanel.AddComponent<Button>();
        ovBtn.transition = Selectable.Transition.None;
        ovBtn.onClick.AddListener(ToggleMenu);

        // Kartu tengah
        var card = new GameObject("Card");
        card.transform.SetParent(_menuPanel.transform, false);
        var cardRT = card.AddComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.5f, 0.5f);
        cardRT.anchorMax = new Vector2(0.5f, 0.5f);
        cardRT.pivot     = new Vector2(0.5f, 0.5f);
        cardRT.sizeDelta = new Vector2(760f, 720f);
        var cardImg = card.AddComponent<Image>();
        cardImg.sprite = GetRoundedRectSprite();
        cardImg.type   = Image.Type.Sliced;
        cardImg.color  = menuPanelWarna;
        var cardOl = card.AddComponent<Outline>();
        cardOl.effectColor    = borderColor;
        cardOl.effectDistance = new Vector2(3f, -3f);
        // Cegah klik di kartu menutup panel (jangan tembus ke overlay)
        var cardBlocker = card.AddComponent<Button>();
        cardBlocker.transition = Selectable.Transition.None;

        // Judul
        var judulGO = new GameObject("Judul");
        judulGO.transform.SetParent(card.transform, false);
        var jRT = judulGO.AddComponent<RectTransform>();
        jRT.anchorMin = new Vector2(0f, 1f); jRT.anchorMax = new Vector2(1f, 1f);
        jRT.pivot = new Vector2(0.5f, 1f);
        jRT.offsetMin = new Vector2(24f, -96f); jRT.offsetMax = new Vector2(-24f, -20f);
        var jTMP = judulGO.AddComponent<TextMeshProUGUI>();
        ApplyFont(jTMP);
        jTMP.text      = menuJudul;
        jTMP.fontSize  = 38;
        jTMP.color     = borderColor;
        jTMP.fontStyle = FontStyles.Bold;
        jTMP.alignment = TextAlignmentOptions.Center;

        var achs = (GameState.Instance != null) ? GameState.Instance.achievements : null;
        bool kosong = (achs == null || achs.Count == 0);

        if (kosong)
        {
            var emptyGO = new GameObject("Kosong");
            emptyGO.transform.SetParent(card.transform, false);
            var eRT = emptyGO.AddComponent<RectTransform>();
            eRT.anchorMin = new Vector2(0f, 0f); eRT.anchorMax = new Vector2(1f, 1f);
            eRT.offsetMin = new Vector2(40f, 110f); eRT.offsetMax = new Vector2(-40f, -110f);
            var eTMP = emptyGO.AddComponent<TextMeshProUGUI>();
            ApplyFont(eTMP);
            eTMP.text      = menuKosongTeks;
            eTMP.fontSize  = 26;
            eTMP.color     = new Color(1f, 1f, 1f, 0.8f);
            eTMP.alignment = TextAlignmentOptions.Center;
            eTMP.textWrappingMode = TextWrappingModes.Normal;
        }
        else
        {
            // Area scrollable berisi daftar lencana
            var scrollGO = new GameObject("Scroll");
            scrollGO.transform.SetParent(card.transform, false);
            var scRT = scrollGO.AddComponent<RectTransform>();
            scRT.anchorMin = new Vector2(0f, 0f); scRT.anchorMax = new Vector2(1f, 1f);
            scRT.offsetMin = new Vector2(28f, 108f); scRT.offsetMax = new Vector2(-28f, -104f);
            var sr = scrollGO.AddComponent<ScrollRect>();
            sr.horizontal = false; sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 28f;

            var vpGO = new GameObject("Viewport");
            vpGO.transform.SetParent(scrollGO.transform, false);
            var vpRT = vpGO.AddComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero; vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = Vector2.zero; vpRT.offsetMax = Vector2.zero;
            vpRT.pivot = new Vector2(0f, 1f);
            var vpImg = vpGO.AddComponent<Image>();
            vpImg.color = new Color(0f, 0f, 0f, 0.001f);
            vpGO.AddComponent<RectMask2D>();
            sr.viewport = vpRT;

            var listGO = new GameObject("Content");
            listGO.transform.SetParent(vpGO.transform, false);
            var listRT = listGO.AddComponent<RectTransform>();
            listRT.anchorMin = new Vector2(0f, 1f); listRT.anchorMax = new Vector2(1f, 1f);
            listRT.pivot = new Vector2(0.5f, 1f);
            var vlg = listGO.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true; vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
            vlg.spacing = 12f; vlg.padding = new RectOffset(6, 6, 6, 6);
            var fit = listGO.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.content = listRT;

            for (int i = 0; i < achs.Count; i++)
                BuildMenuRow(listGO.transform, achs[i]);
        }

        // Tombol tutup
        var tutupGO = new GameObject("BtnTutup");
        tutupGO.transform.SetParent(card.transform, false);
        var tRT = tutupGO.AddComponent<RectTransform>();
        tRT.anchorMin = new Vector2(0.5f, 0f); tRT.anchorMax = new Vector2(0.5f, 0f);
        tRT.pivot = new Vector2(0.5f, 0f);
        tRT.sizeDelta = new Vector2(280f, 64f);
        tRT.anchoredPosition = new Vector2(0f, 24f);
        var tImg = tutupGO.AddComponent<Image>();
        tImg.sprite = GetRoundedRectSprite();
        tImg.type   = Image.Type.Sliced;
        tImg.color  = new Color(0.20f, 0.55f, 0.30f, 1f);
        var tOl = tutupGO.AddComponent<Outline>();
        tOl.effectColor = new Color(1f, 1f, 1f, 0.30f);
        tOl.effectDistance = new Vector2(2f, -2f);
        var tBtn = tutupGO.AddComponent<Button>();
        tBtn.onClick.AddListener(ToggleMenu);
        var tLblGO = new GameObject("Label");
        tLblGO.transform.SetParent(tutupGO.transform, false);
        var tLblRT = tLblGO.AddComponent<RectTransform>();
        tLblRT.anchorMin = Vector2.zero; tLblRT.anchorMax = Vector2.one;
        tLblRT.offsetMin = Vector2.zero; tLblRT.offsetMax = Vector2.zero;
        var tLbl = tLblGO.AddComponent<TextMeshProUGUI>();
        ApplyFont(tLbl);
        tLbl.text = "TUTUP";
        tLbl.fontSize = 28;
        tLbl.color = Color.white;
        tLbl.fontStyle = FontStyles.Bold;
        tLbl.alignment = TextAlignmentOptions.Center;
    }

    // Satu baris lencana di panel daftar.
    void BuildMenuRow(Transform parent, string nama)
    {
        var row = new GameObject("Lencana");
        row.transform.SetParent(parent, false);
        row.AddComponent<RectTransform>();
        var rowImg = row.AddComponent<Image>();
        rowImg.sprite = GetRoundedRectSprite();
        rowImg.type   = Image.Type.Sliced;
        rowImg.color  = new Color(1f, 1f, 1f, 0.06f);
        var rowLE = row.AddComponent<LayoutElement>();
        rowLE.minHeight = 76f;
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = true; hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = true;
        hlg.spacing = 14f;
        hlg.padding = new RectOffset(16, 16, 8, 8);

        var icoGO = new GameObject("Ikon");
        icoGO.transform.SetParent(row.transform, false);
        icoGO.AddComponent<RectTransform>();
        var icoLE = icoGO.AddComponent<LayoutElement>();
        icoLE.preferredWidth = 56f; icoLE.flexibleWidth = 0f;
        var icoTMP = icoGO.AddComponent<TextMeshProUGUI>();
        ApplyFont(icoTMP);
        icoTMP.text = "🏅";
        icoTMP.fontSize = 34;
        icoTMP.color = new Color(1f, 0.85f, 0.3f, 1f);
        icoTMP.alignment = TextAlignmentOptions.Center;

        var txtGO = new GameObject("Nama");
        txtGO.transform.SetParent(row.transform, false);
        txtGO.AddComponent<RectTransform>();
        var txtLE = txtGO.AddComponent<LayoutElement>();
        txtLE.flexibleWidth = 1f;
        var txtTMP = txtGO.AddComponent<TextMeshProUGUI>();
        ApplyFont(txtTMP);
        txtTMP.text = nama;
        txtTMP.fontSize = 26;
        txtTMP.color = Color.white;
        txtTMP.fontStyle = FontStyles.Bold;
        txtTMP.alignment = TextAlignmentOptions.MidlineLeft;
        txtTMP.textWrappingMode = TextWrappingModes.Normal;
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
