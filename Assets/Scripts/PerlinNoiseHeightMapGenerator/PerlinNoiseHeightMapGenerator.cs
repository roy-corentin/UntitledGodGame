using System.IO;
using UnityEngine;

public class PerlinNoiseHeightMapGenerator : MonoBehaviour
{
    // Dimensions de l'image
    public int width = 512;
    public int height = 512;

    // Facteur d'échelle pour le bruit de Perlin
    public float scale = 20f;

    // Position de départ pour générer des variations dans le bruit
    public float offsetX = 100f;
    public float offsetY = 100f;

    // Nom du fichier à sauvegarder
    public string fileName = "heightmap.png";

    // Nom du répertoire pour sauvegarder les Heightmaps
    private string folderName = "Heightmaps";

    void Start()
    {
        // Générer et sauvegarder la HeightMap
        GenerateAndSaveHeightMap();
    }

    void GenerateAndSaveHeightMap()
    {
        // Créer une texture 2D
        Texture2D texture = GenerateHeightMapTexture();

        // Encoder la texture en PNG
        byte[] bytes = texture.EncodeToPNG();

        // Chemin du répertoire "Heightmaps"
        string folderPath = Path.Combine(Application.dataPath, folderName);

        // Vérifier si le répertoire existe, sinon le créer
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log($"Created directory: {folderPath}");
        }

        // Chemin complet du fichier PNG
        string filePath = Path.Combine(folderPath, fileName);

        // Sauvegarder l'image PNG dans le répertoire Heightmaps
        File.WriteAllBytes(filePath, bytes);
        Debug.Log($"HeightMap saved at: {filePath}");
    }

    Texture2D GenerateHeightMapTexture()
    {
        // Créer une nouvelle texture vide
        Texture2D texture = new Texture2D(width, height);

        // Remplir chaque pixel avec du bruit de Perlin
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xCoord = (float)x / width * scale + offsetX;
                float yCoord = (float)y / height * scale + offsetY;

                // Générer une valeur de hauteur avec Perlin Noise
                float sample = Mathf.PerlinNoise(xCoord, yCoord);

                // Appliquer cette valeur comme niveau de gris (HeightMap)
                Color color = new Color(sample, sample, sample);
                texture.SetPixel(x, y, color);
            }
        }

        // Appliquer les modifications à la texture
        texture.Apply();

        return texture;
    }
}
