using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Prototype-only runner for TASK-0001. It loads Resources by default so it can
/// be used without changing a scene or assigning shared assets in the Inspector.
/// </summary>
public sealed class GenerativeDialogueTestRunner : MonoBehaviour
{
    [SerializeField] private StructureDialogueContext npcContext;
    [SerializeField] private int seed = 1001;
    [SerializeField] private bool runOnStart = true;
    [SerializeField] private TextAsset jsonFileLayer1;
    [SerializeField] private TextAsset jsonFileLayer2;
    [SerializeField] private TextAsset jsonFileLayer3;
    [SerializeField] private TextAsset jsonFileLayer4;
    [SerializeField] private bool showDebugButton = true;
    [SerializeField] private Rect debugButtonRect = new Rect(24f, 24f, 260f, 56f);

    private void Start()
    {
        if (runOnStart) Run();
    }

    private void OnGUI()
    {
        if (showDebugButton && GUI.Button(debugButtonRect, "Generate Dialogue Bubble"))
            Run();
    }

    [ContextMenu("Run Generative Dialogue")]
    public void Run()
    {
        var errors = new List<string>();
        var data = GenerativeDialogueGenerator.Load(ReadLayer, errors);
        foreach (string error in errors) Debug.LogError("[GenerativeDialogue] " + error, this);

        if (npcContext == null)
        {
            Debug.LogError("[GenerativeDialogue] NPC context is missing.", this);
            return;
        }

        var result = new GenerativeDialogueGenerator().Generate(npcContext, data, seed);
        var report = new StringBuilder();
        report.AppendLine("[GenerativeDialogue] Seed: " + result.Seed);
        report.AppendLine("Formula: L1=Mood+AgeGroup; L2=Issue+Arktype; L3=Tactic+Arktype; L4=InternalSituation|WorldSituation+Arktype+Mood");
        report.AppendLine("Context: " + DescribeContext(npcContext));
        for (int i = 0; i < result.Lines.Count; i++)
        {
            report.AppendLine("Line " + (i + 1) + " [" + result.SourceLayers[i] + "]: " + result.Lines[i].dialog);
            Debug.Log("[GenerativeDialogue][Bubble][" + result.SourceLayers[i] + "] " + result.Lines[i].dialog, this);
        }
        if (result.Lines.Count == 0) report.AppendLine("Lines: none");
        if (result.FallbackMessages.Count > 0)
        {
            report.AppendLine("Fallbacks:");
            foreach (string fallback in result.FallbackMessages) report.AppendLine("- " + fallback);
        }
        Debug.Log(report.ToString(), this);
    }

    private string ReadLayer(string layerName)
    {
        TextAsset asset = layerName == "Layer 1" ? jsonFileLayer1 :
            layerName == "Layer 2" ? jsonFileLayer2 :
            layerName == "Layer 3" ? jsonFileLayer3 : jsonFileLayer4;
        if (asset == null) asset = Resources.Load<TextAsset>("Dialogue_layer" + layerName.Substring(layerName.Length - 1));
        return asset == null ? null : asset.text;
    }

    private static string DescribeContext(StructureDialogueContext context)
    {
        return "AgeGroup=" + context.ageGroup + ", Mood=" + context.currentMood +
            ", Arktype=" + context.arktypeNPC + ", Issue=" + context.currentIssue +
            ", Tactic=" + context.tactic + ", WorldSituation=" + context.worldState +
            ", InternalSituation=" + context.internalSituation;
    }
}
