using System;
using System.Collections;
using System.Collections.Generic;
using Schema.data;
using UnityEngine;

[System.Serializable]
public class StructureDialogueContext
{
    public AgeGroup ageGroup;
    public Mood currentMood;
    public Arktype arktypeNPC;
    public Issues currentIssue; // its should be override the value while player do interupt
    public Tactic tactic;
    public WorldSituation worldState;
    public InternalSituation internalSituation;
}

public class GenerateDialogueTest : MonoBehaviour
{
    [SerializeField]
    StructureDialogueContext NPCContext;
    public TextAsset jsonFileLayer1;
    public TextAsset jsonFileLayer2;
    public TextAsset jsonFileLayer3;
    public TextAsset jsonFileLayer4;

    // Layer 1: using Schema.Data types
    private List<ExpressionData> dialogLayer1 = new List<ExpressionData>();

    // Layer 2: using Schema.Data types
    private List<Layer2Entry> dialogLayer2 = new List<Layer2Entry>();

    // Layer 3: using Schema.Data types
    private List<Layer3Entry> dialogLayer3 = new List<Layer3Entry>();

    // Layer 4: using Schema.Data types - same structure as Layer 3
    private List<Layer4Entry> dialogLayer4 = new List<Layer4Entry>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadAllDialogueData();
    }

    void LoadAllDialogueData()
    {
        LoadLayer1();
        LoadLayer2();
        LoadLayer3();
        LoadLayer4();
    }

    void LoadLayer1()
    {
        if (jsonFileLayer1 != null)
        {
            string jsonString = jsonFileLayer1.text;
            DialogueDatabaseLayer1 database = JsonUtility.FromJson<DialogueDatabaseLayer1>(jsonString);

            if (database != null && database.layerExpresion != null)
            {
                dialogLayer1 = database.layerExpresion;
                Debug.Log($"Layer 1 loaded: {dialogLayer1.Count} expressions");
            }
            else
            {
                Debug.LogWarning("Format JSON Layer 1 tidak terbaca atau kosong, periksa kembali struktur wrapper-nya.");
            }
        }
        else
        {
            Debug.LogError("File Dialogue_layer1.json belum dimasukkan ke Inspector!");
        }
    }

    void LoadLayer2()
    {
        if (jsonFileLayer2 != null)
        {
            string jsonString = jsonFileLayer2.text;
            DialogueDatabaseLayer2 database = JsonUtility.FromJson<DialogueDatabaseLayer2>(jsonString);

            if (database != null && database.layerData != null)
            {
                dialogLayer2 = database.layerData;
                Debug.Log($"Layer 2 loaded: {dialogLayer2.Count} entries");
            }
            else
            {
                Debug.LogWarning("Format JSON Layer 2 tidak terbaca atau kosong.");
            }
        }
        else
        {
            Debug.LogError("File Dialogue_layer2.json belum dimasukkan ke Inspector!");
        }
    }

    void LoadLayer3()
    {
        if (jsonFileLayer3 != null)
        {
            string jsonString = jsonFileLayer3.text;
            DialogueDatabaseLayer3 database = JsonUtility.FromJson<DialogueDatabaseLayer3>(jsonString);

            if (database != null && database.layerData != null)
            {
                dialogLayer3 = database.layerData;
                Debug.Log($"Layer 3 loaded: {dialogLayer3.Count} entries");
            }
            else
            {
                Debug.LogWarning("Format JSON Layer 3 tidak terbaca atau kosong.");
            }
        }
        else
        {
            Debug.LogError("File Dialogue_layer3.json belum dimasukkan ke Inspector!");
        }
    }

    void LoadLayer4()
    {
        if (jsonFileLayer4 != null)
        {
            string jsonString = jsonFileLayer4.text;
            DialogueDatabaseLayer4 database = JsonUtility.FromJson<DialogueDatabaseLayer4>(jsonString);

            if (database != null && database.layerData != null)
            {
                dialogLayer4 = database.layerData;
                Debug.Log($"Layer 4 loaded: {dialogLayer4.Count} situations");
            }
            else
            {
                Debug.LogWarning("Format JSON Layer 4 tidak terbaca atau kosong.");
            }
        }
        else
        {
            Debug.LogError("File Dialogue_layer4.json belum dimasukkan ke Inspector!");
        }
    }

    public void GenerateFirstBubble()
    {
        // Example: Get first dialogue from Layer 1
        if (dialogLayer1.Count > 0)
        {
            var firstExpression = dialogLayer1[0];
            Debug.Log($"Layer 1 - Expression: {firstExpression.expressionName}, Total age groups: {firstExpression.ageGroups.Count}");
        }

        // Example: Get first dialogue from Layer 2
        if (dialogLayer2.Count > 0)
        {
            var firstEntry = dialogLayer2[0];
            Debug.Log($"Layer 2 - Issue: {firstEntry.issueID}, Total expressions: {firstEntry.expressions.Count}");
        }

        // Example: Get first dialogue from Layer 3
        if (dialogLayer3.Count > 0)
        {
            var firstEntry = dialogLayer3[0];
            Debug.Log($"Layer 3 - Tactic: {firstEntry.tacticName}, Total arktypes: {firstEntry.arktypes.Count}");
        }

        // Example: Get first dialogue from Layer 4
        // Layer4Entry -> situationName + List<ArktypeData4> -> each has arktype + List<MoodData4> -> each has moodName + List<DialogueLine>
        if (dialogLayer4.Count > 0)
        {
            var firstEntry = dialogLayer4[0];
            Debug.Log($"Layer 4 - Situation: {firstEntry.situationName}, Total arktypes: {firstEntry.arktypes.Count}");

            if (firstEntry.arktypes.Count > 0)
            {
                var firstArktype = firstEntry.arktypes[0];
                Debug.Log($"  Arktype: {firstArktype.arktype}, Total moods: {firstArktype.moods.Count}");

                if (firstArktype.moods.Count > 0)
                {
                    var firstMood = firstArktype.moods[0];
                    Debug.Log($"  Mood: {firstMood.moodName}, Total lines: {firstMood.lines.Count}");

                    if (firstMood.lines.Count > 0)
                    {
                        var firstLine = firstMood.lines[0];
                        Debug.Log($"  First line: {firstLine.dialog} (continue_bubble: {firstLine.continue_bubble})");
                    }
                }
            }
        }
    }
}
