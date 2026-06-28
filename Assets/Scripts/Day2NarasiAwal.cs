using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Day2NarasiAwal — Fase narasi pembuka Day 2 (setelah overlay judul, sebelum Halte).
///
/// LOGIKA SAMA DENGAN PathEnvironment (jalan ramai / gang sepi):
///   1. Drag GameObject 'BG day 2' dari Hierarchy ke field 'bgDay2'.
///   2. Saat narasi mulai → SetActive(true). Camera snap ke posisi tersimpan.
///   3. Player klik 'Ketuk Lanjut' → narasi habis → SetActive(false). Camera restore.
///   4. Lanjut ke Halte (Day2Controller).
///
/// CARA PAKAI (3 langkah):
///   A. Klik kanan komponen → '▶ Buat BG day 2 di Hierarchy' (auto-build)
///   B. (Opsional) Customize di Scene View — drag Rara, ubah scale BG, tambah dekorasi
///   C. Klik kanan komponen → '📷 Capture Camera Saat Ini' (Scene View harus 2D)
/// </summary>
public class Day2NarasiAwal : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR
    // ══════════════════════════════════════════════════════════════════════

    [Header("── BG DAY 2 (drag dari Hierarchy) ──")]
    [Tooltip("GameObject 'BG day 2' di Hierarchy. SetActive awal = FALSE.\n" +
             "Klik kanan komponen → '▶ Buat BG day 2 di Hierarchy' kalau belum ada.")]
    public GameObject bgDay2;

    [Tooltip("Props/NPC tambahan yang muncul bersama BG day 2 (opsional).")]
    public GameObject[] objekTambahan;

    [Header("── KAMERA ──")]
    [Tooltip("Main camera scene. Auto = Camera.main.")]
    public Camera mainCamera;
    [Tooltip("Warna camera background saat narasi (cocok dengan langit BG day 2).")]
    public Color  cameraBgWarna = new Color(0.52f, 0.78f, 0.95f, 1f); // sky blue

    public enum KameraMode
    {
        AutoFitCover,    // sprite PENUHI layar (mungkin crop sedikit di sisi terpanjang). DEFAULT.
        AutoFitContain,  // sprite muat seluruhnya (bisa ada bar di atas/bawah/samping)
        Manual           // pakai nilai Inspector apa adanya — atur sendiri di Scene View
    }

    [Tooltip("Cara fit kamera ke BG day 2:\n" +
             "• AutoFitCover  → sprite penuhi layar (DEFAULT, no blue bar)\n" +
             "• AutoFitContain → sprite muat seluruhnya (bisa ada bar)\n" +
             "• Manual         → pakai pos/ortho Inspector apa adanya")]
    public KameraMode kameraMode = KameraMode.AutoFitCover;

    [Tooltip("Posisi kamera saat narasi. Diisi otomatis di Start kalau mode AutoFit*.\n" +
             "Mode Manual → atur sendiri (klik kanan → '📷 Capture Camera dari Scene View').")]
    public Vector3 cameraPosNarasi = new Vector3(40f, 14f, -10f);
    [Tooltip("Orthographic size kamera saat narasi.")]
    public float   cameraOrthoNarasi = 5.4f;

    [Tooltip("Tambahan zoom faktor (1.0 = pas, 0.9 = zoom-in 10%, 1.1 = zoom-out 10%).\n" +
             "Berguna mode AutoFitCover supaya sedikit padding.")]
    [Range(0.5f, 1.5f)]
    public float   kameraZoomFaktor = 1.0f;

    [Tooltip("Nonaktifkan CameraFollow selama narasi (supaya tidak ikut player).")]
    public bool    nonaktifkanCameraFollow = true;

    // ══════════════════════════════════════════════════════════════════════
    // NARASI (per baris — ikut format BarisNarasi di Day1Intro)
    // ══════════════════════════════════════════════════════════════════════
    [System.Serializable]
    public class BarisNarasi
    {
        [Tooltip("Nama pembicara yang tampil di banner (mis. 'Rara', 'Narasi')")]
        public string pembicara = "Rara";

        [TextArea(2, 6)]
        [Tooltip("Isi teks dialog")]
        public string teks = "";

        [Tooltip("Sprite portrait/foto pembicara. Kosong = pakai portrait default.")]
        public Sprite portrait;

        [Tooltip("Sprite latar FULLSCREEN device untuk baris ini (opsional).\n" +
                 "Kalau diisi → background fullscreen langsung ganti ke sprite ini saat baris tampil.\n" +
                 "Kalau kosong → tetap pakai sprite baris sebelumnya / default.")]
        public Sprite latarFullscreen;
    }

    [Header("── NARASI ──")]
    [Tooltip("Setiap entry = satu halaman dialog. Klik / SPACE untuk lanjut.")]
    public BarisNarasi[] narasi = new BarisNarasi[]
    {
        new BarisNarasi { pembicara = "Rara",
            teks = "\"Bismillah, aku pasti bisa!\nHaltenya udah dekat \u2014 ayo cepat!\"" },
        new BarisNarasi { pembicara = "Rara",
            teks = "\"Tapi\u2026 kok jalan ini sepi banget ya?\nAku harus tetap waspada.\"" }
    };

    [Tooltip("DEPRECATED — dipakai sebagai fallback kalau 'narasi' kosong. " +
             "Pakai field 'Narasi' di atas (lebih lengkap dengan pembicara + portrait per baris).")]
    [TextArea(2, 5)]
    public List<string> narasiLines = new List<string>();
    [Tooltip("Nama default speaker kalau BarisNarasi.pembicara kosong.")]
    public string speakerName = "Rara";

    // ══════════════════════════════════════════════════════════════════════
    // STYLE BOX DIALOG (sama dengan Day1Intro — bisa share asset)
    // ══════════════════════════════════════════════════════════════════════
    [Header("── BOX DIALOG (mirror Day1Intro) ──")]
    [Tooltip("Aset DialogBoxLayout bersama. Jika di-assign, NILAINYA akan menimpa\n" +
             "panelSprite + semua field anchor di bawah. Drag aset yang sama dari\n" +
             "Day1Intro/NpcDialog → satu sumber kebenaran.")]
    public DialogBoxLayout layout;

    [Tooltip("Sprite background kotak dialog (sliced). Kosongkan = warna solid + outline.\n" +
             "Auto-load dari UI day 1/8.png saat Reset.")]
    public Sprite panelSprite;
    [Tooltip("Path sprite panel (relatif Assets/) untuk auto-load.")]
    public string panelSpritePath = "sprites/UI day 1/8.png";

    [Tooltip("Sprite banner nama pembicara (lencana kayu). Kosong = warna solid.")]
    public Sprite nameBannerSprite;
    [Tooltip("Sprite portrait default Rara.")]
    public Sprite portraitRara;
    [Tooltip("Sprite portrait untuk baris pembicara 'Narasi'. Kosong = pakai portraitRara.")]
    public Sprite portraitNarasi;
    [Tooltip("Sprite portrait umum (legacy — fallback kalau Rara/Narasi kosong).")]
    public Sprite portraitSprite;

    [Header("Posisi & Ukuran Panel (fraksi layar 0–1)")]
    [Range(0f, 1f)]      public float panelCenterX    = 0.50f;
    [Range(0f, 1f)]      public float panelCenterY    = 0.215f;
    [Range(0.1f, 1f)]    public float panelWidthFrac  = 0.96f;
    [Range(0.02f, 0.5f)] public float panelHeightFrac = 0.395f;

    [Header("Portrait (anchor fraksi panel 0–1)")]
    [Range(0f, 1f)]      public float portraitCenterX = 0.14f;
    [Range(0f, 1f)]      public float portraitCenterY = 0.584f;
    [Range(0.02f, 0.6f)] public float portraitSizeW   = 0.189f;
    [Range(0.02f, 1f)]   public float portraitSizeH   = 0.56f;
    public bool          portraitPreserveAspect       = false;

    [Header("Banner Nama (anchor 0–1)")]
    public Vector2 bannerAnchorMin = new Vector2(0.11f, 0.11f);
    public Vector2 bannerAnchorMax = new Vector2(0.253f, 0.333f);

    [Header("Area Teks (anchor 0–1) - mirror Day1Intro")]
    public Vector2 textAnchorMin = new Vector2(0.31f, 0.55f);
    public Vector2 textAnchorMax = new Vector2(0.84f, 0.76f);

    [Header("Petunjuk Lanjut (anchor 0–1)")]
    [Range(0f, 1f)]      public float hintCenterX = 0.798f;
    [Range(0f, 1f)]      public float hintCenterY = 0.242f;
    [Range(0.05f, 1f)]   public float hintSizeW   = 0.296f;
    [Range(0.02f, 0.5f)] public float hintSizeH   = 0.12f;

    [Header("Warna Box (kalau panelSprite kosong)")]
    public Color warnaPanel       = new Color(0f, 0f, 0f, 0f);
    public Color warnaOutline     = new Color(1f, 0.85f, 0.3f, 1f);
    public Color warnaBanner      = new Color(0.14f, 0.09f, 0.01f, 0f);
    public Color warnaNamaSpeaker = new Color(1f, 0.85f, 0.3f, 1f);
    public Color warnaTeks        = new Color(1f, 0.96f, 0.88f, 1f);
    public Color warnaHintLanjut  = new Color(1f, 1f, 1f, 0.55f);
    public Color portraitFallbackWarna = new Color(0.85f, 0.55f, 0.75f, 1f);

    [Header("Font & Ukuran")]
    public TMP_FontAsset fontAsset;
    public int ukuranNama = 30;
    public int ukuranTeks = 30;
    public int ukuranHint = 18;

    [Header("── TYPEWRITER ──")]
    public float  kecepatanKetik    = 0.025f;
    public float  delaySetelahKetik = 0.15f;
    public string teksHintLanjut    = "";

    [Header("── SPRITE PATH (untuk auto-build) ──")]
    [Tooltip("Path BG day 2 (relatif Assets/) untuk tombol klik kanan auto-build.")]
    public string bgSpritePath = "sprites/UI day 2/BG day 2.png";

    // ══════════════════════════════════════════════════════════════════════
    // RUNTIME
    // ══════════════════════════════════════════════════════════════════════
    private Action          _onSelesai;
    private GameObject      _canvasGO;
    private TextMeshProUGUI _teksDialog;
    private TextMeshProUGUI _teksHint;
    private TombolLanjutVN  _tombolLanjut;
    private TextMeshProUGUI _namaTMP;
    private Image           _portImg;
    private Image           _bgFullscreenImg;
    private bool            _ketikSelesai;
    private bool            _skipKetik;
    private int             _idxLine;

    // restore state
    private Color           _camBgAsli;
    private Vector3         _camPosAsli;
    private float           _camOrthoAsli;
    private bool            _camTersimpan;
    private Behaviour       _camFollow;
    private bool            _camFollowEnabledAsli;

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        // Sembunyikan BG day 2 + props supaya tidak nongol sebelum giliran
        if (bgDay2 != null) bgDay2.SetActive(false);
        SetGroupActive(objekTambahan, false);
    }

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // ── AUTO-BUILD: kalau bgDay2 belum di-assign, buat otomatis sekarang ──
        if (bgDay2 == null)
        {
            Debug.Log("[Day2NarasiAwal] bgDay2 NULL → auto-build runtime sekarang.");
            BuatBGDay2Runtime();
        }

        // ── AUTO-FIT KAMERA: kalau mode AutoFit*, hitung ulang dari bounds sprite ──
        // Mode Manual → SKIP, pakai nilai Inspector apa adanya.
        if (bgDay2 != null && kameraMode != KameraMode.Manual)
            AutoFitKameraDariBgDay2();
        else if (kameraMode == KameraMode.Manual)
            Debug.Log($"[Day2NarasiAwal] Kamera mode = Manual → pakai Inspector " +
                      $"pos={cameraPosNarasi} ortho={cameraOrthoNarasi}");

        if (bgDay2 == null)
            Debug.LogWarning("[Day2NarasiAwal] Field 'BG Day 2' belum di-assign DAN auto-build gagal. " +
                "Periksa file Assets/" + bgSpritePath);
        else
            Debug.Log($"[Day2NarasiAwal] Start OK. bgDay2='{bgDay2.name}' " +
                      $"camPos={cameraPosNarasi} camOrtho={cameraOrthoNarasi}");
    }

    /// <summary>
    /// Hitung ulang cameraPosNarasi & cameraOrthoNarasi dari bounds sprite bgDay2.
    /// Override nilai Inspector untuk menghindari capture Scene View 3D yang salah.
    /// </summary>
    void AutoFitKameraDariBgDay2()
    {
        if (bgDay2 == null) return;
        var sr = bgDay2.GetComponent<SpriteRenderer>();
        if (sr == null) sr = bgDay2.GetComponentInChildren<SpriteRenderer>(true);
        if (sr == null || sr.sprite == null)
        {
            Debug.LogWarning("[Day2NarasiAwal] bgDay2 tidak punya SpriteRenderer/sprite. " +
                "Skip auto-fit kamera.");
            return;
        }

        // Aktifkan sementara supaya bounds valid
        bool wasActive = sr.gameObject.activeSelf;
        if (!wasActive) sr.gameObject.SetActive(true);
        var b = sr.bounds;
        if (!wasActive) sr.gameObject.SetActive(false);

        // Posisi center sprite, Z = -10 standar 2D
        cameraPosNarasi = new Vector3(b.center.x, b.center.y, -10f);

        float aspect    = (Screen.width > 0 && Screen.height > 0)
                          ? (float)Screen.width / Screen.height : 16f / 9f;
        float orthoForH = b.size.y * 0.5f;
        float orthoForW = (b.size.x * 0.5f) / aspect;

        // Cover  = sprite penuhi layar (potong sisi terpanjang) → MIN
        // Contain = sprite muat seluruhnya (ada bar)            → MAX
        float baseOrtho = (kameraMode == KameraMode.AutoFitCover)
                          ? Mathf.Min(orthoForH, orthoForW)
                          : Mathf.Max(orthoForH, orthoForW);
        cameraOrthoNarasi = baseOrtho * Mathf.Max(0.1f, kameraZoomFaktor);

        Debug.Log($"[Day2NarasiAwal] 🎯 Auto-fit ({kameraMode}, zoom={kameraZoomFaktor:F2}): " +
                  $"pos={cameraPosNarasi} ortho={cameraOrthoNarasi:F2} (spriteSize={b.size})");
    }

    /// <summary>
    /// Build BG day 2 runtime kalau bgDay2 field kosong.
    /// Sama logikanya dengan context menu Editor, tapi pakai Resources.Load.
    /// </summary>
    void BuatBGDay2Runtime()
    {
        // Load sprite via Resources atau AssetDatabase (kalau Editor)
        Sprite sp = null;
#if UNITY_EDITOR
        sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/" + bgSpritePath);
#endif
        if (sp == null) sp = Resources.Load<Sprite>("BG day 2");
        if (sp == null) sp = Resources.Load<Sprite>("UI day 2/BG day 2");
        if (sp == null)
        {
            Debug.LogError($"[Day2NarasiAwal] Sprite BG day 2 tidak ketemu " +
                $"(Assets/{bgSpritePath} & Resources). Tidak bisa auto-build.");
            return;
        }

        // Buat GameObject di posisi sama dengan jalur ramai (41.5, 13.66)
        var go = new GameObject("BG day 2 (runtime)");
        go.transform.position = new Vector3(41.5f, 13.66f, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sp;
        sr.sortingOrder = 0;

        // Auto-fit kamera dari bounds sprite (skip kalau Manual)
        if (kameraMode != KameraMode.Manual)
        {
            var b = sr.bounds;
            cameraPosNarasi   = new Vector3(b.center.x, b.center.y, -10f);
            float aspect      = (Screen.width > 0 && Screen.height > 0)
                                ? (float)Screen.width / Screen.height : 16f / 9f;
            float orthoForH   = b.size.y * 0.5f;
            float orthoForW   = (b.size.x * 0.5f) / aspect;
            float baseOrtho   = (kameraMode == KameraMode.AutoFitCover)
                                ? Mathf.Min(orthoForH, orthoForW)
                                : Mathf.Max(orthoForH, orthoForW);
            cameraOrthoNarasi = baseOrtho * Mathf.Max(0.1f, kameraZoomFaktor);
        }

        go.SetActive(false); // Mulai() yang aktifkan
        bgDay2 = go;
        Debug.Log($"[Day2NarasiAwal] ✓ Auto-build BG day 2 di {go.transform.position} " +
                  $"size={sr.bounds.size} → camera pos={cameraPosNarasi} ortho={cameraOrthoNarasi} (mode={kameraMode})");
    }

    static void SetGroupActive(GameObject[] group, bool active)
    {
        if (group == null) return;
        for (int i = 0; i < group.Length; i++)
            if (group[i] != null) group[i].SetActive(active);
    }

    // ══════════════════════════════════════════════════════════════════════
    // PUBLIC API — dipanggil Day2Controller pada Phase.Narasi
    // ══════════════════════════════════════════════════════════════════════
    public void Mulai(Action onSelesai)
    {
        _onSelesai = onSelesai;
        Debug.Log("[Day2NarasiAwal] Mulai()");

        // SAFETY NET: sinkronkan gs.day=2 + Refresh HUD (H1→H2) sebelum narasi mulai.
        // Berguna kalau Narasi dipanggil tanpa lewat Day2Controller / DayTransitionManager.
        var gs = GameState.Instance;
        if (gs != null && gs.day != 2) gs.day = 2;
        if (HUDManager.Instance != null) HUDManager.Instance.Refresh();

        // 1. Aktifkan BG day 2 + props (pola PathEnvironment.AktifkanJalanRamai)
        if (bgDay2 != null)
        {
            bgDay2.SetActive(true);

            // FOOLPROOF: paksa enable SpriteRenderer-nya (kalau di-disable manual)
            var sr = bgDay2.GetComponent<SpriteRenderer>();
            if (sr == null) sr = bgDay2.GetComponentInChildren<SpriteRenderer>(true);
            if (sr != null)
            {
                sr.enabled = true;
                if (sr.sprite == null)
                    Debug.LogError($"[Day2NarasiAwal] ⚠ SpriteRenderer di '{sr.name}' tidak punya sprite!");
                else
                    Debug.Log($"[Day2NarasiAwal] ✓ SR enabled: sprite={sr.sprite.name} sortingOrder={sr.sortingOrder}");
            }
            else
            {
                Debug.LogError($"[Day2NarasiAwal] ⚠ '{bgDay2.name}' tidak punya SpriteRenderer!");
            }

            // FOOLPROOF: paksa aktifkan semua ancestor parent
            var t = bgDay2.transform.parent;
            while (t != null)
            {
                if (!t.gameObject.activeSelf)
                {
                    Debug.Log($"[Day2NarasiAwal] Aktifkan parent inactive: {t.name}");
                    t.gameObject.SetActive(true);
                }
                t = t.parent;
            }
        }
        SetGroupActive(objekTambahan, true);

        // 2. Sembunyikan backdrop biru/oranye Day2Controller supaya BG terlihat
        if (Day2Controller.Instance != null)
            Day2Controller.Instance.SetBackdropAktif(false);

        // 2b. NUCLEAR: hide SEMUA Canvas yang namanya mengandung 'backdrop'
        var allCanvas = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in allCanvas)
        {
            if (c == null) continue;
            string n = c.name.ToLower();
            if (n.Contains("backdrop"))
            {
                Debug.Log($"[Day2NarasiAwal] Nuclear hide canvas: '{c.name}'");
                c.gameObject.SetActive(false);
            }
        }

        // 3. Setup kamera (warna bg + posisi + ortho + disable CameraFollow)
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera != null)
        {
            _camBgAsli    = mainCamera.backgroundColor;
            _camPosAsli   = mainCamera.transform.position;
            _camOrthoAsli = mainCamera.orthographicSize;
            _camTersimpan = true;

            mainCamera.backgroundColor    = cameraBgWarna;
            mainCamera.transform.position = cameraPosNarasi;
            mainCamera.orthographicSize   = cameraOrthoNarasi;

            // FOOLPROOF: pastikan culling mask include semua layer (kalau dimask)
            mainCamera.cullingMask = -1; // Everything
            // Pastikan near clip plane tidak menutup sprite
            if (mainCamera.nearClipPlane > 0.5f) mainCamera.nearClipPlane = 0.3f;

            if (nonaktifkanCameraFollow)
            {
                _camFollow = mainCamera.GetComponent("CameraFollow") as Behaviour;
                if (_camFollow != null)
                {
                    _camFollowEnabledAsli = _camFollow.enabled;
                    _camFollow.enabled = false;
                }
            }

            Debug.Log($"[Day2NarasiAwal] Camera: pos={mainCamera.transform.position} " +
                      $"ortho={mainCamera.orthographicSize} cullingMask={mainCamera.cullingMask}");
        }

        // 4. Bangun panel dialog + jalankan narasi
        BuildCanvasDialog();
        _idxLine = 0;
        StartCoroutine(JalankanNarasi());
    }

    void Selesai()
    {
        Debug.Log("[Day2NarasiAwal] Selesai()");

        // Sembunyikan BG day 2 + props
        if (bgDay2 != null) bgDay2.SetActive(false);
        SetGroupActive(objekTambahan, false);

        // Restore kamera
        if (_camTersimpan && mainCamera != null)
        {
            mainCamera.backgroundColor    = _camBgAsli;
            mainCamera.transform.position = _camPosAsli;
            mainCamera.orthographicSize   = _camOrthoAsli;
            if (_camFollow != null) _camFollow.enabled = _camFollowEnabledAsli;
        }

        // Restore backdrop Day2Controller
        if (Day2Controller.Instance != null)
            Day2Controller.Instance.SetBackdropAktif(true);

        // Destroy canvas dialog
        if (_canvasGO != null) Destroy(_canvasGO);
        _canvasGO = null;

        // Callback ke Day2Controller → lanjut ke Halte
        _onSelesai?.Invoke();
        _onSelesai = null;
    }

    // ══════════════════════════════════════════════════════════════════════
    // DIALOG — typewriter + klik lanjut
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator JalankanNarasi()
    {
        // Pakai array baru kalau ada; kalau kosong, fallback ke legacy narasiLines
        int count = (narasi != null && narasi.Length > 0)
                    ? narasi.Length
                    : (narasiLines != null ? narasiLines.Count : 0);
        if (count == 0) { Selesai(); yield break; }

        while (_idxLine < count)
        {
            string pembicara;
            string teks;
            Sprite portrait;
            Sprite latarFs = null;

            if (narasi != null && _idxLine < narasi.Length)
            {
                var b = narasi[_idxLine];
                pembicara = string.IsNullOrEmpty(b.pembicara) ? speakerName : b.pembicara;
                teks      = b.teks ?? "";
                portrait  = PilihPortrait(pembicara, b.portrait);
                latarFs   = b.latarFullscreen;
            }
            else
            {
                pembicara = speakerName;
                teks      = narasiLines[_idxLine];
                portrait  = PilihPortrait(pembicara, null);
            }

            // Update banner nama + portrait setiap baris
            if (_namaTMP != null) _namaTMP.text = pembicara;
            if (_portImg != null)
            {
                _portImg.sprite  = portrait;
                _portImg.enabled = false; // potret/sprite profil disembunyikan dari box dialog
                if (portrait == null) _portImg.color = portraitFallbackWarna;
                else                  _portImg.color = Color.white;
            }

            // Update BG fullscreen per baris (kalau di-assign)
            if (_bgFullscreenImg != null && latarFs != null)
            {
                _bgFullscreenImg.sprite  = latarFs;
                _bgFullscreenImg.color   = Color.white;
                _bgFullscreenImg.enabled = true;
            }

            yield return KetikTeks(teks);
            yield return new WaitForSeconds(delaySetelahKetik);
            yield return TungguLanjut();
            _idxLine++;
        }

        Selesai();
    }

    IEnumerator KetikTeks(string teks)
    {
        _teksDialog.text = "";
        _teksHint.gameObject.SetActive(false);
        _ketikSelesai = false;
        _skipKetik    = false;

        if (kecepatanKetik <= 0f)
        {
            _teksDialog.text = teks;
        }
        else
        {
            for (int i = 0; i < teks.Length; i++)
            {
                if (_skipKetik) { _teksDialog.text = teks; break; }
                _teksDialog.text += teks[i];
                if (teks[i] != ' ') AudioManager.Instance?.PlayKetikHuruf();
                yield return new WaitForSeconds(kecepatanKetik);
            }
        }
        _ketikSelesai = true;
        _teksHint.gameObject.SetActive(true);
    }

    IEnumerator TungguLanjut()
    {
        // Hanya tombol LANJUT (atau SPACE/ENTER) yang melanjutkan; klik di luar diabaikan.
        _tombolLanjut?.Reset();
        bool lanjut = false;
        while (!lanjut)
        {
            bool ditekan = (_tombolLanjut != null && _tombolLanjut.Konsumsi())
                        || Input.GetKeyDown(KeyCode.Space)
                        || Input.GetKeyDown(KeyCode.Return)
                        || Input.GetKeyDown(KeyCode.KeypadEnter);
            if (ditekan)
            {
                if (!_ketikSelesai) _skipKetik = true;
                else                lanjut     = true;
            }
            yield return null;
        }
    }

    void SkipAtauLanjut()
    {
        // Dipanggil dari tombol panel
        if (!_ketikSelesai) _skipKetik = true;
    }

    // ══════════════════════════════════════════════════════════════════════
    // BUILD UI DIALOG — mirror Day1Intro.TampilkanNarasi (anchor fraksi)
    // ══════════════════════════════════════════════════════════════════════
    void BuildCanvasDialog()
    {
        // Apply layout asset (kalau ada) supaya field anchor sinkron
        ApplyLayoutAsset();

        _canvasGO = new GameObject("Day2_NarasiAwal_Canvas");
        var cv = _canvasGO.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 960;
        var sc = _canvasGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920f, 1080f);
        sc.matchWidthOrHeight  = 0.5f;
        _canvasGO.AddComponent<GraphicRaycaster>();

        // ── BG Fullscreen (paling belakang, di-update per baris kalau di-assign) ──
        var bgFsGO = new GameObject("BG_Fullscreen");
        bgFsGO.transform.SetParent(_canvasGO.transform, false);
        var bgFsRT = bgFsGO.AddComponent<RectTransform>();
        bgFsRT.anchorMin = Vector2.zero;
        bgFsRT.anchorMax = Vector2.one;
        bgFsRT.offsetMin = bgFsRT.offsetMax = Vector2.zero;
        _bgFullscreenImg = bgFsGO.AddComponent<Image>();
        _bgFullscreenImg.preserveAspect = false;
        _bgFullscreenImg.raycastTarget  = false;
        _bgFullscreenImg.enabled        = false; // aktif hanya saat sprite di-set

        // ── Panel utama (anchor fraksi layar) ─────────────────────────────
        float pxMin = panelCenterX - panelWidthFrac  * 0.5f;
        float pyMin = panelCenterY - panelHeightFrac * 0.5f;
        float pxMax = panelCenterX + panelWidthFrac  * 0.5f;
        float pyMax = panelCenterY + panelHeightFrac * 0.5f;

        var panelGO = new GameObject("Panel");
        panelGO.transform.SetParent(_canvasGO.transform, false);
        var panelRT = panelGO.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(pxMin, pyMin);
        panelRT.anchorMax = new Vector2(pxMax, pyMax);
        panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;

        var panelImg = panelGO.AddComponent<Image>();
        if (panelSprite != null)
        {
            panelImg.sprite = panelSprite;
            panelImg.type   = Image.Type.Sliced;
            panelImg.color  = Color.white;
        }
        else
        {
            panelImg.color = warnaPanel;
            var outline = panelGO.AddComponent<Outline>();
            outline.effectColor    = warnaOutline;
            outline.effectDistance = new Vector2(2f, -2f);
        }

        // Klik panel = skip / lanjut
        var btn = panelGO.AddComponent<Button>();
        btn.transition    = Selectable.Transition.None;
        btn.targetGraphic = panelImg;
        btn.onClick.AddListener(SkipAtauLanjut);

        // ── Portrait (anchor fraksi panel) ────────────────────────────────
        var portGO = new GameObject("Portrait");
        portGO.transform.SetParent(panelGO.transform, false);
        var portRT = portGO.AddComponent<RectTransform>();
        portRT.anchorMin = new Vector2(
            portraitCenterX - portraitSizeW * 0.5f,
            portraitCenterY - portraitSizeH * 0.5f);
        portRT.anchorMax = new Vector2(
            portraitCenterX + portraitSizeW * 0.5f,
            portraitCenterY + portraitSizeH * 0.5f);
        portRT.offsetMin = portRT.offsetMax = Vector2.zero;
        _portImg = portGO.AddComponent<Image>();
        _portImg.preserveAspect = portraitPreserveAspect;
        _portImg.color          = portraitFallbackWarna;
        _portImg.enabled        = false; // potret/sprite profil disembunyikan dari box dialog

        // ── Banner nama (anchor 0–1 panel) ────────────────────────────────
        var bannerGO = new GameObject("BannerNama");
        bannerGO.transform.SetParent(panelGO.transform, false);
        var bannerRT = bannerGO.AddComponent<RectTransform>();
        bannerRT.anchorMin = bannerAnchorMin;
        bannerRT.anchorMax = bannerAnchorMax;
        bannerRT.offsetMin = bannerRT.offsetMax = Vector2.zero;
        var bannerImg = bannerGO.AddComponent<Image>();
        if (nameBannerSprite != null)
        {
            bannerImg.sprite = nameBannerSprite;
            bannerImg.type   = Image.Type.Sliced;
            bannerImg.color  = Color.white;
        }
        else
        {
            bannerImg.color = warnaBanner;
        }

        _namaTMP = BuatTMP(bannerGO.transform, "Nama",
            Vector2.zero, Vector2.one,
            speakerName, ukuranNama, warnaNamaSpeaker, true);
        _namaTMP.alignment = TextAlignmentOptions.MidlineLeft;
        _namaTMP.margin    = new Vector4(12f, 0f, 4f, 0f);

        // ── Area teks (anchor 0–1 panel) ──────────────────────────────────
        _teksDialog = BuatTMP(panelGO.transform, "Teks",
            textAnchorMin, textAnchorMax,
            "", ukuranTeks, warnaTeks, false);
        _teksDialog.alignment        = TextAlignmentOptions.TopLeft;
        _teksDialog.textWrappingMode = TextWrappingModes.Normal;
        _teksDialog.overflowMode     = TextOverflowModes.Ellipsis;

        // ── Hint lanjut (anchor 0–1 panel) ────────────────────────────────
        _teksHint = BuatTMP(panelGO.transform, "Hint",
            new Vector2(hintCenterX - hintSizeW * 0.5f, hintCenterY - hintSizeH * 0.5f),
            new Vector2(hintCenterX + hintSizeW * 0.5f, hintCenterY + hintSizeH * 0.5f),
            teksHintLanjut, ukuranHint, warnaHintLanjut, false);
        _teksHint.alignment = TextAlignmentOptions.MidlineRight;
        _teksHint.gameObject.SetActive(false);

        // ── Tombol LANJUT: HANYA tombol ini yang melanjutkan narasi ──
        // (klik di luar tombol tidak lagi melanjutkan)
        _tombolLanjut = TombolLanjutVN.Pasang(panelGO.transform, null,
            "LANJUT", new Vector2(0.70f, 0.06f), new Vector2(0.975f, 0.26f));
    }

    // ── Helper buat TMP UGUI (sama tanda tangan dengan Day1Intro.BuatTMP) ──
    TextMeshProUGUI BuatTMP(Transform parent, string nama,
        Vector2 anchorMin, Vector2 anchorMax,
        string isi, int ukuran, Color warna, bool bold)
    {
        var go = new GameObject(nama);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text     = isi;
        tmp.fontSize = ukuran;
        tmp.color    = warna;
        if (bold) tmp.fontStyle = FontStyles.Bold;
        if (fontAsset != null) tmp.font = fontAsset;
        return tmp;
    }

    /// <summary>Pilih portrait berdasarkan nama pembicara (mirror Day1Intro).</summary>
    Sprite PilihPortrait(string pembicara, Sprite custom)
    {
        if (custom != null) return custom;
        string p = (pembicara ?? "").ToLower();
        if (p.Contains("narasi"))
            return portraitNarasi != null ? portraitNarasi : portraitRara;
        if (p.Contains("rara"))
            return portraitRara != null ? portraitRara : portraitSprite;
        return portraitRara != null ? portraitRara : portraitSprite;
    }

    /// <summary>
    /// Salin field DialogBoxLayout asset (kalau di-assign) ke field lokal,
    /// supaya beberapa komponen dialog bisa share satu asset.
    /// </summary>
    public void ApplyLayoutAsset()
    {
        if (layout == null) return;
        if (layout.boxSprite        != null) panelSprite      = layout.boxSprite;
        if (layout.nameBannerSprite != null) nameBannerSprite = layout.nameBannerSprite;

        panelCenterX    = layout.panelCenterX;
        panelCenterY    = layout.panelCenterY;
        panelWidthFrac  = layout.panelWidthFrac;
        panelHeightFrac = layout.panelHeightFrac;

        portraitCenterX        = layout.portraitCenterX;
        portraitCenterY        = layout.portraitCenterY;
        portraitSizeW          = layout.portraitSizeW;
        portraitSizeH          = layout.portraitSizeH;
        portraitPreserveAspect = layout.portraitPreserveAspect;

        bannerAnchorMin = layout.bannerAnchorMin;
        bannerAnchorMax = layout.bannerAnchorMax;

        textAnchorMin = layout.textAnchorMin;
        textAnchorMax = layout.textAnchorMax;

        hintCenterX = layout.hintCenterX;
        hintCenterY = layout.hintCenterY;
        hintSizeW   = layout.hintSizeW;
        hintSizeH   = layout.hintSizeH;
    }

