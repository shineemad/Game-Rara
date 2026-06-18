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

    [Header("Reaksi Saat Rara Pilih AMAN (Hijau)")]
    [Tooltip("Centang agar Paman otomatis jalan menjauh dari Rara saat pemain memilih respons AMAN.")]
    public bool aktifPergiAman = true;
    [Tooltip("Jarak (unit dunia) yang ditempuh paman untuk pergi menjauh dari Rara.")]
    public float jarakMenjauh  = 12f;
    [Tooltip("Pengali kecepatan saat paman pergi menjauh (1 = sama dengan walkSpeed).")]
    public float kecepatanMenjauhMul = 1.2f;
    [Tooltip("Sembunyikan GameObject paman setelah sampai tujuan menjauh.")]
    public bool sembunyikanSetelahMenjauh = true;

    [Header("Reaksi Saat Rara Pilih BAHAYA (Merah)")]
    [Tooltip("Centang agar Paman maju sedikit ke arah Rara (intimidasi) saat pemain memilih respons BAHAYA.")]
    public bool aktifMendekatBahaya = true;
    [Tooltip("Jarak (unit dunia) Paman maju ke arah Rara saat BAHAYA dipilih.")]
    public float jarakMendekatBahaya = 0.6f;

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
    // Referensi ke Day1Controller untuk freeze/resume karakter saat dialog
    private Day1Controller day1Controller;
    // Status "pergi menjauh dari Rara" — di-set oleh JalanMenjauh()
    private bool           _pergiMenjauh;
    private float          _arahMenjauh;          // +1 = kanan, -1 = kiri
    private float          _xTargetMenjauh;
    // Status "mendekat Rara" (reaksi BAHAYA) — di-set oleh MendekatRara()
    private bool           _mendekatRara;
    private float          _xTargetMendekat;

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
        // Auto-temukan NpcDialog di GameObject yang sama jika belum di-assign
        if (dialog == null)
            dialog = GetComponent<NpcDialog>();

        // Temukan Day1Controller di scene untuk freeze/resume karakter
        day1Controller = FindFirstObjectByType<Day1Controller>();

        // Auto-subscribe: saat NpcDialog selesai → bebaskan karakter
        if (dialog != null && day1Controller != null)
        {
            // Hapus listener lama dulu agar tidak double-subscribe
            dialog.onDialogEnd.RemoveListener(day1Controller.ResumePlayer);
            dialog.onDialogEnd.AddListener(day1Controller.ResumePlayer);
        }

        // Subscribe ke event pilihan dialog: jika Rara memilih AMAN, paman pergi menjauh.
        if (dialog != null)
        {
            dialog.OnPilihanDipilih -= HandlePilihanDialog; // hindari double
            dialog.OnPilihanDipilih += HandlePilihanDialog;
        }

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

    // ── Trigger dialog saat paman dekat Rara ─────────────────────────────
    void UpdateDialogTrigger()
    {
        if (dialog == null || playerTarget == null) return;

        // Sedang pergi menjauh? jangan mainkan dialog lagi.
        if (_pergiMenjauh) return;
        // Sedang mendekat Rara (reaksi BAHAYA)? jangan picu dialog tambahan.
        if (_mendekatRara) return;
        // Sudah pernah dipicu dan hanya sekali? tidak perlu cek lagi
        if (dialogTriggered && dialogOnceOnly) return;

        // Ukur jarak langsung ke Rara (lebih stabil dari moveTarget yang terus berubah)
        float distToRara = Vector2.Distance(transform.position, playerTarget.position);

        // Anggap "sudah tiba" jika: tidak sedang jalan DAN cukup dekat dengan Rara
        bool arrived = !isMoving && distToRara <= (frontOffset + stopDistance + 0.5f);

        if (arrived)
        {
            if (!hasArrived)
            {
                hasArrived  = true;
                arriveTimer = 0f;
            }

            if (!dialogTriggered)
            {
                arriveTimer += Time.deltaTime;
                if (arriveTimer >= dialogDelay)
                {
                    dialogTriggered = true;
                    // Bekukan karakter Rara selama dialog berlangsung
                    if (day1Controller != null) day1Controller.FreezePlayer();
                    if (!dialog.IsPlaying) dialog.Play();
                }
            }
        }
        else if (distToRara > triggerDistance)
        {
            // Rara menjauh cukup jauh — reset agar dialog bisa muncul lagi
            hasArrived  = false;
            arriveTimer = 0f;
            if (!dialogOnceOnly) dialogTriggered = false;
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

        // ── Mode "mendekat Rara" (reaksi BAHAYA): override target maju ke Rara ──
        if (_mendekatRara)
        {
            moveTarget = new Vector2(_xTargetMendekat, transform.position.y);
            float sisa = Mathf.Abs(transform.position.x - _xTargetMendekat);
            if (sisa > stopDistance)
            {
                isMoving  = true;
                direction = (_xTargetMendekat > transform.position.x) ? 1f : -1f;
            }
            else
            {
                isMoving      = false;
                _mendekatRara = false; // selesai langkah mendekat — tetap berdiri intimidasi
            }
            return;
        }

        // ── Mode "pergi menjauh": override target ke titik jauh dari Rara ──
        if (_pergiMenjauh)
        {
            moveTarget = new Vector2(_xTargetMenjauh, transform.position.y);
            float sisa = Mathf.Abs(transform.position.x - _xTargetMenjauh);
            if (sisa > stopDistance)
            {
                isMoving  = true;
                direction = _arahMenjauh;
            }
            else
            {
                isMoving = false;
                if (sembunyikanSetelahMenjauh) gameObject.SetActive(false);
            }
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

    // ── Reaksi pilihan dialog: AMAN → menjauh, BAHAYA → mendekat ──────
    void HandlePilihanDialog(string kategori)
    {
        if (kategori == "AMAN" && aktifPergiAman)        JalanMenjauh();
        else if (kategori == "BAHAYA" && aktifMendekatBahaya) MendekatRara();
    }

    /// <summary>
    /// Aktifkan mode "pergi menjauh": Paman jalan ke arah berlawanan dari Rara
    /// (jika Paman di kanan Rara → ke kanan; di kiri Rara → ke kiri) sejauh
    /// <see cref="jarakMenjauh"/>. Dialog tidak akan diputar lagi.
    /// </summary>
    public void JalanMenjauh()
    {
        if (_pergiMenjauh) return;
        _mendekatRara = false; // batalkan mendekat jika berjalan
        _pergiMenjauh = true;

        // Arah berlawanan dari posisi Rara: paman menjauh, bukan mendekat.
        if (playerTarget != null)
            _arahMenjauh = (transform.position.x >= playerTarget.position.x) ? 1f : -1f;
        else
            _arahMenjauh = (direction != 0f) ? Mathf.Sign(direction) : 1f;

        _xTargetMenjauh = transform.position.x + _arahMenjauh * jarakMenjauh;

        // Matikan trigger dialog ulang
        dialogTriggered = true;
        hasArrived      = true;
    }

    /// <summary>
    /// Aktifkan mode "mendekat Rara": Paman maju sedikit ke arah Rara
    /// (efek intimidasi) sejauh <see cref="jarakMendekatBahaya"/>.
    /// Tidak melakukan apa pun jika sudah dalam mode pergi-menjauh.
    /// </summary>
    public void MendekatRara()
    {
        if (_pergiMenjauh || _mendekatRara || playerTarget == null) return;
        _mendekatRara = true;

        float arah = (playerTarget.position.x - transform.position.x) >= 0f ? 1f : -1f;
        _xTargetMendekat = transform.position.x + arah * jarakMendekatBahaya;
    }

    void OnDestroy()
    {
        if (dialog != null)
            dialog.OnPilihanDipilih -= HandlePilihanDialog;
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

        // Saat pergi menjauh, paman bisa lebih cepat agar terlihat "meninggalkan" Rara.
        float kecepatanAktif = _pergiMenjauh
            ? walkSpeed * Mathf.Max(0.1f, kecepatanMenjauhMul)
            : walkSpeed;

        if (playerTarget != null)
        {
            // Gerak 2D serong ke moveTarget
            Vector2 toTarget = moveTarget - (Vector2)transform.position;
            Vector2 step     = toTarget.normalized * kecepatanAktif * Time.deltaTime;

            // Jangan overshoot: clamp step agar tidak melewati target
            if (step.magnitude > toTarget.magnitude)
                step = toTarget;

            transform.position += new Vector3(step.x, step.y, 0f);
        }
        else
        {
            // Tidak ada target → jalan lurus (patrol)
            transform.position += new Vector3(direction * kecepatanAktif * Time.deltaTime, 0f, 0f);

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
