using UnityEngine;
using UnityEngine.UI;

public class CopyAssignedColorToNeighbour : MonoBehaviour {

	[SerializeField] private ColorAssignButton ColorButtonTo;
	[SerializeField] private ColorAssignButton ColorButtonFrom;
	private Button myButton;

	void Awake(){
		myButton = GetComponent<Button> ();
	}
	void OnEnable() {
		ColorButtonTo.OnColorChange += OnColorChanged;
		ColorButtonFrom.OnColorChange += OnColorChanged;
		myButton.onClick.AddListener(OnButtonClick);
	}
	void OnDisable() {
		ColorButtonTo.OnColorChange -= OnColorChanged;
		ColorButtonFrom.OnColorChange -= OnColorChanged;
		myButton.onClick.RemoveListener(OnButtonClick);
	}
//	void Start(){
//		myButton.interactable = ColorButtonTo.AssignedColorIndex != ColorButtonFrom.AssignedColorIndex;
//		myButton.image.gameObject.SetActive (myButton.interactable);
//	}

	public void OnColorChanged(){
		myButton.interactable = ColorButtonTo.AssignedColorIndex != ColorButtonFrom.AssignedColorIndex;
		myButton.image.gameObject.SetActive (myButton.interactable);
	}

	public void OnButtonClick() {
		ColorButtonTo.AssignColor (ColorButtonFrom.AssignedColorIndex);
	}
}