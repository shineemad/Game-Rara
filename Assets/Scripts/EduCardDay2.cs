using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// EduCardDay2 — Kartu Edukasi Hari 2 (FULLY CUSTOMIZABLE).
///
/// Dipanggil setelah LaporTeriakButton selesai (via Day2Controller),
/// atau secara manual via Tampilkan() / TampilkanStatik().
///
/// Setelah pemain klik LANJUT, akan otomatis cari Day2SummaryScreen
/// di scene & panggil Tampilkan().
/// </summary>
public class EduCardDay2 : MonoBehaviour
{
    public static EduCardDay2 Instance { get; private set; }

    [Header("Auto-Tampil")]
    public bool autoTampilSaatStart = false;

    [Header("Background Kartu")]
    public Sprite backgroundSprite;
    public Image.Type backgroundImageType = Image.Type.Sliced;
    public Color backgroundColor = new Color(0.05f, 0.10f, 0.08f, 0.97f);
    public Sprite borderSprite;
    public Color  borderColor = new Color(0.45f, 1f, 0.65f, 1f);

    [Header("Ornamen Atas (opsional)")]
    public Sprite ornamenAtasSprite;
    public Vector2 ornamenAtasSize    = new Vector2(220f, 110f);
    public float   ornamenAtasOffsetY = 35f;

    [Header("Overlay Belakang")]
    public bool   tampilkanOverlay = true;
    public Sprite overlaySprite;
    public Color  overlayColor     = new Color(0f, 0f, 0f, 0.82f);

    [Header("Judul")]
    public string judul       = "\uD83D\uDEE1\uFE0F  CARA MENJAGA DIRI \u2014 HARI 2";
    public Color  warnaJudul  = new Color(0.45f, 1f, 0.65f, 1f);
    public int    ukuranJudul = 32;

    [System.Serializable]
    public class TipsEntry
    {
        public string heading = "\uD83D\uDEA9 Heading Tip";
        [TextArea(2, 5)] public string isi = "Penjelasan tips di sini...";
        public Sprite ikon;
        public Color  warnaHeading = new Color(0.45f, 1f, 0.65f, 1f);
        public Color  warnaIsi     = new Color(1f, 1f, 0.85f, 1f);
    }

    [Header("Daftar Tips Hari 2 (CUSTOMIZABLE)")]
    public TipsEntry[] tipsList = new TipsEntry[]
    {
        new TipsEntry {
            heading = "\u2728 3 KATA SAKTI saat merasa tidak aman:",
            isi     = "\u2460 TIDAK!  \u2014 tolak dengan TEGAS & suara keras.\n\u2461 PERGI   \u2014 menjauh / pindah ke tempat ramai.\n\u2462 CERITA  \u2014 lapor orang dewasa yang kamu percaya.\nKamu TIDAK pernah salah karena menolak atau melapor."
        },
        new TipsEntry {
            heading = "\uD83D\uDEAB ZONA PRIBADI tubuhmu:",
            isi     = "\u2022 Bagian tubuh yang tertutup baju renang = milik kamu sendiri.\n\u2022 Tidak ada yang boleh menyentuh / melihat / memotretnya.\n\u2022 Kalau ada yang mencoba \u2014 walau orang dikenal \u2014 itu BAHAYA. Lakukan: TIDAK, PERGI, CERITA."
        },
        new TipsEntry {
            heading = "\uD83D\uDEA9 KENALI TANDA BAHAYA (grooming):",
            isi     = "\u2022 Orang asing sok akrab & memberi iming-iming / hadiah gratis.\n\u2022 Minta data pribadi (nomor HP, alamat, foto).\n\u2022 Mengajak menyimpan rahasia dari orang tua.\nSemua itu = RED FLAG. Jangan diladeni!"
        },
        new TipsEntry {
            heading = "\uD83D\uDE8C Aman di Angkot & Jalan:",
            isi     = "\u2022 Duduk dekat Pak Supir / ibu-ibu, hindari pojok sepi.\n\u2022 Catat plat nomor & kabari orang tua.\n\u2022 Kalau ada yang aneh, turun di tempat ramai dan minta tolong."
        },
        new TipsEntry {
            heading = "\uD83D\uDC6E ORANG TEPERCAYA tempat lapor:",
            isi     = "\u2022 Orang tua, guru, dan keluarga dekat.\n\u2022 Polisi, satpam, atau petugas berseragam.\n\u2022 Ibu-ibu / orang dewasa di tempat ramai.\nSimpan nomor mereka & beranikan diri bercerita."
        }
    };

