using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Mouse))]
public class MouseEditor : Editor {

    private Mouse mouse;
    private bool hasSetupColors = false;
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        mouse = target as Mouse;
        if (mouse.Coloring.Palette == null)
            hasSetupColors = false;
        else if (!hasSetupColors)
            SetColors();
    }

    private Color[] pixels;
    void SetColors() {
        hasSetupColors = true;

        pixels = mouse.Coloring.Palette.GetPixels();
        if (pixels.Length != mouse.Coloring.AllColors.Length)
            throw new System.Exception("The color palette and the AllColors-array are of different sizes! The array-size can't be changed without restarting the editor!");

        int _height = mouse.Coloring.Palette.height;
        int _width = mouse.Coloring.Palette.width;
        int _index = 0;
        for (int y = (_height - 1); y >= 0; y--) {
            for (int x = 0; x < _width; x++) {
                mouse.Coloring.AllColors[_index] = pixels[(_width * y) + x];
                _index++;
            }
        }

        Debug.Log("ColoringTool's AllColors has been updated!");
    }
}
