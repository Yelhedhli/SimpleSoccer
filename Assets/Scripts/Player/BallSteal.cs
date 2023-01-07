using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSteal : MonoBehaviour
{
    private Ball ball;
    private PlayerManager playerManager;
    private PlayerController playerController;

    void Awake()
    {
        playerManager = GetComponentInParent<PlayerManager>();
        playerController = this.transform.parent.gameObject.GetComponentInChildren<PlayerController>();
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

    private void OnTriggerStay(Collider other){
        if(ball.Steal(playerController)){
            playerManager.Steal(playerController);
        }
    }
}
