using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleetManager : MonoBehaviour {

    public GameObject excelsiorPrefab;
    public GameObject nebulaPrefab;
    public GameObject mirandaPrefab;
    public GameObject apolloPrefab;
    public GameObject oberthPrefab;
    public GameObject ambassadorPrefab;

    GameObject leader;
    public int fleetNo = 40;
    public float distLine = 10.0f;
    public float distCol = 10.0f;
    public Vector3 leaderPos = new Vector3(0, 0, -200);

    List<Boid> ships = new List<Boid>();

	// Use this for initialization
	void Start () {

        FollowCamera mainCamera = Camera.main.GetComponent<FollowCamera>();

        leader = new GameObject();
        leader.AddComponent<Boid>();
        leader.AddComponent<Arrive>();
        leader.transform.parent = this.transform;
        leader.transform.position = leaderPos;

        int line = 1;
        int shipsPerLine = 0;
        for (int i = 0; i < fleetNo; i++) {
            GameObject prefab = ambassadorPrefab;

            // Generate random ship type
            int shipType = (int)Random.Range(1, 6);

            // Set Predefied Ships for the first line
            switch (i) {
                case 0:
                    shipType = 3;
                    break;
                case 1:
                    shipType = 1;
                    break;
                case 2:
                    shipType = 6;
                    break;
                default:
                    break;
            }

            // Get a random ship type
            switch (shipType) {
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
            ship.transform.parent = this.transform;
            ship.transform.position = leader.transform.position + new Vector3((shipsPerLine - line) * (distCol + Random.Range(0, 5)), 0, - (line - 1) * (distLine + Random.Range(0, 5)));
            //Debug.Log(new Vector3((line - shipsPerLine) * distCol, 0, -(line - 1) * distLine));

            ship.GetComponent<OffsetPursue>().leader = leader.GetComponent<Boid>();

            // There are line * 2 + 1 ships per line
            if (shipsPerLine > line * 2 - 1) {
                line++;
                shipsPerLine = 0;
            }

            ships.Add(ship.GetComponent<Boid>());
            shipsPerLine++;

            if (i == 0) {
                mainCamera.target = ship;
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
