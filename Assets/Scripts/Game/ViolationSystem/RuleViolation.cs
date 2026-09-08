using System.Collections.Generic;
using Schema.data;
using UnityEngine;
public abstract class RuleViolation
{
    public string ruleID;
    public string title;
    public string description;

    public abstract void Apply(PassportSchema passport);
    public abstract string GetViolationDetail(PassportSchema passport);
}

public class ExpiredPassportViolation : RuleViolation
{
    public ExpiredPassportViolation()
    {
        ruleID = "VIOL_EXP_001";
        title = "Expired Passport";
        description = "Passport sudah kedaluwarsa";
    }

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

public class InfoMismatchViolation : RuleViolation
{
    public InfoMismatchViolation()
    {
        ruleID = "VIOL_MISMATCH_001";
        title = "Info Mismatch";
        description = "Informasi tiket dan passport tidak selaras";
    }

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

public class StandardViolation : RuleViolation
{
    public StandardViolation()
    {
        ruleID = "VIOL_STD_001";
        title = "Standard Violation";
        description = "Passport tidak memenuhi standar (photo mismatch / invalid)";
    }

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

public class NewRule_Day2 : RuleViolation
{
    public NewRule_Day2()
    {
        ruleID = "VIOL_BRIBE_001";
        title = "Bribery Attempt";
        description = "NPC mencoba menyuap untuk melewati inspeksi";
    }

    public override void Apply(PassportSchema passport)
    {
        passport.hexaCardColor = "#FF0000";
    }

    public override string GetViolationDetail(PassportSchema passport)
    {
        return title + " — Hexa card color: " + passport.hexaCardColor;
    }
}

public class NewRule_Day3 : RuleViolation
{
    public NewRule_Day3()
    {
        ruleID = "VIOL_D3_001";
        title = "Suspicious Origin";
        description = "Asal daerah mencurigakan";
    }

    public override void Apply(PassportSchema passport)
    {
        passport.districtHome = "Restricted Zone";
    }

    public override string GetViolationDetail(PassportSchema passport)
    {
        return title + " — District: " + passport.districtHome;
    }
}

public class NewRule_Day5 : RuleViolation
{
    public NewRule_Day5()
    {
        ruleID = "VIOL_D5_001";
        title = "Counterfeit Document";
        description = "Dokumen palsu terdeteksi";
    }

    public override void Apply(PassportSchema passport)
    {
        passport.documentNumber = "FAKE" + Random.Range(1000, 9999);
    }

    public override string GetViolationDetail(PassportSchema passport)
    {
        return title + " — DocumentNumber: " + passport.documentNumber;
    }
}

public class NewRule_Day7 : RuleViolation
{
    public NewRule_Day7()
    {
        ruleID = "VIOL_D7_001";
        title = "Blacklist Entry";
        description = "NPC masuk dalam daftar hitam";
    }

    public override void Apply(PassportSchema passport)
    {
        passport.ownerName = "BLACKLISTED";
    }

    public override string GetViolationDetail(PassportSchema passport)
    {
        return title + " — OwnerName: " + passport.ownerName;
    }
}

public class NewRule_Day9 : RuleViolation
{
    public NewRule_Day9()
    {
        ruleID = "VIOL_D9_001";
        title = "Mutation Marker";
        description = "Tanda mutasi terdeteksi pada dokumen";
    }

    public override void Apply(PassportSchema passport)
    {
        passport.hexaCardColor = "#00FF00";
    }

    public override string GetViolationDetail(PassportSchema passport)
    {
        return title + " — HexaCardColor: " + passport.hexaCardColor;
    }
}
