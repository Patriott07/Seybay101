using DG.Tweening;
using UnityEngine;

/// <summary>
/// Dipasang di objek stempel (Approve / Reject). Butuh SpriteRenderer + Collider2D.
/// Semua logika keputusan ada di StampHybridController.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class StampItem : MonoBehaviour
{
    StampHybridController manager;
    SpriteRenderer sr;
    Vector3 posisiRak;
    Color warnaAwal;
    int layerAwal;
    bool terpilih;
    bool sedangDrag;

    public bool IsApprove { get; private set; }
    public bool SedangDrag => sedangDrag;
    public SpriteRenderer Renderer => sr;
    public int LayerAwal => layerAwal;

    // Posisi diam: terangkat sedikit kalau terpilih, di rak kalau tidak
    public Vector3 PosisiIstirahat =>
        terpilih ? posisiRak + Vector3.up * manager.angkatSaatTerpilih : posisiRak;

    bool BisaDipakai => manager != null && terpilih && !manager.SedangDiproses;

    public void Init(StampHybridController m, bool isApprove)
    {
        manager = m;
        IsApprove = isApprove;
        sr = GetComponent<SpriteRenderer>();
        layerAwal = sr.sortingOrder;
        warnaAwal = sr.color;
        posisiRak = transform.position;
    }

    public void SetTerpilih(bool value, bool instan = false)
    {
        terpilih = value;
        sedangDrag = false;

        transform.DOKill();
        sr.DOKill();
        sr.sortingOrder = layerAwal;

        Vector3 posTarget = PosisiIstirahat;
        Color warnaTarget = value ? warnaAwal : warnaAwal * manager.warnaTidakTerpilih;

        if (instan)
        {
            transform.position = posTarget;
            sr.color = warnaTarget;
            return;
        }

        transform.DOMove(posTarget, manager.durasiGantiStempel).SetEase(Ease.OutQuad);
        DOTween.To(() => sr.color, c => sr.color = c, warnaTarget, manager.durasiGantiStempel)
               .SetTarget(sr);
    }

    void OnMouseDown()
    {
        if (!BisaDipakai) return;
        sedangDrag = true;
        transform.DOKill();
        sr.sortingOrder = manager.sortingOrderSaatDiangkat;
    }

    void OnMouseDrag()
    {
        if (!sedangDrag) return;

        Vector3 mousePos = manager.cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        transform.position = mousePos;
    }

    void OnMouseUp()
    {
        if (!sedangDrag) return;
        sedangDrag = false;
        manager.LepasStempel(this);
    }

    public void KembaliKeRak()
    {
        sedangDrag = false;
        transform.DOKill();
        transform.DOMove(PosisiIstirahat, 0.2f)
                 .SetEase(Ease.OutQuad)
                 .OnComplete(() => sr.sortingOrder = layerAwal);
    }
}