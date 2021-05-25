using janovrom.firesimulation.Runtime.PlantGenerators;
using UnityEditor;
using UnityEngine;

namespace janovrom.firesimulation.Editor
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(GridPlantGenerator))]
    public class GridPlantGeneratorEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var generator = (GridPlantGenerator)target;
            GUILayout.Label($"Plant count: {generator.Count}");

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

    }   

}
