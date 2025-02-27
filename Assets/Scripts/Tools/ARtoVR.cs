using UnityEngine;
using DG.Tweening;

public enum GameMode
{
    AR,
    VR
}

public class ARtoVR : MonoBehaviour
{
    public GameMode currentMode = GameMode.AR;
    public static ARtoVR Instance;
    public GameObject mapGO;
    public Vector3 ARMapSize;
    public Vector3 VRMapSize;
    public Vector3 heightCompensation;
    public float transitionDuration;
    public OVRPassthroughLayer passthroughLayer;
    public Camera mainCamera;
    public GameObject toolbox;
    public AudioSource mapAudioSource;
    public AudioClip transitionSound;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GoToAR(true);
    }

    public void Update()
    {
        if (!MapGenerator.Map.Instance.areDotsGenerated) return;

        if (Input.GetKeyDown(KeyCode.B) || OVRInput.GetDown(OVRInput.Button.Four))
        {
            ToggleMode();
        }
    }

    public void ToggleMode()
    {
        SetMode(currentMode == GameMode.AR ? GameMode.VR : GameMode.AR);
    }

    public GameMode GetCurrentMode()
    {
        return currentMode;
    }

    public void SetMode(GameMode mode)
    {
        if (currentMode == mode) return;

        mapAudioSource.clip = transitionSound;
        mapAudioSource.Play();

        if (mode == GameMode.AR) GoToAR();
        else GoToVR();
    }

    private void GoToAR(bool instant = false)
    {
        SetPasstrough(true);

        AnimalSpawner.Instance.DisableAll();
        NavMeshHandler.Instance.ClearNavmesh();

        mapGO.transform
            .DOMove(Vector3.zero, instant ? 0 : transitionDuration)
            .SetEase(Ease.InOutCubic);

        mapGO.transform.localScale = VRMapSize;
        mapGO.transform
            .DOScale(ARMapSize, instant ? 0 : transitionDuration)
            .SetEase(Ease.InOutCubic)
            .OnComplete(() =>
            {
                Debug.Log("AR Mode");
                AnimalSpawner.Instance.LockAnimations(true);
                AnimalSpawner.Instance.ResetReachablesInfos();
                currentMode = GameMode.AR;
            });

        toolbox.gameObject.SetActive(true);
        toolbox.transform
            .DOScale(Vector3.one, instant ? 0 : transitionDuration)
            .SetEase(Ease.InOutCubic);

        DayNightCycle.Instance.StopTime();
    }

    private void GoToVR(bool instant = false)
    {
        SetPasstrough(false);

        mapGO.transform
            .DOMove(heightCompensation, instant ? 0 : transitionDuration)
            .SetEase(Ease.InOutCubic);

        mapGO.transform.localScale = ARMapSize;
        mapGO.transform
            .DOScale(VRMapSize, instant ? 0 : transitionDuration)
            .SetEase(Ease.InOutCubic)
            .OnComplete(() =>
            {
                Debug.Log("VR Mode");
                AnimalSpawner.Instance.LockAnimations(false);
                AnimalSpawner.Instance.ResetReachablesInfos();
                LocationManager.Instance.UpdateGroundDots();
                NavMeshHandler.Instance.Rebake();
                AnimalSpawner.Instance.EnableAll();
                currentMode = GameMode.VR;
            });

        toolbox.transform
            .DOScale(Vector3.one, instant ? 0 : transitionDuration)
            .SetEase(Ease.InOutCubic)
            .OnComplete(() =>
            {
                toolbox.gameObject.SetActive(false);
            });

        DayNightCycle.Instance.ResumeTime();
    }

    private void SetPasstrough(bool active)
    {
        passthroughLayer.hidden = !active;

        switch (passthroughLayer.hidden)
        {
            case true:
                mainCamera.clearFlags = CameraClearFlags.Skybox;
                break;
            case false:
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = new Color32(0, 0, 0, 0);
                break;
        }
    }
}