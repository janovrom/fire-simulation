using NUnit.Framework;
using System.Collections.Generic;
using Janovrom.Firesimulation.Runtime.Simulation;
using Janovrom.Firesimulation.Runtime.Plants;

public class PlantListTests
{

    private static PlantList GetPlantList()
    {
        var list = new List<Plant>(10);
        for (int i = 0; i < 10; ++i)
            list.Add(new Plant());

        return new PlantList(list);
    }

    [Test]
    public void TestLightFire()
    {
        PlantList plantList = GetPlantList();
        plantList.LightPlant(6);

        Assert.AreEqual(expected: 1, plantList.BurningPlantsCount);
        Assert.AreEqual(expected: 1, plantList.ActivePlantsStart);
        Assert.AreEqual(expected: 9, plantList.ActivePlantsCount);
        Assert.AreEqual(expected: 9, plantList.ActivePlantsEnd);
        Assert.AreEqual(expected: 10, plantList.BurnedPlantsStart);

        plantList.LightPlant(2);
        Assert.AreEqual(expected: 2, plantList.BurningPlantsCount);
        Assert.AreEqual(expected: 2, plantList.ActivePlantsStart);
        Assert.AreEqual(expected: 8, plantList.ActivePlantsCount);
        Assert.AreEqual(expected: 9, plantList.ActivePlantsEnd);
        Assert.AreEqual(expected: 10, plantList.BurnedPlantsStart);

        plantList.LightPlant(plantList.ActivePlantsEnd); // 9, from previous test
        Assert.AreEqual(expected: 3, plantList.BurningPlantsCount);
        Assert.AreEqual(expected: 3, plantList.ActivePlantsStart);
        Assert.AreEqual(expected: 7, plantList.ActivePlantsCount);
        Assert.AreEqual(expected: 9, plantList.ActivePlantsEnd);
        Assert.AreEqual(expected: 10, plantList.BurnedPlantsStart);

        for (int i = 0; i < plantList.BurningPlantsCount; ++i)
            Assert.AreEqual(expected: State.OnFire, plantList[i].State);

        for (int i = plantList.ActivePlantsStart; i < plantList.ActivePlantsEnd; ++i)
            Assert.AreEqual(expected: State.Normal, plantList[i].State);

        for (int i = plantList.BurnedPlantsStart; i < plantList.Count; ++i)
            Assert.Fail();
    }

    [Test]
    public void TestBurnDown()
    {
        // There should be 3 burning plants, followed by 7 active plants.
        PlantList plantList = GetPlantList();
        plantList.LightPlant(6);
        plantList.LightPlant(2);
        plantList.LightPlant(plantList.ActivePlantsEnd); // 9, from previous test

        // 2 burning plants, 6 active plants, 1 burned down plant
        plantList.BurnDownPlant(2);
        Assert.AreEqual(expected: 2, plantList.BurningPlantsCount);
        Assert.AreEqual(expected: 2, plantList.ActivePlantsStart);
        Assert.AreEqual(expected: 7, plantList.ActivePlantsCount);
        Assert.AreEqual(expected: 8, plantList.ActivePlantsEnd);
        Assert.AreEqual(expected: 9, plantList.BurnedPlantsStart);

        for (int i = 0; i < plantList.BurningPlantsCount; ++i)
            Assert.AreEqual(expected: State.OnFire, plantList[i].State);

        for (int i = plantList.ActivePlantsStart; i < plantList.ActivePlantsEnd; ++i)
            Assert.AreEqual(expected: State.Normal, plantList[i].State);

        for (int i = plantList.BurnedPlantsStart; i < plantList.Count; ++i)
            Assert.AreEqual(expected: State.Burned, plantList[i].State);

        // 1 burning plants, 6 active plants, 2 burned down plant
        plantList.BurnDownPlant(0);
        Assert.AreEqual(expected: 1, plantList.BurningPlantsCount);
        Assert.AreEqual(expected: 1, plantList.ActivePlantsStart);
        Assert.AreEqual(expected: 7, plantList.ActivePlantsCount);
        Assert.AreEqual(expected: 7, plantList.ActivePlantsEnd);
        Assert.AreEqual(expected: 8, plantList.BurnedPlantsStart);

        for (int i = 0; i < plantList.BurningPlantsCount; ++i)
            Assert.AreEqual(expected: State.OnFire, plantList[i].State);

        for (int i = plantList.ActivePlantsStart; i < plantList.ActivePlantsEnd; ++i)
            Assert.AreEqual(expected: State.Normal, plantList[i].State);

        for (int i = plantList.BurnedPlantsStart; i < plantList.Count; ++i)
            Assert.AreEqual(expected: State.Burned, plantList[i].State);

    }

}
