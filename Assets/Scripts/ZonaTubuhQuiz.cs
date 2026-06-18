using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// ZonaTubuhQuiz — Quiz drag-and-drop untuk Fase 4 Day 2.
///
/// Pemain harus drag setiap chip (label perilaku) ke kolom yang tepat:
///   - "ZONA AMAN" : perilaku yang ok (jabat tangan, peluk keluarga, dst.)
///   - "ZONA BAHAYA": perilaku yang HARUS DITOLAK (sentuh tanpa izin, dll.)
///
/// Setelah waktu habis ATAU semua chip ditempatkan:
///   - Skor +SCORE_QUIZ per chip benar
///   - Bonus achievement "Penjaga Batas Tubuh" kalau 6/6 benar
///   - Tampilkan tombol lanjut
///
/// Custom semua chip & label lewat Inspector.
/// </summary>
public class ZonaTubuhQuiz : MonoBehaviour
{
    [System.Serializable]
    public class ChipData
    {
        public string teks;
        [Tooltip("Jawaban benar: AMAN atau BAHAYA.")]
        public string jawabanBenar = "AMAN"; // "AMAN" | "BAHAYA"
        [Tooltip("Alasan singkat (opsional). Kosong = dipilih otomatis sesuai bagian tubuh.")]
        [TextArea(1, 3)]
        public string alasan = "";
        [Tooltip("Sprite latar belakang fullscreen untuk dialog quiz VN pada chip ini.\n" +
                 "Kosong = pakai latar narasi default.")]
        public Sprite latarBelakangDialog;
    }

    [Header("Judul & Instruksi")]
    public string judulTeks = "\uD83D\uDEE1  Quiz: Seret label ke zona yang tepat!";
    public Color  judulWarna = new Color(1f, 0.85f, 0.3f, 1f);
    public int    judulUkuran = 30;
    [TextArea(2, 3)]
    public string instruksiTeks = "\u2190  Seret ke AMAN      |      Seret ke BAHAYA  \u2192";
    public Color  instruksiWarna = new Color(1f, 1f, 0.92f, 0.85f);
    public int    instruksiUkuran = 20;

    [Header("Mode Visual Novel")]
    [Tooltip("Saat ON, quiz disajikan sebagai TANYA-JAWAB bercabang dalam box dialog VN\n" +
             "(tiap perilaku ditanya 'BOLEH atau TIDAK BOLEH?' + 2 pilihan).\n\n" +
             "DEFAULT OFF: mekanik DRAG-DROP arcade yang interaktif & ber-timer sengaja\n" +
             "dipertahankan. Set true hanya kalau ingin versi novel murni tanpa drag-drop.")]
    public bool modeVisualNovel = false;
    [Tooltip("Template pertanyaan VN. {PERILAKU} diganti teks chip.")]
    public string vnPertanyaanTemplate = "Menurutmu, \u201C{PERILAKU}\u201D itu BOLEH atau TIDAK BOLEH dilakukan orang lain ke tubuhmu?";
    [Tooltip("Label tombol pilihan 'boleh' (jawaban AMAN).")]
    public string vnLabelBoleh = "\u2713  BOLEH";
    [Tooltip("Label tombol pilihan 'tidak boleh' (jawaban BAHAYA).")]
    public string vnLabelTidakBoleh = "\u2716  TIDAK BOLEH";
    [Tooltip("Sprite latar fullscreen untuk dialog ringkasan akhir mode VN. Kosong = pakai latar default.")]
    public Sprite vnRingkasanLatarBelakang;

    [Header("Timer")]
    public float waktuDetik = 15f;
    public Color warnaTimer = new Color(1f, 0.85f, 0.3f, 1f);
    public Color warnaTimerKritis = new Color(0.91f, 0.30f, 0.24f, 1f);
    public int   ukuranTimer = 28;

    [Header("Daftar Chip (CUSTOMIZABLE)")]
    public ChipData[] chips = new ChipData[]
    {
        new ChipData { teks = "Bahu",   jawabanBenar = "AMAN" },
        new ChipData { teks = "Tangan", jawabanBenar = "AMAN" },
        new ChipData { teks = "Pipi",   jawabanBenar = "AMAN" },
        new ChipData { teks = "Paha",   jawabanBenar = "BAHAYA" },
        new ChipData { teks = "Perut",  jawabanBenar = "BAHAYA" },
        new ChipData { teks = "Privat", jawabanBenar = "BAHAYA" }
    };

    [Header("Warna Zona")]
    public Color warnaZonaAman   = new Color(0.10f, 0.35f, 0.22f, 0.92f);
    public Color warnaZonaBahaya = new Color(0.40f, 0.12f, 0.12f, 0.92f);
    public Color warnaBorderAman = new Color(0.45f, 1f, 0.65f, 1f);
    public Color warnaBorderBahaya = new Color(1f, 0.45f, 0.45f, 1f);

    [Header("Label & Subjudul Zona (CUSTOMIZABLE)")]
    public string zonaAmanLabel      = "\u2713  ZONA AMAN";
    public string zonaAmanSubtitle   = "Boleh disentuh\nteman/keluarga";
    public string zonaBahayaLabel    = "\u2716  ZONA BAHAYA";
    public string zonaBahayaSubtitle = "Area privat\nDilarang disentuh!";
    public int    zonaSubtitleUkuran = 18;

    [Header("Tata Letak Zona Samping (anchor 0\u20131 layar)")]
    [Tooltip("Zona AMAN = strip tinggi di tepi KIRI layar.")]
    public Vector2 zonaAmanAnchorMin   = new Vector2(0.012f, 0.10f);
    public Vector2 zonaAmanAnchorMax   = new Vector2(0.205f, 0.86f);
    [Tooltip("Zona BAHAYA = strip tinggi di tepi KANAN layar.")]
    public Vector2 zonaBahayaAnchorMin = new Vector2(0.795f, 0.10f);
    public Vector2 zonaBahayaAnchorMax = new Vector2(0.988f, 0.86f);

    [Header("Karakter Tengah (opsional)")]
    [Tooltip("Sprite ilustrasi tubuh di tengah layar (di antara dua kolom label). Opsional.")]
    public Sprite  karakterSprite;
    public Vector2 karakterAnchoredPos = new Vector2(0f, -20f);
    public Vector2 karakterUkuran      = new Vector2(220f, 360f);

    [Header("Chip Style")]
    public Color  chipWarna       = new Color(0.18f, 0.20f, 0.30f, 0.95f);
    public Color  chipTeksWarna   = new Color(1f, 1f, 0.92f, 1f);
    public Color  chipBenarWarna  = new Color(0.18f, 0.62f, 0.32f, 0.95f);
    public Color  chipSalahWarna  = new Color(0.78f, 0.20f, 0.20f, 0.95f);
    public int    chipUkuranTeks  = 18;
    public Vector2 chipUkuran     = new Vector2(280f, 60f);

    [Header("Achievement (Bonus)")]
    public string namaAchievement = "Penjaga Batas Tubuh";
    public int    bonusAllBenar   = 200;

    [Header("Tombol Lanjut")]
    public string tombolLanjutTeks = "\u25B6  Lanjut";
    public Color  warnaLanjut = new Color(0.18f, 0.62f, 0.32f, 1f);

    [Header("BG Fullscreen Device (opsional)")]
    [Tooltip("Sprite latar FULLSCREEN device (stretch ke seluruh layar). Tampil paling belakang.\n" +
             "Kosongkan = latar polos / dipakai default.")]
    public Sprite bgFullscreenSprite;
    [Tooltip("Jaga aspek rasio sprite saat di-stretch fullscreen (mencegah gepeng).")]
    public bool   bgFullscreenPreserveAspect = false;

    // ══════════════════════════════════════════════════════════════════════
    // NARASI INTRO — baris narasi yang muncul SEBELUM quiz dimulai
    // ══════════════════════════════════════════════════════════════════════
    [System.Serializable]
    public class BarisNarasiQuiz
    {
        [Tooltip("Nama pembicara di banner (mis. 'Rara', 'Narasi').")]
        public string pembicara = "Rara";
        [TextArea(2, 5)]
        [Tooltip("Isi teks narasi.")]
        public string teks = "";
        [Tooltip("Sprite latar belakang fullscreen khusus untuk baris ini. Kosong = pakai latar narasi default.")]
        public Sprite latarBelakang;
    }

    [Header("Narasi Intro (sebelum quiz) — sambungan setelah AngkotSeatPicker")]
    [Tooltip("FALLBACK narasi intro. Dipakai HANYA jika:\n" +
             "  \u2022 GameState.seatCategory kosong, ATAU\n" +
             "  \u2022 Varian narasi spesifik di bawah (Aman/Ragu/Bahaya) kosong.\n" +
             "Kalau pemain sudah pilih kursi di AngkotSeatPicker, sistem akan otomatis\n" +
             "memilih narasiIntroAman / narasiIntroRagu / narasiIntroBahaya yang relevan.")]
    public BarisNarasiQuiz[] narasiIntro = new BarisNarasiQuiz[]
    {
        // CATATAN: narasi ini MENYAMBUNG dari fase Sentuh (pria menyentuh bahu →
        // Rara teriak TIDAK → PERGI/pindah kursi). JANGAN me-reset adegan dengan
        // "Pintu angkot ditutup / mulai melaju" karena itu sudah terjadi di Halte/Sentuh.
        new BarisNarasiQuiz { pembicara = "Narasi",
            teks = "Angkot terus melaju. Setelah kejadian tadi, Rara sudah pindah ke kursi lebih depan dan mencoba menenangkan napasnya." },
        new BarisNarasiQuiz { pembicara = "Rara",
            teks = "\"Untung aku tadi berani bersuara dan menjauh\u2026 tapi jantungku masih berdebar. Aku perlu mengalihkan pikiran sebentar.\"" },
        new BarisNarasiQuiz { pembicara = "Narasi",
            teks = "Rara mengeluarkan buku catatan PR Kesehatan dari tas. Kebetulan bab terakhirnya justru soal ini: \u201CKenali Batas Tubuhmu \u2014 Mana yang Boleh, Mana yang Tidak.\u201D" },
        new BarisNarasiQuiz { pembicara = "Rara",
            teks = "\"Justru sekarang aku makin paham kenapa bab ini penting. Ayo aku pelajari baik-baik \u2014 siapa yang BOLEH dan TIDAK BOLEH menyentuhku.\"" }
    };

