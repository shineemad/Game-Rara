using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Day3Controller — Orkestrator Hari 3: BOSS FIGHT melawan intimidasi (parkiran SMP, musim hujan).
///
/// Berbeda dari Day 1 (side-scroller) & Day 2 (rangkaian fase UI), Hari 3 adalah
/// "boss fight" verbal: Si Bully menjatuhkan mental Rara dengan ejekan/ancaman.
/// Pemain memilih respons AMAN/RAGU/BAHAYA tiap ronde. Pilihan AMAN menurunkan
/// "Mental Si Bully" paling besar (boss mundur), BAHAYA tidak melukai boss dan
/// pemain kehilangan 1 nyawa.
///
/// State machine:
///   BossIntro → Round1 → Round2 → Round3 → BossDefeated → EduCard → Complete
///
/// UI dibangun procedural (sesuai pola Day 2: AngkotSeatPicker / EduCardDay2).
/// Komponen ini SELF-CONTAINED — tidak butuh wiring referensi tambahan.
/// DayTransitionManager.LanjutKeDay3() cukup meng-enable GameObject ini; Start()
/// akan jalan otomatis saat GameState.day == 3.
///
/// Cara pakai:
///   1. GameObject → Create Empty → "Day3Controller" → Add Component Day3Controller.
///   2. Set GameObject DISABLE di Hierarchy, lalu masukkan ke DayTransitionManager.day3Objects.
///   3. (Opsional) Atur teks ronde, warna, sprite boss di Inspector.
/// </summary>
public class Day3Controller : MonoBehaviour
{
    public static Day3Controller Instance { get; private set; }

    public enum Phase
    {
        None,
        BossIntro,
        Round1,
        Round2,
        Round3,
        BossDefeated,
        EduCard,
        Complete
    }

    // ══════════════════════════════════════════════════════════════════════
    // DATA STRUCTURES
    // ══════════════════════════════════════════════════════════════════════

    [System.Serializable]
    public class PilihanRonde
    {
        public string label = "Respons Rara";
        public string kategori = "AMAN";                 // "AMAN" | "RAGU" | "BAHAYA"
        [Tooltip("Pengurangan Mental Si Bully (0-100). AMAN besar, RAGU sedang, BAHAYA 0.")]
        public float damage = 40f;
        [TextArea(2, 4)]
        public string reaksi = "\u2713 Bagus! Si Bully kehilangan nyali.";
    }

    [System.Serializable]
    public class Ronde
    {
        public string namaRonde = "Ronde 1";
        [TextArea(2, 5)]
        [Tooltip("Ejekan/ancaman dari Si Bully untuk ronde ini.")]
        public string ucapanBully = "\"Heh, anak baru! Sini sodorin uang jajanmu!\"";
        public PilihanRonde[] pilihan;
    }

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR
    // ══════════════════════════════════════════════════════════════════════

    [Header("Auto-Start")]
    [Tooltip("Mulai otomatis dari BossIntro saat GameObject di-enable & GameState.day == 3.")]
    public bool autoStart = true;

    [Header("Overlay Judul Hari (BossIntro)")]
    public bool   tampilkanOverlayJudul = true;
    public string barisPertama = "HARI 3";
    public string barisKedua   = "Hadapi Si Pengganggu";
    public string teksLokasi   = "Parkiran SMP \u2014 Musim Hujan";
    public Color  warnaBackground = new Color(0f, 0f, 0.04f, 0.92f);
    public Color  warnaTeksJudul  = new Color(0.95f, 0.78f, 0.10f, 1f);
    public Color  warnaTeksLokasi = Color.white;
    public int    ukuranFontJudul = 72;
    public int    ukuranFontLokasi = 34;
    public float  durasiTampilOverlay = 2.6f;
    public float  durasiTransisiOverlay = 0.5f;

    [Header("Narasi Pembuka (sebelum boss bicara)")]
    [TextArea(2, 5)]
    public string narasiPembuka =
        "Hujan turun di parkiran SMP. Seorang kakak kelas berbadan besar " +
        "menghadang langkah Rara sambil menyeringai...";

    [Header("Backdrop Procedural")]
    public bool buatBackdrop = true;
    public Color warnaBackdrop = new Color(0.10f, 0.12f, 0.18f, 1f); // suram, hujan
    public int   backdropSortingOrder = -100;

    [Header("Boss (Si Bully)")]
    public string bossNama = "Si Bully";
    [Tooltip("Sprite potret boss (opsional). Kosong = kotak warna solid.")]
    public Sprite bossSprite;
    public Color  bossWarnaFallback = new Color(0.35f, 0.10f, 0.12f, 1f);
    [Tooltip("Mental Si Bully maksimum. Boss kalah saat mental mencapai 0.")]
    public float  bossMentalMax = 100f;

