using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance;

    [Header("Story Data")]
    public StorySchema currentStory;

    [Header("Scene Names")]
    public bool isLoadSceneInEnd = true;
    public string mainDeskSceneName = "MainDesk";

    // public string preDaySceneName = "PreDay";
    // public string afterShiftSceneName = "AfterShift";
    // public string recapSceneName = "RecapDay";
    public bool AutoPlayText = true;
    public bool isCanSkip = true;

    [Header("UI References")]
    public TypingText typingText;
    public CanvasGroup contentBlocker;
    public Animator contentAnimationController;
    public CanvasGroup contentGroup;
    public float hideDur = 0.4f;


    [Header("Audio")]
    public AudioSource audioSource;
    public AudioSource immersiveAudioSource;

    [Header("Storyboard")]
    public UnityEngine.UI.RawImage storyboardRawImage;

    private int currentDay = 1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentDay = GameManager.Instance != null ? GameManager.Instance.GetCurrentDay() : 1;
        // LoadStoryData(currentDay);
        // Pastikan memanggil ShowTypingText() agar Event Listener terpasang sebelum data dimuat
        ShowTypingText();
     
    }

    public void LoadStoryData(int day)
    {
        currentDay = day;

        if (AutoPlayText)
            ShowTypingText();
    }

    public void LoadScene(string name){
        SceneManager.LoadScene(name);
    }

    public void StopAudio(AudioSource audioSource)
    {
        audioSource.Stop();
    }

    public void HideCanvas(bool loadScene){
        if(contentGroup != null){
            contentGroup.blocksRaycasts =false;
            contentGroup.interactable =false;
            contentGroup.DOFade(0, hideDur).OnComplete(() => {if(loadScene) SceneManager.LoadScene(mainDeskSceneName);});
        }
    }

    // public void 

    public void SetStoryData(StorySchema story)
    {
        currentStory = story;
        currentDay = story != null ? story.Day : 1;
    }

    public void ShowTypingText()
    {
        if (currentStory == null)
        {
            Debug.LogWarning("CurrentStory masih null! Pastikan data story sudah di-assign.");
            return;
        }

        if (currentStory.storyText == null || currentStory.storyText.Count == 0)
            return;

        if (typingText != null)
        {
            typingText.OnAudioPlay -= OnAudioPlayHandler;
            typingText.OnAudioPlay += OnAudioPlayHandler;

            typingText.OnLineComplete -= OnLineCompleteHandler;
            typingText.OnLineComplete += OnLineCompleteHandler;

            typingText.OnAllComplete -= OnPreDayAllComplete;
            typingText.OnAllComplete += OnPreDayAllComplete;

            typingText.OnLineStoryData -= OnLineStoryDataHandler;
            typingText.OnLineStoryData += OnLineStoryDataHandler;

            typingText.SetTextWithAudio(currentStory.storyText);
        }
    }

    public void HideContent()
    {
        contentAnimationController.Play("BoxChatHide", 0, 0);
    }

    public void PlayAudio(AudioSource audioSource)
    {
        audioSource.Stop();
        audioSource.Play();
    }

    private void OnAudioPlayHandler(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    private void OnLineStoryDataHandler(Texture texture, AudioClip immersiveClip)
    {
        // 1. Storyboard Texture
        if (storyboardRawImage != null)
        {
            if (texture != null)
            {
                storyboardRawImage.gameObject.SetActive(true);
                storyboardRawImage.texture = texture;
            }
            else
            {
                storyboardRawImage.gameObject.SetActive(false);
            }
        }

        // 2. Immersive Audio
        if (immersiveAudioSource != null)
        {
            if (immersiveClip != null)
            {
                immersiveAudioSource.clip = immersiveClip;
                immersiveAudioSource.Play();
            }
            else
            {
                if (immersiveAudioSource.isPlaying)
                    immersiveAudioSource.Stop();
            }
        }
    }

    private void OnLineCompleteHandler()
    {
        StopCurrentAudio();
    }

    private void OnPreDayAllComplete()
    {
        typingText.OnAudioPlay -= OnAudioPlayHandler;
        typingText.OnLineComplete -= OnLineCompleteHandler;
        typingText.OnAllComplete -= OnPreDayAllComplete;
        typingText.OnLineStoryData -= OnLineStoryDataHandler;
        StopCurrentAudio();
        if (isLoadSceneInEnd)
            SceneManager.LoadScene(mainDeskSceneName);
    }

    private void StopCurrentAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        if (immersiveAudioSource != null && immersiveAudioSource.isPlaying)
        {
            immersiveAudioSource.Stop();
        }
    }

    public void OnPreDayComplete()
    {
        AudioManager.Instance.PlaySfxSceneTransition();
        if (GameManager.Instance != null)
        {
            if (isLoadSceneInEnd)
                SceneManager.LoadScene(mainDeskSceneName);
        }
    }

    // public void OnAfterShiftComplete()
    // {
    //     AudioManager.Instance.PlaySfxSceneTransition();
    //     if (GameManager.Instance != null)
    //     {
    //         SceneManager.LoadScene(recapSceneName);
    //     }
    // }

    public void BlockScreenInteract()
    {
        if (contentBlocker != null)
        {
            contentBlocker.alpha = 1;
            contentBlocker.blocksRaycasts = true;
            contentBlocker.blocksRaycasts = true;
        }
    }
    public void UnBlockScreenInteract()
    {
        if (contentBlocker != null)
        {
            contentBlocker.alpha = 0;
            contentBlocker.blocksRaycasts = false;
            contentBlocker.blocksRaycasts = false;
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
        if (typingText != null)
            typingText.SkipAll();
        StopCurrentAudio();
    }

    public void SetupDailyFlow()
    {
        StartCoroutine(DailyFlowCoroutine());
    }

    IEnumerator DailyFlowCoroutine()
    {
        ShowTypingText();
        yield return new WaitUntil(() => typingText != null && typingText.IsComplete());
        yield return new WaitForSeconds(0.5f);
        if (isLoadSceneInEnd)
            SceneManager.LoadScene(mainDeskSceneName);
    }
}