    [Header("Narasi Intro — Varian per Kursi (DINONAKTIFKAN setelah alur Sentuh)")]
    [Tooltip("DIKOSONGKAN secara default. Sejak insiden 'Sentuh' (pria menyentuh bahu)\n" +
             "terjadi DI ANGKOT untuk SEMUA pilihan kursi, varian per-kursi yang\n" +
             "menggambarkan 'pria tidak ikut / hanya melirik' jadi BERTENTANGAN dengan\n" +
             "kejadian itu. Maka semua varian dikosongkan \u2192 fallback ke 'narasiIntro'\n" +
             "yang menyambung dari aftermath Sentuh. Isi lagi hanya kalau kamu mengubah\n" +
             "alur supaya insiden Sentuh tergantung pilihan kursi.")]
    public BarisNarasiQuiz[] narasiIntroAman = new BarisNarasiQuiz[0];

    [Tooltip("DIKOSONGKAN \u2014 lihat catatan narasiIntroAman. Fallback ke 'narasiIntro'.")]
    public BarisNarasiQuiz[] narasiIntroRagu = new BarisNarasiQuiz[0];

    [Tooltip("DIKOSONGKAN \u2014 lihat catatan narasiIntroAman. Fallback ke 'narasiIntro'.")]
    public BarisNarasiQuiz[] narasiIntroBahaya = new BarisNarasiQuiz[0];

    [Header("Narasi Outro (setelah quiz, sebelum fase Lapor) — jembatan ke fase Lapor")]
    [Tooltip("Baris narasi muncul SETELAH pemain klik tombol Lanjut di layar hasil quiz,\n" +
             "SEBELUM callback _onSelesai (yang memicu fase Lapor).\n" +
             "Konteks: Rara MASIH di dalam angkot. Pria asing yang sama (yang tadi\n" +
             "menyentuh bahunya) belum menyerah dan kembali merapat \u2014 memberi alasan\n" +
             "kenapa Rara harus berani CERITA / minta tolong Pak Supir di fase Lapor.\n" +
             "Kosongkan = langsung ke fase Lapor tanpa narasi outro.")]
    public BarisNarasiQuiz[] narasiOutro = new BarisNarasiQuiz[]
    {
        new BarisNarasiQuiz { pembicara = "Narasi",
            teks = "Angkot masih melaju menuju sekolah. Rara menutup buku catatannya \u2014 tapi dari sudut matanya, pria tadi belum menyerah." },
        new BarisNarasiQuiz { pembicara = "Rara",
            teks = "\"Dia\u2026 masih terus melirik ke arahku. Aku harus tetap waspada sampai turun nanti.\"" },
        new BarisNarasiQuiz { pembicara = "Narasi",
            teks = "Tiba-tiba pria itu kembali menggeser duduknya, makin merapat ke arah Rara!" },
        new BarisNarasiQuiz { pembicara = "Rara",
            teks = "\"Cukup! Kalau aku merasa tidak aman, aku harus CERITA \u2014 minta tolong orang dewasa. Pak Supir ada di depan!\"" }
    };

    [Tooltip("Detik per karakter saat narasi diketik. 0 = langsung penuh (skip animasi).")]
    [Range(0f, 0.15f)] public float kecepatanKetikNarasi = 0.025f;
    [Tooltip("Jeda setelah ketikan selesai sebelum hint 'klik untuk lanjut' muncul.")]
    [Range(0f, 1f)]    public float delaySetelahKetikNarasi = 0.12f;
    [Tooltip("Klik / SPACE saat sedang mengetik akan langsung menampilkan teks penuh.")]
    public bool        bolehSkipKetikNarasi = true;

    [Header("Narasi Intro \u2014 Style Box (mirror Day 1 Intro)")]
    [Tooltip("Sprite bingkai box dialog (mirror Day 1 Intro: 'UI day 1/8.png').\n" +
             "Kosong = pakai panel polos + outline.")]
    public Sprite narasiBoxDialogSprite;
    [Tooltip("Path sprite bingkai box dialog untuk auto-load di Editor.")]
    public string narasiBoxDialogSpritePath = "sprites/UI day 1/8.png";
    [Tooltip("Sprite banner nama pembicara (opsional). Kosong = pakai warna polos.")]
    public Sprite narasiNameBannerSprite;
    [Tooltip("Portrait untuk pembicara 'Rara'. Auto-load dari Day1Intro kalau ada.")]
    public Sprite narasiPortraitRara;
    [Tooltip("Portrait untuk pembicara 'Narasi' (siluet Rara).")]
    public Sprite narasiPortraitNarasi;
    [Tooltip("Sprite latar fullscreen khusus layar narasi intro/outro.\n" +
             "Kosong = pakai 'bgFullscreenSprite' yang sama dengan layar quiz.")]
    public Sprite narasiBgFullscreenSprite;
    [Tooltip("Warna latar solid saat semua sprite latar kosong.\n" +
             "Default = warna interior angkot (sama dengan AngkotSentuhScene) supaya konsisten.")]
    public Color  narasiBgWarna     = new Color(0.16f, 0.12f, 0.09f, 1f);
    public Color  narasiPanelWarna  = new Color(0f, 0f, 0f, 0f);
    public Color  narasiBorderWarna = new Color(1f, 0.85f, 0.30f, 1f);
    public Color  narasiBannerWarna = new Color(0.14f, 0.09f, 0.01f, 0f);
    public Color  narasiNamaWarna   = new Color(1f, 0.85f, 0.30f, 1f);
    public Color  narasiTeksWarna   = new Color(1f, 0.96f, 0.88f, 1f);
    public Color  narasiHintWarna   = new Color(1f, 1f, 1f, 0.55f);
    public Color  narasiPortraitFallbackWarna = new Color(0.85f, 0.55f, 0.75f, 1f);
    public int    narasiUkuranNama  = 30;
    public int    narasiUkuranTeks  = 30;
    public int    narasiUkuranHint  = 18;
    public string narasiTeksHint    = "";

    [Header("Narasi Intro \u2014 Anchor Box (mirror Day 1 Intro)")]
    [Range(0f, 1f)] public float narasiPanelCenterX    = 0.5f;
    [Range(0f, 1f)] public float narasiPanelCenterY    = 0.16f;
    [Range(0f, 1f)] public float narasiPanelWidthFrac  = 0.972f;
    [Range(0f, 1f)] public float narasiPanelHeightFrac = 0.291f;
    [Range(0f, 1f)] public float narasiPortraitCenterX = 0.14f;
    [Range(0f, 1f)] public float narasiPortraitCenterY = 0.584f;
    [Range(0f, 1f)] public float narasiPortraitSizeW   = 0.189f;
    [Range(0f, 1f)] public float narasiPortraitSizeH   = 0.56f;
    public bool                 narasiPortraitPreserveAspect = false;
    public Vector2              narasiBannerAnchorMin = new Vector2(0.11f, 0.11f);
    public Vector2              narasiBannerAnchorMax = new Vector2(0.253f, 0.333f);
    public Vector2              narasiTextAnchorMin   = new Vector2(0.31f, 0.55f);
    public Vector2              narasiTextAnchorMax   = new Vector2(0.84f, 0.76f);
    [Range(0f, 1f)] public float narasiHintCenterX     = 0.798f;
    [Range(0f, 1f)] public float narasiHintCenterY     = 0.242f;
    [Range(0f, 1f)] public float narasiHintSizeW       = 0.296f;
    [Range(0f, 1f)] public float narasiHintSizeH       = 0.12f;

    // ══════════════════════════════════════════════════════════════════════
    // TUTORIAL MODAL — panel "🧩 QUIZ: KENALI BATAS TUBUH!" + tombol "▶ SIAP, MULAI!"
    // Mirror Day2.js _showInlineTutorial — muncul SETELAH narasi, SEBELUM quiz UI.
    // ══════════════════════════════════════════════════════════════════════
    [Header("Tutorial Modal (sebelum kartu chip muncul)")]
    [Tooltip("Tampilkan tutorial modal setelah narasi dan SEBELUM kartu chip muncul. " +
             "Mirror Day2.js _showInlineTutorial.")]
    public bool    tampilkanTutorial = true;
    [Tooltip("Judul tutorial modal.")]
    public string  tutorialJudul = "\uD83E\uDDE9 QUIZ: KENALI BATAS TUBUH!";
    [Tooltip("Body tutorial modal — instruksi singkat. Pakai \\n untuk newline.")]
    [TextArea(3, 8)]
    public string  tutorialBody  =
        "Geser / drag nama bagian tubuh ke zona yang sesuai!\n" +
        "(Atau KLIK chip-nya dulu, lalu KLIK zonanya.)\n\n" +
        "\u2705 ZONA AMAN = boleh disentuh teman & keluarga\n" +
        "\u274C ZONA BAHAYA = area privat, NGGAK BOLEH!\n\n" +
        "\u23F0 Waktu: 15 detik — cepat!";
    [Tooltip("Label tombol mulai.")]
    public string  tutorialTombol = "\u25B6  SIAP, MULAI!";

    [Header("Tutorial Modal — Style")]
    public Color tutorialPanelWarna   = new Color(0.07f, 0.13f, 0.27f, 0.97f);
    public Color tutorialBorderWarna  = new Color(0.33f, 0.67f, 1f, 0.90f);
    public Color tutorialJudulWarna   = new Color(1f, 0.84f, 0f, 1f);
    public Color tutorialBodyWarna    = new Color(0.80f, 0.87f, 1f, 1f);
    public Color tutorialTombolBg     = new Color(0f, 0.40f, 0.20f, 0.90f);
    public Color tutorialTombolBorder = new Color(0.27f, 1f, 0.53f, 0.70f);
    public Color tutorialTombolTeks   = new Color(0.27f, 1f, 0.53f, 1f);
    public int   tutorialUkuranJudul  = 30;
    public int   tutorialUkuranBody   = 22;
    public int   tutorialUkuranTombol = 22;

    [Header("Font")]
    public TMP_FontAsset fontAsset;

    [Header("Sorting")]
    public int sortingOrder = 930;

    // ── runtime ───────────────────────────────────────────────────────────
    private Action     _onSelesai;
    private GameObject _canvasGO;
    private RectTransform _zonaAmanRT;
    private RectTransform _zonaBahayaRT;
    private Transform  _zonaAmanContent;   // wadah chip yang sudah masuk ke ZONA AMAN
    private Transform  _zonaBahayaContent; // wadah chip yang sudah masuk ke ZONA BAHAYA
    private TextMeshProUGUI _timerText;
    private TextMeshProUGUI _skorText;
    private float      _sisaWaktu;
    private bool       _quizSelesai;
    private int        _chipDitempatkan;
    private int        _chipBenar;
    private List<GameObject> _chipPool = new List<GameObject>();
    private Sprite     _roundedSprite;
    private Canvas     _canvasComp;
    private DraggableChip _chipTerpilih; // chip yang dipilih lewat KLIK (fallback tanpa drag)

