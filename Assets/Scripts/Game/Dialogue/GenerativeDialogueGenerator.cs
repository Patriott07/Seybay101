using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Schema.data;

/// <summary>
/// Deterministically resolves the four dialogue layers for one NPC context.
/// This class has no Unity scene, UI, economy, or persistence dependency.
/// </summary>
public sealed class GenerativeDialogueGenerator
{
    public sealed class DialogueData
    {
        public DialogueDatabaseLayer1 Layer1;
        public DialogueDatabaseLayer2 Layer2;
        public DialogueDatabaseLayer3 Layer3;
        public DialogueDatabaseLayer4 Layer4;
    }

    public sealed class DialogueResult
    {
        public DialogueResult(int seed)
        {
            Seed = seed;
        }

        public readonly List<DialogueLine> Lines = new List<DialogueLine>();
        public readonly List<string> SourceLayers = new List<string>();
        public readonly List<string> FallbackMessages = new List<string>();
        public int Seed { get; private set; }
    }

    public static DialogueData Load(TextAssetReader reader, IList<string> validationErrors)
    {
        var data = new DialogueData();
        data.Layer1 = Read(reader, "Layer 1", validationErrors, text => UnityEngine.JsonUtility.FromJson<DialogueDatabaseLayer1>(text), x => x.layerExpresion, "layerExpresion");
        data.Layer2 = Read(reader, "Layer 2", validationErrors, text => UnityEngine.JsonUtility.FromJson<DialogueDatabaseLayer2>(text), x => x.layerData, "layerData");
        data.Layer3 = Read(reader, "Layer 3", validationErrors, text => UnityEngine.JsonUtility.FromJson<DialogueDatabaseLayer3>(text), x => x.layerData, "layerData");
        data.Layer4 = Read(reader, "Layer 4", validationErrors, text => UnityEngine.JsonUtility.FromJson<DialogueDatabaseLayer4>(text), x => x.layerData, "layerData");
        return data;
    }

    public DialogueResult Generate(StructureDialogueContext context, DialogueData data, int seed)
    {
        var result = new DialogueResult(seed);
        var random = new Random(seed);
        if (context == null)
        {
            result.FallbackMessages.Add("Context is null; no dialogue was generated.");
            return result;
        }

        AddLayer1(context, data == null ? null : data.Layer1, random, result);
        AddLayer2(context, data == null ? null : data.Layer2, random, result);
        AddLayer3(context, data == null ? null : data.Layer3, random, result);
        AddLayer4(context, data == null ? null : data.Layer4, random, result);
        return result;
    }

    private static void AddLayer1(StructureDialogueContext context, DialogueDatabaseLayer1 database, Random random, DialogueResult result)
    {
        if (database == null || database.layerExpresion == null || database.layerExpresion.Count == 0)
        {
            result.FallbackMessages.Add("Layer 1 unavailable; skipped.");
            return;
        }

        string requestedExpression = MoodKey(context.currentMood);
        ExpressionData expression = database.layerExpresion.FirstOrDefault(x => Key(x == null ? null : x.expressionName) == requestedExpression);
        if (expression == null)
        {
            expression = database.layerExpresion.FirstOrDefault(x => x != null && HasLines(x.ageGroups));
            result.FallbackMessages.Add("Layer 1 expression '" + requestedExpression + "' not found; used first available expression.");
        }

        AgeGroupData ageGroup = expression == null ? null : expression.ageGroups?.FirstOrDefault(x => Key(x == null ? null : x.ageGroupName) == Key(context.ageGroup.ToString()));
        if (ageGroup == null && expression != null)
        {
            ageGroup = expression.ageGroups?.FirstOrDefault(x => x != null && HasLines(x.lines));
            result.FallbackMessages.Add("Layer 1 age group '" + context.ageGroup + "' not found; used first available age group.");
        }
        AddLine(ageGroup == null ? null : ageGroup.lines, "Layer 1", random, result);
    }

    private static void AddLayer2(StructureDialogueContext context, DialogueDatabaseLayer2 database, Random random, DialogueResult result)
    {
        string issueKey = IssueKey(context.currentIssue);
        string arktypeKey = ArktypeKey(context.arktypeNPC);
        Layer2Entry entry = database?.layerData?.FirstOrDefault(x => Key(x == null ? null : x.issueID) == issueKey);
        if (entry == null)
        {
            result.FallbackMessages.Add("Layer 2 issue '" + issueKey + "' not found; skipped.");
            return;
        }
        AgeGroupData2 group = entry.expressions?
            .Where(x => x != null && x.ageGroups != null)
            .SelectMany(x => x.ageGroups)
            .FirstOrDefault(x => Key(x?.ageGroupName) == arktypeKey);
        if (group == null) result.FallbackMessages.Add("Layer 2 arktype '" + arktypeKey + "' not found; skipped.");
        AddLine(group?.lines, "Layer 2", random, result);
    }

