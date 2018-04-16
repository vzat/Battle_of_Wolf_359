﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour {

    List<SteeringBehaviour> behaviours = new List<SteeringBehaviour>();

    public Vector3 force, acceleration, velocity;

    public float maxSpeed = 10;
    public float mass = 1;

    float minMaxSpeed = 15.0f;
    float maxMaxSpeed = 25.0f;

    public bool jobSystemUpdate = false;

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

    public Vector3 EscapeForce(Vector3 target, Vector3 randomDir) {
        Vector3 toTarget = target - transform.position;

        toTarget.Normalize();

        Vector3 desired = Vector3.Cross(toTarget, randomDir) * maxSpeed;

        return desired;
    }

	// Use this for initialization
	void Start () {
        SteeringBehaviour[] behaviours = GetComponents<SteeringBehaviour>();

        foreach (SteeringBehaviour behaviour in behaviours) {
            this.behaviours.Add(behaviour);
        }
	}

    public IEnumerator ChangeSpeed() {
        while (true) {
            float speedDif = Random.Range(-5, 5);

            maxSpeed += speedDif;

            maxSpeed = maxSpeed < minMaxSpeed ? minMaxSpeed : maxSpeed;
            maxSpeed = maxSpeed > maxMaxSpeed ? maxMaxSpeed : maxSpeed;

            yield return new WaitForSeconds(Random.Range(1, 5));
        }
    }

    public Vector3 Calculate() {
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
        if (!jobSystemUpdate) {
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
}


