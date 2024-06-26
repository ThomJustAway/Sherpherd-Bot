﻿using Assets.Scripts.Shepherd.GOAP;
using UnityEditor;
using UnityEngine;

namespace GOAPTHOM
{
    [CustomEditor(typeof(Shepherd))]
    public class ShepherdInsepctor : Editor
    {
        public override void OnInspectorGUI()
        {
            Shepherd shepherd = (Shepherd)target;
            GoapAgent agent = shepherd.agent;
            EditorGUILayout.Space();
            DrawDefaultInspector();

            EditorGUILayout.Space();
            if (!shepherd.UseBehaviourTree)
            {
                DrawGOAP(agent);

            }
        }

        private static void DrawGOAP(GoapAgent agent)
        {
            if (agent.currentGoal != null)
            {
                EditorGUILayout.LabelField("Current Goal:", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.LabelField(agent.currentGoal.Name);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            // Show current Action
            if (agent.currentAction != null)
            {
                EditorGUILayout.LabelField("Current Action:", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.LabelField(agent.currentAction.Name);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            // Show current plan
            if (agent.actionPlan != null)
            {
                EditorGUILayout.LabelField("Plan Stack:", EditorStyles.boldLabel);
                foreach (var a in agent.actionPlan.Actions)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField(a.Name);
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space();

            // Show beliefs
            EditorGUILayout.LabelField("Beliefs:", EditorStyles.boldLabel);
            if (agent.beliefs != null)
            {
                foreach (var belief in agent.beliefs)
                {
                    if (belief.Key is "Nothing" or "Something") continue;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField(belief.Key + ": " + belief.Value.Evaluate());
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space();
        }
    }
}