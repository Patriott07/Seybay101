using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class InspectableObject : MonoBehaviour
{
    [Header("Inspection Settings")]
    [FormerlySerializedAs("titikInspeksi")]
    public Transform inspectionPoint; // Tempat benda melayang saat dizoom
    [FormerlySerializedAs("skalaZoom")]
    public float zoomScale = 2f;    // Seberapa besar benda membesar (2f = 2x lipat)
    [FormerlySerializedAs("kecepatanAnimasi")]
    public float animationSpeed = 8f;

    private Vector3 deskPosition;
    private Vector3 initialScale;
    private int initialSortingOrder;

    private bool isInspected = false;
    private bool isAnimating = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialScale = transform.localScale;
        initialSortingOrder = spriteRenderer.sortingOrder;
    }

    void OnMouseDown()
    {
        // Cegah klik beruntun saat benda masih bergerak
        if (isAnimating) return;

        if (!isInspected)
        {
            // Benda diangkat untuk diinspeksi
            // Kita simpan posisinya SAAT INI (di meja) agar ia tahu jalan pulang
            deskPosition = transform.position;

            // Kita naikkan layernya menjadi 50 agar tidak tertutup stempel/NPC saat dizoom
            StartCoroutine(AnimateMovement(inspectionPoint.position, initialScale * zoomScale, 50));
        }
        else
        {
            // Benda dikembalikan ke meja
            StartCoroutine(AnimateMovement(deskPosition, initialScale, initialSortingOrder));
        }
    }

    IEnumerator AnimateMovement(Vector3 targetPos, Vector3 targetScale, int targetLayer)
    {
        isAnimating = true;

        // Ubah layer langsung saat mulai bergerak naik
        if (!isInspected) spriteRenderer.sortingOrder = targetLayer;

        isInspected = !isInspected;

        float time = 0;
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;

        while (time < 1)
        {
            time += Time.deltaTime * animationSpeed;

            // Bergerak membesar dan berpindah posisi secara bersamaan
            transform.position = Vector3.Lerp(startPos, targetPos, time);
            transform.localScale = Vector3.Lerp(startScale, targetScale, time);
            yield return null;
        }

        // Kembalikan layer ke awal HANYA saat benda sudah selesai mendarat di meja
        if (!isInspected) spriteRenderer.sortingOrder = targetLayer;

        isAnimating = false;
    }

    public void ForceZoomIn()
    {
        if (!isInspected && !isAnimating)
        {
            deskPosition = transform.position;
            StartCoroutine(AnimateMovement(inspectionPoint.position, initialScale * zoomScale, 50));
        }
    }

    public void ForceZoomOut()
    {
        if (isInspected && !isAnimating)
        {
            StartCoroutine(AnimateMovement(deskPosition, initialScale, initialSortingOrder));
        }
    }
}