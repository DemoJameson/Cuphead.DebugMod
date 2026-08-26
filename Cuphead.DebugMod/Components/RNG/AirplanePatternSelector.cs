#if v1_3
using System;
using HarmonyLib;
using MonoMod.Cil;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
internal class AirplanePatternSelector : PluginComponent {
    [HarmonyPatch(typeof(AirplaneLevelBulldogPlane), "mainattack_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void MainAttackManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNext(MoveType.After,
                i => i.OpCode == OpCodes.Ldfld && i.Operand.ToString().Contains("Main::attackDelayRange"),
                i => i.OpCode == OpCodes.Callvirt && i.Operand.ToString().Contains("MinMax::RandomFloat"))) {
            cursor.EmitDelegate<Func<float, float>>(randomDelay =>
                AirplanePhaseOneAttackDelay.Value != -1f ? AirplanePhaseOneAttackDelay.Value : randomDelay);
        }

        cursor.Index = 0;
        if (cursor.TryGotoNext(MoveType.After, i =>
                i.OpCode == OpCodes.Newobj && i.Operand.ToString().Contains("PatternString::.ctor"))) {
            cursor.EmitDelegate<Func<PatternString, PatternString>>(attackType => {
                if (AirplanePhaseOnePattern.Value != AirplanePhaseOnePatterns.Random) {
                    attackType.SetSubStringIndex((int)AirplanePhaseOnePattern.Value - 2);
                }
                return attackType;
            });
        }
    }

    [HarmonyPatch(typeof(AirplaneLevel), "handle_terriers_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void TerrierRotationManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNext(MoveType.After,
                i => i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("Rand::Bool"))) {
            cursor.EmitDelegate<Func<bool, bool>>(isClockwise => AirplanePhaseTwoTerrierRotationDirection.Value switch {
                AirplanePhaseTwoTerrierRotationDirections.Clockwise => true,
                AirplanePhaseTwoTerrierRotationDirections.Counterclockwise => false,
                _ => isClockwise
            });
        }
    }

    [HarmonyPatch(typeof(AirplaneLevelBulldogPlane), nameof(AirplaneLevelBulldogPlane.LevelInit))]
    [HarmonyPostfix]
    private static void ParachuteSideManipulator(ref PatternString ___sideString) {
        if (AirplanePhaseOneSide.Value != AirplanePhaseOneSides.Random) {
            ___sideString.SetSubStringIndex((int)AirplanePhaseOneSide.Value - 2);
        }
    }

    [HarmonyPatch(typeof(AirplaneLevelBulldogParachute), "Awake")]
    [HarmonyPostfix]
    private static void ParachutePinkManipulator(ref PatternString ___pinkString) {
        if (AirplanePhaseOneParryPattern.Value != AirplanePhaseOneParryPatterns.Random) {
            ___pinkString.SetSubStringIndex((int)AirplanePhaseOneParryPattern.Value - 2);
        }
    }
}
#endif
