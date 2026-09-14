using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [Header("Texts Reference")]
    [SerializeField]
    private TMP_Text textTime;

    [SerializeField]
    private TMP_Text textTrust;

    void Awake()
    {
        Instance = this;
    }

    public void UpdateStat(int trust)
    {
        string textShow = $"Trust : {GetStatTrust(trust)} ({trust}%)";
        textTrust.text = textShow;
    }

    string GetStatTrust(int num)
    {
        if (num < 30)
            return "Almost fired.";
        else if (num < 70)
            return "Moderate.";
        else
            return "Good Work!";
    }
}
