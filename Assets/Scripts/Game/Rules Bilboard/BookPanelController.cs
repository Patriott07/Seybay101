using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Satu bagian = satu dokumen (misal: Rules, Guide/Passport).
/// Ini HANYA kelas data untuk Inspector, BUKAN component. Jangan di-Add Component ke GameObject manapun.
/// Halaman = anak-anak langsung dari bukuKiri / bukuKanan (Isi Hal 1, Isi Hal 2, dst).
/// </summary>
[System.Serializable]
public class BagianBuku
{
    public string nama = "Rules";

    [Tooltip("Objek induk buku bagian ini (mis. Book_Rules). Dipakai untuk animasi buka/tutup dan di-aktif/nonaktifkan saat pindah bagian.")]
    public GameObject rootBuku;
    public RectTransform bukuKiri;
    public RectTransform bukuKanan;

    [Tooltip("OPSIONAL. Tab di navbar untuk menuju bagian ini. Kosongkan jika bagian ini tidak punya tab.")]
    public Button tombolTab;
}

/// <summary>
/// Boleh ada lebih dari satu BookPanelController di scene (mis. satu set Rules & Guide untuk Camera 1,
/// satu set lagi untuk Camera 2). Hanya satu panel yang bisa terbuka dalam satu waktu,
/// dan keyboard / input hanya diproses oleh panel yang sedang terbuka.
///
/// PENTING: pasang script ini di objek yang SELALU aktif, yaitu BUKAN di dalam panelDoc
/// dan BUKAN di dalam salah satu rootBuku (objek-objek itu dinonaktifkan oleh script ini).
/// </summary>
public class BookPanelController : MonoBehaviour
{
    // Semua controller yang sedang terbuka. Dicek oleh script lain lewat IsPanelOpen.
    static readonly HashSet<BookPanelController> terbukaSet = new HashSet<BookPanelController>();

    /// <summary>True jika ada panel buku (yang manapun) yang sedang terbuka.</summary>
    public static bool IsPanelOpen => terbukaSet.Count > 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic()
    {
        terbukaSet.Clear();
    }

    [Header("Panel")]
    public GameObject panelDoc;                 // Panel_Doc
    [Tooltip("Opsional. Jika kosong dibuat otomatis di Panel_Doc.")]
    public CanvasGroup panelGroup;

    [Header("Bagian / Dokumen")]
    public BagianBuku[] bagian;                 // 0 = Rules, 1 = Guide, dst

    [Header("Navbar (OPSIONAL)")]
    public bool tampilkanNavbar = true;
    public GameObject navbarRoot;
    public float geserTabAktif = 25f;
    public float durasiGeserTab = 0.2f;

    [Header("Tombol")]
    public Button tombolA;
    public Button tombolD;
    public Button tombolKembali;

    [Header("Keyboard (hanya berlaku saat panel INI terbuka)")]
    public KeyCode[] keySebelumnya = { KeyCode.A, KeyCode.LeftArrow };
    public KeyCode[] keySelanjutnya = { KeyCode.D, KeyCode.RightArrow };
    public KeyCode keyTutup = KeyCode.Escape;
    public KeyCode keyTabAtas = KeyCode.UpArrow;      // pindah ke tab/bagian sebelumnya
    public KeyCode keyTabBawah = KeyCode.DownArrow;   // pindah ke tab/bagian berikutnya

    [Header("Jeda Script Lain Selama Panel Terbuka")]
    [Tooltip("Script yang dinonaktifkan sementara saat panel ini terbuka dan diaktifkan lagi saat ditutup. " +
             "Isi dengan script yang juga memakai keyboard A / D (mis. script stempel) supaya tidak ikut bereaksi. " +
             "Script itu sendiri tidak perlu diubah.")]
    public Behaviour[] jedaSelamaTerbuka;

    [Header("Animasi Zoom Buka / Tutup")]
    public float durasiBukaTutup = 0.6f;
    [Range(0.1f, 1f)] public float skalaTertutup = 0.5f;   // ukuran buku tertutup saat awal/akhir animasi

    [Header("Animasi Balik Halaman")]
    public float durasiFlip = 0.7f;
    public float angkatSkalaY = 0.04f;
    public bool teksHilangDulu = false;
    public float durasiTeksHilang = 0.15f;

