using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// AngkotSeatPicker — Fase pilih tempat duduk di dalam angkot.
///
/// Tampilkan interior angkot dengan 3 kursi:
///   - Kursi DEKAT PINTU (AMAN)   : mudah turun & dekat supir
///   - Kursi DEPAN (RAGU)         : ramai tapi terbatas
///   - Kursi POJOK BELAKANG (BAHAYA): sepi & gelap, ada pria mencurigakan
///
/// Setelah pemain pilih:
///   - Cek plat nomor (toggle bonus +50 poin)
///   - Lanjut ke fase berikutnya
///
/// Custom semua label/warna lewat Inspector.
/// </summary>
public class AngkotSeatPicker : MonoBehaviour
{
    [System.Serializable]
    public class Kursi
    {
        public string label    = "Dekat Pintu";
        public string kategori = "AMAN"; // "AMAN" | "RAGU" | "BAHAYA"
        [TextArea(2, 4)]
        public string deskripsi = "Dekat pak supir, mudah turun cepat.";
        [TextArea(2, 4)]
        public string reaksi    = "\u2713 Pilihan tepat! Kamu duduk dekat supir.";

        [Tooltip("Alur narasi bertahap setelah kursi ini dipilih.\n" +
                 "Tiap entry = satu beat (muncul satu per satu, typewriter + klik lanjut).\n" +
                 "Kosongkan = fallback ke 'reaksi' (dipecah otomatis per baris).")]
        [TextArea(1, 3)]
        public string[] reaksiBeats = new string[0];

        public Color warna     = new Color(0.18f, 0.62f, 0.32f, 1f);
        public Vector2 posisi  = new Vector2(-450f, -50f);
        public Vector2 ukuran  = new Vector2(280f, 200f);

        [Tooltip("Sprite latar FULLSCREEN yang tampil saat kursi ini DIPILIH.\n" +
                 "Kosongkan = pakai latar reaksi per-kategori, fallback ke bgFullscreenSprite default.")]
        public Sprite latarSaatDipilih;
    }

    // Jembatan script-to-script: kategori kursi yang TERAKHIR dipilih pemain.
    // Dibaca AngkotSentuhScene.PilihIntroSesuaiKursi() untuk memilih varian narasi
    // intro (Aman/Ragu/Bahaya) tanpa bergantung pada GameState. Direset saat Mulai().
    public static string KategoriKursiDipilih = "";

    // -- DEPRECATED -- field interior procedural di-hide karena BG sekarang dari sprite saja.
    [HideInInspector] public Sprite angkotInteriorSprite;
    [HideInInspector] public Color warnaLantai  = new Color(0.10f, 0.07f, 0.05f, 1f);
    [HideInInspector] public Color warnaJendela = new Color(0.40f, 0.55f, 0.65f, 0.8f);
    [HideInInspector] public Color warnaBingkai = new Color(0.18f, 0.12f, 0.08f, 1f);
    [HideInInspector] public Color warnaSupir   = new Color(0.55f, 0.40f, 0.30f, 1f);

    [Header("Judul Layar")]
    public string judulTeks = "Angkot datang. Di dalam sudah ada beberapa penumpang — termasuk seorang pria yang melirik ke arahmu. Pilih tempat dudukmu:";
    public Color  judulWarna = new Color(1f, 0.85f, 0.30f, 1f);
    public int    judulUkuran = 32;

    [Header("Daftar Kursi (CUSTOMIZABLE)")]
    public Kursi[] kursiList = new Kursi[]
    {
        new Kursi {
            label = "Dekat Pintu (depan)", kategori = "AMAN",
            deskripsi = "Dekat Pak Supir, gampang turun cepat kalau ada apa-apa.",
            reaksi    = "\u2713 Pintar! Kamu duduk dekat Pak Supir & pintu \u2014 posisi paling aman: gampang minta tolong & cepat turun kalau ada apa-apa.\nAngkot pun mulai berjalan menuju sekolah...",
            reaksiBeats = new string[]
            {
                "\u2713 Pintar! Kamu duduk dekat Pak Supir & pintu.",
                "Ini posisi paling aman \u2014 gampang minta tolong & cepat turun kalau ada apa-apa.",
                "Angkot pun mulai berjalan menuju sekolah..."
            },
            warna     = new Color(0.18f, 0.62f, 0.32f, 1f),
            posisi    = new Vector2(-500f, -40f),
            ukuran    = new Vector2(300f, 220f)
        },
        new Kursi {
            label = "Tengah (di samping ibu-ibu)", kategori = "RAGU",
            deskripsi = "Ramai tapi terjepit di tengah, agak susah turun.",
            reaksi    = "\u26A0 Kamu duduk di tengah, terhimpit di antara penumpang. Ramai memang, tapi kamu susah bergerak kalau terjadi sesuatu.\nAngkot pun mulai berjalan menuju sekolah...",
            reaksiBeats = new string[]
            {
                "\u26A0 Kamu duduk di tengah, terhimpit di antara penumpang.",
                "Ramai memang \u2014 tapi kamu susah bergerak kalau terjadi sesuatu.",
                "Angkot pun mulai berjalan menuju sekolah..."
            },
            warna     = new Color(0.95f, 0.62f, 0.07f, 1f),
            posisi    = new Vector2(0f, -40f),
            ukuran    = new Vector2(300f, 220f)
        },
        new Kursi {
            label = "Pojok Belakang (sepi)", kategori = "BAHAYA",
            deskripsi = "Sepi & gelap, ada pria asing yang dari tadi ngeliatin kamu.",
            reaksi    = "\u2716 Kamu memilih pojok belakang yang sepi & gelap, jauh dari Pak Supir. Posisi ini paling rawan.\nAngkot pun mulai berjalan menuju sekolah...",
            reaksiBeats = new string[]
            {
                "\u2716 Kamu memilih pojok belakang yang sepi & gelap.",
                "Jauh dari Pak Supir \u2014 posisi ini paling rawan.",
                "Angkot pun mulai berjalan menuju sekolah..."
            },
            warna     = new Color(0.91f, 0.30f, 0.24f, 1f),
            posisi    = new Vector2(500f, -40f),
            ukuran    = new Vector2(300f, 220f)
        }
    };

