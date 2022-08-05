using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    // variables
    private GameObject player;
    private float xScale;

    // Start is called before the first frame update
    void Start()
    {
        // variables
        player = GameObject.Find("Player");
        xScale = transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        // flip to face towards the player
        if(transform.position.x < player.transform.position.x)
        {
            transform.localScale = new Vector3(xScale, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(-xScale, transform.localScale.y, transform.localScale.z);
        }
    }
}
