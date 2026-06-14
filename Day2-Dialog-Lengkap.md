# Day 2 — Daftar Lengkap Dialog & Lokasinya

> Dokumen ini berisi **SEMUA dialog Hari 2** ("Naik Angkot ke Sekolah") secara lengkap,
> beserta **lokasi tiap dialog** (file script + posisi dalam alur). Disusun dari kode aktual
> di `Assets/Scripts/`.

## Alur Fase Hari 2

```
Prolog → Narasi Awal → Halte → Angkot (Pilih Kursi) → Sentuh
   → Quiz Zona Tubuh → ChatSim WA → Lapor/Teriak → Edu Card → Summary
```

Orkestrator: `Day2Controller.cs` (enum `Phase`: Intro, Narasi, Halte, Angkot, Sentuh, Quiz, ChatSim, Lapor, EduCard, Summary, Done).

---

## 0. Prolog Hari 2

- **Lokasi**: [Assets/Scripts/Day2PrologScreen.cs](Assets/Scripts/Day2PrologScreen.cs) — array `slides` (3 slide).
- **Kapan**: Tampil sebelum gameplay Day 2, dipicu `DayTransitionManager.LanjutKeDay2()`.

**Slide 1 — "Hari 2: Naik Angkot ke Sekolah"**
> Siang hari. Rara menunggu di halte angkot.
> Ia akan naik angkot menuju sekolah.
>
> "Pilih angkot yang ramai penumpang, ya!" pesan Ibu sebelum berangkat.

**Slide 2 — "Batas Tubuh & Dunia Digital"**
> Di dalam angkot, ada penumpang yang berperilaku mencurigakan.
>
> Selain itu, HP Rara tiba-tiba menerima pesan dari nomor tak dikenal.

**Slide 3 — "Yang Perlu Rara Tahu"**
> ● Tubuhmu = milikmu! Nggak ada yang boleh sembarangan.
> ● Area privat NGGAK BOLEH disentuh orang lain.
> ● Ada pesan mencurigakan di HP?
>     Jangan balas — langsung lapor ke orang tua!

---

## 1. Narasi Awal

- **Lokasi**: [Assets/Scripts/Day2NarasiAwal.cs](Assets/Scripts/Day2NarasiAwal.cs).
- **Kapan**: Setelah overlay judul, pembuka sebelum fase Halte.

| Pembicara | Teks |
| --------- | ---- |
| Rara | "Bismillah, aku pasti bisa! Haltenya udah dekat — ayo cepat!" |
| Rara | "Tapi… kok jalan ini sepi banget ya? Aku harus tetap waspada." |

---

## 2. Halte — Pria Asing Minta Nomor

- **Lokasi**: [Assets/Scripts/HalteDialog.cs](Assets/Scripts/HalteDialog.cs).
- **Kapan**: Fase `Halte`. Demonstrasi tanda bahaya grooming (red flag).

### Narasi Intro (pembicara: Narasi)
1. "Pagi itu Rara sampai di halte yang cukup ramai. Beberapa orang ikut menunggu angkot jurusan sekolah."
2. "Rara berdiri di pinggir sambil sesekali melihat jam. Angkotnya belum datang juga."
3. "Dari tadi, ada seorang pria asing bertopi yang terus memperhatikan Rara dari kejauhan..."
4. "Pelan-pelan, pria itu mendekat dan berdiri tepat di sebelah Rara."

### Dialog Pria Asing (dengan badge ⚠ TANDA BAHAYA)
1. "Hai, cantik! Sendirian aja nih? Om dari tadi merhatiin kamu lho."
   - ⚠ TANDA BAHAYA — *Orang asing tiba-tiba sok akrab & memperhatikanmu*
2. "Mau ke sekolah ya? Om kebetulan searah. Daripada nunggu angkot lama, bareng om aja yuk — gratis kok."
   - ⚠ TANDA BAHAYA — *Memberi iming-iming / tumpangan gratis*
3. "Eh, WA kamu berapa? Nanti om anter pulang sekolah ya. Rahasia aja, nggak usah bilang siapa-siapa."
   - ⚠ TANDA BAHAYA — *Minta data pribadi & mengajak menyimpan rahasia*

