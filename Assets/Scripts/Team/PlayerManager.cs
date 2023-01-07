using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;

    public PlayerController[] teamPlayers;
    public PlayerController activePlayer;

    private Vector3 capsuleStart;
    private Vector3 capsuleEnd;
    
    [SerializeField]
    private int capsuleRadius;

    [SerializeField]
    private int passSelectorLength;

    [SerializeField]
    private LayerMask playerLayerMask;

    private Ball ball;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerManager.instance == null)
        {
            PlayerManager.instance = this;
        }

        ball = Ball.instance;
        teamPlayers = GetComponentsInChildren<PlayerController>();
        activePlayer = teamPlayers[0];
        activePlayer.SetPlayerControlled();
        foreach(PlayerController p in teamPlayers){
            p.SwitchToOffense();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("SwitchPlayer")){
            PlayerController[] targetPlayers = GetTargets();
            if(targetPlayers.Length != 0){
                print(targetPlayers[0]);
                SwitchPlayer(targetPlayers[0]);
                ball.PassTo(activePlayer);
            }
        }
    }

    void OnDrawGizmos(){
        if (activePlayer != null){
            Vector3 movementVector = GetMovementVector();
            capsuleStart = CalculateCapsuleStart(movementVector);
            capsuleEnd = CalculateCapsuleEnd(movementVector);
            GizmoHelper.DrawWireCapsule(capsuleStart, capsuleEnd, capsuleRadius);
        }
    }

    void SwitchPlayer(PlayerController target){
        activePlayer.SetAIControlled();
        activePlayer = target;
        activePlayer.SetPlayerControlled();
    }

    PlayerController[] GetTargets(){
        List<PlayerController> targetList = new List<PlayerController>(); 

        Vector3 movementVector = GetMovementVector();

        if(movementVector == Vector3.zero){
            return targetList.ToArray();
        }

        capsuleStart = CalculateCapsuleStart(movementVector);
        capsuleEnd = CalculateCapsuleEnd(movementVector); 
        Collider[] found = Physics.OverlapCapsule(capsuleStart, capsuleEnd, capsuleRadius, playerLayerMask);

        foreach(Collider c in found){
            PlayerController target = c.transform.parent.gameObject.GetComponentInChildren<PlayerController>();
            if(target != activePlayer){
                targetList.Add(target);
            }
        }

        return targetList.ToArray();
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

    Vector3 CalculateCapsuleStart(Vector3 movementVector){
        return activePlayer.transform.position + movementVector * capsuleRadius;
    }

    Vector3 CalculateCapsuleEnd(Vector3 movementVector){
        return activePlayer.transform.position + movementVector * passSelectorLength;
    }

    public void Steal(PlayerController stealer){
        SwitchPlayer(stealer);
        foreach(PlayerController p in teamPlayers){
            p.SwitchToOffense();
        }
    }

    public void Stolen(){
        foreach(PlayerController p in teamPlayers){
            p.SwitchToDefense();
        }
    }
}
