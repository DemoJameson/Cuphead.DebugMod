using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx.CupheadDebugMod.Config;
using HarmonyLib;
using UnityEngine;
using MonoMod.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using System.Collections;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
internal class ClownPatternSelector : PluginComponent {

    [HarmonyPatch(typeof(ClownLevelClown), nameof(ClownLevelClown.bumper_car_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void PhaseOneBumperDelayForcer(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("timerIndex"))) {
            // I am checking for Settings.ClownDashDelay.Value dynamically during this function call.
            // This is because a HarmonyTranspiler only gets called once when the script is loaded upon game bootup...
            // ...so the function call itself that gets injected into the IL code needs to check the value as it is set by Settings.ClownDashDelay.Value

            ilCursor.EmitDelegate<Func<int, int>>(mode =>
            Settings.ClownDashDelay.Value == ClownDashDelays.Random ?
            // The game selects within the range of 0 to dashDelayPattern.Length here. The latter is always 10 on Regular mode.
            UnityEngine.Random.Range(0, 10)
            :
            (int) Settings.ClownDashDelay.Value - 1
            );
            ilCursor.Index++; // avoid infinite loops


        }
    }
}