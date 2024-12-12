using System.Collections.Generic;
using UnityEngine;

public class ChangeTemperature : ToolAction
{
    [Header("Change Temperature")]
    public float centerTempValue = 0.02f;
    public float surroundingTempValue = 0.01f;

    public override void Action(float pressure)
    {
        SelectedDots selectedDots = PlayerActions.Instance.GetSelectedDots();

        if (selectedDots.centerDot.dot == null) return;

        selectedDots.centerDot.dot.SetTemperature(selectedDots.centerDot.dot.temperature + centerTempValue * direction * pressure);
        ElementsSpawner.Instance.UpdatePrefab(selectedDots.centerDot.dot);
        if (showSelectedDots) selectedDots.centerDot.dot.gameObject.SetActive(true);
        for (int circleIndex = 0; circleIndex < selectedDots.surroundingCircles.Count; circleIndex++)
        {
            List<SelectedDot> currentCircle = selectedDots.surroundingCircles[circleIndex];
            float moveValue = Mathf.Lerp(centerTempValue, surroundingTempValue, (float)(circleIndex + 1) / numberOfCircles);

            foreach (SelectedDot selectedDot in currentCircle)
            {
                selectedDot.dot.SetTemperature(selectedDot.dot.temperature + moveValue * direction * pressure);
                if (showSelectedDots) selectedDot.dot.gameObject.SetActive(true);
                ElementsSpawner.Instance.UpdatePrefab(selectedDot.dot);
            }
        }

        MapGenerator.Map.Instance.UpdateTemperatureMap(selectedDots);
    }
}