    [Header("Cek Plat Nomor (Bonus)")]
    [Tooltip("Aktifkan checkbox 'Cek plat nomor angkot' untuk bonus poin.")]
    public bool tampilkanCekPlat = true;
    public string platLabel = "\uD83D\uDCDD Catat plat nomor angkot (B 1234 XYZ)";
    public int    bonusPlat = 50;

    [Header("Tombol Lanjut")]
    public string tombolLanjutTeks = "\u25B6  Lanjut Perjalanan";
    public Color  warnaLanjut      = new Color(0.20f, 0.62f, 0.86f, 1f);

    [Header("Tata Letak / Layout (CUSTOMIZABLE)")]
    [Tooltip("Tinggi area judul di pojok atas (px).")]
    public float judulTinggi = 85f;
    [Tooltip("Margin kiri/kanan judul dari tepi layar (px).")]
    public float judulMarginSamping = 40f;
    [Tooltip("Ukuran (lebar x tinggi) panel deretan kursi di tengah layar.")]
    public Vector2 kursiPanelUkuran = new Vector2(1700f, 500f);
    [Tooltip("Posisi panel deretan kursi relatif tengah layar.")]
    public Vector2 kursiPanelPosisi = new Vector2(0f, 60f);
    [Tooltip("Margin kiri/kanan box dialog narasi dari tepi layar (px).")]
    public float boxMarginSamping = 60f;
    [Tooltip("Jarak dasar box dialog narasi dari tepi bawah layar (px).")]
    public float boxJarakBawah = 200f;
    [Tooltip("Tinggi box dialog narasi (px).")]
    public float boxTinggi = 230f;
    [Tooltip("Ukuran (lebar x tinggi) baris 'Cek Plat Nomor'.")]
    public Vector2 platRowUkuran = new Vector2(900f, 70f);
    [Tooltip("Jarak baris plat dari tepi bawah layar (px).")]
    public float platRowJarakBawah = 120f;
    [Tooltip("Ukuran (lebar x tinggi) tombol 'Lanjut Perjalanan'.")]
    public Vector2 lanjutUkuran = new Vector2(340f, 68f);
    [Tooltip("Jarak tombol lanjut dari tepi bawah layar (px).")]
    public float lanjutJarakBawah = 40f;

    [Header("BG Fullscreen Device (opsional)")]
    [Tooltip("Sprite latar FULLSCREEN device (stretch ke seluruh layar). Tampil paling belakang.\n" +
             "Kalau diisi → dipakai sebagai BG utama. Kalau kosong → fallback ke angkotInteriorSprite / procedural.")]
    public Sprite bgFullscreenSprite;
    [Tooltip("Jaga aspek rasio sprite saat di-stretch fullscreen (mencegah gepeng).")]
    public bool   bgFullscreenPreserveAspect = false;

    [Header("BG Reaksi per Kategori (opsional)")]
    [Tooltip("BG fullscreen yang tampil setelah pemain memilih kursi AMAN.\n" +
             "Kosongkan = tetap pakai bgFullscreenSprite default.")]
    public Sprite bgReaksiAman;
    [Tooltip("BG fullscreen yang tampil setelah pemain memilih kursi RAGU.")]
    public Sprite bgReaksiRagu;
    [Tooltip("BG fullscreen yang tampil setelah pemain memilih kursi BAHAYA (pria asing dekat).")]
    public Sprite bgReaksiBahaya;

    [Header("Font")]
    public TMP_FontAsset fontAsset;

