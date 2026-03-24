using System.Collections.Generic;
using UnityEngine;

public class CloudManager : MonoBehaviour
{
    [Header("Cloud Setup")]
    public GameObject cloudPrefab;
    public int cloudCount = 12;
    public float cloudHeight = 220f;
    public Vector2 areaSize = new Vector2(800f, 800f);

    [Header("Wind")]
    public float windSpeedMetersPerSecond = 5f;
    public float windDirectionDegrees = 270f; // meteorological: FROM direction (common), see below
    public bool directionIsFrom = true;

    [Header("Motion Tuning")]
    public float speedMultiplier = 8f;

    private readonly List<Transform> _clouds = new();

    void Start()
    {
        SpawnClouds();
    }

    void Update()
    {
        Vector3 windDir = WindDirectionToWorld(windDirectionDegrees, directionIsFrom);
        float speed = Mathf.Max(0f, windSpeedMetersPerSecond) * speedMultiplier;

        Vector3 delta = windDir * speed * Time.deltaTime;

        for (int i = 0; i < _clouds.Count; i++)
        {
            Transform t = _clouds[i];
            t.position += delta;
            Wrap(t);
        }
    }

    public void SetWind(float speedMps, float directionDeg, bool isFromDirection = true)
    {
        windSpeedMetersPerSecond = speedMps;
        windDirectionDegrees = directionDeg;
        directionIsFrom = isFromDirection;
    }

    void SpawnClouds()
    {
        if (cloudPrefab == null) return;

        _clouds.Clear();
        for (int i = 0; i < cloudCount; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
                cloudHeight + Random.Range(-15f, 15f),
                Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f)
            );

            GameObject go = Instantiate(cloudPrefab, transform);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.identity;

            float s = Random.Range(0.7f, 1.4f);
            go.transform.localScale *= s;

            _clouds.Add(go.transform);
        }
    }

    void Wrap(Transform t)
    {
        Vector3 p = t.localPosition;

        float halfX = areaSize.x * 0.5f;
        float halfZ = areaSize.y * 0.5f;

        if (p.x > halfX) p.x = -halfX;
        else if (p.x < -halfX) p.x = halfX;

        if (p.z > halfZ) p.z = -halfZ;
        else if (p.z < -halfZ) p.z = halfZ;

        t.localPosition = p;
    }

    // Converts meteorological degrees to Unity world direction on XZ plane.
    // Meteo degrees: 0=N, 90=E, 180=S, 270=W.
    // "FROM" means wind comes from that direction; cloud moves "TO" opposite direction.
    Vector3 WindDirectionToWorld(float deg, bool isFrom)
    {
        float rad = deg * Mathf.Deg2Rad;

        // Meteo: 0=N => +Z, 90=E => +X
        Vector3 fromDir = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)).normalized;

        Vector3 toDir = -fromDir;
        return (isFrom ? toDir : fromDir);
    }
}