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
        //string from = "2026-03-17T00:00:00Z";
        //string to = "2026-03-20T23:59:59Z";
        string from = System.DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-ddTHH:mm:ssZ");
        string to = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

        string url = $"{historyUrl}?source=fmi&metric=temperature&from={from}&to={to}";

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
            HistoryDataResponse response = JsonUtility.FromJson<HistoryDataResponse>(json);
            onSuccess?.Invoke(response);
        }
    }
}