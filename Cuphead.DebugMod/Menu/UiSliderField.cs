using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BepInEx.CupheadDebugMod.Menu;

/// <summary>
/// Slider + numeric box + up/down arrows control, mirroring the reference layout:
/// [==========o-------] [ 80 |^] [|v]
/// The track is draggable anywhere over its full height (snaps to `snap` increments while
/// dragging), the box accepts typed values (committed with Enter or a click elsewhere), and
/// the arrows step by `step`. Limits can be supplied dynamically — attempts beyond them are
/// clamped and shown as a ghosted track region so it is visible why the value did not move.
/// </summary>
internal sealed class UiSliderField {
    private readonly float min;
    private readonly float max;
    private readonly float step;
    private readonly float snap;
    private readonly int decimals;
    private readonly bool lockable;

    private GameObject root;
    private RectTransform hitRect;
    private Image fillImage;
    private Image handleImage;
    private Image ghostImage;
    private Image boxBg;
    private Text boxText;
    private Button lockButton;
    private Image lockIcon;

    private float value;
    private bool grayed;
    private bool locked;
    private bool dragging;

    private Func<float> limitMin;
    private Func<float> limitMax;
    private Action<float> onDrag;
    private Action onCommit;
    private Action<bool> onLockToggle;

    private float ghostFrom = float.NaN;
    private float ghostTo = float.NaN;
    private float ghostExpiry;

    private static readonly List<UiSliderField> Live = new List<UiSliderField>();

    public GameObject Root {
        get { return root; }
    }

    public float Value {
        get { return value; }
    }

    public bool Locked {
        get { return locked; }
    }

    private UiSliderField(float min, float max, float step, float snap, int decimals, bool lockable) {
        this.min = min;
        this.max = max;
        this.step = step;
        this.snap = snap;
        this.decimals = decimals;
        this.lockable = lockable;
    }

