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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if(brainState == BrainState.Player){
            //  take user input
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            // calc movement vector
            Vector3 movementDirection = new Vector3(h, 0, v);
            movementDirection.Normalize();

            // move in desired direction
            transform.Translate(movementDirection*speed*Time.deltaTime, Space.World); 

            // rotate character to face movement direction
            if(movementDirection != Vector3.zero){
                Quaternion toRotation  = Quaternion.LookRotation(movementDirection, Vector3.up);

                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed*Time.deltaTime);
            }
        }

    }

    public void SetPlayerControlled(){
        brainState = BrainState.Player;
    }

    public void SetAIControlled(){
        brainState = BrainState.Offense;
    }
}