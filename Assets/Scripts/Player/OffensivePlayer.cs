using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffensivePlayer : PlayerController
{
    public override void OffenseFixedUpdate(){
        resetPosition(2.5f);
    }

    public override void DefenseFixedUpdate(){
        resetPosition(2.5f);
    }
}