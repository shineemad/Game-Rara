# CLAUDE.md — RARA: Jaga Dirimu!

> Dokumen ini adalah instruksi utama untuk AI (Claude / GitHub Copilot) yang membantu
> membangun game **"RARA: Jaga Dirimu!"** di Unity 2D.
> Baca seluruh dokumen sebelum menulis baris kode pertama.

---

## 1. Gambaran Proyek

| Item            | Nilai                                                    |
| --------------- | -------------------------------------------------------- |
| Nama game       | RARA: Jaga Dirimu!                                       |
| Genre           | Educational 2D Side-Scroller + Visual Novel + Boss Fight |
| Target pemain   | Siswa SMP (13–17 tahun)                                  |
| Bahasa          | Indonesia                                                |
| Engine          | Unity 2022 LTS (2D)                                      |
| Referensi web   | https://game-jaga-diri.vercel.app/ (Phaser.js)           |
| Platform target | PC (Windows/Mac), WebGL, Android (mobile)                |

**Tema edukasi**: Mengenali situasi berbahaya (orang asing, perundungan, pelecehan) dan
mengambil keputusan yang tepat. Setiap pilihan dialog dikategorikan **AMAN**, **RAGU**,
atau **BAHAYA**.

---

## 2. Arsitektur Game — Alur Scene

```
MainMenu
   └─► PrologScene (3 slide naratif)
          └─► Day1Scene  (side-scroller, jalan kaki ke sekolah)
                 └─► Day2Scene  (angkot — pilihan tempat duduk & respons)
                        └─► Day3Scene  (boss fight — lawan intimidasi)
                               └─► ResultScene (skor akhir + kartu edukasi)
```

Semua perpindahan scene melalui **SceneLoader.Instance.LoadScene(sceneName)** dengan
efek fade hitam.

---

## 3. Singleton Persisten (DontDestroyOnLoad)

Tiga singleton berikut **dibuat sekali di scene pertama** dan **tidak pernah dihancurkan**:

| Kelas          | Tanggung Jawab                              |
| -------------- | ------------------------------------------- |
| `GameState`    | Nyawa, skor, hari, pilihan, checkpoint      |
| `AudioManager` | BGM (menu/day1/day2/day3/boss/result) + SFX |
| `SceneLoader`  | Fade in/out antar scene                     |

Pola Singleton wajib:

```csharp
void Awake()
{
    if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
    else Destroy(gameObject);
}
```

---

## 4. Sistem Skor & Pilihan

```csharp
// GameState.cs — konstanta skor
public const int SCORE_AMAN   = 100;
public const int SCORE_RAGU   = 50;
public const int SCORE_BAHAYA = 0;
public const int SCORE_QUIZ   = 200;
public const int SCORE_LAPOR  = 500;
```

Setiap pilihan dialog memanggil:

```csharp
GameState.Instance.AddChoice(day, labelTeks, "AMAN" | "RAGU" | "BAHAYA");
```

Warna tombol dialog (DialogManager):

- **AMAN** → `#26AD61` (hijau)
- **RAGU** → `#F29D12` (kuning)
- **BAHAYA** → `#E84D3D` (merah)
- **Netral** → `#339FDB` (biru)

---

## 5. Sistem Dialog (DialogManager)

### Cara Memanggil Dialog

```csharp
// Dialog biasa (tanpa pilihan)
dialogManager.StartDialog(new List<DialogLine>
{
    new DialogLine { speaker = "Narasi", portrait = "narasi",
                     text = "Rara berjalan sendirian..." }
}, onComplete: () => { /* lanjutkan */ });

// Dialog dengan pilihan
var lineWithChoice = new DialogLine
{
    speaker = "Orang Asing",
    portrait = "npc",
    text = "Hai, mau ikut aku tidak?",
    choices = new Choice[]
    {
        new Choice { label = "Teriak minta tolong",  category = "AMAN" },
        new Choice { label = "Diam saja",             category = "RAGU" },
        new Choice { label = "Ikut saja",             category = "BAHAYA" }
    }
};
```

### Indeks Potret (portraits[])

