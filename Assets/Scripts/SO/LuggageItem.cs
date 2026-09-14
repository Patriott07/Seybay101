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

    private Vector3 offsetDrag;
    private Camera cam;
    private SpriteRenderer spriteRenderer;
    private int layerAwal;

    private LuggageManager manager;

    void Start()
    {
        cam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // --- KUNCI PERBAIKAN: Sinkronisasi Fisika dan Visual (Tanpa Bug) ---
        // Kita ambil urutan spawn barang ini di dalam koper
        int urutanTumpukan = transform.GetSiblingIndex();

        // 1. Jadikan urutan tersebut sebagai layer visual (yang belakangan spawn = di atas)
        spriteRenderer.sortingOrder = urutanTumpukan;
        layerAwal = spriteRenderer.sortingOrder;

        // 2. Majukan sumbu Z-nya sedikit demi sedikit ke arah kamera.
        // Ini memaksa sistem klik fisik Unity percaya bahwa barang ini benar-benar ada di depan.
        Vector3 posLokal = transform.localPosition;
        posLokal.z = urutanTumpukan * -0.01f;
        transform.localPosition = posLokal;

        // Simpan posisi amannya
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
                    KembalikanPosisiZ(); // Reset kedalaman Z jika klik kanan di udara
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

    private Vector3 DapatkanPosisiMouse()
    {
        Vector3 titikMouse = cam.ScreenToWorldPoint(Input.mousePosition);
        titikMouse.z = transform.position.z;
        return titikMouse;
    }

    void OnMouseDown()
    {
        if (sedangAnimasiPulang) return;
        sedangDigeser = true;

        // Naikkan layer gambar ke paling depan saat dipegang
        spriteRenderer.sortingOrder = layerAwal + 1000;

        // Tarik fisik barang ke paling depan kamera (Z = -5) agar tidak menyangkut barang lain
        Vector3 tempPos = transform.position;
        tempPos.z = -5f;
        transform.position = tempPos;

        // Hitung selisih jarak penjepit (Offset) akurat
        offsetDrag = transform.position - DapatkanPosisiMouse();

        if (manager != null)
        {
            manager.TampilkanInfoBarang(dataBarang.itemName, dataBarang.itemWeight);
        }
    }

    void OnMouseDrag()
    {
        if (sedangAnimasiPulang || !sedangDigeser) return;
        transform.position = DapatkanPosisiMouse() + offsetDrag;
    }

    void OnMouseUp()
    {
        if (!sedangDigeser) return;
        sedangDigeser = false;

        // Kembalikan gambar ke tumpukan semula
        spriteRenderer.sortingOrder = layerAwal;
        KembalikanPosisiZ(); // Kembalikan fisik ke tumpukan semula

        if (manager != null)
        {
            manager.SembunyikanInfoBarang();
        }
    }

    // Fungsi pembantu untuk mengembalikan kedalaman Z barang
    private void KembalikanPosisiZ()
    {
        Vector3 tempPos = transform.localPosition;
        tempPos.z = posisiAwalKoperLokal.z;
        transform.localPosition = tempPos;
    }

    IEnumerator PulangKeKoper()
    {
        sedangAnimasiPulang = true;
        isHovered = false;
        sedangDigeser = false;

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