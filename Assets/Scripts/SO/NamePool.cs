using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NamePool", menuName = "Scriptable Objects/NamePool")]
public class NamePool : ScriptableObject
{
    public List<string> maleNames;
    public List<string> femaleNames;
}
