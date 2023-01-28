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
    
    private enum BrainState{Player, Offense, Defense, RecievePass, Tackle}
    [SerializeField] 
    private BrainState brainState; // dictates whether to use player or AI input (and which AI to use) 

    [SerializeField] 
    private float speed = 10;
    [SerializeField] 
    private float rotationSpeed = 720;
    [SerializeField]
    public GameObject dribblePos; //position for where ball should go if this player has possession

    private double tackleTimer = 0;
    private double tackleCooldownTimer = 0;
    private Vector3 tackleDirection;
    [SerializeField]
    private LayerMask tackleLayerMask;

    private bool playerControlled = false;
    
    [SerializeField]
    private float maxShotDistance;
    [SerializeField]
    private float maxAimDeviation;
    [SerializeField]
    private float shotStrengthModifier;
    private float shotStrength;

    [SerializeField]
    private string xInput;
    [SerializeField]
    private string yInput;
    [SerializeField]
    private string shootInput;
    [SerializeField]
    private string slideTackleInput;
    [SerializeField]
    private string standingTackleInput;

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
        switch(brainState){
            case BrainState.Player:
                if(playerManager.teamInPoss){
                    if(tackleCooldownTimer > 0){
                        tackleCooldownTimer -= Time.deltaTime;
                        return;
                    }

                    if(Input.GetButton(shootInput)){
                        shotStrength += Time.deltaTime*shotStrengthModifier;
                    }
                    if(Input.GetButtonUp(shootInput)){
                        shotStrength = Mathf.Clamp(shotStrength, 0, 1);
                        Shoot();
                        print("shotStrength : " + shotStrength);
                        shotStrength = 0;
                    }
                }else{
                    if(tackleCooldownTimer > 0){
                        tackleCooldownTimer -= Time.deltaTime;
                        return;
                    }

                    if(Input.GetButtonDown(slideTackleInput)){
                        SlideTackleInit(); 
                    }else if(Input.GetButtonDown(standingTackleInput)){
                        StandingTackle();
                    }
                }
                break;
        }
    }

    void FixedUpdate()
    {
        Quaternion toRotation;

        switch(brainState){
            case BrainState.Player:
                // get movement vector
                Vector3 movementDirection = GetMovementVector();
                
                // move in desired direction
                transform.Translate(movementDirection * speed * Time.deltaTime, Space.World); 

                // rotate character to face movement direction
                if(movementDirection != Vector3.zero){
                    toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);

                    transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
                }

                break;

            case BrainState.RecievePass:
                Vector3 lookVector = ball.transform.position - this.transform.position;
                lookVector.y = 0;
                lookVector.Normalize();

                transform.Translate(lookVector * speed * Time.deltaTime, Space.World); 

                toRotation = Quaternion.LookRotation(lookVector, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
                
                break;
            
            case BrainState.Tackle:
                tackleTimer -= Time.deltaTime;
                SlideTackle();
                break;
        }
    }

    Vector3 GetMovementVector(){
        // take user input
        float h = Input.GetAxisRaw(xInput);
        float v = Input.GetAxisRaw(yInput);

        // calc movement vector
        Vector3 movementVector = new Vector3(h, 0, v);
        movementVector.Normalize();

        return movementVector;
    }

    public void SetPlayerControlled(){
        playerControlled = true;
        brainState = BrainState.Tackle;
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
        }else{
            brainState = BrainState.Player;
        }
    }

    public void RecievePass(){
        ballStealCollider.enabled = true;
        brainState = BrainState.RecievePass;
    }

    void Shoot(){
        // distance from the goal as a percentage of max distance from net
        float distanceCoeff = Mathf.Clamp(Vector3.Distance(this.transform.position, playerManager.transform.position)/maxShotDistance, 0.3f, 1); // this is clamped at 0.3 so that you are never penalized for shot powers below 0.3
        
        // how far from the center of net player is aiming as a percentage 
        float deviationCoeff = Mathf.Clamp(GetAimDeviation()/maxAimDeviation, 0, 1);
        
        // how far above the nominal shot power a player's input is
        float powerErr = Mathf.Clamp(shotStrength-distanceCoeff, 0, 1);

        float shotAccuracy = Mathf.Clamp(1 - deviationCoeff - powerErr, 0, 1);
        print("shotAccuracy: " + shotAccuracy);

        // crude way to set a min shot strength
        shotStrength = Mathf.Clamp(shotStrength, 0.3f, 1);

        Vector3 target = playerManager.opponentNet.transform.position + Vector3.back*5*(1-shotAccuracy);

        ball.ShootBall(shotStrength, target);
    }

    float GetAimDeviation(){
        Vector3 intersection = Vector3.zero;
        Vector3 movementVector = GetMovementVector();

        Vector3 planeNormalizedPlayer = this.transform.position;
        planeNormalizedPlayer.y = 0;

        Vector3 planeNormalizedNet = playerManager.opponentNet.transform.position;
        planeNormalizedNet.y = 0;

        if(Math3d.LineLineIntersection(out intersection, planeNormalizedPlayer, movementVector, planeNormalizedNet, Vector3.back.normalized)){ // check intersection to one side of the net
            return Vector3.Distance(intersection, planeNormalizedNet);
        }else if(Math3d.LineLineIntersection(out intersection, planeNormalizedPlayer, movementVector, planeNormalizedNet, Vector3.forward.normalized)){ // check intersection to the other side of the net
            return Vector3.Distance(intersection, planeNormalizedNet);
        }else{ // player must not be facing the net
            return maxAimDeviation;
        }
    }

    void SlideTackleInit(){      
        tackleCooldownTimer = 0.2;
        tackleTimer = 0.4;
        tackleDirection = GetMovementVector() == Vector3.zero ? this.transform.forward : GetMovementVector();
        brainState = BrainState.Tackle;
    }

    void SlideTackle(){      
        transform.Translate(tackleDirection * (speed * 1.5f) * Time.deltaTime, Space.World);  
        GetTackleResult();
        if(tackleTimer <= 0){
            brainState = BrainState.Player;
        }
    }

    void StandingTackle(){
        tackleCooldownTimer = 0.1;
        GetTackleResult();
    }

    void GetTackleResult(){
        RaycastHit hitInfo;
        Vector3 lookVector = GetMovementVector() == Vector3.zero ? this.transform.forward : GetMovementVector();

        if(Physics.SphereCast(this.transform.position, 1, lookVector, out hitInfo, 2, tackleLayerMask, QueryTriggerInteraction.Ignore)){
            if(hitInfo.collider.gameObject.tag == "Ball"){
                ball.Tackle(this);
                playerManager.Steal(this);
            }
        }
    }

}