    [Header("Canvas")]
    [Tooltip("Jika canvas panel bukan Overlay, ia dijadikan Overlay selama panel terbuka (dan dikembalikan setelah ditutup). " +
             "Canvas Screen Space - Camera milik kamera 1 tertutup Camera 2 dan tidak bisa menerima klik di Desk View.")]
    public bool paksaOverlaySaatBuka = true;

    [Header("Debug")]
    [Tooltip("Tampilkan log alur buka/tutup panel di Console.")]
    public bool logDebug = true;

    private int bagianAktif = 0;
    private int index = 0;

    // Status panel INI (berbeda dari IsPanelOpen yang mencakup semua panel)
    private bool terbuka = false;

    private Canvas canvasPanel;
    private RenderMode modeAsliCanvas;
    private Camera kameraAsliCanvas;
    private int sortAsliCanvas;
    private bool canvasDiubah = false;

    private readonly List<Behaviour> dijeda = new List<Behaviour>();
    private bool sudahPeringatRoot = false;

    // true juga saat animasi buka/tutup. Waktu mulai dicatat untuk watchdog (deteksi animasi macet).
    private bool _sedangFlip = false;
    private float waktuBusyMulai;
    private bool sedangFlip
    {
        get { return _sedangFlip; }
        set
        {
            if (value && !_sedangFlip) waktuBusyMulai = Time.unscaledTime;
            _sedangFlip = value;
        }
    }

    private Vector2[] posisiTabAwal;

    // Nilai dasar buku saat animasi buka/tutup
    private Vector3 posDasarRoot, posTertutupRoot, skalaDasarRoot, skalaDasarKiri;

    private BagianBuku Aktif => bagian[bagianAktif];
    private RectTransform BukuKiri => Aktif.bukuKiri;
    private RectTransform BukuKanan => Aktif.bukuKanan;
    private int JumlahHalaman => Mathf.Min(BukuKiri.childCount, BukuKanan.childCount);
    private bool BisaSebelumnya => !sedangFlip && index > 0;
    private bool BisaSelanjutnya => !sedangFlip && index < JumlahHalaman - 1;

    // =========================================================
    // UNITY
    // =========================================================

    void Awake()
    {
        PeriksaPenempatan();

        if (tombolA != null) tombolA.onClick.AddListener(HalamanSebelumnya);
        if (tombolD != null) tombolD.onClick.AddListener(HalamanSelanjutnya);
        if (tombolKembali != null) tombolKembali.onClick.AddListener(Tutup);

        posisiTabAwal = new Vector2[bagian.Length];
        for (int i = 0; i < bagian.Length; i++)
        {
            int idx = i;
            var tab = bagian[i].tombolTab;
            if (tab == null) continue;

            posisiTabAwal[i] = ((RectTransform)tab.transform).anchoredPosition;
            tab.onClick.AddListener(() => PindahBagian(idx));
        }

        if (panelDoc != null)
        {
            if (panelGroup == null)
            {
                panelGroup = panelDoc.GetComponent<CanvasGroup>();
                if (panelGroup == null) panelGroup = panelDoc.AddComponent<CanvasGroup>();
            }
            panelDoc.SetActive(false);
        }
    }

    void OnDisable()
    {
        BersihkanStatus();
    }

    void OnDestroy()
    {
        BersihkanStatus();
    }

    void Update()
    {
        // Watchdog: jika animasi macet (mis. tween tidak pernah selesai), pulihkan paksa agar panel tidak freeze
        if (sedangFlip && Time.unscaledTime - waktuBusyMulai > Mathf.Max(5f, (durasiBukaTutup + durasiFlip) * 4f))
        {
            PulihkanPaksa();
            return;
        }

        // Input hanya diproses oleh panel yang SEDANG TERBUKA (bukan panel lain, bukan saat tertutup)
        if (!terbuka || sedangFlip) return;

        if (Input.GetKeyDown(keyTutup)) { Tutup(); return; }

        if (tampilkanNavbar)
        {
            if (Input.GetKeyDown(keyTabAtas)) { PindahTab(-1); return; }
            if (Input.GetKeyDown(keyTabBawah)) { PindahTab(+1); return; }
        }

        foreach (var k in keySebelumnya)
            if (Input.GetKeyDown(k)) { HalamanSebelumnya(); return; }

        foreach (var k in keySelanjutnya)
            if (Input.GetKeyDown(k)) { HalamanSelanjutnya(); return; }
    }

