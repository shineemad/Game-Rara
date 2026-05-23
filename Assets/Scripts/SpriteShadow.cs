using UnityEngine;

/// <summary>
/// SpriteShadow — Bayangan duplikat sprite untuk karakter 2D.
///
/// Setup di Unity:
///   1. Buat child GameObject bernama "Shadow" di bawah karakter
///   2. Tambah komponen ini pada child "Shadow"
///   3. Atur nilai di Inspector sesuai kebutuhan
///
/// Fitur edit:
///   - [ExecuteAlways] → perubahan langsung terlihat tanpa Play
///   - Handle kuning di Scene view → drag untuk atur arah matahari
///   - Offset manual X/Y untuk posisi fine-tune
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteShadow : MonoBehaviour
{
    [Header("Tampilan Bayangan")]
    [Tooltip("Warna bayangan (alpha = transparansi)")]
    public Color shadowColor = new Color(0f, 0f, 0f, 0.35f);

    [Tooltip("Skala horizontal bayangan")]
    public float scaleX = 0.85f;

    [Tooltip("Skala vertikal bayangan (kecil = lebih gepeng)")]
    public float scaleY = 0.15f;

    [Header("Arah Matahari")]
    [Tooltip("Arah matahari: X positif = kanan, Y positif = atas. Drag handle kuning di Scene view.")]
    public Vector2 sunDirection = new Vector2(1f, 1f);

    [Tooltip("Panjang bayangan (semakin besar = semakin jauh dari kaki)")]
    public float shadowLength = 0.6f;

    [Header("Fine-tune Posisi")]
    [Tooltip("Offset tambahan horizontal (geser kiri-kanan secara manual)")]
    public float extraOffsetX = 0f;

    [Tooltip("Offset tambahan vertikal (geser atas-bawah secara manual)")]
    public float extraOffsetY = -0.05f;

    [Header("Referensi")]
    [Tooltip("SpriteRenderer karakter (parent). Kosongkan = otomatis cari di parent.")]
    public SpriteRenderer characterRenderer;

    // ── internal ──────────────────────────────────────────────────────────
    private SpriteRenderer shadowRenderer;

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        Init();
    }

    void OnEnable()
    {
        Init();
    }

    void Init()
    {
        shadowRenderer = GetComponent<SpriteRenderer>();

        // Selalu prioritaskan SpriteRenderer dari parent —
        // ini mencegah referensi lama (misal Rara) tersimpan di sini
        if (transform.parent != null)
        {
            var parentSR = transform.parent.GetComponent<SpriteRenderer>();
            if (parentSR != null)
                characterRenderer = parentSR;
        }

        if (shadowRenderer != null)
            shadowRenderer.color = shadowColor;

        if (characterRenderer != null && shadowRenderer != null)
            shadowRenderer.sortingOrder = characterRenderer.sortingOrder - 1;
    }

    void LateUpdate()
    {
        if (shadowRenderer == null) Init();
        if (characterRenderer == null || shadowRenderer == null) return;

        // Selalu sinkron warna (agar perubahan di Inspector langsung tampil)
        shadowRenderer.color = shadowColor;

        // Sinkron sprite & arah hadap
        shadowRenderer.sprite = characterRenderer.sprite;
        shadowRenderer.flipX  = characterRenderer.flipX;

        // Proyeksi bayangan berlawanan arah matahari
        Vector2 sunDir  = sunDirection.sqrMagnitude > 0f ? sunDirection.normalized : Vector2.up;
        float   offsetX = -sunDir.x * shadowLength + extraOffsetX;
        float   offsetY = -sunDir.y * shadowLength * scaleY + extraOffsetY;

        transform.localPosition = new Vector3(offsetX, offsetY, 0f);
        transform.localScale    = new Vector3(scaleX, scaleY, 1f);
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// CUSTOM EDITOR — handle visual di Scene view
// ══════════════════════════════════════════════════════════════════════════════
#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(SpriteShadow))]
public class SpriteShadowEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UnityEditor.EditorGUILayout.Space();
        UnityEditor.EditorGUILayout.HelpBox(
            "Drag handle KUNING di Scene view untuk mengatur arah matahari secara visual.",
            UnityEditor.MessageType.Info);
    }

    void OnSceneGUI()
    {
        SpriteShadow shadow = (SpriteShadow)target;
        if (shadow == null) return;

        Transform t        = shadow.transform;
        Vector3   origin   = t.parent != null ? t.parent.position : t.position;
        float     dist     = 1.5f; // panjang gagang handle di Scene view

        Vector2 sunDir     = shadow.sunDirection.sqrMagnitude > 0f
                             ? shadow.sunDirection.normalized
                             : Vector2.up;
        Vector3 handlePos  = origin + new Vector3(sunDir.x, sunDir.y, 0f) * dist;

        // Gambar garis & label
        UnityEditor.Handles.color = new Color(1f, 0.85f, 0f, 0.9f); // kuning
        UnityEditor.Handles.DrawLine(origin, handlePos);
        UnityEditor.Handles.Label(handlePos + Vector3.up * 0.15f, "☀ Matahari");

        // Free-move handle agar bisa di-drag
        UnityEditor.EditorGUI.BeginChangeCheck();
        Vector3 newHandle = UnityEditor.Handles.FreeMoveHandle(
            handlePos,
            0.12f,
            Vector3.zero,
            UnityEditor.Handles.SphereHandleCap);

        if (UnityEditor.EditorGUI.EndChangeCheck())
        {
            UnityEditor.Undo.RecordObject(shadow, "Atur Arah Matahari");
            Vector2 delta = new Vector2(newHandle.x - origin.x, newHandle.y - origin.y);
            shadow.sunDirection = delta.sqrMagnitude > 0f ? delta.normalized : Vector2.up;
            UnityEditor.EditorUtility.SetDirty(shadow);
        }

        // Gambar bayangan preview (garis abu-abu ke arah berlawanan)
        UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.35f);
        Vector3 shadowEnd = origin + new Vector3(-sunDir.x, -sunDir.y * shadow.scaleY, 0f) * shadow.shadowLength;
        UnityEditor.Handles.DrawLine(origin, shadowEnd);
    }
}
#endif


