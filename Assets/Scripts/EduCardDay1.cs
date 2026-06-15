using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// EduCardDay1 — Kartu Edukasi Hari 1 yang FULLY CUSTOMIZABLE.
///
/// Dipanggil setelah SampaiSekolahPopup.onLanjut → EduCardDay1.Tampilkan().
/// Bisa juga dipanggil dari script lain via API publik:
///   - Tampilkan()                — pakai konten yg sudah di-set di Inspector
///   - TampilkanStatik()          — singleton style, auto-find instance di scene
///
/// Cara pakai:
///   1. GameObject → Create Empty → "EduCardDay1"
///   2. Add Component → EduCardDay1
///   3. (Opsional) Drag sprite background, judul, isi tips, hotline
///   4. Di SampaiSekolahPopup → onLanjut() → drag GameObject ini →
///      pilih EduCardDay1.Tampilkan
///   5. Di field onLanjut (kartu edukasi) → hubungkan ke
///      Day1Controller.GoToResult / SceneLoader.LoadScene("Day2")
/// </summary>
public class EduCardDay1 : MonoBehaviour
{
    // Singleton lemah agar bisa dipanggil dari mana saja
    public static EduCardDay1 Instance { get; private set; }

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR
    // ══════════════════════════════════════════════════════════════════════

    [Header("Auto-Tampil")]
    [Tooltip("Tampilkan otomatis saat scene start? Kosongkan agar dipanggil manual via Tampilkan().")]
    public bool autoTampilSaatStart = false;
    [Tooltip("Bekukan pergerakan player saat kartu tampil.")]
    public bool freezePlayerSaatTampil = true;

    [Header("Background Kartu (CUSTOMIZABLE)")]
    [Tooltip("Sprite latar kartu utama. Kosong = panel solid + rounded corner.")]
    public Sprite backgroundSprite;
    public Image.Type backgroundImageType = Image.Type.Sliced;
    [Tooltip("Warna tint background.")]
    public Color backgroundColor = new Color(0.08f, 0.05f, 0.02f, 0.97f);
    [Tooltip("Sprite border (opsional). Kosong = pakai Outline.")]
    public Sprite borderSprite;
    public Color  borderColor = new Color(1f, 0.78f, 0.20f, 1f);

    [Header("Ornamen Atas (opsional)")]
    [Tooltip("Sprite hiasan di tengah-atas kartu.")]
    public Sprite ornamenAtasSprite;
    public Vector2 ornamenAtasSize    = new Vector2(220f, 110f);
    public float   ornamenAtasOffsetY = 35f;

    [Header("Overlay Belakang (dim layar)")]
    public bool   tampilkanOverlay = true;
    public Sprite overlaySprite;
    public Color  overlayColor     = new Color(0f, 0f, 0f, 0.82f);

    [Header("Judul")]
    public string judul       = "📚  KARTU EDUKASI — HARI 1";
    public Color  warnaJudul  = new Color(1f, 0.78f, 0.20f, 1f);
    public int    ukuranJudul = 32;

    [System.Serializable]
    public class TipsEntry
    {
        [Tooltip("Heading tip (tebal & berwarna highlight).")]
        public string heading = "🚩 Heading Tip";
        [TextArea(2, 5)]
        [Tooltip("Isi tip — bisa multi-line.")]
        public string isi     = "Penjelasan tips di sini...";
        [Tooltip("Sprite ikon opsional di kiri heading.")]
        public Sprite ikon;
        [Tooltip("Warna heading. Default merah lembut.")]
        public Color  warnaHeading = new Color(1f, 0.55f, 0.55f, 1f);
        [Tooltip("Warna isi. Default krem.")]
        public Color  warnaIsi     = new Color(1f, 1f, 0.85f, 1f);
    }

