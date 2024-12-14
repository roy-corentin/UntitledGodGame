using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MapGenerator
{
    public class Map : MonoBehaviour
    {
        [SerializeField] private GameObject mapDotPrefab;
        [HideInInspector] public readonly List<List<Dot>> mapDots = new();
        public Transform mapParent;
        [SerializeField] private float mapDimension = 1f;
        public int size = 30;
        private Texture2D heightMap;
        private Texture2D temperatureMap;
        private List<float> heightMapValues;
        private List<float> temperatureMapValues;
        public float perlinScale = 20f;
        public float emplitude = 10f;
        [HideInInspector] public GameObject meshMap;
        [SerializeField] private Material meshMaterial;
        public static Map Instance;
        private Coroutine generateDotsCoroutine;
        private bool areDotsGenerated = false;
        private Coroutine generateAllCoroutine;
        [SerializeField] private int elementQuantity = 50;

        private void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            GenerateAll();
        }

        public void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.G)) GenerateAll();
            else if (Input.GetKeyDown(KeyCode.C))
            {
                ClearDots();
                ClearMesh();
            }
#endif

            if (OVRInput.GetDown(OVRInput.Button.Three)) GenerateAll(); // X
        }

        public void GenerateAll()
        {
            if (generateAllCoroutine != null) StopCoroutine(generateAllCoroutine);
            if (generateDotsCoroutine != null) StopCoroutine(generateDotsCoroutine);

            generateAllCoroutine = StartCoroutine(GenerateAllCacoutine());
        }

        public IEnumerator GenerateAllCacoutine()
        {
            ElementsSpawner.Instance.ClearElements();

            yield return null;

            areDotsGenerated = false;
            generateDotsCoroutine = StartCoroutine(GenerateDots());
            yield return new WaitUntil(() => areDotsGenerated);

            heightMapValues = PerlinNoiseHeightMapGenerator.GenerateHeightMapTexture(size, perlinScale, out heightMap, false);
            temperatureMapValues = PerlinNoiseHeightMapGenerator.GenerateHeightMapTexture(size, perlinScale, out temperatureMap, false);
            var heights = ConvertToListList(heightMapValues, size);
            var temperatures = ConvertToListList(temperatureMapValues, size);

            meshMaterial.SetTexture("_TempNoise", temperatureMap);
            meshMaterial.SetTexture("_HeightNoise", heightMap);
            meshMaterial.SetVector("_Offset_Temp", new(0.5f - mapParent.position.x, 0.5f - mapParent.position.z));
            meshMaterial.SetVector("_Offset_Height", new(0.5f - mapParent.position.x, 0.5f - mapParent.position.z));

            float parentY = mapParent.position.y;
            for (int x = 0; x < mapDots.Count; x++)
            {
                for (int z = 0; z < mapDots[x].Count; z++)
                {
                    int invertedZ = size - z - 1;
                    Dot dot = mapDots[x][z];
                    dot.SetYPosition((heights[x][invertedZ] / emplitude) + parentY);
                    dot.SetTemperature(temperatures[x][invertedZ]);
                }

                yield return null;
            }

            CreateMesh();

            yield return null;

            ElementsSpawner.Instance.SpawnTreeAllAroundMap(mapDots, elementQuantity);
        }

        public void UpdateHeightMap(SelectedDots dots)
        {
            float parentY = mapParent.position.y;

            try { heightMapValues[dots.centerDot.index] = dots.centerDot.dot.transform.position.y * emplitude + parentY; }
            catch (System.Exception e) { Debug.Log(e); return; }

            foreach (List<SelectedDot> circle in dots.surroundingDotsLayers)
            {
                foreach (SelectedDot dot in circle)
                {
                    if (dot.index < 0 || dot.index >= heightMapValues.Count) continue;
                    heightMapValues[dot.index] = dot.dot.transform.position.y * emplitude + parentY;
                }
            }

            heightMap = PerlinNoiseHeightMapGenerator.GenerateTexture(size, heightMapValues);
            meshMaterial.SetTexture("_HeightNoise", heightMap);
        }

        public void UpdateTemperatureMap(SelectedDots dots)
        {

            try { temperatureMapValues[dots.centerDot.index] = dots.centerDot.dot.temperature; }
            catch (System.Exception e) { Debug.Log(e); return; }

            foreach (List<SelectedDot> circle in dots.surroundingDotsLayers)
            {
                foreach (SelectedDot dot in circle)
                {
                    if (dot.index < 0 || dot.index >= temperatureMapValues.Count) continue;
                    temperatureMapValues[dot.index] = dot.dot.temperature;
                }
            }

            temperatureMap = PerlinNoiseHeightMapGenerator.GenerateTexture(size, temperatureMapValues);
            meshMaterial.SetTexture("_TempNoise", temperatureMap);
        }

        public static List<List<float>> ConvertToListList(List<float> list, int size)
        {
            List<List<float>> result = new();
            for (int i = 0; i < list.Count; i += size)
                result.Add(list.GetRange(i, size));
            return result;
        }


        public IEnumerator GenerateDots()
        {
            ClearDots();

            yield return null;

            Vector3 center = mapParent.position;
            Vector3 topLeft = center + new Vector3(-mapDimension / 2, 0, mapDimension / 2);
            float stepX = mapDimension / size;

            for (float x = topLeft.x, indexX = 0; mapDots.Count < size; x += stepX, indexX++)
            {
                var line = new List<Dot>();

                for (float z = topLeft.z, indexZ = 0; line.Count < size; z -= stepX, indexZ++)
                {
                    Vector3 position = new(x + (stepX / 2), 0, z - (stepX / 2));
                    Dot dot = Instantiate(mapDotPrefab, position, Quaternion.identity, mapParent).GetComponent<Dot>();
                    dot.gameObject.SetActive(false);
                    line.Add(dot);
                }

                mapDots.Add(line);

                yield return null;
            }

            areDotsGenerated = true;
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

            if (GUILayout.Button("Clear Dots"))
                map.ClearDots();

            if (GUILayout.Button("Clear Mesh"))
                map.ClearMesh();

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Generate All"))
                map.GenerateAll();

            if (GUILayout.Button("Generate Heightmap"))
            {
                PerlinNoiseHeightMapGenerator.GenerateHeightMapTexture(map.size, map.perlinScale, out Texture2D _, true);
            }
        }
    }

#endif
}
