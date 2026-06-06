using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SampaiSekolahPopup — popup "Rara berhasil sampai" yang muncul otomatis saat
/// Rara melewati titik X tertentu. Style: panel hijau + judul + poin emas besar
/// + subtitel + tombol lanjut.
///
/// Setelah klik LANJUT → hubungkan event onLanjut ke EduCardDay1.Tampilkan()
/// agar kartu edukasi muncul setelahnya.
///
/// Cara pakai:
///   1. GameObject → Create Empty → "SampaiSekolahPopup_Day1"
///   2. Add Component → SampaiSekolahPopup
///   3. Drag Player ke field player (atau biarkan auto-find via tag Player)
///   4. Atur triggerX (mis. 42)
///   5. (Opsional) Drag sprite background custom, ornamen atas, ubah teks
///   6. Hubungkan event onLanjut → EduCardDay1.Tampilkan()
/// </summary>
public class SampaiSekolahPopup : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR
    // ══════════════════════════════════════════════════════════════════════

    [Header("Referensi")]
    [Tooltip("Player Rara — jika kosong, dicari otomatis via tag 'Player'.")]
    public Transform player;

    [Header("Trigger")]
    [Tooltip("X position world. Saat Rara x ≥ nilai ini → popup muncul.")]
    public float triggerX = 42f;
    [Tooltip("Hanya dipicu sekali per sesi.")]
    public bool  triggerOnce = true;
    [Tooltip("Bekukan pergerakan player saat popup tampil.")]
    public bool  freezePlayerSaatTampil = true;
    [Tooltip("Jeda (detik) sebelum popup tampil setelah trigger tercapai.")]
    public float jedaSebelumTampil = 0.4f;
    [Tooltip("Tambahkan nilaiPoin ke GameState.score saat popup muncul.")]
    public bool  tambahPoinKeGameState = true;

    [Header("Background Popup (CUSTOMIZABLE)")]
    [Tooltip("Sprite latar popup utama. Kosong = panel solid + rounded corner hijau.")]
    public Sprite backgroundSprite;
    [Tooltip("Sliced = stretch dengan border 9-slice, Simple = stretch biasa.")]
    public Image.Type backgroundImageType = Image.Type.Sliced;
    [Tooltip("Warna tint background (default hijau tua).")]
    public Color backgroundColor = new Color(0.06f, 0.18f, 0.10f, 0.98f);
    [Tooltip("Sprite border/frame di atas background (opsional).")]
    public Sprite borderSprite;
    [Tooltip("Warna border (default hijau neon).")]
    public Color borderColor = new Color(0.30f, 1f, 0.45f, 1f);

    [Header("Ornamen Atas (opsional)")]
    [Tooltip("Sprite hiasan di tengah-atas popup (mis. atap gerbang sekolah / rumah gadang).")]
    public Sprite ornamenAtasSprite;
    public Vector2 ornamenAtasSize    = new Vector2(260f, 130f);
    [Tooltip("Offset Y ornamen relatif ke top edge popup (positif = naik ke luar).")]
    public float   ornamenAtasOffsetY = 40f;

    [Header("Overlay Belakang (dim layar)")]
    public bool   tampilkanOverlay = true;
    public Sprite overlaySprite;
    public Color  overlayColor     = new Color(0f, 0f, 0f, 0.78f);

    [Header("Judul Sukses")]
    [Tooltip("Judul utama popup.")]
    public string judul       = "🎉  Yeay! Rara Sampai di Sekolah!";
    public Color  warnaJudul  = new Color(0.55f, 1f, 0.55f, 1f);
    public int    ukuranJudul = 38;

    [Header("Poin Besar (Highlight Emas)")]
    [Tooltip("Tampilkan baris poin emas besar?")]
    public bool   tampilkanPoin = true;
    [Tooltip("Nilai poin yang ditampilkan.")]
    public int    nilaiPoin     = 100;
    [Tooltip("Format teks poin. {0} diganti nilaiPoin.")]
    public string formatPoin    = "✦  +{0} poin  ✦";
    public Color  warnaPoin     = new Color(1f, 0.78f, 0.20f, 1f);
    public int    ukuranPoin    = 72;

    [Header("Subtitel")]
    [TextArea(1, 3)]
    public string subtitel       = "Rara pilih Jalan Ramai dan tiba dengan selamat! ✅";
    public Color  warnaSubtitel  = new Color(1f, 0.93f, 0.75f, 1f);
    public int    ukuranSubtitel = 22;

    [Header("Tombol Lanjut")]
    public string tombolLanjutTeks  = "[  LANJUT  →  KARTU EDUKASI  ]";
    [Tooltip("Sprite tombol custom (mis. ornament Minang). Kosong = solid color rounded.")]
    public Sprite tombolSprite;
    public Color  tombolWarna       = new Color(0.42f, 0.24f, 0.08f, 1f);
    public Color  tombolBorderColor = new Color(1f, 0.78f, 0.20f, 1f);
    public Color  tombolTeksWarna   = new Color(1f, 0.86f, 0.40f, 1f);
    public int    tombolUkuranTeks  = 26;
    [Tooltip("Sembunyikan tombol jika ingin auto-close.")]
    public bool   sembunyikanTombol = false;
    [Tooltip("Auto-close popup setelah N detik (0 = tunggu klik).")]
    public float  autoCloseDetik    = 0f;

    [Header("Font (opsional)")]
    public TMP_FontAsset fontAsset;

    [Header("Animasi")]
    [Range(0.5f, 1f)] public float skalaAwal = 0.85f;
    public float durasiPopIn = 0.35f;

    [Header("Audio (opsional)")]
    public AudioClip sfxMunculKartu;
    public AudioClip sfxKlikLanjut;

    [Header("Sorting")]
    [Tooltip("Sorting order Canvas. Default 1010 — di atas dialog (999).")]
    public int sortingOrder = 1010;

    [Header("Kartu Edukasi Selanjutnya (RECOMMENDED)")]
    [Tooltip("Drag GameObject EduCardDay1 ke sini. Setelah klik LANJUT, kartu edukasi ini otomatis tampil. " +
             "Jika kosong, script akan FindFirstObjectByType<EduCardDay1>() di scene.")]
    public EduCardDay1 kartuEduSelanjutnya;
    [Tooltip("Aktifkan auto-find EduCardDay1 di scene jika kartuEduSelanjutnya kosong.")]
    public bool autoCariEduCard = true;

    [Header("Event")]
    [Tooltip("Dipanggil saat pemain klik tombol Lanjut. Selain kartuEduSelanjutnya, bisa tambah aksi lain di sini.")]
    public UnityEngine.Events.UnityEvent onLanjut;

    // ── runtime ───────────────────────────────────────────────────────────
    private bool   _triggered;
    private bool   _kartuTampil;
    private GameObject _canvasGO;
    private Sprite _roundedRectSprite;

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
        if (_kartuTampil) return;
        if (player == null) return;

        if (player.position.x >= triggerX)
        {
            _triggered = true;
            StartCoroutine(TampilkanKartu());
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // PUBLIC API — bisa dipanggil dari script lain
    // ══════════════════════════════════════════════════════════════════════

    /// Paksa tampilkan kartu sekarang (mis. dari trigger lain).
    public void TampilkanSekarang()
    {
        if (_kartuTampil) return;
        _triggered = true;
        StartCoroutine(TampilkanKartu());
    }

    /// Tutup kartu yang sedang tampil.
    public void Tutup()
    {
        if (!_kartuTampil) return;
        if (_canvasGO != null) Destroy(_canvasGO);
        _canvasGO     = null;
        _kartuTampil  = false;

        // Bebaskan player
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

        if (jedaSebelumTampil > 0f) yield return new WaitForSeconds(jedaSebelumTampil);

        // Tambah poin ke GameState
        if (tambahPoinKeGameState && nilaiPoin != 0 && GameState.Instance != null)
            GameState.Instance.score += nilaiPoin;

        // SFX
        if (sfxMunculKartu != null)
            AudioManager.Instance?.sfxSource?.PlayOneShot(sfxMunculKartu);
        else
            AudioManager.Instance?.PlayAchievement();

        BuildUI();

        if (autoCloseDetik > 0f)
        {
            yield return new WaitForSeconds(autoCloseDetik);
            HandleLanjut();
        }
    }

    void BuildUI()
    {
        // ── Canvas ────────────────────────────────────────────────────────
        _canvasGO = new GameObject("EduCardPopupCanvas");
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
            ovImg.raycastTarget = true; // block click ke world
        }

        // ── Popup utama (panel tengah) ────────────────────────────────────
        var card = new GameObject("Popup");
        card.transform.SetParent(_canvasGO.transform, false);
        var cardRT = card.AddComponent<RectTransform>();
        cardRT.anchorMin = cardRT.anchorMax = new Vector2(0.5f, 0.5f);
        cardRT.pivot     = new Vector2(0.5f, 0.5f);
        cardRT.sizeDelta = new Vector2(980f, 520f);

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

        // Border frame
        if (borderSprite != null)
        {
            var border = new GameObject("Border");
            border.transform.SetParent(card.transform, false);
            var bRT = border.AddComponent<RectTransform>();
            bRT.anchorMin = Vector2.zero; bRT.anchorMax = Vector2.one;
            bRT.offsetMin = bRT.offsetMax = Vector2.zero;
            var bImg = border.AddComponent<Image>();
            bImg.sprite = borderSprite;
            bImg.type   = Image.Type.Sliced;
            bImg.color  = borderColor;
            bImg.raycastTarget = false;
        }
        else
        {
            var ol = card.AddComponent<Outline>();
            ol.effectColor    = borderColor;
            ol.effectDistance = new Vector2(4f, -4f);
        }

        // ── Ornamen atas (atap rumah / gerbang sekolah) ──────────────────
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

        // ── Layout vertikal isi popup ────────────────────────────────────
        var content = new GameObject("Content");
        content.transform.SetParent(card.transform, false);
        var ctRT = content.AddComponent<RectTransform>();
        ctRT.anchorMin = Vector2.zero; ctRT.anchorMax = Vector2.one;
        ctRT.offsetMin = new Vector2(40f, sembunyikanTombol ? 40f : 120f);
        ctRT.offsetMax = new Vector2(-40f, -40f);
        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment       = TextAnchor.MiddleCenter;
        vlg.childControlWidth    = true;
        vlg.childControlHeight   = true;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing              = 12f;
        vlg.padding              = new RectOffset(8, 8, 8, 8);

        // Judul
        var titleTMP = BuatTeks(content.transform, "Judul", judul,
                                 ukuranJudul, warnaJudul, FontStyles.Bold);
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.textWrappingMode = TextWrappingModes.Normal;
        AddLE(titleTMP.gameObject, ukuranJudul * 1.8f);

        // Poin besar
        if (tampilkanPoin)
        {
            string teksPoin = string.Format(formatPoin, nilaiPoin);
            var poinTMP = BuatTeks(content.transform, "PoinBesar", teksPoin,
                                    ukuranPoin, warnaPoin, FontStyles.Bold);
            poinTMP.alignment = TextAlignmentOptions.Center;
            poinTMP.textWrappingMode = TextWrappingModes.NoWrap;
            // Glow outline kuning untuk efek emas
            var outline = poinTMP.gameObject.AddComponent<Outline>();
            outline.effectColor    = new Color(0.6f, 0.3f, 0.0f, 0.7f);
            outline.effectDistance = new Vector2(3f, -3f);
            AddLE(poinTMP.gameObject, ukuranPoin * 1.25f);
        }

        // Subtitel
        if (!string.IsNullOrEmpty(subtitel))
        {
            var subTMP = BuatTeks(content.transform, "Subtitel", subtitel,
                                   ukuranSubtitel, warnaSubtitel, FontStyles.Normal);
            subTMP.alignment = TextAlignmentOptions.Center;
            subTMP.textWrappingMode = TextWrappingModes.Normal;
            AddLE(subTMP.gameObject, ukuranSubtitel * 2.2f);
        }

        // ── Tombol Lanjut ────────────────────────────────────────────────
        if (!sembunyikanTombol)
        {
            var btnGO = new GameObject("BtnLanjut");
            btnGO.transform.SetParent(card.transform, false);
            var btnRT = btnGO.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.5f, 0f); btnRT.anchorMax = new Vector2(0.5f, 0f);
            btnRT.pivot     = new Vector2(0.5f, 0f);
            btnRT.sizeDelta = new Vector2(560f, 72f);
            btnRT.anchoredPosition = new Vector2(0f, 28f);

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
            var cb = btn.colors;
            cb.highlightedColor = new Color(1f, 1f, 1f, 1f);
            cb.pressedColor     = new Color(0.8f, 0.8f, 0.8f, 1f);
            btn.colors = cb;

            var btnTxt = BuatTeks(btnGO.transform, "Label", tombolLanjutTeks,
                                   tombolUkuranTeks, tombolTeksWarna, FontStyles.Bold);
            var btnTRT = btnTxt.rectTransform;
            btnTRT.anchorMin = Vector2.zero; btnTRT.anchorMax = Vector2.one;
            btnTRT.offsetMin = btnTRT.offsetMax = Vector2.zero;
            btnTxt.alignment = TextAlignmentOptions.Center;

            btn.onClick.AddListener(HandleLanjut);
        }

        // ── Animasi pop-in ───────────────────────────────────────────────
        cardRT.localScale = Vector3.one * skalaAwal;
        StartCoroutine(PopIn(cardRT));
    }

    void AddLE(GameObject go, float minHeight)
    {
        var le = go.AddComponent<LayoutElement>();
        le.minHeight       = minHeight;
        le.preferredHeight = minHeight;
        le.flexibleHeight  = 0f;
    }

    void HandleLanjut()
    {
        if (sfxKlikLanjut != null)
            AudioManager.Instance?.sfxSource?.PlayOneShot(sfxKlikLanjut);
        else
            AudioManager.Instance?.Click();

        Tutup();

        // 1) Panggil kartu edukasi selanjutnya (drag manual atau auto-find)
        EduCardDay1 kartu = kartuEduSelanjutnya;
        if (kartu == null && autoCariEduCard)
            kartu = FindFirstObjectByType<EduCardDay1>(FindObjectsInactive.Include);

        if (kartu != null)
        {
            Debug.Log("[SampaiSekolahPopup] Klik LANJUT → memanggil EduCardDay1.Tampilkan()");
            kartu.Tampilkan();
        }
        else
        {
            Debug.LogWarning("[SampaiSekolahPopup] Tidak ada EduCardDay1 di scene. " +
                "Tambahkan GameObject 'EduCardDay1' (Create Empty → Add Component EduCardDay1), " +
                "ATAU drag ke field 'kartuEduSelanjutnya'.");
        }

        // 2) Trigger event Inspector tambahan (opsional)
        onLanjut?.Invoke();
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
        tmp.text      = content;
        tmp.fontSize  = size;
        tmp.color     = color;
        tmp.fontStyle = style;
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

    // ══════════════════════════════════════════════════════════════════════
    // GIZMO — tampilkan garis trigger di Scene view
    // ══════════════════════════════════════════════════════════════════════
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.30f, 1f, 0.45f, 0.85f);
        Vector3 a = new Vector3(triggerX, -10f, 0f);
        Vector3 b = new Vector3(triggerX,  10f, 0f);
        Gizmos.DrawLine(a, b);
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(new Vector3(triggerX + 0.2f, 2f, 0f),
            $"Popup Sukses X={triggerX}  (+{nilaiPoin} poin)");
        #endif
    }
}
