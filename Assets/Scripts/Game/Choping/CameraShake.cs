using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;

    private Vector3 originalPos;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        originalPos = transform.localPosition;
    }

    /// <summary>
    /// Memanggil guncangan kamera.
    /// </summary>
    /// <param name="duration">Durasi guncangan (detik)</param>
    /// <param name="magnitude">Kekuatan guncangan</param>
    public void Shake(float duration, float magnitude)
    {
        StopAllCoroutines(); // Menghentikan shake yang sedang berjalan jika ada shake baru
        StartCoroutine(ProcessShake(duration, magnitude));
    }

    private IEnumerator ProcessShake(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}