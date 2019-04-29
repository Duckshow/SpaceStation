using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGridChemicals : EventOwner {

	public const int TILE_CHEM_AMOUNT_MAX = 100;

	private ChemicalContainer[, ] chemicalGrid = new ChemicalContainer[GameGrid.SIZE.x, GameGrid.SIZE.y];
	private ChemicalContainer[, ] chemicalGridStartFrame = new ChemicalContainer[GameGrid.SIZE.x, GameGrid.SIZE.y];

	private ChemicalContainer[, ] chemicalGridSpreadT = new ChemicalContainer[GameGrid.SIZE.x, GameGrid.SIZE.y];
	private ChemicalContainer[, ] chemicalGridSpreadB = new ChemicalContainer[GameGrid.SIZE.x, GameGrid.SIZE.y];
	private ChemicalContainer[, ] chemicalGridSpreadL = new ChemicalContainer[GameGrid.SIZE.x, GameGrid.SIZE.y];
	private ChemicalContainer[, ] chemicalGridSpreadR = new ChemicalContainer[GameGrid.SIZE.x, GameGrid.SIZE.y];

	public override bool IsUsingStartLate() { return true; }
	public override void StartLate() {
		for(int x = 0; x < GameGrid.SIZE.x; x++) {
			for(int y = 0; y < GameGrid.SIZE.y; y++) {
				chemicalGrid[x, y] = new ChemicalContainer(TILE_CHEM_AMOUNT_MAX);
				chemicalGridSpreadT[x, y] = new ChemicalContainer(TILE_CHEM_AMOUNT_MAX);
				chemicalGridSpreadB[x, y] = new ChemicalContainer(TILE_CHEM_AMOUNT_MAX);
				chemicalGridSpreadL[x, y] = new ChemicalContainer(TILE_CHEM_AMOUNT_MAX);
				chemicalGridSpreadR[x, y] = new ChemicalContainer(TILE_CHEM_AMOUNT_MAX);

				Node _node = GameGrid.GetInstance().TryGetNode(x, y);

				int _amount = 0;
				int _temperature = 0;
				if(x <= 30 && y <= 30 && x >= 20 && y >= 20) {
					_amount = 100;
					_temperature = 380;
				}
				else {
					_amount = 0;
					_temperature = 0;
				}

				chemicalGrid[x, y].SetStartValues(_amount, _temperature);
			}
		}
	}

	public override bool IsUsingUpdateDefault() { return true; }
	public override void UpdateDefault() {
		base.UpdateDefault();

		chemicalGridStartFrame = chemicalGrid;

		// int _amount = 0;
		// float _temperature = 0;
		// int _tilesWithTemperature = 0;
		// for(int x = 0; x < GameGrid.SIZE.x; x++) {
		// 	for(int y = 0; y < GameGrid.SIZE.y; y++) {
		// 		Int2 _nodeGridPos = new Int2(x, y);
		// 		ChemicalContainer _container = chemicalGrid[_nodeGridPos.x, _nodeGridPos.y];
		// 		_amount += _container.Water.Amount;
		// 		_temperature += _container.Temperature;

		// 		if(_container.Temperature > 0) {
		// 			_tilesWithTemperature++;
		// 		}
		// 	}
		// }
		// if(_tilesWithTemperature > 0) {
		// 	_temperature /= _tilesWithTemperature;
		// }
		// Debug.Log((_amount + ", " + _temperature).Color(Color.cyan));

		for(int x = 0; x < GameGrid.SIZE.x; x++) {
			for(int y = 0; y < GameGrid.SIZE.y; y++) {
				Int2 _nodeGridPos = new Int2(x, y);
				CalculateChemicalsAndTemperatureToSpread(_nodeGridPos);
			}
		}

		for(int x = 0; x < GameGrid.SIZE.x; x++) {
			for(int y = 0; y < GameGrid.SIZE.y; y++) {
				Int2 _nodeGridPos = new Int2(x, y);
				SpreadChemicalsAndTemperature(_nodeGridPos);

				// Color32 _color = chemicalGrid[_nodeGridPos.x, _nodeGridPos.y].GetColor();
				// GameGrid.GetInstance().SetLighting(_nodeGridPos, GameGridMesh.VERTEX_INDEX_BOTTOM_LEFT, _color);
				// GameGrid.GetInstance().SetLighting(_nodeGridPos, GameGridMesh.VERTEX_INDEX_BOTTOM_RIGHT, _color);
				// GameGrid.GetInstance().SetLighting(_nodeGridPos, GameGridMesh.VERTEX_INDEX_TOP_LEFT, _color);
				// GameGrid.GetInstance().SetLighting(_nodeGridPos, GameGridMesh.VERTEX_INDEX_TOP_RIGHT, _color);
			}
		}
	}

	void CalculateChemicalsAndTemperatureToSpread(Int2 _nodeGridPos) {
		ChemicalContainer _origin = chemicalGrid[_nodeGridPos.x, _nodeGridPos.y];
		int _amountRemaining = _origin.GetAmountTotal();
		if(_amountRemaining <= 0) {
			return;
		}

		List<KeyValuePair<Direction, int>> _sortableNeighbors = new List<KeyValuePair<Direction, int>>();

		if(_nodeGridPos.x > 0) {
			_sortableNeighbors.Add(new KeyValuePair<Direction, int>(Direction.L, chemicalGrid[_nodeGridPos.x - 1, _nodeGridPos.y].GetAmountTotal()));
		}
		if(_nodeGridPos.y > 0) {
			_sortableNeighbors.Add(new KeyValuePair<Direction, int>(Direction.B, chemicalGrid[_nodeGridPos.x, _nodeGridPos.y - 1].GetAmountTotal()));
		}
		if(_nodeGridPos.x < GameGrid.SIZE.x - 1) {
			_sortableNeighbors.Add(new KeyValuePair<Direction, int>(Direction.R, chemicalGrid[_nodeGridPos.x + 1, _nodeGridPos.y].GetAmountTotal()));
		}
		if(_nodeGridPos.y < GameGrid.SIZE.y - 1) {
			_sortableNeighbors.Add(new KeyValuePair<Direction, int>(Direction.T, chemicalGrid[_nodeGridPos.x, _nodeGridPos.y + 1].GetAmountTotal()));
		}

		_sortableNeighbors.Sort((x, y) => x.Value.CompareTo(y.Value));

		for(int i = 0; i < _sortableNeighbors.Count; i++) {
			Direction _dir = _sortableNeighbors[i].Key;

			switch(_dir) {
				case Direction.L:
					chemicalGridSpreadL[_nodeGridPos.x, _nodeGridPos.y] = TryGetNewSpread(_origin, _nodeGridPos + Int2.Left, ref _amountRemaining);
					break;
				case Direction.R:
					chemicalGridSpreadR[_nodeGridPos.x, _nodeGridPos.y] = TryGetNewSpread(_origin, _nodeGridPos + Int2.Right, ref _amountRemaining);
					break;
				case Direction.T:
					chemicalGridSpreadT[_nodeGridPos.x, _nodeGridPos.y] = TryGetNewSpread(_origin, _nodeGridPos + Int2.Up, ref _amountRemaining);
					break;
				case Direction.B:
					chemicalGridSpreadB[_nodeGridPos.x, _nodeGridPos.y] = TryGetNewSpread(_origin, _nodeGridPos + Int2.Down, ref _amountRemaining);
					break;
			}
		}
	}

	void SpreadChemicalsAndTemperature(Int2 _nodeGridPos) {
		TrySpreadChemicalsAndTemperatureInDirection(_nodeGridPos, Direction.T);
		TrySpreadChemicalsAndTemperatureInDirection(_nodeGridPos, Direction.B);
		TrySpreadChemicalsAndTemperatureInDirection(_nodeGridPos, Direction.L);
		TrySpreadChemicalsAndTemperatureInDirection(_nodeGridPos, Direction.R);
	}

	void TrySpreadChemicalsAndTemperatureInDirection(Int2 _nodeGridPos, Direction _direction) {
		Int2 _dir2 = Int2.GetDirection(_direction);
		if(!GameGrid.IsInsideNodeGrid(_nodeGridPos + _dir2)) {
			return;
		}

		ChemicalContainer[, ] gridSpread;
		switch(_direction) {
			case Direction.T:
				gridSpread = chemicalGridSpreadT;
				break;
			case Direction.L:
				gridSpread = chemicalGridSpreadL;
				break;
			case Direction.R:
				gridSpread = chemicalGridSpreadR;
				break;
			case Direction.B:
				gridSpread = chemicalGridSpreadB;
				break;
			default:
				throw new System.NotImplementedException(_dir2 + " hasn't been properly implemented yet!");
		}

		ChemicalContainer _giveSpread = gridSpread[_nodeGridPos.x, _nodeGridPos.y];
		chemicalGrid[_nodeGridPos.x, _nodeGridPos.y].Subtract(_giveSpread);
		chemicalGrid[_nodeGridPos.x + _dir2.x, _nodeGridPos.y + _dir2.y].Add(_giveSpread);
	}

	ChemicalContainer TryGetNewSpread(ChemicalContainer _origin, Int2 _nodeGridPosNeighbor, ref int _amountRemaining) {
		ChemicalContainer _spread = new ChemicalContainer(_origin.MaxAmount);

		ChemicalContainer _neighbor = chemicalGridStartFrame[_nodeGridPosNeighbor.x, _nodeGridPosNeighbor.y];
		int _neighborAmountTotal = _neighbor.GetAmountTotal();

		float _neighborAmountTotalClamped = Mathf.Max(0.001f, _neighborAmountTotal);
		_spread.SetTemperature(Mathf.Lerp(0.0f,(_origin.Temperature - _neighbor.Temperature) * 0.5f,(_amountRemaining / _neighborAmountTotalClamped) * 0.5f));

		if(_amountRemaining <= _neighborAmountTotal) {
			return _spread;
		}

		System.Array _ids = System.Enum.GetValues(typeof(Chemical.ID));
		foreach(Chemical.ID _id in _ids) {
			int _amount = GetChemicalAmountToTransfer(_id, _origin, _neighbor);
			if(_amount <= 0) {
				continue;
			}

			Chemical.Blob _spreadChem = _spread.GetChemical(_id);
			_spreadChem.SetAmount(_amount);
			_spread.SetChemical(_id, _spreadChem);

			_amountRemaining -= _amount;
		}

		return _spread;
	}

	public static int GetChemicalAmountToTransfer(Chemical.ID _id, ChemicalContainer _source, ChemicalContainer _target) {
		int _totalAmountSource = _source.GetAmountTotal();
		int _totalAmountTarget = _target.GetAmountTotal();

		if(_totalAmountSource == 0 ||  _totalAmountSource <= _totalAmountTarget) {
			return 0;
		}

		int _totalAmountDiff = _totalAmountSource - _totalAmountTarget;

		Chemical.Blob _sourceChem = _source.GetChemical(_id);

		int _maxAmountAbleToGet = Mathf.Min(_totalAmountDiff, _sourceChem.GetAmountTransferablePerFrame());
		int _maxAmountAbleToSet = _target.MaxAmount - _totalAmountTarget;
		float _shareOfTotalAmountAtSource = _sourceChem.Amount /(float)_totalAmountSource;

		float _amountToTransfer = Mathf.Min(_maxAmountAbleToSet, _maxAmountAbleToGet * _shareOfTotalAmountAtSource);

		return Mathf.RoundToInt(_amountToTransfer);
	}
}