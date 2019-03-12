using UnityEngine;
using System.Collections.Generic;

public class GameGrid : Singleton<GameGrid> {

	public static readonly Int2 SIZE = new Int2(48, 48);
	public const float TILE_RADIUS = 0.5f;
	public const float TILE_DIAMETER = 1;
	public const int TILE_RESOLUTION = 16;

	public static int GetArea(){
		return SIZE.x * SIZE.y;
	}
	
	public static bool IsInsideNodeGrid(Int2 _nodeGridPos){
		return IsInsideNodeGrid(_nodeGridPos.x, _nodeGridPos.y);
	}
	
	public static bool IsInsideNodeGrid(int _x, int _y){
		return _x >= 0 && _x < SIZE.x && _y >= 0 && _y < SIZE.y;
	}

	[SerializeField]
	private GameObject tilePrefab;

	public enum GridType { None, NodeGrid, TileGrid }

	public bool GenerateWalls = true;
    public bool DisplayGridGizmos;
    public bool DisplayPaths;
    public bool DisplayWaypoints;

	[SerializeField] private int Seed;
	[Space]
	[SerializeField] private Material gridMaterial;
	[SerializeField] private GameGridMesh meshBackground;
	[SerializeField] private GameGridMesh meshInteractivesBack;
	[SerializeField] private GameGridMesh meshInteractivesFront;

	private Node[,] nodeGrid;


	[EasyButtons.Button]
	public void GenerateMeshes() {
		GameGridMesh.GridMaterial = gridMaterial;
		meshBackground.CreateMesh();
		meshInteractivesBack.CreateMesh();
		meshInteractivesFront.CreateMesh();
	}

	public override bool IsUsingAwakeEarly() { return true; }
	public override void AwakeEarly() {
		base.AwakeEarly();

		transform.position = new Vector3(0.5f, 0.5f, 0.0f);

		GameGridMesh.InitStatic();
		meshBackground.Init(Sorting.Back, GameGridMesh.RenderMode.Walls);
		meshInteractivesBack.Init(Sorting.Back, GameGridMesh.RenderMode.Interactives);
		meshInteractivesFront.Init(Sorting.Front, GameGridMesh.RenderMode.Interactives);
		GenerateMeshes();
	}

	public override bool IsUsingStartEarly() { return true; }
	public override void StartEarly() {
		base.StartEarly();
		CreateGrid();
	}

	public override bool IsUsingUpdateLate() { return true; }
	public override void UpdateLate(){
		base.UpdateLate();
		meshBackground.TryUpdateVisuals();
		meshInteractivesBack.TryUpdateVisuals();
		meshInteractivesFront.TryUpdateVisuals();

		// for (int y = 0; y < SIZE.y; y++){
		// 	for (int x = 0; x < SIZE.x; x++){
		// 		nodeGrid[x, y].ChemicalContent.UpdateWallBounce();
		// 	}
		// }
		
		// for (int y = 0; y < SIZE.y; y++){
		// 	for (int x = 0; x < SIZE.x; x++){
		// 		nodeGrid[x, y].ChemicalContent.UpdatePressure();
		// 	}
		// }

		// float _total = 0.0f;
		// for (int y = 0; y < SIZE.y; y++){
		// 	for (int x = 0; x < SIZE.x; x++){
		// 		nodeGrid[x, y].ChemicalContent.ApplyAmountDelta();
		// 		_total += nodeGrid[x, y].ChemicalContent.Amount;
		// 	}
		// }
		// Debug.Log("Total: " + _total);
	}

