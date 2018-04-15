using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffsetPursue : SteeringBehaviour {

    public Boid leader;
    private Vector3 offset;
    Vector3 worldtarget;

    void Start() {
        offset = transform.position - leader.transform.position;
        offset = Quaternion.Inverse(leader.transform.rotation) * offset;
    }

    // Update is called once per frame
    void Update() {

    }

    public override Vector3 Calculate() {
        worldtarget = leader.transform.TransformPoint(offset);
        float dist = Vector3.Distance(worldtarget
            , transform.position);
        float time = dist / boid.maxSpeed;

        Vector3 targetPos = worldtarget + (leader.velocity * time);
        return boid.ArriveForce(targetPos, 10);

    }
}