#if UNITY_EDITOR
    // ══════════════════════════════════════════════════════════════════════
    // EDITOR HELPERS — auto-build BG day 2 + capture camera + load sprites
    // ══════════════════════════════════════════════════════════════════════

    void Reset()
    {
        // Auto-load sprite panel + layout asset bersama (kalau ada)
        TryLoadDialogSprites();
    }

    [ContextMenu("▶ Muat Sprite Box Dialog + Layout Asset")]
    void MuatSpriteBoxDialog()
    {
        TryLoadDialogSprites();
        Debug.Log($"[Day2NarasiAwal] panelSprite={(panelSprite != null ? panelSprite.name : "null")} " +
                  $"nameBannerSprite={(nameBannerSprite != null ? nameBannerSprite.name : "null")} " +
                  $"portraitRara={(portraitRara != null ? portraitRara.name : "null")} " +
                  $"layout={(layout != null ? layout.name : "null")}");
        UnityEditor.EditorUtility.SetDirty(this);
    }

    void TryLoadDialogSprites()
    {
        // Panel sprite default (sama dengan Day1Intro: UI day 1/8.png)
        if (panelSprite == null && !string.IsNullOrEmpty(panelSpritePath))
        {
            var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/" + panelSpritePath);
            if (sp != null) panelSprite = sp;
        }
        // Layout asset bersama (DialogLayoutDefault.asset)
        if (layout == null)
        {
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<DialogBoxLayout>("Assets/DialogLayoutDefault.asset");
            if (asset != null) layout = asset;
        }
        // Coba ambil portraitRara dari Day1Intro di scene (kalau ada)
        if (portraitRara == null)
        {
            var day1 = FindFirstObjectByType<Day1Intro>();
            if (day1 != null)
            {
                if (day1.portraitRara != null) portraitRara = day1.portraitRara;
                if (nameBannerSprite == null && day1.nameBannerSprite != null)
                    nameBannerSprite = day1.nameBannerSprite;
            }
        }
    }

    [ContextMenu("▶ Buat BG day 2 di Hierarchy")]
    void BuatBGDay2DiHierarchy()
    {
        if (bgDay2 != null)
        {
            Debug.LogWarning($"[Day2NarasiAwal] BG day 2 sudah ada: '{bgDay2.name}'. " +
                "Hapus dulu kalau mau regenerate.");
            UnityEditor.Selection.activeGameObject = bgDay2;
            return;
        }

        var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/" + bgSpritePath);
        if (sp == null)
        {
            Debug.LogError($"[Day2NarasiAwal] Sprite tidak ketemu di Assets/{bgSpritePath}. " +
                "Periksa field 'Bg Sprite Path' atau drag manual.");
            return;
        }

        // Buat parent GO di posisi area Day 1 (40, 14, 0) — sama dengan jalan ramai/gang sepi
        var go = new GameObject("BG day 2");
        UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Buat BG day 2");
        go.transform.position = new Vector3(40f, 14f, 0f);

        // Parent ke Day2_Root kalau ada
        var day2Root = GameObject.Find("Day2_Root");
        if (day2Root != null)
        {
            go.transform.SetParent(day2Root.transform, true);
            Debug.Log("[Day2NarasiAwal] Parent-kan ke 'Day2_Root'.");
        }

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sp;
        sr.sortingOrder = 0;

        bgDay2 = go;
        UnityEditor.EditorUtility.SetDirty(this);

        // Auto-fit kamera pakai bounds sprite
        var b = sr.bounds;
        cameraPosNarasi   = new Vector3(b.center.x, b.center.y, -10f);
        float aspect      = 16f / 9f;
        float orthoForH   = b.size.y * 0.5f;
        float orthoForW   = (b.size.x * 0.5f) / aspect;
        cameraOrthoNarasi = Mathf.Max(orthoForH, orthoForW);

        UnityEditor.Selection.activeGameObject = go;
        UnityEditor.SceneView.lastActiveSceneView?.FrameSelected();

        Debug.Log($"[Day2NarasiAwal] ✓ 'BG day 2' dibuat di {go.transform.position} " +
                  $"size={b.size}. Camera auto-set: pos={cameraPosNarasi} ortho={cameraOrthoNarasi}");
    }

    [ContextMenu("📷 Capture Camera dari Scene View (2D)")]
    void CaptureCameraDariSceneView()
    {
        var sv = UnityEditor.SceneView.lastActiveSceneView;
        if (sv == null || sv.camera == null)
        {
            Debug.LogError("[Day2NarasiAwal] Scene View tidak terbuka.");
            return;
        }
        if (!sv.in2DMode)
        {
            Debug.LogWarning("[Day2NarasiAwal] Scene View belum di mode 2D. Klik tombol '2D' di toolbar Scene View.");
        }
        cameraPosNarasi   = sv.camera.transform.position;
        cameraOrthoNarasi = sv.camera.orthographicSize;
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[Day2NarasiAwal] 📷 Capture Scene View: pos={cameraPosNarasi} ortho={cameraOrthoNarasi}");
    }

    [ContextMenu("📷 Apply Camera ke Scene View (Preview)")]
    void ApplyCameraKeSceneView()
    {
        var sv = UnityEditor.SceneView.lastActiveSceneView;
        if (sv == null) { Debug.LogError("[Day2NarasiAwal] Scene View tidak terbuka."); return; }
        sv.in2DMode = true;
        sv.pivot    = new Vector3(cameraPosNarasi.x, cameraPosNarasi.y, 0f);
        sv.size     = cameraOrthoNarasi;
        sv.Repaint();
    }

    [ContextMenu("🎯 Auto-Fit Kamera Sekarang (re-hitung dari BG)")]
    void EditorAutoFitKamera()
    {
        if (bgDay2 == null)
        {
            Debug.LogError("[Day2NarasiAwal] bgDay2 belum di-assign. Klik '▶ Buat BG day 2 di Hierarchy' dulu.");
            return;
        }
        if (kameraMode == KameraMode.Manual)
        {
            Debug.LogWarning("[Day2NarasiAwal] Kamera mode = Manual. Set ke AutoFitCover/Contain dulu, atau capture manual via Scene View.");
            return;
        }
        AutoFitKameraDariBgDay2();
        UnityEditor.EditorUtility.SetDirty(this);
        ApplyCameraKeSceneView();
        Debug.Log($"[Day2NarasiAwal] ✓ Auto-fit selesai. Lihat hasil di Scene View. Mode={kameraMode}, zoom={kameraZoomFaktor:F2}");
    }

    [ContextMenu("🎬 Apply Camera ke MAIN CAMERA (preview Game View)")]
    void ApplyKeMainCamera()
    {
        var cam = mainCamera != null ? mainCamera : Camera.main;
        if (cam == null) { Debug.LogError("[Day2NarasiAwal] Main camera tidak ketemu."); return; }
        cam.orthographic     = true;
        cam.transform.position = cameraPosNarasi;
        cam.orthographicSize = cameraOrthoNarasi;
        cam.backgroundColor  = cameraBgWarna;
        Debug.Log($"[Day2NarasiAwal] 🎬 Apply ke Main Camera: pos={cameraPosNarasi} ortho={cameraOrthoNarasi}");
    }
#endif
}
