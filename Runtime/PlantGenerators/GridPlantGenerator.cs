using janovrom.firesimulation.Runtime.Plants;
using System.Collections.Generic;
using UnityEngine;

namespace janovrom.firesimulation.Runtime.PlantGenerators
{

    public class GridPlantGenerator : PlantProvider
    {

        public int ResolutionX;
        public int ResolutionZ;
        public LayerMask CreateOn;
        public Plant PlantPrefab;

        public int Count => ResolutionX * ResolutionZ;

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

        public override IList<Plant> GetPlants(in Vector3 min, in Vector3 max)
        {
            Clear();
            var plants = new List<Plant>(Count);

            Vector3 size = max - min;
            var deltaX = new Vector3(size.x, 0f, 0f) / ResolutionX;
            var deltaZ = new Vector3(0f, 0f, size.z) / ResolutionZ;
            for (int i = 0; i < ResolutionX; ++i)
            {
                for (int j = 0; j < ResolutionZ; ++j)
                {
                    Vector3 position = deltaX * i + deltaZ * j + min;
                    position.y = max.y;
                    Debug.DrawRay(position, Vector3.down * size.y, Color.red, 10f);
                    if (Physics.Raycast(position, Vector3.down, out RaycastHit hit, 10000f, CreateOn.value))
                    {
                        var plant = GameObject.Instantiate(PlantPrefab, hit.point, Quaternion.identity);
                        plant.transform.SetParent(this.transform);
                        plants.Add(plant);
                    }
                }
            }

            return plants;
        }

    }
}
