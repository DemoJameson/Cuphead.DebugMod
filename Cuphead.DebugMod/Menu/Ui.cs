using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace BepInEx.CupheadDebugMod.Menu;

/// <summary>
/// Small procedural uGUI helpers. Every control is plain RectTransform + Image + Text so it
/// works identically on Unity 5.6 and 2017.4 and can be restyled from MenuTheme alone.
/// </summary>
internal static class Ui {
    public static GameObject Create(string name, Transform parent) {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    public static RectTransform Rect(GameObject go) {
        return go.GetComponent<RectTransform>();
    }

    public static void Stretch(GameObject go) {
        RectTransform rt = Rect(go);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    /// <summary>Full-width band anchored to the top of the parent, `y` pixels down.</summary>
    public static void Band(GameObject go, float y, float height) {
        RectTransform rt = Rect(go);
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -y);
        rt.sizeDelta = new Vector2(0f, height);
    }

    /// <summary>Centered box of the given size.</summary>
    public static void CenterBox(GameObject go, Vector2 size) {
        RectTransform rt = Rect(go);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;
    }

    public static Image Panel(GameObject go, Color color) {
        Image image = go.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    public static Text Label(GameObject go, string text, int size, Color color, TextAnchor align) {
        Text label = go.AddComponent<Text>();
        label.font = MenuTheme.ResolveFont();
        label.fontSize = size;
        label.color = color;
        label.alignment = align;
        label.supportRichText = false;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.raycastTarget = false;
        label.text = text;
        return label;
    }

    /// <summary>Turns the object into a clickable button (Image + Button + hover tint).</summary>
    public static Button MakeButton(GameObject go, Action onClick, Color background) {
        Image image = go.GetComponent<Image>();
        if (image == null) {
            image = go.AddComponent<Image>();
        }
        image.color = background;
        image.raycastTarget = true;

        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f);
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.6f);
        colors.colorMultiplier = 1f;
        button.colors = colors;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        if (onClick != null) {
            button.onClick.AddListener(new UnityEngine.Events.UnityAction(onClick));
        }
        return button;
    }

    public static GameObject TextButton(Transform parent, string name, string label, float width, float height, Action onClick, int fontSize) {
        GameObject go = Create(name, parent);
        Rect(go).sizeDelta = new Vector2(width, height);
        MakeButton(go, onClick, MenuTheme.ButtonBg);

        GameObject textGo = Create("Label", go.transform);
        Stretch(textGo);
        Label(textGo, label, fontSize, MenuTheme.Text, TextAnchor.MiddleCenter);
        return go;
    }

    public static GameObject Row(Transform parent, string name, Color background, float y, float height) {
        GameObject go = Create(name, parent);
        Band(go, y, height);
        Panel(go, background);
        return go;
    }

    /// <summary>Flexible-width label on the left side of a row, stopping `rightReserve` before the right edge.</summary>
    public static Text RowLabel(GameObject row, string text, int size, Color color, float leftPad, float rightReserve) {
        GameObject go = Create("Label", row.transform);
        RectTransform rt = Rect(go);
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.offsetMin = new Vector2(leftPad, 0f);
        rt.offsetMax = new Vector2(-rightReserve, 0f);
        return Label(go, text, size, color, TextAnchor.MiddleLeft);
    }

    /// <summary>A square slot anchored to the row's right edge, `rightOffset` pixels in from it.</summary>
    public static GameObject RightSlot(GameObject row, string name, float rightOffset, float width, float height) {
        GameObject go = Create(name, row.transform);
        RectTransform rt = Rect(go);
        rt.anchorMin = new Vector2(1f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.anchoredPosition = new Vector2(-rightOffset, 0f);
        rt.sizeDelta = new Vector2(width, height);
        return go;
    }

    public static Image Dot(GameObject row, float rightOffset, float size, Color color) {
        GameObject go = RightSlot(row, "ModifiedDot", rightOffset, size, size);
        Image image = go.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }
}

/// <summary>A simple checkbox (accent-filled square when on).</summary>
internal sealed class UiCheck {
    private GameObject root;
    private Image fill;
    private Button button;
    private bool value;
    private Action<bool> onChanged;

    public GameObject Root {
        get { return root; }
    }

    public bool Value {
        get { return value; }
    }

