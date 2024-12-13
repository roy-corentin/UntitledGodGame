using System.Collections.Generic;
using UnityEngine;

public class ChangeHeight : ToolAction
{
    [Header("Change Height")]
    public float centerMoveValue = 0.02f;
    public float surroundingMoveValue = 0.01f;

    public override void Action(float pressure)
    {
        SelectedDots selectedDots = PlayerActions.Instance.GetSelectedDots();

        if (selectedDots.centerDot.dot == null) return;

        selectedDots.centerDot.dot.SetYPosition(selectedDots.centerDot.dot.transform.position.y + centerMoveValue * direction * pressure);
        if (selectedDots.centerDot.dot.element)
            selectedDots.centerDot.dot.element.transform.position = selectedDots.centerDot.dot.transform.position;
        if (showSelectedDots) selectedDots.centerDot.dot.gameObject.SetActive(true);

        for (int circleIndex = 0; circleIndex < selectedDots.surroundingCircles.Count; circleIndex++)
        {
            List<SelectedDot> currentCircle = selectedDots.surroundingCircles[circleIndex];
            float moveValue = Mathf.Lerp(centerMoveValue, surroundingMoveValue, (float)(circleIndex + 1) / numberOfCircles);

            foreach (SelectedDot selectedDot in currentCircle)
            {
                float newY = selectedDot.dot.transform.position.y + moveValue * direction * pressure;
                selectedDot.dot.SetYPosition(newY);
                if (showSelectedDots && circleIndex == selectedDots.surroundingCircles.Count - 1) selectedDot.dot.gameObject.SetActive(true);
                if (selectedDot.dot.element)
                    selectedDot.dot.element.transform.position = selectedDot.dot.transform.position;
            }
        }

        MapGenerator.Map.Instance.CreateMesh();
        MapGenerator.Map.Instance.UpdateHeightMap(selectedDots);
    }
}