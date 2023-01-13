using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Ball : MonoBehaviour
{
    public PlayerController playerInPoss;

    private Net enemyNet;
    
    // this lets us access from any other script via Ball.instance
    public static Ball instance;
    // this lets us access ball's rigidbody from other scripts
    private Rigidbody rb;

    private Vector3 targetPos;
    [SerializeField]
    private int ballSpeed;
    [SerializeField]
    private int maxPassVelocity;
    [SerializeField]
    private int maxShotVelocity;

    private enum BallState{Idle, Dribbling, Passing, Shooting};
    private BallState ballState;
    
    void Awake()
    {
        if (Ball.instance == null)
        {
            Ball.instance = this;
        }

        enemyNet = FindObjectOfType<Net>();
    }

    // Start is called before the first frame update
    void Start()
    {
        targetPos = Vector3.zero;
        ballState = BallState.Idle;
        rb = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch(ballState){            
            case BallState.Dribbling:
                Dribble();
                break;
        }
    }

    private void Dribble(){
        targetPos = playerInPoss.dribblePos.transform.position;
        this.transform.position = Vector3.MoveTowards(this.transform.position, targetPos, ballSpeed * Time.deltaTime);
    }

    public void PassTo(PlayerController target, float passStrength){
        Vector3 forceDirection = target.dribblePos.transform.position - this.transform.position;
        forceDirection.Normalize();
        playerInPoss = null;
        ballState = BallState.Passing;
        rb.AddForce(forceDirection*maxPassVelocity*passStrength, ForceMode.VelocityChange);
        target.RecievePass();
    }

    public bool Steal(PlayerController stealer){
        if(playerInPoss == null){
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            playerInPoss = stealer;
            ballState = BallState.Dribbling;
            return true;
        }
        return false;
    }

    public void ShootBall(float shotStrength){
        Vector3 forceDirection = enemyNet.transform.position - this.transform.position;
        forceDirection.Normalize();
        playerInPoss = null;
        ballState = BallState.Shooting;
        rb.AddForce(forceDirection*maxShotVelocity*shotStrength, ForceMode.VelocityChange);
    }
}
