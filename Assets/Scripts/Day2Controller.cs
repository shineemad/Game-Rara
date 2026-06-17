using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Day2Controller — Orkestrator alur Hari 2: angkot jurusan sekolah.
///
/// FOKUS: Hari 2 = visual novel naratif TAPI tetap mempertahankan bagian
/// interaktif/arcade yang seru. Default-nya mekanik interaktif AKTIF:
///   \u2022 AngkotSentuhScene.aktifkanVoiceMeter = true  (Voice Meter teriak;
///       set gunakanMikrofon=true untuk Voice-Driven via mic asli)
///   \u2022 ZonaTubuhQuiz.modeVisualNovel       = false (drag-drop ber-timer)
///   \u2022 LaporTeriakButton.modeVisualNovel   = false (tahan tombol + bisa
///       Voice-Driven via mic: set gunakanMikrofon=true \u2192 teriak ke mic)
/// Tiap komponen tetap punya mode "Visual Novel pilihan" sebagai opsi toggle.
///
/// Alur Hari 2:
///   Intro \u2192 Narasi \u2192 Halte (PRIA HALTE minta nomor) \u2192 Angkot (pilih kursi)
///   \u2192 Sentuh (PRIA ANGKOT \u2014 yang duduk di belakang \u2014 pindah & sentuh bahu)
///   \u2192 Quiz (zona tubuh) \u2192 ChatSim (HP bergetar: PRIA HALTE meng-WA Rara,
///   dapat nomor dari teman Rara) \u2192 Lapor (PRIA ANGKOT makin merapat \u2192 panggil
///   Pak Supir, tiba di sekolah) \u2192 EduCard \u2192 Summary
///
/// Catatan: ADA DUA PELAKU BERBEDA di Hari 2 \u2014 ini disengaja secara edukatif:
///   * PRIA HALTE  : bahaya yang datang lewat DATA/DARING. Minta nomor di halte
///                   (Rara menolak), tapi tetap bisa meng-WA karena nomornya bocor
///                   lewat teman \u2192 pelajaran: data pribadi bisa menyebar.
///   * PRIA ANGKOT : bahaya FISIK langsung. Diam-diam duduk di belakang, lalu
///                   pindah merapat & menyentuh (Sentuh), dan mendekat lagi (Lapor)
///                   \u2192 pelajaran: TIDAK \u2192 PERGI \u2192 CERITA (panggil Pak Supir).
/// Urutan sengaja menempatkan insiden "sentuh bahu" SEBELUM Quiz zona tubuh.
///
/// Background dibangun procedural (gradient + bentuk sederhana) per fase.
/// Tiap fase punya komponen sendiri (HalteDialog, AngkotSeatPicker, dll)
/// yang dipanggil via OnSelesai callback supaya alur tetap linear.
///
/// Cara pakai:
///   1. Buat Scene "Day2.unity" + tambahkan GameState/AudioManager/SceneLoader
///   2. GameObject \u2192 Create Empty \u2192 "Day2Controller" \u2192 Add Component
///   3. Drag referensi setiap fase (atau biarkan auto-find)
///   4. Klik Play \u2192 controller akan jalan otomatis dari fase Intro
/// </summary>
public class Day2Controller : MonoBehaviour
{
    public static Day2Controller Instance { get; private set; }

    public enum Phase
    {
        None,
        Intro,
        Narasi,
        Halte,
        Angkot,
        Sentuh,
        Quiz,
        ChatSim,
        Lapor,
        EduCard,
        Summary,
        Done
    }

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR
    // ══════════════════════════════════════════════════════════════════════

    [Header("Auto-Start")]
    [Tooltip("Mulai otomatis dari fase Intro saat scene aktif.")]
    public bool autoStart = true;
    [Tooltip("Skip fase ini (untuk debug).")]
    public Phase mulaiDariFase = Phase.Intro;

    [Header("Referensi Fase (kosongkan = auto-find)")]
    public Day2NarasiAwal     narasiAwal;
    public HalteDialog        haltDialog;
    public AngkotSeatPicker   angkotSeatPicker;
    public AngkotSentuhScene  angkotSentuh;
    public ZonaTubuhQuiz      zonaTubuhQuiz;
    public LaporTeriakButton  laporButton;
    public ChatSimWhatsApp    chatSim;
    public EduCardDay2        eduCard;
    public Day2SummaryScreen  summaryScreen;

    // ═════════════════════════════════════════════════════════════════════
    // JEMBATAN VN: ChatSim → Lapor  (pria angkot geser mendekat ke Rara)
    // Setelah simulasi chat, sebelum fase Lapor: pria yang tadi menyentuh bahu
    // (PRIA ANGKOT, bukan pria halte) memakai alasan "bangku sebelah kosong"
    // untuk pindah merapat. Ini TANDA BAHAYA → Rara percaya insting → CERITA.
    // ═════════════════════════════════════════════════════════════════════
    [Header("Jembatan VN: ChatSim → Lapor (pria geser mendekat)")]
    [Tooltip("DEPRECATED: narasi pembuka VN kini ditangani LaporTeriakButton (tampilkanNarasiPembuka).\n" +
             "Biarkan OFF supaya tidak tampil dua kali.")]
    public bool aktifkanJembatanLapor = false;
    [Tooltip("Portrait Rara untuk box dialog jembatan (opsional, upload nanti).")]
    public Sprite jembatanPortraitRara;
    [Tooltip("Portrait Pria Asing untuk box dialog jembatan (opsional, upload nanti).")]
    public Sprite jembatanPortraitPria;
    [Header("Jembatan VN — Animasi Ketik")]
    [Tooltip("Animasi ketik untuk teks box dialog setelah ChatSim (jembatan ke Lapor).")]
    public bool jembatanGunakanAnimasiKetik = true;
    [Tooltip("Detik per huruf pada animasi ketik jembatan VN. 0 = tampil langsung.")]
    [Range(0f, 0.10f)] public float jembatanDetikPerHuruf = 0.018f;

    [Header("Backdrop Procedural")]
    [Tooltip("Background utama dibuat dari script ini (gradient warna per fase).")]
    public bool buatBackdropProcedural = true;
    [Tooltip("Sorting order Canvas backdrop. Default -100 (paling belakang).")]
    public int backdropSortingOrder = -100;

    [Header("Warna Backdrop Per Fase")]
    public Color warnaIntro    = new Color(0.55f, 0.78f, 0.95f, 1f);   // langit pagi
    public Color warnaHalte    = new Color(0.45f, 0.62f, 0.78f, 1f);   // sedikit lebih gelap
    public Color warnaAngkot   = new Color(0.18f, 0.14f, 0.10f, 1f);   // interior coklat
    public Color warnaQuiz     = new Color(0.10f, 0.16f, 0.30f, 1f);   // ungu gelap
    public Color warnaLapor    = new Color(0.20f, 0.05f, 0.05f, 1f);   // merah gelap urgensi
    public Color warnaChatSim  = new Color(0.10f, 0.12f, 0.14f, 1f);   // di angkot \u2014 layar HP (abu gelap)
    public Color warnaEduCard  = new Color(0.05f, 0.10f, 0.08f, 1f);   // hijau gelap

    // ═════════════════════════════════════════════════════════════════════
    // BACKGROUND SPRITE PER FASE (opsional)
    // Kosongkan = pakai warna solid di atas. Isi dengan sprite kamu sendiri
    // (drag PNG/JPG dari folder Assets/sprites) untuk latar bergambar per fase.
    // ═════════════════════════════════════════════════════════════════════
    [Header("Background Sprite Per Fase (opsional — kosong = pakai warna)")]
    [Tooltip("Drag sprite latar untuk tiap fase. Kalau kosong, backdrop pakai warna solid di atas.")]
    public Sprite bgIntroSprite;
    public Sprite bgHalteSprite;
    public Sprite bgAngkotSprite;
    public Sprite bgQuizSprite;
    public Sprite bgLaporSprite;
    [Tooltip("Sprite latar fullscreen khusus jembatan VN sebelum fase Lapor. Kosong = pakai bgAngkotSprite.")]
    public Sprite bgNarasiJembatanSprite;
    public Sprite bgChatSimSprite;
    public Sprite bgEduCardSprite;

