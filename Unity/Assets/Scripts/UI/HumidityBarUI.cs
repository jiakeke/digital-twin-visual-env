using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



[ExecuteAlways]
public class HumidityBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform barFrameRect;
    [SerializeField] private RectTransform pointerRect;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text timeText;

    [Header("Ball (History Visualization)")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform ballSpawnRoot;

    [Header("Ball Settings")]
    public float spawnOffsetY = 35f;  //ball's position compare with pointer's
    public float groundOffsetY = -200f;
    public float firstBounceHeight = 60f;
    public float secondBounceHeight = 45f;

    [Header("Humidity Range")]
    public float minHumidity = 0f;
    public float maxHumidity = 100f;
    public float currentHumidity = 50f;

    private void Start()
    {
        Debug.Log("HumidityBarUI running on: " + gameObject.name);
    }

    private void OnEnable()
    {
        Apply(currentHumidity);
    }

    private void OnValidate()
    {
        Apply(currentHumidity);
    }

 
    public void SetHumidity(float humidity)
    {
        currentHumidity = humidity;
        Apply(humidity);

        if (Application.isPlaying)
        {
            SpawnHumidityBall(humidity);
        }
    }

    //pointer
    private void Apply(float humidity)
    {
        if (barFrameRect == null || pointerRect == null) return;

        humidity = Mathf.Clamp(humidity, minHumidity, maxHumidity);

        float t = Mathf.InverseLerp(minHumidity, maxHumidity, humidity);
        float w = barFrameRect.rect.width;

        float localX = Mathf.Lerp(-w * 0.5f, w * 0.5f, t);

        Vector2 p = pointerRect.anchoredPosition;
        p.x = barFrameRect.anchoredPosition.x + localX;
        pointerRect.anchoredPosition = p;

        if (valueText != null)
        {
            valueText.text = $"Humidity: {Mathf.RoundToInt(humidity)}%";
        }
    }


    //ball drop off
    void SpawnHumidityBall(float humidity)
    {
        Debug.Log("SpawnAQIBall called");
        if (ballPrefab == null || ballSpawnRoot == null || pointerRect == null)
            return;

        GameObject ball = Instantiate(ballPrefab, ballSpawnRoot);
        RectTransform ballRect = ball.GetComponent<RectTransform>();
        Image ballImage = ball.GetComponent<Image>();

        if (ballRect == null)
        {
            Debug.LogWarning("Ball prefab needs a RectTransform.");
            Destroy(ball);
            return;
        }

        if (ballImage != null)
        {
            ballImage.color = GetHumidityColor(humidity);
        }

        Vector2 pointerPos = pointerRect.anchoredPosition;

        Vector2 startPos = new Vector2(pointerPos.x, pointerPos.y + spawnOffsetY);
        Vector2 groundPos = new Vector2(pointerPos.x, pointerPos.y + groundOffsetY);

        ballRect.anchoredPosition = startPos;
        ballRect.localScale = Vector3.one;

        StartCoroutine(PlayBallBounce(ballRect, startPos, groundPos));
    }

    IEnumerator PlayBallBounce(RectTransform ballRect, Vector2 startPos, Vector2 groundPos)
    {
        // First stage: fall down
        float fallDuration = 0.7f;
        float t = 0f;

        while (t < fallDuration)
        {
            if (ballRect == null)
                yield break;

            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / fallDuration);

            // ease-in falling
            float y = Mathf.Lerp(startPos.y, groundPos.y, p * p);
            ballRect.anchoredPosition = new Vector2(startPos.x, y);

            yield return null;
        }

        if (ballRect == null)
            yield break;

        ballRect.anchoredPosition = groundPos;

        // Second stage: first bounce
        yield return StartCoroutine(Bounce(ballRect, groundPos, firstBounceHeight, 0.45f));

        // Third stage: second bounce
        yield return StartCoroutine(Bounce(ballRect, groundPos, secondBounceHeight, 0.35f));

        // End: stay there, do not destroy
        if (ballRect != null)
        {
            ballRect.anchoredPosition = groundPos;
        }
    }

    IEnumerator Bounce(RectTransform ballRect, Vector2 groundPos, float height, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            if (ballRect == null)
                yield break;

            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);

            // simple parabola
            float yOffset = 4f * height * p * (1f - p);
            ballRect.anchoredPosition = new Vector2(groundPos.x, groundPos.y + yOffset);

            yield return null;
        }

        if (ballRect != null)
        {
            ballRect.anchoredPosition = groundPos;
        }
    }


    private Color GetHumidityColor(float humidity)
    {
        if (humidity <= 30f)
            return new Color(0.098f, 0.804f, 0.863f); // Dry (#19CDDC)

        if (humidity <= 60f)
            return new Color(0.043f, 0.714f, 1f);     // Comfortable (#0BB6FF)

        if (humidity <= 80f)
            return new Color(0.082f, 0.420f, 0.796f); // Humid (#156BCB)

        return new Color(0.145f, 0.129f, 0.745f);     // Very Humid (#2521BE)
    }

    //clean balls when leave latest button
    public void ClearBalls()
    {
        if (ballSpawnRoot == null) return;

        for (int i = ballSpawnRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = ballSpawnRoot.GetChild(i);

            if (child.name.Contains("AQIBall"))
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }
    }

    public void SetTime(string isoTime)
    {
        if (timeText == null) return;

        if (string.IsNullOrEmpty(isoTime))
        {
            timeText.text = "TIME: N/A";
            return;
        }

        if (DateTime.TryParse(isoTime, out DateTime dt))
        {
            DateTime localTime = dt.ToLocalTime();
            string timezone = TimeZoneInfo.Local.IsDaylightSavingTime(localTime) ? "EEST" : "EET";
            timeText.text = $"TIME: {localTime:yyyy-MM-dd HH:mm} ({timezone})";
        }
        else
        {
            timeText.text = "TIME: " + isoTime;
        }
    }
}