using UnityEngine;
using UnityEditor;

namespace JelleKUL.MeshMapping
{
    [CustomEditor(typeof(LocalisedImageScriptableObject))]
    public class LocalisedImageEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Space(20);

            LocalisedImageScriptableObject localisedImage = (LocalisedImageScriptableObject)target;
            if (GUILayout.Button("Create Image Material"))
            {
                // Create a material with unlit/texture
                Material material = new Material(Shader.Find("Unlit/Texture"));
                material.mainTexture = localisedImage.image;

                AssetDatabase.CreateAsset(material, "Assets/Materials/" + localisedImage.name + "_mat.mat");

                localisedImage.material = material;
            }
        }
    }
}
