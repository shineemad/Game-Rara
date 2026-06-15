using UnityEngine;

/// <summary>
/// SafeAreaFitter — menyesuaikan sebuah panel UI agar berada di dalam
/// "safe area" layar (menghindari poni/notch, kamera punch-hole, dan sudut
/// membulat HP modern serta gesture bar).
///
/// Cara pakai:
///   1. Buat panel (RectTransform) sebagai WADAH isi UI yang TIDAK boleh
///      tertutup notch — mis. baris tombol HUD, tombol kontrol sentuh.
///   2. Tambahkan komponen ini ke panel tersebut.
///   3. Letakkan elemen UI sebagai anak panel ini.
///
/// Catatan: JANGAN pasang pada background full-screen (latar memang sebaiknya
/// memenuhi layar hingga ke belakang notch). Pasang hanya pada lapisan kontrol/HUD.
///
/// Komponen memantau perubahan safe area (rotasi/perangkat) dan menyesuaikan
/// otomatis. Tidak memakai Update untuk kerja berat — hanya cek ringan per frame.
/// </summary>
[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class SafeAreaFitter : MonoBehaviour
{
    [Tooltip("Terapkan inset tepi kiri/kanan (mis. notch di mode landscape).")]
    public bool ikutiHorizontal = true;
    [Tooltip("Terapkan inset tepi atas/bawah (mis. gesture bar / status bar).")]
    public bool ikutiVertikal = true;

    RectTransform _rt;
    Rect _safeAreaTerakhir;
    Vector2Int _ukuranLayarTerakhir;
    ScreenOrientation _orientasiTerakhir;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        Terapkan(force: true);
    }

    void Update()
    {
        // Cek ringan: hanya terapkan ulang bila safe area / ukuran / orientasi berubah.
        if (Screen.safeArea != _safeAreaTerakhir ||
            Screen.width   != _ukuranLayarTerakhir.x ||
            Screen.height  != _ukuranLayarTerakhir.y ||
            Screen.orientation != _orientasiTerakhir)
        {
            Terapkan(force: false);
        }
    }

    void Terapkan(bool force)
    {
        if (_rt == null) _rt = GetComponent<RectTransform>();
        if (_rt == null) return;

        int w = Screen.width;
        int h = Screen.height;
        if (w <= 0 || h <= 0) return;

        Rect safe = Screen.safeArea;

        // Hitung anchor ternormalisasi (0..1) dari safe area dalam piksel layar.
        Vector2 anchorMin = safe.position;
        Vector2 anchorMax = safe.position + safe.size;
        anchorMin.x /= w; anchorMin.y /= h;
        anchorMax.x /= w; anchorMax.y /= h;

        // Hormati sumbu yang dipilih (sisanya tetap penuh 0..1).
        if (!ikutiHorizontal) { anchorMin.x = 0f; anchorMax.x = 1f; }
        if (!ikutiVertikal)   { anchorMin.y = 0f; anchorMax.y = 1f; }

        // Validasi agar tidak menghasilkan nilai aneh.
        if (float.IsNaN(anchorMin.x) || float.IsNaN(anchorMax.x)) return;

        _rt.anchorMin = anchorMin;
        _rt.anchorMax = anchorMax;
        _rt.offsetMin = Vector2.zero;
        _rt.offsetMax = Vector2.zero;

        _safeAreaTerakhir     = safe;
        _ukuranLayarTerakhir  = new Vector2Int(w, h);
        _orientasiTerakhir    = Screen.orientation;
    }
}
