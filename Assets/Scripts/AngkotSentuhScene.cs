using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// AngkotSentuhScene — Adegan interaktif "Pria Sentuh Bahu" di dalam angkot (Day 2).
///
/// GAYA KOMIK: layar dibagi dua —
///   • ATAS  : panel GAMBAR (sprite komik per-beat). Bisa di-setup nanti via Inspector.
///   • BAWAH : kotak NARASI / dialog dengan efek mengetik.
///
/// Alur:
///   1. Mainkan beat-beat intro varian sesuai kursi pilihan pemain di fase Angkot
///      (panelIntroAman / panelIntroRagu / panelIntroBahaya) — cerita pria mendekat
///      & menyentuh bahu, disesuaikan posisi duduk Rara.
///   2. Munculkan 3 PILIHAN interaktif (AMAN / RAGU / BAHAYA).
///   3. Tampilkan reaksi (gambar + teks) sesuai pilihan + atur skor/nyawa.
///   4. Kalau pilihan AMAN → mainkan beat "panelLapor" (pindah kursi + lapor supir).
///   5. Tombol "Lanjut" → panggil onSelesai.
///
/// Semua GAMBAR opsional — kalau sprite kosong, ditampilkan KOTAK PLACEHOLDER
/// berlabel supaya jelas di mana harus mengisi sprite nanti. Tidak butuh prefab.
/// </summary>
public class AngkotSentuhScene : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // DATA
    // ══════════════════════════════════════════════════════════════════════
    [System.Serializable]
    public class PanelKomik
    {
        [Tooltip("Nama pembicara (mis. 'Narasi', 'Pria Asing', 'Rara', 'Pak Supir').")]
        public string speaker = "Narasi";
        [TextArea(2, 4)]
        [Tooltip("Teks narasi/dialog yang tampil di kotak bawah.")]
        public string narasi = "";
        [Tooltip("Sprite GAMBAR komik untuk beat ini (opsional, set nanti).")]
        public Sprite gambar;
        [Tooltip("Warna tint gambar. Putih = warna asli.")]
        public Color tintGambar = Color.white;
        [Tooltip("Sprite LATAR BELAKANG fullscreen khusus beat ini (opsional).\n" +
                 "Kalau diisi \u2192 background penuh layar langsung ganti ke sprite ini saat beat tampil.\n" +
                 "Kalau kosong \u2192 tetap pakai latar beat sebelumnya / bgSprite default.")]
        public Sprite latarSprite;
    }

    [System.Serializable]
    public class PilihanSentuh
    {
        [Tooltip("Teks tombol pilihan.")]
        public string label = "";
        [Tooltip("Kategori: AMAN | RAGU | BAHAYA.")]
        public string kategori = "AMAN";
        [TextArea(2, 4)]
        [Tooltip("Reaksi yang muncul setelah pilihan dipilih.")]
        public string reaksi = "";
        [Tooltip("Gambar komik reaksi (opsional, set nanti).")]
        public Sprite gambarReaksi;
        [Tooltip("Sprite LATAR BELAKANG fullscreen saat reaksi ini tampil (opsional).")]
        public Sprite latarReaksi;
        [Tooltip("Kurangi 1 nyawa saat pilihan ini dipilih.")]
        public bool kurangiNyawa = false;
        [Tooltip("Setelah reaksi, mainkan beat 'panelLapor' (pindah kursi + lapor supir).")]
        public bool lanjutLapor = false;
    }

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR
    // ══════════════════════════════════════════════════════════════════════
    [Header("Background Fullscreen (opsional — set nanti)")]
    [Tooltip("Sprite latar belakang penuh layar (mis. interior angkot). Kosong = warna solid.")]
    public Sprite bgSprite;
    [Tooltip("Warna solid background saat bgSprite kosong.")]
    public Color bgWarna = new Color(0.16f, 0.12f, 0.09f, 1f);
    [Tooltip("Jaga aspek rasio bgSprite (cegah gepeng).")]
    public bool bgPreserveAspect = false;

    [Header("Panel Gambar Komik (atas)")]
    [Tooltip("Tinggi area gambar sebagai fraksi tinggi layar (0–1).")]
    [Range(0.3f, 0.85f)] public float tinggiAreaGambar = 0.60f;
    [Tooltip("Tipe render sprite gambar komik.")]
    public Image.Type tipeGambar = Image.Type.Simple;
    [Tooltip("Jaga aspek rasio gambar komik.")]
    public bool gambarPreserveAspect = true;
    [Tooltip("Warna kotak placeholder saat gambar belum diisi.")]
    public Color placeholderWarna = new Color(0.10f, 0.10f, 0.14f, 1f);
    [Tooltip("Warna border panel gambar.")]
    public Color borderGambar = new Color(1f, 0.85f, 0.30f, 1f);

    [Header("Kotak Narasi (bawah)")]
    public Color panelNarasiWarna = new Color(0.05f, 0.04f, 0.03f, 0.95f);
    public Color namaWarna        = new Color(1f, 0.85f, 0.30f, 1f);
    public Color teksWarna        = Color.white;
    public int   namaUkuran       = 26;
    public int   teksUkuran       = 26;
    [Tooltip("Teks hint pojok kanan-bawah.")]
    public string hintTeks = "\u25BC  Klik / SPACE untuk lanjut";

    [Header("Animasi Mengetik")]
    [Range(0f, 0.15f)] public float kecepatanKetik = 0.025f;
    public bool bolehSkipKetik = true;

    // ══════════════════════════════════════════════════════════════════════
    // MODE VISUAL NOVEL — box dialog + portrait (gaya HalteDialog/Day1Intro)
    // Saat ON: layar pakai 1 kotak dialog bawah (portrait kiri + nama + teks),
    // BUKAN panel komik atas. Latar tetap fullscreen (bgSprite / latarSprite beat).
    // Semua sprite OPSIONAL → bisa di-upload nanti lewat Inspector.
    // ══════════════════════════════════════════════════════════════════════
    [Header("Mode Visual Novel (Box Dialog + Portrait)")]
    [Tooltip("ON = tampilan Visual Novel (box dialog bawah + portrait), seperti fase Halte.\n" +
             "OFF = tampilan komik lama (panel gambar besar di atas).")]
    public bool modeVisualNovel = true;
    [Tooltip("Sprite panel kayu untuk kotak dialog (sliced). Kosong = panel rounded gelap + border emas.")]
    public Sprite vnPanelSprite;
    [Tooltip("Warna tint panel saat vnPanelSprite di-assign.")]
    public Color vnPanelTint = Color.white;

    [Header("Mode VN \u2014 Portrait per Pembicara (upload nanti)")]
    [Tooltip("Portrait untuk speaker 'Narasi' (mis. gulungan kertas / scroll).")]
    public Sprite portraitNarasi;
    [Tooltip("Portrait wajah Rara untuk box dialog.")]
    public Sprite portraitRara;
    [Tooltip("Portrait Pria Asing. Kosong = fallback ke portraitNarasi.")]
    public Sprite portraitPriaAsing;
    [Tooltip("Portrait Pak Supir. Kosong = fallback ke portraitNarasi.")]
    public Sprite portraitPakSupir;
    [Tooltip("Jaga aspek rasio portrait (cegah gepeng).")]
    public bool vnPortraitPreserveAspect = true;

    [Header("Mode VN \u2014 Layout (anchor 0\u20131 layar; box-relatif utk isi)")]
    [Tooltip("Area kotak dialog di layar (anchorMin/Max). Default = mirror box Day 1 intro.")]
    public Vector2 vnBoxAnchorMin = new Vector2(0.014f, 0.0145f);
    public Vector2 vnBoxAnchorMax = new Vector2(0.986f, 0.3055f);
    [Tooltip("Area portrait DI DALAM box (relatif box 0\u20131).")]
    public Vector2 vnPortraitAnchorMin = new Vector2(0.0455f, 0.304f);
    public Vector2 vnPortraitAnchorMax = new Vector2(0.2345f, 0.864f);
    [Tooltip("Area banner nama pembicara DI DALAM box.")]
    public Vector2 vnNamaAnchorMin = new Vector2(0.11f, 0.11f);
    public Vector2 vnNamaAnchorMax = new Vector2(0.253f, 0.333f);
    [Tooltip("Area teks isi dialog DI DALAM box.")]
    public Vector2 vnTeksAnchorMin = new Vector2(0.31f, 0.55f);
    public Vector2 vnTeksAnchorMax = new Vector2(0.84f, 0.76f);
    [Tooltip("Warna nama & teks VN.")]
    public Color vnNamaWarna = new Color(1f, 0.85f, 0.30f, 1f);
    public Color vnTeksWarna = Color.white;
    public int   vnNamaUkuran = 30;
    public int   vnTeksUkuran = 28;

    [Header("Beat Intro \u2014 Varian Sesuai Pilihan Kursi (otomatis dipilih)")]
    [Tooltip("Dimainkan kalau Rara memilih kursi DEKAT PAK SUPIR (kategori AMAN).\n" +
             "Pria asing tetap mencoba mendekat, TAPI posisi Rara dekat sopir & pintu\n" +
             "membuatnya gampang minta tolong.")]
    public List<PanelKomik> panelIntroAman = new List<PanelKomik>
    {
        new PanelKomik { speaker = "Narasi", narasi = "Rara duduk tepat di belakang Pak Supir, dekat pintu. Dari sini ia bisa melihat seluruh isi angkot." },
        new PanelKomik { speaker = "Narasi", narasi = "Seorang pria asing yang tadi duduk di bangku belakang (bukan yang di halte) berdiri dan pindah, ikut duduk merapat di sebelah Rara." },
        new PanelKomik { speaker = "Pria Asing", narasi = "Sekolah di mana, dek? Sini deket om aja, biar nggak desak-desakan." },
        new PanelKomik { speaker = "Narasi", narasi = "Tangan pria itu menyentuh bahu Rara! Tapi Pak Supir ada tepat di depan \u2014 Rara tahu ia bisa segera minta tolong." }
    };
    [Tooltip("Dimainkan kalau Rara memilih kursi TENGAH di antara ibu-ibu (kategori RAGU).\n" +
             "Ada penumpang lain di sekitar (calon saksi), tapi Rara terjepit & sulit\n" +
             "bergerak.")]
    public List<PanelKomik> panelIntroRagu = new List<PanelKomik>
    {
        new PanelKomik { speaker = "Narasi", narasi = "Rara duduk berdesakan di tengah, terhimpit di antara ibu-ibu yang membawa belanjaan." },
        new PanelKomik { speaker = "Narasi", narasi = "Dari bangku belakang, seorang pria asing (bukan yang di halte) menyusup dan memaksakan diri duduk di sela sempit, merapat ke sisi Rara." },
        new PanelKomik { speaker = "Pria Asing", narasi = "Geser dikit dong, dek. Biar om bisa duduk dekat kamu." },
        new PanelKomik { speaker = "Narasi", narasi = "Tangan pria itu menyentuh bahu Rara! Ibu-ibu di sekitar mulai melirik, tapi Rara terjepit dan susah bergerak." }
    };
    [Tooltip("Dimainkan kalau Rara memilih POJOK BELAKANG yang sepi (kategori BAHAYA).\n" +
             "Rara sendirian di sebelah pria asing, jauh dari Pak Supir \u2014 paling rawan.")]
    public List<PanelKomik> panelIntroBahaya = new List<PanelKomik>
    {
        new PanelKomik { speaker = "Narasi", narasi = "Rara duduk sendirian di pojok belakang yang sepi. Tak ada penumpang lain di dekatnya." },
        new PanelKomik { speaker = "Narasi", narasi = "Pria asing yang sedari tadi memperhatikannya (bukan yang di halte) kini duduk persis di sebelah Rara. Tak ada siapa pun yang melihat." },
        new PanelKomik { speaker = "Pria Asing", narasi = "Tenang, dek... om temani kamu sampai sekolah ya. Deket-deket om aja." },
        new PanelKomik { speaker = "Narasi", narasi = "Tangan pria itu langsung menyentuh bahu Rara! Pojok ini jauh dari Pak Supir \u2014 Rara harus berani bertindak sendiri." }
    };

    [Header("Pilihan Interaktif")]
    public string judulPilihan = "Bahumu disentuh! Apa yang Rara lakukan?";
    public Color  warnaAman   = new Color(0.15f, 0.68f, 0.38f, 1f);
    public Color  warnaRagu   = new Color(0.95f, 0.62f, 0.07f, 1f);
    public Color  warnaBahaya = new Color(0.91f, 0.30f, 0.24f, 1f);
    public PilihanSentuh[] pilihanList = new PilihanSentuh[]
    {
        new PilihanSentuh {
            label = "\u201CJANGAN PEGANG SAYA!\u201D (teriak keras)",
            kategori = "AMAN", lanjutLapor = true,
            reaksi = "✓ HEBAT! Rara berani BERSUARA KERAS — ini kata sakti pertama: TIDAK!\nSemua penumpang menoleh ke arah pria itu. Sekarang lanjut: PERGI (pindah) & CERITA (lapor Pak Supir)."
        },
        new PilihanSentuh {
            label = "Geser menjauh diam-diam",
            kategori = "RAGU",
            reaksi = "⚠ Kamu menggeser badan menjauh, tapi pria itu masih terus mendekat.\nMenjauh saja belum cukup. Ingat 3 kata sakti: TIDAK (bersuara tegas) — PERGI (pindah) — CERITA (lapor Pak Supir)!"
        },
        new PilihanSentuh {
            label = "Diam saja karena takut",
            kategori = "BAHAYA", kurangiNyawa = true,
            reaksi = "✖ Kamu membeku dan diam, jadi pria itu makin berani. Kamu kehilangan 1 nyawa.\nDiam bukan salahmu — tapi kamu BISA melindungi diri. Lakukan 3 kata sakti: TIDAK — PERGI — CERITA (lapor Pak Supir)!"
        }
    };

    [Header("Beat Cerita — Setelah AMAN (PERGI / pindah kursi)")]
    [Tooltip("Dimainkan saat pilihan AMAN. Rara PINDAH menjauh (kata sakti kedua: PERGI),\n" +
             "TAPI situasi belum tuntas \u2014 pria masih satu angkot. Klimaks 'CERITA' (teriak\n" +
             "panggil Pak Supir) terjadi nanti di fase Lapor, bukan di sini.")]
    public List<PanelKomik> panelLapor = new List<PanelKomik>
    {
        new PanelKomik { speaker = "Narasi", narasi = "Rara langsung berdiri dan PINDAH ke kursi lebih depan, menjauh dari pria itu. (Itu kata sakti kedua: PERGI.)" },
        new PanelKomik { speaker = "Rara", narasi = "Aku sudah bersuara dan menjauh. Tapi dia masih satu angkot denganku \u2014 aku harus tetap siaga sampai turun." }
    };
    [Header("Beat Penutup — (KOSONG: tiba di sekolah dipindah ke fase Lapor)")]
    [Tooltip("Sengaja dikosongkan. Adegan 'angkot tiba di sekolah dengan selamat' kini\n" +
             "dimainkan di AKHIR fase Lapor (setelah Rara berani teriak panggil sopir),\n" +
             "bukan di akhir adegan sentuh ini. Isi lagi hanya jika ingin beat penutup\n" +
             "khusus di sini.")]
    public List<PanelKomik> panelSampaiSekolah = new List<PanelKomik>();
    [Header("Tombol Lanjut")]
    public string tombolLanjutTeks = "\u25B6  Lanjut";
    public Color  warnaLanjut      = new Color(0.20f, 0.62f, 0.86f, 1f);

    [Header("Voice Meter \u2014 Mekanik Teriak (Voice-Driven Action)")]
    [Tooltip("Aktifkan mini-game Voice Meter. Saat pilihan AMAN dipilih, pemain harus\n" +
             "MENAHAN tombol Teriak (atau berteriak ke mikrofon) sampai meter MERAH.\n\n" +
             "DEFAULT ON: bagian interaktif ini sengaja dipertahankan supaya Day 2 tetap\n" +
             "seru. Set 'gunakanMikrofon' = true untuk teriak via mic asli (Voice-Driven).")]
    public bool aktifkanVoiceMeter = true;
    [Tooltip("Kategori pilihan yang memicu Voice Meter (default: AMAN).")]
    public string kategoriPemicu = "AMAN";
    [Tooltip("Pakai MIKROFON asli sebagai input suara. Kalau OFF / mic tidak ada\n" +
             "\u2192 otomatis fallback ke TAHAN tombol.")]
    public bool gunakanMikrofon = false;
    [Tooltip("Sensitivitas mikrofon (kalikan loudness mentah). Naikkan kalau suara terlalu pelan.")]
    [Range(1f, 40f)] public float sensitivitasMic = 12f;

    [Header("Voice Meter \u2014 Aturan Level")]
    [Tooltip("Ambang batas zona KUNING (Suara Sedang). 0\u20131. Default 0.5 (\u224860dB).")]
    [Range(0.2f, 0.8f)] public float ambangKuning = 0.5f;
    [Tooltip("Ambang batas zona MERAH (Suara KERAS). 0\u20131. Default 0.8 (\u224880dB).")]
    [Range(0.5f, 0.95f)] public float ambangMerah = 0.8f;
    [Tooltip("Lama (detik) meter harus BERTAHAN di zona MERAH supaya teriakan dianggap berhasil.")]
    [Range(0.2f, 2f)] public float tahanDetikMerah = 0.6f;
    [Tooltip("Kecepatan meter NAIK saat tombol ditahan (per detik).")]
    [Range(0.3f, 2f)] public float isiRate = 0.7f;
    [Tooltip("Kecepatan meter TURUN saat tombol dilepas (per detik).")]
    [Range(0.2f, 2f)] public float surutRate = 0.5f;
    [Tooltip("Bonus skor saat teriakan berhasil (zona MERAH). 0 = tanpa bonus.")]
    public int bonusTeriakKeras = 100;

    [Header("Voice Meter \u2014 Tampilan")]
    public string judulTeriak  = "\uD83D\uDDE3  TERIAK SEKUATNYA!";
    public string instruksiTeriak = "TAHAN tombol & teriak \u201CJANGAN PEGANG SAYA!\u201D sampai meter MERAH!";
    public string teksTombolTeriak = "TAHAN UNTUK TERIAK";
    public string labelNormal = "Suara Normal";
    public string labelSedang = "Suara Sedang";
    public string labelKeras  = "Suara KERAS";
    public string labelBerhasil = "\u2713  TERIAKAN BERHASIL!";
    public Color  hijauWarna  = new Color(0.16f, 0.74f, 0.40f, 1f);
    public Color  kuningWarna = new Color(0.96f, 0.78f, 0.10f, 1f);
    public Color  merahWarna  = new Color(0.91f, 0.26f, 0.22f, 1f);
    [Tooltip("Sprite komik opsional saat adegan teriak (kosong = pakai gambar beat sebelumnya).")]
    public Sprite gambarTeriak;

    [Header("Font & Sorting")]
    public TMP_FontAsset fontAsset;
    public int sortingOrder = 925;

    // ── runtime ───────────────────────────────────────────────────────────
    private Action     _onSelesai;
    private GameObject _canvasGO;
    private Image      _bgImg;
    private Image      _gambarImg;
    private TextMeshProUGUI _placeholderTxt;
    private TextMeshProUGUI _namaTxt;
    private TextMeshProUGUI _narasiTxt;
    private TextMeshProUGUI _hintTxt;
    private GameObject _pilihanRow;
    private GameObject _lanjutBtn;
    private Sprite     _rounded;
    private bool       _ketikSelesai;
    private bool       _lanjutDitekan;
    private Coroutine  _ketikCo;

    // Mode VN
    private GameObject _dialogBoxGO;
    private Image      _portraitImg;

    // Voice Meter
    private bool       _holdTeriak;
    private AudioClip  _micClip;
    private string     _micDevice;

    // ══════════════════════════════════════════════════════════════════════
    public void Mulai(Action onSelesai)
    {
        _onSelesai = onSelesai;
        HUDManager.Instance?.SetNavbarVisible(false); // sembunyikan navbar selama adegan TERIAK
        BuildUI();
        StartCoroutine(JalankanAdegan());
    }

    void OnDestroy()
    {
        // Pastikan mikrofon berhenti kalau adegan dihancurkan saat mini-game berjalan.
        if (_micClip != null)
        {
            try { if (!string.IsNullOrEmpty(_micDevice)) Microphone.End(_micDevice); } catch { }
            _micClip = null;
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // BUILD UI
    // ══════════════════════════════════════════════════════════════════════
    void BuildUI()
    {
        _canvasGO = new GameObject("AngkotSentuh_Canvas");
        var canvas = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = _canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        _canvasGO.AddComponent<GraphicRaycaster>();
        PastikanEventSystem();

        // Background fullscreen
        var bgGO = new GameObject("BG");
        bgGO.transform.SetParent(_canvasGO.transform, false);
        var bgImg = bgGO.AddComponent<Image>();
        if (bgSprite != null) { bgImg.sprite = bgSprite; bgImg.color = Color.white; bgImg.preserveAspect = bgPreserveAspect; }
        else                  { bgImg.color = bgWarna; }
        Stretch(bgImg.rectTransform);
        _bgImg = bgImg;

        // Mode VN: pakai box dialog + portrait (gaya Halte), lewati panel komik.
        if (modeVisualNovel) { BuildVNBox(); return; }

        // ── PANEL GAMBAR (atas) ──────────────────────────────────────────
        var frameGO = new GameObject("PanelGambar");
        frameGO.transform.SetParent(_canvasGO.transform, false);
        var frameImg = frameGO.AddComponent<Image>();
        frameImg.sprite = GetRounded();
        frameImg.type   = Image.Type.Sliced;
        frameImg.color  = new Color(0f, 0f, 0f, 0.35f);
        var frameOutl = frameGO.AddComponent<Outline>();
        frameOutl.effectColor    = borderGambar;
        frameOutl.effectDistance = new Vector2(3f, -3f);
        var frt = frameGO.GetComponent<RectTransform>();
        float botGambar = 1f - tinggiAreaGambar;     // anchor Y bawah panel gambar
        frt.anchorMin = new Vector2(0.04f, botGambar);
        frt.anchorMax = new Vector2(0.96f, 0.97f);
        frt.offsetMin = Vector2.zero; frt.offsetMax = Vector2.zero;

        // Gambar komik di dalam frame
        var gGO = new GameObject("Gambar");
        gGO.transform.SetParent(frameGO.transform, false);
        _gambarImg = gGO.AddComponent<Image>();
        _gambarImg.type = tipeGambar;
        _gambarImg.preserveAspect = gambarPreserveAspect;
        _gambarImg.color = Color.white;
        _gambarImg.enabled = false;
        var grt = _gambarImg.rectTransform;
        grt.anchorMin = new Vector2(0f, 0f); grt.anchorMax = new Vector2(1f, 1f);
        grt.offsetMin = new Vector2(8f, 8f); grt.offsetMax = new Vector2(-8f, -8f);

        // Placeholder teks (saat gambar kosong)
        _placeholderTxt = BuatTeks(frameGO.transform, "Placeholder",
            "[ GAMBAR KOMIK ]\n(atur sprite di Inspector)", 26,
            new Color(1f, 1f, 1f, 0.45f), FontStyles.Italic);
        _placeholderTxt.alignment = TextAlignmentOptions.Center;
        Stretch(_placeholderTxt.rectTransform);

        // ── KOTAK NARASI (bawah) ─────────────────────────────────────────
        var boxGO = new GameObject("KotakNarasi");
        boxGO.transform.SetParent(_canvasGO.transform, false);
        var boxImg = boxGO.AddComponent<Image>();
        boxImg.sprite = GetRounded();
        boxImg.type   = Image.Type.Sliced;
        boxImg.color  = panelNarasiWarna;
        var boxOutl = boxGO.AddComponent<Outline>();
        boxOutl.effectColor    = borderGambar;
        boxOutl.effectDistance = new Vector2(2f, -2f);
        var brt = boxGO.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.04f, 0.03f);
        brt.anchorMax = new Vector2(0.96f, botGambar - 0.015f);
        brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;

        // Nama pembicara
        _namaTxt = BuatTeks(boxGO.transform, "Nama", "", namaUkuran, namaWarna, FontStyles.Bold);
        var nrt = _namaTxt.rectTransform;
        nrt.anchorMin = new Vector2(0f, 1f); nrt.anchorMax = new Vector2(1f, 1f);
        nrt.pivot = new Vector2(0f, 1f);
        nrt.offsetMin = new Vector2(34f, -54f); nrt.offsetMax = new Vector2(-34f, -14f);

        // Teks narasi
        _narasiTxt = BuatTeks(boxGO.transform, "Narasi", "", teksUkuran, teksWarna, FontStyles.Normal);
        var trt = _narasiTxt.rectTransform;
        trt.anchorMin = new Vector2(0f, 0f); trt.anchorMax = new Vector2(1f, 1f);
        trt.offsetMin = new Vector2(34f, 50f); trt.offsetMax = new Vector2(-34f, -58f);

        // Hint
        _hintTxt = BuatTeks(boxGO.transform, "Hint", hintTeks, 16, new Color(1f,1f,1f,0.55f), FontStyles.Italic);
        _hintTxt.alignment = TextAlignmentOptions.MidlineRight;
        var hrt = _hintTxt.rectTransform;
        hrt.anchorMin = new Vector2(0f, 0f); hrt.anchorMax = new Vector2(1f, 0f);
        hrt.pivot = new Vector2(1f, 0f);
        hrt.offsetMin = new Vector2(34f, 10f); hrt.offsetMax = new Vector2(-34f, 40f);

        // Tombol "lanjut transparan" full layar untuk klik maju (di belakang tombol pilihan)
        var clickGO = new GameObject("ClickArea");
        clickGO.transform.SetParent(boxGO.transform, false);
        var clickImg = clickGO.AddComponent<Image>();
        clickImg.color = new Color(0,0,0,0);
        Stretch(clickImg.rectTransform);
        var clickBtn = clickGO.AddComponent<Button>();
        clickBtn.transition = Selectable.Transition.None;
        clickBtn.onClick.AddListener(() => _lanjutDitekan = true);
    }

    // ══════════════════════════════════════════════════════════════════════
    // BUILD BOX DIALOG VN (portrait kiri + nama + teks) — gaya HalteDialog
    // ══════════════════════════════════════════════════════════════════════
    void BuildVNBox()
    {
        _dialogBoxGO = new GameObject("DialogBoxVN");
        _dialogBoxGO.transform.SetParent(_canvasGO.transform, false);
        var img = _dialogBoxGO.AddComponent<Image>();
        if (vnPanelSprite != null)
        {
            img.sprite = vnPanelSprite;
            img.type   = Image.Type.Sliced;
            img.color  = vnPanelTint;
        }
        else
        {
            img.sprite = GetRounded();
            img.type   = Image.Type.Sliced;
            img.color  = new Color(0.05f, 0.08f, 0.12f, 0.94f);
            var outl = _dialogBoxGO.AddComponent<Outline>();
            outl.effectColor    = new Color(1f, 0.85f, 0.25f, 1f);
            outl.effectDistance = new Vector2(2f, -2f);
        }
        var rt = _dialogBoxGO.GetComponent<RectTransform>();
        rt.anchorMin = vnBoxAnchorMin; rt.anchorMax = vnBoxAnchorMax;
        rt.offsetMin = Vector2.zero;   rt.offsetMax = Vector2.zero;

        // Portrait (kiri di dalam box)
        var portraitGO = new GameObject("Portrait");
        portraitGO.transform.SetParent(_dialogBoxGO.transform, false);
        var prt = portraitGO.AddComponent<RectTransform>();
        prt.anchorMin = vnPortraitAnchorMin; prt.anchorMax = vnPortraitAnchorMax;
        prt.offsetMin = prt.offsetMax = Vector2.zero;
        _portraitImg = portraitGO.AddComponent<Image>();
        _portraitImg.preserveAspect = vnPortraitPreserveAspect;
        _portraitImg.color          = Color.white;
        _portraitImg.raycastTarget  = false;
        _portraitImg.enabled        = false; // diisi via SetPortrait()

        // Banner nama pembicara (bawah portrait)
        _namaTxt = BuatTeks(_dialogBoxGO.transform, "Nama", "", vnNamaUkuran, vnNamaWarna, FontStyles.Bold);
        _namaTxt.alignment = TextAlignmentOptions.MidlineLeft;
        var nrt = _namaTxt.rectTransform;
        nrt.anchorMin = vnNamaAnchorMin; nrt.anchorMax = vnNamaAnchorMax;
        nrt.offsetMin = new Vector2(12f, 2f); nrt.offsetMax = new Vector2(-4f, -2f);

        // Teks isi dialog (kanan)
        _narasiTxt = BuatTeks(_dialogBoxGO.transform, "Narasi", "", vnTeksUkuran, vnTeksWarna, FontStyles.Normal);
        _narasiTxt.alignment = TextAlignmentOptions.TopLeft;
        var trt = _narasiTxt.rectTransform;
        trt.anchorMin = vnTeksAnchorMin; trt.anchorMax = vnTeksAnchorMax;
        trt.offsetMin = new Vector2(4f, 4f); trt.offsetMax = new Vector2(-4f, -4f);

        // Hint pojok kanan-bawah box (mirror posisi hint Day 1 intro)
        _hintTxt = BuatTeks(_dialogBoxGO.transform, "Hint", hintTeks, 16, new Color(1f,1f,1f,0.55f), FontStyles.Italic);
        _hintTxt.alignment = TextAlignmentOptions.MidlineRight;
        var hrt = _hintTxt.rectTransform;
        hrt.anchorMin = new Vector2(0.65f, 0.182f); hrt.anchorMax = new Vector2(0.946f, 0.302f);
        hrt.offsetMin = hrt.offsetMax = Vector2.zero;

        // Area klik penuh box untuk maju
        var clickGO = new GameObject("ClickArea");
        clickGO.transform.SetParent(_dialogBoxGO.transform, false);
        var clickImg = clickGO.AddComponent<Image>();
        clickImg.color = new Color(0,0,0,0);
        Stretch(clickImg.rectTransform);
        var clickBtn = clickGO.AddComponent<Button>();
        clickBtn.transition = Selectable.Transition.None;
        clickBtn.onClick.AddListener(() => _lanjutDitekan = true);
    }

    // Pilih portrait sesuai pembicara (mode VN). Kosong = sembunyikan.
    void SetPortrait(string speaker)
    {
        if (_portraitImg == null) return;
        Sprite s = speaker switch
        {
            "Rara"       => portraitRara,
            "Pria Asing" => portraitPriaAsing != null ? portraitPriaAsing : portraitNarasi,
            "Pak Supir"  => portraitPakSupir  != null ? portraitPakSupir  : portraitNarasi,
            _             => portraitNarasi
        };
        if (s != null) { _portraitImg.sprite = s; _portraitImg.enabled = true; }
        else           { _portraitImg.enabled = false; }
    }

    // ══════════════════════════════════════════════════════════════════════
    // ALUR ADEGAN
    // ══════════════════════════════════════════════════════════════════════
    // Pilih beat intro sesuai kursi yang Rara duduki di fase Angkot.
    // Sumber kategori: jembatan AngkotSeatPicker.KategoriKursiDipilih (utama) ->
    // GameState.seatCategory (fallback). AMAN=dekat supir, RAGU=tengah ibu-ibu,
    // BAHAYA=pojok belakang. Kalau kategori tak dikenal/varian kosong -> pakai
    // varian pertama yang terisi (Aman > Ragu > Bahaya).
    List<PanelKomik> PilihIntroSesuaiKursi()
    {
        string kursi = !string.IsNullOrEmpty(AngkotSeatPicker.KategoriKursiDipilih)
            ? AngkotSeatPicker.KategoriKursiDipilih
            : (GameState.Instance != null ? GameState.Instance.seatCategory : null);

        List<PanelKomik> varian = kursi switch
        {
            "AMAN"   => panelIntroAman,
            "RAGU"   => panelIntroRagu,
            "BAHAYA" => panelIntroBahaya,
            _         => null
        };
        if (varian != null && varian.Count > 0)
        {
            Debug.Log($"[AngkotSentuhScene] Intro varian '{kursi}' dipilih sesuai kursi.");
            return varian;
        }

        // Fallback aman kalau kursi belum dipilih / tak dikenal.
        Debug.LogWarning($"[AngkotSentuhScene] Kategori kursi '{kursi}' tak dikenal/varian kosong \u2014 pakai varian pertama yang terisi.");
        if (panelIntroAman   != null && panelIntroAman.Count   > 0) return panelIntroAman;
        if (panelIntroRagu   != null && panelIntroRagu.Count   > 0) return panelIntroRagu;
        if (panelIntroBahaya != null && panelIntroBahaya.Count > 0) return panelIntroBahaya;
        return new List<PanelKomik>();
    }

    IEnumerator JalankanAdegan()
    {
        // 1. Beat intro - varian sesuai pilihan kursi (seatCategory) di fase Angkot.
        var intro = PilihIntroSesuaiKursi();
        foreach (var p in intro)
            yield return TampilkanPanel(p);

        // 2. Pilihan interaktif
        PilihanSentuh dipilih = null;
        yield return TampilkanPilihan(v => dipilih = v);

        // 2b. Voice-Driven Action — kalau pilihan AMAN (teriak), mainkan Voice Meter.
        //     Pemain harus menahan tombol / berteriak sampai meter MERAH (>80 dB).
        if (aktifkanVoiceMeter && dipilih != null && dipilih.kategori == kategoriPemicu)
            yield return JalankanVoiceMeter();

        // 3. Terapkan konsekuensi
        var gs = GameState.Instance;
        if (gs != null)
        {
            gs.AddChoice(2, "Disentuh di angkot: " + dipilih.label, dipilih.kategori);
            if (dipilih.kurangiNyawa)
            {
                gs.LoseLife();
                HUDManager.Instance?.UpdateHearts(gs.lives, gs.maxLives);
            }
        }
        AudioManager.Instance?.PlayKategori(dipilih.kategori);

        // 4. Reaksi (gambar + teks)
        yield return TampilkanPanel(new PanelKomik
        {
            speaker = "Narasi",
            narasi  = dipilih.reaksi,
            gambar  = dipilih.gambarReaksi,
            latarSprite = dipilih.latarReaksi
        });

        // 5. Kalau AMAN → mainkan beat lapor supir
        if (dipilih.lanjutLapor)
            foreach (var p in panelLapor)
                yield return TampilkanPanel(p);

        // 5b. Beat penutup — angkot tiba di sekolah dengan selamat (selalu dimainkan).
        if (panelSampaiSekolah != null)
            foreach (var p in panelSampaiSekolah)
                yield return TampilkanPanel(p);

        // 6. Tombol lanjut
        yield return TampilkanTombolLanjut();

        HUDManager.Instance?.SetNavbarVisible(true); // tampilkan kembali navbar saat keluar
        if (_canvasGO != null) Destroy(_canvasGO);
        _onSelesai?.Invoke();
    }

    IEnumerator TampilkanPanel(PanelKomik p)
    {
        SetLatar(p.latarSprite);
        SetGambar(p.gambar, p.tintGambar);
        SetPortrait(p.speaker);
        _namaTxt.text = string.IsNullOrEmpty(p.speaker) ? "" : p.speaker.ToUpper();
        if (_hintTxt != null) _hintTxt.gameObject.SetActive(true);

        // ketik teks
        _lanjutDitekan = false;
        _ketikSelesai  = false;
        if (_ketikCo != null) StopCoroutine(_ketikCo);
        _ketikCo = StartCoroutine(Ketik(p.narasi));

        // tunggu sampai selesai ketik + klik untuk lanjut
        while (!(_lanjutDitekan && _ketikSelesai))
        {
            bool klik = _lanjutDitekan || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
            if (klik)
            {
                if (!_ketikSelesai && bolehSkipKetik)
                {
                    if (_ketikCo != null) StopCoroutine(_ketikCo);
                    _narasiTxt.text = p.narasi;
                    _ketikSelesai = true;
                    _lanjutDitekan = false;
                }
                else if (_ketikSelesai)
                {
                    break;
                }
            }
            yield return null;
        }
        yield return null; // debounce 1 frame
    }

    IEnumerator Ketik(string teks)
    {
        _narasiTxt.text = "";
        if (kecepatanKetik <= 0f) { _narasiTxt.text = teks; _ketikSelesai = true; yield break; }
        for (int i = 0; i < teks.Length; i++)
        {
            _narasiTxt.text += teks[i];
            yield return new WaitForSeconds(kecepatanKetik);
        }
        _ketikSelesai = true;
    }

    IEnumerator TampilkanPilihan(Action<PilihanSentuh> onPilih)
    {
        // judul di kotak narasi
        SetGambar(null, Color.white);
        SetPortrait("Rara");
        _namaTxt.text = "PILIH";
        if (_ketikCo != null) StopCoroutine(_ketikCo);
        _narasiTxt.text = judulPilihan;
        if (_hintTxt != null) _hintTxt.gameObject.SetActive(false);

        // baris tombol pilihan
        _pilihanRow = new GameObject("PilihanRow");
        _pilihanRow.transform.SetParent(_canvasGO.transform, false);
        var rt = _pilihanRow.AddComponent<RectTransform>();
        if (modeVisualNovel)
        {
            // Mode VN: kotak dialog ada di strip bawah, jadi tombol naik ke atasnya.
            rt.anchorMin = new Vector2(0.12f, 0.34f);
            rt.anchorMax = new Vector2(0.88f, 0.64f);
        }
        else
        {
            rt.anchorMin = new Vector2(0.06f, 0.05f);
            rt.anchorMax = new Vector2(0.94f, 0.27f);
        }
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var vlg = _pilihanRow.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 12f;
        vlg.childControlHeight = true; vlg.childControlWidth = true;
        vlg.childForceExpandHeight = true; vlg.childForceExpandWidth = true;

        PilihanSentuh hasil = null;
        foreach (var pil in pilihanList)
        {
            var local = pil;
            BuatTombol(_pilihanRow.transform, pil.label, WarnaKategori(pil.kategori), () =>
            {
                if (hasil != null) return;
                hasil = local;
                AudioManager.Instance?.Click();
            });
        }

        while (hasil == null) yield return null;

        if (_pilihanRow != null) Destroy(_pilihanRow);
        onPilih?.Invoke(hasil);
    }

    IEnumerator TampilkanTombolLanjut()
    {
        if (_hintTxt != null) _hintTxt.gameObject.SetActive(false);
        bool lanjut = false;
        _lanjutBtn = BuatTombol(_canvasGO.transform, tombolLanjutTeks, warnaLanjut, () => lanjut = true);
        var rt = _lanjutBtn.GetComponent<RectTransform>();
        // Mode VN: tombol di atas kotak dialog bawah; mode komik: di strip bawah.
        float yLanjut = modeVisualNovel ? 0.335f : 0.05f;
        rt.anchorMin = new Vector2(0.5f, yLanjut); rt.anchorMax = new Vector2(0.5f, yLanjut);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(360f, 68f);
        rt.anchoredPosition = new Vector2(0f, 0f);
        while (!lanjut) yield return null;
    }

    // ══════════════════════════════════════════════════════════════════════
    // VOICE METER — mini-game teriak (Voice-Driven Action)
    //   Hijau  = Suara Normal (50-60 dB)
    //   Kuning = Suara Sedang (60-80 dB)
    //   Merah  = Suara KERAS  (>80 dB)  → target supaya berhasil
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator JalankanVoiceMeter()
    {
        // sembunyikan elemen narasi utama selama mini-game
        if (gambarTeriak != null) SetGambar(gambarTeriak, Color.white);
        if (_hintTxt != null) _hintTxt.gameObject.SetActive(false);
        _holdTeriak = false;

        // Sembunyikan kotak dialog VN supaya layar teriak bersih & fokus
        // (prompt "Bahumu disentuh..." tidak ikut tampil di balik overlay).
        if (_dialogBoxGO != null) _dialogBoxGO.SetActive(false);

        // backdrop gelap fokus (lebih pekat supaya kontras tinggi)
        var ov = new GameObject("VoiceOverlay");
        ov.transform.SetParent(_canvasGO.transform, false);
        var ovImg = ov.AddComponent<Image>();
        ovImg.color = new Color(0.02f, 0.01f, 0.04f, 0.9f);
        Stretch(ovImg.rectTransform);

        // Kartu panel tengah sebagai bingkai fokus mini-game (dekoratif).
        var kartu = new GameObject("VoiceKartu");
        kartu.transform.SetParent(ov.transform, false);
        var kartuImg = kartu.AddComponent<Image>();
        kartuImg.sprite = GetRounded(); kartuImg.type = Image.Type.Sliced;
        kartuImg.color  = new Color(0.10f, 0.07f, 0.05f, 0.97f);
        kartuImg.raycastTarget = false;
        var kartuRT = kartu.GetComponent<RectTransform>();
        kartuRT.anchorMin = new Vector2(0.10f, 0.085f);
        kartuRT.anchorMax = new Vector2(0.90f, 0.965f);
        kartuRT.offsetMin = Vector2.zero; kartuRT.offsetMax = Vector2.zero;
        var kartuOutl = kartu.AddComponent<Outline>();
        kartuOutl.effectColor = new Color(0.95f, 0.72f, 0.18f, 0.9f);
        kartuOutl.effectDistance = new Vector2(3f, -3f);

        // judul
        var judul = BuatTeks(ov.transform, "Judul", judulTeriak, 40, merahWarna, FontStyles.Bold);
        judul.alignment = TextAlignmentOptions.Center;
        var jrt = judul.rectTransform;
        jrt.anchorMin = new Vector2(0.08f, 0.80f); jrt.anchorMax = new Vector2(0.92f, 0.92f);
        jrt.offsetMin = Vector2.zero; jrt.offsetMax = Vector2.zero;

        // instruksi
        var ins = BuatTeks(ov.transform, "Instruksi", instruksiTeriak, 24, Color.white, FontStyles.Normal);
        ins.alignment = TextAlignmentOptions.Center;
        var irt = ins.rectTransform;
        irt.anchorMin = new Vector2(0.12f, 0.71f); irt.anchorMax = new Vector2(0.88f, 0.80f);
        irt.offsetMin = Vector2.zero; irt.offsetMax = Vector2.zero;

        // label level (di atas bar) — berubah Normal/Sedang/KERAS
        var lvl = BuatTeks(ov.transform, "Level", labelNormal, 32, hijauWarna, FontStyles.Bold);
        lvl.alignment = TextAlignmentOptions.Center;
        var lrt = lvl.rectTransform;
        lrt.anchorMin = new Vector2(0.1f, 0.62f); lrt.anchorMax = new Vector2(0.9f, 0.70f);
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;

        // bar background
        var bar = new GameObject("Bar");
        bar.transform.SetParent(ov.transform, false);
        var barImg = bar.AddComponent<Image>();
        barImg.sprite = GetRounded(); barImg.type = Image.Type.Sliced;
        barImg.color = new Color(0.08f, 0.08f, 0.10f, 1f);
        var barRT = bar.GetComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0.15f, 0.50f); barRT.anchorMax = new Vector2(0.85f, 0.585f);
        barRT.offsetMin = Vector2.zero; barRT.offsetMax = Vector2.zero;
        var barOutl = bar.AddComponent<Outline>();
        barOutl.effectColor = new Color(1f, 1f, 1f, 0.3f); barOutl.effectDistance = new Vector2(2f, -2f);

        // zona warna (3 segmen sesuai aturan gambar)
        BuatZonaWarna(bar.transform, 0f, ambangKuning, hijauWarna);
        BuatZonaWarna(bar.transform, ambangKuning, ambangMerah, kuningWarna);
        BuatZonaWarna(bar.transform, ambangMerah, 1f, merahWarna);

        // garis ambang MERAH (target)
        var garis = new GameObject("GarisTarget");
        garis.transform.SetParent(bar.transform, false);
        var garisImg = garis.AddComponent<Image>();
        garisImg.color = new Color(1f, 1f, 1f, 0.9f);
        var garisRT = garis.GetComponent<RectTransform>();
        garisRT.anchorMin = new Vector2(ambangMerah, -0.25f);
        garisRT.anchorMax = new Vector2(ambangMerah, 1.25f);
        garisRT.pivot = new Vector2(0.5f, 0.5f);
        garisRT.sizeDelta = new Vector2(4f, 0f);

        // marker level (garis tebal vertikal yang bergerak)
        var marker = new GameObject("Marker");
        marker.transform.SetParent(bar.transform, false);
        var markImg = marker.AddComponent<Image>();
        markImg.color = Color.white;
        var markRT = marker.GetComponent<RectTransform>();
        markRT.anchorMin = new Vector2(0f, -0.18f); markRT.anchorMax = new Vector2(0f, 1.18f);
        markRT.pivot = new Vector2(0.5f, 0.5f);
        markRT.sizeDelta = new Vector2(10f, 0f);

        // legenda 3 baris (mirip gambar referensi)
        BuatLegenda(ov.transform);

        // ── Indikator TAHAN (#7) — progress berapa lama suara sudah di zona MERAH.
        // Memberi umpan balik jelas seberapa dekat pemain ke "berhasil".
        var holdLabel = BuatTeks(ov.transform, "HoldLabel", "TAHAN SUARA DI ZONA MERAH!", 18,
            new Color(1f, 0.85f, 0.3f, 1f), FontStyles.Bold);
        holdLabel.alignment = TextAlignmentOptions.Center;
        var hlrt = holdLabel.rectTransform;
        hlrt.anchorMin = new Vector2(0.15f, 0.475f); hlrt.anchorMax = new Vector2(0.85f, 0.498f);
        hlrt.offsetMin = Vector2.zero; hlrt.offsetMax = Vector2.zero;

        var holdBg = new GameObject("HoldBg");
        holdBg.transform.SetParent(ov.transform, false);
        var holdBgImg = holdBg.AddComponent<Image>();
        holdBgImg.sprite = GetRounded(); holdBgImg.type = Image.Type.Sliced;
        holdBgImg.color = new Color(0.08f, 0.08f, 0.10f, 1f);
        var holdBgRT = holdBg.GetComponent<RectTransform>();
        holdBgRT.anchorMin = new Vector2(0.22f, 0.45f); holdBgRT.anchorMax = new Vector2(0.78f, 0.475f);
        holdBgRT.offsetMin = Vector2.zero; holdBgRT.offsetMax = Vector2.zero;
        var holdBgOutl = holdBg.AddComponent<Outline>();
        holdBgOutl.effectColor = new Color(1f, 1f, 1f, 0.25f); holdBgOutl.effectDistance = new Vector2(2f, -2f);

        var holdFill = new GameObject("HoldFill");
        holdFill.transform.SetParent(holdBg.transform, false);
        var holdFillImg = holdFill.AddComponent<Image>();
        holdFillImg.sprite = GetRounded(); holdFillImg.type = Image.Type.Sliced;
        holdFillImg.color = merahWarna;
        holdFillImg.raycastTarget = false;
        var holdFillRT = holdFill.GetComponent<RectTransform>();
        holdFillRT.anchorMin = new Vector2(0f, 0f); holdFillRT.anchorMax = new Vector2(0f, 1f);
        holdFillRT.offsetMin = Vector2.zero; holdFillRT.offsetMax = Vector2.zero;

        var holdPct = BuatTeks(holdBg.transform, "HoldPct", "0%", 16, Color.white, FontStyles.Bold);
        holdPct.alignment = TextAlignmentOptions.Center;
        var hprt = holdPct.rectTransform;
        hprt.anchorMin = Vector2.zero; hprt.anchorMax = Vector2.one;
        hprt.offsetMin = Vector2.zero; hprt.offsetMax = Vector2.zero;

        // tombol TAHAN untuk teriak (pendukung / fallback input mic)
        var btnGO = BuatTombol(ov.transform, teksTombolTeriak, merahWarna, null);
        var btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.27f, 0.115f); btnRT.anchorMax = new Vector2(0.73f, 0.225f);
        btnRT.offsetMin = Vector2.zero; btnRT.offsetMax = Vector2.zero;
        var btnGlow = btnGO.GetComponent<Outline>();
        if (btnGlow != null) { btnGlow.effectColor = new Color(1f, 0.85f, 0.3f, 0.7f); btnGlow.effectDistance = new Vector2(3f, -3f); }
        var et = btnGO.AddComponent<EventTrigger>();
        TambahTrigger(et, EventTriggerType.PointerDown, () => _holdTeriak = true);
        TambahTrigger(et, EventTriggerType.PointerUp,   () => _holdTeriak = false);
        TambahTrigger(et, EventTriggerType.PointerExit, () => _holdTeriak = false);

        // mulai mikrofon kalau diaktifkan & tersedia
        bool micAktif = false;
        if (gunakanMikrofon && Microphone.devices != null && Microphone.devices.Length > 0)
        {
            try
            {
                _micDevice = Microphone.devices[0];
                _micClip = Microphone.Start(_micDevice, true, 1, 44100);
                micAktif = true;
            }
            catch { micAktif = false; }
        }
        // Instruksi adaptif: Voice-Driven via mic, atau tahan tombol kalau mic tak ada.
        ins.text = micAktif
            ? "TERIAK \u201CJANGAN PEGANG SAYA!\u201D ke mikrofon sampai meter MERAH! (boleh tahan tombol)"
            : instruksiTeriak;

        // ── loop mini-game ───────────────────────────────────────────────
        float level = 0f, waktuMerah = 0f;
        while (waktuMerah < tahanDetikMerah)
        {
            float dt = Time.deltaTime;
            if (micAktif)
            {
                float l = BacaLoudnessMic();
                level = Mathf.Lerp(level, l, 1f - Mathf.Exp(-9f * dt));
            }
            else
            {
                bool hold = _holdTeriak || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
                level += (hold ? isiRate : -surutRate) * dt;
            }
            level = Mathf.Clamp01(level);

            // posisi marker mengikuti level
            markRT.anchorMin = new Vector2(level, -0.18f);
            markRT.anchorMax = new Vector2(level,  1.18f);

            // label & warna sesuai zona
            if (level >= ambangMerah)
            {
                lvl.text = labelKeras;  lvl.color = merahWarna;
                waktuMerah += dt;
                if (markImg != null) markImg.color = merahWarna;
                float pulsa = 1f + 0.25f * Mathf.Sin(Time.time * 18f);
                marker.transform.localScale = new Vector3(pulsa, 1f, 1f);
            }
            else if (level >= ambangKuning)
            {
                lvl.text = labelSedang; lvl.color = kuningWarna;
                waktuMerah = 0f;
                if (markImg != null) markImg.color = Color.white;
                marker.transform.localScale = Vector3.one;
            }
            else
            {
                lvl.text = labelNormal; lvl.color = hijauWarna;
                waktuMerah = 0f;
                if (markImg != null) markImg.color = Color.white;
                marker.transform.localScale = Vector3.one;
            }

            // Perbarui indikator TAHAN (#7): isi bar + persentase + denyut saat di merah.
            float frac = Mathf.Clamp01(waktuMerah / Mathf.Max(0.01f, tahanDetikMerah));
            holdFillRT.anchorMax = new Vector2(frac, 1f);
            holdPct.text = Mathf.RoundToInt(frac * 100f) + "%";
            if (frac > 0f)
            {
                float glow = 0.7f + 0.3f * Mathf.Sin(Time.time * 14f);
                holdFillImg.color = new Color(merahWarna.r, merahWarna.g, merahWarna.b, glow);
            }
            else
            {
                holdFillImg.color = new Color(merahWarna.r, merahWarna.g, merahWarna.b, 1f);
            }
            yield return null;
        }

        // ── berhasil ─────────────────────────────────────────────────────
        if (micAktif) { try { Microphone.End(_micDevice); } catch { } _micClip = null; }

        AudioManager.Instance?.PlayKategori("AMAN");
        if (bonusTeriakKeras != 0 && GameState.Instance != null)
        {
            GameState.Instance.AddScore(bonusTeriakKeras);
            HUDManager.Instance?.UpdateScore(GameState.Instance.score);
        }

        lvl.text = labelBerhasil; lvl.color = merahWarna;
        if (markImg != null) markImg.color = merahWarna;
        yield return new WaitForSeconds(0.7f);

        _holdTeriak = false;
        if (ov != null) Destroy(ov);
        // Tampilkan kembali kotak dialog VN untuk beat reaksi berikutnya.
        if (_dialogBoxGO != null) _dialogBoxGO.SetActive(true);
    }

    // Buat 1 segmen warna pada bar (fraksi x dari→sampai).
    void BuatZonaWarna(Transform bar, float dari, float sampai, Color warna)
    {
        var z = new GameObject("Zona");
        z.transform.SetParent(bar, false);
        var img = z.AddComponent<Image>();
        img.color = warna;
        img.raycastTarget = false;
        var rt = z.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(dari, 0.12f);
        rt.anchorMax = new Vector2(sampai, 0.88f);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    // Legenda 3 baris: kotak warna + label (mirip gambar referensi).
    void BuatLegenda(Transform parent)
    {
        var box = new GameObject("Legenda");
        box.transform.SetParent(parent, false);
        var brt = box.AddComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.30f, 0.27f); brt.anchorMax = new Vector2(0.70f, 0.46f);
        brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;
        var vlg = box.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 6f; vlg.childControlHeight = true; vlg.childControlWidth = true;
        vlg.childForceExpandHeight = true; vlg.childForceExpandWidth = true;

        BuatBarisLegenda(box.transform, hijauWarna,  labelNormal);
        BuatBarisLegenda(box.transform, kuningWarna, labelSedang);
        BuatBarisLegenda(box.transform, merahWarna,  labelKeras);
    }

    void BuatBarisLegenda(Transform parent, Color warna, string teks)
    {
        var row = new GameObject("Baris");
        row.transform.SetParent(parent, false);
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 12f; hlg.childControlHeight = true; hlg.childControlWidth = true;
        hlg.childForceExpandHeight = true; hlg.childForceExpandWidth = false;
        hlg.childAlignment = TextAnchor.MiddleLeft;

        var kotak = new GameObject("Kotak");
        kotak.transform.SetParent(row.transform, false);
        var kImg = kotak.AddComponent<Image>();
        kImg.sprite = GetRounded(); kImg.type = Image.Type.Sliced; kImg.color = warna;
        var kLe = kotak.AddComponent<LayoutElement>();
        kLe.preferredWidth = 44f; kLe.preferredHeight = 28f;
        kLe.flexibleWidth = 0f;

        var lab = BuatTeks(row.transform, "Teks", teks, 20, Color.white, FontStyles.Normal);
        lab.alignment = TextAlignmentOptions.MidlineLeft;
        var labLe = lab.gameObject.AddComponent<LayoutElement>();
        labLe.flexibleWidth = 1f;
    }

    // Hitung loudness mikrofon (RMS) → 0..1.
    float BacaLoudnessMic()
    {
        if (_micClip == null) return 0f;
        int pos = Microphone.GetPosition(_micDevice);
        const int window = 256;
        if (pos < window) return 0f;
        var samples = new float[window];
        _micClip.GetData(samples, pos - window);
        float sum = 0f;
        for (int i = 0; i < window; i++) sum += samples[i] * samples[i];
        float rms = Mathf.Sqrt(sum / window);
        return Mathf.Clamp01(rms * sensitivitasMic);
    }

    static void TambahTrigger(EventTrigger et, EventTriggerType tipe, Action aksi)
    {
        var entry = new EventTrigger.Entry { eventID = tipe };
        entry.callback.AddListener(_ => aksi());
        et.triggers.Add(entry);
    }

    // ══════════════════════════════════════════════════════════════════════
    // HELPER
    // ══════════════════════════════════════════════════════════════════════
    void SetGambar(Sprite s, Color tint)
    {
        if (_gambarImg == null) return;
        if (s != null)
        {
            _gambarImg.sprite = s;
            _gambarImg.color  = tint;
            _gambarImg.enabled = true;
            if (_placeholderTxt != null) _placeholderTxt.gameObject.SetActive(false);
        }
        else
        {
            _gambarImg.enabled = false;
            if (_placeholderTxt != null) _placeholderTxt.gameObject.SetActive(true);
        }
    }

    // Ganti latar belakang fullscreen ke sprite (kalau s != null). Kosong = biarkan
    // latar beat sebelumnya. Mirror perilaku HalteDialog.latarSprite.
    void SetLatar(Sprite s)
    {
        if (_bgImg == null || s == null) return;
        _bgImg.sprite = s;
        _bgImg.color  = Color.white;
        _bgImg.preserveAspect = bgPreserveAspect;
    }

    Color WarnaKategori(string k) => k switch
    {
        "AMAN"   => warnaAman,
        "RAGU"   => warnaRagu,
        "BAHAYA" => warnaBahaya,
        _        => warnaLanjut
    };

    GameObject BuatTombol(Transform parent, string label, Color warna, Action onClick)
    {
        var go = new GameObject("Tombol");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.sprite = GetRounded();
        img.type   = Image.Type.Sliced;
        img.color  = warna;
        var outl = go.AddComponent<Outline>();
        outl.effectColor    = new Color(1f, 1f, 1f, 0.35f);
        outl.effectDistance = new Vector2(2f, -2f);
        var le = go.AddComponent<LayoutElement>();
        le.minHeight = 56f;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => onClick?.Invoke());

        var lab = BuatTeks(go.transform, "Label", label, 22, Color.white, FontStyles.Bold);
        lab.alignment = TextAlignmentOptions.Center;
        lab.raycastTarget = false;
        Stretch(lab.rectTransform, 14f, 6f);
        return go;
    }

    TextMeshProUGUI BuatTeks(Transform parent, string name, string content, int size, Color color, FontStyles style)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) tmp.font = fontAsset;
        else if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
        tmp.text = content; tmp.fontSize = size; tmp.color = color; tmp.fontStyle = style;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        return tmp;
    }

    static void Stretch(RectTransform rt, float padX = 0f, float padY = 0f)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(padX, padY); rt.offsetMax = new Vector2(-padX, -padY);
    }

    void PastikanEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }

    Sprite GetRounded()
    {
        if (_rounded != null) return _rounded;
        int size = 64, radius = 14;
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
        _rounded = Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius,radius,radius,radius));
        return _rounded;
    }
}
