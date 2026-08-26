using System;
using HarmonyLib;
using MonoMod.Cil;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
internal class SlimePatternSelector : PluginComponent {
    [HarmonyPatch(typeof(SlimeLevelSlime), "jump_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void JumpManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNext(MoveType.After,
                i => i.OpCode == OpCodes.Ldfld && i.Operand.ToString().Contains("Jump::numJumps"),
                i => i.OpCode == OpCodes.Callvirt && i.Operand.ToString().Contains("MinMax::RandomInt"))) {
            cursor.EmitDelegate<Func<int, int>>(randomCount =>
                Level.ScoringData.difficulty == Level.Mode.Normal && SlimePhaseOneJumpCountNormal.Value != SlimePhaseOneJumpCountsNormal.Random
                    ? (int)SlimePhaseOneJumpCountNormal.Value
                    : randomCount);
        }

        cursor.Index = 0;
        if (cursor.TryGotoNext(MoveType.After, i =>
                i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random::Range(System.Int32,System.Int32)"))) {
            cursor.EmitDelegate<Func<int, int>>(randomIndex =>
                Level.ScoringData.difficulty == Level.Mode.Normal && SlimePhaseOneJumpPatternNormal.Value != SlimePhaseOneJumpPatternsNormal.Random
                    ? (int)SlimePhaseOneJumpPatternNormal.Value - 1
                    : randomIndex);
        }
    }
}
