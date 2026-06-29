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
        public string reaksi = "Bagus! Pelaku kehilangan nyali.";
        [Tooltip("Sprite latar belakang fullscreen saat REAKSI pilihan ini tampil (gaya Day 2). " +
                 "Kosong = pakai latar arena default.")]
        public Sprite latarReaksi;
    }

    [System.Serializable]
    public class Ronde
    {
        public string namaRonde = "Ronde 1";
        [TextArea(2, 5)]
        [Tooltip("Ejekan/ancaman dari Si Bully untuk ronde ini.")]
        public string ucapanBully = "\"Hai dek, sendirian ya? Ayo ikut om sebentar~\"";
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
        public string reaksi = "Bagus!";
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
        [Tooltip("Sprite latar belakang fullscreen saat REAKSI pilihan ini tampil (gaya Day 2). " +
                 "Kosong = pakai latar arena default.")]
        public Sprite latarReaksi;
    }

    /// <summary>Satu baris dialog intro pembuka Day 3 (gaya naratif Day 2).</summary>
    [System.Serializable]
    public class BarisIntro
    {
        [Tooltip("Nama pembicara. 'Narasi' = kotak narasi kuning; nama lain (mis. 'Rara') = dialog biasa.")]
        public string pembicara = "Narasi";
        [TextArea(2, 4)]
        public string teks = "";
        [Tooltip("Sprite latar belakang fullscreen khusus untuk baris ini (gaya Day 2). " +
                 "Kosong = pakai latar arena default (arenaLatarSprite).")]
        public Sprite latarBelakang;
    }

    /// <summary>
    /// Satu ronde konfrontasi boss yang INTERAKTIF (selaras alur game referensi):
    /// boss bicara → Rara memilih AMAN/RAGU/BAHAYA. Tiap ronde menguras Mental pelaku.
    /// </summary>
    [System.Serializable]
    public class RondeKonfrontasi
    {
        [TextArea(2, 4)]
        [Tooltip("Ucapan/ancaman boss di ronde ini.")]
        public string ucapanBoss = "\"...\"";
        [Tooltip("Pilihan respons Rara (AMAN/RAGU/BAHAYA). AMAN menguras Mental paling banyak.")]
        public PilihanKonfrontasi[] pilihan;
        [Tooltip("Sprite latar belakang fullscreen khusus untuk ronde ini (gaya Day 2). " +
                 "Kosong = pakai latar arena default (arenaLatarSprite).")]
        public Sprite latarBelakang;
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
    [Tooltip("Sprite latar overlay judul BossIntro (opsional). Jika diisi, menggantikan warna solid warnaBackground.")]
    public Sprite spriteBackground;
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
        "TUNGGU! Rara mau naik ojol...\ntapi seseorang tiba-tiba menghadang jalannya!\n" +
        "Itu dia \u2014 si pengirim pesan tadi \u2014 muncul langsung di depan Rara!!";
    [Tooltip("Sprite latar belakang fullscreen saat narasi pembuka boss tampil (gaya Day 2). " +
             "Kosong = pakai latar arena default (arenaLatarSprite).")]
    public Sprite narasiPembukaLatar;

    [Header("Intro Pembuka Day 3 (gaya Day 2)")]
    [Tooltip("Tampilkan dialog pembuka (bel pulang, hujan, pesan ojol) sebelum jalan ke parkiran.")]
    public bool jalankanIntroPembuka = true;
    [Tooltip("Baris dialog pembuka. 'Narasi' = kotak narasi kuning; nama lain = dialog pembicara.")]
    public BarisIntro[] introBaris = new BarisIntro[]
    {
        // Catatan: cerita pembuka (hujan, pulang sendiri, ojol) sudah diceritakan di
        // Day3PrologScreen. Baris di sini sengaja dibuat ringkas sebagai JEMBATAN ke
        // segmen jalan supaya tidak mengulang prolog.
        new BarisIntro { pembicara = "Narasi",
            teks = "Hujan makin deras. Rara harus cepat menuju parkiran." },
        new BarisIntro { pembicara = "Narasi",
            teks = "Tap layar / TERIAK buat jalan ke parkiran!\nSemakin keras teriak = makin cepet jalannya!" }
    };

    [Header("Jalan di Hujan (menuju parkiran)")]
    [Tooltip("Segmen jalan kaki menembus hujan menuju parkiran (TAP / TERIAK untuk maju).")]
    public bool jalankanJalanHujan = true;
    [Tooltip("Sprite latar belakang fullscreen segmen LARI di hujan (gaya Day 2). " +
             "Kosong = warna gelap hujan solid.")]
    public Sprite jalanHujanLatar;
    [Tooltip("Sprite latar belakang fullscreen saat narasi TIBA di parkiran tampil (gaya Day 2). " +
             "Kosong = pakai latar arena default (arenaLatarSprite).")]
    public Sprite jalanSampaiLatar;
    [TextArea(2, 3)]
    public string jalanInstruksi = "Jalan ke parkiran sekolah \u2014 TERIAK buat lari lebih cepat!";
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
        "Rara akhirnya sampai di parkiran. Basah kuyup kena hujan!\nDia langsung buka HP buat ngecek ojol-nya udah nyampe belum...",
        "\"Eh?! Ada notif dari nomor yang nggak aku kenal?!\nSiapa nih... *deg-degan banget*\""
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
        "Hai cantik! Hujan deras ya Hati-hati basah...",
        "Mau jemput? Gratis kok, kasihan kamu basah sendirian!",
        "Eh, foto kamu pakai seragam dong... buat om simpan ya sayang~"
    };
    [Tooltip("Detik tersisa untuk memilih respons chat.")]
    public float chatTimerDetik = 6f;
    [Tooltip("Sprite latar belakang fullscreen device chat agresif (di belakang frame HP). " +
             "Kosong = latar device default.")]
    public Sprite chatAgresifLatar;
    [Tooltip("Referensi ChatSimWhatsApp opsional. Kosong = dibuat otomatis dengan data di atas.")]
    public ChatSimWhatsApp chatAgresif;

    [Header("Tantangan 2 — Ojol Palsu")]
    [Tooltip("Jalankan adegan 'ojek online palsu' menawarkan tumpangan gratis sebelum boss.")]
    public bool jalankanOjolPalsu = true;
    [TextArea(2, 4)]
    public string ojolNarasi =
        "Yes! Rara nggak terpancing pesan mencurigakan itu!\n" +
        "Nah, ojol pesanan Rara baru aja tiba di parkiran!\n" +
        "Tapi jangan langsung naik \u2014 cek plat nomornya dulu ya!";
    public string ojolNamaSpeaker = "Ojek Online (?)";
    [TextArea(1, 3)]
    public string ojolUcapan = "\"Ayo naik, gratis! Cepetan, keburu makin deras nih!\"";
    [Tooltip("Sprite latar belakang fullscreen adegan ojol palsu (gaya Day 2). " +
             "Kosong = pakai latar arena default (arenaLatarSprite).")]
    public Sprite ojolLatarBelakang;
    public PilihanRonde[] ojolPilihan = new PilihanRonde[]
    {
        new PilihanRonde { label = "Foto plat dulu, lalu tolak naik", kategori = "AMAN", bonusPoin = 100,
            reaksi = "Cerdas! Kamu foto plat sebagai bukti, lalu menolak dengan sopan. Jangan naik kendaraan orang asing." },
        new PilihanRonde { label = "\"Makasih, saya jalan kaki saja.\"", kategori = "AMAN",
            reaksi = "Bagus, kamu menolak dengan tegas dan tetap waspada." },
        new PilihanRonde { label = "Naik saja, mumpung gratis", kategori = "BAHAYA",
            reaksi = "Bahaya! Jangan pernah naik kendaraan orang asing meski gratis. Kamu kehilangan 1 nyawa." }
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
        "\"Eh hei, mau kemana sendirian? Ikut aku dulu deh. Sebentar aja kok~\"",
        "\"Sssst! Jangan teriak-teriak, nanti kamu yang dimarahin orang. Diam aja ya~\"",
        "\"Haha, emangnya siapa yang bakal percaya sama kamu? Nggak ada! Diam aja~\"",
        "\"Ini rahasia kita berdua ya. Kalau kamu ngadu, kamu sendiri yang bakal kena masalah!\""
    };
    [Tooltip("RONDE KONFRONTASI INTERAKTIF (selaras alur game referensi). Tiap baris grooming jadi " +
             "satu ronde: boss bicara \u2192 Rara memilih AMAN/RAGU/BAHAYA, menguras Mental pelaku. " +
             "Kosong = pakai mode lama (groomingLines diketik tanpa pilihan).")]
    public RondeKonfrontasi[] groomingRonde = new RondeKonfrontasi[]
    {
        new RondeKonfrontasi {
            ucapanBoss = "\"Eh hei, mau kemana sendirian? Ikut aku dulu deh. Sebentar aja kok~\"",
            pilihan = new PilihanKonfrontasi[]
            {
                new PilihanKonfrontasi {
                    label = "\"PERGI! Aku NGGAK KENAL kamu! TOLONG!!\"", kategori = "AMAN", bonusPoin = 50,
                    reaksi = "BERANI! Suara Rara bikin pelaku kaget dan mundur selangkah.",
                    warna = new Color(0.15f, 0.68f, 0.38f, 1f)
                },
                new PilihanKonfrontasi {
                    label = "\"E-emm... nggak usah deh...\" (suara pelan)", kategori = "RAGU",
                    reaksi = "Kurang tegas. Lain kali bersuara lebih lantang ya!",
                    warna = new Color(0.95f, 0.61f, 0.07f, 1f)
                },
                new PilihanKonfrontasi {
                    label = "(diam, bingung mau gimana...)", kategori = "BAHAYA",
                    reaksi = "DIAM ITU BAHAYA! Pelaku makin berani. Kamu kehilangan 1 nyawa.",
                    kurangiNyawa = true,
                    warna = new Color(0.50f, 0.20f, 0.20f, 1f)
                }
            }
        },
        new RondeKonfrontasi {
            ucapanBoss = "\"Sssst! Jangan teriak-teriak, nanti kamu yang dimarahin orang. Diam aja ya~\"",
            pilihan = new PilihanKonfrontasi[]
            {
                new PilihanKonfrontasi {
                    label = "\"JANGAN DEKET-DEKET! TOLONG!!\" (Teriak KERAS!)", kategori = "AMAN", bonusPoin = 50,
                    reaksi = "HEBAT! Teriakan Rara menggema. Pelaku makin ciut nyalinya.",
                    warna = new Color(0.15f, 0.68f, 0.38f, 1f)
                },
                new PilihanKonfrontasi {
                    label = "\"T-tolong...\" (hampir nggak kedengeran)", kategori = "RAGU",
                    reaksi = "Suaramu kepelanan. TERIAK sekuat tenaga lain kali!",
                    warna = new Color(0.95f, 0.61f, 0.07f, 1f)
                },
                new PilihanKonfrontasi {
                    label = "(beku di tempat, nggak bisa ngomong...)", kategori = "BAHAYA",
                    reaksi = "Rara beku ketakutan. Pelaku makin mendesak. Kehilangan 1 nyawa.",
                    kurangiNyawa = true,
                    warna = new Color(0.50f, 0.20f, 0.20f, 1f)
                }
            }
        },
        new RondeKonfrontasi {
            ucapanBoss = "\"Haha, emangnya siapa yang bakal percaya sama kamu? Nggak ada! Diam aja~\"",
            pilihan = new PilihanKonfrontasi[]
            {
                new PilihanKonfrontasi {
                    label = "\"PERGI! Aku PERCAYA SAMA DIRI SENDIRI! TOLONG!!\"", kategori = "AMAN", bonusPoin = 50,
                    reaksi = "KEREN! Rara percaya diri. Mental pelaku makin jatuh.",
                    warna = new Color(0.15f, 0.68f, 0.38f, 1f)
                },
                new PilihanKonfrontasi {
                    label = "\"Emangnya... kenapa sih?\" (masih ragu-ragu)", kategori = "RAGU",
                    reaksi = "Jangan terpancing. Tetap tegas menolak ya!",
                    warna = new Color(0.95f, 0.61f, 0.07f, 1f)
                },
                new PilihanKonfrontasi {
                    label = "(nangis diem-diem, nggak berani berbuat apa-apa)", kategori = "BAHAYA",
                    reaksi = "Rara terlalu takut. Pelaku menang sesaat. Kehilangan 1 nyawa.",
                    kurangiNyawa = true,
                    warna = new Color(0.50f, 0.20f, 0.20f, 1f)
                }
            }
        },
        new RondeKonfrontasi {
            ucapanBoss = "\"Ini rahasia kita berdua ya. Kalau kamu ngadu, kamu sendiri yang bakal kena masalah!\"",
            pilihan = new PilihanKonfrontasi[]
            {
                new PilihanKonfrontasi {
                    label = "\"Bohong! AKU BAKAL CERITA ke guru sekarang!\"", kategori = "AMAN", bonusPoin = 50,
                    reaksi = "TEPAT! Rahasia jahat HARUS diceritakan. Pelaku panik!",
                    warna = new Color(0.15f, 0.68f, 0.38f, 1f)
                },
                new PilihanKonfrontasi {
                    label = "\"Aku nggak tau harus ngapain...\" (bingung banget)", kategori = "RAGU",
                    reaksi = "Ingat: kamu boleh cerita ke orang dewasa yang dipercaya!",
                    warna = new Color(0.95f, 0.61f, 0.07f, 1f)
                },
                new PilihanKonfrontasi {
                    label = "\"Mungkin... emang salah aku ya...\" (mulai pasrah)", kategori = "BAHAYA",
                    reaksi = "Ini BUKAN salahmu! Jangan pasrah. Kehilangan 1 nyawa.",
                    kurangiNyawa = true,
                    warna = new Color(0.50f, 0.20f, 0.20f, 1f)
                }
            }
        }
    };
    [Tooltip("Ucapan boss saat mendesak Rara memutuskan (muncul bersama pilihan).")]
    [TextArea(1, 3)]
    public string konfrontasiUcapan = "\"Pasrah aja lah! Nggak ada yang bisa nolongin kamu di sini!\"";
    [Tooltip("Sprite latar belakang fullscreen khusus untuk konfrontasi pamungkas (gaya Day 2). " +
             "Kosong = pakai latar arena default (arenaLatarSprite).")]
    public Sprite konfrontasiLatarBelakang;
    [Tooltip("4 pilihan pamungkas: Diam / 'Makasih' / 'JANGAN DEKAT!'+Voice / PANIC BUTTON.")]
    public PilihanKonfrontasi[] konfrontasiPilihan = new PilihanKonfrontasi[]
    {
        new PilihanKonfrontasi {
            label = "(beku di tempat, nggak bisa ngomong...)", kategori = "BAHAYA",
            reaksi = "Rara beku ketakutan, dan dia jadi makin berani. Kehilangan 1 nyawa \u2014 ingat, DIAM ITU BAHAYA. Kamu harus bersuara!",
            kurangiNyawa = true, hasil = HasilDay3.Lanjut,
            warna = new Color(0.50f, 0.20f, 0.20f, 1f)
        },
        new PilihanKonfrontasi {
            label = "\"T-tolong...\" (suaranya hampir nggak kedengeran)", kategori = "RAGU",
            reaksi = "Rara berhasil pergi, tapi suaranya pelan banget. Lain kali TERIAK yang keras ya!",
            hasil = HasilDay3.Aman,
            warna = new Color(0.95f, 0.61f, 0.07f, 1f)
        },
        new PilihanKonfrontasi {
            label = "\"JANGAN DEKET-DEKET! TOLONG!!\" (Teriak KERAS!)", kategori = "AMAN", bonusPoin = 300,
            reaksi = "HEBAT! Teriakan Rara bikin dia kaget dan langsung mundur. Berani bersuara itu kekuatan!",
            butuhVoiceKeras = true, hasil = HasilDay3.Aman,
            warna = new Color(0.15f, 0.68f, 0.38f, 1f)
        },
        new PilihanKonfrontasi {
            label = "TERIAK SEKERAS-KERASNYA + lari ke satpam!", kategori = "LAPOR",
            reaksi = "LAPOR SUKSES! Rara teriak minta tolong dan lari ke satpam. Guru dan satpam langsung datang \u2014 Rara pahlawan buat dirinya sendiri!",
            panicButton = true, hasil = HasilDay3.LaporSukses,
            warna = new Color(0.20f, 0.62f, 0.86f, 1f)
        }
    };
    [Tooltip("Durasi maksimum jendela teriak (detik) untuk opsi 'JANGAN DEKAT!'.")]
    public float voiceTimeoutDetik = 5f;
    [Tooltip("Sprite Rara berteriak yang muncul di layar teriak (Voice MAX). " +
             "Kosong = tidak menampilkan sprite.")]
    public Sprite spriteTeriak;
    [Tooltip("Sprite latar belakang fullscreen layar TERIAK / Panic Button (Voice MAX). " +
             "Kosong = pakai latar arena default (arenaLatarSprite).")]
    public Sprite teriakLatarBelakang;
    [Tooltip("Ukuran sprite teriak (px) di kanan-bawah layar teriak.")]
    public Vector2 spriteTeriakUkuran = new Vector2(420f, 520f);
    [Header("Boss — Panic Button (Polisi Datang)")]
    [Tooltip("Label tombol darurat.")]
    public string panicLabel = "PANIC BUTTON";
    [Tooltip("Narasi saat panic button ditekan (polisi/guru datang).")]
    [TextArea(2, 4)]
    public string panicNarasi = "\"HEEEI! Ada apa ini?! Kami denger ada yang teriak!\"\n" +
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
    [Tooltip("Sprite latar belakang fullscreen ending AMAN / LAPOR SUKSES (gaya Day 2). " +
             "Kosong = pakai latar hasil default (hasilLatarSprite).")]
    public Sprite endingAmanLatar;
    [TextArea(2, 4)]
    public string endingTraumaNarasi =
        "Rasa takut bikin Rara nggak berani bertindak, dan keadaannya jadi berbahaya. " +
        "Tapi tenang \u2014 jangan menyerah! Ayo coba lagi dan belajar cara menjaga diri.";
    [Tooltip("Sprite latar belakang fullscreen ending TRAUMA / Game Over (gaya Day 2). " +
             "Kosong = pakai latar hasil default (hasilLatarSprite).")]
    public Sprite endingTraumaLatar;
    public string endingTraumaJudul = "GAME OVER";

    [Header("Backdrop Procedural")]
    public bool buatBackdrop = true;
    public Color warnaBackdrop = new Color(0.10f, 0.12f, 0.18f, 1f); // suram, hujan
    public int   backdropSortingOrder = -100;

    [Header("Latar Arena (sprite fullscreen, gaya box dialog Day 2)")]
    [Tooltip("Sprite latar belakang fullscreen untuk arena/box dialog Hari 3 (mis. parkiran SMP saat hujan). " +
             "Kosong = pakai warna gelap solid.")]
    public Sprite arenaLatarSprite;
    public Color  arenaLatarWarna = new Color(0.10f, 0.12f, 0.18f, 1f);

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
    [Tooltip("Sprite latar belakang box dialog Day 3 (dipakai SEMUA dialog yang memakai box). " +
             "Kosong = ambil panel kayu dari HalteDialog, lalu fallback panel gelap + outline emas.")]
    public Sprite boxDialogSprite;
    [Tooltip("Portrait untuk pembicara Narasi (mis. ikon gulungan/scroll). Kosong = ambil dari HalteDialog.")]
    public Sprite portraitNarasi;
    [Tooltip("Portrait untuk pembicara Boss/Pria Asing. Kosong = ambil dari HalteDialog, lalu bossSprite.")]
    public Sprite portraitBoss;
    [Tooltip("Portrait untuk pembicara Rara. Kosong = ambil dari HalteDialog.")]
    public Sprite portraitRara;

    [Header("Boss HP Bar")]
    public string bossBarLabel = "Mental Pelaku";
    public Color  bossBarWarnaIsi  = new Color(0.85f, 0.20f, 0.22f, 1f);
    public Color  bossBarWarnaKosong = new Color(0.15f, 0.15f, 0.18f, 1f);

    [Header("Ronde Boss Fight (CUSTOMIZABLE)")]
    public Ronde[] rondeList = new Ronde[]
    {
        new Ronde {
            namaRonde   = "Ronde 1 \u2014 Bujukan",
            ucapanBully = "\"Hai dek, sendirian ya? Ayo ikut om sebentar, om beliin jajan kesukaanmu deh~\"",
            pilihan = new PilihanRonde[]
            {
                new PilihanRonde { label = "\"TIDAK! Aku nggak kenal om.\"", kategori = "AMAN", damage = 40f,
                    reaksi = "Tegas! Kata sakti TIDAK. Pelaku kaget kamu berani menolak." },
                new PilihanRonde { label = "\"Eh... nggak usah deh, om...\"", kategori = "RAGU", damage = 20f,
                    reaksi = "Kurang tegas. Dia masih coba membujukmu." },
                new PilihanRonde { label = "Diam & ragu-ragu mau ikut", kategori = "BAHAYA", damage = 0f,
                    reaksi = "Dia makin memaksa. Kamu kehilangan 1 nyawa." }
            }
        },
        new Ronde {
            namaRonde   = "Ronde 2 \u2014 Rahasia & Ancaman",
            ucapanBully = "\"Sssst, ini rahasia kita berdua ya. Kalau kamu ngadu, kamu sendiri yang bakal kena masalah!\"",
            pilihan = new PilihanRonde[]
            {
                new PilihanRonde { label = "\"Aku PERGI dari sini. Nggak ada rahasia sama orang asing.\"", kategori = "AMAN", damage = 40f,
                    reaksi = "Mantap! Kata sakti PERGI. Nyali pelaku makin ciut." },
                new PilihanRonde { label = "\"I-iya deh, aku nggak bakal cerita...\"", kategori = "RAGU", damage = 20f,
                    reaksi = "Dia merasa kamu bisa ditakut-takuti." },
                new PilihanRonde { label = "Menurut & janji simpan rahasia", kategori = "BAHAYA", damage = 0f,
                    reaksi = "Justru itu jebakannya. Kamu kehilangan 1 nyawa." }
            }
        },
        new Ronde {
            namaRonde   = "Ronde 3 \u2014 Cari Bantuan",
            ucapanBully = "\"Mau apa kamu? Di sini cuma ada kita berdua, nggak ada yang nolongin!\"",
            pilihan = new PilihanRonde[]
            {
                new PilihanRonde { label = "Teriak \"TOLONG!\" & lari CERITA ke satpam", kategori = "AMAN", damage = 50f,
                    reaksi = "Hebat! Kata sakti CERITA. Satpam datang, pelaku kabur!" },
                new PilihanRonde { label = "\"Aku... aku tunggu guru aja deh.\"", kategori = "RAGU", damage = 20f,
                    reaksi = "Lumayan, tapi kamu masih ragu cari bantuan." },
                new PilihanRonde { label = "Ikut saja ke tempat sepi", kategori = "BAHAYA", damage = 0f,
                    reaksi = "BAHAYA besar! Kamu kehilangan 1 nyawa." }
            }
        }
    };

    [Header("Saat Boss Kalah / Lapor Sukses")]
    [TextArea(2, 5)]
    public string narasiBossKalah =
        "\"Tenang Rara, kamu udah berani banget! Kamu nggak salah sama sekali.\"\n" +
        "Guru dan satpam bakal bantu laporin ke polisi. Rara berani cerita \u2014 itu pilihan PALING TEPAT!";
    [Tooltip("Achievement yang diraih saat selamat dengan berani (ending AMAN).")]
    public string achievementMenang = "Berani Menjaga Diri";

    [Header("Kartu Edukasi Hari 3")]
    public string eduJudul = "Kartu Edukasi \u2014 Hari 3: FINAL";
    public Color  eduWarnaJudul = new Color(1f, 0.85f, 0.30f, 1f);
    [Tooltip("Sprite latar belakang penuh layar di belakang kartu edukasi (opsional). " +
             "Kosong = warna gelap solid.")]
    public Sprite eduLatarSprite;
    public Color  eduLatarWarna = new Color(0f, 0f, 0f, 0.82f);
    [TextArea(4, 10)]
    public string eduIsi =
        "Apa itu Grooming?\n" +
        "Grooming = orang dewasa yang pura-pura 'baik' buat mendekati anak \u2014 lewat chat, sosmed, atau ketemu langsung. Ini KEJAHATAN. Kamu boleh lapor!\n\n" +
        "Cara Melindungi Diri:\n" +
        "\u2022 Ingat 3 KATA SAKTI: TIDAK! \u2014 PERGI! \u2014 CERITA!\n" +
        "\u2022 Terasa nggak aman? TERIAK keras dan minta tolong!\n" +
        "\u2022 Chat mencurigakan? Blokir + screenshot + cerita ke ortu.\n" +
        "\u2022 Guru dan polisi ADA untuk melindungi kamu!\n\n" +
        "Yang Paling Penting:\n" +
        "Kalau kamu jadi korban, itu BUKAN salahmu! Berani cerita ke orang yang dipercaya = tindakan paling berani yang bisa kamu lakuin!\n\n" +
        "Darurat: Polisi 110  |  Hotline Anak 129  |  KPAI 021-31901556";

    [Header("Layar Hasil Akhir (Complete)")]
    public string hasilJudul = "TANTANGAN SELESAI!";
    [Tooltip("Sprite latar belakang penuh layar di belakang kartu hasil (opsional). Kosong = warna solid gelap.")]
    public Sprite hasilLatarSprite;
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
    private Image      _arenaBgImg;            // latar belakang fullscreen arena (bisa diganti per baris dialog)
    private Image      _bossBarFill;
    private TextMeshProUGUI _bossBarText;
    private TextMeshProUGUI _bossNamaText;
    private TextMeshProUGUI _ucapanText;
    private TextMeshProUGUI _namaUcapanText;   // banner nama pembicara (gaya box dialog Day 2)
    private TextMeshProUGUI _hintLanjutText;   // hint "klik untuk lanjut" (gaya Day 2)
    private TombolLanjutVN  _tombolLanjutUcapan; // tombol LANJUT box ucapan
    private Image      _portraitUcapanImg;     // portrait di bingkai kiri box (gaya Halte)
    private TextMeshProUGUI _reaksiText;
    private Image      _reaksiBox;             // box latar di belakang teks reaksi (gaya box dialog)
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
            gayaHalteDialog = FindFirstObjectByType<HalteDialog>(FindObjectsInactive.Include);
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
                // Hanya tampilkan portrait boss bila ADA sprite-nya. Tanpa sprite,
                // placeholder kotak merah (bossWarnaFallback) tidak usah muncul.
                if (_bossImg != null) _bossImg.enabled = (bossSprite != null);
                if (_bossNamaText != null) _bossNamaText.text = bossNama;
                GantiLatarArena(narasiPembukaLatar);
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
                // Ganti latar belakang fullscreen sesuai baris ini (gaya Day 2).
                GantiLatarArena(b.latarBelakang);
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

        // Latar hujan gelap (atau sprite latar kustom bila diisi).
        var bg = BuatImage(go.transform, "BG", jalanHujanLatar != null ? Color.white : new Color(0.10f, 0.13f, 0.20f, 1f));
        if (jalanHujanLatar != null) bg.sprite = jalanHujanLatar;
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
        var runner = BuatTeks(barBg.transform, "Runner", "", 40, Color.white, FontStyles.Normal);
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
        var shLbl = BuatTeks(shoutImg.transform, "Lbl", "TERIAK (tahan)", 28, Color.white, FontStyles.Bold);
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
            runner.text = teriak ? "" : "";

            int sisa = Mathf.CeilToInt(totalDist - jalan);
            jarakTxt.text = sisa > 0
                ? "Parkiran " + sisa + " m lagi..."
                : "Hampir sampai!";
            yield return null;
        }

        Destroy(go);

        // Narasi tiba di parkiran (gaya Day 2) — pakai kotak dialog arena tanpa boss.
        BuildArena();
        if (_bossImg != null) _bossImg.enabled = false;
        if (_bossNamaText != null) _bossNamaText.text = "";
        GantiLatarArena(jalanSampaiLatar);
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
        chat.jamTampil          = "13:45";    // jam pulang sekolah anak SMP (Hari 3, hujan)
        chat.hariUntukSkor      = 3;
        chat.notifBerderingKali = 3;          // SFX notif WA berdering 3x
        chat.waktuPilihDetik    = chatTimerDetik;
        chat.fontAsset          = fontAsset;
        chat.sortingOrder       = sortingOrder + 30;
        chat.bgFullscreenSprite = chatAgresifLatar;

        // Pesan masuk (auto-type, 3 detik per pesan).
        var pesan = new List<ChatSimWhatsApp.PesanData>();
        if (chatPesan != null)
            foreach (var teks in chatPesan)
                pesan.Add(new ChatSimWhatsApp.PesanData { teks = teks, delayDetik = 3f });
        chat.pesanMasuk = pesan.ToArray();

        // Tombol screenshot bonus = "Screenshot Dulu" (+100 bukti).
        chat.tampilkanTombolScreenshot = true;
        chat.screenshotLabel = "Screenshot dulu buat bukti";
        chat.screenshotBonus = 100;
        chat.screenshotAchievement = "Detektif Bukti"; // ambil bukti = achievement

        // 3 pilihan utama sesuai alur Hari 3.
        chat.aksiList = new ChatSimWhatsApp.AksiData[]
        {
            new ChatSimWhatsApp.AksiData {
                label = "Oke, ini foto seragamku~", kategori = "BAHAYA",
                reaksi = "STOP! Jangan kirim foto ke orang yang nggak kamu kenal! Foto bisa dipakai buat memeras atau mengancam kamu. Kamu kehilangan 1 nyawa.",
                kurangiNyawa = true,
                warna = warnaBahaya
            },
            new ChatSimWhatsApp.AksiData {
                label = "Iya Om, aku di parkiran SMP. Jemput ya!", kategori = "BAHAYA",
                reaksi = "GAME OVER! Rara pergi sama orang nggak dikenal dari internet! Jangan PERNAH kasih lokasi atau minta dijemput orang asing.",
                akhiriGameOver = true,
                warna = new Color(0.50f, 0.16f, 0.16f, 1f)
            },
            new ChatSimWhatsApp.AksiData {
                label = "BLOKIR sekarang + lapor ke ortu!", kategori = "AMAN",
                reaksi = "TEPAT! Blokir nomornya, terus cerita ke orang tua. Itulah cara pahlawan menjaga diri! (+200)",
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
        GantiLatarArena(ojolLatarBelakang);

        // Pengemudi ojol palsu bicara (bukan boss parkir). Sembunyikan portrait boss
        // supaya placeholder merah (bossWarnaFallback) tidak muncul di tengah arena.
        if (_bossImg != null) _bossImg.enabled = false;
        if (_bossNamaText != null) _bossNamaText.text = ojolNamaSpeaker;
        yield return TampilkanUcapan(ojolNarasi, isNarasi: true);
        yield return new WaitForSeconds(0.3f);
        yield return TampilkanUcapan(ojolUcapan, isNarasi: false);

        // Kartu plat: plat PESANAN selalu tampil, plat ANGKOT awalnya tertutup ("?").
        TextMeshProUGUI platPesananText, platAngkotText, platHeaderText;
        var platPanel = BuatPanelPlat(out platPesananText, out platAngkotText, out platHeaderText);

        // ── Level 1: cek plat dulu, atau langsung naik (fatal). ───────────────
        int pilih1 = -1;
        _menungguPilihan = true;
        TampilkanTombolKustom(new (string, Color)[]
        {
            ("Cek & bandingkan plat dulu", warnaNetral),
            ("Naik saja (gratis!)",        warnaBahaya)
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
            yield return KetikReaksi("Rara naik ojol palsu! Selalu cocokin plat di aplikasi sama plat di motor. Kalau beda, jangan naik! GAME OVER.");
            yield return new WaitForSeconds(2.4f);
            if (platPanel != null) Destroy(platPanel);
            _hasilDay3 = HasilDay3.Trauma;
            GotoFase(Phase.EduCard);
            yield break;
        }

        // Buka plat angkot untuk dibandingkan.
        AudioManager.Instance?.Click();
        if (platAngkotText != null) platAngkotText.text = ojolPlatAngkot;
        // Instruksi perbandingan ditaruh di HEADER kartu plat (bukan _reaksiText) agar
        // tidak menumpuk dengan panel tombol pilihan yang muncul di bawah.
        if (platHeaderText != null) platHeaderText.text = "COCOK NGGAK SAMA PESANAN RARA?";
        if (_reaksiText != null) _reaksiText.text = "";
        yield return new WaitForSeconds(0.6f);

        // ── Level 2: cocokkan plat. Plat sengaja BERBEDA → jawaban benar: TIDAK COCOK. ──
        int pilih2 = -1;
        _menungguPilihan = true;
        TampilkanTombolKustom(new (string, Color)[]
        {
            ("\u274C TIDAK COCOK \u2014 tolak naik", warnaAman),
            ("Cocok \u2014 naik saja",        warnaBahaya)
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
            yield return KetikReaksi("Platnya jelas beda, tapi Rara tetap naik! Rara naik ojol palsu. GAME OVER.");
            yield return new WaitForSeconds(2.4f);
            if (platPanel != null) Destroy(platPanel);
            _hasilDay3 = HasilDay3.Trauma;
            GotoFase(Phase.EduCard);
            yield break;
        }

        // Benar: plat tidak cocok → tolak naik (AMAN + bonus bukti).
        GameState.Instance?.AddChoice(3, "Ojol: cek plat, tidak cocok, tolak naik", "AMAN");
        if (ojolBonusCekPlat > 0) GameState.Instance?.AddScore(ojolBonusCekPlat);
        GameState.Instance?.TambahBukti(GameState.BUKTI_PLAT_DAY3); // B2 — bukti cek plat Hari 3
        AudioManager.Instance?.PlayKategori("AMAN");
        yield return KetikReaksi("PLAT BENAR dicek! Platnya beda = ojol PALSU. Rara nggak naik. Pinter! (+" + ojolBonusCekPlat + " bukti)");
        yield return new WaitForSeconds(2.2f);
        if (_reaksiText != null) _reaksiText.text = "";
        if (platPanel != null) Destroy(platPanel);

        GotoFase(Phase.BossIntro);
    }

    /// <summary>Bangun kartu plat (pesanan + angkot) di tengah-atas arena. Plat angkot awalnya "?".</summary>
    GameObject BuatPanelPlat(out TextMeshProUGUI platPesananText, out TextMeshProUGUI platAngkotText, out TextMeshProUGUI headerText)
    {
        var panel = new GameObject("PanelPlat");
        panel.transform.SetParent(_canvasGO.transform, false);
        var pRt = panel.AddComponent<RectTransform>();
        // Digantung dari ATAS layar (di bawah navbar) agar TIDAK menutupi panel
        // pilihan tombol yang berada di tengah-bawah arena (panelTopFrac).
        pRt.anchorMin = new Vector2(0.5f, 1f); pRt.anchorMax = new Vector2(0.5f, 1f);
        pRt.pivot = new Vector2(0.5f, 1f);
        pRt.sizeDelta = new Vector2(960f, 320f);
        // Diberi jarak lebih ke bawah agar tidak berdempetan dengan indikator hari
        // (1-2-3) navbar di atas — enak dipandang.
        pRt.anchoredPosition = new Vector2(0f, -150f);

        // Backdrop gelap membulat + bingkai emas (selaras tema kayu/sunset).
        var bg = panel.AddComponent<Image>();
        bg.sprite = GetRoundedSpritePlat();
        bg.type   = Image.Type.Sliced;
        bg.color  = new Color(0.10f, 0.07f, 0.05f, 0.96f);
        bg.raycastTarget = false;
        var bgOutline = panel.AddComponent<Outline>();
        bgOutline.effectColor    = new Color(0.95f, 0.72f, 0.18f, 0.90f);
        bgOutline.effectDistance = new Vector2(2.5f, -2.5f);

        // Header judul tantangan.
        var header = BuatTeks(panel.transform, "Header", "COCOKKAN PLAT NOMOR", 26,
            new Color(1f, 0.85f, 0.40f, 1f), FontStyles.Bold);
        header.alignment = TextAlignmentOptions.Center;
        header.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        var hRt = header.rectTransform;
        hRt.anchorMin = new Vector2(0f, 1f); hRt.anchorMax = new Vector2(1f, 1f);
        hRt.pivot     = new Vector2(0.5f, 1f);
        hRt.offsetMin = new Vector2(16f, -54f); hRt.offsetMax = new Vector2(-16f, -14f);
        headerText = header;

        // Area kartu (di bawah header).
        var area = new GameObject("AreaKartu");
        area.transform.SetParent(panel.transform, false);
        var aRt = area.AddComponent<RectTransform>();
        aRt.anchorMin = new Vector2(0f, 0f); aRt.anchorMax = new Vector2(1f, 1f);
        aRt.offsetMin = new Vector2(18f, 18f); aRt.offsetMax = new Vector2(-18f, -60f);

        // Kartu kiri: plat pesanan (acuan).
        platPesananText = BuatKartuPlat(area.transform, "Plat Pesanan (dari Ibu)",
            ojolPlatPesanan, new Color(0.12f, 0.42f, 0.30f, 1f), new Vector2(-0.5f, 0f));
        // Kartu kanan: plat angkot (awalnya tertutup).
        platAngkotText = BuatKartuPlat(area.transform, "Plat Ojol Ini",
            "?  ?  ?", new Color(0.45f, 0.16f, 0.16f, 1f), new Vector2(0.5f, 0f));

        // Badge "VS" emas di tengah antar kartu.
        var badge = BuatImage(area.transform, "BadgeVS", new Color(0.95f, 0.72f, 0.18f, 1f));
        badge.sprite = GetRoundedSpritePlat();
        badge.type   = Image.Type.Sliced;
        badge.raycastTarget = false;
        var badRt = badge.rectTransform;
        badRt.anchorMin = new Vector2(0.5f, 0.5f); badRt.anchorMax = new Vector2(0.5f, 0.5f);
        badRt.pivot = new Vector2(0.5f, 0.5f);
        badRt.sizeDelta = new Vector2(58f, 58f);
        badRt.anchoredPosition = Vector2.zero;
        var badOutline = badge.gameObject.AddComponent<Outline>();
        badOutline.effectColor    = new Color(0.20f, 0.12f, 0.05f, 0.90f);
        badOutline.effectDistance = new Vector2(2f, -2f);
        var vs = BuatTeks(badge.transform, "VS", "VS", 24, new Color(0.18f, 0.10f, 0.04f, 1f), FontStyles.Bold);
        vs.alignment = TextAlignmentOptions.Center;

        return panel;
    }

    TextMeshProUGUI BuatKartuPlat(Transform parent, string judul, string plat, Color warna, Vector2 sisi)
    {
        var card = BuatImage(parent, "Kartu", warna);
        card.sprite = GetRoundedSpritePlat();
        card.type   = Image.Type.Sliced;
        card.raycastTarget = false;
        var cRt = card.rectTransform;
        cRt.anchorMin = new Vector2(sisi.x < 0 ? 0f : 0.52f, 0f);
        cRt.anchorMax = new Vector2(sisi.x < 0 ? 0.48f : 1f, 1f);
        cRt.offsetMin = Vector2.zero; cRt.offsetMax = Vector2.zero;

        var cardOutline = card.gameObject.AddComponent<Outline>();
        cardOutline.effectColor    = new Color(1f, 1f, 1f, 0.22f);
        cardOutline.effectDistance = new Vector2(2f, -2f);

        var jt = BuatTeks(card.transform, "Judul", judul, 20, new Color(1f, 1f, 1f, 0.92f), FontStyles.Bold);
        jt.alignment = TextAlignmentOptions.Center;
        jt.enableAutoSizing = true; jt.fontSizeMin = 14; jt.fontSizeMax = 20;
        jt.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        var jRt = jt.rectTransform;
        jRt.anchorMin = new Vector2(0f, 0.62f); jRt.anchorMax = new Vector2(1f, 1f);
        jRt.offsetMin = new Vector2(10f, 0f); jRt.offsetMax = new Vector2(-10f, -8f);

        // "Papan plat" gelap membulat tempat nomor ditampilkan.
        var board = BuatImage(card.transform, "Papan", new Color(0f, 0f, 0f, 0.28f));
        board.sprite = GetRoundedSpritePlat();
        board.type   = Image.Type.Sliced;
        board.raycastTarget = false;
        var bdRt = board.rectTransform;
        bdRt.anchorMin = new Vector2(0.06f, 0.10f); bdRt.anchorMax = new Vector2(0.94f, 0.58f);
        bdRt.offsetMin = Vector2.zero; bdRt.offsetMax = Vector2.zero;

        var pt = BuatTeks(board.transform, "Plat", plat, 42, Color.white, FontStyles.Bold);
        pt.alignment = TextAlignmentOptions.Center;
        pt.enableAutoSizing = true; pt.fontSizeMin = 24; pt.fontSizeMax = 44;
        pt.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        Stretch(pt.rectTransform, 8f, 4f);
        return pt;
    }

    /// <summary>Tampilkan tombol pilihan kustom (label + warna) di panel pilihan.</summary>
    void TampilkanTombolKustom((string label, Color warna)[] opsi, Action<int> onPilih)
    {
        if (opsi == null || opsi.Length == 0) return;
        IsiPanelPilihan(opsi.Length, i => opsi[i].label, i => opsi[i].warna, onPilih);
    }

    /// <summary>Builder bersama tombol pilihan Day 3: pil membulat + bingkai + efek
    /// hover, ditata rapi sebagai daftar di dalam kartu keputusan (tanpa tumpang
    /// tindih). Dipakai semua titik keputusan (ronde, konfrontasi, tombol kustom).</summary>
    void IsiPanelPilihan(int jumlah, Func<int, string> labelOf, Func<int, Color> warnaOf, Action<int> onPilih)
    {
        if (_pilihanPanel == null) return;
        _pilihanPanel.SetActive(true);
        foreach (Transform child in _pilihanPanel.transform) Destroy(child.gameObject);
        if (jumlah <= 0) return;

        // Tinggi panel ADAPTIF mengikuti jumlah opsi supaya 4 tombol tidak sesak
        // dan 3 tombol tidak menyisakan ruang kosong berlebih.
        var panelRt = (RectTransform)_pilihanPanel.transform;
        const float tinggiPerTombol = 78f;   // tinggi efektif tiap tombol (px)
        const float paddingPanel    = 56f;   // padding atas+bawah kartu (px)
        panelRt.sizeDelta = new Vector2(panelRt.sizeDelta.x, jumlah * tinggiPerTombol + paddingPanel);

        const float padTB = 0.08f;   // padding atas-bawah di dalam kartu (fraksi)
        const float gap   = 0.03f;   // jarak antar tombol (fraksi)
        float usable = 1f - 2f * padTB;
        float slotH  = usable / jumlah;

        for (int i = 0; i < jumlah; i++)
        {
            float yMax = (1f - padTB) - i * slotH;
            float yMin = yMax - slotH + gap;

            var btnObj = new GameObject("Pilihan_" + i);
            btnObj.transform.SetParent(_pilihanPanel.transform, false);
            var img = btnObj.AddComponent<Image>();
            img.color  = warnaOf(i);
            img.sprite = GetRoundedSpritePlat();
            img.type   = Image.Type.Sliced;
            var bRt = btnObj.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0f, yMin); bRt.anchorMax = new Vector2(1f, yMax);
            bRt.offsetMin = new Vector2(28f, 0f); bRt.offsetMax = new Vector2(-28f, 0f);

            var ol = btnObj.AddComponent<Outline>();
            ol.effectColor    = new Color(0f, 0f, 0f, 0.32f);
            ol.effectDistance = new Vector2(2f, -2f);

            var btn = btnObj.AddComponent<Button>();
            int idx = i;
            btn.onClick.AddListener(() =>
            {
                AudioManager.Instance?.Click();
                _pilihanPanel.SetActive(false);
                onPilih?.Invoke(idx);
            });

            // Efek hover (sedikit membesar) supaya terasa interaktif.
            var trig  = btnObj.AddComponent<EventTrigger>();
            var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener(_ => btnObj.transform.localScale = Vector3.one * 1.035f);
            var exit  = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener(_ => btnObj.transform.localScale = Vector3.one);
            trig.triggers.Add(enter); trig.triggers.Add(exit);

            var label = BuatTeks(btnObj.transform, "Label", labelOf(i), 24, Color.white, FontStyles.Bold);
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TMPro.TextWrappingModes.Normal;
            // Auto-size supaya label panjang (mis. opsi konfrontasi boss) tetap
            // muat dalam tombol tanpa terpotong.
            label.enableAutoSizing = true; label.fontSizeMin = 15; label.fontSizeMax = 24;
            Stretch(label.rectTransform, 24f, 6f);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // BOSS — KONFRONTASI PRIA PARKIR (grooming 4 tahap + pilihan pamungkas)
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator JalankanKonfrontasiBoss()
    {
        // Tampilkan bar "Mental Pelaku" yang menyusut tiap ronde — selaras alur game
        // referensi (klimaks: mental boss dikuras oleh keberanian suara Rara).
        if (_bossBarFill == null && _canvasGO != null) BuildBossBar();
        _bossMental = bossMentalMax;
        UpdateBossBar();

        if (_bossNamaText != null) _bossNamaText.text = bossNama;

        // 1) Grooming INTERAKTIF (selaras referensi): tiap baris jadi satu ronde
        //    dengan pilihan AMAN/RAGU/BAHAYA yang menguras Mental pelaku.
        if (groomingRonde != null && groomingRonde.Length > 0)
        {
            foreach (var ronde in groomingRonde)
            {
                if (ronde == null || ronde.pilihan == null || ronde.pilihan.Length == 0) continue;
                // Ganti latar belakang fullscreen sesuai ronde ini (gaya Day 2).
                GantiLatarArena(ronde.latarBelakang);
                if (_bossNamaText != null) _bossNamaText.text = bossNama;
                yield return TampilkanUcapan(ronde.ucapanBoss, isNarasi: false);

                PilihanKonfrontasi pilih = null;
                _menungguPilihan = true;
                TampilkanPilihanKonfrontasi(ronde.pilihan, p => { pilih = p; _menungguPilihan = false; });
                while (_menungguPilihan) yield return null;

                ProsesPilihanKonfrontasi(pilih);
                DrainMentalByKategori(pilih != null ? pilih.kategori : "RAGU");
                if (pilih != null && pilih.latarReaksi != null) GantiLatarArena(pilih.latarReaksi);
                yield return KetikReaksi(pilih != null ? pilih.reaksi : "");
                yield return new WaitForSeconds(1.6f);
                if (_reaksiText != null) _reaksiText.text = "";

                // Nyawa habis di tengah grooming → Trauma (Game Over).
                if (GameState.Instance != null && !GameState.Instance.IsAlive())
                {
                    _hasilDay3 = HasilDay3.Trauma;
                    GotoFase(Phase.EduCard); // EduCard akan skip ke Complete saat Trauma
                    yield break;
                }
            }
        }
        else
        {
            // Mode lama: baris grooming diketik berurutan tanpa pilihan.
            foreach (var line in groomingLines)
            {
                yield return TampilkanUcapan(line, isNarasi: false);
                yield return new WaitForSeconds(1.1f);
            }
        }

        // 2) Pilihan pamungkas (Visual Novel). Opsi 'Diam' (Lanjut) → pria mendesak lagi (loop).
        while (true)
        {
            // Ganti latar belakang fullscreen untuk konfrontasi pamungkas (gaya Day 2).
            GantiLatarArena(konfrontasiLatarBelakang);
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
            // Opsi [4] PANIC BUTTON juga voice-driven: teriak minta tolong sampai
            // ZONA MERAH (meter dB) lalu lari ke satpam.
            else if (dipilih.panicButton)
            {
                yield return PromptTeriakZona(_ => { });
            }

            if (dipilih.butuhVoiceKeras && !berhasilVoice)
            {
                // Teriakan kurang keras → pria belum mundur. Ulangi pilihan.
                yield return KetikReaksi("Suaramu kurang keras! Tarik napas, terus TERIAK sekuat tenaga: \"JANGAN DEKET-DEKET!\"");
                yield return new WaitForSeconds(1.8f);
                if (_reaksiText != null) _reaksiText.text = "";
                continue;
            }

            // Proses skor & nyawa.
            ProsesPilihanKonfrontasi(dipilih);
            DrainMentalByKategori(dipilih.kategori); // bar Mental pelaku ikut menyusut
            if (dipilih.latarReaksi != null) GantiLatarArena(dipilih.latarReaksi);
            yield return KetikReaksi(dipilih.reaksi);
            yield return new WaitForSeconds(2.0f);
            if (_reaksiText != null) _reaksiText.text = "";

            // Opsi [4] PANIC BUTTON — animasi pergerakan polisi/guru DIHAPUS
            // (diganti sprite gambar fullscreen sebagai latar belakang).

            // Nyawa habis → Trauma.
            if (GameState.Instance != null && !GameState.Instance.IsAlive())
            {
                _hasilDay3 = HasilDay3.Trauma;
                break;
            }

            // Belum final (mis. 'Diam' tapi masih hidup) → pria mendesak lagi (Lanjut fase 6).
            if (dipilih.hasil == HasilDay3.Lanjut) continue;

            // B2 — Gerbang ending LAPOR SUKSES: hanya terbuka bila SEMUA bukti
            // Hari 2 & 3 lengkap (screenshot chat + cek plat). Kalau belum lengkap,
            // keberanian lapor tetap dihargai tapi ending turun ke "Aman".
            if (dipilih.hasil == HasilDay3.LaporSukses && GameState.Instance != null
                && !GameState.Instance.SemuaBuktiLengkap())
            {
                _hasilDay3 = HasilDay3.Aman;
                yield return KetikReaksi("Kamu berani minta tolong \u2014 itu sudah tepat! Tapi tanpa bukti " +
                    "lengkap (screenshot chat & cek plat), laporan jadi sulit ditindak. " +
                    "Lain kali kumpulkan dulu buktinya ya!");
                yield return new WaitForSeconds(2.8f);
                if (_reaksiText != null) _reaksiText.text = "";
                break;
            }

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

    /// <summary>Kuras Mental pelaku sesuai kategori pilihan (AMAN besar, RAGU kecil, BAHAYA nol).
    /// Selaras alur referensi: bar mental menyusut tiap ronde keberanian Rara.</summary>
    void DrainMentalByKategori(string kategori)
    {
        float drain = kategori == "AMAN"  ? bossMentalMax * 0.22f
                    : kategori == "RAGU"  ? bossMentalMax * 0.08f
                    : kategori == "LAPOR" ? bossMentalMax * 0.30f
                    : 0f;
        _bossMental = Mathf.Max(0f, _bossMental - drain);
        UpdateBossBar();
    }

    /// <summary>Tampilkan 4 pilihan konfrontasi (PilihanKonfrontasi) di panel pilihan.</summary>
    void TampilkanPilihanKonfrontasi(PilihanKonfrontasi[] pilihan, Action<PilihanKonfrontasi> onPilih)
    {
        if (pilihan == null || pilihan.Length == 0) return;
        IsiPanelPilihan(pilihan.Length, i => pilihan[i].label, i => pilihan[i].warna,
            idx => onPilih?.Invoke(pilihan[idx]));
    }

    // ══════════════════════════════════════════════════════════════════════
    // PROMPT TERIAK (Voice MAX) — untuk opsi "JANGAN DEKAT!"
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator PromptTeriak(Action<bool> onResult)
    {
        // ── B3 (GDD Boss Fight): Pemain wajib MEMPERTAHANKAN suara KERAS (Zona Merah)
        //    secara KONSISTEN selama ~voiceTimeoutDetik (5 dtk) untuk MENGURAS
        //    "Mental Si Bully", SEMBARI menekan PANIC BUTTON memanggil bantuan.
        //    Tanpa mic: fallback TAHAN SPASI / KLIK. Bar regenerasi kalau berhenti
        //    bersuara, jadi pemain harus konsisten. Tidak ada timeout gagal (selalu
        //    bisa diselesaikan) supaya alur tidak macet.
        var go = new GameObject("Day3_PromptTeriak");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder + 50;
        AddScaler(go);
        go.AddComponent<GraphicRaycaster>();

        // Latar belakang penuh layar (gaya box dialog Day 2) — pakai sprite teriak khusus
        // bila diisi, jika tidak jatuh ke sprite arena, lalu warna gelap suram (hujan).
        Sprite teriakBg = teriakLatarBelakang != null ? teriakLatarBelakang : arenaLatarSprite;
        var bg = BuatImage(go.transform, "BG", teriakBg != null ? Color.white : new Color(0.06f, 0.05f, 0.10f, 1f));
        if (teriakBg != null) bg.sprite = teriakBg;
        bg.raycastTarget = true;
        Stretch(bg.rectTransform);
        // Lapisan gelap tipis supaya teks kontras & fokus ke tengah.
        var dim = BuatImage(go.transform, "Dim", new Color(0f, 0f, 0f, 0.45f));
        Stretch(dim.rectTransform);

        // Sprite Rara berteriak (Voice MAX) di kanan-bawah layar — penegas aksi
        // "bersuara keras". Hanya tampil bila sprite diisi di Inspector.
        if (spriteTeriak != null)
        {
            var teriakImg = BuatImage(go.transform, "SpriteTeriak", Color.white);
            teriakImg.sprite = spriteTeriak;
            teriakImg.preserveAspect = true;
            teriakImg.raycastTarget = false;
            var tRt = teriakImg.rectTransform;
            tRt.anchorMin = new Vector2(1f, 0f); tRt.anchorMax = new Vector2(1f, 0f);
            tRt.pivot = new Vector2(1f, 0f);
            tRt.sizeDelta = spriteTeriakUkuran;
            tRt.anchoredPosition = new Vector2(-20f, 20f);
        }

        // Kartu keputusan membulat (mengelompokkan instruksi + label + Mental Bar)
        // supaya rapi dan tidak menumpuk samar dengan latar/boss.
        var kartu = BuatImage(go.transform, "KartuTeriak", new Color(0.10f, 0.07f, 0.05f, 0.92f));
        kartu.sprite = GetRoundedSpritePlat();
        kartu.type   = Image.Type.Sliced;
        kartu.raycastTarget = true;
        var kRt = kartu.rectTransform;
        kRt.anchorMin = new Vector2(0.13f, 0.40f); kRt.anchorMax = new Vector2(0.87f, 0.88f);
        kRt.offsetMin = Vector2.zero; kRt.offsetMax = Vector2.zero;
        var kartuOutline = kartu.gameObject.AddComponent<Outline>();
        kartuOutline.effectColor    = new Color(0.95f, 0.72f, 0.18f, 0.85f);
        kartuOutline.effectDistance = new Vector2(2.5f, -2.5f);

        var instruksi = BuatTeks(go.transform, "Instruksi",
            "TERIAK TERUS \u2014 JANGAN BERHENTI!\n\"JANGAN DEKAT-DEKAT!!\"\n" +
            "<size=65%>(teriak ke mic, atau TAHAN tombol TERIAK) sambil tekan tombol DARURAT</size>",
            38, Color.white, FontStyles.Bold);
        instruksi.alignment = TextAlignmentOptions.Center;
        var iRt = instruksi.rectTransform;
        iRt.anchorMin = new Vector2(0.16f, 0.64f); iRt.anchorMax = new Vector2(0.84f, 0.85f);
        iRt.offsetMin = Vector2.zero; iRt.offsetMax = Vector2.zero;

        // Label "Mental Si Bully" di atas bar.
        var barLabel = BuatTeks(go.transform, "MentalLabel", bossBarLabel, 26, new Color(1f, 0.85f, 0.40f, 1f), FontStyles.Bold);
        barLabel.alignment = TextAlignmentOptions.Center;
        var blRt = barLabel.rectTransform;
        blRt.anchorMin = new Vector2(0.2f, 0.555f); blRt.anchorMax = new Vector2(0.8f, 0.61f);
        blRt.offsetMin = Vector2.zero; blRt.offsetMax = Vector2.zero;

        // Mental Bar — mulai PENUH lalu TERKURAS saat suara konsisten keras.
        var barBg = BuatImage(go.transform, "MentalBg", bossBarWarnaKosong);
        barBg.sprite = GetRoundedSpritePlat(); barBg.type = Image.Type.Sliced;
        var bgRt = barBg.rectTransform;
        bgRt.anchorMin = new Vector2(0.2f, 0.48f); bgRt.anchorMax = new Vector2(0.8f, 0.545f);
        bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
        var barOutline = barBg.gameObject.AddComponent<Outline>();
        barOutline.effectColor    = new Color(0f, 0f, 0f, 0.35f);
        barOutline.effectDistance = new Vector2(2f, -2f);

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(barBg.transform, false);
        var fill = fillGO.AddComponent<Image>();
        fill.color = bossBarWarnaIsi;
        fill.sprite = GetRoundedSpritePlat();
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.fillAmount = 1f;                 // mental PENUH di awal
        Stretch(fill.rectTransform, 4f, 4f);

        // Panic Button — harus ditekan untuk memanggil bantuan (paralel dgn teriak).
        bool panicDitekan = false;
        var panicGO = new GameObject("PanicButton");
        panicGO.transform.SetParent(go.transform, false);
        var panicImg = panicGO.AddComponent<Image>();
        panicImg.color = new Color(0.85f, 0.16f, 0.16f, 1f);
        panicImg.sprite = GetRoundedSpritePlat(); panicImg.type = Image.Type.Sliced;
        var pRt = panicGO.GetComponent<RectTransform>();
        pRt.anchorMin = new Vector2(0.32f, 0.24f); pRt.anchorMax = new Vector2(0.68f, 0.345f);
        pRt.offsetMin = Vector2.zero; pRt.offsetMax = Vector2.zero;
        var panicOutline = panicGO.AddComponent<Outline>();
        panicOutline.effectColor    = new Color(0f, 0f, 0f, 0.35f);
        panicOutline.effectDistance = new Vector2(2.5f, -2.5f);
        var panicBtn = panicGO.AddComponent<Button>();
        var panicTxt = BuatTeks(panicGO.transform, "Label", panicLabel, 28, Color.white, FontStyles.Bold);
        panicTxt.alignment = TextAlignmentOptions.Center;
        Stretch(panicTxt.rectTransform, 12f, 4f);
        // Efek hover supaya tombol terasa interaktif.
        var panicTrig = panicGO.AddComponent<EventTrigger>();
        var panicEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        panicEnter.callback.AddListener(_ => { if (!panicDitekan) panicGO.transform.localScale = Vector3.one * 1.04f; });
        var panicExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        panicExit.callback.AddListener(_ => panicGO.transform.localScale = Vector3.one);
        panicTrig.triggers.Add(panicEnter);
        panicTrig.triggers.Add(panicExit);
        panicBtn.onClick.AddListener(() =>
        {
            if (panicDitekan) return;
            panicDitekan = true;
            panicGO.transform.localScale = Vector3.one;
            panicImg.color = new Color(0.18f, 0.62f, 0.30f, 1f);
            panicTxt.text = "Bantuan dipanggil!";
            AudioManager.Instance?.Click();
        });

        // Tombol TERIAK (tahan) eksplisit — kontrol sentuh untuk menguras Mental Si
        // Bully. Controller global disembunyikan di Hari 3, jadi arena boss perlu
        // tombolnya sendiri (sejajar dgn fase jalan). Tetap kompatibel dgn mic/SPASI.
        bool teriakBtnHold = false;
        float teriakKlikDrain = 0f;   // dorongan instan tiap klik (tap) tombol
        var shoutImg = BuatImage(go.transform, "TombolTeriakBoss", warnaBahaya);
        shoutImg.sprite = GetRoundedSpritePlat(); shoutImg.type = Image.Type.Sliced;
        var shRt = shoutImg.rectTransform;
        shRt.anchorMin = new Vector2(0.32f, 0.115f); shRt.anchorMax = new Vector2(0.68f, 0.205f);
        shRt.offsetMin = Vector2.zero; shRt.offsetMax = Vector2.zero;
        var shOutline = shoutImg.gameObject.AddComponent<Outline>();
        shOutline.effectColor    = new Color(0f, 0f, 0f, 0.35f);
        shOutline.effectDistance = new Vector2(2.5f, -2.5f);
        var shLbl = BuatTeks(shoutImg.transform, "Label", "TERIAK (tahan)", 26, Color.white, FontStyles.Bold);
        shLbl.alignment = TextAlignmentOptions.Center;
        Stretch(shLbl.rectTransform);
        var shTrig = shoutImg.gameObject.AddComponent<EventTrigger>();
        var shDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        shDown.callback.AddListener(_ => teriakBtnHold = true);
        shTrig.triggers.Add(shDown);
        var shUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        shUp.callback.AddListener(_ => teriakBtnHold = false);
        shTrig.triggers.Add(shUp);
        // KLIK (tap) juga menguras Mental — tidak harus ditahan. Tiap klik memberi
        // dorongan teriakan instan agar tombol terasa responsif di layar sentuh.
        var shBtn = shoutImg.gameObject.AddComponent<Button>();
        shBtn.transition = Selectable.Transition.None;
        shBtn.onClick.AddListener(() => { teriakKlikDrain += 0.2f; AudioManager.Instance?.Click(); });

        float durasiKuras = Mathf.Max(1f, voiceTimeoutDetik);  // detik suara keras konsisten → mental 0
        float mental = 1f;                                     // 1 = penuh, 0 = kalah
        bool sukses = false;
        var vm = VoiceMeter.Instance;

        while (true)
        {
            bool teriakMic    = vm != null && vm.IsLoud();
            bool fallbackHold = teriakBtnHold || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)
                                || (vm != null && vm.fallbackButtonHeld);
            bool keras = teriakMic || fallbackHold;

            if (keras)
                mental -= Time.deltaTime / durasiKuras;             // kuras saat konsisten keras
            else
                mental += Time.deltaTime / (durasiKuras * 1.4f);    // regen kalau berhenti

            if (teriakKlikDrain > 0f)
            {
                mental -= teriakKlikDrain;                          // dorongan instan dari klik tombol
                teriakKlikDrain = 0f;
            }

            mental = Mathf.Clamp01(mental);
            fill.fillAmount = mental;

            // Saat mental habis → arahkan pemain menekan tombol darurat.
            if (mental <= 0f && !panicDitekan)
                instruksi.text = "Dia mulai mundur! Sekarang TEKAN TOMBOL DARURAT!";

            // Sukses HANYA jika Mental Si Bully terkuras DAN bantuan sudah dipanggil.
            if (mental <= 0f && panicDitekan) { sukses = true; break; }
            yield return null;
        }

        if (sukses) AudioManager.Instance?.PlayKategori("AMAN");
        Destroy(go);
        onResult?.Invoke(sukses);
    }

    // ══════════════════════════════════════════════════════════════════════
    // PROMPT TERIAK — ZONA SUARA (meter dB) untuk opsi [4] PANIC BUTTON
    // Voice-driven: tahan suara di ZONA MERAH sampai meter penuh, lalu Rara
    // lari & lapor ke satpam. UI meniru tantangan suara Hari 2 (3 zona dB).
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator PromptTeriakZona(Action<bool> onResult)
    {
        // Ambang zona pada bar (fraksi 0..1): hijau → kuning → merah.
        const float ambangKuning = 0.5f;
        const float ambangMerah  = 0.78f;
        const float tahanDetik   = 1.4f;   // lama suara konsisten di ZONA MERAH → berhasil

        Color hijau = warnaAman, kuning = warnaRagu, merah = warnaBahaya;
        string labelNormal = "SUARA NORMAL  (50-60 DB)";
        string labelSedang = "SUARA SEDANG  (60-80 DB)";
        string labelKeras  = "SUARA KERAS  (>80 DB)";

        var go = new GameObject("Day3_PromptTeriakZona");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder + 50;
        AddScaler(go);
        go.AddComponent<GraphicRaycaster>();

        // Latar belakang penuh layar (pakai sprite teriak bila diisi).
        Sprite teriakBg = teriakLatarBelakang != null ? teriakLatarBelakang : arenaLatarSprite;
        var bg = BuatImage(go.transform, "BG", teriakBg != null ? Color.white : new Color(0.06f, 0.05f, 0.10f, 1f));
        if (teriakBg != null) bg.sprite = teriakBg;
        bg.raycastTarget = true;
        Stretch(bg.rectTransform);

        // Kartu utama membulat (gaya gambar referensi).
        var kartu = BuatImage(go.transform, "Kartu", new Color(0.10f, 0.07f, 0.05f, 0.97f));
        kartu.sprite = GetRoundedSpritePlat(); kartu.type = Image.Type.Sliced;
        kartu.raycastTarget = false;
        var kRt = kartu.rectTransform;
        kRt.anchorMin = new Vector2(0.06f, 0.06f); kRt.anchorMax = new Vector2(0.94f, 0.94f);
        kRt.offsetMin = Vector2.zero; kRt.offsetMax = Vector2.zero;
        var kOutline = kartu.gameObject.AddComponent<Outline>();
        kOutline.effectColor = new Color(0.95f, 0.72f, 0.18f, 0.9f);
        kOutline.effectDistance = new Vector2(3f, -3f);

        // Sprite Rara berteriak (opsional) di kanan-bawah layar.
        if (spriteTeriak != null)
        {
            var teriakImg = BuatImage(go.transform, "SpriteTeriak", Color.white);
            teriakImg.sprite = spriteTeriak;
            teriakImg.preserveAspect = true;
            teriakImg.raycastTarget = false;
            var tRt = teriakImg.rectTransform;
            tRt.anchorMin = new Vector2(1f, 0f); tRt.anchorMax = new Vector2(1f, 0f);
            tRt.pivot = new Vector2(1f, 0f);
            tRt.sizeDelta = spriteTeriakUkuran;
            tRt.anchoredPosition = new Vector2(-20f, 20f);
        }

        // Judul.
        var judul = BuatTeks(go.transform, "Judul", "TERIAK MINTA TOLONG!", 40, merah, FontStyles.Bold);
        judul.alignment = TextAlignmentOptions.Center;
        var jRt = judul.rectTransform;
        jRt.anchorMin = new Vector2(0.08f, 0.80f); jRt.anchorMax = new Vector2(0.92f, 0.92f);
        jRt.offsetMin = Vector2.zero; jRt.offsetMax = Vector2.zero;

        // Instruksi.
        var ins = BuatTeks(go.transform, "Instruksi",
            "TERIAK \u201CTOLOOONG! ADA YANG GANGGU!\u201D ke mikrofon sampai meter MERAH, lalu lari ke SATPAM! (boleh tahan tombol)",
            24, Color.white, FontStyles.Normal);
        ins.alignment = TextAlignmentOptions.Center;
        var iRt = ins.rectTransform;
        iRt.anchorMin = new Vector2(0.12f, 0.71f); iRt.anchorMax = new Vector2(0.88f, 0.80f);
        iRt.offsetMin = Vector2.zero; iRt.offsetMax = Vector2.zero;

        // Label level (berubah Normal / Sedang / KERAS).
        var lvl = BuatTeks(go.transform, "Level", labelNormal, 32, hijau, FontStyles.Bold);
        lvl.alignment = TextAlignmentOptions.Center;
        var lRt = lvl.rectTransform;
        lRt.anchorMin = new Vector2(0.1f, 0.62f); lRt.anchorMax = new Vector2(0.9f, 0.70f);
        lRt.offsetMin = Vector2.zero; lRt.offsetMax = Vector2.zero;

        // Bar background.
        var bar = BuatImage(go.transform, "Bar", new Color(0.08f, 0.08f, 0.10f, 1f));
        bar.sprite = GetRoundedSpritePlat(); bar.type = Image.Type.Sliced;
        var barRt = bar.rectTransform;
        barRt.anchorMin = new Vector2(0.15f, 0.50f); barRt.anchorMax = new Vector2(0.85f, 0.585f);
        barRt.offsetMin = Vector2.zero; barRt.offsetMax = Vector2.zero;
        var barOutline = bar.gameObject.AddComponent<Outline>();
        barOutline.effectColor = new Color(1f, 1f, 1f, 0.3f); barOutline.effectDistance = new Vector2(2f, -2f);

        // Zona warna (hijau / kuning / merah).
        BuatZonaWarnaZona(bar.transform, 0f, ambangKuning, hijau);
        BuatZonaWarnaZona(bar.transform, ambangKuning, ambangMerah, kuning);
        BuatZonaWarnaZona(bar.transform, ambangMerah, 1f, merah);

        // Garis target (ambang merah).
        var garis = BuatImage(bar.transform, "GarisTarget", new Color(1f, 1f, 1f, 0.9f));
        garis.raycastTarget = false;
        var garisRt = garis.rectTransform;
        garisRt.anchorMin = new Vector2(ambangMerah, -0.25f);
        garisRt.anchorMax = new Vector2(ambangMerah, 1.25f);
        garisRt.pivot = new Vector2(0.5f, 0.5f);
        garisRt.sizeDelta = new Vector2(4f, 0f);

        // Marker level (garis tebal vertikal yang bergerak).
        var marker = BuatImage(bar.transform, "Marker", Color.white);
        marker.raycastTarget = false;
        var markRt = marker.rectTransform;
        markRt.anchorMin = new Vector2(0f, -0.18f); markRt.anchorMax = new Vector2(0f, 1.18f);
        markRt.pivot = new Vector2(0.5f, 0.5f);
        markRt.sizeDelta = new Vector2(10f, 0f);

        // Legenda 3 baris (mirip gambar referensi).
        BuatLegendaZona(go.transform, hijau, kuning, merah, labelNormal, labelSedang, labelKeras);

        // Indikator TAHAN: progress berapa lama suara sudah di ZONA MERAH.
        var holdLabel = BuatTeks(go.transform, "HoldLabel", "TAHAN SUARA DI ZONA MERAH!", 18,
            new Color(1f, 0.85f, 0.3f, 1f), FontStyles.Bold);
        holdLabel.alignment = TextAlignmentOptions.Center;
        var hlRt = holdLabel.rectTransform;
        hlRt.anchorMin = new Vector2(0.15f, 0.475f); hlRt.anchorMax = new Vector2(0.85f, 0.498f);
        hlRt.offsetMin = Vector2.zero; hlRt.offsetMax = Vector2.zero;

        var holdBg = BuatImage(go.transform, "HoldBg", new Color(0.08f, 0.08f, 0.10f, 1f));
        holdBg.sprite = GetRoundedSpritePlat(); holdBg.type = Image.Type.Sliced;
        var holdBgRt = holdBg.rectTransform;
        holdBgRt.anchorMin = new Vector2(0.22f, 0.45f); holdBgRt.anchorMax = new Vector2(0.78f, 0.475f);
        holdBgRt.offsetMin = Vector2.zero; holdBgRt.offsetMax = Vector2.zero;
        var holdBgOutline = holdBg.gameObject.AddComponent<Outline>();
        holdBgOutline.effectColor = new Color(1f, 1f, 1f, 0.25f); holdBgOutline.effectDistance = new Vector2(2f, -2f);

        var holdFill = BuatImage(holdBg.transform, "HoldFill", merah);
        holdFill.sprite = GetRoundedSpritePlat(); holdFill.type = Image.Type.Sliced;
        holdFill.raycastTarget = false;
        var holdFillRt = holdFill.rectTransform;
        holdFillRt.anchorMin = new Vector2(0f, 0f); holdFillRt.anchorMax = new Vector2(0f, 1f);
        holdFillRt.offsetMin = Vector2.zero; holdFillRt.offsetMax = Vector2.zero;

        var holdPct = BuatTeks(holdBg.transform, "HoldPct", "0%", 16, Color.white, FontStyles.Bold);
        holdPct.alignment = TextAlignmentOptions.Center;

        // Tombol TAHAN UNTUK TERIAK (fallback / pendukung input mic).
        bool holdTeriak = false;
        var btnGO = BuatTombol(go.transform, "TAHAN UNTUK TERIAK", merah, null);
        var btnRt = (RectTransform)btnGO.transform;
        btnRt.anchorMin = new Vector2(0.27f, 0.115f); btnRt.anchorMax = new Vector2(0.73f, 0.225f);
        btnRt.offsetMin = Vector2.zero; btnRt.offsetMax = Vector2.zero;
        var btnImg = btnGO.GetComponent<Image>();
        btnImg.sprite = GetRoundedSpritePlat(); btnImg.type = Image.Type.Sliced;
        var btnOutline = btnGO.AddComponent<Outline>();
        btnOutline.effectColor = new Color(1f, 0.85f, 0.3f, 0.7f); btnOutline.effectDistance = new Vector2(3f, -3f);
        var btnTrig = btnGO.AddComponent<EventTrigger>();
        var downEvt = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        downEvt.callback.AddListener(_ => holdTeriak = true);
        var upEvt = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        upEvt.callback.AddListener(_ => holdTeriak = false);
        var exitEvt = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEvt.callback.AddListener(_ => holdTeriak = false);
        btnTrig.triggers.Add(downEvt); btnTrig.triggers.Add(upEvt); btnTrig.triggers.Add(exitEvt);

        var vm = VoiceMeter.Instance;
        bool voiceDriven = vm != null;

        float level = 0f, waktuMerah = 0f;
        while (waktuMerah < tahanDetik)
        {
            float dt = Time.deltaTime;
            bool hold = holdTeriak || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)
                        || (vm != null && vm.fallbackButtonHeld);

            if (voiceDriven)
            {
                float target = LevelDariVoiceMeterZona(vm, ambangKuning, ambangMerah);
                if (hold) target = Mathf.Max(target, 1f);
                level = Mathf.Lerp(level, target, 1f - Mathf.Exp(-9f * dt));
            }
            else
            {
                level += (hold ? 0.9f : -0.7f) * dt;
            }
            level = Mathf.Clamp01(level);

            markRt.anchorMin = new Vector2(level, -0.18f);
            markRt.anchorMax = new Vector2(level,  1.18f);

            if (level >= ambangMerah)
            {
                lvl.text = labelKeras; lvl.color = merah;
                waktuMerah += dt;
                marker.color = merah;
                float pulsa = 1f + 0.25f * Mathf.Sin(Time.time * 18f);
                marker.transform.localScale = new Vector3(pulsa, 1f, 1f);
            }
            else if (level >= ambangKuning)
            {
                lvl.text = labelSedang; lvl.color = kuning;
                waktuMerah = 0f;
                marker.color = Color.white;
                marker.transform.localScale = Vector3.one;
            }
            else
            {
                lvl.text = labelNormal; lvl.color = hijau;
                waktuMerah = 0f;
                marker.color = Color.white;
                marker.transform.localScale = Vector3.one;
            }

            float frac = Mathf.Clamp01(waktuMerah / Mathf.Max(0.01f, tahanDetik));
            holdFillRt.anchorMax = new Vector2(frac, 1f);
            holdPct.text = Mathf.RoundToInt(frac * 100f) + "%";
            holdFill.color = frac > 0f
                ? new Color(merah.r, merah.g, merah.b, 0.7f + 0.3f * Mathf.Sin(Time.time * 14f))
                : merah;
            yield return null;
        }

        lvl.text = "HEBAT! LARI KE SATPAM!"; lvl.color = merah;
        marker.color = merah;
        AudioManager.Instance?.PlayKategori("AMAN");
        yield return new WaitForSeconds(0.7f);
        Destroy(go);
        onResult?.Invoke(true);
    }

    // Buat 1 segmen warna pada bar zona (helper PromptTeriakZona).
    void BuatZonaWarnaZona(Transform bar, float dari, float sampai, Color warna)
    {
        var z = BuatImage(bar, "Zona", warna);
        z.raycastTarget = false;
        var rt = z.rectTransform;
        rt.anchorMin = new Vector2(dari, 0.12f);
        rt.anchorMax = new Vector2(sampai, 0.88f);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    // Legenda 3 baris: kotak warna + label (mirip gambar referensi).
    void BuatLegendaZona(Transform parent, Color hijau, Color kuning, Color merah,
                         string lNormal, string lSedang, string lKeras)
    {
        var box = new GameObject("Legenda");
        box.transform.SetParent(parent, false);
        var brt = box.AddComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.30f, 0.27f); brt.anchorMax = new Vector2(0.70f, 0.46f);
        brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;
        var vlg = box.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 6f; vlg.childControlHeight = true; vlg.childControlWidth = true;
        vlg.childForceExpandHeight = true; vlg.childForceExpandWidth = true;

        BuatBarisLegendaZona(box.transform, hijau,  lNormal);
        BuatBarisLegendaZona(box.transform, kuning, lSedang);
        BuatBarisLegendaZona(box.transform, merah,  lKeras);
    }

    void BuatBarisLegendaZona(Transform parent, Color warna, string teks)
    {
        var row = new GameObject("Baris");
        row.transform.SetParent(parent, false);
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 12f; hlg.childControlHeight = true; hlg.childControlWidth = true;
        hlg.childForceExpandHeight = true; hlg.childForceExpandWidth = false;
        hlg.childAlignment = TextAnchor.MiddleLeft;

        var kotak = BuatImage(row.transform, "Kotak", warna);
        kotak.sprite = GetRoundedSpritePlat(); kotak.type = Image.Type.Sliced;
        var kLe = kotak.gameObject.AddComponent<LayoutElement>();
        kLe.preferredWidth = 44f; kLe.preferredHeight = 28f; kLe.flexibleWidth = 0f;

        var lab = BuatTeks(row.transform, "Teks", teks, 20, Color.white, FontStyles.Normal);
        lab.alignment = TextAlignmentOptions.MidlineLeft;
        var labLe = lab.gameObject.AddComponent<LayoutElement>();
        labLe.flexibleWidth = 1f;
    }

    // Peta level VoiceMeter global (threshold-relatif) ke skala zona meter ini.
    float LevelDariVoiceMeterZona(VoiceMeter vm, float ambangKuning, float ambangMerah)
    {
        if (vm == null) return 0f;
        float n = vm.NormalizedLevel;
        if (n >= vm.thresholdLoud)
            return Mathf.Lerp(ambangMerah, 1f, Mathf.InverseLerp(vm.thresholdLoud, 1f, n));
        if (n >= vm.thresholdMedium)
            return Mathf.Lerp(ambangKuning, ambangMerah, Mathf.InverseLerp(vm.thresholdMedium, vm.thresholdLoud, n));
        if (n >= vm.thresholdNormal)
            return Mathf.Lerp(0f, ambangKuning, Mathf.InverseLerp(vm.thresholdNormal, vm.thresholdMedium, n));
        return 0f;
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
        UpdateBossBar();
        if (dipilih != null && dipilih.latarReaksi != null) GantiLatarArena(dipilih.latarReaksi);
        yield return KetikReaksi(dipilih.reaksi);

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
        GantiLatarArena(endingAmanLatar);
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

        var bg = BuatImage(go.transform, "BG", spriteBackground != null ? Color.white : warnaBackground);
        if (spriteBackground != null) bg.sprite = spriteBackground;
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

    /// <summary>Ganti latar belakang fullscreen arena per baris dialog (gaya Day 2).
    /// Kosong (null) = kembali ke latar arena default (arenaLatarSprite).</summary>
    void GantiLatarArena(Sprite bgOverride)
    {
        if (_arenaBgImg == null) return;
        Sprite bgAktif = bgOverride != null ? bgOverride : arenaLatarSprite;
        if (bgAktif != null)
        {
            _arenaBgImg.sprite = bgAktif;
            _arenaBgImg.color  = Color.white;
        }
        else
        {
            _arenaBgImg.sprite = null;
            _arenaBgImg.color  = arenaLatarWarna;
        }
    }

    void BuildArena()
    {
        if (_canvasGO != null) return;

        _canvasGO = new GameObject("Day3_Arena");
        var canvas = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        AddScaler(_canvasGO);
        _canvasGO.AddComponent<GraphicRaycaster>();

        // Latar belakang fullscreen (gaya box dialog Day 2) — anak pertama supaya
        // berada di belakang boss & box, dan menutupi scene di belakang arena.
        var arenaBg = BuatImage(_canvasGO.transform, "ArenaBG",
            arenaLatarSprite != null ? Color.white : arenaLatarWarna);
        if (arenaLatarSprite != null) arenaBg.sprite = arenaLatarSprite;
        arenaBg.raycastTarget = true;   // blok klik tembus ke scene di belakang
        _arenaBgImg = arenaBg;          // simpan agar latar bisa diganti per baris dialog
        var abgRt = arenaBg.rectTransform;
        abgRt.anchorMin = Vector2.zero; abgRt.anchorMax = Vector2.one;
        abgRt.offsetMin = Vector2.zero; abgRt.offsetMax = Vector2.zero;

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

        // Nama boss (kotak merah "Si Bully") DIHAPUS dari tampilan — diganti sprite
        // gambar fullscreen sebagai latar belakang. _bossNamaText sengaja dibiarkan null;
        // semua pemakaiannya sudah null-guard sehingga aman.

        // Boss HP Bar (hanya pada mode boss fight; Visual Novel tanpa bar mental)
        if (!modeVisualNovel) BuildBossBar();

        // Kotak ucapan (narasi / ucapan boss) — gaya box dialog Halte:
        // panel kayu berbingkai + portrait di kiri, banner nama, teks, hint.
        // Sprite & layout dipinjam dari komponen HalteDialog supaya tampil PERSIS
        // seperti box dialog Halte. Fallback: panel gelap + outline emas.
        HalteDialog h = gayaHalteDialog;
        Sprite panelSp = boxDialogSprite != null ? boxDialogSprite : (h != null ? h.panelSprite : null);
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
        _hintLanjutText = BuatTeks(box.transform, "HintLanjut", "", 18, new Color(1f, 1f, 1f, 0.55f), FontStyles.Italic);
        _hintLanjutText.alignment = TextAlignmentOptions.MidlineRight;
        var hintRt = _hintLanjutText.rectTransform;
        hintRt.anchorMin = new Vector2(hCX - hW * 0.5f, hCY - hH * 0.5f);
        hintRt.anchorMax = new Vector2(hCX + hW * 0.5f, hCY + hH * 0.5f);
        hintRt.offsetMin = hintRt.offsetMax = Vector2.zero;
        _hintLanjutText.gameObject.SetActive(false);

        // ── Tombol LANJUT: HANYA tombol ini yang melanjutkan dialog ──
        // (klik di luar tombol tidak lagi melanjutkan)
        _tombolLanjutUcapan = TombolLanjutVN.Pasang(box.transform, null,
            "LANJUT  \u25B6", new Vector2(0.80f, 0.06f), new Vector2(0.99f, 0.40f));

        // Reaksi (di ATAS kotak ucapan) — TANPA sprite box dialog. Hanya teks reaksi
        // yang ditampilkan (latar transparan), sesuai permintaan menghapus box dialog.
        float panelTopFrac = pCY + pH * 0.5f;
        var reaksiBox = BuatImage(_canvasGO.transform, "KotakReaksi", new Color(0f, 0f, 0f, 0f));
        reaksiBox.raycastTarget = false;
        reaksiBox.enabled = false; // tidak menggambar latar; box dialog dihapus
        var reaksiBoxRt = reaksiBox.rectTransform;
        reaksiBoxRt.anchorMin = new Vector2(0.5f, panelTopFrac); reaksiBoxRt.anchorMax = new Vector2(0.5f, panelTopFrac);
        reaksiBoxRt.pivot = new Vector2(0.5f, 0f); reaksiBoxRt.sizeDelta = new Vector2(1540f, 96f);
        reaksiBoxRt.anchoredPosition = new Vector2(0f, 188f);
        reaksiBox.gameObject.SetActive(false); // tampil hanya saat ada teks reaksi
        _reaksiBox = reaksiBox;

        _reaksiText = BuatTeks(reaksiBox.transform, "Reaksi", "", 24, new Color(1f, 1f, 0.85f, 1f), FontStyles.Italic);
        _reaksiText.alignment = TextAlignmentOptions.Center;
        var rrt = _reaksiText.rectTransform;
        rrt.anchorMin = Vector2.zero; rrt.anchorMax = Vector2.one;
        rrt.offsetMin = new Vector2(24f, 8f); rrt.offsetMax = new Vector2(-24f, -8f);

        // Panel pilihan — kartu keputusan di ATAS panel dialog (gaya Halte).
        // Diberi latar membulat gelap + bingkai emas supaya tombol terkelompok
        // rapi sebagai satu "kartu" dan tidak menumpuk samar dengan boss/latar.
        _pilihanPanel = new GameObject("PilihanPanel");
        _pilihanPanel.transform.SetParent(_canvasGO.transform, false);
        var prt = _pilihanPanel.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.5f, panelTopFrac); prt.anchorMax = new Vector2(0.5f, panelTopFrac);
        prt.pivot = new Vector2(0.5f, 0f);
        prt.sizeDelta = new Vector2(1120f, 300f);
        prt.anchoredPosition = new Vector2(0f, 20f);
        var pilihanBg = _pilihanPanel.AddComponent<Image>();
        pilihanBg.sprite = GetRoundedSpritePlat();
        pilihanBg.type   = Image.Type.Sliced;
        pilihanBg.color  = new Color(0.08f, 0.06f, 0.04f, 0.88f);
        pilihanBg.raycastTarget = true; // halangi klik tembus ke kotak dialog di belakang
        var pilihanOutline = _pilihanPanel.AddComponent<Outline>();
        pilihanOutline.effectColor    = new Color(0.95f, 0.72f, 0.18f, 0.85f);
        pilihanOutline.effectDistance = new Vector2(2.5f, -2.5f);
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
        // Dialog ucapan baru muncul → sembunyikan box reaksi sebelumnya.
        if (_reaksiBox != null) _reaksiBox.gameObject.SetActive(false);
        // Banner nama pembicara (gaya box dialog Day 2).
        if (_namaUcapanText != null)
        {
            _namaUcapanText.text  = namaPembicara;
            // Tag nama selalu KUNING untuk seluruh dialog Day 3.
            _namaUcapanText.color = new Color(1f, 0.85f, 0.3f, 1f);
        }
        // Portrait/sprite profil per pembicara (Narasi / Rara / Boss) — gaya box dialog Halte.
        if (_portraitUcapanImg != null)
        {
            Sprite pSpr = PortraitUntuk(namaPembicara);
            if (pSpr != null)
            {
                _portraitUcapanImg.sprite  = pSpr;
                _portraitUcapanImg.color   = Color.white;
                _portraitUcapanImg.enabled = true;
            }
            else
            {
                _portraitUcapanImg.sprite  = null;
                _portraitUcapanImg.enabled = false;
            }
        }
        // Label nama boss (di atas potret) hanya tampil bila yang bicara memang si boss.
        if (_bossNamaText != null) _bossNamaText.text = (namaPembicara == bossNama) ? bossNama : "";
        // Teks narasi Day 3 dibuat NORMAL (tidak italic) sesuai permintaan.
        _ucapanText.fontStyle = italic ? FontStyles.Normal : FontStyles.Bold;
        _ucapanText.text = "";
        if (_hintLanjutText != null) _hintLanjutText.gameObject.SetActive(false);

        // Typewriter — bisa di-skip dengan klik box (SkipAtauLanjutUcapan)
        _ucapanSkip = false;
        foreach (char c in teks)
        {
            if (_ucapanSkip) { _ucapanText.text = teks; break; }
            _ucapanText.text += c;
            if (c != ' ') AudioManager.Instance?.PlayKetikHuruf();
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

    // Tampilkan teks reaksi pilihan dengan efek ketik (typewriter), selaras
    // dengan box ucapan boss/narasi. String kosong → cukup bersihkan teks.
    IEnumerator KetikReaksi(string teks)
    {
        if (_reaksiText == null) yield break;
        _reaksiText.text = "";
        if (string.IsNullOrEmpty(teks))
        {
            if (_reaksiBox != null) _reaksiBox.gameObject.SetActive(false);
            yield break;
        }
        // Tampilkan box latar reaksi (gaya box dialog) saat ada teks reaksi.
        if (_reaksiBox != null) _reaksiBox.gameObject.SetActive(true);
        foreach (char c in teks)
        {
            _reaksiText.text += c;
            if (c != ' ') AudioManager.Instance?.PlayKetikHuruf();
            yield return new WaitForSeconds(0.018f);
        }
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
        // Hanya tombol LANJUT (atau SPACE/ENTER) yang melanjutkan; klik di luar diabaikan.
        _tombolLanjutUcapan?.Reset();
        bool lanjut = false;
        while (!lanjut)
        {
            if ((_tombolLanjutUcapan != null && _tombolLanjutUcapan.Konsumsi()) ||
                Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter))
                lanjut = true;
            yield return null;
        }
    }

    void TampilkanPilihan(PilihanRonde[] pilihan, Action<PilihanRonde> onPilih)
    {
        if (pilihan == null || pilihan.Length == 0) return;
        IsiPanelPilihan(pilihan.Length, i => pilihan[i].label, i => WarnaKategori(pilihan[i].kategori),
            idx => onPilih?.Invoke(pilihan[idx]));
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

        var overlay = BuatImage(go.transform, "Overlay", eduLatarSprite != null ? Color.white : eduLatarWarna);
        if (eduLatarSprite != null) { overlay.sprite = eduLatarSprite; overlay.type = Image.Type.Sliced; }
        overlay.raycastTarget = true;
        Stretch(overlay.rectTransform);

        var panel = BuatImage(go.transform, "Panel", new Color(0.05f, 0.10f, 0.08f, 0.98f));
        panel.sprite = GetRoundedSpritePlat(); panel.type = Image.Type.Sliced;
        var pRt = panel.rectTransform;
        pRt.anchorMin = new Vector2(0.5f, 0.5f); pRt.anchorMax = new Vector2(0.5f, 0.5f);
        pRt.pivot = new Vector2(0.5f, 0.5f);
        pRt.sizeDelta = new Vector2(1200f, 760f);
        var panelOutline = panel.gameObject.AddComponent<Outline>();
        panelOutline.effectColor    = new Color(0.95f, 0.72f, 0.18f, 0.90f);
        panelOutline.effectDistance = new Vector2(3f, -3f);

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

        var btn = BuatTombol(panel.transform, "LANJUT", warnaAman, () => lanjut = true);
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
        // Layar hasil harus menutupi SEMUA elemen lain (HUD, kotak dialog VN,
        // voice meter) supaya tidak ada yang bocor di belakang.
        canvas.sortingOrder = 32000;
        AddScaler(go);
        go.AddComponent<GraphicRaycaster>();

        // Latar layar hasil sesuai jenis ending: Trauma → endingTraumaLatar,
        // selain itu (Aman / Lapor Sukses) → endingAmanLatar. Kosong = hasilLatarSprite.
        Sprite hasilBg = _hasilDay3 == HasilDay3.Trauma
            ? (endingTraumaLatar != null ? endingTraumaLatar : hasilLatarSprite)
            : (endingAmanLatar != null ? endingAmanLatar : hasilLatarSprite);
        var bg = BuatImage(go.transform, "BG", hasilBg != null ? Color.white : new Color(0.04f, 0.06f, 0.12f, 1f));
        if (hasilBg != null) { bg.sprite = hasilBg; bg.type = Image.Type.Sliced; }
        Stretch(bg.rectTransform);

        bool trauma = _hasilDay3 == HasilDay3.Trauma;
        bool laporSukses = _hasilDay3 == HasilDay3.LaporSukses;

        // ── Kartu utama (rounded + bingkai emas) menampung seluruh konten. ──
        var card = BuatImage(go.transform, "KartuHasil", new Color(0.10f, 0.08f, 0.06f, 0.97f));
        card.sprite = GetRoundedSpritePlat();
        card.type   = Image.Type.Sliced;
        card.raycastTarget = false;
        var cardRt = card.rectTransform;
        cardRt.anchorMin = new Vector2(0.16f, 0.085f);
        cardRt.anchorMax = new Vector2(0.84f, 0.965f);
        cardRt.offsetMin = Vector2.zero; cardRt.offsetMax = Vector2.zero;
        var cardOutline = card.gameObject.AddComponent<Outline>();
        cardOutline.effectColor    = new Color(0.95f, 0.72f, 0.18f, 0.95f);
        cardOutline.effectDistance = new Vector2(3f, -3f);
        var cardT = card.transform;

        // Judul — ending terbaik (LAPOR SUKSES) punya gelar khusus.
        string judulTeks = trauma ? endingTraumaJudul
                         : laporSukses ? "ENDING TERBAIK \u2014 PELAPOR HEBAT!"
                         : hasilJudul;
        var judul = BuatTeks(cardT, "Judul", judulTeks,
            46, trauma ? warnaBahaya : warnaTeksJudul, FontStyles.Bold);
        judul.alignment = TextAlignmentOptions.Center;
        judul.enableAutoSizing = true; judul.fontSizeMin = 30; judul.fontSizeMax = 48;
        judul.textWrappingMode = TextWrappingModes.NoWrap;
        var jRt = judul.rectTransform;
        jRt.anchorMin = new Vector2(0.04f, 0.90f); jRt.anchorMax = new Vector2(0.96f, 0.985f);
        jRt.offsetMin = Vector2.zero; jRt.offsetMax = Vector2.zero;

        int skor = gs != null ? gs.score : 0;
        string grade = gs != null ? gs.Grade() : "";

        // Pil skor menonjol di bawah judul.
        var skorPil = BuatImage(cardT, "SkorPil", new Color(0.16f, 0.12f, 0.06f, 0.95f));
        skorPil.sprite = GetRoundedSpritePlat();
        skorPil.type   = Image.Type.Sliced;
        skorPil.raycastTarget = false;
        var spRt = skorPil.rectTransform;
        spRt.anchorMin = new Vector2(0.22f, 0.835f); spRt.anchorMax = new Vector2(0.78f, 0.900f);
        spRt.offsetMin = Vector2.zero; spRt.offsetMax = Vector2.zero;
        var spOutline = skorPil.gameObject.AddComponent<Outline>();
        spOutline.effectColor    = new Color(0.95f, 0.72f, 0.18f, 0.6f);
        spOutline.effectDistance = new Vector2(2f, -2f);

        var skorText = BuatTeks(skorPil.transform, "Skor",
            "TOTAL SKOR: <b>" + skor + "</b>   \u2022   <color=#FFD24A>" + grade + "</color>",
            30, Color.white, FontStyles.Normal);
        skorText.alignment = TextAlignmentOptions.Center;
        skorText.enableAutoSizing = true; skorText.fontSizeMin = 16; skorText.fontSizeMax = 30;
        skorText.textWrappingMode = TextWrappingModes.NoWrap;
        Stretch(skorText.rectTransform, 16f, 4f);

        // ── RATING BINTANG (1-3) — sistem poin ala game umumnya. ──
        // Ending TERBAIK = 3 bintang; Trauma = 1; selain itu dari skor (max 1000).
        int bintangD3 = laporSukses ? 3
                      : trauma ? 1
                      : skor >= 800 ? 3
                      : skor >= 500 ? 2
                      : 1;
        RatingBintang.Bangun(cardT, bintangD3,
            new Vector2(0.5f, 0.805f), new Vector2(0.5f, 0.805f), new Vector2(0.5f, 0.5f),
            Vector2.zero, 50f, 16f, this);

        // ── Tangga TINGKAT ENDING (3 tingkat) — perjelas hasil pemain berada
        //    di tingkat mana & tingkat terbaik yang bisa diraih. ──
        var tierText = BuatTeks(cardT, "TierLadder", TeksTierLadder(),
            22, Color.white, FontStyles.Normal);
        tierText.alignment = TextAlignmentOptions.Center;
        tierText.enableAutoSizing = true; tierText.fontSizeMin = 13; tierText.fontSizeMax = 22;
        tierText.textWrappingMode = TextWrappingModes.NoWrap;
        var tierRt = tierText.rectTransform;
        tierRt.anchorMin = new Vector2(0.04f, 0.748f); tierRt.anchorMax = new Vector2(0.96f, 0.778f);
        tierRt.offsetMin = Vector2.zero; tierRt.offsetMax = Vector2.zero;

        // Ringkasan dibagi menjadi 3 SEKSI berpanel terpisah agar rapih dan
        // mudah dibedakan: (1) Keputusan, (2) Bukti, (3) 3 Kata Sakti.
        int jBukti = gs != null ? gs.JumlahBukti : 0;
        BuatSeksiHasil(cardT, "SeksiKeputusan", "RINGKASAN KEPUTUSAN",
            TeksKeputusan(gs), warnaTeksJudul, 0.560f, 0.745f);
        BuatSeksiHasil(cardT, "SeksiBukti", "BUKTI TERKUMPUL (" + jBukti + "/4)",
            TeksBukti(gs), new Color(0.55f, 0.85f, 1f, 1f), 0.405f, 0.545f);
        BuatSeksiHasil(cardT, "SeksiKataSakti", "3 KATA SAKTI",
            TeksKataSakti(gs), new Color(0.95f, 0.72f, 0.18f, 1f), 0.300f, 0.390f);

        // Pesan penutup
        string pesan = trauma ? endingTraumaNarasi
                     : laporSukses ? "Kamu kumpulkan SEMUA bukti dan BERANI melapor. Inilah pahlawan sejati yang menjaga diri sendiri!"
                     : skor >= ambangLuarBiasa ? pesanLuarBiasa
                     : skor >= ambangBagus     ? pesanBagus
                     : pesanKurang;
        // Untuk hasil selain TERBAIK (bukan Trauma & bukan LaporSukses), beri
        // petunjuk konkret cara membuka ending tertinggi: lengkapi semua bukti.
        if (!trauma && !laporSukses)
        {
            int kurang = gs != null ? (4 - gs.JumlahBukti) : 4;
            pesan += kurang > 0
                ? "\n<size=85%><color=#FFD24A>Ending TERBAIK 'PELAPOR HEBAT': lengkapi " + kurang +
                  " bukti lagi (screenshot chat & cek plat) lalu BERANI lapor!</color></size>"
                : "\n<size=85%><color=#FFD24A>Bukti lengkap! Tinggal pilih BERANI LAPOR untuk ending TERBAIK.</color></size>";
        }
        var pesanText = BuatTeks(cardT, "Pesan", pesan, 24, trauma ? warnaBahaya : warnaTeksJudul, FontStyles.Italic);
        pesanText.alignment = TextAlignmentOptions.Center;
        pesanText.enableAutoSizing = true; pesanText.fontSizeMin = 14; pesanText.fontSizeMax = 24;
        pesanText.textWrappingMode = TextWrappingModes.Normal;
        var pRt = pesanText.rectTransform;
        pRt.anchorMin = new Vector2(0.06f, 0.218f); pRt.anchorMax = new Vector2(0.94f, 0.298f);
        pRt.offsetMin = Vector2.zero; pRt.offsetMax = Vector2.zero;

        // Footer nomor darurat — pengingat edukasi yang selalu tampil.
        var footer = BuatTeks(cardT, "Darurat",
            "\u260E Simpan nomor ini: <b>Polisi 110</b>  |  <b>Hotline Anak 129</b>  |  <b>KPAI 021-31901556</b>",
            18, new Color(0.85f, 0.92f, 1f, 0.9f), FontStyles.Normal);
        footer.alignment = TextAlignmentOptions.Center;
        footer.textWrappingMode = TextWrappingModes.Normal;
        var fRt = footer.rectTransform;
        fRt.anchorMin = new Vector2(0.06f, 0.155f); fRt.anchorMax = new Vector2(0.94f, 0.21f);
        fRt.offsetMin = Vector2.zero; fRt.offsetMax = Vector2.zero;

        // Tombol Main Lagi (rounded + hover) di dalam kartu.
        var btnMain = BuatTombol(cardT, "MAIN LAGI", warnaAman, MainLagi);
        HiasTombolHasil(btnMain);
        var bmRt = (RectTransform)btnMain.transform;
        bmRt.anchorMin = new Vector2(0.5f, 0.035f); bmRt.anchorMax = new Vector2(0.5f, 0.035f);
        bmRt.pivot = new Vector2(1f, 0f); bmRt.sizeDelta = new Vector2(300f, 70f);
        bmRt.anchoredPosition = new Vector2(-16f, 0f);

        // Tombol Keluar (rounded + hover) di dalam kartu.
        var btnKeluar = BuatTombol(cardT, "KELUAR", warnaNetral, Keluar);
        HiasTombolHasil(btnKeluar);
        var bkRt = (RectTransform)btnKeluar.transform;
        bkRt.anchorMin = new Vector2(0.5f, 0.035f); bkRt.anchorMax = new Vector2(0.5f, 0.035f);
        bkRt.pivot = new Vector2(0f, 0f); bkRt.sizeDelta = new Vector2(300f, 70f);
        bkRt.anchoredPosition = new Vector2(16f, 0f);

        _fase = Phase.Complete;
    }

    // ── Bangun satu SEKSI hasil: panel rounded + header + isi, agar tiap
    //    kelompok informasi terlihat jelas terpisah (tidak menyatu). ──
    void BuatSeksiHasil(Transform parent, string nama, string header, string isi,
                        Color warnaHeader, float yMin, float yMax)
    {
        var panel = BuatImage(parent, nama, new Color(0.16f, 0.12f, 0.06f, 0.5f));
        panel.sprite = GetRoundedSpritePlat();
        panel.type   = Image.Type.Sliced;
        panel.raycastTarget = false;
        var pRt = panel.rectTransform;
        pRt.anchorMin = new Vector2(0.06f, yMin); pRt.anchorMax = new Vector2(0.94f, yMax);
        pRt.offsetMin = Vector2.zero; pRt.offsetMax = Vector2.zero;
        var outline = panel.gameObject.AddComponent<Outline>();
        outline.effectColor    = new Color(0.95f, 0.72f, 0.18f, 0.35f);
        outline.effectDistance = new Vector2(2f, -2f);

        // Header seksi — pita atas.
        var hd = BuatTeks(panel.transform, "Header", header, 20, warnaHeader, FontStyles.Bold);
        hd.alignment = TextAlignmentOptions.Center;
        hd.enableAutoSizing = true; hd.fontSizeMin = 14; hd.fontSizeMax = 22;
        hd.textWrappingMode = TextWrappingModes.NoWrap;
        var hRt = hd.rectTransform;
        hRt.anchorMin = new Vector2(0.02f, 0.62f); hRt.anchorMax = new Vector2(0.98f, 0.99f);
        hRt.offsetMin = Vector2.zero; hRt.offsetMax = Vector2.zero;

        // Isi seksi.
        var body = BuatTeks(panel.transform, "Isi", isi, 19, new Color(1f, 1f, 0.92f, 1f), FontStyles.Normal);
        body.alignment = TextAlignmentOptions.Center;
        body.enableAutoSizing = true; body.fontSizeMin = 13; body.fontSizeMax = 20;
        body.textWrappingMode = TextWrappingModes.Normal;
        var bRt = body.rectTransform;
        bRt.anchorMin = new Vector2(0.03f, 0.03f); bRt.anchorMax = new Vector2(0.97f, 0.60f);
        bRt.offsetMin = Vector2.zero; bRt.offsetMax = Vector2.zero;
    }

    // Isi seksi "RINGKASAN KEPUTUSAN": jumlah AMAN/RAGU/BAHAYA + daftar red flag.
    string TeksKeputusan(GameState gs)
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
                case "BAHAYA": bahaya++; redFlags.Add(c.label); break;
            }
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<color=#26AD61>AMAN: " + aman + "</color>    " +
                      "<color=#F29D12>RAGU: " + ragu + "</color>    " +
                      "<color=#E84D3D>BAHAYA: " + bahaya + "</color>");
        if (redFlags.Count > 0)
        {
            sb.Append("<color=#E84D3D>Red flag:</color> ");
            int n = Mathf.Min(redFlags.Count, 3);
            for (int i = 0; i < n; i++)
            {
                sb.Append(redFlags[i]);
                if (i < n - 1) sb.Append("   ");
            }
        }
        else
        {
            sb.Append("<color=#26AD61>Tidak ada keputusan berbahaya. Hebat!</color>");
        }
        return sb.ToString();
    }

    // Isi seksi "BUKTI TERKUMPUL": checklist bukti Hari 2 & Hari 3.
    string TeksBukti(GameState gs)
    {
        if (gs == null) return "-";
        string Cek(bool ada) => ada ? "<color=#26AD61>OK</color>" : "<color=#E84D3D>X</color>";
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(Cek(gs.PunyaBukti(GameState.BUKTI_CHAT_DAY2)) + " Screenshot chat (Hari 2)    " +
                      Cek(gs.PunyaBukti(GameState.BUKTI_PLAT_DAY2)) + " Cek plat angkot (Hari 2)");
        sb.Append(Cek(gs.PunyaBukti(GameState.BUKTI_CHAT_DAY3)) + " Screenshot chat (Hari 3)    " +
                  Cek(gs.PunyaBukti(GameState.BUKTI_PLAT_DAY3)) + " Cek plat ojol (Hari 3)");
        return sb.ToString();
    }

    // Isi seksi "3 KATA SAKTI": checklist TIDAK -> PERGI -> CERITA.
    string TeksKataSakti(GameState gs)
    {
        if (gs == null) return "-";
        string Cek(bool ada) => ada ? "<color=#26AD61>OK</color>" : "<color=#E84D3D>X</color>";
        return Cek(gs.usedTidak) + " TIDAK      " +
               Cek(gs.usedPergi) + " PERGI      " +
               Cek(gs.usedCerita) + " CERITA";
    }

    // Tangga TINGKAT ENDING: 3 tingkat (Game Over \u2192 Pulang Aman \u2192 Pelapor Hebat).
    // Tingkat yang diraih pemain disorot (\u25C9 + tebal + warna), sisanya redup.
    string TeksTierLadder()
    {
        int tier = _hasilDay3 == HasilDay3.Trauma ? 1
                 : _hasilDay3 == HasilDay3.LaporSukses ? 3 : 2;
        string[] nama  = { "GAME OVER", "PULANG AMAN", "PELAPOR HEBAT" };
        string[] aktif = { "#E84D3D", "#F2C44D", "#FFD24A" };
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < 3; i++)
        {
            bool ini = (i + 1) == tier;
            string ikon  = ini ? "\u25C9" : "\u25CB";
            string warna = ini ? aktif[i] : "#5A5A66";
            string isi   = ini ? "<b>" + nama[i] + "</b>" : nama[i];
            sb.Append("<color=" + warna + ">" + ikon + " " + isi + "</color>");
            if (i < 2) sb.Append("  <color=#5A5A66>\u2192</color>  ");
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

    /// <summary>Percantik tombol layar hasil: sudut membulat, bingkai emas, dan efek
    /// hover (membesar saat kursor masuk) supaya terasa interaktif.</summary>
    void HiasTombolHasil(GameObject tombol)
    {
        if (tombol == null) return;
        var img = tombol.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = GetRoundedSpritePlat();
            img.type   = Image.Type.Sliced;
        }
        var outline = tombol.GetComponent<Outline>();
        if (outline == null) outline = tombol.AddComponent<Outline>();
        outline.effectColor    = new Color(0.95f, 0.72f, 0.18f, 0.9f);
        outline.effectDistance = new Vector2(2.5f, -2.5f);

        var trig = tombol.GetComponent<EventTrigger>();
        if (trig == null) trig = tombol.AddComponent<EventTrigger>();
        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ => tombol.transform.localScale = Vector3.one * 1.07f);
        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => tombol.transform.localScale = Vector3.one);
        trig.triggers.Add(enter);
        trig.triggers.Add(exit);
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

    // Sprite kotak membulat (9-slice) untuk kartu/panel plat — selaras tema UI lain.
    private static Sprite _roundedPlat;
    Sprite GetRoundedSpritePlat()
    {
        if (_roundedPlat != null) return _roundedPlat;
        int size = 64; int radius = 14;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp; tex.filterMode = FilterMode.Bilinear;
        Color32 w = new Color32(255, 255, 255, 255), c = new Color32(255, 255, 255, 0);
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
        {
            bool inside = true;
            if      (x < radius && y < radius)                 { int dx = radius - x, dy = radius - y; inside = dx * dx + dy * dy <= radius * radius; }
            else if (x >= size - radius && y < radius)          { int dx = x - (size - 1 - radius), dy = radius - y; inside = dx * dx + dy * dy <= radius * radius; }
            else if (x < radius && y >= size - radius)          { int dx = radius - x, dy = y - (size - 1 - radius); inside = dx * dx + dy * dy <= radius * radius; }
            else if (x >= size - radius && y >= size - radius)  { int dx = x - (size - 1 - radius), dy = y - (size - 1 - radius); inside = dx * dx + dy * dy <= radius * radius; }
            tex.SetPixel(x, y, inside ? (Color)w : (Color)c);
        }
        tex.Apply();
        _roundedPlat = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        return _roundedPlat;
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
