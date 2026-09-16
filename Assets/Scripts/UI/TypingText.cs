using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TypingText : MonoBehaviour
{
    [Header("Typing Config")]
    public float baseCharsPerSecond = 0.04f;
    public float speedVariation = 0.015f;
    public float lineDelay = 0.3f;
    public float wobbleAmount = 3f;
    public float wobbleSpeed = 8f;

    [Header("Skip Config")]
    public float skipHoldTime = 3f;
    public KeyCode skipKey = KeyCode.Space;

    [Header("UI References")]
    public TextMeshProUGUI textComponent;
    public GameObject continueButton;
    public GameObject skipProgressBar;
    public Image progressBarFill;
    public GameObject skipTextIndicator;

    private List<string> lines = new List<string>();
    private List<bool> hasEffect = new List<bool>();
    private int currentLineIndex = 0;
    private string currentLineText = "";
    private string displayedText = "";

    [SerializeField]
    private bool isTyping = false;

    [SerializeField]
    private bool isComplete = false;

    [SerializeField]
    private bool isSkipping = false;

    [SerializeField]
    private float skipHoldTimer = 0f;

    [SerializeField]
    private bool isHoldSpace = false;

    private float baseXPosition = 0f;
    private float originalYPosition = 0f;

    public event System.Action OnLineComplete;
    public event System.Action OnAllComplete;
    public event System.Action<AudioClip> OnAudioPlay;

    [Header("UnityEvent")]
    [SerializeField] private UnityEvent onLineCompleteUnity;
    [SerializeField] private UnityEvent onAllCompleteUnity;
    [SerializeField] private UnityEvent<AudioClip> onAudioPlayUnity;

    private List<AudioClip> audioClips = new List<AudioClip>();
    private List<float> lineSpeeds = new List<float>();

    void Start()
    {
        if (textComponent != null && textComponent.rectTransform != null)
        {
            baseXPosition = textComponent.rectTransform.localPosition.x;
            originalYPosition = textComponent.rectTransform.localPosition.y;
        }
    }

    public void SetText(string fullText)
    {
        lines.Clear();
        hasEffect.Clear();
        audioClips.Clear();
        lineSpeeds.Clear();
        currentLineIndex = 0;
        isTyping = false;
        isComplete = false;
        isSkipping = false;
        skipHoldTimer = 0f;
        isHoldSpace = false;

        string[] splitLines = fullText.Split('\n');
        foreach (var line in splitLines)
        {
            string trimmedLine = line.Trim();
            if (trimmedLine.StartsWith("[Effect]"))
            {
                hasEffect.Add(true);
                lines.Add(trimmedLine.Replace("[Effect]", "").Trim());
            }
            else
            {
                hasEffect.Add(false);
                lines.Add(trimmedLine);
            }
            audioClips.Add(null);
            lineSpeeds.Add(baseCharsPerSecond);
        }

        displayedText = "";
        if (textComponent != null)
        {
            textComponent.text = "";
        }

        if (lines.Count > 0)
        {
            StartCoroutine(TypeAllLines());
        }
    }

    public void SetTextWithAudio(List<TextContentSchema> items)
    {
        lines.Clear();
        hasEffect.Clear();
        audioClips.Clear();
        lineSpeeds.Clear();
        currentLineIndex = 0;
        isTyping = false;
        isComplete = false;
        isSkipping = false;
        skipHoldTimer = 0f;
        isHoldSpace = false;

        foreach (var item in items)
        {
            string trimmedText = item.contentText.Trim();
            if (trimmedText.StartsWith("[Effect]"))
            {
                hasEffect.Add(true);
                lines.Add(trimmedText.Replace("[Effect]", "").Trim());
            }
            else
            {
                hasEffect.Add(false);
                lines.Add(trimmedText);
            }
            audioClips.Add(item.audioSource);
            lineSpeeds.Add(item.textSpeed > 0f ? item.textSpeed : baseCharsPerSecond);
        }

        displayedText = "";
        if (textComponent != null)
        {
            textComponent.text = "";
        }

        if (lines.Count > 0)
        {
            StartCoroutine(TypeAllLines());
        }
    }

    public void StartTyping()
    {
        if (!isTyping && !isComplete && lines.Count > 0)
        {
            StartCoroutine(TypeAllLines());
        }
    }

    IEnumerator TypeAllLines()
    {
        isTyping = true;
        isComplete = false;
        isSkipping = false;
        continueButton.SetActive(false);
        skipTextIndicator.SetActive(true);

        for (int i = 0; i < lines.Count; i++)
        {
            currentLineIndex = i;
            currentLineText = lines[i];
            bool lineHasEffect = hasEffect[i];
            float currentSpeed = lineSpeeds[i];
            AudioClip lineClip = audioClips.Count > i ? audioClips[i] : null;
            displayedText = "";

            if (textComponent != null)
            {
                textComponent.text = "";
            }

            if (lineClip != null)
            {
                OnAudioPlay?.Invoke(lineClip);
                onAudioPlayUnity?.Invoke(lineClip);
            }

            float elapsed = 0f;

            for (int j = 0; j <= currentLineText.Length; j++)
            {
                if (isSkipping)
                {
                    displayedText = currentLineText;
                    if (textComponent != null)
                        textComponent.text = displayedText;
                    break;
                }

                float delta = Time.deltaTime;
                elapsed += delta;

                if (isHoldSpace && skipHoldTimer >= skipHoldTime)
                {
                    isSkipping = true;
                    SkipAll();
                    break;
                }

                if (j < currentLineText.Length)
                {
                    displayedText += currentLineText[j];
                    if (textComponent != null)
                    {
                        textComponent.text = displayedText;
                    }

                    float actualDelay = Mathf.Max(0.001f, currentSpeed);
                    yield return new WaitForSeconds(actualDelay);
                }
            }

            if (lineHasEffect)
            {
                StartCoroutine(WobbleEffect());
            }

            OnLineComplete?.Invoke();
            onLineCompleteUnity?.Invoke();

            if (i < lines.Count - 1 && !isSkipping)
            {
                yield return StartCoroutine(WaitForClickOrSkip());
                if (isSkipping)
                    break;
            }
        }

        isTyping = false;
        isComplete = true;
        isSkipping = false;
        continueButton.SetActive(true);
        skipTextIndicator.SetActive(false);
        HideProgressBar();
        OnAllComplete?.Invoke();
        onAllCompleteUnity?.Invoke();
    }

    IEnumerator WaitForClickOrSkip()
    {
        while (!Input.GetMouseButtonDown(0))
        {
            if (isHoldSpace && isTyping && skipHoldTimer >= skipHoldTime)
            {
                SkipAll(false);
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator WobbleEffect()
    {
        float elapsed = 0f;
        float duration = 0.6f;

        if (textComponent == null || textComponent.rectTransform == null)
            yield break;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float wave =
                Mathf.Sin(elapsed * wobbleSpeed) * wobbleAmount * (1f - elapsed / duration);
            textComponent.rectTransform.localPosition = new Vector3(
                baseXPosition,
                originalYPosition + wave,
                textComponent.rectTransform.localPosition.z
            );
            yield return null;
        }

        if (textComponent != null && textComponent.rectTransform != null)
        {
            textComponent.rectTransform.localPosition = new Vector3(
                baseXPosition,
                originalYPosition,
                textComponent.rectTransform.localPosition.z
            );
        }
    }

    void Update()
    {
        isHoldSpace = Input.GetKey(skipKey);
        if (StoryManager.Instance.isCanSkip)
        {
            if (isHoldSpace && (isTyping || !isComplete))
            {
                skipHoldTimer += Time.deltaTime;
                UpdateProgressBar();
            }
            else
            {
                skipHoldTimer = 0f;
                HideProgressBar();
            }
        }
    }

    void UpdateProgressBar()
    {
        if (skipProgressBar != null && progressBarFill != null && skipTextIndicator != null)
        {
            skipProgressBar.SetActive(true);
            skipTextIndicator.SetActive(true);
            float progress = Mathf.Clamp01(skipHoldTimer / skipHoldTime);
            progressBarFill.fillAmount = progress;
        }
    }

    void HideProgressBar()
    {
        if (skipProgressBar != null)
            skipProgressBar.SetActive(false);
        if (skipTextIndicator != null)
            skipTextIndicator.SetActive(false);
    }

    public void SkipAll(bool showAllText = true)
    {
        isSkipping = true;
        isTyping = false;
        isComplete = true;
        isHoldSpace = false;
        skipHoldTimer = 0f;
        HideProgressBar();
        continueButton.SetActive(true);
        skipTextIndicator.SetActive(false);

        audioClips.Clear();
        lineSpeeds.Clear();
        if (showAllText && textComponent != null)
        {
            string allText = string.Join("\n", lines);
            displayedText = allText;
            textComponent.text = allText;
        }

        if (textComponent != null && textComponent.rectTransform != null)
        {
            textComponent.rectTransform.localPosition = new Vector3(
                baseXPosition,
                originalYPosition,
                textComponent.rectTransform.localPosition.z
            );
        }
    }

    public bool IsComplete() => isComplete;

    public bool IsTyping() => isTyping;

    public bool IsSkipping() => isSkipping;

    public void ClearText()
    {
        lines.Clear();
        hasEffect.Clear();
        audioClips.Clear();
        lineSpeeds.Clear();
        displayedText = "";
        currentLineIndex = 0;
        if (textComponent != null)
            textComponent.text = "";
        isComplete = false;
        isTyping = false;
        isSkipping = false;
        HideProgressBar();
    }
}
