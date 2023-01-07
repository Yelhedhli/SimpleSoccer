using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerController : MonoBehaviour
{
    public enum BrainState{Player, Offense, Defense}

    public BrainState brainState; 

    [SerializeField] 
    private float speed = 10;

    [SerializeField] 
    private float rotationSpeed = 720;

    [SerializeField]
    public GameObject dribblePos; //position for where ball should go if this player has possession

    public bool hasBall = false;

    private Ball ball; // leave this logic in for eventual implementation of shooting

    // Start is called before the first frame update
    void Start()
    {
        ball = Ball.instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if(brainState == BrainState.Player){
            // get movement vector
            Vector3 movementDirection = GetMovementVector();
            
            // move in desired direction
            transform.Translate(movementDirection * speed * Time.deltaTime, Space.World); 

            // rotate character to face movement direction
            if(movementDirection != Vector3.zero){
                Quaternion toRotation  = Quaternion.LookRotation(movementDirection, Vector3.up);

                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    public void SetPlayerControlled(){
        brainState = BrainState.Player;
    }

    public void SetAIControlled(){
        brainState = BrainState.Offense;
    }

    Vector3 GetMovementVector(){
        //  take user input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // calc movement vector
        Vector3 movementVector = new Vector3(h, 0, v);
        movementVector.Normalize();

        return movementVector;
    }
}
