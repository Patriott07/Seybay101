using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Schema.data;
using UnityEngine;
using UnityEngine.UI;

public class StampHybridController : MonoBehaviour
{
    [Header("Hubungan Objek")]
    public InspectableObject scriptTiket;
    public NpcEntranceManager npcManager;

    [Header("Dua Stempel")]
    public StampItem stampApprove;
    public StampItem stampReject;
    public KeyCode tombolApprove = KeyCode.A;
    public KeyCode tombolReject = KeyCode.D;

    [Header("Pilih Stempel")]
    [Tooltip("Seberapa naik stempel yang sedang terpilih dari posisi rak.")]
    public float angkatSaatTerpilih = 0.4f;
    public float durasiGantiStempel = 0.15f;
    [Tooltip("Pengali warna untuk stempel yang tidak terpilih (lebih gelap).")]
    public Color warnaTidakTerpilih = new Color(0.55f, 0.55f, 0.55f, 1f);
    [Tooltip("Sorting order stempel saat di-drag / menghantam, agar di atas kertas.")]
    public int sortingOrderSaatDiangkat = 60;

    [Header("Efek Tinta")]
    public GameObject approveMarkPrefab;
    public GameObject rejectMarkPrefab;
    public float kecepatanAnimasi = 8f;
    public Transform locationStamp;
    [Tooltip("Stempel dianggap kena kertas jika dilepas dalam radius ini dari locationStamp.")]
    public float radiusTerimaStempel = 1.5f;
    public Camera cam;

    [Header("Tutup Camera 2 (Desk View)")]
    [Tooltip("Tutup Desk View otomatis setelah stempel selesai.")]
    public bool tutupDeskOtomatis = true;
    [Tooltip("Jeda setelah stempel kembali ke rak, agar tinta sempat terlihat, sebelum Camera 2 ditutup.")]
    public float jedaSebelumTutupDesk = 0.4f;
    [Tooltip("Jeda setelah Camera 2 tertutup sebelum UsirNpc dipanggil. UsirNpc sudah punya jeda tiket sendiri.")]
    public float jedaSebelumNpcPergi = 0.1f;
    [Tooltip("Batas waktu menunggu Camera 2 tertutup. Jika lewat, NPC tetap diproses.")]
    public float batasTungguDeskView = 5f;
    [Tooltip("Batas waktu menunggu tiket sampai ke NPC sebelum stempel boleh dipakai lagi.")]
    public float batasTungguNpcPergi = 5f;

    [Header("Pengaman Back")]
    public Button tombolBack;

    [Header("Pengaturan Notify Mistake")]
    public float jarakGerakX = 5f;
    public float durasiGerak = 0.5f;
    public float waktuBerhenti = 2f;
    [Range(0f, 1f)] public float peluangNotifMistake = 0.33f;

    [SerializeField]
    Transform notifyMistakeOBJ;

    Vector2 notifyMistakeOBJStartPos;
    StampItem terpilih;
    bool sedangDiproses = false;

    public bool SedangDiproses => sedangDiproses;
    public StampItem Terpilih => terpilih;

    void SetBack(bool aktif)
    {
        if (tombolBack != null)
            tombolBack.interactable = aktif;
    }

    void Start()
    {
        if (cam == null) cam = Camera.main;

        if (notifyMistakeOBJ != null)
        {
            RectTransform rect = notifyMistakeOBJ.GetComponent<RectTransform>();
            if (rect != null)
                notifyMistakeOBJStartPos = rect.anchoredPosition;
        }

        if (stampApprove == null || stampReject == null)
        {
            Debug.LogError("Stamp Approve / Stamp Reject belum diisi di StampHybridController!", this);
            enabled = false;
            return;
        }

        stampApprove.Init(this, true);
        stampReject.Init(this, false);
        Pilih(stampApprove, true);
    }

    void Update()
    {
        if (sedangDiproses) return;
        if (terpilih != null && terpilih.SedangDrag) return;

        if (Input.GetKeyDown(tombolApprove)) Pilih(stampApprove);
        else if (Input.GetKeyDown(tombolReject)) Pilih(stampReject);
    }

    void OnEnable()
    {
        GameEvent.DeleteMarkTicket += DeleteMarkTicket;
    }

    void OnDisable()
    {
        GameEvent.DeleteMarkTicket -= DeleteMarkTicket;
    }

