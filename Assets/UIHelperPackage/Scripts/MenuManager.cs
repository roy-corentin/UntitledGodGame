using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject menu;
    public LaserPointerData laserPointer;
    public static MenuManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        menu.SetActive(false);
        laserPointer.Show(false);
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Start) || Input.GetKeyDown(KeyCode.J))
        {
            menu.SetActive(!menu.activeSelf);
            laserPointer.Show(menu.activeSelf);

            if (menu.activeSelf)
            {
                StatsManager.Instance.UpdateStats();
                MoveMenuInFrontOfCamera();
                laserPointer.SetColor(LaserPointerColor.Default);
            }
        }

        if (menu.activeSelf && !IsInView())
        {
            menu.SetActive(false);
            laserPointer.Show(false);
        }
    }

    private void MoveMenuInFrontOfCamera()
    {
        menu.transform.SetPositionAndRotation(
            Camera.main.transform.position + Camera.main.transform.forward * 0.9f,
            Quaternion.LookRotation(Camera.main.transform.forward));
    }

    private bool IsInView()
    {
        Vector3 viewPos = Camera.main.WorldToViewportPoint(menu.transform.position);
        return !(viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1);
    }

    public void Hide()
    {
        menu.SetActive(false);
        laserPointer.Show(false);
    }
}
