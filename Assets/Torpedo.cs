using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torpedo : MonoBehaviour {

    public Vector3 destination;
    public float speed = 20.0f;

    Vector3 velocity;

	// Use this for initialization
	void Start () {
        Vector3 toTarget = destination - transform.position;
        toTarget.Normalize();

        velocity = toTarget * speed;
    }
	
	// Update is called once per frame
	void Update () {
        transform.position += velocity * Time.deltaTime;

        if (Vector3.Distance(destination, this.transform.position) < 3.0f) {
            Destroy(this.gameObject);
        }
	}
}
