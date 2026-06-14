a---
name: Unity Rara Game Guide
description: >
  Use this agent when building "RARA: Jaga Dirimu!" in Unity. Guides implementation
  of all game mechanics: Day1 side-scroll + encounters, Day2 angkot choices, Day3 boss fight,
  dialogue system with AMAN/RAGU/BAHAYA choices, GameState singleton, HUD (lives/score),
  voice meter (shout button), scene transitions, and educational card screens.
  Pick this over the default agent when asking about game flow, scene setup, encounter design,
  boss fight logic, choice scoring, or any scripts inside Assets/Scripts/.
model: claude-sonnet-4-5
tools:
  - read_file
  - replace_string_in_file
  - create_file
  - multi_replace_string_in_file
  - file_search
  - grep_search
  - get_errors
---

# RARA: Jaga Dirimu! — Unity Game Development Agent

## Game Overview

Educational 2D side-scrolling game about child safety (keselamatan anak). The player guides
Rara through 3 days of dangerous situations, making AMAN/RAGU/BAHAYA dialogue choices.

**Original**: Phaser 3 web game at https://game-jaga-diri.vercel.app/
**Target**: Unity 2D (2022+), TextMeshPro available, 2D Physics, Sprites from Assets/sprites/

## Scene Flow

```
MainMenu → Prolog1 → Day1 → Result1
                  → Prolog2 → Day2 → Result2
                           → Prolog3 → Day3 → Result3 → Ending
                                    (any day) → GameOver
```

## Core Systems (all in Assets/Scripts/)

| Script                | Purpose                                                                               |
| --------------------- | ------------------------------------------------------------------------------------- |
| `GameState.cs`        | Singleton: lives(3), score, day, choices[], achievements[], DontDestroyOnLoad         |
| `DialogManager.cs`    | Show dialogue lines with speaker/text, AMAN/RAGU/BAHAYA choice buttons                |
| `AudioManager.cs`     | BGM (menu/day1/day2/day3/boss) + SFX (click/correct/wrong/bosshit), DontDestroyOnLoad |
| `SceneLoader.cs`      | Fade-in/out transitions, LoadScene wrapper                                            |
| `HUDManager.cs`       | Hearts (lives), score text, location name banner                                      |
| `player.cs`           | Movement: walk/run sprites, Rigidbody2D velocity (already exists!)                    |
| `Day1Controller.cs`   | Encounter state machine for Day 1 side-scroller                                       |
| `Day2Controller.cs`   | Angkot scene: seat choice, screenshot mini-game, chat encounters                      |
| `Day3Controller.cs`   | Rain walk → plat check → boss fight                                                   |
| `BossFightManager.cs` | 5-round boss dialogue, voice/shout meter, panic button, rescue                        |

## Score System

- AMAN = 100 pts (safe choice)
- RAGU = 50 pts (hesitant choice)
- BAHAYA = 0 pts + lose 1 life (dangerous choice)
- LAPOR = 500 pts (reporting to authority)
- Max ~1000 pts total across 3 days
- Grade: ≥80% = "★ PAHLAWAN SEJATI ★", ≥60% = "Sang Jagoan", ≥40% = "Si Pemberani"

## Day 1 — Side-Scrolling Walk to School

- World width: 3200 units, camera follows Rara
- Phases: intro → tutorial → walking → encounter1 → path_choice → walking2 → encounter2 → walking3 → encounter3 → educard → complete
- **path_choice**: Safe road (lighted, crowded) vs Dangerous alley (shortcut, dark)
- **NPC Approach**: Stranger slowly approaches; player can shout (Space/button) to push back
- **Voice mechanic**: Hold TERIAK button → fills gauge → stranger backs away

## Day 2 — Inside Angkot (Minibus)

- Static scene (no scrolling), 5 seat positions
- Phases: boarding → seat_choice → riding → encounter_msg → screenshot_choice → encounter_npc → result
- **Seat choice**: Near driver = safe, far back = risky
- **Chat encounter**: Stranger sends suspicious messages → player must screenshot + block
- **NPC encounter**: Stranger gets too close → shout or move away

## Day 3 — School Parking Lot (Rain)

- Rain walk intro → plat check mini-game → boss approach → boss fight → rescue → edu card
- **Plat check**: 3 license plate options, 1 correct (DD 3472 WK)
- **Boss fight**: 5 rounds, each with 3 choices (AMAN/RAGU/BAHAYA)
  - AMAN choice: reduces bossMental by 0.25–0.35
  - BAHAYA choice: lose 1 life, no damage to boss
  - RAGU choice: minor damage (0.10)
  - Shout held for 5s = auto -35% boss mental
- **Panic button**: Appears on isPanic rounds (round 4 & 5) → triggers rescue scene
- **Boss mental bar**: Visible health bar depletes to 0 → victory

## Dialogue System Design

```csharp
// DialogLine structure:
// { speaker, portrait, text, choices[] }
// choices: { label, category, onSelect }
// Color: AMAN=green, RAGU=yellow, BAHAYA=red
```

## Unity Scene Setup Notes

- Use Unity UI Canvas (Screen Space - Overlay) for HUD and dialogue
- DialogPanel: Panel → SpeakerText (TMP) + DialogText (TMP) + PortraitImage + ContinueButton
- ChoicePanel: VerticalLayoutGroup with dynamically spawned ChoiceButton prefabs
- HUD: Top-left hearts (3x Image), top-right score TMP text
- Day1/Day3: Camera with Cinemachine or Camera.main.Follow
- Day2: Fixed camera, UI-style scene

## Key Implementation Rules

1. Always check `GameState.Instance.IsAlive()` after BAHAYA choices
2. All scene transitions use `SceneLoader.Instance.LoadScene(name)`
3. All choice scoring uses `GameState.Instance.AddChoice(day, label, category)`
4. DialogManager is placed on a persistent Canvas in each scene
5. AudioManager and GameState persist via DontDestroyOnLoad (create in first scene / Boot)

## File Structure

```
Assets/Scripts/
  player.cs              ← Already done! Walk/Run movement
  GameState.cs           ← Singleton state
  DialogManager.cs       ← Dialogue UI
  AudioManager.cs        ← BGM + SFX
  SceneLoader.cs         ← Transitions
  HUDManager.cs          ← UI hearts + score
  Day1Controller.cs      ← Day 1 game logic
  Day2Controller.cs      ← Day 2 game logic
  Day3Controller.cs      ← Day 3 game logic
  BossFightManager.cs    ← Boss fight system
  VoiceMeter.cs          ← Shout/mic input
Assets/Scenes/
  MainMenu.unity
  Prolog1.unity
  Gameplay.unity         ← rename to Day1.unity
  Day2.unity
  Day3.unity
  Result.unity           ← shared result scene
  GameOver.unity
  Ending.unity
```
