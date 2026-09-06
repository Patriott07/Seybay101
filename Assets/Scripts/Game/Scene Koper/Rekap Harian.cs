using UnityEngine;
using TMPro;

public class RekapHarianUI : MonoBehaviour
{
    [Header("Referensi UI")]
    public GameObject panelRekap; // Masukkan objek Panel utama rekap di sini
    public TextMeshProUGUI textRekapUang;
    public TextMeshProUGUI textRekapTrust;

    void Start()
    {
        // Pastikan panel disembunyikan saat game/hari baru dimulai
        if (panelRekap != null) panelRekap.SetActive(false);
    }

    // --- FUNGSI UNTUK MEMUNCULKAN REKAP (Panggil saat End Shift) ---
    public void TampilkanRekap()
    {
        if (panelRekap != null) panelRekap.SetActive(true);

        // Mengambil data terbaru dari memori
        int sisaUang = PlayerPrefs.GetInt("CurrentUang", 1500);
        int sisaTrust = PlayerPrefs.GetInt("CurrentTrust", 100);

        // Menampilkan ke layar
        if (textRekapUang != null) textRekapUang.text = "Total Uang: $" + sisaUang;
        if (textRekapTrust != null) textRekapTrust.text = "Tingkat Kepercayaan: " + sisaTrust + "%";

        // Opsional: Pause waktu dalam game jika diperlukan
        // Time.timeScale = 0; 
    }

    // --- FUNGSI UNTUK TOMBOL LANJUT KE HARI BERIKUTNYA ---
    public void TombolLanjutHariBaru()
    {
        // 1. Kembalikan waktu jika sebelumnya di-pause
        // Time.timeScale = 1; 

        // 2. Sembunyikan panel
        if (panelRekap != null) panelRekap.SetActive(false);

        // 3. Reset Trust kembali ke 100 melalui EconomyManager
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.MulaiHariBaru();
        }

        // 4. Panggil NPC Pertama untuk hari baru!
        // Jika kamu menaruh fungsi panggil NPC di NpcEntranceManager:
        NpcEntranceManager npcManager = FindObjectOfType<NpcEntranceManager>();
        if (npcManager != null)
        {
            npcManager.PanggilNpcBaru();
        }
    }
}