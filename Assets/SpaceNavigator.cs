// using System;
// using UnityEngine;
// public class SpaceNavigator {
// 	public enum SpaceEnum { VertexGrid, Grid, VertexLight, Light, VertexTile, World }
// 	public class Position {
// 		public Vector2i Pos;
// 		public Vector2i TranslationKey;
// 	}
// 	private Position vertexGrid 	= new Position();
// 	private Position vertexTile 	= new Position();
// 	private Position vertexMap 		= new Position();
// 	private Position grid 			= new Position();
// 	private Position light 			= new Position();
// 	private Position vertexLight 	= new Position();

// 	private static Vector2i vertexGridSize;
// 	private static Vector2i vertexTileSize;
// 	private static Vector2i vertexMapSize;
// 	private static Vector2i gridSize;
// 	private Vector2i lightSize = new Vector2i();
// 	private Vector2i vertexLightSize = new Vector2i();

// 	private static Vector2i[,] VGridToVMapConversionTable;
// 	private static Vector2i[,] VMapToVGridConversionTable;


// 	public void PrintDebugLog() {
// 		// NOTE: this looks like shit because for some reason Unity can no longer handle newlines combined with vectors or something. I BARELY got this working -.-
// 		string _debugString = "";
// 		if (vertexGrid.TranslationKey == vertexGrid.Pos) _debugString += "vertexGrid: " + vertexGrid.Pos.x + ", " + vertexGrid.Pos.y;
// 		if (grid.TranslationKey == vertexGrid.Pos) _debugString += "\ngrid: " + grid.Pos.x + ", " + grid.Pos.y;
// 		if (vertexTile.TranslationKey == vertexGrid.Pos) _debugString += "\nvertexTile: " + vertexTile.Pos.x + ", " + vertexTile.Pos.y;
// 		if (vertexMap.TranslationKey == vertexGrid.Pos) _debugString += "\nvertexMap: " + vertexMap.Pos.x + ", " + vertexMap.Pos.y;
// 		if (vertexLight.TranslationKey == vertexGrid.Pos || light.TranslationKey == vertexGrid.Pos) _debugString += "\n \nCurrent light: " + currentLight.name;
// 		if (vertexLight.TranslationKey == vertexGrid.Pos) _debugString += "\nvertexLight: " + vertexLight.Pos.x + ", " + vertexLight.Pos.y;
// 		if (light.TranslationKey == vertexGrid.Pos) _debugString += "\nlight: " + light.Pos.x + ", " + light.Pos.y;
// 		SuperDebug.Log(Color.red, _debugString);
// 	}

// 	public static void SetupSizes() { 
// 		vertexGridSize = Grid.GridSize * UVControllerBasic.MESH_VERTICES_PER_EDGE;
// 		vertexTileSize = UVControllerBasic.MESH_VERTICES_PER_EDGE_AS_VECTOR;
// 		vertexMapSize = ConvertToVertexMapSpace(vertexGridSize, _useConversionTable: false);
// 		gridSize = Grid.GridSize;
// 	}
// 	public static void SetupConversionTables(){
// 		// grid -> map
// 		VGridToVMapConversionTable = new Vector2i[vertexGridSize.x, vertexGridSize.y];
// 		for (int y = 0; y < vertexGridSize.y; y++){
// 			for (int x = 0; x < vertexGridSize.x; x++){
// 				VGridToVMapConversionTable[x, y] = ConvertToVertexMapSpace(new Vector2i(x, y), _useConversionTable: false);
// 			}
// 		}
		
// 		// map -> grid
// 		VMapToVGridConversionTable = new Vector2i[vertexMapSize.x, vertexMapSize.y];
// 		for (int y = 0; y < vertexMapSize.y; y++){
// 			for (int x = 0; x < vertexMapSize.x; x++){
// 				VMapToVGridConversionTable[x, y] = ConvertToVertexGridSpace(new Vector2i(x, y), _isSettingUpConversionTable: true);
// 			}
// 		}
// 	}

// 	public SpaceNavigator(Vector2i _vGridPos, CustomLight _light){
// 		SetVertexGridPos(_vGridPos);
// 		if(_light != null) SetLightSpace(_light);
// 	}
// 	public void SetLightSpace(CustomLight _light) {
// 		currentLight 		= _light;
// 		lightSize.x 		= _light.Diameter;
// 		lightSize.y 		= _light.Diameter;
// 		light.TranslationKey = -Vector2i.one;

