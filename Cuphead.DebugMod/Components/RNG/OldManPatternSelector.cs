#if v1_3
using System;
using BepInEx.CupheadDebugMod.Config;
using HarmonyLib;
using MonoMod.Cil;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
internal class OldManPatternSelector : PluginComponent {
    [HarmonyPatch(typeof(OldManLevelPlatformManager), "handle_remove_platforms_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void PlatformRemoveOrderManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNext(MoveType.After, i =>
                i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random::Range(System.Int32,System.Int32)"))) {
            cursor.EmitDelegate<Func<int, int>>(randomIndex =>
                Level.ScoringData.difficulty == Level.Mode.Normal &&
                OldManPhaseOnePlatformRemoveOrderNormal.Value != OldManPhaseOnePlatformRemoveOrdersNormal.Random
                    ? (int)OldManPhaseOnePlatformRemoveOrderNormal.Value - 1
                    : randomIndex);
        }
    }

    [HarmonyPatch(typeof(OldManLevelSockPuppetHandler), "bounce_ball_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void PuppetPositionManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (TryGotoAfterPuppetPatternCtor(cursor, "Hands::leftHandPosString")) {
            cursor.EmitDelegate<Func<PatternString, PatternString>>(SetLeftPuppetPosition);
        }

        cursor.Index = 0;
        if (TryGotoAfterPuppetPatternCtor(cursor, "Hands::rightHandPosString")) {
            cursor.EmitDelegate<Func<PatternString, PatternString>>(SetRightPuppetPosition);
        }
    }

    private static bool TryGotoAfterPuppetPatternCtor(ILCursor cursor, string handPositionField) {
        return cursor.TryGotoNext(MoveType.After,
                   i => i.OpCode == OpCodes.Ldfld && i.Operand.ToString().Contains(handPositionField)) &&
               cursor.TryGotoNext(MoveType.After,
                   i => i.OpCode == OpCodes.Newobj && i.Operand.ToString().Contains("PatternString::.ctor"));
    }

    private static PatternString SetLeftPuppetPosition(PatternString positions) {
        return SetPuppetPosition(positions, GetLeftPuppetPattern());
    }

    private static PatternString SetRightPuppetPosition(PatternString positions) {
        return SetPuppetPosition(positions, GetRightPuppetPattern());
    }

    private static PatternString SetPuppetPosition(PatternString positions, int selectedIndex) {
        if (selectedIndex != 0) {
            positions.SetSubStringIndex(selectedIndex - 2);
        }
        return positions;
    }

    private static int GetLeftPuppetPattern() {
        return Level.ScoringData.difficulty switch {
            Level.Mode.Easy => (int)OldManPhaseTwoLeftPuppetPatternEasy.Value,
            Level.Mode.Hard => (int)OldManPhaseTwoRightNormalLeftHardPuppetPattern.Value,
            _ => (int)OldManPhaseTwoLeftNormalRightHardPuppetPattern.Value
        };
    }

    private static int GetRightPuppetPattern() {
        return Level.ScoringData.difficulty switch {
            Level.Mode.Easy => (int)OldManPhaseTwoRightPuppetPatternEasy.Value,
            Level.Mode.Hard => (int)OldManPhaseTwoLeftNormalRightHardPuppetPattern.Value,
            _ => (int)OldManPhaseTwoRightNormalLeftHardPuppetPattern.Value
        };
    }
}
#endif
