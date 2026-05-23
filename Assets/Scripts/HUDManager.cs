using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Mengelola tampilan HUD: nyawa (hati), skor, dan nama lokasi.
/// Letakkan pada Canvas di tiap scene gameplay (Day1/Day2/Day3).
///
/// Setup di Inspector:
///   heartImages    → Array Image (3 buah) — sprite penuh vs kosong
///   heartFull      → Sprite hati penuh (merah)
///   heartEmpty     → Sprite hati kosong (abu-abu)
///   scoreText      → TMP untuk angka skor
///   locationText   → TMP untuk nama lokasi (mis. "Jalan Menuju Sekolah")
///   dayText        → TMP untuk "Hari 1/2/3"
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("Nyawa (Hati)")]
    public Image[] heartImages;
    public Sprite  heartFull;
    public Sprite  heartEmpty;

    [Header("Teks")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI locationText;
    public TextMeshProUGUI dayText;

    [Header("Lokasi Per Hari")]
    public string[] locationNames = {
        "Jalan Menuju Sekolah",
        "Angkot Jurusan Sekolah",
        "Parkiran SMP — Musim Hujan"
    };

    // ══════════════════════════════════════════════════════════════════════
    void Start()
    {
        Refresh();
    }

    // ══════════════════════════════════════════════════════════════════════
    /// Perbarui semua elemen HUD dari GameState.
    public void Refresh()
    {
        if (GameState.Instance == null) return;

        UpdateHearts(GameState.Instance.lives, GameState.Instance.maxLives);
        UpdateScore(GameState.Instance.score);
        UpdateLocation(GameState.Instance.day);
        UpdateDay(GameState.Instance.day);
    }

    public void UpdateHearts(int current, int max)
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null) continue;
            heartImages[i].sprite = (i < current) ? heartFull : heartEmpty;
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Skor: {score}";
    }

    public void UpdateLocation(int day)
    {
        if (locationText == null) return;
        int idx = Mathf.Clamp(day - 1, 0, locationNames.Length - 1);
        locationText.text = locationNames[idx];
    }

    public void UpdateDay(int day)
    {
        if (dayText != null)
            dayText.text = $"Hari {day}";
    }

    // ══════════════════════════════════════════════════════════════════════
    /// Animasikan kehilangan nyawa (kedipkan hati yang hilang).
    public void FlashHeartLost(int newLives)
    {
        UpdateHearts(newLives, GameState.Instance?.maxLives ?? 3);
        // Kedipkan hati terakhir yang hilang
        if (newLives < heartImages.Length)
            StartCoroutine(FlashImage(heartImages[newLives]));
    }

    System.Collections.IEnumerator FlashImage(Image img)
    {
        if (img == null) yield break;
        for (int i = 0; i < 3; i++)
        {
            img.color = new Color(1f, 0.3f, 0.3f);
            yield return new WaitForSeconds(0.15f);
            img.color = Color.white;
            yield return new WaitForSeconds(0.15f);
        }
    }
}
