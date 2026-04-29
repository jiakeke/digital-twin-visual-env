using System;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class HumidityDropUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform fillRect;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text timeText;

    [Header("Humidity Range")]
    [SerializeField] private float minHumidity = 0f;
    [SerializeField] private float maxHumidity = 100f;

    [Header("Fill Settings")]
    [SerializeField] private float minFillHeight = 0f;
    [SerializeField] private float maxFillHeight = 400f;

    [Header("Debug")]
    [SerializeField] private float currentHumidity = 10f;

    private void OnEnable()
    {
        Apply(currentHumidity);
    }

    private void OnValidate()
    {
        Apply(currentHumidity);
    }

    public void SetHumidity(float humidity)
    {
        currentHumidity = humidity;
        Apply(humidity);
    }

    private void Apply(float humidity)
    {
        if (fillRect == null) return;

        humidity = Mathf.Clamp(humidity, minHumidity, maxHumidity);

        float t = Mathf.InverseLerp(minHumidity, maxHumidity, humidity);
        float targetHeight = Mathf.Lerp(minFillHeight, maxFillHeight, t);

        Vector2 size = fillRect.sizeDelta;
        size.y = targetHeight;
        fillRect.sizeDelta = size;

        if (valueText != null)
        {
            valueText.text = $"Humidity: {Mathf.RoundToInt(humidity)}%";
        }
    }

    public void SetTime(string isoTime)
    {
        if (timeText == null) return;

        if (string.IsNullOrEmpty(isoTime))
        {
            timeText.text = "TIME: N/A";
            return;
        }

        if (DateTime.TryParse(isoTime, out DateTime dt))
        {
            DateTime localTime = dt.ToLocalTime();
            string timezone = TimeZoneInfo.Local.IsDaylightSavingTime(localTime) ? "EEST" : "EET";
            timeText.text = $"TIME: {localTime:yyyy-MM-dd HH:mm} ({timezone})";
        }
        else
        {
            timeText.text = "TIME: " + isoTime;
        }
    }
}