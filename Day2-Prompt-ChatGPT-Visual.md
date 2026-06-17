# Prompt ChatGPT — Visual Per-Dialog Day 2 (RARA: Jaga Dirimu!)

> Kumpulan **prompt image-generation siap tempel ke ChatGPT (DALL·E / GPT-4o)** untuk SETIAP box dialog Hari 2.
> Referensi visual: **gambar 1** — Rara berjalan di gang kampung saat pagi, gaya Studio Ghibli painterly,
> sweater ungu + rok navy panjang, cahaya golden-hour hangat.
> Sumber dialog: [Day2-Dialog-BoxDialog.md](Day2-Dialog-BoxDialog.md).

---

## 🔒 Style & Character Lock (tempel SEKALI di awal chat)

> Salin blok ini **lebih dulu** ke ChatGPT agar semua gambar berikutnya konsisten dengan gambar 1.

```
Kamu adalah ilustrator game edukasi anak "RARA: Jaga Dirimu!".
Gambar SEMUA adegan dengan GAYA & KARAKTER yang KONSISTEN berikut (acuan: gambar referensi Rara
berjalan di gang kampung saat pagi).

GAYA VISUAL (WAJIB SAMA):
- Studio Ghibli–inspired 2D anime illustration, hand-painted cinematic, soft cel-shading.
- Tekstur painterly halus: brushstroke lembut di langit, dedaunan, dan tembok.
- WAKTU SELALU PAGI HARI: golden-hour pagi hangat, sinar matahari rendah menembus gang, embun tipis,
  langit biru cerah bersih, bayangan panjang lembut.
- LINGKUNGAN BERSIH & ASRI: jalan rapi tanpa sampah, tanaman hijau subur, pepohonan rindang,
  bunga & rumput segar, udara segar, suasana sejuk dan tertata.
- LINGKUNGAN TERAWAT (TERUTAMA BANGUNAN): semua bangunan tampak terawat & terpelihara baik —
  dinding bersih & utuh (cat segar, plester rapi, tanpa retak/lumut/coretan), genteng tertata rapi,
  jendela & pintu bersih, pagar & halte kokoh tidak rusak, jalan & trotoar mulus tanpa lubang.
- Palet hangat & natural: terracotta, krem, hijau tropis segar, langit biru-emas pagi.
- Background kampung Indonesia nyata yang asri: rumah bata & berplester bersih terawat, genteng tanah liat
  rapi, pagar bambu rapi, pot tanaman, pohon kelapa & pisang, taman kecil, jalan bersemen bersih.
- Depth of field lembut: foreground tajam, background soft blur film camera.
- Komposisi sinematik 16:9, ruang udara di atas karakter.
- Mood ramah anak (game edukasi SMP), TIDAK gelap/eksplisit/menyeramkan.

KARAKTER RARA (WAJIB IDENTIK tiap gambar — acuan persis gambar 1):
- Anak perempuan Indonesia ±13 tahun, kulit sawo matang hangat, ekspresi hidup & relatable, senyum lembut.
- Rambut hitam pekat lurus, model bob sebatas dagu, berponi rata menutup dahi; mata cokelat hangat bulat besar.
- Sweater rajut turtleneck UNGU lengan panjang, agak longgar.
- Rok lipit (pleated) BIRU DONGKER PANJANG model A-line, ujung rok JATUH SAMPAI SETENGAH BETIS (mid-calf).
  WAJIB rok panjang — JANGAN PERNAH gambar rok pendek / mini / di atas lutut / selutut.
- Ransel kulit cokelat di punggung dengan dua tali bahu, kaus kaki putih pendek, sepatu pantofel hitam (mary jane).
- Proporsi anak (kepala agak besar, badan ramping), bukan dewasa.
- Pose & ekspresi SELALU mencerminkan emosi dialog — jangan pose netral/datar.

ATURAN KONTEN:
- "Pria Asing" = orang asing dewasa, digambar sebagai SILUET/sosok berjarak yang tidak ramah,
  tanpa wajah grafis, tidak eksplisit, tidak ada kontak fisik yang vulgar.
- Tidak ada kekerasan, tidak ada konten dewasa. Fokus emosi waspada & keberanian Rara.

Konfirmasi paham, lalu aku kirim adegan satu per satu.
```

---

## 🔑 Character Lock — Versi Inggris (sisipkan ke SETIAP prompt)

> Pastikan kalimat ini selalu ada di awal tiap prompt agar Rara identik di semua gambar.

```
Rara — Indonesian girl around 13 years old, child proportions (NOT an adult), warm brown
(sawo matang) skin, lively relatable expression with a soft smile; jet-black straight chin-length
bob hair with full straight bangs covering the forehead, big round warm brown eyes; loose PURPLE
long-sleeve knit turtleneck sweater; LONG dark navy-blue A-line pleated skirt whose hem falls all
the way to MID-CALF — the skirt is clearly LONG (absolutely NOT short, mini, above-knee, or
knee-length); brown leather backpack with two shoulder straps on her back; short white ankle socks,
black mary jane shoes. Her pose & expression ALWAYS reflect the scene's emotion — never neutral or
flat. Studio Ghibli painterly cel-shading, warm early-MORNING golden-hour light, clean, tidy and
lush green Indonesian kampung setting with well-maintained, well-kept buildings (clean intact walls with
fresh paint and neat plaster, no cracks/moss/graffiti, tidy roof tiles, clean windows and doors, sturdy
undamaged fences, smooth litter-free roads), fresh plants, leafy trees, crisp clear blue sky.
Cinematic 16:9, child-safe.
```

**Aturan wajib:** sebut deskripsi Rara lengkap di tiap prompt (jangan disingkat "Rara" saja);
rok HARUS panjang sampai setengah betis — jangan pernah pendek/selutut; "Pria Asing" selalu
sosok/siluet dewasa berjarak tanpa wajah grafis; gaya selalu Painterly Ghibli 16:9.

---

## 00. Prolog Day 1 — `PrologScreen.cs`

> Narasi pembuka Hari 1 (3 slide) — ditambahkan di sini sesuai permintaan.
> Sumber: [PrologScreen.cs](Assets/Scripts/PrologScreen.cs).

### 00.1 — Slide 1: Hari 1, Jalan Kaki ke Sekolah

> **Pembicara:** Slide 1 — Judul: _"Hari 1: Jalan Kaki ke Sekolah"_
> **Teks:** _"Pagi hari di sebuah jalan menuju sekolah. Rara, gadis 13 tahun berbaju ungu, bersiap berangkat ke SMP Harapan. \"Hati-hati di jalan, Rara!\" kata Ibu."_

```
Studio Ghibli anime illustration, narrative prolog card. Rara — 13-year-old Indonesian girl, black
straight bob hair with bangs, purple turtleneck sweater, long RED pleated skirt reaching mid-calf,
brown leather backpack, white socks, black mary jane shoes — stands at the front gate of her home in the
bright clean morning, ready to walk to school, waving goodbye with a cheerful hopeful smile. Her mother's
warm presence at the doorway behind her. Clean tidy kampung street ahead with lush green plants, leafy
trees, terracotta-roofed houses, neat bamboo fences, crisp clear blue sky, warm golden morning light and
long soft shadows. Painterly Ghibli cel-shading, soft brushstroke sky. Cinematic 16:9, child-safe.
```

### 00.2 — Slide 2: Kenali Batas

> **Pembicara:** Slide 2 — Judul: _"Kenali Batas"_
> **Teks:** _"Di luar rumah, banyak orang lalu-lalang. Nggak semua orang asing bisa dipercaya! Rara harus tetap waspada dan berani bersuara kalau ada yang bikin dia nggak nyaman."_

