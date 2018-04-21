﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;

public class FleetManager : MonoBehaviour {

    public GameObject excelsiorPrefab;
    public GameObject nebulaPrefab;
    public GameObject mirandaPrefab;
    public GameObject apolloPrefab;
    public GameObject oberthPrefab;
    public GameObject ambassadorPrefab;

    [HideInInspector]
    public GameObject leader;
    public List<Boid> ships = new List<Boid>();
    public GameObject borg;

    StateMachine stateMachine;
    public int fleetNo = 40;
    public float distLine = 10.0f;
    public float distCol = 10.0f;
    public Vector3 leaderPos = new Vector3(0, 0, -200);

    // Job System
    Transform[] transforms;
    JobHandle positionJobHandle;
    TransformAccessArray transformAccessArray;
    NativeArray<Vector3> velocities;

    struct PositionUpdateJob : IJobParallelForTransform {
        [ReadOnly]
        public NativeArray<Vector3> velocity;

        // Delta time needs to be passed to the job as it cannot access it
        public float deltaTime;

        public void Execute(int i, TransformAccess transform) {
            transform.position += velocity[i] * deltaTime;
        }
    }

    // Use this for initialization
    void Start () {

        // Set the initial scene
        FollowCamera mainCamera = Camera.main.GetComponent<FollowCamera>();
        stateMachine = GetComponent<StateMachine>();
        stateMachine.ChangeState(new Scene0());

        // Create leader and set position
        leader = new GameObject();
        leader.AddComponent<Boid>();
        leader.AddComponent<Arrive>();
        leader.transform.parent = this.transform;
        leader.transform.position = leaderPos;
        leader.GetComponent<Arrive>().target = new Vector3(0, 0, -50);
        Object.DontDestroyOnLoad(this);

        // Get Borg Cube
        borg = GameObject.Find("Borg");

        // Job System
        velocities = new NativeArray<Vector3>(fleetNo, Allocator.Persistent);
        transforms = new Transform[fleetNo];

        int line = 1;
        int shipsPerLine = 0;
        for (int i = 0; i < fleetNo; i++) {
            GameObject prefab = ambassadorPrefab;

            // Generate random ship type
            int shipType = (int)Random.Range(1, 6);

            // Set Predefied Ships for the first line
            switch (i) {
                case 0:
                    shipType = 3;
                    break;
                case 1:
                    shipType = 1;
                    break;
                case 2:
                    shipType = 6;
                    break;
                default:
                    break;
            }

            // Get a random ship type
            switch (shipType) {
                case 1:
                    prefab = excelsiorPrefab;
                    break;
                case 2:
                    prefab = nebulaPrefab;
                    break;
                case 3:
                    prefab = mirandaPrefab;
                    break;
                case 4:
                    prefab = apolloPrefab;
                    break;
                case 5:
                    prefab = oberthPrefab;
                    break;
                default:
                    break;
            }

            GameObject ship = Instantiate<GameObject>(prefab);
            ship.transform.parent = this.transform;
            ship.transform.position = leader.transform.position + new Vector3((shipsPerLine - line) * (distCol + Random.Range(0, 5)), Random.Range(-5, 5), - (line - 1) * (distLine + Random.Range(0, 5)));

            ship.GetComponent<StateMachine>().ChangeState(new FollowLeader(leader.GetComponent<Boid>()));
            ship.AddComponent<Escape>();
            ship.GetComponent<Escape>().enabled = false;

            // There are line * 2 + 1 ships per line
            if (shipsPerLine > line * 2 - 1) {
                line++;
                shipsPerLine = 0;
            }

            Boid shipBoid = ship.GetComponent<Boid>();
            shipBoid.jobSystemUpdate = true;
            ships.Add(shipBoid);
            shipsPerLine++;

            if (i == 0) {
                mainCamera.target = ship;
            }

            // Get ship transform
            transforms[i] = ship.transform;
        }

        transformAccessArray = new TransformAccessArray(transforms);
    }
	
