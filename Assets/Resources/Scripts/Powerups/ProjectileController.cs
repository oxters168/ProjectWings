using System.Collections;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float hullDamage;
    public float timeToLive;
    private float birthTime;
    private bool hasHit;

    public Transform lockTarget;
    private Rigidbody2D _projectileBody;
    public Rigidbody2D projectileBody { get { if (!_projectileBody) PrepareProjectileBody(); return _projectileBody; } private set { _projectileBody = value; } }
    public float lockRange;
    public float cappedRocketSpeed, cappedRocketRotSpeed;
    public float rocketAcceleration, rocketRotAcceleration;
    //private float defaultLinearDrag, defaultAngularDrag;

    public ExplosionController explosionPrefab;
    public float explosiveSize, explosiveDurability;

    public float rotTimeout;
    private float thrustPercent, percentRotation;
    private Coroutine rotTimeoutCoroutine;
    private float defaultLinearDrag, defaultAngularDrag;

    public float thrustForce { get { if (projectileBody) return ((rocketAcceleration - Physics.gravity.magnitude) * projectileBody.mass); else return 0; } }
    public float topSpeed { get { if (projectileBody) return (((thrustForce / projectileBody.drag) - Time.fixedDeltaTime * thrustForce) / projectileBody.mass); else return 0; } }
    public float torqueForce { get { if (projectileBody) return (rocketRotAcceleration * projectileBody.mass); else return 0; } }
    public float topRotSpeed { get { if (projectileBody) return (((torqueForce / projectileBody.angularDrag) - Time.fixedDeltaTime * torqueForce) / projectileBody.mass); else return 0; } }

    void Start()
    {
        birthTime = Time.time;
        PrepareProjectileBody();
    }
	void Update ()
    {
        CheckLifeSpan();
        CheckSurroundings();
        CalculateTrajectory();
    }
    void FixedUpdate()
    {
        Traject();
    }
    void OnCollisionEnter2D(Collision2D col)
    {
        if (explosiveSize > 0 && col.relativeVelocity.magnitude > explosiveDurability)
        {
            SelfDestruct();
            return;
        }
        if (!hasHit)
        {
            Damageable damageableObject = col.gameObject.GetComponent<Damageable>();
            if (damageableObject != null)
            {
                if (explosiveSize <= 0) damageableObject.ModifyHealth(-hullDamage);
                SelfDestruct();
            }
        }
        hasHit = true;
    }

    private void PrepareProjectileBody()
    {
        if (!_projectileBody)
        {
            _projectileBody = GetComponentInParent<Rigidbody2D>();
            if (!_projectileBody) GetComponentInChildren<Rigidbody2D>();

            defaultLinearDrag = projectileBody.drag;
            defaultAngularDrag = projectileBody.angularDrag;
        }
    }

    private void CheckSurroundings()
    {
        if(explosiveSize > 0)
        {
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, explosiveSize * 1f, transform.forward);
            if(hit && hit.transform.GetComponent<Damageable>() != null && Time.time - birthTime > timeToLive * 0.1f)
            {
                SelfDestruct();
            }
        }
    }
    private void CalculateTrajectory()
    {
        if(projectileBody && cappedRocketRotSpeed > 0)
        {
            thrustPercent = 1;
            
            if (lockTarget)
            {
                Vector3 displacement = lockTarget.position - transform.position;
                if (displacement.magnitude <= lockRange)
                {
                    float angle = VectorHelpers.AngleSigned(transform.forward, displacement.normalized, Vector3.forward);
                    float maxRotationAngle = 360 * cappedRocketRotSpeed * Time.fixedDeltaTime;
                    percentRotation = Mathf.Clamp(angle / maxRotationAngle, -1, 1);
                    //projectileBody.angularDrag = torqueForce / ((cappedRocketRotSpeed * VehicleController.KI2ME * projectileBody.mass) + (Time.fixedDeltaTime / torqueForce));
                }
            }
        }
    }
    private void Traject()
    {
        if (thrustPercent > 0) projectileBody.drag = thrustForce / ((cappedRocketSpeed * VehicleController.KI2ME * projectileBody.mass) + (Time.fixedDeltaTime / thrustForce));
        else projectileBody.drag = defaultLinearDrag;
        projectileBody.AddForce(transform.forward * thrustPercent * thrustForce);

        if (percentRotation != 0)
        {
            if (rotTimeoutCoroutine != null) { StopCoroutine(rotTimeoutCoroutine); rotTimeoutCoroutine = null; }
            projectileBody.angularDrag = torqueForce / ((cappedRocketRotSpeed * VehicleController.KI2ME * projectileBody.mass) + (Time.fixedDeltaTime / torqueForce));
        }
        else if (projectileBody.angularDrag > defaultAngularDrag && rotTimeoutCoroutine == null)
        {
            rotTimeoutCoroutine = StartCoroutine(ResetRotDrag());
        }
        projectileBody.AddTorque(Mathf.Clamp(percentRotation, -1, 1) * torqueForce);
    }
    private IEnumerator ResetRotDrag()
    {
        yield return new WaitForSeconds(rotTimeout);
        projectileBody.angularDrag = defaultAngularDrag;
    }

    private void CheckLifeSpan()
    {
        if (Time.time - birthTime >= timeToLive) SelfDestruct();
    }
    private void SelfDestruct()
    {
        if(explosiveSize > 0 && explosionPrefab)
        {
            ExplosionController explosion = Instantiate(explosionPrefab);
            explosion.transform.position = transform.position + transform.forward * (explosiveSize / 2);
            explosion.explosionSize = explosiveSize;
            //explosion.pushForce = thrustForce;
            explosion.explosionDamage = hullDamage;
            explosion.Explode();
        }
        Destroy(gameObject);
    }
}
