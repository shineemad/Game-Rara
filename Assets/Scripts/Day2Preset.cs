using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Day2Preset — Bootstrap otomatis untuk seluruh sistem transisi Day 1 → Day 2.
///
/// PRESET SIAP PAKAI: cukup tambah satu GameObject kosong dengan komponen ini
/// ke scene Gameplay, dan SEMUA setup berikut dilakukan otomatis di Awake:
///   1. Buat DayTransitionManager (kalau belum ada)
///   2. Buat GameObject Day2_Root + 8 anak fase Day 2:
///        Day2Controller, HalteDialog, AngkotSeatPicker, ZonaTubuhQuiz,
///        ChatSimWhatsApp, LaporTeriakButton, EduCardDay2, Day2SummaryScreen
///   3. Set Day2_Root SetActive(false) sehingga tidak ganggu Day 1
///   4. Auto-discover Day 1 GameObject (berdasarkan komponen-komponen Day 1)
///   5. Isi day1Objects[] dan day2Objects[] di DayTransitionManager
///
/// ALTERNATIF: pakai metode statis Day2Preset.Bootstrap() langsung dari script lain
/// (mis. dipanggil saat MainMenu → Play untuk siapkan scene tanpa GameObject).
///
/// Cara pakai TERMUDAH:
///   1. Di scene Gameplay → Create Empty → "[Day2Preset]"
///   2. Add Component → Day2Preset
///   3. (Opsional) centang autoRunSaatAwake = true (default)
///   4. Play → semuanya jalan otomatis.
/// </summary>
public class Day2Preset : MonoBehaviour
{
    [Header("Auto-Run")]
    [Tooltip("Bootstrap otomatis saat Awake. Matikan kalau mau panggil manual via Bootstrap().")]
    public bool autoRunSaatAwake = true;

    [Header("Day 1 — Komponen yang dianggap milik Day 1")]
    [Tooltip("Otomatis cari semua GO dengan komponen-komponen ini dan tambahkan ke day1Objects[].")]
    public bool autoDiscoverDay1 = true;

    [Header("Day 2 — Buat otomatis kalau belum ada")]
    [Tooltip("Buat Day2_Root + 8 anak fase Day 2 lewat AddComponent secara procedural.")]
    public bool autoBuildDay2 = true;

    [Tooltip("Nama GameObject root yang akan menampung semua komponen Day 2.")]
    public string day2RootName = "Day2_Root";

    [Header("Debug")]
    public bool debugLog = true;

    void Awake()
    {
        if (autoRunSaatAwake) Bootstrap(this);
    }

