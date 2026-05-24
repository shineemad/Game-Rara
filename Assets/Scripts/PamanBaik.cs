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

    [Header("Sprite Diam (Idle)")]
    [Tooltip("Sprite yang ditampilkan saat paman diam. Kosongkan = pakai walkSprites[0].")]
    public Sprite idleSprite;

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

    [Header("Dialog Saat Tiba")]
    [Tooltip("Komponen NpcDialog yang akan dimainkan ketika paman tiba di samping Rara.\nKosongkan untuk menonaktifkan dialog.")]
    public NpcDialog dialog;
    [Tooltip("Hanya tampilkan dialog satu kali (true) atau setiap kali tiba (false)")]
    public bool dialogOnceOnly = true;
    [Tooltip("Jeda kecil sebelum dialog muncul setelah tiba (detik)")]
    public float dialogDelay = 0.2f;

    // ── internal ──────────────────────────────────────────────────────────
    private SpriteRenderer sr;
    private float          frameTimer;
    private int            currentFrame;
    private bool           isMoving;
    private float          targetWorldHeight = -1f;
    private Vector2        moveTarget;
    private SpriteShadow   shadow;               // referensi komponen shadow
    private bool           hasArrived;            // sudah pernah tiba di target
    private bool           dialogTriggered;       // dialog sudah pernah dimainkan
    private float          arriveTimer;

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

        // Abaikan tabrakan fisik antara Paman ↔ Rara,
        // supaya paman bisa berdiri tepat di sisi Rara tanpa terhalang collider
        if (playerTarget != null)
        {
            var myCols   = GetComponentsInChildren<Collider2D>();
            var raraCols = playerTarget.GetComponentsInChildren<Collider2D>();
            foreach (var a in myCols)
                foreach (var b in raraCols)
                    if (a != null && b != null)
                        Physics2D.IgnoreCollision(a, b, true);
        }
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
        UpdateDialogTrigger();
    }

    // ── Trigger dialog saat paman tiba di samping Rara ───────────────────
    void UpdateDialogTrigger()
    {
        if (dialog == null || playerTarget == null) return;

        // Hitung apakah paman sudah sampai di moveTarget
        float distToTarget = Vector2.Distance(transform.position, moveTarget);
        bool  arrivedNow   = !isMoving && distToTarget <= stopDistance * 1.1f;

        // Reset status jika dialogOnceOnly == false dan paman menjauh lagi
        if (!arrivedNow)
        {
            hasArrived  = false;
            arriveTimer = 0f;
            if (!dialogOnceOnly) dialogTriggered = false;
            return;
        }

        if (hasArrived)
        {
            // Sudah tiba — tunggu jeda lalu mainkan dialog (sekali)
            if (!dialogTriggered)
            {
                arriveTimer += Time.deltaTime;
                if (arriveTimer >= dialogDelay)
                {
                    dialogTriggered = true;
                    if (!dialog.IsPlaying) dialog.Play();
                }
            }
        }
        else
        {
            hasArrived  = true;
            arriveTimer = 0f;
        }
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
            // Mode otomatis: paman selalu berhenti di SISI KANAN Rara
            moveTarget = new Vector2(
                playerTarget.position.x + frontOffset,
                playerTarget.position.y
            );

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
            // Saat diam: tampilkan idleSprite jika ada, kalau tidak pakai frame pertama walk
            frameTimer   = 0f;
            currentFrame = 0;
            sr.sprite    = (idleSprite != null) ? idleSprite : walkSprites[0];
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

        // Hitung target sebenarnya
        Vector3 actualTarget;
        if (stopPoint != null)
        {
            actualTarget = stopPoint.position;
            Gizmos.color = new Color(0f, 0.6f, 1f, 0.5f);
            Gizmos.DrawWireSphere(stopPoint.position, triggerDistance);
        }
        else if (playerTarget != null)
        {
            // Mode otomatis: selalu di kanan Rara
            actualTarget = playerTarget.position + new Vector3(frontOffset, 0f, 0f);
        }
        else return;

        // Lingkaran hijau = target berhenti
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(actualTarget, 0.3f);
        Gizmos.DrawLine(transform.position, actualTarget);
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
