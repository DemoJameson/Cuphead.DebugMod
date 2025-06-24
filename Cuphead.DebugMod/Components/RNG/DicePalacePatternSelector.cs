using System;
using HarmonyLib;
using MonoMod.Cil;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;
[HarmonyPatch]
internal class DicePalacePatternSelector : PluginComponent {

    [HarmonyPatch(typeof(DicePalaceMainLevelGameInfo), nameof(DicePalaceMainLevelGameInfo.ChooseHearts))]
    [HarmonyPostfix]
    public static void HeartsManipulator() {
        if (DicePalaceHeartPosition1.Value != DicePalaceHeartPositions1.Random) {
            DicePalaceMainLevelGameInfo.HEART_INDEXES[0] = (int) DicePalaceHeartPosition1.Value - 1;
        }
        if (DicePalaceHeartPosition2.Value != DicePalaceHeartPositions2.Random) {
            DicePalaceMainLevelGameInfo.HEART_INDEXES[1] = (int) DicePalaceHeartPosition2.Value + 3;
        }
        if (DicePalaceHeartPosition3.Value != DicePalaceHeartPositions3.Random) {
            DicePalaceMainLevelGameInfo.HEART_INDEXES[2] = (int) DicePalaceHeartPosition3.Value + 7;
        }
    }

    [HarmonyPatch(typeof(DicePalaceCigarLevelCigar), nameof(DicePalaceCigarLevelCigar.LevelInit))]
    [HarmonyPostfix]
    public static void CigarAttackCountManipulator(ref DicePalaceCigarLevelCigar __instance) {
        if (DicePalaceCigarSpitAttackCountNormal.Value != DicePalaceCigarSpitAttackCountsNormal.Random) {
            __instance.spitAttackCountIndex = (int) DicePalaceCigarSpitAttackCountNormal.Value - 1;
        }
    }


    [HarmonyPatch(typeof(DicePalaceRabbitLevel), nameof(DicePalaceRabbitLevel.Start))]
    [HarmonyPrefix]
    public static void RabbitPatternManipulator(ref DicePalaceRabbitLevel __instance) {
        if (DicePalaceRabbitPattern.Value != DicePalaceRabbitPatterns.Random) {
            __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<DicePalaceRabbitPatterns>((int) DicePalaceRabbitPattern.Value);
        }
    }

    [HarmonyPatch(typeof(DicePalaceRabbitLevelRabbit), nameof(DicePalaceRabbitLevelRabbit.LevelInit))]
    [HarmonyPostfix]
    public static void RabbitParryDirectionManipulator(ref DicePalaceRabbitLevelRabbit __instance) {
        if (DicePalaceRabbitParryDirection.Value != DicePalaceRabbitParryDirections.Random) {
            __instance.isMagicParryTop = DicePalaceRabbitParryDirection.Value == DicePalaceRabbitParryDirections.Top;
        }
    }

    [HarmonyPatch(typeof(DicePalaceRouletteLevel), nameof(DicePalaceRouletteLevel.Start))]
    [HarmonyPrefix]
    public static void RoulettePatternManipulator(ref DicePalaceRouletteLevel __instance) {
        if (DicePalaceRoulettePattern.Value != DicePalaceRoulettePatterns.Random) {
            __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<DicePalaceRoulettePatterns>((int) DicePalaceRoulettePattern.Value);
        }
    }

    [HarmonyPatch(typeof(DicePalaceRouletteLevelRoulette), nameof(DicePalaceRouletteLevelRoulette.twirl_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void RouletteTwirlAmountsListManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Ldfld && i.Operand.ToString().Contains("twirlAmount"))) {
            ilCursor.Index = ilCursor.Index - 1;
            ilCursor.Remove();
            ilCursor.Remove();
            ilCursor.Remove();
            ilCursor.Remove();
            ilCursor.Remove();
            ilCursor.Remove();
            ilCursor.Remove();
            ilCursor.Remove();
            ilCursor.Remove();
            ilCursor.Remove();
            ilCursor.EmitDelegate<Func<string[], string[]>>(twirl => GetTwirlPatternList());
            break;
        }
    }

    static string[] GetTwirlPatternList() {
        if (Level.ScoringData.difficulty != Level.Mode.Normal) {
            return ["5","6","3","6","4","5","6","3","4","7","4","3"];
        }

        string[][] dicePalaceRouletteTwirlAmountsLists = [["4", "5", "3", "6", "4", "5", "4", "3", "4", "6", "4", "3"], ["5", "3", "4", "4", "3", "5", "4", "3", "6", "4", "3", "3"]];
        if (DicePalaceRouletteTwirlAmountNormal.Value == DicePalaceRouletteTwirlAmountsNormal.Random) {
            return dicePalaceRouletteTwirlAmountsLists[UnityEngine.Random.Range(0, 2)];
        } else if (DicePalaceRouletteTwirlAmountNormal.Value == DicePalaceRouletteTwirlAmountsNormal.Four) {
            return dicePalaceRouletteTwirlAmountsLists[0];
        } else {
            return dicePalaceRouletteTwirlAmountsLists[1];
        }
    }
}