	void CreateGrid() {
		Random.InitState(Seed);
		
		nodeGrid = new Node[SIZE.x, SIZE.y];

		Vector3 worldPosBottomLeft = transform.position;
		worldPosBottomLeft.x -= SIZE.x * 0.5f;
		worldPosBottomLeft.y -= SIZE.y * 0.5f;

		for (int y = 0; y < SIZE.y; y++) {
            for (int x = 0; x < SIZE.x; x++) {
				Vector3 worldPos = worldPosBottomLeft;
				worldPos.x += x * TILE_DIAMETER + TILE_RADIUS;
				worldPos.y += y * TILE_DIAMETER + TILE_RADIUS;

                nodeGrid[x, y] = new Node(worldPos, x, y);
			}
        }

		Node _node;
        for (int y = 0; y < SIZE.y; y++) {
            for (int x = 0; x < SIZE.x; x++) {
				_node = nodeGrid[x, y];

				bool _isXAtLeftBorder = x == 1;
				bool _isXAtRightBorder = x == SIZE.x - 1;
				bool _isXBetweenBorders = x > 0 && x < SIZE.x - 1;

				bool _isYAtBottomBorder = y == 1;
				bool _isYAtTopBorder = y == SIZE.y - 1;
				bool _isYBetweenBorders = y > 0 && y < SIZE.y - 1;

				if (((_isXAtLeftBorder || _isXAtRightBorder) && _isYBetweenBorders) || ((_isYAtBottomBorder || _isYAtTopBorder) && _isXBetweenBorders)){
					_node.TrySetIsWall(true);
				}

				int _roomSize = 4;
				int _roomMinX = SIZE.x / 2 - _roomSize;
				int _roomMinY = SIZE.x / 2 - _roomSize;
				int _roomMaxX = SIZE.x / 2 + _roomSize;
				int _roomMaxY = SIZE.x / 2 + _roomSize;

				int _gasBubbleSize = 1;
				int _gasMinX = SIZE.x / 2 - _gasBubbleSize;
				int _gasMinY = SIZE.x / 2 - _gasBubbleSize;
				int _gasMaxX = SIZE.x / 2 + _gasBubbleSize;
				int _gasMaxY = SIZE.x / 2 + _gasBubbleSize;

				// if ((x == _roomMinX || x == _roomMaxX) && y >= _roomMinY && y <= _roomMaxY){
				// 	_node.TrySetIsWall(true);
				// }
				// if ((y == _roomMinY || y == _roomMaxY) && x >= _roomMinX && x <= _roomMaxX){
				// 	_node.TrySetIsWall(true);
				// }
				// if (x > _gasMinX && x < _gasMaxX && y > _gasMinY && y < _gasMaxY){
				// 	_node.ChemicalContent.SetAmount(Mathf.RoundToInt(100));
				// }

				_node.ScheduleUpdateGraphicsForSurroundingTiles();
			}
        }
    }

	public Int2 GetGridPosFromWorldPos(Vector3 _worldPos, GridType _gridType) {
		float _nodeOffset = 0.0f;
		switch (_gridType){
			case GridType.None:
			case GridType.NodeGrid:
				break;
			case GridType.TileGrid:
				_nodeOffset = TILE_RADIUS;
				break;
			default:
				Debug.LogError(_gridType + " hasn't been properly implemented yet!");
				break;
		}

		float percentX = (_worldPos.x - (TILE_DIAMETER + _nodeOffset) + SIZE.x * 0.5f) / (float)SIZE.x;
		float percentY = (_worldPos.y - (TILE_DIAMETER + _nodeOffset) + SIZE.y * 0.5f) / (float)SIZE.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int _x = Mathf.RoundToInt(SIZE.x * percentX);
		int _y = Mathf.RoundToInt(SIZE.y * percentY);
		_x = (int)Mathf.Clamp(_x, 0, SIZE.x - 1);
		_y = (int)Mathf.Clamp(_y, 0, SIZE.y - 1);

		return new Int2(_x, _y);
	}

	public Node GetNodeFromWorldPos(Vector3 _worldPos) {
		Int2 _nodeGridPos = GetGridPosFromWorldPos(_worldPos, GridType.NodeGrid);
		return nodeGrid[_nodeGridPos.x, _nodeGridPos.y];
    }
    
	public Vector3 GetWorldPosFromNodeGridPos(Int2 _nodeGridPos){
        Vector3 _worldPos = new Vector3(_nodeGridPos.x + TILE_RADIUS, _nodeGridPos.y + TILE_RADIUS, 0.0f);
        _worldPos.x -= (SIZE.x * 0.5f);
        _worldPos.y -= (SIZE.y * 0.5f);
        return _worldPos;
    }

