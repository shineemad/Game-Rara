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
        IntroPembuka,
        JalanHujan,
        ChatAgresif,
        OjolPalsu,
        BossIntro,
        BossKonfrontasi,
        Round1,
        Round2,
        Round3,
        BossDefeated,
        EduCard,
        Complete
    }

    /// <summary>Hasil akhir Hari 3 (menentukan ending).</summary>
    public enum HasilDay3
    {
        Lanjut,       // belum final (mis. pilihan diam → boss mendesak lagi)
        Aman,         // lolos selamat — "Sekolah heroik"
        Trauma,       // gagal — "Game Over" + restart
        LaporSukses   // panic button / lapor — achievement "Pahlawan Diri Sendiri"
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
        [Tooltip("Poin bonus tambahan di luar skor kategori (mis. +100 bukti foto plat).")]
        public int bonusPoin = 0;
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

    /// <summary>Satu opsi pada konfrontasi pamungkas Pria Parkir (mendukung Voice & Panic Button).</summary>
    [System.Serializable]
    public class PilihanKonfrontasi
    {
        public string label = "Respons Rara";
        [Tooltip("\"AMAN\" | \"RAGU\" | \"BAHAYA\" | \"LAPOR\".")]
        public string kategori = "AMAN";
        [TextArea(2, 4)]
        public string reaksi = "\u2713 Bagus!";
        [Tooltip("Poin bonus tambahan di luar skor kategori.")]
        public int bonusPoin = 0;
        [Tooltip("Kurangi 1 nyawa saat dipilih (mis. opsi 'Diam').")]
        public bool kurangiNyawa = false;
        [Tooltip("Opsi ini WAJIB diiringi teriakan KERAS (Voice MAX) agar berhasil. Mis. 'JANGAN DEKAT!'.")]
        public bool butuhVoiceKeras = false;
        [Tooltip("Opsi ini adalah PANIC BUTTON \u2192 lapor sukses (achievement).")]
        public bool panicButton = false;
        [Tooltip("Ending yang dipicu opsi ini.")]
        public HasilDay3 hasil = HasilDay3.Lanjut;
        public Color warna = new Color(0.20f, 0.60f, 0.86f, 1f);
    }

    /// <summary>Satu baris dialog intro pembuka Day 3 (gaya naratif Day 2).</summary>
    [System.Serializable]
    public class BarisIntro
    {
        [Tooltip("Nama pembicara. 'Narasi' = kotak narasi kuning; nama lain (mis. 'Rara') = dialog biasa.")]
        public string pembicara = "Narasi";
        [TextArea(2, 4)]
        public string teks = "";
    }

    // ══════════════════════════════════════════════════════════════════════
    // INSPECTOR
    // ══════════════════════════════════════════════════════════════════════

    [Header("Auto-Start")]
    [Tooltip("Mulai otomatis dari BossIntro saat GameObject di-enable & GameState.day == 3.")]
    public bool autoStart = true;

    [Header("Mode Tampilan")]
    [Tooltip("TRUE = Visual Novel (seperti Day 2): tanpa bar 'Mental Si Bully', tanpa logika " +
             "kombat HP. Konfrontasi mengalir sebagai dialog naratif + pilihan saja.\n" +
             "FALSE = Boss fight: tampil bar mental & boss kalah saat mental 0.")]
    public bool modeVisualNovel = true;

    [Header("Overlay Judul Hari (BossIntro)")]
    public bool   tampilkanOverlayJudul = true;
    public string barisPertama = "HARI 3";
    public string barisKedua   = "Hujan di Parkiran Sekolah";
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
        "TUNGGU! Rara mau naik ojol...\ntapi seseorang tiba-tiba menghadang jalannya! \uD83D\uDE31\n" +
        "Itu dia \u2014 si pengirim pesan tadi \u2014 muncul langsung di depan Rara!!";

    [Header("Intro Pembuka Day 3 (gaya Day 2)")]
    [Tooltip("Tampilkan dialog pembuka (bel pulang, hujan, pesan ojol) sebelum jalan ke parkiran.")]
    public bool jalankanIntroPembuka = true;
    [Tooltip("Baris dialog pembuka. 'Narasi' = kotak narasi kuning; nama lain = dialog pembicara.")]
    public BarisIntro[] introBaris = new BarisIntro[]
    {
        new BarisIntro { pembicara = "Narasi",
            teks = "Bel pulang udah bunyi di SMP Harapan! \uD83D\uDD14\nTapi hujan deras banget hari ini...\nIbu Rara nggak bisa jemput. Rara harus pulang sendiri." },
        new BarisIntro { pembicara = "Rara",
            teks = "\"Yah... tapi nggak apa-apa kok! \uD83D\uDE24\nAku udah pesen ojol lewat HP.\nTinggal jalan dikit ke parkiran deh.\"" },
        new BarisIntro { pembicara = "Narasi",
            teks = "Tap layar / TERIAK buat jalan ke parkiran!\nSemakin keras teriak = makin cepet jalannya! \uD83C\uDFC3" }
    };

    [Header("Jalan di Hujan (menuju parkiran)")]
    [Tooltip("Segmen jalan kaki menembus hujan menuju parkiran (TAP / TERIAK untuk maju).")]
    public bool jalankanJalanHujan = true;
    [TextArea(2, 3)]
    public string jalanInstruksi = "\u2794 Jalan ke parkiran sekolah \u2014 TERIAK buat lari lebih cepat! \uD83C\uDFC3";
    [Tooltip("Jarak total ke parkiran (meter, sekadar pemanis).")]
    public float jalanJarakMeter = 60f;
    [Tooltip("Kecepatan jalan santai (m/detik) saat tanpa input.")]
    public float jalanKecepatanNormal = 13f;
    [Tooltip("Kecepatan saat layar di-tap.")]
    public float jalanKecepatanTap = 22f;
    [Tooltip("Kecepatan saat berteriak (mic keras / tahan SPASI / tombol TERIAK).")]
    public float jalanKecepatanTeriak = 32f;
    [Tooltip("Narasi saat Rara tiba di parkiran (ditampilkan setelah jalan selesai).")]
    [TextArea(2, 4)]
    public string[] jalanNarasiSampai = new string[]
    {
        "Rara akhirnya sampai di parkiran. Basah kuyup kena hujan! \uD83D\uDE05\nDia langsung buka HP buat ngecek ojol-nya udah nyampe belum...",
        "\"Eh?! Ada notif dari nomor yang nggak aku kenal?! \uD83D\uDE28\nSiapa nih... *deg-degan banget*\""
    };

    [Header("Tantangan 1 — Chat Agresif (Ojol Palsu)")]
    [Tooltip("Jalankan simulasi chat WhatsApp 'Paman Baik / ojol palsu' sebelum boss fight.")]
    public bool jalankanChatAgresif = true;
    public string chatNamaKontak = "Paman Baik";
    public string chatStatus = "online";
    [TextArea(1, 3)]
    [Tooltip("Pesan masuk yang diketik otomatis satu per satu.")]
    public string[] chatPesan = new string[]
    {
        "Hai cantik! Hujan deras ya \uD83D\uDE22 Hati-hati basah...",
        "Mau jemput? Gratis kok, kasihan kamu basah sendirian!",
        "\uD83E\uDD7A\uD83D\uDCF1 Cepat balas dong sayang... Mana foto kamu?"
    };
    [Tooltip("Detik tersisa untuk memilih respons chat.")]
    public float chatTimerDetik = 6f;
    [Tooltip("Referensi ChatSimWhatsApp opsional. Kosong = dibuat otomatis dengan data di atas.")]
    public ChatSimWhatsApp chatAgresif;

    [Header("Tantangan 2 — Ojol Palsu")]
    [Tooltip("Jalankan adegan 'ojek online palsu' menawarkan tumpangan gratis sebelum boss.")]
    public bool jalankanOjolPalsu = true;
    [TextArea(2, 4)]
    public string ojolNarasi =
        "Yes! Rara nggak terpancing pesan mencurigakan itu! \uD83D\uDCAA\n" +
        "Nah, ojol pesanan Rara baru aja tiba di parkiran!\n" +
        "Tapi jangan langsung naik \u2014 cek plat nomornya dulu ya!";
    public string ojolNamaSpeaker = "Ojek Online (?)";
    [TextArea(1, 3)]
    public string ojolUcapan = "\"Ayo naik, gratis! Cepetan, keburu makin deras nih!\"";
    public PilihanRonde[] ojolPilihan = new PilihanRonde[]
    {
        new PilihanRonde { label = "\uD83D\uDCF8 Foto plat dulu, lalu tolak naik", kategori = "AMAN", bonusPoin = 100,
            reaksi = "\u2713 Cerdas! Kamu foto plat sebagai bukti, lalu menolak dengan sopan. Jangan naik kendaraan orang asing." },
        new PilihanRonde { label = "\"Makasih, saya jalan kaki saja.\"", kategori = "AMAN",
            reaksi = "\u2713 Bagus, kamu menolak dengan tegas dan tetap waspada." },
        new PilihanRonde { label = "Naik saja, mumpung gratis", kategori = "BAHAYA",
            reaksi = "\u2716 Bahaya! Jangan pernah naik kendaraan orang asing meski gratis. Kamu kehilangan 1 nyawa." }
    };

    [Header("Tantangan 2 — Cek Plat Nomor (Minigame)")]
    [Tooltip("Plat yang DIPESAN / diberitahukan (mis. dari Ibu lewat aplikasi).")]
    public string ojolPlatPesanan = "DD 3472 WK";
    [Tooltip("Plat ojol/angkot yang sebenarnya datang (sengaja BERBEDA = ojol palsu).")]
    public string ojolPlatAngkot  = "DB 8831 QP";
    [Tooltip("Bonus poin saat berhasil mengecek & menolak ojol palsu.")]
    public int    ojolBonusCekPlat = 100;

    [Header("Boss — Pria di Parkiran (Grooming 4 Tahap)")]
    [Tooltip("4 kalimat grooming yang diketik berurutan SEBELUM pilihan pamungkas muncul.")]
    [TextArea(1, 3)]
    public string[] groomingLines = new string[]
    {
        "\"Eh hei, mau kemana sendirian? \uD83D\uDE0F Ikut aku dulu deh. Sebentar aja kok~\"",
        "\"Sssst! Jangan teriak-teriak, nanti kamu yang dimarahin orang. Diam aja ya~\"",
        "\"Haha, emangnya siapa yang bakal percaya sama kamu? Nggak ada! Diam aja~\"",
        "\"Ini rahasia kita berdua ya. Kalau kamu ngadu, kamu sendiri yang bakal kena masalah!\""
    };
    [Tooltip("Ucapan boss saat mendesak Rara memutuskan (muncul bersama pilihan).")]
    [TextArea(1, 3)]
    public string konfrontasiUcapan = "\"Pasrah aja lah! Nggak ada yang bisa nolongin kamu di sini!\"";
    [Tooltip("4 pilihan pamungkas: Diam / 'Makasih' / 'JANGAN DEKAT!'+Voice / PANIC BUTTON.")]
    public PilihanKonfrontasi[] konfrontasiPilihan = new PilihanKonfrontasi[]
    {
        new PilihanKonfrontasi {
            label = "(beku di tempat, nggak bisa ngomong...)", kategori = "BAHAYA",
            reaksi = "\u2716 Rara beku ketakutan, dan dia jadi makin berani. Kehilangan 1 nyawa \u2014 ingat, DIAM ITU BAHAYA. Kamu harus bersuara!",
            kurangiNyawa = true, hasil = HasilDay3.Lanjut,
            warna = new Color(0.50f, 0.20f, 0.20f, 1f)
        },
        new PilihanKonfrontasi {
            label = "\"T-tolong...\" (suaranya hampir nggak kedengeran)", kategori = "RAGU",
            reaksi = "\u26A0 Rara berhasil pergi, tapi suaranya pelan banget. Lain kali TERIAK yang keras ya!",
            hasil = HasilDay3.Aman,
            warna = new Color(0.95f, 0.61f, 0.07f, 1f)
        },
        new PilihanKonfrontasi {
            label = "\"JANGAN DEKET-DEKET! TOLONG!! \uD83D\uDD0A\" (Teriak KERAS!)", kategori = "AMAN", bonusPoin = 300,
            reaksi = "\u2713 HEBAT! Teriakan Rara bikin dia kaget dan langsung mundur. Berani bersuara itu kekuatan!",
            butuhVoiceKeras = true, hasil = HasilDay3.Aman,
            warna = new Color(0.15f, 0.68f, 0.38f, 1f)
        },
        new PilihanKonfrontasi {
            label = "\uD83D\uDCE2 TERIAK SEKERAS-KERASNYA + lari ke satpam! \uD83C\uDD98", kategori = "LAPOR",
            reaksi = "\u2713 LAPOR SUKSES! Rara teriak minta tolong dan lari ke satpam. Guru dan satpam langsung datang \u2014 Rara pahlawan buat dirinya sendiri!",
            panicButton = true, hasil = HasilDay3.LaporSukses,
            warna = new Color(0.20f, 0.62f, 0.86f, 1f)
        }
    };
    [Tooltip("Durasi maksimum jendela teriak (detik) untuk opsi 'JANGAN DEKAT!'.")]
    public float voiceTimeoutDetik = 5f;

    [Header("Boss — Panic Button (Polisi Datang)")]
    [Tooltip("Label tombol darurat.")]
    public string panicLabel = "\uD83D\uDEA8 PANIC BUTTON";
    [Tooltip("Narasi saat panic button ditekan (polisi/guru datang).")]
    [TextArea(2, 4)]
    public string panicNarasi = "\uD83D\uDE94 \"HEEEI! Ada apa ini?! Kami denger ada yang teriak!\"\n" +
        "Si Bayangan Gelap langsung kabur terbirit-birit! Pengecut!";
    [Tooltip("Durasi animasi polisi/guru datang (detik).")]
    public float panicAnimasiDurasi = 2.2f;

    [Header("Ending")]
    [Tooltip("Achievement saat memilih Panic Button / lapor sukses.")]
    public string achievementLapor = "Pahlawan Diri Sendiri";
    [TextArea(2, 4)]
    public string endingAmanNarasi =
        "Hujan mulai reda. Rara masuk ke gerbang sekolah dengan selamat. " +
        "Dadanya masih berdebar, tapi ia bangga \u2014 hari ini ia berhasil menjaga dirinya sendiri!";
    [TextArea(2, 4)]
    public string endingTraumaNarasi =
        "Rasa takut bikin Rara nggak berani bertindak, dan keadaannya jadi berbahaya. " +
        "Tapi tenang \u2014 jangan menyerah! Ayo coba lagi dan belajar cara menjaga diri.";
    public string endingTraumaJudul = "\uD83D\uDC94  GAME OVER";

    [Header("Backdrop Procedural")]
    public bool buatBackdrop = true;
    public Color warnaBackdrop = new Color(0.10f, 0.12f, 0.18f, 1f); // suram, hujan
    public int   backdropSortingOrder = -100;

    [Header("Boss (tampilan)")]
    public string bossNama = "Si Bayangan Gelap";
    [Tooltip("Sprite potret boss (opsional). Kosong = kotak warna solid.")]
    public Sprite bossSprite;
    public Color  bossWarnaFallback = new Color(0.35f, 0.10f, 0.12f, 1f);
    [Tooltip("Mental Si Bully maksimum. Boss kalah saat mental mencapai 0.")]
    public float  bossMentalMax = 100f;

    [Header("Box Dialog (gaya bingkai kayu Halte)")]
    [Tooltip("Referensi komponen HalteDialog di scene. Dipakai untuk meminjam sprite panel kayu, " +
             "layout, dan portrait supaya box dialog Day 3 tampil PERSIS seperti box dialog Halte. " +
             "Kosong = auto-cari di scene; kalau tetap tidak ada = fallback panel gelap + outline emas.")]
    public HalteDialog gayaHalteDialog;
    [Tooltip("Portrait untuk pembicara Narasi (mis. ikon gulungan/scroll). Kosong = ambil dari HalteDialog.")]
    public Sprite portraitNarasi;
    [Tooltip("Portrait untuk pembicara Boss/Pria Asing. Kosong = ambil dari HalteDialog, lalu bossSprite.")]
    public Sprite portraitBoss;
    [Tooltip("Portrait untuk pembicara Rara. Kosong = ambil dari HalteDialog.")]
    public Sprite portraitRara;

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

    [Header("Saat Boss Kalah / Lapor Sukses")]
    [TextArea(2, 5)]
    public string narasiBossKalah =
        "\"Tenang Rara, kamu udah berani banget! Kamu nggak salah sama sekali.\"\n" +
        "Guru dan satpam bakal bantu laporin ke polisi. Rara berani cerita \u2014 itu pilihan PALING TEPAT! \uD83D\uDCAA";
    [Tooltip("Achievement yang diraih saat selamat dengan berani (ending AMAN).")]
    public string achievementMenang = "Berani Menjaga Diri";

    [Header("Kartu Edukasi Hari 3")]
    public string eduJudul = "\uD83C\uDFC6  Kartu Edukasi \u2014 Hari 3: FINAL";
    public Color  eduWarnaJudul = new Color(1f, 0.85f, 0.30f, 1f);
    [TextArea(4, 10)]
    public string eduIsi =
        "\u26A0 Apa itu Grooming?\n" +
        "Grooming = orang dewasa yang pura-pura 'baik' buat mendekati anak \u2014 lewat chat, sosmed, atau ketemu langsung. Ini KEJAHATAN. Kamu boleh lapor!\n\n" +
        "\uD83E\uDD81 Cara Melindungi Diri:\n" +
        "\u2022 Terasa nggak aman? TERIAK keras dan minta tolong!\n" +
        "\u2022 Chat mencurigakan? Blokir + screenshot + cerita ke ortu.\n" +
        "\u2022 Guru dan polisi ADA untuk melindungi kamu!\n\n" +
        "\uD83D\uDCE3 Yang Paling Penting:\n" +
        "Kalau kamu jadi korban, itu BUKAN salahmu! Berani cerita ke orang yang dipercaya = tindakan paling berani yang bisa kamu lakuin! \uD83D\uDCAA\n\n" +
        "\uD83C\uDD98 Darurat: Polisi 110  |  Hotline Anak 129  |  KPAI 021-31901556";

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
    private HasilDay3  _hasilDay3 = HasilDay3.Lanjut;
    private GameObject _backdropGO;
    private GameObject _canvasGO;
    private Image      _bossImg;
    private Image      _bossBarFill;
    private TextMeshProUGUI _bossBarText;
    private TextMeshProUGUI _bossNamaText;
    private TextMeshProUGUI _ucapanText;
    private TextMeshProUGUI _namaUcapanText;   // banner nama pembicara (gaya box dialog Day 2)
    private TextMeshProUGUI _hintLanjutText;   // hint "klik untuk lanjut" (gaya Day 2)
    private Image      _portraitUcapanImg;     // portrait di bingkai kiri box (gaya Halte)
    private TextMeshProUGUI _reaksiText;
    private GameObject _pilihanPanel;
    private bool       _menungguPilihan;
    private bool       _ucapanSkip;            // true saat pemain klik untuk skip typewriter
    private bool       _overlayShown;          // overlay judul "HARI 3" hanya tampil sekali
    private bool       _jalanTapBoost;         // true sesaat setelah layar di-tap (jalan di hujan)
    private bool       _jalanTeriakHold;       // true selama tombol TERIAK ditahan (jalan di hujan)

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        Instance = this;
        // Auto-cari komponen HalteDialog di scene untuk meminjam look box dialog kayu.
        if (gayaHalteDialog == null)
            gayaHalteDialog = FindObjectOfType<HalteDialog>(true);
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

        if (jalankanIntroPembuka) GotoFase(Phase.IntroPembuka);
        else if (jalankanJalanHujan) GotoFase(Phase.JalanHujan);
        else if (jalankanChatAgresif) GotoFase(Phase.ChatAgresif);
        else if (jalankanOjolPalsu) GotoFase(Phase.OjolPalsu);
        else GotoFase(Phase.BossIntro);
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
            case Phase.IntroPembuka:
                yield return JalankanIntroPembuka();
                break;

            case Phase.JalanHujan:
                yield return JalankanJalanHujan();
                break;

            case Phase.ChatAgresif:
                yield return JalankanChatAgresif();
                break;

            case Phase.OjolPalsu:
                yield return JalankanOjolPalsu();
                break;

            case Phase.BossIntro:
                if (tampilkanOverlayJudul && !_overlayShown)
                {
                    yield return TampilkanOverlayJudul();
                    _overlayShown = true;
                }
                BuildArena();
                if (_bossImg != null) _bossImg.enabled = true;
                if (_bossNamaText != null) _bossNamaText.text = bossNama;
                yield return TampilkanUcapan(narasiPembuka, isNarasi: true);
                yield return new WaitForSeconds(0.4f);
                // Alur otentik Hari 3: grooming 4 tahap → konfrontasi pamungkas.
                // Bila groomingLines kosong, jatuh ke mode boss-fight lama (Round 1–3).
                if (groomingLines != null && groomingLines.Length > 0)
                    GotoFase(Phase.BossKonfrontasi);
                else
                    GotoFase(Phase.Round1);
                break;

            case Phase.BossKonfrontasi:
                yield return JalankanKonfrontasiBoss();
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
                // Ending TRAUMA langsung ke layar hasil (Game Over), tanpa kartu edukasi.
                if (_hasilDay3 == HasilDay3.Trauma) { GotoFase(Phase.Complete); break; }
                yield return TampilkanEduCard();
                GotoFase(Phase.Complete);
                break;

            case Phase.Complete:
                TampilkanHasilAkhir();
                break;
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // INTRO PEMBUKA (gaya naratif Day 2) + JALAN DI HUJAN
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator JalankanIntroPembuka()
    {
        // Overlay judul "HARI 3" hanya sekali (tidak diulang di BossIntro).
        if (tampilkanOverlayJudul && !_overlayShown)
        {
            yield return TampilkanOverlayJudul();
            _overlayShown = true;
        }

        // Pakai kotak dialog arena, tapi sembunyikan boss (belum muncul).
        BuildArena();
        if (_bossImg != null) _bossImg.enabled = false;
        if (_bossNamaText != null) _bossNamaText.text = "";

        if (introBaris != null)
        {
            foreach (var b in introBaris)
            {
                if (b == null || string.IsNullOrEmpty(b.teks)) continue;
                bool narasi = string.IsNullOrEmpty(b.pembicara) || b.pembicara == "Narasi";
                Color warnaNama = narasi ? new Color(1f, 0.85f, 0.3f, 1f) : warnaAman;
                yield return TampilkanUcapanNama(b.pembicara, b.teks, warnaNama, narasi);
                yield return new WaitForSeconds(0.15f);
            }
        }

        // Bersihkan arena agar tidak menutupi fase berikutnya.
        if (_canvasGO != null) { Destroy(_canvasGO); _canvasGO = null; }

        if (jalankanJalanHujan) GotoFase(Phase.JalanHujan);
        else if (jalankanChatAgresif) GotoFase(Phase.ChatAgresif);
        else if (jalankanOjolPalsu) GotoFase(Phase.OjolPalsu);
        else GotoFase(Phase.BossIntro);
    }

    IEnumerator JalankanJalanHujan()
    {
        var go = new GameObject("Day3_JalanHujan");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder + 45;
        AddScaler(go);
        go.AddComponent<GraphicRaycaster>();

        // Latar hujan gelap.
        var bg = BuatImage(go.transform, "BG", new Color(0.10f, 0.13f, 0.20f, 1f));
        Stretch(bg.rectTransform);

        // Zona TAP (penuh layar) → boost maju sesaat.
        _jalanTapBoost = false;
        var tapZone = BuatImage(go.transform, "TapZone", new Color(0f, 0f, 0f, 0f));
        Stretch(tapZone.rectTransform);
        var tapBtn = tapZone.gameObject.AddComponent<Button>();
        tapBtn.transition = Selectable.Transition.None;
        tapBtn.onClick.AddListener(() => { _jalanTapBoost = true; });

        // Instruksi (atas).
        var instr = BuatTeks(go.transform, "Instruksi", jalanInstruksi, 30, new Color(1f, 0.92f, 0.7f, 1f), FontStyles.Bold);
        instr.alignment = TextAlignmentOptions.Center;
        var iRt = instr.rectTransform;
        iRt.anchorMin = new Vector2(0.08f, 0.78f); iRt.anchorMax = new Vector2(0.92f, 0.92f);
        iRt.offsetMin = Vector2.zero; iRt.offsetMax = Vector2.zero;

        // Jarak tersisa.
        var jarakTxt = BuatTeks(go.transform, "Jarak", "", 26, Color.white, FontStyles.Normal);
        jarakTxt.alignment = TextAlignmentOptions.Center;
        var jTxtRt = jarakTxt.rectTransform;
        jTxtRt.anchorMin = new Vector2(0.1f, 0.60f); jTxtRt.anchorMax = new Vector2(0.9f, 0.72f);
        jTxtRt.offsetMin = Vector2.zero; jTxtRt.offsetMax = Vector2.zero;

        // Bar progres jalan.
        var barBg = BuatImage(go.transform, "BarBg", new Color(0.15f, 0.15f, 0.18f, 1f));
        barBg.sprite = SolidSprite(); barBg.type = Image.Type.Sliced;
        barBg.raycastTarget = false;
        var barRt = barBg.rectTransform;
        barRt.anchorMin = new Vector2(0.12f, 0.42f); barRt.anchorMax = new Vector2(0.88f, 0.48f);
        barRt.offsetMin = Vector2.zero; barRt.offsetMax = Vector2.zero;

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(barBg.transform, false);
        var fill = fillGO.AddComponent<Image>();
        fill.color = warnaAman;
        fill.sprite = SolidSprite();
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.fillAmount = 0f;
        fill.raycastTarget = false;
        Stretch(fill.rectTransform, 3f, 3f);

        // Ikon Rara berjalan/berlari (emoji) bergerak mengikuti progres.
        var runner = BuatTeks(barBg.transform, "Runner", "\uD83D\uDEB6", 40, Color.white, FontStyles.Normal);
        runner.alignment = TextAlignmentOptions.Center;
        var runRt = runner.rectTransform;
        runRt.anchorMin = new Vector2(0f, 0.5f); runRt.anchorMax = new Vector2(0f, 0.5f);
        runRt.pivot = new Vector2(0.5f, 0f); runRt.sizeDelta = new Vector2(60f, 60f);

        // Tombol TERIAK (tahan) — fallback tanpa mikrofon.
        _jalanTeriakHold = false;
        var shoutImg = BuatImage(go.transform, "TombolTeriak", warnaBahaya);
        shoutImg.sprite = SolidSprite(); shoutImg.type = Image.Type.Sliced;
        var shRt = shoutImg.rectTransform;
        shRt.anchorMin = new Vector2(0.5f, 0f); shRt.anchorMax = new Vector2(0.5f, 0f);
        shRt.pivot = new Vector2(0.5f, 0f); shRt.sizeDelta = new Vector2(380f, 96f);
        shRt.anchoredPosition = new Vector2(0f, 70f);
        var shLbl = BuatTeks(shoutImg.transform, "Lbl", "\uD83D\uDCE2 TERIAK (tahan)", 28, Color.white, FontStyles.Bold);
        shLbl.alignment = TextAlignmentOptions.Center;
        Stretch(shLbl.rectTransform);
        var trig = shoutImg.gameObject.AddComponent<EventTrigger>();
        var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        down.callback.AddListener(_ => _jalanTeriakHold = true);
        trig.triggers.Add(down);
        var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        up.callback.AddListener(_ => _jalanTeriakHold = false);
        trig.triggers.Add(up);

        var vm = VoiceMeter.Instance;
        float jalan = 0f;
        float totalDist = Mathf.Max(1f, jalanJarakMeter);
        while (jalan < totalDist)
        {
            bool teriak = (vm != null && vm.IsLoud())
                          || _jalanTeriakHold
                          || Input.GetKey(KeyCode.Space)
                          || (vm != null && vm.fallbackButtonHeld);

            float spd = teriak ? jalanKecepatanTeriak
                       : _jalanTapBoost ? jalanKecepatanTap
                       : jalanKecepatanNormal;
            _jalanTapBoost = false;

            jalan += spd * Time.deltaTime;
            float p = Mathf.Clamp01(jalan / totalDist);
            fill.fillAmount = p;

            // Runner mengikuti progres sepanjang bar.
            float barW = barRt.rect.width;
            runRt.anchoredPosition = new Vector2(p * barW, runRt.anchoredPosition.y);
            runner.text = teriak ? "\uD83C\uDFC3" : "\uD83D\uDEB6";

            int sisa = Mathf.CeilToInt(totalDist - jalan);
            jarakTxt.text = sisa > 0
                ? "Parkiran " + sisa + " m lagi..." + (teriak ? "  \uD83D\uDCA8" : "")
                : "Hampir sampai!";
            yield return null;
        }

        Destroy(go);

        // Narasi tiba di parkiran (gaya Day 2) — pakai kotak dialog arena tanpa boss.
        BuildArena();
        if (_bossImg != null) _bossImg.enabled = false;
        if (_bossNamaText != null) _bossNamaText.text = "";
        if (jalanNarasiSampai != null)
        {
            foreach (var line in jalanNarasiSampai)
            {
                if (string.IsNullOrEmpty(line)) continue;
                yield return TampilkanUcapanNama("Narasi", line, new Color(1f, 0.85f, 0.3f, 1f), true);
            }
        }
        if (_canvasGO != null) { Destroy(_canvasGO); _canvasGO = null; }

        if (jalankanChatAgresif) GotoFase(Phase.ChatAgresif);
        else if (jalankanOjolPalsu) GotoFase(Phase.OjolPalsu);
        else GotoFase(Phase.BossIntro);
    }

    // ══════════════════════════════════════════════════════════════════════
    // TANTANGAN 1 — CHAT AGRESIF (OJOL PALSU)
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator JalankanChatAgresif()
    {
        bool selesai = false;
        var chat = BuatChatAgresif();
        chat.Mulai(() => selesai = true);
        while (!selesai) yield return null;

        // Sinkron HUD setelah kemungkinan kehilangan nyawa.
        var gs = GameState.Instance;
        if (HUDManager.Instance != null && gs != null)
            HUDManager.Instance.UpdateHearts(gs.lives, gs.maxLives);

        // Pilihan fatal / nyawa habis → langsung ke layar hasil (GAME OVER).
        if (gs != null && !gs.IsAlive())
        {
            GotoFase(Phase.Complete);
            yield break;
        }

        yield return new WaitForSeconds(0.3f);
        // Alur otentik Hari 3: Chat \u2192 Ojol Palsu (cek plat) \u2192 Boss.
        if (jalankanOjolPalsu) GotoFase(Phase.OjolPalsu);
        else GotoFase(Phase.BossIntro);
    }

    /// <summary>Buat (atau pakai ulang) komponen chat WhatsApp untuk tantangan ojol palsu.</summary>
    ChatSimWhatsApp BuatChatAgresif()
    {
        if (chatAgresif != null) return chatAgresif;

        var go = new GameObject("Day3_ChatAgresif");
        go.transform.SetParent(transform, false);
        var chat = go.AddComponent<ChatSimWhatsApp>();

        chat.namaKontak         = chatNamaKontak;
        chat.statusKontak       = chatStatus;
        chat.hariUntukSkor      = 3;
        chat.notifBerderingKali = 3;          // SFX notif WA berdering 3x
        chat.waktuPilihDetik    = chatTimerDetik;
        chat.fontAsset          = fontAsset;
        chat.sortingOrder       = sortingOrder + 30;

        // Pesan masuk (auto-type, 3 detik per pesan).
        var pesan = new List<ChatSimWhatsApp.PesanData>();
        if (chatPesan != null)
            foreach (var teks in chatPesan)
                pesan.Add(new ChatSimWhatsApp.PesanData { teks = teks, delayDetik = 3f });
        chat.pesanMasuk = pesan.ToArray();

        // Tombol screenshot bonus = "Screenshot Dulu" (+100 bukti).
        chat.tampilkanTombolScreenshot = true;
        chat.screenshotLabel = "\uD83D\uDCF8 Screenshot dulu buat bukti";
        chat.screenshotBonus = 100;
        chat.screenshotAchievement = "Detektif Bukti"; // ambil bukti = achievement

        // 3 pilihan utama sesuai alur Hari 3.
        chat.aksiList = new ChatSimWhatsApp.AksiData[]
        {
            new ChatSimWhatsApp.AksiData {
                label = "\uD83D\uDCF8 Oke, ini foto seragamku~", kategori = "BAHAYA",
                reaksi = "\u2716 STOP! Jangan kirim foto ke orang yang nggak kamu kenal! Foto bisa dipakai buat memeras atau mengancam kamu. Kamu kehilangan 1 nyawa.",
                kurangiNyawa = true,
                warna = warnaBahaya
            },
            new ChatSimWhatsApp.AksiData {
                label = "\uD83D\uDE97 Iya Om, aku di parkiran SMP. Jemput ya!", kategori = "BAHAYA",
                reaksi = "\u2716 GAME OVER! Rara pergi sama orang nggak dikenal dari internet! Jangan PERNAH kasih lokasi atau minta dijemput orang asing.",
                akhiriGameOver = true,
                warna = new Color(0.50f, 0.16f, 0.16f, 1f)
            },
            new ChatSimWhatsApp.AksiData {
                label = "\uD83D\uDEAB BLOKIR sekarang + lapor ke ortu!", kategori = "AMAN",
                reaksi = "\u2713 TEPAT! Blokir nomornya, terus cerita ke orang tua. Itulah cara pahlawan menjaga diri! (+200)",
                bonusPoin = 200,
                warna = warnaAman
            }
        };
        chat.aksiSaatTimeout = 0; // waktu habis → dianggap "Balas Foto" (BAHAYA, -1 nyawa)

        chatAgresif = chat;
        return chat;
    }

    // ══════════════════════════════════════════════════════════════════════
    // TANTANGAN 2 — OJOL PALSU
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator JalankanOjolPalsu()
    {
        BuildArena();

        // Pengemudi ojol palsu bicara (bukan boss parkir).
        if (_bossNamaText != null) _bossNamaText.text = ojolNamaSpeaker;
        yield return TampilkanUcapan(ojolNarasi, isNarasi: true);
        yield return new WaitForSeconds(0.3f);
        yield return TampilkanUcapan(ojolUcapan, isNarasi: false);

        // Kartu plat: plat PESANAN selalu tampil, plat ANGKOT awalnya tertutup ("?").
        TextMeshProUGUI platPesananText, platAngkotText;
        var platPanel = BuatPanelPlat(out platPesananText, out platAngkotText);

        // ── Level 1: cek plat dulu, atau langsung naik (fatal). ───────────────
        int pilih1 = -1;
        _menungguPilihan = true;
        TampilkanTombolKustom(new (string, Color)[]
        {
            ("\uD83D\uDD0D Cek & bandingkan plat dulu", warnaNetral),
            ("\uD83D\uDE97 Naik saja (gratis!)",        warnaBahaya)
        }, i => { pilih1 = i; _menungguPilihan = false; });
        while (_menungguPilihan) yield return null;

        if (pilih1 == 1)
        {
            // Naik tanpa cek plat → langsung GAME OVER (Trauma).
            GameState.Instance?.AddChoice(3, "Ojol: naik TANPA cek plat", "BAHAYA");
            if (GameState.Instance != null) GameState.Instance.lives = 0;
            if (HUDManager.Instance != null && GameState.Instance != null)
                HUDManager.Instance.UpdateHearts(GameState.Instance.lives, GameState.Instance.maxLives);
            AudioManager.Instance?.Wrong();
            if (_reaksiText != null)
                _reaksiText.text = "\u2716 Rara naik ojol palsu! Selalu cocokin plat di aplikasi sama plat di motor. Kalau beda, jangan naik! GAME OVER.";
            yield return new WaitForSeconds(2.4f);
            if (platPanel != null) Destroy(platPanel);
            _hasilDay3 = HasilDay3.Trauma;
            GotoFase(Phase.EduCard);
            yield break;
        }

        // Buka plat angkot untuk dibandingkan.
        AudioManager.Instance?.Click();
        if (platAngkotText != null) platAngkotText.text = ojolPlatAngkot;
        if (_reaksiText != null)
            _reaksiText.text = "Bandingkan! Plat ojol ini COCOK nggak sama pesanan Rara?";
        yield return new WaitForSeconds(0.6f);

        // ── Level 2: cocokkan plat. Plat sengaja BERBEDA → jawaban benar: TIDAK COCOK. ──
        int pilih2 = -1;
        _menungguPilihan = true;
        TampilkanTombolKustom(new (string, Color)[]
        {
            ("\u274C TIDAK COCOK \u2014 tolak naik", warnaAman),
            ("\u2705 Cocok \u2014 naik saja",        warnaBahaya)
        }, i => { pilih2 = i; _menungguPilihan = false; });
        while (_menungguPilihan) yield return null;

        if (pilih2 != 0)
        {
            // Salah menilai (plat jelas beda) lalu naik → GAME OVER (Trauma).
            GameState.Instance?.AddChoice(3, "Ojol: salah cocokkan plat lalu naik", "BAHAYA");
            if (GameState.Instance != null) GameState.Instance.lives = 0;
            if (HUDManager.Instance != null && GameState.Instance != null)
                HUDManager.Instance.UpdateHearts(GameState.Instance.lives, GameState.Instance.maxLives);
            AudioManager.Instance?.Wrong();
            if (_reaksiText != null)
                _reaksiText.text = "\u2716 Platnya jelas beda, tapi Rara tetap naik! Rara naik ojol palsu. GAME OVER.";
            yield return new WaitForSeconds(2.4f);
            if (platPanel != null) Destroy(platPanel);
            _hasilDay3 = HasilDay3.Trauma;
            GotoFase(Phase.EduCard);
            yield break;
        }

        // Benar: plat tidak cocok → tolak naik (AMAN + bonus bukti).
        GameState.Instance?.AddChoice(3, "Ojol: cek plat, tidak cocok, tolak naik", "AMAN");
        if (ojolBonusCekPlat > 0) GameState.Instance?.AddScore(ojolBonusCekPlat);
        AudioManager.Instance?.PlayKategori("AMAN");
        if (_reaksiText != null)
            _reaksiText.text = "\u2713 PLAT BENAR dicek! Platnya beda = ojol PALSU. Rara nggak naik. Pinter! (+" + ojolBonusCekPlat + " bukti)";
        yield return new WaitForSeconds(2.2f);
        if (_reaksiText != null) _reaksiText.text = "";
        if (platPanel != null) Destroy(platPanel);

        GotoFase(Phase.BossIntro);
    }

    /// <summary>Bangun kartu plat (pesanan + angkot) di tengah-atas arena. Plat angkot awalnya "?".</summary>
    GameObject BuatPanelPlat(out TextMeshProUGUI platPesananText, out TextMeshProUGUI platAngkotText)
    {
        var panel = new GameObject("PanelPlat");
        panel.transform.SetParent(_canvasGO.transform, false);
        var pRt = panel.AddComponent<RectTransform>();
        pRt.anchorMin = new Vector2(0.5f, 0.5f); pRt.anchorMax = new Vector2(0.5f, 0.5f);
        pRt.pivot = new Vector2(0.5f, 0.5f);
        pRt.sizeDelta = new Vector2(900f, 220f);
        pRt.anchoredPosition = new Vector2(0f, 80f);

        // Kartu kiri: plat pesanan (acuan).
        platPesananText = BuatKartuPlat(panel.transform, "Plat Pesanan (dari Ibu)",
            ojolPlatPesanan, new Color(0.12f, 0.40f, 0.30f, 1f), new Vector2(-0.5f, 0f));
        // Kartu kanan: plat angkot (awalnya tertutup).
        platAngkotText = BuatKartuPlat(panel.transform, "Plat Ojol Ini",
            "?  ?  ?", new Color(0.40f, 0.16f, 0.16f, 1f), new Vector2(0.5f, 0f));

        return panel;
    }

    TextMeshProUGUI BuatKartuPlat(Transform parent, string judul, string plat, Color warna, Vector2 sisi)
    {
        var card = BuatImage(parent, "Kartu", warna);
        card.raycastTarget = false;
        var cRt = card.rectTransform;
        cRt.anchorMin = new Vector2(sisi.x < 0 ? 0f : 0.52f, 0f);
        cRt.anchorMax = new Vector2(sisi.x < 0 ? 0.48f : 1f, 1f);
        cRt.offsetMin = Vector2.zero; cRt.offsetMax = Vector2.zero;

        var jt = BuatTeks(card.transform, "Judul", judul, 22, new Color(1f, 1f, 1f, 0.85f), FontStyles.Bold);
        jt.alignment = TextAlignmentOptions.Center;
        var jRt = jt.rectTransform;
        jRt.anchorMin = new Vector2(0f, 0.6f); jRt.anchorMax = new Vector2(1f, 1f);
        jRt.offsetMin = new Vector2(8f, 0f); jRt.offsetMax = new Vector2(-8f, -6f);

        var pt = BuatTeks(card.transform, "Plat", plat, 44, Color.white, FontStyles.Bold);
        pt.alignment = TextAlignmentOptions.Center;
        var ptRt = pt.rectTransform;
        ptRt.anchorMin = new Vector2(0f, 0f); ptRt.anchorMax = new Vector2(1f, 0.6f);
        ptRt.offsetMin = new Vector2(8f, 8f); ptRt.offsetMax = new Vector2(-8f, 0f);
        return pt;
    }

    /// <summary>Tampilkan tombol pilihan kustom (label + warna) di panel pilihan.</summary>
    void TampilkanTombolKustom((string label, Color warna)[] opsi, Action<int> onPilih)
    {
        if (_pilihanPanel == null) return;
        _pilihanPanel.SetActive(true);
        foreach (Transform child in _pilihanPanel.transform) Destroy(child.gameObject);
        if (opsi == null || opsi.Length == 0) return;

        float slotH = 1f / opsi.Length;
        for (int i = 0; i < opsi.Length; i++)
        {
            float yMax = 1f - i * slotH;
            float yMin = yMax - slotH + 0.03f;

            var btnObj = new GameObject("Pilihan_" + i);
            btnObj.transform.SetParent(_pilihanPanel.transform, false);
            var img = btnObj.AddComponent<Image>();
            img.color = opsi[i].warna;
            img.sprite = SolidSprite();
            img.type = Image.Type.Sliced;
            var bRt = btnObj.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0f, yMin); bRt.anchorMax = new Vector2(1f, yMax);
            bRt.offsetMin = new Vector2(0f, 4f); bRt.offsetMax = new Vector2(0f, -4f);

            var btn = btnObj.AddComponent<Button>();
            int idx = i;
            btn.onClick.AddListener(() =>
            {
                AudioManager.Instance?.Click();
                _pilihanPanel.SetActive(false);
                onPilih?.Invoke(idx);
            });

            var label = BuatTeks(btnObj.transform, "Label", opsi[i].label, 24, Color.white, FontStyles.Bold);
            label.alignment = TextAlignmentOptions.Center;
            Stretch(label.rectTransform, 24f, 6f);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // BOSS — KONFRONTASI PRIA PARKIR (grooming 4 tahap + pilihan pamungkas)
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator JalankanKonfrontasiBoss()
    {
        // 1) Grooming 4 tahap: pria bicara satu per satu (auto-advance + jeda baca).
        if (_bossNamaText != null) _bossNamaText.text = bossNama;
        foreach (var line in groomingLines)
        {
            yield return TampilkanUcapan(line, isNarasi: false);
            yield return new WaitForSeconds(1.1f);
        }

        // 2) Pilihan pamungkas (Visual Novel). Opsi 'Diam' (Lanjut) → pria mendesak lagi (loop).
        while (true)
        {
            if (_bossNamaText != null) _bossNamaText.text = bossNama;
            yield return TampilkanUcapan(konfrontasiUcapan, isNarasi: false);

            PilihanKonfrontasi dipilih = null;
            _menungguPilihan = true;
            TampilkanPilihanKonfrontasi(konfrontasiPilihan, p => { dipilih = p; _menungguPilihan = false; });
            while (_menungguPilihan) yield return null;

            // Opsi [3] 'JANGAN DEKAT!' wajib diiringi teriakan KERAS (Voice MAX).
            bool berhasilVoice = true;
            if (dipilih.butuhVoiceKeras)
            {
                berhasilVoice = false;
                yield return PromptTeriak(ok => berhasilVoice = ok);
            }

            if (dipilih.butuhVoiceKeras && !berhasilVoice)
            {
                // Teriakan kurang keras → pria belum mundur. Ulangi pilihan.
                if (_reaksiText != null)
                    _reaksiText.text = "\u26A0 Suaramu kurang keras! Tarik napas, terus TERIAK sekuat tenaga: \"JANGAN DEKET-DEKET!\"";
                yield return new WaitForSeconds(1.8f);
                if (_reaksiText != null) _reaksiText.text = "";
                continue;
            }

            // Proses skor & nyawa.
            ProsesPilihanKonfrontasi(dipilih);
            if (_reaksiText != null) _reaksiText.text = dipilih.reaksi;
            yield return new WaitForSeconds(2.0f);
            if (_reaksiText != null) _reaksiText.text = "";

            // Opsi [4] PANIC BUTTON → animasi polisi/guru datang.
            if (dipilih.panicButton)
                yield return AnimasiPolisiDatang();

            // Nyawa habis → Trauma.
            if (GameState.Instance != null && !GameState.Instance.IsAlive())
            {
                _hasilDay3 = HasilDay3.Trauma;
                break;
            }

            // Belum final (mis. 'Diam' tapi masih hidup) → pria mendesak lagi (Lanjut fase 6).
            if (dipilih.hasil == HasilDay3.Lanjut) continue;

            _hasilDay3 = dipilih.hasil;
            break;
        }

        // 3) Routing ending.
        if (_hasilDay3 == HasilDay3.Trauma)
        {
            GotoFase(Phase.EduCard); // EduCard akan skip ke Complete saat Trauma
            yield break;
        }

        if (_hasilDay3 == HasilDay3.LaporSukses && !string.IsNullOrEmpty(achievementLapor))
            GameState.Instance?.EarnAchievement(achievementLapor);

        GotoFase(Phase.BossDefeated);
    }

    /// <summary>Animasi singkat: mobil polisi meluncur masuk + sirene berkedip (efek panic button).</summary>
    IEnumerator AnimasiPolisiDatang()
    {
        var go = new GameObject("Day3_PolisiDatang");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder + 70;
        AddScaler(go);
        go.AddComponent<GraphicRaycaster>();

        var dim = BuatImage(go.transform, "Dim", new Color(0f, 0f, 0f, 0.6f));
        Stretch(dim.rectTransform);

        // "Mobil polisi" (proxy panel) meluncur dari kiri ke tengah.
        var mobil = BuatImage(go.transform, "Mobil", new Color(0.10f, 0.20f, 0.55f, 1f));
        mobil.sprite = SolidSprite(); mobil.type = Image.Type.Sliced;
        var mRt = mobil.rectTransform;
        mRt.anchorMin = new Vector2(0.5f, 0.5f); mRt.anchorMax = new Vector2(0.5f, 0.5f);
        mRt.pivot = new Vector2(0.5f, 0.5f);
        mRt.sizeDelta = new Vector2(360f, 200f);
        mRt.anchoredPosition = new Vector2(-1400f, 60f);

        // Sirene (kotak yang berkedip merah/biru) di atas mobil.
        var sirene = BuatImage(mobil.transform, "Sirene", Color.red);
        sirene.sprite = SolidSprite();
        var sRt = sirene.rectTransform;
        sRt.anchorMin = new Vector2(0.5f, 1f); sRt.anchorMax = new Vector2(0.5f, 1f);
        sRt.pivot = new Vector2(0.5f, 0f); sRt.sizeDelta = new Vector2(140f, 38f);
        sRt.anchoredPosition = new Vector2(0f, 6f);

        var label = BuatTeks(go.transform, "Label", panicNarasi, 34, Color.white, FontStyles.Bold);
        label.alignment = TextAlignmentOptions.Center;
        var lRt = label.rectTransform;
        lRt.anchorMin = new Vector2(0.1f, 0.18f); lRt.anchorMax = new Vector2(0.9f, 0.36f);
        lRt.offsetMin = Vector2.zero; lRt.offsetMax = Vector2.zero;

        AudioManager.Instance?.Victory();

        float t = 0f;
        float durasi = Mathf.Max(0.5f, panicAnimasiDurasi);
        while (t < durasi)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / (durasi * 0.4f));
            mRt.anchoredPosition = new Vector2(Mathf.Lerp(-1400f, 0f, p), 60f);
            sirene.color = (Mathf.FloorToInt(t * 8f) % 2 == 0)
                ? new Color(0.95f, 0.15f, 0.15f, 1f)
                : new Color(0.15f, 0.35f, 0.95f, 1f);
            yield return null;
        }

        Destroy(go);
    }

    void ProsesPilihanKonfrontasi(PilihanKonfrontasi p)
    {
        if (p == null) return;
        var gs = GameState.Instance;

        gs?.AddChoice(3, p.label, p.kategori);
        if (p.bonusPoin > 0) gs?.AddScore(p.bonusPoin);
        AudioManager.Instance?.PlayKategori(p.kategori);

        if (p.kurangiNyawa)
        {
            gs?.LoseLife();
            if (HUDManager.Instance != null && gs != null)
                HUDManager.Instance.UpdateHearts(gs.lives, gs.maxLives);
        }
    }

    /// <summary>Tampilkan 4 pilihan konfrontasi (PilihanKonfrontasi) di panel pilihan.</summary>
    void TampilkanPilihanKonfrontasi(PilihanKonfrontasi[] pilihan, Action<PilihanKonfrontasi> onPilih)
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
            img.color = p.warna;
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
    // PROMPT TERIAK (Voice MAX) — untuk opsi "JANGAN DEKAT!"
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator PromptTeriak(Action<bool> onResult)
    {
        // Overlay instruksi + meter. Terisi saat teriak ke mic (VoiceMeter Loud)
        // ATAU tahan SPASI / klik (fallback tanpa mikrofon).
        var go = new GameObject("Day3_PromptTeriak");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder + 50;
        AddScaler(go);
        go.AddComponent<GraphicRaycaster>();

        var dim = BuatImage(go.transform, "Dim", new Color(0f, 0f, 0f, 0.55f));
        Stretch(dim.rectTransform);

        var instruksi = BuatTeks(go.transform, "Instruksi",
            "\uD83D\uDDE3 TERIAK SEKUAT TENAGA:\n\"JANGAN DEKAT!\"\n<size=70%>(teriak ke mic, atau TAHAN SPASI / KLIK)</size>",
            40, Color.white, FontStyles.Bold);
        instruksi.alignment = TextAlignmentOptions.Center;
        var iRt = instruksi.rectTransform;
        iRt.anchorMin = new Vector2(0.1f, 0.55f); iRt.anchorMax = new Vector2(0.9f, 0.78f);
        iRt.offsetMin = Vector2.zero; iRt.offsetMax = Vector2.zero;

        // Meter bar
        var barBg = BuatImage(go.transform, "MeterBg", new Color(0.15f, 0.15f, 0.18f, 1f));
        barBg.sprite = SolidSprite(); barBg.type = Image.Type.Sliced;
        var bgRt = barBg.rectTransform;
        bgRt.anchorMin = new Vector2(0.2f, 0.42f); bgRt.anchorMax = new Vector2(0.8f, 0.50f);
        bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(barBg.transform, false);
        var fill = fillGO.AddComponent<Image>();
        fill.color = new Color(0.91f, 0.25f, 0.20f, 1f);
        fill.sprite = SolidSprite();
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.fillAmount = 0f;
        Stretch(fill.rectTransform, 3f, 3f);

        float progress = 0f;
        float waktu = 0f;
        bool sukses = false;
        var vm = VoiceMeter.Instance;

        while (waktu < voiceTimeoutDetik)
        {
            waktu += Time.deltaTime;

            bool teriakMic    = vm != null && vm.IsLoud();
            bool fallbackHold = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)
                                || (vm != null && vm.fallbackButtonHeld);

            if (teriakMic || fallbackHold)
                progress += Time.deltaTime / 0.6f;   // ~0.6s tahan/teriak → penuh
            else
                progress -= Time.deltaTime / 1.2f;    // turun perlahan kalau berhenti

            progress = Mathf.Clamp01(progress);
            fill.fillAmount = progress;

            if (progress >= 1f) { sukses = true; break; }
            yield return null;
        }

        if (sukses) AudioManager.Instance?.PlayKategori("AMAN");
        Destroy(go);
        onResult?.Invoke(sukses);
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
        // (hanya pada mode boss fight; Visual Novel selalu memutar semua ronde berurutan)
        if (!modeVisualNovel && _bossMental <= 0f)
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
        if (p.bonusPoin > 0) gs?.AddScore(p.bonusPoin);
        AudioManager.Instance?.PlayKategori(p.kategori);

        if (p.damage > 0f)
        {
            _bossMental = Mathf.Max(0f, _bossMental - p.damage);
            // SFX 'pukulan' hanya pada mode boss fight; Visual Novel cukup SFX kategori.
            if (!modeVisualNovel) AudioManager.Instance?.BossHit();
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

        // Ending AMAN memberi achievement "Berani Menjaga Diri".
        // Ending LAPOR SUKSES sudah memberi "Pahlawan Diri Sendiri" sebelumnya.
        if (_hasilDay3 != HasilDay3.LaporSukses && !string.IsNullOrEmpty(achievementMenang))
            GameState.Instance?.EarnAchievement(achievementMenang);

        AudioManager.Instance?.Victory();

        if (_pilihanPanel != null) _pilihanPanel.SetActive(false);
        if (_bossNamaText != null) _bossNamaText.text = "";

        // Narasi penutup sesuai jenis ending.
        string narasi = _hasilDay3 == HasilDay3.LaporSukses ? narasiBossKalah : endingAmanNarasi;
        yield return TampilkanUcapan(narasi, isNarasi: true);
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

        // Boss HP Bar (hanya pada mode boss fight; Visual Novel tanpa bar mental)
        if (!modeVisualNovel) BuildBossBar();

        // Kotak ucapan (narasi / ucapan boss) — gaya box dialog Halte:
        // panel kayu berbingkai + portrait di kiri, banner nama, teks, hint.
        // Sprite & layout dipinjam dari komponen HalteDialog supaya tampil PERSIS
        // seperti box dialog Halte. Fallback: panel gelap + outline emas.
        HalteDialog h = gayaHalteDialog;
        Sprite panelSp = h != null ? h.panelSprite : null;
        // Rect panel (fraksi layar) — dipinjam dari Halte agar bingkai PERSIS sama.
        float pCX = h != null ? h.boxPanelCenterX : 0.50f;
        float pCY = h != null ? h.boxPanelCenterY : 0.16f;
        float pW  = h != null ? h.boxPanelWidth   : 0.972f;
        float pH  = h != null ? h.boxPanelHeight  : 0.291f;
        // Anchor relatif (fraksi 0..1) di dalam box — default = nilai Halte.
        float qCX = h != null ? h.boxPortraitCenterX : 0.14f;
        float qCY = h != null ? h.boxPortraitCenterY : 0.584f;
        float qW  = h != null ? h.boxPortraitW : 0.189f;
        float qH  = h != null ? h.boxPortraitH : 0.56f;
        Vector2 bMin = h != null ? h.boxBannerAnchorMin : new Vector2(0.03f, 0.1f);
        Vector2 bMax = h != null ? h.boxBannerAnchorMax : new Vector2(0.253f, 0.333f);
        Vector2 tMin = h != null ? h.boxTextAnchorMin   : new Vector2(0.31f, 0.55f);
        Vector2 tMax = h != null ? h.boxTextAnchorMax   : new Vector2(0.84f, 0.76f);
        float hCX = h != null ? h.boxHintCenterX : 0.798f;
        float hCY = h != null ? h.boxHintCenterY : 0.242f;
        float hW  = h != null ? h.boxHintSizeW   : 0.296f;
        float hH  = h != null ? h.boxHintSizeH   : 0.12f;
        Color namaCol = h != null ? h.boxNamaColor : new Color(1f, 0.85f, 0.30f, 1f);
        Color teksCol = h != null ? h.boxTextColor : Color.white;
        bool boxPA    = h != null ? h.boxPortraitPreserveAspect : false;

        var box = BuatImage(_canvasGO.transform, "KotakUcapan", Color.white);
        box.raycastTarget = true;
        if (panelSp != null)
        {
            box.sprite = panelSp;          // panel kayu sliced (sama Halte)
            box.type   = Image.Type.Sliced;
            box.color  = Color.white;
        }
        else
        {
            box.sprite = SolidSprite();
            box.type   = Image.Type.Sliced;
            box.color  = new Color(0f, 0f, 0f, 0.82f);
            var boxOutline = box.gameObject.AddComponent<Outline>();
            boxOutline.effectColor    = new Color(1f, 0.85f, 0.3f, 1f);
            boxOutline.effectDistance = new Vector2(3f, -3f);
        }
        var boxBtn = box.gameObject.AddComponent<Button>();
        boxBtn.transition    = Selectable.Transition.None;
        boxBtn.targetGraphic = box;
        boxBtn.onClick.AddListener(SkipAtauLanjutUcapan);
        // Rect panel via anchor fraksi layar — IDENTIK Halte (rasio bingkai terjaga).
        var boxRt = box.rectTransform;
        boxRt.anchorMin = new Vector2(pCX - pW * 0.5f, pCY - pH * 0.5f);
        boxRt.anchorMax = new Vector2(pCX + pW * 0.5f, pCY + pH * 0.5f);
        boxRt.pivot = new Vector2(0.5f, 0.5f);
        boxRt.offsetMin = Vector2.zero; boxRt.offsetMax = Vector2.zero;

        // Portrait di bingkai kiri box (gaya Halte) — di-set per pembicara.
        var portraitGO = new GameObject("PortraitUcapan");
        portraitGO.transform.SetParent(box.transform, false);
        var portraitRT = portraitGO.AddComponent<RectTransform>();
        portraitRT.anchorMin = new Vector2(qCX - qW * 0.5f, qCY - qH * 0.5f);
        portraitRT.anchorMax = new Vector2(qCX + qW * 0.5f, qCY + qH * 0.5f);
        portraitRT.offsetMin = portraitRT.offsetMax = Vector2.zero;
        _portraitUcapanImg = portraitGO.AddComponent<Image>();
        _portraitUcapanImg.preserveAspect = boxPA;
        _portraitUcapanImg.color          = Color.white;
        _portraitUcapanImg.raycastTarget  = false;
        _portraitUcapanImg.enabled        = false; // diaktifkan saat ada sprite portrait

        // Banner nama pembicara (di nameplate panel) — gaya Halte
        _namaUcapanText = BuatTeks(box.transform, "NamaPembicara", "", 26, namaCol, FontStyles.Bold);
        _namaUcapanText.alignment = TextAlignmentOptions.Center;
        var namaRt = _namaUcapanText.rectTransform;
        namaRt.anchorMin = bMin; namaRt.anchorMax = bMax;
        namaRt.offsetMin = new Vector2(6f, 2f); namaRt.offsetMax = new Vector2(-6f, -2f);

        // Teks ucapan/narasi — area teks panel (gaya Halte)
        _ucapanText = BuatTeks(box.transform, "Ucapan", "", 28, teksCol, FontStyles.Normal);
        _ucapanText.alignment = TextAlignmentOptions.TopLeft;
        _ucapanText.textWrappingMode = TMPro.TextWrappingModes.Normal;
        var ucRt = _ucapanText.rectTransform;
        ucRt.anchorMin = tMin; ucRt.anchorMax = tMax;
        ucRt.offsetMin = new Vector2(4f, 4f); ucRt.offsetMax = new Vector2(-4f, -4f);

        // Hint "klik untuk lanjut" (kanan-bawah box) — gaya Halte
        _hintLanjutText = BuatTeks(box.transform, "HintLanjut", "▼ SPACE / Klik untuk lanjut", 18, new Color(1f, 1f, 1f, 0.55f), FontStyles.Italic);
        _hintLanjutText.alignment = TextAlignmentOptions.MidlineRight;
        var hintRt = _hintLanjutText.rectTransform;
        hintRt.anchorMin = new Vector2(hCX - hW * 0.5f, hCY - hH * 0.5f);
        hintRt.anchorMax = new Vector2(hCX + hW * 0.5f, hCY + hH * 0.5f);
        hintRt.offsetMin = hintRt.offsetMax = Vector2.zero;
        _hintLanjutText.gameObject.SetActive(false);

        // Reaksi (di ATAS kotak ucapan)
        float panelTopFrac = pCY + pH * 0.5f;
        _reaksiText = BuatTeks(_canvasGO.transform, "Reaksi", "", 24, new Color(1f, 1f, 0.85f, 1f), FontStyles.Italic);
        _reaksiText.alignment = TextAlignmentOptions.Center;
        var rrt = _reaksiText.rectTransform;
        rrt.anchorMin = new Vector2(0.5f, panelTopFrac); rrt.anchorMax = new Vector2(0.5f, panelTopFrac);
        rrt.pivot = new Vector2(0.5f, 0f); rrt.sizeDelta = new Vector2(1500f, 70f);
        rrt.anchoredPosition = new Vector2(0f, 200f);

        // Panel pilihan — di ATAS panel dialog (gaya Halte: tombol muncul di atas box)
        _pilihanPanel = new GameObject("PilihanPanel");
        _pilihanPanel.transform.SetParent(_canvasGO.transform, false);
        var prt = _pilihanPanel.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.5f, panelTopFrac); prt.anchorMax = new Vector2(0.5f, panelTopFrac);
        prt.pivot = new Vector2(0.5f, 0f);
        prt.sizeDelta = new Vector2(1500f, 280f);
        prt.anchoredPosition = new Vector2(0f, 16f);
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
        yield return TampilkanUcapanNama(
            isNarasi ? "Narasi" : bossNama,
            teks,
            isNarasi ? new Color(1f, 0.85f, 0.3f, 1f) : warnaBahaya,
            isNarasi);
    }

    // Versi umum: tampilkan dialog dengan nama pembicara eksplisit (dipakai intro gaya Day 2).
    IEnumerator TampilkanUcapanNama(string namaPembicara, string teks, Color warnaNama, bool italic)
    {
        if (_ucapanText == null) yield break;
        // Banner nama pembicara (gaya box dialog Day 2).
        if (_namaUcapanText != null)
        {
            _namaUcapanText.text  = namaPembicara;
            _namaUcapanText.color = warnaNama;
        }
        // Portrait di bingkai kiri box (gaya Halte) — sesuai pembicara.
        if (_portraitUcapanImg != null)
        {
            Sprite ps = PortraitUntuk(namaPembicara);
            if (ps != null) { _portraitUcapanImg.sprite = ps; _portraitUcapanImg.enabled = true; }
            else            { _portraitUcapanImg.enabled = false; }
        }
        // Label nama boss (di atas potret) hanya tampil bila yang bicara memang si boss.
        if (_bossNamaText != null) _bossNamaText.text = (namaPembicara == bossNama) ? bossNama : "";
        _ucapanText.fontStyle = italic ? FontStyles.Italic : FontStyles.Bold;
        _ucapanText.text = "";
        if (_hintLanjutText != null) _hintLanjutText.gameObject.SetActive(false);

        // Typewriter — bisa di-skip dengan klik box (SkipAtauLanjutUcapan)
        _ucapanSkip = false;
        foreach (char c in teks)
        {
            if (_ucapanSkip) { _ucapanText.text = teks; break; }
            _ucapanText.text += c;
            yield return new WaitForSeconds(0.018f);
        }

        // Tampilkan hint lalu tunggu klik/SPACE untuk lanjut (gaya alur narasi Day 2)
        if (_hintLanjutText != null) _hintLanjutText.gameObject.SetActive(true);
        yield return TungguLanjutUcapan();
        if (_hintLanjutText != null) _hintLanjutText.gameObject.SetActive(false);
    }

    // Dipanggil tombol box: skip typewriter (gaya SkipAtauLanjut Day 2).
    void SkipAtauLanjutUcapan()
    {
        _ucapanSkip = true;
    }

    // Pilih sprite portrait box berdasarkan nama pembicara (gaya Halte).
    // Prioritas: field Day3Controller → portrait HalteDialog → bossSprite.
    Sprite PortraitUntuk(string nama)
    {
        HalteDialog h = gayaHalteDialog;
        if (string.IsNullOrEmpty(nama)) nama = "Narasi";
        string s = nama.ToLower();
        if (s.Contains("rara"))
            return portraitRara   != null ? portraitRara   : (h != null ? h.portraitRara : null);
        if (s.Contains("narasi") || s.Contains("reaksi"))
            return portraitNarasi != null ? portraitNarasi : (h != null ? h.portraitNarasi : null);
        // Selain itu = boss / pria asing.
        Sprite p = portraitBoss != null ? portraitBoss : (h != null ? h.portraitPriaAsing : null);
        return p != null ? p : bossSprite;
    }

    // Tunggu input klik / SPACE / Enter untuk melanjutkan dialog (gaya Day 2).
    IEnumerator TungguLanjutUcapan()
    {
        // Beri 1 frame jeda supaya klik yang men-skip typewriter tidak
        // langsung dianggap sebagai klik lanjut.
        yield return null;
        bool lanjut = false;
        while (!lanjut)
        {
            if (Input.GetMouseButtonDown(0) ||
                Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter))
                lanjut = true;
            yield return null;
        }
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

        bool trauma = _hasilDay3 == HasilDay3.Trauma;

        // Judul
        var judul = BuatTeks(go.transform, "Judul", trauma ? endingTraumaJudul : hasilJudul,
            48, trauma ? warnaBahaya : warnaTeksJudul, FontStyles.Bold);
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
        string pesan = trauma ? endingTraumaNarasi
                     : skor >= ambangLuarBiasa ? pesanLuarBiasa
                     : skor >= ambangBagus     ? pesanBagus
                     : pesanKurang;
        var pesanText = BuatTeks(go.transform, "Pesan", pesan, 26, trauma ? warnaBahaya : warnaTeksJudul, FontStyles.Italic);
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
