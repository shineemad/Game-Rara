using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// NpcDialog — sistem dialog ringkas untuk NPC (mis. Paman Baik).
///
/// Cara pakai:
///   1. Tambahkan komponen ini ke GameObject mana saja (boleh ke NPC-nya langsung).
///   2. Isi array <see cref="lines"/> di Inspector:
///        • speakerName : nama pembicara ("Paman", "Rara", dll.)
///        • profile     : sprite foto/ikon pembicara (opsional)
///        • text        : isi kalimat
///   3. Atur tampilan (warna panel, ukuran font, posisi) di header "Tampilan".
///   4. Panggil <see cref="Play"/> dari script lain (mis. PamanBaik) untuk memulai.
///
/// Pemain menekan SPACE / ENTER / klik untuk lanjut.
/// UI dibangun otomatis saat runtime — tidak perlu setup Canvas manual.
/// </summary>
public class NpcDialog : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    // DATA
    // ══════════════════════════════════════════════════════════════════════

    // ── Pilihan dialog (AMAN / RAGU / BAHAYA) ─────────────────────────────
    [System.Serializable]
    public class Choice
    {
        [Tooltip("Teks yang tampil di tombol")]
        public string label    = "Pilihan...";
        [Tooltip("Kategori: AMAN | RAGU | BAHAYA")]
        public string category = "AMAN";
        /// Callback saat dipilih — set via kode (tidak muncul di Inspector)
        [System.NonSerialized] public System.Action onSelect;
    }

    [System.Serializable]
    public class DialogEntry
    {
        public string speakerName = "Paman";
        public Sprite profile;
        [TextArea(2, 5)]
        public string text = "Halo Rara, hati-hati di jalan ya!";
        [Tooltip("Isi untuk menampilkan tombol pilihan. Kosongkan jika baris ini hanya teks biasa.")]
        public Choice[] choices;
    }

    [Header("Daftar Dialog")]
    [Tooltip("Tambah elemen untuk menambah baris dialog. Urut dari atas ke bawah.")]
    public DialogEntry[] lines;

    // ══════════════════════════════════════════════════════════════════════
    // TAMPILAN
    // ══════════════════════════════════════════════════════════════════════

    [Header("Tampilan — Panel")]
    public Color  panelColor       = new Color(0f, 0f, 0f, 0.78f);
    public Color  borderColor      = new Color(1f, 0.85f, 0.3f, 1f);
    public float  panelHeight      = 220f;
    [Range(0f, 1f)]
    public float  panelWidthRatio  = 0.85f;
    [Tooltip("Jarak panel dari bawah layar (pixel)")]
    public float  bottomMargin     = 40f;
    public bool   showAtTop        = false;

    [Header("Tampilan — Profil")]
    public float  profileSize      = 160f;
    public Color  profileBgColor   = new Color(1f, 1f, 1f, 0.1f);
    public bool   profileOnRight   = false;

    [Header("Tampilan — Teks")]
    public Color  speakerColor     = new Color(1f, 0.85f, 0.3f, 1f);
    public Color  textColor        = Color.white;
    public int    speakerFontSize  = 32;
    public int    textFontSize     = 26;

    [Header("Efek Ketik")]
    [Range(0f, 0.1f)]
    public float  typeSpeed        = 0.025f;

    [Header("Petunjuk Lanjut")]
    public string continueHint     = "▼ SPACE / Klik untuk lanjut";
    public Color  hintColor        = new Color(1f, 1f, 1f, 0.55f);
    public int    hintFontSize     = 16;

    [Header("Konten Box (live-edit saat dialog tampil)")]
    [Tooltip("Nama pembicara di banner — diisi otomatis dari baris aktif, bisa langsung diedit di sini")]
    public string editHeadline = "";
    [Tooltip("Isi kalimat dialog — diisi otomatis dari baris aktif, bisa langsung diedit (tidak berlaku saat animasi ketik sedang berjalan)")]
    [TextArea(3, 6)]
    public string editBody = "";
    [Tooltip("Foto/ilustrasi pembicara — diisi OTOMATIS dari field 'Profile' tiap baris dialog.\n" +
              "Gunakan sebagai profil AWAL / fallback jika baris pertama tidak punya profile-nya sendiri.\n" +
              "Per-baris 'Profile' di array Lines selalu lebih prioritas dari field ini.")]
    public Sprite editProfile;
    [Tooltip("Tampilkan latar belakang banner nama (NARASI, Paman, dll). Hilangkan centang untuk teks saja tanpa kotak.")]
    public bool   showBannerBg = true;

    [Header("Font (opsional)")]
    [Tooltip("Drag TMP Font Asset di sini. Kosongkan = pakai font default TMP otomatis.")]
    public TMP_FontAsset fontAsset;

    [Header("Sprite Box Dialog")]
    [Tooltip("Aset DialogBoxLayout bersama. Jika di-assign, nilainya akan menimpa\n" +
             "semua field tata letak + sprite di komponen ini.")]
    public DialogBoxLayout layout;
    [Tooltip("Drag sprite kotak dialog kayu ke sini. Layout portrait+banner otomatis menyesuaikan.\n" +
             "Saat Play di Editor, auto-load dari boxDialogSpritePath jika field ini kosong.")]
    public Sprite dialogBoxSprite;
    [Tooltip("Sprite lencana nama kecil (banner berujung diamond). Opsional — jika kosong diganti warna solid.")]
    public Sprite nameBannerSprite;
    [Tooltip("Path sprite box dialog (relatif Assets/). Diambil dari folder UI day 1/8.png agar sama dengan Day1Intro.")]
    public string boxDialogSpritePath = "sprites/UI day 1/8.png";

    [Header("Posisi & Ukuran Box (berlaku saat pakai dialogBoxSprite)")]
    [Tooltip("Posisi tengah horizontal panel (0=kiri, 1=kanan) — sama seperti PrologScreen panelCenterX")]
    [Range(0f, 1f)] public float panelCenterX    = 0.50f;
    [Tooltip("Posisi tengah vertikal panel (0=bawah, 1=atas) — sama seperti PrologScreen panelCenterY")]
    [Range(0f, 1f)] public float panelCenterY    = 0.26f;
    [Tooltip("Lebar panel sebagai fraksi layar (0–1) — sama seperti PrologScreen panelWidth")]
    [Range(0.1f, 1f)] public float panelWidthFrac  = 0.94f;
    [Tooltip("Tinggi panel sebagai fraksi layar (0–1) — sama seperti PrologScreen panelHeight")]
    [Range(0.02f, 0.5f)] public float panelHeightFrac = 0.28f;

    [Header("Tata Letak Box (anchor 0–1, hanya berlaku saat pakai sprite)")]
    [Tooltip("Posisi tengah horizontal foto profil dalam panel (0=kiri, 1=kanan)")]
    [Range(0f, 1f)] public float portraitCenterX = 0.13f;
    [Tooltip("Posisi tengah vertikal foto profil dalam panel (0=bawah, 1=atas)")]
    [Range(0f, 1f)] public float portraitCenterY = 0.50f;
    [Tooltip("Lebar foto profil sebagai fraksi lebar panel")]
    [Range(0.02f, 0.60f)] public float portraitSizeW = 0.20f;
    [Tooltip("Tinggi foto profil sebagai fraksi tinggi panel")]
    [Range(0.02f, 1.00f)] public float portraitSizeH = 0.84f;
    [Tooltip("Pertahankan rasio aspek foto (centang = foto tidak stretch, dikelilingi ruang kosong)")]
    public bool portraitPreserveAspect = true;
    [Tooltip("Posisi banner nama (kiri, bawah)")]
    public Vector2 bannerAnchorMin   = new Vector2(0.25f, 0.58f);
    [Tooltip("Posisi banner nama (kanan, atas)")]
    public Vector2 bannerAnchorMax   = new Vector2(0.52f, 0.82f);
    [Tooltip("Posisi area teks (kiri, bawah)")]
    public Vector2 textAnchorMin     = new Vector2(0.25f, 0.10f);
    [Tooltip("Posisi area teks (kanan, atas)")]
    public Vector2 textAnchorMax     = new Vector2(0.96f, 0.57f);

    [Header("Posisi Petunjuk Lanjut (geser di sini)")]
    [Tooltip("Posisi tengah horizontal petunjuk dalam panel (0=kiri, 1=kanan)")]
    [Range(0f, 1f)] public float hintCenterX = 0.78f;
    [Tooltip("Posisi tengah vertikal petunjuk dalam panel (0=bawah, 1=atas)")]
    [Range(0f, 1f)] public float hintCenterY = 0.13f;
    [Tooltip("Lebar area petunjuk (fraksi lebar panel)")]
    [Range(0.05f, 1f)] public float hintSizeW = 0.36f;
    [Tooltip("Tinggi area petunjuk (fraksi tinggi panel)")]
    [Range(0.02f, 0.5f)] public float hintSizeH = 0.18f;

    [Header("Event")]
    [Tooltip("Dipanggil saat seluruh dialog selesai")]
    public UnityEngine.Events.UnityEvent onDialogEnd;

    // ── runtime state ────────────────────────────────────────────────────
    private Canvas           canvas;
    private GameObject       panelRoot;
    private Image            profileImg;
    private TextMeshProUGUI  speakerTMP;
    private TextMeshProUGUI  textTMP;
    private TextMeshProUGUI  hintTMP;
    private int              currentIndex;
    private bool             isPlaying;
    private bool             isTyping;
    private Coroutine        typingCo;
    private GameObject       choicesPanel;      // panel tombol pilihan
    private Choice[]         _pendingChoices;   // choices baris aktif, diproses TypeText
    private bool             _choiceProcessed;  // guard double-fire pilihan
    private bool             _ignoreNextAdvance;// blokir Advance() 1 frame setelah klik pilihan
    private System.Action    _playLinesCallback;// callback dari PlayLines()
    // Sprite yang sedang ditampilkan — PRIVATE, hanya diubah ShowLine().
    // Tidak ikut OnValidate sehingga Inspector tidak bisa menimpanya.
    private Sprite           _displayedProfile;
    // ── RT untuk live-edit via OnValidate ────────────────────────────────
    private RectTransform    panelRT;
    private RectTransform    profileRT;
    private RectTransform    bannerRT;
    private RectTransform    bodyRT;
    private RectTransform    hintRT;
    private Image            bannerImg;

    public bool IsPlaying => isPlaying;

    // ══════════════════════════════════════════════════════════════════════
    // PRESET DIALOG (dari game web https://game-jaga-diri.vercel.app — Day 1)
    // Klik kanan komponen di Inspector → "Load Preset: Paman Baik (Day 1)"
    // ══════════════════════════════════════════════════════════════════════
    [ContextMenu("Load Preset: Paman Baik (Day 1)")]
    void LoadPresetPamanBaikDay1()
    {
        lines = new DialogEntry[]
        {
            new DialogEntry {
                speakerName = "Narasi",
                text = "*Tiba-tiba ada orang asing mendekat ke arah Rara...*"
            },
            new DialogEntry {
                speakerName = "Paman Baik",
                text = "\"Hei dek, bentar ya~!\""
            },
            new DialogEntry {
                speakerName = "Rara (dalam hati)",
                text = "Eh? Siapa ini? Rara nggak pernah lihat orang ini sebelumnya..."
            },
            new DialogEntry {
                speakerName = "Paman Baik",
                text = "\"Mau permen nggak? Enak banget lho~\nOm punya banyak di warung, ikut bentar aja ya, deket kok!\""
            },
            new DialogEntry {
                speakerName = "Rara (dalam hati)",
                text = "Tunggu... Rara nggak kenal orang ini sama sekali!\nDia nawarin permen DAN mau ngajak Rara pergi... INI NGGAK BENER!"
            },
            new DialogEntry {
                speakerName = "Paman Baik",
                text = "\"Ayo dong, nggak usah malu-malu~ Sebentar aja kok!\""
            },
            new DialogEntry {
                speakerName = "— PILIH RESPONS RARA —",
                text = "Gimana Rara harus merespons orang ini?\n\n[AMAN]  \"NGGAK MAU! Aku nggak kenal Bapak!\" — Lari ke tempat rame!\n[RAGU]  \"Makasih Pak, tapi aku udah mau telat sekolah...\"\n[BAHAYA] \"Boleh~ Om punya permen apa aja?\""
            },
            new DialogEntry {
                speakerName = "Narasi",
                text = "INGAT: Orang asing yang kasih hadiah atau ngajak pergi = TANDA BAHAYA!\nSelalu tolak dengan tegas dan pergi ke tempat yang rame!"
            },
        };

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log("[NpcDialog] Preset 'Paman Baik (Day 1)' dimuat — " + lines.Length + " baris.");
#endif
    }
    [ContextMenu(">>> TEST: Tampilkan Dialog Sekarang (Play Mode)")]
    public void TestPlay() => Play();

