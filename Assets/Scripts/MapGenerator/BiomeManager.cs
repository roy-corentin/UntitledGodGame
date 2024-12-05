using MapGenerator;
using UnityEngine;

public enum Biome
{
    Desert,
    Forest,
    Mountains,
    Tundra,
    Water
}

public class BiomeManager : MonoBehaviour
{
    public float mountainsHeight = 0.5f;
    public float waterHeight = 0.1f;
    public float tempDesert = 0.5f;
    public float tempTundra = 0.2f;

    public static BiomeManager Instance;

    public void Awake()
    {
        Instance = this;
    }

    public Biome GetBiome(Dot dot)
    {
        if (dot.transform.position.y < waterHeight) return Biome.Water;
        if (dot.transform.position.y > mountainsHeight) return Biome.Mountains;

        if (dot.temperature < tempTundra) return Biome.Tundra;
        if (dot.temperature > tempDesert) return Biome.Desert;

        return Biome.Forest;
    }
}