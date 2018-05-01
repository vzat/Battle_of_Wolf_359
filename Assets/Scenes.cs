using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;

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
    VideoManager videoManager;

    public bool videoPlayed;

    public override void Enter() {
        SceneManager.LoadScene("scene1");
        fleetManager = owner.GetComponent<FleetManager>();

        // Get VideoManager
        videoManager = GameObject.Find("VideoManager").GetComponent<VideoManager>();

        // Set the first 3 ships to attack
        for (int i = 0; i < fleetManager.ships.Count && i < 3; i++) {
            Boid ship = fleetManager.ships[i];
            ship.GetComponent<StateMachine>().ChangeState(new AttackState(fleetManager.borg));
            ship.maxSpeed = Random.Range(20.0f, 30.0f);
            ship.StartCoroutine(ship.ChangeSpeed());
        }

        // Change Camera Target
        FollowCamera mainCamera = Camera.main.GetComponent<FollowCamera>();
        mainCamera.target = fleetManager.GetComponent<GameObject>();

        videoPlayed = false;

        fleetManager.audioSource.Stop();
        fleetManager.audioSource.clip = fleetManager.battleMusic;
        fleetManager.audioSource.loop = true;
        fleetManager.audioSource.Play();

        //foreach (Ship ship in fleetManager.shipComp) {
        //    ship.audioSource.loop = true;
        //    ship.audioSource.clip = fleetManager.flybySound;
        //    ship.audioSource.Play();
        //}
    }

    public override void Update() {
        if (videoPlayed && !videoManager.playingVideo) {
            owner.ChangeState(new Scene2());
        }

        if (!videoPlayed && !videoManager.playingVideo && Vector3.Distance(fleetManager.ships[1].transform.position, fleetManager.borg.transform.position) < 35.0f) {
            videoManager.PlayVideo("./Assets/StreamingAssets/Locutus_of_Borg.mp4");
            videoPlayed = true;
        }
    }

    public override void Exit() {
    }
}

class Scene2 : State {
    FleetManager fleetManager;
    //VideoManager videoManager;

    //public bool videoPlayed;

    bool isMelbourneDestroyed = false;

    IEnumerator LoadScene() {
        AsyncOperation asyncSceneChange = SceneManager.LoadSceneAsync("scene2", LoadSceneMode.Single);

        while (!asyncSceneChange.isDone) {
            yield return null;
        }

        GameObject camera = GameObject.Find("Main Camera");
        FollowShip followShip = camera.GetComponent<FollowShip>();
        followShip.enemy = fleetManager.borg;
        followShip.ship = fleetManager.ships[1].gameObject;
        followShip.shipComponent = followShip.ship.GetComponent<Ship>();

        //fleetManager.borg.GetComponent<Borg>().attack = true;
    }

    IEnumerator DestroyMelbourne() {
        yield return new WaitForSeconds(Random.Range(5, 10));

        Borg borg = fleetManager.borg.GetComponent<Borg>();
        borg.capturedShip = borg.ships[1];
        borg.capturedShip.GetComponent<StateMachine>().ChangeState(new CapturedState(borg.gameObject));
        borg.tractorBeamAudioSource.Play();

        yield return new WaitForSeconds(Random.Range(2, 3));

        borg.targetShip = borg.ships[1];
        //borg.cuttingBeamAudioSource.Play();
        borg.cuttingBeamAudioSource.PlayOneShot(borg.cuttingBeamSound);

        yield return new WaitForSeconds(Random.Range(3, 4));

        borg.capturedShip.GetComponent<Ship>().structuralIntegrity = -100.0f;
        borg.capturedShip = null;
        borg.targetShip = null;
        borg.tractorBeamAudioSource.Stop();
        borg.cuttingBeamAudioSource.Stop();

        yield return new WaitForSeconds(0.5f);

        isMelbourneDestroyed = true;
    }

    public override void Enter() {
        fleetManager = owner.GetComponent<FleetManager>();

        // Get VideoManager
        //videoManager = GameObject.Find("VideoManager").GetComponent<VideoManager>();
        //videoManager.playingVideo = false;

        // Load the scene and wait for it to initiallise
        fleetManager.StartCoroutine(LoadScene());

        //videoPlayed = false;

        // Destroy the USS Melbourne
        fleetManager.StartCoroutine(DestroyMelbourne());
    }