// 		vertexLightSize.x 	= _light.Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE;
// 		vertexLightSize.y 	= _light.Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE;
// 		vertexLight.TranslationKey = -Vector2i.one;
// 	}

// 	public void SetVertexGridPos(Vector2i _newPos) {
// 		vertexGrid.Pos = _newPos;
// 	}
// 	private void UpdateSpace(Position _space, Vector2i _translationKey, Vector2i _newPos) {
// 		_space.Pos = _newPos;
// 		_space.TranslationKey = _translationKey;
// 	}

// 	public Vector2i GetVertexGridPos(){
// 		return vertexGrid.Pos;
// 	}
// 	public Vector2i GetGridPos(){
// 		if(grid.TranslationKey != vertexGrid.Pos) UpdateSpace(grid, vertexGrid.Pos, ConvertToGridSpace(vertexGrid.Pos));
// 		return grid.Pos;
// 	}
// 	public Vector2i GetVertexLightPos(){
// 		if (vertexLight.TranslationKey != vertexGrid.Pos) UpdateSpace(vertexLight, vertexGrid.Pos, ConvertToVertexLightSpace(vertexGrid.Pos, currentLight));
// 		return vertexLight.Pos;
// 	}
// 	public Vector2i GetLightPos(){
// 		if (light.TranslationKey != vertexGrid.Pos) UpdateSpace(light, vertexGrid.Pos, ConvertToLightSpace(vertexGrid.Pos, currentLight));
// 		return light.Pos;
// 	}
// 	public Vector2i GetVertexTilePos(){
// 		if (vertexTile.TranslationKey != vertexGrid.Pos) UpdateSpace(vertexTile, vertexGrid.Pos, ConvertToVertexTileSpace(vertexGrid.Pos));
// 		return vertexTile.Pos;
// 	}
// 	public Vector2i GetVertexMapPos(){
// 		if (vertexMap.TranslationKey != vertexGrid.Pos) UpdateSpace(vertexMap, vertexGrid.Pos, ConvertToVertexMapSpace(vertexGrid.Pos));
// 		return vertexMap.Pos;
// 	}
// 	public Vector2 GetWorldPos(){
// 		return ConvertToWorldSpace(vertexGrid.Pos);
// 	}

// 	public static void GetVertexLightPosFirstAndLast(CustomLight _light, out Vector2i _first, out Vector2i _last) {
// 		_first = SpaceNavigator.ConvertToVertexGridSpace(Vector2i.zero, _light);
// 		_first.x = Mathf.Clamp(_first.x, 0, vertexGridSize.x - 1);
// 		_first.y = Mathf.Clamp(_first.y, 0, vertexGridSize.y - 1);

// 		_last = _first + GetVertexLightSize(_light) - Vector2i.one;
// 		_last.x = Mathf.Clamp(_last.x, 0, vertexGridSize.x - 1);
// 		_last.y = Mathf.Clamp(_last.y, 0, vertexGridSize.y - 1);
// 	}

// 	public static Vector2i GetVertexLightSize(CustomLight _light) { return _light.Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE_AS_VECTOR; }
// 	public static Vector2i GetVertexGridSize()	{ return vertexGridSize; }
// 	public static Vector2i GetGridSize()		{ return gridSize; }
// 	public static Vector2i GetVertexTileSize()	{ return vertexTileSize; }
// 	public static Vector2i GetVertexMapSize() 	{ return vertexMapSize; }
// 	public Vector2i GetVertexLightSize()		{ return vertexLightSize; }
// 	public Vector2i GetLightSize()				{ return lightSize; }
// 	public static int GetVertexGridTotalSize() 	{ return vertexGridSize.x * vertexGridSize.y; }
// 	public static int GetGridTotalSize() 		{ return gridSize.x * gridSize.y; }
// 	public static int GetVertexTileTotalSize() 	{ return vertexTileSize.x * vertexTileSize.y; }
// 	public static int GetVertexMapTotalSize() 	{ return vertexMapSize.x * vertexMapSize.y; }
// 	public int GetVertexLightTotalSize() 		{ return vertexLightSize.x * vertexLightSize.y; }
// 	public int GetLightTotalSize() 				{ return lightSize.x * lightSize.y; }

