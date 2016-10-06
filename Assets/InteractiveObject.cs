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
}