    public static UiSliderField Create(
        Transform parent,
        string name,
        float width,
        float height,
        float min,
        float max,
        float step,
        float snap,
        int decimals,
        float initial,
        bool lockable,
        Func<float> limitMin,
        Func<float> limitMax,
        Action<float> onDrag,
        Action onCommit,
        Action<bool> onLockToggle
    ) {
        UiSliderField slider = new UiSliderField(min, max, step, snap, decimals, lockable);
        slider.limitMin = limitMin;
        slider.limitMax = limitMax;
        slider.onDrag = onDrag;
        slider.onCommit = onCommit;
        slider.onLockToggle = onLockToggle;
        slider.value = Mathf.Clamp(initial, min, max);

        slider.root = Ui.Create(name, parent);
        RectTransform rootRect = Ui.Rect(slider.root);
        rootRect.sizeDelta = new Vector2(width, height);
        rootRect.pivot = new Vector2(1f, 0.5f);

        float rightReserved = lockable ? 110f : 88f;
        float trackWidth = width - rightReserved;

        // invisible hit area over the whole track column so dragging is not pixel-precise
        GameObject hit = Ui.Create("Hit", slider.root.transform);
        slider.hitRect = Ui.Rect(hit);
        slider.hitRect.anchorMin = new Vector2(0f, 0.5f);
        slider.hitRect.anchorMax = new Vector2(0f, 0.5f);
        slider.hitRect.pivot = new Vector2(0f, 0.5f);
        slider.hitRect.anchoredPosition = Vector2.zero;
        slider.hitRect.sizeDelta = new Vector2(trackWidth, height);
        Image hitImage = hit.AddComponent<Image>();
        hitImage.color = new Color(0f, 0f, 0f, 0f);
        hitImage.raycastTarget = true;
        TrackEvents events = hit.AddComponent<TrackEvents>();
        events.Owner = slider;

        // visible track: bg, ghost, fill, handle
        GameObject track = Ui.Create("Track", slider.root.transform);
        RectTransform trackRect = Ui.Rect(track);
        trackRect.anchorMin = new Vector2(0f, 0.5f);
        trackRect.anchorMax = new Vector2(0f, 0.5f);
        trackRect.pivot = new Vector2(0f, 0f);
        trackRect.anchoredPosition = new Vector2(0f, -5f);
        trackRect.sizeDelta = new Vector2(trackWidth, 10f);
        Image trackBg = track.AddComponent<Image>();
        trackBg.color = new Color(0.30f, 0.30f, 0.36f);
        trackBg.raycastTarget = false;

        GameObject ghost = Ui.Create("Ghost", track.transform);
        Ui.Stretch(ghost);
        slider.ghostImage = ghost.AddComponent<Image>();
        slider.ghostImage.color = new Color(1f, 1f, 1f, 0.16f);
        slider.ghostImage.raycastTarget = false;
        slider.ghostImage.enabled = false;

        GameObject fill = Ui.Create("Fill", track.transform);
        slider.fillImage = fill.AddComponent<Image>();
        slider.fillImage.color = MenuTheme.Accent;
        slider.fillImage.raycastTarget = false;
        RectTransform fillRect = slider.fillImage.rectTransform;
        fillRect.anchorMin = new Vector2(0f, 0.5f);
        fillRect.anchorMax = new Vector2(0f, 0.5f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = new Vector2(0f, 10f);

        GameObject handle = Ui.Create("Handle", track.transform);
        slider.handleImage = handle.AddComponent<Image>();
        slider.handleImage.color = new Color(0.95f, 0.95f, 0.97f);
        slider.handleImage.raycastTarget = false;
        RectTransform handleRect = slider.handleImage.rectTransform;
        handleRect.anchorMin = new Vector2(0f, 0.5f);
        handleRect.anchorMax = new Vector2(0f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(7f, 14f);

        // value box
        GameObject box = Ui.Create("Box", slider.root.transform);
        RectTransform boxRect = Ui.Rect(box);
        boxRect.anchorMin = new Vector2(1f, 0.5f);
        boxRect.anchorMax = new Vector2(1f, 0.5f);
        boxRect.pivot = new Vector2(1f, 0.5f);
        boxRect.anchoredPosition = new Vector2(-20f, 0f);
        boxRect.sizeDelta = new Vector2(64f, height - 4f);
        slider.boxBg = box.AddComponent<Image>();
        slider.boxBg.color = MenuTheme.ButtonBg;
        slider.boxBg.raycastTarget = true;
        BoxClick click = box.AddComponent<BoxClick>();
        click.Owner = slider;
        GameObject boxTextGo = Ui.Create("Text", box.transform);
        Ui.Stretch(boxTextGo);
        slider.boxText = Ui.Label(boxTextGo, slider.Format(slider.value), MenuTheme.FontSizeSmall, MenuTheme.Text, TextAnchor.MiddleRight);
        RectTransform boxTextRect = slider.boxText.rectTransform;
        boxTextRect.offsetMin = new Vector2(4f, 0f);
        boxTextRect.offsetMax = new Vector2(-6f, 0f);

        // up/down arrows
        slider.BuildArrow(false, height);
        slider.BuildArrow(true, height);

        if (lockable) {
            GameObject lockGo = Ui.Create("Lock", slider.root.transform);
            RectTransform lockRect = Ui.Rect(lockGo);
            lockRect.anchorMin = new Vector2(1f, 0.5f);
            lockRect.anchorMax = new Vector2(1f, 0.5f);
            lockRect.pivot = new Vector2(1f, 0.5f);
            lockRect.anchoredPosition = new Vector2(-88f, 0f);
            lockRect.sizeDelta = new Vector2(18f, 18f);
            slider.lockIcon = lockGo.AddComponent<Image>();
            slider.lockIcon.sprite = UiIcons.LockOpen;
            slider.lockIcon.color = MenuTheme.TextDim;
            slider.lockIcon.raycastTarget = true;
            slider.lockButton = Ui.MakeButton(lockGo, slider.ToggleLock, new Color(0f, 0f, 0f, 0f));
            slider.lockButton.targetGraphic = slider.lockIcon;
        }

        slider.RefreshVisual();
        slider.RefreshColors();
        Live.Add(slider);
        return slider;
    }

    private void BuildArrow(bool up, float height) {
        GameObject arrow = Ui.Create(up ? "ArrowUp" : "ArrowDown", root.transform);
        RectTransform rect = Ui.Rect(arrow);
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(1f, 0.5f);
        float half = (height - 4f) / 2f;
        rect.anchoredPosition = new Vector2(0f, up ? half / 2f : -half / 2f);
        rect.sizeDelta = new Vector2(16f, half);
        Button button = Ui.MakeButton(arrow, () => Step(up ? 1 : -1), MenuTheme.ButtonBg);
        GameObject iconGo = Ui.Create("Icon", arrow.transform);
        RectTransform iconRect = Ui.Rect(iconGo);
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = new Vector2(10f, 8f);
        Image icon = iconGo.AddComponent<Image>();
        icon.sprite = up ? UiIcons.TriangleUp : UiIcons.TriangleDown;
        icon.color = MenuTheme.Text;
        icon.raycastTarget = false;
        button.targetGraphic = icon;
    }

    public void SetValue(float newValue) {
        value = Mathf.Clamp(newValue, min, max);
        RefreshVisual();
    }

    public void SetLocked(bool isLocked) {
        locked = isLocked;
        if (lockIcon != null) {
            lockIcon.sprite = locked ? UiIcons.LockClosed : UiIcons.LockOpen;
            lockIcon.color = locked ? MenuTheme.Accent : MenuTheme.TextDim;
        }
    }

    public void SetGrayed(bool isGrayed) {
        grayed = isGrayed;
        RefreshColors();

        CanvasGroup group = root.GetComponent<CanvasGroup>();
        if (group == null) {
            group = root.AddComponent<CanvasGroup>();
        }
        group.alpha = isGrayed ? 0.5f : 1f;
        group.interactable = !isGrayed;
    }

    public void SetBoxText(string text) {
        if (boxText != null) {
            boxText.text = MenuTheme.MapGlyphs(text);
        }
    }

    public bool BoxContainsScreen(Vector2 screenPoint) {
        if (boxBg == null) {
            return false;
        }
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(boxBg.rectTransform, screenPoint, null, out local);
        Rect rect = boxBg.rectTransform.rect;
        return rect.Contains(local);
    }

    public void BeginBoxEdit() {
        boxBg.color = new Color(0.34f, 0.30f, 0.16f);
    }

    public void EndBoxEdit(bool commit, string buffer = null) {
        boxBg.color = MenuTheme.ButtonBg;
        if (commit && buffer != null) {
            float parsed;
            if (float.TryParse(buffer.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out parsed)) {
                ApplyAttempt(parsed, false);
            }
        }
        RefreshVisual();
        if (onCommit != null) {
            onCommit();
        }
    }

    private void ToggleLock() {
        if (grayed) {
            return;
        }
        locked = !locked;
        SetLocked(locked);
        if (onLockToggle != null) {
            onLockToggle(locked);
        }
    }

    public void Step(int direction) {
        if (grayed) {
            return;
        }
        ApplyAttempt(value + direction * step, false);
        if (onCommit != null) {
            onCommit();
        }
    }

    private void PointerDown(PointerEventData eventData) {
        if (grayed) {
            return;
        }
        dragging = true;
        DragTo(eventData);
    }

    private void Drag(PointerEventData eventData) {
        if (!dragging || grayed) {
            return;
        }
        DragTo(eventData);
    }

    private void PointerUp() {
        if (!dragging) {
            return;
        }
        dragging = false;
        ClearGhost();
        if (onCommit != null) {
            onCommit();
        }
    }

    private void DragTo(PointerEventData eventData) {
        Vector2 local;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(hitRect, eventData.position, eventData.pressEventCamera, out local)) {
            return;
        }
        float rawPct = Mathf.Clamp01(local.x / hitRect.rect.width) * (max - min) + min;
        float target = snap > 0f ? Mathf.Round((rawPct - min) / snap) * snap + min : rawPct;
        ApplyAttempt(target, true);
    }

    private void ApplyAttempt(float target, bool gesture) {
        float lo = limitMin != null ? Mathf.Max(limitMin(), min) : min;
        float hi = limitMax != null ? Mathf.Min(limitMax(), max) : max;

        bool overLimit = false;
        if (target > hi + 0.0001f) {
            SetGhost(hi, target);
            ghostExpiry = Time.unscaledTime + 0.8f;
            target = hi;
            overLimit = true;
        } else if (target < lo - 0.0001f) {
            SetGhost(target, lo);
            ghostExpiry = Time.unscaledTime + 0.8f;
            target = lo;
            overLimit = true;
        } else {
            ClearGhost();
        }

        if (Math.Abs(target - value) > 0.0001f) {
            value = target;
            RefreshVisual();
            if (onDrag != null) {
                onDrag(value);
            }
        } else if (overLimit && !gesture) {
            // keep the ghost visible briefly even when nothing moved
        }
    }

    private void SetGhost(float from, float to) {
        ghostFrom = from;
        ghostTo = to;
        UpdateGhostVisual();
    }

    private void ClearGhost() {
        ghostFrom = float.NaN;
        ghostTo = float.NaN;
        if (ghostImage != null) {
            ghostImage.enabled = false;
        }
    }

    private void UpdateGhostVisual() {
        if (ghostImage == null) {
            return;
        }
        if (float.IsNaN(ghostFrom) || float.IsNaN(ghostTo)) {
            ghostImage.enabled = false;
            return;
        }
        float trackWidth = hitRect.rect.width;
        float fromPct = (Mathf.Clamp(ghostFrom, min, max) - min) / (max - min);
        float toPct = (Mathf.Clamp(ghostTo, min, max) - min) / (max - min);
        float left = Mathf.Min(fromPct, toPct) * trackWidth;
        float width = Mathf.Max(Mathf.Abs(toPct - fromPct) * trackWidth, 2f);
        ghostImage.enabled = true;
        RectTransform rt = ghostImage.rectTransform;
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0f);
        rt.anchoredPosition = new Vector2(left, 0f);
        rt.sizeDelta = new Vector2(width, 10f);
    }

