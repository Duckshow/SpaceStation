using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameGrid {


	private ChemicalContainer[, ] chemicalGridSpreadT = new ChemicalContainer[SIZE.x, SIZE.y];
	private ChemicalContainer[, ] chemicalGridSpreadB = new ChemicalContainer[SIZE.x, SIZE.y];
	private ChemicalContainer[, ] chemicalGridSpreadL = new ChemicalContainer[SIZE.x, SIZE.y];
	private ChemicalContainer[, ] chemicalGridSpreadR = new ChemicalContainer[SIZE.x, SIZE.y];

	public override bool IsUsingAwakeLate() { return true; }
	public override void AwakeLate() {
		base.AwakeLate();
		
		for(int x = 0; x < SIZE.x; x++) {
			for(int y = 0; y < SIZE.y; y++) {
				chemicalGridSpreadT[x, y] = new ChemicalContainer(Node.CHEM_AMOUNT_MAX);
				chemicalGridSpreadB[x, y] = new ChemicalContainer(Node.CHEM_AMOUNT_MAX);
				chemicalGridSpreadL[x, y] = new ChemicalContainer(Node.CHEM_AMOUNT_MAX);
				chemicalGridSpreadR[x, y] = new ChemicalContainer(Node.CHEM_AMOUNT_MAX);
			}
		}
	}

	public override bool IsUsingUpdateDefault() { return true; }
	public override void UpdateDefault() {
		base.UpdateDefault();

		for(int x = 0; x < SIZE.x; x++) {
			for(int y = 0; y < SIZE.y; y++) {
				Int2 _nodeGridPos = new Int2(x, y);
				CalculateChemicalsAndTemperatureToSpread(_nodeGridPos);
			}
		}

		for(int x = 0; x < SIZE.x; x++) {
			for(int y = 0; y < SIZE.y; y++) {
				Int2 _nodeGridPos = new Int2(x, y);
				SpreadChemicalsAndTemperature(_nodeGridPos);
				ScheduleCacheChemicalData(_nodeGridPos);
			}
		}
	}

	void CalculateChemicalsAndTemperatureToSpread(Int2 _nodeGridPos) {
		ChemicalContainer _origin = nodeGrid[_nodeGridPos.x, _nodeGridPos.y].ChemicalContainer;
		int _amountRemaining = _origin.GetAmountTotal();
		if(_amountRemaining <= 0) {
			return;
		}

		List<KeyValuePair<Direction, int>> _sortableNeighbors = new List<KeyValuePair<Direction, int>>();

		if(_nodeGridPos.x > 0) {
			int _amount = nodeGrid[_nodeGridPos.x - 1, _nodeGridPos.y].ChemicalContainer.GetAmountTotal();
			_sortableNeighbors.Add(new KeyValuePair<Direction, int>(Direction.L, _amount));
		}
		if(_nodeGridPos.y > 0) {
			int _amount = nodeGrid[_nodeGridPos.x, _nodeGridPos.y - 1].ChemicalContainer.GetAmountTotal();
			_sortableNeighbors.Add(new KeyValuePair<Direction, int>(Direction.B, _amount));
		}
		if(_nodeGridPos.x < SIZE.x - 1) {
			int _amount = nodeGrid[_nodeGridPos.x + 1, _nodeGridPos.y].ChemicalContainer.GetAmountTotal();
			_sortableNeighbors.Add(new KeyValuePair<Direction, int>(Direction.R, _amount));
		}
		if(_nodeGridPos.y < SIZE.y - 1) {
			int _amount = nodeGrid[_nodeGridPos.x, _nodeGridPos.y + 1].ChemicalContainer.GetAmountTotal();
			_sortableNeighbors.Add(new KeyValuePair<Direction, int>(Direction.T, _amount));
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
		Int2 _dir = Int2.GetDirection(_direction);
		if(!IsInsideNodeGrid(_nodeGridPos + _dir)) {
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
				throw new System.NotImplementedException(_dir + " hasn't been properly implemented yet!");
		}

		ChemicalContainer _giveSpread = gridSpread[_nodeGridPos.x, _nodeGridPos.y];
		ChemicalContainer _takeFrom = nodeGrid[_nodeGridPos.x, _nodeGridPos.y].ChemicalContainer;
		ChemicalContainer _giveTo = nodeGrid[_nodeGridPos.x + _dir.x, _nodeGridPos.y + _dir.y].ChemicalContainer;
		
		_takeFrom.Subtract(_giveSpread);
		_giveTo.Add(_giveSpread);
	}

	ChemicalContainer TryGetNewSpread(ChemicalContainer _origin, Int2 _nodeGridPosNeighbor, ref int _amountRemaining) {
		ChemicalContainer _spread = new ChemicalContainer(_origin.MaxAmount);
		ChemicalContainer _neighbor = nodeGridStartFrame[_nodeGridPosNeighbor.x, _nodeGridPosNeighbor.y].ChemicalContainer;

		int _neighborAmountTotal = _neighbor.GetAmountTotal();
		float _neighborAmountTotalClamped = Mathf.Max(0.001f, _neighborAmountTotal);
		
		_spread.SetTemperature(Mathf.Lerp(0.0f,(_origin.Temperature - _neighbor.Temperature) * 0.5f,(_amountRemaining / _neighborAmountTotalClamped) * 0.5f));

		if(_amountRemaining <= _neighborAmountTotal) {
			return _spread;
		}

		int _chemicalCount = ChemicalManager.GetInstance().GetAllChemicals().Length;
		for(int i = 0; i < _chemicalCount; i++) {
			int _amount = GetChemicalAmountToTransfer(i, _origin, _neighbor);
			if(_amount <= 0) {
				continue;
			}

			_spread.Contents[i].SetAmount(_amount);
			_amountRemaining -= _amount;
		}

		return _spread;
	}

	public static int GetChemicalAmountToTransfer(int _chemicalIndex, ChemicalContainer _source, ChemicalContainer _target) {
		int _totalAmountSource = _source.GetAmountTotal();
		int _totalAmountTarget = _target.GetAmountTotal();

		if(_totalAmountSource == 0 ||  _totalAmountSource <= _totalAmountTarget) {
			return 0;
		}

		int _totalAmountDiff = _totalAmountSource - _totalAmountTarget;

		Chemical.Blob _sourceChem = _source.Contents[_chemicalIndex];

		int _maxAmountAbleToGet = Mathf.Min(_totalAmountDiff, _sourceChem.GetAmountTransferablePerFrame());
		int _maxAmountAbleToSet = _target.MaxAmount - _totalAmountTarget;
		float _shareOfTotalAmountAtSource = _sourceChem.Amount /(float)_totalAmountSource;

		float _amountToTransfer = Mathf.Min(_maxAmountAbleToSet, _maxAmountAbleToGet * _shareOfTotalAmountAtSource);

		return Mathf.RoundToInt(_amountToTransfer);
	}
}