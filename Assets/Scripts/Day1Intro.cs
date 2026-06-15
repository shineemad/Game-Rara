using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Komponen intro Day 1 — berdiri sendiri, semua bisa dikustomisasi di Inspector.
///
/// CARA SETUP:
///   1. Buat Empty GameObject di scene, beri nama "Day1Intro".
///   2. Tambahkan komponen ini ke GameObject tersebut.
///   3. Isi field di Inspector sesuai kebutuhan.
///   4. Di bagian EVENT, sambungkan onIntroSelesai → Day1Controller.MulaiGame().
///   5. Pastikan Day1Controller.autoMulaiTanpaIntro = false di Inspector.
///
/// ALUR:
///   Scene load → tunggu PrologScreen selesai (otomatis)
///   → tampilkan overlay judul (HARI 1: ...)
///   → tampilkan dialog narasi pembuka
///   → fire event onIntroSelesai → Day1Controller.MulaiGame()
/// </summary>
public class Day1Intro : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // OVERLAY JUDUL
    // ══════════════════════════════════════════════════════════════════════

    [Header("──────── OVERLAY JUDUL HARI ────────")]

    [Tooltip("Baris pertama judul besar (mis. 'HARI 1')")]
    public string barisPertama = "HARI 1";

    [Tooltip("Baris kedua judul (mis. 'Jalan Kaki ke Sekolah')")]
    public string barisKedua   = "Jalan Kaki ke Sekolah";

    [Tooltip("Teks lokasi yang tampil di bawah judul")]
    public string teksLokasi   = "Jalan Menuju Sekolah";

    [Tooltip("Teks kecil paling bawah overlay")]
    public string teksHint     = "Bersiaplah...";

    [Header("Warna Overlay")]
    [Tooltip("Warna background gelap overlay")]
    public Color warnaBackground = new Color(0f, 0f, 0.04f, 0.90f);

    [Tooltip("Warna teks judul (kuning emas)")]
    public Color warnaTeksJudul  = new Color(0.95f, 0.78f, 0.10f, 1f);

    [Tooltip("Warna teks lokasi")]
    public Color warnaTeksLokasi = Color.white;

    [Tooltip("Warna garis dekoratif")]
    public Color warnaGaris      = new Color(0.95f, 0.78f, 0.10f, 0.70f);

    [Header("Ukuran Font Overlay")]
    [Tooltip("Ukuran font baris 1 & 2 judul")]
    public int ukuranFontJudul   = 72;

    [Tooltip("Ukuran font lokasi")]
    public int ukuranFontLokasi  = 36;

    [Header("Durasi Overlay (detik)")]
    [Tooltip("Berapa lama overlay tampil penuh sebelum fade out")]
    public float durasiTampil    = 2.8f;

    [Tooltip("Durasi animasi fade in dan fade out")]
    public float durasiTransisi  = 0.5f;

    // ══════════════════════════════════════════════════════════════════════
    // DIALOG NARASI PEMBUKA
    // ══════════════════════════════════════════════════════════════════════

    [System.Serializable]
    public class BarisNarasi
    {
        [Tooltip("Nama pembicara yang tampil di banner (mis. 'Narasi', 'Rara')")]
        public string pembicara = "Narasi";

        [TextArea(2, 6)]
        [Tooltip("Isi teks dialog")]
        public string teks = "";

        [Tooltip("Sprite portrait/foto pembicara. Kosong = pakai portrait default.")]
        public Sprite portrait;
    }

    [Header("──────── DIALOG NARASI PEMBUKA ────────")]
    [Tooltip("3 baris narasi sesuai game web referensi (Day1.js _showIntro).\n" +
             "Kosongkan array jika tidak ingin ada narasi.")]
    public BarisNarasi[] narasiPembuka = new BarisNarasi[]
    {
        new BarisNarasi
        {
            pembicara = "Narasi",
            teks      = "Pagi ini Rara harus jalan kaki ke sekolah sendirian.\nIbu dan Ayah udah berangkat kerja tadi."
        },
        new BarisNarasi
        {
            pembicara = "Rara",
            teks      = "\"Oke, bismillah! SMP Harapan nggak jauh kok. \uD83D\uDE24\nAku pasti bisa jalan sendiri!\""
        },
        new BarisNarasi
        {
            pembicara = "Narasi",
            teks      = "Tapi tunggu\u2026 jalanan ini nggak selalu aman.\nKalau ada orang asing mendekat, Rara harus tahu cara membela diri."
        }
    };

    // ══════════════════════════════════════════════════════════════════════
    // STYLE BOX DIALOG (NpcDialog style)
    // ══════════════════════════════════════════════════════════════════════

    [Header("──────── SPRITE OVERLAY & DIALOG ────────")]
    [Tooltip("Aset DialogBoxLayout bersama. Jika di-assign DAN useLayoutAsset di-centang,\n" +
             "NILAINYA akan menimpa boxDialogSprite + semua field tata letak panel/portrait/banner/teks/hint.\n" +
             "Cukup edit aset → semua box dialog (Day1Intro, NpcDialog, DialogManager) ikut berubah.")]
    public DialogBoxLayout layout;

    [Tooltip("PENTING: Jika centang DAN field 'layout' di atas diisi, nilai Inspector\n" +
             "banner/text anchor, panel, portrait, hint AKAN DITIMPA dari aset DialogBoxLayout\n" +
             "setiap kali Play. UNCHECK kalau ingin mengedit nilai Inspector langsung.")]
    public bool useLayoutAsset = false;

    [Tooltip("Centang: perubahan field tata letak (banner/text anchor, panel, portrait, hint)\n" +
             "di Inspector LANGSUNG terlihat di Game view saat Play, tanpa perlu stop & play ulang.")]
    public bool liveEditLayout = true;
    [Tooltip("Sprite background overlay judul HARI 1. Auto-load dari UI day 1/6.png.\n" +
             "Kosong = pakai warna solid warnaBackground.")]
    public Sprite overlayBgSprite;

    [Tooltip("Sprite kotak dialog narasi. Auto-load dari UI day 1/8.png.\n" +
             "Kosong = pakai panel gelap solid.")]
    public Sprite boxDialogSprite;

    [Tooltip("Path sprite background overlay (relatif Assets/).")]
    public string overlayBgSpritePath  = "sprites/UI day 1/6.png";
    [Tooltip("Path sprite kotak dialog narasi (relatif Assets/).")]
    public string boxDialogSpritePath  = "sprites/UI day 1/8.png";

    [Tooltip("Sprite banner nama pembicara (lencana kecil). Sama seperti NpcDialog.nameBannerSprite.\n" +
             "Kosong = pakai kotak kuning solid.")]
    public Sprite nameBannerSprite;

    [Tooltip("Sprite portrait Rara (foto karakter Rara).")]
    public Sprite portraitRara;

    [Tooltip("Sprite portrait untuk baris Narasi. Kosong = pakai portraitRara.")]
    public Sprite portraitNarasi;

    [Header("Posisi & Ukuran Box (berlaku saat pakai dialogBoxSprite)")]
    [Tooltip("Posisi tengah horizontal panel (0=kiri, 1=kanan)")]
    [Range(0f, 1f)] public float panelCenterX   = 0.50f;
    [Tooltip("Posisi tengah vertikal panel (0=bawah, 1=atas)")]
    [Range(0f, 1f)] public float panelCenterY   = 0.215f;
    [Tooltip("Lebar panel (fraksi layar, 0–1)")]
    [Range(0.1f, 1f)] public float panelWidthFrac  = 0.96f;
    [Tooltip("Tinggi panel (fraksi layar, 0–1)")]
    [Range(0.02f, 0.5f)] public float panelHeightFrac = 0.395f;

    [Header("Tata Letak Box (anchor 0–1, hanya berlaku saat pakai sprite)")]
    [Tooltip("Posisi tengah horizontal portrait dalam panel (0=kiri, 1=kanan)")]
    [Range(0f, 1f)] public float portraitCenterX = 0.153f;
    [Tooltip("Posisi tengah vertikal portrait dalam panel (0=bawah, 1=atas)")]
    [Range(0f, 1f)] public float portraitCenterY = 0.625f;
    [Tooltip("Lebar portrait sebagai fraksi lebar panel")]
    [Range(0.02f, 0.6f)] public float portraitSizeW = 0.192f;
    [Tooltip("Tinggi portrait sebagai fraksi tinggi panel")]
    [Range(0.02f, 1f)] public float portraitSizeH = 0.494f;
    [Tooltip("Pertahankan rasio aspek portrait (centang = tidak stretch)")]
    public bool portraitPreserveAspect = true;
    [Tooltip("Banner nama: anchor kiri-bawah (X=kiri, Y=bawah)")]
    public Vector2 bannerAnchorMin = new Vector2(0.11f, 0.11f);
    [Tooltip("Banner nama: anchor kanan-atas (X=kanan, Y=atas)")]
    public Vector2 bannerAnchorMax = new Vector2(0.253f, 0.333f);
    [Tooltip("Area teks: anchor kiri-bawah")]
    public Vector2 textAnchorMin   = new Vector2(0.31f, 0.55f);
    [Tooltip("Area teks: anchor kanan-atas")]
    public Vector2 textAnchorMax   = new Vector2(0.84f, 0.76f);

    [Header("Posisi Petunjuk Lanjut (geser di sini)")]
    [Tooltip("Posisi tengah horizontal petunjuk dalam panel (0=kiri, 1=kanan)")]
    [Range(0f, 1f)] public float hintCenterX = 0.82f;
    [Tooltip("Posisi tengah vertikal petunjuk dalam panel (0=bawah, 1=atas)")]
    [Range(0f, 1f)] public float hintCenterY = 0.13f;
    [Tooltip("Lebar area petunjuk (fraksi lebar panel)")]
    [Range(0.05f, 1f)] public float hintSizeW = 0.30f;
    [Tooltip("Tinggi area petunjuk (fraksi tinggi panel)")]
    [Range(0.02f, 0.5f)] public float hintSizeH = 0.12f;

    [Header("Warna Box (berlaku jika tidak pakai sprite)")]
    public Color warnaPanel   = new Color(0f, 0f, 0f, 0.82f);
    public Color warnaBorder  = new Color(1f, 0.85f, 0.3f, 1f);
    public Color warnaBanner  = new Color(0.14f, 0.09f, 0.01f, 0.92f);
    public Color warnaNama    = new Color(1f, 0.85f, 0.3f, 1f);
    public Color warnaTeksDlg = Color.white;
    public Color warnaHintDlg = new Color(1f, 1f, 1f, 0.55f);

    [Header("Font & Ukuran")]
    public int fontSizeNama   = 30;
    public int fontSizeTeksDlg = 26;
    public int fontSizeHint   = 16;
    [Tooltip("Detik per karakter efek ketik. 0 = langsung tampil.")]
    [Range(0f, 0.1f)] public float kecepatanKetik = 0.025f;

    // ══════════════════════════════════════════════════════════════════════
    // REFERENSI
    // ══════════════════════════════════════════════════════════════════════

    [Header("──────── REFERENSI (opsional) ────────")]

    [Tooltip("Font TMP untuk overlay judul dan box dialog. Kosong = pakai font default.")]
    public TMP_FontAsset fontAsset;

    // ══════════════════════════════════════════════════════════════════════
    // EVENT — SAMBUNGKAN KE Day1Controller.MulaiGame()
    // ══════════════════════════════════════════════════════════════════════

    [Header("──────── EVENT SETELAH INTRO ────────")]
    [Tooltip("Dipanggil setelah overlay + narasi selesai.\n" +
             "Sambungkan ke Day1Controller.MulaiGame() di Inspector.")]
    public UnityEvent onIntroSelesai;

    /// True setelah intro selesai. GameObject Day1Intro TIDAK dihancurkan (hanya
    /// canvas-nya), jadi sistem lain (mis. MobileControls) harus memeriksa flag ini
    /// — bukan sekadar keberadaan komponen — untuk tahu intro sudah berakhir.
    public bool Selesai { get; private set; }

    // ══════════════════════════════════════════════════════════════════════
    // EDITOR AUTO-LOAD SPRITES
    // ══════════════════════════════════════════════════════════════════════

