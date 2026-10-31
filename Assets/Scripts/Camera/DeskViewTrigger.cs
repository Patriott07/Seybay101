using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(Collider2D))]
public class DeskViewTrigger : MonoBehaviour
{
    [Header("Desk View")]
    public DeskViewController view;
    public NpcEntranceManager npcManager;
    public Transform posisiDesk;
    public bool pindahkanKeLayerDesk = true;

    [Header("Scale Desk")]
    public float scaleDesk = 1.5f;
    public bool gunakanScalePosisiDesk = false;

    [Header("Hover & Tooltip saat di Desk View (Camera 2)")]
    public bool hoverAktifDiDeskView = false;
    public bool tooltipAktifDiDeskView = false;

    [Header("Animasi")]
    public bool gunakanAnimasi = true;
    public float durasiPindah = 0.35f;
    public Ease easePindah = Ease.OutCubic;

    [Header("Debug")]
    public bool logDebug = true;

    private Vector3 posisiAsli;
    private Quaternion rotasiAsli;
    private Vector3 skalaAsli;
    private Vector3 skalaDeskTarget;
    private bool skalaTersimpan = false;

    public bool SedangAnimasi =>
        tweenAktif != null &&
        tweenAktif.IsActive() &&
        tweenAktif.IsPlaying();

    public bool SedangDiDesk => sedangDiDesk;

    public Vector3 SkalaDasar =>
        sedangDiDesk ? skalaDeskTarget : skalaAsli;

    private Transform[] semuaTransform;
    private int[] layerAsli;

    private bool sedangDiDesk = false;
    private Sequence tweenAktif;

    void Awake()
    {
        SimpanTransformAsli();
    }

    void OnEnable()
    {
        if (view != null)
            view.DaftarkanTrigger(this);
    }

    void OnDisable()
    {
        if (view != null)
            view.HapusTrigger(this);

        if (sedangDiDesk)
        {
            MatikanTween();
            TerapkanTransform(posisiAsli, rotasiAsli, skalaAsli);
            PulihkanLayer();
            sedangDiDesk = false;
        }
    }

    void OnMouseDown()
    {
        if (view == null)
            return;

        // PENGAMAN UTAMA:
        // Jangan buka Desk View selama NPC belum selesai
        // masuk dan menyodorkan Passport/Ticket.
        if (npcManager != null &&
            !npcManager.DokumenSiapDiklik)
            return;

        if (DeskViewController.IsViewOpen)
            return;

        if (SedangAnimasi || SedangDiDesk)
            return;

        if (BookPanelController.IsPanelOpen)
            return;

        if (PointerDiAtasUIInteraktif())
            return;

        view.Buka();
    }

    public void SimpanTransformAsli()
    {
        posisiAsli = transform.position;
        rotasiAsli = transform.rotation;

        if (!skalaTersimpan)
        {
            skalaAsli = transform.localScale;
            skalaTersimpan = true;
        }

        semuaTransform =
            GetComponentsInChildren<Transform>(true);

        layerAsli =
            new int[semuaTransform.Length];

        for (int i = 0; i < semuaTransform.Length; i++)
            layerAsli[i] =
                semuaTransform[i].gameObject.layer;
    }

