using System.Collections.Generic;
using UnityEngine;

// Satu baris pesan dalam percakapan
[System.Serializable]
public class PhoneLine
{
    [Tooltip("Centang = Aldo yang membalas (tampil di KANAN, diketik otomatis setelah tombol ditahan). Tidak dicentang = pesan dari kontak (KIRI).")]
    public bool dariAldo;

    [TextArea(2, 4)]
    public string teks;
}

// Satu "babak" percakapan. Isi 'Percakapan' berurutan; Aldo bisa membalas berkali-kali.
[System.Serializable]
public class PhoneStage
{
    [Tooltip("Label bebas, hanya untuk memudahkan di Inspector")]
    public string namaStage;

    [Header("Percakapan (urut dari atas ke bawah)")]
    [Tooltip("Pesan kontak keluar otomatis. Saat bertemu baris 'Dari Aldo', game menunggu pemain menahan tombol Balas.")]
    public List<PhoneLine> percakapan = new List<PhoneLine>();
}

public enum PhoneContactTipe
{
    Biasa,        // kontak normal, muncul di inbox dan jalan lewat stages
    AzuraCorp,    // muncul setelah ada laporan kesalahan pertama
    ManagerBoo    // muncul hanya saat Trust habis (spam)
}

[CreateAssetMenu(fileName = "NewPhoneContact", menuName = "Seybey/Phone Contact")]
public class PhoneContactSO : ScriptableObject
{
    [Header("Info Kontak")]
    [Tooltip("ID unik, dipakai untuk memicu pesan dari script lain. Contoh: nasya")]
    public string idKontak;
    public string namaKontak;
    public Sprite avatar;

    [Tooltip("Biasa = kontak normal. AzuraCorp / ManagerBoo = kontak sistem, hanya muncul sesuai workflow.")]
    public PhoneContactTipe tipe = PhoneContactTipe.Biasa;

    [Header("Alur Percakapan (urut dari atas ke bawah)")]
    [Tooltip("Untuk ManagerBoo: isi baris pesan (tanpa centang Dari Aldo) di sini, dipakai sebagai teks spam. Untuk AzuraCorp: tidak dipakai, isi pesannya otomatis dari kode.")]
    public List<PhoneStage> stages = new List<PhoneStage>();
}