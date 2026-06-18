using UnityEngine;

/// <summary>
/// PemotorMovement — pengendara motor yang melaju di jalanan Day 1.
///
/// Fitur:
///   • Animasi loop dari beberapa frame sprite (Assets/sprites/pemotor/1..4).
///   • Bergerak otomatis ke kiri/kanan dengan kecepatan yang bisa di-custom.
///   • Mode lurus (one-shot) atau patrol (bolak-balik antar batas).
///   • Auto-respawn di sisi seberang setelah melewati batas (efek lalu-lintas).
///   • Bisa dipicu oleh jarak ke player (start saat player dekat).
///   • Bunyi klakson SFX opsional (interval bisa di-atur).
///
/// Pasang ke GameObject "pemotor" yang sudah punya SpriteRenderer.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PemotorMovement : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // ANIMASI
    // ══════════════════════════════════════════════════════════════════════
    [Header("Animasi")]
    [Tooltip("Daftar frame sprite. Drag 1.png..4.png dari Assets/sprites/pemotor/")]
    public Sprite[] frames;

    [Tooltip("Durasi tiap frame (detik). Kecil = animasi cepat.")]
    [Range(0.02f, 0.5f)] public float frameDuration = 0.10f;

    [Tooltip("Aktif: animasi berputar terus walau motor sedang berhenti.")]
    public bool animateWhenIdle = true;

    // ══════════════════════════════════════════════════════════════════════
    // PERGERAKAN
    // ══════════════════════════════════════════════════════════════════════
    [Header("Pergerakan")]
    [Tooltip("Kecepatan melaju (unit/detik). Negatif = ke kiri, positif = ke kanan.")]
    public float speed = -4f;

    [Tooltip("Centang kalau sprite asli menghadap kanan. Otomatis flip saat speed < 0.")]
    public bool spriteFacesRightByDefault = true;

    [Tooltip("Aktif sejak Start. Nonaktifkan kalau mau dipicu manual / oleh jarak player.")]
    public bool moveOnStart = false;

    // ══════════════════════════════════════════════════════════════════════
    // MODE LINTASAN
    // ══════════════════════════════════════════════════════════════════════
    public enum Mode
    {
        Lurus,          // jalan ke satu arah, berhenti / di-destroy di ujung
        Patrol,         // bolak-balik antara batas kiri & kanan
        LaluLintasLoop, // setelah melewati batas, respawn di sisi seberang
        HampiriRara     // diam → saat Rara dekat, motor melaju ke arah Rara lalu berhenti di dekatnya
    }

    [Header("Mode Lintasan")]
    public Mode mode = Mode.HampiriRara;

    [Tooltip("Batas kiri (world X). Pakai untuk Patrol & LaluLintasLoop.")]
    public float batasKiri  = -15f;
    [Tooltip("Batas kanan (world X). Pakai untuk Patrol & LaluLintasLoop.")]
    public float batasKanan =  15f;

    [Tooltip("Mode Lurus: kalau true → motor di-destroy saat melewati batas.")]
    public bool destroyDiUjung = false;

    // ══════════════════════════════════════════════════════════════════════
    // PEMICU JARAK PLAYER (opsional)
    // ══════════════════════════════════════════════════════════════════════
    [Header("Pemicu Jarak Player (opsional)")]
    [Tooltip("Kalau diisi, motor baru jalan saat player berada dalam triggerDistance.")]
    public Transform player;
    [Tooltip("Jarak X dari player untuk mulai jalan. 0 = abaikan jarak.")]
    public float triggerDistance = 6f;

    [Tooltip("Mode HampiriRara: motor berhenti saat jaraknya ke Rara ≤ nilai ini.")]
    public float stopDistance = 1.5f;

    [Tooltip("Mode HampiriRara: jeda diam (detik) sebelum motor pergi menjauh.")]
    public float jedaSebelumPergi = 1.0f;

    [Tooltip("Mode HampiriRara: Rara dianggap 'mulai jalan' kalau bergeser ≥ nilai ini (unit) sejak motor berhenti. 0 = abaikan (pakai jeda waktu).")]
    public float raraMoveThreshold = 0.15f;

    [Tooltip("Mode HampiriRara: auto-cari GameObject bernama 'Rara' kalau player kosong.")]
    public bool autoFindRara = true;

    // ══════════════════════════════════════════════════════════════════════
    // DIALOG SAAT BERHENTI (opsional)
    // ══════════════════════════════════════════════════════════════════════
    [Header("Dialog Saat Berhenti (opsional)")]
    [Tooltip("NpcDialog yang dipicu saat motor berhenti di depan Rara. Kosongkan kalau tidak pakai dialog. Auto-find kalau ada NpcDialog di GameObject yang sama.")]
    public NpcDialog npcDialog;

    [Tooltip("Tahan motor di fase Berhenti sampai dialog selesai (tidak pergi walau Rara jalan).")]
    public bool tahanSampaiDialogSelesai = true;

    [Header("Reaksi Pilihan Dialog Rara")]
    [Tooltip("Centang agar motor LANGSUNG masuk fase Pergi (gas pergi menjauh) saat Rara memilih AMAN —\n" +
             "tidak perlu menunggu Rara mulai jalan kembali.")]
    public bool aktifPergiAman = true;

    // ══════════════════════════════════════════════════════════════════════
    // AUDIO (opsional)
    // ══════════════════════════════════════════════════════════════════════
    [Header("Audio Klakson (opsional)")]
    public AudioClip klaksonSfx;
    [Range(0f, 1f)] public float klaksonVolume = 0.6f;
    [Tooltip("Interval rata-rata bunyi klakson (detik). 0 = nonaktif.")]
    public float klaksonInterval = 0f;
    [Tooltip("Variasi acak ± detik di atas interval.")]
    public float klaksonRandomJitter = 1.5f;

    // ══════════════════════════════════════════════════════════════════════
    // INTERNAL
    // ══════════════════════════════════════════════════════════════════════
    SpriteRenderer sr;
    AudioSource    audioSrc;
    int            frameIndex;
    float          frameTimer;
    float          klaksonTimer;
    bool           bergerak;
    bool           movingRight;   // dihitung dari speed

    // Fase mode HampiriRara: Menunggu → Mendekat → Berhenti (jeda) → Pergi
    enum FaseHampiri { Menunggu, Mendekat, Berhenti, Pergi }
    FaseHampiri fase = FaseHampiri.Menunggu;
    float       timerBerhenti;
    float       arahPergi;       // -1 atau +1, dikunci saat masuk fase Pergi
    float       speedAsli;       // |speed| awal, dipakai supaya kecepatan tidak hilang setelah motor sempat berhenti
    bool        pernahJauh;      // true setelah motor pernah berjarak > stopDistance dari Rara
    Vector3     posisiRaraSaatBerhenti; // snapshot posisi Rara saat motor mulai berhenti

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (npcDialog == null) npcDialog = GetComponent<NpcDialog>();

        if (klaksonSfx != null)
        {
            audioSrc = gameObject.AddComponent<AudioSource>();
            audioSrc.clip         = klaksonSfx;
            audioSrc.volume       = klaksonVolume;
            audioSrc.playOnAwake  = false;
            audioSrc.spatialBlend = 0f;
        }

        if (frames != null && frames.Length > 0 && sr.sprite == null)
            sr.sprite = frames[0];
    }

    void Start()
    {
        if (player == null && autoFindRara)
        {
            var p = GameObject.Find("Rara")
                 ?? GameObject.Find("Player")
                 ?? GameObject.Find("player");
            if (p == null)
            {
                try { p = GameObject.FindGameObjectWithTag("Player"); }
                catch { /* tag "Player" mungkin belum terdaftar */ }
            }
            if (p != null) player = p.transform;
        }

        if (mode == Mode.HampiriRara && player == null)
            Debug.LogWarning($"[PemotorMovement] '{name}': field 'Player' kosong & auto-find gagal. " +
                             "Drag GameObject Rara/Player ke field 'Player' di Inspector.", this);

        bergerak  = moveOnStart && Mathf.Abs(speed) > 0.001f;
        speedAsli = Mathf.Abs(speed);
        ResetKlaksonTimer();

        // Subscribe ke event pilihan dialog (mis. "AMAN" → motor gas pergi).
        if (npcDialog != null)
        {
            npcDialog.OnPilihanDipilih -= HandlePilihanDialog;
            npcDialog.OnPilihanDipilih += HandlePilihanDialog;
        }
    }

    void OnDestroy()
    {
        if (npcDialog != null)
            npcDialog.OnPilihanDipilih -= HandlePilihanDialog;
    }

    // ── Reaksi pilihan dialog: AMAN → motor langsung masuk fase Pergi ─────────────
    void HandlePilihanDialog(string kategori)
    {
        if (!aktifPergiAman) return;
        if (mode != Mode.HampiriRara) return;
        if (fase != FaseHampiri.Mendekat && fase != FaseHampiri.Berhenti) return;

        // Hitung arah pergi = menjauh dari Rara (kalau Rara tidak ada → pakai arah saat ini)
        float dxNow = (player != null) ? (player.position.x - transform.position.x) : 0f;
        arahPergi   = (dxNow >= 0f) ? -1f : +1f;

        float kec = (speedAsli > 0.001f) ? speedAsli : Mathf.Abs(speed);
        if (kec < 0.001f) kec = 4f; // fallback minimal supaya motor benar-benar jalan

        speed    = kec * arahPergi;
        bergerak = true;
        fase     = FaseHampiri.Pergi;
    }

    void Update()
    {
        // ── Mode HampiriRara: dekati Rara → berhenti sebentar → pergi menjauh ──
        if (mode == Mode.HampiriRara)
        {
            // Pakai speedAsli supaya kecepatan tidak hilang setelah fase Berhenti (speed sempat di-nol).
            float kecLaju = (speedAsli > 0.001f) ? speedAsli : Mathf.Abs(speed);
            if (Mathf.Abs(speed) > 0.001f) speedAsli = Mathf.Abs(speed); // refresh kalau user ubah speed live

            switch (fase)
            {
                case FaseHampiri.Menunggu:
                    bergerak = false;
                    if (player != null)
                    {
                        float dxW    = player.position.x - transform.position.x;
                        float jarakW = Mathf.Abs(dxW);

                        // Hanya boleh memicu pendekatan kalau motor PERNAH cukup jauh dari Rara.
                        // Mencegah motor langsung melaju saat di-spawn sudah dekat dengan Rara.
                        if (jarakW > stopDistance) pernahJauh = true;

                        if (pernahJauh
                            && (triggerDistance <= 0f || jarakW <= triggerDistance)
                            && jarakW > stopDistance)
                        {
                            fase     = FaseHampiri.Mendekat;
                            bergerak = true;
                        }
                    }
                    break;

                case FaseHampiri.Mendekat:
                    if (player == null) { fase = FaseHampiri.Menunggu; break; }
                    {
                        float dxM    = player.position.x - transform.position.x;
                        float jarakM = Mathf.Abs(dxM);
                        if (jarakM <= stopDistance)
                        {
                            // Kunci arah pergi = berlawanan dari arah Rara
                            arahPergi               = (dxM >= 0f) ? -1f : +1f;
                            speed                   = 0f;
                            bergerak                = false;
                            timerBerhenti           = jedaSebelumPergi;
                            posisiRaraSaatBerhenti  = player.position;
                            fase                    = FaseHampiri.Berhenti;

                            // Picu dialog kalau ada
                            if (npcDialog != null && !npcDialog.IsPlaying)
                                npcDialog.Play();
                        }
                        else
                        {
                            speed    = (dxM >= 0f ? kecLaju : -kecLaju);
                            bergerak = true;
                        }
                    }
                    break;

                case FaseHampiri.Berhenti:
                    bergerak = false;
                    if (player == null) break;
                    {
                        // Selama dialog aktif & user pilih tahan → motor tetap diam.
                        bool dialogAktif = tahanSampaiDialogSelesai
                                           && npcDialog != null
                                           && npcDialog.IsPlaying;
                        if (dialogAktif) break;

                        // Tunggu Rara mulai bergerak. Pergi setelah Rara bergeser > raraMoveThreshold,
                        // ATAU setelah jeda max tercapai (safety, biar tidak diam selamanya).
                        float geserRara = Vector3.Distance(player.position, posisiRaraSaatBerhenti);
                        timerBerhenti -= Time.deltaTime;

                        bool raraSudahJalan = geserRara > raraMoveThreshold;
                        bool jedaHabis      = timerBerhenti <= 0f && jedaSebelumPergi > 0f;

                        // Kalau dialog dipakai tapi 'tahan' false, baru pergi setelah dialog kelar juga.
                        bool dialogMasihJalan = npcDialog != null && npcDialog.IsPlaying;
                        if (dialogMasihJalan && !raraSudahJalan && !jedaHabis) break;

                        if (raraSudahJalan || jedaHabis)
                        {
                            // Hitung ulang arah pergi pakai posisi Rara SAAT INI → selalu menjauh.
                            float dxNow = player.position.x - transform.position.x;
                            arahPergi   = (dxNow >= 0f) ? -1f : +1f;

                            speed    = kecLaju * arahPergi;
                            bergerak = true;
                            fase     = FaseHampiri.Pergi;
                        }
                    }
                    break;

                case FaseHampiri.Pergi:
                    // Tetap melaju ke arah yang dikunci. Tidak kembali ke Rara.
                    speed = kecLaju * arahPergi;
                    break;
            }
        }
        // ── Pemicu jarak biasa (mode lain) ──
        else if (!bergerak && player != null && triggerDistance > 0f)
        {
            float dx = Mathf.Abs(player.position.x - transform.position.x);
            if (dx <= triggerDistance) bergerak = true;
        }

        // ── Animasi ────────────────────────────────────────────────────────
        bool boleh = bergerak || animateWhenIdle;
        if (boleh && frames != null && frames.Length > 0)
        {
            frameTimer += Time.deltaTime;
            if (frameTimer >= frameDuration)
            {
                frameTimer = 0f;
                frameIndex = (frameIndex + 1) % frames.Length;
                if (frames[frameIndex] != null) sr.sprite = frames[frameIndex];
            }
        }

        // ── Pergerakan ─────────────────────────────────────────────────────
        if (!bergerak) return;

        movingRight = speed > 0f;

        // Flip sesuai arah laju
        if (sr != null)
            sr.flipX = spriteFacesRightByDefault ? !movingRight : movingRight;

        transform.position += new Vector3(speed * Time.deltaTime, 0f, 0f);

        if (mode != Mode.HampiriRara) TanganiBatas();

        // ── Audio klakson ─────────────────────────────────────────────────
        if (klaksonInterval > 0f && audioSrc != null)
        {
            klaksonTimer -= Time.deltaTime;
            if (klaksonTimer <= 0f)
            {
                audioSrc.PlayOneShot(klaksonSfx, klaksonVolume);
                ResetKlaksonTimer();
            }
        }
    }

    void TanganiBatas()
    {
        float x = transform.position.x;
        switch (mode)
        {
            case Mode.Lurus:
                if (movingRight && x > batasKanan + 1f)
                {
                    if (destroyDiUjung) Destroy(gameObject);
                    else bergerak = false;
                }
                else if (!movingRight && x < batasKiri - 1f)
                {
                    if (destroyDiUjung) Destroy(gameObject);
                    else bergerak = false;
                }
                break;

            case Mode.Patrol:
                if (movingRight && x >= batasKanan) speed = -Mathf.Abs(speed);
                else if (!movingRight && x <= batasKiri) speed = Mathf.Abs(speed);
                break;

            case Mode.LaluLintasLoop:
                // Lewat batas → teleport ke sisi seberang (efek lalu-lintas tak putus)
                if (movingRight && x > batasKanan + 1f)
                    transform.position = new Vector3(batasKiri - 1f, transform.position.y, transform.position.z);
                else if (!movingRight && x < batasKiri - 1f)
                    transform.position = new Vector3(batasKanan + 1f, transform.position.y, transform.position.z);
                break;
        }
    }

    void ResetKlaksonTimer()
    {
        klaksonTimer = klaksonInterval + Random.Range(-klaksonRandomJitter, klaksonRandomJitter);
        if (klaksonTimer < 0.5f) klaksonTimer = 0.5f;
    }

    // ══════════════════════════════════════════════════════════════════════
    // API publik — bisa dipanggil dari script lain (mis. Day1Controller)
    // ══════════════════════════════════════════════════════════════════════
    public void MulaiBergerak() => bergerak = true;
    public void Berhenti()      => bergerak = false;
    public void SetKecepatan(float v) => speed = v;

    // ══════════════════════════════════════════════════════════════════════
    // EDITOR — visualisasi batas di Scene
    // ══════════════════════════════════════════════════════════════════════
    void OnDrawGizmosSelected()
    {
        if (mode == Mode.Lurus && !destroyDiUjung) return;

        Gizmos.color = new Color(0.2f, 0.85f, 0.95f, 0.9f);
        Vector3 a = new Vector3(batasKiri,  transform.position.y - 1f, 0f);
        Vector3 b = new Vector3(batasKiri,  transform.position.y + 1f, 0f);
        Vector3 c = new Vector3(batasKanan, transform.position.y - 1f, 0f);
        Vector3 d = new Vector3(batasKanan, transform.position.y + 1f, 0f);
        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(c, d);
        Gizmos.color = new Color(0.2f, 0.85f, 0.95f, 0.3f);
        Gizmos.DrawLine(a, c);
        Gizmos.DrawLine(b, d);
    }

