using UnityEngine;

namespace MapGenerator
{
    public class Dot : MonoBehaviour
    {
        public float temperature;
        public GameObject element;
        public Biome biome;
        public Biome lastBiome;

        public void SetTemperature(float temperature)
        {
            this.temperature = temperature;

            lastBiome = biome;
            biome = BiomeManager.Instance.GetBiome(this);
        }

        public void SetYPosition(float y)
        {
            Vector3 pos = transform.position;
            pos.y = y;
            transform.position = pos;

            lastBiome = biome;
            biome = BiomeManager.Instance.GetBiome(this);
        }

        public void UpdateElementPosition()
        {
            if (!element) return;
            element.transform.position = transform.position;
        }
    }
}