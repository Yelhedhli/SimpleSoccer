using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Ball : MonoBehaviour
{
    public Vector3 targetPos;

    [SerializeField]
    private int ballSpeed;

    //Ball will constantly go to the dribble position of the player in possession
    public PlayerController playerInPossession;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Moves the transform 
        this.transform.position = Vector3.MoveTowards(this.transform.position, targetPos, ballSpeed * Time.deltaTime);
        targetPos = playerInPossession.dribblePos.transform.position;
    }
}
