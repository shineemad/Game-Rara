using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Mengontrol alur Hari 1: Jalan Kaki ke Sekolah.
/// Mesin state side-scroller + sistem encounter.
///
/// Phase: intro → tutorial → walking → encounter1 → path_choice
///        → walking2 → encounter2 → walking3 → encounter3 → educard → complete
///
/// Referensi dari: js/scenes/Day1.js versi web asli
///
/// Setup di Inspector:
///   player          → GameObject player (ada script player.cs)
///   dialogManager   → DialogManager di scene ini
///   hudManager      → HUDManager
///   pathChoicePanel → Panel UI pemilihan jalan (aman vs berbahaya)
///   npcObject       → GameObject NPC asing (siluet)
///   eduCardPanel    → Panel kartu edukasi akhir
///   ... (lihat komentar tiap field)
/// </summary>
public class Day1Controller : MonoBehaviour
{
    // ── Referensi ──────────────────────────────────────────────────────────
    [Header("Referensi Utama")]
    public GameObject   player;
    public DialogManager dialogManager;
    public HUDManager   hudManager;

    [Header("NPC Asing")]
    public GameObject npcStranger;      // siluet NPC berbahaya
    public float      npcApproachSpeed = 0.8f;
    public float      npcSafeDistance  = 4f;   // jarak aman (unit)
    public float      npcDangerDist    = 1.5f; // jarak bahaya

    [Header("Jalur")]
    public Transform  pathSafeMarker;     // titik masuk jalan aman
    public Transform  pathDangerMarker;   // titik masuk gang sepi
    public GameObject pathChoicePanel;    // Panel UI pilihan jalan

    [Header("Zona Encounter (X position di world)")]
    public float encTutorial  = 5f;
    public float encE1        = 14f;
    public float encPathChoice = 18f;
    public float encE2        = 26f;
    public float encE3        = 34f;
    public float encEduCard   = 42f;
    public float encEnd       = 52f;

    [Header("Panel Edu Card & Game Over")]
    public GameObject eduCardPanel;
    public Button     eduCardContinueBtn;

    [Header("Tombol Teriak (untuk yang tidak pakai mic)")]
    public Button  shoutButton;
    public Slider  shoutGauge;          // gauge teriak (0–1)
    public float   shoutFillRate = 0.5f;
    public float   shoutDecayRate = 0.3f;

    // ── State Machine ──────────────────────────────────────────────────────
    enum Phase
    {
        Intro, Tutorial, Walking,
        Encounter1, PathChoice,
        Walking2, Encounter2,
        Walking3, Encounter3,
        EduCard, Complete
    }

    Phase   currentPhase = Phase.Intro;
    bool    dialogActive = false;
    bool    pathChosen   = false;
    bool    npcActive    = false;
    float   shoutLevel   = 0f;
    bool    shoutHeld    = false;
    bool    enc1Done     = false;
    bool    enc2Done     = false;
    bool    enc3Done     = false;

    // ══════════════════════════════════════════════════════════════════════
    void Start()
    {
        AudioManager.Instance?.PlayBGM(AudioManager.BGMTrack.Day1);
        GameState.Instance.day = 1;
        hudManager?.Refresh();

        if (npcStranger != null) npcStranger.SetActive(false);
        if (pathChoicePanel != null) pathChoicePanel.SetActive(false);
        if (eduCardPanel != null) eduCardPanel.SetActive(false);
        if (shoutGauge != null)   shoutGauge.value = 0f;

        // Pasang event tombol teriak
        if (shoutButton != null)
        {
            var trigger = shoutButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            AddTrigger(trigger, UnityEngine.EventSystems.EventTriggerType.PointerDown, _ => shoutHeld = true);
            AddTrigger(trigger, UnityEngine.EventSystems.EventTriggerType.PointerUp,   _ => shoutHeld = false);
        }

        StartCoroutine(ShowIntro());
    }

    // ══════════════════════════════════════════════════════════════════════
    void Update()
    {
        if (dialogActive) return;

        HandleShout();
        CheckEncounterTriggers();

        if (currentPhase == Phase.Encounter1 && npcActive) HandleNPCApproach();
        if (currentPhase == Phase.Encounter2 && npcActive) HandleNPCApproach();
        if (currentPhase == Phase.Encounter3 && npcActive) HandleNPCApproach();

        hudManager?.UpdateScore(GameState.Instance?.score ?? 0);
    }

    // ══════════════════════════════════════════════════════════════════════
    // INTRO & TUTORIAL
    // ══════════════════════════════════════════════════════════════════════

