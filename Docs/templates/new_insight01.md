"kalo saya mau approve task-plan saya perlu ubah apa"

Anda tidak perlu mengubah isi besar task plan. Isinya sudah cukup untuk approval. Yang perlu dilakukan:

1. Review bagian:
   - Scope
   - Out of scope
   - Files expected to change
   - Approval gate
   - Acceptance criteria
2. Ubah bagian ini:

## Approval gate

- Required: Yes.
- Reason: task menambah public runtime type/generator dan kemungkinan mengubah test scene reference.
- Approval: Pending user approval.

menjadi:

## Approval gate

- Required: Yes.
- Reason: task menambah public runtime type/generator dan kemungkinan mengubah test scene reference.
- Approval: Approved by <nama Anda>, 2026-11-01.
- Approved scope:
  - Generator dialog berbasis context.
  - Loader JSON Layer 1–4.
  - Mapping enum ke key JSON.
  - Fallback dan deterministic seed.
  - Test runner prototype.
- Conditions:
  - Tidak mengubah production scene.
  - Tidak mengubah economy, trust, save system, atau GameEvent.
  - Jika scope berubah, AI harus berhenti dan meminta approval baru.

Jangan mencentang acceptance criteria sekarang. Checklist itu baru dicentang setelah implementasi dan testing selesai.

Setelah itu, berikan instruksi ke AI:

TASK-0001 sudah disetujui.

Baca task-plan.md terbaru dan implementasikan hanya scope yang sudah disetujui.
Ikuti AGENTS.md, ENGINEERING_PLAYBOOK.md, TECHNICAL_DESIGN.md, dan Tech Plan.md.

Sebelum coding:
1. Verifikasi ulang file yang akan diubah.
2. Update task-status.md menjadi Implementation.
3. Jika menemukan perubahan di luar scope, berhenti dan minta approval.

Setelah coding:
1. Jalankan test sesuai acceptance criteria.
2. Update task-status.md.
3. Isi task-review.md.
4. Laporkan Changed, Verified, Known gaps, dan Docs.