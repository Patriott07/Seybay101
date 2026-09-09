using System.Collections.Generic;
using Schema.data;
using UnityEngine;

// =============================================
// VIOLATION SYSTEM — Core engine for generating NPC violations
// =============================================
// Flow:
//   1. TryGenerateViolations() → 40% chance, picks 1-3 random violations from active rules
//   2. ApplyViolationsToPassport() → Modifies PassportSchema fields based on violations
//   3. Results stored in static LastViolations / LastViolationReason for access elsewhere
// =============================================
public class ViolationSystem
{
    private const float VIOLATION_CHANCE = 0.4f; // 40% — probability an NPC gets violations (60% clean)
    private const int MIN_VIOLATIONS = 1;         // Minimum violations if triggered
    private const int MAX_VIOLATIONS = 3;         // Maximum violations per NPC (1-3 random)

    // Static properties accessible from other scripts (e.g., NPCPassengerRuntimeData creation)
    public static List<RuleViolation> LastViolations { get; private set; } = new List<RuleViolation>();
    public static string LastViolationReason { get; private set; } = "";

    // Main entry point — generates violations for a given day
    // Returns true if violations were generated (40% chance), false if clean NPC
    public static bool TryGenerateViolations(int currentDay, out List<RuleViolation> violations, out string violationReason, out PassportSchema modifiedPassport)
    {
        violations = new List<RuleViolation>();
        violationReason = "";
        modifiedPassport = null;
        LastViolations = new List<RuleViolation>();
        LastViolationReason = "";

        // Get all active rules for this day (via DayRules factory)
        var allRules = DayRules.GetActiveRules(currentDay);

        // 40% chance to generate violations (60% clean NPC)
        bool hasViolation = Random.Range(0f, 1f) < VIOLATION_CHANCE;

        if (!hasViolation || allRules.Count == 0)
        {
            return false; // Clean NPC — no violations
        }

        // Randomly pick 1-3 violations from available rules
        int count = Random.Range(MIN_VIOLATIONS, MAX_VIOLATIONS + 1);
        count = Mathf.Min(count, allRules.Count); // Don't exceed available rules

        // Fisher-Yates shuffle to randomize selection
        var shuffled = new List<RuleViolation>(allRules);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }

        // Pick first `count` items from shuffled list
        for (int i = 0; i < count; i++)
        {
            violations.Add(shuffled[i]);
        }

        // Create new PassportSchema and apply each violation's modifications
        modifiedPassport = new PassportSchema();
        foreach (var violation in violations)
        {
            violation.Apply(modifiedPassport); // Each violation modifies the passport fields
        }

        // Build comma-separated violation reason string for NPCPassengerRuntimeData
        var reasonList = new List<string>();
        foreach (var violation in violations)
        {
            reasonList.Add(violation.title);
        }
        violationReason = string.Join(", ", reasonList);

        // Store in static properties for external access
        LastViolations = violations;
        LastViolationReason = violationReason;

        return true;
    }

    // Applies generated violations directly to an existing PassportSchema
    // Called from PassportScript.GenerateData() after creating the passport
    public static void ApplyViolationsToPassport(PassportSchema passport, int currentDay)
    {
        if (TryGenerateViolations(currentDay, out var violations, out _, out var modifiedPassport))
        {
            if (modifiedPassport != null)
            {
                // Copy all modified fields from the new passport to the existing one
                passport.expiryDate = modifiedPassport.expiryDate;
                passport.countryName = modifiedPassport.countryName;
                passport.districtHome = modifiedPassport.districtHome;
                passport.sameOwnerPhoto = modifiedPassport.sameOwnerPhoto;
                passport.isValid = modifiedPassport.isValid;
                passport.hexaCardColor = modifiedPassport.hexaCardColor;
                passport.documentNumber = modifiedPassport.documentNumber;
                passport.ownerName = modifiedPassport.ownerName;
            }
        }
    }
}
