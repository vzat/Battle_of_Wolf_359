using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowShip : MonoBehaviour {
    public GameObject enemy;
    public GameObject ship;
    public float distance = 10.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (enemy != null && ship != null) {
            //Vector3 toEnemy = enemy.transform.position - ship.transform.position;
            //float x = Vector3.Angle(toEnemy, ship.transform.forward);
            //float y = Vector3.Angle(toEnemy, ship.transform.up);
            //float z = Vector3.Angle(toEnemy, ship.transform.right);

            //transform.localPosition = new Vector3(Mathf.Sin(x) * distance, 10.0f, Mathf.Cos(x) * distance);
            transform.LookAt(ship.transform);

        }
	}
}
