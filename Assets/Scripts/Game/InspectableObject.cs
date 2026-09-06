using System.Collections;
using UnityEngine;

public class InspectableObject : MonoBehaviour
{
    [Header("Pengaturan Inspeksi")]
    public Transform titikInspeksi; // Tempat benda melayang saat dizoom
    public float skalaZoom = 2f;    // Seberapa besar benda membesar (2f = 2x lipat)
    public float kecepatanAnimasi = 8f;

    private Vector3 posisiMeja;
    private Vector3 skalaAwal;
    private int urutanLayerAwal;

    private bool isInspected = false;
    private bool sedangAnimasi = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        skalaAwal = transform.localScale;
        urutanLayerAwal = spriteRenderer.sortingOrder;
    }

    void OnMouseDown()
    {
        // Cegah klik beruntun saat benda masih bergerak
        if (sedangAnimasi) return;

        if (!isInspected)
        {
            // Benda diangkat untuk diinspeksi
            // Kita simpan posisinya SAAT INI (di meja) agar ia tahu jalan pulang
            posisiMeja = transform.position;

            // Kita naikkan layernya menjadi 50 agar tidak tertutup stempel/NPC saat dizoom
            StartCoroutine(AnimasiGerak(titikInspeksi.position, skalaAwal * skalaZoom, 50));
        }
        else
        {
            // Benda dikembalikan ke meja
            StartCoroutine(AnimasiGerak(posisiMeja, skalaAwal, urutanLayerAwal));
        }
    }

    IEnumerator AnimasiGerak(Vector3 targetPos, Vector3 targetScale, int targetLayer)
    {
        sedangAnimasi = true;

        // Ubah layer langsung saat mulai bergerak naik
        if (!isInspected) spriteRenderer.sortingOrder = targetLayer;

        isInspected = !isInspected;

        float time = 0;
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;

        while (time < 1)
        {
            time += Time.deltaTime * kecepatanAnimasi;

            // Bergerak membesar dan berpindah posisi secara bersamaan
            transform.position = Vector3.Lerp(startPos, targetPos, time);
            transform.localScale = Vector3.Lerp(startScale, targetScale, time);
            yield return null;
        }

        // Kembalikan layer ke awal HANYA saat benda sudah selesai mendarat di meja
        if (!isInspected) spriteRenderer.sortingOrder = targetLayer;

        sedangAnimasi = false;
    }

    // Tambahkan 2 fungsi ini di bagian bawah script InspectableObject
    public void PaksaZoomIn()
    {
        if (!isInspected && !sedangAnimasi)
        {
            posisiMeja = transform.position;
            StartCoroutine(AnimasiGerak(titikInspeksi.position, skalaAwal * skalaZoom, 50));
        }
    }

    public void PaksaZoomOut()
    {
        if (isInspected && !sedangAnimasi)
        {
            StartCoroutine(AnimasiGerak(posisiMeja, skalaAwal, urutanLayerAwal));
        }
    }
}