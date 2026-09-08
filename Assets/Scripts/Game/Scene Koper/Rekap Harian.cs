using TMPro;
using UnityEngine;

public class RekapHarianUI : MonoBehaviour
{
    [Header("Referensi UI")]
    public GameObject panelRekap;
    public TextMeshProUGUI textRekapUang;
    public TextMeshProUGUI textRekapTrust;

    void Start()
    {
        if (panelRekap != null)
            panelRekap.SetActive(false);
    }

    public void TampilkanRekap()
    {
        if (panelRekap != null)
            panelRekap.SetActive(true);

        int sisaUang = PlayerPrefs.GetInt("CurrentUang", 1500);
        int sisaTrust = PlayerPrefs.GetInt("CurrentTrust", 100);

        if (textRekapUang != null)
            textRekapUang.text = "Total Uang: $" + sisaUang;
        if (textRekapTrust != null)
            textRekapTrust.text = "Tingkat Kepercayaan: " + sisaTrust + "%";

        LoseCondition.Instance?.CheckLoseCondition();
    }

    public void TombolLanjutHariBaru()
    {
        if (panelRekap != null)
            panelRekap.SetActive(false);

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.MulaiHariBaru();
        }

        GameEvent.SpawnNPCOnStartDay?.Invoke(3f);
    }
}
