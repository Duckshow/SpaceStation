using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CanClick))] [RequireComponent(typeof(NodeObject))]
public class CanInspect : MonoBehaviour {

	private const byte OUTLINE_ALPHA_SELECTED = 128;

    public enum State { Default, PickedUp, Contained }
    [HideInInspector] public State CurrentState { get; private set; }

    public GUIManager.WindowType Window_Inspector;
    public GUIManager.WindowType Window_ObjectPlacer;
    public bool CanBePickedUp = false;
	public bool IsHidden { get; private set; }

    [Header("Info Window Settings (Leave null if unused)")]
	[SerializeField] private Image OnClickImage;

	public Sprite Selected_Sprite;
    public string Selected_Name;
    public string Selected_Desc;

    [HideInInspector] public bool PrevWasOutlined = false;
    [HideInInspector] public UVController MyUVC;
    [HideInInspector] public Transform PreviousParent;
    [HideInInspector] public Vector3 PreviousPosition; // local if previousparent != null;
    [HideInInspector] public int PreviousComponentSlotIndex;

	private bool mouseIsWithinRange = false;
	private CanClick Clickable;
    [HideInInspector] public NodeObject MyTileObject;

    public delegate void InspectEvent();
    public InspectEvent PostPickUp;
    public InspectEvent PostPutDown;
	public delegate void InspectEventTrueFalse(bool _b);
	public InspectEventTrueFalse OnHide;


	void Awake() {
        if (!hasSetup)
            Setup();
    }

    private bool hasSetup = false;
    public void Setup() {
        if (hasSetup)
            return;

        hasSetup = true;
        MyUVC = GetComponentInChildren<UVController>(); // TODO: this may have to support multiple meshes (because bottom+top and such)
		Clickable = GetComponent<CanClick> ();
        MyTileObject = GetComponent<NodeObject>();
    }

    void OnEnable() {
		Clickable._OnWithinRange += OnWithinRange;
    }
    void OnDisable() {
		Clickable._OnWithinRange -= OnWithinRange;
    }

    void Start() {
        MyUVC.ChangeColor(ColoringTool.COLOR_WHITE, _temporary: true);
    }

	void LateUpdate(){
		if (!mouseIsWithinRange && PrevWasOutlined) {
			PrevWasOutlined = false;
			ShowOutline (false);
		}

		mouseIsWithinRange = false;
	}

	public void OnWithinRange(){
		mouseIsWithinRange = true;

		if (!Clickable.Enabled)
			return;

		if (!PrevWasOutlined) {
			PrevWasOutlined = true;
			ShowOutline(true);
			return;
		}
	}

	public void PickUp(){
        MyTileObject.SetParent(null);
		Hide (true);
		GUIManager.Instance.OpenNewWindow(this, State.PickedUp, Window_ObjectPlacer);

        if(PostPickUp != null)
            PostPickUp();
    }
	public void PutDown(Node _node){
		GUIManager.Instance.CloseInfoWindow(this);

        MyTileObject.SetParent(null);
        MyTileObject.SetGridPosition(_node);
        //transform.eulerAngles = Tile.GetEulerFromOrientation(_orientation);
		Hide (false);

        if(PostPutDown != null)
            PostPutDown();
    }
	public void PutOffGrid(NodeObject _parent, Vector3 _localPos, bool _hide){
		GUIManager.Instance.CloseInfoWindow(this);

        MyTileObject.SetParent(_parent);
		transform.localPosition = _localPos;
        // TODO: might need a Sort() here, but if it's offgrid, as intended, then it shouldn't be needed, right?
        Hide(_hide);
	}

    public void ShowOutline(bool _true) {
        MyUVC.ChangeColor(_true ? ColoringTool.COLOR_GREY : ColoringTool.COLOR_WHITE, _temporary: true);
    }

    public void Hide(bool _b) {
		IsHidden = _b;

		if (!hasSetup)
            Setup();

        MyUVC.Hide(_b);

		if(OnHide != null)
			OnHide(_b);
    }
}
