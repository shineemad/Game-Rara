# Day 3 — Daftar Lengkap Dialog & Lokasinya

> Dokumen ini berisi **SEMUA dialog Hari 3** ("Hujan di Parkiran Sekolah — Boss Fight melawan
> intimidasi/grooming") secara lengkap, beserta **lokasi tiap dialog** (file script + posisi
> dalam alur). Disusun dari kode aktual di `Assets/Scripts/`.

## Alur Fase Hari 3

```
Prolog → Intro Pembuka → Jalan di Hujan → Chat Agresif (Ojol Palsu)
   → Ojol Palsu (Cek Plat) → Boss Intro → Grooming 4 Ronde
   → Konfrontasi Pamungkas → Boss Kalah / Lapor → Edu Card → Hasil Akhir
```

Orkestrator: [Assets/Scripts/Day3Controller.cs](Assets/Scripts/Day3Controller.cs) (enum `Phase`:
None, IntroPembuka, JalanHujan, ChatAgresif, OjolPalsu, BossIntro, BossKonfrontasi, Round1,
Round2, Round3, BossDefeated, EduCard, Complete).

---

## 0. Prolog Hari 3

- **Lokasi**: [Assets/Scripts/Day3PrologScreen.cs](Assets/Scripts/Day3PrologScreen.cs) — array `slides` (3 slide).
- **Kapan**: Tampil sebelum gameplay Day 3, dipicu `DayTransitionManager.LanjutKeDay3()`.

**Slide 1 — "Hari 3: Hujan di Parkiran Sekolah"**

> Hujan deras mengguyur kota.
> Rara berjalan menuju parkiran SMP Harapan.
>
> Tiba-tiba, seseorang menghadang jalannya.
> "Hei, mau kubawa pulang pakai ojol?"

**Slide 2 — "Ancaman Grooming"**

> Sebelumnya, ada orang asing yang terus
> mengirim pesan ke HP Rara — memintanya
> merahasiakan obrolan mereka dan meminta foto.
>
> Ini adalah GROOMING!

**Slide 3 — "Hadapi Si Bayangan Gelap"**

> "Si Bayangan Gelap" adalah orang berbahaya
> yang menyamar sebagai orang baik.
>
> Satu-satunya cara mengalahkannya:
> BERSUARA KERAS dan tekan PANIC BUTTON!
> Minta bantuan orang dewasa terdekat!

---

## 1. Overlay Judul Hari

- **Lokasi**: [Assets/Scripts/Day3Controller.cs](Assets/Scripts/Day3Controller.cs) — field `barisPertama`, `barisKedua`, `teksLokasi`.
- **Kapan**: Tampil di awal fase `BossIntro`.

| Elemen  | Teks                       |
| ------- | -------------------------- |
| Baris 1 | HARI 3                     |
| Baris 2 | Hujan di Parkiran Sekolah  |
| Lokasi  | Parkiran SMP — Musim Hujan |

---

## 2. Intro Pembuka (gaya Day 2)

- **Lokasi**: [Assets/Scripts/Day3Controller.cs](Assets/Scripts/Day3Controller.cs) — array `introBaris`.
- **Kapan**: Fase `IntroPembuka`, jembatan ringkas ke segmen jalan (tidak mengulang prolog).

| Pembicara | Teks                                                                                         |
| --------- | -------------------------------------------------------------------------------------------- |
| Narasi    | "Hujan makin deras. Rara harus cepat menuju parkiran. 🌧"                                    |
| Narasi    | "Tap layar / TERIAK buat jalan ke parkiran! Semakin keras teriak = makin cepet jalannya! 🏃" |

---

## 3. Jalan di Hujan (menuju parkiran)

- **Lokasi**: [Assets/Scripts/Day3Controller.cs](Assets/Scripts/Day3Controller.cs) — field `jalanInstruksi`, `jalanNarasiSampai`.
- **Kapan**: Fase `JalanHujan`. Segmen jalan kaki menembus hujan (TAP / TERIAK untuk maju).

**Instruksi:**

> ➔ Jalan ke parkiran sekolah — TERIAK buat lari lebih cepat! 🏃

**Narasi saat sampai di parkiran:**

