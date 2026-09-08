using System;
using UnityEngine;
using Schema.data;

public class GameEvent : MonoBehaviour
{
    public static Action DeleteMarkTicket;
    public static Action<float> SpawnNPCOnStartDay;
    public static Action GenerateNewNPCView;
    public static Action GenerateNewPassportData;
    public static Action<Gender> OnSetGander;
}
