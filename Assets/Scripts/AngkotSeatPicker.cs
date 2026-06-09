using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// AngkotSeatPicker — Fase pilih tempat duduk di dalam angkot.
///
/// Tampilkan interior angkot dengan 3 kursi:
///   - Kursi DEKAT PINTU (AMAN)   : mudah turun & dekat supir
///   - Kursi DEPAN (RAGU)         : ramai tapi terbatas
///   - Kursi POJOK BELAKANG (BAHAYA): sepi & gelap, ada pria mencurigakan
///
/// Setelah pemain pilih:
///   - Cek plat nomor (toggle bonus +50 poin)
///   - Lanjut ke fase berikutnya
///
/// Custom semua label/warna lewat Inspector.
/// </summary>
public class AngkotSeatPicker : MonoBehaviour
{
    [System.Serializable]
    public class Kursi
    {
        public string label    = "Dekat Pintu";
        public string kategori = "AMAN"; // "AMAN" | "RAGU" | "BAHAYA"
        [TextArea(2, 4)]
        public string deskripsi = "Dekat pak supir, mudah turun cepat.";
        [TextArea(2, 4)]
        public string reaksi    = "\u2713 Pilihan tepat! Kamu duduk dekat supir.";
        public Color warna     = new Color(0.18f, 0.62f, 0.32f, 1f);
        public Vector2 posisi  = new Vector2(-450f, -50f);
        public Vector2 ukuran  = new Vector2(280f, 200f);

        [Tooltip("Sprite latar FULLSCREEN yang tampil saat kursi ini DIPILIH.\n" +
                 "Kosongkan = pakai latar reaksi per-kategori, fallback ke bgFullscreenSprite default.")]
        public Sprite latarSaatDipilih;
    }

    // -- DEPRECATED -- field interior procedural di-hide karena BG sekarang dari sprite saja.
    [HideInInspector] public Sprite angkotInteriorSprite;
    [HideInInspector] public Color warnaLantai  = new Color(0.10f, 0.07f, 0.05f, 1f);
    [HideInInspector] public Color warnaJendela = new Color(0.40f, 0.55f, 0.65f, 0.8f);
    [HideInInspector] public Color warnaBingkai = new Color(0.18f, 0.12f, 0.08f, 1f);
    [HideInInspector] public Color warnaSupir   = new Color(0.55f, 0.40f, 0.30f, 1f);

    [Header("Judul Layar")]
    public string judulTeks = "Pilih tempat dudukmu di angkot:";
    public Color  judulWarna = new Color(1f, 0.85f, 0.30f, 1f);
    public int    judulUkuran = 32;

    [Header("Daftar Kursi (CUSTOMIZABLE)")]
    public Kursi[] kursiList = new Kursi[]
    {
        new Kursi {
            label = "Dekat Pintu (depan)", kategori = "AMAN",
            deskripsi = "Dekat supir, gampang turun cepat kalau ada apa-apa.",
            reaksi    = "\u2713 Pintar! Kamu duduk dekat supir & pintu.",
            warna     = new Color(0.18f, 0.62f, 0.32f, 1f),
            posisi    = new Vector2(-500f, -40f),
            ukuran    = new Vector2(300f, 220f)
        },
        new Kursi {
            label = "Tengah (di samping ibu-ibu)", kategori = "RAGU",
            deskripsi = "Ramai tapi terjepit di tengah, susah turun.",
            reaksi    = "\u26A0 Lumayan aman, tapi posisi turun susah.",
            warna     = new Color(0.95f, 0.62f, 0.07f, 1f),
            posisi    = new Vector2(0f, -40f),
            ukuran    = new Vector2(300f, 220f)
        },
        new Kursi {
            label = "Pojok Belakang (sepi)", kategori = "BAHAYA",
            deskripsi = "Sepi & gelap, ada pria asing yang ngeliatin kamu.",
            reaksi    = "\u2716 Bahaya! Pria asing langsung mendekat. Kamu kehilangan 1 nyawa.",
            warna     = new Color(0.91f, 0.30f, 0.24f, 1f),
            posisi    = new Vector2(500f, -40f),
            ukuran    = new Vector2(300f, 220f)
        }
    };