    public Node GetClosestFreeNode(Vector3 _worldPos) {
        Node _node = GetNodeFromWorldPos(_worldPos);
		if (_node.GetIsWalkable()) { 
			return _node;
		}

		Node[] _nodes;
		NeighborFinder.GetSurroundingNodes(_node.GridPos, out _nodes);
		List<Node> _neighbours = new List<Node>(_nodes);

		int _lastCount = 0;

        while (_neighbours.Count < (SIZE.x * SIZE.y)) {

            // iterate over _neighbours until a free node is found
            for (int i = _lastCount; i < _neighbours.Count; i++) {
				if (_neighbours[i].GetIsWalkable() && _neighbours[i].GetOccupyingNodeObject() == null) { 
					return _neighbours[i];
				}
            }

            int _prevLastCount = _lastCount;
            _lastCount = _neighbours.Count; // save progress before we add new neighbours, so we don't iterate over old stuff later

            // iterate over _neighbours - if their neighbours aren't in _neighbours, add them.
            Node[] _newNeighbours;
            for (int i = _prevLastCount; i < _lastCount; i++) {
                NeighborFinder.GetSurroundingNodes(_neighbours[i].GridPos, out _newNeighbours);
                for (int j = 0; j < _newNeighbours.Length; j++) {
                    if (_neighbours.Contains(_newNeighbours[j]))
                        continue;

                    _neighbours.Add(_newNeighbours[j]);
                }
            }
        }
        return null;
    }
    public Node GetClosestFreeNode(Node _node) {
		if (!_node.IsWall && _node.GetOccupyingNodeObject() == null) { 
			return _node;
		}

		Node[] _nodes;
		NeighborFinder.GetSurroundingNodes(_node.GridPos, out _nodes);
		List<Node> _neighbours = new List<Node>(_nodes);
		int _lastCount = 0;

        while (_neighbours.Count < (SIZE.x * SIZE.y)) {

            // iterate over _neighbours until a free node is found
            for (int i = _lastCount; i < _neighbours.Count; i++) {
				if (!_neighbours[i].IsWall && _neighbours[i].GetOccupyingNodeObject() == null) { 
					return _neighbours[i];
				}
            }

            int _prevLastCount = _lastCount;
            _lastCount = _neighbours.Count; // save progress before we add new neighbours, so we don't iterate over old stuff later

			// iterate over _neighbours - if their neighbours aren't in _neighbours, add them.
			Node[] _newNeighbours;
			for (int i = _prevLastCount; i < _lastCount; i++) {
				NeighborFinder.GetSurroundingNodes(_neighbours[i].GridPos, out _newNeighbours);
				for (int j = 0; j < _newNeighbours.Length; j++) {
					if (_neighbours.Contains(_newNeighbours[j])) { 
						continue;
					}

                    _neighbours.Add(_newNeighbours[j]);
                }
            }
        }
        return null;
    }

    public Node GetRandomWalkableNode(Node _node) {
		RoomManager.Room _room = RoomManager.GetInstance().GetRoom(_node.RoomIndex);
		Int2 _randomNodeGridPos;
		Node _randomNode;

		do{
			_randomNodeGridPos = new Int2(Random.Range(0, GameGrid.SIZE.x), Random.Range(0, GameGrid.SIZE.y));
			_randomNode = TryGetNode(_randomNodeGridPos);
		} while (_randomNodeGridPos == _node.GridPos || _randomNode == null || !_randomNode.GetIsWalkable());
		
		// do{
		// 	_randomNodeGridPos = _room.NodeGridPositions[Random.Range(0, _room.NodeGridPositions.Count)];
		// 	_randomNode = TryGetNode(_randomNodeGridPos);
		// } while (_randomNodeGridPos == _node.GridPos || _randomNode == null || !_randomNode.GetIsWalkable());

		return _randomNode;
	}
    