    [Header("Background Sprite — Pengaturan")]
    [Tooltip("Tipe render sprite latar: Simple (regang penuh) cocok untuk foto, Sliced untuk panel 9-slice.")]
    public Image.Type bgSpriteType = Image.Type.Simple;
    [Tooltip("Warna tint sprite latar (putih = warna asli sprite).")]
    public Color bgSpriteTint = Color.white;
    [Tooltip("Sembunyikan strip lantai gelap saat sprite latar dipakai.")]
    public bool sembunyikanFloorSaatAdaSprite = true;
    [Tooltip("Durasi crossfade saat ganti sprite latar antar fase (detik).")]
    public float bgSpriteFadeDetik = 0.6f;

    [Header("Label Fase (info di pojok atas)")]
    public bool tampilkanLabelFase = true;
    public Color warnaLabelFase = new Color(1f, 0.85f, 0.25f, 0.85f);
    public int   ukuranLabelFase = 22;

    [Header("Transisi Fade Antar Fase")]
    [Tooltip("Beri efek fade hitam halus saat berpindah fase Day 2.")]
    public bool gunakanFadeTransisi = true;
    [Tooltip("Durasi fade-in (gelap → terang) mengungkap fase baru, detik.")]
    public float fadeDurasiMasuk = 0.32f;
    [Tooltip("Warna layar transisi (default hitam).")]
    public Color fadeWarna = new Color(0f, 0f, 0f, 1f);

    [Header("Konsistensi Visual Antar Fase")]
    [Tooltip("Pita gelap sinematik di atas & bawah layar (frame seragam tiap fase).")]
    public bool tampilkanFrameSinematik = true;
    [Tooltip("Bungkus label fase dengan pill (latar gelap + outline emas) agar seragam.")]
    public bool labelFasePill = true;

    [Header("Alat Bantu Edukasi Day 2")]
    [Tooltip("Tampilkan Meteran Bahaya melayang yang naik/turun sesuai pilihan.")]
    public bool tampilkanMeteranBahaya = true;
    [Tooltip("Tampilkan tombol ? Glossary 3 Kata Sakti (TIDAK \u2192 PERGI \u2192 CERITA).")]
    public bool tampilkanGlossaryKataSakti = true;

    [Header("Intro Slide (sebelum Halte) — STYLE DAY 1 PROLOG")]
    [Tooltip("Judul kecil di atas narasi (gold). Kosongkan = sembunyikan.")]
    public string introJudul = "HARI 2 · PERSIAPAN";
    [TextArea(2, 6)]
    [Tooltip("Narasi pembuka Day 2.")]
    public string introNarasi = "🚌  Hari ke-2.\nRara mau naik angkot ke sekolah.\nDi halte ramai — tapi ada yang merhatiin dari tadi...";
    [Tooltip("Teks tombol mulai.")]
    public string introTombolTeks = "▶  MULAI";
    [Tooltip("Detik minimum sebelum tombol Mulai bisa diklik.")]
    public float  introMinDetik   = 1.5f;

    [Header("Intro Slide — Warna (custom seperti Day 1)")]
    [Tooltip("Warna judul (default: kuning emas Day 1).")]
    public Color introJudulWarna  = new Color(1f, 0.84f, 0f, 1f);          // #FFD700
    [Tooltip("Warna narasi (default: putih).")]
    public Color introWarna       = new Color(1f, 1f, 1f, 1f);
    [Tooltip("Warna background panel (RGBA).")]
    public Color introPanelWarna  = new Color(0f, 0f, 0f, 0.92f);
    [Tooltip("Warna border ornamen (kuning Day 1).")]
    public Color introBorderWarna = new Color(1f, 0.85f, 0.2f, 1f);
    [Tooltip("Warna background tombol Mulai.")]
    public Color introBtnWarna    = new Color(0.18f, 0.62f, 0.32f, 1f);
    [Tooltip("Warna teks tombol Mulai.")]
    public Color introBtnTextWarna = Color.white;
    [Tooltip("Warna overlay dim di belakang panel.")]
    public Color introDimWarna    = new Color(0f, 0f, 0f, 0.70f);

    [Header("Intro Slide — Ukuran Font")]
    public int introUkuran        = 28;   // narasi
    public int introJudulUkuran   = 38;   // judul
    public int introBtnUkuran     = 26;   // teks tombol

    [Header("Intro Slide — Layout Panel (px referensi 1920x1080)")]
    [Tooltip("Lebar panel intro (px).")]
    public float introPanelLebar  = 1100f;
    [Tooltip("Tinggi panel intro (px).")]
    public float introPanelTinggi = 380f;
    [Tooltip("Padding horizontal teks dari tepi panel.")]
    public float introPaddingH    = 60f;
    [Tooltip("Padding vertikal teks dari tepi atas/bawah panel.")]
    public float introPaddingV    = 50f;

    [Header("Intro Slide — Layout Tombol Mulai (px)")]
    public float introBtnLebar  = 320f;
    public float introBtnTinggi = 64f;
    [Tooltip("Jarak tombol dari tepi bawah panel (px).")]
    public float introBtnOffsetBawah = 28f;

    [Header("Intro Slide — Sprite Opsional")]
    [Tooltip("Sprite background panel intro (opsional). Kalau null, pakai warna solid + outline.")]
    public Sprite introPanelSprite;
    [Tooltip("Sprite background tombol Mulai (opsional). Kalau null, pakai warna solid.")]
    public Sprite introBtnSprite;

    [Header("Intro Slide — Input")]
    [Tooltip("SPACE / ENTER juga bisa lanjut (selain tombol Mulai).")]
    public bool introAdvanceOnKeyboard = true;

    // ══════════════════════════════════════════════════════════════════════
    // OVERLAY JUDUL HARI (style Day1Intro — fully customizable + sprite support)
    // ══════════════════════════════════════════════════════════════════════

    [Header("──────── OVERLAY JUDUL HARI ────────")]
    [Tooltip("Tampilkan overlay judul (HARI 2 + lokasi + ornamen) sebelum panel intro. Style sama Day1Intro.")]
    public bool tampilkanOverlayJudul = true;
    [Tooltip("Baris pertama judul besar (mis. 'HARI 2').")]
    public string barisPertama = "HARI 2";
    [Tooltip("Baris kedua judul (mis. 'Naik Angkot ke Sekolah').")]
    public string barisKedua   = "Naik Angkot ke Sekolah";
    [Tooltip("Teks lokasi yang tampil di bawah judul.")]
    public string teksLokasi   = "Angkot Jurusan Sekolah";
    [Tooltip("Teks kecil paling bawah overlay.")]
    public string teksHint     = "Bersiaplah...";

    [Header("Warna Overlay")]
    [Tooltip("Warna background gelap overlay (kalau sprite null).")]
    public Color warnaBackground = new Color(0f, 0f, 0.04f, 0.90f);
    [Tooltip("Warna teks judul (kuning emas).")]
    public Color warnaTeksJudul  = new Color(0.95f, 0.78f, 0.10f, 1f);
    [Tooltip("Warna teks lokasi.")]
    public Color warnaTeksLokasi = Color.white;
    [Tooltip("Warna garis dekoratif horizontal.")]
    public Color warnaGaris      = new Color(0.95f, 0.78f, 0.10f, 0.70f);

    [Header("Ukuran Font Overlay")]
    public int ukuranFontJudul   = 72;
    public int ukuranFontLokasi  = 36;

    [Header("Durasi Overlay (detik)")]
    [Tooltip("Berapa lama overlay tampil penuh sebelum fade out.")]
    public float durasiTampil    = 2.8f;
    [Tooltip("Durasi animasi fade in dan fade out.")]
    public float durasiTransisi  = 0.5f;

