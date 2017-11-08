﻿using UnityEngine;
using System.Collections;

public class OrbDeactivator : MonoBehaviour {
	//private EventLogger eventLogger;
	private float secondsDeactivated = 0.3f;

	// Use this for initialization
	void Start () {
        //TODO: fixme
        /*
		eventLogger = GameObject.FindGameObjectWithTag(Tags.Game).GetComponent<EventLogger>();
		eventLogger.EventFight += OnEventFight; 
         * */
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnEventFight(object sender, System.Collections.Generic.IList<GameObject> orbs, GameObject attacker, GameObject defender) {
		foreach (GameObject orb in orbs) {
			StartCoroutine( DisableCollisionTemporaryBetween(orb, defender));
		}
	}


	void OnDestroy() {
		//TODO: fixme
        //eventLogger.EventFight -= OnEventFight;
	}

	// TODO: the ship must be only one object
	private IEnumerator DisableCollisionTemporaryBetween(GameObject gameObject1, GameObject gameObject2) {
		Physics.IgnoreCollision(gameObject1.GetComponent<Collider>(), gameObject2.GetComponent<Collider>(), true);
		yield return new WaitForSeconds(secondsDeactivated);
		Physics.IgnoreCollision(gameObject1.GetComponent<Collider>(), gameObject2.GetComponent<Collider>(), false);
	}


}
