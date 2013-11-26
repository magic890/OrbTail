﻿using UnityEngine;
using System.Collections;

public class DefaultWheelDriver : IWheelDriver {
	private int steeringShip;
	private float adjustedSteer;
	

	public DefaultWheelDriver(int steering) {
		steeringShip = steering;
		adjustedSteer = Mathf.Sqrt(steering / 5f);
	}

	public virtual float GetDirection(float inputSteer) {
		// TODO: to implement
		return Input.GetAxis("Horizontal") * adjustedSteer;
	}

	public int GetSteering() {
		return steeringShip;
	}

	public void Update() {

	}



}
