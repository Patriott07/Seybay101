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

    // Menyimpan skala kecil asli bawaan Unity Editor
    private Vector3 skalaAwalContainerKoper;

    void Awake()
    {
        if (containerKoper != null)
            skalaAwalContainerKoper = containerKoper.transform.localScale;
    }

    void Start()
    {
        InitializeMode();
    }

    void OnEnable()
    {
        GameEvent.OnPassportModeCall += SwitchToPassport;
    }

    void OnDisable()
    {
        GameEvent.OnPassportModeCall -= SwitchToPassport;
    }

    void InitializeMode()
    {
        switch (currentMode)
        {
            case InspectionMode.Koper:
                if (containerPassport != null) containerPassport.SetActive(false);
                if (containerKoper != null) containerKoper.SetActive(true);
                break;
            case InspectionMode.Passport:
                if (containerKoper != null) containerKoper.SetActive(false);
                if (containerPassport != null) containerPassport.SetActive(true);
                break;
        }
    }

    void ToggleMode()
    {
        // === [GEMBOK 1: CEK NPC] ===
        // Jika tidak ada NPC di depan meja, tombol tidak akan merespon!
        if (!GameManager.Instance.isNpcInFront) return;
        // ===========================

        AudioManager.Instance.PlaySfxMenuToggle();
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
        // === [GEMBOK 2: CEK NPC] ===
        // Mencegah koper terbuka lewat event/panggilan fungsi lain saat NPC tidak ada
        if (!GameManager.Instance.isNpcInFront) return;
        // ===========================

        if (containerPassport != null) containerPassport.SetActive(false);
        if (containerKoper != null)
        {
            containerKoper.SetActive(true);
            containerKoper.transform.localScale = Vector3.one;

            if (scriptKoper != null && !scriptKoper.IsOpen())
                scriptKoper.ToggleKoper();
        }
        currentMode = InspectionMode.Koper;
    }

    public void SwitchToPassport()
    {
        if (containerKoper != null)
        {
            containerKoper.SetActive(false);
            containerKoper.transform.localScale = skalaAwalContainerKoper;
        }
        if (containerPassport != null) containerPassport.SetActive(true);
        currentMode = InspectionMode.Passport;
    }
}