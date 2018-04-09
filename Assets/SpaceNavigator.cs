using System;
using UnityEngine;
public class SpaceNavigator {
	public enum SpaceEnum { VertexGrid, Grid, VertexLight, Light, VertexTile, World }
	public class Position {
		public Vector2i Pos;
		public bool IsUpToDate = false;
	}
	private Position vertexGrid 	= new Position();
	private Position grid 			= new Position();
	private Position vertexLight 	= new Position();
	private Position light 			= new Position();
	private Position vertexTile 	= new Position();

	private static Vector2i vertexGridSize;
	private static Vector2i vertexTileSize;
	private static Vector2i gridSize;
	private Vector2i lightSize = new Vector2i();
	private Vector2i vertexLightSize = new Vector2i();

	private CustomLight currentLight;

	public void PrintDebugLog() {
		// NOTE: this looks like shit because for some reason Unity can no longer handle newlines combined with vectors or something. I BARELY got this working -.-
		string _debugString = "";
		if (vertexGrid.IsUpToDate) _debugString += "vertexGrid: " + vertexGrid.Pos.x + ", " + vertexGrid.Pos.y;
		if (grid.IsUpToDate) _debugString += "\ngrid: " + grid.Pos.x + ", " + grid.Pos.y;
		if (vertexTile.IsUpToDate) _debugString += "\nvertexTile: " + vertexTile.Pos.x + ", " + vertexTile.Pos.y;
		if (vertexLight.IsUpToDate || light.IsUpToDate) _debugString += "\n \nCurrent light: " + currentLight.name;
		if (vertexLight.IsUpToDate) _debugString += "\nvertexLight: " + vertexLight.Pos.x + ", " + vertexLight.Pos.y;
		if (light.IsUpToDate) _debugString += "\nlight: " + light.Pos.x + ", " + light.Pos.y;
		SuperDebug.Log(Color.cyan, _debugString);
	}


	public static void SetupSizes() { 
		vertexGridSize = Grid.GridSize * UVControllerBasic.MESH_VERTICES_PER_EDGE;
		vertexTileSize = UVControllerBasic.MESH_VERTICES_PER_EDGE_AS_VECTOR;
		gridSize = Grid.GridSize;
	}
	public SpaceNavigator(Vector2i _vGridPos, CustomLight _light){
		SetVertexGridPos(_vGridPos);
		if(_light != null) SetLightSpace(_light);
	}
	public void SetLightSpace(CustomLight _light) {
		currentLight 		= _light;
		lightSize.x 		= _light.Diameter;
		lightSize.y 		= _light.Diameter;
		light.IsUpToDate = false;

		vertexLightSize.x 	= _light.Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE;
		vertexLightSize.y 	= _light.Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE;
		vertexLight.IsUpToDate = false;
	}

	private void SetVertexGridPos(Vector2i _newPos) {
		vertexGrid.Pos = _newPos;
		vertexGrid.IsUpToDate = true;
		grid.IsUpToDate 		= false;
		vertexLight.IsUpToDate 	= false;
		light.IsUpToDate 		= false;
		vertexTile.IsUpToDate 	= false;
	}
	private void UpdateSpace(Position _space, Vector2i _newPos) {
		_space.Pos = _newPos;
		_space.IsUpToDate = true;
	}

	public Vector2i GetVertexGridPos(){
		return vertexGrid.Pos;
	}
	public Vector2i GetGridPos(){
		if(!grid.IsUpToDate) UpdateSpace(grid, ConvertToGridSpace(vertexGrid.Pos));
		return grid.Pos;
	}
	public Vector2i GetVertexLightPos(){
		if (!vertexLight.IsUpToDate) UpdateSpace(vertexLight, ConvertToVertexLightSpace(vertexGrid.Pos, currentLight));
		return vertexLight.Pos;
	}
	public Vector2i GetLightPos(){
		if (!light.IsUpToDate) UpdateSpace(light, ConvertToLightSpace(vertexGrid.Pos, currentLight));
		return light.Pos;
	}
	public Vector2i GetVertexTilePos(){
		if (!vertexTile.IsUpToDate) UpdateSpace(vertexTile, ConvertToVertexTileSpace(vertexGrid.Pos));
		return vertexTile.Pos;
	}
	public Vector2 GetWorldPos(){
		return ConvertToWorldSpace(vertexGrid.Pos);
	}

