using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ResponsiveCanvasFixer — penyetel responsif TERPUSAT untuk semua UI.
///
/// Game ini membangun puluhan Canvas secara prosedural di banyak script dengan
/// setelan CanvasScaler yang tidak seragam (sebagian referensi landscape
/// 1920×1080, sebagian portrait 1080×1920) dan kebanyakan TIDAK menyetel
/// screenMatchMode. Akibatnya di HP/tablet dengan rasio berbeda UI bisa
/// terpotong atau berukuran tidak konsisten.
///
/// Komponen ini berjalan otomatis (bootstrap [RuntimeInitializeOnLoadMethod] →
/// tidak perlu dipasang manual ke scene) dan secara berkala menormalkan SEMUA
/// CanvasScaler bertipe ScaleWithScreenSize agar:
///   • screenMatchMode = Expand  → seluruh area referensi DIJAMIN muat, tidak
///     ada UI yang terpotong pada rasio layar apa pun (16:9, 18:9, 19.5:9,
///     20:9, tablet 4:3, dst).
///   • referensi portrait (tinggi > lebar) dinormalkan ke landscape 1920×1080
///     karena game ini dikunci landscape — menghilangkan UI yang menciut/melar.
///
/// Pemindaian memakai Coroutine ringan (bukan Update) setiap beberapa ratus ms
/// untuk menangkap Canvas baru yang dibuat saat berpindah hari/scene.
/// </summary>
public class ResponsiveCanvasFixer : MonoBehaviour
{
    // Referensi landscape standar (sesuai defaultScreenWidth/Height proyek).
    static readonly Vector2 RefLandscape = new Vector2(1920f, 1080f);

    // Interval pemindaian Canvas baru (detik). Cukup jarang agar hemat.
    const float IntervalPindai = 0.4f;

    // Lacak CanvasScaler yang sudah diproses agar tidak dikerjakan berulang.
    readonly HashSet<int> _sudahDiproses = new HashSet<int>();

    static ResponsiveCanvasFixer _instance;

    // ── Bootstrap otomatis: spawn singleton sebelum scene pertama dimuat ──────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (_instance != null) return;
        var go = new GameObject("[ResponsiveCanvasFixer]");
        _instance = go.AddComponent<ResponsiveCanvasFixer>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()  => StartCoroutine(RutinPindai());

    IEnumerator RutinPindai()
    {
        var tunggu = new WaitForSecondsRealtime(IntervalPindai);
        while (true)
        {
            ProsesSemuaScaler();
            yield return tunggu;
        }
    }

    void ProsesSemuaScaler()
    {
        // Sertakan yang inactive agar Canvas yang baru disiapkan ikut dinormalkan.
        var scalers = FindObjectsByType<CanvasScaler>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < scalers.Length; i++)
        {
            var sc = scalers[i];
            if (sc == null) continue;

            int id = sc.GetInstanceID();
            if (_sudahDiproses.Contains(id)) continue;

            // Hanya sentuh scaler responsif (ScaleWithScreenSize). Biarkan mode
            // ConstantPixelSize/ConstantPhysicalSize apa adanya.
            if (sc.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                // Anti-terpotong: jamin seluruh area referensi tampil.
                sc.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

                // Normalkan referensi portrait → landscape (game dikunci landscape).
                var r = sc.referenceResolution;
                if (r.y > r.x)
                    sc.referenceResolution = RefLandscape;
            }

            _sudahDiproses.Add(id);
        }
    }
}