// 	public static Vector2i ConvertToVertexGridSpace(Vector2i _vLightPos, CustomLight _light){
// 		return _vLightPos + Vector2i.Scale(_light.MyGridCoord - _light.GetRadiusAsVector(), UVControllerBasic.MESH_VERTICES_PER_EDGE_AS_VECTOR);
// 	}
// 	public static Vector2i ConvertToVertexGridSpace(Vector2i _gGridPos, Vector2i _vTilePos){
// 		return _gGridPos * UVControllerBasic.MESH_VERTICES_PER_EDGE + _vTilePos;
// 	}
// 	public static Vector2i ConvertToVertexGridSpace(Vector2i _mGridPos, bool _isSettingUpConversionTable = false) {
// 		if (_isSettingUpConversionTable){
// 			_mGridPos.x += Mathf.FloorToInt((_mGridPos.x - 1) / (UVControllerBasic.MESH_VERTICES_PER_EDGE - 1));
// 			_mGridPos.y += Mathf.FloorToInt((_mGridPos.y - 1) / (UVControllerBasic.MESH_VERTICES_PER_EDGE - 1));
// 		}
// 		else{
// 			_mGridPos = VMapToVGridConversionTable[_mGridPos.x, _mGridPos.y];
// 		}

// 		return _mGridPos;
// 	}
// 	public static Vector2i ConvertToGridSpace(Vector2i _vGridPos){
// 		return _vGridPos / UVControllerBasic.MESH_VERTICES_PER_EDGE;
// 	}
// 	public static Vector2i ConvertToVertexLightSpace(Vector2i _vGridPos, CustomLight _light){
// 		return _vGridPos - Vector2i.Scale(_light.MyGridCoord - _light.GetRadiusAsVector(), UVControllerBasic.MESH_VERTICES_PER_EDGE_AS_VECTOR);
// 	}
// 	public static Vector2i ConvertToLightSpace(Vector2i _vGridPos, CustomLight _light){
// 		return ConvertToGridSpace(_vGridPos) - (_light.MyGridCoord - _light.GetRadiusAsVector());
// 	}
// 	public static Vector2i ConvertToVertexTileSpace(Vector2i _vGridPos){ // WARNING: does not support top-half vertices! Confusing? Yes!
// 		return _vGridPos - ConvertToGridSpace(_vGridPos) * UVControllerBasic.MESH_VERTICES_PER_EDGE;
// 	}
// 	public static Vector2i ConvertToVertexMapSpace(Vector2i _vGridPos, bool _useConversionTable = true){
// 		if (_useConversionTable){
// 			return VGridToVMapConversionTable[_vGridPos.x, _vGridPos.y];
// 		}
// 		else{
// 			_vGridPos.x -= Mathf.FloorToInt(_vGridPos.x / UVControllerBasic.MESH_VERTICES_PER_EDGE);
// 			_vGridPos.y -= Mathf.FloorToInt(_vGridPos.y / UVControllerBasic.MESH_VERTICES_PER_EDGE);
// 			return _vGridPos;
// 		}
// 	}
// 	public static Vector2i[] GetVertexGridPosForTileVertices(Vector2i _gridPos){
// 		Vector2i[] _vertices = new Vector2i[UVControllerBasic.MESH_VERTICES_PER_EDGE * UVControllerBasic.MESH_VERTICES_PER_EDGE];

// 		int x = 0, y = 0;
// 		for (int i = 0; i < _vertices.Length; i++){
// 			if (i > 0){
// 				x++;
// 				if (x > UVControllerBasic.MESH_VERTICES_PER_EDGE){
// 					x = 0;
// 					y++;
// 				}
// 			}
			
// 			_vertices[i] = ConvertToVertexGridSpace(_gridPos, new Vector2i(x, y));
// 		} 

// 		return _vertices;
// 	}
// 	public static Vector2 ConvertToWorldSpace(Vector2i _vGridPos){
// 		Vector2i _gGridPos 	= ConvertToGridSpace(_vGridPos);
// 		Vector2 _localPos 	= new Vector2(_vGridPos.x * UVControllerBasic.MESH_VERTEX_SEPARATION, _vGridPos.y * UVControllerBasic.MESH_VERTEX_SEPARATION);
// 		Vector2 _correction = new Vector2(_gGridPos.x * UVControllerBasic.MESH_VERTEX_SEPARATION, _gGridPos.y * UVControllerBasic.MESH_VERTEX_SEPARATION); // discount every third vertex (except first) since they overlap
// 		Vector2 _gridWorldPos = Grid.Instance.transform.position;
// 		return _gridWorldPos - Grid.GridSizeHalf + _localPos - _correction;
// 	}

// 	public static void IterateOverGrid(Action<SpaceNavigator> _method) {
// 		SpaceNavigator _spaces = new SpaceNavigator(Vector2i.zero, null);