### Pilihan Pemain

| Kategori | Label | Reaksi |
| -------- | ----- | ------ |
| **AMAN** ✓ | "Maaf Om, saya nggak kenal Om. TOLONG jangan ganggu saya!" (tolak tegas + suara keras) | "✓ BAGUS, RA! Kamu menolak tegas & bersuara keras minta tolong. Orang-orang di halte langsung menoleh dan ibu-ibu menghampirimu. Pria itu salah tingkah lalu pergi. Ingat: nomor HP/WA itu DATA PRIBADI — jangan diberi ke orang asing!" |
| **RAGU** ⚠ | "Hmm... saya pikir-pikir dulu ya Om..." | "⚠ Kamu ragu-ragu menjawab. Pria itu makin maju dan terus memaksa minta nomormu. Untung angkot keburu datang dan kamu cepat naik. Lain kali, langsung TEGAS tolak ya!" |
| **BAHAYA** ✖ | "Boleh deh Om, ini WA saya..." | "✖ GAWAT! Kamu memberi nomor WA-mu ke orang asing. Malamnya HP-mu dibanjiri chat aneh dari pria itu. Kamu kehilangan 1 nyawa. Ingat: kasih sayang & hadiah dari orang asing = RED FLAG grooming!" |

### Aksi Blokir (muncul setelah pilih AMAN)
- Tombol: "🚫 BLOKIR nomor orang asing"
- Reaksi: "✓ TEPAT! Kamu BLOKIR nomor orang asing itu. Kalau ada yang memaksa minta kontak/sosmed-mu: tolak, blokir, lalu ceritakan ke orang dewasa tepercaya."
- Bonus: **+50** (bonusBlokir)

---

## 3. Angkot — Pilih Tempat Duduk

- **Lokasi**: [Assets/Scripts/AngkotSeatPicker.cs](Assets/Scripts/AngkotSeatPicker.cs).
- **Kapan**: Fase `Angkot`.

**Judul Instruksi**:
> Angkot datang. Di dalam sudah ada beberapa penumpang — termasuk seorang pria yang melirik ke arahmu. Pilih tempat dudukmu:

### Opsi Kursi

**1. Dekat Pintu (depan) — AMAN**
- Deskripsi: "Dekat Pak Supir, gampang turun cepat kalau ada apa-apa."
- Reaksi:
  1. "✓ Pintar! Kamu duduk dekat Pak Supir & pintu."
  2. "Ini posisi paling aman — gampang minta tolong & cepat turun kalau ada apa-apa."
  3. "Angkot pun mulai berjalan menuju sekolah..."

**2. Tengah (di samping ibu-ibu) — RAGU**
- Deskripsi: "Ramai tapi terjepit di tengah, agak susah turun."
- Reaksi:
  1. "⚠ Kamu duduk di tengah, terhimpit di antara penumpang."
  2. "Ramai memang — tapi kamu susah bergerak kalau terjadi sesuatu."
  3. "Angkot pun mulai berjalan menuju sekolah..."

**3. Pojok Belakang (sepi) — BAHAYA**
- Deskripsi: "Sepi & gelap, ada pria asing yang dari tadi ngeliatin kamu."
- Reaksi:
  1. "✖ Kamu memilih pojok belakang yang sepi & gelap."
  2. "Jauh dari Pak Supir — posisi ini paling rawan."
  3. "Angkot pun mulai berjalan menuju sekolah..."

> Catatan: fitur "Cek plat nomor" masih ada di kode namun sudah dilepas dari UI.

---

## 4. Sentuh — Pria Menyentuh Bahu

- **Lokasi**: [Assets/Scripts/AngkotSentuhScene.cs](Assets/Scripts/AngkotSentuhScene.cs).
- **Kapan**: Fase `Sentuh`. Intro bervariasi sesuai kursi yang dipilih sebelumnya.

