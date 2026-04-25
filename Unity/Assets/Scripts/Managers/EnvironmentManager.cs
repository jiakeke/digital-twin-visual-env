using System.Collections;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private EnvironmentApiClient apiClient;
    [Header("Temperature")]
    [SerializeField] private ThermometerUI thermometer;
    [SerializeField] private TemperatureHistoryPanelController tempHistoryController;
    [Header("Wind")]
    [SerializeField] private CloudManager cloudManager;
    [SerializeField] private WindUI windUI;
    [SerializeField] private WindHistoryPanelController windHistoryController;
    private HistoryDataResponse pendingWindSpeedHistory;
    private HistoryDataResponse pendingWindDirectionHistory;
    [Header("AQI")]
    [SerializeField] private AQIBarUI aqiLatestBar;
    [SerializeField] private AQIHistoryPanelController aqiHistoryController;

    //press "play" run health api
    private void Start()
    {
        StartCoroutine(CheckApiOnStartup());
    }
    private IEnumerator CheckApiOnStartup()
    {
        yield return StartCoroutine(apiClient.CheckApiHealth());

    }
    //press "Latest" run latest api
    public void OnTemperatureLatestButtonClicked()
    {
        StartCoroutine(LoadLatestTemperature());
    }

    private IEnumerator LoadLatestTemperature()
    {
        yield return StartCoroutine(apiClient.GetLatestEnvironment(OnLatestEnvironmentReceived));
    }

    //run "history" run history api
    public void OnTemperatureHistoryButtonClicked()
    {
        StartCoroutine(LoadTemperatureHistory());
    }

    private IEnumerator LoadTemperatureHistory()
    {
        yield return StartCoroutine(apiClient.GetTemperatureHistory(OnHistoryReceived));
    }

    //run AQI latest api
    public void OnAQILatestButtonClicked()
    {
        StartCoroutine(LoadLatestAQI());
    }

    private IEnumerator LoadLatestAQI()
    {
        yield return StartCoroutine(apiClient.GetLatestEnvironment(OnLatestEnvironmentReceived));
    }

    // Wind latest
    public void OnWindLatestButtonClicked()
    {
        if (cloudManager != null)
            cloudManager.ShowClouds();

        StartCoroutine(LoadLatestEnvironment());
    }

    private IEnumerator LoadLatestEnvironment()
    {
        yield return StartCoroutine(apiClient.GetLatestEnvironment(OnLatestEnvironmentReceived));
    }

    // Wind history 
    public void OnWindHistoryButtonClicked()
    {
        if (cloudManager != null)
            cloudManager.ShowClouds();

        StartCoroutine(LoadWindHistory());
    }
    private IEnumerator LoadWindHistory()
    {
        pendingWindSpeedHistory = null;
        pendingWindDirectionHistory = null;

        yield return StartCoroutine(apiClient.GetWindSpeedHistory(OnWindSpeedHistoryReceived));
        yield return StartCoroutine(apiClient.GetWindDirectionHistory(OnWindDirectionHistoryReceived));

        TryBuildWindHistoryPoints();
    }
    private void OnWindSpeedHistoryReceived(HistoryDataResponse response)
    {
        pendingWindSpeedHistory = response;
    }

    private void OnWindDirectionHistoryReceived(HistoryDataResponse response)
    {
        pendingWindDirectionHistory = response;
    }

    private void TryBuildWindHistoryPoints()
    {
        if (pendingWindSpeedHistory == null || pendingWindDirectionHistory == null)
        {
            Debug.LogWarning("Wind history response is missing.");
            return;
        }

        if (pendingWindSpeedHistory.readings == null || pendingWindDirectionHistory.readings == null)
        {
            Debug.LogWarning("Wind history readings are missing.");
            return;
        }

        int count = Mathf.Min(
            pendingWindSpeedHistory.readings.Length,
            pendingWindDirectionHistory.readings.Length
        );

        if (count == 0)
        {
            Debug.LogWarning("No wind history data available.");
            return;
        }

        WindHistoryPoint[] points = new WindHistoryPoint[count];

        for (int i = 0; i < count; i++)
        {
            points[i] = new WindHistoryPoint
            {
                measuredAt = pendingWindSpeedHistory.readings[i].measured_at,
                speed = pendingWindSpeedHistory.readings[i].value,
                direction = pendingWindDirectionHistory.readings[i].value
            };
        }

        if (windHistoryController != null)
        {
            windHistoryController.SetHistoryData(points);
        }

        Debug.Log($"Wind history points built: {count}");
    }

    //run AQI history api
    public void OnAQIHistoryButtonClicked()
    {
        StartCoroutine(LoadAQIHistory());
    }

    private IEnumerator LoadAQIHistory()
    {
        yield return StartCoroutine(apiClient.GetAQIHistory(OnAQIHistoryReceived));
    }

    //received latest data
    private void OnLatestEnvironmentReceived(LatestDataResponse response)
    {
        if (response == null || response.readings == null || response.readings.Length == 0)
        {
            Debug.LogWarning("No latest environment data received.");
            return;
        }

        float? latestTemperature = null;
        string latestTemperatureTime = null;

        float? latestWindSpeed = null;
        float? latestWindDirection = null;
        string latestWindTime = null;

        float? latestAQI = null;
        string latestAQITime = null;

        foreach (LatestDataReading reading in response.readings)
        {
            Debug.Log($"Metric: {reading.metric}, Value: {reading.value}, Unit: {reading.unit}");

            if (reading.metric == "temperature")
            {
                latestTemperature = reading.value;
                latestTemperatureTime = reading.measured_at;
            }
            else if (reading.metric == "wind_speed")
            {
                latestWindSpeed = reading.value;
                latestWindTime = reading.measured_at;
            }
            else if (reading.metric == "wind_direction")
            {
                latestWindDirection = reading.value;
                latestWindTime = reading.measured_at;
            }
            else if (reading.metric == "aqi" && reading.location_id == "hsy_4")
            {
                latestAQI = reading.value;
                latestAQITime = reading.measured_at;
                Debug.Log($"AQI latest location = {reading.location_id}, time = {latestAQITime}, value = {latestAQI}");
            }
        }

        // update temperature UI
        if (latestTemperature.HasValue)
        {
            Debug.Log("Temperature found: " + latestTemperature.Value);

            if (thermometer != null)
            {
                thermometer.SetTemperature(latestTemperature.Value);
                thermometer.SetTime(latestTemperatureTime);
            }
        }
        else
        {
            Debug.LogWarning("No latest temperature found.");
        }

        // update wind UI
        if (latestWindSpeed.HasValue && latestWindDirection.HasValue)
        {
            Debug.Log($"Wind found: speed={latestWindSpeed.Value}, direction={latestWindDirection.Value}");

            if (cloudManager != null)
            {
                cloudManager.SetWind(latestWindSpeed.Value, latestWindDirection.Value, true);
            }

            if (windUI != null)
            {
                string timeToShow = latestWindTime;

                if (string.IsNullOrEmpty(timeToShow))
                    timeToShow = "N/A";

                windUI.SetWindData(latestWindSpeed.Value, latestWindDirection.Value, timeToShow);
            }
        }
        else
        {
            Debug.LogWarning("Wind speed or wind direction not found.");
        }

        // update AQI UI
        if (latestAQI.HasValue)
        {
            Debug.Log("AQI found: " + latestAQI.Value);

            if (aqiLatestBar != null)
            {
                aqiLatestBar.SetAQI(latestAQI.Value);
                aqiLatestBar.SetTime(latestAQITime);
            }
        }
        else
        {
            Debug.LogWarning("No latest AQI found.");
        }
    }

    // received historical ddata
    private void OnHistoryReceived(HistoryDataResponse response)
    {
        if (response == null || response.readings == null || response.readings.Length == 0)
        {
            Debug.LogWarning("No history temperature data received.");
            return;
        }

        if (tempHistoryController != null)
        {
            tempHistoryController.SetHistoryData(response.readings);
        }
    }

    //received AQI hitorical data 
    private void OnAQIHistoryReceived(HistoryDataResponse response)
    {
        Debug.Log("OnAQIHistoryReceived called");

        if (response == null || response.readings == null || response.readings.Length == 0)
        {
            Debug.LogWarning("No AQI history data received.");
            return;
        }

        if (aqiHistoryController != null)
        {
            aqiHistoryController.SetHistoryData(response.readings);
        }
    }
}