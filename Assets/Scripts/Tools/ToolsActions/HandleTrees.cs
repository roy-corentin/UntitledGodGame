using System.Collections.Generic;
using MapGenerator;
using UnityEngine;

public class HandleTrees : ToolAction
{
    [Header("HandleTress")]
    public int spawnChanceByTick = 1;
    public int maxSpawnChance = 100;

    public override void EditDots(float pressure, SelectionDots selectedDots)
    {
        EditDot(selectedDots.centerDot.dot);

        for (int layerIndex = 0; layerIndex < selectedDots.surroundingDotsLayers.Count; layerIndex++)
        {
            List<SelectedDot> currentLayer = selectedDots.surroundingDotsLayers[layerIndex];

            foreach (SelectedDot selectedDot in currentLayer)
                EditDot(selectedDot.dot);
        }

        NavMeshHandler.Instance.needRebake = true;
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
