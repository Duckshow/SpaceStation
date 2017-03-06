using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanClick))]
public class CanMove : MonoBehaviour {
	[SerializeField] private Vector3 MoveOnClick;
	[SerializeField] private float Speed;
	private CanClick Clickable;
	private Vector3 localPosBeforeMove;
	private float timeAt

	void Awake() {
		Clickable = GetComponent<CanClick> ();
	}

	void OnEnable() {
		Clickable._OnLeftClickRelease += Move;
	}
	void OnDisable() {
		Clickable._OnLeftClickRelease -= Move;
	}

	private bool hasMoved = false;
	private bool pleaseMove = false;
	void Move(){
		hasMoved = !hasMoved;
		pleaseMove = true;
		localPosBeforeMove = transform.localPosition;
	}

	void LateUpdate(){
		if (pleaseMove) {
			transform.localPosition = Vector3.Lerp (localPosBeforeMove, localPosBeforeMove + MoveOnClick, );
		}
	}
}
