using UnityEngine;

public class UITrackObject : MonoBehaviour {

    public Transform trackTransform;
    [SerializeField] private Vector2 offset;

    private Canvas myCanvas;
    private Vector2 pos;
    private Vector2 offsetModified;

    void Awake() {
        myCanvas = FindObjectOfType<Canvas>();
    }

    void OnGUI() {
        if (trackTransform == null)
            return;

        offsetModified = offset * Camera.main.orthographicSize / 10;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(myCanvas.transform as RectTransform, Camera.main.WorldToScreenPoint(trackTransform.position + new Vector3(offsetModified.x, 0, offsetModified.y)), myCanvas.worldCamera, out pos);
        //transform.position = myCanvas.transform.TransformPoint(pos);
        transform.localPosition = pos;
    }
}
