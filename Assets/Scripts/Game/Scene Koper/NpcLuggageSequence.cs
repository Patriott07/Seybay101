using System.Collections;
using UnityEngine;

public class NpcLuggageSequence : MonoBehaviour
{
    [Header("Referensi Objek")]
    public Transform npcKarakter;
    public Transform koper; // Pastikan ini diisi dengan "Koper_Bawah"
    public AnimasiKoperSimple scriptKoper;

    // LuggageManager DIHAPUS dari sini karena sudah diurus otomatis oleh AnimasiKoperSimple

    [Header("Titik Target")]
    public Transform titikMejaKoper;

    [Header("Pengaturan Waktu")]
    public float kecepatanMasuk = 2.5f;
    public float kecepatanGeserKoper = 3f;
    public float jedaSebelumBuka = 0.5f;

    private Vector3 skalaAsliNpc;
    private Vector3 skalaAsliKoper;

    void Start()
    {
        skalaAsliNpc = npcKarakter.localScale;
        skalaAsliKoper = koper.localScale;

        // Sembunyikan koper sebelum NPC datang
        koper.gameObject.SetActive(false);

        // Siapkan ukuran NPC kecil (jauh)
        npcKarakter.localScale = skalaAsliNpc * 0.1f;

        StartCoroutine(JalankanAdegan());
    }

    IEnumerator JalankanAdegan()
    {
        // --- FASE A: NPC MUNCUL DARI JAUH ---
        float time = 0;
        Vector3 skalaKecilNpc = npcKarakter.localScale;

        while (time < 1)
        {
            time += Time.deltaTime * kecepatanMasuk;
            npcKarakter.localScale = Vector3.Lerp(skalaKecilNpc, skalaAsliNpc, time);
            yield return null;
        }

        // Jeda sejenak setelah NPC sampai di loket
        yield return new WaitForSeconds(0.5f);

        // --- FASE B: MENGELUARKAN KOPER KE MEJA ---
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

        // --- FASE C: BUKA KOPER ---
        yield return new WaitForSeconds(jedaSebelumBuka);

        // 1. Koper animasi mengecil, ganti sprite, dan membesar
        // (Barang dan Doc akan otomatis dipanggil di dalam fungsi ini saat koper selesai membesar)
        scriptKoper.ToggleKoper();
    }
}