using UnityEngine;
using System.Collections;

public class CameraRotate : MonoBehaviour {

    public GameObject target;

    public float distance = 500.0f;

    public int cameraSpeed = 8;
    public float zoomSpeed = 10.0f;

    int yMinRotation = -40;
    int yMaxRotation = 60;

    int minDistance = 100;
    int maxDistance = 1000;

    float x = 0.0f;
    float y = 0.0f;

    float vx = 0.0f;
    float vy = 0.0f;

    float targetDistance;

    void Start () {

        this.gameObject.GetComponent<Transform>().position = target.transform.position + new Vector3(0, -distance*.1f, -distance);
        this.gameObject.GetComponent<Transform>().LookAt(target.transform.position);
        
        var angles = transform.eulerAngles;
        x = angles.x;
        y = angles.y;

        // starting animation
        vx = 8;
        vy = 8;
        targetDistance = maxDistance*.9f;

    }
	
	void Update () {

        // HANDLING DISTANCE IN AND OUT
        if ( Input.GetKey(KeyCode.UpArrow))
        {
            targetDistance -= zoomSpeed;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            targetDistance += zoomSpeed;
        }

        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

        distance += (targetDistance - distance) * .2f;

        // ROTATION
        if ( Input.GetMouseButton(0) && ( Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.LeftAlt)) )
        {
            vx += Input.GetAxis("Mouse X");
            vy -= Input.GetAxis("Mouse Y");
        }

        x += vx;
        y += vy;
        vx *= .86f;
        vy *= .86f;

        y = ClampAngle(y, yMinRotation, yMaxRotation);

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        var position = rotation * new Vector3(0.0f, 0.0f, -distance) + target.GetComponent<Transform>().position;

        transform.position = position;
        transform.rotation = rotation;
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;

        return Mathf.Clamp(angle, min, max);

    }
}

