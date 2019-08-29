using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ChemicalContainer : ICloneable<ChemicalContainer> {

	public const int MAX_POSSIBLE_AMOUNT = 10000;

	public const float TEMPERATURE_INCANDESCENCE_MIN    = 525.0f;
	public const float TEMPERATURE_INCANDESCENCE_RED    = 1025.0f;
	public const float TEMPERATURE_INCANDESCENCE_YELLOW = 1525.0f;
	public const float TEMPERATURE_INCANDESCENCE_WHITE  = 2025.0f;
	
	public Chemical.Blob[] Contents;
	public int MaxAmount { get; private set; }
	public float Temperature { get; private set; }
	public float TemperatureLastFrame { get; private set; }
	private int lastFrameSetTemperature;

	public ChemicalContainer Clone() {
		ChemicalContainer cc = new ChemicalContainer(MaxAmount);
		cc.Contents = new Chemical.Blob[Contents.Length];
		for (int i = 0; i < Contents.Length; i++) {
			cc.Contents[i] = Contents[i].Clone();
		}
		cc.Temperature = Temperature;
		return cc;
	}

	public ChemicalContainer(int _maxAmount) {
		MaxAmount = Mathf.Min(_maxAmount, MAX_POSSIBLE_AMOUNT);

		Chemical[] _allChemicals = ChemicalManager.GetInstance().GetAllChemicals();
		Contents = new Chemical.Blob[_allChemicals.Length];
		for(int i = 0; i < Contents.Length; i++) {
			Contents[i] = new Chemical.Blob(_allChemicals[i], this);
		}
	}

	public void SetStartValues(int _amount, int _temperature) { // TODO: remove this
		Temperature = _temperature;
		Contents[0].SetAmount(_amount);
		return;
		int _amount_2 = Random.Range(0, _amount);
		int _amount_1 = Random.Range(0, _amount - _amount_2);
		int _amount_0 = _amount - _amount_2 - _amount_1;
		
		Contents[0].SetAmount(_amount_0);
		Contents[1].SetAmount(_amount_1);
		Contents[2].SetAmount(_amount_2);
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

	public float GetTemperatureTransferRate() {
		float _transferRate = 0.0f;

		int _amountTotal = GetAmountTotal();
		if (_amountTotal > 0.0f) {
			for (int i = 0; i < Contents.Length; i++) {
				_transferRate += Contents[i].Amount / (float)_amountTotal * Contents[i].GetTemperatureTransferRate();
			}
		}
		

		return Mathf.Clamp01(_transferRate);
	}
	
	public bool IsIncandescent(bool _onLastFrame = false) {
		return (_onLastFrame ? TemperatureLastFrame : Temperature) >= TEMPERATURE_INCANDESCENCE_MIN;
	}
	
	public bool HasLostIncandescence() {
		return IsIncandescent(_onLastFrame: true) && IsIncandescent(_onLastFrame: false);
	}

	public void SetTemperature(float _temperature) {
		if (Time.frameCount > lastFrameSetTemperature) {
			TemperatureLastFrame = Temperature;
		}
		lastFrameSetTemperature = Time.frameCount;

		Temperature = Mathf.Clamp(_temperature, 0, Chemical.MAX_TEMPERATURE - 1);
	}
	
	public Color GetIncandescence() {
		if (!IsIncandescent()) {
			return Color.clear;
		}

		float _progressToRed = (Temperature - TEMPERATURE_INCANDESCENCE_MIN) / (TEMPERATURE_INCANDESCENCE_RED - TEMPERATURE_INCANDESCENCE_MIN);
		float _progressToYellow = (Temperature - TEMPERATURE_INCANDESCENCE_RED) / (TEMPERATURE_INCANDESCENCE_YELLOW - TEMPERATURE_INCANDESCENCE_RED);
		float _progressToWhite = (Temperature - TEMPERATURE_INCANDESCENCE_YELLOW) / (TEMPERATURE_INCANDESCENCE_WHITE - TEMPERATURE_INCANDESCENCE_YELLOW);
			
		Color _incandescence = new Color();

		if (_progressToWhite > 0.0f) {
			_incandescence = Color.Lerp(Color.yellow, Color.white, _progressToWhite);
		}
		else if (_progressToYellow > 0.0f) {
			_incandescence = Color.Lerp(Color.red, Color.yellow, _progressToYellow);
		}
		else if (_progressToRed > 0.0f) {
			_incandescence = Color.Lerp(Color.clear, Color.red, _progressToRed);
		}

		return _incandescence;
	}
}
