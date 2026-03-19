using System.Collections;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnvironmentApiClient apiClient;
    [SerializeField] private ThermometerUI thermometer;
    [SerializeField] private HistoryPanelController historyController;

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
    private void OnLatestEnvironmentReceived(LatestDataResponse response)
    {
        if (response == null || response.readings == null || response.readings.Length == 0)
        {
            Debug.LogWarning("No latest environment data received.");
            return;
        }

        foreach (LatestDataReading reading in response.readings)
        {
            if (reading.metric == "temperature")
            {
                Debug.Log("Temperature found: " + reading.value + " " + reading.unit);

                if (thermometer != null)
                {
                    thermometer.SetTemperature(reading.value);
                    thermometer.SetTime(reading.measured_at);
                }

                break;
            }

            //Add another data here
        }
    }

    private void OnHistoryReceived(HistoryDataResponse response)
    {
        if (response == null || response.readings == null || response.readings.Length == 0)
        {
            Debug.LogWarning("No history temperature data received.");
            return;
        }

        if (historyController != null)
        {
            historyController.SetHistoryData(response.readings);
        }
    }
}