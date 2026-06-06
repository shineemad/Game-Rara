using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ZonaWarning — overlay highlight kuning yang menampilkan peringatan
/// saat player mendekati zona encounter (E1, E2, E3 di Day1).
///
/// Cara pakai:
///   1. Drag komponen ini ke GameObject mana saja di scene Day1 (mis. Day1Controller GO)
///   2. Drag referensi Player + Day1Controller di Inspector
///   3. (Opsional) Ganti sprite & warna lewat Inspector
///
/// Tampilan: gradient kuning di tepi layar + label "⚠ Hati-hati! Zona Asing"
/// muncul saat player berada dalam `jarakPeringatan` dari encounter berikutnya.
/// </summary>
public class ZonaWarning : MonoBehaviour
{
    [Header("Referensi (wajib)")]
    public Transform player;
    public Day1Controller day1;

    [Header("Sprite (opsional — drag PNG)")]
    [Tooltip("Sprite overlay gradient. Kosong = gradient kuning prosedural.")]
    public Sprite overlaySprite;
    [Tooltip("Sprite ikon peringatan. Kosong = pakai emoji ⚠.")]
    public Sprite warningIcon;

    [Header("Warna")]
    public Color overlayColor = new Color(1f, 0.85f, 0.20f, 1f);   // tint kuning
    public Color labelColor   = new Color(0.25f, 0.15f, 0f, 1f);   // coklat tua
    public Color labelBgColor = new Color(1f, 0.85f, 0.20f, 0.92f);

    [Header("Gaya Label")]
    [Tooltip("Tampilkan kotak latar di belakang teks peringatan. Off = teks transparan saja.")]
    public bool tampilkanLatarLabel = false;
    [Tooltip("Tambah outline gelap di belakang teks (agar terbaca tanpa latar).")]
    public bool gunakanOutlineTeks  = true;
    [Tooltip("Warna outline teks (hanya berlaku jika gunakanOutlineTeks aktif).")]
    public Color outlineTeksColor   = new Color(0f, 0f, 0f, 0.85f);

    [Header("Teks Peringatan")]
    [Tooltip("Pesan saat dekat Encounter 1 (NPC asing).")]
    public string pesanE1 = "⚠ Hati-hati! Ada orang asing di depan";
    [Tooltip("Pesan saat dekat Encounter 2 (godaan/ancaman).")]
    public string pesanE2 = "⚠ Waspada! Situasi mencurigakan";
    [Tooltip("Pesan saat dekat Encounter 3 (insiden akhir).")]
    public string pesanE3 = "⚠ Tetap tenang, ingat caramu menjaga diri";

    [Header("Behaviour")]
    [Tooltip("Jarak (unit world) untuk mulai memunculkan peringatan.")]
    public float jarakPeringatan = 6f;
    [Tooltip("Alpha maksimum overlay saat player tepat di trigger.")]
    [Range(0f, 1f)] public float alphaMax = 0.35f;
    [Tooltip("Apakah warning juga tampil saat player sudah lewat zona?")]
    public bool tampilkanSetelahLewat = false;

    [Header("Audio (opsional)")]
    [Tooltip("SFX heartbeat / chime saat warning pertama muncul.")]
    public AudioClip sfxPeringatan;

    [Header("Sorting")]
    [Tooltip("Sorting order Canvas. Default 850 — di bawah HUD (900) & dialog.")]
    public int sortingOrder = 850;

    // ── runtime ──────────────────────────────────────────────────────────
    private Canvas _canvas;
    private Image  _overlayImg;
    private GameObject _label;
    private TextMeshProUGUI _labelTMP;
    private Image  _labelBg;
    private Sprite _gradientSprite;
    private Sprite _roundedRect;
    private float  _lastTriggerX = -999f;

