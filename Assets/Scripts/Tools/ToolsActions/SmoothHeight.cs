using System.Collections.Generic;
using UnityEngine;

public class SmoothHeight : ToolAction
{
    [SerializeField] private float smoothingSpeed = 0.5f;

    public override void EditDots(float pressure, SelectedDots selectedDots)
    {
        float finalY = selectedDots.centerDot.dot.transform.position.y;
        if (selectedDots.centerDot.dot.element)
            selectedDots.centerDot.dot.element.transform.position = selectedDots.centerDot.dot.transform.position;

        for (int layerIndex = 0; layerIndex < selectedDots.surroundingDotsLayers.Count; layerIndex++)
        {
            List<SelectedDot> currentLayer = selectedDots.surroundingDotsLayers[layerIndex];
            float smoothingFactor = (1.0f - ((float)layerIndex / selectedDots.surroundingDotsLayers.Count)) * smoothingSpeed;

            foreach (SelectedDot selectedDot in currentLayer)
            {
                float currentY = selectedDot.dot.transform.position.y;
                float smoothedY = Mathf.Lerp(currentY, finalY, smoothingFactor * Time.deltaTime);

                selectedDot.dot.SetYPosition(smoothedY);
                if (selectedDot.dot.element)
                    selectedDot.dot.element.transform.position = selectedDot.dot.transform.position;
            }
        }


        MapGenerator.Map.Instance.CreateMesh();
        MapGenerator.Map.Instance.UpdateHeightMap(selectedDots);
    }

    public void SetSmoothingSpeed(float smoothingSpeed)
    {
        this.smoothingSpeed = smoothingSpeed;
    }
}
