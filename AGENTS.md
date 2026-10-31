# Seybay / Seybey10 — Agent Guide

Dokumen ini adalah pintu masuk untuk agent yang bekerja di repository Unity ini. Baca juga [Docs/ENGINEERING_PLAYBOOK.md](Docs/ENGINEERING_PLAYBOOK.md), [Docs/PROJECT_STATUS.md](Docs/PROJECT_STATUS.md), [Docs/GDD.md](Docs/GDD.md), [Docs/FEATURE_LIST.md](Docs/FEATURE_LIST.md), dan [Docs/TECHNICAL_DESIGN.md](Docs/TECHNICAL_DESIGN.md).

## Konteks singkat

Seybay adalah game 2D simulation/narrative tentang Aldo, petugas loket evakuasi Azura Corp. Pemain memeriksa paspor, boarding pass, dan koper NPC, lalu mengelola uang, trust, cerita, serta pembelian tiket evakuasi untuk keluarga.

Fakta repository saat ini:

- Unity `6000.1.7f1`, target PC Windows, resolusi desain 1920×1080.
- Bahasa: C#; paradigma OOP + komponen Unity.
- Render pipeline: URP 2D; UI memakai Canvas/TMP/uGUI.
- Input memakai Unity Input System package, tetapi banyak interaksi inspeksi lama masih memakai mouse polling (`OnMouse...`, `Input.GetMouseButton...`).
- Scene produksi utama berada di `Assets/Scenes/Game/`; scene eksperimental berada di `Assets/Scenes/Ptorotype/`.
- Data naratif, nama, dan aturan sebagian memakai ScriptableObject.
- Save game memakai JSON native di `Application.persistentDataPath/game_save.json`.

## Source of truth

Urutan kepercayaan ketika dokumentasi dan implementasi berbeda:

1. Kode C# dan asset yang benar-benar direferensikan oleh scene build.
2. `ProjectSettings/EditorBuildSettings.asset` dan `Packages/manifest.json`.
3. ScriptableObject/data asset yang aktif di scene.
4. Markdown lama di `Assets/Scripts/Docs/`.
5. GDD dan komentar desain yang belum dipanggil oleh kode.

PLGDD FigJam menjadi design reference penting, tetapi status engine pada board harus dihormati. Gunakan [Docs/PLGDD_FIGJAM_DIGEST.md](Docs/PLGDD_FIGJAM_DIGEST.md) dan jangan mengubah konsep wireframe menjadi klaim fitur implemented tanpa bukti scene/code.

Jika ada konflik, jangan diam-diam memilih. Catat di `Docs/PROJECT_STATUS.md` dan sebutkan file/line yang menjadi bukti.

## Peta area kode

- `Assets/Scripts/Lib/`: schema runtime dan static game events.
- `Assets/Scripts/Game/`: manager, scene gameplay, timer, UI gameplay, audio.
- `Assets/Scripts/Game/Scene Cek Pasport/`: generate/display paspor, tiket, NPC entrance, stamping.
- `Assets/Scripts/Game/Scene Koper/`: drag-and-drop item, berat, evaluasi kontraband, ekonomi.
- `Assets/Scripts/Game/ViolationSystem/`: rule set per hari dan generator violation.
- `Assets/Scripts/Game/SaveSystem/`: state JSON, load/save, ticket flags, advance day.
- `Assets/Scripts/Game/HP/`: phone/contact/chat state dan notifikasi.
- `Assets/Scripts/Game/Rules Bilboard/`: rulebook/tooltip/chat aturan.
- `Assets/Scripts/SO/` dan `Assets/Scripts/SO/Data/`: ScriptableObject definition dan data story/chat/item/name.
- `Assets/Scripts/UI/`: menu, pause/settings, typewriter.
- `Assets/Scripts/Test/` dan `Assets/Scenes/Ptorotype/`: eksperimen; jangan menganggapnya sebagai flow produksi tanpa bukti scene build.

## Aturan kerja agent

1. Jangan mengubah scene Unity atau prefab secara langsung kecuali user memintanya secara eksplisit.
2. Jangan menambah fitur di luar scope tiket/request. Satu fitur harus punya catatan Markdown singkat bila perubahan cukup besar.
3. Gunakan nama file dan API berbahasa Inggris; pertahankan nama legacy Indonesia bila mengubah file yang sudah ada kecuali ada alasan migrasi.
4. Ikuti pola yang sudah dipakai: manual reference untuk komponen, subscribe/unsubscribe event di `OnEnable`/`OnDisable`, singleton hanya untuk manager.
5. Jangan mengubah data ScriptableObject asset bersama-sama dengan perubahan kode tanpa menyatakan dampaknya.
6. Jangan menganggap komentar sebagai perilaku yang sudah berjalan. Verifikasi call site dan assignment Inspector.
7. Jangan menghapus atau mereset save, scene, asset, atau perubahan user tanpa izin.
8. Sebelum selesai, jalankan pemeriksaan yang relevan: compile Unity, test scene terkait, dan smoke test flow yang berubah.

## Task protocol

Untuk task yang membutuhkan perubahan, gunakan folder `Docs/tasks/TASK-XXXX-short-name/` dan template di `Docs/templates/`. Minimal buat `plan.md`, `status.md`, dan `review.md`. Task yang menyentuh arsitektur atau kontrak lintas subsystem juga membutuhkan ADR di `Docs/decisions/`.

Agent wajib mengikuti urutan: explore → plan → approval gate bila berisiko → implement → verify → review. Detailnya ada di `Docs/ENGINEERING_PLAYBOOK.md`.

## Workflow aman

1. Baca dokumen yang relevan dan cari call site dengan `rg`.
2. Tentukan apakah perubahan termasuk produksi, prototype, data, atau tooling.
3. Buat daftar file yang akan berubah sebelum mengedit.
4. Implementasikan perubahan kecil dan terisolasi.
5. Periksa referensi singleton/event, null reference, scene name, serta state save/load.
6. Laporkan file yang berubah, verifikasi yang dilakukan, dan risiko yang tersisa.

## Invariant penting

- `trust` berada pada rentang 0–100; trust <= 0 memicu lose condition.
- Pemeriksaan paspor/tiket tidak boleh mengubah asset ScriptableObject sumber secara permanen.
- Scene inspeksi perlu menjaga `GameManager.isNpcInFront` sebelum membuka mode koper.
- Event static wajib dilepas di `OnDisable` agar tidak terjadi listener ganda setelah scene reload.
- Save saat ini belum merupakan persistence penuh: `SaveManager.ApplyData()` hanya memulihkan cash dengan aktif; pemulihan day/trust/penalty/ticket perlu diverifikasi sebelum fitur bergantung padanya.
- `DayRuleData` mendeskripsikan aturan koper lanjutan, tetapi beberapa penerapannya masih dikomentari di `LuggageManager`.

## Format laporan agent

Gunakan format ringkas:

```text
Scope: <fitur/bug>
Changed: <file dan ringkasan>
Verified: <compile/test/manual check>
Known gaps: <hal yang belum bisa diverifikasi>
Docs: <link dokumen yang diperbarui>
```

