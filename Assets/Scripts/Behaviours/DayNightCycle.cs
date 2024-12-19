using UnityEngine;

public enum TimeOfDay
{
    Day,
    Night
}

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayLengthinMinutes = 10;
    public float timeMultiplier = 1;

    [Header("Infos (Read Only)")]
    public float timeOfDay = 0;
    public float timeOFDayOn24 = 0;
    public int daySinceStart = 0;
    public TimeOfDay currentTimeOfDay = TimeOfDay.Day;
    private TimeOfDay lastTimeOfDay = TimeOfDay.Day;

    [Header("Skybox")]
    public Material skyboxMaterial;
    public Color32 dayColor = new(255, 255, 255, 255);
    public Color32 nightColor = new(0, 0, 0, 255);
    private Color32 startSkyboxColor;
    private Color32 targetSkyboxColor;

    [Header("Light")]
    public Light sun;
    public Color32 lightDayColor = new(255, 209, 81, 255);
    public Color32 lightNightColor = new(0, 0, 0, 255);
    private Color startLightColor;
    private Color targetLightColor;

    [Header("Transition")]
    private float transitionTime = 0f;
    public float transitionDuration = 5f;

    public static DayNightCycle Instance;

    private void Awake()
    {
        Instance = this;
        InitializeVisuals();
    }

    private void OnDestroy()
    {
        skyboxMaterial.SetColor("_Tint", dayColor);
        sun.color = lightDayColor;
    }

    private void Update()
    {
        UpdateTime();
        if (lastTimeOfDay != currentTimeOfDay) HandleTimeOfDayChange();
        SmoothTransition();
    }

    private void InitializeVisuals()
    {
        currentTimeOfDay = CalculateTimeOfDay();
        lastTimeOfDay = currentTimeOfDay;

        Color32 initialSkyboxColor = currentTimeOfDay == TimeOfDay.Day ? dayColor : nightColor;
        Color initialLightColor = currentTimeOfDay == TimeOfDay.Day ? lightDayColor : lightNightColor;

        skyboxMaterial.SetColor("_Tint", initialSkyboxColor);
        sun.color = initialLightColor;

        startSkyboxColor = initialSkyboxColor;
        targetSkyboxColor = initialSkyboxColor;
        startLightColor = initialLightColor;
        targetLightColor = initialLightColor;
    }

    private void UpdateTime()
    {
        timeOfDay += Time.deltaTime * timeMultiplier;
        timeOFDayOn24 = timeOfDay / (dayLengthinMinutes * 60);
        currentTimeOfDay = CalculateTimeOfDay();

        if (timeOfDay > dayLengthinMinutes * 60)
        {
            timeOfDay = 0;
            daySinceStart++;
        }
    }

    private TimeOfDay CalculateTimeOfDay()
    {
        return timeOFDayOn24 >= 0.5f ? TimeOfDay.Night : TimeOfDay.Day;
    }

    private void HandleTimeOfDayChange()
    {
        lastTimeOfDay = currentTimeOfDay;
        transitionTime = 0f;

        startSkyboxColor = skyboxMaterial.GetColor("_Tint");
        targetSkyboxColor = currentTimeOfDay == TimeOfDay.Day ? dayColor : nightColor;
        startLightColor = sun.color;
        targetLightColor = currentTimeOfDay == TimeOfDay.Day ? lightDayColor : lightNightColor;
    }

    private void SmoothTransition()
    {
        float adjustedTransitionDuration = transitionDuration / timeMultiplier;

        if (transitionTime < adjustedTransitionDuration)
        {
            transitionTime += Time.deltaTime;
            float t = Mathf.Clamp01(transitionTime / adjustedTransitionDuration);

            Color32 newSkyboxColor = Color32.Lerp(startSkyboxColor, targetSkyboxColor, t);
            skyboxMaterial.SetColor("_Tint", newSkyboxColor);

            Color newLightColor = Color.Lerp(startLightColor, targetLightColor, t);
            sun.color = newLightColor;
        }
    }
}