    IEnumerator ShowIntro()
    {
        dialogActive = true;
        var lines = new List<DialogManager.DialogLine>
        {
            new DialogManager.DialogLine
            {
                speaker  = "Narasi",
                portrait = "narasi",
                text     = "Pagi hari di Kota Makassar...\nRara bersiap berangkat ke sekolah sendirian."
            },
            new DialogManager.DialogLine
            {
                speaker  = "Rara",
                portrait = "rara",
                text     = "Hari ini Mama dan Papa kerja dari pagi.\nAku harus berangkat sendiri... semoga aman! 😊"
            }
        };

        dialogManager.Show(lines, () =>
        {
            dialogActive = false;
            currentPhase = Phase.Tutorial;
        });

        yield return null;
    }

    IEnumerator ShowTutorial()
    {
        dialogActive = true;
        var lines = new List<DialogManager.DialogLine>
        {
            new DialogManager.DialogLine
            {
                speaker  = "Narasi",
                portrait = "narasi",
                text     = "TUTORIAL: Gunakan tombol panah / WASD untuk bergerak.\nTahan Shift untuk berlari!"
            },
            new DialogManager.DialogLine
            {
                speaker  = "Narasi",
                portrait = "narasi",
                text     = "Jika ada orang asing mendekati:\nTekan dan tahan tombol 🔊 TERIAK untuk mengusirnya!"
            }
        };

        dialogManager.Show(lines, () =>
        {
            dialogActive = false;
            currentPhase = Phase.Walking;
        });

        yield return null;
    }

    // ══════════════════════════════════════════════════════════════════════
    // ENCOUNTER TRIGGERS
    // ══════════════════════════════════════════════════════════════════════

    void CheckEncounterTriggers()
    {
        if (player == null) return;
        float px = player.transform.position.x;

        switch (currentPhase)
        {
            case Phase.Tutorial:
                if (px >= encTutorial)
                    StartCoroutine(ShowTutorial());
                break;

            case Phase.Walking:
                if (px >= encE1 && !enc1Done)
                    StartEncounter1();
                else if (px >= encPathChoice && enc1Done)
                    ShowPathChoice();
                break;

            case Phase.Walking2:
                if (px >= encE2 && !enc2Done)
                    StartEncounter2();
                break;

            case Phase.Walking3:
                if (px >= encE3 && !enc3Done)
                    StartEncounter3();
                else if (px >= encEduCard && enc3Done)
                    StartCoroutine(ShowEduCard());
                break;
        }
    }

    // ── Encounter 1: Orang Asing di Jalan ─────────────────────────────────
    void StartEncounter1()
    {
        if (enc1Done) return;
        enc1Done = true;
        currentPhase = Phase.Encounter1;
        dialogActive = true;

        if (npcStranger != null)
        {
            npcStranger.SetActive(true);
            npcStranger.transform.position = new Vector3(
                player.transform.position.x + 5f,
                player.transform.position.y, 0f);
        }

        var lines = new List<DialogManager.DialogLine>
        {
            new DialogManager.DialogLine
            {
                speaker  = "Narasi",
                portrait = "narasi",
                text     = "Ada seseorang yang tidak dikenal berjalan mendekati Rara dari depan..."
            },
            new DialogManager.DialogLine
            {
                speaker  = "Orang Asing",
                portrait = "npc",
                text     = "\"Hei dek, sendirian? Mau ke mana? Ayo ikut aku, aku kasih tumpangan gratis~\""
            },
            new DialogManager.DialogLine
            {
                speaker  = "Rara",
                portrait = "rara",
                text     = "Apa yang harus Rara lakukan?",
                choices  = new DialogManager.Choice[]
                {
                    new DialogManager.Choice
                    {
                        label    = "\"TIDAK MAU! PERGI SANA!\" (Teriak keras!)",
                        category = "AMAN",
                        onSelect = () =>
                        {
                            GameState.Instance.EarnAchievement("Berani Menolak Asing");
                            npcActive = false;
                            if (npcStranger != null) npcStranger.SetActive(false);
                        }
                    },
                    new DialogManager.Choice
                    {
                        label    = "\"E-emm... nggak papa deh...\" (ragu-ragu)",
                        category = "RAGU",
                        onSelect = () => { npcActive = false; if (npcStranger != null) npcStranger.SetActive(false); }
                    },
                    new DialogManager.Choice
                    {
                        label    = "\"Oke...\" (mengikuti orang asing)",
                        category = "BAHAYA",
                        onSelect = () =>
                        {
                            npcActive = false;
                            if (!GameState.Instance.IsAlive())
                                SceneLoader.Instance?.LoadScene("GameOver");
                        }
                    }
                }
            }
        };

        dialogManager.Show(lines, () =>
        {
            dialogActive = false;
            npcActive = false;
            currentPhase = Phase.Walking;
            AudioManager.Instance?.Correct();
        });
    }

