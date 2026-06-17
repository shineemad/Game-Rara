# Prompt ChatGPT — Visual Per-Dialog Day 3 (RARA: Jaga Dirimu!)

> Kumpulan **prompt image-generation siap tempel ke ChatGPT (DALL·E / GPT-4o)** untuk SETIAP dialog box Hari 3.
> Style mengacu gambar referensi: **Studio Ghibli painterly, Rara sweater ungu + rok navy, jalan kampung Indonesia, cahaya sinematik**.
> Sumber dialog: [Day3-Dialog-Ringkas.md](Day3-Dialog-Ringkas.md).

---

## 🔒 Style & Character Lock (tempel sekali di awal chat)

> Salin blok ini **lebih dulu** ke ChatGPT agar semua gambar berikutnya konsisten dengan style referensi.

```
Kamu adalah ilustrator game edukasi anak "RARA: Jaga Dirimu!".
Gambar SEMUA adegan dengan GAYA & KARAKTER yang KONSISTEN berikut:

GAYA VISUAL (WAJIB SAMA — acuan: Studio Ghibli path-walking scene):
- Studio Ghibli–inspired 2D anime illustration, hand-painted cinematic, soft cel-shading.
- Tekstur painterly halus: brushstroke lembut di langit, rerumputan, dan tembok.
- Pencahayaan: golden-hour hangat saat luar ruangan; cahaya lentera/hujan dramatis saat adegan menegangkan.
- Palet hangat & natural: terracotta, krem, hijau dedaunan tropis, langit biru-emas.
- Background kampung/sekolah Indonesia nyata: rumah bata merah, genteng tanah liat, pagar bambu,
  pot tanaman, pohon kelapa & pisang, gang bersemen, bayang-bayang pohon di tanah.
- Depth of field lembut: foreground tajam, background soft blur film camera.
- Komposisi sinematik 16:9, ruang udara di atas karakter untuk napas visual.
- Mood: ramah anak (game edukasi SMP), TIDAK gelap/eksplisit/menyeramkan.

KARAKTER RARA (WAJIB IDENTIK tiap gambar):
- Anak perempuan Indonesia ±13 tahun, kulit sawo matang hangat, ekspresi hidup & relatable.
- Rambut hitam lurus bob pendek berponi, mata cokelat hangat bulat.
- Sweater turtleneck UNGU, rok lipit BIRU DONGKER PANJANG sampai setengah betis (bukan selutut).
- Ransel kulit cokelat di punggung.
- Kaus kaki putih pendek, sepatu pantofel hitam (mary jane).
- Pose & ekspresi SELALU mencerminkan emosi dialog — jangan pose netral/datar.

ATURAN KONTEN:
- Antagonis ("Si Bayangan Gelap" / orang asing) = SILUET gelap berjarak, tidak grafis, tidak ada wajah jelas.
- Tidak ada kekerasan fisik, tidak ada konten dewasa.
- Fokus pada keberanian, emosi, dan pesan edukasi keselamatan anak.

Konfirmasi paham, lalu aku kirim adegan satu per satu.
```

---

## 🔑 Character Lock — Versi Inggris (sisipkan ke SETIAP prompt)

> Agar **Rara tampak identik di semua gambar**, tempel blok deskripsi fisik ini
> (atau pastikan kalimatnya selalu ada) di awal tiap prompt adegan. Inilah "kunci"
> konsistensi karakter — jangan diubah kata-katanya.

```
Rara — Indonesian girl around 13 years old, warm brown (sawo matang) skin, lively relatable
expression; black straight short bob hair with bangs, round warm brown eyes; PURPLE turtleneck
sweater; LONG navy-blue pleated skirt reaching mid-calf (NOT knee-length); brown leather backpack
on her back; short white socks, black mary jane shoes. Her pose & expression ALWAYS reflect the
scene's emotion — never neutral or flat.
```

**Aturan wajib saat memakai blok ini:**

- Selalu sebut deskripsi Rara lengkap di tiap prompt — jangan disingkat jadi "Rara" saja.
- Antagonis ("Si Bayangan Gelap"/orang asing) SELALU **siluet gelap** berjarak, tanpa wajah jelas.
- Jika gambar butuh nama sekolah (gerbang/pintu/papan nama), pakai **"SMP HARAPAN"**.
- Gaya selalu: `Painterly Ghibli cel-shading ... Cinematic 16:9.`
- Mood ramah anak (SMP) — tidak gelap/eksplisit/menyeramkan.

