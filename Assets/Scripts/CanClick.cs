using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class CanClick : MonoBehaviour {

	public static List<CanClick> AllClickables = new List<CanClick>();

	public float ClickableRange = 10;
	[HideInInspector] public bool Enabled = true;

	public delegate void DefaultEvent();
	public DefaultEvent _OnWithinRange;
	public DefaultEvent _OnLeftClickRelease;
	public DefaultEvent _OnRightClickRelease;


	void OnEnable() {
		AllClickables.Add(this);
	}
	void OnDisable() {
		AllClickables.Remove(this);        
	}

	public void OnLeftClickRelease(){
		if (!Enabled)
			return;

		if (_OnLeftClickRelease != null)
			_OnLeftClickRelease ();
	}

	public void OnRightClickRelease() {
		if (!Enabled)
			return;
		
		if (_OnRightClickRelease != null)
			_OnRightClickRelease ();
	}
}
