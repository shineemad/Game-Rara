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
    }

    [Header("Judul & Instruksi")]
    public string judulTeks = "\uD83D\uDEE1  Quiz: Mana yang BOLEH, mana yang TIDAK BOLEH?";
    public Color  judulWarna = new Color(1f, 0.85f, 0.3f, 1f);
    public int    judulUkuran = 30;
    [TextArea(2, 3)]
    public string instruksiTeks = "Tarik setiap chip ke ZONA AMAN atau ZONA BAHAYA.\nSetiap jawaban benar = poin. Waktu terbatas!";
    public Color  instruksiWarna = new Color(1f, 1f, 0.92f, 0.85f);
    public int    instruksiUkuran = 18;

    [Header("Timer")]
    public float waktuDetik = 15f;
    public Color warnaTimer = new Color(1f, 0.85f, 0.3f, 1f);
    public Color warnaTimerKritis = new Color(0.91f, 0.30f, 0.24f, 1f);
    public int   ukuranTimer = 28;

    [Header("Daftar Chip (CUSTOMIZABLE)")]
    public ChipData[] chips = new ChipData[]
    {
        new ChipData { teks = "Salam jabat tangan", jawabanBenar = "AMAN" },
        new ChipData { teks = "Peluk ortu/saudara",  jawabanBenar = "AMAN" },
        new ChipData { teks = "Cek up dokter (didampingi)", jawabanBenar = "AMAN" },
        new ChipData { teks = "Disentuh paksa orang asing", jawabanBenar = "BAHAYA" },
        new ChipData { teks = "Diminta lepas baju oleh orang asing", jawabanBenar = "BAHAYA" },
        new ChipData { teks = "Disuruh simpan rahasia 'pertemuan kita'", jawabanBenar = "BAHAYA" }
    };

    [Header("Warna Zona")]
    public Color warnaZonaAman   = new Color(0.10f, 0.35f, 0.22f, 0.92f);
    public Color warnaZonaBahaya = new Color(0.40f, 0.12f, 0.12f, 0.92f);
    public Color warnaBorderAman = new Color(0.45f, 1f, 0.65f, 1f);
    public Color warnaBorderBahaya = new Color(1f, 0.45f, 0.45f, 1f);

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
    }

    [Header("Narasi Intro (sebelum quiz) — sambungan setelah AngkotSeatPicker")]
    [Tooltip("FALLBACK narasi intro. Dipakai HANYA jika:\n" +
             "  \u2022 GameState.seatCategory kosong, ATAU\n" +
             "  \u2022 Varian narasi spesifik di bawah (Aman/Ragu/Bahaya) kosong.\n" +
             "Kalau pemain sudah pilih kursi di AngkotSeatPicker, sistem akan otomatis\n" +
             "memilih narasiIntroAman / narasiIntroRagu / narasiIntroBahaya yang relevan.")]
    public BarisNarasiQuiz[] narasiIntro = new BarisNarasiQuiz[]
    {
        new BarisNarasiQuiz { pembicara = "Narasi",
            teks = "Pintu angkot ditutup. Mesin menderu pelan, lalu mulai melaju membelah jalanan pagi." },
        new BarisNarasiQuiz { pembicara = "Narasi",
            teks = "Untuk mengisi waktu, Rara mengeluarkan buku catatan PR Kesehatan dari tas. Bab terakhir: \u201CKenali Batas Tubuhmu\u201D." }
    };

    [Header("Narasi Intro — Varian per Kursi (otomatis dipilih dari GameState.seatCategory)")]
    [Tooltip("Narasi kalau pemain memilih kursi AMAN (Dekat Pintu / dekat supir).\n" +
             "Tone: tenang, lega, percaya diri. Kosong = pakai 'narasiIntro' default.")]
    public BarisNarasiQuiz[] narasiIntroAman = new BarisNarasiQuiz[]
    {
        new BarisNarasiQuiz { pembicara = "Narasi",
            teks = "Pintu angkot ditutup. Rara duduk di kursi paling depan, tepat di samping pak supir \u2014 posisi paling aman, mudah dilihat semua orang." },
        new BarisNarasiQuiz { pembicara = "Rara",
            teks = "\"Alhamdulillah\u2026 \uD83D\uDE0C dari sini aku bisa lihat semua penumpang yang naik. Pria asing tadi juga nggak ikut. Aku tenang sekarang.\"" },
        new BarisNarasiQuiz { pembicara = "Narasi",
            teks = "Karena perjalanan masih jauh, Rara mengeluarkan buku catatan PR Kesehatan. Bab terakhir: \u201CKenali Batas Tubuhmu \u2014 Mana yang Boleh, Mana yang Tidak\u201D." },
        new BarisNarasiQuiz { pembicara = "Rara",
            teks = "\"Besok ulangan bab ini. Mumpung tenang, mending latihan dulu sekarang.\"" }
    };

    [Tooltip("Narasi kalau pemain memilih kursi RAGU (Tengah, terjepit ibu-ibu).\n" +
             "Tone: sedikit canggung tapi tetap ada saksi. Kosong = pakai 'narasiIntro' default.")]
    public BarisNarasiQuiz[] narasiIntroRagu = new BarisNarasiQuiz[]
    {
        new BarisNarasiQuiz { pembicara = "Narasi",
            teks = "Rara duduk di bangku tengah, terjepit di antara dua ibu-ibu yang sibuk menjaga keranjang belanjaan. Sesekali siku mereka menyenggol tas Rara." },
        new BarisNarasiQuiz { pembicara = "Rara",
            teks = "\"Hmm\u2026 \uD83D\uDE10 posisi ini agak susah kalau aku mau turun cepat. Tapi setidaknya banyak orang dewasa di sekitarku \u2014 nggak akan ada yang berani macam-macam.\"" },
        new BarisNarasiQuiz { pembicara = "Narasi",
            teks = "Untuk mengisi waktu sambil duduk diam, Rara mengeluarkan buku catatan PR Kesehatan dari tas. Bab terakhir: \u201CKenali Batas Tubuhmu\u201D." },
        new BarisNarasiQuiz { pembicara = "Rara",
            teks = "\"Besok ulangan bab ini. Mending dipelajari sekarang \u2014 biar nggak gugup besok.\"" }
    };

    [Tooltip("Narasi kalau pemain memilih kursi BAHAYA (Pojok Belakang, sepi).\n" +
             "Tone: tegang, takut, peringatan. Kosong = pakai 'narasiIntro' default.")]
    public BarisNarasiQuiz[] narasiIntroBahaya = new BarisNarasiQuiz[]
    {
        new BarisNarasiQuiz { pembicara = "Narasi",
            teks = "Rara duduk di pojok belakang. Lampu di area ini redup. Seorang pria asing duduk hanya sebangku darinya \u2014 dan terus melirik ke arahnya tanpa bicara." },
        new BarisNarasiQuiz { pembicara = "Rara",
            teks = "\"(Ya Allah\u2026 \uD83D\uDE28 kenapa aku pilih di sini. Jantungku dag-dig-dug. Aku nggak berani noleh ke samping.)\"" },
        new BarisNarasiQuiz { pembicara = "Narasi",
            teks = "Untuk mengalihkan rasa takutnya, Rara cepat-cepat mengeluarkan buku catatan PR Kesehatan dari tas." },
        new BarisNarasiQuiz { pembicara = "Rara",
            teks = "\"Bab \u2018Kenali Batas Tubuhmu\u2019\u2026 mungkin ini saatnya aku benar-benar paham \u2014 siapa yang BOLEH dan TIDAK BOLEH menyentuhku.\"" }
    };

    [Header("Narasi Outro (setelah quiz, sebelum ChatSim WhatsApp) — jembatan ke fase ChatSim")]
    [Tooltip("Baris narasi muncul SETELAH pemain klik tombol Lanjut di layar hasil quiz,\n" +
             "SEBELUM callback _onSelesai (yang memicu ChatSim WhatsApp).\n" +
             "Konteks: Rara turun di sekolah, masuk jam istirahat, lalu HP-nya bergetar \u2014\n" +
             "pesan dari nomor tak dikenal. Ini memberi context kenapa tiba-tiba muncul\n" +
             "WhatsApp dari 'Pria Asing Halte'.\n" +
             "Kosongkan = langsung ke ChatSim tanpa narasi outro.")]
    public BarisNarasiQuiz[] narasiOutro = new BarisNarasiQuiz[]
    {
        new BarisNarasiQuiz { pembicara = "Narasi",
            teks = "Angkot berhenti tepat di depan gerbang sekolah. Rara turun, mengangguk sopan ke sopir, lalu berjalan cepat menuju kelas. \u2014 Akhirnya sampai dengan selamat." },
        new BarisNarasiQuiz { pembicara = "Rara",
            teks = "\"Fyuh\u2026 \uD83D\uDE0C selamat. Pelajaran pertama hampir mulai \u2014 mending fokus dulu, mikir pria tadi nanti aja.\"" },
        new BarisNarasiQuiz { pembicara = "Narasi",
            teks = "Beberapa jam berlalu. Bel istirahat berbunyi. Rara duduk di kantin dengan sekotak susu \u2014 lalu HP di sakunya bergetar pelan: \uD83D\uDCF2 \u2026 \u2026" },
        new BarisNarasiQuiz { pembicara = "Rara",
            teks = "\"Hah? Pesan WhatsApp\u2026 dari nomor yang nggak aku simpan. \uD83D\uDE2C\nFoto profilnya kosong. Kok\u2026 dia bisa tau nomorku?\"" },
        new BarisNarasiQuiz { pembicara = "Narasi",
            teks = "Dengan tangan sedikit gemetar, Rara membuka pesan itu\u2026" }
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
    public string narasiTeksHint    = "\u25BC SPACE / Klik untuk lanjut";

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
        "Geser / drag nama bagian tubuh ke zona yang sesuai!\n\n" +
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
    private TextMeshProUGUI _timerText;
    private TextMeshProUGUI _skorText;
    private float      _sisaWaktu;
    private bool       _quizSelesai;
    private int        _chipDitempatkan;
    private int        _chipBenar;
    private List<GameObject> _chipPool = new List<GameObject>();
    private Sprite     _roundedSprite;
    private Canvas     _canvasComp;

    // State narasi intro (typewriter)
    private GameObject      _narasiCanvasGO;
    private TextMeshProUGUI _narasiNamaTMP;
    private TextMeshProUGUI _narasiTeksTMP;
    private TextMeshProUGUI _narasiHintTMP;
    private Image           _narasiPortraitImg;
    private bool _ketikSelesai;
    private bool _skipKetik;

    // ══════════════════════════════════════════════════════════════════════
    public void Mulai(Action onSelesai)
    {
        _onSelesai = onSelesai;
        AutoResolveNarasiAssets();
        var narasiAktif = PilihNarasiIntroBerdasarkanKursi();
        if (narasiAktif != null && narasiAktif.Length > 0)
            StartCoroutine(JalankanNarasiLaluQuiz(narasiAktif));
        else if (tampilkanTutorial)
            StartCoroutine(TampilkanTutorialLaluQuiz());
        else
            MulaiQuizLangsung();
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
        BuildScene();
        StartCoroutine(TimerCoroutine());
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
    // NARASI OUTRO — jembatan setelah quiz, sebelum ChatSim WhatsApp
    // Pemain klik "Lanjut" di layar hasil → narasi outro muncul (Rara sampai
    // sekolah, HP bergetar) → setelah selesai, _onSelesai() dipanggil
    // (memicu fase berikutnya di Day2Controller, biasanya ChatSim).
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
            if (_narasiNamaTMP != null) _narasiNamaTMP.text = (baris.pembicara ?? "").ToUpper();
            UpdateNarasiPortrait(baris.pembicara);
            yield return KetikTeksNarasi(baris.teks ?? "");
            yield return TungguTapNarasi();
        }
        if (_narasiCanvasGO != null) Destroy(_narasiCanvasGO);
        _narasiCanvasGO = null;

        _onSelesai?.Invoke();
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
                yield return new WaitForSeconds(kecepatanKetikNarasi);
            }
        }
        _ketikSelesai = true;
        if (delaySetelahKetikNarasi > 0f) yield return new WaitForSeconds(delaySetelahKetikNarasi);
        if (_narasiHintTMP != null) _narasiHintTMP.gameObject.SetActive(true);
    }

    IEnumerator TungguTapNarasi()
    {
        while (true)
        {
            bool ditekan = Input.GetMouseButtonDown(0)
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
        var bgImg = bg.AddComponent<Image>();
        if (bgSprite != null)
        {
            bgImg.sprite         = bgSprite;
            bgImg.preserveAspect = bgFullscreenPreserveAspect;
            bgImg.color          = Color.white;
        }
        else
        {
            bgImg.color = new Color(0f, 0f, 0f, 0.55f);
        }
        bgImg.raycastTarget = false;

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
        _narasiTeksTMP.overflowMode        = TextOverflowModes.Ellipsis;
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
            _narasiPortraitImg.sprite  = sp;
            _narasiPortraitImg.color   = Color.white;
            _narasiPortraitImg.enabled = true;
        }
        else
        {
            // Fallback: tampilkan kotak warna polos supaya tata letak tetap konsisten
            _narasiPortraitImg.sprite  = null;
            _narasiPortraitImg.color   = narasiPortraitFallbackWarna;
            _narasiPortraitImg.enabled = true;
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

        // Dim fullscreen
        var dim = new GameObject("Dim");
        dim.transform.SetParent(canvasGO.transform, false);
        var dimRT = dim.AddComponent<RectTransform>();
        dimRT.anchorMin = Vector2.zero; dimRT.anchorMax = Vector2.one;
        dimRT.offsetMin = dimRT.offsetMax = Vector2.zero;
        var dimImg = dim.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.75f);
        dimImg.raycastTarget = true; // blokir input di belakang

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

        // Judul
        var judul = BuatTeks(_canvasGO.transform, "Judul", judulTeks, judulUkuran, judulWarna, FontStyles.Bold);
        judul.alignment = TextAlignmentOptions.Center;
        var jrt = judul.rectTransform;
        jrt.anchorMin = new Vector2(0f, 1f); jrt.anchorMax = new Vector2(1f, 1f);
        jrt.pivot = new Vector2(0.5f, 1f);
        jrt.offsetMin = new Vector2(40f, -90f);
        jrt.offsetMax = new Vector2(-40f, -25f);

        // Instruksi
        var instr = BuatTeks(_canvasGO.transform, "Instruksi", instruksiTeks, instruksiUkuran, instruksiWarna, FontStyles.Italic);
        instr.alignment = TextAlignmentOptions.Center;
        var irt = instr.rectTransform;
        irt.anchorMin = new Vector2(0f, 1f); irt.anchorMax = new Vector2(1f, 1f);
        irt.pivot = new Vector2(0.5f, 1f);
        irt.offsetMin = new Vector2(40f, -160f);
        irt.offsetMax = new Vector2(-40f, -95f);

        // Timer + Skor (atas kanan & kiri)
        _timerText = BuatTeks(_canvasGO.transform, "Timer", "00:15", ukuranTimer, warnaTimer, FontStyles.Bold);
        _timerText.alignment = TextAlignmentOptions.MidlineRight;
        var trt = _timerText.rectTransform;
        trt.anchorMin = new Vector2(1f, 1f); trt.anchorMax = new Vector2(1f, 1f);
        trt.pivot = new Vector2(1f, 1f);
        trt.sizeDelta = new Vector2(220f, 50f);
        trt.anchoredPosition = new Vector2(-40f, -25f);

        _skorText = BuatTeks(_canvasGO.transform, "Skor", "Benar: 0/" + chips.Length, 24, new Color(1f, 1f, 0.92f, 1f), FontStyles.Bold);
        _skorText.alignment = TextAlignmentOptions.MidlineLeft;
        var srt = _skorText.rectTransform;
        srt.anchorMin = new Vector2(0f, 1f); srt.anchorMax = new Vector2(0f, 1f);
        srt.pivot = new Vector2(0f, 1f);
        srt.sizeDelta = new Vector2(280f, 50f);
        srt.anchoredPosition = new Vector2(40f, -25f);

        // Zona kiri (AMAN) + kanan (BAHAYA)
        _zonaAmanRT   = BuatZona("ZONA_AMAN",   "\u2713  ZONA AMAN",   warnaZonaAman,   warnaBorderAman,   new Vector2(-450f, -80f));
        _zonaBahayaRT = BuatZona("ZONA_BAHAYA", "\u2716  ZONA BAHAYA", warnaZonaBahaya, warnaBorderBahaya, new Vector2( 450f, -80f));

        // Container chip di bawah
        var chipArea = new GameObject("ChipArea");
        chipArea.transform.SetParent(_canvasGO.transform, false);
        var caRT = chipArea.AddComponent<RectTransform>();
        caRT.anchorMin = new Vector2(0.5f, 0f); caRT.anchorMax = new Vector2(0.5f, 0f);
        caRT.pivot = new Vector2(0.5f, 0f);
        caRT.sizeDelta = new Vector2(1700f, 200f);
        caRT.anchoredPosition = new Vector2(0f, 50f);

        var grid = chipArea.AddComponent<GridLayoutGroup>();
        grid.cellSize = chipUkuran;
        grid.spacing  = new Vector2(20f, 20f);
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = Mathf.Min(6, chips.Length);

        foreach (var c in chips)
        {
            var go = BuatChip(c, chipArea.transform);
            _chipPool.Add(go);
        }
    }

    RectTransform BuatZona(string name, string label, Color bg, Color border, Vector2 pos)
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
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(780f, 420f);
        rt.anchoredPosition = pos;

        var lab = BuatTeks(go.transform, "Label", label, 32, Color.white, FontStyles.Bold);
        lab.alignment = TextAlignmentOptions.Center;
        var lrt = lab.rectTransform;
        lrt.anchorMin = new Vector2(0f, 1f); lrt.anchorMax = new Vector2(1f, 1f);
        lrt.pivot = new Vector2(0.5f, 1f);
        lrt.offsetMin = new Vector2(20f, -65f);
        lrt.offsetMax = new Vector2(-20f, -15f);

        return rt;
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
    public bool CekDrop(ChipData data, Vector2 screenPos)
    {
        if (_quizSelesai) return false;

        bool diZonaAman   = RectTransformUtility.RectangleContainsScreenPoint(_zonaAmanRT,   screenPos);
        bool diZonaBahaya = RectTransformUtility.RectangleContainsScreenPoint(_zonaBahayaRT, screenPos);

        if (!diZonaAman && !diZonaBahaya) return false;

        string jawabanPemain = diZonaAman ? "AMAN" : "BAHAYA";
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

        if (_chipDitempatkan >= chips.Length) SelesaikanQuiz();
        return true;
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
                // Jangan langsung selesai — tampilkan narasi jembatan ke ChatSim
                // ("Rara sampai sekolah, HP bergetar..."). Tombol di-disable dulu
                // supaya tidak bisa di-spam-klik.
                btn.interactable = false;
                StartCoroutine(JalankanNarasiOutroLaluSelesai());
            }
            else
            {
                if (_canvasGO != null) Destroy(_canvasGO);
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
public class DraggableChip : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Canvas canvas;
    public ZonaTubuhQuiz quiz;
    public ZonaTubuhQuiz.ChipData data;
    public Image chipImage;

    private RectTransform _rt;
    private CanvasGroup _cg;
    private Vector2 _posAwal;
    private Transform _parentAwal;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _cg = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _posAwal = _rt.anchoredPosition;
        _parentAwal = transform.parent;
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();
        _cg.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform, eventData.position, canvas.worldCamera, out local);
        _rt.anchoredPosition = local;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _cg.blocksRaycasts = true;
        bool accepted = quiz.CekDrop(data, eventData.position);
        if (accepted)
        {
            // Disable & fade
            _cg.interactable = false;
            chipImage.color = new Color(chipImage.color.r, chipImage.color.g, chipImage.color.b, 0.4f);
        }
        else
        {
            // Kembali ke posisi awal
            transform.SetParent(_parentAwal, false);
            _rt.anchoredPosition = _posAwal;
        }
    }
}
