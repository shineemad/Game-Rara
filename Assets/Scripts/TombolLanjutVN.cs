using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tombol "LANJUT" untuk dialog/narasi gaya Visual Novel.
/// Dipakai supaya dialog HANYA lanjut saat tombol ini diklik
/// (bukan saat klik di mana saja di layar).
///
/// Cara pakai:
///   var tombol = TombolLanjutVN.Pasang(boxTransform, font);
///   ...
///   if (tombol.Konsumsi() || Input.GetKeyDown(KeyCode.Space)) lanjut = true;
///
/// Konsumsi() mengembalikan true sekali saat tombol baru ditekan, lalu reset.
/// Tetap dukung keyboard SPACE/ENTER sebagai pintasan (bukan "klik di luar tombol").
///
/// Kustomisasi live di Play mode: ubah field pada komponen
/// <see cref="TombolLanjutKonfig"/> (di scene) atau pada asset
/// Resources/TombolLanjutTema. Semua tombol yang sedang aktif
/// akan menerima perubahan saat itu juga (lihat <see cref="RefreshSemua"/>).
/// </summary>
public class TombolLanjutVN : MonoBehaviour
{
    private bool _diklik;

    // ── Referensi ke komponen visual milik tombol ini ─────────────────────
    // Disimpan agar TerapkanTema() bisa apply ulang saat konfigurasi
    // berubah (live edit di Play mode).
    private RectTransform     _rt;
    private Image             _img;
    private Outline           _outl;     // null bila pakaiOutline=false
    private Button            _btn;
    private TextMeshProUGUI   _label;
    private RectTransform     _labelRT;
    private string            _teksAwal; // fallback bila teksOverride kosong

    // Daftar semua instance hidup — dipakai RefreshSemua().
    private static readonly List<TombolLanjutVN> _semua = new List<TombolLanjutVN>();

    void OnEnable()  { if (!_semua.Contains(this)) _semua.Add(this); }
    void OnDisable() { _semua.Remove(this); }

    /// Dipanggil oleh Button.onClick.
    public void Klik()
    {
        _diklik = true;
        AudioManager.Instance?.Click();
    }

    /// True sekali saat tombol baru ditekan, lalu otomatis reset.
    public bool Konsumsi()
    {
        if (_diklik) { _diklik = false; return true; }
        return false;
    }

    /// Reset paksa (mis. saat ganti baris) agar klik lama tidak terbawa.
    public void Reset() => _diklik = false;

    // Ukuran & margin tetap supaya tombol SELALU sama bentuk/posisinya
    // di semua dialog (pojok kanan-bawah), tidak peduli ukuran parent-nya.
    private const float LebarTombol  = 300f;
    private const float TinggiTombol = 72f;
    private const float MarginKanan  = 36f;
    private const float MarginBawah  = 28f;

    // ── Sumber konfigurasi tampilan (3 lapis, prioritas tertinggi ke terendah) ──
    //   1) Komponen TombolLanjutKonfig di scene  — DISARANKAN (edit di Inspector).
    //   2) Asset Resources/TombolLanjutTema (ScriptableObject) — kompatibilitas.
    //   3) Nilai bawaan di kode (transparan penuh).
    private static TombolLanjutTema _tema;
    private static bool _temaDicari;
    private static TombolLanjutKonfig _konfigCache;

    /// Ambil tema aktif. Komponen di scene menang atas asset ScriptableObject.
    /// Komponen dicari ulang bila cache hilang/destroyed (mis. ganti scene).
    private static TombolLanjutTema AmbilTema()
    {
        // 1) Komponen di scene
        if (_konfigCache == null)
        {
#if UNITY_2023_1_OR_NEWER
            _konfigCache = Object.FindFirstObjectByType<TombolLanjutKonfig>(FindObjectsInactive.Include);
#else
            _konfigCache = Object.FindObjectOfType<TombolLanjutKonfig>(true);
#endif
        }
        if (_konfigCache != null) return _konfigCache.KeTema();

        // 2) Asset ScriptableObject di Resources (di-load sekali)
        if (!_temaDicari)
        {
            _tema = Resources.Load<TombolLanjutTema>("TombolLanjutTema");
            _temaDicari = true;
        }
        return _tema;
    }

