using Janovrom.Firesimulation.Runtime.Simulation;
using UnityEditor;
using UnityEngine;

namespace Janovrom.Firesimulation.Editor
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(FireSimulation))]
    public class FireSimulationEditor : UnityEditor.Editor
    {

        public bool DrawGizmo = true;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            var simulation = target as FireSimulation;
            DrawGizmo = EditorGUILayout.Toggle("Enable grid preview: ", DrawGizmo);
            float windAngle = simulation.WindAngleNormalized * Mathf.PI * 2f;
            var windDirection = new Vector3(Mathf.Cos(windAngle), 0f, Mathf.Sin(windAngle));
            EditorGUILayout.LabelField("Wind direction=" + windDirection.ToString());
            EditorGUILayout.LabelField("Wind angle radians=" + windAngle);
            EditorGUILayout.LabelField("Wind angle degrees=" + (simulation.WindAngleNormalized * 360f));
            FireSimulationGizmoDrawer.EnableGizmo = DrawGizmo;
        }

    }   

}
