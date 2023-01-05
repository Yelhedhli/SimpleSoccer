using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerManager : MonoBehaviour
{
    public PlayerController[] teamPlayers;
    public PlayerController activePlayer;

    private Vector3 capsuleStart;
    private Vector3 capsuleEnd;
    
    [SerializeField]
    private int capsuleRadius;

    [SerializeField]
    private int passSelectorLength;

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
            PlayerController[] targetPlayers = GetTargets();
            if(targetPlayers.Length != 0){
                print(targetPlayers[0]);
                SwitchPlayer(targetPlayers[0]);
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
        Collider[] found = Physics.OverlapCapsule(capsuleStart, capsuleEnd, capsuleRadius, 1);

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

}
