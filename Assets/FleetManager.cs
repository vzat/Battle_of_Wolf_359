using System.Collections;
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
    public GameObject escapePodPrefab;

    [HideInInspector]
    public GameObject leader;
    public List<Boid> ships = new List<Boid>();
    public List<Ship> shipComp = new List<Ship>();
    public GameObject borg;

    StateMachine stateMachine;
    public int fleetNo = 40;
    public float distLine = 10.0f;
    public float distCol = 10.0f;
    public Vector3 leaderPos = new Vector3(0, 0, -200);
    public int shipsDestroyed = 0;

    public AudioClip phaserSound;
    public AudioClip torpedoSound;
    public AudioClip flybySound;
    public AudioClip[] explosionSounds = new AudioClip[3];
    public AudioClip[] phaserSounds = new AudioClip[6];
    public AudioClip introMusic;
    public AudioClip battleMusic;

    // Job System
    Transform[] transforms;
    JobHandle positionJobHandle;
    TransformAccessArray transformAccessArray;
    NativeArray<Vector3> velocities;

    List<Boid> shipsToDestroy = new List<Boid>();

    public AudioSource audioSource;

    VideoManager videoManager;

    public void RemoveShip(Boid ship) {
        shipsToDestroy.Add(ship);
    }

    void RemoveShipsFromList() {
        foreach (Boid ship in shipsToDestroy) {
            // Get ship no
            int shipNo = ships.IndexOf(ship);

            // Remove ship from list
            ships.RemoveAt(shipNo);
            shipComp.RemoveAt(shipNo);

            // Recreate the velocities array
            List<Vector3> tempVelocities = new List<Vector3>(velocities.ToArray());
            tempVelocities.RemoveAt(shipNo);
            velocities.Dispose();
            velocities = new NativeArray<Vector3>(ships.Count, Allocator.Persistent);
            velocities.CopyFrom(tempVelocities.ToArray());

            // Remove from transfrom access array
            transformAccessArray.Dispose();
            Transform[] tempTransforms = new Transform[ships.Count];
            for (int i = 0; i < ships.Count; i++) {
                tempTransforms[i] = ships[i].transform;
            }
            transformAccessArray = new TransformAccessArray(tempTransforms);
        }
        shipsToDestroy.Clear();
    }

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
            ship.GetComponent<Escape>().weight = 2;
            ship.GetComponent<Ship>().fleetManager = this;

            // There are line * 2 + 1 ships per line
            if (shipsPerLine > line * 2 - 1) {
                line++;
                shipsPerLine = 0;
            }

            Boid shipBoid = ship.GetComponent<Boid>();
            shipBoid.jobSystemUpdate = true;
            ships.Add(shipBoid);
            shipComp.Add(ship.GetComponent<Ship>());
            shipsPerLine++;

            if (i == 0) {
                mainCamera.target = ship;

                Ship shipComp = ship.GetComponent<Ship>();
                shipComp.structuralIntegrity = 1000.0f;
                shipComp.maxStructuralIntegrity = 1000.0f;
            }

            // Get ship transform
            transforms[i] = ship.transform;
        }

        transformAccessArray = new TransformAccessArray(transforms);

        audioSource = GetComponent<AudioSource>();
        audioSource.clip = introMusic;
        audioSource.Play();

        videoManager = GameObject.Find("VideoManager").GetComponent<VideoManager>();
    }
	
	// Update is called once per frame
	void Update () {
        RemoveShipsFromList();

        // Put the job before so it can process in parallel
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
            if (!shipComp[i].captured) {
                tempUp = Vector3.Lerp(tempUp, bankUp, smoothRate);
            }

            if (ship.velocity.magnitude > float.Epsilon) {
                if (!shipComp[i].captured) {
                    ship.transform.LookAt(ship.transform.position + ship.velocity, tempUp);
                }
                ship.velocity *= 0.99f;
            }

            velocities[i] = ship.velocity;
        }

        audioSource.volume = videoManager.playingVideo ? 0.25f : 1.0f;
    }

    void LateUpdate() {
        positionJobHandle.Complete();
    }

    void OnDestroy() {
        velocities.Dispose();
        transformAccessArray.Dispose();
    }
}