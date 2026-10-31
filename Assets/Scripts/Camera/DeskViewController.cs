using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mengontrol Desk View menggunakan Camera 2 / Overlay Camera.
///
/// Main Camera tetap aktif.
/// Camera 2 ditambahkan ke Camera Stack Main Camera.
///
/// Isi Desk View dapat berupa:
/// - SpriteRenderer
/// - GameObject biasa
/// - Canvas Screen Space - Camera
/// - UI
/// - Object ber-DeskViewTrigger (otomatis dipindah ke layer Desk View + Posisi Desk)
/// </summary>
public class DeskViewController : MonoBehaviour
{
    public static DeskViewController AktifView { get; private set; }
    public static bool IsViewOpen => AktifView != null;

    [Header("Kamera")]
    public Camera mainCamera;
    public Camera deskCamera;

    [Header("Layer Desk View")]
    [Tooltip("Buat layer ini di Project Settings > Tags and Layers. " +
             "Main Camera otomatis mengabaikannya, Camera 2 otomatis merendernya.")]
    public string namaLayerDesk = "DeskView";

    [Header("Isi Desk View")]
    [Tooltip("Canvas Desk View. Boleh kosong.")]
    public GameObject deskCanvas;

    [Tooltip("Object 2D / Sprite / dekorasi Desk View.")]
    public GameObject[] deskWorldRoots;

    [Header("Tombol")]
    public Button tombolKembali;

    [Header("Hari")]
    public int hariSekarang = 1;

    [Header("Input")]
    public KeyCode keyKembali = KeyCode.Escape;

    [Header("Transisi")]
    public float durasiFade = 0.25f;

    /// <summary>Index layer Desk View. -1 jika layer belum dibuat.</summary>
    public int LayerDesk { get; private set; } = -1;

    private DeskItem[] semuaItem = new DeskItem[0];
    private CanvasGroup grup;

    private AudioListener listenerMain;
    private AudioListener listenerDesk;

    private bool sedangTransisi = false;
    private bool sedangMenutup = false; // TAMBAHAN 1

    private readonly List<DeskViewTrigger> triggers =
        new List<DeskViewTrigger>();

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic()
    {
        AktifView = null;
    }

    void Awake()
    {
        if (deskCanvas != null)
        {
            grup = deskCanvas.GetComponent<CanvasGroup>();

            if (grup == null)
                grup = deskCanvas.AddComponent<CanvasGroup>();
        }

        if (mainCamera != null)
            listenerMain = mainCamera.GetComponent<AudioListener>();

        if (deskCamera != null)
            listenerDesk = deskCamera.GetComponent<AudioListener>();

        SiapkanLayerDesk();

        if (tombolKembali != null)
            tombolKembali.onClick.AddListener(Tutup);

        SetButtonTransisi(false);

        KumpulkanItem();

        SetTampilan(false);
    }

    void OnDisable()
    {
        if (AktifView == this)
            AktifView = null;
    }

void Update()
{
    if (AktifView != this)
        return;

    if (sedangTransisi || sedangMenutup)
        return;

    if (BookPanelController.IsPanelOpen)
        return;

    if (Input.GetMouseButtonDown(0))
        return;

    if (Input.GetKeyDown(keyKembali))
        Tutup();
}

    // =========================================================
    // BUTTON
    // =========================================================

    void SetButtonTransisi(bool terkunci)
    {
        if (tombolKembali != null)
            tombolKembali.interactable = !terkunci;
    }

    // =========================================================
    // LAYER
    // =========================================================

    void SiapkanLayerDesk()
    {
        LayerDesk = LayerMask.NameToLayer(namaLayerDesk);

        if (LayerDesk < 0)
        {
            Debug.LogError(
                "[DeskView] Layer '" + namaLayerDesk + "' belum dibuat. " +
                "Buat di Project Settings > Tags and Layers, " +
                "kalau tidak object ber-DeskViewTrigger tidak akan masuk Camera 2.",
                this);

            return;
        }

        int bit = 1 << LayerDesk;

        if (mainCamera != null)
        {
            mainCamera.cullingMask &= ~bit;
            mainCamera.eventMask = mainCamera.cullingMask;
        }

        if (deskCamera != null)
        {
            deskCamera.cullingMask |= bit;
            deskCamera.eventMask = deskCamera.cullingMask;
        }
    }

    // =========================================================
    // API
    // =========================================================

    public void Buka()
    {
        if (sedangTransisi)
            return;

        if (IsViewOpen)
            return;

        StartCoroutine(BukaRoutine());
    }

    public void Tutup()
    {
        if (sedangTransisi || sedangMenutup) // TAMBAHAN 2
            return;

        if (AktifView != this)
            return;

        if (BookPanelController.IsPanelOpen)
            return;

        sedangMenutup = true; // TAMBAHAN 2
        StartCoroutine(TutupRoutine());
    }

    public void SetHari(int hari)
    {
        hariSekarang = hari;

        if (AktifView == this)
            RefreshItem();
    }

    public void DaftarkanTrigger(DeskViewTrigger trigger)
    {
        if (trigger == null)
            return;

        if (!triggers.Contains(trigger))
            triggers.Add(trigger);
    }

    public void HapusTrigger(DeskViewTrigger trigger)
    {
        if (trigger == null)
            return;

        triggers.Remove(trigger);
    }

    // =========================================================
    // OPEN
    // =========================================================

