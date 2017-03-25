using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CanClick))] [RequireComponent(typeof(TileObject))]
public class CanInspect : MonoBehaviour {

	private const byte OUTLINE_ALPHA_SELECTED = 128;

    public enum State { Default, PickedUp, Contained }
    [HideInInspector] public State CurrentState { get; private set; }

    public GUIManager.WindowType Window_Inspector;
    public GUIManager.WindowType Window_ObjectPlacer;
    public bool CanBePickedUp = false;

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
    [HideInInspector] public TileObject MyTileObject;


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
        MyTileObject = GetComponent<TileObject>();
    }

    void OnEnable() {
		Clickable._OnWithinRange += OnWithinRange;
    }
    void OnDisable() {
		Clickable._OnWithinRange -= OnWithinRange;
    }

    void Start() {
        //Renderer.material.SetColor("_OutlineColor", new Color32(0, 0, 0, 0));
        MyUVC.ChangeColor(Color.white);
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
        Debug.Log(name + " is at " + transform.position);
        MyTileObject.SetParent(null);
		Hide (true);
		GUIManager.Instance.OpenNewWindow(this, State.PickedUp, Window_ObjectPlacer);
	}
	public void PutDown(Tile _tile/*, Tile.TileOrientation _orientation*/){
		GUIManager.Instance.CloseInfoWindow(this);

        MyTileObject.SetParent(null);
        MyTileObject.SetGridPosition(_tile);
        //transform.eulerAngles = Tile.GetEulerFromOrientation(_orientation);
		Hide (false);
	}
	public void PutOffGrid(TileObject _parent, Vector3 _localPos, bool _hide){
        GUIManager.Instance.CloseInfoWindow(this);

        MyTileObject.SetParent(_parent);
		transform.localPosition = _localPos;
        // might need a Sort() here, but if it's offgrid, as intended, then it shouldn't be needed, right?
        Hide(_hide);
	}

    public void ShowOutline(bool _true) {
        byte alpha = _true ? OUTLINE_ALPHA_SELECTED : (byte)0;
        MyUVC.ChangeColor(_true ? Color.grey : Color.white);
        //Renderer.material.SetColor("_OutlineColor", new Color32(0, 0, 0, alpha));
    }

    public void Hide(bool _b) {
        if (!hasSetup)
            Setup();

        MyUVC.Hide(_b); // TODO: this will have to support multiple because top+bottom and such

		if(CanBePickedUp)
			Clickable.Enabled = !_b;
        if (_b)
            MyTileObject.DeActivate();
        else
            MyTileObject.Activate();
    }
}