    /// <summary>
    /// Buat tombol "LANJUT" di pojok kanan-bawah parent dengan ukuran TETAP
    /// agar bentuk & posisinya konsisten di seluruh dialog.
    /// Parameter <paramref name="anchorMin"/>/<paramref name="anchorMax"/> dipertahankan
    /// hanya untuk kompatibilitas pemanggil lama dan sengaja diabaikan.
    ///
    /// Semua tampilan (warna/sprite/ukuran/font/teks) bisa dikustom lewat:
    ///   • Komponen <see cref="TombolLanjutKonfig"/> di scene (DISARANKAN), atau
    ///   • Asset Resources/TombolLanjutTema (ScriptableObject).
    /// Tanpa keduanya, nilai bawaan transparan di bawah ini yang dipakai.
    /// </summary>
    public static TombolLanjutVN Pasang(
        Transform parent,
        TMP_FontAsset font = null,
        string teks = "LANJUT  \u25B6",
        Vector2? anchorMin = null,
        Vector2? anchorMax = null)
    {
        var tema = AmbilTema();

        // ── Cari Canvas root dari parent ─────────────────────────────────────
        // Tombol LANJUT dipasang sebagai anak Canvas (BUKAN anak panel dialog)
        // supaya posisinya di layar SELALU konsisten — tidak tergantung
        // ukuran/panel dialog yang berbeda-beda (NpcDialog vs Day1Intro vs dll).
        // Pojok kanan-bawah tombol = pojok kanan-bawah LAYAR + margin tetap.
        Transform anchorParent = parent;
        Canvas canvasRoot = (parent != null) ? parent.GetComponentInParent<Canvas>() : null;
        if (canvasRoot != null) anchorParent = canvasRoot.transform;

        var go = new GameObject("TombolLanjut");
        go.transform.SetParent(anchorParent, false);
        go.transform.localScale = Vector3.one; // parent = Canvas → skala 1:1

        // Sinkronkan lifecycle dengan parent ASLI (panel dialog):
        // saat panel di-hide/destroy, tombol ikut hide/destroy.
        if (parent != null && parent != anchorParent)
        {
            var binder = go.AddComponent<LifecycleBinder>();
            binder.asalParent = parent;
        }

        var rt = go.AddComponent<RectTransform>();
        // Selalu jangkar ke pojok kanan-bawah CANVAS (layar).
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot     = new Vector2(1f, 0f);

        var img = go.AddComponent<Image>();
        img.type          = Image.Type.Sliced;
        img.raycastTarget = true;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var komp = go.AddComponent<TombolLanjutVN>();
        btn.onClick.AddListener(komp.Klik);

        // Label teks ─ posisi/padding/warna di-set oleh TerapkanTema.
        var tGO = new GameObject("Label");
        tGO.transform.SetParent(go.transform, false);
        var trt = tGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        var tmp = tGO.AddComponent<TextMeshProUGUI>();
        tmp.text             = teks;
        tmp.alignment        = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = true;
        tmp.raycastTarget    = false;
        tmp.enableWordWrapping = false;
        // Font dari parameter pemanggil dipasang sebagai default; bisa
        // di-override oleh tema (lihat TerapkanTema).
        if (font != null) tmp.font = font;

        // Simpan referensi & terapkan visual awal.
        komp._rt        = rt;
        komp._img       = img;
        komp._btn       = btn;
        komp._label     = tmp;
        komp._labelRT   = trt;
        komp._teksAwal  = teks;
        komp.TerapkanTema(tema);

        return komp;
    }

