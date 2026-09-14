using System;
using System.Collections.Generic;
using Schema.data;
using UnityEngine;

public class GameEvent : MonoBehaviour
{
    public static Action DeleteMarkTicket;
    public static Action<float> SpawnNPCOnStartDay;
    public static Action GenerateNewNPCView;
    public static Action GenerateNewPassportData;
    public static Action<PassportSchema> GenerateNewBoardingPass;
    public static Action<Gender> OnSetGander;
}