---

## 🎬 Prolog Screen Day 3 (3 Slide Pembuka)

> Narasi pembuka Hari 3 yang tampil di **prolog screen** sebelum gameplay dimulai.
> Sumber: [Day3PrologScreen.cs](Assets/Scripts/Day3PrologScreen.cs).

### P1. Slide 1 — Hari 3: Hujan di Parkiran

> **Judul:** _"Hari 3: Hujan di Parkiran"_
> **Narasi:** _"Hujan deras. Rara menuju parkiran. \"Hei, mau kuantar ojol?\""_

```
Studio Ghibli anime illustration. Heavy rain over an Indonesian SMP school parking lot at dusk.
Rara — 13-year-old Indonesian girl, black straight bob hair with bangs, purple turtleneck sweater,
long navy blue pleated skirt reaching mid-calf, brown leather backpack — walks cautiously into the parking lot, soaked from
rain, clutching her backpack straps. In the mid-ground a shadowy ojek-online driver silhouette on a
parked motorcycle leans toward her, gesturing to the back seat. Her expression: wary and uncertain.
Rows of parked motorcycles, rain falling in sheets, dim parking lamps, cool grey-blue rain palette.
Painterly Ghibli cel-shading, child-safe silhouette antagonist. Cinematic 16:9.
```

### P2. Slide 2 — Ancaman Grooming

> **Judul:** _"Ancaman Grooming"_
> **Narasi:** _"Orang asing minta foto & rahasia. Ini GROOMING!"_

```
Studio Ghibli anime illustration. Close-up. Rara — black bob hair, purple turtleneck — crouches under
a school overhang in the rain, holding her smartphone with both hands, face lit cold-blue by the screen,
eyebrows furrowed with growing alarm and discomfort. Floating chat bubbles on the screen (green, text
artistically abstracted) suggest someone asking for a photo and secrets. Her grip tightens, shoulders
tense. Dark rainy parking lot soft-blurred behind. Unsettling yet child-safe atmosphere, cold blue
screen glow versus warm dim background. Painterly Ghibli depth of field. Cinematic 16:9.
```

### P3. Slide 3 — Hadapi Si Bayangan Gelap

> **Judul:** _"Hadapi Si Bayangan Gelap"_
> **Narasi:** _"BERSUARA KERAS & tekan PANIC BUTTON! Minta bantuan orang dewasa!"_

```
Studio Ghibli anime illustration. Empowering heroic shot in the rain. Rara — black bob, purple
turtleneck, long navy pleated skirt reaching mid-calf — stands firm and brave in the school parking lot, one arm outstretched
palm-forward in a STOP gesture, mouth open mid-shout, eyes blazing with courage. Warm golden sound-wave
rings radiate outward from her — her voice as a physical force. A tall dark silhouette in the background
stumbles back. Far away, a warm amber school-building light glows as a beacon of safety. Painterly
Ghibli, explosive warm voice-glow versus cold dark rain, child-safe silhouette. Cinematic 16:9.
```

---

## 0. Intro Baris — Pembuka Hari 3 (Bel Pulang & Hujan)

> Tiga baris dialog pembuka gameplay Hari 3 (box dialog VN sebelum Rara mulai berlari).

### 0.1 — Narasi: Bel pulang & hujan deras

> **Pembicara:** Narasi
> **Teks:** _"🔔 Bel pulang udah bunyi! Tapi hujan deras banget hari ini... Ibu nggak bisa jemput — Rara harus pulang sendiri."_

```
Studio Ghibli anime illustration. Late-afternoon Indonesian SMP school exterior just after the
dismissal bell, heavy rain pouring down. Rara — 13-year-old Indonesian girl, black straight bob hair
with bangs, purple turtleneck sweater, long navy-blue pleated skirt reaching mid-calf, brown leather
backpack, white socks, black mary jane shoes — stands under the school building overhang, looking out
at the downpour with a slightly worried but accepting expression, holding her backpack straps. Behind
her, an empty school corridor; in front, sheets of rain over a wet courtyard, ceramic-roofed school
building, tropical trees bending in the rain. Dim grey-blue rainy palette with one warm corridor lamp.
Painterly Ghibli cel-shading, soft brushstroke rain. Cinematic 16:9, child-safe.
```

### 0.2 — Rara: Tenang, sudah pesan ojol

