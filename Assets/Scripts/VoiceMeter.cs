using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Singleton yang mengelola input mikrofon dan mengubahnya menjadi level suara.
///
/// Level suara:
///   Silent  → tidak ada suara terdeteksi
///   Normal  → suara biasa  (50-60 dB)  → karakter jalan normal
///   Medium  → suara sedang (60-80 dB)  → karakter lambat / ragu-ragu
///   Loud    → suara keras  (>80 dB)    → speed boost + NPC lari ketakutan
///
/// Cara kerja:
///   Mikrofon HP/PC merekam audio terus-menerus (loop clip).
///   Setiap frame, RMS dari window sampel terakhir dihitung,
///   di-smooth, lalu dipetakan ke salah satu VoiceLevel di atas.
///
/// Fallback:
///   Jika tidak ada mikrofon (atau permission ditolak), gunakan SpaceBar / tombol TERIAK.
///
/// Setup di scene:
///   Tidak perlu setup manual — VoiceMeter di-spawn otomatis via RuntimeInitializeOnLoadMethod.
///   Sesuaikan threshold di Inspector pada GameObject [VoiceMeter].
/// </summary>
public class VoiceMeter : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // SINGLETON
    // ══════════════════════════════════════════════════════════════════════

    public static VoiceMeter Instance { get; private set; }

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR
    // ══════════════════════════════════════════════════════════════════════

    [Header("Ambang Batas Level (amplitudo RMS 0–1)")]
    [Tooltip("Amplitudo minimum untuk dianggap 'suara normal' (hijau ~50-60 dB)")]
    [Range(0f, 1f)]
    public float thresholdNormal = 0.015f;

    [Tooltip("Amplitudo minimum untuk dianggap 'suara sedang' (kuning ~60-80 dB)")]
    [Range(0f, 1f)]
    public float thresholdMedium = 0.07f;

    [Tooltip("Amplitudo minimum untuk dianggap 'suara keras / teriak' (merah >80 dB)")]
    [Range(0f, 1f)]
    public float thresholdLoud   = 0.22f;

    [Header("Pengaturan Mikrofon")]
    [Tooltip("Panjang buffer rekaman mikrofon dalam detik")]
    public int  clipLengthSec = 2;
    [Tooltip("Sample rate rekaman")]
    public int  sampleRate    = 22050;
    [Tooltip("Jumlah sampel yang dibaca per frame untuk hitung RMS")]
    public int  sampleWindow  = 512;

    [Header("Smoothing (0 = lambat, 1 = instan)")]
    [Range(0.01f, 1f)]
    [Tooltip("Kecepatan naik level (respons cepat ke suara)")]
    public float smoothUp   = 0.35f;
    [Range(0.01f, 1f)]
    [Tooltip("Kecepatan turun level (seberapa cepat 'fade out')")]
    public float smoothDown = 0.12f;

    [Header("Fallback — tanpa mikrofon")]
    [Tooltip("Jika mic tidak tersedia, gunakan SpaceBar sebagai pengganti teriak")]
    public bool useFallback = true;

    // ══════════════════════════════════════════════════════════════════════
    // ENUM LEVEL
    // ══════════════════════════════════════════════════════════════════════

    public enum VoiceLevel
    {
        Silent,   // tidak ada suara
        Normal,   // suara biasa (hijau)  → jalan normal
        Medium,   // suara sedang (kuning) → lambat / ragu
        Loud      // teriak keras (merah)  → speed boost + usir NPC
    }

    // ══════════════════════════════════════════════════════════════════════
    // PROPERTIES PUBLIK
    // ══════════════════════════════════════════════════════════════════════

    /// Amplitudo RMS yang sudah di-smooth (0–1)
    public float      NormalizedLevel  { get; private set; }

    /// Level diskret berdasarkan threshold yang dikonfigurasi
    public VoiceLevel Level            { get; private set; } = VoiceLevel.Silent;

    /// true jika mikrofon fisik sedang aktif
    public bool       MicActive        { get; private set; }

    /// Estimasi dB (skala relatif, bukan dB absolut SPL).
    /// Silent ≈ 0, Normal ≈ 50-60, Loud ≈ 80+
    public float CurrentDB =>
        NormalizedLevel > 0.0001f
            ? Mathf.Clamp(20f * Mathf.Log10(NormalizedLevel) + 80f, 0f, 120f)
            : 0f;

    /// Intensitas khusus zona LOUD (0 saat tidak teriak, 0–1 dalam zona merah)
    public float LoudIntensity =>
        Level == VoiceLevel.Loud
            ? Mathf.Clamp01((NormalizedLevel - thresholdLoud) / (1f - thresholdLoud))
            : 0f;

    /// Event dipanggil setiap kali VoiceLevel berubah
    [HideInInspector]
    public UnityEvent<VoiceLevel> onLevelChanged = new UnityEvent<VoiceLevel>();

    // ══════════════════════════════════════════════════════════════════════
    // WARNA PER LEVEL (dipakai HUDManager untuk warnai gauge)
    // ══════════════════════════════════════════════════════════════════════

    public static readonly Color ColorSilent = new Color(0.30f, 0.30f, 0.34f, 1f);
    public static readonly Color ColorNormal = new Color(0.15f, 0.68f, 0.38f, 1f);  // hijau
    public static readonly Color ColorMedium = new Color(0.95f, 0.62f, 0.07f, 1f);  // kuning
    public static readonly Color ColorLoud   = new Color(0.91f, 0.25f, 0.20f, 1f);  // merah

    // ══════════════════════════════════════════════════════════════════════
    // PRIVATE
    // ══════════════════════════════════════════════════════════════════════

    AudioClip  _micClip;
    string     _micDevice;
    float      _rawRMS;
    float      _smoothedRMS;
    VoiceLevel _prevLevel = VoiceLevel.Silent;

    // ── Auto-spawn: muncul otomatis tanpa perlu ditaruh di scene ──────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoSpawn()
    {
        if (Instance != null) return;
        var go = new GameObject("[VoiceMeter]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<VoiceMeter>();
    }

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        StartCoroutine(InitMicrophone());
    }

    // ══════════════════════════════════════════════════════════════════════
    // INISIALISASI MIKROFON
    // ══════════════════════════════════════════════════════════════════════

    IEnumerator InitMicrophone()
    {
        // Request izin di Android/iOS
#if UNITY_ANDROID || UNITY_IOS
        yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            Debug.LogWarning("[VoiceMeter] Izin mikrofon ditolak — fallback ke tombol aktif.");
            MicActive = false;
            yield break;
        }
