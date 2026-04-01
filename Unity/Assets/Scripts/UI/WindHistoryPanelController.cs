using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class WindHistoryPanelController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider historySlider;
    [SerializeField] private TMP_Text historyTimeText;
    [SerializeField] private TMP_Text historyWindDirectionText;
    [SerializeField] private TMP_Text historyWindSpeedText;
    [SerializeField] private CloudManager cloudManager;

    private bool isPlaying = false;
    private Coroutine autoPlayCoroutine;

    [SerializeField] private float playSpeed = 0.1f;
    [SerializeField] private int step = 5;

    private WindHistoryPoint[] historyPoints;

    public void SetHistoryData(WindHistoryPoint[] points)
    {
        if (points == null || points.Length == 0)
        {
            Debug.LogWarning("No wind history points available.");
            return;
        }

        historyPoints = points;

        if (cloudManager != null)
        {
            cloudManager.ShowClouds();
            cloudManager.ResetCloudPositions();
        }

        if (historySlider != null)
        {
            historySlider.minValue = 0;
            historySlider.maxValue = points.Length - 1;
            historySlider.wholeNumbers = true;
            historySlider.value = points.Length - 1;
        }

        UpdateHistoryDisplay(points.Length - 1);
    }

    public void OnSliderValueChanged(float value)
    {
        if (historyPoints == null || historyPoints.Length == 0) return;

        int index = Mathf.RoundToInt(value);
        UpdateHistoryDisplay(index);
    }

    private void UpdateHistoryDisplay(int index)
    {
        if (historyPoints == null || index < 0 || index >= historyPoints.Length) return;

        WindHistoryPoint point = historyPoints[index];

        if (historyWindSpeedText != null)
            historyWindSpeedText.text = $"Wind Speed: {point.speed} m/s";

        if (historyWindDirectionText != null)
            historyWindDirectionText.text = $"Wind Direction: {point.direction}бу";

        
        if (cloudManager != null)
        {
            //cloudManager.ResetCloudPositions();
            cloudManager.SetWind(point.speed, point.direction, true);
        }

        if (historyTimeText != null)
        {
            if (DateTime.TryParse(point.measuredAt, out DateTime dt))
            {
                dt = dt.ToLocalTime();

                string timeStr = dt.ToString("yyyy-MM-dd HH:mm");
                string timezone = TimeZoneInfo.Local.IsDaylightSavingTime(dt) ? "EEST" : "EET";
                historyTimeText.text = $"Time: {timeStr} ({timezone})";
            }
            else
            {
                historyTimeText.text = "Time: " + point.measuredAt;
            }
        }
    }

    public void ToggleAutoPlay()
    {
        if (historyPoints == null || historyPoints.Length == 0) return;

        if (autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
        }

        if (cloudManager != null)
        {
            cloudManager.ShowClouds();
            cloudManager.ResetCloudPositions();
        }

        historySlider.value = 0;
        isPlaying = true;
        autoPlayCoroutine = StartCoroutine(AutoPlayHistory());
    }

    private IEnumerator AutoPlayHistory()
    {
        while (isPlaying)
        {
            if (historyPoints == null || historyPoints.Length == 0)
            {
                autoPlayCoroutine = null;
                yield break;
            }

            int currentIndex = Mathf.RoundToInt(historySlider.value);
            currentIndex += step;

            if (currentIndex >= historyPoints.Length)
            {
                currentIndex = historyPoints.Length - 1;
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