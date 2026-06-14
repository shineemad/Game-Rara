# 🎧 Daftar Audio TAMBAHAN (Terbaru) — RARA: Jaga Dirimu!

> Lanjutan dari [DAFTAR_AUDIO.md](DAFTAR_AUDIO.md). Semua slot di checklist lama **sudah terisi**.
> File ini berisi audio **baru yang bisa kamu tambahkan** untuk memperkaya rasa game,
> lengkap dengan **rekomendasi karakter suara** ("seperti apa sound yang dicari").

**Format file**

- SFX pendek → `.wav`
- BGM / ambience panjang → `.ogg` atau `.mp3`

**Lokasi simpan:** `Assets/sounds/sfx/` · `Assets/sounds/bgm/` · `Assets/sounds/voice/`

**Legenda prioritas**

- 🔴 Celah nyata — kode sudah memanggil, file belum ada
- 🟡 Sangat disarankan
- 🟢 Pemanis opsional

---

## ✅ Status Audio yang SUDAH Ada (ringkas)

| Kategori   | File sudah terisi                                                     |
| ---------- | --------------------------------------------------------------------- |
| BGM (6)    | Menu · Day1-Jalan · Day2-Angkot · Day3-Parkiran · Boss Fight · Result |
| Ambience   | Ambience Angkot                                                       |
| SFX Umum   | Click · Correct · Wrong · Neutral · Victory · Achievement             |
| SFX Dialog | Aman · Ragu · Bahaya · Lapor                                          |
| SFX Hari 2 | Angkot · ChatMasuk · ChatKetik · Langkah · Peluit                     |
| SFX Hari 3 | BossHit · BossGroan                                                   |

---

## 1. 🔴 Celah Nyata — Kode Sudah Memanggil, File Belum Ada

> Field `AudioClip` ini sudah ada di script tapi masih kosong.
> Kalau diisi → langsung berbunyi. Kalau dibiarkan → game tetap jalan (tanpa suara).
>
> 💡 **Hemat:** kelima slot ini cukup pakai **2 file** → 1 "pop muncul" + 1 "klik konfirmasi".

### 🔴 `sfxUnlock` — Lencana terbuka

- **Dipakai di:** [AchievementPopup.cs](Assets/Scripts/AchievementPopup.cs)
- **Suara dicari:** sparkle / chime ceria naik nada, 0.8–1.5 dtk, terasa "reward"
- **Kata kunci:** `achievement unlock` · `reward sparkle` · `badge earned`

### 🔴 `sfxMuncul` — Kartu / panel muncul

- **Dipakai di:** [EduCardDay1.cs](Assets/Scripts/EduCardDay1.cs) · [EduCardDay2.cs](Assets/Scripts/EduCardDay2.cs) · [Day2SummaryScreen.cs](Assets/Scripts/Day2SummaryScreen.cs)
- **Suara dicari:** whoosh lembut + pop, < 0.6 dtk, ramah anak
- **Kata kunci:** `ui panel whoosh` · `card appear pop` · `swoosh soft`

### 🔴 `sfxMunculKartu` — Popup kartu muncul

- **Dipakai di:** [EduCardTrigger.cs](Assets/Scripts/EduCardTrigger.cs)
- **Suara dicari:** sama seperti `sfxMuncul` (boleh pakai klip yang sama)
- **Kata kunci:** `popup appear` · `slide in soft`

### 🔴 `sfxKlikLanjut` — Klik tombol "Lanjut"

- **Dipakai di:** EduCard & Summary (semua hari)
- **Suara dicari:** konfirmasi positif, klik mantap + sedikit naik nada, < 0.4 dtk
- **Kata kunci:** `confirm click` · `next button positive` · `ui confirm`

### 🔴 `sfxKlikUlangi` — Klik tombol "Ulangi"

- **Dipakai di:** [Day1SummaryScreen.cs](Assets/Scripts/Day1SummaryScreen.cs)
- **Suara dicari:** klik netral / mundur, < 0.4 dtk
- **Kata kunci:** `ui back click` · `retry button` · `soft tap`

---

