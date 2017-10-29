﻿using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(UVController))]
public class UVControllerEditor : Editor {
	UVController uvc;
	public override void OnInspectorGUI() {
		base.OnInspectorGUI ();

		if(!Application.isPlaying)
			return;
			
		uvc = (UVController)target;
		if (GUI.changed) {
			uvc.Setup ();
			uvc.ChangeAsset (uvc.Coordinates, false);
		}
	}
}