// 		_spaces.PrepareIncrementGridPos(0, 0);
// 		do { 
// 			_method(_spaces);
// 			_spaces.IncrementGridPos(0, gridSize.x - 1);
// 		}
// 		while (_spaces.GetGridPos().y < gridSize.y);
// 	}
// 	public static void IterateOverLightsTilesOnGrid(CustomLight _light, Action<SpaceNavigator> _method) {
// 		Vector2i _startVertexGridPos = ConvertToVertexGridSpace(Vector2i.zero, _light);
// 		_startVertexGridPos.x = Mathf.Clamp(_startVertexGridPos.x, 0, vertexGridSize.x);
// 		_startVertexGridPos.y = Mathf.Clamp(_startVertexGridPos.y, 0, vertexGridSize.y);
// 		Vector2i _startGridPos = ConvertToGridSpace(_startVertexGridPos);
// 		SpaceNavigator _spaces = new SpaceNavigator(_startVertexGridPos, _light);

// 		Vector2i _endGridPos = _startGridPos + _spaces.GetLightSize();
// 		_endGridPos.x = Mathf.Clamp(_endGridPos.x, 0, gridSize.x);
// 		_endGridPos.y = Mathf.Clamp(_endGridPos.y, 0, gridSize.y);

// 		_spaces.PrepareIncrementGridPos(_startGridPos.x, _startGridPos.y);

// 		Vector2i _gridPos = _spaces.GetGridPos();
// 		while (_gridPos.y < _endGridPos.y){
// 			if (_gridPos.x < _endGridPos.x) _method(_spaces);
// 			_spaces.IncrementGridPos(_minX: _startGridPos.x, _maxX: _endGridPos.x - 1);
// 			_gridPos = _spaces.GetGridPos();
// 		}
// 	}
// 	public static void IterateOverVertexGridAndSkipOverlaps(Action<SpaceNavigator> _method) {
// 		SpaceNavigator _spaces = new SpaceNavigator(Vector2i.zero, null);
// 		_spaces.PrepareIncrementVertexGridPos(0, 0);

// 		Vector2i _vGridPos = _spaces.GetVertexGridPos();
// 		while (_vGridPos.y < vertexGridSize.y){
// 			_method(_spaces);
// 			_spaces.IncrementVertexGridPos(_minX: 0, _maxX: vertexGridSize.x - 1);
// 			_vGridPos = _spaces.GetVertexGridPos();
// 		}
// 	}
// 	public static void IterateOverLightsVerticesOnVGridAndSkipOverlaps(CustomLight _light, Action<SpaceNavigator> _method) {
// 		Vector2i _startPos = ConvertToVertexGridSpace(Vector2i.zero, _light);
// 		_startPos.x = Mathf.Clamp(_startPos.x, 0, vertexGridSize.x);
// 		_startPos.y = Mathf.Clamp(_startPos.y, 0, vertexGridSize.y);

// 		SpaceNavigator _spaces = new SpaceNavigator(_startPos, _light);
// 		Vector2i _axisLengths = _spaces.GetVertexLightSize();
		
// 		Vector2i _endPos = _startPos + _axisLengths;
// 		_endPos.x = Mathf.Clamp(_endPos.x, 0, vertexGridSize.x);
// 		_endPos.y = Mathf.Clamp(_endPos.y, 0, vertexGridSize.y);

// 		_spaces.PrepareIncrementVertexGridPos(_startPos.x, _startPos.y);

// 		Vector2i _vGridPos = _spaces.GetVertexGridPos();
// 		while (_vGridPos.y < _endPos.y){
// 			if (_vGridPos.x < _endPos.x) _method(_spaces);
// 			_spaces.IncrementVertexGridPos(_minX: _startPos.x, _maxX: _endPos.x - 1);
// 			_vGridPos = _spaces.GetVertexGridPos();
// 		}
// 	}
// 	public static void IterateOverVertexMap(Action<SpaceNavigator> _method) {
// 		Vector2i _vMapPos = Vector2i.zero;
// 		SpaceNavigator _spaces = new SpaceNavigator(Vector2i.zero, null);
// 		_spaces.PrepareIncrementVertexMapPos(_vMapPos);

