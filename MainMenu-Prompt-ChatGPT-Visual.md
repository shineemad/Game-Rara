# Prompt ChatGPT — Visual MAIN MENU (RARA: Jaga Dirimu!)

> Prompt **siap tempel ke ChatGPT (DALL·E / GPT-4o)** untuk membuat **latar belakang Main Menu**
> bergaya **Studio Ghibli**, konsisten dengan dunia game (Rara, kampung Indonesia bersih & asri, pagi hangat).
> Sumber UI: [MainMenu.cs](Assets/Scripts/MainMenu.cs) — judul **"BERANI"**, tombol MULAI GAME / CARA BERMAIN /
> PENGATURAN / TENTANG / KELUAR.

---

## 🔒 Style & Character Lock (tempel SEKALI di awal chat)

```
Kamu adalah ilustrator aset game edukasi anak "RARA: Jaga Dirimu!".
Buat latar MAIN MENU bergaya STUDIO GHIBLI yang KONSISTEN berikut:

GAYA VISUAL (WAJIB SAMA):
- Studio Ghibli–inspired 2D anime illustration, hand-painted cinematic, soft cel-shading, painterly halus.
- WAKTU PAGI HARI: golden-hour pagi hangat, langit biru cerah bersih, awan lembut, bayangan panjang halus.
- LINGKUNGAN BERSIH, TERAWAT & ASRI: kampung/sekolah Indonesia rapi — bangunan terpelihara (cat segar,
  plester rapi, tanpa retak/lumut/coretan), tanaman hijau subur, pepohonan rindang, jalan mulus tanpa sampah.
- Palet hangat & natural: terracotta, krem, hijau tropis segar, langit biru-emas pagi.
- Komposisi MENU: sisakan RUANG KOSONG yang lega (atas-kiri untuk judul besar, sisi/bawah untuk deretan
  tombol) — jangan taruh objek penting di area itu. Rasio 16:9 (landscape).
- Mood ramah anak (game edukasi SMP), hangat, mengundang, penuh harapan — TIDAK gelap/eksplisit/menyeramkan.

KARAKTER RARA (kalau diminta tampil — WAJIB IDENTIK):
- Anak perempuan Indonesia ±13 tahun, kulit sawo matang hangat, ekspresi hidup, senyum lembut percaya diri.
- Rambut hitam lurus bob sebatas dagu berponi rata; mata cokelat hangat bulat besar.
- Sweater turtleneck UNGU; rok lipit BIRU DONGKER PANJANG sampai setengah betis (mid-calf), BUKAN pendek/selutut.
- Ransel kulit cokelat, kaus kaki putih pendek, sepatu pantofel hitam (mary jane).

ATURAN KONTEN: tanpa teks/judul/logo di dalam gambar (teks ditambahkan oleh game), tanpa watermark,
tanpa kekerasan/konten dewasa.

Konfirmasi paham, lalu aku kirim varian menu satu per satu.
```

---

## 1. Main Menu — Versi Utama (Rara di Gerbang SMP HARAPAN)

> Latar utama menu: hangat, mengundang, ada ruang untuk judul & tombol.

```
Studio Ghibli anime main-menu background, landscape 16:9, NO text or logo. A warm welcoming morning
scene in front of a clean, well-kept Indonesian SMP school gate. Rara — 13-year-old Indonesian girl,
black straight chin-length bob hair with bangs, purple turtleneck sweater, long navy-blue pleated skirt
reaching mid-calf, brown leather backpack, white socks, black mary jane shoes — stands on the RIGHT side
of the frame, smiling confidently toward the viewer, ready for the day. The LEFT and UPPER area is open
sky and soft scenery (room for a big title), the LOWER-LEFT is calm ground (room for menu buttons). Lush
green trees, tidy brick walls with fresh paint, neat planters, crisp clear blue morning sky, warm
golden-hour light, long soft shadows. Painterly Ghibli cel-shading, soft brushstroke sky, inviting hopeful
mood. Cinematic 16:9, child-safe.
```

---