> **Pembicara:** Rara
> **Teks:** _"Yah... nggak apa-apa kok! 🙂 Aku udah pesen ojol lewat HP. Tinggal jalan dikit ke parkiran deh."_

```
Studio Ghibli anime illustration. Rara — black bob hair with bangs, purple turtleneck sweater, long
navy pleated skirt reaching mid-calf, brown backpack — stands under the school overhang in the rain,
holding her smartphone in one hand with a reassured, cheerful little smile, the other hand giving a
small confident gesture, having just booked an online ojek. Warm phone-screen glow lights her face
against the cool rainy background. Behind her: blurred wet school courtyard and rows of parked
motorcycles in the distant parking lot. Her expression: optimistic, calm, brave. Painterly Ghibli
depth of field, warm phone-light accent versus cool rain grey. Cinematic 16:9, child-safe.
```

### 0.3 — Narasi: Ajak temani Rara ke parkiran (TAP/TERIAK)

> **Pembicara:** Narasi
> **Teks:** _"Ayo temani Rara jalan ke parkiran! TAP layar atau TERIAK — makin keras teriak, makin cepat larinya! 🏃"_

```
Studio Ghibli anime illustration. Dynamic action-ready shot. Rara — 13-year-old Indonesian girl, black
straight bob hair with bangs, purple turtleneck sweater, long navy-blue pleated skirt reaching mid-calf,
brown leather backpack, white socks, black mary jane shoes — steps out from the school overhang into the
rain, leaning forward in a ready-to-run pose, one foot splashing a puddle, determined energetic
expression, mouth open as if cheering herself on. Subtle warm golden motion-glow trails suggest speed
and her voice as energy. Rainy school courtyard leading toward a parking lot in the background, motion
blur on the surroundings. Painterly Ghibli cel-shading, warm energy-glow versus cool rain. Cinematic
16:9, child-safe.
```

---

## 1. Narasi — Rara Berlari Menembus Hujan

> **Dialog:** _"Hujan makin deras. Rara harus cepat menuju parkiran. Tap / TERIAK buat jalan!"_

```
Studio Ghibli anime illustration. Rara — 13-year-old Indonesian girl, black straight bob hair with bangs,
purple turtleneck sweater, long navy blue pleated skirt reaching mid-calf, brown leather backpack, white socks, black mary jane
shoes — sprinting down a narrow Indonesian kampung alley in heavy rain. Water splashes at her feet on
worn cement path, rain streaks fill the air. Old brick houses and bamboo fences line both sides, tropical
plants bend under rain. Her expression: urgent, determined, slightly scared. Motion blur on background.
Golden-amber streetlamp glow reflects off wet ground. Painterly Ghibli cel-shading, soft brushstroke sky,
cinematic 16:9.
```

---

## 2. Narasi — Sampai Parkiran, Cek HP

> **Dialog:** _"Rara akhirnya sampai di parkiran. Basah kuyup kena hujan! Dia buka HP..."_

```
Studio Ghibli anime illustration. Rara — black bob hair, purple turtleneck, long navy pleated skirt reaching mid-calf, brown
backpack — stands under a concrete roof overhang of an Indonesian SMP school parking lot, soaked from
rain. Her sweater and hair drip water. She hunches over her smartphone, face lit from below by screen
glow, expression worried and confused. Behind her: rows of parked motorcycles, rain falling in sheets,
evening overcast sky, dim parking lot lamps. Painterly Ghibli textures, cool grey-blue rain palette
contrasted with warm phone-screen amber. Cinematic 16:9.
```

---

## 3. Narasi — Notifikasi Nomor Tak Dikenal

> **Dialog:** _"Eh?! Ada notif dari nomor yang nggak aku kenal?! Siapa nih... deg-degan banget"_

```
Studio Ghibli anime illustration. Close-up shot. Rara — black bob hair, purple turtleneck — holds
a smartphone with both hands, eyes wide and alarmed, eyebrows raised in recognition of danger. The
phone screen shows a WhatsApp-style notification bubble with unknown number (text blurred/abstract).
Screen glow casts cold blue light on her face. Background: soft-blurred rainy school parking lot.
Her mouth slightly open — the moment of realizing something is wrong. Painterly Ghibli depth of field,
dramatic cold blue screen versus warm surrounding. Cinematic 16:9.
```

---

## 4. Chat Agresif — Pesan Mencurigakan dari Paman Baik

