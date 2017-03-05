using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CanClick))]
public class CanInspect : MonoBehaviour {

	private const byte OUTLINE_ALPHA_SELECTED = 128;

    public enum State { Default, PickedUp, Contained }
    [HideInInspector] public State CurrentState { get; private set; }

    public GUIManager.WindowType Type;
    public bool CanBePickedUp = false;

    [Header("Info Window Settings (Leave null if unused)")]
	[SerializeField] private Image OnClickImage;

	public Sprite Selected_Sprite;
    public string Selected_Name;
    public string Selected_Desc;

    [HideInInspector] public bool PrevWasOutlined = false;
    [HideInInspector] public MeshRenderer Renderer;
    [HideInInspector] public Transform PreviousParent;
    [HideInInspector] public Vector3 PreviousPosition; // local if previousparent != null;
    [HideInInspector] public int PreviousComponentSlotIndex;

	private bool mouseIsWithinRange = false;
	private CanClick Clickable;


    void Awake() {
        Renderer = GetComponent<MeshRenderer>();
		Clickable = GetComponent<CanClick> ();
    }

    void OnEnable() {
		Clickable.OnWithinRange += OnWithinRange;
    }
    void OnDisable() {
		Clickable.OnWithinRange -= OnWithinRange;
    }

    void Start() {
        Renderer.material.SetColor("_OutlineColor", new Color32(0, 0, 0, 0));
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
		transform.parent = null;
		Hide (true);
		GUIManager.Instance.OpenNewWindow(this, CanInspect.State.PickedUp, Type);
	}
	public void PutDown(Tile _tile){
		GUIManager.Instance.CloseInfoWindow(this);

		transform.parent = null;
		transform.position = _tile.DefaultPositionWorld;
		Hide (false);
	}
	public void PutSomewhereElse(Transform _transform, Vector3 _localPos, bool _hide){
		GUIManager.Instance.CloseInfoWindow(this);

		transform.parent = _transform;
		transform.localPosition = _localPos;
		Debug.Log ("Hide: " + _hide);
		Hide(_hide);
	}

    public void ShowOutline(bool _true) {
        byte alpha = _true ? OUTLINE_ALPHA_SELECTED : (byte)0;
        Renderer.material.SetColor("_OutlineColor", new Color32(0, 0, 0, alpha));
    }

	public void Hide(bool _b) {
        GetComponent<MeshRenderer>().enabled = !_b;

		if(CanBePickedUp)
			Clickable.Enabled = !_b;

		// TODO: disable tileobject functionality when hidden
    }
}
