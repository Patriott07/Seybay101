# README - GAME DESIGN DOCUMENT (GDD): SEYBAY

> **Logline:** "Setiap koper punya rahasia. Setiap keputusan punya harga."  
> **Developer:** KopDes Merah Putih  
> **Format:** 2D Simulation / Narrative / Resource & Time Management (PC)  
> **Inspirasi:** *Papers, Please* (core mekanik), *Beholder* (dystopia & moral dilemma), *This War of Mine* (survival pressure).

---

## 1. PREMIS & WORLD-BUILDING

1. **Peristiwa "The Solar Cascade" (Hitung Mundur 10 Hari):**
   - Rentetan badai matahari mikro merusak ionosfer dan magnetosfer Bumi.
   - Inti bumi melemah, atmosfer menipis, dan temperatur naik drastis. Bumi hanya punya sisa waktu 10 hari sebelum atmosfer terbakar habis (dimulai ~1 Agustus 2045).
2. **Azura Corp & Seybay Outpost:**
   - Megakorporasi Azura Corp membuka proyek evakuasi roket menuju planet koloni baru: **Seybay Outpost**.
   - Tiket **tidak gratis** bahkan untuk karyawan. Azura Corp menerapkan sistem *surge pricing* (harga tiket naik tajam seiring mendekatnya kiamat).
3. **Penyakit Mutasi Partikel Anorganik (3 Stadium):**
   - Radiasi dan runtuhnya atmosfer memaksa manusia menghirup partikel mineral beracun:
     - **Stadium 1 (Silikasi/Kristalisasi):** Kulit mengeras seperti pasir/kristal, persendian kaku.
     - **Stadium 2 (Termal/Plasma):** Suhu tubuh melonjak drastis, darah berubah menjadi plasma membara.
     - **Stadium 3 (Disolusi/Psikis):** Tubuh fisik memudar/melebur menjadi siluet bayangan hipersensitif cahaya.
   - Seybay Outpost diklaim memiliki atmosfer pemulih yang dapat menghentikan kerusakan DNA ini.
4. **Karakter Utama (Aldo & Keluarga):**
   - **Aldo:** Petugas loket validasi tiket & bagasi Azura Corp.
   - **Nasya:** Istri Aldo.
   - **Virly & Kraisa:** Dua anak Aldo yang mulai terpapar mutasi Stadium 1.
   - **Misi Utama:** Mengumpulkan uang kerja untuk membeli tiket evakuasi sebelum peluncuran hari ke-10, sekaligus mencukupi biaya makan dan obat keluarga tiap malam.

---

## 2. CORE GAMEPLAY LOOP

Siklus permainan berjalan harian (Day 1 s.d. Day 10) dengan 3 fase utama:

[Mulai Hari / Briefing Berita & Regulasi Baru]
│
▼
[Fase Shift Loket (Jam Kerja Terbatas)]
├─ NPC Masuk
├─ Verifikasi Berkas (Paspor vs Tiket/Boarding Pass)
├─ Inspeksi Bagasi/Koper (Barang Legal, Ilegal, & Timbangan)
├─ Validasi Aturan Khusus (Penyakit/Mutasi, Aturan Harian via Rulebook)
└─ Keputusan: APPROVE / REJECT / SITA
│
├─ Benar ──> +Credits, +Trust
└─ Salah ──> -Trust, Penalti Denda
│
(Jika Trust < 0 ──> Game Over / Restart Shift)
│
▼
[Fase Malam / Household & Resource Management]
├─ Rekap Penghasilan & Potongan Denda
├─ Bayar Kebutuhan Wajib/Opsional (Ransum Makanan, Obat Salep Anak, Sewa)
├─ Pembelian Tiket Evakuasi (Beli Sekarang vs Simpan Saldo)
└─ Cek Kondisi Kesehatan Keluarga ──> Lanjut ke Hari Berikutnya

---

## 3. MEKANIK UTAMA (CORE MECHANICS)

### A. Pemeriksaan Dokumen (Inspection)
- Mencocokkan keselarasan identitas: Nama pada Paspor vs Boarding Pass.
- Validasi Tanggal Kedaluwarsa (Expiration Date) relatif terhadap tanggal berjalan in-game.
- Validasi segel distrik / cap khusus (pada level lanjutan).