    [Header("Narasi Reaksi Bertahap")]
    [Tooltip("Kecepatan typewriter per huruf (detik). 0 = langsung tampil penuh.")]
    public float kecepatanKetik = 0.02f;
    [Tooltip("Teks petunjuk lanjut yang tampil di bawah narasi reaksi.")]
    public string hintLanjutTeks = "";
    public Color  hintLanjutWarna = new Color(1f, 1f, 1f, 0.55f);

    [Header("Box Dialog Narasi Reaksi")]
    [Tooltip("Tampilkan box/panel di belakang teks narasi reaksi (biar teks tidak polos di layar).")]
    public bool tampilkanBoxDialog = true;
    [Tooltip("Sprite background box dialog (sliced). Kosong = pakai warna solid + outline.")]
    public Sprite boxDialogSprite;
    [Tooltip("Warna box dialog kalau boxDialogSprite kosong.")]
    public Color boxDialogWarna = new Color(0.06f, 0.05f, 0.08f, 0.9f);
    [Tooltip("Warna garis tepi box dialog (saat tanpa sprite).")]
    public Color boxDialogBorder = new Color(1f, 0.85f, 0.3f, 0.9f);

    [Header("Sorting")]
    public int sortingOrder = 920;

    // ── runtime ───────────────────────────────────────────────────────────
    private Action     _onSelesai;
    private GameObject _canvasGO;
    private TextMeshProUGUI _reaksiText;
    private Image           _bgFullscreenImg;
    private GameObject _kursiPanel;
    private GameObject _platRow;
    private GameObject _lanjutBtn;
    private Sprite     _roundedSprite;
    private bool       _platDicek;

    // narasi reaksi bertahap
    private TextMeshProUGUI _hintText;
    private GameObject      _reaksiBox;
    private GameObject      _klikCatcher;
    private Coroutine       _narasiCo;
    private bool            _ketikSelesai;
    private bool            _skipKetik;
    private bool            _bolehLanjut;

    // ══════════════════════════════════════════════════════════════════════
    public void Mulai(Action onSelesai)
    {
        _onSelesai = onSelesai;
        KategoriKursiDipilih = ""; // reset jembatan supaya tidak terbawa dari sesi sebelumnya
        HUDManager.Instance?.SetNavbarVisible(false); // sembunyikan navbar di layar pilih tempat duduk
        BuildScene();
    }

    // Ambil sprite awal BG: bgFullscreenSprite > latarSaatDipilih kursi pertama > bgReaksiAman > bgReaksiRagu > bgReaksiBahaya.
    Sprite AmbilSpriteAwal()
    {
        if (bgFullscreenSprite != null) return bgFullscreenSprite;
        if (kursiList != null)
            foreach (var k in kursiList) if (k != null && k.latarSaatDipilih != null) return k.latarSaatDipilih;
        if (bgReaksiAman   != null) return bgReaksiAman;
        if (bgReaksiRagu   != null) return bgReaksiRagu;
        if (bgReaksiBahaya != null) return bgReaksiBahaya;
        return null;
    }

