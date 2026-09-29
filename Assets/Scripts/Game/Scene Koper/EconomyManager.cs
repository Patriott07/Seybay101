using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    [Header("Pengaturan Nilai Awal (Harian)")]
    public int money = 0;
    public int punish = 0;
    public int trust = 100;
    public int maxTrustHarian = 100;
    public int batasWarningTrust = 30;

    [Header("Pengaturan Skor Trust")]
    public int rewardTrustBenar = 10;
    public int penaltyTrustLolos = 5;
    public int penaltyTrustSalahSita = 1;

    [Header("Pengaturan Uang")]
    public int rewardUangBenar = 10;
    public int penaltyUangSalah = 5;

    [Header("Referensi UI")]
    [SerializeField] GameObject detailedMistakeHUD;
    public TextMeshProUGUI textUangHUD;
    public TextMeshProUGUI textTrustHUD;
    public TextMeshProUGUI textWarningTrust;

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
        LoadFromSave();
        HUDManager.Instance.UpdateStat(trust);
    }

    public void ResetDataGame()
    {
        money = 0;
        trust = maxTrustHarian;
        punish = 0;
        HUDManager.Instance.UpdateStat(trust);
        SaveManager.Instance?.SaveGame();
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
        punish = 0;
        SaveManager.Instance?.SaveGame();
        HUDManager.Instance.UpdateStat(trust);
        Debug.Log("Hari Baru Dimulai! Trust direset ke 100, penalty direset.");
    }

    public void TambahUang()
    {
        money += rewardUangBenar;
        AudioManager.Instance.PlaySfxCoin();
        Debug.Log($"Current money : {money}");
        HUDManager.Instance.UpdateStat(trust);
    }

    public void KurangiUang(int count)
    {
        punish += count;
        AudioManager.Instance.PlaySfxCoin();
        SaveManager.Instance?.SaveGame();
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
        SaveManager.Instance?.SaveGame();
        LoseCondition.Instance?.CheckLoseCondition(trust);
    }

    public void ResetTrustToMax()
    {
        trust = maxTrustHarian;
        HUDManager.Instance.UpdateStat(trust);
        SaveManager.Instance?.SaveGame();
        Debug.Log("Trust direset ke 100.");
    }

    public int GetNetMoney() => money - punish;


    public void DetailMistakeHUD()
    {
        if (!detailedMistakeHUD.activeInHierarchy) detailedMistakeHUD.SetActive(true);
        textTrustHUD.text = "trust is = " + trust.ToString();
        textUangHUD.text = "money amount = " + money.ToString();
        if (trust <= 30) textWarningTrust.text = "Warning! trust is in critical condition(<30)";
        else textWarningTrust.text = "trust currently above 30";
    }

    public void DetailMistakeHUDCloseButton()
    {
        
        detailedMistakeHUD.SetActive(false);
    }

    public int GetPenalty() => punish;

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