### Intro — Varian AMAN (kursi dekat pintu)
| Pembicara | Teks |
| --------- | ---- |
| Narasi | "Rara duduk tepat di belakang Pak Supir, dekat pintu. Dari sini ia bisa melihat seluruh isi angkot." |
| Narasi | "Seorang pria asing yang tadi duduk di bangku belakang (bukan yang di halte) berdiri dan pindah, ikut duduk merapat di sebelah Rara." |
| Pria Asing | "Sekolah di mana, dek? Sini deket om aja, biar nggak desak-desakan." |
| Narasi | "Tangan pria itu menyentuh bahu Rara! Tapi Pak Supir ada tepat di depan — Rara tahu ia bisa segera minta tolong." |

### Intro — Varian RAGU (kursi tengah)
| Pembicara | Teks |
| --------- | ---- |
| Narasi | "Rara duduk berdesakan di tengah, terhimpit di antara ibu-ibu yang membawa belanjaan." |
| Narasi | "Dari bangku belakang, seorang pria asing (bukan yang di halte) menyusup dan memaksakan diri duduk di sela sempit, merapat ke sisi Rara." |
| Pria Asing | "Geser dikit dong, dek. Biar om bisa duduk dekat kamu." |
| Narasi | "Tangan pria itu menyentuh bahu Rara! Ibu-ibu di sekitar mulai melirik, tapi Rara terjepit dan susah bergerak." |

### Intro — Varian BAHAYA (pojok belakang)
| Pembicara | Teks |
| --------- | ---- |
| Narasi | "Rara duduk sendirian di pojok belakang yang sepi. Tak ada penumpang lain di dekatnya." |
| Narasi | "Pria asing yang sedari tadi memperhatikannya (bukan yang di halte) kini duduk persis di sebelah Rara. Tak ada siapa pun yang melihat." |
| Pria Asing | "Tenang, dek... om temani kamu sampai sekolah ya. Deket-deket om aja." |
| Narasi | "Tangan pria itu langsung menyentuh bahu Rara! Pojok ini jauh dari Pak Supir — Rara harus berani bertindak sendiri." |

### Pilihan Pemain — "Bahumu disentuh! Apa yang Rara lakukan?"

| Kategori | Label | Reaksi |
| -------- | ----- | ------ |
| **AMAN** ✓ | "\"JANGAN PEGANG SAYA!\" (teriak keras)" | "✓ HEBAT! Rara berani BERSUARA KERAS — ini kata sakti pertama: TIDAK! Semua penumpang menoleh ke arah pria itu. Sekarang lanjut: PERGI (pindah) & CERITA (lapor Pak Supir)." |
| **RAGU** ⚠ | "Geser menjauh diam-diam" | "⚠ Kamu menggeser badan menjauh, tapi pria itu masih terus mendekat. Menjauh saja belum cukup. Ingat 3 kata sakti: TIDAK (bersuara tegas) — PERGI (pindah) — CERITA (lapor Pak Supir)!" |
| **BAHAYA** ✖ | "Diam saja karena takut" | "✖ Kamu membeku dan diam, jadi pria itu makin berani. Kamu kehilangan 1 nyawa. Diam bukan salahmu — tapi kamu BISA melindungi diri. Lakukan 3 kata sakti: TIDAK — PERGI — CERITA (lapor Pak Supir)!" |

### Panel Lapor (jalur AMAN, pembicara Narasi/Rara)
| Pembicara | Teks |
| --------- | ---- |
| Narasi | "Rara langsung berdiri dan PINDAH ke kursi lebih depan, menjauh dari pria itu. (Itu kata sakti kedua: PERGI.)" |
| Rara | "Aku sudah bersuara dan menjauh. Tapi dia masih satu angkot denganku — aku harus tetap siaga sampai turun." |

---

## 5. Quiz Zona Tubuh

- **Lokasi**: [Assets/Scripts/ZonaTubuhQuiz.cs](Assets/Scripts/ZonaTubuhQuiz.cs).
- **Kapan**: Fase `Quiz`. Drag-drop label tubuh ke zona AMAN/BAHAYA, durasi 15 detik.

