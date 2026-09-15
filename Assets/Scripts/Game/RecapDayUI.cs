using TMPro;
using UnityEngine;

public class RecapDayUI : MonoBehaviour
{
    public static RecapDayUI Instance;

    [Header("UI References")]
    [SerializeField] private TMP_Text textMoney;
    [SerializeField] private TMP_Text textPenalty;
    [SerializeField] private TMP_Text textNetMoney;
    [SerializeField] private TMP_Text textTrust;
    [SerializeField] private TMP_Text textDivider;
    [SerializeField] private TMP_Text textDayInfo;

    [Header("Settings")]
    [SerializeField] private Color positiveNetColor = Color.green;
    [SerializeField] private Color negativeNetColor = Color.red;
    [SerializeField] private Color neutralNetColor = Color.white;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        DisplayRecapData();
    }

    public void DisplayRecapData()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.GetSaveData() == null)
        {
            Debug.LogWarning("[RecapDayUI] No save data found!");
            return;
        }

        SaveManager.Instance.LoadSave();

        int playerCash = SaveManager.Instance.GetSaveData().playerCash;
        int penalty = SaveManager.Instance.GetSaveData().penalty;
        int netMoney = playerCash - penalty;
        int trust = SaveManager.Instance.GetSaveData().trust;
        int currentDay = SaveManager.Instance.GetSaveData().currentDay;

        if (textMoney != null)
            textMoney.text = $"Money : ${playerCash}";

        if (textPenalty != null)
            textPenalty.text = $"Penalty : ${penalty}";

        if (textDivider != null)
            textDivider.text = "------------------------";

        if (textNetMoney != null)
        {
            textNetMoney.text = $"${netMoney}";
            SetNetMoneyColor(netMoney);
        }

        if (textTrust != null)
            textTrust.text = $"Trust : {trust}% remaining. (tomorrow would be refresh)";

        if (textDayInfo != null)
            textDayInfo.text = $"Day {currentDay} Recap";
    }

    private void SetNetMoneyColor(int netMoney)
    {
        if (textNetMoney == null) return;

        if (netMoney > 0)
            textNetMoney.color = positiveNetColor;
        else if (netMoney < 0)
            textNetMoney.color = negativeNetColor;
        else
            textNetMoney.color = neutralNetColor;
    }

    public void RefreshDisplay()
    {
        DisplayRecapData();
    }
}