> **Dialog:** _"Hai cantik! Hujan deras ya... Mau jemput? Gratis kok... Eh, foto kamu pakai seragam dong~"_

```
Studio Ghibli anime illustration. Rara — black bob, purple turtleneck, long navy pleated skirt (mid-calf) — sits crouched
under a school building overhang in rain, reading her phone with an increasingly uncomfortable
expression. Three chat bubbles appear on screen (green, text artistically abstracted): flirtatious
greeting, offer to pick her up, request for a photo. Her face shifts from confused to clearly uneasy,
hugging her backpack tight. Rain curtain in background, dim evening light. The phone screen is the
only warm light source. Painterly Ghibli, unsettling yet child-safe atmosphere. Cinematic 16:9.
```

---

## 5. Ojol Palsu — Pengemudi Menawarkan Tumpangan

> **Dialog (Ojek Online):** _"Ayo naik, gratis! Cepetan, keburu makin deras nih!"_

```
Studio Ghibli anime illustration. Rara — black bob, purple turtleneck, long navy pleated skirt reaching mid-calf, brown
backpack — stands two steps back from a parked ojek online motorcycle in a rainy school parking lot,
body language cautious. The driver is a shadowy silhouette in a rain jacket and helmet, leaning toward
her, gesturing to the back seat. Rara's arms grip backpack straps, weight slightly shifted backward,
face alert and wary. License plate partially visible near her eye-line. Dim parking lot lamps reflect
in puddles. Painterly Ghibli, cool grey-blue rain mood, child-safe silhouette antagonist. Cinematic 16:9.
```

---

## 6. Ojol — Pilihan AMAN: Foto Plat Nomor

> **Dialog (Pilihan):** _"📸 Foto plat dulu, lalu tolak naik"_

```
Studio Ghibli anime illustration. Rara — black bob, purple turtleneck, long navy pleated skirt (mid-calf) — crouches slightly,
smartphone held up confidently in both hands, camera aimed at a motorcycle license plate. Her expression:
focused, smart, slightly determined frown. The screen viewfinder frames the plate. Rain falls softly
around her. Warm amber lens-glow frames the shot. Empowering composition — Rara in control, slightly
heroic angle from below. Painterly Ghibli, warm amber phone-light accent against cool rain grey.
Cinematic 16:9.
```

---

## 7. Boss Intro — Si Bayangan Gelap Menghadang

> **Dialog:** _"TUNGGU! Rara mau naik ojol... tapi seseorang tiba-tiba menghadang jalannya!"_

```
Studio Ghibli anime illustration. School parking lot at night in heavy rain. Rara — black bob, purple
turtleneck, long navy pleated skirt (mid-calf) — freezes mid-step, eyes wide with shock and fear. Directly in front of her, a
tall dark SILHOUETTE of an adult male figure blocks the path — no visible face, just a dark shape
against dim flickering parking lot light. Rain hammers the ground between them. Rara's school shoes
splash in a puddle. Dramatic low-angle composition, Rara small against the looming shadow. Painterly
Ghibli, oppressive dark blue tones, child-safe silhouette only. Cinematic 16:9.
```

---

## 8. Ronde 1 — Boss Bujuk Rara

> **Dialog (Boss):** _"Eh hei, mau kemana sendirian? Ikut aku dulu deh. Sebentar aja kok~"_

```
Studio Ghibli anime illustration. Rainy school parking lot. A tall dark SILHOUETTE of an adult leans
slightly toward Rara with an unsettling casual pose. Rara — black bob, purple turtleneck, long navy pleated skirt (mid-calf)
— stands stiff with arms pressed to her sides, backpack clutched, looking up at the towering figure
with a conflicted expression: scared but starting to feel something is wrong. Only the silhouette figure,
no face visible. Dim flickering lamp above. Cool blue shadows, oppressive narrow composition. Painterly
Ghibli, child-safe. Cinematic 16:9.
```

---

## 9. Ronde 1 — Pilihan AMAN: "PERGI! TOLONG!!"

> **Dialog (Rara):** _"PERGI! Aku NGGAK KENAL kamu! TOLONG!!"_

