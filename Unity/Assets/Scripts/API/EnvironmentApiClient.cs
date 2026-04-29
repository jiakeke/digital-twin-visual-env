using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class EnvironmentApiClient : MonoBehaviour
{
    [Header("API URLs")]
    [SerializeField] private string healthUrl = "http://twin.jias.name:8001/api/v1/health";
    [SerializeField] private string latestUrl = "http://twin.jias.name:8001/api/v1/environment/latest";
    [SerializeField] private string historyUrl = "http://twin.jias.name:8001/api/v1/environment/history";

    //Check if api conncet successfully
    public IEnumerator CheckApiHealth()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(healthUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("API connect fail: " + request.error);
            }
            else
            {
                Debug.Log("API connect successfully.");
                Debug.Log("Health response: " + request.downloadHandler.text);
            }
        }
    }

    //Send the request and Get the latest environment data from api 
    public IEnumerator GetLatestEnvironment(System.Action<LatestDataResponse> onSuccess)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(latestUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Latest environment request failed: " + request.error);
                onSuccess?.Invoke(null);
                yield break;
            }

            string json = request.downloadHandler.text;
            Debug.Log("Latest environment response: " + json);

            LatestDataResponse response = JsonUtility.FromJson<LatestDataResponse>(json);
            onSuccess?.Invoke(response);
        }
    }

    //Obtain dynamic historical temperature
    public IEnumerator GetTemperatureHistory(System.Action<HistoryDataResponse> onSuccess)
    {
       
        string from = System.DateTime.UtcNow.AddDays(-7).ToString("o");
        string to = System.DateTime.UtcNow.ToString("o");

        string url = $"{historyUrl}?source=fmi&metric=temperature" +
                 $"&from={UnityWebRequest.EscapeURL(from)}" +
                 $"&to={UnityWebRequest.EscapeURL(to)}";

        Debug.Log("History request URL: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("History request failed: " + request.error);
                onSuccess?.Invoke(null);
                yield break;
            }

            string json = request.downloadHandler.text;

            Debug.Log("History response: " + json);

            HistoryDataResponse response = JsonUtility.FromJson<HistoryDataResponse>(json);
            onSuccess?.Invoke(response);
        }
    }
    //Obtain dynamic historical wind speed
    public IEnumerator GetWindSpeedHistory(System.Action<HistoryDataResponse> onSuccess)
    {
        string from = System.DateTime.UtcNow.AddDays(-7).ToString("o");
        string to = System.DateTime.UtcNow.ToString("o");

        string url = $"{historyUrl}?source=fmi&metric=wind_speed" +
                     $"&from={UnityWebRequest.EscapeURL(from)}" +
                     $"&to={UnityWebRequest.EscapeURL(to)}";

        Debug.Log("Wind speed history request URL: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Wind speed history request failed: " + request.error);
                onSuccess?.Invoke(null);
                yield break;
            }

            string json = request.downloadHandler.text;
            Debug.Log("Wind speed history response: " + json);

            HistoryDataResponse response = JsonUtility.FromJson<HistoryDataResponse>(json);
            onSuccess?.Invoke(response);
        }
    }
    //Obtain dynamic historical wind direction
    public IEnumerator GetWindDirectionHistory(System.Action<HistoryDataResponse> onSuccess)
    {
        string from = System.DateTime.UtcNow.AddDays(-7).ToString("o");
        string to = System.DateTime.UtcNow.ToString("o");

        string url = $"{historyUrl}?source=fmi&metric=wind_direction" +
                     $"&from={UnityWebRequest.EscapeURL(from)}" +
                     $"&to={UnityWebRequest.EscapeURL(to)}";

        Debug.Log("Wind direction history request URL: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Wind direction history request failed: " + request.error);
                onSuccess?.Invoke(null);
                yield break;
            }

            string json = request.downloadHandler.text;
            Debug.Log("Wind direction history response: " + json);

            HistoryDataResponse response = JsonUtility.FromJson<HistoryDataResponse>(json);
            onSuccess?.Invoke(response);
        }
    }

    //obtain historical AQI data
    public IEnumerator GetAQIHistory(System.Action<HistoryDataResponse> onSuccess)
    {
        string from = System.DateTime.UtcNow.AddDays(-7).ToString("o");
        string to = System.DateTime.UtcNow.AddHours(3).ToString("o");  //extending history query range

        //from "hsy" 
        string url = $"{historyUrl}?source=hsy&metric=aqi" +
                     $"&from={UnityWebRequest.EscapeURL(from)}" +
                     $"&to={UnityWebRequest.EscapeURL(to)}";

        Debug.Log("AQI history request URL: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("AQI history request failed: " + request.error);
                onSuccess?.Invoke(null);
                yield break;
            }

            string json = request.downloadHandler.text;
            Debug.Log("AQI history response: " + json);

            HistoryDataResponse response = JsonUtility.FromJson<HistoryDataResponse>(json);
            onSuccess?.Invoke(response);
        }
    }

    //obtain Himidity historical data
    public IEnumerator GetHumidityHistory(System.Action<HistoryDataResponse> onSuccess)
    {
        string from = System.DateTime.UtcNow.AddDays(-7).ToString("o");
        string to = System.DateTime.UtcNow.ToString("o");

        string url = $"{historyUrl}?source=fmi&metric=humidity" +
                     $"&from={UnityWebRequest.EscapeURL(from)}" +
                     $"&to={UnityWebRequest.EscapeURL(to)}";

        Debug.Log("Humidity history request URL: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Humidity history request failed: " + request.error);
                onSuccess?.Invoke(null);
                yield break;
            }

            string json = request.downloadHandler.text;
            Debug.Log("Humidity history response: " + json);

            HistoryDataResponse response = JsonUtility.FromJson<HistoryDataResponse>(json);
            onSuccess?.Invoke(response);
        }
    }
}