using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using BepInEx.CupheadDebugMod;
using BepInEx.CupheadDebugMod.Config;
[HarmonyPatch(typeof(LevelSelectList), "SetupList")]
public static class LevelSelectList_SetupList_Patch
{
    private static Color Hex(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            return color;
        }
        return Color.white;
    }

#pragma warning disable Harmony003
    private static float GetLuminance(Color color)
    {
        return color.r * 0.299f + color.g * 0.587f + color.b * 0.114f;
    }
#pragma warning restore Harmony003

    private static Color GetHighlightColor(Color baseColor)
    {
        if (GetLuminance(baseColor) < 0.5f)
        {
            return Color.Lerp(baseColor, Color.white, 0.35f);
        }
        return Color.Lerp(baseColor, Color.black, 0.25f);
    }

    private static Color GetPressedColor(Color baseColor)
    {
        if (GetLuminance(baseColor) < 0.5f)
        {
            return Color.Lerp(baseColor, Color.white, 0.2f);
        }
        return Color.Lerp(baseColor, Color.black, 0.35f);
    }

    private static readonly Scenes[] UnusedLevelSelectScenes = new[]
    {
        Scenes.scene_level_retro_arcade,
        Scenes.scene_level_airship_jelly,
        Scenes.scene_level_airship_stork,
        Scenes.scene_level_dice_palace_pachinko,
        Scenes.scene_level_dice_palace_light,
        Scenes.scene_level_dice_palace_card,
        Scenes.scene_level_test,
        Scenes.scene_level_flying_test,
        Scenes.scene_start
    };

    [HarmonyPrefix]
    private static void Prefix(LevelSelectList __instance)
    {
        if (__instance == null || __instance.scenes == null)
        {
            return;
        }

        if (Settings.ShowUnusedLevels.Value)
        {
            return;
        }

        var groups = new List<LevelSelectList.SceneGroup>(__instance.scenes);
        groups.RemoveAll(g => UnusedLevelSelectScenes.Contains(g.scene));
        __instance.scenes = groups.ToArray();
    }

    internal static void ApplyButtonColors(UnityEngine.UI.Button button, Color baseColor)
    {
        Graphic graphic = button.targetGraphic ?? button.GetComponent<Graphic>();
        if (graphic != null)
        {
            graphic.color = Color.white;
        }

        ColorBlock colors = button.colors;
        colors.normalColor = baseColor;
        colors.highlightedColor = GetHighlightColor(baseColor);
        colors.pressedColor = GetPressedColor(baseColor);
        colors.disabledColor = baseColor;
        colors.colorMultiplier = 1f;
        button.colors = colors;
    }

    public class EntryData
    {
        public string name;
        public Color? bgColor;
        public Color? textColor;
        public int index;

        public EntryData(string name, int index)
        {
            this.name = name;
            this.bgColor = null;
            this.textColor = null;
            this.index = index;
        }

        public EntryData(string name, Color bgColor, Color textColor, int index)
        {
            this.name = name;
            this.bgColor = bgColor;
            this.textColor = textColor;
            this.index = index;
        }
    }

    private static readonly Dictionary<Scenes, EntryData> entries = new Dictionary<Scenes, EntryData>
    {
        { Scenes.scene_level_veggies, new EntryData("The Root Pack", Hex("#E1A136"), Hex("#000000"), 0) },
        { Scenes.scene_level_frogs, new EntryData("Ribby and Croaks", Hex("#808000"), Hex("#FFFFFF"), 1) },
        { Scenes.scene_level_slime, new EntryData("Goopy Le Grande", Hex("#6495ED"), Hex("#000000"), 2) },
        { Scenes.scene_level_flying_blimp, new EntryData("Hilda Berg", Hex("#A52A2A"), Hex("#FFCF10"), 3) },
        { Scenes.scene_level_flower, new EntryData("Cagney Carnation", Hex("#FFA500"), Hex("#000000"), 4) },
        { Scenes.scene_level_baroness, new EntryData("Baroness Von Bon Bon", Hex("#FF69B4"), Hex("#FFFFFF"), 5) },
        { Scenes.scene_level_flying_bird, new EntryData("Wally Warbles", Hex("#0000CD"), Hex("#FFFFFF"), 6) },
        { Scenes.scene_level_flying_genie, new EntryData("Djimmi The Great", Hex("#CD5C5C"), Hex("#48D1CC"), 7) },
        { Scenes.scene_level_clown, new EntryData("Beppi The Clown", Hex("#B22222"), Hex("#FFFFFF"), 8) },
        { Scenes.scene_level_dragon, new EntryData("Grim Matchstick", Hex("#9ACD32"), Hex("#000000"), 9) },
        { Scenes.scene_level_bee, new EntryData("Rumor Honeybottoms", Hex("#FDBF20"), Hex("#000000"), 10) },
        { Scenes.scene_level_robot, new EntryData("Dr. Kahl's Robot", Hex("#A9A9A9"), Hex("#000000"), 11) },
        { Scenes.scene_level_sally_stage_play, new EntryData("Sally Stageplay", Hex("#20B2AA"), Hex("#000000"), 12) },
        { Scenes.scene_level_mouse, new EntryData("Werner Werman", Hex("#A0522D"), Hex("#FFFFFF"), 13) },
        { Scenes.scene_level_pirate, new EntryData("Captain Brineybeard", Hex("#DC143C"), Hex("#FFFFFF"), 14) },
        { Scenes.scene_level_flying_mermaid, new EntryData("Cala Maria", Hex("#A0E1C0"), Hex("#433650"), 15) },
        { Scenes.scene_level_train, new EntryData("Phantom Express", Hex("#9370DB"), Hex("#FFFFFF"), 16) },
        { Scenes.scene_level_devil, new EntryData("The Devil", Hex("#000000"), Hex("#FFCF10"), 17) },

        // Levels

        { Scenes.scene_level_platforming_1_1F, new EntryData("Forest Follies", 0) },
        { Scenes.scene_level_platforming_1_2F, new EntryData("Treetop Trouble", 1) },
        { Scenes.scene_level_platforming_2_1F, new EntryData("Funfair Fever", 2) },
        { Scenes.scene_level_platforming_2_2F, new EntryData("Funhouse Frazzle", 3) },
        { Scenes.scene_level_platforming_3_2F, new EntryData("Rugged Ridge", 4) },
        { Scenes.scene_level_platforming_3_1F, new EntryData("Perilous Piers", 5) },

        { Scenes.scene_level_tutorial, new EntryData("Tutorial", 6) },
        { Scenes.scene_level_mausoleum, new EntryData("Mausoleum", 7) },

        // Dice Palace

        { Scenes.scene_level_dice_palace_booze, new EntryData("1 - Tipsy Troop", Hex("#87CEFA"), Hex("#000000"), 0) },
        { Scenes.scene_level_dice_palace_chips, new EntryData("2 - Chips Bettigan", Hex("#1E90FF"), Hex("#FFCF10"), 1) },
        { Scenes.scene_level_dice_palace_cigar, new EntryData("3 - Mr. Wheezy", Hex("#8B4513"), Hex("#FFCF10"), 2) },
        { Scenes.scene_level_dice_palace_domino, new EntryData("4 - Pip and Dot", Hex("#D3D3D3"), Hex("#000000"), 3) },
        { Scenes.scene_level_dice_palace_rabbit, new EntryData("5 - Hopus Pocus", Hex("#4169E1"), Hex("#FFFFFF"), 4) },
        { Scenes.scene_level_dice_palace_flying_horse, new EntryData("6 - Phear Lap", Hex("#483D8B"), Hex("#FFFFFF"), 5) },
        { Scenes.scene_level_dice_palace_roulette, new EntryData("7 - Pirouletta", Hex("#228B22"), Hex("#FFCF10"), 6) },
        { Scenes.scene_level_dice_palace_eight_ball, new EntryData("8 - Mangosteen", Hex("#000000"), Hex("#FFFFFF"), 7) },
        { Scenes.scene_level_dice_palace_flying_memory, new EntryData("9 - Mr. Chimes", Hex("#63B5A9"), Hex("#000000"), 8) },
        { Scenes.scene_level_dice_palace_main, new EntryData("King Dice", Hex("#8A2BE2"), Hex("#FFFFFF"), 9) },

        // Maps

        { Scenes.scene_map_world_1, new EntryData("Isle 1", 0) },
        { Scenes.scene_map_world_2, new EntryData("Isle 2", 1) },
        { Scenes.scene_map_world_3, new EntryData("Isle 3", 2) },
        { Scenes.scene_map_world_4, new EntryData("Hell", 3) },

        // Cutscenes

        { Scenes.scene_cutscene_intro, new EntryData("Intro", 0) },
        { Scenes.scene_cutscene_world2, new EntryData("Isle 2 Intro", 1) },
        { Scenes.scene_cutscene_world3, new EntryData("Isle 3 Intro", 2) },
        { Scenes.scene_cutscene_kingdice, new EntryData("King Dice Intro", 3) },
        { Scenes.scene_cutscene_devil, new EntryData("The Devil Intro", 4) },
        { Scenes.scene_cutscene_outro, new EntryData("Outro", 5) },
        { Scenes.scene_cutscene_credits, new EntryData("Credits", 6) },

        // Others

        { Scenes.scene_title, new EntryData("Title Screen", 0) },
        { Scenes.scene_slot_select, new EntryData("File Select", 1) },
        { Scenes.scene_shop, new EntryData("Shop", 2) },
        { Scenes.scene_win, new EntryData("Win", 3) },
        { Scenes.scene_level_house_elder_kettle, new EntryData("Elder Kettle's House", 4) },
        { Scenes.scene_level_shmup_tutorial, new EntryData("Plane Tutorial", 5) },
        { Scenes.scene_level_dice_gate, new EntryData("Die House", 6) },

#if v1_3
        // DLC

        { Scenes.scene_level_old_man, new EntryData("Glumstone The Giant", Hex("#DEB887"), Hex("#000000"), 0) },
        { Scenes.scene_level_snow_cult, new EntryData("Mortimer Freeze", Hex("#9400D3"), Hex("#00CED1"), 1) },
        { Scenes.scene_level_airplane, new EntryData("The Howling Aces", Hex("#DAA520"), Hex("#DC143C"), 2) },
        { Scenes.scene_level_flying_cowboy, new EntryData("Esther Winchester", Hex("#D2691E"), Hex("#FFCF10"), 3) },
        { Scenes.scene_level_rum_runners, new EntryData("Moonshine Mob", Hex("#008080"), Hex("#FFCF10"), 4) },
        { Scenes.scene_level_saltbaker, new EntryData("Chef Saltbaker", Hex("#D3D3D3"), Hex("#000000"), 5) },

        { Scenes.scene_level_chess_pawn, new EntryData("Pawns", 6) },
        { Scenes.scene_level_chess_knight, new EntryData("Knight", 7) },
        { Scenes.scene_level_chess_bishop, new EntryData("Bishop", 8) },
        { Scenes.scene_level_chess_rook, new EntryData("Rook", 9) },
        { Scenes.scene_level_chess_queen, new EntryData("Queen", 10) },
        { Scenes.scene_level_chess_castle, new EntryData("King of Games' Castle", 11) },
        { Scenes.scene_level_graveyard, new EntryData("Angel and Demon", 12) },
        { Scenes.scene_level_chalice_tutorial, new EntryData("Chalice Tutorial", 13) },

        { Scenes.scene_shop_DLC, new EntryData("DLC Shop", 14) },
        { Scenes.scene_cutscene_dlc_ending, new EntryData("DLC Ending", 15) },
        { Scenes.scene_cutscene_dlc_credits_comic, new EntryData("DLC Credits Comic", 16) },

        { Scenes.scene_map_world_DLC, new EntryData("Isle 4", 17) }
#endif

    };

    internal static readonly Dictionary<Weapon, LoadoutEntryData> WeaponEntries = new Dictionary<Weapon, LoadoutEntryData>
    {
        { Weapon.level_weapon_peashot, new LoadoutEntryData("Peashooter", 0) },
        { Weapon.level_weapon_spreadshot, new LoadoutEntryData("Spread", 1) },
        { Weapon.level_weapon_homing, new LoadoutEntryData("Chaser", 2) },
        { Weapon.level_weapon_bouncer, new LoadoutEntryData("Lobber", 3) },
        { Weapon.level_weapon_charge, new LoadoutEntryData("Charge", 4) },
        { Weapon.level_weapon_boomerang, new LoadoutEntryData("Roundabout", 5) },
#if v1_3
        { Weapon.level_weapon_crackshot, new LoadoutEntryData("Crackshot", 6) },
        { Weapon.level_weapon_wide_shot, new LoadoutEntryData("Converge", 7) },
        { Weapon.level_weapon_upshot, new LoadoutEntryData("Twist-Up", 8) },
        { Weapon.None, new LoadoutEntryData("None", 9) },
#else
        { Weapon.None, new LoadoutEntryData("None", 6) },
#endif
    };

    internal static readonly Dictionary<Super, LoadoutEntryData> SuperEntries = new Dictionary<Super, LoadoutEntryData>
    {
        { Super.level_super_beam, new LoadoutEntryData("1 - Energy Beam", 0) },
        { Super.level_super_invincible, new LoadoutEntryData("2 - Invincibility", 1) },
        { Super.level_super_ghost, new LoadoutEntryData("3 - Giant Ghost", 2) },
        { Super.None, new LoadoutEntryData("None", 3) }
    };

    internal static readonly Dictionary<Charm, LoadoutEntryData> CharmEntries = new Dictionary<Charm, LoadoutEntryData>
    {
        { Charm.charm_health_up_1, new LoadoutEntryData("Heart", 0) },
        { Charm.charm_super_builder, new LoadoutEntryData("Coffee", 1) },
        { Charm.charm_smoke_dash, new LoadoutEntryData("Smoke Bomb", 2) },
        { Charm.charm_parry_plus, new LoadoutEntryData("P. Sugar", 3) },
        { Charm.charm_health_up_2, new LoadoutEntryData("Twin Heart", 4) },
        { Charm.charm_parry_attack, new LoadoutEntryData("Whetstone", 5) },
#if v1_3
        { Charm.charm_chalice, new LoadoutEntryData("Astral Cookie", 6) },
        { Charm.charm_healer, new LoadoutEntryData("Heart Ring", 7) },
        { Charm.charm_curse, new LoadoutEntryData("Cursed Relic", 8) },
        { Charm.charm_EX, new LoadoutEntryData("Divine Relic", 9) },
        { Charm.None, new LoadoutEntryData("None", 10) },
#else
        { Charm.None, new LoadoutEntryData("None", 5) },
#endif
    };

    internal static readonly Dictionary<Level.Mode, string> DifficultyNames = new Dictionary<Level.Mode, string>
    {
        { Level.Mode.Easy, "Simple" },
        { Level.Mode.Normal, "Regular" },
        { Level.Mode.Hard, "Expert" },
    };

    private class EntryButtonInfo
    {
        public UnityEngine.UI.Button button;
        public EntryData data;
        public int index;
    }

    [HarmonyPostfix]
    private static void Postfix(LevelSelectList __instance)
    {
        List<EntryButtonInfo> buttonList = new List<EntryButtonInfo>();

        foreach (UnityEngine.UI.Button b in __instance.contentPanel.GetComponentsInChildren<UnityEngine.UI.Button>(true))
        {
            Scenes scene;
            if (Enum.IsDefined(typeof(Scenes), b.name) && (scene = (Scenes)Enum.Parse(typeof(Scenes), b.name)) == scene)
            {
                if (entries.TryGetValue(scene, out var entryData))
                {
                    buttonList.Add(new EntryButtonInfo { button = b, data = entryData, index = entryData.index });
                }
            }
        }

        buttonList.Sort((a, b) => a.index.CompareTo(b.index));

        int siblingIndex = 0;
        foreach (var info in buttonList)
        {
            UnityEngine.UI.Button button = info.button;
            EntryData entryData = info.data;

            UnityEngine.UI.Text textComponent = button.GetComponentInChildren<UnityEngine.UI.Text>();
            if (textComponent != null)
            {
                textComponent.text = entryData.name;
                if (entryData.textColor.HasValue)
                {
                    textComponent.color = entryData.textColor.Value;
                }
            }

            if (textComponent != null)
            {
                UnityEngine.UI.Shadow shadow = textComponent.GetComponent<UnityEngine.UI.Shadow>();
                if (shadow != null)
                {
                    shadow.enabled = false;
                }
            }

            if (entryData.bgColor.HasValue)
            {
                ApplyButtonColors(button, entryData.bgColor.Value);
                LevelSelectConfigSettings settings = button.GetComponent<LevelSelectConfigSettings>();
                if (settings == null)
                {
                    settings = button.gameObject.AddComponent<LevelSelectConfigSettings>();
                }

                settings.Initialize(button, textComponent, GetConfigSearch(entryData.name));
            }

            button.transform.SetSiblingIndex(siblingIndex++);
        }
    }

