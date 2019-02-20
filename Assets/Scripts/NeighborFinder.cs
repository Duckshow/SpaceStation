using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	public static void GetSurroundingTiles(Int2 _nodeGridPos, out Int2 _tileGridPosTL, out Int2 _tileGridPosTR, out Int2 _tileGridPosBR, out Int2 _tileGridPosBL) {
		_tileGridPosTL = new Int2(_nodeGridPos.x - 1, _nodeGridPos.y);
		_tileGridPosTR = new Int2(_nodeGridPos.x, _nodeGridPos.y);
		_tileGridPosBR = new Int2(_nodeGridPos.x, _nodeGridPos.y - 1);
		_tileGridPosBL = new Int2(_nodeGridPos.x - 1, _nodeGridPos.y - 1);

		if(!GameGrid.IsInsideNodeGrid(_tileGridPosTL)) { _tileGridPosTL = new Int2(-1, -1); }
		if(!GameGrid.IsInsideNodeGrid(_tileGridPosTR)) { _tileGridPosTR = new Int2(-1, -1); }
		if(!GameGrid.IsInsideNodeGrid(_tileGridPosBR)) { _tileGridPosBR = new Int2(-1, -1); }
		if(!GameGrid.IsInsideNodeGrid(_tileGridPosBL)) { _tileGridPosBL = new Int2(-1, -1); }
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
		_nodeT = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.Up);
		_nodeTR = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.UpRight);
		_nodeR = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.Right);
		_nodeBR = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.DownRight);
		_nodeB = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.Down);
		_nodeBL = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.DownLeft);
		_nodeL = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.Left);
	}
	
	public static void GetSurroundingNodes(Int2 _nodeGridPos, out Node[] _nodes) {
		_nodes = new Node[8];
		_nodes[0] = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.UpLeft);
		_nodes[1] = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.Up);
		_nodes[2] = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.UpRight);
		_nodes[3] = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.Right);
		_nodes[4] = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.DownRight);
		_nodes[5] = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.Down);
		_nodes[6] = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.DownLeft);
		_nodes[7] = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.Left);
	}
}