| #   | Teks                                                                                                                          |
| --- | ----------------------------------------------------------------------------------------------------------------------------- |
| 1   | "Rara akhirnya sampai di parkiran. Basah kuyup kena hujan! 😅 Dia langsung buka HP buat ngecek ojol-nya udah nyampe belum..." |
| 2   | "Eh?! Ada notif dari nomor yang nggak aku kenal?! 😨 Siapa nih... _deg-degan banget_"                                         |

---

## 4. Tantangan 1 — Chat Agresif (Ojol Palsu / "Paman Baik")

- **Lokasi**: [Assets/Scripts/Day3Controller.cs](Assets/Scripts/Day3Controller.cs) — `chatPesan` + method `BuatChatAgresif()`; ditampilkan oleh [Assets/Scripts/ChatSimWhatsApp.cs](Assets/Scripts/ChatSimWhatsApp.cs).
- **Kapan**: Fase `ChatAgresif`. Kontak: **Paman Baik** (online, jam 13:45). Timer pilih 6 detik.

**Pesan masuk (diketik otomatis):**

| #   | Teks                                                                 |
| --- | -------------------------------------------------------------------- |
| 1   | "Hai cantik! Hujan deras ya 😢 Hati-hati basah..."                   |
| 2   | "Mau jemput? Gratis kok, kasihan kamu basah sendirian!"              |
| 3   | "🥺📱 Eh, foto kamu pakai seragam dong... buat om simpan ya sayang~" |

**Pilihan respons Rara:**

| Label                                      | Kategori | Reaksi                                                                                                                                   |
| ------------------------------------------ | -------- | ---------------------------------------------------------------------------------------------------------------------------------------- |
| 📸 Oke, ini foto seragamku~                | BAHAYA   | "✖ STOP! Jangan kirim foto ke orang yang nggak kamu kenal! Foto bisa dipakai buat memeras atau mengancam kamu. Kamu kehilangan 1 nyawa." |
| 🚗 Iya Om, aku di parkiran SMP. Jemput ya! | BAHAYA   | "✖ GAME OVER! Rara pergi sama orang nggak dikenal dari internet! Jangan PERNAH kasih lokasi atau minta dijemput orang asing."            |
| 🚫 BLOKIR sekarang + lapor ke ortu!        | AMAN     | "✓ TEPAT! Blokir nomornya, terus cerita ke orang tua. Itulah cara pahlawan menjaga diri! (+200)"                                         |

**Tombol bonus:**

> 📸 Screenshot dulu buat bukti (+100, achievement "Detektif Bukti")

> ⏱ Jika waktu habis → dianggap kirim foto (BAHAYA, −1 nyawa).

---

## 5. Tantangan 2 — Ojol Palsu (Cek Plat Nomor)

- **Lokasi**: [Assets/Scripts/Day3Controller.cs](Assets/Scripts/Day3Controller.cs) — field `ojolNarasi`, `ojolUcapan`, `ojolPilihan`, plat nomor.
- **Kapan**: Fase `OjolPalsu`. Pemain cek plat: pesanan **DD 3472 WK** vs datang **DB 8831 QP** (BERBEDA = palsu).

**Narasi:**

> Yes! Rara nggak terpancing pesan mencurigakan itu! 💪
> Nah, ojol pesanan Rara baru aja tiba di parkiran!
> Tapi jangan langsung naik — cek plat nomornya dulu ya!

**Ucapan (Ojek Online (?)):**

> "Ayo naik, gratis! Cepetan, keburu makin deras nih!"

**Pilihan respons Rara:**

| Label                              | Kategori | Bonus | Reaksi                                                                                                  |
| ---------------------------------- | -------- | ----- | ------------------------------------------------------------------------------------------------------- |
| 📸 Foto plat dulu, lalu tolak naik | AMAN     | +100  | "✓ Cerdas! Kamu foto plat sebagai bukti, lalu menolak dengan sopan. Jangan naik kendaraan orang asing." |
| "Makasih, saya jalan kaki saja."   | AMAN     | —     | "✓ Bagus, kamu menolak dengan tegas dan tetap waspada."                                                 |
| Naik saja, mumpung gratis          | BAHAYA   | —     | "✖ Bahaya! Jangan pernah naik kendaraan orang asing meski gratis. Kamu kehilangan 1 nyawa."             |

---

## 6. Boss Intro — Narasi Pembuka

