using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowShip : MonoBehaviour {
    public GameObject enemy;
    public GameObject ship;
    public float distance = 25.0f;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (enemy != null && ship != null) {
            Vector3 toEnemy = enemy.transform.position - ship.transform.position;
            toEnemy.Normalize();
            Vector3 toCamera = toEnemy * -1 * distance;

            this.transform.position = ship.transform.position + toCamera;

            transform.LookAt(ship.transform);
        }
    }
}