### Narasi Intro
| Pembicara | Teks |
| --------- | ---- |
| Narasi | "Angkot terus melaju. Setelah kejadian tadi, Rara sudah pindah ke kursi lebih depan dan mencoba menenangkan napasnya." |
| Rara | "Untung aku tadi berani bersuara dan menjauh… tapi jantungku masih berdebar. Aku perlu mengalihkan pikiran sebentar." |
| Narasi | "Rara mengeluarkan buku catatan PR Kesehatan dari tas. Kebetulan bab terakhirnya justru soal ini: \"Kenali Batas Tubuhmu — Mana yang Boleh, Mana yang Tidak.\"" |
| Rara | "Justru sekarang aku makin paham kenapa bab ini penting. Ayo aku pelajari baik-baik — siapa yang BOLEH dan TIDAK BOLEH menyentuhku." |

### Modal Tutorial
- Judul: "🧉 QUIZ: KENALI BATAS TUBUH!"
- Isi:
  > Geser / drag nama bagian tubuh ke zona yang sesuai!
  > (Atau KLIK chip-nya dulu, lalu KLIK zonanya.)
  >
  > ✅ ZONA AMAN = boleh disentuh teman & keluarga
  > ❌ ZONA BAHAYA = area privat, NGGAK BOLEH!
  >
  > ⏱ Waktu: 15 detik — cepat!
- Tombol: "▶ SIAP, MULAI!"

### Zona & Chip
- **Judul layar**: "🛡 Quiz: Seret label ke zona yang tepat!"
- **Instruksi**: "← Seret ke AMAN | Seret ke BAHAYA →"
- **Zona AMAN**: "✓ ZONA AMAN" — *Boleh disentuh teman/keluarga*
- **Zona BAHAYA**: "✖ ZONA BAHAYA" — *Area privat. Dilarang disentuh!*

| Chip | Jawaban Benar | Skor |
| ---- | ------------- | ---- |
| Bahu | AMAN | +100 |
| Tangan | AMAN | +100 |
| Pipi | AMAN | +100 |
| Paha | BAHAYA | +100 |
| Perut | BAHAYA | +100 |
| Privat | BAHAYA | +100 |

> Bonus: semua 6 benar → Achievement **"Penjaga Batas Tubuh"** + **200** poin.

### Narasi Outro (sebelum fase Lapor)
| Pembicara | Teks |
| --------- | ---- |
| Narasi | "Angkot masih melaju menuju sekolah. Rara menutup buku catatannya — tapi dari sudut matanya, pria tadi belum menyerah." |
| Rara | "Dia… masih terus melirik ke arahku. Aku harus tetap waspada sampai turun nanti." |
| Narasi | "Tiba-tiba pria itu kembali menggeser duduknya, makin merapat ke arah Rara!" |
| Rara | "Cukup! Kalau aku merasa tidak aman, aku harus CERITA — minta tolong orang dewasa. Pak Supir ada di depan!" |

---

## 6. ChatSim — Chat WhatsApp Orang Asing

- **Lokasi**: [Assets/Scripts/ChatSimWhatsApp.cs](Assets/Scripts/ChatSimWhatsApp.cs).
- **Kapan**: Fase `ChatSim`. HP Rara bergetar, pria halte meng-WA (nomor bocor lewat teman).

**Header**: Nama kontak "Nomor Tak Dikenal" • status "online" • jam "13:45".

### Pesan Masuk
1. "Hai Rara 😊 Ini om yang tadi pagi di halte, yang nanya kamu sekolah di mana. Masih ingat kan?"
2. "Om dapat nomor kamu dari temanmu, si Dina. Om bilang om kenal papamu, eh dia langsung kasih 😄"
3. "Nah, sekarang kita bisa ngobrol diam-diam ya. Fotoin kamu pakai seragam dong, jangan bilang siapa-siapa 😋"

### Tombol Aksi (timer 8 detik; timeout = otomatis BALAS/BAHAYA)

