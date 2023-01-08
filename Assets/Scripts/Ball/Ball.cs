using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Ball : MonoBehaviour
{
    // Ball will constantly go to the dribble position of the player in possession
    public PlayerController playerInPoss;
    
    // this lets us access from any other script via Ball.instance
    public static Ball instance;
    // this lets us access ball's rigidbody from other scripts
    private Rigidbody rb;

    private Vector3 targetPos;
    [SerializeField]
    private int ballSpeed;
    private float passStrength;

    private enum BallState{Idle, Dribbling, Passing};
    private BallState ballState;
    
    void Awake()
    {
        if (Ball.instance == null)
        {
            Ball.instance = this;
        }
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
            case BallState.Idle:
                break;
            
            case BallState.Dribbling:
                Dribble();
                break;

            case BallState.Passing:
                Pass();
                break;
        }
    }

    private void Pass(){
        this.transform.position = Vector3.MoveTowards(this.transform.position, targetPos, ballSpeed * Time.deltaTime * passStrength);
    }

    private void Dribble(){
        targetPos = playerInPoss.dribblePos.transform.position;
        this.transform.position = Vector3.MoveTowards(this.transform.position, targetPos, ballSpeed * Time.deltaTime);
    }

    public void PassTo(PlayerController target, float localPassStrength){
        passStrength = localPassStrength;
        playerInPoss = null;
        ballState = BallState.Passing;
        targetPos = target.dribblePos.transform.position;
        target.RecievePass();
    }

    public bool Steal(PlayerController stealer){
        if(playerInPoss == null){
            playerInPoss = stealer;
            ballState = BallState.Dribbling;
            return true;
        }
        return false;
    }
}
