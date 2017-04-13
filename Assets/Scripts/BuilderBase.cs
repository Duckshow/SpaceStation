using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BuilderBase {

	protected enum ModeEnum { Default, Room, Fill, Diagonal, Door, Airlock, ObjectPlacing }
	protected ModeEnum Mode = ModeEnum.Default;

	[UnityEngine.Serialization.FormerlySerializedAs("Color_NewWall")]
	[SerializeField] protected Color Color_New = Color.white;
	[UnityEngine.Serialization.FormerlySerializedAs("Color_RemoveWall")]
	[SerializeField] protected Color Color_Remove = Color.red;
	[UnityEngine.Serialization.FormerlySerializedAs("Color_AlreadyExistingWall")]
	[SerializeField] protected Color Color_AlreadyExisting = Color.grey;
	[UnityEngine.Serialization.FormerlySerializedAs("Color_BlockedWall")]
	[SerializeField] protected Color Color_Blocked = (Color.yellow + Color.red) * 0.5f;

	[System.NonSerialized] public bool IsActive = false;

	private IEnumerator ghostRoutine;
    //public class GhostInfo {
    //	public UVController BottomQuad;
    //	public UVController TopQuad;
    //	//private SpriteRenderer _renderer;
    //	//public bool _IsActive_ { get { return _renderer.gameObject.activeSelf; } }
    //	public Vector3 position { get; private set; }
    //	public Tile.Type Type;
    //	public Tile.TileOrientation Orientation;
    //	public bool HasNeighbourGhost_Left;
    //	public bool HasNeighbourGhost_Top;
    //	public bool HasNeighbourGhost_Right;
    //	public bool HasNeighbourGhost_Bottom;

    //	public GhostInfo(UVController _bottomQuad, UVController _topQuad) {
    //		BottomQuad = _bottomQuad;
    //		TopQuad = _topQuad;
    //		//_renderer = _rend;
    //		SetPosition(Vector2.zero);
    //		Type = Tile.Type.Empty;
    //		Orientation = Tile.TileOrientation.None;
    //	}

    //	private const float DEFAULT_OFFSET_Y = 0.5f;
    //	private Vector3 newPos;
    //	public void SetPosition(Vector3 _value) {
    //		newPos = new Vector3(Grid.Instance.grid[0, 0].WorldPosition.x + _value.x, Grid.Instance.grid[0, 0].WorldPosition.y + _value.y + DEFAULT_OFFSET_Y, Grid.WORLD_TOP_HEIGHT);
    //		BottomQuad.transform.position = newPos;
    //		TopQuad.transform.position = newPos;
    //		position = _value;
    //	}
    //	public void ChangeAssets(CachedAssets.DoubleInt _bottomIndices, CachedAssets.DoubleInt _topIndices) {
    //		BottomQuad.ChangeAsset(_bottomIndices);
    //		TopQuad.ChangeAsset(_topIndices);
    //	}
    //	public void SetColor(Color _color) {
    //		BottomQuad.ChangeColor(_color);
    //		TopQuad.ChangeColor(_color);
    //	}
    //	public void SetActive(bool _b) {
    //		BottomQuad.gameObject.SetActive(_b);
    //		TopQuad.gameObject.SetActive(_b);
    //	}
    //	public void ResetHasNeighbours() {
    //		HasNeighbourGhost_Left = false;
    //		HasNeighbourGhost_Top = false;
    //		HasNeighbourGhost_Right = false;
    //		HasNeighbourGhost_Bottom = false;
    //	}
    //}
    //protected static GhostInfo[] ALL_GHOSTS;
    //private List<GhostInfo> usedGhosts = new List<GhostInfo>();


	private Vector2 oldMouseGridPos;

	private Tile startTile;
	protected Tile mouseTile;

	protected bool isDeleting = false;
	private bool modeWasChanged = false;
	private bool mouseGhostHasNewTile = false;
	protected bool mouseGhostIsDirty = true;
    private static Tile.TileOrientation cachedMouseOrientation; // to save it from being lost in reset :(

	private int distX;
	private int distY;
	private int distXAbs;
	private int distYAbs;
	private int ghostTile_GridX;
	private int ghostTile_GridY;
	private int highestAxisValue;
	private const int MAX_TILES_AXIS = 13;

	private bool isGoingDiagonal;

    protected List<CachedAssets.DoubleInt> modifiedTiles = new List<CachedAssets.DoubleInt>();
	protected List<Tile> selectedTiles = new List<Tile>();
	//protected List<Tile.Type> selectedTilesNewType = new List<Tile.Type>();
	//protected List<Tile.TileOrientation> selectedTilesNewOrientation = new List<Tile.TileOrientation>();


	public static void Setup(Transform _transform) {
		UVController[] _allQuads = _transform.GetComponentsInChildren<UVController>(true);
		//ALL_GHOSTS = new GhostInfo[(int)(_allQuads.Length * 0.5f)];
		//for (int quadIteration = 0, ghostIteration = 0; quadIteration < _allQuads.Length; quadIteration += 2, ghostIteration++) {
		//	_allQuads[quadIteration].Setup();
		//	_allQuads[quadIteration + 1].Setup();
		//	ALL_GHOSTS[ghostIteration] = new GhostInfo(_allQuads[quadIteration], _allQuads[quadIteration + 1]);
		//}
	}
	public virtual void Setup(){
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
			Mode = ModeEnum.Default;
			TryChangeMode ();
			if (Mode != _oldMode) {
				modeWasChanged = true;
				mouseGhostIsDirty = true;
                OnNewRound();
			}
			
			//// click
			//if (Mouse.StateLeft == Mouse.MouseStateEnum.Click || isDeleting)
			//	mouseGhostIsDirty = true;

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
            case ModeEnum.Default:
            case ModeEnum.Room:
            case ModeEnum.Fill:
                return true;
            case ModeEnum.Diagonal:
            case ModeEnum.Door:
            case ModeEnum.Airlock:
            case ModeEnum.ObjectPlacing:
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
        //  not sure if I need three passes rather than one, but this is definitely safer
        for (int i = 0; i < modifiedTiles.Count; i++) {
            //if (!_includingMouse && mouseTile != null && Grid.Instance.grid[modifiedTiles[i].X, modifiedTiles[i].Y] == mouseTile)
            //    continue;

            if (this is WallBuilder) {
                Grid.Instance.grid[modifiedTiles[i].X, modifiedTiles[i].Y].SetTileType(Tile.Type.Empty, Tile.TileOrientation.None, _temporarily: true);
                Grid.Instance.grid[modifiedTiles[i].X, modifiedTiles[i].Y].ChangeWallGraphics(null, null, true);
            }
            else {
                Grid.Instance.grid[modifiedTiles[i].X, modifiedTiles[i].Y].SetFloorType(Tile.Type.Empty, Tile.TileOrientation.None, _temporarily: true);
                Grid.Instance.grid[modifiedTiles[i].X, modifiedTiles[i].Y].ChangeFloorGraphics(null, true);
            }

            Grid.Instance.grid[modifiedTiles[i].X, modifiedTiles[i].Y].SetColor(Color.white);
        }

        modifiedTiles.Clear();
    }
    protected virtual void ResetSelectedTiles() {
        //  not sure if I need three passes rather than one, but this is definitely safer
        for (int i = 0; i < selectedTiles.Count; i++) {

            if (this is WallBuilder) {
                selectedTiles[i].SetTileType(Tile.Type.Empty, selectedTiles[i].TempOrientation, _temporarily: true);
                selectedTiles[i].ChangeWallGraphics(null, null, true);
            }
            else {
                selectedTiles[i].SetFloorType(Tile.Type.Empty, selectedTiles[i].TempOrientation, _temporarily: true);
                selectedTiles[i].ChangeFloorGraphics(null, true);
            }

            selectedTiles[i].SetColor(Color.white);
        }

        selectedTiles.Clear();
    }


    Tile.TileOrientation prevMouseOrientation;
	private void ControlMouseGhost() {
		// find current tile
		oldMouseGridPos = mouseTile == null ? Vector2.zero : new Vector2(mouseTile.GridX, mouseTile.GridY);
		mouseTile = Grid.Instance.GetTileFromWorldPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));

		mouseGhostHasNewTile = oldMouseGridPos.x != mouseTile.GridX || oldMouseGridPos.y != mouseTile.GridY;
		if (modeWasChanged)
			mouseGhostHasNewTile = true; // have to force my way into the sprite-update stuff below
		if (mouseGhostHasNewTile){
			mouseGhostIsDirty = true;
			ResetModifiedTiles ();
            //Grid.Instance.grid[(int)oldMouseGridPos.x, (int)oldMouseGridPos.y].TempType = Tile.Type.Empty;
            //Grid.Instance.grid[(int)oldMouseGridPos.x, (int)oldMouseGridPos.y].TempOrientation = Tile.TileOrientation.None;
            //Grid.Instance.grid[(int)oldMouseGridPos.x, (int)oldMouseGridPos.y].ChangeWallGraphics(null, null, true);
            //Grid.Instance.grid[(int)oldMouseGridPos.x, (int)oldMouseGridPos.y].ChangeFloorGraphics(null, true);
        }

        //// set position
        //ALL_GHOSTS[0].SetPosition(new Vector3(mouseTile.GridX, mouseTile.GridY, Grid.WORLD_BOTTOM_HEIGHT));

        // set rotation
        //ALL_GHOSTS[0].Orientation = TryRotateMouseGhost();
        prevMouseOrientation = mouseTile.TempOrientation;
        mouseTile.TempOrientation = TryRotateMouseGhost();
        cachedMouseOrientation = mouseTile.TempOrientation;
        if (mouseTile.TempOrientation != prevMouseOrientation)
            mouseGhostIsDirty = true;

		if (mouseGhostIsDirty) {
			mouseGhostIsDirty = false;
			DetermineGhostPositions(_hasClicked: false, _snapToNeighbours: mouseGhostHasNewTile);
		}
	}
	private Tile.TileOrientation TryRotateMouseGhost() {
        // rotate diagonals with Q&E
		int _rotateDirection = 0;
		_rotateDirection += Input.GetKeyUp(KeyCode.E) ? -1 : 0;
		_rotateDirection += Input.GetKeyUp(KeyCode.Q) ? 1 : 0;
		if (_rotateDirection != 0) {
			mouseGhostIsDirty = true;

			if (mouseTile.TempType == Tile.Type.Diagonal) {
				switch (mouseTile.TempOrientation) {
					case Tile.TileOrientation.None:
					case Tile.TileOrientation.BottomLeft:
						return _rotateDirection > 0 ? Tile.TileOrientation.BottomRight : Tile.TileOrientation.TopLeft;
					case Tile.TileOrientation.TopLeft:
						return _rotateDirection > 0 ? Tile.TileOrientation.BottomLeft : Tile.TileOrientation.TopRight;
					case Tile.TileOrientation.TopRight:
						return _rotateDirection > 0 ? Tile.TileOrientation.TopLeft : Tile.TileOrientation.BottomRight;
					case Tile.TileOrientation.BottomRight:
						return _rotateDirection > 0 ? Tile.TileOrientation.TopRight : Tile.TileOrientation.BottomLeft;
				}
			}
			else {
				switch (mouseTile.TempOrientation) {
					case Tile.TileOrientation.None:
					case Tile.TileOrientation.Bottom:
						return _rotateDirection > 0 ? Tile.TileOrientation.Right : Tile.TileOrientation.Left;
					case Tile.TileOrientation.Left:
						return _rotateDirection > 0 ? Tile.TileOrientation.Bottom : Tile.TileOrientation.Top;
					case Tile.TileOrientation.Top:
						return _rotateDirection > 0 ? Tile.TileOrientation.Left : Tile.TileOrientation.Right;
					case Tile.TileOrientation.Right:
						return _rotateDirection > 0 ? Tile.TileOrientation.Top : Tile.TileOrientation.Bottom;
				}
			}
		}

		if (mouseTile.TempOrientation == Tile.TileOrientation.None) {
			switch (Mode) {
				case ModeEnum.Default:
				case ModeEnum.Room:
				case ModeEnum.Fill:
					// don't need to do nothing
					break;
				case ModeEnum.Diagonal:
					return Tile.TileOrientation.TopLeft;
				case ModeEnum.Door:
				case ModeEnum.Airlock:
                case ModeEnum.ObjectPlacing:
					return Tile.TileOrientation.Bottom;
				default:
					throw new System.NotImplementedException(Mode + " hasn't been properly implemented yet!");
			}
		}

        return mouseTile.TempOrientation;
	}

	private bool hasMoved;
	private int oldDistX;
	private int oldDistY;
	protected void DetermineGhostPositions(bool _hasClicked, bool _snapToNeighbours) {

		// find current tile
		if(!_hasClicked || startTile == null)
			startTile = Grid.Instance.GetTileFromWorldPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
	    if(_hasClicked || startTile == null)
			mouseTile = Grid.Instance.GetTileFromWorldPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));

		if (Mode == ModeEnum.Default || Mode == ModeEnum.Room || Mode == ModeEnum.Fill) {
			// get tile distance
			oldDistX = distX;
			oldDistY = distY;
			distX = mouseTile.GridX - startTile.GridX;
			distY = mouseTile.GridY - startTile.GridY;
			hasMoved = !(oldDistX == distX && oldDistY == distY);

			// if hasn't moved, early-out
			if (!hasMoved && mouseTile != startTile)
				return;

			distXAbs = Mathf.Min(Mathf.Abs(distX), MAX_TILES_AXIS);
			distYAbs = Mathf.Min(Mathf.Abs(distY), MAX_TILES_AXIS);

			ghostTile_GridX = startTile.GridX;
			ghostTile_GridY = startTile.GridY;
		}

        ResetModifiedTiles();
        ResetSelectedTiles();
        if (cachedMouseOrientation != Tile.TileOrientation.None) {
            mouseTile.TempOrientation = cachedMouseOrientation;
            cachedMouseOrientation = Tile.TileOrientation.None;
        }

		switch (Mode) {
			// click-Modes
			case ModeEnum.Diagonal:
			case ModeEnum.Door:
			case ModeEnum.Airlock:
			case ModeEnum.ObjectPlacing:
				ghostTile_GridX = mouseTile.GridX;
				ghostTile_GridY = mouseTile.GridY;

				AddNextGhost (ghostTile_GridX, ghostTile_GridY, DetermineGhostType(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY]), DetermineGhostOrientation(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY], _snapToNeighbours), _snapToNeighbours);
                SetGhostType(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY]);
                SetGhostGraphics (Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY], _snapToNeighbours);
				break;

				// drag-Modes
			case ModeEnum.Default:
				if (!_hasClicked) {
					ghostTile_GridX = mouseTile.GridX;
					ghostTile_GridY = mouseTile.GridY;

					AddNextGhost (ghostTile_GridX, ghostTile_GridY, DetermineGhostType(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY]), DetermineGhostOrientation(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY], _snapToNeighbours), _snapToNeighbours);
                    SetGhostType(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY]);
                    SetGhostGraphics (Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY], _snapToNeighbours);
				}
				else {
					#region Default Held
					// determine if we're going to force diagonal ghosting
					highestAxisValue = Mathf.Max(distXAbs, distYAbs);
					isGoingDiagonal = Mathf.Abs(distXAbs - distYAbs) <= highestAxisValue * 0.5f;

					// first pass
					for (int i = 0; i <= highestAxisValue; i++) {
						// determine the offset from the _startTile
						if (distXAbs >= distYAbs || isGoingDiagonal)
							ghostTile_GridX = distX < 0 ? startTile.GridX - i : startTile.GridX + i;
						if (distYAbs >= distXAbs || isGoingDiagonal)
							ghostTile_GridY = distY < 0 ? startTile.GridY - i : startTile.GridY + i;

						// if outside grid, break
						if (ghostTile_GridX < 0 || ghostTile_GridX >= Grid.Instance.GridSizeX)
							break;
						if (ghostTile_GridY < 0 || ghostTile_GridY >= Grid.Instance.GridSizeY)
							break;

						AddNextGhost (ghostTile_GridX, ghostTile_GridY, DetermineGhostType(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY]), DetermineGhostOrientation(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY], _snapToNeighbours), _snapToNeighbours);
					}
                    // second pass
                    for (int i = 0; i <= highestAxisValue; i++) {
                        // determine the offset from the _startTile
                        if (distXAbs >= distYAbs || isGoingDiagonal)
                            ghostTile_GridX = distX < 0 ? startTile.GridX - i : startTile.GridX + i;
                        if (distYAbs >= distXAbs || isGoingDiagonal)
                            ghostTile_GridY = distY < 0 ? startTile.GridY - i : startTile.GridY + i;

                        // if outside grid, break
                        if (ghostTile_GridX < 0 || ghostTile_GridX >= Grid.Instance.GridSizeX)
                            break;
                        if (ghostTile_GridY < 0 || ghostTile_GridY >= Grid.Instance.GridSizeY)
                            break;

                        SetGhostType(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY]);
                    }
                    // third pass
                    for (int i = 0; i <= highestAxisValue; i++) {
						// determine the offset from the _startTile
						if (distXAbs >= distYAbs || isGoingDiagonal)
							ghostTile_GridX = distX < 0 ? startTile.GridX - i : startTile.GridX + i;
						if (distYAbs >= distXAbs || isGoingDiagonal)
							ghostTile_GridY = distY < 0 ? startTile.GridY - i : startTile.GridY + i;

						// if outside grid, break
						if (ghostTile_GridX < 0 || ghostTile_GridX >= Grid.Instance.GridSizeX)
							break;
						if (ghostTile_GridY < 0 || ghostTile_GridY >= Grid.Instance.GridSizeY)
							break;

						SetGhostGraphics (Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY], _snapToNeighbours);
					}
					#endregion
				}
				break;
			case ModeEnum.Room:
				if (!_hasClicked) {
					ghostTile_GridX = mouseTile.GridX;
					ghostTile_GridY = mouseTile.GridY;

                    AddNextGhost(ghostTile_GridX, ghostTile_GridY, DetermineGhostType(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY]), DetermineGhostOrientation(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY], _snapToNeighbours), _snapToNeighbours);
                    SetGhostType(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY]);
                    SetGhostGraphics(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY], _snapToNeighbours);
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

							if (!_isOnEdgeX && !_isOnEdgeY)
								continue;

							ghostTile_GridX = distX < 0 ? startTile.GridX - x : startTile.GridX + x;
							ghostTile_GridY = distY < 0 ? startTile.GridY - y : startTile.GridY + y;

							// if outside grid, continue (would break, but orka)
							if (ghostTile_GridX < 0 || ghostTile_GridX >= Grid.Instance.GridSizeX)
								continue;
							if (ghostTile_GridY < 0 || ghostTile_GridY >= Grid.Instance.GridSizeY)
								continue;

                            AddNextGhost(ghostTile_GridX, ghostTile_GridY, DetermineGhostType(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY]), DetermineGhostOrientation(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY], _snapToNeighbours), _snapToNeighbours);
                        }
                    }
					// second pass
					for (int y = 0; y <= distYAbs; y++) {
						_isOnEdgeY = (y == 0 || y == distYAbs);

						for (int x = 0; x <= distXAbs; x++) {
							_isOnEdgeX = (x == 0 || x == distXAbs);

							if (!_isOnEdgeX && !_isOnEdgeY)
								continue;

							ghostTile_GridX = distX < 0 ? startTile.GridX - x : startTile.GridX + x;
							ghostTile_GridY = distY < 0 ? startTile.GridY - y : startTile.GridY + y;

							// if outside grid, continue (would break, but orka)
							if (ghostTile_GridX < 0 || ghostTile_GridX >= Grid.Instance.GridSizeX)
								continue;
							if (ghostTile_GridY < 0 || ghostTile_GridY >= Grid.Instance.GridSizeY)
								continue;

                            SetGhostType(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY]);
                        }
                    }
                    // third pass
                    for (int y = 0; y <= distYAbs; y++) {
                        _isOnEdgeY = (y == 0 || y == distYAbs);

                        for (int x = 0; x <= distXAbs; x++) {
                            _isOnEdgeX = (x == 0 || x == distXAbs);

                            if (!_isOnEdgeX && !_isOnEdgeY)
                                continue;

                            ghostTile_GridX = distX < 0 ? startTile.GridX - x : startTile.GridX + x;
                            ghostTile_GridY = distY < 0 ? startTile.GridY - y : startTile.GridY + y;

                            // if outside grid, continue (would break, but orka)
                            if (ghostTile_GridX < 0 || ghostTile_GridX >= Grid.Instance.GridSizeX)
                                continue;
                            if (ghostTile_GridY < 0 || ghostTile_GridY >= Grid.Instance.GridSizeY)
                                continue;

                            SetGhostGraphics(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY], _snapToNeighbours);
                        }
                    }
                    #endregion
                }

				break;
			case ModeEnum.Fill:
				if (!_hasClicked) {
					ghostTile_GridX = mouseTile.GridX;
					ghostTile_GridY = mouseTile.GridY;

                    AddNextGhost(ghostTile_GridX, ghostTile_GridY, DetermineGhostType(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY]), DetermineGhostOrientation(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY], _snapToNeighbours), _snapToNeighbours);
                    SetGhostType(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY]);
                    SetGhostGraphics(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY], _snapToNeighbours);
                }
				else {
					#region Room Held
					// first pass
					for (int y = 0; y <= distYAbs; y++) {
						for (int x = 0; x <= distXAbs; x++) {
							ghostTile_GridX = distX < 0 ? startTile.GridX - x : startTile.GridX + x;
							ghostTile_GridY = distY < 0 ? startTile.GridY - y : startTile.GridY + y;

							// if outside grid, continue (would break, but orka)
							if (ghostTile_GridX < 0 || ghostTile_GridX >= Grid.Instance.GridSizeX)
								continue;
							if (ghostTile_GridY < 0 || ghostTile_GridY >= Grid.Instance.GridSizeY)
								continue;

                            AddNextGhost(ghostTile_GridX, ghostTile_GridY, DetermineGhostType(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY]), DetermineGhostOrientation(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY], _snapToNeighbours), _snapToNeighbours);
                        }
                    }
					// second pass
					for (int y = 0; y <= distYAbs; y++) {
						for (int x = 0; x <= distXAbs; x++) {
							ghostTile_GridX = distX < 0 ? startTile.GridX - x : startTile.GridX + x;
							ghostTile_GridY = distY < 0 ? startTile.GridY - y : startTile.GridY + y;

							// if outside grid, continue (would break, but orka)
							if (ghostTile_GridX < 0 || ghostTile_GridX >= Grid.Instance.GridSizeX)
								continue;
							if (ghostTile_GridY < 0 || ghostTile_GridY >= Grid.Instance.GridSizeY)
								continue;

                            SetGhostType(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY]);
                        }
                    }
                    // third pass
                    for (int y = 0; y <= distYAbs; y++) {
                        for (int x = 0; x <= distXAbs; x++) {
                            ghostTile_GridX = distX < 0 ? startTile.GridX - x : startTile.GridX + x;
                            ghostTile_GridY = distY < 0 ? startTile.GridY - y : startTile.GridY + y;

                            // if outside grid, continue (would break, but orka)
                            if (ghostTile_GridX < 0 || ghostTile_GridX >= Grid.Instance.GridSizeX)
                                continue;
                            if (ghostTile_GridY < 0 || ghostTile_GridY >= Grid.Instance.GridSizeY)
                                continue;

                            SetGhostGraphics(Grid.Instance.grid[ghostTile_GridX, ghostTile_GridY], _snapToNeighbours);
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

	protected void AddNextGhost(int _gridX, int _gridY, Tile.Type _tempType, Tile.TileOrientation _tempOrientation, bool _snapToNeighbours) {
		modifiedTiles.Add (new CachedAssets.DoubleInt (_gridX, _gridY));
        Grid.Instance.grid[_gridX, _gridY].TempType = _tempType;
        Grid.Instance.grid[_gridX, _gridY].TempOrientation = _tempOrientation;
    }
    protected virtual void AddGhostsForConnectedDiagonals(Tile _tile) {
	}
	protected virtual void AddGhostsForConnectedDoors(Tile _tile) {
	}

    protected virtual Tile.Type DetermineGhostType(Tile _tile) {
        return Tile.Type.Empty;
    }
    protected virtual Tile.TileOrientation DetermineGhostOrientation(Tile _tile, bool _snapToNeighbours) {
        return Tile.TileOrientation.None;
    }
	protected void SetGhostType(Tile _tile){
        if (this is WallBuilder)
            _tile.SetTileType(_tile.TempType, _tile.TempOrientation, true);
        else if(this is FloorBuilder)
            _tile.SetFloorType(_tile.TempType, _tile.TempOrientation, true);
    }
    protected virtual void SetGhostGraphics(Tile _tile, bool _snapToNeighbours) {
    }

	protected void EvaluateUsedGhostConditions() {
		//GhostInfo _ghost;
		//Tile _tile;
		//Tile.TileOrientation _orientation;

		for (int i = 0; i < modifiedTiles.Count; i++) {
			//_tile = usedTiles[i];
			////_tileUnderGhost = Grid.Instance.grid[(int)usedGhosts[i].position.x, (int)usedGhosts[i].position.y];
			//_orientation = usedGhosts[i].Orientation;

			Evaluate (Grid.Instance.grid[modifiedTiles[i].X, modifiedTiles[i].Y]);
		}
	}
	protected virtual void Evaluate(Tile _tile){
	}

	protected void ApplySettingsToGhost(Tile _tile, bool _applyToGrid, Color _newColor) {
		// apply color and position
		//_ghost.SetActive(true);
		//_ghost.SetColor(_newColor);
		//_ghost.SetPosition(new Vector2(_tile.GridX, _tile.GridY));

        _tile.SetColor(_newColor);


		// mark tile for changes
		if (_applyToGrid) {

			selectedTiles.Add(_tile);

			// add selected settings
			//selectedTilesNewType.Add(_ghost.Type);
			//selectedTilesNewOrientation.Add(_ghost.Orientation);
		}
	}

	protected virtual void ApplyCurrentTool() {
		// reset stuff
		ResetSelectedTiles ();
	}
}
