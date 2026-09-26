using System.Collections;
using UnityEngine;

public class LuggageItem : MonoBehaviour
{
    [Header("Data Spesifik Barang")]
    public LuggageItemData dataBarang;

    [Header("Batas Area Meja (World Space)")]
    public Vector2 mejaMin;
    public Vector2 mejaMax;
    private bool pakaiBatchManual = true;

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
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(0.85f, 0.85f, 0.85f, 1f);

        cam = GameManager.Instance.mainCam;

        int urutanTumpukan = transform.GetSiblingIndex();
        spriteRenderer.sortingOrder = urutanTumpukan;
        layerAwal = spriteRenderer.sortingOrder;

        Vector3 posLokal = transform.localPosition;
        posLokal.z = urutanTumpukan * -0.01f;
        transform.localPosition = posLokal;

        posisiAwalKoperLokal = transform.localPosition;
        manager = LuggageManager.Instance;
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
                    KembalikanPosisiZ(); // Reset kedalaman Z jika klik kanan di udara
                    // if (manager != null) manager.SembunyikanInfoBarang();
                }
                // StartCoroutine(PulangKeKoper());
                StartCoroutine(SnapKeKoper());
            }
        }
    }

    void OnMouseEnter()
    {
        isHovered = true;
        spriteRenderer.color = Color.white; // hover state
    }

    void OnMouseExit()
    {
        isHovered = false;
        spriteRenderer.color = new Color(0.85f, 0.85f, 0.85f, 1f); // abu-abu sedikit
    }

    // === [PERBAIKAN 1: MENCEGAH KURSOR TERBALIK KARENA KEDALAMAN KAMERA] ===
    private Vector3 DapatkanPosisiMouse()
    {
        Vector3 posisiLayar = Input.mousePosition;
        // Mathf.Abs memaksa nilai jarak selalu positif, sehingga arah geser tidak akan pernah terbalik
        posisiLayar.z = Mathf.Abs(cam.transform.position.z - transform.position.z);
        return cam.ScreenToWorldPoint(posisiLayar);
    }
    // =======================================================================

    void OnMouseDown()
    {
        spriteRenderer.color = Color.white; // hover state
        if (sedangAnimasiPulang)
            return;
        sedangDigeser = true;

        // Naikkan layer gambar ke paling depan saat dipegang
        spriteRenderer.sortingOrder = layerAwal + 1000;

        // Tarik fisik barang ke paling depan kamera (Z = -5) agar tidak menyangkut barang lain
        Vector3 tempPos = transform.position;
        tempPos.z = -5f;
        transform.position = tempPos;

        // Hitung selisih jarak penjepit (Offset) akurat
        offsetDrag = transform.position - DapatkanPosisiMouse();

        // if (manager != null)
        // {
        //      manager.TampilkanInfoBarang(dataBarang.itemName, dataBarang.itemWeight);
        // }
    }

    void OnMouseDrag()
    {
        if (sedangAnimasiPulang)
            return;

        // === [PERBAIKAN 2: GUNAKAN FUNGSI YANG SUDAH DIPERBAIKI, JANGAN MENTAHAN] ===
        Vector3 titikMouse = DapatkanPosisiMouse();

        Vector3 targetPos = new Vector3(
            titikMouse.x + offsetDrag.x,
            titikMouse.y + offsetDrag.y,
            transform.position.z
        );

        if (pakaiBatchManual)
        {
            // Mencegah bug terbalik jika angka Min dan Max di Inspector tertukar
            float batasMinX = Mathf.Min(mejaMin.x, mejaMax.x);
            float batasMaxX = Mathf.Max(mejaMin.x, mejaMax.x);
            float batasMinY = Mathf.Min(mejaMin.y, mejaMax.y);
            float batasMaxY = Mathf.Max(mejaMin.y, mejaMax.y);

            targetPos.x = Mathf.Clamp(targetPos.x, batasMinX, batasMaxX);
            targetPos.y = Mathf.Clamp(targetPos.y, batasMinY, batasMaxY);
        }

        transform.position = targetPos;
        // ============================================================================
    }

    void OnMouseUp()
    {
        if (!sedangDigeser) return;
        sedangDigeser = false;

        spriteRenderer.sortingOrder = layerAwal;
        KembalikanPosisiZ();

        if (pakaiBatchManual)
        {
            // Sesuaikan juga pengecekan drop agar anti-terbalik
            float batasMinX = Mathf.Min(mejaMin.x, mejaMax.x);
            float batasMaxX = Mathf.Max(mejaMin.x, mejaMax.x);
            float batasMinY = Mathf.Min(mejaMin.y, mejaMax.y);
            float batasMaxY = Mathf.Max(mejaMin.y, mejaMax.y);

            bool diDalamMeja = transform.position.x >= batasMinX && transform.position.x <= batasMaxX &&
                               transform.position.y >= batasMinY && transform.position.y <= batasMaxY;
            if (!diDalamMeja)
            {
                // StartCoroutine(SnapKeKoper());
                // return;
            }
        }

        LuggageManager.Instance.HitungBeratRealtime();
    }

    IEnumerator SnapKeKoper()
    {
        float time = 0;
        Vector3 startPos = transform.position;
        Vector3 tujuan = transform.parent != null
            ? transform.parent.TransformPoint(posisiAwalKoperLokal)
            : posisiAwalKoperLokal;
        tujuan = new Vector3(tujuan.x, tujuan.y, startPos.z);

        while (time < 1)
        {
            time += Time.deltaTime * 8f;
            transform.position = Vector3.Lerp(startPos, tujuan, time);
            yield return null;
        }

        transform.position = tujuan;
        spriteRenderer.sortingOrder = layerAwal;
        KembalikanPosisiZ();
        LuggageManager.Instance.HitungBeratRealtime();
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
        if (sedangDigeser)
            return false;

        float jarak = Vector3.Distance(transform.position, GetPosisiAwal());
        if (jarak > 1.5f)
            return false;

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