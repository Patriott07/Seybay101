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

        // --- CEK STATUS KEMBALI DARI KOPER ---
        if (PlayerPrefs.HasKey("Paspor_PosX") || PlayerPrefs.HasKey("Tiket_PosX"))
        {
            // JIKA KEMBALI DARI KOPER:
            posisiNpc.localScale = skalaAsliNpc;

            dokumenKertas.gameObject.SetActive(true);
            objekTiket.gameObject.SetActive(true);
        }
        else
        {
            // JIKA NPC BARU DATANG:
            Vector3 skalaKecil = skalaAsliNpc * 0.1f;
            posisiNpc.localScale = skalaKecil;

            dokumenKertas.gameObject.SetActive(false);
            objekTiket.gameObject.SetActive(false);

            StartCoroutine(AdeganNpcMasuk(skalaKecil));
        }
    }

    IEnumerator AdeganNpcMasuk(Vector3 skalaKecil)
    {
        // --- FASE A: NPC MUNCUL DARI BELAKANG ---
        float time = 0;
        while (time < 1)
        {
            time += Time.deltaTime * kecepatanMasuk;
            posisiNpc.localScale = Vector3.Lerp(skalaKecil, skalaAsliNpc, time);
            yield return null;
        }

        yield return new WaitForSeconds(waktuTungguSodor);

        // --- FASE B: MENYODORKAN KERTAS KE MEJA ---
        dokumenKertas.position = new Vector3(posisiNpc.position.x, posisiNpc.position.y, targetDokumen.z);
        objekTiket.position = new Vector3(posisiNpc.position.x, posisiNpc.position.y, targetTiket.z);

        dokumenKertas.gameObject.SetActive(true);
        objekTiket.gameObject.SetActive(true);

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
        // 1. Hapus memori posisi dokumen agar NPC selanjutnya mulai dari awal
        PlayerPrefs.DeleteKey("Paspor_PosX");
        PlayerPrefs.DeleteKey("Paspor_PosY");

        PlayerPrefs.DeleteKey("Tiket_PosX");
        PlayerPrefs.DeleteKey("Tiket_PosY");

        // 2. Hapus status koper (jika kamu pakai alur satu arah)
        PlayerPrefs.DeleteKey("KoperSudahDicek");

        PlayerPrefs.Save();

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

        // --- PANGGIL FUNGSI RANDOM NPC DI SINI ---
        PanggilNpcBaru();
    }

    // --- WADAH FUNGSI UNTUK PROGRAMMER NPC ---
    public void PanggilNpcBaru()
    {
        Debug.Log("Menyiapkan NPC Acak Selanjutnya...");

        // TEMPAT KERJA TEMANMU:
        // Masukkan logika merandom sprite, mereset data identitas, dll di dalam fungsi ini.
        // Setelah diacak, panggil kembali Scene/Animasi Masuk.
    }
}