    [Header("Boss HP Bar")]
    public string bossBarLabel = "Mental Si Bully";
    public Color  bossBarWarnaIsi  = new Color(0.85f, 0.20f, 0.22f, 1f);
    public Color  bossBarWarnaKosong = new Color(0.15f, 0.15f, 0.18f, 1f);

    [Header("Ronde Boss Fight (CUSTOMIZABLE)")]
    public Ronde[] rondeList = new Ronde[]
    {
        new Ronde {
            namaRonde   = "Ronde 1 \u2014 Palak Uang",
            ucapanBully = "\"Heh, anak baru! Sini sodorin semua uang jajanmu, cepat!\"",
            pilihan = new PilihanRonde[]
            {
                new PilihanRonde { label = "\"Nggak. Aku mau lapor guru.\"", kategori = "AMAN", damage = 40f,
                    reaksi = "\u2713 Tegas! Si Bully kaget kamu berani menolak." },
                new PilihanRonde { label = "\"Eh... aku nggak bawa uang kok...\"", kategori = "RAGU", damage = 20f,
                    reaksi = "\u26A0 Kurang tegas. Dia masih coba menekanmu." },
                new PilihanRonde { label = "Diam & serahkan uang jajan", kategori = "BAHAYA", damage = 0f,
                    reaksi = "\u2716 Dia makin pede. Kamu kehilangan 1 nyawa." }
            }
        },
        new Ronde {
            namaRonde   = "Ronde 2 \u2014 Ancaman",
            ucapanBully = "\"Awas ya, kalau berani ngadu, kamu bakal nyesel!\"",
            pilihan = new PilihanRonde[]
            {
                new PilihanRonde { label = "\"Mengancam itu salah. Aku tetap lapor.\"", kategori = "AMAN", damage = 40f,
                    reaksi = "\u2713 Mantap! Nyali Si Bully makin ciut." },
                new PilihanRonde { label = "\"I-iya deh, aku nggak ngadu...\"", kategori = "RAGU", damage = 20f,
                    reaksi = "\u26A0 Dia merasa kamu bisa ditakut-takuti." },
                new PilihanRonde { label = "Menangis & lari sembunyi sendirian", kategori = "BAHAYA", damage = 0f,
                    reaksi = "\u2716 Kamu makin terpojok. Kehilangan 1 nyawa." }
            }
        },
        new Ronde {
            namaRonde   = "Ronde 3 \u2014 Cari Bantuan",
            ucapanBully = "\"Mau apa kamu? Di sini cuma ada kita berdua!\"",
            pilihan = new PilihanRonde[]
            {
                new PilihanRonde { label = "Teriak \"TOLONG!\" & lari ke satpam", kategori = "AMAN", damage = 50f,
                    reaksi = "\u2713 Hebat! Satpam datang. Si Bully kabur!" },
                new PilihanRonde { label = "\"Aku... aku tunggu temanku aja deh.\"", kategori = "RAGU", damage = 20f,
                    reaksi = "\u26A0 Lumayan, tapi kamu masih ragu cari bantuan." },
                new PilihanRonde { label = "Ikut saja ke tempat sepi", kategori = "BAHAYA", damage = 0f,
                    reaksi = "\u2716 BAHAYA besar! Kamu kehilangan 1 nyawa." }
            }
        }
    };

    [Header("Saat Boss Kalah")]
    [TextArea(2, 5)]
    public string narasiBossKalah =
        "Satpam dan guru piket datang! Si Bully langsung kabur ketakutan. " +
        "Rara berhasil menjaga dirinya dengan berani dan cerdas.";
    [Tooltip("Achievement yang diraih saat boss dikalahkan.")]
    public string achievementMenang = "Penakluk Si Bully";

    [Header("Kartu Edukasi Hari 3")]
    public string eduJudul = "\uD83D\uDCDA  KARTU EDUKASI \u2014 HARI 3";
    public Color  eduWarnaJudul = new Color(1f, 0.85f, 0.30f, 1f);
    [TextArea(4, 10)]
    public string eduIsi =
        "\uD83D\uDEE1 Menghadapi Perundungan (Bullying):\n" +
        "\u2022 Tetap tenang & berani berkata \"TIDAK\" dengan tegas.\n" +
        "\u2022 Jangan menyerahkan barang karena takut \u2014 itu bukan salahmu.\n" +
        "\u2022 Cari tempat ramai & orang dewasa terpercaya (guru, satpam, ortu).\n" +
        "\u2022 Ancaman \"jangan ngadu\" justru tanda kamu HARUS lapor.\n" +
        "\u2022 Simpan bukti (chat/foto) bila ada.\n\n" +
        "\u260E Hotline: Polisi 110  |  Hotline Anak 129  |  KPAI 021-31901556";

