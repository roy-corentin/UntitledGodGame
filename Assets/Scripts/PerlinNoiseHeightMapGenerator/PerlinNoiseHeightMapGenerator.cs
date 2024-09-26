using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PerlinNoiseHeightMapGenerator : MonoBehaviour
{
    [HideInInspector] public static float scale = 20f;
    [HideInInspector] public static float randomOffsetRange = 100f;
    private readonly static string folderName = "Heightmaps";

    public static List<float> GenerateHeightMapTexture(int width, int height, out Texture2D texture, bool saveHasFile = false)
    {
        List<float> heightMap = GenerateHeightMap(width, height);
        texture = GenerateTexture(width, height, heightMap);

        if (!saveHasFile) return heightMap;

        byte[] bytes = texture.EncodeToPNG();
        string folderPath = Path.Combine(Application.dataPath, folderName);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filename = $"heightmap_{System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.png";
        string filePath = Path.Combine(folderPath, filename);

        File.WriteAllBytes(filePath, bytes);

        Debug.Log($"Heightmap saved as {filename} in {folderPath}");

        return heightMap;
    }

    static Texture2D GenerateTexture(int width, int height, List<float> heightMap)
    {
        Texture2D texture = new(width, height);
        int index = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float sample = heightMap[index++];
                Color color = new(sample, sample, sample);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();

        return texture;
    }

    static List<float> GenerateHeightMap(int width, int height)
    {
        List<float> heightMap = new();

        float offsetX = Random.Range(0f, randomOffsetRange);
        float offsetY = Random.Range(0f, randomOffsetRange);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xCoord = (float)x / width * scale + offsetX;
                float yCoord = (float)y / height * scale + offsetY;

                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                heightMap.Add(sample);
            }
        }

        return heightMap;
    }
}