```
Studio Ghibli anime illustration, gently cautionary prolog card. Rara — black straight bob hair with
bangs, purple turtleneck sweater, long RED pleated skirt reaching mid-calf, brown backpack — walks along
a clean, lush green Indonesian kampung street in the morning where several blurred passers-by move about,
including one soft-blurred shadowy adult figure (faceless, non-graphic) in the background. Rara's
expression: alert and observant but composed, glancing carefully around. Warm morning light, tidy leafy
surroundings, but a subtle note of watchfulness. Antagonist stays a distant non-graphic silhouette.
Painterly Ghibli depth of field. Cinematic 16:9, child-safe.
```

### 00.3 — Slide 3: Panduan Bermain

> **Pembicara:** Slide 3 — Judul: _"Panduan Bermain"_
> **Teks:** _"← → : Gerakkan Rara ke kiri / kanan · Shift + ← → : Lari lebih cepat · SPASI / Klik : Interaksi · Tombol TERIAK : Usir orang asing yang mendekat · Nyawa Rara ada 3. Hati-hati!"_

```
Studio Ghibli anime illustration, friendly tutorial guidance card. Rara — 13-year-old Indonesian girl,
black straight bob hair with bangs, purple turtleneck sweater, long RED pleated skirt reaching mid-calf,
brown backpack — stands confidently facing the viewer on a clean lush green kampung street in the morning,
giving a cheerful ready-to-go pose with a small confident smile, one hand raised in a friendly wave. Soft
warm glow, bright tidy background with subtle friendly game-guidance motifs (gentle arrow hints, a small
glowing heart trio for lives). A welcoming, empowering tone. Painterly Ghibli cel-shading, warm uplifting
mood. Cinematic 16:9, child-safe.
```

---

## 0. Prolog Day 2 — `Day2PrologScreen.cs`

### 0.1 — Slide 1: Hari 2, Naik Angkot ke Sekolah

> **Pembicara:** Slide 1 — Judul: _"Hari 2: Naik Angkot ke Sekolah"_
> **Teks:** _"Siang hari. Rara menunggu di halte angkot. Ia akan naik angkot menuju sekolah. "Pilih angkot yang ramai penumpang, ya!" pesan Ibu sebelum berangkat."_

```
Studio Ghibli anime illustration, narrative prolog card. Rara — 13-year-old Indonesian girl, black
straight bob hair with bangs, purple turtleneck sweater, long navy-blue pleated skirt reaching mid-calf,
brown leather backpack, white socks, black mary jane shoes — stands waiting at a modest Indonesian
roadside bus stop (halte) in the bright clean morning, looking toward the road expecting an angkot
(minibus). Lush green palm trees, tidy brick shophouses, fresh planters, swept road with painted lines,
crisp clear blue sky, warm golden light. Her expression: hopeful and ready, remembering her mother's
advice. Painterly Ghibli cel-shading, soft brushstroke sky. Cinematic 16:9, child-safe.
```

### 0.2 — Slide 2: Batas Tubuh & Dunia Digital

> **Pembicara:** Slide 2 — Judul: _"Batas Tubuh & Dunia Digital"_
> **Teks:** _"Di dalam angkot, ada penumpang yang berperilaku mencurigakan. Selain itu, HP Rara tiba-tiba menerima pesan dari nomor tak dikenal."_

```
Studio Ghibli anime illustration, tense prolog card. Inside an Indonesian angkot (minibus), Rara —
black bob hair with bangs, purple turtleneck sweater, long navy pleated skirt reaching mid-calf, brown
backpack — sits holding her smartphone which shows an unread message from an unknown number, while in
the soft-blurred background a shadowy adult man (faceless silhouette, non-graphic) behaves suspiciously,
watching her. Rara's expression: wary and uneasy. Warm but subtly tense interior light through the
windows. Antagonist stays a distant unfriendly silhouette. Painterly Ghibli depth of field. Cinematic
16:9, child-safe.
```

### 0.3 — Slide 3: Yang Perlu Rara Tahu

> **Pembicara:** Slide 3 — Judul: _"Yang Perlu Rara Tahu"_
> **Teks:** _"● Tubuhmu = milikmu! Nggak ada yang boleh sembarangan. ● Area privat NGGAK BOLEH disentuh orang lain. ● Ada pesan mencurigakan di HP? Jangan balas — langsung lapor ke orang tua!"_

```
Studio Ghibli anime illustration, warm educational guidance card. Rara — 13-year-old Indonesian girl,
black straight bob hair with bangs, purple turtleneck sweater, long navy pleated skirt reaching mid-calf,
brown backpack — stands confidently facing the viewer with a calm, empowered, reassuring expression,
one hand over her heart and the other raised in a gentle protective gesture. Soft warm glow, clean
bright background with subtle friendly safety motifs (a shield-like light, a phone with a STOP feel),
no graphic content. A tone of self-protection and courage. Painterly Ghibli cel-shading, hopeful uplifting
mood. Cinematic 16:9, child-safe.
```

---

## 1. Narasi Awal — `Day2NarasiAwal.cs`

### 1.1 — Rara semangat menuju halte

> **Pembicara:** Rara
> **Teks:** _"Bismillah, aku pasti bisa! Haltenya udah dekat — ayo cepat!"_

```
Studio Ghibli anime illustration. Rara — 13-year-old Indonesian girl, black straight bob hair with
bangs, purple turtleneck sweater, long navy-blue pleated skirt reaching mid-calf, brown leather
backpack, white socks, black mary jane shoes — walking briskly and cheerfully down a sunlit Indonesian
kampung alley in the early morning. Clean tidy cement path with no litter, lush green potted plants and
leafy trees, fresh dewy grass, old brick and plaster houses with terracotta roofs, neat bamboo fences,
coconut palms lining the way, crisp clear blue morning sky, low warm sunlight casting long
soft shadows. Her expression: bright, hopeful, full of determination, one hand on her backpack strap.
Painterly Ghibli cel-shading, soft brushstroke golden-hour sky. Cinematic 16:9, child-safe.
```

### 1.2 — Rara waspada jalan sepi

> **Pembicara:** Rara
> **Teks:** _"Tapi… kok jalan ini sepi banget ya? Aku harus tetap waspada."_

```
Studio Ghibli anime illustration. Rara — black bob hair with bangs, purple turtleneck sweater, long
navy pleated skirt (mid-calf), brown backpack — pausing mid-step in a quiet, clean and lush green
Indonesian kampung alley in the early morning, glancing cautiously over her shoulder. The tidy lane is
deserted, leafy trees and fresh plants line the way, long morning shadows stretch across the spotless
cement path, only birds and rustling leaves. Her expression: alert, slightly wary but composed. Soft
golden-hour morning light, faint mist in the background, painterly Ghibli depth of field.
Cinematic 16:9, child-safe.
```

---

## 2. Halte — `HalteDialog.cs`

### 2.1 — Narasi: tiba di halte ramai

> **Pembicara:** Narasi
> **Teks:** _"Pagi itu Rara sampai di halte yang cukup ramai. Beberapa orang ikut menunggu angkot jurusan sekolah."_

```
Studio Ghibli anime illustration. Wide shot of a modest Indonesian roadside bus stop (halte) in the
morning. Rara — 13-year-old girl, black bob hair, purple turtleneck sweater, long navy pleated skirt
reaching mid-calf, brown leather backpack — stands among a few other commuters waiting for an angkot
(minibus). Warm clear morning sun, leafy green palm trees and clean brick shophouses behind, tidy
planters, a well-kept road with painted lines, fresh and airy atmosphere.
Her expression calm but observant. Painterly Ghibli cel-shading, soft golden light, gentle background
blur on the crowd. Cinematic 16:9, child-safe.
```

