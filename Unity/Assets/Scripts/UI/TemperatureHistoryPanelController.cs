using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class TemperatureHistoryPanelController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider historySlider;
    [SerializeField] private TMP_Text historyTimeText;
    [SerializeField] private ThermometerUI historyThermometer;
    private bool isPlaying = false;
    [SerializeField] private float playSpeed = 0.2f; // second
    [SerializeField] private int step = 10;

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

    //Auto play silder
    private Coroutine autoPlayCoroutine;
    public void ToggleAutoPlay()
    {
        
        if (historyReadings == null || historyReadings.Length == 0) return;

        // stop old autoplay
        if (autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
        }

        historySlider.value = 0;
        isPlaying = true;
        autoPlayCoroutine = StartCoroutine(AutoPlayHistory());
    }
    private IEnumerator AutoPlayHistory()
    {
        while (isPlaying)
        {
            if (historyReadings == null || historyReadings.Length == 0)
            {
                autoPlayCoroutine = null;
                yield break;
            }       

            int currentIndex = Mathf.RoundToInt(historySlider.value);
            currentIndex += step;

            if (currentIndex >= historyReadings.Length)
            {
                currentIndex = historyReadings.Length - 1;
                historySlider.value = currentIndex;
                isPlaying = false;
                autoPlayCoroutine = null;
                yield break;

            }

            historySlider.value = currentIndex;

            yield return new WaitForSeconds(playSpeed);
        }

        autoPlayCoroutine = null;
    }

}