## 2. 🟡 SFX Baru — Sangat Disarankan (butuh slot baru di `AudioManager`)

> Belum ada di kode. Beri tahu saya kalau mau slot-nya ditambahkan ke `AudioManager.cs`
> dan dipanggil di tempat yang tepat.

### 🟡 `sfxKehilanganNyawa` — Nyawa −1 (pilihan BAHAYA)

- **Suara dicari:** "heart break" / nada turun sedih singkat, 0.5–1 dtk, jelas terasa rugi
- **Kata kunci:** `lose life` · `heart break` · `fail descend`

### 🟡 `sfxGameOver` — Layar Game Over (nyawa habis)

- **Suara dicari:** sting kalah lembut, tidak menyeramkan untuk anak, 1.5–2.5 dtk
- **Kata kunci:** `game over kids` · `soft fail jingle` · `defeat soft`

### 🟡 `sfxDetakJantung` — Momen tegang (disentuh, pojok sepi, boss)

- **Suara dicari:** detak jantung loop pelan "dug-dug", bisa di-loop, makin cepat saat tegang
- **Kata kunci:** `heartbeat loop` · `tension heartbeat` · `suspense pulse`

### 🟡 `sfxTimerTik` — Hitung mundur Quiz (15 dtk) & ChatSim (8 dtk)

- **Suara dicari:** "tik" jam pendek per detik, atau loop ticking; nada naik saat < 3 dtk
- **Kata kunci:** `clock tick` · `countdown timer` · `ticking tense`

### 🟡 `sfxTeriakCharge` — Tahan tombol TERIAK / Voice Meter terisi

- **Suara dicari:** nada naik "power up" selagi ditahan, 1–1.5 dtk, lega saat penuh
- **Kata kunci:** `power up rising` · `charge up` · `gauge fill`

### 🟡 `sfxAngkotPintu` — Naik / turun angkot

- **Suara dicari:** pintu geser besi angkot "klek-srek", < 1 dtk
- **Kata kunci:** `van sliding door` · `bus door open` · `metal slide`

---

## 3. 🟡 SFX & Ambience Hari 3 (Parkiran Musim Hujan + Boss)

> Day 3 = "Parkiran SMP — Musim Hujan". Saat ini hanya ada `sfxBossHit` & `sfxBossGroan`.

### 🟡 `ambienceHujan` — Latar sepanjang Day 3 (loop)

- **Suara dicari:** hujan sedang merata + gemericik, loop mulus 30–60 dtk, tenang-tegang
- **Kata kunci:** `rain loop` · `rain ambience` · `light rain background`

### 🟢 `sfxPetir` — Aksen dramatis saat boss / keputusan

- **Suara dicari:** guruh + petir menggelegar, 1.5–3 dtk, jangan terlalu kasar
- **Kata kunci:** `thunder clap` · `thunder rumble` · `lightning strike`

### 🟡 `sfxPanicAlarm` — Tekan Panic Button (lapor darurat)

- **Suara dicari:** alarm / sirine pendek meyakinkan, 1–2 dtk, "bantuan datang"
- **Kata kunci:** `panic alarm` · `emergency siren short` · `alert beep`

### 🟢 `sfxMotorOjol` — Kedatangan ojol (palsu) Day 3

- **Suara dicari:** mesin motor mendekat lalu idle, 1–2 dtk
- **Kata kunci:** `motorcycle approach` · `scooter idle` · `motorbike arrive`

### 🟢 `sfxBossKalah` — Boss "Si Bayangan Gelap" mundur / kalah

- **Suara dicari:** kombinasi lega + kemenangan kecil, 1.5–2 dtk
- **Kata kunci:** `enemy defeated` · `back off grunt` · `victory small`

---

## 4. 🟢 Pemanis Interaksi (Polish — Opsional)

### 🟢 `sfxDragAmbil` — Angkat chip di Quiz Zona Tubuh

- **Suara dicari:** "pluk" ambil ringan, < 0.2 dtk
- **Kata kunci:** `pick up pop` · `grab soft` · `ui pickup`

