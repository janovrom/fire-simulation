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

    /// <summary>
    /// Data container for flammable plant. It contains state 
    /// of the plant, its burn time, and cached indices for 
    /// a grid cell this plant is in.
    /// </summary>
    /// <remarks>This could possibly be replaced by simple
    /// struct, if there is no need for searching for plants
    /// in the scene, and by a tag.</remarks>
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
