using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// NpcDialog — sistem dialog ringkas untuk NPC (mis. Paman Baik).
///
/// Cara pakai:
///   1. Tambahkan komponen ini ke GameObject mana saja (boleh ke NPC-nya langsung).
///   2. Isi array <see cref="lines"/> di Inspector:
///        • speakerName : nama pembicara ("Paman", "Rara", dll.)
///        • profile     : sprite foto/ikon pembicara (opsional)
///        • text        : isi kalimat
///   3. Atur tampilan (warna panel, ukuran font, posisi) di header "Tampilan".
///   4. Panggil <see cref="Play"/> dari script lain (mis. PamanBaik) untuk memulai.
///
/// Pemain menekan SPACE / ENTER / klik untuk lanjut.
/// UI dibangun otomatis saat runtime — tidak perlu setup Canvas manual.
/// </summary>
public class NpcDialog : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // DATA
    // ══════════════════════════════════════════════════════════════════════

    [System.Serializable]
    public class DialogEntry
    {
        public string speakerName = "Paman";
        public Sprite profile;
        [TextArea(2, 5)]
        public string text = "Halo Rara, hati-hati di jalan ya!";
    }

    [Header("Daftar Dialog")]
    [Tooltip("Tambah elemen untuk menambah baris dialog. Urut dari atas ke bawah.")]
    public DialogEntry[] lines;

    // ══════════════════════════════════════════════════════════════════════
    // TAMPILAN
    // ══════════════════════════════════════════════════════════════════════

    [Header("Tampilan — Panel")]
    public Color  panelColor       = new Color(0f, 0f, 0f, 0.78f);
    public Color  borderColor      = new Color(1f, 0.85f, 0.3f, 1f);
    public float  panelHeight      = 220f;
    [Range(0f, 1f)]
    public float  panelWidthRatio  = 0.85f;
    [Tooltip("Jarak panel dari bawah layar (pixel)")]
    public float  bottomMargin     = 40f;
    public bool   showAtTop        = false;

    [Header("Tampilan — Profil")]
    public float  profileSize      = 160f;
    public Color  profileBgColor   = new Color(1f, 1f, 1f, 0.1f);
    public bool   profileOnRight   = false;

    [Header("Tampilan — Teks")]
    public Color  speakerColor     = new Color(1f, 0.85f, 0.3f, 1f);
    public Color  textColor        = Color.white;
    public int    speakerFontSize  = 32;
    public int    textFontSize     = 26;

    [Header("Efek Ketik")]
    [Range(0f, 0.1f)]
    public float  typeSpeed        = 0.025f;

    [Header("Petunjuk Lanjut")]
    public string continueHint     = "▼ SPACE / Klik untuk lanjut";
    public Color  hintColor        = new Color(1f, 1f, 1f, 0.55f);
    public int    hintFontSize     = 16;

    [Header("Event")]
    [Tooltip("Dipanggil saat seluruh dialog selesai")]
    public UnityEngine.Events.UnityEvent onDialogEnd;

    // ── runtime state ────────────────────────────────────────────────────
    private Canvas           canvas;
    private GameObject       panelRoot;
    private Image            profileImg;
    private TextMeshProUGUI  speakerTMP;
    private TextMeshProUGUI  textTMP;
    private TextMeshProUGUI  hintTMP;
    private int              currentIndex;
    private bool             isPlaying;
    private bool             isTyping;
    private Coroutine        typingCo;

    public bool IsPlaying => isPlaying;

    // ══════════════════════════════════════════════════════════════════════
    void Update()
    {
        if (!isPlaying) return;

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetMouseButtonDown(0))
        {
            Advance();
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // PUBLIC API
    // ══════════════════════════════════════════════════════════════════════

    /// Mulai memutar dialog dari awal.
    public void Play()
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("[NpcDialog] Daftar 'lines' kosong — tidak ada yang diputar.");
            return;
        }

        BuildUIIfNeeded();
        currentIndex = 0;
        isPlaying    = true;
        panelRoot.SetActive(true);
        ShowLine(0);
    }

    /// Tutup paksa.
    public void Close()
    {
        isPlaying = false;
        if (typingCo != null) StopCoroutine(typingCo);
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    // ══════════════════════════════════════════════════════════════════════
    // INTERNAL — alur dialog
    // ══════════════════════════════════════════════════════════════════════

    void Advance()
    {
        if (isTyping)
        {
            // skip animasi ketik
            if (typingCo != null) StopCoroutine(typingCo);
            isTyping     = false;
            textTMP.text = lines[currentIndex].text;
            return;
        }

        currentIndex++;
        if (currentIndex >= lines.Length)
        {
            EndDialog();
            return;
        }
        ShowLine(currentIndex);
    }

    void ShowLine(int idx)
    {
        var line = lines[idx];

        speakerTMP.text = line.speakerName;
        if (line.profile != null)
        {
            profileImg.sprite = line.profile;
            profileImg.color  = Color.white;
            profileImg.enabled = true;
        }
        else
        {
            profileImg.enabled = false;
        }

        if (typingCo != null) StopCoroutine(typingCo);
        typingCo = StartCoroutine(TypeText(line.text));
    }

    IEnumerator TypeText(string full)
    {
        isTyping     = true;
        textTMP.text = "";
        if (typeSpeed <= 0f)
        {
            textTMP.text = full;
            isTyping = false;
            yield break;
        }
        foreach (char c in full)
        {
            textTMP.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
        isTyping = false;
    }

    void EndDialog()
    {
        isPlaying = false;
        panelRoot.SetActive(false);
        onDialogEnd?.Invoke();
    }

    // ══════════════════════════════════════════════════════════════════════
    // INTERNAL — bangun UI otomatis
    // ══════════════════════════════════════════════════════════════════════

    void BuildUIIfNeeded()
    {
        if (panelRoot != null) return;

        // 1) Cari/buat Canvas
        canvas = FindAnyOverlayCanvas();
        if (canvas == null)
        {
            var go = new GameObject("NpcDialogCanvas");
            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.referenceResolution    = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight     = 0.5f;
            go.AddComponent<GraphicRaycaster>();
        }

        // 2) Panel utama
        panelRoot = new GameObject("NpcDialogPanel");
        panelRoot.transform.SetParent(canvas.transform, false);
        var panelRT = panelRoot.AddComponent<RectTransform>();
        var panelImg = panelRoot.AddComponent<Image>();
        panelImg.color = panelColor;

        // anchor bawah-tengah (atau atas-tengah)
        Vector2 anchor = showAtTop ? new Vector2(0.5f, 1f) : new Vector2(0.5f, 0f);
        panelRT.anchorMin = anchor;
        panelRT.anchorMax = anchor;
        panelRT.pivot     = anchor;
        panelRT.sizeDelta = new Vector2(Screen.width * panelWidthRatio, panelHeight);
        panelRT.anchoredPosition = new Vector2(0f, showAtTop ? -bottomMargin : bottomMargin);

        // border (Outline)
        var outline = panelRoot.AddComponent<Outline>();
        outline.effectColor    = borderColor;
        outline.effectDistance = new Vector2(3f, -3f);

        // 3) Profil
        var profileGO = new GameObject("Profile");
        profileGO.transform.SetParent(panelRoot.transform, false);
        var profileRT  = profileGO.AddComponent<RectTransform>();
        var profileBg  = profileGO.AddComponent<Image>();
        profileBg.color = profileBgColor;

        Vector2 pAnchor = profileOnRight ? new Vector2(1f, 0.5f) : new Vector2(0f, 0.5f);
        profileRT.anchorMin = pAnchor;
        profileRT.anchorMax = pAnchor;
        profileRT.pivot     = pAnchor;
        profileRT.sizeDelta = new Vector2(profileSize, profileSize);
        float pOffsetX = profileOnRight ? -20f : 20f;
        profileRT.anchoredPosition = new Vector2(pOffsetX, 0f);

        // Image sprite di dalam frame profil
        var imgGO = new GameObject("Sprite");
        imgGO.transform.SetParent(profileGO.transform, false);
        var imgRT = imgGO.AddComponent<RectTransform>();
        imgRT.anchorMin = Vector2.zero;
        imgRT.anchorMax = Vector2.one;
        imgRT.offsetMin = new Vector2(6, 6);
        imgRT.offsetMax = new Vector2(-6, -6);
        profileImg = imgGO.AddComponent<Image>();
        profileImg.preserveAspect = true;

        // 4) Speaker name
        var speakerGO = new GameObject("SpeakerName");
        speakerGO.transform.SetParent(panelRoot.transform, false);
        var speakerRT = speakerGO.AddComponent<RectTransform>();
        speakerTMP    = speakerGO.AddComponent<TextMeshProUGUI>();
        speakerTMP.fontSize  = speakerFontSize;
        speakerTMP.color     = speakerColor;
        speakerTMP.fontStyle = FontStyles.Bold;
        speakerTMP.alignment = TextAlignmentOptions.TopLeft;

        speakerRT.anchorMin = new Vector2(0f, 1f);
        speakerRT.anchorMax = new Vector2(1f, 1f);
        speakerRT.pivot     = new Vector2(0.5f, 1f);
        float leftPad  = profileOnRight ? 30f : (profileSize + 40f);
        float rightPad = profileOnRight ? (profileSize + 40f) : 30f;
        speakerRT.offsetMin = new Vector2(leftPad,  -50f);
        speakerRT.offsetMax = new Vector2(-rightPad, -10f);

        // 5) Body text
        var bodyGO = new GameObject("BodyText");
        bodyGO.transform.SetParent(panelRoot.transform, false);
        var bodyRT = bodyGO.AddComponent<RectTransform>();
        textTMP    = bodyGO.AddComponent<TextMeshProUGUI>();
        textTMP.fontSize  = textFontSize;
        textTMP.color     = textColor;
        textTMP.alignment = TextAlignmentOptions.TopLeft;
        textTMP.enableWordWrapping = true;

        bodyRT.anchorMin = new Vector2(0f, 0f);
        bodyRT.anchorMax = new Vector2(1f, 1f);
        bodyRT.offsetMin = new Vector2(leftPad,   30f);
        bodyRT.offsetMax = new Vector2(-rightPad, -60f);

        // 6) Hint lanjut
        var hintGO = new GameObject("ContinueHint");
        hintGO.transform.SetParent(panelRoot.transform, false);
        var hintRT = hintGO.AddComponent<RectTransform>();
        hintTMP    = hintGO.AddComponent<TextMeshProUGUI>();
        hintTMP.fontSize  = hintFontSize;
        hintTMP.color     = hintColor;
        hintTMP.alignment = TextAlignmentOptions.BottomRight;
        hintTMP.text      = continueHint;

        hintRT.anchorMin = new Vector2(0f, 0f);
        hintRT.anchorMax = new Vector2(1f, 0f);
        hintRT.pivot     = new Vector2(1f, 0f);
        hintRT.offsetMin = new Vector2(0f, 6f);
        hintRT.offsetMax = new Vector2(-20f, 28f);

        panelRoot.SetActive(false);
    }

    static Canvas FindAnyOverlayCanvas()
    {
#if UNITY_2023_1_OR_NEWER
        var all = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
#else
        var all = UnityEngine.Object.FindObjectsOfType<Canvas>();
#endif
        foreach (var c in all)
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay) return c;
        }
        return null;
    }
}
