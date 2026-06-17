# 🎯 Daftar Ikon Hari 3 (Boss Fight) — RARA: Jaga Dirimu!

> Analisis **semua emoji & simbol yang dipakai di kode Hari 3**
> ([Day3Controller.cs](Assets/Scripts/Day3Controller.cs), [Day3PrologScreen.cs](Assets/Scripts/Day3PrologScreen.cs)).
> Tujuan: tahu ikon mana yang perlu didukung font/sprite agar tidak jadi kotak (□),
> dan **di mana mengunduhnya**.
> Ikon dibagi 2 jenis: **Emoji Warna** (ideal pakai Sprite Asset) & **Simbol** (cukup font fallback).

---

## 🌈 A. Emoji Berwarna (butuh TMP Sprite Asset / Noto Emoji)

Emoji multi-byte yang idealnya berwarna. Unduh dari **Twemoji** atau **OpenMoji**
lalu jadikan TMP Sprite Asset. Untuk versi cepat (monokrom) cukup **Noto Emoji**.

| Tampil | Unicode | Nama                        | Dipakai di (Hari 3)                                    |
| ------ | ------- | --------------------------- | ------------------------------------------------------ |
| 😱     | U+1F631 | Face Screaming in Fear      | Narasi boss menghadang (`bossIntroNarasi`)             |
| 🌧     | U+1F327 | Cloud with Rain             | Narasi jalan menembus hujan                            |
| 🏃     | U+1F3C3 | Runner                      | Instruksi jalan & tutorial TAP/TERIAK                  |
| 🚶     | U+1F6B6 | Pedestrian                  | Ikon runner di bar jalan (`BuatTeks` Runner)           |
| 😅     | U+1F605 | Grinning Face w/ Sweat      | Narasi sampai parkiran basah kuyup                     |
| 😨     | U+1F628 | Fearful Face                | Narasi notif nomor tak dikenal                         |
| 😢     | U+1F622 | Crying Face                 | Chat agresif ("Hujan deras ya")                        |
| 🥺     | U+1F97A | Pleading Face               | Chat groomer (minta foto seragam)                      |
| 😏     | U+1F60F | Smirking Face               | Ucapan boss (bujukan "mau kemana sendirian")           |
| 📱     | U+1F4F1 | Mobile Phone                | Chat groomer (minta foto)                              |
| 📸     | U+1F4F8 | Camera with Flash           | Pilihan AMAN "Foto plat dulu, lalu tolak naik"         |
| 💪     | U+1F4AA | Flexed Biceps               | Reaksi percaya diri, kartu edukasi, narasi LAPOR       |
| 🔊     | U+1F50A | Speaker High Volume         | Pilihan teriak KERAS tiap ronde boss                   |
| 📢     | U+1F4E2 | Loudspeaker                 | Pilihan LAPOR "TERIAK + lari ke satpam", tombol TERIAK |
| 📣     | U+1F4E3 | Megaphone                   | Kartu edukasi ("Yang Paling Penting")                  |
| 🆘     | U+1F198 | SOS Button                  | Pilihan LAPOR + baris darurat kartu edukasi            |
| 🚨     | U+1F6A8 | Police Cars Revolving Light | Label `panicLabel` "PANIC BUTTON"                      |
| 🚔     | U+1F694 | Oncoming Police Car         | `panicNarasi` (satpam & guru datang)                   |
| 💔     | U+1F494 | Broken Heart                | `endingTraumaJudul` "GAME OVER"                        |
| 🏆     | U+1F3C6 | Trophy                      | `eduJudul` "Kartu Edukasi — Hari 3: FINAL"             |
| 🏁     | U+1F3C1 | Chequered Flag              | `hasilJudul` "TANTANGAN SELESAI!"                      |
| 🦁     | U+1F981 | Lion                        | Kartu edukasi ("Cara Melindungi Diri")                 |

### Download:

- **Twemoji** (CC-BY 4.0): https://github.com/twitter/twemoji
- **OpenMoji** (CC-BY-SA 4.0): https://openmoji.org
- **Noto Emoji** (OFL, monokrom): https://fonts.google.com/noto/specimen/Noto+Emoji

---

## 🔣 B. Simbol (cukup font fallback monokrom)

Simbol ini cukup didukung **Noto Sans Symbols 2** / **Noto Emoji** sebagai fallback —
langsung tampil, tidak perlu sprite.

| Tampil | Unicode | Nama                   | Dipakai di (Hari 3)                             |
| ------ | ------- | ---------------------- | ----------------------------------------------- |
| ✓      | U+2713  | Check Mark             | Semua reaksi AMAN ("✓ Bagus!", "✓ HEBAT!")      |
| ✖      | U+2716  | Heavy Multiplication X | Semua reaksi BAHAYA ("✖ DIAM ITU BAHAYA")       |
| ⚠      | U+26A0  | Warning                | Reaksi RAGU + judul "Apa itu Grooming?"         |
| ▼      | U+25BC  | Down Triangle          | `hintText` prolog "▼ SPACE / KLIK UNTUK LANJUT" |
| ➔      | U+2794  | Heavy Wide-Head Arrow  | `jalanInstruksi` ("➔ Jalan ke parkiran")        |
| →      | U+2192  | Rightwards Arrow       | Komentar & narasi alur (Tooltip panic button)   |
| •      | U+2022  | Bullet                 | Kartu edukasi (poin "Cara Melindungi Diri")     |
| —      | U+2014  | Em Dash                | Banyak teks naratif & nama ronde                |

### Download:

- **Noto Sans Symbols 2** (OFL): https://fonts.google.com/noto/specimen/Noto+Sans+Symbols+2
- **Noto Emoji** (OFL, monokrom): https://fonts.google.com/noto/specimen/Noto+Emoji

---

## 📋 Ringkasan: Apa yang Harus Diunduh

| Prioritas    | Download                   | Untuk                                         | Lisensi          |
| ------------ | -------------------------- | --------------------------------------------- | ---------------- |
| ⭐ Wajib     | **Noto Emoji**             | Semua emoji & simbol Hari 3 tampil (monokrom) | OFL gratis       |
| ➕ Pelengkap | **Noto Sans Symbols 2**    | Simbol ▼ ➔ → • ⚠ lebih lengkap                | OFL gratis       |
| 🌈 Opsional  | **Twemoji** / **OpenMoji** | Emoji 😱 📸 🔊 🏆 🚨 jadi BERWARNA            | CC-BY / CC-BY-SA |

---

## 🛠️ Langkah Pasang di Unity (TextMeshPro)

1. **Font fallback (cara tercepat, tanpa ubah kode):**
   - Taruh `NotoEmoji-Regular.ttf` ke folder `Assets/`.
   - `Window → TextMeshPro → Font Asset Creator` → buat Font Asset.
   - `Project Settings → TextMesh Pro → Settings → Fallback Font Assets` → tambahkan font itu.

2. **Emoji berwarna (opsional, sesuai gambar referensi):**
   - Susun PNG emoji (Twemoji/OpenMoji) jadi 1 sprite sheet.
   - `Window → TextMeshPro → Sprite Importer` → buat **Sprite Asset**.
   - Pakai di teks: `<sprite name="...">`.

> ⚠️ Catatan: emoji di kode Hari 3 ditulis sebagai escape `\uXXXX` / surrogate pair
> (mis. `\uD83D\uDD0A` = 🔊). Tidak perlu diubah — yang penting font/sprite-nya tersedia.
