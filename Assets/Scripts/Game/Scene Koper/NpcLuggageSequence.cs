using System.Collections;
using UnityEngine;
using Schema.data;

public class NpcLuggageSequence : MonoBehaviour
{
    [Header("Referensi Objek")]
    public Transform npcKarakter;
    public Transform koper;
    public AnimasiKoperSimple scriptKoper;
    public ProceduralFace scriptWajah;

    [Header("Titik Target")]
    public Transform titikMejaKoper;
    public Transform titikKeluarKanan;
    public Transform titikKeluarKiri;

    [Header("Pengaturan Waktu")]
    public float kecepatanMasuk = 2.5f;
    public float kecepatanGeserKoper = 3f;
    public float jedaSebelumBuka = 0.5f;
    public float kecepatanKeluar = 3f;

    private Vector3 skalaAsliNpc;
    private Vector3 skalaAsliKoper;
    private Vector3 posisiAwalNpc;

    void Start()
    {
        skalaAsliNpc = npcKarakter.localScale;
        skalaAsliKoper = koper.localScale;
        posisiAwalNpc = npcKarakter.position;

        koper.gameObject.SetActive(false);
        npcKarakter.localScale = skalaAsliNpc * 0.1f;

        StartCoroutine(JalankanAdegan());
    }

    public IEnumerator JalankanAdegan()
    {
        float time = 0;
        Vector3 skalaKecilNpc = npcKarakter.localScale;

        while (time < 1)
        {
            time += Time.deltaTime * kecepatanMasuk;
            npcKarakter.localScale = Vector3.Lerp(skalaKecilNpc, skalaAsliNpc, time);
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        koper.position = npcKarakter.position;
        koper.localScale = skalaAsliKoper * 0.1f;
        koper.gameObject.SetActive(true);

        time = 0;
        Vector3 posisiAwalKoper = koper.position;
        Vector3 skalaKecilKoper = koper.localScale;

        while (time < 1)
        {
            time += Time.deltaTime * kecepatanGeserKoper;
            koper.position = Vector3.Lerp(posisiAwalKoper, titikMejaKoper.position, time);
            koper.localScale = Vector3.Lerp(skalaKecilKoper, skalaAsliKoper, time);
            yield return null;
        }

        yield return new WaitForSeconds(jedaSebelumBuka);
        if (scriptKoper != null) scriptKoper.ToggleKoper();
    }

    public IEnumerator NPCKeluar(bool bawaBarangTerlarang, LuggageManager manager)
    {
        // 1. Tutup koper
        if (scriptKoper != null) scriptKoper.ToggleKoper();

        yield return new WaitForSeconds(jedaSebelumBuka);

        // 2. Koper mundur
        float time = 0;
        Vector3 posisiMeja = koper.position;
        Vector3 skalaBesarKoper = koper.localScale;
        Vector3 skalaKecilKoper = skalaAsliKoper * 0.1f;

        while (time < 1)
        {
            time += Time.deltaTime * kecepatanGeserKoper;
            koper.position = Vector3.Lerp(posisiMeja, npcKarakter.position, time);
            koper.localScale = Vector3.Lerp(skalaBesarKoper, skalaKecilKoper, time);
            yield return null;
        }

        koper.gameObject.SetActive(false);

        // 3. NPC Jalan ke samping
        Vector3 posisiSekarang = npcKarakter.position;
        Vector3 posisiTujuan = bawaBarangTerlarang ? titikKeluarKiri.position : titikKeluarKanan.position;

        if (Mathf.Abs(posisiTujuan.x - posisiSekarang.x) < 1f)
        {
            posisiTujuan.x = posisiSekarang.x + (bawaBarangTerlarang ? -15f : 15f);
        }

        time = 0;
        while (time < 1)
        {
            time += Time.deltaTime * kecepatanKeluar;
            npcKarakter.position = Vector3.Lerp(posisiSekarang, posisiTujuan, time);
            yield return null;
        }

        // 4. Reset NPC ke tengah
        npcKarakter.position = posisiAwalNpc;
        npcKarakter.localScale = skalaAsliNpc * 0.1f;

        // --- 5. KEMBALIKAN FULL RANDOM (BENTUK + WARNA) ---
        // Kode ini akan otomatis mencari komponen ProceduralFace di tubuh NPC yang berjalan!
        ProceduralFace wajahNPC = npcKarakter.GetComponentInChildren<ProceduralFace>();

        // Fallback jika tidak ketemu di tubuhnya, baru pakai yang ada di Inspector
        if (wajahNPC == null) wajahNPC = scriptWajah;

        if (wajahNPC != null)
        {
            Gender genderAcak = (Random.value > 0.5f) ? Gender.Man : Gender.Woman;
            wajahNPC.SetGender(genderAcak);

            // Perintah ini akan merandom bentuk rambut, mata, hidung, mulut, SEKALIGUS Warnanya!
            wajahNPC.ProceduralGenerateAll();
        }

        // 6. Isi Koper Baru
        if (manager != null) manager.GenerateBarangNPC();

        // 7. Mulai Adegan NPC Baru Masuk
        StartCoroutine(JalankanAdegan());
    }
}