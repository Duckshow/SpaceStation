using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour {

    public static Grid Instance;
    public GridAnimator Animator;

    private class GridGraphics {
        public bool IsDirty = false;

        public Texture2D Diffuse;
        public Texture2D Normal;
        public Texture2D Emissive;
        public Texture2D Specular;

        public GridGraphics(int _width, int _height, TextureFormat _format, bool _mipmap) {
            ApplySettingsToAsset(out Diffuse, _width, _height, _format, _mipmap);
            ApplySettingsToAsset(out Normal, _width, _height, _format, _mipmap);
            ApplySettingsToAsset(out Emissive, _width, _height, _format, _mipmap);
            ApplySettingsToAsset(out Specular, _width, _height, _format, _mipmap);
        }

        void ApplySettingsToAsset(out Texture2D _texture, int _width, int _height, TextureFormat _format, bool _mipmap) {
            _texture = new Texture2D(_width, _height, _format, _mipmap);
            _texture.filterMode = FilterMode.Point;
            _texture.wrapMode = TextureWrapMode.Clamp;
            _texture.anisoLevel = 0;
        }

        public void ApplyAll() {
            if (!IsDirty)
                return;
            IsDirty = false;

            Diffuse.Apply();
            Normal.Apply();
            Emissive.Apply();
            Specular.Apply();
        }
    }

    [SerializeField] private Texture2D TextureWithGoodImportSettings;
    [SerializeField] private Texture2D NormalMapWithGoodImportSettings;

    [SerializeField] private MeshRenderer[] GridGraphicsBottomRenderers;
    [SerializeField] private MeshRenderer[] GridGraphicsTopRenderers;

    private GridGraphics[] gridGraphicsBottom;
    private GridGraphics[] gridGraphicsTop;
    public const int TILE_RESOLUTION = 64;
    public const float WORLD_BOTTOM_HEIGHT = 0.01f;
    public const float WORLD_TOP_HEIGHT = -0.01f;

    public bool DisplayGridGizmos;
    public bool DisplayPaths;
    public bool DisplayWaypoints;
    public Vector2 GridWorldSize;
    public float NodeRadius;

    [HideInInspector]
    public Tile[,] grid; // should make 1D
    private float nodeDiameter;

    public int MaxSize { get { return GridSizeX * GridSizeY; } }
    public int GridSizeX { get; private set; }
    public int GridSizeY { get; private set; }
    private int sliceSizeX;
    private int sliceSizeY;


    void Awake() {
        Instance = this;
        Animator = GetComponent<GridAnimator>();

        gridGraphicsBottom = new GridGraphics[GridGraphicsBottomRenderers.Length];
        gridGraphicsTop = new GridGraphics[GridGraphicsBottomRenderers.Length];

        nodeDiameter = NodeRadius * 2;
        GridSizeX = Mathf.RoundToInt(GridWorldSize.x / nodeDiameter);
        GridSizeY = Mathf.RoundToInt(GridWorldSize.y / nodeDiameter);

        StartCoroutine(_ApplyPendingTextures());
        StartCoroutine(_ApplyPendingTexturesLowPrio());
        CreateGrid();
    }

    //void Update() {
    //    if (Input.GetKeyDown(KeyCode.K)) // for testing performance
    //        gridGraphics.Apply();
    //}

    void CreateGrid() {
        grid = new Tile[GridSizeX, GridSizeY];

        sliceSizeX = (int)(GridSizeX / (Mathf.Sqrt(gridGraphicsBottom.Length)));
        sliceSizeY = (int)(GridSizeY / (Mathf.Sqrt(gridGraphicsBottom.Length)));
        float _slicePosX;
        float _slicePosY;

        int _currentIndex = 0;
        int _sqrtOfGridGraphics = Mathf.RoundToInt(Mathf.Sqrt(gridGraphicsBottom.Length));
        for (int y = 0; y < _sqrtOfGridGraphics; y++) {
            _slicePosY = ((sliceSizeY * 0.5f) + (sliceSizeY * y) - (GridSizeY * 0.5f)) + 0.5f; // +0.5f for diagonals

            for (int x = 0; x < _sqrtOfGridGraphics; x++) {
                _currentIndex = (y * _sqrtOfGridGraphics) + x;

                // bottom
                gridGraphicsBottom[_currentIndex] = new GridGraphics(sliceSizeX * TILE_RESOLUTION, (sliceSizeY + 1 /*+1 for diagonals*/) * TILE_RESOLUTION, TextureFormat.RGBA32, true);
                GridGraphicsBottomRenderers[_currentIndex].material.mainTexture = gridGraphicsBottom[_currentIndex].Diffuse;
                GridGraphicsBottomRenderers[_currentIndex].material.SetTexture("_BumpMap", gridGraphicsBottom[_currentIndex].Normal);
                GridGraphicsBottomRenderers[_currentIndex].material.SetTexture("_EmissionMap", gridGraphicsBottom[_currentIndex].Emissive);
                GridGraphicsBottomRenderers[_currentIndex].material.SetTexture("_SpecGlossMap", gridGraphicsBottom[_currentIndex].Specular);

                // top
                gridGraphicsTop[_currentIndex] = new GridGraphics(sliceSizeX * TILE_RESOLUTION, (sliceSizeY + 1 /*+1 for diagonals*/) * TILE_RESOLUTION, TextureFormat.RGBA32, true);
                GridGraphicsTopRenderers[_currentIndex].material.mainTexture = gridGraphicsTop[_currentIndex].Diffuse;
                GridGraphicsTopRenderers[_currentIndex].material.SetTexture("_BumpMap", gridGraphicsTop[_currentIndex].Normal);
                GridGraphicsTopRenderers[_currentIndex].material.SetTexture("_EmissionMap", gridGraphicsTop[_currentIndex].Emissive);
                GridGraphicsTopRenderers[_currentIndex].material.SetTexture("_SpecGlossMap", gridGraphicsTop[_currentIndex].Specular);

                _slicePosX = ((sliceSizeX * 0.5f) + (sliceSizeX * x) - (GridSizeX * 0.5f));

                // bottom
                GridGraphicsBottomRenderers[_currentIndex].transform.localScale = new Vector3(sliceSizeX, sliceSizeY + 1 /*+1 for diagonals*/, 1);
                GridGraphicsBottomRenderers[_currentIndex].transform.position = new Vector3(_slicePosX, _slicePosY, (y + 1) * WORLD_BOTTOM_HEIGHT); // the height-thing is to combat z-fighting

                GridGraphicsTopRenderers[_currentIndex].transform.localScale = new Vector3(sliceSizeX, sliceSizeY + 1 /*+1 for diagonals*/, 1);
                GridGraphicsTopRenderers[_currentIndex].transform.position = new Vector3(_slicePosX, _slicePosY, (y + 1) * WORLD_TOP_HEIGHT); // the height-thing is to combat z-fighting
            }
        }

        Vector3 worldBottomLeft = transform.position - (Vector3.right * GridWorldSize.x / 2) - (Vector3.up * GridWorldSize.y / 2) - new Vector3(0, 0.5f, 0); // 0.5f because of the +1 for diagonals
        for (int y = 0; y < GridSizeY; y++) {
            for (int x = 0; x < GridSizeX; x++) {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + NodeRadius) + Vector3.up * (y * nodeDiameter + NodeRadius + 0.5f); // +0.5f for diagonals

                int movementPenalty = 0; // todo: use this for something
                if (Random.value < 0.75f)
                    grid[x, y] = new Tile(Tile.TileType.Empty, Tile.TileOrientation.None, worldPoint, x, y, x % sliceSizeX, y % sliceSizeY, GetGridSliceIndex(x, y), movementPenalty);
                else // for testing-purposes
                    grid[x, y] = new Tile(Tile.TileType.Wall, Tile.TileOrientation.None, worldPoint, x, y, x % sliceSizeX, y % sliceSizeY, GetGridSliceIndex(x, y), movementPenalty);
            }
        }
        // for testing-purposes
        for (int y = 0; y < GridSizeY; y++) {
            for (int x = 0; x < GridSizeX; x++) {
                if (grid[x, y]._Type_ == Tile.TileType.Wall)
                    continue;
                if (RUL.Rul.RandBool())
                    continue;

                // LT
                if (x > 0 && grid[x - 1, y]._Type_ == Tile.TileType.Wall && y < GridSizeY - 1 && grid[x, y + 1]._Type_ == Tile.TileType.Wall) {
                    grid[x, y].SetTileType(Tile.TileType.Diagonal, Tile.TileOrientation.TopLeft);
                    continue;
                }
                // TR
                if (y < GridSizeY - 1 && grid[x, y + 1]._Type_ == Tile.TileType.Wall && x < GridSizeX - 1 && grid[x + 1, y]._Type_ == Tile.TileType.Wall) {
                    grid[x, y].SetTileType(Tile.TileType.Diagonal, Tile.TileOrientation.TopRight);
                    continue;
                }
                // RB
                if (x < GridSizeX - 1 && grid[x + 1, y]._Type_ == Tile.TileType.Wall && y > 0 && grid[x, y - 1]._Type_ == Tile.TileType.Wall) {
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

        for (int y = GridSizeY - 1; y >= 0; y--) {
            for (int x = GridSizeX - 1; x >= 0; x--) { // loop backwards so we draw towards, not away from, the perspective of the camera
                UpdateTile(grid[x, y], _updateNeighbours: false, _forceUpdate: true);
            }
        }

        for (int i = 0; i < gridGraphicsBottom.Length; i++) { // top and bottom should have the same length, so this applies to both
            ApplyGraphics(i);
        }
    }

    int GetGridSliceIndex(int _gridX, int _gridY) {
        int _sliceAmountX = Mathf.FloorToInt(_gridX / sliceSizeX);
        int _sliceAmountY = Mathf.FloorToInt(_gridY / sliceSizeY);

        return (_sliceAmountY * (GridSizeX / sliceSizeX))  + _sliceAmountX;
    }

    public void UpdateTile(Tile _tile, bool _updateNeighbours, bool _forceUpdate) {

        // betting on this being safe enough for an early-out, to save some processing
        if (!_forceUpdate && _tile._Type_ != Tile.TileType.Diagonal && _tile._Type_ == _tile._PrevType_)
            return;

        List<Tile> _neighbours = GetNeighbours(_tile.GridX, _tile.GridY);
        List<Tile> _neighboursToUpdate = new List<Tile>();
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


        // get bottom asset, or null, and draw
        CachedAssets.ShadedAsset _asset = CachedAssets.Instance.GetAssetForTile(_tile._Type_, _tile._Orientation_, 0, true, _tile.HasConnectable_L, _tile.HasConnectable_T, _tile.HasConnectable_R, _tile.HasConnectable_B);
        DrawTileToTexture(_tile, _asset, ref gridGraphicsBottom[_tile.GridSliceIndex]);

        // get top asset, or null, and draw
        _asset = CachedAssets.Instance.GetAssetForTile(_tile._Type_, _tile._Orientation_, 0, false, _tile.HasConnectable_L, _tile.HasConnectable_T, _tile.HasConnectable_R, _tile.HasConnectable_B);
        DrawTileToTexture(_tile, _asset, ref gridGraphicsTop[_tile.GridSliceIndex]);


        // loop through relevant neighbours backwards, towards the perspective of the camera
        for (int i = _neighboursToUpdate.Count - 1; i >= 0; i--) {
            if (_neighboursToUpdate[i]._Type_ == Tile.TileType.Diagonal && (_neighboursToUpdate[i]._Orientation_ == Tile.TileOrientation.TopLeft || _neighboursToUpdate[i]._Orientation_ == Tile.TileOrientation.TopRight))
                continue; // skip diagonals and do them later

            UpdateTile(_neighboursToUpdate[i], _updateNeighbours: false, _forceUpdate: false);
            _neighboursToUpdate.RemoveAt(i);
        }
        for (int i = _neighboursToUpdate.Count - 1; i >= 0; i--) {
            UpdateTile(_neighboursToUpdate[i], _updateNeighbours: false, _forceUpdate: false);
        }

        ApplyGraphics(_tile.GridSliceIndex);
    }

    bool TileIsBlockingOtherTile(Tile.TileType _otherTileType, Tile.TileOrientation _otherTileOrientation, Tile.TileOrientation _directionToOtherTile) {
        if (_otherTileType == Tile.TileType.Empty)
            return false;
        if (_otherTileType == Tile.TileType.Wall || _otherTileType == Tile.TileType.Door)
            return true;

        if (_otherTileType != Tile.TileType.Diagonal)
            Debug.LogError("Wasn't expecting non-diagonals to be here! Handle it!");

        switch (_directionToOtherTile) {
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
        if (!gridGraphicsBottom[_sliceIndex].IsDirty && !gridGraphicsTop[_sliceIndex].IsDirty)
            return;
        if (gridSlicesPendingApply.Contains(_sliceIndex))
            return;

        gridSlicesPendingApply.Add(_sliceIndex);
    }
    private List<int> gridSlicesPendingApply = new List<int>();
    IEnumerator _ApplyPendingTextures() {
        while (true) {
            yield return new WaitForEndOfFrame();
            for (int i = 0; i < gridSlicesPendingApply.Count; i++) {
                gridGraphicsBottom[gridSlicesPendingApply[i]].ApplyAll();
                gridGraphicsTop[gridSlicesPendingApply[i]].ApplyAll();

                gridSlicesPendingApply.RemoveAt(i);
                i--;
            }
        }
    }
    public void ApplyGraphicsLowPrio(int _sliceIndex) {
        if (!gridGraphicsBottom[_sliceIndex].IsDirty && !gridGraphicsTop[_sliceIndex].IsDirty)
            return;
        if (gridSlicesPendingApplyLowPrio.Contains(_sliceIndex))
            return;

        gridSlicesPendingApplyLowPrio.Add(_sliceIndex);
    }
    private List<int> gridSlicesPendingApplyLowPrio = new List<int>();
    IEnumerator _ApplyPendingTexturesLowPrio() {
        while (true) {
            for (int i = 0; i < gridSlicesPendingApplyLowPrio.Count; i++) {
                yield return new WaitForEndOfFrame();


                gridGraphicsBottom[gridSlicesPendingApplyLowPrio[i]].ApplyAll();
                gridGraphicsTop[gridSlicesPendingApplyLowPrio[i]].ApplyAll();

                gridSlicesPendingApplyLowPrio.RemoveAt(i);
                i--;
            }

            yield return null;
        }
    }

    public void ChangeSingleTileGraphics(Tile _tile, CachedAssets.ShadedAsset _bottomAsset, CachedAssets.ShadedAsset _topAsset) {
        if (_bottomAsset != null)
            DrawTileToTexture(_tile, _bottomAsset, ref gridGraphicsBottom[_tile.GridSliceIndex]);
        if(_topAsset != null)
            DrawTileToTexture(_tile, _topAsset, ref gridGraphicsTop[_tile.GridSliceIndex]);

        ApplyGraphicsLowPrio(_tile.GridSliceIndex);

        // update attached diagonal
        if (_tile.ConnectedDiagonal_B != null)
            UpdateTile(_tile.ConnectedDiagonal_B, false, true);
    }

    void DrawTileToTexture(Tile _tile, CachedAssets.ShadedAsset _asset, ref GridGraphics _gridGraphics) {
        Color[] _pixelsDiffuse = null;
        Color[] _pixelsNormal = null;
        Color[] _pixelsEmissive = null;
        Color[] _pixelsSpecular = null;

        int _spriteWidth = TILE_RESOLUTION;
        int _spriteHeight = TILE_RESOLUTION;
        Vector2 _tilePosOnTexture = new Vector2(_tile.LocalGridX * TILE_RESOLUTION, _tile.LocalGridY * TILE_RESOLUTION);

        if (_asset != null) {
            _pixelsDiffuse = CachedAssets.Instance.GetCachedAssetPixels(_asset.Diffuse);
            _pixelsNormal = CachedAssets.Instance.GetCachedAssetPixels(_asset.Normal);
            _pixelsEmissive = CachedAssets.Instance.GetCachedAssetPixels(_asset.Emissive);
            _pixelsSpecular = CachedAssets.Instance.GetCachedAssetPixels(_asset.Specular);

            _spriteWidth = (int)_asset.Diffuse.rect.width;
            _spriteHeight = (int)_asset.Diffuse.rect.height;
        }
        
        else {
            _pixelsDiffuse = new Color[TILE_RESOLUTION * TILE_RESOLUTION];
            _pixelsNormal = new Color[TILE_RESOLUTION * TILE_RESOLUTION];
            _pixelsEmissive = new Color[TILE_RESOLUTION * TILE_RESOLUTION];
            _pixelsSpecular = new Color[TILE_RESOLUTION * TILE_RESOLUTION];

            Color _c = Color.clear;
            Color _clearNormal = new Color(0.5f, 0.5f, 1);
            for (int i = 0; i < _pixelsDiffuse.Length; i++) {
                _pixelsDiffuse[i] = _c;
                _pixelsNormal[i] = _clearNormal;
                _pixelsEmissive[i] = _c;
                _pixelsSpecular[i] = _c;
            }
        }

        // if just below the top line of your slice and you're not drawing up there anyway, draw empty space
        // note: the top line of slices is not included in sliceSize. Also, since it's size and not an index, "sliceSizeY - 1" means the top counted Y, so below the extra line
        if (_tile.LocalGridY == sliceSizeY - 1 && _spriteHeight <= TILE_RESOLUTION) {
            List<Color> _newDiffuse = new List<Color>(_pixelsDiffuse);
            List<Color> _newNormal = new List<Color>(_pixelsNormal);
            List<Color> _newEmissive = new List<Color>(_pixelsEmissive);
            List<Color> _newSpecular = new List<Color>(_pixelsSpecular);

            Color _c = Color.clear;
            Color _clearNormal = new Color(0.5f, 0.5f, 1);

            int _resSqrd = (int)Mathf.Pow(TILE_RESOLUTION, 2);
            for (int i = 0; i < _resSqrd; i++) {
                _newDiffuse.Add(_c);
                _newNormal.Add(_clearNormal);
                _newEmissive.Add(_c);
                _newSpecular.Add(_c);
            }

            _pixelsDiffuse = _newDiffuse.ToArray();
            _pixelsNormal = _newNormal.ToArray();
            _pixelsEmissive = _newEmissive.ToArray();
            _pixelsSpecular = _newSpecular.ToArray();

            _spriteHeight = TILE_RESOLUTION * 2;
        }

        // if bigger than normal (diagonals), find any empty pixels and replace them with pixels from the old texture
        else if (_tile.LocalGridY < sliceSizeY - 1 && _spriteHeight > TILE_RESOLUTION) {

            int _index = 0;
            for (int y = TILE_RESOLUTION; y < _spriteHeight; y++) {
                for (int x = 0; x < _spriteWidth; x++) {
                    _index = (y * _spriteWidth) + x;
                    if (_pixelsDiffuse[_index].a > 0)
                        continue;

                    _pixelsDiffuse[_index] = _gridGraphics.Diffuse.GetPixel((int)_tilePosOnTexture.x + x, (int)_tilePosOnTexture.y + y);
                    _pixelsNormal[_index] = _gridGraphics.Normal.GetPixel((int)_tilePosOnTexture.x + x, (int)_tilePosOnTexture.y + y);
                    _pixelsEmissive[_index] = _gridGraphics.Emissive.GetPixel((int)_tilePosOnTexture.x + x, (int)_tilePosOnTexture.y + y);
                    _pixelsSpecular[_index] = _gridGraphics.Specular.GetPixel((int)_tilePosOnTexture.x + x, (int)_tilePosOnTexture.y + y);
                }
            }
        }

        _gridGraphics.Diffuse.SetPixels((int)_tilePosOnTexture.x, (int)_tilePosOnTexture.y, _spriteWidth, _spriteHeight, _pixelsDiffuse, 0);
        _gridGraphics.Normal.SetPixels((int)_tilePosOnTexture.x, (int)_tilePosOnTexture.y, _spriteWidth, _spriteHeight, _pixelsNormal, 0);
        _gridGraphics.Emissive.SetPixels((int)_tilePosOnTexture.x, (int)_tilePosOnTexture.y, _spriteWidth, _spriteHeight, _pixelsEmissive, 0);
        _gridGraphics.Specular.SetPixels((int)_tilePosOnTexture.x, (int)_tilePosOnTexture.y, _spriteWidth, _spriteHeight, _pixelsSpecular, 0);
        _gridGraphics.IsDirty = true;
    }

    public List<Tile> GetNeighbours(int _gridX, int _gridY) {
        List<Tile> neighbours = new List<Tile>();
        for (int y = -1; y <= 1; y++) {
            for (int x = -1; x <= 1; x++) {
                if (x == 0 && y == 0)
                    continue;

                int checkX = _gridX + x;
                int checkY = _gridY + y;

                if (checkX >= 0 && checkX < GridSizeX && checkY >= 0 && checkY < GridSizeY) {
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

        int x = Mathf.RoundToInt(GridSizeX * percentX);
        int y = Mathf.RoundToInt(GridSizeY * percentY);
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

        while (_neighbours.Count < (GridSizeX * GridSizeY)) {

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

        while (_neighbours.Count < (GridSizeX * GridSizeY)) {

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
