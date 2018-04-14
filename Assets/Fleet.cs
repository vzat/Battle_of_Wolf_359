using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;

public class Fleet : MonoBehaviour {

    public List<Boid> ships = new List<Boid>();
    public GameObject shipPrefab;
    public int fleetNo = 500;

    Transform[] transforms;
    TransformAccessArray transformAccessArray;

    JobHandle accelerationJobHandle;
    JobHandle velocityJobHandle;
    JobHandle positionJobHandle;

    NativeArray<float> mass;
    NativeArray<float> maxSpeed;
    NativeArray<Vector3> force;
    NativeArray<Vector3> acceleration;
    NativeArray<Vector3> velocity;

    // Use this for initialization
    void Start () {

        // Allocate memory for native arrays
        mass = new NativeArray<float>(fleetNo, Allocator.Persistent);
        maxSpeed = new NativeArray<float>(fleetNo, Allocator.Persistent);
        force = new NativeArray<Vector3>(fleetNo, Allocator.Persistent);
        acceleration = new NativeArray<Vector3>(fleetNo, Allocator.Persistent);
        velocity = new NativeArray<Vector3>(fleetNo, Allocator.Persistent);

        transforms = new Transform[fleetNo];
         
		for (int i = 0; i < fleetNo; i++) {
            GameObject ship = Instantiate<GameObject>(shipPrefab);
            ship.transform.parent = this.transform;
            ship.transform.position = this.transform.position;
            ship.transform.rotation = this.transform.rotation;

            ship.transform.position = Random.insideUnitCircle * Random.Range(10, 100);

            transforms[i] = ship.transform;

            Boid shipBoid = ship.GetComponent<Boid>();

            // Initiate native arrays
            mass[i] = shipBoid.mass;
            maxSpeed[i] = shipBoid.maxSpeed;
            force[i] = new Vector3(0.0f, 0.0f, 0.0f);
            acceleration[i] = new Vector3(0.0f, 0.0f, 0.0f);
            velocity[i] = new Vector3(0.0f, 0.0f, 0.0f);

            ships.Add(shipBoid);
        }

        transformAccessArray = new TransformAccessArray(transforms);
    }

    struct AccelerationUpdateJob : IJobParallelFor {
        [ReadOnly]
        public NativeArray<float> mass;
        public NativeArray<Vector3> force;

        public NativeArray<Vector3> acceleration;

        public float deltaTime;

        public void Execute(int i) {
            Vector3 newAcceleration = force[i] / mass[i];

            float smoothRate = Mathf.Clamp(9.0f * deltaTime, 0.15f, 0.4f) / 2.0f;
            acceleration[i] = Vector3.Lerp(acceleration[i], newAcceleration, smoothRate);
        }
    }

    struct VelocityUpdateJob : IJobParallelFor {
        [ReadOnly]
        public NativeArray<float> maxSpeed;
        public NativeArray<Vector3> acceleration;

        public NativeArray<Vector3> velocity;

        public float deltaTime;

        public void Execute(int i) {
            velocity[i] += acceleration[i] * deltaTime;
            velocity[i] = Vector3.ClampMagnitude(velocity[i], maxSpeed[i]);

            if (velocity[i].magnitude > float.Epsilon) {
                velocity[i] *= 0.99f;
            }
        }
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

    // Update is called once per frame
    void Update () {

        for (int i = 0; i < fleetNo; i++) {
            force[i] = ships[i].Calculate();
        }

        //List<Vector3> velocities = new List<Vector3>();

        //for (int i = 0; i < ships.Count; i++) {
        //Boid ship = ships[i];

        //ship.force = ship.Calculate();
        //Vector3 newAcceleration = ship.force / ship.mass;

        //float smoothRate = Mathf.Clamp(9.0f * Time.deltaTime, 0.15f, 0.4f) / 2.0f;
        //ship.acceleration = Vector3.Lerp(ship.acceleration, newAcceleration, smoothRate);

        //ship.velocity += ship.acceleration * Time.deltaTime;
        //ship.velocity = Vector3.ClampMagnitude(ship.velocity, ship.maxSpeed);

        //Vector3 globalUp = new Vector3(0, 0.2f, 0);
        //Vector3 accelUp = ship.acceleration * 0.03f;
        //Vector3 bankUp = accelUp + globalUp;
        //smoothRate = Time.deltaTime * 5.0f;
        //Vector3 tempUp = ship.transform.up;
        //tempUp = Vector3.Lerp(tempUp, bankUp, smoothRate);

        //if (ship.velocity.magnitude > float.Epsilon) {
        //    ship.transform.LookAt(ship.transform.position + ship.velocity, tempUp);
        //    ship.velocity *= 0.99f;
        //}

        //ship.transform.position += ship.velocity * Time.deltaTime;
        //velocities.Add(ship.velocity);
        //velocities.Dispose();
        //velocities[i] = ship.velocity;
        //}

        AccelerationUpdateJob accelerationJob = new AccelerationUpdateJob() {
            mass = mass,
            force = force,
            acceleration = acceleration,
            deltaTime = Time.deltaTime
        };

        VelocityUpdateJob velocityJob = new VelocityUpdateJob() {
            maxSpeed = maxSpeed,
            acceleration = acceleration,
            velocity = velocity,
            deltaTime = Time.deltaTime
        };

        PositionUpdateJob positionJob = new PositionUpdateJob() {
            velocity = velocity,
            deltaTime = Time.deltaTime
        };

        accelerationJobHandle = accelerationJob.Schedule(fleetNo, 64);
        accelerationJobHandle.Complete();

        velocityJobHandle = velocityJob.Schedule(fleetNo, 64);
        velocityJobHandle.Complete();

        positionJobHandle = positionJob.Schedule(transformAccessArray);
    }

    void LateUpdate() {
        positionJobHandle.Complete();
    }

    void OnDestroy() {
        transformAccessArray.Dispose();
        mass.Dispose();
        maxSpeed.Dispose();
        force.Dispose();
        acceleration.Dispose();
        velocity.Dispose();
    }
}
