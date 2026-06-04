using UnityEngine;

/// <summary>
/// Simple idle rotation for the summon circle.
/// Attach to the Image GameObject in the left panel area.
/// </summary>
public class SummonIdleCircle : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(0, 0, -30f * Time.deltaTime);
    }
}