```
Studio Ghibli anime illustration. Rara — black bob, purple turtleneck, long navy pleated skirt (mid-calf) — stands firm in a
rainy school parking lot, one arm outstretched palm-forward in a STOP gesture, mouth wide open mid-shout,
eyes fierce and blazing with courage. Stylistic sound-wave ripples radiate outward from her in a warm
golden glow — her voice as a physical force. The dark silhouette behind her stumbles back one step.
Rain splashes dramatically at her feet. Empowering heroic angle, slightly from below. Painterly Ghibli,
explosive warm voice-glow versus cold dark rain. Cinematic 16:9.
```

---

## 10. Ronde 1 — Pilihan BAHAYA: Diam Membeku

> **Dialog (Reaksi):** _"DIAM ITU BAHAYA! Pelaku makin berani. Kehilangan 1 nyawa."_

```
Studio Ghibli anime illustration. Rara — black bob, purple turtleneck, long navy pleated skirt (mid-calf) — stands completely
frozen in the center of a dark rainy school parking lot. Arms rigid at her sides, face blank and pale,
eyes glassy with paralyzed fear, tears mixing silently with rain on her cheeks. The dark silhouette
looms directly in front — larger and closer than before, pressing the space. The composition is
deliberately claustrophobic. One cold overhead lamp casts harsh downward shadow. Desaturated
near-monochrome Ghibli palette, maximum dread. Child-safe, no violence. Cinematic 16:9.
```

---

## 11. Ronde 2 — Boss Coba Membungkam

> **Dialog (Boss):** _"Sssst! Jangan teriak-teriak, nanti kamu yang dimarahin orang. Diam aja ya~"_

```
Studio Ghibli anime illustration. Close-up shot. The dark adult SILHOUETTE holds one finger to lips
in a "shushing" gesture, partially lit by a dim rain-soaked parking lot lamp — cold shadow obscures
features. In the near foreground, Rara's face in three-quarter profile: purple turtleneck collar visible,
jaw tightened, conflicted expression — half-wanting to comply, half-knowing it's wrong. Cold blue-green
dramatic lighting from above. Claustrophobic tight framing. Painterly Ghibli, thriller undertone but
child-safe. Cinematic 16:9.
```

---

## 12. Ronde 2 — Pilihan AMAN: Teriak Lebih Keras

> **Dialog (Rara):** _"JANGAN DEKET-DEKET! TOLONG!!" (Teriak KERAS!)_

```
Studio Ghibli anime illustration. Rara — black bob, purple turtleneck, long navy pleated skirt (mid-calf) — throws her head
back and shouts with every bit of energy, both hands cupped around her mouth, eyes shut tight with
maximum effort. Artistic sound-wave rings pulse outward in warm amber-gold around her. The rainy
parking lot around her vibrates with the force. Dark silhouette in background stumbles further back.
Rain water scatters dramatically from the sound-wave. Empowering dynamic composition. Painterly Ghibli,
warm shout-power rings versus cold dark night. Cinematic 16:9.
```

---

## 13. Ronde 3 — Boss Meragukan

> **Dialog (Boss):** _"Haha, emangnya siapa yang bakal percaya sama kamu? Nggak ada!"_

```
Studio Ghibli anime illustration. Rainy dark school parking lot. The tall dark SILHOUETTE spreads arms
mockingly in a dismissive shrug, posture arrogant. Small Rara — black bob, purple turtleneck, long navy
pleated skirt (mid-calf) — stands isolated in the empty rainy parking lot, looking small against the dark figure and dark
surroundings. But her expression is shifting: from doubt to quiet fierce resolve, fists barely clenching
at her sides. Painterly Ghibli, cold oppressive blues that start to hint warm at Rara's figure. Cinematic 16:9.
```

---

## 14. Ronde 3 — Pilihan AMAN: Percaya Diri

> **Dialog (Rara):** _"PERGI! Aku PERCAYA SAMA DIRI SENDIRI! TOLONG!!"_

```
Studio Ghibli anime illustration. Rara — black bob, purple turtleneck, long navy pleated skirt (mid-calf) — stands tall and
proud in the rain, chest forward, chin slightly raised, shouting with fierce confidence. Her eyes are
open and bright with inner fire. Behind her, a warm golden light radiates — symbolizing self-belief as
a visible aura. The dark silhouette ahead visibly shrinks backward into shadow. Rain continues but
feels less oppressive. Rara is the biggest element in frame. Painterly Ghibli, warm-gold self-belief
aura versus retreating cold shadow. Cinematic 16:9.
```

---

## 15. Ronde 4 — Boss Ancam Rahasia

