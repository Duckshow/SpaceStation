using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChemicalContainer {
	public Chemical.Blob[] Contents;
	public int MaxAmount { get; private set; }
	public float Temperature {
		get { return temperature; }
		set { temperature = Mathf.Clamp(value, 0, Chemical.MAX_TEMPERATURE - 1); }
	}
	private float temperature;

	public ChemicalContainer(int _maxAmount) {
		MaxAmount = _maxAmount;

		Chemical[] _allChemicals = ChemicalManager.GetInstance().GetAllChemicals();
		Contents = new Chemical.Blob[_allChemicals.Length];
		for(int i = 0; i < Contents.Length; i++) {
			Contents[i] = new Chemical.Blob(_allChemicals[i], this);
		}
	}

	public void SetStartValues(int _amount, int _temperature) { // TODO: remove this
		Temperature = _temperature;
		Contents[0].SetAmount(_amount);
	}

	public Color32 GetColor() {
		// Color32 _c = Color32.Lerp(Color.cyan, Color.red, Temperature / 1000.0f);
		// Color32 _c = new Color32((byte)Mathf.Lerp(0, 255, Temperature / 1000.0f), 0, 0, 0);
		// _c.a =(byte)Mathf.Lerp(0, 255, Water.Amount /(float)MaxAmount);
		return Color.magenta;
	}

	public int GetAmountTotal() {
		int _total = 0;

		for(int i = 0; i < Contents.Length; i++) {
			_total += Contents[i].Amount;
		}

		return _total;
	}

	public void Add(ChemicalSpread _spread) {
		Temperature += _spread.Temperature;

		for(int i = 0; i < Contents.Length; i++) {
			Contents[i].SetAmount(Contents[i].Amount + _spread.Contents[i].Amount);
		}
	}

	public void Subtract(ChemicalSpread _spread) {
		Temperature -= _spread.Temperature * Mathf.Max(0.0f, _spread.TemperatureLossMod);

		for(int i = 0; i < Contents.Length; i++) {
			Contents[i].SetAmount(Contents[i].Amount - _spread.Contents[i].Amount);
		}
	}

	public void GetThreeMostPrevalentChemicals(out Chemical.Blob _chem0, out Chemical.Blob _chem1, out Chemical.Blob _chem2) {
		List<Chemical.Blob> _chems = new List<Chemical.Blob>(Contents.Length);
		for(int i = 0; i < Contents.Length; i++) {
			_chems.Add(Contents[i]);
		}

		_chems.Sort((x, y) => x.Amount.CompareTo(y.Amount));

		_chem0 = _chems[0];
		_chem1 = _chems[1];
		_chem2 = _chems[2];
	}
}

public class ChemicalSpread : ChemicalContainer {
	public float TemperatureLossMod {
		get { return temperatureLossMod; }
		set { temperatureLossMod = Mathf.Clamp01(value); }
	}
	private float temperatureLossMod;

	public ChemicalSpread(int _maxAmount) : base(_maxAmount) { }
}