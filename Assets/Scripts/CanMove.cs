using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CanMove : MonoBehaviour {

	[SerializeField] private Toggle ConnectedToggle;
	[SerializeField] private Transform Transform;
	[SerializeField] private float MoveY;
	[SerializeField] private float Speed;
	private float returnToY;
	private float yAtStartMove;
	private float timeAtMoveStart;
	private Vector3 offset;

	//private CanClick Clickable;


	void Awake() {
		// Clickable = GetComponent<CanClick> ();
		returnToY = Transform.localPosition.y;
		MoveY += returnToY;
	}

	void OnEnable() {
		ConnectedToggle.onValueChanged.AddListener (OnToggleValueChanged);
		// Clickable._OnLeftClickRelease += Move;
	}
	void OnDisable() {
		ConnectedToggle.onValueChanged.RemoveListener (OnToggleValueChanged);
		// Clickable._OnLeftClickRelease -= Move;
	}

    bool oldValue = false;
	void OnToggleValueChanged(bool _b){
        if (_b == oldValue)
            return;
        oldValue = _b;

        Move (_b);
	}

	private bool moveForward = false;
	private bool pleaseMove = false;
	void Move(bool _b){
		//Clickable.Enabled = false;
		pleaseMove = true;
		moveForward = !moveForward;
		yAtStartMove = Transform.localPosition.y;
		timeAtMoveStart = Time.time;
	}

	float _t;
	void LateUpdate(){
		if (pleaseMove) {
			_t = (Time.time - timeAtMoveStart) * Speed;

			if (moveForward)
				Transform.localPosition = new Vector3 (Transform.localPosition.x, Mathf.Lerp (yAtStartMove, MoveY, _t), Transform.localPosition.z);
			else
				Transform.localPosition = new Vector3(Transform.localPosition.x, Mathf.Lerp (yAtStartMove, returnToY, _t), Transform.localPosition.z);

			if (_t >= 1) {
				//Clickable.Enabled = true;
				pleaseMove = false;
			}
		}
	}
}