#if UNITY_EDITOR
    void Reset()
    {
        TryLoadSprites(overwrite: true);
    }

    [ContextMenu("▶ Muat Sprite Overlay & Dialog Default (6.png / 8.png)")]
    void TryLoadSpritesMenu()
    {
        TryLoadSprites(overwrite: true);
        Debug.Log("[Day1Intro] overlayBgSprite=" + (overlayBgSprite != null ? overlayBgSprite.name : "null")
                + "  boxDialogSprite=" + (boxDialogSprite != null ? boxDialogSprite.name : "null"));
    }

    /// <summary>
    /// One-click setup: buat (jika belum ada) aset DialogBoxLayout di
    /// Assets/DialogLayoutDefault.asset, isi dengan preset 8.png + sprite-nya,
    /// lalu auto-assign ke field `layout` komponen ini. Setelah itu,
    /// Day1Intro/NpcDialog/DialogManager yang juga punya field Layout cukup
    /// di-drag aset yang sama → satu sumber kebenaran.
    /// </summary>
    [ContextMenu("▶ Buat + Assign DialogBoxLayout (One-Click Cara B)")]
    void CreateAndAssignLayoutAsset()
    {
        const string assetPath = "Assets/DialogLayoutDefault.asset";
        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<DialogBoxLayout>(assetPath);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<DialogBoxLayout>();
            UnityEditor.AssetDatabase.CreateAsset(asset, assetPath);
            Debug.Log("[Day1Intro] Aset baru dibuat: " + assetPath);
        }

        // Isi sprite default + reset preset 8
        var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/UI day 1/8.png");
        if (sp != null) asset.boxSprite = sp;
        asset.ResetToPreset8();

        // Assign ke field layout komponen ini + apply ke field lokal
        layout = asset;
        ApplyLayoutAsset();

        UnityEditor.EditorUtility.SetDirty(asset);
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();

        Debug.Log("[Day1Intro] Layout di-assign: " + assetPath +
                  ". Drag aset ini ke field 'Layout' pada NpcDialog & DialogManager juga.");
    }

    void TryLoadSprites(bool overwrite)
    {
        bool dirty = false;
        if ((overwrite || overlayBgSprite == null) && !string.IsNullOrEmpty(overlayBgSpritePath))
        {
            var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/" + overlayBgSpritePath);
            if (sp != null) { overlayBgSprite = sp; dirty = true; }
            else Debug.LogWarning("[Day1Intro] Sprite tidak ditemukan: Assets/" + overlayBgSpritePath);
        }
        if ((overwrite || boxDialogSprite == null) && !string.IsNullOrEmpty(boxDialogSpritePath))
        {
            var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/" + boxDialogSpritePath);
            if (sp != null)
            {
                boxDialogSprite = sp;
                // CATATAN: TIDAK auto-apply ApplyLayoutPreset8() di sini, karena
                // itu akan menimpa nilai banner/text anchor yang sudah di-tweak
                // user di Inspector. Untuk reset ke preset: klik kanan komponen
                // → "▶ Terapkan Layout Box untuk 8.png".
                dirty = true;
            }
            else Debug.LogWarning("[Day1Intro] Sprite tidak ditemukan: Assets/" + boxDialogSpritePath);
        }
        if (dirty) UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    // ══════════════════════════════════════════════════════════════════════
    // PRESET LAYOUT untuk sprite 8.png (kotak dialog)
    // ══════════════════════════════════════════════════════════════════════

    [ContextMenu("▶ Terapkan Layout Box untuk 8.png")]
    public void ApplyLayoutPreset8()
    {
        // Sprite 8.png rasio ~2.42:1 (1325×547). Pada layar 16:9 dengan
        // panelWidthFrac=0.96 → tinggi proporsional ≈ 0.96/2.42 ≈ 0.40.
        panelCenterX    = 0.50f;
        panelCenterY    = 0.215f;
        panelWidthFrac  = 0.96f;
        panelHeightFrac = 0.395f;

        // Frame portrait kiri-atas. Pixel sprite X 75–330, Y 70–340.
        portraitCenterX        = 0.153f;
        portraitCenterY        = 0.625f;
        portraitSizeW          = 0.192f;
        portraitSizeH          = 0.494f;
        portraitPreserveAspect = true;

        // Banner nama kayu (di bawah frame portrait). Pixel X 75–335, Y 365–440.
        bannerAnchorMin = new Vector2(0.057f, 0.196f);
        bannerAnchorMax = new Vector2(0.253f, 0.333f);

        // Area teks besar di kanan. Pixel X 440–1280, Y 70–475. Padding aman dari border.
        textAnchorMin = new Vector2(0.345f, 0.20f);
        textAnchorMax = new Vector2(0.955f, 0.78f);

        // Hint "▼ SPACE / Klik untuk lanjut" — pojok kanan-bawah area teks
        hintCenterX = 0.82f;
        hintCenterY = 0.13f;
        hintSizeW   = 0.30f;
        hintSizeH   = 0.12f;

        fontSizeNama    = 30;
        fontSizeTeksDlg = 30;
        fontSizeHint    = 18;

        warnaNama    = new Color(1f,    0.85f, 0.30f, 1f);
        warnaTeksDlg = new Color(1f,    0.96f, 0.88f, 1f);
        warnaHintDlg = new Color(1f,    1f,    1f,    0.55f);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        ApplyLayout();
    }

    /// <summary>
    /// Preset Day Intro — variasi preset 8 dengan banner & teks digeser,
    /// portrait sedikit lebih kecil. Cocok untuk box dialog narasi Day 1.
    /// </summary>
    [ContextMenu("▶ Terapkan Layout Day Intro")]
    public void ApplyLayoutPresetDayIntro()
    {
        panelCenterX    = 0.50f;
        panelCenterY    = 0.219f;
        panelWidthFrac  = 0.939f;
        panelHeightFrac = 0.395f;

        portraitCenterX        = 0.14f;
        portraitCenterY        = 0.584f;
        portraitSizeW          = 0.189f;
        portraitSizeH          = 0.56f;
        portraitPreserveAspect = false;

        bannerAnchorMin = new Vector2(0.10f,  0.10f);
        bannerAnchorMax = new Vector2(0.253f, 0.333f);

        textAnchorMin = new Vector2(0.31f, 0.55f);
        textAnchorMax = new Vector2(0.84f, 0.76f);

        hintCenterX = 0.798f;
        hintCenterY = 0.242f;
        hintSizeW   = 0.296f;
        hintSizeH   = 0.12f;

        fontSizeNama    = 30;
        fontSizeTeksDlg = 30;
        fontSizeHint    = 18;

        warnaNama    = new Color(1f, 0.85f, 0.30f, 1f);
        warnaTeksDlg = new Color(1f, 0.96f, 0.88f, 1f);
        warnaHintDlg = new Color(1f, 1f,    1f,    0.55f);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        ApplyLayout();
    }


    /// <summary>
    /// Muat preset layout box dialog yang cocok dengan sprite NpcDialog Day 1.
    /// Klik kanan komponen Day1Intro di Inspector → "Load Preset: Box Dialog Day 1".
    /// </summary>
    [ContextMenu("Load Preset: Box Dialog Day 1")]
    void LoadPresetBoxDialogDay1()
    {
        // Posisi & Ukuran Box
        panelCenterX    = 0.50f;
        panelCenterY    = 0.178f;
        panelWidthFrac  = 0.924f;
        panelHeightFrac = 0.291f;

        // Portrait
        portraitCenterX       = 0.15f;
        portraitCenterY       = 0.505f;
        portraitSizeW         = 0.186f;
        portraitSizeH         = 0.622f;
        portraitPreserveAspect = false;

        // Banner nama
        bannerAnchorMin = new Vector2(0.25f, 0.58f);
        bannerAnchorMax = new Vector2(0.49f, 0.86f);

        // Area teks
        textAnchorMin = new Vector2(0.25f, 0.10f);
        textAnchorMax = new Vector2(0.96f, 0.57f);

        // Petunjuk lanjut
        hintCenterX = 0.74f;
        hintCenterY = 0.32f;
        hintSizeW   = 0.38f;
        hintSizeH   = 0.18f;

        // Warna & font
        warnaNama    = new Color(1f, 0.85f, 0.3f, 1f);
        warnaTeksDlg = Color.white;
        warnaHintDlg = new Color(1f, 1f, 1f, 0.55f);
        warnaPanel   = new Color(0f, 0f, 0f, 0.82f);
        warnaBorder  = new Color(1f, 0.85f, 0.3f, 1f);
        warnaBanner  = new Color(0.14f, 0.09f, 0.01f, 0.92f);
        fontSizeNama    = 30;
        fontSizeTeksDlg = 26;
        fontSizeHint    = 16;
        kecepatanKetik  = 0.025f;

        // Narasi pembuka (3 baris dari game web referensi)
        narasiPembuka = new BarisNarasi[]
        {
            new BarisNarasi
            {
                pembicara = "Narasi",
                teks = "Pagi ini Rara harus jalan kaki ke sekolah sendirian.\nIbu dan Ayah udah berangkat kerja tadi."
            },
            new BarisNarasi
            {
                pembicara = "Rara",
                teks = "\"Oke, bismillah! SMP Harapan nggak jauh kok.\nAku pasti bisa jalan sendiri!\""
            },
            new BarisNarasi
            {
                pembicara = "Narasi",
                teks = "Tapi tunggu\u2026 jalanan ini nggak selalu aman.\nKalau ada orang asing mendekat, Rara harus tahu cara membela diri."
            }
        };

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log("[Day1Intro] Preset 'Box Dialog Day 1' berhasil dimuat.");
#endif
    }

    // ══════════════════════════════════════════════════════════════════════
    // RUNTIME STATE — referensi UI aktif (untuk live-edit via OnValidate)
    // ══════════════════════════════════════════════════════════════════════

    private player          _playerComp;   // referensi ke player.cs untuk freeze selama intro

    private RectTransform   _panelRT;
    private RectTransform   _portRT;
    private Image           _portImg;
    private RectTransform   _bannerRT;
    private RectTransform   _bodyRT;
    private RectTransform   _hintRT;
    private TextMeshProUGUI _tmpNama;
    private TextMeshProUGUI _tmpTeks;
    private TextMeshProUGUI _tmpHint;

    // ══════════════════════════════════════════════════════════════════════
    // LIVE LAYOUT — geser slider di Inspector saat Play Mode → langsung update
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Terapkan semua nilai field (posisi, ukuran, warna, font) ke UI yang sedang tampil.
    /// Dipanggil otomatis oleh OnValidate() saat nilai Inspector berubah di Play Mode.
    /// Bisa juga dipanggil manual via klik kanan komponen → "Terapkan Layout".
    /// </summary>
    [ContextMenu("Terapkan Layout (Play Mode)")]
    public void ApplyLayout()
    {
        if (!Application.isPlaying || _panelRT == null) return;

        // ── Panel ──────────────────────────────────────────────────────────
        _panelRT.anchorMin = new Vector2(
            panelCenterX - panelWidthFrac  * 0.5f,
            panelCenterY - panelHeightFrac * 0.5f);
        _panelRT.anchorMax = new Vector2(
            panelCenterX + panelWidthFrac  * 0.5f,
            panelCenterY + panelHeightFrac * 0.5f);

        // ── Portrait ───────────────────────────────────────────────────────
        if (_portRT != null)
        {
            _portRT.anchorMin = new Vector2(
                portraitCenterX - portraitSizeW * 0.5f,
                portraitCenterY - portraitSizeH * 0.5f);
            _portRT.anchorMax = new Vector2(
                portraitCenterX + portraitSizeW * 0.5f,
                portraitCenterY + portraitSizeH * 0.5f);
            if (_portImg != null)
                _portImg.preserveAspect = portraitPreserveAspect;
        }

        // ── Banner nama ────────────────────────────────────────────────────
        if (_bannerRT != null)
        {
            _bannerRT.anchorMin = bannerAnchorMin;
            _bannerRT.anchorMax = bannerAnchorMax;
        }

        // ── Area teks ──────────────────────────────────────────────────────
        if (_bodyRT != null)
        {
            _bodyRT.anchorMin = textAnchorMin;
            _bodyRT.anchorMax = textAnchorMax;
        }

        // ── Petunjuk lanjut ────────────────────────────────────────────────
        if (_hintRT != null)
        {
            _hintRT.anchorMin = new Vector2(
                hintCenterX - hintSizeW * 0.5f,
                hintCenterY - hintSizeH * 0.5f);
            _hintRT.anchorMax = new Vector2(
                hintCenterX + hintSizeW * 0.5f,
                hintCenterY + hintSizeH * 0.5f);
        }

        // ── Warna & ukuran font ────────────────────────────────────────────
        if (_tmpNama  != null) { _tmpNama.color  = warnaNama;    _tmpNama.fontSize  = fontSizeNama;    }
        if (_tmpTeks  != null) { _tmpTeks.color  = warnaTeksDlg; _tmpTeks.fontSize  = fontSizeTeksDlg; }
        if (_tmpHint  != null) { _tmpHint.color  = warnaHintDlg; _tmpHint.fontSize  = fontSizeHint;   }
    }

    [System.NonSerialized] bool _inOnValidate;
    void OnValidate()
    {
        // CATATAN: jangan panggil ApplyLayoutAsset() di sini.
        // Sinkronisasi dari aset DialogBoxLayout dilakukan satu arah:
        // DialogBoxLayout.OnValidate → push ke komponen. Kalau komponen
        // juga push balik dari aset di sini, terjadi loop tak terbatas
        // (→ StackOverflowException).
        if (_inOnValidate) return;
        _inOnValidate = true;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) { _inOnValidate = false; return; }
            ApplyLayout();
            _inOnValidate = false;
        };