    public static UiCheck Create(Transform parent, string name, float size, bool value, Action<bool> onChanged) {
        UiCheck check = new UiCheck();
        check.value = value;
        check.onChanged = onChanged;

        check.root = Ui.Create(name, parent);
        Ui.Rect(check.root).sizeDelta = new Vector2(size, size);
        Ui.Panel(check.root, MenuTheme.ButtonBg);

        GameObject fillGo = Ui.Create("Fill", check.root.transform);
        Ui.Stretch(fillGo);
        check.fill = Ui.Panel(fillGo, MenuTheme.Accent);
        RectTransform fillRect = Ui.Rect(fillGo);
        fillRect.offsetMin = new Vector2(4f, 4f);
        fillRect.offsetMax = new Vector2(-4f, -4f);

        check.button = Ui.MakeButton(check.root, check.Toggle, MenuTheme.ButtonBg);
        check.Refresh();
        return check;
    }

    public void Set(bool newValue) {
        if (value == newValue) {
            return;
        }
        value = newValue;
        Refresh();
        if (onChanged != null) {
            onChanged(value);
        }
    }

    /// <summary>Updates the visual state without firing the change callback.</summary>
    public void SetVisual(bool newValue) {
        value = newValue;
        Refresh();
    }

    private void Toggle() {
        Set(!value);
    }

    private void Refresh() {
        if (fill != null) {
            fill.enabled = value;
        }
    }
}

/// <summary>A [prev value next] cycler for enum-style options.</summary>
internal sealed class UiCycle {
    private Text valueLabel;
    private Button prevButton;
    private Button nextButton;

    public GameObject Root { get; private set; }

    public static UiCycle Create(Transform parent, string name, float width, float height, string initialText, Action onPrev, Action onNext) {
        UiCycle cycle = new UiCycle();
        GameObject root = Ui.Create(name, parent);
        Ui.Rect(root).sizeDelta = new Vector2(width, height);
        cycle.Root = root;

        GameObject prev = Ui.Create("Prev", root.transform);
        RectTransform prevRect = Ui.Rect(prev);
        prevRect.anchorMin = new Vector2(0f, 0.5f);
        prevRect.anchorMax = new Vector2(0f, 0.5f);
        prevRect.pivot = new Vector2(0f, 0.5f);
        prevRect.anchoredPosition = Vector2.zero;
        prevRect.sizeDelta = new Vector2(height, height);
        Ui.MakeButton(prev, onPrev, MenuTheme.ButtonBg);
        cycle.prevButton = prev.GetComponent<Button>();
        cycle.BuildArrowIcon(prev.transform, UiIcons.TriangleLeft);

        GameObject valueGo = Ui.Create("Value", root.transform);
        RectTransform valueRect = Ui.Rect(valueGo);
        valueRect.anchorMin = new Vector2(0f, 0.5f);
        valueRect.anchorMax = new Vector2(1f, 0.5f);
        valueRect.pivot = new Vector2(0.5f, 0.5f);
        valueRect.offsetMin = new Vector2(height + 2f, 0f);
        valueRect.offsetMax = new Vector2(-(height + 2f), 0f);
        cycle.valueLabel = Ui.Label(valueGo, initialText, MenuTheme.FontSizeLabel, MenuTheme.Text, TextAnchor.MiddleCenter);

        GameObject next = Ui.Create("Next", root.transform);
        RectTransform nextRect = Ui.Rect(next);
        nextRect.anchorMin = new Vector2(1f, 0.5f);
        nextRect.anchorMax = new Vector2(1f, 0.5f);
        nextRect.pivot = new Vector2(1f, 0.5f);
        nextRect.anchoredPosition = Vector2.zero;
        nextRect.sizeDelta = new Vector2(height, height);
        Ui.MakeButton(next, onNext, MenuTheme.ButtonBg);
        cycle.nextButton = next.GetComponent<Button>();
        cycle.BuildArrowIcon(next.transform, UiIcons.TriangleRight);

        return cycle;
    }

    private void BuildArrowIcon(Transform buttonTransform, Sprite sprite) {
        GameObject iconGo = Ui.Create("Icon", buttonTransform);
        RectTransform iconRect = Ui.Rect(iconGo);
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = new Vector2(10f, 10f);
        Image icon = iconGo.AddComponent<Image>();
        icon.sprite = sprite;
        icon.color = MenuTheme.Text;
        icon.raycastTarget = false;
        Button button = buttonTransform.GetComponent<Button>();
        if (button != null) {
            button.targetGraphic = icon;
        }
    }

    public void SetText(string text) {
        if (valueLabel != null) {
            valueLabel.text = text;
        }
    }
}

/// <summary>Attaches a simple click callback to any GameObject (used for click-to-select labels).</summary>
internal static class UiClick {
    public static void Add(GameObject go, Action onClick) {
        Forwarder forwarder = go.AddComponent<Forwarder>();
        forwarder.OnClick = onClick;
    }

    private sealed class Forwarder : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler {
        public Action OnClick;

        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData) {
            if (OnClick != null) {
                OnClick();
            }
        }
    }
}
