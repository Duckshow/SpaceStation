using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lamp : NodeObject {
	[SerializeField] private int radius = 10;
	public int GetRadius() { return radius; }

	[SerializeField] private Color32 color = new Color32(255, 255, 255, 255);
	public Color32 GetColor() { return color; }

	private bool isTurnedOn = true;


	public override bool IsUsingAwakeDefault() { return true; }
	public override void AwakeDefault() {
		LampManager.GetInstance().OnLampAwake(this);
	}

	public override bool IsUsingStartLate() { return true; }
	public override void StartLate() {
		if (isTurnedOn){
			OnTurnOn();
		}
		else{
			OnTurnOff();
		}
	}

	void OnDestroy() {
		LampManager.GetInstance().OnLampDestroy(this);
	}

	public void OnTurnOn(){
		LampManager.GetInstance().OnLampTurnOn(this);
	}

	public void OnTurnOff() {
		LampManager.GetInstance().OnLampTurnOff(this);
	}

	[EasyButtons.Button] 
	public void Toggle() { 
		isTurnedOn = !isTurnedOn;

		if (isTurnedOn){
			OnTurnOn();
		}
		else{
			OnTurnOff();
		}
	}
}