- **Lokasi**: [Assets/Scripts/Day3Controller.cs](Assets/Scripts/Day3Controller.cs) — field `narasiPembuka`.
- **Kapan**: Sebelum boss bicara. Boss: **Si Bayangan Gelap**.

> TUNGGU! Rara mau naik ojol...
> tapi seseorang tiba-tiba menghadang jalannya! 😱
> Itu dia — si pengirim pesan tadi — muncul langsung di depan Rara!!

---

## 7. Boss Fight — Grooming 4 Ronde (Konfrontasi Interaktif)

- **Lokasi**: [Assets/Scripts/Day3Controller.cs](Assets/Scripts/Day3Controller.cs) — array `groomingRonde`.
- **Kapan**: Inti boss fight. Tiap baris grooming = 1 ronde: boss bicara → Rara memilih AMAN/RAGU/BAHAYA (menguras Mental pelaku).

### Ronde 1

**Boss:** "Eh hei, mau kemana sendirian? 😏 Ikut aku dulu deh. Sebentar aja kok~"

| Label                                      | Kategori | Reaksi                                                             |
| ------------------------------------------ | -------- | ------------------------------------------------------------------ |
| "PERGI! Aku NGGAK KENAL kamu! TOLONG!! 🔊" | AMAN     | "✓ BERANI! Suara Rara bikin pelaku kaget dan mundur selangkah."    |
| "E-emm... nggak usah deh..." (suara pelan) | RAGU     | "⚠ Kurang tegas. Lain kali bersuara lebih lantang ya!"             |
| (diam, bingung mau gimana...)              | BAHAYA   | "✖ DIAM ITU BAHAYA! Pelaku makin berani. Kamu kehilangan 1 nyawa." |

### Ronde 2

**Boss:** "Sssst! Jangan teriak-teriak, nanti kamu yang dimarahin orang. Diam aja ya~"

| Label                                             | Kategori | Reaksi                                                              |
| ------------------------------------------------- | -------- | ------------------------------------------------------------------- |
| "JANGAN DEKET-DEKET! TOLONG!! 🔊" (Teriak KERAS!) | AMAN     | "✓ HEBAT! Teriakan Rara menggema. Pelaku makin ciut nyalinya."      |
| "T-tolong..." (hampir nggak kedengeran)           | RAGU     | "⚠ Suaramu kepelanan. TERIAK sekuat tenaga lain kali!"              |
| (beku di tempat, nggak bisa ngomong...)           | BAHAYA   | "✖ Rara beku ketakutan. Pelaku makin mendesak. Kehilangan 1 nyawa." |

### Ronde 3

**Boss:** "Haha, emangnya siapa yang bakal percaya sama kamu? Nggak ada! Diam aja~"

| Label                                               | Kategori | Reaksi                                                            |
| --------------------------------------------------- | -------- | ----------------------------------------------------------------- |
| "PERGI! Aku PERCAYA SAMA DIRI SENDIRI! TOLONG!! 💪" | AMAN     | "✓ KEREN! Rara percaya diri. Mental pelaku makin jatuh."          |
| "Emangnya... kenapa sih?" (masih ragu-ragu)         | RAGU     | "⚠ Jangan terpancing. Tetap tegas menolak ya!"                    |
| (nangis diem-diem, nggak berani berbuat apa-apa)    | BAHAYA   | "✖ Rara terlalu takut. Pelaku menang sesaat. Kehilangan 1 nyawa." |

### Ronde 4

**Boss:** "Ini rahasia kita berdua ya. Kalau kamu ngadu, kamu sendiri yang bakal kena masalah!"

| Label                                             | Kategori | Reaksi                                                       |
| ------------------------------------------------- | -------- | ------------------------------------------------------------ |
| "Bohong! AKU BAKAL CERITA ke guru sekarang! 🔊"   | AMAN     | "✓ TEPAT! Rahasia jahat HARUS diceritakan. Pelaku panik!"    |
| "Aku nggak tau harus ngapain..." (bingung banget) | RAGU     | "⚠ Ingat: kamu boleh cerita ke orang dewasa yang dipercaya!" |
| "Mungkin... emang salah aku ya..." (mulai pasrah) | BAHAYA   | "✖ Ini BUKAN salahmu! Jangan pasrah. Kehilangan 1 nyawa."    |

---

