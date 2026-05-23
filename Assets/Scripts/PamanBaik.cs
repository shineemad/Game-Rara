using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PamanBaik — NPC yang berjalan otomatis dengan animasi sprite.
///
/// Setup di Inspector:
///   walkSprites      → drag sprite dari Assets/sprites/Paman Baik/ (5 frame)
///   walkSpeed        → kecepatan berjalan (unit/detik)
///   frameDuration    → durasi tiap frame animasi (detik)
///   direction        → arah jalan: 1 = kanan, -1 = kiri
///   patrolMode       → aktifkan agar paman bolak-balik antara patrol kiri & kanan
///   patrolLeft/Right → batas patrol
///
///   playerTarget     → drag GameObject Rara ke sini
///   triggerDistance  → jarak agar paman mulai maju (mendekati Rara)
///   stopDistance     → jarak berhenti (sudah cukup dekat)
/// </summary>
public class PamanBaik : MonoBehaviour
{
    [Header("Animasi Jalan")]
    public Sprite[] walkSprites;
    public float    frameDuration = 0.12f;

    [Header("Pergerakan Otomatis")]
    public float walkSpeed = 2f;
    [Tooltip("1 = kanan, -1 = kiri")]
    public float direction = -1f;

    [Header("Patrol (opsional)")]
    public bool  patrolMode  = false;
    public float patrolLeft  = -5f;
    public float patrolRight =  5f;

    [Header("Deteksi Jarak ke Player")]
    [Tooltip("Drag GameObject Rara ke sini")]
    public Transform playerTarget;
    [Tooltip("Jarak dari Rara agar paman mulai bergerak")]
    public float triggerDistance = 6f;
    [Tooltip("Jarak berhenti — sudah sampai di tujuan")]
    public float stopDistance    = 1.0f;

    [Header("Posisi Tujuan Akhir Paman")]
    [Tooltip("Drag empty GameObject ke sini sebagai titik berhenti paman.\nKosongkan = otomatis di kanan Rara (pakai frontOffset).")]
    public Transform stopPoint;
    [Tooltip("Dipakai hanya jika stopPoint kosong: jarak ke kanan Rara")]
    public float frontOffset = 1.5f;

    [Header("Bayangan")]
    [Tooltip("Aktifkan bayangan otomatis")]
    public bool  shadowEnabled   = true;
    public Color shadowColor     = new Color(0f, 0f, 0f, 0.35f);
    public float shadowScaleX    = 0.85f;
    public float shadowScaleY    = 0.15f;
    public Vector2 sunDirection  = new Vector2(1f, 1f);
    public float shadowLength    = 0.6f;
    public float shadowExtraOffsetY = -0.05f;

