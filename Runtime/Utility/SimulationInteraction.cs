﻿using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.Utility
{

    [RequireComponent(typeof(Simulation.FireSimulation))]
    public class SimulationInteraction : MonoBehaviour
    {

        private const int _AddMode = 0;
        private const int _FireMode = 1;
        private const int _RemoveMode = 2;

        public LayerMask PlaceLayer;
        public LayerMask FireLayer;
        public Vector3Variable CrosshairPosition;
        public IntVariable SimulationMode;

        private Simulation.FireSimulation _fireSimulation;
        private Camera _camera;

        private void Awake()
        {
            _fireSimulation = GetComponent<Simulation.FireSimulation>();
            _camera = Camera.main;
        }

        private void Update()
        {
            if (SimulationMode.Value == _AddMode) TryAddPlant();
            else if (SimulationMode.Value == _FireMode) TryLightFire();
            else if (SimulationMode.Value == _RemoveMode) TryRemovePlant();
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