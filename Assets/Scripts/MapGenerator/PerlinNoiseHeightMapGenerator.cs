using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PerlinNoiseHeightMapGenerator : MonoBehaviour
{
    [HideInInspector] public static float randomOffsetRange = 100f;
    private readonly static string folderName = "Heightmaps";

    public static List<float> GenerateHeightMapTexture(int size, float scale, out Texture2D texture, bool saveHasFile = false)
    {
        List<float> heightMap = GenerateHeightMap(size, scale);
        texture = GenerateTexture(size, heightMap);

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

    public static Texture2D GenerateTexture(int size, List<float> heightMap)
    {
        Texture2D texture = new(size, size);
        int index = 0;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float sample = heightMap[index++];
                Color color = new(sample, sample, sample);
                texture.SetPixel(y, x, color);
            }
        }

        texture.Apply();
        texture.wrapMode = TextureWrapMode.Clamp;

        return texture;
    }

    static List<float> GenerateHeightMap(int size, float scale)
    {
        List<float> heightMap = new();

        float offsetX = Random.Range(0f, randomOffsetRange);
        float offsetY = Random.Range(0f, randomOffsetRange);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float xCoord = (float)x / size * scale + offsetX;
                float yCoord = (float)y / size * scale + offsetY;

                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                heightMap.Add(sample);
            }
        }

        return heightMap;
    }
}
