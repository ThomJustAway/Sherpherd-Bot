using System.Collections;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

namespace Sheep
{
    [CustomEditor(typeof(SheepFlock))]
    public class SheepFlockInspector : Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var currentObject = target as SheepFlock;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Flocks data:", EditorStyles.boldLabel);
            GUILayout.Space(10);
            EditorGUILayout.LabelField($"Total food: {currentObject.flocksaturation}");
            EditorGUILayout.LabelField($"Total water: {currentObject.flockHydration}");
            EditorGUILayout.LabelField($"Total wool: {currentObject.flockWool}");
        }
    }
}