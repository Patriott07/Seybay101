using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChatConversation
{
    [Tooltip("Daftar pesan dalam satu sesi obrolan (NPC & Aldo)")]
    public List<ContentChat> chatLines = new List<ContentChat>();
}

[CreateAssetMenu(fileName = "NewChatTopic", menuName = "Seybey/Chat Topic")]
public class ChatTopic : ScriptableObject
{
    [Header("Info Topik")]
    public string topicName;
    [Range(0f, 1f)] public float chanceToSpeak = 0.5f;

    [Header("Daftar Percakapan")]
    public List<ChatConversation> messages = new List<ChatConversation>();

    public List<ContentChat> GetRandomMessage()
    {
        if (messages == null || messages.Count == 0) return null;
        int index = Random.Range(0, messages.Count);
        return messages[index].chatLines;
    }

    public bool ShouldSpeak()
    {
        return Random.Range(0f, 1f) <= chanceToSpeak;
    }
}