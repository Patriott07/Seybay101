using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Pasang di panel (mis. Panel Home) yang punya Image dengan Raycast Target aktif.
/// Mendeteksi geser horizontal: +1 = geser ke kanan, -1 = geser ke kiri.
/// Tombol di dalam panel tidak mengganggu, event drag naik ke parent.
/// </summary>
public class PhoneSwipeArea : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public event System.Action<int> OnSwipe;

    [Tooltip("Jarak minimal geser (pixel) agar dihitung sebagai swipe")]
    public float jarakMinimal = 80f;

    private Vector2 posAwal;

    public void OnBeginDrag(PointerEventData e)
    {
        posAwal = e.position;
    }

    public void OnDrag(PointerEventData e) { }

    public void OnEndDrag(PointerEventData e)
    {
        Vector2 delta = e.position - posAwal;

        // Harus dominan horizontal
        if (Mathf.Abs(delta.x) < jarakMinimal || Mathf.Abs(delta.x) < Mathf.Abs(delta.y)) return;

        OnSwipe?.Invoke(delta.x > 0 ? 1 : -1);
    }
}