### 2.2 — Narasi: menunggu, melihat jam

> **Pembicara:** Narasi
> **Teks:** _"Rara berdiri di pinggir sambil sesekali melihat jam. Angkotnya belum datang juga."_

```
Studio Ghibli anime illustration. Rara — black bob hair, purple turtleneck, long navy pleated skirt
(mid-calf), brown backpack — standing at the edge of an Indonesian bus stop, glancing at her wristwatch
with a slightly impatient expression. Clean leafy morning street behind her with green palm trees,
tidy brick buildings, fresh plants, a few blurred commuters. Warm low morning sunlight, long soft
shadows on the swept pavement. Painterly Ghibli depth of field.
Cinematic 16:9, child-safe.
```

### 2.3 — Narasi: pria asing memperhatikan dari jauh

> **Pembicara:** Narasi
> **Teks:** _"Dari tadi, ada seorang pria asing bertopi yang terus memperhatikan Rara dari kejauhan..."_

```
Studio Ghibli anime illustration. Rara — black bob hair, purple turtleneck, long navy pleated skirt
reaching mid-calf, brown backpack — stands at an Indonesian bus stop in the foreground, unaware. In the
soft-blurred background, a shadowy adult man in a cap stands apart, faceless and unsettling, watching her
direction. Warm morning light but a subtle tense atmosphere. The man is a distant non-graphic silhouette,
clearly separated from Rara. Painterly Ghibli, foreground sharp, background soft blur. Cinematic 16:9,
child-safe.
```

### 2.4 — Narasi: pria itu mendekat

> **Pembicara:** Narasi
> **Teks:** _"Pelan-pelan, pria itu mendekat dan berdiri tepat di sebelah Rara."_

```
Studio Ghibli anime illustration. At an Indonesian bus stop in the morning, Rara — black bob hair,
purple turtleneck, long navy pleated skirt (mid-calf), brown backpack — stands while a tall shadowy
adult man in a cap (faceless silhouette, non-graphic) has stepped up close beside her. Rara's body
language shifts subtly uncomfortable, leaning slightly away. Warm light but uneasy mood. Keep antagonist
as distant unfriendly silhouette, no explicit detail. Painterly Ghibli cel-shading. Cinematic 16:9,
child-safe.
```

### 2.5 — Pria Asing: menyapa

> **Pembicara:** Pria Asing
> **Teks:** _"Hai, cantik! Sendirian aja nih? Om dari tadi merhatiin kamu lho."_

```
Studio Ghibli anime illustration. Rara — black bob hair, purple turtleneck, long navy pleated skirt
reaching mid-calf, brown backpack — at an Indonesian bus stop, taking a small step back with a guarded,
uneasy expression as a shadowy adult man in a cap (faceless silhouette) leans toward her speaking. Rara
hugs her backpack strap defensively. Warm morning street behind, soft blur. Antagonist non-graphic and
distant in feel. Painterly Ghibli, tense but child-safe mood. Cinematic 16:9.
```

### 2.6 — Pria Asing: menawarkan tumpangan

> **Pembicara:** Pria Asing
> **Teks:** _"Mau ke sekolah ya? Om kebetulan searah. Daripada nunggu angkot lama, bareng om aja yuk — gratis kok."_

```
Studio Ghibli anime illustration. Rara — black bob hair, purple turtleneck, long navy pleated skirt
(mid-calf), brown backpack — stands firm at an Indonesian bus stop, frowning and shaking her head
slightly, refusing. A shadowy adult man in a cap (faceless silhouette) gestures invitingly toward a road,
offering a ride. Rara's expression: cautious refusal, alert. Warm morning light, palm trees behind.
Antagonist stays a non-graphic distant figure. Painterly Ghibli cel-shading. Cinematic 16:9, child-safe.
```

### 2.7 — Pria Asing: minta nomor WA & rahasia

> **Pembicara:** Pria Asing
> **Teks:** _"Eh, WA kamu berapa? Nanti om anter pulang sekolah ya. Rahasia aja, nggak usah bilang siapa-siapa."_

```
Studio Ghibli anime illustration. Rara — black bob hair, purple turtleneck, long navy pleated skirt
reaching mid-calf, brown backpack — at an Indonesian bus stop, clearly uncomfortable, taking a protective
step away with both hands gripping her backpack straps, eyes narrowed in distrust. A shadowy adult man in
a cap (faceless silhouette) holds out a phone gesture asking for her number. The 'secret' request makes
the mood quietly tense. Warm morning street soft-blurred. Antagonist non-graphic and distant. Painterly
Ghibli depth of field. Cinematic 16:9, child-safe.
```

### 2.8 — Reaksi AMAN: Rara menolak tegas, orang sekitar menoleh

> **Pembicara:** Reaksi AMAN
> **Teks:** _"✓ BAGUS, RA! Kamu menolak tegas & bersuara keras minta tolong. Orang-orang di halte langsung menoleh dan ibu-ibu menghampirimu. Pria itu salah tingkah lalu pergi. Ingat: nomor HP/WA itu DATA PRIBADI — jangan diberi ke orang asing!"_

```
Studio Ghibli anime illustration. At a clean Indonesian roadside bus stop in the morning, Rara — black
bob hair with bangs, purple turtleneck sweater, long navy pleated skirt reaching mid-calf, brown leather
backpack — stands brave and firm with one hand raised in a strong STOP gesture, mouth open as if calling
for help loudly. Nearby commuters, especially a few concerned women, turn toward her and move closer to
support her, while the shadowy adult man in a cap (faceless silhouette, non-graphic) recoils awkwardly
and begins backing away. Warm golden morning light, leafy trees, tidy halte sign, clean road, uplifting
protective atmosphere. Rara's expression: courageous, assertive, empowered. Painterly Ghibli cel-shading,
heroic child-safe tone. Cinematic 16:9.
```

### 2.9 — Reaksi RAGU: Rara ragu, pria makin memaksa

> **Pembicara:** Reaksi RAGU
> **Teks:** _"⚠ Kamu ragu-ragu menjawab. Pria itu makin maju dan terus memaksa minta nomormu. Untung angkot keburu datang dan kamu cepat naik. Lain kali, langsung TEGAS tolak ya!"_

```
Studio Ghibli anime illustration. At an Indonesian bus stop in the morning, Rara — black bob hair with
bangs, purple turtleneck sweater, long navy pleated skirt reaching mid-calf, brown leather backpack —
looks hesitant and uneasy, shoulders slightly drawn in, as a shadowy adult man in a cap (faceless
silhouette, non-graphic) leans in more insistently asking for her number. Rara's body language shows
uncertainty and discomfort, one foot subtly stepping back. In the background an angkot is arriving at the
curb, creating a chance to escape. Warm morning light remains, but the mood is tense and cautionary.
Leafy clean street, tidy halte, child-safe painterly Ghibli style. Cinematic 16:9.
```

### 2.10 — Reaksi BAHAYA: memberi nomor, situasi jadi berisiko

> **Pembicara:** Reaksi BAHAYA
> **Teks:** _"✖ GAWAT! Kamu memberi nomor WA-mu ke orang asing. Malamnya HP-mu dibanjiri chat aneh dari pria itu. Kamu kehilangan 1 nyawa. Ingat: kasih sayang & hadiah dari orang asing = RED FLAG grooming!"_

