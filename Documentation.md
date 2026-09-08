# Seybey10 Project Documentation

## Overview
Game story-driven inspection game inspired by *Papers, Please*. Player acts as an immigration officer checking NPC passports at an evacuation desk.

## Architecture

### Core Systems

#### 1. Save System (`Assets/Scripts/Game/SaveSystem/`)
- **SaveManager.cs** — Singleton, handles JSON save/load to `Application.persistentDataPath/game_save.json`
- **GameSaveState.cs** — Serializable schema containing `currentDay`, `playerCash`, `purchasedTickets`, ticket purchase status per character (Aldo/Nasya/Virly/Kraisa), `calculatedEndingID`
- Full JSON serialization, no encryption
- Saves only when shift ends (via `MulaiHariBaru`) or ticket purchased

#### 2. Economy System (`Assets/Scripts/Game/Scene Koper/EconomyManager.cs`)
- Variables: `money`, `trust` (no PlayerPrefs)
- Trust resets to 100 at every scene start
- Trust = 0 triggers LoseCondition
- SaveGame called only on shift end (MulaiHariBaru)

#### 3. Lose Condition (`Assets/Scripts/Game/LoseCondition.cs`)
- Triggers when `trust <= 0`
- Fade-in CanvasGroup overlay (1.5s)
- Hold Space to restart day (reset trust to 100, reset day to 1)
- Progress bar shows skip timer

#### 4. Violation System (`Assets/Scripts/Game/ViolationSystem/`)
- **RuleViolation.cs** — Abstract base class + 8 concrete violation types
  - Base: `ExpiredPassportViolation`, `InfoMismatchViolation`, `StandardViolation`, `BriberyViolation` (NewRule_Day2)
  - Day 3: `NewRule_Day3` (Suspicious Origin)
  - Day 5: `NewRule_Day5` (Counterfeit Document)
  - Day 7: `NewRule_Day7` (Blacklist Entry)
  - Day 9: `NewRule_Day9` (Mutation Marker)
- **DayRules.cs** — Rule accumulation per day (Day 2 = 4 rules, Day 5 = 6 rules, etc.)
- **ViolationSystem.cs** — 40% chance NPC gets 1-3 random violations
- Applies to PassportSchema fields (expiryDate, countryName, districtHome, sameOwnerPhoto, isValid, hexaCardColor, documentNumber, ownerName)

#### 5. Procedural NPC Face (`Assets/Scripts/Game/ProceduralNPC/ProceduralFace.cs`)
- Gender enum: Man, Woman
- Separate FaceContainer for each gender
- SetRandColor() with realistic color presets:
  - Skin tone (7 shades), Hair (8 colors), Eyes (7 colors), Lip (6 colors)
  - Clothes, Shoes, Accessories — each with their own palette
- Body part renderers: baseFace, hair, eyes, nose, mouth, bodyClothes, shoes, accessories

#### 6. Story System (`Assets/Scripts/Game/StoryManager.cs`)
- **StorySchema.cs** — ScriptableObject with Day, preDayText, onDeskText, afterShiftText
- **TypingText.cs** — Typewriter effect component
  - Per-line typing
  - [Effect] tag for wobble animation
  - Variable typing speed (base + random variation)
  - Space hold to skip (3 seconds + progress bar)
  - Events: OnLineComplete, OnAllComplete

#### 7. Game Event System (`Assets/Scripts/Lib/GameEvent.cs`)
- Static events: `GenerateNewPassportData`, `GenerateNewNPCView`, `SpawnNPCOnStartDay`, `DeleteMarkTicket`, `OnSetGander`

#### 8. Game Manager (`Assets/Scripts/Game/GameManager.cs`)
- Singleton with `currentDay` tracking
- Scene management: `ResetDay()`, `EndShiftAndLoadScene()`, `NextDay()`

## Scene Flow
```
PreDay (story intro) → MainDesk (shift starts) → AfterShift (end) → RecapDay
                                                                      ↓
                                                              Next Day → PreDay
```

## Data Schema (Schema.cs)
- `NPCPassengerRuntimeData` — Runtime NPC data with passport, boardingPass, luggage, violationReason
- `PassportSchema` — Document fields (documentNumber, ownerName, sex, bodOwner, expiryDate, countryName, districtHome, isValid, sameOwnerPhoto, hexaCardColor)
- `BoardingPassSchema` — Ticket data (passNumber, destination, seatClass, isValid)
- `LuggageItemData` — Item data for koper inspection

## ScriptableObject Assets
- `DayRuleData` — Baggage rules per day
- `StorySchema` — Story text per day
- `NamePool` (ID, CH, JP) — Name databases for NPC generation

## File Structure
```
Assets/Scripts/
├── Lib/
│   ├── Schema.cs          — Data schemas
│   ├── GameEvent.cs       — Event system
├── SO/
│   ├── StorySchema.cs     — Story ScriptableObject
│   ├── DayRuleData.cs     — Day rules ScriptableObject
│   ├── NamePool*.asset    — Name databases
├── Game/
│   ├── GameManager.cs     — Game state manager
│   ├── Timer.cs           — Timer system
│   ├── LoseCondition.cs   — Lose condition
│   ├── SaveSystem/
│   │   ├── SaveManager.cs — JSON save/load
│   │   └── GameSaveState.cs — Save schema
│   ├── ViolationSystem/
│   │   ├── RuleViolation.cs    — Abstract + 8 types
│   │   ├── DayRuleSet.cs       — Day inheritance chain
│   │   └── ViolationSystem.cs  — 40% chance generator
│   ├── ProceduralNPC/
│   │   └── ProceduralFace.cs  — NPC face generation
│   ├── Scene Cek Pasport/
│   │   ├── PassportScript.cs  — Passport generation & display
│   │   ├── NpcEntranceManager.cs — NPC spawn & animation
│   │   ├── StampHybridController.cs — Stamp approval
│   │   ├── TicketScript.cs    — Ticket handling
│   │   └── DokumenPersistent.cs — Document position persistence
│   ├── Scene Koper/
│   │   ├── EconomyManager.cs  — Money & trust
│   │   ├── LuggageManager.cs  — Koper item management
│   │   ├── RekapHarian.cs     — Daily recap UI
│   │   └── EconomyManager.cs  — Economic system
│   ├── StoryManager.cs        — Story flow controller
├── UI/
│   └── TypingText.cs          — Typewriter text effect
```

## Key Design Decisions
1. **No PlayerPrefs** for core game data — uses variables + JSON save
2. **Violations accumulate per day** — each day inherits previous rules + adds new
3. **40% violation chance** — 60% NPCs have clean passports
4. **Typewriter per line** — not entire text at once
5. **Space hold skip** — 3 second hold with progress bar for quick skip
6. **StorySchema as ScriptableObject** — easy data editing in Inspector
