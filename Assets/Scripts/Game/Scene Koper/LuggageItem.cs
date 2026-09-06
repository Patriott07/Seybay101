using System.Collections;
using UnityEngine;

public class LuggageItem : MonoBehaviour
{
    [Header("Data Spesifik Barang")]
    public LuggageItemData dataBarang;

    private Vector3 posisiAwalKoper;
    private bool sedangDigeser = false;
    private bool sedangAnimasiPulang = false;
    private Camera cam;
    private SpriteRenderer spriteRenderer;
    private int layerAwal;

    private LuggageManager manager;

    void Start()
    {
        cam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        layerAwal = spriteRenderer.sortingOrder;
        posisiAwalKoper = transform.position;

        manager = FindObjectOfType<LuggageManager>();
    }

    void OnMouseDown()
    {
        if (sedangAnimasiPulang) return;
        sedangDigeser = true;
        spriteRenderer.sortingOrder = 100;

        if (manager != null)
        {
            manager.TampilkanInfoBarang(dataBarang.itemName, dataBarang.itemWeight);
        }
    }

    void OnMouseDrag()
    {
        if (sedangAnimasiPulang) return;
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = transform.position.z;
        transform.position = mousePos;
    }

    void OnMouseUp()
    {
        sedangDigeser = false;
        spriteRenderer.sortingOrder = 10;

        if (manager != null)
        {
            manager.SembunyikanInfoBarang();
        }
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1) && !sedangDigeser && !sedangAnimasiPulang)
        {
            StartCoroutine(PulangKeKoper());
        }
    }

    IEnumerator PulangKeKoper()
    {
        sedangAnimasiPulang = true;
        float time = 0;
        Vector3 posisiSekarang = transform.position;

        while (time < 1)
        {
            time += Time.deltaTime * 5f;
            transform.position = Vector3.Lerp(posisiSekarang, posisiAwalKoper, time);
            yield return null;
        }

        transform.position = posisiAwalKoper;
        spriteRenderer.sortingOrder = layerAwal;
        sedangAnimasiPulang = false;
    }

    public Vector3 GetPosisiAwal()
    {
        return posisiAwalKoper;
    }

    // --- ANIMASI KELUAR (DARI MEJA KE BAWAH + MENGECIL + FADE) ---
    public IEnumerator AnimasiKeluar()
    {
        float time = 0;
        float durasi = 0.5f;
        Vector3 skalaAwal = transform.localScale;
        Vector3 posisiAwal = transform.position;

        // Menentukan posisi tujuan ke arah bawah meja (mengurangi sumbu Y)
        Vector3 posisiTujuan = posisiAwal - new Vector3(0f, 1.5f, 0f);

        Color warnaAwal = spriteRenderer != null ? spriteRenderer.color : Color.white;

        while (time < durasi)
        {
            time += Time.deltaTime;
            float t = time / durasi;

            // Bergerak turun ke bawah meja
            transform.position = Vector3.Lerp(posisiAwal, posisiTujuan, t);

            // Mengecil hingga 0 (efek menjauh / ke belakang)
            transform.localScale = Vector3.Lerp(skalaAwal, Vector3.zero, t);

            // Memudar transparansinya (Fade out)
            if (spriteRenderer != null)
            {
                Color warnaBaru = warnaAwal;
                warnaBaru.a = Mathf.Lerp(1f, 0f, t);
                spriteRenderer.color = warnaBaru;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}