using UnityEngine;
using UnityEngine.UI;

public class ColorAssignButton : MonoBehaviour {

	public static ColorAssignButton sActiveButton;

	[SerializeField] private GameObject ColorPalletteObject;
	[SerializeField] private int ColorChannel;
	[System.NonSerialized] public Toggle MyToggle;
	[System.NonSerialized] public byte AssignedColorIndex;
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
		AssignColor (5);
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

	public delegate void DefaultDelegate ();
	public DefaultDelegate OnColorChange;
	public void AssignColor(byte _colorIndex){
		AssignedColorIndex = _colorIndex;
		ColoringTool.AssignColorIndex (ColorChannel, AssignedColorIndex);
		myImage.color = ColorManager.GetColor(AssignedColorIndex);
		MyToggle.isOn = false;

		if (OnColorChange != null)
			OnColorChange ();
	}
}
