using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MapGenerator
{
    public class Map : MonoBehaviour
    {
        [SerializeField] private GameObject mapDotPrefab;
        [HideInInspector] public readonly List<List<Dot>> mapDots = new();
        public Transform mapParent;
        [SerializeField] private float mapDimension = 1f;
        public int nbDotsPerLine = 30;
        private Texture2D heightMap;
        private Texture2D temperatureMap;
        private List<float> heightMapValues;
        private List<float> temperatureMapValues;
        public float perlinScale = 20f;
        public float emplitude = 10f;
        public float moveYBy = 0.5f;
        [HideInInspector] public GameObject meshMap;
        [SerializeField] private Material meshMaterial;

        public void GenerateAll()
        {
            GenerateDots();

            heightMapValues = PerlinNoiseHeightMapGenerator.GenerateHeightMapTexture(nbDotsPerLine, nbDotsPerLine, perlinScale, out heightMap, false);
            temperatureMapValues = PerlinNoiseHeightMapGenerator.GenerateHeightMapTexture(nbDotsPerLine, nbDotsPerLine, perlinScale, out temperatureMap, false);
            var heights = ConvertToListList(heightMapValues, nbDotsPerLine);
            var temperatures = ConvertToListList(temperatureMapValues, nbDotsPerLine);

            meshMaterial.SetTexture("_TempNoise", temperatureMap);
            meshMaterial.SetTexture("_HeightNoise", heightMap);

            for (int x = 0; x < mapDots.Count; x++)
            {
                for (int z = 0; z < mapDots[x].Count; z++)
                {
                    int invertedZ = nbDotsPerLine - z - 1;
                    Dot dot = mapDots[x][z];
                    dot.SetYPosition((heights[x][invertedZ] / emplitude) - moveYBy);
                    dot.SetTemperature(temperatures[x][invertedZ]);
                }
            }

            CreateMesh();
        }

        public static List<List<float>> ConvertToListList(List<float> list, int size)
        {
            List<List<float>> result = new();
            for (int i = 0; i < list.Count; i += size)
                result.Add(list.GetRange(i, Mathf.Min(size, list.Count - i)));
            return result;
        }


        public void GenerateDots()
        {
            ClearDots();

            Vector3 center = mapParent.position;
            Vector3 topLeft = center + new Vector3(-mapDimension / 2, 0, mapDimension / 2);
            float stepX = mapDimension / nbDotsPerLine;

            for (float x = topLeft.x, indexX = 0; mapDots.Count < nbDotsPerLine; x += stepX, indexX++)
            {
                var line = new List<Dot>();

                for (float z = topLeft.z, indexZ = 0; line.Count < nbDotsPerLine; z -= stepX, indexZ++)
                {
                    Vector3 position = new(x + (stepX / 2), 0, z - (stepX / 2));
                    Dot dot = Instantiate(mapDotPrefab, position, Quaternion.identity, mapParent).GetComponent<Dot>();
                    line.Add(dot);
                }

                mapDots.Add(line);
            }
        }

        public void ClearDots()
        {
            foreach (var line in mapDots)
            {
                foreach (var dot in line)
                {
#if UNITY_EDITOR
                    DestroyImmediate(dot.gameObject);
#else
                    Destroy(dot.gameObject);
#endif
                }
            }

            // Si le mode editeur décide de ne pas être sympa, il faut supprimer manuellement les enfants et lancer cette fonction plusieurs fois
            if (mapDots.Count == 0 && mapParent.childCount > 0)
            {
                foreach (Transform child in mapParent)
                {
#if UNITY_EDITOR
                    DestroyImmediate(child.gameObject);
#else
                    Destroy(child.gameObject);
#endif
                }

                ClearDots();
            }

            mapDots.Clear();
        }

        public void CreateMesh()
        {
            ClearMesh();
            meshMap = new GameObject("Mesh Map");
            meshMap.transform.SetParent(this.gameObject.transform);

            MeshFilter meshFilter = meshMap.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = meshMap.AddComponent<MeshRenderer>();
            meshRenderer.material = meshMaterial;

            Mesh mesh = new();
            List<Vector3> vertices = new();
            List<int> triangles = new();

            for (int x = 0; x < mapDots.Count - 1; x++)
            {
                for (int z = 0; z < mapDots[x].Count - 1; z++)
                {
                    Dot dotBottomLeft = mapDots[x][z];          // Bas gauche
                    Dot dotBottomRight = mapDots[x + 1][z];     // Bas droit
                    Dot dotTopLeft = mapDots[x][z + 1];         // Haut gauche
                    Dot dotTopRight = mapDots[x + 1][z + 1];    // Haut droit

                    int vertexIndex = vertices.Count;
                    vertices.Add(dotBottomLeft.transform.position); // Index 0
                    vertices.Add(dotBottomRight.transform.position); // Index 1
                    vertices.Add(dotTopLeft.transform.position); // Index 2
                    vertices.Add(dotTopRight.transform.position); // Index 3

                    // Triangle 1 : Haut droit -> Haut gauche -> Bas gauche
                    triangles.Add(vertexIndex + 3); // Haut droit
                    triangles.Add(vertexIndex + 2); // Haut gauche
                    triangles.Add(vertexIndex);     // Bas gauche

                    // Triangle 2 : Bas droit -> Haut droit -> Bas gauche
                    triangles.Add(vertexIndex + 1); // Bas droit
                    triangles.Add(vertexIndex + 3); // Haut droit
                    triangles.Add(vertexIndex);     // Bas gauche
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();

            mesh.RecalculateNormals();
            mesh.RecalculateUVDistributionMetrics();
            mesh.RecalculateTangents();
            mesh.RecalculateUVDistributionMetrics();

            meshFilter.mesh = mesh;
        }

        public void ClearMesh()
        {
            if (meshMap != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(meshMap);
#else
                Destroy(meshMap);
#endif
            }
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(Map))]
    public class MapEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Map map = (Map)target;

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate Dots"))
            {
                map.GenerateDots();
            }

            if (GUILayout.Button("Clear Dots"))
                map.ClearDots();

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate Mesh"))
                map.CreateMesh();

            if (GUILayout.Button("Clear Mesh"))
                map.ClearMesh();

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Generate All"))
                map.GenerateAll();

            if (GUILayout.Button("Generate Heightmap"))
            {
                PerlinNoiseHeightMapGenerator.GenerateHeightMapTexture(map.nbDotsPerLine, map.nbDotsPerLine, map.perlinScale, out Texture2D _, true);
            }
        }
    }

#endif
}