#pragma warning disable Harmony003
    private static string GetConfigSearch(string entryName)
    {
        int separatorIndex = entryName.IndexOf(" - ", StringComparison.Ordinal);
        if (separatorIndex > 0 && int.TryParse(entryName.Substring(0, separatorIndex), out _))
        {
            entryName = entryName.Substring(separatorIndex + 3);
        }

        return entryName == "Dr. Kahl's Robot" ? "Dr. Kahls Robot" : entryName;
    }
#pragma warning restore Harmony003
}

public class LoadoutEntryData
{
    public string name;
    public int index;

    public LoadoutEntryData(string name, int index)
    {
        this.name = name;
        this.index = index;
    }
}

[HarmonyPatch]
public static class LoadoutSelectList_SetupList_Patch
{
    private const string WeaponIconName = "DebugLoadoutWeaponIcon";
    private static LevelHUDWeapon weaponIconTemplate;

    internal static void CacheWeaponIcon(LevelHUDWeapon source)
    {
        if (weaponIconTemplate != null || source == null || source.gameObject.activeInHierarchy)
        {
            return;
        }

        weaponIconTemplate = UnityEngine.Object.Instantiate(source);
        weaponIconTemplate.gameObject.name = WeaponIconName + "Template";
        weaponIconTemplate.gameObject.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(weaponIconTemplate.gameObject);
    }

