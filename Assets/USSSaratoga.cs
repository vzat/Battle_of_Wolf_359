using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class USSSaratoga : MonoBehaviour {

    Ship ship;

	// Use this for initialization
	void Start () {
        ship = GetComponent<Ship>();
	}
	
	// Update is called once per frame
	void Update () {
		if (ship.structuralIntegrity + 151 < ship.maxStructuralIntegrity) {
            
        }
	}
}
