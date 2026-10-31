using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum SisiTooltip { Kanan, Kiri, Atas, Bawah }

// Mengatur satu tooltip untuk seluruh game. Taruh di objek di dalam Canvas (atau isi field Canvas).
// Prefab tooltip: Image (latar) + TextMeshPro UGUI (teks). Anak bernama "Icon" (Image) bersifat opsional.
public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("Prefab Tooltip (Image + Teks)")]
    public GameObject prefabTooltip;

    [Header("Referensi")]
    [Tooltip("Canvas tempat tooltip tampil. Kosong = Canvas induk objek ini")]
    public Canvas canvas;
    [Tooltip("Kamera yang merender objek game. Kosong = dicari otomatis (kamera yang melihat objek tersebut)")]
    public Camera kameraDunia;

    [Header("Pengaturan")]
    public float durasiFade = 0.15f;
    [Tooltip("Jarak minimal tooltip dari tepi layar (unit canvas)")]
    public Vector2 marginLayar = new Vector2(10f, 10f);

    [Header("Debug")]
    [Tooltip("Tampilkan di Console saat tooltip ditampilkan atau ditolak, beserta alasannya.")]
    public bool logDebug = true;

    [Header("Canvas Tooltip")]
    [Tooltip("Jika canvas bukan Overlay, tooltip dirender di canvas Overlay khusus supaya selalu di atas semua kamera, termasuk Camera 2 (Overlay Camera).")]
    public bool pakaiCanvasOverlayKhusus = true;
    public int sortingOrderOverlay = 5000;

    private GameObject overlayCanvasGO;

    private RectTransform panel;
    private RectTransform canvasRect;
    private Camera kameraCanvas;
    private CanvasGroup canvasGroup;
    private TextMeshProUGUI teks;
    private Image icon;
    private Tween fadeTween;

    private object pemilik;
    private Collider2D target;
    private SisiTooltip sisi;
    private float jarak;
    private bool pindahOtomatis;
    private bool tampil;
    private bool sudahPeringatKamera;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (canvas == null || prefabTooltip == null)
        {
            Debug.LogError("[TooltipManager] Canvas atau Prefab Tooltip belum diisi.");
            enabled = false;
            return;
        }

        canvas = canvas.rootCanvas;

        // Canvas Screen Space - Camera milik kamera 1 akan tertutup Camera 2 saat Desk View terbuka.
        if (pakaiCanvasOverlayKhusus && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            canvas = BuatCanvasOverlay(canvas);

        canvasRect = (RectTransform)canvas.transform;
        kameraCanvas = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        GameObject go = Instantiate(prefabTooltip, canvasRect);
        go.name = "Tooltip (instance)";
        panel = go.GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = Vector2.zero;
        panel.anchoredPosition = Vector2.zero;

        canvasGroup = go.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = go.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        foreach (Graphic g in go.GetComponentsInChildren<Graphic>(true))
            g.raycastTarget = false;

        teks = go.GetComponentInChildren<TextMeshProUGUI>(true);
        Transform ic = go.transform.Find("Icon");
        if (ic != null) icon = ic.GetComponent<Image>();

        go.SetActive(false);
    }

    // Canvas Overlay khusus tooltip. Pengaturan skala disalin dari canvas asli agar ukuran tooltip sama.
    Canvas BuatCanvasOverlay(Canvas asli)
    {
        overlayCanvasGO = new GameObject("Tooltip Canvas (Overlay)");

        Canvas c = overlayCanvasGO.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = sortingOrderOverlay;

        CanvasScaler scaler = overlayCanvasGO.AddComponent<CanvasScaler>();
        CanvasScaler scalerAsli = asli.GetComponent<CanvasScaler>();

        if (scalerAsli != null)
        {
            scaler.uiScaleMode = scalerAsli.uiScaleMode;
            scaler.referenceResolution = scalerAsli.referenceResolution;
            scaler.screenMatchMode = scalerAsli.screenMatchMode;
            scaler.matchWidthOrHeight = scalerAsli.matchWidthOrHeight;
            scaler.scaleFactor = scalerAsli.scaleFactor;
            scaler.referencePixelsPerUnit = scalerAsli.referencePixelsPerUnit;
        }

        return c;
    }

    void OnDestroy()
    {
        if (overlayCanvasGO != null) Destroy(overlayCanvasGO);
        fadeTween?.Kill();
        if (Instance == this) Instance = null;
    }

    // ==========================================
    // --- API ---
    // ==========================================

    public void Show(object owner, Collider2D objek, string isiTeks, Sprite iconSprite, SisiTooltip sisiPilihan, float jarakDariObjek, bool pindahOtomatisJikaPenuh = false)
    {
        if (panel == null || objek == null)
        {
            Debug.LogWarning("[TooltipManager] Show() ditolak: panel atau objek null. " +
                             "Kemungkinan Awake gagal (Canvas / Prefab Tooltip belum diisi), lihat error merah di awal Play.", this);
            return;
        }

        // Object ber-DeskViewTrigger yang sedang di Desk View dan tooltip-nya dimatikan
        if (DilarangDiDesk(objek))
        {
            if (logDebug) Debug.Log("[TooltipManager] Show() ditolak: " + objek.name + " sedang di Desk View dan tooltip-nya dimatikan.", this);
            return;
        }

        pemilik = owner;
        target = objek;
        sisi = sisiPilihan;
        jarak = jarakDariObjek;
        pindahOtomatis = pindahOtomatisJikaPenuh;
        tampil = true;

        fadeTween?.Kill();
        panel.gameObject.SetActive(true);
        panel.SetAsLastSibling();

        if (teks != null) teks.text = isiTeks;
        if (icon != null)
        {
            icon.sprite = iconSprite;
            icon.gameObject.SetActive(iconSprite != null);
        }

        // Ukuran tooltip mengikuti teks, harus dihitung dulu sebelum posisi
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
        UpdatePosisi();

        canvasGroup.alpha = 0f;
        fadeTween = canvasGroup.DOFade(1f, durasiFade).SetUpdate(true);

        if (logDebug)
            Debug.Log("[TooltipManager] tooltip ditampilkan untuk " + objek.name +
                      " | posisi panel=" + panel.anchoredPosition +
                      " | ukuran panel=" + panel.rect.size +
                      " | mode canvas=" + canvas.renderMode +
                      " | kamera=" + (CariKamera() != null ? CariKamera().name : "TIDAK ADA"), this);
    }

    public void Hide(object owner)
    {
        if (!tampil || pemilik != owner) return;

        tampil = false;
        pemilik = null;
        target = null;

        fadeTween?.Kill();
        fadeTween = canvasGroup.DOFade(0f, durasiFade).SetUpdate(true).OnComplete(() =>
        {
            if (!tampil && panel != null) panel.gameObject.SetActive(false);
        });
    }

    // ==========================================
    // --- DESK VIEW ---
    // ==========================================

    // True jika object ber-DeskViewTrigger sedang di Desk View dan tooltip-nya dimatikan.
    // Object tanpa DeskViewTrigger (mis. Rules & Guide yang permanen di scene Camera 2) tidak terpengaruh.
    static bool DilarangDiDesk(Collider2D c)
    {
        if (c == null) return false;

        DeskViewTrigger trigger = c.GetComponentInParent<DeskViewTrigger>();
        return trigger != null && trigger.SedangDiDesk && !trigger.tooltipAktifDiDeskView;
    }

    void SembunyikanPaksa()
    {
        tampil = false;
        pemilik = null;
        target = null;

        fadeTween?.Kill();
        canvasGroup.alpha = 0f;
        panel.gameObject.SetActive(false);
    }

    // ==========================================
    // --- POSISI ---
    // ==========================================

    void LateUpdate()
    {
        if (!tampil) return;

        if (target == null)
        {
            // Objek sudah hilang
            tampil = false;
            pemilik = null;
            panel.gameObject.SetActive(false);
            return;
        }

        // Object baru masuk Desk View saat tooltip sedang tampil
        if (DilarangDiDesk(target))
        {
            SembunyikanPaksa();
            return;
        }

        UpdatePosisi();
    }

    void UpdatePosisi()
    {
        if (target == null) return;

        // 1. Kotak objek di koordinat canvas
        Rect obj;
        Camera cam = CariKamera();
        if (cam != null)
        {
            Bounds b = target.bounds;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, cam.WorldToScreenPoint(b.min), kameraCanvas, out Vector2 lokalMin);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, cam.WorldToScreenPoint(b.max), kameraCanvas, out Vector2 lokalMax);

            obj = Rect.MinMaxRect(
                Mathf.Min(lokalMin.x, lokalMax.x), Mathf.Min(lokalMin.y, lokalMax.y),
                Mathf.Max(lokalMin.x, lokalMax.x), Mathf.Max(lokalMin.y, lokalMax.y));
        }
        else
        {
            // Tidak ada kamera yang melihat objek -> pakai posisi mouse sebagai titik acuan
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, Input.mousePosition, kameraCanvas, out Vector2 lokalMouse);
            obj = new Rect(lokalMouse.x - 1f, lokalMouse.y - 1f, 2f, 2f);

            if (!sudahPeringatKamera)
            {
                sudahPeringatKamera = true;
                Debug.LogWarning("[TooltipManager] Tidak ada kamera yang melihat objek '" + target.name +
                                 "'. Isi field 'Kamera Dunia' di TooltipManager dengan kamera yang merender objek.");
            }
        }

        // 2. Ukuran tooltip & area layar yang boleh dipakai
        Vector2 ukuran = Vector2.Scale(panel.rect.size, panel.localScale);

        Rect area = canvasRect.rect;
        area.xMin += marginLayar.x;
        area.xMax -= marginLayar.x;
        area.yMin += marginLayar.y;
        area.yMax -= marginLayar.y;

        // 3. Pakai sisi pilihan. Pindah ke sisi seberang HANYA jika opsi otomatis dinyalakan
        Rect pilihan = RectSisi(obj, ukuran, sisi);
        if (pindahOtomatis && !Muat(pilihan, area))
        {
            Rect lawan = RectSisi(obj, ukuran, Lawan(sisi));
            if (Muat(lawan, area)) pilihan = lawan;
        }

        // 4. Pastikan tidak keluar layar
        Vector2 pos = pilihan.position;
        pos.x = Mathf.Clamp(pos.x, area.xMin, Mathf.Max(area.xMin, area.xMax - ukuran.x));
        pos.y = Mathf.Clamp(pos.y, area.yMin, Mathf.Max(area.yMin, area.yMax - ukuran.y));

        // Pivot panel (0,0) dan anchor di tengah canvas
        panel.anchoredPosition = pos - canvasRect.rect.center;
    }

    // Pilih kamera yang benar-benar merender objek (bukan sekadar Camera.main)
    Camera CariKamera()
    {
        int layerBit = 1 << target.gameObject.layer;
        Vector3 pusat = target.bounds.center;

        // Saat Desk View terbuka, objek yang dilihat Camera 2 harus diproyeksikan dengan Camera 2.
        // Ini diutamakan di atas field 'Kamera Dunia' (yang biasanya berisi kamera utama).
        if (DeskViewController.IsViewOpen)
        {
            Camera dc = DeskViewController.AktifView.deskCamera;

            if (dc != null && dc.enabled && (dc.cullingMask & layerBit) != 0)
            {
                Vector3 vpDesk = dc.WorldToViewportPoint(pusat);
                if (vpDesk.z > 0f && vpDesk.x >= 0f && vpDesk.x <= 1f && vpDesk.y >= 0f && vpDesk.y <= 1f)
                    return dc;
            }
        }

        if (kameraDunia != null) return kameraDunia;

        Camera terbaik = null;
        float depthTerbaik = float.MinValue;

        foreach (Camera c in Camera.allCameras)
        {
            if (c.targetTexture != null) continue;              // kamera ke RenderTexture
            if ((c.cullingMask & layerBit) == 0) continue;      // tidak merender layer objek

            Vector3 vp = c.WorldToViewportPoint(pusat);
            if (vp.z <= 0f || vp.x < 0f || vp.x > 1f || vp.y < 0f || vp.y > 1f) continue;

            if (c.depth > depthTerbaik)
            {
                depthTerbaik = c.depth;
                terbaik = c;
            }
        }
        return terbaik;
    }

    Rect RectSisi(Rect obj, Vector2 ukuran, SisiTooltip s)
    {
        float x, y;
        switch (s)
        {
            case SisiTooltip.Kiri:
                x = obj.xMin - jarak - ukuran.x;
                y = obj.center.y - ukuran.y * 0.5f;
                break;
            case SisiTooltip.Atas:
                x = obj.center.x - ukuran.x * 0.5f;
                y = obj.yMax + jarak;
                break;
            case SisiTooltip.Bawah:
                x = obj.center.x - ukuran.x * 0.5f;
                y = obj.yMin - jarak - ukuran.y;
                break;
            default: // Kanan
                x = obj.xMax + jarak;
                y = obj.center.y - ukuran.y * 0.5f;
                break;
        }
        return new Rect(x, y, ukuran.x, ukuran.y);
    }

    static bool Muat(Rect r, Rect area)
    {
        return r.xMin >= area.xMin && r.xMax <= area.xMax
            && r.yMin >= area.yMin && r.yMax <= area.yMax;
    }

    static SisiTooltip Lawan(SisiTooltip s)
    {
        switch (s)
        {
            case SisiTooltip.Kiri: return SisiTooltip.Kanan;
            case SisiTooltip.Atas: return SisiTooltip.Bawah;
            case SisiTooltip.Bawah: return SisiTooltip.Atas;
            default: return SisiTooltip.Kiri;
        }
    }
}