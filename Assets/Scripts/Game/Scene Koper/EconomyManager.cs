using UnityEngine;
using TMPro;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    [Header("Pengaturan Nilai Awal (Harian)")]
    public int modalAwalUang = 1500;
    public int maxTrustHarian = 100;
    public int batasWarningTrust = 30; // Munculkan warning jika trust <= 30

    [Header("Pengaturan Skor Trust")]
    public int rewardTrustBenar = 10;
    public int penaltyTrustLolos = 5;
    public int penaltyTrustSalahSita = 1;

    [Header("Pengaturan Uang")]
    public int rewardUangBenar = 50;
    public int penaltyUangSalah = 25;

    [Header("Referensi UI")]
    public TextMeshProUGUI textUangHUD;
    public TextMeshProUGUI textTrustHUD;

    [Header("Pengaturan Warning Teks")]
    public TextMeshProUGUI textWarningTrust; // Diubah dari GameObject Panel menjadi TextMeshProUGUI
    [TextArea]
    public string pesanWarning = "PERINGATAN: Tingkat Kepercayaan Kritis! (< 30%)";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Jika belum ada data sama sekali, inisialisasi nilai awal
        if (!PlayerPrefs.HasKey("CurrentUang"))
        {
            ResetDataGame();
        }
        UpdateUI();
    }

    // --- FUNGSI RESET TOTAL (UNTUK DEBUGGING) ---
    [ContextMenu("Reset Game Data (Debug)")]
    public void ResetDataGame()
    {
        PlayerPrefs.SetInt("CurrentUang", modalAwalUang);
        PlayerPrefs.SetInt("CurrentTrust", maxTrustHarian);
        PlayerPrefs.Save();

        UpdateUI();
        Debug.Log("<color=cyan>[DEBUG] Data Uang & Trust berhasil di-reset ke nilai awal!</color>");
    }

    // --- FUNGSI RESET SETIAP PAGI ---
    public void MulaiHariBaru()
    {
        PlayerPrefs.SetInt("CurrentTrust", maxTrustHarian);
        PlayerPrefs.Save();

        UpdateUI();
        Debug.Log("Hari Baru Dimulai! Trust direset ke 100.");
    }

    // --- FUNGSI UANG ---
    public void TambahUang()
    {
        int uang = PlayerPrefs.GetInt("CurrentUang", modalAwalUang);
        PlayerPrefs.SetInt("CurrentUang", uang + rewardUangBenar);
        PlayerPrefs.Save();
        UpdateUI();
    }

    public void KurangiUang()
    {
        int uang = PlayerPrefs.GetInt("CurrentUang", modalAwalUang);
        PlayerPrefs.SetInt("CurrentUang", Mathf.Max(0, uang - penaltyUangSalah));
        PlayerPrefs.Save();
        UpdateUI();
    }

    // --- FUNGSI TRUST ---
    public void TambahTrust()
    {
        int trust = PlayerPrefs.GetInt("CurrentTrust", maxTrustHarian);
        trust = Mathf.Clamp(trust + rewardTrustBenar, 0, maxTrustHarian);
        PlayerPrefs.SetInt("CurrentTrust", trust);
        PlayerPrefs.Save();
        UpdateUI();
        CekWarningTrust();
    }

    public void KurangiTrust(int jumlahPenalty)
    {
        int trust = PlayerPrefs.GetInt("CurrentTrust", maxTrustHarian);
        trust = Mathf.Clamp(trust - jumlahPenalty, 0, maxTrustHarian);
        PlayerPrefs.SetInt("CurrentTrust", trust);
        PlayerPrefs.Save();
        UpdateUI();
        CekWarningTrust();
    }

    private void CekWarningTrust()
    {
        int trust = PlayerPrefs.GetInt("CurrentTrust", maxTrustHarian);

        if (textWarningTrust != null)
        {
            if (trust <= batasWarningTrust)
            {
                textWarningTrust.text = pesanWarning;
                textWarningTrust.gameObject.SetActive(true);
            }
            else
            {
                textWarningTrust.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateUI()
    {
        int trustSekarang = PlayerPrefs.GetInt("CurrentTrust", maxTrustHarian);

        if (textUangHUD != null) textUangHUD.text = "$" + PlayerPrefs.GetInt("CurrentUang", modalAwalUang);
        if (textTrustHUD != null) textTrustHUD.text = trustSekarang + "%";

        // Pastikan status warning ikut diperbarui setiap UI direfresh
        CekWarningTrust();
    }
}