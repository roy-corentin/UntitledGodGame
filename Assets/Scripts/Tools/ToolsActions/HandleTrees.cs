using System.Collections.Generic;
using MapGenerator;
using UnityEngine;

public class HandleTrees : ToolAction
{
    [Header("HandleTress")]
    public int spawnChanceByTick = 1;
    public int maxSpawnChance = 100;

    public override void Action(float pressure)
    {
        SelectedDots selectedDots = PlayerActions.Instance.GetSelectedDots();

        if (selectedDots.centerDot.dot == null) return;

        EditDot(selectedDots.centerDot.dot);
        if (showSelectedDots) selectedDots.centerDot.dot.gameObject.SetActive(true);

        for (int circleIndex = 0; circleIndex < selectedDots.surroundingCircles.Count; circleIndex++)
        {
            List<SelectedDot> currentCircle = selectedDots.surroundingCircles[circleIndex];

            foreach (SelectedDot selectedDot in currentCircle)
            {
                EditDot(selectedDot.dot);
                if (showSelectedDots && circleIndex == selectedDots.surroundingCircles.Count - 1)
                    selectedDot.dot.gameObject.SetActive(true);
            }
        }

        Map.Instance.CreateMesh();
        Map.Instance.UpdateHeightMap(selectedDots);
    }

    private bool CanSpawnElement()
    {
        return Random.Range(0, maxSpawnChance) < spawnChanceByTick;
    }

    private void EditDot(Dot dot)
    {
        if (direction == 1)
        {
            if (CanSpawnElement()) ElementsSpawner.Instance.SpawnElementOnDot(dot);
        }
        else ElementsSpawner.Instance.DestroyElement(dot.element);
    }

    public void SetSpawnChanceByTick(float spawnChanceByTick)
    {
        this.spawnChanceByTick = (int)spawnChanceByTick;
    }
}