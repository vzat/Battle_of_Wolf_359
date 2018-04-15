using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrive : SteeringBehaviour {

    public Vector3 target = Vector3.zero;
    public float slowingDistance = 15.0f;

    [Range(0.0f, 1.0f)]
    public float deceleration = 0.9f;

    public GameObject targetGameObj = null;

    public override Vector3 Calculate() {
        return boid.ArriveForce(target, slowingDistance, deceleration);
    }

    public void Update() {
        if (targetGameObj != null) {
            target = targetGameObj.transform.position;
        }
    }
}