    [Header("Daftar Tips (CUSTOMIZABLE)")]
    [Tooltip("Tambah/kurangi tips sesuka hati. Setiap tip = heading + isi + ikon opsional.")]
    public TipsEntry[] tipsList = new TipsEntry[]
    {
        new TipsEntry {
            heading = "🚩 Orang Asing Kasih Hadiah = BAHAYA!",
            isi     = "Permen, snack, atau tumpangan gratis dari orang yg baru kamu kenal — tolak dan langsung pergi!"
        },
        new TipsEntry {
            heading = "✅ Yang Harus Kamu Lakuin:",
            isi     = "• Bilang TIDAK dengan tegas — itu HAK kamu!\n• Teriak keras dan lari ke tempat yang rame\n• Ceritain ke ortu, guru, atau orang dewasa yang dipercaya"
        },
        new TipsEntry {
            heading = "🛡 Bilang \"Tidak\" itu BOLEH!",
            isi     = "Kamu BERHAK menolak siapapun yang bikin nggak nyaman — termasuk orang dewasa sekalipun!"
        }
    };

    [Header("Ilustrasi (opsional)")]
    [Tooltip("Sprite ilustrasi besar di bawah judul. Kosong = tidak tampil.")]
    public Sprite ilustrasiSprite;
    [Tooltip("Tinggi area ilustrasi (px). Lebar mengikuti lebar kartu.")]
    public float ilustrasiTinggi = 150f;

    [Header("Accordion")]
    [Tooltip("Tiap tip jadi accordion: klik heading untuk buka/tutup isi (hemat ruang).")]
    public bool accordion = true;
    [Tooltip("Indeks tip yang terbuka di awal (-1 = semua tertutup, 0 = tip pertama).")]
    public int accordionTerbukaAwal = 0;

    [Header("Footer (Hotline)")]
    [TextArea(2, 4)]
    [Tooltip("Teks footer — biasanya nomor hotline. Kosong = tidak tampil.")]
    public string footerText = "📞 Kalau ada yang ganggu atau bikin nggak nyaman:\nHotline Anak 129  |  KPAI 021-31901556";
    public Color  warnaFooter = new Color(1f, 0.78f, 0.20f, 1f);
    public int    ukuranFooter = 18;

    [Header("Tombol Lanjut")]
    public string tombolLanjutTeks = "▶  LANJUT";
    [Tooltip("Sprite tombol custom. Kosong = solid color rounded.")]
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
    [Tooltip("Sorting order Canvas. Default 1020 — di atas SampaiSekolahPopup (1010).")]
    public int sortingOrder = 1020;

    [Header("Event")]
    [Tooltip("Dipanggil saat pemain klik tombol Lanjut.")]
    public UnityEngine.Events.UnityEvent onLanjut;

    [Header("Lanjut Ke Layar Berikut")]
    [Tooltip("Referensi langsung ke Day1SummaryScreen. Kosongkan untuk auto-find di scene.")]
    public Day1SummaryScreen layarRingkasanSelanjutnya;
    [Tooltip("Aktifkan auto-find Day1SummaryScreen di scene kalau referensi di atas kosong.")]
    public bool autoCariLayarRingkasan = true;

