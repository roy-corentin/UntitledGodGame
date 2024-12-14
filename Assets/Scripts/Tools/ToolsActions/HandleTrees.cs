using System.Collections.Generic;
using MapGenerator;
using UnityEngine;

public class HandleTrees : ToolAction
{
    [Header("HandleTress")]
    public int spawnChanceByTick = 1;
    public int maxSpawnChance = 100;

    public override void EditDots(float pressure, SelectedDots selectedDots)
    {
        EditDot(selectedDots.centerDot.dot);

        for (int circleIndex = 0; circleIndex < selectedDots.surroundingCircles.Count; circleIndex++)
        {
            List<SelectedDot> currentCircle = selectedDots.surroundingCircles[circleIndex];

            foreach (SelectedDot selectedDot in currentCircle)
            {
                EditDot(selectedDot.dot);
            }
        }
    }

    private bool CanSpawnElement()
    {
        return Random.Range(0, maxSpawnChance) < spawnChanceByTick;
    }

    private void EditDot(Dot dot)
    {
        if (actionType == ADD)
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
