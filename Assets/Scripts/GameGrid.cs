using UnityEngine;
using System.Collections.Generic;

public class GameGrid : Singleton<GameGrid> {

	public static readonly Int2 SIZE = new Int2(48, 48);
	public static int GetArea(){
		return SIZE.x * SIZE.y;
	}
	public static bool IsInsideGrid(int _x, int _y){
		return _x >= 0 && _x < SIZE.x && _y >= 0 && _y < SIZE.y;
	}

	public static class NeighborFinder {
		public static Dictionary<NeighborEnum, Node> CachedNeighbors = new Dictionary<NeighborEnum, Node>() { 
			{ NeighborEnum.None, null },
			{ NeighborEnum.TL, null },
			{ NeighborEnum.T, null },
			{ NeighborEnum.TR, null },
			{ NeighborEnum.R, null },
			{ NeighborEnum.BR, null },
			{ NeighborEnum.B, null },
			{ NeighborEnum.L, null }
		};

		public static bool TryCacheNeighbor(Int2 _nodeGridPos, NeighborEnum _neighbor){
			switch (_neighbor){
				case NeighborEnum.All:
					TryCacheNeighbor(_nodeGridPos, NeighborEnum.TL);
					TryCacheNeighbor(_nodeGridPos, NeighborEnum.T);
					TryCacheNeighbor(_nodeGridPos, NeighborEnum.TR);
					TryCacheNeighbor(_nodeGridPos, NeighborEnum.R);
					TryCacheNeighbor(_nodeGridPos, NeighborEnum.BR);
					TryCacheNeighbor(_nodeGridPos, NeighborEnum.B);
					TryCacheNeighbor(_nodeGridPos, NeighborEnum.BL);
					TryCacheNeighbor(_nodeGridPos, NeighborEnum.L);
					return true;
				case NeighborEnum.None:
					_nodeGridPos = Int2.Zero;
					break;
				case NeighborEnum.TL:
					_nodeGridPos += Int2.UpLeft;
					break;
				case NeighborEnum.T:
					_nodeGridPos += Int2.Up;
					break;
				case NeighborEnum.TR:
					_nodeGridPos += Int2.UpRight;
					break;
				case NeighborEnum.R:
					_nodeGridPos += Int2.Right;
					break;
				case NeighborEnum.BR:
					_nodeGridPos += Int2.DownRight;
					break;
				case NeighborEnum.B:
					_nodeGridPos += Int2.Down;
					break;
				case NeighborEnum.BL:
					_nodeGridPos += Int2.DownLeft;
					break;
				case NeighborEnum.L:
					_nodeGridPos += Int2.Left;
					break;
				default:
					_nodeGridPos = Int2.Zero;
					Debug.LogError(_neighbor + " hasn't been properly implemented yet!");
					break;
			}

			Node node = GameGrid.GetInstance().TryGetNode(_nodeGridPos.x, _nodeGridPos.y);
			CachedNeighbors[_neighbor] = node;
			return node != null;
		}

		public static bool IsCardinalNeighborWall(Int2 _nodeGridPos) {
			TryCacheNeighbor(_nodeGridPos, NeighborEnum.T);
			TryCacheNeighbor(_nodeGridPos, NeighborEnum.B);
			TryCacheNeighbor(_nodeGridPos, NeighborEnum.L);
			TryCacheNeighbor(_nodeGridPos, NeighborEnum.R);
			bool _isWallT = CachedNeighbors[NeighborEnum.T].IsWall;
			bool _isWallB = CachedNeighbors[NeighborEnum.B].IsWall;
			bool _isWallL = CachedNeighbors[NeighborEnum.L].IsWall;
			bool _isWallR = CachedNeighbors[NeighborEnum.R].IsWall;
			return _isWallT || _isWallB || _isWallL || _isWallR;
		}

		public static void GetSurroundingTiles(Int2 _nodeGridPos, out UVController tileTL, out UVController tileTR, out UVController tileBR, out UVController tileBL) {
			Int2 _nodeGridPosTL = new Int2(_nodeGridPos.x - 1, _nodeGridPos.y);
			Int2 _nodeGridPosTR = new Int2(_nodeGridPos.x, _nodeGridPos.y);
			Int2 _nodeGridPosBR = new Int2(_nodeGridPos.x, _nodeGridPos.y - 1);
			Int2 _nodeGridPosBL = new Int2(_nodeGridPos.x - 1, _nodeGridPos.y - 1);
			tileTL = GameGrid.GetInstance().TryGetTile(_nodeGridPosTL);
			tileTR = GameGrid.GetInstance().TryGetTile(_nodeGridPosTR);
			tileBR = GameGrid.GetInstance().TryGetTile(_nodeGridPosBR);
			tileBL = GameGrid.GetInstance().TryGetTile(_nodeGridPosBL);
		}

