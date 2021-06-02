using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.Utility
{

    [RequireComponent(typeof(Simulation.FireSimulation))]
    public class SimulationInteraction : MonoBehaviour
    {

        public LayerMask InteractionLayer;
        public Vector3Variable CrosshairPosition;

        private Simulation.FireSimulation _fireSimulation;

        void Awake()
        {
            _fireSimulation = GetComponent<Simulation.FireSimulation>();
        }

        void Update()
        {
            AddPlant();
        }

        private void AddPlant()
        {
            bool clickCheck = Input.GetMouseButtonUp(0);
            if (clickCheck && CrosshairPosition is object)
            {
                _fireSimulation.AddPlant(CrosshairPosition.Value);
            }
        }

    }

}