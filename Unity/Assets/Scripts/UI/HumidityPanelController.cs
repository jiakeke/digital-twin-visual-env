using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HumidityPanelController : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text humidityText;
    public Transform dropletGrid;
    public Image dropletTemplate;

    [Header("Grid Layout")]
    public int columns = 20;
    public int rows = 5;
    public Vector2 padding = new Vector2(60f, 160f); // left/right, top/bottom margin

    [Range(0, 100)]
    public float currentHumidity = 60f;
    public float minDistance = 60f;

    public Color dryColor = new Color(1f, 1f, 1f, 0.20f);
    public Color wetColor = new Color(0.2f, 0.6f, 1f, 0.85f);

    private readonly List<Image> _droplets = new();

    void Awake()
    {
        BuildDropletsIfNeeded();
        Apply(currentHumidity);
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        BuildDropletsIfNeeded();
        Apply(currentHumidity);
    }

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
        if (gameObject.activeSelf)
            Apply(currentHumidity);
    }

    public void SetHumidity(float humidity)
    {
        currentHumidity = Mathf.Clamp(humidity, 0f, 100f);
        Apply(currentHumidity);
    }

    void BuildDropletsIfNeeded()
    {
        int count = Mathf.Max(1, columns * rows);

        if (_droplets.Count == count) return;
        if (dropletGrid == null || dropletTemplate == null) return;

        _droplets.Clear();

        for (int i = dropletGrid.childCount - 1; i >= 0; i--)
        {
            var child = dropletGrid.GetChild(i);
            if (child != dropletTemplate.transform)
                Destroy(child.gameObject);
        }

        dropletTemplate.gameObject.SetActive(false);

        RectTransform container = dropletGrid as RectTransform;
        if (container == null) container = dropletGrid.GetComponent<RectTransform>();

        float width = container.rect.width;
        float height = container.rect.height;

        float left = -width * 0.5f + padding.x;
        float right = width * 0.5f - padding.x;
        float top = height * 0.5f - padding.y;
        float bottom = -height * 0.5f + padding.y;

        float stepX = (columns <= 1) ? 0f : (right - left) / (columns - 1);
        float stepY = (rows <= 1) ? 0f : (top - bottom) / (rows - 1);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                Image d = Instantiate(dropletTemplate, dropletGrid);
                d.gameObject.SetActive(true);

                RectTransform rt = d.rectTransform;
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);

                float x = left + stepX * c;
                float y = top - stepY * r;

                rt.anchoredPosition = new Vector2(x, y);

                float s = Random.Range(0.85f, 1.15f);
                rt.localScale = new Vector3(s, s, 1f);

                var drift = d.GetComponent<DropletDrift>();
                if (drift != null)
                    drift.Init(container, x, y);

                _droplets.Add(d);
            }
        }
    }

    void Apply(float humidity)
    {
        if (humidityText != null)
            humidityText.text = $"Humidity: {humidity:0}%";

        int total = columns * rows;
        int blueCount = Mathf.RoundToInt((humidity / 100f) * total);
        blueCount = Mathf.Clamp(blueCount, 0, total);

        for (int i = 0; i < _droplets.Count; i++)
        {
            bool isBlue = i >= (_droplets.Count - blueCount);
            _droplets[i].color = isBlue ? wetColor : dryColor;
        }
    }
}