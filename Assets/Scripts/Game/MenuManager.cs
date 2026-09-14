using UnityEngine;

public class MenuManager : MonoBehaviour
{
   [Header("Referensi Animator")]
    public Animator menuAnimator; // Masukkan GameObject yang punya Animator ke sini

    [Header("Nama Trigger Animasi")]
    public string animOpen = "Billboard_open";
    public string animClose = "Billboard_close";

    private bool isOpen = false; // Status awal menu tertutup
    public BoxCollider2D tableCollider;

    // Fungsi ini bisa dipanggil lewat Tombol (UI Button OnClick)

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab)) ToggleMenu();
    }
    public void ToggleMenu()
    {
        if (menuAnimator == null)
        {
            Debug.LogWarning("Animator belum di-assign di Inspector!");
            return;
        }

        if (!isOpen)
        {
            // Jika posisi tertutup, jalankan animasi BUKA
            menuAnimator.Play(animOpen, 0, 0);
            isOpen = true;
            tableCollider.enabled = true;
        }
        else
        {
            // Jika posisi terbuka, jalankan animasi TUTUP
            // menuAnimator.ResetTrigger(animOpen);
            // menuAnimator.SetTrigger(animClose);
            menuAnimator.Play(animClose, 0, 0);
            isOpen = false;
            tableCollider.enabled = false;
        }
    }
}
