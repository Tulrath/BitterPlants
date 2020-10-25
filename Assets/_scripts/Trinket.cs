using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class Trinket : ScriptableObject
{
    public string displayName;
    public Sprite sprite;
    public int level;

#if UNITY_EDITOR
    [MenuItem("Assets/Create/Trinket")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<Trinket>();
    }
#endif
}