## 8. Konfrontasi Pamungkas (Panic Button)

- **Lokasi**: [Assets/Scripts/Day3Controller.cs](Assets/Scripts/Day3Controller.cs) — field `konfrontasiUcapan`, `konfrontasiPilihan`.
- **Kapan**: Boss mendesak Rara memutuskan. Timeout jendela teriak 5 detik.

**Boss:** "Pasrah aja lah! Nggak ada yang bisa nolongin kamu di sini!"

| Label                                             | Kategori | Hasil       | Reaksi                                                                                                                               |
| ------------------------------------------------- | -------- | ----------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| (beku di tempat, nggak bisa ngomong...)           | BAHAYA   | Lanjut      | "✖ Rara beku ketakutan, dan dia jadi makin berani. Kehilangan 1 nyawa — ingat, DIAM ITU BAHAYA. Kamu harus bersuara!"                |
| "T-tolong..." (suaranya hampir nggak kedengeran)  | RAGU     | Aman        | "⚠ Rara berhasil pergi, tapi suaranya pelan banget. Lain kali TERIAK yang keras ya!"                                                 |
| "JANGAN DEKET-DEKET! TOLONG!! 🔊" (Teriak KERAS!) | AMAN     | Aman        | "✓ HEBAT! Teriakan Rara bikin dia kaget dan langsung mundur. Berani bersuara itu kekuatan!" (+300, butuh Voice keras)                |
| 📢 TERIAK SEKERAS-KERASNYA + lari ke satpam! 🆘   | LAPOR    | LaporSukses | "✓ LAPOR SUKSES! Rara teriak minta tolong dan lari ke satpam. Guru dan satpam langsung datang — Rara pahlawan buat dirinya sendiri!" |

**Panic Button:**

> 🚨 PANIC BUTTON
>
> Narasi saat ditekan:
> 🚔 "HEEEI! Ada apa ini?! Kami denger ada yang teriak!"
> Si Bayangan Gelap langsung kabur terbirit-birit! Pengecut!

---

## 9. Boss Kalah / Lapor Sukses

- **Lokasi**: [Assets/Scripts/Day3Controller.cs](Assets/Scripts/Day3Controller.cs) — field `narasiBossKalah`.

> "Tenang Rara, kamu udah berani banget! Kamu nggak salah sama sekali."
> Guru dan satpam bakal bantu laporin ke polisi. Rara berani cerita — itu pilihan PALING TEPAT! 💪

- **Achievement menang (ending AMAN):** "Berani Menjaga Diri"
- **Achievement lapor:** "Pahlawan Diri Sendiri"

---

## 10. Boss Fight Alternatif — Ronde Klasik (rondeList)

- **Lokasi**: [Assets/Scripts/Day3Controller.cs](Assets/Scripts/Day3Controller.cs) — array `rondeList` (dipakai pada mode boss-fight HP, bukan Visual Novel).

### Ronde 1 — Bujukan

**Si Bully:** "Hai dek, sendirian ya? Ayo ikut om sebentar, om beliin jajan kesukaanmu deh~"

| Label                         | Kategori | Damage | Reaksi                                                         |
| ----------------------------- | -------- | ------ | -------------------------------------------------------------- |
| "TIDAK! Aku nggak kenal om."  | AMAN     | 40     | "✓ Tegas! Kata sakti TIDAK. Pelaku kaget kamu berani menolak." |
| "Eh... nggak usah deh, om..." | RAGU     | 20     | "⚠ Kurang tegas. Dia masih coba membujukmu."                   |
| Diam & ragu-ragu mau ikut     | BAHAYA   | 0      | "✖ Dia makin memaksa. Kamu kehilangan 1 nyawa."                |

### Ronde 2 — Rahasia & Ancaman

**Si Bully:** "Sssst, ini rahasia kita berdua ya. Kalau kamu ngadu, kamu sendiri yang bakal kena masalah!"

| Label                                                      | Kategori | Damage | Reaksi                                                 |
| ---------------------------------------------------------- | -------- | ------ | ------------------------------------------------------ |
| "Aku PERGI dari sini. Nggak ada rahasia sama orang asing." | AMAN     | 40     | "✓ Mantap! Kata sakti PERGI. Nyali pelaku makin ciut." |
| "I-iya deh, aku nggak bakal cerita..."                     | RAGU     | 20     | "⚠ Dia merasa kamu bisa ditakut-takuti."               |
| Menurut & janji simpan rahasia                             | BAHAYA   | 0      | "✖ Justru itu jebakannya. Kamu kehilangan 1 nyawa."    |

