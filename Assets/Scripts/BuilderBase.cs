using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BuilderBase {


	protected enum ModeEnum { None, Room, Wall, ObjectPlacing }
	protected ModeEnum Mode = ModeEnum.None;

	[SerializeField] protected byte ColorIndex_New = ColoringTool.COLOR_WHITE;
	[SerializeField] protected byte ColorIndex_AlreadyExisting = ColoringTool.COLOR_GREY;
	[SerializeField] protected byte ColorIndex_Remove = ColoringTool.COLOR_RED;
	[SerializeField] protected byte ColorIndex_Blocked = ColoringTool.COLOR_ORANGE;

	[System.NonSerialized] public bool IsActive = false;

	private IEnumerator ghostRoutine;

	private Vector2 oldMouseGridPos;

	private Node startTile;
	protected Node mouseTile;

	protected bool isDeleting = false;
	private bool modeWasChanged = false;
	private bool mouseGhostHasNewTile = false;
	protected bool mouseGhostIsDirty = true;

	private int distX;
	private int distY;
	private int distXAbs;
	private int distYAbs;
	private int ghostTile_GridX;
	private int ghostTile_GridY;
	private int highestAxisValue;
	private const int MAX_TILES_AXIS = 13;

	protected List<Node> highlightedTiles = new List<Node>();
	protected List<Node> tilesToModify = new List<Node>();
	//protected List<Tile.Type> selectedTilesNewType = new List<Tile.Type>();
	//protected List<Tile.TileOrientation> selectedTilesNewOrientation = new List<Tile.TileOrientation>();


	public virtual void Setup(Transform _transform) {
		//UVController[] _allQuads = _transform.GetComponentsInChildren<UVController>(true);
		//ALL_GHOSTS = new GhostInfo[(int)(_allQuads.Length * 0.5f)];
		//for (int quadIteration = 0, ghostIteration = 0; quadIteration < _allQuads.Length; quadIteration += 2, ghostIteration++) {
		//	_allQuads[quadIteration].Setup();
		//	_allQuads[quadIteration + 1].Setup();
		//	ALL_GHOSTS[ghostIteration] = new GhostInfo(_allQuads[quadIteration], _allQuads[quadIteration + 1]);
		//}
	}

	private float timeActivated;
	public void Activate() {
		if (IsActive)
			return;
		IsActive = true;
		timeActivated = Time.time;

		modeWasChanged = true; // enables mouseghost to "hotload" when tabbing between builders
        OnNewRound();
    }
	public virtual void DeActivate() {
		if (!IsActive)
			return;
		IsActive = false;
        ResetModifiedTiles(_includingMouse: true);
        ResetSelectedTiles();

		//for (int i = 0; i < ALL_GHOSTS.Length; i++)
		//	ALL_GHOSTS[i].SetActive(false);
	}

    protected virtual void OnNewRound() {

    }

    float timeLastGhostUpdate = -1;
	public void Update() {
		// don't want this running the first frame (causes at least one bug where a pickedup object is immediately put down)
		if (Time.time == timeActivated)
			return;

        isDeleting = Mouse.StateRight != Mouse.MouseStateEnum.None;
		InheritedUpdate ();

        if ((Mouse.StateLeft == Mouse.MouseStateEnum.None || Mouse.StateLeft == Mouse.MouseStateEnum.Click) && (Mouse.StateRight == Mouse.MouseStateEnum.None || Mouse.StateRight == Mouse.MouseStateEnum.Click)) {
			// determine Mode
			ModeEnum _oldMode = Mode;
			Mode = ModeEnum.Wall;
			TryChangeMode ();
			if (Mode != _oldMode) {
				modeWasChanged = true;
				mouseGhostIsDirty = true;
                OnNewRound();
			}
			
			// no click
			if ((Mouse.StateLeft == Mouse.MouseStateEnum.None && Mouse.StateRight == Mouse.MouseStateEnum.None) || mouseGhostIsDirty)
				ControlMouseGhost();
		}

        if (Mouse.IsOverGUI)
            return;

        bool skip = (!isDeleting && Mouse.StateLeft == Mouse.MouseStateEnum.Release) || (isDeleting && Mouse.StateRight == Mouse.MouseStateEnum.Release);
        if (!skip && Time.time - timeLastGhostUpdate < 0.01f)
            return;
        if (CanDrag() && (Mouse.StateLeft == Mouse.MouseStateEnum.Hold || Mouse.StateRight == Mouse.MouseStateEnum.Hold)) {
			DetermineGhostPositions(_hasClicked: true, _snapToNeighbours: false);
			timeLastGhostUpdate = Time.time;
		}

		// click released
		if(ShouldFinish())
            FinishRound();

		//yield return null;
		modeWasChanged = false;
	}
	protected virtual void InheritedUpdate(){
	}
	protected virtual void TryChangeMode(){
	}
    private bool CanDrag() {
        switch (Mode) {
            case ModeEnum.Room:
			case ModeEnum.Wall:
                return true;
            case ModeEnum.ObjectPlacing:
			case ModeEnum.None:
                return false;
            default:
                throw new System.NotImplementedException(Mode.ToString() + " hasn't been properly implemented yet!");
        }
    }
    protected virtual bool ShouldFinish() {
        return (Mouse.StateLeft == Mouse.MouseStateEnum.Release && !isDeleting) || (Mouse.StateRight == Mouse.MouseStateEnum.Release && isDeleting);
    }
    protected virtual void FinishRound() {
        ApplyCurrentTool();
        mouseGhostIsDirty = true;
        OnNewRound();
    }

	protected virtual void ResetModifiedTiles(bool _includingMouse = false) {
        //for (int i = 0; i < modifiedTiles.Count; i++)
        //    modifiedTiles[i].HasBeenEvaluated = false;
        highlightedTiles.Clear();
    }
    protected virtual void ResetSelectedTiles() {
        //for (int i = 0; i < selectedTiles.Count; i++)
        //    selectedTiles[i].HasBeenEvaluated = false;
        tilesToModify.Clear();
    }


	private void ControlMouseGhost() {
		// find current tile
		oldMouseGridPos = mouseTile == null ? Vector2.zero : new Vector2(mouseTile.GridPos.x, mouseTile.GridPos.y);
		mouseTile = GameGrid.Instance.GetNodeFromWorldPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));

		mouseGhostHasNewTile = oldMouseGridPos.x != mouseTile.GridPos.x || oldMouseGridPos.y != mouseTile.GridPos.y;
		if (modeWasChanged)
			mouseGhostHasNewTile = true; // have to force my way into the sprite-update stuff below
		if (mouseGhostHasNewTile){
			mouseGhostIsDirty = true;
			ResetModifiedTiles ();
        }

		if (mouseGhostIsDirty) {
			mouseGhostIsDirty = false;
			DetermineGhostPositions(_hasClicked: false, _snapToNeighbours: mouseGhostHasNewTile);
		}
	}

	private bool hasMoved;
	private int oldDistX;
	private int oldDistY;
	protected void DetermineGhostPositions(bool _hasClicked, bool _snapToNeighbours) {

		// find current tile
		if(!_hasClicked || startTile == null)
			startTile = GameGrid.Instance.GetNodeFromWorldPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
	    if(_hasClicked || startTile == null)
			mouseTile = GameGrid.Instance.GetNodeFromWorldPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));

		if (Mode == ModeEnum.Wall || Mode == ModeEnum.Room) {
			// get tile distance
			oldDistX = distX;
			oldDistY = distY;
			distX = mouseTile.GridPos.x - startTile.GridPos.x;
			distY = mouseTile.GridPos.y - startTile.GridPos.y;
			hasMoved = !(oldDistX == distX && oldDistY == distY);

			// if hasn't moved, early-out
			if (!hasMoved && mouseTile != startTile)
				return;

			distXAbs = Mathf.Min(Mathf.Abs(distX), MAX_TILES_AXIS);
			distYAbs = Mathf.Min(Mathf.Abs(distY), MAX_TILES_AXIS);

			ghostTile_GridX = startTile.GridPos.x;
			ghostTile_GridY = startTile.GridPos.y;
		}

        ResetModifiedTiles();
        ResetSelectedTiles();

		switch (Mode) {
			// click-Modes
			case ModeEnum.ObjectPlacing:
				ghostTile_GridX = mouseTile.GridPos.x;
				ghostTile_GridY = mouseTile.GridPos.y;

				AddNextGhost (ghostTile_GridX, ghostTile_GridY);
				break;

				// drag-Modes
			case ModeEnum.Wall:
				if (!_hasClicked) {
					ghostTile_GridX = mouseTile.GridPos.x;
					ghostTile_GridY = mouseTile.GridPos.y;

					AddNextGhost (ghostTile_GridX, ghostTile_GridY);
				}
				else {
					#region Default Held
					// determine if we're going to force diagonal ghosting
					highestAxisValue = Mathf.Max(distXAbs, distYAbs);

					// first pass
					for (int i = 0; i <= highestAxisValue; i++) {
						// determine the offset from the _startTile
						if (distXAbs >= distYAbs) { 
							ghostTile_GridX = startTile.GridPos.x + (distX < 0 ? -i : i);
						}
						if (distYAbs >= distXAbs) { 
							ghostTile_GridY = startTile.GridPos.y + (distY < 0 ? -i : i);
						}

						if (!GameGrid.Instance.IsInsideGrid(ghostTile_GridX, ghostTile_GridY)){
							break;
						}

						AddNextGhost (ghostTile_GridX, ghostTile_GridY);
					}
					#endregion
				}
				break;
			case ModeEnum.Room:
				if (!_hasClicked) {
					ghostTile_GridX = mouseTile.GridPos.x;
					ghostTile_GridY = mouseTile.GridPos.y;

                    AddNextGhost(ghostTile_GridX, ghostTile_GridY);
                }
				else {
					#region Room Held
					bool _isOnEdgeX = true;
					bool _isOnEdgeY = true;

					// first pass
					for (int y = 0; y <= distYAbs; y++) {
						_isOnEdgeY = (y == 0 || y == distYAbs);

						for (int x = 0; x <= distXAbs; x++) {
							_isOnEdgeX = (x == 0 || x == distXAbs);

							if (!_isOnEdgeX && !_isOnEdgeY) { 
								continue;
							}

							ghostTile_GridX = startTile.GridPos.x + (distX < 0 ? -x : x);
							ghostTile_GridY = startTile.GridPos.y + (distY < 0 ? -y : y);

							if (!GameGrid.Instance.IsInsideGrid(ghostTile_GridX, ghostTile_GridY)){
								continue;
							}

                            AddNextGhost(ghostTile_GridX, ghostTile_GridY);
                        }
                    }
                    #endregion
                }

				break;
			default:
				throw new System.NotImplementedException(Mode.ToString() + " hasn't been implemented!");
		}

		EvaluateUsedGhostConditions();
	}

	protected void AddNextGhost(int _gridX, int _gridY) {
		Node _node = GameGrid.Instance.TryGetNode(_gridX, _gridY);
		highlightedTiles.Add(_node);
		if (this is ColoringTool)
			return;

		_node.SetIsWall(true, _temporarily: true);
	}

	protected void EvaluateUsedGhostConditions() {
		for (int i = 0; i < highlightedTiles.Count; i++) { 
			Evaluate (highlightedTiles[i]);
		}
	}
	protected virtual bool Evaluate(Node _node){
        return true;
    }

	protected virtual void ApplySettingsToGhost(Node _node, bool _applyToGrid, byte _newColorIndex) {
		// mark tile for changes
		if (_applyToGrid) { 
			tilesToModify.Add(_node);
		}
	}

	protected virtual void ApplyCurrentTool() {
		// reset stuff
		ResetSelectedTiles ();
	}
}
