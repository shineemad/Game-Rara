# Analisis Dialog — HARI 2: Naik Angkot ke Sekolah

> Lokasi: **Angkot Jurusan Sekolah**
> Tema edukasi: mengenali **grooming** (bahaya daring/data pribadi) + **pelecehan fisik** (sentuhan di angkot).
> Dua pelaku berbeda: **Pria Halte** (lewat data/WA) & **Pria Angkot** (kontak fisik langsung).
> Sumber: file komponen di `Assets/Scripts/` (teks default Inspector).

Alur fase: **Intro → Narasi → Halte → Angkot → Sentuh → Quiz → ChatSim → Lapor → EduCard → Summary**

Legenda kategori pilihan: 🟢 AMAN · 🟡 RAGU · 🔴 BAHAYA

---

## 1. Intro — Overlay Judul (`Day2Controller`)

| Elemen  | Teks                   |
| ------- | ---------------------- |
| Baris 1 | **HARI 2**             |
| Baris 2 | Naik Angkot ke Sekolah |
| Lokasi  | Angkot Jurusan Sekolah |
| Hint    | Bersiaplah...          |

---

## 2. Narasi Pembuka (`Day2NarasiAwal`)

| #   | Pembicara | Teks                                                           |
| --- | --------- | -------------------------------------------------------------- |
| 1   | Rara      | "Bismillah, aku pasti bisa! Haltenya udah dekat — ayo cepat!"  |
| 2   | Rara      | "Tapi… kok jalan ini sepi banget ya? Aku harus tetap waspada." |

---

## 3. Halte — Pria Asing Mendekat (`HalteDialog`)

### Narasi pembuka (Fase 1)

1. Pagi itu Rara sampai di halte yang cukup ramai. Beberapa orang ikut menunggu angkot jurusan sekolah.
2. Rara berdiri di pinggir sambil sesekali melihat jam. Angkotnya belum datang juga.
3. Dari tadi, ada seorang pria asing bertopi yang terus memperhatikan Rara dari kejauhan...
4. Pelan-pelan, pria itu mendekat dan berdiri tepat di sebelah Rara.

### Dialog Pria Asing (Fase 2) — disertai badge ⚠ TANDA BAHAYA

| #   | Teks                                                                                                   | Red Flag                                          |
| --- | ------------------------------------------------------------------------------------------------------ | ------------------------------------------------- |
| 1   | "Hai, cantik! Sendirian aja nih? Om dari tadi merhatiin kamu lho."                                     | Orang asing tiba-tiba sok akrab & memperhatikanmu |
| 2   | "Mau ke sekolah ya? Om kebetulan searah. Daripada nunggu angkot lama, bareng om aja yuk — gratis kok." | Memberi iming-iming / tumpangan gratis            |
| 3   | "Eh, WA kamu berapa? Nanti om anter pulang sekolah ya. Rahasia aja, nggak usah bilang siapa-siapa."    | Minta data pribadi & mengajak menyimpan rahasia   |

### Pilihan pemain

| Kategori  | Pilihan                                                                                | Reaksi                                                                                                                                                                                  |
| --------- | -------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 🟢 AMAN   | "Maaf Om, saya nggak kenal Om. TOLONG jangan ganggu saya!" (tolak tegas + suara keras) | ✓ BAGUS, RA! Kamu menolak tegas & bersuara keras minta tolong. Orang-orang menoleh, ibu-ibu menghampirimu, pria itu pergi. Nomor HP/WA itu DATA PRIBADI — jangan diberi ke orang asing! |
| 🟡 RAGU   | "Hmm... saya pikir-pikir dulu ya Om..."                                                | ⚠ Kamu ragu menjawab. Pria itu makin maju & memaksa minta nomormu. Untung angkot keburu datang. Lain kali, langsung TEGAS tolak!                                                        |
| 🔴 BAHAYA | "Boleh deh Om, ini WA saya..."                                                         | ✖ GAWAT! Kamu beri nomor WA ke orang asing. Malamnya HP dibanjiri chat aneh. Kehilangan 1 nyawa. Hadiah dari orang asing = RED FLAG grooming!                                           |

