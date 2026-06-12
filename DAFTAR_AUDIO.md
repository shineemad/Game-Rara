# 🎧 Daftar Audio yang Harus Dicari — RARA: Jaga Dirimu!

> Checklist file suara (SFX, BGM, voice) untuk diunduh dari situs gratis lalu diisi ke
> slot Inspector `[AudioManager]`. Centang `[x]` setelah file didapat & dipasang.
>
> **Format file:** SFX pendek → `.wav` · BGM/ambience panjang → `.ogg` atau `.mp3`
> **Lokasi simpan:** `Assets/sounds/sfx/`, `Assets/sounds/bgm/`, `Assets/sounds/voice/`

---

## 1. Situs Sumber Audio Gratis (legal)

| Situs                            | Untuk apa                               | Lisensi                    |
| -------------------------------- | --------------------------------------- | -------------------------- |
| **freesound.org**                | SFX, ambience pasar/jalan, suara teriak | CC0 / CC-BY (cek per file) |
| **mixkit.co/free-sound-effects** | klik UI, notifikasi, jingle benar/salah | Gratis, tanpa atribusi     |
| **pixabay.com/sound-effects**    | SFX + BGM ringan                        | Gratis, tanpa atribusi     |
| **kenney.nl/assets** (Audio)     | paket SFX UI game (cocok anak)          | CC0                        |
| **opengameart.org**              | BGM & SFX bertema game                  | CC0 / CC-BY                |
| **zapsplat.com**                 | notifikasi HP/WhatsApp, ambience        | Gratis (perlu daftar)      |

**Voice / narasi Bahasa Indonesia:**

| Situs                                   | Catatan                                                |
| --------------------------------------- | ------------------------------------------------------ |
| **elevenlabs.io**                       | TTS kualitas tinggi, ada suara ID (free tier terbatas) |
| **ttsmaker.com**                        | Gratis, mendukung Bahasa Indonesia, boleh komersial    |
| **Google Cloud TTS** (`id-ID`, WaveNet) | Suara Indonesia natural                                |

---

## 2. SFX Umum (UI & Feedback)

| ✔   | Slot Inspector   | Deskripsi suara              | Kata kunci pencarian                 |
| --- | ---------------- | ---------------------------- | ------------------------------------ |
| [ ] | `sfxClick`       | Klik tombol lembut           | `ui click soft`, `button tap`        |
| [ ] | `sfxCorrect`     | Jawaban benar (ding ceria)   | `correct ding`, `success chime`      |
| [ ] | `sfxWrong`       | Jawaban salah (buzzer halus) | `wrong buzzer`, `error soft`         |
| [ ] | `sfxNeutral`     | Nada netral / ragu           | `neutral pop`, `soft blip`           |
| [ ] | `sfxVictory`     | Fanfare kemenangan anak      | `win fanfare kids`, `level complete` |
| [ ] | `sfxAchievement` | Lencana / achievement diraih | `achievement unlock`, `reward`       |

---

## 3. SFX Kategori Pilihan Dialog

| ✔   | Slot Inspector | Deskripsi suara                 | Kata kunci pencarian                   |
| --- | -------------- | ------------------------------- | -------------------------------------- |
| [ ] | `sfxAman`      | Jingle positif (pilihan AMAN)   | `positive jingle short`, `good choice` |
| [ ] | `sfxRagu`      | Nada netral (pilihan RAGU)      | `neutral tone`, `hmm sound`            |
| [ ] | `sfxBahaya`    | Buzzer rendah (pilihan BAHAYA)  | `low buzzer`, `danger alert low`       |
| [ ] | `sfxLapor`     | Saat klik LAPOR (recovery skor) | `report alert`, `notify positive`      |

---

## 4. SFX Khusus Hari 2 (Angkot & Chat) — slot baru

