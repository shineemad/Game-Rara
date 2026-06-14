# 🎯 Daftar Ikon yang Dipakai di Game RARA + Sumber Download

> Daftar ini berisi **semua emoji/simbol yang dipakai di kode** (`Assets/Scripts/`).
> Tujuannya: tahu persis ikon mana yang perlu didukung font agar tidak jadi kotak (□).
> Ikon dibagi 2 jenis: **Emoji Warna** (perlu Sprite Asset) & **Simbol** (cukup font fallback).

---

## 🌈 A. Emoji Berwarna (butuh TMP Sprite Asset)

Ini emoji multi-byte yang idealnya berwarna. Unduh dari **Twemoji** atau **OpenMoji**
lalu jadikan TMP Sprite Asset.

| Tampil | Unicode | Nama              | Dipakai di                                              |
| ------ | ------- | ----------------- | ------------------------------------------------------- |
| 🗣     | U+1F5E3 | Speaking Head     | Day1Controller (LATIH SUARAMU), AngkotSentuhScene, Day3 |
| 🔊     | U+1F50A | Speaker Loud      | Day1Controller (tombol TERIAK)                          |
| 🎙     | U+1F399 | Studio Microphone | LaporTeriakButton (TERIAK ke mic)                       |
| 📷     | U+1F4F7 | Camera            | ChatSimWhatsApp (Screenshot Bukti)                      |
| 📝     | U+1F4DD | Memo              | AngkotSeatPicker (catat plat nomor)                     |
| 📍     | U+1F4CD | Round Pushpin     | Day1Intro, Day2Controller (label lokasi)                |
| 📜     | U+1F4DC | Scroll            | Day1SummaryScreen (Pilihanku)                           |
| 🚩     | U+1F6A9 | Triangular Flag   | Day1SummaryScreen (Red Flag)                            |
| 🚫     | U+1F6AB | No Entry          | ChatSimWhatsApp (BLOKIR)                                |
| 💬     | U+1F4AC | Speech Balloon    | ChatSimWhatsApp (Balas)                                 |
| ❤      | U+2764  | Red Heart         | Day1SummaryScreen (nyawa)                               |
| 💔     | U+1F494 | Broken Heart      | Day1SummaryScreen (nyawa habis)                         |
| 😊     | U+1F60A | Smiling Face      | ChatSimWhatsApp (chat groomer)                          |
| 😄     | U+1F604 | Grinning Face     | ChatSimWhatsApp (chat groomer)                          |
| 🤫     | U+1F92B | Shushing Face     | ChatSimWhatsApp (chat groomer)                          |
| 😤     | U+1F624 | Face w/ Steam     | Day1Intro (narasi Rara)                                 |

### Download:

- **Twemoji** (CC-BY 4.0): https://github.com/twitter/twemoji
- **OpenMoji** (CC-BY-SA 4.0): https://openmoji.org

---

## 🔣 B. Simbol (cukup font fallback monokrom)

Simbol-simbol ini cukup didukung font seperti **Noto Sans Symbols 2** /
**Noto Emoji** sebagai fallback — langsung tampil, tidak perlu sprite.

| Tampil | Unicode   | Nama                   | Dipakai di                          |
| ------ | --------- | ---------------------- | ----------------------------------- |
| ✓      | U+2713    | Check Mark             | banyak (reaksi AMAN, ✓ PENUH, dll.) |
| ✖      | U+2716    | Heavy Multiplication X | reaksi BAHAYA                       |
| ✕      | U+2715    | Multiplication X       | Day1SummaryScreen (Tutup)           |
| ❓     | U+2753    | Question Mark          | ChatSimWhatsApp (Abaikan)           |
| ⚠      | U+26A0    | Warning                | reaksi RAGU, DangerGauge            |
| ❕     | U+2755    | White Exclamation      | Day1SummaryScreen (footer)          |
| ▶      | U+25B6    | Play / Lanjut          | tombol "Lanjut" di banyak scene     |
| ▼      | U+25BC    | Down Triangle          | hint "Klik/SPACE lanjut"            |
| ●      | U+25CF    | Black Circle           | Day3PrologScreen (bullet tips)      |
| •      | U+2022    | Bullet                 | ChatSim (typing dots), Summary      |
| ⏱      | U+23F1    | Stopwatch              | ChatSim & Lapor (timer)             |
| ☎      | U+260E    | Telephone              | ChatSimWhatsApp (LAPOR KPAI)        |
| ↻      | U+21BB    | Clockwise Arrow        | Day1SummaryScreen (ULANGI)          |
| →      | U+2192    | Rightwards Arrow       | komentar & narasi                   |
| —      | U+2014    | Em Dash                | teks naratif                        |
| ① ② ③  | U+2460–62 | Circled Digits         | Day1Controller (tooltip tantangan)  |

### Download:

- **Noto Sans Symbols 2** (OFL): https://fonts.google.com/noto/specimen/Noto+Sans+Symbols+2
- **Noto Emoji** (OFL, monokrom): https://fonts.google.com/noto/specimen/Noto+Emoji

---

## 📋 Ringkasan: Apa yang Harus Diunduh

| Prioritas    | Download                   | Untuk                                       | Lisensi          |
| ------------ | -------------------------- | ------------------------------------------- | ---------------- |
| ⭐ Wajib     | **Noto Emoji**             | Semua emoji & simbol jadi tampil (monokrom) | OFL gratis       |
| ➕ Pelengkap | **Noto Sans Symbols 2**    | Simbol ▶ ▼ ● ⏱ ① lebih lengkap              | OFL gratis       |
| 🌈 Opsional  | **Twemoji** / **OpenMoji** | Emoji ❤ 🚩 📷 🗣 jadi BERWARNA              | CC-BY / CC-BY-SA |

---

## 🛠️ Cara Pasang (singkat)

### Fallback Font (Noto Emoji / Symbols) — paling mudah

1. Drag `.ttf` ke `Assets/`
2. `Window → TextMeshPro → Font Asset Creator` → Generate → Save
3. `Edit → Project Settings → TextMesh Pro → Settings`
4. **Fallback Font Assets** → `+` → tambahkan font tadi
5. Selesai — semua kotak (□) hilang, tanpa ubah kode

### Sprite Asset (Twemoji/OpenMoji) — untuk emoji warna

1. Susun PNG emoji jadi 1 sheet → impor sebagai Sprite (Multiple) → Slice
2. `Window → TextMeshPro → Sprite Importer` → Generate Sprite Asset
3. Set sebagai Default Sprite Asset di TMP Settings

---

> 💡 **Saran cepat untuk RARA:** cukup pasang **Noto Emoji** sebagai fallback font.
> Itu sudah menutup hampir semua ikon di tabel A & B di atas (monokrom).
> Baru tambahkan **Twemoji** kalau ingin ❤ 🚩 📷 tampil berwarna.
