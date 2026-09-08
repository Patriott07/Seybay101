using System.Collections;
using DG.Tweening;
using UnityEngine;

public class InspectableObject : MonoBehaviour
{
    [Header("Pengaturan Inspeksi")]
    public Transform titikInspeksi; // Tempat benda melayang saat dizoom
    public float skalaZoom = 2f; // Seberapa besar benda membesar (2f = 2x lipat)
    public float kecepatanAnimasi = 8f;
    public SpriteRenderer bg;

    private Vector3 posisiMeja;
    private Vector3 skalaAwal;
    private int urutanLayerAwal;

    private bool isInspected = false;
    private bool sedangAnimasi = false;
    private SpriteRenderer spriteRenderer;

    private float clickThreshold = 0.3f; // Batas waktu maksimal antara klik 1 dan klik 2 (dalam detik)
    private float lastClickTime = 0f;

    Draggable draggableScript;

    void Start()
    {
        draggableScript = GetComponent<Draggable>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        skalaAwal = transform.localScale;
        urutanLayerAwal = spriteRenderer.sortingOrder;
    }

    public bool GetIsInspect() => isInspected;

    void Update()
    {
        // if(Input.GetMouseButtonDown(0)) OnMouseDown();
    }

    void OnMouseDown()
    {
        // Cegah klik beruntun saat benda masih bergerak
        if (sedangAnimasi)
            return;

        // Cegah document billboard diinspect langsung -> taruh meja baru bisa
        if (draggableScript != null && draggableScript.isOnDesk == false)
            return;

        // --- LOGIKA DOUBLE CLICK ---
        float timeSinceLastClick = Time.time - lastClickTime;

        if (timeSinceLastClick <= clickThreshold)
        {
            // === INI ADALAH DOUBLE CLICK ===
            if (!isInspected)
            {
                posisiMeja = transform.position;
                StartCoroutine(
                    AnimasiGerak(titikInspeksi.position, skalaAwal * skalaZoom, 50, true)
                );
            }
            else
            {
                StartCoroutine(AnimasiGerak(posisiMeja, skalaAwal, urutanLayerAwal, false));
            }

            // Reset waktu klik agar tidak terhitung triple click
            lastClickTime = 0f;
        }
        else
        {
            // Jika ini baru klik pertama, simpan waktunya
            lastClickTime = Time.time;
        }
    }

    IEnumerator AnimasiGerak(Vector3 targetPos, Vector3 targetScale, int targetLayer, bool openBG)
    {
        sedangAnimasi = true;

        // Ubah layer langsung saat mulai bergerak naik
        if (!isInspected)
            spriteRenderer.sortingOrder = targetLayer;

        isInspected = !isInspected;

        float time = 0;
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;

        // bg.
        if (openBG)
            bg.DOFade(0.92f, 0.5f).SetDelay(0.3f);
        else
            bg.DOFade(0, 0.15f);

        while (time < 1)
        {
            time += Time.deltaTime * kecepatanAnimasi;

            // Bergerak membesar dan berpindah posisi secara bersamaan
            transform.position = Vector3.Lerp(startPos, targetPos, time);
            transform.localScale = Vector3.Lerp(startScale, targetScale, time);
            yield return null;
        }

        // Kembalikan layer ke awal HANYA saat benda sudah selesai mendarat di meja
        if (!isInspected)
            spriteRenderer.sortingOrder = targetLayer;

        sedangAnimasi = false;
    }

    // Tambahkan 2 fungsi ini di bagian bawah script InspectableObject
    public void PaksaZoomIn()
    {
        if (!isInspected && !sedangAnimasi)
        {
            posisiMeja = transform.position;
            StartCoroutine(AnimasiGerak(titikInspeksi.position, skalaAwal * skalaZoom, 50, true));
        }
    }

    public void PaksaZoomOut()
    {
        if (isInspected && !sedangAnimasi)
        {
            StartCoroutine(AnimasiGerak(posisiMeja, skalaAwal, urutanLayerAwal, false));
        }
    }
}
