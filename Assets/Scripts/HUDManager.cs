using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Mengelola HUD game: navbar (skor, nyawa, progress hari, shout gauge)
/// dan layar intro hari.
///
/// Centang 'buildHUDAtRuntime' untuk membangun navbar gaya baru secara otomatis
/// tanpa perlu setup Canvas manual di Inspector.
///
/// Pemanggilan dari Day1Controller:
///   hudManager.Refresh()
///   hudManager.ShowDayIntro(1)       → layar intro hari
///   hudManager.SetShoutGauge(0–1)    → isi gauge TERIAK
///   hudManager.FlashHeartLost(lives) → animasi kehilangan nyawa
/// </summary>
public class HUDManager : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — Setup Manual (diabaikan jika buildHUDAtRuntime = true)
    // ══════════════════════════════════════════════════════════════════════

    [Header("Nyawa (Hati) — setup manual")]
    public Image[]         heartImages;
    public Sprite          heartFull;
    public Sprite          heartEmpty;

    [Header("Teks — setup manual")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI locationText;
    public TextMeshProUGUI dayText;

    [Header("Lokasi Per Hari")]
    public string[] locationNames =
    {
        "Jalan Menuju Sekolah",
        "Angkot Jurusan Sekolah",
        "Parkiran SMP — Musim Hujan"
    };

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — Navbar Otomatis
    // ══════════════════════════════════════════════════════════════════════

    [Header("Navbar Otomatis (centang = bangun gaya baru)")]
    public bool          buildHUDAtRuntime = true;
    public TMP_FontAsset fontAsset;

    [Tooltip("Sprite hati penuh. Jika kosong, pakai lingkaran merah solid.")]
    public Sprite heartFullSprite;
    [Tooltip("Sprite hati kosong. Jika kosong, pakai lingkaran abu solid.")]
    public Sprite heartEmptySprite;

    [Header("Warna Navbar (palet sunset/kayu)")]
    [Tooltip("Warna panel HUD — coklat tua semi-transparan, selaras tema sunset/Padang.")]
    public Color panelBgColor     = new Color(0.18f, 0.08f, 0.04f, 0.88f);
    [Tooltip("Warna border emas pada panel HUD.")]
    public Color panelBorderColor = new Color(0.95f, 0.72f, 0.18f, 1f);
    [Tooltip("Warna aktif (hari sekarang) — kuning emas terang.")]
    public Color dayActiveColor   = new Color(1f,    0.82f, 0.18f, 1f);
    [Tooltip("Warna tidak aktif — coklat redup.")]
    public Color dayInactiveColor = new Color(0.42f, 0.27f, 0.12f, 1f);
    public Color gaugeFillColor   = new Color(0.96f, 0.45f, 0.10f, 1f);
    public Color gaugeEmptyColor  = new Color(0.10f, 0.05f, 0.02f, 1f);

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — KUSTOMISASI NAVBAR PROGRESS HARI (H1/H2/H3)
    // Semua field ini bisa diubah di Inspector & di-Apply LANGSUNG saat Play
    // lewat tombol konteks "▶ Apply Custom Navbar" (klik kanan komponen).
    // ══════════════════════════════════════════════════════════════════════
    [Header("Navbar Hari — Label Teks")]
    [Tooltip("Label di bawah tiap lingkaran. 3 elemen → H1, H2, H3 (bisa diganti)")]
    public string[] dayLabels = { "H1", "H2", "H3" };

    [Header("Navbar Hari — Ukuran Font")]
    [Tooltip("Ukuran font angka di dalam lingkaran (1, 2, 3)")]
    [Range(8, 60)] public int dayNumFontSize   = 22;
    [Tooltip("Ukuran font label di bawah lingkaran (H1, H2, H3)")]
    [Range(8, 60)] public int dayLabelFontSize = 19;

    [Header("Navbar Hari — Ukuran Lingkaran")]
    [Tooltip("Diameter lingkaran hari dalam pixel")]
    [Range(20f, 120f)] public float dayCircleSize = 50f;

    [Header("Navbar Hari — Posisi Horizontal (X)")]
    [Tooltip("Posisi X lingkaran H1 (fraksi 0=kiri, 1=kanan panel center)")]
    [Range(0f, 1f)] public float dayCircleX1 = 0.15f;
    [Tooltip("Posisi X lingkaran H2")]
    [Range(0f, 1f)] public float dayCircleX2 = 0.50f;
    [Tooltip("Posisi X lingkaran H3")]
    [Range(0f, 1f)] public float dayCircleX3 = 0.85f;

    [Header("Navbar Hari — Warna (di luar Active/Inactive)")]
    [Tooltip("Warna teks angka saat hari AKTIF (dalam lingkaran)")]
    public Color dayNumColorActive   = Color.black;
    [Tooltip("Warna teks angka saat hari NON-AKTIF")]
    public Color dayNumColorInactive = new Color(0.62f, 0.62f, 0.65f, 0.85f);
    [Tooltip("Warna teks label (H1/H2/H3) saat hari NON-AKTIF")]
    public Color dayLabelColorInactive = new Color(0.48f, 0.48f, 0.52f, 0.85f);

    [Header("Navbar Hari — Tebal Garis Konektor")]
    [Tooltip("Tinggi (tebal) garis penghubung antar lingkaran")]
    [Range(2f, 30f)] public float dayLineThickness = 12f;

    [Header("Navbar Hari — Live Edit")]
    [Tooltip("Centang: tiap perubahan Inspector langsung diterapkan saat Play tanpa restart")]
    public bool liveEditNavbar = true;

    [Header("Intro Hari")]
    public float introDuration     = 2.8f;
    public float introFadeDuration = 0.5f;

    [Header("Popup Skor (kustomisasi bebas)")]
    [Tooltip("Ukuran font teks popup skor")]
    public int   popupFontSize      = 52;
    [Tooltip("Berapa pixel popup naik selama animasi")]
    public float popupRisePixels    = 120f;
    [Tooltip("Durasi total animasi popup dalam detik")]
    public float popupDuration      = 1.5f;
    [Tooltip("Posisi horizontal popup skor (0=kiri, 1=kanan layar)")]
    public float popupAnchorX       = 0.07f;
    [Tooltip("Posisi vertikal popup skor (0=bawah, 1=atas layar)")]
    public float popupAnchorY       = 0.90f;
    [Tooltip("Offset vertikal popup nyawa berkurang dari popup skor")]
    public float popupLifeLostOffsetY = -0.02f;
    [Tooltip("Format teks popup AMAN. {0} = angka poin")]
    public string popupFormatAman   = "+{0} POIN";
    [Tooltip("Format teks popup RAGU. {0} = angka poin")]
    public string popupFormatRagu   = "+{0} poin";
    [Tooltip("Teks popup BAHAYA (poin nol)")]
    public string popupTeksBahaya   = "0 POIN";
    [Tooltip("Teks popup saat nyawa berkurang")]
    public string popupTeksNyawa    = "\u22121 \u2764";
    public Color  popupWarnaAman    = new Color(0.15f, 0.90f, 0.45f, 1f);
    public Color  popupWarnaRagu    = new Color(0.97f, 0.70f, 0.10f, 1f);
    public Color  popupWarnaBahaya  = new Color(0.95f, 0.30f, 0.25f, 1f);
    public Color  popupWarnaNyawa   = new Color(0.95f, 0.18f, 0.18f, 1f);

    // ══════════════════════════════════════════════════════════════════════
    // RUNTIME REFS
    // ══════════════════════════════════════════════════════════════════════

    private TextMeshProUGUI   _rScore;
    private Image[]           _rHearts;
    private Image[]           _dayCircles;
    private TextMeshProUGUI[] _dayNums;
    private TextMeshProUGUI[] _dayLabels;
    private Image[]           _dayLines;
    private RectTransform     _gaugeFillRT;
    private Image             _gaugeFillImg;       // referensi fill untuk ganti warna
    private RectTransform     _gaugeMarkerRT;      // garis merah threshold

    // ── Voice Meter: fill bar + 3 zona background + marker merah ───────
    private TextMeshProUGUI _gaugeLevelLabel;    // teks level di kiri bar
    private Image[]         _gaugeZones;         // [0]=hijau [1]=kuning [2]=merah background
    private RectTransform   _gaugeFillBgRT;      // parent bar (untuk child fill)

    // Batas zona sebagai fraksi 0–1 dari threshold Loud
    // Zona hijau: 0 – thresholdMedium, kuning: thresholdMedium – thresholdLoud, merah: sisanya
    // Versi visual statis untuk saat VoiceMeter belum ada:
    const float ZN_NORMAL_END = 0.40f;
    const float ZN_MEDIUM_END = 0.72f;

    private CanvasGroup      _introGroup;
    private TextMeshProUGUI  _introTitle;
    private TextMeshProUGUI  _introSub;
    private Coroutine        _introCoroutine;

    // Sprite bersama — dibuat sekali saat runtime
    private static Sprite _sCircle;
    private static Sprite _sRoundRect;

    // Referensi singleton runtime — agar Day1Controller bisa mengaksesnya
    public static HUDManager Instance { get; private set; }

    // ══════════════════════════════════════════════════════════════════════
    // AUTO-SPAWN: navbar muncul otomatis di setiap scene Play tanpa perlu
    // menaruh HUDManager secara manual di scene Hierarchy.
    // BeforeSceneLoad memastikan Instance sudah ada sebelum Awake/Start apapun.
    // ══════════════════════════════════════════════════════════════════════
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoSpawn()
    {
        // Jangan spawn ganda
        if (Instance != null) return;

        var go = new GameObject("[HUDManager]");
        DontDestroyOnLoad(go);
        var hud = go.AddComponent<HUDManager>();
        // buildHUDAtRuntime default = true, jadi navbar langsung dibangun di Start()
        Instance = hud;
    }

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        // Jika ada instance manual di scene, prioritaskan itu; hancurkan duplikat
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (buildHUDAtRuntime)
            BuildHUD();
        Refresh();

        // Inisialisasi tracker — biar Update() tahu nilai awal hari
        if (GameState.Instance != null)
            _lastTrackedDay = GameState.Instance.day;
    }

    // ── Tracker perubahan hari ────────────────────────────────────────────
    // Diperiksa tiap frame supaya navbar (H1/H2/H3) langsung pindah aktif
    // begitu GameState.Instance.day berubah — tidak perlu lagi memanggil
    // HUDManager.Instance.Refresh() secara manual di setiap controller.
    private int _lastTrackedDay = -1;

    void Update()
    {
        var gs = GameState.Instance;
        if (gs == null) return;

        // Day berubah → refresh seluruh HUD (lingkaran H1/H2/H3 + label + lokasi)
        if (gs.day != _lastTrackedDay)
        {
            _lastTrackedDay = gs.day;
            Debug.Log($"[HUDManager] Day berubah → {gs.day}. Refresh navbar (H1/H2/H3).");
            Refresh();
        }

        // Live edit: terapkan perubahan Inspector ke navbar secara real-time
        if (liveEditNavbar) ApplyNavbarCustomization();
    }

    // ══════════════════════════════════════════════════════════════════════
    // PUBLIC API — backward-compatible
    // ══════════════════════════════════════════════════════════════════════

    /// Perbarui seluruh HUD dari GameState.
    public void Refresh()
    {
        if (GameState.Instance == null) return;
        UpdateHearts(GameState.Instance.lives, GameState.Instance.maxLives);
        UpdateScore(GameState.Instance.score);
        UpdateLocation(GameState.Instance.day);
        UpdateDay(GameState.Instance.day);
        UpdateDayProgress(GameState.Instance.day);
    }

    public void UpdateHearts(int current, int max)
    {
        // Manual setup
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null) continue;
            heartImages[i].sprite = (i < current) ? heartFull : heartEmpty;
        }
        // Runtime navbar
        if (_rHearts == null) return;
        for (int i = 0; i < _rHearts.Length; i++)
        {
            if (_rHearts[i] == null) continue;
            bool alive = i < current;
            if (heartFullSprite != null && heartEmptySprite != null)
                _rHearts[i].sprite = alive ? heartFullSprite : heartEmptySprite;
            else
                _rHearts[i].color = alive
                    ? new Color(0.92f, 0.18f, 0.18f, 1f)
                    : new Color(0.32f, 0.32f, 0.36f, 0.75f);
        }
    }

    public void UpdateScore(int score)
    {
        Debug.Log($"[HUDManager] UpdateScore({score}) | _rScore={(  _rScore != null ? "ada" : "NULL")} | scoreText={(scoreText != null ? "ada" : "NULL")}");
        if (scoreText != null) scoreText.text = $"Skor: {score}";
        if (_rScore   != null) _rScore.text   = $"Skor: {score}";
    }

    public void UpdateLocation(int day)
    {
        if (locationText == null) return;
        int idx = Mathf.Clamp(day - 1, 0, locationNames.Length - 1);
        locationText.text = locationNames[idx];
    }

    public void UpdateDay(int day)
    {
        if (dayText != null) dayText.text = $"Hari {day}";
    }

    /// Perbarui lingkaran progress hari (H1/H2/H3).
    public void UpdateDayProgress(int currentDay)
    {
        if (_dayCircles == null) return;
        for (int i = 0; i < _dayCircles.Length; i++)
        {
            bool active = (i + 1 == currentDay);
            bool done   = (i + 1 < currentDay);

            if (_dayCircles[i] != null)
                _dayCircles[i].color = active ? dayActiveColor : dayInactiveColor;

            if (_dayNums != null && i < _dayNums.Length && _dayNums[i] != null)
                _dayNums[i].color = active ? dayNumColorActive : dayNumColorInactive;

            if (_dayLabels != null && i < _dayLabels.Length && _dayLabels[i] != null)
                _dayLabels[i].color = active ? dayActiveColor : dayLabelColorInactive;

            if (_dayLines != null && i < _dayLines.Length && _dayLines[i] != null)
                _dayLines[i].color = done ? dayActiveColor : dayInactiveColor;
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // TRANSISI HARI — API publik dipanggil saat tombol "LANJUT HARI X" diklik
    // Logika tersentralisasi di sini supaya tidak tersebar di banyak script.
    // ══════════════════════════════════════════════════════════════════════

    [Header("Transisi Hari — Animasi Highlight")]
    [Tooltip("Centang: saat masuk hari baru, lingkaran hari tersebut berkedip emas → ukuran membesar lalu kembali normal.")]
    public bool   animasiTransisiHari = true;
    [Tooltip("Durasi animasi highlight (detik) saat masuk hari baru.")]
    [Range(0.1f, 2f)] public float durasiHighlightHari = 0.6f;
    [Tooltip("Skala maksimum lingkaran saat dianimasi (1.0 = ukuran normal, 1.4 = membesar 40%).")]
    [Range(1f, 2f)] public float skalaPuncakHighlight = 1.35f;

    private Coroutine _coHighlightHari;

    /// <summary>
    /// Dipanggil saat user MENEKAN TOMBOL "LANJUT HARI X" — menandakan
    /// bahwa hari sudah berganti. Method ini:
    ///   1. Set GameState.Instance.day = dayNumber
    ///   2. Refresh seluruh HUD (skor, nyawa, lokasi, lingkaran H1/H2/H3)
    ///   3. Putar animasi highlight pada lingkaran hari baru (opsional)
    ///
    /// Aman dipanggil berkali-kali — tidak akan terjadi double-refresh.
    /// </summary>
    // Tracker hari terakhir yang animasinya sudah dimainkan — cegah animasi
    // diputar dua kali bila EnterDay() dipanggil berurutan dari dua jalur
    // berbeda (mis. Day1SummaryScreen → DayTransitionManager → keduanya call).
    private int _lastAnimatedDay = -1;

    public void EnterDay(int dayNumber)
    {
        dayNumber = Mathf.Clamp(dayNumber, 1, 3);

        var gs = GameState.Instance;
        if (gs == null)
        {
            Debug.LogWarning($"[HUDManager] EnterDay({dayNumber}) dipanggil tapi GameState.Instance NULL.");
            return;
        }

        int hariSebelum = gs.day;
        bool hariBerubah = (hariSebelum != dayNumber);

        // 1. Sinkronkan state
        if (hariBerubah)
        {
            Debug.Log($"[HUDManager] EnterDay: hari {hariSebelum} → {dayNumber} (set GameState.day)");
            gs.day = dayNumber;
        }
        else
        {
            Debug.Log($"[HUDManager] EnterDay({dayNumber}) — hari sudah sesuai, refresh navbar saja.");
        }

        // 2. Update tracker supaya Update() tidak men-trigger refresh kedua
        _lastTrackedDay = dayNumber;

        // 3. Refresh seluruh HUD
        Refresh();

        // 4. Animasi highlight — hanya kalau memang hari baru (atau pertama kali)
        bool perluAnimasi = animasiTransisiHari && (hariBerubah || _lastAnimatedDay != dayNumber);
        if (perluAnimasi && _dayCircles != null && isActiveAndEnabled)
        {
            int idx = dayNumber - 1;
            if (idx >= 0 && idx < _dayCircles.Length && _dayCircles[idx] != null)
            {
                _lastAnimatedDay = dayNumber;
                if (_coHighlightHari != null) StopCoroutine(_coHighlightHari);
                _coHighlightHari = StartCoroutine(CoHighlightHari(_dayCircles[idx].rectTransform));
            }
        }
    }

    /// <summary>Shortcut: dipanggil saat tombol "LANJUT HARI 2" diklik.</summary>
    public void OnLanjutHari2() => EnterDay(2);

    /// <summary>Shortcut: dipanggil saat tombol "LANJUT HARI 3" diklik.</summary>
    public void OnLanjutHari3() => EnterDay(3);

    private IEnumerator CoHighlightHari(RectTransform rt)
    {
        if (rt == null) yield break;

        float t = 0f;
        Vector3 baseScale = Vector3.one;
        rt.localScale = baseScale;

        while (t < durasiHighlightHari)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / durasiHighlightHari);
            // Kurva: cepat membesar 0→1 (ease-out), lalu balik 1→0 (ease-in)
            float bell = 1f - Mathf.Abs(2f * p - 1f);
            float eased = bell * bell * (3f - 2f * bell);
            float scale = Mathf.Lerp(1f, skalaPuncakHighlight, eased);
            rt.localScale = baseScale * scale;
            yield return null;
        }
        rt.localScale = baseScale;
        _coHighlightHari = null;
    }

    [ContextMenu("▶ Test: Masuk Hari 2")]
    void Ctx_TestEnterDay2() => EnterDay(2);

    [ContextMenu("▶ Test: Masuk Hari 3")]
    void Ctx_TestEnterDay3() => EnterDay(3);

    // ══════════════════════════════════════════════════════════════════════
    // KUSTOMISASI LIVE — terapkan perubahan Inspector tanpa rebuild HUD
    // Dipanggil tiap frame jika liveEditNavbar = true, atau manual dari
    // tombol konteks "▶ Apply Custom Navbar".
    // ══════════════════════════════════════════════════════════════════════
    public void ApplyNavbarCustomization()
    {
        if (_dayCircles == null) return;
        float[] nx = { Mathf.Clamp01(dayCircleX1), Mathf.Clamp01(dayCircleX2), Mathf.Clamp01(dayCircleX3) };

        // Lingkaran: ukuran + posisi X
        for (int i = 0; i < _dayCircles.Length; i++)
        {
            if (_dayCircles[i] == null) continue;
            var rt = _dayCircles[i].rectTransform;
            rt.sizeDelta = new Vector2(dayCircleSize, dayCircleSize);
            rt.anchorMin = rt.anchorMax = new Vector2(nx[i], 0.72f);
        }

        // Nomor: ukuran font
        if (_dayNums != null)
            foreach (var n in _dayNums) if (n != null) n.fontSize = dayNumFontSize;

        // Label: teks + ukuran font + posisi X
        if (_dayLabels != null)
        {
            for (int i = 0; i < _dayLabels.Length; i++)
            {
                if (_dayLabels[i] == null) continue;
                _dayLabels[i].fontSize = dayLabelFontSize;
                if (dayLabels != null && i < dayLabels.Length && !string.IsNullOrEmpty(dayLabels[i]))
                    _dayLabels[i].text = dayLabels[i];
                var lblRT = _dayLabels[i].rectTransform;
                lblRT.anchorMin = new Vector2(nx[i] - 0.10f, 0.00f);
                lblRT.anchorMax = new Vector2(nx[i] + 0.10f, 0.36f);
            }
        }

        // Garis konektor: tebal + posisi X mengikuti lingkaran
        if (_dayLines != null)
        {
            float lineHalfFrac = Mathf.Clamp(dayLineThickness / 96f, 0.02f, 0.30f) * 0.5f;
            float lineCenterY  = 0.66f;
            for (int i = 0; i < _dayLines.Length; i++)
            {
                if (_dayLines[i] == null) continue;
                var lrt = _dayLines[i].rectTransform;
                lrt.anchorMin = new Vector2(nx[i] + 0.11f,     lineCenterY - lineHalfFrac);
                lrt.anchorMax = new Vector2(nx[i + 1] - 0.11f, lineCenterY + lineHalfFrac);
            }
        }

        // Refresh warna sesuai hari aktif
        if (GameState.Instance != null) UpdateDayProgress(GameState.Instance.day);
    }

    [ContextMenu("▶ Apply Custom Navbar")]
    void Ctx_ApplyNavbar() => ApplyNavbarCustomization();

    /// Perbarui Voice Meter tiap frame.
    /// value = VoiceMeter.NormalizedLevel (0–1 amplitudo RMS mentah).
    /// Fill bar dipetakan ke seluruh lebar bar menggunakan threshold,
    /// sehingga teriak keras = bar penuh.
    public void SetShoutGauge(float value)
    {
        float v = Mathf.Clamp01(value);

        // ── Petakan nilai mentah ke posisi visual penuh (0–1 lebar bar) ──
        float fillPos = MapToFillPos(v);

        // ── Fill bar ──────────────────────────────────────────────────────
        if (_gaugeFillRT != null)
        {
            // Minimal 4% lebar agar selalu ada, maks 100%
            float fillWidth = Mathf.Max(0.04f, fillPos);
            _gaugeFillRT.anchorMax = new Vector2(fillWidth, 1f);
        }

        var vm = VoiceMeter.Instance;

        // ── Warna fill sesuai level ───────────────────────────────────────
        if (_gaugeFillImg != null)
        {
            Color targetFill;
            if (vm != null)
            {
                switch (vm.Level)
                {
                    case VoiceMeter.VoiceLevel.Loud:
                        targetFill = Color.Lerp(
                            VoiceMeter.ColorLoud, Color.white,
                            Mathf.Abs(Mathf.Sin(Time.time * 9f)) * vm.LoudIntensity);
                        break;
                    case VoiceMeter.VoiceLevel.Medium:
                        targetFill = VoiceMeter.ColorMedium;
                        break;
                    case VoiceMeter.VoiceLevel.Normal:
                        targetFill = VoiceMeter.ColorNormal;
                        break;
                    default:
                        targetFill = new Color(0.28f, 0.28f, 0.32f, 1f);
                        break;
                }
            }
            else
            {
                targetFill = fillPos > ZN_MEDIUM_END ? new Color(0.92f, 0.22f, 0.18f, 1f)
                           : fillPos > ZN_NORMAL_END ? new Color(0.95f, 0.62f, 0.07f, 1f)
                           : fillPos > 0.05f         ? new Color(0.15f, 0.68f, 0.38f, 1f)
                           :                           new Color(0.28f, 0.28f, 0.32f, 1f);
            }
            _gaugeFillImg.color = Color.Lerp(_gaugeFillImg.color, targetFill, 0.20f);
        }

        // ── Zona background: terangkan zona aktif ────────────────────────
        if (_gaugeZones != null && vm != null)
        {
            Color[] on = {
                new Color(0.12f, 0.68f, 0.30f, 0.60f),
                new Color(0.95f, 0.62f, 0.07f, 0.60f),
                new Color(0.92f, 0.22f, 0.18f, 0.60f),
            };
            Color[] off = {
                new Color(0.10f, 0.45f, 0.20f, 0.28f),
                new Color(0.60f, 0.40f, 0.04f, 0.28f),
                new Color(0.55f, 0.10f, 0.08f, 0.28f),
            };
            int active = vm.Level switch {
                VoiceMeter.VoiceLevel.Normal => 0,
                VoiceMeter.VoiceLevel.Medium => 1,
                VoiceMeter.VoiceLevel.Loud   => 2,
                _                            => -1,
            };
            for (int i = 0; i < _gaugeZones.Length; i++)
            {
                if (_gaugeZones[i] == null) continue;
                Color target = (i == active) ? on[i] : off[i];
                _gaugeZones[i].color = Color.Lerp(_gaugeZones[i].color, target, 0.18f);
            }
        }

        // ── Label level ───────────────────────────────────────────────────
        if (_gaugeLevelLabel != null && vm != null)
        {
            _gaugeLevelLabel.text = vm.NamaLevel();
            Color lc = vm.Level switch {
                VoiceMeter.VoiceLevel.Normal => VoiceMeter.ColorNormal,
                VoiceMeter.VoiceLevel.Medium => VoiceMeter.ColorMedium,
                VoiceMeter.VoiceLevel.Loud   => VoiceMeter.ColorLoud,
                _                            => new Color(0.50f, 0.50f, 0.55f, 1f),
            };
            _gaugeLevelLabel.color = Color.Lerp(_gaugeLevelLabel.color, lc, 0.18f);
        }
    }

    /// Petakan amplitudo RMS mentah (0–1) ke posisi visual fill bar (0–1).
    /// Zona Normal  → 0.0  – ZN_NORMAL_END (0.40)
    /// Zona Medium  → 0.40 – ZN_MEDIUM_END (0.72)
    /// Zona Loud    → 0.72 – 1.00
    float MapToFillPos(float raw)
    {
        var vm = VoiceMeter.Instance;
        if (vm == null) return raw;

        float thN = Mathf.Max(vm.thresholdNormal, 0.001f);
        float thM = Mathf.Max(vm.thresholdMedium, thN + 0.001f);
        float thL = Mathf.Max(vm.thresholdLoud,   thM + 0.001f);

        if (raw <= 0f)
            return 0f;
        else if (raw < thN)
            // Senyap → awal zona Normal (0 – 40% * 0.4)
            return Mathf.Lerp(0f, ZN_NORMAL_END * 0.35f, raw / thN);
        else if (raw < thM)
            // Normal → zona Normal penuh (0.14 – 0.40)
            return Mathf.Lerp(ZN_NORMAL_END * 0.35f, ZN_NORMAL_END,
                (raw - thN) / (thM - thN));
        else if (raw < thL)
            // Medium → zona Medium (0.40 – 0.72)
            return Mathf.Lerp(ZN_NORMAL_END, ZN_MEDIUM_END,
                (raw - thM) / (thL - thM));
        else
            // Loud → zona merah (0.72 – 1.0)
            return Mathf.Lerp(ZN_MEDIUM_END, 1.0f,
                Mathf.Clamp01((raw - thL) / Mathf.Max(1f - thL, 0.05f)));
    }

    /// Petakan nilai threshold amplitudo ke posisi visual pada bar (0–1).
    float MapThresholdPos(float thresholdLoud)
    {
        return ZN_MEDIUM_END;
    }

    /// Tampilkan layar intro "HARI N: ..." dengan fade in/out.
    /// onComplete dipanggil setelah animasi selesai — gunakan untuk mulai game.
    public void ShowDayIntro(int day, System.Action onComplete = null)
    {
        if (_introGroup == null) BuildIntroUI();
        if (_introTitle != null) _introTitle.text = DayTitle(day);
        if (_introSub   != null)
        {
            int idx = Mathf.Clamp(day - 1, 0, locationNames.Length - 1);
            _introSub.text = "📍  " + locationNames[idx];
        }
        if (_introCoroutine != null) StopCoroutine(_introCoroutine);
        _introCoroutine = StartCoroutine(PlayIntroCoroutine(onComplete));
    }

    /// Animasikan kehilangan nyawa.
    public void FlashHeartLost(int newLives)
    {
        UpdateHearts(newLives, GameState.Instance?.maxLives ?? 3);

        if (_rHearts != null && newLives < _rHearts.Length && _rHearts[newLives] != null)
            StartCoroutine(FlashImage(_rHearts[newLives]));

        if (heartImages != null && newLives < heartImages.Length && heartImages[newLives] != null)
            StartCoroutine(FlashImage(heartImages[newLives]));
    }

    // ══════════════════════════════════════════════════════════════════════
    // SCORE POPUP — teks melayang naik kemudian menghilang
    // ══════════════════════════════════════════════════════════════════════

    private Canvas _popupCanvas;   // canvas khusus popup agar tidak tumpang tindih dengan navbar

    /// Tampilkan popup skor mengambang di dekat pojok kiri atas (dekat skor).
    public void ShowScorePopup(int pts, string kategori = "AMAN")
    {
        if (pts <= 0 && kategori != "BAHAYA") return;   // tidak perlu popup 0 poin non-BAHAYA

        string teks;
        Color  warna;

        switch (kategori)
        {
            case "AMAN":
                teks  = string.Format(popupFormatAman, pts);
                warna = popupWarnaAman;
                break;
            case "RAGU":
                teks  = string.Format(popupFormatRagu, pts);
                warna = popupWarnaRagu;
                break;
            default:    // BAHAYA
                teks  = popupTeksBahaya;
                warna = popupWarnaBahaya;
                break;
        }

        StartCoroutine(AnimasiPopup(teks, warna, popupAnchorX, popupAnchorY));
    }

    /// Tampilkan popup nyawa berkurang.
    public void ShowLifeLostPopup()
    {
        StartCoroutine(AnimasiPopup(
            popupTeksNyawa, popupWarnaNyawa,
            popupAnchorX, popupAnchorY + popupLifeLostOffsetY));
    }

    IEnumerator AnimasiPopup(string teks, Color warna, float anchorX, float anchorY)
    {
        // Pastikan canvas popup ada
        if (_popupCanvas == null)
        {
            var cgo = new GameObject("[PopupCanvas]");
            DontDestroyOnLoad(cgo);
            _popupCanvas = cgo.AddComponent<Canvas>();
            _popupCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            _popupCanvas.sortingOrder = 950;
            var sc = cgo.AddComponent<CanvasScaler>();
            sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1920f, 1080f);
            sc.matchWidthOrHeight  = 0.5f;
        }

        // Buat label popup
        var go  = new GameObject("Popup");
        go.transform.SetParent(_popupCanvas.transform, false);
        var rt  = go.AddComponent<RectTransform>();
        rt.anchorMin        = rt.anchorMax = new Vector2(anchorX, anchorY);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = new Vector2(300f, 70f);
        rt.anchoredPosition = Vector2.zero;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        ApplyFont(tmp);
        tmp.text               = teks;
        tmp.fontSize           = popupFontSize;
        tmp.fontStyle          = FontStyles.Bold;
        tmp.color              = warna;
        tmp.alignment          = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;

        // Outline tipis agar terbaca di atas background apapun
        tmp.outlineWidth = 0.25f;
        tmp.outlineColor = new Color(0f, 0f, 0f, 0.80f);

        // Animasi: naik + fade out
        Vector2 posAwal = rt.anchoredPosition;

        for (float t = 0f; t < popupDuration; t += Time.deltaTime)
        {
            float p = t / popupDuration;
            rt.anchoredPosition = posAwal + Vector2.up * (popupRisePixels * p);
            // Tetap penuh 60% pertama, fade di 40% terakhir
            float alpha = p < 0.6f ? 1f : Mathf.Lerp(1f, 0f, (p - 0.6f) / 0.4f);
            tmp.color = new Color(warna.r, warna.g, warna.b, alpha);
            yield return null;
        }

        Destroy(go);
    }

    // ══════════════════════════════════════════════════════════════════════
    // BUILD NAVBAR
    // ══════════════════════════════════════════════════════════════════════

    void BuildHUD()
    {
        // Pastikan sprite bersama sudah dibuat
        if (_sCircle    == null) _sCircle    = GenCircle(64);
        if (_sRoundRect == null) _sRoundRect = GenRoundedRect(128, 64, 16);

        // Canvas navbar
        var cGO = new GameObject("HUDCanvas_Navbar");
        DontDestroyOnLoad(cGO);
        var cv = cGO.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 900;
        var sc = cGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920f, 1080f);
        sc.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // ── KIRI: Skor + Hati ─────────────────────────────────────────────
        var left = Panel(cGO, "NavLeft", panelBgColor,
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(14f, -12f), new Vector2(340f, 66f));

        // Teks skor
        _rScore = Tmp(left, "Score", "Skor: 0", 26, Color.white);
        _rScore.alignment = TextAlignmentOptions.MidlineLeft;
        _rScore.fontStyle = FontStyles.Bold;
        var scoreRT = _rScore.rectTransform;
        scoreRT.anchorMin = new Vector2(0f,    0.08f);
        scoreRT.anchorMax = new Vector2(0.50f, 0.92f);
        scoreRT.offsetMin = new Vector2(14f, 0f);
        scoreRT.offsetMax = Vector2.zero;

        // Tiga hati
        _rHearts = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            float x0 = 0.54f + i * 0.155f;
            var hRT  = Rect(left, "Heart" + i,
                new Vector2(x0, 0.15f), new Vector2(x0 + 0.14f, 0.85f));
            var hImg = hRT.gameObject.AddComponent<Image>();
            hImg.sprite         = heartFullSprite != null ? heartFullSprite : _sCircle;
            hImg.color          = heartFullSprite != null ? Color.white
                                                          : new Color(0.92f, 0.18f, 0.18f, 1f);
            hImg.preserveAspect = true;
            _rHearts[i]         = hImg;
        }

        // ── TENGAH: Progress Hari (H1 → H2 → H3) ─────────────────────────
        var center = Panel(cGO, "NavCenter", new Color(0, 0, 0, 0),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -6f), new Vector2(360f, 92f));

        float[] nx = { Mathf.Clamp01(dayCircleX1), Mathf.Clamp01(dayCircleX2), Mathf.Clamp01(dayCircleX3) };
        _dayCircles = new Image[3];
        _dayNums    = new TextMeshProUGUI[3];
        _dayLabels  = new TextMeshProUGUI[3];
        _dayLines   = new Image[2];

        // Garis konektor (di belakang lingkaran, dibuat lebih dulu)
        // Tebal garis dihitung dari dayLineThickness (px) → fraksi tinggi panel (96px)
        float lineHalfFrac = Mathf.Clamp(dayLineThickness / 96f, 0.02f, 0.30f) * 0.5f;
        float lineCenterY  = 0.66f;
        for (int i = 0; i < 2; i++)
        {
            var lineRT  = Rect(center, "Line" + i,
                new Vector2(nx[i] + 0.11f, lineCenterY - lineHalfFrac),
                new Vector2(nx[i + 1] - 0.11f, lineCenterY + lineHalfFrac));
            var lineImg = lineRT.gameObject.AddComponent<Image>();
            lineImg.color = dayInactiveColor;
            _dayLines[i]  = lineImg;
        }

        // Lingkaran hari + nomor + label
        for (int i = 0; i < 3; i++)
        {
            // Lingkaran (point anchor + sizeDelta agar bulat sempurna)
            var circGO = new GameObject("Circle" + i);
            circGO.transform.SetParent(center.transform, false);
            var circRT = circGO.AddComponent<RectTransform>();
            circRT.anchorMin        = circRT.anchorMax = new Vector2(nx[i], 0.72f);
            circRT.pivot            = new Vector2(0.5f, 0.5f);
            circRT.sizeDelta        = new Vector2(dayCircleSize, dayCircleSize);
            circRT.anchoredPosition = Vector2.zero;
            var circImg   = circGO.AddComponent<Image>();
            circImg.sprite = _sCircle;
            circImg.color  = (i == 0) ? dayActiveColor : dayInactiveColor;
            _dayCircles[i] = circImg;

            // Nomor dalam lingkaran
            var numTMP = Tmp(circGO, "Num", (i + 1).ToString(), dayNumFontSize,
                (i == 0) ? dayNumColorActive : dayNumColorInactive);
            numTMP.alignment = TextAlignmentOptions.Center;
            numTMP.fontStyle = FontStyles.Bold;
            numTMP.rectTransform.anchorMin = Vector2.zero;
            numTMP.rectTransform.anchorMax = Vector2.one;
            numTMP.rectTransform.offsetMin = numTMP.rectTransform.offsetMax = Vector2.zero;
            _dayNums[i] = numTMP;

            // Label "H1/H2/H3" di bawah lingkaran — teks bisa dikustom via dayLabels[]
            string lbl = (dayLabels != null && i < dayLabels.Length && !string.IsNullOrEmpty(dayLabels[i]))
                         ? dayLabels[i] : ("H" + (i + 1));
            var lblTMP = Tmp(center, "Label" + i, lbl, dayLabelFontSize,
                (i == 0) ? dayActiveColor : dayLabelColorInactive);
            lblTMP.alignment = TextAlignmentOptions.Center;
            lblTMP.fontStyle = FontStyles.Bold;
            var lblRT = lblTMP.rectTransform;
            lblRT.anchorMin = new Vector2(nx[i] - 0.10f, 0.00f);
            lblRT.anchorMax = new Vector2(nx[i] + 0.10f, 0.36f);
            lblRT.offsetMin = lblRT.offsetMax = Vector2.zero;
            _dayLabels[i] = lblTMP;
        }

        // ── KANAN: Voice Meter (fill bar + marker) ─────────────────────────
        // Panel lebih lebar agar bar cukup besar
        var right = Panel(cGO, "NavRight", panelBgColor,
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-14f, -12f), new Vector2(340f, 66f));

        // Label "📢 SUARA" di kiri panel
        var gaugeLabel = Tmp(right, "GaugeLabel", "📢 SUARA", 16,
            new Color(1f, 1f, 1f, 0.72f));
        gaugeLabel.alignment = TextAlignmentOptions.MidlineLeft;
        var glRT = gaugeLabel.rectTransform;
        glRT.anchorMin = new Vector2(0f, 0.42f);
        glRT.anchorMax = new Vector2(0.22f, 1.00f);
        glRT.offsetMin = new Vector2(10f, 0f);
        glRT.offsetMax = Vector2.zero;

        // Label level (Diam / Normal / Sedang / TERIAK!) di bawah label SUARA
        _gaugeLevelLabel = Tmp(right, "LevelLabel", "Diam", 13,
            new Color(0.55f, 0.55f, 0.60f, 1f));
        _gaugeLevelLabel.alignment = TextAlignmentOptions.MidlineLeft;
        var llRT = _gaugeLevelLabel.rectTransform;
        llRT.anchorMin = new Vector2(0f,    0.00f);
        llRT.anchorMax = new Vector2(0.22f, 0.42f);
        llRT.offsetMin = new Vector2(10f, 0f);
        llRT.offsetMax = Vector2.zero;

        // ── Background bar (latar gelap, isi penuh dari kiri bar) ──────
        var barBg = new GameObject("GaugeBg");
        barBg.transform.SetParent(right.transform, false);
        var barBgRT = barBg.AddComponent<RectTransform>();
        barBgRT.anchorMin = new Vector2(0.23f, 0.16f);
        barBgRT.anchorMax = new Vector2(0.98f, 0.84f);
        barBgRT.offsetMin = barBgRT.offsetMax = Vector2.zero;
        _gaugeFillBgRT = barBgRT;
        var barBgImg = barBg.AddComponent<Image>();
        barBgImg.color = new Color(0.08f, 0.08f, 0.10f, 1f);

        // ── 3 zona background (selalu terlihat, opacity rendah) ─────────
        float[] zBounds = { 0f, ZN_NORMAL_END, ZN_MEDIUM_END, 1f };
        Color[] zOff = {
            new Color(0.10f, 0.45f, 0.20f, 0.40f),   // hijau redup
            new Color(0.60f, 0.40f, 0.04f, 0.40f),   // kuning redup
            new Color(0.55f, 0.10f, 0.08f, 0.40f),   // merah redup
        };
        _gaugeZones = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var zGO = new GameObject("ZoneBg" + i);
            zGO.transform.SetParent(barBgRT, false);
            var zRT = zGO.AddComponent<RectTransform>();
            zRT.anchorMin = new Vector2(zBounds[i],     0f);
            zRT.anchorMax = new Vector2(zBounds[i + 1], 1f);
            zRT.offsetMin = zRT.offsetMax = Vector2.zero;
            _gaugeZones[i]       = zGO.AddComponent<Image>();
            _gaugeZones[i].color = zOff[i];
        }

        // ── Fill bar (tumbuh dari kiri, warna berubah sesuai level) ────
        var fillGO = new GameObject("GaugeFill");
        fillGO.transform.SetParent(barBgRT, false);
        _gaugeFillRT = fillGO.AddComponent<RectTransform>();
        _gaugeFillRT.anchorMin = Vector2.zero;
        _gaugeFillRT.anchorMax = new Vector2(0.04f, 1f);   // minimal 4% agar selalu terlihat
        _gaugeFillRT.offsetMin = _gaugeFillRT.offsetMax = Vector2.zero;
        _gaugeFillImg = fillGO.AddComponent<Image>();
        _gaugeFillImg.color = new Color(0.20f, 0.72f, 0.38f, 1f);   // hijau awal

        // ── Marker merah — garis vertikal di posisi threshold Loud ─────
        var markerGO = new GameObject("GaugeMarker");
        markerGO.transform.SetParent(barBgRT, false);
        _gaugeMarkerRT = markerGO.AddComponent<RectTransform>();
        _gaugeMarkerRT.anchorMin = new Vector2(ZN_MEDIUM_END - 0.012f, -0.20f);
        _gaugeMarkerRT.anchorMax = new Vector2(ZN_MEDIUM_END + 0.012f,  1.20f);
        _gaugeMarkerRT.offsetMin = _gaugeMarkerRT.offsetMax = Vector2.zero;
        var markerImg = markerGO.AddComponent<Image>();
        markerImg.color = new Color(0.92f, 0.15f, 0.15f, 1f);

    }

    // ══════════════════════════════════════════════════════════════════════
    // BUILD INTRO SCREEN
    // ══════════════════════════════════════════════════════════════════════

    void BuildIntroUI()
    {
        var introGO = new GameObject("DayIntroCanvas");
        DontDestroyOnLoad(introGO);
        var iCv = introGO.AddComponent<Canvas>();
        iCv.renderMode   = RenderMode.ScreenSpaceOverlay;
        iCv.sortingOrder = 970;
        var iSc = introGO.AddComponent<CanvasScaler>();
        iSc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        iSc.referenceResolution = new Vector2(1920f, 1080f);
        iSc.matchWidthOrHeight  = 0.5f;
        introGO.AddComponent<GraphicRaycaster>();

        _introGroup                = introGO.AddComponent<CanvasGroup>();
        _introGroup.alpha          = 0f;
        _introGroup.blocksRaycasts = false;
        introGO.SetActive(false);

        // ── Overlay gelap penuh layar ─────────────────────────────────────
        var overlayGO = new GameObject("Overlay");
        overlayGO.transform.SetParent(introGO.transform, false);
        var overlayRT = overlayGO.AddComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.offsetMin = overlayRT.offsetMax = Vector2.zero;
        var overlayImg  = overlayGO.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0.04f, 0.82f);

        // ── Garis dekoratif atas ─────────────────────────────────────────
        var lineTop = new GameObject("LineTop");
        lineTop.transform.SetParent(introGO.transform, false);
        var ltRT = lineTop.AddComponent<RectTransform>();
        ltRT.anchorMin = new Vector2(0.08f, 0.68f);
        ltRT.anchorMax = new Vector2(0.92f, 0.685f);
        ltRT.offsetMin = ltRT.offsetMax = Vector2.zero;
        var ltImg = lineTop.AddComponent<Image>();
        ltImg.color = new Color(0.95f, 0.78f, 0.10f, 0.75f);

        // ── Garis dekoratif bawah ─────────────────────────────────────────
        var lineBot = new GameObject("LineBot");
        lineBot.transform.SetParent(introGO.transform, false);
        var lbRT = lineBot.AddComponent<RectTransform>();
        lbRT.anchorMin = new Vector2(0.08f, 0.295f);
        lbRT.anchorMax = new Vector2(0.92f, 0.300f);
        lbRT.offsetMin = lbRT.offsetMax = Vector2.zero;
        var lbImg = lineBot.AddComponent<Image>();
        lbImg.color = new Color(0.95f, 0.78f, 0.10f, 0.75f);

        // ── Judul — teks besar kuning bold ────────────────────────────────
        _introTitle                    = Tmp(introGO, "IntroTitle",
                                             "HARI 1: Jalan Kaki ke Sekolah", 72,
                                             new Color(0.96f, 0.80f, 0.12f, 1f));
        _introTitle.alignment          = TextAlignmentOptions.Center;
        _introTitle.fontStyle          = FontStyles.Bold;
        _introTitle.enableWordWrapping = true;
        var tRT = _introTitle.rectTransform;
        tRT.anchorMin = new Vector2(0.05f, 0.44f);
        tRT.anchorMax = new Vector2(0.95f, 0.68f);
        tRT.offsetMin = tRT.offsetMax = Vector2.zero;

        // ── Sub-judul — lokasi putih ──────────────────────────────────────
        _introSub                    = Tmp(introGO, "IntroSub",
                                           "📍  Jalan Menuju Sekolah", 34, Color.white);
        _introSub.alignment          = TextAlignmentOptions.Center;
        _introSub.enableWordWrapping = true;
        var sRT = _introSub.rectTransform;
        sRT.anchorMin = new Vector2(0.10f, 0.31f);
        sRT.anchorMax = new Vector2(0.90f, 0.44f);
        sRT.offsetMin = sRT.offsetMax = Vector2.zero;

        // ── Hint kecil di bawah ───────────────────────────────────────────
        var hintTmp = Tmp(introGO, "IntroHint", "Bersiaplah...", 22,
                          new Color(1f, 1f, 1f, 0.45f));
        hintTmp.alignment = TextAlignmentOptions.Center;
        var hRT = hintTmp.rectTransform;
        hRT.anchorMin = new Vector2(0.15f, 0.22f);
        hRT.anchorMax = new Vector2(0.85f, 0.30f);
        hRT.offsetMin = hRT.offsetMax = Vector2.zero;
    }

    IEnumerator PlayIntroCoroutine(System.Action onComplete = null)
    {
        if (_introGroup == null) yield break;
        _introGroup.gameObject.SetActive(true);

        // Fade in
        for (float t = 0f; t < introFadeDuration; t += Time.deltaTime)
        {
            _introGroup.alpha = t / introFadeDuration;
            yield return null;
        }
        _introGroup.alpha = 1f;

        yield return new WaitForSeconds(introDuration);

        // Fade out
        for (float t = 0f; t < introFadeDuration; t += Time.deltaTime)
        {
            _introGroup.alpha = 1f - t / introFadeDuration;
            yield return null;
        }
        _introGroup.alpha = 0f;
        _introGroup.gameObject.SetActive(false);

        // Panggil callback setelah animasi selesai
        onComplete?.Invoke();
    }

    // ══════════════════════════════════════════════════════════════════════
    // HELPERS
    // ══════════════════════════════════════════════════════════════════════

    string DayTitle(int day)
    {
        switch (day)
        {
            case 1:  return "HARI 1: Jalan Kaki ke Sekolah";
            case 2:  return "HARI 2: Naik Angkot";
            case 3:  return "HARI 3: Parkiran Sekolah";
            default: return "HARI " + day;
        }
    }

    // Panel dengan background warna (support rounded rect via 9-slice)
    GameObject Panel(GameObject parent, string name, Color bg,
        Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = rt.anchorMax = anchor;
        rt.pivot            = pivot;
        rt.sizeDelta        = size;
        rt.anchoredPosition = pos;
        if (bg.a > 0.01f)
        {
            var img    = go.AddComponent<Image>();
            img.sprite = _sRoundRect;
            img.type   = Image.Type.Sliced;
            img.color  = bg;

            // Border emas tipis agar selaras tema kayu/sunset
            var ol = go.AddComponent<Outline>();
            ol.effectColor    = panelBorderColor;
            ol.effectDistance = new Vector2(2f, -2f);
        }
        return go;
    }

    // RectTransform kosong (stretch anchor)
    RectTransform Rect(GameObject parent, string name,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }

    // TextMeshProUGUI
    TextMeshProUGUI Tmp(GameObject parent, string name,
        string text, int size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        ApplyFont(tmp);
        tmp.text               = text;
        tmp.fontSize           = size;
        tmp.color              = color;
        tmp.enableWordWrapping = false;
        tmp.overflowMode       = TextOverflowModes.Overflow;
        return tmp;
    }

    void ApplyFont(TextMeshProUGUI tmp)
    {
        TMP_FontAsset f = fontAsset;
        if (f == null) f = TMP_Settings.defaultFontAsset;
        if (f == null) f = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (f != null) tmp.font = f;
    }

    IEnumerator FlashImage(Image img)
    {
        if (img == null) yield break;
        Color orig = img.color;
        for (int i = 0; i < 3; i++)
        {
            img.color = new Color(1f, 0.3f, 0.3f, orig.a);
            yield return new WaitForSeconds(0.15f);
            img.color = orig;
            yield return new WaitForSeconds(0.15f);
        }
    }

    // Buat sprite lingkaran anti-aliased di runtime
    static Sprite GenCircle(int res)
    {
        var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        float r = res * 0.5f;
        for (int y = 0; y < res; y++)
            for (int x = 0; x < res; x++)
            {
                float dx = x - r + 0.5f, dy = y - r + 0.5f;
                float a  = Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
    }

    // Buat sprite rounded rectangle untuk 9-slice panel
    static Sprite GenRoundedRect(int w, int h, int r)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                // Jarak dari pixel ke titik terdekat di persegi "inner" (inset r pixel)
                float cx   = Mathf.Clamp(x, r, w - r - 1);
                float cy   = Mathf.Clamp(y, r, h - r - 1);
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                float a    = Mathf.Clamp01((float)r - dist + 0.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        // Border untuk 9-slice = r pixel di tiap sisi
        return Sprite.Create(tex,
            new Rect(0, 0, w, h),
            new Vector2(0.5f, 0.5f),
            100f, 0,
            SpriteMeshType.FullRect,
            new Vector4(r, r, r, r));
    }
}
