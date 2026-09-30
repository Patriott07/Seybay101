using UnityEngine;

public class Draggable : MonoBehaviour
{
    [Header("Batas Area Dinamis")]
    [Tooltip("Batas area MEJA (digunakan saat billboard tertutup).")]
    public Collider2D areaBatasDrag;

    [Tooltip("Batas area BILLBOARD (bisa pakai PolygonCollider2D).")]
    public Collider2D areaBatasBillboard;

    [Tooltip("Tarik objek induk 'Papan Billboard' ke sini.")]
    public GameObject objekBillboard;

    [Tooltip("Jika posisi Y Billboard di bawah angka ini, berarti sedang tampil di layar.")]
    public float batasYBillboardTampil = 5f;

    public Vector3 offset;
    public float snapSpeed = 40f;
    public bool isOnDesk = false;
    private bool isSnapping = false;
    private Vector3 snapTarget;
    public Camera cameraTarget;
    private Vector3 skalaAwal;

    InspectableObject inspectableObjectScript;

    void Start()
    {
        inspectableObjectScript = gameObject.GetComponent<InspectableObject>();
        cameraTarget = GameManager.Instance.mainCam;
        skalaAwal = transform.localScale;
    }

    private Vector3 DapatkanPosisiMouse()
    {
        Vector3 posisiLayar = Input.mousePosition;
        posisiLayar.z = cameraTarget.WorldToScreenPoint(transform.position).z;
        return cameraTarget.ScreenToWorldPoint(posisiLayar);
    }

    void OnMouseDown()
    {
        isSnapping = false;
        AudioManager.Instance.PlaySfxPaper();

        if (inspectableObjectScript.GetIsInspect())
            return;

        offset = transform.position - DapatkanPosisiMouse();
    }

    void OnMouseDrag()
    {
        if (inspectableObjectScript.GetIsInspect())
            return;

        Vector3 targetPosition = DapatkanPosisiMouse() + offset;

        Collider2D pembatasAktif = areaBatasDrag;

        if (objekBillboard != null && objekBillboard.transform.position.y < batasYBillboardTampil)
        {
            if (areaBatasBillboard != null)
            {
                pembatasAktif = areaBatasBillboard;
            }
        }

        // === [PERBAIKAN LOGIKA PEMBATAS POLYGON] ===
        if (pembatasAktif != null)
        {
            // Periksa apakah targetPosition berada di DALAM collider
            if (pembatasAktif.OverlapPoint(targetPosition))
            {
                // Jika di dalam, bebas bergerak ke targetPosition
                // (Tidak perlu modifikasi koordinat X & Y)
            }
            else
            {
                // Jika DI LUAR collider, cari titik terdekat.
                // Ini mengurangi beban komputasi dan mencegah glitch "ClosestPoint" di sudut.
                Vector2 posisiDibatasi = pembatasAktif.ClosestPoint(targetPosition);
                targetPosition.x = posisiDibatasi.x;
                targetPosition.y = posisiDibatasi.y;
            }
        }
        else
        {
            Vector3 minBounds = cameraTarget.ViewportToWorldPoint(new Vector3(0, 0, cameraTarget.nearClipPlane));
            Vector3 maxBounds = cameraTarget.ViewportToWorldPoint(new Vector3(1, 1, cameraTarget.nearClipPlane));

            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
        }
        // ===========================================

        targetPosition.z = transform.position.z;
        transform.position = targetPosition;
    }

    void OnMouseUp()
    {
        isSnapping = false;
        if (inspectableObjectScript.GetIsInspect())
            return;

        FindClosestDeskPoint();
    }

    void Update()
    {
        if (isSnapping)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                snapTarget,
                snapSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, snapTarget) < 0.01f)
            {
                isSnapping = false;
            }
        }
    }

    void FindClosestDeskPoint()
    {
        GameObject[] allDesks = GameObject.FindGameObjectsWithTag("Desk");
        GameObject[] allBillboards = GameObject.FindGameObjectsWithTag("Billboard");

        System.Collections.Generic.List<GameObject> allTargets = new System.Collections.Generic.List<GameObject>();
        allTargets.AddRange(allDesks);
        allTargets.AddRange(allBillboards);

        float closestDistance = Mathf.Infinity;
        Vector3 bestPoint = transform.position;
        GameObject targetObject = null;
        bool foundTarget = false;

        foreach (GameObject obj in allTargets)
        {
            Collider2D col = obj.GetComponent<Collider2D>();

            if (col != null)
            {
                Vector2 closestPoint2D = col.ClosestPoint(transform.position);
                Vector3 closestPoint3D = new Vector3(
                    closestPoint2D.x,
                    closestPoint2D.y,
                    transform.position.z
                );

                float distance = Vector3.Distance(transform.position, closestPoint3D);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestPoint = closestPoint3D;
                    targetObject = obj;
                    foundTarget = true;
                }
            }
        }

        if (foundTarget)
        {
            snapTarget = bestPoint;
            isSnapping = true;

            if (targetObject.CompareTag("Desk"))
            {
                isOnDesk = true;
                transform.SetParent(null);
            }
            else if (targetObject.CompareTag("Billboard"))
            {
                isOnDesk = false;
                transform.SetParent(targetObject.transform);
            }
        }
    }
}