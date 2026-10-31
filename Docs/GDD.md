# Game Design Document — Seybay

## 1. Identitas

- **Judul kerja:** Seybay / Seybey10
- **Genre:** 2D inspection simulation, narrative, resource management
- **Platform:** PC Windows
- **Engine:** Unity 6000.1.7f1
- **Inspirasi:** *Papers, Please* untuk inspeksi dan keputusan; drama dystopian untuk tekanan moral.
- **Logline:** Setiap koper menyimpan risiko, setiap dokumen dapat menentukan nasib seseorang, dan setiap gaji membawa keluarga Aldo sedikit lebih dekat atau jauh dari evakuasi.

## 2. Premis dunia

Bumi berada di ambang keruntuhan atmosfer dan masyarakat berebut akses ke evakuasi menuju Seybay Outpost yang dikendalikan Azura Corp. Aldo bekerja di loket pemeriksaan. Ia harus mematuhi regulasi perusahaan, memperoleh uang, dan membeli tiket bagi keluarga sebelum kesempatan berakhir.

Elemen worldbuilding yang sudah muncul di dokumen naratif: Solar Cascade, Azura Corp, Seybay Outpost, mutasi akibat partikel anorganik, Aldo, Nasya, Virly, dan Kraisa. Detail tanggal, penyakit, dan kanon akhir tetap perlu disahkan oleh pemilik game.

## 3. Fantasi pemain dan tujuan

Pemain ingin merasa seperti petugas loket yang teliti namun tertekan waktu. Tujuan jangka pendek adalah memutuskan nasib satu NPC dengan bukti dokumen/koper. Tujuan jangka menengah adalah menjaga trust dan cash. Tujuan jangka panjang adalah mengamankan tiket keluarga dan mencapai ending yang sesuai keputusan ekonomi/moral.

## 4. Core loop

```text
Pre-day story / aturan
        ↓
Shift: NPC datang → baca dokumen → cocokkan paspor/tiket → buka koper → putuskan
        ↓
Reward / penalty → trust & cash berubah
        ↓
Timer selesai → recap harian
        ↓
Belanja kebutuhan / tiket → save → hari berikutnya
```

### Keputusan NPC

Pemain menginspeksi:

- identitas dan nomor dokumen;
- pemilik paspor vs nama pada boarding pass;
- tanggal kedaluwarsa dan tanggal keberangkatan;
- foto/keabsahan dokumen;
- negara/distrik/warna kartu sebagai clue aturan;
- isi koper, barang terlarang, dan berat.

Output keputusan yang dirancang: approve, reject, atau sita barang. Dampak aktual berasal dari `StampHybridController`, `EconomyManager`, dan `LuggageManager`, sehingga agent harus memeriksa call site sebelum mengubah nilai reward.

## 5. Sistem permainan

### Layered inspection design dari PLGDD

PLGDD FigJam mendefinisikan inspeksi sebagai lapisan aturan yang bertambah seiring hari, bukan hanya passport/ticket:

1. **Basic identity:** passport, ticket, photo, sex, age, expiry, date.
2. **Worker/social proof:** worker permit, sector/job, work day, certificate, skill rank.
3. **Special access:** propaganda, Special Way form, secret access code, loyalty/commendation.
4. **Health:** medical check, silicosis stage, vaccine, breathing/temperature/mutation checks.
5. **Faction/security:** wanted poster, Underground identification, Azura audit.
6. **Luggage/security:** rule document, X-Ray, weight, clearance card, receipt.

Masing-masing dokumen memiliki field artist-facing dan programmer-facing. Rincian field dan status engine ada di [PLGDD_FIGJAM_DIGEST.md](PLGDD_FIGJAM_DIGEST.md).

### Shift dan waktu

`Timer` mengukur waktu real-time. Default durasi 10 menit, ditampilkan sebagai jam 08:00–22:00. Saat timer selesai, spawn NPC dihentikan; transisi recap dipicu oleh event.

### Paspor dan boarding pass

Runtime schema utama berada di `Schema.data`:

- `PassportSchema`: document number, owner name, sex, tanggal lahir, expiry, country, district, card color, validity, photo match.
- `BoardingPassSchema`: pass number, destination, seat, passenger name, linked document ID, departure date, seat class, validity.
- `NPCPassengerRuntimeData`: identitas NPC, passport, boarding pass, luggage, bribe, dialogue, violation reason.

### Issue taxonomy

PLGDD membagi issue menjadi `Basic Rule` dan `Emergent Rule`. Emergent rule menghubungkan beberapa dokumen sekaligus, misalnya passport + ticket, passport + worker permit, medical + rulebook, atau receipt + ticket/passport. Case generator idealnya menyimpan sumber bukti dan field yang harus dibandingkan, bukan hanya boolean `isValid`.

