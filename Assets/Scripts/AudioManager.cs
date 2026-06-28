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
    [Tooltip("Bunyi ketik per huruf saat dialog mengetik (typewriter). Kosong = pakai sfxChatKetik.")]
    public AudioClip sfxKetik;
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

    // ── State terakhir BGM/ambience. Dipakai untuk MEMUTAR ULANG musik setelah
    //    perangkat audio berganti (mis. VoiceMeter / minigame Hari 2-3 memanggil
    //    Microphone.Start/End yang memicu Unity menghentikan semua AudioSource). ──
    bool      _bgmAktif;
    BGMTrack  _bgmTrackSekarang;
    float     _bgmVolumeSekarang = 0.7f;
    bool      _ambienceAktif;
    AudioClip _ambienceClipSekarang;
    float     _ambienceVolumeSekarang = 0.4f;

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            AktifkanSemuaSumber();
            // Putar ulang BGM bila perangkat audio berubah (mic dimulai/dihentikan).
            AudioSettings.OnAudioConfigurationChanged += OnPerangkatAudioBerubah;
            // Pemantau yang memulihkan BGM bila mati diam-diam (lihat PantauBgm).
            StartCoroutine(PantauBgm());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Bersihkan referensi singleton saat objek ini dihancurkan agar Instance tidak
    // menunjuk ke AudioManager/AudioSource yang sudah di-destroy (mencegah
    // MissingReferenceException saat scene di-reload pada game satu-scene).
    void OnDestroy()
    {
        if (Instance == this)
        {
            AudioSettings.OnAudioConfigurationChanged -= OnPerangkatAudioBerubah;
            Instance = null;
        }
    }

    // Pastikan GameObject & seluruh AudioSource AKTIF agar BGM/SFX selalu bisa
    // diputar sejak awal. Mencegah error "Can not play a disabled audio source"
    // bila checkbox komponen AudioSource sempat ter-nonaktif di Inspector.
    void AktifkanSemuaSumber()
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        if (bgmSource      != null && !bgmSource.enabled)      bgmSource.enabled      = true;
        if (sfxSource      != null && !sfxSource.enabled)      sfxSource.enabled      = true;
        if (ambienceSource != null && !ambienceSource.enabled) ambienceSource.enabled = true;

        // Jaminan audio global tidak ter-pause. Bila suatu kondisi sempat men-set
        // AudioListener.pause = true (mis. dari pause/alt-tab) dan tidak dikembalikan,
        // SELURUH audio (BGM + SFX) jadi senyap. Pastikan selalu false di sini.
        if (AudioListener.pause) AudioListener.pause = false;
    }

    // Dipanggil Unity saat perangkat/konfigurasi audio berganti. Saat itu Unity
    // MENGHENTIKAN semua AudioSource yang sedang berbunyi (penyebab BGM menu &
    // bagian lain mendadak hilang ketika mikrofon VoiceMeter mulai merekam).
    // Kita putar ulang BGM & ambience terakhir agar musik kembali berjalan.
    void OnPerangkatAudioBerubah(bool perangkatBerubah)
    {
        AktifkanSemuaSumber();

        if (_bgmAktif && bgmSource != null && bgmClips != null)
        {
            int idx = (int)_bgmTrackSekarang;
            if (idx >= 0 && idx < bgmClips.Length && bgmClips[idx] != null)
            {
                bgmSource.clip   = bgmClips[idx];
                bgmSource.volume = _bgmVolumeSekarang;
                bgmSource.loop   = true;
                bgmSource.Play();
            }
        }

        if (_ambienceAktif && ambienceSource != null && _ambienceClipSekarang != null)
        {
            ambienceSource.clip   = _ambienceClipSekarang;
            ambienceSource.volume = _ambienceVolumeSekarang;
            ambienceSource.loop   = true;
            ambienceSource.Play();
        }
    }

    // Watchdog BGM. BGM bisa "mati diam-diam" karena banyak sebab di luar kendali
    // kita: VoiceMeter / minigame Hari 2-3 me-restart perangkat audio (Microphone
    // Start/End), aplikasi kehilangan fokus, atau perangkat audio berganti — dan
    // tidak semua kasus memicu OnAudioConfigurationChanged. Pemantau ringan (cek tiap
    // 0.5 dtk waktu-nyata) memastikan BGM yang seharusnya berbunyi selalu dipulihkan.
    System.Collections.IEnumerator PantauBgm()
    {
        var tunggu = new WaitForSecondsRealtime(0.5f);
        while (true)
        {
            yield return tunggu;
            if (_bgmAktif && !AudioListener.pause
                && bgmSource != null && bgmSource.enabled
                && !bgmSource.mute && bgmSource.clip != null
                && !bgmSource.isPlaying)
            {
                bgmSource.Play();
            }
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
        if (bgmSource == null) return;

        // Pastikan komponen AudioSource AKTIF. Bila checkbox AudioSource sempat
        // ter-nonaktif, bgmSource.Play() gagal "Can not play a disabled audio
        // source" sehingga BGM tidak berjalan sama sekali. Aktifkan ulang.
        if (!bgmSource.enabled) bgmSource.enabled = true;

        // Catat track aktif agar bisa diputar ulang bila perangkat audio berganti.
        _bgmAktif          = true;
        _bgmTrackSekarang  = track;
        _bgmVolumeSekarang = volume;

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

    public void StopBGM()
    {
        _bgmAktif = false;
        if (bgmSource != null) bgmSource.Stop();
    }

    // ══════════════════════════════════════════════════════════════════════
    // SFX
    // ══════════════════════════════════════════════════════════════════════

    void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null) sfxSource.PlayOneShot(clip);
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

    // ── Bunyi ketik dialog (typewriter) ───────────────────────────────────
    // Dipanggil per huruf saat dialog mengetik. Di-throttle supaya tidak
    // terdengar seperti senapan mesin & volume dikecilkan agar halus.
    float _ketikTerakhir = -99f;
    public void PlayKetikHuruf()
    {
        if (Time.unscaledTime - _ketikTerakhir < 0.045f) return;
        _ketikTerakhir = Time.unscaledTime;
        AudioClip c = sfxKetik != null ? sfxKetik
                    : (sfxChatKetik != null ? sfxChatKetik
                    : (sfxChatMasuk != null ? sfxChatMasuk : sfxClick));
        if (c != null && sfxSource != null) sfxSource.PlayOneShot(c, 0.5f);
    }

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
        // Catat ambience aktif agar bisa diputar ulang bila perangkat audio berganti.
        _ambienceAktif          = true;
        _ambienceClipSekarang   = pakai;
        _ambienceVolumeSekarang = volume;
        if (ambienceSource.clip == pakai && ambienceSource.isPlaying) return;
        ambienceSource.clip   = pakai;
        ambienceSource.volume = volume;
        ambienceSource.loop   = true;
        ambienceSource.Play();
    }

    /// Hentikan ambience loop.
    public void StopAmbience()
    {
        _ambienceAktif = false;
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

#if UNITY_EDITOR
    // ══════════════════════════════════════════════════════════════════════
    // EDITOR — AUTO-ASSIGN AUDIO LANGSUNG DARI FOLDER Assets/sounds
    // ──────────────────────────────────────────────────────────────────────
    // Memetakan setiap field AudioClip + bgmClips[] ke file di Assets/sounds
    // berdasarkan NAMA FILE (tanpa ekstensi, abaikan huruf besar/kecil).
    // Pakai bila wiring di Inspector kosong / ter-revert: klik kanan komponen
    // AudioManager → pilih menu di bawah, lalu Save Scene (Ctrl+S).
    // Tidak mengubah API/field apa pun — hanya MENGISI referensi clip.
    // ══════════════════════════════════════════════════════════════════════

    // Dipanggil otomatis saat komponen pertama kali ditambahkan ke GameObject.
    void Reset() => MuatSemuaAudioDariSounds(overwrite: false);

    [ContextMenu("\u25B6 Muat Audio dari folder sounds (isi yang KOSONG saja)")]
    void MuatAudioKosongMenu() => MuatSemuaAudioDariSounds(overwrite: false);

    [ContextMenu("\u25B6 Muat ULANG semua Audio dari folder sounds (TIMPA)")]
    void MuatAudioTimpaMenu() => MuatSemuaAudioDariSounds(overwrite: true);

    /// <summary>
    /// Cari & assign seluruh AudioClip dari Assets/sounds (rekursif).
    /// overwrite=false hanya mengisi field yang masih null; true menimpa semua.
    /// </summary>
    void MuatSemuaAudioDariSounds(bool overwrite)
    {
        // Index semua AudioClip di Assets/sounds berdasar nama file (case-insensitive).
        var index = new System.Collections.Generic.Dictionary<string, AudioClip>(
            System.StringComparer.OrdinalIgnoreCase);
        foreach (var guid in UnityEditor.AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/sounds" }))
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            string nama = System.IO.Path.GetFileNameWithoutExtension(path);
            if (!index.ContainsKey(nama))
                index[nama] = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }

        AudioClip Cari(params string[] kandidat)
        {
            foreach (var n in kandidat)
                if (index.TryGetValue(n, out var clip) && clip != null) return clip;
            return null;
        }

        void Set(ref AudioClip field, params string[] kandidat)
        {
            if (!overwrite && field != null) return;
            var c = Cari(kandidat);
            if (c != null) field = c;
        }

        // ── BGM: [0]menu [1]day1 [2]day2 [3]day3 [4]boss [5]result ──────────
        if (overwrite || bgmClips == null || bgmClips.Length < 6)
        {
            var lama = bgmClips;
            bgmClips = new AudioClip[6];
            if (!overwrite && lama != null)
                for (int i = 0; i < lama.Length && i < 6; i++) bgmClips[i] = lama[i];
        }
        void SetBgm(int i, params string[] kandidat)
        {
            if (!overwrite && bgmClips[i] != null) return;
            var c = Cari(kandidat);
            if (c != null) bgmClips[i] = c;
        }
        SetBgm(0, "Menu");
        SetBgm(1, "Day1-Jalan", "Day1");
        SetBgm(2, "Day2-Angkot", "Day2");
        SetBgm(3, "Day3-Parkiran", "Day3");
        SetBgm(4, "Boss Fight", "BossFight");
        SetBgm(5, "Result");

        // ── SFX Umum ────────────────────────────────────────────────────────
        Set(ref sfxClick,     "sfxClick", "SFXclick");
        Set(ref sfxCorrect,   "sfxCorrect");
        Set(ref sfxWrong,     "sfxWrong");
        Set(ref sfxBossHit,   "sfxBossHit");
        Set(ref sfxBossGroan, "sfxBossGroan");
        Set(ref sfxVictory,   "sfxVictory");
        Set(ref sfxNeutral,   "sfxNeutral");

        // ── SFX Kategori Pilihan Dialog ──────────────────────────────────────
        Set(ref sfxAman,        "sfxAman");
        Set(ref sfxRagu,        "sfxRagu");
        Set(ref sfxBahaya,      "sfxBahaya");
        Set(ref sfxLapor,       "sfxLapor");
        Set(ref sfxAchievement, "sfxAchievement");

        // ── SFX Hari 2 (Angkot & Chat) ───────────────────────────────────────
        Set(ref sfxChatMasuk, "sfxChatMasuk");
        Set(ref sfxChatKetik, "sfxChatKetik");
        Set(ref sfxKetik,     "sfxketik", "sfxKetik");
        Set(ref sfxAngkot,    "sfxAngkot");
        Set(ref sfxPeluit,    "sfxPeluit");
        Set(ref sfxLangkah,   "sfxLangkah");

        // ── Ambience (loop) ──────────────────────────────────────────────────
        Set(ref ambienceAngkot, "Ambience Angkot", "ambienceAngkot");
        Set(ref ambienceHujan,  "ambienceHujan");

        // ── SFX Umum Tambahan (Nyawa, Game Over, Tegang) ────────────────────
        Set(ref sfxKehilanganNyawa, "sfxKehilanganNyawa");
        Set(ref sfxGameOver,        "sfxGameOver");
        Set(ref sfxDetakJantung,    "sfxDetakJantung");
        Set(ref sfxTimerTik,        "sfxTimerTik");
        Set(ref sfxTeriakCharge,    "sfxTeriakCharge");

        // ── SFX Hari 2 Tambahan ──────────────────────────────────────────────
        Set(ref sfxAngkotPintu, "sfxAngkotPintu");
        Set(ref sfxDragAmbil,   "sfxDragAmbil");
        Set(ref sfxDragLepas,   "sfxDragLepas");

        // ── SFX Hari 3 (Hujan & Boss) ────────────────────────────────────────
        Set(ref sfxPetir,      "sfxPetir");
        Set(ref sfxPanicAlarm, "sfxPanicAlarm");
        Set(ref sfxMotorOjol,  "sfxMotorOjol");
        Set(ref sfxBossKalah,  "sfxBossKalah");

        // ── SFX Polish (Prolog, Skor, Kartu, Lencana) ───────────────────────
        Set(ref sfxSlideProlog, "sfxSlideProlog");
        Set(ref sfxSkorNaik,    "sfxSkorNaik");
        Set(ref sfxMuncul,      "sfxMuncul");
        Set(ref sfxMunculKartu, "sfxMunculKartu");
        Set(ref sfxKlikLanjut,  "sfxKlikLanjut");
        Set(ref sfxKlikUlangi,  "sfxKlikUlangi");
        Set(ref sfxUnlock,      "sfxUnlock");

        UnityEditor.EditorUtility.SetDirty(this);
        if (!Application.isPlaying)
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        Debug.Log("[AudioManager] Audio dimuat dari Assets/sounds (timpa=" + overwrite
                + "). Total klip terindeks: " + index.Count + ". Jangan lupa Save Scene.");
    }
#endif
}
