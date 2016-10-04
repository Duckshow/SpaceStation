using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GUIManager : MonoBehaviour {

    public static GUIManager Instance;
    public enum WindowType { Component, Component_SubWindow, ComponentHolder_x6, ComponentHolder_x9, ComponentHolder_15, ComponentHolder_x24 }

    [SerializeField]
    private UIInfoWindow ComponentWindow_Main;
    [SerializeField]
    private UIInfoWindow ComponentWindow_Sub;
    [SerializeField]
    private UIInfoWindow ComponentHolderWindow_x6;
    [SerializeField]
    private UIInfoWindow ComponentHolderWindow_x9;
    [SerializeField]
    private UIInfoWindow ComponentHolderWindow_x15;
    [SerializeField]
    private UIInfoWindow ComponentHolderWindow_x24;

    //private WindowType currentWindowType;
    //private UIInfoWindow currentInfoWindow;
    private ComponentHolder currentComponentHolder;
    //private ComponentObject trackedComponent;

    private class InfoWindowUser {
        public UIInfoWindow Window;
        public InteractiveObject UserIO;

        public InfoWindowUser(UIInfoWindow _window, InteractiveObject _io) {
            Window = _window;
            UserIO = _io;
        }
    }
    private List<InfoWindowUser> currentInfoWindows = new List<InfoWindowUser>();

    void Awake() {
        if (Instance)
            Destroy(this);
        Instance = this;
    }

    void Start(){
        UIInfoWindow[] _windows = FindObjectsOfType<UIInfoWindow>();
        for (int i = 0; i < _windows.Length; i++) {
            _windows[i].HideWindow();
        }
    }

    //public void ToggleWindow(InteractiveObject _io, InteractiveObject.State _state, WindowType _type) { // currently only support for 1 window, but maybe more later?
    //    if (currentInfoWindow != null && currentInfoWindow.OwnerIO == _io)
    //        CloseCurrentInfoWindow();
    //    else
    //        OpenNewWindow(_io, _state, _type);
    //}

    public void OpenNewWindow(InteractiveObject _io, InteractiveObject.State _state, WindowType _type) {
        int _index = currentInfoWindows.FindIndex(x => x.UserIO == _io);
        UIInfoWindow _currentWindow = null;

        if (_index > -1) {
            _currentWindow = currentInfoWindows[_index].Window;

            if (_currentWindow.OwnerIO == _io) {
                if (_state != _currentWindow.State)
                    _currentWindow.ChangeState(_state);

                return;
            }

            CloseInfoWindow(_io, _index);
        }

        //currentWindowType = _type;
        switch (_type) {
            case WindowType.Component:
                _currentWindow = ComponentWindow_Main;
                break;
            case WindowType.Component_SubWindow:
                _currentWindow = ComponentWindow_Sub;
                break;
            case WindowType.ComponentHolder_x6:
                _currentWindow = ComponentHolderWindow_x6;
                break;
            case WindowType.ComponentHolder_x9:
                _currentWindow = ComponentHolderWindow_x9;
                break;
            case WindowType.ComponentHolder_15:
                _currentWindow = ComponentHolderWindow_x15;
                break;
            case WindowType.ComponentHolder_x24:
                _currentWindow = ComponentHolderWindow_x24;
                break;
        }

        ComponentObject _trackedComponent = null;
        switch (_type) {
            case WindowType.Component:
            case WindowType.Component_SubWindow:
                _trackedComponent = _io.GetComponent<ComponentObject>();
                break;
            case WindowType.ComponentHolder_x6:
            case WindowType.ComponentHolder_x9:
            case WindowType.ComponentHolder_15:
            case WindowType.ComponentHolder_x24:
                currentComponentHolder = _io.GetComponent<ComponentHolder>();

                // set button-names to be the same as in the component-slots
                ComponentObject.ComponentType _compType;
                int _compTypeID;
                for (int i = 0; i < _currentWindow.Buttons.Length; i++) {
                    _compType = currentComponentHolder.ComponentSlots[i].SlotType;
                    _compTypeID = currentComponentHolder.ComponentSlots[i].SlotTypeID;
                    _currentWindow.Buttons[i].GetComponentInChildren<Text>().text = ComponentInfoManager.Instance.AllComponentsInfo[_compType][_compTypeID].Name;
                }
                break;
        }

        _currentWindow.UI_Image.sprite = _io.Selected_Sprite;
        _currentWindow.UI_Name.text = _io.Selected_Name;
        _currentWindow.UI_Desc.text = _io.Selected_Desc;

        UITrackObject _tracker = _currentWindow.GetComponent<UITrackObject>();
        if (_tracker != null)
            _tracker.trackTransform = _io.transform;

        _currentWindow.ChangeState(_state);
        _currentWindow.ShowWindow(_io, _trackedComponent);
        currentInfoWindows.Add(new InfoWindowUser(_currentWindow, _io));

        UpdateButtonGraphics(_io);
    }

    public void CloseInfoWindow(InteractiveObject _io, int _index = -1) {
        if (_index == -1) {
            _index = currentInfoWindows.FindIndex(x => x.UserIO == _io);
            if (_index == -1)
                return;
        }

        currentInfoWindows[_index].Window.HideWindow();
        UITrackObject _tracker = currentInfoWindows[_index].Window.GetComponent<UITrackObject>();
        if (_tracker != null)
            _tracker.trackTransform = null;

        //trackedComponent = null;
        currentInfoWindows.RemoveAt(_index);
    }

    // probably works just fine, just not used right now
    //public void CloseAllWindows() {
    //    for (int i = currentInfoWindows.Count - 1; i <= 0; i--) {
    //        currentInfoWindows[i].Window.HideWindow();
    //        currentInfoWindows.RemoveAt(i);
    //    }
    //}

    public void OnComponentSlotClicked(int _slotIndex) {
        currentComponentHolder.OnComponentButtonClicked(_slotIndex);
    }

    public void UpdateButtonGraphics(InteractiveObject _io) {
        int _index = currentInfoWindows.FindIndex(x => x.UserIO == _io);
        if (_index == -1 || currentInfoWindows[_index].Window.UI_ComponentSlots.Length == 0)
            return;

        UIInfoWindow _window = currentInfoWindows[_index].Window;
        Button _button = null;
        ComponentHolder.ComponentSlot _slot = null;
        for (int i = 0; i < _window.UI_ComponentSlots.Length; i++) {
            _button = _window.UI_ComponentSlots[i];
            _slot = currentComponentHolder.ComponentSlots[i];

            if (_slot.HeldComponent == null) {
                _button.GetComponent<Image>().color = Color.black;
                continue;
            }
            if (_slot.HeldComponent._Current_ == 0) {
                _button.GetComponent<Image>().color = Color.red;
                continue;
            }
            if (_slot.HeldComponent.TypeID != _slot.SlotTypeID) {
                _button.GetComponent<Image>().color = Color.yellow;
                continue;
            }

            // everything's fine
            _button.GetComponent<Image>().color = Color.green;
        }
    }
}