#else
        yield return null;
#endif
        AktifkanMikrofon();
    }

    void AktifkanMikrofon()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("[VoiceMeter] Tidak ada mikrofon — fallback ke tombol aktif.");
            MicActive = false;
            return;
        }

        _micDevice = Microphone.devices[0];
        _micClip   = Microphone.Start(_micDevice, /*loop=*/true, clipLengthSec, sampleRate);
        MicActive  = true;
        Debug.Log($"[VoiceMeter] Mikrofon aktif: {_micDevice}");
    }

    void OnDestroy()
    {
        if (MicActive && !string.IsNullOrEmpty(_micDevice))
            Microphone.End(_micDevice);
    }

    // ══════════════════════════════════════════════════════════════════════
    // UPDATE TIAP FRAME
    // ══════════════════════════════════════════════════════════════════════

    void Update()
    {
        // Tombol / SpaceBar selalu override mic — beri prioritas tertinggi
        bool buttonOverride = useFallback && (fallbackButtonHeld || Input.GetKey(KeyCode.Space));

        if (buttonOverride)
            _rawRMS = thresholdLoud + 0.20f;   // langsung Loud saat tombol/SpaceBar ditekan
        else if (MicActive && _micClip != null)
            _rawRMS = HitungMicRMS();
        else if (useFallback)
            _rawRMS = HitungFallbackRMS();
        else
            _rawRMS = 0f;

        // Smooth: naik cepat (responsif saat teriak), turun lebih lambat
        float kecepatan = _rawRMS > _smoothedRMS ? smoothUp : smoothDown;
        _smoothedRMS    = Mathf.Lerp(_smoothedRMS, _rawRMS, kecepatan);
        NormalizedLevel = Mathf.Clamp01(_smoothedRMS);

        // Petakan ke level diskret
        VoiceLevel levelBaru;
        if      (NormalizedLevel >= thresholdLoud)   levelBaru = VoiceLevel.Loud;
        else if (NormalizedLevel >= thresholdMedium) levelBaru = VoiceLevel.Medium;
        else if (NormalizedLevel >= thresholdNormal) levelBaru = VoiceLevel.Normal;
        else                                         levelBaru = VoiceLevel.Silent;

        Level = levelBaru;

        // Panggil event hanya saat level berubah
        if (levelBaru != _prevLevel)
        {
            _prevLevel = levelBaru;
            onLevelChanged?.Invoke(levelBaru);
            LogLevelChange(levelBaru);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // HITUNG RMS DARI MIKROFON
    // ══════════════════════════════════════════════════════════════════════

    float HitungMicRMS()
    {
        int posisi = Microphone.GetPosition(_micDevice);
        if (posisi < sampleWindow) return 0f;

        var sampel = new float[sampleWindow];
        int mulai  = posisi - sampleWindow;
        _micClip.GetData(sampel, Mathf.Max(0, mulai));

        float sumKuadrat = 0f;
        foreach (float s in sampel) sumKuadrat += s * s;
        return Mathf.Sqrt(sumKuadrat / sampel.Length);
    }

    // ══════════════════════════════════════════════════════════════════════
    // FALLBACK: SpaceBar atau tombol TERIAK (dari Day1Controller)
    // ══════════════════════════════════════════════════════════════════════

    // Fallback diperkuat dari luar oleh Day1Controller saat tombol TERIAK ditekan
    [HideInInspector] public bool fallbackButtonHeld = false;

    float HitungFallbackRMS()
    {
        bool spaceHeld = Input.GetKey(KeyCode.Space);
        if (spaceHeld || fallbackButtonHeld)
            // Simulasikan teriak penuh → langsung Loud
            return thresholdLoud + 0.15f;
        return 0f;
    }

    // ══════════════════════════════════════════════════════════════════════
    // HELPER PUBLIK
    // ══════════════════════════════════════════════════════════════════════

    public bool IsLoud()   => Level == VoiceLevel.Loud;
    public bool IsMedium() => Level == VoiceLevel.Medium;
    public bool IsNormal() => Level == VoiceLevel.Normal;
    public bool IsSilent() => Level == VoiceLevel.Silent;

    /// Warna UI untuk level saat ini
    public Color WarnaSaatIni()
    {
        switch (Level)
        {
            case VoiceLevel.Normal: return ColorNormal;
            case VoiceLevel.Medium: return ColorMedium;
            case VoiceLevel.Loud:   return ColorLoud;
            default:                return ColorSilent;
        }
    }

    /// Nama level dalam Bahasa Indonesia
    public string NamaLevel()
    {
        switch (Level)
        {
            case VoiceLevel.Normal: return "TENANG";
            case VoiceLevel.Medium: return "SEDANG";
            case VoiceLevel.Loud:   return "TERIAK!";
            default:                return "DIAM";
        }
    }

    // ══════════════════════════════════════════════════════════════════════

    void LogLevelChange(VoiceLevel level)
    {
        string tanda = level switch
        {
            VoiceLevel.Normal => "[Hijau]",
            VoiceLevel.Medium => "[Kuning]",
            VoiceLevel.Loud   => "[MERAH]",
            _                 => "[Diam]"
        };
        Debug.Log($"[VoiceMeter] {tanda} {NamaLevel()} — RMS={NormalizedLevel:F3} dB≈{CurrentDB:F0}");
    }
}