### Ronde 3 — Cari Bantuan

**Si Bully:** "Mau apa kamu? Di sini cuma ada kita berdua, nggak ada yang nolongin!"

| Label                                    | Kategori | Damage | Reaksi                                                     |
| ---------------------------------------- | -------- | ------ | ---------------------------------------------------------- |
| Teriak "TOLONG!" & lari CERITA ke satpam | AMAN     | 50     | "✓ Hebat! Kata sakti CERITA. Satpam datang, pelaku kabur!" |
| "Aku... aku tunggu guru aja deh."        | RAGU     | 20     | "⚠ Lumayan, tapi kamu masih ragu cari bantuan."            |
| Ikut saja ke tempat sepi                 | BAHAYA   | 0      | "✖ BAHAYA besar! Kamu kehilangan 1 nyawa."                 |

---

## 11. Kartu Edukasi Hari 3 (FINAL)

- **Lokasi**: [Assets/Scripts/Day3Controller.cs](Assets/Scripts/Day3Controller.cs) — field `eduJudul`, `eduIsi`.

**Judul:** 🏆 Kartu Edukasi — Hari 3: FINAL

> ⚠ Apa itu Grooming?
> Grooming = orang dewasa yang pura-pura 'baik' buat mendekati anak — lewat chat, sosmed, atau ketemu langsung. Ini KEJAHATAN. Kamu boleh lapor!
>
> 🦁 Cara Melindungi Diri:
> • Ingat 3 KATA SAKTI: TIDAK! — PERGI! — CERITA!
> • Terasa nggak aman? TERIAK keras dan minta tolong!
> • Chat mencurigakan? Blokir + screenshot + cerita ke ortu.
> • Guru dan polisi ADA untuk melindungi kamu!
>
> 📣 Yang Paling Penting:
> Kalau kamu jadi korban, itu BUKAN salahmu! Berani cerita ke orang yang dipercaya = tindakan paling berani yang bisa kamu lakuin! 💪
>
> 🆘 Darurat: Polisi 110 | Hotline Anak 129 | KPAI 021-31901556

---

## 12. Layar Hasil Akhir (Complete)

- **Lokasi**: [Assets/Scripts/Day3Controller.cs](Assets/Scripts/Day3Controller.cs) — field `hasilJudul`, pesan penutup, ending narasi.

**Judul:** 🏁 TANTANGAN SELESAI!

**Pesan penutup (berdasarkan skor, ikut CLAUDE.md):**

| Rentang Skor | Pesan                                                        |
| ------------ | ------------------------------------------------------------ |
| ≥ 800        | "Luar Biasa! Kamu sangat waspada dan berani menjaga diri."   |
| 500–799      | "Bagus! Kamu cukup berhati-hati menjaga diri."               |
| < 500        | "Kamu masih perlu belajar cara menjaga diri. Ayo coba lagi!" |

**Narasi ending AMAN:**

> Hujan mulai reda. Rara masuk ke gerbang sekolah dengan selamat. Dadanya masih berdebar, tapi ia bangga — hari ini ia berhasil menjaga dirinya sendiri!

**Narasi ending TRAUMA / GAME OVER (💔 GAME OVER):**

> Rasa takut bikin Rara nggak berani bertindak, dan keadaannya jadi berbahaya. Tapi tenang — jangan menyerah! Ayo coba lagi dan belajar cara menjaga diri.

---

## Ringkasan Kategori Pilihan Hari 3

| Kategori | Skor (GameState)        | Arti                                  |
| -------- | ----------------------- | ------------------------------------- |
| AMAN     | +100 (`SCORE_AMAN`)     | Respons tegas & melindungi diri       |
| RAGU     | +50 (`SCORE_RAGU`)      | Kurang tegas, masih bisa diperbaiki   |
| BAHAYA   | 0 (`SCORE_BAHAYA`) −1 ♥ | Respons berisiko, kehilangan nyawa    |
| LAPOR    | +500 (`SCORE_LAPOR`)    | Panic button / lapor → ending terbaik |
