using UnityEngine;

public class PhoneWorldTrigger : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (PhoneManager.Instance != null)
        {
            PhoneManager.Instance.BukaHp();
        }
    }
}