```
Studio Ghibli anime illustration. At a clean Indonesian roadside bus stop in the morning, Rara — black
bob hair with bangs, purple turtleneck sweater, long navy pleated skirt reaching mid-calf, brown leather
backpack — looks worried and regretful after reluctantly showing or handing over her phone contact to a
shadowy adult man in a cap (faceless silhouette, non-graphic) standing too close. The man's posture feels
intrusive and wrong, while Rara's expression shows immediate discomfort and a sinking realization that she
made an unsafe choice. Keep everything child-safe and non-explicit: no graphic detail, just emotional
tension and warning. Warm morning light, tidy halte, leafy street, painterly Ghibli cel-shading.
Cinematic 16:9.
```

---

### 2.11 — Hasil AMAN (gambar terpasang): Rara duduk sendirian di dalam angkot

> **Pembicara:** Reaksi AMAN — _isi gambar yang terpasang_
> **Teks rujukan:** _"Orang-orang di halte menoleh, ibu-ibu menghampiri, pria itu pergi"_ → Rara naik angkot dengan aman.

```
Studio Ghibli anime illustration. Interior of a clean Indonesian angkot (public minibus) in the bright
clean daytime. Rara — black bob hair with bangs, purple turtleneck sweater, long navy pleated skirt
reaching mid-calf, brown leather backpack on her lap — sits ALONE on the bench seat, calm and relieved,
a small safe smile, looking out the window. The angkot is tidy and well-kept, warm daylight pouring
through clean windows showing leafy green street outside. Safe, peaceful, reassuring mood. Painterly
Ghibli cel-shading, child-safe. Cinematic 16:9.
```

### 2.12 — Hasil RAGU (gambar terpasang): Rara duduk aman diapit 2 ibu di angkot

> **Pembicara:** Reaksi RAGU — _isi gambar yang terpasang_
> **Teks rujukan:** _"Pria makin memaksa minta nomor... untung angkot datang dan naik"_ → Rara aman diapit 2 ibu penumpang.

```
Studio Ghibli anime illustration. Interior of a clean Indonesian angkot (public minibus) in the bright
daytime. Rara — black bob hair with bangs, purple turtleneck sweater, long navy pleated skirt reaching
mid-calf, brown leather backpack on her lap — sits SAFELY FLANKED BY TWO kind older Indonesian women
(ibu-ibu) on either side, looking a little relieved but still a bit shaken. The two motherly passengers
sit close protectively, friendly and caring. Tidy well-kept angkot interior, warm daylight through clean
windows, leafy green street outside. Reassuring but slightly cautionary mood. Painterly Ghibli cel-shading,
child-safe. Cinematic 16:9.
```

### 2.13 — Hasil BAHAYA (gambar terpasang): Rara duduk di angkot (siang hari)

> **Pembicara:** Reaksi BAHAYA — _isi gambar yang terpasang_
> **Teks rujukan:** _"GAWAT! beri nomor WA → malamnya HP dibanjiri chat aneh"_ → Rara naik angkot, tapi terganggu pikirannya.

```
Studio Ghibli anime illustration. Interior of a clean Indonesian angkot (public minibus) in the bright
daytime. Rara — black bob hair with bangs, purple turtleneck sweater, long navy pleated skirt reaching
mid-calf, brown leather backpack on her lap — sits on the bench seat looking worried and uneasy, glancing
down at her phone with a troubled expression, sensing she made an unsafe choice. Keep it child-safe and
non-graphic: only emotional unease, no scary detail. Tidy angkot interior, warm daylight through clean
windows, leafy street outside. Cautionary, slightly anxious mood. Painterly Ghibli cel-shading, child-safe.
Cinematic 16:9.
```

---

## 3. Sentuh di Angkot — `AngkotSentuhScene.cs`

### 3.1 — Narasi AMAN: duduk dekat pintu

> **Pembicara:** Narasi
> **Teks:** _"Rara duduk tepat di belakang Pak Supir, dekat pintu. Dari sini ia bisa melihat seluruh isi angkot."_

```
Studio Ghibli anime illustration. Interior of an Indonesian angkot (minibus). Rara — black bob hair,
purple turtleneck, long navy pleated skirt reaching mid-calf, brown backpack on lap — sits confidently
on the bench right behind the driver, near the open door, alert and aware of her surroundings. Warm
morning light pours through the open side, worn vinyl seats, the driver's silhouette up front. Her
expression: composed, watchful, safe. Painterly Ghibli cel-shading, cozy warm interior tones. Cinematic
16:9, child-safe.
```

### 3.2 — Narasi AMAN: pria asing pindah merapat

> **Pembicara:** Narasi
> **Teks:** _"Seorang pria asing yang tadi duduk di bangku belakang (bukan yang di halte) berdiri dan pindah, ikut duduk merapat di sebelah Rara."_

```
Studio Ghibli anime illustration. Inside an Indonesian angkot minibus. Rara — black bob hair, purple
turtleneck, long navy pleated skirt (mid-calf), brown backpack — sits near the door while a shadowy adult
man (faceless silhouette, non-graphic) moves from the back bench to sit too close beside her. Rara leans
toward the door, body alert. Warm interior light, driver visible up front for safety. Antagonist stays a
distant unfriendly silhouette. Painterly Ghibli, mildly tense but child-safe. Cinematic 16:9.
```

### 3.3 — Pria Asing AMAN: basa-basi

> **Pembicara:** Pria Asing
> **Teks:** _"Sekolah di mana, dek? Sini deket om aja, biar nggak desak-desakan."_

```
Studio Ghibli anime illustration. Angkot interior. Rara — black bob hair, purple turtleneck, long navy
pleated skirt reaching mid-calf, brown backpack — sits near the door with a guarded, polite-but-firm
expression as a shadowy adult man (faceless silhouette) beside her leans in talking. Rara keeps distance,
glancing toward the driver up front. Warm minibus light, open door letting in morning sun. Antagonist
non-graphic. Painterly Ghibli cel-shading. Cinematic 16:9, child-safe.
```

### 3.4 — Narasi AMAN: tangan menyentuh bahu

> **Pembicara:** Narasi
> **Teks:** _"Tangan pria itu menyentuh bahu Rara! Tapi Pak Supir ada tepat di depan — Rara tahu ia bisa segera minta tolong."_

```
Studio Ghibli anime illustration. Angkot interior, tense but child-safe moment. Rara — black bob hair,
purple turtleneck, long navy pleated skirt (mid-calf), brown backpack — flinches and turns sharply, eyes
wide and alert, ready to call for help, as a shadowy adult man's hand (silhouette, non-graphic, only a
dark hand suggested at her shoulder) intrudes. The driver is clearly visible up front near the open door —
a beacon of safety. Rara's expression: alarmed but empowered. Warm interior light. Painterly Ghibli,
NON-explicit, child-safe. Cinematic 16:9.
```

### 3.5 — Narasi RAGU: berdesakan di tengah

> **Pembicara:** Narasi
> **Teks:** _"Rara duduk berdesakan di tengah, terhimpit di antara ibu-ibu yang membawa belanjaan."_

```
Studio Ghibli anime illustration. Crowded Indonesian angkot interior. Rara — black bob hair, purple
turtleneck, long navy pleated skirt reaching mid-calf, brown backpack on lap — squeezed in the middle
bench between friendly older women carrying market bags. Warm cluttered cozy minibus, morning light
through windows. Her expression: a little cramped and uneasy, looking around. Painterly Ghibli cel-shading,
busy but warm tones. Cinematic 16:9, child-safe.
```

### 3.6 — Narasi RAGU: pria asing menyusup duduk

> **Pembicara:** Narasi
> **Teks:** _"Dari bangku belakang, seorang pria asing (bukan yang di halte) menyusup dan memaksakan diri duduk di sela sempit, merapat ke sisi Rara."_

