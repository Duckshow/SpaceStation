using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Node {
	
	public const int CHEM_AMOUNT_MAX = 100;

	public class InteractiveObject {
		public enum State { None, Default, OpenLeft, OpenRight, OpenAbove, OpenBelow }

		public InteractiveObjectAsset Asset { get; private set; }
		public State CurrentState { get; private set; }
		public Rotation Rotation { get; private set; }

		private Node node;

		public InteractiveObject(Node _node, InteractiveObjectAsset _asset, Rotation _rotation) {
			node = _node;
			Asset = _asset;
			Rotation = _rotation;
			CurrentState = State.Default;
		}

		public float GetWaitTime() {
			return Asset.WaitTime;
		}

		public bool GetIsWalkable() {
			return Asset.IsWalkable;
		}

		public Int2 GetTileAssetPos(Sorting _sorting, Direction _direction) {
			switch (CurrentState) {
				case State.None:
					return Int2.Zero;
				case State.Default:
					return Asset.GetTileAssetPosForDefaultState(_sorting, Rotation, _direction);
				case State.OpenLeft:
				case State.OpenBelow:
					return Asset.GetTileAssetPosForOpenLeftOrBelow(_sorting, Rotation, _direction);
				case State.OpenRight:
				case State.OpenAbove:
					return Asset.GetTileAssetPosForOpenRightOrAbove(_sorting, Rotation, _direction);
				default:
					Debug.LogError(CurrentState + " hasn't been properly implemented yet!");
					return Int2.Zero;
			}
		}

		public void OnCharacterApproachingOrDeparting(Character character) {
			State _oldState = CurrentState;

			Vector2 _dir = ((Vector2) character.transform.position - node.WorldPos).normalized;
			bool _isCharacterToTheLeft = _dir.x < -0.5f;
			bool _isCharacterToTheRight = _dir.x > 0.5f;
			bool _isCharacterAbove = _dir.y > 0.5f;
			bool _isCharacterBelow = _dir.y < -0.5f;

			if (_isCharacterToTheLeft && Asset.OnCharacterIsLeft != State.None) {
				CurrentState = Asset.OnCharacterIsLeft;
			} else if (_isCharacterToTheRight && Asset.OnCharacterIsRight != State.None) {
				CurrentState = Asset.OnCharacterIsRight;
			} else if (_isCharacterAbove && Asset.OnCharacterIsAbove != State.None) {
				CurrentState = Asset.OnCharacterIsAbove;
			} else if (_isCharacterBelow && Asset.OnCharacterIsBelow != State.None) {
				CurrentState = Asset.OnCharacterIsBelow;
			}

			if (CurrentState != _oldState) {
				node.ScheduleUpdateGraphicsForSurroundingTiles();
			}
		}

		public void OnCharacterApproachOrDepartCancelled(Character _character) {
			CurrentState = State.Default;
			node.ScheduleUpdateGraphicsForSurroundingTiles();
		}

		public void OnCharacterApproachOrDepartFinished(Character character) {
			CurrentState = State.Default;
			node.ScheduleUpdateGraphicsForSurroundingTiles();
		}
	}

	public ChemicalContainer ChemicalContainer;
	public ChemicalContainer DebugChemicalContainer;

	public int RoomIndex { get; private set; }
	public void SetRoomIndex(int _index) {
		RoomIndex = _index;
		ScheduleUpdateGraphicsForSurroundingTiles();
	}

	public bool IsWall { get; private set; }
	public bool IsWallTemporarily { get; private set; }
	public bool UseIsWallTemporary { get; private set; }

	public bool UseAttachedInteractiveObjectTemporary { get; private set; }
	public InteractiveObject AttachedInteractiveObject { get; private set; }
	public InteractiveObject AttachedInteractiveObjectTemporary { get; private set; }

	private Color32 lightingTL = new Color32(0, 0, 0, 0);
	private Color32 lightingTR = new Color32(0, 0, 0, 0);
	private Color32 lightingBR = new Color32(0, 0, 0, 0);
	private Color32 lightingBL = new Color32(0, 0, 0, 0);

	public float GetWaitTime() {
		return AttachedInteractiveObject != null ? AttachedInteractiveObject.GetWaitTime() : 0.0f;
	}

	public bool GetIsWalkable() {
		return !IsWall || (AttachedInteractiveObject != null && AttachedInteractiveObject.GetIsWalkable());
	}

	public Color32 GetLighting() {
		if (!lightingTL.Equals(lightingTR) || !lightingTL.Equals(lightingBR) || !lightingTL.Equals(lightingBL)) {
			Debug.LogErrorFormat("Node({0})'s lighting varies across vertices, but is still expected to return a uniform lighting variable!", GridPos);
		}

		return lightingTL;
	}

	public void SetLighting(Color32 _light) {
		lightingTL = _light;
		lightingTR = _light;
		lightingBR = _light;
		lightingBL = _light;
		ScheduleUpdateGraphicsForSurroundingTiles();
	}

	public void SetLightingBasedOnNeighbors() {
		Node _nodeTL, _nodeT, _nodeTR, _nodeR, _nodeBR, _nodeB, _nodeBL, _nodeL;
		NeighborFinder.GetSurroundingNodes(GridPos, out _nodeTL, out _nodeT, out _nodeTR, out _nodeR, out _nodeBR, out _nodeB, out _nodeBL, out _nodeL);

		lightingTL = GetLightingFromDirection(Direction.TL);
		lightingTR = GetLightingFromDirection(Direction.TR);
		lightingBR = GetLightingFromDirection(Direction.BR);
		lightingBL = GetLightingFromDirection(Direction.BL);

		ScheduleUpdateGraphicsForSurroundingTiles();
	}

	Color32 GetLightingFromDirection(Direction _direction) {
		Direction _directionY = Direction.None;
		Direction _directionX = Direction.None;
		switch (_direction) {
			case Direction.None:
			case Direction.All:
			case Direction.T:
			case Direction.R:
			case Direction.B:
			case Direction.L:
				Debug.LogError(_direction + " isn't supported by GetLightingFromDirection()!");
				break;
			case Direction.TL:
				_directionY = Direction.T;
				_directionX = Direction.L;
				break;
			case Direction.TR:
				_directionY = Direction.T;
				_directionX = Direction.R;
				break;
			case Direction.BR:
				_directionY = Direction.B;
				_directionX = Direction.R;
				break;
			case Direction.BL:
				_directionY = Direction.B;
				_directionX = Direction.L;
				break;
			default:
				Debug.LogError(_direction + " hasn't been properly implemented yet!");
				break;
		}

		NeighborFinder.TryCacheNeighbor(GridPos, _direction);
		NeighborFinder.TryCacheNeighbor(GridPos, _directionX);
		NeighborFinder.TryCacheNeighbor(GridPos, _directionY);

		Node _neighborXY = NeighborFinder.CachedNeighbors[_direction];
		Node _neighborY = NeighborFinder.CachedNeighbors[_directionX];
		Node _neighborX = NeighborFinder.CachedNeighbors[_directionY];

		int _neighborsGivingLight = 0;

		Color32 _lightingFromNeighborXY = new Color32();
		if (_neighborXY != null && !_neighborXY.IsWall) {
			_lightingFromNeighborXY = _neighborXY.GetLighting();
			_neighborsGivingLight++;
		}

		Color32 _lightingFromNeighborY = new Color32();
		if (_neighborY != null && !_neighborY.IsWall) {
			_lightingFromNeighborY = _neighborY.GetLighting();
			_neighborsGivingLight++;
		}

		Color32 _lightingFromNeighborX = new Color32();
		if (_neighborX != null && !_neighborX.IsWall) {
			_lightingFromNeighborX = _neighborX.GetLighting();
			_neighborsGivingLight++;
		}

		if (_neighborsGivingLight == 0) {
			return new Color32();
		}

		Color32 _newLighting = new Color32(
			(byte) ((_lightingFromNeighborXY.r + _lightingFromNeighborY.r + _lightingFromNeighborX.r) / _neighborsGivingLight),
			(byte) ((_lightingFromNeighborXY.g + _lightingFromNeighborY.g + _lightingFromNeighborX.g) / _neighborsGivingLight),
			(byte) ((_lightingFromNeighborXY.b + _lightingFromNeighborY.b + _lightingFromNeighborX.b) / _neighborsGivingLight),
			(byte) ((_lightingFromNeighborXY.a + _lightingFromNeighborY.a + _lightingFromNeighborX.a) / _neighborsGivingLight)
		);

		// Color32 _newLighting = new Color32(
		// 	255, 255, 255, 255
		// );

		return _newLighting;
	}

	public Int2 GridPos { get; private set; }
	public Vector2 WorldPos { get; private set; }

	private NodeObject occupyingNodeObject = null;
	public NodeObject GetOccupyingNodeObject() {
		return occupyingNodeObject;
	}
	public void SetOccupyingNodeObject(NodeObject _newOccupant) {
		if (occupyingNodeObject != null && occupyingNodeObject != _newOccupant)
			Debug.LogErrorFormat("{0}'s new node ({1}) is occupied by {2}! This shouldn't happen!", _newOccupant.transform.name, GridPos, occupyingNodeObject.transform.name);
		occupyingNodeObject = _newOccupant;
	}
	public void ClearOccupyingNodeObject(NodeObject _caller) {
		if (occupyingNodeObject != null && occupyingNodeObject != _caller)
			Debug.LogErrorFormat("{0} tried to set Node({1})'s occupyingNodeObject to null, but isn't actually its current occupant!", _caller.transform.name, GridPos);
		occupyingNodeObject = null;
	}

	public Node(Vector3 _worldPos, int _gridX, int _gridY) {
		WorldPos = _worldPos;
		GridPos = new Int2(_gridX, _gridY);
		
		ChemicalContainer = new ChemicalContainer(CHEM_AMOUNT_MAX);
		DebugChemicalContainer = new ChemicalContainer(CHEM_AMOUNT_MAX);
	}

	public void TrySetIsWall(bool _isWall) {
		if (IsWall == _isWall) {
			return;
		}

		IsWall = _isWall;

		if (_isWall) {
			NeighborFinder.TryCacheNeighbor(GridPos, Direction.All);
			LampManager.GetInstance().TryAddNodeToUpdate(NeighborFinder.CachedNeighbors[Direction.TL]);
			LampManager.GetInstance().TryAddNodeToUpdate(NeighborFinder.CachedNeighbors[Direction.T]);
			LampManager.GetInstance().TryAddNodeToUpdate(NeighborFinder.CachedNeighbors[Direction.TR]);
			LampManager.GetInstance().TryAddNodeToUpdate(NeighborFinder.CachedNeighbors[Direction.R]);
			LampManager.GetInstance().TryAddNodeToUpdate(NeighborFinder.CachedNeighbors[Direction.BR]);
			LampManager.GetInstance().TryAddNodeToUpdate(NeighborFinder.CachedNeighbors[Direction.B]);
			LampManager.GetInstance().TryAddNodeToUpdate(NeighborFinder.CachedNeighbors[Direction.BL]);
			LampManager.GetInstance().TryAddNodeToUpdate(NeighborFinder.CachedNeighbors[Direction.L]);
		} else {
			LampManager.GetInstance().TryAddNodeToUpdate(this);
		}

		RoomManager.GetInstance().ScheduleUpdateForRoomOfNode(GridPos);
		ScheduleUpdateGraphicsForSurroundingTiles();
	}

	public void TrySetIsWallTemporary(bool _isWallTemporary) {
		if (UseIsWallTemporary) {
			return;
		}

		IsWallTemporarily = _isWallTemporary;
		UseIsWallTemporary = true;
		ScheduleUpdateGraphicsForSurroundingTiles();
	}

	public void TryClearIsWallTemporary() {
		if (!UseIsWallTemporary) {
			return;
		}

		IsWallTemporarily = false;
		UseIsWallTemporary = false;
		ScheduleUpdateGraphicsForSurroundingTiles();
	}

	public void TrySetInteractiveObject(InteractiveObjectAsset _asset, Rotation _rotation) {
		if (AttachedInteractiveObject != null) {
			return;
		}

		AttachedInteractiveObject = new InteractiveObject(this, _asset, _rotation);
		ScheduleUpdateGraphicsForSurroundingTiles();
	}

	public void TrySetInteractiveObjectTemporary(InteractiveObjectAsset _asset, Rotation _rotation) {
		if (UseAttachedInteractiveObjectTemporary) {
			return;
		}

		AttachedInteractiveObjectTemporary = new InteractiveObject(this, _asset, _rotation);
		UseAttachedInteractiveObjectTemporary = true;
		ScheduleUpdateGraphicsForSurroundingTiles();
	}

	public void TryClearInteractiveObjectTemporary() {
		if (!UseAttachedInteractiveObjectTemporary) {
			return;
		}

		AttachedInteractiveObjectTemporary = null;
		UseAttachedInteractiveObjectTemporary = false;
		ScheduleUpdateGraphicsForSurroundingTiles();
	}

	public void ScheduleUpdateGraphicsForSurroundingTiles() {
		ColorManager.ColorUsage _context = ColorManager.ColorUsage.Default;

		if (UseIsWallTemporary && IsWallTemporarily) {
			_context = ColorManager.ColorUsage.New;
		}
		if (UseIsWallTemporary && !IsWallTemporarily) {
			_context = ColorManager.ColorUsage.Delete;
		}

		if (UseAttachedInteractiveObjectTemporary && AttachedInteractiveObjectTemporary != null) {
			_context = ColorManager.ColorUsage.New;

			if (AttachedInteractiveObject != null) {
				_context = ColorManager.ColorUsage.Blocked;
			}
		}
		if (UseAttachedInteractiveObjectTemporary && AttachedInteractiveObjectTemporary == null) {
			_context = ColorManager.ColorUsage.Delete;
		}

		byte _colorIndex = ColorManager.GetColorIndex(_context);

		byte[] _colorChannelIndices = new byte[ColorManager.COLOR_CHANNEL_COUNT] {
			_colorIndex,
			_colorIndex,
			_colorIndex,
			_colorIndex,
			_colorIndex,
			_colorIndex,
			_colorIndex,
			_colorIndex,
			_colorIndex,
			_colorIndex
		};

		Int2 _tileGridPosTL, _tileGridPosTR, _tileGridPosBR, _tileGridPosBL;
		NeighborFinder.GetSurroundingTiles(GridPos, out _tileGridPosTL, out _tileGridPosTR, out _tileGridPosBR, out _tileGridPosBL);

		if (_tileGridPosTR != Int2.MinusOne) {
			GameGrid.GetInstance().ScheduleUpdateForTileAsset(_tileGridPosTR);
			UpdateTileColor(_tileGridPosTR, _colorChannelIndices);
			SetTileVertexLighting(_tileGridPosTR, lightingTR, _vertexIndex : GameGridMesh.VERTEX_INDEX_BOTTOM_LEFT);
		}
		if (_tileGridPosBL != Int2.MinusOne) {
			GameGrid.GetInstance().ScheduleUpdateForTileAsset(_tileGridPosBL);
			UpdateTileColor(_tileGridPosBL, _colorChannelIndices);
			SetTileVertexLighting(_tileGridPosBL, lightingBL, _vertexIndex : GameGridMesh.VERTEX_INDEX_TOP_RIGHT);
		}
		if (_tileGridPosTL != Int2.MinusOne) {
			GameGrid.GetInstance().ScheduleUpdateForTileAsset(_tileGridPosTL);
			UpdateTileColor(_tileGridPosTL, _colorChannelIndices);
			SetTileVertexLighting(_tileGridPosTL, lightingTL, _vertexIndex : GameGridMesh.VERTEX_INDEX_BOTTOM_RIGHT);
		}
		if (_tileGridPosBR != Int2.MinusOne) {
			GameGrid.GetInstance().ScheduleUpdateForTileAsset(_tileGridPosBR);
			UpdateTileColor(_tileGridPosBR, _colorChannelIndices);
			SetTileVertexLighting(_tileGridPosBR, lightingBR, _vertexIndex : GameGridMesh.VERTEX_INDEX_TOP_LEFT);
		}
	}
	
	void UpdateTileColor(Int2 _tileGridPos, byte[] _colorChannelIndices) {
		if (UseIsWallTemporary) {
			GameGrid.GetInstance().SetColor(_tileGridPos, _colorChannelIndices, _isPermanent : false);
		} else {
			GameGrid.GetInstance().ClearTemporaryColor(_tileGridPos);
		}
	}

	void SetTileVertexLighting(Int2 _tileGridPos, Color32 _lighting, int _vertexIndex) {
		GameGrid.GetInstance().SetLighting(_tileGridPos, _vertexIndex, _lighting);
	}

	public void SetColor(byte[] _colorChannelIndices, bool _isPermanent) {
		Int2 _tileGridPosTL, _tileGridPosTR, _tileGridPosBR, _tileGridPosBL;
		NeighborFinder.GetSurroundingTiles(GridPos, out _tileGridPosTL, out _tileGridPosTR, out _tileGridPosBR, out _tileGridPosBL);

		if (_tileGridPosTR != Int2.MinusOne) {
			GameGrid.GetInstance().SetColor(_tileGridPosTR, _colorChannelIndices, _isPermanent);
		}
	}

	public void ClearTemporaryColor() {
		Int2 _tileGridPosTL, _tileGridPosTR, _tileGridPosBR, _tileGridPosBL;
		NeighborFinder.GetSurroundingTiles(GridPos, out _tileGridPosTL, out _tileGridPosTR, out _tileGridPosBR, out _tileGridPosBL);

		// if (_tileTL != null){
		// 	_tileTL.ClearTemporaryColor();
		// }
		if (_tileGridPosTR != Int2.MinusOne) {
			GameGrid.GetInstance().ClearTemporaryColor(_tileGridPosTR);
		}
		// if (_tileBR != null){
		// 	_tileBR.ClearTemporaryColor();
		// }
		// if (_tileBL != null){
		// 	_tileBL.ClearTemporaryColor();
		// }
	}

	public int GetMovementPenalty() {
		return 0;
	}

	public void OnCharacterApproaching(Character character) {
		if (AttachedInteractiveObject != null) {
			AttachedInteractiveObject.OnCharacterApproachingOrDeparting(character);
		}
	}

	public void OnCharacterApproachCancel(Character character) {
		if (AttachedInteractiveObject != null) {
			AttachedInteractiveObject.OnCharacterApproachOrDepartCancelled(character);
		}
	}

	public void OnCharacterApproachFinished(Character character) {
		if (AttachedInteractiveObject != null) {
			AttachedInteractiveObject.OnCharacterApproachOrDepartFinished(character);
		}
	}

	public void OnCharacterDeparting(Character character) {
		if (AttachedInteractiveObject != null) {
			AttachedInteractiveObject.OnCharacterApproachingOrDeparting(character);
		}
	}

	public void OnCharacterDepartCancelled(Character character) {
		if (AttachedInteractiveObject != null) {
			AttachedInteractiveObject.OnCharacterApproachOrDepartCancelled(character);
		}
	}

	public void OnCharacterDepartFinished(Character character) {
		if (AttachedInteractiveObject != null) {
			AttachedInteractiveObject.OnCharacterApproachOrDepartFinished(character);
		}
	}
}