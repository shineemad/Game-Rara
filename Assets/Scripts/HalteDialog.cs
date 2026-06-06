using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HalteDialog — Fase Halte Day 2.
///
/// Menampilkan halte angkot dengan NPC "Pria Asing" mendekati Rara,
/// kemudian dialog 2 baris diikuti pilihan AMAN / RAGU / BAHAYA.
///
/// Pilihan AMAN  : "Maaf, saya nggak kenal." (mundur, dapat poin)
/// Pilihan RAGU  : "Hmm... saya mikir dulu."
/// Pilihan BAHAYA: "Boleh, ke mana om?" (kehilangan nyawa)
///
/// Semua dibangun procedural \u2014 tidak butuh prefab.
/// Custom semua teks/warna lewat Inspector.
/// </summary>
public class HalteDialog : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR
    // ══════════════════════════════════════════════════════════════════════

    [Header("Sprite (opsional)")]
    public Sprite raraSprite;
    public Sprite priaAsingSprite;
    public Sprite halteBackgroundSprite;

    [Header("─ Box Dialog (sama dengan Day1Intro/Day2NarasiAwal) ─")]
    [Tooltip("Sprite panel kayu untuk kotak dialog (sliced). Kosong = pakai panel rounded fallback.")]
    public Sprite panelSprite;
    [Tooltip("Path sprite panel (relatif Assets/) untuk auto-load saat Reset.")]
    public string panelSpritePath = "sprites/UI day 1/8.png";
    [Tooltip("Warna panel saat panelSprite di-assign.")]
    public Color  panelTint = Color.white;

    [Header("Background Halte")]
    public Color   warnaLatar       = new Color(0.45f, 0.62f, 0.78f, 1f);
    public Color   warnaAtap        = new Color(0.30f, 0.20f, 0.12f, 1f);
    public Color   warnaTiang       = new Color(0.25f, 0.18f, 0.10f, 1f);
    public Color   warnaPapanInfo   = new Color(0.20f, 0.50f, 0.30f, 1f);

    [Header("Dialog \u2014 baris awal Pria Asing")]
    [Tooltip("Baris dialog sebelum pilihan muncul. Akan ditampilkan satu per satu.")]
    public List<string> dialogAwal = new List<string>
    {
        "Hai dek... kamu sendirian? Mau bareng om aja?",
        "Tenang, om temennya papa kamu. Pulang sekolah om anter ya?"
    };

    [Header("Pilihan Pemain")]
    public string pilihanAman   = "Maaf, saya nggak kenal Om.";
    public string pilihanRagu   = "Hmm... saya mikir dulu...";
    public string pilihanBahaya = "Boleh, ke mana Om?";

    [Header("Reaksi Setelah Pilih")]
    [TextArea(2,4)] public string reaksiAman   = "\u2713 Bagus! Kamu menjauh & cari tempat ramai.\nPak supir & ibu-ibu di halte ngeliatin om itu. Dia kabur.";
    [TextArea(2,4)] public string reaksiRagu   = "\u26A0 Om itu makin mendekat & ngotot. Lain kali, tegas tolak ya!";
    [TextArea(2,4)] public string reaksiBahaya = "\u2716 GAWAT! Untung ada ibu-ibu yang nyadar & teriak.\nKamu kehilangan 1 nyawa karena ambil keputusan berisiko.";

    [Header("Skor & Nyawa")]
    [Tooltip("Tambahkan poin LAPOR bonus saat AMAN.")]
    public bool berikanBonusLaporSaatAman = false;
    [Tooltip("Kurangi nyawa saat pilih BAHAYA.")]
    public bool kurangiNyawaSaatBahaya = true;

    [Header("Tombol Lanjut Setelah Reaksi")]
    public string tombolLanjutTeks = "\u25B6  Naik angkot";

    [Header("Warna Tombol Pilihan")]
    public Color warnaAman   = new Color(0.15f, 0.68f, 0.38f, 1f);
    public Color warnaRagu   = new Color(0.95f, 0.62f, 0.07f, 1f);
    public Color warnaBahaya = new Color(0.91f, 0.30f, 0.24f, 1f);
    public Color warnaNetral = new Color(0.20f, 0.62f, 0.86f, 1f);

    [Header("Font")]
    public TMP_FontAsset fontAsset;

    [Header("Sorting")]
    public int sortingOrder = 920;

    // ── runtime ───────────────────────────────────────────────────────────
    private Action     _onSelesai;
    private GameObject _canvasGO;
    private GameObject _dialogBoxGO;
    private TextMeshProUGUI _dialogText;
    private GameObject _pilihanRowGO;
    private Sprite     _roundedSprite;

    // ══════════════════════════════════════════════════════════════════════
    public void Mulai(Action onSelesai)
    {
        _onSelesai = onSelesai;
        BuildHalteScene();
        StartCoroutine(JalankanDialog());
    }

    // ══════════════════════════════════════════════════════════════════════
    void BuildHalteScene()
    {
        _canvasGO = new GameObject("HalteDialog_Canvas");
        var canvas = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = _canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        _canvasGO.AddComponent<GraphicRaycaster>();

        // ── HALTE (procedural atau sprite) ───────────────────────────────
        if (halteBackgroundSprite != null)
        {
            var bg = new GameObject("HalteBG");
            bg.transform.SetParent(_canvasGO.transform, false);
            var img = bg.AddComponent<Image>();
            img.sprite = halteBackgroundSprite;
            img.color  = Color.white;
            img.preserveAspect = true;
            img.raycastTarget = false;
            var rt = bg.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }
        else
        {
            BuildHalteProcedural();
        }

        // ── KARAKTER ─────────────────────────────────────────────────────
        BuildKarakter("Rara", raraSprite, new Vector2(-380f, -120f), new Color(1f, 0.8f, 0.85f, 1f));
        BuildKarakter("PriaAsing", priaAsingSprite, new Vector2(380f, -120f), new Color(0.35f, 0.20f, 0.25f, 1f));

        // ── KOTAK DIALOG ─────────────────────────────────────────────────
        BuildDialogBox();
    }

    void BuildHalteProcedural()
    {
        // Tiang kiri
        BuatBentuk("TiangKiri", new Vector2(-720f, 0f), new Vector2(34f, 600f), warnaTiang);
        // Tiang kanan
        BuatBentuk("TiangKanan", new Vector2(720f, 0f), new Vector2(34f, 600f), warnaTiang);
        // Atap
        BuatBentuk("Atap", new Vector2(0f, 290f), new Vector2(1500f, 50f), warnaAtap);
        // Bangku (di belakang karakter)
        BuatBentuk("Bangku", new Vector2(0f, -240f), new Vector2(1400f, 26f), warnaTiang);
        // Papan info halte (kotak hijau di atap kiri)
        BuatBentuk("PapanInfo", new Vector2(-540f, 200f), new Vector2(280f, 110f), warnaPapanInfo);
        var papanLabel = BuatTeks(_canvasGO.transform, "PapanLabel", "HALTE\nANGKOT", 22, Color.white, FontStyles.Bold);
        papanLabel.alignment = TextAlignmentOptions.Center;
        var prt = papanLabel.rectTransform;
        prt.anchorMin = new Vector2(0.5f, 0.5f);
        prt.anchorMax = new Vector2(0.5f, 0.5f);
        prt.pivot     = new Vector2(0.5f, 0.5f);
        prt.sizeDelta = new Vector2(280f, 110f);
        prt.anchoredPosition = new Vector2(-540f, 200f);
    }

    void BuatBentuk(string name, Vector2 pos, Vector2 size, Color c)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_canvasGO.transform, false);
        var img = go.AddComponent<Image>();
        img.color = c;
        img.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
    }

    void BuildKarakter(string name, Sprite spr, Vector2 pos, Color fallbackColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_canvasGO.transform, false);
        var img = go.AddComponent<Image>();
        if (spr != null)
        {
            img.sprite = spr;
            img.color  = Color.white;
            img.preserveAspect = true;
        }
        else
        {
            img.sprite = GetRoundedSprite();
            img.color  = fallbackColor;
            img.type   = Image.Type.Sliced;
        }
        img.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = spr != null ? new Vector2(360f, 480f) : new Vector2(200f, 340f);
        rt.anchoredPosition = pos;

        // Label nama di bawah karakter
        var labelGO = new GameObject(name + "_Label");
        labelGO.transform.SetParent(_canvasGO.transform, false);
        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) tmp.font = fontAsset;
        else if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
        tmp.text = name == "PriaAsing" ? "Pria Asing" : name;
        tmp.fontSize  = 22;
        tmp.color     = new Color(1f, 0.95f, 0.75f, 1f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;
        var lrt = tmp.rectTransform;
        lrt.anchorMin = new Vector2(0.5f, 0.5f);
        lrt.anchorMax = new Vector2(0.5f, 0.5f);
        lrt.pivot     = new Vector2(0.5f, 0.5f);
        lrt.sizeDelta = new Vector2(300f, 40f);
        lrt.anchoredPosition = new Vector2(pos.x, pos.y - 200f);
    }

    void BuildDialogBox()
    {
        _dialogBoxGO = new GameObject("DialogBox");
        _dialogBoxGO.transform.SetParent(_canvasGO.transform, false);
        var img = _dialogBoxGO.AddComponent<Image>();
        if (panelSprite != null)
        {
            // Pakai sprite kayu (sama dengan Day1Intro/Day2NarasiAwal)
            img.sprite = panelSprite;
            img.color  = panelTint;
            img.type   = Image.Type.Sliced;
        }
        else
        {
            // Fallback panel rounded gelap
            img.sprite = GetRoundedSprite();
            img.color  = new Color(0.05f, 0.08f, 0.12f, 0.94f);
            img.type   = Image.Type.Sliced;
            var outl = _dialogBoxGO.AddComponent<Outline>();
            outl.effectColor    = new Color(1f, 0.85f, 0.25f, 1f);
            outl.effectDistance = new Vector2(2f, -2f);
        }
        var rt = _dialogBoxGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot     = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(1500f, 290f);
        rt.anchoredPosition = new Vector2(0f, 30f);

        _dialogText = BuatTeks(_dialogBoxGO.transform, "Text", "",
                               26, new Color(1f, 1f, 0.92f, 1f), FontStyles.Normal);
        _dialogText.alignment = TextAlignmentOptions.TopLeft;
        var trt = _dialogText.rectTransform;
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(40f, 100f);
        trt.offsetMax = new Vector2(-40f, -25f);

        // Row pilihan (akan di-populate nanti)
        _pilihanRowGO = new GameObject("PilihanRow");
        _pilihanRowGO.transform.SetParent(_dialogBoxGO.transform, false);
        var prt = _pilihanRowGO.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(0f, 0f);
        prt.anchorMax = new Vector2(1f, 0f);
        prt.pivot     = new Vector2(0.5f, 0f);
        prt.offsetMin = new Vector2(30f, 18f);
        prt.offsetMax = new Vector2(-30f, 90f);
        var hLay = _pilihanRowGO.AddComponent<HorizontalLayoutGroup>();
        hLay.childAlignment = TextAnchor.MiddleCenter;
        hLay.spacing = 20f;
        hLay.childForceExpandWidth = true;
        hLay.childForceExpandHeight = true;
    }

    // ══════════════════════════════════════════════════════════════════════
    IEnumerator JalankanDialog()
    {
        // Tampilkan dialog awal satu per satu (klik / tap untuk next)
        foreach (var line in dialogAwal)
        {
            yield return TampilkanBaris("Pria Asing", line);
            yield return TungguTap();
        }

        // Tampilkan pilihan
        yield return TampilkanPilihan();
    }

    IEnumerator TampilkanBaris(string speaker, string line)
    {
        _dialogText.text = $"<b><color=#FFB6B6>{speaker}:</color></b>\n{line}";
        // Tap hint
        yield return null;
    }

    IEnumerator TungguTap()
    {
        // Klik di mana saja layar untuk lanjut (selama belum ada pilihan)
        while (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.Return))
            yield return null;
        AudioManager.Instance?.Click();
        yield return new WaitForSeconds(0.05f);
    }

    IEnumerator TampilkanPilihan()
    {
        _dialogText.text = "<i><color=#FFD700>Pilih responmu:</color></i>";

        bool dipilih = false;
        string kategori = "";
        string label    = "";
        string reaksi   = "";

        BuatTombolPilihan(pilihanAman,   warnaAman,   () => { kategori = "AMAN";   label = pilihanAman;   reaksi = reaksiAman;   dipilih = true; });
        BuatTombolPilihan(pilihanRagu,   warnaRagu,   () => { kategori = "RAGU";   label = pilihanRagu;   reaksi = reaksiRagu;   dipilih = true; });
        BuatTombolPilihan(pilihanBahaya, warnaBahaya, () => { kategori = "BAHAYA"; label = pilihanBahaya; reaksi = reaksiBahaya; dipilih = true; });

        while (!dipilih) yield return null;

        // Hapus tombol
        foreach (Transform t in _pilihanRowGO.transform) Destroy(t.gameObject);

        // Catat ke GameState
        var gs = GameState.Instance;
        if (gs != null)
        {
            gs.AddChoice(2, label, kategori);
            if (kategori == "AMAN" && berikanBonusLaporSaatAman)
            {
                gs.score += GameState.SCORE_LAPOR;
                Debug.Log($"[HalteDialog] Bonus LAPOR +{GameState.SCORE_LAPOR}");
            }
            if (kategori == "BAHAYA" && kurangiNyawaSaatBahaya)
            {
                gs.lives = Mathf.Max(0, gs.lives - 1);
                Debug.Log($"[HalteDialog] Nyawa -1 (sisa {gs.lives})");
            }
        }

        AudioClip sfx = kategori switch
        {
            "AMAN"   => AudioManager.Instance?.sfxAman,
            "RAGU"   => AudioManager.Instance?.sfxRagu,
            "BAHAYA" => AudioManager.Instance?.sfxBahaya,
            _        => null
        };
        if (sfx != null) AudioManager.Instance.sfxSource.PlayOneShot(sfx);

        // Tampilkan reaksi
        _dialogText.text = $"<b><color=#FFD700>Reaksi:</color></b>\n{reaksi}";

        // Tombol Lanjut
        bool lanjut = false;
        BuatTombolPilihan(tombolLanjutTeks, warnaNetral, () => { lanjut = true; });
        while (!lanjut) yield return null;

        // Cleanup & callback
        if (_canvasGO != null) Destroy(_canvasGO);
        _onSelesai?.Invoke();
    }

    void BuatTombolPilihan(string teks, Color warna, Action onClick)
    {
        var go = new GameObject("Tombol_" + teks);
        go.transform.SetParent(_pilihanRowGO.transform, false);
        var img = go.AddComponent<Image>();
        img.sprite = GetRoundedSprite();
        img.color  = warna;
        img.type   = Image.Type.Sliced;
        var outl = go.AddComponent<Outline>();
        outl.effectColor    = new Color(1f, 1f, 1f, 0.35f);
        outl.effectDistance = new Vector2(2f, -2f);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.highlightedColor = new Color(Mathf.Min(1f, warna.r * 1.18f), Mathf.Min(1f, warna.g * 1.18f), Mathf.Min(1f, warna.b * 1.18f), warna.a);
        colors.pressedColor     = new Color(warna.r * 0.85f, warna.g * 0.85f, warna.b * 0.85f, warna.a);
        btn.colors = colors;
        btn.onClick.AddListener(() =>
        {
            AudioManager.Instance?.Click();
            onClick?.Invoke();
        });

        var t = BuatTeks(go.transform, "Label", teks, 22, Color.white, FontStyles.Bold);
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        var trt = t.rectTransform;
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(12f, 4f);
        trt.offsetMax = new Vector2(-12f, -4f);
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
        tmp.text      = content;
        tmp.fontSize  = size;
        tmp.color     = color;
        tmp.fontStyle = style;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
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
            if      (x<radius && y<radius)               { int dx=radius-x, dy=radius-y; inside = dx*dx+dy*dy <= radius*radius; }
            else if (x>=size-radius && y<radius)         { int dx=x-(size-1-radius), dy=radius-y; inside = dx*dx+dy*dy <= radius*radius; }
            else if (x<radius && y>=size-radius)         { int dx=radius-x, dy=y-(size-1-radius); inside = dx*dx+dy*dy <= radius*radius; }
            else if (x>=size-radius && y>=size-radius)   { int dx=x-(size-1-radius), dy=y-(size-1-radius); inside = dx*dx+dy*dy <= radius*radius; }
            tex.SetPixel(x, y, inside ? (Color)w : (Color)c);
        }
        tex.Apply();
        _roundedSprite = Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius,radius,radius,radius));
        return _roundedSprite;
    }

