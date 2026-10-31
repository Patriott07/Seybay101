using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhoneManager : MonoBehaviour
{
    public static PhoneManager Instance;

    // Cek ini dari script lain (stempel, drag dokumen, dll) agar tidak bisa dipakai saat HP terbuka
    public static bool IsPhoneOpen { get; private set; }

    // True saat Trust habis: semua aksi gameplay harus diblokir, hanya HP yang boleh dipakai
    public static bool IsLockdown { get; private set; }

    // Gabungan, cukup cek ini di script gameplay lain
    public static bool IsInputTerkunci => IsPhoneOpen || IsLockdown;

    // Dipanggil saat pemain membuka chat Manager_Boo (titik pasang fade)
    public static event System.Action OnManagerBooDibuka;

    // ---------- Data runtime (tidak mengubah ScriptableObject) ----------
    class ChatLine
    {
        public string teks;
        public bool dariAldo;
    }

    class ContactState
    {
        public List<ChatLine> history = new List<ChatLine>();

        public int stageIndex = 0;              // stage yang sedang/akan berjalan
        public bool stageMulai = false;         // stage sudah dimulai
        public List<PhoneLine> baris;           // baris milik stage aktif
        public int lineIndex = 0;               // baris berikutnya yang diproses
        public bool menungguBalasan = false;    // baris berikutnya adalah balasan Aldo
        public bool sedangKirim = false;        // kontak sedang mengirim pesan
        public bool unread = false;
    }

    // ---------- Data berita (panel Browse) ----------
    [System.Serializable]
    public class BeritaData
    {
        public string judul;
        [TextArea(4, 12)] public string isi;
        public Sprite gambar;
    }

    private readonly Dictionary<PhoneContactSO, ContactState> state =
        new Dictionary<PhoneContactSO, ContactState>();

    [Header("Data Kontak")]
    [Tooltip("Masukkan SEMUA kontak, termasuk Azura Corp dan Manager_Boo (atur Tipe di asset-nya)")]
    public List<PhoneContactSO> daftarKontak = new List<PhoneContactSO>();
    [Tooltip("Kirim pesan stage pertama semua kontak biasa saat game mulai")]
    public bool kirimStageAwalOtomatis = true;

    // ==========================================
    // --- PANEL / HALAMAN ---
    // ==========================================

    [Header("Panel HP (container utama)")]
    [Tooltip("Root HP. Harus UI (RectTransform). Dianimasikan masuk dari kanan ke kiri.")]
    public GameObject panelPhone;
    [Tooltip("Seberapa jauh (pixel) HP berada di kanan layar sebelum masuk")]
    public float jarakGeserHp = 1200f;
    public float durasiGeserHp = 0.35f;

    [Header("Blokir Interaksi (saat HP terbuka / lockdown)")]
    [Tooltip("Image transparan full-screen di Canvas HP, anak PERTAMA di BELAKANG Panel Phone. Memblokir semua UI di luar HP saat HP terbuka.")]
    public GameObject penghalangInput;
    [Tooltip("Kamera yang diblokir dari OnMouseDown/Up/Drag saat HP terbuka atau lockdown. Kosong = Camera.main")]
    public List<Camera> kameraDiblokir = new List<Camera>();

    [Header("Halaman di dalam HP")]
    [Tooltip("Home screen: 3 tombol (Browse, Chat, Call)")]
    public GameObject panelHome;
    [Tooltip("History chat dengan NPC (daftar kontak)")]
    public GameObject panelInbox;
    [Tooltip("Isi chat dengan satu kontak")]
    public GameObject panelChat;
    [Tooltip("Daftar berita terkini")]
    public GameObject panelBrowse;
    [Tooltip("Isi satu berita + 1 gambar")]
    public GameObject panelBeritaDetail;
    [Tooltip("Halaman panggilan (dering lalu panggilan berjalan)")]
    public GameObject panelPanggilan;
    public float durasiTransisiHalaman = 0.25f;

    [Header("Panel History")]
    [Tooltip("Panel history (tujuan swipe KIRI dari Home)")]
    public GameObject panelHistory;
    public Button tombolKembaliDariHistory;
    [Tooltip("PhoneSwipeArea di Panel History untuk swipe KANAN kembali ke Home (opsional)")]
    public PhoneSwipeArea swipeHistory;
    [Tooltip("Satu TextMeshProUGUI di dalam Content panel history (Rich Text aktif)")]
    public TextMeshProUGUI teksHistory;
    [Tooltip("Scroll View panel history (opsional, otomatis scroll ke pesan terbaru)")]
    public ScrollRect scrollHistory;
    [Tooltip("Batas pesan terakhir yang ditampilkan (0 = semua)")]
    public int maksPesanHistory = 0;
    public string namaMC = "Aldo";
    public string namaNPC = "NPC";

    [Header("Home Screen")]
    public Button tombolBrowse;
    public Button tombolChat;
    public Button tombolCall;
    [Tooltip("Pasang PhoneSwipeArea di Panel Home (Image dengan Raycast Target aktif)")]
    public PhoneSwipeArea swipeHome;
    [Tooltip("Pasang PhoneSwipeArea di Panel Inbox untuk swipe KANAN kembali ke Home (opsional)")]
    public PhoneSwipeArea swipeInbox;
    [Tooltip("Tombol tutup HP di Home (opsional)")]
    public Button tombolTutupDariHome;

    [Header("Tombol Umum")]
    [Tooltip("Tombol di HUD untuk membuka HP")]
    public Button tombolBukaHp;
    [Tooltip("Tombol kembali ke Home dari Inbox (opsional)")]
    public Button tombolHomeDariInbox;
    [Tooltip("Tombol '<' di header chat")]
    public Button tombolKembaliKeInbox;

    [Header("Inbox")]
    public Transform containerKontak;
    public PhoneContactItem prefabKontakItem;

    [Header("Chat")]
    public Image avatarHeader;
    public TextMeshProUGUI namaHeader;
    public RectTransform containerPesan;
    public ScrollRect scrollChat;
    public PhoneBubble prefabBubble;
    public float lebarMaksBubble = 380f;
    public Color warnaBubbleKontak = Color.white;
    public Color warnaBubbleAldo = new Color(0.75f, 0.9f, 1f);
    [Tooltip("Centang = pesan terlama juga dihapus dari riwayat (tidak muncul lagi saat chat dibuka ulang). Hilangkan centang = hanya bubble yang dibuang, riwayat tetap utuh.")]
    public bool hapusRiwayatSaatPenuh = true;

    [Header("Browse / Berita")]
    public List<BeritaData> daftarBerita = new List<BeritaData>();
    public Transform containerBerita;
    public PhoneNewsItem prefabBeritaItem;
    public Button tombolKembaliHomeDariBrowse;
    [Header("Detail Berita")]
    public TextMeshProUGUI judulDetail;
    public TextMeshProUGUI isiDetail;
    public Image gambarDetail;
    public ScrollRect scrollBerita;
    public Button tombolKembaliKeBrowse;

    [Header("Panggilan (Call)")]
    [Tooltip("Logo handphone calling (bergerak naik setelah panggilan diangkat)")]
    public RectTransform logoTelepon;
    [Tooltip("Halaman daftar kontak yang bisa ditelepon (tampil setelah tombol Call di Home ditekan)")]
    public GameObject panelKontakCall;
    [Tooltip("Content tempat item kontak dibuat (memakai Prefab Kontak Item yang sama dengan Inbox)")]
    public Transform containerKontakCall;
    public Button tombolKembaliDariKontakCall;
    public string teksPreviewCall = "Ketuk untuk menelepon";
    [Tooltip("Nama kontak yang sedang ditelepon (opsional)")]
    public TextMeshProUGUI namaPanggilan;
    [Tooltip("Avatar kontak yang sedang ditelepon (opsional)")]
    public Image avatarPanggilan;

    // Pengaturan khusus per kontak (opsional). Kontak yang tidak terdaftar memakai pengaturan default di bawah.
    [System.Serializable]
    public class DataPanggilan
    {
        public PhoneContactSO kontak;
        [Tooltip("Audio saat tersambung (kosong = pakai Audio Panggilan default)")]
        public AudioClip audio;
        [Tooltip("Lama panggilan dalam detik (0 = pakai Durasi Panggilan default)")]
        public float durasiDetik;
    }
    public List<DataPanggilan> dataPanggilan = new List<DataPanggilan>();
    [Tooltip("Teks status: Memanggil... / Tersambung (opsional)")]
    public TextMeshProUGUI teksStatusPanggilan;
    [Tooltip("Panel yang tampil setelah diangkat: berisi timer (awalnya dimatikan, anak dari Panel Panggilan)")]
    public GameObject panelPanggilanBerjalan;
    public TextMeshProUGUI teksTimerPanggilan;
    [Tooltip("Audio percakapan yang diputar setelah diangkat")]
    public AudioClip audioPanggilan;
    [Tooltip("Nada dering (loop) selama menunggu diangkat (opsional)")]
    public AudioClip sfxDering;
    public float kecepatanDenyut = 6f;
    [Tooltip("Besar perubahan skala logo telepon saat berdering (0.15 = 85%-115%)")]
    public float besarDenyut = 0.15f;
    [Tooltip("Jeda setelah diangkat sebelum logo naik")]
    public float jedaSetelahAngkat = 1f;
    public float jarakLogoNaik = 300f;
    public float durasiLogoNaik = 0.5f;
    [Tooltip("Lama berdering sebelum tersambung (detik)")]
    public float jedaDering = 2f;
    [Tooltip("Lama panggilan setelah tersambung sampai otomatis selesai (detik)")]
    public float durasiPanggilanDetik = 10f;
    [Tooltip("Jeda menampilkan 'Panggilan berakhir' sebelum kembali ke Home")]
    public float jedaSetelahSelesai = 1f;

    [Header("Balas Pesan")]
    public PhoneHoldButton tombolBalas;
    public TextMeshProUGUI teksInput;
    public float kecepatanKetik = 0.04f;
    [Tooltip("Jeda sebelum tiap pesan kontak muncul (setelah Aldo membalas)")]
    public float jedaResponKontak = 0.8f;

    [Header("Azura Corp: Pesan Kesalahan")]
    [TextArea(2, 4)]
    public string pesanKesalahan = "PERINGATAN: Terjadi kesalahan. Mohon lebih teliti dalam menjalankan tugas.";
    [Header("Azura Corp: Peringatan Trust Habis")]
    [TextArea(2, 4)]
    public string pesanTrustHabis = "PERINGATAN: Tingkat kepercayaan Anda sudah habis. Segera hubungi manajer Anda.";

    [Header("Trust Habis: Lockdown & Manager_Boo")]
    [Tooltip("Jeda dari Trust habis sampai peringatan Azura muncul")]
    public float jedaPeringatanAzura = 1.5f;
    [Tooltip("Jeda dari peringatan Azura sampai pesan Manager_Boo mulai")]
    public float jedaSebelumSpam = 2.5f;
    public float intervalSpamAwal = 2f;
    public float intervalSpamMin = 0.6f;
    [Tooltip("Batas jumlah pesan jika Kirim Baris Sekali dimatikan (mode spam berulang)")]
    public int maksPesanSpam = 40;
    [Tooltip("Centang = semua baris Boo dikirim berurutan SEKALI lalu berhenti. Hilangkan centang = diulang terus (spam) sampai chat dibuka.")]
    public bool kirimBarisSekali = true;
    [Tooltip("Centang = event OnManagerBooDibuka dipanggil setelah chat dibuka DAN semua pesan sudah masuk. Hilangkan = langsung saat chat dibuka (spam berhenti).")]
    public bool tungguSemuaPesanSebelumEvent = true;
    [Tooltip("Jeda membaca pesan terakhir sebelum event dipanggil")]
    public float jedaSetelahPesanTerakhir = 1.5f;
    [Tooltip("Object yang dimatikan saat lockdown (mis. panel Accept/Reject, dokumen). JANGAN masukkan tombol HP.")]
    public List<GameObject> matikanSaatLockdown = new List<GameObject>();

    [Header("Audio (opsional)")]
    public AudioClip sfxPesanMasuk;
    public AudioClip sfxKirim;

    private PhoneContactSO kontakAktif;
    private Coroutine ketikCoroutine;
    private bool sedangMengetik;
    private AudioSource audioSource;
    private AudioSource callSource;

    // Blokir interaksi dunia (OnMouse*)
    private readonly Dictionary<Camera, int> eventMaskAsli = new Dictionary<Camera, int>();

    // Workflow
    private PhoneContactSO kontakAzura;
    private PhoneContactSO kontakBoo;
    private bool trustHabisDipicu;
    private float trustSebelumnya;
    private bool sudahMenyimpanTrustAwal;
    private bool booAktif;
    private bool booSudahDibuka;
    private bool semuaPesanBooTerkirim;
    private bool eventBooDipanggil;
    private Coroutine spamCoroutine;
    // Violation terakhir
    private ViolationData violationTerakhir;

    // Navigasi HP
    private GameObject halamanAktif;
    private Coroutine transisiCoroutine;
    private Coroutine geserHpCoroutine;
    private Vector2 posHpAsli;
    private readonly Dictionary<GameObject, Vector2> posAsliHalaman = new Dictionary<GameObject, Vector2>();

    // Panggilan
    private bool panggilanBerjalan;
    private Coroutine panggilanCoroutine;
    private Vector2 posLogoAsli;

    // Violation
    [System.Serializable]
    public class ViolationData
    {
        public string id;
        public string nama;
        public int penaltyTrust;
        public bool sudahTerjadi;

        // Diisi saat runtime oleh TambahViolation (daftar rule yang gagal). Tidak disimpan di Inspector.
        [System.NonSerialized] public string detail;
    }

    [Header("Violation System")]
    public List<ViolationData> daftarViolation = new List<ViolationData>();

    // ==========================================
    // --- LIFECYCLE ---
    // ==========================================

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        AudioSource[] sources = GetComponents<AudioSource>();
        audioSource = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // AudioSource terpisah untuk dering & suara panggilan (agar tidak terpotong SFX chat)
        callSource = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();
        callSource.playOnAwake = false;

        foreach (var k in daftarKontak)
        {
            if (k == null) continue;
            if (!state.ContainsKey(k)) state[k] = new ContactState();

            if (k.tipe == PhoneContactTipe.AzuraCorp && kontakAzura == null) kontakAzura = k;
            if (k.tipe == PhoneContactTipe.ManagerBoo && kontakBoo == null) kontakBoo = k;
        }
    }

    void Start()
    {
        // Tombol umum
        if (tombolBukaHp != null) tombolBukaHp.onClick.AddListener(BukaHp);
        if (tombolKembaliKeInbox != null) tombolKembaliKeInbox.onClick.AddListener(TampilkanInbox);
        if (tombolBalas != null) tombolBalas.OnHoldSelesai += MulaiBalas;

        // Home
        if (tombolBrowse != null) tombolBrowse.onClick.AddListener(BukaBrowse);
        if (tombolChat != null) tombolChat.onClick.AddListener(() => TampilkanInboxArah(1));
        if (tombolCall != null) tombolCall.onClick.AddListener(BukaPanggilan);
        if (tombolKembaliDariHistory != null) tombolKembaliDariHistory.onClick.AddListener(() => TampilkanHome(-1));
        if (swipeHistory != null) swipeHistory.OnSwipe += SaatSwipeHistory;
        if (tombolTutupDariHome != null) tombolTutupDariHome.onClick.AddListener(TutupHp);
        if (tombolHomeDariInbox != null) tombolHomeDariInbox.onClick.AddListener(() => TampilkanHome(-1));

        // Swipe
        if (swipeHome != null) swipeHome.OnSwipe += SaatSwipeHome;
        if (swipeInbox != null) swipeInbox.OnSwipe += SaatSwipeInbox;

        // Browse / berita
        if (tombolKembaliHomeDariBrowse != null) tombolKembaliHomeDariBrowse.onClick.AddListener(() => TampilkanHome(-1));
        if (tombolKembaliKeBrowse != null) tombolKembaliKeBrowse.onClick.AddListener(() => PindahHalaman(panelBrowse, -1));

        // Panggilan
        if (tombolKembaliDariKontakCall != null) tombolKembaliDariKontakCall.onClick.AddListener(() => TampilkanHome(-1));

        // Simpan posisi asli (sebelum dimatikan / dianimasikan)
        if (panelPhone != null)
        {
            RectTransform rt = panelPhone.GetComponent<RectTransform>();
            if (rt != null) posHpAsli = rt.anchoredPosition;
        }
        if (logoTelepon != null) posLogoAsli = logoTelepon.anchoredPosition;
        SimpanPosisiAsliHalaman();

        // Semua halaman mati, HP mati
        halamanAktif = null;
        SelesaikanTransisi(null);
        if (panelPhone != null) panelPhone.SetActive(false);
        if (penghalangInput != null) penghalangInput.SetActive(false);

        SimpanBaselineTrust();

        if (kirimStageAwalOtomatis)
        {
            foreach (var k in daftarKontak)
                if (k != null && k.tipe == PhoneContactTipe.Biasa) KirimStageSaatIni(k, false);
        }
    }

    void Update()
    {
        PantauTrust();
    }

    void OnDestroy()
    {
        if (tombolBalas != null) tombolBalas.OnHoldSelesai -= MulaiBalas;
        if (swipeHome != null) swipeHome.OnSwipe -= SaatSwipeHome;
        if (swipeInbox != null) swipeInbox.OnSwipe -= SaatSwipeInbox;
        if (swipeHistory != null) swipeHistory.OnSwipe -= SaatSwipeHistory;

        if (Instance == this)
        {
            IsPhoneOpen = false;
            IsLockdown = false;
            PerbaruiBlokirInput(); // kembalikan event mask kamera
        }
    }

    // Hentikan animasi ketik balasan dan reset status mengetik
    void HentikanKetik()
    {
        if (ketikCoroutine != null)
        {
            StopCoroutine(ketikCoroutine);
            ketikCoroutine = null;
        }
        sedangMengetik = false;
    }

    // Simpan Trust saat ini sebagai acuan PantauTrust
    void SimpanBaselineTrust()
    {
        EconomyManager eco = EconomyManager.Instance;
        if (eco == null) return;

        trustSebelumnya = eco.trust;
        sudahMenyimpanTrustAwal = true;
    }

    // ==========================================
    // --- BLOKIR INTERAKSI ---
    // ==========================================

    // HP terbuka: UI di luar HP diblokir penghalang, dunia (OnMouse*) diblokir lewat event mask kamera.
    // Lockdown (HP ditutup): dunia tetap diblokir, UI bebas agar tombol HP bisa ditekan.
    void PerbaruiBlokirInput()
    {
        if (penghalangInput != null) penghalangInput.SetActive(IsPhoneOpen);

        if (IsPhoneOpen || IsLockdown)
        {
            if (kameraDiblokir.Count == 0 && Camera.main != null) BlokirKamera(Camera.main);
            foreach (Camera c in kameraDiblokir) BlokirKamera(c);
        }
        else
        {
            foreach (var kv in eventMaskAsli)
                if (kv.Key != null) kv.Key.eventMask = kv.Value;
            eventMaskAsli.Clear();
        }
    }

    void BlokirKamera(Camera c)
    {
        if (c == null) return;
        if (!eventMaskAsli.ContainsKey(c)) eventMaskAsli[c] = c.eventMask;
        c.eventMask = 0;
    }

    // ==========================================
    // --- API UNTUK SCRIPT LAIN ---
    // ==========================================

    /// <summary>
    /// Mulai stage berikutnya dari kontak biasa (panggil dari script hari/event).
    /// Contoh: PhoneManager.Instance.KirimStageBerikutnya("nasya");
    /// </summary>
    public void KirimStageBerikutnya(string idKontak)
    {
        PhoneContactSO k = daftarKontak.Find(c => c != null && c.idKontak == idKontak);
        if (k == null)
        {
            Debug.LogWarning("Kontak dengan id '" + idKontak + "' tidak ditemukan.");
            return;
        }
        if (k.tipe != PhoneContactTipe.Biasa)
        {
            Debug.LogWarning("Kontak sistem (" + k.namaKontak + ") tidak dijalankan lewat stage.");
            return;
        }
        KirimStageSaatIni(k, true);
    }

    /// <summary>
    /// Dipanggil setiap kali Trust berkurang karena pemain melakukan kesalahan.
    /// Jika ada violation yang baru dicatat (TambahViolation), pesan memuat nama, detail,
    /// dan jumlah Trust yang benar-benar berkurang. Tidak ada batas jumlah pesan per stage.
    /// </summary>
    public void PicuKesalahan()
    {
        if (kontakAzura == null) return;

        if (violationTerakhir != null)
        {
            string pesan = "PELANGGARAN TERDETEKSI:\n" + violationTerakhir.nama;

            if (!string.IsNullOrEmpty(violationTerakhir.detail))
                pesan += "\n" + violationTerakhir.detail;

            pesan += "\nTrust -" + violationTerakhir.penaltyTrust;

            KirimPesanSistem(kontakAzura, pesan, true);

            violationTerakhir = null;
        }
        else
        {
            KirimPesanSistem(kontakAzura, pesanKesalahan, true);
        }
    }

    /// <summary>
    /// Catat violation terakhir. Panggil SEBELUM/SESUDAH Trust dikurangi (di frame yang sama).
    /// detail         : alasan kesalahan, mis. "- Paspor kedaluwarsa" (opsional).
    /// trustBerkurang : Trust yang benar-benar dikurangi. Kosongkan (-1) untuk memakai
    ///                  nilai bawaan di daftarViolation.
    /// </summary>
    public void TambahViolation(string id, string detail = null, int trustBerkurang = -1)
    {
        ViolationData dasar = daftarViolation.Find(v => v.id == id);

        if (dasar == null)
        {
            Debug.LogWarning("Violation dengan ID '" + id + "' tidak ditemukan.");
            return;
        }

        dasar.sudahTerjadi = true;

        // Salinan, supaya data asli di Inspector tidak ikut berubah.
        violationTerakhir = new ViolationData
        {
            id = dasar.id,
            nama = dasar.nama,
            penaltyTrust = trustBerkurang >= 0 ? trustBerkurang : dasar.penaltyTrust,
            sudahTerjadi = true,
            detail = detail
        };

        Debug.Log("Violation: " + dasar.nama + (string.IsNullOrEmpty(detail) ? "" : "\n" + detail));
    }

    public void PicuTrustHabis()
    {
        if (trustHabisDipicu) return;
        trustHabisDipicu = true;
        StartCoroutine(AlurTrustHabis());
    }

    /// <summary>Hapus semua pesan Azura (panggil saat New Game / reset data).</summary>
    public void ResetPesanAzura()
    {
        if (kontakAzura != null && state.ContainsKey(kontakAzura))
        {
            state[kontakAzura].history.Clear();
            state[kontakAzura].unread = false;
        }

    }

    /// <summary>Buka HP langsung ke chat kontak tertentu (dipakai saat toast HUD diklik).</summary>
    public void BukaHpKeKontak(PhoneContactSO kontak)
    {
        BukaHp();
        if (kontak != null && state.ContainsKey(kontak) && KontakTampil(kontak))
            BukaChat(kontak, 0);
    }

    /// <summary>
    /// Akhiri lockdown (mis. hari baru). Object di daftar matikanSaatLockdown diaktifkan lagi.
    /// Pastikan Trust sudah > 0, kalau tidak lockdown akan terpicu lagi.
    /// </summary>
    public void AkhiriLockdown()
    {
        if (spamCoroutine != null) { StopCoroutine(spamCoroutine); spamCoroutine = null; }

        IsLockdown = false;
        trustHabisDipicu = false;
        booAktif = false;
        booSudahDibuka = false;
        semuaPesanBooTerkirim = false;
        eventBooDipanggil = false;
        SimpanBaselineTrust();

        foreach (var go in matikanSaatLockdown)
            if (go != null) go.SetActive(true);

        PerbaruiBlokirInput();
        TerapkanLockdownUI();
        if (panelInbox != null && panelInbox.activeInHierarchy) RenderInbox();
    }

    // ---------- Menu debug: klik kanan komponen PhoneManager saat Play ----------

    [ContextMenu("Tes Trust Habis")]
    void TesTrustHabis()
    {
        if (!Application.isPlaying) return;
        PicuTrustHabis();
    }

    [ContextMenu("Debug: Reset Trust & Akhiri Lockdown")]
    void DebugResetTrust()
    {
        if (!Application.isPlaying) return;

        // Urutan penting: Trust dikembalikan dulu, baru lockdown diakhiri
        if (EconomyManager.Instance != null) EconomyManager.Instance.ResetTrustToMax();
        ResetPesanAzura();
        AkhiriLockdown();
    }

    // ==========================================
    // --- BUKA / TUTUP HP (SLIDE KANAN -> KIRI) ---
    // ==========================================

    public void BukaHp()
    {
        if (panelPhone == null) return;

        bool sudahTerbuka = IsPhoneOpen;
        IsPhoneOpen = true;
        PerbaruiBlokirInput();

        if (PhoneNotifHUD.Instance != null) PhoneNotifHUD.Instance.SembunyikanSegera();

        if (sudahTerbuka)
        {
            // HP sudah di layar: cukup arahkan ke halaman awal
            if (IsLockdown) TampilkanInboxArah(0); else TampilkanHome(0);
            return;
        }

        RectTransform rt = panelPhone.GetComponent<RectTransform>();
        if (rt != null && !panelPhone.activeSelf)
            rt.anchoredPosition = posHpAsli + new Vector2(jarakGeserHp, 0f);

        panelPhone.SetActive(true);

        // Saat lockdown langsung ke inbox (hanya Manager_Boo yang bisa dibuka)
        if (IsLockdown) TampilkanInboxArah(0); else TampilkanHome(0);

        if (geserHpCoroutine != null) StopCoroutine(geserHpCoroutine);
        geserHpCoroutine = StartCoroutine(GeserHp(true));
    }

    public void TutupHp()
    {
        if (IsLockdown) return; // saat lockdown HP tidak bisa ditutup
        if (!IsPhoneOpen) return;

        HentikanKetik();
        if (teksInput != null) teksInput.text = "";

        AkhiriPanggilan(false);

        kontakAktif = null;
        IsPhoneOpen = false;
        PerbaruiBlokirInput();

        if (panelPhone == null) return;
        if (geserHpCoroutine != null) StopCoroutine(geserHpCoroutine);
        geserHpCoroutine = StartCoroutine(GeserHp(false));
    }

    IEnumerator GeserHp(bool masuk)
    {
        RectTransform rt = panelPhone.GetComponent<RectTransform>();
        if (rt == null)
        {
            panelPhone.SetActive(masuk);
            yield break;
        }

        Vector2 dari = rt.anchoredPosition;
        Vector2 ke = masuk ? posHpAsli : posHpAsli + new Vector2(jarakGeserHp, 0f);

        float t = 0f;
        float durasi = Mathf.Max(0.01f, durasiGeserHp);
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / durasi;
            float e = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            rt.anchoredPosition = Vector2.LerpUnclamped(dari, ke, e);
            yield return null;
        }

        rt.anchoredPosition = ke;
        geserHpCoroutine = null;

        if (!masuk)
        {
            panelPhone.SetActive(false);
            rt.anchoredPosition = posHpAsli;
        }
    }

    void TerapkanLockdownUI()
    {
        bool bebas = !IsLockdown;

        if (tombolTutupDariHome != null) tombolTutupDariHome.interactable = bebas;
        if (tombolHomeDariInbox != null) tombolHomeDariInbox.interactable = bebas;
        if (tombolBrowse != null) tombolBrowse.interactable = bebas;
        if (tombolCall != null) tombolCall.interactable = bebas;
        if (tombolChat != null) tombolChat.interactable = bebas;
        if (tombolKembaliDariHistory != null) tombolKembaliDariHistory.interactable = bebas;
    }

    // ==========================================
    // --- NAVIGASI HALAMAN ---
    // ==========================================

    List<GameObject> SemuaHalaman()
    {
        return new List<GameObject> { panelHome, panelInbox, panelChat, panelBrowse, panelBeritaDetail, panelPanggilan, panelHistory, panelKontakCall };
    }

    void SimpanPosisiAsliHalaman()
    {
        foreach (GameObject go in SemuaHalaman())
        {
            if (go == null) continue;
            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt != null) posAsliHalaman[go] = rt.anchoredPosition;
        }
    }

    void ResetPosisiHalaman(GameObject go)
    {
        if (go == null) return;
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt != null && posAsliHalaman.ContainsKey(go)) rt.anchoredPosition = posAsliHalaman[go];
    }

    // Hentikan transisi berjalan, hanya 'target' yang aktif, semua posisi dikembalikan
    void SelesaikanTransisi(GameObject target)
    {
        if (transisiCoroutine != null)
        {
            StopCoroutine(transisiCoroutine);
            transisiCoroutine = null;
        }

        foreach (GameObject go in SemuaHalaman())
        {
            if (go == null) continue;
            ResetPosisiHalaman(go);
            go.SetActive(go == target);
        }
    }

    /// <summary>
    /// Pindah halaman. arah: +1 = halaman baru masuk dari kanan, -1 = dari kiri, 0 = langsung.
    /// </summary>
    void PindahHalaman(GameObject tujuan, int arah)
    {
        if (tujuan == null) return;

        SelesaikanTransisi(halamanAktif); // rapikan transisi sebelumnya (jika ada)

        GameObject asal = halamanAktif;
        halamanAktif = tujuan;

        bool animasi = asal != null && asal != tujuan && arah != 0
                       && panelPhone != null && panelPhone.activeInHierarchy;

        if (!animasi)
        {
            SelesaikanTransisi(tujuan);
            return;
        }

        transisiCoroutine = StartCoroutine(TransisiHalaman(asal, tujuan, arah));
    }

    IEnumerator TransisiHalaman(GameObject asal, GameObject tujuan, int arah)
    {
        RectTransform rtPhone = panelPhone.GetComponent<RectTransform>();
        float lebar = rtPhone != null ? rtPhone.rect.width : 600f;
        if (lebar <= 1f) lebar = 600f;

        RectTransform rtAsal = asal.GetComponent<RectTransform>();
        RectTransform rtTujuan = tujuan.GetComponent<RectTransform>();
        Vector2 posAsal = posAsliHalaman.ContainsKey(asal) ? posAsliHalaman[asal] : Vector2.zero;
        Vector2 posTujuan = posAsliHalaman.ContainsKey(tujuan) ? posAsliHalaman[tujuan] : Vector2.zero;

        tujuan.SetActive(true);
        asal.SetActive(true);

        float t = 0f;
        float durasi = Mathf.Max(0.01f, durasiTransisiHalaman);
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / durasi;
            float e = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));

            if (rtTujuan != null) rtTujuan.anchoredPosition = posTujuan + new Vector2(arah * lebar * (1f - e), 0f);
            if (rtAsal != null) rtAsal.anchoredPosition = posAsal + new Vector2(-arah * lebar * e, 0f);
            yield return null;
        }

        transisiCoroutine = null;
        SelesaikanTransisi(tujuan);
    }

    // ==========================================
    // --- HOME & SWIPE ---
    // ==========================================

    public void TampilkanHome() { TampilkanHome(-1); }

    void TampilkanHome(int arah)
    {
        if (IsLockdown) return;

        HentikanKetik();
        kontakAktif = null;

        PindahHalaman(panelHome, arah);
        TerapkanLockdownUI();
    }

    void SaatSwipeHome(int dir)
    {
        if (IsLockdown || halamanAktif != panelHome) return;

        // Swipe KIRI di Home -> History masuk dari kanan
        if (dir == -1) TampilkanHistory(1);
    }

    public void TampilkanHistory() { TampilkanHistory(1); }

    void TampilkanHistory(int arah)
    {
        if (IsLockdown) return;

        if (panelHistory == null)
        {
            Debug.LogWarning("PhoneManager: Panel History belum diisi di Inspector.");
            return;
        }

        PindahHalaman(panelHistory, arah);
        TerapkanLockdownUI();
        RenderHistory();
    }

    void SaatSwipeHistory(int dir)
    {
        if (IsLockdown || halamanAktif != panelHistory) return;

        // Swipe KANAN di History -> kembali ke Home (masuk dari kiri)
        if (dir == 1) TampilkanHome(-1);
    }

    void SaatSwipeInbox(int dir)
    {
        if (IsLockdown || halamanAktif != panelInbox) return;

        // Swipe KANAN di Inbox -> kembali ke Home (masuk dari kiri)
        if (dir == 1) TampilkanHome(-1);
    }

    /// <summary>Buka HP langsung ke panel History (hubungkan ke tombol History di HUD).</summary>
    public void BukaHpKeHistory()
    {
        BukaHp();
        if (!IsLockdown) TampilkanHistory(0);
    }

    /// <summary>Dipanggil Manager_Chat saat history berubah. Hanya me-render jika panel sedang terbuka.</summary>
    public void SegarkanHistoryJikaTerbuka()
    {
        if (panelHistory != null && panelHistory.activeInHierarchy) RenderHistory();
    }

    void RenderHistory()
    {
        if (teksHistory == null) return;

        List<ContentChat> data = Manager_Chat.Instance != null ? Manager_Chat.Instance.historyChat : null;
        if (data == null || data.Count == 0)
        {
            teksHistory.text = "";
            return;
        }

        var sb = new System.Text.StringBuilder();

        int mulai = 0;
        if (maksPesanHistory > 0 && data.Count > maksPesanHistory)
            mulai = data.Count - maksPesanHistory;

        for (int i = mulai; i < data.Count; i++)
        {
            ContentChat chat = data[i];
            string nama = chat.isMC ? namaMC : namaNPC;
            sb.AppendLine("<b>" + nama + " :</b> " + chat.textChat);
            sb.AppendLine();
        }
        teksHistory.text = sb.ToString();

        if (scrollHistory != null && isActiveAndEnabled)
            StartCoroutine(ScrollHistoryKeBawah());
    }

    IEnumerator ScrollHistoryKeBawah()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        if (scrollHistory != null && scrollHistory.content != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollHistory.content);
        if (scrollHistory != null) scrollHistory.verticalNormalizedPosition = 0f;
    }

    // ==========================================
    // --- BROWSE / BERITA ---
    // ==========================================

    public void BukaBrowse()
    {
        if (IsLockdown) return;

        RenderBerita();
        PindahHalaman(panelBrowse, 1);
    }

    void RenderBerita()
    {
        if (containerBerita == null || prefabBeritaItem == null) return;

        foreach (Transform child in containerBerita)
            Destroy(child.gameObject);

        foreach (BeritaData b in daftarBerita)
        {
            if (b == null) continue;
            BeritaData data = b;
            PhoneNewsItem item = Instantiate(prefabBeritaItem, containerBerita);
            item.Setup(data.judul, data.gambar, () => BukaBerita(data));
        }
    }

    void BukaBerita(BeritaData data)
    {
        if (data == null) return;

        if (judulDetail != null) judulDetail.text = data.judul;
        if (isiDetail != null) isiDetail.text = data.isi;
        if (gambarDetail != null)
        {
            gambarDetail.sprite = data.gambar;
            gambarDetail.enabled = data.gambar != null;
        }
        if (scrollBerita != null) scrollBerita.verticalNormalizedPosition = 1f;

        PindahHalaman(panelBeritaDetail, 1);
    }

    // ==========================================
    // --- PANGGILAN (CALL) ---
    // ==========================================

    // Tombol Call di Home: tampilkan daftar kontak yang bisa ditelepon
    public void BukaPanggilan()
    {
        if (IsLockdown) return;

        // Panggilan masih berjalan (mis. kembali dari History): tampilkan layar panggilan lagi
        if (panggilanBerjalan)
        {
            PindahHalaman(panelPanggilan, 1);
            return;
        }

        if (panelKontakCall == null)
        {
            Debug.LogWarning("PhoneManager: Panel Kontak Call belum diisi di Inspector.");
            return;
        }

        RenderKontakCall();
        PindahHalaman(panelKontakCall, 1);
    }

    void RenderKontakCall()
    {
        if (containerKontakCall == null || prefabKontakItem == null)
        {
            Debug.LogWarning("PhoneManager: Container Kontak Call / Prefab Kontak Item belum diisi di Inspector.");
            return;
        }

        foreach (Transform child in containerKontakCall)
            Destroy(child.gameObject);

        foreach (var k in daftarKontak)
        {
            // Hanya kontak biasa (Azura Corp & Manager_Boo tidak bisa ditelepon)
            if (k == null || k.tipe != PhoneContactTipe.Biasa) continue;

            PhoneContactItem item = Instantiate(prefabKontakItem, containerKontakCall);
            PhoneContactSO kontakIni = k;
            item.Setup(k, teksPreviewCall, false, () => MulaiPanggilanKe(kontakIni));
        }
    }

    // Pemain menekan kontak: panggilan keluar dimulai
    void MulaiPanggilanKe(PhoneContactSO kontak)
    {
        if (IsLockdown || panggilanBerjalan || kontak == null) return;

        panggilanBerjalan = true;
        PindahHalaman(panelPanggilan, 1);

        panggilanCoroutine = StartCoroutine(AlurPanggilan(kontak));
    }

    IEnumerator AlurPanggilan(PhoneContactSO kontak)
    {
        // Pengaturan per kontak, atau default
        DataPanggilan data = dataPanggilan.Find(d => d != null && d.kontak == kontak);
        AudioClip audio = (data != null && data.audio != null) ? data.audio : audioPanggilan;
        float durasi = (data != null && data.durasiDetik > 0f) ? data.durasiDetik : durasiPanggilanDetik;

        // Reset tampilan
        if (logoTelepon != null)
        {
            logoTelepon.anchoredPosition = posLogoAsli;
            logoTelepon.localScale = Vector3.one;
        }
        if (panelPanggilanBerjalan != null) panelPanggilanBerjalan.SetActive(false);
        if (namaPanggilan != null) namaPanggilan.text = kontak.namaKontak;
        if (avatarPanggilan != null)
        {
            avatarPanggilan.sprite = kontak.avatar;
            avatarPanggilan.enabled = kontak.avatar != null;
        }
        if (teksStatusPanggilan != null) teksStatusPanggilan.text = "Memanggil...";
        if (teksTimerPanggilan != null) teksTimerPanggilan.text = FormatWaktu(0f);

        // Nada dering
        if (sfxDering != null)
        {
            callSource.clip = sfxDering;
            callSource.loop = true;
            callSource.Play();
        }

        // Berdering: logo telepon membesar-mengecil
        float waktu = 0f;
        while (waktu < jedaDering)
        {
            waktu += Time.unscaledDeltaTime;
            if (logoTelepon != null)
                logoTelepon.localScale = Vector3.one * (1f + Mathf.Sin(waktu * kecepatanDenyut) * besarDenyut);
            yield return null;
        }

        // Tersambung
        callSource.Stop();
        callSource.loop = false;
        if (logoTelepon != null) logoTelepon.localScale = Vector3.one;
        if (teksStatusPanggilan != null) teksStatusPanggilan.text = "Tersambung";

        // Tunggu sebentar, lalu logo naik
        yield return new WaitForSecondsRealtime(jedaSetelahAngkat);

        if (logoTelepon != null)
        {
            Vector2 dari = logoTelepon.anchoredPosition;
            Vector2 ke = dari + new Vector2(0f, jarakLogoNaik);
            float t = 0f;
            float durasiNaik = Mathf.Max(0.01f, durasiLogoNaik);
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / durasiNaik;
                logoTelepon.anchoredPosition = Vector2.LerpUnclamped(dari, ke, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t)));
                yield return null;
            }
            logoTelepon.anchoredPosition = ke;
        }

        // Panel panggilan berjalan: audio + timer
        if (panelPanggilanBerjalan != null) panelPanggilanBerjalan.SetActive(true);

        if (audio != null)
        {
            callSource.clip = audio;
            callSource.loop = false;
            callSource.Play();
        }

        float mulai = Time.unscaledTime;
        while (true)
        {
            float lewat = Mathf.Min(Time.unscaledTime - mulai, durasi);
            if (teksTimerPanggilan != null) teksTimerPanggilan.text = FormatWaktu(lewat);

            if (lewat >= durasi) break; // waktu habis -> otomatis selesai
            yield return null;
        }

        // Selesai otomatis
        callSource.Stop();
        if (teksStatusPanggilan != null) teksStatusPanggilan.text = "Panggilan berakhir";

        yield return new WaitForSecondsRealtime(jedaSetelahSelesai);

        panggilanCoroutine = null;
        AkhiriPanggilan(true);
    }

    string FormatWaktu(float detik)
    {
        int total = Mathf.Max(0, Mathf.FloorToInt(detik));
        int h = total / 3600;
        int m = (total % 3600) / 60;
        int s = total % 60;
        return string.Format("{0:00}:{1:00}:{2:00}", h, m, s);
    }

    /// <summary>Hentikan panggilan. kembaliKeHome = true: pindah ke Home jika sedang di halaman panggilan.</summary>
    void AkhiriPanggilan(bool kembaliKeHome)
    {
        bool adaPanggilan = panggilanBerjalan;

        if (panggilanCoroutine != null)
        {
            StopCoroutine(panggilanCoroutine);
            panggilanCoroutine = null;
        }

        if (callSource != null)
        {
            callSource.Stop();
            callSource.loop = false;
        }

        panggilanBerjalan = false;

        if (logoTelepon != null)
        {
            logoTelepon.localScale = Vector3.one;
            logoTelepon.anchoredPosition = posLogoAsli;
        }
        if (panelPanggilanBerjalan != null) panelPanggilanBerjalan.SetActive(false);

        if (adaPanggilan && kembaliKeHome && !IsLockdown && halamanAktif == panelPanggilan)
            TampilkanHome(-1);
    }

    // ==========================================
    // --- INBOX (HISTORY CHAT) ---
    // ==========================================

    public void TampilkanInbox() { TampilkanInboxArah(-1); }

    void TampilkanInboxArah(int arah)
    {
        HentikanKetik();
        kontakAktif = null;

        PindahHalaman(panelInbox, arah);
        TerapkanLockdownUI();
        RenderInbox();
    }

    // Kontak sistem hanya muncul sesuai workflow
    bool KontakTampil(PhoneContactSO k)
    {
        switch (k.tipe)
        {
            case PhoneContactTipe.AzuraCorp: return state[k].history.Count > 0;
            case PhoneContactTipe.ManagerBoo: return booAktif;
            default: return true;
        }
    }

    void RenderInbox()
    {
        if (containerKontak == null || prefabKontakItem == null)
        {
            Debug.LogWarning("PhoneManager: Container Kontak / Prefab Kontak Item belum diisi di Inspector.");
            return;
        }

        foreach (Transform child in containerKontak)
            Destroy(child.gameObject);

        foreach (var k in daftarKontak)
        {
            if (k == null || !KontakTampil(k)) continue;
            ContactState s = state[k];

            string preview = s.history.Count > 0 ? s.history[s.history.Count - 1].teks : "";
            preview = preview.Replace("\n", " ");
            if (preview.Length > 32) preview = preview.Substring(0, 32) + "...";

            PhoneContactItem item = Instantiate(prefabKontakItem, containerKontak);
            PhoneContactSO kontakIni = k;
            item.Setup(k, preview, s.unread, () => BukaChat(kontakIni, 1));

            // Saat lockdown hanya Manager_Boo yang bisa diklik, dan ditaruh paling atas
            if (IsLockdown)
            {
                bool boo = k.tipe == PhoneContactTipe.ManagerBoo;
                if (item.tombol != null) item.tombol.interactable = boo;
                if (boo) item.transform.SetAsFirstSibling();
            }
        }
    }

    // ==========================================
    // --- CHAT ---
    // ==========================================

    void BukaChat(PhoneContactSO kontak, int arah)
    {
        if (IsLockdown && kontak.tipe != PhoneContactTipe.ManagerBoo) return;

        kontakAktif = kontak;
        ContactState s = state[kontak];
        s.unread = false;

        PindahHalaman(panelChat, arah);

        namaHeader.text = kontak.namaKontak;
        if (avatarHeader != null)
        {
            avatarHeader.sprite = kontak.avatar;
            avatarHeader.enabled = kontak.avatar != null;
        }

        foreach (Transform child in containerPesan)
            Destroy(child.gameObject);

        foreach (ChatLine line in s.history)
            BuatBubble(line);

        SegarkanTombolBalas();
        StartCoroutine(RapikanChat(kontak));

        // Pemain membuka Manager_Boo saat lockdown
        if (IsLockdown && kontak.tipe == PhoneContactTipe.ManagerBoo && !booSudahDibuka)
        {
            booSudahDibuka = true;

            if (!tungguSemuaPesanSebelumEvent)
            {
                // Perilaku langsung: spam berhenti, event dipanggil sekarang
                if (spamCoroutine != null) { StopCoroutine(spamCoroutine); spamCoroutine = null; }
                PanggilEventBoo();
            }
            else if (semuaPesanBooTerkirim)
            {
                StartCoroutine(EventBooSetelahJeda());
            }
            // Jika pesan belum selesai, SpamManagerBoo yang memanggil event setelah pesan terakhir
        }
    }

    void BuatBubble(ChatLine line)
    {
        PhoneBubble bubble = Instantiate(prefabBubble, containerPesan);
        bubble.Setup(
            line.teks,
            line.dariAldo,
            lebarMaksBubble,
            line.dariAldo ? warnaBubbleAldo : warnaBubbleKontak
        );
    }

    // Hapus bubble paling lama selama isi chat melebihi area tampil, lalu scroll ke pesan terbaru
    IEnumerator RapikanChat(PhoneContactSO kontak)
    {
        yield return null; // tunggu bubble baru ter-layout
        if (!ChatSedangDibuka(kontak) || containerPesan == null) yield break;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(containerPesan);

        if (scrollChat != null)
        {
            RectTransform area = scrollChat.viewport != null
                ? scrollChat.viewport
                : scrollChat.GetComponent<RectTransform>();
            ContactState s = state[kontak];

            while (containerPesan.rect.height > area.rect.height)
            {
                Transform tertua = null;
                int aktif = 0;
                foreach (Transform child in containerPesan)
                {
                    if (!child.gameObject.activeSelf) continue;
                    aktif++;
                    if (tertua == null) tertua = child;
                }
                if (aktif <= 1) break; // sisakan minimal 1 bubble

                tertua.gameObject.SetActive(false); // langsung hilang dari layout
                Destroy(tertua.gameObject);

                if (hapusRiwayatSaatPenuh && s.history.Count > 0) s.history.RemoveAt(0);

                LayoutRebuilder.ForceRebuildLayoutImmediate(containerPesan);
            }

            scrollChat.verticalNormalizedPosition = 0f;
        }
    }

    bool ChatSedangDibuka(PhoneContactSO kontak)
    {
        return kontakAktif == kontak && panelChat != null && panelChat.activeInHierarchy;
    }

    void TambahPesan(PhoneContactSO kontak, string teks, bool dariAldo)
    {
        ContactState s = state[kontak];
        ChatLine line = new ChatLine { teks = teks, dariAldo = dariAldo };
        s.history.Add(line);

        if (ChatSedangDibuka(kontak))
        {
            BuatBubble(line);
            StartCoroutine(RapikanChat(kontak));
        }
        else if (!dariAldo)
        {
            s.unread = true;
        }

        if (!dariAldo)
        {
            if (sfxPesanMasuk != null) audioSource.PlayOneShot(sfxPesanMasuk);
            if (panelInbox != null && panelInbox.activeInHierarchy) RenderInbox();
        }
    }

    // Pesan dari kontak sistem: masuk riwayat + toast di HUD
    void KirimPesanSistem(PhoneContactSO kontak, string teks, bool kesalahan = false)
    {
        if (kontak == null || !state.ContainsKey(kontak))
        {
            Debug.LogWarning("Kontak sistem (Azura Corp / Manager_Boo) belum ada di Daftar Kontak PhoneManager.");
            return;
        }

        TambahPesan(kontak, teks, false);

        if (!ChatSedangDibuka(kontak) && PhoneNotifHUD.Instance != null)
        {
            if (kesalahan) PhoneNotifHUD.Instance.TampilkanKesalahan(kontak, teks);
            else PhoneNotifHUD.Instance.Tampilkan(kontak, teks);
        }
    }

    // ==========================================
    // --- STAGE & PESAN KONTAK BIASA ---
    // ==========================================

    void KirimStageSaatIni(PhoneContactSO kontak, bool pakaiDelay)
    {
        ContactState s = state[kontak];

        if (s.stageMulai || s.sedangKirim) return;               // stage sebelumnya belum selesai
        if (s.stageIndex >= kontak.stages.Count) return;         // tidak ada stage lagi

        s.baris = kontak.stages[s.stageIndex].percakapan ?? new List<PhoneLine>();
        s.lineIndex = 0;
        s.menungguBalasan = false;
        s.stageMulai = true;

        StartCoroutine(JalankanKontak(kontak, pakaiDelay));
    }

    // Kirim pesan kontak berurutan sampai bertemu baris Aldo (tunggu balasan) atau stage habis
    IEnumerator JalankanKontak(PhoneContactSO kontak, bool pakaiDelay)
    {
        ContactState s = state[kontak];
        s.sedangKirim = true;

        while (s.lineIndex < s.baris.Count)
        {
            PhoneLine line = s.baris[s.lineIndex];

            if (line.dariAldo)
            {
                s.menungguBalasan = true;
                break;
            }

            if (pakaiDelay)
                yield return new WaitForSeconds(jedaResponKontak);

            TambahPesan(kontak, line.teks, false);
            s.lineIndex++;
        }

        // Semua baris habis -> stage selesai
        if (!s.menungguBalasan && s.lineIndex >= s.baris.Count)
        {
            s.stageIndex++;
            s.stageMulai = false;
            s.baris = null;
        }

        s.sedangKirim = false;

        if (ChatSedangDibuka(kontak)) SegarkanTombolBalas();
    }

    // ==========================================
    // --- WORKFLOW: TRUST HABIS ---
    // ==========================================

    void PantauTrust()
    {
        EconomyManager eco = EconomyManager.Instance;
        if (eco == null)
            return;

        float trustSekarang = eco.trust;

        if (!sudahMenyimpanTrustAwal)
        {
            trustSebelumnya = trustSekarang;
            sudahMenyimpanTrustAwal = true;
            return;
        }

        if (trustSekarang == trustSebelumnya)
            return;

        // Jika Trust naik / di-reset, cukup perbarui baseline.
        if (trustSekarang > trustSebelumnya)
        {
            trustSebelumnya = trustSekarang;
            return;
        }

        // Trust berkurang = pemain melakukan kesalahan.
        // Tidak ada batas jumlah pesan Azura per stage.
        trustSebelumnya = trustSekarang;
        PicuKesalahan();

        // Saat Trust mencapai 0, jalankan workflow lockdown + Trust habis.
        if (trustSekarang <= 0 && !trustHabisDipicu)
        {
            PicuTrustHabis();
        }
    }

    // Trust habis -> kunci game -> peringatan Azura -> pesan Manager_Boo
    IEnumerator AlurTrustHabis()
    {
        MulaiLockdown();

        yield return new WaitForSecondsRealtime(jedaPeringatanAzura);
        KirimPesanSistem(kontakAzura, pesanTrustHabis);

        yield return new WaitForSecondsRealtime(jedaSebelumSpam);
        spamCoroutine = StartCoroutine(SpamManagerBoo());
    }

    void MulaiLockdown()
    {
        IsLockdown = true;
        PerbaruiBlokirInput();

        foreach (var go in matikanSaatLockdown)
            if (go != null) go.SetActive(false);

        // Panggilan dihentikan, tanpa pindah halaman (inbox dipaksa di bawah)
        AkhiriPanggilan(false);

        // Jika HP sedang terbuka, paksa kembali ke inbox (halaman lain tidak boleh dibuka)
        if (panelPhone != null && panelPhone.activeSelf) TampilkanInboxArah(0);

        TerapkanLockdownUI();
    }

    IEnumerator SpamManagerBoo()
    {
        if (kontakBoo == null || !state.ContainsKey(kontakBoo))
        {
            Debug.LogWarning("Kontak Manager_Boo belum ada di Daftar Kontak PhoneManager.");
            yield break;
        }

        booAktif = true;
        semuaPesanBooTerkirim = false;
        if (panelInbox != null && panelInbox.activeInHierarchy) RenderInbox();

        List<string> daftar = AmbilBarisBoo();
        if (daftar.Count == 0)
        {
            Debug.LogWarning("Manager_Boo belum punya baris pesan di Stages asset-nya.");
            semuaPesanBooTerkirim = true;
            spamCoroutine = null;
            yield break;
        }

        int total = kirimBarisSekali ? daftar.Count : maksPesanSpam;
        float interval = intervalSpamAwal;

        for (int i = 0; i < total; i++)
        {
            if (!IsLockdown) break;

            // Mode spam berulang berhenti begitu chat dibuka
            if (!kirimBarisSekali && booSudahDibuka) break;

            // Jika ada error di sini, tampil di Console dan loop tetap jalan
            try { KirimPesanSistem(kontakBoo, daftar[i % daftar.Count]); }
            catch (System.Exception e) { Debug.LogException(e); }

            if (i < total - 1)
            {
                yield return new WaitForSecondsRealtime(interval);
                interval = Mathf.Max(intervalSpamMin, interval * 0.9f);
            }
        }

        semuaPesanBooTerkirim = true;
        spamCoroutine = null;

        // Chat sudah dibuka sebelum pesan terakhir -> event dipanggil sekarang
        if (tungguSemuaPesanSebelumEvent && booSudahDibuka)
        {
            yield return new WaitForSecondsRealtime(jedaSetelahPesanTerakhir);
            PanggilEventBoo();
        }
    }

    IEnumerator EventBooSetelahJeda()
    {
        yield return new WaitForSecondsRealtime(jedaSetelahPesanTerakhir);
        PanggilEventBoo();
    }

    void PanggilEventBoo()
    {
        if (eventBooDipanggil) return;
        eventBooDipanggil = true;

        OnManagerBooDibuka?.Invoke();
    }

    // Teks pesan diambil dari stages asset Manager_Boo
    List<string> AmbilBarisBoo()
    {
        List<string> hasil = new List<string>();

        foreach (PhoneStage st in kontakBoo.stages)
        {
            if (st == null || st.percakapan == null) continue;
            foreach (PhoneLine l in st.percakapan)
                if (l != null && !l.dariAldo && !string.IsNullOrEmpty(l.teks))
                    hasil.Add(l.teks);
        }

        return hasil;
    }

    // ==========================================
    // --- BALAS PESAN (TAHAN TOMBOL -> TEKS DIKETIK OTOMATIS) ---
    // ==========================================

    // Baris Aldo yang sedang menunggu dikirim (null jika tidak ada)
    PhoneLine BarisBalasan(PhoneContactSO kontak)
    {
        if (kontak == null) return null;
        ContactState s = state[kontak];

        if (!s.menungguBalasan || s.sedangKirim) return null;
        if (s.baris == null || s.lineIndex >= s.baris.Count) return null;

        return s.baris[s.lineIndex];
    }

    void SegarkanTombolBalas()
    {
        bool adaBalasan = BarisBalasan(kontakAktif) != null;

        if (tombolBalas != null)
        {
            tombolBalas.gameObject.SetActive(adaBalasan);
            tombolBalas.SetInteractable(adaBalasan && !sedangMengetik);
        }
        if (teksInput != null && !sedangMengetik)
            teksInput.text = "";
    }

    void MulaiBalas()
    {
        if (sedangMengetik) return;

        PhoneLine line = BarisBalasan(kontakAktif);
        if (line == null) return;

        ketikCoroutine = StartCoroutine(ProsesBalas(kontakAktif, line));
    }

    IEnumerator ProsesBalas(PhoneContactSO kontak, PhoneLine line)
    {
        sedangMengetik = true;
        tombolBalas.SetInteractable(false);
        teksInput.text = "";

        // Teks diketik otomatis
        string isi = line.teks ?? "";
        foreach (char c in isi)
        {
            teksInput.text += c;
            yield return new WaitForSeconds(kecepatanKetik);
        }

        yield return new WaitForSeconds(0.3f);

        // Kirim: pindah dari kotak input ke bubble kanan
        teksInput.text = "";
        if (sfxKirim != null) audioSource.PlayOneShot(sfxKirim);
        TambahPesan(kontak, isi, true);

        // Lanjut ke baris berikutnya
        ContactState s = state[kontak];
        s.lineIndex++;
        s.menungguBalasan = false;

        sedangMengetik = false;
        ketikCoroutine = null;

        // Kontak merespon (berjalan terus walau HP ditutup). Berhenti lagi di baris Aldo berikutnya.
        StartCoroutine(JalankanKontak(kontak, true));

        SegarkanTombolBalas();
    }
}