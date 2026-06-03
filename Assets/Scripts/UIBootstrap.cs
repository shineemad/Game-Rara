using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// UIBootstrap — pastikan SEMUA tombol UI bisa diklik di scene apapun.
///
/// Berjalan otomatis sesudah setiap scene di-load (tanpa perlu di-attach ke GameObject).
/// Memperbaiki dua penyebab klasik tombol UI tidak merespons:
///
///   1) Tidak ada EventSystem di scene  → tombol tak menerima event sama sekali.
///   2) Canvas tidak punya GraphicRaycaster → raycast UI gagal pada canvas itu.
///
/// Jika sudah ada, script ini tidak melakukan apa-apa (idempotent & aman).
/// </summary>
public static class UIBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnSceneLoadedFirstTime()
    {
        EnsureForActiveScene();
        // Jaga juga jika ada scene tambahan di-load berikutnya.
        SceneManager.sceneLoaded -= OnSceneLoadedAdditive;
        SceneManager.sceneLoaded += OnSceneLoadedAdditive;
    }

    static void OnSceneLoadedAdditive(Scene s, LoadSceneMode m)
    {
        EnsureForActiveScene();
    }

    static void EnsureForActiveScene()
    {
        EnsureEventSystem();
        EnsureGraphicRaycastersOnAllCanvases();
    }

    // Pastikan ada EventSystem (+ StandaloneInputModule) di scene.
    static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        var existing = Object.FindFirstObjectByType<EventSystem>();
        if (existing != null) return;

        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
        Debug.LogWarning("[UIBootstrap] EventSystem tidak ditemukan — dibuat otomatis. Tombol UI sekarang bisa diklik.");
    }

    // Pastikan setiap Canvas punya GraphicRaycaster (tanpa ini, raycast UI gagal).
    static void EnsureGraphicRaycastersOnAllCanvases()
    {
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < canvases.Length; i++)
        {
            var c = canvases[i];
            // Hanya canvas root yang butuh GraphicRaycaster (canvas child mewarisi dari root).
            if (c.isRootCanvas && c.GetComponent<GraphicRaycaster>() == null)
            {
                c.gameObject.AddComponent<GraphicRaycaster>();
                Debug.LogWarning("[UIBootstrap] GraphicRaycaster ditambahkan ke Canvas '" + c.name + "'.");
            }
        }
    }
}
