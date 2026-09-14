using UnityEngine;

public class ModeGameSwitch : MonoBehaviour
{
    enum InspectionMode
    {
        Koper,
        Passport,
    };

    InspectionMode currentMode = InspectionMode.Passport;

    [Header("Container Mode")]
    public GameObject containerKoper;
    public GameObject containerPassport;

    [Header("Referensi Animasi")]
    public AnimasiKoperSimple scriptKoper;

    void Start()
    {
        InitializeMode();
    }

    void Enable()
    {
        GameEvent.OnPassportModeCall += SwitchToPassport;
    }

    void Disable()
    {
        GameEvent.OnPassportModeCall -= SwitchToPassport;
    }

    
    // Update is called once per frame
    void Update()
    {
        bool isSpaceDown = Input.GetKeyDown(KeyCode.Space);
        if (isSpaceDown)
            ToggleMode();
    }

    void InitializeMode()
    {
        switch (currentMode)
        {
            case InspectionMode.Koper:
                if (containerPassport != null)
                    containerPassport.SetActive(false);
                if (containerKoper != null)
                    containerKoper.SetActive(true);
                break;
            case InspectionMode.Passport:
                if (containerKoper != null)
                    containerKoper.SetActive(false);
                if (containerPassport != null)
                    containerPassport.SetActive(true);
                break;
        }
    }

    void ToggleMode()
    {
        switch (currentMode)
        {
            case InspectionMode.Passport:
                SwitchToKoper();
                break;
            case InspectionMode.Koper:
                SwitchToPassport();
                break;
        }
    }


     public void SwitchToKoper()
    {
        if (containerPassport != null)
            containerPassport.SetActive(false);
        if (containerKoper != null)
        {
            containerKoper.SetActive(true);
            containerKoper.transform.localScale = Vector3.one; // ← reset skala parent juga
            if (scriptKoper != null && !scriptKoper.IsOpen())
                scriptKoper.ToggleKoper();
        }
        currentMode = InspectionMode.Koper;
    }

    public void SwitchToPassport()
    {
        if (containerKoper != null)
            containerKoper.SetActive(false);
        if (containerPassport != null)
            containerPassport.SetActive(true);
        currentMode = InspectionMode.Passport;
    }


}
