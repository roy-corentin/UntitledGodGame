using System.Collections.Generic;
using UnityEngine;

public class ChangeHeight : ToolAction
{
    [Header("Change Height")]
    public float centerMoveValue = 0.02f;
    public float surroundingMoveValue = 0.01f;

    void Awake()
    {
        actionType = PlayerAction.ChangeHeight;
        toolGO = gameObject;
    }

    public override void Action(float pressure)
    {
        SelectedDots selectedDots = PlayerActions.Instance.GetSelectedDots();

        if (selectedDots.centerDot.dot == null) return;

        selectedDots.centerDot.dot.SetYPosition(selectedDots.centerDot.dot.transform.position.y + centerMoveValue * direction * pressure);
        if (showSelectedDots) selectedDots.centerDot.dot.gameObject.SetActive(true);
        for (int circleIndex = 0; circleIndex < selectedDots.surroundingCircles.Count; circleIndex++)
        {
            List<SelectedDot> currentCircle = selectedDots.surroundingCircles[circleIndex];
            float moveValue = Mathf.Lerp(centerMoveValue, surroundingMoveValue, (float)(circleIndex + 1) / numberOfCircles);

            foreach (SelectedDot dot in currentCircle)
            {
                dot.dot.SetYPosition(dot.dot.transform.position.y + moveValue * direction * pressure);
                if (showSelectedDots) dot.dot.gameObject.SetActive(true);
            }
        }

        MapGenerator.Map.Instance.CreateMesh();
        MapGenerator.Map.Instance.UpdateHeightMap(selectedDots);
    }
}