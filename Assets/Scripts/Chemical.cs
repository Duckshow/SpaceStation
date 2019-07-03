using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Chemical {

	public const float MAX_TEMPERATURE = 10000.0f;

	public enum State { Solid, Liquid, Gas, Plasma }
	public const float TRANSFER_RATE_MOD = 1.0f;
	public const float TRANSFER_RATE_SOLID = 0.0f;
	public const float TRANSFER_RATE_LIQUID = 0.25f;
	public const float TRANSFER_RATE_GAS = 0.5f;
	public const float TRANSFER_RATE_PLASMA = 0.5f;

	public const float TEMPERATURE_TRANSFER_RATE_MOD = 0.1f;
	public const float TEMPERATURE_TRANSFER_RATE_SOLID = 0.5f;
	public const float TEMPERATURE_TRANSFER_RATE_LIQUID = 0.2f;
	public const float TEMPERATURE_TRANSFER_RATE_GAS = 0.1f;
	public const float TEMPERATURE_TRANSFER_RATE_PLASMA = 0.1f;

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

	public struct Blob {
		public ChemicalContainer Container  { get; private set; }
		public Chemical Chemical  { get; private set; }
		public int Amount  { get; private set; }

		public Blob(Chemical _chemical, ChemicalContainer _container) {
			Container = _container;
			Chemical = _chemical;
			Amount = 0;
		}

		public void SetAmount(int _amount) {
			Amount = _amount;
		}

		public int GetAmountTransferablePerFrame() {
			float _transferRate = 0.0f;

			switch(GetState()) {
				case State.Solid:
					_transferRate = TRANSFER_RATE_MOD * Chemical.TRANSFER_RATE_SOLID;
					break;
				case State.Liquid:
					_transferRate = TRANSFER_RATE_MOD * Chemical.TRANSFER_RATE_LIQUID;
					break;
				case State.Gas:
					_transferRate = TRANSFER_RATE_MOD * Chemical.TRANSFER_RATE_GAS;
					break;
				case State.Plasma:
					_transferRate = TRANSFER_RATE_MOD * Chemical.TRANSFER_RATE_PLASMA;
					break;
				default:
					throw new System.NotImplementedException();
			}

			return Mathf.RoundToInt(Amount * _transferRate);
		}

		public float GetTemperatureTransferRate() { // TODO: why isn't this used?
			float _transferRateMod = 0.0f;

			switch(GetState()) {
				case State.Solid:
					_transferRateMod = Chemical.TEMPERATURE_TRANSFER_RATE_SOLID;
					break;
				case State.Liquid:
					_transferRateMod = Chemical.TEMPERATURE_TRANSFER_RATE_LIQUID;
					break;
				case State.Gas:
					_transferRateMod = Chemical.TEMPERATURE_TRANSFER_RATE_GAS;
					break;
				case State.Plasma:
					_transferRateMod = Chemical.TEMPERATURE_TRANSFER_RATE_PLASMA;
					break;
				default:
					throw new System.NotImplementedException();
			}

			return(Amount / 100.0f) * TEMPERATURE_TRANSFER_RATE_MOD * _transferRateMod;
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
			return(int)_state +(Container.Temperature - _prevStateChangeTemp) /(float)(_nextStateChangeTemp - _prevStateChangeTemp); // (Container.Temperature / (float)MAX_TEMPERATURE) / _stateChangeMaxTempFraction;

		}

		public Color GetCurrentColor() {
			Color c0, c1;
			float _state = GetStateAsFloat();

			if(_state < 1.0f) {
				c0 = ColorManager.GetColor(Chemical.colorIndexSolid);
				c1 = ColorManager.GetColor(Chemical.colorIndexLiquid);
			}
			else if(_state < 2.0f) {
				c0 = ColorManager.GetColor(Chemical.colorIndexLiquid);
				c1 = ColorManager.GetColor(Chemical.colorIndexGas);
			}
			else if(_state < 3.0f) {
				c0 = ColorManager.GetColor(Chemical.colorIndexGas);
				c1 = ColorManager.GetColor(Chemical.colorIndexPlasma);
			}
			else {
				c0 = ColorManager.GetColor(Chemical.colorIndexPlasma);
				c1 = ColorManager.GetColor(ColorManager.GetColorIndex((int)ColorManager.ColorName.White));
			}

			return Color.Lerp(c0, c1, _state - Mathf.Floor(_state));
		}
	}
}