---

## 4. Angkot — Pilih Tempat Duduk (`AngkotSeatPicker`)

> Judul: _"Angkot datang. Di dalam sudah ada beberapa penumpang — termasuk seorang pria yang melirik ke arahmu. Pilih tempat dudukmu:"_

| Kategori  | Kursi                       | Deskripsi                                    | Reaksi (ringkas)                                                  |
| --------- | --------------------------- | -------------------------------------------- | ----------------------------------------------------------------- |
| 🟢 AMAN   | Dekat Pintu (depan)         | Dekat Pak Supir, gampang turun cepat.        | ✓ Pintar! Posisi paling aman: gampang minta tolong & cepat turun. |
| 🟡 RAGU   | Tengah (di samping ibu-ibu) | Ramai tapi terjepit, agak susah turun.       | ⚠ Ramai memang, tapi kamu susah bergerak kalau terjadi sesuatu.   |
| 🔴 BAHAYA | Pojok Belakang (sepi)       | Sepi & gelap, ada pria asing yang ngeliatin. | ✖ Pojok sepi & gelap, jauh dari Pak Supir. Posisi paling rawan.   |

**Bonus:** 📝 Catat plat nomor angkot (B 1234 XYZ) → +50 poin.

---

## 5. Sentuh Bahu — Insiden Fisik (`AngkotSentuhScene`)

### Intro bercabang sesuai pilihan kursi

**Varian AMAN (dekat supir):**

1. Rara duduk tepat di belakang Pak Supir, dekat pintu.
2. Pria asing dari bangku belakang pindah & duduk merapat di sebelah Rara.
3. Pria Asing: "Sekolah di mana, dek? Sini deket om aja, biar nggak desak-desakan."
4. Tangan pria itu menyentuh bahu Rara! Tapi Pak Supir ada di depan — Rara bisa minta tolong.

**Varian RAGU (tengah):** Rara terhimpit di antara ibu-ibu → Pria Asing: "Geser dikit dong, dek. Biar om bisa duduk dekat kamu." → bahu disentuh, susah bergerak.

**Varian BAHAYA (pojok):** Rara sendirian di pojok sepi → Pria Asing: "Tenang, dek... om temani kamu sampai sekolah ya. Deket-deket om aja." → bahu disentuh, jauh dari supir.

### Pilihan: _"Bahumu disentuh! Apa yang Rara lakukan?"_

| Kategori  | Pilihan                              | Reaksi                                                                                                 |
| --------- | ------------------------------------ | ------------------------------------------------------------------------------------------------------ |
| 🟢 AMAN   | "JANGAN PEGANG SAYA!" (teriak keras) | ✓ HEBAT! Kata sakti pertama: TIDAK! Semua penumpang menoleh. Lanjut: PERGI & CERITA (lapor Pak Supir). |
| 🟡 RAGU   | Geser menjauh diam-diam              | ⚠ Pria itu masih mendekat. Menjauh saja belum cukup. Ingat 3 kata sakti: TIDAK — PERGI — CERITA!       |
| 🔴 BAHAYA | Diam saja karena takut               | ✖ Kamu membeku, pria makin berani. Kehilangan 1 nyawa. Lakukan: TIDAK — PERGI — CERITA!                |

### Beat setelah AMAN (PERGI)

1. Rara langsung berdiri & PINDAH ke kursi lebih depan, menjauh dari pria itu. (Kata sakti kedua: PERGI.)
2. Rara: "Aku sudah bersuara dan menjauh. Tapi dia masih satu angkot — aku harus tetap siaga sampai turun."

### Mini-game Voice Meter (Teriak)

- Judul: 🗣 TERIAK SEKUATNYA!
- Instruksi: TAHAN tombol & teriak "JANGAN PEGANG SAYA!" sampai meter MERAH!

---

## 6. Quiz Zona Tubuh (`ZonaTubuhQuiz`)

