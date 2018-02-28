using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPath : SteeringBehaviour {

    public Path path;

    Vector3 currentWaypoint;

    public override Vector3 Calculate() {
        currentWaypoint = path.CurrentWaypoint();

        if (Vector3.Distance(transform.position, currentWaypoint) < 15) {
            path.GoToNextWaypoint();
        }

        return boid.SeekForce(currentWaypoint);
    }
}
