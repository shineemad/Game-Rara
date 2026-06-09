using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BackgroundLayerSetup — atur semua layer latar dari SATU Inspector.
///
/// ══════════════════════════════════════════════════
/// CARA SETUP (Sekali saja):
///   1. Buat Empty GameObject, rename "BackgroundManager".
///   2. Tambah komponen BackgroundLayerSetup ke GO tersebut.
///   3. Drag Main Camera ke field "Kamera Utama".
///   4. Klik "+" di array "Layers" untuk tiap layer latar.
///   5. Untuk tiap layer isi:
///        • Nama          — untuk label di Inspector
///        • Sprite        — drag sprite background-nya
///        • Pivot (X, Y)  — offset posisi dari kamera (X: kiri-kanan, Y: atas-bawah)
///        • Z / Depth     — misal: langit = 10, gedung = 6, jalan = 2
///        • Scale         — ukuran sprite
///        • Parallax X    — 0 = diam, 0.1–0.3 = pelan, 1 = ikut kamera
///        • Lock To Camera— true = diam di layar (langit/clouds), false = parallax
///        • Sorting Order — urutan gambar (lebih besar = lebih depan)
///   6. Klik kanan komponen → "Re-Generate Layers" atau Play.
/// ══════════════════════════════════════════════════
/// </summary>
[ExecuteInEditMode]
public class BackgroundLayerSetup : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // DATA LAYER
    // ══════════════════════════════════════════════════════════════════════

    [System.Serializable]
    public class BgLayer
    {
        [Tooltip("Label layer — hanya untuk identifikasi di Inspector")]
        public string nama = "Layer Baru";

        [Tooltip("Sprite latar untuk layer ini")]
        public Sprite sprite;

        [Header("Posisi & Ukuran")]
        [Tooltip("Offset posisi X dari pusat kamera")]
        public float offsetX = 0f;

        [Tooltip("Offset posisi Y dari pusat kamera")]
        public float offsetY = 0f;

        [Tooltip("Kedalaman (Z) — gunakan angka positif. Langit: 10 | Gedung: 6 | Jalan: 2")]
        public float depth = 5f;

        [Tooltip("Skala sprite (1 = ukuran asli)")]
        public Vector2 scale = Vector2.one;

        [Header("Parallax")]
        [Tooltip("TRUE = layer selalu diam di layar (langit, awan). FALSE = gunakan Parallax Factor.")]
        public bool lockToCamera = true;

        [Range(0f, 1f)]
        [Tooltip("Seberapa cepat layer bergerak. 0 = diam di dunia | 0.5 = setengah | 1 = ikut kamera")]
        public float parallaxX = 0f;

        [Range(0f, 1f)]
        public float parallaxY = 0f;

        [Tooltip("Background berulang (tile) saat karakter jalan jauh")]
        public bool infiniteScroll = false;

        [Header("Render")]
        [Tooltip("Urutan render. Lebih besar = lebih depan. Langit:0 | Gedung:5 | Pagar:8 | Jalan:10")]
        public int sortingOrder = 0;

        [Tooltip("Warna/tint sprite layer ini")]
        public Color tint = Color.white;

        // Internal — referensi GO yang dibuat
        [HideInInspector] public GameObject generatedGO;
    }

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR
    // ══════════════════════════════════════════════════════════════════════

    [Header("── KAMERA ──")]
    [Tooltip("Drag Main Camera ke sini (auto-detect jika kosong)")]
    public Camera mainCamera;

    [Header("── LAYERS LATAR ──")]
    [Tooltip("Tambah layer dengan tombol +. Urutkan dari belakang (Z besar) ke depan (Z kecil).")]
    public List<BgLayer> layers = new List<BgLayer>();

    [Header("── OPSI GENERATE ──")]
    [Tooltip("Hapus GO lama & buat ulang semua layer saat tombol Generate ditekan atau Play.")]
    public bool autoGenerateOnPlay = true;

    [Tooltip("Prefix nama GameObject yang dibuat untuk setiap layer")]
    public string goPrefix = "BG_";

    // ══════════════════════════════════════════════════════════════════════
    void Start()
    {
        if (!Application.isPlaying) return;

        if (mainCamera == null) mainCamera = Camera.main;

        if (autoGenerateOnPlay)
            GenerateLayers();
    }

    void LateUpdate()
    {
        if (!Application.isPlaying) return;
        if (mainCamera == null) return;

        // Update posisi tiap layer sesuai parallax / lock setting
        foreach (var layer in layers)
        {
            if (layer.generatedGO == null) continue;

            var pb = layer.generatedGO.GetComponent<ParallaxBackground>();
            if (pb != null)
            {
                // Nilai sudah dikelola ParallaxBackground.cs
                // Hanya update offset jika diubah saat runtime
                pb.cameraOffset   = new Vector3(layer.offsetX, layer.offsetY, layer.depth);
                pb.lockToCamera   = layer.lockToCamera;
                pb.parallaxFactorX = layer.parallaxX;
                pb.parallaxFactorY = layer.parallaxY;
                pb.infiniteScroll = layer.infiniteScroll;
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // GENERATE LAYERS
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Hapus GO lama lalu buat ulang semua layer berdasarkan konfigurasi Inspector.
    /// Dipanggil otomatis saat Play (jika autoGenerateOnPlay = true),
    /// atau bisa dipanggil manual via Context Menu di Inspector.
    /// </summary>
    [ContextMenu("⟳ Re-Generate Semua Layer")]
    public void GenerateLayers()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // Hapus GO yang pernah dibuat sebelumnya
        foreach (var layer in layers)
        {
            if (layer.generatedGO != null)
                DestroyImmediate(layer.generatedGO);
        }

        // Hapus juga child GO lama dengan prefix yang sama (dari sesi Play sebelumnya)
        var toDelete = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith(goPrefix))
                toDelete.Add(child);
        }
        foreach (var t in toDelete) DestroyImmediate(t.gameObject);

        // Buat GO baru untuk tiap layer
        for (int i = 0; i < layers.Count; i++)
        {
            BgLayer layer = layers[i];
            if (layer.sprite == null) continue;

            // Buat GO
            var go = new GameObject($"{goPrefix}{i:00}_{layer.nama}");
            go.transform.SetParent(transform, false);

            // SpriteRenderer
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = layer.sprite;
            sr.sortingOrder = layer.sortingOrder;
            sr.color        = layer.tint;

            // Posisi awal
            go.transform.position = new Vector3(
                mainCamera.transform.position.x + layer.offsetX,
                mainCamera.transform.position.y + layer.offsetY,
                layer.depth
            );
            go.transform.localScale = new Vector3(layer.scale.x, layer.scale.y, 1f);

            // Komponen ParallaxBackground
            var pb = go.AddComponent<ParallaxBackground>();
            pb.cam            = mainCamera;
            pb.lockToCamera   = layer.lockToCamera;
            pb.parallaxFactorX = layer.parallaxX;
            pb.parallaxFactorY = layer.parallaxY;
            pb.infiniteScroll = layer.infiniteScroll;
            pb.cameraOffset   = new Vector3(layer.offsetX, layer.offsetY, layer.depth);

            layer.generatedGO = go;
        }

        Debug.Log($"[BackgroundLayerSetup] {layers.Count} layer berhasil dibuat.");
    }

    /// <summary>
    /// Perbarui properti visual (sprite, skala, warna, sorting) tanpa buat ulang GO.
    /// Berguna saat kamu ganti sprite atau warna di Inspector saat Edit Mode.
    /// </summary>
    [ContextMenu("↺ Refresh Visual (tanpa buat ulang GO)")]
    public void RefreshVisuals()
    {
        for (int i = 0; i < layers.Count; i++)
        {
            BgLayer layer = layers[i];
            if (layer.generatedGO == null) continue;

            var sr = layer.generatedGO.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite       = layer.sprite;
                sr.sortingOrder = layer.sortingOrder;
                sr.color        = layer.tint;
            }

            layer.generatedGO.transform.localScale = new Vector3(layer.scale.x, layer.scale.y, 1f);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // GIZMOS — tampilkan label layer di Scene View
    // ══════════════════════════════════════════════════════════════════════
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (mainCamera == null) return;

        for (int i = 0; i < layers.Count; i++)
        {
            var layer = layers[i];
            Vector3 worldPos = new Vector3(
                mainCamera.transform.position.x + layer.offsetX,
                mainCamera.transform.position.y + layer.offsetY,
                layer.depth
            );

            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.5f);
            Gizmos.DrawWireCube(worldPos, new Vector3(layer.scale.x * 2f, layer.scale.y, 0.1f));

            UnityEditor.Handles.Label(worldPos + Vector3.up * 0.5f,
                $"[{i}] {layer.nama}  Z={layer.depth}  Srt={layer.sortingOrder}");
        }
    }
#endif
}
