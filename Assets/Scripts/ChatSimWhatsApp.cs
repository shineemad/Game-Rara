using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// ChatSimWhatsApp — Simulasi chat WhatsApp orang asing yang minta foto/lokasi.
///
/// Alur:
///   1. Pesan masuk satu per satu (typing indicator opsional)
///   2. Setelah pesan terakhir, muncul 4 pilihan respons:
///      - SCREENSHOT (bonus +100)
///      - BLOKIR & HAPUS (AMAN, +SCORE_LAPOR)
///      - LAPOR KE KPAI (AMAN, +SCORE_LAPOR)
///      - BALAS (BAHAYA, -1 nyawa)
///   3. Timer 8s: kalau habis, otomatis dianggap "BALAS"
///   4. Reaksi pendek + tombol Lanjut.
///
/// Custom semua isi pesan & label tombol lewat Inspector.
/// </summary>
public class ChatSimWhatsApp : MonoBehaviour
{
    [System.Serializable]
    public class PesanData
    {
        [TextArea(1, 4)] public string teks = "Hai, kenalan dong?";
        [Tooltip("Delay sebelum pesan ini muncul (detik).")]
        public float delayDetik = 1.2f;
    }

    [System.Serializable]
    public class AksiData
    {
        public string label    = "BLOKIR & HAPUS";
        public string kategori = "AMAN"; // "AMAN" | "RAGU" | "BAHAYA"
        [TextArea(2, 4)]
        public string reaksi   = "\u2713 Bagus! Kontak orang asing diblokir.";
        public int    bonusPoin = 0;     // poin tambahan
        public bool   kurangiNyawa = false;
        [Tooltip("Pilihan fatal: langsung GAME OVER (nyawa habis). Mis. 'Iya Om, jemput'.")]
        public bool   akhiriGameOver = false;
        public Color  warna = new Color(0.18f, 0.62f, 0.32f, 1f);
    }

    [Header("Header WhatsApp")]
    public string namaKontak = "Nomor Tak Dikenal";
    public string statusKontak = "online";
    public Color  warnaHeader = new Color(0.05f, 0.32f, 0.27f, 1f);
    public Color  warnaTeksHeader = Color.white;

    [Header("Foto Profil Kontak (folder potrait)")]
    [Tooltip("Foto profil bulat (avatar) di header chat. Kosong = pakai warna default.\n" +
             "Drag sprite dari folder Assets/sprites/potrait ke field ini, sama seperti profil Paman.")]
    public Sprite fotoProfilKontak;
    [Tooltip("Jaga aspek rasio foto profil agar tidak gepeng.")]
    public bool   fotoProfilPreserveAspect = true;

    [Header("Jam Stempel Bubble")]
    [Tooltip("Jam yang ditampilkan di tiap bubble (format HH:mm). Kosong = pakai jam sistem. Hari 3 (pulang sekolah SMP) diisi mis. 13:45.")]
    public string jamTampil = "";

    [Header("Background Chat")]
    public Color warnaChatBg = new Color(0.05f, 0.12f, 0.12f, 1f);
    public Color warnaBubbleMasuk = new Color(0.20f, 0.30f, 0.35f, 1f);
    public Color warnaTeksBubble  = new Color(1f, 1f, 0.95f, 1f);

    [Header("Daftar Pesan Masuk (CUSTOMIZABLE)")]
    public PesanData[] pesanMasuk = new PesanData[]
    {
        new PesanData { teks = "Hai Rara \uD83D\uDE0A Ini om yang tadi pagi di halte, yang nanya kamu sekolah di mana. Masih ingat kan?", delayDetik = 0.8f },
        new PesanData { teks = "Om dapat nomor kamu dari temanmu, si Dina. Om bilang om kenal papamu, eh dia langsung kasih \uD83D\uDE04", delayDetik = 2.2f },
        new PesanData { teks = "Nah, sekarang kita bisa ngobrol diam-diam ya. Fotoin kamu pakai seragam dong, jangan bilang siapa-siapa \uD83E\uDD2B", delayDetik = 2.4f }
    };

    [Header("Notifikasi Masuk")]
    [Tooltip("Berapa kali bunyi notif WA berdering di awal chat (0 = tidak ada). Hari 3: 3x.")]
    public int notifBerderingKali = 0;
    [Tooltip("Jeda antar deringan notif (detik).")]
    public float jedaNotifDetik = 0.45f;

    [Header("Hari untuk Pencatatan Skor")]
    [Tooltip("Nomor hari yang dipakai saat mencatat pilihan ke GameState (Hari 2 = 2, Hari 3 = 3).")]
    public int hariUntukSkor = 2;

