using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class Decision
{
    public IEnumerator Setup()
    {
        // Setup Map
        yield return null;
        SceneManager.LoadScene("SampleScene");
        yield return null;

        // Check if DecisionTree, AnimalSpawner and MapGenerator.Map are not null
        DecisionTree tree = DecisionTree.Instance;
        Assert.IsNotNull(tree);
        yield return null;

        AnimalSpawner animalSpawner = AnimalSpawner.Instance;
        Assert.IsNotNull(animalSpawner);
        yield return null;

        MapGenerator.Map map = MapGenerator.Map.Instance;
        Assert.IsNotNull(map);
        yield return null;

        // Wait for generation
        yield return new WaitUntil(() => map.isMeshGenerated);

        // Spawn Deer
        animalSpawner.SpawnAnimal(1);
        yield return null;
        Animal deer = animalSpawner.spawnedAnimals[0].GetComponent<Animal>();
        Assert.IsNotNull(deer);

        // Spawn Tiger
        animalSpawner.SpawnAnimal(2);
        yield return null;
        Animal tiger = animalSpawner.spawnedAnimals[1].GetComponent<Animal>();
        Assert.IsNotNull(tiger);

        // Toggle VR mode
        ARtoVR.Instance.ToggleMode();
        yield return new WaitForSeconds(ARtoVR.Instance.transitionDuration + 3);

        // ----- DecisionTree -----
        Assert.AreEqual(deer.eventType, EventType.Random);
        tree.Callback(deer);
        Assert.AreEqual(deer.eventType, EventType.Random);
        yield return null;

        // ----- DecisionTree -----
        Assert.AreEqual(tiger.eventType, EventType.Random);
        tree.Callback(tiger);
        Assert.AreEqual(tiger.eventType, EventType.Random);
        yield return null;
    }

    [UnityTest]
    public IEnumerator RandomMoveBehaviour()
    {
        // Setup
        yield return Setup();
        Animal deer = AnimalSpawner.Instance.spawnedAnimals[0].GetComponent<Animal>();
        DecisionTree tree = DecisionTree.Instance;

        // ----- DecisionTree -----
        Assert.AreEqual(deer.eventType, EventType.Random);
        tree.Callback(deer);
        Assert.AreEqual(deer.eventType, EventType.Random);
        yield return null;
    }

    [UnityTest]
    public IEnumerator DrinkBehaviour()
    {
        // Setup
        yield return Setup();
        Animal deer = AnimalSpawner.Instance.spawnedAnimals[0].GetComponent<Animal>();
        DecisionTree tree = DecisionTree.Instance;

        // SearchWater / Value 0
        deer.thirstValue = 0f;
        tree.Callback(deer);
        Assert.AreEqual(deer.eventType, EventType.SearchWater);
        yield return null;

        // Drink
        deer.forceDestination = true;
        tree.Callback(deer);
        Assert.AreEqual(deer.eventType, EventType.Drink);
        deer.forceDestination = false;
        yield return null;

        // Drink / Value 50
        deer.thirstValue = 50f;
        deer.isDrinking = true;
        tree.Callback(deer);
        Assert.AreEqual(deer.eventType, EventType.Drink);
        yield return null;
    }

    [UnityTest]
    public IEnumerator EatBehaviour()
    {
        // Setup
        yield return Setup();
        Animal deer = AnimalSpawner.Instance.spawnedAnimals[0].GetComponent<Animal>();
        Animal tiger = AnimalSpawner.Instance.spawnedAnimals[1].GetComponent<Animal>();
        DecisionTree tree = DecisionTree.Instance;

        // ----- DEER -----

        // SearchFood / Value 0
        deer.hungerValue = 0f;
        tree.Callback(deer);
        Assert.AreEqual(deer.eventType, EventType.SearchFood);
        yield return null;

        // Eat
        deer.forceDestination = true;
        tree.Callback(deer);
        Assert.AreEqual(deer.eventType, EventType.Eat);
        deer.forceDestination = false;
        yield return null;

        // Eat / Value 50
        deer.hungerValue = 50f;
        deer.isEating = true;
        tree.Callback(deer);
        Assert.AreEqual(deer.eventType, EventType.Eat);
        yield return null;

        // ----- TIGER -----

        // SearchFood / Value 0
        tiger.hungerValue = 0f;
        tree.Callback(tiger);
        Assert.AreEqual(tiger.eventType, EventType.SearchFood);
        yield return null;

        // Eat
        Assert.AreEqual(AnimalSpawner.Instance.spawnedAnimals.Count, 2);
        tiger.forceDestination = true;
        tree.Callback(tiger);
        Assert.AreEqual(tiger.eventType, EventType.Eat);
        yield return new WaitForSeconds(10);
        yield return null;
        Assert.IsTrue(deer == null);
        Assert.AreEqual(AnimalSpawner.Instance.spawnedAnimals.Count, 1);
        tiger.forceDestination = false;
        yield return null;

        // Eat / Value 50
        tiger.hungerValue = 50f;
        tiger.isEating = true;
        tree.Callback(tiger);
        Assert.AreEqual(tiger.eventType, EventType.Eat);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SleepBehaviour()
    {
        // Setup
        yield return Setup();
        Animal deer = AnimalSpawner.Instance.spawnedAnimals[0].GetComponent<Animal>();
        DecisionTree tree = DecisionTree.Instance;

        // SearchWater / Value 0
        deer.sleepValue = 0f;
        tree.Callback(deer);
        Assert.AreEqual(deer.eventType, EventType.Sleep);
        yield return null;

        // Drink
        deer.forceDestination = true;
        tree.Callback(deer);
        Assert.AreEqual(deer.eventType, EventType.Sleep);
        deer.forceDestination = false;
        yield return null;

        // Drink / Value 50
        deer.sleepValue = 50f;
        deer.isSleeping = true;
        tree.Callback(deer);
        Assert.AreEqual(deer.eventType, EventType.Sleep);
        yield return null;
    }
}
