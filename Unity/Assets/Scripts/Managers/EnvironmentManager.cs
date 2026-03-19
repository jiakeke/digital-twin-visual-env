using System.Collections;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnvironmentApiClient apiClient;
    [SerializeField] private ThermometerUI thermometer;

    private void Start()
    {
        StartCoroutine(CheckApiOnStartup());
    }
    private IEnumerator CheckApiOnStartup()
    {
        yield return StartCoroutine(apiClient.CheckApiHealth());

    }
    public void OnTemperatureLatestButtonClicked()
    {
        StartCoroutine(LoadLatestTemperature());
    }

    private IEnumerator LoadLatestTemperature()
    {
        yield return StartCoroutine(apiClient.GetLatestEnvironment(OnLatestEnvironmentReceived));
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
}