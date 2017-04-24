using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ColorPalletteButton : MonoBehaviour {

	//[SerializeField] 
	private byte ColorIndex;
	private Button myButton;
	private Image myImage;


	void Awake(){
		myButton = GetComponent<Button> ();
		myImage = GetComponent<Image> ();
		ColorIndex = (byte)transform.GetSiblingIndex ();
	}
	void OnEnable() {
		myButton.onClick.AddListener(OnToggleValueChanged);

		#if UNITY_EDITOR
		myImage.color = FindObjectOfType<Mouse>().Coloring.AllColors [ColorIndex];
		#else
		myImage.color = Mouse.Instance.Coloring.AllColors [ColorIndex];
		#endif
	}
	void OnDisable() {
		myButton.onClick.RemoveListener(OnToggleValueChanged);
	}

	public void OnToggleValueChanged() {
		ColorAssignButton.sActiveButton.AssignColor (ColorIndex);
	}
}
