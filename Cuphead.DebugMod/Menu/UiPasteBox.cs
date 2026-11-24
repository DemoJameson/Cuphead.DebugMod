using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BepInEx.CupheadDebugMod.Menu;

/// <summary>
/// Multiline paste box for the platform rows. One box is active at a time; keys are
/// polled from Input (Input.inputString) like UiTextEdit. Only digits and newlines are
/// accepted - every other character (typed or pasted via Ctrl+V) is dropped, so the
/// buffer can never hold invalid characters. Clicking elsewhere or pressing Escape
/// commits; the buffer persists per box key across menu rebuilds.
/// </summary>
internal static class UiPasteBox {
    private const int MaxCharacters = 4000;

    private static readonly Dictionary<string, string> buffers = new Dictionary<string, string>(StringComparer.Ordinal);

    private static string activeKey;
    private static string editBuffer;
    private static Text activeText;
    private static Image activeBackground;
    private static Image activeFrame;
    private static RectTransform activeContent;
    private static ScrollRect activeScroll;
    private static int autoScrolledLength = -1;
    private static float blinkTimer;
    private static bool caretOn;

    public static bool IsEditing {
        get { return activeKey != null; }
    }

    public static string Buffer(string key) {
        string value;
        return buffers.TryGetValue(key, out value) ? value : "";
    }

    /// <summary>The visible text: the buffer with a caret while this box is being edited.</summary>
    public static string DisplayText(string key) {
        string buffer = Buffer(key);
        bool editing = activeKey == key;
        if (buffer.Length == 0) {
            return editing ? "|" : "click, then type or Ctrl+V";
        }
        return editing ? buffer + (caretOn ? "|" : "") : buffer;
    }

    /// <summary>Rebinds the rebuilt card's visuals (a menu refresh destroys them); keeps
    /// the per-key buffer.</summary>
    public static void Register(string key, RectTransform content, ScrollRect scroll) {
        if (activeKey == key) {
            activeContent = content;
            activeScroll = scroll;
        }
    }

    /// <summary>Sizes the buffer content to its text (at least the viewport height) so the
    /// placeholder and buffer are visible and the box can scroll.</summary>
    public static void ResizeBuffer(string key, RectTransform content, float viewportHeight) {
        Text text = content.GetComponent<Text>();
        if (text == null) {
            return;
        }
        Vector2 size = content.sizeDelta;
        size.y = Mathf.Max(viewportHeight, text.preferredHeight);
        content.sizeDelta = size;
        if (activeKey == key && activeScroll != null) {
            activeScroll.verticalNormalizedPosition = IsEditing ? 0f : 1f;
        }
    }

    public static void Begin(string key, Text text, Image background, Image frame, ScrollRect scroll) {
        if (activeKey == key) {
            return;
        }
        if (activeKey != null) {
            EndEdit();
        }
        activeKey = key;
        editBuffer = Buffer(key);
        activeText = text;
        activeBackground = background;
        activeFrame = frame;
        activeContent = scroll != null ? scroll.content : null;
        activeScroll = scroll;
        autoScrolledLength = -1;
        blinkTimer = 0f;
        caretOn = true;
        if (activeBackground != null) {
            activeBackground.color = new Color(0.34f, 0.30f, 0.16f);
        }
        if (activeFrame != null) {
            activeFrame.color = MenuTheme.Accent;
        }
        RefreshText();
    }

    public static void Cancel() {
        if (activeKey == null) {
            return;
        }
        EndEdit();
    }

    public static void Tick() {
        if (activeKey == null) {
            return;
        }

        if (activeText == null || activeContent == null) {
            activeKey = null;
            return;
        }

        string typed = Input.inputString;
        for (int i = 0; i < typed.Length; i++) {
            char c = typed[i];
            if (c == '\b') {
                if (editBuffer.Length > 0) {
                    editBuffer = editBuffer.Substring(0, editBuffer.Length - 1);
                }
            } else if (c == '\n' || c == '\r') {
                AppendLine();
            } else if (char.IsDigit(c)) {
                Append(c.ToString());
            }
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V)) {
            Append(GUIUtility.systemCopyBuffer);
        }

        // the wheel is handled here directly: this menu's event setup does not deliver
        // scroll events to the ScrollRect reliably
        if (Input.mouseScrollDelta.y != 0f && activeScroll != null && ContainsScreen(Input.mousePosition)) {
            activeScroll.verticalNormalizedPosition = Mathf.Clamp01(
                activeScroll.verticalNormalizedPosition + Input.mouseScrollDelta.y * 0.12f);
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            EndEdit();
            return;
        }

        if (Input.GetMouseButtonDown(0) && !ContainsScreen(Input.mousePosition)) {
            EndEdit();
            return;
        }

        blinkTimer += Time.unscaledDeltaTime;
        if (blinkTimer >= 0.5f) {
            blinkTimer = 0f;
            caretOn = !caretOn;
        }
        RefreshText();
    }

    private static void Append(string text) {
        if (text == null) {
            return;
        }
        // validation: digits survive, everything else (spaces, letters, punctuation,
        // CR/LF variants) becomes a row break or is dropped
        for (int i = 0; i < text.Length; i++) {
            char c = text[i];
            if (editBuffer.Length >= MaxCharacters) {
                break;
            }
            if (char.IsDigit(c)) {
                editBuffer += c;
            } else if (c == '\n' || c == '\r') {
                AppendLine();
            }
        }
    }

    private static void AppendLine() {
        if (editBuffer.Length > 0 && editBuffer[editBuffer.Length - 1] != '\n' && editBuffer.Length < MaxCharacters) {
            editBuffer += "\n";
        }
    }

    private static void EndEdit() {
        if (activeKey != null) {
            buffers[activeKey] = editBuffer;
        }
        activeKey = null;
        if (activeBackground != null) {
            activeBackground.color = MenuTheme.ButtonBg;
        }
        if (activeFrame != null) {
            activeFrame.color = MenuTheme.Outline;
        }
        RefreshText();
        activeText = null;
        activeBackground = null;
        activeFrame = null;
        activeContent = null;
        activeScroll = null;
    }

    private static void RefreshText() {
        if (activeText == null || activeKey == null || activeContent == null) {
            return;
        }
        string buffer = CurrentBuffer();
        activeText.text = buffer.Length == 0
            ? "|"
            : buffer + (caretOn ? "|" : "");
        // keep the scrollable content as tall as the text needs, and follow the caret
        // only when the buffer actually changed (the wheel must stay free otherwise)
        float minHeight = activeScroll != null && activeScroll.viewport != null
            ? activeScroll.viewport.rect.height
            : 0f;
        Vector2 size = activeContent.sizeDelta;
        size.y = Mathf.Max(minHeight, activeText.preferredHeight);
        activeContent.sizeDelta = size;
        if (activeScroll != null && editBuffer.Length != autoScrolledLength) {
            activeScroll.verticalNormalizedPosition = 0f;
            autoScrolledLength = editBuffer.Length;
        }
    }
    private static string CurrentBuffer() {
        return editBuffer ?? "";
    }

    private static bool ContainsScreen(Vector2 screenPoint) {
        if (activeScroll == null || activeScroll.gameObject == null) {
            return false;
        }
        RectTransform boxRect = activeScroll.GetComponent<RectTransform>();
        if (boxRect == null) {
            return false;
        }
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(boxRect, screenPoint, null, out local);
        Rect rect = boxRect.rect;
        return rect.Contains(local);
    }
}
