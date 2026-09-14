using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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
    private bool isTyping = false;
    private bool isComplete = false;
    private bool isSkipping = false;
    private float skipHoldTimer = 0f;
    private bool isHoldSpace = false;

    private float baseXPosition = 0f;
    private float originalYPosition = 0f;

    public event System.Action OnLineComplete;
    public event System.Action OnAllComplete;

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
            displayedText = "";

            if (textComponent != null)
            {
                textComponent.text = "";
            }

            float currentSpeed = baseCharsPerSecond + Random.Range(-speedVariation, speedVariation);
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

                if (isHoldSpace)
                {
                    skipHoldTimer += delta;
                    UpdateProgressBar();

                    if (skipHoldTimer >= skipHoldTime)
                    {
                        isSkipping = true;
                        SkipAll();
                        yield break;
                    }
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

            if (i < lines.Count - 1 && !isSkipping)
            {
                yield return new WaitForSeconds(lineDelay);
            }
        }

        isTyping = false;
        isComplete = true;
        isSkipping = false;
        continueButton.SetActive(true);
        skipTextIndicator.SetActive(false);
        HideProgressBar();
        OnAllComplete?.Invoke();
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
            float wave = Mathf.Sin(elapsed * wobbleSpeed) * wobbleAmount * (1f - elapsed / duration);
            textComponent.rectTransform.localPosition = new Vector3(
                baseXPosition,
                originalYPosition + wave,
                textComponent.rectTransform.localPosition.z
            );
            yield return null;
        }

        if (textComponent != null && textComponent.rectTransform != null)
        {
            textComponent.rectTransform.localPosition = new Vector3(baseXPosition, originalYPosition, textComponent.rectTransform.localPosition.z);
        }
    }

    void Update()
    {
        isHoldSpace = Input.GetKey(skipKey);

        if (!isHoldSpace)
        {
            skipHoldTimer = 0f;
            HideProgressBar();
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

    public void SkipAll()
    {
        isSkipping = true;
        isTyping = false;
        isComplete = true;
        isHoldSpace = false;
        skipHoldTimer = 0f;
        HideProgressBar();
        continueButton.SetActive(true);
        skipTextIndicator.SetActive(false);

        if (textComponent != null)
        {
            string allText = string.Join("\n", lines);
            displayedText = allText;
            textComponent.text = allText;
        }

        if (textComponent != null && textComponent.rectTransform != null)
        {
            textComponent.rectTransform.localPosition = new Vector3(baseXPosition, originalYPosition, textComponent.rectTransform.localPosition.z);
        }
    }

    public bool IsComplete() => isComplete;
    public bool IsTyping() => isTyping;
    public bool IsSkipping() => isSkipping;

    public void ClearText()
    {
        lines.Clear();
        hasEffect.Clear();
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
