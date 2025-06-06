using UnityEngine;
using MapGenerator;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System;

[System.Serializable]
public struct DotData
{
    public float yPosition;
    public float temperature;
    public bool hasElement;
}

[System.Serializable]
public struct DotCoord
{
    public int x;
    public int y;

    public DotCoord(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

[System.Serializable]
public struct AnimalData
{
    public float moveSpeed;
    public float maxMoveTime;
    public float thirstValue;
    public float decreaseThirstPerSecond;
    public float hungerValue;
    public float decreaseHungerPerSecond;
    public float sleepValue;
    public float decreaseSleepPerSecond;
    public int prefabIndex;
    public DotCoord dotIndex;
}

[System.Serializable]
public struct Save
{
    public List<DotData> dots;
    public List<AnimalData> animals;
    public int deadCount;
}

public class SaveManager : MonoBehaviour
{
    public Map map;
    public Button loadBtn;
    private string playerPrefKeyCount = "SavedMapCount";
    private string playerPrefsKey = "SavedMap";
    private Coroutine loadCoroutine;

    public void Start()
    {
        loadBtn.interactable = PlayerPrefs.HasKey(playerPrefKeyCount);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) SaveMap();
        if (Input.GetKeyDown(KeyCode.L)) LoadMap();
    }

    public void LoadMap()
    {
        if (loadCoroutine != null) StopCoroutine(loadCoroutine);
        loadCoroutine = StartCoroutine(LoadMapCoroutine());
    }

    public IEnumerator LoadMapCoroutine()
    {
        if (!PlayerPrefs.HasKey(playerPrefKeyCount)) yield break;

        int count = int.Parse(PlayerPrefs.GetString(playerPrefKeyCount));
        string mapJson = "";
        for (int i = 0; i < count; i++)
            mapJson += PlayerPrefs.GetString(playerPrefsKey + i);

        Save save = JsonUtility.FromJson<Save>(mapJson);

        StatsManager.Instance.deadCount = save.deadCount;

        ElementsSpawner.Instance.ClearElements();
        Map.Instance.ClearMesh();
        AnimalSpawner.Instance.RemoveAll();

        yield return null;

        int index = 0;
        int spawnIndex = 0;
        foreach (List<Dot> dot in map.mapDots)
        {
            foreach (Dot d in dot)
            {
                DotData dotData = save.dots[index];
                d.SetYPosition(dotData.yPosition);
                d.SetTemperature(dotData.temperature);
                if (dotData.hasElement)
                {
                    ElementsSpawner.Instance.SpawnElementOnDot(d);
                    spawnIndex++;
                    if (spawnIndex % 50 == 0) yield return null;
                }
                index++;
            }
        }

        yield return null;

        List<float> temperatures = new();
        List<float> yPositions = new();

        foreach (List<Dot> dot in map.mapDots)
        {
            List<float> temperaturesTemp = new();
            List<float> yPositionsTemp = new();
            foreach (Dot d in dot)
            {

                temperaturesTemp.Add(d.temperature);
                yPositionsTemp.Add(d.transform.position.y * Map.Instance.emplitude + Map.Instance.mapParent.position.y);
            }

            temperaturesTemp.Reverse();
            yPositionsTemp.Reverse();

            temperatures.AddRange(temperaturesTemp);
            yPositions.AddRange(yPositionsTemp);
        }

        yield return null;

        Map.Instance.CreateMesh();
        Map.Instance.UpdateHeightMap(yPositions);
        Map.Instance.UpdateTemperatureMap(temperatures);

        yield return null;

        spawnIndex = 0;
        foreach (AnimalData animalData in save.animals)
        {
            AnimalSpawner.Instance.SpawnAnimal(animalData);
            spawnIndex++;
            if (spawnIndex % 50 == 0) yield return null;
        }

        Debug.Log("Map loaded");
    }

    public void SaveMap()
    {
        Debug.Log("Starting to save map");

        Save save = new()
        {
            dots = new List<DotData>(),
            animals = new List<AnimalData>(),
            deadCount = StatsManager.Instance.deadCount
        };

        foreach (List<Dot> dot in map.mapDots)
        {
            foreach (Dot d in dot)
            {
                DotData dotData = new()
                {
                    yPosition = d.transform.position.y,
                    temperature = d.temperature,
                    hasElement = d.element != null
                };
                save.dots.Add(dotData);
            }
        }

        foreach (GameObject animal in AnimalSpawner.Instance.SpawnedAnimals)
        {
            Animal animalScript = animal.GetComponent<Animal>();
            AnimalData animalData = new()
            {
                moveSpeed = animalScript.moveSpeed,
                maxMoveTime = animalScript.maxMoveTime,
                thirstValue = animalScript.thirstValue,
                decreaseThirstPerSecond = animalScript.decreaseThirstPerSecond,
                hungerValue = animalScript.hungerValue,
                decreaseHungerPerSecond = animalScript.decreaseHungerPerSecond,
                sleepValue = animalScript.sleepValue,
                decreaseSleepPerSecond = animalScript.decreaseSleepPerSecond,
                prefabIndex = animalScript.prefabIndex,
                dotIndex = LocationManager.Instance.GetNearestDot(animal)
            };
            save.animals.Add(animalData);
        }

        string mapJson = JsonUtility.ToJson(save);

        List<string> splitMapJson = new List<string>();
        for (int i = 0; i < mapJson.Length; i += 1000)
            splitMapJson.Add(mapJson.Substring(i, Mathf.Min(1000, mapJson.Length - i)));

        PlayerPrefs.SetString(playerPrefKeyCount, splitMapJson.Count.ToString());
        for (int i = 0; i < splitMapJson.Count; i++)
            PlayerPrefs.SetString(playerPrefsKey + i, splitMapJson[i]);

        Debug.Log("Map saved in " + splitMapJson.Count + " parts");
    }
}