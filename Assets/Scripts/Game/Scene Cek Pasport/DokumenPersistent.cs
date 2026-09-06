using UnityEngine;

public class DokumenPersistent : MonoBehaviour
{
    [Header("Identitas Dokumen")]
    [Tooltip("Beri nama unik, misal: 'IDCard', 'Tiket', 'Paspor'")]
    public string idDokumenUnik;

    private Vector3 offset;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        // Saat Scene dimuat, cek apakah dokumen ini punya posisi yang tersimpan
        if (PlayerPrefs.HasKey(idDokumenUnik + "_PosX"))
        {
            float x = PlayerPrefs.GetFloat(idDokumenUnik + "_PosX");
            float y = PlayerPrefs.GetFloat(idDokumenUnik + "_PosY");

            // Pindahkan dokumen ke posisi terakhir yang disimpan
            transform.position = new Vector3(x, y, transform.position.z);
        }
    }

    void OnMouseDown()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - new Vector3(mousePos.x, mousePos.y, transform.position.z);
    }

    void OnMouseDrag()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePos.x, mousePos.y, transform.position.z) + offset;
    }

    void OnMouseUp()
    {
        // SAAT MOUSE DILEPAS, SIMPAN POSISI TERAKHIR KE MEMORI
        PlayerPrefs.SetFloat(idDokumenUnik + "_PosX", transform.position.x);
        PlayerPrefs.SetFloat(idDokumenUnik + "_PosY", transform.position.y);
        PlayerPrefs.Save();
    }
}