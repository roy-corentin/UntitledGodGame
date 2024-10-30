using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerAction
{
    ChangeHeight,
    ChangeTemperature,
    None
}

public struct SelectedDot
{
    public MapGenerator.Dot dot;
    public int i;
    public int j;
}

public struct SelectedDots
{
    public SelectedDot centerDot;
    public List<List<SelectedDot>> surroundingCircles;
}

public class PlayerActions : MonoBehaviour
{
    public PlayerAction currentAction = PlayerAction.None;
    public List<ToolAction> toolActions = new();
    private readonly Dictionary<PlayerAction, ToolAction> actionMap = new();
    private GameObject playerHand = null;
    public static PlayerActions Instance;
    private SelectedDots lastSelectedDots;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        foreach (ToolAction toolAction in toolActions)
            actionMap.Add(toolAction.actionType, toolAction);
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.DownArrow)) SetDirection(-1);
        else if (Input.GetKeyDown(KeyCode.UpArrow)) SetDirection(1);

        if (playerHand != null)
        {
            if (Input.GetKey(KeyCode.W)) playerHand.transform.position += Vector3.forward * Time.deltaTime;
            else if (Input.GetKey(KeyCode.A)) playerHand.transform.position += Vector3.left * Time.deltaTime;
            else if (Input.GetKey(KeyCode.S)) playerHand.transform.position += Vector3.back * Time.deltaTime;
            else if (Input.GetKey(KeyCode.D)) playerHand.transform.position += Vector3.right * Time.deltaTime;
            if (Input.GetKey(KeyCode.Q)) playerHand.transform.position += Vector3.up * Time.deltaTime;
            else if (Input.GetKey(KeyCode.E)) playerHand.transform.position += Vector3.down * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Space)
            && playerHand != null
            && actionMap.ContainsKey(currentAction)) actionMap[currentAction].Action(1f);
#else
        if (OVRInput.GetDown(OVRInput.Button.Two)) direction = -1; // B
        else if (OVRInput.GetDown(OVRInput.Button.One)) direction = 1; // A

        float pressure = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger); // Right trigger
        if (pressure != 0
            && playerHand != null
            && actionMap.ContainsKey(currentAction)) actionMap[currentAction].Action(pressure);
#endif
    }

    private void SetDirection(int direction)
    {
        if (actionMap.ContainsKey(currentAction)) actionMap[currentAction].direction = direction;
    }

    public void SetAction(PlayerAction action)
    {
        currentAction = action;

        if (!actionMap.ContainsKey(currentAction))
        {
            playerHand = null;
            return;
        }

        playerHand = actionMap[currentAction].Select();
    }

    public SelectedDots GetSelectedDots()
    {
        if (playerHand == null) return new SelectedDots();
        if (!actionMap.ContainsKey(currentAction)) return new SelectedDots();

        List<List<MapGenerator.Dot>> mapDots = MapGenerator.Map.Instance.mapDots;
        Vector3 playerHandPosition = playerHand.transform.position;

        if (mapDots.Count == 0) return new SelectedDots();

        SelectedDot selectedDot = new() { dot = mapDots[0][0], i = 0, j = 0 };
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
        if (nearestDistance > actionMap[currentAction].range) return new SelectedDots();
        if (Mathf.Abs(selectedDot.dot.transform.position.y - playerHandPosition.y) > actionMap[currentAction].range) return new SelectedDots();

        SelectedDots selectedDots = new() { centerDot = selectedDot, surroundingCircles = new() };

        // add the 8 surrounding dots to the selectedDots
        for (int circle = 1; circle <= actionMap[currentAction].numberOfCircles; circle++)
        {
            List<SelectedDot> currentCircleDots = new();

            for (int iOffset = -circle; iOffset <= circle; iOffset++)
            {
                for (int jOffset = -circle; jOffset <= circle; jOffset++)
                {
                    int indexI = selectedDot.i + iOffset;
                    int indexJ = selectedDot.j + jOffset;

                    // Check if the point is on the edge of the current circle
                    if ((Mathf.Abs(iOffset) == circle || Mathf.Abs(jOffset) == circle) &&
                        indexI >= 0 && indexI < mapDots.Count &&
                        indexJ >= 0 && indexJ < mapDots[indexI].Count)
                        currentCircleDots.Add(new SelectedDot { dot = mapDots[indexI][indexJ], i = indexI, j = indexJ });

                    try { if (mapDots[indexI][indexJ].gameObject.activeSelf) mapDots[indexI][indexJ].gameObject.SetActive(false); }
                    catch (ArgumentOutOfRangeException) { }
                }
            }

            if (currentCircleDots.Count > 0)
                selectedDots.surroundingCircles.Add(currentCircleDots);
        }

        lastSelectedDots = selectedDots;

        return selectedDots;
    }

    public void HideAllSelectedDots()
    {
        try
        {
            if (lastSelectedDots.centerDot.dot != null) lastSelectedDots.centerDot.dot.gameObject.SetActive(false);
            foreach (List<SelectedDot> circle in lastSelectedDots.surroundingCircles)
                foreach (SelectedDot dot in circle)
                    if (dot.dot != null) dot.dot.gameObject.SetActive(false);
        }
        catch (NullReferenceException) { }
    }
}