| Index | Karakter         |
| ----- | ---------------- |
| 0     | Rara             |
| 1     | Boss (antagonis) |
| 2     | Polisi           |
| 3     | NPC umum         |
| 4     | Narasi           |

---

## 6. Hari 1 — Jalan Kaki ke Sekolah

### State Machine (Day1Controller)

```
Intro → Tutorial → Walking
     → Encounter1   (NPC asing mendekati)
     → PathChoice    (jalan aman vs gang sepi)
     → Walking2
     → Encounter2   (godaan / ancaman di jalur pilihan)
     → Walking3
     → Encounter3   (insiden akhir)
     → EduCard      (kartu tips keselamatan)
     → Complete     (transisi ke Day 2)
```

### Posisi Trigger X (world space)

| Trigger     | X   |
| ----------- | --- |
| Tutorial    | 5   |
| Encounter 1 | 14  |
| Path Choice | 18  |
| Encounter 2 | 26  |
| Encounter 3 | 34  |
| Edu Card    | 42  |
| End         | 52  |

### Tombol Teriak (Shout)

- Slider `shoutGauge` diisi saat tombol `shoutButton` ditahan
- `shoutFillRate = 0.5` saat ditekan, `shoutDecayRate = 0.3` saat dilepas
- NPC asing mundur saat gauge penuh

### Path Choice

```csharp
// Pilihan jalan disimpan ke GameState
GameState.Instance.pathChoice = "safe";    // jalan ramai
GameState.Instance.pathChoice = "dangerous"; // gang sepi
```

---

## 7. Hari 2 — Angkot Jurusan Sekolah

Mekanisme:

1. **Pilih tempat duduk** — dekat pintu (AMAN) vs pojok terpencil (RAGU/BAHAYA)
2. **Respons terhadap penumpang mencurigakan** — 3 pilihan AMAN/RAGU/BAHAYA
3. **Cek plat nomor angkot** (`GameState.platChecked`) — bonus skor LAPOR
4. **Foto screenshot kondisi** (`GameState.screenshotTaken`) — bonus skor

---

## 8. Hari 3 — Boss Fight (Parkiran SMP)

Mekanisme Boss:

- `bossHP` (mental boss) mulai di 100, turun berdasarkan `Choice.damage`
- **Pilihan AMAN** → damage tinggi (boss mundur)
- **Pilihan BAHAYA** → damage 0, pemain kehilangan 1 nyawa
- Tampilan: progress bar `bossHealthBar` + teks "Mental Si Bully"

### Fase Boss

```
Phase.BossIntro → Phase.Round1 → Phase.Round2 → Phase.Round3
               → Phase.BossDefeated → Phase.EduCard → Phase.Complete
```

---

## 9. HUD (HUDManager)

Setup per scene gameplay:

- `heartImages[]` — 3 Image sprite (penuh/kosong)
- `scoreText` — TMP skor
- `locationText` — nama lokasi hari ini
- `dayText` — "Hari 1 / 2 / 3"

Lokasi per hari:

```
Day 1 → "Jalan Menuju Sekolah"
Day 2 → "Angkot Jurusan Sekolah"
Day 3 → "Parkiran SMP — Musim Hujan"
```

---

## 10. Audio

### BGM Track Index

| Index | Scene      |
| ----- | ---------- |
| 0     | Menu       |
| 1     | Day 1      |
| 2     | Day 2      |
| 3     | Day 3      |
| 4     | Boss Fight |
| 5     | Result     |

```csharp
AudioManager.Instance.PlayBGM(AudioManager.BGMTrack.Day1);
AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxCorrect);
```

---

## 11. Prolog (PrologScreen)

3 slide sebelum Day 1. Setiap slide memiliki:

- `backgroundColor` — warna latar
- `backgroundSprite` — sprite latar (opsional)
- `illustration` — gambar karakter/situasi
- `title` — judul slide
- `text` — narasi teks
- `dialogSprite` — bingkai kotak dialog

Setelah slide terakhir → panggil `Day1Controller.StartDay1()`.

---

## 12. Mobile Controls (MobileControls)

Singleton statis:

