﻿using UnityEngine;
using System.Collections;

public class DefaultEngineDriver : IEngineDriver {
	private int powerShip;
	private float actualForce;
	private float quickSmooth = 10f;
	private float smoothForce;
	private float lastInput;


	public DefaultEngineDriver(int power) {
		powerShip = power;
		smoothForce = Mathf.Pow(power / 5f, 2f) * quickSmooth;
	}

	/// <summary>
	/// Gets the force of the engine.
	/// </summary>
	/// <returns>The force. Range [-1, 1]</returns>
	public virtual float GetForce() {
		return actualForce;
	}

	/// <summary>
	/// Gets the power of the ship prototype.
	/// </summary>
	/// <returns>The power. Range [1, 5]</returns>
	public int GetPower() {
		return powerShip;
	}

	public void Update() {
		float input = Input.GetAxis("Vertical");

		if (input >= 0 && input >= actualForce) {
			actualForce = Mathf.Lerp(actualForce, input, smoothForce * Time.deltaTime);
		}
		else if (input < 0 && input <= actualForce) {
			actualForce = Mathf.Lerp(actualForce, input, smoothForce * Time.deltaTime);
		}
		else {
			actualForce = input;
		}

	}
}