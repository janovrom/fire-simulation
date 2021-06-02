using Janovrom.Firesimulation.Runtime.Plants;
using System;
using System.Collections.Generic;

namespace Janovrom.Firesimulation.Runtime.Simulation
{
    internal class PlantList : List<Plant>
    {

        internal const int BurningPlantsStart = 0;
        public int BurningPlantsCount { get; private set; }
        public int ActivePlantsStart => BurningPlantsCount;
        public int ActivePlantsEnd => ActivePlantsStart + ActivePlantsCount - 1;
        public int ActivePlantsCount { get; private set; }
        public int BurnedPlantsStart => BurningPlantsCount + ActivePlantsCount;


        internal PlantList(IEnumerable<Plant> plants) : base(plants)
        {
            BurningPlantsCount = 0;
            ActivePlantsCount = Count;
        }

        internal void AddPlant(Plant plant)
        {
            base.Add(plant);
            switch (plant.State)
            {
                case State.Normal:
                    // Swap with first burned
                    Plant burned = this[BurnedPlantsStart];
                    this[BurnedPlantsStart] = plant;
                    this[Count - 1] = burned;
                    ActivePlantsCount += 1;
                    break;
                case State.OnFire:
                    // Swap with first burned
                    Plant normal = this[ActivePlantsStart];
                    this[Count - 1] = this[BurnedPlantsStart];
                    this[BurnedPlantsStart] = normal;
                    this[ActivePlantsStart] = plant;
                    BurningPlantsCount += 1;
                    break;
                case State.Burned:
                    // Finished, it should be placed at end
                    break;
            }
        }

        internal void RemovePlant(Plant plant)
        {
            int plantIndex = IndexOf(plant);
            switch (plant.State)
            {
                case State.Normal:
                    this[plantIndex] = this[ActivePlantsEnd];
                    this[ActivePlantsEnd] = this[Count - 1];
                    ActivePlantsCount -= 1;
                    break;
                case State.OnFire:
                    // Swap with first burned
                    this[plantIndex] = this[BurningPlantsCount - 1];
                    this[BurningPlantsCount - 1] = this[ActivePlantsEnd];
                    this[ActivePlantsEnd] = this[Count - 1];
                    BurningPlantsCount -= 1;
                    break;
                case State.Burned:
                    this[plantIndex] = this[Count - 1];
                    break;
            }
            RemoveAt(Count - 1);
        }

        internal void LightPlant(int index)
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

        internal void BurnDownPlant(int index)
        {
            Plant plant = this[index];
            if (plant.State == State.OnFire)
            {
                plant.State = State.Burned;
                // Make it last burning plant
                this[index] = this[BurningPlantsCount - 1];
                // Here should be assignment this[BurningPlantsCount] = plant but let's skip it
                // Shift last normal to last burning
                Plant lastNormalPlant = this[ActivePlantsEnd];
                this[BurningPlantsCount - 1] = lastNormalPlant;
                this[ActivePlantsEnd] = plant;
                BurningPlantsCount -= 1;
            }
        }

    }
}