    [Header("Cek Plat Nomor (Bonus)")]
    [Tooltip("Aktifkan checkbox 'Cek plat nomor angkot' untuk bonus poin.")]
    public bool tampilkanCekPlat = true;
    public string platLabel = "\uD83D\uDCDD Catat plat nomor angkot (B 1234 XYZ)";
    public int    bonusPlat = 50;

    [Header("Tombol Lanjut")]
    public string tombolLanjutTeks = "\u25B6  Lanjut Perjalanan";
    public Color  warnaLanjut      = new Color(0.20f, 0.62f, 0.86f, 1f);

    [Header("BG Fullscreen Device (opsional)")]
    [Tooltip("Sprite latar FULLSCREEN device (stretch ke seluruh layar). Tampil paling belakang.\n" +
             "Kalau diisi → dipakai sebagai BG utama. Kalau kosong → fallback ke angkotInteriorSprite / procedural.")]
    public Sprite bgFullscreenSprite;
    [Tooltip("Jaga aspek rasio sprite saat di-stretch fullscreen (mencegah gepeng).")]
    public bool   bgFullscreenPreserveAspect = false;

    [Header("BG Reaksi per Kategori (opsional)")]
    [Tooltip("BG fullscreen yang tampil setelah pemain memilih kursi AMAN.\n" +
             "Kosongkan = tetap pakai bgFullscreenSprite default.")]
    public Sprite bgReaksiAman;
    [Tooltip("BG fullscreen yang tampil setelah pemain memilih kursi RAGU.")]
    public Sprite bgReaksiRagu;
    [Tooltip("BG fullscreen yang tampil setelah pemain memilih kursi BAHAYA (pria asing dekat).")]
    public Sprite bgReaksiBahaya;

    [Header("Font")]
    public TMP_FontAsset fontAsset;

    [Header("Sorting")]
    public int sortingOrder = 920;

    // ── runtime ───────────────────────────────────────────────────────────
    private Action     _onSelesai;
    private GameObject _canvasGO;
    private TextMeshProUGUI _reaksiText;
    private Image           _bgFullscreenImg;
    private GameObject _kursiPanel;
    private GameObject _platRow;
    private GameObject _lanjutBtn;
    private Sprite     _roundedSprite;
    private bool       _platDicek;

    // ══════════════════════════════════════════════════════════════════════
    public void Mulai(Action onSelesai)
    {
        _onSelesai = onSelesai;
        BuildScene();
    }

    // Ambil sprite awal BG: bgFullscreenSprite > latarSaatDipilih kursi pertama > bgReaksiAman > bgReaksiRagu > bgReaksiBahaya.
    Sprite AmbilSpriteAwal()
    {
        if (bgFullscreenSprite != null) return bgFullscreenSprite;
        if (kursiList != null)
            foreach (var k in kursiList) if (k != null && k.latarSaatDipilih != null) return k.latarSaatDipilih;
        if (bgReaksiAman   != null) return bgReaksiAman;
        if (bgReaksiRagu   != null) return bgReaksiRagu;
        if (bgReaksiBahaya != null) return bgReaksiBahaya;
        return null;
    }