## 2. Main Menu — Alternatif: Gang Kampung Pagi (tanpa karakter)

> Cocok kalau ingin karakter ditaruh sebagai layer terpisah / hanya latar saja.

```
Studio Ghibli anime main-menu background, landscape 16:9, NO characters, NO text. A clean, lush green
Indonesian kampung alley in the warm early morning — well-maintained brick and plastered houses with
fresh paint and neat terracotta roofs (no cracks/moss/graffiti), tidy bamboo fences, potted plants,
coconut and banana trees, a swept litter-free cement path curving into the distance, crisp clear blue
sky, golden-hour sunlight casting long soft shadows. Keep the upper-left open for a big title and the
lower area calm for menu buttons. Painterly Ghibli cel-shading, soft brushstroke sky, peaceful inviting
mood. Cinematic 16:9, child-safe.
```

---

## 3. Main Menu — Alternatif: Perjalanan ke Sekolah (wide, Rara kecil)

> Komposisi sinematik luas, Rara kecil berjalan — terkesan petualangan/keberanian.

```
Studio Ghibli anime main-menu background, landscape 16:9, NO text. A sweeping warm morning view of a
clean, asri Indonesian kampung path leading toward a school in the distance. Rara — 13-year-old girl,
black bob hair with bangs, purple turtleneck sweater, long navy-blue pleated skirt reaching mid-calf,
brown backpack — is a small figure walking along the path in the lower-right third, full of quiet
courage. Wide open golden sky fills the upper half (room for a big title), green tidy fields, leafy
trees, well-kept houses, smooth clean road. Crisp clear blue-gold morning light, long soft shadows.
Painterly Ghibli cel-shading, cinematic depth, hopeful adventurous mood. Cinematic 16:9, child-safe.
```

---

## 4. Panel "CARA BERMAIN / PENGATURAN / TENTANG" — Latar Lembut

> Latar pop-up panel menu (lebih kalem agar teks terbaca).

```
Studio Ghibli anime soft background for a menu panel, landscape 16:9, NO text, gently blurred. A calm,
warm, out-of-focus Indonesian kampung morning scene in soft painterly Ghibli style — muted lush greens,
warm terracotta and cream tones, soft bokeh of leafy trees and tidy houses, gentle golden light. Low
contrast and softly blurred so UI text and buttons stay readable on top. No characters, no sharp focal
point. Painterly Ghibli, soothing child-safe mood. Cinematic 16:9.
```

---

## Skema Nama File (untuk dimasukkan ke game)

> Generate gambar, simpan ke folder **`Assets/sprites/main menu/`** dengan nama persis di bawah.
> Format PNG, **Texture Type = Sprite (2D and UI)**. Pasang ke slot **`MainMenu.latarSprite`** di Inspector.

| Kode | Nama File                       | Slot Tujuan                          |
| ---- | ------------------------------- | ------------------------------------ |
| 1    | `mainmenu_utama_rara.png`       | `MainMenu.latarSprite` (utama)       |
| 2    | `mainmenu_gang_pagi.png`        | (alternatif latar)                   |
| 3    | `mainmenu_perjalanan_wide.png`  | (alternatif latar)                   |
| 4    | `mainmenu_panel_blur.png`       | latar panel popup (opsional)         |

---

## Tips Penggunaan

| Hal | Cara |
| --- | --- |
| Ruang teks | Selalu minta area judul (atas-kiri) & tombol (bawah/sisi) tetap kosong — judul "BERANI" + 5 tombol |
| Tanpa teks | Jangan biarkan AI menulis judul/logo di gambar; teks dipasang oleh game |
| Konsistensi Rara | Sebut deskripsi fisik Rara lengkap (rambut bob poni, sweater ungu, rok navy panjang) |
| Rasio | Selalu **16:9 landscape** untuk latar layar penuh |
| Karakter Rara terpisah | Untuk render Rara presisi, pakai [.github/prompts/prompt-gambar-rara.prompt.md](.github/prompts/prompt-gambar-rara.prompt.md) |
