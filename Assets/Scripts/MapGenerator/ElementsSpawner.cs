using System.Collections;
using System.Collections.Generic;
using MapGenerator;
using UnityEngine;

public class ElementsSpawner : MonoBehaviour
{
    [SerializeField] private GameObject tree;
    [SerializeField] private GameObject deadTree;
    [SerializeField] private GameObject fireTree;
    [SerializeField] private GameObject rock;
    [SerializeField] private Transform elementsParent;
    public readonly List<GameObject> elements = new();
    public static ElementsSpawner Instance;
    private Coroutine spawnTreeAllAroundMapCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnTreeAllAroundMap(List<List<Dot>> mapDots, int number)
    {
        if (spawnTreeAllAroundMapCoroutine != null) StopCoroutine(spawnTreeAllAroundMapCoroutine);
        spawnTreeAllAroundMapCoroutine = StartCoroutine(SpawnTreeAllAroundMapCoroutine(mapDots, number));
    }

    public IEnumerator SpawnTreeAllAroundMapCoroutine(List<List<Dot>> mapDots, int number)
    {
        for (int i = 0; i < number; i++)
        {
            Dot dot = mapDots[Random.Range(0, mapDots.Count)][Random.Range(0, mapDots[0].Count)];
            if (dot.element) continue;

            Biome biome = dot.biome;
            if (biome == Biome.Water) continue;

            GameObject prefab = GetPrefab(biome);
            GameObject newTree = Instantiate(prefab, elementsParent);
            Vector3 position = dot.transform.position;
            Vector3 rotation = new(0, Random.Range(0, 360), 0);
            Vector3 scale = newTree.transform.localScale;
            scale *= Random.Range(0.8f, 1.2f);

            newTree.transform.localPosition = position;
            newTree.transform.localEulerAngles = rotation;
            newTree.transform.localScale = scale;
            dot.element = newTree;

            elements.Add(newTree);

            if (i % 20 == 0) yield return null;
        }
    }

    public void ClearElements()
    {
        foreach (GameObject element in elements)
#if UNITY_EDITOR
            DestroyImmediate(element);
#else
            Destroy(element);
#endif

        elements.Clear();
    }

    public void SpawnElementOnDot(Dot dot)
    {
        if (dot.element) return;
        if (dot.biome == Biome.Water) return;

        GameObject newElement = Instantiate(GetPrefab(dot.biome), elementsParent);
        Vector3 rotation = new(0, Random.Range(0, 360), 0);
        Vector3 scale = newElement.transform.localScale;
        scale *= Random.Range(0.8f, 1.2f);

        newElement.transform.localPosition = dot.transform.position;
        newElement.transform.localEulerAngles = rotation;
        newElement.transform.localScale = scale;

        dot.element = newElement;
        elements.Add(newElement);
    }

    private GameObject GetPrefab(Biome biome)
    {
        return biome switch
        {
            Biome.Desert => deadTree,
            Biome.Forest => tree,
            Biome.Mountains => rock,
            Biome.Tundra => fireTree,
            _ => tree,
        };
    }

    public void DestroyElement(GameObject element)
    {
        if (element == null) return;
        elements.Remove(element);
        Destroy(element);
    }

    public void UpdatePrefab(Dot dot)
    {
        if (!dot.element) return;

        if (dot.lastBiome == dot.biome) return;
        dot.lastBiome = dot.biome;

        DestroyElement(dot.element);
        if (dot.biome == Biome.Water) return;

        GameObject prefab = GetPrefab(dot.biome);
        GameObject newElement = Instantiate(prefab, elementsParent);
        Vector3 rotation = new(0, Random.Range(0, 360), 0);
        Vector3 scale = newElement.transform.localScale;
        scale *= Random.Range(0.8f, 1.2f);

        newElement.transform.localPosition = dot.transform.position;
        newElement.transform.localEulerAngles = rotation;
        newElement.transform.localScale = scale;

        dot.element = newElement;
        elements.Add(newElement);
    }
}