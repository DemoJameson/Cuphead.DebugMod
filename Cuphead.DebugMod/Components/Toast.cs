using System.Collections;
using UnityEngine;

namespace BepInEx.CupheadDebugMod.Components;

public class Toast : PluginComponent {
    private static Toast instance;
    private string text = "";

    private static GUIStyle textStyle = new() {
        alignment = TextAnchor.LowerLeft,
        fontSize = 20,
        normal = {
            textColor = Color.white,
        },
        padding = new RectOffset(25, 0, 0, 25),
    };

    public static void Show(string text) {
        instance.StopAllCoroutines();
        instance.text = text;
        instance.StartCoroutine(ClearToast());
    }

    private static IEnumerator ClearToast() {
        yield return new WaitForSeconds(1f);
        instance.text = "";
    }

    private void Awake() {
        if (!Equals(instance, this)) {
            instance = this;
        }
    }

    private void OnGUI() {
        if (text.Length > 0) {
            DrawTextWithOutline(new Rect(0, Screen.height - 100, 400, 100), text, textStyle, Color.black, Color.white, 0);
        }
    }

    // http://answers.unity.com/answers/1386982/view.html
    private static void DrawTextWithOutline(Rect centerRect, string text, GUIStyle style, Color borderColor, Color innerColor, int borderWidth) {
        // assign the border color
        style.normal.textColor = borderColor;

        // draw an outline color copy to the left and up from original
        Rect modRect = centerRect;
        modRect.x -= borderWidth;
        modRect.y -= borderWidth;
        GUI.Label(modRect, text, style);


        // stamp copies from the top left corner to the top right corner
        while (modRect.x <= centerRect.x + borderWidth) {
            modRect.x++;
            GUI.Label(modRect, text, style);
        }

        // stamp copies from the top right corner to the bottom right corner
        while (modRect.y <= centerRect.y + borderWidth) {
            modRect.y++;
            GUI.Label(modRect, text, style);
        }

        // stamp copies from the bottom right corner to the bottom left corner
        while (modRect.x >= centerRect.x - borderWidth) {
            modRect.x--;
            GUI.Label(modRect, text, style);
        }

        // stamp copies from the bottom left corner to the top left corner
        while (modRect.y >= centerRect.y - borderWidth) {
            modRect.y--;
            GUI.Label(modRect, text, style);
        }

        // draw the inner color version in the center
        style.normal.textColor = innerColor;
        GUI.Label(centerRect, text, style);
    }
}