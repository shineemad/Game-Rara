using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// PathChoiceUI — panel pilihan jalur "Jalan Ramai vs Gang Sepi".
///
/// ═══════════════════════════════════════════════════════
/// MODE A — UI dibuat di Editor (DIREKOMENDASIKAN):
/// ═══════════════════════════════════════════════════════
///   1. Buat Canvas "PathChoiceCanvas" (Screen Space – Overlay, sortOrder 700).
///   2. Di dalam Canvas, buat struktur:
///
///      PathChoiceCanvas
///      └─ UIRoot                      ← RectTransform stretch penuh, SetActive false
///         ├─ Overlay                  ← Image hitam semi-transparan, stretch penuh
///         └─ Panel                    ← Image gelap, size ~75% × 55% layar, di tengah
///            ├─ Title (TMP)           ← judul kuning, bold
///            ├─ Body (TMP)            ← deskripsi putih
///            ├─ BtnSafe (Button)      ← tombol hijau + Label (TMP)
///            └─ BtnDanger (Button)    ← tombol merah + Label (TMP)
///
///   3. Drag referensi ke field di bawah header "── UI REFERENSI (Editor-Built) ──".
///   4. Drag Transform Rara ke "Player Transform".
///   5. Sambungkan onSafeChosen / onDangerChosen ke Day1Controller.
///
/// MODE B — Fallback Programatik (jika referensi tidak di-assign):
///   Jalankan saja — UI dibuat otomatis saat runtime (tidak bisa diedit di Editor).
/// ═══════════════════════════════════════════════════════
/// </summary>
public class PathChoiceUI : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — TRIGGER
    // ══════════════════════════════════════════════════════════════════════

    [Header("── TRIGGER ──")]
    [Tooltip("Drag Transform Rara ke sini")]
    public Transform playerTransform;
    [Tooltip("Jarak (unit) dari tiang agar panel muncul")]
    public float triggerDistance = 2.5f;
    [Tooltip("Hanya tampil sekali (true) atau setiap kali Rara lewat (false)")]
    public bool  triggerOnce = true;

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — UI REFERENSI (Editor-Built)
    // Isi bagian ini jika kamu membuat UI manual di Unity Editor.
    // Jika semua dibiarkan kosong, UI dibuat otomatis (Mode B).
    // ══════════════════════════════════════════════════════════════════════

    [Header("── UI REFERENSI (Editor-Built) ──")]
    [Tooltip("GameObject induk yang berisi Overlay + Panel. Di-hide/show saat trigger.")]
    public GameObject uiRootRef;

    [Tooltip("Image overlay gelap (boleh dikosongkan)")]
    public Image overlayImageRef;

    [Tooltip("GameObject panel utama (Image latar panel)")]
    public GameObject panelRootRef;

    [Tooltip("TextMeshProUGUI judul panel (contoh: '⚠ ADA DUA JALUR!')")]
    public TextMeshProUGUI titleTMPRef;

    [Tooltip("TextMeshProUGUI deskripsi / isi panel")]
    public TextMeshProUGUI bodyTMPRef;

    [Tooltip("Button Jalan Ramai (hijau)")]
    public Button btnSafeRef;

    [Tooltip("Label TMP di dalam BtnSafe")]
    public TextMeshProUGUI btnSafeLabelRef;

    [Tooltip("Button Gang Sepi (merah)")]
    public Button btnDangerRef;

    [Tooltip("Label TMP di dalam BtnDanger")]
    public TextMeshProUGUI btnDangerLabelRef;

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — KONTEN & WARNA (berlaku di kedua mode)
    // ══════════════════════════════════════════════════════════════════════

    [Header("── KONTEN TEKS ──")]
    public string titleText   = "⚠  ADA DUA JALUR!";
    [TextArea(2, 4)]
    public string bodyText    = "Rara harus pilih jalur ke sekolah.\nMana yang menurutmu lebih aman buat Rara?";
    public string safeLabel   = "🏛  Jalan Ramai\n(aman, banyak orang)";
    public string dangerLabel = "🔴  Gang Sepi\n(lebih cepat, tapi... bahaya!)";

    [Header("── WARNA (Mode B / Fallback Programatik) ──")]
    [Tooltip("Warna latar panel (hanya berlaku jika UI dibuat otomatis)")]
    public Color  panelBgColor   = new Color(0.22f, 0.03f, 0.03f, 0.96f);
    public Color  borderColor    = new Color(1f, 0.85f, 0.1f, 1f);
    public Color  titleColor     = new Color(1f, 0.85f, 0.1f, 1f);
    public Color  bodyColor      = Color.white;
    public Color  safeColor      = new Color(0.15f, 0.60f, 0.20f, 1f);
    public Color  dangerColor    = new Color(0.75f, 0.20f, 0.15f, 1f);
    public Color  btnTextColor   = Color.white;
    [Range(0.3f, 1f)]
    public float  panelWidthRatio  = 0.75f;
    [Range(0.3f, 0.9f)]
    public float  panelHeightRatio = 0.55f;

    [Header("── FONT (opsional) ──")]
    public TMP_FontAsset fontAsset;

    [Header("── EVENTS — sambungkan ke Day1Controller ──")]
    [Tooltip("Dipanggil saat pemain memilih Jalan Ramai")]
    public UnityEngine.Events.UnityEvent onSafeChosen;
    [Tooltip("Dipanggil saat pemain memilih Gang Sepi")]
    public UnityEngine.Events.UnityEvent onDangerChosen;

    // ══════════════════════════════════════════════════════════════════════
    // INTERNAL
    // ══════════════════════════════════════════════════════════════════════

    private Canvas      canvas;
    private GameObject  panelRoot;
    private GameObject  uiRoot;
    private bool        triggered = false;
    private bool        shown     = false;

    // Apakah menggunakan referensi dari Editor (true) atau programatik (false)
    private bool        usingEditorRefs = false;

    // referensi komponen Rigidbody2D Rara untuk pause gerak
    private Rigidbody2D playerRb;

    // referensi PathEnvironment — dicari otomatis, tidak perlu di-assign manual
    private PathEnvironment pathEnv;

    // ══════════════════════════════════════════════════════════════════════
    void Start()
    {
        // Auto-find Rara jika belum di-assign
        if (playerTransform == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        if (playerTransform != null)
            playerRb = playerTransform.GetComponent<Rigidbody2D>();

        // Auto-find PathEnvironment di scene
        pathEnv = FindFirstObjectByType<PathEnvironment>();
        if (pathEnv == null)
            Debug.LogWarning("[PathChoiceUI] PathEnvironment tidak ditemukan di scene — latar tidak akan berubah.");

        // Pilih mode: Editor-refs atau Programatik
        if (uiRootRef != null && panelRootRef != null && btnSafeRef != null && btnDangerRef != null)
        {
            usingEditorRefs = true;
            SetupEditorRefs();
        }
        else
        {
            usingEditorRefs = false;
            BuildUI();
        }
    }

    void Update()
    {
        if (shown) return;
        if (triggerOnce && triggered) return;
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist <= triggerDistance)
        {
            triggered = true;
            ShowPanel();
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // PANEL LOGIC
    // ══════════════════════════════════════════════════════════════════════

    void ShowPanel()
    {
        shown = true;

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

        // Bekukan Rara agar tidak jalan terus
        if (playerRb != null)
        {
            playerRb.velocity = Vector2.zero;
            playerRb.isKinematic = true;
        }
    }

    void OnSafeButton()
    {
        if (GameState.Instance != null)
            GameState.Instance.pathChoice = "safe";

        HidePanel();

        // Aktifkan tampilan Jalan Ramai langsung dari sini
        pathEnv?.AktifkanJalanRamai();

        onSafeChosen?.Invoke();
        Debug.Log("[PathChoice] Dipilih: Jalan Ramai (safe)");
    }

    void OnDangerButton()
    {
        if (GameState.Instance != null)
            GameState.Instance.pathChoice = "dangerous";

        HidePanel();

        // Aktifkan tampilan Gang Sepi langsung dari sini
        pathEnv?.AktifkanGangSepi();

        onDangerChosen?.Invoke();
        Debug.Log("[PathChoice] Dipilih: Gang Sepi (dangerous)");
    }

    void HidePanel()
    {
        if (usingEditorRefs)
        {
            panelRootRef.SetActive(false);
            uiRootRef.SetActive(false);
        }
        else
        {
            panelRoot.SetActive(false);
            uiRoot.SetActive(false);
        }

        // Bebaskan kembali Rara
        if (playerRb != null)
            playerRb.isKinematic = false;
    }

    // ══════════════════════════════════════════════════════════════════════
    // SETUP — MODE A (Editor-Built Refs)
    // Terapkan teks & hook tombol ke referensi yang sudah di-assign di Inspector.
    // ══════════════════════════════════════════════════════════════════════

    void SetupEditorRefs()
    {
        // Isi teks dari field Inspector
        if (titleTMPRef  != null) titleTMPRef.text  = titleText;
        if (bodyTMPRef   != null) bodyTMPRef.text   = bodyText;
        if (btnSafeLabelRef   != null) btnSafeLabelRef.text   = safeLabel;
        if (btnDangerLabelRef != null) btnDangerLabelRef.text = dangerLabel;

        // Daftarkan listener tombol
        btnSafeRef.onClick.RemoveAllListeners();
        btnSafeRef.onClick.AddListener(OnSafeButton);

        btnDangerRef.onClick.RemoveAllListeners();
        btnDangerRef.onClick.AddListener(OnDangerButton);

        // Sembunyikan panel saat awal (panel masih terlihat di Editor agar mudah diedit)
        panelRootRef.SetActive(false);
        uiRootRef.SetActive(false);

        Debug.Log("[PathChoiceUI] Mode A aktif — menggunakan UI dari Editor.");
    }

    // ══════════════════════════════════════════════════════════════════════
    // BUILD UI — MODE B (Programatik / Fallback)
    // ══════════════════════════════════════════════════════════════════════

    void BuildUI()
    {
        Debug.Log("[PathChoiceUI] Mode B aktif — UI dibuat secara programatik (fallback).");
        // Canvas khusus — di atas gameplay tapi di bawah dialog
        var cGO = new GameObject("PathChoiceCanvas");
        canvas = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 700;
        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // uiRoot — fullscreen container untuk overlay + panel
        // Ini yang di-hide agar overlay tidak terlihat saat panel belum muncul
        uiRoot = new GameObject("UIRoot");
        uiRoot.transform.SetParent(cGO.transform, false);
        var uiRootRT = uiRoot.AddComponent<RectTransform>();
        uiRootRT.anchorMin = Vector2.zero;
        uiRootRT.anchorMax = Vector2.one;
        uiRootRT.offsetMin = Vector2.zero;
        uiRootRT.offsetMax = Vector2.zero;

        // Overlay gelap — child uiRoot (ikut tersembunyi saat panel disembunyikan)
        var overlayGO = new GameObject("Overlay");
        overlayGO.transform.SetParent(uiRoot.transform, false);
        var overlayRT = overlayGO.AddComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.offsetMin = Vector2.zero;
        overlayRT.offsetMax = Vector2.zero;
        var overlayImg = overlayGO.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.55f);

        // Panel utama — juga child uiRoot
        panelRoot = new GameObject("Panel");
        panelRoot.transform.SetParent(uiRoot.transform, false);
        var panelRT = panelRoot.AddComponent<RectTransform>();
        float pw = (1f - panelWidthRatio)  * 0.5f;
        float ph = (1f - panelHeightRatio) * 0.5f;
        panelRT.anchorMin = new Vector2(pw, ph);
        panelRT.anchorMax = new Vector2(1f - pw, 1f - ph);
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        var panelImg = panelRoot.AddComponent<Image>();
        panelImg.color = panelBgColor;

        // Border panel (outline tebal kuning)
        var outline = panelRoot.AddComponent<Outline>();
        outline.effectColor    = borderColor;
        outline.effectDistance = new Vector2(6f, -6f);

        // ── Judul ─────────────────────────────────────────────────────────
        var titleGO = MakeText(panelRoot, "Title",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(20f, -90f), new Vector2(-20f, -12f),
            52, titleColor, TextAlignmentOptions.Center);
        titleGO.text = titleText;
        titleGO.fontStyle = FontStyles.Bold;

        // ── Deskripsi ─────────────────────────────────────────────────────
        var bodyGO = MakeText(panelRoot, "Body",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(20f, -200f), new Vector2(-20f, -98f),
            34, bodyColor, TextAlignmentOptions.Center);
        bodyGO.text = bodyText;
        bodyGO.enableWordWrapping = true;

        // ── Tombol JALAN RAMAI (hijau) ────────────────────────────────────
        MakeChoiceButton(panelRoot, "BtnSafe", safeLabel,
            new Vector2(0.05f, 0.30f), new Vector2(0.95f, 0.58f),
            safeColor, OnSafeButton);

        // ── Tombol GANG SEPI (merah) ──────────────────────────────────────
        MakeChoiceButton(panelRoot, "BtnDanger", dangerLabel,
            new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.27f),
            dangerColor, OnDangerButton);

        panelRoot.SetActive(false);
        uiRoot.SetActive(false); // overlay ikut tersembunyi
    }

    void MakeChoiceButton(GameObject parent, string name, string label,
        Vector2 ancMin, Vector2 ancMax,
        Color color, System.Action onClick)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin;
        rt.anchorMax = ancMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = go.AddComponent<Image>();
        img.color = color;

        // Rounded corner via outline
        var ol = go.AddComponent<Outline>();
        ol.effectColor    = new Color(1f, 1f, 1f, 0.3f);
        ol.effectDistance = new Vector2(3f, -3f);

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor      = color;
        colors.highlightedColor = new Color(
            Mathf.Min(color.r + 0.15f, 1f),
            Mathf.Min(color.g + 0.15f, 1f),
            Mathf.Min(color.b + 0.15f, 1f), 1f);
        colors.pressedColor = new Color(
            color.r * 0.75f, color.g * 0.75f, color.b * 0.75f, 1f);
        btn.colors = colors;
        btn.onClick.AddListener(() => onClick());

        // Label teks
        var lbl = MakeText(go, "Label",
            Vector2.zero, Vector2.one,
            new Vector2(12f, 8f), new Vector2(-12f, -8f),
            42, btnTextColor, TextAlignmentOptions.Center);
        lbl.text      = label;
        lbl.fontStyle = FontStyles.Bold;
        lbl.enableWordWrapping = true;
    }

    TextMeshProUGUI MakeText(GameObject parent, string name,
        Vector2 ancMin, Vector2 ancMax,
        Vector2 offMin, Vector2 offMax,
        int fontSize, Color color, TextAlignmentOptions align)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin;
        rt.anchorMax = ancMax;
        rt.pivot     = new Vector2(0.5f, 1f);
        rt.offsetMin = offMin;
        rt.offsetMax = offMax;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        ApplyFont(tmp);
        tmp.fontSize  = fontSize;
        tmp.color     = color;
        tmp.alignment = align;
        return tmp;
    }

    void ApplyFont(TextMeshProUGUI tmp)
    {
        TMP_FontAsset f = fontAsset;
        if (f == null) f = TMP_Settings.defaultFontAsset;
        if (f == null) f = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (f != null) tmp.font = f;
    }

    // ══════════════════════════════════════════════════════════════════════
    // GIZMO — tampilkan lingkaran trigger di Scene view
    // ══════════════════════════════════════════════════════════════════════
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
        Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.15f);
        Gizmos.DrawSphere(transform.position, triggerDistance);
    }
#endif
}
