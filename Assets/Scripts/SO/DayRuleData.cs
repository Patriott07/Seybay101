using UnityEngine;

[CreateAssetMenu(fileName = "AturanHari_", menuName = "Seybay/Aturan Harian")]
public class DayRuleData : ScriptableObject
{
    [Header("Info Hari")]
    public int hariKe;
    public string deskripsiAturan;

    [Header("Aturan Koper (Umum)")]
    public int minJumlahBarang = 2;
    public int maxJumlahBarang = 6;
    [Range(0, 100)] public float peluangBarangTerlarang = 30f;
    public int maxBarangTerlarang = 1;

    [Header("Aturan Berat (Hari 3+)")]
    public bool gunakanBatasBerat = false;
    public float batasBeratMaksimal = 35f;

    [Header("Aturan Inspeksi Lanjutan")]
    public bool aktifkanXRay = false; // Toggle UI X-Ray (Mulai Hari 4)
    public bool larangBarangOrganik = false; // Sita makanan/tanaman organik 
    public bool larangBahanTerbakar = false; // Sita kristal pendingin & bahan bakar (Hari 4)
    public bool aktifkanMetalDetector = false; // Toggle suara "Beep" logam (Mulai Hari 6)
    public bool larangLogamPadat = false; // Sita perhiasan/senjata logam (Hari 6)
}