using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// PathEnvironment — toggle latar Jalan Ramai / Gang Sepi saat pilihan dibuat.
/// Setup: drag GO JalanRamai & GangSepi ke field Inspector.
/// Keduanya harus BUKAN child dari Background Awal.
/// </summary>
public class PathEnvironment : MonoBehaviour
{
    [Header("── BACKGROUND AWAL (persimpangan) ──")]
    [Tooltip("GO background persimpangan — disembunyikan saat jalur dipilih.\nJangan masukkan JalanRamai atau GangSepi ke sini.")]
    public GameObject[] backgroundAwal;

    [Header("── JALAN RAMAI ──")]
    [Tooltip("GO berisi sprite Jalan Ramai. Posisikan di scene. SetActive awal = FALSE.")]
    public GameObject jalanRamai;

    [Header("── GANG SEPI ──")]
    [Tooltip("GO berisi sprite Gang Sepi. Posisikan di scene. SetActive awal = FALSE.")]
    public GameObject gangSepi;

    [Header("── KAMERA ──")]
    public Camera mainCamera;
    public Color camBgJalanRamai = new Color(0.35f, 0.65f, 0.90f, 1f);
    public Color camBgGangSepi   = new Color(0.08f, 0.06f, 0.10f, 1f);

    [Header("── PENCAHAYAAN ──")]
    public Color ambientJalanRamai         = new Color(1f, 0.95f, 0.85f, 1f);
    public Color ambientGangSepi           = new Color(0.25f, 0.22f, 0.30f, 1f);
    public float ambientTransitionDuration = 0.8f;

    [Header("── OVERLAY NOTIFIKASI ──")]
    public float  overlayDurasiTampil   = 2.5f;
    public float  overlayDurasiTransisi = 0.45f;
    public string overlayJudulRamai     = "JALAN RAMAI";
    public string overlaySubRamai       = "Jalan yang aman dan ramai";
    public string overlayJudulGang      = "GANG SEPI";
    public string overlaySubGang        = "⚠ Jalur berbahaya! Hati-hati!";
    public Color  overlayBgRamai        = new Color(0.05f, 0.18f, 0.05f, 0.92f);
    public Color  overlayBgGang         = new Color(0.18f, 0.03f, 0.03f, 0.92f);
    public Color  overlayWarnaRamai     = new Color(0.15f, 0.90f, 0.35f, 1f);
    public Color  overlayWarnaGang      = new Color(0.95f, 0.25f, 0.20f, 1f);
    public Color  overlayWarnaSub       = Color.white;
    public Color  overlayWarnaGaris     = new Color(1f, 0.85f, 0.1f, 0.7f);
    public TMP_FontAsset overlayFont;

    [Header("── EVENTS ──")]
    public UnityEvent onJalanRamaiAktif;
    public UnityEvent onGangSepiAktif;

    private Transform playerTransform;

    // ══════════════════════════════════════════════════════════════════════
    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        playerTransform = GameObject.FindWithTag("Player")?.transform;

        if (jalanRamai == null)
            Debug.LogWarning("[PathEnvironment] Field 'Jalan Ramai' belum di-assign!");
        if (gangSepi == null)
            Debug.LogWarning("[PathEnvironment] Field 'Gang Sepi' belum di-assign!");

        // Pisahkan jalanRamai & gangSepi ke root scene supaya tidak ikut
        // inactive saat parent backgroundAwal di-SetActive(false)
        if (jalanRamai != null && jalanRamai.transform.parent != null)
        {
            jalanRamai.transform.SetParent(null, true);
            Debug.Log("[PathEnvironment] jalanRamai dipindah ke root scene");
        }
        if (gangSepi != null && gangSepi.transform.parent != null)
        {
            gangSepi.transform.SetParent(null, true);
            Debug.Log("[PathEnvironment] gangSepi dipindah ke root scene");
        }

