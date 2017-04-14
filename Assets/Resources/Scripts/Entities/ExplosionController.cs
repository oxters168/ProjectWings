using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ExplosionController : MonoBehaviour
{
    private ParticleSystem _explosionEffect;
    public ParticleSystem explosionEffect { get { if (!_explosionEffect) _explosionEffect = GetComponent<ParticleSystem>(); return _explosionEffect; } }
    
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    private float pushForce;
    public float explosionSize, explosionDamage;

    public void Explode()
    {
        if (explosionEffect)
        {
            explosionEffect.startSize = Mathf.Clamp(explosionSize, 0, 100);
            explosionEffect.Play();
            StartCoroutine(WaitForDestruction());
        }
    }
    private IEnumerator WaitForDestruction()
    {
        while (explosionEffect.isPlaying) yield return null;
        Destroy(gameObject);
    }

    void OnParticleCollision(GameObject other)
    {
        if (other != transform.root.gameObject && explosionEffect)
        {
            float percentStrength = Mathf.Clamp01(1 - (explosionEffect.time / explosionEffect.duration));

            Rigidbody2D rigidBody = other.GetComponent<Rigidbody2D>();
            if (rigidBody)
            {
                explosionEffect.GetCollisionEvents(other, collisionEvents);
                for (int i = 0; i < collisionEvents.Count; i++)
                {
                    //Vector3 intersection = collisionEvents[i].intersection;
                    //Vector3 intersectionDir = intersection - transform.position;
                    Vector3 outDirection = other.transform.position - transform.position;

                    pushForce = explosionEffect.startSize * 750;
                    rigidBody.AddForce(outDirection * (pushForce / Mathf.Clamp(explosionEffect.particleCount, 1, 1000)) * percentStrength);
                }
            }

            Damageable damageableBody = other.GetComponent<Damageable>();
            if (damageableBody != null)
            {
                damageableBody.ModifyHealth(-(explosionDamage / Mathf.Clamp(explosionEffect.particleCount, 1, 1000)) * percentStrength);
            }
        }
    }
}
