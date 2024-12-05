using System.Collections;
using System.Collections.Generic;
using MapGenerator;
using UnityEngine;

public class ElementsSpawner : MonoBehaviour
{
    [SerializeField] private GameObject tree;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform elementsParent;
    [SerializeField] private float raycastDistance = 10f;
    private List<GameObject> elements = new();
    public static ElementsSpawner Instance;
    private Coroutine spawnTreeAllAroundMapCoroutine;
    [SerializeField] private float offset = 0.05f;

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
            if (dot.hasElement) continue;

            dot.hasElement = true;

            GameObject newTree = Instantiate(tree, elementsParent);
            Vector3 position = dot.transform.position;

            // add some random offset
            position.x += Random.Range(-offset, offset);
            position.z += Random.Range(-offset, offset);

            newTree.transform.localPosition = position;

            elements.Add(newTree);

            if (i % 20 == 0) yield return null;
        }
    }

    public void ClearElements()
    {
        foreach (GameObject element in elements)
            Destroy(element);

        elements.Clear();
    }
}