    // =========================================================
    // PENEMPATAN & COROUTINE
    // =========================================================

    // Controller yang berada di dalam panelDoc / rootBuku ikut dinonaktifkan, lalu coroutine gagal berjalan
    // ("Coroutine couldn't be started because the game object ... is inactive").
    void PeriksaPenempatan()
    {
        if (panelDoc != null && transform.IsChildOf(panelDoc.transform))
        {
            Debug.LogError("[Book] BookPanelController '" + name + "' berada DI DALAM panelDoc '" + panelDoc.name +
                           "' yang dinonaktifkan oleh script ini. Pindahkan ke objek yang selalu aktif.", this);
        }

        if (bagian == null) return;

        foreach (var b in bagian)
        {
            if (b.rootBuku != null && transform.IsChildOf(b.rootBuku.transform))
            {
                Debug.LogError("[Book] BookPanelController '" + name + "' berada DI DALAM rootBuku '" + b.rootBuku.name +
                               "' yang dinonaktifkan saat pindah bagian. Pindahkan ke objek yang selalu aktif " +
                               "(di luar panelDoc dan semua rootBuku).", this);
            }
        }
    }

    bool Mulai(IEnumerator routine)
    {
        if (!isActiveAndEnabled)
        {
            Debug.LogError("[Book] '" + name + "' tidak aktif sehingga animasi tidak bisa dijalankan. " +
                           "Pindahkan BookPanelController ke objek yang selalu aktif.", this);
            return false;
        }

        StartCoroutine(routine);
        return true;
    }

    // =========================================================
    // BUKA / TUTUP
    // =========================================================

    /// <param name="asal">Opsional: objek buku tertutup di dunia (animasi zoom mulai dari posisi layarnya).</param>
    public void Buka(int indexBagian = 0, Transform asal = null, Camera kameraAsal = null)
    {
        // Hanya satu panel buku yang boleh terbuka dalam satu waktu
        if (IsPanelOpen) return;

        if (bagian == null || indexBagian < 0 || indexBagian >= bagian.Length)
        {
            Debug.LogWarning("BookPanelController: indexBagian tidak valid: " + indexBagian, this);
            return;
        }

        Log("Buka() dipanggil | bagian=" + indexBagian +
            " | asal=" + (asal != null ? asal.name : "null") +
            " | kamera=" + (kameraAsal != null ? kameraAsal.name : "null (Camera.main)") +
            " | timeScale=" + Time.timeScale);

        Mulai(BukaRoutine(indexBagian, asal, kameraAsal));
    }

    public void Tutup()
    {
        if (!terbuka || sedangFlip) return;
        Mulai(TutupRoutine());
    }

    IEnumerator BukaRoutine(int indexBagian, Transform asal, Camera kameraAsal)
    {
        sedangFlip = true;
        terbuka = true;
        terbukaSet.Add(this);

        // Hanya panel buku yang bisa di-interact; objek dunia dan UI lain dikunci
        InteractionLock.Lock(this, DaftarIzinInteraksi());

        // Script yang ikut memakai keyboard A / D (mis. stempel) dijeda
        JedaScriptLain(true);

        SiapkanCanvasOverlay();
        panelDoc.SetActive(true);
        SetTombolTerlihat(true);
        AktifkanBagian(indexBagian, true);

        RectTransform root = Aktif.rootBuku != null ? Aktif.rootBuku.GetComponent<RectTransform>() : null;
        RectTransform kiri = BukuKiri;
        RectTransform kanan = BukuKanan;

        if (root != null && durasiBukaTutup > 0f)
        {
            // Simpan kondisi terbuka (kondisi akhir)
            posDasarRoot = root.localPosition;
            skalaDasarRoot = root.localScale;
            skalaDasarKiri = kiri.localScale;

            // Pivot ruas kiri dipindah ke tulang punggung supaya "membuka" ke arah kiri
            SetPivotTanpaGeser(kiri, new Vector2(1f, kiri.pivot.y));

            // Hitung posisi buku tertutup: ruas kanan (sampul) berada di titik asal
            posTertutupRoot = HitungPosTertutup(root, kanan, asal, kameraAsal);

            // Kondisi awal: buku tertutup (hanya ruas kanan tampak)
            root.localPosition = posTertutupRoot;
            root.localScale = skalaDasarRoot * skalaTertutup;
            kiri.localScale = new Vector3(0f, skalaDasarKiri.y, skalaDasarKiri.z);
            panelGroup.alpha = 0f;

            float d = durasiBukaTutup;
            Sequence seq = DOTween.Sequence().SetUpdate(true);   // tidak terpengaruh Time.timeScale
            seq.Join(panelGroup.DOFade(1f, d * 0.4f));
            seq.Join(root.DOLocalMove(posDasarRoot, d).SetEase(Ease.OutCubic));
            seq.Join(root.DOScale(skalaDasarRoot, d).SetEase(Ease.OutCubic));
            seq.Join(kiri.DOScaleX(skalaDasarKiri.x, d * 0.6f).SetDelay(d * 0.4f).SetEase(Ease.OutSine));
            Log("animasi buka dimulai | posTertutup=" + posTertutupRoot + " | posDasar=" + posDasarRoot);
            yield return seq.WaitForCompletion();
        }
        else
        {
            panelGroup.alpha = 1f;
        }

        sedangFlip = false;
        RefreshTombol();
        Log("panel terbuka penuh");
    }

