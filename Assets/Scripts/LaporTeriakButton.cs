using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// LaporTeriakButton — Tantangan "Berani Lapor" Day 2.
///
/// Pemain harus TAHAN tombol "TERIAK!" selama N detik berturut-turut
/// dalam window waktu terbatas. Kalau berhasil:
///   - Bonus poin LAPOR
///   - Achievement "Berani Lapor"
///   - Reaksi sukses: polisi datang
///
/// Tap-only mode \u2014 tidak butuh mikrofon. Cocok untuk semua platform.
/// Semua label/durasi/warna bisa di-custom lewat Inspector.
/// </summary>
public class LaporTeriakButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Narasi Awal")]
    public string judulTeks = "\uD83D\uDEA8 SAATNYA LAPOR!";
    public Color  judulWarna = new Color(1f, 0.45f, 0.45f, 1f);
    public int    judulUkuran = 38;
    [TextArea(2, 5)]
    public string deskripsiTeks =
        "Kamu sudah turun dari angkot.\nDi depan pos polisi: TAHAN tombol TERIAK untuk minta tolong!\n\nTahan selama {DURASI} detik sebelum waktu habis.";
    public Color  deskripsiWarna = new Color(1f, 1f, 0.92f, 0.95f);
    public int    deskripsiUkuran = 22;

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
    public string reaksiBerhasil  = "\u2713 Polisi langsung datang! Bagus, kamu sudah belajar berani lapor.";
    [TextArea(2, 4)]
    public string reaksiGagal     = "\u2716 Kamu nggak berani teriak. Lain kali, beranikan diri ya!";

    [Header("Tombol Lanjut")]
    public string tombolLanjutTeks = "\u25B6  Lanjut ke Kartu Edukasi";
    public Color  warnaLanjut      = new Color(0.20f, 0.62f, 0.86f, 1f);

    [Header("Font")]
    public TMP_FontAsset fontAsset;

    [Header("Sorting")]
    public int sortingOrder = 940;

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

    // ══════════════════════════════════════════════════════════════════════
    public void Mulai(Action onSelesai)
    {
        _onSelesai = onSelesai;
        BuildScene();
        StartCoroutine(TimerCoroutine());
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
        var j = BuatTeks(card.transform, "Judul", judulTeks, judulUkuran, judulWarna, FontStyles.Bold);
        j.alignment = TextAlignmentOptions.Center;
        var jrt = j.rectTransform;
        jrt.anchorMin = new Vector2(0f, 1f); jrt.anchorMax = new Vector2(1f, 1f);
        jrt.pivot = new Vector2(0.5f, 1f);
        jrt.offsetMin = new Vector2(40f, -90f);
        jrt.offsetMax = new Vector2(-40f, -25f);

        // Deskripsi
        string desc = deskripsiTeks.Replace("{DURASI}", durasiTahan.ToString("0.0"));
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
    }

    // ══════════════════════════════════════════════════════════════════════
    public void OnPointerDown(PointerEventData eventData) { if (!_selesai) _ditekan = true; }
    public void OnPointerUp(PointerEventData eventData)   { _ditekan = false; }

    void Update()
    {
        if (_selesai) return;

        if (_ditekan)
        {
            _holdProgress += Time.deltaTime;
            if (_tombolImg != null) _tombolImg.color = teriakWarnaDitekan;
            if (_tombolLabel != null) _tombolLabel.text = "\uD83D\uDD0A  TERIAAAK!!";
        }
        else
        {
            // Reset hold kalau lepas
            if (_holdProgress > 0f) _holdProgress = Mathf.Max(0f, _holdProgress - Time.deltaTime * 2f);
            if (_tombolImg != null) _tombolImg.color = teriakWarna;
            if (_tombolLabel != null) _tombolLabel.text = teriakLabel;
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
                gs.score += bonusBerhasil;
                gs.AddChoice(2, "Lapor: berani teriak ke polisi", "AMAN", bonusBerhasil);
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
            if (gs != null) gs.AddChoice(2, "Lapor: gagal/waktu habis", "RAGU", 0);
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

        var teks = BuatTeks(panel.transform, "Teks",
            _berhasil ? reaksiBerhasil + (bonusBerhasil > 0 ? $"\n\n<color=#FFD24A>+{bonusBerhasil} poin</color>" : "")
                       : reaksiGagal,
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