| Kategori | Label | Reaksi | Skor / Nyawa |
| -------- | ----- | ------ | ------------ |
| **AMAN** ✓ | "🚫 BLOKIR & CERITA ke ORTU" | "✓ HEBAT, RA! Kamu blokir nomornya lalu CERITA ke ortu. Itu kata ajaib ke-3: CERITA. Jangan simpan rahasia dari orang dewasa yang kamu percaya!" | +500 |
| **AMAN** ✓ | "☎ LAPOR KPAI 021-31901556" | "✓ Hebat! Kamu lapor ke KPAI bersama ortu. Mereka akan tindak lanjut." | +500 |
| **RAGU** ⚠ | "❓ Diamkan / Abaikan" | "⚠ Kamu diamkan. Tapi besok dia kirim chat lagi. Lebih baik BLOKIR lalu CERITA ke ortu — jangan dipendam sendiri." | 0 |
| **BAHAYA** ✖ | "💬 Balas: 'Iya Om'" | "✖ GAWAT! Orang itu makin pede dan minta lokasi rumahmu. Kamu kehilangan 1 nyawa. Orang asing di chat = sama bahayanya dengan di dunia nyata!" | -1 nyawa |
| Bonus | "📷 Screenshot Bukti" | (tombol opsional, simpan bukti) | +100 |

---

## 7. Lapor / Teriak — Memanggil Pak Supir

- **Lokasi**: [Assets/Scripts/LaporTeriakButton.cs](Assets/Scripts/LaporTeriakButton.cs).
- **Kapan**: Fase `Lapor` (klimaks, kata sakti ke-3: CERITA). Mini-game tahan tombol / Voice Meter.

### Narasi Pembuka (box dialog VN)
| Pembicara | Teks |
| --------- | ---- |
| Narasi | "Rara memasukkan kembali HP-nya ke saku. Tapi suasana di dalam angkot terasa berubah." |
| Narasi | "Beberapa penumpang turun di perempatan. Kini bangku tepat di sebelah Rara kosong." |
| Pria Asing | "Wah, kosong nih. Om pindah ke sini aja ya, biar lebih enak ngobrolnya." |
| Narasi | "Pria yang tadi menyentuh bahunya itu menggeser duduknya — makin merapat ke arah Rara." |
| Rara | "Kenapa dia harus pindah ke sebelahku? Padahal masih banyak bangku lain yang kosong..." |
| Narasi | "Hati kecil Rara berkata ada yang tidak beres. Inilah saatnya kata sakti ketiga: CERITA — minta tolong Pak Supir!" |

### Layar Mini-game
- Judul: "📢 SAATNYA LAPOR!"
- Deskripsi: "Pria itu makin merapat di dalam angkot! TAHAN tombol TERIAK (atau tahan SPACE) untuk memanggil Pak Supir. Tahan selama 1.5 detik sebelum waktu habis."
- Tombol: "🔊 TAHAN: TERIAK!" (mode mic: "🎙 TERIAK ke mic!")
- Mekanik: tahan 1.5 detik; window 12 detik; ambang suara 0.6 (jika pakai mic).

### Hasil
- **Berhasil**: Achievement "Berani Lapor" + **500** poin.
  > "✓ Pak Supir mendengar dan langsung menepi! Penumpang lain ikut menoleh — pria itu salah tingkah lalu turun. Angkot kembali melaju dan Rara tiba di sekolah dengan SELAMAT."
- **Gagal / waktu habis** (BAHAYA, -1 nyawa):
  > "✖ Rara terlalu takut untuk bersuara. Untung angkot keburu sampai di sekolah dan Rara cepat turun — tapi lain kali, beranikan diri TERIAK minta tolong, ya!"
- **Tombol "😐 Diam saja, takut..."** (BAHAYA, -1 nyawa): efek sama dengan gagal.
- Tombol lanjut: "▶ Lanjut ke Kartu Edukasi".

---

## 8. Edu Card — Kartu Edukasi Hari 2

- **Lokasi**: [Assets/Scripts/EduCardDay2.cs](Assets/Scripts/EduCardDay2.cs).
- **Kapan**: Fase `EduCard`.

**Judul**: "🛡️ CARA MENJAGA DIRI — HARI 2"

1. **✨ 3 KATA SAKTI saat merasa tidak aman:**
   > ① TIDAK!  — tolak dengan TEGAS & suara keras.
   > ② PERGI   — menjauh / pindah ke tempat ramai.
   > ③ CERITA  — lapor orang dewasa yang kamu percaya.
   > Kamu TIDAK pernah salah karena menolak atau melapor.

