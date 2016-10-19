using UnityEngine;
using System.Collections;

public class ActorOrientation : MonoBehaviour {

    public enum OrientationEnum { Down, Left, Up, Right }
    public OrientationEnum Orientation = OrientationEnum.Down;


    [SerializeField] private SpriteRenderer HairRenderer;
    [SerializeField] private SpriteRenderer HeadRenderer;
    [SerializeField] private SpriteRenderer EyeRenderer;
    [SerializeField] private SpriteRenderer BeardRenderer;

    private Actor actor;
    private TrailObject body;


    void Awake() {
        actor = GetComponent<Actor>();
        body = GetComponentInChildren<TrailObject>();
    }

    void Start() {
        SetOrientation(Orientation);
    }

	void Update () {
        if (Input.GetKeyUp(KeyCode.KeypadEnter)) {
            Orientation++;

            if (Orientation >= (OrientationEnum)4)
                Orientation = 0;

            SetOrientation(Orientation);
        }
	}

    public void SetOrientation(OrientationEnum _orientation) {
        Orientation = _orientation;
        body.Orientation = _orientation;

        HairRenderer.sprite = ActorAssets.Instance.HairStyles[actor.HairStyleIndex].GetOrientedAsset(_orientation);
        HeadRenderer.sprite = ActorAssets.Instance.Heads[actor.HeadIndex].GetOrientedAsset(_orientation);
        EyeRenderer.sprite = ActorAssets.Instance.Eyes[actor.EyeIndex].GetOrientedAsset(_orientation);
        BeardRenderer.sprite = ActorAssets.Instance.Beards[actor.BeardIndex].GetOrientedAsset(_orientation);
    }
}
