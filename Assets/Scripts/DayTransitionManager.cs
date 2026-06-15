using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [Header("Prolog Day 3 (opsional)")]
    [Tooltip("Kalau true, tampilkan Day3PrologScreen sebelum mulai Day 3.")]
    public bool tampilkanPrologDay3 = true;
    [Tooltip("Referensi langsung Day3PrologScreen. Kosongkan = auto-find via Singleton.")]
    public Day3PrologScreen day3Prolog;

    [Header("Transisi Fade Antar Hari")]
    [Tooltip("Kalau true, layar fade ke hitam saat berpindah hari (menutupi swap GameObject yang mendadak).")]
    public bool gunakanFade = true;
    [Tooltip("Durasi fade in/out (detik).")]
    [Range(0.1f, 1.5f)]
    public float fadeDurasi = 0.45f;
    [Tooltip("Warna layar saat transisi (default hitam).")]
    public Color fadeWarna = Color.black;

    // Overlay fade prosedural (dibuat sekali, dipakai ulang).
    private CanvasGroup _fadeGroup;
    private Image       _fadeImg;

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
        StartCoroutine(TransisiKeDay2());
    }

    IEnumerator TransisiKeDay2()
    {
        // Fade ke hitam dulu supaya swap GameObject Day1→Day2 tidak terlihat mendadak.
        yield return Fade(true);

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
                // Pastikan GO prolog AKTIF di hierarki. Kalau ia child dari
                // Day2_Root yang masih disable, SetActive(true) saja tidak cukup
                // (activeInHierarchy tetap false). Lepaskan ke root scene dulu.
                if (!prolog.gameObject.activeInHierarchy)
                {
                    if (prolog.transform.parent != null) prolog.transform.SetParent(null, true);
                    prolog.gameObject.SetActive(true);
                }
                prolog.Tampilkan(() => MulaiDay2Sesungguhnya());
                // Layar prolog sudah menutupi konten → aman fade in (mengungkap prolog).
                yield return Fade(false);
                yield break;
            }

            // Tidak ada prolog → langsung jalan Day 2
            MulaiDay2Sesungguhnya();
            yield return Fade(false);
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

        // Pastikan GameObject Day2Controller (dan SELURUH rantai parent-nya) aktif.
        // Day2Controller ada di bawah Day2_Root, tetapi salah satu leluhurnya
        // (mis. Day2Preset) bisa ikut ter-disable saat SetActiveAll(day1Objects, false).
        // Kalau begitu, mengaktifkan Day2_Root saja tidak cukup — activeInHierarchy
        // tetap false dan StartCoroutine di TriggerStart() gagal
        // ("game object 'Day2_NarasiScene' is inactive").
        if (Day2Controller.Instance != null)
            PastikanAktifHinggaAkar(Day2Controller.Instance.gameObject);

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

    /// <summary>
    /// Aktifkan GameObject beserta seluruh rantai parent-nya sampai akar scene,
    /// supaya activeInHierarchy menjadi true (syarat StartCoroutine bisa jalan).
    /// </summary>
    void PastikanAktifHinggaAkar(GameObject go)
    {
        if (go == null) return;
        // Set activeSelf=true di setiap level. Urutan tidak masalah: setelah semua
        // node ber-activeSelf true, activeInHierarchy otomatis kaskade dari akar.
        for (Transform t = go.transform; t != null; t = t.parent)
        {
            if (!t.gameObject.activeSelf)
            {
                t.gameObject.SetActive(true);
                if (debugLog) Debug.Log($"[DayTransitionManager] Aktifkan leluhur Day2: {t.gameObject.name}");
            }
        }
    }

    /// <summary>Lanjut dari Day 2 ke Day 3.</summary>
    public void LanjutKeDay3()
    {
        if (debugLog) Debug.Log("[DayTransitionManager] LanjutKeDay3()");
        StartCoroutine(TransisiKeDay3());
    }

    IEnumerator TransisiKeDay3()
    {
        // Fade ke hitam dulu supaya swap GameObject Day2→Day3 tidak terlihat mendadak.
        yield return Fade(true);

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

            // Tampilkan prolog DULU sebelum jalankan boss fight Day3Controller.
            var prolog = day3Prolog;
            if (prolog == null) prolog = Day3PrologScreen.Instance;
            if (tampilkanPrologDay3 && prolog != null)
            {
                if (debugLog) Debug.Log("[DayTransitionManager] Tampilkan Day 3 prolog dulu...");
                // Pastikan GO prolog AKTIF di hierarki (lepaskan dari parent yang
                // mungkin masih disable), sama seperti Day 2.
                if (!prolog.gameObject.activeInHierarchy)
                {
                    if (prolog.transform.parent != null) prolog.transform.SetParent(null, true);
                    prolog.gameObject.SetActive(true);
                }
                prolog.Tampilkan(() => MulaiDay3Sesungguhnya());
                // Layar prolog sudah menutupi konten → aman fade in (mengungkap prolog).
                yield return Fade(false);
                yield break;
            }

            // Tidak ada prolog → langsung jalan Day 3
            MulaiDay3Sesungguhnya();
            yield return Fade(false);
        }
        else
        {
            LoadSceneByName(day3SceneName);
        }
    }

    /// <summary>Tahap aktivasi Day 3 sebenarnya — dipanggil setelah prolog selesai (atau langsung kalau tidak ada prolog).</summary>
    void MulaiDay3Sesungguhnya()
    {
        if (AudioManager.Instance != null)
        {
            try { AudioManager.Instance.PlayBGM(AudioManager.BGMTrack.Day3); }
            catch { /* ignore */ }
        }

        // Trigger Day3Controller secara eksplisit (idempotent) — sama seperti Day 2.
        if (Day3Controller.Instance != null)
            Day3Controller.Instance.TriggerStart();
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

    /// <summary>
    /// Fade layar ke hitam (keHitam=true) atau dari hitam ke transparan
    /// (keHitam=false). Overlay dibuat prosedural sekali dan dipakai ulang.
    /// </summary>
    IEnumerator Fade(bool keHitam)
    {
        if (!gunakanFade) yield break;
        PastikanFadeOverlay();
        if (_fadeGroup == null) yield break;

        _fadeGroup.blocksRaycasts = true;   // cegah klik selama transisi
        float dari = _fadeGroup.alpha;
        float tujuan = keHitam ? 1f : 0f;
        float durasi = Mathf.Max(0.01f, fadeDurasi);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / durasi;
            _fadeGroup.alpha = Mathf.Lerp(dari, tujuan, Mathf.Clamp01(t));
            yield return null;
        }
        _fadeGroup.alpha = tujuan;
        // Saat sudah transparan penuh, jangan blok klik konten di bawahnya.
        _fadeGroup.blocksRaycasts = keHitam;
    }

    /// <summary>Buat overlay fade fullscreen (Canvas + Image hitam) sekali saja.</summary>
    void PastikanFadeOverlay()
    {
        if (_fadeGroup != null) return;

        var go = new GameObject("[DayTransitionFade]");
        go.transform.SetParent(transform, false);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 31000;        // di atas HUD, di bawah layar hasil (32000)
        go.AddComponent<GraphicRaycaster>();
        _fadeGroup = go.AddComponent<CanvasGroup>();
        _fadeGroup.alpha = 0f;
        _fadeGroup.blocksRaycasts = false;

        var imgGO = new GameObject("FadeImg");
        imgGO.transform.SetParent(go.transform, false);
        _fadeImg = imgGO.AddComponent<Image>();
        _fadeImg.color = fadeWarna;
        var rt = _fadeImg.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

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
