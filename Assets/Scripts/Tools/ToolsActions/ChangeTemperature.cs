using System.Collections.Generic;
using UnityEngine;

public class ChangeTemperature : ToolAction
{
    [Header("Change Temperature")]
    public float centerTempValue = 0.02f;
    public float surroundingTempValue = 0.01f;

    public override void EditDots(float pressure, SelectedDots selectedDots)
    {
        UpdateTemperatureDot(pressure, selectedDots.centerDot);

        for (int circleIndex = 0; circleIndex < selectedDots.surroundingCircles.Count; circleIndex++)
        {
            List<SelectedDot> currentCircle = selectedDots.surroundingCircles[circleIndex];
            float moveValue = Mathf.Lerp(centerTempValue, surroundingTempValue, (float)(circleIndex + 1) / actionRange);

            foreach (SelectedDot selectedDot in currentCircle)
            {
                UpdateTemperatureDot(pressure, selectedDot);
            }
        }

        MapGenerator.Map.Instance.UpdateTemperatureMap(selectedDots);
    }

    private void UpdateTemperatureDot(float pressure, SelectedDot selectedDot) {
        float newCenterDotTemp = selectedDot.dot.temperature + centerTempValue  * pressure;
        if (actionType == REMOVE) newCenterDotTemp *= -1;

        selectedDot.dot.SetTemperature(newCenterDotTemp);
        ElementsSpawner.Instance.UpdatePrefab(selectedDot.dot);
    }
}
