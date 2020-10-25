using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class Helper :ScriptableObject {

    public string displayName;
    public GameObject spawnObject;
    public Sprite previewSprite;
    public int announceSoundID;

#if UNITY_EDITOR
    [MenuItem("Assets/Create/Helper")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<Helper>();
    }
#endif
}
