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

    void Awake()
    {
        Instance = this;
    }

    public void SpawnAnimal(int prefabIndex)
    {
        GameObject spawnDot = LocationManager.GetRandomGroundPosition(5);
        if (spawnDot == null) return;

        Vector3 spawnpoint = spawnDot.transform.position;
        spawnpoint.y += 0.1f;

        GameObject animal = Instantiate(animalPrefabs[prefabIndex], spawnpoint, Quaternion.identity, spawnParent);
        spawnedAnimals.Add(animal);
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