    [Header("Layar Hasil Akhir (Complete)")]
    public string hasilJudul = "\uD83C\uDFC1  TANTANGAN SELESAI!";
    [Tooltip("Ambang skor untuk pesan penutup (mengikuti CLAUDE.md).")]
    public int ambangLuarBiasa = 800;
    public int ambangBagus     = 500;
    public string pesanLuarBiasa = "Luar Biasa! Kamu sangat waspada dan berani menjaga diri.";
    public string pesanBagus     = "Bagus! Kamu cukup berhati-hati menjaga diri.";
    public string pesanKurang    = "Kamu masih perlu belajar cara menjaga diri. Ayo coba lagi!";

    [Header("Warna & Font Umum")]
    public Color warnaAman   = new Color(0.15f, 0.68f, 0.38f, 1f);
    public Color warnaRagu   = new Color(0.95f, 0.61f, 0.07f, 1f);
    public Color warnaBahaya = new Color(0.91f, 0.30f, 0.24f, 1f);
    public Color warnaNetral = new Color(0.20f, 0.60f, 0.86f, 1f);
    public TMP_FontAsset fontAsset;

    [Header("Sorting")]
    public int sortingOrder = 930;

    [Header("Debug")]
    public bool debugLog = true;

    // ── runtime ───────────────────────────────────────────────────────────
    private Phase      _fase = Phase.None;
    private float      _bossMental;
    private bool       _sudahMulai;
    private GameObject _backdropGO;
    private GameObject _canvasGO;
    private Image      _bossImg;
    private Image      _bossBarFill;
    private TextMeshProUGUI _bossBarText;
    private TextMeshProUGUI _bossNamaText;
    private TextMeshProUGUI _ucapanText;
    private TextMeshProUGUI _reaksiText;
    private GameObject _pilihanPanel;
    private bool       _menungguPilihan;

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    IEnumerator Start()
    {
        var gs = GameState.Instance;

        // Sama seperti Day2Controller: jangan hijack hari lain. Hanya jalan kalau
        // pemain memang sudah di Hari 3 (DayTransitionManager set day=3 sebelum enable).
        if (autoStart && gs != null && gs.day != 3)
        {
            if (debugLog) Debug.Log("[Day3Controller] day=" + gs.day + " (\u2260 3). Menunggu LanjutKeDay3().");
            yield break;
        }

        yield return null; // biarkan Start() lain selesai dulu
        if (autoStart) TriggerStart();
    }

    /// <summary>Mulai Hari 3 secara eksplisit. Idempotent.</summary>
    public void TriggerStart()
    {
        if (_sudahMulai) return;
        _sudahMulai = true;

        var gs = GameState.Instance;
        if (gs != null) gs.day = 3;

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.Refresh();
            HUDManager.Instance.UpdateLocationCustom(teksLokasi);
        }

        if (AudioManager.Instance != null)
        {
            try { AudioManager.Instance.PlayBGM(AudioManager.BGMTrack.Boss); }
            catch { /* fallback diam */ }
        }

        _bossMental = bossMentalMax;

        if (buatBackdrop) BuildBackdrop();
        PastikanEventSystem();