```
Studio Ghibli anime illustration. Crowded angkot interior. Rara — black bob hair, purple turtleneck,
long navy pleated skirt (mid-calf), brown backpack — wedged in the middle bench as a shadowy adult man
(faceless silhouette, non-graphic) forces himself into the narrow gap beside her. Rara stiffens, pressed
between the women and the intruder, uneasy. Warm crowded minibus light. Antagonist stays distant
unfriendly silhouette. Painterly Ghibli, tense but child-safe. Cinematic 16:9.
```

### 3.7 — Pria Asing RAGU: minta geser

> **Pembicara:** Pria Asing
> **Teks:** _"Geser dikit dong, dek. Biar om bisa duduk dekat kamu."_

```
Studio Ghibli anime illustration. Angkot interior. Rara — black bob hair, purple turtleneck, long navy
pleated skirt reaching mid-calf, brown backpack — squeezed on the middle bench, leaning away with a
discomfited expression as a shadowy adult man (faceless silhouette) beside her presses closer talking.
Other passengers (women with bags) around. Warm minibus light. Antagonist non-graphic. Painterly Ghibli
cel-shading. Cinematic 16:9, child-safe.
```

### 3.8 — Narasi RAGU: tangan menyentuh bahu

> **Pembicara:** Narasi
> **Teks:** _"Tangan pria itu menyentuh bahu Rara! Ibu-ibu di sekitar mulai melirik, tapi Rara terjepit dan susah bergerak."_

```
Studio Ghibli anime illustration. Crowded angkot, tense child-safe moment. Rara — black bob hair, purple
turtleneck, long navy pleated skirt (mid-calf), brown backpack — flinches with wide alarmed eyes as a
shadowy man's hand (silhouette, non-graphic) reaches her shoulder; she is wedged tight between passengers.
Nearby women glance over with concern. Warm cluttered interior. Antagonist non-graphic, NON-explicit.
Painterly Ghibli. Cinematic 16:9, child-safe.
```

### 3.9 — Narasi BAHAYA: pojok belakang sepi

> **Pembicara:** Narasi
> **Teks:** _"Rara duduk sendirian di pojok belakang yang sepi. Tak ada penumpang lain di dekatnya."_

```
Studio Ghibli anime illustration. Empty rear corner of an Indonesian angkot. Rara — black bob hair,
purple turtleneck, long navy pleated skirt reaching mid-calf, brown backpack on lap — sits alone in the
back bench, isolated, glancing around an empty minibus. Dimmer cooler light at the back versus warm light
near the distant front door. Her expression: slightly uneasy, vulnerable but composed. Painterly Ghibli
cel-shading, gentle isolation mood. Cinematic 16:9, child-safe.
```

### 3.10 — Narasi BAHAYA: pria asing duduk merapat

> **Pembicara:** Narasi
> **Teks:** _"Pria asing yang sedari tadi memperhatikannya (bukan yang di halte) kini duduk persis di sebelah Rara. Tak ada siapa pun yang melihat."_

```
Studio Ghibli anime illustration. Empty rear of an angkot. Rara — black bob hair, purple turtleneck,
long navy pleated skirt (mid-calf), brown backpack — sits in the isolated back corner as a shadowy adult
man (faceless silhouette, non-graphic) sits right beside her with no other passengers around. Rara tenses,
edging toward the window, alert. Cool dim back-of-bus light, far-off warm front door. Antagonist distant
unfriendly silhouette. Painterly Ghibli, tense but child-safe. Cinematic 16:9.
```

### 3.11 — Pria Asing BAHAYA: membujuk

> **Pembicara:** Pria Asing
> **Teks:** _"Tenang, dek... om temani kamu sampai sekolah ya. Deket-deket om aja."_

```
Studio Ghibli anime illustration. Isolated rear of an angkot. Rara — black bob hair, purple turtleneck,
long navy pleated skirt reaching mid-calf, brown backpack — pressed toward the window with a wary,
distrustful expression as a shadowy adult man (faceless silhouette) beside her speaks in a coaxing manner.
Empty minibus, cool dim light. Antagonist non-graphic and distant in feel. Painterly Ghibli cel-shading.
Cinematic 16:9, child-safe.
```

### 3.12 — Narasi BAHAYA: tangan menyentuh bahu

> **Pembicara:** Narasi
> **Teks:** _"Tangan pria itu langsung menyentuh bahu Rara! Pojok ini jauh dari Pak Supir — Rara harus berani bertindak sendiri."_

```
Studio Ghibli anime illustration. Isolated angkot corner, tense child-safe moment. Rara — black bob hair,
purple turtleneck, long navy pleated skirt (mid-calf), brown backpack — recoils with alarmed determined
eyes, raising a hand in a STOP gesture, as a shadowy man's hand (silhouette, non-graphic) intrudes at her
shoulder. The driver is far away at the front. Rara must be brave alone — her expression shifts from fear
to courage. Cool dim light. Antagonist NON-explicit, non-graphic. Painterly Ghibli. Cinematic 16:9,
child-safe.
```

### 3.13 — Lapor: Rara pindah kursi (kata sakti PERGI)

> **Pembicara:** Narasi
> **Teks:** _"Rara langsung berdiri dan PINDAH ke kursi lebih depan, menjauh dari pria itu. (Itu kata sakti kedua: PERGI.)"_

```
Studio Ghibli anime illustration. Angkot interior, empowering moment. Rara — black bob hair, purple
turtleneck, long navy pleated skirt reaching mid-calf, brown backpack — stands up decisively and moves to
a front bench near the driver, away from a shadowy man left behind in the back (faceless silhouette,
distant). Rara's expression: brave, resolved, taking control. Warm light grows near the front door. A soft
glowing word-feel of 'PERGI / GO'. Painterly Ghibli cel-shading, hopeful tone. Cinematic 16:9, child-safe.
```

### 3.14 — Lapor: Rara tetap siaga

> **Pembicara:** Rara
> **Teks:** _"Aku sudah bersuara dan menjauh. Tapi dia masih satu angkot denganku — aku harus tetap siaga sampai turun."_

```
Studio Ghibli anime illustration. Rara — black bob hair, purple turtleneck, long navy pleated skirt
(mid-calf), brown backpack — sits on a front angkot bench near the driver, alert and watchful, glancing
back cautiously. Warm front-of-bus light, the distant back where a faint silhouette remains. Her
expression: composed but still vigilant, hands ready on her bag. Painterly Ghibli depth of field.
Cinematic 16:9, child-safe.
```

---

## 4. Quiz Zona Tubuh — Narasi Pembungkus — `ZonaTubuhQuiz.cs`

### 4.1 — Narasi intro: angkot melaju, Rara menenangkan diri

> **Pembicara:** Narasi
> **Teks:** _"Angkot terus melaju. Setelah kejadian tadi, Rara sudah pindah ke kursi lebih depan dan mencoba menenangkan napasnya."_

```
Studio Ghibli anime illustration. Inside a moving Indonesian angkot. Rara — black bob hair, purple
turtleneck, long navy pleated skirt reaching mid-calf, brown backpack on lap — sits on a front bench
taking a calming breath, hand on her chest, eyes softly closed or downcast, recovering. Warm morning
light streams through the windows, blurred street rushing past outside (motion blur). Gentle relieved
mood. Painterly Ghibli cel-shading. Cinematic 16:9, child-safe.
```

### 4.2 — Rara: masih berdebar, ingin mengalihkan pikiran

> **Pembicara:** Rara
> **Teks:** _"Untung aku tadi berani bersuara dan menjauh… tapi jantungku masih berdebar. Aku perlu mengalihkan pikiran sebentar."_

