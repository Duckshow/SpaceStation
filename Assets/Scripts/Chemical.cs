using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Chemical {

	public const float MAX_TEMPERATURE = 1000.0f;

	public enum State { Solid, Liquid, Gas, Plasma }
	public const float TRANSFER_RATE_MOD = 0.1f;
	public const float TRANSFER_RATE_SOLID = 0.0f;
	public const float TRANSFER_RATE_LIQUID = 0.2f;
	public const float TRANSFER_RATE_GAS = 0.5f;
	public const float TRANSFER_RATE_PLASMA = 0.5f;

	public const float TEMPERATURE_TRANSFER_RATE_MOD = 1.0f;
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
			Chemical = _chemical;
			Container = _container;
			Amount = 0;
		}

		public void SetAmount(int _amount) {
			Amount = _amount;
		}

		public int GetAmountTransferablePerFrame() {
			return Mathf.RoundToInt(Amount * GetTransferRate());
		}

		public float GetTransferRate() {
			switch(GetState()) {
				case State.Solid:
					return TRANSFER_RATE_MOD * Chemical.TRANSFER_RATE_SOLID;
				case State.Liquid:
					return TRANSFER_RATE_MOD * Chemical.TRANSFER_RATE_LIQUID;
				case State.Gas:
					return TRANSFER_RATE_MOD * Chemical.TRANSFER_RATE_GAS;
				default:
					throw new System.NotImplementedException();
			}
		}

		public float GetTemperatureTransferRate() { // TODO: why isn't this used?
			switch(GetState()) {
				case State.Solid:
					return(Amount / 100.0f) * TEMPERATURE_TRANSFER_RATE_MOD * Chemical.TEMPERATURE_TRANSFER_RATE_SOLID;
				case State.Liquid:
					return(Amount / 100.0f) * TEMPERATURE_TRANSFER_RATE_MOD * Chemical.TEMPERATURE_TRANSFER_RATE_LIQUID;
				case State.Gas:
					return(Amount / 100.0f) * TEMPERATURE_TRANSFER_RATE_MOD * Chemical.TEMPERATURE_TRANSFER_RATE_GAS;
				default:
					throw new System.NotImplementedException();
			}
		}

		public State GetState() {
			if(Container.Temperature <= Chemical.freezingPoint) {
				return State.Solid;
			}
			if(Container.Temperature < Chemical.boilingPoint) {
				return State.Liquid;
			}
			if(Container.Temperature >= Chemical.boilingPoint) {
				return State.Gas;
			}

			return State.Plasma;
		}

		public int GetCurrentColor() {
			switch(GetState()) { // TODO: should have a float that constantly lerps toward the current state or something, so transitions aren't instanteneous
				case State.Solid:	return Chemical.colorIndexSolid;
				case State.Liquid:	return Chemical.colorIndexLiquid;
				case State.Gas:	return Chemical.colorIndexGas;
				case State.Plasma:	return Chemical.colorIndexPlasma;
				default:
					throw new System.NotImplementedException(GetState() + " hasn't been properly implemented yet!");
			}
		}
	}
}