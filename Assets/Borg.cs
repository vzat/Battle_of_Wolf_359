using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Borg : MonoBehaviour {

    public Material tractorBeamMaterial;

    List<Boid> ships = new List<Boid>();
    Boid capturedShip = null;

    LineRenderer tractorBeam;

    IEnumerator TractorBeamTarget() {
        while (true) {
            if (capturedShip != null) {
                capturedShip.GetComponent<Ship>().captured = false;
                capturedShip = null;
            }

            yield return new WaitForSeconds(Random.Range(2, 4));

            int noAttackingShips = 0;
            foreach (Boid ship in ships) {
                StateMachine shipStateMachine = ship.GetComponent<StateMachine>();
                State shipState = shipStateMachine.state;

                if (shipState.GetType().Name == "IdleState" || shipState.GetType().Name == "FollowLeader") {
                    break;
                }

                noAttackingShips++;
            }

            // Capture a random ship that is currently attacking the Borg Cube
            if (noAttackingShips > 0) {
                int shipToCapture = (int)(Random.Range(0, noAttackingShips));
                capturedShip = ships[shipToCapture];

                // Do not capture the ship if it's out of range
                if (Vector3.Distance(capturedShip.transform.position, transform.position) < 35.0f) {
                    StateMachine shipStateMachine = capturedShip.GetComponent<StateMachine>();
                    shipStateMachine.ChangeState(new CapturedState(gameObject));
                }
                else {
                    capturedShip = null;
                }
            }

            yield return new WaitForSeconds(Random.Range(3, 4));
        }
    }

	// Use this for initialization
	void Start () {
        Object.DontDestroyOnLoad(this);

        GameObject fleetManagerObj = GameObject.Find("FleetManager");
        FleetManager fleetManager = fleetManagerObj.GetComponent<FleetManager>();

        ships = fleetManager.ships;

        gameObject.AddComponent<LineRenderer>();
        tractorBeam = GetComponent<LineRenderer>();

        tractorBeam.material = tractorBeamMaterial;
        tractorBeam.startWidth = 0.1f;
        tractorBeam.endWidth = 5.0f;

        StartCoroutine(TractorBeamTarget());
    }
	
	// Update is called once per frame
	void Update () {
        if (capturedShip != null) {
            tractorBeam.SetPosition(0, this.transform.position);
            tractorBeam.SetPosition(1, capturedShip.transform.position);
        }
        else {
            tractorBeam.SetPosition(0, this.transform.position);
            tractorBeam.SetPosition(1, this.transform.position);
        }
	}
}
