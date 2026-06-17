using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Mengontrol alur Hari 1: Jalan Kaki ke Sekolah.
/// Mesin state side-scroller + sistem encounter.
///
/// Phase: intro → tutorial → walking → encounter1 → path_choice
///        → walking2 → encounter2 → walking3 → encounter3 → educard → complete
///
/// Referensi dari: js/scenes/Day1.js versi web asli
///
/// Setup di Inspector:
///   player          → GameObject player (ada script player.cs)
///   dialogManager   → DialogManager di scene ini
///   hudManager      → HUDManager
///   pathChoicePanel → Panel UI pemilihan jalan (aman vs berbahaya)
///   npcObject       → GameObject NPC asing (siluet)
///   eduCardPanel    → Panel kartu edukasi akhir
///   ... (lihat komentar tiap field)
/// </summary>
public class Day1Controller : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // KONFIGURASI PILIHAN DIALOG (dapat diedit dari Inspector)
    // ══════════════════════════════════════════════════════════════════════

    /// Satu baris dialog naratif/pembicara (tanpa pilihan).
    [System.Serializable]
    public class DialogLine
    {
        [Tooltip("Nama pembicara yang tampil di banner")]
        public string speaker   = "Narasi";
        [Tooltip("Foto/portrait pembicara (opsional)")]
        public Sprite portrait;
        [TextArea(2, 4)]
        public string text      = "Isi dialog...";
    }

    /// Satu pilihan yang bisa diklik pemain.
    [System.Serializable]
    public class ChoiceConfig
    {
        [Tooltip("Teks yang tampil di tombol pilihan")]
        [TextArea(1, 3)]
        public string label       = "Teks pilihan...";

        [Tooltip("Kategori: AMAN | RAGU | BAHAYA")]
        public string category    = "AMAN";

        [Tooltip("Centang untuk pakai poin kustom. Jika tidak dicentang, poin ikuti kategori (AMAN=100, RAGU=50, BAHAYA=0)")]
        public bool   gunakanPoinKustom = false;

        [Tooltip("Poin kustom — hanya berlaku jika 'Gunakan Poin Kustom' dicentang")]
        public int    poinKustom = 0;

        [Tooltip("Teks feedback yang muncul setelah pilihan ini dipilih")]
        [TextArea(2, 4)]
        public string feedbackText  = "Pesan feedback...";
    }

    /// Konfigurasi lengkap satu encounter — dialog pembuka + pilihan Rara.
    [System.Serializable]
    public class EncounterConfig
    {
        [Tooltip("Nama encounter — hanya untuk label di Inspector")]
        public string encounterName = "Encounter";

        [Tooltip("Baris dialog sebelum pilihan muncul")]
        public DialogLine[] dialogSebelumPilihan;

        [Tooltip("Teks pertanyaan yang muncul di baris pilihan (speaker = Rara)")]
        [TextArea(1, 2)]
        public string pertanyaanRara = "Gimana Rara harus merespons?";

        [Tooltip("Foto Rara yang tampil di baris pilihan")]
        public Sprite portraitRara;

        [Tooltip("Daftar pilihan yang bisa dipilih pemain")]
        public ChoiceConfig[] pilihan;

        [Tooltip("Tip keselamatan singkat 1 kalimat. Muncul sebagai kartu mini setelah pilihan. Kosongkan untuk menyembunyikan kartu.")]
        [TextArea(1, 3)]
        public string tipKeselamatan = "";
    }

    // ── Referensi ──────────────────────────────────────────────────────────
    [Header("Referensi Utama")]
    public GameObject   player;
    public DialogManager dialogManager;
    public HUDManager   hudManager;

    [Header("Dialog Bersama (Tutorial / Encounter)")]
    [Tooltip("NpcDialog untuk tutorial & encounter. Jika kosong, dicari otomatis di scene.")]
    public NpcDialog    sharedNpcDialog;

    [Header("NPC Asing")]
    public GameObject npcStranger;      // siluet NPC berbahaya
    public float      npcApproachSpeed = 0.8f;
    public float      npcSafeDistance  = 4f;   // jarak aman (unit)
    public float      npcDangerDist    = 1.5f; // jarak bahaya

    [Header("Jalur")]
    public Transform       pathSafeMarker;     // titik masuk jalan aman
    public Transform       pathDangerMarker;   // titik masuk gang sepi
    public GameObject      pathChoicePanel;    // Panel UI pilihan jalan
    [Tooltip("Komponen yang mengatur tampilan Jalan Ramai vs Gang Sepi. Drag PathEnvironment GO ke sini.")]
    public PathEnvironment pathEnvironment;    // lingkungan dua jalur
    [Tooltip("Jarak (unit dunia) yang harus Rara tempuh setelah memilih jalan sebelum box narasi jalan muncul.")]
    public float jarakNarasiSetelahPilihJalan = 2.5f;

    [Header("Zona Encounter (X position di world)")]
    [Tooltip("Posisi X tutorial (latih teriak).")]
    public float encTutorial   = 5f;
    [Tooltip("\u2460 Tantangan #1 \u2014 Paman Penawar Permen.")]
    public float encE1         = 14f;
    [Tooltip("\u2461 Tantangan #2 \u2014 Motor Nyasar tanya alamat sekolah.")]
    public float encE2         = 22f;
    [Tooltip("\u2462 Tantangan #3 \u2014 Persimpangan: Jalan Ramai vs Gang Sepi.")]
    public float encPathChoice = 30f;
    [Tooltip("[DEPRECATED] Encounter 3 lama (Pesan HP) sudah dipindah ke Day 2 ChatSim WhatsApp.\n" +
             "Set 9999 supaya tidak pernah ter-trigger. Field dipertahankan agar scene referensi lama tidak rusak.")]
    public float encE3         = 9999f;
    [Tooltip("Posisi X pemicu Edu Card penutup Hari 1.")]
    public float encEduCard    = 40f;
    [Tooltip("Posisi X akhir hari 1 (gerbang sekolah).")]
    public float encEnd        = 50f;

    [Header("Zona Segmen (label lokasi dinamis Hari 1)")]
    [Tooltip("Threshold X kapan label lokasi berubah. Harus disusun menaik (ascending).")]
    public float[] zoneThresholds = new float[] { 0f, 10f, 22f, 30f };
    [Tooltip("Nama zona untuk setiap threshold di atas (urutan harus sama).\n" +
             "Default: Depan Rumah \u2192 Lorong Pemukiman \u2192 Halte & Persimpangan \u2192 Akses Sekolah.")]
    public string[] zoneNames = new string[]
    {
        "Depan Rumah",
        "Lorong Pemukiman",
        "Halte & Persimpangan",
        "Akses Sekolah"
    };

    [Header("Panel Edu Card & Game Over")]
    public GameObject eduCardPanel;
    public Button     eduCardContinueBtn;

    [Header("Indikator Progres Perjalanan")]
    [Tooltip("Tampilkan bar progres 'Menuju Sekolah' di atas layar Hari 1.")]
    public bool   tampilkanProgres   = true;
    [Tooltip("Posisi X awal perjalanan (0% progres).")]
    public float  progressStartX     = 0f;
    [Tooltip("Posisi X tujuan (100% progres). Kosongkan/biarkan = pakai encEnd.")]
    public float  progressEndX       = 50f;
    [Tooltip("Sorting order canvas progres (di bawah dialog 990).")]
    public int    progressSortingOrder = 40;
    [Tooltip("Font opsional untuk teks progres. Kosong = font TMP default.")]
    public TMP_FontAsset progressFont;
    public Color  progressTrackColor = new Color(0.10f, 0.05f, 0.02f, 0.80f);
    public Color  progressFillColor  = new Color(0.96f, 0.45f, 0.10f, 1f);
    public Color  progressTextColor  = new Color(1f, 0.92f, 0.70f, 1f);

    [Header("Tombol Teriak (untuk yang tidak pakai mic)")]
    public Button  shoutButton;
    public Slider  shoutGauge;          // gauge teriak (0–1)
    public float   shoutFillRate = 0.5f;
    public float   shoutDecayRate = 0.3f;

    [Header("Panel Latih Suara (Voice Validation)")]
    [Tooltip("Judul panel latih suara.")]
    public string latihJudul       = "\uD83D\uDDE3  LATIH SUARAMU!";
    [TextArea(2, 5)]
    [Tooltip("Deskripsi panel. Konteks tutorial Hari 1.")]
    public string latihDeskripsi   =
        "Sebelum jalan, latih dulu suaramu!\nTERIAK KERAS = kamu lebih aman di jalan.\n\nTAHAN tombol TERIAK (atau SPACE) sampai meter PENUH!";
    [Tooltip("Label tombol tahan-teriak.")]
    public string latihTeriakLabel = "\uD83D\uDD0A  TAHAN: TERIAK!";
    [Tooltip("Teks reaksi saat meter penuh / berhasil.")]
    [TextArea(2, 4)]
    public string latihBerhasil    =
        "\u2713 HEBAT! Suaramu lantang! Kalau ada yang mengganggu di jalan, berani TERIAK minta tolong, ya!";
    [Tooltip("Window waktu (detik). 0 = tanpa batas waktu.")]
    public float  latihWaktuWindow = 15f;

    // ── Konfigurasi Encounter dipindah ke GameObject NPC ────────────────────
    // Dialog & pilihan tiap encounter kini dimiliki masing-masing NPC di scene
    // (PamanBaik / Pemotor / NpcGang) lewat komponen NpcDialog-nya sendiri.

    // ── State Machine ──────────────────────────────────────────────────────
    enum Phase
    {
        Intro, Tutorial, Walking,
        Encounter1, PathChoice,
        Walking2, Encounter2,
        Walking3, Encounter3,
        EduCard, Complete
    }

    Phase   currentPhase = Phase.Intro;
    // Property — setiap kali dialogActive di-set true/false, karakter otomatis freeze/unfreeze.
    // Dengan ini SEMUA dialog (Tutorial, Encounter 1-3, PathChoice, EduCard, PamanBaik)
    // langsung menghentikan dan melanjutkan pergerakan karakter tanpa perlu kode tambahan.
    bool _dialogActive = false;
    bool dialogActive
    {
        get => _dialogActive;
        set
        {
            _dialogActive = value;
            // Coba dari field Inspector dulu; fallback ke FindFirstObjectByType
            var p = player != null
                ? player.GetComponent<player>()
                : FindFirstObjectByType<player>();
            if (p != null) p.frozen = value;
        }
    }
    bool    pathChosen   = false;
    // Guard agar pilihan jalan (aman/gang) hanya dieksekusi SEKALI. Mencegah
    // nyawa berkurang ganda kalau ada lebih dari satu panel PathChoiceUI yang
    // sama-sama memanggil ChooseSafePath/ChooseDangerPath.
    bool    _pathResolved = false;
    // Narasi jalan (ramai/gang) ditunda: baru tampil saat Rara BERJALAN setelah
    // memilih jalan, bukan langsung saat panel pilihan ditutup.
    bool    _narasiJalanMenunggu = false;  // ada narasi jalan menunggu Rara bergerak
    float   _narasiJalanTriggerX = 0f;     // X minimal player untuk memicu narasi jalan
    bool    npcActive    = false;
    float   shoutLevel   = 0f;
    bool    shoutHeld    = false;
    bool    tutorialStarted = false;   // guard: ShowTutorial hanya dipanggil sekali
    string  lastZoneLabel   = null;     // label zona segmen aktif (null = belum di-set)

    // Indikator progres perjalanan
    GameObject       _progressCanvasGO;
    RectTransform    _progressFillRt;
    RectTransform    _progressMarkerRt;   // ikon Rara yang meluncur di atas bar
    TextMeshProUGUI  _progressPercentText;
    static Sprite    _sProgressRound;     // sprite rounded bersama (track/fill/panel)

    // Tombol TERIAK on-screen + gauge radial (untuk pemain tanpa mic / mobile)
    GameObject       _shoutCanvasGO;
    Image            _shoutGaugeImg;      // ring radial yang terisi sesuai suara
    Image            _shoutBtnImg;        // lingkaran tombol (warna berubah saat ditahan)
    TextMeshProUGUI  _shoutBtnLabel;      // ikon/teks di dalam tombol
    static Sprite    _sShoutCircle;       // sprite lingkaran penuh untuk gauge & tombol

    // Kartu Edukasi Hari 1 (dibangun runtime, bertema kayu/senja)
    GameObject       _eduCanvasGO;

    // Referensi yang di-cache di Start(), dipakai di OnDestroy untuk lepas listener.
    Day1Intro _introRef;

    [Header("Intro & Start")]
    [Tooltip("Centang jika ingin langsung mulai tanpa Day1Intro (untuk testing).")]
    public bool autoMulaiTanpaIntro = false;

    // Penjaga instance tunggal. Scene ternyata memuat DUA komponen Day1Controller
    // (satu di "day1Intro", satu di "NpcDialogCanvas"). Bila keduanya jalan, mereka
    // saling berebut: dobel set dialogActive, MulaiGame dipanggil 2x, player
    // di-freeze ganda, dua tutorial berjalan, dan state phase balapan — akibatnya
    // game macet/tidak bisa berjalan setelah main menu. Guard ini memastikan hanya
    // satu controller yang aktif; duplikatnya menonaktifkan diri di Awake sehingga
    // Start()-nya tidak pernah dijalankan.
    static Day1Controller _instanceAktif;

    /// Controller Hari 1 yang sedang aktif (dipakai sistem lain, mis. MobileControls
    /// untuk membaca level teriak demi mengisi cincin gauge tombol TERIAK).
    public static Day1Controller Aktif => _instanceAktif;

    /// Level teriak ternormalisasi (0..1) untuk visual gauge di tombol mobile.
    public float ShoutLevel01 => Mathf.Clamp01(shoutLevel);

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        if (_instanceAktif != null && _instanceAktif != this)
        {
            // Sudah ada controller aktif → matikan duplikat ini.
            Debug.LogWarning("[Day1Controller] Duplikat terdeteksi pada '" + gameObject.name +
                             "'. Komponen ini dinonaktifkan agar tidak bentrok.");
            enabled = false;   // Menonaktifkan di Awake menunda Start() selamanya
            return;
        }
        _instanceAktif = this;
    }

    // ══════════════════════════════════════════════════════════════════════
    void Start()
    {
        if (npcStranger != null)     npcStranger.SetActive(false);
        if (pathChoicePanel != null) pathChoicePanel.SetActive(false);
        if (eduCardPanel != null)    eduCardPanel.SetActive(false);
        if (shoutGauge != null)      shoutGauge.value = 0f;

        // Auto-find komponen yang belum di-assign
        if (dialogManager == null) dialogManager = FindFirstObjectByType<DialogManager>();
        if (hudManager    == null) hudManager    = HUDManager.Instance;

        // Auto-find player bila belum di-assign di Inspector. Tanpa ini,
        // CheckEncounterTriggers() langsung return (player == null) sehingga
        // tutorial & seluruh encounter Hari 1 tidak pernah terpicu.
        if (player == null)
        {
            var p = FindFirstObjectByType<player>();
            if (p != null) player = p.gameObject;
        }

        // Pasang event tombol teriak — juga sambungkan ke VoiceMeter fallback.
        // Pakai EventTrigger yang sudah ada bila tersedia supaya tidak menambah duplikat
        // component (dapat terjadi saat scene di-reload tanpa menghancurkan tombol).
        if (shoutButton != null)
        {
            var trigger = shoutButton.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null)
                trigger = shoutButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            else if (trigger.triggers != null)
                trigger.triggers.Clear(); // hapus listener lama agar tidak dobel

            AddTrigger(trigger, UnityEngine.EventSystems.EventTriggerType.PointerDown, _ =>
            {
                shoutHeld = true;
                if (VoiceMeter.Instance != null) VoiceMeter.Instance.fallbackButtonHeld = true;
            });
            AddTrigger(trigger, UnityEngine.EventSystems.EventTriggerType.PointerUp, _ =>
            {
                shoutHeld = false;
                if (VoiceMeter.Instance != null) VoiceMeter.Instance.fallbackButtonHeld = false;
            });
        }

        // Bekukan player selama Day1Intro (overlay + narasi) berlangsung.
        dialogActive = true;

        // Jika tidak ada Day1Intro di scene atau autoMulai → langsung unfreeze
        _introRef = FindFirstObjectByType<Day1Intro>();
        if (autoMulaiTanpaIntro || _introRef == null)
        {
            MulaiGame();
        }
        else
        {
            // Auto-subscribe ke event agar MulaiGame() pasti dipanggil saat intro selesai,
            // tidak bergantung pada wiring Inspector (mencegah player stuck frozen selamanya).
            // RemoveListener dulu (no-op kalau belum terpasang) untuk cegah double-subscribe.
            _introRef.onIntroSelesai.RemoveListener(MulaiGame);
            _introRef.onIntroSelesai.AddListener(MulaiGame);
        }
    }

    /// <summary>
    /// Lepas listener supaya tidak menumpuk saat scene di-reload / domain reload.
    /// </summary>
    void OnDestroy()
    {
        if (_instanceAktif == this) _instanceAktif = null;

        if (_introRef != null && _introRef.onIntroSelesai != null)
            _introRef.onIntroSelesai.RemoveListener(MulaiGame);
        if (eduCardContinueBtn != null)
            eduCardContinueBtn.onClick.RemoveListener(GoToResult);

        // Hentikan ambience suasana jalan saat scene Hari 1 dilepas.
        AudioManager.Instance?.StopAmbience();

        if (_progressCanvasGO != null) Destroy(_progressCanvasGO);
        if (_shoutCanvasGO != null) Destroy(_shoutCanvasGO);
        if (_eduCanvasGO != null) Destroy(_eduCanvasGO);
    }

    /// Dipanggil saat Day1Intro selesai (otomatis via AddListener di Start, atau dari Inspector).
    public void MulaiGame()
    {
        // Guard: cegah double-call hanya jika sudah melewati Tutorial atau lebih jauh
        if (currentPhase != Phase.Intro && currentPhase != Phase.Tutorial) return;

        if (hudManager    == null) hudManager    = HUDManager.Instance;
        if (dialogManager == null) dialogManager = FindFirstObjectByType<DialogManager>();

        hudManager?.Refresh();

        // Mulai ambience suasana jalan Hari 1 (senyap bila klip belum diisi).
        AudioManager.Instance?.PlayAmbience();

        // Bangun indikator progres perjalanan.
        BuildProgressBar();

        // Bangun tombol TERIAK on-screen + gauge suara (mic-less / mobile).
        BuildShoutButton();

        // Resolusi referensi player.
        var p = player != null
            ? player.GetComponent<player>()
            : FindFirstObjectByType<player>();

        // Hanya ubah fase jika masih di Intro
        if (currentPhase == Phase.Intro)
            currentPhase = Phase.Tutorial;

        // Tampilkan panel Latih Suara DI AWAL — sebelum Rara mulai jalan.
        // Player tetap dibekukan sampai panel selesai, lalu lanjut Walking.
        if (currentPhase == Phase.Tutorial && !tutorialStarted)
        {
            tutorialStarted = true;
            if (p != null) p.frozen = true;
            StartCoroutine(ShowTutorial());
        }
        else
        {
            if (p != null) p.frozen = false;
            dialogActive = false;
        }
    }

    // ── Freeze / Resume karakter ───────────────────────────────────────────
    // Dipanggil oleh PamanBaik.cs saat NpcDialog mulai / selesai.
    // Juga bisa disambungkan dari Inspector: NpcDialog.onDialogEnd → ResumePlayer()

    /// Bekukan karakter — dipanggil saat NpcDialog (Paman Baik) mulai bermain.
    public void FreezePlayer()
    {
        dialogActive = true;
        if (player != null)
        {
            var p = player.GetComponent<player>();
            if (p != null) p.frozen = true;
        }
    }

    /// Bebaskan karakter — dipanggil saat NpcDialog (Paman Baik) selesai.
    public void ResumePlayer()
    {
        dialogActive = false;
        if (player != null)
        {
            var p = player.GetComponent<player>();
            if (p != null) p.frozen = false;
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // INDIKATOR PROGRES PERJALANAN
    // ══════════════════════════════════════════════════════════════════════

    /// Bangun bar progres "Menuju Sekolah" di atas-tengah layar (sekali saja).
    void BuildProgressBar()
    {
        if (!tampilkanProgres || _progressCanvasGO != null) return;

        _progressCanvasGO = new GameObject("Day1ProgressCanvas");
        // Jadikan child Day1Controller supaya ikut NONAKTIF otomatis saat Hari 1
        // dimatikan (transisi ke Day 2/3 single-scene). Kalau dibiarkan sebagai
        // objek root, bar "Menuju Sekolah" tetap nongol di Day 2/3 karena Update()
        // Day1Controller berhenti jalan begitu GameObject-nya disable.
        _progressCanvasGO.transform.SetParent(transform, false);
        var canvas = _progressCanvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = progressSortingOrder;
        var scaler = _progressCanvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        _progressCanvasGO.AddComponent<GraphicRaycaster>();

        // Root: panel pil membulat, DI BAWAH navbar hari (H1/H2/H3) agar tidak
        // bertumpuk dengan lingkaran hari di tengah-atas layar.
        var root = new GameObject("ProgressRoot");
        root.transform.SetParent(_progressCanvasGO.transform, false);
        var rootRt = root.AddComponent<RectTransform>();
        rootRt.anchorMin        = new Vector2(0.5f, 1f);
        rootRt.anchorMax        = new Vector2(0.5f, 1f);
        rootRt.pivot            = new Vector2(0.5f, 1f);
        rootRt.anchoredPosition = new Vector2(0f, -108f);
        rootRt.sizeDelta        = new Vector2(560f, 70f);

        // Latar panel membulat + bingkai emas (selaras tema HUD kayu/sunset).
        var panelImg = root.AddComponent<Image>();
        panelImg.sprite = GetRoundedSpriteDay1();
        panelImg.type   = Image.Type.Sliced;
        panelImg.color  = new Color(0.16f, 0.08f, 0.04f, 0.90f);
        var panelOutline = root.AddComponent<Outline>();
        panelOutline.effectColor    = new Color(0.95f, 0.72f, 0.18f, 0.85f);
        panelOutline.effectDistance = new Vector2(2f, -2f);

        // ── Baris atas: caption (kiri) + persen (kanan) ───────────────────
        var capGO = new GameObject("Caption");
        capGO.transform.SetParent(root.transform, false);
        var cap = capGO.AddComponent<TextMeshProUGUI>();
        cap.text      = "\uD83C\uDFEB  Menuju Sekolah";
        cap.fontSize  = 23;
        cap.fontStyle = FontStyles.Bold;
        cap.alignment = TextAlignmentOptions.Left;
        cap.color     = progressTextColor;
        cap.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        if (progressFont != null) cap.font = progressFont;
        var capRt = capGO.GetComponent<RectTransform>();
        capRt.anchorMin = new Vector2(0f, 1f);
        capRt.anchorMax = new Vector2(0.65f, 1f);
        capRt.pivot     = new Vector2(0f, 1f);
        capRt.offsetMin = new Vector2(20f, -34f);
        capRt.offsetMax = new Vector2(20f, -8f);

        var pctGO = new GameObject("Persen");
        pctGO.transform.SetParent(root.transform, false);
        _progressPercentText = pctGO.AddComponent<TextMeshProUGUI>();
        _progressPercentText.text      = "0%";
        _progressPercentText.fontSize  = 23;
        _progressPercentText.fontStyle = FontStyles.Bold;
        _progressPercentText.alignment = TextAlignmentOptions.Right;
        _progressPercentText.color     = new Color(1f, 0.82f, 0.30f, 1f);
        _progressPercentText.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        if (progressFont != null) _progressPercentText.font = progressFont;
        var pctRt = pctGO.GetComponent<RectTransform>();
        pctRt.anchorMin = new Vector2(0.65f, 1f);
        pctRt.anchorMax = new Vector2(1f, 1f);
        pctRt.pivot     = new Vector2(1f, 1f);
        pctRt.offsetMin = new Vector2(-20f, -34f);
        pctRt.offsetMax = new Vector2(-20f, -8f);

        // ── Track (latar bar) membulat ────────────────────────────────────
        var trackGO = new GameObject("Track");
        trackGO.transform.SetParent(root.transform, false);
        var trackImg = trackGO.AddComponent<Image>();
        trackImg.sprite = GetRoundedSpriteDay1();
        trackImg.type   = Image.Type.Sliced;
        trackImg.color  = progressTrackColor;
        var trackRt = trackGO.GetComponent<RectTransform>();
        trackRt.anchorMin = new Vector2(0f, 0f);
        trackRt.anchorMax = new Vector2(1f, 0f);
        trackRt.pivot     = new Vector2(0.5f, 0f);
        trackRt.offsetMin = new Vector2(20f, 14f);
        trackRt.offsetMax = new Vector2(-20f, 32f);

        // Fill (proporsi via anchorMax.x) membulat
        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(trackGO.transform, false);
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.sprite  = GetRoundedSpriteDay1();
        fillImg.type    = Image.Type.Sliced;
        fillImg.color   = progressFillColor;
        _progressFillRt = fillGO.GetComponent<RectTransform>();
        _progressFillRt.anchorMin = new Vector2(0f, 0f);
        _progressFillRt.anchorMax = new Vector2(0f, 1f);
        _progressFillRt.pivot     = new Vector2(0f, 0.5f);
        _progressFillRt.offsetMin = Vector2.zero;
        _progressFillRt.offsetMax = Vector2.zero;

        // Marker: ikon Rara berjalan, meluncur di ujung fill.
        var markGO = new GameObject("Marker");
        markGO.transform.SetParent(trackGO.transform, false);
        var mark = markGO.AddComponent<TextMeshProUGUI>();
        mark.text      = "\uD83D\uDEB6";
        mark.fontSize  = 30;
        mark.alignment = TextAlignmentOptions.Center;
        mark.raycastTarget = false;
        mark.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        if (progressFont != null) mark.font = progressFont;
        _progressMarkerRt = markGO.GetComponent<RectTransform>();
        _progressMarkerRt.anchorMin        = new Vector2(0f, 0.5f);
        _progressMarkerRt.anchorMax        = new Vector2(0f, 0.5f);
        _progressMarkerRt.pivot            = new Vector2(0.5f, 0.5f);
        _progressMarkerRt.sizeDelta        = new Vector2(34f, 34f);
        _progressMarkerRt.anchoredPosition = Vector2.zero;
    }

    /// Sprite kotak membulat (di-cache) untuk panel/track/fill bar progres.
    Sprite GetRoundedSpriteDay1()
    {
        if (_sProgressRound != null) return _sProgressRound;
        int size = 48, radius = 16;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            bool inside = true;
            if      (x < radius && y < radius)               { int dx = radius - x,            dy = radius - y;            inside = dx * dx + dy * dy <= radius * radius; }
            else if (x >= size - radius && y < radius)        { int dx = x - (size - 1 - radius), dy = radius - y;            inside = dx * dx + dy * dy <= radius * radius; }
            else if (x < radius && y >= size - radius)        { int dx = radius - x,            dy = y - (size - 1 - radius); inside = dx * dx + dy * dy <= radius * radius; }
            else if (x >= size - radius && y >= size - radius){ int dx = x - (size - 1 - radius), dy = y - (size - 1 - radius); inside = dx * dx + dy * dy <= radius * radius; }
            tex.SetPixel(x, y, inside ? Color.white : new Color(1f, 1f, 1f, 0f));
        }
        tex.Apply();
        _sProgressRound = Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius));
        return _sProgressRound;
    }

    // ══════════════════════════════════════════════════════════════════════
    // TOMBOL TERIAK ON-SCREEN + GAUGE RADIAL
    // Tombol bundar di kanan-bawah layar (sisi gerak ada di kiri) lengkap dgn
    // cincin gauge yang terisi mengikuti kerasnya suara. Membantu pemain tanpa
    // mikrofon / di HP untuk tetap bisa "TERIAK". Disambungkan ke shoutHeld &
    // VoiceMeter.fallbackButtonHeld — sama persis dengan jalur input lain.
    // ══════════════════════════════════════════════════════════════════════
    void BuildShoutButton()
    {
        if (_shoutCanvasGO != null) return;

        // Tombol TERIAK kini disediakan oleh controller mobile terpadu (MobileControls),
        // satu paket dengan tombol arah. Bila MobileControls ada, JANGAN bangun tombol
        // radial ini agar tidak ada dua tombol TERIAK yang tumpang tindih.
        if (MobileControls.Instance != null) return;

        // Tombol TERIAK (radial) KHUSUS perangkat sentuh (HP/tablet). Di desktop
        // disembunyikan — pemain memakai SPACE / mikrofon. Deteksi disamakan
        // dengan tombol arah mobile lewat MobileControls.ShouldShowTouchUI().
        if (!MobileControls.ShouldShowTouchUI()) return;

        _shoutCanvasGO = new GameObject("Day1ShoutCanvas");
        // Penjaga mandiri: sembunyikan kanvas TERIAK begitu bukan Hari 1 lagi.
        // Dipasang DI kanvas itu sendiri supaya tetap jalan walau GameObject
        // Day1Controller dinonaktifkan saat transisi hari (Update-nya berhenti).
        _shoutCanvasGO.AddComponent<ShoutCanvasDayGuard>();
        var canvas = _shoutCanvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 970; // di bawah dialog (990) & panel latih (980)
        var scaler = _shoutCanvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        _shoutCanvasGO.AddComponent<GraphicRaycaster>();

        // Root: kanan-bawah
        var root = new GameObject("ShoutRoot");
        root.transform.SetParent(_shoutCanvasGO.transform, false);
        var rootRt = root.AddComponent<RectTransform>();
        rootRt.anchorMin        = new Vector2(1f, 0f);
        rootRt.anchorMax        = new Vector2(1f, 0f);
        rootRt.pivot            = new Vector2(1f, 0f);
        rootRt.anchoredPosition = new Vector2(-44f, 150f);
        rootRt.sizeDelta        = new Vector2(168f, 168f);

        // Cincin track (latar gauge, lingkaran penuh gelap)
        var trackGO = new GameObject("GaugeTrack");
        trackGO.transform.SetParent(root.transform, false);
        var trackImg = trackGO.AddComponent<Image>();
        trackImg.sprite = GetCircleSpriteDay1();
        trackImg.color  = new Color(0.10f, 0.05f, 0.02f, 0.85f);
        trackImg.raycastTarget = false;
        Stretch(trackGO.GetComponent<RectTransform>());

        // Cincin gauge radial (terisi sesuai suara)
        var gaugeGO = new GameObject("GaugeFill");
        gaugeGO.transform.SetParent(root.transform, false);
        _shoutGaugeImg = gaugeGO.AddComponent<Image>();
        _shoutGaugeImg.sprite       = GetCircleSpriteDay1();
        _shoutGaugeImg.color        = new Color(0.20f, 0.78f, 0.40f, 1f);
        _shoutGaugeImg.type         = Image.Type.Filled;
        _shoutGaugeImg.fillMethod   = Image.FillMethod.Radial360;
        _shoutGaugeImg.fillOrigin   = (int)Image.Origin360.Top;
        _shoutGaugeImg.fillClockwise= true;
        _shoutGaugeImg.fillAmount   = 0f;
        _shoutGaugeImg.raycastTarget= false;
        Stretch(gaugeGO.GetComponent<RectTransform>());

        // Lingkaran tombol (di dalam cincin, lebih kecil)
        var btnGO = new GameObject("TombolTeriak");
        btnGO.transform.SetParent(root.transform, false);
        _shoutBtnImg = btnGO.AddComponent<Image>();
        _shoutBtnImg.sprite = GetCircleSpriteDay1();
        _shoutBtnImg.color  = new Color(0.91f, 0.30f, 0.24f, 1f);
        var btnRt = btnGO.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.5f);
        btnRt.anchorMax = new Vector2(0.5f, 0.5f);
        btnRt.pivot     = new Vector2(0.5f, 0.5f);
        btnRt.sizeDelta = new Vector2(124f, 124f);
        var btnOutline = btnGO.AddComponent<Outline>();
        btnOutline.effectColor    = new Color(1f, 1f, 1f, 0.35f);
        btnOutline.effectDistance = new Vector2(2f, -2f);

        // Label dalam tombol: ikon corong + teks TERIAK
        _shoutBtnLabel = new GameObject("Label").AddComponent<TextMeshProUGUI>();
        _shoutBtnLabel.transform.SetParent(btnGO.transform, false);
        _shoutBtnLabel.text      = "\uD83D\uDCE2\nTERIAK";
        _shoutBtnLabel.fontSize  = 26;
        _shoutBtnLabel.fontStyle = FontStyles.Bold;
        _shoutBtnLabel.alignment = TextAlignmentOptions.Center;
        _shoutBtnLabel.color     = Color.white;
        _shoutBtnLabel.raycastTarget = false;
        if (progressFont != null) _shoutBtnLabel.font = progressFont;
        Stretch(_shoutBtnLabel.rectTransform);

        // Wiring tahan tombol → shoutHeld + VoiceMeter fallback
        var trigger = btnGO.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        AddTrigger(trigger, UnityEngine.EventSystems.EventTriggerType.PointerDown, _ =>
        {
            shoutHeld = true;
            if (VoiceMeter.Instance != null) VoiceMeter.Instance.fallbackButtonHeld = true;
        });
        AddTrigger(trigger, UnityEngine.EventSystems.EventTriggerType.PointerUp, _ =>
        {
            shoutHeld = false;
            if (VoiceMeter.Instance != null) VoiceMeter.Instance.fallbackButtonHeld = false;
        });

        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    /// Perbarui visual tombol & gauge radial sesuai level suara.
    void UpdateShoutButtonVisual()
    {
        if (_shoutGaugeImg == null) return;

        float level = Mathf.Clamp01(shoutLevel);
        _shoutGaugeImg.fillAmount = Mathf.Lerp(_shoutGaugeImg.fillAmount, level, 0.30f);

        // Warna gauge & tombol sesuai zona suara.
        bool keras  = level >= 0.80f;
        bool sedang = level >= 0.45f && !keras;
        Color zona = keras  ? new Color(0.92f, 0.22f, 0.18f, 1f)
                   : sedang ? new Color(0.95f, 0.62f, 0.07f, 1f)
                   :          new Color(0.20f, 0.78f, 0.40f, 1f);
        _shoutGaugeImg.color = zona;

        bool ditahan = shoutHeld || Input.GetKey(KeyCode.Space)
                     || (VoiceMeter.Instance != null && VoiceMeter.Instance.fallbackButtonHeld);
        Color btnBase = new Color(0.91f, 0.30f, 0.24f, 1f);
        Color btnAktif = keras ? new Color(0.92f, 0.22f, 0.18f, 1f)
                               : new Color(0.20f, 0.78f, 0.40f, 1f);
        if (_shoutBtnImg != null)
            _shoutBtnImg.color = Color.Lerp(_shoutBtnImg.color, ditahan ? btnAktif : btnBase, 0.25f);

        // Denyut ringan saat suara KERAS untuk umpan balik kuat.
        if (_shoutBtnLabel != null)
            _shoutBtnLabel.text = keras ? "\uD83D\uDCE2\nKERAS!" : "\uD83D\uDCE2\nTERIAK";
        var rt = _shoutCanvasGO != null ? _shoutCanvasGO.transform.Find("ShoutRoot") as RectTransform : null;
        if (rt != null)
        {
            float s = keras ? 1f + Mathf.Abs(Mathf.Sin(Time.time * 10f)) * 0.06f : 1f;
            rt.localScale = Vector3.Lerp(rt.localScale, new Vector3(s, s, 1f), 0.4f);
        }
    }

    /// RectTransform stretch penuh ke parent.
    void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    /// Sprite lingkaran penuh (di-cache) untuk gauge radial & tombol.
    Sprite GetCircleSpriteDay1()
    {
        if (_sShoutCircle != null) return _sShoutCircle;
        int size = 128;
        float r = size / 2f - 1f, c = size / 2f;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = x + 0.5f - c, dy = y + 0.5f - c;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            float a = Mathf.Clamp01(r - d);              // anti-alias tepi
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        tex.Apply();
        _sShoutCircle = Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100f);
        return _sShoutCircle;
    }

    /// Perbarui lebar bar progres sesuai posisi X player.
    void UpdateProgressBar()
    {
        if (_progressFillRt == null || player == null) return;

        float endX = Mathf.Abs(progressEndX - progressStartX) > 0.01f ? progressEndX : encEnd;
        float t    = Mathf.Clamp01(Mathf.InverseLerp(progressStartX, endX, player.transform.position.x));

        _progressFillRt.anchorMax = new Vector2(t, 1f);
        _progressFillRt.offsetMin = Vector2.zero;
        _progressFillRt.offsetMax = Vector2.zero;

        // Marker Rara meluncur mengikuti ujung fill.
        if (_progressMarkerRt != null)
        {
            _progressMarkerRt.anchorMin = new Vector2(t, 0.5f);
            _progressMarkerRt.anchorMax = new Vector2(t, 0.5f);
        }

        if (_progressPercentText != null)
            _progressPercentText.text = Mathf.RoundToInt(t * 100f) + "%";
    }

    // ══════════════════════════════════════════════════════════════════════
    void Update()
    {
        // Tombol TERIAK + gauge suara khusus Hari 1 — sembunyikan begitu pemain
        // sudah pindah ke Hari 2 / Hari 3 (Day1Controller tidak ikut dinonaktifkan
        // saat transisi single-scene, jadi kanvasnya perlu disembunyikan manual).
        if (_shoutCanvasGO != null && _shoutCanvasGO.activeSelf
            && GameState.Instance != null && GameState.Instance.day != 1)
        {
            _shoutCanvasGO.SetActive(false);
        }

        // Bar progres "Menuju Sekolah" hanya untuk Hari 1 — sembunyikan saat
        // sudah masuk Hari 2 / Hari 3 (single-scene: Day1Controller tetap hidup).
        if (_progressCanvasGO != null && _progressCanvasGO.activeSelf
            && GameState.Instance != null && GameState.Instance.day != 1)
        {
            _progressCanvasGO.SetActive(false);
        }

        if (dialogActive) return;

        HandleShout();
        CheckEncounterTriggers();
        UpdateZoneSegment();

        if (currentPhase == Phase.Encounter1 && npcActive) HandleNPCApproach();
        if (currentPhase == Phase.Encounter2 && npcActive) HandleNPCApproach();
        if (currentPhase == Phase.Encounter3 && npcActive) HandleNPCApproach();

        hudManager?.UpdateScore(GameState.Instance?.score ?? 0);

        UpdateProgressBar();
    }

    // Perbarui label lokasi HUD sesuai posisi X player (zona segmen Hari 1).
    void UpdateZoneSegment()
    {
        if (player == null || hudManager == null) return;
        if (zoneThresholds == null || zoneNames == null) return;
        int n = Mathf.Min(zoneThresholds.Length, zoneNames.Length);
        if (n == 0) return;

        float px = player.transform.position.x;
        int idx = 0;
        for (int i = 0; i < n; i++)
        {
            if (px >= zoneThresholds[i]) idx = i;
            else break;
        }
        if (idx < 0) return;
        string zone = zoneNames[idx];
        if (zone == lastZoneLabel) return;
        lastZoneLabel = zone;
        hudManager.UpdateLocationCustom(zone);
    }

    // ══════════════════════════════════════════════════════════════════════
    // TUTORIAL
    // ══════════════════════════════════════════════════════════════════════

    IEnumerator ShowTutorial()
    {
        // Tutorial latih suara: panel voice-validation (tahan tombol + meter + countdown).
        yield return StartCoroutine(ShowTutorialPanelLatihSuara());
    }

    // ══════════════════════════════════════════════════════════════════════
    // TUTORIAL — Panel Voice Validation (gaya kartu LAPOR Day 2, konteks Hari 1)
    // Modal: judul + deskripsi + countdown + tombol "TAHAN: TERIAK!" + bar isi.
    // Membaca input lewat VoiceMeter (mic) atau fallback tombol/SPACE — sama
    // seperti HandleShout(). Berhasil saat meter PENUH; timeout = tetap lanjut.
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator ShowTutorialPanelLatihSuara()
    {
        dialogActive = true;
        shoutLevel = 0f;
        hudManager?.SetShoutGauge(0f);

        // ── Canvas overlay ──
        var canvasGO = new GameObject("LatihSuaraCanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 980; // di bawah dialog (990), di atas HUD
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ── Dim latar belakang ──
        var dim = new GameObject("Dim");
        dim.transform.SetParent(canvasGO.transform, false);
        var dimImg = dim.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.55f);
        var dimRt = dim.GetComponent<RectTransform>();
        dimRt.anchorMin = Vector2.zero; dimRt.anchorMax = Vector2.one;
        dimRt.offsetMin = Vector2.zero; dimRt.offsetMax = Vector2.zero;

        // ── Kartu (panel maroon membulat + bingkai emas) ──
        var card = new GameObject("Card");
        card.transform.SetParent(canvasGO.transform, false);
        var cardImg = card.AddComponent<Image>();
        cardImg.sprite = GetRoundedSpriteDay1();
        cardImg.type   = Image.Type.Sliced;
        cardImg.color = new Color(0.24f, 0.08f, 0.10f, 0.97f);
        var cardOutline = card.AddComponent<Outline>();
        cardOutline.effectColor    = new Color(0.95f, 0.72f, 0.18f, 0.90f);
        cardOutline.effectDistance = new Vector2(3f, -3f);
        var cardRt = card.GetComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 0.5f);
        cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.pivot     = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = new Vector2(820f, 470f);

        // ── Judul ──
        var judul = BuatTeksLatih(card.transform, "Judul", latihJudul, 38,
            new Color(1f, 0.45f, 0.45f, 1f), FontStyles.Bold);
        var judulRt = judul.rectTransform;
        judulRt.anchorMin = new Vector2(0f, 1f); judulRt.anchorMax = new Vector2(1f, 1f);
        judulRt.pivot     = new Vector2(0.5f, 1f);
        judulRt.offsetMin = new Vector2(40f, -86f); judulRt.offsetMax = new Vector2(-40f, -26f);

        // ── Deskripsi ──
        var desk = BuatTeksLatih(card.transform, "Deskripsi", latihDeskripsi, 22,
            new Color(1f, 1f, 0.92f, 0.95f), FontStyles.Normal);
        var deskRt = desk.rectTransform;
        deskRt.anchorMin = new Vector2(0f, 1f); deskRt.anchorMax = new Vector2(1f, 1f);
        deskRt.pivot     = new Vector2(0.5f, 1f);
        deskRt.offsetMin = new Vector2(50f, -224f); deskRt.offsetMax = new Vector2(-50f, -96f);

        // ── Tombol TAHAN: TERIAK! (membulat) ──
        var tombolGO = new GameObject("TombolTeriak");
        tombolGO.transform.SetParent(card.transform, false);
        var tombolImg = tombolGO.AddComponent<Image>();
        tombolImg.sprite = GetRoundedSpriteDay1();
        tombolImg.type   = Image.Type.Sliced;
        Color warnaTeriak  = new Color(0.91f, 0.30f, 0.24f, 1f);
        Color warnaDitekan = new Color(0.20f, 0.78f, 0.40f, 1f);
        tombolImg.color = warnaTeriak;
        var tombolOutline = tombolGO.AddComponent<Outline>();
        tombolOutline.effectColor    = new Color(1f, 1f, 1f, 0.30f);
        tombolOutline.effectDistance = new Vector2(2f, -2f);
        var tombolRt = tombolGO.GetComponent<RectTransform>();
        tombolRt.anchorMin = new Vector2(0.5f, 0.5f);
        tombolRt.anchorMax = new Vector2(0.5f, 0.5f);
        tombolRt.pivot     = new Vector2(0.5f, 0.5f);
        tombolRt.sizeDelta = new Vector2(440f, 96f);
        tombolRt.anchoredPosition = new Vector2(0f, -70f);

        var tombolLabel = BuatTeksLatih(tombolGO.transform, "Label", latihTeriakLabel, 28,
            Color.white, FontStyles.Bold);
        var tlRt = tombolLabel.rectTransform;
        tlRt.anchorMin = Vector2.zero; tlRt.anchorMax = Vector2.one;
        tlRt.offsetMin = Vector2.zero; tlRt.offsetMax = Vector2.zero;

        // Wiring tahan tombol (pointer) — sambungkan ke VoiceMeter fallback.
        bool ditekan = false;
        var trigger = tombolGO.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        AddTrigger(trigger, UnityEngine.EventSystems.EventTriggerType.PointerDown, _ =>
        {
            ditekan = true;
            if (VoiceMeter.Instance != null) VoiceMeter.Instance.fallbackButtonHeld = true;
        });
        AddTrigger(trigger, UnityEngine.EventSystems.EventTriggerType.PointerUp, _ =>
        {
            ditekan = false;
            if (VoiceMeter.Instance != null) VoiceMeter.Instance.fallbackButtonHeld = false;
        });

        // ── Bar progres teriak (membulat) ──
        var barBg = new GameObject("BarBg");
        barBg.transform.SetParent(card.transform, false);
        var barBgImg = barBg.AddComponent<Image>();
        barBgImg.sprite = GetRoundedSpriteDay1();
        barBgImg.type   = Image.Type.Sliced;
        barBgImg.color = new Color(0.10f, 0.10f, 0.12f, 1f);
        var barBgRt = barBg.GetComponent<RectTransform>();
        barBgRt.anchorMin = new Vector2(0.5f, 0f); barBgRt.anchorMax = new Vector2(0.5f, 0f);
        barBgRt.pivot     = new Vector2(0.5f, 0f);
        barBgRt.sizeDelta = new Vector2(560f, 26f);
        barBgRt.anchoredPosition = new Vector2(0f, 40f);

        var barFill = new GameObject("BarFill");
        barFill.transform.SetParent(barBg.transform, false);
        var barFillImg = barFill.AddComponent<Image>();
        barFillImg.sprite = GetRoundedSpriteDay1();
        barFillImg.type   = Image.Type.Sliced;
        barFillImg.color = new Color(0.20f, 0.78f, 0.40f, 1f);
        var barFillRt = barFill.GetComponent<RectTransform>();
        barFillRt.anchorMin = new Vector2(0f, 0f); barFillRt.anchorMax = new Vector2(0f, 1f);
        barFillRt.pivot     = new Vector2(0f, 0.5f);
        barFillRt.offsetMin = Vector2.zero; barFillRt.offsetMax = Vector2.zero;

        AudioManager.Instance?.Click();

        // ── Loop: isi meter sampai PENUH. Tanpa batas waktu — panel TIDAK
        // menutup sebelum suara KERAS tercapai. Game tak lanjut sebelum selesai. ──
        bool  berhasil = false;
        while (true)
        {
            // Sumber intensitas. Tombol TERIAK / SPACE harus SELALU berfungsi
            // (mengisi meter langsung), terlepas dari ada-tidaknya mikrofon.
            bool spaceHeld = Input.GetKey(KeyCode.Space);
            bool held      = ditekan || spaceHeld || MobileControls.ShoutHeld;
            bool micAktif  = VoiceMeter.Instance != null && VoiceMeter.Instance.MicActive;

            if (VoiceMeter.Instance != null) VoiceMeter.Instance.fallbackButtonHeld = held;

            if (held)
                // Tombol/SPACE ditahan → isi meter langsung sampai penuh.
                shoutLevel = Mathf.Min(1f, shoutLevel + shoutFillRate * Time.deltaTime);
            else if (micAktif)
                // Tidak menekan tombol → ikuti level mikrofon.
                shoutLevel = VoiceMeter.Instance.NormalizedLevel;
            else
                // Tanpa mic & tanpa tombol → meter luruh perlahan.
                shoutLevel = Mathf.Max(0f, shoutLevel - shoutDecayRate * Time.deltaTime);

            // Apakah suara sudah mencapai level KERAS (>80 dB)?
            // Jalur tombol: pakai shoutLevel langsung. Jalur mic: bandingkan
            // NormalizedLevel dengan ambang KERAS (thresholdLoud).
            bool  keras;
            float progresKeras;
            if (held)
            {
                progresKeras = Mathf.Clamp01(shoutLevel);
                keras        = shoutLevel >= 0.96f;
            }
            else if (micAktif)
            {
                float ambangKeras = Mathf.Max(0.0001f, VoiceMeter.Instance.thresholdLoud);
                progresKeras = Mathf.Clamp01(VoiceMeter.Instance.NormalizedLevel / ambangKeras);
                keras        = VoiceMeter.Instance.Level == VoiceMeter.VoiceLevel.Loud;
            }
            else
            {
                progresKeras = Mathf.Clamp01(shoutLevel);
                keras        = shoutLevel >= 0.96f;
            }

            // Umpan balik visual: warna tombol & bar isi (penuh = KERAS / >80 dB).
            tombolImg.color   = held ? warnaDitekan : warnaTeriak;
            barFillRt.anchorMax = new Vector2(progresKeras, 1f);
            barFillRt.offsetMin = Vector2.zero; barFillRt.offsetMax = Vector2.zero;
            // Warna bar mengikuti zona suara: hijau (normal) \u2192 kuning (sedang) \u2192 merah (KERAS).
            barFillImg.color = keras
                ? new Color(0.91f, 0.25f, 0.20f, 1f)
                : (progresKeras >= 0.6f
                    ? new Color(0.95f, 0.62f, 0.07f, 1f)
                    : new Color(0.20f, 0.78f, 0.40f, 1f));
            hudManager?.SetShoutGauge(shoutLevel);

            // Gerbang: hanya boleh lanjut jalan setelah mencapai suara KERAS (>80 dB).
            if (keras) { berhasil = true; break; }
            yield return null;
        }

        if (VoiceMeter.Instance != null) VoiceMeter.Instance.fallbackButtonHeld = false;

        // ── Reaksi berhasil ──
        if (berhasil)
        {
            AudioManager.Instance?.Correct();
            barFillRt.anchorMax = new Vector2(1f, 1f);
            barFillImg.color = new Color(0.91f, 0.25f, 0.20f, 1f); // merah = KERAS
            tombolImg.color = warnaDitekan;
            tombolLabel.text = "\u2713 SUARA KERAS!";
            desk.text = latihBerhasil;
            yield return new WaitForSeconds(1.6f);
        }

        Destroy(canvasGO);
        shoutLevel = 0f;
        hudManager?.SetShoutGauge(0f);
        dialogActive = false;
        currentPhase = Phase.Walking;
        // Lepas pembekuan: latih suara selesai, sekarang Rara boleh jalan.
        if (player != null)
        {
            var pl = player.GetComponent<player>();
            if (pl != null) pl.frozen = false;
        }
    }

    /// Helper kecil: buat TextMeshProUGUI untuk panel Latih Suara.
    TextMeshProUGUI BuatTeksLatih(Transform parent, string nama, string isi,
                                  int ukuran, Color warna, FontStyles gaya)
    {
        var go = new GameObject(nama);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = isi;
        tmp.fontSize  = ukuran;
        tmp.fontStyle = gaya;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = warna;
        tmp.raycastTarget = false;
        if (progressFont != null) tmp.font = progressFont;
        return tmp;
    }

    // ══════════════════════════════════════════════════════════════════════
    // ENCOUNTER TRIGGERS
    // ══════════════════════════════════════════════════════════════════════

    void CheckEncounterTriggers()
    {
        if (player == null) return;
        float px = player.transform.position.x;

        switch (currentPhase)
        {
            case Phase.Tutorial:
                // Guard: hanya panggil sekali meski player sudah melewati trigger
                if (!tutorialStarted && px >= encTutorial)
                {
                    tutorialStarted = true;
                    StartCoroutine(ShowTutorial());
                }
                break;

            case Phase.Walking:
                // Dialog encounter (Paman / Pemotor / NPC Gang) kini ditangani oleh
                // GameObject masing-masing lewat komponen NpcDialog-nya sendiri.
                // Day1Controller hanya memicu PathChoice saat Rara mencapai persimpangan.
                if (px >= encPathChoice && !pathChosen)
                    ShowPathChoice();
                break;

            case Phase.Walking2:
                // Narasi jalan baru tampil setelah Rara berjalan menjauh dari
                // persimpangan (bukan langsung saat memilih jalan).
                if (_narasiJalanMenunggu && px >= _narasiJalanTriggerX)
                {
                    _narasiJalanMenunggu = false;
                    if (GameState.Instance != null && GameState.Instance.pathChoice == "dangerous")
                        StartCoroutine(ShowDangerPathWarning());
                    else
                        StartCoroutine(ShowSafePathNarasi());
                    break;
                }
                // Setelah narasi jalan selesai — lanjut sampai Edu Card.
                if (!_narasiJalanMenunggu && px >= encEduCard && pathChosen)
                    StartCoroutine(ShowEduCard());
                break;

            case Phase.Walking3:
                // [DEPRECATED] State lama saat Encounter 3 masih ada. Tetap routing ke EduCard
                // untuk backward-compat kalau ada referensi lain yang men-set state ini.
                if (px >= encEduCard)
                    StartCoroutine(ShowEduCard());
                break;
        }
    }



    // ── Path Choice: Jalan Aman vs Gang Sepi ──────────────────────────────
    void ShowPathChoice()
    {
        if (pathChosen) return;
        pathChosen = true;
        dialogActive = true;
        currentPhase = Phase.PathChoice;

        // Langsung tampilkan panel pilihan jalan tanpa dialog narasi.
        if (pathChoicePanel != null) pathChoicePanel.SetActive(true);
    }

    /// Dipanggil oleh tombol di pathChoicePanel.
    public void ChooseSafePath()
    {
        if (_pathResolved) return;
        _pathResolved = true;
        GameState.Instance.pathChoice = "safe";
        GameState.Instance.AddChoice(1, "Pilih jalan aman yang ramai", "AMAN");
        if (pathChoicePanel != null) pathChoicePanel.SetActive(false);

        // Aktifkan tampilan Jalan Ramai
        pathEnvironment?.AktifkanJalanRamai();

        AudioManager.Instance?.Correct();

        // Lepas freeze supaya Rara bisa berjalan; box narasi jalan baru muncul
        // setelah Rara bergerak menjauh dari persimpangan (lihat CheckEncounterTriggers).
        dialogActive = false;
        currentPhase = Phase.Walking2;
        JadwalkanNarasiJalan();
    }

    IEnumerator ShowSafePathNarasi()
    {
        dialogActive = true;
        yield return StartCoroutine(TampilkanNarasiJalan(new Day1Intro.BarisNarasi[]
        {
            new Day1Intro.BarisNarasi
            {
                pembicara = "Narasi",
                teks      = "Rara memilih jalan utama. Suara klakson, tawa anak warung,\ndan langkah orang lalu-lalang membuat hatinya lebih tenang."
            },
            new Day1Intro.BarisNarasi
            {
                pembicara = "\uD83D\uDCA1 Tips Aman",
                teks      = "Pilih jalan yang RAMAI dan TERANG. Banyak orang berarti\nbanyak saksi dan tempat minta tolong kalau terjadi sesuatu."
            }
        }));
        dialogActive = false;
        currentPhase = Phase.Walking2;
    }

    public void ChooseDangerPath()
    {
        if (_pathResolved) return;
        _pathResolved = true;
        GameState.Instance.pathChoice = "dangerous";
        GameState.Instance.AddChoice(1, "Pilih gang sepi sebagai jalan pintas", "BAHAYA");
        bool alive = GameState.Instance.LoseLife();
        hudManager?.FlashHeartLost(GameState.Instance.lives);

        if (pathChoicePanel != null) pathChoicePanel.SetActive(false);

        // Aktifkan tampilan Gang Sepi (gelap)
        pathEnvironment?.AktifkanGangSepi();

        // SFX menegangkan (detak jantung) saat memilih gang sepi yang berisiko.
        AudioManager.Instance?.PlayDetakJantung();

        if (!alive)
        {
            GameOverScreen.Show();
            return;
        }

        // Lepas freeze supaya Rara bisa berjalan; box narasi gang baru muncul
        // setelah Rara melangkah masuk ke gang (lihat CheckEncounterTriggers).
        dialogActive = false;
        currentPhase = Phase.Walking2;
        JadwalkanNarasiJalan();
    }

    IEnumerator ShowDangerPathWarning()
    {
        dialogActive = true;
        yield return StartCoroutine(TampilkanNarasiJalan(new Day1Intro.BarisNarasi[]
        {
            new Day1Intro.BarisNarasi
            {
                pembicara = "Narasi",
                teks      = "Rara melangkah masuk ke gang. Lampu jalan padam,\ndinding-dinding tinggi menelan suara apa pun."
            },
            new Day1Intro.BarisNarasi
            {
                pembicara = "Narasi",
                teks      = "Hanya ada suara langkah Rara\u2026 dan langkah lain dari belakang.\nJantungnya berdegup kencang. (Nyawa \u22121)"
            },
            new Day1Intro.BarisNarasi
            {
                pembicara = "Rara (dalam hati)",
                teks      = "Harusnya aku ambil jalan ramai tadi\u2026"
            },
            new Day1Intro.BarisNarasi
            {
                pembicara = "\u26A0 Pelajaran",
                teks      = "HINDARI jalan pintas yang sepi dan gelap \u2014 walau lebih cepat,\ndi sana tak ada orang yang bisa menolong. Lebih baik lewat jalan ramai."
            }
        }));
        dialogActive = false;
        currentPhase = Phase.Walking2;
    }

    /// Setel agar narasi jalan (ramai/gang) baru tampil setelah Rara berjalan
    /// sejauh `jarakNarasiSetelahPilihJalan` dari titik memilih jalan.
    void JadwalkanNarasiJalan()
    {
        _narasiJalanMenunggu = true;
        float px = player != null ? player.transform.position.x : 0f;
        _narasiJalanTriggerX = px + Mathf.Max(0.1f, jarakNarasiSetelahPilihJalan);
    }

    /// Tampilkan narasi jalan memakai box dialog bergaya Day 1 Intro (layout sama).
    /// Fallback ke shared NpcDialog kalau Day1Intro tidak tersedia.
    IEnumerator TampilkanNarasiJalan(Day1Intro.BarisNarasi[] baris)
    {
        // Sembunyikan tombol TERIAK saat narasi agar tidak menutupi box.
        if (_shoutCanvasGO != null) _shoutCanvasGO.SetActive(false);

        bool done = false;
        if (_introRef != null)
        {
            _introRef.TampilkanNarasiKustom(baris, () => done = true);
        }
        else
        {
            // Fallback: konversi ke NpcDialog.DialogEntry kalau Day1Intro tak ada.
            var entries = new NpcDialog.DialogEntry[baris.Length];
            for (int i = 0; i < baris.Length; i++)
                entries[i] = new NpcDialog.DialogEntry
                {
                    speakerName = baris[i].pembicara,
                    text        = baris[i].teks
                };
            GetSharedDialog()?.PlayLines(entries, () => done = true);
        }

        float deadline = Time.time + 60f;
        yield return new WaitUntil(() => done || Time.time > deadline);

        if (_shoutCanvasGO != null) _shoutCanvasGO.SetActive(true);
    }


    // ══════════════════════════════════════════════════════════════════════
    // HELPER: Bangun NpcDialog.DialogEntry[] dari EncounterConfig Inspector
    // ══════════════════════════════════════════════════════════════════════

    /// Konversi EncounterConfig (Inspector) → array DialogEntry siap pakai.
    /// Semua skor, nyawa, dan feedback dikelola secara otomatis berdasarkan kategori.
    /// onAman/onRagu/onBahaya = callback tambahan khusus per encounter (achievement, dll).
    NpcDialog.DialogEntry[] BangunEncounterLines(
        EncounterConfig cfg, int hari,
        System.Action onAman   = null,
        System.Action onRagu   = null,
        System.Action onBahaya = null,
        System.Action afterFeedback = null)
    {
        var entries = new List<NpcDialog.DialogEntry>();

        // ── Baris dialog sebelum pilihan ─────────────────────────────────
        if (cfg.dialogSebelumPilihan != null)
        {
            foreach (var dl in cfg.dialogSebelumPilihan)
            {
                entries.Add(new NpcDialog.DialogEntry
                {
                    speakerName = dl.speaker,
                    profile     = dl.portrait,
                    text        = dl.text
                });
            }
        }

        // ── Baris pilihan Rara ────────────────────────────────────────────
        if (cfg.pilihan != null && cfg.pilihan.Length > 0)
        {
            var npcChoices = new NpcDialog.Choice[cfg.pilihan.Length];
            for (int i = 0; i < cfg.pilihan.Length; i++)
            {
                var pc = cfg.pilihan[i];
                string kategori         = pc.category;
                string feedbackTeks     = pc.feedbackText;
                bool   pakaiKustom      = pc.gunakanPoinKustom;
                int    nilaiKustom      = pc.poinKustom;
                string labelPilihan     = pc.label;
                string tipKeselamatan   = cfg.tipKeselamatan;

                npcChoices[i] = new NpcDialog.Choice
                {
                    label    = pc.label,
                    category = kategori,
                    onSelect = () =>
                    {
                        Debug.Log($"[Day1] onSelect dipanggil: label={labelPilihan} | kategori={kategori} | GameState={GameState.Instance != null} | HUD={HUDManager.Instance != null}");

                        // SFX per kategori (AMAN/RAGU/BAHAYA)
                        AudioManager.Instance?.PlayKategori(kategori);

                        // Hitung poin — kustom hanya jika dicentang di Inspector
                        int poinDapat;
                        if (pakaiKustom)
                        {
                            GameState.Instance?.AddChoice(hari, labelPilihan, kategori, nilaiKustom);
                            poinDapat = nilaiKustom;
                        }
                        else
                        {
                            GameState.Instance?.AddChoice(hari, labelPilihan, kategori);
                            poinDapat = kategori == "AMAN"  ? GameState.SCORE_AMAN
                                      : kategori == "RAGU"  ? GameState.SCORE_RAGU
                                      :                       GameState.SCORE_BAHAYA;
                        }

                        // Tampilkan popup skor mengambang
                        HUDManager.Instance?.ShowScorePopup(poinDapat, kategori);
                        // Paksa refresh HUD agar skor langsung terlihat
                        HUDManager.Instance?.Refresh();

                        // Konsekuensi BAHAYA: kehilangan nyawa
                        if (kategori == "BAHAYA")
                        {
                            bool masihHidup = GameState.Instance?.LoseLife() ?? false;
                            hudManager?.FlashHeartLost(GameState.Instance?.lives ?? 0);
                            HUDManager.Instance?.ShowLifeLostPopup();

                            if (!masihHidup)
                            {
                                StartCoroutine(TampilkanFeedback(feedbackTeks, kategori,
                                    () => GameOverScreen.Show()));
                                return;
                            }
                        }

                        // Callback tambahan khusus encounter (achievement, dll)
                        if (kategori == "AMAN")        onAman?.Invoke();
                        else if (kategori == "RAGU")   onRagu?.Invoke();
                        else                           onBahaya?.Invoke();

                        // Tampilkan feedback edukasi
                        StartCoroutine(TampilkanFeedback(feedbackTeks, kategori, afterFeedback, tipKeselamatan));
                    }
                };
            }

            entries.Add(new NpcDialog.DialogEntry
            {
                speakerName = "Rara",
                profile     = cfg.portraitRara,
                text        = cfg.pertanyaanRara,
                choices     = npcChoices
            });
        }

        return entries.ToArray();
    }

    /// Tampilkan satu baris feedback edukasi setelah pilihan, lalu panggil onSelesai.
    IEnumerator TampilkanFeedback(string pesan, string kategori, System.Action onSelesai = null, string tip = null)
    {
        yield return new WaitForEndOfFrame();
        if (string.IsNullOrEmpty(pesan)) { onSelesai?.Invoke(); yield break; }

        dialogActive = true;

        // Judul feedback menyertakan skor yang diperoleh / nyawa berkurang
        string infoPoin;
        switch (kategori)
        {
            case "AMAN":   infoPoin = $"  (+{GameState.SCORE_AMAN} poin)";  break;
            case "RAGU":   infoPoin = $"  (+{GameState.SCORE_RAGU} poin)";  break;
            default:       infoPoin = "  (−1 nyawa  |  +0 poin)";              break;
        }

        string judulFeedback = kategori == "AMAN"   ? $"Keputusan Tepat!{infoPoin}"
                             : kategori == "RAGU"   ? $"Perlu Lebih Tegas{infoPoin}"
                             :                        $"Keputusan Berbahaya!{infoPoin}";

        bool selesai = false;
        GetSharedDialog().PlayLines(new NpcDialog.DialogEntry[]
        {
            new NpcDialog.DialogEntry { speakerName = judulFeedback, text = pesan }
        }, () => { selesai = true; dialogActive = false; });

        yield return new WaitUntil(() => selesai);

        // Setelah feedback BAHAYA → tawarkan recovery via tombol LAPOR
        if (kategori == "BAHAYA" && (GameState.Instance?.IsAlive() ?? false))
        {
            bool laporDone = false;
            LaporButtonUI.Show(
                onLapor: () => laporDone = true,
                onSkip:  () => laporDone = true);
            yield return new WaitUntil(() => laporDone);
        }

        // Kartu edukasi mini — tip keselamatan singkat per encounter.
        if (!string.IsNullOrEmpty(tip))
            yield return StartCoroutine(TampilkanMiniEduCard(tip));

        onSelesai?.Invoke();
    }

    /// Tampilkan kartu edukasi mini berisi satu tip keselamatan, tunggu pemain klik "Mengerti".
    IEnumerator TampilkanMiniEduCard(string tip)
    {
        dialogActive = true;

        var canvasGO = new GameObject("MiniEduCardCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 996; // di atas dialog (990)
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Dim latar belakang
        var dim = new GameObject("Dim");
        dim.transform.SetParent(canvasGO.transform, false);
        var dimImg = dim.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.55f);
        var dimRt = dim.GetComponent<RectTransform>();
        dimRt.anchorMin = Vector2.zero; dimRt.anchorMax = Vector2.one;
        dimRt.offsetMin = Vector2.zero; dimRt.offsetMax = Vector2.zero;

        // Kartu
        var card = new GameObject("Card");
        card.transform.SetParent(canvasGO.transform, false);
        var cardImg = card.AddComponent<Image>();
        cardImg.color = new Color(0.13f, 0.09f, 0.05f, 0.98f);
        var cardRt = card.GetComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 0.5f);
        cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.pivot     = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = new Vector2(820f, 360f);

        // Border emas
        var border = new GameObject("Border");
        border.transform.SetParent(card.transform, false);
        var borderImg = border.AddComponent<Image>();
        borderImg.color = new Color(0.95f, 0.72f, 0.18f, 1f);
        var borderRt = border.GetComponent<RectTransform>();
        borderRt.anchorMin = new Vector2(0f, 1f);
        borderRt.anchorMax = new Vector2(1f, 1f);
        borderRt.pivot     = new Vector2(0.5f, 1f);
        borderRt.sizeDelta = new Vector2(0f, 8f);
        borderRt.anchoredPosition = Vector2.zero;

        // Judul
        var judulGO = new GameObject("Judul");
        judulGO.transform.SetParent(card.transform, false);
        var judul = judulGO.AddComponent<TextMeshProUGUI>();
        judul.text      = "Tips Keselamatan";
        judul.fontSize  = 34;
        judul.fontStyle = FontStyles.Bold;
        judul.alignment = TextAlignmentOptions.Center;
        judul.color     = new Color(1f, 0.82f, 0.18f, 1f);
        if (progressFont != null) judul.font = progressFont;
        var judulRt = judulGO.GetComponent<RectTransform>();
        judulRt.anchorMin = new Vector2(0f, 1f);
        judulRt.anchorMax = new Vector2(1f, 1f);
        judulRt.pivot     = new Vector2(0.5f, 1f);
        judulRt.offsetMin = new Vector2(40f, -86f);
        judulRt.offsetMax = new Vector2(-40f, -28f);

        // Isi tip
        var isiGO = new GameObject("Isi");
        isiGO.transform.SetParent(card.transform, false);
        var isi = isiGO.AddComponent<TextMeshProUGUI>();
        isi.text      = tip;
        isi.fontSize  = 26;
        isi.alignment = TextAlignmentOptions.Center;
        isi.color     = new Color(1f, 0.95f, 0.85f, 1f);
        if (progressFont != null) isi.font = progressFont;
        var isiRt = isiGO.GetComponent<RectTransform>();
        isiRt.anchorMin = new Vector2(0f, 0f);
        isiRt.anchorMax = new Vector2(1f, 1f);
        isiRt.offsetMin = new Vector2(50f, 110f);
        isiRt.offsetMax = new Vector2(-50f, -100f);

        // Tombol "Mengerti"
        bool lanjut = false;
        var btnGO = new GameObject("BtnMengerti");
        btnGO.transform.SetParent(card.transform, false);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.96f, 0.45f, 0.10f, 1f);
        var btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(() => lanjut = true);
        var btnRt = btnGO.GetComponent<RectTransform>();
        btnRt.anchorMin        = new Vector2(0.5f, 0f);
        btnRt.anchorMax        = new Vector2(0.5f, 0f);
        btnRt.pivot            = new Vector2(0.5f, 0f);
        btnRt.sizeDelta        = new Vector2(260f, 64f);
        btnRt.anchoredPosition = new Vector2(0f, 28f);

        var btnTxtGO = new GameObject("Label");
        btnTxtGO.transform.SetParent(btnGO.transform, false);
        var btnTxt = btnTxtGO.AddComponent<TextMeshProUGUI>();
        btnTxt.text      = "Mengerti";
        btnTxt.fontSize  = 26;
        btnTxt.fontStyle = FontStyles.Bold;
        btnTxt.alignment = TextAlignmentOptions.Center;
        btnTxt.color     = Color.white;
        if (progressFont != null) btnTxt.font = progressFont;
        var btnTxtRt = btnTxtGO.GetComponent<RectTransform>();
        btnTxtRt.anchorMin = Vector2.zero; btnTxtRt.anchorMax = Vector2.one;
        btnTxtRt.offsetMin = Vector2.zero; btnTxtRt.offsetMax = Vector2.zero;

        AudioManager.Instance?.Click();

        yield return new WaitUntil(() => lanjut);
        AudioManager.Instance?.Click();
        Destroy(canvasGO);
        dialogActive = false;
    }

    // ══════════════════════════════════════════════════════════════════════
    // EDU CARD
    // ══════════════════════════════════════════════════════════════════════

    IEnumerator ShowEduCard()
    {
        if (currentPhase == Phase.EduCard || currentPhase == Phase.Complete) yield break;
        currentPhase = Phase.EduCard;
        dialogActive = true;

        // Narasi penutup Hari 1 sebelum kartu edukasi muncul.
        bool outroDone = false;
        GetSharedDialog()?.PlayLines(new NpcDialog.DialogEntry[]
        {
            new NpcDialog.DialogEntry
            {
                speakerName = "Narasi",
                text        = "Akhirnya gerbang SMP Harapan terlihat jelas.\nRara menarik napas lega — perjalanan pertamanya sendirian selesai."
            },
            new NpcDialog.DialogEntry
            {
                speakerName = "Rara",
                text        = "\"Fyuh\u2026 ternyata di luar sana banyak hal yang harus aku waspadai.\nTapi aku berhasil sampai sini!\""
            }
        }, () => outroDone = true);
        float outroDeadline = Time.time + 45f;
        yield return new WaitUntil(() => outroDone || Time.time > outroDeadline);

        yield return new WaitForSeconds(0.5f);

        GameState.Instance.checkpointD1 = true;

        // Panel kartu edukasi serialized lama (eduCardPanel) kosong/rusak di scene,
        // jadi kita bangun kartu edukasi bertema runtime yang konsisten dengan UI Hari 1.
        BuildEduCard();
    }

    // ══════════════════════════════════════════════════════════════════════
    // KARTU EDUKASI HARI 1 (runtime — tema kayu/senja, sama dgn progres & latih)
    // ══════════════════════════════════════════════════════════════════════
    void BuildEduCard()
    {
        if (_eduCanvasGO != null) return;

        // ── Canvas penuh layar (paling atas) ──
        _eduCanvasGO = new GameObject("Day1EduCardCanvas");
        var canvas = _eduCanvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // di atas dialog (990) & panel latih (980)
        var scaler = _eduCanvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        _eduCanvasGO.AddComponent<GraphicRaycaster>();

        // ── Overlay gelap (blok input ke gameplay) ──
        var overlay = new GameObject("Overlay");
        overlay.transform.SetParent(_eduCanvasGO.transform, false);
        var overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.82f);
        Stretch(overlay.GetComponent<RectTransform>());

        // ── Kartu utama (membulat, coklat, border emas) ──
        var card = new GameObject("KartuEdukasi");
        card.transform.SetParent(_eduCanvasGO.transform, false);
        var cardImg = card.AddComponent<Image>();
        cardImg.sprite = GetRoundedSpriteDay1();
        cardImg.type   = Image.Type.Sliced;
        cardImg.color  = new Color(0.16f, 0.08f, 0.04f, 0.97f);
        var cardOutline = card.AddComponent<Outline>();
        cardOutline.effectColor    = new Color(0.95f, 0.72f, 0.18f, 0.95f);
        cardOutline.effectDistance = new Vector2(3f, -3f);
        var cardRt = card.GetComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 0.5f);
        cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.pivot     = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = new Vector2(880f, 600f);

        // ── Pita judul (membulat, emas) ──
        var pita = new GameObject("PitaJudul");
        pita.transform.SetParent(card.transform, false);
        var pitaImg = pita.AddComponent<Image>();
        pitaImg.sprite = GetRoundedSpriteDay1();
        pitaImg.type   = Image.Type.Sliced;
        pitaImg.color  = new Color(0.95f, 0.72f, 0.18f, 1f);
        pitaImg.raycastTarget = false;
        var pitaRt = pita.GetComponent<RectTransform>();
        pitaRt.anchorMin = new Vector2(0f, 1f); pitaRt.anchorMax = new Vector2(1f, 1f);
        pitaRt.pivot     = new Vector2(0.5f, 1f);
        pitaRt.offsetMin = new Vector2(28f, -96f); pitaRt.offsetMax = new Vector2(-28f, -22f);

        var judul = BuatTeksLatih(pita.transform, "Judul", "\uD83D\uDCDA  KARTU EDUKASI — HARI 1",
            34, new Color(0.18f, 0.09f, 0.02f, 1f), FontStyles.Bold);
        Stretch(judul.rectTransform);

        // ── Isi tips (krem, rata kiri) — BERBEDA sesuai jalur yang Rara pilih ──
        bool ambilGangSepi = GameState.Instance != null && GameState.Instance.pathChoice == "dangerous";
        string tips;
        if (ambilGangSepi)
        {
            // Jalur GANG SEPI (BAHAYA): Rara sempat kehilangan 1 nyawa — tekankan pelajarannya.
            tips =
                "<color=#FF8A7A><b>\uD83D\uDD34 Rara tadi lewat GANG SEPI\u2026 itu berisiko!</b></color>\n" +
                "Jalan pintas yang sepi & gelap = tempat paling rawan. Tak ada orang yang bisa menolong kalau terjadi sesuatu.\n\n" +
                "<color=#8FE3A2><b>\u2705 Lain kali, pilih JALAN RAMAI:</b></color>\n" +
                "•  Banyak orang = banyak <b>saksi</b> & tempat minta tolong.\n" +
                "•  Lebih terang, lebih mudah lari ke warung/rumah orang.\n" +
                "•  Sedikit lebih jauh tak apa \u2014 <b>selamat lebih penting</b> daripada cepat.\n\n" +
                "<color=#FFD24A><b>\uD83D\uDCE2 Kalau merasa diikuti:</b></color>  TERIAK, lari ke keramaian, dan CERITA ke orang dewasa yang dipercaya.";
        }
        else
        {
            // Jalur JALAN RAMAI (AMAN): Rara memilih tepat — kuatkan kebiasaan baik.
            tips =
                "<color=#8FE3A2><b>\u2705 Hebat! Rara memilih JALAN RAMAI.</b></color>\n" +
                "Jalan yang ramai & terang itu paling aman: banyak orang yang bisa jadi saksi dan tempat minta tolong.\n\n" +
                "<color=#FFD24A><b>\uD83D\uDEA9 Jauhi jalan pintas yang sepi!</b></color>\n" +
                "Gang gelap atau jalan sepi memang lebih cepat, tapi paling rawan \u2014 hindari walau terburu-buru.\n\n" +
                "<color=#8FE3A2><b>\uD83D\uDDDD 3 Kata Sakti kalau merasa nggak aman:</b></color>\n" +
                "•  <b>TIDAK!</b>  — kamu BERHAK menolak siapa pun.\n" +
                "•  <b>PERGI!</b>  — menjauh & lari ke tempat yang ramai.\n" +
                "•  <b>CERITA!</b> — laporkan ke orang dewasa yang dipercaya.";
        }

        var isi = BuatTeksLatih(card.transform, "Isi", tips, 23,
            new Color(1f, 1f, 0.90f, 0.97f), FontStyles.Normal);
        isi.alignment = TextAlignmentOptions.TopLeft;
        var isiRt = isi.rectTransform;
        isiRt.anchorMin = new Vector2(0f, 0f); isiRt.anchorMax = new Vector2(1f, 1f);
        isiRt.offsetMin = new Vector2(50f, 120f); isiRt.offsetMax = new Vector2(-50f, -112f);

        // ── Tombol LANJUTKAN (membulat, hijau, border putih) ──
        var tombolGO = new GameObject("TombolLanjut");
        tombolGO.transform.SetParent(card.transform, false);
        var tombolImg = tombolGO.AddComponent<Image>();
        tombolImg.sprite = GetRoundedSpriteDay1();
        tombolImg.type   = Image.Type.Sliced;
        tombolImg.color  = new Color(0.20f, 0.70f, 0.36f, 1f);
        var tombolOutline = tombolGO.AddComponent<Outline>();
        tombolOutline.effectColor    = new Color(1f, 1f, 1f, 0.35f);
        tombolOutline.effectDistance = new Vector2(2f, -2f);
        var tombolRt = tombolGO.GetComponent<RectTransform>();
        tombolRt.anchorMin = new Vector2(0.5f, 0f);
        tombolRt.anchorMax = new Vector2(0.5f, 0f);
        tombolRt.pivot     = new Vector2(0.5f, 0f);
        tombolRt.anchoredPosition = new Vector2(0f, 28f);
        tombolRt.sizeDelta = new Vector2(420f, 76f);

        var tombolLabel = BuatTeksLatih(tombolGO.transform, "Label", "\u25B6  LANJUTKAN",
            27, Color.white, FontStyles.Bold);
        Stretch(tombolLabel.rectTransform);

        var tombolBtn = tombolGO.AddComponent<Button>();
        tombolBtn.targetGraphic = tombolImg;
        tombolBtn.onClick.AddListener(GoToResult);
        tombolBtn.onClick.AddListener(() => AudioManager.Instance?.Click());

        // Pastikan ada EventSystem agar tombol bisa diklik.
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Animasi pop-in singkat.
        StartCoroutine(PopInEduCard(cardRt));

        // SFX saat kartu edukasi muncul.
        AudioManager.Instance?.Correct();
    }

    /// Animasi muncul kartu edukasi (skala 0.85 → 1.0).
    IEnumerator PopInEduCard(RectTransform rt)
    {
        if (rt == null) yield break;
        float t = 0f, dur = 0.28f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0.85f, 1f, Mathf.Clamp01(t / dur));
            rt.localScale = new Vector3(k, k, 1f);
            yield return null;
        }
        rt.localScale = Vector3.one;
    }

    public void GoToResult()
    {
        if (currentPhase == Phase.Complete) return;
        currentPhase = Phase.Complete;

        // Bersihkan kartu edukasi sebelum transisi.
        if (_eduCanvasGO != null) { Destroy(_eduCanvasGO); _eduCanvasGO = null; }

        // Single-scene: lanjut ke Hari 2 via DayTransitionManager.
        if (DayTransitionManager.Instance != null)
            DayTransitionManager.Instance.LanjutKeDay2();
        else
            SceneLoader.Instance?.LoadScene("Result1");
    }

    // ══════════════════════════════════════════════════════════════════════
    // NPC APPROACH MECHANIC
    // ══════════════════════════════════════════════════════════════════════

    void ActivateNPCAt(float x)
    {
        if (npcStranger == null) return;
        npcStranger.SetActive(true);
        npcStranger.transform.position = new Vector3(x, player.transform.position.y, 0f);
        npcActive = true;
    }

    void DismissNPC()
    {
        npcActive = false;
        if (npcStranger != null) npcStranger.SetActive(false);
    }

    void HandleNPCApproach()
    {
        if (npcStranger == null || player == null) return;

        float dist = Vector3.Distance(npcStranger.transform.position, player.transform.position);
        VoiceMeter.VoiceLevel voiceLevel = VoiceMeter.Instance != null
            ? VoiceMeter.Instance.Level
            : (shoutLevel >= 0.5f ? VoiceMeter.VoiceLevel.Loud : VoiceMeter.VoiceLevel.Silent);

        if (voiceLevel == VoiceMeter.VoiceLevel.Loud)
        {
            // TERIAK KERAS (merah >80dB) → NPC lari ketakutan (kecepatan 3× lebih cepat menjauh)
            float lariSpeed = npcApproachSpeed * 3f +
                              (VoiceMeter.Instance != null ? VoiceMeter.Instance.LoudIntensity * 2f : 0f);
            Vector3 lariDir = (npcStranger.transform.position - player.transform.position).normalized;
            npcStranger.transform.position += lariDir * lariSpeed * Time.deltaTime;

            // Jika NPC sudah cukup jauh → hilangkan
            if (dist > npcSafeDistance * 2f)
            {
                GameState.Instance.AddChoice(1, "Teriak keras mengusir orang asing", "AMAN");
                DismissNPC();
                currentPhase = Phase.Walking;
                AudioManager.Instance?.Correct();
            }
        }
        else if (voiceLevel == VoiceMeter.VoiceLevel.Medium)
        {
            // SUARA SEDANG (kuning 60-80dB) → NPC berhenti (ragu, tidak mundur tapi tidak maju)
            // tidak bergerak → beri waktu pemain untuk memilih teriak
        }
        else
        {
            // DIAM / suara normal → NPC terus mendekati
            Vector3 dekatDir = (player.transform.position - npcStranger.transform.position).normalized;
            npcStranger.transform.position += dekatDir * npcApproachSpeed * Time.deltaTime;
        }

        // Terlalu dekat dan pemain tidak teriak → kehilangan nyawa
        if (dist < npcDangerDist && !dialogActive && voiceLevel != VoiceMeter.VoiceLevel.Loud)
        {
            npcActive = false;
            npcStranger.SetActive(false);
            bool alive = GameState.Instance.LoseLife();
            GameState.Instance.AddChoice(1, "Diam saat didekati orang asing", "BAHAYA");
            hudManager?.FlashHeartLost(GameState.Instance.lives);
            if (!alive) GameOverScreen.Show();
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // SHOUT / TERIAK — dikendalikan VoiceMeter (mikrofon HP/PC)
    // ══════════════════════════════════════════════════════════════════════

    void HandleShout()
    {
        // Tombol TERIAK on-screen / SpaceBar harus SELALU berfungsi (bukan hanya
        // lewat mikrofon). Saat ditahan → langsung mode TERIAK (Loud).
        // MobileControls.ShoutHeld = tombol TERIAK di controller mobile terpadu.
        bool tombolDitahan = shoutHeld || Input.GetKey(KeyCode.Space) || MobileControls.ShoutHeld;

        if (tombolDitahan)
        {
            // Naikkan gauge cepat & paksa efek LARI, tak bergantung mikrofon.
            shoutLevel = Mathf.Min(1f, shoutLevel + shoutFillRate * Time.deltaTime);
            if (VoiceMeter.Instance != null) VoiceMeter.Instance.fallbackButtonHeld = true;
            AplikasiEfekSuara(VoiceMeter.VoiceLevel.Loud);
        }
        else if (VoiceMeter.Instance != null)
        {
            // Tidak menekan tombol → ikuti mikrofon (atau diam).
            if (VoiceMeter.Instance.fallbackButtonHeld)
                VoiceMeter.Instance.fallbackButtonHeld = false;
            shoutLevel = VoiceMeter.Instance.NormalizedLevel;
            AplikasiEfekSuara(VoiceMeter.Instance.Level);
        }
        else
        {
            // Tanpa VoiceMeter & tanpa tombol → meter luruh.
            shoutLevel = Mathf.Max(0f, shoutLevel - shoutDecayRate * Time.deltaTime);
            AplikasiEfekSuara(VoiceMeter.VoiceLevel.Normal);
        }

        if (shoutGauge != null) shoutGauge.value = shoutLevel;
        hudManager?.SetShoutGauge(shoutLevel);

        // Perbarui tombol TERIAK on-screen + gauge radial.
        UpdateShoutButtonVisual();
    }

    /// Terapkan efek kecepatan karakter sesuai level suara:
    ///   Normal (hijau)  → jalan biasa (x1.0)
    ///   Medium (kuning) → lambat / ragu (x0.55)
    ///   Loud   (merah)  → speed boost  (x1.6)
    void AplikasiEfekSuara(VoiceMeter.VoiceLevel level)
    {
        var p = player != null ? player.GetComponent<player>() : null;
        if (p == null) return;

        switch (level)
        {
            case VoiceMeter.VoiceLevel.Loud:
                p.voiceSpeedMultiplier = 1.6f;   // teriak → lari kencang
                p.forceRun = true;               // teriak → paksa animasi & state LARI
                break;
            case VoiceMeter.VoiceLevel.Medium:
                p.voiceSpeedMultiplier = 0.55f;  // suara sedang → ragu, lambat
                p.forceRun = false;
                break;
            default:
                p.voiceSpeedMultiplier = 1.0f;   // normal / diam → biasa
                p.forceRun = false;
                break;
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // HELPER
    // ══════════════════════════════════════════════════════════════════════

    /// Dapatkan atau auto-temukan NpcDialog untuk dialog bersama (tutorial & encounter).
    /// Tidak pernah mengembalikan null — buat GO baru jika tidak ada di scene.
    NpcDialog GetSharedDialog()
    {
        // Pakai referensi Inspector kalau ada (tapi pastikan masih valid & aktif-mampu)
        if (sharedNpcDialog != null) return sharedNpcDialog;

        // PRIORITAS 1: cari NpcDialog di GO yang AKTIF di hierarchy.
        // Hindari NpcDialog yang menempel di NPC inactive (mis. 'pemotor', 'paman')
        // karena StartCoroutine akan gagal di GO inactive.
        var semua = FindObjectsByType<NpcDialog>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < semua.Length; i++)
        {
            if (semua[i] != null && semua[i].gameObject.activeInHierarchy)
            {
                sharedNpcDialog = semua[i];
                return sharedNpcDialog;
            }
        }

        // PRIORITAS 2: tidak ada yang aktif \u2014 buat GO baru khusus (selalu aktif).
        var go = new GameObject("[SharedNpcDialog]");
        sharedNpcDialog = go.AddComponent<NpcDialog>();
        Debug.Log("[Day1Controller] SharedNpcDialog tidak ditemukan di GO aktif \u2014 dibuat otomatis.");
        return sharedNpcDialog;
    }

    static void AddTrigger(
        UnityEngine.EventSystems.EventTrigger trigger,
        UnityEngine.EventSystems.EventTriggerType type,
        UnityEngine.Events.UnityAction<UnityEngine.EventSystems.BaseEventData> action)
    {
        var entry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }
}

/// <summary>
/// Penjaga kanvas TERIAK Hari 1. Dipasang langsung pada GameObject kanvas
/// (Day1ShoutCanvas) sehingga Update-nya tetap berjalan meski Day1Controller
/// dinonaktifkan saat transisi ke Hari 2 / Hari 3. Tombol TERIAK hanya untuk
/// Hari 1 — begitu GameState.day != 1, kanvas dihancurkan agar tak muncul lagi.
/// </summary>
public class ShoutCanvasDayGuard : MonoBehaviour
{
    void Update()
    {
        var gs = GameState.Instance;
        if (gs != null && gs.day != 1)
            Destroy(gameObject);
    }
}
