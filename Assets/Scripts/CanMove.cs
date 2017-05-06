using UnityEngine;
using UnityEngine.UI;

public class CanMove : MonoBehaviour {

	[SerializeField] private Toggle ConnectedToggle;
	[SerializeField] private Transform Transform;
   	[SerializeField] private GameObject ObjectToDisableWhenMovedBack;
    [SerializeField] private Vector2 MoveDelta;
	[SerializeField] private float Speed;
	private Vector2 returnToPos;
	private Vector2 posAtStartMove;
	private float timeAtMoveStart;
	private Vector3 offset;
    private enum ProgressEnum { Default, OnTheWay, Target }
    private ProgressEnum progress = ProgressEnum.Default;

	//private CanClick Clickable;


	void Awake() {
		// Clickable = GetComponent<CanClick> ();
		returnToPos = Transform.localPosition;
		MoveDelta += returnToPos;
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
		moveForward = _b;
		posAtStartMove = Transform.localPosition;
		timeAtMoveStart = Time.time;
	}

	float _t;
	void LateUpdate(){
		if (pleaseMove) {
			_t = (Time.time - timeAtMoveStart) * Speed;

			if (moveForward)
				Transform.localPosition = new Vector3 (Mathf.Lerp(posAtStartMove.x, MoveDelta.x, _t), Mathf.Lerp (posAtStartMove.y, MoveDelta.y, _t), Transform.localPosition.z);
			else
				Transform.localPosition = new Vector3(Mathf.Lerp(posAtStartMove.x, returnToPos.x, _t), Mathf.Lerp (posAtStartMove.y, returnToPos.y, _t), Transform.localPosition.z);

            progress = ProgressEnum.OnTheWay;
            if (ObjectToDisableWhenMovedBack != null && !ObjectToDisableWhenMovedBack.activeSelf)
                ObjectToDisableWhenMovedBack.SetActive(true);

            if (_t >= 1) {
				//Clickable.Enabled = true;
				pleaseMove = false;
                progress = moveForward ? ProgressEnum.Target : ProgressEnum.Default;
            }
		}
        else if (progress == ProgressEnum.Default) {
            if (ObjectToDisableWhenMovedBack != null && ObjectToDisableWhenMovedBack.activeSelf)
                ObjectToDisableWhenMovedBack.SetActive(false);
        }
	}
}