    // ══════════════════════════════════════════════════════════════════════
    // STATIC ENTRY POINT
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Bootstrap semua sistem transisi Day 1 → Day 2. Idempotent — aman dipanggil
    /// berkali-kali. Bisa dipanggil tanpa instance Day2Preset di scene.
    /// </summary>
    public static void Bootstrap(Day2Preset config = null)
    {
        bool log = config == null || config.debugLog;

        // 1. Pastikan DayTransitionManager ada
        var mgr = DayTransitionManager.Instance;
        if (mgr == null)
        {
            var go = new GameObject("[DayTransitionManager]");
            mgr = go.AddComponent<DayTransitionManager>();
            mgr.singleSceneMode = true;
            mgr.ulangiReloadScene = true;
            mgr.resetAchievementsSaatUlangi = true;
            mgr.tampilkanPrologDay2 = true;
            if (log) Debug.Log("[Day2Preset] DayTransitionManager dibuat otomatis.");
        }

        // 2. Pastikan Day2PrologScreen ada sebagai GameObject TERPISAH (sibling Day2_Root)
        //    supaya tetap aktif walau Day2_Root disable.
        //
        // PENTING: pakai FindFirstObjectByType (bukan Day2PrologScreen.Instance)
        // karena Instance baru ter-set setelah Day2PrologScreen.Awake() jalan.
        // Kalau Day2Preset.Awake() race lebih dulu \u2192 Instance == null \u2192 spawn
        // GameObject BARU dengan nilai DEFAULT dari kode \u2192 GameObject yang sudah
        // diedit user di Inspector lalu Awake \u2192 lihat Instance != this \u2192 self-destroy.
        // Hasilnya: SEMUA edit user di Inspector hilang.
        var prolog = Object.FindFirstObjectByType<Day2PrologScreen>(FindObjectsInactive.Include);
        if (prolog == null)
        {
            var prologGO = new GameObject("[Day2PrologScreen]");
            prolog = prologGO.AddComponent<Day2PrologScreen>();
            if (log) Debug.Log("[Day2Preset] Day2PrologScreen dibuat otomatis.");
        }
        else
        {
            if (log) Debug.Log($"[Day2Preset] Pakai Day2PrologScreen yang sudah ada: {prolog.gameObject.name} (menghormati nilai Inspector).");
        }
        mgr.day2Prolog = prolog;

        // 3. Bangun Day 2 root + komponen kalau belum ada
        GameObject day2Root = null;
        if (config == null || config.autoBuildDay2)
        {
            day2Root = BangunDay2Root(config == null ? "Day2_Root" : config.day2RootName, log);
        }

        // 4. Auto-discover Day 1 objects
        List<GameObject> day1List = new List<GameObject>();
        if (config == null || config.autoDiscoverDay1)
        {
            day1List = DiscoverDay1Objects(log);
        }

        // 5. Inject ke DayTransitionManager (hanya kalau array masih kosong supaya
        //    setup manual user tidak ditimpa)
        if ((mgr.day1Objects == null || mgr.day1Objects.Length == 0) && day1List.Count > 0)
        {
            mgr.day1Objects = day1List.ToArray();
            if (log) Debug.Log($"[Day2Preset] day1Objects[] terisi {day1List.Count} GameObject otomatis.");
        }

        if ((mgr.day2Objects == null || mgr.day2Objects.Length == 0) && day2Root != null)
        {
            mgr.day2Objects = new GameObject[] { day2Root };
            if (log) Debug.Log("[Day2Preset] day2Objects[] terisi dengan Day2_Root.");
        }

        // 6. Pastikan Day 2 root dimatikan di awal supaya tidak hijack Day 1
        if (day2Root != null && day2Root.activeSelf)
        {
            day2Root.SetActive(false);
            if (log) Debug.Log("[Day2Preset] Day2_Root.SetActive(false) — menunggu LANJUT HARI 2.");
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // BANGUN DAY 2 ROOT + 8 ANAK
    // ══════════════════════════════════════════════════════════════════════

    static GameObject BangunDay2Root(string rootName, bool log)
    {
        // Cari root yang sudah ada
        var existingRoot = GameObject.Find(rootName);
        if (existingRoot != null)
        {
            // Pastikan semua 8 komponen ada sebagai child
            EnsureChildKomponenDay2(existingRoot, log);
            return existingRoot;
        }

        // Cari berdasarkan keberadaan Day2Controller — kalau ada, pakai parent-nya
        var existingCtrl = Object.FindFirstObjectByType<Day2Controller>(FindObjectsInactive.Include);
        if (existingCtrl != null)
        {
            Transform parent = existingCtrl.transform.parent;
            if (parent != null)
            {
                EnsureChildKomponenDay2(parent.gameObject, log);
                return parent.gameObject;
            }
        }

        // Buat dari nol
        var root = new GameObject(rootName);
        if (log) Debug.Log($"[Day2Preset] Root '{rootName}' dibuat.");
        EnsureChildKomponenDay2(root, log);
        return root;
    }

    static void EnsureChildKomponenDay2(GameObject root, bool log)
    {
        // Mapping nama → tipe komponen Day 2 yang wajib ada
        EnsureChildWithComponent<Day2Controller>     (root, "Day2Controller",      log);
        EnsureChildWithComponent<HalteDialog>        (root, "HalteDialog",         log);
        EnsureChildWithComponent<AngkotSeatPicker>   (root, "AngkotSeatPicker",    log);
        EnsureChildWithComponent<ZonaTubuhQuiz>      (root, "ZonaTubuhQuiz",       log);
        EnsureChildWithComponent<ChatSimWhatsApp>    (root, "ChatSimWhatsApp",     log);
        EnsureChildWithComponent<LaporTeriakButton>  (root, "LaporTeriakButton",   log);
        EnsureChildWithComponent<EduCardDay2>        (root, "EduCardDay2",         log);
        EnsureChildWithComponent<Day2SummaryScreen>  (root, "Day2SummaryScreen",   log);
    }

    static void EnsureChildWithComponent<T>(GameObject root, string childName, bool log) where T : Component
    {
        // Cek apakah komponen T sudah ada di mana pun (sebagai child root atau bukan)
        var existing = Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
        if (existing != null)
        {
            // Kalau bukan child dari root, pindahkan
            if (existing.transform.parent != root.transform)
            {
                existing.transform.SetParent(root.transform, false);
                if (log) Debug.Log($"[Day2Preset] {typeof(T).Name} dipindahkan ke {root.name}.");
            }
            return;
        }

        // Buat baru sebagai child
        var go = new GameObject(childName);
        go.transform.SetParent(root.transform, false);
        go.AddComponent<T>();
        if (log) Debug.Log($"[Day2Preset] {typeof(T).Name} dibuat sebagai child {root.name}.");
    }

    // ══════════════════════════════════════════════════════════════════════
    // AUTO-DISCOVER DAY 1
    // ══════════════════════════════════════════════════════════════════════

    static List<GameObject> DiscoverDay1Objects(bool log)
    {
        var result = new HashSet<GameObject>();

        // Komponen-komponen yang jelas-jelas milik Day 1
        AddTopLevelOf<Day1Controller>     (result);
        AddTopLevelOf<player>             (result);
        // NOTE: CameraFollow SENGAJA tidak di-include, supaya Main Camera tetap aktif
        // saat Day 2. Kalau di-disable, scene jadi tanpa kamera ("No cameras rendering").
        // AddTopLevelOf<CameraFollow>       (result);
        AddTopLevelOf<ParallaxBackground> (result);
        AddTopLevelOf<BackgroundController>(result);
        AddTopLevelOf<BackgroundLayerSetup>(result);
        AddTopLevelOf<PathChoiceUI>       (result);
        AddTopLevelOf<PathEnvironment>    (result);
        AddTopLevelOf<NpcDialog>          (result);
        AddTopLevelOf<NpcGang>            (result);
        AddTopLevelOf<NpcLatarPatroli>    (result);
        AddTopLevelOf<PamanBaik>          (result);
        AddTopLevelOf<PemotorMovement>    (result);
        AddTopLevelOf<SampaiSekolahPopup> (result);
        AddTopLevelOf<EduCardDay1>        (result);
        AddTopLevelOf<Day1SummaryScreen>  (result);
        AddTopLevelOf<Day1Intro>          (result);
        AddTopLevelOf<PrologScreen>       (result);
        AddTopLevelOf<ZonaWarning>        (result);

        // SAFETY NET: kalau ternyata Main Camera ke-grab via parent chain
        // (mis. user pernah parent-kan Player ke Main Camera), kita keluarkan dari list.
        var mainCam = Camera.main;
        if (mainCam != null)
        {
            var camRoot = mainCam.transform;
            while (camRoot.parent != null) camRoot = camRoot.parent;
            if (result.Remove(camRoot.gameObject) && log)
                Debug.Log("[Day2Preset] Main Camera dikeluarkan dari day1Objects (jangan di-disable).");
        }

        var arr = new List<GameObject>(result);
        if (log) Debug.Log($"[Day2Preset] Day 1 auto-discover menemukan {arr.Count} GameObject root.");
        return arr;
    }

    static void AddTopLevelOf<T>(HashSet<GameObject> set) where T : Component
    {
        var comps = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in comps)
        {
            if (c == null) continue;
            // Ambil root teratas — supaya kalau Player adalah child dari "Day1_Root", kita ambil root-nya
            Transform t = c.transform;
            while (t.parent != null) t = t.parent;
            if (t != null && t.gameObject != null) set.Add(t.gameObject);
        }
    }
}
