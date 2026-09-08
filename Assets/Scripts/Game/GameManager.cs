using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Camera mainCam;
    public bool isCanSpawnNpc = true;
    private int currentDay = 1;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public int GetCurrentDay() => currentDay;

    public void SetCurrentDay(int day) => currentDay = day;

    public void ResetDay()
    {
        // currentDay = 1;
        // SaveManager.Instance?.ResetSave();
        SceneManager.LoadScene("Day" + currentDay);
        Debug.Log("Day direset ke 1.");
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