        // Sembunyikan jalur di awal — background persimpangan tetap aktif
        if (jalanRamai != null) jalanRamai.SetActive(false);
        if (gangSepi   != null) gangSepi.SetActive(false);

        Debug.Log("[PathEnvironment] Start OK. JalanRamai=" +
            (jalanRamai != null ? jalanRamai.name : "NULL") +
            " GangSepi=" + (gangSepi != null ? gangSepi.name : "NULL"));
    }

    // ══════════════════════════════════════════════════════════════════════
    // PUBLIC API
    // ══════════════════════════════════════════════════════════════════════

    public void AktifkanJalanRamai()
    {
        Debug.Log("[PathEnvironment] AktifkanJalanRamai()");
        SembunyikanBackgroundAwal();
        if (gangSepi   != null) gangSepi.SetActive(false);
        if (jalanRamai != null)
        {
            jalanRamai.SetActive(true);
            Debug.Log("[PathEnvironment] JalanRamai AKTIF");
        }
        else Debug.LogWarning("[PathEnvironment] jalanRamai NULL!");

        if (mainCamera != null) mainCamera.backgroundColor = camBgJalanRamai;
        StopAllCoroutines();
        StartCoroutine(TransisiAmbient(ambientJalanRamai));
        StartCoroutine(TampilkanOverlay(overlayJudulRamai, overlaySubRamai,
            overlayBgRamai, overlayWarnaRamai));
        onJalanRamaiAktif?.Invoke();
    }

    public void AktifkanGangSepi()
    {
        Debug.Log("[PathEnvironment] AktifkanGangSepi()");
        SembunyikanBackgroundAwal();
        if (jalanRamai != null) jalanRamai.SetActive(false);
        if (gangSepi   != null)
        {
            gangSepi.SetActive(true);
            Debug.Log("[PathEnvironment] GangSepi AKTIF");
        }
        else Debug.LogWarning("[PathEnvironment] gangSepi NULL!");

        if (mainCamera != null) mainCamera.backgroundColor = camBgGangSepi;
        StopAllCoroutines();
        StartCoroutine(TransisiAmbient(ambientGangSepi));
        StartCoroutine(TampilkanOverlay(overlayJudulGang, overlaySubGang,
            overlayBgGang, overlayWarnaGang));
        onGangSepiAktif?.Invoke();
    }

    // ══════════════════════════════════════════════════════════════════════
    // INTERNAL
    // ══════════════════════════════════════════════════════════════════════

    void SembunyikanBackgroundAwal()
    {
        if (backgroundAwal == null) return;

        // Selamatkan player jika ia child dari backgroundAwal
        if (playerTransform != null)
        {
            foreach (var bg in backgroundAwal)
            {
                if (bg != null && playerTransform.IsChildOf(bg.transform))
                {
                    playerTransform.SetParent(null, true);
                    Debug.Log("[PathEnvironment] Player dikeluarkan dari " + bg.name);
                    break;
                }
            }
        }

        foreach (var bg in backgroundAwal)
        {
            if (bg == null || bg == jalanRamai || bg == gangSepi) continue;
            bg.SetActive(false);
            Debug.Log("[PathEnvironment] Hide: " + bg.name);
        }
    }

    IEnumerator TransisiAmbient(Color target)
    {
        Color start = RenderSettings.ambientLight;
        for (float t = 0f; t < ambientTransitionDuration; t += Time.deltaTime)
        {
            RenderSettings.ambientLight = Color.Lerp(start, target,
                t / ambientTransitionDuration);
            yield return null;
        }
        RenderSettings.ambientLight = target;
    }

    IEnumerator TampilkanOverlay(string judul, string sub, Color bgColor, Color judulColor)
    {
        var (canvasGO, bgImg, judulTMP, subTMP) = BangunOverlay(bgColor, judulColor);
        judulTMP.text = judul;
        subTMP.text   = sub;

        var playerComp = playerTransform?.GetComponent<player>();
        if (playerComp != null) playerComp.frozen = true;

        // Fade in
        for (float t = 0f; t < overlayDurasiTransisi; t += Time.deltaTime)
        { SetAlpha(bgImg, judulTMP, subTMP, t / overlayDurasiTransisi); yield return null; }
        SetAlpha(bgImg, judulTMP, subTMP, 1f);

        yield return new WaitForSeconds(overlayDurasiTampil);

        // Fade out
        for (float t = 0f; t < overlayDurasiTransisi; t += Time.deltaTime)
        { SetAlpha(bgImg, judulTMP, subTMP, 1f - t / overlayDurasiTransisi); yield return null; }

        if (playerComp != null) playerComp.frozen = false;
        Destroy(canvasGO);
    }

    (GameObject, Image, TextMeshProUGUI, TextMeshProUGUI)
        BangunOverlay(Color bgColor, Color judulColor)
    {
        var cGO = new GameObject("PathOverlayCanvas");
        var cv  = cGO.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 900;
        var sc = cGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1080f, 1920f);
        sc.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // Background fullscreen
        var bgGO = new GameObject("BG");
        bgGO.transform.SetParent(cGO.transform, false);
        var bgRT = bgGO.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = bgColor;

        // Box tengah
        var boxGO = new GameObject("Box");
        boxGO.transform.SetParent(cGO.transform, false);
        var boxRT = boxGO.AddComponent<RectTransform>();
        boxRT.anchorMin = new Vector2(0f, 0.35f);
        boxRT.anchorMax = new Vector2(1f, 0.65f);
        boxRT.offsetMin = Vector2.zero;
        boxRT.offsetMax = Vector2.zero;

        BuatGaris(boxGO, new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.90f));
        var judulTMP = BuatTeks(boxGO, "Judul",
            new Vector2(0f, 0.45f), new Vector2(1f, 0.88f),
            96, judulColor, FontStyles.Bold);
        BuatGaris(boxGO, new Vector2(0.05f, 0.40f), new Vector2(0.95f, 0.42f));
        var subTMP = BuatTeks(boxGO, "Sub",
            new Vector2(0f, 0.10f), new Vector2(1f, 0.40f),
            44, overlayWarnaSub, FontStyles.Normal);

        return (cGO, bgImg, judulTMP, subTMP);
    }

    TextMeshProUGUI BuatTeks(GameObject parent, string nama,
        Vector2 ancMin, Vector2 ancMax, int size, Color color, FontStyles style)
    {
        var go = new GameObject(nama);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        var f = overlayFont
            ?? TMP_Settings.defaultFontAsset
            ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (f != null) tmp.font = f;
        tmp.fontSize = size; tmp.color = color; tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        return tmp;
    }

    void BuatGaris(GameObject parent, Vector2 ancMin, Vector2 ancMax)
    {
        var go = new GameObject("Garis");
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = overlayWarnaGaris;
    }

    void SetAlpha(Image bg, TextMeshProUGUI j, TextMeshProUGUI s, float a)
    {
        if (bg != null) { var c = bg.color; c.a = a * 0.92f; bg.color = c; }
        if (j  != null) { var c = j.color;  c.a = a;         j.color  = c; }
        if (s  != null) { var c = s.color;  c.a = a * 0.9f;  s.color  = c; }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (jalanRamai != null)
        {
            Gizmos.color = new Color(0.15f, 0.85f, 0.3f, 0.5f);
            Gizmos.DrawWireCube(jalanRamai.transform.position, Vector3.one * 2f);
            UnityEditor.Handles.Label(
                jalanRamai.transform.position + Vector3.up * 2f, "JALAN RAMAI");
        }
        if (gangSepi != null)
        {
            Gizmos.color = new Color(0.9f, 0.2f, 0.2f, 0.5f);
            Gizmos.DrawWireCube(gangSepi.transform.position, Vector3.one * 2f);
            UnityEditor.Handles.Label(
                gangSepi.transform.position + Vector3.up * 2f, "GANG SEPI");
        }
    }
#endif
}
