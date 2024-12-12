using UnityEngine;
using TMPro;
using System;

public class Slider : MonoBehaviour
{
    public TextMeshProUGUI text;
    private string lastText = default;
    public bool firstUpdate = true;

    public void UpdateText(float value)
    {
        double roundedValue = Math.Round(value, 2);
        string newValue = roundedValue.ToString();
        text.text = newValue;

        if (lastText != newValue) lastText = newValue;

        firstUpdate = false;
    }

    public void UpdateSliderAndText(float value)
    {
        UpdateText(value);
        GetComponent<UnityEngine.UI.Slider>().value = value;
    }
}