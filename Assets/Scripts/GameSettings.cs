using System;
using UnityEngine;

/// <summary>
/// GameSettings — penyimpan preferensi pemain (volume, ukuran font, aksesibilitas)
/// yang persisten lewat PlayerPrefs. Dipakai oleh panel Pengaturan di PauseMenu.
///
/// Hanya VOLUME yang berefek global secara langsung (AudioListener.volume + mute BGM).
/// FontScale & ReduceMotion disimpan sebagai preferensi; UI yang dibangun ulang
/// boleh membacanya lewat properti statis di sini (mis. GameSettings.FontScale).
///
/// Cara pakai:
///   GameSettings.MasterVolume = 0.5f;   // langsung diterapkan + disimpan
///   float skala = GameSettings.FontScale;
///   GameSettings.OnChanged += () => { /* bangun ulang UI bila perlu */ };
/// </summary>
public static class GameSettings
{
    // ── Kunci PlayerPrefs ──────────────────────────────────────────────────
    const string KEY_VOLUME   = "rara_master_volume";
    const string KEY_MUSIC    = "rara_music_on";
    const string KEY_FONT     = "rara_font_scale";
    const string KEY_MOTION   = "rara_reduce_motion";

    /// Dipanggil setiap ada perubahan setting (agar UI bisa menyegarkan diri).
    public static event Action OnChanged;

    static bool  _loaded;
    static float _masterVolume = 1f;
    static bool  _musicOn      = true;
    static float _fontScale    = 1f;
    static bool  _reduceMotion = false;

    // ── Properti ───────────────────────────────────────────────────────────

    public static float MasterVolume
    {
        get { EnsureLoaded(); return _masterVolume; }
        set
        {
            EnsureLoaded();
            _masterVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(KEY_VOLUME, _masterVolume);
            PlayerPrefs.Save();
            TerapkanAudio();
            OnChanged?.Invoke();
        }
    }

    public static bool MusicOn
    {
        get { EnsureLoaded(); return _musicOn; }
        set
        {
            EnsureLoaded();
            _musicOn = value;
            PlayerPrefs.SetInt(KEY_MUSIC, _musicOn ? 1 : 0);
            PlayerPrefs.Save();
            TerapkanAudio();
            OnChanged?.Invoke();
        }
    }

    /// Skala ukuran font (1 = normal). Disarankan 1.0 – 1.4.
    public static float FontScale
    {
        get { EnsureLoaded(); return _fontScale; }
        set
        {
            EnsureLoaded();
            _fontScale = Mathf.Clamp(value, 0.8f, 1.6f);
            PlayerPrefs.SetFloat(KEY_FONT, _fontScale);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }
    }

    /// Aksesibilitas: kurangi animasi/gerakan untuk pemain yang sensitif.
    public static bool ReduceMotion
    {
        get { EnsureLoaded(); return _reduceMotion; }
        set
        {
            EnsureLoaded();
            _reduceMotion = value;
            PlayerPrefs.SetInt(KEY_MOTION, _reduceMotion ? 1 : 0);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }
    }

    // ── Internal ─────────────────────────────────────────────────────────────

    static void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;
        _masterVolume = PlayerPrefs.GetFloat(KEY_VOLUME, 1f);
        _musicOn      = PlayerPrefs.GetInt(KEY_MUSIC, 1) == 1;
        _fontScale    = PlayerPrefs.GetFloat(KEY_FONT, 1f);
        _reduceMotion = PlayerPrefs.GetInt(KEY_MOTION, 0) == 1;
        TerapkanAudio();
    }

    /// Terapkan setting audio ke engine (global). Aman dipanggil kapan saja.
    public static void TerapkanAudio()
    {
        AudioListener.volume = _masterVolume;
        var am = AudioManager.Instance;
        if (am != null && am.bgmSource != null)
            am.bgmSource.mute = !_musicOn;
    }

    /// Pastikan setting termuat & diterapkan di awal game (mis. dari splash).
    public static void Init() => EnsureLoaded();
}
