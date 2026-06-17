using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// HalteDialog — Fase Halte Day 2.
///
/// Menampilkan halte angkot dengan NPC "Pria Asing" mendekati Rara,
/// kemudian dialog 2 baris diikuti pilihan AMAN / RAGU / BAHAYA.
///
/// Pilihan AMAN  : "Maaf, saya nggak kenal." (mundur, dapat poin)
/// Pilihan RAGU  : "Hmm... saya mikir dulu."
/// Pilihan BAHAYA: "Boleh, ke mana om?" (kehilangan nyawa)
///
/// Semua dibangun procedural \u2014 tidak butuh prefab.
/// Custom semua teks/warna lewat Inspector.
/// </summary>
public class HalteDialog : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // KELAS PROP — sprite tambahan bebas (tanaman, motor, awan, dsb)
    // ══════════════════════════════════════════════════════════════════════
    [System.Serializable]
    public class HalteProp
    {
        [Tooltip("Nama prop (hanya label untuk debugging).")]
        public string  nama = "Prop";
        [Tooltip("Sprite yang akan ditampilkan.")]
        public Sprite  sprite;
        [Tooltip("Posisi prop di kanvas (x=horizontal, y=vertikal).")]
        public Vector2 posisi = Vector2.zero;
        [Tooltip("Ukuran prop dalam pixel.")]
        public Vector2 ukuran = new Vector2(120f, 120f);
        [Tooltip("Tint sprite. Putih = warna asli.")]
        public Color   warna  = Color.white;
        [Tooltip("Jaga aspect ratio sprite (centang) atau stretch ke ukuran (kosong).")]
        public bool    jagaAspek = true;
        [Tooltip("Urutan render. Kecil = di belakang, besar = di depan. 0 = default.")]
        [Range(-10, 10)] public int orderOffset = 0;
    }

    // ══════════════════════════════════════════════════════════════════════
    // KELAS BARIS DIALOG — teks + sprite latar per baris (opsional)
    // ══════════════════════════════════════════════════════════════════════
    [System.Serializable]
    public class BarisDialog
    {
        [Tooltip("Isi teks dialog/narasi yang akan ditampilkan.")]
        [TextArea(2, 4)] public string teks = "";
        [Tooltip("Sprite latar khusus untuk baris ini (opsional).\n" +
                 "Kalau diisi → background halte langsung ganti ke sprite ini saat baris tampil.\n" +
                 "Kalau kosong → tetap pakai sprite fase sebelumnya.")]
        public Sprite latarSprite;
        [Tooltip("Tampilkan badge '⚠ TANDA BAHAYA' saat baris ini muncul — untuk\n" +
                 "mengajari anak MENGENALI perilaku grooming (red flag).")]
        public bool tandaBahaya = false;
        [Tooltip("Teks penjelas badge (mis. 'Memberi iming-iming gratis', 'Minta nomor HP',\n" +
                 "'Mengajak menyimpan rahasia'). Kosong = pakai teks default badge.")]
        public string tandaBahayaTeks = "";
    }

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR
    // ══════════════════════════════════════════════════════════════════════

    [Header("Sprite (opsional)")]
    [HideInInspector] public Sprite raraSprite;
    [HideInInspector] public Sprite priaAsingSprite;

    // ── DEPRECATED (per request user): semua latar halte sekarang diambil dari
    //    BarisDialog.latarSprite per dialog. Field-field di bawah disembunyikan
    //    supaya tidak muncul lagi di Inspector, tapi tetap ada untuk
    //    kompatibilitas YAML scene lama (tidak akan menyebabkan error MissingField).
    [HideInInspector] public Sprite halteBackgroundSprite;
    [HideInInspector] public Vector2 halteBackgroundPos  = Vector2.zero;
    [HideInInspector] public Vector2 halteBackgroundSize = new Vector2(1920f, 1080f);
    [HideInInspector] public Color   halteBackgroundTint = Color.white;
    [HideInInspector] public bool    halteBackgroundPreserveAspect = false;
    [HideInInspector] public bool    halteBackgroundStretchFull    = true;
    [HideInInspector] public Sprite  haltePhase1Sprite;
    [HideInInspector] public Sprite  haltePhase2Sprite;
    [HideInInspector] public Sprite  haltePhase3Sprite;

    // ══════════════════════════════════════════════════════════════════════
    // KUSTOMISASI SPRITE — atur di Inspector, semua field opsional
    // Jika sprite kosong → fallback ke kotak procedural / warna solid.
    // Klik kanan komponen → "▶ Apply Halte Customization" untuk
    // langsung lihat hasilnya saat Play (live edit).
    // ══════════════════════════════════════════════════════════════════════

    // ── DEPRECATED / HIDDEN: parameter scene procedural lama. Disembunyikan dari Inspector
    //    karena sekarang halte memakai sprite tunggal (halteBackgroundSprite + 3-fase).
    //    Field tetap ada untuk kompatibilitas YAML scene & fallback procedural.
    [HideInInspector] public Vector2 raraPos  = new Vector2(-380f, -120f);
    [HideInInspector] public Vector2 raraSize = new Vector2(360f, 480f);
    [HideInInspector] public Color   raraTint = Color.white;

    [HideInInspector] public Vector2 priaPos  = new Vector2(380f, -120f);
    [HideInInspector] public Vector2 priaSize = new Vector2(360f, 480f);
    [HideInInspector] public Color   priaTint = Color.white;

    [HideInInspector] public Sprite  atapSprite;
    [HideInInspector] public Vector2 atapPos  = new Vector2(0f, 290f);
    [HideInInspector] public Vector2 atapSize = new Vector2(1500f, 50f);

    [HideInInspector] public Sprite  tiangKiriSprite;
    [HideInInspector] public Vector2 tiangKiriPos  = new Vector2(-720f, 0f);
    [HideInInspector] public Vector2 tiangKiriSize = new Vector2(34f, 600f);

    [HideInInspector] public Sprite  tiangKananSprite;
    [HideInInspector] public Vector2 tiangKananPos  = new Vector2(720f, 0f);
    [HideInInspector] public Vector2 tiangKananSize = new Vector2(34f, 600f);

    [HideInInspector] public Sprite  bangkuSprite;
    [HideInInspector] public Vector2 bangkuPos  = new Vector2(0f, -240f);
    [HideInInspector] public Vector2 bangkuSize = new Vector2(1400f, 26f);

    [HideInInspector] public Sprite  papanInfoSprite;
    [HideInInspector] public Vector2 papanInfoPos  = new Vector2(-540f, 200f);
    [HideInInspector] public Vector2 papanInfoSize = new Vector2(280f, 110f);
    [HideInInspector] public string  papanInfoText = "HALTE\nANGKOT";
    [HideInInspector] public int     papanInfoFontSize = 22;

    [HideInInspector] public List<HalteProp> propsTambahan = new List<HalteProp>();

    [Header("Live Edit (Play Mode)")]
    [Tooltip("Centang: tiap perubahan Inspector langsung terapkan ke scene halte saat sedang aktif.")]
    public bool liveEditHalte = true;

    [Header("─ Box Dialog (sama dengan Day1Intro/Day2NarasiAwal) ─")]
    [Tooltip("Sprite panel kayu untuk kotak dialog (sliced). Kosong = pakai panel rounded fallback.")]
    public Sprite panelSprite;
    [Tooltip("Path sprite panel (relatif Assets/) untuk auto-load saat Reset.")]
    public string panelSpritePath = "sprites/UI day 1/8.png";
    [Tooltip("Warna panel saat panelSprite di-assign.")]
    public Color  panelTint = Color.white;

    [Header("Box Dialog — Portrait di Kotak (sama Day1Intro)")]
    [Tooltip("Sprite portrait untuk fase Narasi (mis. gulungan kertas / scroll icon). Tampil di area kiri box dialog saat speaker = 'Narasi'.")]
    public Sprite portraitNarasi;
    [Tooltip("Sprite portrait Rara untuk box dialog (foto wajah, BUKAN sprite full body di scene).")]
    public Sprite portraitRara;
    [Tooltip("Sprite portrait Pria Asing untuk box dialog. Kosong = pakai portraitNarasi sebagai fallback.")]
    public Sprite portraitPriaAsing;

    [Header("Box Dialog — Layout (anchor 0–1, default = Day1Intro)")]
    [Range(0f, 1f)] public float boxPanelCenterX   = 0.50f;
    [Range(0f, 1f)] public float boxPanelCenterY   = 0.215f;
    [Range(0.1f, 1f)]   public float boxPanelWidth  = 0.96f;
    [Range(0.02f, 0.5f)] public float boxPanelHeight = 0.395f;
    [Range(0f, 1f)] public float boxPortraitCenterX = 0.153f;
    [Range(0f, 1f)] public float boxPortraitCenterY = 0.625f;
    [Range(0.02f, 0.6f)] public float boxPortraitW = 0.192f;
    [Range(0.02f, 1f)]   public float boxPortraitH = 0.494f;
    public bool boxPortraitPreserveAspect = true;
    public Vector2 boxBannerAnchorMin = new Vector2(0.11f, 0.11f);
    public Vector2 boxBannerAnchorMax = new Vector2(0.253f, 0.333f);
    public Vector2 boxTextAnchorMin   = new Vector2(0.31f, 0.55f);
    public Vector2 boxTextAnchorMax   = new Vector2(0.84f, 0.76f);
    [Range(0f, 1f)] public float boxHintCenterX = 0.82f;
    [Range(0f, 1f)] public float boxHintCenterY = 0.13f;
    [Range(0.05f, 1f)] public float boxHintSizeW = 0.30f;
    [Range(0.02f, 0.5f)] public float boxHintSizeH = 0.12f;

    [Header("Box Dialog — Warna & Font")]
    public Color  boxNamaColor   = new Color(1f, 0.85f, 0.30f, 1f);
    public Color  boxTextColor   = Color.white;
    public Color  boxHintColor   = new Color(1f, 1f, 1f, 0.55f);
    public int    boxNamaFontSize = 30;
    public int    boxTextFontSize = 26;
    public int    boxHintFontSize = 16;
    [Tooltip("Speaker name di-render UPPERCASE (sesuai Day1Intro 'NARASI').")]
    public bool   boxNamaUppercase = true;
    [Tooltip("Teks hint pojok kanan-bawah panel.")]
    public string boxHintText = "\u25BC SPACE / Klik untuk lanjut";

    [Header("Animasi Mengetik (Typewriter)")]
    [Tooltip("Detik per karakter saat teks diketik. 0 = langsung penuh (skip animasi).")]
    [Range(0f, 0.2f)] public float kecepatanKetik = 0.025f;
    [Tooltip("Jeda singkat (detik) setelah ketikan selesai sebelum bisa lanjut.")]
    [Range(0f, 1f)]   public float delaySetelahKetik = 0.10f;
    [Tooltip("Klik / SPACE saat sedang mengetik akan langsung menampilkan teks penuh (skip).")]
    public bool   bolehSkipKetik = true;

    // Warna procedural (dipakai hanya saat halteBackgroundSprite kosong)
    [HideInInspector] public Color   warnaLatar       = new Color(0.45f, 0.62f, 0.78f, 1f);
    [HideInInspector] public Color   warnaAtap        = new Color(0.30f, 0.20f, 0.12f, 1f);
    [HideInInspector] public Color   warnaTiang       = new Color(0.25f, 0.18f, 0.10f, 1f);
    [HideInInspector] public Color   warnaPapanInfo   = new Color(0.20f, 0.50f, 0.30f, 1f);

    [Header("Dialog \u2014 baris awal Pria Asing")]
    [Tooltip("Baris narasi FASE 1. Setiap baris boleh punya sprite latar sendiri (opsional).\n" +
             "Kalau Latar Sprite diisi → BG halte langsung ganti saat baris itu tampil.\n" +
             "Kalau kosong → tetap pakai sprite Fase 1 dari section di atas.")]
    public List<BarisDialog> dialogIntro = new List<BarisDialog>
    {
        new BarisDialog { teks = "Pagi itu Rara sampai di halte yang cukup ramai. Beberapa orang ikut menunggu angkot jurusan sekolah." },
        new BarisDialog { teks = "Rara berdiri di pinggir sambil sesekali melihat jam. Angkotnya belum datang juga." },
        new BarisDialog { teks = "Dari tadi, ada seorang pria asing bertopi yang terus memperhatikan Rara dari kejauhan..." },
        new BarisDialog { teks = "Pelan-pelan, pria itu mendekat dan berdiri tepat di sebelah Rara." }
    };

    [Tooltip("Baris dialog Pria Asing FASE 2. Setiap baris boleh punya sprite latar sendiri (opsional).\n" +
             "Kosongkan Latar Sprite → tetap pakai sprite Fase 2 dari section di atas.")]
    public List<BarisDialog> dialogAwal = new List<BarisDialog>
    {
        new BarisDialog { teks = "Hai, cantik! Sendirian aja nih? Om dari tadi merhatiin kamu lho.",
                          tandaBahaya = true, tandaBahayaTeks = "Orang asing tiba-tiba sok akrab & memperhatikanmu" },
        new BarisDialog { teks = "Mau ke sekolah ya? Om kebetulan searah. Daripada nunggu angkot lama, bareng om aja yuk — gratis kok.",
                          tandaBahaya = true, tandaBahayaTeks = "Memberi iming-iming / tumpangan gratis" },
        new BarisDialog { teks = "Eh, WA kamu berapa? Nanti om anter pulang sekolah ya. Rahasia aja, nggak usah bilang siapa-siapa.",
                          tandaBahaya = true, tandaBahayaTeks = "Minta data pribadi & mengajak menyimpan rahasia" }
    };

    [Header("Pilihan Pemain")]
    public string pilihanAman   = "“Maaf Om, saya nggak kenal Om. TOLONG jangan ganggu saya!” (tolak tegas + suara keras)";
    public string pilihanRagu   = "“Hmm... saya pikir-pikir dulu ya Om...”";
    public string pilihanBahaya = "“Boleh deh Om, ini WA saya...”";

    [Header("Reaksi Setelah Pilih")]
    [Tooltip("Reaksi saat pemain pilih AMAN. Latar Sprite di sini akan jadi halte background saat teks reaksi tampil.")]
    public BarisDialog reaksiAman = new BarisDialog { teks = "\u2713 BAGUS, RA! Kamu menolak tegas & bersuara keras minta tolong.\nOrang-orang di halte langsung menoleh dan ibu-ibu menghampirimu. Pria itu salah tingkah lalu pergi.\nIngat: nomor HP/WA itu DATA PRIBADI — jangan diberi ke orang asing!" };
    [Tooltip("Reaksi saat pemain pilih RAGU. Latar Sprite di sini akan jadi halte background saat teks reaksi tampil.")]
    public BarisDialog reaksiRagu = new BarisDialog { teks = "\u26A0 Kamu ragu-ragu menjawab. Pria itu makin maju dan terus memaksa minta nomormu.\nUntung angkot keburu datang dan kamu cepat naik. Lain kali, langsung TEGAS tolak ya!" };
    [Tooltip("Reaksi saat pemain pilih BAHAYA. Latar Sprite di sini akan jadi halte background saat teks reaksi tampil.")]
    public BarisDialog reaksiBahaya = new BarisDialog { teks = "\u2716 GAWAT! Kamu memberi nomor WA-mu ke orang asing.\nMalamnya HP-mu dibanjiri chat aneh dari pria itu. Kamu kehilangan 1 nyawa.\nIngat: kasih sayang & hadiah dari orang asing = RED FLAG grooming!" };

    // Deprecated: field reaksi latar terpisah sudah di-merge ke BarisDialog di atas.
    [HideInInspector] public Sprite latarReaksiAman;
    [HideInInspector] public Sprite latarReaksiRagu;
    [HideInInspector] public Sprite latarReaksiBahaya;

    [Header("Skor & Nyawa")]
    [Tooltip("Tambahkan poin LAPOR bonus saat AMAN.")]
    public bool berikanBonusLaporSaatAman = false;
    [Tooltip("Kurangi nyawa saat pilih BAHAYA.")]
    public bool kurangiNyawaSaatBahaya = true;

    [Header("Blokir (aksi konkret menolak grooming)")]
    [Tooltip("Setelah pilih AMAN, tampilkan tombol BLOKIR sebagai aksi konkret menolak kontak orang asing.")]
    public bool aktifkanBlokir = true;
    public string tombolBlokirTeks = "🚫  BLOKIR nomor orang asing";
    [TextArea(2, 4)]
    public string reaksiBlokirTeks = "✓ TEPAT! Kamu BLOKIR nomor orang asing itu.\nKalau ada yang memaksa minta kontak/sosmed-mu: tolak, blokir, lalu ceritakan ke orang dewasa tepercaya.";
    [Tooltip("Bonus skor saat menekan BLOKIR (aksi perlindungan diri).")]
    public int bonusBlokir = 50;

    [Header("Tombol Lanjut Setelah Reaksi")]
    public string tombolLanjutTeks = "\u25B6  Naik angkot";

    [Header("Warna Tombol Pilihan")]
    public Color warnaAman   = new Color(0.15f, 0.68f, 0.38f, 1f);
    public Color warnaRagu   = new Color(0.95f, 0.62f, 0.07f, 1f);
    public Color warnaBahaya = new Color(0.91f, 0.30f, 0.24f, 1f);
    public Color warnaNetral = new Color(0.20f, 0.62f, 0.86f, 1f);

    [Header("Font")]
    public TMP_FontAsset fontAsset;

    [Header("Badge Tanda Bahaya (Edukasi Red Flag)")]
    [Tooltip("Judul tetap pada badge peringatan.")]
    public string badgeJudul = "⚠  TANDA BAHAYA";
    [Tooltip("Warna latar badge.")]
    public Color badgeWarna = new Color(0.85f, 0.18f, 0.14f, 0.95f);
    [Tooltip("Warna teks badge.")]
    public Color badgeTeksWarna = Color.white;

    [Header("Sorting")]
    public int sortingOrder = 920;

    [Header("Voice Meter \u2014 Mekanik Teriak (Voice-Driven Action)")]
    [Tooltip("Aktifkan mini-game Voice Meter di Halte. Saat pilihan pemicu (default AMAN)\n" +
             "dipilih, pemain harus MENAHAN tombol Teriak (atau berteriak ke mikrofon)\n" +
             "sampai meter MERAH sebelum reaksi muncul. Konsisten dgn AngkotSentuhScene.")]
    public bool aktifkanVoiceMeter = true;
    [Tooltip("Kategori pilihan yang memicu Voice Meter (default: AMAN \u2014 tolak tegas + suara keras).")]
    public string kategoriPemicu = "AMAN";
    [Tooltip("Pakai MIKROFON asli sebagai input suara. Kalau OFF / mic tidak ada\n" +
             "\u2192 otomatis fallback ke TAHAN tombol.")]
    public bool gunakanMikrofon = false;
    [Tooltip("Sensitivitas mikrofon (kalikan loudness mentah). Naikkan kalau suara terlalu pelan.")]
    [Range(1f, 40f)] public float sensitivitasMic = 12f;
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
    public string judulTeriak     = "\uD83D\uDDE3  TERIAK SEKUATNYA!";
    public string instruksiTeriak = "TAHAN tombol & teriak \u201CJANGAN GANGGU SAYA!\u201D sampai meter MERAH!";
    public string teksTombolTeriak = "TAHAN UNTUK TERIAK";
    public string labelNormal   = "Suara Normal";
    public string labelSedang   = "Suara Sedang";
    public string labelKeras    = "Suara KERAS";
    public string labelBerhasil = "\u2713  TERIAKAN BERHASIL!";
    public Color  hijauWarna  = new Color(0.16f, 0.74f, 0.40f, 1f);
    public Color  kuningWarna = new Color(0.96f, 0.78f, 0.10f, 1f);
    public Color  merahWarna  = new Color(0.91f, 0.26f, 0.22f, 1f);

    // ── runtime ───────────────────────────────────────────────────────────
    private Action     _onSelesai;
    private GameObject _canvasGO;
    private GameObject _dialogBoxGO;
    private TextMeshProUGUI _dialogText;
    private TextMeshProUGUI _speakerText;
    private TextMeshProUGUI _hintText;
    private Image            _portraitImg;
    private GameObject _pilihanRowGO;
    private Sprite     _roundedSprite;

    // Voice Meter (mini-game teriak) — input tahan tombol / mikrofon.
    private bool       _holdTeriak;
    private AudioClip  _micClip;
    private string     _micDevice;

    // Badge "Tanda Bahaya" (edukasi red flag)
    private GameObject      _badgeGO;
    private TextMeshProUGUI _badgeText;

    // State typewriter (animasi mengetik per baris).
    private bool   _ketikSelesai;
    private bool   _skipKetik;
    private string _teksLengkapAktif = "";

    // Refs ke setiap elemen halte — dipakai oleh ApplyHalteCustomization
    private Image           _rRaraImg, _rPriaImg;
    private RectTransform   _rRaraRT,  _rPriaRT;
    private Image           _rHalteBgImg;
    private RectTransform   _rHalteBgRT;
    // Override sprite halte aktif saat runtime (di-set oleh SetPhaseSprite per fase/baris).
    // ApplyHalteCustomization akan respect ini supaya tidak menimpa balik ke halteBackgroundSprite.
    private Sprite          _rHalteBgRuntimeOverride;
    private Image           _rAtap, _rTiangKiri, _rTiangKanan, _rBangku, _rPapan;
    private TextMeshProUGUI _rPapanLabel;
    private RectTransform   _rPapanLabelRT;
    private List<Image>     _rProps = new List<Image>();

    // Refs untuk LIVE-EDIT box dialog (panel, portrait, banner, text, hint, choices row)
    private RectTransform _bxPanelRT;
    private Image         _bxPanelImg;
    private RectTransform _bxPortraitRT;
    private RectTransform _bxSpeakerRT;
    private RectTransform _bxBodyRT;
    private RectTransform _bxHintRT;
    private RectTransform _bxChoicesRT;

    // ══════════════════════════════════════════════════════════════════════
    public void Mulai(Action onSelesai)
    {
        _onSelesai = onSelesai;
        // Reset runtime override supaya fase pertama (Phase 1) langsung apply bersih.
        _rHalteBgRuntimeOverride = null;
        BuildHalteScene();
        StartCoroutine(JalankanDialog());
    }

    // ══════════════════════════════════════════════════════════════════════
    void BuildHalteScene()
    {
        _canvasGO = new GameObject("HalteDialog_Canvas");
        var canvas = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = _canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        _canvasGO.AddComponent<GraphicRaycaster>();

        // ── HALTE (procedural atau sprite) ───────────────────────────────
        // Halte BG SELALU dibuat fullscreen, sprite-nya per-baris.
        var bg = new GameObject("HalteBG");
        bg.transform.SetParent(_canvasGO.transform, false);
        var img = bg.AddComponent<Image>();
        Sprite spriteAwal = AmbilSpriteAwal();
        img.sprite         = spriteAwal;
        img.color          = Color.white;
        img.preserveAspect = false;
        _rHalteBgRuntimeOverride = spriteAwal;
        img.raycastTarget  = false;
        var rt = bg.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        _rHalteBgImg = img;
        _rHalteBgRT  = rt;

        // ── KARAKTER ─────────────────────────────────────────────────────
        // Hanya render Rara/PriaAsing sprite procedural saat MODE PROCEDURAL aktif
        // (halteBackgroundSprite kosong). Saat pakai sprite halte (halte_1/2/3),
        // karakter sudah baked di gambar — jangan double-render.
        // Karakter Rara & Pria Asing tidak di-render terpisah lagi karena per-baris sprite sudah berisi karakter.

        // ── PROPS TAMBAHAN (sprite custom user) ──────────────────────────
        BuildPropsTambahan();

        // ── KOTAK DIALOG ─────────────────────────────────────────────────
        BuildDialogBox();
    }

    void BuildHalteProcedural()
    {
        // Tiang kiri
        _rTiangKiri  = BuatBentukOrSprite("TiangKiri",  tiangKiriSprite,  tiangKiriPos,  tiangKiriSize,  warnaTiang);
        // Tiang kanan
        _rTiangKanan = BuatBentukOrSprite("TiangKanan", tiangKananSprite, tiangKananPos, tiangKananSize, warnaTiang);
        // Atap
        _rAtap       = BuatBentukOrSprite("Atap",       atapSprite,       atapPos,       atapSize,       warnaAtap);
        // Bangku
        _rBangku     = BuatBentukOrSprite("Bangku",     bangkuSprite,     bangkuPos,     bangkuSize,     warnaTiang);
        // Papan info halte
        _rPapan      = BuatBentukOrSprite("PapanInfo",  papanInfoSprite,  papanInfoPos,  papanInfoSize,  warnaPapanInfo);

        // Label papan info (di atas papan)
        _rPapanLabel = BuatTeks(_canvasGO.transform, "PapanLabel", papanInfoText, papanInfoFontSize, Color.white, FontStyles.Bold);
        _rPapanLabel.alignment = TextAlignmentOptions.Center;
        _rPapanLabelRT = _rPapanLabel.rectTransform;
        _rPapanLabelRT.anchorMin = new Vector2(0.5f, 0.5f);
        _rPapanLabelRT.anchorMax = new Vector2(0.5f, 0.5f);
        _rPapanLabelRT.pivot     = new Vector2(0.5f, 0.5f);
        _rPapanLabelRT.sizeDelta = papanInfoSize;
        _rPapanLabelRT.anchoredPosition = papanInfoPos;
    }

    void BuildPropsTambahan()
    {
        _rProps.Clear();
        if (propsTambahan == null) return;

        for (int i = 0; i < propsTambahan.Count; i++)
        {
            var p = propsTambahan[i];
            if (p == null || p.sprite == null) { _rProps.Add(null); continue; }

            var go = new GameObject($"Prop_{i}_{(string.IsNullOrEmpty(p.nama) ? "Untitled" : p.nama)}");
            go.transform.SetParent(_canvasGO.transform, false);
            var img = go.AddComponent<Image>();
            img.sprite         = p.sprite;
            img.color          = p.warna;
            img.preserveAspect = p.jagaAspek;
        img.raycastTarget  = false;

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta        = p.ukuran;
            rt.anchoredPosition = p.posisi;
            rt.SetSiblingIndex(Mathf.Clamp(rt.GetSiblingIndex() + p.orderOffset, 0, _canvasGO.transform.childCount - 1));

            _rProps.Add(img);
        }
    }

    /// Helper baru: bangun Image dengan sprite kalau ada, fallback ke warna solid.
    Image BuatBentukOrSprite(string name, Sprite spr, Vector2 pos, Vector2 size, Color fallbackColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_canvasGO.transform, false);
        var img = go.AddComponent<Image>();
        if (spr != null)
        {
            img.sprite         = spr;
        img.color          = Color.white;
        img.preserveAspect = false;   // bagian halte biasanya di-stretch
            img.type           = Image.Type.Simple;
        }
        else
        {
            img.color = fallbackColor;
        }
        img.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = size;
        rt.anchoredPosition = pos;
        return img;
    }

    void BuatBentuk(string name, Vector2 pos, Vector2 size, Color c)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_canvasGO.transform, false);
        var img = go.AddComponent<Image>();
        img.color = c;
        img.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
    }

    void BuildKarakter(string name, Sprite spr, Vector2 pos, Vector2 size, Color tint, Color fallbackColor, bool isRara)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_canvasGO.transform, false);
        var img = go.AddComponent<Image>();
        if (spr != null)
        {
            img.sprite         = spr;
            img.color          = tint;
            img.preserveAspect = true;
        }
        else
        {
            img.sprite = GetRoundedSprite();
            img.color  = fallbackColor;
            img.type   = Image.Type.Sliced;
        }
        img.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        // Simpan referensi untuk live-edit
        if (isRara) { _rRaraImg = img; _rRaraRT = rt; }
        else        { _rPriaImg = img; _rPriaRT = rt; }

        // Label nama di bawah karakter
        var labelGO = new GameObject(name + "_Label");
        labelGO.transform.SetParent(_canvasGO.transform, false);
        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) tmp.font = fontAsset;
        else if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
        tmp.text = name == "PriaAsing" ? "Pria Asing" : name;
        tmp.fontSize  = 22;
        tmp.color     = new Color(1f, 0.95f, 0.75f, 1f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;
        var lrt = tmp.rectTransform;
        lrt.anchorMin = new Vector2(0.5f, 0.5f);
        lrt.anchorMax = new Vector2(0.5f, 0.5f);
        lrt.pivot     = new Vector2(0.5f, 0.5f);
        lrt.sizeDelta = new Vector2(300f, 40f);
        lrt.anchoredPosition = new Vector2(pos.x, pos.y - 200f);
    }

    void BuildDialogBox()
    {
        // ── Panel utama — meniru Day1Intro persis (panel besar 0.395 H) ──
        _dialogBoxGO = new GameObject("DialogBox");
        _dialogBoxGO.transform.SetParent(_canvasGO.transform, false);
        var img = _dialogBoxGO.AddComponent<Image>();
        bool hasBox = (panelSprite != null);
        if (hasBox)
        {
            // Day1Intro pakai Sliced — sprite 8.png punya border yg pas
            img.sprite         = panelSprite;
            img.type           = Image.Type.Sliced;
        img.color          = Color.white;
        }
        else
        {
            img.sprite = GetRoundedSprite();
            img.color  = new Color(0.05f, 0.08f, 0.12f, 0.94f);
            img.type   = Image.Type.Sliced;
            var outl = _dialogBoxGO.AddComponent<Outline>();
            outl.effectColor    = new Color(1f, 0.85f, 0.25f, 1f);
            outl.effectDistance = new Vector2(2f, -2f);
        }
        var rt = _dialogBoxGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(boxPanelCenterX - boxPanelWidth  * 0.5f,
                                   boxPanelCenterY - boxPanelHeight * 0.5f);
        rt.anchorMax = new Vector2(boxPanelCenterX + boxPanelWidth  * 0.5f,
                                   boxPanelCenterY + boxPanelHeight * 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        _bxPanelRT  = rt;
        _bxPanelImg = img;

        // ── Portrait (kiri, di area frame portrait baked-in sprite) ──
        var portraitGO = new GameObject("Portrait");
        portraitGO.transform.SetParent(_dialogBoxGO.transform, false);
        var portraitRT = portraitGO.AddComponent<RectTransform>();
        portraitRT.anchorMin = new Vector2(
            boxPortraitCenterX - boxPortraitW * 0.5f,
            boxPortraitCenterY - boxPortraitH * 0.5f);
        portraitRT.anchorMax = new Vector2(
            boxPortraitCenterX + boxPortraitW * 0.5f,
            boxPortraitCenterY + boxPortraitH * 0.5f);
        portraitRT.offsetMin = portraitRT.offsetMax = Vector2.zero;
        _portraitImg = portraitGO.AddComponent<Image>();
        _portraitImg.preserveAspect = boxPortraitPreserveAspect;
        _portraitImg.color          = Color.white;
        _portraitImg.raycastTarget  = false;
        _portraitImg.enabled        = false; // diaktifkan saat TampilkanBaris assign sprite
        _bxPortraitRT = portraitRT;

        // ── Banner nama pembicara (anchor sama Day1Intro) ──
        _speakerText = BuatTeks(_dialogBoxGO.transform, "SpeakerName", "",
                                boxNamaFontSize, boxNamaColor, FontStyles.Bold);
        _speakerText.alignment = TextAlignmentOptions.Center;
        var srt = _speakerText.rectTransform;
        srt.anchorMin = boxBannerAnchorMin;
        srt.anchorMax = boxBannerAnchorMax;
        srt.offsetMin = new Vector2(6f, 2f);
        srt.offsetMax = new Vector2(-6f, -2f);
        _bxSpeakerRT = srt;

        // ── Teks isi dialog (anchor sama Day1Intro) ──
        _dialogText = BuatTeks(_dialogBoxGO.transform, "BodyText", "",
                               boxTextFontSize, boxTextColor, FontStyles.Normal);
        _dialogText.alignment = TextAlignmentOptions.TopLeft;
        _dialogText.textWrappingMode = TMPro.TextWrappingModes.Normal;
        var trt = _dialogText.rectTransform;
        trt.anchorMin = boxTextAnchorMin;
        trt.anchorMax = boxTextAnchorMax;
        trt.offsetMin = new Vector2(4f, 4f);
        trt.offsetMax = new Vector2(-4f, -4f);
        _bxBodyRT = trt;

        // ── Hint "▼ SPACE / Klik untuk lanjut" pojok kanan-bawah ──
        _hintText = BuatTeks(_dialogBoxGO.transform, "Hint", boxHintText,
                             boxHintFontSize, boxHintColor, FontStyles.Italic);
        _hintText.alignment = TextAlignmentOptions.MidlineRight;
        var hrt = _hintText.rectTransform;
        hrt.anchorMin = new Vector2(boxHintCenterX - boxHintSizeW * 0.5f,
                                    boxHintCenterY - boxHintSizeH * 0.5f);
        hrt.anchorMax = new Vector2(boxHintCenterX + boxHintSizeW * 0.5f,
                                    boxHintCenterY + boxHintSizeH * 0.5f);
        hrt.offsetMin = hrt.offsetMax = Vector2.zero;
        _bxHintRT = hrt;

        // ── Row pilihan: di-parent ke Canvas, posisi di ATAS panel ──
        _pilihanRowGO = new GameObject("PilihanRow");
        _pilihanRowGO.transform.SetParent(_canvasGO.transform, false);
        var prt = _pilihanRowGO.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(boxPanelCenterX - boxPanelWidth * 0.5f,
                                    boxPanelCenterY + boxPanelHeight * 0.5f);
        prt.anchorMax = new Vector2(boxPanelCenterX + boxPanelWidth * 0.5f,
                                    boxPanelCenterY + boxPanelHeight * 0.5f);
        prt.pivot     = new Vector2(0.5f, 0f);
        prt.sizeDelta = new Vector2(0f, 90f);
        prt.anchoredPosition = new Vector2(0f, 12f);
        _bxChoicesRT = prt;
        var hLay = _pilihanRowGO.AddComponent<HorizontalLayoutGroup>();
        hLay.childAlignment = TextAnchor.MiddleCenter;
        hLay.spacing = 20f;
        hLay.childForceExpandWidth = true;
        hLay.childForceExpandHeight = true;

        BuildBadge();
    }

    // Badge "⚠ TANDA BAHAYA" — banner merah di atas layar untuk menandai red flag
    // grooming. Tersembunyi secara default; ditampilkan via SetBadge().
    void BuildBadge()
    {
        _badgeGO = new GameObject("BadgeTandaBahaya");
        _badgeGO.transform.SetParent(_canvasGO.transform, false);
        var img = _badgeGO.AddComponent<Image>();
        img.sprite = GetRoundedSprite();
        img.type   = Image.Type.Sliced;
        img.color  = badgeWarna;
        var outl = _badgeGO.AddComponent<Outline>();
        outl.effectColor    = new Color(1f, 1f, 1f, 0.5f);
        outl.effectDistance = new Vector2(2f, -2f);
        var rt = _badgeGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.86f);
        rt.anchorMax = new Vector2(0.5f, 0.86f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(900f, 92f);
        rt.anchoredPosition = Vector2.zero;

        var judul = BuatTeks(_badgeGO.transform, "Judul", badgeJudul, 26, badgeTeksWarna, FontStyles.Bold);
        judul.alignment = TextAlignmentOptions.Top;
        var jrt = judul.rectTransform;
        jrt.anchorMin = new Vector2(0f, 0.52f); jrt.anchorMax = new Vector2(1f, 1f);
        jrt.offsetMin = new Vector2(12f, 2f);   jrt.offsetMax = new Vector2(-12f, -4f);

        _badgeText = BuatTeks(_badgeGO.transform, "Keterangan", "", 19, badgeTeksWarna, FontStyles.Italic);
        _badgeText.alignment = TextAlignmentOptions.Bottom;
        var krt = _badgeText.rectTransform;
        krt.anchorMin = new Vector2(0f, 0f); krt.anchorMax = new Vector2(1f, 0.5f);
        krt.offsetMin = new Vector2(12f, 4f); krt.offsetMax = new Vector2(-12f, -2f);

        _badgeGO.SetActive(false);
    }

    // Tampilkan / sembunyikan badge red flag. keterangan kosong = pakai judul saja.
    void SetBadge(bool tampil, string keterangan = "")
    {
        if (_badgeGO == null) return;
        _badgeGO.SetActive(tampil);
        if (tampil)
        {
            if (_badgeText != null) _badgeText.text = keterangan ?? "";
            _badgeGO.transform.SetAsLastSibling();
        }
    }

    // Pilih sprite portrait box berdasarkan nama speaker
    Sprite GetPortraitForSpeaker(string speaker)
    {
        if (string.IsNullOrEmpty(speaker)) return portraitNarasi;
        string s = speaker.ToLower();
        if (s.Contains("rara"))   return portraitRara   != null ? portraitRara   : portraitNarasi;
        if (s.Contains("pria") || s.Contains("om") || s.Contains("asing"))
                                  return portraitPriaAsing != null ? portraitPriaAsing : portraitNarasi;
        if (s.Contains("narasi") || s.Contains("reaksi")) return portraitNarasi;
        return portraitNarasi;
    }

    // ══════════════════════════════════════════════════════════════════════
    IEnumerator JalankanDialog()
    {
        // FASE 1 — INTRO: Rara baru tiba. BG diambil dari latarSprite per baris.
        SetKarakterAktif(rara: true, pria: false);
        foreach (var baris in dialogIntro)
        {
            if (baris == null) continue;
            if (baris.latarSprite != null) SetPhaseSprite(baris.latarSprite);
            SetBadge(baris.tandaBahaya, string.IsNullOrEmpty(baris.tandaBahayaTeks) ? "" : baris.tandaBahayaTeks);
            yield return TampilkanBaris("Narasi", baris.teks);
            yield return TungguTap();
        }

        // FASE 2 — PRIA ASING MENDEKATI: dialog awal + pilihan. BG per baris.
        SetKarakterAktif(rara: true, pria: true);
        foreach (var baris in dialogAwal)
        {
            if (baris == null) continue;
            if (baris.latarSprite != null) SetPhaseSprite(baris.latarSprite);
            SetBadge(baris.tandaBahaya, string.IsNullOrEmpty(baris.tandaBahayaTeks) ? "" : baris.tandaBahayaTeks);
            yield return TampilkanBaris("Pria Asing", baris.teks);
            yield return TungguTap();
        }

        // Sembunyikan badge sebelum pilihan tampil.
        SetBadge(false);
        // Tampilkan pilihan (sprite halte berganti ke fase pilihan saat tombol muncul,
        // lalu ke fase reaksi setelah pemain memilih).
        yield return TampilkanPilihan();
    }

    // Ganti sprite halte BG ke fase yang ditentukan (kalau di-assign).
    // Per-baris/per-fase sprite SELALU di-stretch fullscreen (preserveAspect=false)
    // supaya benar-benar jadi latar belakang, bukan kotak kecil di tengah layar.
    // Pilih sprite halte awal: ambil latarSprite pertama yang non-null dari dialog/reaksi.
    Sprite AmbilSpriteAwal()
    {
        if (dialogIntro != null) foreach (var b in dialogIntro) if (b != null && b.latarSprite != null) return b.latarSprite;
        if (dialogAwal  != null) foreach (var b in dialogAwal)  if (b != null && b.latarSprite != null) return b.latarSprite;
        if (reaksiAman   != null && reaksiAman.latarSprite   != null) return reaksiAman.latarSprite;
        if (reaksiRagu   != null && reaksiRagu.latarSprite   != null) return reaksiRagu.latarSprite;
        if (reaksiBahaya != null && reaksiBahaya.latarSprite != null) return reaksiBahaya.latarSprite;
        return null;
    }

    void SetPhaseSprite(Sprite phaseSprite)
    {
        if (phaseSprite == null) return;       // tidak diisi → jangan ganti
        if (_rHalteBgImg == null) return;      // pakai mode procedural — skip
        _rHalteBgImg.sprite = phaseSprite;
        _rHalteBgImg.color  = halteBackgroundTint;
        _rHalteBgImg.preserveAspect = false;   // paksa fill seluruh layar
        // Paksa rect halte BG ke stretch fullscreen supaya sprite menutupi semua area
        if (_rHalteBgRT != null)
        {
            _rHalteBgRT.anchorMin = Vector2.zero;
            _rHalteBgRT.anchorMax = Vector2.one;
            _rHalteBgRT.offsetMin = Vector2.zero;
            _rHalteBgRT.offsetMax = Vector2.zero;
        }
        // Pastikan halte BG ada di paling belakang (di-bawah props, karakter, kotak dialog)
        if (_rHalteBgImg.transform != null)
            _rHalteBgImg.transform.SetAsFirstSibling();
        // Catat override aktif supaya ApplyHalteCustomization tidak menimpa balik
        _rHalteBgRuntimeOverride = phaseSprite;
    }

    // Show/hide Rara & Pria Asing per fase (Pria Asing tidak tampil di fase 1).
    void SetKarakterAktif(bool rara, bool pria)
    {
        if (_rRaraImg != null) _rRaraImg.gameObject.SetActive(rara);
        if (_rPriaImg != null) _rPriaImg.gameObject.SetActive(pria);
    }

    IEnumerator TampilkanBaris(string speaker, string line)
    {
        if (_speakerText != null)
            _speakerText.text = boxNamaUppercase ? (speaker ?? "").ToUpper() : (speaker ?? "");
        if (_portraitImg != null)
        {
            var sp = GetPortraitForSpeaker(speaker);
            _portraitImg.sprite  = sp;
            _portraitImg.enabled = false; // potret/sprite profil disembunyikan dari box dialog
        }
        // Sembunyikan hint selama mengetik
        if (_hintText != null) _hintText.gameObject.SetActive(false);

        // Animasi mengetik (typewriter)
        yield return KetikTeks(line);

        // Hint tampil saat menunggu tap
        if (_hintText != null) _hintText.gameObject.SetActive(true);
    }

    // Ketik teks per karakter. Tekan saat mengetik = skip (langsung penuh).
    IEnumerator KetikTeks(string teks)
    {
        _teksLengkapAktif = teks ?? "";
        _ketikSelesai     = false;
        _skipKetik        = false;
        if (_dialogText == null) yield break;

        if (kecepatanKetik <= 0f)
        {
            _dialogText.text = _teksLengkapAktif;
            _ketikSelesai    = true;
            yield break;
        }

        _dialogText.text = "";
        for (int i = 0; i < _teksLengkapAktif.Length; i++)
        {
            if (bolehSkipKetik && _skipKetik)
            {
                _dialogText.text = _teksLengkapAktif;
                break;
            }
            _dialogText.text += _teksLengkapAktif[i];
            yield return new WaitForSeconds(kecepatanKetik);
        }
        _ketikSelesai = true;

        if (delaySetelahKetik > 0f)
            yield return new WaitForSeconds(delaySetelahKetik);
    }

    IEnumerator TungguTap()
    {
        // Tekan saat sedang mengetik = skip; tekan setelah selesai = lanjut.
        while (true)
        {
            bool ditekan = Input.GetMouseButtonDown(0)
                        || Input.GetKeyDown(KeyCode.Space)
                        || Input.GetKeyDown(KeyCode.Return)
                        || Input.GetKeyDown(KeyCode.KeypadEnter);
            if (ditekan)
            {
                if (bolehSkipKetik && !_ketikSelesai) _skipKetik = true;
                else if (_ketikSelesai)              break;
            }
            yield return null;
        }
        AudioManager.Instance?.Click();
        yield return new WaitForSeconds(0.05f);
    }

    IEnumerator TampilkanPilihan()
    {
        if (_speakerText != null)
            _speakerText.text = boxNamaUppercase ? "RARA" : "Rara";
        if (_portraitImg != null)
        {
            var sp = GetPortraitForSpeaker("Rara");
            _portraitImg.sprite  = sp;
            _portraitImg.enabled = false; // potret/sprite profil disembunyikan dari box dialog
        }
        // Sembunyikan hint saat menampilkan tombol pilihan
        if (_hintText != null) _hintText.gameObject.SetActive(false);
        _dialogText.text = "<i><color=#FFD700>Pilih responmu:</color></i>";

        bool dipilih = false;
        string kategori = "";
        string label    = "";
        BarisDialog reaksi = null;

        BuatTombolPilihan(pilihanAman,   warnaAman,   () => { kategori = "AMAN";   label = pilihanAman;   reaksi = reaksiAman;   dipilih = true; });
        BuatTombolPilihan(pilihanRagu,   warnaRagu,   () => { kategori = "RAGU";   label = pilihanRagu;   reaksi = reaksiRagu;   dipilih = true; });
        BuatTombolPilihan(pilihanBahaya, warnaBahaya, () => { kategori = "BAHAYA"; label = pilihanBahaya; reaksi = reaksiBahaya; dipilih = true; });

        while (!dipilih) yield return null;

        // Hapus tombol
        foreach (Transform t in _pilihanRowGO.transform) Destroy(t.gameObject);

        // Catat ke GameState
        var gs = GameState.Instance;
        if (gs != null)
        {
            gs.AddChoice(2, label, kategori);
            if (kategori == "AMAN" && berikanBonusLaporSaatAman)
            {
                gs.score += GameState.SCORE_LAPOR;
                Debug.Log($"[HalteDialog] Bonus LAPOR +{GameState.SCORE_LAPOR}");
            }
            if (kategori == "BAHAYA" && kurangiNyawaSaatBahaya)
            {
                gs.lives = Mathf.Max(0, gs.lives - 1);
                Debug.Log($"[HalteDialog] Nyawa -1 (sisa {gs.lives})");
            }
        }

        AudioClip sfx = kategori switch
        {
            "AMAN"   => AudioManager.Instance?.sfxAman,
            "RAGU"   => AudioManager.Instance?.sfxRagu,
            "BAHAYA" => AudioManager.Instance?.sfxBahaya,
            _        => null
        };
        if (sfx != null) AudioManager.Instance.sfxSource.PlayOneShot(sfx);

        // Voice-Driven Action — kalau pilihan pemicu (default AMAN = tolak tegas + suara
        // keras) dipilih, mainkan Voice Meter. Pemain harus menahan tombol / berteriak
        // sampai meter MERAH sebelum reaksi muncul. Konsisten dgn AngkotSentuhScene.
        if (aktifkanVoiceMeter && kategori == kategoriPemicu)
            yield return JalankanVoiceMeter();

        // Tampilkan reaksi
        // FASE 3 — REAKSI: tampilkan dampak pilihan + halte BG dari BarisDialog reaksi.
        if (reaksi != null && reaksi.latarSprite != null)
            SetPhaseSprite(reaksi.latarSprite);
        if (_speakerText != null)
            _speakerText.text = boxNamaUppercase ? "REAKSI" : "Reaksi";
        if (_portraitImg != null)
        {
            var sp = GetPortraitForSpeaker("Narasi");
            _portraitImg.sprite  = sp;
            _portraitImg.enabled = false; // potret/sprite profil disembunyikan dari box dialog
        }
        // Animasi mengetik reaksi
        yield return KetikTeks(reaksi != null ? (reaksi.teks ?? "") : "");

        // Tombol Lanjut
        bool lanjut = false;
        BuatTombolPilihan(tombolLanjutTeks, warnaNetral, () => { lanjut = true; });
        while (!lanjut) yield return null;

        // Cleanup & callback
        if (_canvasGO != null) Destroy(_canvasGO);
        _onSelesai?.Invoke();
    }

    // ══════════════════════════════════════════════════════════════════════
    // VOICE METER — mini-game teriak (Voice-Driven Action).
    // Pemain MENAHAN tombol / berteriak ke mikrofon sampai meter MERAH dan
    // bertahan beberapa detik. Tampilan & aturan mengikuti AngkotSentuhScene.
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator JalankanVoiceMeter()
    {
        if (_hintText != null) _hintText.gameObject.SetActive(false);
        _holdTeriak = false;

        // Sembunyikan kotak dialog VN supaya layar teriak bersih & fokus.
        if (_dialogBoxGO != null) _dialogBoxGO.SetActive(false);

        // backdrop gelap fokus
        var ov = new GameObject("VoiceOverlay");
        ov.transform.SetParent(_canvasGO.transform, false);
        var ovImg = ov.AddComponent<Image>();
        ovImg.color = new Color(0.02f, 0.01f, 0.04f, 0.9f);
        Stretch(ovImg.rectTransform);

        // Kartu panel tengah sebagai bingkai fokus mini-game (dekoratif).
        var kartu = new GameObject("VoiceKartu");
        kartu.transform.SetParent(ov.transform, false);
        var kartuImg = kartu.AddComponent<Image>();
        kartuImg.sprite = GetRoundedSprite(); kartuImg.type = Image.Type.Sliced;
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
        barImg.sprite = GetRoundedSprite(); barImg.type = Image.Type.Sliced;
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

        // Indikator TAHAN — progress berapa lama suara sudah di zona MERAH.
        var holdLabel = BuatTeks(ov.transform, "HoldLabel", "TAHAN SUARA DI ZONA MERAH!", 18,
            new Color(1f, 0.85f, 0.3f, 1f), FontStyles.Bold);
        holdLabel.alignment = TextAlignmentOptions.Center;
        var hlrt = holdLabel.rectTransform;
        hlrt.anchorMin = new Vector2(0.15f, 0.475f); hlrt.anchorMax = new Vector2(0.85f, 0.498f);
        hlrt.offsetMin = Vector2.zero; hlrt.offsetMax = Vector2.zero;

        var holdBg = new GameObject("HoldBg");
        holdBg.transform.SetParent(ov.transform, false);
        var holdBgImg = holdBg.AddComponent<Image>();
        holdBgImg.sprite = GetRoundedSprite(); holdBgImg.type = Image.Type.Sliced;
        holdBgImg.color = new Color(0.08f, 0.08f, 0.10f, 1f);
        var holdBgRT = holdBg.GetComponent<RectTransform>();
        holdBgRT.anchorMin = new Vector2(0.22f, 0.45f); holdBgRT.anchorMax = new Vector2(0.78f, 0.475f);
        holdBgRT.offsetMin = Vector2.zero; holdBgRT.offsetMax = Vector2.zero;
        var holdBgOutl = holdBg.AddComponent<Outline>();
        holdBgOutl.effectColor = new Color(1f, 1f, 1f, 0.25f); holdBgOutl.effectDistance = new Vector2(2f, -2f);

        var holdFill = new GameObject("HoldFill");
        holdFill.transform.SetParent(holdBg.transform, false);
        var holdFillImg = holdFill.AddComponent<Image>();
        holdFillImg.sprite = GetRoundedSprite(); holdFillImg.type = Image.Type.Sliced;
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
        var btnGO = BuatTombolTahan(ov.transform, teksTombolTeriak, merahWarna);
        var btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.27f, 0.115f); btnRT.anchorMax = new Vector2(0.73f, 0.225f);
        btnRT.offsetMin = Vector2.zero; btnRT.offsetMax = Vector2.zero;

        // Sumber suara VOICE-DRIVEN: utamakan VoiceMeter global (mic + izin +
        // smoothing + fallback sudah dikelola terpusat & auto-spawn). Ini membuat
        // meter benar-benar digerakkan suara mikrofon, bukan cuma tombol tahan.
        var voice = VoiceMeter.Instance;
        bool voiceDriven = voice != null;

        // Legacy: hanya pakai mic sendiri kalau VoiceMeter tidak tersedia DAN
        // gunakanMikrofon di-ON (hindari konflik dua Microphone.Start di device sama).
        bool micAktif = false;
        if (!voiceDriven && gunakanMikrofon && Microphone.devices != null && Microphone.devices.Length > 0)
        {
            try
            {
                _micDevice = Microphone.devices[0];
                _micClip = Microphone.Start(_micDevice, true, 1, 44100);
                micAktif = true;
            }
            catch { micAktif = false; }
        }
        ins.text = (voiceDriven || micAktif)
            ? "TERIAK \u201CJANGAN GANGGU SAYA!\u201D ke mikrofon sampai meter MERAH! (boleh tahan tombol)"
            : instruksiTeriak;

        // ── loop mini-game ───────────────────────────────────────────────
        float level = 0f, waktuMerah = 0f;
        while (waktuMerah < tahanDetikMerah)
        {
            float dt = Time.deltaTime;
            bool hold = _holdTeriak || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);

            if (voiceDriven)
            {
                // VOICE-DRIVEN: petakan loudness mikrofon (VoiceMeter) ke skala meter.
                // Tombol TAHAN tetap berfungsi sebagai pendukung/fallback (dorong penuh).
                float lTarget = LevelDariVoiceMeter(voice);
                if (hold) lTarget = Mathf.Max(lTarget, 1f);
                level = Mathf.Lerp(level, lTarget, 1f - Mathf.Exp(-9f * dt));
            }
            else if (micAktif)
            {
                float l = BacaLoudnessMic();
                if (hold) l = Mathf.Max(l, 1f);
                level = Mathf.Lerp(level, l, 1f - Mathf.Exp(-9f * dt));
            }
            else
            {
                level += (hold ? isiRate : -surutRate) * dt;
            }
            level = Mathf.Clamp01(level);

            markRT.anchorMin = new Vector2(level, -0.18f);
            markRT.anchorMax = new Vector2(level,  1.18f);

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
        kImg.sprite = GetRoundedSprite(); kImg.type = Image.Type.Sliced; kImg.color = warna;
        var kLe = kotak.AddComponent<LayoutElement>();
        kLe.preferredWidth = 44f; kLe.preferredHeight = 28f;
        kLe.flexibleWidth = 0f;

        var lab = BuatTeks(row.transform, "Teks", teks, 20, Color.white, FontStyles.Normal);
        lab.alignment = TextAlignmentOptions.MidlineLeft;
        var labLe = lab.gameObject.AddComponent<LayoutElement>();
        labLe.flexibleWidth = 1f;
    }

    // Tombol "TAHAN UNTUK TERIAK" — pakai EventTrigger supaya bisa tahan-lepas.
    GameObject BuatTombolTahan(Transform parent, string label, Color warna)
    {
        var go = new GameObject("TombolTahan");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.sprite = GetRoundedSprite(); img.type = Image.Type.Sliced; img.color = warna;
        var outl = go.AddComponent<Outline>();
        outl.effectColor = new Color(1f, 0.85f, 0.3f, 0.7f); outl.effectDistance = new Vector2(3f, -3f);

        var t = BuatTeks(go.transform, "Label", label, 24, Color.white, FontStyles.Bold);
        t.alignment = TextAlignmentOptions.Center;
        var trt = t.rectTransform;
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(12f, 4f); trt.offsetMax = new Vector2(-12f, -4f);

        var et = go.AddComponent<EventTrigger>();
        TambahTrigger(et, EventTriggerType.PointerDown, () => _holdTeriak = true);
        TambahTrigger(et, EventTriggerType.PointerUp,   () => _holdTeriak = false);
        TambahTrigger(et, EventTriggerType.PointerExit, () => _holdTeriak = false);
        return go;
    }

    static void TambahTrigger(EventTrigger et, EventTriggerType tipe, Action aksi)
    {
        var entry = new EventTrigger.Entry { eventID = tipe };
        entry.callback.AddListener(_ => aksi());
        et.triggers.Add(entry);
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

    // Petakan level VoiceMeter global (threshold-relatif) ke skala meter Halte
    // (ambangKuning / ambangMerah) supaya zona Normal/Sedang/KERAS konsisten
    // dengan suara mikrofon nyata.
    float LevelDariVoiceMeter(VoiceMeter vm)
    {
        if (vm == null) return 0f;
        float n = vm.NormalizedLevel;
        if (n >= vm.thresholdLoud)
            return Mathf.Lerp(ambangMerah, 1f, Mathf.InverseLerp(vm.thresholdLoud, 1f, n));
        if (n >= vm.thresholdMedium)
            return Mathf.Lerp(ambangKuning, ambangMerah, Mathf.InverseLerp(vm.thresholdMedium, vm.thresholdLoud, n));
        if (n >= vm.thresholdNormal)
            return Mathf.Lerp(0f, ambangKuning, Mathf.InverseLerp(vm.thresholdNormal, vm.thresholdMedium, n));
        return 0f;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    void BuatTombolPilihan(string teks, Color warna, Action onClick)
    {
        var go = new GameObject("Tombol_" + teks);
        go.transform.SetParent(_pilihanRowGO.transform, false);
        var img = go.AddComponent<Image>();
        img.sprite = GetRoundedSprite();
        img.color  = warna;
        img.type   = Image.Type.Sliced;
        var outl = go.AddComponent<Outline>();
        outl.effectColor    = new Color(1f, 1f, 1f, 0.35f);
        outl.effectDistance = new Vector2(2f, -2f);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.highlightedColor = new Color(Mathf.Min(1f, warna.r * 1.18f), Mathf.Min(1f, warna.g * 1.18f), Mathf.Min(1f, warna.b * 1.18f), warna.a);
        colors.pressedColor     = new Color(warna.r * 0.85f, warna.g * 0.85f, warna.b * 0.85f, warna.a);
        btn.colors = colors;
        btn.onClick.AddListener(() =>
        {
            AudioManager.Instance?.Click();
            onClick?.Invoke();
        });

        var t = BuatTeks(go.transform, "Label", teks, 22, Color.white, FontStyles.Bold);
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        var trt = t.rectTransform;
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(12f, 4f);
        trt.offsetMax = new Vector2(-12f, -4f);
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
        tmp.text      = content;
        tmp.fontSize  = size;
        tmp.color     = color;
        tmp.fontStyle = style;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
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
            if      (x<radius && y<radius)               { int dx=radius-x, dy=radius-y; inside = dx*dx+dy*dy <= radius*radius; }
            else if (x>=size-radius && y<radius)         { int dx=x-(size-1-radius), dy=radius-y; inside = dx*dx+dy*dy <= radius*radius; }
            else if (x<radius && y>=size-radius)         { int dx=radius-x, dy=y-(size-1-radius); inside = dx*dx+dy*dy <= radius*radius; }
            else if (x>=size-radius && y>=size-radius)   { int dx=x-(size-1-radius), dy=y-(size-1-radius); inside = dx*dx+dy*dy <= radius*radius; }
            tex.SetPixel(x, y, inside ? (Color)w : (Color)c);
        }
        tex.Apply();
        _roundedSprite = Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius,radius,radius,radius));
        return _roundedSprite;
    }

    // ══════════════════════════════════════════════════════════════════════
    // LIVE EDIT — terapkan perubahan Inspector ke scene halte saat aktif.
    // Dipanggil tiap frame jika liveEditHalte=true, atau dari ContextMenu.
    // Hanya bekerja jika scene halte sedang ditampilkan (Mulai() sudah dipanggil).
    // ══════════════════════════════════════════════════════════════════════
    public void ApplyHalteCustomization()
    {
        if (_canvasGO == null) return;   // scene halte belum dibangun

        // ── Rara ────────────────────────────────────────────────────────
        if (_rRaraRT != null)
        {
            _rRaraRT.anchoredPosition = raraPos;
            _rRaraRT.sizeDelta        = raraSize;
        }
        if (_rRaraImg != null)
        {
            if (raraSprite != null) _rRaraImg.sprite = raraSprite;
            _rRaraImg.color = raraSprite != null ? raraTint : _rRaraImg.color;
        }

        // ── Pria Asing ──────────────────────────────────────────────────
        if (_rPriaRT != null)
        {
            _rPriaRT.anchoredPosition = priaPos;
            _rPriaRT.sizeDelta        = priaSize;
        }
        if (_rPriaImg != null)
        {
            if (priaAsingSprite != null) _rPriaImg.sprite = priaAsingSprite;
            _rPriaImg.color = priaAsingSprite != null ? priaTint : _rPriaImg.color;
        }
        // ── Halte BG (sprite tunggal) ────────────────────────────
        // PRIORITAS: runtime override (per-fase / per-baris) > halteBackgroundSprite default.
        // Tanpa cek ini, override di SetPhaseSprite akan langsung ditimpa balik
        // oleh ApplyHalteCustomization yang dipanggil tiap frame oleh HalteLiveEditTicker.
        if (_rHalteBgImg != null)
        {
            // Saat ada runtime override (per-fase / per-baris), paksa fullscreen stretch
            // tanpa preserveAspect — sprite per-baris harus benar-benar jadi latar belakang.
            bool adaOverride = _rHalteBgRuntimeOverride != null;
            Sprite spriteAktif = adaOverride ? _rHalteBgRuntimeOverride : halteBackgroundSprite;
            if (spriteAktif != null)
            {
                _rHalteBgImg.sprite         = spriteAktif;
                _rHalteBgImg.color          = halteBackgroundTint;
                _rHalteBgImg.preserveAspect = adaOverride ? false : halteBackgroundPreserveAspect;
            }
        }
        if (_rHalteBgRT != null)
        {
            // Saat override aktif → force stretch fullscreen (abaikan halteBackgroundPos/Size).
            if (_rHalteBgRuntimeOverride != null)
            {
                _rHalteBgRT.anchorMin = Vector2.zero;
                _rHalteBgRT.anchorMax = Vector2.one;
                _rHalteBgRT.offsetMin = Vector2.zero;
                _rHalteBgRT.offsetMax = Vector2.zero;
            }
            else
            {
                ApplyHalteBgRect(_rHalteBgRT);
            }
        }
        // ── Bagian-bagian halte ────────────────────────────────────────
        ApplyBagian(_rAtap,       atapSprite,       atapPos,       atapSize,       warnaAtap);
        ApplyBagian(_rTiangKiri,  tiangKiriSprite,  tiangKiriPos,  tiangKiriSize,  warnaTiang);
        ApplyBagian(_rTiangKanan, tiangKananSprite, tiangKananPos, tiangKananSize, warnaTiang);
        ApplyBagian(_rBangku,     bangkuSprite,     bangkuPos,     bangkuSize,     warnaTiang);
        ApplyBagian(_rPapan,      papanInfoSprite,  papanInfoPos,  papanInfoSize,  warnaPapanInfo);

        // ── Label papan info ───────────────────────────────────────────
        if (_rPapanLabel != null)
        {
            _rPapanLabel.text     = papanInfoText;
            _rPapanLabel.fontSize = papanInfoFontSize;
        }
        if (_rPapanLabelRT != null)
        {
            _rPapanLabelRT.anchoredPosition = papanInfoPos;
            _rPapanLabelRT.sizeDelta        = papanInfoSize;
        }

        // ── Props tambahan ─────────────────────────────────────────────
        if (_rProps != null && propsTambahan != null)
        {
            int n = Mathf.Min(_rProps.Count, propsTambahan.Count);
            for (int i = 0; i < n; i++)
            {
                var img = _rProps[i];
                var p   = propsTambahan[i];
                if (img == null || p == null) continue;

                if (p.sprite != null) img.sprite = p.sprite;
                img.color          = p.warna;
                img.preserveAspect = p.jagaAspek;

                var rt = img.rectTransform;
                rt.sizeDelta        = p.ukuran;
                rt.anchoredPosition = p.posisi;
            }
        }

        // ── LIVE EDIT BOX DIALOG (panel/portrait/banner/text/hint/choices) ──
        ApplyBoxDialogLive();
    }

    // Live-apply semua parameter Box Dialog ke runtime UI tiap frame.
    void ApplyBoxDialogLive()
    {
        if (!_liveEditLogged)
        {
            _liveEditLogged = true;
            Debug.Log("[HalteDialog] ✓ LIVE EDIT BOX DIALOG aktif — ubah field 'box*' di Inspector saat Play untuk lihat hasilnya langsung.");
        }

        if (_bxPanelRT != null)
        {
            _bxPanelRT.anchorMin = new Vector2(boxPanelCenterX - boxPanelWidth  * 0.5f,
                                               boxPanelCenterY - boxPanelHeight * 0.5f);
            _bxPanelRT.anchorMax = new Vector2(boxPanelCenterX + boxPanelWidth  * 0.5f,
                                               boxPanelCenterY + boxPanelHeight * 0.5f);
            _bxPanelRT.offsetMin = Vector2.zero;
            _bxPanelRT.offsetMax = Vector2.zero;
        }
        if (_bxPanelImg != null && panelSprite != null)
        {
            _bxPanelImg.sprite = panelSprite;
            _bxPanelImg.color  = panelTint;
        }

        if (_bxPortraitRT != null)
        {
            _bxPortraitRT.anchorMin = new Vector2(
                boxPortraitCenterX - boxPortraitW * 0.5f,
                boxPortraitCenterY - boxPortraitH * 0.5f);
            _bxPortraitRT.anchorMax = new Vector2(
                boxPortraitCenterX + boxPortraitW * 0.5f,
                boxPortraitCenterY + boxPortraitH * 0.5f);
            _bxPortraitRT.offsetMin = _bxPortraitRT.offsetMax = Vector2.zero;
        }
        if (_portraitImg != null)
        {
            _portraitImg.preserveAspect = boxPortraitPreserveAspect;
        }

        if (_bxSpeakerRT != null)
        {
            _bxSpeakerRT.anchorMin = boxBannerAnchorMin;
            _bxSpeakerRT.anchorMax = boxBannerAnchorMax;
        }
        if (_speakerText != null)
        {
            _speakerText.fontSize = boxNamaFontSize;
            _speakerText.color    = boxNamaColor;
        }

        if (_bxBodyRT != null)
        {
            _bxBodyRT.anchorMin = boxTextAnchorMin;
            _bxBodyRT.anchorMax = boxTextAnchorMax;
        }
        if (_dialogText != null)
        {
            _dialogText.fontSize = boxTextFontSize;
            _dialogText.color    = boxTextColor;
        }

        if (_bxHintRT != null)
        {
            _bxHintRT.anchorMin = new Vector2(boxHintCenterX - boxHintSizeW * 0.5f,
                                              boxHintCenterY - boxHintSizeH * 0.5f);
            _bxHintRT.anchorMax = new Vector2(boxHintCenterX + boxHintSizeW * 0.5f,
                                              boxHintCenterY + boxHintSizeH * 0.5f);
        }
        if (_hintText != null)
        {
            _hintText.fontSize = boxHintFontSize;
            _hintText.color    = boxHintColor;
            _hintText.text     = boxHintText;
        }

        if (_bxChoicesRT != null)
        {
            _bxChoicesRT.anchorMin = new Vector2(boxPanelCenterX - boxPanelWidth * 0.5f,
                                                 boxPanelCenterY + boxPanelHeight * 0.5f);
            _bxChoicesRT.anchorMax = new Vector2(boxPanelCenterX + boxPanelWidth * 0.5f,
                                                 boxPanelCenterY + boxPanelHeight * 0.5f);
        }
    }

    void ApplyBagian(Image img, Sprite spr, Vector2 pos, Vector2 size, Color fallbackColor)
    {
        if (img == null) return;
        if (spr != null)
        {
            img.sprite         = spr;
        img.color          = Color.white;
        img.preserveAspect = false;
            img.type           = Image.Type.Simple;
        }
        else
        {
            img.sprite = null;
            img.color  = fallbackColor;
        }
        var rt = img.rectTransform;
        rt.sizeDelta        = size;
        rt.anchoredPosition = pos;
    }

    // Set anchor/size sprite halte sesuai mode stretch atau pos+size manual.
    void ApplyHalteBgRect(RectTransform rt)
    {
        if (rt == null) return;
        if (halteBackgroundStretchFull)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }
        else
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta        = halteBackgroundSize;
            rt.anchoredPosition = halteBackgroundPos;
        }
    }

    void Update()
    {
        // Heartbeat diagnostik tiap detik supaya user bisa konfirmasi Update jalan
        _hbTimer += Time.unscaledDeltaTime;
        if (_hbTimer >= 1f)
        {
            _hbTimer = 0f;
            Debug.Log($"[HalteDialog] HEARTBEAT: gameObject.activeInHierarchy={gameObject.activeInHierarchy}, " +
                      $"liveEditHalte={liveEditHalte}, _canvasGO={(_canvasGO != null ? "EXISTS" : "NULL")}, " +
                      $"_bxPanelRT={(_bxPanelRT != null ? "EXISTS" : "NULL")}, " +
                      $"boxPanelHeight={boxPanelHeight:F3}, boxNamaColor=#{ColorUtility.ToHtmlStringRGB(boxNamaColor)}");
        }

        if (liveEditHalte) ApplyHalteCustomization();
    }

    // LateUpdate juga — kalau layout system Unity sempat reset anchor di Update,
    // di LateUpdate kita override lagi. Ini memastikan box dialog selalu update.
    void LateUpdate()
    {
        if (liveEditHalte && _canvasGO != null) ApplyBoxDialogLive();
    }

