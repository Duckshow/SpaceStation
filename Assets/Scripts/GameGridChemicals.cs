using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using UnityEngine;

public partial class GameGrid {

	private ChemicalSpread[, ] chemicalGridSpreadT = new ChemicalSpread[SIZE.x, SIZE.y];
	private ChemicalSpread[, ] chemicalGridSpreadB = new ChemicalSpread[SIZE.x, SIZE.y];
	private ChemicalSpread[, ] chemicalGridSpreadL = new ChemicalSpread[SIZE.x, SIZE.y];
	private ChemicalSpread[, ] chemicalGridSpreadR = new ChemicalSpread[SIZE.x, SIZE.y];

	public override bool IsUsingAwakeLate() { return true; }
	public override void AwakeLate() {
		base.AwakeLate();

		for(int x = 0; x < SIZE.x; x++) {
			for(int y = 0; y < SIZE.y; y++) {
				chemicalGridSpreadT[x, y] = new ChemicalSpread(Node.CHEM_AMOUNT_MAX);
				chemicalGridSpreadB[x, y] = new ChemicalSpread(Node.CHEM_AMOUNT_MAX);
				chemicalGridSpreadL[x, y] = new ChemicalSpread(Node.CHEM_AMOUNT_MAX);
				chemicalGridSpreadR[x, y] = new ChemicalSpread(Node.CHEM_AMOUNT_MAX);
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

		float _total = 0.0f;
		float _temp = 0.0f;
		float min = Mathf.Infinity;
		float max = Mathf.NegativeInfinity;
		for(int x = 0; x < SIZE.x; x++) {
			for(int y = 0; y < SIZE.y; y++) {
				ChemicalContainer chemCont = nodeGrid[x, y].ChemicalContainer;

				int _a = chemCont.Contents[0].Amount;
				_total += _a;
				_temp += chemCont.Temperature;

				if (_a < min) {
					min = _a;
				}

				if (_a > max) {
					max = _a;
				}
			}
		}

		_temp /=(float)(SIZE.x * SIZE.y);
		Debug.LogFormat("Amount: {0}, Min Amount: {1}, Max Amount: {2}, Temperature: {3}", _total, min, max, _temp);
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

		ChemicalSpread[, ] gridSpread;
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

		ChemicalSpread _giveSpread = gridSpread[_nodeGridPos.x, _nodeGridPos.y];
		ChemicalContainer _takeFrom = nodeGrid[_nodeGridPos.x, _nodeGridPos.y].ChemicalContainer;
		ChemicalContainer _giveTo = nodeGrid[_nodeGridPos.x + _dir.x, _nodeGridPos.y + _dir.y].ChemicalContainer;
		
		_takeFrom.Subtract(_giveSpread);
		_giveTo.Add(_giveSpread);
		
		nodeGrid[_nodeGridPos.x, _nodeGridPos.y].DebugChemicalContainer.Contents[0].SetAmount(chemicalGridSpreadT[_nodeGridPos.x, _nodeGridPos.y].Contents[0].Amount);
		nodeGrid[_nodeGridPos.x, _nodeGridPos.y].DebugChemicalContainer.Contents[1].SetAmount(0);
		nodeGrid[_nodeGridPos.x, _nodeGridPos.y].DebugChemicalContainer.Contents[2].SetAmount(0);
	}

	ChemicalSpread TryGetNewSpread(ChemicalContainer _origin, Int2 _nodeGridPosNeighbor, ref int _amountRemaining) {
		ChemicalSpread _spread = new ChemicalSpread(_origin.MaxAmount);
		ChemicalContainer _neighbor = nodeGridStartFrame[_nodeGridPosNeighbor.x, _nodeGridPosNeighbor.y].ChemicalContainer;

		int _neighborAmountTotal = _neighbor.GetAmountTotal();
		float _neighborAmountTotalClamped = Mathf.Max(0.001f, _neighborAmountTotal);

		float _t = Mathf.Clamp01(_amountRemaining / _neighborAmountTotalClamped * 0.5f);

		float _averageTemperatureTransferRate = 0.0f;
		int _transferredTemperatureCount = 0;
		int _transferredAmount = 0;

		int _chemicalCount = ChemicalManager.GetInstance().GetAllChemicals().Length;
		for(int i = 0; i < _chemicalCount; i++) {
			
			float _originTemperatureTransferRate = _origin.Contents[i].GetTemperatureTransferRate();
			float _neighborTemperatureTransferRate = _neighbor.Contents[i].GetTemperatureTransferRate();
			_averageTemperatureTransferRate +=(_originTemperatureTransferRate + _neighborTemperatureTransferRate) * 0.5f;
			_transferredTemperatureCount++;
			
			if(_amountRemaining < _neighborAmountTotal) {
				continue;
			}

			int _amount = GetChemicalAmountToTransfer(i, _origin, _neighbor);
			if(_amount <= 0) {
				continue;
			}

			_spread.Contents[i].SetAmount(_amount);
			_amountRemaining -= _amount;
			_transferredAmount += _amount;
		}

		_averageTemperatureTransferRate /=(float)(_transferredTemperatureCount + 1);
		float _maxTemperatureAbleToSet =(_origin.Temperature - _neighbor.Temperature);
		float _finalTemperatureTransferRate = _averageTemperatureTransferRate * _maxTemperatureAbleToSet;

		float _temperatureTransferredByContact = _finalTemperatureTransferRate * _t;
		float _temperatureTransferredByAmount = _maxTemperatureAbleToSet * Mathf.Clamp01(_transferredAmount / _neighborAmountTotalClamped);
	
		if (_temperatureTransferredByContact > 0 && _temperatureTransferredByContact > _temperatureTransferredByAmount) {
			_spread.Temperature = _temperatureTransferredByContact;
			_spread.TemperatureLossMod = _neighborAmountTotal / (float)_amountRemaining;
		}
		else if(_temperatureTransferredByAmount > 0){
			_spread.Temperature        = _temperatureTransferredByAmount;
			_spread.TemperatureLossMod = _neighborAmountTotal / (float)_transferredAmount;
		}
		else {
			_spread.Temperature = 0;
			_spread.TemperatureLossMod = 0;
		}

		return _spread;
	}

	static int GetChemicalAmountToTransfer(int _chemicalIndex, ChemicalContainer _source, ChemicalContainer _target) {
		int _totalAmountSource = Mathf.Clamp(_source.GetAmountTotal(), 0, _source.MaxAmount);
		int _totalAmountTarget = Mathf.Clamp(_target.GetAmountTotal(), 0, _target.MaxAmount);
		int _totalAmountDiff = _totalAmountSource - _totalAmountTarget;

		Chemical.Blob _sourceChem = _source.Contents[_chemicalIndex];

		int _maxAmountAbleToGet = Mathf.Min(_totalAmountDiff, _sourceChem.GetAmountTransferablePerFrame());
		int _maxAmountAbleToSet = _target.MaxAmount - _totalAmountTarget;
		float _shareOfTotalAmountAtSource = _sourceChem.Amount /(float)_totalAmountSource;

		return Mathf.RoundToInt(Mathf.Min(_maxAmountAbleToSet, _maxAmountAbleToGet * _shareOfTotalAmountAtSource));
	}
}