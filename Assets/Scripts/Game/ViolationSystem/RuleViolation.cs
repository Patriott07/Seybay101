using System.Collections.Generic;
using Schema.data;

// =============================================
// ABSTRACT BASE CLASS — All violation types inherit from this
// =============================================
// Each concrete violation overrides:
//   - Apply()     : Modifies PassportSchema fields to create the violation
//   - GetViolationDetail() : Returns human-readable description of the violation
// =============================================
public abstract class RuleViolation
{
    public string ruleID;       // Unique identifier for this violation type
    public string title;        // Short title shown to player (e.g., "Expired Passport")
    public string description;  // Longer description of what the violation means

    // Override in each subclass to define how the violation modifies the passport
    public abstract void Apply(PassportSchema passport);

    // Override in each subclass to return a detail string for display/reporting
    public abstract string GetViolationDetail(PassportSchema passport);
}

// =============================================
// BASE RULES — Available from Day 1 onwards
// =============================================

// Rule 1: Passport has expired (expiryDate set to past year 1960-2044)
public class ExpiredPassportViolation : RuleViolation
{
    public ExpiredPassportViolation()
    {
        ruleID = "VIOL_EXP_001";
        title = "Expired Passport";
        description = "Passport sudah kedaluwarsa";
    }

    // Sets expiryDate to a random past date (pre-2044), making the passport expired
    public override void Apply(PassportSchema passport)
    {
        string day = Random.Range(1, 31).ToString();
        string month = Random.Range(1, 12).ToString();
        int year = Random.Range(1960, 2044);
        passport.expiryDate = day + "/" + month + "/" + year.ToString();
    }

    public override string GetViolationDetail(PassportSchema passport)
    {
        return title + " — Passport kedaluwarsa: " + passport.expiryDate;
    }
}

// Rule 2: Ticket and passport info don't match (country/district changed to invalid values)
public class InfoMismatchViolation : RuleViolation
{
    public InfoMismatchViolation()
    {
        ruleID = "VIOL_MISMATCH_001";
        title = "Info Mismatch";
        description = "Informasi tiket dan passport tidak selaras";
    }

    // Sets countryName to "XXX" and districtHome to "Unknown District" — indicates mismatch
    public override void Apply(PassportSchema passport)
    {
        passport.countryName = "XXX";
        passport.districtHome = "Unknown District";
    }

    public override string GetViolationDetail(PassportSchema passport)
    {
        return title + " — Country: " + passport.countryName + ", District: " + passport.districtHome;
    }
}

// Rule 3: Passport doesn't meet standards (photo mismatch + invalid document)
public class StandardViolation : RuleViolation
{
    public StandardViolation()
    {
        ruleID = "VIOL_STD_001";
        title = "Standard Violation";
        description = "Passport tidak memenuhi standar (photo mismatch / invalid)";
    }

    // Sets sameOwnerPhoto = false and isValid = false — passport fails inspection
    public override void Apply(PassportSchema passport)
    {
        passport.sameOwnerPhoto = false;
        passport.isValid = false;
    }

    public override string GetViolationDetail(PassportSchema passport)
    {
        return title + " — SameOwnerPhoto: " + passport.sameOwnerPhoto + ", IsValid: " + passport.isValid;
    }
}

// =============================================
// DAY 2+ RULES — Each adds new violation type on top of previous day's rules
// =============================================

// Day 2 — Bribery: NPC tries to bribe the officer
public class NewRule_Day2 : RuleViolation
{
    public NewRule_Day2()
    {
        ruleID = "VIOL_BRIBE_001";
        title = "Bribery Attempt";
        description = "NPC mencoba menyuap untuk melewati inspeksi";
    }

    // Sets hexaCardColor to red (#FF0000) — visual indicator of suspicious card
    public override void Apply(PassportSchema passport)
    {
        passport.hexaCardColor = "#FF0000";
    }

    public override string GetViolationDetail(PassportSchema passport)
    {
        return title + " — Hexa card color: " + passport.hexaCardColor;
    }
}

// Day 3 — Suspicious Origin: NPC comes from restricted district
public class NewRule_Day3 : RuleViolation
{
    public NewRule_Day3()
    {
        ruleID = "VIOL_D3_001";
        title = "Suspicious Origin";
        description = "Asal daerah mencurigakan";
    }

    // Sets districtHome to "Restricted Zone" — indicates NPC from high-risk area
    public override void Apply(PassportSchema passport)
    {
        passport.districtHome = "Restricted Zone";
    }

    public override string GetViolationDetail(PassportSchema passport)
    {
        return title + " — District: " + passport.districtHome;
    }
}

// Day 5 — Counterfeit Document: Document number is fake
public class NewRule_Day5 : RuleViolation
{
    public NewRule_Day5()
    {
        ruleID = "VIOL_D5_001";
        title = "Counterfeit Document";
        description = "Dokumen palsu terdeteksi";
    }

    // Prefixes documentNumber with "FAKE" — makes it immediately identifiable as counterfeit
    public override void Apply(PassportSchema passport)
    {
        passport.documentNumber = "FAKE" + Random.Range(1000, 9999);
    }

    public override string GetViolationDetail(PassportSchema passport)
    {
        return title + " — DocumentNumber: " + passport.documentNumber;
    }
}

// Day 7 — Blacklist Entry: NPC is on a blacklist
public class NewRule_Day7 : RuleViolation
{
    public NewRule_Day7()
    {
        ruleID = "VIOL_D7_001";
        title = "Blacklist Entry";
        description = "NPC masuk dalam daftar hitam";
    }

    // Sets ownerName to "BLACKLISTED" — immediately flags NPC as prohibited
    public override void Apply(PassportSchema passport)
    {
        passport.ownerName = "BLACKLISTED";
    }

    public override string GetViolationDetail(PassportSchema passport)
    {
        return title + " — OwnerName: " + passport.ownerName;
    }
}

// Day 9 — Mutation Marker: Special marker detected on the card
public class NewRule_Day9 : RuleViolation
{
    public NewRule_Day9()
    {
        ruleID = "VIOL_D9_001";
        title = "Mutation Marker";
        description = "Tanda mutasi terdeteksi pada dokumen";
    }

    // Sets hexaCardColor to green (#00FF00) — mutation indicator
    public override void Apply(PassportSchema passport)
    {
        passport.hexaCardColor = "#00FF00";
    }

    public override string GetViolationDetail(PassportSchema passport)
    {
        return title + " — HexaCardColor: " + passport.hexaCardColor;
    }
}
