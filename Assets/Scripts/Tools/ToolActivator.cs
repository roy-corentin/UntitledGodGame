using UnityEngine;

public class ToolActivator : MonoBehaviour
{
    private Vector3 lastPosition;
    private PlayerAction playerAction;
    private bool isMoving;
    private Vector3 originalPosition;
    private Vector3 originalRotation;

    public void Start()
    {
        lastPosition = transform.position;
        isMoving = false;
        playerAction = GetComponent<ToolAction>().actionType;
        originalPosition = transform.localPosition;
        originalRotation = transform.eulerAngles;
    }

    public void FixedUpdate()
    {
#if UNITY_EDITOR
        Vector3 currentPosition = transform.position;

        if (currentPosition != lastPosition && !isMoving) OnStartMoving();
        else if (currentPosition == lastPosition && isMoving) isMoving = false;

        if (Input.GetKeyDown(KeyCode.I) && isMoving) OnStopMoving();

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
        ReturnToToolbox();
    }

    public virtual void ReturnToToolbox()
    {
        transform.localPosition = originalPosition;
        transform.rotation = Quaternion.Euler(originalRotation);
    }
}