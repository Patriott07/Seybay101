using System.Collections;
using UnityEngine;

public class AnimasiKoperSimple : MonoBehaviour
{
    [Header("Referensi Objek")]
    public Transform engselTutup;
    public GameObject containerBarang;

    // --- TAMBAHAN BARU ---
    public GameObject dokumenKoper; // Tempat memasukkan objek Doc dari Hierarchy

    [Header("Pengaturan Animasi")]
    public float waktuAnimasi = 0.5f;
    public float sudutBukaX = 180f;

    private bool isTerbuka = false;
    private bool sedangAnimasi = false;
    private bool sudahGenerateBarang = false;

    void Start()
    {
        // Pastikan barang dan dokumen tersembunyi saat mulai
        if (containerBarang != null) containerBarang.SetActive(false);
        if (dokumenKoper != null) dokumenKoper.SetActive(false);
    }

    public void ToggleKoper()
    {
        if (sedangAnimasi) return;
        StartCoroutine(ProsesBukaEngsel3D());
    }

    IEnumerator ProsesBukaEngsel3D()
    {
        sedangAnimasi = true;

        // JIKA MAU TUTUP KOPER
        if (isTerbuka)
        {
            // Sembunyikan barang dan dokumen SEBELUM animasi tutup dimulai
            if (containerBarang != null) containerBarang.SetActive(false);
            if (dokumenKoper != null) dokumenKoper.SetActive(false);
        }

        float time = 0;
        Quaternion rotasiAwal = engselTutup.localRotation;
        Quaternion targetRotasi = isTerbuka ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(sudutBukaX, 0, 0);

        while (time < 1)
        {
            time += Time.deltaTime / waktuAnimasi;
            engselTutup.localRotation = Quaternion.Lerp(rotasiAwal, targetRotasi, time);
            yield return null;
        }

        engselTutup.localRotation = targetRotasi;
        isTerbuka = !isTerbuka;

        // JIKA KOPER BARU SAJA SELESAI TERBUKA
        if (isTerbuka)
        {
            // 1. Munculkan barang dan dokumen SETELAH animasi selesai
            if (containerBarang != null) containerBarang.SetActive(true);
            if (dokumenKoper != null) dokumenKoper.SetActive(true);

            // 2. Generate Barang (Jika belum)
            if (!sudahGenerateBarang)
            {
                LuggageManager manager = FindObjectOfType<LuggageManager>();
                if (manager != null)
                {
                    manager.GenerateBarangNPC();
                    sudahGenerateBarang = true;
                }
            }

            // 3. Izinkan dokumen diklik
            if (dokumenKoper != null)
            {
                DokumenKoperZoom docZoom = dokumenKoper.GetComponent<DokumenKoperZoom>();
                if (docZoom != null) docZoom.bisaDiklik = true;
            }
        }

        sedangAnimasi = false;
    }
}