    [Header("Timer Pilihan")]
    public float waktuPilihDetik = 8f;
    public Color warnaTimer = new Color(1f, 0.85f, 0.3f, 1f);

    [Header("Tombol Screenshot (Bonus opsional)")]
    public bool   tampilkanTombolScreenshot = true;
    public string screenshotLabel = "\uD83D\uDCF7 Screenshot Bukti";
    public int    screenshotBonus = 100;
    public Color  screenshotWarna = new Color(0.20f, 0.62f, 0.86f, 1f);
    [Tooltip("Achievement saat screenshot bukti diambil (kosong = tidak ada).")]
    public string screenshotAchievement = "";

    [Header("Daftar Aksi Utama (CUSTOMIZABLE)")]
    public AksiData[] aksiList = new AksiData[]
    {
        new AksiData {
            label = "\uD83D\uDEAB BLOKIR & CERITA ke ORTU", kategori = "AMAN",
            reaksi = "\u2713 HEBAT, RA! Kamu blokir nomornya lalu CERITA ke ortu. Itu kata ajaib ke-3: CERITA. Jangan simpan rahasia dari orang dewasa yang kamu percaya!",
            bonusPoin = 500, // SCORE_LAPOR
            warna = new Color(0.91f, 0.30f, 0.24f, 1f)
        },
        new AksiData {
            label = "\u260E LAPOR KPAI 021-31901556", kategori = "AMAN",
            reaksi = "\u2713 Hebat! Kamu lapor ke KPAI bersama ortu. Mereka akan tindak lanjut.",
            bonusPoin = 500,
            warna = new Color(0.18f, 0.62f, 0.32f, 1f)
        },
        new AksiData {
            label = "\uD83D\uDCAC Balas: 'Iya Om'", kategori = "BAHAYA",
            reaksi = "\u2716 GAWAT! Orang itu makin pede dan minta lokasi rumahmu. Kamu kehilangan 1 nyawa. Orang asing di chat = sama bahayanya dengan di dunia nyata!",
            kurangiNyawa = true,
            warna = new Color(0.50f, 0.20f, 0.20f, 1f)
        },
        new AksiData {
            label = "\u2753 Diamkan / Abaikan", kategori = "RAGU",
            reaksi = "\u26A0 Kamu diamkan. Tapi besok dia kirim chat lagi. Lebih baik BLOKIR lalu CERITA ke ortu \u2014 jangan dipendam sendiri.",
            warna = new Color(0.95f, 0.62f, 0.07f, 1f)
        }
    };

    [Header("Aksi Default Saat Timeout")]
    [Tooltip("Index aksi yang dipakai kalau waktu habis (biasanya BAHAYA).")]
    public int aksiSaatTimeout = 2;

    [Header("Tombol Lanjut")]
    public string tombolLanjutTeks = "\u25B6  Lanjut";
    public Color  warnaLanjut = new Color(0.20f, 0.62f, 0.86f, 1f);

    [Header("BG Fullscreen Device (opsional)")]
    [Tooltip("Sprite latar FULLSCREEN device (stretch ke seluruh layar). Tampil paling belakang, di belakang frame HP.\n" +
             "Misal: foto kamar Rara dengan HP di tangan.")]
    public Sprite bgFullscreenSprite;
    [Tooltip("Jaga aspek rasio sprite saat di-stretch fullscreen (mencegah gepeng).")]
    public bool   bgFullscreenPreserveAspect = false;

    [Header("Font")]
    public TMP_FontAsset fontAsset;

    [Header("Sorting")]
    public int sortingOrder = 935;

    // ── runtime ───────────────────────────────────────────────────────────
    private Action     _onSelesai;
    private GameObject _canvasGO;
    private GameObject _phoneFrame;
    private RectTransform _chatScroll;
    private TextMeshProUGUI _timerText;
    private GameObject _tombolPanel;
    private GameObject _reaksiPanel;
    private RectTransform _timerFillRt;
    private bool       _aksiDipilih;
    private bool       _screenshotDiambil;
    private float      _sisaWaktu;
    private Sprite     _roundedSprite;

    // ══════════════════════════════════════════════════════════════════════
    public void Mulai(Action onSelesai)
    {
        _onSelesai = onSelesai;
        // Sembunyikan navbar HUD agar layar chat tampil penuh tanpa tertimpa navbar.
        HUDManager.Instance?.SetNavbarVisible(false);
        BuildScene();
        StartCoroutine(JalankanChat());
    }

