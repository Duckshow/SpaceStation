using UnityEngine;

public class CameraController : MonoBehaviour {

    [SerializeField]
    private int Boundary; // distance from edge scrolling starts
    [SerializeField]
    private int SpeedMax;
    [SerializeField]
    private int Speed;
    [SerializeField]
    private int ZoomMin;
    [SerializeField]
    private int ZoomMax;

    private Camera myCamera;
    private int screenWidth;
    private int screenHeight;
    private Vector2 mousePos;
    private Vector2 move;
    private float adjustedSpeed;
    private float zoomOffsetMultiplier;
    private float zoomDelta;


    void Awake() {
        myCamera = GetComponent<Camera>();
    }
    void Start() {
        screenWidth = Screen.width;
        screenHeight = Screen.height;
    }
    void Update() {
        Move();
        Scroll();
    }

    void Move() {
        mousePos = Input.mousePosition;
        move = new Vector2();
        adjustedSpeed = Mathf.Min(Speed * (myCamera.orthographicSize * 0.1f), SpeedMax);

        if (mousePos.x > screenWidth - Boundary)
            move.x = (((mousePos.x - (screenWidth - Boundary)) / Boundary) * adjustedSpeed) * Time.deltaTime; // move on +X axis

        if (mousePos.x < 0 + Boundary)
            move.x = ((Boundary - mousePos.x) / Boundary) * -adjustedSpeed * Time.deltaTime; // move on -X axis

        if (mousePos.y > screenHeight - Boundary)
            move.y = (((mousePos.y - (screenHeight - Boundary)) / Boundary) * adjustedSpeed) * Time.deltaTime; // move on +Z axis

        if (mousePos.y < 0 + Boundary)
            move.y = ((Boundary - mousePos.y) / Boundary) * -adjustedSpeed * Time.deltaTime; // move on -Z axis

        if (Mathf.Abs(transform.position.x + move.x) > Grid.GridSize.x * 0.5f)
            return;
        if (Mathf.Abs(transform.position.y + move.y) > Grid.GridSize.y * 0.5f)
            return;

        transform.position = new Vector3(transform.position.x + move.x, transform.position.y + move.y, transform.position.z);
    }
    void Scroll() {
        zoomDelta = Input.mouseScrollDelta.y;
        if (zoomDelta == 0 || zoomDelta < 0 && myCamera.orthographicSize == ZoomMax || zoomDelta > 0 && myCamera.orthographicSize == ZoomMin)
            return;

        zoomOffsetMultiplier = 1.0f / (myCamera.orthographicSize * zoomDelta);

        // move
        transform.position += (myCamera.ScreenToWorldPoint(mousePos) - transform.position) * zoomOffsetMultiplier;
        // zoom
        myCamera.orthographicSize = Mathf.Clamp(myCamera.orthographicSize - zoomDelta, ZoomMin, ZoomMax);
    }

    //void OnGUI() {
    //    GUI.Box(new Rect((Screen.width / 2) - 140, 5, 280, 25), "Mouse Position = " + Input.mousePosition);
    //    GUI.Box(new Rect((Screen.width / 2) - 70, Screen.height - 30, 140, 25), "Mouse X = " + Input.mousePosition.x);
    //    GUI.Box(new Rect(5, (Screen.height / 2) - 12, 140, 25), "Mouse Y = " + Input.mousePosition.y);
    //}
}