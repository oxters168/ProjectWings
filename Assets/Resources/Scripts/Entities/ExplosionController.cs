using UnityEngine;
using System.Collections.Generic;

public class ExplosionController : MonoBehaviour
{
    public ParticleSystem explosionEffect;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    void OnParticleCollision(GameObject other)
    {
        if (other != transform.root.gameObject)
        {
            //Debug.Log(other.name);
            Rigidbody2D otherBody = other.GetComponent<Rigidbody2D>();
            if (otherBody)
            {
                explosionEffect.GetCollisionEvents(other, collisionEvents);
                for (int i = 0; i < collisionEvents.Count; i++)
                {
                    Vector3 incidentVelocity = collisionEvents[i].velocity;
                    Vector3 intersection = collisionEvents[i].intersection;
                    Vector3 intersectionDir = intersection - transform.position;
                    /*if (otherBody != shipController.vehicleBody && Vector3.Dot(transform.forward, intersectionDir.normalized) > 0.9f)
                    {
                        float totalParticles = explosionEffect.maxParticles + secondaryThruster.maxParticles;
                        //float percentVelocity = incidentVelocity.magnitude / (mainThruster.startSpeed + secondaryThruster.startSpeed) / 2f;
                        //if (percentVelocity < 0) percentVelocity = 0;
                        //if (percentVelocity > 1) percentVelocity = 1;

                        float incidentDistance = intersectionDir.magnitude, flameLength = mainThruster.startSpeed * mainThruster.startLifetime;
                        //float torqueSign = (Vector3.Dot(otherBody.transform.right, intersectionDir.normalized) * -1) * (Vector3.Dot(otherBody.transform.forward, intersectionDir.normalized));
                        float percentForce = 1 - (incidentDistance / flameLength);
                        if (percentForce < 0) percentForce = 0;
                        if (percentForce > 1) percentForce = 1;

                        float forceOnPoint = shipController.thrustForce * percentForce / shipController.thrusters.Length;
                        Vector3 thrustForce = incidentVelocity.normalized * forceOnPoint;
                        otherBody.AddForce(thrustForce / totalParticles);
                        Debug.DrawRay(intersection, thrustForce / totalParticles, Color.black);
                    }*/
                }
            }
        }
    }
}
