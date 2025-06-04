using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public struct AnimalTarget
{
    public GameObject animal;
    public float distance;
}

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
        if (ARtoVR.Instance.currentMode == GameMode.VR) return;

        GameObject spawnDot = LocationManager.Instance.GetRandomGroundPosition();
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

        foreach (Transform child in spawnParent)
        {
            if (child == null) continue;
            Destroy(child.gameObject);
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

    public AnimalTarget GetNearestAnimalOfFoodLevel(GameObject currentAnimal, AnimalFoodLevel foodLevel, List<GameObject> notReachable = null)
    {
        AnimalTarget nearestAnimal = new()
        {
            animal = null,
            distance = float.MaxValue
        };

        foreach (GameObject animal in spawnedAnimals)
        {
            if (animal == null) continue;
            if (animal == currentAnimal) continue;
            if (notReachable != null && notReachable.Contains(animal)) continue;
            Animal animalScript = animal.GetComponent<Animal>();
            if (animalScript.thisFoodLevel == foodLevel)
            {
                float distance = Vector3.Distance(currentAnimal.transform.position, animal.transform.position);
                if (distance < nearestAnimal.distance)
                {
                    nearestAnimal.distance = distance;
                    nearestAnimal.animal = animal;
                }
            }
        }

        return nearestAnimal;
    }

    public void LockAnimations(bool status)
    {
        foreach (GameObject animal in spawnedAnimals)
        {
            if (animal == null) continue;
            Animal animalScript = animal.GetComponent<Animal>();
            animalScript.LockAnimation(status);
        }
    }

    public void ResetReachablesInfos()
    {
        foreach (GameObject animal in spawnedAnimals)
        {
            if (animal == null) continue;
            Animal animalScript = animal.GetComponent<Animal>();
            animalScript.ResetReachableInfos();
        }
    }

    public void RemoveAllAnimalsFromTheSamePrefab(int prefabIndex)
    {
        List<GameObject> animalsToRemove = new();

        foreach (GameObject animal in spawnedAnimals)
        {
            if (animal == null) continue;
            Animal animalScript = animal.GetComponent<Animal>();
            if (animalScript.prefabIndex == prefabIndex)
                animalsToRemove.Add(animal);
        }

        foreach (GameObject animal in animalsToRemove)
        {
            spawnedAnimals.Remove(animal);
            Destroy(animal);
        }
    }

    public void UpdateAllSpeeds()
    {
        foreach (GameObject animal in spawnedAnimals)
        {
            if (animal == null) continue;
            Animal animalScript = animal.GetComponent<Animal>();
            animalScript.UpdateSpeed();
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

        if (!Application.isPlaying) return;

        AnimalSpawner animal = (AnimalSpawner)target;

        GUILayout.Space(10);

        foreach (GameObject animalPrefab in animal.animalPrefabs)
        {
            if (animalPrefab == null) continue;

            if (GUILayout.Button("New " + animalPrefab.name))
                animal.SpawnAnimal(animal.animalPrefabs.IndexOf(animalPrefab));
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Update Speeds"))
            animal.UpdateAllSpeeds();
    }
}
#endif