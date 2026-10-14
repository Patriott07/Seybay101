using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Tombol "Balas Pesan": harus ditahan beberapa detik
public class PhoneHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public float durasiTahan = 1f;

    [Tooltip("Image bertipe Filled untuk progress tahan (opsional)")]
    public Image isiProgress;

    public event Action OnHoldSelesai;

    private bool menahan;
    private float waktu;
    private bool bisaDitekan = true;

    public void SetInteractable(bool value)
    {
        bisaDitekan = value;
        if (!value) Batalkan();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!bisaDitekan) return;
        menahan = true;
        waktu = 0f;
    }

    public void OnPointerUp(PointerEventData eventData) => Batalkan();
    public void OnPointerExit(PointerEventData eventData) => Batalkan();
    void OnDisable() => Batalkan();

    void Update()
    {
        if (!menahan) return;

        waktu += Time.unscaledDeltaTime;
        if (isiProgress != null)
            isiProgress.fillAmount = Mathf.Clamp01(waktu / durasiTahan);

        if (waktu >= durasiTahan)
        {
            Batalkan();
            OnHoldSelesai?.Invoke();
        }
    }

    void Batalkan()
    {
        menahan = false;
        waktu = 0f;
        if (isiProgress != null)
            isiProgress.fillAmount = 0f;
    }
}