using System.Collections;
using UnityEngine;

public class NpcEntranceManager : MonoBehaviour
{
    [Header("Objek yang Digerakkan")]
    public Transform posisiNpc;
    public Transform dokumenKertas;
    public Transform objekTiket;

    [Header("Pengaturan Animasi")]
    public float kecepatanMasuk = 2.5f;
    public float waktuTungguSodor = 1f;
    public float kecepatanSodorKertas = 4f;
    public float kecepatanPergi = 3f;
    public float jarakPergi = 15f;

    private Vector3 targetDokumen;
    private Vector3 targetTiket;
    private Vector3 skalaAsliNpc;

    void Start()
    {
        targetDokumen = dokumenKertas.position;
        targetTiket = objekTiket.position;
        skalaAsliNpc = posisiNpc.localScale;

        // 1. NPC mulai dengan ukuran sangat kecil (jauh di belakang)
        Vector3 skalaKecil = skalaAsliNpc * 0.1f;
        posisiNpc.localScale = skalaKecil;

        // --- BARIS BARU (SOLUSI) ---
        // 2. MATIKAN (Sembunyikan) kertas dan tiket agar meja benar-benar kosong
        dokumenKertas.gameObject.SetActive(false);
        objekTiket.gameObject.SetActive(false);

        StartCoroutine(AdeganNpcMasuk(skalaKecil));
    }

    IEnumerator AdeganNpcMasuk(Vector3 skalaKecil)
    {
        // --- FASE A: NPC MUNCUL DARI BELAKANG (ZOOM IN) ---
        float time = 0;
        while (time < 1)
        {
            time += Time.deltaTime * kecepatanMasuk;
            posisiNpc.localScale = Vector3.Lerp(skalaKecil, skalaAsliNpc, time);
            yield return null;
        }

        // NPC diam menatap pemain sejenak
        yield return new WaitForSeconds(waktuTungguSodor);

        // --- FASE B: MENYODORKAN KERTAS KE MEJA ---

        // 1. Posisikan kertas di titik tengah NPC
        dokumenKertas.position = new Vector3(posisiNpc.position.x, posisiNpc.position.y, targetDokumen.z);
        objekTiket.position = new Vector3(posisiNpc.position.x, posisiNpc.position.y, targetTiket.z);

        // --- BARIS BARU (SOLUSI) ---
        // 2. NYALAKAN (Munculkan) kertas dan tiket kembali
        dokumenKertas.gameObject.SetActive(true);
        objekTiket.gameObject.SetActive(true);

        // 3. Animasi meluncur ke posisi aslinya di meja
        time = 0;
        Vector3 titikAwalDokumen = dokumenKertas.position;
        Vector3 titikAwalTiket = objekTiket.position;

        while (time < 1)
        {
            time += Time.deltaTime * kecepatanSodorKertas;
            dokumenKertas.position = Vector3.Lerp(titikAwalDokumen, targetDokumen, time);
            objekTiket.position = Vector3.Lerp(titikAwalTiket, targetTiket, time);
            yield return null;
        }
    }

    // --- FASE C: FUNGSI UNTUK MENGUSIR NPC ---
    public void UsirNpc(bool isApprove)
    {
        StartCoroutine(AnimasiNpcPergi(isApprove));
    }

    IEnumerator AnimasiNpcPergi(bool isApprove)
    {
        float time = 0;
        Vector3 posisiAwalNpc = posisiNpc.position;

        float arahX = isApprove ? jarakPergi : -jarakPergi;
        Vector3 targetPergi = posisiAwalNpc + new Vector3(arahX, 0, 0);

        while (time < 1)
        {
            time += Time.deltaTime * kecepatanPergi;
            posisiNpc.position = Vector3.Lerp(posisiAwalNpc, targetPergi, time);
            yield return null;
        }

        Debug.Log("NPC sudah pergi dari layar!");
    }
}