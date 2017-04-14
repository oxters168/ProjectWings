using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(ParticleSystem))]
public class ThrusterController : MonoBehaviour
{
    public VehicleController vehicleController;
    public ParticleSystem mainThruster { get; private set; }
    public Color mainColor = Color.yellow;
    public ParticleSystem secondaryThruster { get; private set; }
    public Color secondaryColor = new Color(1, 58f / 255f, 0);
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    public float thrusterStrength = 30f, thrusterPercent;

    void Start()
    {
        SetupThrusters();
	}

    void Update()
    {
        ManageThrusters();
        //Debug.DrawRay(transform.position, transform.forward * thrusterStrength * thrusterPercent * 0.25f, Color.green, 0.01f);
    }

    void OnParticleCollision(GameObject other)
    {
        if (other != transform.root.gameObject)
        {
            //Debug.Log(other.name);
            Rigidbody2D otherBody = other.GetComponent<Rigidbody2D>();
            if (otherBody)
            {
                mainThruster.GetCollisionEvents(other, collisionEvents);
                for (int i = 0; i < collisionEvents.Count; i++)
                {
                    Vector3 incidentVelocity = collisionEvents[i].velocity;
                    Vector3 intersection = collisionEvents[i].intersection;
                    Vector3 intersectionDir = intersection - transform.position;
                    if (otherBody != vehicleController.vehicleBody && Vector3.Dot(transform.forward, intersectionDir.normalized) > 0.9f)
                    {
                        //Vector3 colliderSize = otherBody.GetComponentInChildren<Collider2D>().bounds.size;
                        //float largestSide = Mathf.Max(colliderSize.x, colliderSize.y, colliderSize.z);

                        //float totalParticles = mainThruster.particleCount + secondaryThruster.particleCount;
                        //float percentVelocity = incidentVelocity.magnitude / (mainThruster.startSpeed + secondaryThruster.startSpeed) / 2f;
                        //if (percentVelocity < 0) percentVelocity = 0;
                        //if (percentVelocity > 1) percentVelocity = 1;

                        float incidentDistance = intersectionDir.magnitude, flameLength = mainThruster.startSpeed * mainThruster.startLifetime;
                        //float torqueSign = 
                        //float torqueSign = (Vector3.Dot(otherBody.transform.right, intersectionDir.normalized) * -1) * (Vector3.Dot(otherBody.transform.forward, intersectionDir.normalized));
                        float torqueSign = Mathf.Sin(2 * VectorHelpers.AngleSigned(otherBody.transform.forward, intersectionDir.normalized, Vector3.forward)) > 0 ? -1 : 1;
                        //float percentTorque = Mathf.Clamp01(incidentDistance / largestSide);
                        float percentForce = 1 - (incidentDistance / flameLength);
                        if (percentForce < 0) percentForce = 0;
                        if (percentForce > 1) percentForce = 1;

                        float forceOnPoint = (vehicleController.thrustForce * percentForce / vehicleController.thrusters.Length) / Mathf.Clamp(mainThruster.particleCount / 5, 1, 1000);
                        Vector3 thrustForce = incidentVelocity.normalized * forceOnPoint;
                        otherBody.AddForce(thrustForce);
                        otherBody.AddTorque(forceOnPoint * incidentDistance * torqueSign);
                        Debug.DrawRay(intersection, thrustForce, Color.black);
                    }
                }
            }
        }
    }

