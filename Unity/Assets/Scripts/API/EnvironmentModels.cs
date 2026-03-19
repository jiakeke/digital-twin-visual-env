using System;

[Serializable]
public class LatestDataResponse
{
    public string fetched_at;
    public LatestDataReading[] readings;
}

[Serializable]
public class LatestDataReading
{
    public string source;
    public string location_id;
    public float latitude;
    public float longitude;
    public string measured_at;
    public string metric;
    public float value;
    public string unit;
}