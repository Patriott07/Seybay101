using System.Collections;
using UnityEngine;

public class DokumenKoperZoom : MonoBehaviour
{
    [Header("Pengaturan Zoom")]
    public Vector3 posisiTengahLayar = new Vector3(0, 0, -5f); // Posisi target saat di-zoom
    public Vector3 skalaZoom = new Vector3(2f, 2f, 1f); // Ukuran saat membesar
    public float kecepatanZoom = 5f;

    [Header("Status Dokumen")]
    public bool bisaDiklik = false;

    private Vector3 posisiLokalAsli;
    private Vector3 skalaAsli;
    private Quaternion rotasiLokalAsli;

    private bool isZoomed = false;
    private bool sedangAnimasi = false;
    private SpriteRenderer spriteRenderer;
    private int layerAwal;

    void Start()
    {
        // Simpan data relatif terhadap tutup koper
        posisiLokalAsli = transform.localPosition;
        skalaAsli = transform.localScale;
        rotasiLokalAsli = transform.localRotation;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) layerAwal = spriteRenderer.sortingOrder;
    }

    void OnMouseDown()
    {
        if (!bisaDiklik || sedangAnimasi) return;

        isZoomed = !isZoomed;
        StartCoroutine(ProsesZoom(isZoomed));
    }

    IEnumerator ProsesZoom(bool keZoom)
    {
        sedangAnimasi = true;
        float time = 0;

        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;

        // Target posisi dan skala
        Vector3 targetPos = keZoom ? posisiTengahLayar : transform.parent.TransformPoint(posisiLokalAsli);
        Vector3 targetScale = keZoom ? skalaZoom : skalaAsli;

        if (keZoom)
        {
            if (spriteRenderer != null) spriteRenderer.sortingOrder = 100; // Muncul paling depan

            // CEGAH SALTO 3D: Langsung tegak lurus menghadap kamera saat mulai zoom
            transform.rotation = Quaternion.identity;
        }

        while (time < 1)
        {
            time += Time.deltaTime * kecepatanZoom;

            // Kita HANYA menganimasikan Posisi dan Skala (Rotasi tidak di-lerp agar tidak flip 3D)
            transform.position = Vector3.Lerp(startPos, targetPos, time);
            transform.localScale = Vector3.Lerp(startScale, targetScale, time);

            yield return null;
        }

        // Pastikan presisi di akhir
        transform.position = targetPos;
        transform.localScale = targetScale;

        if (!keZoom)
        {
            // Saat kembali ke koper, langsung tempelkan rotasinya mengikuti engsel koper
            transform.localRotation = rotasiLokalAsli;
            if (spriteRenderer != null) spriteRenderer.sortingOrder = layerAwal;
        }

        sedangAnimasi = false;
    }
}