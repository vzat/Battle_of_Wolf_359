using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapePod : MonoBehaviour {

	// Use this for initialization
	void Start () {
        this.GetComponent<Boid>().maxSpeed = 10.0f;
	}
	
	// Update is called once per frame
	void Update () {
		if (Vector3.Distance(this.transform.position, Vector3.zero) > 300.0f) {
            Destroy(gameObject);
        }
	}
}