    private class LoadoutButtonInfo
    {
        public Button button;
        public LoadoutEntryData entry;
    }

    [HarmonyPatch(typeof(LoadoutSelectList), "SetupList")]
    [HarmonyPostfix]
    private static void Postfix(LoadoutSelectList __instance)
    {
        if (__instance == null || __instance.contentPanel == null || Settings.ShowUnusedLevels == null)
        {
            return;
        }

        bool isWeaponList = __instance.mode is LoadoutSelectList.Mode.Primary or LoadoutSelectList.Mode.Secondary;
        List<LoadoutButtonInfo> orderedButtons = new List<LoadoutButtonInfo>();

        foreach (Button button in __instance.contentPanel.GetComponentsInChildren<Button>(true))
        {
            if (isWeaponList && Enum.IsDefined(typeof(Weapon), button.name))
            {
                Weapon weapon = (Weapon)Enum.Parse(typeof(Weapon), button.name);
                ApplyEntryVisibility(button, LevelSelectList_SetupList_Patch.WeaponEntries.TryGetValue(weapon, out LoadoutEntryData weaponEntry), weaponEntry);
                if (weaponEntry != null)
                {
                    if (weapon != Weapon.None)
                    {
                        AddWeaponIcon(button, weapon);
                    }

                    orderedButtons.Add(new LoadoutButtonInfo { button = button, entry = weaponEntry });
                }
            }

            if (__instance.mode == LoadoutSelectList.Mode.Super && Enum.IsDefined(typeof(Super), button.name))
            {
                Super super = (Super)Enum.Parse(typeof(Super), button.name);
                ApplyEntryVisibility(button, LevelSelectList_SetupList_Patch.SuperEntries.TryGetValue(super, out LoadoutEntryData superEntry), superEntry);
                if (superEntry != null)
                {
                    orderedButtons.Add(new LoadoutButtonInfo { button = button, entry = superEntry });
                }
            }

            if (__instance.mode == LoadoutSelectList.Mode.Charm && Enum.IsDefined(typeof(Charm), button.name))
            {
                Charm charm = (Charm)Enum.Parse(typeof(Charm), button.name);
                ApplyEntryVisibility(button, LevelSelectList_SetupList_Patch.CharmEntries.TryGetValue(charm, out LoadoutEntryData charmEntry), charmEntry);
                if (charmEntry != null)
                {
                    orderedButtons.Add(new LoadoutButtonInfo { button = button, entry = charmEntry });
                }
            }

            Text textComponent = button.GetComponentInChildren<Text>();
            if (textComponent != null && TryGetDisplayName(__instance.mode, button.name, out string displayName))
            {
                textComponent.text = displayName;
            }
        }

        orderedButtons.Sort((a, b) => a.entry.index.CompareTo(b.entry.index));
        for (int i = 0; i < orderedButtons.Count; i++)
        {
            orderedButtons[i].button.transform.SetSiblingIndex(i);
        }
    }

