using UnityEngine;
using TMPro;

/// <summary>
/// Konfigurasi tampilan tombol "LANJUT" gaya Visual Novel — versi Component.
/// Berfungsi sama dengan <see cref="TombolLanjutTema"/> (ScriptableObject)
/// tapi dipasang langsung sebagai komponen pada GameObject di scene
/// (mis. GameObject manager / kanvas dialog). Lebih praktis: pemain bisa
/// custom dari Inspector tanpa perlu membuat asset di folder Resources.
///
/// CARA PAKAI
///   1. Tambahkan komponen "Tombol Lanjut Konfig" ke salah satu GameObject
///      di scene (boleh GameObject kosong baru).
///   2. Atur warna / sprite / ukuran / font / teks lewat Inspector.
///   3. Semua tombol LANJUT otomatis ikut konfigurasi ini.
///
/// PRIORITAS (yang dipakai TombolLanjutVN):
///   1) Komponen ini (TombolLanjutKonfig) jika ada di scene — DISARANKAN.
///   2) Asset Resources/TombolLanjutTema (ScriptableObject) — kompatibilitas.
///   3) Nilai bawaan di kode (transparan penuh, hanya teks).
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("RARA/Tombol Lanjut Konfig")]
public class TombolLanjutKonfig : MonoBehaviour
{
    [Header("Teks")]
    [Tooltip("Kosongkan = pakai teks dari pemanggil (default 'LANJUT  \u25B6').")]
    public string teksOverride = "";
    [Tooltip("Font teks tombol. Kosongkan = font TMP default.")]
    public TMP_FontAsset font;
    public Color warnaTeks = new Color(0.95f, 0.91f, 0.78f, 1f);
    public FontStyles gayaTeks = FontStyles.Bold;
    [Tooltip("Rentang ukuran font (auto-size).")]
    public float fontMin = 16f;
    public float fontMax = 24f;
    public float jarakHuruf = 4f;
    [Tooltip("Padding teks dari tepi tombol (kiri/bawah, lalu kanan/atas).")]
    public Vector2 paddingTeksKiriBawah = new Vector2(22f, 14f);
    public Vector2 paddingTeksKananAtas = new Vector2(22f, 14f);

    [Header("Latar / Sprite")]
    [Tooltip("Sprite kustom untuk badan tombol. Kosongkan = sprite sudut membulat bawaan.")]
    public Sprite sprite;
    [Tooltip("Warna latar tombol. Alpha 0 = transparan penuh (hanya teks tampil).")]
    public Color warnaIsi = new Color(1f, 1f, 1f, 0f);

    [Header("Border (Outline)")]
    public bool pakaiOutline = false;
    public Color warnaOutline = new Color(0.55f, 0.50f, 0.36f, 0.45f);
    public Vector2 jarakOutline = new Vector2(1f, -1f);

    [Header("Efek Tekan")]
    public Color warnaSorot = new Color(1f, 0.98f, 0.88f, 1f);
    public Color warnaTekan = new Color(0.85f, 0.82f, 0.70f, 1f);

    [Header("Ukuran & Posisi (pojok kanan-bawah parent)")]
    public float lebar = 300f;
    public float tinggi = 72f;
    public float marginKanan = 36f;
    public float marginBawah = 28f;

    // ── Internal: konversi ke TombolLanjutTema (di-cache) ────────────────
    // Dipakai TombolLanjutVN agar jalur konfigurasi tetap satu pintu.
    // Membuat ulang field setiap pemanggilan supaya perubahan di Inspector
    // saat Play mode langsung berlaku tanpa restart.
    private TombolLanjutTema _temaRuntime;
    internal TombolLanjutTema KeTema()
    {
        if (_temaRuntime == null)
            _temaRuntime = ScriptableObject.CreateInstance<TombolLanjutTema>();
        _temaRuntime.teksOverride         = teksOverride;
        _temaRuntime.font                 = font;
        _temaRuntime.warnaTeks            = warnaTeks;
        _temaRuntime.gayaTeks             = gayaTeks;
        _temaRuntime.fontMin              = fontMin;
        _temaRuntime.fontMax              = fontMax;
        _temaRuntime.jarakHuruf           = jarakHuruf;
        _temaRuntime.paddingTeksKiriBawah = paddingTeksKiriBawah;
        _temaRuntime.paddingTeksKananAtas = paddingTeksKananAtas;
        _temaRuntime.sprite               = sprite;
        _temaRuntime.warnaIsi             = warnaIsi;
        _temaRuntime.pakaiOutline         = pakaiOutline;
        _temaRuntime.warnaOutline         = warnaOutline;
        _temaRuntime.jarakOutline         = jarakOutline;
        _temaRuntime.warnaSorot           = warnaSorot;
        _temaRuntime.warnaTekan           = warnaTekan;
        _temaRuntime.lebar                = lebar;
        _temaRuntime.tinggi               = tinggi;
        _temaRuntime.marginKanan          = marginKanan;
        _temaRuntime.marginBawah          = marginBawah;
        return _temaRuntime;
    }

    // ── Live-edit di Play mode ────────────────────────────────────────────
    // OnValidate dipanggil setiap kali field di Inspector diubah. Saat Play
    // berlangsung kita beri tahu TombolLanjutVN agar SELURUH tombol yang
    // sedang aktif menerapkan ulang tampilan baru.
    void OnValidate()
    {
        if (!Application.isPlaying) return;
#if UNITY_EDITOR
        // Tunda 1 frame agar nilai terupdate sebelum di-apply (menghindari
        // race saat Unity sedang memvalidasi banyak field).
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            TombolLanjutVN.RefreshSemua();
        };
#else
        TombolLanjutVN.RefreshSemua();
#endif
    }

    void OnEnable()  { if (Application.isPlaying) TombolLanjutVN.RefreshSemua(); }
    void OnDisable() { if (Application.isPlaying) TombolLanjutVN.RefreshSemua(); }
}
