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
        public int nbDotsPerLine = 30;
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
        [HideInInspector] public bool areDotsGenerated = false;
        [HideInInspector] public bool isMeshGenerated = false;
        private Coroutine generateAllCoroutine;
        [SerializeField] private int elementQuantity = 50;
        private float startPressTime;
        public float timeToLongPress = 1.5f;

        private void Awake()
        {
            Instance = this;
        }

        public void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.G)) GenerateAll();
            if (Input.GetKeyDown(KeyCode.C))
            {
                ClearDots();
                ClearMesh();
            }
            if (Input.GetKeyDown(KeyCode.F)) FlatAll();
#endif
            if (ARtoVR.Instance.GetCurrentMode() == GameMode.AR && areDotsGenerated)
            {
                if (OVRInput.GetDown(OVRInput.Button.Three)) startPressTime = Time.time;
                if (OVRInput.GetUp(OVRInput.Button.Three))
                {
                    if (Time.time - startPressTime > timeToLongPress) FlatAll();
                    else GenerateAll();
                }
            }
        }

        public void FlatAll()
        {
            if (areDotsGenerated == false)
            {
                if (generateAllCoroutine != null) StopCoroutine(generateAllCoroutine);
                if (generateDotsCoroutine != null) StopCoroutine(generateDotsCoroutine);

                generateAllCoroutine = StartCoroutine(GenerateAllCacoutine(true));
                return;
            }

            List<float> heightMapValues = new();

            foreach (var line in mapDots)
            {
                foreach (var dot in line)
                {
                    dot.SetYPosition(BiomeManager.Instance.waterHeight - 0.01f);
                    if (dot.element)
                    {
                        dot.element.transform.position = dot.transform.position;
                        ElementsSpawner.Instance.UpdatePrefab(dot);
                    }
                    heightMapValues.Add(dot.transform.position.y);
                }
            }

            Instance.CreateMesh();
            Instance.UpdateHeightMap(heightMapValues);
            AnimalSpawner.Instance.RemoveAll();
        }

        public void GenerateAll()
        {
            if (generateAllCoroutine != null) StopCoroutine(generateAllCoroutine);
            if (generateDotsCoroutine != null) StopCoroutine(generateDotsCoroutine);

            generateAllCoroutine = StartCoroutine(GenerateAllCacoutine());
        }

        public IEnumerator GenerateAllCacoutine(bool flat = false)
        {
            ElementsSpawner.Instance.ClearElements();
            AnimalSpawner.Instance.RemoveAll();

            yield return null;

            areDotsGenerated = false;
            isMeshGenerated = false;
            generateDotsCoroutine = StartCoroutine(GenerateDots());
            yield return new WaitUntil(() => areDotsGenerated);

            heightMapValues = PerlinNoiseHeightMapGenerator.GenerateHeightMapTexture(nbDotsPerLine, nbDotsPerLine, perlinScale, out heightMap, false);
            temperatureMapValues = PerlinNoiseHeightMapGenerator.GenerateHeightMapTexture(nbDotsPerLine, nbDotsPerLine, perlinScale, out temperatureMap, false);
            var heights = ConvertToListList(heightMapValues, nbDotsPerLine);
            var temperatures = ConvertToListList(temperatureMapValues, nbDotsPerLine);

            meshMaterial.SetTexture("_TempNoise", temperatureMap);
            meshMaterial.SetTexture("_HeightNoise", heightMap);
            meshMaterial.SetVector("_Offset_Temp", new(0.5f - mapParent.position.x, 0.5f - mapParent.position.z));
            meshMaterial.SetVector("_Offset_Height", new(0.5f - mapParent.position.x, 0.5f - mapParent.position.z));

            float parentY = mapParent.position.y;
            for (int x = 0; x < mapDots.Count; x++)
            {
                for (int z = 0; z < mapDots[x].Count; z++)
                {
                    int invertedZ = nbDotsPerLine - z - 1;
                    Dot dot = mapDots[x][z];
                    dot.SetYPosition((heights[x][invertedZ] / emplitude) + parentY);
                    dot.SetTemperature(temperatures[x][invertedZ]);
                }

                if (x % 5 == 0) CreateMesh();
                yield return null;
            }

            CreateMesh();

            yield return null;

            ElementsSpawner.Instance.SpawnTreeAllAroundMap(mapDots, elementQuantity);

            yield return null;

            if (flat) FlatAll();

            isMeshGenerated = true;
        }

        public void UpdateHeightMap(List<float> heightMapValues)
        {
            this.heightMapValues = heightMapValues;
            heightMap = PerlinNoiseHeightMapGenerator.GenerateTexture(nbDotsPerLine, nbDotsPerLine, heightMapValues);
            meshMaterial.SetTexture("_HeightNoise", heightMap);
        }

        public void UpdateHeightMap(SelectionDots selectedDots)
        {
            float parentY = mapParent.position.y;
            int centerIndex = GetIndex(selectedDots.centerDot.i, selectedDots.centerDot.j);

            try { heightMapValues[centerIndex] = selectedDots.centerDot.dot.transform.position.y * emplitude + parentY; }
            catch (System.Exception e) { Debug.Log(e); return; }

            foreach (List<SelectedDot> circle in selectedDots.surroundingDotsLayers)
            {
                foreach (SelectedDot dot in circle)
                {
                    int index = GetIndex(dot.i, dot.j);
                    if (index < 0 || index >= heightMapValues.Count) continue;
                    heightMapValues[index] = dot.dot.transform.position.y * emplitude + parentY;
                }
            }

            Debug.Log(heightMapValues[centerIndex]);

            heightMap = PerlinNoiseHeightMapGenerator.GenerateTexture(nbDotsPerLine, nbDotsPerLine, heightMapValues);
            meshMaterial.SetTexture("_HeightNoise", heightMap);
        }

        public void UpdateTemperatureMap(List<float> temperatureMapValues)
        {
            this.temperatureMapValues = temperatureMapValues;
            temperatureMap = PerlinNoiseHeightMapGenerator.GenerateTexture(nbDotsPerLine, nbDotsPerLine, temperatureMapValues);
            meshMaterial.SetTexture("_TempNoise", temperatureMap);
        }

        public void UpdateTemperatureMap(SelectionDots selectedDots)
        {
            int centerIndex = GetIndex(selectedDots.centerDot.i, selectedDots.centerDot.j);

            try { temperatureMapValues[centerIndex] = selectedDots.centerDot.dot.temperature; }
            catch (System.Exception e) { Debug.Log(e); return; }

            foreach (List<SelectedDot> circle in selectedDots.surroundingDotsLayers)
            {
                foreach (SelectedDot dot in circle)
                {
                    int index = GetIndex(dot.i, dot.j);
                    if (index < 0 || index >= temperatureMapValues.Count) continue;
                    temperatureMapValues[index] = dot.dot.temperature;
                }
            }

            temperatureMap = PerlinNoiseHeightMapGenerator.GenerateTexture(nbDotsPerLine, nbDotsPerLine, temperatureMapValues);
            meshMaterial.SetTexture("_TempNoise", temperatureMap);
        }

        private int GetIndex(int i, int j)
        {
            return (i + 1) * nbDotsPerLine - (j + 1);
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
            if (mapDots.Count > 0)
            {
                areDotsGenerated = true;
                yield break;
            }

            yield return null;

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
            meshMap.layer = LayerMask.NameToLayer("Ground");
            meshMap.transform.SetParent(this.gameObject.transform);

            MeshFilter meshFilter = meshMap.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = meshMap.AddComponent<MeshRenderer>();
            MeshCollider meshCollider = meshMap.AddComponent<MeshCollider>();
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
            meshCollider.sharedMesh = mesh;
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
                PerlinNoiseHeightMapGenerator.GenerateHeightMapTexture(map.nbDotsPerLine, map.nbDotsPerLine, map.perlinScale, out Texture2D _, true);
            }
        }
    }

#endif
}