    // ══════════════════════════════════════════════════════════════════════
    void BuildScene()
    {
        _canvasGO = new GameObject("AngkotSeatPicker_Canvas");
        var canvas = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = _canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        _canvasGO.AddComponent<GraphicRaycaster>();

        // BG fullscreen SELALU dibuat (1 Image stretch fullscreen).
        // Sprite awal: bgFullscreenSprite > Kursi.latarSaatDipilih[0] > bgReaksi*. Diganti runtime saat pemain memilih kursi.
        {
            var fs = new GameObject("BG_Fullscreen");
            fs.transform.SetParent(_canvasGO.transform, false);
            _bgFullscreenImg = fs.AddComponent<Image>();
            _bgFullscreenImg.sprite         = AmbilSpriteAwal();
            _bgFullscreenImg.color          = Color.white;
            _bgFullscreenImg.preserveAspect = false;
            _bgFullscreenImg.raycastTarget  = false;
            var fsRt = fs.GetComponent<RectTransform>();
            fsRt.anchorMin = Vector2.zero; fsRt.anchorMax = Vector2.one;
            fsRt.offsetMin = Vector2.zero; fsRt.offsetMax = Vector2.zero;
        }

        // Judul
        var judul = BuatTeks(_canvasGO.transform, "Judul", judulTeks, judulUkuran, judulWarna, FontStyles.Bold);
        judul.alignment = TextAlignmentOptions.Center;
        var jrt = judul.rectTransform;
        jrt.anchorMin = new Vector2(0f, 1f); jrt.anchorMax = new Vector2(1f, 1f);
        jrt.pivot     = new Vector2(0.5f, 1f);
        jrt.offsetMin = new Vector2(40f, -110f);
        jrt.offsetMax = new Vector2(-40f, -25f);

        // Panel kursi
        _kursiPanel = new GameObject("KursiPanel");
        _kursiPanel.transform.SetParent(_canvasGO.transform, false);
        var krt = _kursiPanel.AddComponent<RectTransform>();
        krt.anchorMin = new Vector2(0.5f, 0.5f); krt.anchorMax = new Vector2(0.5f, 0.5f);
        krt.pivot = new Vector2(0.5f, 0.5f);
        krt.sizeDelta = new Vector2(1700f, 500f);
        krt.anchoredPosition = new Vector2(0f, 60f);

        foreach (var k in kursiList) BuatKursiButton(k);

        // Reaksi area (initially kosong)
        _reaksiText = BuatTeks(_canvasGO.transform, "Reaksi", "", 24, new Color(1f,1f,0.92f,1f), FontStyles.Normal);
        _reaksiText.alignment = TextAlignmentOptions.Center;
        var rrt = _reaksiText.rectTransform;
        rrt.anchorMin = new Vector2(0f, 0f); rrt.anchorMax = new Vector2(1f, 0f);
        rrt.pivot     = new Vector2(0.5f, 0f);
        rrt.offsetMin = new Vector2(80f, 230f);
        rrt.offsetMax = new Vector2(-80f, 380f);

        // Plat toggle row (hidden until choice made)
        if (tampilkanCekPlat) BuildPlatRow();
    }

    void BuildInteriorProcedural()
    {
        // Lantai
        var lantai = new GameObject("Lantai");
        lantai.transform.SetParent(_canvasGO.transform, false);
        var lImg = lantai.AddComponent<Image>();
        lImg.color = warnaLantai;
        lImg.raycastTarget = false;
        var lrt = lantai.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0f, 0f); lrt.anchorMax = new Vector2(1f, 0.35f);
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;

        // Jendela kiri
        BuatKotak("JendelaKiri", new Vector2(-680f, 220f), new Vector2(420f, 240f), warnaJendela);
        BuatKotak("BingkaiJndKiri", new Vector2(-680f, 220f), new Vector2(440f, 260f), warnaBingkai, true);

        // Jendela kanan
        BuatKotak("JendelaKanan", new Vector2(680f, 220f), new Vector2(420f, 240f), warnaJendela);
        BuatKotak("BingkaiJndKanan", new Vector2(680f, 220f), new Vector2(440f, 260f), warnaBingkai, true);

