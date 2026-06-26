using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// KontrolHint — petunjuk kontrol singkat untuk pemain baru.
///
/// Tampil otomatis sekali saat player mulai bisa bergerak di Hari 1
/// (menunggu player.frozen = false), lalu menghilang sendiri setelah
/// pemain bergerak cukup jauh ATAU setelah durasi maksimum.
///
/// Hint menyesuaikan platform: keyboard di desktop, sentuh di HP.
/// Tidak menyentuh script lain — cukup tempel di GameObject kosong "[KontrolHint]".
/// </summary>
public class KontrolHint : MonoBehaviour
{
    [Header("Aktif")]
    public bool tampilkanHint = true;
    [Tooltip("Hanya tampil sekali per sesi main (tidak berulang saat ganti hari).")]
    public bool hanyaSekaliPerSesi = true;

    [Header("Teks (kosong = otomatis per platform)")]
    [TextArea(2, 4)]
    public string teksKeyboard = "";
    [TextArea(2, 4)]
    public string teksSentuh = "";

    [Header("Waktu")]
    [Tooltip("Durasi maksimum hint tampil (detik), walau pemain diam.")]
    public float durasiMaks = 9f;
    [Tooltip("Jarak horizontal yang harus ditempuh pemain agar hint hilang lebih cepat.")]
    public float jarakBubar = 3.5f;

    [Header("Tampilan")]
    public Color warnaPanel = new Color(0f, 0f, 0f, 0.78f);
    public Color warnaTeks  = new Color(1f, 0.97f, 0.88f, 1f);
    public int   ukuranFont = 30;
    [Tooltip("Posisi vertikal hint dari bawah layar (0 = bawah, 1 = atas).")]
    [Range(0f, 1f)] public float posisiY = 0.20f;
    public TMP_FontAsset fontAsset;

    [Header("Sorting")]
    public int sortingOrder = 950;

    // ── Runtime ──────────────────────────────────────────────────────────────
    private static bool _sudahTampilSesiIni;
    private static Sprite _spriteRound;

    void Start()
    {
        if (!tampilkanHint) return;
        if (hanyaSekaliPerSesi && _sudahTampilSesiIni) return;
        StartCoroutine(Rutin());
    }

    IEnumerator Rutin()
    {
        // Tunggu sampai player ada & sudah bisa digerakkan (tidak frozen)
        player pl = null;
        float batasTunggu = Time.time + 30f;
        while (pl == null && Time.time < batasTunggu)
        {
            pl = FindFirstObjectByType<player>();
            yield return null;
        }
        if (pl == null) yield break;

        // Tunggu kontrol diberikan (intro/narasi selesai)
        while (pl.frozen && Time.time < batasTunggu)
            yield return null;

        _sudahTampilSesiIni = true;

        // Game hanya untuk mobile → selalu tampilkan petunjuk kontrol sentuh
        string isi = string.IsNullOrEmpty(teksSentuh)
            ? "\uD83D\uDC46  Tombol panah kiri-bawah untuk JALAN  \u2022  tombol LARI & TERIAK di kanan-bawah"
            : teksSentuh;

        // Bangun UI
        var root = BangunUI(isi, out CanvasGroup cg);

        // Fade in (kecuali Reduce Motion)
        if (!GameSettings.ReduceMotion)
            yield return Fade(cg, 0f, 1f, 0.35f);
        else
            cg.alpha = 1f;

        // Tampil sampai pemain bergerak cukup jauh atau waktu habis
        float startX = pl.transform.position.x;
        float habis  = Time.time + durasiMaks;
        while (Time.time < habis)
        {
            if (pl == null) break;
            if (Mathf.Abs(pl.transform.position.x - startX) >= jarakBubar) break;
            yield return null;
        }

        // Fade out
        if (!GameSettings.ReduceMotion)
            yield return Fade(cg, 1f, 0f, 0.4f);

        if (root != null) Destroy(root);
    }

    IEnumerator Fade(CanvasGroup cg, float dari, float ke, float durasi)
    {
        float t = 0f;
        while (t < durasi)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(dari, ke, t / durasi);
            yield return null;
        }
        cg.alpha = ke;
    }

    GameObject BangunUI(string isi, out CanvasGroup cg)
    {
        var root = new GameObject("KontrolHintCanvas");
        var cv = root.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = sortingOrder;
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        root.AddComponent<GraphicRaycaster>();
        cg = root.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable   = false;
        cg.blocksRaycasts = false;   // jangan blok input game

        // Panel
        var panel = new GameObject("Panel", typeof(RectTransform));
        panel.transform.SetParent(root.transform, false);
        var pRT = panel.GetComponent<RectTransform>();
        pRT.anchorMin = new Vector2(0.5f, posisiY);
        pRT.anchorMax = new Vector2(0.5f, posisiY);
        pRT.pivot     = new Vector2(0.5f, 0.5f);
        pRT.sizeDelta = new Vector2(1400f, 84f);
        var pImg = panel.AddComponent<Image>();
        pImg.sprite = GetRoundedSprite();
        pImg.type   = Image.Type.Sliced;
        pImg.color  = warnaPanel;
        pImg.raycastTarget = false;

        // Teks
        var txtGO = new GameObject("Teks", typeof(RectTransform));
        txtGO.transform.SetParent(panel.transform, false);
        var tRT = txtGO.GetComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero;
        tRT.anchorMax = Vector2.one;
        tRT.offsetMin = new Vector2(30f, 8f);
        tRT.offsetMax = new Vector2(-30f, -8f);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = isi;
        tmp.fontSize  = ukuranFont;
        tmp.color     = warnaTeks;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        if (fontAsset != null) tmp.font = fontAsset;

        return root;
    }

    // ── Sprite sudut membulat (cache statis) ─────────────────────────────────
    static Sprite GetRoundedSprite()
    {
        if (_spriteRound != null) return _spriteRound;

        const int size = 48;
        const int radius = 16;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int yy = 0; yy < size; yy++)
        for (int xx = 0; xx < size; xx++)
        {
            bool inside = true;
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
