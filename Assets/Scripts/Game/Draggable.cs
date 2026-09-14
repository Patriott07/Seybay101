using UnityEngine;

public class Draggable : MonoBehaviour
{
    public Vector3 offset;
    public float snapSpeed = 40f;
    public bool isOnDesk = false;
    private bool isSnapping = false;
    private Vector3 snapTarget; // Kita simpan titik koordinat spesifik, bukan Transform mejanya
    public Camera cameraTarget;
    private Vector3 skalaAwal;

    InspectableObject inspectableObjectScript;

    void Start()
    {
        inspectableObjectScript = gameObject.GetComponent<InspectableObject>();
        cameraTarget = GameManager.Instance.mainCam;
        skalaAwal = transform.localScale;
    }

    void OnMouseDown()
    {
        isSnapping = false;

        if (inspectableObjectScript.GetIsInspect())
            return;

        // transform.SetParent(null);

        // transform.localScale = skalaAwal;

        Vector3 mousePos = cameraTarget.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - new Vector3(mousePos.x, mousePos.y, transform.position.z);
    }

    void OnMouseDrag()
    {
        if (inspectableObjectScript.GetIsInspect())
            return;
        // Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // transform.position = new Vector3(mousePos.x, mousePos.y, transform.position.z) + offset;

        // 1. Ambil posisi mouse saat ini di dunia
        Vector3 mousePos = cameraTarget.ScreenToWorldPoint(Input.mousePosition);
        Vector3 targetPosition = new Vector3(mousePos.x, mousePos.y, transform.position.z) + offset;

        // 2. Batasi (Clamp) agar posisinya tidak keluar dari area kamera
        // Menggunakan ViewportToWorldPoint dengan nilai 0 (kiri/bawah) sampai 1 (kanan/atas)
        Vector3 minBounds = cameraTarget.ViewportToWorldPoint(
            new Vector3(0, 0, Camera.main.nearClipPlane)
        );
        Vector3 maxBounds = cameraTarget.ViewportToWorldPoint(
            new Vector3(1, 1, Camera.main.nearClipPlane)
        );

        // Opsional: Jika ingin memperhitungkan ukuran objek (biar sprite tidak setengah keluar layar)
        // Anda bisa menambahkan sedikit padding atau batas ukuran sprite di sini.

        targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);

        // 3. Terapkan posisi yang sudah dibatasi ke objek
        transform.position = targetPosition;
    }

    void OnMouseUp()
    {
        isSnapping = false;
        if (inspectableObjectScript.GetIsInspect())
            return;

        FindClosestDeskPoint();
        // Vector2 mousePos2D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

        // if (hit.collider != null)
        // {
        //     // PERBAIKAN: Pisahkan pengecekan tag Desk dan Billboard
        //     if (hit.collider.CompareTag("Desk"))
        //     {
        //         isOnDesk = true;
        //         isSnapping = false;
        //         return;
        //     }
        //     else if (hit.collider.CompareTag("Billboard"))
        //     {
        //         isOnDesk = false;
        //         isSnapping = false;
        //         return;
        //     }
        // }
        // else
        // {
        //     // Jika didrop di luar meja, cari titik area meja yang paling dekat
        //     FindClosestDeskPoint();
        // }
    }

    void Update()
    {
        if (isSnapping)
        {
            // Meluncur ke titik target (Z tetap dikunci)
            transform.position = Vector3.MoveTowards(
                transform.position,
                snapTarget,
                snapSpeed * Time.deltaTime
            );

            // Jika jaraknya sudah sangat dekat dengan target, berhenti meluncur
            if (Vector3.Distance(transform.position, snapTarget) < 0.01f)
            {
                isSnapping = false;
            }
        }
    }

    void FindClosestDeskPoint()
    {
        // Ambil semua objek yang bertag "Desk" dan "Billboard"
        GameObject[] allDesks = GameObject.FindGameObjectsWithTag("Desk");
        GameObject[] allBillboards = GameObject.FindGameObjectsWithTag("Billboard");

        System.Collections.Generic.List<GameObject> allTargets =
            new System.Collections.Generic.List<GameObject>();
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

            // Tentukan status isOnDesk berdasarkan tag objek terdekat yang dituju
            if (targetObject.CompareTag("Desk"))
            {
                isOnDesk = true;
                transform.SetParent(null); // Lepas dari Billboard jika ada di meja
            }
            else if (targetObject.CompareTag("Billboard"))
            {
                isOnDesk = false;
                // transform.localScale = skalaAwal;
                transform.SetParent(targetObject.transform);
            }
        }
    }
}