    private void RefreshVisual() {
        if (root == null) {
            return;
        }
        float trackWidth = hitRect.rect.width;
        float pct = (value - min) / (max - min);
        fillImage.rectTransform.sizeDelta = new Vector2(pct * trackWidth, 10f);
        handleImage.rectTransform.anchoredPosition = new Vector2(pct * trackWidth, 0f);
        if (!UiTextEdit.IsEditing || UiTextEdit.Owner != this) {
            boxText.text = MenuTheme.MapGlyphs(Format(value));
        }
        UpdateGhostVisual();
    }

    private void RefreshColors() {
        if (root == null) {
            return;
        }
        Color dim = new Color(0.42f, 0.42f, 0.48f);
        fillImage.color = grayed ? dim : MenuTheme.Accent;
        handleImage.color = grayed ? new Color(0.55f, 0.55f, 0.60f) : new Color(0.95f, 0.95f, 0.97f);
        boxText.color = grayed ? MenuTheme.TextDim : MenuTheme.Text;
        if (lockButton != null) {
            lockButton.interactable = !grayed;
            lockIcon.color = grayed ? new Color(0.45f, 0.45f, 0.50f) : (locked ? MenuTheme.Accent : MenuTheme.TextDim);
        }
    }

    private string Format(float v) {
        return v.ToString("0." + new string('0', decimals), CultureInfo.InvariantCulture);
    }

