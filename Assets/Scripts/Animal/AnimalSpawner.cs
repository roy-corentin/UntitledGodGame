using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class AnimalSpawner : MonoBehaviour
{
    public List<GameObject> animalPrefabs;
    public readonly List<GameObject> spawnedAnimals = new();
    public Transform spawnParent;
    public static AnimalSpawner Instance;
    public const float Y_OFFSET = 0.1f;

    void Awake()
    {
        Instance = this;
    }

    public void SpawnAnimal(int prefabIndex)
    {
        GameObject spawnDot = LocationManager.GetRandomGroundPosition(5);
        if (spawnDot == null) return;

        Vector3 spawnpoint = spawnDot.transform.position;
        spawnpoint.y += Y_OFFSET;

        GameObject animal = Instantiate(animalPrefabs[prefabIndex], spawnpoint, Quaternion.identity, spawnParent);
        Animal animalScript = animal.GetComponent<Animal>();
        animalScript.prefabIndex = prefabIndex;
        spawnedAnimals.Add(animal);
    }

    public void SpawnAnimal(AnimalData animalData)
    {
        MapGenerator.Dot spawnDot = MapGenerator.Map.Instance.mapDots[animalData.dotIndex.x][animalData.dotIndex.y];
        if (spawnDot == null) return;

        Vector3 spawnpoint = spawnDot.transform.position;
        spawnpoint.y += Y_OFFSET;

        GameObject animal = Instantiate(animalPrefabs[animalData.prefabIndex], spawnpoint, Quaternion.identity, spawnParent);
        Animal animalScript = animal.GetComponent<Animal>();
        animalScript.prefabIndex = animalData.prefabIndex;
        animalScript.moveSpeed = animalData.moveSpeed;
        animalScript.maxMoveTime = animalData.maxMoveTime;
        animalScript.thirstValue = animalData.thirstValue;
        animalScript.decreaseThirstPerSecond = animalData.decreaseThirstPerSecond;
        animalScript.hungerValue = animalData.hungerValue;
        animalScript.decreaseHungerPerSecond = animalData.decreaseHungerPerSecond;
        animalScript.sleepValue = animalData.sleepValue;
        animalScript.decreaseSleepPerSecond = animalData.decreaseSleepPerSecond;
        spawnedAnimals.Add(animal);
    }

    public void RemoveAll()
    {
        foreach (GameObject animal in spawnedAnimals)
        {
            if (animal == null) continue;
            Destroy(animal);
        }

        spawnedAnimals.Clear();
    }

    public void AddNavAgentToAll()
    {
        foreach (GameObject animal in spawnedAnimals)
        {
            if (animal == null) continue;
            Animal animalScript = animal.GetComponent<Animal>();
            animalScript.SetupNavAgent();
        }
    }

    public void DisableAll()
    {
        foreach (GameObject animal in spawnedAnimals)
        {
            if (animal == null) continue;
            Animal animalScript = animal.GetComponent<Animal>();
            animalScript.Disable();
        }
    }

    public void EnableAll()
    {
        foreach (GameObject animal in spawnedAnimals)
        {
            if (animal == null) continue;
            Animal animalScript = animal.GetComponent<Animal>();
            animalScript.Enable();
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AnimalSpawner))]
public class AnimalSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        AnimalSpawner animal = (AnimalSpawner)target;

        GUILayout.Space(10);

        foreach (GameObject animalPrefab in animal.animalPrefabs)
        {
            if (animalPrefab == null) continue;

            if (GUILayout.Button("New " + animalPrefab.name))
                animal.SpawnAnimal(animal.animalPrefabs.IndexOf(animalPrefab));
        }
    }
}
#endif