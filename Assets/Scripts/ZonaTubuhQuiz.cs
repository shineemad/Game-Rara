using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// ZonaTubuhQuiz — Quiz drag-and-drop untuk Fase 4 Day 2.
///
/// Pemain harus drag setiap chip (label perilaku) ke kolom yang tepat:
///   - "ZONA AMAN" : perilaku yang ok (jabat tangan, peluk keluarga, dst.)
///   - "ZONA BAHAYA": perilaku yang HARUS DITOLAK (sentuh tanpa izin, dll.)
///
/// Setelah waktu habis ATAU semua chip ditempatkan:
///   - Skor +SCORE_QUIZ per chip benar
///   - Bonus achievement "Penjaga Batas Tubuh" kalau 6/6 benar
///   - Tampilkan tombol lanjut
///
/// Custom semua chip & label lewat Inspector.
/// </summary>
public class ZonaTubuhQuiz : MonoBehaviour
{
    [System.Serializable]
    public class ChipData
    {
        public string teks;
        [Tooltip("Jawaban benar: AMAN atau BAHAYA.")]
        public string jawabanBenar = "AMAN"; // "AMAN" | "BAHAYA"
    }

    [Header("Judul & Instruksi")]
    public string judulTeks = "\uD83D\uDEE1  Quiz: Mana yang BOLEH, mana yang TIDAK BOLEH?";
    public Color  judulWarna = new Color(1f, 0.85f, 0.3f, 1f);
    public int    judulUkuran = 30;
    [TextArea(2, 3)]
    public string instruksiTeks = "Tarik setiap chip ke ZONA AMAN atau ZONA BAHAYA.\nSetiap jawaban benar = poin. Waktu terbatas!";
    public Color  instruksiWarna = new Color(1f, 1f, 0.92f, 0.85f);
    public int    instruksiUkuran = 18;

    [Header("Timer")]
    public float waktuDetik = 15f;
    public Color warnaTimer = new Color(1f, 0.85f, 0.3f, 1f);
    public Color warnaTimerKritis = new Color(0.91f, 0.30f, 0.24f, 1f);
    public int   ukuranTimer = 28;

    [Header("Daftar Chip (CUSTOMIZABLE)")]
    public ChipData[] chips = new ChipData[]
    {
        new ChipData { teks = "Salam jabat tangan", jawabanBenar = "AMAN" },
        new ChipData { teks = "Peluk ortu/saudara",  jawabanBenar = "AMAN" },
        new ChipData { teks = "Cek up dokter (didampingi)", jawabanBenar = "AMAN" },
        new ChipData { teks = "Disentuh paksa orang asing", jawabanBenar = "BAHAYA" },
        new ChipData { teks = "Diminta lepas baju oleh orang asing", jawabanBenar = "BAHAYA" },
        new ChipData { teks = "Disuruh simpan rahasia 'pertemuan kita'", jawabanBenar = "BAHAYA" }
    };

    [Header("Warna Zona")]
    public Color warnaZonaAman   = new Color(0.10f, 0.35f, 0.22f, 0.92f);
    public Color warnaZonaBahaya = new Color(0.40f, 0.12f, 0.12f, 0.92f);
    public Color warnaBorderAman = new Color(0.45f, 1f, 0.65f, 1f);
    public Color warnaBorderBahaya = new Color(1f, 0.45f, 0.45f, 1f);

    [Header("Chip Style")]
    public Color  chipWarna       = new Color(0.18f, 0.20f, 0.30f, 0.95f);
    public Color  chipTeksWarna   = new Color(1f, 1f, 0.92f, 1f);
    public Color  chipBenarWarna  = new Color(0.18f, 0.62f, 0.32f, 0.95f);
    public Color  chipSalahWarna  = new Color(0.78f, 0.20f, 0.20f, 0.95f);
    public int    chipUkuranTeks  = 18;
    public Vector2 chipUkuran     = new Vector2(280f, 60f);

    [Header("Achievement (Bonus)")]
    public string namaAchievement = "Penjaga Batas Tubuh";
    public int    bonusAllBenar   = 200;

