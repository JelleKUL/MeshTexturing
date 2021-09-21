using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshMappingController))]
public class MeshMappingEditor : Editor
{
    private string arraySavePath = "Assets/Images/texturearray";

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(20);

        MeshMappingController meshMapper = (MeshMappingController)target;

        

        if(GUILayout.TextField(arraySavePath) != "")
        {

        }
        if (GUILayout.Button("Create TextureArray"))
        {
            Texture2DArray array = meshMapper.SetTextureArray();
            if (arraySavePath != "") AssetDatabase.CreateAsset(array, arraySavePath + ".tarr");
        }
    }
}
