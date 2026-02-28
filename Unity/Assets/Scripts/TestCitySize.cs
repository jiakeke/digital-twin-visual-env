using UnityEngine;

public class test : MonoBehaviour
{
    void Start()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;

        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        Debug.Log("City Center: " + bounds.center);
        Debug.Log("City Size: " + bounds.size);
    }
}