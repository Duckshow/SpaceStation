using UnityEngine;
using System.Collections.Generic;

public class Grid : MonoBehaviour {

	private static Grid instance;
	public static Grid Instance {
		get { 
			if(instance == null)
				instance = FindObjectOfType<Grid>();
			return instance;
		}
	}
	public Material GridMaterial;

    public const float WORLD_BOTTOM_HEIGHT = 0.01f;
    public const float WORLD_TOP_HEIGHT = -0.01f;

    public bool GenerateWalls = true;
    public bool DisplayGridGizmos;
    public bool DisplayPaths;
    public bool DisplayWaypoints;
    public Vector2 GridWorldSize;

    [HideInInspector]
    public Tile[,] grid; // should make 1D

    public static int MaxSize { get { return GridSizeX * GridSizeY; } }
	private int sizeX = -1;
	public static int GridSizeX { 
		get { 
			if(Grid.Instance.sizeX == -1)
				Grid.Instance.sizeX = Mathf.RoundToInt(Grid.Instance.GridWorldSize.x / Tile.DIAMETER);
			return Grid.Instance.sizeX;
		} 
	}
	private int sizeY = -1;
	public static int GridSizeY{
		get{
			if (Grid.Instance.sizeY == -1)
				Grid.Instance.sizeY = Mathf.RoundToInt(Grid.Instance.GridWorldSize.y / Tile.DIAMETER);
			return Grid.Instance.sizeY;
		}
	}
	private int sizeXHalf = -1;
	public static int GridSizeXHalf{
		get{
			if (Grid.Instance.sizeXHalf == -1)
				Grid.Instance.sizeXHalf = Mathf.RoundToInt(Grid.GridSizeX * 0.5f);
			return Grid.Instance.sizeXHalf;
		}
	}
	private int sizeYHalf = -1;
	public static int GridSizeYHalf{
		get{
			if (Grid.Instance.sizeYHalf == -1)
				Grid.Instance.sizeYHalf = Mathf.RoundToInt(Grid.GridSizeY * 0.5f);
			return Grid.Instance.sizeYHalf;
		}
	}


	void Awake() {
	}

	void Start() { 
        CreateGrid();
	}

	public static List<TileAnimator> LateUpdateAnimators = new List<TileAnimator>();
    void LateUpdate() {
        for (int i = 0; i < LateUpdateAnimators.Count; i++) {
            LateUpdateAnimators[i].LateUpdate();
        }
    }