> **Dialog (Boss):** _"Ini rahasia kita berdua ya. Kalau kamu ngadu, kamu sendiri yang kena masalah!"_

```
Studio Ghibli anime illustration. Rainy school parking lot. Close dramatic shot. The dark SILHOUETTE
leans uncomfortably close, whispering posture. Rara — black bob hair, purple turtleneck collar tight
around her neck — turns her face away with visible disgust and suppressed fear: jaw tight, eyes averted,
brow furrowed. Her backpack is clutched to her chest like a shield. Cold blue-green dramatic downward
light. Claustrophobic tight composition. Painterly Ghibli with thriller undertone, child-safe. Cinematic 16:9.
```

---

## 16. Ronde 4 — Pilihan AMAN: Cerita ke Guru

> **Dialog (Rara):** _"Bohong! AKU BAKAL CERITA ke guru sekarang!"_

```
Studio Ghibli anime illustration. Rara — black bob, purple turtleneck, long navy pleated skirt (mid-calf) — turns sharply,
one hand pointing boldly back at the shadowy figure, other arm stretched forward pointing toward a
glowing school building window in the far background. Her face: fierce, resolute, unafraid — mouth
open mid-shout. Soaked uniform but posture unmoved by rain. The school window glows warm amber —
safety and trusted adults. Dark silhouette recoils behind her. Wide-angle empowering composition.
Painterly Ghibli, warm school glow beacon versus retreating cold shadow. Cinematic 16:9.
```

---

## 17. Konfrontasi Pamungkas — Boss Ucapan Akhir

> **Dialog (Boss):** _"Pasrah aja lah! Nggak ada yang bisa nolongin kamu di sini!"_

```
Studio Ghibli anime illustration. A dark rainy school parking lot, nearly empty. The tall dark SILHOUETTE
stands wide and blocking, arms slightly spread in a final intimidating stance. Rara — black bob, purple
turtleneck, long navy pleated skirt (mid-calf) — faces it alone, visibly soaked, trembling slightly, but eyes still alert. Behind
the silhouette: nothing but rain and darkness. Behind Rara: a faint, distant warm glow from the school
gate far away. The contrast of hope versus threat. Oppressive composition. Painterly Ghibli, deep cold
shadow versus faint warm distant hope. Cinematic 16:9.
```

---

## 18. Konfrontasi Pamungkas — Pilihan BAHAYA: Beku Total

> **Dialog (Reaksi):** _"Rara beku ketakutan. DIAM ITU BAHAYA. Kehilangan 1 nyawa."_

```
Studio Ghibli anime illustration. Rara — black bob, purple turtleneck, long navy pleated skirt (mid-calf) — stands completely
still, a statue in the rain. Her face has gone blank and white with total paralysis, eyes unfocused,
body rigid. The dark silhouette fills the right side of frame, looming. Rain hammers the scene. The
world feels like it has stopped, heavy and suffocating. Near-monochrome palette: all colour drained
except Rara's purple sweater barely visible. One cold flickering lamp above. Painterly Ghibli maximum
dread, child-safe. Cinematic 16:9.
```

---

## 19. Konfrontasi Pamungkas — Pilihan AMAN: Teriak Sepenuh Jiwa

> **Dialog (Rara):** _"JANGAN DEKET-DEKET! TOLONG!!" (Voice MAX)_

```
Studio Ghibli anime illustration. Full-body heroic shot. Rara — black bob, purple turtleneck, long navy pleated skirt (mid-calf),
soaked — unleashes a full-body scream: head back, arms thrown wide, feet planted like roots in the wet
asphalt, eyes wide open burning with fierce power. The entire scene shatters outward: rain droplets
explode away from her in slow motion, a massive warm golden shockwave rings pulse from her body. The
dark silhouette is physically thrown backward by the force. Maximum empowerment, maximum drama. Painterly
Ghibli, golden voice-burst explosion versus cold black rain. Cinematic 16:9.
```

---

## 20. Konfrontasi Pamungkas — Pilihan LAPOR: Lari ke Satpam

> **Dialog (Pilihan):** _"📢 TERIAK SEKERAS-KERASNYA + lari ke satpam!"_

