using UnityEngine;

[RequireComponent(typeof(ToolActivator))]
public class ToolAction : MonoBehaviour
{
    [Header("Tool Action")]
    public float triggerRange;
    public int actionRange = 2;
    public const int ADD = 1;
    public const int REMOVE = -1;
    public int actionType = ADD;
    public float triggerRange;
    public int actionRange = 2;
    public const int ADD = 1;
    public const int REMOVE = -1;
    public int actionType = ADD;
    [SerializeField] protected bool showSelectedDots = false;

    public void Action(float pressure)
    {
        SelectionDots selectedDots = PlayerActions.Instance.GetSelectedDots();
        if (selectedDots.centerDot.dot == null) return;

        EditDots(pressure, selectedDots);

        if (showSelectedDots)
        {
            selectedDots.centerDot.dot.gameObject.SetActive(true);
            foreach (SelectedDot selectedDot in selectedDots.surroundingDotsLayers[selectedDots.surroundingDotsLayers.Count - 1])
            {
                selectedDot.dot.gameObject.SetActive(true);
            }
        }
    }

    public virtual void EditDots(float pressure, SelectionDots selectedDots) { }

    public void SetActionRange(float actionRange)
    {
        this.actionRange = (int)actionRange;
    }

    public void SetTriggerRange(float triggerRange)
    public void SetTriggerRange(float triggerRange)
    {
        this.triggerRange = triggerRange;
        this.triggerRange = triggerRange;
    }
}