    // ══════════════════════════════════════════════════════════════════════
    void BuildScene()
    {
        _canvasGO = new GameObject("AngkotSeatPicker_Canvas");
        var canvas = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = _canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        _canvasGO.AddComponent<GraphicRaycaster>();

        // BG fullscreen SELALU dibuat (1 Image stretch fullscreen).
        // Sprite awal: bgFullscreenSprite > Kursi.latarSaatDipilih[0] > bgReaksi*. Diganti runtime saat pemain memilih kursi.
        {
            var fs = new GameObject("BG_Fullscreen");
            fs.transform.SetParent(_canvasGO.transform, false);
            _bgFullscreenImg = fs.AddComponent<Image>();
            _bgFullscreenImg.sprite         = AmbilSpriteAwal();
            _bgFullscreenImg.color          = Color.white;
            _bgFullscreenImg.preserveAspect = false;
            _bgFullscreenImg.raycastTarget  = false;
            var fsRt = fs.GetComponent<RectTransform>();
            fsRt.anchorMin = Vector2.zero; fsRt.anchorMax = Vector2.one;
            fsRt.offsetMin = Vector2.zero; fsRt.offsetMax = Vector2.zero;
        }

        // Backdrop judul — pita gelap semi-transparan supaya teks instruksi
        // tetap terbaca di atas latar interior angkot yang ramai.
        {
            var jbg = new GameObject("JudulBackdrop");
            jbg.transform.SetParent(_canvasGO.transform, false);
            var jbgImg = jbg.AddComponent<Image>();
            jbgImg.sprite = GetRoundedSprite();
            jbgImg.type   = Image.Type.Sliced;
            jbgImg.color  = new Color(0.06f, 0.05f, 0.08f, 0.78f);
            jbgImg.raycastTarget = false;
            var jbgOutl = jbg.AddComponent<Outline>();
            jbgOutl.effectColor    = new Color(1f, 0.85f, 0.3f, 0.55f);
            jbgOutl.effectDistance = new Vector2(2f, -2f);
            var jbgRt = jbg.GetComponent<RectTransform>();
            jbgRt.anchorMin = new Vector2(0f, 1f); jbgRt.anchorMax = new Vector2(1f, 1f);
            jbgRt.pivot     = new Vector2(0.5f, 1f);
            jbgRt.offsetMin = new Vector2(judulMarginSamping - 12f, -(judulTinggi + 36f));
            jbgRt.offsetMax = new Vector2(-(judulMarginSamping - 12f), -16f);
        }

        // Judul
        var judul = BuatTeks(_canvasGO.transform, "Judul", judulTeks, judulUkuran, judulWarna, FontStyles.Bold);
        judul.alignment = TextAlignmentOptions.Center;
        var jrt = judul.rectTransform;
        jrt.anchorMin = new Vector2(0f, 1f); jrt.anchorMax = new Vector2(1f, 1f);
        jrt.pivot     = new Vector2(0.5f, 1f);
        jrt.offsetMin = new Vector2(judulMarginSamping, -(judulTinggi + 25f));
        jrt.offsetMax = new Vector2(-judulMarginSamping, -25f);

        // Panel kursi
        _kursiPanel = new GameObject("KursiPanel");
        _kursiPanel.transform.SetParent(_canvasGO.transform, false);
        var krt = _kursiPanel.AddComponent<RectTransform>();
        krt.anchorMin = new Vector2(0.5f, 0.5f); krt.anchorMax = new Vector2(0.5f, 0.5f);
        krt.pivot = new Vector2(0.5f, 0.5f);
        krt.sizeDelta = kursiPanelUkuran;
        krt.anchoredPosition = kursiPanelPosisi;

        foreach (var k in kursiList) BuatKursiButton(k);

        // ── Box dialog narasi reaksi (di belakang teks, awalnya hidden) ──
        _reaksiBox = new GameObject("ReaksiBox");
        _reaksiBox.transform.SetParent(_canvasGO.transform, false);
        var boxImg = _reaksiBox.AddComponent<Image>();
        if (boxDialogSprite != null)
        {
            boxImg.sprite = boxDialogSprite;
            boxImg.type   = Image.Type.Sliced;
            boxImg.color  = Color.white;
        }
        else
        {
            boxImg.sprite = GetRoundedSprite();
            boxImg.type   = Image.Type.Sliced;
            boxImg.color  = boxDialogWarna;
            var boxOutl = _reaksiBox.AddComponent<Outline>();
            boxOutl.effectColor    = boxDialogBorder;
            boxOutl.effectDistance = new Vector2(2f, -2f);
        }
        boxImg.raycastTarget = false;
        var boxRT = _reaksiBox.GetComponent<RectTransform>();
        boxRT.anchorMin = new Vector2(0f, 0f); boxRT.anchorMax = new Vector2(1f, 0f);
        boxRT.pivot     = new Vector2(0.5f, 0f);
        boxRT.offsetMin = new Vector2(boxMarginSamping, boxJarakBawah);
        boxRT.offsetMax = new Vector2(-boxMarginSamping, boxJarakBawah + boxTinggi);
        _reaksiBox.SetActive(false);

        // Reaksi area (initially kosong) — anak dari box dialog
        _reaksiText = BuatTeks(_reaksiBox.transform, "Reaksi", "", 24, new Color(1f,1f,0.92f,1f), FontStyles.Normal);
        _reaksiText.alignment = TextAlignmentOptions.Center;
        var rrt = _reaksiText.rectTransform;
        rrt.anchorMin = new Vector2(0f, 0f); rrt.anchorMax = new Vector2(1f, 1f);
        rrt.pivot     = new Vector2(0.5f, 0.5f);
        rrt.offsetMin = new Vector2(40f, 70f);
        rrt.offsetMax = new Vector2(-40f, -25f);

        // Hint "klik untuk lanjut" (hidden sampai narasi reaksi berjalan) — anak dari box
        _hintText = BuatTeks(_reaksiBox.transform, "HintLanjut", hintLanjutTeks, 20, hintLanjutWarna, FontStyles.Italic);
        _hintText.alignment = TextAlignmentOptions.Center;
        var hrt = _hintText.rectTransform;
        hrt.anchorMin = new Vector2(0f, 0f); hrt.anchorMax = new Vector2(1f, 0f);
        hrt.pivot     = new Vector2(0.5f, 0f);
        hrt.offsetMin = new Vector2(40f, 18f);
        hrt.offsetMax = new Vector2(-40f, 55f);
        // Hint teks 'Klik untuk lanjut' dihilangkan sesuai permintaan.
        _hintText.text = "";
        _hintText.gameObject.SetActive(false);

        // Baris "Catat plat nomor angkot" DIHAPUS dari tampilan (sesuai permintaan).
        // BuildPlatRow tidak dipanggil lagi; checkbox cek plat Day 2 tidak ditampilkan.
    }

