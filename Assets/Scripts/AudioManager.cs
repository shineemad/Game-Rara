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

    [Header("SFX — Kategori Pilihan Dialog")]
    [Tooltip("SFX saat pemain pilih jawaban AMAN (jingle positif singkat). Kosong = pakai sfxCorrect.")]
    public AudioClip sfxAman;
    [Tooltip("SFX saat pemain pilih jawaban RAGU (nada netral). Kosong = pakai sfxNeutral.")]
    public AudioClip sfxRagu;
    [Tooltip("SFX saat pemain pilih jawaban BAHAYA (buzzer rendah). Kosong = pakai sfxWrong.")]
    public AudioClip sfxBahaya;
    [Tooltip("SFX saat pemain klik tombol LAPOR untuk recovery skor.")]
    public AudioClip sfxLapor;
    [Tooltip("SFX saat lencana / achievement diraih. Kosong = pakai sfxCorrect.")]
    public AudioClip sfxAchievement;

    [Header("SFX — Hari 2 (Angkot & Chat)")]
    [Tooltip("Bunyi notifikasi chat masuk (gaya WhatsApp 'ting') di ChatSim.")]
    public AudioClip sfxChatMasuk;
    [Tooltip("Bunyi 'tik' saat bubble chat selesai diketik / muncul. Kosong = pakai sfxChatMasuk.")]
    public AudioClip sfxChatKetik;
    [Tooltip("Suara mesin/klakson angkot pendek saat naik angkot.")]
    public AudioClip sfxAngkot;
    [Tooltip("Bunyi peluit / teriak nyaring untuk efek tombol TERIAK. Kosong = pakai sfxLapor.")]
    public AudioClip sfxPeluit;
    [Tooltip("Suara langkah kaki Rara saat berjalan.")]
    public AudioClip sfxLangkah;

    [Header("Ambience (loop) — Hari 2")]
    [Tooltip("AudioSource terpisah untuk suara latar suasana (angkot/jalan). Boleh dikosongkan.")]
    public AudioSource ambienceSource;
    [Tooltip("Klip ambience suasana angkot/jalan yang diputar berulang (loop) selama Hari 2.")]
    public AudioClip ambienceAngkot;

    [Header("SFX — Umum Tambahan (Nyawa, Game Over, Tegang)")]
    [Tooltip("Nyawa −1 saat pilihan BAHAYA. 'Heart break' / nada turun singkat.")]
    public AudioClip sfxKehilanganNyawa;
    [Tooltip("Layar Game Over saat nyawa habis. Sting kalah lembut, ramah anak.")]
    public AudioClip sfxGameOver;
    [Tooltip("Momen tegang (disentuh, pojok sepi, boss). Detak jantung 'dug-dug'.")]
    public AudioClip sfxDetakJantung;
    [Tooltip("Hitung mundur Quiz/ChatSim. 'Tik' jam per detik.")]
    public AudioClip sfxTimerTik;
    [Tooltip("Tahan tombol TERIAK / Voice Meter terisi. Nada naik 'power up'.")]
    public AudioClip sfxTeriakCharge;

    [Header("SFX — Hari 2 Tambahan")]
    [Tooltip("Naik / turun angkot. Pintu geser besi 'klek-srek'.")]
    public AudioClip sfxAngkotPintu;
    [Tooltip("Angkat chip di Quiz Zona Tubuh. 'Pluk' ambil ringan.")]
    public AudioClip sfxDragAmbil;
    [Tooltip("Jatuhkan chip ke zona. 'Tuk' tempel mantap.")]
    public AudioClip sfxDragLepas;

    [Header("SFX — Hari 3 (Hujan & Boss)")]
    [Tooltip("Aksen dramatis saat boss / keputusan. Guruh + petir.")]
    public AudioClip sfxPetir;
    [Tooltip("Tekan Panic Button (lapor darurat). Alarm / sirine pendek.")]
    public AudioClip sfxPanicAlarm;
    [Tooltip("Kedatangan ojol (palsu) Day 3. Mesin motor mendekat lalu idle.")]
    public AudioClip sfxMotorOjol;
    [Tooltip("Boss 'Si Bayangan Gelap' mundur / kalah. Lega + kemenangan kecil.")]
    public AudioClip sfxBossKalah;

    [Header("SFX — Polish (Prolog, Skor, Kartu, Lencana)")]
    [Tooltip("Pindah slide Prolog. Whoosh halaman geser.")]
    public AudioClip sfxSlideProlog;
    [Tooltip("Animasi skor bertambah di Summary. 'Tik-tik-tik' cepat naik nada.")]
    public AudioClip sfxSkorNaik;
    [Tooltip("Kartu / panel muncul. Whoosh lembut + pop.")]
    public AudioClip sfxMuncul;
    [Tooltip("Popup kartu muncul. Kosong = pakai sfxMuncul.")]
    public AudioClip sfxMunculKartu;
    [Tooltip("Klik tombol 'Lanjut'. Konfirmasi positif. Kosong = pakai sfxClick.")]
    public AudioClip sfxKlikLanjut;
    [Tooltip("Klik tombol 'Ulangi'. Klik netral/mundur. Kosong = pakai sfxClick.")]
    public AudioClip sfxKlikUlangi;
    [Tooltip("Lencana terbuka. Sparkle / chime ceria. Kosong = pakai sfxAchievement.")]
    public AudioClip sfxUnlock;

    [Header("Ambience (loop) — Hari 3")]
    [Tooltip("Hujan latar sepanjang Day 3 (loop). Diputar via ambienceSource.")]
    public AudioClip ambienceHujan;

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

    // ── Per Kategori Pilihan ──────────────────────────────────────────────
    public void PlayAman()        => PlaySFX(sfxAman        != null ? sfxAman        : sfxCorrect);
    public void PlayRagu()        => PlaySFX(sfxRagu        != null ? sfxRagu        : sfxNeutral);
    public void PlayBahaya()      => PlaySFX(sfxBahaya      != null ? sfxBahaya      : sfxWrong);
    public void PlayLapor()       => PlaySFX(sfxLapor       != null ? sfxLapor       : sfxCorrect);
    public void PlayAchievement() => PlaySFX(sfxAchievement != null ? sfxAchievement : sfxCorrect);

    // ── Hari 2: Angkot & Chat ─────────────────────────────────────────────
    public void PlayChatMasuk() => PlaySFX(sfxChatMasuk);
    public void PlayChatKetik() => PlaySFX(sfxChatKetik != null ? sfxChatKetik : sfxChatMasuk);
    public void PlayAngkot()    => PlaySFX(sfxAngkot);
    public void PlayPeluit()    => PlaySFX(sfxPeluit != null ? sfxPeluit : sfxLapor);
    public void PlayLangkah()   => PlaySFX(sfxLangkah);

    // ── Umum Tambahan (Nyawa, Game Over, Tegang) ──────────────────────────
    public void PlayKehilanganNyawa() => PlaySFX(sfxKehilanganNyawa);
    public void PlayGameOver()        => PlaySFX(sfxGameOver);
    public void PlayDetakJantung()    => PlaySFX(sfxDetakJantung);
    public void PlayTimerTik()        => PlaySFX(sfxTimerTik);
    public void PlayTeriakCharge()    => PlaySFX(sfxTeriakCharge);

    // ── Hari 2 Tambahan ───────────────────────────────────────────────────
    public void PlayAngkotPintu() => PlaySFX(sfxAngkotPintu);
    public void PlayDragAmbil()   => PlaySFX(sfxDragAmbil);
    public void PlayDragLepas()   => PlaySFX(sfxDragLepas);

    // ── Hari 3 (Hujan & Boss) ─────────────────────────────────────────────
    public void PlayPetir()      => PlaySFX(sfxPetir);
    public void PlayPanicAlarm() => PlaySFX(sfxPanicAlarm);
    public void PlayMotorOjol()  => PlaySFX(sfxMotorOjol);
    public void PlayBossKalah()  => PlaySFX(sfxBossKalah);

    // ── Polish (Prolog, Skor, Kartu, Lencana) ─────────────────────────────
    public void PlaySlideProlog() => PlaySFX(sfxSlideProlog);
    public void PlaySkorNaik()    => PlaySFX(sfxSkorNaik);
    public void PlayMuncul()      => PlaySFX(sfxMuncul);
    public void PlayMunculKartu() => PlaySFX(sfxMunculKartu != null ? sfxMunculKartu : sfxMuncul);
    public void PlayKlikLanjut()  => PlaySFX(sfxKlikLanjut  != null ? sfxKlikLanjut  : sfxClick);
    public void PlayKlikUlangi()  => PlaySFX(sfxKlikUlangi  != null ? sfxKlikUlangi  : sfxClick);
    public void PlayUnlock()      => PlaySFX(sfxUnlock      != null ? sfxUnlock      : sfxAchievement);

    // ── Ambience suasana (loop) ───────────────────────────────────────────
    /// Mulai ambience loop (mis. suasana angkot/jalan Hari 2).
    public void PlayAmbience(AudioClip clip = null, float volume = 0.4f)
    {
        if (ambienceSource == null) return;
        var pakai = clip != null ? clip : ambienceAngkot;
        if (pakai == null) return;
        if (ambienceSource.clip == pakai && ambienceSource.isPlaying) return;
        ambienceSource.clip   = pakai;
        ambienceSource.volume = volume;
        ambienceSource.loop   = true;
        ambienceSource.Play();
    }

    /// Hentikan ambience loop.
    public void StopAmbience()
    {
        if (ambienceSource != null) ambienceSource.Stop();
    }

    /// Mulai ambience hujan loop (Hari 3). Praktis = PlayAmbience(ambienceHujan).
    public void PlayAmbienceHujan(float volume = 0.4f) => PlayAmbience(ambienceHujan, volume);

    /// Putar SFX sesuai kategori "AMAN" | "RAGU" | "BAHAYA" | "LAPOR".
    public void PlayKategori(string kategori)
    {
        switch (kategori)
        {
            case "AMAN":   PlayAman();   break;
            case "RAGU":   PlayRagu();   break;
            case "BAHAYA": PlayBahaya(); break;
            case "LAPOR":  PlayLapor();  break;
            default:       Neutral();    break;
        }
    }
}
