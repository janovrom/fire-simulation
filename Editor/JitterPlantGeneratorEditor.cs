using Janovrom.Firesimulation.Runtime.PlantGenerators;
using UnityEditor;
using UnityEngine;

namespace Janovrom.Firesimulation.Editor
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(JitterPlantGenerator))]
    public class JitterPlantGeneratorEditor : PlantGeneratorEditorBase
    {

        protected override void OnInspectorGUI_Internal()
        {
            var generator = (JitterPlantGenerator)target;
            GUILayout.Label($"Plant count: {generator.Count}");
        }

    }   

}
