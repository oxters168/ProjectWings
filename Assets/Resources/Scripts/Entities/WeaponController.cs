using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(ParticleSystem))]
public class WeaponController : MonoBehaviour
{
    public ParticleSystem bulletParticles;
    ParticleSystem.EmissionModule emission;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    public float bulletSpeed = 100, bulletSize = 1;

    void Start()
    {
        SetupBulletParticles();
    }

    void OnParticleCollision(GameObject other)
    {
        Rigidbody otherBody = other.GetComponent<Rigidbody>();
        if (otherBody != null)
        {
            bulletParticles.GetCollisionEvents(other, collisionEvents);
            for (int i = 0; i < collisionEvents.Count; i++)
            {
                Vector3 incidentVelocity = collisionEvents[i].velocity;
                Debug.DrawRay(collisionEvents[i].intersection, incidentVelocity, Color.black);
                otherBody.AddForce(incidentVelocity.normalized * bulletSize);
            }
        }
    }

    private void SetupBulletParticles()
    {
        bulletParticles = GetComponent<ParticleSystem>();

        #region Main Properties
        bulletParticles.startLifetime = 5;
        bulletParticles.startSpeed = bulletSpeed;
        bulletParticles.startSize = bulletSize;
        bulletParticles.startColor = new Color(176f / 255f, 138f / 255f, 57f / 255f);
        bulletParticles.gravityModifier = 9.8f;
        bulletParticles.simulationSpace = ParticleSystemSimulationSpace.World;
        bulletParticles.playOnAwake = true;
        bulletParticles.maxParticles = 100;
        #endregion
        #region Emission Module
        emission = bulletParticles.emission;
        emission.enabled = true;
        emission.rate = 0;
        #endregion
        #region Shape Module
        ParticleSystem.ShapeModule shape = bulletParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 0;
        shape.radius = 0.01f;
        #endregion
        #region Collision Module
        ParticleSystem.CollisionModule collision = bulletParticles.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.mode = ParticleSystemCollisionMode.Collision3D;
        collision.bounce = 0;
        collision.radiusScale = 1f;
        collision.enableInteriorCollisions = false;
        collision.sendCollisionMessages = true;
        #endregion
        #region Renderer Module
        ParticleSystemRenderer renderer = bulletParticles.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Resources.Load<Material>("Materials/Particles/Bullet"));
        #endregion
    }

    public void StartFiring()
    {
        if (bulletParticles != null) emission.rate = 10;
    }
    public void StopFiring()
    {
        if (bulletParticles != null) emission.rate = 0;
    }
}
