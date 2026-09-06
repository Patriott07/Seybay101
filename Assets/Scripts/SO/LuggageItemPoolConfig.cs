using System.Collections.Generic;
using Schema.data;
using UnityEngine;

[CreateAssetMenu(
    fileName = "LuggageItemPoolConfig",
    menuName = "Scriptable Objects/LuggageItemPoolConfig"
)]
public class LuggageItemPoolConfig : ScriptableObject
{
    public List<LuggageItemTemplate> normalItems;
    public List<LuggageItemTemplate> organicItems;
    public List<LuggageItemTemplate> contrabandItems; // pisau, narkotik, dll
}
