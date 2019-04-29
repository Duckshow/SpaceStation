using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Chemical {

	public enum ID { Water }

	// TODO: would be nice if chemicals were defined in an asset
	public static readonly Chemical WATER = new Chemical(_freezingPoint: 273, _boilingPoint: 373);

	public enum State { Solid, Liquid, Gas }
	public const float TRANSFER_RATE_MOD = 0.1f;
	public const float TRANSFER_RATE_SOLID = 0.0f;
	public const float TRANSFER_RATE_LIQUID = 0.2f;
	public const float TRANSFER_RATE_GAS = 0.5f;

	public const float TEMPERATURE_TRANSFER_RATE_MOD = 1.0f;
	public const float TEMPERATURE_TRANSFER_RATE_SOLID = 0.5f;
	public const float TEMPERATURE_TRANSFER_RATE_LIQUID = 0.2f;
	public const float TEMPERATURE_TRANSFER_RATE_GAS = 0.1f;

	public int FreezingPoint { get; private set; }
	public int BoilingPoint { get; private set; }

	public Chemical(int _freezingPoint, int _boilingPoint) {
		FreezingPoint = _freezingPoint;
		BoilingPoint = _boilingPoint;
	}

	public struct Blob {
		public ChemicalContainer Container  { get; private set; }
		public Chemical Chemical  { get; private set; }
		public int Amount  { get; private set; }

		public Blob(Chemical _chemical, ChemicalContainer _container) {
			Chemical = _chemical;
			Container = _container;
			Amount = 0;
		}

		public Blob(Chemical _chemical, ChemicalContainer _container, int _amount) {
			Chemical = _chemical;
			Container = _container;
			Amount = _amount;
		}

		public void SetAmount(int _amount) {
			Amount = _amount;
		}

		public int GetAmountTransferablePerFrame() {
			return Mathf.RoundToInt(Amount * GetTransferRate());
		}

		public float GetTransferRate() {
			switch(GetState()) {
				case Chemical.State.Solid:
					return TRANSFER_RATE_MOD * Chemical.TRANSFER_RATE_SOLID;
				case Chemical.State.Liquid:
					return TRANSFER_RATE_MOD * Chemical.TRANSFER_RATE_LIQUID;
				case Chemical.State.Gas:
					return TRANSFER_RATE_MOD * Chemical.TRANSFER_RATE_GAS;
				default:
					throw new System.NotImplementedException();
			}
		}

		public float GetTemperatureTransferRate() {
			switch(GetState()) {
				case Chemical.State.Solid:
					return(Amount / 100.0f) * TEMPERATURE_TRANSFER_RATE_MOD * Chemical.TEMPERATURE_TRANSFER_RATE_SOLID;
				case Chemical.State.Liquid:
					return(Amount / 100.0f) * TEMPERATURE_TRANSFER_RATE_MOD * Chemical.TEMPERATURE_TRANSFER_RATE_LIQUID;
				case Chemical.State.Gas:
					return(Amount / 100.0f) * TEMPERATURE_TRANSFER_RATE_MOD * Chemical.TEMPERATURE_TRANSFER_RATE_GAS;
				default:
					throw new System.NotImplementedException();
			}
		}

		public Chemical.State GetState() {
			if(Container.Temperature <= Chemical.FreezingPoint) {
				return Chemical.State.Solid;
			}
			else if(Container.Temperature >= Chemical.BoilingPoint) {
				return Chemical.State.Gas;
			}
			else {
				return Chemical.State.Liquid;
			}
		}
	}
}