    void OnDrawGizmosSelected()
    {
        if (locationStamp == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(locationStamp.position, radiusTerimaStempel);
    }

    // ================== PILIH STEMPEL ==================

    public void Pilih(StampItem item, bool instan = false)
    {
        if (item == null) return;
        if (item == terpilih && !instan) return;

        terpilih = item;
        stampApprove.SetTerpilih(item == stampApprove, instan);
        stampReject.SetTerpilih(item == stampReject, instan);
    }

    // ================== DIPANGGIL OLEH StampItem SAAT DILEPAS ==================

    public void LepasStempel(StampItem item)
    {
        if (sedangDiproses) return;

        // DokumenSiapDiklik false = NPC belum selesai masuk atau sudah diusir (sedang pergi),
        // jadi tidak boleh dinilai lagi.
        bool npcSiap = npcManager == null || npcManager.DokumenSiapDiklik;

        if (!GameManager.Instance.isNpcInFront || !npcSiap || !DiAtasKertas(item))
        {
            item.KembaliKeRak();
            return;
        }

        bool isApprove = item.IsApprove;
        StartCoroutine(SiklusAnimasiKeputusan(item, isApprove));
        Manager_Chat.Instance.ClearHistory();
        CheckRule(isApprove);
    }

    bool DiAtasKertas(StampItem item)
    {
        if (locationStamp == null) return false;
        return Vector2.Distance(item.transform.position, locationStamp.position) <= radiusTerimaStempel;
    }

    // ================== NOTIFY MISTAKE ==================

    void notifyMistake()
    {
        if (notifyMistakeOBJ != null)
        {
            RectTransform rect = notifyMistakeOBJ.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.DOKill();
                rect.anchoredPosition = notifyMistakeOBJStartPos;

                Sequence seq = DOTween.Sequence();
                seq.Append(rect.DOAnchorPosX(notifyMistakeOBJStartPos.x + jarakGerakX, durasiGerak));
                seq.AppendInterval(waktuBerhenti);
                seq.Append(rect.DOAnchorPosX(notifyMistakeOBJStartPos.x, durasiGerak));
            }
        }
    }

    // ================== ANIMASI STEMPEL ==================

    IEnumerator SiklusAnimasiKeputusan(StampItem stamp, bool isApprove)
    {
        AudioManager.Instance.PlaySfxMenuToggle();
        sedangDiproses = true;
        SetBack(false);

        SpriteRenderer sr = stamp.Renderer;
        stamp.transform.DOKill();

        sr.sortingOrder = 60;

        Vector3 targetTerbang = locationStamp.position + new Vector3(0f, 0f, -1f);
        Vector3 posisiAncangAncang = targetTerbang + new Vector3(0f, 1.5f, 0f);

        yield return stamp.transform
            .DOMove(posisiAncangAncang, 1f / kecepatanAnimasi)
            .SetEase(Ease.OutQuad)
            .WaitForCompletion();

        yield return new WaitForSeconds(0.1f);

        yield return stamp.transform
            .DOMove(targetTerbang, 0.15f)
            .SetEase(Ease.InQuad)
            .WaitForCompletion();

        stamp.transform.DOPunchScale(
            stamp.transform.localScale + new Vector3(0.3f, -0.3f, 0f), 0.15f, 10, 1);

        GameObject prefabTinta = isApprove ? approveMarkPrefab : rejectMarkPrefab;

        if (prefabTinta != null)
        {
            GameObject cetakan = Instantiate(
                prefabTinta,
                locationStamp.position,
                Quaternion.identity,
                locationStamp
            );

            cetakan.transform.localPosition = new Vector3(0f, 0f, -0.1f);
            cetakan.GetComponent<SpriteRenderer>().sortingOrder = 51;
        }
        else
        {
            Debug.LogWarning("Prefab tinta (Approve/Reject Mark) belum diisi!");
        }

        yield return new WaitForSeconds(0.4f);

        yield return stamp.transform
            .DOMove(stamp.PosisiIstirahat, 0.25f)
            .SetEase(Ease.OutQuad)
            .WaitForCompletion();

        sr.sortingOrder = stamp.LayerAwal;

        // ===== TUTUP CAMERA 2 OTOMATIS =====

        if (tutupDeskOtomatis)
        {
            yield return new WaitForSeconds(jedaSebelumTutupDesk);
            yield return TutupDeskViewDanTunggu();
            yield return new WaitForSeconds(jedaSebelumNpcPergi);
        }
        else
        {
            yield return new WaitForSeconds(0.8f);
        }

        // ===== NPC DIPROSES =====

        if (isApprove)
            AudioManager.Instance.PlaySfxApprove();
        else
            AudioManager.Instance.PlaySfxReject();

        if (npcManager != null)
        {
            npcManager.UsirNpc(isApprove);

            float batas = Time.unscaledTime + batasTungguNpcPergi;

            while (GameManager.Instance.isNpcInFront && Time.unscaledTime < batas)
                yield return null;
        }
        else
        {
            Debug.LogWarning(
                "Kamu belum memasukkan Game Manager ke kolom Npc Manager di Inspector Stempel!"
            );
        }

        sedangDiproses = false;
        SetBack(true);
    }

