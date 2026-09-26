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
    public float multiplierBouncing = 1.15f;
    public float tinggiLompatan = 0.3f;
    public Vector3 skalaKecil = Vector3.zero;

    public bool isTerbuka = false;
    private bool sedangAnimasi = false;

    private Vector3 skalaAsli;
    private Vector3 posisiAsli;

    void Start()
    {
        if (objekKoperUtama == null)
            objekKoperUtama = this.transform;

        skalaAsli = objekKoperUtama.localScale;

        if (skalaAsli.x < 0.1f)
            skalaAsli = new Vector3(0.80555f, 0.80555f, 0.80555f);

        posisiAsli = objekKoperUtama.localPosition;

        if (koperTutup != null) koperTutup.SetActive(true);
        if (koperBuka != null) koperBuka.SetActive(false);
        if (containerBarang != null) containerBarang.SetActive(false);
        if (dokumenKoper != null) dokumenKoper.SetActive(false);
    }

    public bool IsOpen()
    {
        return isTerbuka;
    }

    public void ToggleKoper()
    {
        if (sedangAnimasi)
            return;
        StartCoroutine(ProsesTransisi2DPositionalBounce());
    }

    // FUNGSI: Membersihkan status animasi nyangkut tanpa merombak posisi
    public void ResetStatusMurni()
    {
        StopAllCoroutines();
        sedangAnimasi = false;
        isTerbuka = false;

        if (koperTutup != null) koperTutup.SetActive(true);
        if (koperBuka != null) koperBuka.SetActive(false);
        if (containerBarang != null) containerBarang.SetActive(false);
        if (dokumenKoper != null) dokumenKoper.SetActive(false);

        if (objekKoperUtama != null)
        {
            objekKoperUtama.localScale = new Vector3(0.80555f, 0.80555f, 0.80555f);
        }
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
        Vector3 posisiPuncak = posisiAsli + new Vector3(0, tinggiLompatan, 0);

        float time = 0;
        while (time < 1)
        {
            time += Time.deltaTime / waktuBouncing;
            objekKoperUtama.localScale = Vector3.Lerp(skalaAsli, skalaMemantul, time);
            objekKoperUtama.localPosition = Vector3.Lerp(posisiAsli, posisiPuncak, time);
            yield return null;
        }

        time = 0;
        while (time < 1)
        {
            time += Time.deltaTime / waktuAnimasi;
            float smoothTime = Mathf.SmoothStep(0f, 1f, time);
            objekKoperUtama.localScale = Vector3.Lerp(skalaMemantul, skalaKecil, smoothTime);
            objekKoperUtama.localPosition = Vector3.Lerp(posisiPuncak, posisiAsli, smoothTime);
            yield return null;
        }

        objekKoperUtama.localScale = skalaKecil;
        objekKoperUtama.localPosition = posisiAsli;

        isTerbuka = !isTerbuka;

        if (koperTutup != null) koperTutup.SetActive(!isTerbuka);
        if (koperBuka != null) koperBuka.SetActive(isTerbuka);

        if (isTerbuka)
        {
            if (containerBarang != null) containerBarang.SetActive(true);
            if (dokumenKoper != null) dokumenKoper.SetActive(true);

            LuggageManager manager = LuggageManager.Instance;
            if (manager != null && !manager.sudahGenerateBarang)
            {
                manager.GenerateBarangNPC();
                manager.sudahGenerateBarang = true;
            }
        }
        else
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxLuggageClose();
            if (containerBarang != null) containerBarang.SetActive(false);
            if (dokumenKoper != null) dokumenKoper.SetActive(false);
        }

        time = 0;
        while (time < 1)
        {
            time += Time.deltaTime / waktuAnimasi;
            float smoothTime = Mathf.SmoothStep(0f, 1f, time);
            objekKoperUtama.localScale = Vector3.Lerp(skalaKecil, skalaMemantul, smoothTime);
            objekKoperUtama.localPosition = Vector3.Lerp(posisiAsli, posisiPuncak, smoothTime);
            yield return null;
        }

        time = 0;
        while (time < 1)
        {
            time += Time.deltaTime / waktuBouncing;
            objekKoperUtama.localScale = Vector3.Lerp(skalaMemantul, skalaAsli, time);
            objekKoperUtama.localPosition = Vector3.Lerp(posisiPuncak, posisiAsli, time);
            yield return null;
        }

        objekKoperUtama.localScale = skalaAsli;
        objekKoperUtama.localPosition = posisiAsli;

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