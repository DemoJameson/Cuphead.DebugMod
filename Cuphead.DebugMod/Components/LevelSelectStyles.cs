using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public class BossData
    {
        public string name;
        public Color primaryColor;
        public Color secondaryColor;
        public int index;

        public BossData(string name, Color primaryColor, Color secondaryColor, int index)
        {
            this.name = name;
            this.primaryColor = primaryColor;
            this.secondaryColor = secondaryColor;
            this.index = index;
        }
    }

    private static readonly Dictionary<Scenes, BossData> bosses = new()
    {
        { Scenes.scene_level_veggies, new BossData("The Root Pack", Hex("#E1A136"), Hex("#000000"), 0) },
        { Scenes.scene_level_frogs, new BossData("Ribby and Croaks", Hex("#808000"), Hex("#FFFFFF"), 1) },
        { Scenes.scene_level_slime, new BossData("Goopy Le Grande", Hex("#6495ED"), Hex("#000000"), 2) },
        { Scenes.scene_level_flying_blimp, new BossData("Hilda Berg", Hex("#A52A2A"), Hex("#FFCF10"), 3) },
        { Scenes.scene_level_flower, new BossData("Cagney Carnation", Hex("#FFA500"), Hex("#000000"), 4) },
        { Scenes.scene_level_baroness, new BossData("Baroness Von Bon Bon", Hex("#FF69B4"), Hex("#FFFFFF"), 5) },
        { Scenes.scene_level_flying_bird, new BossData("Wally Warbles", Hex("#0000CD"), Hex("#FFFFFF"), 6) },
        { Scenes.scene_level_flying_genie, new BossData("Djimmi The Great", Hex("#CD5C5C"), Hex("#48D1CC"), 7) },
        { Scenes.scene_level_clown, new BossData("Beppi The Clown", Hex("#B22222"), Hex("#FFFFFF"), 8) },
        { Scenes.scene_level_dragon, new BossData("Grim Matchstick", Hex("#9ACD32"), Hex("#000000"), 9) },
        { Scenes.scene_level_bee, new BossData("Rumor Honeybottoms", Hex("#FDBF20"), Hex("#000000"), 10) },
        { Scenes.scene_level_robot, new BossData("Dr. Kahl's Robot", Hex("#A9A9A9"), Hex("#000000"), 11) },
        { Scenes.scene_level_sally_stage_play, new BossData("Sally Stageplay", Hex("#20B2AA"), Hex("#000000"), 12) },
        { Scenes.scene_level_mouse, new BossData("Werner Werman", Hex("#A0522D"), Hex("#FFFFFF"), 13) },
        { Scenes.scene_level_pirate, new BossData("Captain Brineybeard", Hex("#DC143C"), Hex("#FFFFFF"), 14) },
        { Scenes.scene_level_flying_mermaid, new BossData("Cala Maria", Hex("#A0E1C0"), Hex("#433650"), 15) },
        { Scenes.scene_level_train, new BossData("Phantom Express", Hex("#9370DB"), Hex("#FFFFFF"), 16) },
        { Scenes.scene_level_devil, new BossData("The Devil", Hex("#000000"), Hex("#FFCF10"), 17) },
    };

    private class BossButtonInfo
    {
        public UnityEngine.UI.Button button;
        public BossData data;
        public int index;
    }

    [HarmonyPostfix]
    private static void Postfix(LevelSelectList __instance)
    {
        List<BossButtonInfo> buttonList = new List<BossButtonInfo>();

        foreach (UnityEngine.UI.Button b in __instance.contentPanel.GetComponentsInChildren<UnityEngine.UI.Button>(true))
        {
            Scenes scene;
            if (Enum.IsDefined(typeof(Scenes), b.name) && (scene = (Scenes)Enum.Parse(typeof(Scenes), b.name)) == scene)
            {
                if (bosses.TryGetValue(scene, out var bossData))
                {
                    buttonList.Add(new BossButtonInfo { button = b, data = bossData, index = bossData.index });
                }
            }
        }

        buttonList.Sort((a, b) => a.index.CompareTo(b.index));

        foreach (var info in buttonList)
        {
            UnityEngine.UI.Button button = info.button;
            BossData bossData = info.data;

            UnityEngine.UI.Text textComponent = button.GetComponentInChildren<UnityEngine.UI.Text>();
            if (textComponent != null)
            {
                textComponent.text = bossData.name;
                textComponent.color = bossData.secondaryColor;
            }

            UnityEngine.UI.Shadow shadow = textComponent.GetComponent<UnityEngine.UI.Shadow>();
            if (shadow != null)
            {
                shadow.enabled = false;
            }

            UnityEngine.UI.Image[] images = button.GetComponentsInChildren<UnityEngine.UI.Image>(true);
            foreach (var img in images)
            {
                img.color = bossData.primaryColor;
            }

            button.transform.SetSiblingIndex(button.transform.parent.childCount - 1);
        }
    }
}