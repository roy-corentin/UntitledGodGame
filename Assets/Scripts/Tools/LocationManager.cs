using UnityEngine;
using MapGenerator;
using System.Collections.Generic;

public class LocationManager
{
    public static GameObject GetRandomLocation(int padding = 0)
    {
        int randomX = UnityEngine.Random.Range(padding, Map.Instance.mapDots.Count - padding);
        int randomY = UnityEngine.Random.Range(padding, Map.Instance.mapDots[randomX].Count - padding);

        return Map.Instance.mapDots[randomX][randomY].gameObject;
    }

    public static GameObject GetRandomGroundPosition(int padding = 0)
    {
        int attempts = 0;
        do
        {
            GameObject randomPoint = GetRandomLocation(padding);

            if (randomPoint.transform.position.y > BiomeManager.Instance.waterHeight)
                return randomPoint;

            attempts++;
        } while (attempts < 100);

        return null;
    }

    public static DotCoord GetNearestDot(GameObject target)
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

    public static DotCoord GetNearestDotOfType(GameObject target, Biome type)
    {
        List<List<Dot>> dots = Map.Instance.mapDots;
        float nearestDistance = float.MaxValue;
        DotCoord nearestDot = new(0, 0);

        for (int x = 0; x < dots.Count; x++)
        {
            for (int y = 0; y < dots[x].Count; y++)
            {
                Biome dotbiome = dots[x][y].biome;
                if (dotbiome != type) continue;

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
}