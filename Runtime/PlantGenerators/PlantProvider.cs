using System.Collections.Generic;
using UnityEngine;

namespace janovrom.firesimulation.Runtime.PlantGenerators
{
    public abstract class PlantProvider : MonoBehaviour
    {

        public abstract IList<Plants.Plant> GetPlants(in Vector3 min, in Vector3 max);

    }
}
