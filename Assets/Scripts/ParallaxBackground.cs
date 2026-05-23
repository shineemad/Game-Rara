using UnityEngine;

/// <summary>
/// Parallax Background — background mengikuti kamera sebagian atau diam total.
///
/// MODE PENGGUNAAN:
///   lockToCamera = true  → background selalu mengikuti kamera (diam di layar),
///                          cocok untuk langit / latar paling belakang
///   lockToCamera = false → parallax biasa, background bergerak lambat sesuai
///                          parallaxFactorX (0 = diam di dunia, 1 = ikut penuh)
///
/// Setup di Inspector:
///   cam              → Main Camera (auto-detect jika kosong)
///   lockToCamera     → TRUE = background DIAM di layar (recommended untuk bg utama)
///   parallaxFactorX  → hanya aktif jika lockToCamera = false
///                      0 = diam di world | 0.1-0.3 = bergerak pelan
///   infiniteScroll   → background berulang saat karakter berjalan jauh
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("Referensi Kamera")]
    public Camera cam;

    [Header("Mode Latar")]
    [Tooltip("TRUE = background diam di layar (ikut kamera sepenuhnya).\n" +
             "Cocok untuk langit dan latar paling belakang seperti gambar kamu.")]
    public bool lockToCamera = true;

    [Header("Parallax (aktif hanya jika Lock To Camera = false)")]
    [Range(0f, 1f)]
    [Tooltip("0 = diam di dunia | 0.1 = pelan | 1 = ikut kamera penuh")]
    public float parallaxFactorX = 0f;

    [Range(0f, 1f)]
    public float parallaxFactorY = 0f;

    [Header("Offset dari Kamera (saat Lock To Camera aktif)")]
    [Tooltip("Geser posisi background relatif terhadap pusat kamera")]
    public Vector3 cameraOffset = new Vector3(0f, 0f, 1f);

    [Header("Infinite Scroll")]
    [Tooltip("Background berulang tanpa ujung saat karakter berjalan jauh.\n" +
             "Aktifkan jika level lebih lebar dari sprite background.")]
    public bool infiniteScroll = false;

    // ── Internal ──────────────────────────────────────────────────────────
    private Vector3 lastCamPos;
    private float   spriteWidth;

    // ══════════════════════════════════════════════════════════════════════
    void Start()
    {
        if (cam == null) cam = Camera.main;

        lastCamPos = cam.transform.position;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
            spriteWidth = sr.sprite.bounds.size.x * transform.localScale.x;

        if (lockToCamera)
            SnapToCamera();
    }

    void LateUpdate()
    {
        if (cam == null) return;

        if (lockToCamera)
        {
            // Background selalu di tengah kamera → tidak bergerak di layar
            SnapToCamera();
        }
        else
        {
            // Parallax biasa: geser sebagian delta kamera
            Vector3 delta = cam.transform.position - lastCamPos;
            transform.position += new Vector3(
                delta.x * parallaxFactorX,
                delta.y * parallaxFactorY,
                0f
            );
        }

        lastCamPos = cam.transform.position;

        // ── Infinite Scroll ────────────────────────────────────────────
        if (infiniteScroll && spriteWidth > 0f)
        {
            float dist = cam.transform.position.x - transform.position.x;
            if (dist >  spriteWidth) transform.position += new Vector3( spriteWidth, 0f, 0f);
            if (dist < -spriteWidth) transform.position -= new Vector3( spriteWidth, 0f, 0f);
        }
    }

    void SnapToCamera()
    {
        transform.position = new Vector3(
            cam.transform.position.x + cameraOffset.x,
            cam.transform.position.y + cameraOffset.y,
            transform.position.z
        );
    }

    void OnDrawGizmosSelected()
    {
        if (!infiniteScroll || spriteWidth <= 0f) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            transform.position + Vector3.left  * spriteWidth,
            transform.position + Vector3.right * spriteWidth
        );
    }
}
