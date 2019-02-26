using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildTool : Singleton<BuildTool> {

	public class ToolSettings {
		public virtual GameGrid.GridType GetGridType() {
			return GameGrid.GridType.None;
		}
		public virtual bool CanUseOnNode(Node _node) {
			return true;
		}
		public virtual void UseOnNode(Node _node, bool _isDeleting, bool _isPermanent) {
		}
	}

	public class ToolSettingsBuild : ToolSettings {
		public override GameGrid.GridType GetGridType(){
			return GameGrid.GridType.NodeGrid;
		}
		public override void UseOnNode(Node _node, bool _isDeleting, bool _isPermanent) {
			base.UseOnNode(_node, _isDeleting, _isPermanent);

			if (_isPermanent) {
				_node.TrySetIsWall(!_isDeleting);
			}
			else{
				_node.TrySetIsWallTemporary(!_isDeleting);
			}
		}
	}

	public class ToolSettingsColor : ToolSettings {
		public const int COLOR_CHANNEL_COUNT = 10;

		private byte[] colorChannelIndices = new byte[COLOR_CHANNEL_COUNT]{
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5,
			5 
		};

		public void SetColorIndex(int _channel, byte _value){
			colorChannelIndices[_channel] = _value;
		}

		public override GameGrid.GridType GetGridType(){
			return GameGrid.GridType.TileGrid;
		}

		public override void UseOnNode(Node _node, bool _isDeleting, bool _isPermanent){
			base.UseOnNode(_node, _isDeleting, _isPermanent);
			_node.SetColor(colorChannelIndices, _isPermanent);
		}
	}

	[System.Serializable]
	public abstract class ToolSettingsPlaceInteractives : ToolSettings {

		private Rotation currentRotation;

		public abstract InteractiveObjectAsset GetAsset();

		public override GameGrid.GridType GetGridType(){
			return GameGrid.GridType.NodeGrid;
		}

		public override bool CanUseOnNode(Node _node) {
			if (!_node.IsWall){
				return false;
			}

			NeighborFinder.TryCacheNeighbor(_node.GridPos, Direction.T);
			NeighborFinder.TryCacheNeighbor(_node.GridPos, Direction.B);
			Node _nodeT = NeighborFinder.CachedNeighbors[Direction.T];
			Node _nodeB = NeighborFinder.CachedNeighbors[Direction.B];
			if (_nodeT != null && _nodeB != null &&  _nodeT.IsWall && _nodeB.IsWall && _nodeT.AttachedInteractiveObject == null && _nodeB.AttachedInteractiveObject == null){
				currentRotation = Rotation.Left;
				return true;
			}

			NeighborFinder.TryCacheNeighbor(_node.GridPos, Direction.L);
			NeighborFinder.TryCacheNeighbor(_node.GridPos, Direction.R);
			Node _nodeL = NeighborFinder.CachedNeighbors[Direction.L];
			Node _nodeR = NeighborFinder.CachedNeighbors[Direction.R];
			if (_nodeL != null && _nodeR != null && _nodeL.IsWall && _nodeR.IsWall && _nodeL.AttachedInteractiveObject == null && _nodeR.AttachedInteractiveObject == null){
				currentRotation = Rotation.Down;
				return true;
			}

			return false;
		}

		public override void UseOnNode(Node _node, bool _isDeleting, bool _isPermanent) {
			base.UseOnNode(_node, _isDeleting, _isPermanent);


			if (_isPermanent) {
				_node.TrySetInteractiveObject(_isDeleting ? null : GetAsset(), currentRotation);
			}
			else{
				_node.TrySetInteractiveObjectTemporary(GetAsset(), currentRotation);
			}
		}
	}

	[System.Serializable]
	public class ToolSettingsPlaceDoors : ToolSettingsPlaceInteractives {

		public InteractiveObjectAsset Door;
		public override InteractiveObjectAsset GetAsset() { return Door; }
	}

	[System.Serializable]
	public class ToolSettingsPlaceAirlocks : ToolSettingsPlaceInteractives {

		public InteractiveObjectAsset Airlock;
		public override InteractiveObjectAsset GetAsset() { return Airlock; }
	}

	public ToolSettingsBuild Building = new ToolSettingsBuild();
	public ToolSettingsColor Coloring = new ToolSettingsColor();
	public ToolSettingsPlaceDoors PlaceDoors = new ToolSettingsPlaceDoors();
	public ToolSettingsPlaceAirlocks PlaceAirlocks = new ToolSettingsPlaceAirlocks();

	public enum ToolMode { None, Build, Color, PlaceDoors, PlaceAirlocks }
	private ToolMode currentToolMode = ToolMode.Build;

	public ToolMode GetCurrentToolMode() {
		return currentToolMode;
	}

	public void SetCurrentToolMode(ToolMode _newMode){
		currentToolMode = _newMode;
	}

	private ToolSettings GetCurrentToolSettings() { 
		switch (currentToolMode){
			case ToolMode.None:
				return null;
			case ToolMode.Build:
				return Building;
			case ToolMode.Color:
				return Coloring;
			case ToolMode.PlaceDoors:
				return PlaceDoors;
			case ToolMode.PlaceAirlocks:
				return PlaceAirlocks;
			default:
				Debug.LogError(currentToolMode + " hasn't been properly implemented yet!");
				return null;
		}
	}

	private enum ShapeModeEnum { None, Room, Wall, Fill }
	private ShapeModeEnum currentShapeMode = ShapeModeEnum.None;

	private Int2 nodeGridPosStart;
	private Int2 nodeGridPosEnd;
	private List<Int2> affectedNodeGridPositions = new List<Int2>();


	public override bool IsUsingUpdateDefault() { return true; }
	public override void UpdateDefault() {
		if (currentToolMode == ToolMode.None){
			return;
		}

		GameGrid.GridType _gridType = GetCurrentToolSettings().GetGridType();
		if (!Mouse.GetInstance().HasMovedOnGrid(_gridType) && !Mouse.GetInstance().DoesEitherButtonEqual(Mouse.StateEnum.Click) && !Mouse.GetInstance().DoesEitherButtonEqual(Mouse.StateEnum.Release)){
			return;
		}

		for (int i = affectedNodeGridPositions.Count - 1; i >= 0; i--){
			Node _node = GameGrid.GetInstance().TryGetNode(affectedNodeGridPositions[i]);
			affectedNodeGridPositions.RemoveAt(i);
			if (_node == null){
				continue;
			}

			_node.TryClearIsWallTemporary();
			_node.ClearTemporaryColor();
			_node.TryClearInteractiveObjectTemporary();
		}
	}

	public override bool IsUsingUpdateLate(){ return true; }
	public override void UpdateLate() {
		if (currentToolMode == ToolMode.None){
			return;
		}

		bool _isDeleting = false;
		bool _isPermanent = false;

		currentShapeMode = ShapeModeEnum.Fill;

		Mouse.StateEnum mouseState = Mouse.GetInstance().GetStateLMB();
		if (mouseState == Mouse.StateEnum.Idle){
			mouseState = Mouse.GetInstance().GetStateRMB();
			_isDeleting = mouseState != Mouse.StateEnum.Idle;
		}

		GameGrid.GridType _gridType = GetCurrentToolSettings().GetGridType();

		if (!Mouse.GetInstance().HasMovedOnGrid(_gridType) && (mouseState == Mouse.StateEnum.Idle || mouseState == Mouse.StateEnum.Hold)){
			return;
		}

		switch (mouseState){
			case Mouse.StateEnum.Idle:
				nodeGridPosStart = Mouse.GetInstance().GetGridPos(_gridType);
				DrawOrPreviewOnNode(nodeGridPosStart, _isDeleting: false, _isPermanent: false);
				return;
			case Mouse.StateEnum.Click:
			case Mouse.StateEnum.Hold:
				_isPermanent = false;
				nodeGridPosEnd = Mouse.GetInstance().GetGridPos(_gridType);
				break;
			case Mouse.StateEnum.Release:
				_isPermanent = true;
				break;
			default:
				Debug.LogError(mouseState + " hasn't been properly implemented yet!");
				break;
		}

		switch (currentShapeMode){
			case ShapeModeEnum.None:
				break;
			case ShapeModeEnum.Wall:
				TryDrawWall(nodeGridPosStart, nodeGridPosEnd, _isDeleting, _isPermanent);
				break;
			case ShapeModeEnum.Room:
				TryDrawRoom(nodeGridPosStart, nodeGridPosEnd, false, _isDeleting, _isPermanent);
				break;
			case ShapeModeEnum.Fill:
				TryDrawRoom(nodeGridPosStart, nodeGridPosEnd, true, _isDeleting, _isPermanent);
				break;
			default:
				Debug.LogError(currentShapeMode + " hasn't been properly implemented yet!");
				break;
		}
	}

	void TryDrawWall(Int2 _nodeGridPosStart, Int2 _nodeGridPosEnd, bool _isDeleting, bool _isPermanent) {
		Int2 _nodeGridPosWallStart = new Int2(
			Mathf.Min(_nodeGridPosStart.x, _nodeGridPosEnd.x),
			Mathf.Min(_nodeGridPosStart.y, _nodeGridPosEnd.y)
		);

		int _wallLengthX = Mathf.Abs(_nodeGridPosEnd.x - _nodeGridPosStart.x);
		int _wallLengthY = Mathf.Abs(_nodeGridPosEnd.y - _nodeGridPosStart.y);

		if (_wallLengthX > _wallLengthY) { 
			_nodeGridPosWallStart.y = _nodeGridPosStart.y;
			_wallLengthY = 0;
		}
		else { 
			_nodeGridPosWallStart.x = _nodeGridPosStart.x;
			_wallLengthX = 0;
		}

		for (int _y = 0; _y <= _wallLengthY; _y++){
			for (int _x = 0; _x <= _wallLengthX; _x++){
				Int2 _nodeGridPosCurrent = _nodeGridPosWallStart + new Int2(_x, _y);

				Node _node = GameGrid.GetInstance().TryGetNode(_nodeGridPosCurrent);
				if (_node == null){
					break;
				}

				DrawOrPreviewOnNode(_nodeGridPosCurrent, _isDeleting, _isPermanent);
			}
		}
	}

	void DrawOrPreviewOnNode(Int2 _nodeGridPos, bool _isDeleting, bool _isPermanent) {
		ToolSettings _tool = GetCurrentToolSettings();
		Node _node = GameGrid.GetInstance().TryGetNode(_nodeGridPos);
		if (!_tool.CanUseOnNode(_node)){
			return;
		}

		affectedNodeGridPositions.Add(_nodeGridPos);
		GetCurrentToolSettings().UseOnNode(_node, _isDeleting, _isPermanent);
	}

	void TryDrawRoom(Int2 _nodeGridPosStart, Int2 _nodeGridPosEnd, bool _isFilledRoom, bool _isDeleting, bool _isTemporary) {
		Int2 _nodeGridPosBL = new Int2(
			Mathf.Min(_nodeGridPosStart.x, _nodeGridPosEnd.x),
			Mathf.Min(_nodeGridPosStart.y, _nodeGridPosEnd.y)
		);

		Int2 _nodeGridPosTR = new Int2(
			Mathf.Max(_nodeGridPosStart.x, _nodeGridPosEnd.x),
			Mathf.Max(_nodeGridPosStart.y, _nodeGridPosEnd.y)
		);

		Int2 _roomSize = new Int2(
			_nodeGridPosTR.x - _nodeGridPosBL.x,
			_nodeGridPosTR.y - _nodeGridPosBL.y
		);

		for (int _y = 0; _y <= _roomSize.y; _y++){
			for (int _x = 0; _x <= _roomSize.x; _x++){
				Int2 _nodeGridPosCurrent = _nodeGridPosBL + new Int2(_x, _y);

				Node _node = GameGrid.GetInstance().TryGetNode(_nodeGridPosCurrent);
				if (_node == null){
					break;
				}

				if (!_isFilledRoom && _y != 0 && _y != _roomSize.y && _x != 0 && _x != _roomSize.x){
					continue;
				}

				DrawOrPreviewOnNode(_nodeGridPosCurrent, _isDeleting, _isTemporary);
			}
		}
	}
}
