# Feature List — Seybay

Legend: **Implemented** = ada perilaku runtime yang jelas; **Partial** = ada fondasi tetapi belum lengkap/terintegrasi; **Design** = ada di GDD/narasi tetapi belum terbukti runtime; **Prototype** = ada di eksperimen atau test scene.

## Player-facing features

| Feature | Status | Ringkasan | Owner area |
|---|---|---|---|
| Home/menu | Implemented | Entry scene dan navigasi menu dasar | `UI/Home.cs`, `MenuManager.cs` |
| Pre-day story | Implemented | Story data ditampilkan dengan typewriter | `StoryManager.cs`, `TypingText.cs` |
| Skip dialog | Implemented | Click lanjut; tahan Space hingga progress penuh untuk skip | `TypingText.cs` |
| NPC datang ke loket | Implemented | Spawn/entrance animation dan generate data baru | `NpcEntranceManager.cs` |
| Random NPC appearance | Implemented | Face, rambut, mata, pakaian, aksesori dengan palette | `ProceduralFace.cs` |
| Paspor inspection | Implemented | Inspectable document, zoom/persistence, display field | `PassportScript.cs`, `InspectableObject.cs` |
| Boarding pass inspection | Implemented | Generate/display dan dibandingkan dengan paspor | `TicketScript.cs`, stamp controller |
| Approve/reject/stamp | Implemented/partial | Keputusan meja dan efek ekonomi/trust | `StampHybridController.cs`, `EconomyManager.cs` |
| Mode paspor ↔ koper | Implemented | Toggle hanya saat NPC di depan | `ModeGameSwitch.cs` |
| Koper drag-and-drop | Implemented | Item dapat diangkat, dipindah, disnap kembali | `LuggageItem.cs` |
| Random luggage contents | Implemented | Legal/contraband prefabs dengan peluang item ilegal | `LuggageManager.cs` |
| Luggage weight display | Implemented | Berat dihitung dari item yang masih di koper | `LuggageManager.cs` |
| Contraband evaluation | Implemented/partial | Sita item yang benar; penalti bila terlewat/keputusan salah | `LuggageManager.cs` |
| Timer shift | Implemented | Waktu real dikonversi ke jam display; default 10 menit | `Timer.cs` |
| Economy | Implemented/partial | Cash, penalty, trust, reward/penalty hooks | `EconomyManager.cs` |
| Lose condition | Implemented | Trust habis menampilkan overlay dan restart flow | `LoseCondition.cs` |
| Daily recap | Implemented | Menampilkan cash, penalty, net money, trust, day | `RecapDayUI.cs` |
| Shop/kebutuhan | Partial | Toggle item dan biaya, sebagian item wajib | `ShopManager.cs`, `ItemOnDay` |
| Ticket purchase | Partial | Empat karakter, harga, flags save | `ShopManager.cs`, `SaveManager.cs` |
| Phone contacts | Partial | Stage conversation, inbox, reply hold, notifications | `HP/*.cs` |
| Rule billboard/tooltips | Implemented/partial | Hover, tooltip, document reader, chat content | `Rules Bilboard/*.cs` |
| Audio and transition | Implemented/partial | Audio manager, SFX, DOTween transition hooks | `AudioManager.cs`, `GameManager.cs` |

## Progression and content

| Content | Status | Evidence/intent |
|---|---|---|
| Day 1–3 production scenes | Implemented/partial | `AlphaPreDay1..3`, `AlphaDay1..3`, `AlphaRecapDay1..3` |
| Day 1 base rules | Implemented | Expired passport, info mismatch, ticket date |
| Day 2 bribery rule | Design/bug | Class exists, but `Day2RuleSet` currently does not add it |
| Day 3 suspicious origin | Implemented in factory | `NewRule_Day3` |
| Day 5 counterfeit | Implemented in factory | `NewRule_Day5` |
| Day 7 blacklist | Implemented in factory | `NewRule_Day7` |
| Day 9 mutation marker | Implemented in factory | `NewRule_Day9` |
| Day 4–10 final progression | Design/partial | Rule factory exists; scene/runtime integration not proven |
| Four family tickets | Partial | Aldo, Nasya, Virly, Kraisa flags/prices |
| Multiple endings | Design | Described in old GDD; ending resolver not identified |

## PLGDD expansion backlog

Fitur berikut terdeteksi di PLGDD FigJam dan belum terbukti sebagai runtime production feature:

| Feature | Status | Intended gameplay |
|---|---|---|
| Propaganda documents | Design | Identifikasi slogan/faksi dan pengaruh Azura/Seybay |
| Worker permit | Design | Cocokkan worker ID, sector, job, work day, company stamp |
| Worker certificate/social proof | Design | Cocokkan skill rank, job, nama, tanggal sertifikasi |
| The Special Way | Design | Form khusus dengan alias, passport ID, height, age, valid stamp |
| Medical check | Design | Cek nama, silicosis stage, tanggal, dokter/stamp |
| Vaccine document | Design | Cek vaccine type, dose count, patient, clinic stamp |
| Wanted person | Design | Cocokkan poster buronan dengan identitas/fisik NPC |
| Underground identification | Design | Deteksi simbol/faksi dan kaitkan dengan rule/faction |
| X-Ray paper | Design | Deteksi item tersembunyi/contraband |
| Luggage clearance card | Design | Validasi izin bebas pemeriksaan koper |
| Payment receipt | Design | Cocokkan total tiket, nama, passport ID, tanggal, stamp |
| Temperature/breathing/mutation checks | Design/partial | Memperluas medical inspection dan emergency response |
| NPC interruption and choices | Design/partial | Interupsi petugas, percakapan, pilihan A/B |

## Non-feature technical debt

- Resolve save/load authority and singleton lifetime.
- Make ticket purchase idempotent; current purchase counter can be at risk if called repeatedly for the same character.
- Replace magic scene names with a centralized scene catalog or validated constants.
- Add tests and a deterministic random seed mode for bug reproduction.
- Remove dead/commented code only after confirming it is not an intended prototype hook.
- Treat [PLGDD_FIGJAM_DIGEST.md](PLGDD_FIGJAM_DIGEST.md) as the current design expansion reference.