    /// <summary>
    /// Terapkan ulang seluruh tampilan tombol ini berdasarkan tema.
    /// Aman dipanggil kapan saja (mis. saat Inspector berubah di Play).
    /// </summary>
    public void TerapkanTema(TombolLanjutTema tema)
    {
        if (_rt == null || _img == null) return; // belum di-Pasang

        // ── Ukuran & posisi (pojok kanan-bawah parent) ────────────────
        float lebar  = tema != null ? tema.lebar       : LebarTombol;
        float tinggi = tema != null ? tema.tinggi      : TinggiTombol;
        float mKanan = tema != null ? tema.marginKanan : MarginKanan;
        float mBawah = tema != null ? tema.marginBawah : MarginBawah;
        _rt.sizeDelta        = new Vector2(lebar, tinggi);
        _rt.anchoredPosition = new Vector2(-mKanan, mBawah);

        // ── Image (latar) ─────────────────────────────────────────────
        _img.color  = tema != null ? tema.warnaIsi : new Color(1f, 1f, 1f, 0f);
        _img.sprite = (tema != null && tema.sprite != null) ? tema.sprite : GetRoundedSprite();

        // ── Outline (add/remove dinamis sesuai toggle) ────────────────
        bool pakaiOutline = tema != null && tema.pakaiOutline;
        if (pakaiOutline)
        {
            if (_outl == null) _outl = gameObject.AddComponent<Outline>();
            _outl.effectColor    = tema.warnaOutline;
            _outl.effectDistance = tema.jarakOutline;
        }
        else if (_outl != null)
        {
            // Destroy aman di Play mode (deferred 1 frame).
            Destroy(_outl);
            _outl = null;
        }

        // ── Warna state Button (hover/press) ──────────────────────────
        if (_btn != null)
        {
            var cb = _btn.colors;
            if (tema != null)
            {
                cb.normalColor      = Color.white;
                cb.highlightedColor = tema.warnaSorot;
                cb.pressedColor     = tema.warnaTekan;
                cb.selectedColor    = Color.white;
            }
            else
            {
                cb.normalColor      = Color.white;
                cb.highlightedColor = new Color(1f, 0.98f, 0.88f, 1f);
                cb.pressedColor     = new Color(0.85f, 0.82f, 0.70f, 1f);
                cb.selectedColor    = Color.white;
            }
            cb.fadeDuration = 0.08f;
            _btn.colors = cb;
        }

        // ── Label (padding + font + warna + size) ─────────────────────
        if (_label != null)
        {
            Vector2 padKB = tema != null ? tema.paddingTeksKiriBawah : new Vector2(22f, 14f);
            Vector2 padKA = tema != null ? tema.paddingTeksKananAtas : new Vector2(22f, 14f);
            if (_labelRT != null)
            {
                _labelRT.offsetMin = new Vector2(padKB.x, padKB.y);
                _labelRT.offsetMax = new Vector2(-padKA.x, -padKA.y);
            }

            // Teks: override tema jika diisi; jika kosong, kembali ke teks awal.
            _label.text = (tema != null && !string.IsNullOrEmpty(tema.teksOverride))
                ? tema.teksOverride
                : _teksAwal;

            if (tema != null && tema.font != null) _label.font = tema.font;
            _label.color            = tema != null ? tema.warnaTeks  : new Color(0.95f, 0.91f, 0.78f, 1f);
            _label.fontSizeMin      = tema != null ? tema.fontMin    : 16f;
            _label.fontSizeMax      = tema != null ? tema.fontMax    : 24f;
            _label.fontStyle        = tema != null ? tema.gayaTeks   : FontStyles.Bold;
            _label.characterSpacing = tema != null ? tema.jarakHuruf : 4f;
        }
    }

    /// <summary>
    /// Terapkan ulang tema ke SEMUA tombol LANJUT yang sedang aktif.
    /// Dipanggil otomatis oleh TombolLanjutKonfig.OnValidate() saat Inspector
    /// diubah di Play mode, sehingga kustomisasi langsung terlihat.
    /// </summary>
    public static void RefreshSemua()
    {
        // Paksa pencarian ulang konfigurasi (komponen bisa baru ditambahkan
        // atau dihapus di scene saat Play).
        _konfigCache = null;
        var tema = AmbilTema();

        for (int i = _semua.Count - 1; i >= 0; i--)
        {
            var x = _semua[i];
            if (x == null) { _semua.RemoveAt(i); continue; }
            x.TerapkanTema(tema);
        }
    }


    // ── Sprite sudut membulat (di-cache) ──────────────────────────────────
    private static Sprite _rounded;
    private static Sprite GetRoundedSprite()
    {
        if (_rounded != null) return _rounded;
        const int S = 64, R = 16;
        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
        var px = new Color32[S * S];
        for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
            {
                bool inside = true;
                int dx = -1, dy = -1;
                if (x < R && y < R)            { dx = R - x; dy = R - y; }
                else if (x >= S - R && y < R)  { dx = x - (S - R - 1); dy = R - y; }
                else if (x < R && y >= S - R)  { dx = R - x; dy = y - (S - R - 1); }
                else if (x >= S - R && y >= S - R) { dx = x - (S - R - 1); dy = y - (S - R - 1); }
                if (dx >= 0) inside = (dx * dx + dy * dy) <= R * R;
                px[y * S + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
            }
        tex.SetPixels32(px);
        tex.Apply();
        _rounded = Sprite.Create(tex, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(R, R, R, R));
        return _rounded;
    }

    // ── Binder lifecycle: ikat visibilitas/destroy tombol ke parent dialog asli ──
    // Tombol di-parent ke Canvas (bukan ke panel) demi posisi layar yang konsisten,
    // tapi secara logis dia "milik" panel dialog tertentu. Komponen ini:
    //   • menyembunyikan tombol saat panel asli di-nonaktifkan;
    //   • menghancurkan tombol saat panel asli dihancurkan (mis. ganti scene).
    private sealed class LifecycleBinder : MonoBehaviour
    {
        public Transform asalParent;
        private bool _pernahAdaParent;

        void LateUpdate()
        {
            // Parent asli sudah hancur → ikut hancur agar tidak meninggalkan
            // tombol "yatim" di Canvas.
            if (asalParent == null)
            {
                if (_pernahAdaParent) Destroy(gameObject);
                return;
            }
            _pernahAdaParent = true;

            bool aktifkan = asalParent.gameObject.activeInHierarchy;
            if (gameObject.activeSelf != aktifkan)
                gameObject.SetActive(aktifkan);
        }
    }
}
