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
using System;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
public class FlyingBirdPatternSelector : PluginComponent {

    // Sets the leading attack in the sequence.
    [HarmonyPatch(typeof(FlyingBirdLevel), nameof(FlyingBirdLevel.OnStateChanged))]
    [HarmonyPrefix]
    public static void PhaseFinalPatternManipulator(ref FlyingBirdLevel __instance) {
        if (Level.ScoringData.difficulty != Level.Mode.Easy && FlyingBirdPhaseFinalPattern.Value != FlyingBirdPhaseFinalPatterns.Random) {
            // similarly to Cala Maria, Wally's final phase patterns end up being the same, but its pattern list is swapped around
            // since having a different setting for this would be a little redundant for the user, i am just recalculating the pattern index behind the scenes here
            int userPattern = (int) FlyingBirdPhaseFinalPattern.Value;
            if (Level.ScoringData.difficulty == Level.Mode.Hard) {
                if (FlyingBirdPhaseFinalPattern.Value == FlyingBirdPhaseFinalPatterns.Garbage)
                    userPattern = userPattern + 1;
                else
                    userPattern = userPattern - 1;
            }
            __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<FlyingBirdPhaseFinalPatterns>(userPattern);
        }
    }

    [HarmonyPatch(typeof(FlyingBirdLevelBird), nameof(FlyingBirdLevelBird.stretcherMove_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseFinalDirectionManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("<movingRight>"))) {
            // I am checking for Settings.FlyingBirdPhaseFinalDirection.Value dynamically during this function call.
            // This is because a HarmonyTranspiler only gets called once when the script is loaded upon game bootup...
            // ...so the function call itself that gets injected into the IL code needs to check the value as it is set by Settings.FlyingBirdPhaseFinalDirection.Value
            ilCursor.EmitDelegate<Func<bool, bool>>(movingRight =>
                FlyingBirdPhaseFinalDirection.Value == FlyingBirdPhaseFinalDirections.Random ?
                Rand.Bool()
                :
                FlyingBirdPhaseFinalDirection.Value == FlyingBirdPhaseFinalDirections.Right
            );
            ilCursor.Index++; // avoid infinite loops
        }
    }

}