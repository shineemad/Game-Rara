using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SampaiSekolahDay1 — Popup "Rara Sampai di Sekolah" + Kartu Edukasi Hari 1 yang
/// BERCABANG mengikuti pilihan jalur (GameState.pathChoice):
///   • "safe"      → Rara lewat JALAN RAMAI (aman)   → popup ceria + tips kebiasaan baik
///   • "dangerous" → Rara lewat GANG SEPI (berisiko) → popup waspada + tips pelajaran
///
/// Menggantikan SampaiSekolahPopup + EduCardDay1 lama yang single-content.
/// Alur: trigger X → Popup Sampai (bercabang) → [Lanjut] → Kartu Edukasi (bercabang)
///       → [Lanjutkan] → Day1SummaryScreen (auto-find) / DayTransitionManager.
///
/// Self-contained: UI dibangun procedural, tak perlu wiring tambahan.
/// Cara pakai:
///   1. GameObject → Create Empty → "SampaiSekolahDay1"
///   2. Add Component → SampaiSekolahDay1
///   3. Drag Player (atau biarkan auto-find via tag 'Player'); atur triggerX (mis. 60).
/// </summary>
public class SampaiSekolahDay1 : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR
    // ══════════════════════════════════════════════════════════════════════

    [Header("Referensi")]
    [Tooltip("Player Rara — jika kosong, dicari otomatis via tag 'Player'.")]
    public Transform player;

    [Header("Trigger")]
    [Tooltip("X world position. Saat Rara x ≥ nilai ini → popup muncul.")]
    public float triggerX = 60f;
    [Tooltip("Hanya dipicu sekali per sesi.")]
    public bool  triggerOnce = true;
    [Tooltip("Bekukan pergerakan player saat popup/kartu tampil.")]
    public bool  freezePlayerSaatTampil = true;
    [Tooltip("Jeda (detik) sebelum popup tampil setelah trigger tercapai.")]
    public float jedaSebelumTampil = 0.4f;

    [Header("Bonus Poin Tiba Selamat (jalur AMAN saja)")]
    [Tooltip("Tambahkan bonus poin ke GameState saat Rara tiba lewat JALAN RAMAI.")]
    public bool  tambahBonusJalurAman = true;
    public int   bonusJalurAman       = 100;

    [Header("Font (opsional)")]
    public TMP_FontAsset fontAsset;

    [Header("Sorting")]
    [Tooltip("Sorting order Canvas. Default 1010 — di atas dialog (999).")]
    public int sortingOrder = 1010;

    [Header("Lanjutan")]
    [Tooltip("Layar ringkasan Hari 1 berikutnya. Kosong = auto-find di scene.")]
    public Day1SummaryScreen layarRingkasanSelanjutnya;
    public bool autoCariLayarRingkasan = true;

    // ── runtime ───────────────────────────────────────────────────────────
    private bool       _triggered;
    private bool       _aktif;
    private GameObject _canvasGO;
    private Sprite     _roundedRectSprite;

    // ══════════════════════════════════════════════════════════════════════
    void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (_triggered && triggerOnce) return;
        if (_aktif || player == null) return;

        if (player.position.x >= triggerX)
        {
            _triggered = true;
            StartCoroutine(JalankanAlur());
        }
    }

    /// TRUE jika Rara memilih GANG SEPI (jalur berbahaya) di PathChoice.
    bool AmbilGangSepi =>
        GameState.Instance != null && GameState.Instance.pathChoice == "dangerous";

    // ══════════════════════════════════════════════════════════════════════
    // ALUR UTAMA
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator JalankanAlur()
    {
        _aktif = true;
        FreezePlayer(true);

        if (jedaSebelumTampil > 0f) yield return new WaitForSeconds(jedaSebelumTampil);

        // Bonus poin hanya untuk jalur AMAN (jalur BAHAYA sudah dipotong nyawa saat memilih).
        if (!AmbilGangSepi && tambahBonusJalurAman && bonusJalurAman != 0 && GameState.Instance != null)
            GameState.Instance.score += bonusJalurAman;

        AudioManager.Instance?.PlayAchievement();

        // 1) Popup "Sampai Sekolah" (bercabang).
        bool lanjut1 = false;
        BuildPopupSampai(() => lanjut1 = true);
        while (!lanjut1) yield return null;

        // 2) Kartu Edukasi (bercabang).
        bool lanjut2 = false;
        BuildEduCard(() => lanjut2 = true);
        while (!lanjut2) yield return null;

        // 3) Lanjut ke ringkasan / Hari 2.
        FreezePlayer(false);
        LanjutKeRingkasan();
    }

    void LanjutKeRingkasan()
    {
        Day1SummaryScreen layar = layarRingkasanSelanjutnya;
        if (layar == null && autoCariLayarRingkasan)
            layar = FindFirstObjectByType<Day1SummaryScreen>(FindObjectsInactive.Include);

        if (layar != null)
        {
            layar.Tampilkan();
        }
        else if (DayTransitionManager.Instance != null)
        {
            DayTransitionManager.Instance.LanjutKeDay2();
        }
        else
        {
            Debug.LogWarning("[SampaiSekolahDay1] Day1SummaryScreen & DayTransitionManager tak ditemukan.");
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // 1) POPUP "SAMPAI SEKOLAH" — bercabang per jalur
    // ══════════════════════════════════════════════════════════════════════
    void BuildPopupSampai(Action onLanjut)
    {
        bool gangSepi = AmbilGangSepi;

        // Tema warna sesuai jalur.
        Color warnaJudul = gangSepi ? new Color(0.95f, 0.45f, 0.30f, 1f)   // oranye waspada
                                    : new Color(0.30f, 0.78f, 0.42f, 1f);  // hijau ceria
        string judul = gangSepi
            ? "Fyuh\u2026 Rara Akhirnya Sampai"
            : "Yeay! Rara Sampai dengan Selamat!";
        string subtitel = gangSepi
            ? "Rara nekat lewat <b>GANG SEPI</b> tadi \u2014 sempat deg-degan dan kehilangan 1 nyawa. Untung berhasil tiba di SMP Harapan!"
            : "Rara memilih <b>JALAN RAMAI</b> dan tiba di SMP Harapan dengan aman. Keputusan yang tepat! \u2705";

        var go = BuatCanvas("PopupSampaiCanvas");

        // Overlay gelap.
        var overlay = BuatImage(go.transform, "Overlay", new Color(0f, 0f, 0f, 0.80f));
        Stretch(overlay.rectTransform);
        overlay.raycastTarget = true;

        // Kartu utama.
        var card = BuatImage(go.transform, "KartuSampai", new Color(0.12f, 0.10f, 0.08f, 0.98f));
        card.sprite = GetRoundedRect(); card.type = Image.Type.Sliced;
        var cardOutline = card.gameObject.AddComponent<Outline>();
        cardOutline.effectColor    = gangSepi ? new Color(0.95f, 0.55f, 0.25f, 0.95f)
                                              : new Color(0.40f, 0.95f, 0.55f, 0.95f);
        cardOutline.effectDistance = new Vector2(3f, -3f);
        var cRt = card.rectTransform;
        cRt.anchorMin = new Vector2(0.5f, 0.5f); cRt.anchorMax = new Vector2(0.5f, 0.5f);
        cRt.pivot = new Vector2(0.5f, 0.5f);
        cRt.sizeDelta = new Vector2(900f, 520f);

        // Judul.
        var jdl = BuatTeks(card.transform, "Judul", judul, 36, warnaJudul, FontStyles.Bold);
        jdl.alignment = TextAlignmentOptions.Center;
        var jRt = jdl.rectTransform;
        jRt.anchorMin = new Vector2(0f, 1f); jRt.anchorMax = new Vector2(1f, 1f);
        jRt.pivot = new Vector2(0.5f, 1f); jRt.sizeDelta = new Vector2(-80f, 110f);
        jRt.anchoredPosition = new Vector2(0f, -40f);

        // Highlight tengah: poin (jalur aman) atau peringatan (jalur bahaya).
        string highlight = gangSepi ? "\u22121 Nyawa" : $"+{bonusJalurAman} poin";
        Color  warnaHighlight = gangSepi ? new Color(0.95f, 0.35f, 0.30f, 1f)
                                         : new Color(0.95f, 0.62f, 0.10f, 1f);
        var hl = BuatTeks(card.transform, "Highlight", highlight, 64, warnaHighlight, FontStyles.Bold);
        hl.alignment = TextAlignmentOptions.Center;
        var hlRt = hl.rectTransform;
        hlRt.anchorMin = new Vector2(0.5f, 0.5f); hlRt.anchorMax = new Vector2(0.5f, 0.5f);
        hlRt.pivot = new Vector2(0.5f, 0.5f); hlRt.sizeDelta = new Vector2(820f, 100f);
        hlRt.anchoredPosition = new Vector2(0f, 60f);

        // Subtitel.
        var sub = BuatTeks(card.transform, "Subtitel", subtitel, 23,
            new Color(1f, 0.95f, 0.85f, 0.97f), FontStyles.Normal);
        sub.alignment = TextAlignmentOptions.Center;
        var sRt = sub.rectTransform;
        sRt.anchorMin = new Vector2(0f, 0.5f); sRt.anchorMax = new Vector2(1f, 0.5f);
        sRt.pivot = new Vector2(0.5f, 1f); sRt.sizeDelta = new Vector2(-90f, 150f);
        sRt.anchoredPosition = new Vector2(0f, 0f);

        // Tombol lanjut.
        Color warnaTombol = gangSepi ? new Color(0.86f, 0.45f, 0.20f, 1f)
                                     : new Color(0.20f, 0.70f, 0.36f, 1f);
        BuatTombol(card.transform, "LANJUT KE KARTU EDUKASI", warnaTombol, () =>
        {
            AudioManager.Instance?.Click();
            if (_canvasGO != null) Destroy(_canvasGO);
            onLanjut?.Invoke();
        });

        StartCoroutine(PopIn(cRt));
    }

    // ══════════════════════════════════════════════════════════════════════
    // 2) KARTU EDUKASI — bercabang per jalur
    // ══════════════════════════════════════════════════════════════════════
    void BuildEduCard(Action onLanjut)
    {
        bool gangSepi = AmbilGangSepi;

        string tips;
        if (gangSepi)
        {
            tips =
                "<color=#FF8A7A><b>\uD83D\uDD34 Rara tadi lewat GANG SEPI\u2026 itu berisiko!</b></color>\n" +
                "Jalan pintas yang sepi & gelap = tempat paling rawan. Tak ada orang yang bisa menolong kalau terjadi sesuatu.\n\n" +
                "<color=#8FE3A2><b>\u2705 Lain kali, pilih JALAN RAMAI:</b></color>\n" +
                "•  Banyak orang = banyak <b>saksi</b> & tempat minta tolong.\n" +
                "•  Lebih terang, lebih mudah lari ke warung/rumah orang.\n" +
                "•  Sedikit lebih jauh tak apa \u2014 <b>selamat lebih penting</b> daripada cepat.\n\n" +
                "<color=#FFD24A><b>\uD83D\uDCE2 Kalau merasa diikuti:</b></color>  TERIAK, lari ke keramaian, dan CERITA ke orang dewasa yang dipercaya.";
        }
        else
        {
            tips =
                "<color=#8FE3A2><b>\u2705 Hebat! Rara memilih JALAN RAMAI.</b></color>\n" +
                "Jalan yang ramai & terang itu paling aman: banyak orang yang bisa jadi saksi dan tempat minta tolong.\n\n" +
                "<color=#FFD24A><b>\uD83D\uDEA9 Jauhi jalan pintas yang sepi!</b></color>\n" +
                "Gang gelap atau jalan sepi memang lebih cepat, tapi paling rawan \u2014 hindari walau terburu-buru.\n\n" +
                "<color=#8FE3A2><b>\uD83D\uDDDD 3 Kata Sakti kalau merasa nggak aman:</b></color>\n" +
                "•  <b>TIDAK!</b>  — kamu BERHAK menolak siapa pun.\n" +
                "•  <b>PERGI!</b>  — menjauh & lari ke tempat yang ramai.\n" +
                "•  <b>CERITA!</b> — laporkan ke orang dewasa yang dipercaya.";
        }

        var go = BuatCanvas("EduCardDay1Canvas");

        var overlay = BuatImage(go.transform, "Overlay", new Color(0f, 0f, 0f, 0.82f));
        Stretch(overlay.rectTransform);
        overlay.raycastTarget = true;

        // Kartu coklat + border emas.
        var card = BuatImage(go.transform, "KartuEdukasi", new Color(0.16f, 0.08f, 0.04f, 0.97f));
        card.sprite = GetRoundedRect(); card.type = Image.Type.Sliced;
        var cardOutline = card.gameObject.AddComponent<Outline>();
        cardOutline.effectColor    = new Color(0.95f, 0.72f, 0.18f, 0.95f);
        cardOutline.effectDistance = new Vector2(3f, -3f);
        var cardRt = card.rectTransform;
        cardRt.anchorMin = new Vector2(0.5f, 0.5f); cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.pivot = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = new Vector2(900f, 620f);

        // Pita judul emas.
        var pita = BuatImage(card.transform, "PitaJudul", new Color(0.95f, 0.72f, 0.18f, 1f));
        pita.sprite = GetRoundedRect(); pita.type = Image.Type.Sliced; pita.raycastTarget = false;
        var pitaRt = pita.rectTransform;
        pitaRt.anchorMin = new Vector2(0f, 1f); pitaRt.anchorMax = new Vector2(1f, 1f);
        pitaRt.pivot = new Vector2(0.5f, 1f);
        pitaRt.offsetMin = new Vector2(28f, -96f); pitaRt.offsetMax = new Vector2(-28f, -22f);

        string judulKartu = gangSepi
            ? "KARTU EDUKASI \u2014 PELAJARAN HARI 1"
            : "KARTU EDUKASI \u2014 HARI 1";
        var judul = BuatTeks(pita.transform, "Judul", judulKartu, 32,
            new Color(0.18f, 0.09f, 0.02f, 1f), FontStyles.Bold);
        judul.alignment = TextAlignmentOptions.Center;
        Stretch(judul.rectTransform);

        // Isi tips.
        var isi = BuatTeks(card.transform, "Isi", tips, 23,
            new Color(1f, 1f, 0.90f, 0.97f), FontStyles.Normal);
        isi.alignment = TextAlignmentOptions.Center;
        var isiRt = isi.rectTransform;
        isiRt.anchorMin = new Vector2(0f, 0f); isiRt.anchorMax = new Vector2(1f, 1f);
        isiRt.offsetMin = new Vector2(50f, 120f); isiRt.offsetMax = new Vector2(-50f, -112f);

        // Tombol lanjutkan.
        BuatTombol(card.transform, "LANJUTKAN", new Color(0.20f, 0.70f, 0.36f, 1f), () =>
        {
            AudioManager.Instance?.Click();
            if (_canvasGO != null) Destroy(_canvasGO);
            onLanjut?.Invoke();
        });

        StartCoroutine(PopIn(cardRt));
    }

    // ══════════════════════════════════════════════════════════════════════
    // HELPERS
    // ══════════════════════════════════════════════════════════════════════
    GameObject BuatCanvas(string nama)
    {
        _canvasGO = new GameObject(nama);
        var canvas = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var sc = _canvasGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920f, 1080f);
        sc.matchWidthOrHeight  = 0.5f;
        _canvasGO.AddComponent<GraphicRaycaster>();

        // Pastikan ada EventSystem agar tombol bisa diklik.
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        return _canvasGO;
    }

    void BuatTombol(Transform parent, string teks, Color warna, Action onClick)
    {
        var go = new GameObject("Tombol");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.sprite = GetRoundedRect(); img.type = Image.Type.Sliced; img.color = warna;
        var outline = go.AddComponent<Outline>();
        outline.effectColor    = new Color(1f, 1f, 1f, 0.35f);
        outline.effectDistance = new Vector2(2f, -2f);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f); rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 28f);
        rt.sizeDelta = new Vector2(560f, 80f);

        var label = BuatTeks(go.transform, "Label", teks, 26, Color.white, FontStyles.Bold);
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.enableAutoSizing = true;
        label.fontSizeMin = 18f; label.fontSizeMax = 26f;
        Stretch(label.rectTransform);
        label.rectTransform.offsetMin = new Vector2(24f, 0f);
        label.rectTransform.offsetMax = new Vector2(-24f, 0f);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => onClick?.Invoke());
    }

    Image BuatImage(Transform parent, string nama, Color warna)
    {
        var go = new GameObject(nama);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = warna;
        return img;
    }

    TextMeshProUGUI BuatTeks(Transform parent, string nama, string isi,
                             int size, Color color, FontStyles style)
    {
        var go = new GameObject(nama);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        TMP_FontAsset f = fontAsset ?? TMP_Settings.defaultFontAsset;
        if (f != null) tmp.font = f;
        tmp.text      = isi;
        tmp.fontSize  = size;
        tmp.color     = color;
        tmp.fontStyle = style;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        return tmp;
    }

    void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    void FreezePlayer(bool freeze)
    {
        if (!freezePlayerSaatTampil) return;
        var d1 = FindFirstObjectByType<Day1Controller>(FindObjectsInactive.Include);
        if (d1 == null) return;
        if (freeze) d1.FreezePlayer();
        else        d1.ResumePlayer();
    }

    IEnumerator PopIn(RectTransform rt)
    {
        if (rt == null) yield break;
        float t = 0f, dur = 0.28f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0.85f, 1f, Mathf.Clamp01(t / dur));
            rt.localScale = new Vector3(k, k, 1f);
            yield return null;
        }
        rt.localScale = Vector3.one;
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

    // ══════════════════════════════════════════════════════════════════════
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.30f, 1f, 0.45f, 0.85f);
        Gizmos.DrawLine(new Vector3(triggerX, -10f, 0f), new Vector3(triggerX, 10f, 0f));
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(new Vector3(triggerX + 0.2f, 2f, 0f),
            $"Sampai Sekolah X={triggerX}");
        #endif
    }
}
