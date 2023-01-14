using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class PlayerManager : MonoBehaviour
{
    private Ball ball;
    
    public bool teamInPoss = false;

    public PlayerController[] teamPlayers;
    public PlayerController activePlayer;

    public PlayerManager otherTeam;

    private Vector3 capsuleStart;
    private Vector3 capsuleEnd; 
    [SerializeField]
    private int capsuleRadius;
    [SerializeField]
    private int passSelectorLength;
    [SerializeField]
    private LayerMask playerLayerMask;
    [SerializeField]
    private float passStrengthModifier;
    private float passStrength;

    [SerializeField]
    private string switchPlayerInput;
    [SerializeField]
    private string xInput;
    [SerializeField]
    private string yInput;

    // Start is called before the first frame update
    void Start()
    {
        ball = Ball.instance;
        teamPlayers = GetComponentsInChildren<PlayerController>();
        activePlayer = teamPlayers[0];
        activePlayer.SetPlayerControlled();
        foreach(PlayerController p in teamPlayers){
            p.SwitchToDefense();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButton(switchPlayerInput)){
            passStrength += Time.deltaTime*passStrengthModifier;
        }
        if(Input.GetButtonUp(switchPlayerInput)){
            passStrength = Mathf.Clamp(passStrength, 0, 1);
            if(teamInPoss && ball.playerInPoss != null){
                Pass();
            }else if(!teamInPoss){
                SwitchDefender();
            }
            print("passStrength : " + passStrength);
            passStrength = 0;
        }
    }

    void OnDrawGizmos(){
        if (activePlayer != null){
            Vector3 movementVector = GetMovementVector();
            Vector3 orthogonalVector = Vector3.Cross(movementVector, Vector3.up);
            capsuleStart = CalculateCapsuleStart(movementVector);
            capsuleEnd = CalculateCapsuleEnd(movementVector);

            // to visualize pass target point
            Gizmos.DrawWireSphere(activePlayer.transform.position + movementVector*passStrength*passSelectorLength , 1);

            // to visualize first stage of progressive target sweep
            // GizmoHelper.DrawWireCapsule(capsuleStart, capsuleEnd, capsuleRadius);

            // to visualize second stage colliders for progressive target sweeping
            // GizmoHelper.DrawWireCapsule(capsuleStart, capsuleEnd+orthogonalVector*capsuleRadius*3/2, capsuleRadius*2/3); 
            // GizmoHelper.DrawWireCapsule(capsuleStart, capsuleEnd-orthogonalVector*capsuleRadius*3/2, capsuleRadius*2/3);
        }
    }

    void SwitchPlayer(PlayerController target){
        activePlayer.SetAIControlled();
        activePlayer = target;
        activePlayer.SetPlayerControlled();
    }

    PlayerController[] GetTargets(Vector3 movementVector){
        List<PlayerController> targetList = new List<PlayerController>(); 

        if(movementVector == Vector3.zero){
            return targetList.ToArray();
        }

        capsuleStart = CalculateCapsuleStart(movementVector);
        capsuleEnd = CalculateCapsuleEnd(movementVector); 
        Collider[] found = Physics.OverlapCapsule(capsuleStart, capsuleEnd, capsuleRadius, playerLayerMask);

        foreach(Collider c in found){
            PlayerController target = c.transform.parent.gameObject.GetComponentInChildren<PlayerController>();
            if(target != activePlayer && !targetList.Contains(target)){
                targetList.Add(target);
            }
        }

        if(targetList.Count == 0){
            Vector3 orthogonalVector = Vector3.Cross(movementVector, Vector3.up);

            found = Physics.OverlapCapsule(capsuleStart, capsuleEnd+orthogonalVector*capsuleRadius*3/2, capsuleRadius*2/3, playerLayerMask);
            Collider[] found2 = Physics.OverlapCapsule(capsuleStart, capsuleEnd-orthogonalVector*capsuleRadius*3/2, capsuleRadius*2/3, playerLayerMask);

            foreach(Collider c in found){
                PlayerController target = c.transform.parent.gameObject.GetComponentInChildren<PlayerController>();
                if(target != activePlayer && !targetList.Contains(target)){
                    targetList.Add(target);
                }
            }

            foreach(Collider c in found2){
                PlayerController target = c.transform.parent.gameObject.GetComponentInChildren<PlayerController>();
                if(target != activePlayer && !targetList.Contains(target)){
                    targetList.Add(target);
                }
            }
        }

        return targetList.ToArray();
    }

    Vector3 GetMovementVector(){
        //  take user input
        float h = Input.GetAxisRaw(xInput);
        float v = Input.GetAxisRaw(yInput);

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
        otherTeam.Stolen();
        teamInPoss = true;
        SwitchPlayer(stealer);
        foreach(PlayerController p in teamPlayers){
            p.SwitchToOffense();
        }
    }

    public void Stolen(){
        teamInPoss = false;
        foreach(PlayerController p in teamPlayers){
            p.SwitchToDefense();
        }
    }

    private void Pass(){
        Vector3 movementVector = GetMovementVector();
        PlayerController[] targetPlayers = GetTargets(movementVector);
        if(targetPlayers.Length != 0){
            //PlayerController target = GetCenterest(targetPlayers, movementVector);
            PlayerController target = GetClosest(targetPlayers, movementVector);
            SwitchPlayer(target);
            ball.PassTo(activePlayer, passStrength);
        }
    }

    private void SwitchDefender(){
        List<PlayerController> targetList = new List<PlayerController>(teamPlayers.OrderBy(p => Vector3.Distance(ball.transform.position, p.transform.position)));
        targetList.Remove(activePlayer);
        SwitchPlayer(targetList[0]);
    }

    private PlayerController GetCenterest(PlayerController[] targetPlayers, Vector3 movementVector){ // get player closest to center of player selector
        float minDistance = LinAlgHelper.DistancePointLine(targetPlayers[0].transform.position, CalculateCapsuleStart(movementVector), CalculateCapsuleEnd(movementVector));
        PlayerController target = targetPlayers[0];
        foreach(PlayerController p in targetPlayers){
            float distance = LinAlgHelper.DistancePointLine(p.transform.position, CalculateCapsuleStart(movementVector), CalculateCapsuleEnd(movementVector));
            if(distance < minDistance){
                minDistance = distance;
                target = p;
            }
        }

        return target;
    }

    private PlayerController GetClosest(PlayerController[] targetPlayers, Vector3 movementVector){ // get player closest to pass target point
        Vector3 targetPos = activePlayer.transform.position + movementVector*passStrength*passSelectorLength;
        float minDistance = Vector3.Distance(targetPlayers[0].transform.position, targetPos);
        PlayerController target = targetPlayers[0];
        foreach(PlayerController p in targetPlayers){
            float distance = Vector3.Distance(p.transform.position, targetPos);
            if(distance < minDistance){
                minDistance = distance;
                target = p;
            }
        }

        return target;
    }
}