    // State narasi intro (typewriter)
    private GameObject      _narasiCanvasGO;
    private TextMeshProUGUI _narasiNamaTMP;
    private TextMeshProUGUI _narasiTeksTMP;
    private TextMeshProUGUI _narasiHintTMP;
    private TombolLanjutVN  _narasiTombolLanjut;
    private Image           _narasiBgImg;
    private Image           _narasiPortraitImg;
    private bool _ketikSelesai;
    private bool _skipKetik;

    // State UI persisten yang disembunyikan/diatur selama kuis (controller & pause).
    private MobileControls _mcRef;
    private bool           _mcForceHideAsli;
    private PauseMenu      _pauseRef;
    private Vector2        _pauseMarginAsli;
    private bool           _uiPersistenDisesuaikan = false;

    // ══════════════════════════════════════════════════════════════════════
    public void Mulai(Action onSelesai)
    {
        _onSelesai = onSelesai;
        HUDManager.Instance?.SetNavbarVisible(false); // sembunyikan navbar selama kuis batas tubuh
        SesuaikanUiPersisten();                       // sembunyikan controller + rapikan tombol pause
        AutoResolveNarasiAssets();
        TerapkanDefaultLatarDialogKosong();
        var narasiAktif = PilihNarasiIntroBerdasarkanKursi();
        if (narasiAktif != null && narasiAktif.Length > 0)
            StartCoroutine(JalankanNarasiLaluQuiz(narasiAktif));
        else if (tampilkanTutorial)
            StartCoroutine(TampilkanTutorialLaluQuiz());
        else
            MulaiQuizLangsung();
    }

    void TerapkanDefaultLatarDialogKosong()
    {
        Sprite defaultBg = narasiBgFullscreenSprite != null ? narasiBgFullscreenSprite : bgFullscreenSprite;
        if (defaultBg == null) return;

        IsiDefaultLatarPadaBaris(narasiIntro, defaultBg);
        IsiDefaultLatarPadaBaris(narasiIntroAman, defaultBg);
        IsiDefaultLatarPadaBaris(narasiIntroRagu, defaultBg);
        IsiDefaultLatarPadaBaris(narasiIntroBahaya, defaultBg);
        IsiDefaultLatarPadaBaris(narasiOutro, defaultBg);

        if (chips != null)
        {
            for (int i = 0; i < chips.Length; i++)
            {
                if (chips[i] == null) continue;
                if (chips[i].latarBelakangDialog == null)
                    chips[i].latarBelakangDialog = defaultBg;
            }
        }

        if (vnRingkasanLatarBelakang == null)
            vnRingkasanLatarBelakang = defaultBg;
    }

    void IsiDefaultLatarPadaBaris(BarisNarasiQuiz[] baris, Sprite defaultBg)
    {
        if (baris == null) return;
        for (int i = 0; i < baris.Length; i++)
        {
            if (baris[i] == null) continue;
            if (baris[i].latarBelakang == null)
                baris[i].latarBelakang = defaultBg;
        }
    }

    /// <summary>
    /// Sembunyikan tombol controller (D-pad/TERIAK) dan rapikan posisi tombol pause
    /// selama kuis berlangsung supaya tidak menumpuk dengan panel zona AMAN/BAHAYA.
    /// Nilai asli disimpan agar bisa dikembalikan saat kuis selesai.
    /// </summary>
    void SesuaikanUiPersisten()
    {
        if (_uiPersistenDisesuaikan) return;
        _uiPersistenDisesuaikan = true;

        // Controller: paksa sembunyi selama kuis (drag-drop tidak butuh kontrol jalan).
        _mcRef = MobileControls.Instance;
        if (_mcRef != null)
        {
            _mcForceHideAsli = _mcRef.forceHide;
            _mcRef.forceHide = true;
        }

        // Tombol pause: angkat ke pojok kanan-atas yang bersih (di atas panel zona)
        // supaya tidak menimpa judul "ZONA BAHAYA". Posisi di-sync tiap frame oleh
        // PauseMenu dari margin ini, jadi cukup ubah margin-nya.
        _pauseRef = FindFirstObjectByType<PauseMenu>(FindObjectsInactive.Include);
        if (_pauseRef != null)
        {
            _pauseMarginAsli = _pauseRef.mobilePauseButtonMargin;
            _pauseRef.mobilePauseButtonMargin = new Vector2(28f, 24f);
        }
    }

    /// <summary>
    /// Kembalikan controller & posisi tombol pause ke kondisi semula setelah kuis.
    /// </summary>
    void KembalikanUiPersisten()
    {
        if (!_uiPersistenDisesuaikan) return;
        _uiPersistenDisesuaikan = false;

        if (_mcRef != null) _mcRef.forceHide = _mcForceHideAsli;
        if (_pauseRef != null) _pauseRef.mobilePauseButtonMargin = _pauseMarginAsli;
    }

    // Jaring pengaman: kalau objek kuis dinonaktifkan/dihancurkan di tengah jalan,
    // pastikan controller & tombol pause tetap dipulihkan agar tidak stuck.
    void OnDisable()
    {
        KembalikanUiPersisten();
    }

    /// <summary>
    /// Auto-cari sprite box dialog + portrait dari Day1Intro yang ada di scene,
    /// supaya saat runtime ZonaTubuhQuiz langsung punya look sama dengan Day1Intro
    /// tanpa harus assign manual di Inspector.
    /// </summary>
    void AutoResolveNarasiAssets()
    {
        var d1 = FindFirstObjectByType<Day1Intro>(FindObjectsInactive.Include);
        if (d1 != null)
        {
            if (narasiBoxDialogSprite == null && d1.boxDialogSprite != null)
                narasiBoxDialogSprite = d1.boxDialogSprite;
            if (narasiNameBannerSprite == null && d1.nameBannerSprite != null)
                narasiNameBannerSprite = d1.nameBannerSprite;
            if (narasiPortraitRara == null && d1.portraitRara != null)
                narasiPortraitRara = d1.portraitRara;
            if (narasiPortraitNarasi == null && d1.portraitNarasi != null)
                narasiPortraitNarasi = d1.portraitNarasi;
        }
        // Fallback: kalau ada Day2NarasiAwal aktif, ikut ambil sprite-nya
        if (narasiBoxDialogSprite == null || narasiPortraitRara == null)
        {
            var d2 = FindFirstObjectByType<Day2NarasiAwal>(FindObjectsInactive.Include);
            if (d2 != null)
            {
                if (narasiBoxDialogSprite == null && d2.panelSprite != null)
                    narasiBoxDialogSprite = d2.panelSprite;
                if (narasiNameBannerSprite == null && d2.nameBannerSprite != null)
                    narasiNameBannerSprite = d2.nameBannerSprite;
                if (narasiPortraitRara == null && d2.portraitRara != null)
                    narasiPortraitRara = d2.portraitRara;
                if (narasiPortraitNarasi == null && d2.portraitNarasi != null)
                    narasiPortraitNarasi = d2.portraitNarasi;
            }
        }
    }

    /// <summary>
    /// Pilih array narasi intro yang relevan berdasarkan
    /// <c>GameState.seatCategory</c> yang di-set oleh AngkotSeatPicker:
    ///   AMAN   \u2192 narasiIntroAman
    ///   RAGU   \u2192 narasiIntroRagu
    ///   BAHAYA \u2192 narasiIntroBahaya
    /// Kalau varian-nya kosong (atau seatCategory belum di-set), fallback ke
    /// <c>narasiIntro</c> default.
    /// </summary>
    BarisNarasiQuiz[] PilihNarasiIntroBerdasarkanKursi()
    {
        var gs = GameState.Instance;
        string kategori = gs != null ? (gs.seatCategory ?? "") : "";
        BarisNarasiQuiz[] varian = null;
        switch (kategori)
        {
            case "AMAN":   varian = narasiIntroAman;   break;
            case "RAGU":   varian = narasiIntroRagu;   break;
            case "BAHAYA": varian = narasiIntroBahaya; break;
        }
        if (varian != null && varian.Length > 0)
        {
            Debug.Log($"[ZonaTubuhQuiz] Pilih narasi intro varian '{kategori}' ({varian.Length} baris).");
            return varian;
        }
        if (!string.IsNullOrEmpty(kategori))
            Debug.Log($"[ZonaTubuhQuiz] seatCategory='{kategori}' tapi variannya kosong \u2192 fallback narasiIntro default.");
        return narasiIntro;
    }

    void MulaiQuizLangsung()
    {
        if (modeVisualNovel)
        {
            StartCoroutine(JalankanQuizVN());
            return;
        }
        BuildScene();
        StartCoroutine(TimerCoroutine());
    }

