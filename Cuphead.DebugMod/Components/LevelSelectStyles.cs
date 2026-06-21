using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
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

    private static float GetLuminance(Color color)
    {
        return color.r * 0.299f + color.g * 0.587f + color.b * 0.114f;
    }

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

        if (!Settings.HideUnusedLevels.Value)
        {
            return;
        }

        var groups = new List<LevelSelectList.SceneGroup>(__instance.scenes);
        groups.RemoveAll(g => UnusedLevelSelectScenes.Contains(g.scene));
        __instance.scenes = groups.ToArray();
    }

    private static void ApplyButtonColors(UnityEngine.UI.Button button, Color baseColor)
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
        { Scenes.scene_level_dice_gate, new EntryData("Die House", 6) }

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
        { Scenes.scene_cutscene_dlc_credits_comic, new EntryData("DLC Credits Comic", 16) }

        { Scenes.scene_map_world_DLC, new EntryData("Isle 4", 17) }
#endif

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
            }

            button.transform.SetSiblingIndex(siblingIndex++);
        }
    }
}