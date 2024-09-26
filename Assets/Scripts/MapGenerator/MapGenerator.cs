using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MapGenerator
{
    public class Map : MonoBehaviour
    {
        [SerializeField] private GameObject mapDotPrefab;
        private readonly List<List<Dot>> mapDots = new();
        [SerializeField] private Transform mapParent;
        [SerializeField] private float mapDimension = 1f;
        public int nbDotsPerLine = 30;
        private GameObject meshMap;

        private void Start()
        {
            GenerateDots();
        }

        private void GenerateDots()
        {
            Vector3 center = mapParent.position;
            Vector3 topLeft = center + new Vector3(-mapDimension / 2, 0, mapDimension / 2);
            Vector3 bottomRight = center + new Vector3(mapDimension / 2, 0, -mapDimension / 2);
            float stepX = mapDimension / nbDotsPerLine;

            for (float x = topLeft.x; x < bottomRight.x; x += stepX)
            {
                var line = new List<Dot>();

                for (float z = topLeft.z; z > bottomRight.z; z -= stepX)
                {
                    Vector3 position = new(x, 0, z);
                    Dot dot = Instantiate(mapDotPrefab, position, Quaternion.identity, mapParent).GetComponent<Dot>();
                    line.Add(dot);
                }

                mapDots.Add(line);
            }
        }

        public void CreateMesh()
        {
            if (meshMap != null) Destroy(meshMap);
            meshMap = new GameObject("Mesh Map");
            meshMap.transform.SetParent(this.gameObject.transform);

            MeshFilter meshFilter = meshMap.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = meshMap.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Standard"));

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

            meshFilter.mesh = mesh;
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
            if (GUILayout.Button("Create Mesh"))
            {
                map.CreateMesh();
            }

            if (GUILayout.Button("Generate Heightmap"))
            {
                PerlinNoiseHeightMapGenerator.GenerateHeightMapTexture(map.nbDotsPerLine, map.nbDotsPerLine, out Texture2D _, true);
            }
        }
    }

#endif
}
