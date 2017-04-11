using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public Transform[] targets;
    public float minDistance, maxDistance, cushionDistance;
    public float lerp = 8f;

    public float maxVerticalAngle = 45, maxHorizontalAngle = 45;
    [Range(-1, 1)]
    public float inputVertical, inputHorizontal;

    public Slider healthBar;
    public Text speedFeed, resultText;
    public Button leftDragArea, rightDragArea;
    public Image powerupImage1, powerupImage2, powerupImage3;

    private Camera cameraComponent;

	void Start ()
    {
        cameraComponent = GetComponent<Camera>();
    }
	
	void FixedUpdate ()
    {
        if (targets != null)
        {
            Vector3 cameraPosition = Vector3.zero;
            float orthoSize = 0;
            Transform lookTarget = null;

            if (targets.Length == 1)
            {
                VehicleController targetBody = targets[0].gameObject.GetComponent<VehicleController>();
                float distancePercent = 0;

                //if (targetBody) distancePercent = (targetBody.velocity.magnitude + minDistance) / maxDistance;
                if (targetBody) distancePercent = targetBody.velocity.magnitude / targetBody.cappedTopSpeed * VehicleController.ME2KI;
                if (distancePercent > 1) distancePercent = 1;

                Vector3 direction = -Vector3.forward;
                direction = direction.Rotate(Vector3.right, inputVertical * maxVerticalAngle);
                direction = direction.Rotate(Vector3.up, inputHorizontal * maxHorizontalAngle);

                float cameraDistance = ((maxDistance - minDistance) * distancePercent) + minDistance;
                cameraPosition = targets[0].position + direction * cameraDistance;
                orthoSize = cameraDistance / 2;
                lookTarget = targets[0];
                //transform.position = Vector3.Lerp(transform.position, targets[0].position + direction * cameraDistance, Time.deltaTime * singleTargetLerp);
                //cameraComponent.orthographicSize = Mathf.Lerp(cameraComponent.orthographicSize, cameraDistance / 2, Time.deltaTime * singleTargetLerp);

                //transform.LookAt(targets[0]);
            }
            else if(targets.Length > 0)
            {
                bool addedInitial = false;
                Vector3 minBounds = Vector3.zero, maxBounds = Vector3.zero;
                int liveTargets = 0;
                for (int i = 0; i < targets.Length; i++)
                {
                    Transform target = targets[i];
                    VehicleController vehicle = target.GetComponent<VehicleController>();
                    if (vehicle && vehicle.health > 0)
                    {
                        if (addedInitial)
                        {
                            if (target.position.x < minBounds.x) minBounds.x = target.position.x;
                            if (target.position.x > maxBounds.x) maxBounds.x = target.position.x;
                            if (target.position.y < minBounds.y) minBounds.y = target.position.y;
                            if (target.position.y > maxBounds.y) maxBounds.y = target.position.y;
                        }
                        else
                        {
                            minBounds.x = target.position.x;
                            maxBounds.x = target.position.x;
                            minBounds.y = target.position.y;
                            maxBounds.y = target.position.y;

                            addedInitial = true;
                        }
                        liveTargets++;
                    }
                }
                Vector3 center = (maxBounds + minBounds) / 2;
                float boundsWidth = maxBounds.x - minBounds.x + cushionDistance, boundsHeight = maxBounds.y - minBounds.y + cushionDistance;
                float heightAspectedFromWidth = boundsWidth / cameraComponent.aspect;

                float cameraDistance = Mathf.Max(boundsHeight, heightAspectedFromWidth) / 2 / Mathf.Tan((cameraComponent.fieldOfView / 2) * Mathf.Deg2Rad);
                if (cameraDistance < minDistance) cameraDistance = minDistance;

                cameraPosition = center - (Vector3.forward * cameraDistance);
                orthoSize = cameraDistance;
                //transform.position = Vector3.Lerp(transform.position, center - (Vector3.forward * cameraDistance), Time.deltaTime * multiTargetLerp);
                //cameraComponent.orthographicSize = Mathf.Lerp(cameraComponent.orthographicSize, cameraDistance, Time.deltaTime * singleTargetLerp);
                //transform.forward = Vector3.forward;
            }

            transform.position = Vector3.Lerp(transform.position, cameraPosition, Time.deltaTime * lerp);
            cameraComponent.orthographicSize = Mathf.Lerp(cameraComponent.orthographicSize, orthoSize, Time.deltaTime * lerp);
            if (lookTarget) transform.LookAt(lookTarget);
            else transform.forward = Vector3.forward;
        }
	}
    public void RotateCamera(Vector3 euler)
    {
        transform.rotation = ((Quaternion.AngleAxis(euler.y, Vector3.up) * transform.rotation) * Quaternion.AngleAxis(euler.x, Vector3.right)) * Quaternion.AngleAxis(euler.z, Vector3.forward);
    }
}
