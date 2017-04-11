using UnityEngine;
using UnityEngine.UI;

public class VehicleStatsController : MonoBehaviour
{
    public Slider healthbar;
    public Image speedometer, powerup1, powerup2, powerup3;

    public Transform target;
    public float distance;
	
	void Update ()
    {
        FollowTarget();
	}

    private void FollowTarget()
    {
        transform.position = target.position + target.up * distance;
    }
}
