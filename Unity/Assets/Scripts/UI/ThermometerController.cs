using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ThermometerUI : MonoBehaviour
{
    public RectTransform tubeRect;
    public RectTransform fillRect;
    public RectTransform pointerRect;

    [Header("Temperature Range")]
    public float minTemp = -20f;
    public float maxTemp = 40f;

    [Range(-50, 60)]
    public float currentTemp = 10f;

    [Header("Optional UI Text")]
    public TMP_Text temperatureValueText;
    public string temperatureUnit = "°„C";
    public int decimalPlaces = 1;

    void OnEnable()
    {
        Apply(currentTemp);
    }

    void OnValidate()
    {
        Apply(currentTemp);
    }

    public void SetTemperature(float temp)
    {
        currentTemp = temp;
        Apply(temp);
    }

    void Apply(float temp)
    {
        if (tubeRect == null) return;

        float t01 = Mathf.Clamp01(Mathf.InverseLerp(minTemp, maxTemp, temp));
        float h = tubeRect.rect.height;

        if (fillRect != null)
        {
            Vector2 size = fillRect.sizeDelta;
            size.y = h * t01;
            fillRect.sizeDelta = size;
        }

        if (pointerRect != null)
        {
            Vector2 p = pointerRect.anchoredPosition;
            p.y = h * t01;
            pointerRect.anchoredPosition = p;
        }

        var img = fillRect ? fillRect.GetComponent<Image>() : null;
        if (img != null)
        {
            Color cold = new Color(0.2f, 0.6f, 1f);
            Color hot = new Color(1f, 0.2f, 0.2f);
            img.color = Color.Lerp(cold, hot, t01);
        }

        if (temperatureValueText != null)
        {
            temperatureValueText.text = temp.ToString($"F{decimalPlaces}") + temperatureUnit;
        }
    }
}