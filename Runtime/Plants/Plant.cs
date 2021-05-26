using Janovrom.Firesimulation.Runtime.Renderers;
using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.Plants
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
