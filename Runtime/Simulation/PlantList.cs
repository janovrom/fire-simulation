using Janovrom.Firesimulation.Runtime.Plants;
using System.Collections.Generic;

namespace Janovrom.Firesimulation.Runtime.Simulation
{
    internal class PlantList : List<Plant>
    {

        public const int BurningPlantsStart = 0;
        public int BurningPlantsCount { get; private set; }
        public int ActivePlantsStart => BurningPlantsCount;
        public int ActivePlantsEnd => ActivePlantsStart + ActivePlantsCount - 1;
        public int ActivePlantsCount { get; private set; }
        public int BurnedPlantsStart => BurningPlantsCount + ActivePlantsCount;


        public PlantList(IEnumerable<Plant> plants) : base(plants)
        {
            BurningPlantsCount = 0;
            ActivePlantsCount = Count;
        }

        public void LightPlant(int index)
        {
            Plant plant = this[index];
            if (plant.State == State.Normal)
            {
                plant.State = State.OnFire;
                Plant firstNormalPlant = this[ActivePlantsStart];
                this[index] = firstNormalPlant;
                this[ActivePlantsStart] = plant;
                BurningPlantsCount += 1;
                ActivePlantsCount -= 1;
            }
        }

        public void BurnDownPlant(int index)
        {
            Plant plant = this[index];
            if (plant.State == State.OnFire)
            {
                plant.State = State.Burned;
                // Make it last burning plant
                this[index] = this[BurningPlantsCount];
                // Here should be assignment this[BurningPlantsCount] = plant but let's skip it
                // Shift last normal to last burning
                Plant lastNormalPlant = this[ActivePlantsStart + ActivePlantsCount - 1];
                this[BurningPlantsCount] = lastNormalPlant;
                this[ActivePlantsStart + ActivePlantsCount - 1] = plant;
                BurningPlantsCount -= 1;
            }
        }

    }
}
