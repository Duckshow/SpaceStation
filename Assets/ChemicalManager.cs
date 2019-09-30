using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChemicalManager : Singleton<ChemicalManager> {

	[SerializeField] private ChemicalsAsset chemicalsAsset;
	[SerializeField] private Material gridMaterial;

	public override bool IsUsingAwakeEarly() { return true; }
	public override void AwakeEarly() {
		Chemical[] _allChemicals = chemicalsAsset.GetAllChemicals();

		List<float> _allChemicalsColorIndex = new List<float>(_allChemicals.Length);
		List<float> _allChemicalsFreezingPoint = new List<float>(_allChemicals.Length);
		List<float> _allChemicalsBoilingPoint = new List<float>(_allChemicals.Length);

		for(int i = 0; i < _allChemicals.Length; i++) {
			Chemical _chemical = _allChemicals[i];
			_allChemicalsColorIndex.Add(_chemical.GetColorIndex());
			_allChemicalsFreezingPoint.Add(_chemical.GetFreezingPoint());
			_allChemicalsBoilingPoint.Add(_chemical.GetBoilingPoint());
		}

		gridMaterial.SetFloatArray("allChemicalsColorIndex", _allChemicalsColorIndex);
		gridMaterial.SetFloatArray("allChemicalsFreezingPoint", _allChemicalsFreezingPoint);
		gridMaterial.SetFloatArray("allChemicalsBoilingPoint", _allChemicalsBoilingPoint);
	}

	public Chemical[] GetAllChemicals() {
		return chemicalsAsset.GetAllChemicals();
	}
}