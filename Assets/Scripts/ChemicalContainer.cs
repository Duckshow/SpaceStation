using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChemicalContainer {
	public int MaxAmount { get; private set; }
	public float Temperature { get; private set; }
	public Chemical.Blob Water;

	public ChemicalContainer(int _maxAmount) {
		MaxAmount = _maxAmount;
		Water = new Chemical.Blob(Chemical.WATER, this);
	}

	public void SetStartValues(int _amount, int _temperature) {
		Temperature = _temperature;
		Water.SetAmount(_amount);
	}

	public void SetTemperature(float _temperature) {
		Temperature = _temperature;
	}

	public Color32 GetColor() {
		// Color32 _c = Color32.Lerp(Color.cyan, Color.red, Temperature / 1000.0f);
		Color32 _c = new Color32((byte)Mathf.Lerp(0, 255, Temperature / 1000.0f), 0, 0, 0);
		_c.a =(byte)Mathf.Lerp(0, 255, Water.Amount /(float)MaxAmount);
		return _c;
	}

	public int GetAmountTotal() {
		int _total = 0;

		_total += Water.Amount;

		return _total;
	}

	public Chemical.Blob GetChemical(Chemical.ID _id) {
		switch(_id) {
			case Chemical.ID.Water:
				return Water;
			default:
				throw new System.NotImplementedException();
		}
	}

	public void SetChemical(Chemical.ID _id, Chemical.Blob _blob) {
		switch(_id) {
			case Chemical.ID.Water:
				Water = _blob;
				break;
			default:
				throw new System.NotImplementedException();
		}
	}

	public float GetMaxPossibleAmountToTransfer() {
		float _amount = 0.0f;
		System.Array _ids = System.Enum.GetValues(typeof(Chemical.ID));
		foreach(Chemical.ID _id in _ids) {
			_amount += GetChemical(_id).GetAmountTransferablePerFrame();
		}

		return _amount;
	}

	public void Add(ChemicalContainer _otherChemicalContainer) {
		Temperature += _otherChemicalContainer.Temperature;
		Water.SetAmount(Water.Amount + _otherChemicalContainer.Water.Amount);
	}

	public void Subtract(ChemicalContainer _otherChemicalContainer) {
		Water.SetAmount(Water.Amount - _otherChemicalContainer.Water.Amount);
	}
}