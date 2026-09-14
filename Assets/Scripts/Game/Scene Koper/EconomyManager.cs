using TMPro;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    [Header("Pengaturan Nilai Awal (Harian)")]
    public int money = 1500;
    public int punish = 0;
    public int trust = 100;
    public int maxTrustHarian = 100;
    public int batasWarningTrust = 30;

    [Header("Pengaturan Skor Trust")]
    public int rewardTrustBenar = 10;
    public int penaltyTrustLolos = 5;
    public int penaltyTrustSalahSita = 1;

    [Header("Pengaturan Uang")]
    public int rewardUangBenar = 50;
    public int penaltyUangSalah = 25;

    [Header("Referensi UI")]
    // public TextMeshProUGUI textUangHUD;
    // public TextMeshProUGUI textTrustHUD;
    // public TextMeshProUGUI textWarningTrust;
    public string pesanWarning = "PERINGATAN: Tingkat Kepercayaan Kritis! (< 30%)";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        ResetDataGame();
        LoadFromSave();
        HUDManager.Instance.UpdateStat(trust);
    }

    public void ResetDataGame()
    {
        // money = 1500;
        trust = maxTrustHarian;
        HUDManager.Instance.UpdateStat(trust);
        Debug.Log("[DEBUG] Data Uang & Trust berhasil di-reset ke nilai awal!");
    }

    private void LoadFromSave()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSave())
        {
            SaveManager.Instance.LoadSave();
        }
    }

    public void MulaiHariBaru()
    {
        trust = maxTrustHarian;
        SaveManager.Instance?.SaveGame();
        HUDManager.Instance.UpdateStat(trust);
        Debug.Log("Hari Baru Dimulai! Trust direset ke 100.");
    }

    public void TambahUang()
    {
        money += rewardUangBenar;
        Debug.Log($"Current money : {money}");
        HUDManager.Instance.UpdateStat(trust);
    }

    public void KurangiUang(int count)
    {
        punish += count;
        HUDManager.Instance.UpdateStat(trust);
        Debug.Log($"Punishment money : {count}");
        Debug.Log($"Current money : {money - punish}");
    }

    public void TambahTrust(int jumlh)
    {
        trust = Mathf.Clamp(trust + jumlh, 0, maxTrustHarian);
        HUDManager.Instance.UpdateStat(trust);
        
        // LoseCondition.Instance?.CheckLoseCondition();
    }

    public void KurangiTrust(int jumlahPenalty)
    {
        trust -= jumlahPenalty;
        HUDManager.Instance.UpdateStat(trust);
        
        LoseCondition.Instance?.CheckLoseCondition(trust);
    }

    public void ResetTrustToMax()
    {
        trust = maxTrustHarian;
        HUDManager.Instance.UpdateStat(trust);
        
        Debug.Log("Trust direset ke 100.");
    }

    // private void CekWarningTrust()
    // {
    //     if (textWarningTrust != null)
    //     {
    //         if (trust <= batasWarningTrust)
    //         {
    //             textWarningTrust.text = pesanWarning;
    //             textWarningTrust.gameObject.SetActive(true);
    //         }
    //         else
    //         {
    //             textWarningTrust.gameObject.SetActive(false);
    //         }
    //     }
    // }

    // public void HUDManager.Instance.UpdateStat(trust)
    // {
    //     if (textUangHUD != null) textUangHUD.text = "$" + money;
    //     if (textTrustHUD != null) textTrustHUD.text = trust + "%";
    //     
    // }
}