	[SerializeField] private int Seed;
	void CreateGrid() {
		Random.InitState(Seed);
		grid = new Tile[GridSizeX, GridSizeY];
		Tile _tile;

		Vector3 worldBottomLeft = transform.position - (Vector3.right * GridWorldSize.x / 2) - (Vector3.up * GridWorldSize.y / 2) - new Vector3(0, 0.5f, 0); // 0.5f because of the +1 for diagonals
        for (int y = 0; y < GridSizeY; y++) {
            for (int x = 0; x < GridSizeX; x++) {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * Tile.DIAMETER + Tile.RADIUS) + Vector3.up * (y * Tile.DIAMETER + Tile.RADIUS + 0.5f); // +0.5f for diagonals

                grid[x, y] = new Tile(worldPoint, x, y);
            }
        }
        // for testing-purposes
        for (int y = 0; y < GridSizeY; y++) {
            for (int x = 0; x < GridSizeX; x++) {
				_tile = grid[x, y];

				// generate one wall in the center
				// if (GenerateWalls && y == (GridSizeY * 0.5f) && x == (GridSizeX * 0.5f)) {
				//     grid[x, y].SetTileType(Tile.Type.Solid, Tile.TileOrientation.None);
				//     continue;
				// }
				// generate walls randomly
				if (GenerateWalls && Random.value > 0.9f) {
                    _tile.SetTileType(Tile.Type.Solid, Tile.TileOrientation.None);
                    continue;
                }

				_tile.SetTileType(Tile.Type.Empty, Tile.TileOrientation.None);
				_tile.SetFloorType(Tile.Type.Solid, Tile.TileOrientation.None);
            }
        }
		bool _b;
		bool _success;
		Tile _neighbourL;
		Tile _neighbourR;
		Tile _neighbourT;
		Tile _neighbourB;
        for (int y = 0; y < GridSizeY; y++) {
            for (int x = 0; x < GridSizeX; x++) {
				_tile = grid[x, y];

				if (_tile._WallType_ == Tile.Type.Solid)
                    continue;
				#if !UNITY_EDITOR_OSX
				_b = RUL.Rul.RandBool () ;
				#endif
				#if UNITY_EDITOR_OSX
				_b = Random.value > 0.5f;
				#endif

				if (_b) {
					_success = false;

					_neighbourL = x > 0 ? 				grid[x - 1, y] : null;
					_neighbourR = x < GridSizeX - 1 ? 	grid[x + 1, y] : null;
					_neighbourT = y < GridSizeY - 1 ? 	grid[x, y + 1] : null;
					_neighbourB = y > 0 ? 				grid[x, y - 1] : null;

					// TL
					if (_neighbourT != null && _neighbourL != null && _neighbourT.CanConnect_B && _neighbourL.CanConnect_R){
						_tile.SetTileType(Tile.Type.Diagonal, Tile.TileOrientation.TopLeft);
						_success = true;
					}
					// TR
					else if (_neighbourT != null && _neighbourR != null && _neighbourT.CanConnect_B && _neighbourR.CanConnect_L){
						_tile.SetTileType(Tile.Type.Diagonal, Tile.TileOrientation.TopRight);
						_success = true;
					}
					// BR
					else if (_neighbourB != null && _neighbourR != null && _neighbourB.CanConnect_T && _neighbourR.CanConnect_L){
						_tile.SetTileType(Tile.Type.Diagonal, Tile.TileOrientation.BottomRight);
						_success = true;
					}
					// BL
					else if (_neighbourB != null && _neighbourL != null && _neighbourB.CanConnect_T && _neighbourL.CanConnect_R){
						_tile.SetTileType(Tile.Type.Diagonal, Tile.TileOrientation.BottomLeft);
						_success = true;
					}

					if(_success)
						_tile.SetFloorType(Tile.Type.Empty, Tile.TileOrientation.None);
				}

				// floor stuff

				if (_tile._FloorType_ == Tile.Type.Solid)
					continue;
				if (_tile._WallType_ == Tile.Type.Diagonal)
					_tile.SetFloorType (Tile.Type.Diagonal, Tile.GetReverseDirection(_tile._Orientation_));


				_neighbourL = grid[x - 1, y];
				_neighbourR = grid[x + 1, y];
				_neighbourT = grid[x, y + 1];
				_neighbourB = grid[x, y + 1];

				// TL
				if (_neighbourT != null && _neighbourL != null && _neighbourT.CanConnectFloor_B && _neighbourL.CanConnectFloor_R){
					_tile.SetFloorType(Tile.Type.Diagonal, Tile.TileOrientation.TopLeft);
				}
				// TR
				else if (_neighbourT != null && _neighbourR != null && _neighbourT.CanConnectFloor_B && _neighbourR.CanConnectFloor_L){
					_tile.SetFloorType(Tile.Type.Diagonal, Tile.TileOrientation.TopRight);
				}
				// BR
				else if (_neighbourB != null && _neighbourR != null && _neighbourB.CanConnectFloor_T && _neighbourR.CanConnectFloor_L){
					_tile.SetFloorType(Tile.Type.Diagonal, Tile.TileOrientation.BottomRight);
				}
				// BL
				else if (_neighbourB != null && _neighbourL != null && _neighbourB.CanConnectFloor_T && _neighbourL.CanConnectFloor_R){
					_tile.SetFloorType(Tile.Type.Diagonal, Tile.TileOrientation.BottomLeft);
				}
            }
        }
        for (int y = 0; y < GridSizeY; y++) {
            for (int x = 0; x < GridSizeX; x++) {
				_tile = grid[x, y];
				_tile.UpdateWallCornerHider(false);
				_tile.UpdateFloorCornerHider(false);
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

	public Vector2i GetTileCoordFromWorldPoint(Vector3 _worldPosition) { 
        float percentX = (_worldPosition.x - Tile.RADIUS + GridWorldSize.x * 0.5f) / GridWorldSize.x;
        float percentY = (_worldPosition.y - Tile.RADIUS + GridWorldSize.y * 0.5f) / GridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt(GridSizeX * percentX);
        int y = Mathf.RoundToInt(GridSizeY * percentY);
        x = (int)Mathf.Clamp(x, 0, GridWorldSize.x - 1);
        y = (int)Mathf.Clamp(y, 0, GridWorldSize.y - 1);

		return new Vector2i(x, y);
	}
	public Tile GetTileFromWorldPoint(Vector3 _worldPosition) {
		Vector2i _coord = GetTileCoordFromWorldPoint(_worldPosition);
		return grid[_coord.x, _coord.y];
    }
    public Vector3 GetWorldPointFromTileCoord(Vector2i _tileCoord){
        Vector3 _pos = _tileCoord;
        _pos.x -= (GridSizeXHalf - 0.5f);
        _pos.y -= (GridSizeYHalf - 0.5f);
        return _pos;
    }

    public Tile GetClosestFreeNode(Vector3 _worldPosition) {
        Tile _tile = GetTileFromWorldPoint(_worldPosition);
        if (_tile.Walkable && _tile.GetOccupyingTileObject() == null)
            return _tile;

        List<Tile> _neighbours = GetNeighbours(_tile.GridCoord.x, _tile.GridCoord.y);
        int _lastCount = 0;

        while (_neighbours.Count < (GridSizeX * GridSizeY)) {

            // iterate over _neighbours until a free node is found
            for (int i = _lastCount; i < _neighbours.Count; i++) {
                if (_neighbours[i].Walkable && _neighbours[i].GetOccupyingTileObject() == null)
                    return _neighbours[i];
            }

            int _prevLastCount = _lastCount;
            _lastCount = _neighbours.Count; // save progress before we add new neighbours, so we don't iterate over old stuff later

            // iterate over _neighbours - if their neighbours aren't in _neighbours, add them.
            List<Tile> _newNeighbours = new List<Tile>();
            for (int i = _prevLastCount; i < _lastCount; i++) {
                _newNeighbours = GetNeighbours(_neighbours[i].GridCoord.x, _neighbours[i].GridCoord.y);
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
        if (_tile._WallType_ == Tile.Type.Empty && _tile.GetOccupyingTileObject() == null)
            return _tile;

        List<Tile> _neighbours = GetNeighbours(_tile.GridCoord.x, _tile.GridCoord.y);
        int _lastCount = 0;

        while (_neighbours.Count < (GridSizeX * GridSizeY)) {

            // iterate over _neighbours until a free node is found
            for (int i = _lastCount; i < _neighbours.Count; i++) {
                if (_neighbours[i]._WallType_ == Tile.Type.Empty && _neighbours[i].GetOccupyingTileObject() == null)
                    return _neighbours[i];
            }

            int _prevLastCount = _lastCount;
            _lastCount = _neighbours.Count; // save progress before we add new neighbours, so we don't iterate over old stuff later

            // iterate over _neighbours - if their neighbours aren't in _neighbours, add them.
            List<Tile> _newNeighbours = new List<Tile>();
            for (int i = _prevLastCount; i < _lastCount; i++) {
                _newNeighbours = GetNeighbours(_neighbours[i].GridCoord.x, _neighbours[i].GridCoord.y);
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
        int _xDiff = _neighbour.GridCoord.x - _tile.GridCoord.x;
        int _yDiff = _neighbour.GridCoord.y - _tile.GridCoord.y;
        Tile _nonDiagonalNeighbour1 = null;
        Tile _nonDiagonalNeighbour2 = null;

        // bottom left
        if (_xDiff == -1 && _yDiff == -1) {
            _nonDiagonalNeighbour1 = grid[_tile.GridCoord.x, _tile.GridCoord.y - 1];
            _nonDiagonalNeighbour2 = grid[_tile.GridCoord.x - 1, _tile.GridCoord.y];
        }
        // bottom right
        else if (_xDiff == 1 && _yDiff == -1) {
            _nonDiagonalNeighbour1 = grid[_tile.GridCoord.x, _tile.GridCoord.y - 1];
            _nonDiagonalNeighbour2 = grid[_tile.GridCoord.x + 1, _tile.GridCoord.y];
        }
        // top left
        else if (_xDiff == -1 && _yDiff == 1) {
            _nonDiagonalNeighbour1 = grid[_tile.GridCoord.x, _tile.GridCoord.y + 1];
            _nonDiagonalNeighbour2 = grid[_tile.GridCoord.x - 1, _tile.GridCoord.y];
        }
        // top right
        else if (_xDiff == 1 && _yDiff == 1) {
            _nonDiagonalNeighbour1 = grid[_tile.GridCoord.x, _tile.GridCoord.y + 1];
            _nonDiagonalNeighbour2 = grid[_tile.GridCoord.x + 1, _tile.GridCoord.y];
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
                Gizmos.DrawWireCube(n.WorldPosition, Vector3.one * (Tile.DIAMETER - 0.1f));
            }
        }
    }
}
