using Schema.data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{

    [Header("--- Prefab ---")]
    public GameObject PrefabCheckbox;
    public GameObject ParentContainer;

    [Header("--- TIKET KARAKTER ---")]
    public Toggle toggleAldo;
    public Toggle toggleNasya;
    public Toggle toggleVirly;
    public Toggle toggleKraisa;

    public int hargaAldo = 5320;
    public int hargaNasya = 9400;
    public int hargaVirly = 21000;
    public int hargaKraisa = 32000;

    [Header("--- OBAT-OBATAN ---")]
    public ItemOnDay[] ListItemBuy;

    [Header("--- UI & NAVIGASI ---")]
    public TMP_Text textTotalUang;
    public TMP_Text textPeringatan; // Opsional: Untuk memunculkan tulisan "Uang tidak cukup"
    public string sceneSelanjutnya = "Day2Scene"; // Ganti dengan nama scene-mu

    void Start()
    {
        if (textPeringatan != null)
            textPeringatan.gameObject.SetActive(false);

        // SetShouldBuy();
        DrawCheckbox();
        UpdateTampilanUang();
    }

    public void SetShouldBuy(ItemOnDay Item)
    {
        // foreach (var Item in ListItemBuy)
        // {
            if (Item.toggleCheckbox != null && Item.isShouldBuy)
            {
                Item.toggleCheckbox.isOn = true;
                Item.toggleCheckbox.interactable = false;
            }
        // }
    }

    public void DrawCheckbox()
    {
        foreach (var Item in ListItemBuy)
        {
            GameObject toggle = Instantiate(PrefabCheckbox, ParentContainer.transform);
            SetShouldBuy(Item);
        }
        // PrefabCheckbox
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
        if (EconomyManager.Instance == null || SaveManager.Instance == null)
            return;

        int totalBelanja = 0;

        // 1. Kalkulasi Harga Tiket yang dicentang
        if (toggleAldo != null && toggleAldo.isOn)
            totalBelanja += hargaAldo;
        if (toggleNasya != null && toggleNasya.isOn)
            totalBelanja += hargaNasya;
        if (toggleVirly != null && toggleVirly.isOn)
            totalBelanja += hargaVirly;
        if (toggleKraisa != null && toggleKraisa.isOn)
            totalBelanja += hargaKraisa;

        // 2. Kalkulasi Harga Obat yang dicentang
        if (ListItemBuy != null)
        {
            foreach (var Item in ListItemBuy)
            {
                if (Item.toggleCheckbox != null && Item.toggleCheckbox.isOn)
                {
                    totalBelanja += Item.price;
                }
            }
        }

        int uangBersih = EconomyManager.Instance.GetNetMoney();

        // 3. Eksekusi Pembayaran
        if (uangBersih >= totalBelanja)
        {
            // Potong Uang & Catat Tiket
            if (toggleAldo != null && toggleAldo.isOn)
            {
                EconomyManager.Instance.money -= hargaAldo;
                SaveManager.Instance.PurchaseTicket("Aldo");
            }
            if (toggleNasya != null && toggleNasya.isOn)
            {
                EconomyManager.Instance.money -= hargaNasya;
                SaveManager.Instance.PurchaseTicket("Nasya");
            }
            if (toggleVirly != null && toggleVirly.isOn)
            {
                EconomyManager.Instance.money -= hargaVirly;
                SaveManager.Instance.PurchaseTicket("Virly");
            }
            if (toggleKraisa != null && toggleKraisa.isOn)
            {
                EconomyManager.Instance.money -= hargaKraisa;
                SaveManager.Instance.PurchaseTicket("Kraisa");
            }

            // Potong Uang Obat
            if (ListItemBuy != null)
            {
                foreach (var Item in ListItemBuy)
                {
                    if (Item.toggleCheckbox != null && Item.toggleCheckbox.isOn)
                    {
                        EconomyManager.Instance.money -= Item.price;
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
        if (SaveManager.Instance != null)
            SaveManager.Instance.AdvanceDay();
        SceneManager.LoadScene(sceneSelanjutnya);
    }
}