    void BuildInteriorProcedural()
    {
        // Lantai
        var lantai = new GameObject("Lantai");
        lantai.transform.SetParent(_canvasGO.transform, false);
        var lImg = lantai.AddComponent<Image>();
        lImg.color = warnaLantai;
        lImg.raycastTarget = false;
        var lrt = lantai.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0f, 0f); lrt.anchorMax = new Vector2(1f, 0.35f);
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;

        // Jendela kiri
        BuatKotak("JendelaKiri", new Vector2(-680f, 220f), new Vector2(420f, 240f), warnaJendela);
        BuatKotak("BingkaiJndKiri", new Vector2(-680f, 220f), new Vector2(440f, 260f), warnaBingkai, true);

        // Jendela kanan
        BuatKotak("JendelaKanan", new Vector2(680f, 220f), new Vector2(420f, 240f), warnaJendela);
        BuatKotak("BingkaiJndKanan", new Vector2(680f, 220f), new Vector2(440f, 260f), warnaBingkai, true);

        // Supir di kiri atas (kepala)
        BuatKotak("Supir", new Vector2(-820f, 60f), new Vector2(110f, 130f), warnaSupir);
        var sLabel = BuatTeks(_canvasGO.transform, "SupirLabel", "Supir", 16, new Color(1f, 0.95f, 0.75f, 1f), FontStyles.Italic);
        sLabel.alignment = TextAlignmentOptions.Center;
        var slrt = sLabel.rectTransform;
        slrt.anchorMin = new Vector2(0.5f, 0.5f); slrt.anchorMax = new Vector2(0.5f, 0.5f);
        slrt.pivot = new Vector2(0.5f, 0.5f); slrt.sizeDelta = new Vector2(120f, 22f);
        slrt.anchoredPosition = new Vector2(-820f, -20f);
    }

    void BuatKotak(string name, Vector2 pos, Vector2 size, Color c, bool isBorder = false)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_canvasGO.transform, false);
        var img = go.AddComponent<Image>();
        img.color = c;
        img.raycastTarget = false;
        if (isBorder)
        {
            img.color = new Color(0,0,0,0);
            var outl = go.AddComponent<Outline>();
            outl.effectColor = c;
            outl.effectDistance = new Vector2(3f, -3f);
        }
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
    }

    void BuatKursiButton(Kursi k)
    {
        var go = new GameObject("Kursi_" + k.label);
        go.transform.SetParent(_kursiPanel.transform, false);

        // Panel dasar: gelap & pekat supaya teks terbaca jelas di atas latar
        // interior angkot yang ramai. Garis tepi mengikuti warna kategori.
        var img = go.AddComponent<Image>();
        img.sprite = GetRoundedSprite();
        img.color  = new Color(0.10f, 0.09f, 0.07f, 0.95f);
        img.type   = Image.Type.Sliced;
        var outl = go.AddComponent<Outline>();
        outl.effectColor    = k.warna;
        outl.effectDistance = new Vector2(3f, -3f);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = k.ukuran;
        rt.anchoredPosition = k.posisi;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.18f, 0.15f, 0.11f, 0.98f);
        colors.pressedColor     = new Color(0.06f, 0.05f, 0.04f, 0.98f);
        btn.colors = colors;
        btn.onClick.AddListener(() => OnPilihKursi(k));

        // ── Header band kategori (warna kategori, di bagian atas kartu) ──
        var head = new GameObject("Header");
        head.transform.SetParent(go.transform, false);
        var headImg = head.AddComponent<Image>();
        headImg.sprite = GetRoundedSprite();
        headImg.type   = Image.Type.Sliced;
        headImg.color  = k.warna;
        headImg.raycastTarget = false;
        var hbRt = head.GetComponent<RectTransform>();
        hbRt.anchorMin = new Vector2(0f, 1f); hbRt.anchorMax = new Vector2(1f, 1f);
        hbRt.pivot     = new Vector2(0.5f, 1f);
        hbRt.offsetMin = new Vector2(7f, -56f);
        hbRt.offsetMax = new Vector2(-7f, -7f);

        var katTxt = BuatTeks(head.transform, "Kategori", BadgeKategori(k.kategori), 20, Color.white, FontStyles.Bold);
        katTxt.alignment = TextAlignmentOptions.Center;
        katTxt.enableAutoSizing = true; katTxt.fontSizeMin = 13f; katTxt.fontSizeMax = 20f;
        var ktRt = katTxt.rectTransform;
        ktRt.anchorMin = Vector2.zero; ktRt.anchorMax = Vector2.one;
        ktRt.offsetMin = new Vector2(8f, 4f); ktRt.offsetMax = new Vector2(-8f, -4f);

        // ── Label kursi (nama posisi tempat duduk) ──
        var lab = BuatTeks(go.transform, "Label", k.label, 20, Color.white, FontStyles.Bold);
        lab.alignment = TextAlignmentOptions.Center;
        lab.enableAutoSizing = true; lab.fontSizeMin = 14f; lab.fontSizeMax = 20f;
        var lrt = lab.rectTransform;
        lrt.anchorMin = new Vector2(0f, 0.5f); lrt.anchorMax = new Vector2(1f, 1f);
        lrt.offsetMin = new Vector2(10f, 8f);
        lrt.offsetMax = new Vector2(-10f, -62f);

        // ── Deskripsi singkat ──
        var desc = BuatTeks(go.transform, "Desc", k.deskripsi, 15, new Color(1f, 1f, 1f, 0.9f), FontStyles.Normal);
        desc.alignment = TextAlignmentOptions.Top;
        var drt = desc.rectTransform;
        drt.anchorMin = new Vector2(0f, 0f); drt.anchorMax = new Vector2(1f, 0.5f);
        drt.offsetMin = new Vector2(12f, 30f);
        drt.offsetMax = new Vector2(-12f, 2f);

        // ── Hint interaktif di bawah kartu ──
        var hint = BuatTeks(go.transform, "Hint", "TEKAN UNTUK PILIH", 13, new Color(1f, 0.95f, 0.7f, 0.75f), FontStyles.Italic);
        hint.alignment = TextAlignmentOptions.Center;
        var hntRt = hint.rectTransform;
        hntRt.anchorMin = new Vector2(0f, 0f); hntRt.anchorMax = new Vector2(1f, 0f);
        hntRt.pivot     = new Vector2(0.5f, 0f);
        hntRt.offsetMin = new Vector2(8f, 7f);
        hntRt.offsetMax = new Vector2(-8f, 28f);

        // ── Umpan balik interaktif: kartu sedikit membesar saat disorot/disentuh ──
        PasangHoverKursi(go);
    }

    // Label badge kategori yang ramah edukasi (warna + kata penjelas).
    string BadgeKategori(string kategori)
    {
        switch (kategori)
        {
            case "AMAN":   return "PILIHAN AMAN";
            case "RAGU":   return "KURANG AMAN";
            case "BAHAYA": return "BERBAHAYA";
            default:       return kategori;
        }
    }

    // Pasang efek hover (membesar sedikit) pada kartu kursi untuk kesan interaktif.
    void PasangHoverKursi(GameObject kartu)
    {
        var trig = kartu.AddComponent<EventTrigger>();
        Vector3 normal  = Vector3.one;
        Vector3 disorot = Vector3.one * 1.06f;

        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ => { if (kartu != null) kartu.transform.localScale = disorot; });
        trig.triggers.Add(enter);

        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => { if (kartu != null) kartu.transform.localScale = normal; });
        trig.triggers.Add(exit);
    }

    void OnPilihKursi(Kursi k)
    {
        AudioManager.Instance?.Click();

        // Nonaktifkan semua tombol kursi
        foreach (Transform t in _kursiPanel.transform)
        {
            var b = t.GetComponent<Button>(); if (b != null) b.interactable = false;
            var img = t.GetComponent<Image>(); if (img != null && t.gameObject != FindKursiGO(k)) img.color = new Color(img.color.r, img.color.g, img.color.b, 0.35f);
        }

        // Jembatan langsung ke AngkotSentuhScene (tidak bergantung GameState).
        KategoriKursiDipilih = k.kategori;

        // Catat ke GameState
        var gs = GameState.Instance;
        if (gs != null)
        {
            gs.AddChoice(2, "Pilih kursi: " + k.label, k.kategori);
            // Simpan kategori kursi supaya fase berikutnya (ZonaTubuhQuiz)
            // bisa memilih varian narasi yang menyambung pilihan pemain.
            gs.seatCategory = k.kategori;
            if (k.kategori == "BAHAYA")
            {
                gs.lives = Mathf.Max(0, gs.lives - 1);
                Debug.Log($"[AngkotSeatPicker] Pilih BAHAYA \u2192 nyawa -1 (sisa {gs.lives})");
            }
            else if (k.kategori == "AMAN" && tampilkanCekPlat && !gs.platChecked)
            {
                // Pengganti baris "Catat plat nomor" yang dihapus: bukti cek plat
                // Hari 2 + bonus poin kini otomatis diraih saat memilih kursi AMAN
                // (Rara duduk dekat supir = paling waspada, sempat mencatat plat).
                gs.platChecked = true;
                gs.score += bonusPlat;
                gs.TambahBukti(GameState.BUKTI_PLAT_DAY2);
                Debug.Log($"[AngkotSeatPicker] Kursi AMAN \u2192 bukti plat Day2 + {bonusPlat} poin");
            }
        }

        AudioClip sfx = k.kategori switch
        {
            "AMAN"   => AudioManager.Instance?.sfxAman,
            "RAGU"   => AudioManager.Instance?.sfxRagu,
            "BAHAYA" => AudioManager.Instance?.sfxBahaya,
            _        => null
        };
        if (sfx != null) AudioManager.Instance.sfxSource.PlayOneShot(sfx);

        // Ganti BG fullscreen sesuai prioritas: kursi.latarSaatDipilih → bgReaksi<Kategori> → tetap default.
        if (_bgFullscreenImg != null)
        {
            Sprite spriteReaksi = k.latarSaatDipilih;
            if (spriteReaksi == null)
            {
                spriteReaksi = k.kategori switch
                {
                    "AMAN"   => bgReaksiAman,
                    "RAGU"   => bgReaksiRagu,
                    "BAHAYA" => bgReaksiBahaya,
                    _        => null
                };
            }
            if (spriteReaksi != null)
            {
                _bgFullscreenImg.sprite         = spriteReaksi;
                _bgFullscreenImg.color          = Color.white;
                _bgFullscreenImg.preserveAspect = false;
                // Pastikan tetap fullscreen stretch
                var bgRt = _bgFullscreenImg.rectTransform;
                bgRt.anchorMin = Vector2.zero;
                bgRt.anchorMax = Vector2.one;
                bgRt.offsetMin = Vector2.zero;
                bgRt.offsetMax = Vector2.zero;
                _bgFullscreenImg.transform.SetAsFirstSibling();
            }
        }

        // Putar alur narasi reaksi bertahap, baru tampilkan plat + tombol lanjut.
        if (_narasiCo != null) StopCoroutine(_narasiCo);
        _narasiCo = StartCoroutine(JalankanNarasiReaksi(k));
    }

    // Pecah reaksi kursi jadi daftar beat. Pakai reaksiBeats kalau ada,
    // selain itu fallback memecah string 'reaksi' per baris.
    List<string> SusunBeatReaksi(Kursi k)
    {
        var beats = new List<string>();
        if (k.reaksiBeats != null)
            foreach (var b in k.reaksiBeats)
                if (!string.IsNullOrWhiteSpace(b)) beats.Add(b.Trim());

        if (beats.Count == 0 && !string.IsNullOrWhiteSpace(k.reaksi))
            foreach (var seg in k.reaksi.Split('\n'))
                if (!string.IsNullOrWhiteSpace(seg)) beats.Add(seg.Trim());

        if (beats.Count == 0) beats.Add(k.reaksi ?? "");
        return beats;
    }

    IEnumerator JalankanNarasiReaksi(Kursi k)
    {
        var beats = SusunBeatReaksi(k);

        // Setelah memilih, kartu pilihan kursi DIHILANGKAN supaya layar fokus ke
        // dialog reaksi saja.
        if (_kursiPanel != null) _kursiPanel.SetActive(false);

        // Munculkan box dialog di belakang teks narasi (kalau diaktifkan).
        if (_reaksiBox != null) _reaksiBox.SetActive(tampilkanBoxDialog);

        // Click-catcher fullscreen supaya klik di mana pun memajukan narasi.
        PastikanKlikCatcher();
        _klikCatcher.SetActive(true);
        if (_hintText != null) _hintText.gameObject.SetActive(true);

        for (int i = 0; i < beats.Count; i++)
        {
            yield return StartCoroutine(KetikBeat(beats[i]));
            _bolehLanjut = false;
            while (!_bolehLanjut) yield return null;
            AudioManager.Instance?.Click();
        }

        // Narasi selesai → sembunyikan catcher & hint, munculkan plat + tombol lanjut.
        if (_klikCatcher != null) _klikCatcher.SetActive(false);
        if (_hintText != null) _hintText.gameObject.SetActive(false);
        if (_platRow != null) _platRow.SetActive(true);
        if (_lanjutBtn == null) BuildTombolLanjut();
    }

    IEnumerator KetikBeat(string teks)
    {
        _ketikSelesai = false;
        _skipKetik    = false;
        teks = teks ?? "";

        if (kecepatanKetik <= 0f)
        {
            _reaksiText.text = teks;
        }
        else
        {
            _reaksiText.text = "";
            for (int i = 0; i < teks.Length; i++)
            {
                if (_skipKetik) { _reaksiText.text = teks; break; }
                _reaksiText.text += teks[i];
                if (teks[i] != ' ') AudioManager.Instance?.PlayKetikHuruf();
                yield return new WaitForSeconds(kecepatanKetik);
            }
        }
        _reaksiText.text = teks;
        _ketikSelesai = true;
    }

    // Klik narasi: klik pertama menyelesaikan ketikan, klik berikutnya lanjut beat.
    void OnKlikNarasi()
    {
        if (!_ketikSelesai) _skipKetik = true;
        else _bolehLanjut = true;
    }

    void PastikanKlikCatcher()
    {
        if (_klikCatcher != null) return;
        _klikCatcher = new GameObject("KlikCatcher");
        _klikCatcher.transform.SetParent(_canvasGO.transform, false);
        var img = _klikCatcher.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0f); // transparan, hanya untuk menangkap klik
        var rt = _klikCatcher.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var btn = _klikCatcher.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(OnKlikNarasi);
    }

    GameObject FindKursiGO(Kursi k)
    {
        var t = _kursiPanel.transform.Find("Kursi_" + k.label);
        return t != null ? t.gameObject : null;
    }

    void BuildPlatRow()
    {
        _platRow = new GameObject("PlatRow");
        _platRow.transform.SetParent(_canvasGO.transform, false);
        var rt = _platRow.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f); rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = platRowUkuran;
        rt.anchoredPosition = new Vector2(0f, platRowJarakBawah);

        var bg = _platRow.AddComponent<Image>();
        bg.sprite = GetRoundedSprite();
        bg.color  = new Color(0.10f, 0.10f, 0.12f, 0.85f);
        bg.type   = Image.Type.Sliced;

        var hLay = _platRow.AddComponent<HorizontalLayoutGroup>();
        hLay.childAlignment = TextAnchor.MiddleLeft;
        hLay.spacing = 16f;
        hLay.padding = new RectOffset(20, 20, 8, 8);

        // Toggle box
        var box = new GameObject("Box");
        box.transform.SetParent(_platRow.transform, false);
        var bImg = box.AddComponent<Image>();
        bImg.sprite = GetRoundedSprite();
        bImg.color  = new Color(0.95f, 0.95f, 0.95f, 1f);
        bImg.type   = Image.Type.Sliced;
        var bLe = box.AddComponent<LayoutElement>();
        bLe.preferredWidth = 40f; bLe.preferredHeight = 40f;
        var bBtn = box.AddComponent<Button>();
        bBtn.targetGraphic = bImg;

        var check = BuatTeks(box.transform, "Check", "", 28, new Color(0.15f, 0.68f, 0.38f, 1f), FontStyles.Bold);
        check.alignment = TextAlignmentOptions.Center;
        var crt = check.rectTransform;
        crt.anchorMin = Vector2.zero; crt.anchorMax = Vector2.one;
        crt.offsetMin = Vector2.zero; crt.offsetMax = Vector2.zero;

        // Label
        var lab = BuatTeks(_platRow.transform, "Label", platLabel + $"  (+{bonusPlat} poin)", 20, new Color(1f, 0.95f, 0.85f, 1f), FontStyles.Normal);
        lab.alignment = TextAlignmentOptions.MidlineLeft;
        var lLe = lab.gameObject.AddComponent<LayoutElement>();
        lLe.preferredWidth = 760f; lLe.preferredHeight = 40f;

        bBtn.onClick.AddListener(() =>
        {
            _platDicek = !_platDicek;
            check.text = _platDicek ? "\u2713" : "";
            AudioManager.Instance?.Click();
            var gs = GameState.Instance;
            if (gs != null)
            {
                gs.platChecked = _platDicek;
                if (_platDicek) { gs.score += bonusPlat; gs.TambahBukti(GameState.BUKTI_PLAT_DAY2); }
                else gs.score = Mathf.Max(0, gs.score - bonusPlat);
            }
        });
    }

    void BuildTombolLanjut()
    {
        _lanjutBtn = new GameObject("LanjutBtn");
        _lanjutBtn.transform.SetParent(_canvasGO.transform, false);
        var img = _lanjutBtn.AddComponent<Image>();
        img.sprite = GetRoundedSprite();
        img.color  = warnaLanjut;
        img.type   = Image.Type.Sliced;
        var outl = _lanjutBtn.AddComponent<Outline>();
        outl.effectColor    = new Color(1f, 1f, 1f, 0.4f);
        outl.effectDistance = new Vector2(2f, -2f);
        var rt = _lanjutBtn.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f); rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = lanjutUkuran;
        rt.anchoredPosition = new Vector2(0f, lanjutJarakBawah);

        var btn = _lanjutBtn.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() =>
        {
            AudioManager.Instance?.Click();
            HUDManager.Instance?.SetNavbarVisible(true); // tampilkan kembali navbar saat keluar
            if (_canvasGO != null) Destroy(_canvasGO);
            _onSelesai?.Invoke();
        });

        var lab = BuatTeks(_lanjutBtn.transform, "Label", tombolLanjutTeks, 24, Color.white, FontStyles.Bold);
        lab.alignment = TextAlignmentOptions.Center;
        var lrt = lab.rectTransform;
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
    }

    // ══════════════════════════════════════════════════════════════════════
    TextMeshProUGUI BuatTeks(Transform parent, string name, string content, int size, Color color, FontStyles style)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) tmp.font = fontAsset;
        else if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
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
