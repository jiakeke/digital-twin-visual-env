using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class AQIBarUI : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform barFrameRect;
    public RectTransform pointerRect;
    public TMP_Text valueText;
    public TMP_Text timeText;

    [Header("Ball Effect")]
    public RectTransform ballSpawnRoot;
    public GameObject ballPrefab;

    [Header("AQI Range")]
    public float minAQI = 0f;
    public float maxAQI = 200f;

    [Header("Debug")]
    public float currentAQI = 25f;

    [Header("Ball Settings")]
    public float spawnOffsetY = 35f;  //ball's position compare with pointer's
    public float groundOffsetY = -200f;   
    public float firstBounceHeight = 60f;
    public float secondBounceHeight = 45f;

   
    void OnEnable()
    {
        Apply(currentAQI);
    }

    void OnValidate()
    {
        Apply(currentAQI);
    }

    public void SetAQI(float aqi)
    {
        currentAQI = aqi;
        Apply(aqi);

        if (Application.isPlaying)
        {
            SpawnAQIBall(aqi);
        }
    }

    void Apply(float aqi)
    {
        if (barFrameRect == null || pointerRect == null) return;

        aqi = Mathf.Clamp(aqi, minAQI, maxAQI);

        float t = Mathf.InverseLerp(minAQI, maxAQI, aqi);
        float w = barFrameRect.rect.width;

        // Map AQI to the bar from left to right
        float localX = Mathf.Lerp(-w * 0.5f, w * 0.5f, t);

        Vector2 p = pointerRect.anchoredPosition;
        p.x = barFrameRect.anchoredPosition.x + localX;
        pointerRect.anchoredPosition = p;

        if (valueText != null)
        {
            valueText.text = $"AQI: {Mathf.RoundToInt(aqi)}";
        }
    }

    //ball drop off
    void SpawnAQIBall(float aqi)
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
            ballImage.color = GetAQIColor(aqi);
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

    Color GetAQIColor(float aqi)
    {
        if (aqi <= 50f)
            return new Color(0.38f, 0.85f, 0.53f);   // Green

        if (aqi <= 75f)
            return new Color(0.93f, 0.76f, 0.22f);   // Yellow

        if (aqi <= 100f)
            return new Color(0.96f, 0.51f, 0.25f);   // Orange

        if (aqi <= 150f)
            return new Color(0.87f, 0.22f, 0.31f);   // Red

        return new Color(0.56f, 0.39f, 0.84f);       // Purple
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

    //Time switch for AQI from "hsy"
    public void SetTime(string isoTime)
    {
        if (timeText == null) return;

        if (string.IsNullOrEmpty(isoTime))
        {
            timeText.text = "TIME: N/A";
            return;
        }

        // Remove trailing Z so it won't be auto-converted from UTC to local time
        string cleanedTime = isoTime.TrimEnd('Z');

        if (DateTime.TryParse(cleanedTime, out DateTime dt))
        {
            // HSY time currently behaves like EEST already,
            // so do NOT call ToLocalTime() here

            string timeStr = dt.ToString("yyyy-MM-dd HH:mm");
            string timezone = "EEST"; // or EET, depending on season if you want to hardcode temporarily

            timeText.text = $"TIME: {timeStr} ({timezone})";
        }
        else
        {
            timeText.text = "TIME: " + isoTime;
        }
    }
}