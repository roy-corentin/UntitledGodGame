using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class Decision
{
    [UnityTest]
    public IEnumerator DecisionWithEnumeratorPasses()
    {
        // Setup Map
        yield return null;
        SceneManager.LoadScene("SampleScene");
        yield return null;

        // Check if DecisionTree, AnimalSpawner and MapGenerator.Map are not null
        DecisionTree tree = DecisionTree.Instance;
        Assert.AreNotEqual(tree, null);
        yield return null;

        AnimalSpawner animalSpawner = AnimalSpawner.Instance;
        Assert.AreNotEqual(animalSpawner, null);
        yield return null;

        MapGenerator.Map map = MapGenerator.Map.Instance;
        Assert.AreNotEqual(map, null);
        yield return null;

        // Wait for generation
        yield return new WaitUntil(() => map.areMeshGenerated);

        // Spawn animal
        GameObject animalGO = animalSpawner.SpawnAnimal(1);
        yield return null;
        Animal animal = animalGO.GetComponent<Animal>();
        Assert.AreNotEqual(animal, null);

        // Toggle VR mode
        ARtoVR.Instance.ToggleMode();
        yield return new WaitForSeconds(ARtoVR.Instance.transitionDuration + 3);

        // ----- DecisionTree -----
        Assert.AreEqual(animal.eventType, EventType.Random);
        tree.Callback(animal);
        Assert.AreEqual(animal.eventType, EventType.Random);
        yield return null;

        // ----- DecisionTree ----- SearchWater
        animal.thirstValue = 0f;
        tree.Callback(animal);
        Assert.AreEqual(animal.eventType, EventType.SearchWater);
        yield return null;
    }
}