> Judul: 🛡 _Quiz: Mana yang BOLEH, mana yang TIDAK BOLEH?_
> Template VN: _"Menurutmu, '{PERILAKU}' itu BOLEH atau TIDAK BOLEH dilakukan orang lain ke tubuhmu?"_

| Chip / Perilaku                         | Jawaban Benar           |
| --------------------------------------- | ----------------------- |
| Salam jabat tangan                      | 🟢 AMAN (BOLEH)         |
| Peluk ortu/saudara                      | 🟢 AMAN (BOLEH)         |
| Cek up dokter (didampingi)              | 🟢 AMAN (BOLEH)         |
| Disentuh paksa orang asing              | 🔴 BAHAYA (TIDAK BOLEH) |
| Diminta lepas baju oleh orang asing     | 🔴 BAHAYA (TIDAK BOLEH) |
| Disuruh simpan rahasia 'pertemuan kita' | 🔴 BAHAYA (TIDAK BOLEH) |

**Achievement:** "Penjaga Batas Tubuh" (semua benar → +200 poin).

---

## 7. ChatSim WhatsApp — Pria Halte Meng-WA (`ChatSimWhatsApp`)

> Kontak: _Nomor Tak Dikenal_ (status: online).

### Pesan masuk

1. "Hai Rara 😊 Ini om yang tadi pagi di halte, yang nanya kamu sekolah di mana. Masih ingat kan?"
2. "Om dapat nomor kamu dari temanmu, si Dina. Om bilang om kenal papamu, eh dia langsung kasih 😄"
3. "Nah, sekarang kita bisa ngobrol diam-diam ya. Fotoin kamu pakai seragam dong, jangan bilang siapa-siapa 🤫"

### Pilihan aksi

| Kategori  | Pilihan                           | Reaksi                                                                                                                |
| --------- | --------------------------------- | --------------------------------------------------------------------------------------------------------------------- |
| 🟢 AMAN   | 🚫 BLOKIR & CERITA ke ORTU (+500) | ✓ HEBAT, RA! Kamu blokir lalu CERITA ke ortu. Itu kata ajaib ke-3: CERITA. Jangan simpan rahasia!                     |
| 🟢 AMAN   | ☎ LAPOR KPAI 021-31901556 (+500)  | ✓ Hebat! Kamu lapor ke KPAI bersama ortu. Mereka akan tindak lanjut.                                                  |
| 🟡 RAGU   | ❓ Diamkan / Abaikan              | ⚠ Besok dia kirim chat lagi. Lebih baik BLOKIR lalu CERITA ke ortu — jangan dipendam.                                 |
| 🔴 BAHAYA | 💬 Balas: 'Iya Om'                | ✖ GAWAT! Orang itu minta lokasi rumahmu. Kehilangan 1 nyawa. Orang asing di chat = sama bahayanya dengan dunia nyata! |

> Catatan edukatif: nomor Rara bocor lewat teman (Dina) → **data pribadi bisa menyebar** walau Rara sudah menolak di halte.
> Bonus: 📷 Screenshot Bukti.

---

## 8. Lapor / Teriak — Klimaks (`LaporTeriakButton`)

### Narasi pembuka VN (pria geser mendekat lagi)

| #   | Pembicara  | Teks                                                                                                             |
| --- | ---------- | ---------------------------------------------------------------------------------------------------------------- |
| 1   | Narasi     | Rara memasukkan kembali HP-nya ke saku. Tapi suasana di dalam angkot terasa berubah.                             |
| 2   | Narasi     | Beberapa penumpang turun di perempatan. Kini bangku tepat di sebelah Rara kosong.                                |
| 3   | Pria Asing | "Wah, kosong nih. Om pindah ke sini aja ya, biar lebih enak ngobrolnya."                                         |
| 4   | Narasi     | Pria yang tadi menyentuh bahunya itu menggeser duduknya — makin merapat ke arah Rara.                            |
| 5   | Rara       | "Kenapa dia harus pindah ke sebelahku? Padahal masih banyak bangku lain yang kosong..."                          |
| 6   | Narasi     | Hati kecil Rara berkata ada yang tidak beres. Inilah saatnya kata sakti ketiga: CERITA — minta tolong Pak Supir! |