#else
        ApplyLayout();
        _inOnValidate = false;
#endif
    }

    // ══════════════════════════════════════════════════════════════════════
    // RUNTIME
    // ══════════════════════════════════════════════════════════════════════

    void Start()
    {
#if UNITY_EDITOR
        // Saat Play di Editor, selalu sinkronkan sprite dengan path
        // (overwrite=true) — sprite lama yang tersimpan di scene akan diganti.
        // Kosongkan path di Inspector kalau ingin pakai sprite manual.
        TryLoadSprites(overwrite: true);
#endif
        // Jika aset DialogBoxLayout di-assign DAN user mau pakai — terapkan nilainya.
        // Kalau useLayoutAsset = false, nilai Inspector (yang user edit manual) DIPERTAHANKAN.
        if (useLayoutAsset)
        {
            ApplyLayoutAsset();
        }
        else if (layout != null)
        {
            Debug.Log($"[Day1Intro] useLayoutAsset = false — nilai dari aset '{layout.name}' DIABAIKAN. Pakai nilai Inspector.");
        }
        StartCoroutine(JalankanIntro());
    }

    // ── Live edit: kalau user ubah field tata letak di Inspector saat Play,
    // terapkan setiap frame ke RectTransform agar perubahan langsung kelihatan.
    void Update()
    {
        if (liveEditLayout && _panelRT != null)
            ApplyLayout();
    }

    /// <summary>
    /// Salin nilai dari aset DialogBoxLayout ke field lokal komponen ini.
    /// Dipanggil di Start(); juga di OnValidate() agar perubahan di aset
    /// langsung terlihat saat Play Mode.
    /// </summary>
