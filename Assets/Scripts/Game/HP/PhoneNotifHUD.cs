using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhoneNotifHUD : MonoBehaviour
{
    public static PhoneNotifHUD Instance;

    [Header("Referensi")]
    public CanvasGroup grup;              // CanvasGroup di panel toast
    public RectTransform panel;           // panel toast
    public Image avatar;                  // opsional
    public TextMeshProUGUI teksNama;
    public TextMeshProUGUI teksIsi;
    public Button tombolNotif;            // opsional: klik toast = buka HP ke chat pengirim

    [Header("Animasi")]
    public float durasiTampil = 3f;
    public float durasiAnim = 0.25f;
    public float jarakSlide = 80f;
    public int maksKarakterPreview = 60;

    [Header("Audio (opsional)")]
    public AudioClip sfxNotif;

    [Header("Notifikasi Trust Berkurang")]
    public AudioClip sfxKesalahan;

    private PhoneContactSO kontakSaatIni;
    private Coroutine rutin;
    private Vector2 posisiAkhir;
    private AudioSource audioSource;

    void Awake()
    {
        Instance = this;

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;

        if (panel != null)
            posisiAkhir = panel.anchoredPosition;

        SembunyikanSegera();

        if (tombolNotif != null)
            tombolNotif.onClick.AddListener(KlikNotif);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Tampilkan(PhoneContactSO kontak, string isi)
    {
        if (kontak == null || grup == null || panel == null)
            return;

        kontakSaatIni = kontak;

        if (teksNama != null)
            teksNama.text = kontak.namaKontak;

        if (teksIsi != null)
            teksIsi.text = Potong(isi);

        if (avatar != null)
        {
            avatar.sprite = kontak.avatar;
            avatar.enabled = kontak.avatar != null;
        }

        if (sfxNotif != null && audioSource != null)
            audioSource.PlayOneShot(sfxNotif);

        MulaiAnimasi();
    }

    public void TampilkanKesalahan(PhoneContactSO kontak, string isi)
    {
        if (kontak == null || grup == null || panel == null)
            return;

        kontakSaatIni = kontak;

        if (teksNama != null)
            teksNama.text = kontak.namaKontak;

        if (teksIsi != null)
            teksIsi.text = Potong(isi);

        if (avatar != null)
        {
            avatar.sprite = kontak.avatar;
            avatar.enabled = kontak.avatar != null;
        }

        if (sfxKesalahan != null && audioSource != null)
            audioSource.PlayOneShot(sfxKesalahan);

        MulaiAnimasi();
    }

    void MulaiAnimasi()
    {
        if (rutin != null)
            StopCoroutine(rutin);

        rutin = StartCoroutine(Animasi());
    }

    public void SembunyikanSegera()
    {
        if (rutin != null)
        {
            StopCoroutine(rutin);
            rutin = null;
        }

        if (grup != null)
        {
            grup.alpha = 0f;
            grup.blocksRaycasts = false;
        }

        if (panel != null)
        {
            panel.anchoredPosition =
                posisiAkhir + Vector2.up * jarakSlide;
        }
    }

    void KlikNotif()
    {
        if (kontakSaatIni == null ||
            PhoneManager.Instance == null)
            return;

        PhoneManager.Instance.BukaHpKeKontak(kontakSaatIni);

        SembunyikanSegera();
    }

    IEnumerator Animasi()
    {
        grup.blocksRaycasts = true;

        yield return Geser(grup.alpha, 1f);

        yield return new WaitForSecondsRealtime(durasiTampil);

        yield return Geser(1f, 0f);

        grup.blocksRaycasts = false;
        rutin = null;
    }

    IEnumerator Geser(float dari, float ke)
    {
        float t = 0f;

        while (t < durasiAnim)
        {
            t += Time.unscaledDeltaTime;

            float k = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(t / durasiAnim)
            );

            Terapkan(Mathf.Lerp(dari, ke, k));

            yield return null;
        }

        Terapkan(ke);
    }

    void Terapkan(float v)
    {
        if (grup != null)
            grup.alpha = v;

        if (panel != null)
        {
            panel.anchoredPosition =
                posisiAkhir +
                Vector2.up * jarakSlide * (1f - v);
        }
    }

    string Potong(string s)
    {
        if (string.IsNullOrEmpty(s))
            return "";

        s = s.Replace("\n", " ");

        return s.Length > maksKarakterPreview
            ? s.Substring(0, maksKarakterPreview) + "..."
            : s;
    }
}