using Janovrom.Firesimulation.Runtime.Plants;
using System.Collections.Generic;
using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.PlantGenerators
{

    public class JitterPlantGenerator : PlantProvider
    {

        public int ResolutionX;
        public int ResolutionZ;
        public LayerMask CreateOn;
        public Plant PlantPrefab;

        public int Count => ResolutionX * ResolutionZ;

        public override IList<Plant> GetPlants(in Vector3 min, in Vector3 max)
        {
            Clear();
            var plants = new List<Plant>(Count);

            Vector3 size = max - min;
            var deltaX = new Vector3(size.x, 0f, 0f) / ResolutionX;
            var deltaZ = new Vector3(0f, 0f, size.z) / ResolutionZ;
            Vector3 cellCenterOffset = (deltaX + deltaZ) * 0.5f;
            for (int i = 0; i < ResolutionX; ++i)
            {
                for (int j = 0; j < ResolutionZ; ++j)
                {
                    Vector3 position = deltaX * i + deltaZ * j + min + cellCenterOffset;
                    position.y = max.y;
                    
                    Vector2 rndCircle = Random.insideUnitCircle;
                    float dx = rndCircle.x * deltaX.x * 0.45f;
                    float dz = rndCircle.y * deltaZ.z * 0.45f;
                    Vector3 jitteredPosition = position;
                    jitteredPosition.x += dx;
                    jitteredPosition.z += dz;

                    if (Physics.Raycast(jitteredPosition, Vector3.down, out RaycastHit hit, 10000f, CreateOn.value))
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
