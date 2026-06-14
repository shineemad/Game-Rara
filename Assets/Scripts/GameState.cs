using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton — Menyimpan status permainan di seluruh scene.
/// Dibuat sekali di scene pertama dan tidak dihancurkan saat pindah scene.
/// </summary>
public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    // ── Status Pemain ──────────────────────────────────────────────────────
    [Header("Status Pemain")]
    public int lives    = 3;
    public int maxLives = 3;
    public int score    = 0;
    public int day      = 1;
    public string playerName = "Rara";

    [Header("Game Over")]
    [Tooltip("Tampilkan layar Game Over otomatis saat nyawa habis (Day 1 & Day 2). " +
             "Day 3 punya layar hasil sendiri, jadi dikecualikan dari pemicu otomatis ini.")]
    public bool autoGameOver = true;

    // ── Keputusan Pemain ───────────────────────────────────────────────────
    [Header("Keputusan")]
    public string pathChoice       = "safe";   // "safe" | "dangerous"
    public bool   screenshotTaken  = false;
    public bool   platChecked      = false;
    [Tooltip("Kategori kursi yang dipilih di Angkot (Day 2): 'AMAN' | 'RAGU' | 'BAHAYA'. " +
             "Dipakai ZonaTubuhQuiz untuk memilih varian narasi intro yang menyambung " +
             "keputusan pemain di AngkotSeatPicker.")]
    public string seatCategory     = "";       // "" | "AMAN" | "RAGU" | "BAHAYA"

    // ── Rekam Pilihan & Pencapaian ─────────────────────────────────────────
    public List<ChoiceRecord> choices      = new List<ChoiceRecord>();
    public List<string>       achievements = new List<string>();

    // ── Inventory Bukti (B2) ───────────────────────────────────────────────
    // Kumpulan bukti yang dikumpulkan pemain di Hari 2 & 3. Ending tertinggi
    // (LAPOR SUKSES di Hari 3) hanya terbuka bila SEMUA bukti ini lengkap.
    public const string BUKTI_CHAT_DAY2 = "chat_day2"; // screenshot chat WhatsApp Hari 2
    public const string BUKTI_PLAT_DAY2 = "plat_day2"; // cek plat angkot Hari 2
    public const string BUKTI_CHAT_DAY3 = "chat_day3"; // screenshot chat ojol Hari 3
    public const string BUKTI_PLAT_DAY3 = "plat_day3"; // cek plat ojol Hari 3
    private readonly HashSet<string> _bukti = new HashSet<string>();
    public int JumlahBukti => _bukti.Count;

    // ── Meteran Bahaya (0..1) ──────────────────────────────────────────────
    // Naik tiap pilihan RAGU/BAHAYA, turun tiap AMAN. Dipakai DangerGauge
    // sebagai umpan balik berkelanjutan seberapa terkendali situasi Rara.
    [Header("Meteran Bahaya")]
    [Range(0f, 1f)] public float dangerLevel = 0f;
    public float dangerNaikBahaya = 0.34f;   // BAHAYA → +
    public float dangerNaikRagu   = 0.15f;   // RAGU   → +
    public float dangerTurunAman  = 0.20f;   // AMAN   → -
    /// Dipanggil tiap dangerLevel berubah (untuk UI bereaksi).
    public event System.Action<float> OnDangerChanged;

    // ── Kata Sakti yang sudah dipakai (TIDAK → PERGI → CERITA) ─────────────
    [Header("Kata Sakti Dikuasai")]
    public bool usedTidak  = false;  // menolak / berkata TIDAK
    public bool usedPergi  = false;  // menjauh / PERGI dari bahaya
    public bool usedCerita = false;  // CERITA / lapor ke orang dewasa

    // ── Checkpoint (agar pemain tidak frustrasi jika Game Over) ───────────
    public bool checkpointD1 = false;
    public bool checkpointD2 = false;
    public bool checkpointD3 = false;

    // ── Nilai Skor Per Kategori ────────────────────────────────────────────
    public const int SCORE_AMAN   = 100;
    public const int SCORE_RAGU   = 50;
    public const int SCORE_BAHAYA = 0;
    public const int SCORE_QUIZ   = 200;
    public const int SCORE_LAPOR  = 500;

    // ── Data Pilihan ───────────────────────────────────────────────────────
    [System.Serializable]
    public class ChoiceRecord
    {
        public int    day;
        public string label;
        public string category; // "AMAN" | "RAGU" | "BAHAYA"
        public int    points;
    }

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Kecerahan 2D: ambient light harus putih agar sprite tampil
            // dengan warna aslinya — default Unity seringkali abu-abu gelap
            RenderSettings.ambientMode  = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = Color.white;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // Watchdog Game Over terpusat: begitu nyawa habis, tampilkan layar Game
    // Over (untuk Day 1 & Day 2). Day 3 dikecualikan karena memakai layar
    // hasil/ending sendiri. GameOverScreen.Show() idempotent, jadi aman walau
    // dipanggil bersamaan dengan pemicu lain.
    void Update()
    {
        if (!autoGameOver) return;
        if (lives <= 0 && day != 3 && !GameOverScreen.IsShowing)
            GameOverScreen.Show();
    }

    // ══════════════════════════════════════════════════════════════════════
    /// Catat satu bukti (HashSet otomatis cegah duplikat).
    public void TambahBukti(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (_bukti.Add(id))
        {
            Debug.Log($"[GameState] Bukti dikumpulkan: {id} | total bukti={_bukti.Count}");
            // Umpan balik visual: toast "📸 Bukti tersimpan!" + perbarui counter HUD.
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.ShowBuktiToast(id);
                HUDManager.Instance.UpdateBukti(day);
            }
        }
    }

    /// Apakah bukti tertentu sudah dimiliki?
    public bool PunyaBukti(string id) => _bukti.Contains(id);

    /// Gerbang ending LAPOR SUKSES: butuh SEMUA bukti Hari 2 & 3.
    public bool SemuaBuktiLengkap() =>
        PunyaBukti(BUKTI_CHAT_DAY2) && PunyaBukti(BUKTI_PLAT_DAY2) &&
        PunyaBukti(BUKTI_CHAT_DAY3) && PunyaBukti(BUKTI_PLAT_DAY3);

    // ══════════════════════════════════════════════════════════════════════
    /// Tambah pilihan dan hitung skor.
    public void AddChoice(int day, string label, string category, int? overridePts = null)
    {
        int pts = overridePts ?? CategoryToPoints(category);
        score += pts;
        Debug.Log($"[GameState] AddChoice: {label} | {category} | +{pts} poin | Total skor={score}");
        choices.Add(new ChoiceRecord
        {
            day      = day,
            label    = label,
            category = category,
            points   = pts
        });

        // Update meteran bahaya berdasarkan kategori pilihan.
        ApplyDanger(category);
        // Deteksi otomatis kata sakti dari label pilihan AMAN.
        DetectKataSakti(label, category);

        // Update HUD langsung agar skor tampil segera
        HUDManager.Instance?.UpdateScore(score);
    }

    /// Tambah skor langsung.
    public void AddScore(int pts)
    {
        score += pts;
        HUDManager.Instance?.UpdateScore(score);
    }

    // ── Meteran Bahaya ──────────────────────────────────────────────────────
    /// Ubah dangerLevel sesuai kategori pilihan, lalu beri tahu listener (UI).
    void ApplyDanger(string category)
    {
        float delta = category switch
        {
            "BAHAYA" => +dangerNaikBahaya,
            "RAGU"   => +dangerNaikRagu,
            "AMAN"   => -dangerTurunAman,
            "LAPOR"  => -dangerTurunAman,
            _        => 0f
        };
        SetDanger(dangerLevel + delta);
    }

    /// Set nilai bahaya (clamp 0..1) + picu event.
    public void SetDanger(float value)
    {
        dangerLevel = Mathf.Clamp01(value);
        OnDangerChanged?.Invoke(dangerLevel);
    }

    // ── Kata Sakti ──────────────────────────────────────────────────────────
    /// Tandai satu kata sakti dipakai: "TIDAK" | "PERGI" | "CERITA".
    public void MarkKataSakti(string kata)
    {
        switch ((kata ?? "").ToUpperInvariant())
        {
            case "TIDAK": usedTidak  = true; break;
            case "PERGI": usedPergi  = true; break;
            case "CERITA": usedCerita = true; break;
        }
    }

    /// Deteksi otomatis kata sakti dari teks label pilihan (hanya untuk AMAN/LAPOR).
    void DetectKataSakti(string label, string category)
    {
        if (category != "AMAN" && category != "LAPOR") return;
        if (string.IsNullOrEmpty(label)) return;
        string u = label.ToUpperInvariant();
        if (u.Contains("TIDAK") || u.Contains("TOLAK") || u.Contains("MENOLAK")) usedTidak = true;
        if (u.Contains("PERGI") || u.Contains("PINDAH") || u.Contains("MENJAUH") || u.Contains("JAUH") || u.Contains("TERIAK")) usedPergi = true;
        if (u.Contains("CERITA") || u.Contains("LAPOR") || u.Contains("SUPIR") || u.Contains("ORTU") || u.Contains("KPAI")) usedCerita = true;
    }

    /// Jumlah kata sakti yang sudah dikuasai (0..3).
    public int KataSaktiDikuasai()
    {
        int n = 0;
        if (usedTidak)  n++;
        if (usedPergi)  n++;
        if (usedCerita) n++;
        return n;
    }

    // ── Achievement Day 2 ────────────────────────────────────────────────────
    /// Evaluasi & beri pencapaian khusus Hari 2 berdasarkan pilihan pemain.
    /// Dipanggil di akhir Day 2 (sebelum layar Summary).
    public void EvaluateDay2Achievements()
    {
        var day2 = choices.FindAll(c => c.day == 2);
        if (day2.Count == 0) return;

        bool adaBahaya = day2.Exists(c => c.category == "BAHAYA");
        bool adaRagu   = day2.Exists(c => c.category == "RAGU");

        // Semua pilihan AMAN sepanjang Hari 2.
        if (!adaBahaya && !adaRagu) EarnAchievement("Si Waspada");
        // Berhasil memakai ketiga kata sakti.
        if (usedTidak && usedPergi && usedCerita) EarnAchievement("Jagoan 3 Kata Sakti");
        // Berani CERITA / lapor.
        if (usedCerita) EarnAchievement("Berani Cerita");
        // Tetap tenang: dangerLevel rendah di akhir.
        if (dangerLevel <= 0.25f) EarnAchievement("Kepala Dingin");
    }

    /// Kurangi nyawa. Return false jika nyawa habis (Game Over).
    public bool LoseLife()
    {
        lives = Mathf.Max(0, lives - 1);
        return lives > 0;
    }

    public bool IsAlive() => lives > 0;

    /// Tambah pencapaian (tidak duplikat). Memunculkan popup AchievementPopup.
    public void EarnAchievement(string name)
    {
        if (achievements.Contains(name)) return;
        achievements.Add(name);
        AchievementPopup.Show(name);
        AudioManager.Instance?.PlayAchievement();
    }

    /// Nilai akhir berdasarkan skor dari max 1000.
    public string Grade()
    {
        float pct = score / 1000f;
        if (pct >= 0.8f) return "★ PAHLAWAN SEJATI ★";
        if (pct >= 0.6f) return "Sang Jagoan";
        if (pct >= 0.4f) return "Si Pemberani";
        return "Masih Perlu Belajar";
    }

    /// Reset semua state (untuk mulai ulang dari awal).
    public void Reset()
    {
        lives        = maxLives;
        score        = 0;
        day          = 1;
        pathChoice   = "safe";
        screenshotTaken = false;
        platChecked  = false;
        seatCategory = "";
        checkpointD1 = false;
        checkpointD2 = false;
        checkpointD3 = false;
        choices.Clear();
        achievements.Clear();
        _bukti.Clear();
        dangerLevel = 0f;
        usedTidak   = false;
        usedPergi   = false;
        usedCerita  = false;
        OnDangerChanged?.Invoke(dangerLevel);
    }

    // ── Helper ─────────────────────────────────────────────────────────────
    int CategoryToPoints(string category) => category switch
    {
        "AMAN"  => SCORE_AMAN,
        "RAGU"  => SCORE_RAGU,
        "LAPOR" => SCORE_LAPOR,
        _       => SCORE_BAHAYA
    };
}