    public float PindahKeDesk()
    {
        if (posisiDesk == null)
        {
            Debug.LogWarning(
                "[DeskView] " +
                gameObject.name +
                " belum memiliki Posisi Desk.",
                this);

            return 0f;
        }

        MatikanTween();

        if (!sedangDiDesk)
            SimpanTransformAsli();

        sedangDiDesk = true;

        if (pindahkanKeLayerDesk)
            TerapkanLayerDesk();

        Vector3 targetScale;

        if (gunakanScalePosisiDesk)
        {
            Vector3 parentScale =
                transform.parent != null
                    ? transform.parent.lossyScale
                    : Vector3.one;

            Vector3 w =
                posisiDesk.lossyScale;

            targetScale = new Vector3(
                parentScale.x != 0f
                    ? w.x / parentScale.x
                    : w.x,

                parentScale.y != 0f
                    ? w.y / parentScale.y
                    : w.y,

                parentScale.z != 0f
                    ? w.z / parentScale.z
                    : w.z
            );
        }
        else
        {
            targetScale =
                skalaAsli * scaleDesk;
        }

        skalaDeskTarget = targetScale;

        if (logDebug)
        {
            Debug.Log(
                "[DeskView] " +
                gameObject.name +
                " | mode scale: " +
                (gunakanScalePosisiDesk
                    ? "ikut Posisi Desk (scaleDesk DIABAIKAN)"
                    : "skalaAsli x scaleDesk") +
                " | skalaAsli=" +
                skalaAsli +
                " | scaleDesk=" +
                scaleDesk +
                " | target=" +
                targetScale,
                this
            );
        }

        if (!gunakanAnimasi ||
            durasiPindah <= 0f)
        {
            TerapkanTransform(
                posisiDesk.position,
                posisiDesk.rotation,
                targetScale
            );

            return 0f;
        }

        tweenAktif =
            BuatTween(
                posisiDesk.position,
                posisiDesk.rotation,
                targetScale
            );

        return durasiPindah;
    }

    public float KembaliKePosisiAsli()
    {
        if (!sedangDiDesk)
            return 0f;

        MatikanTween();

        if (!gunakanAnimasi ||
            durasiPindah <= 0f)
        {
            TerapkanTransform(
                posisiAsli,
                rotasiAsli,
                skalaAsli
            );

            PulihkanLayer();
            sedangDiDesk = false;

            return 0f;
        }

        tweenAktif =
            BuatTween(
                posisiAsli,
                rotasiAsli,
                skalaAsli
            );

        tweenAktif.OnComplete(() =>
        {
            PulihkanLayer();
            sedangDiDesk = false;
            tweenAktif = null;
        });

        return durasiPindah;
    }

    static readonly System.Collections.Generic.List<RaycastResult>
        hasilRaycastUI =
        new System.Collections.Generic.List<RaycastResult>();

    static bool PointerDiAtasUIInteraktif()
    {
        if (EventSystem.current == null)
            return false;

        var data =
            new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

        hasilRaycastUI.Clear();

        EventSystem.current.RaycastAll(
            data,
            hasilRaycastUI
        );

        foreach (var r in hasilRaycastUI)
        {
            if (r.gameObject != null &&
                ExecuteEvents.GetEventHandler<IPointerClickHandler>(
                    r.gameObject
                ) != null)
            {
                return true;
            }
        }

        return false;
    }

    Sequence BuatTween(
        Vector3 pos,
        Quaternion rot,
        Vector3 scale)
    {
        Sequence seq =
            DOTween.Sequence().SetUpdate(true);

        seq.Join(
            transform
                .DOMove(pos, durasiPindah)
                .SetEase(easePindah)
        );

        seq.Join(
            transform
                .DORotateQuaternion(
                    rot,
                    durasiPindah
                )
                .SetEase(easePindah)
        );

        seq.Join(
            transform
                .DOScale(
                    scale,
                    durasiPindah
                )
                .SetEase(easePindah)
        );

        return seq;
    }

    void TerapkanTransform(
        Vector3 pos,
        Quaternion rot,
        Vector3 scale)
    {
        transform.position = pos;
        transform.rotation = rot;
        transform.localScale = scale;
    }

    void MatikanTween()
    {
        if (tweenAktif != null &&
            tweenAktif.IsActive())
        {
            tweenAktif.Kill();
        }

        tweenAktif = null;
        transform.DOKill();
    }

    void TerapkanLayerDesk()
    {
        if (view == null ||
            view.LayerDesk < 0)
            return;

        foreach (Transform t in semuaTransform)
        {
            if (t != null)
                t.gameObject.layer =
                    view.LayerDesk;
        }
    }

    void PulihkanLayer()
    {
        if (semuaTransform == null)
            return;

        for (int i = 0;
             i < semuaTransform.Length;
             i++)
        {
            if (semuaTransform[i] != null)
            {
                semuaTransform[i].gameObject.layer =
                    layerAsli[i];
            }
        }
    }
}