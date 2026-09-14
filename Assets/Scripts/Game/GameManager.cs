using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Camera mainCam;
    public bool isCanSpawnNpc = true;
    private int currentDay = 1;
    public float chanceTOGetSomeActiveViolation = 0.2f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public int GetCurrentDay() => currentDay;

    public void SetCurrentDay(int day) => currentDay = day;

    // this front end for Implementation generating violation by currentday
    public void DoViolationGenerateSetup()
    {
        int currentDay = GetCurrentDay();
        DayRuleSet ruleSet = null;

        switch (currentDay)
        {
            case 1:
                // RuleViolation ins2 = new ExpiredPassportViolation();
                // RuleViolation ins3 = new ExpiredPassportViolation();

                // if (Random.Range(0, 100) < chanceTOGetSomeActiveViolation)
                //     ins1.Apply(PassportScript.Instance.currentData);

                // if (Random.Range(0, 100) < chanceTOGetSomeActiveViolation)
                //     ins1.Apply(PassportScript.Instance.currentData);

                // if (Random.Range(0, 100) < chanceTOGetSomeActiveViolation)
                //     ins1.Apply(PassportScript.Instance.currentData);
                ruleSet = new Day1RuleSet();
                break;
            case 2:
                ruleSet = new Day2RuleSet();
                break;
            case 3:
                break;
            default:
                break;
        }

        List<RuleViolation> rules = ruleSet.GetRules();
        foreach (RuleViolation rule in rules)
        {
            if (Random.Range(0f, 1f) < chanceTOGetSomeActiveViolation)
            {
                if (PassportScript.Instance.currentData == null) continue;
                rule.Apply(PassportScript.Instance.currentData);
                Debug.Log(rule.title);
            }
        }
    }

    public void ResetDay()
    {
        SceneManager.LoadScene("Day" + currentDay);
        Debug.Log("Day direset.");
    }

    public void EndShiftAndLoadScene()
    {
        SceneManager.LoadScene("RecapDay");
    }

    public void NextDay()
    {
        // currentDay++;
        SaveManager.Instance?.SaveGame();
        SceneManager.LoadScene("AfterShift");
    }
}