    [Header("Overlay — Sprite (opsional)")]
    [Tooltip("Sprite background overlay judul. Kalau di-set, override warnaBackground. Drag dari sprites/UI day 1/6.png untuk match Day 1.")]
    public Sprite overlayBgSprite;
    [Tooltip("Path default sprite background (relatif Assets/) untuk auto-load Editor.")]
    public string overlayBgSpritePath = "sprites/UI day 1/6.png";

    [Header("Font (opsional)")]
    public TMP_FontAsset fontAsset;

    [Header("Event")]
    public UnityEngine.Events.UnityEvent onPhaseChanged;

    // ── runtime ───────────────────────────────────────────────────────────
    private Phase      _fase = Phase.None;
    private GameObject _backdropGO;
    private Image      _backdropImg;
    private bool       _jembatanLaporTampil; // cegah jembatan VN tampil dua kali
    private Image      _bgSpriteImg;   // layer sprite latar di atas warna solid
    private Image      _floorImg;      // strip lantai gelap (bisa disembunyikan saat ada sprite)
    private TextMeshProUGUI _labelFase;
    private GameObject _introPanel;
    private CanvasGroup _fadeGroup;     // overlay fade hitam antar fase
    private GameObject  _fadeGO;

    /// <summary>
    /// Sembunyikan/munculkan backdrop procedural runtime.
    /// Dipanggil oleh fase yang punya BG sprite world space sendiri
    /// (mis. Day2NarasiAwal) supaya canvas biru tidak menutupi sprite.
    /// </summary>
    public void SetBackdropAktif(bool aktif)
    {
        if (_backdropGO != null) _backdropGO.SetActive(aktif);
    }

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    IEnumerator Start()
    {
        var gs = GameState.Instance;

        // PENTING: jangan hijack Day 1. Day2Controller hanya boleh aktif kalau
        // pemain memang sedang di Day 2 (gs.day == 2). DayTransitionManager
        // akan set gs.day = 2 SEBELUM meng-enable Day2_Root, jadi pada saat itu
        // Start() ini boleh lanjut. Tanpa pengecekan ini, panel Intro full-screen
        // (dim raycastTarget=true di sortingOrder 980) akan memblokir SEMUA klik
        // tombol Day 1.
        if (autoStart && gs != null && gs.day != 2)
        {
            Debug.Log("[Day2Controller] gs.day = " + gs.day + " (≠ 2). Day2Controller diam dulu, menunggu DayTransitionManager.LanjutKeDay2().");
            yield break;
        }

        if (gs != null) gs.day = 2;

        // Sinkronkan HUD: progress hari H1→H2, label lokasi, dst.
        if (HUDManager.Instance != null) HUDManager.Instance.Refresh();

        // BGM Day 2 (kalau ada track)
        if (AudioManager.Instance != null)
        {
            try { AudioManager.Instance.PlayBGM(AudioManager.BGMTrack.Day2); }
            catch { /* ignore kalau enum berbeda */ }
        }

        if (buatBackdropProcedural) BuildBackdrop();
        if (tampilkanLabelFase)     BuildLabelFase();
        TampilkanAlatBantuDay2();

        AutoFindRefs();

        // Tunggu 1 frame supaya semua Start() lain jalan dulu
        yield return null;

        if (autoStart) GotoFase(mulaiDariFase);
    }

    /// <summary>
    /// Dipanggil oleh DayTransitionManager.LanjutKeDay2() untuk memulai Day 2
    /// secara eksplisit. Idempotent: kalau sudah jalan, tidak akan dobel.
    /// </summary>
    public void TriggerStart()
    {
        if (_fase != Phase.None && _fase != Phase.Done) return; // sudah jalan
        StartCoroutine(StartManual());
    }

    IEnumerator StartManual()
    {
        var gs = GameState.Instance;
        if (gs != null) gs.day = 2;

        // Sinkronkan HUD: H1→H2 + label lokasi Day 2.
        if (HUDManager.Instance != null) HUDManager.Instance.Refresh();

        if (AudioManager.Instance != null)
        {
            try { AudioManager.Instance.PlayBGM(AudioManager.BGMTrack.Day2); }
            catch { }
        }

        if (buatBackdropProcedural && _backdropGO == null) BuildBackdrop();
        if (tampilkanLabelFase     && _labelFase  == null) BuildLabelFase();
        TampilkanAlatBantuDay2();

        AutoFindRefs();
        yield return null;
        GotoFase(mulaiDariFase);
    }

    // ══════════════════════════════════════════════════════════════════════
    // PUBLIC API
    // ══════════════════════════════════════════════════════════════════════
    public Phase CurrentPhase => _fase;

    public void GotoFase(Phase next)
    {
        // SAFETY NET: pastikan gs.day=2 + HUD ter-refresh tiap fase Day 2.
        // Berguna kalau user enable Day2_Root langsung tanpa lewat DayTransitionManager,
        // atau kalau HUDManager baru selesai BuildHUD setelah Day2Controller.Start.
        var gs = GameState.Instance;
        if (gs != null && gs.day != 2) gs.day = 2;
        if (HUDManager.Instance != null) HUDManager.Instance.Refresh();

        StopAllCoroutines();
        StartCoroutine(RunFase(next));
    }

    // ══════════════════════════════════════════════════════════════════════
    // STATE MACHINE
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator RunFase(Phase next)
    {
        _fase = next;
        UpdateBackdrop(next);
        UpdateLabelFase(next);
        onPhaseChanged?.Invoke();

        // Tombol "?" Glossary: sembunyikan di fase berlayar-penuh supaya tidak
        // menumpuk/mengganggu elemen UI lain; tampilkan lagi di fase dialog biasa.
        AturGlossaryUntukFase(next);

        // Transisi fade: layar gelap sejenak lalu memudar mengungkap fase baru,
        // supaya perpindahan antar halaman terasa halus (bukan potong mendadak).
        if (gunakanFadeTransisi && next != Phase.Done)
        {
            EnsureFadeOverlay();
            if (_fadeGroup != null)
            {
                _fadeGroup.alpha = 1f;
                yield return null; // 1 frame: biar UI fase lama hilang & backdrop baru terpasang
                yield return FadeOverlayKe(0f, fadeDurasiMasuk);
            }
        }

        Debug.Log($"[Day2Controller] \u2192 Fase: {next}");

        switch (next)
        {
            case Phase.Intro:
                // Hanya overlay judul singkat (ala referensi web). Tidak ada panel intro tombol MULAI.
                if (tampilkanOverlayJudul) yield return TampilkanOverlayJudul();
                GotoFase(Phase.Narasi);
                break;

            case Phase.Narasi:
                if (narasiAwal == null)
                {
                    Debug.LogWarning("[Day2] Day2NarasiAwal tidak ada \u2014 skip ke Halte.");
                    GotoFase(Phase.Halte);
                    break;
                }
                narasiAwal.Mulai(() => GotoFase(Phase.Halte));
                break;

            case Phase.Halte:
                if (haltDialog == null) { Debug.LogWarning("[Day2] HalteDialog tidak ada \u2014 skip ke Angkot."); GotoFase(Phase.Angkot); break; }
                haltDialog.Mulai(() => GotoFase(Phase.Angkot));
                break;

            case Phase.Angkot:
                if (angkotSeatPicker == null) { Debug.LogWarning("[Day2] AngkotSeatPicker tidak ada — skip ke Sentuh."); GotoFase(Phase.Sentuh); break; }
                angkotSeatPicker.Mulai(() => GotoFase(Phase.Sentuh));
                break;

            case Phase.Sentuh:
                if (angkotSentuh == null) { Debug.LogWarning("[Day2] AngkotSentuhScene tidak ada — skip ke Quiz."); GotoFase(Phase.Quiz); break; }
                // Insiden disentuh di angkot \u2192 lanjut ke Quiz Zona Tubuh
                // (urutan sengaja: Sentuh SEBELUM Quiz).
                angkotSentuh.Mulai(() => GotoFase(Phase.Quiz));
                break;

            case Phase.Quiz:
                if (zonaTubuhQuiz == null) { Debug.LogWarning("[Day2] ZonaTubuhQuiz tidak ada \u2014 skip ke ChatSim."); GotoFase(Phase.ChatSim); break; }
                zonaTubuhQuiz.Mulai(() => GotoFase(Phase.ChatSim));
                break;

            case Phase.ChatSim:
                if (chatSim == null) { Debug.LogWarning("[Day2] ChatSimWhatsApp tidak ada \u2014 skip ke Lapor."); KeLapor(); break; }
                // Masih di angkot: HP Rara bergetar, pria yang sama meng-WA Rara.
                // Tampilkan narasi jembatan dulu, baru buka simulasi chat \u2192 lanjut Lapor.
                StartCoroutine(JalankanChatSim());
                break;

            case Phase.Lapor:
                if (laporButton == null) { Debug.LogWarning("[Day2] LaporTeriakButton tidak ada \u2014 skip ke EduCard."); GotoFase(Phase.EduCard); break; }
                // Narasi pembuka VN "pria geser mendekat" kini ditangani DI DALAM
                // LaporTeriakButton.Mulai() supaya pasti tampil dari jalur mana pun.
                laporButton.Mulai(() => GotoFase(Phase.EduCard));
                break;

            case Phase.EduCard:
                if (eduCard == null) { Debug.LogWarning("[Day2] EduCardDay2 tidak ada \u2014 skip ke Summary."); GotoFase(Phase.Summary); break; }
                eduCard.onLanjut.AddListener(() => GotoFase(Phase.Summary));
                eduCard.Tampilkan();
                break;

            case Phase.Summary:
                // Evaluasi pencapaian Hari 2 + sembunyikan alat bantu sebelum ringkasan.
                GameState.Instance?.EvaluateDay2Achievements();
                SembunyikanAlatBantuDay2();
                if (summaryScreen != null) summaryScreen.Tampilkan();
                else if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene("Day3");
                _fase = Phase.Done;
                break;
        }
        yield break;
    }

