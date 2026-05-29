using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BackgroundController — atur semua layer latar yang SUDAH ADA di scene
/// dari satu komponen Inspector.
///
/// ══════════════════════════════════════════════════
/// CARA SETUP:
///   1. Buat Empty GO, rename "BackgroundController".
///   2. Tambah komponen BackgroundController ke GO tersebut.
///   3. Drag Main Camera ke field "Kamera Utama".
///   4. Klik "+" di array "Layer", lalu:
///        • Drag GameObject latar (mis. Pembatas1) ke field "Target GO"
///        • Atur Offset, Scale, Parallax sesuai keinginan
///   5. Klik kanan komponen → "Terapkan Semua Layer" untuk langsung lihat
///      hasilnya di Scene View (tanpa Play).
/// ══════════════════════════════════════════════════
/// </summary>
[ExecuteAlways]
public class BackgroundController : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // DATA PER LAYER
    // ══════════════════════════════════════════════════════════════════════

    [System.Serializable]
    public class LayerSetting
    {
        [Tooltip("Drag GameObject latar yang sudah ada di scene ke sini")]
        public GameObject targetGO;

        [Tooltip("Label — hanya untuk kamu sendiri")]
        public string nama = "Layer";

        [Header("── Posisi ──")]
        [Tooltip("Posisi X di dunia (world space)")]
        public float posX = 0f;

        [Tooltip("Posisi Y di dunia (world space)")]
        public float posY = 0f;

        [Tooltip("Kedalaman Z — lebih besar = lebih belakang. Langit:10 | Gedung:5 | Jalan:1")]
        public float posZ = 5f;

        [Header("── Ukuran ──")]
        public Vector2 skala = Vector2.one;

        [Header("── Parallax ──")]
        [Tooltip("TRUE = diam di layar (ikut kamera). Cocok untuk langit/latar jauh.")]
        public bool lockToCamera = false;

        [Range(0f, 1f)]
        [Tooltip("Kecepatan parallax horizontal. 0 = diam | 0.5 = lambat | 1 = ikut kamera")]
        public float parallaxX = 0f;

        [Range(0f, 1f)]
        public float parallaxY = 0f;

        [Tooltip("Layer berulang (tile) saat pemain berjalan jauh")]
        public bool infiniteScroll = false;

        [Header("── Tampilan ──")]
        [Tooltip("Urutan render. Lebih besar = lebih depan layar.")]
        public int sortingOrder = 0;

        [Tooltip("Warna / tint sprite (putih = normal)")]
        public Color tint = Color.white;
    }

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR
    // ══════════════════════════════════════════════════════════════════════

    [Header("── KAMERA ──")]
    [Tooltip("Drag Main Camera ke sini")]
    public Camera mainCamera;

    [Header("── LAYER LATAR (drag GO dari Hierarchy) ──")]
    public List<LayerSetting> layers = new List<LayerSetting>();

    // ══════════════════════════════════════════════════════════════════════
    // INTERNAL
    // ══════════════════════════════════════════════════════════════════════

    // Posisi kamera sebelumnya — untuk hitung delta parallax
    private Vector3 lastCamPos;

    // ══════════════════════════════════════════════════════════════════════
    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera != null) lastCamPos = mainCamera.transform.position;

        ApplyAll();
    }

    void LateUpdate()
    {
        if (!Application.isPlaying) return;
        if (mainCamera == null) return;

        Vector3 camPos = mainCamera.transform.position;
        Vector3 delta  = camPos - lastCamPos;

        foreach (var layer in layers)
        {
            if (layer.targetGO == null) continue;

            var pb = layer.targetGO.GetComponent<ParallaxBackground>();
            if (pb == null) continue;

            // Sinkronisasi nilai Inspector → ParallaxBackground
            pb.lockToCamera    = layer.lockToCamera;
            pb.parallaxFactorX = layer.parallaxX;
            pb.parallaxFactorY = layer.parallaxY;
            pb.infiniteScroll  = layer.infiniteScroll;
            pb.cameraOffset    = new Vector3(layer.posX - camPos.x,
                                             layer.posY - camPos.y,
                                             layer.posZ);
        }

        lastCamPos = camPos;
    }

    // ══════════════════════════════════════════════════════════════════════
    // TERAPKAN SEMUA — bisa dipanggil dari Context Menu (tanpa Play)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Terapkan posisi, skala, warna, sorting, dan parallax ke semua GO.
    /// Dipanggil otomatis saat Start, atau manual via klik kanan komponen.
    /// </summary>
    [ContextMenu("✔  Terapkan Semua Layer")]
    public void ApplyAll()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        foreach (var layer in layers)
        {
            if (layer.targetGO == null) continue;

            // ── Posisi & Skala ────────────────────────────────────────
            layer.targetGO.transform.position   = new Vector3(layer.posX, layer.posY, layer.posZ);
            layer.targetGO.transform.localScale  = new Vector3(layer.skala.x, layer.skala.y, 1f);

            // ── SpriteRenderer ────────────────────────────────────────
            var sr = layer.targetGO.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = layer.sortingOrder;
                sr.color        = layer.tint;
            }

            // ── ParallaxBackground ────────────────────────────────────
            // Tambah komponen jika belum ada
            var pb = layer.targetGO.GetComponent<ParallaxBackground>();
            if (pb == null && (layer.lockToCamera || layer.parallaxX > 0f || layer.parallaxY > 0f))
                pb = layer.targetGO.AddComponent<ParallaxBackground>();

            if (pb != null)
            {
                pb.cam             = mainCamera;
                pb.lockToCamera    = layer.lockToCamera;
                pb.parallaxFactorX = layer.parallaxX;
                pb.parallaxFactorY = layer.parallaxY;
                pb.infiniteScroll  = layer.infiniteScroll;
                pb.cameraOffset    = new Vector3(0f, 0f, layer.posZ);
            }
        }

