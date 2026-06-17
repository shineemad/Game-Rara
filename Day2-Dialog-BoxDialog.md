# Day 2 — Dialog dengan Kotak Dialog Sprite (VN Box)

> Daftar dialog Hari 2 yang ditampilkan lewat **kotak dialog sprite** (VN box: panel kayu + potret + nama pembicara).
> Hanya bagian naratif/percakapan berkarakter — **bukan** tombol pilihan, kuis drag-drop, chat WhatsApp, kartu edukasi, atau layar ringkasan (itu pakai panel UI biasa).

---

## 1. Narasi Awal — `Day2NarasiAwal.cs`

| Pembicara | Teks                                                           |
| --------- | -------------------------------------------------------------- |
| Rara      | "Bismillah, aku pasti bisa! Haltenya udah dekat — ayo cepat!"  |
| Rara      | "Tapi… kok jalan ini sepi banget ya? Aku harus tetap waspada." |

---

## 2. Halte — `HalteDialog.cs`

| Pembicara  | Teks                                                                                                   |
| ---------- | ------------------------------------------------------------------------------------------------------ |
| Narasi     | "Pagi itu Rara sampai di halte yang cukup ramai. Beberapa orang ikut menunggu angkot jurusan sekolah." |
| Narasi     | "Rara berdiri di pinggir sambil sesekali melihat jam. Angkotnya belum datang juga."                    |
| Narasi     | "Dari tadi, ada seorang pria asing bertopi yang terus memperhatikan Rara dari kejauhan..."             |
| Narasi     | "Pelan-pelan, pria itu mendekat dan berdiri tepat di sebelah Rara."                                    |
| Pria Asing | "Hai, cantik! Sendirian aja nih? Om dari tadi merhatiin kamu lho."                                     |
| Pria Asing | "Mau ke sekolah ya? Om kebetulan searah. Daripada nunggu angkot lama, bareng om aja yuk — gratis kok." |
| Pria Asing | "Eh, WA kamu berapa? Nanti om anter pulang sekolah ya. Rahasia aja, nggak usah bilang siapa-siapa."    |

---

## 3. Sentuh di Angkot — `AngkotSentuhScene.cs`

### Varian AMAN (kursi dekat pintu)

| Pembicara  | Teks                                                                                                                                  |
| ---------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| Narasi     | "Rara duduk tepat di belakang Pak Supir, dekat pintu. Dari sini ia bisa melihat seluruh isi angkot."                                  |
| Narasi     | "Seorang pria asing yang tadi duduk di bangku belakang (bukan yang di halte) berdiri dan pindah, ikut duduk merapat di sebelah Rara." |
| Pria Asing | "Sekolah di mana, dek? Sini deket om aja, biar nggak desak-desakan."                                                                  |
| Narasi     | "Tangan pria itu menyentuh bahu Rara! Tapi Pak Supir ada tepat di depan — Rara tahu ia bisa segera minta tolong."                     |

### Varian RAGU (kursi tengah)

| Pembicara  | Teks                                                                                                                                      |
| ---------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| Narasi     | "Rara duduk berdesakan di tengah, terhimpit di antara ibu-ibu yang membawa belanjaan."                                                    |
| Narasi     | "Dari bangku belakang, seorang pria asing (bukan yang di halte) menyusup dan memaksakan diri duduk di sela sempit, merapat ke sisi Rara." |
| Pria Asing | "Geser dikit dong, dek. Biar om bisa duduk dekat kamu."                                                                                   |
| Narasi     | "Tangan pria itu menyentuh bahu Rara! Ibu-ibu di sekitar mulai melirik, tapi Rara terjepit dan susah bergerak."                           |

### Varian BAHAYA (pojok belakang)

| Pembicara  | Teks                                                                                                                                    |
| ---------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| Narasi     | "Rara duduk sendirian di pojok belakang yang sepi. Tak ada penumpang lain di dekatnya."                                                 |
| Narasi     | "Pria asing yang sedari tadi memperhatikannya (bukan yang di halte) kini duduk persis di sebelah Rara. Tak ada siapa pun yang melihat." |
| Pria Asing | "Tenang, dek... om temani kamu sampai sekolah ya. Deket-deket om aja."                                                                  |
| Narasi     | "Tangan pria itu langsung menyentuh bahu Rara! Pojok ini jauh dari Pak Supir — Rara harus berani bertindak sendiri."                    |

