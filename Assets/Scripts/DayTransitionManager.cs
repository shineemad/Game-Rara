using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// DayTransitionManager — Manager transisi antar hari dalam SATU scene (single-scene mode).
///
/// Pendekatan: alih-alih load scene baru, manager ini mematikan GameObject Day 1
/// (player, Day1Controller, NPC, latar belakang side-scroller) dan menghidupkan
/// GameObject Day 2 (Day2Controller + 7 komponen fase). Day2Controller punya
/// IEnumerator Start() + autoStart=true, jadi begitu di-SetActive(true) ia langsung
/// jalan dari fase Intro.
///
/// Cara pakai:
///   1. GameObject → Create Empty → "DayTransitionManager" → Add Component
///   2. Drag semua GameObject milik Day 1 ke "day1Objects" (Player, Day1Controller,
///      kamera follower, NPC, dialog canvas, dll).
///   3. Drag semua GameObject milik Day 2 ke "day2Objects" (Day2Controller dan
///      7 fase: HalteDialog, AngkotSeatPicker, ZonaTubuhQuiz, ChatSimWhatsApp,
///      LaporTeriakButton, EduCardDay2, Day2SummaryScreen).
///   4. Set semua Day 2 objects DISABLE di Hierarchy (checkbox dimatikan).
///   5. Day1SummaryScreen otomatis cari manager ini lewat Instance — tidak perlu
///      wiring UnityEvent kalau hanya butuh perilaku default.
/// </summary>
public class DayTransitionManager : MonoBehaviour
{
    public static DayTransitionManager Instance { get; private set; }

    [Header("Mode")]
    [Tooltip("Single-scene: matikan Day 1, hidupkan Day 2 (rekomendasi).")]
    public bool singleSceneMode = true;
    [Tooltip("Kalau false (multi-scene), gunakan SceneLoader untuk pindah scene.")]
    public string day2SceneName = "Day2";
    public string day3SceneName = "Day3";

    [Header("Day 1 Objects (disable saat LANJUT ke Day 2)")]
    [Tooltip("Player, Day1Controller, kamera, NPC, latar side-scroll, dll.")]
    public GameObject[] day1Objects;

    [Header("Day 2 Objects (enable saat LANJUT ke Day 2)")]
    [Tooltip("Day2Controller + semua 7 fase. Set semua DISABLE awalnya.")]
    public GameObject[] day2Objects;

    [Header("Day 3 Objects (enable saat LANJUT ke Day 3, opsional)")]
    public GameObject[] day3Objects;

    [Header("ULANGI Behavior")]
    [Tooltip("Saat ULANGI HARI 1: reload scene aktif (paling aman).")]
    public bool ulangiReloadScene = true;
    [Tooltip("Saat reload, juga reset achievements list di GameState.")]
    public bool resetAchievementsSaatUlangi = true;

    [Header("Prolog Day 2 (opsional)")]
    [Tooltip("Kalau true, tampilkan Day2PrologScreen sebelum mulai Day 2.")]
    public bool tampilkanPrologDay2 = true;
    [Tooltip("Referensi langsung Day2PrologScreen. Kosongkan = auto-find via Singleton.")]
    public Day2PrologScreen day2Prolog;

    [Header("Debug")]
    public bool debugLog = true;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ══════════════════════════════════════════════════════════════════════
    // PUBLIC API — dipanggil dari Day1SummaryScreen / tombol UI
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Lanjut dari Day 1 ke Day 2.</summary>
    public void LanjutKeDay2()
    {
        if (debugLog) Debug.Log("[DayTransitionManager] LanjutKeDay2()");

        // Logika transisi hari (set GameState.day=2 + refresh navbar + animasi
        // highlight H2) DI-CENTRALIZE di HUDManager.OnLanjutHari2().
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.OnLanjutHari2();
        }
        else
        {
            // Fallback bila HUDManager belum ada (seharusnya tidak terjadi
            // karena auto-spawn BeforeSceneLoad).
            var gs = GameState.Instance;
            if (gs != null) gs.day = 2;
        }

