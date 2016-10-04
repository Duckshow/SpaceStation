using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class InteractiveObject : MonoBehaviour {

    public static List<InteractiveObject> AllInteractiveObjects = new List<InteractiveObject>();

    public enum State { Default, PickedUp, Contained }
    [HideInInspector]
    public State CurrentState { get; private set; }

    private const byte OUTLINE_ALPHA_SELECTED = 128;
    private static bool trackingStarted = false;
    private static Vector2 mousePos;
    private static IEnumerator _TrackInput() {
        while (true) {
            mousePos = Input.mousePosition;
            yield return null;
        }
    }

    public GUIManager.WindowType Type;
    public float ClickableRange = 10;
    public bool CanBePickedUp = false;
    [HideInInspector]
    public bool CanBePickedUpCurrently = false;

    [Header("Info Window Settings (Leave null if unused)")]
	[SerializeField] 
	private Image OnClickImage;

	public Sprite Selected_Sprite;
    public string Selected_Name;
    public string Selected_Desc;

	//private Canvas defaultCanvas;
 //   private Transform[] onClickImageHierarchy;

    [Header("")]
    public UnityEvent OnLeftClickedExt;
    public UnityEvent OnRightClickedExt;

    [HideInInspector]
    public bool PrevWasOutlined = false;
    [HideInInspector]
    public MeshRenderer Renderer;
    [HideInInspector]
    public Transform PreviousParent;
    [HideInInspector]
    public Vector3 PreviousPosition; // local if previousparent != null;
    [HideInInspector]
    public int PreviousComponentSlotIndex;


    void Awake() {
        Renderer = GetComponent<MeshRenderer>();
        CanBePickedUpCurrently = CanBePickedUp;
  //      onClickImageHierarchy = OnClickImage.GetComponentsInChildren<Transform>();
		//defaultCanvas = FindObjectOfType<Canvas>();
    }

    void OnEnable() {
        AllInteractiveObjects.Add(this);
    }
    void OnDisable() {
        AllInteractiveObjects.Remove(this);        
    }

    void Start() {
        if (!trackingStarted) {
            trackingStarted = true;
            StartCoroutine(_TrackInput());
        }

        Renderer.material.SetColor("_OutlineColor", new Color32(0, 0, 0, 0));
    }

    //void Update() {
    //    Vector2 _screenPos = Camera.main.WorldToScreenPoint(transform.position); // optimization: can I track the mouse's worldpos rather than each object's screenpos?
    //    float magnitude = (mousePos - _screenPos).magnitude;
    //    bool withinRange = magnitude < ClickableRange;

    //    if (!PrevWasSelected && withinRange) {
    //        PrevWasSelected = true;
    //        renderer.material.SetColor("_OutlineColor", new Color32(0, 0, 0, OUTLINE_ALPHA_SELECTED));
    //    }
    //    else if (PrevWasSelected && withinRange) {
    //        if (Input.GetMouseButtonUp(0))
    //            OnLeftClickedExt.Invoke();
    //        else if (Input.GetMouseButtonUp(1))
    //            OnRightClickedExt.Invoke();
    //    }
    //    else if (PrevWasSelected && !withinRange) {
    //        PrevWasSelected = false;
    //        renderer.material.SetColor("_OutlineColor", new Color32(0, 0, 0, 0));
    //    }
    //}

    public void OnLeftClicked() {
        OnLeftClickedExt.Invoke();
    }

    public void OnRightClicked() {
        OnRightClickedExt.Invoke();
    }

    public void ShowWithOutline(bool _true) {
        byte alpha = _true ? OUTLINE_ALPHA_SELECTED : (byte)0;
        Renderer.material.SetColor("_OutlineColor", new Color32(0, 0, 0, alpha));
    }

    public void Hide(bool _b) {
        GetComponent<MeshRenderer>().enabled = !_b;

        if(CanBePickedUp)
            CanBePickedUpCurrently = !_b;
    }
    //public void ChangeStateAndVisuals(State _state) {
    //    CurrentState = _state;

    //    if (OnClickImage != null && OnClickImage.GetComponent<UIInfoWindow>())
    //        OnClickImage.GetComponent<UIInfoWindow>().ChangeToComponentState(_state);

    //    switch (_state) {
    //        case State.Default:
    //            Renderer.enabled = true;
    //            GetComponent<InteractiveObject>().TurnOffInfoWindow();
    //            break;
    //        case State.PickedUp:
    //            Renderer.enabled = false;
    //            GetComponent<InteractiveObject>().TurnOnInfoWindow();
    //            break;
    //        case State.Contained:
    //            Renderer.enabled = false;
    //            GetComponent<InteractiveObject>().TurnOffInfoWindow();
    //            break;
    //    }
    //}

    //public void ToggleInfoWindow() { // should only happen on left-click
    //    if (OnClickImage != null) {
    //        if (OnClickImage.transform.parent == transform)
    //            TurnOnInfoWindow();
    //        else
    //            TurnOffInfoWindow();
    //    }
    //}

    //public void TurnOnInfoWindow() {
    //    RecursiveToggleUIComponents(true);
    //    OnClickImage.transform.SetParent(defaultCanvas.transform);
    //}
    //public void TurnOffInfoWindow() {
    //    RecursiveToggleUIComponents(false);
    //    OnClickImage.transform.SetParent(transform);
    //}
    //void RecursiveToggleUIComponents(bool enable) {
    //    Image[] images;
    //    Button[] buttons;
    //    Text[] texts;
    //    for (int i = 0; i < onClickImageHierarchy.Length; i++) {
    //        images = onClickImageHierarchy[i].GetComponents<Image>();
    //        for (int j = 0; j < images.Length; j++)
    //            images[j].enabled = enable;

    //        buttons = onClickImageHierarchy[i].GetComponents<Button>();
    //        for (int j = 0; j < buttons.Length; j++)
    //            buttons[j].enabled = enable;

    //        texts = onClickImageHierarchy[i].GetComponents<Text>();
    //        for (int j = 0; j < texts.Length; j++)
    //            texts[j].enabled = enable;
    //    }
    //}
}
