using System;
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

        public float BurnTime = 20f;

        [HideInInspector]
        public float TimeOnFire = 0f;

        public State State = State.Normal;

        public int IndexX = 0;
        public int IndexY = 0;

    }
}
