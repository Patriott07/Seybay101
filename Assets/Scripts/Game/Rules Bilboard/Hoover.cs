using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Collider2D))]
public class HoverEfek : MonoBehaviour
{
    [Header("Pengaturan Warna (Gradient)")]
    public Gradient warnaHover;
    public float kecepatanWarna = 0.8f;

    [Header("Target Mewarnai")]
    [Tooltip("Tarik objek 'bg' (latar paspor/tiket) ke salah satu kolom ini!")]
    public SpriteRenderer targetSprite;
    public Image targetUIImage;

    [Header("Pengaturan Skala (Naik Turun)")]
    public float perbesaranSkala = 1.05f;
    public float kecepatanSkala = 0.5f;

    private Vector3 skalaAsli;
    private Color warnaAsli;

    private Tween scaleTween;
    private Tween colorTween;

    private InspectableObject zoomScript;
    private DeskViewTrigger deskTrigger;
    private bool isHovering = false;
    private bool isScriptActive = true;

    // Ukuran "normal" saat ini: mengikuti Desk View jika object ber-DeskViewTrigger
    // (skala kamera 1 saat di kamera 1, skala desk saat di Camera 2).
    private Vector3 SkalaNormal => deskTrigger != null ? deskTrigger.SkalaDasar : skalaAsli;

    // Hover dilarang jika object ber-DeskViewTrigger sedang di Desk View dan opsi hover-nya dimatikan
    private bool HoverDilarang =>
        deskTrigger != null && deskTrigger.SedangDiDesk && !deskTrigger.hoverAktifDiDeskView;

    // True selama object sedang dianimasikan masuk/keluar Desk View
    private bool SedangPindahDesk => deskTrigger != null && deskTrigger.SedangAnimasi;

    void Awake()
    {
        skalaAsli = transform.localScale;
        zoomScript = GetComponent<InspectableObject>();
        deskTrigger = GetComponent<DeskViewTrigger>();

        if (targetSprite == null) targetSprite = GetComponentInChildren<SpriteRenderer>();
        if (targetUIImage == null) targetUIImage = GetComponentInChildren<Image>();

        if (targetSprite != null) warnaAsli = targetSprite.color;
        else if (targetUIImage != null) warnaAsli = targetUIImage.color;
    }

    void OnEnable() { isScriptActive = true; }

    void OnDisable()
    {
        isScriptActive = false;
        HentikanPaksa();
    }

    void Update()
    {
        if (!isScriptActive) return;
        if (!isHovering) return;

        // Hover dihentikan jika dokumen sedang dibaca, atau object sedang animasi pindah Desk View
        if ((zoomScript != null && zoomScript.GetIsInspect()) || SedangPindahDesk || HoverDilarang)
        {
            HentikanPaksa();
        }
    }

    void OnMouseEnter()
    {
        if (!isScriptActive) return;
        if (zoomScript != null && zoomScript.GetIsInspect()) return;
        if (SedangPindahDesk) return;
        if (HoverDilarang) return;

        isHovering = true;

        scaleTween?.Kill();
        colorTween?.Kill();

        scaleTween = transform.DOScale(SkalaNormal * perbesaranSkala, kecepatanSkala)
                              .SetLoops(-1, LoopType.Yoyo)
                              .SetEase(Ease.InOutSine);

        if (targetSprite != null)
        {
            colorTween = DOVirtual.Float(0f, 1f, kecepatanWarna, (nilai) =>
            {
                if (targetSprite != null) targetSprite.color = warnaHover.Evaluate(nilai);
            }).SetLoops(-1, LoopType.Yoyo);
        }
        else if (targetUIImage != null)
        {
            colorTween = DOVirtual.Float(0f, 1f, kecepatanWarna, (nilai) =>
            {
                if (targetUIImage != null) targetUIImage.color = warnaHover.Evaluate(nilai);
            }).SetLoops(-1, LoopType.Yoyo);
        }
    }

    void OnMouseExit()
    {
        if (!isScriptActive || !isHovering) return;
        if (zoomScript != null && zoomScript.GetIsInspect()) return;

        KembalikanKeAsli();
    }

    private void KembalikanKeAsli()
    {
        // Jika object sedang dianimasikan oleh Desk View, jangan ikut menganimasikan scale
        if (SedangPindahDesk)
        {
            HentikanPaksa();
            return;
        }

        isHovering = false;

        scaleTween?.Kill();
        colorTween?.Kill();

        Vector3 target = SkalaNormal;

        if (target != Vector3.zero)
        {
            transform.DOScale(target, 0.2f).SetEase(Ease.OutSine);
        }

        if (targetSprite != null) targetSprite.DOColor(warnaAsli, 0.2f);
        else if (targetUIImage != null) targetUIImage.DOColor(warnaAsli, 0.2f);
    }

    private void HentikanPaksa()
    {
        isHovering = false;

        scaleTween?.Kill();
        colorTween?.Kill();

        if (targetSprite != null) targetSprite.color = warnaAsli;
        else if (targetUIImage != null) targetUIImage.color = warnaAsli;
    }
}