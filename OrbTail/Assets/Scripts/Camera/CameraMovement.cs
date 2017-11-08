﻿using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {

	public float distanceSmooth = 10f;	// The relative speed at which the camera will catch up.
	public float rotationSmooth = 10f;
	public float finalSmooth = 2f; 		// Speed of camera at the end of the game
	public float relDistancePos = 7f;
	public float relHighPos = 2.2f;
	public float targetUpOffset = 0f;

	private float currentDistanceSmooth;
	
	private Transform player;           // Reference to the player's transform.
	private Vector3 newPos;             // The position the camera is trying to reach.

    /// <summary>
    /// The camera target
    /// </summary>
    public GameObject Target { get; private set; }

	private FloatingObject FloatingComponent{ get;set; }

    public void LookAt(GameObject target)
    {

        Target = target;

        // Setting up the reference.
        player = target.transform;
        FloatingComponent = target.GetComponent<FloatingObject>();
	
    }

    void Start()
    {
		GameBuilder builder = GameObject.FindGameObjectWithTag(Tags.Master).GetComponent<GameBuilder>();
		builder.EventGameBuilt += OnGameBuilt;
		currentDistanceSmooth = distanceSmooth;
    }

	void Awake ()
	{
	}
	
	void FixedUpdate ()
	{
		if (FloatingComponent != null) {
			// The standard position of the camera is the relative position of the camera from the player.
			//Vector3 standardPos = player.position + relCameraPos;
			Vector3 arenaDown = FloatingComponent.ArenaDown;

			Vector3 standardPos = player.position - player.forward * relDistancePos + relHighPos * -arenaDown;

			if (Physics.Raycast(standardPos, player.position - standardPos, (player.position - standardPos).magnitude, Layers.Obstacles)) {
				standardPos = player.position + player.forward * relDistancePos / 4f + relHighPos * 5f * -arenaDown;
			}
		
			newPos = standardPos;
			// Lerp the camera's position between it's current position and it's new position.
			float newDistanceSmooth = distanceSmooth;
			if (Vector3.Dot(player.transform.forward, player.GetComponent<Rigidbody>().velocity) < 0) {
				newDistanceSmooth *= 10;
			}

			currentDistanceSmooth = Mathf.Lerp(currentDistanceSmooth, newDistanceSmooth, 0.3f * Time.deltaTime);

			transform.position = Vector3.Lerp(transform.position, newPos, currentDistanceSmooth * Time.deltaTime);

		
			// Make sure the camera is looking at the player.
			SmoothLookAt(arenaDown);
		}
	}
	
	
	bool ViewingPosCheck (Vector3 checkPos)
	{
		RaycastHit hit;
		
		// If a raycast from the check position to the player hits something...
		if(Physics.Raycast(checkPos, player.position - checkPos, out hit, (player.position - checkPos).magnitude - 0.5f))
			// ... if it is not the player...
			if(hit.transform != player)
				// This position isn't appropriate.
				return false;
		
		// If we haven't hit anything or we've hit the player, this is an appropriate position.
		newPos = checkPos;
		return true;
	}
	
	
	void SmoothLookAt (Vector3 arenaDown)
	{
		// Create a vector from the camera towards the player.
		Vector3 relPlayerPosition = player.position + player.transform.up * targetUpOffset - transform.position;

		// Create a rotation based on the relative position of the player being the forward vector.

		Quaternion lookAtRotation = Quaternion.LookRotation(relPlayerPosition, -arenaDown);
		
		// Lerp the camera's rotation between it's current rotation and the rotation that looks at the player.
		transform.rotation = Quaternion.Lerp(transform.rotation, lookAtRotation, rotationSmooth * Time.deltaTime);
	}

	private void OnGameBuilt(object sender) {
		Game game = GameObject.FindGameObjectWithTag(Tags.Game).GetComponent<Game>();
		game.EventEnd += OnEventEnd;
	}

	private void OnEventEnd(object sender, GameObject winner, int info) {
		if (winner != null) {
			distanceSmooth = finalSmooth;
			LookAt(winner);
		}
	}
}