    // ══════════════════════════════════════════════════════════════════════
    // MODE VISUAL NOVEL — quiz sebagai tanya-jawab bercabang dalam box dialog.
    // Tiap perilaku ditanyakan "BOLEH / TIDAK BOLEH?" lewat 2 tombol pilihan.
    // Skor & achievement identik dengan mode drag-drop (SCORE_QUIZ/2 per benar,
    // bonus + achievement kalau semua benar), lalu menyambung ke narasi outro.
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator JalankanQuizVN()
    {
        _quizSelesai = false;
        _chipBenar = 0;

        // Pastikan EventSystem ada supaya tombol pilihan VN bisa diklik.
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        BuildNarasiCanvas();

        var gs = GameState.Instance;

        for (int i = 0; i < chips.Length; i++)
        {
            var chip = chips[i];
            if (chip == null) continue;
            UpdateNarasiBackground(chip.latarBelakangDialog);

            // Tampilkan pertanyaan (typewriter) sebagai Rara.
            if (_narasiNamaTMP != null) _narasiNamaTMP.text = "RARA";
            UpdateNarasiPortrait("Rara");
            string tanya = (vnPertanyaanTemplate ?? "{PERILAKU}").Replace("{PERILAKU}", chip.teks ?? "");
            yield return KetikTeksNarasi(tanya);

            // Sembunyikan hint "klik untuk lanjut" — pemain harus memilih tombol.
            if (_narasiHintTMP != null) _narasiHintTMP.gameObject.SetActive(false);

            // Bangun 2 tombol pilihan, tunggu pemain memilih.
            int dipilih = -1;
            var tombolGO = BuildPilihanVN(new[] { vnLabelBoleh, vnLabelTidakBoleh }, idx => dipilih = idx);
            while (dipilih < 0) yield return null;
            if (tombolGO != null) Destroy(tombolGO);

            string jawabanPemain = dipilih == 0 ? "AMAN" : "BAHAYA";
            bool benar = jawabanPemain == chip.jawabanBenar;
            if (benar) _chipBenar++;

            // SFX
            var am = AudioManager.Instance;
            if (am != null && am.sfxSource != null)
            {
                if (benar && am.sfxCorrect != null) am.sfxSource.PlayOneShot(am.sfxCorrect);
                else if (!benar && am.sfxWrong != null) am.sfxSource.PlayOneShot(am.sfxWrong);
            }

            // Skor (identik drag-drop: SCORE_QUIZ/2 per jawaban benar).
            if (gs != null)
            {
                int pts = benar ? (GameState.SCORE_QUIZ / 2) : 0;
                gs.score += pts;
                gs.AddChoice(2, $"Quiz: {chip.teks} \u2192 {jawabanPemain}", benar ? "AMAN" : "BAHAYA", pts);
            }

            // Umpan balik (typewriter) lalu tunggu tap.
            string benarTxt = chip.jawabanBenar == "AMAN" ? "BOLEH" : "TIDAK BOLEH";
            string fb = benar
                ? "\u2713 Tepat! Ini memang " + benarTxt + "."
                : "\u2716 Kurang tepat. Yang benar: " + benarTxt + ".";
            if (_narasiNamaTMP != null) _narasiNamaTMP.text = "NARASI";
            UpdateNarasiPortrait("Narasi");
            yield return KetikTeksNarasi(fb);
            yield return TungguTapNarasi();
        }

        // Ringkasan singkat.
        bool semuaBenar = _chipBenar == chips.Length;
        UpdateNarasiBackground(vnRingkasanLatarBelakang);
        if (_narasiNamaTMP != null) _narasiNamaTMP.text = "NARASI";
        UpdateNarasiPortrait("Narasi");
        yield return KetikTeksNarasi($"Kamu menjawab benar {_chipBenar}/{chips.Length}. Ingat: tubuhmu milikmu \u2014 kamu berhak bilang TIDAK.");
        yield return TungguTapNarasi();

        // Hancurkan canvas narasi quiz.
        if (_narasiCanvasGO != null) Destroy(_narasiCanvasGO);
        _narasiCanvasGO = null;

        // Scoring akhir (bonus + achievement) — identik SelesaikanQuiz.
        _quizSelesai = true;
        if (semuaBenar && gs != null)
        {
            gs.score += bonusAllBenar;
            if (!gs.achievements.Contains(namaAchievement))
            {
                gs.achievements.Add(namaAchievement);
                AchievementPopup.Show(namaAchievement);
            }
            Debug.Log($"[ZonaTubuhQuiz] (VN) PERFECT! Bonus +{bonusAllBenar} + achievement.");
        }

        // Lanjut ke narasi outro (jembatan ke Lapor) atau langsung selesai.
        if (narasiOutro != null && narasiOutro.Length > 0)
            yield return JalankanNarasiOutroLaluSelesai();
        else
        {
            HUDManager.Instance?.SetNavbarVisible(true); // tampilkan kembali navbar saat keluar
            KembalikanUiPersisten();
            _onSelesai?.Invoke();
        }
    }

    // Bangun 2+ tombol pilihan di atas box dialog narasi VN. onPick(index) saat diklik.
    GameObject BuildPilihanVN(string[] labels, Action<int> onPick)
    {
        var wrap = new GameObject("PilihanVN");
        wrap.transform.SetParent(_narasiCanvasGO.transform, false);
        var wrt = wrap.AddComponent<RectTransform>();
        wrt.anchorMin = new Vector2(0.5f, 0.42f);
        wrt.anchorMax = new Vector2(0.5f, 0.42f);
        wrt.pivot     = new Vector2(0.5f, 0.5f);
        wrt.anchoredPosition = Vector2.zero;
        wrt.sizeDelta = new Vector2(760f, 180f);

        var vlg = wrap.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 18f;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true; vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

        Color[] warna = { new Color(0.18f, 0.62f, 0.32f, 0.96f), new Color(0.85f, 0.27f, 0.24f, 0.96f) };

        for (int i = 0; i < labels.Length; i++)
        {
            int idx = i;
            var btnGO = new GameObject("Pilihan_" + i);
            btnGO.transform.SetParent(wrap.transform, false);
            var img = btnGO.AddComponent<Image>();
            img.sprite = GetRoundedSprite();
            img.color  = warna[i % warna.Length];
            img.type   = Image.Type.Sliced;
            var le = btnGO.AddComponent<LayoutElement>();
            le.preferredHeight = 72f; le.preferredWidth = 700f;
            var outl = btnGO.AddComponent<Outline>();
            outl.effectColor    = new Color(1f, 1f, 1f, 0.35f);
            outl.effectDistance = new Vector2(2f, -2f);

            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = img;
            var baseC = warna[i % warna.Length];
            var cb = btn.colors;
            cb.normalColor      = baseC;
            cb.highlightedColor = new Color(Mathf.Min(1f, baseC.r * 1.15f), Mathf.Min(1f, baseC.g * 1.15f), Mathf.Min(1f, baseC.b * 1.15f), baseC.a);
            cb.pressedColor     = new Color(baseC.r * 0.8f, baseC.g * 0.8f, baseC.b * 0.8f, baseC.a);
            btn.colors = cb;
            btn.onClick.AddListener(() =>
            {
                AudioManager.Instance?.Click();
                onPick?.Invoke(idx);
            });

            var lbl = BuatTeks(btnGO.transform, "Label", labels[i], 24, Color.white, FontStyles.Bold);
            lbl.alignment = TextAlignmentOptions.Center;
            lbl.raycastTarget = false;
            var lrt = lbl.rectTransform;
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        }

        return wrap;
    }

    // ══════════════════════════════════════════════════════════════════════
    // NARASI INTRO — overlay dialog + typewriter, klik untuk skip/lanjut
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator JalankanNarasiLaluQuiz(BarisNarasiQuiz[] narasiAktif)
    {
        BuildNarasiCanvas();
        for (int i = 0; i < narasiAktif.Length; i++)
        {
            var baris = narasiAktif[i];
            if (baris == null) continue;
            UpdateNarasiBackground(baris);
            if (_narasiNamaTMP != null) _narasiNamaTMP.text = (baris.pembicara ?? "").ToUpper();
            UpdateNarasiPortrait(baris.pembicara);
            yield return KetikTeksNarasi(baris.teks ?? "");
            yield return TungguTapNarasi();
        }
        if (_narasiCanvasGO != null) Destroy(_narasiCanvasGO);
        _narasiCanvasGO = null;

        if (tampilkanTutorial)
            yield return TampilkanTutorialLaluQuiz();
        else
            MulaiQuizLangsung();
    }

    // ══════════════════════════════════════════════════════════════════════
    // NARASI OUTRO — jembatan setelah quiz, sebelum fase Lapor
    // Pemain klik "Lanjut" di layar hasil → narasi outro muncul (Rara MASIH di
    // angkot, pria yang sama merapat lagi) → setelah selesai, _onSelesai()
    // dipanggil (memicu fase berikutnya di Day2Controller, yaitu Lapor).
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator JalankanNarasiOutroLaluSelesai()
    {
        // Hancurkan canvas quiz dulu supaya outro tampil di layar bersih.
        if (_canvasGO != null) Destroy(_canvasGO);
        _canvasGO = null;
        // Beri 1 frame agar Destroy benar-benar diproses.
        yield return null;

        BuildNarasiCanvas();
        for (int i = 0; i < narasiOutro.Length; i++)
        {
            var baris = narasiOutro[i];
            if (baris == null) continue;
            UpdateNarasiBackground(baris);
            if (_narasiNamaTMP != null) _narasiNamaTMP.text = (baris.pembicara ?? "").ToUpper();
            UpdateNarasiPortrait(baris.pembicara);
            yield return KetikTeksNarasi(baris.teks ?? "");
            yield return TungguTapNarasi();
        }
        if (_narasiCanvasGO != null) Destroy(_narasiCanvasGO);
        _narasiCanvasGO = null;

        HUDManager.Instance?.SetNavbarVisible(true); // tampilkan kembali navbar saat keluar
        KembalikanUiPersisten();
        _onSelesai?.Invoke();
    }

    void UpdateNarasiBackground(BarisNarasiQuiz baris)
    {
        UpdateNarasiBackground(baris != null ? baris.latarBelakang : null);
    }

    void UpdateNarasiBackground(Sprite bgOverride)
    {
        if (_narasiBgImg == null) return;

        Sprite bgAktif = bgOverride;
        if (bgAktif == null)
            bgAktif = narasiBgFullscreenSprite != null ? narasiBgFullscreenSprite : bgFullscreenSprite;

        if (bgAktif != null)
        {
            _narasiBgImg.sprite         = bgAktif;
            _narasiBgImg.preserveAspect = bgFullscreenPreserveAspect;
            _narasiBgImg.color          = Color.white;
        }
        else
        {
            _narasiBgImg.sprite = null;
            _narasiBgImg.color  = narasiBgWarna;
        }
    }

    IEnumerator KetikTeksNarasi(string teks)
    {
        if (_narasiTeksTMP == null) yield break;
        _ketikSelesai = false;
        _skipKetik    = false;
        if (_narasiHintTMP != null) _narasiHintTMP.gameObject.SetActive(false);

        if (kecepatanKetikNarasi <= 0f)
        {
            _narasiTeksTMP.text = teks;
        }
        else
        {
            _narasiTeksTMP.text = "";
            for (int i = 0; i < teks.Length; i++)
            {
                if (bolehSkipKetikNarasi && _skipKetik) { _narasiTeksTMP.text = teks; break; }
                _narasiTeksTMP.text += teks[i];
                if (teks[i] != ' ') AudioManager.Instance?.PlayKetikHuruf();
                yield return new WaitForSeconds(kecepatanKetikNarasi);
            }
        }
        _ketikSelesai = true;
        if (delaySetelahKetikNarasi > 0f) yield return new WaitForSeconds(delaySetelahKetikNarasi);
        if (_narasiHintTMP != null) _narasiHintTMP.gameObject.SetActive(true);
    }

    IEnumerator TungguTapNarasi()
    {
        // Hanya tombol LANJUT (atau SPACE/ENTER) yang melanjutkan; klik di luar diabaikan.
        _narasiTombolLanjut?.Reset();
        while (true)
        {
            bool ditekan = (_narasiTombolLanjut != null && _narasiTombolLanjut.Konsumsi())
                        || Input.GetKeyDown(KeyCode.Space)
                        || Input.GetKeyDown(KeyCode.Return)
                        || Input.GetKeyDown(KeyCode.KeypadEnter);
            if (ditekan)
            {
                if (bolehSkipKetikNarasi && !_ketikSelesai) _skipKetik = true;
                else if (_ketikSelesai)                     break;
            }
            yield return null;
        }
        AudioManager.Instance?.Click();
        yield return new WaitForSeconds(0.05f);
    }

