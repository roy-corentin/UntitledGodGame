using System.Collections.Generic;
using UnityEngine;

namespace MapGenerator
{
    public class Map : MonoBehaviour
    {
        [SerializeField] private GameObject mapDotPrefab;
        private readonly List<Dot> mapDots = new();
        [SerializeField] private Transform mapParent;
        [SerializeField] private float mapDimension = 1f;
        [SerializeField] private float nbDotsPerLine = 30;

        private void Start()
        {
            GenerateMap();
        }

        private void GenerateMap()
        {
            Vector3 center = mapParent.position;
            Vector3 topLeft = center + new Vector3(-mapDimension / 2, 0, mapDimension / 2);
            Vector3 bottomRight = center + new Vector3(mapDimension / 2, 0, -mapDimension / 2);
            float stepX = mapDimension / nbDotsPerLine;

            for (float x = topLeft.x; x < bottomRight.x; x += stepX)
            {
                for (float z = topLeft.z; z > bottomRight.z; z -= stepX)
                {
                    Vector3 position = new(x, 0, z);
                    Dot dot = Instantiate(mapDotPrefab, position, Quaternion.identity, mapParent).GetComponent<Dot>();
                    mapDots.Add(dot);
                }
            }
        }
    }
}
