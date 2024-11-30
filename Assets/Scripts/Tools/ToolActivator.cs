using UnityEngine;

public class ToolActivator : MonoBehaviour
{
    private Vector3 lastPosition;
    private PlayerAction playerAction;
    private bool isMoving;
    private Vector3 originalPosition;
    private Vector3 originalRotation;

    public void Awake()
    {
        lastPosition = transform.localPosition;
        isMoving = false;
        playerAction = GetComponent<ToolAction>().actionType;
        originalPosition = transform.localPosition;
        originalRotation = transform.eulerAngles;
    }

    public void Update()
    {
#if UNITY_EDITOR
        Vector3 currentPosition = transform.localPosition;

        if (currentPosition != lastPosition && !isMoving) OnStartMoving();
        else if (currentPosition == lastPosition && isMoving) isMoving = false;

        if (Input.GetKeyDown(KeyCode.I) && isMoving) OnStopMoving();

        lastPosition = currentPosition;
#else
        float pressure = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);

        Vector3 currentPosition = transform.localPosition;

        if (currentPosition != lastPosition && !isMoving) OnStartMoving();
        else if (currentPosition == lastPosition && isMoving && pressure == 0) OnStopMoving();
        else
        {
            Debug.Log($"LOG: {currentPosition} - {lastPosition} - {pressure} - {isMoving}");
        }

        lastPosition = currentPosition;
#endif
    }

    public void OnStartMoving()
    {
        Debug.Log($"LOG: OnStartMoving() {this.playerAction}");
        lastPosition = transform.localPosition;
        isMoving = true;
        PlayerActions.Instance.SetAction(playerAction);
    }

    public void OnStopMoving()
    {
        Debug.Log($"LOG: OnStopMoving() {this.playerAction}");
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