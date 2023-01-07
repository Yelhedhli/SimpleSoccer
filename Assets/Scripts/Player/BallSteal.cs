using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSteal : MonoBehaviour
{
    private Ball ball;
    private PlayerManager playerManager;
    private PlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {
        playerController = this.transform.parent.gameObject.GetComponentInChildren<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other){ 
        ball = Ball.instance;
        playerManager = PlayerManager.instance;
        if(ball.Steal(playerController)){
            playerManager.Steal(playerController);
        }
    }
}
