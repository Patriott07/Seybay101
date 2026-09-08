using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance;

    [Header("Story Data")]
    public StorySchema currentStory;

    [Header("Scene Names")]
    public string preDaySceneName = "PreDay";
    public string mainDeskSceneName = "MainDesk";
    public string afterShiftSceneName = "AfterShift";
    public string recapSceneName = "RecapDay";

    [Header("UI References")]
    public TypingText typingTextPreDay;
    public TypingText typingTextOnDesk;
    public TypingText typingTextAfterShift;

    private int currentDay = 1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentDay = GameManager.Instance != null ? GameManager.Instance.GetCurrentDay() : 1;
        LoadStoryData(currentDay);
    }

    public void LoadStoryData(int day)
    {
        currentDay = day;
    }

    public void SetStoryData(StorySchema story)
    {
        currentStory = story;
        currentDay = story != null ? story.Day : 1;
    }

    public void ShowPreDay()
    {
        if (currentStory == null || currentStory.preDayText == null || currentStory.preDayText.Count == 0)
            return;

        string combinedText = string.Join("\n", currentStory.preDayText);

        if (typingTextPreDay != null)
        {
            typingTextPreDay.SetText(combinedText);
        }
    }

    public void ShowOnDesk()
    {
        if (currentStory == null || currentStory.onDeskText == null || currentStory.onDeskText.Count == 0)
            return;

        string combinedText = string.Join("\n", currentStory.onDeskText);

        if (typingTextOnDesk != null)
        {
            typingTextOnDesk.SetText(combinedText);
        }
    }

    public void ShowAfterShift()
    {
        if (currentStory == null || currentStory.afterShiftText == null || currentStory.afterShiftText.Count == 0)
            return;

        string combinedText = string.Join("\n", currentStory.afterShiftText);

        if (typingTextAfterShift != null)
        {
            typingTextAfterShift.SetText(combinedText);
        }
    }

    public void OnPreDayComplete()
    {
        if (GameManager.Instance != null)
        {
            SceneManager.LoadScene(mainDeskSceneName);
        }
    }

    public void OnAfterShiftComplete()
    {
        if (GameManager.Instance != null)
        {
            SceneManager.LoadScene(recapSceneName);
        }
    }

    public void AdvanceDay()
    {
        currentDay++;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentDay(currentDay);
        }
        LoadStoryData(currentDay);
    }

    public void SkipAllStory()
    {
        if (typingTextPreDay != null) typingTextPreDay.SkipAll();
        if (typingTextOnDesk != null) typingTextOnDesk.SkipAll();
        if (typingTextAfterShift != null) typingTextAfterShift.SkipAll();
    }

    public void SetupDailyFlow()
    {
        StartCoroutine(DailyFlowCoroutine());
    }

    IEnumerator DailyFlowCoroutine()
    {
        ShowPreDay();
        yield return new WaitUntil(() => typingTextPreDay != null && typingTextPreDay.IsComplete());
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(mainDeskSceneName);

        ShowOnDesk();
        yield return new WaitUntil(() => typingTextOnDesk != null && typingTextOnDesk.IsComplete());
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(afterShiftSceneName);

        ShowAfterShift();
        yield return new WaitUntil(() => typingTextAfterShift != null && typingTextAfterShift.IsComplete());
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(recapSceneName);
    }
}
