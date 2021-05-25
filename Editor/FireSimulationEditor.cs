using janovrom.firesimulation.Runtime.Simulation;
using UnityEditor;
using UnityEngine;

namespace janovrom.firesimulation.Editor
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(FireSimulation))]
    public class FireSimulationEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var simulation = (FireSimulation)target;
        }

    }   

}
