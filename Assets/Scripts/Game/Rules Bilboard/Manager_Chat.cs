using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public enum PosisiVertikalProgress
    {
        DiBawahBubble,
        TengahBubble,
        DiAtasBubble
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

    [Header("Kontrol Dialog")]
    [Tooltip("Tekan tombol ini untuk menampilkan pesan berikutnya")]
    public KeyCode tombolLanjut = KeyCode.Space;
    [Tooltip("Aktifkan jika dialog biasa juga ingin dilanjutkan dengan klik kiri mouse")]
    public bool izinkanKlikMouse = true;
    [Tooltip("Durasi Space harus ditahan setelah bubble MC muncul untuk melanjutkan dialog")]
    [Min(0.1f)] public float durasiHoldSpaceMC = 0.6f;
    [Tooltip("Opsional: Image progress bar dengan Image Type = Filled. Fill Amount akan bergerak saat Space ditahan.")]
    public Image progressHoldSpaceMC;
    [Tooltip("Opsional: GameObject progress bar yang diaktifkan hanya saat menahan Space.")]
    public GameObject objekProgressHoldMC;

    [Header("Posisi Progress Bar MC")]
    [Tooltip("Canvas tempat progress bar berada. Kosong = dicari otomatis dari parent progress bar.")]
    public Canvas canvasProgressMC;
    [Tooltip("Kamera yang merender bubble. Kosong = Camera.main.")]
    public Camera kameraBubbleMC;
    [Tooltip("Sisi kiri progress bar selalu menempel di ujung kiri bubble MC. Ini mengatur posisi vertikalnya.")]
    public PosisiVertikalProgress posisiVertikal = PosisiVertikalProgress.DiBawahBubble;
    [Tooltip("Jarak (pixel layar) antara bubble dan progress bar. Dipakai untuk Di Bawah / Di Atas.")]
    public float jarakProgressDariBubble = 10f;
    [Tooltip("Geser tambahan (pixel layar). X negatif = lebih ke kiri, Y positif = lebih ke atas.")]
    public Vector2 geserTambahanPixel = Vector2.zero;
    [Tooltip("ON: lebar progress bar mengikuti lebar bubble MC. " +
             "Jika bar punya anak Image, buat anak itu anchor stretch agar ikut melebar.")]
    public bool samakanLebarDenganBubble = false;

    [Header("Waktu Percakapan")]
    public float delaySebelumMulai = 0.5f;

    [Header("Animasi Hilang (naik sedikit + memudar)")]
    [Tooltip("Jarak naik secara visual saat hilang (unit dunia)")]
    public float jarakNaikSaatHilang = 0.6f;
    public float durasiHilang = 0.4f;
    [Tooltip("Jeda antar bubble saat semuanya hilang satu per satu")]
    public float jedaAntarHilang = 0.25f;

    [Header("Daftar Percakapan")]
    public ChatTopic currentTopic;

    [Header("=== DATA HISTORY CHAT ===")]
    [Tooltip("Dibaca oleh PhoneManager untuk panel History di HP")]
    public List<ContentChat> historyChat = new List<ContentChat>();

    private readonly List<BubbleAktif> daftarBubble = new List<BubbleAktif>();
    private readonly List<GameObject> daftarSedangHilang = new List<GameObject>();
    private Coroutine prosesChatAktif;
    private AudioSource audioSourceChat;

    // Bubble MC yang sedang menunggu hold Space (untuk posisi progress bar)
    private Transform bubbleMCAktif;
    private Renderer[] rendererBubbleMC;
    private RectTransform rectProgressMC;

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

        // Panel History di HP ikut dikosongkan jika sedang terbuka
        if (PhoneManager.Instance != null)
            PhoneManager.Instance.SegarkanHistoryJikaTerbuka();
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

        if (progressHoldSpaceMC != null)
            progressHoldSpaceMC.fillAmount = 0f;
        if (objekProgressHoldMC != null)
            objekProgressHoldMC.SetActive(false);

        foreach (BubbleAktif b in daftarBubble)
            HancurkanSeketika(b.go);
        daftarBubble.Clear();
        bubbleMCAktif = null;
        rendererBubbleMC = null;

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
            prosesChatAktif = null;
            yield break;
        }

        if (!currentTopic.ShouldSpeak())
        {
            prosesChatAktif = null;
            yield break;
        }

        List<ContentChat> chats = currentTopic.GetRandomMessage();
        if (chats == null || chats.Count == 0)
        {
            Debug.LogWarning("List messages di ChatTopic kosong!");
            prosesChatAktif = null;
            yield break;
        }

        for (int i = 0; i < chats.Count; i++)
        {
            ContentChat chat = chats[i];
            if (chat == null) continue;

            // NPC pertama muncul otomatis. NPC berikutnya menunggu input biasa.
            // Bubble MC ditampilkan terlebih dahulu, lalu pemain menahan Space
            // untuk menyelesaikan progress sebelum dialog berlanjut.
            if (!chat.isMC && i > 0)
                yield return StartCoroutine(TungguInputNPC());

            // Simpan ke history saat pesan ditampilkan agar tetap muncul di HP.
            historyChat.Add(chat);
            if (PhoneManager.Instance != null)
                PhoneManager.Instance.SegarkanHistoryJikaTerbuka();

            TampilkanBubble(chat);

            // MC sudah terlihat; hold Space sekarang mengisi progress dan
            // menahan alur sebelum pesan berikutnya ditampilkan.
            if (chat.isMC)
                yield return StartCoroutine(TungguHoldSpaceMC());
        }

        // Semua baris selesai: bubble hilang satu per satu seperti sebelumnya.
        while (daftarBubble.Count > 0)
        {
            HapusBubbleTeratas(geserTumpukanSetelahHapus);
            yield return new WaitForSeconds(jedaAntarHilang);
        }

        prosesChatAktif = null;
    }

    IEnumerator TungguInputNPC()
    {
        // Pastikan input yang dipakai untuk memulai percakapan tidak langsung
        // melompati pesan berikutnya.
        yield return new WaitUntil(() => !Input.GetKey(tombolLanjut));

        while (true)
        {
            if (Input.GetKeyDown(tombolLanjut) ||
                (izinkanKlikMouse && Input.GetMouseButtonDown(0)))
                yield break;

            yield return null;
        }
    }

    IEnumerator TungguHoldSpaceMC()
    {
        if (objekProgressHoldMC != null)
            objekProgressHoldMC.SetActive(true);

        if (progressHoldSpaceMC != null)
            progressHoldSpaceMC.fillAmount = 0f;

        // Wajib melepas Space terlebih dahulu agar tombol yang dipakai
        // untuk memulai interaksi tidak langsung dihitung sebagai hold.
        yield return new WaitUntil(() => !Input.GetKey(tombolLanjut));

        while (true)
        {
            yield return new WaitUntil(() => Input.GetKeyDown(tombolLanjut));

            float waktuHold = 0f;
            while (Input.GetKey(tombolLanjut) && waktuHold < durasiHoldSpaceMC)
            {
                waktuHold += Time.deltaTime;
                if (progressHoldSpaceMC != null)
                    progressHoldSpaceMC.fillAmount = Mathf.Clamp01(waktuHold / durasiHoldSpaceMC);
                yield return null;
            }

            if (waktuHold >= durasiHoldSpaceMC)
                break;

            // Space dilepas terlalu cepat: progress kembali ke nol dan pemain
            // harus menahan lagi dari awal.
            if (progressHoldSpaceMC != null)
                progressHoldSpaceMC.fillAmount = 0f;
        }

        if (progressHoldSpaceMC != null)
            progressHoldSpaceMC.fillAmount = 0f;
        if (objekProgressHoldMC != null)
            objekProgressHoldMC.SetActive(false);
    }

    // ==========================================
    // --- POSISI PROGRESS BAR MC ---
    // ==========================================

    void LateUpdate()
    {
        UpdatePosisiProgressMC();
    }

    void UpdatePosisiProgressMC()
    {
        if (bubbleMCAktif == null || objekProgressHoldMC == null)
            return;

        if (!objekProgressHoldMC.activeSelf)
            return;

        if (rectProgressMC == null)
            rectProgressMC = objekProgressHoldMC.GetComponent<RectTransform>();
        if (rectProgressMC == null)
            return;

        RectTransform parentRect = rectProgressMC.parent as RectTransform;
        if (parentRect == null)
            return;

        // Kamera UI (null untuk Screen Space - Overlay)
        Canvas canvas = canvasProgressMC != null
            ? canvasProgressMC
            : rectProgressMC.GetComponentInParent<Canvas>();
        if (canvas == null)
            return;
        canvas = canvas.rootCanvas;

        Camera kameraUI = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        // Kamera yang merender bubble
        Camera kameraDunia = kameraBubbleMC != null ? kameraBubbleMC : Camera.main;
        if (kameraDunia == null)
            return;

        // Kotak bubble MC di layar (mengikuti ukuran bubble yang sebenarnya)
        if (!HitungKotakBubbleDiLayar(kameraDunia,
                out float kiri, out float kanan, out float bawah, out float atas))
            return;

        // Titik acuan vertikal + titik pada bar yang harus menempel di sana
        // ay = 1 -> sisi ATAS bar menempel, 0.5 -> tengah bar, 0 -> sisi BAWAH bar
        float yAcuan;
        float ay;
        switch (posisiVertikal)
        {
            case PosisiVertikalProgress.TengahBubble:
                yAcuan = (atas + bawah) * 0.5f;
                ay = 0.5f;
                break;
            case PosisiVertikalProgress.DiAtasBubble:
                yAcuan = atas + jarakProgressDariBubble;
                ay = 0f;
                break;
            default: // DiBawahBubble
                yAcuan = bawah - jarakProgressDariBubble;
                ay = 1f;
                break;
        }

        Vector2 titikKiriLayar = new Vector2(kiri, yAcuan) + geserTambahanPixel;
        Vector2 titikKananLayar = new Vector2(kanan, yAcuan) + geserTambahanPixel;

        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(
                parentRect, titikKiriLayar, kameraUI, out Vector3 titikKiriDunia))
            return;

        // Lebar progress bar = lebar bubble (opsional)
        if (samakanLebarDenganBubble &&
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                parentRect, titikKananLayar, kameraUI, out Vector3 titikKananDunia))
        {
            float skalaX = Mathf.Abs(rectProgressMC.lossyScale.x);
            if (skalaX > 0.0001f)
            {
                float lebarLokal = Mathf.Abs(titikKananDunia.x - titikKiriDunia.x) / skalaX;
                if (Mathf.Abs(rectProgressMC.rect.width - lebarLokal) > 0.01f)
                    rectProgressMC.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, lebarLokal);
            }
        }

        // Kompensasi pivot: sisi kiri bar (x = 0) dan titik vertikal (ay) tepat di titik acuan,
        // apa pun anchor dan pivot bar-nya.
        Vector2 ukuranDunia = new Vector2(
            rectProgressMC.rect.width * Mathf.Abs(rectProgressMC.lossyScale.x),
            rectProgressMC.rect.height * Mathf.Abs(rectProgressMC.lossyScale.y));
        Vector2 pivot = rectProgressMC.pivot;

        rectProgressMC.position = titikKiriDunia + new Vector3(
            pivot.x * ukuranDunia.x,
            (pivot.y - ay) * ukuranDunia.y,
            0f);
    }

    // Gabungan semua renderer bubble MC -> kotak di koordinat layar (pixel)
    bool HitungKotakBubbleDiLayar(Camera kamera,
        out float kiri, out float kanan, out float bawah, out float atas)
    {
        kiri = bawah = float.MaxValue;
        kanan = atas = float.MinValue;
        bool ada = false;

        if (rendererBubbleMC == null)
            return false;

        for (int r = 0; r < rendererBubbleMC.Length; r++)
        {
            Renderer rend = rendererBubbleMC[r];
            if (rend == null || !rend.enabled || !rend.gameObject.activeInHierarchy)
                continue;

            // SpriteRenderer tanpa sprite tidak punya ukuran, lewati
            SpriteRenderer sr = rend as SpriteRenderer;
            if (sr != null && sr.sprite == null)
                continue;

            Bounds b = rend.bounds;
            Vector3 c = b.center;
            Vector3 e = b.extents;

            for (int i = 0; i < 8; i++)
            {
                Vector3 sudut = c + new Vector3(
                    (i & 1) == 0 ? -e.x : e.x,
                    (i & 2) == 0 ? -e.y : e.y,
                    (i & 4) == 0 ? -e.z : e.z);

                Vector3 p = kamera.WorldToScreenPoint(sudut);
                if (p.z <= 0f) continue;

                if (p.x < kiri) kiri = p.x;
                if (p.x > kanan) kanan = p.x;
                if (p.y < bawah) bawah = p.y;
                if (p.y > atas) atas = p.y;
                ada = true;
            }
        }

        return ada;
    }

    // Bubble MC = semua SpriteRenderer (badan bubble). Jika tidak ada, pakai semua Renderer.
    static Renderer[] AmbilRendererBubble(GameObject go)
    {
        SpriteRenderer[] sprites = go.GetComponentsInChildren<SpriteRenderer>(true);
        if (sprites.Length > 0)
            return sprites;

        return go.GetComponentsInChildren<Renderer>(true);
    }

    // ==========================================
    // --- BUBBLE ---
    // ==========================================

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

        if (chat.isMC)
        {
            bubbleMCAktif = data.go.transform;
            rendererBubbleMC = AmbilRendererBubble(data.go);
        }

        data.go.transform.DOScale(originalScale, 0.3f)
            .SetEase(Ease.OutBack)
            .SetLink(data.go);

        // Teks langsung muncul (tanpa typing)
        TextMeshPro teks3D = data.go.GetComponentInChildren<TextMeshPro>();
        if (teks3D != null)
        {
            teks3D.text = chat.textChat;
            teks3D.ForceMeshUpdate(); // ukuran teks langsung benar agar bubble & progress bar pas
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
}