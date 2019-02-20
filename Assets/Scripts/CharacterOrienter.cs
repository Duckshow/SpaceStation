using UnityEngine;
public class CharacterOrienter : MonoBehaviour {

    public enum OrientationEnum { Down, Left, Up, Right }
    public OrientationEnum Orientation = OrientationEnum.Down;

    [SerializeField] private SpriteRenderer HairRenderer;
    [SerializeField] private SpriteRenderer HeadRenderer;
    [SerializeField] private SpriteRenderer EyeRenderer;
    [SerializeField] private SpriteRenderer BeardRenderer;
    [SerializeField] private SpriteRenderer BodyRenderer;

    private Character character;
    private TrailObject body;


    void Awake() {
        character = GetComponent<Character>();
        body = GetComponentInChildren<TrailObject>();
    }

    void Start() {
        SetOrientation(Orientation);
    }

    private const float TIME_BETWEEN_POS_UPDATES = 0.1f;
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

            newSortOrder = UVController.GetSortOrderFromGridY(GameGrid.GetInstance().GetNodeFromWorldPos(transform.position).GridPos.y);
            BodyRenderer.sortingOrder = newSortOrder + 2;
            HeadRenderer.sortingOrder = newSortOrder + 3;
            BeardRenderer.sortingOrder = newSortOrder + 4;
            EyeRenderer.sortingOrder = newSortOrder + 5;
            HairRenderer.sortingOrder = newSortOrder + 6;
        }
    }

	public void SetOrientation(float _angle360) {
		if (_angle360 > 315 || _angle360 < 45){
			SetOrientation(CharacterOrienter.OrientationEnum.Up);
		}
		else if (_angle360 > 45 && _angle360 < 135){
			SetOrientation(CharacterOrienter.OrientationEnum.Right);
		}
		else if (_angle360 > 135 && _angle360 < 225){
			SetOrientation(CharacterOrienter.OrientationEnum.Down);
		}
		else if (_angle360 > 225 && _angle360 < 315){
			SetOrientation(CharacterOrienter.OrientationEnum.Left);
		}
    }

    public void SetOrientation(OrientationEnum _orientation) {
        Orientation = _orientation;
        body.Orientation = _orientation;

        HairRenderer.sprite = CachedAssets.Instance.HairStyles[0].GetOrientedAsset(_orientation);
        HeadRenderer.sprite = CachedAssets.Instance.Heads[0].GetOrientedAsset(_orientation);
        EyeRenderer.sprite = CachedAssets.Instance.Eyes[0].GetOrientedAsset(_orientation);
        BeardRenderer.sprite = CachedAssets.Instance.Beards[0].GetOrientedAsset(_orientation);
    }

    public void ForceLieDown(bool _b) {
        body.ForceTargetRotation = _b;
    }
}
