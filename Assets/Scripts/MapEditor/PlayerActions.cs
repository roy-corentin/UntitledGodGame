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
    public Dictionary<PlayerAction, Action<float>> actionMap = new();
    private GameObject playerHand = null;
    public static PlayerActions Instance;

    [Header("General")]
    public float range;
    public int numberOfCircles = 2;
    private int direction = 1;


    [Header("Height Dots")]
    public GameObject heightTools;
    private IsMoving heightIsMoving;
    public float centerMoveValue = 0.02f;
    public float surroundingMoveValue = 0.01f;

    [Header("Temp Dots")]
    public GameObject tempTools;
    private IsMoving tempIsMoving;
    public float centerTempValue = 0.02f;
    public float surroundingTempValue = 0.01f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        actionMap.Add(PlayerAction.ChangeHeight, (pressure) => ChangeHeight(pressure));
        actionMap.Add(PlayerAction.ChangeTemperature, (pressure) => ChangeTemperature(pressure));

        heightIsMoving = heightTools.GetComponent<IsMoving>();
        tempIsMoving = tempTools.GetComponent<IsMoving>();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.DownArrow)) direction = -1;
        else if (Input.GetKeyDown(KeyCode.UpArrow)) direction = 1;

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
            && actionMap.ContainsKey(currentAction)) actionMap[currentAction].Invoke(1f);
#else
        if (OVRInput.GetDown(OVRInput.Button.Two)) direction = -1; // B
        else if (OVRInput.GetDown(OVRInput.Button.One)) direction = 1; // A

        float pressure = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger); // Right trigger
        if (pressure != 0
            && playerHand != null
            && actionMap.ContainsKey(currentAction)) actionMap[currentAction].Invoke(pressure);
#endif
    }

    public void SetAction(PlayerAction action)
    {
        currentAction = action;

        playerHand = currentAction switch
        {
            PlayerAction.ChangeHeight => heightTools,
            PlayerAction.ChangeTemperature => tempTools,
            _ => null,
        };
    }

    private SelectedDots GetSelectedDots()
    {
        if (playerHand == null) return new SelectedDots();

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
                float distance = Vector3.Distance(dot.transform.position, playerHandPosition);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    selectedDot.dot = dot;
                    selectedDot.i = i;
                    selectedDot.j = j;
                }
            }
        }

        // if the nearest dot is too far, return an empty SelectedDots
        if (nearestDistance > range) return new SelectedDots();

        SelectedDots selectedDots = new() { centerDot = selectedDot, surroundingCircles = new() };

        // add the 8 surrounding dots to the selectedDots
        for (int circle = 1; circle <= numberOfCircles; circle++)
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
                    {
                        currentCircleDots.Add(new SelectedDot { dot = mapDots[indexI][indexJ], i = indexI, j = indexJ });
                    }
                }
            }

            // Add the current circle to the list of surrounding circles
            if (currentCircleDots.Count > 0)
                selectedDots.surroundingCircles.Add(currentCircleDots);
        }

        return selectedDots;
    }

    private void ChangeHeight(float pressure)
    {
        SelectedDots selectedDots = GetSelectedDots();

        if (selectedDots.centerDot.dot == null) return;

        selectedDots.centerDot.dot.SetYPosition(selectedDots.centerDot.dot.transform.position.y + centerMoveValue * direction * pressure);
        for (int circleIndex = 0; circleIndex < selectedDots.surroundingCircles.Count; circleIndex++)
        {
            List<SelectedDot> currentCircle = selectedDots.surroundingCircles[circleIndex];
            float moveValue = Mathf.Lerp(centerMoveValue, surroundingMoveValue, (float)(circleIndex + 1) / numberOfCircles);

            foreach (SelectedDot dot in currentCircle)
                dot.dot.SetYPosition(dot.dot.transform.position.y + moveValue * direction * pressure);
        }

        MapGenerator.Map.Instance.CreateMesh();
        MapGenerator.Map.Instance.UpdateHeightMap(selectedDots);
    }

    private void ChangeTemperature(float pressure)
    {
        SelectedDots selectedDots = GetSelectedDots();

        if (selectedDots.centerDot.dot == null) return;

        selectedDots.centerDot.dot.SetTemperature(selectedDots.centerDot.dot.temperature + centerTempValue * direction * pressure);
        for (int circleIndex = 0; circleIndex < selectedDots.surroundingCircles.Count; circleIndex++)
        {
            List<SelectedDot> currentCircle = selectedDots.surroundingCircles[circleIndex];
            float moveValue = Mathf.Lerp(centerMoveValue, surroundingTempValue, (float)(circleIndex + 1) / numberOfCircles);

            foreach (SelectedDot dot in currentCircle)
                dot.dot.SetTemperature(dot.dot.temperature + moveValue * direction * pressure);
        }

        MapGenerator.Map.Instance.UpdateTemperatureMap(selectedDots);
    }
}