using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NpcGang — NPC antagonis (gang/preman jalanan) yang berjalan otomatis
/// dengan animasi sprite. Strukturnya identik dengan PamanBaik, tapi
/// dipakai untuk NPC mencurigakan di jalur Gang Sepi.
///
/// Setup di Inspector:
///   walkSprites      → drag sprite dari Assets/sprites/NPC gang sepi/
///   walkSpeed        → kecepatan berjalan (unit/detik)
///   frameDuration    → durasi tiap frame animasi (detik)
///   direction        → arah jalan: 1 = kanan, -1 = kiri
///   patrolMode       → aktifkan agar NPC bolak-balik antara patrol kiri & kanan
///   patrolLeft/Right → batas patrol
///
///   playerTarget     → drag GameObject Rara ke sini
///   triggerDistance  → jarak agar NPC mulai mendekati Rara
///   stopDistance     → jarak berhenti (sudah cukup dekat)
/// </summary>
public class NpcGang : MonoBehaviour
{
    [Header("Animasi Jalan")]
    public Sprite[] walkSprites;
    public float    frameDuration = 0.12f;

    [Header("Sprite Diam (Idle)")]
    [Tooltip("Sprite yang ditampilkan saat NPC diam. Kosongkan = pakai walkSprites[0].")]
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
    [Tooltip("Jarak dari Rara agar NPC mulai bergerak mendekat. Set besar agar gang grup terdeteksi dari jauh.")]
    public float triggerDistance = 12f;
    [Tooltip("Jarak berhenti — sudah sampai di tujuan (toleransi clamp ke moveTarget)")]
    public float stopDistance    = 0.15f;

    [Header("Jarak Berhenti dari Rara")]
    [Tooltip("Sisi mana NPC berhenti relatif Rara: 1 = kanan, -1 = kiri, 0 = otomatis (sisi terdekat)")]
    public int stopSide = 0;
    [Tooltip("Jarak horizontal MINIMAL NPC dari Rara saat berhenti (unit dunia). \nGang grup default berhenti agak jauh agar terlihat menghadang.")]
    public float jarakBerhentiDariRara = 5f;
    [Tooltip("Jika true: ketika Rara mendekat ke NPC yang sedang diam, NPC akan mundur menjaga jarak.")]
    public bool jagaJarakMundur = true;

    [Header("Posisi Tujuan Akhir NPC (opsional)")]
    [Tooltip("Drag empty GameObject ke sini sebagai titik berhenti NPC.\nKosongkan = otomatis di sisi Rara (pakai jarakBerhentiDariRara).")]
    public Transform stopPoint;
    [Tooltip("[DEPRECATED] Pakai jarakBerhentiDariRara. Disimpan untuk kompatibilitas scene lama.")]
    public float frontOffset = 5f;

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
    [Tooltip("Komponen NpcDialog yang akan dimainkan ketika NPC tiba di samping Rara.\nKosongkan untuk menonaktifkan dialog.")]
    public NpcDialog dialog;
    [Tooltip("Hanya tampilkan dialog satu kali (true) atau setiap kali tiba (false)")]
    public bool dialogOnceOnly = true;
    [Tooltip("Jeda kecil sebelum dialog muncul setelah tiba (detik)")]
    public float dialogDelay = 0.2f;

