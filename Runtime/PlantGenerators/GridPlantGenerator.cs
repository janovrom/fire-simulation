using Janovrom.Firesimulation.Runtime.Plants;
using System.Collections.Generic;
using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.PlantGenerators
{

    /// <summary>
    /// Generates plants in a grid. The plants are centered
    /// on each grid cell. The number of cells can be specified.
    /// </summary>
    public class GridPlantGenerator : PlantProvider
    {

        public int ResolutionX;
        public int ResolutionZ;
        public LayerMask CreateOn;

        public int Count => ResolutionX * ResolutionZ;

        /// <summary>
        /// <inheritdoc/>
        /// The plants are created in a grid with each plant centered 
        /// on the cell. The resolution is specified by <see cref="ResolutionX"/>
        /// and <see cref="ResolutionY"/>. The plants are created only if the ray
        /// from the top to bottom of the min/max hits an object in layer 
        /// <see cref="CreateOn"/>.
        /// </summary>
        /// <returns><inheritdoc/></returns>
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
