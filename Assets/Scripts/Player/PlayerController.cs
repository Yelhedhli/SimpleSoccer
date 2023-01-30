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

    Vector3 GetMovementVectorORHeading(){
        return GetMovementVector() == Vector3.zero ? this.transform.forward : GetMovementVector();
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
        Vector3 aimIntersection = GetAimIntersection();
        Vector3 nominalShotTarget = GetNominalShotTarget(aimIntersection);

        // distance from the goal as a percentage of max distance from net
        float nominalShotPower = Mathf.Clamp(Vector3.Distance(this.transform.position, playerManager.opponentNet.transform.position)/maxShotDistance, 0.3f, 1); // this is clamped at 0.3 so that you are never penalized for shot powers below 0.3
        
        // math is how far from the target the player is aiming as a percentage 
        float nominalAccuracy = 1 - Mathf.Clamp(GetAimDeviation(nominalShotTarget, aimIntersection)/maxAimDeviation, 0, 1);
        
        // how far above the nominal shot power a player's input is
        float powerErr = Mathf.Clamp(shotStrength-nominalShotPower, 0, 1);
        // 
        float powerPercentage = Mathf.Clamp(shotStrength/nominalShotPower, 0, 1);

        float shotAccuracy = Mathf.Clamp(nominalAccuracy - powerErr, 0, 1);
        print("shotAccuracy: " + shotAccuracy);

        // crude way to set a min shot strength
        shotStrength = Mathf.Clamp(shotStrength, 0.3f, 1);

        Vector3 inaccuracyVector = playerManager.opponentNet.transform.position - nominalShotTarget;
        inaccuracyVector = inaccuracyVector.normalized;

        Vector3 powerVector = playerManager.opponentNet.shotTargets["Corner TR"] - playerManager.opponentNet.shotTargets["Corner BR"];
        
        Vector3 realShotTarget = nominalShotTarget + inaccuracyVector*2*(1-shotAccuracy) + powerVector*powerPercentage;

        ball.ShootBall(shotStrength, realShotTarget);
    }

    Vector3 GetAimIntersection(){
        Vector3 intersection;
        Vector3 movementVector = GetMovementVectorORHeading();

        Vector3 planeNormalizedPlayer = this.transform.position;
        planeNormalizedPlayer.y = 0;

        Vector3 planeNormalizedNet = playerManager.opponentNet.transform.position;
        planeNormalizedNet.y = 0;

        if(!Math3d.LineLineIntersection(out intersection, planeNormalizedPlayer, movementVector, planeNormalizedNet, Vector3.back.normalized)){ // check intersection to one side of the net
            Math3d.LineLineIntersection(out intersection, planeNormalizedPlayer, movementVector, planeNormalizedNet, Vector3.forward.normalized); // check the other side of the net
        }

        return intersection == Vector3.zero ? Vector3.positiveInfinity : intersection;
    }

    Vector3 GetNominalShotTarget(Vector3 aimIntersection){
        if(Vector3.Distance(playerManager.opponentNet.shotTargets["Corner BR"], aimIntersection) < Vector3.Distance(playerManager.opponentNet.shotTargets["Corner BL"], aimIntersection)){
            return playerManager.opponentNet.shotTargets["Corner BR"];
        }

        return playerManager.opponentNet.shotTargets["Corner BL"];
    }

    float GetAimDeviation(Vector3 target, Vector3 aimIntersection){
        return Mathf.Clamp(Vector3.Distance(target, aimIntersection), 0, maxAimDeviation);
    }

    void SlideTackleInit(){      
        tackleCooldownTimer = 0.2;
        tackleTimer = 0.4;
        tackleDirection = GetMovementVectorORHeading();
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
        Vector3 lookVector = GetMovementVectorORHeading();

        if(Physics.SphereCast(this.transform.position, 1, lookVector, out hitInfo, 2, tackleLayerMask, QueryTriggerInteraction.Ignore)){
            if(hitInfo.collider.gameObject.tag == "Ball"){
                ball.Tackle(this);
                playerManager.Steal(this);
            }
        }
    }

}