        // Supir di kiri atas (kepala)
        BuatKotak("Supir", new Vector2(-820f, 60f), new Vector2(110f, 130f), warnaSupir);
        var sLabel = BuatTeks(_canvasGO.transform, "SupirLabel", "Supir", 16, new Color(1f, 0.95f, 0.75f, 1f), FontStyles.Italic);
        sLabel.alignment = TextAlignmentOptions.Center;
        var slrt = sLabel.rectTransform;
        slrt.anchorMin = new Vector2(0.5f, 0.5f); slrt.anchorMax = new Vector2(0.5f, 0.5f);
        slrt.pivot = new Vector2(0.5f, 0.5f); slrt.sizeDelta = new Vector2(120f, 22f);
        slrt.anchoredPosition = new Vector2(-820f, -20f);
    }

    void BuatKotak(string name, Vector2 pos, Vector2 size, Color c, bool isBorder = false)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_canvasGO.transform, false);
        var img = go.AddComponent<Image>();
        img.color = c;
        img.raycastTarget = false;
        if (isBorder)
        {
            img.color = new Color(0,0,0,0);
            var outl = go.AddComponent<Outline>();
            outl.effectColor = c;
            outl.effectDistance = new Vector2(3f, -3f);
        }
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
    }

    void BuatKursiButton(Kursi k)
    {
        var go = new GameObject("Kursi_" + k.label);
        go.transform.SetParent(_kursiPanel.transform, false);
        var img = go.AddComponent<Image>();
        img.sprite = GetRoundedSprite();
        img.color  = k.warna;
        img.type   = Image.Type.Sliced;
        var outl = go.AddComponent<Outline>();
        outl.effectColor    = new Color(1f, 1f, 1f, 0.35f);
        outl.effectDistance = new Vector2(2f, -2f);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = k.ukuran;
        rt.anchoredPosition = k.posisi;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.highlightedColor = new Color(Mathf.Min(1f, k.warna.r * 1.15f), Mathf.Min(1f, k.warna.g * 1.15f), Mathf.Min(1f, k.warna.b * 1.15f), k.warna.a);
        colors.pressedColor     = new Color(k.warna.r * 0.85f, k.warna.g * 0.85f, k.warna.b * 0.85f, k.warna.a);
        btn.colors = colors;
        btn.onClick.AddListener(() => OnPilihKursi(k));

        // Label
        var lab = BuatTeks(go.transform, "Label", k.label, 22, Color.white, FontStyles.Bold);
        lab.alignment = TextAlignmentOptions.Center;
        var lrt = lab.rectTransform;
        lrt.anchorMin = new Vector2(0f, 0.5f); lrt.anchorMax = new Vector2(1f, 1f);
        lrt.offsetMin = new Vector2(8f, 8f);
        lrt.offsetMax = new Vector2(-8f, -8f);

        // Deskripsi
        var desc = BuatTeks(go.transform, "Desc", k.deskripsi, 16, new Color(1f,1f,1f,0.92f), FontStyles.Normal);
        desc.alignment = TextAlignmentOptions.Center;
        var drt = desc.rectTransform;
        drt.anchorMin = new Vector2(0f, 0f); drt.anchorMax = new Vector2(1f, 0.5f);
        drt.offsetMin = new Vector2(12f, 8f);
        drt.offsetMax = new Vector2(-12f, -4f);
    }

    void OnPilihKursi(Kursi k)
    {
        AudioManager.Instance?.Click();

        // Nonaktifkan semua tombol kursi
        foreach (Transform t in _kursiPanel.transform)
        {
            var b = t.GetComponent<Button>(); if (b != null) b.interactable = false;
            var img = t.GetComponent<Image>(); if (img != null && t.gameObject != FindKursiGO(k)) img.color = new Color(img.color.r, img.color.g, img.color.b, 0.35f);
        }

        // Catat ke GameState
        var gs = GameState.Instance;
        if (gs != null)
        {
            gs.AddChoice(2, "Pilih kursi: " + k.label, k.kategori);
            // Simpan kategori kursi supaya fase berikutnya (ZonaTubuhQuiz)
            // bisa memilih varian narasi yang menyambung pilihan pemain.
            gs.seatCategory = k.kategori;
            if (k.kategori == "BAHAYA")
            {
                gs.lives = Mathf.Max(0, gs.lives - 1);
                Debug.Log($"[AngkotSeatPicker] Pilih BAHAYA \u2192 nyawa -1 (sisa {gs.lives})");
            }
        }

        AudioClip sfx = k.kategori switch
        {
            "AMAN"   => AudioManager.Instance?.sfxAman,
            "RAGU"   => AudioManager.Instance?.sfxRagu,
            "BAHAYA" => AudioManager.Instance?.sfxBahaya,
            _        => null
        };
        if (sfx != null) AudioManager.Instance.sfxSource.PlayOneShot(sfx);

        // Ganti BG fullscreen sesuai prioritas: kursi.latarSaatDipilih → bgReaksi<Kategori> → tetap default.
        if (_bgFullscreenImg != null)
        {
            Sprite spriteReaksi = k.latarSaatDipilih;
            if (spriteReaksi == null)
            {
                spriteReaksi = k.kategori switch
                {
                    "AMAN"   => bgReaksiAman,
                    "RAGU"   => bgReaksiRagu,
                    "BAHAYA" => bgReaksiBahaya,
                    _        => null
                };
            }
            if (spriteReaksi != null)
            {
                _bgFullscreenImg.sprite         = spriteReaksi;
                _bgFullscreenImg.color          = Color.white;
                _bgFullscreenImg.preserveAspect = false;
                // Pastikan tetap fullscreen stretch
                var bgRt = _bgFullscreenImg.rectTransform;
                bgRt.anchorMin = Vector2.zero;
                bgRt.anchorMax = Vector2.one;
                bgRt.offsetMin = Vector2.zero;
                bgRt.offsetMax = Vector2.zero;
                _bgFullscreenImg.transform.SetAsFirstSibling();
            }
        }

        _reaksiText.text = k.reaksi;

        // Tampilkan plat row + tombol lanjut
        if (_platRow != null) _platRow.SetActive(true);
        if (_lanjutBtn == null) BuildTombolLanjut();
    }

    GameObject FindKursiGO(Kursi k)
    {
        var t = _kursiPanel.transform.Find("Kursi_" + k.label);
        return t != null ? t.gameObject : null;
    }

    void BuildPlatRow()
    {
        _platRow = new GameObject("PlatRow");
        _platRow.transform.SetParent(_canvasGO.transform, false);
        var rt = _platRow.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f); rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(900f, 70f);
        rt.anchoredPosition = new Vector2(0f, 140f);

        var bg = _platRow.AddComponent<Image>();
        bg.sprite = GetRoundedSprite();
        bg.color  = new Color(0.10f, 0.10f, 0.12f, 0.85f);
        bg.type   = Image.Type.Sliced;

        var hLay = _platRow.AddComponent<HorizontalLayoutGroup>();
        hLay.childAlignment = TextAnchor.MiddleLeft;
        hLay.spacing = 16f;
        hLay.padding = new RectOffset(20, 20, 8, 8);

        // Toggle box
        var box = new GameObject("Box");
        box.transform.SetParent(_platRow.transform, false);
        var bImg = box.AddComponent<Image>();
        bImg.sprite = GetRoundedSprite();
        bImg.color  = new Color(0.95f, 0.95f, 0.95f, 1f);
        bImg.type   = Image.Type.Sliced;
        var bLe = box.AddComponent<LayoutElement>();
        bLe.preferredWidth = 40f; bLe.preferredHeight = 40f;
        var bBtn = box.AddComponent<Button>();
        bBtn.targetGraphic = bImg;

        var check = BuatTeks(box.transform, "Check", "", 28, new Color(0.15f, 0.68f, 0.38f, 1f), FontStyles.Bold);
        check.alignment = TextAlignmentOptions.Center;
        var crt = check.rectTransform;
        crt.anchorMin = Vector2.zero; crt.anchorMax = Vector2.one;
        crt.offsetMin = Vector2.zero; crt.offsetMax = Vector2.zero;

        // Label
        var lab = BuatTeks(_platRow.transform, "Label", platLabel + $"  (+{bonusPlat} poin)", 20, new Color(1f, 0.95f, 0.85f, 1f), FontStyles.Normal);
        lab.alignment = TextAlignmentOptions.MidlineLeft;
        var lLe = lab.gameObject.AddComponent<LayoutElement>();
        lLe.preferredWidth = 760f; lLe.preferredHeight = 40f;

        bBtn.onClick.AddListener(() =>
        {
            _platDicek = !_platDicek;
            check.text = _platDicek ? "\u2713" : "";
            AudioManager.Instance?.Click();
            var gs = GameState.Instance;
            if (gs != null)
            {
                gs.platChecked = _platDicek;
                if (_platDicek) gs.score += bonusPlat;
                else gs.score = Mathf.Max(0, gs.score - bonusPlat);
            }
        });
    }

    void BuildTombolLanjut()
    {
        _lanjutBtn = new GameObject("LanjutBtn");
        _lanjutBtn.transform.SetParent(_canvasGO.transform, false);
        var img = _lanjutBtn.AddComponent<Image>();
        img.sprite = GetRoundedSprite();
        img.color  = warnaLanjut;
        img.type   = Image.Type.Sliced;
        var outl = _lanjutBtn.AddComponent<Outline>();
        outl.effectColor    = new Color(1f, 1f, 1f, 0.4f);
        outl.effectDistance = new Vector2(2f, -2f);
        var rt = _lanjutBtn.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f); rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(340f, 68f);
        rt.anchoredPosition = new Vector2(0f, 50f);

        var btn = _lanjutBtn.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() =>
        {
            AudioManager.Instance?.Click();
            if (_canvasGO != null) Destroy(_canvasGO);
            _onSelesai?.Invoke();
        });

        var lab = BuatTeks(_lanjutBtn.transform, "Label", tombolLanjutTeks, 24, Color.white, FontStyles.Bold);
        lab.alignment = TextAlignmentOptions.Center;
        var lrt = lab.rectTransform;
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
    }

    // ══════════════════════════════════════════════════════════════════════
    TextMeshProUGUI BuatTeks(Transform parent, string name, string content, int size, Color color, FontStyles style)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) tmp.font = fontAsset;
        else if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
        tmp.text = content; tmp.fontSize = size; tmp.color = color; tmp.fontStyle = style;
        tmp.textWrappingMode = TextWrappingModes.Normal; tmp.raycastTarget = false;
        return tmp;
    }

    Sprite GetRoundedSprite()
    {
        if (_roundedSprite != null) return _roundedSprite;
        int size = 64; int radius = 14;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp; tex.filterMode = FilterMode.Bilinear;
        Color32 w = new Color32(255,255,255,255), c = new Color32(255,255,255,0);
        for (int y=0;y<size;y++) for (int x=0;x<size;x++)
        {
            bool inside = true;
            if      (x<radius && y<radius)             { int dx=radius-x, dy=radius-y; inside = dx*dx+dy*dy <= radius*radius; }
            else if (x>=size-radius && y<radius)       { int dx=x-(size-1-radius), dy=radius-y; inside = dx*dx+dy*dy <= radius*radius; }
            else if (x<radius && y>=size-radius)       { int dx=radius-x, dy=y-(size-1-radius); inside = dx*dx+dy*dy <= radius*radius; }
            else if (x>=size-radius && y>=size-radius) { int dx=x-(size-1-radius), dy=y-(size-1-radius); inside = dx*dx+dy*dy <= radius*radius; }
            tex.SetPixel(x, y, inside ? (Color)w : (Color)c);
        }
        tex.Apply();
        _roundedSprite = Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius,radius,radius,radius));
        return _roundedSprite;
    }
}
