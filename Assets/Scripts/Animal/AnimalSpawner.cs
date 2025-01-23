using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    public List<GameObject> animalPrefabs;
    public readonly List<GameObject> spawnedAnimals = new();
    public Transform spawnParent;

    public void SpawnAnimal(int prefabIndex)
    {
        GameObject spawnDot = LocationManager.GetRandomGroundPosition(5);
        if (spawnDot == null) return;

        Vector3 spawnpoint = spawnDot.transform.position;
        spawnpoint.y += 0.1f;

        GameObject animal = Instantiate(animalPrefabs[prefabIndex], spawnpoint, Quaternion.identity, spawnParent);
        spawnedAnimals.Add(animal);
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