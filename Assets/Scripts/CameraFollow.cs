using UnityEngine;

/// <summary>
/// Kamera mengikuti karakter Rara dengan efek smooth (lerp).
/// Letakkan script ini pada Main Camera.
///
/// Setup di Inspector:
///   target       → Transform karakter Rara
///   smoothSpeed  → kelembutan kamera (0=instan, 1=tidak bergerak; default 0.12)
///   offset       → geser posisi kamera relatif terhadap Rara (default Y+1 Z-10)
///   useBounds    → aktifkan batas dunia (agar kamera tidak keluar peta)
///   minX/maxX    → batas kiri-kanan dunia (aktif jika useBounds = true)
///   minY/maxY    → batas atas-bawah dunia  (aktif jika useBounds = true)
///   lookAheadX   → kamera sedikit "lihat ke depan" arah gerak Rara
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;            // drag Rara ke sini

    [Header("Smooth")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.12f;   // makin kecil = makin smooth/lambat

    [Header("Offset Kamera")]
    public Vector3 offset = new Vector3(0f, 3.6f, -10f);

    [Header("Look Ahead (kamera lihat ke depan arah jalan)")]
    public bool  useLookAhead    = false;
    public float lookAheadAmount = 1.5f;    // unit ke depan
    public float lookAheadSpeed  = 3f;      // seberapa cepat bergeser

    [Header("Batas Dunia (World Bounds)")]
    public bool  useBounds = true;
    public float minX      = -13f;
    public float maxX      = 65f;
    public float minY      = -5f;
    public float maxY      = 8f;

    // ── internal ──────────────────────────────────────────────────────────
    private float     currentLookAhead;
    private float     lastTargetX;
    private Camera    cam;

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        if (target == null)
        {
            // Coba temukan otomatis jika belum di-assign
            var p = GameObject.FindWithTag("Player");
            if (p != null) target = p.transform;
        }

        if (target != null)
        {
            // Langsung snap ke posisi awal tanpa animasi
            SnapToTarget();
            lastTargetX = target.position.x;
        }
    }

    // ── Gunakan LateUpdate agar karakter sudah selesai bergerak ───────────
    void LateUpdate()
    {
        if (target == null) return;

        // ── Look Ahead ─────────────────────────────────────────────────
        float targetLookAhead = 0f;
        if (useLookAhead)
        {
            float moveDir = target.position.x - lastTargetX;
            if (Mathf.Abs(moveDir) > 0.01f)
                targetLookAhead = Mathf.Sign(moveDir) * lookAheadAmount;
            currentLookAhead = Mathf.Lerp(currentLookAhead, targetLookAhead, lookAheadSpeed * Time.deltaTime);
        }
        lastTargetX = target.position.x;

        // ── Target posisi kamera ────────────────────────────────────────
        Vector3 desired = new Vector3(
            target.position.x + offset.x + currentLookAhead,
            target.position.y + offset.y,
            offset.z
        );

        // ── Smooth lerp ─────────────────────────────────────────────────
        Vector3 smoothed = Vector3.Lerp(transform.position, desired, smoothSpeed);

        // ── Clamp ke batas dunia ────────────────────────────────────────
        if (useBounds && cam != null)
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;

            smoothed.x = Mathf.Clamp(smoothed.x, minX + halfW, maxX - halfW);
            smoothed.y = Mathf.Clamp(smoothed.y, minY + halfH, maxY - halfH);
        }

        transform.position = smoothed;
    }

    // ══════════════════════════════════════════════════════════════════════
    /// Langsung pindah ke posisi target tanpa lerp (pakai saat start/respawn).
    public void SnapToTarget()
    {
        if (target == null) return;
        transform.position = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            offset.z
        );
    }

    // ── Gizmo batas dunia di Scene view ───────────────────────────────────
    void OnDrawGizmosSelected()
    {
        if (!useBounds) return;
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0f);
        Vector3 size   = new Vector3(maxX - minX, maxY - minY, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}
