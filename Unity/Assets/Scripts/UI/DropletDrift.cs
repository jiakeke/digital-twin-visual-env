using UnityEngine;

public class DropletDrift : MonoBehaviour
{
    public float swayAmplitude = 8f;
    public float swayFrequency = 0.08f;

    private RectTransform _rt;
    private RectTransform _container;
    private float _phase;
    private float _baseX;
    private float _baseY;

    public void Init(RectTransform container, float baseX, float baseY)
    {
        _rt = GetComponent<RectTransform>();
        _container = container;
        _phase = Random.Range(0f, 1000f);
        _baseX = baseX;
        _baseY = baseY;
    }

    void Update()
    {
        if (_rt == null || _container == null) return;

        float t = Time.time + _phase;

        float swayX = Mathf.Sin(t * swayFrequency * Mathf.PI * 2f) * swayAmplitude;
        float swayY = Mathf.Cos(t * (swayFrequency * 0.8f) * Mathf.PI * 2f) * (swayAmplitude * 0.35f);

        _rt.anchoredPosition = new Vector2(_baseX + swayX, _baseY + swayY);
    }
}