using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.Simulation
{

    [CreateAssetMenu(fileName ="New Simulation Settings", menuName ="FireSimulation/Simulation Settings")]
    public class SimulationSettings : ScriptableObject
    {

        public PlantGenerators.PlantProvider Generator;

    }
}
