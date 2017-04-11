using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    private SolarSystem solarSystem;
    public float cycleDuration, orbitDuration;
    private Vector3 displacement;
    public float currentCycle, currentOrbit;

    void Start()
    {
        solarSystem = GetComponentInParent<SolarSystem>();
        displacement = transform.position - solarSystem.star.transform.position;
    }

    void FixedUpdate()
    {
        UpdateCycle();
        UpdateOrbit();
    }

    private void UpdateCycle()
    {
        if (cycleDuration > 0)
        {
            float deltaCycle = Time.fixedDeltaTime / cycleDuration;
            currentCycle += deltaCycle;
            currentCycle = Mathf.Repeat(currentCycle, 1);
            transform.rotation = Quaternion.AngleAxis(currentCycle * 360f, transform.up);
        }
    }
    private void UpdateOrbit()
    {
        if (solarSystem && orbitDuration > 0)
        {
            float deltaOrbit = Time.fixedDeltaTime / orbitDuration;
            currentOrbit += deltaOrbit;
            currentOrbit = Mathf.Repeat(currentOrbit, 1);
            transform.position = Quaternion.AngleAxis(currentOrbit * 360f, solarSystem.star.transform.up) * displacement + solarSystem.star.transform.position;
        }
    }
}
