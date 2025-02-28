using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject menu;
    public LaserPointerData laserPointer;
    public static MenuManager Instance;
    public Transform handTransform;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        menu.SetActive(false);
        laserPointer.Show(true);
    }

    public void ToggleMenu()
    {
        menu.SetActive(!menu.activeSelf);

        if (menu.activeSelf)
        {
            StatsManager.Instance.UpdateStats();
            MoveMenuInFrontOfCamera();
        }
    }

    void Update()
    {
        if (menu.activeSelf && !IsInView())
        {
            menu.SetActive(false);
        }
    }

    private void MoveMenuInFrontOfCamera()
    {
        menu.transform.SetPositionAndRotation(
            handTransform.position + handTransform.forward * 0.3f,
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
    }
}
