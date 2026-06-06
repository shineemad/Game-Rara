using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Day1SummaryScreen — Layar ringkasan akhir Hari 1.
///
/// Tampil setelah pemain klik "Lanjut" di EduCardDay1.
/// Menampilkan: judul "Hari 1 Selesai!", skor + target, panel RED FLAG
/// (daftar tanda bahaya yang harus dikenali), footer hotline, ikon nyawa,
/// dan dua tombol aksi: ULANGI HARI 1 / LANJUT HARI 2.
///
/// Cara pakai:
///   1. GameObject → Create Empty → "Day1SummaryScreen"
///   2. Add Component → Day1SummaryScreen
///   3. (Opsional) Custom semua field di Inspector
///   4. Di EduCardDay1 → onLanjut() → drag GameObject ini →
///      Day1SummaryScreen.Tampilkan
///      (atau biarkan auto-find via EduCardDay1.HandleLanjut)
///   5. Hubungkan onUlangiHari1 & onLanjutHari2 ke SceneLoader / aksi lain.
/// </summary>
public class Day1SummaryScreen : MonoBehaviour
{
    public static Day1SummaryScreen Instance { get; private set; }

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR
    // ══════════════════════════════════════════════════════════════════════

    [Header("Auto-Tampil")]
    [Tooltip("Tampilkan otomatis saat scene start? Untuk debug saja.")]
    public bool autoTampilSaatStart = false;
    [Tooltip("Bekukan pergerakan player saat layar tampil.")]
    public bool freezePlayerSaatTampil = true;

    [Header("Background Kartu (CUSTOMIZABLE)")]
    [Tooltip("Sprite latar kartu utama. Kosong = panel solid warna.")]
    public Sprite backgroundSprite;
    public Image.Type backgroundImageType = Image.Type.Sliced;
    [Tooltip("Warna tint background. Default hijau gelap pekat.")]
    public Color backgroundColor = new Color(0.04f, 0.13f, 0.07f, 0.97f);
    [Tooltip("Sprite border luar (opsional). Kosong = pakai Outline kuning.")]
    public Sprite borderSprite;
    public Color  borderColor = new Color(1f, 0.85f, 0.20f, 1f);

    [Header("Overlay Belakang")]
    public bool   tampilkanOverlay = true;
    public Sprite overlaySprite;
    public Color  overlayColor     = new Color(0f, 0f, 0f, 0.82f);

    [Header("Judul")]
    public string judul       = "\u2713  Hari 1 Selesai!";
    public Color  warnaJudul  = new Color(0.45f, 1f, 0.65f, 1f);
    public int    ukuranJudul = 44;

    [Header("Subtitle")]
    [Tooltip("Gunakan {SKOR} {NYAWA} {MAXNYAWA} sebagai placeholder.")]
    public string subtitleFormat = "Skor Hari 1   |   Nyawa tersisa: {NYAWA}/{MAXNYAWA}";
    public Color  warnaSubtitle  = new Color(1f, 0.85f, 0.25f, 1f);
    public int    ukuranSubtitle = 22;

    [Header("Progress Bar Skor")]
    [Tooltip("Tampilkan progress bar skor di bawah subtitle.")]
    public bool tampilkanBar = true;
    [Tooltip("Target skor minimum untuk Hari 1. Bar penuh saat skor >= target.")]
    public int  targetSkor = 300;
    [Tooltip("Sprite background bar. Kosong = solid color rounded.")]
    public Sprite barBackgroundSprite;
    public Color  barBackgroundColor = new Color(0.08f, 0.25f, 0.13f, 1f);
    [Tooltip("Sprite fill bar. Kosong = solid color.")]
    public Sprite barFillSprite;
    public Color  barFillColor = new Color(0.18f, 0.78f, 0.45f, 1f);
    [Tooltip("Format teks di tengah bar. Gunakan {SKOR} {TARGET}.")]
    public string barTeksFormat = "{SKOR} / {TARGET} poin";
    public Color  barTeksWarna  = Color.white;
    public int    barUkuranTeks = 20;
    public float  barTinggi     = 36f;

    [Header("Panel RED FLAG (CUSTOMIZABLE)")]
    [Tooltip("Tampilkan panel RED FLAG (daftar tanda bahaya).")]
    public bool tampilkanRedFlag = true;
    [Tooltip("Sprite background panel RED FLAG. Kosong = transparan rounded.")]
    public Sprite redFlagBgSprite;
    public Color  redFlagBgColor     = new Color(0.18f, 0.04f, 0.06f, 0.85f);
    [Tooltip("Warna garis tepi panel.")]
    public Color  redFlagBorderColor = new Color(0.95f, 0.30f, 0.30f, 1f);
    public string redFlagJudul       = "\uD83D\uDEA9  KENALI TANDA BAHAYA (RED FLAG)";
    public Color  redFlagJudulWarna  = new Color(1f, 0.55f, 0.55f, 1f);
    public int    redFlagJudulUkuran = 22;

