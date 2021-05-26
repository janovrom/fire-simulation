using System.Collections.Generic;
using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.PlantGenerators
{
    public abstract class PlantProvider : MonoBehaviour
    {

        public abstract IList<Plants.Plant> GetPlants(in Vector3 min, in Vector3 max);

        public void Clear()
        {
            for (int i = transform.childCount - 1; i >= 0; --i)
            {
#if UNITY_EDITOR
                DestroyImmediate(transform.GetChild(i).gameObject);
#else
                Destroy(transform.GetChild(i).gameObject);
#endif
            }
        }

    }
}
