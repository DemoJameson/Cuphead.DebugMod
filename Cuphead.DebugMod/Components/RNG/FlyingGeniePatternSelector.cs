using System;
using HarmonyLib;
using MonoMod.Cil;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG {

    [HarmonyPatch]
    internal class FlyingGeniePatternSelector {

        [HarmonyPatch(typeof(FlyingGenieLevelGenie), nameof(FlyingGenieLevelGenie.StartTreasure))]
        [HarmonyILManipulator]
        private static void PhaseOneTreasureForcer(ILContext il) {
            ILCursor ilCursor = new(il);
            while (ilCursor.TryGotoNext(MoveType.After, i => i.OpCode == OpCodes.Ldfld && i.Operand.ToString().Contains("treasureAttacks"))) {
                ilCursor.Index++;
                ilCursor.EmitDelegate<Func<int, int>>(treasure => GetTreasurePattern());
                ilCursor.Index++; // avoid infinite loops
            }
        }

        [HarmonyPatch(typeof(FlyingGenieLevelGenie), nameof(FlyingGenieLevelGenie.LevelInit))]
        [HarmonyPostfix]
        private static void PhaseOneParryPatternManipulator(ref FlyingGenieLevelGenie __instance) {
            if (Level.ScoringData.difficulty == Level.Mode.Easy) {
                if (FlyingGeniePhaseOneSwordTypeEasyNormal.Value != FlyingGeniePhaseOneSwordTypesEasyNormal.Random) {
                    __instance.swordPinkIndex = (int) FlyingGeniePhaseOneSwordTypeEasyNormal.Value - 1;
                }
                if (FlyingGeniePhaseOneGemsTypeEasy.Value != FlyingGeniePhaseOneGemsTypesEasy.Random) {
                    __instance.gemPinkIndex = (int) FlyingGeniePhaseOneGemsTypeEasy.Value - 1;
                }
            }
            else if (Level.ScoringData.difficulty == Level.Mode.Normal) {
                if (FlyingGeniePhaseOneSwordTypeEasyNormal.Value != FlyingGeniePhaseOneSwordTypesEasyNormal.Random) {
                    __instance.swordPinkIndex = (int) FlyingGeniePhaseOneSwordTypeEasyNormal.Value - 1;
                }
                if (FlyingGeniePhaseOneGemsTypeNormalHard.Value != FlyingGeniePhaseOneGemsTypesNormalHard.Random) {
                    __instance.gemPinkIndex = (int) FlyingGeniePhaseOneGemsTypeNormalHard.Value - 1;
                }
            } else if (Level.ScoringData.difficulty == Level.Mode.Hard) {
                if (FlyingGeniePhaseOneSwordTypeHard.Value != FlyingGeniePhaseOneSwordTypesHard.Random) {
                    __instance.swordPinkIndex = (int) FlyingGeniePhaseOneSwordTypeHard.Value - 1;
                }
                if (FlyingGeniePhaseOneGemsTypeNormalHard.Value != FlyingGeniePhaseOneGemsTypesNormalHard.Random) {
                    __instance.gemPinkIndex = (int) FlyingGeniePhaseOneGemsTypeNormalHard.Value - 1;
                }
            }

        }

        [HarmonyPatch(typeof(FlyingGenieLevelGenie), nameof(FlyingGenieLevelGenie.obelisk_cr), MethodType.Enumerator)]
        [HarmonyILManipulator]
        private static void PhaseTwoObeliskManipulator(ILContext il) {
            ILCursor ilCursor = new(il);
            while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("mainObeliskIndex"))) {
                ilCursor.EmitDelegate<Func<int, int>>(mainObeliskIndex => GetMainObeliskIndex());
                break;
            }
            ilCursor.Index = 0;
            while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("obeliskIndex"))) {
                ilCursor.EmitDelegate<Func<int, int>>(mainObeliskIndex => GetObeliskIndex());
                break;
            }
        }

        static int GetObeliskIndex() {
            if (FlyingGeniePhaseTwoObeliskPattern.Value == FlyingGeniePhaseTwoObeliskPatterns.Random) {
                return UnityEngine.Random.Range(0, 9);
            }
            return ((int) FlyingGeniePhaseTwoObeliskPattern.Value - 1) % 9;
        }

        static int GetMainObeliskIndex() {
            if (FlyingGeniePhaseTwoObeliskPattern.Value == FlyingGeniePhaseTwoObeliskPatterns.Random) {
                return UnityEngine.Random.Range(0, 27);
            }
            return ((int) FlyingGeniePhaseTwoObeliskPattern.Value - 1) / 9;
        }

        static int GetTreasurePattern() {
            if (FlyingGeniePhaseOneTreasurePattern.Value != FlyingGeniePhaseOneTreasurePatterns.Random) {
                return (int) FlyingGeniePhaseOneTreasurePattern.Value - 1;
            }
            return UnityEngine.Random.Range(0, 3);
        }

    }
}
