using UnityEngine;
using TMPro;

/// <summary>
/// Tema (pengaturan tampilan) untuk tombol "LANJUT" gaya Visual Novel.
///
/// CARA PAKAI (custom sendiri TANPA edit kode):
///   1. Klik kanan di folder Assets/Resources  →  Create → RARA → Tema Tombol Lanjut.
///   2. Beri nama PERSIS:  TombolLanjutTema   (harus di dalam folder bernama "Resources").
///   3. Atur warna / sprite / ukuran / font / teks di Inspector.
///   4. Semua tombol LANJUT di game otomatis ikut tema ini.
///
/// Kalau asset ini TIDAK ada, tombol memakai nilai bawaan (tampilan saat ini).
/// </summary>
[CreateAssetMenu(menuName = "RARA/Tema Tombol Lanjut", fileName = "TombolLanjutTema")]
public class TombolLanjutTema : ScriptableObject
{
    [Header("Teks")]
    [Tooltip("Kosongkan = pakai teks dari pemanggil (default 'LANJUT  \u25B6').")]
    public string teksOverride = "";
    [Tooltip("Font teks tombol. Kosongkan = font TMP default.")]
    public TMP_FontAsset font;
    public Color warnaTeks = new Color(1f, 0.93f, 0.70f, 1f);
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
    public Color warnaIsi = new Color(0.16f, 0.10f, 0.04f, 0.96f);

    [Header("Border (Outline)")]
    public bool pakaiOutline = true;
    public Color warnaOutline = new Color(0.95f, 0.72f, 0.18f, 0.95f);
    public Vector2 jarakOutline = new Vector2(2f, -2f);

    [Header("Efek Tekan")]
    public Color warnaSorot  = new Color(1f, 0.86f, 0.40f, 1f);
    public Color warnaTekan  = new Color(0.85f, 0.62f, 0.14f, 1f);

    [Header("Ukuran & Posisi (pojok kanan-bawah)")]
    public float lebar  = 300f;
    public float tinggi = 72f;
    public float marginKanan = 36f;
    public float marginBawah = 28f;
}
