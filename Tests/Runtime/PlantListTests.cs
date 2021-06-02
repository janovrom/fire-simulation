using NUnit.Framework;
using System.Collections.Generic;
using Janovrom.Firesimulation.Runtime.Simulation;
using Janovrom.Firesimulation.Runtime.Plants;
using UnityEngine;

public class PlantListTests
{

    private static PlantList GetPlantList()
    {
        var list = new List<Plant>(10);
        for (int i = 0; i < 10; ++i)
            list.Add(GetPlant());

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

        AssertCoherence(plantList);

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

        AssertCoherence(plantList);

        // 1 burning plants, 6 active plants, 2 burned down plant
        plantList.BurnDownPlant(0);
        Assert.AreEqual(expected: 1, plantList.BurningPlantsCount);
        Assert.AreEqual(expected: 1, plantList.ActivePlantsStart);
        Assert.AreEqual(expected: 7, plantList.ActivePlantsCount);
        Assert.AreEqual(expected: 7, plantList.ActivePlantsEnd);
        Assert.AreEqual(expected: 8, plantList.BurnedPlantsStart);

        AssertCoherence(plantList);

    }

    [Test]
    public void TestAddPlant()
    {
        // There should be 3 burning plants, followed by 5 active plants, and 2 burned.
        // State is checked by previous test.
        PlantList plantList = GetPlantList();
        plantList.LightPlant(6);
        plantList.LightPlant(2);
        plantList.LightPlant(plantList.ActivePlantsEnd);
        plantList.BurnDownPlant(2);
        plantList.BurnDownPlant(0);

        Plant p0 = GetPlant();
        p0.State = State.Burned;
        plantList.AddPlant(p0);
        AssertCoherence(plantList);
        Assert.AreEqual(expected: 1, plantList.BurningPlantsCount);
        Assert.AreEqual(expected: 1, plantList.ActivePlantsStart);
        Assert.AreEqual(expected: 7, plantList.ActivePlantsCount);
        Assert.AreEqual(expected: 7, plantList.ActivePlantsEnd);
        Assert.AreEqual(expected: 8, plantList.BurnedPlantsStart);
        Assert.AreEqual(expected: 11, plantList.Count);

        Plant p1 = GetPlant();
        p1.State = State.Normal;
        plantList.AddPlant(p1);
        AssertCoherence(plantList);
        Assert.AreEqual(expected: 1, plantList.BurningPlantsCount);
        Assert.AreEqual(expected: 1, plantList.ActivePlantsStart);
        Assert.AreEqual(expected: 8, plantList.ActivePlantsCount);
        Assert.AreEqual(expected: 8, plantList.ActivePlantsEnd);
        Assert.AreEqual(expected: 9, plantList.BurnedPlantsStart);
        Assert.AreEqual(expected: 12, plantList.Count);

        Plant p2 = GetPlant();
        p2.State = State.OnFire;
        plantList.AddPlant(p2);
        AssertCoherence(plantList);
        Assert.AreEqual(expected: 2, plantList.BurningPlantsCount);
        Assert.AreEqual(expected: 2, plantList.ActivePlantsStart);
        Assert.AreEqual(expected: 8, plantList.ActivePlantsCount);
        Assert.AreEqual(expected: 9, plantList.ActivePlantsEnd);
        Assert.AreEqual(expected: 10, plantList.BurnedPlantsStart);
        Assert.AreEqual(expected: 13, plantList.Count);
    }

    [Test]
    public void TestRemovePlant()
    {
        PlantList plantList = GetPlantList();
        plantList.LightPlant(6);
        plantList.LightPlant(2);
        plantList.LightPlant(4);
        plantList.LightPlant(plantList.ActivePlantsEnd);
        plantList.BurnDownPlant(2);
        plantList.BurnDownPlant(0);

        plantList.RemovePlant(plantList[4]); // Normal
        AssertCoherence(plantList);
        Assert.AreEqual(expected: 2, plantList.BurningPlantsCount);
        Assert.AreEqual(expected: 2, plantList.ActivePlantsStart);
        Assert.AreEqual(expected: 5, plantList.ActivePlantsCount);
        Assert.AreEqual(expected: 6, plantList.ActivePlantsEnd);
        Assert.AreEqual(expected: 7, plantList.BurnedPlantsStart);
        Assert.AreEqual(expected: 9, plantList.Count);

        plantList.RemovePlant(plantList[7]); // Burned
        AssertCoherence(plantList);
        Assert.AreEqual(expected: 2, plantList.BurningPlantsCount);
        Assert.AreEqual(expected: 2, plantList.ActivePlantsStart);
        Assert.AreEqual(expected: 5, plantList.ActivePlantsCount);
        Assert.AreEqual(expected: 6, plantList.ActivePlantsEnd);
        Assert.AreEqual(expected: 7, plantList.BurnedPlantsStart);
        Assert.AreEqual(expected: 8, plantList.Count);

        plantList.RemovePlant(plantList[0]); // On fire
        AssertCoherence(plantList);
        Assert.AreEqual(expected: 1, plantList.BurningPlantsCount);
        Assert.AreEqual(expected: 1, plantList.ActivePlantsStart);
        Assert.AreEqual(expected: 5, plantList.ActivePlantsCount);
        Assert.AreEqual(expected: 5, plantList.ActivePlantsEnd);
        Assert.AreEqual(expected: 6, plantList.BurnedPlantsStart);
        Assert.AreEqual(expected: 7, plantList.Count);

        plantList.RemovePlant(plantList[0]); // On fire
        AssertCoherence(plantList);
        Assert.AreEqual(expected: 0, plantList.BurningPlantsCount);
        Assert.AreEqual(expected: 0, plantList.ActivePlantsStart);
        Assert.AreEqual(expected: 5, plantList.ActivePlantsCount);
        Assert.AreEqual(expected: 4, plantList.ActivePlantsEnd);
        Assert.AreEqual(expected: 5, plantList.BurnedPlantsStart);
        Assert.AreEqual(expected: 6, plantList.Count);

        plantList.RemovePlant(plantList[0]); // Normal
        AssertCoherence(plantList);
        Assert.AreEqual(expected: 0, plantList.BurningPlantsCount);
        Assert.AreEqual(expected: 0, plantList.ActivePlantsStart);
        Assert.AreEqual(expected: 4, plantList.ActivePlantsCount);
        Assert.AreEqual(expected: 3, plantList.ActivePlantsEnd);
        Assert.AreEqual(expected: 4, plantList.BurnedPlantsStart);
        Assert.AreEqual(expected: 5, plantList.Count);
    }

    private static void AssertCoherence(PlantList plantList)
    {
        for (int i = 0; i < plantList.BurningPlantsCount; ++i)
            Assert.AreEqual(expected: State.OnFire, plantList[i].State);

        for (int i = plantList.ActivePlantsStart; i < plantList.ActivePlantsEnd; ++i)
            Assert.AreEqual(expected: State.Normal, plantList[i].State);

        for (int i = plantList.BurnedPlantsStart; i < plantList.Count; ++i)
            Assert.AreEqual(expected: State.Burned, plantList[i].State);
    }

    private static Plant GetPlant()
    {
        return new GameObject().AddComponent<Plant>();
    }

}