    public override void Update() {
        if (isMelbourneDestroyed) {
            owner.ChangeState(new Scene3());
        }
        //if (videoPlayed && !videoManager.playingVideo) {
        //    owner.ChangeState(new Scene3());
        //}

        //// The ship was hit a few times
        //if (!videoPlayed && !videoManager.playingVideo && ussSaratoga.structuralIntegrity + 151 < ussSaratoga.maxStructuralIntegrity) {
        //    ussSaratoga.structuralIntegrity = 1;
        //    videoManager.PlayVideo("./Assets/StreamingAssets/Sisko_Escape_Pod.mp4");
        //    videoPlayed = true;
        //}
    }

    public override void Exit() {
    }
}

class Scene3 : State {
    FleetManager fleetManager;
    VideoManager videoManager;

    FollowShip followShip;

    bool videoPlayed = false;

    IEnumerator LoadScene() {
        AsyncOperation asyncSceneChange = SceneManager.LoadSceneAsync("scene3", LoadSceneMode.Single);

        while (!asyncSceneChange.isDone) {
            yield return null;
        }

        GameObject camera = GameObject.Find("Main Camera");
        followShip = camera.GetComponent<FollowShip>();
        followShip.enemy = fleetManager.borg;
        followShip.ship = fleetManager.ships[0].gameObject;
        followShip.shipComponent = followShip.ship.GetComponent<Ship>();

        //fleetManager.borg.GetComponent<Borg>().attack = true;
    }

    IEnumerator DestroySaratoga() {
        yield return new WaitForSeconds(Random.Range(5, 10));

        Borg borg = fleetManager.borg.GetComponent<Borg>();
        borg.capturedShip = borg.ships[0];
        borg.capturedShip.GetComponent<StateMachine>().ChangeState(new CapturedState(borg.gameObject));
        borg.tractorBeamAudioSource.Play();

        yield return new WaitForSeconds(Random.Range(2, 4));

        borg.capturedShip.GetComponent<Ship>().structuralIntegrity = 50.0f;

        yield return new WaitForSeconds(1);

        // Follow escape pod
        GameObject escapePod = GameObject.FindGameObjectsWithTag("EscapePod")[0];
        followShip.enemy = escapePod;
        followShip.ship = escapePod;
        followShip.shipComponent = null;

        yield return new WaitForSeconds(Random.Range(3, 4));

        videoManager.PlayVideo("./Assets/StreamingAssets/Sisko_Escape_Pod.mp4");
        videoPlayed = true;

        yield return new WaitForSeconds(1);

        borg.capturedShip.GetComponent<Ship>().structuralIntegrity = -100.0f;
        followShip.ship = borg.gameObject;
        borg.capturedShip = null;
        borg.tractorBeamAudioSource.Stop();
    }

    public override void Enter() {
        fleetManager = owner.GetComponent<FleetManager>();

        // Get VideoManager
        videoManager = GameObject.Find("VideoManager").GetComponent<VideoManager>();

        // Load the scene and wait for it to initiallise
        fleetManager.StartCoroutine(LoadScene());

        // Destroy the USS Saratoga
        fleetManager.StartCoroutine(DestroySaratoga());
    }

    public override void Update() {
        if (videoPlayed && !videoManager.playingVideo) {
            owner.ChangeState(new Scene4());
        }
    }

    public override void Exit() {
    }
}

class Scene4 : State {
    FleetManager fleetManager;
    int initialShipsAlive;
    int shipsToAttack;

    IEnumerator LoadScene() {
        AsyncOperation asyncSceneChange = SceneManager.LoadSceneAsync("scene4", LoadSceneMode.Single);

        while (!asyncSceneChange.isDone) {
            yield return null;
        }

        GameObject camera = GameObject.Find("Main Camera");
        FollowCamera followCamera = camera.GetComponent<FollowCamera>();
        //followCamera.target = fleetManager.ships[0].gameObject;
        followCamera.target = fleetManager.borg;

        Borg borg = fleetManager.borg.GetComponent<Borg>();
        borg.attack = true;
        borg.StartAI();
    }

    public override void Enter() {
        fleetManager = owner.GetComponent<FleetManager>();

        // Get inital no of ships alive
        initialShipsAlive = fleetManager.ships.Count;

        // Set more ships to attack
        shipsToAttack = Random.Range(3, 5);
        for (int i = 1; i < fleetManager.ships.Count && i < shipsToAttack; i++) {
            Boid ship = fleetManager.ships[i];
            ship.GetComponent<StateMachine>().ChangeState(new AttackState(fleetManager.borg));
            ship.maxSpeed = Random.Range(20.0f, 30.0f);
            ship.StartCoroutine(ship.ChangeSpeed());
        }

        // Load the scene and wait for it to initiallise
        fleetManager.StartCoroutine(LoadScene());
    }

