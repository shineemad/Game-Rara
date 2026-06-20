using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Day2SummaryScreen — Ringkasan akhir Hari 2.
///
/// Tampil setelah pemain klik "Lanjut" di EduCardDay2.
/// Menampilkan: judul "Hari 2 Selesai!", skor & nyawa, daftar pencapaian,
/// dan tombol ULANGI HARI 2 / LANJUT HARI 3.
/// </summary>
public class Day2SummaryScreen : MonoBehaviour
{
    public static Day2SummaryScreen Instance { get; private set; }

    [Header("Auto-Tampil")]
    public bool autoTampilSaatStart = false;

    [Header("Background Kartu")]
    public Sprite backgroundSprite;
    public Image.Type backgroundImageType = Image.Type.Sliced;
    public Color backgroundColor = new Color(0.04f, 0.13f, 0.07f, 0.97f);
    public Sprite borderSprite;
    public Color  borderColor = new Color(1f, 0.85f, 0.20f, 1f);

    [Header("Overlay Belakang")]
    public bool   tampilkanOverlay = true;
    [Tooltip("Sprite latar belakang penuh layar di belakang kartu (opsional). Kosong = pakai overlayColor.")]
    public Sprite overlaySprite;
    public Color  overlayColor = new Color(0f, 0f, 0f, 0.82f);

    [Header("Judul")]
    public string judul       = "\u2713  Hari 2 Selesai!";
    public Color  warnaJudul  = new Color(0.45f, 1f, 0.65f, 1f);
    public int    ukuranJudul = 44;

    [Header("Subtitle")]
    public string subtitleFormat = "Skor Hari 2   |   Nyawa tersisa: {NYAWA}/{MAXNYAWA}";
    public Color  warnaSubtitle  = new Color(1f, 0.85f, 0.25f, 1f);
    public int    ukuranSubtitle = 22;

    [Header("Rating Bintang")]
    [Tooltip("Tampilkan rating bintang 1-3 di bawah subtitle (ala game umumnya).")]
    public bool tampilkanBintang = true;

    [Header("Progress Bar Skor")]
    public bool tampilkanBar = true;
    public int  targetSkor = 3500;
    public Color barBackgroundColor = new Color(0.08f, 0.25f, 0.13f, 1f);
    public Color barFillColor = new Color(0.18f, 0.78f, 0.45f, 1f);
    public string barTeksFormat = "{SKOR} / {TARGET} poin";
    public Color  barTeksWarna = Color.white;
    public int    barUkuranTeks = 20;
    public float  barTinggi = 36f;

    [Header("Panel Pencapaian")]
    public bool tampilkanPanelPencapaian = true;
    public Color panelBgColor = new Color(0.05f, 0.18f, 0.10f, 0.85f);
    public Color panelBorderColor = new Color(0.45f, 1f, 0.65f, 1f);
    public string panelJudul = "\uD83C\uDFC6  PENCAPAIAN HARI 2";
    public Color  panelJudulWarna = new Color(0.45f, 1f, 0.65f, 1f);
    public int    panelJudulUkuran = 22;
    [Tooltip("Akan tampil otomatis dari GameState.achievements. Kosongkan kalau mau tampil kustom.")]
    public string[] pencapaianKustom = new string[0];
    public Color  pencapaianWarna = new Color(1f, 0.95f, 0.85f, 1f);
    public int    pencapaianUkuran = 18;

    [Header("Footer Hotline")]
    public bool tampilkanFooter = true;
    [TextArea(2, 4)]
    public string footerText = "\u2755  Ingat! Polisi 110  |  Hotline Anak 129  |  KPAI 021-31901556";
    public Color  warnaFooter = new Color(1f, 0.85f, 0.25f, 1f);
    public int    ukuranFooter = 16;

    [Header("Baris Nyawa")]
    public bool tampilkanNyawa = true;
    public Sprite hatiPenuhSprite;
    public Sprite hatiKosongSprite;
    public Vector2 hatiUkuran = new Vector2(36f, 36f);
    public float   hatiJarak = 6f;
    public string nyawaLabelFormat = "Nyawa Rara: {NYAWA}";
    public Color  nyawaLabelWarna = Color.white;
    public int    nyawaLabelUkuran = 18;

