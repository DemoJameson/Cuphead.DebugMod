#if v1_3
using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.CupheadDebugMod.Config;
using HarmonyLib;
using MonoMod.Cil;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using OpCodes = Mono.Cecil.Cil.OpCodes;


namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
internal class RumRunnersPatternSelector : PluginComponent {
    private static PatternString spiderPositionPattern;
    private static PatternString spiderActionPositionPattern;

    [HarmonyPatch(typeof(RumRunnersLevelSpider), "run_cr", MethodType.Enumerator)]
    [HarmonyPrefix]
    private static void ResetSpiderPatternStarts() {
        spiderPositionPattern = null;
        spiderActionPositionPattern = null;
    }

    [HarmonyPatch(typeof(RumRunnersLevelSpider), "run_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void SpiderActionManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNext(MoveType.After,
                i => i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("Rand::Bool"))) {
            cursor.EmitDelegate<Func<bool, bool>>(randomAction => Settings.RumRunnersPhaseOneSpiderInitialAction.Value switch {
                RumRunnersPhaseOneSpiderInitialActions.None => true,
                RumRunnersPhaseOneSpiderInitialActions.Mine => false,
                _ => randomAction
            });
        }

        if (TryGotoAfterPatternStringCtor(cursor, "<spiderPositionString>")) {
            cursor.EmitDelegate<Func<PatternString, PatternString>>(positions => {
                int positionIndex = Level.CurrentMode == Level.Mode.Easy
                    ? (int)Settings.RumRunnersPhaseOneSpiderPositionEasy.Value - 2
                    : (int)Settings.RumRunnersPhaseOneSpiderPositionNormalHard.Value - 2;
                if (positionIndex >= -1) {
                    spiderPositionPattern = positions;
                }
                return positions;
            });
        }

        if (TryGotoAfterPatternStringCtor(cursor, "<spiderActionString>")) {
            cursor.EmitDelegate<Func<PatternString, PatternString>>(actions => {
                int actionIndex = Level.CurrentMode == Level.Mode.Hard
                    ? (int)Settings.RumRunnersPhaseOneSpiderActionHard.Value - 2
                    : (int)Settings.RumRunnersPhaseOneSpiderActionEasyNormal.Value - 2;
                if (actionIndex >= -1) {
                    actions.SetSubStringIndex(actionIndex);
                }
                return actions;
            });
        }

        if (TryGotoAfterPatternStringCtor(cursor, "<spiderActionPositionString>")) {
            cursor.EmitDelegate<Func<PatternString, PatternString>>(actionPositions => {
                if (Settings.RumRunnersPhaseOneSpiderActionPosition.Value != RumRunnersPhaseOneSpiderActionPositions.Random) {
                    spiderActionPositionPattern = actionPositions;
                }
                return actionPositions;
            });
        }
    }

    [HarmonyPatch(typeof(PatternString), nameof(PatternString.PopInt))]
    [HarmonyPostfix]
    private static void SetSpiderPositionAfterInitialPop(PatternString __instance) {
        if (!ReferenceEquals(__instance, spiderPositionPattern)) return;

        int positionIndex = Level.CurrentMode == Level.Mode.Easy
            ? (int)Settings.RumRunnersPhaseOneSpiderPositionEasy.Value - 2
            : (int)Settings.RumRunnersPhaseOneSpiderPositionNormalHard.Value - 2;
        __instance.SetSubStringIndex(positionIndex);
        spiderPositionPattern = null;
    }

    [HarmonyPatch(typeof(PatternString), nameof(PatternString.PopFloat))]
    [HarmonyPostfix]
    private static void SetSpiderActionPositionAfterInitialPop(PatternString __instance) {
        if (!ReferenceEquals(__instance, spiderActionPositionPattern)) return;

        __instance.SetSubStringIndex((int)Settings.RumRunnersPhaseOneSpiderActionPosition.Value - 2);
        spiderActionPositionPattern = null;
    }

    private static bool TryGotoAfterPatternStringCtor(ILCursor cursor, string targetField) {
        cursor.Index = 0;
        while (cursor.TryGotoNext(MoveType.After,
                   i => i.OpCode == OpCodes.Newobj && i.Operand.ToString().Contains("PatternString::.ctor"))) {
            if (cursor.Next.OpCode == OpCodes.Stfld && cursor.Next.Operand.ToString().Contains(targetField)) {
                return true;
            }
        }
        return false;
    }

    [HarmonyPatch(typeof(RumRunnersLevelSpider), nameof(RumRunnersLevelSpider.LevelInit))]
    [HarmonyILManipulator]
    private static void MinePlacementManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(MoveType.After,
                i => i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random::Range"))) {
            return;
        }

        cursor.EmitDelegate<Func<int, int>>(randomIndex => {
        int selectedIndex = Level.CurrentMode switch {
            Level.Mode.Easy => (int)Settings.RumRunnersPhaseOneMinePlacementEasy.Value - 1,
            Level.Mode.Hard => (int)Settings.RumRunnersPhaseOneMinePlacementHard.Value - 1,
            _ => (int)Settings.RumRunnersPhaseOneMinePlacementNormal.Value - 1
        };

            return selectedIndex >= 0 ? selectedIndex : randomIndex;
        });
    }

    [HarmonyPatch(typeof(RumRunnersLevelAnteater), nameof(RumRunnersLevelAnteater.LevelInit))]
    [HarmonyPostfix]
    private static void SnoutPositionManipulator(ref RumRunnersLevelAnteater __instance) {
        if (Settings.RumRunnersPhaseThreeSnoutPosition.Value != RumRunnersPhaseThreeSnoutPositions.Random) {
            __instance.snoutPositionPattern.subIndex = Utility.GetUserPattern<RumRunnersPhaseThreeSnoutPositions>((int) Settings.RumRunnersPhaseThreeSnoutPosition.Value);
        }

        int actionIndex = Level.CurrentMode switch {
            Level.Mode.Hard => (int)Settings.RumRunnersPhaseThreeSnoutActionHard.Value - 1,
            Level.Mode.Normal => (int)Settings.RumRunnersPhaseThreeSnoutActionNormal.Value - 1,
            _ => -1
        };
        if (actionIndex >= 0) {
            SetPatternMainIndex(__instance.snoutAttackPattern, actionIndex);
        }
    }

    private static void SetPatternMainIndex(PatternString pattern, int index) {
        pattern.SetSubStringIndex(pattern.SubStringLength() - 1);
        pattern.SetMainStringIndex(index - 1);
        pattern.IncrementString();
    }
}
#endif