    void BuildNarasiCanvas()
    {
        _narasiCanvasGO = new GameObject("ZonaTubuhQuiz_NarasiCanvas");
        var cv = _narasiCanvasGO.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = sortingOrder + 5; // di atas quiz canvas
        var sc = _narasiCanvasGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920f, 1080f);
        sc.matchWidthOrHeight  = 0.5f;
        _narasiCanvasGO.AddComponent<GraphicRaycaster>();

        // ── BG fullscreen device (sprite latar Day 2) ──
        // Pakai narasiBgFullscreenSprite kalau diisi; fallback ke bgFullscreenSprite
        // (sprite yang sama dipakai layar quiz). Kalau dua-duanya kosong → dim hitam.
        Sprite bgSprite = narasiBgFullscreenSprite != null ? narasiBgFullscreenSprite : bgFullscreenSprite;
        var bg = new GameObject("BG_Fullscreen");
        bg.transform.SetParent(_narasiCanvasGO.transform, false);
        var bgRT = bg.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
        _narasiBgImg = bg.AddComponent<Image>();
        if (bgSprite != null)
        {
            _narasiBgImg.sprite         = bgSprite;
            _narasiBgImg.preserveAspect = bgFullscreenPreserveAspect;
            _narasiBgImg.color          = Color.white;
        }
        else
        {
            // Fallback: warna interior angkot SOLID (sama dengan AngkotSentuhScene),
            // bukan dim transparan — supaya latar biru kosong scene tidak menembus.
            _narasiBgImg.color = narasiBgWarna;
        }
        _narasiBgImg.raycastTarget = false;

        // ── Panel utama (anchor fraksi layar, mirror Day1Intro) ──
        float pxMin = narasiPanelCenterX - narasiPanelWidthFrac  * 0.5f;
        float pyMin = narasiPanelCenterY - narasiPanelHeightFrac * 0.5f;
        float pxMax = narasiPanelCenterX + narasiPanelWidthFrac  * 0.5f;
        float pyMax = narasiPanelCenterY + narasiPanelHeightFrac * 0.5f;

        var panel = new GameObject("Panel");
        panel.transform.SetParent(_narasiCanvasGO.transform, false);
        var prt = panel.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(pxMin, pyMin);
        prt.anchorMax = new Vector2(pxMax, pyMax);
        prt.offsetMin = prt.offsetMax = Vector2.zero;

        var pImg = panel.AddComponent<Image>();
        if (narasiBoxDialogSprite != null)
        {
            pImg.sprite = narasiBoxDialogSprite;
            pImg.type   = Image.Type.Sliced;
            pImg.color  = Color.white;
        }
        else
        {
            pImg.color = narasiPanelWarna;
            var outl = panel.AddComponent<Outline>();
            outl.effectColor    = narasiBorderWarna;
            outl.effectDistance = new Vector2(2f, -2f);
        }
        // Klik panel = skip / lanjut (raycast aktif untuk klik di seluruh box)
        pImg.raycastTarget = true;

        // ── Portrait kiri (siluet pembicara) ──
        var portGO = new GameObject("Portrait");
        portGO.transform.SetParent(panel.transform, false);
        var portRT = portGO.AddComponent<RectTransform>();
        portRT.anchorMin = new Vector2(
            narasiPortraitCenterX - narasiPortraitSizeW * 0.5f,
            narasiPortraitCenterY - narasiPortraitSizeH * 0.5f);
        portRT.anchorMax = new Vector2(
            narasiPortraitCenterX + narasiPortraitSizeW * 0.5f,
            narasiPortraitCenterY + narasiPortraitSizeH * 0.5f);
        portRT.offsetMin = portRT.offsetMax = Vector2.zero;
        _narasiPortraitImg = portGO.AddComponent<Image>();
        _narasiPortraitImg.preserveAspect = narasiPortraitPreserveAspect;
        _narasiPortraitImg.color          = narasiPortraitFallbackWarna;
        _narasiPortraitImg.raycastTarget  = false;

        // ── Banner nama (kiri-bawah panel) ──
        var banner = new GameObject("BannerNama");
        banner.transform.SetParent(panel.transform, false);
        var brt = banner.AddComponent<RectTransform>();
        brt.anchorMin = narasiBannerAnchorMin;
        brt.anchorMax = narasiBannerAnchorMax;
        brt.offsetMin = brt.offsetMax = Vector2.zero;
        var bImg = banner.AddComponent<Image>();
        if (narasiNameBannerSprite != null)
        {
            bImg.sprite = narasiNameBannerSprite;
            bImg.type   = Image.Type.Sliced;
            bImg.color  = Color.white;
        }
        else
        {
            bImg.color = narasiBannerWarna;
        }
        bImg.raycastTarget = false;

        _narasiNamaTMP = BuatTeks(banner.transform, "Nama", "",
            narasiUkuranNama, narasiNamaWarna, FontStyles.Bold);
        _narasiNamaTMP.alignment = TextAlignmentOptions.MidlineLeft;
        _narasiNamaTMP.margin    = new Vector4(12f, 0f, 4f, 0f);
        var nrt = _narasiNamaTMP.rectTransform;
        nrt.anchorMin = Vector2.zero; nrt.anchorMax = Vector2.one;
        nrt.offsetMin = nrt.offsetMax = Vector2.zero;

        // ── Area teks (kanan portrait) ──
        _narasiTeksTMP = BuatTeks(panel.transform, "Teks", "",
            narasiUkuranTeks, narasiTeksWarna, FontStyles.Normal);
        _narasiTeksTMP.alignment           = TextAlignmentOptions.TopLeft;
        _narasiTeksTMP.textWrappingMode    = TMPro.TextWrappingModes.Normal;
        _narasiTeksTMP.overflowMode        = TextOverflowModes.Overflow;
        var trt = _narasiTeksTMP.rectTransform;
        trt.anchorMin = narasiTextAnchorMin;
        trt.anchorMax = narasiTextAnchorMax;
        trt.offsetMin = trt.offsetMax = Vector2.zero;

        // ── Hint (kanan-bawah panel) ──
        _narasiHintTMP = BuatTeks(panel.transform, "Hint", narasiTeksHint,
            narasiUkuranHint, narasiHintWarna, FontStyles.Italic);
        _narasiHintTMP.alignment = TextAlignmentOptions.MidlineRight;
        var hrt = _narasiHintTMP.rectTransform;
        hrt.anchorMin = new Vector2(
            narasiHintCenterX - narasiHintSizeW * 0.5f,
            narasiHintCenterY - narasiHintSizeH * 0.5f);
        hrt.anchorMax = new Vector2(
            narasiHintCenterX + narasiHintSizeW * 0.5f,
            narasiHintCenterY + narasiHintSizeH * 0.5f);
        hrt.offsetMin = hrt.offsetMax = Vector2.zero;
        _narasiHintTMP.gameObject.SetActive(false);