    [Header("Setelah Dialog Selesai")]
    [Tooltip("Jika true: setelah dialog selesai, NPC gang berjalan ke kiri menjauh dari Rara lalu hilang.")]
    public bool perigiKeKiriSetelahDialog = true;
    [Tooltip("Kecepatan saat NPC pergi menjauh setelah dialog (unit/detik).")]
    public float kecepatanPergi = 3.5f;
    [Tooltip("Jarak horizontal dari Rara saat NPC dianggap sudah cukup jauh dan di-nonaktifkan.")]
    public float jarakHilangSetelahPergi = 14f;
    [Header("Reaksi Pilihan Dialog Rara")]
    [Tooltip("Centang agar NPC gang LANGSUNG mundur (leaving mode) saat Rara memilih AMAN —\n" +
             "tanpa menunggu dialog edukasi selesai. Efek: gang takut & buru-buru pergi.")]
    public bool aktifPergiAman = true;
    [Tooltip("Centang agar NPC gang mendekat sedikit ke Rara saat Rara memilih BAHAYA (intimidasi).")]
    public bool aktifMendekatBahaya = true;
    [Tooltip("Jarak (unit dunia) NPC maju ke Rara saat BAHAYA dipilih.")]
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
    // Guard: hanya panggil ResumePlayer jika NpcGang-lah yang melakukan freeze
    private bool           frozePlayerFromGang = false;
    // Kunci sisi (kiri/kanan) saat NPC pertama kali bergerak menuju Rara.
    // Mencegah sideSign flip tiap frame saat NPC melewati posisi X Rara,
    // sehingga dua NPC gang berhenti simetris pada jarak yang sama.
    private int            _lockedSideSign = 0;
    // Mode "pergi menjauh ke kiri" — aktif setelah dialog selesai.
    // Saat true, UpdateMoveState diabaikan; NPC berjalan terus ke kiri
    // dengan kecepatanPergi sampai cukup jauh, lalu di-SetActive(false).
    private bool           _leavingMode = false;
    // Mode "mendekat Rara" (reaksi BAHAYA): NPC maju sedikit lalu berhenti.
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
            // Fallback: cari komponen 'player' bila tag "Player" belum di-set di scene.
            // Tanpa ini, playerTarget null → NPC jatuh ke patrol & jalan menjauh ke kiri.
            if (playerTarget == null)
            {
                var pc = FindFirstObjectByType<player>();
                if (pc != null) playerTarget = pc.transform;
            }
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
        // Gunakan OnGangDialogEnd (bukan langsung ResumePlayer) agar hanya
        // unfreeze jika NpcGang-lah yang melakukan freeze — mencegah
        // premature unfreeze saat Day1Controller sedang encounter lain.
        if (dialog != null && day1Controller != null)
        {
            dialog.onDialogEnd.RemoveListener(day1Controller.ResumePlayer);
            dialog.onDialogEnd.RemoveListener(OnGangDialogEnd);
            dialog.onDialogEnd.AddListener(OnGangDialogEnd);
        }

        // Subscribe ke event pilihan dialog: AMAN → langsung mundur, BAHAYA → mendekat.
        if (dialog != null)
        {
            dialog.OnPilihanDipilih -= HandlePilihanDialog;
            dialog.OnPilihanDipilih += HandlePilihanDialog;
        }

        // Pastikan dialog memakai style Day1Intro saat runtime juga (bukan hanya saat klik ContextMenu)
        if (dialog != null)
            ApplyDay1BoxStyleRuntime(dialog);

        // Setup shadow di Start agar sr.sprite & sorting layer sudah siap
        SetupShadow();

        // Abaikan tabrakan fisik antara NPC ↔ Rara,
        // supaya NPC bisa berdiri tepat di sisi Rara tanpa terhalang collider
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

        // Pastikan NPC punya sprite awal dari walkSprites
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

        // Salin sorting layer dari NPC, order satu di bawahnya
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

