using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Escape : SteeringBehaviour {

    public GameObject targetGameObj = null;
    public Vector3 randomDir;

    public Vector3 target = Vector3.zero;

    public override Vector3 Calculate() {
        return boid.EscapeForce(target, randomDir);
    }

    // Update is called once per frame
    void Update() {
        if (targetGameObj != null) {
            target = targetGameObj.transform.position;
        }
    }
}
