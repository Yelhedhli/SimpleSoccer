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

    private Vector3 capsuleStart;
    private Vector3 capsuleEnd; 
    [SerializeField]
    private int capsuleRadius;
    [SerializeField]
    private int passSelectorLength;
    [SerializeField]
    private LayerMask playerLayerMask;

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
        if(Input.GetButtonDown("SwitchPlayer")){
            if(teamInPoss && ball.playerInPoss != null){
                Pass();
            }else if(!teamInPoss){
                SwitchDefender();
            }
        }
    }

    void OnDrawGizmos(){
        if (activePlayer != null){
            Vector3 movementVector = GetMovementVector();
            Vector3 orthogonalVector = Vector3.Cross(movementVector, Vector3.up);
            capsuleStart = CalculateCapsuleStart(movementVector);
            capsuleEnd = CalculateCapsuleEnd(movementVector);
            GizmoHelper.DrawWireCapsule(capsuleStart, capsuleEnd, capsuleRadius);

            // to visualize colliders for progressive target sweeping
            //GizmoHelper.DrawWireCapsule(capsuleStart, capsuleEnd+orthogonalVector*capsuleRadius*3/2, capsuleRadius*2/3); 
            //GizmoHelper.DrawWireCapsule(capsuleStart, capsuleEnd-orthogonalVector*capsuleRadius*3/2, capsuleRadius*2/3);
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
        PlayerController[] targetPlayers = GetTargets();
        foreach(PlayerController p in targetPlayers){
            print(p);
        }
        if(targetPlayers.Length != 0){
            SwitchPlayer(targetPlayers[0]);
            ball.PassTo(activePlayer);
        }
    }

    private void SwitchDefender(){
        List<PlayerController> targetList = new List<PlayerController>(teamPlayers.OrderBy(p => Vector3.Distance(ball.transform.position, p.transform.position)));
        targetList.Remove(activePlayer);
        SwitchPlayer(targetList[0]);
    }
}