    // ══════════════════════════════════════════════════════════════════════
    void BuildScene()
    {
        _canvasGO = new GameObject("ChatSim_Canvas");
        var canvas = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = _canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        _canvasGO.AddComponent<GraphicRaycaster>();

        // ── BACKDROP FULLSCREEN ─────────────────────────────────────────────
        // Menutup TOTAL latar Day 1 di belakang frame HP + memblokir input.
        // Pakai sprite kustom bila di-assign, kalau tidak: deep-navy opaque.
        var backdrop = new GameObject("Backdrop");
        backdrop.transform.SetParent(_canvasGO.transform, false);
        var bdImg = backdrop.AddComponent<Image>();
        if (bgFullscreenSprite != null)
        {
            bdImg.sprite         = bgFullscreenSprite;
            bdImg.preserveAspect = bgFullscreenPreserveAspect;
            bdImg.color          = Color.white;
        }
        else
        {
            bdImg.color = new Color(0.035f, 0.055f, 0.10f, 1f); // deep navy, opaque
        }
        bdImg.raycastTarget = true; // blokir input ke Day 1 di belakang
        var bdRT = backdrop.GetComponent<RectTransform>();
        bdRT.anchorMin = Vector2.zero; bdRT.anchorMax = Vector2.one;
        bdRT.offsetMin = Vector2.zero; bdRT.offsetMax = Vector2.zero;

        // Glow lembut di belakang frame HP untuk kedalaman.
        var glow = new GameObject("GlowTengah");
        glow.transform.SetParent(_canvasGO.transform, false);
        var glowRT = glow.AddComponent<RectTransform>();
        glowRT.anchorMin = new Vector2(0.5f, 0.5f);
        glowRT.anchorMax = new Vector2(0.5f, 0.5f);
        glowRT.pivot     = new Vector2(0.5f, 0.5f);
        glowRT.sizeDelta = new Vector2(900f, 1180f);
        glowRT.anchoredPosition = Vector2.zero;
        var glowImg = glow.AddComponent<Image>();
        glowImg.sprite        = GetRoundedSprite();
        glowImg.type          = Image.Type.Sliced;
        glowImg.color         = new Color(0.12f, 0.40f, 0.34f, 0.20f); // hijau-teal lembut (tema WA)
        glowImg.raycastTarget = false;

        // Frame HP (di tengah layar)
        _phoneFrame = new GameObject("PhoneFrame");
        _phoneFrame.transform.SetParent(_canvasGO.transform, false);
        var pImg = _phoneFrame.AddComponent<Image>();
        pImg.sprite = GetRoundedSprite();
        pImg.color  = new Color(0.02f, 0.02f, 0.02f, 1f);
        pImg.type   = Image.Type.Sliced;
        var pOutl = _phoneFrame.AddComponent<Outline>();
        pOutl.effectColor    = new Color(0.4f, 0.4f, 0.4f, 1f);
        pOutl.effectDistance = new Vector2(3f, -3f);
        var pRT = _phoneFrame.GetComponent<RectTransform>();
        pRT.anchorMin = new Vector2(0.5f, 0.5f); pRT.anchorMax = new Vector2(0.5f, 0.5f);
        pRT.pivot = new Vector2(0.5f, 0.5f);
        // Frame HP diperbesar mendekati fullscreen vertikal agar teks chat mudah dibaca di mobile.
        pRT.sizeDelta = new Vector2(880f, 1050f);

        // Header WhatsApp
        var header = new GameObject("Header");
        header.transform.SetParent(_phoneFrame.transform, false);
        var hImg = header.AddComponent<Image>();
        hImg.sprite = GetRoundedSprite();
        hImg.color  = warnaHeader;
        hImg.type   = Image.Type.Sliced;
        var hRT = header.GetComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0f, 1f); hRT.anchorMax = new Vector2(1f, 1f);
        hRT.pivot = new Vector2(0.5f, 1f);
        hRT.sizeDelta = new Vector2(0f, 104f);
        hRT.offsetMin = new Vector2(8f, hRT.offsetMin.y);
        hRT.offsetMax = new Vector2(-8f, -8f);

        var nama = BuatTeks(header.transform, "Nama", namaKontak, 30, warnaTeksHeader, FontStyles.Bold);
        nama.alignment = TextAlignmentOptions.MidlineLeft;
        var nrt = nama.rectTransform;
        nrt.anchorMin = new Vector2(0f, 0.4f); nrt.anchorMax = new Vector2(1f, 1f);
        nrt.offsetMin = new Vector2(80f, 0f); nrt.offsetMax = new Vector2(-12f, -8f);

