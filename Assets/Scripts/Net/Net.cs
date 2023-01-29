using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Net : MonoBehaviour
{
    public Dictionary<string, Vector3> shotTargets = new Dictionary<string, Vector3>();

    //[SerializeField]
    private string[] targetNames = new string [] {"TR", "BR", "TL", "BL"};

    [SerializeField]
    private List<GameObject> shotTargetList;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < targetNames.Length; i++){
            int c = i;
            print(c);
            shotTargets.Add(targetNames[c] , shotTargetList[c].transform.position);
        }

        // foreach (KeyValuePair<string, Vector3> kvp in shotTargets){
        //     print(kvp.Key + " : " + kvp.Value.ToString());
        // }
        
        //print(shotTargets["TR"]);
    }
}
