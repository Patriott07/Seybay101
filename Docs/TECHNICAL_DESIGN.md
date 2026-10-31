# Technical Design — Seybay

## 1. Runtime architecture

Arsitektur saat ini adalah komponen Unity dengan beberapa singleton manager dan static event bus.

```text
Scene / Inspector references
          ↓
Managers (Game, Economy, Save, Timer, Story, Audio)
          ↓
Gameplay controllers (NPC, Passport, Ticket, Luggage, Stamp)
          ↓
Runtime schemas + ScriptableObject data
          ↓
UI / audio / transitions
```

`GameEvent` menjadi event bus global untuk generate data, spawn NPC, mode paspor, koper, dan transisi recap. Event subscriber wajib mendaftar di `OnEnable` dan melepas di `OnDisable`.

## 2. Scene topology

Build settings saat ini mencampur scene produksi, prototype, dan sample. Scene produksi yang terlihat:

- `Assets/Scenes/Game/Home.unity`
- `Assets/Scenes/Game/AlphaPreDay1.unity` hingga `AlphaPreDay3.unity`
- `Assets/Scenes/Game/AlphaDay1.unity` hingga `AlphaDay3.unity`
- `Assets/Scenes/Game/AlphaRecapDay1.unity` hingga `AlphaRecapDay3.unity`
- `Assets/Scenes/Story/Preday.unity`

Prototype ada di `Assets/Scenes/Ptorotype/` dan tidak boleh menjadi referensi flow final tanpa keputusan product owner.

## 3. Primary runtime flows

### NPC inspection

1. `NpcEntranceManager` menyiapkan NPC dan memicu `GenerateNewPassportData`/`GenerateNewBoardingPass`/`GenerateNewNPCView`.
2. `PassportScript` dan `TicketScript` mengisi data dan view.
3. `ModeGameSwitch` menjaga mode paspor atau koper.
4. `StampHybridController` mengevaluasi keputusan dokumen.
5. `LuggageManager` membuat item, menerima drag, menghitung berat, dan mengevaluasi kontraband.
6. `EconomyManager` menerima perubahan money/trust/penalty.
7. NPC keluar dan sistem menyiapkan NPC berikutnya.

### Day transition

1. Timer berhenti atau shift diakhiri.
2. `GameManager.EndShiftAndLoadScene()` menyimpan game, menunggu transition, memicu `StartTranstionDayToRecap`, lalu load recap scene.
3. `RecapDayUI` membaca save untuk menampilkan hasil.
4. `ShopManager` membayar item/tiket atau memilih later.
5. `SaveManager.AdvanceDay()` menaikkan day dan memanggil reset harian economy.

## 4. Data ownership

| Data | Owner | Persistence |
|---|---|---|
| Current day | `GameManager` + `SaveManager.currentSave` | Intended JSON; runtime synchronization incomplete |
| Cash | `EconomyManager.money` | JSON capture; load cash active |
| Penalty | `EconomyManager.punish` | JSON capture; load apply commented |
| Trust | `EconomyManager.trust` | JSON capture; load apply commented |
| Purchased tickets | `GameSaveState` flags/count | Save fields exist; full load/ending integration incomplete |
| Story | `StorySchema` asset | Asset data, not runtime save |
| Rules | `DayRules` class and `DayRuleData` asset | Two competing sources; consolidate |
| NPC runtime | `NPCPassengerRuntimeData` and component instances | Per shift, not save |

## 5. Persistence contract

Save path: `Path.Combine(Application.persistentDataPath, "game_save.json")`.

Current save schema:

```text
currentDay
playerCash
purchasedTickets
purhaceTicketForAldo
purhaceTicketForNasya
purhaceTicketForVirly
purhaceTicketForKraisa
penalty
trust
```

Before extending save, define invariants: day >= 1, cash >= 0 unless debt is intentional, trust 0–100, ticket count equals flags, and purchase is idempotent by character.

## 6. PLGDD data expansion

The FigJam PLGDD defines a larger document vocabulary than the current runtime schema: worker permit, worker certificate, propaganda, Special Way form, medical check, vaccine, wanted poster, Underground identification, luggage rule, X-Ray paper, clearance card, and payment receipt. These should be modeled as typed case evidence rather than adding unrelated booleans to `PassportSchema`.

Recommended shape:

```text
InspectionCase
  passengerIdentity
  documents[]
  luggage[]
  activeRules[]
  issues[]

DocumentEvidence
  documentType
  fields
  validity
  issuer
  visualAsset

IssueDefinition
  category: Basic | Emergent
  evidenceRefs[]
  fieldComparisons[]
  expectedDecision
```

Keep the current `PassportSchema` and `BoardingPassSchema` compatible while introducing this as a separate layer. Do not expand the save file with transient NPC case data.

## 7. Rule engine design

`RuleViolation` memakai inheritance dan `Apply(PassportSchema, BoardingPassSchema)`. `DayRules.GetActiveRules(day)` membangun inheritance chain. Ini mudah diperluas, tetapi saat ini rule application mencampur passport/ticket mutation dan view singleton calls.

Recommended boundary for future work:

```text
RuleDefinition (pure data)
        ↓
ViolationGenerator (random selection, seedable)
        ↓
InspectionCase (immutable expected truth)
        ↓
InspectionEvaluator (pure result)
        ↓
UI / Economy adapters
```

Jangan mengubah rule generator menjadi static global baru tanpa test; gunakan dependency yang bisa diberi random seed untuk reproducibility.

## 8. Risks and failure modes

- Singleton manager dibuat ulang saat scene load dan kehilangan state bila tidak persistent.
- Static events dapat memanggil listener lama bila unsubscribe terlewat.
- Nama scene literal berbeda antara production dan prototype.
- `SaveManager` melakukan load pada `Awake`, sementara `EconomyManager` juga load pada `Start`; urutan inisialisasi dapat mengubah hasil.
- `ViolationSystem.TryGenerateViolations()` membuat `PassportSchema` kosong lalu menyalin field yang dimodifikasi; rule yang mengandalkan field dasar harus diuji.
- Random item bisa menghasilkan jumlah ilegal yang lebih besar dari total item pada konfigurasi ekstrem.
- Penggunaan `DateTime` di `JsonUtility` perlu dihindari dalam save runtime; saat ini schema paspor runtime, bukan save utama.

## 9. Testing plan

### Automated candidates

- `DayRules.GetActiveRules(1..10)` menghasilkan rule yang diharapkan.
- Violation generator dengan seed menghasilkan 0 atau 1–3 violation sesuai kontrak.
- Paspor/tiket mismatch terdeteksi dan clean case diterima.
- `PurchaseTicket` tidak menggandakan tiket untuk karakter yang sama.
- Save round-trip memulihkan semua field.
- Trust 0 memicu lose dan trust di luar range di-clamp.

### Manual smoke test

1. Home → pre-day → shift.
2. Spawn minimal tiga NPC.
3. Inspect document, toggle koper, drag item, snap item kembali.
4. Approve/reject clean dan violating cases.
5. Timer end → recap → shop → next day.
6. Tutup/buka game dan cek save.
7. Turunkan trust ke 0 dan cek restart flow.

## 10. Change checklist

- Apakah perubahan menyentuh scene/prefab atau hanya code/data?
- Apakah reference Inspector tetap valid?
- Apakah static event di-unsubscribe?
- Apakah state load/save ikut berubah?
- Apakah scene target production, bukan prototype?
- Apakah doc feature status diperbarui?

