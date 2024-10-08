using HarmonyLib;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
internal class FlyingMermaidPatternSelector : PluginComponent {

    // Sets the leading attack. Subsequent attacks work as normal.
    [HarmonyPatch(typeof(FlyingMermaidLevel), nameof(FlyingMermaidLevel.Start))]
    [HarmonyPrefix]

    // this applies to all difficulties
    public static void PhaseOneFirstPatternManipulator(ref FlyingMermaidLevel __instance) {
        if (Level.ScoringData.difficulty == Level.Mode.Easy) {
            if (FlyingMermaidPhaseOneFirstPatternEasy.Value != FlyingMermaidPhaseOneFirstPatternsEasy.Random) {
                __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<FlyingMermaidPhaseOneFirstPatternsEasy>((int) FlyingMermaidPhaseOneFirstPatternEasy.Value);
            }
        } else {
            if (FlyingMermaidPhaseOnePatternNormalHard.Value != FlyingMermaidPhaseOnePatternsNormalHard.Random) {
                // little trick i'm doing behind the scenes here
                // normal and hard mode don't QUITE have the same patterns list. their lists are ordered the same in the end, but the starting point is different
                // to avoid creating a different setting for them (which would be redundant), i am accepting the same one as input for both normal and hard...
                // ...and just handling some logic here to make it work
                int userPattern = (int) FlyingMermaidPhaseOnePatternNormalHard.Value;
                if (Level.ScoringData.difficulty == Level.Mode.Hard) {
                    if (FlyingMermaidPhaseOnePatternNormalHard.Value < FlyingMermaidPhaseOnePatternsNormalHard.Fish)
                        userPattern = userPattern + 2;
                    else
                        userPattern = userPattern - 2;
                }
                __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<FlyingMermaidPhaseOnePatternsNormalHard>(userPattern);
            }
        }

    }

    // this applies specifically for easy mode's second generic phase
    [HarmonyPatch(typeof(FlyingMermaidLevel), nameof(FlyingMermaidLevel.OnStateChanged))]
    [HarmonyPrefix]
    public static void PhaseOneSecondPatternManipulatorEasy(ref FlyingMermaidLevel __instance) {
        if (Level.ScoringData.difficulty == Level.Mode.Easy) {
            if (IsWithinPhase(0.75f, 0.35f, __instance)) {
                if (FlyingMermaidPhaseOneSecondPatternEasy.Value != FlyingMermaidPhaseOneSecondPatternsEasy.Random) {
                    __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<FlyingMermaidPhaseOneSecondPatternsEasy>((int) FlyingMermaidPhaseOneSecondPatternEasy.Value);
                }
            }
        }
    }

    [HarmonyPatch(typeof(FlyingMermaidLevelMermaid), nameof(FlyingMermaidLevelMermaid.LevelInit))]
    [HarmonyPostfix]
    public static void PhaseOneFishManipulator(ref FlyingMermaidLevelMermaid __instance) {
        if (FlyingMermaidPhaseOneFishPattern.Value != FlyingMermaidPhaseOneFishPatterns.Random) {
            FlyingMermaidLevelMermaid.FishPossibility[] array = new FlyingMermaidLevelMermaid.FishPossibility[2];
            array[0] = FlyingMermaidLevelMermaid.FishPossibility.Homer;
            __instance.fishPattern = array;
            __instance.fishIndex = Utility.GetUserPattern<FlyingMermaidPhaseOneFishPatterns>((int) FlyingMermaidPhaseOneFishPattern.Value);
        }

    }

    [HarmonyPatch(typeof(FlyingMermaidLevelMermaid), nameof(FlyingMermaidLevelMermaid.LevelInit))]
    [HarmonyPostfix]
    public static void PhaseOneSummonManipulator(ref FlyingMermaidLevelMermaid __instance) {
        if (FlyingMermaidPhaseOneSummonPattern.Value != FlyingMermaidPhaseOneSummonPatterns.Random) {
            FlyingMermaidLevelMermaid.SummonPossibility[] array = new FlyingMermaidLevelMermaid.SummonPossibility[] {
                FlyingMermaidLevelMermaid.SummonPossibility.Seahorse,
                FlyingMermaidLevelMermaid.SummonPossibility.Pufferfish,
                FlyingMermaidLevelMermaid.SummonPossibility.Turtle
            };

            __instance.summonPattern = array;
            __instance.summonIndex = Utility.GetUserPattern<FlyingMermaidPhaseOneSummonPatterns>((int) FlyingMermaidPhaseOneSummonPattern.Value);
        }

    }




    protected static bool IsWithinPhase(float phaseStart, float phaseEnd, FlyingMermaidLevel __instance) {
        return (__instance.properties.CurrentState.stateName == LevelProperties.FlyingMermaid.States.Generic &&
        (Level.Current.timeline.health - Level.Current.timeline.damage) / Level.Current.timeline.health < phaseStart &&
        (Level.Current.timeline.health - Level.Current.timeline.damage) / Level.Current.timeline.health > phaseEnd);
    }
}
