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
        public Texture2D HeatTexture;

        private List<Plant> _burningPlants;
        private List<Plant> _burnedDownPlants;
        private List<Plant> _plants;

        private float[,] _temperatureGrid;
        private float[,] _outputGrid;
        private float _deltaXDistance, _deltaZDistance, _deltaDistance;
        private bool _isSimulationRunning = false;

        private const float _FlashpointTemperature = 600f;

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
            _temperatureGrid = new float[ResolutionX + 2, ResolutionY + 2];
            _outputGrid = new float[ResolutionX + 2, ResolutionY + 2];
            _deltaXDistance = SimulationBounds.size.x / ResolutionX;
            _deltaZDistance = SimulationBounds.size.z / ResolutionY;
            _deltaDistance = Mathf.Sqrt(_deltaXDistance * _deltaXDistance + _deltaZDistance * _deltaZDistance);
            HeatTexture = new Texture2D(ResolutionX, ResolutionY);
        }

        private void Update()
        {
            if (!_isSimulationRunning)
                return;

            float simulationDeltaTime = Time.deltaTime;
            SetFireTemperatures();
            SpreadTemperature(simulationDeltaTime * HeatTransferSpeed);
            ApplyMask();
            UpdateBurningTimes(simulationDeltaTime);
            UpdateTimesBeforeFlashpoint(simulationDeltaTime);
            SwapBuffers();
            Renderer.Render();
            UpdateTexture();
        }

        private void UpdateTexture()
        {
            for (int x = 0; x < ResolutionX; ++x)
            {
                for (int y = 0; y < ResolutionY; ++y)
                {
                    float red = Mathf.Clamp01(_temperatureGrid[x + 1, y + 1] / _FlashpointTemperature);
                    HeatTexture.SetPixel(x, y, new Color(red, 0f, 0f));
                }
            }
        }

        private void SwapBuffers()
        {
            var tmp = _outputGrid;
            _outputGrid = _temperatureGrid;
            _temperatureGrid = tmp;
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

            float x0 =  mixx* _outputGrid[ix + dx, iy + dy] + (1f - mixx) * _outputGrid[ix, iy + dy];
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

        private void SpreadTemperature(float deltaTime)
        {
            for (int x = 1; x <= ResolutionX; ++x)
            {
                for (int y = 1; y <= ResolutionY; ++y)
                {
                    // Compute temperature gradient
                    // Spread the temperature (convective heat transfer)
                    // Heat transfer is modified by wind and upwards heat movement
                    var t = _temperatureGrid[x, y];
                    float dy0 = (_temperatureGrid[x, y - 1]);
                    float dy1 = (_temperatureGrid[x, y + 1]);
                    float dx0 = (_temperatureGrid[x - 1, y]);
                    float dx1 = (_temperatureGrid[x + 1, y]);
                    float dy0x0 = (_temperatureGrid[x - 1, y - 1]);
                    float dx1y1 = (_temperatureGrid[x + 1, y + 1]);
                    float dx1y0 = (_temperatureGrid[x + 1, y - 1]);
                    float dx0y1 = (_temperatureGrid[x - 1, y + 1]);

                    float dt = dy0 + dy1 + dx0 + dx1 + dy0x0 + dx1y1 + dx1y0 + dx0y1;
                    _outputGrid[x, y] = t + dt * deltaTime / 8f;

                }
            }
        }

    }
}