#if UNITY_EDITOR
    [ContextMenu("▶ Sync sekarang dari Layout")]
    void SyncFromLayoutMenu()
    {
        ApplyLayoutAsset();
        UnityEditor.EditorUtility.SetDirty(this);
        if (!Application.isPlaying)
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[Day1Intro] Disinkron dari layout: " + (layout != null ? layout.name : "<null>"));
    }
#endif
    public void ApplyLayoutAsset()
    {
        if (layout == null) return;

        if (layout.boxSprite        != null) boxDialogSprite  = layout.boxSprite;
        if (layout.nameBannerSprite != null) nameBannerSprite = layout.nameBannerSprite;

        panelCenterX    = layout.panelCenterX;
        panelCenterY    = layout.panelCenterY;
        panelWidthFrac  = layout.panelWidthFrac;
        panelHeightFrac = layout.panelHeightFrac;

        portraitCenterX        = layout.portraitCenterX;
        portraitCenterY        = layout.portraitCenterY;
        portraitSizeW          = layout.portraitSizeW;
        portraitSizeH          = layout.portraitSizeH;
        portraitPreserveAspect = layout.portraitPreserveAspect;

        bannerAnchorMin = layout.bannerAnchorMin;
        bannerAnchorMax = layout.bannerAnchorMax;
        textAnchorMin   = layout.textAnchorMin;
        textAnchorMax   = layout.textAnchorMax;

        hintCenterX = layout.hintCenterX;
        hintCenterY = layout.hintCenterY;
        hintSizeW   = layout.hintSizeW;
        hintSizeH   = layout.hintSizeH;

        if (Application.isPlaying) ApplyLayout();
    }

    IEnumerator JalankanIntro()
    {
        // ── Bekukan player sejak awal intro hingga selesai ─────────────────
        _playerComp = FindFirstObjectByType<player>();
        if (_playerComp != null) _playerComp.frozen = true;

        // ── Langkah 1: Tunggu PrologScreen selesai (jika ada) ─────────────────
        bool adaProlog = FindAnyObjectByType<PrologScreen>() != null;
        if (adaProlog)
        {
            // Poll static flag — PrologScreen.prologDone di-set true saat EndProlog()
            yield return new WaitUntil(() => PrologScreen.prologDone);

            // 1 frame ekstra agar canvas prolog selesai di-destroy
            yield return null;
        }

        // ── Langkah 2: Setup audio & game state ───────────────────────────
        AudioManager.Instance?.PlayBGM(AudioManager.BGMTrack.Day1);
        if (GameState.Instance != null) GameState.Instance.day = 1;

        // ── Langkah 3: Tampilkan overlay judul ────────────────────────────
        yield return StartCoroutine(TampilkanOverlay());

        // ── Langkah 4: Tampilkan narasi (jika ada) ────────────────────────
        if (narasiPembuka != null && narasiPembuka.Length > 0)
            yield return StartCoroutine(TampilkanNarasi());

        // ── Langkah 5: Beritahu Day1Controller untuk mulai game ───────────        // Pastikan player di-unfreeze langsung di sini sebagai jaminan,
        // agar tidak bergantung penuh pada Day1Controller.MulaiGame()
        if (_playerComp != null) _playerComp.frozen = false;
        Selesai = true;
        onIntroSelesai?.Invoke();
    }

    // ══════════════════════════════════════════════════════════════════════
    // OVERLAY JUDUL
    // ══════════════════════════════════════════════════════════════════════

    IEnumerator TampilkanOverlay()
    {
        // Buat canvas overlay
        var cGO = new GameObject("Day1IntroCanvas");
        var cv  = cGO.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 980;

        var sc = cGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920f, 1080f);
        sc.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        var cg = cGO.AddComponent<CanvasGroup>();
        cg.alpha          = 0f;
        cg.blocksRaycasts = false;

        // Background — sprite jika tersedia, fallback warna solid
        var bgImg = BuatImage(cGO.transform, "BG", Vector2.zero, Vector2.one, warnaBackground);
        if (overlayBgSprite != null)
        {
            bgImg.sprite         = overlayBgSprite;
            bgImg.type           = Image.Type.Simple;
            bgImg.color          = Color.white;
            bgImg.preserveAspect = false;
        }

        // Garis dekoratif atas
        BuatImage(cGO.transform, "GarisAtas",
            new Vector2(0.08f, 0.685f), new Vector2(0.92f, 0.690f), warnaGaris);

        // Garis dekoratif bawah
        BuatImage(cGO.transform, "GarisBawah",
            new Vector2(0.08f, 0.295f), new Vector2(0.92f, 0.300f), warnaGaris);

        // Baris pertama — "HARI 1:"
        BuatTMP(cGO.transform, "Judul1",
            new Vector2(0.05f, 0.565f), new Vector2(0.95f, 0.685f),
            barisPertama + ":", ukuranFontJudul + 8, warnaTeksJudul, true);

        // Baris kedua — "Jalan Kaki ke Sekolah"
        BuatTMP(cGO.transform, "Judul2",
            new Vector2(0.05f, 0.44f), new Vector2(0.95f, 0.565f),
            barisKedua, ukuranFontJudul, warnaTeksJudul, true);

        // Lokasi
        BuatTMP(cGO.transform, "Lokasi",
            new Vector2(0.05f, 0.31f), new Vector2(0.95f, 0.43f),
            "\uD83D\uDCCD  " + teksLokasi, ukuranFontLokasi, warnaTeksLokasi, false);

        // Hint kecil
        BuatTMP(cGO.transform, "Hint",
            new Vector2(0.15f, 0.22f), new Vector2(0.85f, 0.305f),
            teksHint, 22, new Color(1f, 1f, 1f, 0.45f), false);

        // Fade in
        for (float t = 0f; t < durasiTransisi; t += Time.deltaTime)
        { cg.alpha = t / durasiTransisi; yield return null; }
        cg.alpha = 1f;

        // Tahan
        yield return new WaitForSeconds(durasiTampil);

        // Fade out
        for (float t = 0f; t < durasiTransisi; t += Time.deltaTime)
        { cg.alpha = 1f - t / durasiTransisi; yield return null; }
        cg.alpha = 0f;

        Destroy(cGO);
    }

    // ══════════════════════════════════════════════════════════════════════
    // NARASI — via DialogManager atau overlay fallback
    // ══════════════════════════════════════════════════════════════════════

    // ══════════════════════════════════════════════════════════════════════
    // NARASI PEMBUKA — style NpcDialog (portrait + box + banner + efek ketik)
    // ══════════════════════════════════════════════════════════════════════

    IEnumerator TampilkanNarasi()
    {
        yield return StartCoroutine(JalankanNarasi(narasiPembuka));
    }

    /// <summary>
    /// API publik: tampilkan narasi kustom memakai box dialog Day 1 Intro
    /// (tata letak/layout SAMA dengan intro). onSelesai dipanggil setelah pemain
    /// menutup seluruh baris. Dipakai mis. oleh Day1Controller untuk narasi jalan
    /// (Jalan Ramai / Gang Sepi) agar box-nya seragam dengan intro.
    /// </summary>
    public void TampilkanNarasiKustom(BarisNarasi[] daftar, System.Action onSelesai = null)
    {
        StartCoroutine(JalankanNarasiKustom(daftar, onSelesai));
    }

    IEnumerator JalankanNarasiKustom(BarisNarasi[] daftar, System.Action onSelesai)
    {
        yield return StartCoroutine(JalankanNarasi(daftar));
        onSelesai?.Invoke();
    }

    IEnumerator JalankanNarasi(BarisNarasi[] daftar)
    {
        if (daftar == null || daftar.Length == 0)
            yield break;

        // Buat canvas narasi
        var cGO = new GameObject("NarasiDialogCanvas");
        var cv  = cGO.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 960;
        var sc = cGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920f, 1080f);
        sc.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // ── Panel utama ───────────────────────────────────────────────────
        float pxMin = panelCenterX - panelWidthFrac  * 0.5f;
        float pyMin = panelCenterY - panelHeightFrac * 0.5f;
        float pxMax = panelCenterX + panelWidthFrac  * 0.5f;
        float pyMax = panelCenterY + panelHeightFrac * 0.5f;

        var panelGO = new GameObject("Panel");
        panelGO.transform.SetParent(cGO.transform, false);
        var panelRT = panelGO.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(pxMin, pyMin);
        panelRT.anchorMax = new Vector2(pxMax, pyMax);
        panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;
        var panelImg = panelGO.AddComponent<Image>();
        if (boxDialogSprite != null)
        {
            panelImg.sprite           = boxDialogSprite;
            panelImg.type             = Image.Type.Sliced;
            panelImg.color            = Color.white;
        }
        else
        {
            panelImg.color = warnaPanel;
            // Border outline
            var outline = panelGO.AddComponent<Outline>();
            outline.effectColor    = warnaBorder;
            outline.effectDistance = new Vector2(2f, -2f);
        }

        // ── Portrait (kiri, mengikuti field portraitCenter/Size) ──────────────
        var portGO = new GameObject("Portrait");
        portGO.transform.SetParent(panelGO.transform, false);
        var portRT = portGO.AddComponent<RectTransform>();
        portRT.anchorMin = new Vector2(
            portraitCenterX - portraitSizeW * 0.5f,
            portraitCenterY - portraitSizeH * 0.5f);
        portRT.anchorMax = new Vector2(
            portraitCenterX + portraitSizeW * 0.5f,
            portraitCenterY + portraitSizeH * 0.5f);
        portRT.offsetMin = portRT.offsetMax = Vector2.zero;
        var portImg = portGO.AddComponent<Image>();
        portImg.preserveAspect = portraitPreserveAspect;
        portImg.color          = Color.white;

        // ── Banner nama (anchor dari field bannerAnchorMin/Max) ───────────────
        var bannerGO = new GameObject("BannerNama");
        bannerGO.transform.SetParent(panelGO.transform, false);
        var bannerRT = bannerGO.AddComponent<RectTransform>();
        bannerRT.anchorMin = bannerAnchorMin;
        bannerRT.anchorMax = bannerAnchorMax;
        bannerRT.offsetMin = bannerRT.offsetMax = Vector2.zero;
        var bannerImg = bannerGO.AddComponent<Image>();
        if (nameBannerSprite != null)
        {
            bannerImg.sprite = nameBannerSprite;
            bannerImg.type   = Image.Type.Sliced;
            bannerImg.color  = Color.white;
        }
        else
        {
            bannerImg.color = warnaBanner;
        }

        var tmpNama = BuatTMP(bannerGO.transform, "Nama",
            Vector2.zero, Vector2.one,
            "", fontSizeNama, warnaNama, true);
        tmpNama.alignment = TextAlignmentOptions.MidlineLeft;
        tmpNama.margin    = new Vector4(12f, 0f, 4f, 0f);

        // ── Area teks (anchor dari field textAnchorMin/Max) ───────────────────
        var tmpTeks = BuatTMP(panelGO.transform, "Teks",
            textAnchorMin, textAnchorMax,
            "", fontSizeTeksDlg, warnaTeksDlg, false);
        tmpTeks.alignment          = TextAlignmentOptions.TopLeft;
        tmpTeks.textWrappingMode   = TextWrappingModes.Normal;
        tmpTeks.overflowMode       = TextOverflowModes.Ellipsis;

        // ── Hint lanjut (anchor dari field hintCenter/Size) ───────────────────
        var tmpHint = BuatTMP(panelGO.transform, "Hint",
            new Vector2(hintCenterX - hintSizeW * 0.5f, hintCenterY - hintSizeH * 0.5f),
            new Vector2(hintCenterX + hintSizeW * 0.5f, hintCenterY + hintSizeH * 0.5f),
            "\u25BC SPACE / Klik untuk lanjut", fontSizeHint,
            warnaHintDlg, false);
        tmpHint.alignment = TextAlignmentOptions.MidlineRight;

        // ── Simpan referensi untuk live-edit (OnValidate / ApplyLayout) ────────────
        _panelRT  = panelRT;
        _portRT   = portRT;
        _portImg  = portImg;
        _bannerRT = bannerRT;
        _bodyRT   = (RectTransform)tmpTeks.transform;
        _hintRT   = (RectTransform)tmpHint.transform;
        _tmpNama  = tmpNama;
        _tmpTeks  = tmpTeks;
        _tmpHint  = tmpHint;

        // ── Tampilkan setiap baris ─────────────────────────────────────────
        foreach (var baris in daftar)
        {
            // Guard: lewati baris yang teks-nya kosong/null (mencegah dialog
            // menggantung di body kosong karena entri Inspector belum diisi).
            if (baris == null || string.IsNullOrWhiteSpace(baris.teks))
            {
                Debug.LogWarning("[Day1Intro] Baris narasi kosong dilewati " +
                    "(pembicara=\"" + (baris != null ? baris.pembicara : "<null>") + "\"). " +
                    "Cek Inspector \u2192 Narasi Pembuka.");
                continue;
            }

            // Pilih portrait
            Sprite port = baris.portrait;
            if (port == null)
                port = baris.pembicara.ToLower().Contains("rara") ? portraitRara
                     : baris.pembicara.ToLower().Contains("narasi") ? (portraitNarasi != null ? portraitNarasi : portraitRara)
                     : portraitRara;
            portImg.sprite  = port;
            portImg.enabled = (port != null);

            tmpNama.text = baris.pembicara ?? "";
            tmpTeks.text = "";

            // Efek ketik
            string fullText = baris.teks ?? "";
            bool skipTyping = false;

            if (kecepatanKetik <= 0f)
            {
                tmpTeks.text = fullText;
            }
            else
            {
                // Jalankan efek ketik di coroutine — bisa di-skip dengan klik/SPACE
                bool selesaiKetik = false;
                StartCoroutine(EfekKetik(tmpTeks, fullText,
                    () => selesaiKetik = true,
                    () => skipTyping));

                // Tunggu ketik selesai ATAU user menekan skip
                while (!selesaiKetik)
                {
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                    {
                        skipTyping   = true;       // sinyal stop ke coroutine
                        tmpTeks.text = fullText;   // langsung tampilkan semua
                        yield return null;         // beri 1 frame agar coroutine bisa berhenti
                        break;
                    }
                    yield return null;
                }
            }

            // Tunggu klik/SPACE untuk lanjut ke baris berikutnya
            bool lanjut = false;
            // Jangan langsung lanjut di frame yang sama dengan skip ketik
            yield return null;
            while (!lanjut)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                    lanjut = true;
                yield return null;
            }
            yield return null; // hindari double-skip
        }

        Destroy(cGO);

        // Bersihkan referensi agar ApplyLayout tidak bekerja pada GO yang sudah di-destroy
        _panelRT = null; _portRT = null; _portImg = null;
        _bannerRT = null; _bodyRT = null; _hintRT = null;
        _tmpNama = null; _tmpTeks = null; _tmpHint = null;
    }

    // Coroutine efek ketik — bisa dihentikan dari luar lewat flag
    IEnumerator EfekKetik(TextMeshProUGUI target, string fullText,
        System.Action onSelesai, System.Func<bool> shouldSkip)
    {
        target.text = "";
        foreach (char c in fullText)
        {
            if (shouldSkip()) break;
            target.text += c;
            yield return new WaitForSeconds(kecepatanKetik);
        }
        target.text = fullText; // pastikan semua teks tampil
        onSelesai?.Invoke();
    }

    // ══════════════════════════════════════════════════════════════════════
    // HELPERS UI
    // ══════════════════════════════════════════════════════════════════════

    static Image BuatImage(Transform parent, string nama,
        Vector2 anchorMin, Vector2 anchorMax, Color warna)
    {
        var go = new GameObject(nama);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = warna;
        return img;
    }

    TextMeshProUGUI BuatTMP(Transform parent, string nama,
        Vector2 anchorMin, Vector2 anchorMax,
        string teks, int ukuranFont, Color warna, bool bold)
    {
        var go = new GameObject(nama);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text               = teks;
        tmp.fontSize           = ukuranFont;
        tmp.color              = warna;
        tmp.fontStyle          = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.alignment          = TextAlignmentOptions.Center;
        tmp.textWrappingMode   = TextWrappingModes.Normal;
        if (fontAsset != null) tmp.font = fontAsset;
        return tmp;
    }
}
