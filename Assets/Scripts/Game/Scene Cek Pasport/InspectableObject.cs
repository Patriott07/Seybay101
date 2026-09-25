using System.Collections;
using DG.Tweening;
using UnityEngine;

public class InspectableObject : MonoBehaviour
{
    [Header("Pengaturan Inspeksi")]
    public Transform titikInspeksi;
    public float skalaZoom = 2f;
    public float kecepatanAnimasi = 8f;
    public SpriteRenderer bg;

    private static InspectableObject currentInspectedObject;
    private Vector3 posisiMeja;
    private Vector3 skalaAwal;
    private int urutanLayerAwal;

    private bool isInspected = false;
    private bool sedangAnimasi = false;
    private SpriteRenderer spriteRenderer;

    private float clickThreshold = 0.3f;
    private float lastClickTime = 0f;
    private int clickCount = 0;

    Draggable draggableScript;

    void Awake()
    {
        skalaAwal = transform.localScale;
    }

    void Start()
    {
        draggableScript = GetComponent<Draggable>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        urutanLayerAwal = spriteRenderer.sortingOrder;
    }

    public bool GetIsInspect() => isInspected;

    void OnMouseDown()
    {
        if (sedangAnimasi)
            return;

        if (draggableScript != null && draggableScript.isOnDesk == false)
            return;

        float timeSinceLastClick = Time.time - lastClickTime;

        if (timeSinceLastClick <= clickThreshold)
        {
            clickCount++;
        }
        else
        {
            clickCount = 1;
        }
        lastClickTime = Time.time;

        if (clickCount < 2)
            return;

        // === DOUBLE CLICK DETECTED ===
        clickCount = 0;

        if (!isInspected)
        {
            if (currentInspectedObject != null && currentInspectedObject != this)
            {
                currentInspectedObject.PaksaZoomOut();
            }

            currentInspectedObject = this;
            posisiMeja = transform.position;
            sedangAnimasi = true;
            StartCoroutine(AnimasiGerak(titikInspeksi.position, skalaAwal * skalaZoom, 50, true));
            AudioManager.Instance.PlaySfxTicket();
        }
        else
        {
            StartCoroutine(AnimasiGerak(posisiMeja, skalaAwal, urutanLayerAwal, false));
            AudioManager.Instance.PlaySfxTicket();
        }
    }

    IEnumerator AnimasiGerak(Vector3 targetPos, Vector3 targetScale, int targetLayer, bool openBG)
    {
        sedangAnimasi = true;

        if (!isInspected)
            spriteRenderer.sortingOrder = targetLayer;

        isInspected = !isInspected;

        float time = 0;
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;

        bg.DOKill();
        if (openBG)
            bg.DOFade(0.92f, 0.5f).SetDelay(0.3f);
        else
            bg.DOFade(0, 0.15f);

        while (time < 1)
        {
            time += Time.deltaTime * kecepatanAnimasi;
            transform.position = Vector3.Lerp(startPos, targetPos, time);
            transform.localScale = Vector3.Lerp(startScale, targetScale, time);
            yield return null;
        }

        if (!isInspected)
            spriteRenderer.sortingOrder = targetLayer;

        sedangAnimasi = false;
    }

    public void PaksaZoomIn()
    {
        if (!isInspected && !sedangAnimasi)
        {
            currentInspectedObject = this;
            posisiMeja = transform.position;
            sedangAnimasi = true;
            StartCoroutine(AnimasiGerak(titikInspeksi.position, skalaAwal * skalaZoom, 50, true));
        }
    }

    public void PaksaZoomOut()
    {
        if (isInspected && !sedangAnimasi)
        {
            StartCoroutine(AnimasiGerak(posisiMeja, skalaAwal, urutanLayerAwal, false));
            if (currentInspectedObject == this) currentInspectedObject = null;
        }
    }
}
