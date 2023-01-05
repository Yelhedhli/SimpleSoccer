using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public PlayerController[] teamPlayers;
    public PlayerController activePlayer;
    private int index = 0;
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
            index = (index+1)%3;
            print(index);
            SwitchPlayer(teamPlayers[index]);
        }
    }

    void SwitchPlayer(PlayerController target){
        activePlayer.SetAIControlled();
        activePlayer = target;
        activePlayer.SetPlayerControlled();
        
    }
}