	public Vector2i GetVertexGridAxisLengths()	{ return vertexGridSize; }
	public Vector2i GetGridAxisLengths()		{ return gridSize; }
	public Vector2i GetVertexLightAxisLengths()	{ return vertexLightSize; }
	public Vector2i GetLightAxisLengths()		{ return lightSize; }
	public Vector2i GetVertexTileAxisLengths()	{ return vertexTileSize; }
	public int GetVertexGridSize() 				{ return vertexGridSize.x * vertexGridSize.y; }
	public int GetGridSize() 					{ return gridSize.x * gridSize.y; }
	public int GetVertexLightSize() 			{ return vertexLightSize.x * vertexLightSize.y; }
	public int GetLightSize() 					{ return lightSize.x * lightSize.y; }
	public int GetVertexTileSize() 				{ return vertexTileSize.x * vertexTileSize.y; }

	public static Vector2i ConvertToVertexGridSpace(Vector2i _vLightPos, CustomLight _light){
		return _vLightPos + Vector2i.Scale(_light.MyGridCoord - _light.GetRadiusAsVector(), UVControllerBasic.MESH_VERTICES_PER_EDGE_AS_VECTOR);
	}
	public static Vector2i ConvertToVertexGridSpace(Vector2i _gGridPos, Vector2i _vTilePos){
		return _gGridPos * UVControllerBasic.MESH_VERTICES_PER_EDGE + _vTilePos;
	}
	public static Vector2i ConvertToGridSpace(Vector2i _vGridPos){
		return _vGridPos / UVControllerBasic.MESH_VERTICES_PER_EDGE;
	}
	public static Vector2i ConvertToVertexLightSpace(Vector2i _vGridPos, CustomLight _light){
		return _vGridPos - Vector2i.Scale(_light.MyGridCoord - _light.GetRadiusAsVector(), UVControllerBasic.MESH_VERTICES_PER_EDGE_AS_VECTOR);
	}
	public static Vector2i ConvertToLightSpace(Vector2i _vGridPos, CustomLight _light){
		return ConvertToGridSpace(_vGridPos) - (_light.MyGridCoord - _light.GetRadiusAsVector());
	}
	public static Vector2i ConvertToVertexTileSpace(Vector2i _vGridPos){ // WARNING: does not support top-half vertices! Confusing? Yes!
		return _vGridPos - ConvertToGridSpace(_vGridPos) * UVControllerBasic.MESH_VERTICES_PER_EDGE;
	}
	public static Vector2 ConvertToWorldSpace(Vector2i _vGridPos){
		Vector2i _gGridPos 	= ConvertToGridSpace(_vGridPos);
		Vector2 _localPos 	= new Vector2(_vGridPos.x * UVControllerBasic.MESH_VERTEX_SEPARATION, _vGridPos.y * UVControllerBasic.MESH_VERTEX_SEPARATION);
		Vector2 _correction = new Vector2(_gGridPos.x * UVControllerBasic.MESH_VERTEX_SEPARATION, _gGridPos.y * UVControllerBasic.MESH_VERTEX_SEPARATION); // discount every third vertex (except first) since they overlap
		Vector2 _gridWorldPos = Grid.Instance.transform.position;
		return _gridWorldPos - Grid.GridSizeHalf + _localPos - _correction;
	}

