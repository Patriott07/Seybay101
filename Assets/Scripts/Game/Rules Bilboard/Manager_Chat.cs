using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

[System.Serializable]
public class ContentChat
{
    [TextArea(2, 4)]
    public string textChat;
    public AudioClip clip;
    public bool isMC = true;
}

public class Manager_Chat : MonoBehaviour
{
    public static Manager_Chat Instance;

    // Data satu bubble yang sedang tampil di HUD
    class BubbleAktif
    {
        public GameObject go;
        public float y;         // posisi Y relatif terhadap objek ini (turun = negatif)
        public float offsetX;   // kiri (NPC) / kanan (MC)
        public bool isMC;
    }

    [Header("Prefab Popup HUD")]
    public GameObject prefabBubbleMC;
    public GameObject prefabBubbleNPC;

    [Header("Rata Kiri/Kanan HUD (NPC kiri, MC kanan)")]
    public float geserKiriNPC = -0.5f;
    public float geserKananMC = 0.5f;

    [Header("Pengaturan Jarak (Spacing)")]
    public float jarakBedaKarakter = 1.5f;
    public float jarakSamaKarakter = 0.8f;

    [Header("Batas Bubble di HUD")]
    [Tooltip("Jika lebih dari ini, bubble paling atas hilang. 0 = tanpa batas")]
    public int batasMaksimalPesan = 4;
    [Tooltip("Sisa bubble ikut bergeser naik menutup ruang kosong setelah yang teratas hilang")]
    public bool geserTumpukanSetelahHapus = true;
    public float durasiGeser = 0.25f;

    [Header("Waktu Percakapan")]
    public float delaySebelumMulai = 0.5f;
    [Tooltip("Jeda sebelum pesan berikutnya muncul")]
    public float jedaAntarPesan = 1.5f;
    [Tooltip("Jeda sebelum pesan berikutnya muncul minimal selama audio clip-nya")]
    public bool tungguAudioSelesai = true;
    [Tooltip("Setelah pesan TERAKHIR muncul, semua bubble bertahan selama ini lalu hilang satu per satu")]
    public float durasiTampilPopup = 3.0f;

    [Header("Animasi Hilang (naik sedikit + memudar)")]
    [Tooltip("Jarak naik secara visual saat hilang (unit dunia)")]
    public float jarakNaikSaatHilang = 0.6f;
    public float durasiHilang = 0.4f;
    [Tooltip("Jeda antar bubble saat semuanya hilang satu per satu")]
    public float jedaAntarHilang = 0.25f;

    [Header("Daftar Percakapan")]
    public ChatTopic currentTopic;

    [Header("=== SYSTEM HISTORY CHAT ===")]
    public List<ContentChat> historyChat = new List<ContentChat>();

    [Header("UI Panel History")]
    public GameObject panelHistory;
    [Tooltip("Satu TextMeshProUGUI di dalam panel history")]
    public TextMeshProUGUI teksHistory;
    [Tooltip("Batas pesan yang ditampilkan di kertas (0 = tanpa batas)")]
    public int maksPesanHistory = 8;
    public string namaMC = "Aldo";
    public string namaNPC = "NPC";

    private readonly List<BubbleAktif> daftarBubble = new List<BubbleAktif>();
    private readonly List<GameObject> daftarSedangHilang = new List<GameObject>();
    private Coroutine prosesChatAktif;
    private AudioSource audioSourceChat;


