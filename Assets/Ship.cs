using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour {

    public Material phaserMaterial;
    public GameObject torpedoPrefab;
    public bool captured = false;
    public float maxStructuralIntegrity = 100.0f;
    public float structuralIntegrity = 100.0f;
    public ParticleSystem explosionPrefab;
    public FleetManager fleetManager;

    bool firePhaser = false;
    bool fireTorpedoes = false;

    Vector3 firePos = Vector3.zero;

    LineRenderer phaser;

    [HideInInspector]
    public bool destroyed = false;
    public bool escapePodsReleased = false;

    IEnumerator fireWeapons;

    // Use this for initialization
    void Start () {
        gameObject.AddComponent<LineRenderer>();
        phaser = GetComponent<LineRenderer>();

        phaser.material = phaserMaterial;
        phaser.startWidth = 0.1f;
        phaser.endWidth = 0.1f;

        fireWeapons = FireWeapons();
        StartCoroutine(fireWeapons);
    }
	
	// Update is called once per frame
	void Update () {
        // Remove GameObject from scene
        //if (destroyed && Vector3.Distance(this.transform.position, Vector3.zero) > 100.0f) {
        //    //Destroy(this);
        //}

        if (structuralIntegrity < 0.0f && !destroyed) {
            // Ship Destroyed
            destroyed = true;

            // Stop firing weapons
            StopCoroutine(fireWeapons);
            firePhaser = false;
            fireTorpedoes = false;

            ParticleSystem explosion = Instantiate(explosionPrefab);
            explosion.transform.position = this.transform.position;
            explosion.Play();

            this.GetComponent<StateMachine>().ChangeState(new DestroyedState(explosion.main.duration));

            Destroy(explosion, explosion.main.duration);
        }
        else if (!escapePodsReleased && structuralIntegrity < 100.0f && structuralIntegrity > 0.0f) {
            // Release Escape Pods
            escapePodsReleased = true;
            int noEscapePods = Random.Range(3, 5);

            for (int i = 0; i < noEscapePods; i++) {
                GameObject escapePod = Instantiate(fleetManager.escapePodPrefab);
                escapePod.transform.parent = fleetManager.transform;
                escapePod.transform.position = this.transform.position;
                escapePod.AddComponent<EscapePod>();

                Flee escapePodFlee = escapePod.GetComponent<Flee>();
                escapePodFlee.enabled = true;
                escapePodFlee.target = new Vector3(Random.Range(0, 5), Random.Range(0, 5), Random.Range(0, 5));
            }
        }

        // Phaser
        if (firePhaser) {
            phaser.SetPosition(0, transform.position);
            phaser.SetPosition(1, firePos);
        }
        else {
            phaser.SetPosition(0, transform.position);
            phaser.SetPosition(1, transform.position);
        }
    }

    IEnumerator FireWeapons() {
        yield return new WaitForSeconds(2);
        while (true) {
            if (Vector3.Distance(this.transform.position, Vector3.zero) < 30.0f && !captured) {
                firePos = Random.insideUnitSphere * 3.0f;

                if (Random.Range(-1, 1) >= 0) {
                    firePhaser = true;
                    yield return new WaitForSeconds(Random.Range(1, 2));
                    firePhaser = false;
                }
                else {
                    fireTorpedoes = true;

                    int noTorpedoes = Random.Range(1, 4);

                    for (int i = 0; i < noTorpedoes; i++) {
                        GameObject torpedo = Instantiate<GameObject>(torpedoPrefab);
                        torpedo.AddComponent<Torpedo>();

                        torpedo.transform.position = this.transform.position;

                        Torpedo torpedoObj = torpedo.GetComponent<Torpedo>();
                        torpedoObj.destination = firePos;

                        yield return new WaitForSeconds(0.1f);
                    }

                    fireTorpedoes = false;
                }
            }
            yield return new WaitForSeconds(Random.Range(1, 3));
        }
    }
}
