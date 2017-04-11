using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(ParticleSystem))]
public class WaterController : MonoBehaviour
{
    private ParticleSystem waterParticles;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    public float waterStrengthMultiplier = 0.5f;

	void Start ()
    {
        SetupParticleSystem();
	}

    void OnParticleCollision(GameObject other)
    {
        Rigidbody otherBody = other.transform.root.gameObject.GetComponent<Rigidbody>();
        //Debug.Log(other.name);
        if (otherBody)
        {
            waterParticles.GetCollisionEvents(other, collisionEvents);
            for (int i = 0; i < collisionEvents.Count; i++)
            {
                Vector3 incidentVelocity = collisionEvents[i].velocity;
                Debug.DrawRay(collisionEvents[i].intersection, incidentVelocity, Color.black);
                otherBody.AddForce(incidentVelocity * waterStrengthMultiplier);
            }
        }
    }

    private void SetupParticleSystem()
    {
        waterParticles = GetComponent<ParticleSystem>();
        waterParticles.startLifetime = 1.3f;
        waterParticles.startSpeed = 10;
        waterParticles.startSize = 3;
        waterParticles.startColor = new Color(66f / 255f, 208f / 255f, 1, 1);
        waterParticles.gravityModifier = 9.8f;
        waterParticles.simulationSpace = ParticleSystemSimulationSpace.World;
        waterParticles.maxParticles = 750;
        ParticleSystem.EmissionModule emission = waterParticles.emission;
        emission.enabled = true;
        emission.rate = 500;
        ParticleSystem.ShapeModule shape = waterParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.ConeVolume;
        shape.angle = 0;
        shape.radius = 0.01f;
        shape.length = 3;
        ParticleSystem.CollisionModule collision = waterParticles.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.mode = ParticleSystemCollisionMode.Collision3D;
        collision.bounce = 0;
        collision.radiusScale = 0.3f;
        collision.enableInteriorCollisions = true;
        collision.sendCollisionMessages = true;
    }
}
