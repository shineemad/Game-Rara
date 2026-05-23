using UnityEngine;

/// <summary>
/// Singleton — Mengelola BGM dan SFX.
/// Persist di semua scene (DontDestroyOnLoad).
///
/// Setup di Inspector:
///   bgmSource  → AudioSource untuk musik latar (loop, volume rendah)
///   sfxSource  → AudioSource untuk efek suara (PlayOneShot)
///   bgmClips   → [0]=menu  [1]=day1  [2]=day2  [3]=day3  [4]=boss  [5]=result
///   sfx*       → masing-masing AudioClip untuk efek
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("BGM (urutan: menu, day1, day2, day3, boss, result)")]
    public AudioClip[] bgmClips;

    [Header("SFX")]
    public AudioClip sfxClick;
    public AudioClip sfxCorrect;
    public AudioClip sfxWrong;
    public AudioClip sfxBossHit;
    public AudioClip sfxBossGroan;
    public AudioClip sfxVictory;
    public AudioClip sfxNeutral;

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
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // BGM
    // ══════════════════════════════════════════════════════════════════════

    public enum BGMTrack { Menu = 0, Day1 = 1, Day2 = 2, Day3 = 3, Boss = 4, Result = 5 }

    /// Putar BGM berdasarkan enum track.
    public void PlayBGM(BGMTrack track, float volume = 0.7f)
    {
        int idx = (int)track;
        if (bgmClips == null || idx >= bgmClips.Length || bgmClips[idx] == null)
            return;

        if (bgmSource.clip == bgmClips[idx] && bgmSource.isPlaying)
            return; // sudah diputar

        bgmSource.clip   = bgmClips[idx];
        bgmSource.volume = volume;
        bgmSource.loop   = true;
        bgmSource.Play();
    }

    /// Ganti BGM dengan fade keluar → fade masuk.
    public void SwitchBGM(BGMTrack track, float volume = 0.7f)
    {
        StartCoroutine(CrossFadeBGM(track, volume));
    }

    System.Collections.IEnumerator CrossFadeBGM(BGMTrack track, float volume)
    {
        // Fade out
        float startVol = bgmSource.volume;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            bgmSource.volume = Mathf.Lerp(startVol, 0f, t);
            yield return null;
        }
        bgmSource.Stop();

        PlayBGM(track, 0f);

        // Fade in
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            bgmSource.volume = Mathf.Lerp(0f, volume, t);
            yield return null;
        }
        bgmSource.volume = volume;
    }

    public void StopBGM() => bgmSource.Stop();

    // ══════════════════════════════════════════════════════════════════════
    // SFX
    // ══════════════════════════════════════════════════════════════════════

    void PlaySFX(AudioClip clip)
    {
        if (clip != null) sfxSource.PlayOneShot(clip);
    }

    public void Click()      => PlaySFX(sfxClick);
    public void Correct()    => PlaySFX(sfxCorrect);
    public void Wrong()      => PlaySFX(sfxWrong);
    public void BossHit()    => PlaySFX(sfxBossHit);
    public void BossGroan()  => PlaySFX(sfxBossGroan);
    public void Victory()    => PlaySFX(sfxVictory);
    public void Neutral()    => PlaySFX(sfxNeutral);
}
