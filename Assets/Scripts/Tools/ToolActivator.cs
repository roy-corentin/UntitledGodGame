using UnityEngine;

public class ToolActivator : MonoBehaviour
{
    private Vector3 lastPosition;
    private PlayerAction playerAction;
    private bool isMoving;

    public void Start()
    {
        lastPosition = transform.position;
        isMoving = false;
        playerAction = GetComponent<ToolAction>().actionType;
    }

    public void FixedUpdate()
    {
#if UNITY_EDITOR
        Vector3 currentPosition = transform.position;

        if (currentPosition != lastPosition && !isMoving) OnStartMoving();
        else if (currentPosition == lastPosition && isMoving) isMoving = false;

        lastPosition = currentPosition;
#else
        float pressure = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);

        Vector3 currentPosition = transform.position;

        if (currentPosition != lastPosition && !isMoving && pressure > 0) OnStartMoving();
        else if (currentPosition == lastPosition && isMoving && pressure == 0) OnStopMoving();

        lastPosition = currentPosition;
#endif
    }

    public void OnStartMoving()
    {
        lastPosition = transform.position;
        isMoving = true;
        PlayerActions.Instance.SetAction(playerAction);
    }

    public void OnStopMoving()
    {
        isMoving = false;
        PlayerActions.Instance.SetAction(PlayerAction.None);
        PlayerActions.Instance.HideAllSelectedDots();
    }
}