    // ══════════════════════════════════════════════════════════════════════
    // ALAT BANTU EDUKASI: Meteran Bahaya + Glossary 3 Kata Sakti
    // ══════════════════════════════════════════════════════════════════════
    void TampilkanAlatBantuDay2()
    {
        // Meteran "TINGKAT BAHAYA" dinonaktifkan (dihapus dari tampilan) atas permintaan.
        DangerGauge.Hide();
        if (tampilkanGlossaryKataSakti) KataSaktiGlossary.EnsureButton();
    }

    void SembunyikanAlatBantuDay2()
    {
        DangerGauge.Hide();
        KataSaktiGlossary.Hide();
    }

    // Fase berlayar-penuh (mini-game / kartu / chat) yang elemennya bisa
    // tertumpuk tombol "?" Glossary di pojok kanan-atas. Saat masuk fase ini,
    // tombol "?" disembunyikan; di fase lain ditampilkan lagi (jika diaktifkan).
    void AturGlossaryUntukFase(Phase fase)
    {
        if (!tampilkanGlossaryKataSakti) return;

        bool ganggu =
            fase == Phase.Angkot  ||   // pemilihan kursi (UI penuh)
            fase == Phase.Sentuh  ||
            fase == Phase.Quiz    ||   // drag-drop zona tubuh
            fase == Phase.ChatSim ||   // layar HP WhatsApp
            fase == Phase.Lapor   ||   // tombol teriak + hasil
            fase == Phase.EduCard ||
            fase == Phase.Summary ||
            fase == Phase.Done;

        if (ganggu) KataSaktiGlossary.Hide();
        else        KataSaktiGlossary.EnsureButton();
    }

    // ══════════════════════════════════════════════════════════════════════
    // FASE CHATSIM \u2014 jembatan naratif "HP bergetar" + simulasi WhatsApp
    // Pelaku di sini adalah PRIA HALTE (yang minta nomor pagi tadi), BUKAN pria
    // yang duduk di angkot. Nomor Rara bocor lewat temannya, jadi pria halte bisa
    // meng-WA walau Rara sudah menolak di halte. Setelah chat \u2192 lanjut ke Lapor.
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator JalankanChatSim()
    {
        // Narasi jembatan: HP Rara tiba-tiba bergetar, pesan dari nomor tak dikenal.
        string[] narasi =
        {
            "Belum lama Rara menyimpan bukunya, HP di sakunya tiba-tiba bergetar.",
            "Ada pesan WhatsApp dari nomor tak dikenal \u2014 padahal Rara merasa tidak pernah memberi nomornya ke orang asing.",
            "Dengan jantung berdebar, Rara membuka pesannya."
        };
        yield return NarasiJembatan(narasi);

        // Buka simulasi chat. Selesai \u2192 jembatan VN "pria geser mendekat" \u2192 Lapor.
        chatSim.Mulai(KeLapor);
    }

    // Lanjut ke fase Lapor. Jembatan VN kini dimainkan DI DALAM fase Lapor
    // (lihat JalankanLaporDenganJembatan) supaya tampil dari jalur mana pun.
    void KeLapor()
    {
        GotoFase(Phase.Lapor);
    }

    // ══════════════════════════════════════════════════════════════════════
    // FASE LAPOR: mainkan jembatan VN dulu (pria geser mendekat) baru tombol teriak
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator JalankanLaporDenganJembatan()
    {
        if (aktifkanJembatanLapor && !_jembatanLaporTampil)
        {
            _jembatanLaporTampil = true;
            yield return JalankanJembatanLapor();
        }
        laporButton.Mulai(() => GotoFase(Phase.EduCard));
    }

    // ══════════════════════════════════════════════════════════════════════
    // JEMBATAN VN: pria angkot geser mendekat sebelum fase Lapor
    // Pelaku = PRIA ANGKOT (yang menyentuh bahu Rara di fase Sentuh), BUKAN
    // pria halte. Memakai alasan "bangku kosong" sbg modus -> red flag -> CERITA.
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator JalankanJembatanLapor()
    {
        Debug.Log("[Day2] JembatanLapor (VN pria geser mendekat) MULAI tampil.");
        var baris = new (string speaker, string teks)[]
        {
            ("Narasi",     "Rara memasukkan kembali HP-nya ke saku. Tapi suasana di dalam angkot terasa berubah."),
            ("Narasi",     "Beberapa penumpang turun di perempatan. Kini bangku tepat di sebelah Rara kosong."),
            ("Pria Asing", "Wah, kosong nih. Om pindah ke sini aja ya, biar lebih enak ngobrolnya."),
            ("Narasi",     "Pria yang tadi menyentuh bahunya itu menggeser duduknya \u2014 makin merapat ke arah Rara."),
            ("Rara",       "Kenapa dia harus pindah ke sebelahku? Padahal masih banyak bangku lain yang kosong..."),
            ("Narasi",     "Hati kecil Rara berkata ada yang tidak beres. Inilah saatnya kata sakti ketiga: CERITA \u2014 minta tolong Pak Supir!")
        };
        yield return NarasiVN(baris);
        // Tidak GotoFase di sini — pemanggil (JalankanLaporDenganJembatan)
        // langsung lanjut ke laporButton.Mulai setelah VN selesai.
    }

    // Narasi gaya Visual Novel: panel kayu + bingkai portrait (IDENTIK HalteDialog).
    // Sprite panel & portrait diambil dari komponen haltDialog supaya tampilannya
    // sama persis dengan fase Halte (tidak perlu assign ulang sprite).
    IEnumerator NarasiVN((string speaker, string teks)[] baris)
    {
        var cGO = new GameObject("Day2_JembatanLaporVN");
        var cv  = cGO.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 972;
        var sc = cGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920f, 1080f);
        sc.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // ── Latar: pakai sprite khusus jembatan VN kalau ada; fallback ke angkot. ──
        Sprite bgSprite = bgNarasiJembatanSprite != null ? bgNarasiJembatanSprite : bgAngkotSprite;
        var bgImg = OvBuatImage(cGO.transform, "BG", Vector2.zero, Vector2.one, warnaAngkot);
        if (bgSprite != null)
        {
            bgImg.sprite        = bgSprite;
            bgImg.type          = Image.Type.Simple;
            bgImg.preserveAspect = false;
            bgImg.color         = Color.white;
        }

