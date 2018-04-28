using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowShipsAlive : MonoBehaviour {

    Boid lastShip = null;
    Vector3 lastShipPos = Vector3.zero;
    List<Boid> ships = new List<Boid>();

    float startTime;

	// Use this for initialization
	void Start () {
        GameObject fleetManagerObj = GameObject.Find("FleetManager");
        FleetManager fleetManager = fleetManagerObj.GetComponent<FleetManager>();

        ships = fleetManager.ships;

        lastShip = ships[0];
        lastShipPos = lastShip.transform.position;
        startTime = Time.time;
    }
	
	// Update is called once per frame
	void Update () {
        if (ships[0] != null) {
            if (lastShip != null && lastShip != ships[0]) {

                float journeyLength = Vector3.Distance(lastShipPos, ships[0].transform.position);
                float distCovered = (Time.time - startTime) * 10.0f;
                float fracJoruney = distCovered / journeyLength;

                Vector3 lookAtPos = Vector3.Lerp(
                    lastShipPos,
                    ships[0].transform.position,
                    fracJoruney
                );

                if (Vector3.Distance(lookAtPos, ships[0].transform.position) < 1.0f) {
                    lastShip = ships[0];
                }

                transform.LookAt(lookAtPos);
            }
            else {
                startTime = Time.time;
                lastShipPos = ships[0].transform.position;
                transform.LookAt(ships[0].transform.position);
            }
        }
	}
}
