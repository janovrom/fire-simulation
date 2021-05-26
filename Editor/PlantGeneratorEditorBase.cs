using Janovrom.Firesimulation.Runtime.PlantGenerators;
using UnityEditor;
using UnityEngine;

namespace Janovrom.Firesimulation.Editor
{

    public abstract class PlantGeneratorEditorBase : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var generator = (PlantProvider)target;

            OnInspectorGUI_Internal();

            if (GUILayout.Button("Clear"))
            {
                generator?.Clear();
            }

            if (GUILayout.Button("Generate"))
            {
                var simulation = generator.GetComponent<Runtime.Simulation.FireSimulation>();
                if (simulation is null)
                    generator?.GetPlants(Vector3.zero, Vector3.one * 500f);
                else
                    generator?.GetPlants(simulation.SimulationBounds.min, simulation.SimulationBounds.max);
            }
        }

        protected virtual void OnInspectorGUI_Internal()
        {
        }

    }   

}
