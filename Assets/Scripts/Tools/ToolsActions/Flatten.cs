using System.Collections.Generic;
using UnityEngine;

public class Flatten : ToolAction
{
    public override void EditDots(float pressure, SelectionDots selectedDots)
    {
        float finalY = selectedDots.centerDot.dot.transform.position.y;
        if (selectedDots.centerDot.dot.element)
            selectedDots.centerDot.dot.element.transform.position = selectedDots.centerDot.dot.transform.position;

        for (int layerIndex = 0; layerIndex < selectedDots.surroundingDotsLayers.Count; layerIndex++)
        {
            List<SelectedDot> currentLayer = selectedDots.surroundingDotsLayers[layerIndex];

            foreach (SelectedDot selectedDot in currentLayer)
            {
                float currentY = selectedDot.dot.transform.position.y;
                int dotIndex = currentLayer.IndexOf(selectedDot);
                int nummberOfDots = currentLayer.Count;
                float moveValue = Mathf.Lerp(currentY, finalY, (float)(dotIndex + 1) / nummberOfDots);
                moveValue = Mathf.Clamp(moveValue, MapGenerator.Map.Instance.minHeight, MapGenerator.Map.Instance.maxHeight);

                selectedDot.dot.SetYPosition(moveValue);
                if (selectedDot.dot.element)
                {
                    selectedDot.dot.element.transform.position = selectedDot.dot.transform.position;
                    ElementsSpawner.Instance.UpdatePrefab(selectedDot.dot);
                }
            }
        }

        MapGenerator.Map.Instance.CreateMesh();
        MapGenerator.Map.Instance.UpdateHeightMap(selectedDots);
    }
}
