using UnityEngine;
using System.Collections.Generic;

public class Grid : MonoBehaviour {

    public static Grid Instance;

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

        gridGraphicsBottom = new GridGraphics[GridGraphicsBottomRenderers.Length];
        gridGraphicsTop = new GridGraphics[GridGraphicsBottomRenderers.Length];

        nodeDiameter = NodeRadius * 2;
        GridSizeX = Mathf.RoundToInt(GridWorldSize.x / nodeDiameter);
        GridSizeY = Mathf.RoundToInt(GridWorldSize.y / nodeDiameter);

        CreateGrid();
    }

    public static List<TileAnimator> LateUpdateAnimators = new List<TileAnimator>();
    void LateUpdate() {
        for (int i = 0; i < LateUpdateAnimators.Count; i++) {
            LateUpdateAnimators[i].LateUpdate();
        }
    }

    void CreateGrid() {
        grid = new Tile[GridSizeX, GridSizeY];

        Vector3 worldBottomLeft = transform.position - (Vector3.right * GridWorldSize.x / 2) - (Vector3.up * GridWorldSize.y / 2) - new Vector3(0, 0.5f, 0); // 0.5f because of the +1 for diagonals
        for (int y = 0; y < GridSizeY; y++) {
            for (int x = 0; x < GridSizeX; x++) {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + NodeRadius) + Vector3.up * (y * nodeDiameter + NodeRadius + 0.5f); // +0.5f for diagonals

                grid[x, y] = new Tile(worldPoint, x, y/*, x % sliceSizeX, y % sliceSizeY, GetGridSliceIndex(x, y)*/);
                grid[x, y].Init();
            }
        }
        // for testing-purposes
        for (int y = 0; y < GridSizeY; y++) {
            for (int x = 0; x < GridSizeX; x++) {
				if (Random.value > 0.75f) {
                    grid[x, y].SetTileType(Tile.Type.Solid, Tile.TileOrientation.None);
                    continue;
                }

				grid[x, y].SetTileType(Tile.Type.Empty, Tile.TileOrientation.None);
				grid[x, y].SetFloorType(Tile.Type.Solid, Tile.TileOrientation.None);
            }
        }
		bool _b;
		bool _success;
        for (int y = 0; y < GridSizeY; y++) {
            for (int x = 0; x < GridSizeX; x++) {
                if (grid[x, y]._WallType_ == Tile.Type.Solid)
                    continue;
				#if !UNITY_EDITOR_OSX
				_b = RUL.Rul.RandBool () ;
				#endif
				#if UNITY_EDITOR_OSX
				_b = Random.value > 0.5f;
				#endif

				if (_b) {
					_success = false;

					// LT
					if (x > 0 && grid[x - 1, y].CanConnect_R && y < GridSizeY - 1 && grid[x, y + 1].CanConnect_B){
						grid[x, y].SetTileType(Tile.Type.Diagonal, Tile.TileOrientation.TopLeft);
						_success = true;
					}
					// TR
					else if (x < GridSizeX - 1 && grid[x + 1, y].CanConnect_L && y < GridSizeY - 1 && grid[x, y + 1].CanConnect_B){
						grid[x, y].SetTileType(Tile.Type.Diagonal, Tile.TileOrientation.TopRight);
						_success = true;
					}
					// RB
					else if (x < GridSizeX - 1 && grid[x + 1, y].CanConnect_L && y > 0 && grid[x, y - 1].CanConnect_T){
						grid[x, y].SetTileType(Tile.Type.Diagonal, Tile.TileOrientation.BottomRight);
						_success = true;
					}
					// BL
					else if (x > 0 && grid[x - 1, y].CanConnect_R && y > 0 && grid[x, y - 1].CanConnect_T){
						grid[x, y].SetTileType(Tile.Type.Diagonal, Tile.TileOrientation.BottomLeft);
						_success = true;
					}

					if(_success)
						grid[x, y].SetFloorType(Tile.Type.Empty, Tile.TileOrientation.None);
				}

				// floor stuff

				if (grid[x, y]._FloorType_ == Tile.Type.Solid)
					continue;
				if (grid [x, y]._WallType_ == Tile.Type.Diagonal)
					grid [x, y].SetFloorType (Tile.Type.Diagonal, Tile.GetReverseDirection(grid[x, y]._Orientation_));
				
				// LT
				if (x > 0 && grid [x - 1, y].CanConnectFloor_R && y < GridSizeY - 1 && grid [x, y + 1].CanConnectFloor_B)
					grid [x, y].SetFloorType (Tile.Type.Diagonal, Tile.TileOrientation.TopLeft);
				// TR
				else if (x < GridSizeX - 1 && grid [x + 1, y].CanConnectFloor_L && y < GridSizeY - 1 && grid [x, y + 1].CanConnectFloor_B)
					grid [x, y].SetFloorType (Tile.Type.Diagonal, Tile.TileOrientation.TopRight);
				// RB
				else if (x < GridSizeX - 1 && grid [x + 1, y].CanConnectFloor_L && y > 0 && grid [x, y - 1].CanConnectFloor_T)
					grid [x, y].SetFloorType (Tile.Type.Diagonal, Tile.TileOrientation.BottomRight);
				// BL
				else if (x > 0 && grid [x - 1, y].CanConnectFloor_R && y > 0 && grid [x, y - 1].CanConnectFloor_T)
					grid [x, y].SetFloorType (Tile.Type.Diagonal, Tile.TileOrientation.BottomLeft);
            }
        }
    }

    public static bool OtherTileIsBlockingPath(Tile.Type _otherTileType, Tile.TileOrientation _otherTileOrientation, Tile.TileOrientation _directionToOtherTile) {
        if (_otherTileType == Tile.Type.Empty)
            return false;
        if (_otherTileType == Tile.Type.Solid || _otherTileType == Tile.Type.Door || _otherTileType == Tile.Type.Airlock)
            return true;

        if (_otherTileType != Tile.Type.Diagonal)
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
        if (_tile._WallType_ == Tile.Type.Empty && !_tile.IsOccupied)
            return _tile;

        List<Tile> _neighbours = GetNeighbours(_tile.GridX, _tile.GridY);
        int _lastCount = 0;

        while (_neighbours.Count < (GridSizeX * GridSizeY)) {

            // iterate over _neighbours until a free node is found
            for (int i = _lastCount; i < _neighbours.Count; i++) {
                if (_neighbours[i]._WallType_ != Tile.Type.Empty || _neighbours[i].IsOccupied)
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