    [Header("Daftar Tanda Bahaya (CUSTOMIZABLE)")]
    [Tooltip("Tambah / hapus / edit bullet sesuka hati.")]
    [TextArea(1, 3)]
    public string[] redFlagItems = new string[]
    {
        "Orang asing ngajak kamu pergi tanpa izin ortu",
        "Ada yang nawarin hadiah atau makanan berlebihan",
        "Orang dewasa minta rahasiain pertemuan kalian — \uD83D\uDEA9",
        "Ada yang sentuh tubuhmu tanpa izin — TERIAK & LARI!",
        "Jangan lewatin gang sepi sendirian — pilih jalan rame!"
    };
    [Tooltip("Karakter bullet di depan setiap item.")]
    public string bulletChar = "\u2022 ";
    public Color  bulletColor = new Color(1f, 0.95f, 0.85f, 1f);
    public int    bulletUkuran = 18;
    public float  bulletJarakBaris = 10f;

    [Header("Tombol Pilihanku (top-right RED FLAG)")]
    public bool   tampilkanTombolPilihanku = true;
    public string pilihankuTeks  = "\uD83D\uDCDC Pilihanku";
    public Color  pilihankuWarna = new Color(0.85f, 0.45f, 0.10f, 1f);
    public Color  pilihankuTeksWarna = Color.white;
    public int    pilihankuUkuranTeks = 16;

    [Header("Footer Hotline")]
    public bool   tampilkanFooter = true;
    [TextArea(2, 4)]
    public string footerText = "\u2755  Kalau ada yang ngancam kamu:\nPolisi 110  |  Hotline Anak 129  |  KPAI 021-31901556";
    public Color  warnaFooter = new Color(1f, 0.85f, 0.25f, 1f);
    public int    ukuranFooter = 16;

    [Header("Baris Nyawa")]
    public bool tampilkanNyawa = true;
    [Tooltip("Sprite hati penuh.")]
    public Sprite hatiPenuhSprite;
    [Tooltip("Sprite hati kosong.")]
    public Sprite hatiKosongSprite;
    public Color  hatiTintColor = Color.white;
    public Vector2 hatiUkuran   = new Vector2(36f, 36f);
    public float   hatiJarak    = 6f;
    public string nyawaLabelFormat = "Nyawa Rara: {NYAWA}";
    public Color  nyawaLabelWarna  = Color.white;
    public int    nyawaLabelUkuran = 18;

    [Header("Tombol ULANGI HARI 1")]
    public bool   tampilkanTombolUlangi = true;
    public string ulangiTeks   = "\u21BB  ULANGI HARI 1";
    public Sprite ulangiSprite;
    public Color  ulangiWarna  = new Color(0.78f, 0.58f, 0.20f, 1f);
    public Color  ulangiBorder = new Color(1f, 0.78f, 0.20f, 1f);
    public Color  ulangiTeksWarna = Color.white;
    public int    ulangiUkuranTeks = 22;

    [Header("Tombol LANJUT HARI 2")]
    public string lanjutTeks   = "\u25B6  LANJUT HARI 2";
    public Sprite lanjutSprite;
    public Color  lanjutWarna  = new Color(0.18f, 0.62f, 0.32f, 1f);
    public Color  lanjutBorder = new Color(0.45f, 1f, 0.65f, 1f);
    public Color  lanjutTeksWarna = Color.white;
    public int    lanjutUkuranTeks = 24;

    [Header("Font (opsional)")]
    public TMP_FontAsset fontAsset;

    [Header("Animasi")]
    [Range(0.5f, 1f)] public float skalaAwal = 0.85f;
    public float durasiPopIn = 0.32f;

    [Header("Audio (opsional)")]
    public AudioClip sfxMuncul;
    public AudioClip sfxKlikLanjut;
    public AudioClip sfxKlikUlangi;

    [Header("Sorting")]
    [Tooltip("Sorting order Canvas. Default 1030 — di atas EduCardDay1 (1020).")]
    public int sortingOrder = 1030;

    [Header("Event")]
    [Tooltip("Dipanggil saat tombol ULANGI HARI 1 ditekan. Default = reload scene aktif.")]
    public UnityEngine.Events.UnityEvent onUlangiHari1;
    [Tooltip("Dipanggil saat tombol LANJUT HARI 2 ditekan. Hubungkan ke SceneLoader.LoadScene(\"Day2\").")]
    public UnityEngine.Events.UnityEvent onLanjutHari2;

    [Header("Aksi Default (kalau Event kosong)")]
    [Tooltip("Kalau onUlangiHari1 kosong → reload scene saat ini.")]
    public bool ulangiReloadScene = true;
    [Tooltip("Kalau onLanjutHari2 kosong → coba LoadScene nama berikut.")]
    public string lanjutSceneName = "Day2";

