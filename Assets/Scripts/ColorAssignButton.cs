using UnityEngine;
using UnityEngine.UI;

public class ColorAssignButton : MonoBehaviour {

	public static ColorAssignButton sActiveButton;

	[SerializeField] private GameObject ColorPalletteObject;
	[SerializeField] private int ColorChannel;
	[System.NonSerialized] public Toggle MyToggle;
	private Image myImage;


	void Awake(){
		MyToggle = GetComponent<Toggle> ();
		myImage = GetComponent<Image> ();
		ColorPalletteObject.SetActive (false);
	}
	void OnEnable() {
		MyToggle.onValueChanged.AddListener(OnToggleValueChanged);
	}
	void OnDisable() {
		MyToggle.onValueChanged.RemoveListener(OnToggleValueChanged);
	}
	void Start(){
		AssignColor (0);
	}

	public void OnToggleValueChanged(bool _b) {
		if (_b) {
			sActiveButton = this;
			ColorPalletteObject.SetActive (true);
		} 
		else if(sActiveButton == this) {
			sActiveButton = null;
			ColorPalletteObject.SetActive (false);
		}
	}
	public void AssignColor(byte _colorIndex){
		ColoringTool.AssignColorIndex (ColorChannel, _colorIndex);
		myImage.color = Mouse.Instance.Coloring.AllColors [_colorIndex];
		MyToggle.isOn = false;
	}
}