#if UNITY_EDITOR
    void Reset()
    {
        TryLoadBoxSprite(overwrite: true);
    }

    [ContextMenu("▶ Muat Sprite Box Dialog (UI day 1/8.png) + Layout")]
    void TryLoadBoxSpriteMenu()
    {
        TryLoadBoxSprite(overwrite: true);
        Debug.Log("[NpcDialog] dialogBoxSprite=" + (dialogBoxSprite != null ? dialogBoxSprite.name : "null"));
    }

    void TryLoadBoxSprite(bool overwrite)
    {
        if (string.IsNullOrEmpty(boxDialogSpritePath)) return;
        if (!overwrite && dialogBoxSprite != null) return;
        var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/" + boxDialogSpritePath);
        if (sp != null)
        {
            dialogBoxSprite = sp;
            // CATATAN: TIDAK auto-apply preset di sini agar tweak slider Inspector
            // tidak ke-overwrite. Panggil ApplyLayoutPreset8 manual via context menu.
            UnityEditor.EditorUtility.SetDirty(this);
        }
        else
        {
            Debug.LogWarning("[NpcDialog] Sprite tidak ditemukan: Assets/" + boxDialogSpritePath);
        }
    }
#endif

    // Preset tata letak yang pas untuk sprite 8.png (1325×547, rasio ~2.42:1).
    // Frame portrait kiri-atas (X 5.7–24.9%, Y 38–87%), banner kayu di bawah
    // portrait (X 5.7–25.3%, Y 19.6–33.3%), area teks besar di kanan.
    [ContextMenu("▶ Terapkan Layout Box untuk 8.png")]
    public void ApplyLayoutPreset8()
    {
        panelCenterX    = 0.50f;
        panelCenterY    = 0.215f;
        panelWidthFrac  = 0.96f;
        panelHeightFrac = 0.395f;

        portraitCenterX        = 0.153f;
        portraitCenterY        = 0.625f;
        portraitSizeW          = 0.192f;
        portraitSizeH          = 0.494f;
        portraitPreserveAspect = true;

        bannerAnchorMin = new Vector2(0.057f, 0.196f);
        bannerAnchorMax = new Vector2(0.253f, 0.333f);

        textAnchorMin = new Vector2(0.345f, 0.20f);
        textAnchorMax = new Vector2(0.955f, 0.78f);

        hintCenterX = 0.82f;
        hintCenterY = 0.13f;
        hintSizeW   = 0.30f;
        hintSizeH   = 0.12f;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        if (Application.isPlaying) ApplyLayout();
    }

    // ══════════════════════════════════════════════════════════════════════
    void Update()
    {
        if (!isPlaying) return;

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetMouseButtonDown(0))
        {
            Advance();
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // PUBLIC API
    // ══════════════════════════════════════════════════════════════════════

    /// Mulai memutar dialog dari awal.
    public void Play()
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("[NpcDialog] Daftar 'lines' kosong! Isi dulu di Inspector.");
            return;
        }