    private static void ApplyEntryVisibility(Button button, bool hasEntry, LoadoutEntryData entry)
    {
        button.gameObject.SetActive(Settings.ShowUnusedLevels.Value || hasEntry);
    }

#pragma warning disable Harmony003
    private static void AddWeaponIcon(Button button, Weapon weapon)
    {
        if (weaponIconTemplate == null || button.transform.Find(WeaponIconName) != null)
        {
            return;
        }

        GameObject iconObject = UnityEngine.Object.Instantiate(weaponIconTemplate.gameObject, button.transform, false);
        iconObject.name = WeaponIconName;
        UnityEngine.Object.DestroyImmediate(iconObject.GetComponent<LevelHUDWeapon>());
        iconObject.SetActive(true);

        Animator animator = iconObject.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(weapon.ToString());
        }

        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(1f, 0.5f);
        iconRect.anchorMax = new Vector2(1f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(-26f, -12f);
        iconRect.localScale = Vector3.one * 0.32f;
    }
#pragma warning restore Harmony003

    private static bool TryGetDisplayName(LoadoutSelectList.Mode mode, string buttonName, out string displayName)
    {
        displayName = null;

        if (mode is LoadoutSelectList.Mode.Primary or LoadoutSelectList.Mode.Secondary &&
            Enum.IsDefined(typeof(Weapon), buttonName))
        {
            Weapon weapon = (Weapon)Enum.Parse(typeof(Weapon), buttonName);
            if (LevelSelectList_SetupList_Patch.WeaponEntries.TryGetValue(weapon, out LoadoutEntryData weaponEntry))
            {
                displayName = weaponEntry.name;
                return true;
            }
        }

        if (mode == LoadoutSelectList.Mode.Super && Enum.IsDefined(typeof(Super), buttonName))
        {
            if (LevelSelectList_SetupList_Patch.SuperEntries.TryGetValue((Super)Enum.Parse(typeof(Super), buttonName), out LoadoutEntryData superEntry))
            {
                displayName = superEntry.name;
                return true;
            }
        }

        if (mode == LoadoutSelectList.Mode.Charm && Enum.IsDefined(typeof(Charm), buttonName))
        {
            if (LevelSelectList_SetupList_Patch.CharmEntries.TryGetValue((Charm)Enum.Parse(typeof(Charm), buttonName), out LoadoutEntryData charmEntry))
            {
                displayName = charmEntry.name;
                return true;
            }
        }

        if (mode == LoadoutSelectList.Mode.Difficulty && Enum.IsDefined(typeof(Level.Mode), buttonName))
        {
            return LevelSelectList_SetupList_Patch.DifficultyNames.TryGetValue((Level.Mode)Enum.Parse(typeof(Level.Mode), buttonName), out displayName);
        }

        return false;
    }
}

