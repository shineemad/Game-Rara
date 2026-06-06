using UnityEngine;

/// <summary>
/// NpcLatarPatroli — NPC latar (ramai) untuk membuat Jalan Ramai terasa hidup.
/// Berjalan bolak-balik antara dua titik X, sprite auto-flip sesuai arah.
///
/// Cara pakai:
///   1. Buat GameObject baru dengan SpriteRenderer (sprite orang dewasa / anak)
///   2. Tambah komponen ini
///   3. Set jarakPatroli (mis. 4) → NPC akan jalan dari posisi awal -2 → +2
///   4. (Opsional) Drag SpriteRenderer.sprite atau gunakan animator
///   5. Drag GameObject ini ke PathEnvironment.objekJalanRamai[]
///
/// Setup minimal: sprite + script ini. Tidak perlu Rigidbody/Collider.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class NpcLatarPatroli : MonoBehaviour
{
    [Header("Patroli")]
    [Tooltip("Jarak total patroli dari posisi awal (unit world). NPC bergerak ±setengah dari nilai ini.")]
    public float jarakPatroli = 4f;
    [Tooltip("Kecepatan jalan (unit/detik).")]
    public float kecepatan    = 1.2f;
    [Tooltip("Jeda saat sampai ujung (detik). 0 = tanpa jeda.")]
    public float jedaDiUjung  = 0.8f;
    [Tooltip("Mulai arah ke kanan? Jika false, mulai ke kiri.")]
    public bool  mulaiKeKanan = true;

    [Header("Visual")]
    [Tooltip("Auto-flip sprite saat ganti arah (X scale dibalik).")]
    public bool  autoFlip = true;
    [Tooltip("Sprite menghadap ke kanan secara default? (default true)")]
    public bool  spriteMenghadapKanan = true;

    [Header("Animasi Sederhana (opsional)")]
    [Tooltip("Frame animasi jalan. Kosong = sprite statis.")]
    public Sprite[] frameJalan;
    [Tooltip("FPS animasi.")]
    public float fpsAnimasi = 6f;

    // ── runtime ───────────────────────────────────────────────────────────
    private SpriteRenderer _sr;
    private float _xAwal;
    private float _xMin, _xMax;
    private int   _arah; // -1 / +1
    private float _jedaTimer;
    private float _animTimer;
    private int   _animFrame;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _xAwal = transform.position.x;
        float half = jarakPatroli * 0.5f;
        _xMin = _xAwal - half;
        _xMax = _xAwal + half;
        _arah = mulaiKeKanan ? 1 : -1;
        TerapkanFlip();
    }

    void Update()
    {
        // Jeda di ujung
        if (_jedaTimer > 0f)
        {
            _jedaTimer -= Time.deltaTime;
            return;
        }

        // Gerak
        var p = transform.position;
        p.x += _arah * kecepatan * Time.deltaTime;

        if (p.x >= _xMax) { p.x = _xMax; _arah = -1; _jedaTimer = jedaDiUjung; TerapkanFlip(); }
        else if (p.x <= _xMin) { p.x = _xMin; _arah = 1; _jedaTimer = jedaDiUjung; TerapkanFlip(); }

        transform.position = p;

        // Animasi frame
        if (frameJalan != null && frameJalan.Length > 0)
        {
            _animTimer += Time.deltaTime;
            float frameDur = 1f / Mathf.Max(0.01f, fpsAnimasi);
            if (_animTimer >= frameDur)
            {
                _animTimer = 0f;
                _animFrame = (_animFrame + 1) % frameJalan.Length;
                _sr.sprite = frameJalan[_animFrame];
            }
        }
    }

    void TerapkanFlip()
    {
        if (!autoFlip) return;
        // Tentukan apakah sprite perlu di-flip:
        // sprite default menghadap kanan + arah kiri → flip
        // sprite default menghadap kiri  + arah kanan → flip
        bool perluFlip = spriteMenghadapKanan ? (_arah < 0) : (_arah > 0);
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (perluFlip ? -1 : 1);
        transform.localScale = s;
    }
}