#if UNITY_EDITOR
        Debug.Log($"[BackgroundController] {layers.Count} layer diterapkan.");
#endif
    }

    /// <summary>
    /// Baca posisi & skala aktual dari GO yang sudah ada di scene,
    /// lalu masukkan ke array layers. Berguna untuk sinkronisasi awal.
    /// </summary>
    [ContextMenu("↓  Ambil Posisi dari Scene ke Inspector")]
    public void ReadFromScene()
    {
        foreach (var layer in layers)
        {
            if (layer.targetGO == null) continue;

            var pos = layer.targetGO.transform.position;
            var scl = layer.targetGO.transform.localScale;

            layer.posX  = pos.x;
            layer.posY  = pos.y;
            layer.posZ  = pos.z;
            layer.skala = new Vector2(scl.x, scl.y);

            var sr = layer.targetGO.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                layer.sortingOrder = sr.sortingOrder;
                layer.tint         = sr.color;
            }

            var pb = layer.targetGO.GetComponent<ParallaxBackground>();
            if (pb != null)
            {
                layer.lockToCamera = pb.lockToCamera;
                layer.parallaxX    = pb.parallaxFactorX;
                layer.parallaxY    = pb.parallaxFactorY;
                layer.infiniteScroll = pb.infiniteScroll;
            }
        }

#if UNITY_EDITOR
        Debug.Log("[BackgroundController] Posisi berhasil dibaca dari scene.");
#endif
    }

    // ══════════════════════════════════════════════════════════════════════
    // GIZMOS — tampilkan label di Scene View
    // ══════════════════════════════════════════════════════════════════════
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        foreach (var layer in layers)
        {
            if (layer.targetGO == null) continue;

            Vector3 pos = layer.targetGO.transform.position;
            Vector3 scl = layer.targetGO.transform.localScale;

            Gizmos.color = new Color(0.3f, 0.9f, 1f, 0.25f);
            Gizmos.DrawWireCube(pos, new Vector3(Mathf.Abs(scl.x) * 2f, Mathf.Abs(scl.y), 0.05f));

            UnityEditor.Handles.Label(pos + Vector3.up * (Mathf.Abs(scl.y) * 0.5f + 0.3f),
                $"{layer.nama}  |  Z={layer.posZ:F1}  Srt={layer.sortingOrder}");
        }
    }
#endif
}