		public static void GetSurroundingNodes(Int2 _tileGridPos, out Node _nodeTL, out Node _nodeTR, out Node _nodeBR, out Node _nodeBL) {
			Int2 _nodeGridPos = _tileGridPos;
			_nodeTL = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.Up);
			_nodeTR = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.UpRight);
			_nodeBR = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.Right);
			_nodeBL = GameGrid.GetInstance().TryGetNode(_nodeGridPos);
		}

		public static void GetSurroundingNodes(Int2 _nodeGridPos, out Node _nodeTL, out Node _nodeT, out Node _nodeTR, out Node _nodeR, out Node _nodeBR, out Node _nodeB, out Node _nodeBL, out Node _nodeL) {
			_nodeTL = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.UpLeft);
			_nodeT 	= GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.Up);
			_nodeTR = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.UpRight);
			_nodeR 	= GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.Right);
			_nodeBR = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.DownRight);
			_nodeB 	= GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.Down);
			_nodeBL = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.DownLeft);
			_nodeL 	= GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.Left);
		}
	}

	public Material GridMaterial;
	[SerializeField]
	private GameObject tilePrefab;

	public bool GenerateWalls = true;
    public bool DisplayGridGizmos;
    public bool DisplayPaths;
    public bool DisplayWaypoints;

    private Node[,] nodeGrid;
	private UVController[,] tileGrid;


	public override bool IsUsingStartEarly() { return true; }
	public override void StartEarly() {
		base.StartEarly();
		CreateGrid();
	}

	[SerializeField] private int Seed;
	void CreateGrid() {
		Random.InitState(Seed);
		
		nodeGrid = new Node[SIZE.x, SIZE.y];
		tileGrid = new UVController[SIZE.x + 2, SIZE.y + 2];

		Vector3 worldPosBottomLeft = transform.position;
		worldPosBottomLeft.x -= SIZE.x * 0.5f;
		worldPosBottomLeft.y -= SIZE.y * 0.5f;

		for (int y = 0; y < SIZE.y; y++) {
            for (int x = 0; x < SIZE.x; x++) {
				Vector3 worldPos = worldPosBottomLeft;
				worldPos.x += x * Node.DIAMETER + Node.RADIUS;
				worldPos.y += y * Node.DIAMETER + Node.RADIUS;

                nodeGrid[x, y] = new Node(worldPos, x, y);
			}
        }

		for (int y = 0; y < SIZE.y - 1; y++) {
            for (int x = 0; x < SIZE.x - 1; x++) {
				Vector3 worldPos = worldPosBottomLeft + new Vector3(Node.RADIUS, Node.RADIUS, 0.0f);
				worldPos.x += x * Node.DIAMETER + Node.RADIUS;
				worldPos.y += y * Node.DIAMETER + Node.RADIUS;

				tileGrid[x, y] = (Instantiate(tilePrefab, worldPos, Quaternion.identity) as GameObject).GetComponent<UVController>();
				tileGrid[x, y].SetTileGridPos(new Int2(x, y));
			}
        }

		Node _node;
        for (int y = 0; y < SIZE.y; y++) {
            for (int x = 0; x < SIZE.x; x++) {
				_node = nodeGrid[x, y];
				
				if (x == 1 || y == 1 || x == SIZE.x - 2 || y == SIZE.y - 2){
					_node.TrySetIsWall(true);
				}

				_node.ScheduleUpdateGraphicsForSurroundingTiles();
			}
        }
    }

    public List<Node> GetNeighbours(int _gridX, int _gridY) {
        List<Node> neighbours = new List<Node>();
        for (int y = -1; y <= 1; y++) {
            for (int x = -1; x <= 1; x++) {
                if (x == 0 && y == 0)
                    continue;

                int checkX = _gridX + x;
                int checkY = _gridY + y;

                if (checkX >= 0 && checkX < SIZE.x && checkY >= 0 && checkY < SIZE.y) {
                    neighbours.Add(nodeGrid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

	public Int2 GetNodeGridPosFromWorldPos(Vector3 _worldPos) { 
        float percentX = (_worldPos.x - Node.RADIUS + SIZE.x * 0.5f) / (float)SIZE.x;
        float percentY = (_worldPos.y - Node.RADIUS + SIZE.y * 0.5f) / (float)SIZE.y;
		percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int _x = Mathf.RoundToInt(SIZE.x * percentX);
        int _y = Mathf.RoundToInt(SIZE.y * percentY);
        _x = (int)Mathf.Clamp(_x, 0, SIZE.x - 1);
        _y = (int)Mathf.Clamp(_y, 0, SIZE.y - 1);

		return new Int2(_x, _y);
	}
	public Node GetNodeFromWorldPos(Vector3 _worldPos) {
		Int2 _gridPos = GetNodeGridPosFromWorldPos(_worldPos);
		return nodeGrid[_gridPos.x, _gridPos.y];
    }
    public Vector3 GetWorldPosFromNodeGridPos(Int2 _nodeGridPos){
        Vector3 _worldPos = new Vector3(_nodeGridPos.x + Node.RADIUS, _nodeGridPos.y + Node.RADIUS, 0.0f);
        _worldPos.x -= (SIZE.x * 0.5f);
        _worldPos.y -= (SIZE.y * 0.5f);
        return _worldPos;
    }

    public Node GetClosestFreeNode(Vector3 _worldPos) {
        Node _node = GetNodeFromWorldPos(_worldPos);
		if (_node.IsWalkable()) { 
			return _node;
		}

        List<Node> _neighbours = GetNeighbours(_node.GridPos.x, _node.GridPos.y);
        int _lastCount = 0;

        while (_neighbours.Count < (SIZE.x * SIZE.y)) {

            // iterate over _neighbours until a free node is found
            for (int i = _lastCount; i < _neighbours.Count; i++) {
				if (_neighbours[i].IsWalkable() && _neighbours[i].GetOccupyingTileObject() == null) { 
					return _neighbours[i];
				}
            }

            int _prevLastCount = _lastCount;
            _lastCount = _neighbours.Count; // save progress before we add new neighbours, so we don't iterate over old stuff later

            // iterate over _neighbours - if their neighbours aren't in _neighbours, add them.
            List<Node> _newNeighbours = new List<Node>();
            for (int i = _prevLastCount; i < _lastCount; i++) {
                _newNeighbours = GetNeighbours(_neighbours[i].GridPos.x, _neighbours[i].GridPos.y);
                for (int j = 0; j < _newNeighbours.Count; j++) {
                    if (_neighbours.Contains(_newNeighbours[j]))
                        continue;

                    _neighbours.Add(_newNeighbours[j]);
                }
            }
        }
        return null;
    }
    public Node GetClosestFreeNode(Node _tile) {
		if (!_tile.IsWall && _tile.GetOccupyingTileObject() == null) { 
			return _tile;
		}

        List<Node> _neighbours = GetNeighbours(_tile.GridPos.x, _tile.GridPos.y);
        int _lastCount = 0;

        while (_neighbours.Count < (SIZE.x * SIZE.y)) {

            // iterate over _neighbours until a free node is found
            for (int i = _lastCount; i < _neighbours.Count; i++) {
				if (!_neighbours[i].IsWall && _neighbours[i].GetOccupyingTileObject() == null) { 
					return _neighbours[i];
				}
            }

            int _prevLastCount = _lastCount;
            _lastCount = _neighbours.Count; // save progress before we add new neighbours, so we don't iterate over old stuff later

            // iterate over _neighbours - if their neighbours aren't in _neighbours, add them.
            List<Node> _newNeighbours = new List<Node>();
            for (int i = _prevLastCount; i < _lastCount; i++) {
                _newNeighbours = GetNeighbours(_neighbours[i].GridPos.x, _neighbours[i].GridPos.y);
                for (int j = 0; j < _newNeighbours.Count; j++) {
					if (_neighbours.Contains(_newNeighbours[j])) { 
						continue;
					}

                    _neighbours.Add(_newNeighbours[j]);
                }
            }
        }
        return null;
    }

    public Node GetRandomWalkableNode(Node _exclude = null) {
        Node _node = null;
        int _x = 0;
        int _y = 0;

        do {
            _x = (int)Random.Range(0, SIZE.x);
            _y = (int)Random.Range(0, SIZE.y);

            _node = nodeGrid[_x, _y];
        } while (!_node.IsWalkable());

        return _node;
    }
    
    void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position, new Vector3(SIZE.x, SIZE.y, 1));

        if (nodeGrid != null && DisplayGridGizmos) {
            foreach (Node _node in nodeGrid) {
                Gizmos.color = _node.IsWalkable() ? Color.white : Color.red;
                Gizmos.DrawWireCube(_node.WorldPos, Vector3.one * 0.1f);
            }
        }
    }

	public Node TryGetNode(Int2 _posGrid) {
		return TryGetNode(_posGrid.x, _posGrid.y);
	}

	public Node TryGetNode(int _posGridX, int _posGridY) {
		if (!IsInsideGrid(_posGridX, _posGridY)) {
			return null;
		}

		return nodeGrid[_posGridX, _posGridY];
	}

	public UVController TryGetTile(Int2 _posGrid) {
		return TryGetTile(_posGrid.x, _posGrid.y);
	}

	public UVController TryGetTile(int _posGridX, int _posGridY) {
		if (!IsInsideGrid(_posGridX, _posGridY)) {
			return null;
		}

		return tileGrid[_posGridX, _posGridY];
	}
}
