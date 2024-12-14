using System.Collections.Generic;
using UnityEngine;

public class ChangeHeight : ToolAction
{
    [Header("Change Height")]
    public float centerMoveValue = 0.02f;
    public float surroundingMoveValue = 0.01f;

    public override void EditDots(float pressure, SelectedDots selectedDots)
    {
        UpdateHeightDot(pressure, selectedDots.centerDot, centerMoveValue);

        for (int circleIndex = 0; circleIndex < selectedDots.surroundingCircles.Count; circleIndex++)
        {
            List<SelectedDot> currentCircle = selectedDots.surroundingCircles[circleIndex];
            float moveValue = Mathf.Lerp(centerMoveValue, surroundingMoveValue, (float)(circleIndex + 1) / actionRange);

            foreach (SelectedDot selectedDot in currentCircle)
            {
                UpdateHeightDot(pressure, selectedDot, moveValue);
            }
        }

        MapGenerator.Map.Instance.CreateMesh();
        MapGenerator.Map.Instance.UpdateHeightMap(selectedDots);
    }

    private void UpdateHeightDot(float pressure, SelectedDot selectedDot, float moveValue) {
        float newY = selectedDot.dot.transform.position.y + moveValue * pressure;
        if (actionType == REMOVE) newY *= -1;

        selectedDot.dot.SetYPosition(newY);
        if (selectedDot.dot.element)
            selectedDot.dot.element.transform.position = selectedDot.dot.transform.position;
    }
}
