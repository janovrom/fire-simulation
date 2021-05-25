using UnityEngine;

namespace janovrom.firesimulation.Runtime.Plants
{
    [CreateAssetMenu(fileName ="New Flammable Data", menuName ="FireSimulation/Flammable Data")]
    public class FlammableData : ScriptableObject
    {

        public float FireTemperature;
        public float BurnTime;

    }
}
