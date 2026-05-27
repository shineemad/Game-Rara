using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// PathChoiceUI — panel pilihan jalur "Jalan Ramai vs Gang Sepi".
///
/// Cara Setup di Scene (5 langkah):
///   1. Buat GameObject kosong bernama "PathTrigger" — taruh di posisi tiang/persimpangan.
///   2. Tambahkan komponen ini ke PathTrigger.
///   3. Di field "playerTransform" → drag Rara.
///   4. Atur "triggerDistance" (jarak agar panel muncul, default 2).
///   5. Play → saat Rara mendekati tiang, panel otomatis muncul.
///
/// Setelah pilihan dibuat:
///   GameState.Instance.pathChoice = "safe" atau "dangerous"
///   onSafeChosen / onDangerChosen dipanggil (sambungkan ke Day1Controller)
/// </summary>
public class PathChoiceUI : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR
    // ══════════════════════════════════════════════════════════════════════

    [Header("Trigger")]
    [Tooltip("Drag Transform Rara ke sini")]
    public Transform playerTransform;
    [Tooltip("Jarak (unit) dari tiang agar panel muncul")]
    public float triggerDistance = 2.5f;
    [Tooltip("Hanya tampil sekali (true) atau setiap kali Rara lewat (false)")]
    public bool  triggerOnce = true;

    [Header("Konten Panel")]
    public string titleText    = "⚠  ADA DUA JALUR!";
    [TextArea(2, 4)]
    public string bodyText     = "Rara harus pilih jalur ke sekolah.\nMana yang menurutmu lebih aman buat Rara?";
    public string safeLabel    = "🏛  Jalan Ramai\n(aman, banyak orang)";
    public string dangerLabel  = "🔴  Gang Sepi\n(lebih cepat, tapi... bahaya!)";

    [Header("Tampilan")]
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

    [Header("Font (opsional)")]
    public TMP_FontAsset fontAsset;

    [Header("Events — sambungkan ke Day1Controller")]
    [Tooltip("Dipanggil saat pemain memilih Jalan Ramai")]
    public UnityEngine.Events.UnityEvent onSafeChosen;
    [Tooltip("Dipanggil saat pemain memilih Gang Sepi")]
    public UnityEngine.Events.UnityEvent onDangerChosen;

    // ══════════════════════════════════════════════════════════════════════
    // INTERNAL
    // ══════════════════════════════════════════════════════════════════════

    private Canvas  canvas;
    private GameObject panelRoot;
    private GameObject uiRoot;   // parent overlay + panel — ini yang di-hide/show
    private bool    triggered = false;
    private bool    shown     = false;

    // referensi komponen Rigidbody2D Rara untuk pause gerak
    private Rigidbody2D playerRb;

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

        BuildUI();
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
        uiRoot.SetActive(true);
        panelRoot.SetActive(true);

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
        onSafeChosen?.Invoke();
        Debug.Log("[PathChoice] Dipilih: Jalan Ramai (safe)");
    }

    void OnDangerButton()
    {
        if (GameState.Instance != null)
            GameState.Instance.pathChoice = "dangerous";

        HidePanel();
        onDangerChosen?.Invoke();
        Debug.Log("[PathChoice] Dipilih: Gang Sepi (dangerous)");
    }

    void HidePanel()
    {
        panelRoot.SetActive(false);
        uiRoot.SetActive(false); // overlay ikut tersembunyi

        // Bebaskan kembali Rara
        if (playerRb != null)
            playerRb.isKinematic = false;
    }

    // ══════════════════════════════════════════════════════════════════════
    // BUILD UI
    // ══════════════════════════════════════════════════════════════════════

    void BuildUI()
    {
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