public class LoadoutWeaponIconCache : PluginComponent
{
    private static readonly FieldInfo WeaponIconPrefabField = AccessTools.Field(typeof(LevelHUDPlayer), "weaponIconPrefab");

    private void Update()
    {
        if (WeaponIconPrefabField == null)
        {
            return;
        }

        LevelHUDPlayer hudPlayer = Resources.FindObjectsOfTypeAll<LevelHUDPlayer>().FirstOrDefault();
        if (hudPlayer != null)
        {
            LoadoutSelectList_SetupList_Patch.CacheWeaponIcon(WeaponIconPrefabField.GetValue(hudPlayer) as LevelHUDWeapon);
        }
    }
}

public class LevelSelectConfigSettings : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private const string ConfigurationManagerTypeName = "ConfigurationManager.ConfigurationManager";

    private RectTransform entryRect;
    private GameObject settingsObject;
    private Text settingsText;
    private Color textColor;
    private string searchText;

    public void Initialize(Button entryButton, Text entryText, string search)
    {
        entryRect = entryButton.GetComponent<RectTransform>();
        searchText = search;

        if (settingsObject == null)
        {
            settingsObject = new GameObject("DebugConfigSettings", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            settingsObject.transform.SetParent(entryButton.transform, false);

            RectTransform settingsRect = settingsObject.GetComponent<RectTransform>();
            settingsRect.anchorMin = new Vector2(1f, 0.5f);
            settingsRect.anchorMax = new Vector2(1f, 0.5f);
            settingsRect.pivot = new Vector2(0.5f, 0.5f);
            settingsRect.anchoredPosition = new Vector2(-20f, -5f);
            settingsRect.sizeDelta = new Vector2(28f, 28f);

            Image settingsImage = settingsObject.GetComponent<Image>();
            settingsImage.color = Color.clear;

            Button settingsButton = settingsObject.GetComponent<Button>();
            settingsButton.targetGraphic = settingsImage;
            settingsButton.navigation = new Navigation { mode = Navigation.Mode.None };
            ColorBlock colors = settingsButton.colors;
            colors.normalColor = Color.clear;
            colors.highlightedColor = Color.clear;
            colors.pressedColor = Color.clear;
            colors.disabledColor = Color.clear;
            settingsButton.colors = colors;
            settingsButton.onClick.AddListener(OpenConfigurationManager);

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(settingsObject.transform, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            settingsText = textObject.GetComponent<Text>();
            settingsText.text = "...";
            settingsText.alignment = TextAnchor.MiddleCenter;
            settingsText.raycastTarget = false;
            settingsText.fontSize = 24;
        }

        settingsText.font = entryText != null && entryText.font != null ? entryText.font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        textColor = entryText != null ? entryText.color : Color.white;
        SetTextOpacity(1f);

        Button textButton = settingsObject.GetComponent<Button>();
        textButton.targetGraphic = settingsText;
        ColorBlock textColors = textButton.colors;
        textColors.normalColor = textColor;
        textColors.highlightedColor = WithOpacity(textColor, 0.65f);
        textColors.pressedColor = WithOpacity(textColor, 0.35f);
        textColors.disabledColor = WithOpacity(textColor, 0.35f);
        textColors.colorMultiplier = 1f;
        textButton.colors = textColors;
        settingsObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetTextOpacity(1f);
        settingsObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        settingsObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SetTextOpacity(0.35f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SetTextOpacity(0.65f);
    }

    private void SetTextOpacity(float opacity)
    {
        settingsText.color = WithOpacity(textColor, opacity);
    }

    private static Color WithOpacity(Color color, float opacity)
    {
        color.a *= opacity;
        return color;
    }

    private void OpenConfigurationManager()
    {
        Type configurationManagerType = AccessTools.TypeByName(ConfigurationManagerTypeName);
        Component configurationManager = configurationManagerType == null
            ? null
            : UnityEngine.Object.FindObjectOfType(configurationManagerType) as Component;
        if (configurationManager == null)
        {
            return;
        }

        configurationManagerType.GetProperty("DisplayingWindow")?.SetValue(configurationManager, true, null);
        StartCoroutine(SetConfigurationSearchNextFrame(configurationManager, configurationManagerType));
    }

    private IEnumerator SetConfigurationSearchNextFrame(Component configurationManager, Type configurationManagerType)
    {
        yield return null;

        PropertyInfo searchProperty = configurationManagerType.GetProperty("SearchString", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        searchProperty?.GetSetMethod(true)?.Invoke(configurationManager, new object[] { searchText });
    }
}
