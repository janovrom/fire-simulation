using Janovrom.Firesimulation.Runtime.PlantGenerators;
using Janovrom.Firesimulation.Runtime.Plants;
using Janovrom.Firesimulation.Runtime.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.Simulation
{
    public class FireSimulation : MonoBehaviour
    {

        public PlantProvider PlantGenerator;
        public PlantBasicRenderer Renderer;

        public int ResolutionX = 256;
        public int ResolutionY = 256;
        [Range(0f, 1f)]
        public float HeatTransferSpeed = 1f;
        [Range(0f, 100f)]
        public float WindSpeed = 0f;
        public Vector3 WindDirection = Vector3.right;
        public Bounds SimulationBounds;

        private PlantList _plantList;

        private float[,] _fireSourceGrid;
        private float[,] _heatTransferGrid;

        private float _deltaXDistance, _deltaZDistance, _deltaDistance;

        private Vector3 _min;
        private bool _isSimulationRunning = false;

        private const float _fireTemperature = 1200f;
        private const float _flashpointTemperature = 600f;
        private const int _loopStart = 1;
        private const int _padding = _loopStart * 2;

        public void AddPlant(Vector3 position)
        {
            if (_plantList is null)
                return;

            Plant plant = PlantGenerator.GetPlant(in position);
            Renderer.Register(plant);
            _plantList.AddPlant(plant);
            CacheIndices(plant);
        }

        public void RemovePlant(Plant plant)
        {
            if (_plantList is null)
                return;

            Renderer.Unregister(plant);
            _plantList.RemovePlant(plant);
        }

        public void ClearPlants()
        {
            PauseSimulation();

            if (Renderer is object)
                Renderer.Clear();

            if (PlantGenerator is object)
                PlantGenerator.Clear();

            _plantList = null;
        }

        public void GeneratePlants()
        {
            if (PlantGenerator is null)
                return;

            PauseSimulation();
            ClearPlants();

            _plantList = new PlantList(PlantGenerator.GetPlants(SimulationBounds.min, SimulationBounds.max));
            var plantToLightIdx = UnityEngine.Random.Range(0, _plantList.Count);

            _plantList.LightPlant(plantToLightIdx);

            foreach (var plant in _plantList)
            {
                Renderer.Register(plant);
            }
        }

        public void PauseSimulation()
        {
            _isSimulationRunning = false;
        }

        public void StartSimulation()
        {
            if (PlantGenerator is null)
                return;

            if (_plantList is null)
                GeneratePlants();

            InitializeGrid();
            CacheIndices();

            _isSimulationRunning = true;
        }

        private void InitializeGrid()
        {
            _fireSourceGrid = new float[ResolutionX + _padding, ResolutionY + _padding];
            _heatTransferGrid = new float[ResolutionX + _padding, ResolutionY + _padding];

            _deltaXDistance = SimulationBounds.size.x / ResolutionX;
            _deltaZDistance = SimulationBounds.size.z / ResolutionY;
            _deltaDistance = Mathf.Sqrt(_deltaXDistance * _deltaXDistance + _deltaZDistance * _deltaZDistance);
            // Lock minimum for editing and faster access
            _min = SimulationBounds.min;
        }

        private void CacheIndices()
        {
            foreach (var plant in _plantList)
            {
                CacheIndices(plant);
            }
        }

        private void CacheIndices(Plant plant)
        {
            GetIndices(plant, out int x, out int y);
            plant.IndexX = x;
            plant.IndexY = y;
        }

        private void Update()
        {
            if (!_isSimulationRunning)
                return;

            float simulationDeltaTime = Time.deltaTime;
            SetFireTemperatures();
            FireSourceRadiation(simulationDeltaTime * HeatTransferSpeed);
            ApplyWind(simulationDeltaTime * HeatTransferSpeed);
            UpdateBurningTimes(simulationDeltaTime);
            UpdateTimesBeforeFlashpoint();
            Renderer.Render();
        }

        private void OnDisable()
        {
            _isSimulationRunning = false;
        }

        private void GetIndices(Plant plant, out int x, out int y)
        {
            Vector3 pos = plant.transform.position;
            x = (int)((pos.x - _min.x) / _deltaXDistance) + 1;
            y = (int)((pos.z - _min.z) / _deltaZDistance) + 1;
        }

        private float GetLocalTemperature_NNCached(Plant plant)
        {
            return _heatTransferGrid[plant.IndexX, plant.IndexY];
        }

        private float GetLocalTemperature_NN(Plant plant)
        {
            GetIndices(plant, out int x, out int y);
            return _heatTransferGrid[x, y];
        }

        private float GetLocalTemperature(Plant plant)
        {
            Vector3 pos = plant.transform.position;
            // x,y correspond to the bottom corner of the grid cell
            float x = ((pos.x - _min.x) / _deltaXDistance) + 1;
            float y = ((pos.z - _min.z) / _deltaZDistance) + 1;

            float mixx = x % 1f;
            float mixy = y % 1f;
            int ix = (int)x;
            int iy = (int)y;

            float x0 = mixx * _heatTransferGrid[ix + 1, iy] + (1f - mixx) * _heatTransferGrid[ix, iy];
            float x1 = mixx * _heatTransferGrid[ix + 1, iy + 1] + (1f - mixx) * _heatTransferGrid[ix, iy + 1];

            return mixy * x1 + (1f - mixy) * x0;
        }

        private void UpdateTimesBeforeFlashpoint()
        {
            int start = _plantList.ActivePlantsStart;
            int end = _plantList.ActivePlantsEnd;
            for (int i =  start; i < end; ++i)
            {
                Plant plant = _plantList[i];
                var currentLocalTemperature = GetLocalTemperature_NNCached(plant);

                // Let's say flashpoint temperature is 600°C and let's ignore the heat transfer
                // from air to the wood.
                if (currentLocalTemperature > _flashpointTemperature)
                {
                    // Light the fire
                    _plantList.LightPlant(i);
                    Renderer.NotifyStateChange(plant);
                }
            }
        }

        private void UpdateBurningTimes(float simulationDeltaTime)
        {
            int start = _plantList.BurningPlantsCount - 1;
            for (int i = start; i >= 0; --i)
            {
                Plant plant = _plantList[i];
                plant.TimeOnFire += simulationDeltaTime;
                if (plant.BurnTime < plant.TimeOnFire)
                {
                    _plantList.BurnDownPlant(i);
                    Renderer.NotifyStateChange(plant);
                }
            }
        }

        private void ApplyMask()
        {
        }

        private void SetFireTemperatures()
        {
            for (int x = 0; x < ResolutionX + _padding; ++x)
            {
                for (int y = 0; y < ResolutionY + _padding; ++y)
                {
                    _fireSourceGrid[x, y] = 0f;
                }
            }

            int end = _plantList.BurningPlantsCount;
            for (int i = 0; i < end; ++i)
            {
                Plant plant = _plantList[i];
                GetIndices(plant, out int x, out int y);
                _fireSourceGrid[x, y] = Mathf.Max(_fireSourceGrid[x, y], _fireTemperature);
            }
        }

        private void Dissipate(float deltaTime)
        {
            for (int x = 1; x <= ResolutionX; ++x)
            {
                for (int y = 1; y <= ResolutionY; ++y)
                {
                    _heatTransferGrid[x, y] = Mathf.Max(0f, _heatTransferGrid[x, y] - deltaTime * 0.1f);
                }
            }
        }

        private void ApplyWind(float deltaTime)
        {
            Vector3 heatPropagation = WindDirection.normalized * WindSpeed + Vector3.up;
            heatPropagation.Normalize();
            int ix = (int)Mathf.Sign(heatPropagation.x) * (Mathf.Abs(heatPropagation.x) > 0.707f ? 1 : 0);
            int iz = (int)Mathf.Sign(heatPropagation.z) * (Mathf.Abs(heatPropagation.z) > 0.707f ? 1 : 0);

            if (ix == 0 && iz == 0)
            {
                // When there is no wind or the wind is not strong enough, we can 
                // assume that the heat propagates upwards. Which should create
                // air currents from the ground up slowly cooling down the area.
                Dissipate(deltaTime);
                return;
            }

            heatPropagation.y = 0f;

            // The lower the y component was, the higher the windMultiplier.
            // If the wind blows strongly in a direction, windMultiplier should
            // be around 1. If there is no wind, it should be 0.
            float windMultiplier = heatPropagation.magnitude;

            for (int x = 1; x <= ResolutionX; ++x)
            {
                for (int y = 1; y <= ResolutionY; ++y)
                {
                    float t0 = _heatTransferGrid[x, y];
                    float t1 = _heatTransferGrid[x + ix, y + iz];
                    float dt = (t1 - t0) * deltaTime * windMultiplier;
                    // If t0 is larger temperature, then dt is negative.
                    // but we don't care, we always move a positive value
                    // from x,y to x+ix, y + iy.
                    dt = Mathf.Abs(dt);
                    _heatTransferGrid[x, y] -= dt;
                    _heatTransferGrid[x + ix, y + iz] += dt;
                }
            }
        }

        private void FireSourceRadiation(float deltaTime)
        {
            for (int x = _loopStart; x < ResolutionX + _loopStart; ++x)
            {
                for (int y = _loopStart; y < ResolutionY + _loopStart; ++y)
                {
                    var t = _fireSourceGrid[x, y];

                    float dy0 = (_fireSourceGrid[x, y - 1] - t) * deltaTime / _deltaZDistance;
                    float dy1 = (_fireSourceGrid[x, y + 1] - t) * deltaTime / _deltaZDistance;
                    float dx0 = (_fireSourceGrid[x - 1, y] - t) * deltaTime / _deltaXDistance;
                    float dx1 = (_fireSourceGrid[x + 1, y] - t) * deltaTime / _deltaXDistance;

                    float dx0y0 = (_fireSourceGrid[x - 1, y - 1] - t) * deltaTime / _deltaDistance;
                    float dx0y1 = (_fireSourceGrid[x - 1, y + 1] - t) * deltaTime / _deltaDistance;
                    float dx1y0 = (_fireSourceGrid[x + 1, y - 1] - t) * deltaTime / _deltaDistance;
                    float dx1y1 = (_fireSourceGrid[x + 1, y + 1] - t) * deltaTime / _deltaDistance;

                    // When d* is negative, we should add as it means, that [x,y] has higher
                    // temperature and we should propagate from higher to lower temperature.
                    _heatTransferGrid[x, y - 1] -= dy0;
                    _heatTransferGrid[x, y + 1] -= dy1;
                    _heatTransferGrid[x - 1, y] -= dx0;
                    _heatTransferGrid[x + 1, y] -= dx1;

                    _heatTransferGrid[x - 1, y - 1] -= dx0y0;
                    _heatTransferGrid[x - 1, y + 1] -= dx0y1;
                    _heatTransferGrid[x + 1, y - 1] -= dx1y0;
                    _heatTransferGrid[x + 1, y + 1] -= dx1y1;
                }
            }
        }

    }
}