    [Header("Tombol ULANGI HARI 2")]
    public bool   tampilkanTombolUlangi = true;
    public string ulangiTeks   = "\u21BB  ULANGI HARI 2";
    public Color  ulangiWarna  = new Color(0.78f, 0.58f, 0.20f, 1f);
    public Color  ulangiBorder = new Color(1f, 0.78f, 0.20f, 1f);
    public int    ulangiUkuranTeks = 22;

    [Header("Tombol LANJUT HARI 3")]
    public string lanjutTeks   = "\u25B6  LANJUT HARI 3";
    public Color  lanjutWarna  = new Color(0.18f, 0.62f, 0.32f, 1f);
    public Color  lanjutBorder = new Color(0.45f, 1f, 0.65f, 1f);
    public int    lanjutUkuranTeks = 24;

    [Header("Refleksi (Kata Sakti & Bahaya)")]
    [Tooltip("Tampilkan ringkasan 3 Kata Sakti yang dikuasai + hasil Meteran Bahaya di panel pencapaian.")]
    public bool tampilkanRefleksi = true;

    [Header("Rekap Keputusan Hari 2")]
    [Tooltip("Tampilkan daftar tiap keputusan yang diambil pemain di Hari 2 (AMAN/RAGU/BAHAYA) untuk refleksi edukatif.")]
    public bool tampilkanRekapPilihan = true;
    [Tooltip("Judul kecil di atas daftar keputusan.")]
    public string rekapJudul = "\uD83D\uDCDD  Keputusanmu hari ini:";
    public int    rekapUkuran = 17;

    [Header("Font (opsional)")]
    public TMP_FontAsset fontAsset;

    [Header("Animasi")]
    [Range(0.5f, 1f)] public float skalaAwal = 0.85f;
    public float durasiPopIn = 0.32f;

    [Header("Audio (opsional)")]
    public AudioClip sfxMuncul;
    public AudioClip sfxKlikLanjut;

    [Header("Sorting")]
    public int sortingOrder = 1030;

    [Header("Event")]
    public UnityEngine.Events.UnityEvent onUlangiHari2;
    public UnityEngine.Events.UnityEvent onLanjutHari3;

    [Header("Aksi Default")]
    public bool ulangiReloadScene = true;
    public string lanjutSceneName = "Day3";

    // ── runtime ───────────────────────────────────────────────────────────
    private bool       _tampil;
    private GameObject _canvasGO;
    private Sprite     _roundedSprite;

    void Awake() { Instance = this; }
    void OnDestroy() { if (Instance == this) Instance = null; }
    void Start() { if (autoTampilSaatStart) Tampilkan(); }

    public void Tampilkan()
    {
        if (_tampil) return;
        // Pastikan GameObject (+ rantai parent) aktif sebelum StartCoroutine,
        // supaya layar ringkasan tidak gagal tampil saat leluhur sempat ter-disable.
        if (!gameObject.activeInHierarchy)
        {
            for (Transform t = transform; t != null; t = t.parent)
                if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
        }
        StartCoroutine(TampilkanLayar());
    }

    public void Tutup()
    {
        if (!_tampil) return;
        if (_canvasGO != null) Destroy(_canvasGO);
        _canvasGO = null;
        _tampil = false;
    }

