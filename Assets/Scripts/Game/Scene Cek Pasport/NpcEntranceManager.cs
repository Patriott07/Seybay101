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

private bool dokumenSiapDiklik = false;
public bool DokumenSiapDiklik => dokumenSiapDiklik;

    // Pengaman agar dokumen tidak bisa diklik
    // sebelum seluruh proses NPC masuk selesai.
  
    

    void Start()
    {
        targetDokumen = dokumenKertas.position;
        targetTiket = objekTiket.position;
        skalaAsliNpc = posisiNpc.localScale;

        if (PlayerPrefs.HasKey("Paspor_PosX") || PlayerPrefs.HasKey("Tiket_PosX"))
        {
            posisiNpc.localScale = skalaAsliNpc;

            dokumenKertas.gameObject.SetActive(true);
            objekTiket.gameObject.SetActive(true);

            GameEvent.GenerateNewPassportData?.Invoke();
            GameEvent.GenerateNewBoardingPass?.Invoke(PassportScript.Instance.currentData);

            dokumenSiapDiklik = true;
            SetDokumenInteractive(true);
        }
        else
        {
            Vector3 skalaKecil = skalaAsliNpc * 0f;
            posisiNpc.localScale = skalaKecil;

            dokumenKertas.localScale = Vector3.zero;
            objekTiket.localScale = Vector3.zero;

            dokumenKertas.gameObject.SetActive(false);
            objekTiket.gameObject.SetActive(false);

            dokumenSiapDiklik = false;
            SetDokumenInteractive(false);
        }
    }

    IEnumerator AdeganNpcMasuk(Vector3 skalaKecil)
    {
        // ==================================================
        // AWAL NPC BARU
        // ==================================================

        dokumenSiapDiklik = false;
        SetDokumenInteractive(false);

        posisiNpc.localScale = Vector3.zero;

        // Pastikan dokumen NPC lama benar-benar tidak aktif
        dokumenKertas.gameObject.SetActive(false);
        objekTiket.gameObject.SetActive(false);

        // Randomize data baru
        GameEvent.GenerateNewPassportData?.Invoke();
        GameEvent.GenerateNewBoardingPass?.Invoke(PassportScript.Instance.currentData);
        GameEvent.GenerateNewNPCView?.Invoke();

        GameManager.Instance.DoViolationGenerateSetup();
        GameManager.Instance.DoViolationGenerateSetup();

        if (npcPosSpawn != null)
            posisiNpc.position = npcPosSpawn.position;
        else
            posisiNpc.localPosition = Vector3.zero;

        // ==================================================
        // FASE A: NPC MUNCUL
        // ==================================================

        Vector3 animStartingPoint = posisiNpc.position;

        float entranceDuration = 1f / kecepatanMasuk;
        int bounceCount = 3;

        posisiNpc.DOScale(
            skalaAsliNpc,
            entranceDuration
        ).SetEase(Ease.OutQuad);

        yield return posisiNpc
            .DOMoveY(
                animStartingPoint.y + 0.3f,
                entranceDuration / (bounceCount * 2f)
            )
            .SetLoops(
                bounceCount * 2,
                LoopType.Yoyo
            )
            .SetEase(Ease.InOutSine)
            .WaitForCompletion();

        posisiNpc.position = animStartingPoint;

        posisiNpc.DOScale(
            skalaAsliNpc,
            entranceDuration
        ).SetEase(Ease.OutQuad);

        yield return posisiNpc
            .DOMoveY(
                animStartingPoint.y + 0.3f,
                entranceDuration / (bounceCount * 2f)
            )
            .SetLoops(
                bounceCount * 2,
                LoopType.Yoyo
            )
            .SetEase(Ease.InOutSine)
            .WaitForCompletion();

        posisiNpc.position = animStartingPoint;

        yield return new WaitForSeconds(waktuTungguSodor);

        // ==================================================
        // FASE B: MENYODORKAN KERTAS
        // ==================================================

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySfxPaper();

        // Aktifkan dokumen NPC baru
        dokumenKertas.gameObject.SetActive(true);
        objekTiket.gameObject.SetActive(true);

        // Reset transform dari NPC sebelumnya
        dokumenKertas.DOKill();
        objekTiket.DOKill();

        dokumenKertas.localScale = Vector3.zero;
        objekTiket.localScale = Vector3.zero;

        dokumenKertas.position = new Vector3(
            posisiNpc.position.x,
            posisiNpc.position.y,
            targetDokumen.z
        );

        objekTiket.position = new Vector3(
            posisiNpc.position.x,
            posisiNpc.position.y,
            targetTiket.z
        );

        dokumenKertas.DOScale(
            new Vector3(2f, 2f, 1f),
            0.8f
        );

        objekTiket.DOScale(
            new Vector3(1.5f, 1.4f, 1f),
            0.8f
        );

        // Tetap tidak bisa diklik selama bergerak
        SetDokumenInteractive(false);

        yield return new WaitForSeconds(0.3f);

        float time = 0f;

        Vector3 titikAwalDokumen = dokumenKertas.position;
        Vector3 titikAwalTiket = objekTiket.position;

        while (time < 1f)
        {
            time += Time.deltaTime * kecepatanSodorKertas;

            dokumenKertas.position = Vector3.Lerp(
                titikAwalDokumen,
                targetDokumen,
                time
            );

            objekTiket.position = Vector3.Lerp(
                titikAwalTiket,
                targetTiket,
                time
            );

            yield return null;
        }

        // Pastikan posisi benar-benar tepat di meja
        dokumenKertas.position = targetDokumen;
        objekTiket.position = targetTiket;

        // ==================================================
        // NPC BARU SUDAH SELESAI MASUK
        // ==================================================

        GameManager.Instance.isNpcInFront = true;

        if (Manager_Chat.Instance != null)
            Manager_Chat.Instance.MulaiChatBaru();

        // BARU SEKARANG dokumen boleh diklik
        dokumenSiapDiklik = true;
        SetDokumenInteractive(true);
    }

    void SetDokumenInteractive(bool status)
    {
        // Tidak boleh aktif sebelum NPC benar-benar siap
        status = status && dokumenSiapDiklik;

        if (dokumenKertas != null)
        {
            var col3D = dokumenKertas.GetComponent<Collider>();
            var col2D = dokumenKertas.GetComponent<Collider2D>();

            if (col3D != null)
                col3D.enabled = status;

            if (col2D != null)
                col2D.enabled = status;
        }

        if (objekTiket != null)
        {
            var col3D = objekTiket.GetComponent<Collider>();
            var col2D = objekTiket.GetComponent<Collider2D>();

            if (col3D != null)
                col3D.enabled = status;

            if (col2D != null)
                col2D.enabled = status;
        }
    }

    public void UsirNpc(bool isApprove)
    {
        // Langsung kunci dokumen
        dokumenSiapDiklik = false;
        SetDokumenInteractive(false);

        PlayerPrefs.DeleteKey("Paspor_PosX");
        PlayerPrefs.DeleteKey("Paspor_PosY");
        PlayerPrefs.DeleteKey("Tiket_PosX");
        PlayerPrefs.DeleteKey("Tiket_PosY");
        PlayerPrefs.DeleteKey("KoperSudahDicek");
        PlayerPrefs.Save();

        if (LuggageManager.Instance != null)
            LuggageManager.Instance.ClearItem();

        AnimasiKoperSimple scriptKoper =
            FindObjectOfType<AnimasiKoperSimple>(true);

        if (scriptKoper != null)
            scriptKoper.ResetStatusMurni();

        ModeGameSwitch mode =
            FindObjectOfType<ModeGameSwitch>(true);

        if (mode != null)
            mode.SwitchToPassport();

        AnimasiNpcPergi(isApprove);
    }

    void AnimasiNpcPergi(bool isApprove)
    {
        dokumenSiapDiklik = false;
        SetDokumenInteractive(false);

        StartCoroutine(
            ProsesNpcPergiRoutine(isApprove)
        );
    }

    IEnumerator ProsesNpcPergiRoutine(bool isApprove)
    {
        Vector3 posisiAwalNpc = posisiNpc.position;

        // Passport kembali ke NPC lama
        dokumenKertas.DOMove(
            posisiAwalNpc,
            0.8f
        );

        dokumenKertas.DOScale(
            0f,
            0.6f
        );

        // Ticket kembali ke NPC lama
        objekTiket.DOScale(
            0f,
            0.6f
        ).SetDelay(0.4f);

        yield return objekTiket
            .DOMove(
                posisiAwalNpc,
                0.8f
            )
            .SetDelay(0.4f)
            .WaitForCompletion();

        // ==================================================
        // HILANGKAN DOKUMEN NPC LAMA
        // ==================================================

        dokumenKertas.gameObject.SetActive(false);
        objekTiket.gameObject.SetActive(false);

        SetDokumenInteractive(false);

        float arahX = isApprove
            ? jarakPergi
            : -jarakPergi;

        Vector3 targetPergi =
            posisiAwalNpc +
            new Vector3(
                isApprove ? arahX : arahX / 2,
                0,
                0
            );

        GameEvent.DeleteMarkTicket?.Invoke();

        float animDuration = isApprove
            ? kecepatanPergi
            : kecepatanPergi / 2f;

        int bounceCount = isApprove
            ? 12
            : 5;

        posisiNpc
            .DOMoveX(
                targetPergi.x,
                animDuration
            )
            .SetEase(Ease.Linear);

        yield return posisiNpc
            .DOMoveY(
                posisiAwalNpc.y + 0.3f,
                animDuration / (bounceCount * 2f)
            )
            .SetLoops(
                bounceCount * 2,
                LoopType.Yoyo
            )
            .SetEase(Ease.InOutSine)
            .WaitForCompletion();

        if (GameManager.Instance.isCanSpawnNpc)
        {
            coroutineSpawnNpc =
                StartCoroutine(
                    SpawnAnotherNPC(1f)
                );
        }
        else
        {
            if (coroutineSpawnNpc != null)
                StopCoroutine(coroutineSpawnNpc);

            GameManager.Instance.EndShiftAndLoadScene();
        }

        GameManager.Instance.isNpcInFront = false;
    }

    void OnEnable()
    {
        GameEvent.SpawnNPCOnStartDay +=
            CallSpawnAnotherNPC;
    }

    void OnDisable()
    {
        GameEvent.SpawnNPCOnStartDay -=
            CallSpawnAnotherNPC;
    }

    public void CallSpawnAnotherNPC(float d)
    {
        StartCoroutine(
            SpawnAnotherNPC(d)
        );
    }

    IEnumerator SpawnAnotherNPC(float d)
    {
        yield return new WaitForSeconds(d);

        Vector3 skalaKecil =
            skalaAsliNpc * 0.1f;

        yield return AdeganNpcMasuk(
            skalaKecil
        );
    }
}