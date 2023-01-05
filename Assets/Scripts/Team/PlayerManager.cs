using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public PlayerController[] teamPlayers;
    public int activePlayer = 0;
    // Start is called before the first frame update
    void Start()
    {
        teamPlayers = GetComponentsInChildren<PlayerController>();
        teamPlayers[activePlayer].brainState = PlayerController.BrainState.Player;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("SwitchPlayer")){
            print(activePlayer);
            teamPlayers[activePlayer].brainState = PlayerController.BrainState.Offense;
            activePlayer = (activePlayer+1)%3;
            print(activePlayer);
            teamPlayers[activePlayer].brainState = PlayerController.BrainState.Player;
        }
    }
}
