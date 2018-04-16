using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FleetManager : MonoBehaviour {

    public GameObject excelsiorPrefab;
    public GameObject nebulaPrefab;
    public GameObject mirandaPrefab;
    public GameObject apolloPrefab;
    public GameObject oberthPrefab;
    public GameObject ambassadorPrefab;

    [HideInInspector]
    public GameObject leader;
    public List<Boid> ships = new List<Boid>();

    StateMachine stateMachine;
    public int fleetNo = 40;
    public float distLine = 10.0f;
    public float distCol = 10.0f;
    public Vector3 leaderPos = new Vector3(0, 0, -200);

    int currentScene = 0;

    GameObject camera;

	// Use this for initialization
	void Start () {

        FollowCamera mainCamera = Camera.main.GetComponent<FollowCamera>();
        stateMachine = GetComponent<StateMachine>();
        stateMachine.ChangeState(new Scene0());

        leader = new GameObject();
        leader.AddComponent<Boid>();
        leader.AddComponent<Arrive>();
        leader.transform.parent = this.transform;
        leader.transform.position = leaderPos;
        leader.GetComponent<Arrive>().target = new Vector3(0, 0, -50);
        Object.DontDestroyOnLoad(this);

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
            ship.transform.position = leader.transform.position + new Vector3((shipsPerLine - line) * (distCol + Random.Range(0, 5)), Random.Range(-5, 5), - (line - 1) * (distLine + Random.Range(0, 5)));

            ship.GetComponent<StateMachine>().ChangeState(new FollowLeader(leader.GetComponent<Boid>()));

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

    public void PlayVideo (string videoUrl) {
        camera = GameObject.Find("Main Camera");
        var videoPlayer = camera.AddComponent<UnityEngine.Video.VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraNearPlane;
        videoPlayer.url = videoUrl;
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.loopPointReached += EndReached;
        videoPlayer.Play();
    }

    void EndReached(UnityEngine.Video.VideoPlayer vp) {
        Destroy(camera.GetComponent<UnityEngine.Video.VideoPlayer>());
    }
}

// Scenes
class Scene0 : State {
    FleetManager fleetManager;

    public override void Enter() {
        fleetManager = owner.GetComponent<FleetManager>();
    }

    public override void Update() {
        if (Vector3.Distance(fleetManager.leader.transform.position, new Vector3(0, 0, 0)) < 60.0f) {
            owner.ChangeState(new Scene1());
        }
    }

    public override void Exit() {
    }
}

class Scene1 : State {
    FleetManager fleetManager;

    public override void Enter() {
        SceneManager.LoadScene("scene1");
        fleetManager = owner.GetComponent<FleetManager>();

        FollowCamera mainCamera = Camera.main.GetComponent<FollowCamera>();
        mainCamera.target = fleetManager.GetComponent<GameObject>();
    }

    public override void Update() {
        //fleetManager.PlayVideo("./Assets/StreamingAssets/Locutus_of_Borg.mp4");
    }

    public override void Exit() {
    }
}