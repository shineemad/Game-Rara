using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Singleton — Mengelola pergantian scene dengan efek fade hitam.
/// Letakkan satu objek SceneLoader pada scene pertama (MainMenu).
///
/// Setup di Inspector:
///   fadePanel → Image fullscreen berwarna hitam (Alpha 0 saat mulai)
///   fadeDuration → durasi fade in/out (default 0.4 detik)
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Fade")]
    public Image  fadePanel;
    [Range(0.1f, 1.5f)]
    public float  fadeDuration = 0.4f;

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Pastikan fade panel tidak memblokir klik saat tidak dipakai
        if (fadePanel != null)
        {
            fadePanel.color      = new Color(0f, 0f, 0f, 0f);
            fadePanel.raycastTarget = false;
        }
    }

    void Start()
    {
        // Fade in saat scene pertama dimuat
        StartCoroutine(FadeIn());
    }

    // ══════════════════════════════════════════════════════════════════════
    // PUBLIC API
    // ══════════════════════════════════════════════════════════════════════

    /// Muat scene berdasarkan nama dengan efek fade.
    public void LoadScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    /// Muat scene berikutnya dalam Build Settings.
    public void LoadNextScene()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
            StartCoroutine(FadeAndLoad(next));
    }

    /// Muat ulang scene yang sedang aktif.
    public void ReloadCurrentScene()
    {
        StartCoroutine(FadeAndLoad(SceneManager.GetActiveScene().name));
    }

    // ══════════════════════════════════════════════════════════════════════
    // COROUTINES
    // ══════════════════════════════════════════════════════════════════════

    IEnumerator FadeAndLoad(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(sceneName);
        yield return StartCoroutine(FadeIn());
    }

    IEnumerator FadeAndLoad(int buildIndex)
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(buildIndex);
        yield return StartCoroutine(FadeIn());
    }

    IEnumerator FadeOut()
    {
        if (fadePanel == null) yield break;
        fadePanel.raycastTarget = true;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            fadePanel.color = new Color(0f, 0f, 0f, Mathf.Clamp01(t));
            yield return null;
        }
        fadePanel.color = new Color(0f, 0f, 0f, 1f);
    }

    IEnumerator FadeIn()
    {
        if (fadePanel == null) yield break;
        fadePanel.color = new Color(0f, 0f, 0f, 1f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            fadePanel.color = new Color(0f, 0f, 0f, 1f - Mathf.Clamp01(t));
            yield return null;
        }
        fadePanel.color         = new Color(0f, 0f, 0f, 0f);
        fadePanel.raycastTarget = false;
    }
}
