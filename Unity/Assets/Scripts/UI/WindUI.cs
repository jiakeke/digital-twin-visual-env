using System;
using TMPro;
using UnityEngine;

public class WindUI : MonoBehaviour
{
    [SerializeField] private TMP_Text updateAtText;
    [SerializeField] private TMP_Text windDirectionText;
    [SerializeField] private TMP_Text windSpeedText;

    public void SetWindData(float speed, float direction, string measuredAt)
    {
        if (updateAtText != null)
            updateAtText.text = TimeFormatter.FormatToLocalTime(measuredAt);

        if (windDirectionText != null)
            windDirectionText.text = "Wind Direction: " + direction + "бу";

        if (windSpeedText != null)
            windSpeedText.text = "Speed: " + speed + " m/s";
    }

    public static class TimeFormatter
    {
        public static string FormatToLocalTime(string isoTime)
        {
            if (DateTime.TryParse(isoTime, out DateTime dt))
            {
                dt = dt.ToLocalTime();

                string timeStr = dt.ToString("HH:mm");
                string timezone = TimeZoneInfo.Local.IsDaylightSavingTime(dt) ? "EEST" : "EET";

                return $"Updated at: {timeStr} ({timezone})";
            }

            return "Updated at: " + isoTime;
        }
    }
}
