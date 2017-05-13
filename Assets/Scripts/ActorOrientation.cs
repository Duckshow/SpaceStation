using UnityEngine;
public class ActorOrientation : MonoBehaviour {

    public enum OrientationEnum { Down, Left, Up, Right }
    public OrientationEnum Orientation = OrientationEnum.Down;

    [SerializeField] private SpriteRenderer HairRenderer;
    [SerializeField] private SpriteRenderer HeadRenderer;
    [SerializeField] private SpriteRenderer EyeRenderer;
    [SerializeField] private SpriteRenderer BeardRenderer;
    [SerializeField] private SpriteRenderer BodyRenderer;

    private Actor actor;
    private TrailObject body;


    void Awake() {
        actor = GetComponent<Actor>();
        body = GetComponentInChildren<TrailObject>();
    }

    void Start() {
        SetOrientation(Orientation);
    }

    private const float TIME_BETWEEN_POS_UPDATES = 0.1f;
    private float timeAtLastPosUpdate = 0;
    private int newSortOrder = 0;
    private int prevY = -1;
    private int currentY = 0;
	void Update () {
        if (Input.GetKeyUp(KeyCode.KeypadEnter)) {
            Orientation++;

            if (Orientation >= (OrientationEnum)4)
                Orientation = 0;

            SetOrientation(Orientation);
        }

        // TODO: this should be a part of the sorting system proper!
        currentY = Mathf.RoundToInt(transform.position.y - 0.5f);
        if (currentY != prevY) {
            prevY = currentY;

            newSortOrder = UVController.GetSortOrderFromGridY(Grid.Instance.GetTileFromWorldPoint(transform.position).GridY);
            BodyRenderer.sortingOrder = newSortOrder + 2;
            HeadRenderer.sortingOrder = newSortOrder + 3;
            BeardRenderer.sortingOrder = newSortOrder + 4;
            EyeRenderer.sortingOrder = newSortOrder + 5;
            HairRenderer.sortingOrder = newSortOrder + 6;
        }
    }

    public void SetOrientation(OrientationEnum _orientation) {
        Orientation = _orientation;
        body.Orientation = _orientation;

        HairRenderer.sprite = CachedAssets.Instance.HairStyles[actor.HairStyleIndex].GetOrientedAsset(_orientation);
        HeadRenderer.sprite = CachedAssets.Instance.Heads[actor.HeadIndex].GetOrientedAsset(_orientation);
        EyeRenderer.sprite = CachedAssets.Instance.Eyes[actor.EyeIndex].GetOrientedAsset(_orientation);
        BeardRenderer.sprite = CachedAssets.Instance.Beards[actor.BeardIndex].GetOrientedAsset(_orientation);
    }

    public void ForceLieDown(bool _b) {
        body.ForceTargetRotation = _b;
    }
}
