using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public static Timer Singleton;

    public float elapsedTime = 0f;
    public float endTime = 0f;

    public bool isRunning = false;
    public TextMeshProUGUI timerText;

    [Header("Display Settings")]
    [Tooltip("Jam awal tampilan")]
    public float displayStartHour = 8f;
    [Tooltip("Jam akhir tampilan")]
    public float displayEndHour = 22f;
    [Tooltip("Durasi waktu real-time dalam menit (default 10 menit)")]
    public float realTimeDurationMinutes = 10f;

    private float realTimeSeconds;

    void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartTimerDefault();
    }

    void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;

            if (endTime > 0 && elapsedTime >= endTime)
            {
                elapsedTime = endTime;
                EndTimer();
            }
        }

        if (timerText != null)
            timerText.text = FormatDisplayTime();
    }

    string FormatDisplayTime()
    {
        float totalDisplayHours = displayEndHour - displayStartHour;
        float progress = Mathf.Clamp01(elapsedTime / realTimeSeconds);
        float currentHour = displayStartHour + (totalDisplayHours * progress);

        int hours = Mathf.FloorToInt(currentHour);
        float minutesFloat = (currentHour - hours) * 60f;
        int minutes = Mathf.FloorToInt(minutesFloat);

        return string.Format("{0:00}.{1:00}", hours, minutes);
    }

    public void StartTimer(float realTimeMinutes = 10f)
    {
        realTimeSeconds = realTimeMinutes * 60f;
        elapsedTime = 0f;
        endTime = realTimeSeconds;
        isRunning = true;
    }

    public void StartTimerDefault()
    {
        StartTimer(realTimeDurationMinutes);
    }

    [ContextMenu("End Timer")]
    public void EndTimer()
    {
        isRunning = false;
        GameManager.Instance.isCanSpawnNpc = false;
        Debug.Log("Timer end in: " + elapsedTime + " second");
    }
}
