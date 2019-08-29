using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using UnityEngine;
using UnityEngine.Collections;

[System.Serializable]
public class Chemical {

	public const float MAX_TEMPERATURE = 10000.0f;

	public enum State { Solid, Liquid, Gas, Plasma }
	public const float TRANSFER_RATE_MOD = 0.5f;
	public const float TRANSFER_RATE_SOLID = 0.0f;
	public const float TRANSFER_RATE_LIQUID = 0.5f;
	public const float TRANSFER_RATE_GAS = 0.5f;
	public const float TRANSFER_RATE_PLASMA = 0.5f;

	public const float TEMPERATURE_TRANSFER_RATE_MOD = 1.0f;
	public const float TEMPERATURE_TRANSFER_RATE_SOLID = 0.5f;
	public const float TEMPERATURE_TRANSFER_RATE_LIQUID = 0.35f;
	public const float TEMPERATURE_TRANSFER_RATE_GAS = 0.25f;
	public const float TEMPERATURE_TRANSFER_RATE_PLASMA = 0.15f;
	
	private const float STATE_TRANSITION_UP     = 0.8f;

	[SerializeField] private byte colorIndexSolid;
	[SerializeField] private byte colorIndexLiquid;
	[SerializeField] private byte colorIndexGas;
	[SerializeField] private byte colorIndexPlasma;
	[SerializeField] private int freezingPoint;
	[SerializeField] private int boilingPoint;
	[SerializeField] private int ionizationPoint;

	public int GetColorIndexSolid() { return colorIndexSolid; }
	public int GetColorIndexLiquid() { return colorIndexLiquid; }
	public int GetColorIndexGas() { return colorIndexGas; }
	public int GetFreezingPoint() { return freezingPoint; }
	public int GetBoilingPoint() { return boilingPoint; }
	public int GetIonizationPoint() { return ionizationPoint; }

	public struct Blob : ICloneable<Blob> {
		public ChemicalContainer Container  { get; private set; }
		public Chemical Chemical  { get; private set; }
		public int Amount  { get; private set; }

		public Blob(Chemical _chemical, ChemicalContainer _container) {
			Container = _container;
			Chemical = _chemical;
			Amount = 0;
		}

		public Blob Clone() {
			Blob _clone = new Blob(Chemical, Container);
			_clone.Amount = Amount;
			return _clone;
		}

		public void SetAmount(int _amount) {
			Amount = _amount;
		}
		
		public void AddAmount(int _amount) {
			Amount += _amount;
		}
		
		public void SubtractAmount(int _amount) {
			Amount -= _amount;
		}

		public float GetAmountTransferRate() {
			switch(GetState()) {
				case State.Solid: return TRANSFER_RATE_MOD * TRANSFER_RATE_SOLID;
				case State.Liquid: return TRANSFER_RATE_MOD * TRANSFER_RATE_LIQUID;
				case State.Gas: return TRANSFER_RATE_MOD * TRANSFER_RATE_GAS;
				case State.Plasma: return TRANSFER_RATE_MOD * TRANSFER_RATE_PLASMA;
				default:
					throw new System.NotImplementedException();
			}
		}

		public float GetTemperatureTransferRate() {
			switch(GetState()) {
				case State.Solid: return TEMPERATURE_TRANSFER_RATE_MOD * TEMPERATURE_TRANSFER_RATE_SOLID;
				case State.Liquid: return TEMPERATURE_TRANSFER_RATE_MOD * TEMPERATURE_TRANSFER_RATE_LIQUID;
				case State.Gas: return TEMPERATURE_TRANSFER_RATE_MOD * TEMPERATURE_TRANSFER_RATE_GAS;
				case State.Plasma: return TEMPERATURE_TRANSFER_RATE_MOD * TEMPERATURE_TRANSFER_RATE_PLASMA;
				default:
					throw new System.NotImplementedException();
			}
		}

		public State GetState() {
			if(Container.Temperature >= Chemical.ionizationPoint) {
				return State.Plasma;
			}
			if(Container.Temperature >= Chemical.boilingPoint) {
				return State.Gas;
			}
			if(Container.Temperature >= Chemical.freezingPoint) {
				return State.Liquid;
			}

			return State.Solid;
		}

