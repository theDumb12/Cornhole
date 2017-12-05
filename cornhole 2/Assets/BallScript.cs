using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallScript : MonoBehaviour {

    public Vector3 throwVel;
    public Transform bag;

    private bool inHand = true;
    private bool thrown = false;

    private Shader shader;

    private Rigidbody rb;
    private Renderer rend;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        Physics.gravity = 18.0f * Physics.gravity;
        rend.material.color = Color.black;
	}
	
	// Update is called once per frame
	void Update () {
        GameObject camera = GameObject.Find("Main Camera");
        KinectListener kinectListener = camera.GetComponent<KinectListener>();

        if(inHand == false && kinectListener.releasedNow == false)
        {
            inHand = true;
        }

        thrown = kinectListener.releasedNow;

        float magicCoeff = 75.0f;

        //if (inHand == true && thrown == false)
        //{
            transform.position = new Vector3((float)kinectListener.handX * 20.0f,
                                             (float)kinectListener.handy * 20.0f,
                                     20.0f - (float)kinectListener.handz * 30.0f); // Magic values are fun
        //}
        if (inHand == true && thrown == true)
        {
            inHand = false;
            //throwVel = rb.velocity;
            //rb.velocity = new Vector3(magicCoeff * (float)kinectListener.releasedXVelocity,
            //                          magicCoeff * (float)kinectListener.releasedYVelocity,
            //                          magicCoeff * (float)kinectListener.releasedZVelocity);
            Transform obj;
            obj = Instantiate<Transform>(bag, transform.position, Quaternion.identity);
            obj.GetComponent<Rigidbody>().velocity = new Vector3(magicCoeff * (float)kinectListener.releasedXVelocity,
                                                                 magicCoeff * (float)kinectListener.releasedYVelocity,
                                                         (magicCoeff + 5.0f) * (float)kinectListener.releasedZVelocity);

        }
    }
}
