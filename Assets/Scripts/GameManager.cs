using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int Team1Goals;
    public int Team2Goals;

    private void Awake()
    {
        if (GameManager.instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);
    }

    public void GoalTeam1()
    {
        print("Team 1 scores a goal!");
        Team1Goals += 1;
        //Here we load scene with
    }

    public void GoalTeam2()
    {
        print("Team 2 scores a goal!");
        Team2Goals += 1;
    }
}
