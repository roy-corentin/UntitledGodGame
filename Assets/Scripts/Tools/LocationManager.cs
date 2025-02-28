using UnityEngine;
using MapGenerator;
using System.Collections.Generic;

public class LocationManager : MonoBehaviour
{
    public static LocationManager Instance;
    readonly List<Dot> NotWaterDots = new();
    readonly List<Dot> DesertDots = new();
    readonly List<Dot> ForestDots = new();
    readonly List<Dot> MountainsDots = new();
    readonly List<Dot> TundraDots = new();
    readonly List<Dot> WaterDots = new();
    bool isInitialized = false;

    public void Awake()
    {
        Instance = this;
    }

    public void UpdateGroundDots()
    {
        NotWaterDots.Clear();
        foreach (List<Dot> dots in Map.Instance.mapDots)
            foreach (Dot dot in dots)
            {
                if (dot.biome == Biome.Desert) DesertDots.Add(dot);
                if (dot.biome == Biome.Forest) ForestDots.Add(dot);
                if (dot.biome == Biome.Mountains) MountainsDots.Add(dot);
                if (dot.biome == Biome.Tundra) TundraDots.Add(dot);
                if (dot.biome != Biome.Water && dot.biome != Biome.DeepWater) NotWaterDots.Add(dot);
                if (dot.biome == Biome.Water) WaterDots.Add(dot);
            }

        isInitialized = true;
    }

    public DotCoord GetNearestDot(GameObject target)
    {
        List<List<Dot>> dots = Map.Instance.mapDots;
        float nearestDistance = float.MaxValue;
        DotCoord nearestDot = new(0, 0);

        for (int x = 0; x < dots.Count; x++)
        {
            for (int y = 0; y < dots[x].Count; y++)
            {
                float distance = Vector3.Distance(target.transform.position, dots[x][y].transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestDot.x = x;
                    nearestDot.y = y;
                }
            }
        }

        return nearestDot;
    }

    public Dot GetNearestDotOfType(GameObject target, Biome type, List<GameObject> exclude = null)
    {
        if (!isInitialized)
            UpdateGroundDots();

        List<Dot> dots = type switch
        {
            Biome.Desert => DesertDots,
            Biome.Forest => ForestDots,
            Biome.Mountains => MountainsDots,
            Biome.Tundra => TundraDots,
            Biome.Water => WaterDots,
            _ => NotWaterDots
        };
        float nearestDistance = float.MaxValue;
        Dot nearestDot = null;

        for (int x = 0; x < dots.Count; x++)
        {
            if (exclude != null && exclude.Contains(dots[x].gameObject)) continue;
            Biome dotbiome = dots[x].biome;
            if (dotbiome != type) continue;

            float distance = Vector3.Distance(target.transform.position, dots[x].transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestDot = dots[x];
            }
        }

        return nearestDot;
    }

    public Dot GetNearestGroundPosition(GameObject target, List<GameObject> exclude = null)
    {
        if (!isInitialized) UpdateGroundDots();

        List<Dot> notWaterDotsCopy = new(NotWaterDots);

        if (exclude != null)
            foreach (GameObject ex in exclude)
                notWaterDotsCopy.Remove(ex.GetComponent<Dot>());

        if (notWaterDotsCopy.Count == 0) return null;

        float nearestDistance = float.MaxValue;
        Dot nearestDot = null;

        for (int x = 0; x < notWaterDotsCopy.Count; x++)
        {
            float distance = Vector3.Distance(target.transform.position, notWaterDotsCopy[x].transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestDot = notWaterDotsCopy[x];
            }
        }

        return nearestDot;
    }

    public GameObject GetRandomGroundPosition(List<GameObject> exclude = null)
    {
        if (!isInitialized)
            UpdateGroundDots();

        List<Dot> notWaterDotsCopy = new(NotWaterDots);

        if (exclude != null)
            foreach (GameObject ex in exclude)
                notWaterDotsCopy.Remove(ex.GetComponent<Dot>());

        if (notWaterDotsCopy.Count == 0) return null;

        int randomIndex = UnityEngine.Random.Range(0, notWaterDotsCopy.Count);
        return notWaterDotsCopy[randomIndex].gameObject;
    }
}