	// Update is called once per frame
	void Update () {
        // Put the job before so it can process in parallel with the velocity calculation
        PositionUpdateJob positionJob = new PositionUpdateJob() {
            velocity = velocities,
            deltaTime = Time.deltaTime
        };

        positionJobHandle = positionJob.Schedule(transformAccessArray);

        for (int i = 0; i < ships.Count; i++) {
            Boid ship = ships[i];

            ship.force = ship.Calculate();
            Vector3 newAcceleration = ship.force / ship.mass;

            float smoothRate = Mathf.Clamp(9.0f * Time.deltaTime, 0.15f, 0.4f) / 2.0f;
            ship.acceleration = Vector3.Lerp(ship.acceleration, newAcceleration, smoothRate);

            ship.velocity += ship.acceleration * Time.deltaTime;
            ship.velocity = Vector3.ClampMagnitude(ship.velocity, ship.maxSpeed);

            Vector3 globalUp = new Vector3(0, 0.2f, 0);
            Vector3 accelUp = ship.acceleration * 0.03f;
            Vector3 bankUp = accelUp + globalUp;
            smoothRate = Time.deltaTime * 5.0f;
            Vector3 tempUp = ship.transform.up;
            tempUp = Vector3.Lerp(tempUp, bankUp, smoothRate);

            if (ship.velocity.magnitude > float.Epsilon) {
                ship.transform.LookAt(ship.transform.position + ship.velocity, tempUp);
                ship.velocity *= 0.99f;
            }

            velocities[i] = ship.velocity;
        }
    }

    void LateUpdate() {
        positionJobHandle.Complete();
    }

    void OnDestroy() {
        velocities.Dispose();
        transformAccessArray.Dispose();
    }
}

// Scenes
class Scene0 : State {
    FleetManager fleetManager;

    public override void Enter() {
        fleetManager = owner.GetComponent<FleetManager>();
    }

    public override void Update() {
        if (Vector3.Distance(fleetManager.leader.transform.position, new Vector3(0, 0, 0)) < 60.0f) {
            owner.ChangeState(new Scene1());
        }
    }

    public override void Exit() {
    }
}

class Scene1 : State {
    FleetManager fleetManager;
    VideoManager videoManager;

    public bool videoPlayed;

    public override void Enter() {
        SceneManager.LoadScene("scene1");
        fleetManager = owner.GetComponent<FleetManager>();

        // Get VideoManager
        videoManager = GameObject.Find("VideoManager").GetComponent<VideoManager>();

        // Set the first 3 ships to attack
        for (int i = 0; i < fleetManager.ships.Count && i < 3; i++) {
            Boid ship = fleetManager.ships[i];
            ship.GetComponent<StateMachine>().ChangeState(new AttackState(fleetManager.borg));
            ship.maxSpeed = 20.0f;
            ship.StartCoroutine(ship.ChangeSpeed());
        }

        // Change Camera Target
        FollowCamera mainCamera = Camera.main.GetComponent<FollowCamera>();
        mainCamera.target = fleetManager.GetComponent<GameObject>();

        videoPlayed = false;
    }

    public override void Update() {
        if (videoPlayed && !videoManager.playingVideo) {
            owner.ChangeState(new Scene2());
        }

        if (!videoManager.playingVideo && Vector3.Distance(fleetManager.ships[1].transform.position, fleetManager.borg.transform.position) < 35.0f) {
            videoManager.PlayVideo("./Assets/StreamingAssets/Locutus_of_Borg.mp4");
            videoPlayed = true;
        }
    }

    public override void Exit() {
    }
}

class Scene2 : State {
    FleetManager fleetManager;

    IEnumerator LoadScene() {
        AsyncOperation asyncSceneChange = SceneManager.LoadSceneAsync("scene2", LoadSceneMode.Single);

        while (!asyncSceneChange.isDone) {
            yield return null;
        }

        GameObject camera = GameObject.Find("Main Camera");
        FollowShip followShip = camera.GetComponent<FollowShip>();
        followShip.enemy = fleetManager.borg;
        followShip.ship = fleetManager.ships[0].gameObject;
    }

    public override void Enter() {
        fleetManager = owner.GetComponent<FleetManager>();

        fleetManager.StartCoroutine(LoadScene());
    }

    public override void Update() {
        
    }

    public override void Exit() {

    }
}