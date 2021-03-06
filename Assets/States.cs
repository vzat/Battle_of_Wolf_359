﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class AttackState : State {
    Boid boid;
    Seek seek;
    GameObject enemy;
    StateMachine stateMachine;

    public AttackState(GameObject enemy) {
        this.enemy = enemy;
    }

    public override void Enter() {
        boid = owner.GetComponent<Boid>();
        seek = owner.GetComponent<Seek>();

        seek.enabled = true;
        seek.targetGameObj = enemy;

        stateMachine = owner.GetComponent<StateMachine>();

        owner.GetComponent<ObstacleAvoidance>().enabled = true;
    }

    public override void Update() {
        if (Vector3.Distance(boid.transform.position, enemy.transform.position) < 30.0f) {
            stateMachine.ChangeState(new EscapeState(enemy));
        }
    }

    public override void Exit() {
        seek.enabled = false;
    }
}

public class EscapeState : State {
    Boid boid;
    Escape escape;
    GameObject enemy;
    StateMachine stateMachine;

    public EscapeState(GameObject enemy) {
        this.enemy = enemy;
    }

    public override void Enter() {
        boid = owner.GetComponent<Boid>();
        escape = owner.GetComponent<Escape>();

        escape.randomDir = Random.insideUnitCircle;
        escape.enabled = true;
        escape.targetGameObj = enemy;

        stateMachine = owner.GetComponent<StateMachine>();
    }

    public override void Update() {
        if (Vector3.Distance(boid.transform.position, enemy.transform.position) > 30.0f) {
            stateMachine.ChangeState(new AttackState(enemy));
        }
    }

    public override void Exit() {
        escape.enabled = false;
    }
}

public class CapturedState : State {
    Ship ship;
    GameObject enemy;

    public CapturedState(GameObject enemy) {
        this.enemy = enemy;
    }

    public override void Enter() {
        Boid boid = owner.GetComponent<Boid>();
        ship = owner.GetComponent<Ship>();

        // Stop the ship in the current place
        boid.velocity = Vector3.ClampMagnitude(boid.velocity, 0.0001f);

        ship.captured = true;
    }

    public override void Update() {
        if (!ship.captured) {
            StateMachine stateMachine = owner.GetComponent<StateMachine>();

            stateMachine.ChangeState(new AttackState(enemy));
        }
    }

    public override void Exit() {
    }
}

public class RunAwayState : State {
    GameObject enemy;
    Boid boid;
    Flee flee;

    public RunAwayState(GameObject enemy) {
        this.enemy = enemy;
    }

    public override void Enter() {
        boid = owner.GetComponent<Boid>();
        flee = owner.GetComponent<Flee>();

        flee.targetGameObj = enemy;
        flee.enabled = true;
    }

    public override void Update() {
        if (Vector3.Distance(boid.transform.position, enemy.transform.position) > 50.0f) {
            owner.GetComponent<StateMachine>().ChangeState(new AttackState(enemy));
        }
    }

    public override void Exit() {
        flee.enabled = false;
    }
}

public class DestroyedState : State {
    FleetManager fleetManager;
    float explosionTime;
    float timeElapsed = 0.0f;

    public DestroyedState(float explosionTime) {
        this.explosionTime = explosionTime;
    }

    void Destroy() {
        fleetManager.RemoveShip(owner.GetComponent<Boid>());

        //fleetManager.ships.Remove(owner.GetComponent<Boid>());
        //fleetManager.borg.GetComponent<Borg>().ships.Remove(owner.GetComponent<Boid>());
        fleetManager.shipsDestroyed++;
        owner.GetComponent<ObstacleAvoidance>().enabled = false;
        owner.GetComponent<StateMachine>().ChangeState(new WreckState());
    }

    public override void Enter() {
        owner.GetComponent<Boid>().velocity = Vector3.zero;

        Ship ship = owner.GetComponent<Ship>();
        fleetManager = ship.fleetManager;

        Boid boid = owner.GetComponent<Boid>();
        boid.acceleration = Vector3.zero;
        boid.velocity = Vector3.ClampMagnitude(boid.velocity, 0.0001f);

        boid.minMaxSpeed = 0.0f;
        boid.maxMaxSpeed = 0.0f;
    }

    public override void Update() {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > explosionTime) {
            Destroy();
        }
    }

    public override void Exit() {
    }
}

public class WreckState : State {
    Vector3 fromEnemy;
    Boid boid;
    Vector3 velocity;

    public override void Enter() {
        fromEnemy = owner.transform.position;
        fromEnemy.Normalize();

        boid = owner.GetComponent<Boid>();
        boid.acceleration = Vector3.zero;
        boid.velocity = Vector3.zero;

        velocity = fromEnemy * 1.0f;
    }

    public override void Update() {
        owner.transform.rotation = owner.transform.rotation * Quaternion.Euler(fromEnemy * Time.deltaTime * 3);
        boid.transform.position += velocity * Time.deltaTime;
    }

    public override void Exit() {
    }
}