using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State {
    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
}

public class StateMachine : MonoBehaviour {

    public State state;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (state != null) {
            state.Update();
        }
	}

    public void ChangeState(State newState) {
        if (state != null) {
            state.Exit();
        }

        state = newState;

        if (state != null) {
            state.Enter();
        }
    }
}