    [Header("Footer (Hotline)")]
    [TextArea(2, 4)]
    public string footerText = "\u260E Hotline:\nPolisi 110  |  Hotline Anak 129  |  KPAI 021-31901556";
    public Color  warnaFooter = new Color(1f, 0.78f, 0.20f, 1f);
    public int    ukuranFooter = 18;

    [Header("Tombol Lanjut")]
    public string tombolLanjutTeks = "\u25B6  LANJUT";
    public Sprite tombolSprite;
    public Color  tombolWarna       = new Color(0.20f, 0.65f, 0.35f, 1f);
    public Color  tombolBorderColor = new Color(1f, 0.78f, 0.20f, 1f);
    public Color  tombolTeksWarna   = Color.white;
    public int    tombolUkuranTeks  = 26;

    [Header("Font (opsional)")]
    public TMP_FontAsset fontAsset;

    [Header("Animasi")]
    [Range(0.5f, 1f)] public float skalaAwal = 0.85f;
    public float durasiPopIn = 0.32f;

    [Header("Audio (opsional)")]
    public AudioClip sfxMuncul;
    public AudioClip sfxKlikLanjut;

    [Header("Sorting")]
    public int sortingOrder = 1020;

    [Header("Event")]
    public UnityEngine.Events.UnityEvent onLanjut;

    [Header("Lanjut Ke Layar Berikut")]
    [Tooltip("Referensi langsung ke Day2SummaryScreen. Kosongkan untuk auto-find.")]
    public Day2SummaryScreen layarRingkasanSelanjutnya;
    [Tooltip("Auto-find Day2SummaryScreen kalau referensi di atas kosong.")]
    public bool autoCariLayarRingkasan = true;

    // ── runtime ───────────────────────────────────────────────────────────
    private bool       _kartuTampil;
    private GameObject _canvasGO;
    private Sprite     _roundedRectSprite;

    void Awake() { Instance = this; }
    void OnDestroy() { if (Instance == this) Instance = null; }
    void Start() { if (autoTampilSaatStart) Tampilkan(); }

    public void Tampilkan()
    {
        if (_kartuTampil) return;
        StartCoroutine(TampilkanKartu());
    }

    public static void TampilkanStatik()
    {
        if (Instance != null) Instance.Tampilkan();
        else Debug.LogWarning("[EduCardDay2] Tidak ada instance di scene!");
    }

    public void Tutup()
    {
        if (!_kartuTampil) return;
        if (_canvasGO != null) Destroy(_canvasGO);
        _canvasGO    = null;
        _kartuTampil = false;
    }

    IEnumerator TampilkanKartu()
    {
        _kartuTampil = true;
        if (sfxMuncul != null) AudioManager.Instance?.sfxSource?.PlayOneShot(sfxMuncul);
        else AudioManager.Instance?.PlayAchievement();
        BuildUI();
        yield return null;
    }

