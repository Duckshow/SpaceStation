using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

    private CustomLight light;

    void Start () {
        light = FindObjectOfType<CustomLight>();
    }
	
	void Update () {
        // angle (0-360)
  //      Vector2 C = light.transform.position - transform.position;
  //      float _angle = Mathf.Atan2(C.y, C.x);
  //      int _deg = Mathf.RoundToInt(90 + (_angle * Mathf.Rad2Deg));
		//if(_deg < 0)
  //          _deg += 360;

  //      ulong degrees = 0;
  //      int _digits = MathfExtensions.Digits(_deg);
		//if(_digits == 1)
  //          degrees = MathfExtensions.Concatenate(100, _deg);
		//else if(_digits == 2)
  //          degrees = MathfExtensions.Concatenate(10, _deg);
		//else if (_digits == 3)
  //          degrees = MathfExtensions.Concatenate(1, _deg);

  //      // distance
  //      int _dist = Mathf.Min(Mathf.RoundToInt(Vector2.Distance(transform.position, light.transform.position)), 999);
  //      ulong distance = 0;
  //      _digits = MathfExtensions.Digits(_dist);
  //      if (_digits == 1)
  //          distance = MathfExtensions.Concatenate(100, _dist);
  //      else if (_digits == 2)
  //          distance = MathfExtensions.Concatenate(10, _dist);
  //      else if (_digits == 3)
  //          distance = MathfExtensions.Concatenate(1, _dist);

  //      Debug.Log(distance);
  //      Debug.DrawLine(transform.position, light.transform.position, Color.red, Time.deltaTime);
    }
}
