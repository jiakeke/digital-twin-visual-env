using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HumidityHistoryPanelController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider historySlider;
    [SerializeField] private HumidityBarUI historyHumidityBar;

    [SerializeField] private float playSpeed = 0.2f;
    [SerializeField] private int step = 5;

    private bool isPlaying = false;
    private Coroutine autoPlayCoroutine;

    private LatestDataReading[] historyReadings;

    public void SetHistoryData(LatestDataReading[] readings)
    {
        Debug.Log("Humidity SetHistoryData called");

        if (readings == null || readings.Length == 0)
        {
            Debug.LogWarning("No humidity history readings available.");
            return;
        }

        historyReadings = readings
            .Where(r => r.metric == "humidity")
            .OrderBy(r => DateTime.Parse(r.measured_at))
            .ToArray();

        if (historyReadings.Length == 0)
        {
            Debug.LogWarning("No humidity history readings after filtering.");
            return;
        }

        if (historyHumidityBar != null)
        {
            historyHumidityBar.ClearBalls();
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
            Debug.Log($"Humidity history[{i}] time = {historyReadings[i].measured_at}, value = {historyReadings[i].value}");
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

        if (historyHumidityBar != null)
        {
            historyHumidityBar.SetHumidity(reading.value);
            historyHumidityBar.SetTime(reading.measured_at);
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

        if (historyHumidityBar != null)
        {
            historyHumidityBar.ClearBalls();
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