    // ── Path Choice: Jalan Aman vs Gang Sepi ──────────────────────────────
    void ShowPathChoice()
    {
        if (pathChosen) return;
        pathChosen = true;
        dialogActive = true;
        currentPhase = Phase.PathChoice;

        if (pathChoicePanel != null) pathChoicePanel.SetActive(true);
    }

    /// Dipanggil oleh tombol di pathChoicePanel.
    public void ChooseSafePath()
    {
        GameState.Instance.pathChoice = "safe";
        GameState.Instance.AddChoice(1, "Pilih jalan aman yang ramai", "AMAN");
        if (pathChoicePanel != null) pathChoicePanel.SetActive(false);
        dialogActive     = false;
        currentPhase     = Phase.Walking2;
        AudioManager.Instance?.Correct();
    }

    public void ChooseDangerPath()
    {
        GameState.Instance.pathChoice = "dangerous";
        GameState.Instance.AddChoice(1, "Pilih gang sepi sebagai jalan pintas", "BAHAYA");
        bool alive = GameState.Instance.LoseLife();
        hudManager?.FlashHeartLost(GameState.Instance.lives);

        if (pathChoicePanel != null) pathChoicePanel.SetActive(false);
        dialogActive = false;

        if (!alive)
        {
            SceneLoader.Instance?.LoadScene("GameOver");
            return;
        }

        // Tetap lanjut tapi dengan konsekuensi
        currentPhase = Phase.Walking2;
        StartCoroutine(ShowDangerPathWarning());
    }

    IEnumerator ShowDangerPathWarning()
    {
        dialogActive = true;
        var lines = new List<DialogManager.DialogLine>
        {
            new DialogManager.DialogLine
            {
                speaker  = "Narasi",
                portrait = "narasi",
                text     = "⚠ Gang ini sangat sepi dan gelap!\nRara kehilangan 1 ❤ karena pilihan berbahaya ini.\nJalan yang ramai jauh lebih aman!"
            }
        };
        dialogManager.Show(lines, () => dialogActive = false);
        yield return null;
    }

    // ── Encounter 2: Orang Asing di Gang ──────────────────────────────────
    void StartEncounter2()
    {
        if (enc2Done) return;
        enc2Done = true;
        currentPhase = Phase.Encounter2;
        dialogActive = true;

        ActivateNPCAt(player.transform.position.x + 4f);

        var lines = new List<DialogManager.DialogLine>
        {
            new DialogManager.DialogLine
            {
                speaker  = "Orang Asing",
                portrait = "npc",
                text     = "\"Eh kamu... sendirian nih? Boleh aku foto kamu?\nCantik sekali~\""
            },
            new DialogManager.DialogLine
            {
                speaker  = "Rara",
                portrait = "rara",
                text     = "Apa yang harus Rara lakukan?",
                choices  = new DialogManager.Choice[]
                {
                    new DialogManager.Choice
                    {
                        label    = "\"TIDAK BOLEH! TOLONG ADA ORANG ASING!\" (Teriak minta tolong)",
                        category = "AMAN",
                        onSelect = () =>
                        {
                            GameState.Instance.EarnAchievement("Tolak Difoto Asing");
                            DismissNPC();
                        }
                    },
                    new DialogManager.Choice
                    {
                        label    = "\"S-sebentar aja ya...\" (ragu dan bingung)",
                        category = "RAGU",
                        onSelect = () => DismissNPC()
                    },
                    new DialogManager.Choice
                    {
                        label    = "\"Oke...\" (diam saja membiarkan)",
                        category = "BAHAYA",
                        onSelect = () =>
                        {
                            DismissNPC();
                            if (!GameState.Instance.IsAlive())
                                SceneLoader.Instance?.LoadScene("GameOver");
                        }
                    }
                }
            }
        };

        dialogManager.Show(lines, () =>
        {
            dialogActive = false;
            currentPhase = Phase.Walking3;
        });
    }

