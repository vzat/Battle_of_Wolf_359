using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Borg : MonoBehaviour {

    public Material tractorBeamMaterial;
    public Material cuttingBeamMaterial;

    [HideInInspector]
    public bool attack = false;
    public List<Boid> ships = new List<Boid>();
    public Boid capturedShip = null;
    public Boid targetShip = null;

    LineRenderer tractorBeam;
    LineRenderer cuttingBeam;

    Vector3 tractorBeamSource = Vector3.zero;
    Vector3 cuttingBeamSource;

    FleetManager fleetManager;

    public AudioSource cuttingBeamAudioSource;
    public AudioSource tractorBeamAudioSource;

    public AudioClip cuttingBeamSound;
    public AudioClip tractorBeamSound;
    public AudioClip flyby;

    bool left = false;

    IEnumerator TractorBeamTarget() {
        yield return new WaitForSeconds(Random.Range(2, 3));
        while (true) {
            if (capturedShip != null) {
                capturedShip.GetComponent<Ship>().captured = false;
                capturedShip = null;
            }

            // Stop Sound
            if (tractorBeamAudioSource.isPlaying) {
                tractorBeamAudioSource.Stop();
            }

            yield return new WaitForSeconds(Random.Range(2, 4));

            int noAttackingShips = 0;
            for (int i = 0; i < ships.Count; i++) {
                Boid ship = ships[i];
                StateMachine shipStateMachine = ship.GetComponent<StateMachine>();
                State shipState = shipStateMachine.state;

                if (shipState.GetType().Name == "IdleState" || shipState.GetType().Name == "FollowLeader") {
                    break;
                }

                noAttackingShips++;
            }

            // Capture a random ship that is currently attacking the Borg Cube
            if (noAttackingShips > 0 && attack) {
                int shipToCapture = (int)(Random.Range(0, noAttackingShips));
                capturedShip = ships[shipToCapture];

                // Do not capture the ship if it's out of range
                StateMachine shipStateMachine = capturedShip.GetComponent<StateMachine>();
                if (Vector3.Distance(capturedShip.transform.position, transform.position) < 35.0f && 
                    (shipStateMachine.state.GetType().Name == "AttackState" || 
                     shipStateMachine.state.GetType().Name == "EscapeState")) {
                        shipStateMachine.ChangeState(new CapturedState(gameObject));
                        tractorBeamSource = Random.insideUnitSphere * 3.0f;

                        // Play Sound
                        tractorBeamAudioSource.Play();
                }
                else {
                    capturedShip = null;
                }
            }

            yield return new WaitForSeconds(Random.Range(3, 4));
        }
    }

    IEnumerator CuttingBeamTarget() {
        yield return new WaitForSeconds(Random.Range(2, 3));
        while (true) {
            if (targetShip != null) {
                targetShip.GetComponent<Ship>().structuralIntegrity -= Random.Range(50, 150);
            }
            targetShip = null;

            // Stop Sound
            if (cuttingBeamAudioSource.isPlaying) {
                cuttingBeamAudioSource.Stop();
            }

            yield return new WaitForSeconds(Random.Range(1, 2));

            int noAttackingShips = 0;
            for (int i = 0; i < ships.Count; i++) {
                Boid ship = ships[i];
                StateMachine shipStateMachine = ship.GetComponent<StateMachine>();
                State shipState = shipStateMachine.state;

                if (shipState.GetType().Name == "IdleState" || shipState.GetType().Name == "FollowLeader") {
                    break;
                }

                noAttackingShips++;
            }

            // Choose a random ship to attack
            // The captured ship will be more likely to be attacked
            if (noAttackingShips > 0 && attack) {
                if (Random.Range(1, 3) < 2 && capturedShip != null) {
                    // Attack Captured Ship
                    targetShip = capturedShip;
                } 
                else {
                    int shipToAttack = (int)(Random.Range(0, noAttackingShips));
                    targetShip = ships[shipToAttack];

                    if (Vector3.Distance(targetShip.transform.position, transform.position) < 35.0f) {
                        cuttingBeamSource = Random.insideUnitSphere * 3.0f;

                        // Play Sound
                        //cuttingBeamAudioSource.Play();
                        cuttingBeamAudioSource.PlayOneShot(cuttingBeamSound);
                    }
                    else {
                        targetShip = null;
                    }
                }
            }

            yield return new WaitForSeconds(Random.Range(1, 2));
        }
    }

    public void StartAI() {
        StartCoroutine(TractorBeamTarget());
        StartCoroutine(CuttingBeamTarget());
    }

    // Use this for initialization
    void Start () {
        Object.DontDestroyOnLoad(this);

        GameObject fleetManagerObj = GameObject.Find("FleetManager");
        fleetManager = fleetManagerObj.GetComponent<FleetManager>();

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

        //StartCoroutine(TractorBeamTarget());


        // Setup cutting beam
        cuttingBeam = cuttingBeamObject.GetComponent<LineRenderer>();

        cuttingBeam.material = cuttingBeamMaterial;
        cuttingBeam.startWidth = 0.5f;
        cuttingBeam.endWidth = 0.5f;

        //StartCoroutine(CuttingBeamTarget());

        // Setup Audio Sources
        cuttingBeamAudioSource = this.GetComponents<AudioSource>()[0];
        tractorBeamAudioSource = this.GetComponents<AudioSource>()[1];

        cuttingBeamAudioSource.clip = cuttingBeamSound;
        cuttingBeamAudioSource.loop = true;

        tractorBeamAudioSource.clip = tractorBeamSound;
        tractorBeamAudioSource.volume = 0.5f;
        tractorBeamAudioSource.loop = true;
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

        // Leave after all the enemies have been destroyed
        if (ships.Count == 0 && !left) {
            left = true;
            gameObject.AddComponent<Boid>();
            gameObject.AddComponent<Seek>();
            gameObject.GetComponent<Seek>().target = new Vector3(0, 0, 1000);
            gameObject.GetComponent<Boid>().maxSpeed = 50.0f;

            cuttingBeamAudioSource.PlayOneShot(flyby);
        }
	}
}
