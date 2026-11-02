# TASK-0001 — Generative Dialog Interrupt

## Goal

Membuat generator dialog deterministik berbasis `StructureDialogueContext` yang membaca empat JSON dialogue layer, memilih dialog sesuai context NPC, dan menampilkan hasilnya pada test runner di scene prototype.

Task ini hanya membuat fondasi generator dan validasi data. Belum mengintegrasikan dialog ke gameplay production.

## User behavior

Developer dapat menjalankan test pada scene prototype dan melihat sequence dialog berdasarkan:

- `AgeGroup`;
- `Mood`;
- `Arktype`;
- `Issues`;
- `Tactic`;
- `WorldSituation`;
- `InternalSituation`.

Console/test runner harus menampilkan context, dialog yang dipilih, source layer, dan fallback yang digunakan.

## Evidence

- Code/asset:
  - `Assets/Scripts/Test/GenerateDialogueTest.cs` sudah memuat empat JSON layer tetapi baru melakukan load dan debug log.
  - `Assets/Scripts/Lib/Schema.cs` sudah memiliki `StructureDialogueContext`, `DialogueLine`, dan schema Layer 1–4.
  - JSON aktif berada di `Assets/Resources/Dialogue_layer1.json` sampai `Dialogue_layer4.json`.
  - Test scene tersedia di `Assets/Scenes/Ptorotype/TestGeneratifDialogue.unity`.
- Technical design:
  - Ikuti `AGENTS.md`, `Docs/ENGINEERING_PLAYBOOK.md`, `Docs/TECHNICAL_DESIGN.md`, dan `Docs/Tech Plan.md`.
  - Gunakan manual reference, event lifecycle yang aman, dan script terpisah berdasarkan tanggung jawab.
- GDD/PLGDD:
  - Rumus dialog: `Arktype + Issue + Tactic + Background + Current Situation`.
  - Dialog dirancang sebagai respons NPC terhadap issue, tactic, dan kondisi internal/world.
- Current gap:
  - Belum ada resolver yang memilih line berdasarkan context.
  - Belum ada normalisasi key enum C# ke key JSON.
  - Belum ada deterministic seed atau fallback report.

## Scope

### In scope

- Membuat generator reusable berbasis context.
- Membaca dan memvalidasi empat JSON dialogue layer.
- Normalisasi key enum C# ke key JSON.
- Menghasilkan sequence Layer 1 → Layer 2 → Layer 3 → Layer 4.
- Deterministic random selection memakai `System.Random` dan seed.
- Fallback aman jika layer/key/dialog tidak tersedia.
- Test runner untuk memanggil generator dan mencetak hasil.
- Dokumentasi hasil test dan fallback.

### Out of scope

- Integrasi ke `NpcEntranceManager`.
- Integrasi ke UI bubble production.
- Branching choice pemain.
- Voice/audio dialogue.
- Pemanggilan API AI eksternal.
- Perubahan production scene.
- Perubahan persistence dialog.
- Penghapusan atau rename `GenerateDialogueTest.cs`.

## Technical approach

Buat dua tanggung jawab terpisah:

1. Generator reusable di `Assets/Scripts/Game/Dialogue/GenerativeDialogueGenerator.cs`.
2. Test runner di `Assets/Scripts/Test/GenerativeDialogueTestRunner.cs`.

Generator menerima `StructureDialogueContext`, data layer yang sudah ter-load, dan seed. Generator mengembalikan result berisi:

- `List<DialogueLine> lines`;
- source layer untuk setiap line;
- fallback messages;
- seed yang digunakan.

Urutan lookup:

1. Layer 1: expression/mood + age group.
2. Layer 2: issue ID + arktype. Field JSON `ageGroupName` pada Layer 2 diperlakukan sebagai arktype karena data aktual berisi `THE_WORKER`, `THE_ELITE`, dan sejenisnya.
3. Layer 3: tactic + arktype.
4. Layer 4: `InternalSituation` terlebih dahulu, lalu `WorldSituation`, kemudian arktype + mood.
5. tolong benerin kalo ada yang janggal dari penamaan atau kamu bisa tanya dlu kalo bingng

Tactic mapping wajib:

```text
Honest                → COOPERATE
Pretending_BeConfused → LIE
Bribe                 → BRIBE
GuiltTrip             → BEG
Begging               → BEG
Intimidation          → THREATEN
Force                 → THREATEN
```

Fallback wajib:

- Mood/expression yang tidak ditemukan: gunakan expression pertama yang tersedia dan catat warning.
- Issue yang tidak ditemukan: skip Layer 2 dan catat alasan.
- Tactic yang tidak ditemukan: skip Layer 3 dan catat alasan.
- Situation yang tidak ditemukan: skip Layer 4 dan catat alasan.
- TextAsset null/JSON invalid: error jelas tanpa NullReferenceException.
- Tidak boleh ada null line di result.