### B. Pemeriksaan Koper (Luggage Inspection)
- Pemain membuka koper dan menata/menggeser item bawaan (Drag and drop).
- Memisahkan item **Legal** (Pakaian, Foto, P3K standar, Ransum kering) dan item **Kontraband/Ilegal** (Senjata tajam, narkotika *Neuro-Drop*, bahan peledak, kristal anomali).
- **Mekanik Berat (Mulai Day 3):** Timbangan bagasi aktif dengan batas maksimal beban (contoh: 35 KG). Barang berlebih harus disita/ditolak.

### C. Sistem Ekonomi, Trust, & Denda
- **Metrik Trust (Maks 100):** Menjadi representasi performa kerja di mata Azura Corp. Poin Trust turun jika salah stempel atau meloloskan kontraband. Peringatan bahaya jika di bawah 30; dipecat (*Fired / Restart Day*) jika turun di bawah 0.
- **Pendapatan:** Upah pokok harian + insentif akurasi per penumpang.
- **Tawaran Suap:** Muncul godaan moral dari NPC ilegal/bermasalah yang menawarkan uang kilat bernilai besar dengan risiko denda audit perusahaan.

### D. Manajemen Malam & Toko Kebutuhan
Setelah shift selesai, uang dipakai untuk menjaga keluarga tetap hidup:
- **Ransum Makanan Bersih (Wajib):** Mencegah anggota keluarga kelaparan dan jatuh sakit.
- **Sewa Tempat Tinggal (Periodik):** Menghindari ancaman penggusuran/penjarahan.
- **Obat & Salep Silikasi (Medis):** Menekan rasa sakit dan laju pembentukan kristal pada anak-anak.
- **Tiket Evakuasi:** Tiket dijual berjenjang (anak lebih murah, dewasa lebih mahal) dan mengalami inflasi tajam (*surge pricing*) tiap pergantian hari.

---

## 4. PROGRESI TINGKAT KESULITAN & ATURAN HARIAN (MILESTONE)

- **Day 1 (Tutorial):** Paspor & Boarding Pass standar (cek nama & tanggal kedaluwarsa). Penumpang masih berwujud manusia normal. Belum ada razia koper.
- **Day 2 (Inspeksi Koper Perdana):** Terjadi insiden berdarah di roket sebelumnya. Razia senjata tajam dan obat terlarang tak berizin.
- **Day 3 (Limit Bobot Bagasi):** Krisis bahan bakar roket. Timbangan koper aktif (Maks 35 KG). Gejala demam & kristalisasi keluarga Aldo mulai memburuk.
- **Day 4 - 6 (Mutasi Stadium 2 & Scanner):** Deteksi demam plasma/suhu tubuh, alat sensor metal/X-Ray, mulai muncul tawaran suap besar.
- **Day 7 - 9 (Mutasi Stadium 3 & Kekacauan Massal):** Penumpang wujud bayangan/anomali, regulasi darurat saling tumpang tindih, dokumen palsu beredar masif.
- **Day 10 (Peluncuran Terakhir):** Seluruh filter aktif simultan. Gerbang ditutup permanen. Penentuan ending.

---

## 5. SKENARIO ENDING (KONDISI AKHIR)

Kelulusan game dievaluasi di akhir Hari ke-10 berdasarkan total lembar tiket yang berhasil dibeli:

1. **Ending 3 - "Evacuated" (Good Ending):**
   - **Syarat:** Berhasil membeli 4 Tiket.
   - **Hasil:** Seluruh keluarga (Aldo, Nasya, Virly, Kraisa) berangkat bersama ke Seybay Outpost.
2. **Ending 1 - "The Sacrifice" (Bittersweet Ending):**
   - **Syarat:** Berhasil membeli 3 Tiket.
   - **Hasil:** Nasya dan kedua anak berangkat; Aldo mengorbankan dirinya tetap tinggal di Bumi.
3. **Ending 2 - "Love Forever" (Tragic Ending):**
   - **Syarat:** Berhasil membeli 2 Tiket.
   - **Hasil:** Kedua anak diberangkatkan ke Seybay; Aldo dan Nasya menetap di Bumi menyongsong kiamat bersama.
4. **Ending 0 - "The Cruelest Choice" / Bad Ending:**
   - **Syarat:** Hanya mampu membeli 1 Tiket (atau 0 tiket).
   - **Hasil:** Pemain dipaksa memilih satu-satunya anak yang diselamatkan sementara sisanya tertinggal di Bumi (atau tewas semua jika 0 tiket).