```
Studio Ghibli anime illustration. Rara — black bob hair, purple turtleneck, long navy pleated skirt
(mid-calf), brown backpack — sits in a front angkot seat, a hand lightly over her heart, expression a mix
of relief and lingering nervousness, gazing out the window. Warm morning light, soft-blur street outside.
Painterly Ghibli, tender introspective mood. Cinematic 16:9, child-safe.
```

### 4.3 — Narasi: Rara membuka buku catatan PR Kesehatan

> **Pembicara:** Narasi
> **Teks:** _"Rara mengeluarkan buku catatan PR Kesehatan dari tas. Kebetulan bab terakhirnya justru soal ini: \"Kenali Batas Tubuhmu — Mana yang Boleh, Mana yang Tidak.\""_

```
Studio Ghibli anime illustration. Rara — black bob hair, purple turtleneck, long navy pleated skirt
reaching mid-calf, brown backpack — sits in an angkot seat opening a school notebook on her lap, reading
a chapter titled (text abstracted) about knowing your body boundaries. Her expression: curious, focused,
a little reassured. Warm window light falls on the pages. Painterly Ghibli depth of field, cozy interior.
Cinematic 16:9, child-safe.
```

### 4.4 — Rara: bertekad belajar batas tubuh

> **Pembicara:** Rara
> **Teks:** _"Justru sekarang aku makin paham kenapa bab ini penting. Ayo aku pelajari baik-baik — siapa yang BOLEH dan TIDAK BOLEH menyentuhku."_

```
Studio Ghibli anime illustration. Rara — black bob hair, purple turtleneck, long navy pleated skirt
(mid-calf), brown backpack — sits in an angkot, holding her notebook with a newly determined, empowered
expression, a small confident smile. Soft warm light, blurred street outside. A feeling of learning and
self-protection. Painterly Ghibli cel-shading, hopeful tone. Cinematic 16:9, child-safe.
```

### 4.5 — Narasi outro: pria belum menyerah

> **Pembicara:** Narasi
> **Teks:** _"Angkot masih melaju menuju sekolah. Rara menutup buku catatannya — tapi dari sudut matanya, pria tadi belum menyerah."_

```
Studio Ghibli anime illustration. Angkot interior. Rara — black bob hair, purple turtleneck, long navy
pleated skirt reaching mid-calf, brown backpack — closes her notebook, but her side-eye catches a shadowy
adult man (faceless silhouette, distant in the bus) still lurking, not giving up. Rara's expression shifts
back to wary alertness. Warm light up front, cooler tone toward the back. Antagonist non-graphic, distant.
Painterly Ghibli. Cinematic 16:9, child-safe.
```

### 4.6 — Rara: tetap waspada sampai turun

> **Pembicara:** Rara
> **Teks:** _"Dia… masih terus melirik ke arahku. Aku harus tetap waspada sampai turun nanti."_

```
Studio Ghibli anime illustration. Rara — black bob hair, purple turtleneck, long navy pleated skirt
(mid-calf), brown backpack — sits tense and alert in an angkot front seat, gripping her bag, glancing
warily toward a faint distant silhouette at the back of the bus. Warm yet uneasy interior light. Her
expression: vigilant, brave. Antagonist non-graphic. Painterly Ghibli cel-shading. Cinematic 16:9,
child-safe.
```

### 4.7 — Narasi: pria menggeser makin merapat

> **Pembicara:** Narasi
> **Teks:** _"Tiba-tiba pria itu kembali menggeser duduknya, makin merapat ke arah Rara!"_

```
Studio Ghibli anime illustration. Angkot interior, sudden tense child-safe beat. Rara — black bob hair,
purple turtleneck, long navy pleated skirt reaching mid-calf, brown backpack — startles as a shadowy
adult man (faceless silhouette, non-graphic) shifts seats again, edging closer toward her. Rara braces,
eyes wide, ready to act. Warm interior light with rising tension. Antagonist distant unfriendly
silhouette. Painterly Ghibli. Cinematic 16:9, child-safe.
```

### 4.8 — Rara: kata sakti CERITA, minta tolong

> **Pembicara:** Rara
> **Teks:** _"Cukup! Kalau aku merasa tidak aman, aku harus CERITA — minta tolong orang dewasa. Pak Supir ada di depan!"_

```
Studio Ghibli anime illustration. Empowering moment in an angkot. Rara — black bob hair, purple turtleneck,
long navy pleated skirt (mid-calf), brown backpack — stands up brave and resolute, one arm pointing toward
the driver at the front, mouth open calling for help, eyes blazing with courage. A soft glowing word-feel
of 'CERITA / TELL'. The driver turns to look. Warm hopeful light surges toward the front. Painterly Ghibli
cel-shading, heroic child-safe tone. Cinematic 16:9.
```

---

## 5. Lapor/Teriak — Narasi Pembuka — `LaporTeriakButton.cs`

### 5.1 — Narasi: Rara menyimpan HP

> **Pembicara:** Narasi
> **Teks:** _"Rara memasukkan kembali HP-nya ke saku. Tapi suasana di dalam angkot terasa berubah."_

```
Studio Ghibli anime illustration. Angkot interior. Rara — black bob hair, purple turtleneck, long navy
pleated skirt reaching mid-calf, brown backpack — slips her smartphone back into her pocket, sensing a
subtle change in the air, expression turning quietly alert. Warm but slightly tense minibus light,
half-empty seats. Painterly Ghibli cel-shading, building unease. Cinematic 16:9, child-safe.
```

### 5.2 — Narasi: penumpang turun, bangku kosong

> **Pembicara:** Narasi
> **Teks:** _"Beberapa penumpang turun di perempatan. Kini bangku tepat di sebelah Rara kosong."_

```
Studio Ghibli anime illustration. Angkot interior at a crossroads. Rara — black bob hair, purple
turtleneck, long navy pleated skirt (mid-calf), brown backpack — sits as a few passengers step off,
leaving the seat right beside her empty and exposed. Warm clear morning light, clean leafy green street
at the crossroads blurred outside. Her expression: noticing the empty seat with quiet apprehension.
Painterly Ghibli depth of field. Cinematic 16:9, child-safe.
```

### 5.3 — Pria Asing: pindah ke sebelah Rara

> **Pembicara:** Pria Asing
> **Teks:** _"Wah, kosong nih. Om pindah ke sini aja ya, biar lebih enak ngobrolnya."_

```
Studio Ghibli anime illustration. Angkot interior. A shadowy adult man (faceless silhouette, non-graphic)
moves to take the empty seat beside Rara — black bob hair, purple turtleneck, long navy pleated skirt
reaching mid-calf, brown backpack — who leans away with a guarded, uneasy expression. Warm interior light,
fewer passengers around. Antagonist stays distant unfriendly silhouette. Painterly Ghibli cel-shading.
Cinematic 16:9, child-safe.
```

### 5.4 — Narasi: pria menyentuh tadi, kini makin dekat

> **Pembicara:** Narasi
> **Teks:** _"Pria yang tadi menyentuh bahunya itu menggeser duduknya — makin merapat ke arah Rara."_

```
Studio Ghibli anime illustration. Angkot interior, tense child-safe beat. Rara — black bob hair, purple
turtleneck, long navy pleated skirt (mid-calf), brown backpack — presses toward the window as a shadowy
adult man (faceless silhouette, non-graphic) slides closer beside her. Rara's body language: protective,
alert, edging away. Warm but tense light. Antagonist non-graphic and distant in feel. Painterly Ghibli.
Cinematic 16:9, child-safe.
```

### 5.5 — Rara: merasa ada yang tidak beres

> **Pembicara:** Rara
> **Teks:** _"Kenapa dia harus pindah ke sebelahku? Padahal masih banyak bangku lain yang kosong..."_

