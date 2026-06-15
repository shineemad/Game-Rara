using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// MainMenu — layar menu utama yang tampil di awal game (single-scene).
///
/// Cara kerja gate prolog:
///   Game ini satu scene. Entry point asli adalah PrologScreen (auto-start di Start),
///   lalu Day1Intro menunggu flag statis PrologScreen.prologDone.
///   MainMenu menahan prolog dengan menonaktifkan KOMPONEN PrologScreen di Awake
///   (Start-nya jadi tertunda), TAPI GameObject-nya tetap aktif sehingga
///   Day1Intro tetap mendeteksinya dan ikut menunggu. Saat pemain menekan
///   "MULAI GAME", komponen PrologScreen diaktifkan lagi → prolog berjalan normal.
///
/// Pemakaian:
///   1. Buat GameObject kosong "[MainMenu]" di scene → Add Component MainMenu.
///   2. Biarkan tampilkanMenu = true. UI dibangun otomatis saat Play.
///
/// Catatan: tidak menyentuh script lain. Bila tidak ada PrologScreen di scene,
/// menu tetap tampil dan "MULAI GAME" hanya menutup menu.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Aktif")]
    [Tooltip("Matikan untuk melewati menu (prolog langsung jalan seperti semula).")]
    public bool tampilkanMenu = true;

    [Header("Identitas Game")]
    public string judulGame = "RARA";
    public string subJudul  = "Jaga Dirimu!";
    public string versiTeks = "v1.0";

    [Header("Latar")]
    [Tooltip("Sprite latar penuh layar (opsional). Kosong = gradien warna.")]
    public Sprite latarSprite;
    public Color latarWarnaAtas  = new Color(0.09f, 0.16f, 0.31f, 1f);
    public Color latarWarnaBawah = new Color(0.05f, 0.09f, 0.18f, 1f);

    [Header("Warna")]
    public Color warnaJudul     = new Color(1f, 0.85f, 0.30f, 1f);
    public Color warnaSubJudul  = new Color(0.95f, 0.97f, 1f, 1f);
    public Color warnaMulai     = new Color(0.15f, 0.68f, 0.38f, 1f);   // hijau
    public Color warnaNetral    = new Color(0.20f, 0.45f, 0.72f, 1f);   // biru
    public Color warnaKeluar    = new Color(0.84f, 0.27f, 0.22f, 1f);   // merah
    public Color warnaTeksTombol = Color.white;

    [Header("Font (opsional)")]
    public TMP_FontAsset fontAsset;

    [Header("Audio")]
    public bool mainkanBgmMenu = true;

    [Header("Sorting")]
    public int sortingOrder = 11000;

    // ── Runtime ──────────────────────────────────────────────────────────────
    private GameObject   _root;        // canvas menu utama
    private GameObject   _overlayPanel; // panel modal aktif (settings/kontrol/tentang/keluar)
    private PrologScreen _prolog;
    private bool         _mulaiDitekan;
    private static Sprite _spriteRound;

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        if (!tampilkanMenu) return;

        // Tahan prolog: nonaktifkan komponen agar Start-nya tertunda.
        // GameObject tetap aktif → Day1Intro tetap melihatnya & ikut menunggu.
        _prolog = FindFirstObjectByType<PrologScreen>(FindObjectsInactive.Include);
        if (_prolog != null) _prolog.enabled = false;
    }

    void Start()
    {
        if (!tampilkanMenu) return;

        GameSettings.Init();
        BuildUI();

        if (mainkanBgmMenu && AudioManager.Instance != null)
            AudioManager.Instance.PlayBGM(AudioManager.BGMTrack.Menu);
    }

    // ══════════════════════════════════════════════════════════════════════
    // BUILD UI UTAMA
    // ══════════════════════════════════════════════════════════════════════
    void BuildUI()
    {
        _root = new GameObject("MainMenuCanvas");
        var cv = _root.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = sortingOrder;
        var scaler = _root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        // Expand: seluruh area referensi DIJAMIN muat di layar (tidak ada UI terpotong)
        // pada rasio apa pun — 16:9, 18:9, 19.5:9, tablet 4:3, hingga ultrawide.
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.Expand;
        scaler.matchWidthOrHeight  = 0.5f;
        _root.AddComponent<GraphicRaycaster>();

        // ── Latar ──────────────────────────────────────────────────────────
        var bg = NewUI("Latar", _root.transform);
        StretchFull(bg);
        var bgImg = bg.AddComponent<Image>();
        if (latarSprite != null)
        {
            bgImg.sprite        = latarSprite;
            bgImg.preserveAspect = false;
            bgImg.color          = Color.white;
        }
        else
        {
            bgImg.color = latarWarnaBawah;
            // Lapis gradien sederhana di atasnya
            var grad = NewUI("Gradien", bg.transform);
            StretchFull(grad);
            var gImg = grad.AddComponent<Image>();
            gImg.color = latarWarnaAtas;
            var gRT = grad.GetComponent<RectTransform>();
            gRT.anchorMin = new Vector2(0f, 0.45f);
            gRT.anchorMax = new Vector2(1f, 1f);
            gImg.raycastTarget = false;
        }
        bgImg.raycastTarget = true; // blok klik ke layer di bawah

        // ── Kartu judul ──────────────────────────────────────────────────────
        // Pakai anchor PECAHAN (proporsional terhadap tinggi layar) supaya tata
        // letak tetap seimbang di berbagai rasio, bukan offset piksel tetap.
        var judul = MakeText(_root.transform, "Judul", judulGame, 120, warnaJudul, FontStyles.Bold);
        var jRT = judul.rectTransform;
        jRT.anchorMin = jRT.anchorMax = new Vector2(0.5f, 0.82f);
        jRT.pivot     = new Vector2(0.5f, 0.5f);
        jRT.anchoredPosition = Vector2.zero;
        jRT.sizeDelta = new Vector2(1100f, 180f);

        var sub = MakeText(_root.transform, "SubJudul", subJudul, 54, warnaSubJudul, FontStyles.Bold);
        var sRT = sub.rectTransform;
        sRT.anchorMin = sRT.anchorMax = new Vector2(0.5f, 0.70f);
        sRT.pivot     = new Vector2(0.5f, 0.5f);
        sRT.anchoredPosition = Vector2.zero;
        sRT.sizeDelta = new Vector2(1000f, 90f);

        // ── Tombol-tombol ───────────────────────────────────────────────────
        // Dibungkus dalam container ber-VerticalLayoutGroup yang terpusat dengan
        // tinggi otomatis (ContentSizeFitter). Jarak antar tombol selalu rata dan
        // seluruh blok dijamin muat & seimbang di semua rasio layar.
        var stack = NewUI("TombolStack", _root.transform);
        var stRT = stack.GetComponent<RectTransform>();
        stRT.anchorMin = stRT.anchorMax = new Vector2(0.5f, 0.5f);
        stRT.pivot     = new Vector2(0.5f, 0.5f);
        stRT.anchoredPosition = new Vector2(0f, -90f);
        stRT.sizeDelta = new Vector2(460f, 0f);

        var vlg = stack.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment        = TextAnchor.MiddleCenter;
        vlg.spacing               = 18f;
        vlg.childControlWidth     = true;
        vlg.childControlHeight    = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var fit = stack.AddComponent<ContentSizeFitter>();
        fit.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;
        fit.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        MakeButton(stack.transform, "BtnMulai",  "\u25B6  MULAI GAME",   warnaMulai,  0f, 460f, 80, MulaiGame,       layoutChild: true);
        MakeButton(stack.transform, "BtnKontrol","\uD83C\uDFAE  CARA BERMAIN", warnaNetral, 0f, 460f, 70, BukaKontrol,    layoutChild: true);
        MakeButton(stack.transform, "BtnSetting","\u2699  PENGATURAN",   warnaNetral, 0f, 460f, 70, BukaPengaturan, layoutChild: true);
        MakeButton(stack.transform, "BtnTentang","\u2139  TENTANG",      warnaNetral, 0f, 460f, 70, BukaTentang,    layoutChild: true);
        MakeButton(stack.transform, "BtnKeluar", "\u2716  KELUAR",       warnaKeluar, 0f, 460f, 70, KonfirmasiKeluar, layoutChild: true);

        // ── Footer versi ──────────────────────────────────────────────────────
        var ver = MakeText(_root.transform, "Versi", versiTeks, 24,
                           new Color(1f, 1f, 1f, 0.45f), FontStyles.Normal);
        var vRT = ver.rectTransform;
        vRT.anchorMin = new Vector2(1f, 0f);
        vRT.anchorMax = new Vector2(1f, 0f);
        vRT.pivot     = new Vector2(1f, 0f);
        vRT.anchoredPosition = new Vector2(-24f, 18f);
        vRT.sizeDelta = new Vector2(220f, 40f);
        ver.alignment = TextAlignmentOptions.Right;
    }

    // ══════════════════════════════════════════════════════════════════════
    // AKSI TOMBOL
    // ══════════════════════════════════════════════════════════════════════
    void MulaiGame()
    {
        if (_mulaiDitekan) return;
        _mulaiDitekan = true;
        if (AudioManager.Instance != null) AudioManager.Instance.Click();
        StartCoroutine(MulaiRutin());
    }

    IEnumerator MulaiRutin()
    {
        // Fade out menu (atau langsung tutup bila Reduce Motion)
        if (!GameSettings.ReduceMotion)
        {
            var cg = _root.GetComponent<CanvasGroup>();
            if (cg == null) cg = _root.AddComponent<CanvasGroup>();
            float t = 0f;
            const float dur = 0.35f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(1f, 0f, t / dur);
                yield return null;
            }
        }

        // Aktifkan kembali prolog → Start-nya berjalan sekarang.
        if (_prolog != null) _prolog.enabled = true;

        // Hancurkan kanvas menu (objek runtime terpisah).
        if (_root != null) Destroy(_root);

        // PENTING: JANGAN Destroy(gameObject).
        // Panduan setup menyarankan menaruh MainMenu "di GameObject mana saja",
        // sehingga komponen ini SERING menempel di GameObject yang SAMA dengan
        // PrologScreen. Memanggil Destroy(gameObject) akan ikut menghancurkan
        // PrologScreen (dan Day1Intro) sebelum Start prolog sempat berjalan →
        // prolog TIDAK pernah tampil. Cukup hancurkan komponen MainMenu ini saja
        // agar aman baik saat satu-GameObject maupun GameObject terpisah.
        Destroy(this);
    }

    // ══════════════════════════════════════════════════════════════════════
    // PANEL: CARA BERMAIN (kontrol)
    // ══════════════════════════════════════════════════════════════════════
    void BukaKontrol()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.Click();
        var card = BukaModal("\uD83C\uDFAE  CARA BERMAIN", 760f, 640f);

        string isi =
            "<b>Komputer (Keyboard):</b>\n" +
            "\u2022 <b>A / D</b> atau <b>\u2190 / \u2192</b>  \u2014  jalan\n" +
            "\u2022 <b>Shift</b>  \u2014  lari\n" +
            "\u2022 <b>Spasi</b> (tahan)  \u2014  TERIAK minta tolong\n" +
            "\u2022 <b>Spasi / Klik</b>  \u2014  lanjut dialog\n" +
            "\u2022 <b>Esc / P</b>  \u2014  jeda (pause)\n\n" +
            "<b>HP / Tablet (Sentuh):</b>\n" +
            "\u2022 Tombol panah kiri-bawah  \u2014  jalan\n" +
            "\u2022 Tombol <b>LARI</b> & <b>TERIAK</b> kanan-bawah\n" +
            "\u2022 Ketuk tombol pilihan untuk menjawab\n\n" +
            "<b>Tujuanmu:</b>\n" +
            "Jaga 3 hati \u2764, kumpulkan skor dengan memilih tindakan " +
            "<color=#26AD61>AMAN</color>, dan belajar cara melindungi diri " +
            "dari situasi berbahaya. Berani <color=#F29D12>TERIAK</color> dan " +
            "<color=#339FDB>LAPOR</color> kalau merasa tidak aman!";

        var teks = MakeText(card.transform, "Isi", isi, 26,
                            new Color(0.93f, 0.95f, 1f, 1f), FontStyles.Normal);
        teks.alignment = TextAlignmentOptions.TopLeft;
        var tRT = teks.rectTransform;
        tRT.anchorMin = new Vector2(0f, 0f);
        tRT.anchorMax = new Vector2(1f, 1f);
        tRT.offsetMin = new Vector2(44f, 96f);
        tRT.offsetMax = new Vector2(-44f, -96f);

        TombolTutup(card);
    }

    // ══════════════════════════════════════════════════════════════════════
    // PANEL: PENGATURAN
    // ══════════════════════════════════════════════════════════════════════
    void BukaPengaturan()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.Click();
        var card = BukaModal("\u2699  PENGATURAN", 640f, 600f);

        float y = -110f;
        LabelSeksi(card, "\uD83D\uDD0A  Volume", ref y);
        Slider(card, "Volume Master", GameSettings.MasterVolume, 0f, 1f, ref y,
               v => GameSettings.MasterVolume = v);
        Toggle(card, "Musik Latar", GameSettings.MusicOn, ref y,
               on => GameSettings.MusicOn = on);

        y -= 14f;
        LabelSeksi(card, "\uD83D\uDD24  Ukuran Font", ref y);
        Slider(card, "Skala Teks", GameSettings.FontScale, 0.8f, 1.6f, ref y,
               v => GameSettings.FontScale = v);

        y -= 14f;
        LabelSeksi(card, "\u267F  Aksesibilitas", ref y);
        Toggle(card, "Kurangi Animasi", GameSettings.ReduceMotion, ref y,
               on => GameSettings.ReduceMotion = on);

        TombolTutup(card);
    }

    // ══════════════════════════════════════════════════════════════════════
    // PANEL: TENTANG
    // ══════════════════════════════════════════════════════════════════════
    void BukaTentang()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.Click();
        var card = BukaModal("\u2139  TENTANG", 700f, 520f);

        string isi =
            "<b>RARA: Jaga Dirimu!</b>\n\n" +
            "Game edukasi untuk mengenali situasi berbahaya " +
            "(orang asing, perundungan, pelecehan) dan belajar " +
            "mengambil keputusan yang tepat untuk melindungi diri.\n\n" +
            "Ingat 3 KATA SAKTI:\n" +
            "<color=#F29D12><b>TIDAK!  \u2014  PERGI!  \u2014  CERITA!</b></color>\n\n" +
            "\uD83C\uDD98  Nomor Darurat:\n" +
            "Polisi 110   \u2022   Hotline Anak 129   \u2022   KPAI 021-31901556";

        var teks = MakeText(card.transform, "Isi", isi, 26,
                            new Color(0.93f, 0.95f, 1f, 1f), FontStyles.Normal);
        teks.alignment = TextAlignmentOptions.Top;
        var tRT = teks.rectTransform;
        tRT.anchorMin = new Vector2(0f, 0f);
        tRT.anchorMax = new Vector2(1f, 1f);
        tRT.offsetMin = new Vector2(44f, 96f);
        tRT.offsetMax = new Vector2(-44f, -96f);

        TombolTutup(card);
    }

    // ══════════════════════════════════════════════════════════════════════
    // PANEL: KONFIRMASI KELUAR
    // ══════════════════════════════════════════════════════════════════════
    void KonfirmasiKeluar()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.Click();
        var card = BukaModal("\u2716  KELUAR", 620f, 360f);

        var teks = MakeText(card.transform, "Isi",
            "Yakin ingin keluar dari game?\nProgres yang belum selesai tidak tersimpan.",
            28, new Color(0.93f, 0.95f, 1f, 1f), FontStyles.Normal);
        teks.alignment = TextAlignmentOptions.Top;
        var tRT = teks.rectTransform;
        tRT.anchorMin = new Vector2(0f, 1f);
        tRT.anchorMax = new Vector2(1f, 1f);
        tRT.pivot     = new Vector2(0.5f, 1f);
        tRT.anchoredPosition = new Vector2(0f, -110f);
        tRT.sizeDelta = new Vector2(-80f, 120f);

        // Dua tombol: BATAL & KELUAR
        MakeButton(card.transform, "BtnBatal", "BATAL", warnaNetral, -90f, 220f, 70,
                   TutupModal, posX: -130f);
        MakeButton(card.transform, "BtnKeluarYa", "KELUAR", warnaKeluar, -90f, 220f, 70,
                   KeluarSekarang, posX: 130f);
    }

    void KeluarSekarang()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.Click();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ══════════════════════════════════════════════════════════════════════
    // HELPER MODAL
    // ══════════════════════════════════════════════════════════════════════
    /// Buat overlay gelap + kartu tengah. Klik area gelap = tutup.
    GameObject BukaModal(string judul, float lebar, float tinggi)
    {
        TutupModal(); // hanya satu modal aktif

        _overlayPanel = NewUI("Overlay", _root.transform);
        StretchFull(_overlayPanel);
        var ovImg = _overlayPanel.AddComponent<Image>();
        ovImg.color = new Color(0f, 0f, 0f, 0.72f);
        var ovBtn = _overlayPanel.AddComponent<Button>();
        ovBtn.transition = Selectable.Transition.None;
        ovBtn.onClick.AddListener(TutupModal);

        var card = NewUI("Kartu", _overlayPanel.transform);
        var cRT = card.GetComponent<RectTransform>();
        cRT.anchorMin = cRT.anchorMax = new Vector2(0.5f, 0.5f);
        cRT.pivot     = new Vector2(0.5f, 0.5f);
        cRT.sizeDelta = new Vector2(lebar, tinggi);
        var cImg = card.AddComponent<Image>();
        cImg.sprite = GetRoundedSprite();
        cImg.type   = Image.Type.Sliced;
        cImg.color  = new Color(0.12f, 0.18f, 0.30f, 1f);
        // Tahan klik agar tidak menutup saat mengenai kartu
        card.AddComponent<Button>().transition = Selectable.Transition.None;

        // Judul kartu
        var jt = MakeText(card.transform, "JudulKartu", judul, 38, warnaJudul, FontStyles.Bold);
        var jRT = jt.rectTransform;
        jRT.anchorMin = new Vector2(0f, 1f);
        jRT.anchorMax = new Vector2(1f, 1f);
        jRT.pivot     = new Vector2(0.5f, 1f);
        jRT.anchoredPosition = new Vector2(0f, -28f);
        jRT.sizeDelta = new Vector2(-60f, 56f);

        return card;
    }

    void TutupModal()
    {
        if (_overlayPanel != null) { Destroy(_overlayPanel); _overlayPanel = null; }
    }

    void TombolTutup(GameObject card)
    {
        MakeButton(card.transform, "BtnTutup", "TUTUP", warnaNetral, 36f, 240f, 66,
                   TutupModal, anchorBottom: true);
    }

    // ══════════════════════════════════════════════════════════════════════
    // HELPER WIDGET PENGATURAN
    // ══════════════════════════════════════════════════════════════════════
    void LabelSeksi(GameObject card, string teks, ref float y)
    {
        var t = MakeText(card.transform, "Seksi_" + teks, teks, 26, warnaJudul, FontStyles.Bold);
        t.alignment = TextAlignmentOptions.Left;
        var rt = t.rectTransform;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot     = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.offsetMin = new Vector2(40f, rt.offsetMin.y);
        rt.offsetMax = new Vector2(-40f, rt.offsetMax.y);
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, 34f);
        y -= 44f;
    }

    void Slider(GameObject card, string nama, float nilai, float min, float max,
                ref float y, Action<float> onChange)
    {
        var lbl = MakeText(card.transform, nama + "_lbl", nama, 22,
                           new Color(0.9f, 0.93f, 1f, 1f), FontStyles.Normal);
        lbl.alignment = TextAlignmentOptions.Left;
        var lRT = lbl.rectTransform;
        lRT.anchorMin = new Vector2(0f, 1f);
        lRT.anchorMax = new Vector2(1f, 1f);
        lRT.pivot     = new Vector2(0.5f, 1f);
        lRT.anchoredPosition = new Vector2(0f, y);
        lRT.offsetMin = new Vector2(40f, lRT.offsetMin.y);
        lRT.offsetMax = new Vector2(-40f, lRT.offsetMax.y);
        lRT.sizeDelta = new Vector2(lRT.sizeDelta.x, 28f);

        var nilaiLbl = MakeText(card.transform, nama + "_val",
                                Mathf.RoundToInt(nilai * 100f) + "%", 22,
                                warnaJudul, FontStyles.Bold);
        nilaiLbl.alignment = TextAlignmentOptions.Right;
        var nRT = nilaiLbl.rectTransform;
        nRT.anchorMin = new Vector2(0f, 1f);
        nRT.anchorMax = new Vector2(1f, 1f);
        nRT.pivot     = new Vector2(0.5f, 1f);
        nRT.anchoredPosition = new Vector2(0f, y);
        nRT.offsetMin = new Vector2(40f, nRT.offsetMin.y);
        nRT.offsetMax = new Vector2(-40f, nRT.offsetMax.y);
        nRT.sizeDelta = new Vector2(nRT.sizeDelta.x, 28f);

        y -= 38f;

        // Track
        var track = NewUI(nama + "_track", card.transform);
        var trRT = track.GetComponent<RectTransform>();
        trRT.anchorMin = new Vector2(0f, 1f);
        trRT.anchorMax = new Vector2(1f, 1f);
        trRT.pivot     = new Vector2(0.5f, 1f);
        trRT.anchoredPosition = new Vector2(0f, y);
        trRT.offsetMin = new Vector2(40f, trRT.offsetMin.y);
        trRT.offsetMax = new Vector2(-40f, trRT.offsetMax.y);
        trRT.sizeDelta = new Vector2(trRT.sizeDelta.x, 26f);

        var slider = track.AddComponent<UnityEngine.UI.Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value    = nilai;

        var bg = NewUI("Background", track.transform);
        StretchFull(bg);
        var bgImg = bg.AddComponent<Image>();
        bgImg.sprite = GetRoundedSprite();
        bgImg.type   = Image.Type.Sliced;
        bgImg.color  = new Color(0f, 0f, 0f, 0.45f);

        var fillArea = NewUI("Fill Area", track.transform);
        var faRT = fillArea.GetComponent<RectTransform>();
        faRT.anchorMin = new Vector2(0f, 0.5f);
        faRT.anchorMax = new Vector2(1f, 0.5f);
        faRT.offsetMin = new Vector2(8f, -13f);
        faRT.offsetMax = new Vector2(-8f, 13f);

        var fill = NewUI("Fill", fillArea.transform);
        var fRT = fill.GetComponent<RectTransform>();
        fRT.anchorMin = new Vector2(0f, 0f);
        fRT.anchorMax = new Vector2(1f, 1f);
        fRT.sizeDelta = Vector2.zero;
        var fImg = fill.AddComponent<Image>();
        fImg.sprite = GetRoundedSprite();
        fImg.type   = Image.Type.Sliced;
        fImg.color  = warnaMulai;

        var handleArea = NewUI("Handle Slide Area", track.transform);
        var haRT = handleArea.GetComponent<RectTransform>();
        haRT.anchorMin = new Vector2(0f, 0f);
        haRT.anchorMax = new Vector2(1f, 1f);
        haRT.offsetMin = new Vector2(10f, 0f);
        haRT.offsetMax = new Vector2(-10f, 0f);

        var handle = NewUI("Handle", handleArea.transform);
        var hRT = handle.GetComponent<RectTransform>();
        hRT.sizeDelta = new Vector2(26f, 26f);
        var hImg = handle.AddComponent<Image>();
        hImg.sprite = GetRoundedSprite();
        hImg.type   = Image.Type.Sliced;
        hImg.color  = Color.white;

        slider.fillRect       = fRT;
        slider.handleRect     = hRT;
        slider.targetGraphic  = hImg;
        slider.direction      = UnityEngine.UI.Slider.Direction.LeftToRight;

        slider.onValueChanged.AddListener(v =>
        {
            onChange(v);
            nilaiLbl.text = Mathf.RoundToInt(v * 100f) + "%";
        });

        y -= 44f;
    }

    void Toggle(GameObject card, string nama, bool nilai, ref float y, Action<bool> onChange)
    {
        var row = NewUI(nama + "_toggle", card.transform);
        var rRT = row.GetComponent<RectTransform>();
        rRT.anchorMin = new Vector2(0f, 1f);
        rRT.anchorMax = new Vector2(1f, 1f);
        rRT.pivot     = new Vector2(0.5f, 1f);
        rRT.anchoredPosition = new Vector2(0f, y);
        rRT.offsetMin = new Vector2(40f, rRT.offsetMin.y);
        rRT.offsetMax = new Vector2(-40f, rRT.offsetMax.y);
        rRT.sizeDelta = new Vector2(rRT.sizeDelta.x, 44f);

        var lbl = MakeText(row.transform, "lbl", nama, 22,
                           new Color(0.9f, 0.93f, 1f, 1f), FontStyles.Normal);
        lbl.alignment = TextAlignmentOptions.Left;
        var lRT = lbl.rectTransform;
        lRT.anchorMin = new Vector2(0f, 0f);
        lRT.anchorMax = new Vector2(0.7f, 1f);
        lRT.offsetMin = Vector2.zero;
        lRT.offsetMax = Vector2.zero;

        var box = NewUI("Box", row.transform);
        var bRT = box.GetComponent<RectTransform>();
        bRT.anchorMin = new Vector2(1f, 0.5f);
        bRT.anchorMax = new Vector2(1f, 0.5f);
        bRT.pivot     = new Vector2(1f, 0.5f);
        bRT.anchoredPosition = new Vector2(0f, 0f);
        bRT.sizeDelta = new Vector2(56f, 36f);
        var boxImg = box.AddComponent<Image>();
        boxImg.sprite = GetRoundedSprite();
        boxImg.type   = Image.Type.Sliced;
        boxImg.color  = new Color(0f, 0f, 0f, 0.45f);

        var tgl = box.AddComponent<UnityEngine.UI.Toggle>();
        tgl.isOn = nilai;

        var check = NewUI("Check", box.transform);
        StretchFull(check);
        var cRT = check.GetComponent<RectTransform>();
        cRT.offsetMin = new Vector2(6f, 6f);
        cRT.offsetMax = new Vector2(-6f, -6f);
        var cImg = check.AddComponent<Image>();
        cImg.sprite = GetRoundedSprite();
        cImg.type   = Image.Type.Sliced;
        cImg.color  = warnaMulai;

        tgl.graphic = cImg;
        tgl.targetGraphic = boxImg;
        tgl.onValueChanged.AddListener(on => onChange(on));

        y -= 52f;
    }

    // ══════════════════════════════════════════════════════════════════════
    // HELPER UI DASAR
    // ══════════════════════════════════════════════════════════════════════
    GameObject NewUI(string nama, Transform parent)
    {
        var go = new GameObject(nama, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    void StretchFull(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    TextMeshProUGUI MakeText(Transform parent, string nama, string isi, int ukuran,
                             Color warna, FontStyles gaya)
    {
        var go = NewUI(nama, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = isi;
        tmp.fontSize  = ukuran;
        tmp.color     = warna;
        tmp.fontStyle = gaya;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = true;
        tmp.raycastTarget = false;
        if (fontAsset != null) tmp.font = fontAsset;
        return tmp;
    }

    /// Buat tombol. posX = offset X dari tengah (0 = tengah). anchorBottom = jangkar ke
    /// bawah kartu (untuk tombol TUTUP). anchorYFrac >= 0 = jangkar pada pecahan tinggi
    /// layar (0..1) agar posisi proporsional/responsif. layoutChild = jadi anak
    /// VerticalLayoutGroup (ukuran diatur LayoutElement, posisi oleh layout group).
    void MakeButton(Transform parent, string nama, string label, Color warna, float posY,
                    float lebar, int tinggi, Action onClick,
                    float posX = 0f, bool anchorBottom = false, float anchorYFrac = -1f,
                    bool layoutChild = false)
    {
        var go = NewUI(nama, parent);
        var rt = go.GetComponent<RectTransform>();
        if (layoutChild)
        {
            // Ukuran ditentukan LayoutElement; posisi diatur oleh VerticalLayoutGroup.
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = tinggi;
            le.minHeight       = tinggi;
            le.preferredWidth  = lebar;
        }
        else if (anchorYFrac >= 0f)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, anchorYFrac);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(posX, posY);
            rt.sizeDelta = new Vector2(lebar, tinggi);
        }
        else if (anchorBottom)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot     = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(posX, posY);
            rt.sizeDelta = new Vector2(lebar, tinggi);
        }
        else
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(posX, posY);
            rt.sizeDelta = new Vector2(lebar, tinggi);
        }

        var img = go.AddComponent<Image>();
        img.sprite = GetRoundedSprite();
        img.type   = Image.Type.Sliced;
        img.color  = warna;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.highlightedColor = new Color(
            Mathf.Min(1f, warna.r + 0.12f),
            Mathf.Min(1f, warna.g + 0.12f),
            Mathf.Min(1f, warna.b + 0.12f), 1f);
        colors.pressedColor = new Color(warna.r * 0.85f, warna.g * 0.85f, warna.b * 0.85f, 1f);
        btn.colors = colors;
        btn.onClick.AddListener(() => onClick());

        var tmp = MakeText(go.transform, "Label", label,
                           Mathf.RoundToInt(tinggi * 0.42f), warnaTeksTombol, FontStyles.Bold);
        StretchFull(tmp.gameObject);
    }

    // ══════════════════════════════════════════════════════════════════════
    // SPRITE SUDUT MEMBULAT (dibuat sekali, di-cache)
    // ══════════════════════════════════════════════════════════════════════
    static Sprite GetRoundedSprite()
    {
        if (_spriteRound != null) return _spriteRound;

        const int size = 48;
        const int radius = 14;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int yy = 0; yy < size; yy++)
        for (int xx = 0; xx < size; xx++)
        {
            bool inside = true;
            // sudut kiri-bawah
            if (xx < radius && yy < radius)
                inside = (xx - radius) * (xx - radius) + (yy - radius) * (yy - radius) <= radius * radius;
            else if (xx > size - radius && yy < radius)
                inside = (xx - (size - radius)) * (xx - (size - radius)) + (yy - radius) * (yy - radius) <= radius * radius;
            else if (xx < radius && yy > size - radius)
                inside = (xx - radius) * (xx - radius) + (yy - (size - radius)) * (yy - (size - radius)) <= radius * radius;
            else if (xx > size - radius && yy > size - radius)
                inside = (xx - (size - radius)) * (xx - (size - radius)) + (yy - (size - radius)) * (yy - (size - radius)) <= radius * radius;

            tex.SetPixel(xx, yy, inside ? Color.white : new Color(1f, 1f, 1f, 0f));
        }
        tex.Apply();
        tex.wrapMode   = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        _spriteRound = Sprite.Create(tex, new Rect(0, 0, size, size),
                                     new Vector2(0.5f, 0.5f), 100f, 0,
                                     SpriteMeshType.FullRect,
                                     new Vector4(radius, radius, radius, radius));
        return _spriteRound;
    }
}
