#if v1_3
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.CupheadDebugMod.Config;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using OpCodes = Mono.Cecil.Cil.OpCodes;
using HarmonyLib;
using MonoMod.Cil;

namespace BepInEx.CupheadDebugMod.Components.RNG {

    [HarmonyPatch]
    internal class SaltbakerPatternSelector : PluginComponent {

        [HarmonyPatch(typeof(SaltbakerLevelSaltbaker), nameof(SaltbakerLevelSaltbaker.intro_cr))]
        [HarmonyPostfix]
        public static void PhaseOneTableManipulator(ref SaltbakerLevelSaltbaker __instance) {
            if (SaltbakerPhaseOneTablePattern.Value != SaltbakerPhaseOneTablePatterns.Random) {
                if ((Utility.GetUserPattern<SaltbakerPhaseOneTablePatterns>((int) SaltbakerPhaseOneTablePattern.Value) == 0)) {
                    __instance.animator.SetBool("IntroCubes", true);
                }
                else {
                    __instance.animator.SetBool("IntroCubes", false);
                }
            }
        }

        [HarmonyPatch(typeof(SaltbakerLevel), nameof(SaltbakerLevel.Start))]
        [HarmonyPostfix]
        public static void PhaseOnePatternManipulator(ref SaltbakerLevel __instance) {
            if (SaltbakerPhaseOnePattern.Value != SaltbakerPhaseOnePatterns.Random) {
                __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<SaltbakerPhaseOnePatterns>((int) SaltbakerPhaseOnePattern.Value);
            }
        }

        [HarmonyPatch(typeof(SaltbakerLevelSaltbaker), nameof(SaltbakerLevelSaltbaker.sugarcubes_cr))]
        [HarmonyPostfix]
        public static void PhaseOneSugarParryManipulator(ref SaltbakerLevelSaltbaker __instance) {
            if (SaltbakerParryStringPattern.Value != SaltbakerParryStringPatterns.Random) {
                __instance.sugarcubesParryString.subIndex = Utility.GetUserPattern<SaltbakerParryStringPatterns>((int) SaltbakerParryStringPattern.Value);
            }
        }

        [HarmonyPatch(typeof(SaltbakerLevel), nameof(SaltbakerLevel.SpawnCutters))]
        [HarmonyILManipulator]
        public static void PhaseThreeCutterManipulator(ILContext il) {
            ILCursor ilCursor = new(il);
            while (ilCursor.TryGotoNext(MoveType.After, i => i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("Bool"))) {
                //ilCursor.Index++;
                ilCursor.EmitDelegate<Func<bool, bool>>(mode =>
                    Settings.SaltbakerPhaseThreeSawPattern.Value == SaltbakerPhaseThreeSawPatterns.Random ?
                    Rand.Bool()
                    :
                    SaltbakerPhaseThreeSawPattern.Value == SaltbakerPhaseThreeSawPatterns.Left
                );
                ilCursor.Index++; // avoid infinite loops
            }
        }

    }
}
#endif
