using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace janovrom.firesimulation.Runtime.Simulation
{
    public class FireSimulation : MonoBehaviour
    {

        private void Update()
        {
            SpreadFire();
        }

        private void SpreadFire()
        {
            UpdateFireSources();
            LightPotentialSources();
            RemoveBurnedDownPlants();

        }

    }
}