#if UNITY_EDITOR
    // ══════════════════════════════════════════════════════════════════════
    // EDITOR HELPERS — auto-load sprite box dialog & portrait
    // ══════════════════════════════════════════════════════════════════════
    void Reset()
    {
        TryLoadSprites();
    }

    [ContextMenu("▶ Muat Sprite Box Dialog + Portrait")]
    void MuatSpriteMenu()
    {
        TryLoadSprites();
        Debug.Log($"[HalteDialog] panelSprite={(panelSprite != null ? panelSprite.name : "null")} " +
                  $"raraSprite={(raraSprite != null ? raraSprite.name : "null")} " +
                  $"priaAsingSprite={(priaAsingSprite != null ? priaAsingSprite.name : "null")}");
        UnityEditor.EditorUtility.SetDirty(this);
    }

    void TryLoadSprites()
    {
        if (panelSprite == null && !string.IsNullOrEmpty(panelSpritePath))
        {
            var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/" + panelSpritePath);
            if (sp != null) panelSprite = sp;
        }
        // Coba ambil portraitRara dari Day1Intro di scene
        if (raraSprite == null)
        {
            var day1 = FindFirstObjectByType<Day1Intro>();
            if (day1 != null && day1.portraitRara != null) raraSprite = day1.portraitRara;
        }
    }
#endif
}
