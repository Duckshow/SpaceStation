using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NeighborFinder {
	public static Dictionary<Direction, Node> CachedNeighbors = new Dictionary<Direction, Node>() { 
		{ Direction.None, null },
		{ Direction.TL, null },
		{ Direction.T, null },
		{ Direction.TR, null },
		{ Direction.R, null },
		{ Direction.BR, null },
		{ Direction.B, null },
		{ Direction.L, null }
	};

	public static bool TryCacheNeighbor(Int2 _nodeGridPos, Direction _neighbor){
		switch (_neighbor){
			case Direction.All:
				TryCacheNeighbor(_nodeGridPos, Direction.TL);
				TryCacheNeighbor(_nodeGridPos, Direction.T);
				TryCacheNeighbor(_nodeGridPos, Direction.TR);
				TryCacheNeighbor(_nodeGridPos, Direction.R);
				TryCacheNeighbor(_nodeGridPos, Direction.BR);
				TryCacheNeighbor(_nodeGridPos, Direction.B);
				TryCacheNeighbor(_nodeGridPos, Direction.BL);
				TryCacheNeighbor(_nodeGridPos, Direction.L);
				return true;
			case Direction.None:
				_nodeGridPos = Int2.Zero;
				break;
			case Direction.TL:
				_nodeGridPos += Int2.UpLeft;
				break;
			case Direction.T:
				_nodeGridPos += Int2.Up;
				break;
			case Direction.TR:
				_nodeGridPos += Int2.UpRight;
				break;
			case Direction.R:
				_nodeGridPos += Int2.Right;
				break;
			case Direction.BR:
				_nodeGridPos += Int2.DownRight;
				break;
			case Direction.B:
				_nodeGridPos += Int2.Down;
				break;
			case Direction.BL:
				_nodeGridPos += Int2.DownLeft;
				break;
			case Direction.L:
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
		TryCacheNeighbor(_nodeGridPos, Direction.T);
		TryCacheNeighbor(_nodeGridPos, Direction.B);
		TryCacheNeighbor(_nodeGridPos, Direction.L);
		TryCacheNeighbor(_nodeGridPos, Direction.R);
		bool _isWallT = CachedNeighbors[Direction.T].IsWall;
		bool _isWallB = CachedNeighbors[Direction.B].IsWall;
		bool _isWallL = CachedNeighbors[Direction.L].IsWall;
		bool _isWallR = CachedNeighbors[Direction.R].IsWall;
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
		_nodeT 	= GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.Up);
		_nodeTR = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.UpRight);
		_nodeR 	= GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.Right);
		_nodeBR = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.DownRight);
		_nodeB 	= GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.Down);
		_nodeBL = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.DownLeft);
		_nodeL 	= GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.Left);
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

	public static Direction GetDirectionMirrored(Direction _dir) {
		switch (_dir){
			case Direction.TL:	return Direction.BR;
			case Direction.T:	return Direction.B;
			case Direction.TR:	return Direction.BL;
			case Direction.R:	return Direction.L;
			case Direction.BR:	return Direction.TL;
			case Direction.B:	return Direction.T;
			case Direction.BL:	return Direction.TR;
			case Direction.L:	return Direction.R;
			default:			return Direction.None;
		}
	}
	
	public static Direction GetDirectionMirroredInX(Direction _dir) {
		switch (_dir){
			case Direction.TL: return Direction.TR;
			case Direction.T:  return Direction.T;
			case Direction.TR: return Direction.TL;
			case Direction.R:  return Direction.L;
			case Direction.BR: return Direction.BL;
			case Direction.B:  return Direction.B;
			case Direction.BL: return Direction.BR;
			case Direction.L:  return Direction.R;
			default:           return Direction.None;
		}
	}
	
	public static Direction GetDirectionMirroredInY(Direction _dir) {
		switch (_dir){
			case Direction.TL: return Direction.BL;
			case Direction.T:  return Direction.B;
			case Direction.TR: return Direction.BR;
			case Direction.R:  return Direction.R;
			case Direction.BR: return Direction.TR;
			case Direction.B:  return Direction.B;
			case Direction.BL: return Direction.TL;
			case Direction.L:  return Direction.L;
			default:           return Direction.None;
		}
	}
}
