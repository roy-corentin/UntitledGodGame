using System.Collections.Generic;
using UnityEngine;

public class Flatten : ToolAction
{
    public override void EditDots(float pressure, SelectedDots selectedDots)
    {
        float finalY = selectedDots.centerDot.dot.transform.position.y;
        if (selectedDots.centerDot.dot.element)
            selectedDots.centerDot.dot.element.transform.position = selectedDots.centerDot.dot.transform.position;

        for (int circleIndex = 0; circleIndex < selectedDots.surroundingCircles.Count; circleIndex++)
        {
            List<SelectedDot> currentCircle = selectedDots.surroundingCircles[circleIndex];

            foreach (SelectedDot selectedDot in currentCircle)
            {
                float currentY = selectedDot.dot.transform.position.y;
                int dotIndex = currentCircle.IndexOf(selectedDot);
                int nummberOfDots = currentCircle.Count;
                float moveValue = Mathf.Lerp(currentY, finalY, (float)(dotIndex + 1) / nummberOfDots);

                selectedDot.dot.SetYPosition(moveValue);
                if (selectedDot.dot.element)
                    selectedDot.dot.element.transform.position = selectedDot.dot.transform.position;
            }
        }

        MapGenerator.Map.Instance.CreateMesh();
        MapGenerator.Map.Instance.UpdateHeightMap(selectedDots);
    }
}