// 		while (_vMapPos.y < vertexMapSize.y){
// 			if (_vMapPos.x < vertexMapSize.x) _method(_spaces);
// 			_spaces.IncrementVertexMapPos(_minX: 0, _maxX: vertexMapSize.x - 1);
// 			_vMapPos = _spaces.GetVertexMapPos();
// 		}
// 	}
// 	public static void IterateOverLightsVerticesOnVertexMap(CustomLight _light, Action<SpaceNavigator> _method) {
// 		Vector2i _vGridPosFirst;
// 		Vector2i _vGridPosLast;
// 		GetVertexLightPosFirstAndLast(_light, out _vGridPosFirst, out _vGridPosLast);
// 		Vector2i _vMapPosFirst = ConvertToVertexMapSpace(_vGridPosFirst);
// 		Vector2i _vMapPosLast = ConvertToVertexMapSpace(_vGridPosLast);
// 		Vector2i _vMapPos = _vMapPosFirst;

// 		SpaceNavigator _spaces = new SpaceNavigator(Vector2i.zero, _light);
// 		_spaces.PrepareIncrementVertexMapPos(_vMapPosFirst);

// 		while(true){
// 			Debug.Log(_vMapPos + ", " + _vMapPosFirst + ", " + _vMapPosLast);
// 			if (_vMapPos.x <= _vMapPosLast.x) _method(_spaces);
// 			if (_vMapPos == _vMapPosLast) break;
// 			break;

// 			_spaces.IncrementVertexMapPos(_minX: _vMapPosFirst.x, _maxX: _vMapPosLast.x);
// 			_vMapPos = _spaces.GetVertexMapPos();
// 		} 
// 	}

// 	public void PrepareIncrementVertexGridPos(int _minX, int _minY) {
// 		SetVertexGridPos(new Vector2i(_minX, _minY));
// 	}
// 	public void IncrementVertexGridPos(int _minX, int _maxX) {
// 		Vector2i _vTilePos = GetVertexTilePos();
// 		vertexGrid.Pos.x += _vTilePos.x == UVControllerBasic.MESH_VERTICES_PER_EDGE - 1 ? 2 : 1;
// 		if(vertexGrid.Pos.x > Mathf.Min(_maxX, vertexGridSize.x - 1)){
// 			vertexGrid.Pos.x = _minX;
// 			vertexGrid.Pos.y += _vTilePos.y == UVControllerBasic.MESH_VERTICES_PER_EDGE - 1 ? 2 : 1;
// 		}
// 	}
// 	public void PrepareIncrementGridPos(int _minX, int _minY) {
// 		Vector2i _grid = new Vector2i(_minX, _minY);
// 		Vector2i _vTile = UVControllerBasic.MESH_EDGE_MIDDLE_INDEX_AS_VECTOR;
// 		Vector2i _vGrid = ConvertToVertexGridSpace(_grid, _vTile);
// 		UpdateSpace(vertexGrid, _vGrid, _vGrid);
// 		UpdateSpace(grid, _vGrid, _grid);
// 		UpdateSpace(vertexTile, _vGrid, _vTile);
// 	}
// 	public void IncrementGridPos(int _minX, int _maxX) {
// 		grid.Pos.x++;
// 		if(grid.Pos.x > Mathf.Min(_maxX, gridSize.x - 1)){
// 			grid.Pos.x = _minX;
// 			grid.Pos.y++;
// 		}

// 		vertexTile.Pos = UVControllerBasic.MESH_EDGE_MIDDLE_INDEX_AS_VECTOR;
// 		vertexGrid.Pos = ConvertToVertexGridSpace(grid.Pos, vertexTile.Pos);
// 	}
// 	public void PrepareIncrementVertexMapPos(Vector2i _min) {
// 		Vector2i _vGridPos = ConvertToVertexGridSpace(_min, _isSettingUpConversionTable: false);
// 		_vGridPos += Vector2i.one; // WARNING: this assumes _min corresponds to a _vTilePos of 0, 0. I can't figure out a better solution.
// 		UpdateSpace(vertexGrid, _vGridPos, _vGridPos);
// 		UpdateSpace(vertexMap, _vGridPos, _min);
// 	}
// 	public void IncrementVertexMapPos(int _minX, int _maxX) {
// 		vertexMap.Pos.x++;
// 		if(vertexMap.Pos.x > Mathf.Min(_maxX, vertexMapSize.x - 1)){
// 			vertexMap.Pos.x = _minX;
// 			vertexMap.Pos.y++;
// 		}

// 		if (vertexMap.Pos.y < vertexMapSize.y){
// 			vertexGrid.Pos = ConvertToVertexGridSpace(vertexMap.Pos, _isSettingUpConversionTable: false);
// 		}
// 	}
// }