```csharp
MobileControls.Horizontal  // float -1 / 0 / +1
MobileControls.IsRunning   // bool (tombol lari ditekan)
```

Player sudah membaca kedua input (keyboard + mobile) secara bersamaan.

---

## 13. Kartu Edukasi (EduCard)

Tampil di akhir setiap hari. Berisi:

- Tips keselamatan singkat (3–4 poin)
- Tombol "Lanjutkan" → transisi ke hari berikutnya

---

## 14. Result Scene

Tampilkan:

- Total skor `GameState.Instance.score`
- Daftar semua pilihan `GameState.Instance.choices`
- Pesan penutup berbeda berdasarkan rentang skor:
  - ≥ 800 → "Luar Biasa! Kamu sangat waspada."
  - 500–799 → "Bagus! Kamu cukup berhati-hati."
  - < 500 → "Kamu masih perlu belajar cara menjaga diri."

---

## 15. Konvensi Kode

| Aturan                | Detail                                                         |
| --------------------- | -------------------------------------------------------------- |
| Bahasa komentar       | Indonesia (sesuai komentar yang sudah ada)                     |
| Penamaan kelas        | PascalCase                                                     |
| Penamaan field publik | camelCase                                                      |
| Header Inspector      | Gunakan `[Header("...")]` untuk tiap grup                      |
| Singleton             | Selalu gunakan pola `Awake` + `DontDestroyOnLoad`              |
| Scene name            | String eksplisit: "MainMenu", "Day1", "Day2", "Day3", "Result" |
| Null-check            | Selalu cek `.Instance == null` sebelum akses singleton         |
| Layer sorting         | Background=0, Midground=5, Player=10, NPC=10, UI=20            |

---

## 16. Analisis Kesenjangan GDD vs Implementasi (Status per 2026-06-13)

> Perbandingan antara **GDD "3 Hari Pulang Sekolah Aman"** dengan kode yang sudah dibuat.
> Tujuan: jadi acuan apa yang masih perlu dilengkapi agar game sesuai desain.

### A. Sudah Sesuai GDD (tidak perlu diubah)

- **Voice Meter zona suara → kecepatan** (kontinu) di Hari 1 sudah ada via
  `Day1Controller.HandleShout` → `AplikasiEfekSuara`: Loud=x1.6, Medium=x0.55, Normal=x1.0.
- **Life system 3 Hati**, skor `AMAN=100 / RAGU=50 / QUIZ=200 / LAPOR=500` (`GameState`).
- **Sistem dialog 3 pilihan AMAN/RAGU/BAHAYA** + auto-pause (`NpcDialog`, `DialogManager`).
- **Hari 1**: Paman Baik, Motor Nyasar (Pemotor), Gang Gelap (PathChoice) — lengkap.
- **Hari 2**: Halte grooming (`HalteDialog`), kuis zona tubuh drag-drop 15 detik dengan
  Tangan/Pipi/Bahu=AMAN, Perut/Paha/Privat=BAHAYA, achievement "Penjaga Batas Tubuh" 6/6
  (`ZonaTubuhQuiz`), ChatSim WhatsApp + Screenshot/Blokir/Lapor KPAI (`ChatSimWhatsApp`).
- **Hari 3**: Chat Agresif, Ojol Palsu + cek plat nomor, Boss "Si Bayangan Gelap",
  Panic Button (`Day3Controller`).

### B. KEKURANGAN / Menyimpang dari GDD (perlu dilengkapi)

1. **Arsitektur SATU scene, bukan multi-scene.**
   - GDD/Bagian 2-7 dokumen ini mengasumsikan scene terpisah (MainMenu, Prolog, Day1, Day2,
     Day3, Result) + `SceneLoader` fade. **Realitas: hanya `Assets/Scenes/Gameplay.unity`**;
     transisi hari lewat `DayTransitionManager` (toggle objek), ending di dalam `Day3Controller`.
   - ⚠️ Belum ada **Result/Ending scene** & **MainMenu scene** terpisah.