#if UNITY_EDITOR
        // Hanya load sprite kalau field kosong — JANGAN overwrite tweak Inspector.
        TryLoadBoxSprite(overwrite: false);
#endif
        // Jika DialogBoxLayout di-assign, salin nilainya ke field lokal.
        ApplyLayoutAsset();

        Debug.Log("[NpcDialog] Play() dipanggil — " + lines.Length + " baris dialog.");
        BuildUIIfNeeded();
        StopAllCoroutines();
        _pendingChoices    = null;
        _choiceProcessed   = false;
        _ignoreNextAdvance = false;
        if (choicesPanel != null) { Destroy(choicesPanel); choicesPanel = null; }
        currentIndex = 0;
        isPlaying    = true;
        panelRoot.SetActive(true);
        // Bekukan player langsung — tidak bergantung controller manapun
        SetPlayerFrozen(true);
        ShowLine(0);
    }

    /// Mainkan dialog dari array baris + callback saat selesai.
    public void PlayLines(DialogEntry[] newLines, System.Action onDone = null)
    {
        lines              = newLines;
        _playLinesCallback = onDone;
        Play();
    }

    /// Tutup paksa.
    public void Close()
    {
        isPlaying = false;
        StopAllCoroutines();
        _pendingChoices = null;
        if (choicesPanel != null) { Destroy(choicesPanel); choicesPanel = null; }
        if (panelRoot != null) panelRoot.SetActive(false);
        SetPlayerFrozen(false);
    }

    // ══════════════════════════════════════════════════════════════════════
    // INTERNAL — alur dialog
    // ══════════════════════════════════════════════════════════════════════

    void Advance()
    {
        // Blokir 1 frame setelah klik tombol pilihan (EventSystem + Update berjalan frame sama)
        if (_ignoreNextAdvance) { _ignoreNextAdvance = false; return; }

        // Panel pilihan aktif — harus klik tombol, bukan SPACE/klik sembarang
        if (choicesPanel != null && choicesPanel.activeSelf) return;

        if (isTyping)
        {
            // Skip animasi ketik
            if (typingCo != null) StopCoroutine(typingCo);
            isTyping     = false;
            textTMP.text = lines[currentIndex].text;
            var cur = lines[currentIndex];
            if (cur.choices != null && cur.choices.Length > 0)
            {
                _pendingChoices = null;
                if (hintTMP != null) hintTMP.gameObject.SetActive(false);
                BuildChoiceButtons(cur.choices);
            }
            return;
        }

        currentIndex++;
        if (currentIndex >= lines.Length)
        {
            EndDialog();
            return;
        }
        ShowLine(currentIndex);
    }

    void ShowLine(int idx)
    {
        var line = lines[idx];

        speakerTMP.text = line.speakerName;
        editHeadline    = line.speakerName;
        editBody        = line.text;

        // Prioritas profil:
        //   1. line.profile (per-baris) — tertinggi, ganti foto saat dialog berganti
        //   2. _displayedProfile (pertahankan foto sebelumnya jika baris ini tidak punya)
        //   3. editProfile (fallback global, hanya dipakai jika belum ada foto sama sekali)
        if (line.profile != null)
            _displayedProfile = line.profile;           // ganti ke foto baris ini
        else if (_displayedProfile == null)
            _displayedProfile = editProfile;            // pakai default Inspector jika belum ada
        // else: pertahankan _displayedProfile (foto pembicara sebelumnya tetap tampil)

        ApplyProfile(_displayedProfile);

        // Simpan choices — TypeText akan build tombol saat ketikan selesai
        _pendingChoices = (line.choices != null && line.choices.Length > 0) ? line.choices : null;
        if (_pendingChoices != null)
        {
            if (hintTMP != null) hintTMP.gameObject.SetActive(false);
        }
        else
        {
            if (hintTMP != null) hintTMP.gameObject.SetActive(true);
        }

        if (typingCo != null) StopCoroutine(typingCo);
        typingCo = StartCoroutine(TypeText(line.text));
    }

    IEnumerator TypeText(string full)
    {
        isTyping     = true;
        textTMP.text = "";
        if (typeSpeed <= 0f)
        {
            textTMP.text = full;
            isTyping     = false;
            if (_pendingChoices != null) { BuildChoiceButtons(_pendingChoices); _pendingChoices = null; }
            yield break;
        }
        foreach (char c in full)
        {
            textTMP.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
        isTyping = false;
        if (_pendingChoices != null) { BuildChoiceButtons(_pendingChoices); _pendingChoices = null; }
    }

    void EndDialog()
    {
        isPlaying = false;
        StopAllCoroutines();
        _pendingChoices = null;
        if (choicesPanel != null) { Destroy(choicesPanel); choicesPanel = null; }
        panelRoot.SetActive(false);
        SetPlayerFrozen(false);
        onDialogEnd?.Invoke();
        // callback dari PlayLines() — dipanggil setelah onDialogEnd
        var cb = _playLinesCallback;
        _playLinesCallback = null;
        cb?.Invoke();
    }

    // Freeze / unfreeze player secara langsung — tidak bergantung controller manapun
    static void SetPlayerFrozen(bool frozen)
    {
        var p = FindFirstObjectByType<player>();
        if (p != null) p.frozen = frozen;
    }

    // ══ Sistem Pilihan ═══════════════════════════════════════════════════════════════════

    void BuildChoiceButtons(Choice[] choices)
    {
        if (choicesPanel != null) { Destroy(choicesPanel); choicesPanel = null; }
        _choiceProcessed = false;

        choicesPanel = new GameObject("ChoicesPanel");
        choicesPanel.transform.SetParent(canvas.transform, false);
        var cpRT = choicesPanel.AddComponent<RectTransform>();
        cpRT.anchorMin = new Vector2(0.01f, 0.36f);
        cpRT.anchorMax = new Vector2(0.62f, 0.90f);
        cpRT.offsetMin = Vector2.zero;
        cpRT.offsetMax = Vector2.zero;

        float slotH = 1f / choices.Length;
        for (int i = 0; i < choices.Length; i++)
        {
            var   c    = choices[i];
            float yMax = 1f - i * slotH;
            float yMin = yMax - slotH + 0.015f;

            var btnGO = new GameObject("Btn_" + c.category);
            btnGO.transform.SetParent(choicesPanel.transform, false);
            var btnRT = btnGO.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0f, yMin);
            btnRT.anchorMax = new Vector2(1f, yMax);
            btnRT.offsetMin = new Vector2(0f,  4f);
            btnRT.offsetMax = new Vector2(0f, -4f);

            var img = btnGO.AddComponent<Image>();
            img.color = CategoryToColor(c.category);

            var btn = btnGO.AddComponent<Button>();
            var bc  = btn.colors;
            bc.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
            bc.pressedColor     = new Color(0.7f, 0.7f, 0.7f, 1f);
            bc.colorMultiplier  = 1f;
            btn.colors = bc;

            var lblGO = new GameObject("Label");
            lblGO.transform.SetParent(btnGO.transform, false);
            var lblRT = lblGO.AddComponent<RectTransform>();
            lblRT.anchorMin = Vector2.zero;
            lblRT.anchorMax = Vector2.one;
            lblRT.offsetMin = new Vector2(14f,  4f);
            lblRT.offsetMax = new Vector2(-14f, -4f);
            var tmp = lblGO.AddComponent<TextMeshProUGUI>();
            ApplyFont(tmp);
            tmp.text               = c.label;
            tmp.fontSize           = textFontSize - 2;
            tmp.color              = Color.white;
            tmp.fontStyle          = FontStyles.Bold;
            tmp.alignment          = TextAlignmentOptions.MidlineLeft;
            tmp.enableWordWrapping = true;
            tmp.raycastTarget      = false;

            var localC = c;
            btn.onClick.AddListener(() => OnChoiceSelected(localC));
        }
    }

    static Color CategoryToColor(string cat)
    {
        switch (cat)
        {
            case "AMAN":   return new Color(0.149f, 0.678f, 0.380f, 1f);
            case "RAGU":   return new Color(0.949f, 0.616f, 0.071f, 1f);
            case "BAHAYA": return new Color(0.910f, 0.302f, 0.239f, 1f);
            default:       return new Color(0.200f, 0.624f, 0.859f, 1f);
        }
    }

    void OnChoiceSelected(Choice c)
    {
        if (_choiceProcessed) return;
        _choiceProcessed   = true;
        _ignoreNextAdvance = true;

        if (choicesPanel != null) { Destroy(choicesPanel); choicesPanel = null; }
        if (hintTMP != null) hintTMP.gameObject.SetActive(true);

        // Jika onSelect sudah diisi via kode (mis. BangunEncounterLines),
        // biarkan callback itu yang mengurus skor & nyawa — jangan duplikat di sini.
        if (c.onSelect != null)
        {
            c.onSelect.Invoke();
        }
        else if (GameState.Instance != null)
        {
            // Fallback built-in: untuk dialog Inspector tanpa callback kustom
            GameState.Instance.AddChoice(GameState.Instance.day, c.label, c.category);
            if (c.category == "BAHAYA")
                GameState.Instance.LoseLife();
            HUDManager.Instance?.Refresh();
        }

        currentIndex++;
        if (currentIndex >= lines.Length)
            EndDialog();
        else
            ShowLine(currentIndex);
    }

    // ══════════════════════════════════════════════════════════════════════
    // LIVE LAYOUT — ubah nilai di Inspector saat Play Mode → langsung update
    // ══════════════════════════════════════════════════════════════════════

    [ContextMenu("Terapkan Layout (Play Mode)")]
    public void ApplyLayout()
    {
        if (!Application.isPlaying || panelRoot == null) return;

        bool hasBox = (dialogBoxSprite != null);

        // Posisi & ukuran panel
        if (hasBox && panelRT != null)
        {
            panelRT.anchorMin = new Vector2(
                panelCenterX - panelWidthFrac  * 0.5f,
                panelCenterY - panelHeightFrac * 0.5f);
            panelRT.anchorMax = new Vector2(
                panelCenterX + panelWidthFrac  * 0.5f,
                panelCenterY + panelHeightFrac * 0.5f);
        }

        // Potret
        if (hasBox && profileRT != null)
        {
            profileRT.anchorMin = new Vector2(
                portraitCenterX - portraitSizeW * 0.5f,
                portraitCenterY - portraitSizeH * 0.5f);
            profileRT.anchorMax = new Vector2(
                portraitCenterX + portraitSizeW * 0.5f,
                portraitCenterY + portraitSizeH * 0.5f);
            if (profileImg != null)
                profileImg.preserveAspect = portraitPreserveAspect;
        }

        // Banner nama
        if (hasBox && bannerRT != null)
        {
            bannerRT.anchorMin = bannerAnchorMin;
            bannerRT.anchorMax = bannerAnchorMax;
        }

        // Banner nama — toggle latar belakang
        if (bannerImg != null)
            bannerImg.enabled = showBannerBg;

        // Area teks
        if (hasBox && bodyRT != null)
        {
            bodyRT.anchorMin = textAnchorMin;
            bodyRT.anchorMax = textAnchorMax;
        }

        // Petunjuk lanjut
        if (hasBox && hintRT != null)
        {
            hintRT.anchorMin = new Vector2(hintCenterX - hintSizeW * 0.5f, hintCenterY - hintSizeH * 0.5f);
            hintRT.anchorMax = new Vector2(hintCenterX + hintSizeW * 0.5f, hintCenterY + hintSizeH * 0.5f);
        }

        // Warna & ukuran font
        if (speakerTMP != null)
        {
            speakerTMP.color    = speakerColor;
            speakerTMP.fontSize = speakerFontSize;
        }
        if (textTMP != null)
        {
            textTMP.color    = textColor;
            textTMP.fontSize = textFontSize;
        }
        if (hintTMP != null)
        {
            hintTMP.color    = hintColor;
            hintTMP.fontSize = hintFontSize;
            hintTMP.text     = continueHint;
        }

        // Konten teks aktif (headline di banner, isi kalimat)
        if (speakerTMP != null)
            speakerTMP.text = editHeadline;
        if (textTMP != null && !isTyping)
            textTMP.text = editBody;

        // CATATAN: profil foto TIDAK diubah di sini.
        // Dikelola oleh ShowLine() via _displayedProfile agar OnValidate
        // tidak bisa menimpa foto yang sedang tampil.
    }

    [System.NonSerialized] bool _inOnValidate;
    void OnValidate()
    {
        // CATATAN: jangan panggil ApplyLayoutAsset() di sini.
        // Sinkronisasi dari aset DialogBoxLayout adalah satu arah:
        // DialogBoxLayout.OnValidate → push ke komponen. Memanggil balik
        // di komponen menyebabkan loop tak terbatas (StackOverflow).
        if (_inOnValidate) return;
        _inOnValidate = true;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) { _inOnValidate = false; return; }
            ApplyLayout();
            _inOnValidate = false;
        };
