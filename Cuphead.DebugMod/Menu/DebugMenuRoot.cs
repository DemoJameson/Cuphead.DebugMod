using System;
using System.Collections.Generic;
using BepInEx.CupheadDebugMod.Components;
using BepInEx.CupheadDebugMod.Config;
using BepInEx.CupheadDebugMod.Rng;
using Rewired.Integration.UnityUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace BepInEx.CupheadDebugMod.Menu;

/// <summary>
/// Hosts the in-game debug menu: a persistent screen-space uGUI canvas toggled with a
/// hotkey. Existing IMGUI overlays (Info HUD, toasts) are unaffected.
/// </summary>
public class DebugMenuRoot : PluginComponent {
    private GameObject root;
    private Canvas menuCanvas;
    private RectTransform windowRect;
    private Rect lastPaintedRect;
    private RngPage page;
    private Text profileLabel;
    private Text footerStatus;
    private Button copyToCustomButton;
    private bool open;
    private bool prevCursorVisible;
    private CursorLockMode prevCursorLock;
    private RewiredStandaloneInputModule rewiredModule;
    private readonly List<BossTab> bossTabs = new List<BossTab>();

    /// <summary>One selectable boss row in the sidebar; the selected one is highlighted.</summary>
    private sealed class BossTab {
        public string StageId;
        public Image Highlight;
        public Text Label;
    }

    private void Awake() {
        Settings.OnKeyUpdate += OnKeyUpdate;
        HookHelper.ActiveSceneChanged(() => {
            if (open) {
                EnableMouseInput();
            }
        });
    }

    private void OnDestroy() {
        Settings.OnKeyUpdate -= OnKeyUpdate;
    }

    private void OnKeyUpdate() {
        if (Settings.RngMenu.IsDownEx()) {
            if (open) {
                Close();
            } else {
                Open();
            }
            return;
        }

        if (open && Input.GetKeyDown(KeyCode.Escape)) {
            Close();
        }
    }

    private void Open() {
        RngProfiles.Initialize();
        EnsureUi();
        prevCursorVisible = Cursor.visible;
        prevCursorLock = Cursor.lockState;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        EnableMouseInput();
        // resize + rescale for the current screen (also covers resolutions changed while
        // the menu was closed); page.Refresh below rebuilds with the new content width
        UpdateCanvasScale();
        root.SetActive(true);
        page.Refresh();
        open = true;
    }

    private void Close() {
        open = false;
        UiTextEdit.Cancel();
        UiTooltip.Hide();
        if (root != null) {
            root.SetActive(false);
        }
        Cursor.visible = prevCursorVisible;
        Cursor.lockState = prevCursorLock;
        RestoreMouseInput();
    }

    private void Update() {
        if (!open) {
            return;
        }
        UiTextEdit.Tick();
        UiSliderField.Tick();
        UpdateCanvasScale();
    }

    /// <summary>
    /// Recomputes the canvas scale and window placement from the actively painted scene -
    /// the main camera's pixel rect, the same source the IMGUI panels use in
    /// DebugInfo.GameAreaRect. CanvasScaler misses resolution changes the game drives
    /// itself on this engine version, and the painted area can letterbox inside the
    /// window, so both the scale and the window placement follow the camera.
    /// </summary>
    private void UpdateCanvasScale() {
        if (menuCanvas == null || windowRect == null) {
            return;
        }
        Camera cam = Camera.main;
        Rect painted = cam != null ? cam.pixelRect : new Rect(0f, 0f, Screen.width, Screen.height);
        if (painted.width <= 0f || painted.height <= 0f) {
            return;
        }
        if (painted == lastPaintedRect) {
            return;
        }
        lastPaintedRect = painted;

        // canvas units stay "1080p pixels" of the painted scene, so fonts and fixed
        // widgets scale with it
        menuCanvas.scaleFactor = painted.height / MenuTheme.ReferenceHeight;

        // the window keeps the same share of the painted area in both axes and hugs its
        // right edge, vertically centered in it
        float widthUnits = painted.width * MenuTheme.ReferenceHeight / painted.height;
        windowRect.sizeDelta = new Vector2(
            widthUnits * MenuTheme.WindowWidthFraction,
            MenuTheme.ReferenceHeight * MenuTheme.WindowHeightFraction);
        windowRect.anchoredPosition = new Vector2(
            (painted.xMax - Screen.width) / menuCanvas.scaleFactor - MenuTheme.Padding,
            (painted.center.y - Screen.height * 0.5f) / menuCanvas.scaleFactor);

        // sliders and other fixed-width controls are computed from the row width at build
        // time, so the page has to rebuild for the new content width
        if (page != null) {
            page.ContentWidth = windowRect.sizeDelta.x - MenuTheme.SidebarWidth;
            if (open) {
                page.Refresh();
            }
        }
    }

