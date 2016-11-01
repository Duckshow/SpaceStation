using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WallBuilder {

    private enum ModeEnum { Default, Room, Diagonal }
    private ModeEnum Mode = ModeEnum.Default;

    [SerializeField] private SpriteRenderer GhostSpriteRend;
    [SerializeField] private Color Color_NewWall = Color.white;
    [SerializeField] private Color Color_RemoveWall = Color.red;
    [SerializeField] private Color Color_AlreadyExistingWall = Color.grey;
    [SerializeField] private Color Color_BlockedWall = (Color.yellow + Color.red) * 0.5f;

    private IEnumerator ghostRoutine;
    private SpriteRenderer[] allGhostSprites;
    public class GhostInfo {
        public SpriteRenderer Renderer;
        public Vector2 GridPosition;
        public Tile.TileType Type;
        public Tile.TileOrientation Orientation;

        public GhostInfo(SpriteRenderer _rend, Vector2 _gridPos, Tile.TileType _type, Tile.TileOrientation _orientation) {
            Renderer = _rend;
            GridPosition = _gridPos;
            Type = _type;
            Orientation = _orientation;
        }
    }
    private List<GhostInfo> usedGhostTiles = new List<GhostInfo>();

    private Vector2 startPos;
    private Vector3 mousePos;
    private Vector2 oldMouseGridPos;

    private Tile startTile;
    // TODO: can't I just reuse these two? -.-
    private Tile tileUnderMouse;
    private Tile tileUnderGhost;

    private Color ghostColor;

    private bool isDeleting = false;
    private bool modeWasChanged = false;
    private bool mouseGhostHasNewTile = false;
    private bool hasUsedGhosts = false; // used because of a yield
    private bool mouseIsDown = false; // used because of a yield
    private bool mouseGhostIsDirty = true;
    private Tile.TileOrientation mouseGhostOrientation = Tile.TileOrientation.None;
    private Tile.TileType mouseGhostType = Tile.TileType.Wall;

    private int distX;
    private int distY;
    private int distXAbs;
    private int distYAbs;
    private int ghostTile_GridX;
    private int ghostTile_GridY;
    private int highestAxisValue;
    private const int MAX_TILES_AXIS = 40;

    private bool isGoingDiagonal;

    private bool hasMoved;
    private int oldDistX;
    private int oldDistY;

    private List<Tile> selectedTiles = new List<Tile>();
    private List<Tile.TileType> selectedTilesNewType = new List<Tile.TileType>();
    private List<Tile.TileOrientation> selectedTilesNewOrientation = new List<Tile.TileOrientation>();


    public void Setup(Transform _transform) {
        allGhostSprites = _transform.GetComponentsInChildren<SpriteRenderer>(true);
    }

    public void Activate() {
        ghostRoutine = _BuildRoutine();
        Mouse.Instance.StartCoroutine(ghostRoutine);
    }
    public void DeActivate() {
        for (int i = 0; i < allGhostSprites.Length; i++)
            allGhostSprites[i].gameObject.SetActive(false);
        Mouse.Instance.StopCoroutine(ghostRoutine);
    }

    IEnumerator _BuildRoutine() {
        if (GhostSpriteRend.sprite == null)
            yield break;

        while (Mouse.Instance.Mode == Mouse.ModeEnum.BuildWalls) {

            isDeleting = Input.GetMouseButtonDown(1);

            // determine Mode
            modeWasChanged = false;
            ModeEnum _oldMode = Mode;
            Mode = ModeEnum.Default;
            if (Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftShift))
                Mode = ModeEnum.Room;
            if (Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.LeftControl) && !isDeleting)
                Mode = ModeEnum.Diagonal;
            //if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && isDeleting)
            //    Mode = ModeEnum.RoomFull;
            if (Mode != _oldMode) {
                modeWasChanged = true;
                mouseGhostIsDirty = true;
            }

            // click
            if (Input.GetMouseButtonDown(0) || isDeleting) {
                mouseIsDown = true;
                mouseGhostIsDirty = true;

                // find start tile
                startTile = Grid.Instance.GetTileFromWorldPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                startPos = new Vector2(startTile.WorldPosition.x, startTile.WorldPosition.y);
            }

            // no click
            if (!mouseIsDown || mouseGhostIsDirty) {
                GhostFollowMouse();
            }

            // click held
            while (Input.GetMouseButton(0) || Input.GetMouseButton(1)) {
                ControlBuildTool();
                yield return new WaitForSeconds(0.01f);
            }

            // click released
            if (((!Input.GetMouseButton(0) && !isDeleting) || (!Input.GetMouseButton(1) && isDeleting)) && mouseIsDown) { // replacement for GetMouseUp, which failed due to the yield above
                mouseIsDown = false;
                ApplyCurrentTool();
            }

            yield return null;
        }
    }

    void GhostFollowMouse() {
        // find current tile
        oldMouseGridPos = tileUnderMouse == null ? Vector2.zero : new Vector2(tileUnderMouse.GridX, tileUnderMouse.GridY);
        tileUnderMouse = Grid.Instance.GetTileFromWorldPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        mousePos = new Vector2(tileUnderMouse.WorldPosition.x, tileUnderMouse.WorldPosition.y);

        mouseGhostHasNewTile = oldMouseGridPos.x != tileUnderMouse.GridX || oldMouseGridPos.y != tileUnderMouse.GridY;
        if (modeWasChanged)
            mouseGhostHasNewTile = true; // have to force my way into the sprite-update stuff below
        if (mouseGhostHasNewTile)
            mouseGhostIsDirty = true;

        // set position
        allGhostSprites[0].transform.position = tileUnderMouse.WorldPosition;

        // rotate diagonals with Q&E
        int _rotateDirection = 0;
        _rotateDirection += Input.GetKeyUp(KeyCode.E) ? -1 : 0;
        _rotateDirection += Input.GetKeyUp(KeyCode.Q) ? 1 : 0;
        if (_rotateDirection != 0) {
            if (mouseGhostType == Tile.TileType.Diagonal) {
                switch (mouseGhostOrientation) {
                    case Tile.TileOrientation.None:
                    case Tile.TileOrientation.BottomLeft:
                        mouseGhostOrientation = _rotateDirection > 0 ? Tile.TileOrientation.BottomRight : Tile.TileOrientation.TopLeft;
                        break;
                    case Tile.TileOrientation.TopLeft:
                        mouseGhostOrientation = _rotateDirection > 0 ? Tile.TileOrientation.BottomLeft : Tile.TileOrientation.TopRight;
                        break;
                    case Tile.TileOrientation.TopRight:
                        mouseGhostOrientation = _rotateDirection > 0 ? Tile.TileOrientation.TopLeft : Tile.TileOrientation.BottomRight;
                        break;
                    case Tile.TileOrientation.BottomRight:
                        mouseGhostOrientation = _rotateDirection > 0 ? Tile.TileOrientation.TopRight : Tile.TileOrientation.BottomLeft;
                        break;
                }
            }
            else {
                switch (mouseGhostOrientation) {
                    case Tile.TileOrientation.None:
                    case Tile.TileOrientation.Bottom:
                        mouseGhostOrientation = _rotateDirection > 0 ? Tile.TileOrientation.Right : Tile.TileOrientation.Left;
                        break;
                    case Tile.TileOrientation.Left:
                        mouseGhostOrientation = _rotateDirection > 0 ? Tile.TileOrientation.Bottom : Tile.TileOrientation.Top;
                        break;
                    case Tile.TileOrientation.Top:
                        mouseGhostOrientation = _rotateDirection > 0 ? Tile.TileOrientation.Left : Tile.TileOrientation.Right;
                        break;
                    case Tile.TileOrientation.Right:
                        mouseGhostOrientation = _rotateDirection > 0 ? Tile.TileOrientation.Top : Tile.TileOrientation.Bottom;
                        break;
                }
            }

            mouseGhostIsDirty = true;
        }

        if (mouseGhostIsDirty) {
            mouseGhostIsDirty = false;

            Color _newColor = Color.white;
            if (Mode == ModeEnum.Diagonal) {

                // default value stuff
                if (mouseGhostType != Tile.TileType.Diagonal) {
                    mouseGhostType = Tile.TileType.Diagonal;
                    allGhostSprites[0].sprite = CachedAssets.Instance.WallSets[0].Diagonal_TopLeft;
                }

                bool _ghostFitsTiles = false;
                if ((mouseGhostHasNewTile && tileUnderMouse.HasConnectable_L && tileUnderMouse.HasConnectable_T) || (!mouseGhostHasNewTile && mouseGhostOrientation == Tile.TileOrientation.TopLeft)) {
                    mouseGhostOrientation = Tile.TileOrientation.TopLeft;
                    allGhostSprites[0].sprite = CachedAssets.Instance.WallSets[0].Diagonal_TopLeft;
                    _ghostFitsTiles = tileUnderMouse.HasConnectable_L && tileUnderMouse.HasConnectable_T;
                }
                else if ((mouseGhostHasNewTile && tileUnderMouse.HasConnectable_T && tileUnderMouse.HasConnectable_R) || (!mouseGhostHasNewTile && mouseGhostOrientation == Tile.TileOrientation.TopRight)) {
                    mouseGhostOrientation = Tile.TileOrientation.TopRight;
                    allGhostSprites[0].sprite = CachedAssets.Instance.WallSets[0].Diagonal_TopRight;
                    _ghostFitsTiles = tileUnderMouse.HasConnectable_T && tileUnderMouse.HasConnectable_R;
                }
                else if ((mouseGhostHasNewTile && tileUnderMouse.HasConnectable_R && tileUnderMouse.HasConnectable_B) || (!mouseGhostHasNewTile && mouseGhostOrientation == Tile.TileOrientation.BottomRight)) {
                    mouseGhostOrientation = Tile.TileOrientation.BottomRight;
                    allGhostSprites[0].sprite = CachedAssets.Instance.WallSets[0].Diagonal_BottomRight;
                    _ghostFitsTiles = tileUnderMouse.HasConnectable_R && tileUnderMouse.HasConnectable_B;
                }
                else if ((mouseGhostHasNewTile && tileUnderMouse.HasConnectable_B && tileUnderMouse.HasConnectable_L) || (!mouseGhostHasNewTile && mouseGhostOrientation == Tile.TileOrientation.BottomLeft)) {
                    mouseGhostOrientation = Tile.TileOrientation.BottomLeft;
                    allGhostSprites[0].sprite = CachedAssets.Instance.WallSets[0].Diagonal_BottomLeft;
                    _ghostFitsTiles = tileUnderMouse.HasConnectable_B && tileUnderMouse.HasConnectable_L;
                }

                _newColor = _ghostFitsTiles ? Color_NewWall : Color_BlockedWall;
            }
            else {
                mouseGhostType = Tile.TileType.Wall;
                mouseGhostOrientation = Tile.TileOrientation.None;
                allGhostSprites[0].sprite = CachedAssets.Instance.WallSets[0].Single;
                _newColor = Color_NewWall;
            }
            
            if (tileUnderMouse._Type_ != Tile.TileType.Empty) {
                if (Mode == ModeEnum.Diagonal && (tileUnderMouse._Type_ != Tile.TileType.Diagonal || tileUnderMouse._Orientation_ != mouseGhostOrientation))
                    _newColor = Color_BlockedWall;
                else
                    _newColor = Color_AlreadyExistingWall;
            }

            allGhostSprites[0].gameObject.SetActive(true);
            allGhostSprites[0].color = _newColor;
        }
    }

    void ControlBuildTool() {
        hasUsedGhosts = true;

        // find current tile
        tileUnderMouse = Grid.Instance.GetTileFromWorldPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        mousePos = new Vector2(tileUnderMouse.WorldPosition.x, tileUnderMouse.WorldPosition.y);

        // get tile distance
        oldDistX = distX;
        oldDistY = distY;
        distX = tileUnderMouse.GridX - startTile.GridX;
        distY = tileUnderMouse.GridY - startTile.GridY;

        // check if it's worth continuing
        hasMoved = !(oldDistX == distX && oldDistY == distY);
        if (hasMoved || tileUnderMouse == startTile) {

            // get the rest of the tile distance and shit
            distXAbs = Mathf.Min(Mathf.Abs(distX), MAX_TILES_AXIS);
            distYAbs = Mathf.Min(Mathf.Abs(distY), MAX_TILES_AXIS);

            ghostTile_GridX = startTile.GridX;
            ghostTile_GridY = startTile.GridY;
            tileUnderGhost = null;

            selectedTiles.Clear();
            selectedTilesNewType.Clear();
            selectedTilesNewOrientation.Clear();
            usedGhostTiles = new List<GhostInfo>();
            for (int i = 0; i < allGhostSprites.Length; i++)
                allGhostSprites[i].gameObject.SetActive(false);

            switch (Mode) {
                case ModeEnum.Default:
                    #region Default
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
                        if (ghostTile_GridX < 0 || ghostTile_GridX >= Grid.Instance.GridWorldSize.x)
                            break;
                        if (ghostTile_GridY < 0 || ghostTile_GridY >= Grid.Instance.GridWorldSize.y)
                            break;

                        // determine which sprite to use
                        Sprite _thisSprite = null;
                        if (isGoingDiagonal)
                            _thisSprite = CachedAssets.Instance.WallSets[0].Single;
                        else if (distXAbs > distYAbs) {
                            if (i == 0 || i == highestAxisValue) {
                                if ((distX > 0) == (i == 0))
                                    _thisSprite = CachedAssets.Instance.WallSets[0].Horizontal_L;
                                else if ((distX < 0) == (i == 0))
                                    _thisSprite = CachedAssets.Instance.WallSets[0].Horizontal_R;
                            }
                            else
                                _thisSprite = CachedAssets.Instance.WallSets[0].Horizontal_M;
                        }
                        else if (distYAbs > distXAbs) {
                            if (i == 0 || i == highestAxisValue) {
                                if ((distY > 0) == (i == 0))
                                    _thisSprite = CachedAssets.Instance.WallSets[0].Vertical_B;
                                else if ((distY < 0) == (i == 0))
                                    _thisSprite = CachedAssets.Instance.WallSets[0].Vertical_T;
                            }
                            else
                                _thisSprite = CachedAssets.Instance.WallSets[0].Vertical_M;
                        }

                        allGhostSprites[i].sprite = _thisSprite;
                        usedGhostTiles.Add(new GhostInfo(allGhostSprites[i], new Vector2(ghostTile_GridX, ghostTile_GridY), mouseGhostType, mouseGhostOrientation));
                    }
                    #endregion
                    break;
                case ModeEnum.Room:
                    #region Room
                    for (int y = 0; y <= distYAbs; y++) {
                        for (int x = 0; x <= distXAbs; x++) {
                            if ((y > 0 && y < distYAbs) && (x > 0 && x < distXAbs))
                                continue;

                            ghostTile_GridX = distX < 0 ? startTile.GridX - x : startTile.GridX + x;
                            ghostTile_GridY = distY < 0 ? startTile.GridY - y : startTile.GridY + y;

                            // if outside grid, continue
                            if (ghostTile_GridX < 0 || ghostTile_GridX >= Grid.Instance.GridWorldSize.x) {
                                //_break = true;
                                continue;
                            }
                            if (ghostTile_GridY < 0 || ghostTile_GridY >= Grid.Instance.GridWorldSize.y) {
                                //_break = true;
                                continue;
                            }

                            // determine which sprite to use
                            Sprite _thisSprite = CachedAssets.Instance.WallSets[0].Single;

                            // 1D horizontal
                            if (distXAbs > 0 && distYAbs == 0) {
                                if (x == 0)
                                    _thisSprite = distX > 0 ? CachedAssets.Instance.WallSets[0].Horizontal_L : CachedAssets.Instance.WallSets[0].Horizontal_R;
                                else if (x == distXAbs)
                                    _thisSprite = distX > 0 ? CachedAssets.Instance.WallSets[0].Horizontal_R : CachedAssets.Instance.WallSets[0].Horizontal_L;
                                else if (x > 0)
                                    _thisSprite = CachedAssets.Instance.WallSets[0].Horizontal_M;
                            }
                            // 1D vertical
                            else if (distXAbs == 0 && distYAbs > 0) {
                                if (y == 0)
                                    _thisSprite = distY > 0 ? CachedAssets.Instance.WallSets[0].Vertical_B : CachedAssets.Instance.WallSets[0].Vertical_T;
                                else if (y == distYAbs)
                                    _thisSprite = distY > 0 ? CachedAssets.Instance.WallSets[0].Vertical_T : CachedAssets.Instance.WallSets[0].Vertical_B;
                                else if (y > 0)
                                    _thisSprite = CachedAssets.Instance.WallSets[0].Vertical_M;
                            }
                            // 2D both
                            else {
                                if ((x > 0 && y == 0) || (x > 0 && y == distYAbs))
                                    _thisSprite = CachedAssets.Instance.WallSets[0].Horizontal_M;
                                else if ((x == 0 && y > 0) || (x == distXAbs && y > 0))
                                    _thisSprite = CachedAssets.Instance.WallSets[0].Vertical_M;

                                if (distX > 0 && distY > 0) {
                                    if (x == 0 && y == 0)
                                        _thisSprite = CachedAssets.Instance.WallSets[0].Corner_TopRight;
                                    else if (x == distXAbs && y == 0)
                                        _thisSprite = CachedAssets.Instance.WallSets[0].Corner_TopLeft;
                                    else if (x == distXAbs && y == distYAbs)
                                        _thisSprite = CachedAssets.Instance.WallSets[0].Corner_BottomLeft;
                                    else if (x == 0 && y == distYAbs)
                                        _thisSprite = CachedAssets.Instance.WallSets[0].Corner_BottomRight;
                                }
                                else if (distX > 0 && distY < 0) {
                                    if (x == 0 && y == 0)
                                        _thisSprite = CachedAssets.Instance.WallSets[0].Corner_BottomRight;
                                    else if (x == distXAbs && y == 0)
                                        _thisSprite = CachedAssets.Instance.WallSets[0].Corner_BottomLeft;
                                    else if (x == distXAbs && y == distYAbs)
                                        _thisSprite = CachedAssets.Instance.WallSets[0].Corner_TopLeft;
                                    else if (x == 0 && y == distYAbs)
                                        _thisSprite = CachedAssets.Instance.WallSets[0].Corner_TopRight;
                                }
                                else if (distX < 0 && distY > 0) {
                                    if (x == 0 && y == 0)
                                        _thisSprite = CachedAssets.Instance.WallSets[0].Corner_TopLeft;
                                    else if (x == distXAbs && y == 0)
                                        _thisSprite = CachedAssets.Instance.WallSets[0].Corner_TopRight;
                                    else if (x == distXAbs && y == distYAbs)
                                        _thisSprite = CachedAssets.Instance.WallSets[0].Corner_BottomRight;
                                    else if (x == 0 && y == distYAbs)
                                        _thisSprite = CachedAssets.Instance.WallSets[0].Corner_BottomLeft;
                                }
                                else if (distX < 0 && distY < 0) {
                                    if (x == 0 && y == 0)
                                        _thisSprite = CachedAssets.Instance.WallSets[0].Corner_BottomLeft;
                                    else if (x == distXAbs && y == 0)
                                        _thisSprite = CachedAssets.Instance.WallSets[0].Corner_BottomRight;
                                    else if (x == distXAbs && y == distYAbs)
                                        _thisSprite = CachedAssets.Instance.WallSets[0].Corner_TopRight;
                                    else if (x == 0 && y == distYAbs)
                                        _thisSprite = CachedAssets.Instance.WallSets[0].Corner_TopLeft;
                                }
                            }

                            allGhostSprites[usedGhostTiles.Count].sprite = _thisSprite;
                            usedGhostTiles.Add(new GhostInfo(allGhostSprites[usedGhostTiles.Count], new Vector2(ghostTile_GridX, ghostTile_GridY), mouseGhostType, mouseGhostOrientation));
                        }
                    }
                    #endregion
                    break;
                case ModeEnum.Diagonal:
                    usedGhostTiles.Add(new GhostInfo(allGhostSprites[0], new Vector2(tileUnderMouse.GridX, tileUnderMouse.GridY), mouseGhostType, mouseGhostOrientation));
                    break;
                default:
                    Debug.LogError(Mode.ToString() + " hasn't been implemented!");
                    break;
            }

            bool _applyTile = false;
			List<Tile> _foundDiagonals;
            for (int i = 0; i < usedGhostTiles.Count; i++) {
                tileUnderGhost = Grid.Instance.grid[(int)usedGhostTiles[i].GridPosition.x, (int)usedGhostTiles[i].GridPosition.y];

                // color and sparkles!
                if (isDeleting) {

                    if (tileUnderGhost.IsOccupied) {
                        ghostColor = Color_BlockedWall;
                        _applyTile = false;
                    }
                    else {
                        ghostColor = Color_RemoveWall;
                        _applyTile = true;

						// add connected diagonal neighbors to be deleted
						_foundDiagonals = GetDiagonalNeighbourTiles(tileUnderGhost);
						for (int j = 0; j < _foundDiagonals.Count; j++){
							Vector2 _gridPos = new Vector2(_foundDiagonals[j].GridX, _foundDiagonals[j].GridY);
							if(usedGhostTiles.Find(x => x.GridPosition == _gridPos) != null)
								continue;

							usedGhostTiles.Add(new GhostInfo(allGhostSprites[usedGhostTiles.Count], _gridPos, _foundDiagonals[j]._Type_, _foundDiagonals[j]._Orientation_));
						}
                    }

                }
                else if (tileUnderGhost._Type_ == Tile.TileType.Empty && !tileUnderGhost.IsOccupied) {
                    ghostColor = Color_NewWall;
                    _applyTile = true;

					// mark diagonals as unapplicable if not able to connect to neighbors
                    if (Mode == ModeEnum.Diagonal) { // TODO: this should probably be removed as Diagonal isn't really a hold-click-tool
                        if (   (usedGhostTiles[i].Orientation == Tile.TileOrientation.TopLeft && !(tileUnderGhost.HasConnectable_L && tileUnderGhost.HasConnectable_T))
                            || (usedGhostTiles[i].Orientation == Tile.TileOrientation.TopRight && !(tileUnderGhost.HasConnectable_T && tileUnderGhost.HasConnectable_R))
                            || (usedGhostTiles[i].Orientation == Tile.TileOrientation.BottomRight && !(tileUnderGhost.HasConnectable_R && tileUnderGhost.HasConnectable_B))
                            || (usedGhostTiles[i].Orientation == Tile.TileOrientation.BottomLeft && !(tileUnderGhost.HasConnectable_B && tileUnderGhost.HasConnectable_L))) {

                            ghostColor = Color_BlockedWall;
                            _applyTile = false;
                        }
                    }
                }
                else {
                    ghostColor = Color_AlreadyExistingWall;
                    _applyTile = false;
                }

                // make it do something
				if (_applyTile) {
                    selectedTiles.Add(tileUnderGhost);
                    selectedTilesNewType.Add(usedGhostTiles[i].Type);
                    selectedTilesNewOrientation.Add(usedGhostTiles[i].Orientation);
                }

                // apply stuff
                usedGhostTiles[i].Renderer.gameObject.SetActive(true);
                usedGhostTiles[i].Renderer.color = ghostColor;
                usedGhostTiles[i].Renderer.transform.position = tileUnderGhost.WorldPosition;
            }
        }
    }

    void ApplyCurrentTool() {
        if (hasUsedGhosts) {
            for (int i = 1; i < allGhostSprites.Length; i++) // start on 1 to skip the mouseGhost
                allGhostSprites[i].gameObject.SetActive(false);

            hasUsedGhosts = false;
        }

        for (int i = 0; i < selectedTiles.Count; i++) {
			if (selectedTiles[i].ConnectedDiagonal_L != null) {
				selectedTiles.Add(selectedTiles[i].ConnectedDiagonal_L);
				selectedTiles[i].ConnectedDiagonal_L = null;
			}
			if (selectedTiles[i].ConnectedDiagonal_T != null) {
				selectedTiles.Add(selectedTiles[i].ConnectedDiagonal_T);
				selectedTiles[i].ConnectedDiagonal_T = null;
			}
			if (selectedTiles[i].ConnectedDiagonal_R != null) {
				selectedTiles.Add(selectedTiles[i].ConnectedDiagonal_R);
				selectedTiles[i].ConnectedDiagonal_R = null;
			}
			if (selectedTiles[i].ConnectedDiagonal_B != null) {
				selectedTiles.Add(selectedTiles[i].ConnectedDiagonal_B);
				selectedTiles[i].ConnectedDiagonal_B = null;
			}

            // set tile info
            selectedTiles[i].SetTileType(isDeleting ? Tile.TileType.Empty : selectedTilesNewType[i], isDeleting ? Tile.TileOrientation.None : selectedTilesNewOrientation[i]);

            // apply graphics
            Grid.Instance.UpdateTile(selectedTiles[i], true, true);
            Grid.Instance.ApplyGraphics(selectedTiles[i].GridSliceIndex);
        }
    }

	List<Tile> GetDiagonalNeighbourTiles(Tile _tile){
		List<Tile> _neighbours = Grid.Instance.GetNeighbours(_tile.GridX, _tile.GridY);
		List<Tile> _foundDiagonals = new List<Tile>();
		int _diffX = 0;
		int _diffY = 0;
		Tile.TileOrientation _orientation;
		for (int i = 0; i < _neighbours.Count; i++) {
            if (_neighbours[i]._Type_ != Tile.TileType.Diagonal)
                continue;

			_orientation = _neighbours[i]._Orientation_;
			_diffX = _neighbours[i].GridX - _tile.GridX;
			_diffY = _neighbours[i].GridY - _tile.GridY;
			if(_diffX == -1 && _diffY == 0) {
				if(_orientation == Tile.TileOrientation.BottomRight || _orientation == Tile.TileOrientation.TopRight){
					_tile.ConnectedDiagonal_L = _neighbours[i];
					_foundDiagonals.Add(_neighbours[i]);
					continue;
				}
			}
			if(_diffX == 0 && _diffY == 1){
				if(_orientation == Tile.TileOrientation.BottomLeft || _orientation == Tile.TileOrientation.BottomRight){
					_tile.ConnectedDiagonal_T = _neighbours[i];
					_foundDiagonals.Add(_neighbours[i]);
					continue;
				}
			}

			if(_diffX == 1 && _diffY == 0){
				if (_orientation == Tile.TileOrientation.BottomLeft || _orientation == Tile.TileOrientation.TopLeft) {
					_tile.ConnectedDiagonal_R = _neighbours[i];
					_foundDiagonals.Add(_neighbours[i]);
					continue;
				}
			}
			
			if(_diffX == 0 && _diffY == -1){
				if (_orientation == Tile.TileOrientation.TopLeft || _orientation == Tile.TileOrientation.TopRight) {
					_tile.ConnectedDiagonal_B = _neighbours[i];
					_foundDiagonals.Add(_neighbours[i]);
					continue;
				}
			}
		}

		return _foundDiagonals;
	}
}