using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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

    // ---------- Data simpanan log Azura (file JSON) ----------
    [System.Serializable]
    public class BarisTersimpan
    {
        public string teks;
        public bool dariAldo;
    }

    [System.Serializable]
    public class LogTersimpan
    {
        public List<BarisTersimpan> baris = new List<BarisTersimpan>();
        public bool unread;
    }

    private readonly Dictionary<PhoneContactSO, ContactState> state =
        new Dictionary<PhoneContactSO, ContactState>();

    [Header("Data Kontak")]
    [Tooltip("Masukkan SEMUA kontak, termasuk Azura Corp dan Manager_Boo (atur Tipe di asset-nya)")]
    public List<PhoneContactSO> daftarKontak = new List<PhoneContactSO>();
    [Tooltip("Kirim pesan stage pertama semua kontak biasa saat game mulai")]
    public bool kirimStageAwalOtomatis = true;

    [Header("Panel")]
    public GameObject panelPhone;
    public GameObject panelInbox;
    public GameObject panelChat;

    [Header("Tombol")]
    [Tooltip("Tab 'Chat' di bagian atas layar")]
    public Button tombolBukaHp;
    [Tooltip("Tombol 'Back work' di Inbox")]
    public Button tombolBackWork;
    [Tooltip("Tombol '<' di header chat")]
    public Button tombolKembaliKeInbox;
    [Tooltip("Titik merah di tab Chat jika ada pesan belum dibaca (opsional)")]
    public GameObject badgeNotif;

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

    [Header("Balas Pesan")]
    public PhoneHoldButton tombolBalas;
    public TextMeshProUGUI teksInput;
    public string placeholderInput = "";
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
    [Tooltip("Dipakai jika asset Manager_Boo tidak punya baris pesan di stages")]
    public string[] pesanSpamCadangan = { "Aldo!!", "Kamu di mana?!", "Balas sekarang!", "Ini serius!" };
    [Tooltip("Centang = semua baris Boo dikirim berurutan SEKALI lalu berhenti. Hilangkan centang = diulang terus (spam) sampai chat dibuka.")]
    public bool kirimBarisSekali = true;
    [Tooltip("Centang = event 'On Manager Boo Dibuka' dipanggil setelah chat dibuka DAN semua pesan sudah masuk. Hilangkan = langsung saat chat dibuka (spam berhenti).")]
    public bool tungguSemuaPesanSebelumEvent = true;
    [Tooltip("Jeda membaca pesan terakhir sebelum event dipanggil")]
    public float jedaSetelahPesanTerakhir = 1.5f;
    [Tooltip("Object yang dimatikan saat lockdown (mis. panel Accept/Reject, dokumen). JANGAN masukkan tombol HP.")]
    public List<GameObject> matikanSaatLockdown = new List<GameObject>();
    public UnityEvent onLockdownMulai;
    [Tooltip("Dipanggil saat pemain membuka chat Manager_Boo (pasang fade di sini nanti)")]
    public UnityEvent onManagerBooDibuka;

    [Header("Audio (opsional)")]
    public AudioClip sfxPesanMasuk;
    public AudioClip sfxKirim;

    private PhoneContactSO kontakAktif;
    private Coroutine ketikCoroutine;
    private bool sedangMengetik;
    private AudioSource audioSource;

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

    // Violation
    [System.Serializable]
    public class ViolationData
    {
    public string id;
    public string nama;
    public int penaltyTrust;
    public bool sudahTerjadi;
    }

    [Header("Violation System")]
    public List<ViolationData> daftarViolation = new List<ViolationData>();

    private string PathLogAzura => Path.Combine(Application.persistentDataPath, "phone_azura_log.json");

    // ==========================================
    // --- LIFECYCLE ---
    // ==========================================

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        foreach (var k in daftarKontak)
        {
            if (k == null) continue;
            if (!state.ContainsKey(k)) state[k] = new ContactState();

            if (k.tipe == PhoneContactTipe.AzuraCorp && kontakAzura == null) kontakAzura = k;
            if (k.tipe == PhoneContactTipe.ManagerBoo && kontakBoo == null) kontakBoo = k;
        }

        //MuatLogAzura();
    }

    void Start()
    {
        if (tombolBukaHp != null) tombolBukaHp.onClick.AddListener(BukaHp);
        if (tombolBackWork != null) tombolBackWork.onClick.AddListener(TutupHp);
        if (tombolKembaliKeInbox != null) tombolKembaliKeInbox.onClick.AddListener(TampilkanInbox);
        if (tombolBalas != null) tombolBalas.OnHoldSelesai += MulaiBalas;

        if (panelPhone != null) panelPhone.SetActive(false);
        EconomyManager eco = EconomyManager.Instance;
        if (eco != null)
        {
            trustSebelumnya = eco.trust;
            sudahMenyimpanTrustAwal = true;
        }

        if (kirimStageAwalOtomatis)
        {
            foreach (var k in daftarKontak)
                if (k != null && k.tipe == PhoneContactTipe.Biasa) KirimStageSaatIni(k, false);
        }

        UpdateBadge();
    }

    void Update()
    {
        PantauTrust();
    }

    void OnDestroy()
    {
        if (tombolBalas != null) tombolBalas.OnHoldSelesai -= MulaiBalas;
        if (Instance == this)
        {
            IsPhoneOpen = false;
            IsLockdown = false;
        }
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
    /// Tidak ada batas jumlah pesan per stage.
    /// </summary>
    public void PicuKesalahan()
{
    if (kontakAzura == null) return;

    if (violationTerakhir != null)
    {
        string pesan =
            "PELANGGARAN TERDETEKSI:\n" +
            violationTerakhir.nama +
            "\nTrust -" +
            violationTerakhir.penaltyTrust;

        KirimPesanSistem(kontakAzura, pesan);

        violationTerakhir = null;
    }
    else
    {
        KirimPesanSistem(kontakAzura, pesanKesalahan);
    }
}

    public void TambahViolation(string id)
{
    ViolationData violation = daftarViolation.Find(v => v.id == id);

    if (violation == null)
    {
        Debug.LogWarning("Violation dengan ID '" + id + "' tidak ditemukan.");
        return;
    }

    violation.sudahTerjadi = true;
    violationTerakhir = violation;

    Debug.Log(
        "Violation: " + violation.nama
    );
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

        try { if (File.Exists(PathLogAzura)) File.Delete(PathLogAzura); }
        catch (System.Exception e) { Debug.LogWarning("Gagal hapus log Azura: " + e.Message); }

        UpdateBadge();
    }

    /// <summary>Buka HP langsung ke chat kontak tertentu (dipakai saat toast HUD diklik).</summary>
    public void BukaHpKeKontak(PhoneContactSO kontak)
    {
        BukaHp();
        if (kontak != null && state.ContainsKey(kontak) && KontakTampil(kontak))
            BukaChat(kontak);
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
        EconomyManager eco = EconomyManager.Instance;
        if (eco != null)
        {
            trustSebelumnya = eco.trust;
            sudahMenyimpanTrustAwal = true;
        }

        foreach (var go in matikanSaatLockdown)
            if (go != null) go.SetActive(true);

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
    // --- BUKA / TUTUP HP ---
    // ==========================================

    public void BukaHp()
    {
        if (panelPhone == null) return;
        panelPhone.SetActive(true);
        IsPhoneOpen = true;

        if (PhoneNotifHUD.Instance != null) PhoneNotifHUD.Instance.SembunyikanSegera();

        TampilkanInbox();
    }

    public void TutupHp()
    {
        if (IsLockdown) return; // saat lockdown HP tidak bisa ditutup

        if (ketikCoroutine != null)
        {
            StopCoroutine(ketikCoroutine);
            ketikCoroutine = null;
        }
        sedangMengetik = false;
        if (teksInput != null) teksInput.text = placeholderInput;

        if (panelPhone != null) panelPhone.SetActive(false);
        kontakAktif = null;
        IsPhoneOpen = false;
        UpdateBadge();
    }

    void TerapkanLockdownUI()
    {
        if (tombolBackWork != null) tombolBackWork.interactable = !IsLockdown;
    }

    // ==========================================
    // --- INBOX ---
    // ==========================================

    public void TampilkanInbox()
    {
        if (ketikCoroutine != null)
        {
            StopCoroutine(ketikCoroutine);
            ketikCoroutine = null;
        }
        sedangMengetik = false;
        kontakAktif = null;

        panelInbox.SetActive(true);
        panelChat.SetActive(false);
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
            item.Setup(k, preview, s.unread, () => BukaChat(kontakIni));

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

    void BukaChat(PhoneContactSO kontak)
    {
        if (IsLockdown && kontak.tipe != PhoneContactTipe.ManagerBoo) return;

        kontakAktif = kontak;
        ContactState s = state[kontak];
        s.unread = false;
        UpdateBadge();

        panelInbox.SetActive(false);
        panelChat.SetActive(true);

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
        StartCoroutine(ScrollKeBawah());

        //if (kontak.tipe == PhoneContactTipe.AzuraCorp) SimpanLogAzura(); // simpan status "sudah dibaca"

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

    IEnumerator ScrollKeBawah()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(containerPesan);
        if (scrollChat != null) scrollChat.verticalNormalizedPosition = 0f;
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
            StartCoroutine(ScrollKeBawah());
        }
        else if (!dariAldo)
        {
            s.unread = true;
        }

        if (!dariAldo)
        {
            if (sfxPesanMasuk != null) audioSource.PlayOneShot(sfxPesanMasuk);
            UpdateBadge();
            if (panelInbox != null && panelInbox.activeInHierarchy) RenderInbox();
        }

        //if (kontak.tipe == PhoneContactTipe.AzuraCorp) SimpanLogAzura();
    }

    // Pesan dari kontak sistem: masuk riwayat + toast di HUD
    void KirimPesanSistem(PhoneContactSO kontak, string teks)
    {
        if (kontak == null || !state.ContainsKey(kontak))
        {
            Debug.LogWarning("Kontak sistem (Azura Corp / Manager_Boo) belum ada di Daftar Kontak PhoneManager.");
            return;
        }

        TambahPesan(kontak, teks, false);

        if (!ChatSedangDibuka(kontak) && PhoneNotifHUD.Instance != null)
            PhoneNotifHUD.Instance.Tampilkan(kontak, teks);
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

        foreach (var go in matikanSaatLockdown)
            if (go != null) go.SetActive(false);

        onLockdownMulai?.Invoke();

        // Jika HP sedang terbuka, paksa kembali ke inbox (chat lain tidak boleh dibuka)
        if (panelPhone != null && panelPhone.activeSelf) TampilkanInbox();

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

        onManagerBooDibuka?.Invoke();
        OnManagerBooDibuka?.Invoke();
    }

    // Teks pesan diambil dari stages asset Manager_Boo; kalau kosong pakai cadangan
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

        if (hasil.Count == 0)
        {
            foreach (string c in pesanSpamCadangan)
                if (!string.IsNullOrEmpty(c)) hasil.Add(c);
        }

        if (hasil.Count == 0) hasil.Add("...");
        return hasil;
    }

    // ==========================================
    // --- SIMPAN / MUAT LOG AZURA ---
    // ==========================================

    void SimpanLogAzura()
    {
        if (kontakAzura == null || !state.ContainsKey(kontakAzura)) return;

        ContactState s = state[kontakAzura];
        LogTersimpan data = new LogTersimpan { unread = s.unread };
        foreach (ChatLine l in s.history)
            data.baris.Add(new BarisTersimpan { teks = l.teks, dariAldo = l.dariAldo });

        try { File.WriteAllText(PathLogAzura, JsonUtility.ToJson(data)); }
        catch (System.Exception e) { Debug.LogWarning("Gagal simpan log Azura: " + e.Message); }
    }

    void MuatLogAzura()
    {
        if (kontakAzura == null || !File.Exists(PathLogAzura)) return;

        try
        {
            LogTersimpan data = JsonUtility.FromJson<LogTersimpan>(File.ReadAllText(PathLogAzura));
            if (data == null) return;

            ContactState s = state[kontakAzura];
            s.history.Clear();
            foreach (BarisTersimpan b in data.baris)
                s.history.Add(new ChatLine { teks = b.teks, dariAldo = b.dariAldo });
            s.unread = data.unread;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Gagal muat log Azura: " + e.Message);
        }
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
            teksInput.text = placeholderInput;
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
        teksInput.text = placeholderInput;
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

    void UpdateBadge()
    {
        if (badgeNotif == null) return;

        bool adaUnread = false;
        foreach (var kv in state)
        {
            if (kv.Value.unread) { adaUnread = true; break; }
        }
        badgeNotif.SetActive(adaUnread);
    }
}