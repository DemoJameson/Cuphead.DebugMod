using System;
using BepInEx.CupheadDebugMod.Config;
using HarmonyLib;
using MonoMod.Cil;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using OpCodes = Mono.Cecil.Cil.OpCodes;


namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
public class RobotPatternSelector : PluginComponent {
    [HarmonyPatch(typeof(RobotLevelHelihead), nameof(RobotLevelHelihead.InitHeliHead))]
    [HarmonyILManipulator]
    public static void PhaseFinalGemManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("attackTypeIndex"))) {
            ilCursor.EmitDelegate<Func<int, int>>(gemColor =>
                Settings.RobotPhaseFinalGemColor.Value == RobotPhaseFinalGemColors.Random
                ? UnityEngine.Random.Range(0, 2)
                : (int) Settings.RobotPhaseFinalGemColor.Value - 1
            );
            ilCursor.Index++;
        }
    }

}