### 🟢 `sfxDragLepas` — Jatuhkan chip ke zona

- **Suara dicari:** "tuk" tempel mantap, < 0.2 dtk
- **Kata kunci:** `drop place` · `snap soft` · `ui drop`

### 🟢 `sfxSlideProlog` — Pindah slide Prolog

- **Suara dicari:** whoosh halaman geser, < 0.5 dtk
- **Kata kunci:** `slide whoosh` · `page swipe` · `transition soft`

### 🟢 `sfxBukaBuku` — Rara buka buku catatan PR (intro Quiz)

- **Suara dicari:** lembar buku dibuka "srek", < 0.5 dtk
- **Kata kunci:** `page turn` · `book open` · `paper flip`

### 🟢 `sfxSkorNaik` — Animasi skor bertambah di Summary

- **Suara dicari:** "tik-tik-tik" cepat naik nada, loop pendek
- **Kata kunci:** `score count up` · `points tally` · `coin tick`

### 🟢 `ambienceHalteRamai` — Suasana halte ramai (Day 2 awal)

- **Suara dicari:** kerumunan ngobrol pelan + lalu lintas, loop
- **Kata kunci:** `crowd murmur` · `street crowd` · `bus stop ambience`

---

## 5. 🟢 Voice-over (opsional, kalau mau immersive)

> Bahasa Indonesia, suara natural.
> Sumber: **elevenlabs.io** · **ttsmaker.com** · Google Cloud TTS (`id-ID`).

### Narator

- **Suara dicari:** hangat, jelas, tempo sedang
- **Contoh:** "Siang itu Rara menunggu angkot di halte..."

### Rara

- **Suara dicari:** anak perempuan ~13 th, ceria tapi waspada
- **Contoh:** "Aku harus tetap waspada."

### Pria Asing

- **Suara dicari:** dewasa, sok ramah / manipulatif (tidak menyeramkan)
- **Contoh:** "Hai cantik, bareng om aja yuk."

### Pak Supir / Polisi

- **Suara dicari:** dewasa berwibawa & menenangkan
- **Contoh:** "Ada apa, nak? Biar Bapak bantu."

---

## 6. Rekomendasi Sumber & Tips Memilih

**Situs gratis (legal):**

- **freesound.org** — ambience hujan/halte, detak jantung, teriak (cek lisensi CC0/CC-BY)
- **mixkit.co** & **pixabay.com/sound-effects** — UI, notifikasi, jingle (bebas, tanpa atribusi)
- **kenney.nl** (Audio) — paket SFX UI gaya game anak (CC0)
- **zapsplat.com** — alarm, sirine, pintu, motor (perlu daftar)

**Tips memilih sound yang cocok untuk game edukasi anak:**

1. **Pendek & jelas** — SFX UI idealnya < 0.5 dtk supaya tidak menumpuk.
2. **Ramah, tidak menakuti** — hindari jumpscare/horor; pakai nada bulat & lembut.
3. **Konsisten** — semua SFX UI sebaiknya dari 1 "family" suara (mis. satu paket Kenney).
4. **Mood sesuai kategori warna** — AMAN = cerah/naik nada, BAHAYA = rendah/turun nada.
5. **Loop mulus** untuk ambience — pastikan awal & akhir menyambung tanpa "klik".
6. **Normalisasi volume** — SFX jangan lebih keras dari BGM; setel pelan saat dipasang.

**Setelan Import Unity:**
| Jenis | Load Type | Compression |
| ----- | --------- | ----------- |
| SFX pendek (< 3 dtk) | Decompress On Load | PCM / ADPCM |
| BGM & ambience panjang | Streaming | Vorbis |

---

> ⚠️ **Lisensi:** Untuk file CC-BY simpan nama pembuat + tautan untuk halaman kredit.
> File CC0 / "tanpa atribusi" bebas dipakai tanpa syarat.
>
> 📌 Untuk item di **Bagian 2–4**, slot belum ada di kode. Beri tahu saya kalau mau slot-nya
> ditambahkan ke `AudioManager.cs` beserta pemanggilannya di momen yang tepat.
