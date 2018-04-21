using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flee : SteeringBehaviour {

    public GameObject targetGameObj = null;

    public Vector3 target = Vector3.zero;

    public override Vector3 Calculate() {
        return boid.FleeForce(target);
    }

    // Update is called once per frame
    void Update() {
        if (targetGameObj != null) {
            target = targetGameObj.transform.position;
        }
    }
}
