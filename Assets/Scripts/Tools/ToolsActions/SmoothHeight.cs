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
        if (selectedDots.centerDot.dot.element)
            selectedDots.centerDot.dot.element.transform.position = selectedDots.centerDot.dot.transform.position;

        if (showSelectedDots) selectedDots.centerDot.dot.gameObject.SetActive(true);

        for (int circleIndex = 0; circleIndex < selectedDots.surroundingCircles.Count; circleIndex++)
        {
            List<SelectedDot> currentCircle = selectedDots.surroundingCircles[circleIndex];
            float smoothingFactor = (1.0f - ((float)circleIndex / selectedDots.surroundingCircles.Count)) * smoothingSpeed;

            foreach (SelectedDot selectedDot in currentCircle)
            {
                float currentY = selectedDot.dot.transform.position.y;
                float smoothedY = Mathf.Lerp(currentY, finalY, smoothingFactor * Time.deltaTime);

                selectedDot.dot.SetYPosition(smoothedY);
                if (showSelectedDots && circleIndex == selectedDots.surroundingCircles.Count - 1) selectedDot.dot.gameObject.SetActive(true);
                if (selectedDot.dot.element)
                    selectedDot.dot.element.transform.position = selectedDot.dot.transform.position;
            }
        }


        MapGenerator.Map.Instance.CreateMesh();
        MapGenerator.Map.Instance.UpdateHeightMap(selectedDots);
    }

    public void SetSmoothingSpeed(float smoothingSpeed)
    {
        this.smoothingSpeed = smoothingSpeed;
    }
}