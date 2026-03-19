using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class HistoryPanelController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider historySlider;
    [SerializeField] private TMP_Text historyTimeText;
    [SerializeField] private ThermometerUI historyThermometer;

    private LatestDataReading[] historyReadings;

    public void SetHistoryData(LatestDataReading[] readings)
    {
        if (readings == null || readings.Length == 0)
        {
            Debug.LogWarning("No history readings available.");
            return;
        }

        historyReadings = readings;

        if (historySlider != null)
        {
            historySlider.minValue = 0;
            historySlider.maxValue = readings.Length - 1;
            historySlider.wholeNumbers = true;
            historySlider.value = readings.Length - 1;
        }

        UpdateHistoryDisplay(readings.Length - 1);
    }

    public void OnSliderValueChanged(float value)
    {
        if (historyReadings == null || historyReadings.Length == 0) return;

        int index = Mathf.RoundToInt(value);
        UpdateHistoryDisplay(index);
    }

    private void UpdateHistoryDisplay(int index)
    {
        if (historyReadings == null || index < 0 || index >= historyReadings.Length) return;

        LatestDataReading reading = historyReadings[index];

        if (historyThermometer != null)
        {
            historyThermometer.SetTemperature(reading.value);
            historyThermometer.SetTime(reading.measured_at);
        }

       
        if (DateTime.TryParse(reading.measured_at, out DateTime dt))
        {
            dt = dt.ToLocalTime();

            string timeStr = dt.ToString("yyyy-MM-dd HH:mm");
            string timezone = TimeZoneInfo.Local.IsDaylightSavingTime(dt) ? "EEST" : "EET";
            historyTimeText.text = "Time: " + $"{timeStr} ({timezone})";
        }
    }
}