    IEnumerator BukaRoutine()
    {
        sedangTransisi = true;
        SetButtonTransisi(true);

        AktifView = this;

        RefreshItem();

        SetTampilan(true);

        float durasiTrigger = 0f;

        foreach (var trigger in triggers)
        {
            if (trigger != null &&
                trigger.gameObject.activeInHierarchy)
            {
                durasiTrigger = Mathf.Max(
                    durasiTrigger,
                    trigger.PindahKeDesk());
            }
        }

        if (grup != null)
        {
            grup.DOKill();
            grup.alpha = 0f;

            if (durasiFade > 0f)
            {
                yield return grup
                    .DOFade(1f, durasiFade)
                    .SetUpdate(true)
                    .WaitForCompletion();
            }

            grup.alpha = 1f;
        }

        float sisa = durasiTrigger - (grup != null ? durasiFade : 0f);

        if (sisa > 0f)
            yield return new WaitForSecondsRealtime(sisa);

        foreach (var item in semuaItem)
        {
            if (item != null &&
                item.gameObject.activeInHierarchy)
            {
                item.onDeskDibuka?.Invoke();
            }
        }

        sedangTransisi = false;
        SetButtonTransisi(false);
    }

    // =========================================================
    // CLOSE
    // =========================================================

    IEnumerator TutupRoutine()
    {
        sedangTransisi = true;
        SetButtonTransisi(true);

        foreach (var item in semuaItem)
        {
            if (item != null &&
                item.gameObject.activeInHierarchy)
            {
                item.onDeskDitutup?.Invoke();
            }
        }

        float durasiTrigger = 0f;

        foreach (var trigger in triggers)
        {
            if (trigger != null)
            {
                durasiTrigger = Mathf.Max(
                    durasiTrigger,
                    trigger.KembaliKePosisiAsli());
            }
        }

        if (grup != null)
        {
            grup.DOKill();

            if (durasiFade > 0f)
            {
                yield return grup
                    .DOFade(0f, durasiFade)
                    .SetUpdate(true)
                    .WaitForCompletion();
            }
        }

        float sisa = durasiTrigger - (grup != null ? durasiFade : 0f);

        if (sisa > 0f)
            yield return new WaitForSecondsRealtime(sisa);

        SetTampilan(false);

        if (grup != null)
            grup.alpha = 1f;

        AktifView = null;

        // TAMBAHAN 3
        sedangTransisi = false;
        sedangMenutup = false;
        SetButtonTransisi(false);
    }

    // =========================================================
    // CAMERA / VIEW
    // =========================================================

    void SetTampilan(bool desk)
    {
        if (desk)
        {
            PeriksaKamera();

            if (deskCamera != null)
            {
                deskCamera.gameObject.SetActive(true);
                deskCamera.enabled = true;

                if (mainCamera != null && deskCamera.depth <= mainCamera.depth)
                    deskCamera.depth = mainCamera.depth + 1f;
            }

            ArahkanCanvasKeKameraDesk();

            if (deskWorldRoots != null)
            {
                foreach (var root in deskWorldRoots)
                {
                    if (root != null)
                        root.SetActive(true);
                }
            }
        }
        else
        {
            if (deskCamera != null)
                deskCamera.enabled = false;

            if (deskWorldRoots != null)
            {
                foreach (var root in deskWorldRoots)
                {
                    if (root != null)
                        root.SetActive(false);
                }
            }
        }

        if (deskCanvas != null)
            deskCanvas.SetActive(desk);

        if (listenerMain != null)
            listenerMain.enabled = !desk;

        if (listenerDesk != null)
            listenerDesk.enabled = desk;
    }

    // =========================================================
    // CANVAS CAMERA
    // =========================================================

    void ArahkanCanvasKeKameraDesk()
    {
        if (deskCanvas == null)
            return;

        Canvas[] canvases =
            deskCanvas.GetComponentsInChildren<Canvas>(true);

        foreach (Canvas canvas in canvases)
        {
            if (canvas.isRootCanvas &&
                canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                canvas.worldCamera = deskCamera;
            }
        }
    }

    // =========================================================
    // CAMERA CHECK
    // =========================================================

    void PeriksaKamera()
    {
        if (deskCamera == null)
        {
            Debug.LogError(
                "[DeskView] Desk Camera belum diisi!",
                this
            );

            return;
        }

        if (deskCamera.targetTexture != null)
        {
            Debug.LogWarning(
                "[DeskView] Desk Camera menggunakan Target Texture. " +
                "Kosongkan Output Texture agar tampil di Game View.",
                deskCamera
            );
        }

        if (deskCamera.cullingMask == 0)
        {
            Debug.LogWarning(
                "[DeskView] Culling Mask Desk Camera = Nothing.",
                deskCamera
            );
        }
    }

    // =========================================================
    // ITEM
    // =========================================================

    void KumpulkanItem()
    {
        List<DeskItem> list =
            new List<DeskItem>();

        if (deskCanvas != null)
        {
            list.AddRange(
                deskCanvas.GetComponentsInChildren<DeskItem>(true)
            );
        }

        if (deskWorldRoots != null)
        {
            foreach (GameObject root in deskWorldRoots)
            {
                if (root == null)
                    continue;

                list.AddRange(
                    root.GetComponentsInChildren<DeskItem>(true)
                );
            }
        }

        semuaItem = list.ToArray();
    }

    void RefreshItem()
    {
        foreach (DeskItem item in semuaItem)
        {
            if (item == null)
                continue;

            item.gameObject.SetActive(
                item.TersediaDiHari(hariSekarang)
            );
        }
    }
}