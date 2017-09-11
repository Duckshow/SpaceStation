using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

    private CustomLight light;

    void Start () {
        light = FindObjectOfType<CustomLight>();
    }

    Vector2 C;
    float tan;
    int deg;
    byte reportAngle;
    void Update () {
        //      C = transform.position - light.transform.position;
        //     tan = Mathf.Atan2(C.x, C.y);
        //     deg = Mathf.RoundToInt(180 + (tan * Mathf.Rad2Deg));
        //     if (deg > 360)
        //         deg -= 360;
        //     reportAngle = (byte)(((float)deg / (float)360) * 255);

        float dotX = Vector2.Dot(Vector2.left, Vector3.Normalize((Vector2)transform.position - (Vector2)light.transform.position));
        dotX = (dotX + 1) * 0.5f;
        float dotY = Vector2.Dot(Vector2.down, Vector3.Normalize((Vector2)transform.position - (Vector2)light.transform.position));
        dotY = (dotY + 1) * 0.5f;

        C = new Vector2(dotX, dotY);

        Debug.Log(C);
        Debug.DrawLine(transform.position, light.transform.position, Color.red, Time.deltaTime);
    }
}