    void BuildUI()
    {
        _canvasGO = new GameObject("EduCardDay2Canvas");
        var canvas = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var sc = _canvasGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        sc.matchWidthOrHeight = 0.5f;
        _canvasGO.AddComponent<GraphicRaycaster>();

        if (tampilkanOverlay)
        {
            var ov = new GameObject("Overlay");
            ov.transform.SetParent(_canvasGO.transform, false);
            var ovImg = ov.AddComponent<Image>();
            if (overlaySprite != null) { ovImg.sprite = overlaySprite; ovImg.type = Image.Type.Sliced; ovImg.color = Color.white; }
            else ovImg.color = overlayColor;
            ovImg.raycastTarget = true;
            var orRT = ov.GetComponent<RectTransform>();
            orRT.anchorMin = Vector2.zero; orRT.anchorMax = Vector2.one;
            orRT.offsetMin = Vector2.zero; orRT.offsetMax = Vector2.zero;
        }

        // Card
        var card = new GameObject("Card");
        card.transform.SetParent(_canvasGO.transform, false);
        var cardRT = card.AddComponent<RectTransform>();
        cardRT.anchorMin = cardRT.anchorMax = new Vector2(0.5f, 0.5f);
        cardRT.pivot = new Vector2(0.5f, 0.5f);
        cardRT.sizeDelta = new Vector2(960f, 680f);

        var cardImg = card.AddComponent<Image>();
        cardImg.sprite = backgroundSprite != null ? backgroundSprite : GetRoundedRect();
        cardImg.type   = backgroundSprite != null ? backgroundImageType : Image.Type.Sliced;
        cardImg.color  = backgroundColor;

        if (borderSprite != null)
        {
            var border = new GameObject("Border");
            border.transform.SetParent(card.transform, false);
            var bRT = border.AddComponent<RectTransform>();
            bRT.anchorMin = Vector2.zero; bRT.anchorMax = Vector2.one;
            bRT.offsetMin = bRT.offsetMax = Vector2.zero;
            var bImg = border.AddComponent<Image>();
            bImg.sprite = borderSprite; bImg.type = Image.Type.Sliced; bImg.color = borderColor; bImg.raycastTarget = false;
        }
        else
        {
            var ol = card.AddComponent<Outline>();
            ol.effectColor = borderColor;
            ol.effectDistance = new Vector2(3f, -3f);
        }

        if (ornamenAtasSprite != null)
        {
            var orn = new GameObject("OrnamenAtas");
            orn.transform.SetParent(card.transform, false);
            var oRT = orn.AddComponent<RectTransform>();
            oRT.anchorMin = oRT.anchorMax = new Vector2(0.5f, 1f);
            oRT.pivot = new Vector2(0.5f, 0.5f);
            oRT.sizeDelta = ornamenAtasSize;
            oRT.anchoredPosition = new Vector2(0f, ornamenAtasOffsetY);
            var oImg = orn.AddComponent<Image>();
            oImg.sprite = ornamenAtasSprite; oImg.preserveAspect = true; oImg.raycastTarget = false;
        }

        // Judul
        var titleTMP = BuatTeks(card.transform, "Judul", judul, ukuranJudul, warnaJudul, FontStyles.Bold);
        var tRT = titleTMP.rectTransform;
        tRT.anchorMin = new Vector2(0f, 1f); tRT.anchorMax = new Vector2(1f, 1f);
        tRT.pivot = new Vector2(0.5f, 1f);
        tRT.offsetMin = new Vector2(30f, -90f);
        tRT.offsetMax = new Vector2(-30f, -25f);
        titleTMP.alignment = TextAlignmentOptions.Center;

        // Tips list
        var listGO = new GameObject("TipsList");
        listGO.transform.SetParent(card.transform, false);
        var listRT = listGO.AddComponent<RectTransform>();
        listRT.anchorMin = new Vector2(0f, 0f); listRT.anchorMax = new Vector2(1f, 1f);
        listRT.offsetMin = new Vector2(40f, 140f);
        listRT.offsetMax = new Vector2(-40f, -110f);
        var vlg = listGO.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlWidth = true; vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
        vlg.spacing = 14f; vlg.padding = new RectOffset(8, 8, 8, 8);

        if (tipsList != null) foreach (var tip in tipsList) BuatTipEntry(listGO.transform, tip);

        // Footer
        if (!string.IsNullOrEmpty(footerText))
        {
            var footTMP = BuatTeks(card.transform, "Footer", footerText, ukuranFooter, warnaFooter, FontStyles.Italic);
            var fRT = footTMP.rectTransform;
            fRT.anchorMin = new Vector2(0f, 0f); fRT.anchorMax = new Vector2(1f, 0f);
            fRT.pivot = new Vector2(0.5f, 0f);
            fRT.offsetMin = new Vector2(30f, 90f);
            fRT.offsetMax = new Vector2(-30f, 140f);
            footTMP.alignment = TextAlignmentOptions.Center;
        }

        // Tombol Lanjut
        var btnGO = new GameObject("BtnLanjut");
        btnGO.transform.SetParent(card.transform, false);
        var btnRT = btnGO.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.5f, 0f); btnRT.anchorMax = new Vector2(0.5f, 0f);
        btnRT.pivot = new Vector2(0.5f, 0f);
        btnRT.sizeDelta = new Vector2(320f, 64f);
        btnRT.anchoredPosition = new Vector2(0f, 25f);

        var btnImg = btnGO.AddComponent<Image>();
        if (tombolSprite != null) { btnImg.sprite = tombolSprite; btnImg.type = Image.Type.Sliced; btnImg.color = Color.white; }
        else
        {
            btnImg.sprite = GetRoundedRect(); btnImg.type = Image.Type.Sliced; btnImg.color = tombolWarna;
            var bOL = btnGO.AddComponent<Outline>();
            bOL.effectColor = tombolBorderColor; bOL.effectDistance = new Vector2(3f, -3f);
        }
        var btn = btnGO.AddComponent<Button>();
        var btnTxt = BuatTeks(btnGO.transform, "Label", tombolLanjutTeks, tombolUkuranTeks, tombolTeksWarna, FontStyles.Bold);
        var btnTRT = btnTxt.rectTransform;
        btnTRT.anchorMin = Vector2.zero; btnTRT.anchorMax = Vector2.one;
        btnTRT.offsetMin = btnTRT.offsetMax = Vector2.zero;
        btnTxt.alignment = TextAlignmentOptions.Center;
        btn.onClick.AddListener(HandleLanjut);