### Panel Lapor (jalur AMAN)

| Pembicara | Teks                                                                                                           |
| --------- | -------------------------------------------------------------------------------------------------------------- |
| Narasi    | "Rara langsung berdiri dan PINDAH ke kursi lebih depan, menjauh dari pria itu. (Itu kata sakti kedua: PERGI.)" |
| Rara      | "Aku sudah bersuara dan menjauh. Tapi dia masih satu angkot denganku — aku harus tetap siaga sampai turun."    |

---

## 4. Quiz Zona Tubuh (narasi pembungkus) — `ZonaTubuhQuiz.cs`

### Intro

| Pembicara | Teks                                                                                                                                                            |
| --------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Narasi    | "Angkot terus melaju. Setelah kejadian tadi, Rara sudah pindah ke kursi lebih depan dan mencoba menenangkan napasnya."                                          |
| Rara      | "Untung aku tadi berani bersuara dan menjauh… tapi jantungku masih berdebar. Aku perlu mengalihkan pikiran sebentar."                                           |
| Narasi    | "Rara mengeluarkan buku catatan PR Kesehatan dari tas. Kebetulan bab terakhirnya justru soal ini: \"Kenali Batas Tubuhmu — Mana yang Boleh, Mana yang Tidak.\"" |
| Rara      | "Justru sekarang aku makin paham kenapa bab ini penting. Ayo aku pelajari baik-baik — siapa yang BOLEH dan TIDAK BOLEH menyentuhku."                            |

### Outro

| Pembicara | Teks                                                                                                                    |
| --------- | ----------------------------------------------------------------------------------------------------------------------- |
| Narasi    | "Angkot masih melaju menuju sekolah. Rara menutup buku catatannya — tapi dari sudut matanya, pria tadi belum menyerah." |
| Rara      | "Dia… masih terus melirik ke arahku. Aku harus tetap waspada sampai turun nanti."                                       |
| Narasi    | "Tiba-tiba pria itu kembali menggeser duduknya, makin merapat ke arah Rara!"                                            |
| Rara      | "Cukup! Kalau aku merasa tidak aman, aku harus CERITA — minta tolong orang dewasa. Pak Supir ada di depan!"             |

---

## 5. Lapor/Teriak (narasi pembuka) — `LaporTeriakButton.cs`

| Pembicara  | Teks                                                                                                               |
| ---------- | ------------------------------------------------------------------------------------------------------------------ |
| Narasi     | "Rara memasukkan kembali HP-nya ke saku. Tapi suasana di dalam angkot terasa berubah."                             |
| Narasi     | "Beberapa penumpang turun di perempatan. Kini bangku tepat di sebelah Rara kosong."                                |
| Pria Asing | "Wah, kosong nih. Om pindah ke sini aja ya, biar lebih enak ngobrolnya."                                           |
| Narasi     | "Pria yang tadi menyentuh bahunya itu menggeser duduknya — makin merapat ke arah Rara."                            |
| Rara       | "Kenapa dia harus pindah ke sebelahku? Padahal masih banyak bangku lain yang kosong..."                            |
| Narasi     | "Hati kecil Rara berkata ada yang tidak beres. Inilah saatnya kata sakti ketiga: CERITA — minta tolong Pak Supir!" |

---

## Catatan

- Fase yang **TIDAK** memakai kotak dialog sprite VN:
  - **Pilih kursi** (`AngkotSeatPicker.cs`) — pakai "box narasi reaksi" tanpa pembicara.
  - **Kuis drag-drop** (`ZonaTubuhQuiz.cs` bagian permainan).
  - **ChatSim WhatsApp** (`ChatSimWhatsApp.cs`) — chat bubble.
  - **Kartu Edukasi** (`EduCardDay2.cs`).
  - **Summary** (`Day2SummaryScreen.cs`).
- `Day2Controller.cs` punya metode `NarasiVN` (box dialog VN "jembatan" antar-fase) yang meniru gaya HalteDialog; teksnya diisi per-pemanggilan di kode.
