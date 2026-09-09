using System.Collections.Generic;
using Schema.data;
using TMPro;
using UnityEngine;

// =============================================
// TICKET SCRIPT — Generates boarding pass data
// =============================================
// Ticket appears when NPC hands over passport (NpcEntranceManager)
// Data includes: passNumber, passengerName, seatClass, seatNumber, idPassengerCard, departureDate
// Ticket can be valid or invalid based on day rules
// =============================================
public class TicketScript : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text textID; // passNumber
    public TMP_Text textName; // passengerName
    public TMP_Text textClassSeat; // seatClass (Economy/Business/FirstClass)
    public TMP_Text textSeat; // seat number
    public TMP_Text textPassport; // idPassengerCard
    public TMP_Text textDate; // departureDate

    [Header("Database")]
    public BoardingPassSchema currentData;

    // Ticket validity: chance to be invalid based on day
    private const float TICKET_INVALID_CHANCE = 0.3f; // 30% chance ticket is invalid
    private const float TICKET_NAMEPASSEGER_INVALID_CHANCE = 0.3f; // 30% chance ticket is invalid

    void Awake()
    {
        // Subscribe to boarding pass generation event
        GameEvent.GenerateNewBoardingPass += GenerateData;
    }

    void OnDestroy()
    {
        GameEvent.GenerateNewBoardingPass -= GenerateData;
    }

    // Called when NPC arrives — generates boarding pass data
    public void GenerateData(PassportSchema passport)
    {
        int currentDay = GameManager.Instance != null ? GameManager.Instance.GetCurrentDay() : 1;
        bool isTicketValid = Random.Range(0f, 1f) > TICKET_INVALID_CHANCE;
        bool isNameValid = Random.Range(0f, 1f) > TICKET_NAMEPASSEGER_INVALID_CHANCE;

        BoardingPassSchema ticketData = new BoardingPassSchema();
        ticketData.passNumber = GeneratePassNumber(isTicketValid);
        ticketData.passengerName = GeneratePassengerName(isNameValid, passport.ownerName);
        ticketData.seatClass = GenerateSeatClass();
        ticketData.seat = GenerateSeatNumber(ticketData.seatClass);
        ticketData.idPassengerCard = GenerateIDCard(isTicketValid);
        ticketData.departureDate = GenerateDepartureDate(isTicketValid);
        ticketData.isValid = isTicketValid;
        // ticketData.estimationTime = GenerateEstimationTime();
        ticketData.destination = "Seybay Outpost";

        currentData = ticketData;
        UpdateView(ticketData);
    }

    // Updates all UI text fields with boarding pass data
    private void UpdateView(BoardingPassSchema data)
    {
        textID.text = data.passNumber;
        textName.text = data.passengerName;
        textClassSeat.text = data.seatClass.ToString();
        textSeat.text = data.seat;
        textPassport.text = data.idPassengerCard;
        textDate.text = data.departureDate;
    }

    // Generates pass number — valid tickets have format "TKT-XXXX", invalid have "FAKE-XXXX"
    private string GeneratePassNumber(bool isValid)
    {
        if (isValid)
            return "TKT-" + Random.Range(1000, 9999);
        else
            return "FAKE-" + Random.Range(1000, 9999);
    }

    // Generates random passenger name
    private string GeneratePassengerName(bool isValid, string realname)
    {
        if (isValid)
            return realname;
        else
        {
            var dataname = PassportScript.dataName[Random.Range(0, PassportScript.dataName.Count)];
            return Random.Range(0,1f) > 0.5 ? dataname.maleNames[Random.Range(0, dataname.maleNames.Count)] : dataname.femaleNames[Random.Range(0, dataname.femaleNames.Count)];
        }
    }

    // Random seat class
    private SeatClass GenerateSeatClass()
    {
        return (SeatClass)Random.Range(0, 3);
    }

    // Generates seat number based on class
    private string GenerateSeatNumber(SeatClass seatClass)
    {
        string prefix =
            seatClass == SeatClass.Economy ? "E"
            : seatClass == SeatClass.Business ? "B"
            : "F";
        return prefix + Random.Range(1, 60).ToString("D2");
    }

    // Generates ID card — valid tickets have "ID-XXXX", invalid have "ID-FAKE"
    private string GenerateIDCard(bool isValid)
    {
        if (isValid)
            return "ID-" + Random.Range(1000, 9999);
        else
            return "ID-FAKE-" + Random.Range(100, 999);
    }

    // Generates departure date — invalid tickets might have expired dates
    private string GenerateDepartureDate(bool isValid)
    {
        string day = Random.Range(1, 31).ToString();
        string month = Random.Range(1, 12).ToString();

        if (isValid)
            return day + "/" + month + "/2046";
        else
            return day + "/" + month + "/" + Random.Range(1960, 2044);
    }

    // Generates estimation time
    private string GenerateEstimationTime()
    {
        string[] times = { "2 Day", "48 Hours", "3 Day", "24 Hours" };
        return times[Random.Range(0, times.Length)];
    }

    // Returns whether current ticket is valid
    public bool IsValid()
    {
        return currentData != null && currentData.isValid;
    }
}
