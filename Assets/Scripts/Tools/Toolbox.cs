using UnityEngine;

public class Toolbox : MonoBehaviour
{
    public Camera playerCamera;

    public void Update()
    {
        transform.position = playerCamera.transform.position
            + playerCamera.transform.forward * 0.35f
            + playerCamera.transform.up * -0.35f;

        transform.LookAt(playerCamera.transform);
        transform.Rotate(0, 180, 0);
    }
}