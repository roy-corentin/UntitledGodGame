using UnityEngine;
using MapGenerator;

public class LocationManager
{
    public static GameObject GetRandomLocation(int padding = 0)
    {
        int randomX = Random.Range(padding, Map.Instance.mapDots.Count - padding);
        int randomY = Random.Range(padding, Map.Instance.mapDots[randomX].Count - padding);

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
}