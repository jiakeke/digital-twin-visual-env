using UnityEngine;
using TMPro;

[ExecuteAlways]
public class ThermometerTicks : MonoBehaviour
{
    public ThermometerUI thermometer;
    public RectTransform tubeRect;

    public int step = 10;
    public int minLabel = -20;
    public int maxLabel = 40;

    public TMP_Text labelPrefab;
    public float xOffset = -10f;

    void Start()
    {
        Rebuild();
    }
    public void Rebuild()
    {
        if (thermometer == null || tubeRect == null || labelPrefab == null) return;
        if (step <= 0 || maxLabel <= minLabel) return;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying) Destroy(transform.GetChild(i).gameObject);
            else DestroyImmediate(transform.GetChild(i).gameObject);
        }

        float h = tubeRect.rect.height;

        for (int v = minLabel; v <= maxLabel; v += step)
        {
            float t01 = Mathf.Clamp01(Mathf.InverseLerp(thermometer.minTemp, thermometer.maxTemp, v));
            float y = h * t01;

            TMP_Text txt = CreateLabel();
            txt.text = v.ToString();

            var rt = txt.rectTransform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(1f, 0.5f);
            rt.anchoredPosition = new Vector2(xOffset, y);
        }
    }

    TMP_Text CreateLabel()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return (TMP_Text)UnityEditor.PrefabUtility.InstantiatePrefab(labelPrefab, transform);
#endif
        return Instantiate(labelPrefab, transform);
    }
}