#else
        ApplyLayout();
        _inOnValidate = false;
#endif
    }

    /// <summary>Salin field DialogBoxLayout ke field lokal jika layout di-assign.</summary>
#if UNITY_EDITOR
    [ContextMenu("▶ Sync sekarang dari Layout")]
    void SyncFromLayoutMenu()
    {
        ApplyLayoutAsset();
        UnityEditor.EditorUtility.SetDirty(this);
        if (!Application.isPlaying)
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[NpcDialog] Disinkron dari layout: " + (layout != null ? layout.name : "<null>"));
    }
#endif
    public void ApplyLayoutAsset()
    {
        if (layout == null) return;
        if (layout.boxSprite        != null) dialogBoxSprite  = layout.boxSprite;
        if (layout.nameBannerSprite != null) nameBannerSprite = layout.nameBannerSprite;

        panelCenterX    = layout.panelCenterX;
        panelCenterY    = layout.panelCenterY;
        panelWidthFrac  = layout.panelWidthFrac;
        panelHeightFrac = layout.panelHeightFrac;

        portraitCenterX        = layout.portraitCenterX;
        portraitCenterY        = layout.portraitCenterY;
        portraitSizeW          = layout.portraitSizeW;
        portraitSizeH          = layout.portraitSizeH;
        portraitPreserveAspect = layout.portraitPreserveAspect;

        bannerAnchorMin = layout.bannerAnchorMin;
        bannerAnchorMax = layout.bannerAnchorMax;
        textAnchorMin   = layout.textAnchorMin;
        textAnchorMax   = layout.textAnchorMax;

        hintCenterX = layout.hintCenterX;
        hintCenterY = layout.hintCenterY;
        hintSizeW   = layout.hintSizeW;
        hintSizeH   = layout.hintSizeH;
    }