        // Pop-in
        cardRT.localScale = Vector3.one * skalaAwal;
        StartCoroutine(PopIn(cardRT));
    }

    void BuatTipEntry(Transform parent, TipsEntry tip)
    {
        if (tip == null) return;
        var entry = new GameObject("TipEntry");
        entry.transform.SetParent(parent, false);
        entry.AddComponent<RectTransform>();
        var hlg = entry.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.UpperLeft;
        hlg.childControlWidth = true; hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
        hlg.spacing = 12f;

        if (tip.ikon != null)
        {
            var iconGO = new GameObject("Ikon");
            iconGO.transform.SetParent(entry.transform, false);
            var iRT = iconGO.AddComponent<RectTransform>();
            iRT.sizeDelta = new Vector2(56f, 56f);
            var iImg = iconGO.AddComponent<Image>();
            iImg.sprite = tip.ikon; iImg.preserveAspect = true;
            var le = iconGO.AddComponent<LayoutElement>();
            le.preferredWidth = 56f; le.preferredHeight = 56f; le.flexibleWidth = 0f;
        }

        var txtCol = new GameObject("Text");
        txtCol.transform.SetParent(entry.transform, false);
        txtCol.AddComponent<RectTransform>();
        var vlg = txtCol.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlWidth = true; vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
        vlg.spacing = 4f;
        var txtLE = txtCol.AddComponent<LayoutElement>();
        txtLE.flexibleWidth = 1f;

        var h = BuatTeks(txtCol.transform, "Heading", tip.heading, 22, tip.warnaHeading, FontStyles.Bold);
        h.alignment = TextAlignmentOptions.TopLeft;
        var b = BuatTeks(txtCol.transform, "Isi", tip.isi, 18, tip.warnaIsi, FontStyles.Normal);
        b.alignment = TextAlignmentOptions.TopLeft;
        b.lineSpacing = 6f;
    }

    void HandleLanjut()
    {
        if (sfxKlikLanjut != null) AudioManager.Instance?.sfxSource?.PlayOneShot(sfxKlikLanjut);
        else AudioManager.Instance?.Click();

        Tutup();
        onLanjut?.Invoke();

        Day2SummaryScreen layar = layarRingkasanSelanjutnya;
        if (layar == null && autoCariLayarRingkasan)
            layar = FindFirstObjectByType<Day2SummaryScreen>(FindObjectsInactive.Include);

        if (layar != null)
        {
            Debug.Log("[EduCardDay2] \u2192 Tampilkan Day2SummaryScreen");
            layar.Tampilkan();
        }
        else
        {
            Debug.LogWarning("[EduCardDay2] Day2SummaryScreen tidak ditemukan. Tambah komponen di scene atau set 'Layar Ringkasan Selanjutnya'.");
        }
    }

    // Helpers
    TextMeshProUGUI BuatTeks(Transform parent, string name, string content, int size, Color color, FontStyles style)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        TMP_FontAsset f = fontAsset ?? TMP_Settings.defaultFontAsset;
        if (f != null) tmp.font = f;
        tmp.text = content; tmp.fontSize = size; tmp.color = color; tmp.fontStyle = style;
        tmp.textWrappingMode = TextWrappingModes.Normal; tmp.raycastTarget = false;
        return tmp;
    }

    IEnumerator PopIn(RectTransform rt)
    {
        float t = 0f;
        Vector3 from = Vector3.one * skalaAwal, to = Vector3.one;
        while (t < durasiPopIn)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / durasiPopIn);
            float c1 = 1.70158f, c3 = c1 + 1f;
            float e = 1f + c3 * Mathf.Pow(k - 1, 3) + c1 * Mathf.Pow(k - 1, 2);
            rt.localScale = Vector3.LerpUnclamped(from, to, e);
            yield return null;
        }
        rt.localScale = to;
    }

    Sprite GetRoundedRect()
    {
        if (_roundedRectSprite != null) return _roundedRectSprite;
        const int w = 64, h = 32, radius = 14;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
        {
            int dx = x < radius ? radius - x : x > w - radius ? x - (w - radius) : 0;
            int dy = y < radius ? radius - y : y > h - radius ? y - (h - radius) : 0;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            float a = Mathf.Clamp01(radius - dist);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        tex.Apply();
        _roundedRectSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        return _roundedRectSprite;
    }
}
