using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Borg : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Object.DontDestroyOnLoad(this);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
