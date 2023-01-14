using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private GameObject ball;
    // Start is called before the first frame update
    void Awake()
    {
        ball = GameObject.FindGameObjectWithTag("Ball");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = new Vector3(ball.transform.position.x, transform.position.y, transform.position.z);
    }
}