#if UNITY_EDITOR
    /// <summary>One-click setup Cara B — sama dengan tombol di Day1Intro.</summary>
    [ContextMenu("▶ Buat + Assign DialogBoxLayout (One-Click Cara B)")]
    void CreateAndAssignLayoutAsset()
    {
        const string assetPath = "Assets/DialogLayoutDefault.asset";
        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<DialogBoxLayout>(assetPath);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<DialogBoxLayout>();
            UnityEditor.AssetDatabase.CreateAsset(asset, assetPath);
            Debug.Log("[NpcDialog] Aset baru dibuat: " + assetPath);
        }

        var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/UI day 1/8.png");
        if (sp != null) asset.boxSprite = sp;
        asset.ResetToPreset8();

        layout = asset;
        ApplyLayoutAsset();

        UnityEditor.EditorUtility.SetDirty(asset);
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();

        Debug.Log("[NpcDialog] Layout di-assign: " + assetPath +
                  ". Drag aset ini ke field 'Layout' pada Day1Intro & DialogManager juga.");
    }
#endif

    // Tampilkan sprite di kotak potret
    void ApplyProfile(Sprite spr)
    {
        if (profileImg == null)
        {
            Debug.LogWarning("[NpcDialog] profileImg NULL — UI belum dibangun? Panggil Play() dulu.");
            return;
        }
        profileImg.enabled = true; // selalu aktif
        profileImg.preserveAspect = portraitPreserveAspect;   // terapkan setiap ganti sprite
        if (spr != null)
        {
            profileImg.sprite = spr;
            profileImg.color  = Color.white;
            Debug.Log("[NpcDialog] ApplyProfile ✓ sprite: '" + spr.name + "'");
        }
        else
        {
            profileImg.sprite = null;
            // Tetap tampil sebagai placeholder abu-abu agar area portrait kelihatan
            profileImg.color  = new Color(0.15f, 0.15f, 0.15f, 0.7f);
            Debug.Log("[NpcDialog] ApplyProfile: sprite NULL — tampil placeholder abu-abu. " +
                      "Drag sprite ke field 'Edit Profile' di Inspector.");
        }
    }

    [ContextMenu(">>> TEST: Force Tampilkan Profil Sekarang")]
    void ForceShowProfile()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[NpcDialog] Hanya bisa dipakai saat Play Mode."); return; }
        // Gunakan _displayedProfile (runtime) dulu, lalu fallback ke Inspector fields
        Sprite spr = _displayedProfile;
        if (spr == null) spr = editProfile;
        if (spr == null && lines != null && lines.Length > 0)
            spr = lines[currentIndex < lines.Length ? currentIndex : 0].profile;
        ApplyProfile(spr);
    }

    [ContextMenu(">>> DIAGNOSIS: Status Profil")]
    void DiagnosisProfile()
    {
        // Edit Mode: tampilkan state Inspector
        if (!Application.isPlaying)
        {
            string msg = "[NpcDialog DIAGNOSIS - Edit Mode]\n" +
                "  editProfile      : " + (editProfile      != null ? editProfile.name      : "NULL") + "\n" +
                "  dialogBoxSprite  : " + (dialogBoxSprite  != null ? dialogBoxSprite.name  : "NULL") + "\n" +
                "  lines count      : " + (lines != null ? lines.Length.ToString() : "NULL");
            if (lines != null)
                for (int i = 0; i < lines.Length; i++)
                    msg += "\n  lines[" + i + "].profile: " + (lines[i].profile != null ? lines[i].profile.name : "NULL");
            Debug.Log(msg);
            return;
        }
        // Play Mode: tampilkan state runtime
        if (profileImg == null) { Debug.LogError("[NpcDialog DIAGNOSIS] profileImg == null! Panggil Play() dulu."); return; }
        var rt = profileImg.rectTransform;
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        Debug.Log(
            "[NpcDialog DIAGNOSIS - Play Mode]\n" +
            "  profileImg.enabled          : " + profileImg.enabled + "\n" +
            "  profileImg.color            : " + profileImg.color + "\n" +
            "  profileImg.sprite           : " + (profileImg.sprite != null ? profileImg.sprite.name : "NULL") + "\n" +
            "  profileImg GO activeInHier  : " + profileImg.gameObject.activeInHierarchy + "\n" +
            "  panelRoot activeInHier      : " + panelRoot.activeInHierarchy + "\n" +
            "  RT world corners BL→TR      : " + corners[0] + " → " + corners[2] + "\n" +
            "  editProfile                 : " + (editProfile != null ? editProfile.name : "NULL") + "\n" +
            "  currentIndex                : " + currentIndex + "\n" +
            "  lines[currentIndex].profile : " + (lines != null && currentIndex < lines.Length && lines[currentIndex].profile != null ? lines[currentIndex].profile.name : "NULL")
        );
    }

    // ══════════════════════════════════════════════════════════════════════
    // INTERNAL — bangun UI otomatis
    // ══════════════════════════════════════════════════════════════════════

    void BuildUIIfNeeded()
    {
        if (panelRoot != null) return;

        // ── 1) Canvas ──────────────────────────────────────────────────────
        var canvasGO = new GameObject("NpcDialogCanvas");
        DontDestroyOnLoad(canvasGO);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── EventSystem — wajib agar tombol pilihan merespon klik ──────────
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            DontDestroyOnLoad(esGO);
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ── 2) Panel utama ─────────────────────────────────────────────────
        panelRoot = new GameObject("Panel");
        panelRoot.transform.SetParent(canvas.transform, false);
        panelRT      = panelRoot.AddComponent<RectTransform>();
        var panelImg = panelRoot.AddComponent<Image>();

        bool hasBox = (dialogBoxSprite != null);

        if (hasBox)
        {
            // Anchor-based sizing — sistem sama persis dengan PrologScreen
            // panelCenterX/Y dan panelWidthFrac/HeightFrac mengontrol posisi & ukuran
            panelRT.anchorMin = new Vector2(
                panelCenterX - panelWidthFrac  * 0.5f,
                panelCenterY - panelHeightFrac * 0.5f);
            panelRT.anchorMax = new Vector2(
                panelCenterX + panelWidthFrac  * 0.5f,
                panelCenterY + panelHeightFrac * 0.5f);
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            panelImg.sprite         = dialogBoxSprite;
            panelImg.type           = Image.Type.Simple;
            panelImg.preserveAspect = false;   // stretch mengisi panel (sama seperti PrologScreen)
            panelImg.color          = Color.white;
        }
        else
        {
            // Fallback tanpa sprite — pakai bottomMargin + sizeDelta
            float margin = (1f - Mathf.Clamp01(panelWidthRatio)) * 0.5f;
            if (showAtTop)
            {
                panelRT.anchorMin        = new Vector2(margin, 1f);
                panelRT.anchorMax        = new Vector2(1f - margin, 1f);
                panelRT.pivot            = new Vector2(0.5f, 1f);
                panelRT.anchoredPosition = new Vector2(0f, -bottomMargin);
            }
            else
            {
                panelRT.anchorMin        = new Vector2(margin, 0f);
                panelRT.anchorMax        = new Vector2(1f - margin, 0f);
                panelRT.pivot            = new Vector2(0.5f, 0f);
                panelRT.anchoredPosition = new Vector2(0f, bottomMargin);
            }
            panelRT.sizeDelta = new Vector2(0f, panelHeight);
            panelImg.color    = panelColor;

            var outline = panelRoot.AddComponent<Outline>();
            outline.effectColor    = borderColor;
            outline.effectDistance = new Vector2(3f, -3f);
        }

        // ── 3) Potret karakter ─────────────────────────────────────────────
        // SATU Image langsung di GO ini — tidak ada nested child untuk menghindari
        // masalah layer/rendering yang tidak terlihat.
        var profileGO = new GameObject("Profile");
        profileGO.transform.SetParent(panelRoot.transform, false);
        profileRT  = profileGO.AddComponent<RectTransform>();
        profileImg = profileGO.AddComponent<Image>();
        profileImg.preserveAspect = portraitPreserveAspect;   // baca dari Inspector
        profileImg.raycastTarget  = false;

        if (hasBox)
        {
            profileRT.anchorMin = new Vector2(
                portraitCenterX - portraitSizeW * 0.5f,
                portraitCenterY - portraitSizeH * 0.5f);
            profileRT.anchorMax = new Vector2(
                portraitCenterX + portraitSizeW * 0.5f,
                portraitCenterY + portraitSizeH * 0.5f);
            profileRT.offsetMin = new Vector2(4f, 4f);
            profileRT.offsetMax = new Vector2(-4f, -4f);
            // Kotak abu-abu placeholder — selalu terlihat agar mudah di-debug posisinya
            profileImg.color    = new Color(0.15f, 0.15f, 0.15f, 0.7f);
        }
        else
        {
            profileImg.color = profileBgColor;
            if (profileOnRight)
            {
                profileRT.anchorMin = profileRT.anchorMax = new Vector2(1f, 0.5f);
                profileRT.pivot     = new Vector2(1f, 0.5f);
                profileRT.anchoredPosition = new Vector2(-16f, 0f);
            }
            else
            {
                profileRT.anchorMin = profileRT.anchorMax = new Vector2(0f, 0.5f);
                profileRT.pivot     = new Vector2(0f, 0.5f);
                profileRT.anchoredPosition = new Vector2(16f, 0f);
            }
            profileRT.sizeDelta = new Vector2(profileSize, profileSize);
        }

        // ── 4) Banner nama pembicara ───────────────────────────────────────
        // Lencana kecil berujung diamond di atas area teks (sesuai sprite kayu)
        GameObject speakerParent;
        if (hasBox)
        {
            var bannerGO  = new GameObject("NameBanner");
            bannerGO.transform.SetParent(panelRoot.transform, false);
            bannerRT      = bannerGO.AddComponent<RectTransform>();
            bannerImg     = bannerGO.AddComponent<Image>();

            if (nameBannerSprite != null)
            {
                bannerImg.sprite         = nameBannerSprite;
                bannerImg.type           = Image.Type.Simple;
                bannerImg.preserveAspect = false;
                bannerImg.color          = Color.white;
            }
            else
            {
                // Simulasi banner: latar emas semi-transparan
                bannerImg.color = new Color(
                    borderColor.r, borderColor.g, borderColor.b, 0.30f);
            }
            bannerImg.enabled = showBannerBg;

            bannerRT.anchorMin = bannerAnchorMin;
            bannerRT.anchorMax = bannerAnchorMax;
            bannerRT.offsetMin = Vector2.zero;
            bannerRT.offsetMax = Vector2.zero;
            speakerParent = bannerGO;
        }
        else
        {
            speakerParent = panelRoot;
        }

        // ── 5) Nama pembicara (text di dalam banner / panel) ───────────────
        var speakerGO = new GameObject("SpeakerName");
        speakerGO.transform.SetParent(speakerParent.transform, false);
        var speakerRT = speakerGO.AddComponent<RectTransform>();
        speakerTMP = speakerGO.AddComponent<TextMeshProUGUI>();
        ApplyFont(speakerTMP);
        speakerTMP.fontSize  = speakerFontSize;
        speakerTMP.color     = speakerColor;
        speakerTMP.fontStyle = FontStyles.Bold;

        if (hasBox)
        {
            // Isi penuh banner — rata tengah (horizontal + vertikal)
            speakerTMP.alignment = TextAlignmentOptions.Center;
            speakerRT.anchorMin  = Vector2.zero;
            speakerRT.anchorMax  = Vector2.one;
            speakerRT.offsetMin  = new Vector2( 8f,  2f);
            speakerRT.offsetMax  = new Vector2(-8f, -2f);
        }
        else
        {
            float lp = profileOnRight ? 24f : (profileSize + 32f);
            float rp = profileOnRight ? (profileSize + 32f) : 24f;
            speakerTMP.alignment = TextAlignmentOptions.TopLeft;
            speakerRT.anchorMin  = new Vector2(0f, 1f);
            speakerRT.anchorMax  = new Vector2(1f, 1f);
            speakerRT.pivot      = new Vector2(0f, 1f);
            speakerRT.offsetMin  = new Vector2(lp,  -52f);
            speakerRT.offsetMax  = new Vector2(-rp,  -8f);
        }

        // ── 6) Teks isi dialog ─────────────────────────────────────────────
        var bodyGO = new GameObject("BodyText");
        bodyGO.transform.SetParent(panelRoot.transform, false);
        bodyRT  = bodyGO.AddComponent<RectTransform>();
        textTMP = bodyGO.AddComponent<TextMeshProUGUI>();
        ApplyFont(textTMP);
        textTMP.fontSize           = textFontSize;
        textTMP.color              = textColor;
        textTMP.alignment          = TextAlignmentOptions.TopLeft;
        textTMP.enableWordWrapping = true;

        if (hasBox)
        {
            bodyRT.anchorMin = textAnchorMin;
            bodyRT.anchorMax = textAnchorMax;
            bodyRT.offsetMin = new Vector2( 4f,  4f);
            bodyRT.offsetMax = new Vector2(-4f, -4f);
        }
        else
        {
            float lp = profileOnRight ? 24f : (profileSize + 32f);
            float rp = profileOnRight ? (profileSize + 32f) : 24f;
            bodyRT.anchorMin = new Vector2(0f, 0f);
            bodyRT.anchorMax = new Vector2(1f, 1f);
            bodyRT.offsetMin = new Vector2(lp,  32f);
            bodyRT.offsetMax = new Vector2(-rp, -56f);
        }

        // ── 7) Petunjuk lanjut (pojok kanan bawah) ────────────────────────
        var hintGO = new GameObject("Hint");
        hintGO.transform.SetParent(panelRoot.transform, false);
        hintRT  = hintGO.AddComponent<RectTransform>();
        hintTMP = hintGO.AddComponent<TextMeshProUGUI>();
        ApplyFont(hintTMP);
        hintTMP.fontSize  = hintFontSize;
        hintTMP.color     = hintColor;
        hintTMP.alignment = TextAlignmentOptions.BottomRight;
        hintTMP.text      = continueHint;

        if (hasBox)
        {
            hintRT.anchorMin = new Vector2(hintCenterX - hintSizeW * 0.5f, hintCenterY - hintSizeH * 0.5f);
            hintRT.anchorMax = new Vector2(hintCenterX + hintSizeW * 0.5f, hintCenterY + hintSizeH * 0.5f);
            hintRT.offsetMin = Vector2.zero;
            hintRT.offsetMax = Vector2.zero;
        }
        else
        {
            float lp = profileOnRight ? 24f : (profileSize + 32f);
            float rp = profileOnRight ? (profileSize + 32f) : 24f;
            hintRT.anchorMin = new Vector2(0f, 0f);
            hintRT.anchorMax = new Vector2(1f, 0f);
            hintRT.pivot     = new Vector2(1f, 0f);
            hintRT.offsetMin = new Vector2(lp,  4f);
            hintRT.offsetMax = new Vector2(-rp, 30f);
        }

        panelRoot.SetActive(false);
        Debug.Log("[NpcDialog] UI berhasil dibuat. Canvas: NpcDialogCanvas");
    }

    // Pastikan font ada — tanpa font, TMP hanya tampilkan kotak "T"
    void ApplyFont(TextMeshProUGUI tmp)
    {
        TMP_FontAsset f = fontAsset;
        if (f == null) f = TMP_Settings.defaultFontAsset;
        if (f == null) f = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (f != null)
            tmp.font = f;
        else
            Debug.LogWarning("[NpcDialog] Font tidak ditemukan! Drag font ke field 'Font Asset' di komponen NpcDialog.");
    }
}
