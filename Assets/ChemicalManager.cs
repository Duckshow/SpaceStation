using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChemicalManager : Singleton<ChemicalManager> {

	[SerializeField] private ChemicalsAsset chemicalsAsset;
	[SerializeField] private Material gridMaterial;

	public override bool IsUsingAwakeEarly() { return true; }
	public override void AwakeEarly() {
		Chemical[] _allChemicals = chemicalsAsset.GetAllChemicals();

		List<float> _allChemicalsColorIndexSolid = new List<float>(_allChemicals.Length);
		List<float> _allChemicalsColorIndexLiquid = new List<float>(_allChemicals.Length);
		List<float> _allChemicalsColorIndexGas = new List<float>(_allChemicals.Length);
		List<float> _allChemicalsFreezingPoint = new List<float>(_allChemicals.Length);
		List<float> _allChemicalsBoilingPoint = new List<float>(_allChemicals.Length);

		for(int i = 0; i < _allChemicals.Length; i++) {
			Chemical _chemical = _allChemicals[i];
			_allChemicalsColorIndexSolid.Add(_chemical.GetColorIndexSolid());
			_allChemicalsColorIndexLiquid.Add(_chemical.GetColorIndexLiquid());
			_allChemicalsColorIndexGas.Add(_chemical.GetColorIndexGas());
			_allChemicalsFreezingPoint.Add(_chemical.GetFreezingPoint());
			_allChemicalsBoilingPoint.Add(_chemical.GetBoilingPoint());
		}

		gridMaterial.SetFloatArray("allChemicalsColorIndexSolid", _allChemicalsColorIndexSolid);
		gridMaterial.SetFloatArray("allChemicalsColorIndexLiquid", _allChemicalsColorIndexLiquid);
		gridMaterial.SetFloatArray("allChemicalsColorIndexGas", _allChemicalsColorIndexGas);
		gridMaterial.SetFloatArray("allChemicalsFreezingPoint", _allChemicalsFreezingPoint);
		gridMaterial.SetFloatArray("allChemicalsBoilingPoint", _allChemicalsBoilingPoint);
	}

	public Chemical[] GetAllChemicals() {
		return chemicalsAsset.GetAllChemicals();
	}
}