    // ── runtime ───────────────────────────────────────────────────────────
    private bool       _tampil;
    private GameObject _canvasGO;
    private Sprite     _roundedRectSprite;
    private GameObject _pilihankuPanel;

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        if (autoTampilSaatStart) Tampilkan();
    }

    // ══════════════════════════════════════════════════════════════════════
    // PUBLIC API
    // ══════════════════════════════════════════════════════════════════════
    public void Tampilkan()
    {
        if (_tampil) return;
        StartCoroutine(TampilkanLayar());
    }

    public static void TampilkanStatik()
    {
        if (Instance != null) Instance.Tampilkan();
        else Debug.LogWarning("[Day1SummaryScreen] Tidak ada instance di scene!");
    }

    public void Tutup()
    {
        if (!_tampil) return;
        if (_canvasGO != null) Destroy(_canvasGO);
        _canvasGO = null;
        _tampil   = false;

        if (freezePlayerSaatTampil)
        {
            var d1 = FindFirstObjectByType<Day1Controller>();
            if (d1 != null) d1.ResumePlayer();
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // BUILDER
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator TampilkanLayar()
    {
        _tampil = true;

        if (freezePlayerSaatTampil)
        {
            var d1 = FindFirstObjectByType<Day1Controller>();
            if (d1 != null) d1.FreezePlayer();
        }

        if (sfxMuncul != null)
            AudioManager.Instance?.sfxSource?.PlayOneShot(sfxMuncul);

        BuildUI();

        // Pop-in animasi
        var kartuRT = _canvasGO.transform.Find("Kartu").GetComponent<RectTransform>();
        float t = 0f;
        Vector3 from = Vector3.one * skalaAwal;
        Vector3 to   = Vector3.one;
        while (t < durasiPopIn)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / durasiPopIn);
            float ease = 1f - Mathf.Pow(1f - p, 3f);
            kartuRT.localScale = Vector3.LerpUnclamped(from, to, ease);
            yield return null;
        }
        kartuRT.localScale = to;
    }

    void BuildUI()
    {
        // ── CANVAS ───────────────────────────────────────────────────────
        _canvasGO = new GameObject("Day1SummaryCanvas");
        var canvas = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = _canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        _canvasGO.AddComponent<GraphicRaycaster>();

        // ── OVERLAY ──────────────────────────────────────────────────────
        if (tampilkanOverlay)
        {
            var ov = new GameObject("Overlay");
            ov.transform.SetParent(_canvasGO.transform, false);
            var ovImg = ov.AddComponent<Image>();
            ovImg.sprite = overlaySprite;
            ovImg.color  = overlayColor;
            ovImg.raycastTarget = true;
            var rt = ov.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        // ── KARTU (panel utama) ──────────────────────────────────────────
        var kartu = new GameObject("Kartu");
        kartu.transform.SetParent(_canvasGO.transform, false);
        var kartuImg = kartu.AddComponent<Image>();
        kartuImg.sprite = backgroundSprite != null ? backgroundSprite : GetRoundedRectSprite();
        kartuImg.color  = backgroundColor;
        kartuImg.type   = backgroundSprite != null ? backgroundImageType : Image.Type.Sliced;

        var kRT = kartu.GetComponent<RectTransform>();
        kRT.anchorMin = new Vector2(0.5f, 0.5f);
        kRT.anchorMax = new Vector2(0.5f, 0.5f);
        kRT.pivot     = new Vector2(0.5f, 0.5f);
        kRT.sizeDelta = new Vector2(1280f, 880f);

        // Border (outline kuning)
        if (borderSprite != null)
        {
            var bd = new GameObject("Border");
            bd.transform.SetParent(kartu.transform, false);
            var bImg = bd.AddComponent<Image>();
            bImg.sprite = borderSprite;
            bImg.color  = borderColor;
            bImg.type   = Image.Type.Sliced;
            bImg.raycastTarget = false;
            var brt = bd.GetComponent<RectTransform>();
            brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
            brt.offsetMin = new Vector2(-4f, -4f);
            brt.offsetMax = new Vector2( 4f,  4f);
        }
        else
        {
            var outline = kartu.AddComponent<Outline>();
            outline.effectColor    = borderColor;
            outline.effectDistance = new Vector2(3f, -3f);
        }

        // ── JUDUL ────────────────────────────────────────────────────────
        var judulTmp = BuatTeks(kartu.transform, "Judul", judul,
                                ukuranJudul, warnaJudul, FontStyles.Bold);
        var jrt = judulTmp.rectTransform;
        jrt.anchorMin = new Vector2(0f, 1f);
        jrt.anchorMax = new Vector2(1f, 1f);
        jrt.pivot     = new Vector2(0.5f, 1f);
        jrt.offsetMin = new Vector2(40f, -110f);
        jrt.offsetMax = new Vector2(-40f, -25f);
        judulTmp.alignment = TextAlignmentOptions.Center;

        // ── SUBTITLE ─────────────────────────────────────────────────────
        var gs = GameState.Instance;
        int curScore = gs != null ? gs.score    : 0;
        int curLives = gs != null ? gs.lives    : 3;
        int maxLives = gs != null ? gs.maxLives : 3;

        string subt = subtitleFormat
            .Replace("{SKOR}",     curScore.ToString())
            .Replace("{NYAWA}",    curLives.ToString())
            .Replace("{MAXNYAWA}", maxLives.ToString());

        var subTmp = BuatTeks(kartu.transform, "Subtitle", subt,
                              ukuranSubtitle, warnaSubtitle, FontStyles.Normal);
        var srt = subTmp.rectTransform;
        srt.anchorMin = new Vector2(0f, 1f);
        srt.anchorMax = new Vector2(1f, 1f);
        srt.pivot     = new Vector2(0.5f, 1f);
        srt.offsetMin = new Vector2(40f, -155f);
        srt.offsetMax = new Vector2(-40f, -115f);
        subTmp.alignment = TextAlignmentOptions.Center;

        // ── PROGRESS BAR SKOR ────────────────────────────────────────────
        float barTopY = -170f;
        if (tampilkanBar)
        {
            BuatBarSkor(kartu.transform, curScore, barTopY);
            barTopY -= (barTinggi + 18f);
        }

        // ── PANEL RED FLAG ───────────────────────────────────────────────
        float redFlagBottomY = tampilkanNyawa ? 230f : 170f;
        if (tampilkanRedFlag)
        {
            BuatPanelRedFlag(kartu.transform, barTopY, redFlagBottomY);
        }

        // ── BARIS NYAWA ──────────────────────────────────────────────────
        if (tampilkanNyawa)
        {
            BuatBarisNyawa(kartu.transform, curLives, maxLives);
        }

        // ── TOMBOL AKSI BAWAH ────────────────────────────────────────────
        BuatTombolAksi(kartu.transform);
    }

    // ══════════════════════════════════════════════════════════════════════
    // BAR SKOR
    // ══════════════════════════════════════════════════════════════════════
    void BuatBarSkor(Transform parent, int curScore, float topY)
    {
        var holder = new GameObject("BarHolder");
        holder.transform.SetParent(parent, false);
        var hRT = holder.AddComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0.5f, 1f);
        hRT.anchorMax = new Vector2(0.5f, 1f);
        hRT.pivot     = new Vector2(0.5f, 1f);
        hRT.sizeDelta = new Vector2(560f, barTinggi);
        hRT.anchoredPosition = new Vector2(0f, topY);

        // Background bar
        var bg = new GameObject("BarBG");
        bg.transform.SetParent(holder.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.sprite = barBackgroundSprite != null ? barBackgroundSprite : GetRoundedRectSprite();
        bgImg.color  = barBackgroundColor;
        bgImg.type   = Image.Type.Sliced;
        var bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;

        // Fill bar
        var fill = new GameObject("BarFill");
        fill.transform.SetParent(holder.transform, false);
        var fImg = fill.AddComponent<Image>();
        fImg.sprite = barFillSprite != null ? barFillSprite : GetRoundedRectSprite();
        fImg.color  = barFillColor;
        fImg.type   = Image.Type.Sliced;
        var fRT = fill.GetComponent<RectTransform>();
        fRT.anchorMin = new Vector2(0f, 0f);
        fRT.anchorMax = new Vector2(0f, 1f);
        fRT.pivot     = new Vector2(0f, 0.5f);
        float pct  = targetSkor <= 0 ? 1f : Mathf.Clamp01((float)curScore / targetSkor);
        float fullW = 560f - 4f;
        fRT.offsetMin = new Vector2(2f, 2f);
        fRT.offsetMax = new Vector2(2f, -2f);
        fRT.sizeDelta = new Vector2(fullW * pct, 0f);

        // Teks bar
        string txt = barTeksFormat
            .Replace("{SKOR}",   curScore.ToString())
            .Replace("{TARGET}", targetSkor.ToString());
        var label = BuatTeks(holder.transform, "BarTeks", txt,
                             barUkuranTeks, barTeksWarna, FontStyles.Bold);
        var lrt = label.rectTransform;
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        label.alignment = TextAlignmentOptions.Center;
    }

    // ══════════════════════════════════════════════════════════════════════
    // PANEL RED FLAG
    // ══════════════════════════════════════════════════════════════════════
    void BuatPanelRedFlag(Transform parent, float topY, float bottomY)
    {
        var panel = new GameObject("RedFlagPanel");
        panel.transform.SetParent(parent, false);
        var pImg = panel.AddComponent<Image>();
        pImg.sprite = redFlagBgSprite != null ? redFlagBgSprite : GetRoundedRectSprite();
        pImg.color  = redFlagBgColor;
        pImg.type   = Image.Type.Sliced;
        var pRT = panel.GetComponent<RectTransform>();
        pRT.anchorMin = new Vector2(0f, 0f);
        pRT.anchorMax = new Vector2(1f, 1f);
        pRT.offsetMin = new Vector2(40f, bottomY);
        pRT.offsetMax = new Vector2(-40f, topY);

        var outl = panel.AddComponent<Outline>();
        outl.effectColor    = redFlagBorderColor;
        outl.effectDistance = new Vector2(2f, -2f);

        // Judul panel
        var judulP = BuatTeks(panel.transform, "RFJudul", redFlagJudul,
                              redFlagJudulUkuran, redFlagJudulWarna, FontStyles.Bold);
        var jRT = judulP.rectTransform;
        jRT.anchorMin = new Vector2(0f, 1f);
        jRT.anchorMax = new Vector2(1f, 1f);
        jRT.pivot     = new Vector2(0.5f, 1f);
        jRT.offsetMin = new Vector2(20f, -50f);
        jRT.offsetMax = new Vector2(-20f, -10f);
        judulP.alignment = TextAlignmentOptions.Center;

        // Tombol Pilihanku (top-right)
        if (tampilkanTombolPilihanku)
        {
            BuatTombolPilihanku(panel.transform);
        }

        // List items (VerticalLayout)
        float footerH = tampilkanFooter ? 70f : 0f;
        var list = new GameObject("RFList");
        list.transform.SetParent(panel.transform, false);
        var lrt = list.AddComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0f, 0f);
        lrt.anchorMax = new Vector2(1f, 1f);
        lrt.offsetMin = new Vector2(30f, footerH + 10f);
        lrt.offsetMax = new Vector2(-30f, -60f);

        var vlg = list.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.spacing = bulletJarakBaris;
        vlg.childControlWidth  = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;

        foreach (var item in redFlagItems)
        {
            if (string.IsNullOrEmpty(item)) continue;
            var t = BuatTeks(list.transform, "Item", bulletChar + item,
                             bulletUkuran, bulletColor, FontStyles.Normal);
            t.alignment = TextAlignmentOptions.TopLeft;
            var fitter = t.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        // Footer hotline
        if (tampilkanFooter)
        {
            var ft = BuatTeks(panel.transform, "Footer", footerText,
                              ukuranFooter, warnaFooter, FontStyles.Italic);
            var frt = ft.rectTransform;
            frt.anchorMin = new Vector2(0f, 0f);
            frt.anchorMax = new Vector2(1f, 0f);
            frt.pivot     = new Vector2(0.5f, 0f);
            frt.offsetMin = new Vector2(20f, 10f);
            frt.offsetMax = new Vector2(-20f, 65f);
            ft.alignment = TextAlignmentOptions.Center;
        }
    }

    void BuatTombolPilihanku(Transform parent)
    {
        var btnGO = new GameObject("PilihankuBtn");
        btnGO.transform.SetParent(parent, false);
        var img = btnGO.AddComponent<Image>();
        img.sprite = GetRoundedRectSprite();
        img.color  = pilihankuWarna;
        img.type   = Image.Type.Sliced;
        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot     = new Vector2(1f, 1f);
        rt.sizeDelta = new Vector2(150f, 38f);
        rt.anchoredPosition = new Vector2(-12f, -10f);

        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = img;

        var t = BuatTeks(btnGO.transform, "Label", pilihankuTeks,
                         pilihankuUkuranTeks, pilihankuTeksWarna, FontStyles.Bold);
        var trt = t.rectTransform;
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;

        btn.onClick.AddListener(HandlePilihanku);
    }

    void HandlePilihanku()
    {
        AudioManager.Instance?.Click();
        // Toggle panel sederhana yang menampilkan daftar pilihan pemain
        if (_pilihankuPanel != null)
        {
            Destroy(_pilihankuPanel);
            _pilihankuPanel = null;
            return;
        }

        _pilihankuPanel = new GameObject("PilihankuPanel");
        _pilihankuPanel.transform.SetParent(_canvasGO.transform, false);
        var img = _pilihankuPanel.AddComponent<Image>();
        img.sprite = GetRoundedRectSprite();
        img.color  = new Color(0.05f, 0.05f, 0.08f, 0.96f);
        img.type   = Image.Type.Sliced;
        var rt = _pilihankuPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(720f, 540f);

        var outl = _pilihankuPanel.AddComponent<Outline>();
        outl.effectColor    = pilihankuWarna;
        outl.effectDistance = new Vector2(3f, -3f);

        var jud = BuatTeks(_pilihankuPanel.transform, "Judul",
            "\uD83D\uDCDC  Pilihanku — Hari 1", 26, warnaJudul, FontStyles.Bold);
        var jrt = jud.rectTransform;
        jrt.anchorMin = new Vector2(0f, 1f);
        jrt.anchorMax = new Vector2(1f, 1f);
        jrt.pivot     = new Vector2(0.5f, 1f);
        jrt.offsetMin = new Vector2(20f, -55f);
        jrt.offsetMax = new Vector2(-20f, -15f);
        jud.alignment = TextAlignmentOptions.Center;

        // Build text dari GameState.choices (filter Hari 1)
        string body = "";
        var gs = GameState.Instance;
        if (gs != null && gs.choices != null)
        {
            int idx = 1;
            foreach (var c in gs.choices)
            {
                if (c.day != 1) continue;
                string colorHex =
                    c.category == "AMAN"   ? "#26AD61" :
                    c.category == "RAGU"   ? "#F29D12" :
                    c.category == "BAHAYA" ? "#E84D3D" : "#339FDB";
                body += $"<color={colorHex}><b>{idx}. [{c.category}]</b></color>  {c.label}   <color=#FFD24A>+{c.points}</color>\n\n";
                idx++;
            }
        }
        if (string.IsNullOrEmpty(body)) body = "<i>Belum ada pilihan yang tercatat.</i>";

        var isi = BuatTeks(_pilihankuPanel.transform, "Isi", body,
                           18, new Color(1f, 1f, 0.92f, 1f), FontStyles.Normal);
        var irt = isi.rectTransform;
        irt.anchorMin = new Vector2(0f, 0f);
        irt.anchorMax = new Vector2(1f, 1f);
        irt.offsetMin = new Vector2(28f, 70f);
        irt.offsetMax = new Vector2(-28f, -70f);
        isi.alignment = TextAlignmentOptions.TopLeft;

        // Tombol tutup
        var closeGO = new GameObject("TutupBtn");
        closeGO.transform.SetParent(_pilihankuPanel.transform, false);
        var cImg = closeGO.AddComponent<Image>();
        cImg.sprite = GetRoundedRectSprite();
        cImg.color  = new Color(0.55f, 0.18f, 0.18f, 1f);
        cImg.type   = Image.Type.Sliced;
        var crt = closeGO.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0.5f, 0f);
        crt.anchorMax = new Vector2(0.5f, 0f);
        crt.pivot     = new Vector2(0.5f, 0f);
        crt.sizeDelta = new Vector2(180f, 44f);
        crt.anchoredPosition = new Vector2(0f, 16f);

        var cBtn = closeGO.AddComponent<Button>();
        cBtn.targetGraphic = cImg;
        var cT = BuatTeks(closeGO.transform, "Label", "\u2715  Tutup",
                          18, Color.white, FontStyles.Bold);
        var crt2 = cT.rectTransform;
        crt2.anchorMin = Vector2.zero; crt2.anchorMax = Vector2.one;
        crt2.offsetMin = Vector2.zero; crt2.offsetMax = Vector2.zero;
        cT.alignment = TextAlignmentOptions.Center;
        cT.raycastTarget = false;
        cBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance?.Click();
            if (_pilihankuPanel != null) Destroy(_pilihankuPanel);
            _pilihankuPanel = null;
        });
    }

    // ══════════════════════════════════════════════════════════════════════
    // BARIS NYAWA
    // ══════════════════════════════════════════════════════════════════════
    void BuatBarisNyawa(Transform parent, int curLives, int maxLives)
    {
        var holder = new GameObject("NyawaHolder");
        holder.transform.SetParent(parent, false);
        var hRT = holder.AddComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0.5f, 0f);
        hRT.anchorMax = new Vector2(0.5f, 0f);
        hRT.pivot     = new Vector2(0.5f, 0f);
        hRT.sizeDelta = new Vector2(360f, 90f);
        hRT.anchoredPosition = new Vector2(0f, 130f);

        // Row hati
        var row = new GameObject("HatiRow");
        row.transform.SetParent(holder.transform, false);
        var rRT = row.AddComponent<RectTransform>();
        rRT.anchorMin = new Vector2(0.5f, 1f);
        rRT.anchorMax = new Vector2(0.5f, 1f);
        rRT.pivot     = new Vector2(0.5f, 1f);
        rRT.sizeDelta = new Vector2(360f, hatiUkuran.y + 4f);
        rRT.anchoredPosition = Vector2.zero;

        var hLay = row.AddComponent<HorizontalLayoutGroup>();
        hLay.childAlignment = TextAnchor.MiddleCenter;
        hLay.spacing = hatiJarak;
        hLay.childForceExpandWidth = false;
        hLay.childForceExpandHeight = false;
        hLay.childControlWidth = false;
        hLay.childControlHeight = false;

        for (int i = 0; i < maxLives; i++)
        {
            var hg = new GameObject("Hati_" + i);
            hg.transform.SetParent(row.transform, false);
            var im = hg.AddComponent<Image>();
            im.sprite = (i < curLives && hatiPenuhSprite != null) ? hatiPenuhSprite
                        : (i >= curLives && hatiKosongSprite != null) ? hatiKosongSprite
                        : (hatiPenuhSprite != null ? hatiPenuhSprite : null);
            im.color  = (i < curLives) ? hatiTintColor : new Color(hatiTintColor.r, hatiTintColor.g, hatiTintColor.b, 0.35f);
            im.preserveAspect = true;
            im.raycastTarget = false;
            // Fallback emoji kalau sprite kosong
            if (im.sprite == null)
            {
                Destroy(im);
                var emoji = BuatTeks(hg.transform, "Emoji",
                    (i < curLives) ? "\u2764" : "\uD83D\uDC94",
                    (int)hatiUkuran.y, (i < curLives) ? new Color(1f, 0.25f, 0.25f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f),
                    FontStyles.Bold);
                emoji.alignment = TextAlignmentOptions.Center;
                var ert = emoji.rectTransform;
                ert.sizeDelta = hatiUkuran;
            }
            var le = hg.AddComponent<LayoutElement>();
            le.preferredWidth  = hatiUkuran.x;
            le.preferredHeight = hatiUkuran.y;
        }

        // Label "Nyawa Rara: X"
        string lbl = nyawaLabelFormat.Replace("{NYAWA}", curLives.ToString())
                                     .Replace("{MAXNYAWA}", maxLives.ToString());
        var labelTmp = BuatTeks(holder.transform, "Label", lbl,
                                nyawaLabelUkuran, nyawaLabelWarna, FontStyles.Normal);
        var lrt = labelTmp.rectTransform;
        lrt.anchorMin = new Vector2(0f, 0f);
        lrt.anchorMax = new Vector2(1f, 0f);
        lrt.pivot     = new Vector2(0.5f, 0f);
        lrt.offsetMin = new Vector2(0f, 0f);
        lrt.offsetMax = new Vector2(0f, 30f);
        labelTmp.alignment = TextAlignmentOptions.Center;
    }

    // ══════════════════════════════════════════════════════════════════════
    // TOMBOL AKSI (ULANGI / LANJUT)
    // ══════════════════════════════════════════════════════════════════════
    void BuatTombolAksi(Transform parent)
    {
        var holder = new GameObject("TombolHolder");
        holder.transform.SetParent(parent, false);
        var hRT = holder.AddComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0.5f, 0f);
        hRT.anchorMax = new Vector2(0.5f, 0f);
        hRT.pivot     = new Vector2(0.5f, 0f);
        hRT.sizeDelta = new Vector2(820f, 72f);
        hRT.anchoredPosition = new Vector2(0f, 30f);

        var hLay = holder.AddComponent<HorizontalLayoutGroup>();
        hLay.childAlignment = TextAnchor.MiddleCenter;
        hLay.spacing = 30f;
        hLay.childForceExpandWidth = false;
        hLay.childForceExpandHeight = true;
        hLay.childControlWidth = false;
        hLay.childControlHeight = true;

        if (tampilkanTombolUlangi)
        {
            BuatTombol(holder.transform, "UlangiBtn", ulangiTeks, ulangiSprite,
                       ulangiWarna, ulangiBorder, ulangiTeksWarna, ulangiUkuranTeks,
                       new Vector2(330f, 72f), HandleUlangi);
        }
        BuatTombol(holder.transform, "LanjutBtn", lanjutTeks, lanjutSprite,
                   lanjutWarna, lanjutBorder, lanjutTeksWarna, lanjutUkuranTeks,
                   new Vector2(380f, 72f), HandleLanjut);
    }

    void BuatTombol(Transform parent, string name, string teks, Sprite spr,
                    Color bg, Color border, Color teksWarna, int ukuran,
                    Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var im = go.AddComponent<Image>();
        im.sprite = spr != null ? spr : GetRoundedRectSprite();
        im.color  = bg;
        im.type   = Image.Type.Sliced;
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth  = size.x;
        le.preferredHeight = size.y;

        var outl = go.AddComponent<Outline>();
        outl.effectColor    = border;
        outl.effectDistance = new Vector2(2f, -2f);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = im;
        var colors = btn.colors;
        colors.highlightedColor = new Color(bg.r * 1.15f, bg.g * 1.15f, bg.b * 1.15f, bg.a);
        colors.pressedColor     = new Color(bg.r * 0.85f, bg.g * 0.85f, bg.b * 0.85f, bg.a);
        btn.colors = colors;

        var t = BuatTeks(go.transform, "Label", teks, ukuran, teksWarna, FontStyles.Bold);
        var trt = t.rectTransform;
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;

        btn.onClick.AddListener(onClick);
    }

    void HandleUlangi()
    {
        if (sfxKlikUlangi != null)
            AudioManager.Instance?.sfxSource?.PlayOneShot(sfxKlikUlangi);
        else
            AudioManager.Instance?.Click();

        Tutup();

        // PRIORITAS 1: UnityEvent dari Inspector
        if (onUlangiHari1 != null && onUlangiHari1.GetPersistentEventCount() > 0)
        {
            onUlangiHari1.Invoke();
            return;
        }

        // PRIORITAS 2: DayTransitionManager (single-scene mode)
        if (DayTransitionManager.Instance != null)
        {
            DayTransitionManager.Instance.UlangiHari1();
            return;
        }

        // PRIORITAS 3 (fallback): reload scene aktif
        if (ulangiReloadScene)
        {
            var gs = GameState.Instance;
            if (gs != null) { gs.score = 0; gs.lives = gs.maxLives; gs.choices.Clear(); gs.day = 1; }
            var active = SceneManager.GetActiveScene().name;
            if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene(active);
            else SceneManager.LoadScene(active);
        }
    }

    void HandleLanjut()
    {
        if (sfxKlikLanjut != null)
            AudioManager.Instance?.sfxSource?.PlayOneShot(sfxKlikLanjut);
        else
            AudioManager.Instance?.Click();

        Tutup();

        // ── JAMINAN NAVBAR ─────────────────────────────────────────────────
        // Apapun PRIORITAS yang dipakai di bawah (UnityEvent / DayTransitionManager
        // / LoadScene fallback), pastikan navbar HUD sudah tahu sekarang Hari 2.
        // HUDManager.OnLanjutHari2() idempotent — set GameState.day=2 + refresh
        // navbar (H1→H2) + putar animasi highlight lingkaran H2.
        if (HUDManager.Instance != null) HUDManager.Instance.OnLanjutHari2();

        // PRIORITAS 1: UnityEvent dari Inspector
        if (onLanjutHari2 != null && onLanjutHari2.GetPersistentEventCount() > 0)
        {
            onLanjutHari2.Invoke();
            return;
        }

        // PRIORITAS 2: DayTransitionManager (single-scene mode)
        if (DayTransitionManager.Instance != null)
        {
            DayTransitionManager.Instance.LanjutKeDay2();
            return;
        }

        // PRIORITAS 3 (fallback): load scene Day 2
        if (!string.IsNullOrEmpty(lanjutSceneName))
        {
            var gs = GameState.Instance;
            if (gs != null) gs.day = 2;
            if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene(lanjutSceneName);
            else SceneManager.LoadScene(lanjutSceneName);
        }
        else
        {
            Debug.LogWarning("[Day1SummaryScreen] Tidak ada DayTransitionManager, onLanjutHari2, atau lanjutSceneName. Tidak ada aksi.");
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // HELPERS
    // ══════════════════════════════════════════════════════════════════════
    TextMeshProUGUI BuatTeks(Transform parent, string name, string content,
                             int size, Color color, FontStyles style)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        TMP_FontAsset f = fontAsset ?? TMP_Settings.defaultFontAsset;
        if (f != null) tmp.font = f;
        tmp.text          = content;
        tmp.fontSize      = size;
        tmp.color         = color;
        tmp.fontStyle     = style;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        return tmp;
    }

    Sprite GetRoundedRectSprite()
    {
        if (_roundedRectSprite != null) return _roundedRectSprite;
        // Generate sprite rounded-rect 32x32 dengan corner radius ~10
        int size = 64;
        int radius = 14;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        Color32 white = new Color32(255, 255, 255, 255);
        Color32 clear = new Color32(255, 255, 255, 0);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool inside = true;
                int dx = 0, dy = 0;
                if (x < radius && y < radius)            { dx = radius - x; dy = radius - y; inside = dx*dx + dy*dy <= radius*radius; }
                else if (x >= size-radius && y < radius) { dx = x - (size-1-radius); dy = radius - y; inside = dx*dx + dy*dy <= radius*radius; }
                else if (x < radius && y >= size-radius) { dx = radius - x; dy = y - (size-1-radius); inside = dx*dx + dy*dy <= radius*radius; }
                else if (x >= size-radius && y >= size-radius) { dx = x - (size-1-radius); dy = y - (size-1-radius); inside = dx*dx + dy*dy <= radius*radius; }
                tex.SetPixel(x, y, inside ? (Color)white : (Color)clear);
            }
        }
        tex.Apply();
        _roundedRectSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
                                           new Vector2(0.5f, 0.5f), 100f,
                                           0, SpriteMeshType.FullRect,
                                           new Vector4(radius, radius, radius, radius));
        return _roundedRectSprite;
    }
}
