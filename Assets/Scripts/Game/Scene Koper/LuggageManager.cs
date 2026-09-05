using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LuggageManager : MonoBehaviour
{
    [Header("Aturan Hari Ini (Scalable)")]
    public DayRuleData aturanHariIni;

    [Header("UI & Referensi")]
    public TextMeshProUGUI textBeratKoper;
    public TextMeshProUGUI textInfoBarang;
    public TextMeshProUGUI textAturanBerat;
    public GameObject containerBarang;

    [Header("Pengaturan Pindah Scene")]
    public string namaSceneSelanjutnya = "NamaSceneBerikutnya";

    [Header("Database Barang (Prefabs)")]
    public List<GameObject> prefabBarangLegal;
    public List<GameObject> prefabBarangTerlarang;
    public Transform[] titikSpawn;

    private float totalBerat = 0f;
    private List<LuggageItem> barangAktif = new List<LuggageItem>();

    void Start()
    {
        SembunyikanInfoBarang();
    }

    public void GenerateBarangNPC()
    {
        foreach (Transform child in containerBarang.transform)
        {
            if (child.GetComponent<LuggageItem>() != null)
            {
                Destroy(child.gameObject);
            }
        }

        totalBerat = 0f;
        barangAktif.Clear();

        if (aturanHariIni == null)
        {
            Debug.LogError("Aturan Hari Ini belum dimasukkan ke Luggage Manager!");
            return;
        }

        List<GameObject> barangDiSpawn = new List<GameObject>();

        int totalBarang = Random.Range(aturanHariIni.minJumlahBarang, aturanHariIni.maxJumlahBarang + 1);
        int jumlahTerlarang = 0;

        if (Random.Range(0f, 100f) <= aturanHariIni.peluangBarangTerlarang)
        {
            jumlahTerlarang = Random.Range(1, aturanHariIni.maxBarangTerlarang + 1);
        }

        int jumlahLegal = totalBarang - jumlahTerlarang;

        for (int i = 0; i < jumlahLegal; i++)
        {
            if (prefabBarangLegal.Count > 0)
                barangDiSpawn.Add(prefabBarangLegal[Random.Range(0, prefabBarangLegal.Count)]);
        }
        for (int i = 0; i < jumlahTerlarang; i++)
        {
            if (prefabBarangTerlarang.Count > 0)
                barangDiSpawn.Add(prefabBarangTerlarang[Random.Range(0, prefabBarangTerlarang.Count)]);
        }

        for (int i = 0; i < barangDiSpawn.Count; i++)
        {
            GameObject temp = barangDiSpawn[i];
            int randomIndex = Random.Range(i, barangDiSpawn.Count);
            barangDiSpawn[i] = barangDiSpawn[randomIndex];
            barangDiSpawn[randomIndex] = temp;
        }

        for (int i = 0; i < barangDiSpawn.Count; i++)
        {
            if (i < titikSpawn.Length)
            {
                GameObject barangBaru = Instantiate(barangDiSpawn[i], titikSpawn[i].position, Quaternion.identity, containerBarang.transform);
                LuggageItem itemScript = barangBaru.GetComponent<LuggageItem>();

                if (itemScript != null)
                {
                    totalBerat += itemScript.dataBarang.itemWeight;
                    barangAktif.Add(itemScript);
                }
            }
        }

        UpdateUIBerat();
    }

    public void UpdateUIBerat()
    {
        if (aturanHariIni != null && aturanHariIni.gunakanBatasBerat && totalBerat > aturanHariIni.batasBeratMaksimal)
        {
            textBeratKoper.color = Color.red;
        }
        else
        {
            textBeratKoper.color = Color.white;
        }

        textBeratKoper.text = "Berat: " + totalBerat.ToString("F1") + " kg";
    }

    public void ToggleModeKoper(bool isModeKoperAktif)
    {
        containerBarang.SetActive(isModeKoperAktif);
    }

    public void TampilkanInfoBarang(string nama, float berat)
    {
        if (textInfoBarang != null) textInfoBarang.text = nama + " (" + berat.ToString("F1") + " kg)";
    }

    public void SembunyikanInfoBarang()
    {
        if (textInfoBarang != null) textInfoBarang.text = "";
    }

    public void EvaluasiInspeksi()
    {
        StartCoroutine(ProsesEvaluasiDanAnimasiKeluar());
    }

    private IEnumerator ProsesEvaluasiDanAnimasiKeluar()
    {
        List<LuggageItem> itemDisitaBenar = new List<LuggageItem>();

        foreach (LuggageItem item in barangAktif)
        {
            if (item == null) continue;

            bool diAtasMeja = Vector3.Distance(item.transform.position, item.GetPosisiAwal()) > 1.5f;

            bool isIlegal = item.dataBarang.isContraband;
            if (aturanHariIni != null && aturanHariIni.larangBarangOrganik && item.dataBarang.isOrganic)
            {
                isIlegal = true;
            }

            if (diAtasMeja)
            {
                if (isIlegal)
                {
                    // --- KEPUTUSAN BENAR: Sita Barang Ilegal ---
                    Debug.Log($"<color=green>[BENAR] {item.dataBarang.itemName} disita! Trust +10, Uang +50</color>");

                    if (EconomyManager.Instance != null)
                    {
                        EconomyManager.Instance.TambahTrust(); // Otomatis +10 (sesuai settingan EconomyManager)
                        EconomyManager.Instance.TambahUang();  // Otomatis +50 (sesuai settingan EconomyManager)
                    }

                    itemDisitaBenar.Add(item);
                }
                else
                {
                    // --- KEPUTUSAN SALAH: Barang Legal Malah Disita ---
                    Debug.Log($"<color=red>[SALAH] {item.dataBarang.itemName} legal malah disita! Trust -1, Uang -25</color>");

                    if (EconomyManager.Instance != null)
                    {
                        EconomyManager.Instance.KurangiTrust(EconomyManager.Instance.penaltyTrustSalahSita); // -1 Trust
                        EconomyManager.Instance.KurangiUang(); // -25 Uang
                    }
                }
            }
            else
            {
                if (isIlegal)
                {
                    // --- KEPUTUSAN SALAH: Barang Ilegal Lolos ---
                    Debug.Log($"<color=orange>[TERLEWAT] {item.dataBarang.itemName} ilegal lolos! Trust -5, Uang -25</color>");

                    if (EconomyManager.Instance != null)
                    {
                        EconomyManager.Instance.KurangiTrust(EconomyManager.Instance.penaltyTrustLolos); // -5 Trust
                        EconomyManager.Instance.KurangiUang(); // -25 Uang
                    }
                }
                else
                {
                    // --- KEPUTUSAN BENAR: Biarkan Barang Legal di Koper ---
                    // Tidak ada penalti/reward khusus, atau bisa disesuaikan jika mau.
                }
            }
        }

        Debug.Log("-------------------------------------------------");

        // --- JALANKAN ANIMASI HILANG DAN TUNGGU SAMPAI BENAR-BENAR SELESAI ---
        if (itemDisitaBenar.Count > 0)
        {
            foreach (LuggageItem item in itemDisitaBenar)
            {
                if (item != null)
                {
                    StartCoroutine(item.AnimasiKeluar());
                }
            }

            yield return new WaitForSeconds(0.5f);
        }

        // Pindah ke scene selanjutnya setelah evaluasi dan animasi selesai
        if (!string.IsNullOrEmpty(namaSceneSelanjutnya))
        {
            SceneManager.LoadScene(namaSceneSelanjutnya);
        }
        else
        {
            Debug.LogWarning("Nama Scene selanjutnya belum diisi di Inspector!");
        }
    }
}