using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowShip : MonoBehaviour {
    public GameObject enemy;
    public GameObject ship;
    public float distance = 25.0f;

    public Ship shipComponent;

    // Use this for initialization
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        if (shipComponent != null && shipComponent.captured) {
            transform.position = Vector3.Lerp(
                this.transform.position,
                ship.transform.TransformPoint(new Vector3(0, 10.0f, 5.0f)),
                Time.deltaTime
            );

            transform.LookAt(ship.transform);
        }
        else if (enemy != null && ship != null) {
            Vector3 toEnemy = enemy.transform.position - ship.transform.position;
            toEnemy.Normalize();
            Vector3 toCamera = toEnemy * -1 * distance;

            this.transform.position = Vector3.Lerp(
                this.transform.position,
                ship.transform.position + toCamera,
                Time.deltaTime
            );

            transform.LookAt(ship.transform);
        }
    }
}