2. **Ending "LAPOR SUKSES" belum digerbang oleh koleksi bukti.**
   - GDD: ending tertinggi terbuka HANYA jika pemain mengumpulkan **semua screenshot bukti
     Hari 2 & 3** + mengeksekusi semua pelaporan darurat.
   - Realitas: `HasilDay3.LaporSukses` dipicu sekadar memilih Panic Button di boss.
     `GameState` cuma punya `screenshotTaken` (satu bool), **belum ada inventory bukti per-hari**.
   - TODO: tambah penghitung bukti (mis. `buktiTerkumpul`/`buktiTotal`) + syarat ending.

3. **Mekanik Boss Fight menyimpang.**
   - GDD: pertahankan Voice Meter Zona Merah (>80 dB) **konsisten 5 detik** untuk menguras
     **Mental Bar** pelaku, **sambil** menekan Panic Button.
   - Realitas: boss = ronde pilihan verbal AMAN/RAGU/BAHAYA + opsi jendela teriak; `bossMentalMax`
     ada tapi **bukan** drain berbasis "tahan suara 5 detik". Panic Button = salah satu pilihan, bukan paralel.

4. **Speed Boost suara keras: detail beda.**
   - GDD: **+50% selama 3 detik** (ber-timer) saat teriak, lalu habis.
   - Realitas: **x1.6 (+60%) kontinu** selama suara keras, tanpa timer 3 detik.
   - **Run_Away NPC** hanya saat encounter (`HandleNPCApproach`), belum jadi state global NPC roaming.

5. **Tombol "Blokir" di Halte (Hari 2) belum ada.**
   - GDD: di halte, catcalling + minta sosmed → suara menolak **+ klik tombol 'Blokir'**.
   - Realitas: halte hanya pilihan dialog menolak; mekanik 'Blokir' baru ada di `ChatSimWhatsApp`.

6. **Respawn ke checkpoint belum jalan.**
   - GDD: kontak fisik → −1 Hati **+ respawn di checkpoint tantangan terdekat**.
   - Realitas: `checkpointD1/D2/D3` (bool) ada di `GameState` tapi **belum dipakai** untuk respawn;
     Hati habis → `GameOverScreen` (restart), bukan kembali ke checkpoint.

7. **Satuan dB tidak ditampilkan literal.**
   - GDD: ambang 50–60 / 60–80 / >80 dB. Realitas: `VoiceMeter` pakai level ternormalisasi
     - `thresholdLoud`; HUD bar suara tidak menampilkan angka dB. (Kosmetik, prioritas rendah.)

8. **Panjang level & durasi belum dikalibrasi ke GDD.**
   - GDD: Hari 1 ≈ 800px / 5 mnt, Hari 2 ≈ 1200px / 7 mnt, Hari 3 ≈ 1600px / 10 mnt.
   - Realitas: pakai world-units berbeda skala (Hari 1: trigger x=5..50); durasi belum diukur.

### C. Prioritas Saran (bila ingin menutup gap)

1. Inventory bukti + gerbang ending LAPOR SUKSES (poin B2) — paling berdampak ke alur ending.
2. Mekanik boss "tahan suara 5 detik kuras Mental Bar" (poin B3) — inti pengalaman Hari 3.
3. Respawn checkpoint (poin B6) — mengurangi frustrasi, sesuai niat GDD.
4. Tombol Blokir di halte + Speed Boost ber-timer (poin B4, B5) — penyempurnaan mekanik.

> ⚠️ Catatan: daftar ini hanya ANALISIS. Jangan implementasi otomatis — kerjakan hanya item
> yang diminta user secara eksplisit (lihat Bagian 19 aturan perubahan kode di versi instruksi).

---

## 16. Struktur Folder Assets

```
Assets/
├── Scripts/          # Semua .cs scripts
├── Scenes/           # .unity scene files
├── sprites/
│   ├── RARA/         # idle, walk (5 frame), run (4 frame)
│   ├── Paman Baik/   # 5 frame walk
│   ├── Boss/
│   └── UI/
├── sounds/
│   ├── bgm/
│   └── sfx/
├── animation/        # AnimationClip assets
└── TextMesh Pro/
```

---

## 17. Langkah Build Step-by-Step