    void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (day1 == null) day1 = FindFirstObjectByType<Day1Controller>();
        BuildUI();
    }

    void BuildUI()
    {
        // Canvas
        var cGO = new GameObject("ZonaWarningCanvas");
        cGO.transform.SetParent(transform, false);
        _canvas = cGO.AddComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = sortingOrder;
        var sc = cGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920f, 1080f);
        sc.matchWidthOrHeight = 0.5f;
        // Tidak perlu raycaster — overlay hanya visual

        // Overlay gradient fullscreen
        var oGO = new GameObject("Overlay");
        oGO.transform.SetParent(cGO.transform, false);
        var oRT = oGO.AddComponent<RectTransform>();
        oRT.anchorMin = Vector2.zero; oRT.anchorMax = Vector2.one;
        oRT.offsetMin = oRT.offsetMax = Vector2.zero;
        _overlayImg = oGO.AddComponent<Image>();
        _overlayImg.sprite = overlaySprite != null ? overlaySprite : GetGradientSprite();
        _overlayImg.color  = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0f);
        _overlayImg.raycastTarget = false;
        _overlayImg.type   = overlaySprite != null ? Image.Type.Sliced : Image.Type.Simple;

        // Label peringatan (top-center)
        _label = new GameObject("WarningLabel");
        _label.transform.SetParent(cGO.transform, false);
        var lRT = _label.AddComponent<RectTransform>();
        lRT.anchorMin = new Vector2(0.5f, 1f);
        lRT.anchorMax = new Vector2(0.5f, 1f);
        lRT.pivot     = new Vector2(0.5f, 1f);
        lRT.sizeDelta = new Vector2(620f, 60f);
        lRT.anchoredPosition = new Vector2(0f, -140f); // di bawah HUD top

        if (tampilkanLatarLabel)
        {
            _labelBg = _label.AddComponent<Image>();
            _labelBg.sprite = GetRoundedRect();
            _labelBg.type   = Image.Type.Sliced;
            _labelBg.color  = new Color(labelBgColor.r, labelBgColor.g, labelBgColor.b, 0f);
            _labelBg.raycastTarget = false;
            var outline = _label.AddComponent<Outline>();
            outline.effectColor = new Color(0.4f, 0.25f, 0f, 0.5f);
            outline.effectDistance = new Vector2(2f, -2f);
        }

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(_label.transform, false);
        var tRT = txtGO.AddComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = new Vector2(16f, 0f); tRT.offsetMax = new Vector2(-16f, 0f);
        _labelTMP = txtGO.AddComponent<TextMeshProUGUI>();
        _labelTMP.font = TMP_Settings.defaultFontAsset;
        _labelTMP.fontSize  = 24;
        _labelTMP.fontStyle = FontStyles.Bold;
        _labelTMP.color     = new Color(labelColor.r, labelColor.g, labelColor.b, 0f);
        _labelTMP.alignment = TextAlignmentOptions.Center;
        _labelTMP.text      = "";

        // Outline teks (agar terbaca tanpa background)
        if (gunakanOutlineTeks && !tampilkanLatarLabel)
        {
            var txtOutline = txtGO.AddComponent<Outline>();
            txtOutline.effectColor    = outlineTeksColor;
            txtOutline.effectDistance = new Vector2(2f, -2f);
        }
        _labelTMP.raycastTarget = false;
    }

    void Update()
    {
        if (player == null || day1 == null || _overlayImg == null) return;

        // Cari trigger encounter terdekat
        float px = player.position.x;
        float bestDist = float.MaxValue;
        float bestX    = 0f;
        string bestMsg = "";

        TryZone(px, day1.encE1, pesanE1, ref bestDist, ref bestX, ref bestMsg);
        TryZone(px, day1.encE2, pesanE2, ref bestDist, ref bestX, ref bestMsg);
        TryZone(px, day1.encE3, pesanE3, ref bestDist, ref bestX, ref bestMsg);

        float alpha = 0f;
        string msg  = "";
        if (bestDist < jarakPeringatan)
        {
            alpha = Mathf.Lerp(0f, alphaMax, 1f - (bestDist / jarakPeringatan));
            msg   = bestMsg;

            // SFX hanya saat pertama masuk zona ini
            if (sfxPeringatan != null && Mathf.Abs(bestX - _lastTriggerX) > 0.5f)
            {
                AudioManager.Instance?.sfxSource?.PlayOneShot(sfxPeringatan, 0.6f);
                _lastTriggerX = bestX;
            }
        }
        else
        {
            _lastTriggerX = -999f;
        }

        // Apply alpha
        var oc = _overlayImg.color;
        oc.a = Mathf.MoveTowards(oc.a, alpha, Time.deltaTime * 1.2f);
        _overlayImg.color = oc;

        if (_labelTMP != null)
        {
            if (!string.IsNullOrEmpty(msg)) _labelTMP.text = msg;
            var tc = _labelTMP.color;
            tc.a = Mathf.MoveTowards(tc.a, alpha > 0.05f ? 1f : 0f, Time.deltaTime * 3f);
            _labelTMP.color = tc;
            if (_labelBg != null)
            {
                var bc = _labelBg.color;
                bc.a = Mathf.MoveTowards(bc.a, alpha > 0.05f ? labelBgColor.a : 0f, Time.deltaTime * 3f);
                _labelBg.color = bc;
            }
        }
    }

    void TryZone(float px, float zoneX, string msg,
                 ref float bestDist, ref float bestX, ref string bestMsg)
    {
        float d = zoneX - px;
        // hanya zona di depan player (atau di belakang jika diizinkan)
        if (!tampilkanSetelahLewat && d < 0f) return;
        float ad = Mathf.Abs(d);
        if (ad < bestDist) { bestDist = ad; bestX = zoneX; bestMsg = msg; }
    }

    // ══════════════════════════════════════════════════════════════════════
    // SPRITE PROSEDURAL
    // ══════════════════════════════════════════════════════════════════════
    Sprite GetGradientSprite()
    {
        if (_gradientSprite != null) return _gradientSprite;
        // Gradient: opaque di tepi, transparan di tengah (vignette kuning)
        const int w = 256, h = 144;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var cx = w / 2f; var cy = h / 2f;
        float maxR = Mathf.Sqrt(cx * cx + cy * cy);
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            float dx = (x - cx) / cx;
            float dy = (y - cy) / cy;
            float r  = Mathf.Sqrt(dx * dx + dy * dy);
            float a  = Mathf.Clamp01(r * r * 1.2f - 0.15f);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        tex.Apply();
        _gradientSprite = Sprite.Create(tex, new Rect(0, 0, w, h),
            new Vector2(0.5f, 0.5f), 100f);
        return _gradientSprite;
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
