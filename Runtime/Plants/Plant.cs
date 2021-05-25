using janovrom.firesimulation.Runtime.Renderers;
using UnityEngine;

namespace janovrom.firesimulation.Runtime.Plants
{

    public enum State
    {
        Normal,
        OnFire,
        Burned
    }

    public class Plant : MonoBehaviour
    {

        public FlammableData Data;
        [HideInInspector]
        public float TimeOnFire = 0f;
        public State State = State.Normal;
    }
}