    /// <summary>Per-frame upkeep (called while the menu is open): expires transient ghosts.</summary>
    public static void Tick() {
        for (int i = Live.Count - 1; i >= 0; i--) {
            UiSliderField slider = Live[i];
            if (slider.root == null) {
                Live.RemoveAt(i);
                continue;
            }
            if (!slider.dragging && !float.IsNaN(slider.ghostFrom) && Time.unscaledTime > slider.ghostExpiry) {
                slider.ClearGhost();
            }
        }
    }

    private sealed class TrackEvents : MonoBehaviour, UnityEngine.EventSystems.IPointerDownHandler, UnityEngine.EventSystems.IDragHandler, UnityEngine.EventSystems.IPointerUpHandler {
        public UiSliderField Owner;

        public void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData) {
            Owner.PointerDown(eventData);
        }

        public void OnDrag(UnityEngine.EventSystems.PointerEventData eventData) {
            Owner.Drag(eventData);
        }

        public void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData) {
            Owner.PointerUp();
        }
    }

    private sealed class BoxClick : MonoBehaviour, UnityEngine.EventSystems.IPointerDownHandler {
        public UiSliderField Owner;

        public void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData) {
            if (Owner.grayed) {
                return;
            }
            UiTextEdit.Begin(Owner, Owner.Format(Owner.value));
        }
    }
}