        // ── Tombol LANJUT: HANYA tombol ini yang melanjutkan narasi ──
        // (klik di luar tombol tidak lagi melanjutkan)
        _narasiTombolLanjut = TombolLanjutVN.Pasang(panel.transform, null,
            "LANJUT  \u25B6", new Vector2(0.70f, 0.06f), new Vector2(0.975f, 0.26f));
    }

    /// <summary>Pilih portrait berdasarkan nama pembicara (mirror Day1Intro/Day2NarasiAwal).</summary>
    void UpdateNarasiPortrait(string pembicara)
    {
        if (_narasiPortraitImg == null) return;
        string p = (pembicara ?? "").ToLower();
        Sprite sp = null;
        if (p.Contains("narasi"))
            sp = narasiPortraitNarasi != null ? narasiPortraitNarasi : narasiPortraitRara;
        else // Rara atau default
            sp = narasiPortraitRara != null ? narasiPortraitRara : narasiPortraitNarasi;

        if (sp != null)
        {
            // Potret/sprite profil disembunyikan dari box dialog.
            _narasiPortraitImg.sprite  = sp;
            _narasiPortraitImg.color   = Color.white;
            _narasiPortraitImg.enabled = false;
        }
        else
        {
            // Fallback: tampilkan kotak warna polos supaya tata letak tetap konsisten
            _narasiPortraitImg.sprite  = null;
            _narasiPortraitImg.color   = narasiPortraitFallbackWarna;
            _narasiPortraitImg.enabled = false;
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // TUTORIAL MODAL — panel "🧩 QUIZ: KENALI BATAS TUBUH!" + tombol "▶ SIAP, MULAI!"
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator TampilkanTutorialLaluQuiz()
    {
        bool siap = false;
        var tutorialGO = BuildTutorialModal(() => siap = true);
        while (!siap) yield return null;
        if (tutorialGO != null) Destroy(tutorialGO);
        // Beri 1 frame agar modal benar-benar destroyed sebelum quiz scene dibangun
        yield return null;
        MulaiQuizLangsung();
    }

    GameObject BuildTutorialModal(Action onSiap)
    {
        var canvasGO = new GameObject("ZonaTubuhQuiz_TutorialCanvas");
        var cv = canvasGO.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = sortingOrder + 6; // di atas narasi & quiz
        var sc = canvasGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920f, 1080f);
        sc.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── BACKDROP FULLSCREEN BURAM ───────────────────────────────────────
        // Menutup TOTAL latar Day 1 di belakang modal (bukan sekadar dim transparan).
        // Lapisan 1: warna dasar deep-navy OPAQUE (atau sprite kustom bila di-assign).
        var dim = new GameObject("Backdrop");
        dim.transform.SetParent(canvasGO.transform, false);
        var dimRT = dim.AddComponent<RectTransform>();
        dimRT.anchorMin = Vector2.zero; dimRT.anchorMax = Vector2.one;
        dimRT.offsetMin = dimRT.offsetMax = Vector2.zero;
        var dimImg = dim.AddComponent<Image>();
        if (bgFullscreenSprite != null)
        {
            dimImg.sprite        = bgFullscreenSprite;
            dimImg.preserveAspect = bgFullscreenPreserveAspect;
            dimImg.color         = Color.white;
        }
        else
        {
            dimImg.color = new Color(0.035f, 0.055f, 0.12f, 1f); // deep navy, opaque
        }
        dimImg.raycastTarget = true; // blokir input di belakang

        // Lapisan 2: glow lembut di tengah (di belakang panel) untuk kedalaman.
        var glow = new GameObject("GlowTengah");
        glow.transform.SetParent(canvasGO.transform, false);
        var glowRT = glow.AddComponent<RectTransform>();
        glowRT.anchorMin = new Vector2(0.5f, 0.5f);
        glowRT.anchorMax = new Vector2(0.5f, 0.5f);
        glowRT.pivot     = new Vector2(0.5f, 0.5f);
        glowRT.sizeDelta = new Vector2(1400f, 980f);
        glowRT.anchoredPosition = Vector2.zero;
        var glowImg = glow.AddComponent<Image>();
        glowImg.sprite        = GetRoundedSprite();
        glowImg.type          = Image.Type.Sliced;
        glowImg.color         = new Color(0.16f, 0.34f, 0.70f, 0.18f); // biru lembut
        glowImg.raycastTarget = false;

        // Panel utama (ukuran ~ 50% × 60% layar)
        var panel = new GameObject("Panel");
        panel.transform.SetParent(canvasGO.transform, false);
        var prt = panel.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.5f, 0.5f);
        prt.anchorMax = new Vector2(0.5f, 0.5f);
        prt.pivot     = new Vector2(0.5f, 0.5f);
        prt.sizeDelta = new Vector2(900f, 560f);
        prt.anchoredPosition = Vector2.zero;
        var pImg = panel.AddComponent<Image>();
        pImg.color = tutorialPanelWarna;
        var outl = panel.AddComponent<Outline>();
        outl.effectColor    = tutorialBorderWarna;
        outl.effectDistance = new Vector2(3f, -3f);

        // Judul
        var judul = BuatTeks(panel.transform, "Judul", tutorialJudul,
            tutorialUkuranJudul, tutorialJudulWarna, FontStyles.Bold);
        judul.alignment = TextAlignmentOptions.Center;
        var jrt = judul.rectTransform;
        jrt.anchorMin = new Vector2(0f, 1f);
        jrt.anchorMax = new Vector2(1f, 1f);
        jrt.pivot     = new Vector2(0.5f, 1f);
        jrt.sizeDelta = new Vector2(0f, 80f);
        jrt.anchoredPosition = new Vector2(0f, -30f);

        // Body
        var body = BuatTeks(panel.transform, "Body", tutorialBody,
            tutorialUkuranBody, tutorialBodyWarna, FontStyles.Normal);
        body.alignment        = TextAlignmentOptions.Center;
        body.textWrappingMode = TMPro.TextWrappingModes.Normal;
        body.lineSpacing      = 6f;
        var brt = body.rectTransform;
        brt.anchorMin = new Vector2(0.05f, 0.20f);
        brt.anchorMax = new Vector2(0.95f, 0.82f);
        brt.offsetMin = brt.offsetMax = Vector2.zero;

        // Tombol "▶ SIAP, MULAI!"
        var btnGO = new GameObject("BtnSiap");
        btnGO.transform.SetParent(panel.transform, false);
        var btnRT = btnGO.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.5f, 0f);
        btnRT.anchorMax = new Vector2(0.5f, 0f);
        btnRT.pivot     = new Vector2(0.5f, 0f);
        btnRT.sizeDelta = new Vector2(380f, 70f);
        btnRT.anchoredPosition = new Vector2(0f, 38f);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = tutorialTombolBg;
        var btnOutl = btnGO.AddComponent<Outline>();
        btnOutl.effectColor    = tutorialTombolBorder;
        btnOutl.effectDistance = new Vector2(2f, -2f);
        var btn = btnGO.AddComponent<Button>();
        btn.transition    = Selectable.Transition.ColorTint;
        btn.targetGraphic = btnImg;
        var cb = btn.colors;
        cb.normalColor      = tutorialTombolBg;
        cb.highlightedColor = new Color(tutorialTombolBg.r * 1.2f, tutorialTombolBg.g * 1.2f, tutorialTombolBg.b * 1.2f, tutorialTombolBg.a);
        cb.pressedColor     = new Color(tutorialTombolBg.r * 0.7f, tutorialTombolBg.g * 0.7f, tutorialTombolBg.b * 0.7f, tutorialTombolBg.a);
        btn.colors = cb;

        var btnLbl = BuatTeks(btnGO.transform, "Label", tutorialTombol,
            tutorialUkuranTombol, tutorialTombolTeks, FontStyles.Bold);
        btnLbl.alignment = TextAlignmentOptions.Center;
        var brtl = btnLbl.rectTransform;
        brtl.anchorMin = Vector2.zero; brtl.anchorMax = Vector2.one;
        brtl.offsetMin = brtl.offsetMax = Vector2.zero;

        btn.onClick.AddListener(() =>
        {
            AudioManager.Instance?.Click();
            onSiap?.Invoke();
        });

        return canvasGO;
    }

    // ══════════════════════════════════════════════════════════════════════
    void BuildScene()
    {
        _canvasGO = new GameObject("ZonaTubuhQuiz_Canvas");
        _canvasComp = _canvasGO.AddComponent<Canvas>();
        _canvasComp.renderMode  = RenderMode.ScreenSpaceOverlay;
        _canvasComp.sortingOrder = sortingOrder;
        var scaler = _canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        _canvasGO.AddComponent<GraphicRaycaster>();

        // Latar belakang HITAM penuh layar (paling belakang) supaya scene di
        // belakang tidak tembus ke layar quiz. Selalu ada, walau sprite kosong.
        var blackBg = new GameObject("BG_Hitam");
        blackBg.transform.SetParent(_canvasGO.transform, false);
        var blackImg = blackBg.AddComponent<Image>();
        blackImg.color        = Color.black;
        blackImg.raycastTarget = false;
        var blackRt = blackBg.GetComponent<RectTransform>();
        blackRt.anchorMin = Vector2.zero; blackRt.anchorMax = Vector2.one;
        blackRt.offsetMin = Vector2.zero; blackRt.offsetMax = Vector2.zero;

        // BG Fullscreen device (opsional, paling belakang).
        if (bgFullscreenSprite != null)
        {
            var fs = new GameObject("BG_Fullscreen");
            fs.transform.SetParent(_canvasGO.transform, false);
            var fsImg = fs.AddComponent<Image>();
            fsImg.sprite         = bgFullscreenSprite;
            fsImg.preserveAspect = bgFullscreenPreserveAspect;
            fsImg.raycastTarget  = false;
            var fsRt = fs.GetComponent<RectTransform>();
            fsRt.anchorMin = Vector2.zero; fsRt.anchorMax = Vector2.one;
            fsRt.offsetMin = Vector2.zero; fsRt.offsetMax = Vector2.zero;
        }

        // Pastikan ada EventSystem (untuk drag)
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // ── Panel TENGAH (arena) — bingkai gelap untuk mengelompokkan judul,
        //    info (timer/skor), kartu, & instruksi supaya rapi + terbaca di atas
        //    latar pasar yang ramai. Berada di antara dua zona samping, DI BAWAH
        //    HUD persisten (skor/nyawa/nav/voice) supaya tidak bertabrakan.
        var arenaGO = new GameObject("ArenaTengah");
        arenaGO.transform.SetParent(_canvasGO.transform, false);
        var arenaImg = arenaGO.AddComponent<Image>();
        arenaImg.sprite = GetRoundedSprite();
        arenaImg.type   = Image.Type.Sliced;
        arenaImg.color  = new Color(0.06f, 0.05f, 0.08f, 0.86f);
        arenaImg.raycastTarget = false;
        var arenaOutl = arenaGO.AddComponent<Outline>();
        arenaOutl.effectColor    = new Color(0.95f, 0.72f, 0.18f, 0.85f);
        arenaOutl.effectDistance = new Vector2(3f, -3f);
        var arenaRT = arenaGO.GetComponent<RectTransform>();
        arenaRT.anchorMin = new Vector2(0.225f, 0.085f);
        arenaRT.anchorMax = new Vector2(0.775f, 0.80f);
        arenaRT.offsetMin = Vector2.zero; arenaRT.offsetMax = Vector2.zero;

        // Pita judul (header) di puncak arena
        var headBar = new GameObject("HeaderBar");
        headBar.transform.SetParent(arenaGO.transform, false);
        var headImg = headBar.AddComponent<Image>();
        headImg.sprite = GetRoundedSprite();
        headImg.type   = Image.Type.Sliced;
        headImg.color  = new Color(0.16f, 0.10f, 0.05f, 0.95f);
        headImg.raycastTarget = false;
        var headOutl = headBar.AddComponent<Outline>();
        headOutl.effectColor    = new Color(0.95f, 0.72f, 0.18f, 0.7f);
        headOutl.effectDistance = new Vector2(2f, -2f);
        var headRT = headBar.GetComponent<RectTransform>();
        headRT.anchorMin = new Vector2(0f, 1f); headRT.anchorMax = new Vector2(1f, 1f);
        headRT.pivot = new Vector2(0.5f, 1f);
        headRT.offsetMin = new Vector2(14f, -64f);
        headRT.offsetMax = new Vector2(-14f, -10f);

        // Judul
        var judul = BuatTeks(headBar.transform, "Judul", judulTeks, judulUkuran, judulWarna, FontStyles.Bold);
        judul.alignment = TextAlignmentOptions.Center;
        judul.enableAutoSizing = true; judul.fontSizeMin = 16f; judul.fontSizeMax = judulUkuran;
        var jrt = judul.rectTransform;
        jrt.anchorMin = Vector2.zero; jrt.anchorMax = Vector2.one;
        jrt.offsetMin = new Vector2(12f, 4f); jrt.offsetMax = new Vector2(-12f, -4f);

        // Baris info: Skor (kiri) + Timer (kanan), tepat di bawah header
        _skorText = BuatTeks(arenaGO.transform, "Skor", "Benar: 0/" + chips.Length, 22, new Color(1f, 1f, 0.92f, 1f), FontStyles.Bold);
        _skorText.alignment = TextAlignmentOptions.MidlineLeft;
        var srt = _skorText.rectTransform;
        srt.anchorMin = new Vector2(0f, 1f); srt.anchorMax = new Vector2(0.5f, 1f);
        srt.pivot = new Vector2(0f, 1f);
        srt.offsetMin = new Vector2(22f, -104f);
        srt.offsetMax = new Vector2(-6f, -68f);

        _timerText = BuatTeks(arenaGO.transform, "Timer", "00:15", ukuranTimer, warnaTimer, FontStyles.Bold);
        _timerText.alignment = TextAlignmentOptions.MidlineRight;
        var trt = _timerText.rectTransform;
        trt.anchorMin = new Vector2(0.5f, 1f); trt.anchorMax = new Vector2(1f, 1f);
        trt.pivot = new Vector2(1f, 1f);
        trt.offsetMin = new Vector2(6f, -104f);
        trt.offsetMax = new Vector2(-22f, -68f);

        // Instruksi (hint seret) — di dasar arena
        var instr = BuatTeks(arenaGO.transform, "Instruksi", instruksiTeks, instruksiUkuran, instruksiWarna, FontStyles.Italic);
        instr.alignment = TextAlignmentOptions.Center;
        var irt = instr.rectTransform;
        irt.anchorMin = new Vector2(0f, 0f); irt.anchorMax = new Vector2(1f, 0f);
        irt.pivot = new Vector2(0.5f, 0f);
        irt.offsetMin = new Vector2(16f, 14f);
        irt.offsetMax = new Vector2(-16f, 54f);

        // Zona KIRI (AMAN) & KANAN (BAHAYA) — strip tinggi di tepi layar
        _zonaAmanRT   = BuatZona("ZONA_AMAN",   zonaAmanLabel,   zonaAmanSubtitle,   warnaZonaAman,   warnaBorderAman,   zonaAmanAnchorMin,   zonaAmanAnchorMax,   out _zonaAmanContent);
        _zonaBahayaRT = BuatZona("ZONA_BAHAYA", zonaBahayaLabel, zonaBahayaSubtitle, warnaZonaBahaya, warnaBorderBahaya, zonaBahayaAnchorMin, zonaBahayaAnchorMax, out _zonaBahayaContent);

        // Fallback KLIK: klik zona untuk menempatkan chip yang sedang dipilih.
        TambahKlikZona(_zonaAmanRT,   "AMAN");
        TambahKlikZona(_zonaBahayaRT, "BAHAYA");

        // Karakter ilustrasi di tengah (opsional, di belakang label)
        if (karakterSprite != null)
        {
            var karGO = new GameObject("Karakter");
            karGO.transform.SetParent(_canvasGO.transform, false);
            var karImg = karGO.AddComponent<Image>();
            karImg.sprite         = karakterSprite;
            karImg.preserveAspect = true;
            karImg.raycastTarget  = false;
            var karRT = karGO.GetComponent<RectTransform>();
            karRT.anchorMin = new Vector2(0.5f, 0.5f); karRT.anchorMax = new Vector2(0.5f, 0.5f);
            karRT.pivot = new Vector2(0.5f, 0.5f);
            karRT.sizeDelta = karakterUkuran;
            karRT.anchoredPosition = karakterAnchoredPos;
        }

        // Container label di TENGAH arena — grid 2 kolom, di antara header & instruksi.
        var chipArea = new GameObject("ChipArea");
        chipArea.transform.SetParent(arenaGO.transform, false);
        var caRT = chipArea.AddComponent<RectTransform>();
        caRT.anchorMin = new Vector2(0.5f, 0.5f); caRT.anchorMax = new Vector2(0.5f, 0.5f);
        caRT.pivot = new Vector2(0.5f, 0.5f);

        int kolom = 2;
        int baris = Mathf.CeilToInt(chips.Length / (float)kolom);
        float celahKolom  = 28f;
        float spasiBaris  = 18f;
        float lebarTotal  = kolom * chipUkuran.x + celahKolom;
        float tinggiTotal = baris * chipUkuran.y + (baris - 1) * spasiBaris;
        caRT.sizeDelta = new Vector2(lebarTotal, tinggiTotal);
        caRT.anchoredPosition = new Vector2(0f, -26f);

        var grid = chipArea.AddComponent<GridLayoutGroup>();
        grid.cellSize = chipUkuran;
        grid.spacing  = new Vector2(celahKolom, spasiBaris);
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = kolom;

        foreach (var c in chips)
        {
            var go = BuatChip(c, chipArea.transform);
            _chipPool.Add(go);
        }
    }

    RectTransform BuatZona(string name, string label, string subtitle, Color bg, Color border, Vector2 anchorMin, Vector2 anchorMax, out Transform content)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_canvasGO.transform, false);
        var img = go.AddComponent<Image>();
        img.sprite = GetRoundedSprite();
        img.color  = bg;
        img.type   = Image.Type.Sliced;
        var outl = go.AddComponent<Outline>();
        outl.effectColor    = border;
        outl.effectDistance = new Vector2(3f, -3f);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        // Judul zona (atas)
        var lab = BuatTeks(go.transform, "Label", label, 26, Color.white, FontStyles.Bold);
        lab.alignment = TextAlignmentOptions.Center;
        var lrt = lab.rectTransform;
        lrt.anchorMin = new Vector2(0f, 1f); lrt.anchorMax = new Vector2(1f, 1f);
        lrt.pivot = new Vector2(0.5f, 1f);
        lrt.offsetMin = new Vector2(8f, -56f);
        lrt.offsetMax = new Vector2(-8f, -12f);

        // Subjudul zona (di bawah judul)
        if (!string.IsNullOrEmpty(subtitle))
        {
            var sub = BuatTeks(go.transform, "Subtitle", subtitle, zonaSubtitleUkuran, new Color(1f, 1f, 1f, 0.82f), FontStyles.Italic);
            sub.alignment = TextAlignmentOptions.Center;
            var subrt = sub.rectTransform;
            subrt.anchorMin = new Vector2(0f, 1f); subrt.anchorMax = new Vector2(1f, 1f);
            subrt.pivot = new Vector2(0.5f, 1f);
            subrt.offsetMin = new Vector2(8f, -120f);
            subrt.offsetMax = new Vector2(-8f, -58f);
        }

        // Wadah tempat chip MASUK & menumpuk (di bawah judul/subjudul)
        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(go.transform, false);
        var cRT = contentGO.AddComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0f, 0f); cRT.anchorMax = new Vector2(1f, 1f);
        cRT.offsetMin = new Vector2(14f, 14f);
        cRT.offsetMax = new Vector2(-14f, -130f); // sisakan ruang utk judul + subjudul
        var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 12f;
        vlg.padding = new RectOffset(6, 6, 6, 6);
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;  vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
        content = contentGO.transform;

        return rt;
    }

    // Pasang Button pada zona supaya bisa diklik untuk menempatkan chip terpilih.
    void TambahKlikZona(RectTransform zona, string jawabanPemain)
    {
        if (zona == null) return;
        var btn = zona.gameObject.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(() => TempatkanKeZona(jawabanPemain));
    }

    GameObject BuatChip(ChipData data, Transform parent)
    {
        var go = new GameObject("Chip_" + data.teks);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.sprite = GetRoundedSprite();
        img.color  = chipWarna;
        img.type   = Image.Type.Sliced;
        var outl = go.AddComponent<Outline>();
        outl.effectColor    = new Color(1f, 1f, 1f, 0.35f);
        outl.effectDistance = new Vector2(1f, -1f);

        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = chipUkuran.x; le.preferredHeight = chipUkuran.y;

        var teks = BuatTeks(go.transform, "Label", data.teks, chipUkuranTeks, chipTeksWarna, FontStyles.Bold);
        teks.alignment = TextAlignmentOptions.Center;
        var trt = teks.rectTransform;
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(8f, 4f); trt.offsetMax = new Vector2(-8f, -4f);

        // Drag handler
        var drag = go.AddComponent<DraggableChip>();
        drag.canvas    = _canvasComp;
        drag.quiz      = this;
        drag.data      = data;
        drag.chipImage = img;

        return go;
    }

    // ══════════════════════════════════════════════════════════════════════
    IEnumerator TimerCoroutine()
    {
        _sisaWaktu = waktuDetik;
        while (_sisaWaktu > 0f && !_quizSelesai)
        {
            _sisaWaktu -= Time.deltaTime;
            int s = Mathf.CeilToInt(_sisaWaktu);
            _timerText.text = "\u23F1 " + (s < 10 ? "00:0" + s : "00:" + s);
            _timerText.color = s <= 5 ? warnaTimerKritis : warnaTimer;
            yield return null;
        }
        if (!_quizSelesai) SelesaikanQuiz();
    }

    // Dipanggil oleh DraggableChip saat drop selesai
    public bool CekDrop(DraggableChip chip, Vector2 screenPos)
    {
        if (_quizSelesai || chip == null) return false;

        bool diZonaAman   = RectTransformUtility.RectangleContainsScreenPoint(_zonaAmanRT,   screenPos);
        bool diZonaBahaya = RectTransformUtility.RectangleContainsScreenPoint(_zonaBahayaRT, screenPos);

        if (!diZonaAman && !diZonaBahaya) return false;

        string jawabanPemain = diZonaAman ? "AMAN" : "BAHAYA";
        LabuhkanChip(chip, jawabanPemain);
        return true;
    }

    // Masukkan chip ke DALAM kotak zona (reparent + menumpuk) lalu proses skor.
    void LabuhkanChip(DraggableChip chip, string jawabanPemain)
    {
        Transform wadah = jawabanPemain == "AMAN" ? _zonaAmanContent : _zonaBahayaContent;
        bool benar = chip.data != null && chip.data.jawabanBenar == jawabanPemain;
        chip.Labuh(wadah, benar ? chipBenarWarna : chipSalahWarna);
        ProsesJawaban(chip.data, jawabanPemain);
    }

    // ── FALLBACK KLIK (tap chip → tap zona) ────────────────────────────────
    // Dipanggil DraggableChip saat chip di-KLIK (bukan di-drag). Menandai chip
    // sebagai "terpilih". Klik lagi = batal pilih.
    public void PilihChip(DraggableChip chip)
    {
        if (_quizSelesai || chip == null) return;
        if (_chipTerpilih == chip) { chip.SetTerpilih(false); _chipTerpilih = null; return; }
        if (_chipTerpilih != null) _chipTerpilih.SetTerpilih(false);
        _chipTerpilih = chip;
        chip.SetTerpilih(true);
        AudioManager.Instance?.Click();
    }

    // Dipanggil saat ZONA diklik. Menempatkan chip terpilih ke zona tsb.
    public void TempatkanKeZona(string jawabanPemain)
    {
        if (_quizSelesai || _chipTerpilih == null) return;
        var chip = _chipTerpilih;
        _chipTerpilih = null;
        chip.SetTerpilih(false);
        LabuhkanChip(chip, jawabanPemain);
    }

    // Logika skor & feedback bersama untuk drag-drop maupun klik.
    void ProsesJawaban(ChipData data, string jawabanPemain)
    {
        bool benar = jawabanPemain == data.jawabanBenar;

        _chipDitempatkan++;
        if (benar) _chipBenar++;
        _skorText.text = $"Benar: {_chipBenar}/{chips.Length}";

        // SFX
        var am = AudioManager.Instance;
        if (am != null && am.sfxSource != null)
        {
            if (benar && am.sfxCorrect != null) am.sfxSource.PlayOneShot(am.sfxCorrect);
            else if (!benar && am.sfxWrong != null) am.sfxSource.PlayOneShot(am.sfxWrong);
        }

        // Score
        var gs = GameState.Instance;
        if (gs != null)
        {
            int pts = benar ? (GameState.SCORE_QUIZ / 2) : 0; // 100 poin per chip benar
            gs.score += pts;
            gs.AddChoice(2, $"Quiz: {data.teks} \u2192 {jawabanPemain}", benar ? "AMAN" : "BAHAYA", pts);
        }

        // Feedback edukatif: tampilkan ALASAN singkat kenapa benar/salah.
        TampilkanFeedbackChip(benar, data, jawabanPemain);

        if (_chipDitempatkan >= chips.Length) SelesaikanQuiz();
    }

    // ── Toast feedback per-chip (edukatif) ─────────────────────────────────
    // Muncul sebentar di tengah-bawah layar: ✓ benar / ✗ salah + alasan singkat.
    private GameObject _feedbackToast;
    void TampilkanFeedbackChip(bool benar, ChipData data, string jawabanPemain)
    {
        if (_canvasGO == null) return;

        // Hanya satu toast aktif — ganti yang lama.
        if (_feedbackToast != null) Destroy(_feedbackToast);

        var toast = new GameObject("FeedbackToast");
        toast.transform.SetParent(_canvasGO.transform, false);
        var img = toast.AddComponent<Image>();
        img.sprite = GetRoundedSprite();
        img.type   = Image.Type.Sliced;
        img.color  = benar ? new Color(0.10f, 0.32f, 0.18f, 0.97f)
                           : new Color(0.34f, 0.10f, 0.10f, 0.97f);
        img.raycastTarget = false;
        var outl = toast.AddComponent<Outline>();
        outl.effectColor    = benar ? new Color(0.40f, 0.92f, 0.55f, 1f)
                                    : new Color(0.95f, 0.45f, 0.45f, 1f);
        outl.effectDistance = new Vector2(2f, -2f);
        var rt = toast.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f); rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(720f, 120f);
        rt.anchoredPosition = new Vector2(0f, 130f);

        string judul = benar ? "\u2713 TEPAT!" : "\u2716 KURANG TEPAT";
        string alasan = AlasanChip(data);
        var tmp = BuatTeks(toast.transform, "Teks",
            $"<b>{judul}</b>\n<size=85%>{alasan}</size>",
            22, new Color(1f, 1f, 0.95f, 1f), FontStyles.Normal);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = true; tmp.fontSizeMin = 15; tmp.fontSizeMax = 23;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        var trt = tmp.rectTransform;
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(22f, 12f); trt.offsetMax = new Vector2(-22f, -12f);

        _feedbackToast = toast;
        StartCoroutine(AnimasiFeedbackToast(toast, rt));
    }

    IEnumerator AnimasiFeedbackToast(GameObject toast, RectTransform rt)
    {
        if (toast == null) yield break;
        // Pop masuk
        float t = 0f;
        while (t < 0.2f && toast != null)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / 0.2f);
            float s = p < 0.7f ? Mathf.Lerp(0.85f, 1.05f, p / 0.7f)
                               : Mathf.Lerp(1.05f, 1f, (p - 0.7f) / 0.3f);
            if (rt != null) rt.localScale = Vector3.one * s;
            yield return null;
        }
        if (rt != null) rt.localScale = Vector3.one;
        // Tahan
        yield return new WaitForSeconds(2.2f);
        // Fade keluar
        var img = toast != null ? toast.GetComponent<Image>() : null;
        var tmps = toast != null ? toast.GetComponentsInChildren<TextMeshProUGUI>() : null;
        var ol   = toast != null ? toast.GetComponent<Outline>() : null;
        float f = 0f;
        while (f < 0.4f && toast != null)
        {
            f += Time.deltaTime;
            float a = 1f - Mathf.Clamp01(f / 0.4f);
            if (img != null) { var c = img.color; c.a = 0.97f * a; img.color = c; }
            if (ol  != null) { var c = ol.effectColor; c.a = a; ol.effectColor = c; }
            if (tmps != null) foreach (var x in tmps) { if (x == null) continue; var c = x.color; c.a = a; x.color = c; }
            yield return null;
        }
        if (toast != null) { if (_feedbackToast == toast) _feedbackToast = null; Destroy(toast); }
    }

    // Alasan singkat per bagian tubuh. Pakai override Inspector jika diisi,
    // selain itu cari berdasarkan kata kunci nama chip, lalu fallback umum.
    string AlasanChip(ChipData data)
    {
        if (data == null) return "";
        if (!string.IsNullOrWhiteSpace(data.alasan)) return data.alasan;

        string t = data.teks != null ? data.teks.ToLowerInvariant() : "";
        if (t.Contains("bahu"))   return "Bahu boleh disentuh teman/keluarga dengan sopan.";
        if (t.Contains("tangan")) return "Tangan boleh untuk bersalaman atau menyapa.";
        if (t.Contains("pipi"))   return "Pipi boleh dari keluarga dekat, asal kamu nyaman.";
        if (t.Contains("paha"))   return "Paha termasuk area pribadi \u2014 tidak boleh disentuh orang lain.";
        if (t.Contains("perut"))  return "Perut termasuk area pribadi \u2014 katakan TIDAK bila disentuh.";
        if (t.Contains("privat") || t.Contains("kelamin") || t.Contains("dada"))
            return "Ini area sangat pribadi (tertutup baju renang) \u2014 dilarang disentuh siapa pun.";

        // Fallback berdasarkan jawaban benar.
        return data.jawabanBenar == "AMAN"
            ? "Bagian ini umumnya aman disentuh dengan sopan dan seizinmu."
            : "Bagian ini area pribadi \u2014 kamu berhak menolak bila disentuh.";
    }


    void SelesaikanQuiz()
    {
        if (_quizSelesai) return;
        _quizSelesai = true;
        StopAllCoroutines();

        var gs = GameState.Instance;
        bool semuaBenar = _chipBenar == chips.Length;
        if (semuaBenar && gs != null)
        {
            gs.score += bonusAllBenar;
            if (!gs.achievements.Contains(namaAchievement))
            {
                gs.achievements.Add(namaAchievement);
                AchievementPopup.Show(namaAchievement);
            }
            Debug.Log($"[ZonaTubuhQuiz] PERFECT! Bonus +{bonusAllBenar} + achievement.");
        }

        BuildLayarHasil(semuaBenar);
    }

    void BuildLayarHasil(bool semuaBenar)
    {
        // Tombol Lanjut
        var btnGO = new GameObject("LanjutBtn");
        btnGO.transform.SetParent(_canvasGO.transform, false);
        var img = btnGO.AddComponent<Image>();
        img.sprite = GetRoundedSprite();
        img.color  = warnaLanjut;
        img.type   = Image.Type.Sliced;
        var outl = btnGO.AddComponent<Outline>();
        outl.effectColor    = Color.white;
        outl.effectDistance = new Vector2(2f, -2f);
        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f); rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(360f, 70f);
        rt.anchoredPosition = new Vector2(0f, 30f);

        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() =>
        {
            AudioManager.Instance?.Click();
            if (narasiOutro != null && narasiOutro.Length > 0)
            {
                // Jangan langsung selesai — tampilkan narasi jembatan ke Lapor
                // ("Rara masih di angkot, pria merapat lagi..."). Tombol di-disable
                // dulu supaya tidak bisa di-spam-klik.
                btn.interactable = false;
                StartCoroutine(JalankanNarasiOutroLaluSelesai());
            }
            else
            {
                if (_canvasGO != null) Destroy(_canvasGO);
                KembalikanUiPersisten();
                _onSelesai?.Invoke();
            }
        });

        string label = semuaBenar
            ? $"\uD83C\uDFC6  Perfect! +{bonusAllBenar} bonus"
            : $"\u25B6  Lanjut  ({_chipBenar}/{chips.Length} benar)";

        var lab = BuatTeks(btnGO.transform, "Label", label, 22, Color.white, FontStyles.Bold);
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

