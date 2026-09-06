using System.Collections;
using UnityEngine;

public class StampHybridController : MonoBehaviour
{
    [Header("Hubungan Objek")]
    public InspectableObject scriptTiket;

    // --- BARIS BARU: Referensi ke NPC Manager ---
    public NpcEntranceManager npcManager;

    [Header("Efek Tinta")]
    public GameObject approveMarkPrefab;
    public GameObject rejectMarkPrefab;
    public float kecepatanAnimasi = 8f;

    private Vector3 posisiAwal;
    private Camera cam;
    private bool sedangDiproses = false;

    private SpriteRenderer spriteRenderer;
    private int layerAwal;

    void Start()
    {
        cam = Camera.main;
        posisiAwal = transform.position;

        spriteRenderer = GetComponent<SpriteRenderer>();
        layerAwal = spriteRenderer.sortingOrder;
    }

    void OnMouseDrag()
    {
        if (sedangDiproses) return;
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        transform.position = mousePos;
    }

    void OnMouseUp()
    {
        if (sedangDiproses) return;

        Collider2D[] hits = Physics2D.OverlapPointAll(transform.position);
        bool dilepasDiZona = false;

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("ZoneYes"))
            {
                StartCoroutine(SiklusAnimasiKeputusan(true, hit.transform.position));
                dilepasDiZona = true;
                break;
            }
            else if (hit.CompareTag("ZoneNo"))
            {
                StartCoroutine(SiklusAnimasiKeputusan(false, hit.transform.position));
                dilepasDiZona = true;
                break;
            }
        }

        if (!dilepasDiZona)
        {
            transform.position = posisiAwal;
        }
    }

    IEnumerator SiklusAnimasiKeputusan(bool isApprove, Vector3 posisiZona)
    {
        sedangDiproses = true;

        transform.position = posisiZona;
        scriptTiket.PaksaZoomIn();

        yield return new WaitForSeconds(0.8f);

        float time = 0;
        Vector3 titikDiTombol = transform.position;
        while (time < 1)
        {
            time += Time.deltaTime * kecepatanAnimasi;
            transform.position = Vector3.Lerp(titikDiTombol, posisiAwal, time);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        spriteRenderer.sortingOrder = 60;

        time = 0;
        Vector3 targetTerbang = scriptTiket.titikInspeksi.position + new Vector3(0f, 0f, -1f);
        while (time < 1)
        {
            time += Time.deltaTime * kecepatanAnimasi;
            transform.position = Vector3.Lerp(posisiAwal, targetTerbang, time);
            yield return null;
        }

        GameObject prefabTinta = isApprove ? approveMarkPrefab : rejectMarkPrefab;
        GameObject cetakan = Instantiate(prefabTinta, scriptTiket.transform.position, Quaternion.identity, scriptTiket.transform);

        cetakan.transform.localPosition = new Vector3(0f, 0f, -0.1f);
        cetakan.GetComponent<SpriteRenderer>().sortingOrder = 51;

        yield return new WaitForSeconds(0.6f);

        time = 0;
        while (time < 1)
        {
            time += Time.deltaTime * kecepatanAnimasi;
            transform.position = Vector3.Lerp(targetTerbang, posisiAwal, time);
            yield return null;
        }

        spriteRenderer.sortingOrder = layerAwal;

        scriptTiket.PaksaZoomOut();
        yield return new WaitForSeconds(0.8f);

        // --- BARIS BARU: Suruh NPC Pergi! ---
        if (npcManager != null)
        {
            npcManager.UsirNpc(isApprove);
        }
        else
        {
            Debug.LogWarning("Kamu belum memasukkan Game Manager ke kolom Npc Manager di Inspector Stempel!");
        }

        sedangDiproses = false;
    }
}