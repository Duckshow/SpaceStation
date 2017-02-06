﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WallBuilder {

    private enum ModeEnum { Default, Room, Diagonal, Door }
    private ModeEnum Mode = ModeEnum.Default;

    [SerializeField] private Color Color_NewWall = Color.white;
    [SerializeField] private Color Color_RemoveWall = Color.red;
    [SerializeField] private Color Color_AlreadyExistingWall = Color.grey;
    [SerializeField] private Color Color_BlockedWall = (Color.yellow + Color.red) * 0.5f;

    private IEnumerator ghostRoutine;
    public class GhostInfo {
        public UVController BottomQuad;
        public UVController TopQuad;
        //private SpriteRenderer _renderer;
        //public bool _IsActive_ { get { return _renderer.gameObject.activeSelf; } }
        public Vector3 position { get; private set; }
        public Tile.TileType Type;
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
            Type = Tile.TileType.Empty;
            Orientation = Tile.TileOrientation.None;
        }

        //private Sprite _mergedSprite;
        //private Color[] _bottomPixels;
        //private Color[] _topSpritePixels;
        //private Texture2D _mergedTexture;
        //public void SetSprites(CachedAssets.DoubleInt _assetBottom, CachedAssets.DoubleInt _assetTop, int _style) {
        //if (_spriteBottom == null || _spriteTop == null) {
        //    if (_spriteTop == null) {
        //        _renderer.sprite = _spriteBottom;
        //        return;
        //    }
        //    if (_spriteBottom == null) {
        //        _renderer.sprite = _spriteTop;
        //        return;
        //    }
        //}

        //// get pixels from sprites
        //_bottomPixels = _spriteBottom.texture.GetPixels((int)_spriteBottom.rect.x, (int)_spriteBottom.rect.y, (int)_spriteBottom.rect.width, (int)_spriteBottom.rect.height);
        //_topSpritePixels = _spriteTop.texture.GetPixels((int)_spriteTop.rect.x, (int)_spriteTop.rect.y, (int)_spriteTop.rect.width, (int)_spriteTop.rect.height);
        //MergeTextures(ref _topSpritePixels, ref _bottomPixels);

        //// if no texture, create it
        //if (_mergedTexture == null) {
        //    _mergedTexture = new Texture2D((int)_spriteBottom.rect.width, (int)_spriteBottom.rect.height, TextureFormat.RGBA32, true);
        //    _mergedTexture.filterMode = FilterMode.Point;
        //}
        //// else, resize the old one
        //else {
        //    _mergedTexture.Resize((int)Mathf.Max(_spriteBottom.rect.width, _spriteTop.rect.width), (int)Mathf.Max(_spriteBottom.rect.height, _spriteTop.rect.height));
        //}

        //// merge textures
        //_mergedTexture.SetPixels(_bottomPixels);
        //_mergedTexture.Apply();

        //// create sprite from merged texture
        //_mergedSprite = Sprite.Create(_mergedTexture, new Rect(0, 0, _mergedTexture.width, _mergedTexture.height), new Vector2(_spriteBottom.pivot.x / _spriteBottom.rect.width, _spriteBottom.pivot.y / _spriteBottom.rect.height), _spriteBottom.pixelsPerUnit);
        //_mergedSprite.name = _spriteBottom.name + " (Merged)";
        //_renderer.sprite = _mergedSprite;
        //}
        //void MergeTextures(ref Color[] _from, ref Color[] _to) {
        //    List<Color> _toPixels = new List<Color>();
        //    for (int i = 0; i < _from.Length; i++) {

        //        if (_from[i].a == 0 && i < _to.Length) {
        //             _toPixels.Add(_to[i]);
        //            continue;
        //        }

        //        _toPixels.Add(_from[i]);
        //    }

        //    _to = _toPixels.ToArray();
        //}

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
            //_renderer.sprite = null;
            //GridPosition = Vector2.zero;
            //Type = Tile.TileType.Empty;
            //Orientation = Tile.TileOrientation.None;
            HasNeighbourGhost_Left = false;
            HasNeighbourGhost_Top = false;
            HasNeighbourGhost_Right = false;
            HasNeighbourGhost_Bottom = false;
        }
    }
    private GhostInfo[] allGhosts;
    private List<GhostInfo> usedGhosts = new List<GhostInfo>();

    private Vector2 oldMouseGridPos;

    private Tile startTile;
    private Tile mouseTile;

    private bool isDeleting = false;
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
    private const int MAX_TILES_AXIS = 40;

    private bool isGoingDiagonal;

    private List<Tile> selectedTiles = new List<Tile>();
    private List<Tile.TileType> selectedTilesNewType = new List<Tile.TileType>();
    private List<Tile.TileOrientation> selectedTilesNewOrientation = new List<Tile.TileOrientation>();


    public void Setup(Transform _transform) {
        UVController[] _allQuads = _transform.GetComponentsInChildren<UVController>(true);
        allGhosts = new GhostInfo[(int)(_allQuads.Length * 0.5f)];
        for (int quadIteration = 0, ghostIteration = 0; quadIteration < _allQuads.Length; quadIteration += 2, ghostIteration++) {
            _allQuads[quadIteration].Setup();
            _allQuads[quadIteration + 1].Setup();
            allGhosts[ghostIteration] = new GhostInfo(_allQuads[quadIteration], _allQuads[quadIteration + 1]);
        }
    }

    public void Activate() {
        ghostRoutine = _Update();
        Mouse.Instance.StartCoroutine(ghostRoutine);
    }
    public void DeActivate() {
        for (int i = 0; i < allGhosts.Length; i++)
            allGhosts[i].SetActive(false);
        Mouse.Instance.StopCoroutine(ghostRoutine);
    }

    IEnumerator _Update() {
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
            if (Input.GetKey(KeyCode.Tab))
                Mode = ModeEnum.Door;

            if (Mode != _oldMode) {
                modeWasChanged = true;
                mouseGhostIsDirty = true;
            }

            // click
            if (Input.GetMouseButtonDown(0) || isDeleting) {
                mouseIsDown = true;
                mouseGhostIsDirty = true;
            }

            // no click
            if (!mouseIsDown || mouseGhostIsDirty) {
                ControlMouseGhost();
            }

            // click held
            while (Input.GetMouseButton(0) || Input.GetMouseButton(1)) {
                DetermineGhostPositions(_hasClicked: true, _snapToNeighbours: false);
                yield return new WaitForSeconds(0.01f);
            }

            // click released
            if (((!Input.GetMouseButton(0) && !isDeleting) || (!Input.GetMouseButton(1) && isDeleting)) && mouseIsDown) { // replacement for GetMouseUp, which failed due to the yield above
                mouseIsDown = false;
                ApplyCurrentTool();

                mouseGhostIsDirty = true;
            }

            yield return null;
        }
    }

    void ControlMouseGhost() {
        // find current tile
        oldMouseGridPos = mouseTile == null ? Vector2.zero : new Vector2(mouseTile.GridX, mouseTile.GridY);
        mouseTile = Grid.Instance.GetTileFromWorldPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        mouseGhostHasNewTile = oldMouseGridPos.x != mouseTile.GridX || oldMouseGridPos.y != mouseTile.GridY;
        if (modeWasChanged)
            mouseGhostHasNewTile = true; // have to force my way into the sprite-update stuff below
        if (mouseGhostHasNewTile)
            mouseGhostIsDirty = true;

        // set position
        allGhosts[0].SetPosition(new Vector3(mouseTile.GridX, mouseTile.GridY, Grid.WORLD_BOTTOM_HEIGHT));

        // set rotation
        allGhosts[0].Orientation = TryRotateMouseGhost();

        if (mouseGhostIsDirty) {
            mouseGhostIsDirty = false;
            DetermineGhostPositions(_hasClicked: false, _snapToNeighbours: mouseGhostHasNewTile);
        }
    }
    Tile.TileOrientation TryRotateMouseGhost() {
        // rotate diagonals with Q&E
        int _rotateDirection = 0;
        _rotateDirection += Input.GetKeyUp(KeyCode.E) ? -1 : 0;
        _rotateDirection += Input.GetKeyUp(KeyCode.Q) ? 1 : 0;
        if (_rotateDirection != 0) {
            mouseGhostIsDirty = true;

            if (allGhosts[0].Type == Tile.TileType.Diagonal) {
                switch (allGhosts[0].Orientation) {
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
                switch (allGhosts[0].Orientation) {
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

        if (allGhosts[0].Orientation == Tile.TileOrientation.None) {
            switch (Mode) {
                case ModeEnum.Default:
                case ModeEnum.Room:
                    // don't need to do nothing
                    break;
                case ModeEnum.Diagonal:
                    return Tile.TileOrientation.TopLeft;
                case ModeEnum.Door:
                    return Tile.TileOrientation.Bottom;
                default:
                    throw new System.NotImplementedException(Mode + " hasn't been properly implemented yet!");
            }
        }

        return allGhosts[0].Orientation;
    }

    private bool hasMoved;
    private int oldDistX;
    private int oldDistY;
    private void DetermineGhostPositions(bool _hasClicked, bool _snapToNeighbours) {

        // find current tile
        if(!_hasClicked)
            startTile = Grid.Instance.GetTileFromWorldPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        else
            mouseTile = Grid.Instance.GetTileFromWorldPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        
        if (Mode == ModeEnum.Default || Mode == ModeEnum.Room) {
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
        for (int i = 0; i < allGhosts.Length; i++) {
            allGhosts[i].ResetHasNeighbours();
            allGhosts[i].SetActive(false);
        }

        switch (Mode) {
            // click-Modes
            case ModeEnum.Diagonal:
            case ModeEnum.Door:
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
                                allGhosts[usedGhosts.Count].HasNeighbourGhost_Top = (distY > 0) ? (i < highestAxisValue) : (i > 0);
                                allGhosts[usedGhosts.Count].HasNeighbourGhost_Bottom = (distY > 0) ? (i > 0) : (i < highestAxisValue);
                            }
                            if (distXAbs > distYAbs) {
                                allGhosts[usedGhosts.Count].HasNeighbourGhost_Right = (distX > 0) ? (i < highestAxisValue) : (i > 0);
                                allGhosts[usedGhosts.Count].HasNeighbourGhost_Left = (distX > 0) ? (i > 0) : (i < highestAxisValue);
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
                                allGhosts[usedGhosts.Count].HasNeighbourGhost_Top = (distY > 0) ? (y < distYAbs) : (y > 0);
                                allGhosts[usedGhosts.Count].HasNeighbourGhost_Bottom = (distY > 0) ? (y > 0) : (y < distYAbs);
                            }
                            if (_isOnEdgeY) {
                                allGhosts[usedGhosts.Count].HasNeighbourGhost_Right = (distX > 0) ? (x < distXAbs) : (x > 0);
                                allGhosts[usedGhosts.Count].HasNeighbourGhost_Left = (distX > 0) ? (x > 0) : (x < distXAbs);
                            }

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

    void AddNextGhost(int _gridX, int _gridY, bool _snapToNeighbours) {
        if (usedGhosts.Find(x => x.position.x == _gridX && x.position.y == _gridY) != null)
            return;

        allGhosts[usedGhosts.Count].SetPosition(new Vector2(_gridX, _gridY));
        allGhosts[usedGhosts.Count].SetActive(true);
        SetGhostGraphics(ref allGhosts[usedGhosts.Count], Grid.Instance.grid[_gridX, _gridY], _snapToNeighbours);
        usedGhosts.Add(allGhosts[usedGhosts.Count]);
    }
    void AddGhostsForConnectedDiagonals(Tile _tile) {
        if (_tile.ConnectedDiagonal_B != null)
            AddNextGhost(_tile.ConnectedDiagonal_B.GridX, _tile.ConnectedDiagonal_B.GridY, false);
        if (_tile.ConnectedDiagonal_L != null)
            AddNextGhost(_tile.ConnectedDiagonal_L.GridX, _tile.ConnectedDiagonal_L.GridY, false);
        if (_tile.ConnectedDiagonal_T != null)
            AddNextGhost(_tile.ConnectedDiagonal_T.GridX, _tile.ConnectedDiagonal_T.GridY, false);
        if (_tile.ConnectedDiagonal_R != null)
            AddNextGhost(_tile.ConnectedDiagonal_R.GridX, _tile.ConnectedDiagonal_R.GridY, false);
    }
    void AddGhostsForConnectedDoors(Tile _tile) {
        if (_tile.ConnectedDoor_B != null)
            AddNextGhost(_tile.ConnectedDoor_B.GridX, _tile.ConnectedDoor_B.GridY, false);
        if (_tile.ConnectedDoor_L != null)
            AddNextGhost(_tile.ConnectedDoor_L.GridX, _tile.ConnectedDoor_L.GridY, false);
        if (_tile.ConnectedDoor_R != null)
            AddNextGhost(_tile.ConnectedDoor_R.GridX, _tile.ConnectedDoor_R.GridY, false);
        if (_tile.ConnectedDoor_T != null)
            AddNextGhost(_tile.ConnectedDoor_T.GridX, _tile.ConnectedDoor_T.GridY, false);
    }

    void SetGhostGraphics(ref GhostInfo _ghost, Tile _tileUnderGhost, bool _snapToNeighbours) {

        bool _hasConnection_Left = _ghost.HasNeighbourGhost_Left || _tileUnderGhost.HasConnectable_L;
        bool _hasConnection_Right = _ghost.HasNeighbourGhost_Right || _tileUnderGhost.HasConnectable_R;
        bool _hasConnection_Top = _ghost.HasNeighbourGhost_Top || _tileUnderGhost.HasConnectable_T;
        bool _hasConnection_Bottom = _ghost.HasNeighbourGhost_Bottom || _tileUnderGhost.HasConnectable_B;
        int _ghostGridX = (int)_ghost.position.x;
        int _ghostGridY = (int)_ghost.position.y;

        switch (Mode) {

            case ModeEnum.Default:
            case ModeEnum.Room:
                _ghost.Type = Tile.TileType.Wall;
                _ghost.Orientation = Tile.TileOrientation.None;

                //if (!_hasConnection_Left && _ghostGridX > 0)
                //    _hasConnection_Left = Grid.Instance.grid[_ghostGridX - 1, _ghostGridY]._Type_ == Tile.TileType.Wall;
                //if (!_hasConnection_Right && _ghostGridX < Grid.Instance.GridSizeX - 1)
                //    _hasConnection_Right = Grid.Instance.grid[_ghostGridX + 1, _ghostGridY]._Type_ == Tile.TileType.Wall;
                //if (!_hasConnection_Top && _ghostGridY < Grid.Instance.GridSizeY - 1)
                //    _hasConnection_Top = Grid.Instance.grid[_ghostGridX, _ghostGridY + 1]._Type_ == Tile.TileType.Wall;
                //if (!_hasConnection_Bottom && _ghostGridY > 0)
                //    _hasConnection_Bottom = Grid.Instance.grid[_ghostGridX, _ghostGridY - 1]._Type_ == Tile.TileType.Wall;

                if (isDeleting && _tileUnderGhost._Type_ != Tile.TileType.Empty) { // excluding empty because otherwise it doesn't get graphics
                    _ghost.ChangeAssets(
                        CachedAssets.Instance.GetAssetForTile(_tileUnderGhost._Type_, _tileUnderGhost._Orientation_, 0, true, _hasConnection_Left, _hasConnection_Top, _hasConnection_Right, _hasConnection_Bottom),
                        CachedAssets.Instance.GetAssetForTile(_tileUnderGhost._Type_, _tileUnderGhost._Orientation_, 0, false, _hasConnection_Left, _hasConnection_Top, _hasConnection_Right, _hasConnection_Bottom));
                }
                else {
                    _ghost.ChangeAssets(
                        CachedAssets.Instance.GetAssetForTile(_ghost.Type, _ghost.Orientation, 0, true, _hasConnection_Left, _hasConnection_Top, _hasConnection_Right, _hasConnection_Bottom),
                        CachedAssets.Instance.GetAssetForTile(_ghost.Type, _ghost.Orientation, 0, false, _hasConnection_Left, _hasConnection_Top, _hasConnection_Right, _hasConnection_Bottom));
                }
                break;

            case ModeEnum.Diagonal:

                // default values 
                _ghost.Type = Tile.TileType.Diagonal;
                _ghost.Orientation = _snapToNeighbours ? Tile.TileOrientation.TopLeft : _ghost.Orientation;
                _ghost.ChangeAssets(
                   CachedAssets.WallSet.index_Diagonal_TopLeft,
                   null);

                //_ghost.SetSprites(CachedAssets.Instance.WallSets[0].Diagonal_TopLeft.Diffuse, null);

                // diagonal top left
                if ((_snapToNeighbours && _tileUnderGhost.HasConnectable_L && _tileUnderGhost.HasConnectable_T)
                || (!_snapToNeighbours && allGhosts[0].Orientation == Tile.TileOrientation.TopLeft)) {

                    _ghost.Orientation = Tile.TileOrientation.TopLeft;
                    _ghost.ChangeAssets(
                         CachedAssets.WallSet.index_Diagonal_TopLeft,
                         null);
                    //_ghost.SetSprites(CachedAssets.Instance.WallSets[0].Diagonal_TopLeft.Diffuse, null);
                }

                // diagonal top right
                else if ((_snapToNeighbours && _tileUnderGhost.HasConnectable_T && _tileUnderGhost.HasConnectable_R)
                     || (!_snapToNeighbours && allGhosts[0].Orientation == Tile.TileOrientation.TopRight)) {

                    _ghost.Orientation = Tile.TileOrientation.TopRight;
                    _ghost.ChangeAssets(
                        CachedAssets.WallSet.index_Diagonal_TopRight,
                        null);
                    //_ghost.SetSprites(CachedAssets.Instance.WallSets[0].Diagonal_TopRight.Diffuse, null);
                }

                // diagonal bottom right
                else if ((_snapToNeighbours && _tileUnderGhost.HasConnectable_R && _tileUnderGhost.HasConnectable_B)
                     || (!_snapToNeighbours && allGhosts[0].Orientation == Tile.TileOrientation.BottomRight)) {

                    _ghost.Orientation = Tile.TileOrientation.BottomRight;
                    _ghost.ChangeAssets(
                        CachedAssets.WallSet.index_Diagonal_BottomRight,
                        null);
                    //_ghost.SetSprites(CachedAssets.Instance.WallSets[0].Diagonal_BottomRight.Diffuse, null);
                }

                // diagonal bottom left
                else if ((_snapToNeighbours && _tileUnderGhost.HasConnectable_B && _tileUnderGhost.HasConnectable_L)
                     || (!_snapToNeighbours && allGhosts[0].Orientation == Tile.TileOrientation.BottomLeft)) {

                    _ghost.Orientation = Tile.TileOrientation.BottomLeft;
                    _ghost.ChangeAssets(
                       CachedAssets.WallSet.index_Diagonal_BottomLeft,
                       null);
                    //_ghost.SetSprites(CachedAssets.Instance.WallSets[0].Diagonal_BottomLeft.Diffuse, null);
                }

                break;

            case ModeEnum.Door:

                //if (_ghost.Type == Tile.TileType.Door) {
                _ghost.Type = Tile.TileType.Door;
                _ghost.Orientation = _snapToNeighbours ? Tile.TileOrientation.Left : _ghost.Orientation;
                _ghost.ChangeAssets(
                       CachedAssets.WallSet.anim_DoorHorizontal_Open.Bottom[0],
                       CachedAssets.WallSet.anim_DoorHorizontal_Open.Top[0]);

                if ((_snapToNeighbours && (_tileUnderGhost.HasConnectable_L && _tileUnderGhost.HasConnectable_R && !_tileUnderGhost.HasConnectable_B && !_tileUnderGhost.HasConnectable_T))
                || (!_snapToNeighbours && (allGhosts[0].Orientation == Tile.TileOrientation.Left || allGhosts[0].Orientation == Tile.TileOrientation.Right))) {
                    _ghost.Orientation = Tile.TileOrientation.Left; // left or right shouldn't matter...
                    _ghost.ChangeAssets(
                              CachedAssets.WallSet.anim_DoorHorizontal_Open.GetBottomFirstFrame(),
                              CachedAssets.WallSet.anim_DoorHorizontal_Open.GetTopFirstFrame());
                }
                else if ((_snapToNeighbours && (!_tileUnderGhost.HasConnectable_L && !_tileUnderGhost.HasConnectable_R && _tileUnderGhost.HasConnectable_B && _tileUnderGhost.HasConnectable_T))
                     || (!_snapToNeighbours && (allGhosts[0].Orientation == Tile.TileOrientation.Bottom || allGhosts[0].Orientation == Tile.TileOrientation.Top))) {
                    _ghost.Orientation = Tile.TileOrientation.Bottom; // bottom or top shouldn't matter...
                    _ghost.ChangeAssets(
                          CachedAssets.WallSet.anim_DoorVertical_Open.GetBottomFirstFrame(),
                          CachedAssets.WallSet.anim_DoorVertical_Open.GetTopFirstFrame());
                }

                break;

            default:
                throw new System.NotImplementedException(Mode.ToString() + " hasn't been properly implemented yet!");
        }

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

    List<Tile> neighbours;
    int diffX;
    int diffY;
    bool isHorizontal = false;
    bool isVertical = false;
    void EvaluateUsedGhostConditions() {
        GhostInfo _ghost;
        Tile _tileUnderGhost;
        Tile.TileOrientation _orientation;

        for (int i = 0; i < usedGhosts.Count; i++) {
            _ghost = usedGhosts[i];
            _tileUnderGhost = Grid.Instance.grid[(int)usedGhosts[i].position.x, (int)usedGhosts[i].position.y];
            _orientation = usedGhosts[i].Orientation;

            // deleting old tiles
            if (isDeleting) {
                // is building even allowed?
                if (!_tileUnderGhost._BuildingAllowed_ && _tileUnderGhost._Type_ != Tile.TileType.Empty) { // empty tiles allowed for deletion bc it looks better
                    ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_BlockedWall);
                    continue;
                }

                // is the tile occupied?
                if (_tileUnderGhost.IsOccupied) {
                    ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_BlockedWall);
                    continue;
                }

                // all's good - but add connected diagonals and doors to be deleted as well!
                if (_tileUnderGhost._Type_ != Tile.TileType.Empty) {
                    AddGhostsForConnectedDiagonals(_tileUnderGhost);
                    AddGhostsForConnectedDoors(_tileUnderGhost);
                }

                ApplySettingsToGhost(_ghost, _tileUnderGhost, true, Color_RemoveWall);
                continue;
            }


            // adding new tiles

            // is building even allowed?
            if (!_tileUnderGhost._BuildingAllowed_) {
                ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_BlockedWall);
                continue;
            }
            switch (Mode) {
                case ModeEnum.Diagonal:
                    // is the tile below already a diagonal of the same orientation?
                    if (_tileUnderGhost._Type_ == Tile.TileType.Diagonal && _tileUnderGhost._Orientation_ == _ghost.Orientation) {
                        ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_AlreadyExistingWall);
                        continue;
                    }
                    // is the tile below not cleared?
                    if (_tileUnderGhost._Type_ != Tile.TileType.Empty || _tileUnderGhost.IsOccupied) {
                        ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_BlockedWall);
                        continue;
                    }

                    // does the ghost's orientation match the neighbouring walls below?
                    if ((_ghost.Orientation == Tile.TileOrientation.TopLeft && !(_tileUnderGhost.HasConnectable_L && _tileUnderGhost.HasConnectable_T))
                        || (_ghost.Orientation == Tile.TileOrientation.TopRight && !(_tileUnderGhost.HasConnectable_T && _tileUnderGhost.HasConnectable_R))
                        || (_ghost.Orientation == Tile.TileOrientation.BottomRight && !(_tileUnderGhost.HasConnectable_R && _tileUnderGhost.HasConnectable_B))
                        || (_ghost.Orientation == Tile.TileOrientation.BottomLeft && !(_tileUnderGhost.HasConnectable_B && _tileUnderGhost.HasConnectable_L))) {

                        ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_BlockedWall);
                        continue;
                    }
                    break;

                case ModeEnum.Door:

                    // is the tile... living on the edge? B)
                    if (_tileUnderGhost.GridX == 0 || _tileUnderGhost.GridX == Grid.Instance.GridSizeX - 1 || _tileUnderGhost.GridY == 0 || _tileUnderGhost.GridY == Grid.Instance.GridSizeY) {
                        ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_BlockedWall);
                        continue;
                    }

                    //if (_type == Tile.TileType.Door) {
                    isHorizontal = _orientation == Tile.TileOrientation.Left || _orientation == Tile.TileOrientation.Right;
                    isVertical = _orientation == Tile.TileOrientation.Bottom || _orientation == Tile.TileOrientation.Top;

                    // does the tile have adjacent walls for the door to be in?
                    if (isHorizontal && (!_tileUnderGhost.HasConnectable_L || !_tileUnderGhost.HasConnectable_R)
                       || isVertical && (!_tileUnderGhost.HasConnectable_B || !_tileUnderGhost.HasConnectable_T)) {

                        ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_BlockedWall);
                        continue;
                    }

                    // does the tile have space for door entrances?
                    bool _failed = false;
                    neighbours = Grid.Instance.GetNeighbours(_tileUnderGhost.GridX, _tileUnderGhost.GridY);
                    for (int j = 0; j < neighbours.Count; j++) {
                        diffX = neighbours[j].GridX - _tileUnderGhost.GridX;
                        diffY = neighbours[j].GridY - _tileUnderGhost.GridY;

                        if (((isHorizontal && (diffX == 0 && diffY != 0)) || (isVertical && (diffX != 0 && diffY == 0))) && neighbours[j]._Type_ != Tile.TileType.Empty) {
                            ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_BlockedWall);
                            _failed = true;
                            break;
                        }
                    }
                    if (_failed)
                        continue;

                    // is there already a door?
                    if (_tileUnderGhost._Type_ == Tile.TileType.Door) {
                        ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_AlreadyExistingWall);
                        continue;
                    }
                    //}
                    //else if (_type == Tile.TileType.Empty) { // door entrance should never write to grid
                    //    ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_NewWall);
                    //    return;
                    //}

                    // is the tile below not cleared?
                    if (_tileUnderGhost.IsOccupied) {
                        ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_BlockedWall);
                        continue;
                    }
                    break;

                case ModeEnum.Default:
                case ModeEnum.Room:
                    // is the tile below already a wall?
                    if (_tileUnderGhost._Type_ == Tile.TileType.Wall) {
                        ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_AlreadyExistingWall);
                        continue;
                    }
                    // is the tile below not cleared?
                    if (_tileUnderGhost._Type_ != Tile.TileType.Empty || _tileUnderGhost.IsOccupied) {
                        ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_BlockedWall);
                        continue;
                    }
                    break;

                default:
                    throw new System.NotImplementedException(Mode.ToString() + " hasn't been fully implemented yet!");
            }

            // all's good
            ApplySettingsToGhost(_ghost, _tileUnderGhost, true, Color_NewWall);
        }
    }

    void ApplySettingsToGhost(GhostInfo _ghost, Tile _tileUnderGhost, bool _applyToGrid, Color _newColor) {
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

    void ApplyCurrentTool() {
        for (int i = 0; i < selectedTiles.Count; i++) {
            selectedTiles[i].SetTileType(isDeleting ? Tile.TileType.Empty : selectedTilesNewType[i], isDeleting ? Tile.TileOrientation.None : selectedTilesNewOrientation[i]);
            //Grid.Instance.UpdateNeighbours(selectedTiles[i]);
            //Grid.Instance.ApplyGraphics(selectedTiles[i].GridSliceIndex);
        }

        // reset stuff
        selectedTiles.Clear();
        selectedTilesNewType.Clear();
        selectedTilesNewOrientation.Clear();
        usedGhosts.Clear();
        for (int i = 0; i < allGhosts.Length; i++) {
            allGhosts[i].ResetHasNeighbours();
            allGhosts[i].SetActive(false);
        }
    }
}