	public static void IterateOverGrid(Action<SpaceNavigator> _method) {
		SpaceNavigator _spaces = new SpaceNavigator(Vector2i.zero, null);
		Vector2i _axisLengths = _spaces.GetGridAxisLengths();

		_spaces.PrepareIncrementGridPos(0, 0);
		do { 
			_method(_spaces);
			_spaces.IncrementGridPos(0, _axisLengths.x - 1);
		}
		while (_spaces.GetGridPos().y < _axisLengths.y);
	}
	public static void IterateOverLightsTilesOnGrid(CustomLight _light, Action<SpaceNavigator> _method) {
		Vector2i _startVertexGridPos = ConvertToVertexGridSpace(Vector2i.zero, _light);
		_startVertexGridPos.x = Mathf.Clamp(_startVertexGridPos.x, 0, vertexGridSize.x);
		_startVertexGridPos.y = Mathf.Clamp(_startVertexGridPos.y, 0, vertexGridSize.y);
		Vector2i _startGridPos = ConvertToGridSpace(_startVertexGridPos);
		SpaceNavigator _spaces = new SpaceNavigator(_startVertexGridPos, _light);

		Vector2i _gridAxisLengths = _spaces.GetGridAxisLengths();
		Vector2i _lightAxisLengths = _spaces.GetLightAxisLengths();

		Vector2i _endGridPos = _startGridPos + _lightAxisLengths;
		_endGridPos.x = Mathf.Clamp(_endGridPos.x, 0, _gridAxisLengths.x);
		_endGridPos.y = Mathf.Clamp(_endGridPos.y, 0, _gridAxisLengths.y);

		_spaces.PrepareIncrementGridPos(_startGridPos.x, _startGridPos.y);

		Vector2i _gridPos = _spaces.GetGridPos();
		while (_gridPos.y < _endGridPos.y){
			if (_gridPos.x < _endGridPos.x) _method(_spaces);
			_spaces.IncrementGridPos(_minX: _startGridPos.x, _maxX: _endGridPos.x - 1);
			_gridPos = _spaces.GetGridPos();
		}
	}
	public static void IterateOverVertexGridAndSkipOverlaps(Action<SpaceNavigator> _method) {
		SpaceNavigator _spaces = new SpaceNavigator(Vector2i.zero, null);
		Vector2i _axisLengths = _spaces.GetVertexGridAxisLengths();

		_spaces.PrepareIncrementVertexGridPos(0, 0);

		Vector2i _vGridPos = _spaces.GetVertexGridPos();
		while (_vGridPos.y < _axisLengths.y){
			_method(_spaces);
			_spaces.IncrementVertexGridPos(_minX: 0, _maxX: _axisLengths.x - 1);
			_vGridPos = _spaces.GetVertexGridPos();
		}
	}
	public static void IterateOverLightsVerticesOnVGridAndSkipOverlaps(CustomLight _light, Action<SpaceNavigator> _method) {
		Vector2i _startPos = ConvertToVertexGridSpace(Vector2i.zero, _light);
		_startPos.x = Mathf.Clamp(_startPos.x, 0, vertexGridSize.x);
		_startPos.y = Mathf.Clamp(_startPos.y, 0, vertexGridSize.y);

		SpaceNavigator _spaces = new SpaceNavigator(_startPos, _light);
		Vector2i _axisLengths = _spaces.GetVertexLightAxisLengths();
		
		Vector2i _endPos = _startPos + _axisLengths;
		_endPos.x = Mathf.Clamp(_endPos.x, 0, vertexGridSize.x);
		_endPos.y = Mathf.Clamp(_endPos.y, 0, vertexGridSize.y);

		_spaces.PrepareIncrementVertexGridPos(_startPos.x, _startPos.y);

		Vector2i _vGridPos = _spaces.GetVertexGridPos();
		while (_vGridPos.y < _endPos.y){
			if (_vGridPos.x < _endPos.x) _method(_spaces);
			_spaces.IncrementVertexGridPos(_minX: _startPos.x, _maxX: _endPos.x - 1);
			_vGridPos = _spaces.GetVertexGridPos();
		}
	}

	public void PrepareIncrementVertexGridPos(int _minX, int _minY) {
		SetVertexGridPos(new Vector2i(_minX, _minY));
	}
	public void IncrementVertexGridPos(int _minX, int _maxX) {
		Vector2i _vTilePos = GetVertexTilePos();
		vertexGrid.Pos.x += _vTilePos.x == UVControllerBasic.MESH_VERTICES_PER_EDGE - 1 ? 2 : 1;
		if(vertexGrid.Pos.x > Mathf.Min(_maxX, vertexGridSize.x - 1)){
			vertexGrid.Pos.x = _minX;
			vertexGrid.Pos.y += _vTilePos.y == UVControllerBasic.MESH_VERTICES_PER_EDGE - 1 ? 2 : 1;
		}

		vertexGrid.IsUpToDate 	= true;

		grid.IsUpToDate 		= false;
		vertexLight.IsUpToDate 	= false;
		light.IsUpToDate 		= false;
		vertexTile.IsUpToDate 	= false;
	}
	public void PrepareIncrementGridPos(int _minX, int _minY) {
		UpdateSpace(grid, new Vector2i(_minX, _minY));
		UpdateSpace(vertexTile, UVControllerBasic.MESH_EDGE_MIDDLE_INDEX_AS_VECTOR);
		UpdateSpace(vertexGrid, ConvertToVertexGridSpace(grid.Pos, vertexTile.Pos));
	}
	public void IncrementGridPos(int _minX, int _maxX) {
		grid.Pos.x++;
		if(grid.Pos.x > Mathf.Min(_maxX, gridSize.x - 1)){
			grid.Pos.x = _minX;
			grid.Pos.y++;
		}

		vertexTile.Pos = UVControllerBasic.MESH_EDGE_MIDDLE_INDEX_AS_VECTOR;
		vertexGrid.Pos = ConvertToVertexGridSpace(grid.Pos, vertexTile.Pos);

		grid.IsUpToDate 		= true;
		vertexTile.IsUpToDate 	= true;
		vertexGrid.IsUpToDate 	= true;

		vertexLight.IsUpToDate 	= false;
		light.IsUpToDate 		= false;
	}
}