    [Header("Tombol Lanjut")]
    public string tombolLanjutTeks = "\u25B6  Lanjut";
    public Color  warnaLanjut = new Color(0.18f, 0.62f, 0.32f, 1f);

    [Header("Font")]
    public TMP_FontAsset fontAsset;

    [Header("Sorting")]
    public int sortingOrder = 930;

    // ── runtime ───────────────────────────────────────────────────────────
    private Action     _onSelesai;
    private GameObject _canvasGO;
    private RectTransform _zonaAmanRT;
    private RectTransform _zonaBahayaRT;
    private TextMeshProUGUI _timerText;
    private TextMeshProUGUI _skorText;
    private float      _sisaWaktu;
    private bool       _quizSelesai;
    private int        _chipDitempatkan;
    private int        _chipBenar;
    private List<GameObject> _chipPool = new List<GameObject>();
    private Sprite     _roundedSprite;
    private Canvas     _canvasComp;

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
        _canvasGO = new GameObject("ZonaTubuhQuiz_Canvas");
        _canvasComp = _canvasGO.AddComponent<Canvas>();
        _canvasComp.renderMode  = RenderMode.ScreenSpaceOverlay;
        _canvasComp.sortingOrder = sortingOrder;
        var scaler = _canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        _canvasGO.AddComponent<GraphicRaycaster>();

        // Pastikan ada EventSystem (untuk drag)
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // Judul
        var judul = BuatTeks(_canvasGO.transform, "Judul", judulTeks, judulUkuran, judulWarna, FontStyles.Bold);
        judul.alignment = TextAlignmentOptions.Center;
        var jrt = judul.rectTransform;
        jrt.anchorMin = new Vector2(0f, 1f); jrt.anchorMax = new Vector2(1f, 1f);
        jrt.pivot = new Vector2(0.5f, 1f);
        jrt.offsetMin = new Vector2(40f, -90f);
        jrt.offsetMax = new Vector2(-40f, -25f);

        // Instruksi
        var instr = BuatTeks(_canvasGO.transform, "Instruksi", instruksiTeks, instruksiUkuran, instruksiWarna, FontStyles.Italic);
        instr.alignment = TextAlignmentOptions.Center;
        var irt = instr.rectTransform;
        irt.anchorMin = new Vector2(0f, 1f); irt.anchorMax = new Vector2(1f, 1f);
        irt.pivot = new Vector2(0.5f, 1f);
        irt.offsetMin = new Vector2(40f, -160f);
        irt.offsetMax = new Vector2(-40f, -95f);

        // Timer + Skor (atas kanan & kiri)
        _timerText = BuatTeks(_canvasGO.transform, "Timer", "00:15", ukuranTimer, warnaTimer, FontStyles.Bold);
        _timerText.alignment = TextAlignmentOptions.MidlineRight;
        var trt = _timerText.rectTransform;
        trt.anchorMin = new Vector2(1f, 1f); trt.anchorMax = new Vector2(1f, 1f);
        trt.pivot = new Vector2(1f, 1f);
        trt.sizeDelta = new Vector2(220f, 50f);
        trt.anchoredPosition = new Vector2(-40f, -25f);

        _skorText = BuatTeks(_canvasGO.transform, "Skor", "Benar: 0/" + chips.Length, 24, new Color(1f, 1f, 0.92f, 1f), FontStyles.Bold);
        _skorText.alignment = TextAlignmentOptions.MidlineLeft;
        var srt = _skorText.rectTransform;
        srt.anchorMin = new Vector2(0f, 1f); srt.anchorMax = new Vector2(0f, 1f);
        srt.pivot = new Vector2(0f, 1f);
        srt.sizeDelta = new Vector2(280f, 50f);
        srt.anchoredPosition = new Vector2(40f, -25f);

        // Zona kiri (AMAN) + kanan (BAHAYA)
        _zonaAmanRT   = BuatZona("ZONA_AMAN",   "\u2713  ZONA AMAN",   warnaZonaAman,   warnaBorderAman,   new Vector2(-450f, -80f));
        _zonaBahayaRT = BuatZona("ZONA_BAHAYA", "\u2716  ZONA BAHAYA", warnaZonaBahaya, warnaBorderBahaya, new Vector2( 450f, -80f));

