using UnityEngine;

public class TrailObject : MonoBehaviour {

    [SerializeField] private Vector3 CurrentVelocity;
    [SerializeField] private float FollowSpeed;
    [SerializeField] private float RotateSpeed;

    [HideInInspector] public ActorOrientation.OrientationEnum Orientation;
    [HideInInspector] public bool ForceTargetRotation = false;

    private const float VERTICAL_TARGETROTATION_MOVEDOWN = 180;
    private const float HORIZONTAL_TARGETROTATION = 90;
    private const float HORIZONTAL_TARGETPOSITION = 0.15f;

    private Vector3 targetPosition;
    private Vector3 targetRotation;
    private Vector3 originalPositionLocal;
    private Vector3 previousPosition;

    private bool usePosition = false;
    private bool useRotation = false;

    private Quaternion targetAngle;
    private float targetEulerX;
    private float targetEulerZ;
    private float currentTargetRotation;

    private float eulerZ;


    void Start() {
        CurrentVelocity = Vector3.zero;
        originalPositionLocal = transform.localPosition;
        previousPosition = transform.position;
    }
    void LateUpdate() {

        // reset temporary stuff
        targetEulerX = 0;
        targetEulerZ = 0;
        useRotation = false;
        usePosition = false;

        // reset changes made by parent
        CurrentVelocity = transform.position - previousPosition;
        transform.position = previousPosition;

        float _angle = (Mathf.Rad2Deg * Mathf.Atan2(-CurrentVelocity.normalized.x, CurrentVelocity.normalized.y));
        if (ForceTargetRotation)
            targetEulerZ = _angle;
        else
            targetEulerZ = Mathf.Lerp(0, _angle, CurrentVelocity.magnitude * 10);
        targetAngle = Quaternion.Euler(targetEulerX, 0, targetEulerZ);

        useRotation = true;
        usePosition = true;

        if (useRotation) {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle, Mathf.Clamp01(Time.smoothDeltaTime * RotateSpeed * Mathf.Max(0.01f, Mathf.Abs(CurrentVelocity.magnitude))));
        }

        if (usePosition) {
            // the position will move in tandem with the rotation so as to stay in sync
            eulerZ = transform.eulerAngles.z;
            if (eulerZ > 180)
                eulerZ -= 360;

            targetPosition = new Vector3(0, HORIZONTAL_TARGETPOSITION, 0);
            targetPosition = transform.parent.position + Vector3.Lerp(originalPositionLocal, targetPosition, Mathf.Clamp01(Mathf.Abs(eulerZ) / 90) + Mathf.Max(0, CurrentVelocity.y * 10));
            transform.position += (targetPosition - transform.position) * Time.smoothDeltaTime * FollowSpeed;
        }
        else {
            // just follow the original local position
            transform.position += ((transform.parent.position + originalPositionLocal) - transform.position) * Time.smoothDeltaTime * FollowSpeed;
        }

        // save position for reset
        previousPosition = transform.position;
    }
}
