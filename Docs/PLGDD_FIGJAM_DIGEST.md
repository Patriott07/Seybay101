# PLGDD FigJam Digest

Sumber: FigJam `Ideation WPEG3`, file key `bszwtkLDTvAr3yy1BmguUW`. Digest ini dibuat dari struktur node, text layer, table cell, connector, dan metadata visual yang bisa dibaca melalui Figma API. Board memiliki banyak gambar/ilustrasi; isi gambar yang tidak memiliki text layer belum dianggap sebagai fakta tekstual.

## Apa yang ditemukan

- 1 page, 45 frame, 1.111 shape berteks.
- 376 table cell dalam dua tabel besar.
- 67 connector dan 75 elemen dengan image fill.
- Teks board mencakup dokumen, mechanic, arketipe, progression Day 1–11, UI flow, issue matrix, dan design status.

## Dokumen dan progression

| Hari | Dokumen/mekanik yang muncul di PLGDD | Status pada matrix FigJam |
|---|---|---|
| 1 | Passport; Ticket; propaganda ajakan percaya Seybay | Passport/Ticket engine `Done`; propaganda engine `Not` |
| 2 | Propaganda Azura Community; propaganda kebutuhan worker | Wireframe ada; engine `Not` |
| 3 | Worker document; social proof worker/certificate | Wireframe ada; engine `Not` |
| 4 | The Special Way propaganda/form | Engine `Not` |
| 5 | Medical Check Document; The Special Way form | Engine `Not` |
| 6 | Vaccine Document | Engine `Not` |
| 7 | You Loyal We Make You Easy; Lets Talk Business with Azura | Engine `Not` |
| 8 | Wanted Person; You Find Them You Earn Ticket | Engine `Not` |
| 9 | Document Koper Rule; X-Ray scanning paper | Engine `Not` |
| 10 | Card Bebas Koper Checker; Resi Pembayaran | Engine `Not` |
| 11 | Muncul sebagai label progression di board; detail final perlu dikonfirmasi | Belum dipetakan ke scene produksi |

Matrix FigJam mencatat Passport dan Ticket sebagai wireframe + engine `Done`, sementara sebagian besar dokumen lanjutan baru selesai di wireframe atau belum selesai di engine.

## Document schemas dari PLGDD

- **Passport:** `ID_PASSPORT`, `NAME`, `COUNTRY`, `BIRTHOFDAY`, `EXPIRED DATE`, `SEX`, `PHOTO`.
- **Ticket:** `TICKET_ID`, `PASSENGER_NAME`, `SEAT_CLASS`, `DEPARTURE_DATE`, `DESTINATION`, `BARCODE/STAMP`.
- **Worker permit:** `WORKER_ID`, `NAME`, `SECTOR_JOB`, `work_day`, `COMPANY_STAMP`.
- **Worker certificate:** `NAME`, `SKILL_RANK`, `JOB`, certification date.
- **Special Way form:** alias, passport ID, height, age, valid stamp, applicable requirements.
- **Medical check:** `PATIENT_NAME`, `SILICOSIS_STAGE`, `CHECK_DATE`, stamp.
- **Vaccine:** `PATIENT_NAME`, `VACCINE_TYPE`, `DOSE_COUNT`, clinic stamp.
- **Wanted poster:** bounty ID, suspect name, crime description, suspect photo, status.
- **Luggage rule:** rule version, allowed item list, max weight, revision date.
- **X-Ray paper:** person sprite, `HAS_ITEM`, `IS_ITEM_CONTRABAND`.
- **Clearance card:** clearance number, passenger name, inspector stamp, valid date.
- **Payment receipt:** receipt number, total ticket, name, passport ID, payment date, paid stamp.

## Issue matrix

### Basic rules

Passport photo/sex/validity, ticket date, age limit, worker permit/stamp, certificate skill/name, Special Way height/age/stamp, medical date/stamp, wanted poster physical identity, Underground symbol, luggage weight.

### Emergent rules

Passport–ticket identity, sector/job–corporate need, Special Way passport ID, high silicosis stage, vaccine name/dose, wanted-person match, X-Ray contraband, clearance card validity/name, and receipt total/name/passport/ticket mismatch.

## Arketipe NPC

The Elite, warga biasa/jobless, kelompok rawan sakit/anak-anak, academia/peneliti, loyalist workers, audit Azura Corp, dan The Underground.

## Mekanik dan UI yang terlihat

Passport, ticket, propaganda, worker papers, dialog/browser/chat/call, clock/calendar/CCTV/radio, billboard/rulebook, koper, X-Ray, weight check, temperature/breathing/mutation check, HP, pause, setting, save/load, ratio screen, language, audio, stamp Yes/No, ticket give, choice A/B, gudang, memo, dan trash bin.

## Dampak terhadap dokumentasi

Game sebenarnya dirancang sebagai layered inspection system: basic document checks → worker/social proof → medical/vaccine → faction/propaganda → wanted/Underground → X-Ray/weight/clearance/payment receipt. Kode saat ini baru mencakup sebagian fondasi tersebut. Gunakan label `PLGDD design`, `implemented`, dan `not yet engine` ketika membuat task.

