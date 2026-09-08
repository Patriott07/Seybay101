using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StoryData_", menuName = "Seybay/Story Data")]
public class StorySchema : ScriptableObject
{
    [Header("Hari Aktif")]
    public int Day = 1;

    [Header("Sebelum Shift (Scene Pre-Day)")]
    [TextArea(3, 10)]
    public List<string> preDayText;

    [Header("Saat Shift Dimulai (On Desk)")]
    [TextArea(3, 10)]
    public List<string> onDeskText;

    [Header("Setelah Shift Selesai")]
    [TextArea(3, 10)]
    public List<string> afterShiftText;
}
