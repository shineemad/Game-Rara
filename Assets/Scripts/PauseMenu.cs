using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// PauseMenu — menu pause yang muncul di pojok kiri bawah,
/// berisi tips keselamatan + nomor darurat (110 / 129).
///
/// Tekan tombol "P" / Escape (configurable) untuk toggle.
///
/// MODE A — UI dibuat di Editor (drag ref).
/// MODE B — Programatik fallback.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — UI REFERENSI
    // ══════════════════════════════════════════════════════════════════════
    [Header("── UI REFERENSI (Editor-Built) ──")]
    public GameObject       uiRootRef;
    public Image            overlayImageRef;
    public GameObject       panelRootRef;
    public TextMeshProUGUI  titleTMPRef;
    public TextMeshProUGUI  tipsTMPRef;
    public TextMeshProUGUI  emergencyTMPRef;
    public Button           btnResumeRef;
    public TextMeshProUGUI  btnResumeLabelRef;
    public Button           btnMenuRef;
    public TextMeshProUGUI  btnMenuLabelRef;

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — INPUT
    // ══════════════════════════════════════════════════════════════════════
    [Header("── INPUT ──")]
    public KeyCode toggleKey       = KeyCode.Escape;
    public KeyCode altToggleKey    = KeyCode.P;
    public bool    pauseTimeScale  = true;

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — MOBILE
    // ══════════════════════════════════════════════════════════════════════
    [Header("── MOBILE ──")]
    [Tooltip("Tampilkan tombol Pause on-screen saat di mobile.")]
    public bool   showMobilePauseButton = true;
    [Tooltip("Sprite tombol pause on-screen (opsional). Kosongkan untuk tombol bulat default.")]
    public Sprite mobilePauseButtonSprite;
    [Tooltip("Ukuran tombol pause on-screen (px)")]
    public Vector2 mobilePauseButtonSize    = new Vector2(90f, 90f);

    public enum ScreenCorner { TopLeft, TopRight, BottomLeft, BottomRight }
    [Tooltip("Sudut layar tempat tombol pause ditempatkan. Pilih sudut yang tidak ditempati HUD lain (skor / meter SUARA / Hari).")]
    public ScreenCorner mobilePauseButtonCorner = ScreenCorner.BottomRight;
    [Tooltip("Jarak tombol pause dari sudut layar (px)")]
    public Vector2 mobilePauseButtonMargin  = new Vector2(30f, 30f);

    [Tooltip("Hormati Safe Area (notch / punch hole) di mobile.")]
    public bool   useSafeArea              = true;
    [Tooltip("Minimum touch target untuk semua tombol (px). Standar Material 48, Apple 44. Default 96 untuk anak.")]
    public float  minTouchTargetPx         = 96f;

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — KONTEN TEKS
    // ══════════════════════════════════════════════════════════════════════
    [Header("── KONTEN TEKS ──")]
    public string titleText     = "JEDA";
    [TextArea(3, 8)]
    public string tipsText      = "• Selalu cerita ke orang tua jika ada yang aneh.\n• Hindari menerima sesuatu dari orang asing.\n• Pilih jalan ramai daripada jalan sepi.";
    [TextArea(2, 4)]
    public string emergencyText = "📞 Polisi: 110\n📞 KPAI: 129";
    public string resumeLabel   = "LANJUTKAN";
    public string menuLabel     = "MENU UTAMA";

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — KONTEN KHUSUS HARI 2 (3 KATA SAKTI)
    // ══════════════════════════════════════════════════════════════════════
    [Header("── KONTEN HARI 2 (3 Kata Sakti) ──")]
    [Tooltip("Saat berada di Day 2, ganti isi panel JEDA dengan 3 Kata Sakti (TIDAK → PERGI → CERITA).")]
    public bool   gunakanKontenKataSaktiHari2 = true;
    public string titleTextHari2 = "3 KATA SAKTI MENJAGA DIRI";
    [TextArea(4, 10)]
    public string tipsTextHari2 =
        "<b><color=#E84D3D>TIDAK</color></b>  :  Berani berkata tidak saat ada yang membuatmu tak nyaman. Kamu berhak menolak; tubuhmu milikmu sendiri.\n\n" +
        "<b><color=#F29D12>PERGI</color></b>  :  Segera menjauh dari situasi bahaya. Cari tempat ramai atau orang dewasa yang bisa dipercaya.\n\n" +
        "<b><color=#26AD61>CERITA</color></b>  :  Ceritakan ke orang dewasa yang kamu percaya: orang tua, guru, atau telepon KPAI 021-31901556. Jangan simpan sendiri!";
    [TextArea(2, 4)]
    public string emergencyTextHari2 = "";

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — WARNA (Mode B)
    // ══════════════════════════════════════════════════════════════════════
    [Header("── WARNA (Mode B / Fallback) ──")]
    public Color panelBgColor   = new Color(0.10f, 0.10f, 0.15f, 0.96f);
    public Color borderColor    = new Color(1f, 0.85f, 0.1f, 1f);
    public Color titleColor     = new Color(1f, 0.85f, 0.1f, 1f);
    public Color tipsColor      = Color.white;
    public Color emergencyColor = new Color(1f, 0.6f, 0.6f, 1f);
    public Color resumeColor    = new Color(0.15f, 0.60f, 0.20f, 1f);
    public Color menuColor      = new Color(0.5f, 0.5f, 0.5f, 1f);
    public Color btnTextColor   = Color.white;

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — FONT
    // ══════════════════════════════════════════════════════════════════════
    [Header("── FONT (opsional) ──")]
    public TMP_FontAsset fontAsset;
    public TMP_FontAsset titleFontAsset;
    public TMP_FontAsset bodyFontAsset;
    public TMP_FontAsset buttonFontAsset;

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — KUSTOMISASI TEKS
    // ══════════════════════════════════════════════════════════════════════
    [Header("── KUSTOMISASI TEKS (centang untuk override) ──")]
    public bool overrideTextStyle = false;

    [Header("   Judul")]
    public float                titleFontSize        = 44f;
    public FontStyles           titleFontStyle       = FontStyles.Bold;
    public TextAlignmentOptions titleAlignment       = TextAlignmentOptions.Center;
    public bool                 titleUseOutline      = false;
    public Color                titleOutlineColor    = Color.black;
    [Range(0f, 1f)] public float titleOutlineWidth   = 0.2f;

    [Header("   Tips")]
    public float                tipsFontSize         = 26f;
    public FontStyles           tipsFontStyle        = FontStyles.Normal;
    public TextAlignmentOptions tipsAlignment        = TextAlignmentOptions.TopLeft;
    [Range(-50f, 100f)] public float tipsLineSpacing = 8f;
    public bool                 tipsUseOutline       = false;
    public Color                tipsOutlineColor     = Color.black;
    [Range(0f, 1f)] public float tipsOutlineWidth    = 0.15f;

    [Header("   Nomor Darurat")]
    public float                emergencyFontSize    = 28f;
    public FontStyles           emergencyFontStyle   = FontStyles.Bold;
    public TextAlignmentOptions emergencyAlignment   = TextAlignmentOptions.Center;
    public bool                 emergencyUseOutline  = false;
    public Color                emergencyOutlineColor = Color.black;
    [Range(0f, 1f)] public float emergencyOutlineWidth = 0.15f;

    [Header("   Tombol")]
    public float                buttonFontSize       = 32f;
    public FontStyles           buttonFontStyle      = FontStyles.Bold;
    public TextAlignmentOptions buttonAlignment      = TextAlignmentOptions.Center;
    public bool                 buttonUseOutline     = false;
    public Color                buttonOutlineColor   = Color.black;
    [Range(0f, 1f)] public float buttonOutlineWidth  = 0.2f;

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — SPRITES
    // ══════════════════════════════════════════════════════════════════════
    [Header("── SPRITES (opsional, drag image di sini) ──")]
    public Sprite panelBgSprite;
    public Sprite resumeButtonSprite;
    public Sprite menuButtonSprite;
    public Sprite overlayBgSprite;
    public Image.Type spriteImageType = Image.Type.Sliced;
    public bool tintSpriteWithColor   = false;

    [Header("   Ukuran Sprite (centang untuk override)")]
    public bool    overrideSpriteSize = false;
    public Vector2 panelSize          = new Vector2(520f, 700f);
    public Vector2 panelAnchoredPos   = new Vector2(0f, 0f);   // tengah layar default
    [Range(0.1f, 5f)] public float panelScale = 1f;

    public bool overlayStretchFullscreen = true;
    public Vector2 overlaySize            = new Vector2(1920f, 1080f);

    [Range(0.1f, 5f)] public float resumeButtonScale = 1f;
    [Range(0.1f, 5f)] public float menuButtonScale   = 1f;

    [Tooltip("Jika true: panel di-dock ke pojok kiri bawah. Jika false (default): panel di tengah layar pakai panelAnchoredPos.")]
    public bool dockBottomLeft = false;
    [Tooltip("Margin dari pojok kiri bawah (px) — hanya berlaku jika dockBottomLeft=true")]
    public Vector2 dockMargin = new Vector2(20f, 20f);

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — POSISI TOMBOL & TEKS
    // ══════════════════════════════════════════════════════════════════════
    [Header("── POSISI TOMBOL (centang untuk override) ──")]
    public bool    overrideButtonLayout    = false;
    public Vector2 resumeButtonAnchoredPos = new Vector2(0f, -250f);
    public Vector2 resumeButtonSize        = new Vector2(420f, 80f);
    public Vector2 menuButtonAnchoredPos   = new Vector2(0f, -340f);
    public Vector2 menuButtonSize          = new Vector2(420f, 80f);

    [Header("── POSISI TEKS (centang untuk override) ──")]
    public bool    overrideTextLayout = false;
    public Vector2 titleAnchoredPos     = new Vector2(0f, 280f);
    public Vector2 titleSize            = new Vector2(480f, 70f);
    public Vector2 tipsAnchoredPos      = new Vector2(0f, 100f);
    public Vector2 tipsSize             = new Vector2(460f, 280f);
    public Vector2 emergencyAnchoredPos = new Vector2(0f, -120f);
    public Vector2 emergencySize        = new Vector2(460f, 100f);

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — LIVE EDIT
    // ══════════════════════════════════════════════════════════════════════
    [Header("── LIVE EDIT (saat Play) ──")]
    public bool liveEdit               = true;
    public bool previewPanelInPlay     = false;
    public bool autoDisableLayoutGroup = true;
    public bool debugLog               = false;

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — EVENTS
    // ══════════════════════════════════════════════════════════════════════
    [Header("── EVENTS ──")]
    public UnityEngine.Events.UnityEvent onResume;
    public UnityEngine.Events.UnityEvent onGoMenu;
    public string mainMenuSceneName = "MainMenu";

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — PENGATURAN (Settings)
    // ══════════════════════════════════════════════════════════════════════
    [Header("── PENGATURAN (Settings) ──")]
    [Tooltip("Tampilkan tombol ⚙ Pengaturan di panel jeda (volume, font, aksesibilitas).")]
    public bool   tampilkanPengaturan = true;
    public string settingsButtonLabel = "⚙ PENGATURAN";
    public string settingsTitleText   = "PENGATURAN";
    public Color  settingsColor       = new Color(0.20f, 0.40f, 0.65f, 1f);

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — KELUAR (Quit)
    // ══════════════════════════════════════════════════════════════════════
    [Header("── KELUAR (Quit) ──")]
    [Tooltip("Tampilkan tombol ✖ Keluar di panel jeda (dengan konfirmasi).")]
    public bool   tampilkanKeluar      = true;
    public string keluarKonfirmasiJudul = "KELUAR GAME?";
    [TextArea(2, 4)]
    public string keluarKonfirmasiPesan = "Yakin ingin keluar dari game?\nProgres yang belum selesai tidak tersimpan.";
    public string keluarBatalLabel     = "BATAL";
    public string keluarYaLabel        = "KELUAR";
    public Color  keluarColor          = new Color(0.84f, 0.27f, 0.22f, 1f);

    // ══════════════════════════════════════════════════════════════════════
    // INTERNAL
    // ══════════════════════════════════════════════════════════════════════
    Canvas canvas;
    GameObject uiRoot;
    GameObject panelRoot;
    GameObject mobileBtnRoot;
    Button     mobilePauseBtn;
    TextMeshProUGUI _lencanaBadgeLabel;   // angka jumlah lencana yang menyatu di tombol pause
    bool usingEditorRefs = false;
    bool isOpen = false;
    float prevTimeScale = 1f;
    bool builtOnce = false;

    GameObject settingsPanel;   // overlay Pengaturan (null = belum dibangun / tertutup)
    GameObject keluarPanel;     // overlay Konfirmasi Keluar (null = tertutup)

    void Start()
    {
        GameSettings.Init();   // muat & terapkan preferensi tersimpan (volume, dll)
        if (uiRootRef != null && panelRootRef != null && btnResumeRef != null)
        {
            usingEditorRefs = true;
            SetupEditorRefs();
        }
        else
        {
            usingEditorRefs = false;
            BuildUI();
        }
        ClosePanel();
        BuildMobilePauseButton();
        builtOnce = true;
    }

    void Update()
    {
        if (!builtOnce) return;
        if (Application.isPlaying)
        {
            // Saat Game Over tampil: blokir jeda agar hanya tombol Main Lagi & Keluar aktif.
            if (GameOverScreen.IsShowing) return;

            // Keyboard: Esc / P
            if (Input.GetKeyDown(toggleKey) || Input.GetKeyDown(altToggleKey))
                Toggle();

            // Android back button → toggle pause
            if (Application.platform == RuntimePlatform.Android &&
                Input.GetKeyDown(KeyCode.Escape))
                Toggle();

            UpdateMobileButtonVisibility();
            ApplySafeAreaIfNeeded();

            if (previewPanelInPlay) ForcePreview();
            if (liveEdit)           ReapplyAllCustomization();
        }
    }

    // ══════ Mobile pause button on-screen ══════
    void BuildMobilePauseButton()
    {
        if (canvas == null) return;          // butuh canvas dari BuildUI / cari di scene
        if (!showMobilePauseButton) return;

        mobileBtnRoot = new GameObject("MobilePauseButton");
        mobileBtnRoot.transform.SetParent(canvas.transform, false);
        var rt = mobileBtnRoot.AddComponent<RectTransform>();
        ApplyCornerAnchor(rt);
        rt.sizeDelta = mobilePauseButtonSize;

        var img = mobileBtnRoot.AddComponent<Image>();
        if (mobilePauseButtonSprite != null) { img.sprite = mobilePauseButtonSprite; img.type = Image.Type.Sliced; }
        else                                  img.color = new Color(0.1f, 0.1f, 0.15f, 0.92f);

        mobilePauseBtn = mobileBtnRoot.AddComponent<Button>();
        mobilePauseBtn.onClick.AddListener(Toggle);

        // Ikon pause = dua batang vertikal yang DIGAMBAR via Image (tidak
        // bergantung pada glyph font, jadi selalu tampil jelas di semua platform).
        if (mobilePauseButtonSprite == null)
        {
            // Bingkai lingkaran tipis kuning agar tombol jelas terlihat sebagai kontrol.
            var ringGO = new GameObject("Ring");
            ringGO.transform.SetParent(mobileBtnRoot.transform, false);
            var ringRT = ringGO.AddComponent<RectTransform>();
            ringRT.anchorMin = Vector2.zero; ringRT.anchorMax = Vector2.one;
            ringRT.offsetMin = new Vector2(4f, 4f); ringRT.offsetMax = new Vector2(-4f, -4f);
            var ringImg = ringGO.AddComponent<Image>();
            ringImg.color = new Color(1f, 0.85f, 0.1f, 0.9f);

            var innerGO = new GameObject("Inner");
            innerGO.transform.SetParent(ringGO.transform, false);
            var innerRT = innerGO.AddComponent<RectTransform>();
            innerRT.anchorMin = Vector2.zero; innerRT.anchorMax = Vector2.one;
            innerRT.offsetMin = new Vector2(4f, 4f); innerRT.offsetMax = new Vector2(-4f, -4f);
            var innerImg = innerGO.AddComponent<Image>();
            innerImg.color = new Color(0.1f, 0.1f, 0.15f, 1f);

            // Dua batang putih (simbol jeda).
            float barW = mobilePauseButtonSize.x * 0.13f;
            float gapH = mobilePauseButtonSize.x * 0.10f;
            float barH = mobilePauseButtonSize.y * 0.40f;
            for (int s = -1; s <= 1; s += 2)
            {
                var barGO = new GameObject(s < 0 ? "BarKiri" : "BarKanan");
                barGO.transform.SetParent(innerGO.transform, false);
                var barRT = barGO.AddComponent<RectTransform>();
                barRT.anchorMin = barRT.anchorMax = new Vector2(0.5f, 0.5f);
                barRT.pivot      = new Vector2(0.5f, 0.5f);
                barRT.sizeDelta  = new Vector2(barW, barH);
                barRT.anchoredPosition = new Vector2(s * (gapH + barW) * 0.5f, 0f);
                var barImg = barGO.AddComponent<Image>();
                barImg.color = Color.white;
            }
        }

        // ── Badge jumlah lencana (digabung dari tombol 🏆 lama) ───────────
        // Lingkaran kuning kecil di pojok kanan-atas tombol pause yang menampilkan
        // berapa lencana sudah diraih. Info ini dipindah dari tombol lencana terpisah.
        var badgeGO = new GameObject("LencanaBadge");
        badgeGO.transform.SetParent(mobileBtnRoot.transform, false);
        var badgeRT = badgeGO.AddComponent<RectTransform>();
        float badgeD = mobilePauseButtonSize.x * 0.46f;
        badgeRT.anchorMin = badgeRT.anchorMax = new Vector2(1f, 1f);
        badgeRT.pivot     = new Vector2(0.5f, 0.5f);
        badgeRT.sizeDelta = new Vector2(badgeD, badgeD);
        badgeRT.anchoredPosition = new Vector2(-badgeD * 0.12f, -badgeD * 0.12f);
        var badgeImg = badgeGO.AddComponent<Image>();
        badgeImg.color = new Color(0.96f, 0.65f, 0.10f, 1f);   // kuning lencana
        badgeImg.raycastTarget = false;

        _lencanaBadgeLabel = new GameObject("Jumlah").AddComponent<TextMeshProUGUI>();
        _lencanaBadgeLabel.transform.SetParent(badgeGO.transform, false);
        var lblRT = _lencanaBadgeLabel.rectTransform;
        lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one;
        lblRT.offsetMin = Vector2.zero; lblRT.offsetMax = Vector2.zero;
        if (buttonFontAsset != null) _lencanaBadgeLabel.font = buttonFontAsset;
        _lencanaBadgeLabel.text          = "0";
        _lencanaBadgeLabel.fontSize      = badgeD * 0.62f;
        _lencanaBadgeLabel.color         = new Color(0.2f, 0.1f, 0f, 1f);
        _lencanaBadgeLabel.fontStyle     = FontStyles.Bold;
        _lencanaBadgeLabel.alignment     = TextAlignmentOptions.Center;
        _lencanaBadgeLabel.raycastTarget = false;
        RefreshLencanaBadge();
    }

    // Perbarui angka lencana pada badge tombol pause dari GameState.
    void RefreshLencanaBadge()
    {
        if (_lencanaBadgeLabel == null) return;
        int n = (GameState.Instance != null && GameState.Instance.achievements != null)
            ? GameState.Instance.achievements.Count : 0;
        string s = n.ToString();
        if (_lencanaBadgeLabel.text != s) _lencanaBadgeLabel.text = s;
    }

    // Atur anchor & pivot tombol pause sesuai sudut yang dipilih, plus margin
    // (akan diadjust ulang oleh safe area jika aktif).
    void ApplyCornerAnchor(RectTransform rt)
    {
        switch (mobilePauseButtonCorner)
        {
            case ScreenCorner.TopLeft:
                rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot     = new Vector2(0f, 1f);
                rt.anchoredPosition = new Vector2( mobilePauseButtonMargin.x, -mobilePauseButtonMargin.y);
                break;
            case ScreenCorner.TopRight:
                rt.anchorMin = new Vector2(1f, 1f); rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot     = new Vector2(1f, 1f);
                rt.anchoredPosition = new Vector2(-mobilePauseButtonMargin.x, -mobilePauseButtonMargin.y);
                break;
            case ScreenCorner.BottomLeft:
                rt.anchorMin = new Vector2(0f, 0f); rt.anchorMax = new Vector2(0f, 0f);
                rt.pivot     = new Vector2(0f, 0f);
                rt.anchoredPosition = new Vector2( mobilePauseButtonMargin.x,  mobilePauseButtonMargin.y);
                break;
            case ScreenCorner.BottomRight:
            default:
                rt.anchorMin = new Vector2(1f, 0f); rt.anchorMax = new Vector2(1f, 0f);
                rt.pivot     = new Vector2(1f, 0f);
                rt.anchoredPosition = new Vector2(-mobilePauseButtonMargin.x,  mobilePauseButtonMargin.y);
                break;
        }
    }

    void UpdateMobileButtonVisibility()
    {
        if (mobileBtnRoot == null) return;
        // Sembunyikan tombol pause selama layar prolog tampil.
        bool shouldShow = showMobilePauseButton && !isOpen && !PrologScreen.SedangTampil && !Day1Intro.SedangTampil && !Day1SummaryScreen.SedangTampil;
        if (mobileBtnRoot.activeSelf != shouldShow) mobileBtnRoot.SetActive(shouldShow);

        // Sync ukuran/posisi live (mendukung perubahan sudut di Inspector)
        var rt = mobileBtnRoot.GetComponent<RectTransform>();
        if (rt != null)
        {
            ApplyCornerAnchor(rt);
            rt.sizeDelta = mobilePauseButtonSize;
        }
        var img = mobileBtnRoot.GetComponent<Image>();
        if (img != null && mobilePauseButtonSprite != null && img.sprite != mobilePauseButtonSprite)
        {
            img.sprite = mobilePauseButtonSprite; img.color = Color.white;
        }

        RefreshLencanaBadge();
    }

    // ══════ Safe Area (notch handling) ══════
    Rect lastSafeArea = Rect.zero;
    void ApplySafeAreaIfNeeded()
    {
        if (!useSafeArea) return;
        Rect sa = Screen.safeArea;
        if (sa == lastSafeArea) return;
        lastSafeArea = sa;

        // Padding panel agar tidak kepotong notch
        RectTransform panelRT = (panelRootRef != null) ? panelRootRef.GetComponent<RectTransform>()
            : (panelRoot != null ? panelRoot.GetComponent<RectTransform>() : null);
        // Hanya geser jika dock bottom-left aktif (Mode B default)
        if (panelRT != null && dockBottomLeft)
        {
            float leftMargin   = Mathf.Max(dockMargin.x, sa.xMin);
            float bottomMargin = Mathf.Max(dockMargin.y, sa.yMin);
            panelRT.anchoredPosition = new Vector2(leftMargin, bottomMargin);
        }

        // Geser tombol pause mobile agar tidak kena notch sesuai sudut yang dipilih
        if (mobileBtnRoot != null)
        {
            var rt = mobileBtnRoot.GetComponent<RectTransform>();
            float leftInset   = sa.xMin;
            float rightInset  = Screen.width  - sa.xMax;
            float topInset    = Screen.height - sa.yMax;
            float bottomInset = sa.yMin;

            switch (mobilePauseButtonCorner)
            {
                case ScreenCorner.TopLeft:
                    rt.anchoredPosition = new Vector2( mobilePauseButtonMargin.x + leftInset, -(mobilePauseButtonMargin.y + topInset));
                    break;
                case ScreenCorner.TopRight:
                    rt.anchoredPosition = new Vector2(-(mobilePauseButtonMargin.x + rightInset), -(mobilePauseButtonMargin.y + topInset));
                    break;
                case ScreenCorner.BottomLeft:
                    rt.anchoredPosition = new Vector2( mobilePauseButtonMargin.x + leftInset,  mobilePauseButtonMargin.y + bottomInset);
                    break;
                case ScreenCorner.BottomRight:
                default:
                    rt.anchoredPosition = new Vector2(-(mobilePauseButtonMargin.x + rightInset), mobilePauseButtonMargin.y + bottomInset);
                    break;
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // API
    // ══════════════════════════════════════════════════════════════════════
    public void Toggle() { if (isOpen) ClosePanel(); else OpenPanel(); }

    public void OpenPanel()
    {
        if (usingEditorRefs) { uiRootRef.SetActive(true); panelRootRef.SetActive(true); }
        else                 { uiRoot.SetActive(true); panelRoot.SetActive(true); }
        isOpen = true;
        if (pauseTimeScale) { prevTimeScale = Time.timeScale; Time.timeScale = 0f; }
        ReapplyAllCustomization();
    }

    public void ClosePanel()
    {
        if (settingsPanel != null) { Destroy(settingsPanel); settingsPanel = null; }
        if (keluarPanel != null) { Destroy(keluarPanel); keluarPanel = null; }
        if (usingEditorRefs)
        {
            if (panelRootRef != null) panelRootRef.SetActive(false);
            if (uiRootRef    != null) uiRootRef.SetActive(false);
        }
        else
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (uiRoot    != null) uiRoot.SetActive(false);
        }
        isOpen = false;
        if (pauseTimeScale) Time.timeScale = (prevTimeScale > 0f ? prevTimeScale : 1f);
    }

    void OnResumeClicked() { ClosePanel(); onResume?.Invoke(); }

    void OnMenuClicked()
    {
        ClosePanel();
        Time.timeScale = 1f;
        onGoMenu?.Invoke();

        // Reset progres pemain (nyawa, skor, pilihan, hari) supaya menu utama
        // benar-benar mulai dari awal — tidak membawa state Hari 1/2/3.
        if (GameState.Instance != null) GameState.Instance.Reset();

        // Game ini single-scene (Gameplay). Kalau mainMenuSceneName kosong,
        // sama dengan scene aktif, atau bukan scene yang ada di Build Settings,
        // muat ulang scene aktif. MainMenu (komponen di scene) otomatis tampil
        // lagi pada Awake/Start dan memutar BGM Menu via PlayBGM(BGMTrack.Menu).
        string current = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool targetValid = !string.IsNullOrEmpty(mainMenuSceneName)
                           && mainMenuSceneName != current
                           && Application.CanStreamedLevelBeLoaded(mainMenuSceneName);

        if (targetValid)
        {
            if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene(mainMenuSceneName);
            else UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            if (SceneLoader.Instance != null) SceneLoader.Instance.ReloadCurrentScene();
            else UnityEngine.SceneManagement.SceneManager.LoadScene(current);
        }
    }

    void ForcePreview()
    {
        if (usingEditorRefs)
        {
            if (uiRootRef != null && !uiRootRef.activeSelf)       uiRootRef.SetActive(true);
            if (panelRootRef != null && !panelRootRef.activeSelf) panelRootRef.SetActive(true);
        }
        else
        {
            if (uiRoot != null && !uiRoot.activeSelf)       uiRoot.SetActive(true);
            if (panelRoot != null && !panelRoot.activeSelf) panelRoot.SetActive(true);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // SETUP — Mode A
    // ══════════════════════════════════════════════════════════════════════
    void SetupEditorRefs()
    {
        if (btnResumeRef != null) { btnResumeRef.onClick.RemoveAllListeners(); btnResumeRef.onClick.AddListener(OnResumeClicked); }
        if (btnMenuRef   != null) { btnMenuRef.onClick.RemoveAllListeners();   btnMenuRef.onClick.AddListener(OnMenuClicked); }
        ReapplyAllCustomization();
        if (debugLog) Debug.Log("[PauseMenu] Mode A aktif.");
    }

    // ══════════════════════════════════════════════════════════════════════
    // BUILD — Mode B Programatik
    // ══════════════════════════════════════════════════════════════════════
    void BuildUI()
    {
        var cGO = new GameObject("PauseMenuCanvas");
        canvas = cGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        // 1100 = paling atas dari semua overlay (lencana 1050, prolog 1000, dialog 999,
        // mobile 985, navbar 940) agar tombol pause SELALU menerima klik & tak tertutup.
        canvas.sortingOrder = 1100;
        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        // Landscape 1920x1080 — samakan dengan canvas lain agar skala/posisi konsisten.
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        uiRoot = new GameObject("UIRoot");
        uiRoot.transform.SetParent(cGO.transform, false);
        var uiRT = uiRoot.AddComponent<RectTransform>();
        uiRT.anchorMin = Vector2.zero; uiRT.anchorMax = Vector2.one;
        uiRT.offsetMin = Vector2.zero; uiRT.offsetMax = Vector2.zero;

        var ovGO = new GameObject("Overlay");
        ovGO.transform.SetParent(uiRoot.transform, false);
        var ovRT = ovGO.AddComponent<RectTransform>();
        ovRT.anchorMin = Vector2.zero; ovRT.anchorMax = Vector2.one;
        ovRT.offsetMin = Vector2.zero; ovRT.offsetMax = Vector2.zero;
        var ovImg = ovGO.AddComponent<Image>(); ovImg.color = new Color(0f, 0f, 0f, 0.5f);

        // Panel — default di tengah layar
        panelRoot = new GameObject("Panel");
        panelRoot.transform.SetParent(uiRoot.transform, false);
        var pRT = panelRoot.AddComponent<RectTransform>();
        if (dockBottomLeft)
        {
            pRT.anchorMin = new Vector2(0f, 0f);
            pRT.anchorMax = new Vector2(0f, 0f);
            pRT.pivot     = new Vector2(0f, 0f);
            pRT.anchoredPosition = dockMargin;
        }
        else
        {
            pRT.anchorMin = new Vector2(0.5f, 0.5f);
            pRT.anchorMax = new Vector2(0.5f, 0.5f);
            pRT.pivot     = new Vector2(0.5f, 0.5f);
            pRT.anchoredPosition = panelAnchoredPos;
        }
        pRT.sizeDelta        = panelSize;
        var pImg = panelRoot.AddComponent<Image>(); pImg.color = panelBgColor;
        pImg.sprite = GetRoundedSpritePause();
        pImg.type   = Image.Type.Sliced;
        var outl = panelRoot.AddComponent<Outline>(); outl.effectColor = borderColor;
        outl.effectDistance = new Vector2(3f, -3f);

        // Judul — pita atas, terpusat.
        MakeText(panelRoot, "Title", new Vector2(0f, 1f), new Vector2(1f, 1f),
                 new Vector2(12f, -100f), new Vector2(-12f, -28f), 44, titleColor,
                 TextAlignmentOptions.Center, true, titleText);

        // Tips — blok teks rata kiri dengan jarak baris lega. Kotak diperluas ke
        // bawah (hingga tepat di atas tombol) + auto-size agar teks panjang Hari 2
        // (3 Kata Sakti) tidak meluber menutupi tombol LANJUTKAN.
        var tipsTmp = MakeText(panelRoot, "Tips", new Vector2(0f, 1f), new Vector2(1f, 1f),
                 new Vector2(28f, -492f), new Vector2(-28f, -126f), 26, tipsColor,
                 TextAlignmentOptions.TopLeft, false, tipsText);
        tipsTmp.lineSpacing = 12f;
        tipsTmp.enableAutoSizing = true;
        tipsTmp.fontSizeMin = 16f;
        tipsTmp.fontSizeMax = 26f;

        // Nomor darurat — terpusat, ikon emoji 📞 diganti bullet agar tak jadi kotak kosong.
        MakeText(panelRoot, "Emergency", new Vector2(0f, 1f), new Vector2(1f, 1f),
                 new Vector2(16f, -500f), new Vector2(-16f, -408f), 28, emergencyColor,
                 TextAlignmentOptions.Center, true, BersihkanIkonDarurat(emergencyText));

        // Tombol — dirapatkan ke konten (hilangkan ruang kosong besar).
        MakeButton(panelRoot, "BtnResume", resumeLabel,
                   new Vector2(0.09f, 0.175f), new Vector2(0.91f, 0.275f),
                   resumeColor, OnResumeClicked);
        MakeButton(panelRoot, "BtnMenu", menuLabel,
                   new Vector2(0.09f, 0.05f), new Vector2(0.91f, 0.15f),
                   menuColor, OnMenuClicked);

        // Tombol ⚙ Pengaturan — kotak kecil di pojok kanan-atas panel agar tidak
        // mengganggu tata letak konten (tips / nomor darurat).
        if (tampilkanPengaturan)
        {
            var gearGO = new GameObject("BtnSettings");
            gearGO.transform.SetParent(panelRoot.transform, false);
            var gRT = gearGO.AddComponent<RectTransform>();
            gRT.anchorMin = new Vector2(1f, 1f); gRT.anchorMax = new Vector2(1f, 1f);
            gRT.pivot     = new Vector2(1f, 1f);
            gRT.sizeDelta = new Vector2(64f, 64f);
            gRT.anchoredPosition = new Vector2(-14f, -14f);
            var gImg = gearGO.AddComponent<Image>();
            gImg.sprite = GetRoundedSpritePause();
            gImg.type   = Image.Type.Sliced;
            gImg.color  = settingsColor;
            var gOl = gearGO.AddComponent<Outline>();
            gOl.effectColor = new Color(1f, 1f, 1f, 0.30f);
            gOl.effectDistance = new Vector2(2f, -2f);
            var gBtn = gearGO.AddComponent<Button>();
            gBtn.onClick.AddListener(OpenSettings);
            var gLblGO = new GameObject("Icon");
            gLblGO.transform.SetParent(gearGO.transform, false);
            var gLblRT = gLblGO.AddComponent<RectTransform>();
            gLblRT.anchorMin = Vector2.zero; gLblRT.anchorMax = Vector2.one;
            gLblRT.offsetMin = Vector2.zero; gLblRT.offsetMax = Vector2.zero;
            var gLbl = gLblGO.AddComponent<TextMeshProUGUI>();
            ApplyFont(gLbl);
            gLbl.text = "⚙";
            gLbl.fontSize = 34;
            gLbl.color = Color.white;
            gLbl.alignment = TextAlignmentOptions.Center;
        }

        // Tombol ✖ Keluar — kotak kecil di pojok kiri-atas panel (cermin tombol ⚙).
        if (tampilkanKeluar)
        {
            var exitGO = new GameObject("BtnKeluar");
            exitGO.transform.SetParent(panelRoot.transform, false);
            var eRT = exitGO.AddComponent<RectTransform>();
            eRT.anchorMin = new Vector2(0f, 1f); eRT.anchorMax = new Vector2(0f, 1f);
            eRT.pivot     = new Vector2(0f, 1f);
            eRT.sizeDelta = new Vector2(64f, 64f);
            eRT.anchoredPosition = new Vector2(14f, -14f);
            var eImg = exitGO.AddComponent<Image>();
            eImg.sprite = GetRoundedSpritePause();
            eImg.type   = Image.Type.Sliced;
            eImg.color  = keluarColor;
            var eOl = exitGO.AddComponent<Outline>();
            eOl.effectColor = new Color(1f, 1f, 1f, 0.30f);
            eOl.effectDistance = new Vector2(2f, -2f);
            var eBtn = exitGO.AddComponent<Button>();
            eBtn.onClick.AddListener(KonfirmasiKeluar);
            var eLblGO = new GameObject("Icon");
            eLblGO.transform.SetParent(exitGO.transform, false);
            var eLblRT = eLblGO.AddComponent<RectTransform>();
            eLblRT.anchorMin = Vector2.zero; eLblRT.anchorMax = Vector2.one;
            eLblRT.offsetMin = Vector2.zero; eLblRT.offsetMax = Vector2.zero;
            var eLbl = eLblGO.AddComponent<TextMeshProUGUI>();
            ApplyFont(eLbl);
            eLbl.text = "\u2716";
            eLbl.fontSize = 32;
            eLbl.color = Color.white;
            eLbl.alignment = TextAlignmentOptions.Center;
        }

        if (debugLog) Debug.Log("[PauseMenu] Mode B aktif.");
    }

    // ══════════════════════════════════════════════════════════════════════
    // KONFIRMASI KELUAR (Quit)
    // ══════════════════════════════════════════════════════════════════════
    public void KonfirmasiKeluar()
    {
        AudioManager.Instance?.Click();
        if (keluarPanel != null) { Destroy(keluarPanel); keluarPanel = null; }
        BuildKeluarPanel();
    }

    public void TutupKonfirmasiKeluar()
    {
        AudioManager.Instance?.Click();
        if (keluarPanel != null) { Destroy(keluarPanel); keluarPanel = null; }
    }

    public void KeluarSekarang()
    {
        AudioManager.Instance?.Click();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void BuildKeluarPanel()
    {
        Transform parent = (uiRoot != null) ? uiRoot.transform
                         : (uiRootRef != null) ? uiRootRef.transform
                         : (canvas != null ? canvas.transform : transform);

        // Overlay gelap (klik luar = batal)
        keluarPanel = new GameObject("KeluarPanel");
        keluarPanel.transform.SetParent(parent, false);
        var ovRT = keluarPanel.AddComponent<RectTransform>();
        ovRT.anchorMin = Vector2.zero; ovRT.anchorMax = Vector2.one;
        ovRT.offsetMin = Vector2.zero; ovRT.offsetMax = Vector2.zero;
        var ovImg = keluarPanel.AddComponent<Image>();
        ovImg.color = new Color(0f, 0f, 0f, 0.65f);
        var ovBtn = keluarPanel.AddComponent<Button>();
        ovBtn.transition = Selectable.Transition.None;
        ovBtn.onClick.AddListener(TutupKonfirmasiKeluar);

        // Kartu tengah
        var card = new GameObject("Card");
        card.transform.SetParent(keluarPanel.transform, false);
        var cardRT = card.AddComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.5f, 0.5f);
        cardRT.anchorMax = new Vector2(0.5f, 0.5f);
        cardRT.pivot     = new Vector2(0.5f, 0.5f);
        cardRT.sizeDelta = new Vector2(620f, 360f);
        var cardImg = card.AddComponent<Image>();
        cardImg.sprite = GetRoundedSpritePause();
        cardImg.type   = Image.Type.Sliced;
        cardImg.color  = panelBgColor;
        var cardOl = card.AddComponent<Outline>();
        cardOl.effectColor = keluarColor;
        cardOl.effectDistance = new Vector2(3f, -3f);
        var cardBlocker = card.AddComponent<Button>();
        cardBlocker.transition = Selectable.Transition.None;

        // Judul
        MakeText(card, "KeluarTitle", new Vector2(0f, 1f), new Vector2(1f, 1f),
                 new Vector2(20f, -84f), new Vector2(-20f, -18f), 36, keluarColor,
                 TextAlignmentOptions.Center, true, keluarKonfirmasiJudul);

        // Pesan
        MakeText(card, "KeluarPesan", new Vector2(0f, 0f), new Vector2(1f, 1f),
                 new Vector2(30f, 110f), new Vector2(-30f, -96f), 26, tipsColor,
                 TextAlignmentOptions.Center, false, keluarKonfirmasiPesan);

        // Tombol BATAL (kiri)
        var batalGO = new GameObject("BtnBatal");
        batalGO.transform.SetParent(card.transform, false);
        var bRT = batalGO.AddComponent<RectTransform>();
        bRT.anchorMin = new Vector2(0.5f, 0f); bRT.anchorMax = new Vector2(0.5f, 0f);
        bRT.pivot = new Vector2(0.5f, 0f);
        bRT.sizeDelta = new Vector2(240f, 64f);
        bRT.anchoredPosition = new Vector2(-130f, 28f);
        var bImg = batalGO.AddComponent<Image>();
        bImg.sprite = GetRoundedSpritePause(); bImg.type = Image.Type.Sliced;
        bImg.color  = settingsColor;
        var bBtn = batalGO.AddComponent<Button>();
        bBtn.onClick.AddListener(TutupKonfirmasiKeluar);
        var bLbl = MakeText(batalGO, "Label", Vector2.zero, Vector2.one,
                            Vector2.zero, Vector2.zero, 28, btnTextColor,
                            TextAlignmentOptions.Center, true, keluarBatalLabel);
        bLbl.rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // Tombol KELUAR (kanan)
        var yaGO = new GameObject("BtnKeluarYa");
        yaGO.transform.SetParent(card.transform, false);
        var yRT = yaGO.AddComponent<RectTransform>();
        yRT.anchorMin = new Vector2(0.5f, 0f); yRT.anchorMax = new Vector2(0.5f, 0f);
        yRT.pivot = new Vector2(0.5f, 0f);
        yRT.sizeDelta = new Vector2(240f, 64f);
        yRT.anchoredPosition = new Vector2(130f, 28f);
        var yImg = yaGO.AddComponent<Image>();
        yImg.sprite = GetRoundedSpritePause(); yImg.type = Image.Type.Sliced;
        yImg.color  = keluarColor;
        var yBtn = yaGO.AddComponent<Button>();
        yBtn.onClick.AddListener(KeluarSekarang);
        var yLbl = MakeText(yaGO, "Label", Vector2.zero, Vector2.one,
                            Vector2.zero, Vector2.zero, 28, btnTextColor,
                            TextAlignmentOptions.Center, true, keluarYaLabel);
        yLbl.rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }

    // ══════════════════════════════════════════════════════════════════════
    // PENGATURAN (Settings) — volume, font, aksesibilitas
    // ══════════════════════════════════════════════════════════════════════
    public void OpenSettings()
    {
        AudioManager.Instance?.Click();
        if (settingsPanel != null) { Destroy(settingsPanel); settingsPanel = null; }
        BuildSettingsPanel();
    }

    public void CloseSettings()
    {
        AudioManager.Instance?.Click();
        if (settingsPanel != null) { Destroy(settingsPanel); settingsPanel = null; }
    }

    void BuildSettingsPanel()
    {
        // Induk = uiRoot (Mode B) atau uiRootRef (Mode A); fallback ke canvas.
        Transform parent = (uiRoot != null) ? uiRoot.transform
                         : (uiRootRef != null) ? uiRootRef.transform
                         : (canvas != null ? canvas.transform : transform);

        // Overlay gelap (klik luar = tutup)
        settingsPanel = new GameObject("SettingsPanel");
        settingsPanel.transform.SetParent(parent, false);
        var ovRT = settingsPanel.AddComponent<RectTransform>();
        ovRT.anchorMin = Vector2.zero; ovRT.anchorMax = Vector2.one;
        ovRT.offsetMin = Vector2.zero; ovRT.offsetMax = Vector2.zero;
        var ovImg = settingsPanel.AddComponent<Image>();
        ovImg.color = new Color(0f, 0f, 0f, 0.6f);
        var ovBtn = settingsPanel.AddComponent<Button>();
        ovBtn.transition = Selectable.Transition.None;
        ovBtn.onClick.AddListener(CloseSettings);

        // Kartu tengah
        var card = new GameObject("Card");
        card.transform.SetParent(settingsPanel.transform, false);
        var cardRT = card.AddComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.5f, 0.5f);
        cardRT.anchorMax = new Vector2(0.5f, 0.5f);
        cardRT.pivot     = new Vector2(0.5f, 0.5f);
        cardRT.sizeDelta = new Vector2(620f, 640f);
        var cardImg = card.AddComponent<Image>();
        cardImg.sprite = GetRoundedSpritePause();
        cardImg.type   = Image.Type.Sliced;
        cardImg.color  = panelBgColor;
        var cardOl = card.AddComponent<Outline>();
        cardOl.effectColor = borderColor;
        cardOl.effectDistance = new Vector2(3f, -3f);
        var cardBlocker = card.AddComponent<Button>();   // cegah klik tembus ke overlay
        cardBlocker.transition = Selectable.Transition.None;

        // Judul
        var judul = MakeText(card, "SettingsTitle", new Vector2(0f, 1f), new Vector2(1f, 1f),
                             new Vector2(20f, -84f), new Vector2(-20f, -18f), 36, titleColor,
                             TextAlignmentOptions.Center, true, settingsTitleText);
        judul.color = borderColor;

        float y = -110f;   // kursor vertikal dari atas kartu

        // ── VOLUME ───────────────────────────────────────────────────────
        BuatLabelSeksi(card, "🔊 Volume", ref y);
        BuatSlider(card, "Volume Suara", GameSettings.MasterVolume, 0f, 1f, ref y, (v) =>
        {
            GameSettings.MasterVolume = v;
        });
        BuatToggle(card, "Musik Latar", GameSettings.MusicOn, ref y, (on) =>
        {
            GameSettings.MusicOn = on;
        });

        // ── FONT ─────────────────────────────────────────────────────────
        BuatLabelSeksi(card, "🔤 Ukuran Font", ref y);
        BuatSlider(card, "Skala Teks", GameSettings.FontScale, 0.8f, 1.6f, ref y, (v) =>
        {
            GameSettings.FontScale = v;
        });

        // ── AKSESIBILITAS ────────────────────────────────────────────────
        BuatLabelSeksi(card, "♿ Aksesibilitas", ref y);
        BuatToggle(card, "Kurangi Animasi", GameSettings.ReduceMotion, ref y, (on) =>
        {
            GameSettings.ReduceMotion = on;
        });

        // Tombol Tutup
        var tutupGO = new GameObject("BtnTutupSettings");
        tutupGO.transform.SetParent(card.transform, false);
        var tRT = tutupGO.AddComponent<RectTransform>();
        tRT.anchorMin = new Vector2(0.5f, 0f); tRT.anchorMax = new Vector2(0.5f, 0f);
        tRT.pivot = new Vector2(0.5f, 0f);
        tRT.sizeDelta = new Vector2(300f, 64f);
        tRT.anchoredPosition = new Vector2(0f, 24f);
        var tImg = tutupGO.AddComponent<Image>();
        tImg.sprite = GetRoundedSpritePause();
        tImg.type   = Image.Type.Sliced;
        tImg.color  = resumeColor;
        var tOl = tutupGO.AddComponent<Outline>();
        tOl.effectColor = new Color(1f, 1f, 1f, 0.30f);
        tOl.effectDistance = new Vector2(2f, -2f);
        var tBtn = tutupGO.AddComponent<Button>();
        tBtn.onClick.AddListener(CloseSettings);
        var tLbl = MakeText(tutupGO, "Label", Vector2.zero, Vector2.one,
                            Vector2.zero, Vector2.zero, 28, btnTextColor,
                            TextAlignmentOptions.Center, true, "TUTUP");
        tLbl.rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }

    // Label judul seksi (Volume / Font / Aksesibilitas)
    void BuatLabelSeksi(GameObject card, string teks, ref float y)
    {
        var lbl = MakeText(card, "Seksi_" + teks, new Vector2(0f, 1f), new Vector2(1f, 1f),
                           new Vector2(28f, y - 36f), new Vector2(-28f, y), 24,
                           borderColor, TextAlignmentOptions.MidlineLeft, true, teks);
        y -= 46f;
    }

    // Slider berlabel dengan nilai persen di kanan.
    void BuatSlider(GameObject card, string nama, float nilai, float min, float max,
                    ref float y, Action<float> onChange)
    {
        // Label kiri
        MakeText(card, "Lbl_" + nama, new Vector2(0f, 1f), new Vector2(0.5f, 1f),
                 new Vector2(40f, y - 44f), new Vector2(-8f, y), 20, tipsColor,
                 TextAlignmentOptions.MidlineLeft, false, nama);

        // Nilai persen kanan
        var valTMP = MakeText(card, "Val_" + nama, new Vector2(0.78f, 1f), new Vector2(1f, 1f),
                 new Vector2(0f, y - 44f), new Vector2(-28f, y), 20, borderColor,
                 TextAlignmentOptions.MidlineRight, true, Mathf.RoundToInt(nilai * 100f) + "%");

        y -= 44f;

        // Track slider
        var sGO = new GameObject("Slider_" + nama);
        sGO.transform.SetParent(card.transform, false);
        var sRT = sGO.AddComponent<RectTransform>();
        sRT.anchorMin = new Vector2(0f, 1f); sRT.anchorMax = new Vector2(1f, 1f);
        sRT.pivot = new Vector2(0.5f, 1f);
        sRT.offsetMin = new Vector2(40f, y - 28f);
        sRT.offsetMax = new Vector2(-28f, y);
        var slider = sGO.AddComponent<Slider>();

        // Background track
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sGO.transform, false);
        var bgRT = bgGO.AddComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0f, 0.35f); bgRT.anchorMax = new Vector2(1f, 0.65f);
        bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.sprite = GetRoundedSpritePause(); bgImg.type = Image.Type.Sliced;
        bgImg.color = new Color(0f, 0f, 0f, 0.55f);

        // Fill area
        var fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sGO.transform, false);
        var faRT = fillAreaGO.AddComponent<RectTransform>();
        faRT.anchorMin = new Vector2(0f, 0.35f); faRT.anchorMax = new Vector2(1f, 0.65f);
        faRT.offsetMin = new Vector2(2f, 0f); faRT.offsetMax = new Vector2(-2f, 0f);
        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        var fillRT = fillGO.AddComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0f, 0f); fillRT.anchorMax = new Vector2(0f, 1f);
        fillRT.offsetMin = Vector2.zero; fillRT.offsetMax = Vector2.zero;
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.sprite = GetRoundedSpritePause(); fillImg.type = Image.Type.Sliced;
        fillImg.color = settingsColor;

        // Handle
        var hAreaGO = new GameObject("Handle Slide Area");
        hAreaGO.transform.SetParent(sGO.transform, false);
        var haRT = hAreaGO.AddComponent<RectTransform>();
        haRT.anchorMin = new Vector2(0f, 0f); haRT.anchorMax = new Vector2(1f, 1f);
        haRT.offsetMin = new Vector2(10f, 0f); haRT.offsetMax = new Vector2(-10f, 0f);
        var handleGO = new GameObject("Handle");
        handleGO.transform.SetParent(hAreaGO.transform, false);
        var handleRT = handleGO.AddComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(34f, 34f);
        var handleImg = handleGO.AddComponent<Image>();
        handleImg.sprite = GetRoundedSpritePause(); handleImg.type = Image.Type.Sliced;
        handleImg.color = Color.white;

        slider.fillRect       = fillRT;
        slider.handleRect     = handleRT;
        slider.targetGraphic  = handleImg;
        slider.direction      = Slider.Direction.LeftToRight;
        slider.minValue       = min;
        slider.maxValue       = max;
        slider.value          = nilai;
        slider.onValueChanged.AddListener((v) =>
        {
            valTMP.text = Mathf.RoundToInt(v * 100f) + "%";
            onChange(v);
        });

        y -= 40f;
    }

    // Toggle berlabel (kotak centang) untuk opsi on/off.
    void BuatToggle(GameObject card, string nama, bool nilai, ref float y, Action<bool> onChange)
    {
        MakeText(card, "Lbl_" + nama, new Vector2(0f, 1f), new Vector2(0.7f, 1f),
                 new Vector2(40f, y - 48f), new Vector2(0f, y), 20, tipsColor,
                 TextAlignmentOptions.MidlineLeft, false, nama);

        var tGO = new GameObject("Toggle_" + nama);
        tGO.transform.SetParent(card.transform, false);
        var tRT = tGO.AddComponent<RectTransform>();
        tRT.anchorMin = new Vector2(1f, 1f); tRT.anchorMax = new Vector2(1f, 1f);
        tRT.pivot = new Vector2(1f, 1f);
        tRT.sizeDelta = new Vector2(48f, 48f);
        tRT.anchoredPosition = new Vector2(-28f, y - 2f);
        var bgImg = tGO.AddComponent<Image>();
        bgImg.sprite = GetRoundedSpritePause(); bgImg.type = Image.Type.Sliced;
        bgImg.color = new Color(0f, 0f, 0f, 0.55f);
        var toggle = tGO.AddComponent<Toggle>();

        // Tanda centang
        var checkGO = new GameObject("Check");
        checkGO.transform.SetParent(tGO.transform, false);
        var cRT = checkGO.AddComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0.12f, 0.12f); cRT.anchorMax = new Vector2(0.88f, 0.88f);
        cRT.offsetMin = cRT.offsetMax = Vector2.zero;
        var cImg = checkGO.AddComponent<Image>();
        cImg.sprite = GetRoundedSpritePause(); cImg.type = Image.Type.Sliced;
        cImg.color = settingsColor;

        toggle.targetGraphic = bgImg;
        toggle.graphic       = cImg;
        toggle.isOn          = nilai;
        toggle.onValueChanged.AddListener((on) => onChange(on));

        y -= 56f;
    }

    TextMeshProUGUI MakeText(GameObject parent, string name,
        Vector2 ancMin, Vector2 ancMax, Vector2 offMin, Vector2 offMax,
        int fontSize, Color color, TextAlignmentOptions align, bool bold, string content)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.pivot = new Vector2(0.5f, 1f);
        rt.offsetMin = offMin; rt.offsetMax = offMax;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        ApplyFont(tmp);
        tmp.text = content; tmp.fontSize = fontSize; tmp.color = color; tmp.alignment = align;
        if (bold) tmp.fontStyle = FontStyles.Bold;
        return tmp;
    }

    void MakeButton(GameObject parent, string name, string label,
        Vector2 ancMin, Vector2 ancMax, Color color, Action onClick)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>(); img.color = color;
        img.sprite = GetRoundedSpritePause();
        img.type   = Image.Type.Sliced;
        var btnOutline = go.AddComponent<Outline>();
        btnOutline.effectColor    = new Color(1f, 1f, 1f, 0.30f);
        btnOutline.effectDistance = new Vector2(2f, -2f);
        var btn = go.AddComponent<Button>(); btn.onClick.AddListener(() => onClick());

        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(go.transform, false);
        var lrt = lblGO.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = new Vector2(6f, 4f); lrt.offsetMax = new Vector2(-6f, -4f);
        var lbl = lblGO.AddComponent<TextMeshProUGUI>();
        ApplyFont(lbl);
        lbl.text = label; lbl.fontSize = 32; lbl.color = btnTextColor;
        lbl.alignment = TextAlignmentOptions.Center; lbl.fontStyle = FontStyles.Bold;
    }

    void ApplyFont(TextMeshProUGUI tmp)
    {
        var f = fontAsset;
        if (f == null) f = TMP_Settings.defaultFontAsset;
        if (f != null) tmp.font = f;
    }

    // Ganti ikon telepon 📞 (U+1F4DE) — yang tak tersedia di font aktif sehingga
    // muncul sebagai kotak kosong — dengan bullet "•" yang pasti ada.
    static string BersihkanIkonDarurat(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Replace("\U0001F4DE", "\u2022");
    }

    // Sprite kotak sudut-membulat (9-slice) untuk panel & tombol. Di-cache statis.
    static Sprite _sRoundedPause;
    static Sprite GetRoundedSpritePause()
    {
        if (_sRoundedPause != null) return _sRoundedPause;
        const int S = 48, R = 16;
        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < S; y++)
        for (int x = 0; x < S; x++)
        {
            float dx = Mathf.Max(R - x, x - (S - 1 - R), 0f);
            float dy = Mathf.Max(R - y, y - (S - 1 - R), 0f);
            float d  = Mathf.Sqrt(dx * dx + dy * dy);
            float a  = Mathf.Clamp01(R - d + 0.5f); // tepi anti-alias
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        tex.Apply();
        _sRoundedPause = Sprite.Create(tex, new Rect(0, 0, S, S),
            new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
            new Vector4(R, R, R, R));
        return _sRoundedPause;
    }

    // ══════════════════════════════════════════════════════════════════════
    // APPLY
    // ══════════════════════════════════════════════════════════════════════
    [ContextMenu("▶ Apply Customization Sekarang")]
    public void ReapplyAllCustomization()
    {
        // Hari 2: tampilkan konten 3 Kata Sakti sebagai isi panel JEDA.
        bool kataSaktiHari2 = gunakanKontenKataSaktiHari2
            && GameState.Instance != null && GameState.Instance.day == 2;
        string isiTitle     = kataSaktiHari2 ? titleTextHari2     : titleText;
        string isiTips      = kataSaktiHari2 ? tipsTextHari2      : tipsText;
        string isiEmergency = kataSaktiHari2 ? emergencyTextHari2 : emergencyText;

        SetText(titleTMPRef,     "Title",     isiTitle);
        SetText(tipsTMPRef,      "Tips",      isiTips);
        SetText(emergencyTMPRef, "Emergency", isiEmergency);
        if (btnResumeLabelRef != null) btnResumeLabelRef.text = resumeLabel;
        if (btnMenuLabelRef   != null) btnMenuLabelRef.text   = menuLabel;
        var bR = FindChild("BtnResume"); if (bR != null) { var l = bR.Find("Label"); if (l != null) { var x = l.GetComponent<TextMeshProUGUI>(); if (x != null) x.text = resumeLabel; } }
        var bM = FindChild("BtnMenu");   if (bM != null) { var l = bM.Find("Label"); if (l != null) { var x = l.GetComponent<TextMeshProUGUI>(); if (x != null) x.text = menuLabel; } }

        ApplySprites();
        ApplyLayout();
        ApplyTextStyle();
    }

    Transform FindChild(string name)
    {
        if (panelRoot != null) return panelRoot.transform.Find(name);
        if (panelRootRef != null) return panelRootRef.transform.Find(name);
        return null;
    }

    void SetText(TextMeshProUGUI refTmp, string fallbackName, string val)
    {
        // Ikon telepon 📞 tidak ada di font aktif → tampil kotak kosong. Ganti
        // dengan bullet untuk blok Nomor Darurat (judul/tips tak terpengaruh).
        if (fallbackName == "Emergency") val = BersihkanIkonDarurat(val);
        if (refTmp != null) { refTmp.text = val; return; }
        var t = FindChild(fallbackName);
        if (t != null) { var x = t.GetComponent<TextMeshProUGUI>(); if (x != null) x.text = val; }
    }

    void ApplySprites()
    {
        if (panelBgSprite != null)
        {
            var img = (panelRootRef != null) ? panelRootRef.GetComponent<Image>()
                : (panelRoot != null ? panelRoot.GetComponent<Image>() : null);
            if (img != null) SetSprite(img, panelBgSprite);
        }
        if (overlayBgSprite != null)
        {
            Image ov = overlayImageRef;
            if (ov == null && uiRoot != null) { var t = uiRoot.transform.Find("Overlay"); if (t != null) ov = t.GetComponent<Image>(); }
            if (ov != null) SetSprite(ov, overlayBgSprite);
        }
        if (resumeButtonSprite != null)
        {
            Image img = (btnResumeRef != null) ? btnResumeRef.GetComponent<Image>() : null;
            if (img == null) { var t = FindChild("BtnResume"); if (t != null) img = t.GetComponent<Image>(); }
            if (img != null) SetSprite(img, resumeButtonSprite);
        }
        if (menuButtonSprite != null)
        {
            Image img = (btnMenuRef != null) ? btnMenuRef.GetComponent<Image>() : null;
            if (img == null) { var t = FindChild("BtnMenu"); if (t != null) img = t.GetComponent<Image>(); }
            if (img != null) SetSprite(img, menuButtonSprite);
        }
        ApplySpriteSizes();
    }

    void SetSprite(Image img, Sprite sp)
    {
        img.sprite = sp; img.type = spriteImageType;
        if (!tintSpriteWithColor) img.color = Color.white;
    }

    void ApplySpriteSizes()
    {
        if (!overrideSpriteSize) return;
        RectTransform panelRT = (panelRootRef != null) ? panelRootRef.GetComponent<RectTransform>()
            : (panelRoot != null ? panelRoot.GetComponent<RectTransform>() : null);
        if (panelRT != null)
        {
            DisableLayout(panelRT);
            if (dockBottomLeft)
            {
                panelRT.anchorMin = new Vector2(0f, 0f);
                panelRT.anchorMax = new Vector2(0f, 0f);
                panelRT.pivot     = new Vector2(0f, 0f);
                panelRT.anchoredPosition = dockMargin;
            }
            else
            {
                panelRT.anchorMin = new Vector2(0.5f, 0.5f);
                panelRT.anchorMax = new Vector2(0.5f, 0.5f);
                panelRT.pivot     = new Vector2(0.5f, 0.5f);
                panelRT.anchoredPosition = panelAnchoredPos;
            }
            panelRT.sizeDelta  = panelSize;
            panelRT.localScale = Vector3.one * panelScale;
        }

        RectTransform ovRT = (overlayImageRef != null) ? overlayImageRef.GetComponent<RectTransform>() : null;
        if (ovRT == null && uiRoot != null) { var t = uiRoot.transform.Find("Overlay"); if (t != null) ovRT = t as RectTransform; }
        if (ovRT != null)
        {
            if (overlayStretchFullscreen)
            {
                ovRT.anchorMin = Vector2.zero; ovRT.anchorMax = Vector2.one;
                ovRT.offsetMin = Vector2.zero; ovRT.offsetMax = Vector2.zero;
            }
            else
            {
                ovRT.anchorMin = new Vector2(0.5f, 0.5f); ovRT.anchorMax = new Vector2(0.5f, 0.5f);
                ovRT.pivot = new Vector2(0.5f, 0.5f);
                ovRT.anchoredPosition = Vector2.zero; ovRT.sizeDelta = overlaySize;
            }
        }

        RectTransform rR = (btnResumeRef != null) ? btnResumeRef.GetComponent<RectTransform>() : null;
        if (rR == null) { var t = FindChild("BtnResume"); if (t != null) rR = t as RectTransform; }
        if (rR != null) rR.localScale = Vector3.one * resumeButtonScale;

        RectTransform mR = (btnMenuRef != null) ? btnMenuRef.GetComponent<RectTransform>() : null;
        if (mR == null) { var t = FindChild("BtnMenu"); if (t != null) mR = t as RectTransform; }
        if (mR != null) mR.localScale = Vector3.one * menuButtonScale;
    }

    void ApplyLayout()
    {
        if (overrideButtonLayout)
        {
            ApplyBtnRect(btnResumeRef, "BtnResume", resumeButtonAnchoredPos, resumeButtonSize);
            ApplyBtnRect(btnMenuRef,   "BtnMenu",   menuButtonAnchoredPos,   menuButtonSize);
        }
        if (overrideTextLayout)
        {
            ApplyTextRect(titleTMPRef,     "Title",     titleAnchoredPos,     titleSize);
            ApplyTextRect(tipsTMPRef,      "Tips",      tipsAnchoredPos,      tipsSize);
            ApplyTextRect(emergencyTMPRef, "Emergency", emergencyAnchoredPos, emergencySize);
        }
    }

    void ApplyBtnRect(Button btnRef, string fallback, Vector2 pos, Vector2 size)
    {
        RectTransform rt = (btnRef != null) ? btnRef.GetComponent<RectTransform>() : null;
        if (rt == null) { var t = FindChild(fallback); if (t != null) rt = t as RectTransform; }
        if (rt != null)
        {
            // Enforce min touch target untuk mobile
            Vector2 finalSize = new Vector2(
                Mathf.Max(size.x, minTouchTargetPx),
                Mathf.Max(size.y, minTouchTargetPx));
            DisableLayout(rt); ApplyRect(rt, pos, finalSize);
        }
    }

    void ApplyTextRect(TextMeshProUGUI refTmp, string fallback, Vector2 pos, Vector2 size)
    {
        RectTransform rt = (refTmp != null) ? refTmp.rectTransform : null;
        if (rt == null) { var t = FindChild(fallback); if (t != null) rt = t as RectTransform; }
        if (rt != null) { DisableLayout(rt); ApplyRect(rt, pos, size); }
    }

    void ApplyRect(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
    }

    void DisableLayout(RectTransform child)
    {
        if (!autoDisableLayoutGroup) return;
        var parent = child.parent;
        if (parent != null)
        {
            var lg = parent.GetComponent<LayoutGroup>(); if (lg != null && lg.enabled) lg.enabled = false;
            var fit = parent.GetComponent<ContentSizeFitter>(); if (fit != null && fit.enabled) fit.enabled = false;
        }
        var le = child.GetComponent<LayoutElement>(); if (le != null) le.ignoreLayout = true;
    }

    void ApplyTextStyle()
    {
        ApplyOne(titleTMPRef, "Title", titleFontAsset,
                 titleFontSize, titleFontStyle, titleAlignment, 0f, 0f,
                 titleUseOutline, titleOutlineColor, titleOutlineWidth, titleColor);

        ApplyOne(tipsTMPRef, "Tips", bodyFontAsset,
                 tipsFontSize, tipsFontStyle, tipsAlignment, 0f, tipsLineSpacing,
                 tipsUseOutline, tipsOutlineColor, tipsOutlineWidth, tipsColor);

        ApplyOne(emergencyTMPRef, "Emergency", bodyFontAsset,
                 emergencyFontSize, emergencyFontStyle, emergencyAlignment, 0f, 0f,
                 emergencyUseOutline, emergencyOutlineColor, emergencyOutlineWidth, emergencyColor);

        ApplyButtonLabel(btnResumeLabelRef, "BtnResume");
        ApplyButtonLabel(btnMenuLabelRef,   "BtnMenu");
    }

    void ApplyButtonLabel(TextMeshProUGUI refLbl, string parentName)
    {
        TextMeshProUGUI lbl = refLbl;
        if (lbl == null) { var p = FindChild(parentName); if (p != null) { var l = p.Find("Label"); if (l != null) lbl = l.GetComponent<TextMeshProUGUI>(); } }
        if (lbl == null) return;
        if (buttonFontAsset != null) lbl.font = buttonFontAsset;
        else if (fontAsset != null) lbl.font = fontAsset;
        lbl.color = btnTextColor;
        if (!overrideTextStyle) return;
        lbl.fontSize = buttonFontSize; lbl.fontStyle = buttonFontStyle; lbl.alignment = buttonAlignment;
        if (buttonUseOutline) { lbl.outlineColor = buttonOutlineColor; lbl.outlineWidth = buttonOutlineWidth; }
        else lbl.outlineWidth = 0f;
    }

    void ApplyOne(TextMeshProUGUI refTmp, string fallback, TMP_FontAsset specificFont,
                  float size, FontStyles style, TextAlignmentOptions align,
                  float charSpace, float lineSpace,
                  bool useOutline, Color outlineCol, float outlineWidth, Color baseColor)
    {
        TextMeshProUGUI tmp = refTmp;
        if (tmp == null) { var t = FindChild(fallback); if (t != null) tmp = t.GetComponent<TextMeshProUGUI>(); }
        if (tmp == null) return;
        if (specificFont != null) tmp.font = specificFont;
        else if (fontAsset != null) tmp.font = fontAsset;
        tmp.color = baseColor;
        if (!overrideTextStyle) return;
        tmp.fontSize = size; tmp.fontStyle = style; tmp.alignment = align;
        tmp.characterSpacing = charSpace; tmp.lineSpacing = lineSpace;
        if (useOutline) { tmp.outlineColor = outlineCol; tmp.outlineWidth = outlineWidth; }
        else tmp.outlineWidth = 0f;
    }

    [ContextMenu("▶ Test: Buka Pause")]
    public void TestOpen() { if (Application.isPlaying) OpenPanel(); }

    [ContextMenu("▶ Test: Tutup Pause")]
    public void TestClose() { ClosePanel(); }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying || !liveEdit) return;
        UnityEditor.EditorApplication.delayCall += () => { if (this != null) ReapplyAllCustomization(); };
    }
#endif
}