    IEnumerator TampilkanLayar()
    {
        _tampil = true;
        if (sfxMuncul != null) AudioManager.Instance?.sfxSource?.PlayOneShot(sfxMuncul);
        BuildUI();

        var kartuRT = _canvasGO.transform.Find("Kartu").GetComponent<RectTransform>();
        float t = 0f;
        while (t < durasiPopIn)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / durasiPopIn);
            float ease = 1f - Mathf.Pow(1f - p, 3f);
            kartuRT.localScale = Vector3.LerpUnclamped(Vector3.one * skalaAwal, Vector3.one, ease);
            yield return null;
        }
        kartuRT.localScale = Vector3.one;
    }

    void BuildUI()
    {
        _canvasGO = new GameObject("Day2SummaryCanvas");
        var canvas = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = _canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        _canvasGO.AddComponent<GraphicRaycaster>();

        // BG hitam pekat (selalu ada) supaya scene di belakang tidak menembus kartu ringkasan.
        var bgHitam = new GameObject("BG_Hitam");
        bgHitam.transform.SetParent(_canvasGO.transform, false);
        var bgHitamImg = bgHitam.AddComponent<Image>();
        bgHitamImg.color = Color.black; bgHitamImg.raycastTarget = true;
        var bgHitamRT = bgHitam.GetComponent<RectTransform>();
        bgHitamRT.anchorMin = Vector2.zero; bgHitamRT.anchorMax = Vector2.one;
        bgHitamRT.offsetMin = Vector2.zero; bgHitamRT.offsetMax = Vector2.zero;

        if (tampilkanOverlay)
        {
            var ov = new GameObject("Overlay");
            ov.transform.SetParent(_canvasGO.transform, false);
            var ovImg = ov.AddComponent<Image>();
            if (overlaySprite != null) { ovImg.sprite = overlaySprite; ovImg.type = Image.Type.Sliced; ovImg.color = Color.white; }
            else                       ovImg.color = overlayColor;
            ovImg.raycastTarget = true;
            var rt = ov.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        // Kartu
        var kartu = new GameObject("Kartu");
        kartu.transform.SetParent(_canvasGO.transform, false);
        var kartuImg = kartu.AddComponent<Image>();
        kartuImg.sprite = backgroundSprite != null ? backgroundSprite : GetRoundedSprite();
        kartuImg.color = backgroundColor;
        kartuImg.type = backgroundSprite != null ? backgroundImageType : Image.Type.Sliced;
        var kRT = kartu.GetComponent<RectTransform>();
        kRT.anchorMin = new Vector2(0.5f, 0.5f); kRT.anchorMax = new Vector2(0.5f, 0.5f);
        kRT.pivot = new Vector2(0.5f, 0.5f);
        kRT.sizeDelta = new Vector2(1280f, 980f);

        if (borderSprite != null)
        {
            var bd = new GameObject("Border");
            bd.transform.SetParent(kartu.transform, false);
            var bImg = bd.AddComponent<Image>();
            bImg.sprite = borderSprite; bImg.color = borderColor; bImg.type = Image.Type.Sliced; bImg.raycastTarget = false;
            var brt = bd.GetComponent<RectTransform>();
            brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
            brt.offsetMin = new Vector2(-4f, -4f); brt.offsetMax = new Vector2(4f, 4f);
        }
        else
        {
            var outl = kartu.AddComponent<Outline>();
            outl.effectColor = borderColor;
            outl.effectDistance = new Vector2(3f, -3f);
        }

        // Judul
        var judulTmp = BuatTeks(kartu.transform, "Judul", judul, ukuranJudul, warnaJudul, FontStyles.Bold);
        var jrt = judulTmp.rectTransform;
        jrt.anchorMin = new Vector2(0f, 1f); jrt.anchorMax = new Vector2(1f, 1f);
        jrt.pivot = new Vector2(0.5f, 1f);
        jrt.offsetMin = new Vector2(40f, -110f);
        jrt.offsetMax = new Vector2(-40f, -25f);
        judulTmp.alignment = TextAlignmentOptions.Center;

        // Data dari GameState
        var gs = GameState.Instance;
        int curScore = gs != null ? gs.score : 0;
        int curLives = gs != null ? gs.lives : 3;
        int maxLives = gs != null ? gs.maxLives : 3;

        // Subtitle
        string subt = subtitleFormat.Replace("{SKOR}", curScore.ToString())
            .Replace("{NYAWA}", curLives.ToString()).Replace("{MAXNYAWA}", maxLives.ToString());
        var subTmp = BuatTeks(kartu.transform, "Subtitle", subt, ukuranSubtitle, warnaSubtitle, FontStyles.Normal);
        var srt = subTmp.rectTransform;
        srt.anchorMin = new Vector2(0f, 1f); srt.anchorMax = new Vector2(1f, 1f);
        srt.pivot = new Vector2(0.5f, 1f);
        srt.offsetMin = new Vector2(40f, -155f); srt.offsetMax = new Vector2(-40f, -115f);
        subTmp.alignment = TextAlignmentOptions.Center;

        // Rating bintang
        float topY = -170f;
        if (tampilkanBintang)
        {
            int bintang = RatingBintang.HitungBintang(curScore, targetSkor);
            RatingBintang.Bangun(kartu.transform, bintang,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, topY), 50f, 16f, this);
            topY -= 74f;
        }

        // Bar skor
        if (tampilkanBar)
        {
            BuatBarSkor(kartu.transform, curScore, topY);
            topY -= (barTinggi + 18f);
        }

        // Panel pencapaian
        if (tampilkanPanelPencapaian)
        {
            float bottomY = tampilkanNyawa ? 230f : 170f;
            BuatPanelPencapaian(kartu.transform, topY, bottomY, gs);
        }

        if (tampilkanNyawa) BuatBarisNyawa(kartu.transform, curLives, maxLives);
        BuatTombolAksi(kartu.transform);
    }

    void BuatBarSkor(Transform parent, int curScore, float topY)
    {
        var holder = new GameObject("BarHolder");
        holder.transform.SetParent(parent, false);
        var hRT = holder.AddComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0.5f, 1f); hRT.anchorMax = new Vector2(0.5f, 1f);
        hRT.pivot = new Vector2(0.5f, 1f);
        hRT.sizeDelta = new Vector2(560f, barTinggi);
        hRT.anchoredPosition = new Vector2(0f, topY);

        var bg = new GameObject("BarBG");
        bg.transform.SetParent(holder.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.sprite = GetRoundedSprite(); bgImg.color = barBackgroundColor; bgImg.type = Image.Type.Sliced;
        var bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;

        var fill = new GameObject("BarFill");
        fill.transform.SetParent(holder.transform, false);
        var fImg = fill.AddComponent<Image>();
        fImg.sprite = GetRoundedSprite(); fImg.color = barFillColor; fImg.type = Image.Type.Sliced;
        var fRT = fill.GetComponent<RectTransform>();
        fRT.anchorMin = new Vector2(0f, 0f); fRT.anchorMax = new Vector2(0f, 1f);
        fRT.pivot = new Vector2(0f, 0.5f);
        float pct = targetSkor <= 0 ? 1f : Mathf.Clamp01((float)curScore / targetSkor);
        fRT.offsetMin = new Vector2(2f, 2f); fRT.offsetMax = new Vector2(2f, -2f);
        fRT.sizeDelta = new Vector2((560f - 4f) * pct, 0f);

        string txt = barTeksFormat.Replace("{SKOR}", curScore.ToString()).Replace("{TARGET}", targetSkor.ToString());
        var label = BuatTeks(holder.transform, "BarTeks", txt, barUkuranTeks, barTeksWarna, FontStyles.Bold);
        var lrt = label.rectTransform;
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        label.alignment = TextAlignmentOptions.Center;
    }

    void BuatPanelPencapaian(Transform parent, float topY, float bottomY, GameState gs)
    {
        var panel = new GameObject("PencapaianPanel");
        panel.transform.SetParent(parent, false);
        var pImg = panel.AddComponent<Image>();
        pImg.sprite = GetRoundedSprite(); pImg.color = panelBgColor; pImg.type = Image.Type.Sliced;
        var pRT = panel.GetComponent<RectTransform>();
        pRT.anchorMin = new Vector2(0f, 0f); pRT.anchorMax = new Vector2(1f, 1f);
        pRT.offsetMin = new Vector2(40f, bottomY); pRT.offsetMax = new Vector2(-40f, topY);
        var outl = panel.AddComponent<Outline>();
        outl.effectColor = panelBorderColor; outl.effectDistance = new Vector2(2f, -2f);

        var jud = BuatTeks(panel.transform, "PJudul", panelJudul, panelJudulUkuran, panelJudulWarna, FontStyles.Bold);
        var jRT = jud.rectTransform;
        jRT.anchorMin = new Vector2(0f, 1f); jRT.anchorMax = new Vector2(1f, 1f);
        jRT.pivot = new Vector2(0.5f, 1f);
        jRT.offsetMin = new Vector2(20f, -50f); jRT.offsetMax = new Vector2(-20f, -10f);
        jud.alignment = TextAlignmentOptions.Center;

        // ── ScrollRect (viewport + content) ─────────────────────────────
        // Konten bisa lebih panjang dari panel (banyak pilihan + refleksi);
        // ScrollRect membuat ringkasan tetap rapi & bisa di-scroll, bukan
        // menumpuk teks satu sama lain.
        float footerH = tampilkanFooter ? 70f : 0f;
        var scrollGO = new GameObject("ScrollArea");
        scrollGO.transform.SetParent(panel.transform, false);
        var sRT = scrollGO.AddComponent<RectTransform>();
        sRT.anchorMin = new Vector2(0f, 0f); sRT.anchorMax = new Vector2(1f, 1f);
        sRT.offsetMin = new Vector2(16f, footerH + 8f); sRT.offsetMax = new Vector2(-16f, -60f);
        var scroll = scrollGO.AddComponent<ScrollRect>();
        scroll.horizontal = false; scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28f;

        var viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollGO.transform, false);
        var vRT = viewportGO.AddComponent<RectTransform>();
        vRT.anchorMin = Vector2.zero; vRT.anchorMax = Vector2.one;
        vRT.offsetMin = Vector2.zero; vRT.offsetMax = Vector2.zero;
        vRT.pivot = new Vector2(0f, 1f);
        // Image transparan penuh hanya untuk menangkap raycast (scroll/drag).
        var vImg = viewportGO.AddComponent<Image>();
        vImg.color = new Color(0f, 0f, 0f, 0f);
        vImg.raycastTarget = true;
        // RectMask2D meng-clip berdasarkan rectangle (bukan alpha stencil),
        // sehingga child tidak hilang seperti saat memakai Mask + Image alpha~0.
        viewportGO.AddComponent<RectMask2D>();
        scroll.viewport = vRT;

        var list = new GameObject("Content");
        list.transform.SetParent(viewportGO.transform, false);
        var lrt = list.AddComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0f, 1f); lrt.anchorMax = new Vector2(1f, 1f);
        lrt.pivot = new Vector2(0.5f, 1f);
        lrt.anchoredPosition = Vector2.zero;
        lrt.sizeDelta = new Vector2(0f, 0f);
        scroll.content = lrt;

        var vlg = list.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperLeft; vlg.spacing = 10f;
        vlg.padding = new RectOffset(14, 14, 8, 8);
        vlg.childControlWidth = true; vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
        // ContentSizeFitter pada CONTENT (bukan pada item) — agar Content
        // membesar vertikal mengikuti jumlah item → ScrollRect bisa scroll.
        var csf = list.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // Sumber: kustom > GameState.achievements (filter relevan H2)
        string[] items;
        if (pencapaianKustom != null && pencapaianKustom.Length > 0)
            items = pencapaianKustom;
        else if (gs != null && gs.achievements != null && gs.achievements.Count > 0)
            items = gs.achievements.ToArray();
        else
            items = new[] { "(Belum ada pencapaian khusus untuk hari ini.)" };

        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item)) continue;
            var t = BuatTeks(list.transform, "Item", "\uD83C\uDFC6  " + item, pencapaianUkuran, pencapaianWarna, FontStyles.Normal);
            t.alignment = TextAlignmentOptions.MidlineLeft;
        }

        // Refleksi: 3 Kata Sakti yang dikuasai + hasil Meteran Bahaya.
        if (tampilkanRefleksi && gs != null)
        {
            BuatPemisah(list.transform); // garis pemisah supaya blok Refleksi terbedakan dari daftar pencapaian
            string ck(bool on, string kata) => (on ? "\u2705 " : "\u2B1C ") + kata;
            string kataSaktiBaris = "\uD83D\uDDDD Kata Sakti: " +
                ck(gs.usedTidak, "TIDAK") + "   " + ck(gs.usedPergi, "PERGI") + "   " + ck(gs.usedCerita, "CERITA");
            var ks = BuatTeks(list.transform, "KataSakti", kataSaktiBaris, pencapaianUkuran, new Color(1f, 0.95f, 0.7f, 1f), FontStyles.Bold);
            ks.alignment = TextAlignmentOptions.MidlineLeft;

            float d = gs.dangerLevel;
            string status = d <= 0.25f ? "TERKENDALI \u2014 kamu tenang & waspada"
                          : d <= 0.6f  ? "WASPADA \u2014 ada beberapa keputusan berisiko"
                                       : "RAWAN \u2014 yuk pelajari lagi cara menjaga diri";
            Color dColor = d <= 0.25f ? new Color(0.45f, 1f, 0.65f, 1f)
                         : d <= 0.6f  ? new Color(1f, 0.85f, 0.25f, 1f)
                                      : new Color(1f, 0.45f, 0.4f, 1f);
            var db = BuatTeks(list.transform, "Bahaya", $"\u26A0 Tingkat Bahaya akhir: {Mathf.RoundToInt(d * 100f)}% \u2014 {status}", pencapaianUkuran, dColor, FontStyles.Normal);
            db.alignment = TextAlignmentOptions.MidlineLeft;
        }

        // Rekap keputusan Hari 2: tiap pilihan diwarnai sesuai kategori (AMAN/RAGU/BAHAYA).
        if (tampilkanRekapPilihan && gs != null && gs.choices != null)
        {
            bool adaPilihan = gs.choices.Exists(c => c != null && c.day == 2);
            if (adaPilihan)
            {
                BuatPemisah(list.transform); // garis pemisah supaya blok Keputusan terbedakan dari blok Refleksi
                var rj = BuatTeks(list.transform, "RekapJudul", rekapJudul, rekapUkuran,
                    new Color(0.75f, 0.92f, 1f, 1f), FontStyles.Bold);
                rj.alignment = TextAlignmentOptions.MidlineLeft;

                foreach (var ch in gs.choices)
                {
                    if (ch == null || ch.day != 2) continue;
                    string hex = WarnaKategoriHex(ch.category);
                    string ikon = ch.category == "AMAN" ? "\u2705"
                                : ch.category == "RAGU" ? "\u26A0"
                                : "\u274C";
                    string baris = $"{ikon} {ch.label}  <color={hex}>[{ch.category}]</color>";
                    var ct = BuatTeks(list.transform, "Pilihan", baris, rekapUkuran,
                        new Color(0.92f, 0.92f, 0.88f, 1f), FontStyles.Normal);
                    ct.alignment = TextAlignmentOptions.TopLeft;
                }
            }
        }

        if (tampilkanFooter)
        {
            var ft = BuatTeks(panel.transform, "Footer", footerText, ukuranFooter, warnaFooter, FontStyles.Italic);
            var frt = ft.rectTransform;
            frt.anchorMin = new Vector2(0f, 0f); frt.anchorMax = new Vector2(1f, 0f);
            frt.pivot = new Vector2(0.5f, 0f);
            frt.offsetMin = new Vector2(20f, 10f); frt.offsetMax = new Vector2(-20f, 65f);
            ft.alignment = TextAlignmentOptions.Center;
        }

        // Paksa rebuild layout supaya tinggi Content terhitung dengan benar
        // dari TMP preferredHeight pada frame yang sama dengan build UI.
        LayoutRebuilder.ForceRebuildLayoutImmediate(lrt);
        // Scroll ke atas (item pertama).
        scroll.verticalNormalizedPosition = 1f;
    }

    void BuatBarisNyawa(Transform parent, int curLives, int maxLives)
    {
        var holder = new GameObject("NyawaHolder");
        holder.transform.SetParent(parent, false);
        var hRT = holder.AddComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0.5f, 0f); hRT.anchorMax = new Vector2(0.5f, 0f);
        hRT.pivot = new Vector2(0.5f, 0f);
        hRT.sizeDelta = new Vector2(360f, 90f);
        hRT.anchoredPosition = new Vector2(0f, 130f);

        var row = new GameObject("Row");
        row.transform.SetParent(holder.transform, false);
        var rRT = row.AddComponent<RectTransform>();
        rRT.anchorMin = new Vector2(0.5f, 1f); rRT.anchorMax = new Vector2(0.5f, 1f);
        rRT.pivot = new Vector2(0.5f, 1f);
        rRT.sizeDelta = new Vector2(360f, hatiUkuran.y + 4f);
        var hLay = row.AddComponent<HorizontalLayoutGroup>();
        hLay.childAlignment = TextAnchor.MiddleCenter; hLay.spacing = hatiJarak;
        hLay.childForceExpandWidth = false; hLay.childForceExpandHeight = false;
        hLay.childControlWidth = false; hLay.childControlHeight = false;

        for (int i = 0; i < maxLives; i++)
        {
            var hg = new GameObject("Hati_" + i);
            hg.transform.SetParent(row.transform, false);
            var im = hg.AddComponent<Image>();
            im.sprite = (i < curLives && hatiPenuhSprite != null) ? hatiPenuhSprite
                : (i >= curLives && hatiKosongSprite != null) ? hatiKosongSprite
                : (hatiPenuhSprite != null ? hatiPenuhSprite : null);
            im.color = (i < curLives) ? Color.white : new Color(1f,1f,1f,0.35f);
            im.preserveAspect = true; im.raycastTarget = false;
            if (im.sprite == null)
            {
                Destroy(im);
                var emoji = BuatTeks(hg.transform, "Emoji",
                    (i < curLives) ? "\u2764" : "\uD83D\uDC94",
                    (int)hatiUkuran.y,
                    (i < curLives) ? new Color(1f, 0.25f, 0.25f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f),
                    FontStyles.Bold);
                emoji.alignment = TextAlignmentOptions.Center;
                emoji.rectTransform.sizeDelta = hatiUkuran;
            }
            var le = hg.AddComponent<LayoutElement>();
            le.preferredWidth = hatiUkuran.x; le.preferredHeight = hatiUkuran.y;
        }

        string lbl = nyawaLabelFormat.Replace("{NYAWA}", curLives.ToString()).Replace("{MAXNYAWA}", maxLives.ToString());
        var labelTmp = BuatTeks(holder.transform, "Label", lbl, nyawaLabelUkuran, nyawaLabelWarna, FontStyles.Normal);
        var lrt = labelTmp.rectTransform;
        lrt.anchorMin = new Vector2(0f, 0f); lrt.anchorMax = new Vector2(1f, 0f);
        lrt.pivot = new Vector2(0.5f, 0f);
        lrt.offsetMin = new Vector2(0f, 0f); lrt.offsetMax = new Vector2(0f, 30f);
        labelTmp.alignment = TextAlignmentOptions.Center;
    }

    void BuatTombolAksi(Transform parent)
    {
        var holder = new GameObject("TombolHolder");
        holder.transform.SetParent(parent, false);
        var hRT = holder.AddComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0.5f, 0f); hRT.anchorMax = new Vector2(0.5f, 0f);
        hRT.pivot = new Vector2(0.5f, 0f);
        hRT.sizeDelta = new Vector2(820f, 72f);
        hRT.anchoredPosition = new Vector2(0f, 30f);
        var hLay = holder.AddComponent<HorizontalLayoutGroup>();
        hLay.childAlignment = TextAnchor.MiddleCenter; hLay.spacing = 30f;
        hLay.childForceExpandWidth = false; hLay.childForceExpandHeight = true;
        hLay.childControlWidth = false; hLay.childControlHeight = true;

        if (tampilkanTombolUlangi)
            BuatTombol(holder.transform, ulangiTeks, ulangiWarna, ulangiBorder, ulangiUkuranTeks, new Vector2(330f, 72f), HandleUlangi);
        BuatTombol(holder.transform, lanjutTeks, lanjutWarna, lanjutBorder, lanjutUkuranTeks, new Vector2(380f, 72f), HandleLanjut);
    }

    void BuatTombol(Transform parent, string teks, Color bg, Color border, int ukuran, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("Btn_" + teks);
        go.transform.SetParent(parent, false);
        var im = go.AddComponent<Image>();
        im.sprite = GetRoundedSprite(); im.color = bg; im.type = Image.Type.Sliced;
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = size.x; le.preferredHeight = size.y;
        var outl = go.AddComponent<Outline>();
        outl.effectColor = border; outl.effectDistance = new Vector2(2f, -2f);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = im;
        var colors = btn.colors;
        colors.highlightedColor = new Color(bg.r * 1.15f, bg.g * 1.15f, bg.b * 1.15f, bg.a);
        colors.pressedColor = new Color(bg.r * 0.85f, bg.g * 0.85f, bg.b * 0.85f, bg.a);
        btn.colors = colors;

        var t = BuatTeks(go.transform, "Label", teks, ukuran, Color.white, FontStyles.Bold);
        t.alignment = TextAlignmentOptions.Center;
        var trt = t.rectTransform;
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        btn.onClick.AddListener(onClick);
    }

    void HandleUlangi()
    {
        AudioManager.Instance?.Click();
        Tutup();

        // PRIORITAS 1: UnityEvent dari Inspector
        if (onUlangiHari2 != null && onUlangiHari2.GetPersistentEventCount() > 0)
        {
            onUlangiHari2.Invoke();
            return;
        }
        // PRIORITAS 2: DayTransitionManager (single-scene)
        if (DayTransitionManager.Instance != null)
        {
            DayTransitionManager.Instance.UlangiHari2();
            return;
        }
        // PRIORITAS 3 (fallback): reload scene
        if (ulangiReloadScene)
        {
            var gs = GameState.Instance;
            if (gs != null) { gs.day = 2; }
            var active = SceneManager.GetActiveScene().name;
            if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene(active);
            else SceneManager.LoadScene(active);
        }
    }

    void HandleLanjut()
    {
        if (sfxKlikLanjut != null) AudioManager.Instance?.sfxSource?.PlayOneShot(sfxKlikLanjut);
        else AudioManager.Instance?.Click();
        Tutup();

        // ── JAMINAN NAVBAR (sama pola seperti Day1SummaryScreen) ──────────
        if (HUDManager.Instance != null) HUDManager.Instance.OnLanjutHari3();

        // PRIORITAS 1: UnityEvent dari Inspector
        if (onLanjutHari3 != null && onLanjutHari3.GetPersistentEventCount() > 0)
        {
            onLanjutHari3.Invoke();
            return;
        }
        // PRIORITAS 2: DayTransitionManager (single-scene)
        if (DayTransitionManager.Instance != null)
        {
            DayTransitionManager.Instance.LanjutKeDay3();
            return;
        }
        // PRIORITAS 3 (fallback): load scene Day 3
        if (!string.IsNullOrEmpty(lanjutSceneName))
        {
            var gs = GameState.Instance;
            if (gs != null) gs.day = 3;
            if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene(lanjutSceneName);
            else SceneManager.LoadScene(lanjutSceneName);
        }
        else
        {
            Debug.LogWarning("[Day2SummaryScreen] Tidak ada DayTransitionManager / onLanjutHari3 / lanjutSceneName.");
        }
    }

    // Helpers
    void BuatPemisah(Transform parent)
    {
        var sep = new GameObject("Pemisah");
        sep.transform.SetParent(parent, false);
        var img = sep.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.16f); img.raycastTarget = false;
        var le = sep.AddComponent<LayoutElement>();
        le.preferredHeight = 2f; le.minHeight = 2f; le.flexibleWidth = 1f;
    }

    string WarnaKategoriHex(string kategori)
    {
        switch (kategori)
        {
            case "AMAN":   return "#26AD61";
            case "RAGU":   return "#F29D12";
            case "BAHAYA": return "#E84D3D";
            default:       return "#339FDB";
        }
    }

    TextMeshProUGUI BuatTeks(Transform parent, string name, string content, int size, Color color, FontStyles style)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        TMP_FontAsset f = fontAsset ?? TMP_Settings.defaultFontAsset;
        if (f != null) tmp.font = f;
        tmp.text = content; tmp.fontSize = size; tmp.color = color; tmp.fontStyle = style;
        tmp.textWrappingMode = TextWrappingModes.Normal; tmp.raycastTarget = false;
        return tmp;
    }

    Sprite GetRoundedSprite()
    {
        if (_roundedSprite != null) return _roundedSprite;
        int size = 64; int radius = 14;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp; tex.filterMode = FilterMode.Bilinear;
        Color32 w = new Color32(255,255,255,255), c = new Color32(255,255,255,0);
        for (int y=0;y<size;y++) for (int x=0;x<size;x++)
        {
            bool inside = true;
            if      (x<radius && y<radius)             { int dx=radius-x, dy=radius-y; inside = dx*dx+dy*dy <= radius*radius; }
            else if (x>=size-radius && y<radius)       { int dx=x-(size-1-radius), dy=radius-y; inside = dx*dx+dy*dy <= radius*radius; }
            else if (x<radius && y>=size-radius)       { int dx=radius-x, dy=y-(size-1-radius); inside = dx*dx+dy*dy <= radius*radius; }
            else if (x>=size-radius && y>=size-radius) { int dx=x-(size-1-radius), dy=y-(size-1-radius); inside = dx*dx+dy*dy <= radius*radius; }
            tex.SetPixel(x, y, inside ? (Color)w : (Color)c);
        }
        tex.Apply();
        _roundedSprite = Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius,radius,radius,radius));
        return _roundedSprite;
    }
}
