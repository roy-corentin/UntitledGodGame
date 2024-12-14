using System.Collections.Generic;
using UnityEngine;

public class ChangeTemperature : ToolAction
{
    [Header("Change Temperature")]
    public float centerTempValue = 0.02f;
    public float surroundingTempValue = 0.01f;

    public override void EditDots(float pressure, SelectionDots selectedDots)
    {
        UpdateTemperatureDot(pressure, selectedDots.centerDot, centerTempValue);

        for (int layerIndex = 0; layerIndex < selectedDots.surroundingDotsLayers.Count; layerIndex++)
        {
            List<SelectedDot> currentLayer = selectedDots.surroundingDotsLayers[layerIndex];
            float moveValue = Mathf.Lerp(centerTempValue, surroundingTempValue, (float)(layerIndex + 1) / actionRange);

            foreach (SelectedDot selectedDot in currentLayer)
            {
                UpdateTemperatureDot(pressure, selectedDot, moveValue);
            }
        }

        MapGenerator.Map.Instance.UpdateTemperatureMap(selectedDots);
    }

    private void UpdateTemperatureDot(float pressure, SelectedDot selectedDot, float moveValue) {
        float newCenterDotTemp = selectedDot.dot.temperature + moveValue  * pressure;
        if (actionType == REMOVE) newCenterDotTemp *= -1;

        selectedDot.dot.SetTemperature(newCenterDotTemp);
        ElementsSpawner.Instance.UpdatePrefab(selectedDot.dot);
    }
}