    // History of chat

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        audioSourceChat = GetComponent<AudioSource>();
        if (audioSourceChat == null)
            audioSourceChat = gameObject.AddComponent<AudioSource>();
        audioSourceChat.playOnAwake = false;
    }

    // ==========================================
    // --- ACC / REJECT -> CLEAR HISTORY ---
    // ==========================================

    // Hubungkan ke tombol Accept
    public void OnAccept() => ClearHistory();

    // Hubungkan ke tombol Reject
    public void OnReject() => ClearHistory();

    public void ClearHistory()
    {
        historyChat.Clear();
        BersihkanChat();

        if (teksHistory != null)
            teksHistory.text = "";

        if (panelHistory != null)
            panelHistory.SetActive(false);
    }

    // Membersihkan popup HUD seketika (history tidak ikut terhapus)
    public void BersihkanChat()
    {
        if (prosesChatAktif != null)
        {
            StopCoroutine(prosesChatAktif);
            prosesChatAktif = null;
        }

        if (audioSourceChat != null && audioSourceChat.isPlaying)
            audioSourceChat.Stop();

        foreach (BubbleAktif b in daftarBubble)
            HancurkanSeketika(b.go);
        daftarBubble.Clear();

        foreach (GameObject g in daftarSedangHilang)
            HancurkanSeketika(g);
        daftarSedangHilang.Clear();
    }

    static void HancurkanSeketika(GameObject go)
    {
        if (go == null) return;
        go.transform.DOKill();
        Destroy(go);
    }

    // ==========================================
    // --- MULAI CHAT BARU (NPC DATANG) ---
    // ==========================================

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
            Debug.LogWarning("ChatTopic belum di-assign di Inspector!");
            yield break;
        }

        if (!currentTopic.ShouldSpeak())
            yield break;

        List<ContentChat> chats = currentTopic.GetRandomMessage();
        if (chats == null)
        {
            Debug.LogWarning("List messages di ChatTopic kosong!");
            yield break;
        }

        for (int i = 0; i < chats.Count; i++)
        {
            ContentChat chat = chats[i];

            // 1. Simpan ke history
            historyChat.Add(chat);
            if (panelHistory != null && panelHistory.activeSelf)
                RenderPanelHistory();

            // 2. Tampilkan bubble + audio
            TampilkanBubble(chat);

            // 3. Tunggu sebelum pesan berikutnya (atau sebelum bubble mulai hilang)
            bool terakhir = (i == chats.Count - 1);
            float tunggu = terakhir ? durasiTampilPopup : jedaAntarPesan;
            if (tungguAudioSelesai && chat.clip != null)
                tunggu = Mathf.Max(tunggu, chat.clip.length);

            yield return new WaitForSeconds(tunggu);
        }

        // 4. Semua chat selesai -> bubble hilang satu per satu dari yang paling atas
        while (daftarBubble.Count > 0)
        {
            HapusBubbleTeratas(false);
            yield return new WaitForSeconds(jedaAntarHilang);
        }

        prosesChatAktif = null;
    }

    // === [AWAL PERUBAHAN 5: PARAMETER TAMBAHAN UNTUK AUDIOCLIP] ===

    Vector3 PosisiBubble(BubbleAktif b)
    {
        return transform.position + new Vector3(b.offsetX, b.y, 0f);
    }

    void TampilkanBubble(ContentChat chat)
    {
        GameObject prefab = chat.isMC ? prefabBubbleMC : prefabBubbleNPC;
        if (prefab == null) return;

        // Melewati batas -> bubble paling atas hilang dulu
        if (batasMaksimalPesan > 0)
        {
            while (daftarBubble.Count >= batasMaksimalPesan)
                HapusBubbleTeratas(geserTumpukanSetelahHapus);
        }

        // Hitung posisi: di bawah bubble terakhir
        float y = 0f;
        if (daftarBubble.Count > 0)
        {
            BubbleAktif terakhir = daftarBubble[daftarBubble.Count - 1];
            y = terakhir.y - (chat.isMC == terakhir.isMC ? jarakSamaKarakter : jarakBedaKarakter);
        }

        var data = new BubbleAktif
        {
            y = y,
            isMC = chat.isMC,
            offsetX = chat.isMC ? geserKananMC : geserKiriNPC
        };

        Vector3 originalScale = prefab.transform.localScale;
        data.go = Instantiate(prefab, PosisiBubble(data), Quaternion.identity, transform);
        data.go.transform.localScale = Vector3.zero;
        daftarBubble.Add(data);

        data.go.transform.DOScale(originalScale, 0.3f)
            .SetEase(Ease.OutBack)
            .SetLink(data.go);

        // Teks langsung muncul (tanpa typing)
        TextMeshPro teks3D = data.go.GetComponentInChildren<TextMeshPro>();
        if (teks3D != null)
        {
            teks3D.text = chat.textChat;
        }
        else
        {
            TextMeshProUGUI teksUI = data.go.GetComponentInChildren<TextMeshProUGUI>();
            if (teksUI != null) teksUI.text = chat.textChat;
        }

        // Audio
        if (chat.clip != null && audioSourceChat != null)
        {
            audioSourceChat.Stop();
            audioSourceChat.clip = chat.clip;
            audioSourceChat.Play();
        }
    }

    // Menghilangkan bubble paling atas (yang paling lama)
    void HapusBubbleTeratas(bool geserSisa)
    {
        if (daftarBubble.Count == 0) return;

        BubbleAktif atas = daftarBubble[0];
        daftarBubble.RemoveAt(0);

        float delta = 0f;
        if (daftarBubble.Count > 0)
            delta = atas.y - daftarBubble[0].y;

        AnimasiHilang(atas.go);

        // Sisa bubble naik menutup ruang kosong
        if (geserSisa && delta > 0f)
        {
            foreach (BubbleAktif b in daftarBubble)
            {
                b.y += delta;
                if (b.go != null)
                    b.go.transform.DOMove(PosisiBubble(b), durasiGeser)
                        .SetEase(Ease.OutQuad)
                        .SetLink(b.go);
            }
        }
    }

    // Naik sedikit sambil memudar, lalu dihapus (naik hanya efek visual)
    void AnimasiHilang(GameObject go)
    {
        if (go == null) return;

        daftarSedangHilang.Add(go);
        go.transform.DOKill();

        SpriteRenderer[] sprites = go.GetComponentsInChildren<SpriteRenderer>(true);
        TMP_Text[] teksList = go.GetComponentsInChildren<TMP_Text>(true);

        float[] alphaSprite = new float[sprites.Length];
        for (int i = 0; i < sprites.Length; i++) alphaSprite[i] = sprites[i].color.a;

        float[] alphaTeks = new float[teksList.Length];
        for (int i = 0; i < teksList.Length; i++) alphaTeks[i] = teksList[i].alpha;

        Vector3 target = go.transform.position + Vector3.up * jarakNaikSaatHilang;

        Sequence seq = DOTween.Sequence();
        seq.Join(go.transform.DOMove(target, durasiHilang).SetEase(Ease.OutQuad));
        seq.Join(DOVirtual.Float(1f, 0f, durasiHilang, nilai =>
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] == null) continue;
                Color c = sprites[i].color;
                c.a = alphaSprite[i] * nilai;
                sprites[i].color = c;
            }
            for (int i = 0; i < teksList.Length; i++)
            {
                if (teksList[i] == null) continue;
                teksList[i].alpha = alphaTeks[i] * nilai;
            }
        }).SetEase(Ease.InQuad));
        seq.SetLink(go);
        seq.OnComplete(() =>
        {
            daftarSedangHilang.Remove(go);
            if (go != null) Destroy(go);
        });
    }

    // ==========================================
    // --- PANEL HISTORY ---
    // ==========================================

    public void TogglePanelHistory()
    {
        if (panelHistory == null) return;

        bool aktif = !panelHistory.activeSelf;
        panelHistory.SetActive(aktif);
        if (aktif) RenderPanelHistory();
    }

    public void BukaPanelHistory()
    {
        if (panelHistory == null) return;
        panelHistory.SetActive(true);
        RenderPanelHistory();
    }

    public void TutupPanelHistory()
    {
        if (panelHistory != null)
            panelHistory.SetActive(false);
    }

    void RenderPanelHistory()
    {
        if (teksHistory == null) return;

        var sb = new System.Text.StringBuilder();

        // Tanpa scroll: tampilkan hanya N pesan terakhir agar muat di kertas (0 = semua)
        int mulai = 0;
        if (maksPesanHistory > 0 && historyChat.Count > maksPesanHistory)
            mulai = historyChat.Count - maksPesanHistory;

        for (int i = mulai; i < historyChat.Count; i++)
        {
            ContentChat chat = historyChat[i];
            string nama = chat.isMC ? namaMC : namaNPC;
            sb.AppendLine("<b>" + nama + " :</b> " + chat.textChat);
            sb.AppendLine();
        }
        teksHistory.text = sb.ToString();
    }
}
