using System.Collections.Generic;
using UnityEngine;

public class Toolbox : MonoBehaviour
{
    // public Camera playerCamera;
    public Transform leftController;
    [SerializeField] private float distanceUp = -0.35f;
    public List<ToolAction> toolActions = new();

    public void Start()
    {
        foreach (Transform child in transform)
        {
            ToolAction toolAction = child.GetComponent<ToolAction>();
            if (toolAction) toolActions.Add(toolAction);

            child.gameObject.SetActive(false);
        }
    }

    public void Update()
    {
        transform.SetPositionAndRotation(leftController.position + leftController.up * distanceUp, leftController.rotation);
    }

    public void ShowTool(ToolAction toolAction)
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(false);
        toolAction.gameObject.SetActive(true);
    }
}