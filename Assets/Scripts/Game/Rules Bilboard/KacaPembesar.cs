using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Kaca pembesar khusus Camera 2 (Desk View).
/// Lensa bulat yang bisa digeser, membesarkan isi Camera 2 (Image / teks UI) tepat di bawahnya.
/// Pasang di UI object di dalam Canvas Screen Space - Overlay (dibuat otomatis oleh tool editor).
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(1000)]
[RequireComponent(typeof(RectTransform))]
public class KacaPembesar : MonoBehaviour,
    IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("Kamera Desk (Camera 2)")]
    [Tooltip("Camera 2 (Desk Camera). Lensa hanya tampil saat kamera ini aktif. Harus Orthographic.")]
    public Camera kameraDesk;
    [Tooltip("Batasi layer yang dibesarkan. Default = semua layer yang dirender Camera 2. " +
             "Contoh: pilih hanya layer UI agar yang dibesarkan hanya Image / teks UI.")]
    public LayerMask layerDibesarkan = ~0;
    [Tooltip("Warna latar isi lensa. Alpha 0 = transparan (hanya konten Camera 2 yang membesar). " +
             "Jika tepi teks terlihat gelap, ubah ke warna meja yang solid (alpha 1).")]
    public Color warnaLatar = new Color(0f, 0f, 0f, 0f);

    [Header("Lensa")]
    [Tooltip("Diameter kaca (satuan UI). Otomatis diterapkan ke RectTransform.")]
    public float diameterLensa = 300f;
    [Range(1f, 2f)]
    [Tooltip("Kualitas render isi lensa. 1 = normal, 2 = lebih tajam tapi lebih berat.")]
    public float kualitasRender = 1f;
    [Tooltip("ON: tetap di tempat saat dilepas (seperti ditaruh di meja). OFF: kembali ke posisi awal.")]
    public bool tetapDiTempatSaatDilepas = true;

    [Header("Pembesaran (scroll mouse di atas lensa)")]
    public float zoomAwal = 2f;
    public float zoomMin = 1.5f;
    public float zoomMax = 6f;
    [Tooltip("Perubahan zoom per 1 notch scroll.")]
    public float langkahZoom = 0.5f;
    [Tooltip("Kehalusan perubahan zoom (makin besar makin cepat).")]
    public float kecepatanZoom = 12f;

    [Header("Tampilan (opsional: ganti dengan sprite sendiri)")]
    public Sprite spriteBingkai;
    public Sprite spriteGagang;
    public Color warnaBingkai = new Color(0.62f, 0.47f, 0.2f, 1f);
    public Color warnaGagang = new Color(0.22f, 0.13f, 0.07f, 1f);

    [Header("Referensi (diisi otomatis oleh tool, jangan dihapus)")]
    [SerializeField] RawImage tampilan;

    // ====== API ======
    public float Zoom
    {
        get => zoomTarget;
        set => zoomTarget = Mathf.Clamp(value, zoomMin, zoomMax);
    }
    public bool SedangDipegang => sedangDrag;
    public void KembaliKePosisiAwal() => rect.anchoredPosition = posisiAwal;

    // ====== Internal ======
    RectTransform rect;
    RectTransform parentRect;
    Canvas canvasRoot;
    CanvasGroup grup;
    Vector2 posisiAwal;

    Camera helper;
    RenderTexture hasilRT;

    readonly List<Object> asetBuatan = new List<Object>();
    readonly Vector3[] sudut = new Vector3[4];

    float zoomTarget, zoomSekarang;
    bool sedangDrag, hover;
    Vector3 offsetGrab;
    int statusTampil = -1;
    bool sudahPeringatPerspektif;

    // ================== SIKLUS HIDUP ==================

    void Awake()
    {
        rect = (RectTransform)transform;
        parentRect = rect.parent as RectTransform;

        Canvas c = GetComponentInParent<Canvas>();
        if (c == null)
        {
            Debug.LogError("[KacaPembesar] Harus berada di dalam Canvas.", this);
            enabled = false;
            return;
        }
        canvasRoot = c.rootCanvas;

        if (canvasRoot.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Debug.LogWarning(
                "[KacaPembesar] Letakkan lensa di Canvas 'Screen Space - Overlay'. Canvas jenis lain bisa " +
                "ikut terekam kamera lensa sehingga isi lensa menampilkan dirinya sendiri.", this);
        }

        if (kameraDesk == null)
            Debug.LogWarning("[KacaPembesar] Kamera Desk (Camera 2) belum diisi.", this);

        // Anchor & ukuran lensa (posisi di layar dipertahankan)
        Vector3 posDunia = rect.position;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(diameterLensa, diameterLensa);
        rect.position = posDunia;
        posisiAwal = rect.anchoredPosition;

        grup = GetComponent<CanvasGroup>();
        if (grup == null) grup = gameObject.AddComponent<CanvasGroup>();

        var goKamera = new GameObject("[KacaPembesar] Kamera");
        helper = goKamera.AddComponent<Camera>();
        helper.enabled = false;

        // Cadangan: jika tampilan belum dibangun oleh tool editor, bangun saat runtime
        if (tampilan == null)
        {
            BangunTampilan(
                BuatSpriteBulat(256, 0f),
                spriteBingkai != null ? null : BuatSpriteBulat(256, 14f));
        }

        zoomTarget = zoomSekarang = Mathf.Clamp(zoomAwal, zoomMin, zoomMax);
    }

    void OnDisable()
    {
        sedangDrag = false;
        hover = false;
        statusTampil = -1;

        if (helper != null) helper.enabled = false;
    }

    void OnDestroy()
    {
        if (helper != null) Destroy(helper.gameObject);

        if (hasilRT != null)
        {
            hasilRT.Release();
            Destroy(hasilRT);
        }

        foreach (var o in asetBuatan)
            if (o != null) Destroy(o);
    }

    // ================== RENDER LENSA ==================

    void SiapkanRT(int px)
    {
        px = Mathf.Clamp(px, 64, 2048);
        if (hasilRT != null && Mathf.Abs(hasilRT.width - px) <= 2) return;

        if (hasilRT != null)
        {
            helper.targetTexture = null;
            hasilRT.Release();
            Destroy(hasilRT);
        }

        hasilRT = new RenderTexture(px, px, 24, RenderTextureFormat.ARGB32)
        {
            name = "KacaPembesar_RT",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            antiAliasing = 1
        };
        hasilRT.Create();
        tampilan.texture = hasilRT;
    }

    Camera KameraCanvas()
    {
        if (canvasRoot == null) return null;
        return canvasRoot.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvasRoot.worldCamera;
    }

    // Tampil hanya saat Camera 2 aktif
    void AturTampil(bool tampil)
    {
        int status = tampil ? 1 : 0;
        if (status == statusTampil) return;
        statusTampil = status;

        grup.alpha = tampil ? 1f : 0f;
        grup.blocksRaycasts = tampil;
        grup.interactable = tampil;

        if (!tampil)
        {
            sedangDrag = false;
            hover = false;
            helper.enabled = false;
        }
    }

    void LateUpdate()
    {
        bool aktif = kameraDesk != null && kameraDesk.isActiveAndEnabled;

        if (aktif && !kameraDesk.orthographic)
        {
            if (!sudahPeringatPerspektif)
            {
                Debug.LogWarning("[KacaPembesar] Kamera Desk harus Orthographic.", kameraDesk);
                sudahPeringatPerspektif = true;
            }
            aktif = false;
        }

        AturTampil(aktif);
        if (!aktif) return;

        // --- Zoom: scroll mouse saat kursor di atas lensa ---
#if ENABLE_LEGACY_INPUT_MANAGER
        if (hover || sedangDrag)
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
                zoomTarget = Mathf.Clamp(zoomTarget + scroll * langkahZoom, zoomMin, zoomMax);
        }
