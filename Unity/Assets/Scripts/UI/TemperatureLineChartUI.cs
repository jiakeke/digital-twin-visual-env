using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TemperatureLineChartUI : MonoBehaviour
{
    [Header("Containers")]
    [SerializeField] private RectTransform lineContainer;
    [SerializeField] private RectTransform pointContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private GameObject pointPrefab;

    [Header("Text")]
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text timeText;

    [SerializeField] private TMP_Text maxLabel;
    [SerializeField] private TMP_Text midLabel;
    [SerializeField] private TMP_Text minLabel;

    private LatestDataReading[] readings;

    public void SetData(LatestDataReading[] data)
    {
        readings = data;

        ClearChart();
        DrawChart();
    }

    private void ClearChart()
    {
        if (lineContainer != null)
        {
            for (int i = lineContainer.childCount - 1; i >= 0; i--)
                Destroy(lineContainer.GetChild(i).gameObject);
        }

        if (pointContainer != null)
        {
            for (int i = pointContainer.childCount - 1; i >= 0; i--)
                Destroy(pointContainer.GetChild(i).gameObject);
        }
    }

    private void DrawChart()
    {

        if (readings == null || readings.Length == 0) return;
        if (lineContainer == null || pointContainer == null) return;
        if (linePrefab == null || pointPrefab == null) return;

        float minTemp = readings[0].value;
        float maxTemp = readings[0].value;

        foreach (var r in readings)
        {
            minTemp = Mathf.Min(minTemp, r.value);
            maxTemp = Mathf.Max(maxTemp, r.value);
        }

        float range = Mathf.Max(maxTemp - minTemp, 0.1f);
        float width = lineContainer.rect.width;
        float height = lineContainer.rect.height;

        Vector2 previousPoint = Vector2.zero;

        for (int i = 0; i < readings.Length; i++)
        {
            float normalizedX = readings.Length == 1 ? 0.5f : i / (float)(readings.Length - 1);
            float normalizedY = (readings[i].value - minTemp) / range;

            float x = Mathf.Lerp(-width * 0.5f, width * 0.5f, normalizedX);
            float y = Mathf.Lerp(-height * 0.5f, height * 0.5f, normalizedY);

            Vector2 currentPoint = new Vector2(x, y);

            GameObject point = Instantiate(pointPrefab, pointContainer);
            point.GetComponent<RectTransform>().anchoredPosition = currentPoint;

            if (i > 0)
            {
                CreateLine(previousPoint, currentPoint);
            }

            previousPoint = currentPoint;
        }
        //update chart lable
        if (maxLabel != null)
            maxLabel.text = $"{maxTemp:F1}°„C";

        if (minLabel != null)
            minLabel.text = $"{minTemp:F1}°„C";

        if (midLabel != null)
        {
            float mid = (maxTemp + minTemp) / 2f;
            midLabel.text = $"{mid:F1}°„C";
        }
    }

    private void CreateLine(Vector2 start, Vector2 end)
    {
        GameObject line = Instantiate(linePrefab, lineContainer);
        RectTransform rect = line.GetComponent<RectTransform>();

        Vector2 direction = (end - start).normalized;
        float distance = Vector2.Distance(start, end);

        rect.sizeDelta = new Vector2(distance, 4f);
        rect.anchoredPosition = start + direction * distance * 0.5f;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rect.rotation = Quaternion.Euler(0, 0, angle);
    }

    //presents timeText and other inforn in "HistoryContent"
    public void SetTemperatureHistoryData(int index)
    {
        if (readings == null || index < 0 || index >= readings.Length) return;

        var reading = readings[index];

        if (valueText != null)
        {
            valueText.text = $"Temperature: {reading.value:F1} °„C";
        }

        if (timeText != null && DateTime.TryParse(reading.measured_at, out DateTime dt))
        {
            dt = dt.ToLocalTime();
            string timeStr = dt.ToString("yyyy-MM-dd HH:mm");
            string timezone = TimeZoneInfo.Local.IsDaylightSavingTime(dt) ? "EEST" : "EET";
            timeText.text = $"Time: {timeStr} ({timezone})";
        }
    }
}