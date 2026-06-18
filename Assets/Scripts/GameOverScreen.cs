using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// GameOverScreen — Layar "GAME OVER" terpusat untuk seluruh game.
///
/// Tampil otomatis saat nyawa pemain habis (dipicu oleh watchdog di GameState),
/// atau bisa dipanggil manual lewat <see cref="Show"/>. Dibangun procedural
/// (tanpa prefab), konsisten dengan gaya UI project lain.
///
/// Aman dipanggil berkali-kali: jika layar sudah tampil, panggilan berikutnya
/// diabaikan (idempotent) sehingga tidak pernah muncul ganda.
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    /// True selama layar Game Over sedang tampil.
    public static bool IsShowing { get; private set; }

    private static GameOverScreen _instance;

    // Sorting order sangat tinggi agar menutupi semua UI lain.
    private const int SORTING_ORDER = 30000;

    private CanvasGroup _grp;

    /// <summary>
    /// Tampilkan layar Game Over. Idempotent — aman dipanggil berkali-kali.
    /// </summary>
    /// <param name="pesan">Pesan opsional. Kosong = pakai pesan default.</param>
    public static void Show(string pesan = null)
    {
        if (IsShowing) return;
        IsShowing = true;

        var go = new GameObject("[GameOverScreen]");
        _instance = go.AddComponent<GameOverScreen>();
        _instance.Build(pesan);
    }

    // ══════════════════════════════════════════════════════════════════════
    void Build(string pesan)
    {
        // ── Canvas overlay fullscreen ──
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = SORTING_ORDER;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();
        PastikanEventSystem();

        _grp = gameObject.AddComponent<CanvasGroup>();
        _grp.alpha = 0f;

        // ── Latar gelap menutupi layar (blokir input ke UI di bawahnya) ──
        var bg = BuatImage(transform, "BG", new Color(0.04f, 0.02f, 0.05f, 0.92f));
        bg.raycastTarget = true;
        Stretch(bg.rectTransform);

        // ── Judul GAME OVER ──
        var judul = BuatTeks(transform, "Judul", "GAME OVER", 96,
            new Color(0.92f, 0.26f, 0.21f, 1f), FontStyles.Bold);
        judul.alignment = TextAlignmentOptions.Center;
        var jrt = judul.rectTransform;
        jrt.anchorMin = new Vector2(0.1f, 0.60f); jrt.anchorMax = new Vector2(0.9f, 0.82f);
        jrt.offsetMin = Vector2.zero; jrt.offsetMax = Vector2.zero;

        // ── Pesan ──
        string msg = string.IsNullOrEmpty(pesan)
            ? "Jangan takut, kejadian ini bukan salahmu.\nSegera laporkan dan ceritakan hal ini kepada\norang tua atau Guru BK di sekolahmu!"
            : pesan;
        var teks = BuatTeks(transform, "Pesan", msg, 34, new Color(1f, 0.95f, 0.9f, 1f), FontStyles.Italic);
        teks.alignment = TextAlignmentOptions.Center;
        teks.textWrappingMode = TextWrappingModes.Normal;
        var trt = teks.rectTransform;
        trt.anchorMin = new Vector2(0.15f, 0.42f); trt.anchorMax = new Vector2(0.85f, 0.58f);
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;

        // ── Tombol Main Lagi & Keluar ──
        BuatTombol("MainLagiBtn", "\u27F2  Main Lagi", new Color(0.15f, 0.68f, 0.38f, 1f),
            new Vector2(0.5f, 0.275f), MainLagi);
        BuatTombol("KeluarBtn", "\u2716  Keluar", new Color(0.45f, 0.22f, 0.22f, 1f),
            new Vector2(0.5f, 0.155f), Keluar);

        // Bekukan gameplay: hanya tombol Main Lagi & Keluar yang boleh diklik.
        // - timeScale 0 menghentikan gerak/fisika berbasis Time.deltaTime.
        // - player.frozen mematikan input gerak pemain.
        // - Mobile controls & tombol pause disembunyikan agar tak bisa ditekan.
        // (FadeIn pakai unscaledDeltaTime sehingga animasi tetap jalan saat timeScale 0.)
        Time.timeScale = 0f;
        var pl = FindFirstObjectByType<player>();
        if (pl != null) pl.frozen = true;
        if (MobileControls.Instance != null) MobileControls.Instance.forceHide = true;
        var pause = FindFirstObjectByType<PauseMenu>(FindObjectsInactive.Include);
        if (pause != null) pause.showMobilePauseButton = false;

        AudioManager.Instance?.Wrong();
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        const float durasi = 0.5f;
        while (t < durasi)
        {
            t += Time.unscaledDeltaTime;
            if (_grp != null) _grp.alpha = Mathf.Clamp01(t / durasi);
            yield return null;
        }
        if (_grp != null) _grp.alpha = 1f;
    }

    // ── Tombol: Ulangi permainan dari awal ──
    void MainLagi()
    {
        AudioManager.Instance?.Click();
        Time.timeScale = 1f;
        IsShowing = false;
        GameState.Instance?.Reset();

        // Reset flag statis yang TIDAK ikut ter-reset saat scene dimuat ulang,
        // supaya alur benar-benar mulai dari awal (MainMenu → Prolog → Hari 1).
        // - prologDone: MainMenu menonaktifkan komponen PrologScreen sebelum Start()-nya
        //   jalan, jadi PrologScreen.Start() (yang me-reset flag) tidak pernah dipanggil.
        //   Tanpa reset ini, Day1Intro mengira prolog sudah selesai lalu Hari 1 langsung
        //   jalan di belakang menu.
        // - SkipKeDay: pastikan tidak melompat ke Hari 2/3 dari sesi sebelumnya.
        PrologScreen.prologDone   = false;
        PrologScreen.SedangTampil = false;
        DayTransitionResumeFlag.SkipKeDay = 0;

        // Kembalikan kontrol mobile (singleton persisten — disembunyikan saat Game Over).
        if (MobileControls.Instance != null) MobileControls.Instance.forceHide = false;

        // Putar musik menu utama (game kembali ke layar menu setelah scene reload).
        AudioManager.Instance?.PlayBGM(AudioManager.BGMTrack.Menu);

        string scene = SceneManager.GetActiveScene().name;
        Destroy(gameObject);

        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene(scene);
        else                              SceneManager.LoadScene(scene);
    }

    // ── Tombol: Keluar dari game ──
    void Keluar()
    {
        AudioManager.Instance?.Click();
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnDestroy()
    {
        if (_instance == this) { _instance = null; IsShowing = false; }
    }

    // ══════════════════════════════════════════════════════════════════════
    // HELPER UI (procedural)
    // ══════════════════════════════════════════════════════════════════════
    void BuatTombol(string nama, string label, Color warna, Vector2 anchorCenter, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(nama);
        go.transform.SetParent(transform, false);
        var img = go.AddComponent<Image>();
        img.sprite = RoundedSprite();
        img.type   = Image.Type.Sliced;
        img.color  = warna;
        var outl = go.AddComponent<Outline>();
        outl.effectColor    = new Color(1f, 1f, 1f, 0.25f);
        outl.effectDistance = new Vector2(2f, -2f);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorCenter; rt.anchorMax = anchorCenter;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(420f, 84f);
        rt.anchoredPosition = Vector2.zero;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        var lab = BuatTeks(go.transform, "Label", label, 30, Color.white, FontStyles.Bold);
        lab.alignment = TextAlignmentOptions.Center;
        Stretch(lab.rectTransform);
    }

    Image BuatImage(Transform parent, string nama, Color warna)
    {
        var go = new GameObject(nama);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = warna;
        return img;
    }

    TextMeshProUGUI BuatTeks(Transform parent, string nama, string isi, int size, Color warna, FontStyles style)
    {
        var go = new GameObject(nama);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
        tmp.text = isi; tmp.fontSize = size; tmp.color = warna; tmp.fontStyle = style;
        tmp.raycastTarget = false;
        return tmp;
    }

    void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    void PastikanEventSystem()
    {
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    // Sprite kotak rounded sederhana (sekali buat, di-cache).
    private static Sprite _roundedSprite;
    Sprite RoundedSprite()
    {
        if (_roundedSprite != null) return _roundedSprite;
        int size = 64, radius = 14;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
        Color32 putih = new Color32(255, 255, 255, 255), kosong = new Color32(255, 255, 255, 0);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                bool inside = true;
                if      (x < radius && y < radius)               { int dx = radius - x, dy = radius - y;               inside = dx * dx + dy * dy <= radius * radius; }
                else if (x >= size - radius && y < radius)       { int dx = x - (size - 1 - radius), dy = radius - y;   inside = dx * dx + dy * dy <= radius * radius; }
                else if (x < radius && y >= size - radius)       { int dx = radius - x, dy = y - (size - 1 - radius);   inside = dx * dx + dy * dy <= radius * radius; }
                else if (x >= size - radius && y >= size - radius){ int dx = x - (size - 1 - radius), dy = y - (size - 1 - radius); inside = dx * dx + dy * dy <= radius * radius; }
                tex.SetPixel(x, y, inside ? (Color)putih : (Color)kosong);
            }
        tex.Apply();
        _roundedSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
            100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        return _roundedSprite;
    }
}
