using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// PathChoiceUI — panel pilihan jalur "Jalan Ramai vs Gang Sepi".
///
/// ═══════════════════════════════════════════════════════
/// MODE A — UI dibuat di Editor (DIREKOMENDASIKAN):
/// ═══════════════════════════════════════════════════════
///   1. Buat Canvas "PathChoiceCanvas" (Screen Space – Overlay, sortOrder 700).
///   2. Di dalam Canvas, buat struktur:
///
///      PathChoiceCanvas
///      └─ UIRoot                      ← RectTransform stretch penuh, SetActive false
///         ├─ Overlay                  ← Image hitam semi-transparan, stretch penuh
///         └─ Panel                    ← Image gelap, size ~75% × 55% layar, di tengah
///            ├─ Title (TMP)           ← judul kuning, bold
///            ├─ Body (TMP)            ← deskripsi putih
///            ├─ BtnSafe (Button)      ← tombol hijau + Label (TMP)
///            └─ BtnDanger (Button)    ← tombol merah + Label (TMP)
///
///   3. Drag referensi ke field di bawah header "── UI REFERENSI (Editor-Built) ──".
///   4. Drag Transform Rara ke "Player Transform".
///   5. Sambungkan onSafeChosen / onDangerChosen ke Day1Controller.
///
/// MODE B — Fallback Programatik (jika referensi tidak di-assign):
///   Jalankan saja — UI dibuat otomatis saat runtime (tidak bisa diedit di Editor).
/// ═══════════════════════════════════════════════════════
/// </summary>
public class PathChoiceUI : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — TRIGGER
    // ══════════════════════════════════════════════════════════════════════

    [Header("── TRIGGER ──")]
    [Tooltip("Drag Transform Rara ke sini")]
    public Transform playerTransform;
    [Tooltip("Jarak (unit) dari tiang agar panel muncul")]
    public float triggerDistance = 2.5f;
    [Tooltip("Hanya tampil sekali (true) atau setiap kali Rara lewat (false)")]
    public bool  triggerOnce = true;

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — UI REFERENSI (Editor-Built)
    // Isi bagian ini jika kamu membuat UI manual di Unity Editor.
    // Jika semua dibiarkan kosong, UI dibuat otomatis (Mode B).
    // ══════════════════════════════════════════════════════════════════════

    [Header("── UI REFERENSI (Editor-Built) ──")]
    [Tooltip("GameObject induk yang berisi Overlay + Panel. Di-hide/show saat trigger.")]
    public GameObject uiRootRef;

    [Tooltip("Image overlay gelap (boleh dikosongkan)")]
    public Image overlayImageRef;

    [Tooltip("GameObject panel utama (Image latar panel)")]
    public GameObject panelRootRef;

    [Tooltip("TextMeshProUGUI judul panel (contoh: '⚠ ADA DUA JALUR!')")]
    public TextMeshProUGUI titleTMPRef;

    [Tooltip("TextMeshProUGUI deskripsi / isi panel")]
    public TextMeshProUGUI bodyTMPRef;

    [Tooltip("Button Jalan Ramai (hijau)")]
    public Button btnSafeRef;

    [Tooltip("Label TMP di dalam BtnSafe")]
    public TextMeshProUGUI btnSafeLabelRef;

    [Tooltip("Button Gang Sepi (merah)")]
    public Button btnDangerRef;

    [Tooltip("Label TMP di dalam BtnDanger")]
    public TextMeshProUGUI btnDangerLabelRef;

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — KONTEN & WARNA (berlaku di kedua mode)
    // ══════════════════════════════════════════════════════════════════════

    [Header("── KONTEN TEKS ──")]
    public string titleText   = "⚠  ADA DUA JALUR!";
    [TextArea(2, 4)]
    public string bodyText    = "Rara harus pilih jalur ke sekolah.\nMana yang menurutmu lebih aman buat Rara?";
    public string safeLabel   = "🏛  Jalan Ramai\n(aman, banyak orang)";
    public string dangerLabel = "🔴  Gang Sepi\n(lebih cepat, tapi... bahaya!)";

    [Header("── WARNA (Mode B / Fallback Programatik) ──")]
    [Tooltip("Warna latar panel (hanya berlaku jika UI dibuat otomatis)")]
    public Color  panelBgColor   = new Color(0.22f, 0.03f, 0.03f, 0.96f);
    public Color  borderColor    = new Color(1f, 0.85f, 0.1f, 1f);
    public Color  titleColor     = new Color(1f, 0.85f, 0.1f, 1f);
    public Color  bodyColor      = Color.white;
    public Color  safeColor      = new Color(0.15f, 0.60f, 0.20f, 1f);
    public Color  dangerColor    = new Color(0.75f, 0.20f, 0.15f, 1f);
    public Color  btnTextColor   = Color.white;
    [Range(0.3f, 1f)]
    public float  panelWidthRatio  = 0.75f;
    [Range(0.3f, 0.9f)]
    public float  panelHeightRatio = 0.55f;

    [Header("── FONT (opsional) ──")]
    public TMP_FontAsset fontAsset;
    [Tooltip("Font khusus untuk judul. Kosongkan untuk pakai fontAsset di atas.")]
    public TMP_FontAsset titleFontAsset;
    [Tooltip("Font khusus untuk body. Kosongkan untuk pakai fontAsset di atas.")]
    public TMP_FontAsset bodyFontAsset;
    [Tooltip("Font khusus untuk label tombol. Kosongkan untuk pakai fontAsset di atas.")]
    public TMP_FontAsset buttonFontAsset;

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — KUSTOMISASI TEKS (size, style, alignment, outline)
    // Berlaku di kedua mode. Centang `overrideTextStyle` untuk aktifkan.
    // ══════════════════════════════════════════════════════════════════════

    [Header("── KUSTOMISASI TEKS (centang untuk override) ──")]
    [Tooltip("Aktifkan agar pengaturan style teks di bawah diterapkan.")]
    public bool overrideTextStyle = false;

    [Header("   Judul")]
    public float            titleFontSize        = 52f;
    public FontStyles       titleFontStyle       = FontStyles.Bold;
    public TextAlignmentOptions titleAlignment   = TextAlignmentOptions.Center;
    [Range(-50f, 100f)] public float titleCharacterSpacing = 0f;
    [Range(-50f, 100f)] public float titleLineSpacing      = 0f;
    public bool             titleUseOutline      = false;
    public Color            titleOutlineColor    = Color.black;
    [Range(0f, 1f)] public float titleOutlineWidth = 0.2f;

    [Header("   Body")]
    public float            bodyFontSize         = 34f;
    public FontStyles       bodyFontStyle        = FontStyles.Normal;
    public TextAlignmentOptions bodyAlignment    = TextAlignmentOptions.Center;
    [Range(-50f, 100f)] public float bodyCharacterSpacing = 0f;
    [Range(-50f, 100f)] public float bodyLineSpacing      = 0f;
    public bool             bodyUseOutline       = false;
    public Color            bodyOutlineColor     = Color.black;
    [Range(0f, 1f)] public float bodyOutlineWidth = 0.2f;

    [Header("   Label Tombol")]
    public float            buttonFontSize       = 42f;
    public FontStyles       buttonFontStyle      = FontStyles.Bold;
    public TextAlignmentOptions buttonAlignment  = TextAlignmentOptions.Center;
    public bool             buttonUseOutline     = false;
    public Color            buttonOutlineColor   = Color.black;
    [Range(0f, 1f)] public float buttonOutlineWidth = 0.2f;

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — SPRITES (upload custom art untuk panel & tombol)
    // Sprite di sini akan menggantikan warna solid. Berlaku di kedua mode.
    // ══════════════════════════════════════════════════════════════════════

    [Header("── SPRITES (opsional, drag image di sini) ──")]
    [Tooltip("Sprite latar panel utama. Kosongkan untuk pakai warna solid panelBgColor.")]
    public Sprite panelBgSprite;

    [Tooltip("Sprite tombol Jalan Ramai. Kosongkan untuk pakai warna solid safeColor.")]
    public Sprite safeButtonSprite;

    [Tooltip("Sprite tombol Gang Sepi. Kosongkan untuk pakai warna solid dangerColor.")]
    public Sprite dangerButtonSprite;

    [Tooltip("Sprite overlay gelap (opsional). Kosongkan untuk pakai warna hitam transparan default.")]
    public Sprite overlayBgSprite;

    [Tooltip("Image Type untuk semua sprite (gunakan Sliced jika sprite punya 9-slice border).")]
    public Image.Type spriteImageType = Image.Type.Sliced;

    [Tooltip("Jika true, warna tint tetap diterapkan ke sprite. Jika false, sprite tampil apa adanya (warna putih).")]
    public bool tintSpriteWithColor = false;

    [Header("   Ukuran Sprite (centang untuk override)")]
    [Tooltip("Aktifkan agar ukuran sprite di bawah diterapkan ke panel/overlay/tombol.")]
    public bool overrideSpriteSize = false;

    [Tooltip("Ukuran panel utama (lebar × tinggi, px). Anchor di-set ke center otomatis.")]
    public Vector2 panelSize          = new Vector2(900f, 500f);
    [Tooltip("Posisi panel (px, relatif pusat layar). 0,0 = tengah layar.")]
    public Vector2 panelAnchoredPos   = Vector2.zero;

    [Tooltip("Ukuran overlay gelap (lebar × tinggi, px). Set besar agar menutup layar penuh.")]
    public Vector2 overlaySize        = new Vector2(1920f, 1080f);
    [Tooltip("Jika true, overlay tetap stretch fullscreen (mengabaikan overlaySize).")]
    public bool overlayStretchFullscreen = true;

    [Tooltip("Skala uniform tambahan untuk sprite tombol Safe (1 = ukuran asli).")]
    [Range(0.1f, 5f)] public float safeButtonScale   = 1f;
    [Tooltip("Skala uniform tambahan untuk sprite tombol Danger (1 = ukuran asli).")]
    [Range(0.1f, 5f)] public float dangerButtonScale = 1f;
    [Tooltip("Skala uniform tambahan untuk sprite panel (1 = ukuran asli).")]
    [Range(0.1f, 5f)] public float panelScale        = 1f;

    [Tooltip("Jika ON: skala tombol, posisi tombol, ukuran tombol, dan ukuran font\n" +
             "otomatis IKUT 'panelScale'. Cocok kalau kamu cuma mau atur 1 slider (panelScale)\n" +
             "untuk membesarkan SELURUH panel + isinya secara proporsional.\n" +
             "OFF = tiap field berdiri sendiri (mode lama).")]
    public bool scaleChildrenWithPanel = true;

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR — POSISI & UKURAN TOMBOL (override layout)
    // Centang `overrideButtonLayout` untuk memakai nilai di bawah ini.
    // Nilai dalam satuan piksel relatif terhadap pusat panel (anchor center).
    // ══════════════════════════════════════════════════════════════════════

    [Header("── POSISI & UKURAN TOMBOL (centang untuk override) ──")]
    [Tooltip("Aktifkan agar nilai posisi/ukuran di bawah diterapkan ke tombol di kedua mode.")]
    public bool overrideButtonLayout = false;

    [Tooltip("Posisi tombol Jalan Ramai (px, relatif pusat panel)")]
    public Vector2 safeButtonAnchoredPos = new Vector2(0f, 80f);
    [Tooltip("Ukuran tombol Jalan Ramai (lebar × tinggi, px)")]
    public Vector2 safeButtonSize        = new Vector2(700f, 120f);

    [Tooltip("Posisi tombol Gang Sepi (px, relatif pusat panel)")]
    public Vector2 dangerButtonAnchoredPos = new Vector2(0f, -80f);
    [Tooltip("Ukuran tombol Gang Sepi (lebar × tinggi, px)")]
    public Vector2 dangerButtonSize        = new Vector2(700f, 120f);

    [Tooltip("ON: ukuran RectTransform tombol otomatis = ukuran asli SPRITE\n" +
             "(sprite.rect / pixelsPerUnit). Cocok kalau sprite tombolmu sudah\n" +
             "punya bentuk/border, supaya kotak klik PAS dengan visual.\n" +
             "Nilai 'safeButtonSize' & 'dangerButtonSize' di atas akan DIABAIKAN saat ON.\n" +
             "OFF = pakai ukuran manual di atas.")]
    public bool matchButtonSizeToSprite = false;
    [Tooltip("Pengali tambahan saat matchButtonSizeToSprite ON (1 = ukuran sprite asli, 1.5 = 150%).")]
    [Range(0.1f, 5f)] public float buttonSpriteSizeMul = 1f;

    [Header("── LIVE EDIT (saat Play) ──")]
    [Tooltip("Jika true: setiap perubahan Inspector (teks, sprite, ukuran, posisi, style) langsung diterapkan saat Play.")]
    public bool liveEdit = true;

    [Tooltip("Jika true: paksa panel tetap terlihat saat Play (berguna untuk mengedit tata letak). Matikan saat selesai.")]
    public bool previewPanelInPlay = false;

    [Tooltip("Otomatis non-aktifkan LayoutGroup / ContentSizeFitter pada parent saat override layout. Wajib true jika UI memakai Vertical/Horizontal/Grid Layout.")]
    public bool autoDisableLayoutGroup = true;

    [Tooltip("Tampilkan log debug di Console saat layout/sprite di-apply.")]
    public bool debugLog = false;

    [Header("── MOBILE ──")]
    [Tooltip("Hormati Safe Area (notch/punch hole) di mobile. Matikan kalau panel ter-clamp di Game View kecil.")]
    public bool  useSafeArea       = false;
    [Tooltip("Minimum touch target untuk tombol (px). Default 96 untuk anak.")]
    public float minTouchTargetPx  = 96f;
    [Tooltip("Auto-batasi panel agar tidak melebihi layar. 1.0 = boleh sebesar layar penuh.")]
    [Range(0.5f, 1f)] public float maxPanelWidthScreenRatio  = 1f;
    [Range(0.5f, 1f)] public float maxPanelHeightScreenRatio = 1f;

    [Header("── KONSISTENSI UKURAN (Cross-Device) ──")]
    [Tooltip("WAJIB ON agar tampilan panel/tombol KONSISTEN di semua ukuran device.\n" +
             "Otomatis set CanvasScaler mengikuti 'canvasScaleMode' di bawah.")]
    public bool autoConfigCanvasScaler = true;

    public enum CanvasScaleMode
    {
        Proportional,       // Scale With Screen Size — panel/sprite skala mengikuti layar
        ConstantPixelSize,  // Sprite UI selalu ukuran piksel SAMA di semua device
        ConstantPhysicalSize // Selalu ukuran fisik sama (cm/inch) di semua device
    }

    [Tooltip("MODE skala canvas:\n" +
             " • Proportional (default) — panel & sprite SKALA ikut ukuran layar. Cocok kalau mau panel\n" +
             "   selalu ~47% lebar layar di HP kecil, tablet, & 4K.\n" +
             " • ConstantPixelSize — sprite/UI selalu UKURAN PIKSEL SAMA di semua device.\n" +
             "   Sprite tidak ikut membesar/mengecil saat ganti device. Pilih ini kalau mau\n" +
             "   asset pixel-art tampil tajam tanpa upscale/downscale.\n" +
             " • ConstantPhysicalSize — selalu ukuran fisik sama (mm/inch) berdasar DPI device.")]
    public CanvasScaleMode canvasScaleMode = CanvasScaleMode.Proportional;

    [Tooltip("Hanya untuk mode Proportional. Resolusi referensi CanvasScaler.\n" +
             "Default 1920x1080 = jika panelSize=900x500, panel akan ~47% lebar layar di SEMUA device.")]
    public Vector2 referenceResolution   = new Vector2(1920f, 1080f);
    [Tooltip("Hanya untuk mode Proportional. 0 = scale berdasarkan lebar, 1 = berdasarkan tinggi, 0.5 = seimbang.")]
    [Range(0f, 1f)] public float scalerMatchWidthOrHeight = 0.5f;

    [Header("── EVENTS — sambungkan ke Day1Controller ──")]
    [Tooltip("Dipanggil saat pemain memilih Jalan Ramai")]
    public UnityEngine.Events.UnityEvent onSafeChosen;
    [Tooltip("Dipanggil saat pemain memilih Gang Sepi")]
    public UnityEngine.Events.UnityEvent onDangerChosen;

    // ══════════════════════════════════════════════════════════════════════
    // INTERNAL
    // ══════════════════════════════════════════════════════════════════════

    private Canvas      canvas;
    private GameObject  panelRoot;
    private GameObject  uiRoot;
    private bool        triggered = false;
    private bool        shown     = false;

    // Apakah menggunakan referensi dari Editor (true) atau programatik (false)
    private bool        usingEditorRefs = false;

    // referensi komponen Rigidbody2D Rara untuk pause gerak
    private Rigidbody2D playerRb;

    // referensi komponen player Rara untuk bekukan gerak (velocity di-set tiap frame
    // oleh player.Update, jadi isKinematic saja tidak cukup — wajib set frozen).
    private player playerScript;

    // referensi PathEnvironment — dicari otomatis, tidak perlu di-assign manual
    private PathEnvironment pathEnv;

    // ══════════════════════════════════════════════════════════════════════
    void Start()
    {
        // Auto-find Rara jika belum di-assign
        if (playerTransform == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        if (playerTransform != null)
        {
            playerRb     = playerTransform.GetComponent<Rigidbody2D>();
            playerScript = playerTransform.GetComponent<player>();
        }

        // Auto-find PathEnvironment di scene
        pathEnv = FindFirstObjectByType<PathEnvironment>();
        if (pathEnv == null)
            Debug.LogWarning("[PathChoiceUI] PathEnvironment tidak ditemukan di scene — latar tidak akan berubah.");

        // Pilih mode: Editor-refs atau Programatik
        if (uiRootRef != null && panelRootRef != null && btnSafeRef != null && btnDangerRef != null)
        {
            usingEditorRefs = true;
            SetupEditorRefs();
        }
        else
        {
            usingEditorRefs = false;
            BuildUI();
        }

        // Pastikan EventSystem ada — tanpa ini tombol UI tidak akan merespons klik.
        EnsureEventSystem();

        // Auto-hookup ref UI yang masih kosong (panel, overlay, tombol) supaya
        // bagian "Ukuran Sprite" & "Posisi Tombol" langsung bekerja saat Play
        // walau user belum drag manual ke Inspector.
        AutoHookupMissingRefs();

        // Apply sekali di Start agar nilai Inspector langsung kelihatan tanpa menunggu trigger.
        ReapplyAllCustomization();
    }

    // Cari & isi field ref yang masih null. Aman dipanggil berulang (idempotent).
    void AutoHookupMissingRefs()
    {
        // Panel — pakai auto-detect berdasarkan luas Image terbesar.
        if (panelRootRef == null)
        {
            var rt = AutoFindPanelRectTransform();
            if (rt != null)
            {
                panelRootRef = rt.gameObject;
                Debug.Log("[PathChoiceUI] Auto-hookup panelRootRef → " + rt.name);
            }
        }

        // Overlay — cari Image stretch fullscreen bernama "Overlay" / "Dim" / sejenis.
        if (overlayImageRef == null)
        {
            Transform searchRoot = uiRootRef != null ? uiRootRef.transform
                                  : (panelRootRef != null ? panelRootRef.transform.root : null);
            if (searchRoot != null)
            {
                var imgs = searchRoot.GetComponentsInChildren<Image>(true);
                foreach (var img in imgs)
                {
                    string n = img.name.ToLower();
                    if (n.Contains("overlay") || n.Contains("dim") || n.Contains("backdrop"))
                    {
                        overlayImageRef = img;
                        Debug.Log("[PathChoiceUI] Auto-hookup overlayImageRef → " + img.name);
                        break;
                    }
                }
            }
        }

        // Tombol Safe/Danger — cari child by name di hierarki panel.
        if (btnSafeRef == null)
        {
            var t = FindRT(null, "BtnSafe");
            if (t != null) btnSafeRef = t.GetComponent<Button>();
            if (btnSafeRef != null) Debug.Log("[PathChoiceUI] Auto-hookup btnSafeRef → " + btnSafeRef.name);
        }
        if (btnDangerRef == null)
        {
            var t = FindRT(null, "BtnDanger");
            if (t != null) btnDangerRef = t.GetComponent<Button>();
            if (btnDangerRef != null) Debug.Log("[PathChoiceUI] Auto-hookup btnDangerRef → " + btnDangerRef.name);
        }

        // Daftarkan listener tombol kalau belum (di Mode A SetupEditorRefs sudah handle, ini fallback).
        if (btnSafeRef != null)
        {
            btnSafeRef.onClick.RemoveListener(OnSafeButton);
            btnSafeRef.onClick.AddListener(OnSafeButton);
        }
        if (btnDangerRef != null)
        {
            btnDangerRef.onClick.RemoveListener(OnDangerButton);
            btnDangerRef.onClick.AddListener(OnDangerButton);
        }
    }

    // Auto-buat EventSystem jika scene tidak punya satupun.
    void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        var es = FindFirstObjectByType<EventSystem>();
        if (es != null) return;
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
        Debug.LogWarning("[PathChoiceUI] EventSystem tidak ditemukan di scene — dibuat otomatis. Tombol UI sekarang bisa diklik.");
    }

    void Update()
    {
        if (Application.isPlaying)
        {
            ApplySafeAreaIfNeeded();

            // Preview panel — independent dari liveEdit, paksa panel terlihat.
            if (previewPanelInPlay)
            {
                ForceShowPanelForPreview();
            }

            // Live edit — terapkan perubahan Inspector tiap frame.
            if (liveEdit)
            {
                WarnIfLiveEditMisconfigured();
                ReapplyAllCustomization();
            }
        }

        if (shown) return;
        if (triggerOnce && triggered) return;
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist <= triggerDistance)
        {
            triggered = true;
            ShowPanel();
        }
    }

    // Cetak peringatan SEKALI saja kalau user nyalakan liveEdit tapi lupa
    // centang override toggle — sehingga edit Inspector seperti panelSize/buttonSize/fontSize
    // tidak akan terlihat efeknya.
    bool _warnedMisconfig = false;
    void WarnIfLiveEditMisconfigured()
    {
        if (_warnedMisconfig) return;
        if (overrideSpriteSize && overrideButtonLayout && overrideTextStyle) return;

        _warnedMisconfig = true;
        var missing = new System.Text.StringBuilder();
        if (!overrideSpriteSize)   missing.Append("\n  • overrideSpriteSize  → panelSize/panelScale/overlaySize/buttonScale");
        if (!overrideButtonLayout) missing.Append("\n  • overrideButtonLayout → safeButtonSize/dangerButtonSize/posisi/matchButtonSizeToSprite");
        if (!overrideTextStyle)    missing.Append("\n  • overrideTextStyle    → titleFontSize/bodyFontSize/buttonFontSize/style/alignment");

        Debug.LogWarning(
            "[PathChoiceUI] liveEdit AKTIF tapi beberapa toggle override BELUM dicentang. " +
            "Field-field di bawah TIDAK akan berubah saat Inspector diedit:" + missing +
            "\n\n→ Klik kanan pada komponen PathChoiceUI → '▶ Aktifkan Semua Override' untuk fix cepat.",
            this);
    }

    [ContextMenu("▶ Aktifkan Semua Override (Live Edit)")]
    public void EnableAllOverrides()
    {
        overrideSpriteSize   = true;
        overrideButtonLayout = true;
        overrideTextStyle    = true;
        liveEdit             = true;
        _warnedMisconfig     = false; // reset — supaya warning bisa muncul lagi kalau user matikan
        if (Application.isPlaying) ReapplyAllCustomization();
        Debug.Log("[PathChoiceUI] Semua override toggle di-AKTIFKAN. Sekarang setiap perubahan Inspector saat Play akan langsung terlihat.");
    }

    // Aktifkan panel untuk preview tanpa harus kena trigger Rara.
    void ForceShowPanelForPreview()
    {
        if (usingEditorRefs)
        {
            if (uiRootRef != null && !uiRootRef.activeSelf)       uiRootRef.SetActive(true);
            if (panelRootRef != null && !panelRootRef.activeSelf) panelRootRef.SetActive(true);
        }
        else
        {
            if (uiRoot != null && !uiRoot.activeSelf)       uiRoot.SetActive(true);
            if (panelRoot != null && !panelRoot.activeSelf) panelRoot.SetActive(true);
        }
    }

    // ══════ Safe Area (notch handling) ══════
    // BUKAN mengubah field panelSize (yang diatur user di Inspector), tapi hanya membatasi
    // ukuran AKTUAL saat diterapkan ke RectTransform di ApplySpriteSizes (via GetSafePanelSize).
    Rect lastSafeArea = Rect.zero;
    void ApplySafeAreaIfNeeded()
    {
        if (!useSafeArea) return;
        Rect sa = Screen.safeArea;
        if (sa == lastSafeArea) return;
        lastSafeArea = sa;
        // Tidak perlu lakukan apa-apa di sini — clamping dilakukan saat apply di GetSafePanelSize().
    }

    // Hitung ukuran panel final, dibatasi safe area jika useSafeArea aktif.
    // Field panelSize TIDAK diubah — hanya return value yang di-clamp.
    Vector2 GetSafePanelSize()
    {
        if (!useSafeArea) return panelSize;
        Rect sa = Screen.safeArea;
        float maxW = sa.width  * maxPanelWidthScreenRatio;
        float maxH = sa.height * maxPanelHeightScreenRatio;
        return new Vector2(
            Mathf.Min(panelSize.x, maxW),
            Mathf.Min(panelSize.y, maxH));
    }

    // ──────────── CONTEXT MENU UNTUK TEST SAAT PLAY ────────────
    [ContextMenu("▶ Test: Tampilkan Panel Sekarang")]
    public void TestShowPanel()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[PathChoiceUI] Tekan Play dulu baru pakai test ini.");
            return;
        }
        // Pastikan setup sudah jalan
        if (!usingEditorRefs && panelRoot == null)
        {
            Debug.LogWarning("[PathChoiceUI] Panel belum dibuat. Tunggu 1 frame setelah Play.");
            return;
        }
        ShowPanel();
        ReapplyAllCustomization();
        Debug.Log("[PathChoiceUI] Panel di-tampilkan via ContextMenu.");
    }

    [ContextMenu("▶ Test: Sembunyikan Panel")]
    public void TestHidePanel()
    {
        HidePanel();
        shown = false;
        triggered = false;
    }

    /// <summary>
    /// Atur otomatis ukuran & posisi tombol agar masuk ke dalam panel.
    /// Berguna kalau angka tombol jauh lebih besar dari panel sehingga tidak terlihat.
    /// Hasil: 2 tombol horizontal di bawah panel, masing-masing ~45% lebar panel.
    /// </summary>
    [ContextMenu("▶ Auto-Fit: Pasang Tombol Di Dalam Panel")]
    public void AutoFitButtonsToPanel()
    {
        // Hitung ukuran panel — pakai panelSize (dari override) atau dari RectTransform aktual.
        Vector2 ps = panelSize;
        RectTransform panelRT = null;
        if (panelRootRef != null)   panelRT = panelRootRef.GetComponent<RectTransform>();
        else if (panelRoot != null) panelRT = panelRoot.GetComponent<RectTransform>();
        if (panelRT != null && panelRT.sizeDelta.x > 50f && panelRT.sizeDelta.y > 50f)
            ps = panelRT.sizeDelta;

        // 2 tombol horizontal di bagian bawah panel, padding 8%.
        float bw = ps.x * 0.42f;      // lebar tiap tombol
        float bh = ps.y * 0.18f;      // tinggi tiap tombol
        float gap = ps.x * 0.04f;      // jarak antar tombol
        float yPos = -ps.y * 0.32f;    // posisi vertikal (di bawah tengah panel)

        overrideButtonLayout    = true;
        safeButtonAnchoredPos   = new Vector2(-(bw * 0.5f + gap * 0.5f), yPos);
        safeButtonSize          = new Vector2(bw, bh);
        dangerButtonAnchoredPos = new Vector2( (bw * 0.5f + gap * 0.5f), yPos);
        dangerButtonSize        = new Vector2(bw, bh);

        Debug.Log("[PathChoiceUI] Auto-Fit selesai. Panel=" + ps + "  Safe pos=" + safeButtonAnchoredPos + " size=" + safeButtonSize + "  Danger pos=" + dangerButtonAnchoredPos + " size=" + dangerButtonSize);

        if (Application.isPlaying)
            ReapplyAllCustomization();
    }

    /// <summary>
    /// Sembunyikan latar Image kedua tombol (alpha=0) tapi tetap menerima klik.
    /// Berguna kalau ribbon tombol sudah "baked" di sprite panel — tombol UI cukup jadi area klik invisible.
    /// </summary>
    [ContextMenu("▶ Buat Tombol Transparan (klik tetap aktif)")]
    public void MakeButtonsInvisibleButClickable()
    {
        SetButtonAlpha(btnSafeRef,   0f);
        SetButtonAlpha(btnDangerRef, 0f);
        Debug.Log("[PathChoiceUI] Tombol di-set transparan. Raycast Target tetap ON — klik tetap berfungsi.");
    }

    /// <summary>
    /// Kembalikan latar Image tombol jadi terlihat (alpha=1).
    /// </summary>
    [ContextMenu("▶ Tampilkan Lagi Latar Tombol")]
    public void ShowButtonBackgrounds()
    {
        SetButtonAlpha(btnSafeRef,   1f);
        SetButtonAlpha(btnDangerRef, 1f);
    }

    void SetButtonAlpha(Button b, float a)
    {
        if (b == null) return;
        var img = b.GetComponent<Image>();
        if (img == null) return;
        var c = img.color; c.a = a; img.color = c;
        img.raycastTarget = true; // pastikan tetap menerima klik
    }

    /// <summary>
    /// Diagnostic khusus untuk bagian "Ukuran Sprite".
    /// Print ukuran AKTUAL panel/overlay setelah ApplySpriteSizes dipanggil.
    /// Kalau RectTransform-nya tidak berubah → ada parent dengan LayoutGroup
    /// atau panelRootRef menunjuk ke GameObject yang salah.
    /// </summary>
    [ContextMenu("▶ Diagnostic: Cek Ukuran Sprite Aktual")]
    public void DiagnosticSpriteSize()
    {
        Debug.Log("════════ DIAGNOSTIC UKURAN SPRITE ════════");
        Debug.Log("overrideSpriteSize = " + overrideSpriteSize + "  (HARUS true agar nilai diterapkan)");
        Debug.Log("Inspector → panelSize=" + panelSize + "  panelAnchoredPos=" + panelAnchoredPos + "  panelScale=" + panelScale);
        Debug.Log("Inspector → overlaySize=" + overlaySize + "  overlayStretch=" + overlayStretchFullscreen);
        Debug.Log("Inspector → safeButtonScale=" + safeButtonScale + "  dangerButtonScale=" + dangerButtonScale);

        // Panel
        RectTransform panelRT = null;
        string panelSource = "NULL";
        if (panelRootRef != null)   { panelRT = panelRootRef.GetComponent<RectTransform>(); panelSource = "panelRootRef → " + panelRootRef.name; }
        else if (panelRoot != null) { panelRT = panelRoot.GetComponent<RectTransform>();    panelSource = "panelRoot (Mode B) → " + panelRoot.name; }
        Debug.Log("─── PANEL ───");
        Debug.Log("  Source        : " + panelSource);
        if (panelRT != null)
        {
            Debug.Log("  AKTUAL pos    = " + panelRT.anchoredPosition);
            Debug.Log("  AKTUAL size   = " + panelRT.sizeDelta);
            Debug.Log("  AKTUAL scale  = " + panelRT.localScale.x);
            Debug.Log("  Parent name   = " + (panelRT.parent != null ? panelRT.parent.name : "(none)"));
            CheckParentLayout(panelRT);
            // Cek anchor — kalau stretch, sizeDelta jadi offset, bukan ukuran absolut!
            if (panelRT.anchorMin != panelRT.anchorMax)
                Debug.LogWarning("  ⚠ Anchor STRETCH (" + panelRT.anchorMin + " → " + panelRT.anchorMax + ") — sizeDelta jadi offset, bukan ukuran piksel. ApplySpriteSizes akan set anchor ke center.");
        }
        else Debug.LogWarning("  Panel RT NULL — drag Panel Root Ref di Inspector!");

        // Overlay
        Debug.Log("─── OVERLAY ───");
        RectTransform ovRT = null;
        string ovSource = "NULL";
        if (overlayImageRef != null) { ovRT = overlayImageRef.GetComponent<RectTransform>(); ovSource = "overlayImageRef → " + overlayImageRef.name; }
        else
        {
            var t = FindRT(null, "Overlay");
            if (t != null) { ovRT = t; ovSource = "auto-found 'Overlay' child"; }
        }
        Debug.Log("  Source        : " + ovSource);
        if (ovRT != null)
        {
            Debug.Log("  AKTUAL size   = " + ovRT.sizeDelta);
        }

        // Buttons (untuk scale)
        Debug.Log("─── TOMBOL (scale) ───");
        var safeRT   = FindRT(btnSafeRef   != null ? btnSafeRef.transform   : null, "BtnSafe");
        var dangerRT = FindRT(btnDangerRef != null ? btnDangerRef.transform : null, "BtnDanger");
        if (safeRT   != null) Debug.Log("  Safe   AKTUAL scale = " + safeRT.localScale.x);
        if (dangerRT != null) Debug.Log("  Danger AKTUAL scale = " + dangerRT.localScale.x);
        Debug.Log("════════════════════════════════════════════");
    }

    /// <summary>
    /// Auto-detect: cari Image dengan sprite TERBESAR di hierarki Canvas, anggap itu panel.
    /// Lalu set panelRootRef otomatis. Berguna kalau bingung GameObject mana yang sebenarnya panel.
    /// </summary>
    [ContextMenu("▶ Auto-Detect: Cari & Pasang Panel Root Ref")]
    public void AutoDetectPanelRoot()
    {
        Transform searchRoot = null;
        if (uiRootRef != null) searchRoot = uiRootRef.transform.root;
        else if (panelRootRef != null) searchRoot = panelRootRef.transform.root;
        else
        {
            var anyCanvas = FindFirstObjectByType<Canvas>();
            if (anyCanvas != null) searchRoot = anyCanvas.transform;
        }
        if (searchRoot == null) { Debug.LogWarning("[PathChoiceUI] Tidak ada Canvas ditemukan."); return; }

        Image best = null;
        float bestArea = 0f;
        var imgs = searchRoot.GetComponentsInChildren<Image>(true);
        foreach (var img in imgs)
        {
            if (img.sprite == null) continue;
            var rt = img.rectTransform;
            float a = Mathf.Abs(rt.rect.width * rt.rect.height);
            if (a > bestArea) { bestArea = a; best = img; }
        }
        if (best == null) { Debug.LogWarning("[PathChoiceUI] Tidak ada Image bersprite ditemukan."); return; }

        panelRootRef = best.gameObject;
        Debug.Log("[PathChoiceUI] panelRootRef di-set ke: " + best.name + "  (area=" + bestArea.ToString("F0") + ", parent=" + (best.transform.parent != null ? best.transform.parent.name : "(none)") + ")");
        if (Application.isPlaying) ReapplyAllCustomization();
    }

    /// <summary>
    /// Tekan untuk paksa apply Ukuran Sprite SEKARANG dengan log verbose tiap langkah.
    /// </summary>
    [ContextMenu("▶ Force Apply: Ukuran Sprite Sekarang")]
    public void ForceApplySpriteSizesNow()
    {
        if (!overrideSpriteSize)
        {
            overrideSpriteSize = true;
            Debug.LogWarning("[PathChoiceUI] overrideSpriteSize otomatis dicentang.");
        }
        bool prev = debugLog;
        debugLog = true;
        ApplySpriteSizes();
        debugLog = prev;
        DiagnosticSpriteSize();
    }

    /// <summary>
    /// List SEMUA Image bersprite di Canvas — terurut dari TERBESAR.
    /// Pakai ini untuk melihat semua "kandidat panel" dan tahu mana yang sedang dipakai script.
    /// </summary>
    [ContextMenu("▶ Diagnostic: List Semua Image Bersprite")]
    public void DiagnosticListAllSprites()
    {
        Transform searchRoot = null;
        if (uiRootRef    != null) searchRoot = uiRootRef.transform;
        else if (panelRootRef != null) searchRoot = panelRootRef.transform.root;
        else
        {
            var anyCanvas = FindFirstObjectByType<Canvas>();
            if (anyCanvas != null) searchRoot = anyCanvas.transform;
        }
        if (searchRoot == null) { Debug.LogWarning("[PathChoiceUI] Tidak ada Canvas ditemukan."); return; }

        var imgs = searchRoot.GetComponentsInChildren<Image>(true);
        var list = new System.Collections.Generic.List<(Image img, float area)>();
        foreach (var img in imgs)
        {
            if (img.sprite == null) continue;
            var rt = img.rectTransform;
            float a = Mathf.Abs(rt.rect.width * rt.rect.height);
            list.Add((img, a));
        }
        list.Sort((a, b) => b.area.CompareTo(a.area));

        Debug.Log("════════ DAFTAR SEMUA IMAGE BERSPRITE (root: " + searchRoot.name + ") ════════");
        Debug.Log("Total ditemukan: " + list.Count);
        int currentPanelId = panelRootRef != null ? panelRootRef.GetInstanceID() : 0;
        for (int i = 0; i < list.Count; i++)
        {
            var (img, area) = list[i];
            var rt = img.rectTransform;
            bool isTarget = img.gameObject.GetInstanceID() == currentPanelId;
            string marker = isTarget ? " ★ ← PANEL ROOT REF saat ini" : "";
            string stretch = (rt.anchorMin == Vector2.zero && rt.anchorMax == Vector2.one) ? " [STRETCH]" : "";
            Debug.Log(string.Format("  #{0} {1}{2}  size={3}  pos={4}  sprite='{5}'  area={6:F0}  parent='{7}'{8}",
                i, img.name, stretch, rt.sizeDelta, rt.anchoredPosition,
                img.sprite.name, area,
                rt.parent != null ? rt.parent.name : "(none)", marker));
        }
        Debug.Log("Cara pakai: lihat # yang sebenarnya panel ramai/bingkai kayu yang kamu lihat di game.");
        Debug.Log("Lalu Drag GameObject itu ke field 'Panel Root Ref' di Inspector PathChoiceUI.");
        Debug.Log("════════════════════════════════════════════════════════════════════");
    }

    [ContextMenu("▶ Diagnostic: Lihat Apa Yang Ditemukan")]
    public void DiagnosticDump()
    {
        string modeStr      = usingEditorRefs ? "A (Editor-Built)" : "B (Programatik)";
        string uiRootStr    = uiRootRef    != null ? uiRootRef.name    : "NULL";
        string panelRootStr = panelRootRef != null ? panelRootRef.name : "NULL";
        string btnSafeStr   = btnSafeRef   != null ? btnSafeRef.name   : "NULL";
        string btnDangerStr = btnDangerRef != null ? btnDangerRef.name : "NULL";

        Debug.Log("════════ PathChoiceUI DIAGNOSTIC ════════");
        Debug.Log("Mode             : " + modeStr);
        Debug.Log("overrideButtonLayout = " + overrideButtonLayout + "  | overrideSpriteSize = " + overrideSpriteSize + "  | overrideTextStyle = " + overrideTextStyle);
        Debug.Log("liveEdit          = " + liveEdit);
        Debug.Log("---- REFS ----");
        Debug.Log("  uiRootRef       = " + uiRootStr);
        Debug.Log("  panelRootRef    = " + panelRootStr);
        Debug.Log("  btnSafeRef      = " + btnSafeStr);
        Debug.Log("  btnDangerRef    = " + btnDangerStr);
        Debug.Log("---- HASIL PENCARIAN (FindRT) ----");
        var rtSafe   = FindRT(btnSafeRef   != null ? btnSafeRef.transform   : null, "BtnSafe");
        var rtDanger = FindRT(btnDangerRef != null ? btnDangerRef.transform : null, "BtnDanger");
        string safePathStr   = rtSafe   != null ? GetPath(rtSafe)   : "TIDAK KETEMU";
        string dangerPathStr = rtDanger != null ? GetPath(rtDanger) : "TIDAK KETEMU";
        Debug.Log("  Safe   tombol   = " + safePathStr);
        Debug.Log("  Danger tombol   = " + dangerPathStr);
        if (rtSafe != null)
        {
            Debug.Log($"  Safe   sekarang : anchoredPos={rtSafe.anchoredPosition} size={rtSafe.sizeDelta} scale={rtSafe.localScale.x}");
            CheckParentLayout(rtSafe);
        }
        if (rtDanger != null)
        {
            Debug.Log($"  Danger sekarang : anchoredPos={rtDanger.anchoredPosition} size={rtDanger.sizeDelta} scale={rtDanger.localScale.x}");
            CheckParentLayout(rtDanger);
        }
        Debug.Log("---- INSPECTOR VALUES ----");
        Debug.Log("  safeButtonAnchoredPos   = " + safeButtonAnchoredPos);
        Debug.Log("  safeButtonSize          = " + safeButtonSize);
        Debug.Log("  dangerButtonAnchoredPos = " + dangerButtonAnchoredPos);
        Debug.Log("  dangerButtonSize        = " + dangerButtonSize);
        Debug.Log("---- KLIK & RAYCAST ----");
        string esStatus = EventSystem.current != null ? "OK (" + EventSystem.current.name + ")" : "NULL ⚠ tombol tak akan merespons!";
        Debug.Log("  EventSystem.current     = " + esStatus);
        if (btnSafeRef != null)
        {
            var img = btnSafeRef.GetComponent<Image>();
            string rt = img != null ? img.raycastTarget.ToString() : "NO IMAGE";
            Debug.Log("  Safe   button interactable=" + btnSafeRef.interactable + " raycastTarget=" + rt + " siblingIndex=" + btnSafeRef.transform.GetSiblingIndex());
        }
        if (btnDangerRef != null)
        {
            var img = btnDangerRef.GetComponent<Image>();
            string rt = img != null ? img.raycastTarget.ToString() : "NO IMAGE";
            Debug.Log("  Danger button interactable=" + btnDangerRef.interactable + " raycastTarget=" + rt + " siblingIndex=" + btnDangerRef.transform.GetSiblingIndex());
        }
        if (overlayImageRef != null)
        {
            Debug.Log("  Overlay raycastTarget=" + overlayImageRef.raycastTarget + " siblingIndex=" + overlayImageRef.transform.GetSiblingIndex() + "  (overlay HARUS sibling lebih awal dari Panel)");
        }
        Debug.Log("════════════════════════════════════════");
    }

    string GetPath(Transform t)
    {
        if (t == null) return "(null)";
        string p = t.name;
        while (t.parent != null) { t = t.parent; p = t.name + "/" + p; }
        return p;
    }

    // Panel harus menjadi sibling terakhir agar tombolnya berada di atas overlay (raycast lolos).
    void EnsurePanelOnTop()
    {
        Transform pr = panelRootRef != null ? panelRootRef.transform : (panelRoot != null ? panelRoot.transform : null);
        if (pr != null) pr.SetAsLastSibling();
    }

    // Pastikan overlay tidak menutupi panel — kalau urutan salah, klik tombol akan dimakan overlay.
    void EnsureOverlayDoesNotBlock()
    {
        Image ov = overlayImageRef;
        if (ov == null)
        {
            var t = FindRT(null, "Overlay");
            if (t != null) ov = t.GetComponent<Image>();
        }
        if (ov == null) return;
        // Pindahkan overlay ke sibling pertama agar panel selalu render di atasnya.
        ov.transform.SetAsFirstSibling();
    }

    void CheckParentLayout(RectTransform child)
    {
        var p = child.parent;
        if (p == null) return;
        var lg = p.GetComponent<LayoutGroup>();
        var fit = p.GetComponent<ContentSizeFitter>();
        if (lg != null)
            Debug.LogWarning($"    ⚠ Parent '{p.name}' punya {lg.GetType().Name} (enabled={lg.enabled}) — bisa menimpa posisi/ukuran!");
        if (fit != null)
            Debug.LogWarning($"    ⚠ Parent '{p.name}' punya ContentSizeFitter (enabled={fit.enabled}) — bisa menimpa ukuran!");
        var le = child.GetComponent<LayoutElement>();
        if (le != null && !le.ignoreLayout)
            Debug.LogWarning($"    ⚠ Child '{child.name}' punya LayoutElement (ignoreLayout={le.ignoreLayout}) — bisa mengunci ukuran!");
    }

    // ══════════════════════════════════════════════════════════════════════
    // PANEL LOGIC
    // ══════════════════════════════════════════════════════════════════════

    void ShowPanel()
    {
        shown = true;

        if (usingEditorRefs)
        {
            uiRootRef.SetActive(true);
            panelRootRef.SetActive(true);
        }
        else
        {
            uiRoot.SetActive(true);
            panelRoot.SetActive(true);
        }

        // Pastikan panel berada di atas overlay (sibling terakhir → render paling atas → klik diterima).
        EnsurePanelOnTop();
        EnsureOverlayDoesNotBlock();

        // Bekukan Rara agar tidak jalan terus.
        // player.Update menulis ulang rb.velocity tiap frame, jadi set frozen wajib.
        if (playerScript != null) playerScript.frozen = true;
        if (playerRb != null)
        {
            playerRb.velocity = Vector2.zero;
            playerRb.isKinematic = true;
        }
    }

    void OnSafeButton()
    {
        if (GameState.Instance != null)
            GameState.Instance.pathChoice = "safe";

        HidePanel();

        // Aktifkan tampilan Jalan Ramai langsung dari sini
        pathEnv?.AktifkanJalanRamai();

        onSafeChosen?.Invoke();
        Debug.Log("[PathChoice] Dipilih: Jalan Ramai (safe)");
    }

    void OnDangerButton()
    {
        if (GameState.Instance != null)
            GameState.Instance.pathChoice = "dangerous";

        HidePanel();

        // Aktifkan tampilan Gang Sepi langsung dari sini
        pathEnv?.AktifkanGangSepi();

        onDangerChosen?.Invoke();
        Debug.Log("[PathChoice] Dipilih: Gang Sepi (dangerous)");
    }

    void HidePanel()
    {
        if (usingEditorRefs)
        {
            panelRootRef.SetActive(false);
            uiRootRef.SetActive(false);
        }
        else
        {
            panelRoot.SetActive(false);
            uiRoot.SetActive(false);
        }

        // Bebaskan kembali Rara
        if (playerScript != null) playerScript.frozen = false;
        if (playerRb != null)
            playerRb.isKinematic = false;
    }

    // ══════════════════════════════════════════════════════════════════════
    // SETUP — MODE A (Editor-Built Refs)
    // Terapkan teks & hook tombol ke referensi yang sudah di-assign di Inspector.
    // ══════════════════════════════════════════════════════════════════════

    void SetupEditorRefs()
    {
        // Isi teks dari field Inspector
        if (titleTMPRef  != null) titleTMPRef.text  = titleText;
        if (bodyTMPRef   != null) bodyTMPRef.text   = bodyText;
        if (btnSafeLabelRef   != null) btnSafeLabelRef.text   = safeLabel;
        if (btnDangerLabelRef != null) btnDangerLabelRef.text = dangerLabel;

        // Daftarkan listener tombol
        btnSafeRef.onClick.RemoveAllListeners();
        btnSafeRef.onClick.AddListener(OnSafeButton);

        btnDangerRef.onClick.RemoveAllListeners();
        btnDangerRef.onClick.AddListener(OnDangerButton);

        // Terapkan sprite & layout custom (jika di-set di Inspector)
        ApplyCustomSprites();
        ApplyCustomLayout();
        ApplyTextStyleAll();

        // Sembunyikan panel saat awal (panel masih terlihat di Editor agar mudah diedit)
        panelRootRef.SetActive(false);
        uiRootRef.SetActive(false);

        Debug.Log("[PathChoiceUI] Mode A aktif — menggunakan UI dari Editor.");
    }

    // ══════════════════════════════════════════════════════════════════════
    // APPLY — Sprite & Layout Override (berlaku di Mode A & Mode B)
    // ══════════════════════════════════════════════════════════════════════

    void ApplyCustomSprites()
    {
        // Panel background — cari Image di GameObject yang sama ATAU di child.
        if (panelBgSprite != null)
        {
            Image img = ResolveImage(panelRootRef, panelRoot, "Panel");
            if (img != null) SetSpriteOnImage(img, panelBgSprite);
            else if (debugLog) Debug.LogWarning("[PathChoiceUI] panelBgSprite di-set tapi tidak ketemu Image target. Drag Panel Root Ref di Inspector.");
        }

        // Overlay — cek overlayImageRef (Mode A), uiRootRef child, uiRoot child (Mode B), recursive.
        if (overlayBgSprite != null)
        {
            Image ov = overlayImageRef;
            if (ov == null)
            {
                var t = FindRT(null, "Overlay");
                if (t != null) ov = t.GetComponent<Image>();
            }
            if (ov != null) SetSpriteOnImage(ov, overlayBgSprite);
            else if (debugLog) Debug.LogWarning("[PathChoiceUI] overlayBgSprite di-set tapi Overlay tidak ketemu. Drag Overlay Image Ref di Inspector.");
        }

        // Tombol Safe — pakai FindRT recursive supaya nemu meski tombol bersarang dalam.
        if (safeButtonSprite != null)
        {
            Image img = ResolveButtonImage(btnSafeRef, "BtnSafe");
            if (img != null) SetSpriteOnImage(img, safeButtonSprite);
            else if (debugLog) Debug.LogWarning("[PathChoiceUI] safeButtonSprite di-set tapi BtnSafe tidak ketemu. Drag Btn Safe Ref di Inspector.");
        }

        // Tombol Danger — sama seperti Safe.
        if (dangerButtonSprite != null)
        {
            Image img = ResolveButtonImage(btnDangerRef, "BtnDanger");
            if (img != null) SetSpriteOnImage(img, dangerButtonSprite);
            else if (debugLog) Debug.LogWarning("[PathChoiceUI] dangerButtonSprite di-set tapi BtnDanger tidak ketemu. Drag Btn Danger Ref di Inspector.");
        }

        // Terapkan ukuran sprite (panel, overlay, scale tombol)
        ApplySpriteSizes();
    }

    // Helper: cari Image di refA / refB ATAU di anaknya (pertama yang punya Image).
    // childName dipakai sebagai fallback recursive search.
    Image ResolveImage(GameObject refA, GameObject refB, string childName)
    {
        GameObject src = refA != null ? refA : refB;
        if (src != null)
        {
            var img = src.GetComponent<Image>();
            if (img != null) return img;
            img = src.GetComponentInChildren<Image>(true);
            if (img != null) return img;
        }
        var t = FindRT(null, childName);
        if (t != null) return t.GetComponent<Image>();
        return null;
    }

    Image ResolveButtonImage(Button btnRef, string childName)
    {
        if (btnRef != null)
        {
            var img = btnRef.GetComponent<Image>();
            if (img != null) return img;
            img = btnRef.GetComponentInChildren<Image>(true);
            if (img != null) return img;
        }
        var t = FindRT(null, childName);
        if (t != null)
        {
            var img = t.GetComponent<Image>();
            if (img != null) return img;
            return t.GetComponentInChildren<Image>(true);
        }
        return null;
    }

    void ApplySpriteSizes()
    {
        if (!overrideSpriteSize) return;

        // — PANEL —
        RectTransform panelRT = null;
        if (panelRootRef != null)   panelRT = panelRootRef.GetComponent<RectTransform>();
        else if (panelRoot != null) panelRT = panelRoot.GetComponent<RectTransform>();
        // Fallback: kalau panelRootRef belum di-assign user, auto-detect dari Canvas.
        if (panelRT == null)
        {
            var auto = AutoFindPanelRectTransform();
            if (auto != null)
            {
                panelRT = auto;
                panelRootRef = auto.gameObject; // simpan untuk pakai berikutnya
                Debug.LogWarning("[PathChoiceUI] panelRootRef kosong — auto-detect ke: " + auto.name);
            }
        }
        if (panelRT != null)
        {
            DisableLayoutOnParent(panelRT);
            DisableLayoutElement(panelRT);
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.pivot     = new Vector2(0.5f, 0.5f);
            panelRT.anchoredPosition = panelAnchoredPos;
            panelRT.sizeDelta        = GetSafePanelSize();
            panelRT.localScale       = Vector3.one * panelScale;
            if (debugLog) Debug.Log($"[PathChoiceUI] Panel rect → pos={panelAnchoredPos} size={panelRT.sizeDelta} (Inspector={panelSize}) scale={panelScale}");
        }
        else if (debugLog) Debug.LogWarning("[PathChoiceUI] Panel RT tidak ditemukan — drag Panel Root Ref di Inspector.");

        // — OVERLAY —
        RectTransform ovRT = null;
        if (overlayImageRef != null) ovRT = overlayImageRef.GetComponent<RectTransform>();
        else
        {
            var t = FindRT(null, "Overlay");
            if (t != null) ovRT = t;
        }
        if (ovRT != null)
        {
            if (overlayStretchFullscreen)
            {
                ovRT.anchorMin = Vector2.zero;
                ovRT.anchorMax = Vector2.one;
                ovRT.offsetMin = Vector2.zero;
                ovRT.offsetMax = Vector2.zero;
            }
            else
            {
                ovRT.anchorMin = new Vector2(0.5f, 0.5f);
                ovRT.anchorMax = new Vector2(0.5f, 0.5f);
                ovRT.pivot     = new Vector2(0.5f, 0.5f);
                ovRT.anchoredPosition = Vector2.zero;
                ovRT.sizeDelta        = overlaySize;
            }
        }

        // — SKALA TOMBOL —
        // Kalau scaleChildrenWithPanel ON, kalikan skala tombol dengan panelScale
        // supaya tombol membesar/mengecil mengikuti panel.
        float childMul = scaleChildrenWithPanel ? panelScale : 1f;

        RectTransform safeRT   = FindRT(btnSafeRef   != null ? btnSafeRef.transform   : null, "BtnSafe");
        if (safeRT != null) safeRT.localScale = Vector3.one * (safeButtonScale * childMul);

        RectTransform dangerRT = FindRT(btnDangerRef != null ? btnDangerRef.transform : null, "BtnDanger");
        if (dangerRT != null) dangerRT.localScale = Vector3.one * (dangerButtonScale * childMul);
    }

    // Cari Image dengan luas TERBESAR di hierarki Canvas terdekat — anggap sebagai panel.
    // Skip Overlay (anchor stretch fullscreen) agar tidak salah pilih.
    RectTransform AutoFindPanelRectTransform()
    {
        Transform searchRoot = null;
        if (uiRootRef    != null) searchRoot = uiRootRef.transform;
        else if (panelRootRef != null) searchRoot = panelRootRef.transform.root;
        else if (uiRoot != null) searchRoot = uiRoot.transform;
        else
        {
            var anyCanvas = FindFirstObjectByType<Canvas>();
            if (anyCanvas != null) searchRoot = anyCanvas.transform;
        }
        if (searchRoot == null) return null;

        Image best = null;
        float bestArea = 0f;
        var imgs = searchRoot.GetComponentsInChildren<Image>(true);
        foreach (var img in imgs)
        {
            if (img.sprite == null) continue;
            var rt = img.rectTransform;
            // Skip Image yang stretch fullscreen (kemungkinan Overlay/UIRoot)
            if (rt.anchorMin == Vector2.zero && rt.anchorMax == Vector2.one) continue;
            if (rt.name.ToLower().Contains("overlay")) continue;
            float a = Mathf.Abs(rt.rect.width * rt.rect.height);
            if (a > bestArea) { bestArea = a; best = img; }
        }
        return best != null ? best.rectTransform : null;
    }

    void SetSpriteOnImage(Image img, Sprite sp)
    {
        img.sprite = sp;
        img.type   = spriteImageType;
        if (!tintSpriteWithColor) img.color = Color.white;
    }

    void ApplyCustomLayout()
    {
        if (!overrideButtonLayout) return;

        // Kalau scaleChildrenWithPanel ON, posisi & size tombol ikut panelScale
        // sehingga jarak/ukuran tetap proporsional saat panel diperbesar.
        float k = scaleChildrenWithPanel ? panelScale : 1f;

        RectTransform rtSafe   = FindRT(btnSafeRef   != null ? btnSafeRef.transform   : null, "BtnSafe");
        RectTransform rtDanger = FindRT(btnDangerRef != null ? btnDangerRef.transform : null, "BtnDanger");

        // Sumber ukuran tombol: dari sprite (kalau ON) atau dari Inspector.
        Vector2 safeBase   = GetButtonBaseSize(rtSafe,   safeButtonSprite,   safeButtonSize);
        Vector2 dangerBase = GetButtonBaseSize(rtDanger, dangerButtonSprite, dangerButtonSize);

        if (rtSafe != null)
        {
            DisableLayoutOnParent(rtSafe);
            DisableLayoutElement(rtSafe);
            Vector2 sz = new Vector2(
                Mathf.Max(safeBase.x * k, minTouchTargetPx),
                Mathf.Max(safeBase.y * k, minTouchTargetPx));
            ApplyRect(rtSafe, safeButtonAnchoredPos * k, sz);
            if (debugLog) Debug.Log($"[PathChoiceUI] Safe rect → pos={safeButtonAnchoredPos * k} size={sz} (base={safeBase}, k={k})");
        }
        else if (debugLog) Debug.LogWarning("[PathChoiceUI] rtSafe TIDAK ditemukan — drag Btn Safe Ref di Inspector atau pastikan nama child 'BtnSafe' di bawah Panel.");

        if (rtDanger != null)
        {
            DisableLayoutOnParent(rtDanger);
            DisableLayoutElement(rtDanger);
            Vector2 szD = new Vector2(
                Mathf.Max(dangerBase.x * k, minTouchTargetPx),
                Mathf.Max(dangerBase.y * k, minTouchTargetPx));
            ApplyRect(rtDanger, dangerButtonAnchoredPos * k, szD);
            if (debugLog) Debug.Log($"[PathChoiceUI] Danger rect → pos={dangerButtonAnchoredPos * k} size={szD} (base={dangerBase}, k={k})");
        }
        else if (debugLog) Debug.LogWarning("[PathChoiceUI] rtDanger TIDAK ditemukan — drag Btn Danger Ref di Inspector atau pastikan nama child 'BtnDanger' di bawah Panel.");
    }

    // Tentukan ukuran dasar tombol: ikut sprite (kalau ON) atau pakai nilai manual.
    Vector2 GetButtonBaseSize(RectTransform rt, Sprite explicitSprite, Vector2 manualSize)
    {
        if (!matchButtonSizeToSprite) return manualSize;

        // Cari sprite aktif: prioritas field Inspector → Image pada button.
        Sprite sp = explicitSprite;
        if (sp == null && rt != null)
        {
            var img = rt.GetComponent<Image>();
            if (img != null) sp = img.sprite;
        }
        if (sp == null) return manualSize; // fallback

        // Ukuran natural sprite dalam piksel UI = (rect.size) / pixelsPerUnit.
        // (untuk UI, sprite biasanya pixelsPerUnit=100; tapi pakai sprite.pixelsPerUnit langsung agar akurat)
        float ppu = sp.pixelsPerUnit > 0f ? sp.pixelsPerUnit : 100f;
        Vector2 natural = new Vector2(sp.rect.width, sp.rect.height) * (100f / ppu);
        return natural * buttonSpriteSizeMul;
    }

    // Helper: cari RectTransform dari ref langsung, atau fallback cari child by name
    // di SEMUA kandidat parent (panelRoot, panelRootRef, dan rekursif).
    RectTransform FindRT(Transform direct, string childName)
    {
        if (direct != null) return direct as RectTransform;

        // Cek di panelRoot (Mode B)
        if (panelRoot != null)
        {
            var t = FindDeep(panelRoot.transform, childName);
            if (t != null) return t as RectTransform;
        }
        // Cek di panelRootRef (Mode A)
        if (panelRootRef != null)
        {
            var t = FindDeep(panelRootRef.transform, childName);
            if (t != null) return t as RectTransform;
        }
        // Cek di uiRootRef (kalau tombol berada langsung di UIRoot, bukan di Panel)
        if (uiRootRef != null)
        {
            var t = FindDeep(uiRootRef.transform, childName);
            if (t != null) return t as RectTransform;
        }
        return null;
    }

    Transform FindDeep(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            var c = parent.GetChild(i);
            if (c.name == name) return c;
            var r = FindDeep(c, name);
            if (r != null) return r;
        }
        return null;
    }

    // Non-aktifkan LayoutGroup pada parent (jika ada) supaya anchoredPosition/sizeDelta tidak ditimpa tiap frame.
    void DisableLayoutOnParent(RectTransform child)
    {
        if (!autoDisableLayoutGroup) return;
        var parent = child.parent;
        if (parent == null) return;
        var lg = parent.GetComponent<UnityEngine.UI.LayoutGroup>();
        if (lg != null && lg.enabled)
        {
            lg.enabled = false;
            if (debugLog) Debug.Log($"[PathChoiceUI] LayoutGroup di '{parent.name}' DI-NONAKTIFKAN agar override layout berlaku.");
        }
        var fit = parent.GetComponent<UnityEngine.UI.ContentSizeFitter>();
        if (fit != null && fit.enabled)
        {
            fit.enabled = false;
            if (debugLog) Debug.Log($"[PathChoiceUI] ContentSizeFitter di '{parent.name}' DI-NONAKTIFKAN.");
        }
    }

    // Non-aktifkan LayoutElement pada child sendiri agar tidak mengunci ukuran.
    void DisableLayoutElement(RectTransform child)
    {
        if (!autoDisableLayoutGroup) return;
        var le = child.GetComponent<UnityEngine.UI.LayoutElement>();
        if (le != null && le.enabled)
        {
            le.ignoreLayout = true;
            if (debugLog) Debug.Log($"[PathChoiceUI] LayoutElement di '{child.name}' → ignoreLayout=true.");
        }
    }

    void ApplyRect(RectTransform rt, Vector2 pos, Vector2 size)
    {
        // Anchor di tengah panel agar posisi/ukuran konsisten
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
    }

    // Terapkan font & style ke seluruh teks (judul, body, label tombol).
    void ApplyTextStyleAll()
    {
        // Kalau scaleChildrenWithPanel ON, font ikut panelScale.
        float fk = scaleChildrenWithPanel ? panelScale : 1f;

        // —— JUDUL ——
        TextMeshProUGUI title = titleTMPRef;
        if (title == null && panelRoot != null)
        {
            var t = panelRoot.transform.Find("Title");
            if (t != null) title = t.GetComponent<TextMeshProUGUI>();
        }
        if (title != null)
        {
            if (titleFontAsset != null) title.font = titleFontAsset;
            else if (fontAsset != null) title.font = fontAsset;
            title.color = titleColor;
            if (overrideTextStyle)
            {
                title.fontSize          = titleFontSize * fk;
                title.fontStyle         = titleFontStyle;
                title.alignment         = titleAlignment;
                title.characterSpacing  = titleCharacterSpacing;
                title.lineSpacing       = titleLineSpacing;
                ApplyOutline(title, titleUseOutline, titleOutlineColor, titleOutlineWidth);
            }
        }

        // —— BODY ——
        TextMeshProUGUI body = bodyTMPRef;
        if (body == null && panelRoot != null)
        {
            var t = panelRoot.transform.Find("Body");
            if (t != null) body = t.GetComponent<TextMeshProUGUI>();
        }
        if (body != null)
        {
            if (bodyFontAsset != null) body.font = bodyFontAsset;
            else if (fontAsset != null) body.font = fontAsset;
            body.color = bodyColor;
            if (overrideTextStyle)
            {
                body.fontSize          = bodyFontSize * fk;
                body.fontStyle         = bodyFontStyle;
                body.alignment         = bodyAlignment;
                body.characterSpacing  = bodyCharacterSpacing;
                body.lineSpacing       = bodyLineSpacing;
                ApplyOutline(body, bodyUseOutline, bodyOutlineColor, bodyOutlineWidth);
            }
        }

        // —— LABEL TOMBOL ——
        ApplyButtonLabelStyle(btnSafeLabelRef,   "BtnSafe",   fk);
        ApplyButtonLabelStyle(btnDangerLabelRef, "BtnDanger", fk);
    }

    void ApplyButtonLabelStyle(TextMeshProUGUI lblRef, string parentName, float fontMul)
    {
        TextMeshProUGUI lbl = lblRef;
        if (lbl == null && panelRoot != null)
        {
            var p = panelRoot.transform.Find(parentName);
            if (p != null)
            {
                var t = p.Find("Label");
                if (t != null) lbl = t.GetComponent<TextMeshProUGUI>();
            }
        }
        if (lbl == null) return;

        if (buttonFontAsset != null) lbl.font = buttonFontAsset;
        else if (fontAsset != null)  lbl.font = fontAsset;
        lbl.color = btnTextColor;
        if (overrideTextStyle)
        {
            lbl.fontSize  = buttonFontSize * fontMul;
            lbl.fontStyle = buttonFontStyle;
            lbl.alignment = buttonAlignment;
            ApplyOutline(lbl, buttonUseOutline, buttonOutlineColor, buttonOutlineWidth);
        }
    }

    void ApplyOutline(TextMeshProUGUI tmp, bool on, Color col, float width)
    {
        if (on)
        {
            tmp.outlineColor = col;
            tmp.outlineWidth = width;
        }
        else
        {
            tmp.outlineWidth = 0f;
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // LIVE EDIT — re-apply semua kustomisasi (sprite, teks, layout, ukuran)
    // ══════════════════════════════════════════════════════════════════════

    [ContextMenu("▶ Apply Customization Sekarang")]
    public void ReapplyAllCustomization()
    {
        // Update teks dasar (Mode A & B)
        if (titleTMPRef       != null) titleTMPRef.text       = titleText;
        if (bodyTMPRef        != null) bodyTMPRef.text        = bodyText;
        if (btnSafeLabelRef   != null) btnSafeLabelRef.text   = safeLabel;
        if (btnDangerLabelRef != null) btnDangerLabelRef.text = dangerLabel;

        // Mode B — cari child by name untuk update teks
        if (panelRoot != null)
        {
            var tT = panelRoot.transform.Find("Title");
            if (tT != null) { var x = tT.GetComponent<TextMeshProUGUI>(); if (x != null) x.text = titleText; }
            var tB = panelRoot.transform.Find("Body");
            if (tB != null) { var x = tB.GetComponent<TextMeshProUGUI>(); if (x != null) x.text = bodyText; }
            var bS = panelRoot.transform.Find("BtnSafe");
            if (bS != null) { var l = bS.Find("Label"); if (l != null) { var x = l.GetComponent<TextMeshProUGUI>(); if (x != null) x.text = safeLabel; } }
            var bD = panelRoot.transform.Find("BtnDanger");
            if (bD != null) { var l = bD.Find("Label"); if (l != null) { var x = l.GetComponent<TextMeshProUGUI>(); if (x != null) x.text = dangerLabel; } }
        }

        ApplyCustomSprites();   // sprite + ApplySpriteSizes()
        ApplyCustomLayout();    // posisi & ukuran tombol
        ApplyTextStyleAll();    // font, size, style, alignment, outline
        EnsureCanvasScalerConfigured(); // konsistensi ukuran di semua device
    }

    // ══════════════════════════════════════════════════════════════════════
    // CROSS-DEVICE — auto-config CanvasScaler agar nilai px konsisten
    // di semua ukuran layar (HP, tablet, PC, 4K).
    // ══════════════════════════════════════════════════════════════════════
    Canvas _cachedCanvas;
    void EnsureCanvasScalerConfigured()
    {
        if (!autoConfigCanvasScaler) return;

        // Cari Canvas pemilik PathChoice UI — pakai uiRootRef/panelRootRef dulu.
        Canvas cv = _cachedCanvas;
        if (cv == null)
        {
            if (uiRootRef != null)        cv = uiRootRef.GetComponentInParent<Canvas>();
            if (cv == null && panelRootRef != null) cv = panelRootRef.GetComponentInParent<Canvas>();
            if (cv == null && uiRoot != null)       cv = uiRoot.GetComponentInParent<Canvas>();
            if (cv == null && panelRoot != null)    cv = panelRoot.GetComponentInParent<Canvas>();
            if (cv == null) cv = GetComponentInParent<Canvas>();
            if (cv != null) _cachedCanvas = cv;
        }
        if (cv == null) return;

        var scaler = cv.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = cv.gameObject.AddComponent<CanvasScaler>();

        switch (canvasScaleMode)
        {
            case CanvasScaleMode.ConstantPixelSize:
                if (scaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPixelSize
                    && Mathf.Approximately(scaler.scaleFactor, 1f)) return;
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                scaler.scaleFactor = 1f;
                if (debugLog) Debug.Log($"[PathChoiceUI] CanvasScaler '{cv.name}' → ConstantPixelSize (sprite size FIXED).");
                break;

            case CanvasScaleMode.ConstantPhysicalSize:
                if (scaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPhysicalSize) return;
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPhysicalSize;
                if (debugLog) Debug.Log($"[PathChoiceUI] CanvasScaler '{cv.name}' → ConstantPhysicalSize.");
                break;

            default: // Proportional
                if (scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize
                    && scaler.referenceResolution == referenceResolution
                    && Mathf.Approximately(scaler.matchWidthOrHeight, scalerMatchWidthOrHeight)
                    && scaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
                {
                    return;
                }
                scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = referenceResolution;
                scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight  = scalerMatchWidthOrHeight;
                if (debugLog)
                    Debug.Log($"[PathChoiceUI] CanvasScaler '{cv.name}' → Proportional, ref={referenceResolution}, match={scalerMatchWidthOrHeight}");
                break;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Saat user ubah nilai di Inspector ketika Play — re-apply.
        if (!Application.isPlaying) return;
        if (!liveEdit) return;
        // Tunda 1 frame agar nilai sudah diserap Unity.
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            ReapplyAllCustomization();
        };
    }
#endif

    // ══════════════════════════════════════════════════════════════════════
    // BUILD UI — MODE B (Programatik / Fallback)
    // ══════════════════════════════════════════════════════════════════════

    void BuildUI()
    {
        Debug.Log("[PathChoiceUI] Mode B aktif — UI dibuat secara programatik (fallback).");
        // Canvas khusus — di atas gameplay tapi di bawah dialog
        var cGO = new GameObject("PathChoiceCanvas");
        canvas = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 700;
        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // uiRoot — fullscreen container untuk overlay + panel
        // Ini yang di-hide agar overlay tidak terlihat saat panel belum muncul
        uiRoot = new GameObject("UIRoot");
        uiRoot.transform.SetParent(cGO.transform, false);
        var uiRootRT = uiRoot.AddComponent<RectTransform>();
        uiRootRT.anchorMin = Vector2.zero;
        uiRootRT.anchorMax = Vector2.one;
        uiRootRT.offsetMin = Vector2.zero;
        uiRootRT.offsetMax = Vector2.zero;

        // Overlay gelap — child uiRoot (ikut tersembunyi saat panel disembunyikan)
        var overlayGO = new GameObject("Overlay");
        overlayGO.transform.SetParent(uiRoot.transform, false);
        var overlayRT = overlayGO.AddComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.offsetMin = Vector2.zero;
        overlayRT.offsetMax = Vector2.zero;
        var overlayImg = overlayGO.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.55f);

        // Panel utama — juga child uiRoot
        panelRoot = new GameObject("Panel");
        panelRoot.transform.SetParent(uiRoot.transform, false);
        var panelRT = panelRoot.AddComponent<RectTransform>();
        float pw = (1f - panelWidthRatio)  * 0.5f;
        float ph = (1f - panelHeightRatio) * 0.5f;
        panelRT.anchorMin = new Vector2(pw, ph);
        panelRT.anchorMax = new Vector2(1f - pw, 1f - ph);
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        var panelImg = panelRoot.AddComponent<Image>();
        panelImg.color = panelBgColor;

        // Border panel (outline tebal kuning)
        var outline = panelRoot.AddComponent<Outline>();
        outline.effectColor    = borderColor;
        outline.effectDistance = new Vector2(6f, -6f);

        // ── Judul ─────────────────────────────────────────────────────────
        var titleGO = MakeText(panelRoot, "Title",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(20f, -90f), new Vector2(-20f, -12f),
            52, titleColor, TextAlignmentOptions.Center);
        titleGO.text = titleText;
        titleGO.fontStyle = FontStyles.Bold;

        // ── Deskripsi ─────────────────────────────────────────────────────
        var bodyGO = MakeText(panelRoot, "Body",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(20f, -200f), new Vector2(-20f, -98f),
            34, bodyColor, TextAlignmentOptions.Center);
        bodyGO.text = bodyText;
        bodyGO.textWrappingMode = TMPro.TextWrappingModes.Normal;

        // ── Tombol JALAN RAMAI (hijau) ────────────────────────────────────
        MakeChoiceButton(panelRoot, "BtnSafe", safeLabel,
            new Vector2(0.05f, 0.30f), new Vector2(0.95f, 0.58f),
            safeColor, OnSafeButton);

        // ── Tombol GANG SEPI (merah) ──────────────────────────────────────
        MakeChoiceButton(panelRoot, "BtnDanger", dangerLabel,
            new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.27f),
            dangerColor, OnDangerButton);

        // Terapkan sprite & layout custom (jika di-set di Inspector)
        ApplyCustomSprites();
        ApplyCustomLayout();
        ApplyTextStyleAll();

        panelRoot.SetActive(false);
        uiRoot.SetActive(false); // overlay ikut tersembunyi
    }

    void MakeChoiceButton(GameObject parent, string name, string label,
        Vector2 ancMin, Vector2 ancMax,
        Color color, System.Action onClick)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin;
        rt.anchorMax = ancMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = go.AddComponent<Image>();
        img.color = color;

        // Rounded corner via outline
        var ol = go.AddComponent<Outline>();
        ol.effectColor    = new Color(1f, 1f, 1f, 0.3f);
        ol.effectDistance = new Vector2(3f, -3f);

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor      = color;
        colors.highlightedColor = new Color(
            Mathf.Min(color.r + 0.15f, 1f),
            Mathf.Min(color.g + 0.15f, 1f),
            Mathf.Min(color.b + 0.15f, 1f), 1f);
        colors.pressedColor = new Color(
            color.r * 0.75f, color.g * 0.75f, color.b * 0.75f, 1f);
        btn.colors = colors;
        btn.onClick.AddListener(() => onClick());

        // Label teks
        var lbl = MakeText(go, "Label",
            Vector2.zero, Vector2.one,
            new Vector2(12f, 8f), new Vector2(-12f, -8f),
            42, btnTextColor, TextAlignmentOptions.Center);
        lbl.text      = label;
        lbl.fontStyle = FontStyles.Bold;
        lbl.textWrappingMode = TMPro.TextWrappingModes.Normal;
    }

    TextMeshProUGUI MakeText(GameObject parent, string name,
        Vector2 ancMin, Vector2 ancMax,
        Vector2 offMin, Vector2 offMax,
        int fontSize, Color color, TextAlignmentOptions align)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin;
        rt.anchorMax = ancMax;
        rt.pivot     = new Vector2(0.5f, 1f);
        rt.offsetMin = offMin;
        rt.offsetMax = offMax;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        ApplyFont(tmp);
        tmp.fontSize  = fontSize;
        tmp.color     = color;
        tmp.alignment = align;
        return tmp;
    }

    void ApplyFont(TextMeshProUGUI tmp)
    {
        TMP_FontAsset f = fontAsset;
        if (f == null) f = TMP_Settings.defaultFontAsset;
        if (f == null) f = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (f != null) tmp.font = f;
    }

    // ══════════════════════════════════════════════════════════════════════
    // GIZMO — tampilkan lingkaran trigger di Scene view
    // ══════════════════════════════════════════════════════════════════════
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
        Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.15f);
        Gizmos.DrawSphere(transform.position, triggerDistance);
    }
#endif
}