        // ── Ambil aset/look dari HalteDialog (fallback ke nilai default kalau null). ──
        Sprite panelSp = haltDialog != null ? haltDialog.panelSprite : null;
        float pCX = haltDialog != null ? haltDialog.boxPanelCenterX : 0.50f;
        float pCY = haltDialog != null ? haltDialog.boxPanelCenterY : 0.215f;
        float pW  = haltDialog != null ? haltDialog.boxPanelWidth   : 0.96f;
        float pH  = haltDialog != null ? haltDialog.boxPanelHeight  : 0.395f;
        float qCX = haltDialog != null ? haltDialog.boxPortraitCenterX : 0.153f;
        float qCY = haltDialog != null ? haltDialog.boxPortraitCenterY : 0.625f;
        float qW  = haltDialog != null ? haltDialog.boxPortraitW : 0.192f;
        float qH  = haltDialog != null ? haltDialog.boxPortraitH : 0.494f;
        Vector2 bMin = haltDialog != null ? haltDialog.boxBannerAnchorMin : new Vector2(0.11f, 0.11f);
        Vector2 bMax = haltDialog != null ? haltDialog.boxBannerAnchorMax : new Vector2(0.253f, 0.333f);
        Vector2 tMin = haltDialog != null ? haltDialog.boxTextAnchorMin   : new Vector2(0.31f, 0.55f);
        Vector2 tMax = haltDialog != null ? haltDialog.boxTextAnchorMax   : new Vector2(0.84f, 0.76f);
        Color namaCol = haltDialog != null ? haltDialog.boxNamaColor : new Color(1f, 0.85f, 0.30f, 1f);
        Color teksCol = haltDialog != null ? haltDialog.boxTextColor : Color.white;
        int   namaFs  = haltDialog != null ? haltDialog.boxNamaFontSize : 30;
        int   teksFs  = haltDialog != null ? haltDialog.boxTextFontSize : 26;

        // ── Panel kotak dialog (panel kayu sliced, sama Halte) ──
        var boxGO = new GameObject("DialogBox");
        boxGO.transform.SetParent(cGO.transform, false);
        var boxImg = boxGO.AddComponent<Image>();
        boxImg.raycastTarget = true;
        if (panelSp != null)
        {
            boxImg.sprite = panelSp;
            boxImg.type   = Image.Type.Sliced;
            boxImg.color  = Color.white;
        }
        else
        {
            boxImg.color = new Color(0.05f, 0.08f, 0.12f, 0.94f);
            var outl = boxGO.AddComponent<Outline>();
            outl.effectColor    = new Color(1f, 0.85f, 0.25f, 1f);
            outl.effectDistance = new Vector2(2f, -2f);
        }
        var brt = boxGO.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(pCX - pW * 0.5f, pCY - pH * 0.5f);
        brt.anchorMax = new Vector2(pCX + pW * 0.5f, pCY + pH * 0.5f);
        brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;

        // ── Portrait (kiri, di area bingkai panel) ──
        var portraitGO = new GameObject("Portrait");
        portraitGO.transform.SetParent(boxGO.transform, false);
        var prt2 = portraitGO.AddComponent<RectTransform>();
        prt2.anchorMin = new Vector2(qCX - qW * 0.5f, qCY - qH * 0.5f);
        prt2.anchorMax = new Vector2(qCX + qW * 0.5f, qCY + qH * 0.5f);
        prt2.offsetMin = prt2.offsetMax = Vector2.zero;
        var portraitImg = portraitGO.AddComponent<Image>();
        portraitImg.preserveAspect = true;
        portraitImg.color          = Color.white;
        portraitImg.raycastTarget  = false;
        portraitImg.enabled        = false;

        // ── Banner nama pembicara ──
        var namaTmp = OvBuatTMP(boxGO.transform, "Nama", bMin, bMax, "", namaFs, namaCol, true);
        namaTmp.alignment = TextAlignmentOptions.Center;

        // ── Teks isi dialog ──
        var teksTmp = OvBuatTMP(boxGO.transform, "Teks", tMin, tMax, "", teksFs, teksCol, false);
        teksTmp.alignment = TextAlignmentOptions.TopLeft;

        // ── Hint pojok kanan-bawah panel ──
        OvBuatTMP(boxGO.transform, "Hint",
            new Vector2(0.67f, 0.07f), new Vector2(0.97f, 0.19f),
            "\u25BC  Ketuk lanjut", 16, new Color(1f, 1f, 1f, 0.55f), false)
            .alignment = TextAlignmentOptions.MidlineRight;

        foreach (var b in baris)
        {
            namaTmp.text  = string.IsNullOrEmpty(b.speaker) ? "" : b.speaker.ToUpper();
            namaTmp.color = b.speaker == "Pria Asing" ? new Color(0.95f, 0.45f, 0.40f, 1f)
                          : b.speaker == "Rara"       ? new Color(0.45f, 0.78f, 1.00f, 1f)
                          :                             namaCol;

            // Portrait: override jembatan -> portrait HalteDialog -> sembunyikan.
            Sprite ps =
                b.speaker == "Rara"       ? (jembatanPortraitRara != null ? jembatanPortraitRara : (haltDialog != null ? haltDialog.portraitRara : null))
              : b.speaker == "Pria Asing" ? (jembatanPortraitPria != null ? jembatanPortraitPria : (haltDialog != null ? haltDialog.portraitPriaAsing : null))
              :                             (haltDialog != null ? haltDialog.portraitNarasi : null);
            if (ps != null) { portraitImg.sprite = ps; portraitImg.enabled = false; } // potret disembunyikan dari box dialog
            else            { portraitImg.enabled = false; }

            if (jembatanGunakanAnimasiKetik && jembatanDetikPerHuruf > 0f)
                yield return OvKetikTMP(teksTmp, b.teks ?? "", jembatanDetikPerHuruf);
            else
                teksTmp.text = b.teks;

            bool lanjut = false; float timer = 0f;
            while (!lanjut)
            {
                timer += Time.deltaTime;
                if (timer >= 0.25f &&
                    (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
                    lanjut = true;
                yield return null;
            }
        }
        Destroy(cGO);
    }

    IEnumerator OvKetikTMP(TextMeshProUGUI tmp, string teks, float detikPerHuruf)
    {
        if (tmp == null) yield break;
        if (string.IsNullOrEmpty(teks) || detikPerHuruf <= 0f)
        {
            tmp.text = teks ?? "";
            yield break;
        }

        tmp.text = "";
        for (int i = 0; i < teks.Length; i++)
        {
            // Klik/SPACE/ENTER saat mengetik = langsung tampil penuh.
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                tmp.text = teks;
                yield break;
            }

            tmp.text += teks[i];
            yield return new WaitForSeconds(detikPerHuruf);
        }
    }

    // Narasi sederhana antar-fase: klik/tap/SPACE untuk maju per baris.
    IEnumerator NarasiJembatan(string[] baris)
    {
        var cGO = new GameObject("Day2_NarasiJembatan");
        var cv  = cGO.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 970;
        var sc = cGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920f, 1080f);
        sc.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // Latar malam (pakai warna fase ChatSim biar nyambung)
        OvBuatImage(cGO.transform, "BG", Vector2.zero, Vector2.one, warnaChatSim);

        // Kotak narasi semi-transparan
        var box = OvBuatImage(cGO.transform, "Box",
            new Vector2(0.1f, 0.36f), new Vector2(0.9f, 0.64f),
            new Color(0f, 0f, 0f, 0.55f));
        box.raycastTarget = true;

        var tmp = OvBuatTMP(cGO.transform, "Teks",
            new Vector2(0.14f, 0.38f), new Vector2(0.86f, 0.62f),
            "", 34, Color.white, false);

        OvBuatTMP(cGO.transform, "Hint",
            new Vector2(0.2f, 0.27f), new Vector2(0.8f, 0.34f),
            "\u25B6  Klik / tap untuk lanjut", 22, new Color(1f, 1f, 1f, 0.5f), false);

