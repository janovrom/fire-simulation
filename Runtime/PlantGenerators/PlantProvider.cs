using Janovrom.Firesimulation.Runtime.Plants;
using System.Collections.Generic;
using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.PlantGenerators
{
    public abstract class PlantProvider : MonoBehaviour
    {

        public Plant PlantPrefab;

        public abstract IList<Plant> GetPlants(in Vector3 min, in Vector3 max);


        public Plant GetPlant(in Vector3 position)
        {
            var plant = GameObject.Instantiate<Plant>(PlantPrefab);
            plant.transform.position = position;
            plant.transform.SetParent(this.transform);
            return plant;
        }

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