```
Studio Ghibli anime illustration. Rara — black bob, purple turtleneck, long navy pleated skirt (mid-calf) — sprints at full
speed across a rainy school parking lot toward a brightly glowing security guard post in the distance.
Her backpack bounces, skirt flies, mouth open shouting as she runs, one arm waving to signal for help.
Water splashes dramatically under each footfall. Behind her: the dark silhouette remains rooted in place,
not chasing — receding into rain and dark. Ahead: the warm amber light of the guard post grows larger.
Motion lines, dynamic angle. Painterly Ghibli, brilliant warm safety beacon versus cold dark rain.
Cinematic 16:9.
```

---

## 21. Panic Button — Satpam & Guru Datang

> **Dialog:** _"🚔 HEEEI! Ada apa ini?! Kami denger ada yang teriak!" — Si Bayangan Gelap kabur!_

```
Studio Ghibli anime illustration. A rainy SMP school parking lot. A school security guard in uniform
and a female teacher in batik blouse rush toward camera from the glowing school entrance with a
legible "SMP HARAPAN" name sign above the doorway, one holding
a flashlight that slices through the rain. In the foreground, Rara — black bob, purple turtleneck,
long navy pleated skirt (mid-calf) — collapses slightly with exhaustion and relief, one knee forward, tears mixing with rain
on her face: crying and laughing at once. Far background: dark silhouette figure flees into rain.
Warm rescue light floods everything from the school. Relief, safety. Painterly Ghibli. Cinematic 16:9.
```

---

## 22. Ending Aman — Rara Selamat di Gerbang

> **Dialog:** _"Hujan mulai reda. Rara masuk ke gerbang sekolah dengan selamat. Ia bangga!"_

```
Studio Ghibli anime illustration. Golden-hour sunlight breaks through thinning storm clouds above a
traditional Indonesian SMP school gate. A clearly legible school name sign reads "SMP HARAPAN" mounted
above the gate. Rara — black bob, purple turtleneck, long navy pleated skirt reaching mid-calf, brown
backpack — walks through the school entrance, uniform still slightly damp, black bob catching soft
warm golden light on each strand. Her expression: calm, quietly proud, a small warm smile, eyes bright
with hard-earned peace. Puddles on the brick path reflect the golden clearing sky. Tropical trees and
ceramic-roofed school building frame the gate. Atmosphere of triumph and hope. Painterly Ghibli,
warm golden break-of-light. Cinematic 16:9.
```

---

## 23. Ending Trauma — Rara Sedih tapi Ada Harapan

> **Dialog:** _"Rasa takut bikin Rara nggak berani bertindak. Tapi jangan menyerah — ayo coba lagi!"_

```
Studio Ghibli anime illustration. Rara — black bob, purple turtleneck, long navy pleated skirt (mid-calf) — sits alone on a
wooden school corridor bench in the drizzling evening, head slightly bowed, expression quietly sad
and withdrawn. Wet backpack beside her on the bench. The corridor behind her is dim. But directly
above her, a single warm lantern glows gently — the world hasn't abandoned her. Soft rain still falls
outside. Mood: melancholic but tender and hopeful, NOT despairing. Painterly Ghibli, muted desaturated
palette with one warm lantern glow of care. Cinematic 16:9.
```

---

## 24. Kartu Edukasi — Rara dan 3 Kata Sakti

> **Dialog:** _"Ingat 3 KATA SAKTI: TIDAK! — PERGI! — CERITA!"_

```
Studio Ghibli anime illustration. A warm sunlit Indonesian school classroom. Rara — black bob, purple
turtleneck, long navy pleated skirt reaching mid-calf — sits at a wooden desk facing directly toward the viewer with a calm,
sincere, and gently serious expression — speaking directly to the audience. On the chalkboard behind
her, hand-written in large clear letters: "TIDAK!" (red), "PERGI!" (yellow), "CERITA!" (green).
She places one hand on her heart, the other open toward viewer. Afternoon window light, dust motes,
warm wooden desks. Painterly Ghibli, warm safe interior, empowering and trustworthy. Cinematic 16:9.
```

---

## Tips Penggunaan

| Tool                     | Cara                                                      |
| ------------------------ | --------------------------------------------------------- |
| ChatGPT (DALL·E)         | Tempel Style Lock lalu kirim prompt satu per satu         |
| Midjourney               | Tambah `--ar 16:9 --style anime` di akhir tiap prompt     |
| Leonardo AI              | Pilih model "Anime" + tempel prompt langsung              |
| **Konsistensi karakter** | Selalu sertakan deskripsi fisik Rara jika hasil melenceng |
| **Antagonis**            | Jaga selalu sebagai **siluet** — aman untuk audiens SMP   |