        if (singleSceneMode)
        {
            SetActiveAll(day1Objects, false);
            SetActiveAll(day3Objects, false);

            // Tampilkan prolog DULU sebelum enable Day2_Root + jalankan Day2Controller
            var prolog = day2Prolog;
            if (prolog == null) prolog = Day2PrologScreen.Instance;
            if (tampilkanPrologDay2 && prolog != null)
            {
                if (debugLog) Debug.Log("[DayTransitionManager] Tampilkan Day 2 prolog dulu...");
                // Pastikan GO prolog aktif (kalau ia child dari day2_root yang disable, perlu di-aktifkan dulu)
                if (!prolog.gameObject.activeInHierarchy) prolog.gameObject.SetActive(true);
                prolog.Tampilkan(() => MulaiDay2Sesungguhnya());
                return;
            }

            // Tidak ada prolog → langsung jalan Day 2
            MulaiDay2Sesungguhnya();
        }
        else
        {
            LoadSceneByName(day2SceneName);
        }
    }

    /// <summary>Tahap aktivasi Day 2 sebenarnya — dipanggil setelah prolog selesai (atau langsung kalau tidak ada prolog).</summary>
    void MulaiDay2Sesungguhnya()
    {
        SetActiveAll(day2Objects, true);

        // BGM Day 2
        if (AudioManager.Instance != null)
        {
            try { AudioManager.Instance.PlayBGM(AudioManager.BGMTrack.Day2); }
            catch { /* ignore */ }
        }

        // Trigger Day2Controller (idempotent)
        if (Day2Controller.Instance != null)
            Day2Controller.Instance.TriggerStart();
    }

    /// <summary>Lanjut dari Day 2 ke Day 3.</summary>
    public void LanjutKeDay3()
    {
        if (debugLog) Debug.Log("[DayTransitionManager] LanjutKeDay3()");

        // Logika transisi hari (set GameState.day=3 + refresh navbar + animasi
        // highlight H3) DI-CENTRALIZE di HUDManager.OnLanjutHari3().
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.OnLanjutHari3();
        }
        else
        {
            var gs = GameState.Instance;
            if (gs != null) gs.day = 3;
        }

        if (singleSceneMode)
        {
            SetActiveAll(day1Objects, false);
            SetActiveAll(day2Objects, false);
            SetActiveAll(day3Objects, true);

            if (AudioManager.Instance != null)
            {
                try { AudioManager.Instance.PlayBGM(AudioManager.BGMTrack.Day3); }
                catch { /* ignore */ }
            }

            // Trigger Day3Controller secara eksplisit (idempotent) — sama seperti Day 2.
            if (Day3Controller.Instance != null)
                Day3Controller.Instance.TriggerStart();
        }
        else
        {
            LoadSceneByName(day3SceneName);
        }
    }

    /// <summary>Ulangi Hari 1 (reset state + reload scene).</summary>
    public void UlangiHari1()
    {
        if (debugLog) Debug.Log("[DayTransitionManager] UlangiHari1()");

        ResetGameStateUntukHari(1);
        if (ulangiReloadScene) ReloadActiveScene();
        else SwitchKeDay(1);
    }

    /// <summary>Ulangi Hari 2 (reset score & nyawa, lalu balik ke awal Day 2).</summary>
    public void UlangiHari2()
    {
        if (debugLog) Debug.Log("[DayTransitionManager] UlangiHari2()");

        ResetGameStateUntukHari(2);
        if (ulangiReloadScene)
        {
            // Setelah reload, kita masih perlu skip Day 1 dan langsung ke Day 2.
            // Simpan flag supaya bisa dibaca setelah reload.
            DayTransitionResumeFlag.SkipKeDay = 2;
            ReloadActiveScene();
        }
        else SwitchKeDay(2);
    }

    // ══════════════════════════════════════════════════════════════════════
    // INTERNAL HELPERS
    // ══════════════════════════════════════════════════════════════════════

    void SetActiveAll(GameObject[] arr, bool active)
    {
        if (arr == null) return;
        // Safety net: jangan pernah disable Main Camera walaupun terlanjur masuk array.
        // Tanpa kamera scene tampil "No cameras rendering" dan Day 2 jadi blank.
        GameObject mainCamRoot = null;
        if (!active && Camera.main != null)
        {
            var t = Camera.main.transform;
            while (t.parent != null) t = t.parent;
            mainCamRoot = t.gameObject;
        }

        foreach (var go in arr)
        {
            if (go == null) continue;
            if (!active && go == mainCamRoot)
            {
                if (debugLog) Debug.Log("[DayTransitionManager] Main Camera dilewati (tidak di-disable).");
                continue;
            }
            go.SetActive(active);
        }
    }

    void SwitchKeDay(int day)
    {
        switch (day)
        {
            case 1:
                SetActiveAll(day2Objects, false);
                SetActiveAll(day3Objects, false);
                SetActiveAll(day1Objects, true);
                break;
            case 2: LanjutKeDay2(); break;
            case 3: LanjutKeDay3(); break;
        }
    }

    void ResetGameStateUntukHari(int day)
    {
        var gs = GameState.Instance;
        if (gs == null) return;

        gs.score   = 0;
        gs.lives   = gs.maxLives;
        gs.day     = day;
        if (gs.choices != null) gs.choices.Clear();
        if (resetAchievementsSaatUlangi && gs.achievements != null)
            gs.achievements.Clear();
        gs.platChecked      = false;
        gs.screenshotTaken  = false;
        gs.pathChoice       = null;
    }

    void ReloadActiveScene()
    {
        var active = SceneManager.GetActiveScene().name;
        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene(active);
        else SceneManager.LoadScene(active);
    }

    void LoadSceneByName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("[DayTransitionManager] Scene name kosong.");
            return;
        }
        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene(name);
        else SceneManager.LoadScene(name);
    }

    // ── Setelah scene reload, cek flag SkipKeDay supaya ULANGI Day 2 langsung
    //    masuk ke Day 2 lagi tanpa harus jalanin Day 1 lagi.
    void Start()
    {
        if (DayTransitionResumeFlag.SkipKeDay > 0)
        {
            int day = DayTransitionResumeFlag.SkipKeDay;
            DayTransitionResumeFlag.SkipKeDay = 0;
            SwitchKeDay(day);
        }
    }
}

/// <summary>
/// Penyimpan flag statis untuk handle reload scene + lanjut ke Day tertentu.
/// Static field bertahan antar scene karena class-level static.
/// </summary>
public static class DayTransitionResumeFlag
{
    public static int SkipKeDay = 0;
}