    private void SetupThrusters()
    {
        SetupMainThruster();
        SetupSecondaryThruster();
    }
    private void SetupMainThruster()
    {
        mainThruster = GetComponent<ParticleSystem>();

        #region Main Properties
        mainThruster.startLifetime = 0.25f;
        mainThruster.startSpeed = thrusterStrength;
        mainThruster.startSize = 2.5f;
        mainThruster.gravityModifier = 9.8f;
        mainThruster.simulationSpace = ParticleSystemSimulationSpace.World;
        mainThruster.scalingMode = ParticleSystemScalingMode.Shape;
        mainThruster.playOnAwake = false;
        mainThruster.maxParticles = 0;
        #endregion
        #region Emission Module
        ParticleSystem.EmissionModule emission = mainThruster.emission;
        emission.enabled = true;
        emission.rate = 0;
        #endregion
        #region Shape Module
        ParticleSystem.ShapeModule shape = mainThruster.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 0;
        shape.radius = 0.01f;
        #endregion
        #region Color Over Lifetime Module
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = mainThruster.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(new GradientColorKey[] { new GradientColorKey(mainColor, 0.3f), new GradientColorKey(Color.white, 1f) }, new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) });
        colorOverLifetime.color = gradient;
        #endregion
        #region Size Over Lifetime Module
        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = mainThruster.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.separateAxes = false;
        AnimationCurve curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1, curve);
        #endregion
        #region Collision Module
        ParticleSystem.CollisionModule collision = mainThruster.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.mode = ParticleSystemCollisionMode.Collision2D;
        collision.bounce = 0;
        collision.radiusScale = 0.5f;
        collision.enableInteriorCollisions = false;
        collision.sendCollisionMessages = true;
        LayerMask layerMask = new LayerMask();
        layerMask.value = (1 << LayerMask.NameToLayer("World")) | (1 << LayerMask.NameToLayer("Vehicle") | (1 << LayerMask.NameToLayer("Object")));
        collision.collidesWith = layerMask;
        #endregion
        #region Renderer Module
        ParticleSystemRenderer renderer = GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Resources.Load<Material>("Materials/Particles/Thruster"));
        renderer.sortingFudge = 10;
        #endregion
    }
    private void SetupSecondaryThruster()
    {
        GameObject secondaryThrusterObject = new GameObject("SecondColor");
        secondaryThrusterObject.transform.parent = transform;
        secondaryThrusterObject.transform.localPosition = Vector3.zero;
        secondaryThrusterObject.transform.rotation = Quaternion.Euler(90, 0, 0);
        secondaryThruster = secondaryThrusterObject.AddComponent<ParticleSystem>();

        #region Main Properties
        secondaryThruster.startLifetime = 0.25f;
        secondaryThruster.startSpeed = thrusterStrength;
        secondaryThruster.startSize = 2.5f;
        secondaryThruster.gravityModifier = 9.8f;
        secondaryThruster.simulationSpace = ParticleSystemSimulationSpace.World;
        secondaryThruster.scalingMode = ParticleSystemScalingMode.Shape;
        secondaryThruster.playOnAwake = false;
        secondaryThruster.maxParticles = 0;
        #endregion
        #region Emission Module
        ParticleSystem.EmissionModule emission = secondaryThruster.emission;
        emission.enabled = true;
        emission.rate = 0;
        #endregion
        #region Shape Module
        ParticleSystem.ShapeModule shape = secondaryThruster.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 0;
        shape.radius = 0.01f;
        #endregion
        #region Color Over Lifetime Module
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = secondaryThruster.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(new GradientColorKey[] { new GradientColorKey(secondaryColor, 0.3f), new GradientColorKey(Color.white, 1f) }, new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) });
        colorOverLifetime.color = gradient;
        #endregion
        #region Size Over Lifetime Module
        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = secondaryThruster.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.separateAxes = false;
        AnimationCurve curve = new AnimationCurve(new Keyframe(0.1f, 0), new Keyframe(0.18f, 0.44f), new Keyframe(0.3f, 0.56f), new Keyframe(0.93f, 0));
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1, curve);
        #endregion
        #region Collision Module
        ParticleSystem.CollisionModule collision = secondaryThruster.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.mode = ParticleSystemCollisionMode.Collision2D;
        collision.bounce = 0;
        collision.radiusScale = 0.5f;
        collision.enableInteriorCollisions = false;
        //collision.sendCollisionMessages = true;
        LayerMask layerMask = new LayerMask();
        layerMask.value = (1 << LayerMask.NameToLayer("World")) | (1 << LayerMask.NameToLayer("Vehicle") | (1 << LayerMask.NameToLayer("Object")));
        collision.collidesWith = layerMask;
        #endregion
        #region Renderer Module
        ParticleSystemRenderer renderer = secondaryThruster.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Resources.Load<Material>("Materials/Particles/Thruster"));
        #endregion
    }
    private void ManageThrusters()
    {
        ManageMainThruster();
        ManageSecondaryThruster();
    }
    private void ManageMainThruster()
    {
        mainThruster.startSpeed = thrusterStrength * thrusterPercent;
        ParticleSystem.EmissionModule emission = mainThruster.emission;
        float emissionRate = vehicleController.topSpeed * VehicleController.ME2KI * thrusterPercent;
        emission.rate = emissionRate;
        mainThruster.maxParticles = Mathf.CeilToInt(emissionRate * mainThruster.startLifetime);
    }
    private void ManageSecondaryThruster()
    {
        secondaryThruster.startSpeed = thrusterStrength * thrusterPercent;
        ParticleSystem.EmissionModule emission = secondaryThruster.emission;
        float emissionRate = vehicleController.topSpeed * VehicleController.ME2KI * thrusterPercent;
        emission.rate = emissionRate;
        secondaryThruster.maxParticles = Mathf.CeilToInt(emissionRate * secondaryThruster.startLifetime);
    }

    public void StartThruster()
    {
        if (mainThruster != null) mainThruster.Play();
        if (secondaryThruster != null) secondaryThruster.Play();
    }
    public void StopThruster()
    {
        if (mainThruster != null) mainThruster.Stop();
        if (secondaryThruster != null) secondaryThruster.Stop();
    }
}
