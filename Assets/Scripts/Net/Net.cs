using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Net : MonoBehaviour
{
    public Dictionary<string, Vector3> shotTargets = new Dictionary<string, Vector3>();

    [SerializeField]
    private List<GameObject> shotTargetList;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < shotTargetList.Count; i++)
        {
            shotTargets.Add(shotTargetList[i].name, shotTargetList[i].transform.position);
        }
    }
} 