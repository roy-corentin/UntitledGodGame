using System.Collections.Generic;
using UnityEngine;

public enum LaserPointerColor
{
    Default,
    Active,
}

public struct LaserRaycastData
{
    public Vector3 startPoint;
    public Vector3 rayDirection;
}

public class LaserPointerData : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private LaserPointer laserPointer;
    public Material defaultMat;
    public Material activeMat;
    private Dictionary<LaserPointerColor, Material> materials;

    private void Awake()
    {
        materials = new Dictionary<LaserPointerColor, Material>
        {
            {LaserPointerColor.Default, defaultMat},
            {LaserPointerColor.Active, activeMat}
        };

        lineRenderer = GetComponent<LineRenderer>();
        laserPointer = GetComponent<LaserPointer>();
    }

    public void SetColor(LaserPointerColor color)
    {
        if (lineRenderer.material == materials[color]) return;
        if (materials.ContainsKey(color))
            lineRenderer.material = materials[color];
    }

    public void Show(bool show)
    {
        laserPointer.laserBeamBehavior = show ? LaserPointer.LaserBeamBehavior.OnWhenHitTarget : LaserPointer.LaserBeamBehavior.Off;
    }

    public bool IsVisible()
    {
        return lineRenderer.enabled;
    }

    public Vector3 GetPosition(int index)
    {
        return lineRenderer.GetPosition(index);
    }

    public int GetPositionCount()
    {
        return lineRenderer.positionCount;
    }

    public LaserRaycastData GetRaycastData()
    {
        return new LaserRaycastData
        {
            startPoint = GetPosition(0),
            rayDirection = GetPosition(GetPositionCount() - 1) - GetPosition(0)
        };
    }
}