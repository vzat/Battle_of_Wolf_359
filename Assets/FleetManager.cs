using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleetManager : MonoBehaviour {

    public GameObject leader;
    public GameObject excelsiorPrefab;
    public GameObject nebulaPrefab;
    public GameObject mirandaPrefab;
    public GameObject apolloPrefab;
    public GameObject oberthPrefab;
    public GameObject ambassadorPrefab;

    public int fleetNo = 40;

    List<Boid> ships = new List<Boid>();
    

	// Use this for initialization
	void Start () {

		for (int i = 0; i < fleetNo; i++) {
            GameObject prefab = ambassadorPrefab;

            switch((int)Random.Range(1, 6)) {
                case 1:
                    prefab = excelsiorPrefab;
                    break;
                case 2:
                    prefab = nebulaPrefab;
                    break;
                case 3:
                    prefab = mirandaPrefab;
                    break;
                case 4:
                    prefab = apolloPrefab;
                    break;
                case 5:
                    prefab = oberthPrefab;
                    break;
                default:
                    break;
            }

            GameObject ship = Instantiate<GameObject>(prefab);
            //ship.transform.position = 
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
