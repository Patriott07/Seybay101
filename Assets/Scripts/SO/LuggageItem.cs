using System.Collections;
using UnityEngine;

public class LuggageItem : MonoBehaviour
{
    [Header("Data Spesifik Barang")]
    public LuggageItemData dataBarang;

    private Vector3 posisiAwalKoperLokal;
    private bool sedangDigeser = false;
    private bool sedangAnimasiPulang = false;
    private bool isHovered = false;

    // --- KUNCI PERBAIKAN DRAG: Menyimpan selisih jarak klik ---
    private Vector3 offsetDrag;

    private Camera cam;
    private SpriteRenderer spriteRenderer;
    private int layerAwal;

    private LuggageManager manager;

    void Start()
    {
        cam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        layerAwal = spriteRenderer.sortingOrder;

        posisiAwalKoperLokal = transform.localPosition;
        manager = FindObjectOfType<LuggageManager>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && !sedangAnimasiPulang)
        {
            if (sedangDigeser || isHovered)
            {
                if (sedangDigeser)
                {
                    sedangDigeser = false;
                    if (manager != null) manager.SembunyikanInfoBarang();
                }
                StartCoroutine(PulangKeKoper());
            }
        }
    }

    void OnMouseEnter()
    {
        isHovered = true;
    }

    void OnMouseExit()
    {
        isHovered = false;
    }

    void OnMouseDown()
    {
        if (sedangAnimasiPulang) return;
        sedangDigeser = true;
        spriteRenderer.sortingOrder = 100;

        // --- HITUNG OFFSET SAAT PERTAMA KALI DIKLIK ---
        Vector3 titikMouse = cam.ScreenToWorldPoint(Input.mousePosition);
        offsetDrag = transform.position - titikMouse;

        if (manager != null)
        {
            manager.TampilkanInfoBarang(dataBarang.itemName, dataBarang.itemWeight);
        }
    }

    void OnMouseDrag()
    {
        if (sedangAnimasiPulang) return;

        // --- TERAPKAN OFFSET SAAT BARANG DISERET ---
        Vector3 titikMouse = cam.ScreenToWorldPoint(Input.mousePosition);

        // Posisi barang sekarang = Posisi Mouse + Selisih jarak klik awal
        transform.position = new Vector3(titikMouse.x + offsetDrag.x, titikMouse.y + offsetDrag.y, transform.position.z);
    }

    void OnMouseUp()
    {
        sedangDigeser = false;
        spriteRenderer.sortingOrder = layerAwal;

        if (manager != null)
        {
            manager.SembunyikanInfoBarang();
        }
    }

    IEnumerator PulangKeKoper()
    {
        sedangAnimasiPulang = true;
        isHovered = false;

        float time = 0;
        Vector3 posisiSekarang = transform.localPosition;

        while (time < 1)
        {
            time += Time.deltaTime * 5f;
            transform.localPosition = Vector3.Lerp(posisiSekarang, posisiAwalKoperLokal, time);
            yield return null;
        }

        transform.localPosition = posisiAwalKoperLokal;
        spriteRenderer.sortingOrder = layerAwal;
        sedangAnimasiPulang = false;
    }

    public Vector3 GetPosisiAwal()
    {
        if (transform.parent != null)
        {
            return transform.parent.TransformPoint(posisiAwalKoperLokal);
        }
        return transform.position;
    }

    public bool ApakahDiDalamKoper()
    {
        if (sedangDigeser) return false;

        float jarak = Vector3.Distance(transform.position, GetPosisiAwal());
        if (jarak > 1.5f) return false;

        return true;
    }

    public IEnumerator AnimasiKeluar()
    {
        float time = 0;
        float durasi = 0.5f;
        Vector3 skalaAwal = transform.localScale;
        Vector3 posisiAwal = transform.position;
        Vector3 posisiTujuan = posisiAwal - new Vector3(0f, 1.5f, 0f);

        Color warnaAwal = spriteRenderer != null ? spriteRenderer.color : Color.white;

        while (time < durasi)
        {
            time += Time.deltaTime;
            float t = time / durasi;

            transform.position = Vector3.Lerp(posisiAwal, posisiTujuan, t);
            transform.localScale = Vector3.Lerp(skalaAwal, Vector3.zero, t);

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