# Engineering Playbook — AI-Assisted Development

Dokumen ini menjelaskan cara bekerja sebagai engineer senior ketika menggunakan AI di proyek Seybay. AI bertindak sebagai engineer yang cepat, bukan sebagai sumber kebenaran. Engineer manusia tetap mengendalikan tujuan, risiko, keputusan arsitektur, dan acceptance criteria.

## Prinsip utama

1. **Evidence before implementation** — baca source, call site, scene build, dan data aktif sebelum menyimpulkan perilaku.
2. **Plan before code** — setiap task memiliki plan yang disetujui sebelum perubahan berisiko.
3. **Small change, clear proof** — satu task menyelesaikan satu outcome yang dapat diverifikasi.
4. **Design intent bukan runtime fact** — GDD/PLGDD menjelaskan tujuan; kode dan scene membuktikan implementasi.
5. **No silent decisions** — konflik atau asumsi penting ditulis di task/ADR.
6. **Documentation is control surface** — Markdown bukan catatan pasif; ia menjadi kontrak kerja agent.

## Hirarki source of truth

Jika terjadi konflik, gunakan urutan berikut:

1. Runtime code dan asset yang benar-benar direferensikan active scene.
2. `ProjectSettings/EditorBuildSettings.asset` dan package configuration.
3. ScriptableObject/data asset aktif.
4. `AGENTS.md` dan `Docs/TECHNICAL_DESIGN.md`.
5. `Docs/GDD.md`, PLGDD FigJam, dan design notes.
6. Komentar atau asumsi agent.

Konflik tidak boleh diselesaikan diam-diam. Catat bukti, dampak, dan keputusan yang dibutuhkan.

## Lifecycle task

```text
Request → Explore evidence → Write plan.md → Approval gate
       → Implement small diff → Verify behavior → Review and handoff
```

## Kapan approval wajib

Agent harus berhenti setelah membuat plan dan menunggu approval jika task menyentuh scene/prefab, save/load schema, public API, arsitektur, shared ScriptableObject, balance/aturan gameplay, rename/delete massal, atau compatibility.

Task lokal yang tidak mengubah kontrak publik, seperti null guard atau test tambahan, dapat dikerjakan setelah plan dibuat.

## Checklist eksplorasi

- [ ] Baca dokumen yang relevan.
- [ ] Cari definisi class/function.
- [ ] Cari seluruh call site.
- [ ] Identifikasi active scene dan prototype scene.
- [ ] Identifikasi state yang berubah.
- [ ] Identifikasi event/singleton/Inspector reference.
- [ ] Cek apakah fitur sudah ada sebagian.
- [ ] Catat evidence dengan path dan line bila memungkinkan.

## Checklist implementasi

- [ ] Perubahan sesuai `plan.md`.
- [ ] Tidak menambah scope sampingan.
- [ ] Tidak mengubah scene/prefab tanpa approval.
- [ ] Event static di-subscribe/unsubscribe dengan benar.
- [ ] State save/load ikut dipertimbangkan.
- [ ] Tidak mengubah asset source secara tidak sengaja.
- [ ] Error path dan null reference diperiksa.

## Checklist verifikasi

- [ ] Compile/check yang relevan.
- [ ] Unit/pure logic test bila tersedia.
- [ ] Smoke test flow utama.
- [ ] Regression check fitur terkait.
- [ ] Save/load check bila state berubah.
- [ ] Active scene dan Inspector reference diverifikasi.
- [ ] Known gaps ditulis, bukan disembunyikan.

## Cara menilai output AI

Jangan menilai jawaban AI dari panjang kode. Nilai dari lima pertanyaan:

1. Apa masalah yang diselesaikan?
2. Bukti apa yang mendukung solusi?
3. State dan invariant apa yang berubah?
4. Apa risiko regresinya?
5. Bagaimana hasilnya dibuktikan?

Jika AI tidak bisa menjawab lima pertanyaan ini, task belum siap diimplementasikan.

## Definition of Done

Task selesai hanya jika tujuan, acceptance criteria, perubahan, verifikasi, risiko, dan follow-up terdokumentasi. “Kode sudah dibuat” bukan Definition of Done.

## Format handoff

```text
Scope: <fitur atau bug>
Changed: <file dan ringkasan>
Verified: <compile/test/smoke test>
Known gaps: <hal yang belum diverifikasi>
Docs: <dokumen yang diperbarui>
```

