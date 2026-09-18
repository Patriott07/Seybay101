using UnityEngine;
using TMPro;

[RequireComponent(typeof(InspectableObject))]
public class DocumentReader : MonoBehaviour
{
    [Header("Komponen UI")]
    public TextMeshProUGUI teksDokumen;

    [Header("Daftar Halaman")]
    [TextArea(3, 5)]
    public string[] daftarHalaman;

    private InspectableObject inspectScript;
    private int halamanSaatIni = 0;
    private bool teksAktif = false;

    // --- VARIABEL UNTUK DETEKSI SWIPE MOUSE ---
    private Vector2 posisiAwalSwipe;
    private Vector2 posisiAkhirSwipe;
    private float batasSwipe = 50f; // Jarak minimal kursor geser agar dihitung sebagai swipe (dalam pixel)

    void Start()
    {
        inspectScript = GetComponent<InspectableObject>();

        if (teksDokumen != null) teksDokumen.gameObject.SetActive(false);
        UpdateTeks();
    }

    void Update()
    {
        bool sedangDibaca = inspectScript.GetIsInspect();
        bool animasiSelesai = Vector3.Distance(transform.position, inspectScript.titikInspeksi.position) < 0.1f;

        // Tampilkan teks HANYA jika dokumen sudah beres di-zoom
        if (sedangDibaca && animasiSelesai && !teksAktif)
        {
            teksAktif = true;
            teksDokumen.gameObject.SetActive(true);
        }
        else if (!sedangDibaca && teksAktif)
        {
            teksAktif = false;
            teksDokumen.gameObject.SetActive(false);
        }

        // --- LOGIKA SWIPE MOUSE DI LAYAR ---
        if (teksAktif)
        {
            // 1. Saat klik kiri ditekan, rekam titik awalnya
            if (Input.GetMouseButtonDown(0))
            {
                posisiAwalSwipe = Input.mousePosition;
            }

            // 2. Saat klik kiri dilepas, rekam titik akhirnya lalu hitung arahnya
            if (Input.GetMouseButtonUp(0))
            {
                posisiAkhirSwipe = Input.mousePosition;
                DeteksiArahSwipe();
            }
        }
    }

    private void DeteksiArahSwipe()
    {
        float jarakX = posisiAkhirSwipe.x - posisiAwalSwipe.x;
        float jarakY = Mathf.Abs(posisiAkhirSwipe.y - posisiAwalSwipe.y);

        // Pastikan geserannya lebih dominan mendatar (horizontal) dan melewati batas minimal
        if (Mathf.Abs(jarakX) > batasSwipe && Mathf.Abs(jarakX) > jarakY)
        {
            if (jarakX < 0)
            {
                // Nilai X minus = Kursor digeser ke Kiri
                HalamanSelanjutnya();
            }
            else if (jarakX > 0)
            {
                // Nilai X positif = Kursor digeser ke Kanan
                HalamanSebelumnya();
            }
        }
    }

    public void HalamanSelanjutnya()
    {
        if (halamanSaatIni < daftarHalaman.Length - 1)
        {
            halamanSaatIni++;
            UpdateTeks();
        }
    }

    public void HalamanSebelumnya()
    {
        if (halamanSaatIni > 0)
        {
            halamanSaatIni--;
            UpdateTeks();
        }
    }

    private void UpdateTeks()
    {
        if (daftarHalaman.Length > 0 && teksDokumen != null)
        {
            teksDokumen.text = daftarHalaman[halamanSaatIni];
        }
    }
}