```
Studio Ghibli anime illustration. Close-up. Rara — black bob hair, purple turtleneck, long navy pleated
skirt reaching mid-calf, brown backpack — in an angkot, brow furrowed in suspicious thought, glancing
sideways at empty seats elsewhere, sensing something is wrong. Warm interior light, soft-blurred shadowy
figure beside her. Her expression: questioning, wary, intelligent. Painterly Ghibli depth of field.
Cinematic 16:9, child-safe.
```

### 5.6 — Narasi: kata sakti CERITA, minta tolong Pak Supir

> **Pembicara:** Narasi
> **Teks:** _"Hati kecil Rara berkata ada yang tidak beres. Inilah saatnya kata sakti ketiga: CERITA — minta tolong Pak Supir!"_

```
Studio Ghibli anime illustration. Empowering climactic moment in an angkot. Rara — black bob hair, purple
turtleneck, long navy pleated skirt (mid-calf), brown backpack — rises bravely, calling out toward the
driver at the front, one hand raised, eyes determined and courageous. A warm glowing word-feel of 'CERITA
/ TELL' radiates. The driver and other passengers turn to help; a shadowy figure shrinks back. Warm
hopeful light floods the scene. Painterly Ghibli cel-shading, heroic child-safe tone. Cinematic 16:9.
```

---

## 6. Pilih Kursi Angkot — `AngkotSeatPicker.cs`

### 6.1 — Judul layar: pilih tempat duduk

> **Pembicara:** Narasi (judul layar)
> **Teks:** _"Angkot datang. Di dalam sudah ada beberapa penumpang — termasuk seorang pria yang melirik ke arahmu. Pilih tempat dudukmu:"_

```
Studio Ghibli anime illustration. An Indonesian angkot (minibus) has just pulled up; the side door is
open revealing the interior. Rara — 13-year-old Indonesian girl, black straight bob hair with bangs,
purple turtleneck sweater, long navy-blue pleated skirt reaching mid-calf, brown leather backpack,
white socks, black mary jane shoes — stands at the door about to choose a seat, looking inside
thoughtfully. A few passengers are already seated, and a shadowy adult man (faceless silhouette,
non-graphic) in the back glances toward her. Warm clean morning light, cozy worn interior. Rara's
expression: careful, deciding. Antagonist stays a distant unfriendly silhouette. Painterly Ghibli
cel-shading. Cinematic 16:9, child-safe.
```

### 6.2 — Reaksi AMAN: duduk dekat pintu & supir

> **Pembicara:** Narasi (reaksi — pilihan AMAN)
> **Teks:** _"✓ Pintar! Kamu duduk dekat Pak Supir & pintu. Ini posisi paling aman — gampang minta tolong & cepat turun kalau ada apa-apa. Angkot pun mulai berjalan menuju sekolah..."_

```
Studio Ghibli anime illustration. Interior of an Indonesian angkot, warm and reassuring. Rara — black
bob hair with bangs, purple turtleneck sweater, long navy pleated skirt reaching mid-calf, brown
backpack on lap — sits confidently on the bench right behind the driver, near the open door, with a
calm, smart, satisfied expression and a small confident smile. Bright warm morning light pours through
the open side, the friendly driver visible up front. A subtle safe, positive glow. Painterly Ghibli
cel-shading, cozy warm tones. Cinematic 16:9, child-safe.
```

### 6.3 — Reaksi RAGU: duduk di tengah berdesakan

> **Pembicara:** Narasi (reaksi — pilihan RAGU)
> **Teks:** _"⚠ Kamu duduk di tengah, terhimpit di antara penumpang. Ramai memang — tapi kamu susah bergerak kalau terjadi sesuatu. Angkot pun mulai berjalan menuju sekolah..."_

```
Studio Ghibli anime illustration. Crowded Indonesian angkot interior. Rara — black bob hair with bangs,
purple turtleneck sweater, long navy pleated skirt reaching mid-calf, brown backpack on lap — squeezed
in the middle bench between friendly older women carrying market bags, looking a little cramped and
uncertain, glancing around. Warm cluttered cozy minibus, morning light through the windows. A mood of
mild unease — safe but hard to move. Painterly Ghibli cel-shading, busy warm tones. Cinematic 16:9,
child-safe.
```

### 6.4 — Reaksi BAHAYA: duduk di pojok belakang sepi

> **Pembicara:** Narasi (reaksi — pilihan BAHAYA)
> **Teks:** _"✖ Kamu memilih pojok belakang yang sepi & gelap. Jauh dari Pak Supir — posisi ini paling rawan. Angkot pun mulai berjalan menuju sekolah..."_

```
Studio Ghibli anime illustration. Isolated rear corner of an Indonesian angkot. Rara — black bob hair
with bangs, purple turtleneck sweater, long navy pleated skirt reaching mid-calf, brown backpack on lap
— sits alone in the dim, quiet back bench, far from the driver, looking slightly vulnerable and uneasy
but composed, glancing around the empty space. Cooler dimmer light at the back versus warm light near
the distant front door. A subtle risky, lonely mood (still child-safe, non-graphic). Painterly Ghibli
cel-shading, gentle isolation tone. Cinematic 16:9, child-safe.
```

---

## Catatan Pemakaian

1. **Lampirkan gambar 1 (Rara berjalan di gang)** langsung ke ChatGPT sebagai referensi visual,
   lalu tulis: _"Pakai anak perempuan ini sebagai karakter Rara di semua gambar — wajah, rambut,
   sweater ungu, dan ROK NAVY PANJANG-nya harus sama persis."_ Ini cara paling ampuh menjaga konsistensi.
2. Tempel **Style & Character Lock** sekali di awal chat ChatGPT, tunggu konfirmasi.
3. Kirim prompt adegan **satu per satu** (jangan digabung) agar konsisten.
4. Jika Rara mulai berubah (rok jadi pendek / warna sweater geser), kirim ulang blok
   **Character Lock — Versi Inggris** lalu lanjutkan.
5. Untuk semua adegan "Pria Asing", pertahankan instruksi **siluet non-grafis & berjarak** —
   ini wajib agar tetap ramah anak (SMP) dan tidak eksplisit.

---

## 🗂️ Skema Nama File Latar Day 2 (untuk dimasukkan ke game)

> Generate gambar dari tiap prompt di atas, lalu **simpan dengan nama file persis** seperti tabel ini
> ke folder **`Assets/sprites/day 2/intro/`** (buat folder baru ini — meniru pola `Assets/sprites/day 3/intro/`).
> Format: PNG, Import Type **Sprite (2D and UI)**.
>
> Kolom **Slot Tujuan** menunjukkan field latar yang TERSEDIA SAAT INI di script Day 2 (latar **per-segmen**).
> ⚠️ Catatan penting: script Day 2 sekarang hanya punya **1 slot latar per segmen** (bukan per kotak dialog
> seperti Day 3). Jadi untuk tiap segmen, pilih **1 gambar paling mewakili** untuk dipasang ke slotnya.
> Kalau ingin latar **berbeda untuk SETIAP kotak dialog** (seperti Day 3), beri tahu saya — perlu menambah
> slot per-dialog di script-nya dulu (perubahan kode).

### Prolog Day 1 — `PrologScreen.cs` (bonus, di luar Day 2)

| Kode | Nama File                         | Slot Tujuan          |
| ---- | --------------------------------- | -------------------- |
| 00.1 | `day1_prolog_01_berangkat.png`    | PrologScreen slide 1 |
| 00.2 | `day1_prolog_02_kenali_batas.png` | PrologScreen slide 2 |
| 00.3 | `day1_prolog_03_panduan.png`      | PrologScreen slide 3 |

