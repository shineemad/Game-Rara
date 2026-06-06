using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;       // kecepatan jalan
    public float runSpeed  = 10f;      // kecepatan lari (tahan Shift)

    [Header("Sprites")]
    public Sprite   idleSprite;        // folder: sprites/RARA/IDLE
    public Sprite[] walkSprites;       // folder: sprites/RARA/PNG RARA/walk  (5 frame)
    public Sprite[] runSprites;        // folder: sprites/RARA/RUN   (4 frame)

    [Header("Sprite Takut (Gang Sepi)")]
    [Tooltip("Sprite idle Rara saat ketakutan di gang sepi (1 frame, statis).")]
    public Sprite   idleScaredSprite;
    [Tooltip("Animasi idle takut (mis. gemetar). Kosong = pakai idleScaredSprite statis.")]
    public Sprite[] idleScaredFrames;
    [Tooltip("Durasi per frame animasi takut (detik).")]
    public float    scaredFrameDuration = 0.18f;
    [Tooltip("Animasi jalan saat takut (langkah pelan/gemetar). Kosong = pakai walkSprites biasa.")]
    public Sprite[] walkScaredSprites;
    [Tooltip("Pengali kecepatan saat di mode takut (mis. 0.7 = 70% kecepatan normal).")]
    [Range(0.3f, 1f)] public float scaredSpeedMultiplier = 0.75f;
    [Tooltip("Paksa mode takut aktif (untuk testing). Normalnya auto-aktif saat GameState.pathChoice = 'dangerous'.")]
    public bool     forceScaredMode = false;

    [Header("Animation")]
    public float walkFrameDuration = 0.12f; // detik per frame saat jalan
    public float runFrameDuration  = 0.07f; // detik per frame saat lari (lebih cepat)

    // ── state internal ──────────────────────────────────────────────
    private enum MoveState { Idle, Walk, Run }

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D    rb;

    private MoveState currentState = MoveState.Idle;
    private float     frameTimer;
    private int       currentFrame;

    /// Jika true, karakter berhenti bergerak (dipakai saat dialog aktif).
    /// Set via Day1Controller.FreezePlayer() / ResumePlayer().
    [HideInInspector] public bool frozen = false;

    /// Pengali kecepatan dari Voice Meter:
    ///   1.6f → teriak keras (merah >80dB) — speed boost
    ///   0.55f → suara sedang (kuning 60-80dB) — lambat / ragu
    ///   1.0f → normal / diam
    [HideInInspector] public float voiceSpeedMultiplier = 1f;

    /// Batas pergerakan horizontal player (diset oleh PathEnvironment).
    [HideInInspector] public bool  useBounds = false;
    [HideInInspector] public float boundMinX = -999f;
    [HideInInspector] public float boundMaxX =  999f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb             = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Cek apakah mode takut harus aktif (Rara di gang sepi)
        bool scared = IsScaredModeActive();

        // Saat dialog aktif — hentikan semua pergerakan, tampilkan idle
        if (frozen)
        {
            rb.velocity = Vector2.zero;
            if (currentState != MoveState.Idle)
            {
                currentState = MoveState.Idle;
                currentFrame = 0;
                frameTimer   = 0f;
            }
            if (spriteRenderer != null)
                spriteRenderer.sprite = PilihIdleSprite(scared);
            return;
        }

        // Gabungkan input keyboard + tombol mobile (mana saja yang aktif)
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical   = Input.GetAxisRaw("Vertical");
        bool  shiftHeld  = Input.GetKey(KeyCode.LeftShift);

        if (MobileControls.Instance != null)
        {
            if (Mathf.Abs(MobileControls.Horizontal) > 0f)
                horizontal = MobileControls.Horizontal;
            if (MobileControls.IsRunning) shiftHeld = true;
        }

        Vector2 direction = new Vector2(horizontal, vertical).normalized;
        bool    isMoving  = direction.magnitude > 0f;
        bool    isRunning = isMoving && shiftHeld;

        // ── tentukan state ──────────────────────────────────────────
        MoveState nextState;
        if      (!isMoving)  nextState = MoveState.Idle;
        else if (isRunning)  nextState = MoveState.Run;
        else                 nextState = MoveState.Walk;

        // reset frame saat state berganti agar animasi tidak patah
        if (nextState != currentState)
        {
            currentState = nextState;
            currentFrame = 0;
            frameTimer   = 0f;
        }

        // ── gerak fisika ─────────────────────────────────────────────
        float baseSpeed = isRunning ? runSpeed : moveSpeed;
        if (scared) baseSpeed *= scaredSpeedMultiplier; // di gang sepi → melangkah pelan
        float speed  = baseSpeed * voiceSpeedMultiplier;
        rb.velocity  = direction * speed;

        // ── clamp posisi ke batas jalur ──────────────────────────────
        if (useBounds)
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, boundMinX, boundMaxX);
            transform.position = pos;
        }

        // ── flip sprite ─────────────────────────────────────────────
        if      (horizontal > 0f) spriteRenderer.flipX = false;
        else if (horizontal < 0f) spriteRenderer.flipX = true;

        // ── animasi ─────────────────────────────────────────────────
        switch (currentState)
        {
            case MoveState.Idle:
                // Mode takut: prioritas frame animasi takut → sprite tunggal takut → idle normal
                if (scared && idleScaredFrames != null && idleScaredFrames.Length > 0)
                {
                    AdvanceAnimation(idleScaredFrames, scaredFrameDuration);
                }
                else
                {
                    spriteRenderer.sprite = PilihIdleSprite(scared);
                }
                break;

            case MoveState.Walk:
                // Mode takut + ada walkScaredSprites → pakai animasi takut
                if (scared && walkScaredSprites != null && walkScaredSprites.Length > 0)
                    AdvanceAnimation(walkScaredSprites, walkFrameDuration * 1.2f);
                else
                    AdvanceAnimation(walkSprites, walkFrameDuration);
                break;

            case MoveState.Run:
                AdvanceAnimation(runSprites, runFrameDuration);
                break;
        }
    }

    // ── Helper mode takut ────────────────────────────────

    /// True jika Rara harus tampil takut: di gang sepi (pathChoice = "dangerous") atau force.
    bool IsScaredModeActive()
    {
        if (forceScaredMode) return true;
        if (GameState.Instance == null) return false;
        return GameState.Instance.pathChoice == "dangerous";
    }

    /// Pilih sprite idle yang sesuai mode (takut vs normal).
    Sprite PilihIdleSprite(bool scared)
    {
        if (scared)
        {
            if (idleScaredSprite != null) return idleScaredSprite;
            if (idleScaredFrames != null && idleScaredFrames.Length > 0)
                return idleScaredFrames[0];
        }
        if (idleSprite != null) return idleSprite;
        if (walkSprites != null && walkSprites.Length > 0) return walkSprites[0];
        return null;
    }

    // helper: majukan frame animasi berdasarkan array sprite yang diberikan
    private void AdvanceAnimation(Sprite[] frames, float duration)
    {
        if (frames == null || frames.Length == 0) return;

        frameTimer += Time.deltaTime;
        if (frameTimer >= duration)
        {
            frameTimer   = 0f;
            currentFrame = (currentFrame + 1) % frames.Length;
        }
        spriteRenderer.sprite = frames[currentFrame];
    }

// ══════════════════════════════════════════════════════════════════════
// EDITOR HELPER — klik kanan komponen player di Inspector, pilih menu ini
// ══════════════════════════════════════════════════════════════════════
#if UNITY_EDITOR
    [ContextMenu("Load Walk Sprites dari PNG RARA/walk")]
    void EditorLoadWalkSprites()
    {
        string folder = "Assets/sprites/RARA/PNG RARA/walk";
        var guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite", new[] { folder });

        var list = new List<Sprite>();
        foreach (var guid in guids)
        {
            string path   = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var    sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) list.Add(sprite);
        }

        // Urutkan berdasarkan angka di belakang nama file (walk 1, walk 2, ...)
        list.Sort((a, b) =>
        {
            int na = ExtractNumber(a.name);
            int nb = ExtractNumber(b.name);
            return na.CompareTo(nb);
        });

        walkSprites = list.ToArray();
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[player] walkSprites ter-assign: {walkSprites.Length} frame dari '{folder}'");
    }

    static int ExtractNumber(string s)
    {
        // Ambil angka pertama yang ditemukan di string nama
        var match = System.Text.RegularExpressions.Regex.Match(s, @"\d+");
        return match.Success ? int.Parse(match.Value) : 0;
    }
#endif
}