// ──────────────────────────────────────────────────────────────────────────
// Helper drag component (di file yang sama supaya nggak nambah file kecil).
// ──────────────────────────────────────────────────────────────────────────
public class DraggableChip : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Canvas canvas;
    public ZonaTubuhQuiz quiz;
    public ZonaTubuhQuiz.ChipData data;
    public Image chipImage;

    private RectTransform _rt;
    private CanvasGroup _cg;
    private Outline _outline;
    private Vector2 _posAwal;
    private Transform _parentAwal;
    private bool _ditempatkan;
    private bool _sedangDrag;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _cg = gameObject.AddComponent<CanvasGroup>();
        _outline = GetComponent<Outline>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_ditempatkan) return;
        _sedangDrag = true;
        _posAwal = _rt.anchoredPosition;
        _parentAwal = transform.parent;
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();
        _cg.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_ditempatkan) return;
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform, eventData.position, canvas.worldCamera, out local);
        _rt.anchoredPosition = local;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _sedangDrag = false;
        _cg.blocksRaycasts = true;
        // CekDrop akan memasukkan chip ke dalam kotak zona bila diterima.
        bool accepted = quiz.CekDrop(this, eventData.position);
        if (!accepted)
        {
            // Kembali ke posisi awal
            transform.SetParent(_parentAwal, false);
            _rt.anchoredPosition = _posAwal;
        }
    }

    // KLIK (tanpa drag) = pilih/batal-pilih chip ini.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_ditempatkan || _sedangDrag || eventData.dragging) return;
        quiz?.PilihChip(this);
    }

    // Tandai chip sebagai terpilih (highlight outline kuning).
    public void SetTerpilih(bool on)
    {
        if (_outline == null) return;
        _outline.effectColor    = on ? new Color(1f, 0.88f, 0.2f, 1f) : new Color(1f, 1f, 1f, 0.35f);
        _outline.effectDistance = on ? new Vector2(3f, -3f) : new Vector2(1f, -1f);
    }

    // Labuhkan chip ke DALAM kotak zona (dipakai oleh drag ATAU klik):
    // pindahkan chip menjadi anak wadah zona supaya menumpuk & tetap di sana.
    public void Labuh(Transform wadahZona, Color warnaAkhir)
    {
        _ditempatkan = true;
        SetTerpilih(false);
        _cg.interactable   = false;
        _cg.blocksRaycasts = false;
        if (wadahZona != null)
        {
            transform.SetParent(wadahZona, false);
            _rt.anchoredPosition = Vector2.zero;
            _rt.localScale = Vector3.one;
        }
        if (chipImage != null)
            chipImage.color = warnaAkhir;
    }
}