### 0. Prolog Day 2 — `Day2PrologScreen.cs`

| Kode | Nama File                            | Slot Tujuan              |
| ---- | ------------------------------------ | ------------------------ |
| 0.1  | `day2_prolog_01_halte.png`           | Day2PrologScreen slide 1 |
| 0.2  | `day2_prolog_02_batas_digital.png`   | Day2PrologScreen slide 2 |
| 0.3  | `day2_prolog_03_yang_perlu_tahu.png` | Day2PrologScreen slide 3 |

### 1. Narasi Awal — `Day2NarasiAwal.cs` → slot `bgIntroSprite`

| Kode | Nama File                   | Slot Tujuan                            |
| ---- | --------------------------- | -------------------------------------- |
| 1.1  | `day2_01_semangat_gang.png` | `Day2Controller.bgIntroSprite` (utama) |
| 1.2  | `day2_02_waspada_sepi.png`  | (alternatif intro)                     |

### 2. Halte — `HalteDialog.cs` → slot `bgHalteSprite`

| Kode | Nama File                         | Slot Tujuan                            |
| ---- | --------------------------------- | -------------------------------------- |
| 2.1  | `day2_03_halte_ramai.png`         | `Day2Controller.bgHalteSprite` (utama) |
| 2.2  | `day2_04_halte_lihat_jam.png`     | (halte)                                |
| 2.3  | `day2_05_pria_perhatikan.png`     | (halte)                                |
| 2.4  | `day2_06_pria_mendekat.png`       | (halte)                                |
| 2.5  | `day2_07_pria_menyapa.png`        | (halte)                                |
| 2.6  | `day2_08_pria_tumpangan.png`      | (halte)                                |
| 2.7  | `day2_09_pria_minta_wa.png`       | (halte)                                |
| 2.8  | `day2_42_halte_reaksi_aman.png`   | `HalteDialog.reaksiAman.latarSprite`   |
| 2.9  | `day2_43_halte_reaksi_ragu.png`   | `HalteDialog.reaksiRagu.latarSprite`   |
| 2.10 | `day2_44_halte_reaksi_bahaya.png` | `HalteDialog.reaksiBahaya.latarSprite` |
| 2.11 | `day2_45_angkot_hasil_aman.png`   | (gambar hasil AMAN — Rara sendiri)     |
| 2.12 | `day2_46_angkot_hasil_ragu.png`   | (gambar hasil RAGU — diapit 2 ibu)     |
| 2.13 | `day2_47_angkot_hasil_bahaya.png` | (gambar hasil BAHAYA — Rara cemas)     |

### 3. Sentuh di Angkot — `AngkotSentuhScene.cs` → slot `bgAngkotSprite`

| Kode | Nama File                         | Slot Tujuan                             |
| ---- | --------------------------------- | --------------------------------------- |
| 3.1  | `day2_10_aman_dekat_pintu.png`    | `Day2Controller.bgAngkotSprite` (utama) |
| 3.2  | `day2_11_aman_pria_merapat.png`   | (angkot — jalur AMAN)                   |
| 3.3  | `day2_12_aman_basabasi.png`       | (angkot — jalur AMAN)                   |
| 3.4  | `day2_13_aman_sentuh_bahu.png`    | (angkot — jalur AMAN)                   |
| 3.5  | `day2_14_ragu_berdesakan.png`     | (angkot — jalur RAGU)                   |
| 3.6  | `day2_15_ragu_pria_menyusup.png`  | (angkot — jalur RAGU)                   |
| 3.7  | `day2_16_ragu_minta_geser.png`    | (angkot — jalur RAGU)                   |
| 3.8  | `day2_17_ragu_sentuh_bahu.png`    | (angkot — jalur RAGU)                   |
| 3.9  | `day2_18_bahaya_pojok_sepi.png`   | (angkot — jalur BAHAYA)                 |
| 3.10 | `day2_19_bahaya_pria_merapat.png` | (angkot — jalur BAHAYA)                 |
| 3.11 | `day2_20_bahaya_membujuk.png`     | (angkot — jalur BAHAYA)                 |
| 3.12 | `day2_21_bahaya_sentuh_bahu.png`  | (angkot — jalur BAHAYA)                 |
| 3.13 | `day2_22_lapor_pindah_kursi.png`  | (angkot — LAPOR)                        |
| 3.14 | `day2_23_lapor_tetap_siaga.png`   | (angkot — LAPOR)                        |

### 4. Quiz Zona Tubuh (narasi) — `ZonaTubuhQuiz.cs` → slot `bgQuizSprite` / `narasiBgFullscreenSprite`

| Kode | Nama File                            | Slot Tujuan                           |
| ---- | ------------------------------------ | ------------------------------------- |
| 4.1  | `day2_24_quiz_intro_tenang.png`      | `Day2Controller.bgQuizSprite` (utama) |
| 4.2  | `day2_25_quiz_berdebar.png`          | (quiz narasi)                         |
| 4.3  | `day2_26_quiz_buka_buku.png`         | (quiz narasi)                         |
| 4.4  | `day2_27_quiz_bertekad.png`          | (quiz narasi)                         |
| 4.5  | `day2_28_quiz_pria_belum_nyerah.png` | (quiz narasi)                         |
| 4.6  | `day2_29_quiz_tetap_waspada.png`     | (quiz narasi)                         |
| 4.7  | `day2_30_quiz_pria_merapat.png`      | (quiz narasi)                         |
| 4.8  | `day2_31_quiz_cerita_tolong.png`     | (quiz narasi)                         |

### 5. Lapor/Teriak — `LaporTeriakButton.cs` → slot `bgLaporSprite` / `vnLatarSprite`

| Kode | Nama File                         | Slot Tujuan                            |
| ---- | --------------------------------- | -------------------------------------- |
| 5.1  | `day2_32_lapor_simpan_hp.png`     | `Day2Controller.bgLaporSprite` (utama) |
| 5.2  | `day2_33_lapor_bangku_kosong.png` | (lapor)                                |
| 5.3  | `day2_34_lapor_pria_pindah.png`   | (lapor)                                |
| 5.4  | `day2_35_lapor_pria_dekat.png`    | (lapor)                                |
| 5.5  | `day2_36_lapor_ada_ganjil.png`    | (lapor)                                |
| 5.6  | `day2_37_lapor_cerita_supir.png`  | (lapor)                                |

### 6. Pilih Kursi Angkot — `AngkotSeatPicker.cs`

| Kode | Nama File                  | Slot Tujuan       |
| ---- | -------------------------- | ----------------- |
| 6.1  | `day2_38_kursi_pilih.png`  | layar pilih kursi |
| 6.2  | `day2_39_kursi_aman.png`   | reaksi AMAN       |
| 6.3  | `day2_40_kursi_ragu.png`   | reaksi RAGU       |
| 6.4  | `day2_41_kursi_bahaya.png` | reaksi BAHAYA     |

### Langkah Memasukkan ke Game

1. Generate semua gambar, simpan dengan nama persis di atas ke `Assets/sprites/day 2/intro/`.
2. Di Unity, set tiap PNG: **Texture Type = Sprite (2D and UI)**, lalu Apply.
3. Buka scene `Gameplay`, pilih GameObject Day 2 (komponen `Day2Controller`).
4. Drag gambar **utama** tiap segmen ke slot per-fase: `bgIntroSprite`, `bgHalteSprite`,
   `bgAngkotSprite`, `bgQuizSprite`, `bgLaporSprite`.
5. Bila sudah ada filenya di folder, beri tahu saya — saya akan otomatis pasangkan ke slot di scene
   (seperti yang sudah dilakukan untuk Day 3).
