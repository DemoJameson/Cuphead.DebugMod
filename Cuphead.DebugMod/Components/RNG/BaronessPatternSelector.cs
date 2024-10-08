using System.Collections.Generic;
using System.Reflection;
using BepInEx.CupheadDebugMod.Config;
using HarmonyLib;
using MonoMod.Cil;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using OpCodes = Mono.Cecil.Cil.OpCodes;


namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
internal class BaronessPatternSelector : PluginComponent {

    [HarmonyPatch(typeof(BaronessLevel), nameof(BaronessLevel.pickminibosses_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void MinibossSelectionManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("SetUpTimeline"))) {
            ilCursor.Index = ilCursor.Index - 2;
            ilCursor.Emit(OpCodes.Call, typeof(BaronessPatternSelector).GetMethod(nameof(SetMinibossSelectionWrapper), BindingFlags.Static | BindingFlags.Public));
            ilCursor.Index = ilCursor.Index + 3;
        }
    }

    public static void SetMinibossSelectionWrapper() {

        if (Level.ScoringData.difficulty == Level.Mode.Easy) {
            int setting1 = Settings.BaronessMiniboss1Easy.Value == BaronessMinibossesEasy.Jawbreaker ? (int) Settings.BaronessMiniboss1Easy.Value + 1 : (int) Settings.BaronessMiniboss1Easy.Value;
            int setting2 = Settings.BaronessMiniboss2Easy.Value == BaronessMinibossesEasy.Jawbreaker ? (int) Settings.BaronessMiniboss2Easy.Value + 1 : (int) Settings.BaronessMiniboss2Easy.Value;
            int setting3 = Settings.BaronessMiniboss3Easy.Value == BaronessMinibossesEasy.Jawbreaker ? (int) Settings.BaronessMiniboss3Easy.Value + 1 : (int) Settings.BaronessMiniboss3Easy.Value;
            SetMinibossSelection(new List<string> { "1", "2", "3", "5" }, setting1, setting2, setting3, BaronessMinibossesNormal.Random);

        } else if (Level.ScoringData.difficulty == Level.Mode.Normal) {
            SetMinibossSelection(new List<string> { "1", "2", "3", "4", "5" }, (int) Settings.BaronessMiniboss1Normal.Value, (int) Settings.BaronessMiniboss2Normal.Value, (int) Settings.BaronessMiniboss3Normal.Value, BaronessMinibossesNormal.Random);
        } else // Hard
          {
            SetMinibossSelection(new List<string> { "1", "2", "3", "4", "5" }, (int) Settings.BaronessMiniboss1Hard.Value, (int) Settings.BaronessMiniboss2Hard.Value, (int) Settings.BaronessMiniboss3Hard.Value, BaronessMinibossesHard.Random);
        }
    }

    public static void SetMinibossSelection(List<string> minibossPool, int setting1, int setting2, int setting3, object random) {

        List<string> pickedMinibosses = new List<string> {
            "0",
            "0",
            "0"
        };

        if (setting1 != (int) random) {
            string minibossIndex = setting1.ToString();
            pickedMinibosses[0] = minibossIndex;
            minibossPool.Remove(minibossIndex);
        }

        if (setting2 != (int) random) {
            string minibossIndex = setting2.ToString();
            pickedMinibosses[1] = minibossIndex;
            minibossPool.Remove(minibossIndex);
        }

        if (setting3 != (int) random) {
            string minibossIndex = setting3.ToString();
            pickedMinibosses[2] = minibossIndex;
            minibossPool.Remove(minibossIndex);
        }

        for (int i = 0; i < pickedMinibosses.Count; i++) {
            if (pickedMinibosses[i] == "0") {
                int randIndex = UnityEngine.Random.Range(0, minibossPool.Count);
                pickedMinibosses[i] = minibossPool[randIndex];
                minibossPool.Remove(pickedMinibosses[i]);
            }
        }

        BaronessLevel.PICKED_BOSSES = pickedMinibosses;
    }
}
