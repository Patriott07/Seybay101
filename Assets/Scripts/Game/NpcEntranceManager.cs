using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class NpcEntranceManager : MonoBehaviour
{
    [Header("Objek yang Digerakkan")]
    [FormerlySerializedAs("posisiNpc")]
    public Transform npcTransform;
    [FormerlySerializedAs("dokumenKertas")]
    public Transform paperDocument;
    [FormerlySerializedAs("objekTiket")]
    public Transform ticketObject;

    [Header("Pengaturan Animasi")]
    [FormerlySerializedAs("kecepatanMasuk")]
    public float entranceSpeed = 2.5f;
    [FormerlySerializedAs("waktuTungguSodor")]
    public float swayWaitTime = 1f;
    [FormerlySerializedAs("kecepatanSodorKertas")]
    public float paperSlideSpeed = 4f;
    [FormerlySerializedAs("kecepatanPergi")]
    public float departureSpeed = 3f;
    [FormerlySerializedAs("jarakPergi")]
    public float departureDistance = 15f;

    private Vector3 targetDocumentPosition;
    private Vector3 targetTicketPosition;
    private Vector3 originalNpcScale;

    private Vector2 defaultNPCPos;

    void Awake()
    {
        defaultNPCPos = npcTransform.position;    
    }

    void Start()
    {

        targetDocumentPosition = paperDocument.position;
        targetTicketPosition = ticketObject.position;
        originalNpcScale = npcTransform.localScale;

        // 1. NPC mulai dengan ukuran sangat kecil (jauh di belakang)
        Vector3 smallScale = originalNpcScale * 0.1f;
        npcTransform.localScale = smallScale;

        // --- BARIS BARU (SOLUSI) ---
        // 2. MATIKAN (Sembunyikan) kertas dan tiket agar meja benar-benar kosong
        paperDocument.gameObject.SetActive(false);
        ticketObject.gameObject.SetActive(false);

    }

    public void NPCEnter()
    {
        npcTransform.position = defaultNPCPos;
        Vector3 smallScale = originalNpcScale * 0.1f;
        StartCoroutine(AdeganNpcMasuk(smallScale));
        Timer.Singleton.StartTimer();
    }

    IEnumerator AdeganNpcMasuk(Vector3 smallScale)
    {
        // --- FASE A: NPC MUNCUL DARI BELAKANG (ZOOM IN) ---
        float time = 0;
        while (time < 1)
        {
            time += Time.deltaTime * entranceSpeed;
            npcTransform.localScale = Vector3.Lerp(smallScale, originalNpcScale, time);
            yield return null;
        }

        // NPC diam menatap pemain sejenak
        yield return new WaitForSeconds(swayWaitTime);

        // --- FASE B: MENYODORKAN KERTAS KE MEJA ---

        // 1. Posisikan kertas di titik tengah NPC
        paperDocument.position = new Vector3(npcTransform.position.x, npcTransform.position.y, targetDocumentPosition.z);
        ticketObject.position = new Vector3(npcTransform.position.x, npcTransform.position.y, targetTicketPosition.z);

        // --- BARIS BARU (SOLUSI) ---
        // 2. NYALAKAN (Munculkan) kertas dan tiket kembali
        paperDocument.gameObject.SetActive(true);
        ticketObject.gameObject.SetActive(true);

        // 3. Animasi meluncur ke posisi aslinya di meja
        time = 0;
        Vector3 startDocumentPos = paperDocument.position;
        Vector3 startTicketPos = ticketObject.position;

        while (time < 1)
        {
            time += Time.deltaTime * paperSlideSpeed;
            paperDocument.position = Vector3.Lerp(startDocumentPos, targetDocumentPosition, time);
            ticketObject.position = Vector3.Lerp(startTicketPos, targetTicketPosition, time);
            yield return null;
        }
    }

    // --- FASE C: FUNGSI UNTUK MENGUSIR NPC ---
    public void UsirNpc(bool isApprove)
    {
        StartCoroutine(AnimasiNpcPergi(isApprove));
    }

    IEnumerator AnimasiNpcPergi(bool isApprove)
    {
        float time = 0;
        Vector3 initialNpcPosition = npcTransform.position;

        float directionX = isApprove ? departureDistance : -departureDistance;
        Vector3 targetDeparturePosition = initialNpcPosition + new Vector3(directionX, 0, 0);

        while (time < 1)
        {
            time += Time.deltaTime * departureSpeed;
            npcTransform.position = Vector3.Lerp(initialNpcPosition, targetDeparturePosition, time);
            yield return null;
        }

        Debug.Log("NPC sudah pergi dari layar!");
    }
}