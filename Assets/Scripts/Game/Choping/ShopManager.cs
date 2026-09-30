using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    [Header("--- TIKET KARAKTER ---")]
    public Toggle toggleAldo;
    public Toggle toggleNasya;
    public Toggle toggleVirly;
    public Toggle toggleKraisa;

    public int hargaAldo = 5320;
    public int hargaNasya = 9400;
    public int hargaVirly = 21000;
    public int hargaKraisa = 32000;

    // Format untuk menambahkan jenis obat berapapun secara bebas di Unity
    [System.Serializable]
    public struct DataObat
    {
        public string namaObat;
        public int hargaObat;
        public Toggle toggleCheckbox;
    }

    [Header("--- OBAT-OBATAN ---")]
    public DataObat[] daftarObat;

    [Header("--- UI & NAVIGASI ---")]
    public TMP_Text textTotalUang;
    public TMP_Text textPeringatan; // Opsional: Untuk memunculkan tulisan "Uang tidak cukup"
    public string sceneSelanjutnya = "Day2Scene"; // Ganti dengan nama scene-mu

    void Start()
    {
        if (textPeringatan != null) textPeringatan.gameObject.SetActive(false);
        UpdateTampilanUang();
    }

    public void UpdateTampilanUang()
    {
        if (textTotalUang != null && EconomyManager.Instance != null)
        {
            textTotalUang.text = EconomyManager.Instance.GetNetMoney().ToString();
        }
    }

    // Dipanggil saat tombol "Buy" ditekan
    public void OnClickBuy()
    {
        if (EconomyManager.Instance == null || SaveManager.Instance == null) return;

        int totalBelanja = 0;

        // 1. Kalkulasi Harga Tiket yang dicentang
        if (toggleAldo != null && toggleAldo.isOn) totalBelanja += hargaAldo;
        if (toggleNasya != null && toggleNasya.isOn) totalBelanja += hargaNasya;
        if (toggleVirly != null && toggleVirly.isOn) totalBelanja += hargaVirly;
        if (toggleKraisa != null && toggleKraisa.isOn) totalBelanja += hargaKraisa;

        // 2. Kalkulasi Harga Obat yang dicentang
        if (daftarObat != null)
        {
            foreach (var obat in daftarObat)
            {
                if (obat.toggleCheckbox != null && obat.toggleCheckbox.isOn)
                {
                    totalBelanja += obat.hargaObat;
                }
            }
        }

        int uangBersih = EconomyManager.Instance.GetNetMoney();

        // 3. Eksekusi Pembayaran
        if (uangBersih >= totalBelanja)
        {
            // Potong Uang & Catat Tiket
            if (toggleAldo != null && toggleAldo.isOn) { EconomyManager.Instance.money -= hargaAldo; SaveManager.Instance.PurchaseTicket("Aldo"); }
            if (toggleNasya != null && toggleNasya.isOn) { EconomyManager.Instance.money -= hargaNasya; SaveManager.Instance.PurchaseTicket("Nasya"); }
            if (toggleVirly != null && toggleVirly.isOn) { EconomyManager.Instance.money -= hargaVirly; SaveManager.Instance.PurchaseTicket("Virly"); }
            if (toggleKraisa != null && toggleKraisa.isOn) { EconomyManager.Instance.money -= hargaKraisa; SaveManager.Instance.PurchaseTicket("Kraisa"); }

            // Potong Uang Obat
            if (daftarObat != null)
            {
                foreach (var obat in daftarObat)
                {
                    if (obat.toggleCheckbox != null && obat.toggleCheckbox.isOn)
                    {
                        EconomyManager.Instance.money -= obat.hargaObat;
                    }
                }
            }

            // 4. Majukan Hari (AdvanceDay otomatis memanggil SaveGame di kodemu)
            SaveManager.Instance.AdvanceDay();

            // 5. Pindah Scene
            SceneManager.LoadScene(sceneSelanjutnya);
        }
        else
        {
            // Tampilkan tulisan jika uang tidak cukup
            if (textPeringatan != null)
            {
                textPeringatan.text = "Not enough money!";
                textPeringatan.gameObject.SetActive(true);
            }
        }
    }

    // Dipanggil saat tombol "Later ->" ditekan
    public void OnClickLater()
    {
        if (SaveManager.Instance != null) SaveManager.Instance.AdvanceDay();
        SceneManager.LoadScene(sceneSelanjutnya);
    }
}