#if UNITY_EDITOR
    // Dipanggil Editor SETIAP kali Inspector field berubah (termasuk saat Play).
    // Apply langsung tanpa nunggu Update tick → live edit instan.
    void OnValidate()
    {
        if (!Application.isPlaying) return;
        if (!liveEditHalte) return;
        // Defer ke akhir frame karena OnValidate kadang dipanggil di tengah serialize.
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            if (_canvasGO == null)
            {
                Debug.LogWarning("[HalteDialog] OnValidate dipanggil tapi _canvasGO masih NULL — fase Halte belum aktif. Edit field akan terapply nanti saat fase Halte dimulai.");
                return;
            }
            Debug.Log($"[HalteDialog] OnValidate → ApplyHalteCustomization (boxPanelHeight={boxPanelHeight:F3})");
            ApplyHalteCustomization();
        };
    }
#endif

    // Heartbeat sekali untuk konfirmasi live-edit aktif. Toggle off di production.
    bool _liveEditLogged = false;
    float _hbTimer = 0f;
    void OnEnable() { _liveEditLogged = false; _hbTimer = 0.9f; }

    [ContextMenu("▶ Apply Halte Customization")]
    void Ctx_ApplyHalte() => ApplyHalteCustomization();

    [ContextMenu("▶ Reset Dialog Intro & Awal ke Default")]
    void Ctx_ResetDialogDefault()
    {
        dialogIntro = new List<BarisDialog>
        {
            new BarisDialog { teks = "Akhirnya sampai halte. Tapi... belum ada angkot satu pun yang lewat." },
            new BarisDialog { teks = "Rara duduk menunggu. Suasana sepi, hanya terdengar suara daun tertiup angin." },
            new BarisDialog { teks = "Tak lama, ada motor berhenti di seberang. Seorang pria asing turun dan menatap Rara dari kejauhan..." },
            new BarisDialog { teks = "Pelan-pelan, pria itu berjalan mendekati halte." }
        };
        dialogAwal = new List<BarisDialog>
        {
            new BarisDialog { teks = "Hai dek, kenalan dong! Om dari tadi lihat kamu sendirian di sini." },
            new BarisDialog { teks = "Mau berangkat sekolah ya? Om kebetulan searah lho. Bareng om aja, biar lebih cepat sampai." },
            new BarisDialog { teks = "Eh, nomor HP kamu berapa? Nanti om kabarin kalau om mau jemput pulang sekolah ya." }
        };
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        Debug.Log($"[HalteDialog] Dialog di-reset ke default. Intro={dialogIntro.Count} baris, Awal={dialogAwal.Count} baris.");
    }

