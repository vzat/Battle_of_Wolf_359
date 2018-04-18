using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour {

    public GameObject target;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (target != null) {
            //transform.LookAt(Vector3.Lerp(transform.position, target.transform.position, Time.deltaTime));
            transform.LookAt(target.transform.position);
        }
	}
}
