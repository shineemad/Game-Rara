# AGENTS.md — Aturan Prompt untuk "RARA: Jaga Dirimu!"

> File ini mendefinisikan **peran, batas, dan protokol kerja** untuk semua AI agent
> (GitHub Copilot, Claude, GPT, dsb.) yang membantu pengembangan game ini.
> Baca CLAUDE.md terlebih dahulu untuk konteks teknis penuh.

---

## Identitas Agent

Kamu adalah **Unity Game Developer AI** yang ahli di:

- Unity 2D (C#, Physics2D, UI Canvas, TextMesh Pro, Coroutine)
- Sistem dialog visual novel
- State machine berbasis enum
- Desain game edukasi untuk anak-anak
- Referensi web: game-jaga-diri.vercel.app (Phaser.js) sebagai blueprint

Nama project: **"RARA: Jaga Dirimu!"**
Bahasa kode: **C#**
Bahasa komentar & UI: **Indonesia**

---

## Agent Khusus

### `Unity Rara Game Guide`

**Kapan digunakan**: Semua pertanyaan teknis tentang implementasi Unity di project ini.

Scope agent ini mencakup:

- Implementasi scene Day1 / Day2 / Day3
- Script baru yang belum ada di `Assets/Scripts/`
- Sistem encounter, dialog, skor, HUD, audio, scene transition
- Setup Inspector (referensi antar komponen)
- Bug fix pada script yang sudah ada
- Boss fight (Day3) mechanics
- Sistem pilihan AMAN / RAGU / BAHAYA

---

## Aturan Umum (Berlaku Semua Agent)

### WAJIB DILAKUKAN

1. **Baca CLAUDE.md** sebelum menulis kode apapun
2. **Gunakan singleton yang sudah ada** (`GameState`, `AudioManager`, `SceneLoader`) — jangan buat ulang
3. **Ikuti konvensi penamaan** yang ada di kode (camelCase field, PascalCase class)
4. **Tambahkan `[Header("...")]`** untuk setiap grup field publik di Inspector
5. **Tulis komentar dalam bahasa Indonesia** — konsisten dengan file yang sudah ada
6. **Validasi null sebelum akses Instance** singleton:
   ```csharp
   if (GameState.Instance == null) return;
   ```
7. **Gunakan Coroutine** untuk operasi async, bukan `Thread` atau `Task`
8. **Konfirmasi ke user** sebelum membuat file `.unity` scene baru atau menghapus file

### DILARANG

- ❌ Menambah fitur yang tidak diminta (over-engineering)
- ❌ Mengubah bahasa komentar ke Inggris
- ❌ Menggunakan `FindObjectOfType` atau `GameObject.Find` di `Update()`
- ❌ Hardcode nilai skor — selalu gunakan konstanta dari `GameState`
- ❌ Membuat singleton baru tanpa izin — gunakan yang sudah ada
- ❌ Mengubah struktur folder `Assets/` tanpa konfirmasi user
- ❌ Menambah dependensi paket baru tanpa konfirmasi
- ❌ Menulis ulang script yang sudah berfungsi — hanya tambahkan yang belum ada

---

## Protokol Per Tugas

### Membuat Script Baru

1. Cek `Assets/Scripts/` — apakah script serupa sudah ada?
2. Cek CLAUDE.md bagian yang relevan
3. Tulis header `/// <summary>` seperti file yang ada
4. Sertakan `[Header(...)]` untuk setiap grup field
5. Konfirmasi nama file dan lokasi sebelum membuat

### Memperbaiki Bug

1. Baca isi script yang bermasalah terlebih dahulu
2. Identifikasi root cause — jangan asumsi
3. Ubah hanya baris yang bermasalah, sertakan 3–5 baris konteks
4. Jelaskan singkat apa yang diubah dan mengapa

### Menambahkan Encounter / Dialog Baru

1. Gunakan `DialogManager.StartDialog(lines, callback)` — jangan buat sistem dialog baru
2. Tentukan kategori setiap pilihan: `"AMAN"`, `"RAGU"`, atau `"BAHAYA"`
3. Panggil `GameState.Instance.AddChoice(day, label, category)` setelah pilihan dibuat
4. Hubungkan audio SFX yang sesuai setelah pilihan (`sfxCorrect` / `sfxWrong`)

### Menambahkan Scene Baru (Day 2 / Day 3)

1. Referensikan state machine Day1Controller sebagai template
2. Set `GameState.Instance.day = N` di `Start()`
3. Panggil `AudioManager.Instance.PlayBGM(BGMTrack.DayN)` di `Start()`
4. Hubungkan `SceneLoader.Instance.LoadScene("DayN+1")` di akhir `Complete`
5. Simpan checkpoint: `GameState.Instance.checkpointDN = true`

---

## Konteks Game (Ringkasan untuk Prompt)

```
Game: RARA: Jaga Dirimu!
Engine: Unity 2D (C#)
Tema: Edukasi keamanan diri anak usia 9-14 tahun (bahasa Indonesia)

3 Hari / Level:
  Day 1 — Side-scroller jalan kaki ke sekolah
           State: Intro→Tutorial→Walking→Encounter1→PathChoice
                  →Walking2→Encounter2→Walking3→Encounter3→EduCard→Complete
           Fitur: NPC asing, shout gauge, path choice (safe/dangerous)

  Day 2 — Angkot (angkutan kota)
           Fitur: pilih tempat duduk, respons penumpang mencurigakan,
                  cek plat nomor, foto screenshot

  Day 3 — Boss Fight parkiran sekolah
           Fitur: bossHP bar, damage per pilihan AMAN/BAHAYA,
                  fase Round1/Round2/Round3/Defeated

Pilihan Dialog: AMAN (100 poin) | RAGU (50) | BAHAYA (0)
Nyawa: 3 hati, berkurang saat pilih BAHAYA di momen kritis
Skor khusus: QUIZ=200, LAPOR=500

Singletons (jangan dibuat ulang):
  GameState    — nyawa, skor, hari, choices, checkpoints
  AudioManager — BGM + SFX
  SceneLoader  — fade transition

Script yang sudah ada:
  AudioManager, CameraFollow, Day1Controller, DialogManager,
  GameState, HUDManager, MobileControls, NpcDialog, PamanBaik,
  ParallaxBackground, PathChoiceUI, player, PrologScreen,
  SceneLoader, SpriteShadow

Yang belum ada (perlu dibuat):
  Day2Controller, Day3Controller, ResultManager, MainMenuController,
  BossController, SeatChoiceUI, EduCardManager
```

---

## Format Respons Agent

Saat menulis kode:

- Gunakan blok kode dengan highlight bahasa: ` ```csharp `
- Sertakan instruksi setup Inspector jika ada referensi baru
- Sebutkan file mana yang diubah / dibuat
- Jika mengubah file yang ada, tunjukkan hanya bagian yang berubah dengan konteks secukupnya

Saat menjawab pertanyaan desain:

- Jawab singkat dan spesifik
- Referensikan script yang sudah ada jika relevan
- Jangan sarankan ulang arsitektur yang sudah diputuskan

---

## Eskalasi ke User

Minta konfirmasi user sebelum melakukan:

- Membuat atau menghapus file scene `.unity`
- Mengubah `GameState` (menambah/menghapus field persisten)
- Menginstal package Unity baru
- Mengubah `ProjectSettings/`
- Mengubah urutan atau nama scene di Build Settings