        var status = BuatTeks(header.transform, "Status", statusKontak, 18, new Color(1f,1f,1f,0.75f), FontStyles.Italic);
        status.alignment = TextAlignmentOptions.MidlineLeft;
        var stt = status.rectTransform;
        stt.anchorMin = new Vector2(0f, 0f); stt.anchorMax = new Vector2(1f, 0.4f);
        stt.offsetMin = new Vector2(80f, 4f); stt.offsetMax = new Vector2(-12f, 0f);

        // Avatar bulat
        var avatar = new GameObject("Avatar");
        avatar.transform.SetParent(header.transform, false);
        var aImg = avatar.AddComponent<Image>();
        if (fotoProfilKontak != null)
        {
            // Foto profil kontak di-assign (seperti profil Paman) — tampilkan sprite-nya.
            aImg.sprite         = fotoProfilKontak;
            aImg.color          = Color.white;
            aImg.preserveAspect = fotoProfilPreserveAspect;
        }
        else
        {
            // Belum ada foto profil — pakai lingkaran warna default.
            aImg.sprite = GetRoundedSprite();
            aImg.color  = new Color(0.45f, 0.25f, 0.30f, 1f);
            aImg.type   = Image.Type.Sliced;
        }
        var aRT = avatar.GetComponent<RectTransform>();
        aRT.anchorMin = new Vector2(0f, 0.5f); aRT.anchorMax = new Vector2(0f, 0.5f);
        aRT.pivot = new Vector2(0f, 0.5f);
        aRT.sizeDelta = new Vector2(60f, 60f);
        aRT.anchoredPosition = new Vector2(12f, 0f);

        // Chat area
        var chatBg = new GameObject("ChatBg");
        chatBg.transform.SetParent(_phoneFrame.transform, false);
        var cbImg = chatBg.AddComponent<Image>();
        cbImg.color = warnaChatBg;
        var cbRT = chatBg.GetComponent<RectTransform>();
        cbRT.anchorMin = new Vector2(0f, 0f); cbRT.anchorMax = new Vector2(1f, 1f);
        cbRT.offsetMin = new Vector2(8f, 372f);
        cbRT.offsetMax = new Vector2(-8f, -114f);

