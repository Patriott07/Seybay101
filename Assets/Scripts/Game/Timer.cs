using UnityEditorInternal;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public static Timer Singleton;

    public float elapsedTime = 0f;
    public float endTime = 0f; // Batas waktu timer akan berhenti
    
    public bool isRunning = false; 
    public TextMeshProUGUI timerText;


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

    void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;

            // Jika endTime di-set lebih dari 0, cek apakah waktu sudah habis
            if (endTime > 0 && elapsedTime >= endTime)
            {
                elapsedTime = endTime; // Kunci di angka pas agar tidak kelebihan (misal pas 10.000)
                EndTimer();
            }
        }

        timerText.text = elapsedTime.ToString();

        if (Input.GetKeyDown(KeyCode.A))
        {
            StartTimer(endTime);
        }
    }

    // Fungsi 1: Start biasa (jalan terus tanpa henti, atau pakai nilai endTime dari Inspector)

    // Fungsi 2: Start dengan parameter (Dipanggil dari script lain)
    // Contoh pemanggilan: Timer.Singleton.StartTimer(5.5f);
    public void StartTimer(float targetEndTime)
    {
        endTime = targetEndTime;
        isRunning = true;
    }

    [ContextMenu("End Timer")]
    public void EndTimer()
    {
        isRunning = false;
        Debug.Log("Timer end in: " + elapsedTime + " second");
        
        // Anda bisa tambahkan event/fungsi lain di sini saat timer selesai
    }
}