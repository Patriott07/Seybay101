using System.Collections.Generic;
using UnityEngine;

// =============================================
// STORY SCHEMA — ScriptableObject for per-day story text
// =============================================
// Each day has its own StorySchema asset (created via Create > Seybay > Story Data)
// Contains 3 text phases that appear at different points in the game flow:
//   - preDayText   : Appears before the shift starts (story intro scene)
//   - onDeskText   : Appears when shift begins (at the immigration desk)
//   - afterShiftText : Appears after shift ends (post-shift report)
// =============================================
[CreateAssetMenu(fileName = "StoryData_", menuName = "Seybay/Story Data")]
public class StorySchema : ScriptableObject
{
    [Header("Hari Aktif")]
    public int Day = 1; // Day number (1-10) that this schema belongs to

    [Header("Sebelum Shift (Scene Pre-Day)")]
    [TextArea(3, 10)]
    public List<string> preDayText; // Text shown in PreDay scene before shift starts

    [Header("Saat Shift Dimulai (On Desk)")]
    [TextArea(3, 10)]
    public List<string> onDeskText; // Text shown when player starts the shift at the desk

    [Header("Setelah Shift Selesai")]
    [TextArea(3, 10)]
    public List<string> afterShiftText; // Text shown after shift ends (daily report)
}
