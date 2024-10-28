using UnityEngine;

[RequireComponent(typeof(ToolActivator))]
public class ToolAction : MonoBehaviour
{
    [Header("Tool Action")]
    [HideInInspector] public PlayerAction actionType;
    protected GameObject toolGO;
    public float range;
    public int numberOfCircles = 2;
    public int direction = 1;
    [SerializeField] protected bool showSelectedDots = false;

    public virtual void Action(float pressure) { }

    public virtual GameObject Select() { return toolGO; }
}