    // ── runtime ───────────────────────────────────────────────────────────
    private bool       _kartuTampil;
    private GameObject _canvasGO;
    private Sprite     _roundedRectSprite;

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        if (autoTampilSaatStart) Tampilkan();
    }

    // ══════════════════════════════════════════════════════════════════════
    // PUBLIC API
    // ══════════════════════════════════════════════════════════════════════

    /// Tampilkan kartu edukasi sekarang.
    public void Tampilkan()
    {
        if (_kartuTampil) return;
        StartCoroutine(TampilkanKartu());
    }

    /// Static helper — bisa dipanggil dari script lain.
    /// Cari instance EduCardDay1 di scene & panggil Tampilkan().
    public static void TampilkanStatik()
    {
        if (Instance != null) Instance.Tampilkan();
        else Debug.LogWarning("[EduCardDay1] Tidak ada instance di scene!");
    }

    /// Tutup kartu yang sedang tampil.
    public void Tutup()
    {
        if (!_kartuTampil) return;
        if (_canvasGO != null) Destroy(_canvasGO);
        _canvasGO    = null;
        _kartuTampil = false;

        if (freezePlayerSaatTampil)
        {
            var d1 = FindFirstObjectByType<Day1Controller>();
            if (d1 != null) d1.ResumePlayer();
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // BUILDER
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator TampilkanKartu()
    {
        _kartuTampil = true;

        if (freezePlayerSaatTampil)
        {
            var d1 = FindFirstObjectByType<Day1Controller>();
            if (d1 != null) d1.FreezePlayer();
        }

        // SFX
        if (sfxMuncul != null)
            AudioManager.Instance?.sfxSource?.PlayOneShot(sfxMuncul);
        else
            AudioManager.Instance?.PlayAchievement();

        BuildUI();
        yield return null;
    }

    void BuildUI()
    {
        // ── Canvas ────────────────────────────────────────────────────────
        _canvasGO = new GameObject("EduCardDay1Canvas");
        var canvas = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var sc = _canvasGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920f, 1080f);
        sc.matchWidthOrHeight  = 0.5f;
        _canvasGO.AddComponent<GraphicRaycaster>();

        // ── Overlay belakang ─────────────────────────────────────────────
        if (tampilkanOverlay)
        {
            var ov = new GameObject("Overlay");
            ov.transform.SetParent(_canvasGO.transform, false);
            var ovRT = ov.AddComponent<RectTransform>();
            ovRT.anchorMin = Vector2.zero; ovRT.anchorMax = Vector2.one;
            ovRT.offsetMin = ovRT.offsetMax = Vector2.zero;
            var ovImg = ov.AddComponent<Image>();
            if (overlaySprite != null)
            {
                ovImg.sprite = overlaySprite;
                ovImg.type   = Image.Type.Sliced;
                ovImg.color  = Color.white;
            }
            else ovImg.color = overlayColor;
            ovImg.raycastTarget = true;
        }

        // ── Kartu utama ──────────────────────────────────────────────────
        var card = new GameObject("Card");
        card.transform.SetParent(_canvasGO.transform, false);
        var cardRT = card.AddComponent<RectTransform>();
        cardRT.anchorMin = cardRT.anchorMax = new Vector2(0.5f, 0.5f);
        cardRT.pivot     = new Vector2(0.5f, 0.5f);
        cardRT.sizeDelta = new Vector2(960f, 680f);

        var cardImg = card.AddComponent<Image>();
        if (backgroundSprite != null)
        {
            cardImg.sprite = backgroundSprite;
            cardImg.type   = backgroundImageType;
            cardImg.color  = backgroundColor;
        }
        else
        {
            cardImg.sprite = GetRoundedRect();
            cardImg.type   = Image.Type.Sliced;
            cardImg.color  = backgroundColor;
        }

        // Border
        if (borderSprite != null)
        {
            var border = new GameObject("Border");
            border.transform.SetParent(card.transform, false);
            var bRT = border.AddComponent<RectTransform>();
            bRT.anchorMin = Vector2.zero; bRT.anchorMax = Vector2.one;
            bRT.offsetMin = bRT.offsetMax = Vector2.zero;
            var bImg = border.AddComponent<Image>();
            bImg.sprite        = borderSprite;
            bImg.type          = Image.Type.Sliced;
            bImg.color         = borderColor;
            bImg.raycastTarget = false;
        }
        else
        {
            var ol = card.AddComponent<Outline>();
            ol.effectColor    = borderColor;
            ol.effectDistance = new Vector2(3f, -3f);
        }

        // ── Ornamen atas ─────────────────────────────────────────────────
        if (ornamenAtasSprite != null)
        {
            var orn = new GameObject("OrnamenAtas");
            orn.transform.SetParent(card.transform, false);
            var oRT = orn.AddComponent<RectTransform>();
            oRT.anchorMin = oRT.anchorMax = new Vector2(0.5f, 1f);
            oRT.pivot     = new Vector2(0.5f, 0.5f);
            oRT.sizeDelta = ornamenAtasSize;
            oRT.anchoredPosition = new Vector2(0f, ornamenAtasOffsetY);
            var oImg = orn.AddComponent<Image>();
            oImg.sprite         = ornamenAtasSprite;
            oImg.preserveAspect = true;
            oImg.raycastTarget  = false;
        }

        // ── Judul ────────────────────────────────────────────────────────
        var titleTMP = BuatTeks(card.transform, "Judul", judul,
                                 ukuranJudul, warnaJudul, FontStyles.Bold);
        var tRT = titleTMP.rectTransform;
        tRT.anchorMin = new Vector2(0f, 1f); tRT.anchorMax = new Vector2(1f, 1f);
        tRT.pivot     = new Vector2(0.5f, 1f);
        tRT.offsetMin = new Vector2(30f, -90f);
        tRT.offsetMax = new Vector2(-30f, -25f);
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.textWrappingMode = TextWrappingModes.Normal;

        // ── Ilustrasi (opsional) di bawah judul ──────────────────────────
        float tipsTopOffset = -110f;
        if (ilustrasiSprite != null)
        {
            var illGO = new GameObject("Ilustrasi");
            illGO.transform.SetParent(card.transform, false);
            var illRT = illGO.AddComponent<RectTransform>();
            illRT.anchorMin = new Vector2(0f, 1f); illRT.anchorMax = new Vector2(1f, 1f);
            illRT.pivot     = new Vector2(0.5f, 1f);
            illRT.offsetMin = new Vector2(40f, -110f - ilustrasiTinggi);
            illRT.offsetMax = new Vector2(-40f, -110f);
            var illImg = illGO.AddComponent<Image>();
            illImg.sprite         = ilustrasiSprite;
            illImg.preserveAspect = true;
            illImg.raycastTarget  = false;
            tipsTopOffset = -110f - ilustrasiTinggi - 14f;
        }

        // ── Container tips ───────────────────────────────────────────────
        var listGO = new GameObject("TipsList");
        listGO.transform.SetParent(card.transform, false);
        var listRT = listGO.AddComponent<RectTransform>();
        listRT.anchorMin = new Vector2(0f, 0f); listRT.anchorMax = new Vector2(1f, 1f);
        listRT.offsetMin = new Vector2(40f, 140f);
        listRT.offsetMax = new Vector2(-40f, tipsTopOffset);
        var vlg = listGO.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment       = TextAnchor.UpperLeft;
        vlg.childControlWidth    = true;
        vlg.childControlHeight   = true;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing              = 14f;
        vlg.padding              = new RectOffset(8, 8, 8, 8);

        if (tipsList != null)
            for (int i = 0; i < tipsList.Length; i++)
                BuatTipEntry(listGO.transform, tipsList[i], i);

        // ── Footer (hotline) ─────────────────────────────────────────────
        if (!string.IsNullOrEmpty(footerText))
        {
            var footTMP = BuatTeks(card.transform, "Footer", footerText,
                                    ukuranFooter, warnaFooter, FontStyles.Italic);
            var fRT = footTMP.rectTransform;
            fRT.anchorMin = new Vector2(0f, 0f); fRT.anchorMax = new Vector2(1f, 0f);
            fRT.pivot     = new Vector2(0.5f, 0f);
            fRT.offsetMin = new Vector2(30f, 90f);
            fRT.offsetMax = new Vector2(-30f, 140f);
            footTMP.alignment = TextAlignmentOptions.Center;
            footTMP.textWrappingMode = TextWrappingModes.Normal;
        }

        // ── Tombol Lanjut ────────────────────────────────────────────────
        var btnGO = new GameObject("BtnLanjut");
        btnGO.transform.SetParent(card.transform, false);
        var btnRT = btnGO.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.5f, 0f); btnRT.anchorMax = new Vector2(0.5f, 0f);
        btnRT.pivot     = new Vector2(0.5f, 0f);
        btnRT.sizeDelta = new Vector2(320f, 64f);
        btnRT.anchoredPosition = new Vector2(0f, 25f);

        var btnImg = btnGO.AddComponent<Image>();
        if (tombolSprite != null)
        {
            btnImg.sprite = tombolSprite;
            btnImg.type   = Image.Type.Sliced;
            btnImg.color  = Color.white;
        }
        else
        {
            btnImg.sprite = GetRoundedRect();
            btnImg.type   = Image.Type.Sliced;
            btnImg.color  = tombolWarna;

            var bOL = btnGO.AddComponent<Outline>();
            bOL.effectColor    = tombolBorderColor;
            bOL.effectDistance = new Vector2(3f, -3f);
        }

        var btn = btnGO.AddComponent<Button>();
        var btnTxt = BuatTeks(btnGO.transform, "Label", tombolLanjutTeks,
                               tombolUkuranTeks, tombolTeksWarna, FontStyles.Bold);
        var btnTRT = btnTxt.rectTransform;
        btnTRT.anchorMin = Vector2.zero; btnTRT.anchorMax = Vector2.one;
        btnTRT.offsetMin = btnTRT.offsetMax = Vector2.zero;
        btnTxt.alignment = TextAlignmentOptions.Center;

        btn.onClick.AddListener(HandleLanjut);

        // ── Animasi pop-in ───────────────────────────────────────────────
        cardRT.localScale = Vector3.one * skalaAwal;
        StartCoroutine(PopIn(cardRT));
    }

    void BuatTipEntry(Transform parent, TipsEntry tip, int index)
    {
        if (tip == null) return;

        var entry = new GameObject("TipEntry");
        entry.transform.SetParent(parent, false);
        entry.AddComponent<RectTransform>();
        var hlg = entry.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment       = TextAnchor.UpperLeft;
        hlg.childControlWidth    = true;
        hlg.childControlHeight   = true;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = false;
        hlg.spacing              = 12f;

        if (tip.ikon != null)
        {
            var iconGO = new GameObject("Ikon");
            iconGO.transform.SetParent(entry.transform, false);
            var iRT = iconGO.AddComponent<RectTransform>();
            iRT.sizeDelta = new Vector2(56f, 56f);
            var iImg = iconGO.AddComponent<Image>();
            iImg.sprite         = tip.ikon;
            iImg.preserveAspect = true;
            var le = iconGO.AddComponent<LayoutElement>();
            le.preferredWidth  = 56f;
            le.preferredHeight = 56f;
            le.flexibleWidth   = 0f;
        }

        var txtCol = new GameObject("Text");
        txtCol.transform.SetParent(entry.transform, false);
        txtCol.AddComponent<RectTransform>();
        var vlg = txtCol.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment       = TextAnchor.UpperLeft;
        vlg.childControlWidth    = true;
        vlg.childControlHeight   = true;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing              = 4f;
        var txtLE = txtCol.AddComponent<LayoutElement>();
        txtLE.flexibleWidth      = 1f;

        // ── Baris heading (klik untuk buka/tutup bila accordion) ─────────
        var headRow = new GameObject("HeaderRow");
        headRow.transform.SetParent(txtCol.transform, false);
        headRow.AddComponent<RectTransform>();
        var headHLG = headRow.AddComponent<HorizontalLayoutGroup>();
        headHLG.childAlignment       = TextAnchor.UpperLeft;
        headHLG.childControlWidth    = true;
        headHLG.childControlHeight   = true;
        headHLG.childForceExpandWidth  = false;
        headHLG.childForceExpandHeight = false;
        headHLG.spacing              = 8f;

        var h = BuatTeks(headRow.transform, "Heading", tip.heading, 22,
                          tip.warnaHeading, FontStyles.Bold);
        h.alignment        = TextAlignmentOptions.TopLeft;
        h.textWrappingMode = TextWrappingModes.Normal;
        var hLE = h.gameObject.AddComponent<LayoutElement>();
        hLE.flexibleWidth  = 1f;

        var b = BuatTeks(txtCol.transform, "Isi", tip.isi, 18,
                          tip.warnaIsi, FontStyles.Normal);
        b.alignment        = TextAlignmentOptions.TopLeft;
        b.textWrappingMode = TextWrappingModes.Normal;
        b.lineSpacing      = 6f;

        if (accordion)
        {
            // Chevron indikator buka/tutup
            bool terbuka = (index == accordionTerbukaAwal);
            var chev = BuatTeks(headRow.transform, "Chevron", terbuka ? "▼" : "▶", 18,
                                 tip.warnaHeading, FontStyles.Bold);
            chev.alignment = TextAlignmentOptions.MidlineRight;
            var chLE = chev.gameObject.AddComponent<LayoutElement>();
            chLE.preferredWidth = 28f;
            chLE.flexibleWidth  = 0f;

            b.gameObject.SetActive(terbuka);

            // Tombol transparan di area heading
            var headBtnImg = headRow.AddComponent<Image>();
            headBtnImg.color = new Color(1f, 1f, 1f, 0.001f); // hampir transparan tapi bisa diklik
            var headBtn = headRow.AddComponent<Button>();
            var bodyGO = b.gameObject;
            var chevTMP = chev;
            headBtn.onClick.AddListener(() =>
            {
                bool baru = !bodyGO.activeSelf;
                bodyGO.SetActive(baru);
                chevTMP.text = baru ? "▼" : "▶";
                AudioManager.Instance?.Click();
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)entry.transform);
            });
        }
    }

    void HandleLanjut()
    {
        if (sfxKlikLanjut != null)
            AudioManager.Instance?.sfxSource?.PlayOneShot(sfxKlikLanjut);
        else
            AudioManager.Instance?.Click();

        Tutup();
        onLanjut?.Invoke();

        // Auto-buka Day1SummaryScreen kalau ditemukan
        Day1SummaryScreen layar = layarRingkasanSelanjutnya;
        if (layar == null && autoCariLayarRingkasan)
        {
            layar = FindFirstObjectByType<Day1SummaryScreen>(FindObjectsInactive.Include);
        }
        if (layar != null)
        {
            Debug.Log("[EduCardDay1] → Tampilkan Day1SummaryScreen");
            layar.Tampilkan();
        }
        else
        {
            Debug.LogWarning("[EduCardDay1] Day1SummaryScreen tidak ditemukan di scene. " +
                             "Tambahkan GameObject dengan komponen Day1SummaryScreen, " +
                             "atau hubungkan via field 'Layar Ringkasan Selanjutnya'.");
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // HELPERS
    // ══════════════════════════════════════════════════════════════════════
    TextMeshProUGUI BuatTeks(Transform parent, string name, string content,
                             int size, Color color, FontStyles style)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        TMP_FontAsset f = fontAsset ?? TMP_Settings.defaultFontAsset;
        if (f != null) tmp.font = f;
        tmp.text          = content;
        tmp.fontSize      = size;
        tmp.color         = color;
        tmp.fontStyle     = style;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        return tmp;
    }

    IEnumerator PopIn(RectTransform rt)
    {
        float t = 0f;
        Vector3 from = Vector3.one * skalaAwal;
        Vector3 to   = Vector3.one;
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
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            int dx = x < radius ? radius - x : x > w - radius ? x - (w - radius) : 0;
            int dy = y < radius ? radius - y : y > h - radius ? y - (h - radius) : 0;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            float a = Mathf.Clamp01(radius - dist);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        tex.Apply();
        _roundedRectSprite = Sprite.Create(tex, new Rect(0, 0, w, h),
            new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius));
        return _roundedRectSprite;
    }
}
