using HarmonyLib;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
internal class MousePatternSelector : PluginComponent {
    [HarmonyPatch(typeof(MouseLevel), nameof(MouseLevel.Start))]
    [HarmonyPostfix]
    public static void PhaseOnePatternManipulator(ref MouseLevel __instance) {

        if (Level.ScoringData.difficulty == Level.Mode.Easy) {
            if (MousePhaseOnePatternEasy.Value != MousePhaseOnePatternsEasy.Random) {
                __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<MousePhaseOnePatternsEasy>((int) MousePhaseOnePatternEasy.Value);
            }
        } else if (Level.ScoringData.difficulty == Level.Mode.Normal) {
            if (MousePhaseOnePatternNormal.Value != MousePhaseOnePatternsNormal.Random) {
                __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<MousePhaseOnePatternsNormal>((int) MousePhaseOnePatternNormal.Value);
            }
        } else if (Level.ScoringData.difficulty == Level.Mode.Hard) {
            if (MousePhaseOnePatternHard.Value != MousePhaseOnePatternsHard.Random) {
                __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<MousePhaseOnePatternsHard>((int) MousePhaseOnePatternHard.Value);
            }
        }

    }
}
