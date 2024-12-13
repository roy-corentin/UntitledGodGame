using UnityEngine;

namespace MapGenerator
{
    public class Dot : MonoBehaviour
    {
        public float temperature;
        public GameObject element;

        public void SetTemperature(float temperature)
        {
            this.temperature = temperature;
        }

        public void SetYPosition(float y)
        {
            Vector3 pos = transform.position;
            pos.y = y;
            transform.position = pos;
        }

        public void UpdateElementPosition()
        {
            if (!element) return;
            element.transform.position = transform.position;
        }
    }
}