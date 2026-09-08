using System.Collections.Generic;
using Schema.data;
using UnityEngine;
public class ViolationSystem
{
    private const float VIOLATION_CHANCE = 0.35f;
    private const int MIN_VIOLATIONS = 1;
    private const int MAX_VIOLATIONS = 3;

    public static List<RuleViolation> LastViolations { get; private set; } = new List<RuleViolation>();
    public static string LastViolationReason { get; private set; } = "";

    public static bool TryGenerateViolations(int currentDay, out List<RuleViolation> violations, out string violationReason, out PassportSchema modifiedPassport)
    {
        violations = new List<RuleViolation>();
        violationReason = "";
        modifiedPassport = null;
        LastViolations = new List<RuleViolation>();
        LastViolationReason = "";

        var allRules = DayRules.GetActiveRules(currentDay);

        bool hasViolation = Random.Range(0f, 1f) < VIOLATION_CHANCE;

        if (!hasViolation || allRules.Count == 0)
        {
            return false;
        }

        int count = Random.Range(MIN_VIOLATIONS, MAX_VIOLATIONS + 1);
        count = Mathf.Min(count, allRules.Count);

        var shuffled = new List<RuleViolation>(allRules);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }

        for (int i = 0; i < count; i++)
        {
            violations.Add(shuffled[i]);
        }

        modifiedPassport = new PassportSchema();
        foreach (var violation in violations)
        {
            violation.Apply(modifiedPassport);
        }

        var reasonList = new List<string>();
        foreach (var violation in violations)
        {
            reasonList.Add(violation.title);
        }
        violationReason = string.Join(", ", reasonList);

        LastViolations = violations;
        LastViolationReason = violationReason;

        return true;
    }

    public static void ApplyViolationsToPassport(PassportSchema passport, int currentDay)
    {
        if (TryGenerateViolations(currentDay, out var violations, out _, out var modifiedPassport))
        {
            if (modifiedPassport != null)
            {
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
