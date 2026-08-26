using System;
using HarmonyLib;
using MonoMod.Cil;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
internal class MousePatternSelector : PluginComponent {
    [HarmonyPatch(typeof(MouseLevel), nameof(MouseLevel.Start))]
    [HarmonyPostfix]
    public static void PhaseOnePatternManipulator(ref MouseLevel __instance) {

        if (Level.ScoringData.difficulty == Level.Mode.Easy) {
            if (MousePhaseOnePatternEasy.Value != MousePhaseOnePatternsEasy.Random) {
                __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<MousePhaseOnePatternsEasy>((int) MousePhaseOnePatternEasy.Value);
            }
        } else if (Level.ScoringData.difficulty == Level.Mode.Normal) {
            if (MousePhaseOnePatternNormal.Value != MousePhaseOnePatternsNormal.Random) {
                __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<MousePhaseOnePatternsNormal>((int) MousePhaseOnePatternNormal.Value);
            }
        } else if (Level.ScoringData.difficulty == Level.Mode.Hard) {
            if (MousePhaseOnePatternHard.Value != MousePhaseOnePatternsHard.Random) {
                __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<MousePhaseOnePatternsHard>((int) MousePhaseOnePatternHard.Value);
            }
        }

    }

    [HarmonyPatch(typeof(MouseLevelCanMouse), "move_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void CanMoveMaxXPositionManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNext(MoveType.After,
                i => i.OpCode == OpCodes.Ldfld && i.Operand.ToString().Contains("CanMove::maxXPositionRange"),
                i => i.OpCode == OpCodes.Callvirt && i.Operand.ToString().Contains("MinMax::RandomFloat"))) {
            cursor.EmitDelegate<Func<float, float>>(randomPosition =>
                MouseCanMoveMaxXPosition.Value != -1f ? MouseCanMoveMaxXPosition.Value : randomPosition);
        }
    }

    [HarmonyPatch(typeof(MouseLevelBrokenCanMouse), "move_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void BrokenCanMoveMaxXPositionManipulator(ILContext il) {
        ILCursor cursor = new(il);
        while (cursor.TryGotoNext(MoveType.After,
                   i => i.OpCode == OpCodes.Ldfld && i.Operand.ToString().Contains("BrokenCanMove::maxXPositionRange"),
                   i => i.OpCode == OpCodes.Callvirt && i.Operand.ToString().Contains("MinMax::RandomFloat"))) {
            cursor.EmitDelegate<Func<float, float>>(randomPosition =>
                MouseBrokenCanMoveMaxXPosition.Value != -1f ? MouseBrokenCanMoveMaxXPosition.Value : randomPosition);
        }
    }

    [HarmonyPatch(typeof(MouseLevelCanMouse), "cherryBomb_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void CanCherryBombPatternManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNext(MoveType.After,
                i => i.OpCode == OpCodes.Ldfld && i.Operand.ToString().Contains("CanCherryBomb::patterns")) &&
            cursor.TryGotoNext(MoveType.After,
                i => i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random::Range(System.Int32,System.Int32)"))) {
            cursor.EmitDelegate<Func<int, int>>(randomIndex => {
                if (Level.ScoringData.difficulty == Level.Mode.Normal &&
                    MouseCherryBombPatternNormal.Value != MouseCherryBombPatternsNormal.Random) {
                    return (int)MouseCherryBombPatternNormal.Value - 1;
                }

                if (Level.ScoringData.difficulty == Level.Mode.Hard &&
                    MouseCherryBombPatternHard.Value != MouseCherryBombPatternsHard.Random) {
                    return (int)MouseCherryBombPatternHard.Value - 1;
                }

                return randomIndex;
            });
        }
    }

    [HarmonyPatch(typeof(MouseLevelCanMouse), "FireCatapult")]
    [HarmonyILManipulator]
    private static void CanCatapultPatternManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNext(MoveType.After,
                i => i.OpCode == OpCodes.Ldfld && i.Operand.ToString().Contains("CanCatapult::patterns"))) {
            cursor.EmitDelegate<Func<string[], string[]>>(patterns => {
                int index = Level.ScoringData.difficulty switch {
                    Level.Mode.Easy when MouseCatapultPatternEasy.Value != MouseCatapultPatternsEasy.Random =>
                        (int)MouseCatapultPatternEasy.Value - 1,
                    Level.Mode.Normal when MouseCatapultPatternNormal.Value != MouseCatapultPatternsNormal.Random =>
                        (int)MouseCatapultPatternNormal.Value - 1,
                    Level.Mode.Hard when MouseCatapultPatternHard.Value != MouseCatapultPatternsHard.Random =>
                        (int)MouseCatapultPatternHard.Value - 1,
                    _ => -1
                };
                return index >= 0 ? new[] { patterns[index] } : patterns;
            });
        }
    }
}
