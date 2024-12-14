using System;
using System.Collections.Generic;
using UnityEngine;

public struct SelectedDot
{
    public MapGenerator.Dot dot;
    public int index;
}

public struct SelectedDots
{
    public SelectedDot centerDot;
    public List<List<SelectedDot>> surroundingDotsLayer;
}

public class PlayerActions : MonoBehaviour
{
    public ToolAction currentTool;
    public static PlayerActions Instance;
    private SelectedDots lastSelectedDots;
    public float actionCooldown = 0.1f;
    private float nextActionTime = 0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.DownArrow)) SetActionType(ToolAction.REMOVE);
        else if (Input.GetKeyDown(KeyCode.UpArrow)) SetActionType(ToolAction.ADD);

        if (currentTool != null)
        {
            if (Input.GetKey(KeyCode.W)) currentTool.transform.position += Vector3.forward * Time.deltaTime;
            else if (Input.GetKey(KeyCode.A)) currentTool.transform.position += Vector3.left * Time.deltaTime;
            else if (Input.GetKey(KeyCode.S)) currentTool.transform.position += Vector3.back * Time.deltaTime;
            else if (Input.GetKey(KeyCode.D)) currentTool.transform.position += Vector3.right * Time.deltaTime;
            if (Input.GetKey(KeyCode.Q)) currentTool.transform.position += Vector3.up * Time.deltaTime;
            else if (Input.GetKey(KeyCode.E)) currentTool.transform.position += Vector3.down * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Space)
            && currentTool != null
            && Time.time >= nextActionTime)
        {
            currentTool.Action(1f);
            nextActionTime = Time.time + actionCooldown;
        }
#else
        if (OVRInput.GetDown(OVRInput.Button.Two)) SetDirection(-currentTool.actionType); // B

        float pressure = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger); // Right trigger
        if (pressure != 0
            && currentTool != null
            && Time.time >= nextActionTime)
        {
            currentTool.Action(1f);
            nextActionTime = Time.time + actionCooldown;
        }
#endif
    }

    private void SetActionType(int actionType)
    {
        currentTool.actionType = actionType;
    }

    public SelectedDots GetSelectedDots()
    {
        if (currentTool == null) return new SelectedDots();

        List<List<MapGenerator.Dot>> mapDots = MapGenerator.Map.Instance.mapDots;
        Vector3 playerHandPosition = currentTool.transform.position;

        if (mapDots.Count == 0) return new SelectedDots();

        SelectedDot centerDot = new() { dot = mapDots[0][0], index = 0 };
        float nearestDistance = Vector3.Distance(centerDot.dot.transform.position, playerHandPosition);

        // get the nearest dot to the playerHand
        for (int i = 0; i < mapDots.Count; i++)
        {
            for (int j = 0; j < mapDots[i].Count; j++)
            {
                MapGenerator.Dot dot = mapDots[i][j];
                Vector2 playerHandPosition2D = new(playerHandPosition.x, playerHandPosition.z);
                Vector2 dotPosition2D = new(dot.transform.position.x, dot.transform.position.z);
                float distance = Vector2.Distance(playerHandPosition2D, dotPosition2D);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    centerDot.dot = dot;
                    centerDot.index = i * mapDots[i].Count + j;
                }

                if (dot.gameObject.activeSelf) dot.gameObject.SetActive(false);
            }
        }

        // if the nearest dot is too far, return an empty SelectedDots
        if (nearestDistance > currentTool.triggerRange) return new SelectedDots();
        if (Mathf.Abs(centerDot.dot.transform.position.y - playerHandPosition.y) > currentTool.triggerRange) return new SelectedDots();

        SelectedDots selectedDots = new() { centerDot = centerDot, surroundingDotsLayer = new() };

        for (int layerIndex = 1; layerIndex <= currentTool.actionRange; layerIndex++)
        {
            List<SelectedDot> currentLayerDots = new();

            for (int iOffset = -layerIndex; iOffset <= layerIndex; iOffset++)
            {
                for (int jOffset = -layerIndex; jOffset <= layerIndex; jOffset++)
                {
                    int indexI = centerDot.index / mapDots.Count + iOffset;
                    int indexJ = centerDot.index % mapDots.Count + jOffset;

                    // Check if the point is on the edge of the current layer
                    if (isPointOnEdgeOfLayer(layerIndex, indexI, indexJ, iOffset, jOffset, mapDots))
                        currentLayerDots.Add(new SelectedDot { dot = mapDots[indexI][indexJ], i = indexI, j = indexJ });

                    try { if (mapDots[indexI][indexJ].gameObject.activeSelf) mapDots[indexI][indexJ].gameObject.SetActive(false); }
                    catch (ArgumentOutOfRangeException) { }
                }
            }

            if (currentLayerDots.Count > 0)
                selectedDots.surroundingDotsLayer.Add(currentLayerDots);
        }

        lastSelectedDots = selectedDots;

        return selectedDots;
    }

    public void HideAllSelectedDots()
    {
        try
        {
            if (lastSelectedDots.centerDot.dot != null) lastSelectedDots.centerDot.dot.gameObject.SetActive(false);
            foreach (List<SelectedDot> layer in lastSelectedDots.surroundingDotsLayer)
                foreach (SelectedDot dot in layer)
                    if (dot.dot != null) dot.dot.gameObject.SetActive(false);
        }
        catch (NullReferenceException) { }
    }

    private bool isPointOnEdgeOfLayer(int layer, int i, int j, int iOffset, int jOffset, List<List<MapGenerator.Dot>> mapDots)
    {
        return (Mathf.Abs(iOffset) == layer || Mathf.Abs(jOffset) == layer) &&
               i >= 0 && i < mapDots.Count &&
               j >= 0 && j < mapDots[i].Count;
    }
}