    IEnumerator TutupRoutine()
    {
        sedangFlip = true;

        // Semua tombol (A, D, Kembali, navbar) langsung hilang saat zoom out dimulai
        SetTombolTerlihat(false);

        RectTransform root = Aktif.rootBuku != null ? Aktif.rootBuku.GetComponent<RectTransform>() : null;
        RectTransform kiri = BukuKiri;

        if (root != null && durasiBukaTutup > 0f)
        {
            float d = durasiBukaTutup;
            Sequence seq = DOTween.Sequence().SetUpdate(true);   // tidak terpengaruh Time.timeScale
            seq.Join(kiri.DOScaleX(0f, d * 0.6f).SetEase(Ease.InSine));
            seq.Join(root.DOLocalMove(posTertutupRoot, d).SetDelay(d * 0.3f).SetEase(Ease.InCubic));
            seq.Join(root.DOScale(skalaDasarRoot * skalaTertutup, d).SetDelay(d * 0.3f).SetEase(Ease.InCubic));
            seq.Join(panelGroup.DOFade(0f, d * 0.4f).SetDelay(d * 0.6f));
            yield return seq.WaitForCompletion();

            // Kembalikan ke kondisi terbuka agar pindah bagian / buka berikutnya normal
            root.localPosition = posDasarRoot;
            root.localScale = skalaDasarRoot;
            kiri.localScale = skalaDasarKiri;
        }

        panelGroup.alpha = 1f;
        panelDoc.SetActive(false);
        PulihkanCanvas();

        // Interaksi dunia / UI lain dibuka lagi, script yang dijeda dinyalakan kembali
        InteractionLock.Unlock(this);
        JedaScriptLain(false);

        terbuka = false;
        terbukaSet.Remove(this);
        sedangFlip = false;
    }

    // Dipakai saat objek ini dinonaktifkan / dihancurkan ketika panel masih terbuka
    void BersihkanStatus()
    {
        InteractionLock.Unlock(this);
        JedaScriptLain(false);
        PulihkanCanvas();

        if (terbuka)
        {
            PulihkanTransformDasar();

            if (panelGroup != null) panelGroup.alpha = 1f;
            if (panelDoc != null) panelDoc.SetActive(false);
        }

        terbukaSet.Remove(this);
        terbuka = false;
        _sedangFlip = false;
    }

    // =========================================================
    // JEDA SCRIPT LAIN (mis. stempel yang juga memakai A / D)
    // =========================================================

    void JedaScriptLain(bool jeda)
    {
        if (jeda)
        {
            if (jedaSelamaTerbuka == null) return;

            foreach (Behaviour b in jedaSelamaTerbuka)
            {
                if (b == null || !b.enabled || b == this) continue;

                b.enabled = false;
                dijeda.Add(b);
            }
        }
        else
        {
            foreach (Behaviour b in dijeda)
                if (b != null)
                    b.enabled = true;

            dijeda.Clear();
        }
    }