### Mini-game / Pilihan

- Judul: 📢 SAATNYA LAPOR!
- Deskripsi: _"Pria itu makin merapat di dalam angkot! TAHAN tombol TERIAK (atau tahan SPACE) untuk memanggil Pak Supir."_
- Tombol teriak: 🔊 TAHAN: TERIAK! · Mode VN: 📢 TERIAK panggil Pak Supir! · Mode mic: 🎙 TERIAK ke mic!
- Tombol diam: 😐 Diam saja, takut... (🔴 BAHAYA, −1 nyawa)

| Hasil                                                 | Reaksi                                                                                                                                 |
| ----------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------- |
| 🟢 BERHASIL teriak (+500, achievement "Berani Lapor") | ✓ Pak Supir mendengar & menepi! Penumpang menoleh — pria itu salah tingkah lalu turun. Rara tiba di sekolah dengan SELAMAT.            |
| 🔴 GAGAL / diam                                       | ✖ Rara terlalu takut bersuara. Untung angkot keburu sampai sekolah & Rara cepat turun — lain kali, beranikan diri TERIAK minta tolong! |

---

## 9. Kartu Edukasi (`EduCardDay2`)

> Judul: 🛡️ **CARA MENJAGA DIRI — HARI 2**

| #   | Heading                                | Isi                                                                                                                                                                       |
| --- | -------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | ✨ 3 KATA SAKTI saat merasa tidak aman | ① TIDAK! — tolak tegas & suara keras. ② PERGI — menjauh/pindah ke tempat ramai. ③ CERITA — lapor orang dewasa terpercaya. Kamu TIDAK pernah salah karena menolak/melapor. |
| 2   | 🚫 ZONA PRIBADI tubuhmu                | Bagian tubuh tertutup baju renang = milikmu. Tidak ada yang boleh menyentuh/melihat/memotretnya. Kalau ada yang mencoba (walau dikenal) = BAHAYA → TIDAK, PERGI, CERITA.  |
| 3   | 🚩 KENALI TANDA BAHAYA (grooming)      | Sok akrab & iming-iming/hadiah gratis; minta data pribadi (HP, alamat, foto); ajak menyimpan rahasia dari ortu. Semua = RED FLAG.                                         |
| 4   | 🚌 Aman di Angkot & Jalan              | Duduk dekat Pak Supir/ibu-ibu, hindari pojok sepi. Catat plat nomor & kabari ortu. Kalau aneh, turun di tempat ramai & minta tolong.                                      |
| 5   | 👮 ORANG TEPERCAYA tempat lapor        | Ortu, guru, keluarga dekat; polisi, satpam, petugas berseragam; orang dewasa di tempat ramai. Simpan nomor & beranikan diri bercerita.                                    |

> Footer Hotline: ☎ Polisi 110 · Hotline Anak 129 · KPAI 021-31901556

---

## 10. Ringkasan Hari 2 (`Day2SummaryScreen`)

| Elemen   | Teks                                                          |
| -------- | ------------------------------------------------------------- |
| Judul    | ✓ Hari 2 Selesai!                                             |
| Subtitle | Skor Hari 2 \| Nyawa tersisa: {NYAWA}/{MAXNYAWA}              |
| Panel    | 🏆 PENCAPAIAN HARI 2                                          |
| Footer   | ❗ Ingat! Polisi 110 \| Hotline Anak 129 \| KPAI 021-31901556 |
| Tombol   | ↻ ULANGI HARI 2 · LANJUT HARI 3                               |

---

## Ringkasan Pelajaran (3 Kata Sakti)

| Kata Sakti                           | Diterapkan di fase                                   |
| ------------------------------------ | ---------------------------------------------------- |
| **TIDAK** (tolak tegas, suara keras) | Halte, Sentuh Bahu                                   |
| **PERGI** (menjauh / pindah)         | Sentuh Bahu (pindah kursi)                           |
| **CERITA** (lapor orang dewasa)      | ChatSim (lapor ortu/KPAI), Lapor (panggil Pak Supir) |
