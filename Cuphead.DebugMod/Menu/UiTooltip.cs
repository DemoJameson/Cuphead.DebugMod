using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BepInEx.CupheadDebugMod.Menu;

/// <summary>
/// Hover tooltips for small controls. Attach to any GameObject with a raycasting Graphic
/// (e.g. the spot type badges) and the shared tooltip panel follows the cursor while it
/// hovers. The panel is created once under the menu's canvas and never blocks the pointer
/// itself, so it can sit wherever the cursor is.
/// </summary>
internal class UiTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    private const float PanelWidth = 380f;
    private const float FollowOffsetX = 16f;
    private const float FollowOffsetY = 12f;

    private static GameObject panel;
    private static Text label;
    private static RectTransform panelRect;

    /// <summary>What the tooltip says while this control is hovered.</summary>
    public string HintText;

    public void OnPointerEnter(PointerEventData eventData) {
        Show(HintText, transform.root.gameObject);
    }

    public void OnPointerExit(PointerEventData eventData) {
        Hide();
    }

    private static void Show(string text, GameObject canvasRoot) {
        Ensure(canvasRoot);
        label.text = text;
        panelRect.sizeDelta = new Vector2(PanelWidth, label.preferredHeight + 12f);
        panel.transform.SetAsLastSibling();
        panel.SetActive(true);
    }

    public static void Hide() {
        if (panel != null) {
            panel.SetActive(false);
        }
    }

    private static void Ensure(GameObject canvasRoot) {
        if (panel != null) {
            return;
        }

        panel = Ui.Create("Tooltip", canvasRoot.transform);
        panelRect = Ui.Rect(panel);
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0f, 0f);
        Ui.Panel(panel, MenuTheme.HeaderBg);

        GameObject textGo = Ui.Create("Text", panel.transform);
        RectTransform textRect = Ui.Rect(textGo);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(6f, 4f);
        textRect.offsetMax = new Vector2(-6f, -4f);
        label = Ui.Label(textGo, "", MenuTheme.FontSizeSmall, MenuTheme.Text, TextAnchor.UpperLeft);

        panel.SetActive(false);
    }

    private void Update() {
        if (panel == null || !panel.activeSelf) {
            return;
        }
        Follow();
    }

    private static void Follow() {
        RectTransform rootRect = panel.transform.parent as RectTransform;
        if (rootRect == null) {
            return;
        }
        Canvas canvas = panel.GetComponentInParent<Canvas>();
        float scale = canvas != null ? canvas.scaleFactor : 1f;
        Vector2 half = rootRect.rect.size * 0.5f;
        Vector2 position = new Vector2(
            (Input.mousePosition.x - Screen.width * 0.5f) / scale,
            (Input.mousePosition.y - Screen.height * 0.5f) / scale);
        position += new Vector2(FollowOffsetX, FollowOffsetY);
        position.x = Mathf.Clamp(position.x, -half.x + 8f, half.x - panelRect.rect.width - 8f);
        position.y = Mathf.Clamp(position.y, -half.y + 8f, half.y - panelRect.rect.height - 8f);
        panelRect.anchoredPosition = position;
    }
}
