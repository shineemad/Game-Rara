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
        // Update HUD langsung agar skor tampil segera
        HUDManager.Instance?.UpdateScore(score);
    }

    /// Tambah skor langsung.
    public void AddScore(int pts)
    {
        score += pts;
        HUDManager.Instance?.UpdateScore(score);
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