    public override void Update() {
        if (initialShipsAlive - fleetManager.ships.Count >= shipsToAttack) {
            owner.ChangeState(new Scene5());
        }
    }

    public override void Exit() {
    }
}

class Scene5 : State {
    FleetManager fleetManager;
    GameObject camera;
    FollowShip followShip;
    FollowShipsAlive followShipsAlive;

    int initialShipsAlive;
    int shipsToAttack;

    IEnumerator changeCamera;

    IEnumerator LoadScene() {
        AsyncOperation asyncSceneChange = SceneManager.LoadSceneAsync("scene5", LoadSceneMode.Single);

        while (!asyncSceneChange.isDone) {
            yield return null;
        }

        camera = GameObject.Find("Main Camera");
        followShip = camera.GetComponent<FollowShip>();
        followShipsAlive = camera.GetComponent<FollowShipsAlive>();

        changeCamera = ChangeCamera();
        fleetManager.StartCoroutine(changeCamera);
    }

    IEnumerator ChangeCamera() {
        while (true) {
            if (Random.Range(-1, 1) >= 0) {
                // Switch to Follow Ship Camera
                followShip.enabled = true;
                followShipsAlive.enabled = false;

                // Setup Camera
                followShip.enemy = fleetManager.borg;

                int attackingShips = shipsToAttack - (fleetManager.ships.Count - initialShipsAlive);
                followShip.ship = fleetManager.ships[(int)Random.Range(0, attackingShips)].gameObject;
                followShip.distance = Random.Range(25, 50);
            }
            else {
                // Switch to Follow Ships Alive Camera
                followShip.enabled = false;
                followShipsAlive.enabled = true;
                followShipsAlive.transform.position = Random.insideUnitSphere * Random.Range(30, 50);
            }

            yield return new WaitForSeconds(Random.Range(5, 10));
        }
    }

    void NextWave() {
        // Set more ships to attack
        shipsToAttack = Random.Range(3, 10);

        int attackingShips = 0;
        for (int i = 0; i < fleetManager.ships.Count && attackingShips < shipsToAttack; i++) {
            Boid ship = fleetManager.ships[i];
            StateMachine stateMachine = ship.GetComponent<StateMachine>();

            if (stateMachine.state.GetType().Name == "IdleState") {
                ship.GetComponent<StateMachine>().ChangeState(new AttackState(fleetManager.borg));
                ship.maxSpeed = Random.Range(20.0f, 30.0f);
                ship.StartCoroutine(ship.ChangeSpeed());
                attackingShips++;
            }
        }

        // Remove the no of ships attacking if the no exceeds the total no of ships
        shipsToAttack = attackingShips < shipsToAttack ? attackingShips : shipsToAttack;

        initialShipsAlive = fleetManager.ships.Count;
    }

    public override void Enter() {
        fleetManager = owner.GetComponent<FleetManager>();

        // Get inital no of ships alive
        initialShipsAlive = fleetManager.ships.Count;

        // Create the next attacking wave
        NextWave();

        // Load the scene and wait for it to initiallise
        fleetManager.StartCoroutine(LoadScene());
    }

    public override void Update() {
        if (initialShipsAlive - fleetManager.ships.Count >= shipsToAttack) {
            NextWave();
        }

        if (fleetManager.ships.Count == 0) {
            owner.ChangeState(new Scene6());
        }
    }

    public override void Exit() {
        fleetManager.StopCoroutine(changeCamera);
    }
}

class Scene6 : State {
    FleetManager fleetManager;

    IEnumerator LoadScene() {
        AsyncOperation asyncSceneChange = SceneManager.LoadSceneAsync("scene6", LoadSceneMode.Single);

        while (!asyncSceneChange.isDone) {
            yield return null;
        }

        GameObject camera = GameObject.Find("Main Camera");
        FollowCamera followCamera = camera.GetComponent<FollowCamera>();

        followCamera.target = fleetManager.borg;
    }

    public override void Enter() {
        fleetManager = owner.GetComponent<FleetManager>();

        // Load the scene and wait for it to initiallise
        fleetManager.StartCoroutine(LoadScene());
    }

    public override void Update() {
    }

    public override void Exit() {
    }
}