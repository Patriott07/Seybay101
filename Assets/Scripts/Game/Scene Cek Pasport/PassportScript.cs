using System.Collections.Generic;
using Schema.data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PassportScript : MonoBehaviour
{
    public static PassportScript Instance;

    [Header("Proprty UI ref")]
    public TMP_Text textID;
    public TMP_Text textName,
        textCountry,
        textExp,
        textSex,
        textBod;

    public GameObject realImagePass, fakeImagePass;

    [Header("Database")]
    public List<NamePool> dataName;
    public PassportSchema currentData;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void UpdateViewPassword(PassportSchema data)
    {
        textID.text = data.documentNumber;
        textName.text = data.ownerName;
        textCountry.text = data.countryName;
        textExp.text = data.expiryDate;
        textSex.text = data.sex;
        textBod.text = data.bodOwner;
    }

    void OnEnable()
    {
        GameEvent.GenerateNewPassportData += GenerateData;
    }

    void OnDisable()
    {
        GameEvent.GenerateNewPassportData -= GenerateData;
    }

    void GenerateData()
    {
        PassportSchema Passdata = new PassportSchema();
        Passdata.documentNumber = GenerateID();
        Passdata.sex = GenerateSex();
        Passdata.ownerName = GenerateOwnerName(Passdata.sex == "F" ? true : false);
        Passdata.countryName = "IND";
        Passdata.expiryDate = GenerateExpireDate();
        Passdata.bodOwner = GenerateBodOwner();

        currentData = Passdata;
        UpdateViewPassword(Passdata);

        int currentDay = GameManager.Instance != null ? GameManager.Instance.GetCurrentDay() : 1;
        ViolationSystem.ApplyViolationsToPassport(currentData, currentDay);
    }

    string GenerateID()
    {
        return "ART0" + Random.Range(1111, 9999);
    }

    string GenerateSex()
    {
        if (Random.Range(1, 5) > 2.5)
        {
            GameEvent.OnSetGander?.Invoke(Gender.Woman);
            return "F";
        }
        else
        {
            GameEvent.OnSetGander?.Invoke(Gender.Man);
            return "M";
        }
    }

    string GenerateBodOwner()
    {
        string day = Random.Range(1, 31).ToString();
        string month = Random.Range(1, 12).ToString();

        int age = Random.Range(19, 54);
        return day + "/" + month + "/" + (2045 - age).ToString();
    }

    string GenerateExpireDate()
    {
        bool isValid = Random.Range(1, 5) > 2.5 ? true : false;

        string day = Random.Range(1, 31).ToString();
        string month = Random.Range(1, 12).ToString();

        if (isValid)
            return day + "/" + month + "/" + Random.Range(2046, 2066).ToString();
        else
            return day + "/" + month + "/" + Random.Range(1960, 2044).ToString();
    }

    string GenerateOwnerName(bool isFemale)
    {
        if (isFemale)
        {
            return dataName[0].femaleNames[Random.Range(0, dataName[0].femaleNames.Count)]
                + " "
                + dataName[0].femaleNames[Random.Range(0, dataName[0].femaleNames.Count)];
        }
        else
        {
            return dataName[0].maleNames[Random.Range(0, dataName[0].maleNames.Count)]
                + " "
                + dataName[0].maleNames[Random.Range(0, dataName[0].maleNames.Count)];
        }
    }
}
