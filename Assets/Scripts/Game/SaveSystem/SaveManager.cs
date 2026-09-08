using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Schema.data;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string saveFilePath;
    private GameSaveState currentSave;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Path.Combine(Application.persistentDataPath, "game_save.json");
            LoadSave();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region SAVE / LOAD

    public void SaveGame()
    {
        CaptureData();
        string json = JsonUtility.ToJson(currentSave, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("[SaveManager] Game saved to: " + saveFilePath);
    }

    public void LoadSave()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            currentSave = JsonUtility.FromJson<GameSaveState>(json);
            ApplyData();
            Debug.Log("[SaveManager] Game loaded from: " + saveFilePath);
        }
        else
        {
            currentSave = CreateNewSave();
            Debug.Log("[SaveManager] No save found. New save created.");
        }
    }

    public void ResetSave()
    {
        if (File.Exists(saveFilePath))
            File.Delete(saveFilePath);
        currentSave = CreateNewSave();
        Debug.Log("[SaveManager] Save reset.");
    }

    public bool HasSave()
    {
        return File.Exists(saveFilePath);
    }

    #endregion

    #region DATA CAPTURE

    private void CaptureData()
    {
        currentSave.currentDay = GameManager.Instance != null ? GameManager.Instance.GetCurrentDay() : 1;

        if (EconomyManager.Instance != null)
        {
            currentSave.playerCash = EconomyManager.Instance.money;
        }
    }

    private GameSaveState CreateNewSave()
    {
        return new GameSaveState
        {
            currentDay = 1,
            playerCash = 1500,
            purchasedTickets = 0,
            purhaceTicketForAldo = false,
            purhaceTicketForNasya = false,
            purhaceTicketForVirly = false,
            purhaceTicketForKraisa = false,
        };
    }

    #endregion

    #region APPLY DATA

    private void ApplyData()
    {
        if (currentSave == null) return;

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.money = currentSave.playerCash;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentDay(currentSave.currentDay);
        }
    }

    #endregion

    #region TICKET MANAGEMENT

    public void PurchaseTicket(string characterName)
    {
        switch (characterName)
        {
            case "Aldo":
                currentSave.purhaceTicketForAldo = true;
                break;
            case "Nasya":
                currentSave.purhaceTicketForNasya = true;
                break;
            case "Virly":
                currentSave.purhaceTicketForVirly = true;
                break;
            case "Kraisa":
                currentSave.purhaceTicketForKraisa = true;
                break;
        }
        currentSave.purchasedTickets++;
        SaveGame();
    }

    public bool HasPurchasedTicket(string characterName)
    {
        switch (characterName)
        {
            case "Aldo": return currentSave.purhaceTicketForAldo;
            case "Nasya": return currentSave.purhaceTicketForNasya;
            case "Virly": return currentSave.purhaceTicketForVirly;
            case "Kraisa": return currentSave.purhaceTicketForKraisa;
            default: return false;
        }
    }

    public int GetPurchasedTickets()
    {
        return currentSave.purchasedTickets;
    }

    #endregion

    #region DAY MANAGEMENT

    public void AdvanceDay()
    {
        currentSave.currentDay++;
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.MulaiHariBaru();
        SaveGame();
    }

    public int GetCurrentDay()
    {
        return currentSave.currentDay;
    }

    #endregion

    public string GetSaveStatus()
    {
        if (currentSave == null) return "No save data";
        return $"Day: {currentSave.currentDay} | Cash: ${currentSave.playerCash} | Tickets: {currentSave.purchasedTickets}";
    }
}