    IEnumerator TutupDeskViewDanTunggu()
    {
        if (!DeskViewController.IsViewOpen)
            yield break;

        float batas = Time.unscaledTime + batasTungguDeskView;

        // Tutup hanya sekali agar tidak spam selama transisi.
        DeskViewController.AktifView.Tutup();

        while (DeskViewController.IsViewOpen && Time.unscaledTime < batas)
            yield return null;

        if (DeskViewController.IsViewOpen)
            Debug.LogWarning(
                "[Stamp] Desk View belum tertutup setelah batas waktu, NPC tetap diproses."
            );
    }

    void DeleteMarkTicket()
    {
        if (locationStamp != null && locationStamp.childCount > 0)
            Destroy(locationStamp.GetChild(0).gameObject);
    }

    // ================== CHECK RULE ==================

    // Cek paspor vs tiket. Mengembalikan jumlah kesalahan dan mengisi daftar alasan.
    int CekDokumen(PassportSchema passport, BoardingPassSchema ticket, List<string> alasan)
    {
        int salah = 0;

        // 1. Nama / nomor dokumen paspor harus sama dengan tiket
        if (passport.ownerName != ticket.passengerName
            || passport.documentNumber != ticket.idPassengerCard)
        {
            salah++;
            alasan.Add("Nama/ID paspor tidak cocok dengan tiket");
        }

        // 2. Paspor tidak boleh kedaluwarsa (tahun game 2045)
        if (passport.expiryDate.Year < 2045)
        {
            salah++;
            alasan.Add("Paspor kedaluwarsa");
        }

        // 3. Tanggal keberangkatan tiket harus di tahun 2045
        if (ticket.departureDate.Year != 2045)
        {
            salah++;
            alasan.Add("Tanggal tiket tidak valid");
        }

        return salah;
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
        List<string> alasan = new List<string>();

        switch (currentDay)
        {
            case 1:
                PassportSchema passport = PassportScript.Instance.currentData;
                if (passport == null)
                    return;
                BoardingPassSchema ticket = TicketScript.Instance.currentData;
                if (ticket == null)
                    return;

                _mistakeCount += CekDokumen(passport, ticket, alasan);
                break;

            case 2:
                PassportSchema passport2 = PassportScript.Instance.currentData;
                BoardingPassSchema ticket2 = TicketScript.Instance.currentData;

                if (passport2 != null && ticket2 != null)
                    _mistakeCount += CekDokumen(passport2, ticket2, alasan);

                if (LuggageManager.Instance != null && LuggageManager.Instance.isSystemActive)
                {
                    int illegalItemsInKoper =
                        LuggageManager.Instance.JumlahBarangTerlarangBelumDisita();

                    if (illegalItemsInKoper > 0)
                    {
                        _mistakeCount += illegalItemsInKoper;
                        alasan.Add("Barang terlarang lolos: " + illegalItemsInKoper);
                    }
                }
                break;

            default:
                break;
        }

        int penalty = _mistakeCount * 5;

        // Trust yang hilang saat approve salah = penalty x jumlah kesalahan (5 x n^2):
        // 1 salah = 5, 2 salah = 20, 3 salah = 45. Ubah ke `penalty` saja jika ingin linear (5 x n).
        int trustHilang = penalty * _mistakeCount;

        string detailSalah = alasan.Count > 0
            ? string.Join("\n", alasan.Select(a => "- " + a))
            : null;

        Debug.Log($"Mistake : {_mistakeCount} | Penalty : {penalty} | Trust hilang : {trustHilang}"
                  + (detailSalah != null ? "\n" + detailSalah : ""));

        if (isApprove)
        {
            if (_mistakeCount == 0)
            {
                EconomyManager.Instance.TambahUang();
                EconomyManager.Instance.TambahTrust(5);
            }
            else
            {
                if (Random.value < peluangNotifMistake)
                    notifyMistake();

                EconomyManager.Instance.KurangiUang(3 * _mistakeCount);
                EconomyManager.Instance.KurangiTrust(trustHilang);
                PhoneManager.Instance.TambahViolation("wrong_accept", detailSalah, trustHilang);
            }
        }
        else
        {
            if (_mistakeCount > 0)
            {
                EconomyManager.Instance.TambahUang();
                EconomyManager.Instance.TambahTrust(5);
            }
            else
            {
                EconomyManager.Instance.KurangiUang(5);
                EconomyManager.Instance.KurangiTrust(5);
                PhoneManager.Instance.TambahViolation(
                    "wrong_reject", "- Dokumen valid tetapi ditolak", 5);
            }
        }
    }
}