        // Container chip di bawah
        var chipArea = new GameObject("ChipArea");
        chipArea.transform.SetParent(_canvasGO.transform, false);
        var caRT = chipArea.AddComponent<RectTransform>();
        caRT.anchorMin = new Vector2(0.5f, 0f); caRT.anchorMax = new Vector2(0.5f, 0f);
        caRT.pivot = new Vector2(0.5f, 0f);
        caRT.sizeDelta = new Vector2(1700f, 200f);
        caRT.anchoredPosition = new Vector2(0f, 50f);

        var grid = chipArea.AddComponent<GridLayoutGroup>();
        grid.cellSize = chipUkuran;
        grid.spacing  = new Vector2(20f, 20f);
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = Mathf.Min(6, chips.Length);

        foreach (var c in chips)
        {
            var go = BuatChip(c, chipArea.transform);
            _chipPool.Add(go);
        }
    }

    RectTransform BuatZona(string name, string label, Color bg, Color border, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_canvasGO.transform, false);
        var img = go.AddComponent<Image>();
        img.sprite = GetRoundedSprite();
        img.color  = bg;
        img.type   = Image.Type.Sliced;
        var outl = go.AddComponent<Outline>();
        outl.effectColor    = border;
        outl.effectDistance = new Vector2(3f, -3f);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(780f, 420f);
        rt.anchoredPosition = pos;

        var lab = BuatTeks(go.transform, "Label", label, 32, Color.white, FontStyles.Bold);
        lab.alignment = TextAlignmentOptions.Center;
        var lrt = lab.rectTransform;
        lrt.anchorMin = new Vector2(0f, 1f); lrt.anchorMax = new Vector2(1f, 1f);
        lrt.pivot = new Vector2(0.5f, 1f);
        lrt.offsetMin = new Vector2(20f, -65f);
        lrt.offsetMax = new Vector2(-20f, -15f);

        return rt;
    }

    GameObject BuatChip(ChipData data, Transform parent)
    {
        var go = new GameObject("Chip_" + data.teks);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.sprite = GetRoundedSprite();
        img.color  = chipWarna;
        img.type   = Image.Type.Sliced;
        var outl = go.AddComponent<Outline>();
        outl.effectColor    = new Color(1f, 1f, 1f, 0.35f);
        outl.effectDistance = new Vector2(1f, -1f);

        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = chipUkuran.x; le.preferredHeight = chipUkuran.y;

        var teks = BuatTeks(go.transform, "Label", data.teks, chipUkuranTeks, chipTeksWarna, FontStyles.Bold);
        teks.alignment = TextAlignmentOptions.Center;
        var trt = teks.rectTransform;
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(8f, 4f); trt.offsetMax = new Vector2(-8f, -4f);

        // Drag handler
        var drag = go.AddComponent<DraggableChip>();
        drag.canvas    = _canvasComp;
        drag.quiz      = this;
        drag.data      = data;
        drag.chipImage = img;

        return go;
    }

    // ══════════════════════════════════════════════════════════════════════
    IEnumerator TimerCoroutine()
    {
        _sisaWaktu = waktuDetik;
        while (_sisaWaktu > 0f && !_quizSelesai)
        {
            _sisaWaktu -= Time.deltaTime;
            int s = Mathf.CeilToInt(_sisaWaktu);
            _timerText.text = "\u23F1 " + (s < 10 ? "00:0" + s : "00:" + s);
            _timerText.color = s <= 5 ? warnaTimerKritis : warnaTimer;
            yield return null;
        }
        if (!_quizSelesai) SelesaikanQuiz();
    }

    // Dipanggil oleh DraggableChip saat drop selesai
    public bool CekDrop(ChipData data, Vector2 screenPos)
    {
        if (_quizSelesai) return false;

        bool diZonaAman   = RectTransformUtility.RectangleContainsScreenPoint(_zonaAmanRT,   screenPos);
        bool diZonaBahaya = RectTransformUtility.RectangleContainsScreenPoint(_zonaBahayaRT, screenPos);

        if (!diZonaAman && !diZonaBahaya) return false;

        string jawabanPemain = diZonaAman ? "AMAN" : "BAHAYA";
        bool benar = jawabanPemain == data.jawabanBenar;

        _chipDitempatkan++;
        if (benar) _chipBenar++;
        _skorText.text = $"Benar: {_chipBenar}/{chips.Length}";

        // SFX
        var am = AudioManager.Instance;
        if (am != null && am.sfxSource != null)
        {
            if (benar && am.sfxCorrect != null) am.sfxSource.PlayOneShot(am.sfxCorrect);
            else if (!benar && am.sfxWrong != null) am.sfxSource.PlayOneShot(am.sfxWrong);
        }

        // Score
        var gs = GameState.Instance;
        if (gs != null)
        {
            int pts = benar ? (GameState.SCORE_QUIZ / 2) : 0; // 100 poin per chip benar
            gs.score += pts;
            gs.AddChoice(2, $"Quiz: {data.teks} \u2192 {jawabanPemain}", benar ? "AMAN" : "BAHAYA", pts);
        }

        if (_chipDitempatkan >= chips.Length) SelesaikanQuiz();
        return true;
    }

    void SelesaikanQuiz()
    {
        if (_quizSelesai) return;
        _quizSelesai = true;
        StopAllCoroutines();

        var gs = GameState.Instance;
        bool semuaBenar = _chipBenar == chips.Length;
        if (semuaBenar && gs != null)
        {
            gs.score += bonusAllBenar;
            if (!gs.achievements.Contains(namaAchievement))
            {
                gs.achievements.Add(namaAchievement);
                AchievementPopup.Show(namaAchievement);
            }
            Debug.Log($"[ZonaTubuhQuiz] PERFECT! Bonus +{bonusAllBenar} + achievement.");
        }

        BuildLayarHasil(semuaBenar);
    }

    void BuildLayarHasil(bool semuaBenar)
    {
        // Tombol Lanjut
        var btnGO = new GameObject("LanjutBtn");
        btnGO.transform.SetParent(_canvasGO.transform, false);
        var img = btnGO.AddComponent<Image>();
        img.sprite = GetRoundedSprite();
        img.color  = warnaLanjut;
        img.type   = Image.Type.Sliced;
        var outl = btnGO.AddComponent<Outline>();
        outl.effectColor    = Color.white;
        outl.effectDistance = new Vector2(2f, -2f);
        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f); rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(360f, 70f);
        rt.anchoredPosition = new Vector2(0f, 30f);

        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() =>
        {
            AudioManager.Instance?.Click();
            if (_canvasGO != null) Destroy(_canvasGO);
            _onSelesai?.Invoke();
        });

        string label = semuaBenar
            ? $"\uD83C\uDFC6  Perfect! +{bonusAllBenar} bonus"
            : $"\u25B6  Lanjut  ({_chipBenar}/{chips.Length} benar)";

        var lab = BuatTeks(btnGO.transform, "Label", label, 22, Color.white, FontStyles.Bold);
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

// ──────────────────────────────────────────────────────────────────────────
// Helper drag component (di file yang sama supaya nggak nambah file kecil).
// ──────────────────────────────────────────────────────────────────────────
public class DraggableChip : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Canvas canvas;
    public ZonaTubuhQuiz quiz;
    public ZonaTubuhQuiz.ChipData data;
    public Image chipImage;

    private RectTransform _rt;
    private CanvasGroup _cg;
    private Vector2 _posAwal;
    private Transform _parentAwal;
    private bool _dropAccepted;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _cg = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _posAwal = _rt.anchoredPosition;
        _parentAwal = transform.parent;
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();
        _cg.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform, eventData.position, canvas.worldCamera, out local);
        _rt.anchoredPosition = local;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _cg.blocksRaycasts = true;
        bool accepted = quiz.CekDrop(data, eventData.position);
        if (accepted)
        {
            _dropAccepted = true;
            // Disable & fade
            _cg.interactable = false;
            chipImage.color = new Color(chipImage.color.r, chipImage.color.g, chipImage.color.b, 0.4f);
        }
        else
        {
            // Kembali ke posisi awal
            transform.SetParent(_parentAwal, false);
            _rt.anchoredPosition = _posAwal;
        }
    }
}
