using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// LaporTeriakButton — Tantangan "Berani Lapor" Day 2 (KLIMAKS: kata sakti CERITA).
///
/// Konteks alur: MASIH di dalam angkot. Pria asing yang sama (dari halte, yang
/// tadi menyentuh bahu Rara) kembali merapat dan makin nekat. Rara harus berani
/// TERIAK memanggil Pak Supir — inilah penutup rangkaian TIDAK → PERGI → CERITA.
///
/// Pemain harus TAHAN tombol "TERIAK!" selama N detik berturut-turut
/// dalam window waktu terbatas. Kalau berhasil:
///   - Bonus poin LAPOR
///   - Achievement "Berani Lapor"
///   - Reaksi sukses: Pak Supir & penumpang menolong, angkot tiba di sekolah.
///
/// Tap-only mode \u2014 tidak butuh mikrofon. Cocok untuk semua platform.
/// Semua label/durasi/warna bisa di-custom lewat Inspector.
/// </summary>
public class LaporTeriakButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Narasi Awal")]
    public string judulTeks = "\uD83D\uDCE2 SAATNYA LAPOR!";
    public Color  judulWarna = new Color(1f, 0.45f, 0.45f, 1f);
    public int    judulUkuran = 38;
    [TextArea(2, 5)]
    public string deskripsiTeks =
        "Pria itu makin merapat di dalam angkot!\nTAHAN tombol TERIAK (atau tahan SPACE) untuk memanggil Pak Supir.\n\nTahan selama {DURASI} detik sebelum waktu habis.";
    public Color  deskripsiWarna = new Color(1f, 1f, 0.92f, 0.95f);
    public int    deskripsiUkuran = 22;

    // ═════════════════════════════════════════════════════════════════════
    // NARASI PEMBUKA VISUAL NOVEL (sebelum tombol teriak / layar LAPOR)
    // Adegan jembatan: pria asing di angkot geser mendekat karena bangku di
    // sebelah Rara kosong -> red flag -> Rara percaya insting -> saatnya CERITA.
    // Ditaruh DI SINI (bukan Day2Controller) supaya PASTI tampil setiap kali
    // layar LAPOR dibuka, dari jalur mana pun fase ini dipanggil.
    // ═════════════════════════════════════════════════════════════════════
    [Header("Narasi Pembuka VN (sebelum tombol teriak)")]
    [Tooltip("ON = mainkan dialog box VN 'pria geser mendekat' dulu sebelum layar teriak.")]
    public bool tampilkanNarasiPembuka = true;
    [Tooltip("Sprite panel kayu untuk box dialog VN (sliced). Kosong = panel gelap + border emas.")]
    public Sprite vnPanelSprite;
    [Tooltip("Portrait pembicara 'Narasi' (mis. gulungan kertas). Opsional.")]
    public Sprite vnPortraitNarasi;
    [Tooltip("Portrait wajah Rara. Opsional.")]
    public Sprite vnPortraitRara;
    [Tooltip("Portrait Pria Asing. Opsional.")]
    public Sprite vnPortraitPria;
    [Tooltip("Sprite latar belakang (mis. interior angkot). Opsional.")]
    public Sprite vnLatarSprite;

    [Header("Timer")]
    [Tooltip("Window total waktu (detik) untuk menyelesaikan tantangan.")]
    public float waktuWindow = 12f;
    [Tooltip("Berapa detik tombol harus DITAHAN BERTURUT-TURUT supaya berhasil.")]
    public float durasiTahan = 1.5f;

    [Header("Tombol Teriak")]
    public string teriakLabel = "\uD83D\uDD0A  TAHAN: TERIAK!";
    public Color  teriakWarna = new Color(0.91f, 0.30f, 0.24f, 1f);
    public Color  teriakWarnaDitekan = new Color(0.20f, 0.78f, 0.40f, 1f);
    public int    teriakUkuran = 28;

    [Header("Progress Bar Teriak")]
    public Color warnaBarBg   = new Color(0.10f, 0.10f, 0.12f, 1f);
    public Color warnaBarFill = new Color(0.20f, 0.78f, 0.40f, 1f);

    [Header("Hasil")]
    public string achievementName = "Berani Lapor";
    public int    bonusBerhasil   = 500;
    [TextArea(2, 4)]
    public string reaksiBerhasil  = "\u2713 Pak Supir mendengar dan langsung menepi! Penumpang lain ikut menoleh — pria itu salah tingkah lalu turun. Angkot kembali melaju dan Rara tiba di sekolah dengan SELAMAT.";
    [TextArea(2, 4)]
    public string reaksiGagal     = "\u2716 Rara terlalu takut untuk bersuara. Untung angkot keburu sampai di sekolah dan Rara cepat turun — tapi lain kali, beranikan diri TERIAK minta tolong, ya!";

    // ═════════════════════════════════════════════════════════════════════
    // ADEGAN ESKALASI DI ANGKOT (Day 2 — penutup rangkaian sentuh bahu)
    // Pria asing yang sama (dari halte, yang tadi menyentuh bahu) kembali
    // merapat di dalam angkot. BERANI teriak panggil Pak Supir (CERITA) →
    // supir menolong, pria turun, angkot tiba di sekolah (AMAN).
    // TIDAK teriak (tombol Diam / waktu habis) → alur berbeda (BAHAYA, -1 nyawa).
    // ═════════════════════════════════════════════════════════════════════
    [Header("Adegan Eskalasi di Angkot (Day 2)")]
    [Tooltip("Aktifkan supaya fase ini memakai narasi 'pria yang sama merapat lagi di angkot' + alur bercabang.\n" +
             "NONAKTIFKAN (false) untuk memakai teks 'Narasi Awal' & 'Hasil' standar di atas (mode in-angkot yang sudah diselaraskan).")]
    public bool tampilkanNarasiPengejaran = false;
    [Tooltip("Judul kartu saat adegan eskalasi di angkot.")]
    public string pengejaranJudul = "\uD83D\uDCE2 MINTA TOLONG SEKARANG!";
    [TextArea(3, 6)]
    [Tooltip("Deskripsi adegan. {DURASI} diganti durasi tahan tombol teriak.")]
    public string pengejaranDeskripsi =
        "Pria asing yang sama menggeser duduknya lagi — makin merapat ke arah Rara!\n" +
        "Pak Supir ada di depan. Inilah saatnya CERITA: minta tolong orang dewasa.\n\n" +
        "TAHAN tombol TERIAK \"PAK, TOLONG!\" (atau tahan SPACE) sebelum waktu habis ({DURASI} dtk).";
    [TextArea(2, 5)]
    [Tooltip("Reaksi saat BERHASIL teriak (Pak Supir menolong, tiba di sekolah).")]
    public string pengejaranReaksiBerhasil =
        "\u2713 Rara berteriak \"PAK, TOLONG!\" sekencang-kencangnya! Pak Supir langsung menepi dan menegur pria itu. " +
        "Penumpang lain ikut menoleh — pria itu salah tingkah lalu turun. Angkot melaju lagi dan Rara tiba di sekolah dengan SELAMAT.";
    [TextArea(2, 5)]
    [Tooltip("Reaksi saat TIDAK teriak / waktu habis (alur berbeda).")]
    public string pengejaranReaksiGagal =
        "\u2716 Rara terlalu takut untuk bersuara. Pria itu makin berani — untung angkot keburu sampai di sekolah dan Rara cepat turun. " +
        "Rara selamat, tapi sangat ketakutan. Lain kali, berani TERIAK minta tolong Pak Supir, ya!";
    [Tooltip("Kategori pilihan saat gagal/tidak teriak.")]
    public string kategoriGagal = "BAHAYA";
    [Tooltip("Kurangi 1 nyawa saat gagal/tidak teriak.")]
    public bool kurangiNyawaSaatGagal = true;

    [Header("Tombol 'Diam saja' (pilih TIDAK teriak)")]
    [Tooltip("Tampilkan tombol supaya pemain bisa memilih tidak teriak — memicu alur berbeda.")]
    public bool tampilkanTombolDiam = true;
    public string diamLabel = "\uD83D\uDE10  Diam saja, takut...";
    public Color  warnaDiam  = new Color(0.55f, 0.45f, 0.20f, 1f);

    [Header("Tombol Lanjut")]
    public string tombolLanjutTeks = "\u25B6  Lanjut ke Kartu Edukasi";
    public Color  warnaLanjut      = new Color(0.20f, 0.62f, 0.86f, 1f);

    [Header("Font")]
    public TMP_FontAsset fontAsset;

    [Header("Sorting")]
    public int sortingOrder = 940;

    [Header("Mode Visual Novel")]
    [Tooltip("Saat ON, klimaks LAPOR disajikan sebagai PILIHAN dialog (bukan mini-game\n" +
             "tahan-tombol): pemain memilih TERIAK panggil Pak Supir (AMAN) atau Diam (BAHAYA).\n\n" +
             "DEFAULT OFF: mekanik TAHAN tombol + Voice-Driven (mic) yang interaktif sengaja\n" +
             "dipertahankan. Set true hanya kalau ingin versi pilihan murni.")]
    public bool modeVisualNovel = false;
    [Tooltip("Label tombol pilihan TERIAK saat mode Visual Novel aktif.")]
    public string vnTeriakLabel = "\uD83D\uDCE2  TERIAK panggil Pak Supir!";

    [Header("Voice-Driven Action (Mikrofon)")]
    [Tooltip("Aktifkan supaya tombol TERIAK bisa diisi dengan BERTERIAK ke mikrofon asli.\n" +
             "Kalau mic tidak tersedia / izin ditolak \u2192 otomatis fallback ke TAHAN tombol.")]
    public bool gunakanMikrofon = false;
    [Tooltip("Sensitivitas mikrofon (kalikan loudness mentah). Naikkan kalau suara terlalu pelan.")]
    [Range(1f, 40f)] public float sensitivitasMic = 12f;
    [Tooltip("Ambang loudness (0\u20131) yang dianggap 'berteriak'. Di atas ini, progress terisi.")]
    [Range(0.2f, 0.95f)] public float ambangTeriak = 0.6f;
    [Tooltip("Label tombol saat mode mikrofon aktif (petunjuk berteriak).")]
    public string teriakLabelMic = "\uD83C\uDF99  TERIAK ke mic!";

    // ── runtime ───────────────────────────────────────────────────────────
    private Action     _onSelesai;
    private GameObject _canvasGO;
    private TextMeshProUGUI _timerText;
    private Image      _barFill;
    private Image      _tombolImg;
    private TextMeshProUGUI _tombolLabel;
    private float      _sisaWaktu;
    private float      _holdProgress;
    private bool       _ditekan;
    private bool       _selesai;
    private bool       _berhasil;
    private Sprite     _roundedSprite;

    // Voice-Driven (mikrofon)
    private bool       _micAktif;
    private AudioClip  _micClip;
    private string     _micDevice;
    private float      _micLevel;

    // ══════════════════════════════════════════════════════════════════════
    public void Mulai(Action onSelesai)
    {
        _onSelesai = onSelesai;
        // Mainkan narasi pembuka VN dulu (pria geser mendekat) baru layar teriak.
        if (tampilkanNarasiPembuka) StartCoroutine(JalankanNarasiLaluScene());
        else MulaiScene();
    }

    // Jalankan narasi VN pembuka, lalu lanjut ke layar tombol teriak.
    IEnumerator JalankanNarasiLaluScene()
    {
        yield return NarasiPembukaVN();
        MulaiScene();
    }

    // Bangun layar tombol teriak (mini-game) + mic + timer.
    void MulaiScene()
    {
        BuildScene();
        // Mode VN = pilihan murni, tanpa tekanan waktu. Timer hanya untuk mekanik tahan tombol.
        if (!modeVisualNovel)
        {
            MulaiMikrofon();
            StartCoroutine(TimerCoroutine());
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // NARASI PEMBUKA VN — box dialog (portrait + nama + teks) tema visual novel.
    // 6 baris: bangku kosong -> pria pindah mendekat (modus) -> Rara curiga
    // (red flag, percaya insting) -> saatnya CERITA (minta tolong Pak Supir).
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator NarasiPembukaVN()
    {
        var baris = new (string speaker, string teks)[]
        {
            ("Narasi",     "Rara memasukkan kembali HP-nya ke saku. Tapi suasana di dalam angkot terasa berubah."),
            ("Narasi",     "Beberapa penumpang turun di perempatan. Kini bangku tepat di sebelah Rara kosong."),
            ("Pria Asing", "Wah, kosong nih. Om pindah ke sini aja ya, biar lebih enak ngobrolnya."),
            ("Narasi",     "Pria yang tadi menyentuh bahunya itu menggeser duduknya \u2014 makin merapat ke arah Rara."),
            ("Rara",       "Kenapa dia harus pindah ke sebelahku? Padahal masih banyak bangku lain yang kosong..."),
            ("Narasi",     "Hati kecil Rara berkata ada yang tidak beres. Inilah saatnya kata sakti ketiga: CERITA \u2014 minta tolong Pak Supir!")
        };

        // ── Canvas overlay ──
        var cGO = new GameObject("LaporNarasiPembuka_Canvas");
        var canvas = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder + 5;
        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // ── Latar ──
        var bgGO = new GameObject("BG");
        bgGO.transform.SetParent(cGO.transform, false);
        var bgImg = bgGO.AddComponent<Image>();
        if (vnLatarSprite != null) { bgImg.sprite = vnLatarSprite; bgImg.color = Color.white; }
        else                       bgImg.color = new Color(0.16f, 0.12f, 0.09f, 1f);
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;

        // ── Box dialog (panel kayu / fallback gelap + border emas) ──
        var box = new GameObject("DialogBox");
        box.transform.SetParent(cGO.transform, false);
        var boxImg = box.AddComponent<Image>();
        boxImg.raycastTarget = true;
        if (vnPanelSprite != null)
        {
            boxImg.sprite = vnPanelSprite;
            boxImg.type   = Image.Type.Sliced;
            boxImg.color  = Color.white;
        }
        else
        {
            boxImg.sprite = GetRoundedSprite();
            boxImg.type   = Image.Type.Sliced;
            boxImg.color  = new Color(0.05f, 0.08f, 0.12f, 0.95f);
            var outl = box.AddComponent<Outline>();
            outl.effectColor    = new Color(1f, 0.85f, 0.25f, 1f);
            outl.effectDistance = new Vector2(2f, -2f);
        }
        var bxRT = box.GetComponent<RectTransform>();
        bxRT.anchorMin = new Vector2(0.04f, 0.04f);
        bxRT.anchorMax = new Vector2(0.96f, 0.40f);
        bxRT.offsetMin = Vector2.zero; bxRT.offsetMax = Vector2.zero;

        // ── Portrait (kiri) ──
        var pGO = new GameObject("Portrait");
        pGO.transform.SetParent(box.transform, false);
        var pRT = pGO.AddComponent<RectTransform>();
        pRT.anchorMin = new Vector2(0.025f, 0.14f);
        pRT.anchorMax = new Vector2(0.20f, 0.96f);
        pRT.offsetMin = pRT.offsetMax = Vector2.zero;
        var portraitImg = pGO.AddComponent<Image>();
        portraitImg.preserveAspect = true;
        portraitImg.raycastTarget  = false;
        portraitImg.enabled        = false;

        // ── Banner nama ──
        var namaTmp = BuatTeks(box.transform, "Nama", "", 30, new Color(1f, 0.85f, 0.30f, 1f), FontStyles.Bold);
        namaTmp.alignment = TextAlignmentOptions.Left;
        var nRT = namaTmp.rectTransform;
        nRT.anchorMin = new Vector2(0.225f, 0.74f);
        nRT.anchorMax = new Vector2(0.95f, 0.96f);
        nRT.offsetMin = nRT.offsetMax = Vector2.zero;

        // ── Teks isi ──
        var teksTmp = BuatTeks(box.transform, "Teks", "", 28, Color.white, FontStyles.Normal);
        teksTmp.alignment = TextAlignmentOptions.TopLeft;
        var tRT = teksTmp.rectTransform;
        tRT.anchorMin = new Vector2(0.225f, 0.16f);
        tRT.anchorMax = new Vector2(0.96f, 0.74f);
        tRT.offsetMin = new Vector2(4f, 4f); tRT.offsetMax = new Vector2(-8f, -4f);

        // ── Hint ──
        var hintTmp = BuatTeks(box.transform, "Hint", "\u25BC  Ketuk / SPACE untuk lanjut", 16, new Color(1f,1f,1f,0.55f), FontStyles.Italic);
        hintTmp.alignment = TextAlignmentOptions.MidlineRight;
        var hRT = hintTmp.rectTransform;
        hRT.anchorMin = new Vector2(0.60f, 0.04f);
        hRT.anchorMax = new Vector2(0.965f, 0.15f);
        hRT.offsetMin = hRT.offsetMax = Vector2.zero;

        // ── Klik area penuh untuk maju ──
        var clickGO = new GameObject("ClickArea");
        clickGO.transform.SetParent(cGO.transform, false);
        var clickImg = clickGO.AddComponent<Image>();
        clickImg.color = new Color(0,0,0,0);
        var clickRT = clickGO.GetComponent<RectTransform>();
        clickRT.anchorMin = Vector2.zero; clickRT.anchorMax = Vector2.one;
        clickRT.offsetMin = Vector2.zero; clickRT.offsetMax = Vector2.zero;
        bool diklik = false;
        var clickBtn = clickGO.AddComponent<Button>();
        clickBtn.transition = Selectable.Transition.None;
        clickBtn.onClick.AddListener(() => diklik = true);

        foreach (var b in baris)
        {
            namaTmp.text  = b.speaker.ToUpper();
            namaTmp.color = b.speaker == "Pria Asing" ? new Color(0.95f, 0.45f, 0.40f, 1f)
                          : b.speaker == "Rara"       ? new Color(0.45f, 0.78f, 1.00f, 1f)
                          :                             new Color(1f, 0.85f, 0.30f, 1f);

            Sprite ps = b.speaker == "Rara"       ? vnPortraitRara
                      : b.speaker == "Pria Asing" ? vnPortraitPria
                      :                             vnPortraitNarasi;
            if (ps != null) { portraitImg.sprite = ps; portraitImg.enabled = true; }
            else            { portraitImg.enabled = false; }

            teksTmp.text = b.teks;

            diklik = false;
            float timer = 0f;
            while (!diklik)
            {
                timer += Time.deltaTime;
                if (timer >= 0.2f &&
                    (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
                    diklik = true;
                yield return null;
            }
        }
        Destroy(cGO);
    }

    // Coba nyalakan mikrofon. Kalau gagal/izin ditolak → fallback tahan tombol.
    void MulaiMikrofon()
    {
        if (!gunakanMikrofon) return;
        if (Microphone.devices == null || Microphone.devices.Length == 0) return;
        try
        {
            _micDevice = Microphone.devices[0];
            _micClip   = Microphone.Start(_micDevice, true, 1, 44100);
            _micAktif  = true;
            // Ubah label tombol jadi petunjuk berteriak ke mic.
            if (_tombolLabel != null) _tombolLabel.text = teriakLabelMic;
        }
        catch { _micAktif = false; _micClip = null; }
    }

    void OnDestroy()
    {
        if (_micClip != null)
        {
            try { if (!string.IsNullOrEmpty(_micDevice)) Microphone.End(_micDevice); } catch { }
            _micClip = null;
        }
    }

    // Baca loudness RMS mic terkini (0–1, sudah dikali sensitivitas).
    float BacaLoudnessMic()
    {
        if (_micClip == null) return 0f;
        int pos = Microphone.GetPosition(_micDevice);
        const int window = 256;
        if (pos < window) return 0f;
        var samples = new float[window];
        _micClip.GetData(samples, pos - window);
        float sum = 0f;
        for (int i = 0; i < window; i++) sum += samples[i] * samples[i];
        float rms = Mathf.Sqrt(sum / window);
        return Mathf.Clamp01(rms * sensitivitasMic);
    }

    // ══════════════════════════════════════════════════════════════════════
    void BuildScene()
    {
        _canvasGO = new GameObject("LaporTeriak_Canvas");
        var canvas = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = _canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        _canvasGO.AddComponent<GraphicRaycaster>();

        // Pastikan EventSystem
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // Card panel
        var card = new GameObject("Card");
        card.transform.SetParent(_canvasGO.transform, false);
        var cImg = card.AddComponent<Image>();
        cImg.sprite = GetRoundedSprite();
        cImg.color  = new Color(0.08f, 0.04f, 0.04f, 0.95f);
        cImg.type   = Image.Type.Sliced;
        var cOutl = card.AddComponent<Outline>();
        cOutl.effectColor    = new Color(1f, 0.45f, 0.45f, 1f);
        cOutl.effectDistance = new Vector2(3f, -3f);
        var cRT = card.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0.5f, 0.5f); cRT.anchorMax = new Vector2(0.5f, 0.5f);
        cRT.pivot = new Vector2(0.5f, 0.5f);
        cRT.sizeDelta = new Vector2(1100f, 720f);

        // Judul
        string judulFinal = tampilkanNarasiPengejaran ? pengejaranJudul : judulTeks;
        var j = BuatTeks(card.transform, "Judul", judulFinal, judulUkuran, judulWarna, FontStyles.Bold);
        j.alignment = TextAlignmentOptions.Center;
        var jrt = j.rectTransform;
        jrt.anchorMin = new Vector2(0f, 1f); jrt.anchorMax = new Vector2(1f, 1f);
        jrt.pivot = new Vector2(0.5f, 1f);
        jrt.offsetMin = new Vector2(40f, -90f);
        jrt.offsetMax = new Vector2(-40f, -25f);

        // Deskripsi
        string descSumber = tampilkanNarasiPengejaran ? pengejaranDeskripsi : deskripsiTeks;
        string desc = descSumber.Replace("{DURASI}", durasiTahan.ToString("0.0"));
        if (modeVisualNovel)
        {
            // Mode VN: hilangkan instruksi "tahan tombol/timer", ganti jadi ajakan memilih.
            desc = "Pria asing yang sama kembali merapat di dalam angkot.\n" +
                   "Pak Supir ada di depan. Inilah saatnya CERITA \u2014 minta tolong orang dewasa.\n\n" +
                   "Apa yang Rara lakukan?";
        }
        var d = BuatTeks(card.transform, "Desc", desc, deskripsiUkuran, deskripsiWarna, FontStyles.Normal);
        d.alignment = TextAlignmentOptions.Center;
        var drt = d.rectTransform;
        drt.anchorMin = new Vector2(0f, 1f); drt.anchorMax = new Vector2(1f, 1f);
        drt.pivot = new Vector2(0.5f, 1f);
        drt.offsetMin = new Vector2(40f, -240f);
        drt.offsetMax = new Vector2(-40f, -100f);

        // Timer
        _timerText = BuatTeks(card.transform, "Timer", "", 32, warnaBarFill, FontStyles.Bold);
        _timerText.alignment = TextAlignmentOptions.Center;
        var trt = _timerText.rectTransform;
        trt.anchorMin = new Vector2(0.5f, 1f); trt.anchorMax = new Vector2(0.5f, 1f);
        trt.pivot = new Vector2(0.5f, 1f);
        trt.sizeDelta = new Vector2(360f, 50f);
        trt.anchoredPosition = new Vector2(0f, -290f);

        // ──────────────────────────────────────────────────────────────────
        // MODE VISUAL NOVEL: sajikan klimaks sebagai PILIHAN dialog, bukan
        // mini-game tahan tombol. Tombol "TERIAK" = AMAN (Selesaikan true),
        // tombol "Diam saja" (dibangun di bawah) = BAHAYA.
        // ──────────────────────────────────────────────────────────────────
        if (modeVisualNovel)
        {
            if (_timerText != null) _timerText.gameObject.SetActive(false);

            var vnGO = new GameObject("TombolTeriakVN");
            vnGO.transform.SetParent(card.transform, false);
            var vnImg = vnGO.AddComponent<Image>();
            vnImg.sprite = GetRoundedSprite();
            vnImg.color  = teriakWarna;
            vnImg.type   = Image.Type.Sliced;
            var vnOutl = vnGO.AddComponent<Outline>();
            vnOutl.effectColor    = Color.white;
            vnOutl.effectDistance = new Vector2(3f, -3f);
            var vnRT = vnGO.GetComponent<RectTransform>();
            vnRT.anchorMin = new Vector2(0.5f, 0f); vnRT.anchorMax = new Vector2(0.5f, 0f);
            vnRT.pivot = new Vector2(0.5f, 0f);
            vnRT.sizeDelta = new Vector2(560f, 110f);
            vnRT.anchoredPosition = new Vector2(0f, 120f);

            var vnBtn = vnGO.AddComponent<Button>();
            vnBtn.targetGraphic = vnImg;
            var vnCb = vnBtn.colors;
            vnCb.highlightedColor = new Color(teriakWarna.r * 1.15f, teriakWarna.g * 1.15f, teriakWarna.b * 1.15f, teriakWarna.a);
            vnCb.pressedColor     = new Color(teriakWarna.r * 0.8f, teriakWarna.g * 0.8f, teriakWarna.b * 0.8f, teriakWarna.a);
            vnBtn.colors = vnCb;
            vnBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance?.Click();
                Selesaikan(true);
            });

            var vnLbl = BuatTeks(vnGO.transform, "Label", vnTeriakLabel, teriakUkuran, Color.white, FontStyles.Bold);
            vnLbl.alignment = TextAlignmentOptions.Center;
            vnLbl.raycastTarget = false;
            var vnLrt = vnLbl.rectTransform;
            vnLrt.anchorMin = Vector2.zero; vnLrt.anchorMax = Vector2.one;
            vnLrt.offsetMin = Vector2.zero; vnLrt.offsetMax = Vector2.zero;
        }
        else
        {
        // Tombol Teriak
        var btnGO = new GameObject("TombolTeriak");
        btnGO.transform.SetParent(card.transform, false);
        _tombolImg = btnGO.AddComponent<Image>();
        _tombolImg.sprite = GetRoundedSprite();
        _tombolImg.color  = teriakWarna;
        _tombolImg.type   = Image.Type.Sliced;
        var bOutl = btnGO.AddComponent<Outline>();
        bOutl.effectColor    = Color.white;
        bOutl.effectDistance = new Vector2(3f, -3f);
        var bRT = btnGO.GetComponent<RectTransform>();
        bRT.anchorMin = new Vector2(0.5f, 0f); bRT.anchorMax = new Vector2(0.5f, 0f);
        bRT.pivot = new Vector2(0.5f, 0f);
        bRT.sizeDelta = new Vector2(520f, 140f);
        bRT.anchoredPosition = new Vector2(0f, 150f);

        // Tambah handler ke tombol
        var handler = btnGO.AddComponent<LaporTeriakInputProxy>();
        handler.owner = this;

        _tombolLabel = BuatTeks(btnGO.transform, "Label", teriakLabel, teriakUkuran, Color.white, FontStyles.Bold);
        _tombolLabel.alignment = TextAlignmentOptions.Center;
        _tombolLabel.raycastTarget = false;
        var lrt = _tombolLabel.rectTransform;
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;

        // Bar progress di bawah tombol
        var barBg = new GameObject("BarBG");
        barBg.transform.SetParent(card.transform, false);
        var bgImg = barBg.AddComponent<Image>();
        bgImg.sprite = GetRoundedSprite();
        bgImg.color  = warnaBarBg;
        bgImg.type   = Image.Type.Sliced;
        var bgRT = barBg.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0.5f, 0f); bgRT.anchorMax = new Vector2(0.5f, 0f);
        bgRT.pivot = new Vector2(0.5f, 0f);
        bgRT.sizeDelta = new Vector2(540f, 30f);
        bgRT.anchoredPosition = new Vector2(0f, 100f);

        var fill = new GameObject("Fill");
        fill.transform.SetParent(barBg.transform, false);
        _barFill = fill.AddComponent<Image>();
        _barFill.sprite = GetRoundedSprite();
        _barFill.color  = warnaBarFill;
        _barFill.type   = Image.Type.Sliced;
        var fRT = fill.GetComponent<RectTransform>();
        fRT.anchorMin = new Vector2(0f, 0f); fRT.anchorMax = new Vector2(0f, 1f);
        fRT.pivot = new Vector2(0f, 0.5f);
        fRT.offsetMin = new Vector2(2f, 2f); fRT.offsetMax = new Vector2(2f, -2f);
        fRT.sizeDelta = new Vector2(0f, 0f);
        } // tutup blok mekanik tahan-tombol (non-VN)

        // ── Tombol "Diam saja" (memilih TIDAK teriak → alur berbeda) ──────
        if (tampilkanTombolDiam)
        {
            var diamGO = new GameObject("TombolDiam");
            diamGO.transform.SetParent(card.transform, false);
            var dImg = diamGO.AddComponent<Image>();
            dImg.sprite = GetRoundedSprite();
            dImg.color  = warnaDiam;
            dImg.type   = Image.Type.Sliced;
            var dRT = diamGO.GetComponent<RectTransform>();
            dRT.anchorMin = new Vector2(0.5f, 0f); dRT.anchorMax = new Vector2(0.5f, 0f);
            dRT.pivot = new Vector2(0.5f, 0f);
            dRT.sizeDelta = new Vector2(440f, 54f);
            dRT.anchoredPosition = new Vector2(0f, 35f);

            var dBtn = diamGO.AddComponent<Button>();
            dBtn.targetGraphic = dImg;
            dBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance?.Click();
                Selesaikan(false);
            });

            var dLab = BuatTeks(diamGO.transform, "Label", diamLabel, 20, Color.white, FontStyles.Bold);
            dLab.alignment = TextAlignmentOptions.Center;
            dLab.raycastTarget = false;
            var dlrt = dLab.rectTransform;
            dlrt.anchorMin = Vector2.zero; dlrt.anchorMax = Vector2.one;
            dlrt.offsetMin = Vector2.zero; dlrt.offsetMax = Vector2.zero;
        }    }

    // ══════════════════════════════════════════════════════════════════════
    public void OnPointerDown(PointerEventData eventData) { if (!_selesai) _ditekan = true; }
    public void OnPointerUp(PointerEventData eventData)   { _ditekan = false; }

    void Update()
    {
        if (_selesai) return;

        // Voice-Driven Action: kalau mic aktif, BERTERIAK (loudness > ambang)
        // dianggap sama dengan menahan tombol. Tombol & keyboard (SPACE) tetap fallback.
        bool keyboardTahan = Input.GetKey(KeyCode.Space);
        bool teriak;
        if (_micAktif)
        {
            float raw = BacaLoudnessMic();
            _micLevel = Mathf.Lerp(_micLevel, raw, 1f - Mathf.Exp(-9f * Time.deltaTime));
            teriak = _micLevel >= ambangTeriak || _ditekan || keyboardTahan;
        }
        else
        {
            teriak = _ditekan || keyboardTahan;
        }

        if (teriak)
        {
            _holdProgress += Time.deltaTime;
            if (_tombolImg != null) _tombolImg.color = teriakWarnaDitekan;
            if (_tombolLabel != null) _tombolLabel.text = "\uD83D\uDD0A  TERIAAAK!!";
        }
        else
        {
            // Reset hold kalau lepas / suara mengecil
            if (_holdProgress > 0f) _holdProgress = Mathf.Max(0f, _holdProgress - Time.deltaTime * 2f);
            if (_tombolImg != null) _tombolImg.color = teriakWarna;
            if (_tombolLabel != null) _tombolLabel.text = _micAktif ? teriakLabelMic : teriakLabel;
        }

        // Update bar
        if (_barFill != null)
        {
            float pct = Mathf.Clamp01(_holdProgress / durasiTahan);
            float fullW = 540f - 4f;
            var fRT = _barFill.GetComponent<RectTransform>();
            fRT.sizeDelta = new Vector2(fullW * pct, 0f);
        }

        if (_holdProgress >= durasiTahan)
        {
            Selesaikan(true);
        }
    }

    IEnumerator TimerCoroutine()
    {
        _sisaWaktu = waktuWindow;
        while (_sisaWaktu > 0f && !_selesai)
        {
            _sisaWaktu -= Time.deltaTime;
            int s = Mathf.CeilToInt(_sisaWaktu);
            _timerText.text = $"\u23F1 {s} detik tersisa";
            _timerText.color = s <= 3 ? new Color(0.91f, 0.30f, 0.24f, 1f) : warnaBarFill;
            yield return null;
        }
        if (!_selesai) Selesaikan(false);
    }

    void Selesaikan(bool berhasil)
    {
        if (_selesai) return;
        _selesai = true;
        _berhasil = berhasil;
        StopAllCoroutines();

        var gs = GameState.Instance;
        if (berhasil)
        {
            if (gs != null)
            {
                // AddChoice sudah menambah skor (override = bonusBerhasil); jangan dobel.
                gs.AddChoice(2, "Teriak panggil Pak Supir saat pria mendekat di angkot", "AMAN", bonusBerhasil);
                if (!gs.achievements.Contains(achievementName))
                {
                    gs.achievements.Add(achievementName);
                    AchievementPopup.Show(achievementName);
                }
            }
            if (AudioManager.Instance != null && AudioManager.Instance.sfxLapor != null)
                AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.sfxLapor);
            else
                AudioManager.Instance?.PlayAchievement();
        }
        else
        {
            if (gs != null)
            {
                gs.AddChoice(2, "Tidak berani teriak saat pria mendekat di angkot", kategoriGagal, 0);
                if (kurangiNyawaSaatGagal)
                {
                    gs.LoseLife();
                    HUDManager.Instance?.UpdateHearts(gs.lives, gs.maxLives);
                }
            }
            if (AudioManager.Instance != null && AudioManager.Instance.sfxWrong != null)
                AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.sfxWrong);
        }

        BuildHasil();
    }

    void BuildHasil()
    {
        // Hapus tombol & bar, ganti dengan reaksi + lanjut
        var card = _canvasGO.transform.Find("Card");
        if (card != null)
        {
            var btnT = card.Find("TombolTeriak"); if (btnT != null) Destroy(btnT.gameObject);
            var bar  = card.Find("BarBG");        if (bar  != null) Destroy(bar.gameObject);
            var diam = card.Find("TombolDiam");   if (diam != null) Destroy(diam.gameObject);
        }
        if (_timerText != null) _timerText.text = "";

        var panel = new GameObject("HasilPanel");
        panel.transform.SetParent(_canvasGO.transform, false);
        var img = panel.AddComponent<Image>();
        img.sprite = GetRoundedSprite();
        img.color  = new Color(0.05f, 0.08f, 0.10f, 0.95f);
        img.type   = Image.Type.Sliced;
        var outl = panel.AddComponent<Outline>();
        outl.effectColor    = _berhasil ? new Color(0.45f, 1f, 0.65f, 1f) : new Color(1f, 0.55f, 0.55f, 1f);
        outl.effectDistance = new Vector2(3f, -3f);
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(900f, 380f);

        string rBerhasil = tampilkanNarasiPengejaran ? pengejaranReaksiBerhasil : reaksiBerhasil;
        string rGagal    = tampilkanNarasiPengejaran ? pengejaranReaksiGagal    : reaksiGagal;
        var teks = BuatTeks(panel.transform, "Teks",
            _berhasil ? rBerhasil + (bonusBerhasil > 0 ? $"\n\n<color=#FFD24A>+{bonusBerhasil} poin</color>" : "")
                       : rGagal,
            24, new Color(1f,1f,0.92f,1f), FontStyles.Normal);
        teks.alignment = TextAlignmentOptions.Center;
        var trt = teks.rectTransform;
        trt.anchorMin = new Vector2(0f, 0f); trt.anchorMax = new Vector2(1f, 1f);
        trt.offsetMin = new Vector2(40f, 100f);
        trt.offsetMax = new Vector2(-40f, -40f);

        // Tombol lanjut
        var btnGO = new GameObject("LanjutBtn");
        btnGO.transform.SetParent(panel.transform, false);
        var bImg = btnGO.AddComponent<Image>();
        bImg.sprite = GetRoundedSprite();
        bImg.color  = warnaLanjut;
        bImg.type   = Image.Type.Sliced;
        var bRT = btnGO.GetComponent<RectTransform>();
        bRT.anchorMin = new Vector2(0.5f, 0f); bRT.anchorMax = new Vector2(0.5f, 0f);
        bRT.pivot = new Vector2(0.5f, 0f);
        bRT.sizeDelta = new Vector2(380f, 60f);
        bRT.anchoredPosition = new Vector2(0f, 25f);

        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = bImg;
        btn.onClick.AddListener(() =>
        {
            AudioManager.Instance?.Click();
            if (_canvasGO != null) Destroy(_canvasGO);
            _onSelesai?.Invoke();
        });

        var lab = BuatTeks(btnGO.transform, "Label", tombolLanjutTeks, 22, Color.white, FontStyles.Bold);
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

// Helper: forward pointer events ke owner LaporTeriakButton
public class LaporTeriakInputProxy : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public LaporTeriakButton owner;
    public void OnPointerDown(PointerEventData eventData) { owner?.OnPointerDown(eventData); }
    public void OnPointerUp(PointerEventData eventData)   { owner?.OnPointerUp(eventData); }
}
