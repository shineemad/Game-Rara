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

    [Header("Tombol Teriak (untuk yang tidak pakai mic)")]
    public Button  shoutButton;
    public Slider  shoutGauge;          // gauge teriak (0–1)
    public float   shoutFillRate = 0.5f;
    public float   shoutDecayRate = 0.3f;

    [Header("Tutorial Rintangan")]
    [Tooltip("Rintangan merah di tutorial. Kosong = dibuat otomatis saat runtime.")]
    public GameObject tutorialObstacle;

    // ── Konfigurasi Encounter (edit dari Inspector) ────────────────────────
    [Header("━━ KONFIGURASI DIALOG ENCOUNTER ━━")]
    [Tooltip("Isi semua dialog & pilihan Encounter 1 dari sini. Klik ▶ untuk expand.")]
    public EncounterConfig encounter1 = new EncounterConfig
    {
        encounterName        = "Encounter 1 — Orang Asing Penawar Permen",
        pertanyaanRara       = "Gimana Rara harus merespons orang ini?",
        dialogSebelumPilihan = new DialogLine[]
        {
            new DialogLine { speaker = "Narasi",
                text = "Langkah Rara terhenti. Seorang pria asing berdiri di tengah trotoar,\ntersenyum sambil menggenggam sebungkus permen warna-warni." },
            new DialogLine { speaker = "Orang Asing",
                text = "\"Hei dek, bentar ya~!\nMau permen nggak? Enak banget loh!\"" },
            new DialogLine { speaker = "Orang Asing",
                text = "\"Om punya banyak di warung deket sini.\nIkut bentar aja ya, deket kok!\"" },
            new DialogLine { speaker = "Rara (dalam hati)",
                text = "Aku nggak kenal orang ini sama sekali...\nDia nawarin permen DAN ngajak pergi \u2014 ini tanda bahaya!" },
        },
        pilihan = new ChoiceConfig[]
        {
            new ChoiceConfig
            {
                label         = "\"NGGAK MAU! Aku nggak kenal Bapak!\" (Teriak & lari ke tempat ramai)",
                category      = "AMAN",
                
                feedbackText  = "✅ Bagus sekali! Rara menolak dengan tegas!\nOrang asing yang menawarkan hadiah dan mengajak pergi = TANDA BAHAYA!\nSelalu tolak dan pergi ke tempat yang ramai."
            },
            new ChoiceConfig
            {
                label         = "\"Makasih pak, tapi aku sudah mau telat sekolah...\" (Menolak dengan alasan)",
                category      = "RAGU",
                
                feedbackText  = "⚠️ Lumayan... Rara menolak, tapi kurang tegas.\nSebaiknya langsung pergi ke tempat yang lebih ramai\ndan ceritakan ke orang dewasa yang dipercaya."
            },
            new ChoiceConfig
            {
                label         = "\"Boleh~\" (Ikut saja)",
                category      = "BAHAYA",
                
                feedbackText  = "❌ BAHAYA! Rara kehilangan ❤ karena ikut orang asing!\nJANGAN PERNAH ikut dengan orang yang tidak dikenal,\napapun yang ditawarkan!"
            }
        }
    };

    [Tooltip("Isi semua dialog & pilihan Encounter 2 dari sini.")]
    public EncounterConfig encounter2 = new EncounterConfig
    {
        encounterName        = "Encounter 2 — Motor Nyasar Tanya Sekolah",
        pertanyaanRara       = "Bagaimana Rara harus merespons orang ini?",
        dialogSebelumPilihan = new DialogLine[]
        {
            new DialogLine { speaker = "Narasi",
                text = "Sebuah motor matic berhenti pelan di sebelah Rara.\nPengendaranya laki-laki, helm setengah terbuka, tersenyum lebar." },
            new DialogLine { speaker = "Pengendara Motor",
                text = "\"Eh dek, maaf nih\u2026\nKamu tau nggak SMP Harapan di mana?\nOm nyasar, mau anter ponakan.\"" },
            new DialogLine { speaker = "Pengendara Motor",
                text = "\"Naik aja deh, kebetulan om searah.\nDaripada kamu jalan kepanasan, mending bareng om aja ya?\"" },
            new DialogLine { speaker = "Rara (dalam hati)",
                text = "Hmm\u2026 dia bilang nyasar tapi langsung nawarin tumpangan?\nOrang asing yang tiba-tiba ramah begini bisa jadi tanda bahaya." },
        },
        pilihan = new ChoiceConfig[]
        {
            new ChoiceConfig
            {
                label         = "\"Maaf om, aku nggak boleh ikut orang asing!\" (Mundur & lari ke tempat ramai)",
                category      = "AMAN",
                feedbackText  = "✅ Tepat sekali!\nORANG TIDAK DIKENAL = JANGAN PERNAH IKUT, walau alasannya seramah apapun.\nKalau ditanya jalan, cukup tunjuk arah dari jauh — jangan dekati & jangan naik."
            },
            new ChoiceConfig
            {
                label         = "\"SMP Harapan lurus aja om\u2026 maaf aku nggak bisa nganterin.\" (Jawab dari jauh)",
                category      = "RAGU",
                feedbackText  = "⚠️ Lumayan — Rara nggak naik motor, tapi terlalu lama ngobrol.\nLebih baik langsung pergi ke tempat ramai. Jangan kasih info detail ke orang asing."
            },
            new ChoiceConfig
            {
                label         = "\"Boleh om, ayo!\" (Naik motor)",
                category      = "BAHAYA",
                feedbackText  = "❌ BAHAYA! Rara hampir naik motor orang asing!\nNAIK KENDARAAN ORANG TAK DIKENAL = jalan menuju penculikan.\nKamu kehilangan ❤. Ingat: \"Searah\" cuma alasan grooming yang umum dipakai."
            }
        }
    };

    [Tooltip("Isi semua dialog & pilihan Encounter 3 dari sini.")]
    public EncounterConfig encounter3 = new EncounterConfig
    {
        encounterName        = "Encounter 3 — Pesan Mencurigakan di HP",
        pertanyaanRara       = "Apa yang harus Rara lakukan dengan pesan ini?",
        dialogSebelumPilihan = new DialogLine[]
        {
            new DialogLine { speaker = "Narasi",
                text = "Gerbang SMP Harapan sudah terlihat dari kejauhan.\nTiba-tiba HP Rara bergetar di saku." },
            new DialogLine { speaker = "Narasi",
                text = "📱 Pesan dari nomor tidak dikenal:\n\"Hei Rara, aku tau kamu lagi di jalan. Mau aku jemput?\"" },
            new DialogLine { speaker = "Rara (dalam hati)",
                text = "Hah?! Kok dia tau namaku?\nDan dia tau aku lagi di jalan... ini menakutkan." },
        },
        pilihan = new ChoiceConfig[]
        {
            new ChoiceConfig
            {
                label         = "Screenshot lalu blokir nomor dan cerita ke Mama",
                category      = "AMAN",
                
                feedbackText  = "✅ Tepat! Screenshot sebagai bukti, blokir nomornya,\ndan SELALU ceritakan ke orang dewasa yang dipercaya."
            },
            new ChoiceConfig
            {
                label         = "Balas: \"Siapa kamu?\" (penasaran)",
                category      = "RAGU",
                
                feedbackText  = "⚠️ Membalas pesan orang asing bisa berbahaya.\nLebih baik abaikan, blokir, dan lapor ke orang tua."
            },
            new ChoiceConfig
            {
                label         = "Ikuti ajakannya (sangat berbahaya!)",
                category      = "BAHAYA",
                
                feedbackText  = "❌ SANGAT BERBAHAYA! Jangan pernah temui orang asing\nyang hanya kamu kenal lewat pesan/medsos!"
            }
        }
    };

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
    bool    npcActive    = false;
    float   shoutLevel   = 0f;
    bool    shoutHeld    = false;
    bool    enc1Done     = false;
    bool    enc2Done     = false;
    bool    enc3Done     = false;
    bool    tutorialStarted = false;   // guard: ShowTutorial hanya dipanggil sekali
    string  lastZoneLabel   = null;     // label zona segmen aktif (null = belum di-set)

    // Referensi yang di-cache di Start(), dipakai di OnDestroy untuk lepas listener.
    Day1Intro _introRef;

    [Header("Intro & Start")]
    [Tooltip("Centang jika ingin langsung mulai tanpa Day1Intro (untuk testing).")]
    public bool autoMulaiTanpaIntro = false;

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
        if (_introRef != null && _introRef.onIntroSelesai != null)
            _introRef.onIntroSelesai.RemoveListener(MulaiGame);
        if (eduCardContinueBtn != null)
            eduCardContinueBtn.onClick.RemoveListener(GoToResult);
    }

    /// Dipanggil saat Day1Intro selesai (otomatis via AddListener di Start, atau dari Inspector).
    public void MulaiGame()
    {
        // Guard: cegah double-call hanya jika sudah melewati Tutorial atau lebih jauh
        if (currentPhase != Phase.Intro && currentPhase != Phase.Tutorial) return;

        if (hudManager    == null) hudManager    = HUDManager.Instance;
        if (dialogManager == null) dialogManager = FindFirstObjectByType<DialogManager>();

        hudManager?.Refresh();

        // Selalu pastikan player tidak frozen, apapun fase-nya
        var p = player != null
            ? player.GetComponent<player>()
            : FindFirstObjectByType<player>();
        if (p != null) p.frozen = false;

        // Hanya ubah fase jika masih di Intro
        if (currentPhase == Phase.Intro)
        {
            dialogActive = false;
            currentPhase = Phase.Tutorial;
        }
        else
        {
            // Sudah di Tutorial — pastikan dialogActive juga false
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
    void Update()
    {
        if (dialogActive) return;

        HandleShout();
        CheckEncounterTriggers();
        UpdateZoneSegment();

        if (currentPhase == Phase.Encounter1 && npcActive) HandleNPCApproach();
        if (currentPhase == Phase.Encounter2 && npcActive) HandleNPCApproach();
        if (currentPhase == Phase.Encounter3 && npcActive) HandleNPCApproach();

        hudManager?.UpdateScore(GameState.Instance?.score ?? 0);
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
        // ── 1. Dialog petunjuk TERIAK ─────────────────────────────────────
        dialogActive = true;
        bool dialogDone = false;
        GetSharedDialog().PlayLines(new NpcDialog.DialogEntry[]
        {
            new NpcDialog.DialogEntry
            {
                speakerName = "Narasi",
                text        = "📢 Tekan & TAHAN tombol TERIAK untuk berlatih.\nTeriak sampai meter PENUH — rintangan di depan akan terdorong!"
            }
        }, () => dialogDone = true);
        // Safety timeout 60 detik — cegah WaitUntil hang jika callback tidak pernah terpanggil
        float deadline = Time.time + 60f;
        yield return new WaitUntil(() => dialogDone || Time.time > deadline);
        if (!dialogDone)
            Debug.LogWarning("[Day1Controller] ShowTutorial: dialog timeout, lanjut paksa.");
        dialogActive = false;

        // ── 2. Munculkan rintangan merah ────────────────────────────────
        var obstacle = (tutorialObstacle != null)
            ? tutorialObstacle
            : BuildTutorialObstacle();
        obstacle.SetActive(true);

        // Animasi masuk: tumbuh dari atas ke bawah
        var sr = obstacle.GetComponent<SpriteRenderer>();
        Vector3 fullScale = obstacle.transform.localScale;
        obstacle.transform.localScale = new Vector3(fullScale.x, 0f, 1f);
        for (float t = 0f; t < 0.35f; t += Time.deltaTime)
        {
            obstacle.transform.localScale = Vector3.Lerp(
                new Vector3(fullScale.x, 0f, 1f), fullScale, t / 0.35f);
            yield return null;
        }
        obstacle.transform.localScale = fullScale;

        // ── 3. Reset gauge & tunggu TERIAK penuh ─────────────────────────
        shoutLevel = 0f;
        hudManager?.SetShoutGauge(0f);

        // Timeout 120 detik — cegah stuck jika shout tidak terdeteksi
        float shoutDeadline = Time.time + 120f;
        while (shoutLevel < 0.96f && Time.time < shoutDeadline)
        {
            // Kedipkan obstacle saat gauge di atas 60% sebagai umpan balik
            if (sr != null)
                sr.color = shoutLevel > 0.6f
                    ? Color.Lerp(new Color(0.9f, 0.15f, 0.15f, 0.9f), Color.white,
                                 (shoutLevel - 0.6f) / 0.4f)
                    : new Color(0.9f, 0.15f, 0.15f, 0.9f);
            yield return null;
        }

        // ── 4. Hancurkan rintangan + lanjut ──────────────────────────────
        yield return StartCoroutine(PlayObstacleEffect(obstacle));
        // Pastikan player tidak frozen dan bisa jalan bebas
        dialogActive = false;
        currentPhase = Phase.Walking;
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
                // Urutan 3-tantangan konsep: Paman → Motor → PathChoice (gang gelap).
                if (px >= encE1 && !enc1Done)
                    StartEncounter1();
                else if (px >= encE2 && enc1Done && !enc2Done)
                    StartEncounter2();
                else if (px >= encPathChoice && enc2Done && !pathChosen)
                    ShowPathChoice();
                break;

            case Phase.Walking2:
                // Setelah PathChoice — lanjut sampai Edu Card.
                if (px >= encEduCard && pathChosen)
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

    // ── Encounter 1: Orang Asing di Jalan ─────────────────────────────────
    void StartEncounter1()
    {
        if (enc1Done) return;
        enc1Done     = true;
        currentPhase = Phase.Encounter1;

        if (npcStranger != null)
        {
            npcStranger.SetActive(true);
            npcStranger.transform.position = new Vector3(
                player.transform.position.x + 5f,
                player.transform.position.y, 0f);
            npcActive = true;
        }

        // Langsung putar dialog via PlayLines (bukan menunggu PamanBaik.Play())
        // agar onSelect callback dari BangunEncounterLines benar-benar terpanggil.
        dialogActive = true;
        var npcDialog = GetSharedDialog();
        npcDialog.lines = BangunEncounterLines(encounter1, 1,
            onAman:   () => { GameState.Instance?.EarnAchievement("Tolak Orang Asing"); DismissNPC(); },
            onRagu:   () => { DismissNPC(); },
            onBahaya: () => { DismissNPC(); },
            afterFeedback: null);

        npcDialog.PlayLines(npcDialog.lines, () =>
        {
            dialogActive = false;
            currentPhase = Phase.Walking;
            npcActive    = false;
            if (npcStranger != null) npcStranger.SetActive(false);
            AudioManager.Instance?.Correct();
        });
    }

    // ── Path Choice: Jalan Aman vs Gang Sepi ──────────────────────────────
    void ShowPathChoice()
    {
        if (pathChosen) return;
        pathChosen = true;
        dialogActive = true;
        currentPhase = Phase.PathChoice;

        // Narasi setup dulu, baru tampilkan panel pilihan jalan.
        StartCoroutine(ShowPathChoiceWithNarasi());
    }

    IEnumerator ShowPathChoiceWithNarasi()
    {
        bool done = false;
        GetSharedDialog()?.PlayLines(new NpcDialog.DialogEntry[]
        {
            new NpcDialog.DialogEntry
            {
                speakerName = "Narasi",
                text        = "Di depan ada percabangan jalan.\nKiri: jalan utama yang ramai, ada warung dan tukang ojek.\nKanan: gang pintas yang sepi, lebih cepat tapi lengang."
            },
            new NpcDialog.DialogEntry
            {
                speakerName = "Rara (dalam hati)",
                text        = "Mana yang sebaiknya aku pilih?"
            }
        }, () => done = true);

        float deadline = Time.time + 60f;
        yield return new WaitUntil(() => done || Time.time > deadline);

        if (pathChoicePanel != null) pathChoicePanel.SetActive(true);
    }

    /// Dipanggil oleh tombol di pathChoicePanel.
    public void ChooseSafePath()
    {
        GameState.Instance.pathChoice = "safe";
        GameState.Instance.AddChoice(1, "Pilih jalan aman yang ramai", "AMAN");
        if (pathChoicePanel != null) pathChoicePanel.SetActive(false);

        // Aktifkan tampilan Jalan Ramai
        pathEnvironment?.AktifkanJalanRamai();

        AudioManager.Instance?.Correct();
        StartCoroutine(ShowSafePathNarasi());
    }

    IEnumerator ShowSafePathNarasi()
    {
        dialogActive = true;
        bool done = false;
        GetSharedDialog()?.PlayLines(new NpcDialog.DialogEntry[]
        {
            new NpcDialog.DialogEntry
            {
                speakerName = "Narasi",
                text        = "Rara memilih jalan utama. Suara klakson, tawa anak warung,\ndan langkah orang lalu-lalang membuat hatinya lebih tenang."
            }
        }, () => done = true);
        float deadline = Time.time + 30f;
        yield return new WaitUntil(() => done || Time.time > deadline);
        dialogActive = false;
        currentPhase = Phase.Walking2;
    }

    public void ChooseDangerPath()
    {
        GameState.Instance.pathChoice = "dangerous";
        GameState.Instance.AddChoice(1, "Pilih gang sepi sebagai jalan pintas", "BAHAYA");
        bool alive = GameState.Instance.LoseLife();
        hudManager?.FlashHeartLost(GameState.Instance.lives);

        if (pathChoicePanel != null) pathChoicePanel.SetActive(false);

        // Aktifkan tampilan Gang Sepi (gelap)
        pathEnvironment?.AktifkanGangSepi();

        if (!alive)
        {
            SceneLoader.Instance?.LoadScene("GameOver");
            return;
        }

        // Tetap lanjut tapi dengan konsekuensi
        StartCoroutine(ShowDangerPathWarning());
    }

    IEnumerator ShowDangerPathWarning()
    {
        dialogActive = true;
        bool done = false;
        GetSharedDialog()?.PlayLines(new NpcDialog.DialogEntry[]
        {
            new NpcDialog.DialogEntry
            {
                speakerName = "Narasi",
                text        = "Rara melangkah masuk ke gang. Lampu jalan padam,\ndinding-dinding tinggi menelan suara apa pun."
            },
            new NpcDialog.DialogEntry
            {
                speakerName = "Narasi",
                text        = "Hanya ada suara langkah Rara\u2026 dan langkah lain dari belakang.\nJantungnya berdegup kencang. ❤ −1"
            },
            new NpcDialog.DialogEntry
            {
                speakerName = "Rara (dalam hati)",
                text        = "Harusnya aku ambil jalan ramai tadi\u2026"
            }
        }, () => done = true);
        float deadline = Time.time + 45f;
        yield return new WaitUntil(() => done || Time.time > deadline);
        dialogActive = false;
        currentPhase = Phase.Walking2;
    }

    // ── Encounter 2: Motor Nyasar Tanya Sekolah ───────────────────────────
    void StartEncounter2()
    {
        if (enc2Done) return;
        enc2Done     = true;
        currentPhase = Phase.Encounter2;
        dialogActive = true;

        ActivateNPCAt(player.transform.position.x + 4f);

        var npcDialog = GetSharedDialog();
        npcDialog.lines = BangunEncounterLines(encounter2, 1,
            onAman:   () => { GameState.Instance?.EarnAchievement("Tolak Tumpangan Asing"); DismissNPC(); },
            onRagu:   () => { DismissNPC(); },
            onBahaya: () => { DismissNPC(); },
            afterFeedback: null);

        npcDialog.PlayLines(npcDialog.lines, () =>
        {
            dialogActive = false;
            // Kembali ke Walking — pemain lanjut sampai mencapai trigger PathChoice (#3).
            currentPhase = Phase.Walking;
        });
    }

    // ── Encounter 3: Pesan Mencurigakan di HP ─────────────────────────────
    void StartEncounter3()
    {
        if (enc3Done) return;
        enc3Done     = true;
        currentPhase = Phase.Encounter3;
        dialogActive = true;

        var npcDialog = GetSharedDialog();
        npcDialog.lines = BangunEncounterLines(encounter3, 1,
            onAman:   () =>
            {
                // Bonus: screenshot → achievement + skor lapor
                GameState.Instance.screenshotTaken = true;
                GameState.Instance?.EarnAchievement("Screenshot & Laporkan");
            },
            onRagu:   null,
            onBahaya: null,
            afterFeedback: null);

        npcDialog.PlayLines(npcDialog.lines, () =>
        {
            dialogActive = false;
            enc3Done     = true;
            currentPhase = Phase.Walking3;  // izinkan EduCard trigger dari CheckEncounterTriggers
        });
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
                                    () => SceneLoader.Instance?.LoadScene("GameOver")));
                                return;
                            }
                        }

                        // Callback tambahan khusus encounter (achievement, dll)
                        if (kategori == "AMAN")        onAman?.Invoke();
                        else if (kategori == "RAGU")   onRagu?.Invoke();
                        else                           onBahaya?.Invoke();

                        // Tampilkan feedback edukasi
                        StartCoroutine(TampilkanFeedback(feedbackTeks, kategori, afterFeedback));
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
    IEnumerator TampilkanFeedback(string pesan, string kategori, System.Action onSelesai = null)
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
            default:       infoPoin = "  (−1 ❤  |  +0 poin)";              break;
        }

        string judulFeedback = kategori == "AMAN"   ? $"✅ Keputusan Tepat!{infoPoin}"
                             : kategori == "RAGU"   ? $"⚠ Perlu Lebih Tegas{infoPoin}"
                             :                        $"❌ Keputusan Berbahaya!{infoPoin}";

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

        onSelesai?.Invoke();
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

        if (eduCardPanel != null) eduCardPanel.SetActive(true);
        if (eduCardContinueBtn != null)
        {
            // RemoveListener dulu agar bisa di-call ulang tanpa menumpuk subscriber.
            eduCardContinueBtn.onClick.RemoveListener(GoToResult);
            eduCardContinueBtn.onClick.AddListener(GoToResult);
        }

        GameState.Instance.checkpointD1 = true;
    }

    public void GoToResult()
    {
        currentPhase = Phase.Complete;
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
            if (!alive) SceneLoader.Instance?.LoadScene("GameOver");
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // SHOUT / TERIAK — dikendalikan VoiceMeter (mikrofon HP/PC)
    // ══════════════════════════════════════════════════════════════════════

    void HandleShout()
    {
        // Prioritas: gunakan VoiceMeter jika tersedia
        if (VoiceMeter.Instance != null)
        {
            // shoutLevel = NormalizedLevel dari mic (atau fallback tombol/SpaceBar)
            shoutLevel = VoiceMeter.Instance.NormalizedLevel;

            // Efek kecepatan player berdasarkan level suara
            AplikasiEfekSuara(VoiceMeter.Instance.Level);
        }
        else
        {
            // Fallback lama: tombol shout / SpaceBar
            bool spaceheld = Input.GetKey(KeyCode.Space);
            bool isShout   = shoutHeld || spaceheld;
            if (isShout)
                shoutLevel = Mathf.Min(1f, shoutLevel + shoutFillRate * Time.deltaTime);
            else
                shoutLevel = Mathf.Max(0f, shoutLevel - shoutDecayRate * Time.deltaTime);
        }

        if (shoutGauge != null) shoutGauge.value = shoutLevel;
        hudManager?.SetShoutGauge(shoutLevel);
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
                break;
            case VoiceMeter.VoiceLevel.Medium:
                p.voiceSpeedMultiplier = 0.55f;  // suara sedang → ragu, lambat
                break;
            default:
                p.voiceSpeedMultiplier = 1.0f;   // normal / diam → biasa
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

    // ══════════════════════════════════════════════════════════════════════
    // TUTORIAL OBSTACLE HELPERS
    // ══════════════════════════════════════════════════════════════════════

    /// Buat rintangan merah (3 bar vertikal) yang memblokir jalur player.
    GameObject BuildTutorialObstacle()
    {
        // Tentukan posisi: tepat di depan player
        float ox = player != null ? player.transform.position.x + 3.5f : encTutorial + 2f;
        float oy = player != null ? player.transform.position.y + 1.5f : 1.5f;

        var root = new GameObject("TutorialObstacle");
        root.transform.position = new Vector3(ox, oy, 0f);

        // Collider pada root (menghalangi player secara fisika)
        var col = root.AddComponent<BoxCollider2D>();
        col.size   = new Vector2(1.8f, 4.0f);
        col.offset = Vector2.zero;

        // 3 bar merah vertikal sebagai visual
        float[] barOffsets = { -0.55f, 0f, 0.55f };
        foreach (float xOff in barOffsets)
        {
            var bar = new GameObject("Bar");
            bar.transform.SetParent(root.transform, false);
            bar.transform.localPosition = new Vector3(xOff, 0f, 0f);
            bar.transform.localScale    = new Vector3(0.28f, 4.0f, 1f);

            var sr = bar.AddComponent<SpriteRenderer>();
            sr.sprite       = MakeSolidSprite(4, 64);
            sr.color        = new Color(0.90f, 0.14f, 0.14f, 0.92f);
            sr.sortingOrder = 8;
        }

        // Tanda ⚠ di tengah (bar lebih terang)
        var warnBar = root.transform.Find("Bar");    // bar tengah sudah ada
        // Warnanya sedikit lebih terang untuk beda
        var bars = root.GetComponentsInChildren<SpriteRenderer>();
        if (bars.Length >= 2) bars[1].color = new Color(1f, 0.25f, 0.25f, 0.95f);

        root.SetActive(false);   // disembunyikan sampai ShowTutorial aktifkan
        return root;
    }

    /// Efek hancur: flash putih → scale down → nonaktif.
    IEnumerator PlayObstacleEffect(GameObject obstacle)
    {
        if (obstacle == null) yield break;
        var renderers = obstacle.GetComponentsInChildren<SpriteRenderer>();

        // Flash: merah → putih × 3
        for (int i = 0; i < 4; i++)
        {
            Color c = (i % 2 == 0) ? Color.white : new Color(0.9f, 0.14f, 0.14f, 0.9f);
            foreach (var sr in renderers) sr.color = c;
            yield return new WaitForSeconds(0.07f);
        }

        // Scale down → hancur
        Vector3 startScale = obstacle.transform.localScale;
        for (float t = 0f; t < 0.30f; t += Time.deltaTime)
        {
            obstacle.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t / 0.30f);
            yield return null;
        }

        obstacle.SetActive(false);
    }

    /// Sprite putih solid (width × height piksel) untuk pewarnaan runtime.
    static Sprite MakeSolidSprite(int w, int h)
    {
        var tex    = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }
}
