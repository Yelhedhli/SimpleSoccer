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
    private int capsuleRadius = 5;

    // Start is called before the first frame update
    void Start()
    {
        teamPlayers = GetComponentsInChildren<PlayerController>();
        activePlayer = teamPlayers[0];
        teamPlayers[0].SetPlayerControlled();

        capsuleStart = activePlayer.transform.position;
        capsuleEnd = new Vector3(0,0,0);
    }

    // Update is called once per frame
    void Update()
    {
        capsuleStart = activePlayer.transform.position;
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
            GizmoHelper.DrawWireCapsule(capsuleStart, capsuleEnd, capsuleRadius);
        }
    }

    void SwitchPlayer(PlayerController target){
        activePlayer.SetAIControlled();
        activePlayer = target;
        activePlayer.SetPlayerControlled();
    }

    Collider[] GetTargets(){
        return Physics.OverlapCapsule(capsuleStart, capsuleEnd, capsuleRadius);
    }
}
