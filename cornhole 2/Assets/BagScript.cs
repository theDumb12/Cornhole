using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BagScript : MonoBehaviour {

    private Rigidbody rb;
    private Renderer rend;

    float lifetime = 30.0f;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        rend.material.color = Color.green;
        StartCoroutine(WaitThenDie());
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator WaitThenDie()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
}
