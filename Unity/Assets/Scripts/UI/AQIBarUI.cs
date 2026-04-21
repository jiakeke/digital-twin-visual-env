using UnityEngine;
using TMPro;

[ExecuteAlways]
public class AQIBarUI : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform barFrameRect;
    public RectTransform pointerRect;
    public TMP_Text valueText;

    [Header("AQI Range")]
    public float minAQI = 0f;
    public float maxAQI = 120f;

    [Header("Debug")]
    public float currentAQI = 25f;

    void OnEnable() => Apply(currentAQI);
    void OnValidate() => Apply(currentAQI);

    public void SetAQI(float aqi)
    {
        currentAQI = aqi;
        Apply(aqi);
    }

    void Apply(float aqi)
    {
        if (barFrameRect == null || pointerRect == null) return;

        float t = Mathf.InverseLerp(minAQI, maxAQI, aqi);
        t = Mathf.Clamp01(t);

        float w = barFrameRect.rect.width;
        float x = Mathf.Lerp(0f, w, t);

        Vector2 p = pointerRect.anchoredPosition;
        p.x = barFrameRect.anchoredPosition.x - w * 0.5f + x;
        pointerRect.anchoredPosition = p;

        if (valueText != null)
            valueText.text = $"AQI: {aqi:0}";
    }
}