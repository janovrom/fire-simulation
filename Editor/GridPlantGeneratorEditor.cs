using Janovrom.Firesimulation.Runtime.PlantGenerators;
using UnityEditor;
using UnityEngine;

namespace Janovrom.Firesimulation.Editor
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(GridPlantGenerator))]
    public class GridPlantGeneratorEditor : PlantGeneratorEditorBase
    {

        protected override void OnInspectorGUI_Internal()
        {
            var generator = (GridPlantGenerator)target;
            GUILayout.Label($"Plant count: {generator.Count}");
        }

    }   

}