| ✔   | Slot Inspector | Deskripsi suara                         | Kata kunci pencarian                   |
| --- | -------------- | --------------------------------------- | -------------------------------------- |
| [ ] | `sfxChatMasuk` | Notifikasi chat masuk (WhatsApp "ting") | `whatsapp notification`, `message pop` |
| [ ] | `sfxChatKetik` | Bubble selesai diketik                  | `soft pop`, `type blip`                |
| [ ] | `sfxAngkot`    | Klakson/mesin angkot saat naik          | `minibus horn`, `car engine short`     |
| [ ] | `sfxPeluit`    | Peluit / teriak untuk tombol TERIAK     | `whistle blow`, `shout help`           |
| [ ] | `sfxLangkah`   | Langkah kaki Rara berjalan              | `footstep walk`, `footsteps pavement`  |

---

## 5. SFX Hari 3 (Boss Fight)

| ✔   | Slot Inspector | Deskripsi suara             | Kata kunci pencarian            |
| --- | -------------- | --------------------------- | ------------------------------- |
| [ ] | `sfxBossHit`   | Mental boss berkurang (hit) | `impact hit`, `punch soft`      |
| [ ] | `sfxBossGroan` | Boss mengeluh / mundur      | `monster groan`, `defeat grunt` |

---

## 6. BGM (Musik Latar) — array `bgmClips[]`

| ✔   | Index | Scene            | Suasana                | Kata kunci pencarian                     |
| --- | ----- | ---------------- | ---------------------- | ---------------------------------------- |
| [ ] | 0     | Menu             | Riang, ramah anak      | `kids menu loop`, `cheerful intro`       |
| [ ] | 1     | Day 1 — Jalan    | Santai jalan kaki      | `casual walking loop`, `light adventure` |
| [ ] | 2     | Day 2 — Angkot   | Tenang sedikit waspada | `calm city loop`, `light tension`        |
| [ ] | 3     | Day 3 — Parkiran | Mendung, tegang        | `rainy tense loop`, `suspense soft`      |
| [ ] | 4     | Boss Fight       | Konfrontasi            | `confrontation theme`, `boss kids`       |
| [ ] | 5     | Result           | Penutup hangat         | `happy ending loop`, `reflective`        |

---

## 7. Ambience Loop (opsional — slot baru)

| ✔   | Slot Inspector   | Deskripsi suara             | Kata kunci pencarian                         |
| --- | ---------------- | --------------------------- | -------------------------------------------- |
| [ ] | `ambienceAngkot` | Suasana jalan/angkot (loop) | `street ambience loop`, `traffic background` |

> Butuh `AudioSource` terpisah ke-3 di GameObject `[AudioManager]`, lalu drag ke slot `ambienceSource`.

---

## 8. Voice-over (opsional) — narasi & dialog

| ✔   | Karakter     | Contoh kalimat                              | Sumber                 |
| --- | ------------ | ------------------------------------------- | ---------------------- |
| [ ] | Narator      | "Rara berjalan sendirian menuju sekolah..." | TTS ID / rekam sendiri |
| [ ] | Rara         | "Aku harus hati-hati."                      | TTS suara anak/wanita  |
| [ ] | Orang Asing  | "Hai, mau ikut aku tidak?"                  | TTS suara dewasa       |
| [ ] | Polisi/Supir | "Ada apa, nak? Biar Bapak bantu."           | TTS suara dewasa ramah |

> Voice-over butuh slot `AudioClip` baru — beri tahu saya kalau ingin ditambahkan ke `AudioManager`.

---

## 9. Pengaturan Import di Unity (penting)

| Jenis                  | Load Type              | Compression |
| ---------------------- | ---------------------- | ----------- |
| SFX pendek (< 3 dtk)   | **Decompress On Load** | PCM / ADPCM |
| BGM & ambience panjang | **Streaming**          | Vorbis      |

**Langkah pasang:**

1. Drag file audio ke folder yang sesuai (`Assets/sounds/sfx` / `bgm` / `voice`).
2. Klik file → atur Load Type & Compression di Inspector → **Apply**.
3. Pilih GameObject `[AudioManager]` → drag tiap klip ke slot sesuai tabel di atas.

---

> ⚠️ **Lisensi:** Untuk file CC-BY, simpan nama pembuat + tautan untuk halaman kredit.
> File CC0 / "tanpa atribusi" bebas dipakai tanpa syarat.
