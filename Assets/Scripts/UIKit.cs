using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// UIPalette — SUMBER TUNGGAL warna kategori pilihan (AMAN/RAGU/BAHAYA/Netral)
/// beserta ikon penanda aksesibilitas. Dipakai bersama oleh DialogManager dkk
/// agar tidak ada drift warna antar layar.
///
/// Nilai warna mengikuti CLAUDE.md:
///   AMAN  #26AD61 (hijau) · RAGU #F29D12 (kuning) ·
///   BAHAYA #E84D3D (merah) · Netral #339FDB (biru)
/// </summary>
public static class UIPalette
{
    public static readonly Color Aman   = new Color(0.149f, 0.678f, 0.380f, 1f); // #26AD61
    public static readonly Color Ragu   = new Color(0.949f, 0.616f, 0.071f, 1f); // #F29D12
    public static readonly Color Bahaya = new Color(0.910f, 0.302f, 0.239f, 1f); // #E84D3D
    public static readonly Color Netral = new Color(0.200f, 0.624f, 0.859f, 1f); // #339FDB

    /// Warna untuk kategori pilihan. Default → Netral (biru).
    public static Color Kategori(string kategori) => kategori switch
    {
        "AMAN"   => Aman,
        "RAGU"   => Ragu,
        "BAHAYA" => Bahaya,
        _        => Netral
    };

    /// Ikon penanda kategori — agar tidak hanya bergantung warna
    /// (aksesibilitas untuk pemain buta warna). ✓ aman, ! ragu, ✕ bahaya.
    public static string KategoriIkon(string kategori) => kategori switch
    {
        "AMAN"   => "\u2713", // ✓
        "RAGU"   => "\u0021", // !
        "BAHAYA" => "\u2715", // ✕
        _        => "\u2022"  // •
    };
}

/// <summary>
/// UIKit — utilitas pembuatan UI bersama: sprite rounded 9-slice tunggal dan
/// factory Canvas responsif (selalu Expand → tidak ada UI terpotong).
/// </summary>
public static class UIKit
{
    private static Sprite _roundedShared;

    /// Sprite kotak sudut-membulat 9-slice bersama (di-cache sekali).
    /// Menggantikan banyak Texture2D rounded yang digenerate per-layar.
    public static Sprite RoundedSprite()
    {
        if (_roundedShared != null) return _roundedShared;

        const int size = 64, radius = 14;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode   = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        Color32 putih = new Color32(255, 255, 255, 255);
        Color32 kosong = new Color32(255, 255, 255, 0);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                bool inside = true;
                if      (x < radius && y < radius)             { int dx = radius - x, dy = radius - y; inside = dx*dx + dy*dy <= radius*radius; }
                else if (x >= size-radius && y < radius)       { int dx = x-(size-1-radius), dy = radius - y; inside = dx*dx + dy*dy <= radius*radius; }
                else if (x < radius && y >= size-radius)       { int dx = radius - x, dy = y-(size-1-radius); inside = dx*dx + dy*dy <= radius*radius; }
                else if (x >= size-radius && y >= size-radius) { int dx = x-(size-1-radius), dy = y-(size-1-radius); inside = dx*dx + dy*dy <= radius*radius; }
                tex.SetPixel(x, y, inside ? (Color)putih : (Color)kosong);
            }
        tex.Apply();
        _roundedShared = Sprite.Create(
            tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
            100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        return _roundedShared;
    }

    /// Buat Canvas overlay dengan CanvasScaler responsif yang BENAR sejak awal
    /// (ScaleWithScreenSize + Expand + ref 1920×1080) — tanpa perlu koreksi
    /// belakangan oleh ResponsiveCanvasFixer.
    public static Canvas CreateOverlayCanvas(string nama, int sortingOrder, bool dontDestroy = false)
    {
        var go = new GameObject(nama);
        if (dontDestroy) Object.DontDestroyOnLoad(go);

        var cv = go.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = sortingOrder;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.Expand;
        scaler.matchWidthOrHeight  = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return cv;
    }
}

/// <summary>
/// ButtonPressFeedback — efek tekan kecil (skala mengecil saat ditekan) agar
/// tombol terasa responsif, terutama di layar sentuh. Tambahkan ke GameObject
/// tombol mana pun.
/// </summary>
[DisallowMultipleComponent]
public class ButtonPressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("Skala tombol saat ditekan (1 = tidak berubah).")]
    [Range(0.8f, 1f)] public float pressedScale = 0.94f;

    private Vector3 _normalScale = Vector3.one;
    private bool    _captured;

    void Awake()  => CaptureNormal();
    void OnEnable() { if (_captured) transform.localScale = _normalScale; }

    void CaptureNormal()
    {
        _normalScale = transform.localScale;
        _captured    = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_captured) CaptureNormal();
        transform.localScale = _normalScale * pressedScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = _normalScale;
    }

    void OnDisable()
    {
        if (_captured) transform.localScale = _normalScale;
    }
}
