using System;
using System.Collections.Generic;
using UnityEngine;

public struct SelectedDot
{
    public MapGenerator.Dot dot;
    public int j;
    public int i;
}

public struct SelectionDots
{
    public SelectedDot centerDot;
    public List<List<SelectedDot>> surroundingDotsLayers;
}

public class PlayerActions : MonoBehaviour
{
    public Camera playerCamera;
    public ToolAction currentTool;
    public static PlayerActions Instance;
    private SelectionDots lastSelectedDots;
    public float actionCooldown = 0.1f;
    private float nextActionTime = 0f;
    public float moveSpeed = 1f;

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

        if (Input.GetKeyDown(KeyCode.Space) && currentTool != null)
            if (!currentTool.audioSource.isPlaying) currentTool.audioSource.Play();

        if (Input.GetKeyUp(KeyCode.Space) && currentTool != null)
            currentTool.audioSource.Stop();

        if (Input.GetKey(KeyCode.Space)
            && currentTool != null
            && Time.time >= nextActionTime)
        {
            currentTool.Action(1f);
            nextActionTime = Time.time + actionCooldown;
        }
#else
        if (ARtoVR.Instance.GetCurrentMode() == GameMode.AR) ARInputs();
        else VRInputs();
#endif
    }

    private void ARInputs()
    {
        if (MapGenerator.Map.Instance.areDotsGenerated) return;

        if (OVRInput.GetDown(OVRInput.Button.Two)) SetActionType(-currentTool.actionType); // B

        float pressure = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger); // Right trigger
        if (pressure != 0
            && currentTool != null
            && Time.time >= nextActionTime)
        {
            if (!currentTool.audioSource.isPlaying) currentTool.audioSource.Play();
            currentTool.Action(1f);
            nextActionTime = Time.time + actionCooldown;
        }

        if (pressure == 0 && currentTool != null)
        {
            HideAllSelectedDots();
            currentTool.audioSource.Stop();
        }
    }

    private void VRInputs()
    {
        Vector2 joystick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick); // Left joystick
        Vector3 playerForward = playerCamera.transform.forward;
        playerForward.y = 0;
        playerForward.Normalize();
        Vector3 playerRight = playerCamera.transform.right;
        playerRight.y = 0;
        playerRight.Normalize();
        Vector3 moveDirection = playerForward * joystick.y + playerRight * joystick.x;
        MapGenerator.Map.Instance.transform.position -= moveDirection * Time.deltaTime * moveSpeed;

        float heightJoystick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y; // Right joystick
        MapGenerator.Map.Instance.transform.position += Vector3.up * -heightJoystick * Time.deltaTime * moveSpeed;
    }

    private void SetActionType(int actionType)
    {
        currentTool.actionType = actionType;
    }

    public SelectionDots GetSelectedDots()
    {
        if (currentTool == null) return new SelectionDots();

        List<List<MapGenerator.Dot>> mapDots = MapGenerator.Map.Instance.mapDots;
        Vector3 playerHandPosition = currentTool.transform.position;

        if (mapDots.Count == 0) return new SelectionDots();

        SelectedDot selectedDot = new() { dot = mapDots[0][0], j = 0, i = 0 };
        float nearestDistance = Vector3.Distance(selectedDot.dot.transform.position, playerHandPosition);

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
                    selectedDot.dot = dot;
                    selectedDot.i = i;
                    selectedDot.j = j;
                }

                if (dot.gameObject.activeSelf) dot.gameObject.SetActive(false);
            }
        }

        // if the nearest dot is too far, return an empty SelectedDots
        if (nearestDistance > currentTool.triggerRange) return new SelectionDots();
        if (Mathf.Abs(selectedDot.dot.transform.position.y - playerHandPosition.y) > currentTool.triggerRange) return new SelectionDots();

        SelectionDots selectedDots = new() { centerDot = selectedDot, surroundingDotsLayers = new() };

        for (int layerIndex = 1; layerIndex <= currentTool.actionRange; layerIndex++)
        {
            List<SelectedDot> currentLayerDots = new();

            for (int iOffset = -layerIndex; iOffset <= layerIndex; iOffset++)
            {
                for (int jOffset = -layerIndex; jOffset <= layerIndex; jOffset++)
                {
                    int i = selectedDot.i + iOffset;
                    int j = selectedDot.j + jOffset;

                    // Check if the point is on the edge of the current layer
                    if (IsPointOnEdgeOfLayer(layerIndex, i, j, iOffset, jOffset, mapDots))
                        currentLayerDots.Add(new SelectedDot { dot = mapDots[i][j], j = j, i = i });

                    try { if (mapDots[i][j].gameObject.activeSelf) mapDots[i][j].gameObject.SetActive(false); }
                    catch (ArgumentOutOfRangeException) { }
                }
            }

            if (currentLayerDots.Count > 0)
                selectedDots.surroundingDotsLayers.Add(currentLayerDots);
        }

        lastSelectedDots = selectedDots;

        return selectedDots;
    }

    public void HideAllSelectedDots()
    {
        try
        {
            if (lastSelectedDots.centerDot.dot != null) lastSelectedDots.centerDot.dot.gameObject.SetActive(false);
            foreach (List<SelectedDot> layer in lastSelectedDots.surroundingDotsLayers)
                foreach (SelectedDot dot in layer)
                    if (dot.dot != null) dot.dot.gameObject.SetActive(false);
        }
        catch (NullReferenceException) { }
    }

    private bool IsPointOnEdgeOfLayer(int layer, int i, int j, int iOffset, int jOffset, List<List<MapGenerator.Dot>> mapDots)
    {
        return (Mathf.Abs(iOffset) == layer || Mathf.Abs(jOffset) == layer) &&
                i >= 0 && i < mapDots.Count &&
                j >= 0 && j < mapDots[i].Count;
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
}
