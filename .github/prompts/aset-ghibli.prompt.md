---
description: "Buat prompt image-generation aset game (props, item, UI, environment, background) bergaya Studio Ghibli yang konsisten untuk game RARA — siap tempel ke ChatGPT/DALL·E. Sudah termasuk katalog aset lengkap."
name: "Prompt Aset Ghibli RARA"
argument-hint: "Sebut nama aset (mis. 'pohon kelapa', 'tombol TERIAK', 'angkot') atau ketik 'semua' untuk seluruh katalog"
agent: "agent"
---

# Generator Prompt ASET — RARA: Jaga Dirimu!

Tugasmu: ubah nama/daftar **aset** yang aku berikan menjadi **prompt image-generation berbahasa Inggris siap tempel** (ChatGPT/DALL·E, Midjourney, Leonardo) untuk game edukasi anak **"RARA: Jaga Dirimu!"**, dengan **gaya Studio Ghibli yang konsisten** dan **siap dipakai sebagai aset game** (background bersih/transparan, tepi rapi).

Kalau aku menulis **"semua"**, keluarkan prompt untuk SELURUH item di katalog di bawah. Kalau aku menyebut kategori (mis. "props Day 3" / "UI"), keluarkan prompt untuk item di kategori itu saja. Kalau aku sebut satu aset, buat satu prompt saja.

---

## 🔒 Style Lock Aset (WAJIB sama di setiap prompt)

- Medium: **Studio Ghibli–inspired 2D anime game asset, hand-painted, soft cel-shading, gentle painterly texture**.
- Palet hangat & natural: terracotta, krem, hijau tropis subur, biru-emas pagi.
- Pencahayaan: **soft warm morning light**, bayangan lembut konsisten dari satu arah (kiri-atas).
- Lingkungan kampung/sekolah Indonesia nyata yang **bersih, terawat & asri**: bangunan terpelihara
  (cat segar, plester rapi, tanpa retak/lumut/coretan), tanaman hijau segar, jalan mulus tanpa sampah.
- Mood ramah anak (game edukasi SMP) — TIDAK gelap/eksplisit/menyeramkan.

## 🧩 Aturan "Siap Jadi Aset Game" (WAJIB)

- **Props / item / UI tunggal** → render **isolated, centered, on a plain flat/transparent background**
  (no scene, no characters), tepi bersih, **orthographic / front or 3/4 game-sprite view**, pencahayaan rata.
- **Background / environment** → render **scene 16:9 penuh tanpa karakter**, ada ruang kosong untuk
  menaruh karakter & UI, layer foreground–midground–background jelas.
- **Tilesheet / set** → susun beberapa varian item sejenis dalam grid rapi berlatar polos.
- Jangan masukkan teks/watermark/tangan/karakter ke dalam aset kecuali memang diminta.
- Konsistensi: semua aset terlihat dari satu "dunia" yang sama (palet & goresan kuas serupa).

## 🛡️ Aturan Konten (keamanan anak)

- Jika aset menyiratkan antagonis (mis. siluet ojol palsu / Si Bayangan Gelap) → gambar sebagai
  **siluet gelap berjarak tanpa wajah jelas, tidak grafis**.
- Tidak ada kekerasan, senjata realistis, atau konten dewasa.

## 🏫 Nama Sekolah

- Bila aset memuat papan nama / gerbang sekolah, gunakan **"SMP HARAPAN"**.

---

## 📦 Katalog Aset (sumber kebenaran — pakai saat aku tulis "semua"/kategori)

### A. Environment & Background

1. Background gang kampung pagi (Day 1) — jalan ramah, rumah bata terawat, pohon.
2. Background gang sepi/sempit (Day 1 jalur bahaya) — tetap rapi, sedikit lebih teduh.
3. Background halte angkot pinggir jalan (Day 2).
4. Interior angkot (Day 2) — bangku vinil, jendela, pintu samping terbuka.
5. Background parkiran SMP saat hujan sore (Day 3).
6. Gerbang depan **SMP HARAPAN** (papan nama terbaca) — ending cerah.
7. Koridor/lorong sekolah (ending trauma) — bangku kayu, lentera hangat.
8. Ruang kelas hangat (kartu edukasi) — meja kayu, papan tulis.

### B. Props / Objek

9. Pohon kelapa & pohon pisang (set).
10. Pot tanaman hias kampung (beberapa varian).
11. Pagar bambu & pagar kayu (tile).
12. Angkot (kendaraan tampak samping, warna khas).
13. Sepeda motor ojek online (parkir, tampak 3/4).
14. Plat nomor motor "B 1234 XYZ" (aset close-up).
15. Halte sederhana (atap seng, bangku).
16. Pos satpam sekolah bercahaya (Day 3).
17. Tas ransel kulit cokelat Rara (item).
18. Buku catatan PR Kesehatan (item).
19. Smartphone dengan notifikasi (item, layar abstrak).
20. Tetesan & tirai hujan (overlay efek, latar transparan).

### C. UI / HUD / Ikon

21. Ikon hati nyawa (penuh & kosong).
22. Bar skor / panel skor.
23. Tombol **TERIAK** (gaya tombol game ramah anak).
24. Tombol **PANIC** / lapor darurat (Day 3).
25. Voice meter / gauge suara (zona aman–medium–keras).
26. Panel kotak dialog VN (bingkai kayu + slot potret + nama).
27. Tombol pilihan AMAN (hijau), RAGU (kuning), BAHAYA (merah).
28. Kartu edukasi (frame kartu tips keselamatan).
29. Bingkai potret karakter (Rara / Narasi / Pria Asing).
30. Ikon "3 Kata Sakti": TIDAK! / PERGI! / CERITA!

### D. Karakter Pendukung (siluet/aman)

31. Siluet "Pria Asing" dewasa (non-grafis, berjarak).
32. Siluet ojol palsu berjas hujan + helm (Day 3).
33. Siluet "Si Bayangan Gelap" (boss, non-grafis).
34. NPC ramah: Paman Baik, ibu-ibu penumpang, satpam, guru (batik).

> Aset karakter UTAMA (Rara) bukan tugas file ini — gunakan
> [prompt-gambar-rara.prompt.md](prompt-gambar-rara.prompt.md) agar Rara tetap identik.

---

## 📝 Format Output (selalu seperti ini)

Untuk SETIAP aset, keluarkan:

1. **Judul aset** (Bahasa Indonesia) + nomor urut.
2. **Tipe**: `Prop` / `Background` / `UI` / `Siluet`.
3. Satu **blok kode** berisi prompt **Bahasa Inggris** yang memuat: deskripsi aset + gaya Ghibli
   (Style Lock di atas) + aturan "siap jadi aset game" yang sesuai tipe + penutup
   `Painterly Ghibli game asset, soft warm morning light.` Tambahkan `Cinematic 16:9` hanya untuk Background.

Untuk props/UI tunggal akhiri dengan: `isolated on a plain transparent background, centered, clean edges, orthographic game-sprite view.`

## Sebelum Menulis

- Kalau aku menyebut aset yang tidak ada di katalog, tetap buatkan promptnya mengikuti Style Lock.
- Kalau permintaanku ambigu (mis. tidak jelas prop atau background), tanyakan singkat dulu — jangan menebak.

Aset yang ingin kubuatkan: ${input:aset}
