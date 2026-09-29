using System.Collections;
using DG.Tweening;
using Schema.data;
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
    public Transform locationStamp;

    private Vector3 posisiAwal;
    public Camera cam;
    private bool sedangDiproses = false;

    private SpriteRenderer spriteRenderer;
    private int layerAwal;

    private bool isMoving;
    private float elapsedTime = 0f;
    private Transform startPos;

    [SerializeField]
    Transform stampleDecisionHolder;
    Transform targetPos;
    [SerializeField] Transform targetOne, targetTwo;

    [Header("Pengaturan Notify Mistake")]
    public float jarakGerakX = 5f;
    public float durasiGerak = 0.5f;
    public float waktuBerhenti = 2f;

    [SerializeField]
    Transform notifyMistakeOBJ;

    private Vector2 notifyMistakeOBJStartPos; // Diubah jadi Vector2 untuk UI

    void Start()
    {
        if (notifyMistakeOBJ != null)
        {
            // Ambil posisi Anchor UI
            RectTransform rect = notifyMistakeOBJ.GetComponent<RectTransform>();
            if (rect != null)
            {
                notifyMistakeOBJStartPos = rect.anchoredPosition;
            }
        }

        // cam = Camera.main;
        posisiAwal = transform.position;

        spriteRenderer = GetComponent<SpriteRenderer>();
        layerAwal = spriteRenderer.sortingOrder;
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.W))
        {
            notifyMistake();
            Debug.Log("mistake notify");
            
        }
        // Moving stample holder to right(visible) if the stample is dragged, move toward the otherwise position if not dragged;
        // #800020
        if (isMoving)
        {
            elapsedTime += Time.deltaTime;
            float percentageComplete = elapsedTime / 0.3f;

            stampleDecisionHolder.position = Vector2.Lerp(
                startPos.position,
                targetPos.position,
                percentageComplete
            );

            if (percentageComplete >= 1f)
            {
                isMoving = false;
            }
        }
        // #800020
        // Moving stample holder to right(visible) if the stample is dragged, move toward the otherwise position if not dragged;
    }

    void OnEnable()
    {
        GameEvent.DeleteMarkTicket += DeleteMarkTicket; // to delete approve/disapprove mark from a used ticket
    }

    void OnDisable()
    {
        GameEvent.DeleteMarkTicket -= DeleteMarkTicket;
    }

    // Tambahkan OnMouseDown untuk trigger klik pertama kali
    void OnMouseDown()
    {
        if (sedangDiproses)
            return;

        targetPos = targetOne;
        startPos = targetTwo;
        elapsedTime = 0f; // Reset waktu agar animasi jalan perlahan dari awal
        isMoving = true;
    }

    // Bersihkan OnMouseDrag, cukup untuk menggeser objek
    void OnMouseDrag()
    {
        // Debug.Log("DOWN");

        if (sedangDiproses)
            return;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        transform.position = mousePos;
    }

    void OnMouseUp()
    {
        targetPos = targetTwo;
        startPos = targetOne;
        // target pos set ke luar layar supaya tidak visible
        elapsedTime = 0f; // Reset waktu lagi untuk animasi pulangnya
        isMoving = true;

        if (sedangDiproses)
            return;

        if (!GameManager.Instance.isNpcInFront)
        {
            transform.position = posisiAwal;
            return;
        }

        Collider2D[] hits = Physics2D.OverlapPointAll(transform.position);
        bool dilepasDiZona = false;

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("ZoneYes"))
            {
                StartCoroutine(SiklusAnimasiKeputusan(true, hit.transform.position));
                dilepasDiZona = true;
                Manager_Chat.Instance.BersihkanChat();
                // lets check here
                CheckRule(true);

                break;
            }
            else if (hit.CompareTag("ZoneNo"))
            {
                StartCoroutine(SiklusAnimasiKeputusan(false, hit.transform.position));
                dilepasDiZona = true;
                Manager_Chat.Instance.BersihkanChat();
                // lets check here
                CheckRule(false);
                break;
            }
        }

        if (!dilepasDiZona)
        {
            transform.position = posisiAwal;
        }
    }


    void notifyMistake()
    {
        if (notifyMistakeOBJ != null)
        {
            RectTransform rect = notifyMistakeOBJ.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Hentikan animasi sebelumnya jika sedang berjalan agar tidak tumpang tindih
                rect.DOKill();
                // Pastikan mulai dari posisi awal
                rect.anchoredPosition = notifyMistakeOBJStartPos;

                Sequence seq = DOTween.Sequence();
                
                // Gerak ke kanan khusus UI (DOAnchorPosX)
                seq.Append(rect.DOAnchorPosX(notifyMistakeOBJStartPos.x + jarakGerakX, durasiGerak));
                
                // Berhenti
                seq.AppendInterval(waktuBerhenti);
                
                // Balik ke kiri (posisi awal)
                seq.Append(rect.DOAnchorPosX(notifyMistakeOBJStartPos.x, durasiGerak));
            }
        }
    }



    IEnumerator SiklusAnimasiKeputusan(bool isApprove, Vector3 posisiZona)
    {
        AudioManager.Instance.PlaySfxMenuToggle();
        sedangDiproses = true;

        // cek apakah di scene ada ticket
        // GameEvent.OnPassportModeCall?.Invoke();

        transform.position = posisiZona;
        scriptTiket.PaksaZoomIn();

        yield return new WaitForSeconds(0.5f);

        // 1. Gerakan awal dari tombol kembali ke posisiAwal (Lerp manual Anda)
        float time = 0;
        Vector3 titikDiTombol = transform.position;
        while (time < 1)
        {
            time += Time.deltaTime * kecepatanAnimasi;
            transform.position = Vector3.Lerp(titikDiTombol, posisiAwal, time);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        // Ubah layer agar berada di atas kertas
        spriteRenderer.sortingOrder = 60;

        // Tentukan titik target stempel di atas kertas
        Vector3 targetTerbang = locationStamp.position + new Vector3(0f, 0f, -1f);
        Vector3 posisiAncangAncang = targetTerbang + new Vector3(0f, 1.5f, 0f);

        // Pindah ke posisi ancang-ancang di atas kertas
        transform.position = posisiAncangAncang;
        yield return new WaitForSeconds(0.1f);

        // 2. GERAKAN "SMASH" (Membanting ke bawah dengan DOTween)
        yield return transform
            .DOMove(targetTerbang, 0.15f)
            .SetEase(Ease.InQuad)
            .WaitForCompletion();

        // --- 💥 EFEK IMPACT / HANTAMAN ---
        // Efek penyok/membal kecil saat stempel menghantam kertas
        transform.DOPunchScale(transform.localScale + new Vector3(0.3f, -0.3f, 0f), 0.15f, 10, 1);

        // Munculkan Tinta Stempel
        GameObject prefabTinta = isApprove ? approveMarkPrefab : rejectMarkPrefab;
        GameObject cetakan = Instantiate(
            prefabTinta,
            locationStamp.position,
            Quaternion.identity,
            locationStamp
        );

        cetakan.transform.localPosition = new Vector3(0f, 0f, -0.1f);
        cetakan.GetComponent<SpriteRenderer>().sortingOrder = 51;

        // Efek pop tinta muncul basah
        // cetakan.transform.localScale = Vector3.zero;
        // cetakan.transform.DOScale(1f, 0.1f).SetEase(Ease.OutBack);

        // Tahan sebentar di bawah
        yield return new WaitForSeconds(0.4f);

        // 3. Tarik balik stempel ke posisi semula
        yield return transform.DOMove(posisiAwal, 0.25f).SetEase(Ease.OutQuad).WaitForCompletion();

        // Kembalikan layer sorting
        spriteRenderer.sortingOrder = layerAwal;

        yield return new WaitForSeconds(0.3f);
        scriptTiket.PaksaZoomOut();
        yield return new WaitForSeconds(0.5f);

        if (isApprove)
            AudioManager.Instance.PlaySfxApprove();
        else
            AudioManager.Instance.PlaySfxReject();

        if (npcManager != null)
        {
            npcManager.UsirNpc(isApprove);
        }
        else
        {
            Debug.LogWarning(
                "Kamu belum memasukkan Game Manager ke kolom Npc Manager di Inspector Stempel!"
            );
        }

        sedangDiproses = false;
    }

    void DeleteMarkTicket()
    {
        Destroy(locationStamp.GetChild(0).gameObject);
    }

    void CheckRule(bool isApprove)
    {
        int currentDay = GameManager.Instance.GetCurrentDay();
        Debug.Log($"CurrentDay {currentDay}");
        GameManager.Instance.chanceTOGetSomeActiveViolation = Mathf.Clamp(
            GameManager.Instance.chanceTOGetSomeActiveViolation + Random.Range(0.02f, 0.03f),
            0f,
            0.4f
        );

        int _mistakeCount = 0;

        switch (currentDay)
        {
            case 1:
                /// <summary>
                /// 1. Ticket must same with passport data (name, id)
                /// 2. Makesure passport is not a expired
                /// 3. Makesure people bring ticket date thats normal
                /// its 2045th
                /// </summary>
                /// <summary>
                ///  Lets prepare some data compare
                /// </summary>
                PassportSchema passport = PassportScript.Instance.currentData;
                if (passport == null)
                    return;
                BoardingPassSchema ticket = TicketScript.Instance.currentData;
                if (ticket == null)
                    return;

                bool _ticketIsMatchPassport = true;
                bool _ticketIsNotExpired = true;
                bool _validTicketDate = true;

                // rule 1
                if (
                    passport.ownerName != ticket.passengerName
                    || passport.documentNumber != ticket.idPassengerCard
                )
                {
                    _mistakeCount++;
                    _ticketIsMatchPassport = false;
                }

                // rule 2
                if (passport.expiryDate.Year < 2045)
                {
                    _mistakeCount++;
                    _ticketIsNotExpired = false;
                }

                // rule 3
                if (ticket.departureDate.Year != 2045)
                {
                    _mistakeCount++;
                    _validTicketDate = false;
                }
                break;

            case 2:
                /// <summary>
                /// Day 2: Check passport + koper items
                /// </summary>
                PassportSchema passport2 = PassportScript.Instance.currentData;
                BoardingPassSchema ticket2 = TicketScript.Instance.currentData;
                if (passport2 != null && ticket2 != null)
                {
                    if (passport2.ownerName != ticket2.passengerName
                        || passport2.documentNumber != ticket2.idPassengerCard)
                        _mistakeCount++;
                    if (passport2.expiryDate.Year < 2045)
                        _mistakeCount++;
                    if (ticket2.departureDate.Year != 2045)
                        _mistakeCount++;
                }

                // Check koper: barang terlarang yang masih belum disita
                if (LuggageManager.Instance != null && LuggageManager.Instance.isSystemActive)
                {
                    int illegalItemsInKoper = LuggageManager.Instance.JumlahBarangTerlarangBelumDisita();
                    _mistakeCount += illegalItemsInKoper;
                }
                break;
            default:
                break;
        }

        int penalty = _mistakeCount * 5;
        Debug.Log($"Penalty : {penalty}");
        Debug.Log($"Mistake : {_mistakeCount}");

        if (isApprove)
        {
            // cek stamp sebagai benar
            if (_mistakeCount == 0)
            {
                EconomyManager.Instance.TambahUang();
                EconomyManager.Instance.TambahTrust(5);
            }
            else
            {
            if (Random.Range(1, 3) == 3)
            {
                // ShowMistake() a function that notify player if he make a mistake!!!
            }
                notifyMistake();
                EconomyManager.Instance.KurangiUang(3 * _mistakeCount);
                EconomyManager.Instance.KurangiTrust(penalty * _mistakeCount);
                Debug.Log("=====  mistake =====");
            }
        }
        else
        {
            // cek stamp sebagai salah
            if (_mistakeCount > 0)
            {
                EconomyManager.Instance.TambahUang();
                EconomyManager.Instance.TambahTrust(5);
            }
            else
            {
                Debug.Log("=====  mistake =====");
                EconomyManager.Instance.KurangiUang(5);
                EconomyManager.Instance.KurangiTrust(5);
                notifyMistake();
            }
        }
    }
}