2. **🚫 ZONA PRIBADI tubuhmu:**
   > • Bagian tubuh yang tertutup baju renang = milik kamu sendiri.
   > • Tidak ada yang boleh menyentuh / melihat / memotretnya.
   > • Kalau ada yang mencoba — walau orang dikenal — itu BAHAYA. Lakukan: TIDAK, PERGI, CERITA.

3. **🚫 KENALI TANDA BAHAYA (grooming):**
   > • Orang asing sok akrab & memberi iming-iming / hadiah gratis.
   > • Minta data pribadi (nomor HP, alamat, foto).
   > • Mengajak menyimpan rahasia dari orang tua.
   > Semua itu = RED FLAG. Jangan diladeni!

4. **🚘 Aman di Angkot & Jalan:**
   > • Duduk dekat Pak Supir / ibu-ibu, hindari pojok sepi.
   > • Catat plat nomor & kabari orang tua.
   > • Kalau ada yang aneh, turun di tempat ramai dan minta tolong.

5. **👮 ORANG TEPERCAYA tempat lapor:**
   > • Orang tua, guru, dan keluarga dekat.
   > • Polisi, satpam, atau petugas berseragam.
   > • Ibu-ibu / orang dewasa di tempat ramai.
   > Simpan nomor mereka & beranikan diri bercerita.

**Footer**: ☎ Hotline: Polisi 110 | Hotline Anak 129 | KPAI 021-31901556
**Tombol**: "▶ LANJUT"

---

## 9. Summary — Ringkasan Hari 2

- **Lokasi**: [Assets/Scripts/Day2SummaryScreen.cs](Assets/Scripts/Day2SummaryScreen.cs).
- **Kapan**: Fase `Summary` (akhir Hari 2).

- **Judul**: "✓ Hari 2 Selesai!"
- **Subtitle**: "Skor Hari 2 | Nyawa tersisa: {NYAWA}/{MAXNYAWA}"
- **Progress skor**: "{SKOR} / {TARGET} poin" (target 3500)
- **Panel Pencapaian**: "🏆 PENCAPAIAN HARI 2" (mis. "Penjaga Batas Tubuh", "Berani Lapor")
- **Kata Sakti**: "🗒 Kata Sakti: ✅ TIDAK   ✅ PERGI   ✅ CERITA"
- **Tingkat Bahaya**: "⚠ Tingkat Bahaya akhir: ...% — TERKENDALI / WASPADA / RAWAN"
- **Recap keputusan**: daftar pilihan AMAN/RAGU/BAHAYA hari ini.
- **Tombol**: "↻ ULANGI HARI 2" • "▶ LANJUT HARI 3"
- **Footer**: ⚔ Ingat! Polisi 110 | Hotline Anak 129 | KPAI 021-31901556

---

## Ringkasan Lokasi File

| Fase | Dialog | File Script |
| ---- | ------ | ----------- |
| Prolog | 3 slide pembuka | [Day2PrologScreen.cs](Assets/Scripts/Day2PrologScreen.cs) |
| Narasi | Pembuka Rara | [Day2NarasiAwal.cs](Assets/Scripts/Day2NarasiAwal.cs) |
| Halte | Pria minta nomor + Blokir | [HalteDialog.cs](Assets/Scripts/HalteDialog.cs) |
| Angkot | Pilih kursi | [AngkotSeatPicker.cs](Assets/Scripts/AngkotSeatPicker.cs) |
| Sentuh | Pria sentuh bahu | [AngkotSentuhScene.cs](Assets/Scripts/AngkotSentuhScene.cs) |
| Quiz | Zona tubuh | [ZonaTubuhQuiz.cs](Assets/Scripts/ZonaTubuhQuiz.cs) |
| ChatSim | Chat WA | [ChatSimWhatsApp.cs](Assets/Scripts/ChatSimWhatsApp.cs) |
| Lapor | Teriak panggil supir | [LaporTeriakButton.cs](Assets/Scripts/LaporTeriakButton.cs) |
| EduCard | Kartu edukasi | [EduCardDay2.cs](Assets/Scripts/EduCardDay2.cs) |
| Summary | Ringkasan | [Day2SummaryScreen.cs](Assets/Scripts/Day2SummaryScreen.cs) |
| Orkestrator | Alur fase | [Day2Controller.cs](Assets/Scripts/Day2Controller.cs) |
