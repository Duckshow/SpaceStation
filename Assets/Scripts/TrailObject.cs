using UnityEngine;

public class TrailObject : MonoBehaviour {

    [SerializeField] private Vector3 CurrentVelocity;
    [SerializeField] private float FollowSpeed;
    [SerializeField] private float RotateSpeed;

    [HideInInspector] public ActorOrientation.OrientationEnum Orientation;

    private const float VERTICAL_TARGETROTATION_MOVEDOWN = 175;
    private const float VERTICAL_TARGETROTATION_MOVEUP = -1.1f;
    private const float HORIZONTAL_TARGETROTATION = 90;
    private const float HORIZONTAL_TARGETPOSITION = 0.25f;

    private Vector3 targetPosition;
    private Vector3 targetRotation;
    private Vector3 originalPositionLocal;
    private Vector3 previousPosition;

    private bool usePosition = false;
    private bool useRotation = false;

    private Quaternion targetAngle;
    private float targetEulerX;
    private float targetEulerZ;

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

        switch (Orientation) {
            case ActorOrientation.OrientationEnum.Down:
            case ActorOrientation.OrientationEnum.Up:
                if (CurrentVelocity.x == 0)
                    targetEulerZ = 0;
                else if (CurrentVelocity.x > 0)
                    targetEulerZ = -HORIZONTAL_TARGETROTATION * 0.5f;
                else if (CurrentVelocity.x < 0)
                    targetEulerZ = HORIZONTAL_TARGETROTATION * 0.5f;

                if (CurrentVelocity.y == 0)
                    targetEulerX = 0;
                else if (CurrentVelocity.y > 0)
                    targetEulerX = VERTICAL_TARGETROTATION_MOVEUP;
                else if (CurrentVelocity.y < 0)
                    targetEulerX = VERTICAL_TARGETROTATION_MOVEDOWN;

                targetAngle = Quaternion.Euler(targetEulerX, 0, targetEulerZ);

                useRotation = true;
                usePosition = true;
                break;
            case ActorOrientation.OrientationEnum.Left:
            case ActorOrientation.OrientationEnum.Right:
                if (CurrentVelocity.x == 0)
                    targetEulerZ = 0;
                else if (CurrentVelocity.x > 0)
                    targetEulerZ = -HORIZONTAL_TARGETROTATION;
                else if (CurrentVelocity.x < 0)
                    targetEulerZ = HORIZONTAL_TARGETROTATION;

                targetAngle = Quaternion.Euler(targetEulerX, 0, targetEulerZ);

                useRotation = true;
                usePosition = true;
                break;
        }

        if (useRotation) {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle, Mathf.Clamp01(Time.deltaTime * RotateSpeed * Mathf.Max(0.01f, Mathf.Abs(CurrentVelocity.x))));
        }

        if (usePosition) {
            // the position will move in tandem with the rotation so as to stay in sync
            eulerZ = transform.eulerAngles.z;
            if (eulerZ > 180)
                eulerZ -= 360;

            targetPosition = new Vector3(0, HORIZONTAL_TARGETPOSITION, 0);
            targetPosition = transform.parent.position + Vector3.Slerp(originalPositionLocal, targetPosition, Mathf.Abs(eulerZ) / Mathf.Abs(HORIZONTAL_TARGETROTATION));
            transform.position += (targetPosition - transform.position) * Time.deltaTime * FollowSpeed;
        }
        else {
            // just follow the original local position
            transform.position += ((transform.parent.position + originalPositionLocal) - transform.position) * Time.deltaTime * FollowSpeed;
        }

        // save position for reset
        previousPosition = transform.position;
    }
}
