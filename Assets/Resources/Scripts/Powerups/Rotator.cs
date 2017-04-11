using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour
{
    public float speed;
    private Quaternion randomRotation;

	void Start ()
    {
        randomRotation = Random.rotation;
	}
	
	void Update ()
    {
        AddRotation();
	}

    private void AddRotation()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, transform.rotation * randomRotation, Time.deltaTime * speed);
    }
}
