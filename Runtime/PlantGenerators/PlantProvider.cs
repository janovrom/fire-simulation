using Janovrom.Firesimulation.Runtime.Plants;
using System.Collections.Generic;
using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.PlantGenerators
{

    /// <summary>
    /// Abstract base class that provides <see cref="Plant"/>.
    /// It can be either generated on the fly, or can grab
    /// all instances in the region. The plants created should
    /// be transformation children of the game object this 
    /// component is attached to.
    /// </summary>
    public abstract class PlantProvider : MonoBehaviour
    {

        public Plant PlantPrefab;

        /// <summary>
        /// Gets the plants based on the <see cref="PlantPrefab"/>. The region
        /// provided is not checked and the min/max should be correct.
        /// </summary>
        /// <param name="min">minimum of aabb</param>
        /// <param name="max">maximum of aabb</param>
        /// <returns>list of plants in the given region</returns>
        public abstract IList<Plant> GetPlants(in Vector3 min, in Vector3 max);

        /// <summary>
        /// Creates and returns <see cref="Plant"/> in the <paramref name="position"/>.
        /// </summary>
        /// <param name="position">position of the object</param>
        /// <returns>new instance of a plant</returns>
        public Plant GetPlant(in Vector3 position)
        {
            var plant = GameObject.Instantiate<Plant>(PlantPrefab);
            plant.transform.position = position;
            plant.transform.SetParent(this.transform);
            return plant;
        }

        /// <summary>
        /// Clears all the childen of this component.
        /// </summary>
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
