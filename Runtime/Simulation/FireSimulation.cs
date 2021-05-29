using Janovrom.Firesimulation.Runtime.PlantGenerators;
using Janovrom.Firesimulation.Runtime.Plants;
using Janovrom.Firesimulation.Runtime.Renderers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.Simulation
{
    public class FireSimulation : MonoBehaviour
    {

        public PlantProvider PlantGenerator;
        public int ResolutionX = 256;
        public int ResolutionY = 256;
        [Range(0f, 1f)]
        public float HeatTransferSpeed = 1f;
        public PlantBasicRenderer Renderer;
        public Bounds SimulationBounds;
        [Range(0f, 100f)]
        public float WindSpeed = 0f;
        public Vector3 WindDirection = Vector3.right;

        private List<Plant> _burningPlants;
        private List<Plant> _burnedDownPlants;
        private List<Plant> _plants;

        private float[,] _temperatureGrid;
        private float[,] _outputGrid;
        private float _deltaXDistance, _deltaZDistance, _deltaDistance;
        private bool _isSimulationRunning = false;

        private const float _FlashpointTemperature = 600f;
        private const int _loopStart = 2;
        private const int _padding = _loopStart * 2;


        public void StartSimulation()
        {
            if (PlantGenerator is null)
                return;

            _isSimulationRunning = false;

            Renderer.Clear();
            IList<Plant> plants = PlantGenerator.GetPlants(SimulationBounds.min, SimulationBounds.max);
            var plantToLightIdx = Random.Range(0, plants.Count);
            Plant plantToLight = plants[plantToLightIdx];
            plantToLight.State = State.OnFire;

            _burningPlants = new List<Plant>();
            _burnedDownPlants = new List<Plant>();
            _plants = new List<Plant>();

            _plants = plants.Where(plant => plant != plantToLight).ToList();
            _burningPlants.Add(plantToLight);

            foreach (var plant in plants)
            {
                Renderer.Register(plant);
            }

            InitializeGrid();

            _isSimulationRunning = true;
        }

        private void InitializeGrid()
        {
            _temperatureGrid = new float[ResolutionX + _padding, ResolutionY + _padding];
            _outputGrid = new float[ResolutionX + _padding, ResolutionY + _padding];
            _deltaXDistance = SimulationBounds.size.x / ResolutionX;
            _deltaZDistance = SimulationBounds.size.z / ResolutionY;
            _deltaDistance = Mathf.Sqrt(_deltaXDistance * _deltaXDistance + _deltaZDistance * _deltaZDistance);
        }

        private void Update()
        {
            if (!_isSimulationRunning)
                return;

            float simulationDeltaTime = Time.deltaTime;
            SetFireTemperatures();
            SpreadTemperature(simulationDeltaTime * HeatTransferSpeed);
            ApplyWind(simulationDeltaTime * HeatTransferSpeed);
            UpdateBurningTimes(simulationDeltaTime);
            UpdateTimesBeforeFlashpoint(simulationDeltaTime);
            Renderer.Render();
        }

        private void CopyOutputBuffer()
        {
            for (int x = 0; x < ResolutionX; ++x)
            {
                for (int y = 0; y < ResolutionY; ++y)
                {
                    _temperatureGrid[x, y] = _outputGrid[x, y];
                }
            }
        }

        private void GetIndices(Plant plant, out int x, out int y)
        {
            Vector3 delta = (plant.transform.position - SimulationBounds.min);
            x = (int)(delta.x / SimulationBounds.size.x * ResolutionX) + 1;
            y = (int)(delta.z / SimulationBounds.size.z * ResolutionY) + 1;
        }

        private float GetLocalTemperature(Plant plant)
        {
            Vector3 delta = (plant.transform.position - SimulationBounds.min);
            float x = (delta.x / SimulationBounds.size.x * ResolutionX) + 1;
            float y = (delta.z / SimulationBounds.size.z * ResolutionY) + 1;
            float mixx = x % 1f;
            float mixy = y % 1f;
            int dx = mixx + 0.5f > 1f ? 1 : -1;
            int dy = mixy + 0.5f > 1f ? 1 : -1;
            int ix = (int)x;
            int iy = (int)y;

            float x0 = mixx * _outputGrid[ix + dx, iy + dy] + (1f - mixx) * _outputGrid[ix, iy + dy];
            float x1 = mixx * _outputGrid[ix + dx, iy] + (1f - mixx) * _outputGrid[ix, iy];

            return mixy * x0 + (1f - mixy)  * x1;
        }

        private void UpdateTimesBeforeFlashpoint(float simulationDeltaTime)
        {
            for (int i =  _plants.Count - 1; i >= 0; --i)
            {
                Plant plant = _plants[i];
                var currentLocalTemperature = GetLocalTemperature(plant);

                // Let's say flashpoint temperature is 600°C and let's ignore the heat transfer
                // from air to the wood.
                if (currentLocalTemperature > _FlashpointTemperature)
                {
                    // Light the fire
                    _plants.RemoveAt(i);
                    _burningPlants.Add(plant);
                    plant.State = State.OnFire;
                }
            }
        }

        private void UpdateBurningTimes(float simulationDeltaTime)
        {
            for (int i = _burningPlants.Count - 1; i >= 0; --i)
            {
                Plant plant = _burningPlants[i];
                plant.TimeOnFire += simulationDeltaTime;
                if (plant.Data.BurnTime < plant.TimeOnFire)
                {
                    _burningPlants.RemoveAt(i);
                    _burnedDownPlants.Add(plant);
                    plant.State = State.Burned;
                }
            }
        }

        private void ApplyMask()
        {
        }

        private void SetFireTemperatures()
        {
            foreach (var plant in _burningPlants)
            {
                GetIndices(plant, out int x, out int y);
                _temperatureGrid[x, y] = Mathf.Max(_temperatureGrid[x, y], plant.Data.FireTemperature);
            }
        }

        private void ApplyWind(float deltaTime)
        {
            Vector3 heatPropagation = WindDirection.normalized * WindSpeed + Vector3.up;
            heatPropagation.Normalize();
            int ix = (int)Mathf.Sign(heatPropagation.x) * Mathf.Abs(heatPropagation.x) > 0.707f ? 1 : 0;
            int iz = (int)Mathf.Sign(heatPropagation.z) * Mathf.Abs(heatPropagation.z) > 0.707f ? 1 : 0;

            if (ix == 0 && iz == 0)
            {
                return;
            }

            heatPropagation.y = 0f;

            // The lower the y component was, the higher the windMultiplier.
            // If the wind blows strongly in a direction, windMultiplier should
            // be around 1. If there is no wind, it should be 0.
            float windMultiplier = heatPropagation.magnitude;
            float distanceMultiplier = 1f / Mathf.Sqrt(ix * _deltaXDistance * _deltaXDistance + iz * _deltaZDistance * _deltaZDistance);

            for (int x = 1; x <= ResolutionX; ++x)
            {
                for (int y = 1; y <= ResolutionY; ++y)
                {
                    float t0 = _temperatureGrid[x, y];
                    float t1 = _temperatureGrid[x + ix, y + iz];
                    float dt = (t1 - t0) * deltaTime * windMultiplier * distanceMultiplier * 0.1f;

                    _outputGrid[x, y] -= dt;
                    _outputGrid[x + ix, y + iz] += dt;
                }
            }
        }

        private void SpreadTemperature(float deltaTime)
        {
            for (int x = _loopStart; x < ResolutionX - _loopStart; ++x)
            {
                for (int y = _loopStart; y < ResolutionY - _loopStart; ++y)
                {
                    // Spread the temperature (convective heat transfer)
                    // Heat transfer is modified by wind and upwards heat movement
                    var t = _temperatureGrid[x, y];
                    float dy0 = (_temperatureGrid[x, y - 1]);
                    float dy1 = (_temperatureGrid[x, y + 1]);
                    float dx0 = (_temperatureGrid[x - 1, y]);
                    float dx1 = (_temperatureGrid[x + 1, y]);

                    float dt = dy0 + dy1 + dx0 + dx1;
                    _outputGrid[x, y] = t + dt * deltaTime / 4f;
                }
            }

            CopyOutputBuffer();
        }

    }
}
