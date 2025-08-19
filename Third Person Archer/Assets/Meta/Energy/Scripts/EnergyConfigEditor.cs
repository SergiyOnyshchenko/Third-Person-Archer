#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Meta.Energy.Editor
{
    [CustomEditor(typeof(EnergyConfig))]
    public sealed class EnergyConfigEditor : UnityEditor.Editor
    {
        private bool _showPerType = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var cfg = (EnergyConfig)target;

            // Draw all serialized fields first
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            int max = cfg.MaxEnergy;
            int start = cfg.StartingEnergy;
            int interval = cfg.RegenIntervalSeconds;
            int adAmt = cfg.AdRefillAmount;
            int adMax = cfg.MaxAdRefillsPerDay;

            // Global preview
            using (new EditorGUILayout.VerticalScope("box"))
            {
                var timeToFull = TimeSpan.FromSeconds((double)max * interval);
                EditorGUILayout.LabelField("Time to Full from Empty",
                    $"{(int)timeToFull.TotalHours:D2}:{timeToFull.Minutes:D2}:{timeToFull.Seconds:D2}");
                EditorGUILayout.LabelField("Max Ad Energy per Day", $"{adAmt * adMax}");
                EditorGUILayout.LabelField("Starting Energy", $"{start}/{max}");
            }

            // Per-mission-type preview
            _showPerType = EditorGUILayout.Foldout(_showPerType, "Per-Mission-Type Pacing");
            if (_showPerType)
            {
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    DrawPerTypeHeader();

                    var types = (MissionType[])Enum.GetValues(typeof(MissionType));
                    foreach (var t in types)
                    {
                        int cost = Mathf.Max(0, cfg.GetCostForType(t)); // 0 => free
                        DrawPerTypeRow(
                            t.ToString(),
                            cost,
                            attemptsFromFull: cost > 0 ? (max / cost) : int.MaxValue,
                            adAttempts: cost > 0 ? (adAmt / cost) : int.MaxValue,
                            perAttemptCooldownSec: cost * interval
                        );
                    }
                }

                EditorGUILayout.HelpBox(
                    "Notes:\n" +
                    "• Attempts from full = MaxEnergy / Cost(type)\n" +
                    "• Ad attempts = AdRefillAmount / Cost(type), clamped by daily cap\n" +
                    "• Per-attempt cooldown (empty) = Cost(type) × RegenInterval",
                    MessageType.Info);
            }

            // Validation messages from config
            var errors = cfg.GetValidationErrors();
            if (errors != null && errors.Length > 0)
            {
                EditorGUILayout.Space(10);
                foreach (var e in errors)
                    EditorGUILayout.HelpBox(e, MessageType.Warning);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawPerTypeHeader()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Mission Type", EditorStyles.miniBoldLabel, GUILayout.Width(140));
                EditorGUILayout.LabelField("Cost", EditorStyles.miniBoldLabel, GUILayout.Width(60));
                EditorGUILayout.LabelField("Attempts (Full)", EditorStyles.miniBoldLabel, GUILayout.Width(110));
                EditorGUILayout.LabelField("Ad Attempts", EditorStyles.miniBoldLabel, GUILayout.Width(100));
                EditorGUILayout.LabelField("Cooldown / Attempt", EditorStyles.miniBoldLabel);
            }
            var rect = GUILayoutUtility.GetRect(1, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(1,1,1,0.15f));
        }

        private static void DrawPerTypeRow(
            string typeName,
            int cost,
            int attemptsFromFull,
            int adAttempts,
            int perAttemptCooldownSec)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(typeName, GUILayout.Width(140));

                // Cost
                EditorGUILayout.LabelField(cost <= 0 ? "Free" : cost.ToString(), GUILayout.Width(60));

                // Attempts from full
                EditorGUILayout.LabelField(cost <= 0 ? "∞" : attemptsFromFull.ToString(), GUILayout.Width(110));

                // Attempts per ad
                EditorGUILayout.LabelField(cost <= 0 ? "∞" : adAttempts.ToString(), GUILayout.Width(100));

                // Per-attempt cooldown (mm:ss)
                if (cost <= 0)
                {
                    EditorGUILayout.LabelField("—");
                }
                else
                {
                    int m = perAttemptCooldownSec / 60;
                    int s = perAttemptCooldownSec % 60;
                    EditorGUILayout.LabelField($"{m:00}:{s:00}");
                }
            }
        }
    }
}
#endif