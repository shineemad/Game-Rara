using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Sprites")]
    public Sprite idleSprite;          // rara-1 dari PNG/idle
    public Sprite[] walkSprites;       // rara-1 s/d rara-5 dari PNG/walk

    [Header("Animation")]
    public float frameDuration = 0.1f; // durasi tiap frame (detik)

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private float frameTimer;
    private int currentFrame;
    private bool isMoving;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        frameTimer = 0f;
        currentFrame = 0;
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("~Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 direction = new Vector2(horizontal, vertical).normalized;
        isMoving = direction.magnitude > 0f;

        // Gerak
        rb.velocity = direction * moveSpeed;

        // Flip sprite kiri / kanan
        if (horizontal > 0f)
            spriteRenderer.flipX = false;
        else if (horizontal < 0f)
            spriteRenderer.flipX = true;

        // Animasi
        if (isMoving)
        {
            frameTimer += Time.deltaTime;
            if (frameTimer >= frameDuration)
            {
                frameTimer = 0f;
                currentFrame = (currentFrame + 1) % walkSprites.Length;
                spriteRenderer.sprite = walkSprites[currentFrame];
            }
        }
        else
        {
            // Diam: pakai idle sprite, reset frame
            frameTimer = 0f;
            currentFrame = 0;
            if (idleSprite != null)
                spriteRenderer.sprite = idleSprite;
            else if (walkSprites.Length > 0)
                spriteRenderer.sprite = walkSprites[0];
        }
    }
}
