using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour {

    public static Grid Instance;

    [SerializeField] private MeshRenderer[] GridGraphicsRenderers;
    private Texture2D[] gridGraphics;
    private List<int> gridSlicesPendingApply = new List<int>();
    public const int TILE_RESOLUTION = 64;
    public const float WORLD_HEIGHT = 1;

    public bool DisplayGridGizmos;
    public bool DisplayPaths;
    public LayerMask UnwalkableMask;
    public Vector2 GridWorldSize;
    public float NodeRadius;

    [HideInInspector]
    public Tile[,] grid; // should make 1D
    private float nodeDiameter;

    public int MaxSize { get { return gridSizeX * gridSizeY; } }
    private int gridSizeX;
    private int gridSizeY;
    private int sliceSizeX;
    private int sliceSizeY;


    void Awake() {
        Instance = this;

        gridGraphics = new Texture2D[GridGraphicsRenderers.Length];

        nodeDiameter = NodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(GridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(GridWorldSize.y / nodeDiameter);

        StartCoroutine(_ApplyPendingTextures());
        CreateGrid();
    }

    //void Update() {
    //    if (Input.GetKeyDown(KeyCode.K)) // for testing performance
    //        gridGraphics.Apply();
    //}

    void CreateGrid() {
        grid = new Tile[gridSizeX, gridSizeY];

        sliceSizeX = (int)(gridSizeX / (Mathf.Sqrt(gridGraphics.Length)));
        sliceSizeY = (int)(gridSizeY / (Mathf.Sqrt(gridGraphics.Length)));
        float _slicePosX;
        float _slicePosY;

        int _currentIndex = 0;
        int _sqrtOfGridGraphics = Mathf.RoundToInt(Mathf.Sqrt(gridGraphics.Length));
        for (int y = 0; y < _sqrtOfGridGraphics; y++) {
            _slicePosY = ((sliceSizeY * 0.5f) + (sliceSizeY * y) - (gridSizeY * 0.5f)) + 0.5f; // +0.5f for diagonals

            for (int x = 0; x < _sqrtOfGridGraphics; x++) {
                _currentIndex = (y * _sqrtOfGridGraphics) + x;

                gridGraphics[_currentIndex] = new Texture2D(sliceSizeX * TILE_RESOLUTION, (sliceSizeY + 1 /*+1 for diagonals*/) * TILE_RESOLUTION, TextureFormat.RGBA32, true);
                gridGraphics[_currentIndex].filterMode = FilterMode.Point;
                gridGraphics[_currentIndex].wrapMode = TextureWrapMode.Clamp;
                GridGraphicsRenderers[_currentIndex].material.mainTexture = gridGraphics[_currentIndex];

                _slicePosX = ((sliceSizeX * 0.5f) + (sliceSizeX * x) - (gridSizeX * 0.5f));

                GridGraphicsRenderers[_currentIndex].transform.localScale = new Vector3(sliceSizeX, sliceSizeY + 1 /*+1 for diagonals*/, 1);
                GridGraphicsRenderers[_currentIndex].transform.position = new Vector3(_slicePosX, _slicePosY, WORLD_HEIGHT + (_currentIndex * 0.01f)); // the height-thing is to combat z-fighting
            }
        }

        Vector3 worldBottomLeft = transform.position - (Vector3.right * GridWorldSize.x / 2) - (Vector3.up * GridWorldSize.y / 2) - new Vector3(0, 0.5f, 0); // 0.5f because of the +1 for diagonals
        for (int y = 0; y < gridSizeY; y++) {
            for (int x = 0; x < gridSizeX; x++) {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + NodeRadius) + Vector3.up * (y * nodeDiameter + NodeRadius + 0.5f); // +0.5f for diagonals

                int movementPenalty = 0; // todo: use this for something
                if (Random.value < 0.75f)
                    grid[x, y] = new Tile(Tile.TileType.Empty, Tile.TileOrientation.None, worldPoint, x, y, x % sliceSizeX, y % sliceSizeY, GetGridSliceIndex(x, y), movementPenalty);
                else // for testing-purposes
                    grid[x, y] = new Tile(Tile.TileType.Wall, Tile.TileOrientation.None, worldPoint, x, y, x % sliceSizeX, y % sliceSizeY, GetGridSliceIndex(x, y), movementPenalty);
            }
        }
        // for testing-purposes
        for (int y = 0; y < gridSizeY; y++) {
            for (int x = 0; x < gridSizeX; x++) {
                if (grid[x, y]._Type_ == Tile.TileType.Wall)
                    continue;
                if (RUL.Rul.RandBool())
                    continue;

                // LT
                if (x > 0 && grid[x - 1, y]._Type_ == Tile.TileType.Wall && y < gridSizeY - 1 && grid[x, y + 1]._Type_ == Tile.TileType.Wall) {
                    grid[x, y].SetTileType(Tile.TileType.Diagonal, Tile.TileOrientation.TopLeft);
                    continue;
                }
                // TR
                if (y < gridSizeY - 1 && grid[x, y + 1]._Type_ == Tile.TileType.Wall && x < gridSizeX - 1 && grid[x + 1, y]._Type_ == Tile.TileType.Wall) {
                    grid[x, y].SetTileType(Tile.TileType.Diagonal, Tile.TileOrientation.TopRight);
                    continue;
                }
                // RB
                if (x < gridSizeX - 1 && grid[x + 1, y]._Type_ == Tile.TileType.Wall && y > 0 && grid[x, y - 1]._Type_ == Tile.TileType.Wall) {
                    grid[x, y].SetTileType(Tile.TileType.Diagonal, Tile.TileOrientation.BottomRight);
                    continue;
                }
                // BL
                if (y > 0 && grid[x, y - 1]._Type_ == Tile.TileType.Wall && x > 0 && grid[x - 1, y]._Type_ == Tile.TileType.Wall) {
                    grid[x, y].SetTileType(Tile.TileType.Diagonal, Tile.TileOrientation.BottomLeft);
                    continue;
                }
            }
        }

        for (int y = gridSizeY - 1; y >= 0; y--) {
            for (int x = gridSizeX - 1; x >= 0; x--) { // loop backwards so we draw towards, not away from the perspective of the camera
                UpdateTile(grid[x, y], _individualUpdate: false, _updateNeighbours: false);
            }
        }

        for (int i = 0; i < gridGraphics.Length; i++) {
            ApplyGraphics(i);
        }
    }

    int GetGridSliceIndex(int _gridX, int _gridY) {
        int _sliceAmountX = Mathf.FloorToInt(_gridX / sliceSizeX);
        int _sliceAmountY = Mathf.FloorToInt(_gridY / sliceSizeY);

        return (_sliceAmountY * (gridSizeX / sliceSizeX))  + _sliceAmountX;
    }

    public void UpdateTile(Tile _tile, bool _individualUpdate, bool _updateNeighbours) {
        List<Tile> _neighbours = GetNeighbours(_tile.GridX, _tile.GridY);
        List<Tile> _neighboursToUpdate = new List<Tile>();
        List<int> _affectedGridSlices = new List<int>();
        int _x = _tile.GridX;
        int _y = _tile.GridY;
        int _xDiff = 0;
        int _yDiff = 0;

        #region Get neighbour-connections
        for (int i = 0; i < _neighbours.Count; i++) {

            _xDiff = _neighbours[i].GridX - _x;
            _yDiff = _neighbours[i].GridY - _y;

            // determine what kinds of neighbours we got here
            if (_yDiff == -1) {
                if (_xDiff == 0) {
                    _tile.HasConnectable_B = _neighbours[i].CanConnect_T;
                    _tile.IsBlocked_B = TileIsBlockingOtherTile(_neighbours[i]._Type_, _neighbours[i]._Orientation_, Tile.TileOrientation.Bottom);

                    // must update if the tile below is diagonal (since they stretch up)
                    if (_updateNeighbours || _neighbours[i]._Type_ == Tile.TileType.Diagonal && (_neighbours[i]._Orientation_ == Tile.TileOrientation.TopLeft || _neighbours[i]._Orientation_ == Tile.TileOrientation .TopRight)) {
                        _neighbours[i].HasConnectable_T = _tile.CanConnect_B;
                        _neighbours[i].IsBlocked_T = TileIsBlockingOtherTile(_tile._Type_, _tile._Orientation_, Tile.TileOrientation.Top);

                        _neighboursToUpdate.Add(_neighbours[i]);
                    }
                }
            }
            else if (_yDiff == 0) {
                if (_xDiff == -1) {
                    _tile.HasConnectable_L = _neighbours[i].CanConnect_R;
                    _tile.IsBlocked_L = TileIsBlockingOtherTile(_neighbours[i]._Type_, _neighbours[i]._Orientation_, Tile.TileOrientation.Left);

                    if (_updateNeighbours) {
                        _neighbours[i].HasConnectable_R = _tile.CanConnect_L;
                        _neighbours[i].IsBlocked_R = TileIsBlockingOtherTile(_tile._Type_, _tile._Orientation_, Tile.TileOrientation.Right);

                        _neighboursToUpdate.Add(_neighbours[i]);
                    }
                }
                else if (_xDiff == 1) {
                    _tile.HasConnectable_R = _neighbours[i].CanConnect_L;
                    _tile.IsBlocked_R = TileIsBlockingOtherTile(_neighbours[i]._Type_, _neighbours[i]._Orientation_, Tile.TileOrientation.Right);

                    if (_updateNeighbours) {
                        _neighbours[i].HasConnectable_L = _tile.CanConnect_R;
                        _neighbours[i].IsBlocked_L = TileIsBlockingOtherTile(_tile._Type_, _tile._Orientation_, Tile.TileOrientation.Left);

                        _neighboursToUpdate.Add(_neighbours[i]);
                    }
                }
            }
            else if (_yDiff == 1) {
                if (_xDiff == 0) {
                    _tile.HasConnectable_T = _neighbours[i].CanConnect_B;
                    _tile.IsBlocked_T = TileIsBlockingOtherTile(_neighbours[i]._Type_, _neighbours[i]._Orientation_, Tile.TileOrientation.Top);

                    if (_updateNeighbours) {
                        _neighbours[i].HasConnectable_B = _tile.CanConnect_T;
                        _neighbours[i].IsBlocked_B = TileIsBlockingOtherTile(_tile._Type_, _tile._Orientation_, Tile.TileOrientation.Bottom);

                        _neighboursToUpdate.Add(_neighbours[i]);
                    }
                }
            }
        }
        #endregion

        // find and set correct sprite
        DrawTileToTexture(_tile, CachedAssets.Instance.GetAssetForTile(_tile, 0, _isOnGroundLevel: true));

        // loop through relevant neighbours backwards, towards the perspective of the camera
        for (int i = _neighboursToUpdate.Count - 1; i >= 0; i--) {
            if (_neighboursToUpdate[i]._Type_ == Tile.TileType.Diagonal && (_neighboursToUpdate[i]._Orientation_ == Tile.TileOrientation.TopLeft || _neighboursToUpdate[i]._Orientation_ == Tile.TileOrientation.TopRight))
                continue; // skip diagonals and do them later

            UpdateTile(_neighboursToUpdate[i], _individualUpdate: false, _updateNeighbours: false);
            _neighboursToUpdate.RemoveAt(i);
        }
        for (int i = _neighboursToUpdate.Count - 1; i >= 0; i--) {
            UpdateTile(_neighboursToUpdate[i], _individualUpdate: false, _updateNeighbours: false);
        }

        ApplyGraphics(_tile.GridSliceIndex);
    }

    bool TileIsBlockingOtherTile(Tile.TileType _otherTileType, Tile.TileOrientation _otherTileOrientation, Tile.TileOrientation _otherTileDirection) {
        if (_otherTileType == Tile.TileType.Empty || _otherTileType == Tile.TileType.DoorEntrance)
            return false;
        if (_otherTileType == Tile.TileType.Wall || _otherTileType == Tile.TileType.Door)
            return true;

        if (_otherTileType != Tile.TileType.Diagonal)
            Debug.LogError("Wasn't expecting non-diagonals to be here! Handle it!");

        switch (_otherTileDirection) {
            case Tile.TileOrientation.Bottom:
                return _otherTileOrientation == Tile.TileOrientation.TopLeft || _otherTileOrientation == Tile.TileOrientation.TopRight;
            case Tile.TileOrientation.Left:
                return _otherTileOrientation == Tile.TileOrientation.TopRight || _otherTileOrientation == Tile.TileOrientation.BottomRight;
            case Tile.TileOrientation.Top:
                return _otherTileOrientation == Tile.TileOrientation.BottomRight || _otherTileOrientation == Tile.TileOrientation.BottomLeft;
            case Tile.TileOrientation.Right:
                return _otherTileOrientation == Tile.TileOrientation.BottomLeft || _otherTileOrientation == Tile.TileOrientation.TopLeft;
        }

        return false;
    }

    public void ApplyGraphics(int _sliceIndex) {
        if (gridSlicesPendingApply.Contains(_sliceIndex))
            return;

        gridSlicesPendingApply.Add(_sliceIndex);
    }
    IEnumerator _ApplyPendingTextures() {
        while (true) {
            yield return new WaitForEndOfFrame();
            for (int i = 0; i < gridSlicesPendingApply.Count; i++) {
                gridGraphics[gridSlicesPendingApply[i]].Apply();
                gridSlicesPendingApply.RemoveAt(i);
                i--;
            }
        }
    }

    void DrawTileToTexture(Tile _tile, Sprite _sprite) {
        Color[] _pixels = null;
        int _spriteWidth = 64;
        int _spriteHeight = 64;
        Vector2 _tilePosOnTexture = new Vector2(_tile.LocalGridX * TILE_RESOLUTION, _tile.LocalGridY * TILE_RESOLUTION);

        if (_sprite != null) {
            _pixels = CachedAssets.Instance.GetCachedAssetPixels(_sprite);
            _spriteWidth = (int)_sprite.rect.width;
            _spriteHeight = (int)_sprite.rect.height;
        }
        
        else {
            _pixels = new Color[TILE_RESOLUTION * TILE_RESOLUTION];
            Color _c = new Color(0, 0, 0, 0);
            for (int i = 0; i < _pixels.Length; i++)
                _pixels[i] = _c;
        }

        // if just below the top line of your slice and you're not drawing up there anyway, draw empty space
        // note: the top line of slices is not included in sliceSize. Also, since it's size and not an index, "sliceSizeY - 1" means the top counted Y, so below the extra line
        if (_tile.LocalGridY == sliceSizeY - 1 && _spriteHeight <= TILE_RESOLUTION) {
            List<Color> _newPixels = new List<Color>(_pixels);
            Color _c = new Color(0, 0, 0, 0);
            for (int i = 0, resSqrd = (int)Mathf.Pow(TILE_RESOLUTION, 2); i < resSqrd; i++) {
                _newPixels.Add(_c);
            }
            
            _pixels = _newPixels.ToArray();
            _spriteHeight = TILE_RESOLUTION * 2;
        }

        // if bigger than normal (diagonals), find any empty pixels and replace them with pixels from the old texture
        else if (_tile.LocalGridY < sliceSizeY - 1 && _spriteHeight > TILE_RESOLUTION) {

            int _index = 0;
            for (int y = TILE_RESOLUTION; y < _spriteHeight; y++) {
                for (int x = 0; x < _spriteWidth; x++) {
                    _index = (y * _spriteWidth) + x;
                    if (_pixels[_index].a > 0)
                        continue;

                    _pixels[_index] = gridGraphics[_tile.GridSliceIndex].GetPixel((int)_tilePosOnTexture.x + x, (int)_tilePosOnTexture.y + y);
                }
            }

        }

        gridGraphics[_tile.GridSliceIndex].SetPixels((int)_tilePosOnTexture.x, (int)_tilePosOnTexture.y, _spriteWidth, _spriteHeight, _pixels, 0);
    }

    public List<Tile> GetNeighbours(int _gridX, int _gridY) {
        List<Tile> neighbours = new List<Tile>();
        for (int y = -1; y <= 1; y++) {
            for (int x = -1; x <= 1; x++) {
                if (x == 0 && y == 0)
                    continue;

                int checkX = _gridX + x;
                int checkY = _gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    public Tile GetTileFromWorldPoint(Vector3 _worldPosition) {
        float percentX = (_worldPosition.x - NodeRadius + GridWorldSize.x * 0.5f) / GridWorldSize.x;
        float percentY = (_worldPosition.y - NodeRadius + GridWorldSize.y * 0.5f) / GridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt(gridSizeX * percentX);
        int y = Mathf.RoundToInt(gridSizeY * percentY);
        x = (int)Mathf.Clamp(x, 0, GridWorldSize.x - 1);
        y = (int)Mathf.Clamp(y, 0, GridWorldSize.y - 1);

        return grid[x, y];
    }

    public Tile GetClosestFreeNode(Vector3 _worldPosition) {
        Tile _tile = GetTileFromWorldPoint(_worldPosition);
        if (_tile.Walkable && !_tile.IsOccupied)
            return _tile;

        List<Tile> _neighbours = GetNeighbours(_tile.GridX, _tile.GridY);
        int _lastCount = 0;

        while (_neighbours.Count < (gridSizeX * gridSizeY)) {

            // iterate over _neighbours until a free node is found
            for (int i = _lastCount; i < _neighbours.Count; i++) {
                if (!_neighbours[i].Walkable || _neighbours[i].IsOccupied)
                    continue;

                return _neighbours[i];
            }

            int _prevLastCount = _lastCount;
            _lastCount = _neighbours.Count; // save progress before we add new neighbours, so we don't iterate over old stuff later

            // iterate over _neighbours - if their neighbours aren't in _neighbours, add them.
            List<Tile> _newNeighbours = new List<Tile>();
            for (int i = _prevLastCount; i < _lastCount; i++) {
                _newNeighbours = GetNeighbours(_neighbours[i].GridX, _neighbours[i].GridY);
                for (int j = 0; j < _newNeighbours.Count; j++) {
                    if (_neighbours.Contains(_newNeighbours[j]))
                        continue;

                    _neighbours.Add(_newNeighbours[j]);
                }
            }
        }
        return null;
    }
    public Tile GetClosestFreeNode(Tile _tile) { // todo: diagonals can be seen as "free" depending on the usage - fix that! Removed diagonals from consideration for now.
        if (_tile._Type_ == Tile.TileType.Empty && !_tile.IsOccupied)
            return _tile;

        List<Tile> _neighbours = GetNeighbours(_tile.GridX, _tile.GridY);
        int _lastCount = 0;

        while (_neighbours.Count < (gridSizeX * gridSizeY)) {

            // iterate over _neighbours until a free node is found
            for (int i = _lastCount; i < _neighbours.Count; i++) {
                if (_neighbours[i]._Type_ != Tile.TileType.Empty || _neighbours[i].IsOccupied)
                    continue;

                return _neighbours[i];
            }

            int _prevLastCount = _lastCount;
            _lastCount = _neighbours.Count; // save progress before we add new neighbours, so we don't iterate over old stuff later

            // iterate over _neighbours - if their neighbours aren't in _neighbours, add them.
            List<Tile> _newNeighbours = new List<Tile>();
            for (int i = _prevLastCount; i < _lastCount; i++) {
                _newNeighbours = GetNeighbours(_neighbours[i].GridX, _neighbours[i].GridY);
                for (int j = 0; j < _newNeighbours.Count; j++) {
                    if (_neighbours.Contains(_newNeighbours[j]))
                        continue;

                    _neighbours.Add(_newNeighbours[j]);
                }
            }
        }
        return null;
    }

    public Tile GetRandomWalkableNode(Tile _exclude = null) {
        Tile node = null;
        int x = 0;
        int y = 0;

        do {
            x = (int)Random.Range(0, GridWorldSize.x);
            y = (int)Random.Range(0, GridWorldSize.y);

            // added this due to the path being randomly empty once. doesn't appear to fix anything, but keeping it for now.
            //if (_exclude != null && x == _exclude.gridX && y == _exclude.gridY) 
            //    continue;

            node = grid[x, y];
        } while (!node.Walkable);

        return node;
    }
    
    // checks if two normal walls are diagonally aligned, because you shouldn't be able to walk though there
    public bool IsNeighbourBlockedDiagonally(Tile _tile, Tile _neighbour) {
        int _xDiff = _neighbour.GridX - _tile.GridX;
        int _yDiff = _neighbour.GridY - _tile.GridY;
        Tile _nonDiagonalNeighbour1 = null;
        Tile _nonDiagonalNeighbour2 = null;

        // bottom left
        if (_xDiff == -1 && _yDiff == -1) {
            _nonDiagonalNeighbour1 = grid[_tile.GridX, _tile.GridY - 1];
            _nonDiagonalNeighbour2 = grid[_tile.GridX - 1, _tile.GridY];
        }
        // bottom right
        else if (_xDiff == 1 && _yDiff == -1) {
            _nonDiagonalNeighbour1 = grid[_tile.GridX, _tile.GridY - 1];
            _nonDiagonalNeighbour2 = grid[_tile.GridX + 1, _tile.GridY];
        }
        // top left
        else if (_xDiff == -1 && _yDiff == 1) {
            _nonDiagonalNeighbour1 = grid[_tile.GridX, _tile.GridY + 1];
            _nonDiagonalNeighbour2 = grid[_tile.GridX - 1, _tile.GridY];
        }
        // top right
        else if (_xDiff == 1 && _yDiff == 1) {
            _nonDiagonalNeighbour1 = grid[_tile.GridX, _tile.GridY + 1];
            _nonDiagonalNeighbour2 = grid[_tile.GridX + 1, _tile.GridY];
        }
        // not even diagonal
        else {
            return false;
        }

        if (!_nonDiagonalNeighbour1.Walkable || !_nonDiagonalNeighbour2.Walkable)
            return true;

        return false;
    }

    //// checks if the path between two tiles is blocked because either one is a diagonal tile and the rotation of said tile is such as that it blocks the path.
    //public bool IsPathToNeighbourBlockedByTileRotation(Tile _tile, Tile _neighbour) {
    //    int _xDiff = _neighbour.GridX - _tile.GridX;
    //    int _yDiff = _neighbour.GridY - _tile.GridY;

    //    // top or below
    //    if (_xDiff == 0 && _yDiff != 0)
    //        return _tile.CanConnect_T || _neighbour.CanConnect_B;
        
    //    // left or right
    //    else if (_xDiff != 0 && _yDiff == 0)
    //        return _tile.CanConnect_L || _tile.CanConnect_R;
        
    //    // top left or bottom right
    //    else if (_xDiff != 0 && _yDiff == _xDiff * -1)
    //        return (_tile.CanConnect_L && _tile.CanConnect_T) || (_neighbour.CanConnect_B && _tile.CanConnect_R);

    //    // top right or bottom left
    //    else if (_xDiff != 0 && _yDiff == _xDiff)
    //        return (_tile.CanConnect_T && _tile.CanConnect_R) || (_neighbour.CanConnect_B && _tile.CanConnect_L);


    //    return false;
    //}


    void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position, new Vector3(GridWorldSize.x, GridWorldSize.y, 1));

        if (grid != null && DisplayGridGizmos) {
            foreach (Tile n in grid) {
                Gizmos.color = n.Walkable ? Color.white : Color.red;
                Gizmos.DrawWireCube(n.WorldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}
