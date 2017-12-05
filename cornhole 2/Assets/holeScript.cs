using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class holeScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other)
    {
        GameObject camera = GameObject.Find("Main Camera");
        KinectListener kinectListener = camera.GetComponent<KinectListener>();

        other.gameObject.SetActive(false);
        kinectListener.score += 10;
        kinectListener.newBall = true;
    }
}
