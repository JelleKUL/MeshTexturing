using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LocalisedImageListScriptableObject))]
public class LocalisedImageListEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(20);

        LocalisedImageListScriptableObject localisedImageList = (LocalisedImageListScriptableObject)target;
        if (GUILayout.Button("Spawn Localised Images"))
        {
            localisedImageList.SpawnImages();
        }
    }
}