        // Scroll container utk pesan
        var scroll = new GameObject("ChatScroll");
        scroll.transform.SetParent(chatBg.transform, false);
        _chatScroll = scroll.AddComponent<RectTransform>();
        _chatScroll.anchorMin = new Vector2(0f, 0f); _chatScroll.anchorMax = new Vector2(1f, 1f);
        _chatScroll.offsetMin = new Vector2(12f, 12f);
        _chatScroll.offsetMax = new Vector2(-12f, -12f);
        var vlg = scroll.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.LowerLeft;
        vlg.spacing = 14f;
        vlg.padding = new RectOffset(6, 6, 6, 10);
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;
    }

    // ══════════════════════════════════════════════════════════════════════
    IEnumerator JalankanChat()
    {
        // Notif WA berdering di awal (mis. Hari 3: 3x).
        for (int i = 0; i < notifBerderingKali; i++)
        {
            AudioManager.Instance?.PlayChatMasuk();
            yield return new WaitForSeconds(jedaNotifDetik);
        }

        foreach (var p in pesanMasuk)
        {
            yield return TampilkanTyping();
            yield return new WaitForSeconds(p.delayDetik);
            TambahBubble(p.teks);
            yield return new WaitForSeconds(0.4f);
        }

        // Tampilkan tombol aksi + timer
        BuildTombolAksi();
        yield return TimerCoroutine();
    }

    IEnumerator TampilkanTyping()
    {
        // Bubble kecil khusus indikator "sedang mengetik" (tidak lebar penuh, tanpa jam).
        var row = new GameObject("TypingRow");
        row.transform.SetParent(_chatScroll, false);
        var rowLE = row.AddComponent<LayoutElement>();
        rowLE.preferredWidth = 96f;
        rowLE.flexibleWidth  = 0f;
        var rowVlg = row.AddComponent<HorizontalLayoutGroup>();
        rowVlg.childAlignment      = TextAnchor.MiddleLeft;
        rowVlg.childControlWidth   = true;
        rowVlg.childControlHeight  = true;
        rowVlg.childForceExpandWidth  = true;
        rowVlg.childForceExpandHeight = false;

        var bubble = new GameObject("TypingBubble");
        bubble.transform.SetParent(row.transform, false);
        var img = bubble.AddComponent<Image>();
        img.sprite = GetRoundedSprite();
        img.color  = warnaBubbleMasuk;
        img.type   = Image.Type.Sliced;
        var bVlg = bubble.AddComponent<VerticalLayoutGroup>();
        bVlg.padding = new RectOffset(18, 18, 12, 14);
        bVlg.childControlWidth = true; bVlg.childControlHeight = true;
        bVlg.childForceExpandWidth = true; bVlg.childForceExpandHeight = false;

        var tmp = BuatTeks(bubble.transform, "Dots", "\u2022 \u2022 \u2022", 28,
                           new Color(1f, 1f, 1f, 0.6f), FontStyles.Bold);
        tmp.alignment = TextAlignmentOptions.Center;

        float t = 0f;
        while (t < 0.6f)
        {
            t += Time.deltaTime;
            int n = (Mathf.FloorToInt(t * 4f) % 3) + 1;
            tmp.text = string.Join(" ", System.Linq.Enumerable.Repeat("\u2022", n));
            yield return null;
        }
        Destroy(row);
    }

    GameObject TambahBubble(string teks)
    {
        // Pembungkus baris (kiri) supaya bubble tidak melar selebar area chat.
        var row = new GameObject("Row");
        row.transform.SetParent(_chatScroll, false);
        var rowLE = row.AddComponent<LayoutElement>();
        rowLE.preferredWidth = 640f;   // lebar maksimum bubble masuk
        rowLE.flexibleWidth  = 0f;
        var rowVlg = row.AddComponent<VerticalLayoutGroup>();
        rowVlg.childAlignment      = TextAnchor.UpperLeft;
        rowVlg.childControlWidth   = true;
        rowVlg.childControlHeight  = true;
        rowVlg.childForceExpandWidth  = true;
        rowVlg.childForceExpandHeight = false;

        // Bubble (kartu) — tinggi otomatis dari isi teks via VerticalLayoutGroup.
        var go = new GameObject("Bubble");
        go.transform.SetParent(row.transform, false);
        var img = go.AddComponent<Image>();
        img.sprite = GetRoundedSprite();
        img.color  = warnaBubbleMasuk;
        img.type   = Image.Type.Sliced;

        // Bayangan halus utk kedalaman.
        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor    = new Color(0f, 0f, 0f, 0.28f);
        shadow.effectDistance = new Vector2(2f, -3f);

        var bubbleVlg = go.AddComponent<VerticalLayoutGroup>();
        bubbleVlg.padding = new RectOffset(18, 18, 12, 12);
        bubbleVlg.spacing = 2f;
        bubbleVlg.childAlignment      = TextAnchor.UpperLeft;
        bubbleVlg.childControlWidth   = true;
        bubbleVlg.childControlHeight  = true;
        bubbleVlg.childForceExpandWidth  = true;
        bubbleVlg.childForceExpandHeight = false;

        // Teks pesan — word-wrap, tinggi mengikuti panjang teks.
        var t = BuatTeks(go.transform, "Text", teks, 28, warnaTeksBubble, FontStyles.Normal);
        t.alignment          = TextAlignmentOptions.TopLeft;
        t.textWrappingMode   = TextWrappingModes.Normal;
        t.lineSpacing        = 6f;

        // Jam kecil ala WhatsApp di pojok kanan bawah bubble.
        var jam = BuatTeks(go.transform, "Jam", WaktuSekarang(), 16,
                           new Color(1f, 1f, 1f, 0.45f), FontStyles.Normal);
        jam.alignment = TextAlignmentOptions.BottomRight;

        // Animasi masuk: fade + skala kecil -> normal supaya terasa hidup.
        var cg = row.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        StartCoroutine(AnimasiMasukBubble(go.transform, cg));

        return go;
    }

    /// <summary>Animasi gelembung chat: fade-in + skala 0.85 -> 1 (ease-out).</summary>
    IEnumerator AnimasiMasukBubble(Transform target, CanvasGroup cg)
    {
        if (target == null) yield break;
        float durasi = 0.22f, t = 0f;
        AudioManager.Instance?.PlayChatMasuk();
        while (t < durasi && target != null)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / durasi);
            float ease = 1f - (1f - p) * (1f - p); // ease-out quad
            if (cg != null) cg.alpha = ease;
            target.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, ease);
            yield return null;
        }
        if (cg != null) cg.alpha = 1f;
        if (target != null) target.localScale = Vector3.one;
    }

    // Jam HH:mm utk stempel waktu bubble. Kalau jamTampil diisi, pakai itu (mis. jam pulang SMP);
    // kalau kosong, pakai jam sistem.
    string WaktuSekarang() =>
        string.IsNullOrEmpty(jamTampil) ? System.DateTime.Now.ToString("HH:mm") : jamTampil;

    /// <summary>Pasang efek hover (membesar saat kursor masuk, kembali saat keluar).</summary>
    void PasangHover(GameObject go, float skala)
    {
        if (go == null) return;
        var trig = go.GetComponent<EventTrigger>();
        if (trig == null) trig = go.AddComponent<EventTrigger>();
        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ => { if (go != null) go.transform.localScale = Vector3.one * skala; });
        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => { if (go != null) go.transform.localScale = Vector3.one; });
        trig.triggers.Add(enter);
        trig.triggers.Add(exit);
    }

    /// <summary>Animasi pop: skala 0.8 -> 1.06 -> 1 (overshoot) untuk panel reaksi.</summary>
    IEnumerator AnimasiPop(RectTransform rt)
    {
        if (rt == null) yield break;
        float durasi = 0.26f, t = 0f;
        while (t < durasi && rt != null)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / durasi);
            // Overshoot sederhana.
            float s = p < 0.7f ? Mathf.Lerp(0.8f, 1.06f, p / 0.7f)
                               : Mathf.Lerp(1.06f, 1f, (p - 0.7f) / 0.3f);
            rt.localScale = Vector3.one * s;
            yield return null;
        }
        if (rt != null) rt.localScale = Vector3.one;
    }

    // ══════════════════════════════════════════════════════════════════════
    void BuildTombolAksi()
    {
        // Bilah aksi di BAGIAN BAWAH dalam frame HP (seperti area input WhatsApp).
        _tombolPanel = new GameObject("ActionBar");
        _tombolPanel.transform.SetParent(_phoneFrame.transform, false);
        var barImg = _tombolPanel.AddComponent<Image>();
        barImg.sprite = GetRoundedSprite();
        barImg.color  = new Color(0.06f, 0.09f, 0.10f, 1f);
        barImg.type   = Image.Type.Sliced;
        var barOutl = _tombolPanel.AddComponent<Outline>();
        barOutl.effectColor    = new Color(1f, 1f, 1f, 0.10f);
        barOutl.effectDistance = new Vector2(1f, -1f);
        var tpRT = _tombolPanel.GetComponent<RectTransform>();
        tpRT.anchorMin = new Vector2(0f, 0f); tpRT.anchorMax = new Vector2(1f, 0f);
        tpRT.pivot = new Vector2(0.5f, 0f);
        tpRT.sizeDelta = new Vector2(-16f, 348f);
        tpRT.anchoredPosition = new Vector2(0f, 8f);

        // Timer di bagian atas bilah aksi
        _timerText = BuatTeks(_tombolPanel.transform, "Timer", "", 30, warnaTimer, FontStyles.Bold);
        _timerText.alignment = TextAlignmentOptions.Center;
        var trt = _timerText.rectTransform;
        trt.anchorMin = new Vector2(0f, 1f); trt.anchorMax = new Vector2(1f, 1f);
        trt.pivot = new Vector2(0.5f, 1f);
        trt.offsetMin = new Vector2(12f, -44f);
        trt.offsetMax = new Vector2(-12f, -10f);

        // Bar waktu visual (menyusut) tepat di bawah teks timer.
        var barBgGO = new GameObject("TimerBar");
        barBgGO.transform.SetParent(_tombolPanel.transform, false);
        var barBgImg = barBgGO.AddComponent<Image>();
        barBgImg.sprite = GetRoundedSprite();
        barBgImg.type   = Image.Type.Sliced;
        barBgImg.color  = new Color(1f, 1f, 1f, 0.12f);
        barBgImg.raycastTarget = false;
        var barBgRT = barBgGO.GetComponent<RectTransform>();
        barBgRT.anchorMin = new Vector2(0f, 1f); barBgRT.anchorMax = new Vector2(1f, 1f);
        barBgRT.pivot = new Vector2(0.5f, 1f);
        barBgRT.offsetMin = new Vector2(16f, -58f);
        barBgRT.offsetMax = new Vector2(-16f, -48f);

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(barBgGO.transform, false);
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.sprite = GetRoundedSprite();
        fillImg.type   = Image.Type.Sliced;
        fillImg.color  = warnaTimer;
        fillImg.raycastTarget = false;
        _timerFillRt = fillGO.GetComponent<RectTransform>();
        _timerFillRt.anchorMin = new Vector2(0f, 0f); _timerFillRt.anchorMax = new Vector2(1f, 1f);
        _timerFillRt.offsetMin = Vector2.zero; _timerFillRt.offsetMax = Vector2.zero;

        // Grid tombol di bawah timer
        var gridGO = new GameObject("Grid");
        gridGO.transform.SetParent(_tombolPanel.transform, false);
        var gRT = gridGO.AddComponent<RectTransform>();
        gRT.anchorMin = new Vector2(0f, 0f); gRT.anchorMax = new Vector2(1f, 1f);
        gRT.offsetMin = new Vector2(14f, 14f);
        gRT.offsetMax = new Vector2(-14f, -64f);

        var grid = gridGO.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(410f, 84f);
        grid.spacing = new Vector2(14f, 12f);
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;

        // Tombol screenshot (kalau diaktifkan)
        if (tampilkanTombolScreenshot)
        {
            var ssGO = BuatTombol(gridGO.transform, screenshotLabel + $" (+{screenshotBonus})", screenshotWarna, null);
            var ssBtn = ssGO.GetComponent<Button>();
            ssBtn.onClick.AddListener(() =>
            {
                if (_screenshotDiambil) return;
                _screenshotDiambil = true;
                AudioManager.Instance?.Click();
                var gs = GameState.Instance;
                if (gs != null)
                {
                    gs.score += screenshotBonus; gs.screenshotTaken = true;
                    // B2 — catat bukti screenshot sesuai hari (2 = chat halte/angkot, 3 = chat ojol).
                    gs.TambahBukti(hariUntukSkor == 3 ? GameState.BUKTI_CHAT_DAY3 : GameState.BUKTI_CHAT_DAY2);
                }
                if (!string.IsNullOrEmpty(screenshotAchievement))
                    GameState.Instance?.EarnAchievement(screenshotAchievement);
                Debug.Log($"[ChatSim] Screenshot diambil. +{screenshotBonus} poin.");
                var label = ssGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
                if (label != null) label.text = "\u2713 Screenshot tersimpan";
            });
        }

        foreach (var a in aksiList)
        {
            var aksiRef = a; // capture
            BuatTombol(gridGO.transform, a.label, a.warna, () => PilihAksi(aksiRef));
        }
    }

    GameObject BuatTombol(Transform parent, string teks, Color warna, Action onClick)
    {
        var go = new GameObject(teks);
        go.transform.SetParent(parent, false);
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
        if (onClick != null) btn.onClick.AddListener(() => onClick.Invoke());

        // Efek hover: tombol membesar saat kursor masuk (terasa interaktif).
        PasangHover(go, 1.05f);

        var t = BuatTeks(go.transform, "Label", teks, 24, Color.white, FontStyles.Bold);
        t.alignment = TextAlignmentOptions.Center;
        t.enableAutoSizing = true;
        t.fontSizeMin = 18f; t.fontSizeMax = 24f;
        var trt = t.rectTransform;
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(8f, 4f);
        trt.offsetMax = new Vector2(-8f, -4f);

        return go;
    }

    IEnumerator TimerCoroutine()
    {
        _sisaWaktu = waktuPilihDetik;
        while (_sisaWaktu > 0f && !_aksiDipilih)
        {
            _sisaWaktu -= Time.deltaTime;
            int s = Mathf.CeilToInt(_sisaWaktu);
            _timerText.text = $"\u23F1 {s} detik untuk memutuskan!";
            bool kritis = s <= 3;
            var warnaKini = kritis ? new Color(0.91f, 0.30f, 0.24f, 1f) : warnaTimer;
            _timerText.color = warnaKini;

            // Bar menyusut mengikuti sisa waktu + ikut berubah merah saat kritis.
            if (_timerFillRt != null)
            {
                float frac = waktuPilihDetik > 0f ? Mathf.Clamp01(_sisaWaktu / waktuPilihDetik) : 0f;
                _timerFillRt.anchorMax = new Vector2(frac, 1f);
                _timerFillRt.offsetMin = Vector2.zero; _timerFillRt.offsetMax = Vector2.zero;
                var fillImg = _timerFillRt.GetComponent<Image>();
                if (fillImg != null)
                {
                    // Saat kritis, bar berdenyut halus.
                    float pulse = kritis ? 0.7f + 0.3f * Mathf.Abs(Mathf.Sin(Time.time * 6f)) : 1f;
                    fillImg.color = new Color(warnaKini.r, warnaKini.g, warnaKini.b, pulse);
                }
            }
            yield return null;
        }
        if (!_aksiDipilih && aksiSaatTimeout >= 0 && aksiSaatTimeout < aksiList.Length)
        {
            Debug.Log("[ChatSim] Waktu habis \u2192 default aksi index " + aksiSaatTimeout);
            PilihAksi(aksiList[aksiSaatTimeout]);
        }
    }

    void PilihAksi(AksiData a)
    {
        if (_aksiDipilih) return;
        _aksiDipilih = true;
        AudioManager.Instance?.Click();

        var gs = GameState.Instance;
        if (gs != null)
        {
            gs.AddChoice(hariUntukSkor, "Chat: " + a.label, a.kategori);
            if (a.bonusPoin > 0)
            {
                gs.score += a.bonusPoin;
                Debug.Log($"[ChatSim] Bonus +{a.bonusPoin}");
            }
            if (a.akhiriGameOver)
            {
                // Pilihan fatal → nyawa langsung habis (GAME OVER).
                gs.lives = 0;
                Debug.Log("[ChatSim] Pilihan fatal → GAME OVER.");
            }
            else if (a.kurangiNyawa)
            {
                gs.lives = Mathf.Max(0, gs.lives - 1);
                Debug.Log($"[ChatSim] Nyawa -1 (sisa {gs.lives})");
            }
        }

        AudioClip sfx = a.kategori switch
        {
            "AMAN"   => AudioManager.Instance?.sfxAman,
            "RAGU"   => AudioManager.Instance?.sfxRagu,
            "BAHAYA" => AudioManager.Instance?.sfxBahaya,
            _        => null
        };
        if (sfx != null) AudioManager.Instance.sfxSource.PlayOneShot(sfx);

        // Hapus tombol & timer, tampilkan reaksi
        if (_tombolPanel != null) Destroy(_tombolPanel);
        if (_timerText != null) _timerText.text = "";

        BuildReaksi(a);
    }

    void BuildReaksi(AksiData a)
    {
        _reaksiPanel = new GameObject("ReaksiPanel");
        _reaksiPanel.transform.SetParent(_canvasGO.transform, false);
        var img = _reaksiPanel.AddComponent<Image>();
        img.sprite = GetRoundedSprite();
        img.color  = new Color(0.08f, 0.10f, 0.14f, 0.97f);
        img.type   = Image.Type.Sliced;
        var outl = _reaksiPanel.AddComponent<Outline>();
        outl.effectColor    = a.warna;
        outl.effectDistance = new Vector2(2f, -2f);
        var rt = _reaksiPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f); rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(1120f, 280f);
        rt.anchoredPosition = new Vector2(0f, 30f);

        // Animasi pop saat panel reaksi muncul.
        StartCoroutine(AnimasiPop(rt));

        var teks = BuatTeks(_reaksiPanel.transform, "Reaksi", a.reaksi, 28, new Color(1f,1f,0.92f,1f), FontStyles.Normal);
        teks.alignment = TextAlignmentOptions.Center;
        var trt = teks.rectTransform;
        trt.anchorMin = new Vector2(0f, 0f); trt.anchorMax = new Vector2(1f, 1f);
        trt.offsetMin = new Vector2(30f, 80f);
        trt.offsetMax = new Vector2(-30f, -25f);

        // Tombol lanjut
        var btnGO = new GameObject("LanjutBtn");
        btnGO.transform.SetParent(_reaksiPanel.transform, false);
        var bImg = btnGO.AddComponent<Image>();
        bImg.sprite = GetRoundedSprite();
        bImg.color  = warnaLanjut;
        bImg.type   = Image.Type.Sliced;
        var bRT = btnGO.GetComponent<RectTransform>();
        bRT.anchorMin = new Vector2(0.5f, 0f); bRT.anchorMax = new Vector2(0.5f, 0f);
        bRT.pivot = new Vector2(0.5f, 0f);
        bRT.sizeDelta = new Vector2(360f, 88f);
        bRT.anchoredPosition = new Vector2(0f, 18f);

        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = bImg;
        btn.onClick.AddListener(() =>
        {
            AudioManager.Instance?.Click();
            // Tampilkan kembali navbar HUD sebelum keluar dari layar chat.
            HUDManager.Instance?.SetNavbarVisible(true);
            if (_canvasGO != null) Destroy(_canvasGO);
            _onSelesai?.Invoke();
        });
        PasangHover(btnGO, 1.06f);

        var lab = BuatTeks(btnGO.transform, "Label", tombolLanjutTeks, 26, Color.white, FontStyles.Bold);
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
