using System.Collections;
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