using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SuperDebug : MonoBehaviour {

	class DebugObject{
		public Vector3 Position { get; private set; }
        public Color TextColor { get; private set; }
		public object[] Things { get; private set; }

        public DebugObject (Vector3 _pos, Color _color, params object[] _things) {
			Position = _pos;
            TextColor = _color;
            Things = _things;
        }
	}

    private static List<DebugObject> sThingsToDebug = new List<DebugObject>();

    public static void Mark(Vector3 _pos, Color _color, params object[] _things) {
        sThingsToDebug.Add(new DebugObject(_pos, _color, _things));
    }
	public static void MarkPoint(Vector3 _pos, Color _color, float _markRadius = 0.1f){
		Vector3 _above 	= _pos + new Vector3(0, _markRadius);
		Vector3 _right 	= _pos + new Vector3(_markRadius, 0);
		Vector3 _below 	= _pos + new Vector3(0, -_markRadius);
		Vector3 _left 	= _pos + new Vector3(-_markRadius, 0);
		Debug.DrawLine(_above, _right, _color, Mathf.Infinity);
		Debug.DrawLine(_right, _below, _color, Mathf.Infinity);
		Debug.DrawLine(_below, _left, _color, Mathf.Infinity);
		Debug.DrawLine(_left, _above, _color, Mathf.Infinity);
	}
	
	public static void Log(Color color = new Color(), params object[] _stuff) {
		string _debugText = "";
		for (int i = 0; i < _stuff.Length; i++){
			_debugText += _stuff[i].ToString();
			if(i < _stuff.Length - 1) _debugText += "\n";
		}
		if(color == Color.clear) color = Color.white;
		Debug.Log(_debugText.Color(color));
	}
	public static void LogArray<T>(T[] _array, string _arrayName = "", Color color = new Color()) {
		string _debugText = _arrayName + ":\n";
		for (int i = 0; i < _array.Length; i++){
			_debugText += _array[i].ToString();
			if(i < _array.Length - 1) _debugText += "\n";
		}
		if(color == Color.clear) color = Color.white;
		Debug.Log(_debugText.Color(color));
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
            for (int j = 0; j < sThingsToDebug[i].Things.Length; j++)
                _text += sThingsToDebug[i].Things[j].ToString() + "\n";

            Handles.Label(sThingsToDebug[i].Position, _text, _style);
        }
	}
}
