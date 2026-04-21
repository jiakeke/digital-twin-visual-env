using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(ParticleSystem))]
public class PM25ParticleController : MonoBehaviour
{
    [Header("PM2.5 (ug/m3)")]
    [Range(0, 120)]
    public float pm25 = 10f;

    public bool useAQI = false;

    [Header("AQI Range")]
    public float aqiMin = 0f;
    public float aqiMax = 120f;

    [Header("Mapping")]
    public float pmMin = 0f;
    public float pmMax = 120f;

    [Header("Emission")]
    public float rateMin = 0f;
    public float rateMax = 800f;

    [Header("Appearance")]
    public float sizeMin = 0.05f;
    public float sizeMax = 0.1f;

    [Range(0f, 1f)]
    public float alphaMin = 0.02f;
    [Range(0f, 1f)]
    public float alphaMax = 0.25f;

    [Header("Motion")]
    public float speedMin = 0.00f;
    public float speedMax = 0.08f;

    private ParticleSystem _ps;
    private ParticleSystem.MainModule _main;
    private ParticleSystem.EmissionModule _emission;

    void OnEnable()
    {
        Cache();
        Apply();
    }

    void OnValidate()
    {
        Cache();
        Apply();
    }

    public void SetPM25(float value)
    {
        pm25 = value;
        Apply();
    }

    void Cache()
    {
        if (_ps == null) _ps = GetComponent<ParticleSystem>();
        _main = _ps.main;
        _emission = _ps.emission;
    }

    void Apply()
    {
        if (_ps == null) return;

        float t = Mathf.InverseLerp(pmMin, pmMax, pm25);
        t = Mathf.Clamp01(t);

        float rate = Mathf.Lerp(rateMin, rateMax, t);
        var rateOverTime = _emission.rateOverTime;
        rateOverTime.constant = rate;
        _emission.rateOverTime = rateOverTime;

        float size = Mathf.Lerp(sizeMin, sizeMax, t);
        _main.startSize = size;

        float speed = Mathf.Lerp(speedMin, speedMax, t);
        _main.startSpeed = speed;

        Color c = _main.startColor.color;
        c.a = Mathf.Lerp(alphaMin, alphaMax, t);
        _main.startColor = c;

        _ps.Play(true);
    }


    public void SetAQI(float aqi)
    {
        useAQI = true;
        pm25 = aqi;          // reuse the same pipeline
        pmMin = aqiMin;
        pmMax = aqiMax;
        Apply();
    }
}