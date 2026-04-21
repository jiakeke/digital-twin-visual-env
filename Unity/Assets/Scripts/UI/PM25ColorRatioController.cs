using UnityEngine;

[ExecuteAlways]
public class PM25ColorRatioController : MonoBehaviour
{
    [Header("PM2.5 (ug/m3)")]
    [Range(0, 120)]
    public float pm25 = 8f;

    [Header("Thresholds")]
    public float safeValue = 15f;
    public float pmMax = 120f;

    [Header("Total emission mapping")]
    public float totalRateMin = 0f;
    public float totalRateMax = 900f;

    [Header("Red ratio mapping")]
    [Range(0f, 0.2f)]
    public float redRatioAtSafe = 0.01f;
    [Range(0.8f, 1f)]
    public float redRatioAtMax = 0.99f;

    [Header("Particle Systems")]
    public ParticleSystem greenPs;
    public ParticleSystem redPs;

    [Header("Debug Behavior")]
    public bool clearOnPMChange = true;
    public float clearThreshold = 1f;

    float _lastPm = float.NaN;

    void OnEnable() => Apply(true);
    void OnValidate() => Apply(false);

    public void SetPM25(float value)
    {
        pm25 = value;
        Apply(true);
    }

    void Apply(bool forceClear)
    {
        if (greenPs == null || redPs == null) return;

        bool shouldClear = forceClear;

        if (clearOnPMChange)
        {
            if (float.IsNaN(_lastPm) || Mathf.Abs(pm25 - _lastPm) >= clearThreshold)
                shouldClear = true;
        }

        _lastPm = pm25;

        float pmClamped = Mathf.Clamp(pm25, 0f, pmMax);

        float totalT = Mathf.InverseLerp(0f, pmMax, pmClamped);
        float totalRate = Mathf.Lerp(totalRateMin, totalRateMax, totalT);

        float redRatio;
        if (pm25 <= safeValue) redRatio = redRatioAtSafe;
        else
        {
            float t = Mathf.InverseLerp(safeValue, pmMax, pm25);
            redRatio = Mathf.Lerp(redRatioAtSafe, redRatioAtMax, Mathf.Clamp01(t));
        }

        float redRate = totalRate * redRatio;
        float greenRate = totalRate * (1f - redRatio);

        SetRate(greenPs, greenRate);
        SetRate(redPs, redRate);

        if (shouldClear)
        {
            greenPs.Clear(true);
            redPs.Clear(true);
        }

        if (!greenPs.isPlaying) greenPs.Play(true);
        if (!redPs.isPlaying) redPs.Play(true);
    }

    static void SetRate(ParticleSystem ps, float rate)
    {
        var em = ps.emission;
        var rot = em.rateOverTime;
        rot.constant = Mathf.Max(0f, rate);
        em.rateOverTime = rot;
    }
}