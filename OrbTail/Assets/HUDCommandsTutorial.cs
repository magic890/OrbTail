﻿using UnityEngine;
using System.Collections;

public class HUDCommandsTutorial : MonoBehaviour {
	private static string pcTutorialPrefab = "Prefabs/HUD/Tutorial/CommandsPCTutorial";
	private static string mobileTutorialPrefab = "Prefabs/HUD/Tutorial/CommandsMobileTutorial";

	// Use this for initialization
	void Start () {
		string prefabPath;

		if (SystemInfo.deviceType == DeviceType.Handheld) {
			prefabPath = mobileTutorialPrefab;
		}
		else {
			Debug.Log("PCCCCC");
			prefabPath = pcTutorialPrefab;
		}

		GameObject commandTab = (GameObject) GameObject.Instantiate(Resources.Load(prefabPath));

		Vector3 prevPosition = commandTab.transform.position;
		commandTab.transform.parent = gameObject.transform;
		commandTab.transform.localPosition = prevPosition;
	
	}
}
