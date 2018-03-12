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
    public static void LogArray(Vector3 _pos, bool[,] _array) {
        for (int x = 0; x < _array.GetLength(0); x++){
            for (int y = 0; y < _array.GetLength(1); y++){
                Color32 _color = _array[x, y] ? Color.green : Color.red;
                Debug.DrawLine(_pos + new Vector3(x, y, 0), _pos + new Vector3(x + 1, y + 1, 0), _color, Mathf.Infinity);
                Debug.DrawLine(_pos + new Vector3(x, y + 1, 0), _pos + new Vector3(x + 1, y, 0), _color, Mathf.Infinity);

                Debug.DrawLine(_pos + new Vector3(x, y + 0.5f, 0), _pos + new Vector3(x + 1, y + 0.5f, 0), _color, Mathf.Infinity);
                Debug.DrawLine(_pos + new Vector3(x + 0.5f, y, 0), _pos + new Vector3(x + 0.5f, y + 1, 0), _color, Mathf.Infinity); 
            }
        }
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
