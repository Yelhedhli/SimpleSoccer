using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerManager : MonoBehaviour
{
    public PlayerController[] teamPlayers;
    public PlayerController activePlayer;
    
    private int index = 0;

    private Vector3 capsuleStart;
    private Vector3 capsuleEnd;
    
    [SerializeField]
    private int capsuleRadius;

    // Start is called before the first frame update
    void Start()
    {
        teamPlayers = GetComponentsInChildren<PlayerController>();
        activePlayer = teamPlayers[0];
        teamPlayers[0].SetPlayerControlled();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("SwitchPlayer")){
            Collider[] targetPlayers = GetTargets();
            foreach(Collider c in targetPlayers){
                print(c.transform.parent);
            }
            index = (index+1)%3;
            SwitchPlayer(teamPlayers[index]);
        }
    }

    void OnDrawGizmos(){
        if (activePlayer != null){
            Vector3 movementVector = GetMovementVector();
            capsuleStart = activePlayer.transform.position;
            capsuleEnd = activePlayer.transform.position + movementVector*10;
            GizmoHelper.DrawWireCapsule(capsuleStart, capsuleEnd, capsuleRadius);
        }
    }

    void SwitchPlayer(PlayerController target){
        activePlayer.SetAIControlled();
        activePlayer = target;
        activePlayer.SetPlayerControlled();
    }

    Collider[] GetTargets(){
        Vector3 movementVector = GetMovementVector();

        if(movementVector == Vector3.zero){
            Collider[] collider = new Collider[0];
            return collider;
        }

        capsuleStart = activePlayer.transform.position;
        capsuleEnd = activePlayer.transform.position + movementVector*10;
        return Physics.OverlapCapsule(capsuleStart, capsuleEnd, capsuleRadius);
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
}
