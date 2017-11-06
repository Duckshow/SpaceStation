using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SuperDebug : MonoBehaviour {

	class DebugObject{
		public Vector3 Position { get; private set; }
        public Color TextColor { get; private set; }
		public string[] Text { get; private set; }

        public DebugObject (Vector3 _pos, Color _color, params string[] _text) {
			Position = _pos;
            TextColor = _color;
            Text = _text;
        }
	}

    private static List<DebugObject> sThingsToDebug = new List<DebugObject>();

    public static void Log(Vector3 _pos, Color _color, params string[] _text) {
        sThingsToDebug.Add(new DebugObject(_pos, _color, _text));
    }

    int fontSize = 0;
    void Update() {
        if (Camera.current != null) 
            fontSize = Mathf.Max(1, Mathf.RoundToInt(15 * (1 - (Mathf.Abs(Camera.current.transform.position.z) / 35))));
    }	

    void OnDrawGizmos() { 
		for (int i = 0; i < sThingsToDebug.Count; i++){
            GUIStyle _style = new GUIStyle();
            _style.normal.textColor = sThingsToDebug[i].TextColor;
            _style.fontSize = fontSize;

            string _text = "";
            for (int j = 0; j < sThingsToDebug[i].Text.Length; j++)
                _text += sThingsToDebug[i].Text[j] + "\n";

            Handles.Label(sThingsToDebug[i].Position, _text, _style);
        }
	}
}
