using UnityEngine;

public class Toolbox : MonoBehaviour
{
    public Camera playerCamera;
    [SerializeField] private float distanceForward = 0.35f;
    [SerializeField] private float distanceUp = -0.35f;

    public void Update()
    {
        transform.position = playerCamera.transform.position
            + playerCamera.transform.forward * distanceForward
            + playerCamera.transform.up * distanceUp;

        transform.LookAt(playerCamera.transform);
        transform.Rotate(0, 180, 0);
    }
}