#endif
        zoomSekarang = Mathf.Lerp(
            zoomSekarang, zoomTarget,
            1f - Mathf.Exp(-kecepatanZoom * Time.unscaledDeltaTime));

        // --- Posisi & ukuran lensa di layar (pixel) ---
        rect.GetWorldCorners(sudut);
        Camera camUI = KameraCanvas();
        Vector2 a = RectTransformUtility.WorldToScreenPoint(camUI, sudut[0]);
        Vector2 b = RectTransformUtility.WorldToScreenPoint(camUI, sudut[2]);
        Vector2 pusat = (a + b) * 0.5f;
        float diameterPx = Mathf.Abs(b.x - a.x);
        if (diameterPx < 8f) return;

        SiapkanRT(Mathf.RoundToInt(diameterPx * kualitasRender));

        // --- Render ulang area di bawah lensa dengan pembesaran ---
        helper.CopyFrom(kameraDesk);
        helper.ResetAspect();
        helper.targetTexture = hasilRT;
        helper.rect = new Rect(0f, 0f, 1f, 1f);
        helper.cullingMask = kameraDesk.cullingMask & layerDibesarkan.value;
        helper.eventMask = 0;       // jangan ikut memproses klik OnMouse...
        helper.depth = -100f;       // render sebelum kamera layar
        helper.allowMSAA = false;
        helper.allowHDR = false;
        helper.useOcclusionCulling = false;
        helper.clearFlags = CameraClearFlags.SolidColor;
        helper.backgroundColor = warnaLatar;

        // Pusatkan kamera di titik dunia yang tepat di bawah tengah lensa
        Vector3 titikPusat = kameraDesk.ScreenToWorldPoint(new Vector3(pusat.x, pusat.y, 0f));
        helper.transform.SetPositionAndRotation(titikPusat, kameraDesk.transform.rotation);

        // Area dunia yang tercakup lensa dibagi zoom = pembesaran
        float duniaPerPiksel = 2f * kameraDesk.orthographicSize / Mathf.Max(1f, kameraDesk.pixelHeight);
        helper.orthographicSize = diameterPx * duniaPerPiksel * 0.5f / zoomSekarang;

        helper.enabled = true;
        tampilan.enabled = true;
    }

    // ================== DRAG (angkat & geser lensa) ==================

    public void OnPointerDown(PointerEventData e)
    {
        e.useDragThreshold = false; // langsung ikut kursor, tanpa jeda geser awal
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (parentRect == null) return;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                parentRect, e.position, e.pressEventCamera, out Vector3 w))
        {
            offsetGrab = rect.position - w;
            sedangDrag = true;
            transform.SetAsLastSibling(); // lensa terangkat di atas UI lain
        }
    }

    public void OnDrag(PointerEventData e)
    {
        if (!sedangDrag || parentRect == null) return;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                parentRect, e.position, e.pressEventCamera, out Vector3 w))
        {
            rect.position = w + offsetGrab;
            TahanDiLayar();
        }
    }

    public void OnEndDrag(PointerEventData e)
    {
        sedangDrag = false;
        if (!tetapDiTempatSaatDilepas)
            rect.anchoredPosition = posisiAwal;
    }

    public void OnPointerEnter(PointerEventData e) { hover = true; }
    public void OnPointerExit(PointerEventData e) { hover = false; }

    // Titik tengah lensa tidak boleh keluar dari layar
    void TahanDiLayar()
    {
        parentRect.GetWorldCorners(sudut);
        Vector3 p = rect.position;
        p.x = Mathf.Clamp(p.x, sudut[0].x, sudut[2].x);
        p.y = Mathf.Clamp(p.y, sudut[0].y, sudut[2].y);
        rect.position = p;
    }

    // ================== TAMPILAN ==================

    /// <summary>
    /// Membangun hierarki lensa: Gagang, Kaca (mask bulat) + Isi (hasil kamera), Bingkai.
    /// Dipanggil oleh tool editor (spriteKaca/spriteCincin berupa asset) atau saat runtime sebagai cadangan.
    /// </summary>
    public void BangunTampilan(Sprite spriteKaca, Sprite spriteCincin)
    {
        // 1) Gagang (paling belakang). Ukuran relatif terhadap diameter lensa.
        RectTransform gagang = BuatUI("Gagang", transform);
        gagang.pivot = new Vector2(0.5f, 1f);
        gagang.anchorMin = new Vector2(0.7603f, -0.3753f);
        gagang.anchorMax = new Vector2(0.8903f, 0.1747f);
        gagang.offsetMin = Vector2.zero;
        gagang.offsetMax = Vector2.zero;
        gagang.localRotation = Quaternion.Euler(0f, 0f, 45f);
        Image imgGagang = gagang.gameObject.AddComponent<Image>();
        imgGagang.sprite = spriteGagang;
        imgGagang.color = spriteGagang != null ? Color.white : warnaGagang;
        imgGagang.raycastTarget = true;

        // 2) Kaca: bentuk bulat sebagai mask
        RectTransform kaca = BuatUI("Kaca", transform);
        Penuh(kaca);
        Image imgKaca = kaca.gameObject.AddComponent<Image>();
        imgKaca.sprite = spriteKaca;
        imgKaca.alphaHitTestMinimumThreshold = 0.1f; // area klik berbentuk bulat
        kaca.gameObject.AddComponent<Mask>().showMaskGraphic = false;

        // 3) Isi lensa: hasil render kamera
        RectTransform isi = BuatUI("Isi", kaca);
        Penuh(isi);
        tampilan = isi.gameObject.AddComponent<RawImage>();
        tampilan.raycastTarget = false;
        tampilan.enabled = false; // aktif setelah ada hasil render

        // 4) Bingkai (paling depan)
        RectTransform bingkai = BuatUI("Bingkai", transform);
        Penuh(bingkai);
        Image imgBingkai = bingkai.gameObject.AddComponent<Image>();
        imgBingkai.sprite = spriteBingkai != null ? spriteBingkai : spriteCincin;
        imgBingkai.color = spriteBingkai != null ? Color.white : warnaBingkai;
        imgBingkai.raycastTarget = false;
    }

    static RectTransform BuatUI(string nama, Transform parent)
    {
        var go = new GameObject(nama, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        var r = (RectTransform)go.transform;
        r.SetParent(parent, false);
        return r;
    }

    static void Penuh(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }

    /// <summary>Tekstur lingkaran putih (tebalCincin = 0) atau cincin (tebalCincin > 0, satuan pixel).</summary>
    public static Texture2D BuatTeksturBulat(int ukuran, float tebalCincin)
    {
        var tex = new Texture2D(ukuran, ukuran, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        var px = new Color32[ukuran * ukuran];
        float r = ukuran * 0.5f;

        for (int y = 0; y < ukuran; y++)
        {
            for (int x = 0; x < ukuran; x++)
            {
                float dx = x + 0.5f - r;
                float dy = y + 0.5f - r;
                float jarak = Mathf.Sqrt(dx * dx + dy * dy);

                float alpha = Mathf.Clamp01(r - jarak); // tepi luar halus
                if (tebalCincin > 0f)
                    alpha = Mathf.Min(alpha, Mathf.Clamp01(jarak - (r - tebalCincin))); // lubang tengah

                px[y * ukuran + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
            }
        }

        tex.SetPixels32(px);
        tex.Apply();
        return tex;
    }

    // Versi runtime (cadangan): hasilnya dibersihkan di OnDestroy
    Sprite BuatSpriteBulat(int ukuran, float tebalCincin)
    {
        Texture2D tex = BuatTeksturBulat(ukuran, tebalCincin);
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, ukuran, ukuran), new Vector2(0.5f, 0.5f), 100f);
        asetBuatan.Add(tex);
        asetBuatan.Add(sprite);
        return sprite;
    }
}