        GotoFase(Phase.BossIntro);
    }

    // ══════════════════════════════════════════════════════════════════════
    // STATE MACHINE
    // ══════════════════════════════════════════════════════════════════════
    public Phase CurrentPhase => _fase;

    public void GotoFase(Phase next)
    {
        StopAllCoroutines();
        StartCoroutine(RunFase(next));
    }

    IEnumerator RunFase(Phase next)
    {
        _fase = next;
        if (debugLog) Debug.Log("[Day3Controller] \u2192 Fase: " + next);

        switch (next)
        {
            case Phase.BossIntro:
                if (tampilkanOverlayJudul) yield return TampilkanOverlayJudul();
                BuildArena();
                yield return TampilkanUcapan(narasiPembuka, isNarasi: true);
                yield return new WaitForSeconds(0.4f);
                GotoFase(Phase.Round1);
                break;

            case Phase.Round1:
                yield return JalankanRonde(0, Phase.Round2);
                break;

            case Phase.Round2:
                yield return JalankanRonde(1, Phase.Round3);
                break;

            case Phase.Round3:
                yield return JalankanRonde(2, Phase.BossDefeated);
                break;

            case Phase.BossDefeated:
                yield return TampilkanBossKalah();
                GotoFase(Phase.EduCard);
                break;

            case Phase.EduCard:
                yield return TampilkanEduCard();
                GotoFase(Phase.Complete);
                break;

            case Phase.Complete:
                TampilkanHasilAkhir();
                break;
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // RONDE
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator JalankanRonde(int idx, Phase lanjutKe)
    {
        if (rondeList == null || idx >= rondeList.Length || rondeList[idx] == null)
        {
            GotoFase(lanjutKe);
            yield break;
        }

        var ronde = rondeList[idx];

        // Boss bicara
        if (_bossNamaText != null) _bossNamaText.text = bossNama;
        yield return TampilkanUcapan(ronde.ucapanBully, isNarasi: false);

        // Tampilkan pilihan & tunggu
        PilihanRonde dipilih = null;
        _menungguPilihan = true;
        TampilkanPilihan(ronde.pilihan, p => { dipilih = p; _menungguPilihan = false; });
        while (_menungguPilihan) yield return null;

        // Proses hasil pilihan
        ProsesPilihan(dipilih);

        // Reaksi
        if (_reaksiText != null) _reaksiText.text = dipilih.reaksi;

        // Update bar mental
        UpdateBossBar();

        yield return new WaitForSeconds(1.6f);
        if (_reaksiText != null) _reaksiText.text = "";

        // Game over kalau nyawa habis
        if (GameState.Instance != null && !GameState.Instance.IsAlive())
        {
            yield return TampilkanGameOver();
            yield break;
        }

        // Kalau mental boss sudah 0 sebelum ronde habis → langsung kalah
        if (_bossMental <= 0f)
        {
            GotoFase(Phase.BossDefeated);
            yield break;
        }

        GotoFase(lanjutKe);
    }

    void ProsesPilihan(PilihanRonde p)
    {
        if (p == null) return;
        var gs = GameState.Instance;

        gs?.AddChoice(3, p.label, p.kategori);
        AudioManager.Instance?.PlayKategori(p.kategori);

        if (p.damage > 0f)
        {
            _bossMental = Mathf.Max(0f, _bossMental - p.damage);
            AudioManager.Instance?.BossHit();
        }

        if (p.kategori == "BAHAYA")
        {
            gs?.LoseLife();
            if (HUDManager.Instance != null && gs != null)
                HUDManager.Instance.UpdateHearts(gs.lives, gs.maxLives);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // BOSS KALAH / GAME OVER
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator TampilkanBossKalah()
    {
        _bossMental = 0f;
        UpdateBossBar();

        if (!string.IsNullOrEmpty(achievementMenang))
            GameState.Instance?.EarnAchievement(achievementMenang);

        AudioManager.Instance?.Victory();

        if (_pilihanPanel != null) _pilihanPanel.SetActive(false);
        if (_bossNamaText != null) _bossNamaText.text = "";
        yield return TampilkanUcapan(narasiBossKalah, isNarasi: true);
        yield return new WaitForSeconds(0.8f);
    }

    IEnumerator TampilkanGameOver()
    {
        if (_pilihanPanel != null) _pilihanPanel.SetActive(false);
        if (_reaksiText != null) _reaksiText.text = "";
        if (_bossNamaText != null) _bossNamaText.text = "";

        AudioManager.Instance?.Wrong();
        yield return TampilkanUcapan(
            "Nyawa Rara habis. Tapi jangan menyerah \u2014 setiap keputusan adalah pelajaran. Ayo coba lagi!",
            isNarasi: true);

        // Tampilkan langsung layar hasil (dengan skor apa adanya)
        _fase = Phase.Complete;
        TampilkanHasilAkhir();
    }

    // ══════════════════════════════════════════════════════════════════════
    // OVERLAY JUDUL
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator TampilkanOverlayJudul()
    {
        var go = new GameObject("Day3_OverlayJudul");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder + 60;
        AddScaler(go);
        go.AddComponent<GraphicRaycaster>();

        var bg = BuatImage(go.transform, "BG", warnaBackground);
        Stretch(bg.rectTransform);
        var grp = go.AddComponent<CanvasGroup>();
        grp.alpha = 0f;

        var judul = BuatTeks(go.transform, "Judul", barisPertama + "\n" + barisKedua,
            ukuranFontJudul, warnaTeksJudul, FontStyles.Bold);
        judul.alignment = TextAlignmentOptions.Center;
        var jrt = judul.rectTransform;
        jrt.anchorMin = new Vector2(0.1f, 0.45f); jrt.anchorMax = new Vector2(0.9f, 0.75f);
        jrt.offsetMin = Vector2.zero; jrt.offsetMax = Vector2.zero;

        var lokasi = BuatTeks(go.transform, "Lokasi", teksLokasi,
            ukuranFontLokasi, warnaTeksLokasi, FontStyles.Italic);
        lokasi.alignment = TextAlignmentOptions.Center;
        var lrt = lokasi.rectTransform;
        lrt.anchorMin = new Vector2(0.1f, 0.32f); lrt.anchorMax = new Vector2(0.9f, 0.45f);
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;

        // fade in
        yield return Fade(grp, 0f, 1f, durasiTransisiOverlay);
        yield return new WaitForSeconds(durasiTampilOverlay);
        yield return Fade(grp, 1f, 0f, durasiTransisiOverlay);

        Destroy(go);
    }

    IEnumerator Fade(CanvasGroup grp, float from, float to, float durasi)
    {
        float t = 0f;
        while (t < durasi)
        {
            t += Time.deltaTime;
            grp.alpha = Mathf.Lerp(from, to, durasi > 0f ? t / durasi : 1f);
            yield return null;
        }
        grp.alpha = to;
    }

    // ══════════════════════════════════════════════════════════════════════
    // ARENA UI (boss + bar + kotak ucapan + panel pilihan)
    // ══════════════════════════════════════════════════════════════════════
    void BuildArena()
    {
        if (_canvasGO != null) return;

        _canvasGO = new GameObject("Day3_Arena");
        var canvas = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        AddScaler(_canvasGO);
        _canvasGO.AddComponent<GraphicRaycaster>();

        // Boss portrait (tengah atas)
        var bossGO = new GameObject("Boss");
        bossGO.transform.SetParent(_canvasGO.transform, false);
        _bossImg = bossGO.AddComponent<Image>();
        _bossImg.sprite = bossSprite;
        _bossImg.color  = bossSprite != null ? Color.white : bossWarnaFallback;
        _bossImg.preserveAspect = true;
        _bossImg.raycastTarget = false;
        var brt = bossGO.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.5f, 0.5f); brt.anchorMax = new Vector2(0.5f, 0.5f);
        brt.pivot = new Vector2(0.5f, 0.5f);
        brt.sizeDelta = new Vector2(360f, 420f);
        brt.anchoredPosition = new Vector2(0f, 150f);

        // Nama boss
        _bossNamaText = BuatTeks(_canvasGO.transform, "BossNama", bossNama, 30, warnaBahaya, FontStyles.Bold);
        _bossNamaText.alignment = TextAlignmentOptions.Center;
        var nrt = _bossNamaText.rectTransform;
        nrt.anchorMin = new Vector2(0.5f, 1f); nrt.anchorMax = new Vector2(0.5f, 1f);
        nrt.pivot = new Vector2(0.5f, 1f); nrt.sizeDelta = new Vector2(700f, 50f);
        nrt.anchoredPosition = new Vector2(0f, -24f);

        // Boss HP Bar
        BuildBossBar();

        // Kotak ucapan (narasi / ucapan boss)
        var box = BuatImage(_canvasGO.transform, "KotakUcapan", new Color(0f, 0f, 0f, 0.78f));
        box.raycastTarget = false;
        var boxRt = box.rectTransform;
        boxRt.anchorMin = new Vector2(0.5f, 0f); boxRt.anchorMax = new Vector2(0.5f, 0f);
        boxRt.pivot = new Vector2(0.5f, 0f);
        boxRt.sizeDelta = new Vector2(1500f, 200f);
        boxRt.anchoredPosition = new Vector2(0f, 320f);

        _ucapanText = BuatTeks(box.transform, "Ucapan", "", 28, Color.white, FontStyles.Normal);
        _ucapanText.alignment = TextAlignmentOptions.Center;
        Stretch(_ucapanText.rectTransform, 40f, 20f);

        // Reaksi (di atas kotak ucapan)
        _reaksiText = BuatTeks(_canvasGO.transform, "Reaksi", "", 24, new Color(1f, 1f, 0.85f, 1f), FontStyles.Italic);
        _reaksiText.alignment = TextAlignmentOptions.Center;
        var rrt = _reaksiText.rectTransform;
        rrt.anchorMin = new Vector2(0.5f, 0f); rrt.anchorMax = new Vector2(0.5f, 0f);
        rrt.pivot = new Vector2(0.5f, 0f); rrt.sizeDelta = new Vector2(1500f, 70f);
        rrt.anchoredPosition = new Vector2(0f, 540f);

        // Panel pilihan
        _pilihanPanel = new GameObject("PilihanPanel");
        _pilihanPanel.transform.SetParent(_canvasGO.transform, false);
        var prt = _pilihanPanel.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.5f, 0f); prt.anchorMax = new Vector2(0.5f, 0f);
        prt.pivot = new Vector2(0.5f, 0f);
        prt.sizeDelta = new Vector2(1500f, 280f);
        prt.anchoredPosition = new Vector2(0f, 30f);
        _pilihanPanel.SetActive(false);

        UpdateBossBar();
    }

    void BuildBossBar()
    {
        var barBg = BuatImage(_canvasGO.transform, "BossBar_BG", bossBarWarnaKosong);
        barBg.raycastTarget = false;
        var bgRt = barBg.rectTransform;
        bgRt.anchorMin = new Vector2(0.5f, 1f); bgRt.anchorMax = new Vector2(0.5f, 1f);
        bgRt.pivot = new Vector2(0.5f, 1f);
        bgRt.sizeDelta = new Vector2(900f, 44f);
        bgRt.anchoredPosition = new Vector2(0f, -84f);

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(barBg.transform, false);
        _bossBarFill = fillGO.AddComponent<Image>();
        _bossBarFill.color = bossBarWarnaIsi;
        _bossBarFill.raycastTarget = false;
        _bossBarFill.type = Image.Type.Filled;
        _bossBarFill.fillMethod = Image.FillMethod.Horizontal;
        _bossBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        _bossBarFill.sprite = SolidSprite();
        _bossBarFill.fillAmount = 1f;
        var fRt = _bossBarFill.rectTransform;
        Stretch(fRt, 3f, 3f);

        _bossBarText = BuatTeks(barBg.transform, "BarLabel", bossBarLabel, 20, Color.white, FontStyles.Bold);
        _bossBarText.alignment = TextAlignmentOptions.Center;
        Stretch(_bossBarText.rectTransform);
    }

    void UpdateBossBar()
    {
        if (_bossBarFill != null)
            _bossBarFill.fillAmount = bossMentalMax > 0f ? Mathf.Clamp01(_bossMental / bossMentalMax) : 0f;
        if (_bossBarText != null)
            _bossBarText.text = bossBarLabel + "  " + Mathf.RoundToInt(_bossMental) + "/" + Mathf.RoundToInt(bossMentalMax);
    }

    // ══════════════════════════════════════════════════════════════════════
    // UCAPAN (type effect) & PILIHAN
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator TampilkanUcapan(string teks, bool isNarasi)
    {
        if (_ucapanText == null) yield break;
        if (_bossNamaText != null) _bossNamaText.text = isNarasi ? "" : bossNama;
        _ucapanText.fontStyle = isNarasi ? FontStyles.Italic : FontStyles.Bold;
        _ucapanText.text = "";
        foreach (char c in teks)
        {
            _ucapanText.text += c;
            yield return new WaitForSeconds(0.018f);
        }
        // jeda baca singkat untuk narasi
        if (isNarasi) yield return new WaitForSeconds(0.6f);
    }

    void TampilkanPilihan(PilihanRonde[] pilihan, Action<PilihanRonde> onPilih)
    {
        if (_pilihanPanel == null) return;
        _pilihanPanel.SetActive(true);

        foreach (Transform child in _pilihanPanel.transform) Destroy(child.gameObject);

        if (pilihan == null || pilihan.Length == 0) return;

        float slotH = 1f / pilihan.Length;
        for (int i = 0; i < pilihan.Length; i++)
        {
            var p = pilihan[i];
            float yMax = 1f - i * slotH;
            float yMin = yMax - slotH + 0.03f;

            var btnObj = new GameObject("Pilihan_" + i);
            btnObj.transform.SetParent(_pilihanPanel.transform, false);
            var img = btnObj.AddComponent<Image>();
            img.color = WarnaKategori(p.kategori);
            img.sprite = SolidSprite();
            img.type = Image.Type.Sliced;
            var bRt = btnObj.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0f, yMin); bRt.anchorMax = new Vector2(1f, yMax);
            bRt.offsetMin = new Vector2(0f, 4f); bRt.offsetMax = new Vector2(0f, -4f);

            var btn = btnObj.AddComponent<Button>();
            var captured = p;
            btn.onClick.AddListener(() =>
            {
                AudioManager.Instance?.Click();
                _pilihanPanel.SetActive(false);
                onPilih?.Invoke(captured);
            });

            var label = BuatTeks(btnObj.transform, "Label", p.label, 24, Color.white, FontStyles.Bold);
            label.alignment = TextAlignmentOptions.Center;
            Stretch(label.rectTransform, 24f, 6f);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // EDU CARD
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator TampilkanEduCard()
    {
        bool lanjut = false;

        var go = new GameObject("Day3_EduCard");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder + 40;
        AddScaler(go);
        go.AddComponent<GraphicRaycaster>();

        var overlay = BuatImage(go.transform, "Overlay", new Color(0f, 0f, 0f, 0.82f));
        Stretch(overlay.rectTransform);

        var panel = BuatImage(go.transform, "Panel", new Color(0.05f, 0.10f, 0.08f, 0.98f));
        panel.sprite = SolidSprite(); panel.type = Image.Type.Sliced;
        var pRt = panel.rectTransform;
        pRt.anchorMin = new Vector2(0.5f, 0.5f); pRt.anchorMax = new Vector2(0.5f, 0.5f);
        pRt.pivot = new Vector2(0.5f, 0.5f);
        pRt.sizeDelta = new Vector2(1200f, 760f);

        var judul = BuatTeks(panel.transform, "Judul", eduJudul, 34, eduWarnaJudul, FontStyles.Bold);
        judul.alignment = TextAlignmentOptions.Center;
        var jRt = judul.rectTransform;
        jRt.anchorMin = new Vector2(0f, 1f); jRt.anchorMax = new Vector2(1f, 1f);
        jRt.pivot = new Vector2(0.5f, 1f); jRt.sizeDelta = new Vector2(0f, 70f);
        jRt.anchoredPosition = new Vector2(0f, -30f);

        var isi = BuatTeks(panel.transform, "Isi", eduIsi, 24, new Color(1f, 1f, 0.9f, 1f), FontStyles.Normal);
        isi.alignment = TextAlignmentOptions.TopLeft;
        var iRt = isi.rectTransform;
        iRt.anchorMin = new Vector2(0f, 0f); iRt.anchorMax = new Vector2(1f, 1f);
        iRt.offsetMin = new Vector2(60f, 120f); iRt.offsetMax = new Vector2(-60f, -120f);

        var btn = BuatTombol(panel.transform, "\u25B6  LANJUT", warnaAman, () => lanjut = true);
        var tRt = ((RectTransform)btn.transform);
        tRt.anchorMin = new Vector2(0.5f, 0f); tRt.anchorMax = new Vector2(0.5f, 0f);
        tRt.pivot = new Vector2(0.5f, 0f); tRt.sizeDelta = new Vector2(340f, 70f);
        tRt.anchoredPosition = new Vector2(0f, 30f);

        AudioManager.Instance?.PlayAchievement();

        while (!lanjut) yield return null;
        Destroy(go);
    }

    // ══════════════════════════════════════════════════════════════════════
    // HASIL AKHIR
    // ══════════════════════════════════════════════════════════════════════
    void TampilkanHasilAkhir()
    {
        var gs = GameState.Instance;

        if (AudioManager.Instance != null)
        {
            try { AudioManager.Instance.PlayBGM(AudioManager.BGMTrack.Result); }
            catch { }
        }

        var go = new GameObject("Day3_Hasil");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder + 80;
        AddScaler(go);
        go.AddComponent<GraphicRaycaster>();

        var bg = BuatImage(go.transform, "BG", new Color(0.04f, 0.06f, 0.12f, 0.98f));
        Stretch(bg.rectTransform);

        // Judul
        var judul = BuatTeks(go.transform, "Judul", hasilJudul, 48, warnaTeksJudul, FontStyles.Bold);
        judul.alignment = TextAlignmentOptions.Center;
        var jRt = judul.rectTransform;
        jRt.anchorMin = new Vector2(0.1f, 0.80f); jRt.anchorMax = new Vector2(0.9f, 0.92f);
        jRt.offsetMin = Vector2.zero; jRt.offsetMax = Vector2.zero;

        int skor = gs != null ? gs.score : 0;
        string grade = gs != null ? gs.Grade() : "";

        var skorText = BuatTeks(go.transform, "Skor",
            "Total Skor: <b>" + skor + "</b>\n" + grade,
            34, Color.white, FontStyles.Normal);
        skorText.alignment = TextAlignmentOptions.Center;
        var sRt = skorText.rectTransform;
        sRt.anchorMin = new Vector2(0.1f, 0.62f); sRt.anchorMax = new Vector2(0.9f, 0.80f);
        sRt.offsetMin = Vector2.zero; sRt.offsetMax = Vector2.zero;

        // Ringkasan pilihan
        var ringkasanText = BuatTeks(go.transform, "Ringkasan", RingkasPilihan(gs),
            22, new Color(1f, 1f, 0.9f, 1f), FontStyles.Normal);
        ringkasanText.alignment = TextAlignmentOptions.Top;
        var rRt = ringkasanText.rectTransform;
        rRt.anchorMin = new Vector2(0.15f, 0.30f); rRt.anchorMax = new Vector2(0.85f, 0.62f);
        rRt.offsetMin = Vector2.zero; rRt.offsetMax = Vector2.zero;

        // Pesan penutup
        string pesan = skor >= ambangLuarBiasa ? pesanLuarBiasa
                     : skor >= ambangBagus     ? pesanBagus
                     : pesanKurang;
        var pesanText = BuatTeks(go.transform, "Pesan", pesan, 26, warnaTeksJudul, FontStyles.Italic);
        pesanText.alignment = TextAlignmentOptions.Center;
        var pRt = pesanText.rectTransform;
        pRt.anchorMin = new Vector2(0.1f, 0.20f); pRt.anchorMax = new Vector2(0.9f, 0.30f);
        pRt.offsetMin = Vector2.zero; pRt.offsetMax = Vector2.zero;

        // Tombol Main Lagi
        var btnMain = BuatTombol(go.transform, "\uD83D\uDD04  MAIN LAGI", warnaAman, MainLagi);
        var bmRt = (RectTransform)btnMain.transform;
        bmRt.anchorMin = new Vector2(0.5f, 0f); bmRt.anchorMax = new Vector2(0.5f, 0f);
        bmRt.pivot = new Vector2(1f, 0f); bmRt.sizeDelta = new Vector2(320f, 72f);
        bmRt.anchoredPosition = new Vector2(-20f, 80f);

        // Tombol Keluar
        var btnKeluar = BuatTombol(go.transform, "\u274C  KELUAR", warnaNetral, Keluar);
        var bkRt = (RectTransform)btnKeluar.transform;
        bkRt.anchorMin = new Vector2(0.5f, 0f); bkRt.anchorMax = new Vector2(0.5f, 0f);
        bkRt.pivot = new Vector2(0f, 0f); bkRt.sizeDelta = new Vector2(320f, 72f);
        bkRt.anchoredPosition = new Vector2(20f, 80f);

        _fase = Phase.Complete;
    }

    string RingkasPilihan(GameState gs)
    {
        if (gs == null || gs.choices == null || gs.choices.Count == 0)
            return "Belum ada pilihan tercatat.";

        int aman = 0, ragu = 0, bahaya = 0;
        var redFlags = new List<string>();
        foreach (var c in gs.choices)
        {
            switch (c.category)
            {
                case "AMAN": aman++; break;
                case "RAGU": ragu++; break;
                case "BAHAYA": bahaya++; redFlags.Add("\u2716 " + c.label); break;
            }
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Ringkasan Keputusan:");
        sb.AppendLine("<color=#26AD61>AMAN: " + aman + "</color>   " +
                      "<color=#F29D12>RAGU: " + ragu + "</color>   " +
                      "<color=#E84D3D>BAHAYA: " + bahaya + "</color>");
        if (redFlags.Count > 0)
        {
            sb.AppendLine("\n<color=#E84D3D>Red Flag (perlu diperbaiki):</color>");
            int n = Mathf.Min(redFlags.Count, 4);
            for (int i = 0; i < n; i++) sb.AppendLine(redFlags[i]);
        }
        else
        {
            sb.AppendLine("\n<color=#26AD61>Tidak ada keputusan berbahaya. Hebat!</color>");
        }
        return sb.ToString();
    }

    void MainLagi()
    {
        AudioManager.Instance?.Click();
        GameState.Instance?.Reset();
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void Keluar()
    {
        AudioManager.Instance?.Click();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ══════════════════════════════════════════════════════════════════════
    // BACKDROP
    // ══════════════════════════════════════════════════════════════════════
    void BuildBackdrop()
    {
        if (_backdropGO != null) return;
        _backdropGO = new GameObject("Day3_Backdrop");
        var canvas = _backdropGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = backdropSortingOrder;
        AddScaler(_backdropGO);

        var bg = BuatImage(_backdropGO.transform, "BG", warnaBackdrop);
        bg.raycastTarget = false;
        Stretch(bg.rectTransform);

        // strip lantai gelap untuk depth
        var floor = BuatImage(_backdropGO.transform, "Floor", new Color(0f, 0f, 0f, 0.40f));
        floor.raycastTarget = false;
        var fRt = floor.rectTransform;
        fRt.anchorMin = new Vector2(0f, 0f); fRt.anchorMax = new Vector2(1f, 0.28f);
        fRt.offsetMin = Vector2.zero; fRt.offsetMax = Vector2.zero;
    }

    // ══════════════════════════════════════════════════════════════════════
    // HELPER UI
    // ══════════════════════════════════════════════════════════════════════
    Color WarnaKategori(string kategori) => kategori switch
    {
        "AMAN"   => warnaAman,
        "RAGU"   => warnaRagu,
        "BAHAYA" => warnaBahaya,
        _        => warnaNetral
    };

    void AddScaler(GameObject go)
    {
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
    }

    Image BuatImage(Transform parent, string nama, Color warna)
    {
        var go = new GameObject(nama);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = warna;
        return img;
    }

    TextMeshProUGUI BuatTeks(Transform parent, string nama, string teks, int ukuran, Color warna, FontStyles style)
    {
        var go = new GameObject(nama);
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = teks;
        t.fontSize = ukuran;
        t.color = warna;
        t.fontStyle = style;
        t.raycastTarget = false;
        if (fontAsset != null) t.font = fontAsset;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return t;
    }

    GameObject BuatTombol(Transform parent, string teks, Color warna, Action onClick)
    {
        var go = new GameObject("Tombol");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = warna;
        img.sprite = SolidSprite();
        img.type = Image.Type.Sliced;
        var btn = go.AddComponent<Button>();
        btn.onClick.AddListener(() => onClick?.Invoke());

        var label = BuatTeks(go.transform, "Label", teks, 26, Color.white, FontStyles.Bold);
        label.alignment = TextAlignmentOptions.Center;
        return go;
    }

    void Stretch(RectTransform rt, float padH = 0f, float padV = 0f)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(padH, padV);
        rt.offsetMax = new Vector2(-padH, -padV);
    }

    // Sprite putih 1x1 (untuk Image type Sliced/Filled supaya bisa diwarnai).
    private static Sprite _solid;
    Sprite SolidSprite()
    {
        if (_solid != null) return _solid;
        var tex = Texture2D.whiteTexture;
        _solid = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        return _solid;
    }

    void PastikanEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }
}
