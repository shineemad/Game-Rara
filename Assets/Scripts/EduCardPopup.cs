using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// EduCardPopup — kartu edukasi yang muncul di akhir tiap hari.
/// Berisi judul kuning + daftar 3 tips dengan ikon (🚩 ✅ ♡) dan tombol Lanjutkan.
///
/// ═══════════════════════════════════════════════════════════════════
/// MODE A — UI dibuat di Editor:
///   EduCardCanvas
///   └─ UIRoot (RectTransform stretch)
///      ├─ Overlay (Image fullscreen gelap)
///      └─ Panel (Image cokelat tua)
///         ├─ Title (TMP)
///         ├─ Tip1 (TMP)  ← format "🚩 isi tip..."
///         ├─ Tip2 (TMP)
///         ├─ Tip3 (TMP)
///         └─ BtnContinue (Button) + Label (TMP)
///   Drag semua referensi di Inspector.
///
/// MODE B — Programatik (fallback): semua dibuat otomatis jika ref kosong.
/// ═══════════════════════════════════════════════════════════════════
/// </summary>
public class EduCardPopup : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — UI REFERENSI (Editor-Built)
    // ══════════════════════════════════════════════════════════════════════
    [Header("── UI REFERENSI (Editor-Built) ──")]
    public GameObject       uiRootRef;
    public Image            overlayImageRef;
    public GameObject       panelRootRef;
    public TextMeshProUGUI  titleTMPRef;
    public TextMeshProUGUI  tip1TMPRef;
    public TextMeshProUGUI  tip2TMPRef;
    public TextMeshProUGUI  tip3TMPRef;
    public Button           btnContinueRef;
    public TextMeshProUGUI  btnContinueLabelRef;

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — KONTEN TEKS
    // ══════════════════════════════════════════════════════════════════════
    [Header("── KONTEN TEKS ──")]
    public string titleText        = "TIPS KESELAMATAN";
    [TextArea(1, 3)] public string tip1Text = "🚩 Jangan ikut orang asing meski diberi hadiah.";
    [TextArea(1, 3)] public string tip2Text = "✅ Pilih jalan ramai daripada gang sepi.";
    [TextArea(1, 3)] public string tip3Text = "♡ Cerita pada orang dewasa yang kamu percaya.";
    public string continueLabel    = "LANJUTKAN";

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — WARNA (Mode B / Fallback)
    // ══════════════════════════════════════════════════════════════════════
    [Header("── WARNA (Mode B / Fallback) ──")]
    public Color panelBgColor      = new Color(0.30f, 0.18f, 0.10f, 0.98f); // cokelat tua
    public Color borderColor       = new Color(1f, 0.85f, 0.1f, 1f);
    public Color titleColor        = new Color(1f, 0.85f, 0.1f, 1f);
    public Color tipColor          = Color.white;
    public Color continueColor     = new Color(0.15f, 0.60f, 0.20f, 1f);
    public Color btnTextColor      = Color.white;
    [Range(0.3f, 1f)]  public float panelWidthRatio  = 0.80f;
    [Range(0.3f, 0.9f)] public float panelHeightRatio = 0.65f;

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — FONT
    // ══════════════════════════════════════════════════════════════════════
    [Header("── FONT (opsional) ──")]
    public TMP_FontAsset fontAsset;
    public TMP_FontAsset titleFontAsset;
    public TMP_FontAsset tipFontAsset;
    public TMP_FontAsset buttonFontAsset;

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — KUSTOMISASI TEKS
    // ══════════════════════════════════════════════════════════════════════
    [Header("── KUSTOMISASI TEKS (centang untuk override) ──")]
    public bool overrideTextStyle = false;

    [Header("   Judul")]
    public float                titleFontSize        = 56f;
    public FontStyles           titleFontStyle       = FontStyles.Bold;
    public TextAlignmentOptions titleAlignment       = TextAlignmentOptions.Center;
    public bool                 titleUseOutline      = false;
    public Color                titleOutlineColor    = Color.black;
    [Range(0f, 1f)] public float titleOutlineWidth   = 0.2f;

    [Header("   Tips")]
    public float                tipFontSize          = 32f;
    public FontStyles           tipFontStyle         = FontStyles.Normal;
    public TextAlignmentOptions tipAlignment         = TextAlignmentOptions.Left;
    [Range(-50f, 100f)] public float tipLineSpacing  = 10f;
    public bool                 tipUseOutline        = false;
    public Color                tipOutlineColor      = Color.black;
    [Range(0f, 1f)] public float tipOutlineWidth     = 0.15f;

    [Header("   Tombol")]
    public float                buttonFontSize       = 42f;
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
    public Sprite continueButtonSprite;
    public Sprite overlayBgSprite;
    public Image.Type spriteImageType = Image.Type.Sliced;
    public bool tintSpriteWithColor = false;

    [Header("   Ukuran Sprite (centang untuk override)")]
    public bool   overrideSpriteSize = false;
    public Vector2 panelSize          = new Vector2(900f, 700f);
    public Vector2 panelAnchoredPos   = Vector2.zero;
    [Range(0.1f, 5f)] public float panelScale = 1f;

    public Vector2 overlaySize        = new Vector2(1920f, 1080f);
    public bool overlayStretchFullscreen = true;

    [Range(0.1f, 5f)] public float continueButtonScale = 1f;

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — POSISI & UKURAN TOMBOL
    // ══════════════════════════════════════════════════════════════════════
    [Header("── POSISI & UKURAN TOMBOL (centang untuk override) ──")]
    public bool    overrideButtonLayout      = false;
    public Vector2 continueButtonAnchoredPos = new Vector2(0f, -260f);
    public Vector2 continueButtonSize        = new Vector2(500f, 120f);

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — POSISI ELEMEN TEKS (centang untuk override)
    // ══════════════════════════════════════════════════════════════════════
    [Header("── POSISI TEKS (centang untuk override) ──")]
    public bool    overrideTextLayout       = false;
    public Vector2 titleAnchoredPos         = new Vector2(0f, 250f);
    public Vector2 titleSize                = new Vector2(800f, 90f);
    public Vector2 tip1AnchoredPos          = new Vector2(0f, 120f);
    public Vector2 tip2AnchoredPos          = new Vector2(0f, 30f);
    public Vector2 tip3AnchoredPos          = new Vector2(0f, -60f);
    public Vector2 tipSize                  = new Vector2(780f, 70f);

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — LIVE EDIT
    // ══════════════════════════════════════════════════════════════════════
    [Header("── LIVE EDIT (saat Play) ──")]
    public bool liveEdit              = true;
    public bool previewPanelInPlay    = false;
    public bool autoDisableLayoutGroup = true;
    public bool debugLog              = false;

    [Header("── MOBILE ──")]
    [Tooltip("Hormati Safe Area (notch / punch hole) di mobile.")]
    public bool  useSafeArea       = true;
    [Tooltip("Minimum touch target untuk tombol (px). Default 96 untuk anak.")]
    public float minTouchTargetPx  = 96f;
    [Tooltip("Auto-scale panel agar tidak melebihi layar mobile. 0.9 = 90% lebar layar.")]
    [Range(0.5f, 1f)] public float maxPanelWidthScreenRatio = 0.92f;
    [Range(0.5f, 1f)] public float maxPanelHeightScreenRatio = 0.85f;

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — EVENTS
    // ══════════════════════════════════════════════════════════════════════
    [Header("── EVENTS ──")]
    public UnityEngine.Events.UnityEvent onContinue;

    // ══════════════════════════════════════════════════════════════════════
    // INTERNAL
    // ══════════════════════════════════════════════════════════════════════
    Canvas      canvas;
    GameObject  uiRoot;
    GameObject  panelRoot;
    bool        usingEditorRefs = false;
    bool        builtOnce       = false;
    Action      pendingOnClose;

    void Start()
    {
        if (uiRootRef != null && panelRootRef != null && btnContinueRef != null)
        {
            usingEditorRefs = true;
            SetupEditorRefs();
        }
        else
        {
            usingEditorRefs = false;
            BuildUI();
        }
        HidePanel();
        builtOnce = true;
    }

    void Update()
    {
        if (!Application.isPlaying || !builtOnce) return;

        ApplySafeAreaIfNeeded();
        if (previewPanelInPlay) ForceShowPanelForPreview();
        if (liveEdit)           ReapplyAllCustomization();
    }

    // ══════ Safe Area (notch) ══════
    Rect lastSafeArea = Rect.zero;
    void ApplySafeAreaIfNeeded()
    {
        if (!useSafeArea) return;
        Rect sa = Screen.safeArea;
        if (sa == lastSafeArea) return;
        lastSafeArea = sa;

        // Clamp panel size jika overrideSpriteSize aktif
        if (overrideSpriteSize)
        {
            float maxW = sa.width  * maxPanelWidthScreenRatio;
            float maxH = sa.height * maxPanelHeightScreenRatio;
            panelSize = new Vector2(
                Mathf.Min(panelSize.x, maxW),
                Mathf.Min(panelSize.y, maxH));
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // API PUBLIK
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Tampilkan kartu edukasi. Callback dipanggil saat user tekan Lanjutkan.</summary>
    public void Show(Action onClose = null)
    {
        pendingOnClose = onClose;
        if (usingEditorRefs)
        {
            uiRootRef.SetActive(true);
            panelRootRef.SetActive(true);
        }
        else
        {
            uiRoot.SetActive(true);
            panelRoot.SetActive(true);
        }
        ReapplyAllCustomization();
    }

    public void Hide() => HidePanel();

    void OnContinueClicked()
    {
        HidePanel();
        pendingOnClose?.Invoke();
        pendingOnClose = null;
        onContinue?.Invoke();
    }

    void HidePanel()
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
    }

    void ForceShowPanelForPreview()
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
        btnContinueRef.onClick.RemoveAllListeners();
        btnContinueRef.onClick.AddListener(OnContinueClicked);
        ReapplyAllCustomization();
        if (debugLog) Debug.Log("[EduCardPopup] Mode A aktif.");
    }

    // ══════════════════════════════════════════════════════════════════════
    // BUILD — Mode B Programatik
    // ══════════════════════════════════════════════════════════════════════
    void BuildUI()
    {
        var cGO = new GameObject("EduCardCanvas");
        canvas = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 750;
        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight  = 0.5f;
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
        var ovImg = ovGO.AddComponent<Image>();
        ovImg.color = new Color(0f, 0f, 0f, 0.6f);

        panelRoot = new GameObject("Panel");
        panelRoot.transform.SetParent(uiRoot.transform, false);
        var pRT = panelRoot.AddComponent<RectTransform>();
        float pw = (1f - panelWidthRatio) * 0.5f;
        float ph = (1f - panelHeightRatio) * 0.5f;
        pRT.anchorMin = new Vector2(pw, ph);
        pRT.anchorMax = new Vector2(1f - pw, 1f - ph);
        pRT.offsetMin = Vector2.zero; pRT.offsetMax = Vector2.zero;
        var pImg = panelRoot.AddComponent<Image>();
        pImg.color = panelBgColor;
        var outl = panelRoot.AddComponent<Outline>();
        outl.effectColor = borderColor;
        outl.effectDistance = new Vector2(6f, -6f);

        MakeText(panelRoot, "Title", new Vector2(0f, 1f), new Vector2(1f, 1f),
                 new Vector2(20f, -110f), new Vector2(-20f, -20f),
                 56, titleColor, TextAlignmentOptions.Center, true);

        MakeText(panelRoot, "Tip1", new Vector2(0f, 1f), new Vector2(1f, 1f),
                 new Vector2(40f, -200f), new Vector2(-40f, -120f),
                 32, tipColor, TextAlignmentOptions.Left, false);
        MakeText(panelRoot, "Tip2", new Vector2(0f, 1f), new Vector2(1f, 1f),
                 new Vector2(40f, -290f), new Vector2(-40f, -210f),
                 32, tipColor, TextAlignmentOptions.Left, false);
        MakeText(panelRoot, "Tip3", new Vector2(0f, 1f), new Vector2(1f, 1f),
                 new Vector2(40f, -380f), new Vector2(-40f, -300f),
                 32, tipColor, TextAlignmentOptions.Left, false);

        MakeButton(panelRoot, "BtnContinue", continueLabel,
                   new Vector2(0.20f, 0.05f), new Vector2(0.80f, 0.18f),
                   continueColor, OnContinueClicked);

        if (debugLog) Debug.Log("[EduCardPopup] Mode B aktif.");
    }

    TextMeshProUGUI MakeText(GameObject parent, string name,
        Vector2 ancMin, Vector2 ancMax, Vector2 offMin, Vector2 offMax,
        int fontSize, Color color, TextAlignmentOptions align, bool bold)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.pivot = new Vector2(0.5f, 1f);
        rt.offsetMin = offMin; rt.offsetMax = offMax;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        ApplyFont(tmp);
        tmp.fontSize = fontSize; tmp.color = color; tmp.alignment = align;
        if (bold) tmp.fontStyle = FontStyles.Bold;
        if (name == "Title") tmp.text = titleText;
        else if (name == "Tip1") tmp.text = tip1Text;
        else if (name == "Tip2") tmp.text = tip2Text;
        else if (name == "Tip3") tmp.text = tip3Text;
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
        var img = go.AddComponent<Image>();
        img.color = color;
        var btn = go.AddComponent<Button>();
        btn.onClick.AddListener(() => onClick());

        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(go.transform, false);
        var lrt = lblGO.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = new Vector2(12f, 8f); lrt.offsetMax = new Vector2(-12f, -8f);
        var lbl = lblGO.AddComponent<TextMeshProUGUI>();
        ApplyFont(lbl);
        lbl.text = label; lbl.fontSize = 42; lbl.color = btnTextColor;
        lbl.alignment = TextAlignmentOptions.Center; lbl.fontStyle = FontStyles.Bold;
    }

    void ApplyFont(TextMeshProUGUI tmp)
    {
        var f = fontAsset;
        if (f == null) f = TMP_Settings.defaultFontAsset;
        if (f != null) tmp.font = f;
    }

    // ══════════════════════════════════════════════════════════════════════
    // APPLY — Sprite, Layout, Text Style
    // ══════════════════════════════════════════════════════════════════════
    [ContextMenu("▶ Apply Customization Sekarang")]
    public void ReapplyAllCustomization()
    {
        // Teks dasar
        SetText(titleTMPRef,        "Title",       titleText);
        SetText(tip1TMPRef,         "Tip1",        tip1Text);
        SetText(tip2TMPRef,         "Tip2",        tip2Text);
        SetText(tip3TMPRef,         "Tip3",        tip3Text);
        if (btnContinueLabelRef != null) btnContinueLabelRef.text = continueLabel;
        var bC = FindChild("BtnContinue"); if (bC != null) { var l = bC.Find("Label");
            if (l != null) { var x = l.GetComponent<TextMeshProUGUI>(); if (x != null) x.text = continueLabel; } }

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
        // Panel
        if (panelBgSprite != null)
        {
            var img = (panelRootRef != null) ? panelRootRef.GetComponent<Image>()
                : (panelRoot != null ? panelRoot.GetComponent<Image>() : null);
            if (img != null) SetSprite(img, panelBgSprite);
        }
        // Overlay
        if (overlayBgSprite != null)
        {
            Image ov = overlayImageRef;
            if (ov == null && uiRoot != null)
            {
                var t = uiRoot.transform.Find("Overlay");
                if (t != null) ov = t.GetComponent<Image>();
            }
            if (ov != null) SetSprite(ov, overlayBgSprite);
        }
        // Tombol
        if (continueButtonSprite != null)
        {
            Image img = (btnContinueRef != null) ? btnContinueRef.GetComponent<Image>() : null;
            if (img == null) { var t = FindChild("BtnContinue"); if (t != null) img = t.GetComponent<Image>(); }
            if (img != null) SetSprite(img, continueButtonSprite);
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
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.pivot     = new Vector2(0.5f, 0.5f);
            panelRT.anchoredPosition = panelAnchoredPos;
            panelRT.sizeDelta        = panelSize;
            panelRT.localScale       = Vector3.one * panelScale;
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

        RectTransform cRT = (btnContinueRef != null) ? btnContinueRef.GetComponent<RectTransform>() : null;
        if (cRT == null) { var t = FindChild("BtnContinue"); if (t != null) cRT = t as RectTransform; }
        if (cRT != null) cRT.localScale = Vector3.one * continueButtonScale;
    }

    void ApplyLayout()
    {
        if (overrideButtonLayout)
        {
            RectTransform cRT = (btnContinueRef != null) ? btnContinueRef.GetComponent<RectTransform>() : null;
            if (cRT == null) { var t = FindChild("BtnContinue"); if (t != null) cRT = t as RectTransform; }
            if (cRT != null)
            {
                Vector2 sz = new Vector2(
                    Mathf.Max(continueButtonSize.x, minTouchTargetPx),
                    Mathf.Max(continueButtonSize.y, minTouchTargetPx));
                DisableLayout(cRT); ApplyRect(cRT, continueButtonAnchoredPos, sz);
            }
        }
        if (overrideTextLayout)
        {
            ApplyTextRect(titleTMPRef, "Title", titleAnchoredPos, titleSize);
            ApplyTextRect(tip1TMPRef,  "Tip1",  tip1AnchoredPos,  tipSize);
            ApplyTextRect(tip2TMPRef,  "Tip2",  tip2AnchoredPos,  tipSize);
            ApplyTextRect(tip3TMPRef,  "Tip3",  tip3AnchoredPos,  tipSize);
        }
    }

    void ApplyTextRect(TextMeshProUGUI refTmp, string fallbackName, Vector2 pos, Vector2 size)
    {
        RectTransform rt = (refTmp != null) ? refTmp.rectTransform : null;
        if (rt == null) { var t = FindChild(fallbackName); if (t != null) rt = t as RectTransform; }
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

        ApplyOne(tip1TMPRef, "Tip1", tipFontAsset,
                 tipFontSize, tipFontStyle, tipAlignment, 0f, tipLineSpacing,
                 tipUseOutline, tipOutlineColor, tipOutlineWidth, tipColor);
        ApplyOne(tip2TMPRef, "Tip2", tipFontAsset,
                 tipFontSize, tipFontStyle, tipAlignment, 0f, tipLineSpacing,
                 tipUseOutline, tipOutlineColor, tipOutlineWidth, tipColor);
        ApplyOne(tip3TMPRef, "Tip3", tipFontAsset,
                 tipFontSize, tipFontStyle, tipAlignment, 0f, tipLineSpacing,
                 tipUseOutline, tipOutlineColor, tipOutlineWidth, tipColor);

        // Tombol label
        TextMeshProUGUI lbl = btnContinueLabelRef;
        if (lbl == null) { var bC = FindChild("BtnContinue"); if (bC != null) { var l = bC.Find("Label"); if (l != null) lbl = l.GetComponent<TextMeshProUGUI>(); } }
        if (lbl != null)
        {
            if (buttonFontAsset != null) lbl.font = buttonFontAsset;
            else if (fontAsset != null)  lbl.font = fontAsset;
            lbl.color = btnTextColor;
            if (overrideTextStyle)
            {
                lbl.fontSize = buttonFontSize;
                lbl.fontStyle = buttonFontStyle;
                lbl.alignment = buttonAlignment;
                if (buttonUseOutline) { lbl.outlineColor = buttonOutlineColor; lbl.outlineWidth = buttonOutlineWidth; }
                else lbl.outlineWidth = 0f;
            }
        }
    }

    void ApplyOne(TextMeshProUGUI refTmp, string fallbackName, TMP_FontAsset specificFont,
                  float size, FontStyles style, TextAlignmentOptions align,
                  float charSpace, float lineSpace,
                  bool useOutline, Color outlineCol, float outlineWidth, Color baseColor)
    {
        TextMeshProUGUI tmp = refTmp;
        if (tmp == null) { var t = FindChild(fallbackName); if (t != null) tmp = t.GetComponent<TextMeshProUGUI>(); }
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

    [ContextMenu("▶ Test: Tampilkan EduCard")]
    public void TestShow() { if (Application.isPlaying) Show(null); }

    [ContextMenu("▶ Test: Sembunyikan")]
    public void TestHide() { HidePanel(); }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying || !liveEdit) return;
        UnityEditor.EditorApplication.delayCall += () => { if (this != null) ReapplyAllCustomization(); };
    }
#endif
}
