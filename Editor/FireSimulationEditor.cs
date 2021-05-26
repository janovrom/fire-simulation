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

            DrawGizmo = EditorGUILayout.Toggle("Enable grid preview: ", DrawGizmo);
            FireSimulationGizmoDrawer.EnableGizmo = DrawGizmo;
        }

    }   

}
