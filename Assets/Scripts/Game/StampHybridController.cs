using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class StampHybridController : MonoBehaviour
{
    [Header("Object References")]
    [FormerlySerializedAs("scriptTiket")]
    public InspectableObject ticketInspectable;

    // --- Referensi ke NPC Manager ---
    public NpcEntranceManager npcManager;

    [Header("Ink Effects")]
    public GameObject approveMarkPrefab;
    public GameObject rejectMarkPrefab;
    [FormerlySerializedAs("kecepatanAnimasi")]
    public float animationSpeed = 8f;

    private Vector3 initialPosition;
    private Camera cam;
    private bool isProcessing = false;

    private SpriteRenderer spriteRenderer;
    private int initialSortingOrder;

    void Start()
    {
        cam = Camera.main;
        initialPosition = transform.position;

        spriteRenderer = GetComponent<SpriteRenderer>();
        initialSortingOrder = spriteRenderer.sortingOrder;
    }

    void OnMouseDrag()
    {
        if (isProcessing) return;
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        transform.position = mousePos;
    }

    void OnMouseUp()
    {
        if (isProcessing) return;

        Collider2D[] hits = Physics2D.OverlapPointAll(transform.position);
        bool isDroppedInZone = false;

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("ZoneYes"))
            {
                StartCoroutine(DecisionAnimationRoutine(true, hit.transform.position));
                isDroppedInZone = true;
                break;
            }
            else if (hit.CompareTag("ZoneNo"))
            {
                StartCoroutine(DecisionAnimationRoutine(false, hit.transform.position));
                isDroppedInZone = true;
                break;
            }
        }

        if (!isDroppedInZone)
        {
            transform.position = initialPosition;
        }
    }

    IEnumerator DecisionAnimationRoutine(bool isApproved, Vector3 zonePosition)
    {
        isProcessing = true;

        transform.position = zonePosition;
        ticketInspectable.ForceZoomIn();

        yield return new WaitForSeconds(0.8f);

        float time = 0;
        Vector3 buttonPosition = transform.position;
        while (time < 1)
        {
            time += Time.deltaTime * animationSpeed;
            transform.position = Vector3.Lerp(buttonPosition, initialPosition, time);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        spriteRenderer.sortingOrder = 60;

        time = 0;
        Vector3 flightTargetPosition = ticketInspectable.inspectionPoint.position + new Vector3(0f, 0f, -1f);
        while (time < 1)
        {
            time += Time.deltaTime * animationSpeed;
            transform.position = Vector3.Lerp(initialPosition, flightTargetPosition, time);
            yield return null;
        }

        GameObject markPrefab = isApproved ? approveMarkPrefab : rejectMarkPrefab;
        GameObject stampMarkInstance = Instantiate(markPrefab, ticketInspectable.transform.position, Quaternion.identity, ticketInspectable.transform);

        stampMarkInstance.transform.localPosition = new Vector3(0f, 0f, -0.1f);
        stampMarkInstance.GetComponent<SpriteRenderer>().sortingOrder = 51;

        yield return new WaitForSeconds(0.6f);

        time = 0;
        while (time < 1)
        {
            time += Time.deltaTime * animationSpeed;
            transform.position = Vector3.Lerp(flightTargetPosition, initialPosition, time);
            yield return null;
        }

        spriteRenderer.sortingOrder = initialSortingOrder;

        ticketInspectable.ForceZoomOut();
        yield return new WaitForSeconds(0.8f);

        // --- Suruh NPC Pergi! ---
        if (npcManager != null)
        {
            npcManager.UsirNpc(isApproved);
        }
        else
        {
            Debug.LogWarning("Kamu belum memasukkan Game Manager ke kolom Npc Manager di Inspector Stempel!");
        }

        isProcessing = false;
    }
}