using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class CanClick : MonoBehaviour {

	public static List<CanClick> AllClickables = new List<CanClick>();

	public float ClickableRange = 1;
    public bool UseTileAsRange = true;
	public bool IsOnGUI = false;
	[HideInInspector] public bool Enabled = true;

	public delegate void DefaultEvent();
	public DefaultEvent _OnWithinRange;
	public DefaultEvent _OnLeftClickRelease;
	public DefaultEvent _OnRightClickRelease;

    private Renderer myRenderer;


    void Awake() {
        myRenderer = GetComponentInChildren<Renderer>();
    }
	void OnEnable() {
		AllClickables.Add(this);
	}
	void OnDisable() {
		AllClickables.Remove(this);        
	}

    public bool IsVisible() {
        return myRenderer.isVisible;
    }
    private Vector2 bottomLeft = new Vector3(-0.5f, -0.5f);
    private Vector2 topRight = new Vector3(0.5f, 0.5f);
    private Vector2 myBottomLeft;
    private Vector2 myTopRight;
    Vector2 myPos = new Vector2();
    Vector2 mousePos;
    float distance;
    public bool IsMouseWithinRange() {
        myPos.x = transform.position.x;
        myPos.y = transform.position.y;
        mousePos = IsOnGUI ? Input.mousePosition : Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (UseTileAsRange) {
            myBottomLeft = myPos + bottomLeft;
            myTopRight = myPos + topRight;
            return mousePos.x > myBottomLeft.x && mousePos.y > myBottomLeft.y && mousePos.x < myTopRight.x && mousePos.y < myTopRight.y;
        }
        else {
            distance = (myPos - mousePos).magnitude/* * (Camera.main.orthographicSize / 10)*/;
            return distance < ClickableRange;
        }
    }

	public void OnLeftClickRelease(){
		if (!Enabled)
			return;

		if (_OnLeftClickRelease != null)
			_OnLeftClickRelease ();
	}

	public void OnRightClickRelease() {
		if (!Enabled)
			return;
		
		if (_OnRightClickRelease != null)
			_OnRightClickRelease ();
	}
}