#if UNITY_EDITOR
    [ContextMenu("▶ Auto-load frames dari Assets/sprites/pemotor/")]
    void AutoLoadFrames()
    {
        var list = new System.Collections.Generic.List<Sprite>();
        for (int i = 1; i <= 10; i++)
        {
            var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
                $"Assets/sprites/pemotor/{i}.png");
            if (sp != null) list.Add(sp);
        }
        frames = list.ToArray();
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[PemotorMovement] {frames.Length} frame dimuat.");
    }

    // ──────────────────────────────────────────────────────────────────────
    // Setup dialog "Pemotor Jalur Ramai" sekali klik.
    //   • Tambahkan NpcDialog ke GameObject ini (kalau belum ada)
    //   • Assign DialogLayoutDefault.asset (layout Day Intro) → bentuk box sama
    //   • Isi 3 baris dialog default sesuai referensi game-jaga-diri.vercel.app
    // Setelah ini kamu tinggal edit teks/pilihan/profil di Inspector NpcDialog.
    // ──────────────────────────────────────────────────────────────────────
    [ContextMenu("▶ Setup Dialog Pemotor Jalur Ramai (One-Click)")]
    void SetupDialogPemotorRamai()
    {
        var nd = GetComponent<NpcDialog>();
        if (nd == null) nd = gameObject.AddComponent<NpcDialog>();
        npcDialog = nd;

        // Pakai layout yang sama dengan Day 1 Intro biar box-nya seragam
        var layout = UnityEditor.AssetDatabase.LoadAssetAtPath<DialogBoxLayout>(
            "Assets/DialogLayoutDefault.asset");
        if (layout != null) nd.layout = layout;
        else Debug.LogWarning("[PemotorMovement] DialogLayoutDefault.asset tidak ditemukan. " +
                              "Klik kanan NpcDialog \u2192 'Buat + Assign DialogBoxLayout' untuk buat baru.");

        // Sembunyikan latar banner nama \u2014 hanya teks "Pemotor" yang tampil
        nd.showBannerBg = false;

        // Sprite profil pemotor \u2014 frame pertama dari folder pemotor (kalau ada)
        Sprite profil = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/sprites/pemotor/1.png");

        // Preset dialog ala jalur ramai (orang asing nawarin tumpangan motor)
        nd.lines = new NpcDialog.DialogEntry[]
        {
            new NpcDialog.DialogEntry
            {
                speakerName = "Pemotor",
                profile     = profil,
                text        = "Hei adek! Mau bareng om ke sekolah? Naik motor lebih cepat lho daripada jalan kaki."
            },
            new NpcDialog.DialogEntry
            {
                speakerName = "Pemotor",
                profile     = profil,
                text        = "Tenang aja, om kenal sama gurumu kok. Ayo cepetan, nanti telat.",
                choices = new NpcDialog.Choice[]
                {
                    new NpcDialog.Choice { label = "Tidak, terima kasih. Saya jalan saja.", category = "AMAN"   },
                    new NpcDialog.Choice { label = "Em... siapa Om? Saya tidak kenal.",     category = "RAGU"   },
                    new NpcDialog.Choice { label = "Boleh, Om! Kebetulan capek.",           category = "BAHAYA" },
                }
            }
        };

        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.EditorUtility.SetDirty(nd);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        Debug.Log("[PemotorMovement] NpcDialog terpasang + 2 baris preset 'Pemotor Jalur Ramai' siap. " +
                  "Atur teks/pilihan/portrait lebih lanjut di komponen NpcDialog.");
    }
#endif
}
