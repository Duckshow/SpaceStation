using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WallBuilder {

    private enum ModeEnum { Default, Room, Diagonal }
    private ModeEnum Mode = ModeEnum.Default;

    [SerializeField]
    private SpriteRenderer ghostSpriteRend;

    private IEnumerator ghostRoutine;
    private SpriteRenderer[] allGhostSprites;
    public class GhostInfo {
        public SpriteRenderer Renderer;
        public Vector2 GridPosition;
        public Tile.TileType Type;

        public GhostInfo(SpriteRenderer _rend, Vector2 _gridPos, Tile.TileType _type) {
            Renderer = _rend;
            GridPosition = _gridPos;
            Type = _type;
        }
    }
    private List<GhostInfo> usedGhostTiles = new List<GhostInfo>();

    private Vector2 startPos;
    private Vector3 mousePos;
    private Vector2 oldMouseGridPos;

    private Tile startTile;
    private Tile mouseTile;
    private Tile tileUnderGhost;

    private Color color;

    private bool modeWasChanged = false;
    private bool mouseGhostHasNewTile = false;
    private bool hasUsedGhosts = false;
    private bool mouseIsDown = false;
    private bool mouseGhostIsDirty = true;
    private int mouseGhostRotation = 0;
    private Tile.TileType mouseGhostType = Tile.TileType.Wall;

    private int distX;
    private int distY;
    private int distXAbs;
    private int distYAbs;
    private int highestAxisValue;
    private int ghostTile_GridX;
    private int ghostTile_GridY;

    private bool isGoingDiagonal;

    private bool hasMoved;
    private int oldDistX;
    private int oldDistY;

    private List<Tile> selectedTiles = new List<Tile>();
    private List<Tile.TileType> selectedTilesType = new List<Tile.TileType>();


    public void Setup(Transform _transform) {
        allGhostSprites = _transform.GetComponentsInChildren<SpriteRenderer>();
    }

    public void Activate() {
        ghostRoutine = _GhostSpriteFollowMouse();
        Mouse.Instance.StartCoroutine(ghostRoutine);
    }
    public void DeActivate() {
        for (int i = 0; i < allGhostSprites.Length; i++)
            allGhostSprites[i].enabled = false;
        Mouse.Instance.StopCoroutine(ghostRoutine);
    }

    IEnumerator _GhostSpriteFollowMouse() {
        if (ghostSpriteRend.sprite == null)
            yield break;

        while (Mouse.Instance.CurrentMode == Mouse.Mode.BuildWalls) {

            // determine Mode
            modeWasChanged = false;
            ModeEnum _oldMode = Mode;
            if (Input.GetKey(KeyCode.LeftControl))
                Mode = ModeEnum.Room;
            else if (Input.GetKey(KeyCode.LeftShift))
                Mode = ModeEnum.Diagonal;
            else
                Mode = ModeEnum.Default;
            if (Mode != _oldMode) {
                modeWasChanged = true;
                mouseGhostIsDirty = true;
            }

            // click
            if (Input.GetMouseButtonDown(0)) {
                mouseIsDown = true;
                mouseGhostIsDirty = true;

                // find start tile
                startTile = Grid.Instance.GetNodeFromWorldPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                startPos = new Vector2(startTile.WorldPosition.x, startTile.WorldPosition.z);
            }

            // no click
            if (!mouseIsDown || mouseGhostIsDirty) {

                // find current tile
                oldMouseGridPos = mouseTile == null ? Vector2.zero : new Vector2(mouseTile.GridX, mouseTile.GridY);
                mouseTile = Grid.Instance.GetNodeFromWorldPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                mousePos = new Vector2(mouseTile.WorldPosition.x, mouseTile.WorldPosition.z);

                mouseGhostHasNewTile = oldMouseGridPos.x != mouseTile.GridX || oldMouseGridPos.y != mouseTile.GridY;
                if (modeWasChanged)
                    mouseGhostHasNewTile = true; // have to force my way into the sprite-update stuff below
                if (mouseGhostHasNewTile)
                    mouseGhostIsDirty = true;

                // set position
                allGhostSprites[0].transform.position = mouseTile.WorldPosition;

                // rotate diagonals with Q&E
                if (Mode == ModeEnum.Diagonal) {
                    if (Input.GetKeyUp(KeyCode.E)) {
                        mouseGhostRotation++;
                        if (mouseGhostRotation > 3)
                            mouseGhostRotation = 0;

                        mouseGhostIsDirty = true;
                    }
                    if (Input.GetKeyUp(KeyCode.Q)) {
                        mouseGhostRotation--;
                        if (mouseGhostRotation < 0)
                            mouseGhostRotation = 3;

                        mouseGhostIsDirty = true;
                    }
                }

                if (mouseGhostIsDirty) {
                    mouseGhostIsDirty = false;

                    Color _newColor = Color.white;
                    if (Mode == ModeEnum.Diagonal) {
                        allGhostSprites[0].sprite = Grid.Instance.CachedAssets.Wall_0_Diagonal_LT; // just as a default value kind of thing

                        bool _ghostFitsTiles = false;
                        if ((mouseGhostHasNewTile && mouseTile.HasConnectable_L && mouseTile.HasConnectable_T) || (!mouseGhostHasNewTile && mouseGhostRotation == 0)) {
                            mouseGhostRotation = 0;
                            mouseGhostType = Tile.TileType.Diagonal_LT;
                            allGhostSprites[0].sprite = Grid.Instance.CachedAssets.Wall_0_Diagonal_LT;
                            _ghostFitsTiles = mouseTile.HasConnectable_L && mouseTile.HasConnectable_T;
                        }
                        else if ((mouseGhostHasNewTile && mouseTile.HasConnectable_T && mouseTile.HasConnectable_R) || (!mouseGhostHasNewTile && mouseGhostRotation == 1)) {
                            mouseGhostRotation = 1;
                            mouseGhostType = Tile.TileType.Diagonal_TR;
                            allGhostSprites[0].sprite = Grid.Instance.CachedAssets.Wall_0_Diagonal_TR;
                            _ghostFitsTiles = mouseTile.HasConnectable_T && mouseTile.HasConnectable_R;
                        }
                        else if ((mouseGhostHasNewTile && mouseTile.HasConnectable_R && mouseTile.HasConnectable_B) || (!mouseGhostHasNewTile && mouseGhostRotation == 2)) {
                            mouseGhostRotation = 2;
                            mouseGhostType = Tile.TileType.Diagonal_RB;
                            allGhostSprites[0].sprite = Grid.Instance.CachedAssets.Wall_0_Diagonal_RB;
                            _ghostFitsTiles = mouseTile.HasConnectable_R && mouseTile.HasConnectable_B;
                        }
                        else if ((mouseGhostHasNewTile && mouseTile.HasConnectable_B && mouseTile.HasConnectable_L) || (!mouseGhostHasNewTile && mouseGhostRotation == 3)) {
                            mouseGhostRotation = 3;
                            mouseGhostType = Tile.TileType.Diagonal_BL;
                            allGhostSprites[0].sprite = Grid.Instance.CachedAssets.Wall_0_Diagonal_BL;
                            _ghostFitsTiles = mouseTile.HasConnectable_B && mouseTile.HasConnectable_L;
                        }

                        _newColor = _ghostFitsTiles ? Color.white : Color.red;
                    }
                    else {
                        mouseGhostType = Tile.TileType.Wall;
                        allGhostSprites[0].sprite = Grid.Instance.CachedAssets.Wall_0_Single;
                        _newColor = Color.white;
                    }

                    if (mouseTile._Type_ != Tile.TileType.Default)
                        _newColor = Color.red;

                    //Debug.Log("(" + mouseTile.GridX + ", " + mouseTile.GridY + ") is " + mouseTile._Type_.ToString());
                    allGhostSprites[0].enabled = true;
                    _newColor.a = 0.5f;
                    allGhostSprites[0].color = _newColor;
                }
            }

            // click held
            while (Input.GetMouseButton(0)) {
                hasUsedGhosts = true;

                // find current tile
                mouseTile = Grid.Instance.GetNodeFromWorldPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                mousePos = new Vector2(mouseTile.WorldPosition.x, mouseTile.WorldPosition.z);

                // get tile distance
                oldDistX = distX;
                oldDistY = distY;
                distX = mouseTile.GridX - startTile.GridX;
                distY = mouseTile.GridY - startTile.GridY;

                // check if it's worth continuing
                hasMoved = !(oldDistX == distX && oldDistY == distY);
                if (hasMoved || mouseTile == startTile) {

                    // get the rest of the tile distance and shit
                    distXAbs = Mathf.Abs(distX);
                    distYAbs = Mathf.Abs(distY);

                    ghostTile_GridX = startTile.GridX;
                    ghostTile_GridY = startTile.GridY;
                    tileUnderGhost = null;

                    selectedTiles.Clear();
                    selectedTilesType.Clear();
                    usedGhostTiles = new List<GhostInfo>();
                    for (int i = 0; i < allGhostSprites.Length; i++)
                        allGhostSprites[i].enabled = false;
                    switch (Mode) {
                        case ModeEnum.Default:
                            #region Default
                            // determine if we're going to force diagonal ghosting
                            highestAxisValue = Mathf.Clamp(Mathf.Max(distXAbs, distYAbs), 0, allGhostSprites.Length);
                            isGoingDiagonal = Mathf.Abs(distXAbs - distYAbs) <= Mathf.RoundToInt(highestAxisValue * 0.5f);

                            for (int i = 0; i < allGhostSprites.Length; i++) {
                                if (i <= highestAxisValue) {
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
                                        _thisSprite = Grid.Instance.CachedAssets.Wall_0_Single;
                                    else if (distXAbs > distYAbs) {
                                        if (i == 0 || i == highestAxisValue) {
                                            if ((distX > 0) == (i == 0))
                                                _thisSprite = Grid.Instance.CachedAssets.Wall_0_Horizontal_L;
                                            else if ((distX < 0) == (i == 0))
                                                _thisSprite = Grid.Instance.CachedAssets.Wall_0_Horizontal_R;
                                        }
                                        else
                                            _thisSprite = Grid.Instance.CachedAssets.Wall_0_Horizontal_M;
                                    }
                                    else if (distYAbs > distXAbs) {
                                        if (i == 0 || i == highestAxisValue) {
                                            if ((distY > 0) == (i == 0))
                                                _thisSprite = Grid.Instance.CachedAssets.Wall_0_Vertical_B;
                                            else if ((distY < 0) == (i == 0))
                                                _thisSprite = Grid.Instance.CachedAssets.Wall_0_Vertical_T;
                                        }
                                        else
                                            _thisSprite = Grid.Instance.CachedAssets.Wall_0_Vertical_M;
                                    }

                                    allGhostSprites[i].sprite = _thisSprite;
                                    usedGhostTiles.Add(new GhostInfo(allGhostSprites[i], new Vector2(ghostTile_GridX, ghostTile_GridY), mouseGhostType));
                                }
                            }
                            #endregion
                            break;
                        case ModeEnum.Room:
                            #region Room
                            highestAxisValue = Mathf.Clamp(distXAbs * 2 + distYAbs * 2, 1, allGhostSprites.Length);


                            bool _break = false;
                            for (int y = 0; y <= distYAbs; y++) {
                                for (int x = 0; x <= distXAbs; x++) {
                                    if ((y > 0 && y < distYAbs) && (x > 0 && x < distXAbs))
                                        continue;

                                    if (usedGhostTiles.Count >= highestAxisValue) {
                                        _break = true;
                                        break;
                                    }

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
                                    Sprite _thisSprite = Grid.Instance.CachedAssets.Wall_0_Single;
                                   
                                    // 1D horizontal
                                    if (distXAbs > 0 && distYAbs == 0) {
                                        if(x == 0)
                                            _thisSprite = distX > 0 ? Grid.Instance.CachedAssets.Wall_0_Horizontal_L : Grid.Instance.CachedAssets.Wall_0_Horizontal_R;
                                        else if(x == distXAbs)
                                            _thisSprite = distX > 0 ? Grid.Instance.CachedAssets.Wall_0_Horizontal_R : Grid.Instance.CachedAssets.Wall_0_Horizontal_L;
                                        else if(x > 0)
                                            _thisSprite = Grid.Instance.CachedAssets.Wall_0_Horizontal_M;
                                    }
                                    // 1D vertical
                                    else if (distXAbs == 0 && distYAbs > 0) {
                                        if (y == 0)
                                            _thisSprite = distY > 0 ? Grid.Instance.CachedAssets.Wall_0_Vertical_B : Grid.Instance.CachedAssets.Wall_0_Vertical_T;
                                        else if (y == distYAbs)
                                            _thisSprite = distY > 0 ? Grid.Instance.CachedAssets.Wall_0_Vertical_T : Grid.Instance.CachedAssets.Wall_0_Vertical_B;
                                        else if (y > 0)
                                            _thisSprite = Grid.Instance.CachedAssets.Wall_0_Vertical_M;
                                    }
                                    // 2D both
                                    else {
                                        if ((x > 0 && y == 0) || (x > 0 && y == distYAbs))
                                            _thisSprite = Grid.Instance.CachedAssets.Wall_0_Horizontal_M;
                                        else if ((x == 0 && y > 0) || (x == distXAbs && y > 0))
                                            _thisSprite = Grid.Instance.CachedAssets.Wall_0_Vertical_M;

                                        if (distX > 0 && distY > 0) {
                                            if (x == 0 && y == 0)
                                                _thisSprite = Grid.Instance.CachedAssets.Wall_0_Corner_TR;
                                            else if (x == distXAbs && y == 0)
                                                _thisSprite = Grid.Instance.CachedAssets.Wall_0_Corner_LT;
                                            else if (x == distXAbs && y == distYAbs)
                                                _thisSprite = Grid.Instance.CachedAssets.Wall_0_Corner_BL;
                                            else if (x == 0 && y == distYAbs)
                                                _thisSprite = Grid.Instance.CachedAssets.Wall_0_Corner_RB;
                                        }
                                        else if (distX > 0 && distY < 0) {
                                            if (x == 0 && y == 0)
                                                _thisSprite = Grid.Instance.CachedAssets.Wall_0_Corner_RB;
                                            else if (x == distXAbs && y == 0)
                                                _thisSprite = Grid.Instance.CachedAssets.Wall_0_Corner_BL;
                                            else if (x == distXAbs && y == distYAbs)
                                                _thisSprite = Grid.Instance.CachedAssets.Wall_0_Corner_LT;
                                            else if (x == 0 && y == distYAbs)
                                                _thisSprite = Grid.Instance.CachedAssets.Wall_0_Corner_TR;
                                        }
                                        else if (distX < 0 && distY > 0) {
                                            if (x == 0 && y == 0)
                                                _thisSprite = Grid.Instance.CachedAssets.Wall_0_Corner_LT;
                                            else if (x == distXAbs && y == 0)
                                                _thisSprite = Grid.Instance.CachedAssets.Wall_0_Corner_TR;
                                            else if (x == distXAbs && y == distYAbs)
                                                _thisSprite = Grid.Instance.CachedAssets.Wall_0_Corner_RB;
                                            else if (x == 0 && y == distYAbs)
                                                _thisSprite = Grid.Instance.CachedAssets.Wall_0_Corner_BL;
                                        }
                                        else if (distX < 0 && distY < 0) {
                                            if (x == 0 && y == 0)
                                                _thisSprite = Grid.Instance.CachedAssets.Wall_0_Corner_BL;
                                            else if (x == distXAbs && y == 0)
                                                _thisSprite = Grid.Instance.CachedAssets.Wall_0_Corner_RB;
                                            else if (x == distXAbs && y == distYAbs)
                                                _thisSprite = Grid.Instance.CachedAssets.Wall_0_Corner_TR;
                                            else if (x == 0 && y == distYAbs)
                                                _thisSprite = Grid.Instance.CachedAssets.Wall_0_Corner_LT;
                                        }
                                    }

                                    allGhostSprites[usedGhostTiles.Count].sprite = _thisSprite;
                                    usedGhostTiles.Add(new GhostInfo(allGhostSprites[usedGhostTiles.Count], new Vector2(ghostTile_GridX, ghostTile_GridY), mouseGhostType));
                                }

                                if (_break)
                                    break;
                            }
                            #endregion
                            break;
                        case ModeEnum.Diagonal:
                            usedGhostTiles.Add(new GhostInfo(allGhostSprites[0], new Vector2(mouseTile.GridX, mouseTile.GridY), mouseGhostType));
                            break;
                         #region SuperDiagonal
                        //case ModeEnum.SuperDiagonal:
                        //    // determine if we're going to force diagonal ghosting
                        //    highestAxisValue = Mathf.Clamp((Mathf.Max(distXAbs, distYAbs) * 3) + 2, 1, allGhostSprites.Length);
                        //    int _iModulo = 0;
                        //    int _defaultWallsPlacedAmount = 0;
                        //    for (int i = 0; i < allGhostSprites.Length; i++) {
                        //        if (i <= highestAxisValue) {
                        //            _iModulo = i % 3;

                        //            // determine the offset from the _startTile
                        //            if (_iModulo == 0) {
                        //                ghostTile_GridX = distX < 0 ? startTile.GridX - _defaultWallsPlacedAmount : startTile.GridX + _defaultWallsPlacedAmount;
                        //                ghostTile_GridY = distY < 0 ? startTile.GridY - _defaultWallsPlacedAmount : startTile.GridY + _defaultWallsPlacedAmount;
                        //                _defaultWallsPlacedAmount++;
                        //            }
                        //            else if (_iModulo == 1) {
                        //                //ghostTile_GridX = ghostTile_GridX;
                        //                ghostTile_GridY = ghostTile_GridY - 1;
                        //            }
                        //            else if (_iModulo == 2) {
                        //                //ghostTile_GridX = ghostTile_GridX;
                        //                ghostTile_GridY = ghostTile_GridY + 2; // +2 because the previous one was -1
                        //            }

                        //            // if outside grid, continue
                        //            if (ghostTile_GridX < 0 || ghostTile_GridX >= Grid.Instance.GridWorldSize.x)
                        //                continue;
                        //            if (ghostTile_GridY < 0 || ghostTile_GridY >= Grid.Instance.GridWorldSize.y)
                        //                continue;

                        //            // determine what type of tile this is
                        //            Tile.TileType _thisType = Tile.TileType.Default;
                        //            Sprite _thisSprite = null;
                        //            if (_iModulo == 0) {
                        //                _thisType = Tile.TileType.Wall;
                        //                _thisSprite = Grid.Instance.CachedAssets.Wall_0_Single;
                        //            }
                        //            else if (_iModulo == 1) {
                        //                if (distX < 0 && distY < 0 || distX > 0 && distY > 0) {
                        //                    _thisType = Tile.TileType.Diagonal_LT;
                        //                    _thisSprite = Grid.Instance.CachedAssets.Wall_0_Diagonal_LT;
                        //                }
                        //                else if (distX < 0 && distY > 0 || distX > 0 && distY < 0) {
                        //                    _thisType = Tile.TileType.Diagonal_TR;
                        //                    _thisSprite = Grid.Instance.CachedAssets.Wall_0_Diagonal_TR;
                        //                }

                        //                else { // only happens if the cursor hasn't moved since mousebuttondown
                        //                    _thisType = Tile.TileType.Diagonal_TR;
                        //                    _thisSprite = Grid.Instance.CachedAssets.Wall_0_Diagonal_TR;
                        //                }

                        //            }
                        //            else if (_iModulo == 2) {
                        //                if (distX < 0 && distY < 0 || distX > 0 && distY > 0) {
                        //                    _thisType = Tile.TileType.Diagonal_RB;
                        //                    _thisSprite = Grid.Instance.CachedAssets.Wall_0_Diagonal_RB;
                        //                }
                        //                else if (distX < 0 && distY > 0 || distX > 0 && distY < 0) {
                        //                    _thisType = Tile.TileType.Diagonal_BL;
                        //                    _thisSprite = Grid.Instance.CachedAssets.Wall_0_Diagonal_BL;
                        //                }

                        //                else { // only happens if the cursor hasn't moved since mousebuttondown
                        //                    _thisType = Tile.TileType.Diagonal_BL;
                        //                    _thisSprite = Grid.Instance.CachedAssets.Wall_0_Diagonal_BL;
                        //                }
                        //            }

                        //            // done, add it to the list
                        //            allGhostSprites[i].sprite = _thisSprite;
                        //            usedGhostTiles.Add(new GhostInfo(allGhostSprites[i], new Vector2(ghostTile_GridX, ghostTile_GridY), _thisType));
                        //        }
                        //    }
                        //    break;
                        #endregion
                        default:
                            Debug.LogError(Mode.ToString() + " hasn't been implemented!");
                            break;
                    }

                    for (int i = 0; i < usedGhostTiles.Count; i++) {
                        tileUnderGhost = Grid.Instance.grid[(int)usedGhostTiles[i].GridPosition.x, (int)usedGhostTiles[i].GridPosition.y];

                        // color and sparkles!
                        if (tileUnderGhost._Type_ == Tile.TileType.Default) {
                            color = Color.white;

                            if (Mode == ModeEnum.Diagonal) {
                                if (usedGhostTiles[i].Type == Tile.TileType.Diagonal_LT && !(tileUnderGhost.HasConnectable_L && tileUnderGhost.HasConnectable_T))
                                    color = Color.red;
                                else if (usedGhostTiles[i].Type == Tile.TileType.Diagonal_TR && !(tileUnderGhost.HasConnectable_T && tileUnderGhost.HasConnectable_R))
                                    color = Color.red;
                                else if (usedGhostTiles[i].Type == Tile.TileType.Diagonal_RB && !(tileUnderGhost.HasConnectable_R && tileUnderGhost.HasConnectable_B))
                                    color = Color.red;
                                else if (usedGhostTiles[i].Type == Tile.TileType.Diagonal_BL && !(tileUnderGhost.HasConnectable_B && tileUnderGhost.HasConnectable_L))
                                    color = Color.red;
                            }
                        }
                        else {
                            color = Color.red;
                        }

                        // make it do something
                        if (color == Color.white) {
                            selectedTiles.Add(tileUnderGhost);
                            selectedTilesType.Add(usedGhostTiles[i].Type);
                        }

                        // apply stuff
                        color.a = 0.5f;
                        usedGhostTiles[i].Renderer.enabled = true;
                        usedGhostTiles[i].Renderer.color = color;
                        usedGhostTiles[i].Renderer.transform.position = tileUnderGhost.WorldPosition;
                    }
                }

                yield return new WaitForSeconds(0.01f);
            }

            // click released
            if (!Input.GetMouseButton(0) && mouseIsDown) { // replacement for GetMouseUp, which failed due to the yield above
                mouseIsDown = false;

                if (hasUsedGhosts) {
                    for (int i = 1; i < allGhostSprites.Length; i++) // start on 1 to skip the mouseGhost
                        allGhostSprites[i].enabled = false;

                    hasUsedGhosts = false;
                }

                for (int i = 0; i < selectedTiles.Count; i++) {
                    Debug.Log("Set (" + selectedTiles[i].GridX + ", " + selectedTiles[i].GridY + ") to " + selectedTilesType[i].ToString() + " from " + selectedTiles[i]._Type_.ToString());
                    selectedTiles[i].SetTileType(selectedTilesType[i]);
                    Grid.Instance.UpdateTileGraphics(selectedTiles[i], true, true);
                    Grid.Instance.ApplyGraphics(selectedTiles[i].GridSliceIndex);
                }
            }

            yield return null;
        }
    }
}
