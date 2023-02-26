using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
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
        private static void PhaseOneBumperDelayForcer(ILContext il) {
            ILCursor ilCursor = new(il);
            while (ilCursor.TryGotoNext(MoveType.After, i => i.OpCode == OpCodes.Ldfld && i.Operand.ToString().Contains("treasureAttacks"))) {
                ilCursor.Index++;
                ilCursor.EmitDelegate<Func<int, int>>(treasure => GetTreasurePattern());
                ilCursor.Index++; // avoid infinite loops
            }
        }

        static int GetTreasurePattern() {
            if (FlyingGeniePhaseOneTreasurePattern.Value != FlyingGeniePhaseOneTreasurePatterns.Random) {
                return (int) FlyingGeniePhaseOneTreasurePattern.Value - 1;
            }
            return UnityEngine.Random.Range(0, 3);
        }

    }
}