    void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position, new Vector3(SIZE.x, SIZE.y, 1));
		if (nodeGrid == null){
			return;
		}

        if (DisplayGridGizmos) {
            foreach (Node _node in nodeGrid) {
                Gizmos.color = _node.GetIsWalkable() ? Color.white : Color.red;
                Gizmos.DrawWireCube(_node.WorldPos, Vector3.one * 0.1f);
            }
        }

		// for (int y = 0; y < SIZE.y; y++){
		// 	for (int x = 0; x < SIZE.x; x++){
		// 		Node _node = nodeGrid[x, y];
		// 		if (_node.ChemicalContent.Amount == 0){
		// 			continue;
		// 		}

		// 		GUIStyle _style = new GUIStyle();
		// 		_style.normal.textColor = Color.cyan;
		// 		_style.fontSize = 10;

		// 		string _text = _node.ChemicalContent.Amount.ToString();
		// 		UnityEditor.Handles.Label(_node.WorldPos, _text, _style);
		// 	}
		// }
    }

	public Node TryGetNode(Int2 _posGrid) {
		return TryGetNode(_posGrid.x, _posGrid.y);
	}

	public Node TryGetNode(int _posGridX, int _posGridY) {
		if (!IsInsideNodeGrid(_posGridX, _posGridY)) {
			return null;
		}

		return nodeGrid[_posGridX, _posGridY];
	}

	public void ScheduleUpdateForTile(Int2 _tileGridPos){
		meshBackground.ScheduleUpdateForTile(_tileGridPos);
		meshInteractivesBack.ScheduleUpdateForTile(_tileGridPos);
		meshInteractivesFront.ScheduleUpdateForTile(_tileGridPos);
	}
	
	public void ClearTemporaryColor(Int2 _tileGridPos){
		meshBackground.ClearTemporaryColor(_tileGridPos);
		meshInteractivesBack.ClearTemporaryColor(_tileGridPos);
		meshInteractivesFront.ClearTemporaryColor(_tileGridPos);
	}

	public void SetColor(Int2 _tileGridPos, byte _colorIndex, bool _isPermanent) {
		meshBackground.SetColor(_tileGridPos, _colorIndex, _isPermanent);
		meshInteractivesBack.SetColor(_tileGridPos, _colorIndex, _isPermanent);
		meshInteractivesFront.SetColor(_tileGridPos, _colorIndex, _isPermanent);
	}

	public void SetColor(Int2 _tileGridPos, byte[] _colorIndices, bool _isPermanent) {
		meshBackground.SetColor(_tileGridPos, _colorIndices, _isPermanent);
		meshInteractivesBack.SetColor(_tileGridPos, _colorIndices, _isPermanent);
		meshInteractivesFront.SetColor(_tileGridPos, _colorIndices, _isPermanent);
	}

	public void SetLighting(Int2 _tileGridPos, int _vertexIndex, Color32 _lighting, bool _setAverage = true) {
		meshBackground.SetLighting(_tileGridPos, _vertexIndex, _lighting, _setAverage);
		meshInteractivesBack.SetLighting(_tileGridPos, _vertexIndex, _lighting, _setAverage);
		meshInteractivesFront.SetLighting(_tileGridPos, _vertexIndex, _lighting, _setAverage);
	}

	public void SetChemicalAmount(Int2 _nodeGridPos, int _amount){
		nodeGrid[_nodeGridPos.x, _nodeGridPos.y].ChemicalContent.SetAmount(_amount);
		meshBackground.SetChemicalAmount(_nodeGridPos, _amount);
		meshInteractivesBack.SetChemicalAmount(_nodeGridPos, _amount);
		meshInteractivesFront.SetChemicalAmount(_nodeGridPos, _amount);
	}

	// public UVController TryGetTile(Int2 _posGrid) {
	// 	return TryGetTile(_posGrid.x, _posGrid.y);
	// }

	// public UVController TryGetTile(int _posGridX, int _posGridY) {
	// 	if (!IsInsideGrid(_posGridX, _posGridY)) {
	// 		return null;
	// 	}

	// 	return tileGrid[_posGridX, _posGridY];
	// }
}
