using UnityEngine;

[CreateAssetMenu(fileName = "New ChemicalsAsset.asset", menuName = "New ChemicalsAsset")]
public class ChemicalsAsset : ScriptableObject {
	[SerializeField] private Chemical[] allChemicals;

	public Chemical[] GetAllChemicals() {
		return allChemicals;
	}
}