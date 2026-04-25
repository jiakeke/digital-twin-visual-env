using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class AQIHistoryPanelController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider historySlider;
    [SerializeField] private TMP_Text historyTimeText;
    [SerializeField] private AQIBarUI historyAQIBar;

    [SerializeField] private float playSpeed = 0.2f;
    [SerializeField] private int step = 1;

    private bool isPlaying = false;
    private Coroutine autoPlayCoroutine;

    private LatestDataReading[] historyReadings;

    public void SetHistoryData(LatestDataReading[] readings)
    {
        Debug.Log("AQI SetHistoryData called");

        if (readings == null || readings.Length == 0)
        {
            Debug.LogWarning("No AQI history readings available.");
            return;
        }

        //choose location_id "hsy_4" which same as latest data position
        historyReadings = readings
              .Where(r => r.location_id == "hsy_4")
              .OrderBy(r => DateTime.Parse(r.measured_at))
              .ToArray();

        if (historyAQIBar != null)
        {
            historyAQIBar.ClearBalls();
        }

        if (historySlider != null)
        {
            historySlider.minValue = 0;
            historySlider.maxValue = historyReadings.Length - 1;
            historySlider.wholeNumbers = true;
            historySlider.value = historyReadings.Length - 1;
        }

        UpdateHistoryDisplay(historyReadings.Length - 1);

        for (int i = 0; i < historyReadings.Length; i++)
        {
            Debug.Log($"AQI history[{i}] time = {historyReadings[i].measured_at}, value = {historyReadings[i].value}");
        }
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

        if (historyAQIBar != null)
        {
            historyAQIBar.SetAQI(reading.value);
            historyAQIBar.SetTime(reading.measured_at);
        }

        
    }

    public void ToggleAutoPlay()
    {
        if (historyReadings == null || historyReadings.Length == 0) return;

        if (autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
        }

        if (historyAQIBar != null)
        {
            historyAQIBar.ClearBalls();
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