		public float GetStateAsFloat() {
			float _prevStateChangeTemp;
			float _nextStateChangeTemp;

			State _state = GetState();
			switch(_state) {
				case State.Solid:
					_prevStateChangeTemp = 0.0f;
					_nextStateChangeTemp = Chemical.freezingPoint;
					break;
				case State.Liquid:
					_prevStateChangeTemp = Chemical.freezingPoint;
					_nextStateChangeTemp = Chemical.boilingPoint;
					break;
				case State.Gas:
					_prevStateChangeTemp = Chemical.boilingPoint;
					_nextStateChangeTemp = Chemical.ionizationPoint;
					break;
				case State.Plasma:
					_prevStateChangeTemp = Chemical.ionizationPoint;
					_nextStateChangeTemp = MAX_TEMPERATURE;
					break;
				default:
					throw new System.NotImplementedException();
			}
			
			// Debug.Log ( Container.Temperature + " - " + _prevStateChangeTemp + " = " + (Container.Temperature - _prevStateChangeTemp));
			// float _stateChangeMaxTempFraction = _stateChangeTemp / (float)MAX_TEMPERATURE;
			// Debug.Log((int)_state + " + " + Container.Temperature + " / " + _stateChangeMaxTempFraction + " = " + ((int)_state + Container.Temperature / _stateChangeMaxTempFraction));

			float _progressToNext = (Container.Temperature - _prevStateChangeTemp) / (float)(_nextStateChangeTemp - _prevStateChangeTemp);
			
			return(int)_state + _progressToNext; // (Container.Temperature / (float)MAX_TEMPERATURE) / _stateChangeMaxTempFraction;
		}

		public Color GetColorIndicesForStates() {
			float _colorCount = (float)ColorManager.COLOR_COUNT - 1;
			float _colorIndexSolid = Chemical.colorIndexSolid / _colorCount;
			float _colorIndexLiquid = Chemical.colorIndexLiquid / _colorCount;
			float _colorIndexGas = Chemical.colorIndexGas / _colorCount;
			float _colorIndexPlasma = Chemical.colorIndexPlasma / _colorCount;
			return new Color(_colorIndexSolid, _colorIndexLiquid, _colorIndexGas, _colorIndexPlasma);
			
			// Color c0, c1;
			float _stateExact = GetStateAsFloat();
			int _state = Mathf.FloorToInt(_stateExact);
			float _stateDecimals = _stateExact - _state;
			
			// if(_stateExact < 1.0f) {
			// 	c0 = ColorManager.GetColor(Chemical.colorIndexSolid);
			// 	c1 = ColorManager.GetColor(Chemical.colorIndexLiquid);
			// }
			// else if(_stateExact < 2.0f) {
			// 	c0 = ColorManager.GetColor(Chemical.colorIndexLiquid);
			// 	c1 = ColorManager.GetColor(Chemical.colorIndexGas);
			// }
			// else if(_stateExact < 3.0f) {
			// 	c0 = ColorManager.GetColor(Chemical.colorIndexGas);
			// 	c1 = ColorManager.GetColor(Chemical.colorIndexPlasma);
			// }
			// else {
			// 	c0 = ColorManager.GetColor(Chemical.colorIndexPlasma);
			// 	c1 = c0;
			// }
			
			// return Color.Lerp(c0, c1, (_state - Mathf.Floor(_state) - STATE_TRANSITION_UP) / (1.0f - STATE_TRANSITION_UP));

			int _colorIndexLowerState = 0;
			int colorIndexCurrentState = 0;
			int _colorIndexHigherState = 0;
			
			if(_state == 0) {
				_colorIndexLowerState   = Chemical.colorIndexSolid;
				colorIndexCurrentState = Chemical.colorIndexSolid;
				_colorIndexHigherState  = Chemical.colorIndexLiquid;
			}
			else if(_state == 1) {
				_colorIndexLowerState   = Chemical.colorIndexSolid;
				colorIndexCurrentState = Chemical.colorIndexLiquid;
				_colorIndexHigherState  = Chemical.colorIndexGas;
			}
			else if(_state == 2) {
				_colorIndexLowerState   = Chemical.colorIndexLiquid;
				colorIndexCurrentState = Chemical.colorIndexGas;
				_colorIndexHigherState  = Chemical.colorIndexPlasma;
			}
			else if(_state == 3){
				_colorIndexLowerState   = Chemical.colorIndexGas;
				colorIndexCurrentState = Chemical.colorIndexPlasma;
				_colorIndexHigherState  = Chemical.colorIndexPlasma;
			}
			else {
				Debug.LogError("Weird state on chem here!");
			}

			int _colorIndexStart, _colorIndexTarget;
			float _t;
			
			if (_stateDecimals < 0.5f) {
				_colorIndexStart = _colorIndexLowerState;
				_colorIndexTarget = colorIndexCurrentState;
				_t = _stateDecimals / 0.5f;
			}
			else {
				_colorIndexStart  = colorIndexCurrentState;
				_colorIndexTarget = _colorIndexHigherState;
				_t = (_stateDecimals - 0.5f) / 0.5f;
			}
			
			return Color.Lerp(ColorManager.GetColor(_colorIndexStart), ColorManager.GetColor(_colorIndexTarget), _t);
		}
	}
}