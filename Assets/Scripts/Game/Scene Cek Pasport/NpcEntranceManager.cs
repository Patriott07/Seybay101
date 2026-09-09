using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Schema.data;
using UnityEngine;

public class NpcEntranceManager : MonoBehaviour
{
    [Header("NPC setting")]
    public Transform npcPosSpawn;

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
    private Coroutine coroutineSpawnNpc;

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

        posisiNpc.localScale = skalaKecil;

        // randomize doc passport
        GameEvent.GenerateNewPassportData?.Invoke();

        // randomize boarding pass (ticket appears when NPC hands over passport)
        GameEvent.GenerateNewBoardingPass?.Invoke(PassportScript.Instance.currentData);

        // randomize look NPC
        GameEvent.GenerateNewNPCView?.Invoke();


        // Paksa posisinya kembali tepat ke titik spawn GameObject 'npcPosSpawn' di dunia nyata
        if (npcPosSpawn != null)
        {
            posisiNpc.position = npcPosSpawn.position;
        }
        else
        {
            posisiNpc.localPosition = Vector3.zero; // Cadangan jika spawner kosong
        }

        while (time < 1)
        {
            time += Time.deltaTime * kecepatanMasuk;
            posisiNpc.localScale = Vector3.Lerp(skalaKecil, skalaAsliNpc, time);
            yield return null;
        }

        yield return new WaitForSeconds(waktuTungguSodor);

        // --- FASE B: MENYODORKAN KERTAS KE MEJA ---

        // 1. Posisikan kertas di titik tengah NPC
        dokumenKertas.DOScale(new Vector3(2, 2f, 1), 0.8f);
        dokumenKertas.position = new Vector3(
            posisiNpc.position.x,
            posisiNpc.position.y,
            targetDokumen.z
        );

        objekTiket.DOScale(new Vector3(1.5f, 1.4f, 1), 0.8f);
        objekTiket.position = new Vector3(
            posisiNpc.position.x,
            posisiNpc.position.y,
            targetTiket.z
        );

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
        AnimasiNpcPergi(isApprove);
    }

    void AnimasiNpcPergi(bool isApprove)
    {
        // float time = 0;
        Vector3 posisiAwalNpc = posisiNpc.position;

        dokumenKertas.DOMove(posisiAwalNpc, 0.8f);
        dokumenKertas.DOScale(0, 0.6f);

        objekTiket.DOScale(0, 0.6f).SetDelay(0.4f);

        objekTiket
            .DOMove(posisiAwalNpc, 0.8f)
            .SetDelay(0.4f)
            .OnComplete(() =>
            {
                float arahX = isApprove ? jarakPergi : -jarakPergi;
                Vector3 targetPergi =
                    posisiAwalNpc + new Vector3(isApprove ? arahX : arahX / 2, 0, 0);

                // while (time < 1)
                // {
                //     time += Time.deltaTime * kecepatanPergi;
                //     posisiNpc.position = Vector3.Lerp(posisiAwalNpc, targetPergi, time);
                //     yield return null;
                // }

                GameEvent.DeleteMarkTicket?.Invoke();

                posisiNpc.DOMove(targetPergi, isApprove ? kecepatanPergi : kecepatanPergi / 2);
                if (GameManager.Instance.isCanSpawnNpc)
                    coroutineSpawnNpc = StartCoroutine(SpawnAnotherNPC(kecepatanPergi + 1f));
                else
                {
                    StopCoroutine(coroutineSpawnNpc);
                    GameManager.Instance.EndShiftAndLoadScene();
                }
                Debug.Log("NPC sudah pergi dari layar!");
            });
    }

    void OnEnable()
    {
        GameEvent.SpawnNPCOnStartDay += CallSpawnAnotherNPC;
    }

    void OnDisable()
    {
        GameEvent.SpawnNPCOnStartDay -= CallSpawnAnotherNPC;
    }

    void CallSpawnAnotherNPC(float d)
    {
        StartCoroutine(SpawnAnotherNPC(d));
    }

    IEnumerator SpawnAnotherNPC(float d)
    {
        yield return new WaitForSeconds(d);
        Vector3 skalaKecil = skalaAsliNpc * 0.1f;
        StartCoroutine(AdeganNpcMasuk(skalaKecil));
    }
}