    // ── internal ──────────────────────────────────────────────────────────
    private SpriteRenderer sr;
    private float          frameTimer;
    private int            currentFrame;
    private bool           isMoving;
    private float          targetWorldHeight = -1f;
    private Vector2        moveTarget;
    private SpriteShadow   shadow;               // referensi komponen shadow

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        // Coba temukan player otomatis jika belum di-assign
        if (playerTarget == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) playerTarget = p.transform;
        }

        // Rekam tinggi dunia frame pertama sebagai referensi ukuran
        if (walkSprites != null && walkSprites.Length > 0 && walkSprites[0] != null)
            targetWorldHeight = walkSprites[0].bounds.size.y * transform.localScale.y;
    }

    void Start()
    {
        // Setup shadow di Start agar sr.sprite & sorting layer sudah siap
        SetupShadow();
    }

    void SetupShadow()
    {
        if (!shadowEnabled || sr == null) return;

        // Pastikan paman punya sprite awal dari walkSprites
        if (sr.sprite == null && walkSprites != null && walkSprites.Length > 0)
            sr.sprite = walkSprites[0];

        // Cari child Shadow yang sudah ada
        Transform shadowTf = transform.Find("Shadow");
        if (shadowTf == null)
        {
            var go = new GameObject("Shadow");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            shadowTf = go.transform;
        }

        // Pastikan ada SpriteRenderer di Shadow
        var shadowSR = shadowTf.GetComponent<SpriteRenderer>();
        if (shadowSR == null) shadowSR = shadowTf.gameObject.AddComponent<SpriteRenderer>();

        // Salin sorting layer dari paman, order satu di bawahnya
        shadowSR.sortingLayerName = sr.sortingLayerName;
        shadowSR.sortingOrder     = sr.sortingOrder - 1;

        // Langsung sync sprite awal agar langsung terlihat
        shadowSR.sprite = sr.sprite;
        shadowSR.flipX  = sr.flipX;

        // Pastikan ada komponen SpriteShadow
        shadow = shadowTf.GetComponent<SpriteShadow>();
        if (shadow == null) shadow = shadowTf.gameObject.AddComponent<SpriteShadow>();

        // Terapkan semua setting
        shadow.characterRenderer = sr;
        shadow.shadowColor       = shadowColor;
        shadow.scaleX            = shadowScaleX;
        shadow.scaleY            = shadowScaleY;
        shadow.sunDirection      = sunDirection;
        shadow.shadowLength      = shadowLength;
        shadow.extraOffsetY      = shadowExtraOffsetY;
        shadow.enabled           = true;
    }

    void Update()
    {
        UpdateMoveState();
        UpdateAnimation();
        MoveCharacter();
        SyncShadowSettings();
    }

    // ── Sync setting shadow tiap frame agar perubahan Inspector langsung tampil ──
    void SyncShadowSettings()
    {
        if (shadow == null) return;
        shadow.shadowColor  = shadowColor;
        shadow.scaleX       = shadowScaleX;
        shadow.scaleY       = shadowScaleY;
        shadow.sunDirection = sunDirection;
        shadow.shadowLength = shadowLength;
        shadow.extraOffsetY = shadowExtraOffsetY;
        shadow.enabled      = shadowEnabled;
    }

    // ── Cek jarak ke player, hitung target kanan Rara ────────────────────
    void UpdateMoveState()
    {
        if (playerTarget == null)
        {
            isMoving = walkSpeed > 0f;
            return;
        }

        if (stopPoint != null)
        {
            // Mode stopPoint: paman berjalan ke titik yang sudah ditentukan.
            // Trigger = Rara mendekati AREA STOPPOINT (bukan paman)
            moveTarget = new Vector2(stopPoint.position.x, stopPoint.position.y);

            float distRaraToStop = Vector2.Distance(playerTarget.position, stopPoint.position);
            float distToTarget   = Vector2.Distance(transform.position, moveTarget);

            if (distRaraToStop <= triggerDistance && distToTarget > stopDistance)
            {
                isMoving  = true;
                direction = (moveTarget.x > transform.position.x) ? 1f : -1f;
            }
            else
            {
                isMoving = false;
            }
        }
        else
        {
            // Mode otomatis: paman menuju kanan Rara, trigger = Rara mendekati paman
            moveTarget = new Vector2(playerTarget.position.x + frontOffset, playerTarget.position.y);

            float distToRara   = Vector2.Distance(transform.position, playerTarget.position);
            float distToTarget = Vector2.Distance(transform.position, moveTarget);

            if (distToRara <= triggerDistance && distToTarget > stopDistance)
            {
                isMoving  = true;
                direction = (moveTarget.x > transform.position.x) ? 1f : -1f;
            }
            else
            {
                isMoving = false;
            }
        }
    }

    // ── Animasi looping ───────────────────────────────────────────────────
    void UpdateAnimation()
    {
        if (walkSprites == null || walkSprites.Length == 0) return;

        // Selalu update arah hadap berdasarkan direction
        sr.flipX = direction > 0f;

        if (isMoving)
        {
            frameTimer += Time.deltaTime;
            if (frameTimer >= frameDuration)
            {
                frameTimer   = 0f;
                currentFrame = (currentFrame + 1) % walkSprites.Length;
            }
            sr.sprite = walkSprites[currentFrame];
        }
        else
        {
            // Saat diam: tampilkan frame pertama, reset timer & frame
            frameTimer   = 0f;
            currentFrame = 0;
            sr.sprite    = walkSprites[0];
        }

        // Pastikan skala tinggi tetap konsisten antar frame
        NormalizeScale();
    }

    // ── Kunci tinggi agar tidak berubah antar frame ───────────────────────
    void NormalizeScale()
    {
        if (targetWorldHeight <= 0f || sr.sprite == null) return;

        float spriteH = sr.sprite.bounds.size.y;
        if (spriteH <= 0f) return;

        float ratio = targetWorldHeight / spriteH;
        Vector3 s   = transform.localScale;
        transform.localScale = new Vector3(s.x > 0f ? ratio : -ratio, ratio, s.z);
    }

    // ── Gerak fisik (serong menuju titik depan Rara) ──────────────────────
    void MoveCharacter()
    {
        if (!isMoving || walkSpeed <= 0f) return;

        if (playerTarget != null)
        {
            // Gerak 2D serong ke moveTarget
            Vector2 toTarget = moveTarget - (Vector2)transform.position;
            Vector2 step     = toTarget.normalized * walkSpeed * Time.deltaTime;

            // Jangan overshoot: clamp step agar tidak melewati target
            if (step.magnitude > toTarget.magnitude)
                step = toTarget;

            transform.position += new Vector3(step.x, step.y, 0f);
        }
        else
        {
            // Tidak ada target → jalan lurus (patrol)
            transform.position += new Vector3(direction * walkSpeed * Time.deltaTime, 0f, 0f);

            if (patrolMode)
            {
                if (direction > 0f && transform.position.x >= patrolRight)
                    direction = -1f;
                else if (direction < 0f && transform.position.x <= patrolLeft)
                    direction =  1f;
            }
        }
    }

    // ── Public: hentikan / lanjutkan jalan ────────────────────────────────
    public void SetWalking(bool active)
    {
        enabled = active;
    }

    // ── Gizmo: visualisasi jarak di Scene view ────────────────────────────
    void OnDrawGizmosSelected()
    {
        // Lingkaran kuning = triggerDistance (dari paman)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);

        // Lingkaran merah = stopDistance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        // Titik hijau = posisi tujuan paman (depan Rara)
        if (playerTarget != null)
        {
            var   rSR  = playerTarget.GetComponent<SpriteRenderer>();
            float fDir = (rSR != null && rSR.flipX) ? -1f : 1f;
            Vector3 front = playerTarget.position + new Vector3(fDir * frontOffset, 0f, 0f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(front, 0.25f);
            Gizmos.DrawLine(playerTarget.position, front);
        }
    }

// ══════════════════════════════════════════════════════════════════════
// EDITOR HELPER
// ══════════════════════════════════════════════════════════════════════
#if UNITY_EDITOR
    [ContextMenu("Load Walk Sprites dari Paman Baik")]
    void EditorLoadSprites()
    {
        string folder = "Assets/sprites/Paman Baik";
        var guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite", new[] { folder });

        var list = new List<Sprite>();
        foreach (var guid in guids)
        {
            string path   = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var    sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) list.Add(sprite);
        }

        // Urutkan berdasarkan angka di nama file (1, 2, 3, 4, 5)
        list.Sort((a, b) =>
        {
            int na = ExtractNumber(a.name);
            int nb = ExtractNumber(b.name);
            return na.CompareTo(nb);
        });

        walkSprites = list.ToArray();
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[PamanBaik] walkSprites ter-assign: {walkSprites.Length} frame dari '{folder}'");
    }

    static int ExtractNumber(string s)
    {
        var match = System.Text.RegularExpressions.Regex.Match(s, @"\d+");
        return match.Success ? int.Parse(match.Value) : 0;
    }
#endif
}