Pemilihan line harus memakai `System.Random(seed)`, bukan `UnityEngine.Random`, agar seed yang sama menghasilkan result yang sama.

## Files expected to change

- `Docs/tasks/TASK-0001-GenerativeDialogInterupt/task-plan.md` — kontrak task ini.
- `Assets/Scripts/Game/Dialogue/GenerativeDialogueGenerator.cs` — generator reusable.
- `Assets/Scripts/Test/GenerativeDialogueTestRunner.cs` — test runner prototype.
- `Assets/Scenes/Ptorotype/TestGeneratifDialogue.unity` — hanya jika diperlukan untuk memasang reference test runner.
- `Docs/tasks/TASK-0001-GenerativeDialogInterupt/task-status.md` — progres.
- `Docs/tasks/TASK-0001-GenerativeDialogInterupt/task-review.md` — hasil verifikasi.

File berikut tidak boleh diubah pada MVP ini:

- `Assets/Scenes/Game/*`.
- `Assets/Scripts/Game/EconomyManager.cs`.
- `Assets/Scripts/Game/SaveSystem/*`.
- `Assets/Scripts/Lib/GameEvent.cs`.

## Interfaces and state

- Public API/type changes: generator/result type baru; tidak mengubah schema existing kecuali compiler membutuhkan adapter non-breaking.
- Runtime state changes: hanya state sementara selama test run; tidak disimpan.
- Save/load impact: tidak ada.
- Scene/Inspector impact: hanya test scene prototype bila diperlukan; semua JSON harus dapat di-assign sebagai `TextAsset` reference.
- Events and dependencies: tidak memakai `GameEvent` dan tidak mengubah event global.

## Invariants

- Seed yang sama dan input context/data yang sama menghasilkan sequence yang sama.
- Generator tidak mengubah `StructureDialogueContext` atau data JSON yang sudah di-load.
- Generator tidak memanggil economy, trust, save, scene loader, atau production UI.
- Missing data menghasilkan fallback/report, bukan exception fatal.
- Result tidak mengandung `null` atau line kosong.
- JSON source tetap read-only selama runtime.

## Risks

- Key enum C# dan key JSON tidak konsisten, terutama `Mood`, `Issues`, dan `Tactic`.
- Layer 2 memakai nama field `ageGroupName` tetapi nilainya berupa arktipe.
- Data JSON saat ini sparse; banyak context valid mungkin hanya menghasilkan fallback.
- Test scene mungkin membutuhkan perubahan Inspector reference.
- `JsonUtility` membutuhkan wrapper JSON yang sesuai schema existing.

## Approval gate

- Required: Yes.
- Reason: task menambah public runtime type/generator dan kemungkinan mengubah test scene reference.
- Approval: Approved by Abdii, 2026-11-01.
- Perubahan mandiri yang diperbolehkan setelah plan disetujui:
  - membuat generator dan test runner sesuai scope;
  - menambah helper mapping internal;
  - menambah validation/fallback;
  - menambah log dan test case;
  - mengisi `task-status.md` dan `task-review.md`.
- Perubahan yang tetap membutuhkan approval baru:
  - mengubah schema existing di `Schema.cs`;
  - mengubah JSON content/dialogue data;
  - mengubah production scene/prefab;
  - menghubungkan generator ke UI/gameplay production;
  - menambah dependency/plugin/API eksternal;
  - mengubah economy, trust, save, atau GameEvent.
- Approval: Pending user approval.

## Acceptance criteria

- [ ] Empat JSON layer dapat di-load dan divalidasi.
- [ ] Context NPC dapat dinormalisasi ke key JSON.
- [ ] Generator menghasilkan sequence Layer 1 → Layer 2 → Layer 3 → Layer 4 jika data tersedia.
- [ ] Tactic mapping bekerja untuk semua tactic yang ditentukan.
- [ ] Tiap Layer sudah memuat aturan generate bubble yang lengkap
- [ ] Missing key/data menghasilkan fallback yang tercatat dan tidak crash.
- [ ] Seed yang sama menghasilkan hasil yang sama.
- [ ] Test runner dapat menampilkan context, source layer, line, dan fallback.
- [ ] Missing TextAsset menghasilkan error yang jelas.
- [ ] Tidak ada perubahan pada production scene atau gameplay economy.
- [ ] Compile Unity berhasil atau error compile terdokumentasi.
- [ ] `task-status.md` dan `task-review.md` diisi setelah implementasi/test.


Sebelum coding:
1. Verifikasi ulang file yang akan diubah.
2. Update task-status.md menjadi Implementation.
3. Jika menemukan perubahan di luar scope, berhenti dan minta approval.

Setelah coding:
1. Jalankan test sesuai acceptance criteria.
2. Update task-status.md.
3. Isi task-review.md.
4. Laporkan Changed, Verified, Known gaps, dan Docs.