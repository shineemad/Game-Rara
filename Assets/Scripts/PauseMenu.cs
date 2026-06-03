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
    // INTERNAL
    // ══════════════════════════════════════════════════════════════════════
    Canvas canvas;
    GameObject uiRoot;
    GameObject panelRoot;
    GameObject mobileBtnRoot;
    Button     mobilePauseBtn;
    bool usingEditorRefs = false;
    bool isOpen = false;
    float prevTimeScale = 1f;
    bool builtOnce = false;

    void Start()
    {
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
        else                                  img.color = new Color(0f, 0f, 0f, 0.5f);

        mobilePauseBtn = mobileBtnRoot.AddComponent<Button>();
        mobilePauseBtn.onClick.AddListener(Toggle);

        // Icon "❚❚"
        if (mobilePauseButtonSprite == null)
        {
            var iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(mobileBtnRoot.transform, false);
            var irt = iconGO.AddComponent<RectTransform>();
            irt.anchorMin = Vector2.zero; irt.anchorMax = Vector2.one;
            irt.offsetMin = Vector2.zero; irt.offsetMax = Vector2.zero;
            var icon = iconGO.AddComponent<TextMeshProUGUI>();
            ApplyFont(icon);
            icon.text = "❚❚";
            icon.fontSize = mobilePauseButtonSize.y * 0.5f;
            icon.color = Color.white;
            icon.alignment = TextAlignmentOptions.Center;
            icon.fontStyle = FontStyles.Bold;
        }
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
        bool shouldShow = showMobilePauseButton && !isOpen;
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
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene(mainMenuSceneName);
            else UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
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
        canvas.sortingOrder = 800;
        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
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
        var outl = panelRoot.AddComponent<Outline>(); outl.effectColor = borderColor;
        outl.effectDistance = new Vector2(4f, -4f);

        MakeText(panelRoot, "Title", new Vector2(0f, 1f), new Vector2(1f, 1f),
                 new Vector2(10f, -80f), new Vector2(-10f, -10f), 44, titleColor,
                 TextAlignmentOptions.Center, true, titleText);

        MakeText(panelRoot, "Tips", new Vector2(0f, 1f), new Vector2(1f, 1f),
                 new Vector2(20f, -360f), new Vector2(-20f, -90f), 26, tipsColor,
                 TextAlignmentOptions.TopLeft, false, tipsText);

        MakeText(panelRoot, "Emergency", new Vector2(0f, 1f), new Vector2(1f, 1f),
                 new Vector2(10f, -480f), new Vector2(-10f, -370f), 28, emergencyColor,
                 TextAlignmentOptions.Center, true, emergencyText);

        MakeButton(panelRoot, "BtnResume", resumeLabel,
                   new Vector2(0.08f, 0.13f), new Vector2(0.92f, 0.24f),
                   resumeColor, OnResumeClicked);
        MakeButton(panelRoot, "BtnMenu", menuLabel,
                   new Vector2(0.08f, 0.02f), new Vector2(0.92f, 0.12f),
                   menuColor, OnMenuClicked);

        if (debugLog) Debug.Log("[PauseMenu] Mode B aktif.");
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

    // ══════════════════════════════════════════════════════════════════════
    // APPLY
    // ══════════════════════════════════════════════════════════════════════
    [ContextMenu("▶ Apply Customization Sekarang")]
    public void ReapplyAllCustomization()
    {
        SetText(titleTMPRef,     "Title",     titleText);
        SetText(tipsTMPRef,      "Tips",      tipsText);
        SetText(emergencyTMPRef, "Emergency", emergencyText);
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
