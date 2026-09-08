using System;
using System.Collections.Generic;
using UnityEngine;
using Schema.data;

[System.Serializable]
public class GameSaveState
{
    public int currentDay;          // Hari aktif (1 - 10)
    public int playerCash;       // Saldo uang saat ini
    public int purchasedTickets;    // Jumlah tiket evakuasi terbeli (0 - 4)
    public bool purhaceTicketForAldo;
    public bool purhaceTicketForNasya;
    public bool purhaceTicketForVirly;
    public bool purhaceTicketForKraisa;
}
