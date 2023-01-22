using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public bool isTeam1Net;

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.tag == "Ball")
        {
            if (isTeam1Net)
            {
                GameManager.instance.GoalTeam2();
            }
            else
            {
                GameManager.instance.GoalTeam1();
            }
            
        }
    }

}