    // Semua bagian yang tetap boleh di-interact selama panel terbuka (panel, navbar, tombol di luar panel)
    Transform[] DaftarIzinInteraksi()
    {
        var list = new List<Transform>();

        if (panelDoc != null) list.Add(panelDoc.transform);
        if (navbarRoot != null) list.Add(navbarRoot.transform);
        if (tombolA != null) list.Add(tombolA.transform);
        if (tombolD != null) list.Add(tombolD.transform);
        if (tombolKembali != null) list.Add(tombolKembali.transform);

        foreach (var b in bagian)
            if (b.tombolTab != null) list.Add(b.tombolTab.transform);

        return list.ToArray();
    }

    // =========================================================
    // CANVAS
    // =========================================================

    // Jadikan canvas panel Overlay selama panel terbuka agar selalu di atas Camera 2 dan menerima klik
    void SiapkanCanvasOverlay()
    {
        if (!paksaOverlaySaatBuka || panelDoc == null) return;

        Canvas c = panelDoc.GetComponentInParent<Canvas>();
        if (c == null) return;

        c = c.rootCanvas;
        if (c.renderMode == RenderMode.ScreenSpaceOverlay) return;

        canvasPanel = c;
        modeAsliCanvas = c.renderMode;
        kameraAsliCanvas = c.worldCamera;
        sortAsliCanvas = c.sortingOrder;
        canvasDiubah = true;

        c.renderMode = RenderMode.ScreenSpaceOverlay;
        Log("canvas panel (" + c.name + ") dijadikan Overlay sementara, mode asli: " + modeAsliCanvas);
    }

    void PulihkanCanvas()
    {
        if (!canvasDiubah || canvasPanel == null) return;

        canvasPanel.renderMode = modeAsliCanvas;
        canvasPanel.worldCamera = kameraAsliCanvas;
        canvasPanel.sortingOrder = sortAsliCanvas;
        canvasDiubah = false;
    }

    // =========================================================
    // PEMULIHAN
    // =========================================================

    void Log(string pesan)
    {
        if (logDebug) Debug.Log("[Book] " + name + ": " + pesan, this);
    }

    void PulihkanTransformDasar()
    {
        if (bagian == null || bagian.Length == 0 || bagianAktif >= bagian.Length) return;
        if (skalaDasarRoot == Vector3.zero) return;

        RectTransform root = Aktif.rootBuku != null ? Aktif.rootBuku.GetComponent<RectTransform>() : null;
        RectTransform kiri = BukuKiri;

        if (root != null)
        {
            DOTween.Kill(root);
            root.localPosition = posDasarRoot;
            root.localScale = skalaDasarRoot;
        }

        if (kiri != null)
        {
            DOTween.Kill(kiri);
            kiri.localScale = skalaDasarKiri;
        }
    }

    // Dipanggil watchdog saat animasi macet: kembalikan semuanya ke keadaan terbuka yang bisa dipakai
    void PulihkanPaksa()
    {
        Debug.LogError("[Book] Animasi macet lebih dari batas waktu, dipulihkan paksa. " +
                       "timeScale=" + Time.timeScale + " | IsPanelOpen=" + IsPanelOpen +
                       " | Cek error merah lain di Console sebelum pesan ini.", this);

        StopAllCoroutines();

        if (panelGroup != null) { DOTween.Kill(panelGroup); panelGroup.alpha = 1f; }
        PulihkanTransformDasar();

        // Buang klon flip yang mungkin tertinggal
        RectTransform kiri = BukuKiri;
        if (kiri != null && kiri.parent != null)
        {
            for (int i = kiri.parent.childCount - 1; i >= 0; i--)
            {
                Transform c = kiri.parent.GetChild(i);
                if (c.name.EndsWith("_Flip")) Destroy(c.gameObject);
            }
        }

        TampilkanHalaman(BukuKiri, index);
        TampilkanHalaman(BukuKanan, index);

        sedangFlip = false;
        SetTombolTerlihat(true);
        RefreshTombol();
    }

