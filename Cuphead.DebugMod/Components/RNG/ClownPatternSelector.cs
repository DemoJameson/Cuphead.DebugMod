using System;
using BepInEx.CupheadDebugMod.Config;
using HarmonyLib;
using MonoMod.Cil;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
internal class ClownPatternSelector : PluginComponent {

    [HarmonyPatch(typeof(ClownLevelClown), nameof(ClownLevelClown.bumper_car_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void PhaseOneBumperDelayManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("timerIndex"))) {
            ilCursor.EmitDelegate<Func<int, int>>(dash => GetDashPattern());
            ilCursor.Index++; // avoid infinite loops
        }
    }

    static int GetDashPattern() {
        // I am checking for Settings.ClownDashDelay.Value dynamically during this function call.
        // This is because a HarmonyTranspiler only gets called once when the script is loaded upon game bootup...
        // ...so the function call itself that gets injected into the IL code needs to check the value as it is set by Settings.ClownDashDelay.Value
        if (Level.ScoringData.difficulty == Level.Mode.Easy) {
            return Settings.ClownDashDelayEasy.Value == ClownDashDelaysEasy.Random
                ? UnityEngine.Random.Range(0, 9)
                : (int) Settings.ClownDashDelayEasy.Value - 1;
        } else if (Level.ScoringData.difficulty == Level.Mode.Normal) {
            return Settings.ClownDashDelayNormal.Value == ClownDashDelaysNormal.Random
                ? UnityEngine.Random.Range(0, 10)
                : (int) Settings.ClownDashDelayNormal.Value - 1;
        } else // hard difficulty
          {
            return Settings.ClownDashDelayHard.Value == ClownDashDelaysHard.Random
                ? UnityEngine.Random.Range(0, 11)
                : (int) Settings.ClownDashDelayHard.Value - 1;
        }

    }

}

