using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BuilderBase {

	protected enum ModeEnum { Default, Room, Fill, Diagonal, Door, Airlock }
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
	public class GhostInfo {
		public UVController BottomQuad;
		public UVController TopQuad;
		//private SpriteRenderer _renderer;
		//public bool _IsActive_ { get { return _renderer.gameObject.activeSelf; } }
		public Vector3 position { get; private set; }
		public Tile.Type Type;
		public Tile.TileOrientation Orientation;
		public bool HasNeighbourGhost_Left;
		public bool HasNeighbourGhost_Top;
		public bool HasNeighbourGhost_Right;
		public bool HasNeighbourGhost_Bottom;

		public GhostInfo(UVController _bottomQuad, UVController _topQuad) {
			BottomQuad = _bottomQuad;
			TopQuad = _topQuad;
			//_renderer = _rend;
			SetPosition(Vector2.zero);
			Type = Tile.Type.Empty;
			Orientation = Tile.TileOrientation.None;
		}

		private const float DEFAULT_OFFSET_Y = 0.5f;
		private Vector3 newPos;
		public void SetPosition(Vector3 _value) {
			newPos = new Vector3(Grid.Instance.grid[0, 0].WorldPosition.x + _value.x, Grid.Instance.grid[0, 0].WorldPosition.y + _value.y + DEFAULT_OFFSET_Y, Grid.WORLD_TOP_HEIGHT);
			BottomQuad.transform.position = newPos;
			newPos.z -= 0.01f; // needs to be higher than the top-height, right? ( '.__.)
			TopQuad.transform.position = newPos;
			position = _value;
		}
		public void ChangeAssets(CachedAssets.DoubleInt _bottomIndices, CachedAssets.DoubleInt _topIndices) {
			BottomQuad.ChangeAsset(_bottomIndices);
			TopQuad.ChangeAsset(_topIndices);
		}
		public void SetColor(Color _color) {
			BottomQuad.ChangeColor(_color);
			TopQuad.ChangeColor(_color);
		}
		public void SetActive(bool _b) {
			BottomQuad.gameObject.SetActive(_b);
			TopQuad.gameObject.SetActive(_b);
		}
		public void ResetHasNeighbours() {
			HasNeighbourGhost_Left = false;
			HasNeighbourGhost_Top = false;
			HasNeighbourGhost_Right = false;
			HasNeighbourGhost_Bottom = false;
		}
	}
	protected static GhostInfo[] ALL_GHOSTS;
	private List<GhostInfo> usedGhosts = new List<GhostInfo>();

	private Vector2 oldMouseGridPos;

	private Tile startTile;
	private Tile mouseTile;

	protected bool isDeleting = false;
	private bool modeWasChanged = false;
	private bool mouseGhostHasNewTile = false;
	private bool mouseIsDown = false; // used because of a yield
	private bool mouseGhostIsDirty = true;

	private int distX;
	private int distY;
	private int distXAbs;
	private int distYAbs;
	private int ghostTile_GridX;
	private int ghostTile_GridY;
	private int highestAxisValue;
	private const int MAX_TILES_AXIS = 13;

	private bool isGoingDiagonal;

	protected List<Tile> selectedTiles = new List<Tile>();
	protected List<Tile.Type> selectedTilesNewType = new List<Tile.Type>();
	protected List<Tile.TileOrientation> selectedTilesNewOrientation = new List<Tile.TileOrientation>();


	public static void Setup(Transform _transform) {
		UVController[] _allQuads = _transform.GetComponentsInChildren<UVController>(true);
		ALL_GHOSTS = new GhostInfo[(int)(_allQuads.Length * 0.5f)];
		for (int quadIteration = 0, ghostIteration = 0; quadIteration < _allQuads.Length; quadIteration += 2, ghostIteration++) {
			_allQuads[quadIteration].Setup();
			_allQuads[quadIteration + 1].Setup();
			ALL_GHOSTS[ghostIteration] = new GhostInfo(_allQuads[quadIteration], _allQuads[quadIteration + 1]);
		}
	}
	public virtual void Setup(){
	}

	public void Activate() {
		if (IsActive)
			return;
		IsActive = true;

		modeWasChanged = true; // enables mouseghost to "hotload" when tabbing between builders
	}
	public void DeActivate() {
		if (!IsActive)
			return;
		IsActive = false;

		for (int i = 0; i < ALL_GHOSTS.Length; i++)
			ALL_GHOSTS[i].SetActive(false);
	}
		
	float timeLastGhostUpdate = -1;
	public void Update() {
		isDeleting = Mouse.StateRight != Mouse.MouseStateEnum.None;
		if ((Mouse.StateLeft == Mouse.MouseStateEnum.None || Mouse.StateLeft == Mouse.MouseStateEnum.Click) && (Mouse.StateRight == Mouse.MouseStateEnum.None || Mouse.StateRight == Mouse.MouseStateEnum.Click)) {
			InheritedUpdate ();

			// determine Mode
			ModeEnum _oldMode = Mode;
			Mode = ModeEnum.Default;
			TryChangeMode ();
			if (Mode != _oldMode) {
				modeWasChanged = true;
				mouseGhostIsDirty = true;
			}
			
			// click
			if (Mouse.StateLeft == Mouse.MouseStateEnum.Click || isDeleting) {
				mouseIsDown = true;
				mouseGhostIsDirty = true;
			}
			
			// no click
			if (!mouseIsDown || mouseGhostIsDirty)
				ControlMouseGhost();
		}


		bool skip = (!isDeleting && Mouse.StateLeft == Mouse.MouseStateEnum.Release) || (isDeleting && Mouse.StateRight == Mouse.MouseStateEnum.Release);
		if (!skip && Time.time - timeLastGhostUpdate < 0.01f)
			return;
		if (Mouse.StateLeft == Mouse.MouseStateEnum.Hold || Mouse.StateRight == Mouse.MouseStateEnum.Hold) {
			DetermineGhostPositions(_hasClicked: true, _snapToNeighbours: false);
			timeLastGhostUpdate = Time.time;
		}

		// click released
		if((Mouse.StateLeft == Mouse.MouseStateEnum.Release && !isDeleting) || (Mouse.StateRight == Mouse.MouseStateEnum.Release && isDeleting)){ // TODO: will fail because of yield!!
		mouseIsDown = false;
			ApplyCurrentTool();

			mouseGhostIsDirty = true;
		}

		//yield return null;
		modeWasChanged = false;
	}
	protected virtual void InheritedUpdate(){
	}

	protected virtual void TryChangeMode(){
	}

	private void ControlMouseGhost() {
		// find current tile
		oldMouseGridPos = mouseTile == null ? Vector2.zero : new Vector2(mouseTile.GridX, mouseTile.GridY);
		mouseTile = Grid.Instance.GetTileFromWorldPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));

		mouseGhostHasNewTile = oldMouseGridPos.x != mouseTile.GridX || oldMouseGridPos.y != mouseTile.GridY;
		if (modeWasChanged)
			mouseGhostHasNewTile = true; // have to force my way into the sprite-update stuff below
		if (mouseGhostHasNewTile)
			mouseGhostIsDirty = true;

		// set position
		ALL_GHOSTS[0].SetPosition(new Vector3(mouseTile.GridX, mouseTile.GridY, Grid.WORLD_BOTTOM_HEIGHT));

		// set rotation
		ALL_GHOSTS[0].Orientation = TryRotateMouseGhost();

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

			if (ALL_GHOSTS[0].Type == Tile.Type.Diagonal) {
				switch (ALL_GHOSTS[0].Orientation) {
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
				switch (ALL_GHOSTS[0].Orientation) {
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

		if (ALL_GHOSTS[0].Orientation == Tile.TileOrientation.None) {
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
					return Tile.TileOrientation.Bottom;
				default:
					throw new System.NotImplementedException(Mode + " hasn't been properly implemented yet!");
			}
		}

		return ALL_GHOSTS[0].Orientation;
	}

	private bool hasMoved;
	private int oldDistX;
	private int oldDistY;
	protected void DetermineGhostPositions(bool _hasClicked, bool _snapToNeighbours) {

		// find current tile
		if(!_hasClicked)
			startTile = Grid.Instance.GetTileFromWorldPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
		else
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

		// reset old stuff
		selectedTiles.Clear();
		selectedTilesNewType.Clear();
		selectedTilesNewOrientation.Clear();
		usedGhosts.Clear();
		for (int i = 0; i < ALL_GHOSTS.Length; i++) {
			ALL_GHOSTS[i].ResetHasNeighbours();
			ALL_GHOSTS[i].SetActive(false);
		}

		switch (Mode) {
			// click-Modes
			case ModeEnum.Diagonal:
			case ModeEnum.Door:
			case ModeEnum.Airlock:
				ghostTile_GridX = mouseTile.GridX;
				ghostTile_GridY = mouseTile.GridY;
				AddNextGhost(ghostTile_GridX, ghostTile_GridY, _snapToNeighbours);
				break;

				// drag-Modes
			case ModeEnum.Default:
				if (!_hasClicked) {
					ghostTile_GridX = mouseTile.GridX;
					ghostTile_GridY = mouseTile.GridY;
					AddNextGhost(ghostTile_GridX, ghostTile_GridY, _snapToNeighbours);
				}
				else {
					#region Default Held
					// determine if we're going to force diagonal ghosting
					highestAxisValue = Mathf.Max(distXAbs, distYAbs);
					isGoingDiagonal = Mathf.Abs(distXAbs - distYAbs) <= highestAxisValue * 0.5f;

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

						if (!isGoingDiagonal) {
							if (distYAbs > distXAbs) {
								ALL_GHOSTS[usedGhosts.Count].HasNeighbourGhost_Top = (distY > 0) ? (i < highestAxisValue) : (i > 0);
								ALL_GHOSTS[usedGhosts.Count].HasNeighbourGhost_Bottom = (distY > 0) ? (i > 0) : (i < highestAxisValue);
							}
							if (distXAbs > distYAbs) {
								ALL_GHOSTS[usedGhosts.Count].HasNeighbourGhost_Right = (distX > 0) ? (i < highestAxisValue) : (i > 0);
								ALL_GHOSTS[usedGhosts.Count].HasNeighbourGhost_Left = (distX > 0) ? (i > 0) : (i < highestAxisValue);
							}
						}

						AddNextGhost(ghostTile_GridX, ghostTile_GridY, _snapToNeighbours);
					}
					#endregion
				}
				break;
			case ModeEnum.Room:
				if (!_hasClicked) {
					ghostTile_GridX = mouseTile.GridX;
					ghostTile_GridY = mouseTile.GridY;
					AddNextGhost(ghostTile_GridX, ghostTile_GridY, _snapToNeighbours);
				}
				else {
					#region Room Held
					bool _isOnEdgeX = true;
					bool _isOnEdgeY = true;

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

							if (_isOnEdgeX) {
								ALL_GHOSTS[usedGhosts.Count].HasNeighbourGhost_Top = (distY > 0) ? (y < distYAbs) : (y > 0);
								ALL_GHOSTS[usedGhosts.Count].HasNeighbourGhost_Bottom = (distY > 0) ? (y > 0) : (y < distYAbs);
							}
							if (_isOnEdgeY) {
								ALL_GHOSTS[usedGhosts.Count].HasNeighbourGhost_Right = (distX > 0) ? (x < distXAbs) : (x > 0);
								ALL_GHOSTS[usedGhosts.Count].HasNeighbourGhost_Left = (distX > 0) ? (x > 0) : (x < distXAbs);
							}

							AddNextGhost(ghostTile_GridX, ghostTile_GridY, _snapToNeighbours);
						}
					}
					#endregion
				}

				break;
			case ModeEnum.Fill:
				if (!_hasClicked) {
					ghostTile_GridX = mouseTile.GridX;
					ghostTile_GridY = mouseTile.GridY;
					AddNextGhost(ghostTile_GridX, ghostTile_GridY, _snapToNeighbours);
				}
				else {
					#region Room Held
					for (int y = 0; y <= distYAbs; y++) {
						for (int x = 0; x <= distXAbs; x++) {
							ghostTile_GridX = distX < 0 ? startTile.GridX - x : startTile.GridX + x;
							ghostTile_GridY = distY < 0 ? startTile.GridY - y : startTile.GridY + y;

							// if outside grid, continue (would break, but orka)
							if (ghostTile_GridX < 0 || ghostTile_GridX >= Grid.Instance.GridSizeX)
								continue;
							if (ghostTile_GridY < 0 || ghostTile_GridY >= Grid.Instance.GridSizeY)
								continue;

								ALL_GHOSTS[usedGhosts.Count].HasNeighbourGhost_Top = (distY > 0) ? (y < distYAbs) : (y > 0);
								ALL_GHOSTS[usedGhosts.Count].HasNeighbourGhost_Bottom = (distY > 0) ? (y > 0) : (y < distYAbs);
								ALL_GHOSTS[usedGhosts.Count].HasNeighbourGhost_Right = (distX > 0) ? (x < distXAbs) : (x > 0);
								ALL_GHOSTS[usedGhosts.Count].HasNeighbourGhost_Left = (distX > 0) ? (x > 0) : (x < distXAbs);

							AddNextGhost(ghostTile_GridX, ghostTile_GridY, _snapToNeighbours);
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

	protected void AddNextGhost(int _gridX, int _gridY, bool _snapToNeighbours) {
		if (usedGhosts.Find(x => x.position.x == _gridX && x.position.y == _gridY) != null)
			return;

		ALL_GHOSTS[usedGhosts.Count].SetPosition(new Vector2(_gridX, _gridY));
		ALL_GHOSTS[usedGhosts.Count].SetActive(true);
		SetGhostGraphics(ref ALL_GHOSTS[usedGhosts.Count], Grid.Instance.grid[_gridX, _gridY], _snapToNeighbours);
		usedGhosts.Add(ALL_GHOSTS[usedGhosts.Count]);
	}
	protected virtual void AddGhostsForConnectedDiagonals(Tile _tile) {
	}
	protected virtual void AddGhostsForConnectedDoors(Tile _tile) {
	}

	protected virtual void SetGhostGraphics(ref GhostInfo _ghost, Tile _tileUnderGhost, bool _snapToNeighbours) {

		// if a diagonal is below, sort ghost so the diagonal covers it in a pretty way
		if (_tileUnderGhost.ConnectedDiagonal_B != null) {
			_ghost.BottomQuad.SortCustom(_tileUnderGhost.BottomQuad.GetSortOrder() + 1);
			_ghost.TopQuad.SortCustom(_tileUnderGhost.TopQuad.GetSortOrder() + 1);
		}
		// otherwise just go on top
		else {
			_ghost.BottomQuad.SortCustom(_tileUnderGhost.TopQuad.GetSortOrder() + 1);
			_ghost.TopQuad.SortCustom(_tileUnderGhost.TopQuad.GetSortOrder() + 2);
		}
	}

	protected void EvaluateUsedGhostConditions() {
		GhostInfo _ghost;
		Tile _tileUnderGhost;
		Tile.TileOrientation _orientation;

		for (int i = 0; i < usedGhosts.Count; i++) {
			_ghost = usedGhosts[i];
			_tileUnderGhost = Grid.Instance.grid[(int)usedGhosts[i].position.x, (int)usedGhosts[i].position.y];
			_orientation = usedGhosts[i].Orientation;

			Evaluate (_ghost, _tileUnderGhost, _orientation);
		}
	}
	protected virtual void Evaluate(GhostInfo _ghost, Tile _tileUnderGhost, Tile.TileOrientation _orientation){
	}

	protected void ApplySettingsToGhost(GhostInfo _ghost, Tile _tileUnderGhost, bool _applyToGrid, Color _newColor) {
		// apply color and position
		_ghost.SetActive(true);
		_ghost.SetColor(_newColor);
		_ghost.SetPosition(new Vector2(_tileUnderGhost.GridX, _tileUnderGhost.GridY));

		// mark tile for changes
		if (_applyToGrid) {

			selectedTiles.Add(_tileUnderGhost);

			// add selected settings
			selectedTilesNewType.Add(_ghost.Type);
			selectedTilesNewOrientation.Add(_ghost.Orientation);
		}
	}

	protected virtual void ApplyCurrentTool() {
		// reset stuff
		selectedTiles.Clear();
		selectedTilesNewType.Clear();
		selectedTilesNewOrientation.Clear();
		usedGhosts.Clear();
		for (int i = 0; i < ALL_GHOSTS.Length; i++) {
			ALL_GHOSTS[i].ResetHasNeighbours();
			ALL_GHOSTS[i].SetActive(false);
		}
	}
}
