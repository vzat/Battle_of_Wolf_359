using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour {

    public List<Vector3> waypoints = new List<Vector3>();

    public int currentWaypoint = 0;

	// Use this for initialization
	void Start () {
		for (int i = 0; i < transform.childCount; i++) {
            waypoints.Add(transform.GetChild(i).position);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public Vector3 CurrentWaypoint() {
        return waypoints[currentWaypoint];
    }

    public void GoToNextWaypoint() {
        currentWaypoint = (currentWaypoint + 1) % waypoints.Count;
    }
}