    // ── Trigger dialog saat NPC dekat Rara ───────────────────────────────
    void UpdateDialogTrigger()
    {
        if (dialog == null || playerTarget == null) return;

        // Mode leaving / mendekat-BAHAYA: jangan picu dialog ulang.
        if (_leavingMode || _mendekatRara) return;

        // Sudah pernah dipicu dan hanya sekali? tidak perlu cek lagi
        if (dialogTriggered && dialogOnceOnly) return;

        // Ukur jarak langsung ke Rara (lebih stabil dari moveTarget yang terus berubah)
        float distToRara = Vector2.Distance(transform.position, playerTarget.position);

        // Anggap "sudah tiba" jika: tidak sedang jalan DAN cukup dekat dengan Rara
        bool arrived = !isMoving && distToRara <= (jarakBerhentiDariRara + stopDistance + 0.5f);

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
                    // Tandai bahwa freeze berasal dari NpcGang agar ResumePlayer
                    // hanya dipanggil oleh kita (bukan bocor ke encounter lain).
                    if (day1Controller != null)
                    {
                        frozePlayerFromGang = true;
                        day1Controller.FreezePlayer();
                    }
                    if (!dialog.IsPlaying) dialog.Play();
                }
            }
        }
        else if (distToRara > triggerDistance)
        {
            // Rara menjauh cukup jauh — reset agar dialog bisa muncul lagi
            hasArrived      = false;
            arriveTimer     = 0f;
            _lockedSideSign = 0;   // reset kunci sisi agar bisa tentukan ulang
            if (!dialogOnceOnly) dialogTriggered = false;
        }
    }

    // ── Dipanggil saat dialog NpcGang selesai ───────────────────────────
    void OnGangDialogEnd()
    {
        // Hanya bebaskan player jika NpcGang yang melakukan freeze
        if (frozePlayerFromGang && day1Controller != null)
        {
            frozePlayerFromGang = false;
            day1Controller.ResumePlayer();
        }

        // Aktifkan mode pergi ke kiri menjauh dari Rara
        if (perigiKeKiriSetelahDialog)
            AktifkanLeavingMode();
    }

    // ── Reaksi pilihan dialog: AMAN → langsung mundur; BAHAYA → mendekat ────
    void HandlePilihanDialog(string kategori)
    {
        if (_leavingMode) return;

        if (kategori == "AMAN" && aktifPergiAman)
        {
            // Gang ketakutan — langsung mundur tanpa menunggu dialog edukasi selesai.
            AktifkanLeavingMode();
        }
        else if (kategori == "BAHAYA" && aktifMendekatBahaya && playerTarget != null)
        {
            MendekatRara();
        }
    }

    void AktifkanLeavingMode()
    {
        if (_leavingMode) return;
        _leavingMode  = true;
        _mendekatRara = false; // batalkan mendekat jika sedang aktif
        direction     = -1f;   // hadap & jalan ke kiri
        isMoving      = true;
    }

    /// <summary>
    /// Aktifkan mode "mendekat Rara": NPC gang maju sedikit ke arah Rara
    /// (efek intimidasi) sejauh <see cref="jarakMendekatBahaya"/>.
    /// Tidak melakukan apa pun jika sudah dalam mode leaving.
    /// </summary>
    public void MendekatRara()
    {
        if (_leavingMode || _mendekatRara || playerTarget == null) return;
        _mendekatRara = true;

        float arah = (playerTarget.position.x - transform.position.x) >= 0f ? 1f : -1f;
        _xTargetMendekat = transform.position.x + arah * jarakMendekatBahaya;
    }

    void OnDestroy()
    {
        if (dialog != null)
            dialog.OnPilihanDipilih -= HandlePilihanDialog;
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
        // Mode pergi: NPC berjalan terus ke kiri menjauhi Rara, lalu hilang.
        if (_leavingMode)
        {
            isMoving  = true;
            direction = -1f;
            // Target imajiner jauh di kiri agar MoveCharacter() bergerak konsisten
            moveTarget = new Vector2(transform.position.x - 100f, transform.position.y);

            if (playerTarget != null)
            {
                float jarakX = transform.position.x - playerTarget.position.x;
                // Sudah cukup jauh di kiri Rara → nonaktifkan NPC
                if (jarakX < -jarakHilangSetelahPergi)
                {
                    gameObject.SetActive(false);
                }
            }
            return;
        }

        // Mode mendekat (reaksi BAHAYA): NPC maju sedikit ke arah Rara lalu berhenti.
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

        if (playerTarget == null)
        {
            // Tanpa target Rara: DIAM di tempat — jangan jalan menjauh ke kiri.
            // (Mencegah gang ngeloyor pergi saat referensi Rara belum ke-set.)
            isMoving = false;
            return;
        }

        if (stopPoint != null)
        {
            // Mode stopPoint: NPC berjalan ke titik yang sudah ditentukan.
            // Trigger = Rara mendekati AREA STOPPOINT (bukan NPC)
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
            // Mode otomatis: NPC berhenti pada JARAK tertentu dari Rara (jaga jarak)
            // stopSide: 1 = kanan Rara, -1 = kiri Rara, 0 = sisi terdekat (otomatis)
            int sideSign = stopSide;
            if (sideSign == 0)
            {
                // Kunci sisi saat pertama kali NPC dipicu — jangan hitung ulang
                // tiap frame agar NPC tidak berpindah sisi saat melintas Rara.
                if (_lockedSideSign == 0)
                    _lockedSideSign = (transform.position.x >= playerTarget.position.x) ? 1 : -1;
                sideSign = _lockedSideSign;
            }

            // Titik berhenti = berjarak jarakBerhentiDariRara dari Rara di sisi NPC.
            moveTarget = new Vector2(
                playerTarget.position.x + sideSign * jarakBerhentiDariRara,
                playerTarget.position.y
            );

            float distToRara = Vector2.Distance(transform.position, playerTarget.position);

            // 1) Rara masuk jangkauan trigger & gap belum tertutup → MAJU ke arah Rara.
            //    Arah dihitung langsung ke posisi Rara (bukan ke titik offset) supaya
            //    NPC selalu terlihat bergerak mendekati Rara, lalu berhenti saat
            //    jaraknya sudah mencapai jarakBerhentiDariRara.
            if (distToRara <= triggerDistance && distToRara > jarakBerhentiDariRara + stopDistance)
            {
                isMoving  = true;
                direction = (playerTarget.position.x > transform.position.x) ? 1f : -1f;
            }
            // 2) Rara terlalu dekat (menerobos personal space) → NPC mundur jika diizinkan
            else if (jagaJarakMundur && distToRara < jarakBerhentiDariRara - 0.1f)
            {
                isMoving  = true;
                // Mundur = menjauhi Rara
                direction = (transform.position.x >= playerTarget.position.x) ? 1f : -1f;
                moveTarget = new Vector2(
                    playerTarget.position.x + sideSign * jarakBerhentiDariRara,
                    playerTarget.position.y
                );
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
        // Mode pergi: jalan lurus ke kiri dengan kecepatanPergi
        if (_leavingMode)
        {
            transform.position += new Vector3(direction * kecepatanPergi * Time.deltaTime, 0f, 0f);
            return;
        }
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

    // ══════════════════════════════════════════════════════════════════════
    // RUNTIME: Terapkan tata letak box dialog identik Day1Intro
    // (dijalankan tiap Start() agar tampilan box gang konsisten saat Play,
    // tanpa harus klik ContextMenu di Editor lebih dulu).
    // ══════════════════════════════════════════════════════════════════════
    void ApplyDay1BoxStyleRuntime(NpcDialog d)
    {
        if (d == null) return;

        // ── 0. PAKSA load sprite box dialog ──────────────────────────────
        // Tanpa sprite, NpcDialog jatuh ke fallback panel yang memunculkan
        // Outline kuning 2 garis (bug "garis kuning di gang sepi").
        // Coba dari Resources dulu, lalu AssetDatabase (Editor only).
        if (d.dialogBoxSprite == null)
        {
            // 1) Resources.Load — bekerja di Editor & Build
            var sp = Resources.Load<Sprite>("UI day 1/8");
            if (sp == null) sp = Resources.Load<Sprite>("8");
            if (sp != null) d.dialogBoxSprite = sp;
        }
#if UNITY_EDITOR
        if (d.dialogBoxSprite == null)
        {
            // 2) Fallback Editor: AssetDatabase
            d.dialogBoxSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/sprites/UI day 1/8.png");
        }
#endif
        if (string.IsNullOrEmpty(d.boxDialogSpritePath))
            d.boxDialogSpritePath = "sprites/UI day 1/8.png";

        // Jika SEMUA cara load gagal → panel transparan total agar tidak
        // muncul Outline kuning (lebih baik tidak terlihat daripada bug visual).
        if (d.dialogBoxSprite == null)
        {
            d.panelColor  = new Color(0f, 0f, 0f, 0f);
            d.borderColor = new Color(0f, 0f, 0f, 0f);
        }
        else
        {
            d.panelColor  = new Color(0f, 0f, 0f, 0.78f);
            d.borderColor = new Color(1f, 0.85f, 0.3f, 0f);
        }

        // ── Warna & font ──
        d.speakerColor    = new Color(1f, 0.85f, 0.3f, 1f);
        d.textColor       = Color.white;
        d.hintColor       = new Color(1f, 1f, 1f, 0.55f);
        d.speakerFontSize = 26;
        d.textFontSize    = 26;
        d.hintFontSize    = 16;
        d.typeSpeed       = 0.025f;
        d.showBannerBg    = false;
        d.continueHint    = "";

        // Pastikan banner nama TIDAK pakai sprite (tanpa kotak coklat)
        d.nameBannerSprite = null;

        // ── Posisi & ukuran panel (ikut box dialog Paman NPC) ──
        d.panelCenterX    = 0.50f;
        d.panelCenterY    = 0.16f;
        d.panelWidthFrac  = 0.972f;
        d.panelHeightFrac = 0.291f;

        // ── Portrait kiri ──
        d.portraitCenterX = 0.14f;
        d.portraitCenterY = 0.584f;
        d.portraitSizeW   = 0.189f;
        d.portraitSizeH   = 0.56f;
        d.portraitPreserveAspect = true;   // potret jaga rasio (tercentang) saat game berjalan

        // ── Banner nama (anchor area teks nama) ──
        d.bannerAnchorMin = new Vector2(0.03f,  0.11f);
        d.bannerAnchorMax = new Vector2(0.253f, 0.333f);

        // ── Area teks utama ──
        d.textAnchorMin   = new Vector2(0.31f, 0.55f);
        d.textAnchorMax   = new Vector2(0.84f, 0.76f);

        // ── Petunjuk lanjut ──
        d.hintCenterX     = 0.798f;
        d.hintCenterY     = 0.242f;
        d.hintSizeW       = 0.296f;
        d.hintSizeH       = 0.12f;

        // ── Fallback layout legacy ──
        d.panelHeight     = 220f;
        d.panelWidthRatio = 0.85f;
        d.bottomMargin    = 40f;
        d.showAtTop       = false;
    }

    // ── Gizmo: visualisasi jarak di Scene view ────────────────────────────
    void OnDrawGizmosSelected()
    {
        // Lingkaran kuning = triggerDistance (dari NPC)
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
            // Mode otomatis: berhenti pada jarak tertentu di sisi Rara
            int sideSign = stopSide;
            if (sideSign == 0)
                sideSign = (transform.position.x >= playerTarget.position.x) ? 1 : -1;
            actualTarget = playerTarget.position + new Vector3(sideSign * jarakBerhentiDariRara, 0f, 0f);
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
    [ContextMenu("Load Walk Sprites dari NPC gang sepi")]
    void EditorLoadSprites()
    {
        string folder = "Assets/sprites/NPC gang sepi";
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
        Debug.Log($"[NpcGang] walkSprites ter-assign: {walkSprites.Length} frame dari '{folder}'");
    }

    [ContextMenu("Load Walk Sprites dari NPC jalan ramai")]
    void EditorLoadSpritesRamai()
    {
        string folder = "Assets/sprites/NPC jalan ramai";
        var guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite", new[] { folder });

        var list = new List<Sprite>();
        foreach (var guid in guids)
        {
            string path   = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var    sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) list.Add(sprite);
        }

        list.Sort((a, b) =>
        {
            int na = ExtractNumber(a.name);
            int nb = ExtractNumber(b.name);
            return na.CompareTo(nb);
        });

        walkSprites = list.ToArray();
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[NpcGang] walkSprites ter-assign: {walkSprites.Length} frame dari '{folder}'");
    }

    static int ExtractNumber(string s)
    {
        var match = System.Text.RegularExpressions.Regex.Match(s, @"\d+");
        return match.Success ? int.Parse(match.Value) : 0;
    }

    // ══════════════════════════════════════════════════════════════════════
    // DIALOG DEFAULT — GANG SEPI (3 skenario perundungan/intimidasi)
    // ══════════════════════════════════════════════════════════════════════
    // Otomatis menambahkan komponen NpcDialog (jika belum ada) dan mengisi
    // baris dialog + pilihan AMAN/RAGU/BAHAYA bertema gang/preman di gang sepi.
    // Style box dialog mengikuti Day1Intro (sprite UI day 1/8.png).

    [ContextMenu("Dialog Default ▶ Skenario 1: Hadang & Palak")]
    void EditorSeedDialog_Hadang()
    {
        var d = EnsureDialog();
        d.lines = new NpcDialog.DialogEntry[]
        {
            new NpcDialog.DialogEntry
            {
                speakerName = "Narasi",
                text = "Tiga remaja besar berdiri menghadang di tengah gang.\nMereka menatap Rara sambil tersenyum sinis."
            },
            new NpcDialog.DialogEntry
            {
                speakerName = "Anak Gang",
                text = "\"Eh bocah! Lewat sini bayar dulu dong~\nMana uang jajan lo? HP-nya juga sini!\""
            },
            new NpcDialog.DialogEntry
            {
                speakerName = "Rara (batin)",
                text = "Jantung Rara berdebar kencang.\nMereka mau memalak! Aku harus pilih dengan tepat..."
            },
            new NpcDialog.DialogEntry
            {
                speakerName = "Rara",
                text = "Apa yang harus aku lakukan sekarang?",
                choices = new NpcDialog.Choice[]
                {
                    new NpcDialog.Choice { label = "BALIK ARAH & lari ke jalan ramai sambil teriak \"TOLONG!\"", category = "AMAN" },
                    new NpcDialog.Choice { label = "Diam, pura-pura tidak dengar sambil terus jalan",          category = "RAGU"   },
                    new NpcDialog.Choice { label = "Serahkan saja uang & HP biar selamat",                     category = "BAHAYA" },
                }
            },
        };
        ApplyDay1BoxStyle(d);
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.EditorUtility.SetDirty(d);
        Debug.Log("[NpcGang] Dialog default 'Hadang & Palak' ter-isi.");
    }

    [ContextMenu("Dialog Default ▶ Skenario 2: Ejek & Bully")]
    void EditorSeedDialog_Bully()
    {
        var d = EnsureDialog();
        d.lines = new NpcDialog.DialogEntry[]
        {
            new NpcDialog.DialogEntry
            {
                speakerName = "Anak Gang",
                text = "\"HAHAHA, lihat tuh anak kecil! Sok berani lewat gang ini ya?\nDorong aja yuk biar nangis~\""
            },
            new NpcDialog.DialogEntry
            {
                speakerName = "Anak Gang",
                text = "\"Hei, jawab dong! Atau lo bisu?\nMana tas lo, gue periksa dulu!\""
            },
            new NpcDialog.DialogEntry
            {
                speakerName = "Rara",
                text = "Mereka mau membully aku! Aku harus...?",
                choices = new NpcDialog.Choice[]
                {
                    new NpcDialog.Choice { label = "Tatap balik, mundur perlahan & teriak minta tolong",        category = "AMAN" },
                    new NpcDialog.Choice { label = "Tertawa ikut-ikutan supaya mereka tidak marah",             category = "RAGU"   },
                    new NpcDialog.Choice { label = "Balas mengejek mereka",                                     category = "BAHAYA" },
                }
            },
        };
        ApplyDay1BoxStyle(d);
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.EditorUtility.SetDirty(d);
        Debug.Log("[NpcGang] Dialog default 'Ejek & Bully' ter-isi.");
    }

    [ContextMenu("Dialog Default ▶ Skenario 3: Ajak Bolos / Ikut Geng")]
    void EditorSeedDialog_Ajakan()
    {
        var d = EnsureDialog();
        d.lines = new NpcDialog.DialogEntry[]
        {
            new NpcDialog.DialogEntry
            {
                speakerName = "Anak Gang",
                text = "\"Eh kamu kelas berapa? Ikut nongkrong sama kita yuk!\nKita ada tempat asik nih, bolos sekolah aja sekali-kali~\""
            },
            new NpcDialog.DialogEntry
            {
                speakerName = "Anak Gang",
                text = "\"Tenang, sama abang-abang aman kok.\nNanti dikasih jajan, dijamin seru!\""
            },
            new NpcDialog.DialogEntry
            {
                speakerName = "Rara",
                text = "Mereka mengajak aku ikut. Aku harus...?",
                choices = new NpcDialog.Choice[]
                {
                    new NpcDialog.Choice { label = "\"NGGAK! Aku mau ke sekolah.\" (Tegas menolak & jalan terus ke jalan ramai)", category = "AMAN" },
                    new NpcDialog.Choice { label = "\"Hmm... lain kali aja ya kak.\" (Menolak ragu)",                              category = "RAGU"   },
                    new NpcDialog.Choice { label = "\"Boleh deh!\" (Ikut mereka)",                                                 category = "BAHAYA" },
                }
            },
        };
        ApplyDay1BoxStyle(d);
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.EditorUtility.SetDirty(d);
        Debug.Log("[NpcGang] Dialog default 'Ajak Bolos' ter-isi.");
    }

    // Pastikan komponen NpcDialog ada di GO ini, lalu link ke field 'dialog'.
    NpcDialog EnsureDialog()
    {
        if (dialog == null) dialog = GetComponent<NpcDialog>();
        if (dialog == null) dialog = gameObject.AddComponent<NpcDialog>();
        return dialog;
    }

    // Terapkan style box dialog seperti Day1Intro: pakai sprite UI day 1/8.png
    // sehingga tampilan kotak dialog NPC gang seragam dengan intro.
    void ApplyDay1BoxStyle(NpcDialog d)
    {
        // ── 1. Auto-load sprite box dialog & banner nama (sama dengan Day1Intro) ──
        if (d.dialogBoxSprite == null)
        {
            d.dialogBoxSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/UI day 1/8.png");
            d.boxDialogSpritePath = "sprites/UI day 1/8.png";
        }
        if (d.nameBannerSprite == null)
        {
            // Coba beberapa kandidat sprite banner di folder UI day 1
            string[] candidates = { "Assets/sprites/UI day 1/9.png", "Assets/sprites/UI day 1/7.png", "Assets/sprites/UI day 1/10.png" };
            foreach (var p in candidates)
            {
                var s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(p);
                if (s != null) { d.nameBannerSprite = s; break; }
            }
        }

        // Hilangkan sprite banner nama untuk NPC gang (nama saja, tanpa kotak background)
        d.nameBannerSprite = null;

        // ── 2. Warna & font (kuning emas + teks putih, identik Day1Intro) ──
        d.speakerColor    = new Color(1f, 0.85f, 0.3f, 1f);
        d.borderColor     = new Color(1f, 0.85f, 0.3f, 1f);
        d.textColor       = Color.white;
        d.panelColor      = new Color(0f, 0f, 0f, 0.82f);
        d.hintColor       = new Color(1f, 1f, 1f, 0.55f);
        d.speakerFontSize = 30;
        d.textFontSize    = 26;
        d.hintFontSize    = 16;
        d.typeSpeed       = 0.025f;
        d.showBannerBg    = false;
        d.continueHint    = "";

        // ── 3. Posisi & ukuran panel (match Day1Intro narasiPembuka) ──
        d.panelCenterX    = 0.50f;
        d.panelCenterY    = 0.219f;
        d.panelWidthFrac  = 0.939f;
        d.panelHeightFrac = 0.395f;

        // ── 4. Tata letak portrait kiri (gulungan kertas / foto karakter) ──
        d.portraitCenterX = 0.14f;
        d.portraitCenterY = 0.584f;
        d.portraitSizeW   = 0.189f;
        d.portraitSizeH   = 0.56f;
        d.portraitPreserveAspect = false;

        // ── 5. Banner nama di bawah portrait ──
        d.bannerAnchorMin = new Vector2(0.03f,  0.1f);
        d.bannerAnchorMax = new Vector2(0.253f, 0.333f);

        // ── 6. Area teks di kanan (besar) ──
        d.textAnchorMin   = new Vector2(0.31f, 0.55f);
        d.textAnchorMax   = new Vector2(0.84f, 0.76f);

        // ── 7. Petunjuk lanjut di pojok kanan bawah panel ──
        d.hintCenterX     = 0.798f;
        d.hintCenterY     = 0.242f;
        d.hintSizeW       = 0.296f;
        d.hintSizeH       = 0.12f;

        // ── 8. Fallback panel legacy (kalau sprite gagal load) ──
        d.panelHeight     = 220f;
        d.panelWidthRatio = 0.85f;
        d.bottomMargin    = 40f;
        d.showAtTop       = false;
    }
#endif
}
