using UnityEngine;

public class Draggable : MonoBehaviour
{
    public Vector3 offset;
    public float snapSpeed = 40f;

    private bool isSnapping = false;
    private Vector3 snapTarget; // Kita simpan titik koordinat spesifik, bukan Transform mejanya

    void OnMouseDown()
    {
        isSnapping = false; 

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - new Vector3(mousePos.x, mousePos.y, transform.position.z);
    }

    void OnMouseDrag()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePos.x, mousePos.y, transform.position.z) + offset;
    }

    void OnMouseUp()
    {
        Vector2 mousePos2D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Desk"))
        {
            // Jika didrop langsung di area meja, objek diam secara bebas di sana
            isSnapping = false;
            return; 
        }
        else
        {
            // Jika didrop di luar meja, cari titik area meja yang paling dekat
            FindClosestDeskPoint();
        }
    }

    void Update()
    {
        if (isSnapping)
        {
            // Meluncur ke titik target (Z tetap dikunci)
            transform.position = Vector3.MoveTowards(transform.position, snapTarget, snapSpeed * Time.deltaTime);

            // Jika jaraknya sudah sangat dekat dengan target, berhenti meluncur
            if (Vector3.Distance(transform.position, snapTarget) < 0.01f)
            {
                isSnapping = false;
            }
        }
    }

    void FindClosestDeskPoint()
    {
        GameObject[] allDesks = GameObject.FindGameObjectsWithTag("Desk");
        
        float closestDistance = Mathf.Infinity;
        Vector3 bestPoint = transform.position;
        bool foundDesk = false;

        foreach (GameObject desk in allDesks)
        {
            Collider2D deskCollider = desk.GetComponent<Collider2D>();
            
            if (deskCollider != null)
            {
                // Inilah kuncinya: Mencari koordinat terdekat di area meja, bukan posisi tengah meja
                Vector2 closestPoint2D = deskCollider.ClosestPoint(transform.position);
                
                // Konversi ke Vector3 dengan mengunci sumbu Z objek
                Vector3 closestPoint3D = new Vector3(closestPoint2D.x, closestPoint2D.y, transform.position.z);
                
                float distance = Vector3.Distance(transform.position, closestPoint3D);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestPoint = closestPoint3D;
                    foundDesk = true;
                }
            }
        }

        if (foundDesk)
        {
            snapTarget = bestPoint;
            isSnapping = true; 
        }
    }
}