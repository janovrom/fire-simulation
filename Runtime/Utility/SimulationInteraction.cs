using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.Utility
{

    [RequireComponent(typeof(Simulation.FireSimulation))]
    public class SimulationInteraction : MonoBehaviour
    {

        private const int AddMode = 0;
        private const int FireMode = 1;
        private const int RemoveMode = 2;

        public LayerMask PlaceLayer;
        public LayerMask FireLayer;
        public Vector3Variable CrosshairPosition;

        private Simulation.FireSimulation _fireSimulation;
        private Camera _camera;
        private int _mode = 0;


        public void ChangeMode(int newMode)
        {
            _mode = newMode;
        }

        void Awake()
        {
            _fireSimulation = GetComponent<Simulation.FireSimulation>();
            _camera = Camera.main;
        }

        void Update()
        {
            if (_mode == AddMode) TryAddPlant();
            else if (_mode == FireMode) TryLightFire();
            else if (_mode == RemoveMode) TryRemovePlant();
        }

        private void TryAddPlant()
        {
            bool clickCheck = Input.GetMouseButtonUp(0);
            if (clickCheck && CrosshairPosition is object)
            {
                _fireSimulation.AddPlant(CrosshairPosition.Value);
            }
        }

        private void TryLightFire()
        {
            if (Input.GetMouseButtonUp(0))
            {
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100000f, FireLayer))
                {
                    _fireSimulation.LightFire(hit.collider.gameObject);
                }
            }
        }

        private void TryRemovePlant()
        {
            if (Input.GetMouseButtonUp(0))
            {
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100000f, FireLayer))
                {
                    _fireSimulation.RemovePlant(hit.collider.gameObject);
                }
            }
        }

    }

}