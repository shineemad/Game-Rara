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

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb             = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical   = Input.GetAxisRaw("Vertical");

        Vector2 direction = new Vector2(horizontal, vertical).normalized;
        bool    isMoving  = direction.magnitude > 0f;
        bool    isRunning = isMoving && Input.GetKey(KeyCode.LeftShift);

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

        // ── gerak fisika ────────────────────────────────────────────
        float speed  = isRunning ? runSpeed : moveSpeed;
        rb.velocity  = direction * speed;

        // ── flip sprite ─────────────────────────────────────────────
        if      (horizontal > 0f) spriteRenderer.flipX = false;
        else if (horizontal < 0f) spriteRenderer.flipX = true;

        // ── animasi ─────────────────────────────────────────────────
        switch (currentState)
        {
            case MoveState.Idle:
                spriteRenderer.sprite = idleSprite != null
                    ? idleSprite
                    : (walkSprites.Length > 0 ? walkSprites[0] : null);
                break;

            case MoveState.Walk:
                AdvanceAnimation(walkSprites, walkFrameDuration);
                break;

            case MoveState.Run:
                AdvanceAnimation(runSprites, runFrameDuration);
                break;
        }
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
