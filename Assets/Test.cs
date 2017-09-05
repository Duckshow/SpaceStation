using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

    private CustomLight light;

    void Start () {
        light = FindObjectOfType<CustomLight>();
    }
	
	void Update () {
        float _dot = Vector2.Dot(Vector2.right, ((Vector2)light.transform.position - (Vector2)transform.position).normalized);
        // int _angle = Mathf.RoundToInt(Vector2.Angle((Vector2)transform.position, (Vector2)light.transform.position));
        // if (_dot < 0)
        //     _angle = 360 - _angle;


        Vector2 C = light.transform.position - transform.position;
        float _angle = Mathf.Atan2(C.y, C.x);
        int _deg = Mathf.RoundToInt(90 + (_angle * Mathf.Rad2Deg));
		if(_deg < 0)
            _deg += 360;

		dwji // PLACEHOLDER: just proof-of-concept. The 1 is to hold 0s in place. 1 must only be added on the first concatenated value.
        int _digits = MathfExtensions.Digits(_deg);
		if(_digits == 1)
            _deg = MathfExtensions.Concatenate(100, _deg);
		else if(_digits == 2)
            _deg = MathfExtensions.Concatenate(10, _deg);
		else if (_digits == 3)
            _deg = MathfExtensions.Concatenate(1, _deg);

        Debug.Log(_deg);
        Debug.DrawLine(transform.position, light.transform.position, Color.red, Time.deltaTime);
    }
}
