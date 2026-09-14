using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections;

public class LoseCondition : MonoBehaviour
{
    public static LoseCondition Instance;

    [Header("UI References")]
    public CanvasGroup loseCanvasGroup;
    public TMP_Text textEndingReason;
    public UnityEngine.UI.Button btnRestartDay;

    [Header("Lose Config")]
    public float fadeDuration = 1.5f;
    public float waitBeforeRestart = 0.5f;

    private bool isLoseTriggered = false;

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
        if (loseCanvasGroup != null)
        {
            loseCanvasGroup.alpha = 0f;
            loseCanvasGroup.blocksRaycasts = false;
            loseCanvasGroup.interactable = false;
        }

       

    }

    public void CheckLoseCondition()
    {
        if (isLoseTriggered) return;
        if (EconomyManager.Instance == null) return;

        int currentTrust = EconomyManager.Instance.trust;
        if (currentTrust <= 0)
        {
            TriggerLose();
        }
    }

    void TriggerLose()
    {
        isLoseTriggered = true;
        StartCoroutine(FadeInLose());
    }

    IEnumerator FadeInLose()
    {
        yield return new WaitForSeconds(waitBeforeRestart);

        if (loseCanvasGroup != null)
        {
            loseCanvasGroup.alpha = 0f;
            loseCanvasGroup.blocksRaycasts = true;
            loseCanvasGroup.interactable = true;
            loseCanvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.InOutQuad);
        }
    }

    public void RestartDay()
    {
        StartCoroutine(ResetAndRestart());
    }

    IEnumerator ResetAndRestart()
    {
        if (loseCanvasGroup != null)
        {
            loseCanvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InOutQuad);
        }

        yield return new WaitForSeconds(fadeDuration + 0.2f);


        isLoseTriggered = false;

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.ResetTrustToMax();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetDay();
        }

        // SaveManager.Instance?.ResetSave();
    }

    public bool IsLoseTriggered() => isLoseTriggered;
}
