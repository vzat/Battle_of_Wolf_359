using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Borg : MonoBehaviour {

    public Material tractorBeamMaterial;
    public Material cuttingBeamMaterial;

    List<Boid> ships = new List<Boid>();
    Boid capturedShip = null;
    Boid targetShip = null;

    LineRenderer tractorBeam;
    LineRenderer cuttingBeam;

    Vector3 tractorBeamSource = Vector3.zero;
    Vector3 cuttingBeamSource;

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
                    tractorBeamSource = Random.insideUnitSphere * 3.0f;
                }
                else {
                    capturedShip = null;
                }
            }

            yield return new WaitForSeconds(Random.Range(3, 4));
        }
    }

    IEnumerator CuttingBeamTarget() {
        while (true) {
            targetShip = null;

            yield return new WaitForSeconds(Random.Range(1, 2));

            int noAttackingShips = 0;
            foreach (Boid ship in ships) {
                StateMachine shipStateMachine = ship.GetComponent<StateMachine>();
                State shipState = shipStateMachine.state;

                if (shipState.GetType().Name == "IdleState" || shipState.GetType().Name == "FollowLeader") {
                    break;
                }

                noAttackingShips++;
            }

            // Choose a random ship to attack
            // The captured ship will be more likely to be attacked
            if (noAttackingShips > 0) {
                if (Random.Range(1, 3) < 2 && capturedShip != null) {
                    // Attack Captured Ship
                    targetShip = capturedShip;
                } 
                else {
                    int shipToAttack = (int)(Random.Range(0, noAttackingShips));
                    targetShip = ships[shipToAttack];

                    if (Vector3.Distance(targetShip.transform.position, transform.position) < 35.0f) {
                        cuttingBeamSource = Random.insideUnitSphere * 3.0f;
                        targetShip.GetComponent<Ship>().structuralIntegrity -= Random.Range(50, 150);
                    }
                    else {
                        targetShip = null;
                    }
                }
            }

            yield return new WaitForSeconds(Random.Range(1, 2));
        }
    }

	// Use this for initialization
	void Start () {
        Object.DontDestroyOnLoad(this);

        GameObject fleetManagerObj = GameObject.Find("FleetManager");
        FleetManager fleetManager = fleetManagerObj.GetComponent<FleetManager>();

        ships = fleetManager.ships;

        GameObject tractorBeamObject = new GameObject();
        tractorBeamObject.transform.parent = this.transform;
        tractorBeamObject.AddComponent<LineRenderer>();

        GameObject cuttingBeamObject = new GameObject();
        cuttingBeamObject.transform.parent = this.transform;
        cuttingBeamObject.AddComponent<LineRenderer>();


        // Setup tractor beam
        tractorBeam = tractorBeamObject.GetComponent<LineRenderer>();

        tractorBeam.material = tractorBeamMaterial;
        tractorBeam.startWidth = 0.1f;
        tractorBeam.endWidth = 5.0f;

        StartCoroutine(TractorBeamTarget());


        // Setup cutting beam
        cuttingBeam = cuttingBeamObject.GetComponent<LineRenderer>();

        cuttingBeam.material = cuttingBeamMaterial;
        cuttingBeam.startWidth = 0.5f;
        cuttingBeam.endWidth = 0.5f;

        StartCoroutine(CuttingBeamTarget());
    }
	
	// Update is called once per frame
	void Update () {
        if (capturedShip != null) {
            tractorBeam.SetPosition(0, tractorBeamSource);
            tractorBeam.SetPosition(1, capturedShip.transform.position);
        }
        else {
            tractorBeam.SetPosition(0, this.transform.position);
            tractorBeam.SetPosition(1, this.transform.position);
        }

        if (targetShip != null) {
            cuttingBeam.SetPosition(0, cuttingBeamSource);
            cuttingBeam.SetPosition(1, targetShip.transform.position);
        }
        else {
            cuttingBeam.SetPosition(0, this.transform.position);
            cuttingBeam.SetPosition(1, this.transform.position);
        }
	}
}
