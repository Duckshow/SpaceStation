using UnityEngine;
public static class Extensions {
    public static string Color(this string s, Color c) {
        Color32 c32 = c;
        string cHex = "#" + c32.r.ToString("X2") + c32.g.ToString("X2") + c32.b.ToString("X2") + c32.a.ToString("X2");
        return "<color=" + cHex + ">" + s + "</color>";
    }
}
