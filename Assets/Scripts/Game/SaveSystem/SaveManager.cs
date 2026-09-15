using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Schema.data;

// =============================================
// SAVE MANAGER — Singleton handling JSON save/load
// =============================================
// Save file location: Application.persistentDataPath/game_save.json
// Uses JsonUtility.ToJson/FromJson — no encryption
// Data: day, money, penalty, trust, ticket purchase status
// Note: RecapDayUI reads directly from this JSON file
// =============================================
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string saveFilePath;    // Path to the JSON save file
    private GameSaveState currentSave; // Current save state in memory

    // Singleton pattern + auto-load on Awake
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
            saveFilePath = Path.Combine(Application.persistentDataPath, "game_save.json");
            LoadSave(); // Auto-load existing save on game start
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameSaveState GetSaveData()
    {
        return currentSave;
    }

    #region SAVE / LOAD

    // Saves currentSave to JSON file on disk
    public void SaveGame()
    {
        CaptureData(); // Update currentSave with latest game state
        string json = JsonUtility.ToJson(currentSave, true); // Pretty-print JSON
        File.WriteAllText(saveFilePath, json); // Write to disk
        Debug.Log("[SaveManager] Game saved to: " + saveFilePath);
    }

    // Loads save from JSON file if exists, otherwise creates new save
    public void LoadSave()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            currentSave = JsonUtility.FromJson<GameSaveState>(json);

            if (IsSaveValid())
            {
                ApplyData();
                Debug.Log("[SaveManager] Game loaded from: " + saveFilePath);
            }
            else
            {
                Debug.Log("[SaveManager] Save data invalid. Creating fresh save.");
                currentSave = CreateNewSave();
                ApplyData();
                SaveGame();
            }
        }
        else
        {
            currentSave = CreateNewSave();
            ApplyData();
            SaveGame();
            Debug.Log("[SaveManager] No save found. New save created.");
        }
    }

    private bool IsSaveValid()
    {
        if (currentSave == null) return false;
        if (currentSave.playerCash < 0) return false;
        if (currentSave.currentDay < 1) return false;
        if (currentSave.trust < 0 || currentSave.trust > 100) return false;
        return true;
    }

    // Deletes save file and resets to new save state
    public void ResetSave()
    {
        if (File.Exists(saveFilePath))
            File.Delete(saveFilePath);
        currentSave = CreateNewSave();
        Debug.Log("[SaveManager] Save reset.");
    }

    public bool HasSave() => File.Exists(saveFilePath);

    #endregion

    #region DATA CAPTURE

    // Captures current game state into currentSave for serialization
    // Captures: day, money, penalty, and trust
    private void CaptureData()
    {
        currentSave.currentDay = GameManager.Instance != null ? GameManager.Instance.GetCurrentDay() : 1;

        if (EconomyManager.Instance != null)
        {
            currentSave.playerCash = EconomyManager.Instance.money;
            currentSave.penalty = EconomyManager.Instance.punish;
            currentSave.trust = EconomyManager.Instance.trust;
        }
    }

    // Creates a fresh default save state
    private GameSaveState CreateNewSave()
    {
        return new GameSaveState
        {
            currentDay = 1,
            playerCash = 0,
            purchasedTickets = 0,
            purhaceTicketForAldo = false,
            purhaceTicketForNasya = false,
            purhaceTicketForVirly = false,
            purhaceTicketForKraisa = false,
            penalty = 0,
            trust = 100,
        };
    }

    // Applies loaded save data back to game objects
    // Restores money, penalty, trust to EconomyManager and day to GameManager
    private void ApplyData()
    {
        if (currentSave == null) return;

        if (EconomyManager.Instance != null)
        {
            if (currentSave.playerCash > 0)
                EconomyManager.Instance.money = currentSave.playerCash;
            else
                EconomyManager.Instance.money = 0;

            // EconomyManager.Instance.punish = currentSave.penalty;

            // if (currentSave.trust > 0)
            //     EconomyManager.Instance.trust = currentSave.trust;
            // else
            //     EconomyManager.Instance.trust = 100;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentDay(currentSave.currentDay);
        }
    }

    #endregion

    #region TICKET MANAGEMENT

    // Records ticket purchase for a specific character and triggers save
    // Called when player buys a ticket (e.g., from Shop UI)
    public void PurchaseTicket(string characterName)
    {
        // Set the corresponding character's ticket flag to true
        switch (characterName)
        {
            case "Aldo":    currentSave.purhaceTicketForAldo = true; break;
            case "Nasya":   currentSave.purhaceTicketForNasya = true; break;
            case "Virly":   currentSave.purhaceTicketForVirly = true; break;
            case "Kraisa":  currentSave.purhaceTicketForKraisa = true; break;
        }
        currentSave.purchasedTickets++; // Increment total ticket count
        SaveGame(); // Save immediately after purchase
    }

    // Checks if player has purchased ticket for a specific character
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

    public int GetPurchasedTickets() => currentSave.purchasedTickets;

    #endregion

    #region DAY MANAGEMENT

    // Advances to next day: increments day counter, calls EconomyManager.MulaiHariBaru(), saves
    public void AdvanceDay()
    {
        currentSave.currentDay++;
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.MulaiHariBaru();
        SaveGame();
    }

    public int GetCurrentDay() => currentSave.currentDay;

    #endregion

    public string GetSaveStatus()
    {
        if (currentSave == null) return "No save data";
        return $"Day: {currentSave.currentDay} | Cash: ${currentSave.playerCash} | Tickets: {currentSave.purchasedTickets} | Penalty: ${currentSave.penalty} | Trust: {currentSave.trust}%";
    }
}