    // ── Encounter 3: Pesan Mencurigakan di HP ─────────────────────────────
    void StartEncounter3()
    {
        if (enc3Done) return;
        enc3Done = true;
        currentPhase = Phase.Encounter3;
        dialogActive = true;

        var lines = new List<DialogManager.DialogLine>
        {
            new DialogManager.DialogLine
            {
                speaker  = "Narasi",
                portrait = "narasi",
                text     = "📱 HP Rara berbunyi! Ada pesan dari nomor tidak dikenal:\n\"Hei Rara, aku tau kamu lagi di jalan. Mau aku jemput?\""
            },
            new DialogManager.DialogLine
            {
                speaker  = "Rara",
                portrait = "rara",
                text     = "Pesan aneh... Apa yang harus Rara lakukan dengan pesan ini?",
                choices  = new DialogManager.Choice[]
                {
                    new DialogManager.Choice
                    {
                        label    = "Screenshot lalu blokir nomor dan cerita ke Mama",
                        category = "AMAN",
                        onSelect = () =>
                        {
                            GameState.Instance.screenshotTaken = true;
                            GameState.Instance.EarnAchievement("Screenshot & Laporkan");
                        }
                    },
                    new DialogManager.Choice
                    {
                        label    = "Balas: \"Siapa kamu?\" (penasaran)",
                        category = "RAGU",
                    },
                    new DialogManager.Choice
                    {
                        label    = "Ikuti ajakannya (sangat berbahaya!)",
                        category = "BAHAYA",
                        onSelect = () =>
                        {
                            if (!GameState.Instance.IsAlive())
                                SceneLoader.Instance?.LoadScene("GameOver");
                        }
                    }
                }
            }
        };

        dialogManager.Show(lines, () =>
        {
            dialogActive = false;
            enc3Done     = true;
        });
    }

    // ══════════════════════════════════════════════════════════════════════
    // EDU CARD
    // ══════════════════════════════════════════════════════════════════════

    IEnumerator ShowEduCard()
    {
        if (currentPhase == Phase.EduCard || currentPhase == Phase.Complete) yield break;
        currentPhase = Phase.EduCard;
        dialogActive = true;

        yield return new WaitForSeconds(0.5f);

        if (eduCardPanel != null) eduCardPanel.SetActive(true);
        if (eduCardContinueBtn != null)
            eduCardContinueBtn.onClick.AddListener(GoToResult);

        GameState.Instance.checkpointD1 = true;
    }

    public void GoToResult()
    {
        currentPhase = Phase.Complete;
        SceneLoader.Instance?.LoadScene("Result1");
    }

    // ══════════════════════════════════════════════════════════════════════
    // NPC APPROACH MECHANIC
    // ══════════════════════════════════════════════════════════════════════

    void ActivateNPCAt(float x)
    {
        if (npcStranger == null) return;
        npcStranger.SetActive(true);
        npcStranger.transform.position = new Vector3(x, player.transform.position.y, 0f);
        npcActive = true;
    }

    void DismissNPC()
    {
        npcActive = false;
        if (npcStranger != null) npcStranger.SetActive(false);
    }

    void HandleNPCApproach()
    {
        if (npcStranger == null || player == null) return;

        float dist = Vector3.Distance(npcStranger.transform.position, player.transform.position);

        // NPC mendekati pemain
        if (shoutLevel < 0.5f)
        {
            Vector3 dir = (player.transform.position - npcStranger.transform.position).normalized;
            npcStranger.transform.position += dir * npcApproachSpeed * Time.deltaTime;
        }
        else
        {
            // Teriak mengusir NPC
            Vector3 dir = (npcStranger.transform.position - player.transform.position).normalized;
            npcStranger.transform.position += dir * npcApproachSpeed * 2f * Time.deltaTime;
        }

        // Terlalu dekat → kehilangan nyawa
        if (dist < npcDangerDist && !dialogActive)
        {
            npcActive = false;
            npcStranger.SetActive(false);
            bool alive = GameState.Instance.LoseLife();
            GameState.Instance.AddChoice(1, "Diam saat didekati orang asing", "BAHAYA");
            hudManager?.FlashHeartLost(GameState.Instance.lives);
            if (!alive) SceneLoader.Instance?.LoadScene("GameOver");
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // SHOUT / TERIAK
    // ══════════════════════════════════════════════════════════════════════

    void HandleShout()
    {
        bool spaceheld = Input.GetKey(KeyCode.Space);
        bool isShout   = shoutHeld || spaceheld;

        if (isShout)
            shoutLevel = Mathf.Min(1f, shoutLevel + shoutFillRate * Time.deltaTime);
        else
            shoutLevel = Mathf.Max(0f, shoutLevel - shoutDecayRate * Time.deltaTime);

        if (shoutGauge != null) shoutGauge.value = shoutLevel;
    }

    // ══════════════════════════════════════════════════════════════════════
    // HELPER
    // ══════════════════════════════════════════════════════════════════════

    static void AddTrigger(
        UnityEngine.EventSystems.EventTrigger trigger,
        UnityEngine.EventSystems.EventTriggerType type,
        UnityEngine.Events.UnityAction<UnityEngine.EventSystems.BaseEventData> action)
    {
        var entry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }
}
