using System;
using System.Collections.Generic;
using UnityEngine;

public struct SelectedDot
{
    public MapGenerator.Dot dot;
    public int index;
}

public struct SelectionDots
{
    public SelectedDot centerDot;
    public SelectedDot[][] surroundingDotsLayers;
}

public class PlayerActions : MonoBehaviour
{
    public ToolAction currentTool;
    public static PlayerActions Instance;
    private SelectionDots lastSelectedDots;
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
        currentTool.actionType = actionType;
    }

    public SelectionDots GetSelectedDots()
    {
        if (currentTool == null) return new SelectionDots();

        List<List<MapGenerator.Dot>> mapDots = MapGenerator.Map.Instance.mapDots;
        Vector3 playerHandPosition = currentTool.transform.position;

        if (mapDots.Count == 0) return new SelectionDots();

        Vector2 playerHandPosition2D = new(playerHandPosition.x, playerHandPosition.z);
        SelectedDot centerDot = GetNearestDot(mapDots, playerHandPosition2D, out int centerI, out int centerJ);
        if (centerDot.dot == null) return new SelectionDots();

        SelectionDots selectedDots = new() { centerDot = centerDot, surroundingDotsLayers = new SelectedDot[currentTool.actionRange][] };

        for (int layerIndex = 1; layerIndex <= currentTool.actionRange; layerIndex++)
        {
            int dotLayerIndex = 0;
            SelectedDot[] currentLayerDots = new SelectedDot[layerIndex * 8];

            for (int iOffset = -layerIndex; iOffset <= layerIndex; iOffset++)
                for (int jOffset = -layerIndex; jOffset <= layerIndex; jOffset++)
                {
                    int i = centerI + iOffset;
                    int j = centerJ + jOffset;

                    // Check if the point is on the edge of the current layer
                    if (IsInMap(layerIndex, iOffset, jOffset, i, j, mapDots))
                    {
                        currentLayerDots[dotLayerIndex].dot = mapDots[i][j];
                        currentLayerDots[dotLayerIndex].index = i * MapGenerator.Map.Instance.size - j;
                        dotLayerIndex++;
                    }

                    try { if (mapDots[i][j].gameObject.activeSelf) mapDots[i][j].gameObject.SetActive(false); }
                    catch (ArgumentOutOfRangeException) { }
                }

            if (dotLayerIndex > 0)
                selectedDots.surroundingDotsLayers[layerIndex - 1] = currentLayerDots;
        }

        lastSelectedDots = selectedDots;

        return selectedDots;
    }

    public void HideAllSelectedDots()
    {
        try
        {
            if (lastSelectedDots.centerDot.dot != null) lastSelectedDots.centerDot.dot.gameObject.SetActive(false);
            foreach (SelectedDot[] layer in lastSelectedDots.surroundingDotsLayers)
                foreach (SelectedDot dot in layer)
                    if (dot.dot != null) dot.dot.gameObject.SetActive(false);
        }
        catch (NullReferenceException) { }
    }

    private SelectedDot GetNearestDot(List<List<MapGenerator.Dot>> mapDots, Vector2 target2D, out int centerI, out int centerJ)
    {
        SelectedDot result = new() { dot = mapDots[0][0], index = -1 };
        float nearestDistance = currentTool.triggerRange;

        centerI = 0;
        centerJ = 0;

        // get the nearest dot to the playerHand
        for (int i = 0; i < mapDots.Count; i++)
        {
            for (int j = 0; j < mapDots[i].Count; j++)
            {
                MapGenerator.Dot dot = mapDots[i][j];
                Vector2 dotPosition2D = new(dot.transform.position.x, dot.transform.position.z);
                float distance = Vector2.Distance(target2D, dotPosition2D);

                // TODO maybe use Vector3.Distance ?
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    result.dot = dot;
                    result.index = (i - 1) * MapGenerator.Map.Instance.size + j;
                    centerI = i;
                    centerJ = j;
                }

                if (dot.gameObject.activeSelf) dot.gameObject.SetActive(false);
            }
        }

        if (result.dot && Mathf.Abs(result.dot.transform.position.y - target2D.y) > currentTool.triggerRange)
            result.dot = null;

        return result;
    }

    private bool IsInMap(int circle, int iOffset, int jOffset, int i, int j, List<List<MapGenerator.Dot>> mapDots)
    {
        bool temp = (Mathf.Abs(iOffset) == circle || Mathf.Abs(jOffset) == circle)
                    && i >= 0
                    && i < mapDots.Count
                    && j >= 0
                    && j < mapDots[i].Count;
        if (!temp) Debug.Log("not is point on edge of layer");
        return temp;
    }
}
