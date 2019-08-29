using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using UnityEngine;

public partial class GameGrid {
	private bool b;

	public override bool IsUsingUpdateDefault() { return true; }
	public override void UpdateDefault() {
		base.UpdateDefault();

		if (b) {
			for(int x = 0; x < SIZE.x; x++) {
				for(int y = 0; y < SIZE.y; y++) {
					Int2 _nodeGridPos = new Int2(x, y);
					SpreadChemicalsAndTemperature(_nodeGridPos);
					ScheduleCacheChemicalData(_nodeGridPos);
				}
			}
		}
		else {
			for(int x = SIZE.x - 1; x >= 0; x--) {
				for(int y = SIZE.y - 1; y >= 0; y--) {
					Int2 _nodeGridPos = new Int2(x, y);
					SpreadChemicalsAndTemperature(_nodeGridPos);
					ScheduleCacheChemicalData(_nodeGridPos);
				}
			}
		}
		
		b = !b;

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

	void SpreadChemicalsAndTemperature(Int2 _nodeGridPos) {
		Node _sourceRead = TryGetNode(_nodeGridPos, _atStartFrame: true);
		Node _sourceWrite = TryGetNode(_nodeGridPos);

		Node _neighborLRead = TryGetNode(_nodeGridPos + Int2.Left, true);
		Node _neighborTRead  = TryGetNode(_nodeGridPos + Int2.Up, true);
		Node _neighborRRead  = TryGetNode(_nodeGridPos + Int2.Right, true);
		Node _neighborBRead  = TryGetNode(_nodeGridPos + Int2.Down, true);

		List<Node> _sortedNodes = new List<Node>();

		int _sourceAmountTotal = _sourceRead.ChemicalContainer.GetAmountTotal();
		if (_neighborLRead != null && !_neighborLRead.IsWall) { _sortedNodes.Add(_neighborLRead); }
		if (_neighborTRead != null && !_neighborTRead.IsWall) { _sortedNodes.Add(_neighborTRead); }
		if (_neighborRRead != null && !_neighborRRead.IsWall) { _sortedNodes.Add(_neighborRRead); }
		if (_neighborBRead != null && !_neighborBRead.IsWall) { _sortedNodes.Add(_neighborBRead); }
		
		_sortedNodes.Sort((_x, _y) => {
			ChemicalContainer _xCC = _x.ChemicalContainer;
			ChemicalContainer _yCC = _y.ChemicalContainer;
			return _xCC.GetAmountTotal() < _yCC.GetAmountTotal() ? 0 : 1;
		});

		int _sourceAmountTotalRemaining = _sourceAmountTotal;
		for (int _nodeIndex = 0; _nodeIndex < _sortedNodes.Count; _nodeIndex++) {

			Node _targetRead = _sortedNodes[_nodeIndex];
			Node _targetWrite = TryGetNode(_targetRead.GridPos);
			int _targetAmountTotal = _targetRead.ChemicalContainer.GetAmountTotal();
			
			float _sourceReadTemp = _sourceRead.ChemicalContainer.Temperature;
			float _sourceWriteTemp = _sourceWrite.ChemicalContainer.Temperature;
			float _targetReadTemp = _targetRead.ChemicalContainer.Temperature;
			float _targetWriteTemp = _targetWrite.ChemicalContainer.Temperature;

			if (_sourceAmountTotal > _targetAmountTotal) {
				int _transferAmountTotal = GetTotalChemAmountToTransfer(ref _sourceAmountTotalRemaining, _sourceRead, _sourceWrite, _targetRead, _targetWrite);

				if (_transferAmountTotal > 0) {

					_targetWriteTemp = Mathf.Lerp(_targetWriteTemp, _sourceReadTemp, _transferAmountTotal / (float)_targetAmountTotal * 0.5f);

					
					for (int _chemIndex = 0; _chemIndex < _sourceRead.ChemicalContainer.Contents.Length; _chemIndex++) {
						Chemical.Blob _sourceChemBlob = _sourceRead.ChemicalContainer.Contents[_chemIndex];
						int _sourceChemAmount = _sourceChemBlob.Amount;
						if (_sourceChemAmount <= 0) {
							continue;
						}

						int _chemTransferAmount = Mathf.FloorToInt(_transferAmountTotal * (_sourceChemAmount /(float)_sourceAmountTotal) * _sourceChemBlob.GetAmountTransferRate());
						if (_chemTransferAmount <= 0) {
							continue;
						}

						_sourceWrite.ChemicalContainer.Contents[_chemIndex].SubtractAmount(_chemTransferAmount);
						_targetWrite.ChemicalContainer.Contents[_chemIndex].AddAmount(_chemTransferAmount);
					}
				}
			}
			
			if (_sourceAmountTotal > 0 && _targetAmountTotal > 0 && _sourceReadTemp > _targetReadTemp) {
				float _sourceTempLoss, _targetTempGain;
				GetTemperatureToTransfer(_sourceRead, _sourceWrite, _targetRead, _targetWrite, out _sourceTempLoss, out _targetTempGain);
				_sourceWriteTemp -= _sourceTempLoss;
				_targetWriteTemp += _targetTempGain;
			}
			
			_sourceWrite.ChemicalContainer.SetTemperature(_sourceWriteTemp);
			_targetWrite.ChemicalContainer.SetTemperature(_targetWriteTemp);
		}
	}

	static int GetTotalChemAmountToTransfer(ref int _amountRemainingAtSource, Node _sourceRead, Node _sourceWrite, Node _targetRead, Node _targetWrite) {
		if (_sourceRead == null || _sourceWrite == null) {
			return 0;
		}
		if (_targetRead == null || _targetWrite == null) {
			return 0;
		}

		int _transferAmount = GetChemicalAmountToTransfer(_sourceRead.ChemicalContainer, _targetRead.ChemicalContainer);
		_transferAmount = Mathf.Min(_transferAmount, _amountRemainingAtSource);

		if (_transferAmount < 0 || _transferAmount > _sourceRead.ChemicalContainer.MaxAmount) {
			Debug.LogError("Something wrong here: " + _transferAmount);
			return 0;
		}

		_amountRemainingAtSource -= _transferAmount;
		return _transferAmount;
	}

	static int GetChemicalAmountToTransfer(ChemicalContainer _source, ChemicalContainer _target) {
		int _totalAmountSource = Mathf.Clamp(_source.GetAmountTotal(), 0, _source.MaxAmount);
		int _totalAmountTarget = Mathf.Clamp(_target.GetAmountTotal(), 0, _target.MaxAmount);
		int _totalAmountDiff = _totalAmountSource - _totalAmountTarget;

		int _maxAmountAbleToGet = _totalAmountDiff;
		int _maxAmountAbleToSet = _target.MaxAmount - _totalAmountTarget;

		return Mathf.RoundToInt(0.5f * Mathf.Min(_maxAmountAbleToSet, _maxAmountAbleToGet));
	}
	
	static void GetTemperatureToTransfer(Node _sourceRead, Node _sourceWrite, Node _targetRead, Node _targetWrite, out float _sourceTempLoss, out float _targetTempGain) {
		_sourceTempLoss = 0;
		_targetTempGain = 0;
		
		if (_sourceRead == null || _sourceWrite == null) {
			return;
		}
		if (_targetRead == null || _targetWrite == null) {
			return;
		}
		if (_sourceRead.ChemicalContainer.Temperature <= _targetRead.ChemicalContainer.Temperature) {
			Debug.LogErrorFormat("{0}({1}) -> {2}({3})", _sourceRead.ChemicalContainer.Temperature, _sourceRead.GridPos, _targetRead.ChemicalContainer.Temperature, _targetRead.GridPos);
			return;
		}

		int _sourceAmountTotal = _sourceRead.ChemicalContainer.GetAmountTotal();
		int _targetAmountTotal = _targetRead.ChemicalContainer.GetAmountTotal();

		float _diffMod = _targetAmountTotal / (float)_sourceAmountTotal;

		float _temperatureDiff = _sourceRead.ChemicalContainer.Temperature - _targetRead.ChemicalContainer.Temperature;
		float _sourceTransferRate = _sourceRead.ChemicalContainer.GetTemperatureTransferRate();
		float _targetTransferRate = _targetRead.ChemicalContainer.GetTemperatureTransferRate();
		float _temperatureTransferRate =  _sourceTransferRate * _targetTransferRate;
		_sourceTempLoss = _temperatureDiff * _diffMod * 0.5f * _temperatureTransferRate;
		_targetTempGain = _temperatureDiff * _diffMod * 0.5f * _temperatureTransferRate;

		// Debug.LogFormat("{0} -> {1}, {2} -> {3}: {4}, {5}", _sourceRead.ChemicalContainer.Temperature, _targetRead.ChemicalContainer.Temperature, _sourceRead.ChemicalContainer.GetAmountTotal(), _targetRead.ChemicalContainer.GetAmountTotal(), _sourceTempLoss, _targetTempGain);
	}
}