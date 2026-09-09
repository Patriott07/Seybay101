using System.Collections;
using UnityEngine;

public class AnimasiKoperSimple : MonoBehaviour
{
    [Header("Referensi Koper Utama")]
    public Transform objekKoperUtama;

    [Header("Referensi Objek 2D")]
    public GameObject koperTutup;
    public GameObject koperBuka;

    [Header("Isi Koper")]
    public GameObject containerBarang;
    public GameObject dokumenKoper;

    [Header("Pengaturan Animasi & Juice")]
    public float waktuAnimasi = 0.25f;
    public float waktuBouncing = 0.1f;
    public float multiplierBouncing = 1.15f; // Efek melar membesar
    public float tinggiLompatan = 0.3f; // BARU: Seberapa tinggi koper melompat dari meja
    public Vector3 skalaKecil = Vector3.zero;

    private bool isTerbuka = false;
    private bool sedangAnimasi = false;
    private bool sudahGenerateBarang = false;

    private Vector3 skalaAsli;
    private Vector3 posisiAsli; // BARU: Untuk menyimpan koordinat nempel di meja

    void Start()
    {
        if (objekKoperUtama == null) objekKoperUtama = this.transform;

        skalaAsli = objekKoperUtama.localScale;
        if (skalaAsli.x < 0.1f) skalaAsli = new Vector3(1f, 1f, 1f);

        // Simpan titik mendarat awal koper
        posisiAsli = objekKoperUtama.localPosition;

        if (koperTutup != null) koperTutup.SetActive(true);
        if (koperBuka != null) koperBuka.SetActive(false);

        if (containerBarang != null) containerBarang.SetActive(false);
        if (dokumenKoper != null) dokumenKoper.SetActive(false);
    }

    public void ToggleKoper()
    {
        if (sedangAnimasi) return;
        StartCoroutine(ProsesTransisi2DPositionalBounce());
    }

    IEnumerator ProsesTransisi2DPositionalBounce()
    {
        sedangAnimasi = true;

        if (isTerbuka && dokumenKoper != null)
        {
            DokumenKoperZoom docZoom = dokumenKoper.GetComponent<DokumenKoperZoom>();
            if (docZoom != null) docZoom.bisaDiklik = false;
        }

        Vector3 skalaMemantul = skalaAsli * multiplierBouncing;
        // Titik tertinggi saat koper terangkat
        Vector3 posisiPuncak = posisiAsli + new Vector3(0, tinggiLompatan, 0);

        // --- FASE 1: MELOMPAT NAIK (Terangkat dari meja) ---
        float time = 0;
        while (time < 1)
        {
            time += Time.deltaTime / waktuBouncing;
            objekKoperUtama.localScale = Vector3.Lerp(skalaAsli, skalaMemantul, time);
            objekKoperUtama.localPosition = Vector3.Lerp(posisiAsli, posisiPuncak, time);
            yield return null;
        }

        // --- FASE 2: MENGHEMPAS TURUN & MENGECIL (Terbanting ke meja) ---
        time = 0;
        while (time < 1)
        {
            time += Time.deltaTime / waktuAnimasi;
            float smoothTime = Mathf.SmoothStep(0f, 1f, time);
            objekKoperUtama.localScale = Vector3.Lerp(skalaMemantul, skalaKecil, smoothTime);
            objekKoperUtama.localPosition = Vector3.Lerp(posisiPuncak, posisiAsli, smoothTime);
            yield return null;
        }

        // Pastikan nempel di meja dengan skala 0
        objekKoperUtama.localScale = skalaKecil;
        objekKoperUtama.localPosition = posisiAsli;

        // --- FASE 3: GANTI GAMBAR & MUNCULKAN ISI ---
        isTerbuka = !isTerbuka;

        if (koperTutup != null) koperTutup.SetActive(!isTerbuka);
        if (koperBuka != null) koperBuka.SetActive(isTerbuka);

        if (isTerbuka)
        {
            if (containerBarang != null) containerBarang.SetActive(true);
            if (dokumenKoper != null) dokumenKoper.SetActive(true);

            if (!sudahGenerateBarang)
            {
                LuggageManager manager = FindObjectOfType<LuggageManager>();
                if (manager != null)
                {
                    manager.GenerateBarangNPC();
                    sudahGenerateBarang = true;
                }
            }
        }
        else
        {
            if (containerBarang != null) containerBarang.SetActive(false);
            if (dokumenKoper != null) dokumenKoper.SetActive(false);
        }

        // --- FASE 4: MEMBESAR & MELOMPAT NAIK ---
        time = 0;
        while (time < 1)
        {
            time += Time.deltaTime / waktuAnimasi;
            float smoothTime = Mathf.SmoothStep(0f, 1f, time);
            objekKoperUtama.localScale = Vector3.Lerp(skalaKecil, skalaMemantul, smoothTime);
            objekKoperUtama.localPosition = Vector3.Lerp(posisiAsli, posisiPuncak, smoothTime);
            yield return null;
        }

        // --- FASE 5: JATUH KE MEJA (Settle) ---
        time = 0;
        while (time < 1)
        {
            time += Time.deltaTime / waktuBouncing;
            objekKoperUtama.localScale = Vector3.Lerp(skalaMemantul, skalaAsli, time);
            objekKoperUtama.localPosition = Vector3.Lerp(posisiPuncak, posisiAsli, time);
            yield return null;
        }

        // Kunci posisi akhir agar pas di meja
        objekKoperUtama.localScale = skalaAsli;
        objekKoperUtama.localPosition = posisiAsli;

        // --- SELESAI ---
        if (isTerbuka)
        {
            if (dokumenKoper != null)
            {
                DokumenKoperZoom docZoom = dokumenKoper.GetComponent<DokumenKoperZoom>();
                if (docZoom != null) docZoom.bisaDiklik = true;
            }
        }

        sedangAnimasi = false;
    }
}