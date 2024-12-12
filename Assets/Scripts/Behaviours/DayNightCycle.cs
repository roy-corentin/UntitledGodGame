using UnityEngine;

public enum TimeOfDay
{
    Day,
    Night
}

public class DayNightCycle : MonoBehaviour
{
    public float dayLengthinMinutes = 10;
    public float timeOfDay = 0;
    public float timeOFDayOn24 = 0;
    public float timeMultiplier = 1;
    public int daySinceStart = 0;
    public TimeOfDay currentTimeOfDay = TimeOfDay.Day;
    private TimeOfDay lastTimeOfDay = TimeOfDay.Day;
    public Material skyboxMaterial;
    public Light sun;
    public Camera mainCamera;
    public float lerpSpeed = 1;
    public Color32 dayColor = new(255, 255, 255, 255);
    public Color32 nightColor = new(0, 0, 0, 255);
    public static DayNightCycle Instance;

    public void Awake()
    {
        Instance = this;
    }

    public void OnDestroy()
    {
        skyboxMaterial.SetColor("_Tint", dayColor);
    }

    public void Update()
    {
        timeOfDay += Time.deltaTime * timeMultiplier;
        timeOFDayOn24 = timeOfDay / (dayLengthinMinutes * 60);
        currentTimeOfDay = CalculateTimeOfDay();
        UpdateTimeVisual();

        if (lastTimeOfDay != currentTimeOfDay)
        {
            lastTimeOfDay = currentTimeOfDay;
            Debug.Log("It's " + currentTimeOfDay);
        }

        if (timeOfDay > dayLengthinMinutes * 60)
        {
            timeOfDay = 0;
            daySinceStart++;
        }
    }

    public TimeOfDay CalculateTimeOfDay()
    {
        if (timeOFDayOn24 >= 0.5f) return TimeOfDay.Night;
        return TimeOfDay.Day;
    }

    public void UpdateTimeVisual()
    {
        Color32 targetColor = currentTimeOfDay == TimeOfDay.Day ? dayColor : nightColor;
        Color32 currentColor = skyboxMaterial.GetColor("_Tint");
        Color32 newColor = Color32.Lerp(currentColor, targetColor, Time.deltaTime * lerpSpeed);
        skyboxMaterial.SetColor("_Tint", newColor);
    }
}