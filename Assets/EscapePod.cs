using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapePod : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Vector3.Distance(this.transform.position, Vector3.zero) > 100.0f) {
            Destroy(gameObject);
        }
	}
}
