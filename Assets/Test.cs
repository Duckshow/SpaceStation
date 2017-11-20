using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEditor;

public class Test : MonoBehaviour
{

    public Vector2 InputDot;

    private CustomLight light;

    void Start(){
        light = FindObjectOfType<CustomLight>();
    }

    Vector2 C;
    float tan;
    int deg;
    byte reportAngle;
    void Update(){
        tan = GetAngleClockwise(light.transform.position, transform.position, 1);
        Debug.DrawLine((Vector2)light.transform.position, (Vector2)transform.position, Color.red, Mathf.Infinity);
        Debug.Log(tan);
    }

    [EasyButtons.Button]
    public void PrintValues(){
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

    static float GetAngle(Vector2 _pos1, Vector2 _pos2, Vector2 _referenceAngle, int maxAngle)
    { // TODO: replace with GetAngleClockwise!
        return maxAngle * (0.5f * (1 + Vector2.Dot(
                                            _referenceAngle,
                                            Vector3.Normalize(_pos1 - _pos2))));
    }
    static float GetAngleClockwise(Vector2 _pos1, Vector2 _pos2, int _maxAngle)
    {
        // gets the angle between _pos1 and _pos2, ranging from 0 to _maxAngle.
        // the angle goes all-the-way-around, like a clock and unlike a dot-product.

        float _vertical = (0.5f * (1 + Vector2.Dot(Vector2.down, Vector3.Normalize(_pos1 - _pos2))));
        float _horizontal = Vector2.Dot(Vector2.right, Vector3.Normalize(_pos1 - _pos2));

        _vertical = (_vertical * (_maxAngle * 0.5f));
        if (_horizontal < 0)
            _vertical = Mathf.Abs(_vertical - _maxAngle);

        return _vertical;
    }

    public static void ClearLogConsole()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
        System.Type logEntries = assembly.GetType("UnityEditorInternal.LogEntries");
        MethodInfo clearConsoleMethod = logEntries.GetMethod("Clear");
        clearConsoleMethod.Invoke(new object(), null);
    }
}
