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

    [Header("Area Zona Spawn (Box Collider 2D)")]
    public BoxCollider2D areaSpawn;

    private float totalBerat = 0f;
    private List<LuggageItem> barangAktif = new List<LuggageItem>();

    void Start()
    {
        SembunyikanInfoBarang();
    }

    // --- FITUR BARU: Mengecek berat koper secara real-time setiap frame ---
    void Update()
    {
        if (barangAktif.Count > 0)
        {
            HitungBeratRealtime();
        }
    }

    private void HitungBeratRealtime()
    {
        float beratSekarang = 0f;

        foreach (LuggageItem item in barangAktif)
        {
            // Cek ke script LuggageItem: Apakah barang ini masih valid di dalam koper?
            if (item != null && item.ApakahDiDalamKoper())
            {
                beratSekarang += item.dataBarang.itemWeight;
            }
        }

        // Cek jika ada perubahan berat (menggunakan threshold kecil agar tidak update UI sia-sia)
        if (Mathf.Abs(totalBerat - beratSekarang) > 0.01f)
        {
            totalBerat = beratSekarang;
            UpdateUIBerat();
        }
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

        if (aturanHariIni == null || areaSpawn == null) return;

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

        Vector2 ukuranZona = areaSpawn.size;
        Vector2 offsetZona = areaSpawn.offset;
        Vector3 posisiZonaLokal = areaSpawn.transform.localPosition;

        for (int i = 0; i < barangDiSpawn.Count; i++)
        {
            GameObject barangBaru = Instantiate(barangDiSpawn[i], containerBarang.transform);

            float randomX = Random.Range(-ukuranZona.x / 2f, ukuranZona.x / 2f) + offsetZona.x;
            float randomY = Random.Range(-ukuranZona.y / 2f, ukuranZona.y / 2f) + offsetZona.y;

            barangBaru.transform.localPosition = posisiZonaLokal + new Vector3(randomX, randomY, 0f);

            // --- DIPERBAIKI: Barang di-spawn secara tegak lurus (tidak miring) ---
            barangBaru.transform.localRotation = Quaternion.identity;

            LuggageItem itemScript = barangBaru.GetComponent<LuggageItem>();
            if (itemScript != null)
            {
                // HAPUS penambahan totalBerat di sini, karena sekarang diurus otomatis oleh HitungBeratRealtime()
                barangAktif.Add(itemScript);
            }
        }
    }

    public void UpdateUIBerat()
    {
        if (aturanHariIni != null && aturanHariIni.gunakanBatasBerat && totalBerat > aturanHariIni.batasBeratMaksimal)
        {
            // Teks menjadi merah jika koper kelebihan muatan (Overweight)
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
                    if (EconomyManager.Instance != null)
                    {
                        EconomyManager.Instance.TambahTrust();
                        EconomyManager.Instance.TambahUang();
                    }
                    itemDisitaBenar.Add(item);
                }
                else
                {
                    if (EconomyManager.Instance != null)
                    {
                        EconomyManager.Instance.KurangiTrust(EconomyManager.Instance.penaltyTrustSalahSita);
                        EconomyManager.Instance.KurangiUang();
                    }
                }
            }
            else
            {
                if (isIlegal)
                {
                    if (EconomyManager.Instance != null)
                    {
                        EconomyManager.Instance.KurangiTrust(EconomyManager.Instance.penaltyTrustLolos);
                        EconomyManager.Instance.KurangiUang();
                    }
                }
            }
        }

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

        if (!string.IsNullOrEmpty(namaSceneSelanjutnya))
        {
            SceneManager.LoadScene(namaSceneSelanjutnya);
        }
    }
}