    // Posisi lokal root agar pusat ruas kanan (sampul) tepat di titik asal (atau di posisi dasar jika asal kosong)
    Vector3 HitungPosTertutup(RectTransform root, RectTransform kanan, Transform asal, Camera kameraAsal)
    {
        // Pusat ruas kanan dalam koordinat lokal root
        float cx = root.InverseTransformPoint(kanan.TransformPoint(kanan.rect.center)).x;
        float cy = root.InverseTransformPoint(kanan.TransformPoint(kanan.rect.center)).y;

        Vector2 titik = new Vector2(posDasarRoot.x, posDasarRoot.y);

        Camera camDunia = kameraAsal != null ? kameraAsal : Camera.main;

        if (asal != null && camDunia != null && root.parent is RectTransform parentRect)
        {
            Canvas canvas = root.GetComponentInParent<Canvas>();
            Camera camUI = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;

            Vector2 layar = camDunia.WorldToScreenPoint(asal.position);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, layar, camUI, out Vector2 lokal))
                titik = lokal;
        }

        return new Vector3(
            titik.x - cx * skalaDasarRoot.x * skalaTertutup,
            titik.y - cy * skalaDasarRoot.y * skalaTertutup,
            posDasarRoot.z);
    }

    // =========================================================
    // NAVBAR / PINDAH BAGIAN
    // =========================================================

    public void PindahBagian(int indexBagian)
    {
        if (!terbuka || sedangFlip) return;
        if (indexBagian < 0 || indexBagian >= bagian.Length) return;
        if (indexBagian == bagianAktif) return;

        AktifkanBagian(indexBagian, false);
    }

    void AktifkanBagian(int indexBagian, bool instanTab)
    {
        bagianAktif = indexBagian;
        index = 0;

        for (int i = 0; i < bagian.Length; i++)
        {
            GameObject root = bagian[i].rootBuku;
            if (root == null) continue;

            bool tampil = i == bagianAktif;

            // Jangan pernah menonaktifkan objek yang berisi controller ini sendiri
            if (!tampil && transform.IsChildOf(root.transform))
            {
                if (!sudahPeringatRoot)
                {
                    sudahPeringatRoot = true;
                    Debug.LogError("[Book] rootBuku '" + root.name + "' berisi BookPanelController ini sehingga tidak dinonaktifkan. " +
                                   "Dua buku akan tampil bersamaan. Pindahkan controller ke objek yang selalu aktif.", this);
                }
                continue;
            }

            root.SetActive(tampil);
        }

        TampilkanHalaman(BukuKiri, index);
        TampilkanHalaman(BukuKanan, index);

        UpdateTampilanTab(instanTab);
        RefreshTombol();
    }

    // Pindah ke tab berikutnya/sebelumnya (arah -1 = atas, +1 = bawah). Hanya bagian yang punya tab yang dilewati ke.
    void PindahTab(int arah)
    {
        for (int i = bagianAktif + arah; i >= 0 && i < bagian.Length; i += arah)
        {
            if (bagian[i].tombolTab != null)
            {
                PindahBagian(i);
                return;
            }
        }
    }

    // Tampilkan / sembunyikan semua tombol (A, D, Kembali, dan navbar)
    void SetTombolTerlihat(bool tampil)
    {
        if (tombolA != null) tombolA.gameObject.SetActive(tampil);
        if (tombolD != null) tombolD.gameObject.SetActive(tampil);
        if (tombolKembali != null) tombolKembali.gameObject.SetActive(tampil);

        bool navbarTampil = tampil && tampilkanNavbar;

        if (navbarRoot != null)
            navbarRoot.SetActive(navbarTampil);

        for (int i = 0; i < bagian.Length; i++)
        {
            var tab = bagian[i].tombolTab;
            if (tab != null && navbarRoot == null)
                tab.gameObject.SetActive(navbarTampil);
        }
    }

    void UpdateTampilanTab(bool instan)
    {
        for (int i = 0; i < bagian.Length; i++)
        {
            var tab = bagian[i].tombolTab;
            if (tab == null) continue;

            var rt = (RectTransform)tab.transform;
            Vector2 target = posisiTabAwal[i] + (i == bagianAktif ? new Vector2(geserTabAktif, 0f) : Vector2.zero);

            rt.DOKill();
            if (instan) rt.anchoredPosition = target;
            else rt.DOAnchorPos(target, durasiGeserTab).SetEase(Ease.OutSine);
        }
    }

    // =========================================================
    // NAVIGASI HALAMAN
    // =========================================================

    public void HalamanSelanjutnya()
    {
        if (!terbuka || !BisaSelanjutnya) return;
        Mulai(FlipRoutine(+1));
    }

    public void HalamanSebelumnya()
    {
        if (!terbuka || !BisaSebelumnya) return;
        Mulai(FlipRoutine(-1));
    }

    // =========================================================
    // ANIMASI BALIK HALAMAN
    // =========================================================
    IEnumerator FlipRoutine(int arah)
    {
        sedangFlip = true;
        RefreshTombol();

        bool next = arah > 0;
        int dari = index;
        int ke = index + arah;

        RectTransform kiri = BukuKiri;
        RectTransform kanan = BukuKanan;

        RectTransform sisiAsal = next ? kanan : kiri;
        RectTransform sisiTujuan = next ? kiri : kanan;

        RectTransform depan = Klon(sisiAsal, dari, kiri, kanan);
        RectTransform belakang = Klon(sisiTujuan, ke, kiri, kanan);
        belakang.gameObject.SetActive(false);

        SetPivotTanpaGeser(depan, new Vector2(next ? 0f : 1f, depan.pivot.y));
        SetPivotTanpaGeser(belakang, new Vector2(next ? 1f : 0f, belakang.pivot.y));

        TampilkanHalaman(sisiAsal, ke);

        if (teksHilangDulu)
        {
            foreach (var t in depan.GetComponentsInChildren<TMP_Text>(true))
                t.DOFade(0f, durasiTeksHilang).SetUpdate(true);
            yield return new WaitForSecondsRealtime(durasiTeksHilang);
        }

        float setengah = durasiFlip * 0.5f;
        Vector3 skala = depan.localScale;

        Sequence s1 = DOTween.Sequence().SetUpdate(true);
        s1.Join(depan.DOScaleX(0f, setengah).SetEase(Ease.InSine));
        s1.Join(depan.DOScaleY(skala.y * (1f + angkatSkalaY), setengah).SetEase(Ease.OutSine));
        yield return s1.WaitForCompletion();

        depan.gameObject.SetActive(false);

        Vector3 skalaTujuan = sisiTujuan.localScale;
        belakang.localScale = new Vector3(0f, skalaTujuan.y * (1f + angkatSkalaY), skalaTujuan.z);
        belakang.gameObject.SetActive(true);

        Sequence s2 = DOTween.Sequence().SetUpdate(true);
        s2.Join(belakang.DOScaleX(skalaTujuan.x, setengah).SetEase(Ease.OutSine));
        s2.Join(belakang.DOScaleY(skalaTujuan.y, setengah).SetEase(Ease.InSine));
        yield return s2.WaitForCompletion();

        index = ke;
        TampilkanHalaman(kiri, index);
        TampilkanHalaman(kanan, index);

        Destroy(depan.gameObject);
        Destroy(belakang.gameObject);

        sedangFlip = false;
        RefreshTombol();
    }

    // =========================================================
    // HELPER
    // =========================================================

    RectTransform Klon(RectTransform sumber, int halaman, RectTransform kiri, RectTransform kanan)
    {
        var obj = Instantiate(sumber.gameObject, sumber.parent);
        obj.name = sumber.name + "_Flip";

        var rt = obj.GetComponent<RectTransform>();
        int idxTertinggi = Mathf.Max(kiri.GetSiblingIndex(), kanan.GetSiblingIndex());
        rt.SetSiblingIndex(idxTertinggi + 1);

        var cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        TampilkanHalaman(rt, halaman);
        return rt;
    }

    void TampilkanHalaman(RectTransform ruas, int halaman)
    {
        for (int i = 0; i < ruas.childCount; i++)
            ruas.GetChild(i).gameObject.SetActive(i == halaman);
    }

    void SetPivotTanpaGeser(RectTransform rt, Vector2 pivotBaru)
    {
        Vector2 delta = pivotBaru - rt.pivot;
        Vector2 ukuran = rt.rect.size;
        rt.pivot = pivotBaru;
        rt.anchoredPosition += new Vector2(
            delta.x * ukuran.x * rt.localScale.x,
            delta.y * ukuran.y * rt.localScale.y);
    }

    void RefreshTombol()
    {
        if (tombolA != null) tombolA.interactable = BisaSebelumnya;
        if (tombolD != null) tombolD.interactable = BisaSelanjutnya;
        if (tombolKembali != null) tombolKembali.interactable = !sedangFlip;

        for (int i = 0; i < bagian.Length; i++)
            if (bagian[i].tombolTab != null)
                bagian[i].tombolTab.interactable = !sedangFlip;
    }
}