#if UNITY_EDITOR
    // ══════════════════════════════════════════════════════════════════════
    // EDITOR HELPERS — auto-load sprite box dialog & portrait
    // ══════════════════════════════════════════════════════════════════════
    void Reset()
    {
        TryLoadSprites();
    }

    [ContextMenu("▶ Muat Sprite Box Dialog + Portrait")]
    void MuatSpriteMenu()
    {
        TryLoadSprites();
        Debug.Log($"[HalteDialog] panelSprite={(panelSprite != null ? panelSprite.name : "null")} " +
                  $"raraSprite={(raraSprite != null ? raraSprite.name : "null")} " +
                  $"priaAsingSprite={(priaAsingSprite != null ? priaAsingSprite.name : "null")}");
        UnityEditor.EditorUtility.SetDirty(this);
    }

    void TryLoadSprites()
    {
        if (panelSprite == null && !string.IsNullOrEmpty(panelSpritePath))
        {
            var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/" + panelSpritePath);
            if (sp != null) panelSprite = sp;
        }
        // Coba ambil portraitRara dari Day1Intro di scene
        if (raraSprite == null)
        {
            var day1 = FindFirstObjectByType<Day1Intro>();
            if (day1 != null && day1.portraitRara != null) raraSprite = day1.portraitRara;
        }
    }
#endif
}
