using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Seeker)), RequireComponent(typeof(SimpleSmoothModifier))]
public class AIDriver : Driver
{
    public float pathGenTime = 0.1f;
    private float lastPathTime;
    private Seeker pathfinder;

    private bool steering;
    private Vector3 destination;
    public float dangerRayTimeFrame = 1.2f;
    Vector2 dangerAvoidanceVector;
    private float riskCheckRadius;
    private bool inDanger;
    private RaycastHit2D imminentDanger;
    public float thrusterLerp = 5, steerLerp = 5, minTargetAngle = 2, minDangerAngle = 2;
    public float pathTimeDist = 0.3f;
    private float optimumDistance;

    private RaycastHit2D[] hits;

    public bool showLogic = false;

    protected override void Start()
    {
        base.Start();
        pathfinder = GetComponent<Seeker>();
        pathfinder.drawGizmos = false;
    }
    void Update()
    {
        CalculateOptimumDistance();
        FindPath();
        PassInput();
    }
    void OnDrawGizmos()
    {
        if (showLogic)
        {
            Gizmos.color = Color.green;
            //Gizmos.DrawSphere(destination, 1);
            Gizmos.DrawLine(transform.position, destination);

            Gizmos.color = inDanger ? Color.red : Color.black;
            Gizmos.DrawRay(transform.position, vehicle.velocity.normalized * riskCheckRadius);
            if (hits != null)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    Gizmos.color = (inDanger && hit.transform == imminentDanger.transform) ? Color.red : Color.black;
                    if (!hit.transform.IsChildOf(transform)) Gizmos.DrawSphere(hit.point, 1);
                    if (inDanger && hit.transform == imminentDanger.transform) Gizmos.DrawRay(hit.point, dangerAvoidanceVector * riskCheckRadius);
                }
            }
        }
    }

    private void CalculateOptimumDistance()
    {
        optimumDistance = vehicle.topSpeed * pathTimeDist;
    }

    private void FindPath()
    {
        lastPathTime -= Time.deltaTime;
        if (lastPathTime <= 0)
        {
            if (target && pathfinder.IsDone()) pathfinder.StartPath(transform.position, target.position, OnPathComplete);
        }
    }
    private void OnPathComplete(Path p)
    {
        if (p.vectorPath != null && p.vectorPath.Count > 0)
        {
            destination = p.vectorPath[0];
            for(int i = 1; i < p.vectorPath.Count; i++)
            {
                if (Vector2.Distance(p.vectorPath[i], transform.position) <= optimumDistance) destination = p.vectorPath[i];
            }
        }
        lastPathTime = pathGenTime;
    }

    private void PassInput()
    {
        CastRays();
        RotationLogic();
        ThrustLogic();
    }
    private void CastRays()
    {
        inDanger = false;

        Vector2 expectedTrajectory = vehicle.velocity * dangerRayTimeFrame;
        riskCheckRadius = expectedTrajectory.magnitude;

        hits = Physics2D.RaycastAll(transform.position, vehicle.velocity.normalized, riskCheckRadius, (1 << LayerMask.NameToLayer("World")) | (1 << LayerMask.NameToLayer("Vehicle")));

        float closestDanger = riskCheckRadius;
        foreach(RaycastHit2D hit in hits)
        {
            if (!hit.transform.IsChildOf(transform))
            {
                float dangerDistance = Vector2.Distance(hit.point, transform.position);
                VehicleController otherShip = hit.transform.GetComponent<VehicleController>();
                if (otherShip)
                {
                    Vector2 otherShipTrajectory = otherShip.velocity * dangerRayTimeFrame;
                    //dangerDistance = (expectedTrajectory + Vector2.Dot(expectedTrajectory.normalized, otherShipTrajectory.normalized) * otherShipTrajectory).magnitude;
                    //if (dangerDistance < 0) dangerDistance = 0;
                    dangerDistance = Vector2.Distance((Vector2)otherShip.transform.position + otherShipTrajectory, vehicle.transform.position);
                    //Debug.Log(otherShip.name + " is in front of " + name);
                    //dangerDistance = Vector2.Distance((Vector2)otherShip.transform.position + otherShip.velocity * dangerRayTimeFrame, transform.position);
                    if (dangerDistance > riskCheckRadius) continue;
                }
                if (dangerDistance < closestDanger)
                {
                    inDanger = true;
                    imminentDanger = hit;
                    closestDanger = dangerDistance;
                }
            }
        }
    }
    private void RotationLogic()
    {
        float rotationAngle = vehicle.inputLeftStick.x;

        if (inDanger)
        {
            VehicleController otherShip = imminentDanger.transform.GetComponent<VehicleController>();

            if (otherShip)
            {
                Vector2 otherShipTrajectory = otherShip.velocity * dangerRayTimeFrame;
                float trajectoryAngle = -VectorHelpers.AngleSigned(otherShip.transform.position - vehicle.transform.position, (Vector2)otherShip.transform.position + otherShipTrajectory - (Vector2)vehicle.transform.position, transform.up);
                rotationAngle = trajectoryAngle + (trajectoryAngle > 0 ? 5 : -5);
                //Debug.Log("Trajectory Angle: " + trajectoryAngle);
            }
            else
            {
                float percentVelocityInDanger = Vector2.Dot(vehicle.velocity.normalized, imminentDanger.normal);
                percentVelocityInDanger = Mathf.Clamp(percentVelocityInDanger, 0.6f, 1);

                Vector2 reflection = Vector2.Reflect((imminentDanger.point - (Vector2)transform.position), imminentDanger.normal).normalized;
                dangerAvoidanceVector = Vector2.Lerp(reflection, imminentDanger.normal, percentVelocityInDanger);

                rotationAngle = VectorHelpers.AngleSigned(transform.forward, dangerAvoidanceVector, transform.up);
            }
        }
        else
        {
            rotationAngle = VectorHelpers.AngleSigned(transform.forward, (destination - transform.position).normalized, transform.up);
        }

        if ((inDanger && Mathf.Abs(rotationAngle) <= minDangerAngle) || (!inDanger && Mathf.Abs(rotationAngle) <= minTargetAngle)) rotationAngle = 0;

        //float maxRotationAngle = 360 * vehicle.revolutionsPerSecond * Time.fixedDeltaTime;
        float maxRotationAngle = 360 * vehicle.cappedRotSpeed * Time.fixedDeltaTime;
        float steerPercent = Mathf.Clamp(rotationAngle / maxRotationAngle, -1, 1);

        if (steerPercent != 0) { vehicle.inputLeftStick = new Vector2(Mathf.Lerp(vehicle.inputLeftStick.x, steerPercent, Time.deltaTime * steerLerp), 0); steering = true; }
        else if (steering) { vehicle.inputLeftStick = Vector2.zero; steering = false; }
    }
    private void ThrustLogic()
    {
        float thrust = 1;

        /*float immediateDistance = vehicle.acceleration * dangerRayTimeFrame * dangerRayTimeFrame;
        RaycastHit2D worldHit = Physics2D.Raycast(transform.position, transform.forward, immediateDistance, (1 << LayerMask.NameToLayer("World")));
        if(worldHit)
        {
            thrust = Mathf.Clamp01(Vector2.Distance(worldHit.point, transform.position) / immediateDistance);
        }*/
        /*if (inDanger)
        {
            Vector2 dangerDisplacement = imminentDanger.point - (Vector2)transform.position;
            float percentFacingDanger = Vector2.Dot(transform.forward, dangerDisplacement.normalized);
            if (percentFacingDanger < 0) percentFacingDanger = 0;
            thrust = dangerDisplacement.magnitude / riskCheckRadius * (1 - percentFacingDanger);
        }*/
        if(inDanger)
        {
            Vector2 dangerDisplacement = imminentDanger.point - (Vector2)transform.position;
            thrust = Mathf.Clamp01(dangerDisplacement.magnitude / riskCheckRadius);
        }

        vehicle.inputThruster = Mathf.Lerp(vehicle.inputThruster, thrust, Time.deltaTime * thrusterLerp);
    }
}
