using System.Collections.Generic;
using UnityEngine;

public class SmoothHeight : ToolAction
{
    [SerializeField] private float smoothingSpeed = 0.5f;

    public override void Action(float pressure)
    {
        SelectedDots selectedDots = PlayerActions.Instance.GetSelectedDots();

        if (selectedDots.centerDot.dot == null) return;
        float finalY = selectedDots.centerDot.dot.transform.position.y;

        if (showSelectedDots)
            selectedDots.centerDot.dot.gameObject.SetActive(true);

        for (int circleIndex = 0; circleIndex < selectedDots.surroundingCircles.Count; circleIndex++)
        {
            List<SelectedDot> currentCircle = selectedDots.surroundingCircles[circleIndex];
            float smoothingFactor = (1.0f - ((float)circleIndex / selectedDots.surroundingCircles.Count)) * smoothingSpeed;

            foreach (SelectedDot dot in currentCircle)
            {
                float currentY = dot.dot.transform.position.y;
                float smoothedY = Mathf.Lerp(currentY, finalY, smoothingFactor * Time.deltaTime);

                dot.dot.SetYPosition(smoothedY);
                if (showSelectedDots)
                    dot.dot.gameObject.SetActive(true);
            }
        }


        MapGenerator.Map.Instance.CreateMesh();
        MapGenerator.Map.Instance.UpdateHeightMap(selectedDots);
    }
}