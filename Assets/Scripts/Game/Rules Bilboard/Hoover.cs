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
    private bool isHovering = false;
    private bool isScriptActive = true;

    // === [PERBAIKAN 4: GANTI START MENJADI AWAKE] ===
    void Awake()
    {
        skalaAsli = transform.localScale;
        zoomScript = GetComponent<InspectableObject>();

        if (targetSprite == null) targetSprite = GetComponentInChildren<SpriteRenderer>();
        if (targetUIImage == null) targetUIImage = GetComponentInChildren<Image>();

        if (targetSprite != null) warnaAsli = targetSprite.color;
        else if (targetUIImage != null) warnaAsli = targetUIImage.color;
    }
    // ================================================

    void OnEnable() { isScriptActive = true; }

    void OnDisable()
    {
        isScriptActive = false;
        HentikanPaksa();
    }

    void Update()
    {
        if (!isScriptActive) return;

        if (zoomScript != null && zoomScript.GetIsInspect() && isHovering)
        {
            HentikanPaksa();
        }
    }

    void OnMouseEnter()
    {
        if (!isScriptActive) return;
        if (zoomScript != null && zoomScript.GetIsInspect()) return;

        isHovering = true;

        scaleTween?.Kill();
        colorTween?.Kill();

        scaleTween = transform.DOScale(skalaAsli * perbesaranSkala, kecepatanSkala)
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
        isHovering = false;

        scaleTween?.Kill();
        colorTween?.Kill();

        if (skalaAsli != Vector3.zero)
        {
            transform.DOScale(skalaAsli, 0.2f).SetEase(Ease.OutSine);
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