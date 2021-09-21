using UnityEngine;
using UnityEditor;
using System;

namespace JelleKUL.MeshMapping
{
    [CustomEditor(typeof(DepthMapper))]
    public class DepthMapperEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Space(20);

            DepthMapper depthMapper = (DepthMapper)target;
            if (GUILayout.Button("Render Depth"))
            {
                DateTime before = DateTime.Now;

                depthMapper.RenderDepth();

                // log the time it took to complete the function
                DateTime after = DateTime.Now;
                TimeSpan duration = after.Subtract(before);
                Debug.Log("Duration in milliseconds: " + duration.Milliseconds);
            }
        }
    }
}