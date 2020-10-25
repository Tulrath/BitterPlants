using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Trinket))]
public class TrinketEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Trinket myTarget = (Trinket)target;
        

        EditorGUILayout.LabelField("Trinket Display Name:");
        myTarget.displayName = EditorGUILayout.TextField(myTarget.displayName);

        Object spriteObj = EditorGUILayout.ObjectField(myTarget.sprite, typeof(Sprite), false);
        myTarget.sprite = (Sprite)spriteObj;

        Texture2D myTexture = AssetPreview.GetAssetPreview(spriteObj);

        GUILayout.Label(myTexture);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(myTarget);
        }


    }

}
