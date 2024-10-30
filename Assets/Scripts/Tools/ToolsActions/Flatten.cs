using System.Collections.Generic;
using UnityEngine;

public class Flatten : ToolAction
{
    void Awake()
    {
        actionType = PlayerAction.Flatten;
        toolGO = gameObject;
    }

    public override void Action(float pressure)
    {
        SelectedDots selectedDots = PlayerActions.Instance.GetSelectedDots();

        if (selectedDots.centerDot.dot == null) return;
        float finalY = selectedDots.centerDot.dot.transform.position.y;

        if (showSelectedDots) selectedDots.centerDot.dot.gameObject.SetActive(true);
        for (int circleIndex = 0; circleIndex < selectedDots.surroundingCircles.Count; circleIndex++)
        {
            List<SelectedDot> currentCircle = selectedDots.surroundingCircles[circleIndex];

            foreach (SelectedDot dot in currentCircle)
            {
                float currentY = dot.dot.transform.position.y;
                int dotIndex = currentCircle.IndexOf(dot);
                int nummberOfDots = currentCircle.Count;
                float moveValue = Mathf.Lerp(currentY, finalY, (float)(dotIndex + 1) / nummberOfDots);
                dot.dot.SetYPosition(moveValue);
                if (showSelectedDots) dot.dot.gameObject.SetActive(true);
            }
        }

        MapGenerator.Map.Instance.CreateMesh();
        MapGenerator.Map.Instance.UpdateHeightMap(selectedDots);
    }
}