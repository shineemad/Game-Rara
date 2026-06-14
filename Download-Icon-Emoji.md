# 📥 Daftar Download — Memperbaiki Ikon/Emoji (Kotak □) di TextMeshPro

> Glyph kotak (□) muncul karena font TextMeshPro yang dipakai tidak punya
> karakter emoji/simbol. Unity/TMP tidak merender emoji otomatis — perlu
> font fallback atau sprite asset tambahan. Berikut daftar yang bisa diunduh.

---

## ✅ Rekomendasi Utama (paling praktis, tanpa ubah kode)

### 1. Noto Emoji (monokrom) — untuk fallback font

Membuat emoji 🎙 🔊 ⏱ ✓ ✅ 🗣 tampil (hitam-putih), bukan kotak.

- **Download:** https://fonts.google.com/noto/specimen/Noto+Emoji
- **Lisensi:** SIL Open Font License (OFL) — gratis untuk komersial
- **Format:** `.ttf`
- **Langkah Unity:**
  1. Masukkan `.ttf` ke folder `Assets/`
  2. `Window → TextMeshPro → Font Asset Creator` → buat Font Asset
  3. `Project Settings → TextMesh Pro → Settings → Fallback Font Assets` → tambahkan font ini

---

## 🌈 Untuk Emoji BERWARNA (seperti gambar referensi)

### 2. Twemoji (Twitter Emoji) — sprite sheet warna

- **Download:** https://github.com/twitter/twemoji
- **Lisensi:** CC-BY 4.0 (wajib mencantumkan kredit)
- **Format:** PNG/SVG sprite
- **Langkah Unity:**
  1. Susun emoji jadi 1 sprite sheet (grid)
  2. `Window → TextMeshPro → Sprite Importer` → buat **Sprite Asset**
  3. Pakai di teks: `<sprite name="...">`

### 3. OpenMoji — alternatif emoji warna

- **Download:** https://openmoji.org
- **Lisensi:** CC-BY-SA 4.0 (gratis, wajib kredit + share-alike)
- **Format:** PNG/SVG
- **Catatan:** Sama seperti Twemoji — diimpor sebagai TMP Sprite Asset.

---

## 🔣 Untuk Simbol/Ikon UI Konsisten

### 4. Noto Sans Symbols 2 — simbol tambahan (⏱ ✓ ▶ dll.)

- **Download:** https://fonts.google.com/noto/specimen/Noto+Sans+Symbols+2
- **Lisensi:** OFL — gratis
- **Format:** `.ttf`

### 5. Font Awesome (Free) — ikon UI (mic, peringatan, dll.)

- **Download:** https://fontawesome.com/download
- **Lisensi:** OFL (font) + CC-BY 4.0 (ikon) — versi Free gratis
- **Format:** `.ttf` / `.otf`
- **Catatan:** Buat TMP Font Asset, lalu pakai unicode ikonnya.

---

## 📋 Ringkasan Pilihan

| Kebutuhan               | Download                   | Hasil          | Lisensi          |
| ----------------------- | -------------------------- | -------------- | ---------------- |
| Cepat, ikon hitam-putih | **Noto Emoji**             | Emoji monokrom | OFL (gratis)     |
| Emoji berwarna          | **Twemoji** / **OpenMoji** | Emoji warna    | CC-BY / CC-BY-SA |
| Simbol UI tambahan      | **Noto Sans Symbols 2**    | Simbol ⏱ ✓ ▶   | OFL (gratis)     |
| Ikon UI (mic, dll.)     | **Font Awesome Free**      | Ikon vektor    | OFL / CC-BY      |

---

## 🛠️ Langkah Umum Setelah Download

### A. Untuk Font Fallback (Opsi 1, 4, 5)

1. Drag `.ttf` ke `Assets/`
2. `Window → TextMeshPro → Font Asset Creator`
3. Pilih font → **Generate Font Atlas** → **Save**
4. `Edit → Project Settings → TextMesh Pro → Settings`
5. Di **Fallback Font Assets** → klik `+` → tambahkan Font Asset tadi
6. Selesai — emoji/simbol akan otomatis muncul di semua teks TMP

### B. Untuk Sprite Asset Emoji Warna (Opsi 2, 3)

1. Susun PNG emoji jadi 1 sprite sheet
2. Drag ke `Assets/` → set **Texture Type = Sprite**, **Sprite Mode = Multiple** → Slice
3. `Window → TextMeshPro → Sprite Importer`
4. Generate **Sprite Asset**
5. (Opsional) set sebagai Default Sprite Asset di TMP Settings
6. Pakai di teks: `<sprite index=0>` atau `<sprite name="mic">`

---

> 💡 **Saran untuk game RARA:** mulai dari **Noto Emoji** (Opsi 1) sebagai
> fallback — paling cepat, gratis, dan tidak perlu mengubah kode sama sekali.
> Jika ingin tampilan emoji berwarna seperti referensi, baru lanjut ke
> **Twemoji** (Opsi 2) sebagai Sprite Asset.
