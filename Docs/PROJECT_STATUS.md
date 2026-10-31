# Project Status — Seybay / Seybey10

Status ini disusun dari repository pada 31 Oktober 2026. Ia memisahkan fakta implementasi dari niat desain agar agent tidak menganggap prototype sebagai fitur produksi.

## Ringkasan

Seybay adalah game Unity 2D single-player berbasis shift harian. Flow yang terlihat dari scene, script, dan data adalah: menu → pre-day/story → shift di meja inspeksi → recap/after-shift → pembelian tiket/kebutuhan → hari berikutnya.

Implementasi paling matang saat ini adalah fondasi inspeksi dokumen, UI story/typewriter, generate NPC/paspor, inspeksi koper drag-and-drop, ekonomi/trust, serta save JSON. Namun flow final 10 hari, persistence seluruh state, dan beberapa aturan lanjutan belum konsisten antara kode, scene, dan GDD lama.

PLGDD FigJam memperjelas bahwa scope desain lebih besar: ada worker permit/certificate, propaganda, Special Way form, medical check, vaccine, wanted poster, Underground identification, X-Ray, clearance card, dan payment receipt. Sebagian besar item itu tercatat `Done` di wireframe tetapi `Not` di engine.

## Evidence matrix

| Area | Status | Bukti utama | Catatan |
|---|---|---|---|
| Unity project | Implemented | `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json` | Unity 6000.1.7f1, URP, 2D, TMP/uGUI |
| Main scenes | Implemented/partial | `Assets/Scenes/Game/` | Home, pre-day, day 1–3, recap day 1–3 tersedia |
| Passport/ticket inspection | Implemented | `Scene Cek Pasport/*.cs`, active game scenes | Generate, display, inspect, stamp/evaluate |
| NPC generation | Implemented | `NpcEntranceManager.cs`, `ProceduralFace.cs`, `NamePool` assets | Random identity/visuals; perlu verifikasi Inspector |
| Violation rules | Partial | `ViolationSystem/` | Factory mendukung day 1–10, tetapi `GameManager.DoViolationGenerateSetup()` hanya switch day 1–3 |
| Luggage inspection | Partial | `LuggageManager.cs`, `LuggageItem.cs` | Drag, snap, weight display, contraband; rule flags lanjutan belum seluruhnya dipakai |
| Economy/trust | Implemented/partial | `EconomyManager.cs`, `LoseCondition.cs` | Nilai aktif sederhana; trust reset harian dan save/load perlu audit |
| Story/typewriter | Implemented | `StoryManager.cs`, `TypingText.cs`, story assets | Typewriter, audio/storyboard hook, click advance, hold-space skip |
| Phone/chat | Partial | `HP/*.cs`, phone assets | Contact stages, auto messages, reply hold, JSON log tertentu |
| Shop/tickets | Partial | `ShopManager.cs`, `SaveManager.cs` | Ticket flags dan harga ada; ending/final progression belum terlihat lengkap |
| Save system | Partial | `SaveManager.cs`, `GameSaveState.cs` | JSON native, tetapi beberapa field tidak di-apply saat load |
| Final narrative | Design intent | `Assets/Scripts/Docs/gdd-n-narrative.md` | Belum boleh dianggap sebagai perilaku runtime penuh |
| PLGDD source | Design reference | `Docs/PLGDD_FIGJAM_DIGEST.md` | Board FigJam berisi matriks dokumen, issue, arketipe, dan UI |

## Known discrepancies

1. Dokumentasi lama menyebut `StorySchema` memiliki pre-day/on-desk/after-shift text, tetapi class aktif saat ini hanya memiliki `Day` dan `storyText` berupa `TextContentSchema`.
2. `DayRuleData` memiliki flag berat, X-Ray, organik, bahan terbakar, metal detector, dan batas 35 kg, tetapi `LuggageManager` masih memiliki penerapan rule tertentu dalam komentar.
3. `GameManager` menampilkan switch violation yang hanya menangani hari 1–3, sedangkan `DayRules.GetActiveRules()` sudah mendefinisikan hari 1–10.
4. `Day2RuleSet` tidak menambahkan `NewRule_Day2` walaupun komentar dan `RuleViolation.cs` mendeskripsikan aturan bribery hari 2.
5. `SaveManager.CaptureData()` menyimpan cash/penalty/trust/day, tetapi `ApplyData()` aktif terutama hanya mengembalikan cash; day, trust, penalty, dan ticket flags perlu dipastikan sebelum dipercaya untuk resume.
6. `ShopManager` memanggil `AdvanceDay()` dan menaikkan `currentSave.currentDay`, tetapi `GameManager.currentDay` runtime tidak selalu di-set dari save.
7. Banyak scene prototype terdaftar di build settings. Agent harus membedakan scene produksi `Assets/Scenes/Game/` dari eksperimen `Assets/Scenes/Ptorotype/`.
8. Nama folder `Ptorotype`, typo `Pasport`, `purhaceTicket...`, dan `Gander` adalah legacy API. Jangan rename massal tanpa migration plan.
9. FigJam menyebut Day 1–11, sedangkan runtime dan GDD lama terutama membahas Day 1–10. Batas campaign final harus diputuskan.
10. Raster preview FigJam tidak dapat diunduh dari environment saat analisis ini; struktur, text layer, tabel, connector, dan metadata image sudah terbaca. Detail yang hanya ada di dalam gambar raster perlu dikonfirmasi dari export PNG/PDF bila diperlukan.

## Prioritas berikutnya

1. Tetapkan satu flow scene produksi dan hapus/disable scene prototype dari build bila sudah disepakati.
2. Satukan sumber kebenaran hari aktif: `GameManager`, `SaveManager`, dan scene names.
3. Selesaikan persistence: day, trust, penalty, ticket flags, ending state.
4. Satukan rules: pilih `DayRuleData` atau factory class sebagai authoritative rules engine.
5. Tambahkan automated tests untuk save validation, violation application, ticket purchase idempotency, dan ending calculation.
6. Verifikasi semua ScriptableObject/Inspector reference untuk build baru setelah setiap scene change.

