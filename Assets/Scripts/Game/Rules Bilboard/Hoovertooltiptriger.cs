using System.Collections;
using UnityEngine;

// Pasang di objek yang ingin punya tooltip (paspor, tiket, dll). Berdiri sendiri, tidak mengubah HoverEfek.
// Alur: mouse di objek -> tunggu 'delayTampil' detik -> tooltip muncul di samping objek.
// Tooltip hilang kalau: mouse keluar, objek diklik/di-inspect, HP terbuka, game di-pause,
// atau objek (ber-DeskViewTrigger) sedang di Desk View dan tooltip-nya dimatikan.
[RequireComponent(typeof(Collider2D))]
public class HoverTooltipTrigger : MonoBehaviour
{
    [Header("Isi Tooltip")]
    [Tooltip("Teks tooltip. Jika 'Pakai Kondisi Meja' aktif, ini dipakai saat objek SUDAH di meja")]
    public string teksTooltip = "Double Left Click to inspect";
    [Tooltip("Opsional")]
    public Sprite iconTooltip;

    [Header("Kondisi Meja (opsional, untuk objek yang punya Draggable)")]
    [Tooltip("Aktifkan agar teks berbeda tergantung objek sudah di meja atau belum (membaca Draggable.isOnDesk)")]
    public bool pakaiKondisiMeja = false;
    [Tooltip("Teks saat objek BELUM di meja")]
    public string teksBelumDiMeja = "Drag to table";
    [Tooltip("Opsional")]
    public Sprite iconBelumDiMeja;

    [Header("Waktu & Posisi")]
    [Tooltip("Lama mouse harus berada di objek sebelum tooltip muncul (detik)")]
    public float delayTampil = 2f;
    [Tooltip("Sisi objek tempat tooltip muncul")]
    public SisiTooltip sisi = SisiTooltip.Kanan;
    [Tooltip("Jika dicentang dan sisi pilihan tidak muat di layar, tooltip pindah ke sisi seberang. Jika tidak, sisi pilihan selalu dipakai (hanya digeser masuk layar)")]
    public bool pindahSisiOtomatis = false;
    [Tooltip("Jarak tooltip dari tepi objek (unit canvas)")]
    public float jarakDariObjek = 15f;

    [Header("Debug")]
    [Tooltip("Tampilkan di Console langkah-langkah tooltip dan alasan jika tertahan.")]
    public bool logDebug = true;

    private string alasanTerakhir;

    private Collider2D col;
    private InspectableObject inspect;
    private Draggable draggable;
    private DeskViewTrigger deskTrigger;

    private bool hovering;
    private bool suppressed;    // true setelah objek diklik, sampai mouse keluar
    private bool tampil;
    private Coroutine tunggu;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        inspect = GetComponent<InspectableObject>();
        deskTrigger = GetComponent<DeskViewTrigger>();
        draggable = GetComponent<Draggable>();
        if (draggable == null) draggable = GetComponentInParent<Draggable>();
    }

    void OnDisable()
    {
        hovering = false;
        suppressed = false;
        Batalkan();
    }

    void OnMouseEnter()
    {
        hovering = true;
        suppressed = false;
        Log("mouse masuk, hitung mundur " + delayTampil + " detik");
    }

    void Log(string pesan)
    {
        if (logDebug) Debug.Log("[Tooltip] " + name + ": " + pesan, this);
    }

    void OnMouseExit()
    {
        hovering = false;
        suppressed = false;
        Batalkan();
    }

    // Klik / drag = aksi -> tooltip hilang dan tidak muncul lagi sampai mouse keluar dan masuk lagi
    void OnMouseDown()
    {
        suppressed = true;
        Batalkan();
    }

    void Update()
    {
        if (!hovering) return;

        // Sedang inspect / HP terbuka / pause / di Desk View (jika dimatikan) -> sembunyikan.
        // Begitu kondisi ini hilang dan mouse masih di objek, hitung mundur dimulai lagi.
        if (Terhalang())
        {
            Batalkan();
            return;
        }

        if (suppressed) return;

        // Mulai hitung 2 detik (juga dipakai lagi setelah inspect selesai)
        if (tunggu == null && !tampil)
            tunggu = StartCoroutine(TungguLaluTampil());
    }

    // Objek ber-DeskViewTrigger yang sedang di Desk View dan tooltip-nya dimatikan.
    // Objek tanpa DeskViewTrigger (mis. Rules & Guide di scene Camera 2) tidak terpengaruh.
    bool DilarangDiDesk =>
        deskTrigger != null && deskTrigger.SedangDiDesk && !deskTrigger.tooltipAktifDiDeskView;

    bool Terhalang()
    {
        string alasan = null;

        if (inspect != null && inspect.GetIsInspect())
            alasan = "panel buku sedang terbuka (BookPanelController.IsPanelOpen = true)";
        else if (PhoneManager.IsPhoneOpen)
            alasan = "HP terbuka (PhoneManager.IsPhoneOpen = true)";
        else if (Time.timeScale == 0f)
            alasan = "Time.timeScale = 0 (game di-pause)";
        else if (DilarangDiDesk)
            alasan = "objek sedang di Desk View dan tooltip-nya dimatikan";

        // Log hanya saat alasan berubah, supaya Console tidak penuh
        if (hovering && alasan != alasanTerakhir && alasan != null)
            Log("TERTAHAN: " + alasan);

        alasanTerakhir = alasan;
        return alasan != null;
    }

    IEnumerator TungguLaluTampil()
    {
        yield return new WaitForSecondsRealtime(delayTampil);
        tunggu = null;

        if (!hovering || suppressed || Terhalang()) yield break;

        if (TooltipManager.Instance == null)
        {
            Debug.LogWarning("[HoverTooltipTrigger] TooltipManager tidak ada di scene.");
            yield break;
        }

        // Pilih teks sesuai kondisi: belum di meja -> "Drag to table", sudah di meja -> teks utama
        bool belumDiMeja = pakaiKondisiMeja && draggable != null && !draggable.isOnDesk;
        string isi = belumDiMeja ? teksBelumDiMeja : teksTooltip;
        Sprite ikon = belumDiMeja ? iconBelumDiMeja : iconTooltip;

        Log("memanggil TooltipManager.Show(\"" + isi + "\")");
        TooltipManager.Instance.Show(this, col, isi, ikon, sisi, jarakDariObjek, pindahSisiOtomatis);
        tampil = true;
    }

    void Batalkan()
    {
        if (tunggu != null)
        {
            StopCoroutine(tunggu);
            tunggu = null;
        }

        if (tampil)
        {
            tampil = false;
            if (TooltipManager.Instance != null)
                TooltipManager.Instance.Hide(this);
        }
    }
}