using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MonoMod.Cil;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
public class PiratePatternSelector : PluginComponent {

    [HarmonyPatch(typeof(PirateLevel), nameof(PirateLevel.peashot_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void GunManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random"))) {
            ilCursor.Index = ilCursor.Index - 6;
            ilCursor.Remove();
            ilCursor.Remove();
            ilCursor.Remove();
            ilCursor.Remove();
            ilCursor.Remove();
            ilCursor.Remove();
            ilCursor.Remove();
            ilCursor.Emit(OpCodes.Call, typeof(PiratePatternSelector).GetMethod(nameof(SetGunPattern), BindingFlags.Static | BindingFlags.Public));
            break;
        }
    }

    [HarmonyPatch(typeof(PirateLevel), nameof(PirateLevel.OnStateChanged))]
    [HarmonyPrefix]
    public static void SubsequentPhasesPatternManipulator(ref PirateLevel __instance) {
        if (Level.ScoringData.difficulty == Level.Mode.Easy) {
            if (IsWithinPhase(0.68f, 0.52f)) {
                if (PiratePhaseFourPatternEasy.Value != PiratePhaseFourPatternsEasy.Random) {
                    __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<PiratePhaseFourPatternsEasy>((int) PiratePhaseFourPatternEasy.Value);
                }
            }
            if (IsWithinPhase(0.22f, 0.0f)) {
                if (PiratePhaseSevenPatternEasy.Value != PiratePhaseSevenPatternsEasy.Random) {
                    __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<PiratePhaseSevenPatternsEasy>((int) PiratePhaseSevenPatternEasy.Value);
                }
            }
        }

        if (Level.ScoringData.difficulty == Level.Mode.Normal) {
            if (IsWithinPhase(0.87f, 0.51f)) {
                if (PiratePhaseTwoPatternNormalHard.Value != PiratePhaseTwoPatternsNormalHard.Random) {
                    var firstPattern = __instance.properties.CurrentState.patterns[0];
                    if (firstPattern == LevelProperties.Pirate.Pattern.Peashot) {
                        __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<PiratePhaseTwoPatternsNormalHard>(PhaseTwoPatternsNormal1[PiratePhaseTwoPatternNormalHard.Value]);
                    } else {
                        __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<PiratePhaseTwoPatternsNormalHard>(PhaseTwoPatternsNormal2[PiratePhaseTwoPatternNormalHard.Value]);
                    }
                }
            }
            if (IsWithinPhase(0.51f, 0.22f)) {
                if (PiratePhaseThreePatternNormalHard.Value != PiratePhaseThreePatternsNormalHard.Random) {
                    var firstPattern = __instance.properties.CurrentState.patterns[0];
                    if (firstPattern == LevelProperties.Pirate.Pattern.Peashot) {
                        __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<PiratePhaseThreePatternsNormalHard>(PhaseThreePatternsNormal1[PiratePhaseThreePatternNormalHard.Value]);
                    } else {
                        __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<PiratePhaseThreePatternsNormalHard>(PhaseThreePatternsNormal2[PiratePhaseThreePatternNormalHard.Value]);
                    }
                }
            }
        }

        if (Level.ScoringData.difficulty == Level.Mode.Hard) {
            if (IsWithinPhase(0.92f, 0.77f)) {
                if (PiratePhaseTwoPatternNormalHard.Value != PiratePhaseTwoPatternsNormalHard.Random) {
                    var firstPattern = __instance.properties.CurrentState.patterns[0];
                    if (firstPattern == LevelProperties.Pirate.Pattern.Peashot) {
                        __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<PiratePhaseTwoPatternsNormalHard>(PhaseTwoPatternsHard1[PiratePhaseTwoPatternNormalHard.Value]);
                    } else {
                        __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<PiratePhaseTwoPatternsNormalHard>(PhaseTwoPatternsHard2[PiratePhaseTwoPatternNormalHard.Value]);
                    }
                }
            }
            if (IsWithinPhase(0.77f, 0.32f)) {
                if (PiratePhaseThreePatternNormalHard.Value != PiratePhaseThreePatternsNormalHard.Random) {
                    var firstPattern = __instance.properties.CurrentState.patterns[0];
                    if (firstPattern == LevelProperties.Pirate.Pattern.Peashot) {
                        __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<PiratePhaseThreePatternsNormalHard>(PhaseThreePatternsHard1[PiratePhaseThreePatternNormalHard.Value]);
                    } else {
                        __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<PiratePhaseThreePatternsNormalHard>(PhaseThreePatternsHard2[PiratePhaseThreePatternNormalHard.Value]);
                    }
                }
            }
        }
    }

    public static int SetGunPattern() {
        if (Level.ScoringData.difficulty == Level.Mode.Easy) {
            if (IsWithinPhase(0.82f, 0.68f)) {
                if (PiratePhaseThreeGunPatternEasy.Value != PiratePhaseThreeGunPatternsEasy.Random) {
                    return (int) PiratePhaseThreeGunPatternEasy.Value - 1;
                }
            }
            if (IsWithinPhase(0.68f, 0.52f)) {
                if (PiratePhaseFourGunPatternEasy.Value != PiratePhaseFourGunPatternsEasy.Random) {
                    return (int) PiratePhaseFourGunPatternEasy.Value - 1;
                }
            }
            if (IsWithinPhase(0.22f, 0.0f)) {
                if (PiratePhaseSevenGunPatternEasy.Value != PiratePhaseSevenGunPatternsEasy.Random) {
                    return PhaseSevenGunPatternsEasy[PiratePhaseSevenGunPatternEasy.Value];
                }
            }
        }


        if (Level.ScoringData.difficulty == Level.Mode.Normal) {
            if (IsWithinPhase(1.0f, 0.87f)) {
                if (PiratePhaseOneGunPatternNormal.Value != PiratePhaseOneGunPatternsNormal.Random) {
                    return (int) PiratePhaseOneGunPatternNormal.Value - 1;
                }
            }
            if (IsWithinPhase(0.87f, 0.51f)) {
                if (PiratePhaseTwoGunPatternNormal.Value != PiratePhaseTwoGunPatternsNormal.Random) {
                    return (int) PiratePhaseTwoGunPatternNormal.Value - 1;
                }
            }
            if (IsWithinPhase(0.51f, 0.22f)) {
                if (PiratePhaseThreeGunPatternNormal.Value != PiratePhaseThreeGunPatternsNormal.Random) {
                    return (int) PiratePhaseThreeGunPatternNormal.Value - 1;
                }
            }
        }

        if (Level.ScoringData.difficulty == Level.Mode.Hard) {
            if (IsWithinPhase(1.0f, 0.92f)) {
                if (PiratePhaseOneGunPatternHard.Value != PiratePhaseOneGunPatternsHard.Random) {
                    return (int) PiratePhaseOneGunPatternHard.Value - 1;
                }
            }
            if (IsWithinPhase(0.92f, 0.77f)) {
                if (PiratePhaseTwoGunPatternHard.Value != PiratePhaseTwoGunPatternsHard.Random) {
                    return (int) PiratePhaseTwoGunPatternHard.Value - 1;
                }
            }
            if (IsWithinPhase(0.77f, 0.32f)) {
                if (PiratePhaseThreeGunPatternHard.Value != PiratePhaseThreeGunPatternsHard.Random) {
                    return PhaseThreeGunPatternsHard[PiratePhaseThreeGunPatternHard.Value];
                }
            }
        }

        PirateLevel pirateLevelInstance = FindObjectOfType<PirateLevel>();
        return UnityEngine.Random.Range(0, pirateLevelInstance.properties.CurrentState.peashot.patterns.Length);


    }

    protected static bool IsWithinPhase(float phaseStart, float phaseEnd) {
        return
        (Level.Current.timeline.health - Level.Current.timeline.damage) / Level.Current.timeline.health <= phaseStart &&
        (Level.Current.timeline.health - Level.Current.timeline.damage) / Level.Current.timeline.health > phaseEnd;
    }

    public static readonly Dictionary<PiratePhaseTwoPatternsNormalHard, int> PhaseTwoPatternsNormal1 = new Dictionary<PiratePhaseTwoPatternsNormalHard, int> {
        { PiratePhaseTwoPatternsNormalHard.PeashotShark, 1 },
        { PiratePhaseTwoPatternsNormalHard.Shark, 2 },
        { PiratePhaseTwoPatternsNormalHard.PeashotSquid, 3 },
        { PiratePhaseTwoPatternsNormalHard.Squid, 4 },
        { PiratePhaseTwoPatternsNormalHard.PeashotDogfish, 5 },
        { PiratePhaseTwoPatternsNormalHard.Dogfish, 6 }
    };

    public static readonly Dictionary<PiratePhaseTwoPatternsNormalHard, int> PhaseTwoPatternsNormal2 = new Dictionary<PiratePhaseTwoPatternsNormalHard, int> {
        { PiratePhaseTwoPatternsNormalHard.PeashotShark, 2 },
        { PiratePhaseTwoPatternsNormalHard.Shark, 3 },
        { PiratePhaseTwoPatternsNormalHard.PeashotSquid, 6 },
        { PiratePhaseTwoPatternsNormalHard.Squid, 1 },
        { PiratePhaseTwoPatternsNormalHard.PeashotDogfish, 4 },
        { PiratePhaseTwoPatternsNormalHard.Dogfish, 5 }
    };

    public static readonly Dictionary<PiratePhaseTwoPatternsNormalHard, int> PhaseTwoPatternsHard1 = new Dictionary<PiratePhaseTwoPatternsNormalHard, int> {
        { PiratePhaseTwoPatternsNormalHard.PeashotShark, 3 },
        { PiratePhaseTwoPatternsNormalHard.Shark, 4 },
        { PiratePhaseTwoPatternsNormalHard.PeashotSquid, 1 },
        { PiratePhaseTwoPatternsNormalHard.Squid, 2 },
        { PiratePhaseTwoPatternsNormalHard.PeashotDogfish, 5 },
        { PiratePhaseTwoPatternsNormalHard.Dogfish, 6 }
    };

    public static readonly Dictionary<PiratePhaseTwoPatternsNormalHard, int> PhaseTwoPatternsHard2 = new Dictionary<PiratePhaseTwoPatternsNormalHard, int> {
        { PiratePhaseTwoPatternsNormalHard.PeashotShark, 6 },
        { PiratePhaseTwoPatternsNormalHard.Shark, 1 },
        { PiratePhaseTwoPatternsNormalHard.PeashotSquid, 2 },
        { PiratePhaseTwoPatternsNormalHard.Squid, 3 },
        { PiratePhaseTwoPatternsNormalHard.PeashotDogfish, 4 },
        { PiratePhaseTwoPatternsNormalHard.Dogfish, 5 }
    };


    public static readonly Dictionary<PiratePhaseThreePatternsNormalHard, int> PhaseThreePatternsNormal1 = new Dictionary<PiratePhaseThreePatternsNormalHard, int> {
        { PiratePhaseThreePatternsNormalHard.PeashotShark, 1 },
        { PiratePhaseThreePatternsNormalHard.Shark, 2 },
        { PiratePhaseThreePatternsNormalHard.PeashotSquid, 3 },
        { PiratePhaseThreePatternsNormalHard.Squid, 4 },
        { PiratePhaseThreePatternsNormalHard.PeashotDogfish, 5 },
        { PiratePhaseThreePatternsNormalHard.Dogfish, 6 }
    };

    public static readonly Dictionary<PiratePhaseThreePatternsNormalHard, int> PhaseThreePatternsNormal2 = new Dictionary<PiratePhaseThreePatternsNormalHard, int> {
        { PiratePhaseThreePatternsNormalHard.PeashotShark, 4 },
        { PiratePhaseThreePatternsNormalHard.Shark, 5 },
        { PiratePhaseThreePatternsNormalHard.PeashotSquid, 2 },
        { PiratePhaseThreePatternsNormalHard.Squid, 3 },
        { PiratePhaseThreePatternsNormalHard.PeashotDogfish, 6 },
        { PiratePhaseThreePatternsNormalHard.Dogfish, 1 }
    };

    public static readonly Dictionary<PiratePhaseThreePatternsNormalHard, int> PhaseThreePatternsHard1 = new Dictionary<PiratePhaseThreePatternsNormalHard, int> {
        { PiratePhaseThreePatternsNormalHard.PeashotShark, 3 },
        { PiratePhaseThreePatternsNormalHard.Shark, 4 },
        { PiratePhaseThreePatternsNormalHard.PeashotSquid, 1 },
        { PiratePhaseThreePatternsNormalHard.Squid, 2 },
        { PiratePhaseThreePatternsNormalHard.PeashotDogfish, 5 },
        { PiratePhaseThreePatternsNormalHard.Dogfish, 6 }
    };

    public static readonly Dictionary<PiratePhaseThreePatternsNormalHard, int> PhaseThreePatternsHard2 = new Dictionary<PiratePhaseThreePatternsNormalHard, int> {
        { PiratePhaseThreePatternsNormalHard.PeashotShark, 6 },
        { PiratePhaseThreePatternsNormalHard.Shark, 1 },
        { PiratePhaseThreePatternsNormalHard.PeashotSquid, 2 },
        { PiratePhaseThreePatternsNormalHard.Squid, 3 },
        { PiratePhaseThreePatternsNormalHard.PeashotDogfish, 4 },
        { PiratePhaseThreePatternsNormalHard.Dogfish, 5 }
    };


    public static readonly Dictionary<PiratePhaseSevenGunPatternsEasy, int> PhaseSevenGunPatternsEasy = new Dictionary<PiratePhaseSevenGunPatternsEasy, int> {
        { PiratePhaseSevenGunPatternsEasy.Two_One_Two_1, 1 },
        { PiratePhaseSevenGunPatternsEasy.Two_Two_One, 2 },
        { PiratePhaseSevenGunPatternsEasy.Three, 3 },
        { PiratePhaseSevenGunPatternsEasy.One_Two_Two, 4 },
        { PiratePhaseSevenGunPatternsEasy.One_Three, 5 },
        { PiratePhaseSevenGunPatternsEasy.Two_One_Two_2, 7 },
        { PiratePhaseSevenGunPatternsEasy.Four, 8 }
    };

    public static readonly Dictionary<PiratePhaseThreeGunPatternsHard, int> PhaseThreeGunPatternsHard = new Dictionary<PiratePhaseThreeGunPatternsHard, int> {
        { PiratePhaseThreeGunPatternsHard.Two, 1 },
        { PiratePhaseThreeGunPatternsHard.Three, 4 },
        { PiratePhaseThreeGunPatternsHard.Four, 3 }
    };


}
