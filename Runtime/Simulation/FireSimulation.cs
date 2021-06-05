using Janovrom.Firesimulation.Runtime.Bindings;
using Janovrom.Firesimulation.Runtime.PlantGenerators;
using Janovrom.Firesimulation.Runtime.Plants;
using Janovrom.Firesimulation.Runtime.Renderers;
using Janovrom.Firesimulation.Runtime.Variables;
using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.Simulation
{

    /// <summary>
    /// Physically inspired fire simulation in grid. The grid can be modified
    /// by resolution on each axis, plant provider and renderer. The provider
    /// and size of the simulation is handled by aabb. For differently sized
    /// grids, tweak <see cref="HeatTransferSpeed"/>. The simulation is using
    /// one-way from source bindings for wind speed and wind direction.
    /// <br/>
    /// Fire simulation is simplified by constant fire temperature (1200° Celsius)
    /// and flashpoint temperature (600° Celsius). Two heat transfer phenomenons
    /// are used - heat radiation, heat convection. Heat convection is simulaed by
    /// wind and dissipation (upwards heat movement).
    /// </summary>
    public class FireSimulation : MonoBehaviour
    {

        public PlantProvider PlantGenerator;
        public PlantBasicRenderer Renderer;

        public int ResolutionX = 256;
        public int ResolutionY = 256;
        [Range(0f, 1f)]
        public float HeatTransferSpeed = 1f;
        public FloatVariable WindSpeedNormalized;
        public FloatVariable WindAngleNormalized;
        public Bounds SimulationBounds;
        public BoolBinding IsSimulationRunning;

        private bool _isSimulationRunning = false;

        // Cached data
        private PlantList _plantList;
        private float[,] _fireSourceGrid;
        private float[,] _heatTransferGrid;
        private float _deltaXDistance, _deltaZDistance, _deltaDistance;
        private Vector3 _min;

        // Fire settings
        private const float _fireTemperature = 1200f;
        private const float _flashpointTemperature = 600f;

        // Paddings for grid - no if checks for boundaries.
        private const int _loopStart = 1;
        private const int _padding = _loopStart * 2;

        public void ToggleSimulation(bool value)
        {
            _isSimulationRunning = value;
        }

        /// <summary>
        /// Adds a new plant on the <paramref name="position"/> if it's
        /// in the <see cref="SimulationBounds"/>. The plant is registered
        /// to the <see cref="Renderer"/> and the indices for a grid are cached.
        /// </summary>
        /// <param name="position"></param>
        public void AddPlant(Vector3 position)
        {
            if (_plantList is null)
                return;

            // Don't create plants outside the simulation
            if (!SimulationBounds.Contains(position))
                return;

            Plant plant = PlantGenerator.GetPlant(in position);
            Renderer.Register(plant);
            _plantList.AddPlant(plant);
            CacheIndices(plant);
        }

        /// <summary>
        /// Removes the <paramref name="gameObject"/> if it has <see cref="Plant"/> 
        /// component and notifies the <see cref="Renderer"/> and destroys the 
        /// game object afterwards.
        /// </summary>
        /// <param name="gameObject"></param>
        public void RemovePlant(GameObject gameObject)
        {
            var plant = gameObject.GetComponentInParent<Plant>();
            if (!(_plantList is null) && !(plant is null))
            {
                Renderer.Unregister(plant);
                _plantList.RemovePlant(plant);
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Lights the <paramref name="gameObject"/> on fire if it has
        /// <see cref="Plant"/> component and notifies the <see cref="Renderer"/>.
        /// </summary>
        /// <param name="gameObject"></param>
        public void LightFire(GameObject gameObject)
        {
            var plant = gameObject.GetComponentInParent<Plant>();
            if (!(_plantList is null) && !(plant is null))
            {
                _plantList.LightPlant(_plantList.IndexOf(plant));
                Renderer.NotifyStateChange(plant);
            }
        }

        /// <summary>
        /// Pauses the simulation (event is invoked and can be listened to
        /// on <see cref="IsSimulationRunning"/> binding) and clears the 
        /// renderer and all plants.
        /// </summary>
        public void ClearPlants()
        {
            IsSimulationRunning.Value = false;

            if (!(Renderer is null))
                Renderer.Clear();

            if (!(PlantGenerator is null))
                PlantGenerator.Clear();

            _plantList = null;
        }

        /// <summary>
        /// Pauses the simulation, removes all old plants and replaces them with
        /// new ones. One random plant is lit on fire, indices are cached and 
        /// simulation is started.
        /// </summary>
        public void GeneratePlants()
        {
            if (PlantGenerator is null)
                return;

            IsSimulationRunning.Value = false;

            ClearPlants();

            _plantList = new PlantList(PlantGenerator.GetPlants(SimulationBounds.min, SimulationBounds.max));
            var plantToLightIdx = UnityEngine.Random.Range(0, _plantList.Count);

            _plantList.LightPlant(plantToLightIdx);

            foreach (var plant in _plantList)
            {
                Renderer.Register(plant);
            }

            InitializeGrid();
            CacheIndices();

            IsSimulationRunning.Value = true;
        }

        /// <summary>
        /// Caches some data as fields for faster access and initializes the simulation
        /// grids.
        /// </summary>
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

        /// <summary>
        /// Runs the simulation if any plants are present and the simulation is not paused.
        /// First fire is spread using radiation, then wind is applied, and then linear 
        /// temperature dissipation is used to simulate heat upwards movement and transfer
        /// of the heat to ground (other objects). After each plant state is updated, the
        /// plants are rendered (if needed).
        /// </summary>
        private void Update()
        {
            if (!_isSimulationRunning || _plantList is null)
                return;

            float simulationDeltaTime = Time.deltaTime * HeatTransferSpeed;

            // Caching so that each fire source in cell is used only once
            FireSourceRadiation(simulationDeltaTime);
            ApplyWind(simulationDeltaTime);
            Dissipate(simulationDeltaTime);

            // Mask out unneeded cells
            ApplyMask();

            // Update states
            UpdateBurningTimes(Time.deltaTime);
            UpdateTimesBeforeFlashpoint();

            Renderer.Render();
        }

        private void OnDisable()
        {
            IsSimulationRunning.Value = false;
        }

        private void GetIndices(Plant plant, out int x, out int y)
        {
            Vector3 pos = plant.transform.position;
            x = (int)((pos.x - _min.x) / _deltaXDistance) + 1;
            y = (int)((pos.z - _min.z) / _deltaZDistance) + 1;
        }

        /// <summary>
        /// Nearest-neighbor access to plant's temperature. The indices
        /// are cached. It suffers from popping but could be resolved 
        /// by randomizing burn times, flashpoint temperature.
        /// </summary>
        /// <param name="plant"></param>
        /// <returns></returns>
        private float GetLocalTemperature_NNCached(Plant plant)
        {
            return _heatTransferGrid[plant.IndexX, plant.IndexY];
        }

        /// <summary>
        /// Nearest-neighbor access to plant's temperature. The indices
        /// are computed. It suffers from popping but could be resolved 
        /// by randomizing burn times, flashpoint temperature.
        /// </summary>
        /// <param name="plant"></param>
        /// <returns></returns>
        private float GetLocalTemperature_NN(Plant plant)
        {
            GetIndices(plant, out int x, out int y);
            return _heatTransferGrid[x, y];
        }

        /// <summary>
        /// Bilinear interpolation based on the plant distance from the centers of cells.
        /// This slows down the simulation but provides nicer results on edges as nearest 
        /// neighbor sampling suffers from popping.
        /// </summary>
        /// <param name="plant"></param>
        /// <returns></returns>
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
            for (int i =  start; i <= end; ++i)
            {
                Plant plant = _plantList[i];
                var currentLocalTemperature = GetLocalTemperature_NNCached(plant);

                // Let's say flashpoint temperature is 600°C and let's ignore the heat transfer
                // from air to the wood and heat specific capacity.
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
            // TODO here we could mask out parts of the simulation. I.e. only
            // parts could be lit or inverse.
        }

        /// <summary>
        /// Update the heat radiation in nearest cells from fire source.
        /// Using the grid for only applying the radiation once provides
        /// nicer results.
        /// </summary>
        /// <param name="deltaTime"></param>
        private void FireSourceRadiation(float deltaTime)
        {
            // Reset the look up table
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
                if (_fireSourceGrid[x, y] > 0f)
                    continue;

                _fireSourceGrid[x, y] = Mathf.Max(_fireSourceGrid[x, y], _fireTemperature);

                float t = _fireTemperature;

                float dy0 = (_fireSourceGrid[x, y - 1] - t) * deltaTime / _deltaZDistance;
                float dy1 = (_fireSourceGrid[x, y + 1] - t) * deltaTime / _deltaZDistance;
                float dx0 = (_fireSourceGrid[x - 1, y] - t) * deltaTime / _deltaXDistance;
                float dx1 = (_fireSourceGrid[x + 1, y] - t) * deltaTime / _deltaXDistance;

                float dx0y0 = (_fireSourceGrid[x - 1, y - 1] - t) * deltaTime / _deltaDistance;
                float dx0y1 = (_fireSourceGrid[x - 1, y + 1] - t) * deltaTime / _deltaDistance;
                float dx1y0 = (_fireSourceGrid[x + 1, y - 1] - t) * deltaTime / _deltaDistance;
                float dx1y1 = (_fireSourceGrid[x + 1, y + 1] - t) * deltaTime / _deltaDistance;

                // Let's divide by radius of the cell
                _heatTransferGrid[x, y] += t * deltaTime;

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

        private void Dissipate(float deltaTime)
        {
            for (int x = 1; x <= ResolutionX; ++x)
            {
                for (int y = 1; y <= ResolutionY; ++y)
                {
                    _heatTransferGrid[x, y] = Mathf.Max(0f, _heatTransferGrid[x, y] - deltaTime);
                }
            }
        }

        /// <summary>
        /// Computes wind direction as an (i,j) index offset and moves the heat from
        /// one cell to other by a value specified by direction of the wind (mostly 
        /// depends on the Y direction). If the (i,j) is (0,0) then the heat is not 
        /// propagated but dissipated.
        /// </summary>
        /// <param name="deltaTime"></param>
        private void ApplyWind(float deltaTime)
        {
            float windSpeed = WindSpeedNormalized * 100f;
            float windAngle = WindAngleNormalized * Mathf.PI * 2f;
            // 1 on y for drag up
            Vector3 heatPropagation = new Vector3(Mathf.Cos(windAngle) * windSpeed, 1f, Mathf.Sin(windAngle) * windSpeed);
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
                    _heatTransferGrid[x, y] = Mathf.Max(_heatTransferGrid[x, y], 0f);
                    _heatTransferGrid[x + ix, y + iz] += dt;
                }
            }
        }

    }
}
