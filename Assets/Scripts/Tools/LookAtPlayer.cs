using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public Camera playerCamera;
    public bool lockX = false;
    public bool lockY = false;
    public bool lockZ = false;
    public bool rotate180 = true;

    public void Update()
    {
        transform.LookAt(playerCamera.transform);

        if (rotate180) transform.Rotate(0, 180, 0);
        if (lockX) transform.eulerAngles = new(0, transform.eulerAngles.y, transform.eulerAngles.z);
        if (lockY) transform.eulerAngles = new(transform.eulerAngles.x, 0, transform.eulerAngles.z);
        if (lockZ) transform.eulerAngles = new(transform.eulerAngles.x, transform.eulerAngles.y, 0);
    }
}