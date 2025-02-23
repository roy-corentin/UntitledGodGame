using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class StatsManager : MonoBehaviour
{
    [HideInInspector] public static StatsManager Instance;
    public TextMeshProUGUI animals;
    public TextMeshProUGUI dead;
    public TextMeshProUGUI carnivors;
    public TextMeshProUGUI herbivors;
    public TextMeshProUGUI water;
    public TextMeshProUGUI forest;
    public TextMeshProUGUI frost;
    public TextMeshProUGUI desert;
    public TextMeshProUGUI mountains;
    public TextMeshProUGUI FPS;
    public TextMeshProUGUI meanFPS;
    public TextMeshProUGUI lowFPS;
    [HideInInspector] public int deadCount;
    public GameObject menu;
    private float meanFPSValue = 0;
    private float lowFPSValue = 1000;

    private void Awake()
    {
        Instance = this;
    }

    public void Update()
    {
        if (!menu.activeSelf) return;
        if (Time.frameCount % 120 == 0)
        {
            float fps = 1 / Time.deltaTime;

            FPS.text = $"{fps:0}";
            meanFPSValue = (meanFPSValue + fps) / 2;
            meanFPS.text = $"{meanFPSValue:0}";

            if (fps < lowFPSValue)
            {
                lowFPSValue = fps;
                lowFPS.text = $"{lowFPSValue:0}";
            }
        }
    }

    public void UpdateStats()
    {
        int carnivorsCount = 0;
        int herbivorsCount = 0;
        foreach (GameObject animal in AnimalSpawner.Instance.spawnedAnimals)
        {
            if (animal.GetComponent<Animal>().foodType == FoodType.Carnivore) carnivorsCount++;
            else herbivorsCount++;
        }

        animals.text = AnimalSpawner.Instance.spawnedAnimals.Count.ToString();
        dead.text = deadCount.ToString();
        carnivors.text = carnivorsCount.ToString();
        herbivors.text = herbivorsCount.ToString();

        int waterCount = 0;
        int forestCount = 0;
        int frostCount = 0;
        int desertCount = 0;
        int mountainsCount = 0;
        foreach (List<MapGenerator.Dot> dot in MapGenerator.Map.Instance.mapDots)
        {
            foreach (MapGenerator.Dot d in dot)
            {
                switch (d.biome)
                {
                    case Biome.DeepWater:
                    case Biome.Water: waterCount++; break;
                    case Biome.Forest: forestCount++; break;
                    case Biome.Tundra: frostCount++; break;
                    case Biome.Desert: desertCount++; break;
                    case Biome.Mountains: mountainsCount++; break;
                }
            }
        }

        int totalDot = MapGenerator.Map.Instance.mapDots.Count * MapGenerator.Map.Instance.mapDots[0].Count;
        float waterPourcent = (float)waterCount / totalDot * 100;
        float forestPourcent = (float)forestCount / totalDot * 100;
        float frostPourcent = (float)frostCount / totalDot * 100;
        float desertPourcent = (float)desertCount / totalDot * 100;
        float mountainsPourcent = (float)mountainsCount / totalDot * 100;

        water.text = $"{waterPourcent:0.00}%";
        forest.text = $"{forestPourcent:0.00}%";
        frost.text = $"{frostPourcent:0.00}%";
        desert.text = $"{desertPourcent:0.00}%";
        mountains.text = $"{mountainsPourcent:0.00}%";
    }
}