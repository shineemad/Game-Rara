---
description: "Buat prompt image-generation Studio Ghibli yang KONSISTEN untuk game RARA (karakter Rara terkunci identik tiap gambar)."
name: "Prompt Gambar RARA Konsisten"
argument-hint: "Deskripsikan adegan/dialog yang ingin dibuatkan gambarnya"
agent: "agent"
---

Tugasmu: buat **prompt image-generation berbahasa Inggris** (siap tempel ke ChatGPT/DALL·E, Midjourney, atau Leonardo) untuk adegan game edukasi anak **"RARA: Jaga Dirimu!"** yang aku berikan.

Tujuan utama: **konsistensi visual** — karakter Rara WAJIB tampak identik di setiap gambar, dan gaya selalu sama persis. Acuan gaya & wujud Rara ada di [Day3-Prompt-ChatGPT-Visual.md](../../Day3-Prompt-ChatGPT-Visual.md) (blok "Style & Character Lock"). Patuhi itu sebagai sumber kebenaran.

## Aturan Karakter Rara (WAJIB disebut lengkap di SETIAP prompt)

Selalu sertakan deskripsi fisik ini kata demi kata agar generator tidak melenceng:

- Indonesian girl, around 13 years old, warm brown (sawo matang) skin, lively relatable expression
- black straight short bob hair with bangs, round warm brown eyes
- **purple turtleneck sweater**
- **long navy-blue pleated skirt reaching mid-calf** (BUKAN selutut/pendek)
- brown leather backpack on her back
- short white socks, black mary jane shoes
- pose & ekspresi WAJIB mencerminkan emosi adegan — jangan netral/datar

## Aturan Gaya (WAJIB konsisten)

- Studio Ghibli–inspired 2D anime illustration, hand-painted cinematic, soft cel-shading, painterly brushstrokes
- pencahayaan: golden-hour hangat saat luar ruangan; cahaya hujan/lentera dramatis saat menegangkan
- palet hangat natural: terracotta, krem, hijau tropis, langit biru-emas
- latar kampung/sekolah Indonesia nyata (bata merah, genteng tanah liat, pagar bambu, pohon kelapa/pisang)
- depth of field lembut, komposisi sinematik 16:9, ruang udara di atas karakter
- mood ramah anak (SMP) — TIDAK gelap/eksplisit/menyeramkan

## Aturan Konten (keamanan anak)

- Antagonis ("Si Bayangan Gelap"/orang asing) SELALU berupa **siluet gelap** berjarak — tanpa wajah jelas, tidak grafis
- tidak ada kekerasan fisik, tidak ada konten dewasa
- fokus pada keberanian, emosi, dan pesan edukasi keselamatan

## Format Output

Untuk SETIAP adegan yang aku minta, keluarkan:

1. Judul singkat adegan (Bahasa Indonesia).
2. Kutipan dialog/narasi sumber (jika ada).
3. Satu blok kode berisi prompt **Bahasa Inggris** yang memuat: deskripsi Rara lengkap (lihat di atas) + aksi/ekspresi sesuai emosi adegan + latar + pencahayaan + `Painterly Ghibli ... Cinematic 16:9.`

Jika aku menyebut beberapa adegan sekaligus, buat satu blok per adegan dengan penomoran berurutan, mengikuti gaya penulisan yang sudah ada di [Day3-Prompt-ChatGPT-Visual.md](../../Day3-Prompt-ChatGPT-Visual.md).

## Sebelum Menulis

- Jika nama lokasi sekolah dibutuhkan pada gambar (gerbang/pintu/papan nama), pakai **"SMP HARAPAN"**.
- Jika adegan/dialog yang kuberikan ambigu, tanyakan dulu — jangan menebak.

Adegan yang ingin kubuatkan gambarnya: ${input:adegan}