    private void EnableMouseInput() {
        if (rewiredModule == null) {
            rewiredModule = FindObjectOfType<RewiredStandaloneInputModule>();
        }
        if (rewiredModule != null) {
            rewiredModule.allowMouseInput = true;
            return;
        }

        // Rare fallback: no game input module at all, provide a minimal one for the menu.
        if (FindObjectOfType<EventSystem>() == null) {
            GameObject eventSystem = new GameObject("DebugModEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            UnityEngine.Object.DontDestroyOnLoad(eventSystem);
        }
    }

    private void RestoreMouseInput() {
        if (rewiredModule == null) {
            rewiredModule = FindObjectOfType<RewiredStandaloneInputModule>();
        }
        if (rewiredModule != null) {
            // Same rule LevelSelectorFixer applies on scene changes.
            rewiredModule.allowMouseInput = PluginComponent.CurrentSceneName == nameof(Scenes.scene_menu);
        }
    }

    private void EnsureUi() {
        if (root != null) {
            return;
        }

        root = new GameObject("DebugModMenu", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
        UnityEngine.Object.DontDestroyOnLoad(root);

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30000;
        menuCanvas = canvas;

        // Backdrop: swallows clicks so they cannot reach the game while the menu is open.
        GameObject backdrop = Ui.Create("Backdrop", root.transform);
        Ui.Stretch(backdrop);
        Image backdropImage = backdrop.AddComponent<Image>();
        backdropImage.color = MenuTheme.Backdrop;
        backdropImage.raycastTarget = true;

        GameObject window = Ui.Create("Window", root.transform);
        // right-aligned: the left side of the screen stays clear for the Info HUD panels;
        // UpdateCanvasScale sizes it as a fraction of the screen
        windowRect = Ui.Rect(window);
        windowRect.anchorMin = new Vector2(1f, 0.5f);
        windowRect.anchorMax = new Vector2(1f, 0.5f);
        windowRect.pivot = new Vector2(1f, 0.5f);
        windowRect.anchoredPosition = new Vector2(-MenuTheme.Padding, 0f);
        Ui.Panel(window, MenuTheme.WindowBg).raycastTarget = true;

        BuildHeader(window);
        BuildSidebar(window);
        BuildFooter(window);
        BuildContent(window);
        HighlightStage();
        UpdateCanvasScale();
    }

    private void BuildHeader(GameObject window) {
        GameObject header = Ui.Create("Header", window.transform);
        Ui.Band(header, 0f, MenuTheme.HeaderHeight);
        Ui.Panel(header, MenuTheme.HeaderBg);

        GameObject title = Ui.Create("Title", header.transform);
        RectTransform titleRect = Ui.Rect(title);
        titleRect.anchorMin = new Vector2(0f, 0.5f);
        titleRect.anchorMax = new Vector2(1f, 0.5f);
        titleRect.pivot = new Vector2(0f, 0.5f);
        titleRect.offsetMin = new Vector2(MenuTheme.Padding, 0f);
        titleRect.offsetMax = new Vector2(-530f, 0f);
        Ui.Label(title, "Cuphead DebugMod - RNG", MenuTheme.FontSizeTitle, MenuTheme.Accent, TextAnchor.MiddleLeft);

        GameObject prev = Ui.Create("ProfilePrev", header.transform);
        RectTransform prevRect = Ui.Rect(prev);
        prevRect.anchorMin = new Vector2(1f, 0.5f);
        prevRect.anchorMax = new Vector2(1f, 0.5f);
        prevRect.pivot = new Vector2(1f, 0.5f);
        prevRect.anchoredPosition = new Vector2(-300f, 0f);
        prevRect.sizeDelta = new Vector2(MenuTheme.ControlSize, MenuTheme.ControlSize);
        Ui.MakeButton(prev, () => page.CycleProfile(-1), MenuTheme.ButtonBg);
        BuildHeaderArrow(prev.transform, UiIcons.TriangleLeft);

        GameObject next = Ui.Create("ProfileNext", header.transform);
        RectTransform nextRect = Ui.Rect(next);
        nextRect.anchorMin = new Vector2(1f, 0.5f);
        nextRect.anchorMax = new Vector2(1f, 0.5f);
        nextRect.pivot = new Vector2(1f, 0.5f);
        nextRect.anchoredPosition = new Vector2(-56f, 0f);
        nextRect.sizeDelta = new Vector2(MenuTheme.ControlSize, MenuTheme.ControlSize);
        Ui.MakeButton(next, () => page.CycleProfile(1), MenuTheme.ButtonBg);
        BuildHeaderArrow(next.transform, UiIcons.TriangleRight);

        GameObject profileGo = Ui.Create("ProfileLabel", header.transform);
        RectTransform profileRect = Ui.Rect(profileGo);
        profileRect.anchorMin = new Vector2(1f, 0.5f);
        profileRect.anchorMax = new Vector2(1f, 0.5f);
        profileRect.pivot = new Vector2(1f, 0.5f);
        profileRect.anchoredPosition = new Vector2(-100f, 0f);
        profileRect.sizeDelta = new Vector2(190f, MenuTheme.ControlSize);
        profileLabel = Ui.Label(profileGo, "", MenuTheme.FontSizeLabel, MenuTheme.Text, TextAnchor.MiddleCenter);

        copyToCustomButton = Ui.TextButton(header.transform, "CopyToCustom", "Copy to Custom", 170f, MenuTheme.ControlSize, () => page.CopyToCustom(), MenuTheme.FontSizeSmall).GetComponent<Button>();
        RectTransform copyRect = Ui.Rect(copyToCustomButton.gameObject);
        copyRect.anchorMin = new Vector2(1f, 0.5f);
        copyRect.anchorMax = new Vector2(1f, 0.5f);
        copyRect.pivot = new Vector2(1f, 0.5f);
        copyRect.anchoredPosition = new Vector2(-344f, 0f);
        copyToCustomButton.gameObject.SetActive(false);

        GameObject close = Ui.Create("Close", header.transform);
        RectTransform closeRect = Ui.Rect(close);
        closeRect.anchorMin = new Vector2(1f, 0.5f);
        closeRect.anchorMax = new Vector2(1f, 0.5f);
        closeRect.pivot = new Vector2(1f, 0.5f);
        closeRect.anchoredPosition = new Vector2(-MenuTheme.Padding, 0f);
        closeRect.sizeDelta = new Vector2(MenuTheme.ControlSize, MenuTheme.ControlSize);
        Ui.MakeButton(close, () => Close(), MenuTheme.ButtonBg);
        GameObject closeLabel = Ui.Create("Label", close.transform);
        Ui.Stretch(closeLabel);
        Ui.Label(closeLabel, "X", MenuTheme.FontSizeButton, MenuTheme.Text, TextAnchor.MiddleCenter);
    }

    private void BuildHeaderArrow(Transform buttonTransform, Sprite sprite) {
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

    private void BuildSidebar(GameObject window) {
        GameObject sidebar = Ui.Create("Sidebar", window.transform);
        RectTransform sidebarRect = Ui.Rect(sidebar);
        sidebarRect.anchorMin = new Vector2(0f, 0f);
        sidebarRect.anchorMax = new Vector2(0f, 1f);
        sidebarRect.offsetMin = new Vector2(0f, MenuTheme.FooterHeight);
        sidebarRect.offsetMax = new Vector2(MenuTheme.SidebarWidth, -MenuTheme.HeaderHeight);
        Ui.Panel(sidebar, MenuTheme.SidebarBg);

        GameObject sectionTitle = Ui.Create("SectionTitle", sidebar.transform);
        Ui.Band(sectionTitle, 0f, 38f);
        Ui.Panel(sectionTitle, MenuTheme.SectionBg);
        GameObject sectionLabel = Ui.Create("Label", sectionTitle.transform);
        Ui.Stretch(sectionLabel);
        Ui.Label(sectionLabel, "RNG", MenuTheme.FontSizeLabel, MenuTheme.Text, TextAnchor.MiddleLeft);
        RectTransform sectionLabelRect = Ui.Rect(sectionLabel);
        sectionLabelRect.offsetMin = new Vector2(MenuTheme.Padding, 0f);

        // one selectable row per migrated stage, straight below the RNG header
        bossTabs.Clear();
        float y = 42f;
        string lastStageId = null;
        foreach (RngSpot spot in RngSpots.All) {
            string stageId = RngSpots.StageOf(spot.Id);
            if (stageId == lastStageId) {
                continue;
            }
            lastStageId = stageId;
            BuildBossTab(sidebar, stageId, RngSpots.StageLabel(stageId), y);
            y += 50f;
        }

        GameObject globalTitle = Ui.Create("GlobalTitle", sidebar.transform);
        Ui.Band(globalTitle, y + 4f, 38f);
        Ui.Panel(globalTitle, MenuTheme.SectionBg);
        GameObject globalTitleLabel = Ui.Create("Label", globalTitle.transform);
        Ui.Stretch(globalTitleLabel);
        Ui.Label(globalTitleLabel, "Global", MenuTheme.FontSizeLabel, MenuTheme.Text, TextAnchor.MiddleLeft);
        RectTransform globalTitleRect = Ui.Rect(globalTitleLabel);
        globalTitleRect.offsetMin = new Vector2(MenuTheme.Padding, 0f);

        GameObject formsRow = Ui.Create("PatternFormsRow", sidebar.transform);
        RectTransform formsRowRect = Ui.Rect(formsRow);
        formsRowRect.anchorMin = new Vector2(0f, 1f);
        formsRowRect.anchorMax = new Vector2(1f, 1f);
        formsRowRect.pivot = new Vector2(0f, 1f);
        formsRowRect.anchoredPosition = new Vector2(0f, -(y + 46f));
        formsRowRect.sizeDelta = new Vector2(0f, 46f);
        Ui.Panel(formsRow, MenuTheme.SidebarBg);
        UiCheck formsCheck = UiCheck.Create(formsRow.transform, "PatternForms", MenuTheme.CheckSize, Settings.EnablePatternForms.Value, on => {
            Settings.EnablePatternForms.Value = on;
            RngState.ResetSession();
            if (page != null) {
                if (!on) {
                    // collapsing on the global switch keeps stale expansions from resurfacing
                    page.CollapseForms("");
                }
                page.Refresh();
            }
        });
        RectTransform formsCheckRect = Ui.Rect(formsCheck.Root);
        formsCheckRect.anchorMin = new Vector2(0f, 0.5f);
        formsCheckRect.anchorMax = new Vector2(0f, 0.5f);
        formsCheckRect.pivot = new Vector2(0f, 0.5f);
        formsCheckRect.anchoredPosition = new Vector2(MenuTheme.Padding, 0f);
        GameObject formsLabel = Ui.Create("Label", formsRow.transform);
        RectTransform formsLabelRect = Ui.Rect(formsLabel);
        formsLabelRect.anchorMin = new Vector2(0f, 0.5f);
        formsLabelRect.anchorMax = new Vector2(1f, 0.5f);
        formsLabelRect.pivot = new Vector2(0f, 0.5f);
        formsLabelRect.offsetMin = new Vector2(MenuTheme.Padding + 28f, 0f);
        formsLabelRect.offsetMax = new Vector2(-MenuTheme.Padding, 0f);
        Ui.Label(formsLabel, "Pattern forms", MenuTheme.FontSizeSmall, MenuTheme.Text, TextAnchor.MiddleLeft);

        GameObject note = Ui.Create("Note", sidebar.transform);
        RectTransform noteRect = Ui.Rect(note);
        noteRect.anchorMin = new Vector2(0f, 0f);
        noteRect.anchorMax = new Vector2(1f, 0f);
        noteRect.pivot = new Vector2(0f, 0f);
        noteRect.anchoredPosition = new Vector2(0f, MenuTheme.Padding);
        noteRect.sizeDelta = new Vector2(0f, 60f);
        Ui.Label(note, "More bosses join this menu as they migrate over.", MenuTheme.FontSizeSmall, MenuTheme.TextDim, TextAnchor.UpperLeft);
    }

    private void BuildBossTab(GameObject sidebar, string stageId, string label, float y) {
        BossTab tab = new BossTab();
        tab.StageId = stageId;

        GameObject row = Ui.Create("Boss_" + stageId, sidebar.transform);
        RectTransform rowRect = Ui.Rect(row);
        rowRect.anchorMin = new Vector2(0f, 1f);
        rowRect.anchorMax = new Vector2(1f, 1f);
        rowRect.pivot = new Vector2(0f, 1f);
        rowRect.anchoredPosition = new Vector2(0f, -y);
        rowRect.sizeDelta = new Vector2(0f, 46f);

        // the row's own image carries the hover tint; the highlight child paints the
        // selection on top of it (invisible until selected)
        GameObject highlightGo = Ui.Create("Highlight", row.transform);
        Ui.Stretch(highlightGo);
        tab.Highlight = Ui.Panel(highlightGo, MenuTheme.Accent);
        RectTransform highlightRect = tab.Highlight.rectTransform;
        highlightRect.offsetMin = new Vector2(3f, 3f);
        highlightRect.offsetMax = new Vector2(-3f, -3f);

        GameObject labelGo = Ui.Create("Label", row.transform);
        Ui.Stretch(labelGo);
        tab.Label = Ui.Label(labelGo, label, MenuTheme.FontSizeLabel, MenuTheme.Text, TextAnchor.MiddleLeft);
        RectTransform labelRect = Ui.Rect(labelGo);
        labelRect.offsetMin = new Vector2(MenuTheme.Padding, 0f);

        string captured = stageId;
        Ui.MakeButton(row, () => SelectStage(captured), MenuTheme.ButtonBg);

        bossTabs.Add(tab);
    }

    private void SelectStage(string stageId) {
        if (page != null && page.SelectStage(stageId)) {
            HighlightStage();
        }
    }

    private void HighlightStage() {
        if (page == null) {
            return;
        }
        string selected = page.SelectedStage;
        foreach (BossTab tab in bossTabs) {
            bool active = tab.StageId == selected;
            tab.Highlight.enabled = active;
            tab.Label.color = active ? MenuTheme.WindowBg : MenuTheme.Text;
        }
    }

    private void BuildFooter(GameObject window) {
        GameObject footer = Ui.Create("Footer", window.transform);
        RectTransform footerRect = Ui.Rect(footer);
        footerRect.anchorMin = new Vector2(0f, 0f);
        footerRect.anchorMax = new Vector2(1f, 0f);
        footerRect.pivot = new Vector2(0.5f, 0f);
        footerRect.anchoredPosition = Vector2.zero;
        footerRect.sizeDelta = new Vector2(0f, MenuTheme.FooterHeight);
        Ui.Panel(footer, MenuTheme.HeaderBg);

        GameObject hint = Ui.Create("Hint", footer.transform);
        RectTransform hintRect = Ui.Rect(hint);
        hintRect.anchorMin = new Vector2(0f, 0.5f);
        hintRect.anchorMax = new Vector2(1f, 0.5f);
        hintRect.pivot = new Vector2(0f, 0.5f);
        hintRect.offsetMin = new Vector2(MenuTheme.Padding, 0f);
        hintRect.offsetMax = new Vector2(-340f, 0f);
        Ui.Label(hint, "F7 close - edits save to BepInEx/config/Cuphead.DebugMod/rng_profiles.cfg", MenuTheme.FontSizeSmall, MenuTheme.TextDim, TextAnchor.MiddleLeft);

        GameObject status = Ui.Create("Status", footer.transform);
        RectTransform statusRect = Ui.Rect(status);
        statusRect.anchorMin = new Vector2(1f, 0.5f);
        statusRect.anchorMax = new Vector2(1f, 0.5f);
        statusRect.pivot = new Vector2(1f, 0.5f);
        statusRect.anchoredPosition = new Vector2(-MenuTheme.Padding, 0f);
        statusRect.sizeDelta = new Vector2(320f, MenuTheme.FooterHeight);
        footerStatus = Ui.Label(status, "", MenuTheme.FontSizeSmall, MenuTheme.TextDim, TextAnchor.MiddleRight);
    }

    private void BuildContent(GameObject window) {
        GameObject scroll = Ui.Create("Scroll", window.transform);
        RectTransform scrollRect = Ui.Rect(scroll);
        scrollRect.anchorMin = new Vector2(0f, 0f);
        scrollRect.anchorMax = new Vector2(1f, 1f);
        scrollRect.offsetMin = new Vector2(MenuTheme.SidebarWidth, MenuTheme.FooterHeight);
        scrollRect.offsetMax = new Vector2(0f, -MenuTheme.HeaderHeight);

        ScrollRect scrollRectComponent = scroll.AddComponent<ScrollRect>();
        scrollRectComponent.horizontal = false;
        scrollRectComponent.vertical = true;
        scrollRectComponent.movementType = ScrollRect.MovementType.Clamped;
        scrollRectComponent.scrollSensitivity = 30f;

        GameObject viewport = Ui.Create("Viewport", scroll.transform);
        Ui.Stretch(viewport);
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = MenuTheme.WindowBg;
        viewportImage.raycastTarget = true;
        viewport.AddComponent<RectMask2D>();

        GameObject content = Ui.Create("Content", viewport.transform);
        RectTransform contentRect = Ui.Rect(content);
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;

        scrollRectComponent.viewport = viewport.GetComponent<RectTransform>();
        scrollRectComponent.content = contentRect;

        page = new RngPage(contentRect, profileLabel, footerStatus, copyToCustomButton);
    }
}