### Violation dan progresi rule

NPC memiliki peluang 40% memperoleh 1–3 violation random dari rules aktif; 60% bersih. `DayRules` mendefinisikan akumulasi rule sampai hari 10:

| Hari mulai | Rule |
|---|---|
| 1 | Expired passport, info mismatch, ticket date |
| 3 | Suspicious origin |
| 5 | Counterfeit document |
| 7 | Blacklist entry |
| 9 | Mutation marker |

Bribery class tersedia sebagai `NewRule_Day2`, tetapi belum terpasang di `Day2RuleSet`; ini adalah gap implementasi, bukan asumsi fitur aktif.

### Koper

`LuggageManager` mengacak prefab legal dan terlarang, menaruhnya pada area spawn, menghitung berat item yang masih di koper, dan mengevaluasi item yang dipindahkan ke meja. `LuggageItem` memakai mouse drag, sorting order, dan snap-back.

`DayRuleData` mendukung desain lebih kaya: batas berat, X-Ray, larangan organik, bahan terbakar, metal detector, dan logam padat. Sebagian flag belum terbukti mengubah evaluasi runtime.

### Ekonomi, trust, dan lose

- Cash aktif pada `EconomyManager.money`.
- Penalty dikumpulkan pada `punish`; net money = money − punish.
- Trust mulai 100, warning di 30, dan trust <= 0 memicu `LoseCondition`.
- Reward/penalty default adalah field Inspector, bukan hard-coded game balance yang aman dianggap final.
- Trust dirancang refresh setiap hari melalui `MulaiHariBaru`, tetapi save/load harus diaudit sebelum dianggap final.

### Malam dan tiket

`ShopManager` menghitung harga toggle tiket Aldo/Nasya/Virly/Kraisa dan item kebutuhan. `SaveManager` menyimpan flag pembelian dan jumlah tiket. Harga saat ini adalah data code/Inspector, bukan sistem surge pricing generik yang sudah terbukti.

### Cerita dan telepon

`StorySchema` adalah ScriptableObject berisi `Day` dan daftar `TextContentSchema` (teks, audio, speed, storyboard texture, immersive audio). `TypingText` memberi efek ketik, `[Effect]` wobble, audio per line, click advance, dan hold-space skip.

Phone system memakai `PhoneContactSO` dengan stages dan `PhoneLine`; pesan kontak berjalan otomatis, giliran balasan Aldo menunggu tombol hold. Sebagian log chat disimpan terpisah.

## 6. Experience goals

1. Membaca bukti terasa lebih penting daripada refleks.
2. Waktu membuat keputusan sulit, tetapi UI tetap memberi clue yang adil.
3. Kesalahan terasa konsekuensial melalui trust, uang, dan kondisi keluarga.
4. Cerita muncul melalui ritme pre-day, shift, recap, shop, dan phone.
5. Randomisasi NPC memberi variasi tanpa mengorbankan kemampuan pemain memahami aturan.

## 7. Konten hari

Hari 1–3 sudah memiliki scene produksi yang jelas. Desain lama mengusulkan eskalasi sampai hari 10: aturan dasar → koper → berat → scanner/aturan lanjutan → anomali/blacklist → peluncuran terakhir. Ini masih target konten sampai integration scene, balance, dan ending resolver terbukti.

PLGDD FigJam memiliki matriks dokumen hingga Day 10 dan label Day 11. Campaign length harus dikunci: apakah Day 11 playable, post-campaign, atau hanya catatan ideasi.

## 8. Ending design intent

GDD lama mengusulkan ending berdasarkan jumlah tiket: empat tiket menyelamatkan keluarga, tiga mengorbankan Aldo, dua menyelamatkan anak, dan satu/zero memicu pilihan buruk. Resolver ending belum ditemukan dalam inventory kode yang dianalisis; anggap ini design intent yang memerlukan implementasi dan acceptance test.

## 9. Accessibility dan UX target

- Semua informasi aturan penting tersedia dalam billboard/rule document, bukan hanya warna.
- Typewriter dapat dilewati dengan tahan Space.
- Status trust dan uang terlihat di HUD.
- Drag item harus memberi feedback hover dan snap.
- Target fixed 1920×1080; layout harus diuji pada aspect ratio berbeda bila platform memperluas target.

## 10. Acceptance criteria vertical slice

Vertical slice minimum dianggap berhasil bila: pemain membuka game, membaca pre-day, memproses NPC, memeriksa paspor dan tiket, berpindah ke koper, menyita/menyimpan item, menerima reward/penalty yang benar, melihat recap, membeli atau menunda tiket, save/load kembali pada hari yang sama, dan mencapai lose condition ketika trust habis.