        foreach (var t in baris)
        {
            tmp.text = t;
            bool lanjut = false;
            float timer = 0f;
            while (!lanjut)
            {
                timer += Time.deltaTime;
                if (timer >= 0.25f &&
                    (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
                    lanjut = true;
                yield return null;
            }
        }
        Destroy(cGO);
    }

    // ══════════════════════════════════════════════════════════════════════
    // AUTO-FIND
    // ══════════════════════════════════════════════════════════════════════
    void AutoFindRefs()
    {
        if (narasiAwal       == null) narasiAwal       = FindFirstObjectByType<Day2NarasiAwal>(FindObjectsInactive.Include);
        if (haltDialog       == null) haltDialog       = FindFirstObjectByType<HalteDialog>(FindObjectsInactive.Include);
        if (angkotSeatPicker == null) angkotSeatPicker = FindFirstObjectByType<AngkotSeatPicker>(FindObjectsInactive.Include);
        if (angkotSentuh     == null) angkotSentuh     = FindFirstObjectByType<AngkotSentuhScene>(FindObjectsInactive.Include);
        if (zonaTubuhQuiz    == null) zonaTubuhQuiz    = FindFirstObjectByType<ZonaTubuhQuiz>(FindObjectsInactive.Include);
        if (laporButton      == null) laporButton      = FindFirstObjectByType<LaporTeriakButton>(FindObjectsInactive.Include);
        if (chatSim          == null) chatSim          = FindFirstObjectByType<ChatSimWhatsApp>(FindObjectsInactive.Include);
        if (eduCard          == null) eduCard          = FindFirstObjectByType<EduCardDay2>(FindObjectsInactive.Include);
        if (summaryScreen    == null) summaryScreen    = FindFirstObjectByType<Day2SummaryScreen>(FindObjectsInactive.Include);
    }

    // ══════════════════════════════════════════════════════════════════════
    // BACKDROP PROCEDURAL
    // ══════════════════════════════════════════════════════════════════════
    void BuildBackdrop()
    {
        _backdropGO = new GameObject("Day2_Backdrop");
        var canvas = _backdropGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = backdropSortingOrder;
        var scaler = _backdropGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        var bgGO = new GameObject("BG");
        bgGO.transform.SetParent(_backdropGO.transform, false);
        _backdropImg = bgGO.AddComponent<Image>();
        _backdropImg.color = warnaIntro;
        _backdropImg.raycastTarget = false;
        var rt = bgGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        // Layer SPRITE latar (di atas warna solid). Kosong saat tidak ada sprite.
        var spriteGO = new GameObject("BG_Sprite");
        spriteGO.transform.SetParent(_backdropGO.transform, false);
        _bgSpriteImg = spriteGO.AddComponent<Image>();
        _bgSpriteImg.color = bgSpriteTint;
        _bgSpriteImg.type  = bgSpriteType;
        _bgSpriteImg.preserveAspect = false;
        _bgSpriteImg.raycastTarget = false;
        _bgSpriteImg.enabled = false; // diaktifkan kalau fase punya sprite
        var srt = spriteGO.GetComponent<RectTransform>();
        srt.anchorMin = Vector2.zero; srt.anchorMax = Vector2.one;
        srt.offsetMin = Vector2.zero; srt.offsetMax = Vector2.zero;

        // Lantai gelap di bawah untuk depth (procedural ground strip)
        var floor = new GameObject("Floor");
        floor.transform.SetParent(_backdropGO.transform, false);
        _floorImg = floor.AddComponent<Image>();
        _floorImg.color = new Color(0f, 0f, 0f, 0.35f);
        _floorImg.raycastTarget = false;
        var frt = floor.GetComponent<RectTransform>();
        frt.anchorMin = new Vector2(0f, 0f);
        frt.anchorMax = new Vector2(1f, 0.18f);
        frt.offsetMin = Vector2.zero; frt.offsetMax = Vector2.zero;

        // Frame sinematik (#6): pita gelap atas & bawah agar SEMUA fase punya
        // bingkai visual seragam (konsistensi antar halaman).
        if (tampilkanFrameSinematik)
        {
            BuatPitaSinematik("PitaAtas",  new Vector2(0f, 0.93f), new Vector2(1f, 1f),    true);
            BuatPitaSinematik("PitaBawah", new Vector2(0f, 0f),    new Vector2(1f, 0.06f), false);
        }

        // Terapkan kondisi awal sesuai fase pertama (Intro)
        ApplyBackdropSprite(Phase.Intro, instan: true);
    }

    // Pita gelap sinematik (gradient sederhana via 2 layer) di tepi atas/bawah.
    void BuatPitaSinematik(string nama, Vector2 anchorMin, Vector2 anchorMax, bool atas)
    {
        var go = new GameObject(nama);
        go.transform.SetParent(_backdropGO.transform, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.55f);
        img.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        // Garis aksen emas tipis di sisi dalam pita (sentuhan tema kayu/sunset).
        var line = new GameObject("Aksen");
        line.transform.SetParent(go.transform, false);
        var lImg = line.AddComponent<Image>();
        lImg.color = new Color(0.95f, 0.72f, 0.18f, 0.35f);
        lImg.raycastTarget = false;
        var lrt = line.GetComponent<RectTransform>();
        if (atas) { lrt.anchorMin = new Vector2(0f, 0f); lrt.anchorMax = new Vector2(1f, 0f); lrt.pivot = new Vector2(0.5f, 1f); }
        else      { lrt.anchorMin = new Vector2(0f, 1f); lrt.anchorMax = new Vector2(1f, 1f); lrt.pivot = new Vector2(0.5f, 0f); }
        lrt.sizeDelta = new Vector2(0f, 2.5f);
        lrt.anchoredPosition = Vector2.zero;
    }

    // Pilih sprite latar untuk fase tertentu (null kalau fase tidak punya sprite).
    Sprite SpriteUntukFase(Phase p) => p switch
    {
        Phase.Intro   => bgIntroSprite,
        Phase.Halte   => bgHalteSprite,
        Phase.Angkot  => bgAngkotSprite,
        Phase.Sentuh  => bgAngkotSprite,
        Phase.Quiz    => bgQuizSprite,
        Phase.Lapor   => bgLaporSprite,
        Phase.ChatSim => bgChatSimSprite,
        Phase.EduCard => bgEduCardSprite,
        _             => null
    };

    // Pasang/lepas sprite latar sesuai fase. instan=true langsung tanpa fade.
    void ApplyBackdropSprite(Phase p, bool instan = false)
    {
        if (_bgSpriteImg == null) return;
        Sprite s = SpriteUntukFase(p);

        if (s == null)
        {
            _bgSpriteImg.enabled = false;
            if (_floorImg != null) _floorImg.enabled = true;
            return;
        }

        _bgSpriteImg.sprite = s;
        _bgSpriteImg.type   = bgSpriteType;
        _bgSpriteImg.enabled = true;
        if (_floorImg != null) _floorImg.enabled = !sembunyikanFloorSaatAdaSprite;

        if (instan || bgSpriteFadeDetik <= 0f)
        {
            _bgSpriteImg.color = bgSpriteTint;
        }
        else
        {
            StopCoroutine(nameof(FadeBgSprite));
            StartCoroutine(FadeBgSprite());
        }
    }

    IEnumerator FadeBgSprite()
    {
        Color target = bgSpriteTint;
        Color from   = new Color(target.r, target.g, target.b, 0f);
        float t = 0f;
        _bgSpriteImg.color = from;
        while (t < bgSpriteFadeDetik)
        {
            t += Time.deltaTime;
            _bgSpriteImg.color = Color.Lerp(from, target, t / bgSpriteFadeDetik);
            yield return null;
        }
        _bgSpriteImg.color = target;
    }

    void BuildLabelFase()
    {
        var canvasGO = new GameObject("Day2_LabelFase_Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 950; // di atas backdrop, di bawah dialog
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var labelGO = new GameObject("LabelFase");
        labelGO.transform.SetParent(canvasGO.transform, false);
        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) tmp.font = fontAsset;
        else if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
        tmp.fontSize = ukuranLabelFase;
        tmp.color    = warnaLabelFase;
        tmp.fontStyle = FontStyles.Bold;
        tmp.text     = "";
        tmp.raycastTarget = false;
        var rt = tmp.rectTransform;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot     = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(520f, 40f);
        rt.anchoredPosition = new Vector2(30f, -25f);
        _labelFase = tmp;

        // Pill di belakang label (#6): latar gelap + outline emas agar chip lokasi
        // tampil seragam di tiap fase. Diselipkan sebagai sibling di belakang teks.
        if (labelFasePill)
        {
            var pill = new GameObject("LabelPill");
            pill.transform.SetParent(canvasGO.transform, false);
            pill.transform.SetSiblingIndex(labelGO.transform.GetSiblingIndex()); // di belakang teks
            var pImg = pill.AddComponent<Image>();
            pImg.color = new Color(0.10f, 0.07f, 0.04f, 0.82f);
            pImg.raycastTarget = false;
            var pol = pill.AddComponent<Outline>();
            pol.effectColor    = new Color(0.95f, 0.72f, 0.18f, 0.75f);
            pol.effectDistance = new Vector2(2f, -2f);
            var prt = pill.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0f, 1f);
            prt.anchorMax = new Vector2(0f, 1f);
            prt.pivot     = new Vector2(0f, 1f);
            prt.sizeDelta = new Vector2(420f, 44f);
            prt.anchoredPosition = new Vector2(22f, -23f);

            // Beri padding teks di dalam pill.
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            rt.anchoredPosition = new Vector2(40f, -24f);
        }
    }

