using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class OffensivePlayer : PlayerController
{

    private enum OffensiveAction{CutIn, MakeRunOutside, MakeRunInside, GetOpen};
    private OffensiveAction offensiveAction;
    private Vector3 offensiveTargetPosition;
    private Random random = new Random();

    public override void OffenseFixedUpdate(){
        switch(offensiveBrainState){
            case OffensiveBrainState.Decide:
                MakeOffensiveDecision();
                offensiveBrainState = OffensiveBrainState.Act;
                break;

            case OffensiveBrainState.Act:

                switch (offensiveAction){
                    case OffensiveAction.GetOpen:
                        offensiveBrainState = OffensiveBrainState.Reset;
                        break;

                    default:
                        MoveAI(offensiveTargetPosition);

                        if (Vector3.Distance(offensiveTargetPosition, this.transform.position) < 2){
                            offensiveBrainState = OffensiveBrainState.Reset;
                        }

                        break;
                }

                break;

            case OffensiveBrainState.Reset:
                resetPosition(1);

                if (Vector3.Distance(anchorPosition, this.transform.position) < 2){
                    offensiveBrainState = OffensiveBrainState.Decide;
                }

                break;
        }
    }

    public override void DefenseFixedUpdate(){
        resetPosition(2.5f);
    }

    void MakeOffensiveDecision(){
        Array values = Enum.GetValues(typeof(OffensiveAction));
        offensiveAction = (OffensiveAction)values.GetValue(random.Next(values.Length));

        switch (offensiveAction){
            case OffensiveAction.CutIn:
                offensiveTargetPosition = playerManager.opponentNet.transform.position;
                offensiveTargetPosition.z = 0;
                break;

            case OffensiveAction.MakeRunOutside:
                offensiveTargetPosition = playerManager.opponentNet.transform.position;
                offensiveTargetPosition.z = 0;
                break;

            case OffensiveAction.MakeRunInside:
                offensiveTargetPosition = playerManager.opponentNet.transform.position;
                offensiveTargetPosition.z = 0;
                break;
        }
    }
}