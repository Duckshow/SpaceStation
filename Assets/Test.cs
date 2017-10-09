using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEditor;

public class Test : MonoBehaviour
{

    public Vector2 InputDot;

    private CustomLight light;

    void Start()
    {
        light = FindObjectOfType<CustomLight>();
    }

    Vector2 C;
    float tan;
    int deg;
    byte reportAngle;
    void Update()
    {
        // tan = (((Vector2.Dot(Vector2.left, Vector3.Normalize((Vector2)light.transform.position - (Vector2)transform.position)) + 1) * 0.5f));
        // Debug.DrawLine((Vector2)light.transform.position, (Vector2)transform.position, Color.red, Mathf.Infinity);
        // Debug.Log(tan);

        Debug.Log((int)GetAngleVertical(transform.position, light.transform.position, 360));
    }

    [EasyButtons.Button]
    public void PrintValues()
    {
        ClearLogConsole();

        Vector2 nrmT = new Vector2(0, 1);
        Vector2 nrmTR = new Vector2(0.5f, 0.5f);
        Vector2 nrmR = new Vector2(1, 0);
        Vector2 nrmBR = new Vector2(0.5f, -0.5f);
        Vector2 nrmB = new Vector2(0, -1);
        Vector2 nrmBL = new Vector2(-0.5f, -0.5f);
        Vector2 nrmL = new Vector2(-1, 0);
        Vector2 nrmTL = new Vector2(-0.5f, 0.5f);

        InputDot = nrmTL;
        Debug.LogFormat("(x: {9}, y: {10})\n{0} - {1} - {2}\n{3} - {4} - {5}\n{6} - {7} - {8}\n.", DoMath(nrmTL), DoMath(nrmT), DoMath(nrmTR), DoMath(nrmL), DoMath(Vector2.zero), DoMath(nrmR), DoMath(nrmBL), DoMath(nrmB), DoMath(nrmBR), nrmTL.x, nrmTL.y);

        InputDot = nrmT;
        Debug.LogFormat("(x: {9}, y: {10})\n{0} - {1} - {2}\n{3} - {4} - {5}\n{6} - {7} - {8}\n.", DoMath(nrmTL), DoMath(nrmT), DoMath(nrmTR), DoMath(nrmL), DoMath(Vector2.zero), DoMath(nrmR), DoMath(nrmBL), DoMath(nrmB), DoMath(nrmBR), nrmT.x, nrmT.y);

        InputDot = nrmTR;
        Debug.LogFormat("(x: {9}, y: {10})\n{0} - {1} - {2}\n{3} - {4} - {5}\n{6} - {7} - {8}\n.", DoMath(nrmTL), DoMath(nrmT), DoMath(nrmTR), DoMath(nrmL), DoMath(Vector2.zero), DoMath(nrmR), DoMath(nrmBL), DoMath(nrmB), DoMath(nrmBR), nrmTR.x, nrmTR.y);

        InputDot = nrmR;
        Debug.LogFormat("(x: {9}, y: {10})\n{0} - {1} - {2}\n{3} - {4} - {5}\n{6} - {7} - {8}\n.", DoMath(nrmTL), DoMath(nrmT), DoMath(nrmTR), DoMath(nrmL), DoMath(Vector2.zero), DoMath(nrmR), DoMath(nrmBL), DoMath(nrmB), DoMath(nrmBR), nrmR.x, nrmR.y);

        InputDot = nrmBR;
        Debug.LogFormat("(x: {9}, y: {10})\n{0} - {1} - {2}\n{3} - {4} - {5}\n{6} - {7} - {8}\n.", DoMath(nrmTL), DoMath(nrmT), DoMath(nrmTR), DoMath(nrmL), DoMath(Vector2.zero), DoMath(nrmR), DoMath(nrmBL), DoMath(nrmB), DoMath(nrmBR), nrmBR.x, nrmBR.y);

        InputDot = nrmB;
        Debug.LogFormat("(x: {9}, y: {10})\n{0} - {1} - {2}\n{3} - {4} - {5}\n{6} - {7} - {8}\n.", DoMath(nrmTL), DoMath(nrmT), DoMath(nrmTR), DoMath(nrmL), DoMath(Vector2.zero), DoMath(nrmR), DoMath(nrmBL), DoMath(nrmB), DoMath(nrmBR), nrmB.x, nrmB.y);

        InputDot = nrmBL;
        Debug.LogFormat("(x: {9}, y: {10})\n{0} - {1} - {2}\n{3} - {4} - {5}\n{6} - {7} - {8}\n.", DoMath(nrmTL), DoMath(nrmT), DoMath(nrmTR), DoMath(nrmL), DoMath(Vector2.zero), DoMath(nrmR), DoMath(nrmBL), DoMath(nrmB), DoMath(nrmBR), nrmBL.x, nrmBL.y);

        InputDot = nrmL;
        Debug.LogFormat("(x: {9}, y: {10})\n{0} - {1} - {2}\n{3} - {4} - {5}\n{6} - {7} - {8}\n.", DoMath(nrmTL), DoMath(nrmT), DoMath(nrmTR), DoMath(nrmL), DoMath(Vector2.zero), DoMath(nrmR), DoMath(nrmBL), DoMath(nrmB), DoMath(nrmBR), nrmL.x, nrmL.y);
    }

    float DoMath(Vector2 nrm)
    {
        Vector2 diff = nrm - InputDot;
        return Mathf.Min(Mathf.Ceil(Mathf.Abs(nrm.x) + Mathf.Abs(nrm.y)), 1 - Mathf.Clamp01(Mathf.Floor(Mathf.Max(Mathf.Abs(diff.x), Mathf.Abs(diff.y)))));
    }

    public static void ClearLogConsole()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
        System.Type logEntries = assembly.GetType("UnityEditorInternal.LogEntries");
        MethodInfo clearConsoleMethod = logEntries.GetMethod("Clear");
        clearConsoleMethod.Invoke(new object(), null);
    }

    static float GetAngleHorizontal(Vector2 _pos1, Vector2 _pos2, int _maxAngle){
        float _horizontal    = 0.5f * (1 + Vector2.Dot(Vector2.left, Vector3.Normalize(_pos1 - _pos2)));
        float _vertical      =             Vector2.Dot(Vector2.up,   Vector3.Normalize(_pos1 - _pos2));

        _horizontal *= _maxAngle * 0.5f;
        if(_vertical < 0)
            _horizontal -= _maxAngle * 0.5f;

        return _horizontal;
    }
    static float GetAngleVertical(Vector2 _pos1, Vector2 _pos2, int _maxAngle){
        float _vertical     = (0.5f * (1 + Vector2.Dot(Vector2.up,   Vector3.Normalize(_pos1 - _pos2))));
        float _horizontal   =              Vector2.Dot(Vector2.right, Vector3.Normalize(_pos1 - _pos2));
admwp // I'm trying to get an angle between 0-360 to use to determine whether a simple gridcast will hit its own collider or not.
        _vertical = (_vertical * _maxAngle);
        if (_horizontal < 0)
            _vertical -= _maxAngle;

        return _vertical;
    }
}