    void UpdateBackdrop(Phase p)
    {
        if (_backdropImg == null) return;
        Color c = p switch
        {
            Phase.Intro   => warnaIntro,
            Phase.Halte   => warnaHalte,
            Phase.Angkot  => warnaAngkot,
            Phase.Sentuh  => warnaAngkot,
            Phase.Quiz    => warnaQuiz,
            Phase.Lapor   => warnaLapor,
            Phase.ChatSim => warnaChatSim,
            Phase.EduCard => warnaEduCard,
            _             => _backdropImg.color
        };
        StopCoroutine(nameof(LerpBackdrop));
        StartCoroutine(LerpBackdrop(c, 0.6f));

        // Ganti sprite latar (kalau fase ini punya sprite custom)
        ApplyBackdropSprite(p);
    }

    IEnumerator LerpBackdrop(Color target, float dur)
    {
        Color from = _backdropImg.color;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            _backdropImg.color = Color.Lerp(from, target, t / dur);
            yield return null;
        }
        _backdropImg.color = target;
    }

    // ══════════════════════════════════════════════════════════════════════
    // FADE TRANSISI ANTAR FASE (#5) — overlay hitam fullscreen di atas semua UI
    // ══════════════════════════════════════════════════════════════════════
    void EnsureFadeOverlay()
    {
        if (_fadeGO != null) return;

        _fadeGO = new GameObject("Day2_FadeOverlay");
        _fadeGO.transform.SetParent(transform, false);
        var canvas = _fadeGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000; // di atas semua UI fase Day 2
        var scaler = _fadeGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var img = _fadeGO.AddComponent<Image>();
        img.color = fadeWarna;
        img.raycastTarget = false; // jangan blokir input — hanya visual
        var rt = img.rectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        _fadeGroup = _fadeGO.AddComponent<CanvasGroup>();
        _fadeGroup.alpha = 0f;
        _fadeGroup.blocksRaycasts = false;
        _fadeGroup.interactable   = false;
    }

    IEnumerator FadeOverlayKe(float target, float durasi)
    {
        if (_fadeGroup == null) yield break;
        float from = _fadeGroup.alpha;
        float t = 0f;
        while (t < durasi)
        {
            t += Time.deltaTime;
            _fadeGroup.alpha = Mathf.Lerp(from, target, durasi <= 0f ? 1f : t / durasi);
            yield return null;
        }
        _fadeGroup.alpha = target;
    }

    void UpdateLabelFase(Phase p)
    {
        if (_labelFase == null) return;
        string lokasi = p switch
        {
            Phase.Intro   => "Hari 2 \u00B7 Persiapan",
            Phase.Halte   => "Hari 2 \u00B7 Halte Angkot",
            Phase.Angkot  => "Hari 2 \u00B7 Di Dalam Angkot",
            Phase.Sentuh  => "Hari 2 \u00B7 Di Dalam Angkot",
            Phase.Quiz    => "Hari 2 \u00B7 Quiz Zona Tubuh",
            Phase.Lapor   => "Hari 2 \u00B7 Lapor",
            Phase.ChatSim => "Hari 2 \u00B7 Di Dalam Angkot",
            Phase.EduCard => "Hari 2 \u00B7 Kartu Edukasi",
            _             => ""
        };
        _labelFase.text = lokasi;
    }

    // ══════════════════════════════════════════════════════════════════════
    // INTRO PANEL — style DAY 1 PROLOG (gold border + dark panel + title/body/btn)
    // Semua nilai bisa di-custom dari Inspector header "Intro Slide".
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator TampilkanIntro()
    {
        _introPanel = new GameObject("Day2_IntroPanel");
        var canvas = _introPanel.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 980;
        var scaler = _introPanel.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        _introPanel.AddComponent<GraphicRaycaster>();

        // ── Dim overlay (full-screen) ──────────────────────────────────
        var dim = new GameObject("Dim");
        dim.transform.SetParent(_introPanel.transform, false);
        var drt = dim.AddComponent<RectTransform>();
        drt.anchorMin = Vector2.zero; drt.anchorMax = Vector2.one;
        drt.offsetMin = Vector2.zero; drt.offsetMax = Vector2.zero;
        var dImg = dim.AddComponent<Image>();
        dImg.color = introDimWarna;
        dImg.raycastTarget = true;   // tangkap klik agar tidak tembus ke world

        // ── Card panel (style Day 1: dark + outline gold) ──────────────
        var card = new GameObject("Card");
        card.transform.SetParent(_introPanel.transform, false);
        var crt = card.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(0.5f, 0.5f);
        crt.anchorMax = new Vector2(0.5f, 0.5f);
        crt.pivot     = new Vector2(0.5f, 0.5f);
        crt.sizeDelta = new Vector2(introPanelLebar, introPanelTinggi);
        var cImg = card.AddComponent<Image>();
        if (introPanelSprite != null)
        {
            cImg.sprite = introPanelSprite;
            cImg.type   = Image.Type.Simple;
            cImg.color  = Color.white;
        }
        else
        {
            cImg.color = introPanelWarna;
            var outl = card.AddComponent<Outline>();
            outl.effectColor    = introBorderWarna;
            outl.effectDistance = new Vector2(3f, -3f);
        }
        cImg.raycastTarget = false;

        float pH = introPaddingH;
        float pV = introPaddingV;

        // ── Judul (gold, atas-tengah) — sembunyikan kalau kosong ───────
        if (!string.IsNullOrEmpty(introJudul))
        {
            var titleGO = new GameObject("Judul");
            titleGO.transform.SetParent(card.transform, false);
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) { titleTMP.font = fontAsset; if (fontAsset.material != null) titleTMP.fontSharedMaterial = fontAsset.material; }
            else if (TMP_Settings.defaultFontAsset != null) titleTMP.font = TMP_Settings.defaultFontAsset;
            titleTMP.text      = introJudul;
            titleTMP.fontSize  = introJudulUkuran;
            titleTMP.color     = introJudulWarna;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Top;
            titleTMP.textWrappingMode = TextWrappingModes.Normal;
            titleTMP.raycastTarget = false;
            var trtJ = titleTMP.rectTransform;
            trtJ.anchorMin = new Vector2(0f, 1f); trtJ.anchorMax = new Vector2(1f, 1f);
            trtJ.pivot     = new Vector2(0.5f, 1f);
            trtJ.offsetMin = new Vector2(pH, -(pV + introJudulUkuran + 12f));
            trtJ.offsetMax = new Vector2(-pH, -pV);
        }

        // ── Narasi (putih, tengah) ─────────────────────────────────────
        var txtGO = new GameObject("Narasi");
        txtGO.transform.SetParent(card.transform, false);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) { tmp.font = fontAsset; if (fontAsset.material != null) tmp.fontSharedMaterial = fontAsset.material; }
        else if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
        tmp.text      = introNarasi;
        tmp.fontSize  = introUkuran;
        tmp.color     = introWarna;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        var trt = tmp.rectTransform;
        trt.anchorMin = new Vector2(0f, 0f); trt.anchorMax = new Vector2(1f, 1f);
        // Sisakan ruang untuk judul (atas) & tombol (bawah)
        float topReserve = string.IsNullOrEmpty(introJudul) ? pV : (pV + introJudulUkuran + 18f);
        float botReserve = introBtnTinggi + introBtnOffsetBawah + 16f;
        trt.offsetMin = new Vector2(pH, botReserve);
        trt.offsetMax = new Vector2(-pH, -topReserve);

        // ── Tombol MULAI (hijau / sprite custom) ───────────────────────
        var btnGO = new GameObject("MulaiBtn");
        btnGO.transform.SetParent(card.transform, false);
        var brt = btnGO.AddComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.5f, 0f);
        brt.anchorMax = new Vector2(0.5f, 0f);
        brt.pivot     = new Vector2(0.5f, 0f);
        brt.sizeDelta = new Vector2(introBtnLebar, introBtnTinggi);
        brt.anchoredPosition = new Vector2(0f, introBtnOffsetBawah);
        var bImg = btnGO.AddComponent<Image>();
        if (introBtnSprite != null)
        {
            bImg.sprite = introBtnSprite;
            bImg.type   = Image.Type.Simple;
            bImg.color  = Color.white;
        }
        else
        {
            bImg.color = introBtnWarna;
            var bOutl = btnGO.AddComponent<Outline>();
            bOutl.effectColor    = new Color(introBtnWarna.r + 0.25f, introBtnWarna.g + 0.25f, introBtnWarna.b + 0.25f, 1f);
            bOutl.effectDistance = new Vector2(2f, -2f);
        }
        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = bImg;

        var btnTxtGO = new GameObject("Label");
        btnTxtGO.transform.SetParent(btnGO.transform, false);
        var btmp = btnTxtGO.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) { btmp.font = fontAsset; if (fontAsset.material != null) btmp.fontSharedMaterial = fontAsset.material; }
        else if (TMP_Settings.defaultFontAsset != null) btmp.font = TMP_Settings.defaultFontAsset;
        btmp.text      = introTombolTeks;
        btmp.fontSize  = introBtnUkuran;
        btmp.color     = introBtnTextWarna;
        btmp.alignment = TextAlignmentOptions.Center;
        btmp.fontStyle = FontStyles.Bold;
        btmp.raycastTarget = false;
        var btrt = btmp.rectTransform;
        btrt.anchorMin = Vector2.zero; btrt.anchorMax = Vector2.one;
        btrt.offsetMin = Vector2.zero; btrt.offsetMax = Vector2.zero;

        bool clicked = false;
        float startTime = Time.time;
        btn.onClick.AddListener(() =>
        {
            if (Time.time - startTime < introMinDetik) return;
            AudioManager.Instance?.Click();
            clicked = true;
        });

        // ── Wait loop: keyboard SPACE/ENTER juga bisa lanjut ──────────
        while (!clicked)
        {
            if (introAdvanceOnKeyboard && Time.time - startTime >= introMinDetik)
            {
                if (Input.GetKeyDown(KeyCode.Space)
                 || Input.GetKeyDown(KeyCode.Return)
                 || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    AudioManager.Instance?.Click();
                    clicked = true;
                    break;
                }
            }
            yield return null;
        }

        if (_introPanel != null) Destroy(_introPanel);
        _introPanel = null;
    }

    // ══════════════════════════════════════════════════════════════════════
    // OVERLAY JUDUL HARI — tampil sebelum panel intro (style Day 1)
    // Mendukung sprite background + ornamen garis + judul/lokasi/hint.
    // Semua bisa di-custom dari Inspector header "OVERLAY JUDUL HARI".
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator TampilkanOverlayJudul()
    {
        var cGO = new GameObject("Day2_OverlayJudul");
        var cv  = cGO.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 985;
        var sc = cGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920f, 1080f);
        sc.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        var cg = cGO.AddComponent<CanvasGroup>();
        cg.alpha          = 0f;
        cg.blocksRaycasts = false;

        // Background — sprite jika tersedia, fallback warna solid
        var bgImg = OvBuatImage(cGO.transform, "BG", Vector2.zero, Vector2.one, warnaBackground);
        if (overlayBgSprite != null)
        {
            bgImg.sprite         = overlayBgSprite;
            bgImg.type           = Image.Type.Simple;
            bgImg.color          = Color.white;
            bgImg.preserveAspect = false;
        }

        // Garis dekoratif atas & bawah dihilangkan — sprite background (sprites/UI day 1/6.png)
        // sudah punya ornamen border sendiri, jadi 2 garis kuning ini bikin double & ganggu visual.

        // Baris pertama (mis. "HARI 2:")
        OvBuatTMP(cGO.transform, "Judul1",
            new Vector2(0.05f, 0.565f), new Vector2(0.95f, 0.685f),
            barisPertama + ":", ukuranFontJudul + 8, warnaTeksJudul, true);

        // Baris kedua (mis. "Naik Angkot ke Sekolah")
        OvBuatTMP(cGO.transform, "Judul2",
            new Vector2(0.05f, 0.44f), new Vector2(0.95f, 0.565f),
            barisKedua, ukuranFontJudul, warnaTeksJudul, true);

        // Lokasi
        OvBuatTMP(cGO.transform, "Lokasi",
            new Vector2(0.05f, 0.31f), new Vector2(0.95f, 0.43f),
            "\uD83D\uDCCD  " + teksLokasi, ukuranFontLokasi, warnaTeksLokasi, false);

        // Hint kecil
        OvBuatTMP(cGO.transform, "Hint",
            new Vector2(0.15f, 0.22f), new Vector2(0.85f, 0.305f),
            teksHint, 22, new Color(1f, 1f, 1f, 0.45f), false);

        // Fade in
        for (float t = 0f; t < durasiTransisi; t += Time.deltaTime)
        { cg.alpha = t / durasiTransisi; yield return null; }
        cg.alpha = 1f;

        // Tahan
        yield return new WaitForSeconds(durasiTampil);

        // Fade out
        for (float t = 0f; t < durasiTransisi; t += Time.deltaTime)
        { cg.alpha = 1f - t / durasiTransisi; yield return null; }
        cg.alpha = 0f;

        Destroy(cGO);
    }

    // ── Helper UI untuk overlay judul ───────────────────────────────────
    Image OvBuatImage(Transform parent, string name, Vector2 ancMin, Vector2 ancMax, Color warna)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = warna;
        img.raycastTarget = false;
        return img;
    }

    TextMeshProUGUI OvBuatTMP(Transform parent, string name,
        Vector2 ancMin, Vector2 ancMax,
        string text, int fontSize, Color color, bool bold)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) { tmp.font = fontAsset; if (fontAsset.material != null) tmp.fontSharedMaterial = fontAsset.material; }
        else if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        if (bold) tmp.fontStyle = FontStyles.Bold;
        return tmp;
    }

#if UNITY_EDITOR
    // Auto-load sprite overlay default saat komponen ditambahkan / Reset
    void Reset()
    {
        TryLoadOverlayBgSpriteDefault();
    }

    [ContextMenu("\u25b6 Muat Sprite Overlay BG Default (UI day 1/6.png)")]
    void TryLoadOverlayBgSpriteDefault()
    {
        if (overlayBgSprite != null) return;
        if (string.IsNullOrEmpty(overlayBgSpritePath)) return;
        var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/" + overlayBgSpritePath);
        if (sp != null)
        {
            overlayBgSprite = sp;
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[Day2Controller] Overlay BG sprite di-assign: {sp.name}");
        }
    }
#endif
}
