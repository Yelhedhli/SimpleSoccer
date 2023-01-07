using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerController : MonoBehaviour
{
    private Ball ball; // leave this logic in for eventual implementation of shooting
    private Collider ballStealCollider;
    private PlayerManager playerManager;
    
    private enum BrainState{Player, Offense, Defense, RecievePass}
    private BrainState brainState; // dictates whether to use player or AI input (and which AI to use) 

    [SerializeField] 
    private float speed = 10;
    [SerializeField] 
    private float rotationSpeed = 720;
    [SerializeField]
    public GameObject dribblePos; //position for where ball should go if this player has possession

    private bool playerControlled = false;

    void Awake()
    {
        ballStealCollider = GetComponentInChildren<BallSteal>().GetComponentInChildren<Collider>();
        playerManager = GetComponentInParent<PlayerManager>();
    }

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

    Vector3 GetMovementVector(){
        // take user input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // calc movement vector
        Vector3 movementVector = new Vector3(h, 0, v);
        movementVector.Normalize();

        return movementVector;
    }

    public void SetPlayerControlled(){
        playerControlled = true;
        brainState = BrainState.Player;
    }

    public void SetAIControlled(){
        playerControlled = false;
        if(playerManager.teamInPoss){
            brainState = BrainState.Offense;
        }else{
            brainState = BrainState.Defense;
        }
    }

    public void SwitchToOffense(){
        ballStealCollider.enabled = false;
        if(!playerControlled){
            SetAIControlled();
        }
    }

    public void SwitchToDefense(){
        ballStealCollider.enabled = true;
        if(!playerControlled){
            SetAIControlled();
        }
    }

    public void RecievePass(){
        ballStealCollider.enabled = true;
        brainState = BrainState.RecievePass;
    }
}