### Step 1 — Singleton GameObjects (scene pertama / MainMenu)

1. Buat empty GO `[GameState]` → tambah `GameState.cs`
2. Buat empty GO `[AudioManager]` → tambah `AudioManager.cs` + 2 AudioSource
3. Buat empty GO `[SceneLoader]` → tambah `SceneLoader.cs` + Image (fadePanel fullscreen)

### Step 2 — Scene Prolog

1. Canvas (Screen Space - Overlay)
2. Image fullscreen hitam sebagai `fadePanel`
3. Empty GO → `PrologScreen.cs`
4. Isi 3 slide di Inspector
5. Hubungkan `onPrologEnd` → `Day1Controller.StartDay1`

### Step 3 — Scene Day 1

1. **Player GO** → `player.cs` + `SpriteRenderer` + `Rigidbody2D` (Constraints: Freeze Rotation Z) + `BoxCollider2D`
2. **Camera** → `CameraFollow.cs` (target = Player)
3. **Background layers** → `ParallaxBackground.cs`
4. **NPC Asing** → GO siluet + `NpcDialog.cs`
5. **Paman Baik** → GO + `PamanBaik.cs`
6. **Canvas HUD** → `HUDManager.cs`
7. **Canvas Dialog** → `DialogManager.cs`
8. **PathChoicePanel** → `PathChoiceUI.cs`
9. **EduCardPanel** → Button "Lanjutkan"
10. **Day1Controller GO** → `Day1Controller.cs`, hubungkan semua referensi di Inspector

### Step 4 — Scene Day 2

1. Setup Canvas + HUD + Dialog sama seperti Day 1
2. Buat `Day2Controller.cs` dengan state machine angkot
3. Tambahkan objek angkot + kursi interaktif
4. `GameState.Instance.day = 2`

### Step 5 — Scene Day 3 (Boss Fight)

1. Setup Canvas + HUD + Dialog
2. Buat `Day3Controller.cs`
3. Boss GameObject + `bossHealthBar` Slider
4. `GameState.Instance.day = 3`

### Step 6 — Result Scene

1. Canvas fullscreen
2. Text TMP: nama, skor, daftar pilihan, pesan akhir
3. Tombol "Main Lagi" → LoadScene("MainMenu") + reset GameState
4. Tombol "Keluar"

### Step 7 — Build Settings

```
File → Build Settings → tambahkan urutan:
  0: MainMenu
  1: Prolog
  2: Day1
  3: Day2
  4: Day3
  5: Result
```

---

## 18. Hal Yang Tidak Boleh Dilakukan

- ❌ Jangan gunakan `Find` atau `FindObjectOfType` di `Update()`
- ❌ Jangan hardcode skor tanpa konstanta dari `GameState`
- ❌ Jangan tambah fitur baru di luar scope 3 hari yang sudah didefinisikan
- ❌ Jangan ganti bahasa komentar ke Inggris
- ❌ Jangan pakai `Thread` atau `async/await` — gunakan Coroutine Unity
- ❌ Jangan ubah struktur folder tanpa konfirmasi user

---

## 19. Aturan Perubahan Kode (WAJIB DIPATUHI)

> **JANGAN mengubah kode apa pun selain yang diperintahkan secara eksplisit oleh user di prompt.**

- ✅ Kerjakan **HANYA** apa yang diminta dalam prompt — tidak lebih.
- ❌ Jangan menambah fitur, refactor, atau "perbaikan" yang tidak diminta.
- ❌ Jangan mengubah file/baris/fungsi lain yang tidak disebut dalam perintah.
- ❌ Jangan menambah komentar, docstring, atau anotasi pada kode yang tidak diubah.
- ❌ Jangan merapikan format/gaya kode di luar bagian yang sedang dikerjakan.
- ⚠️ Jika ada perbaikan tambahan yang menurutmu perlu, **usulkan dulu** dan tunggu persetujuan user — jangan langsung terapkan.
- ⚠️ Jika perintah/prompt **ambigu atau tidak jelas**, **tanya dulu** untuk klarifikasi sebelum mengerjakan — jangan menebak.
