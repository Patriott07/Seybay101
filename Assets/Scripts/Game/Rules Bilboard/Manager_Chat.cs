using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

// === [AWAL PERUBAHAN 1: CLASS CONTENT CHAT DARI TEMANMU] ===
[System.Serializable]
public class ContentChat
{
    [TextArea(2, 4)]
    public string textChat;
    public AudioClip clip;
    public bool isMC = true;
}

// === [AKHIR PERUBAHAN 1] ===

public class Manager_Chat : MonoBehaviour
{
    public static Manager_Chat Instance;

    [Header("Pengaturan Posisi & Objek")]
    public GameObject prefabBubbleMC;
    public GameObject prefabBubbleNPC;

    [Header("Pengaturan Chat (Sensor Batas)")]
    public int batasMaksimalPesan = 4;

    [Header("Pengaturan Jarak (Spacing)")]
    public float jarakBedaKarakter = 1.5f;
    public float jarakSamaKarakter = 0.8f;

    [Header("Pengaturan Animasi & Waktu")]
    public float delaySebelumMulai = 0.5f; // Dipercepat karena dipanggil NpcEntrance
    public float jedaAntarPesan = 1f;
    public float kecepatanKetik = 0.05f;

    [Header("Pengaturan Rata Kiri/Kanan")]
    public float geserKiriNPC = -0.5f;
    public float geserKananMC = 0.5f;

    // === [AWAL PERUBAHAN 2: MENGGUNAKAN CHAT TOPIC SCRIPTABLEOBJECT] ===
    [Header("Daftar Percakapan")]
    public ChatTopic currentTopic;

    // === [AKHIR PERUBAHAN 2] ===

    private List<GameObject> daftarPesanAktif = new List<GameObject>();
    private float posisiYSelanjutnya = 0f;
    private bool? apakahMCSebelumnya = null;
    private Coroutine prosesChatAktif;

    // Tambahan AudioSource untuk memutar clip chat
    private AudioSource audioSourceChat;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Otomatis membuat komponen AudioSource di belakang layar
        // audioSourceChat = gameObject.AddComponent<AudioSource>();
        // audioSourceChat.playOnAwake = false;
    }

    void Start()
    {
        audioSourceChat = GetComponent<AudioSource>();
        // Tetap dibiarkan KOSONG agar NpcEntranceManager yang memanggil MulaiChatBaru()
    }

    public void BersihkanChat()
    {
        if (prosesChatAktif != null)
            StopCoroutine(prosesChatAktif);

        // === [AWAL PERUBAHAN 3: HENTIKAN SUARA SAAT DIBERSIHKAN] ===
        if (audioSourceChat != null && audioSourceChat.isPlaying)
        {
            audioSourceChat.Stop();
        }
        // === [AKHIR PERUBAHAN 3] ===

        foreach (GameObject pesanLama in daftarPesanAktif)
        {
            if (pesanLama != null)
                Destroy(pesanLama);
        }
        daftarPesanAktif.Clear();

        posisiYSelanjutnya = 0f;
        apakahMCSebelumnya = null;
    }

    public void MulaiChatBaru()
    {
        BersihkanChat();
        prosesChatAktif = StartCoroutine(MulaiPercakapan());
    }

    IEnumerator MulaiPercakapan()
    {
        yield return new WaitForSeconds(delaySebelumMulai);

        if (currentTopic == null)
        {
            Debug.LogWarning("ChatTopic belum di-assignment di Inspector!");
            yield break;
        }

        if (!currentTopic.ShouldSpeak())
        {
            yield break;
        }

        List<ContentChat> chats = currentTopic.GetRandomMessage();
        if (chats == null)
        {
            Debug.LogWarning("List messages di ChatTopic kosong!");
            yield break;
        }

        foreach (ContentChat chat in chats)
        {
            bool isMC = chat.isMC;
            float offsetX = isMC ? geserKiriNPC : geserKananMC;
            GameObject prefab = isMC ? prefabBubbleMC : prefabBubbleNPC;

            yield return StartCoroutine(
                MunculkanPesan(prefab, chat.textChat, chat.clip, offsetX, isMC)
            );
        }
    }

    // === [AWAL PERUBAHAN 5: PARAMETER TAMBAHAN UNTUK AUDIOCLIP] ===
    IEnumerator MunculkanPesan(
        GameObject prefab,
        string teks,
        AudioClip suara,
        float offsetX,
        bool isMC
    )
    {
        if (daftarPesanAktif.Count >= batasMaksimalPesan)
        {
            foreach (GameObject pesanLama in daftarPesanAktif)
            {
                if (pesanLama != null)
                    Destroy(pesanLama);
            }
            daftarPesanAktif.Clear();
            posisiYSelanjutnya = 0f;
            apakahMCSebelumnya = null;
        }

        if (apakahMCSebelumnya != null)
        {
            if (isMC == apakahMCSebelumnya)
                posisiYSelanjutnya -= jarakSamaKarakter;
            else
                posisiYSelanjutnya -= jarakBedaKarakter;
        }

        Vector3 posisiMuncul = transform.position + new Vector3(offsetX, posisiYSelanjutnya, 0);

        Vector3 originalScale = prefab.transform.localScale;
        GameObject pesanBaru = Instantiate(prefab, posisiMuncul, Quaternion.identity, transform);
        pesanBaru.transform.localScale = Vector3.zero;
        daftarPesanAktif.Add(pesanBaru);

        pesanBaru.transform.DOScale(originalScale, 0.4f).SetEase(Ease.OutBack);

        // Putar suara jika AudioClip-nya diisi di Inspector
        if (suara != null && audioSourceChat != null)
        {
            audioSourceChat.Stop(); // Hentikan suara sebelumnya (jika menumpuk)
            audioSourceChat.clip = suara;
            audioSourceChat.Play();
        }

        TextMeshPro teks3D = pesanBaru.GetComponentInChildren<TextMeshPro>();
        if (teks3D != null)
        {
            teks3D.text = "";
            yield return new WaitForSeconds(0.2f);

            foreach (char huruf in teks.ToCharArray())
            {
                teks3D.text += huruf;
                yield return new WaitForSeconds(kecepatanKetik);
            }
        }
        apakahMCSebelumnya = isMC;
    }
    // === [AKHIR PERUBAHAN 5] ===
}
