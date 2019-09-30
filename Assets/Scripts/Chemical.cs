using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using UnityEngine;
using UnityEngine.Collections;

[System.Serializable]
public class Chemical {

	public enum ChemicalID { H, O, H20, C, CO2, CH4, Fe, Misc }

	public const float MAX_TEMPERATURE = 10000.0f;

	public enum State { Solid, Liquid, Gas }
	public const float TRANSFER_RATE_MOD = 0.5f;
	public const float TRANSFER_RATE_SOLID = 0.0f;
	public const float TRANSFER_RATE_LIQUID = 0.5f;
	public const float TRANSFER_RATE_GAS = 0.5f;

	public const float TEMPERATURE_TRANSFER_RATE_MOD = 1.0f;
	public const float TEMPERATURE_TRANSFER_RATE_SOLID = 0.5f;
	public const float TEMPERATURE_TRANSFER_RATE_LIQUID = 0.35f;
	public const float TEMPERATURE_TRANSFER_RATE_GAS = 0.25f;

	[SerializeField] private byte colorIndex;
	[SerializeField] private int freezingPoint;
	[SerializeField] private int boilingPoint;

	public int GetColorIndex() { return colorIndex; }
	public int GetFreezingPoint() { return freezingPoint; }
	public int GetBoilingPoint() { return boilingPoint; }

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
				default:
					throw new System.NotImplementedException();
			}
		}

		public float GetTemperatureTransferRate() {
			switch(GetState()) {
				case State.Solid: return TEMPERATURE_TRANSFER_RATE_MOD * TEMPERATURE_TRANSFER_RATE_SOLID;
				case State.Liquid: return TEMPERATURE_TRANSFER_RATE_MOD * TEMPERATURE_TRANSFER_RATE_LIQUID;
				case State.Gas: return TEMPERATURE_TRANSFER_RATE_MOD * TEMPERATURE_TRANSFER_RATE_GAS;
				default:
					throw new System.NotImplementedException();
			}
		}

		public State GetState() {
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
	}
}