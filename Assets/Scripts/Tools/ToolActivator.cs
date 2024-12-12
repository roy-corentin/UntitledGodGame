using UnityEngine;

public class ToolActivator : MonoBehaviour
{
    private Vector3 lastPosition;
    private bool isMoving;
    private Vector3 originalPosition;
    private Vector3 originalRotation;
    private ToolAction playerAction;

    public void Awake()
    {
        lastPosition = transform.localPosition;
        isMoving = false;
        originalPosition = transform.localPosition;
        originalRotation = transform.eulerAngles;
        playerAction = GetComponent<ToolAction>();
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
        Debug.Log($"LOG: OnStartMoving() {gameObject.name}");
        lastPosition = transform.localPosition;
        isMoving = true;
        PlayerActions.Instance.currentTool = playerAction;
    }

    public void OnStopMoving()
    {
        Debug.Log($"LOG: OnStopMoving() {gameObject.name}");
        isMoving = false;
        PlayerActions.Instance.currentTool = null;
        PlayerActions.Instance.HideAllSelectedDots();
        ReturnToToolbox();
    }

    public virtual void ReturnToToolbox()
    {
        transform.localPosition = originalPosition;
        transform.rotation = Quaternion.Euler(originalRotation);
    }
}