using UnityEngine;
using System.Collections;

public class MoveMorpher : MonoBehaviour {

    [SerializeField][Range(0, 1)]
    private float CurrentVelocity;

    [SerializeField]
    private Vector3 TargetPositionLocal;
    [SerializeField]
    private Vector3 TargetRotationLocal;

    private Vector3 originalPositionLocal;
    private Vector3 originalRotationLocal;


    void Start() {
        originalPositionLocal = transform.localPosition;
        originalRotationLocal = transform.localEulerAngles;
    }
    void LateUpdate() {
        transform.localPosition = Vector3.Lerp(originalPositionLocal, TargetPositionLocal, CurrentVelocity);
        transform.localEulerAngles = Vector3.Lerp(originalRotationLocal, TargetRotationLocal, CurrentVelocity);

    }
}
