﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour {

    List<SteeringBehaviour> behaviours = new List<SteeringBehaviour>();

    public Vector3 force, acceleration, velocity;

    public float maxSpeed = 10;
    public float mass = 1;

    public Vector3 SeekForce(Vector3 target) {
        Vector3 toTarget = target - transform.position;
        toTarget.Normalize();

        Vector3 desired = toTarget * maxSpeed;

        return desired - velocity;
    }

    public Vector3 ArriveForce(Vector3 target, float slowingDistance = 15.0f, float deceleration = 1.0f) {
        Vector3 toTarget = target - transform.position;

        float distance = toTarget.magnitude;
        if (distance == 0) {
            return Vector3.zero;
        }

        float ramped = maxSpeed * (distance / (slowingDistance * deceleration));
        float clamped = Mathf.Min(ramped, maxSpeed);

        Vector3 desired = clamped * (toTarget / distance);

        return desired - velocity;
    }

	// Use this for initialization
	void Start () {
        SteeringBehaviour[] behaviours = GetComponents<SteeringBehaviour>();

        foreach (SteeringBehaviour behaviour in behaviours) {
            this.behaviours.Add(behaviour);
        }
	}

    Vector3 Calculate() {
        Vector3 force = Vector3.zero;

        foreach (SteeringBehaviour behaviour in behaviours) {
            if (behaviour.isActiveAndEnabled) {
                force += behaviour.Calculate() * behaviour.weight;
            }
        }

        return force;
    }
	
	// Update is called once per frame
	void Update () {
        force = Calculate();
        Vector3 newAcceleration = force / mass;

        float smoothRate = Mathf.Clamp(9.0f * Time.deltaTime, 0.15f, 0.4f) / 2.0f;
        acceleration = Vector3.Lerp(acceleration, newAcceleration, smoothRate);

        velocity += acceleration * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        Vector3 globalUp = new Vector3(0, 0.2f, 0);
        Vector3 accelUp = acceleration * 0.03f;
        Vector3 bankUp = accelUp + globalUp;
        smoothRate = Time.deltaTime * 5.0f;
        Vector3 tempUp = transform.up;
        tempUp = Vector3.Lerp(tempUp, bankUp, smoothRate);

        if (velocity.magnitude > float.Epsilon) {
            transform.LookAt(transform.position + velocity, tempUp);
            velocity *= 0.99f;
        }

        transform.position += velocity * Time.deltaTime;
	}
}

// Ship States
public class IdleState : State {
    Boid boid;

    public override void Enter() {
        boid = owner.GetComponent<Boid>();
        boid.force = Vector3.zero;
    }

    public override void Update() {
    }

    public override void Exit() {
    }
}

public class FollowLeader : State {
    Boid boid;
    Boid leader;
    OffsetPursue offsetPursue;
    float lastVelocityMagnitude;

    public FollowLeader(Boid leader) {
        this.leader = leader;
    }

    public override void Enter() {
        boid = owner.GetComponent<Boid>();
        offsetPursue = owner.GetComponent<OffsetPursue>();
        offsetPursue.leader = leader;
        offsetPursue.enabled = true;
        lastVelocityMagnitude = -1;
    }

    public override void Update() {
        if (lastVelocityMagnitude > boid.velocity.magnitude && boid.velocity.magnitude < 1) {
            boid.GetComponent<StateMachine>().ChangeState(new IdleState());
        }
        lastVelocityMagnitude = boid.velocity.magnitude;
    }

    public override void Exit() {
        offsetPursue.enabled = false;
    }
}
