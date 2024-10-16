using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public Camera playerCamera;

    public void Update()
    {
        transform.LookAt(playerCamera.transform);
    }
}