using UnityEngine;
using UnityEngine.EventSystems;

public class InspectableObject : MonoBehaviour
{
    [Header("Panel yang dibuka saat klik 2x")]
    public BookPanelController panel;

    [Tooltip("Urutan bagian di list 'bagian' BookPanelController. Contoh: 0 = Rules, 1 = Guide")]
    public int indexBagian = 0;

    [Header("Pengaturan")]
    public float clickThreshold = 0.3f;

    [Header("Debug")]
    [Tooltip("Tampilkan di Console apakah klik diterima atau diblokir (dan alasannya).")]
    public bool logDebug = true;

    private float lastClickTime = 0f;
    private int clickCount = 0;

    // Dipertahankan agar script lain yang memanggil GetIsInspect() tidak error
    public bool GetIsInspect() => BookPanelController.IsPanelOpen;

    void Log(string pesan)
    {
        if (logDebug) Debug.Log("[Inspectable] " + name + ": " + pesan, this);
    }

    // Hanya UI yang bisa diklik (Button dst) yang memblokir klik. Background UI full-screen tidak.
    static readonly System.Collections.Generic.List<RaycastResult> hasilRaycastUI =
        new System.Collections.Generic.List<RaycastResult>();

    static bool PointerDiAtasUIInteraktif()
    {
        if (EventSystem.current == null)
            return false;

        var data = new PointerEventData(EventSystem.current) { position = Input.mousePosition };

        hasilRaycastUI.Clear();
        EventSystem.current.RaycastAll(data, hasilRaycastUI);

        foreach (var r in hasilRaycastUI)
            if (r.gameObject != null &&
                ExecuteEvents.GetEventHandler<IPointerClickHandler>(r.gameObject) != null)
                return true;

        return false;
    }

    void OnMouseDown()
    {
        Log("OnMouseDown diterima");

        // Saat panel terbuka, objek di luar panel tidak bisa di-interact
        if (BookPanelController.IsPanelOpen)
        {
            Log("diblokir, panel buku sedang terbuka");
            return;
        }

        // Hanya UI yang bisa diklik (Button dst) yang memblokir. Background UI tidak.
        if (PointerDiAtasUIInteraktif())
        {
            Log("diblokir, pointer di atas UI interaktif");
            return;
        }

        if (Time.time - lastClickTime <= clickThreshold)
            clickCount++;
        else
            clickCount = 1;

        lastClickTime = Time.time;

        if (clickCount < 2)
            return;

        // === DOUBLE CLICK ===
        clickCount = 0;

        if (panel == null)
        {
            Debug.LogWarning("[Inspectable] " + name + ": field 'panel' belum diisi!", this);
            return;
        }

        // Jika sedang di Desk View, posisi awal animasi dihitung dari Kamera 2
        Camera kameraAsal = DeskViewController.IsViewOpen
            ? DeskViewController.AktifView.deskCamera
            : null;

        panel.Buka(indexBagian, transform, kameraAsal);
        AudioManager.Instance.PlaySfxTicket();
    }
}