﻿using UnityEngine;
using System.Collections;

public class HUDBoostIndicatorHandler : MonoBehaviour {


	private PowerView boostView;
	private TextMesh textMesh;
	private float refreshTime = 0.2f;
	private const float animationTime = 1f;

	// Use this for initialization
	void Start () {
		GameBuilder builder = GameObject.FindGameObjectWithTag(Tags.Master).GetComponent<GameBuilder>();
		builder.EventGameBuilt += OnGameBuilt;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void OnGameBuilt(object sender) {
		Game game = GameObject.FindGameObjectWithTag(Tags.Game).GetComponent<Game>();
		game.EventEnd += OnEventEnd;
		game.EventStart += OnEventStart;
		game.EventEnd += OnEnd;

		GameObject player = game.ActivePlayer;
		boostView = player.GetComponent<PowerController>().GetPowerView(PowerGroups.Passive);
		textMesh = GetComponent<TextMesh>();
	}


	private void OnEventEnd(object sender, GameObject winner) {
		StopCoroutine("RefreshIndicator");
	}

	private void OnEventStart(object sender, int countdown) {

		if (countdown <= 0) {
			textMesh.color = Color.green;
			StartCoroutine("RefreshIndicator");
		}

	}

	private IEnumerator RefreshIndicator() {

		while (true) {
			float percentage = boostView.IsReady * 100f;
			textMesh.text = string.Format("∝ {0:0}%", percentage);
			textMesh.color = Color.Lerp(Color.red, Color.green, boostView.IsReady);
			yield return new WaitForSeconds(refreshTime);
		}

	}

	private void OnEnd(object sender, GameObject winner) {
		iTween.FadeTo(gameObject, 0f, animationTime);
	}




}