using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Ball : MonoBehaviour
{
    //Ball will constantly go to the dribble position of the player in possession
    public PlayerController playerInPoss;
    
    private Vector3 targetPos;

    [SerializeField]
    private int ballSpeed;

    private enum BallState{Idle, Dribbling, Passing};

    private BallState ballState;

    // Start is called before the first frame update
    void Start()
    {
        targetPos = Vector3.zero;
        ballState = BallState.Dribbling;
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

         this.transform.position = Vector3.MoveTowards(this.transform.position, targetPos, ballSpeed * Time.deltaTime);
    }

    private void Pass(){
        print("Pass");
    }

    private void Dribble(){
        targetPos = playerInPoss.dribblePos.transform.position;
    }

    public void PassTo(PlayerController target){
        playerInPoss = target;
    }
}