    private static void AddLayer3(StructureDialogueContext context, DialogueDatabaseLayer3 database, Random random, DialogueResult result)
    {
        string tacticKey = TacticKey(context.tactic);
        string arktypeKey = ArktypeKey(context.arktypeNPC);
        Layer3Entry entry = database?.layerData?.FirstOrDefault(x => Key(x?.tacticName) == tacticKey);
        if (entry == null)
        {
            result.FallbackMessages.Add("Layer 3 tactic '" + tacticKey + "' not found; skipped.");
            return;
        }
        ArktypeData3 group = entry.arktypes?.FirstOrDefault(x => Key(x?.arktype) == arktypeKey);
        if (group == null) result.FallbackMessages.Add("Layer 3 arktype '" + arktypeKey + "' not found; skipped.");
        AddLine(group?.lines, "Layer 3", random, result);
    }

    private static void AddLayer4(StructureDialogueContext context, DialogueDatabaseLayer4 database, Random random, DialogueResult result)
    {
        string situationKey = Key(context.internalSituation.ToString());
        Layer4Entry entry = database?.layerData?.FirstOrDefault(x => Key(x?.situationName) == situationKey);
        if (entry == null)
        {
            situationKey = Key(context.worldState.ToString());
            entry = database?.layerData?.FirstOrDefault(x => Key(x?.situationName) == situationKey);
        }
        if (entry == null)
        {
            result.FallbackMessages.Add("Layer 4 situations for internal/world context not found; skipped.");
            return;
        }
        ArktypeData4 arktype = entry.arktypes?.FirstOrDefault(x => Key(x?.arktype) == ArktypeKey(context.arktypeNPC));
        MoodData4 mood = arktype?.moods?.FirstOrDefault(x => Key(x?.moodName) == MoodKey(context.currentMood));
        if (mood == null) mood = arktype?.moods?.FirstOrDefault(x => x != null && HasLines(x.lines));
        if (arktype == null || mood == null) result.FallbackMessages.Add("Layer 4 arktype/mood was not found; skipped.");
        AddLine(mood?.lines, "Layer 4", random, result);
    }

    private static void AddLine(List<DialogueLine> lines, string source, Random random, DialogueResult result)
    {
        if (!HasLines(lines)) return;
        List<DialogueLine> validLines = lines.Where(x => x != null && !string.IsNullOrWhiteSpace(x.dialog)).ToList();
        if (validLines.Count == 0) return;
        DialogueLine line = validLines[random.Next(validLines.Count)];
        if (line != null)
        {
            result.Lines.Add(line);
            result.SourceLayers.Add(source);
        }
    }

    private static bool HasLines<T>(ICollection<T> values) where T : class => values != null && values.Count > 0;
    private static string Key(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();
    private static string IssueKey(Issues value) => value == Issues.Invalid_Passport ? "EXPIRED_PERMIT" : Key(value.ToString());
    private static string ArktypeKey(Arktype value)
    {
        switch (value)
        {
            case Arktype.ORDINARY_CITIZEN: return "CIVILIAN";
            case Arktype.SICK_PERSON: case Arktype.CHILD: return "THE_DEPENDENT";
            case Arktype.SCIENTIST: return "ACADEMIA";
            case Arktype.WORKER: return "THE_WORKER";
            case Arktype.AUDIT_AZURA: return "THE_ENFORCER";
            case Arktype.THE_UNDERGROUND: return "THE_MARGINAL";
            default: return Key(value.ToString());
        }
    }
    private static string TacticKey(Tactic value)
    {
        switch (value)
        {
            case Tactic.Honest: return "COOPERATE";
            case Tactic.Pretending_BeConfused: return "LIE";
            case Tactic.GuiltTrip: case Tactic.Begging: return "BEG";
            case Tactic.Intimidation: case Tactic.Force: return "THREATEN";
            default: return Key(value.ToString());
        }
    }
    private static string MoodKey(Mood value)
    {
        if (value == Mood.Scare) return "FEARFUL";
        return Key(value.ToString());
    }

    public delegate T TextAssetReader<T>(string text);
    public delegate string TextAssetReader(string layerName);

    private static T Read<T>(TextAssetReader reader, string layerName, IList<string> errors, Func<string, T> parse, Func<T, ICollection> getCollection, string collectionName) where T : class
    {
        string text = reader == null ? null : reader(layerName);
        if (string.IsNullOrWhiteSpace(text)) { errors.Add(layerName + " TextAsset is missing or empty."); return null; }
        try
        {
            T value = parse(text);
            if (value == null) errors.Add(layerName + " JSON parsed to null.");
            else if (getCollection(value) == null || getCollection(value).Count == 0)
                errors.Add(layerName + " JSON is valid but '" + collectionName + "' is missing or empty.");
            return value;
        }
        catch (Exception exception) { errors.Add(layerName + " JSON is invalid: " + exception.Message); return null; }
    }
}
