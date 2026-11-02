using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Pasang di setiap objek yang tampil di Desk View (Passport, Ticket, Rules, Guide, Stempel, Kaca Pembesar, dst).
/// Untuk menambah objek baru: cukup taruh objeknya di dalam Desk Canvas / Desk World Root lalu Add Component DeskItem.
/// Tidak perlu mengubah script lain.
/// </summary>
public class DeskItem : MonoBehaviour
{
    [Header("Identitas")]
    public string idItem = "Passport";

    [Header("Ketersediaan per Hari")]
    [Tooltip("Mulai tampil di hari ke-berapa. Day 1 = 1")]
    public int hariMulai = 1;
    [Tooltip("Terakhir tampil di hari ke-berapa. 0 = tanpa batas")]
    public int hariAkhir = 0;

    [Header("Event (opsional)")]
    public UnityEvent onDeskDibuka;     // dipanggil saat Desk View dibuka (mis. reset posisi item)
    public UnityEvent onDeskDitutup;    // dipanggil saat Desk View ditutup

    public bool TersediaDiHari(int hari)
    {
        return hari >= hariMulai && (hariAkhir <= 0 || hari <= hariAkhir);
    }
}
