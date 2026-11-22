using UnityEngine;

namespace BepInEx.CupheadDebugMod.Menu;

/// <summary>
/// Keyboard editing for the value boxes. A single box is active at a time; keys are polled
/// directly from Input (Input.inputString) so editing works regardless of how the game's
/// event system is configured. Clicking elsewhere or pressing Enter commits, Escape cancels.
/// </summary>
internal static class UiTextEdit {
    private static UiSliderField owner;
    private static string buffer = "";
    private static float blinkTimer;
    private static bool caretOn;

    public static bool IsEditing {
        get { return owner != null; }
    }

    public static UiSliderField Owner {
        get { return owner; }
    }

    public static void Begin(UiSliderField field, string initial) {
        if (owner == field) {
            return;
        }
        if (owner != null) {
            Commit();
        }
        owner = field;
        buffer = initial;
        blinkTimer = 0f;
        caretOn = true;
        owner.BeginBoxEdit();
    }

    public static void Cancel() {
        if (owner == null) {
            return;
        }
        UiSliderField field = owner;
        owner = null;
        field.EndBoxEdit(false);
    }

    public static void Tick() {
        if (owner == null) {
            return;
        }

        if (owner.Root == null) {
            owner = null;
            return;
        }

        string typed = Input.inputString;
        for (int i = 0; i < typed.Length; i++) {
            char c = typed[i];
            if (c == '\b') {
                if (buffer.Length > 0) {
                    buffer = buffer.Substring(0, buffer.Length - 1);
                }
            } else if (c == '\n' || c == '\r') {
                Commit();
                return;
            } else if (char.IsDigit(c) || c == '.') {
                if (buffer.Length < 8 && !(c == '.' && buffer.IndexOf('.') >= 0)) {
                    buffer += c;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            Cancel();
            return;
        }

        if (Input.GetMouseButtonDown(0) && !owner.BoxContainsScreen(Input.mousePosition)) {
            Commit();
            return;
        }

        blinkTimer += Time.unscaledDeltaTime;
        if (blinkTimer >= 0.5f) {
            blinkTimer = 0f;
            caretOn = !caretOn;
        }
        owner.SetBoxText(buffer + (caretOn ? "|" : ""));
    }

    private static void Commit() {
        UiSliderField field = owner;
        owner = null;
        field.EndBoxEdit(true, buffer);
    }
}
